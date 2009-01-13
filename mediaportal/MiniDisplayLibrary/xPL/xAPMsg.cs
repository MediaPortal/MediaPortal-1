using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.xPL
{
  public class xAPMsg
  {
    private const int XAP_BASE_PORT = 0xe37;
    private string xAP_Raw;

    public xAPMsg()
    {
    }

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