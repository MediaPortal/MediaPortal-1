using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Net.NetworkInformation;

//using RtpStreamer;

namespace Mediaportal.TV.Server.TVLibrary.SatIp.Rtsp
{
  public class RtspServer
  {
    private TcpListener tcpListener;
    private Thread listenThread;
    private string serverIp;
    private bool listen = true;
    private int streams = 0;
    private Object StartStreamLock = new Object();

    private Dictionary<int, RtspClients> clients = new Dictionary<int, RtspClients>();
    private Dictionary<int, rtpStream> rtpStreams = new Dictionary<int, rtpStream>();

    //private static readonly Regex REGEX_STATUS_LINE = new Regex(@"(?<request>\w+)\s([^.]+?)\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", RegexOptions.Singleline);

    /// <summary>
    /// Enumeration to describe the available Rtsp Methods, used in responses
    /// </summary>
    public enum RtspRequestMethod
    {
      UNKNOWN,
      ANNOUNCE,
      DESCRIBE,
      REDIRECT,
      OPTIONS,
      SETUP,
      GET_PARAMETER,
      SET_PARAMETER,
      PLAY,
      PAUSE,
      RECORD,
      TEARDOWN
    }

    #region constructor/destructur

    public RtspServer()
    {
      serverIp = LocalIPAddress();
      
      tcpListener = new TcpListener(IPAddress.Any, 554);
      listenThread = new Thread(new ThreadStart(ListenForClients));
      listenThread.Start();

      /*rtpStreams.Add(streams, new rtpStream(StartStreamThread(streams, "192.168.178.26", 8888, "test.ts")));
      streams++;
      rtpStreams.Add(streams, new rtpStream(StartStreamThread(streams, "192.168.178.26", 9999, "test2.ts")));
      streams++;*/
    }

    ~RtspServer()
    {
      stop();
    }

    #endregion

    public void stop()
    {
      listen = false;
      tcpListener.Stop();

      // stop all running streams
      foreach (KeyValuePair<int, rtpStream> streamThread in rtpStreams)
      {
        rtpStreams[streamThread.Key].stopStream = true;
      }
    }

    private void ListenForClients()
    {
      tcpListener.Start();

      while (listen)
      {
        try
        {
          // blocks until a client has connected to the server
          TcpClient client = tcpListener.AcceptTcpClient();

          // create a thread to handle communication 
          // with connected client
          Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
          clientThread.Start(client);
        }catch(Exception ex)
        {
          System.Diagnostics.Debug.WriteLine(ex.Message);
        }
      }
    }

    private void HandleClientComm(object client)
    {
      TcpClient tcpClient = (TcpClient)client;
      NetworkStream clientStream = tcpClient.GetStream();

      byte[] message = new byte[4096];
      int bytesRead;

      while (true)
      {
        bytesRead = 0;

        try
        {
          //blocks until a client sends a message
          bytesRead = clientStream.Read(message, 0, 4096);
        }
        catch
        {
          //a socket error has occured
          break;
        }

        if (bytesRead == 0)
        {
          //the client has disconnected from the server
          break;
        }

        //message has successfully been received => process it
        ASCIIEncoding encoder = new ASCIIEncoding();
        //System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
        processMessage(encoder.GetString(message, 0, bytesRead), clientStream);
      }

      tcpClient.Close();
    }

    private void processMessage(string message, NetworkStream clientStream)
    {
      System.Diagnostics.Debug.WriteLine("REQUEST:");
      System.Diagnostics.Debug.WriteLine("-----------");
      System.Diagnostics.Debug.WriteLine(message);
      System.Diagnostics.Debug.WriteLine("-----------");
      
      RtspRequestHeader requestHeader = new RtspRequestHeader(message);
      RtspRequestMethod _method;
      Uri uri;
      NameValueCollection query;
      byte[] buffer;
      StringBuilder rtspDescribeRequest = new StringBuilder();

      // Method: SETUP, Play...
      //C->S[0]SETUP[1]rtsp://example.com/media.mp4/streamid=0[2]RTSP/1.0
      string[] parts = message.Split(' ');

      if (parts.Length < 2 || !Enum.TryParse<RtspRequestMethod>(parts[0], true, out _method))
      {
        System.Diagnostics.Debug.WriteLine("Message Invalid");
        return;
      }


      switch (_method)
      {
        case RtspRequestMethod.SETUP:
          #region setup
          System.Diagnostics.Debug.WriteLine("Case Setup");

          RtspClients client = new RtspClients();
          client.rtpClientPort = requestHeader.rtpPort;
          client.rtcpClientPort = requestHeader.rtcpPort;

          // setting up the server ports
          int rtpPort = findFreePort();
          client.rtpServerPort = rtpPort;
          client.rtcpServerPort = rtpPort+1;

          // client IP
          var pi = clientStream.GetType().GetProperty("Socket", BindingFlags.NonPublic | BindingFlags.Instance);
          string socketIp = ((Socket)pi.GetValue(clientStream, null)).RemoteEndPoint.ToString();
          client.ip = socketIp.Split(':')[0];

          System.Diagnostics.Debug.WriteLine("Client IP: " + client.ip);
          System.Diagnostics.Debug.WriteLine("Client Count: " + client.clientId.ToString() + " SessionID: " + client.sessionId.ToString());
          System.Diagnostics.Debug.WriteLine("rtpPort: " + requestHeader.rtpPort + " rtcpPort: " + requestHeader.rtcpPort);

          uri = new Uri(parts[1]);
          query = HttpUtility.ParseQueryString(uri.Query);
              
          //?src=1&fe=1&freq=12402&pol=v&msys=dvbs&sr=27500&fec=34&pids=0,16
          foreach (string key in query.Keys)
          {
            int value;
            
            switch (key.ToLowerInvariant()) {
              case "src":
                if (int.TryParse(query.Get(key), out value)) client.src = value;
                break;
              case "freq":
                double freq;
                if (double.TryParse(query.Get(key), out freq)) client.freq = freq;
                break;
              case "sr":
                if (int.TryParse(query.Get(key), out value)) client.sr = value;
                break;
              case "fec":
                if (int.TryParse(query.Get(key), out value)) client.fec = value;
                break;
              case "msys":
                client.msys = query.Get(key);
                break;
              case "pol":
                client.pol = query.Get(key);
                break;
              case "pids":
                foreach (string pid in query.Get(key).Split(','))
                {
                  if (int.TryParse(pid, out value)) client.addPid(value);
                }
                break;
            }
          }

          clients.Add(client.sessionId, client);


          // Response:
          // RTSP/1.0 200 OK
          // Session:379007aecd6c6;timeout=30
          // com.ses.streamID:3
          // Transport:RTP/AVP;unicast;destination=192.168.128.100;client_port=4222-4223
          // CSeq:1

          rtspDescribeRequest.Append("RTSP/1.0 200 OK\r\n");
          // sessionID
          rtspDescribeRequest.AppendFormat("Session: {0};timeout=60\r\n", client.sessionId);
          // streamID
          rtspDescribeRequest.AppendFormat("com.ses.streamID: {0}\r\n", client.clientId);
          // Transport
          // TODO add destination, remove source
          rtspDescribeRequest.AppendFormat("Transport: RTP/AVP/UDP;unicast;client_port={0}-{1};source={2};server_port={3}-{4}\r\n", requestHeader.rtpPort, requestHeader.rtcpPort, serverIp, rtpPort, rtpPort+1);
          //CSeq
          rtspDescribeRequest.AppendFormat("CSeq: {0}", requestHeader.CSeq);
          rtspDescribeRequest.Append("\r\n\r\n");

          buffer = Encoding.UTF8.GetBytes(rtspDescribeRequest.ToString());
          clientStream.Write(buffer, 0, buffer.Length);
          clientStream.Flush();

          
          System.Diagnostics.Debug.WriteLine("ANSWER:");
          System.Diagnostics.Debug.WriteLine("-----------");
          System.Diagnostics.Debug.WriteLine(rtspDescribeRequest.ToString());
          System.Diagnostics.Debug.WriteLine("-----------");
          #endregion
          break;
        case RtspRequestMethod.PLAY:
          #region play
          // Request
          // PLAY rtsp://192.168.128.192:554/stream=3 RTSP/1.0
          // CSeq:2
          // Session:379007aecd6c6
          // Connection:close

          // Response
          // RTSP/1.0 200 OK
          // RTP-Info:url=rtsp://192.168.128.192/stream=3;seq=29427
          // CSeq:2
          // Session:379007aecd6c6
          
          System.Diagnostics.Debug.WriteLine("Case Play");

          uri = new Uri(parts[1]);
          query = HttpUtility.ParseQueryString(uri.Query);
          //int streamID = int.Parse(query.Get("stream"));

          if (query.Get("addpids") != null)
          {
            System.Diagnostics.Debug.WriteLine("Add pids!");
            int value;
            foreach (string pid in query.Get("addpids").Split(','))
            {
              if (int.TryParse(pid, out value)) clients[int.Parse(requestHeader.sessionId)].addPid(value);
            }
          }

          if (query.Get("delpids") != null)
          {
            System.Diagnostics.Debug.WriteLine("Delete pids!");
            int value;
            foreach (string pid in query.Get("delpids").Split(','))
            {
              if (int.TryParse(pid, out value)) clients[int.Parse(requestHeader.sessionId)].delPid(value);
            }
          }

          // starting the stream
          //if (clients[int.Parse(requestHeader.sessionId)].pids.Count != 1 && (int)clients[int.Parse(requestHeader.sessionId)].pids[0] != 0)
          //{
          if (query.Get("addpids") == "18") // for debuging only...
          {
            // TODO: lock?!
            System.Diagnostics.Debug.WriteLine("Start Stream!");
            System.Diagnostics.Debug.WriteLine("PLAY CLIENT PORT: " + clients[int.Parse(requestHeader.sessionId)].rtpClientPort.ToString());
            rtpStreams.Add(streams, new rtpStream(StartStreamThread(streams, clients[int.Parse(requestHeader.sessionId)].ip, clients[int.Parse(requestHeader.sessionId)].rtpClientPort, "test3.ts")));
            clients[int.Parse(requestHeader.sessionId)].streamId = streams;
            streams++;
          }

          // creating the response

          rtspDescribeRequest.Append("RTSP/1.0 200 OK\r\n");
          // RTP-Info
          // RTP-Info: url=rtsp://192.168.178.44:554/?freq=530.000&msys=dvbc&sr=6900&mtype=256qam&pids=0,259,533,538,534,18,17,16 RTSP/1.0\r\n
          rtspDescribeRequest.AppendFormat("RTP-Info: url={0} RTSP/1.0\r\n", parts[1]);
          // session
          rtspDescribeRequest.AppendFormat("Session: {0}\r\n", clients[int.Parse(requestHeader.sessionId)].sessionId);
          //CSeq
          rtspDescribeRequest.AppendFormat("CSeq: {0}", requestHeader.CSeq);
          rtspDescribeRequest.Append("\r\n\r\n");

          buffer = Encoding.UTF8.GetBytes(rtspDescribeRequest.ToString());
          clientStream.Write(buffer, 0, buffer.Length);
          clientStream.Flush();


          System.Diagnostics.Debug.WriteLine("ANSWER:");
          System.Diagnostics.Debug.WriteLine("-----------");
          System.Diagnostics.Debug.WriteLine(rtspDescribeRequest.ToString());
          System.Diagnostics.Debug.WriteLine("-----------");

          //rtpStreams.Add(streams, StartStream(clients[int.Parse(requestHeader.sessionId)].ip, clients[int.Parse(requestHeader.sessionId)].rtpClientPort, "test.ts"));
          //streams++;
          //rtpStreams[streams].Start(clients[int.Parse(requestHeader.sessionId)].ip, clients[int.Parse(requestHeader.sessionId)].rtpClientPort, "test.ts");
          //RtpSetup(clients[int.Parse(requestHeader.sessionId)].ip, clients[int.Parse(requestHeader.sessionId)].rtpClientPort, "test.ts");
          #endregion
          break;
        case RtspRequestMethod.DESCRIBE:
          #region describe
          System.Diagnostics.Debug.WriteLine("Case Describe");

          // session description protocol
          StringBuilder rtspSDP = new StringBuilder();
          rtspSDP.Append("v=0\r\n");
          rtspSDP.AppendFormat("o=- 9876543210 1 IN IP4 {0}\r\n", serverIp);  // use any sessionID
          rtspSDP.Append("s=MPEG TS\r\n");
          rtspSDP.Append("t=0 0\r\n");
          rtspSDP.Append("m=video 0 RTP/AVP 33\r\n");
          rtspSDP.Append("c=IN IP4 0.0.0.0\r\n");
          rtspSDP.AppendFormat("a=control:{0}\r\n", parts[1]);

          // Response
          rtspDescribeRequest.Append("RTSP/1.0 200 OK\r\n");
          //CSeq
          rtspDescribeRequest.AppendFormat("CSeq: {0}\r\n", requestHeader.CSeq);
          // Public
          rtspDescribeRequest.Append("Content-Type: application/sdp\r\n");
          rtspDescribeRequest.AppendFormat("Content-length: {0}", Encoding.UTF8.GetBytes(rtspSDP.ToString()).Length);
          rtspDescribeRequest.Append("\r\n\r\n");
          rtspDescribeRequest.Append(rtspSDP);
          

          buffer = Encoding.UTF8.GetBytes(rtspDescribeRequest.ToString());
          clientStream.Write(buffer, 0, buffer.Length);
          clientStream.Flush();

          System.Diagnostics.Debug.WriteLine("ANSWER:");
          System.Diagnostics.Debug.WriteLine("-----------");
          System.Diagnostics.Debug.WriteLine(rtspDescribeRequest.ToString());
          System.Diagnostics.Debug.WriteLine("-----------");
          #endregion
          break;
        case RtspRequestMethod.OPTIONS:
          #region options
          System.Diagnostics.Debug.WriteLine("Case options");

          rtspDescribeRequest.Append("RTSP/1.0 200 OK\r\n");
          //CSeq
          rtspDescribeRequest.AppendFormat("CSeq: {0}\r\n", requestHeader.CSeq);
          // Public
          rtspDescribeRequest.AppendFormat("Public: OPTIONS, DESCRIBE, SETUP, PLAY, TEARDOWN");
          rtspDescribeRequest.Append("\r\n\r\n");

          buffer = Encoding.UTF8.GetBytes(rtspDescribeRequest.ToString());
          clientStream.Write(buffer, 0, buffer.Length);
          clientStream.Flush();

          System.Diagnostics.Debug.WriteLine("ANSWER:");
          System.Diagnostics.Debug.WriteLine("-----------");
          System.Diagnostics.Debug.WriteLine(rtspDescribeRequest.ToString());
          System.Diagnostics.Debug.WriteLine("-----------");
          #endregion
          break;
        case RtspRequestMethod.TEARDOWN:
          #region teardown
          // stop the stream
          rtpStreams[clients[int.Parse(requestHeader.sessionId)].streamId].stopStream = true;

          // creating the response

          rtspDescribeRequest.Append("RTSP/1.0 200 OK\r\n");
          //CSeq
          rtspDescribeRequest.AppendFormat("CSeq: {0}", requestHeader.CSeq);
          rtspDescribeRequest.Append("\r\n\r\n");

          buffer = Encoding.UTF8.GetBytes(rtspDescribeRequest.ToString());
          clientStream.Write(buffer, 0, buffer.Length);
          clientStream.Flush();

          System.Diagnostics.Debug.WriteLine("ANSWER:");
          System.Diagnostics.Debug.WriteLine("-----------");
          System.Diagnostics.Debug.WriteLine(rtspDescribeRequest.ToString());
          System.Diagnostics.Debug.WriteLine("-----------");
          #endregion
          break;
        default:
          #region bad request
          System.Diagnostics.Debug.WriteLine("case: Bad Request");

          // creating the response

          rtspDescribeRequest.Append("RTSP/1.0 400 Bad Request\r\n");
          //CSeq
          rtspDescribeRequest.AppendFormat("CSeq: {0}", requestHeader.CSeq);
          rtspDescribeRequest.Append("\r\n\r\n");

          buffer = Encoding.UTF8.GetBytes(rtspDescribeRequest.ToString());
          clientStream.Write(buffer, 0, buffer.Length);
          clientStream.Flush();


          System.Diagnostics.Debug.WriteLine("ANSWER:");
          System.Diagnostics.Debug.WriteLine("-----------");
          System.Diagnostics.Debug.WriteLine(rtspDescribeRequest.ToString());
          System.Diagnostics.Debug.WriteLine("-----------");
          #endregion
          break;
      }
    }

    #region helper functions

    private int findFreePort()
    {
      // Find a free port for sending the RTP stream.
      int rtpServerPort = 0;
      TcpConnectionInformation[] activeTcpConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
      HashSet<int> usedPorts = new HashSet<int>();
      foreach (TcpConnectionInformation connection in activeTcpConnections)
      {
        usedPorts.Add(connection.LocalEndPoint.Port);
      }
      for (int port = 40000; port <= 65534; port += 2)
      {
        // We need two adjacent ports. One for RTP; one for RTCP. By
        // convention, the RTP port is even.
        if (!usedPorts.Contains(port) && !usedPorts.Contains(port + 1))
        {
          rtpServerPort = port;
          break;
        }
      }
      return rtpServerPort;
    }

    private string LocalIPAddress()
    {
      IPHostEntry host;
      string localIP = "";
      host = Dns.GetHostEntry(Dns.GetHostName());
      foreach (IPAddress ip in host.AddressList)
      {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
          localIP = ip.ToString();
          break;
        }
      }
      return localIP;
    }

    #endregion

    #region threads

    private Thread StartStreamThread(int id, string ipAdress, int port, string filename)
    {
      var t = new Thread(() => StartStream(id, ipAdress, port, filename));
      t.Start(); 
      return t;
    }

    private void StartStream(int id, string ipAdress, int port, string filename)
    {
      // TODO: Here we will start the timeshifting anf than call the named pipe of the filter
        
        /*RtpStreamer.RtpStreamer rtpStream = new RtpStreamer.RtpStreamer();

      try
      {
        var t = new Thread(() => rtpStream.RtpStreamCreate(ipAdress, port, filename));
        t.Start();

        while (!rtpStreams[id].stopStream)  // keep the thread running
        {
          System.Threading.Thread.Sleep(500);
        }
        rtpStream.RtpStreamStop();  // End our EventLoop on the C++ side => ends the t thread started above
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine("Exception in Thread! "+ ex.Message);
      }

      removeStream(id);
      System.Diagnostics.Debug.WriteLine("!!End Streaming Thread!!");*/
    }

    private void removeStream(int id)
    {
      rtpStreams.Remove(id);
    }
    #endregion
  }
}
