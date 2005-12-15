using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

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

    public delegate void ReceiveEventHandler(object sender, EventArguments e);
    public delegate void ErrorHandler(object sender, EventArguments e);
    public event ReceiveEventHandler ReceiveEvent;
    public event ErrorHandler ErrorEvent;


    public class EventArguments : EventArgs
    {
      public string message;
      public DateTime timestamp;
      public string Message { get { return message; } }
      public DateTime Timestamp { get { return timestamp; } }

      public EventArguments(string msg)
      {
        message = msg;
        timestamp = DateTime.Now;
      }
    }

    
    protected virtual void OnError(string strError)
    {
      if (ErrorEvent != null)
      {
        EventArguments e = new EventArguments(strError);
        ErrorEvent(this, e);
      }
    }


    protected virtual void OnReceive(string strReceived)
    {
      if (ReceiveEvent != null)
      {
        EventArguments e = new EventArguments(strReceived);
        ReceiveEvent(this, e);
      }
    }


    #endregion
    #region


    private class SocketPacket
    {
      public System.Net.Sockets.Socket Socket;
      public byte[] dataBuffer = new byte[100];
    }


    ~Connection()
    {
      Close();
    }


    private void OnRemoteDisconnected()
    {
      Close();
      Connect(tcpPort);
    }


    private void OnClientConnect(IAsyncResult asyn)
    {
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
        OnError("OnClientConnect: Socket has been closed");
      }
      catch (SocketException se)
      {
        OnError("OnClientConnect: " + se.Message);
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
        OnError("OnDataReceived: Socket has been closed");
      }
      catch (SocketException se)
      {
        if (se.ErrorCode == 10054)
          OnRemoteDisconnected();
        else
          OnError("OnDataReceived: " + se.Message);
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
        OnError("WaitForData: " + se.Message);
      }
    }


    public bool Connect(int port)
    {
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
        OnError("Connection failed: " + se.Message);
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
          clientSocket.Send(Encoding.UTF8.GetBytes(type + "|" + send));
      }
      catch (SocketException se)
      {
        OnError("Send: " + se.Message);
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