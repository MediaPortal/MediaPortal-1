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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using MediaPortal.GUI.Library;

namespace UdpHelper
{
  public class Connection
  {
    private bool logVerbose;
    private UdpClient udpClient;

    private Socket socket;
    private IPAddress hostIP = IPAddress.Parse("127.0.0.1");

    public delegate void ReceiveEventHandler(string strReceive);

    public event ReceiveEventHandler ReceiveEvent;


    protected virtual void OnReceive(string strReceive)
    {
      if (ReceiveEvent != null)
      {
        ReceiveEvent(strReceive);
      }
    }


    private class UdpState
    {
      public IPEndPoint EndPoint;
      public UdpClient UdpClient;
    }

    public Connection(bool log)
    {
      logVerbose = log;
    }


    public void Stop()
    {
      try
      {
        udpClient.Close();
      }
      catch (System.NullReferenceException) {}
      udpClient = null;
      socket = null;
    }


    public bool Send(int udpPort, string strType, string strSend, DateTime timeStamp)
    {
      if (socket == null)
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      try
      {
        byte[] sendbuf = Encoding.UTF8.GetBytes(string.Format("{0}|{1}|{2}~", strType, strSend, timeStamp.ToBinary()));
        IPEndPoint endPoint = new IPEndPoint(hostIP, udpPort);
        socket.SendTo(sendbuf, endPoint);
        return true;
      }
      catch (SocketException se)
      {
        Log.Info("UDPHelper: Send port {0}: {1} - {2}", udpPort, se.ErrorCode, se.Message);
        return false;
      }
    }


    public bool Start(int udpPort)
    {
      if (socket == null)
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      try
      {
        if (logVerbose) Log.Info("UDPHelper: Starting listener on port {0}", udpPort);

        // Port already used?
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
        foreach (TcpConnectionInformation c in connections)
          if (c.RemoteEndPoint.Port == udpPort)
          {
            Log.Info("UDPHelper: UDP port {0} is already in use", udpPort);
            return false;
          }
        IPAddress hostIP = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new IPEndPoint(hostIP, udpPort);
        udpClient = new UdpClient(endPoint);
        UdpState state = new UdpState();
        state.EndPoint = endPoint;
        state.UdpClient = udpClient;
        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), state);
        if (logVerbose) Log.Info("UDPHelper: Listening for messages on port {0}", udpPort);
        return true;
      }
      catch (SocketException se)
      {
        Log.Info("UDPHelper: Start port {0}: {1} - {2}", udpPort, se.ErrorCode, se.Message);
        return false;
      }
    }


    public void ReceiveCallback(IAsyncResult ar)
    {
      UdpClient udpClientLoc = (UdpClient)((UdpState)(ar.AsyncState)).UdpClient;
      IPEndPoint endPoint = (IPEndPoint)((UdpState)(ar.AsyncState)).EndPoint;

      try
      {
        Byte[] bytesReceived = udpClientLoc.EndReceive(ar, ref endPoint);
        string strReceived = Encoding.UTF8.GetString(bytesReceived);
        OnReceive(strReceived);
        udpClientLoc.BeginReceive(new AsyncCallback(ReceiveCallback), (UdpState)(ar.AsyncState));
      }
      catch (System.ObjectDisposedException) {}
      catch (SocketException) {}
    }
  }
}