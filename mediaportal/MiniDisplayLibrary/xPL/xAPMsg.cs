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

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.xPL
{
  public class xAPMsg
  {
    private const int XAP_BASE_PORT = 0xe37;
    private string xAP_Raw;

    public xAPMsg() {}

    public xAPMsg(string xAPMsg)
    {
      this.xAP_Raw = xAPMsg;
    }

    public void Send()
    {
      IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, 0xe37);
      this.Send(ep);
    }

    public void Send(IPEndPoint ep)
    {
      Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
      socket.SendTo(Encoding.ASCII.GetBytes(this.xAP_Raw), ep);
    }

    public void Send(string s)
    {
      this.xAP_Raw = s;
      IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, 0xe37);
      this.Send(ep);
    }

    public string Content
    {
      get { return this.xAP_Raw; }
    }
  }
}