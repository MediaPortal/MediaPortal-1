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
    private Object tuningLock = new Object();

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
      
      

      // Method: SETUP, Play...
      //C->S[0]SETUP[1]rtsp://example.com/media.mp4/streamid=0[2]RTSP/1.0
      string[] parts = message.Split(' ');

      if (parts.Length < 2 || !Enum.TryParse<RtspRequestMethod>(parts[0], true, out _method))
      {
        this.LogDebug("SAT>IP: Message Invalid");
        return;
      }


      switch (_method)
      {
        case RtspRequestMethod.SETUP:
          #region setup
          this.LogDebug("Case Setup");

          RTSP_setup(requestHeader, clientStream, parts);

          break;
          #endregion setup
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

          RTSP_play(requestHeader, clientStream, parts);

          #endregion
          break;
        case RtspRequestMethod.DESCRIBE:
          #region describe
          this.LogDebug("Case Describe");

          RTSP_describe(requestHeader, clientStream, parts);

          #endregion
          break;
        case RtspRequestMethod.OPTIONS:
          #region options
          this.LogDebug("Case options");

          RTSP_options(requestHeader, clientStream);

          #endregion
          break;
        case RtspRequestMethod.TEARDOWN:
          #region teardown
          this.LogDebug("Case teadown");

          RTSP_teardown(requestHeader, clientStream);

          #endregion
          break;
        default:
          #region bad request
          this.LogDebug("case: Bad Request");

          RTSP_badRequest(requestHeader, clientStream);
          
          #endregion
          break;
      }
    }

    #region RTSP sections

    private void RTSP_setup(RtspRequestHeader requestHeader, NetworkStream clientStream, string[] parts)
    {
      Uri uri;
      NameValueCollection query;
      StringBuilder rtspDescribeRequest = new StringBuilder();
      byte[] buffer;
      
      RtspClients client = new RtspClients();
      client.rtpClientPort = requestHeader.rtpPort;
      client.rtcpClientPort = requestHeader.rtcpPort;

      // setting up the server ports
      int rtpPort = findFreePort();
      client.rtpServerPort = rtpPort;
      client.rtcpServerPort = rtpPort + 1;

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
      rtspDescribeRequest.AppendFormat("Transport: RTP/AVP/UDP;unicast;client_port={0}-{1};source={2};server_port={3}-{4}\r\n", requestHeader.rtpPort, requestHeader.rtcpPort, serverIp, rtpPort, rtpPort + 1);
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
    }

    private void RTSP_play(RtspRequestHeader requestHeader, NetworkStream clientStream, string[] parts)
    {
      Uri uri;
      NameValueCollection query;
      StringBuilder rtspDescribeRequest = new StringBuilder();
      byte[] buffer;
      
      uri = new Uri(parts[1]);
      query = HttpUtility.ParseQueryString(uri.Query);
      //int streamID = int.Parse(query.Get("stream"));

      // check if the client was already setup
      if (!clients.ContainsKey(requestHeader.sessionId))
      {
        RTSP_badRequest(requestHeader, clientStream);
        return;
      }

      // rtsp or SAT>IP?

      if (query.Get("channelId") != null && query.Get("pmtPid") != null)
      {
        // rtsp
        if (!RTSP_play_rtsp(requestHeader, clientStream, query))
        {
          RTSP_serviceUnavailable(requestHeader, clientStream);
          return;
        }
      }
      else
      {
        // SAT>IP
        if (!RTSP_play_satip(requestHeader, clientStream, query))
        {
          RTSP_serviceUnavailable(requestHeader, clientStream);
          return;
        }
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
    }
    
    private bool RTSP_play_rtsp(RtspRequestHeader requestHeader, NetworkStream clientStream, NameValueCollection query)
    {
      if (query.Get("channelId") == null || query.Get("pmtPid") == null)
      {
        return false;
      }

      int channelId;
      if (int.TryParse(query.Get("channelId"), out channelId))
      {
        this.LogDebug("SAT>IP: channelId {0} on sessionID: {1}", channelId, requestHeader.sessionId);
      }

      int pmtPid;
      if (int.TryParse(query.Get("pmtPid"), out pmtPid))
      {
        this.LogDebug("SAT>IP: pmtPid {0} on sessionID: {1}", pmtPid, requestHeader.sessionId);
      }

      TuningDetail tuningDetail = ChannelManagement.GetChannel(channelId).TuningDetails[0];
      clients[requestHeader.sessionId].freq = tuningDetail.Frequency / 1000;
      clients[requestHeader.sessionId].msys = getChannelTypeAsString(tuningDetail.ChannelType);


      if (!performTuning(requestHeader.sessionId, requestHeader, clientStream, channelId, pmtPid))
      {
        return false;
      }

      // send commands to the filter
      FilterCommunication communication = new FilterCommunication(GlobalServiceProvider.Get<IControllerService>().CardDevice(clients[requestHeader.sessionId].card.Id), clients[requestHeader.sessionId].slot);
      communication.addPmt(pmtPid);
      communication.addClientPort(clients[requestHeader.sessionId].rtpClientPort);
      communication.addClientIp(clients[requestHeader.sessionId].ip);
      communication.requestNewSlot();
      communication.send();

      return true;
    }
    
    private bool RTSP_play_satip(RtspRequestHeader requestHeader, NetworkStream clientStream, NameValueCollection query)
    {
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
        syncPidsWithFilter(requestHeader.sessionId, GlobalServiceProvider.Get<IControllerService>().CardDevice(clients[requestHeader.sessionId].card.Id));
      }

      parseQuery(requestHeader.sessionId, query);

      if (!performTuning(requestHeader.sessionId, requestHeader, clientStream))
      {
        return false;
      }

      // send commands to the filter
      FilterCommunication communication = new FilterCommunication(GlobalServiceProvider.Get<IControllerService>().CardDevice(clients[requestHeader.sessionId].card.Id), clients[requestHeader.sessionId].slot);
      communication.addClientPort(clients[requestHeader.sessionId].rtpClientPort);
      communication.addClientIp(clients[requestHeader.sessionId].ip);
      communication.requestNewSlot();
      communication.send();

      return true;
    }

    private void RTSP_describe(RtspRequestHeader requestHeader, NetworkStream clientStream, string[] parts)
    {
      StringBuilder rtspDescribeRequest = new StringBuilder();
      byte[] buffer;
      
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
    }

    private void RTSP_options(RtspRequestHeader requestHeader, NetworkStream clientStream)
    {
      StringBuilder rtspDescribeRequest = new StringBuilder();
      byte[] buffer;
      
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
    }
    
    private void RTSP_teardown(RtspRequestHeader requestHeader, NetworkStream clientStream)
    {
      StringBuilder rtspDescribeRequest = new StringBuilder();
      byte[] buffer;

      if (clients.ContainsKey(requestHeader.sessionId))
      {
        // TODO: send stop command to SAT>IP Filter
        // Test: start timeshifting in setup Tv
        //       connect to the same channel via vlc
        //       stop VLC => Streaming should stop!

        // stop timeshifting
        this.LogDebug("SAT>IP: remove user from card with id {0} - sessionID {1}", clients[requestHeader.sessionId].cardId, requestHeader.sessionId);
        cards[clients[requestHeader.sessionId].cardId].removeUser(int.Parse(requestHeader.sessionId));
        IUser _user;
        lock (tuningLock)
        {
          GlobalServiceProvider.Get<IControllerService>().StopTimeShifting(clients[requestHeader.sessionId].user.Name, out _user);
          clients[requestHeader.sessionId].user = _user;
        }

        if (cards[clients[requestHeader.sessionId].cardId].ownerId == -1)
        {
          this.LogDebug("SAT>IP: no users on card => stop timeshifting on card with id {0} - sessionID {1}", clients[requestHeader.sessionId].cardId, requestHeader.sessionId);
          IVirtualCard card = clients[requestHeader.sessionId].card;
          // delete card from card array
          cards.Remove(clients[requestHeader.sessionId].cardId);
          lock (tuningLock)
          {
            //card.StopTimeShifting();
          }
        }

        // remove client
        clients.Remove(requestHeader.sessionId);
      }

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
    }

    private void RTSP_badRequest(RtspRequestHeader requestHeader, NetworkStream clientStream)
    {
      StringBuilder rtspDescribeRequest = new StringBuilder();
      byte[] buffer;
      
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
    }

    private void RTSP_serviceUnavailable(RtspRequestHeader requestHeader, NetworkStream clientStream)
    {
      StringBuilder rtspDescribeRequest = new StringBuilder();
      byte[] buffer;

      // remove client
      clients.Remove(requestHeader.sessionId);

      // creating the response

      rtspDescribeRequest.Append("RTSP/1.0 503 Service Unavailable\r\n");
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
    }

    #endregion RTSP sections

    private bool performTuning(string sessionId, RtspRequestHeader requestHeader, NetworkStream clientStream, int channelId = -1, int pmtPid = -1)
    {
      if (clients[sessionId].tunedToFrequency != clients[sessionId].freq)
      {
        this.LogDebug("SAT>IP: tuned Freq [{0}] != requested Freq [{1}] - sessionID {2}", clients[sessionId].tunedToFrequency, clients[sessionId].freq, sessionId);
        IUser _user;

        // remove user from current card
        if (clients[sessionId].isTunedToFrequency)
        {
          this.LogDebug("SAT>IP: remove user from card with id {0} - sessionID {1}", clients[sessionId].cardId, sessionId);
          cards[clients[sessionId].cardId].removeUser(int.Parse(sessionId));
          lock (tuningLock)
          {
            GlobalServiceProvider.Get<IControllerService>().StopTimeShifting(clients[sessionId].user.Name, out _user);
            clients[sessionId].user = _user;
          }
          if (cards[clients[sessionId].cardId].ownerId == -1)
          {
            this.LogDebug("SAT>IP: no users on card => stop timeshifting on card with id {0} - sessionID {1}", clients[sessionId].cardId, sessionId);
            //cards[clients[sessionId].cardId].card.StopTimeShifting();
            // delete card from card array
            cards.Remove(clients[sessionId].cardId);
          }
        }

        // get the right tuning details, this is needed to ensure that TVE decrypts the channel if possible
        TuningDetail _tuningDetail = null;

        if (channelId != -1 && pmtPid != -1)
        {
          this.LogInfo("SAT>IP: There is a channel Id [{0}] so use this for the TuningDetail selection, sessionId: {1}", channelId, sessionId);
          foreach (TuningDetail detail in ChannelManagement.GetChannel(channelId).TuningDetails)
          {
            if (detail.PmtPid == pmtPid)
            {
              _tuningDetail = detail;
              this.LogInfo("SAT>IP: Selected TuningDetail with IdTuning={0}, sessionId: {1}", detail.IdTuning, sessionId);
              break;
            }
          }
        }
        else
        {
          _tuningDetail = ChannelManagement.GetTuningDetail(getChannelTypeAsInt(clients[sessionId].msys), (clients[sessionId].freq * 1000));
        }

        if (_tuningDetail == null)
        {
          Log.Debug("SAT>IP: no such channel found!");
          return false;
        }

        this.LogInfo("SAT>IP: creating User: \"SAT>IP - {0} - {1}\" for sessionId: {1}", clients[sessionId].ip, sessionId);
        _user = UserFactory.CreateBasicUser("SAT>IP - " + clients[sessionId].ip + " - " + sessionId);
        IVirtualCard _card;
        this.LogInfo("SAT>IP: Tuning to freq={0} for sessionId: {1}", clients[sessionId].freq, sessionId);
        lock (tuningLock)
        {
          TvResult result = GlobalServiceProvider.Get<IControllerService>().StartTimeShifting(_user.Name, _tuningDetail.IdChannel, out _card, out _user);
          if (result != TvResult.Succeeded)
          {
            this.LogError("SAT>IP: Tuning failed" + result);
            return false;
          }
          else
          {
            this.LogInfo("SAT>IP: tunging success");
          }
        }

        if (!cards.ContainsKey(_card.Id))
        {
          RtspCards card = new RtspCards();
          card.ownerId = int.Parse(sessionId);
          card.user = _user;
          card.card = _card;
          card.tuningDetail = _tuningDetail;
          card.freq = clients[sessionId].freq;
          card.msys = clients[sessionId].msys;
          card.devicePath = GlobalServiceProvider.Get<IControllerService>().CardDevice(_card.Id); // device path
          cards.Add(_card.Id, card);
        }
        
        clients[sessionId].cardId = _card.Id;
        clients[sessionId].slot = cards[_card.Id].getSlot(int.Parse(sessionId));

        clients[sessionId].card = _card;
        clients[sessionId].user = _user;
        clients[sessionId].tuningDetail = _tuningDetail;

      }

      clients[sessionId].isTunedToFrequency = true;
      clients[sessionId].tunedToFrequency = clients[sessionId].freq;

      return true;
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

    private string getChannelTypeAsString(int channelType)
    {
      string _channelType;

      if (channelType == 4)
      {
        _channelType = "DVBT";
      }
      else if (channelType == 3)
      {
        _channelType = "DVBS";
      }
      else if (channelType == 2)
      {
        _channelType = "DVBC";
      }
      /*else if (channel is DVBIPChannel)
      {
        channelType = 7;
      }*/
      else // must be ATSCChannel
      {
        _channelType = "ATSC";
      }
      return _channelType;
    }

    private bool cardAlreadyTunedToFreq(string msys, int freq, out int cardKey)
    {
      foreach (KeyValuePair<int, RtspCards> card in cards) {
        if (card.Value.freq == freq && string.Equals(card.Value.msys, msys, StringComparison.CurrentCultureIgnoreCase) && card.Value.getNumberOfFreeSlots() > 0)
        {
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
  }
}
