#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using MediaPortal.GUI.Library;

namespace NetHelper
{
  public class Connection
  {
    #region Locals

    private Socket serverSocket;
    private Socket clientSocket;
    private AsyncCallback callBackMethod;
    private int tcpPort;
    private bool isOnline;
    private bool logVerbose = false;

    #endregion
    #region Labels

    public bool IsServer { get { return (serverSocket != null); } }
    public bool IsConnected
    {
      get
      {
        if (clientSocket != null)
          return (clientSocket.Connected);
        else return false;
      }
    }

    public bool IsOnline { get { return (isOnline); } }

    #endregion
    #region Events

    public delegate void ReceiveEventHandler(EventArguments e);
    public delegate void DisconnectHandler();
    public event ReceiveEventHandler ReceiveEvent;
    public event DisconnectHandler DisconnectEvent;


    public class EventArguments
    {
      private string message;
      public string Message { get { return message; } }

      public EventArguments(string msg)
      {
        message = msg;
      }
    }

    
    protected virtual void OnReceive(string strReceived)
    {
      if (ReceiveEvent != null)
      {
        ReceiveEvent(new EventArguments(strReceived));
      }
    }

    #endregion
    #region

    private class SocketPacket
    {
      public System.Net.Sockets.Socket Socket;
      public byte[] dataBuffer = new byte[65535];
    }


    ~Connection()
    {
      if (logVerbose) MediaPortal.GUI.Library.Log.Write("NetHelper: shutting down connection");
      Close();
    }


    public Connection(bool log)
    {
      logVerbose = log;
    }


    private void OnRemoteDisconnected()
    {
      if (logVerbose) MediaPortal.GUI.Library.Log.Write("NetHelper: peer disconnected");
      if (DisconnectEvent != null)
      {
        DisconnectEvent();
      }
      Close();
      Thread.Sleep(200);
      Connect(tcpPort);
    }


    private void OnClientConnect(IAsyncResult asyn)
    {
      if (logVerbose) MediaPortal.GUI.Library.Log.Write("NetHelper: client connected");
      try
      {
        if (serverSocket != null)
        {
          clientSocket = serverSocket.EndAccept(asyn);
          WaitForData(clientSocket);
          serverSocket.Close();
        }
      }
      catch (ObjectDisposedException)
      {
        if (logVerbose) MediaPortal.GUI.Library.Log.Write("NetHelper: OnClientConnect: socket has been closed");
      }
      catch (SocketException se)
      {
        MediaPortal.GUI.Library.Log.Write("NetHelper: OnClientConnect: {0}", se.Message);
      }
    }


    private void OnDataReceived(IAsyncResult asyn)
    {
      try
      {
        SocketPacket socketData = (SocketPacket)asyn.AsyncState;
        int iRx = socketData.Socket.EndReceive(asyn);
        OnReceive(Encoding.UTF8.GetString(socketData.dataBuffer, 0, iRx));
        WaitForData(socketData.Socket);
      }
      catch (ObjectDisposedException)
      {
        if (logVerbose) MediaPortal.GUI.Library.Log.Write("NetHelper: OnDataReceived: socket has been closed");
      }
      catch (SocketException se)
      {
        if (se.ErrorCode == 10054)
          OnRemoteDisconnected();
        else
          MediaPortal.GUI.Library.Log.Write("NetHelper: OnDataReceived: {0}", se.Message);
      }
    }


    private void WaitForData(System.Net.Sockets.Socket socket)
    {
      try
      {
        if (callBackMethod == null)
          callBackMethod = new AsyncCallback(OnDataReceived);
        SocketPacket socketPacket = new SocketPacket();
        socketPacket.Socket = socket;
        socket.BeginReceive(socketPacket.dataBuffer, 0, socketPacket.dataBuffer.Length, SocketFlags.None, callBackMethod, socketPacket);
      }
      catch (SocketException se)
      {
        if (se.ErrorCode == 10054)
          OnRemoteDisconnected();
        else
          MediaPortal.GUI.Library.Log.Write("NetHelper: WaitForData: {0}", se.Message);
      }
    }


    public bool Connect(int port)
    {
      if (logVerbose) MediaPortal.GUI.Library.Log.Write("NetHelper: connecting");
      IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
      TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();

      foreach (TcpConnectionInformation c in connections)
        if (c.RemoteEndPoint.Port == port)
        {
          isOnline = false;
          return false;
        }
      tcpPort = port;
      IPAddress hostIP = (Dns.GetHostEntry("localhost")).AddressList[0];
      IPEndPoint ep = new IPEndPoint(hostIP, tcpPort);
      try
      {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect(ep);
        if (clientSocket.Connected)
          WaitForData(clientSocket);
        isOnline = true;
        return true;
      }
      catch (SocketException se)
      {
        switch (se.ErrorCode)
        {
          case 10054:
            OnRemoteDisconnected();
            break;
          case 10061:
            break;
          default:
            MediaPortal.GUI.Library.Log.Write("NetHelper: Connect: {1} - {0}", se.Message, se.ErrorCode);
            break;
        }
      }
      clientSocket.Close();
      clientSocket = null;
      try
      {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(ep);
        serverSocket.Listen(4);
        serverSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
        isOnline = true;
        return true;
      }
      catch (SocketException se)
      {
        MediaPortal.GUI.Library.Log.Write("NetHelper: Connection failed: {0}", se.Message);
        Close();
        isOnline = false;
        return false;
      }
    }


    public void Send(string type, string send)
    {
      try
      {
        if (clientSocket != null)
          clientSocket.Send(Encoding.UTF8.GetBytes(string.Format("{0}|{1}|{2}~", type, send, DateTime.Now.ToBinary())));
      }
      catch (SocketException se)
      {
        MediaPortal.GUI.Library.Log.Write("NetHelper: Send: {0}", se.Message);
      }
    }


    private void Close()
    {
      if (clientSocket != null)
      {
        clientSocket.Close();
        clientSocket = null;
      }
      if (serverSocket != null)
      {
        serverSocket.Close();
        serverSocket = null;
      }
    }

    #endregion
  }
}