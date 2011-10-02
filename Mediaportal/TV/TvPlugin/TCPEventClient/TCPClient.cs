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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Constants;
using Mediaportal.TV.Server.TVControl.Enums;
using Mediaportal.TV.Server.TVControl.TCP;
using Mediaportal.TV.Server.Plugins.Base.Events;

#endregion

namespace Mediaportal.TV.TvPlugin.TCPEventClient
{
  public class TCPClient
  {

    #region private vars

    private PacketProtocol _packetProtocol;
    private readonly TcpClient _client = new TcpClient();
    private Thread _isConnectedThread;
    private Thread _heartBeatTransmitterThread;
    private Thread _handleServerCommThread;
    private const int HEARTBEAT_INTERVAL_SEC = 5;
    private const int IS_SOCKET_CONNECTED_THREAD_POLLING_SEC = 30;
    private readonly static ManualResetEvent _evtHeartbeatCtrl = new ManualResetEvent(false);
    private readonly static ManualResetEvent _evtTCPCtrl = new ManualResetEvent(false);
    private readonly static ManualResetEvent _evtIsConnectedCtrl = new ManualResetEvent(false);
    private readonly static ManualResetEvent _evtIsConnectedWaitCtrl = new ManualResetEvent(false);
    private bool _isConnected;
    private bool _isRunning;

    #endregion

    #region events & delegates

    public delegate void TimeShiftingForcefullyStoppedDelegate(string username, TvStoppedReason tvStoppedReason);
    public delegate void ChannelStatesDelegate(Dictionary<int, ChannelState> channelStates);    
    public delegate void RecordingStartedDelegate(int idRecording);
    public delegate void RecordingEndedDelegate(int idRecording);
    public delegate void RecordingFailedDelegate(int idSchedule);

    public event TimeShiftingForcefullyStoppedDelegate TimeShiftingForcefullyStopped;
    public event ChannelStatesDelegate ChannelStates;
    public event RecordingStartedDelegate RecordingStarted;
    public event RecordingEndedDelegate RecordingEnded;
    public event RecordingFailedDelegate RecordingFailed;

    public delegate void TCPDisconnectedDelegate();
    public delegate void TCPConnectedDelegate();

    public event TCPDisconnectedDelegate OnTCPDisconnected;
    public event TCPConnectedDelegate OnTCPConnected;

    #endregion

    #region ctors

    public TCPClient ()
    {
      
    }

    #endregion

    #region public methods

    public void Start()
    {
      System.Diagnostics.Debugger.Launch();      
      StartIsConnectedThread();        
      try
      {
        ConnectToTCPserver();
        _packetProtocol = new PacketProtocol(10000);
        _packetProtocol.MessageArrived -= new PacketProtocol.MessageArrivedDelegate(_packetProtocol_MessageArrived);
        _packetProtocol.MessageArrived += new PacketProtocol.MessageArrivedDelegate(_packetProtocol_MessageArrived);
        _isRunning = true;
      } 
      finally
      {
        //always start these regardless
        StartListenForServerThread();
        StartHeartBeatThread(); 
      }      
    }    

    public void Stop()
    {      
      _packetProtocol.MessageArrived -= new PacketProtocol.MessageArrivedDelegate(_packetProtocol_MessageArrived);
      _isRunning = false;

      StopIsConnectedThread();  
      StopHeartBeatThread();
      DisconnectTCPserver();
      StopListenForServerThread();
    }


    #endregion

    #region private methods

    private void _packetProtocol_MessageArrived(byte[] msg)
    {
      if (_isConnected && msg.Length > 0)
      {
        try
        {
          TvServerEventArgs tvServerEventArgs = TvServerEventArgsTranslator.FromBinary(msg);
          Log.Debug("TCPClient.packetProtocol_MessageArrived: TvServerEventArgs: {0} - (length:{1})", tvServerEventArgs.EventType, msg.Length);
          HandleTvServerEvent(tvServerEventArgs);
        }
        catch (Exception ex)
        {
          Log.Error("TCPClient.HandleServerComm - error handling TV Server Event {0}", ex);
        }
      }
    }    

    private void StartIsConnectedThread()
    {
      if (_isConnectedThread == null || !_isConnectedThread.IsAlive)
      {        
        _evtIsConnectedCtrl.Reset();
        _evtIsConnectedWaitCtrl.Set();

        Log.Debug("TCPClient: IsConnected thread started.");
        _isConnectedThread = new Thread(IsSocketConnectedThread);
        _isConnectedThread.IsBackground = true;
        _isConnectedThread.Name = "TCPClient: IsSocketConnected Thread";
        _isConnectedThread.Start();
      }
    }

    private void StopIsConnectedThread()
    {
      if (_isConnectedThread != null && _isConnectedThread.IsAlive)
      {
        try
        {
          _evtIsConnectedWaitCtrl.Set();
          _evtIsConnectedCtrl.Set();
          _isConnectedThread.Join();
          Log.Debug("TCPClient: IsSocketConnected Thread stopped.");
        }
        catch (Exception) { }
      }
    }

    private void IsSocketConnectedThread()
    {
      //detect any sudden disconnections      
      bool wasConnected = _isConnected;
      while (!_evtIsConnectedCtrl.WaitOne(1))
      {
        bool resetTimer = _evtIsConnectedWaitCtrl.WaitOne(IS_SOCKET_CONNECTED_THREAD_POLLING_SEC * 1000);
        if (_isRunning)
        {
          try
          {
            if (_isConnected && !wasConnected)
            {
              //reconnected
              if (OnTCPConnected != null)
              {
                OnTCPConnected();
              }
              wasConnected = _isConnected;
            }
            else if (!_isConnected && wasConnected)
            {
              //disconnected
              if (OnTCPDisconnected != null)
              {
                OnTCPDisconnected();
              }
              DisconnectTCPserver();
              ConnectToTCPserver();

              if (_client.Connected)
              {
                wasConnected = _isConnected;
              }
            }
            else if (!_client.Client.Connected)
            {
              ConnectToTCPserver();
              _isConnected = IsSocketConnected();
            }
            else
            {
              _isConnected = IsSocketConnected();
            }
          }
          finally
          {
            if (resetTimer)
            {
              _evtIsConnectedWaitCtrl.Reset();
            }
          }
        }
      }
    }

    private bool IsSocketConnected()
    {      
      bool isConnected = false;
      /*try
      {
        if (_client.Client.Connected)
        {
          if ((_client.Client.Poll(0, SelectMode.SelectWrite)) && (!_client.Client.Poll(0, SelectMode.SelectError)))
          {            
            byte[] buffer = new byte[1];
            isConnected = (_client.Client.Receive(buffer, SocketFlags.Peek) == 0);                        
          }          
        }        
      }
      catch (SocketException)
      {        
        //ignore
      }*/     
      if (_client.Client.Connected)
      {
        bool blockingState = _client.Client.Blocking;
        try
        {
          var tmp = new byte[1];
          _client.Client.Blocking = false;
          _client.Client.Send(tmp, 0, 0);
          isConnected = true;
        }
        catch (SocketException e)
        {
          //ignore
        }
        finally
        {
          _client.Client.Blocking = blockingState;
        }
      }

      return isConnected;
    }

    private IPAddress GetActiveIPAddress()
    {
      // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection) 
      var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

      foreach (var network in networkInterfaces)
      {
        // Read the IP configuration for each network 
        var properties = network.GetIPProperties();

        // Only consider those with valid gateways
        var gateways = properties.GatewayAddresses.Select(x => x.Address).Where(
            x => !x.Equals(IPAddress.Any) && !x.Equals(IPAddress.None) && !x.Equals(IPAddress.Loopback) &&
            !x.Equals(IPAddress.IPv6Any) && !x.Equals(IPAddress.IPv6None) && !x.Equals(IPAddress.IPv6Loopback));
        if (gateways.Count() < 1)
          continue;

        // Each network interface may have multiple IP addresses 
        foreach (var address in properties.UnicastAddresses)
        {
          // Comment these next two lines to show IPv6 addresses too
          if (address.Address.AddressFamily != AddressFamily.InterNetwork)
            continue;

          return address.Address;
        }
      }

      return IPAddress.None;
    }
    
    private void ConnectToTCPserver()
    {      
      bool isSingleSeat = Network.IsSingleSeat();
      try
      {
        if (isSingleSeat)
        {
          IPEndPoint serverEndPoint = new IPEndPoint(GetActiveIPAddress(), TCPconsts.ServerPort);
          _client.Connect(serverEndPoint);
        }
        else
        {
          IPAddress[] addresslist = Dns.GetHostAddresses(RemoteControl.HostName);
          if (addresslist.Length > 0)
          {
            foreach (IPAddress ip in addresslist)
            {
              if (ip.AddressFamily == AddressFamily.InterNetwork)
              {
                IPEndPoint serverEndPoint = new IPEndPoint(ip, TCPconsts.ServerPort);
                _client.Connect(serverEndPoint);
                break;
              }

            }
          }
        }        

        if (!_client.Connected)
        {
          Log.Debug("TCPClient: could not resolve Ip adress: {0}", RemoteControl.HostName);
        }
        else
        {
          Log.Debug("TCPClient: connected to server : {0}", RemoteControl.HostName);
          SignalIsSocketConnectedThread();
          if (OnTCPConnected != null)
          {
            OnTCPConnected();            
          }          
        }
        
      }
      catch (Exception e)
      {
        Log.Error("TCPClient: could not connect to TCP server {0} - {1}", RemoteControl.HostName, e.Message);        
      }
    }

    private static void SignalIsSocketConnectedThread()
    {
      _evtIsConnectedWaitCtrl.Set();
    }

    private void DisconnectTCPserver()
    {
      if (_client != null && _client.Connected)
      {        
        _client.GetStream().Close();
        _client.Close();
        Log.Debug("TCPClient: disconnected from server : {0}", RemoteControl.HostName);
      }
    }

    private void StartListenForServerThread()
    {
      if (_handleServerCommThread == null || !_handleServerCommThread.IsAlive)
      {
        _evtTCPCtrl.Reset();
        Log.Debug("TCPClient: HandleServerComm thread started.");
        _handleServerCommThread = new Thread(HandleServerComm);
        _handleServerCommThread.IsBackground = true;
        _handleServerCommThread.Name = "TCPClient: HandleServerComm thread";
        _handleServerCommThread.Start();
      }
    }
    
    private void HandleServerComm()
    {
      NetworkStream clientStream = null;
      var message = new byte[] {};
      while (!_evtTCPCtrl.WaitOne(1))
      {
        if (_client != null)
        {
          if (clientStream == null && _client.Connected)
          {
            clientStream = _client.GetStream();
            message = new byte[_client.ReceiveBufferSize];
          }
          if (clientStream != null)
          {            
            if (_client.Connected)
            {
              SignalIsSocketConnectedThread();
              int bytesRead = 0;
              try
              {
                if (!_isConnected)
                {
                  clientStream = _client.GetStream();
                }
                //blocks until a client sends a message
                Array.Clear(message, 0, message.Length); //clear previous buffer contents
                bytesRead = clientStream.Read(message, 0, message.Length);
                _isConnected = true;
              }
              catch
              {
                //a socket error has occured
                _isConnected = false;
              }

              if (bytesRead == 0)
              {
                //the client has disconnected from the server          
                _isConnected = false;
              }

              if (_isConnected)
              {
                try
                {
                  _packetProtocol.DataReceived(message);
                }
                catch (Exception ex)
                {
                  Log.Error("TCPClient.HandleServerComm - error during TCP packet parsing {0}", ex);
                }
              }
            }
          }
        }
      }      
    }

    private void HandleTvServerEvent(TvServerEventArgs tvServerEventArgs)
    {
      if (tvServerEventArgs != null)
      {        
        switch (tvServerEventArgs.EventType)
        {
          case TvServerEventType.ForcefullyStoppedTimeShifting:
            if (TimeShiftingForcefullyStopped != null)
            {
              TimeShiftingForcefullyStopped(tvServerEventArgs.User.Name, tvServerEventArgs.User.TvStoppedReason);
            }
            break;

          case TvServerEventType.ChannelStatesChanged:
            if (ChannelStates != null)
            {
              ChannelStates(tvServerEventArgs.User.ChannelStates);
            }
            break;

          case TvServerEventType.RecordingFailed:            
            if (RecordingFailed != null)
            {                
              int idSchedule = tvServerEventArgs.Schedule.IdSchedule;
              RecordingFailed(idSchedule);                
            }
            break;

          case TvServerEventType.RecordingStarted:
            if (RecordingStarted != null)
            {
              int idRecording = tvServerEventArgs.Recording.IdRecording;
              RecordingStarted(idRecording);                
            }
            break;

          case TvServerEventType.RecordingEnded:            
            if (RecordingEnded != null)
            {                
              int idRecording = tvServerEventArgs.Recording.IdRecording;
              RecordingEnded(idRecording);                
            }
            break;
        }                         
      }
    }

    private void StartHeartBeatThread()
    {
      if (_heartBeatTransmitterThread ==null || !_heartBeatTransmitterThread.IsAlive)
      {        
        _evtHeartbeatCtrl.Reset();
        Log.Debug("TCPClient: HeartBeat Transmitter started.");
        _heartBeatTransmitterThread = new Thread(HeartBeatTransmitterThread);
        _heartBeatTransmitterThread.IsBackground = true;
        _heartBeatTransmitterThread.Name = "TvClient-TvHome: HeartBeat transmitter thread";
        _heartBeatTransmitterThread.Start();
      }
    }

    private void HeartBeatTransmitterThread()
    {      
      while (!_evtHeartbeatCtrl.WaitOne(HEARTBEAT_INTERVAL_SEC * 1000))      
      {
        if (_isRunning)
        {
          // send heartbeat to tv server each 5 sec.
          // this way we signal to the server that we are alive thus avoid being kicked.
          // Log.Debug("TVHome: sending HeartBeat signal to server.");
          // when debugging we want to disable heartbeats
          try
          {
            SignalIsSocketConnectedThread();
            if (_isConnected)
            {
              SendHeartBeat();
            }
            else
            {
              Log.Error("TCPClient: failed sending HeartBeat signal to server. currently not connected to server");
            }
          }
          catch (Exception e)
          {
            Log.Error("TCPClient: failed sending HeartBeat signal to server. ({0})", e.Message);
          }
        }
      }                 
    }

   private void SendHeartBeat()
    {
      var encoder = new ASCIIEncoding();
      byte[] data = encoder.GetBytes("heartbeat");
      NetworkStream clientStream = _client.GetStream();
      clientStream.Write(data, 0, data.Length);
      clientStream.Flush();      
    }    

    private void StopListenForServerThread()
    {
      if (_handleServerCommThread != null && _handleServerCommThread.IsAlive)
      {
        try
        {
          _evtTCPCtrl.Set();
          _handleServerCommThread.Join();
          Log.Debug("TCPClient: HandleServerComm thread stopped.");
        }
        catch (Exception) { }
      }      
    }

    private void StopHeartBeatThread()
    {
      if (_heartBeatTransmitterThread != null && _heartBeatTransmitterThread.IsAlive)
      {
        try
        {
          _evtHeartbeatCtrl.Set();
          _heartBeatTransmitterThread.Join();
          Log.Debug("TCPClient: HeartBeat Transmitter stopped.");
        }
        catch (Exception) { }
      }
    }

    #endregion
  }
}
