#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region usings

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using Mediaportal.TV.Server.Plugins.Base.Events;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Constants;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.TCP;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

#endregion

namespace Mediaportal.TV.Server.TVService.TCPEventServer
{
	public class TCPServer
  {
    #region private vars 

    private readonly TVController _tvController;        
    private Thread _listenThread;
	  private readonly IList<Thread> _clientThreads = new List<Thread>();

    private readonly List<TCPClientData> _clients = new List<TCPClientData>();
    private readonly IList<TvServerEventArgs> _notificationsQueue = new List<TvServerEventArgs>();
    private readonly TcpListener _tcpListener;
    private readonly object _notificationLock = new object();
    private readonly object _clientsLock = new object();
    private const int HEARTBEAT_MAX_SECS_EXCEED_ALLOWED = 30;
    private Thread _heartBeatMonitorThread;
    private Thread _clientsNotificationThread;    

    private static readonly ManualResetEvent _evtTCPCtrl = new ManualResetEvent(false);
    private static readonly ManualResetEvent _evtNotifications = new ManualResetEvent(true);
    private static readonly ManualResetEvent _evtHeartbeatCtrl = new ManualResetEvent(false);
    private static readonly ManualResetEvent _evtNotificationsCtrl = new ManualResetEvent(false);

    #endregion

    #region events & delegates

    public delegate void HeartbeatUserLostDelegate(string username);
    public event HeartbeatUserLostDelegate HeartbeatUserLost;

    #endregion

    #region ctors

    public TCPServer(TVController tvController)
    {
      _tvController = tvController;      
      _tcpListener = new TcpListener(IPAddress.Any, TCPconsts.ServerPort);      
    }

    #endregion

    #region public methods

    

    public void Start()
    {
      _tvController.OnTvServerEvent -= new TvServerEventHandler(_tvController_OnTvServerEvent);
      _tvController.OnTvServerEvent += new TvServerEventHandler(_tvController_OnTvServerEvent);

      _evtTCPCtrl.Reset();
      _listenThread = new Thread(new ThreadStart(ListenForClients));
      _listenThread.Start();

      SetupClientsNotificationsThread();
      SetupHeartbeatThread();
    }

    public void Stop()
    {
      _tvController.OnTvServerEvent -= new TvServerEventHandler(_tvController_OnTvServerEvent);

      StopHeartbeatThread();
      StopClientsNotificationsThread();

      _evtTCPCtrl.Set();

      _notificationsQueue.Clear();
    }

    #endregion

    #region private methods

    private void SendCommand(TvServerEventArgs tvServerEventArgs)
    {
      byte[] data = TvServerEventArgsTranslator.ToBinary(tvServerEventArgs);

      lock (_clientsLock)
      {
        foreach (TCPClientData tcpClientData in _clients)
        {
          if (tcpClientData.Client.Connected)
          {
            try
            {
              if (tvServerEventArgs.EventType == TvServerEventType.ChannelStatesChanged && tvServerEventArgs.User != null)
              {                
                data = PrepareChannelStateEvent(tvServerEventArgs, tcpClientData);
              }

              byte[] dataWrapped = PacketProtocol.WrapMessage(data);
              Log.Debug("TCPServer.SendCommand : sending cmd to : {0} - {1} - (length: {2}) - {3}", tcpClientData.Name, tvServerEventArgs.EventType, data.Length, BitConverter.ToString(data));

              NetworkStream clientStream = tcpClientData.Client.GetStream();                            
              clientStream.Write(dataWrapped, 0, dataWrapped.Length);
              clientStream.Flush();
            }
            catch (Exception e)
            {
              Log.Error("TCPServer.SendCommand : failed to send command to client : {0} - {1}", tcpClientData.Name, e);
            }
          }
          else
          {
            Log.Debug("TCPServer.SendCommand : could not send cmd to : {0} - disconnected", tcpClientData.Name);
          }
        }
      }
    }

	  private byte[] PrepareChannelStateEvent(TvServerEventArgs tvServerEventArgs, TCPClientData tcpClientData)
	  {
	    byte[] data;
	    IUser user = new User(tcpClientData.Name, false);
	    bool isTimeShifting = _tvController.IsTimeShifting(ref user);
	    if (isTimeShifting)
	    {
	      tvServerEventArgs.User.ChannelStates = user.ChannelStates;
	    }
	    else
	    {
	      tvServerEventArgs.User.ChannelStates = _tvController.GetAllChannelStatesForIdleUserCached();
	    }
	    data = TvServerEventArgsTranslator.ToBinary(tvServerEventArgs);
	    return data;
	  }

	  private void HeartBeatMonitorThread()
    {
      Log.Info("TCPServer: Heartbeat Monitor initiated, max timeout allowed is {0} sec.",
               HEARTBEAT_MAX_SECS_EXCEED_ALLOWED);
#if !DEBUG

      if (HeartbeatUserLost != null)
      {
        List<TCPClientData> _removeClients = new List<TCPClientData>();

        while (!_evtHeartbeatCtrl.WaitOne(HEARTBEAT_MAX_SECS_EXCEED_ALLOWED * 1000))
        {
          lock (_clientsLock)
          {
            foreach (TCPClientData client in _clients)
            {
              DateTime now = DateTime.Now;
              TimeSpan ts = client.LastSeen - now;

              // more than 30 seconds have elapsed since last heartbeat was received. lets kick the client
              if (ts.TotalSeconds < (-1 * HEARTBEAT_MAX_SECS_EXCEED_ALLOWED))
              {
                Log.Write("TCPServer: Heartbeat Monitor - idle user found: {0}", client.Name);
                HeartbeatUserLost(client.Name);
                _removeClients.Add(client);
              }
            }
            foreach (TCPClientData removeClient in _removeClients)
            {              
              removeClient.Client.GetStream().Close();
              removeClient.Client.Close();
              _clients.Remove(removeClient);
            }
          }
          _removeClients.Clear();
        }
      }
      Log.Info("TCPServer: HeartBeat monitor stopped...");
#endif
    }

    private void SetupHeartbeatThread()
    {
      // setup heartbeat monitoring thread.
      // useful for kicking idle/dead clients.
      Log.Info("TCPServer: setup HeartBeat Monitor");
      StopHeartbeatThread();

      _evtHeartbeatCtrl.Reset();
      _heartBeatMonitorThread = new Thread(HeartBeatMonitorThread);
      _heartBeatMonitorThread.Name = "HeartBeatMonitorThread";
      _heartBeatMonitorThread.IsBackground = true;
      _heartBeatMonitorThread.Start();
    }

    private void StopHeartbeatThread()
    {
      if (_heartBeatMonitorThread != null && _heartBeatMonitorThread.IsAlive)
      {
        try
        {
          _evtHeartbeatCtrl.Set();
          _heartBeatMonitorThread.Join();
        }
        catch (Exception) { }
      }
    }

    private void SetupClientsNotificationsThread()
    {
      Log.Info("TCPServer: setup ClientsNotifications thread");
      StopClientsNotificationsThread();

      _evtNotificationsCtrl.Reset();
      _clientsNotificationThread = new Thread(ClientsNotificationsQueueThread);
      _clientsNotificationThread.Name = "ClientsNotificationsThread";
      _clientsNotificationThread.IsBackground = true;
      _clientsNotificationThread.Start();
    }

    private void StopClientsNotificationsThread()
    {
      if (_clientsNotificationThread != null && _clientsNotificationThread.IsAlive)
      {
        try
        {
          _evtNotifications.Set();
          _evtNotificationsCtrl.Set();
          _clientsNotificationThread.Join();
        }
        catch (Exception) { }
      }
    }    

    private void _tvController_OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      TvServerEventArgs tvEvent = eventArgs as TvServerEventArgs;
      
      if (tvEvent != null)
      {
        AddServerEvent(tvEvent);        
      }
    }

    private void AddServerEvent(TvServerEventArgs tvEvent)
    {
      lock (_notificationLock)
      {
        _notificationsQueue.Add(tvEvent);
        _evtNotifications.Set();
      }
    }

    
    private void ClientsNotificationsQueueThread()
    {
      while (!_evtNotificationsCtrl.WaitOne(1))
      {
        lock (_notificationLock)
        {
          foreach (TvServerEventArgs tvEvent in _notificationsQueue)
          {
            SendCommand(tvEvent);           
          }
          _notificationsQueue.Clear();
        }

        bool isQueueEmpty;
        lock (_notificationLock)
        {
          isQueueEmpty = (_notificationsQueue.Count == 0);
        }

        if (isQueueEmpty)
        {
          Log.Debug("TCPServer.ClientsNotificationsQueueThread - queue empty, suspending thread.");
          _evtNotifications.Reset();
        }

        _evtNotifications.WaitOne();
        Thread.Sleep(1);
      }
      Log.Info("TCPServer: ClientsNotifications thread stopped...");
    }

    private void ListenForClients()
    {      
      _tcpListener.Start();

      while (!_evtTCPCtrl.WaitOne(1))
      {
        //blocks until a client has connected to the server
        TcpClient client = _tcpListener.AcceptTcpClient();
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
        AddClient(client);
        //create a thread to handle communication
        //with connected client
        Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
        _clientThreads.Add(clientThread);
        clientThread.Start(client);
      }

      foreach (Thread clientThread in _clientThreads)
      {
        clientThread.Join();
      }
      _clientThreads.Clear();

      lock (_clientsLock)
      {
        foreach (TCPClientData clientData in _clients)
        {
          clientData.Client.GetStream().Close();
          clientData.Client.Close();
        }
        _clients.Clear();
      }
      
      _tcpListener.Stop();

      Log.Info("TCPServer: ListenForClients stopped...");
    }

	

	  private void HandleClientComm(object client)
	  {
	    TcpClient tcpClient = (TcpClient)client;
	    NetworkStream clientStream = tcpClient.GetStream();        
      byte[] message = new byte[4096];
      int bytesRead;

      IPEndPoint endPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
      IPAddress ipAddress = endPoint.Address;
      IPHostEntry hostEntry = Dns.GetHostEntry(ipAddress);
      string clientHostName = hostEntry.HostName;        

      while (!_evtTCPCtrl.WaitOne(1))
      {
        bytesRead = 0;
        try
        {
          //blocks until a client sends a message
          bytesRead = clientStream.Read(message, 0, 4);
        }
        catch
        {
          // a socket error has occured
          // ignore
        }

        if (bytesRead == 0)
        {
          //the client has disconnected from the server
          RemoveClient(tcpClient);
          break;
        }

        TCPClientData existingClient;
        lock (_clientsLock)
        {
          existingClient = _clients.Find(t => t.Name.Equals(clientHostName));
          if (existingClient != null)
          {
            Log.Debug("TCPServer.HandleClientComm: updating heartbeat for user: {0}", clientHostName);
            existingClient.LastSeen = DateTime.Now;
          }
        }        
      }
      Log.Info("TCPServer: HandleClientComm stopped for host: {0}", clientHostName);      
    }

    private void RemoveClient(TcpClient client) 
    {
	    string name = GetClientName(client);

      lock (_clientsLock)
      {
        if (_clients.Count > 0)
        {
          TCPClientData tcpClientData = _clients.First(t => t.Name.Equals(name));
          if (tcpClientData != null)
          {
            _clients.Remove(tcpClientData);
          }
        }
      }
    }

    private void AddClient(TcpClient client)
    {
      
      string name = GetClientName(client);
      lock (_clientsLock)
      {
        TCPClientData existingTcpClientData = _clients.Find(t => t.Name.Equals(name));
        if (existingTcpClientData == null)
        {
          TCPClientData tcpClientData = new TCPClientData(client);
          _clients.Add(tcpClientData);
        }
        else
        {
          existingTcpClientData.Client = client;
        }
      }
    }

    private string GetClientName(TcpClient client)
    {
      IPAddress ipAddress = ((IPEndPoint)(client.Client.RemoteEndPoint)).Address;
      IPHostEntry ipHostEntry = Dns.GetHostEntry(ipAddress);
      return ipHostEntry.HostName;
    }

    #endregion
  }
}
