#region Copyright (C) 2005 Team MediaPortal

/* 
 *	Copyright (C) 2005 Team MediaPortal
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
    public delegate void LogHandler(string strLog);
    public delegate void DisconnectHandler();
    public event ReceiveEventHandler ReceiveEvent;
    public event LogHandler LogEvent;
    public event DisconnectHandler DisconnectEvent;


    public class EventArguments
    {
      private string message;
      private DateTime timestamp;

      public string Message { get { return message; } }
      public DateTime Timestamp { get { return timestamp; } }

      public EventArguments(string msg)
      {
        message = msg;
        timestamp = DateTime.Now;
      }
    }

    
    protected virtual void OnLog(string strLog)
    {
      if (LogEvent != null)
      {
        LogEvent(strLog);
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
      OnLog("Shutdown connection");
      Close();
    }


    private void OnRemoteDisconnected()
    {
      OnLog("Remote disconnected");
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
      OnLog("Remote connected.");
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
        OnLog("OnClientConnect: Socket has been closed");
      }
      catch (SocketException se)
      {
        OnLog("OnClientConnect: " + se.Message);
      }
    }


    private void OnDataReceived(IAsyncResult asyn)
    {
      OnLog("Data received");
      try
      {
        SocketPacket socketData = (SocketPacket)asyn.AsyncState;
        int iRx = socketData.Socket.EndReceive(asyn);
        OnReceive(Encoding.UTF8.GetString(socketData.dataBuffer, 0, iRx));
        WaitForData(socketData.Socket);
      }
      catch (ObjectDisposedException)
      {
        OnLog("OnDataReceived: Socket has been closed");
      }
      catch (SocketException se)
      {
        if (se.ErrorCode == 10054)
          OnRemoteDisconnected();
        else
          OnLog("OnDataReceived: " + se.Message);
      }
    }


    private void WaitForData(System.Net.Sockets.Socket socket)
    {
      OnLog("Wait for data");
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
          OnLog("WaitForData: " + se.Message);
      }
    }


    public bool Connect(int port)
    {
      OnLog("Connect");
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
      catch (SocketException)
      {

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
        OnLog("Connection failed: " + se.Message);
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
          clientSocket.Send(Encoding.UTF8.GetBytes(string.Format("{0}|{1}~", type, send)));
      }
      catch (SocketException se)
      {
        OnLog("Send: " + se.Message);
      }
    }

    private void Close()
    {
      OnLog("Close");
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