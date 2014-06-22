#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Globalization;
using System.IO;
using System.IO.Pipes;

using Mediaportal.TV.Server.TVControl;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;

using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;

//using RtpStreamer;

namespace Mediaportal.TV.Server.TVLibrary.SatIp.Rtsp
{
  public class RtspServer
  {
    private TcpListener tcpListener;
    private Thread listenThread;
    private string serverIp;
    private bool listen = true;
    private Object StartStreamLock = new Object();

    private Dictionary<string, RtspClients> clients = new Dictionary<string, RtspClients>();
    private Dictionary<int, RtspCards> cards = new Dictionary<int, RtspCards>();



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
      this.LogInfo("SAT>IP: Start RTSP-Server");
      serverIp = LocalIPAddress();
      
      tcpListener = new TcpListener(IPAddress.Any, 554);
      listenThread = new Thread(new ThreadStart(ListenForClients));
      listenThread.Start();
      this.LogInfo("SAT>IP: RTSP-Server started");
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

      // stop all running cards
      foreach (KeyValuePair<int, RtspCards> card in cards)
      {
        card.Value.card.StopTimeShifting();
      }

      // TODO: remove all clients
    }

    private void ListenForClients()
    {
      this.LogInfo("SAT>IP: start TCP listener");
      tcpListener.Start();
      this.LogInfo("SAT>IP: TCP listener started");
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
          this.LogError(ex.Message);
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
        //this.LogDebug(encoder.GetString(message, 0, bytesRead));
        processMessage(encoder.GetString(message, 0, bytesRead), clientStream);
      }

      tcpClient.Close();
    }

    private void processMessage(string message, NetworkStream clientStream)
    {
      this.LogDebug("REQUEST:");
      this.LogDebug("-----------");
      this.LogDebug(message);
      this.LogDebug("-----------");
      
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
        this.LogDebug("Message Invalid");
        return;
      }


      switch (_method)
      {
        case RtspRequestMethod.SETUP:
          #region setup
          this.LogDebug("Case Setup");

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

          this.LogDebug("Client IP: " + client.ip);
          this.LogDebug("Client Count: " + client.clientId.ToString() + " SessionID: " + client.sessionId.ToString());
          this.LogDebug("rtpPort: " + requestHeader.rtpPort + " rtcpPort: " + requestHeader.rtcpPort);

          uri = new Uri(parts[1]);
          query = HttpUtility.ParseQueryString(uri.Query);
              
          //?src=1&fe=1&freq=12402&pol=v&msys=dvbs&sr=27500&fec=34&pids=0,16
          /*foreach (string key in query.Keys)
          {
            int value;
            
            switch (key.ToLowerInvariant()) {
              case "src":
                if (int.TryParse(query.Get(key), out value)) client.src = value;
                break;
              case "freq":
                int freq;
                if (int.TryParse(query.Get(key), out freq)) client.freq = freq;
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
          }*/

          clients.Add(client.sessionId, client);

          parseQuery(client.sessionId, query);

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

          
          this.LogDebug("ANSWER:");
          this.LogDebug("-----------");
          this.LogDebug(rtspDescribeRequest.ToString());
          this.LogDebug("-----------");
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
          
          this.LogDebug("Case Play");

          uri = new Uri(parts[1]);
          query = HttpUtility.ParseQueryString(uri.Query);
          //int streamID = int.Parse(query.Get("stream"));

          if (query.Get("delpids") != null)
          {
            this.LogDebug("SAT>IP: Delete pids on sessionID: {0}", requestHeader.sessionId);
            int value;
            foreach (string pid in query.Get("delpids").Split(','))
            {
              if (int.TryParse(pid, out value)) clients[requestHeader.sessionId].delPid(value);
            }
          }

          if (query.Get("addpids") != null)
          {
            this.LogDebug("SAT>IP: Add pids on sessionID: {0}", requestHeader.sessionId);
            int value;
            foreach (string pid in query.Get("addpids").Split(','))
            {
              if (int.TryParse(pid, out value)) clients[requestHeader.sessionId].addPid(value);
            }
          }

          // sync Pids with filter
          if (clients[requestHeader.sessionId].isTunedToFrequency)
          {
            this.LogDebug("SAT>IP: sync Pids with Filter for sessionID: {0}", requestHeader.sessionId);
            syncPidsWithFilter(requestHeader.sessionId, cards[clients[requestHeader.sessionId].cardId].devicePath);
          }

          parseQuery(requestHeader.sessionId, query);
          

          if (clients[requestHeader.sessionId].tunedToFrequency != clients[requestHeader.sessionId].freq)
          {
            this.LogDebug("SAT>IP: tuned Freq [{0}] != requested Freq [{1}] - sessionID {2}",clients[requestHeader.sessionId].tunedToFrequency, clients[requestHeader.sessionId].freq, requestHeader.sessionId);
            
            
            int cardKey;
            if (cardAlreadyTunedToFreq(clients[requestHeader.sessionId].msys, clients[requestHeader.sessionId].freq, out cardKey))
            {
              // there is already a card tuned to the frequency so lets add us to this card.
              this.LogDebug("SAT>IP: there is already a card tuned to the Freq={0} - sessionID: {1}", clients[requestHeader.sessionId].freq, requestHeader.sessionId);
              cards[cardKey].AddSlave(int.Parse(requestHeader.sessionId));
              clients[requestHeader.sessionId].cardId = cardKey;
              clients[requestHeader.sessionId].slot = cards[cardKey].getSlot(int.Parse(requestHeader.sessionId));
            }
            else
            {
              this.LogDebug("SAT>IP: there is no card already tuned to the Freq={0} - sessionID: {1}", clients[requestHeader.sessionId].freq, requestHeader.sessionId);

              // remove user from current card
              if (clients[requestHeader.sessionId].isTunedToFrequency)
              {
                this.LogDebug("SAT>IP: remove user from card with id {0} - sessionID {1}", clients[requestHeader.sessionId].cardId, requestHeader.sessionId);
                cards[clients[requestHeader.sessionId].cardId].removeUser(int.Parse(requestHeader.sessionId));
                if (cards[clients[requestHeader.sessionId].cardId].ownerId == -1)
                {
                  this.LogDebug("SAT>IP: no users on card => stop timeshifting on card with id {0} - sessionID {1}", clients[requestHeader.sessionId].cardId, requestHeader.sessionId);
                  cards[clients[requestHeader.sessionId].cardId].card.StopTimeShifting();
                  // delete card from card array
                  cards.Remove(clients[requestHeader.sessionId].cardId);
                }
              }


              // TODO: Add proper error handling if no tuning Detail is found
              TuningDetail _tuningDetail = ChannelManagement.GetTuningDetail(getChannelTypeAsInt(clients[requestHeader.sessionId].msys), (clients[requestHeader.sessionId].freq * 1000));

              if (_tuningDetail == null)
                Log.Debug("SAT>IP: no such channel found!");

              this.LogInfo("SAT>IP: creating User: \"SAT>IP - {0}\" for sessionId: {1}", clients[requestHeader.sessionId].ip, requestHeader.sessionId);
              IUser _user = UserFactory.CreateBasicUser("SAT>IP - " + clients[requestHeader.sessionId].ip);
              IVirtualCard _card;
              this.LogInfo("SAT>IP: Tuning to freq={0} for sessionId: {1}", clients[requestHeader.sessionId].freq, requestHeader.sessionId);

              TvResult result = GlobalServiceProvider.Get<IControllerService>().StartTimeShifting(_user.Name, _tuningDetail.IdChannel, out _card, out _user);

              if (result != TvResult.Succeeded)
              {
                this.LogError("SAT>IP: Tuning failed" + result);
              }
              else
              {
                this.LogInfo("SAT>IP: tunging success");
              }

              RtspCards card = new RtspCards();
              card.ownerId = int.Parse(requestHeader.sessionId);
              card.user = _user;
              card.card = _card;
              card.tuningDetail = _tuningDetail;
              card.freq = clients[requestHeader.sessionId].freq;
              card.msys = clients[requestHeader.sessionId].msys;
              cards.Add(card.id, card);
              clients[requestHeader.sessionId].cardId = card.id;
              clients[requestHeader.sessionId].slot = card.getSlot(int.Parse(requestHeader.sessionId));

              // TODO remove
              clients[requestHeader.sessionId].card = _card;
              clients[requestHeader.sessionId].user = _user;
              clients[requestHeader.sessionId].tuningDetail = _tuningDetail;

              cards[clients[requestHeader.sessionId].cardId].devicePath = GlobalServiceProvider.Get<IControllerService>().CardDevice(_card.Id); // device path
            }

            clients[requestHeader.sessionId].isTunedToFrequency = true;
            clients[requestHeader.sessionId].tunedToFrequency = clients[requestHeader.sessionId].freq;

            // send commands to the filter
            FilterCommunication communication = new FilterCommunication(cards[clients[requestHeader.sessionId].cardId].devicePath, clients[requestHeader.sessionId].slot);
            communication.addClientPort(clients[requestHeader.sessionId].rtpClientPort);
            communication.addClientIp(clients[requestHeader.sessionId].ip);
            communication.requestNewSlot();
            communication.send();
          }


          


          // creating the response

          rtspDescribeRequest.Append("RTSP/1.0 200 OK\r\n");
          // RTP-Info
          // RTP-Info: url=rtsp://192.168.178.44:554/?freq=530.000&msys=dvbc&sr=6900&mtype=256qam&pids=0,259,533,538,534,18,17,16 RTSP/1.0\r\n
          rtspDescribeRequest.AppendFormat("RTP-Info: url={0} RTSP/1.0\r\n", parts[1]);
          // session
          rtspDescribeRequest.AppendFormat("Session: {0}\r\n", clients[requestHeader.sessionId].sessionId);
          //CSeq
          rtspDescribeRequest.AppendFormat("CSeq: {0}", requestHeader.CSeq);
          rtspDescribeRequest.Append("\r\n\r\n");

          buffer = Encoding.UTF8.GetBytes(rtspDescribeRequest.ToString());
          clientStream.Write(buffer, 0, buffer.Length);
          clientStream.Flush();


          this.LogDebug("ANSWER:");
          this.LogDebug("-----------");
          this.LogDebug(rtspDescribeRequest.ToString());
          this.LogDebug("-----------");

          #endregion
          break;
        case RtspRequestMethod.DESCRIBE:
          #region describe
          this.LogDebug("Case Describe");

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

          this.LogDebug("ANSWER:");
          this.LogDebug("-----------");
          this.LogDebug(rtspDescribeRequest.ToString());
          this.LogDebug("-----------");
          #endregion
          break;
        case RtspRequestMethod.OPTIONS:
          #region options
          this.LogDebug("Case options");

          rtspDescribeRequest.Append("RTSP/1.0 200 OK\r\n");
          //CSeq
          rtspDescribeRequest.AppendFormat("CSeq: {0}\r\n", requestHeader.CSeq);
          // Public
          rtspDescribeRequest.AppendFormat("Public: OPTIONS, DESCRIBE, SETUP, PLAY, TEARDOWN");
          rtspDescribeRequest.Append("\r\n\r\n");

          buffer = Encoding.UTF8.GetBytes(rtspDescribeRequest.ToString());
          clientStream.Write(buffer, 0, buffer.Length);
          clientStream.Flush();

          this.LogDebug("ANSWER:");
          this.LogDebug("-----------");
          this.LogDebug(rtspDescribeRequest.ToString());
          this.LogDebug("-----------");
          #endregion
          break;
        case RtspRequestMethod.TEARDOWN:
          #region teardown
          // stop timeshifting
          // TODO what happens if more than one user is on this card? What happens if we are not the owner?
          int cardsKey;
          if (isSessionIdCardOwner(int.Parse(requestHeader.sessionId), out cardsKey))
          {
            if (cards[cardsKey].streams == 1)
            {
              cards[cardsKey].card.StopTimeShifting();
              cards.Remove(cardsKey);
            }
            else
            {
              // TODO remove owner and set a new oner
            }
          }
          else
          {
            // TODO remove from slave
          }

          // remove client
          clients.Remove(requestHeader.sessionId);

          // creating the response

          rtspDescribeRequest.Append("RTSP/1.0 200 OK\r\n");
          //CSeq
          rtspDescribeRequest.AppendFormat("CSeq: {0}", requestHeader.CSeq);
          rtspDescribeRequest.Append("\r\n\r\n");

          buffer = Encoding.UTF8.GetBytes(rtspDescribeRequest.ToString());
          clientStream.Write(buffer, 0, buffer.Length);
          clientStream.Flush();

          this.LogDebug("ANSWER:");
          this.LogDebug("-----------");
          this.LogDebug(rtspDescribeRequest.ToString());
          this.LogDebug("-----------");
          #endregion
          break;
        default:
          #region bad request
          this.LogDebug("case: Bad Request");

          // creating the response

          rtspDescribeRequest.Append("RTSP/1.0 400 Bad Request\r\n");
          //CSeq
          rtspDescribeRequest.AppendFormat("CSeq: {0}", requestHeader.CSeq);
          rtspDescribeRequest.Append("\r\n\r\n");

          buffer = Encoding.UTF8.GetBytes(rtspDescribeRequest.ToString());
          clientStream.Write(buffer, 0, buffer.Length);
          clientStream.Flush();


          this.LogDebug("ANSWER:");
          this.LogDebug("-----------");
          this.LogDebug(rtspDescribeRequest.ToString());
          this.LogDebug("-----------");
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

    private int getChannelTypeAsInt(string channelType)
    {
      int _channelType;

      if (channelType.ToUpper() == "DVBT" || channelType == "DVBT2")
      {
        _channelType = 4;
      }
      else if (channelType.ToUpper() == "DVBS" || channelType == "DVBS2")
      {
        _channelType = 3;
      }
      else if (channelType.ToUpper() == "DVBC" || channelType == "DVBC2")
      {
        _channelType = 2;
      }
      /*else if (channel is DVBIPChannel)
      {
        channelType = 7;
      }*/
      else // must be ATSCChannel
      {
        _channelType = 1;
      }
      return _channelType;
    }

    private bool cardAlreadyTunedToFreq(string msys, int freq, out int cardKey)
    {
      foreach (KeyValuePair<int, RtspCards> card in cards) {
        if (card.Value.freq == freq && card.Value.msys == msys && card.Value.getNumberOfFreeSlots() > 0) {
          cardKey = card.Key;
          return true;
        }
      }

      cardKey = -1;
      return false;
    }

    private bool isSessionIdCardOwner(int sessionId, out int cardKey)
    {
      foreach (KeyValuePair<int, RtspCards> card in cards)
      {
        if (card.Value.ownerId == sessionId)
        {
          cardKey = card.Key;
          return true;
        }
      }

      cardKey = -1;
      return false;
    }

    private void parseQuery(string sessionId, NameValueCollection query)
    {
      foreach (string key in query.Keys)
      {
        int value;

        switch (key.ToLowerInvariant())
        {
          case "src":
            if (int.TryParse(query.Get(key), out value)) clients[sessionId].src = value;
            break;
          case "freq":
            int freq;
            if (int.TryParse(query.Get(key), out freq)) clients[sessionId].freq = freq;
            break;
          case "sr":
            if (int.TryParse(query.Get(key), out value)) clients[sessionId].sr = value;
            break;
          case "fec":
            if (int.TryParse(query.Get(key), out value)) clients[sessionId].fec = value;
            break;
          case "msys":
            clients[sessionId].msys = query.Get(key);
            break;
          case "pol":
            clients[sessionId].pol = query.Get(key);
            break;
          case "pids":
            foreach (string pid in query.Get(key).Split(','))
            {
              if (int.TryParse(pid, out value)) clients[sessionId].addPid(value);
            }
            break;
        }
      }
    }

    private void syncPidsWithFilter(string sessionId, string namedPipeName)
    {
      FilterCommunication communication = new FilterCommunication(namedPipeName, clients[sessionId].slot);
      communication.addSyncPids(clients[sessionId].pids);
      communication.send();
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
        this.LogDebug("Exception in Thread! "+ ex.Message);
      }

      removeStream(id);
      this.LogDebug("!!End Streaming Thread!!");*/
    }
    #endregion
  }
}
