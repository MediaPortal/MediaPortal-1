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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using UPnP.Infrastructure.CP.Description;
using RtspClient = Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp.RtspClient;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// A base implementation of <see cref="ITuner"/> for SAT>IP tuners.
  /// </summary>
  internal abstract class TunerSatIpBase : TunerBase, IMpeg2PidFilter
  {
    #region constants

    private static readonly Regex REGEX_DESCRIBE_RESPONSE_SIGNAL_INFO = new Regex(@";tuner=\d+,(\d+),(\d+),(\d+),", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex REGEX_RTSP_SESSION_HEADER = new Regex(@"\s*([^\s;]+)(;timeout=(\d+))?");
    private static readonly TimeSpan DEFAULT_RTSP_SESSION_TIMEOUT = new TimeSpan(0, 0, 60);
    private static readonly TimeSpan RTCP_REPORT_WAIT_TIMEOUT = new TimeSpan(0, 0, 0, 0, 400);    // specification says the server should deliver 5 reports per second; we are generous and allow double time

    #endregion

    #region variables

    /// <summary>
    /// The SAT>IP server's UPnP device descriptor.
    /// </summary>
    protected DeviceDescriptor _serverDescriptor = null;

    /// <summary>
    /// The IP address of the local NIC which is connected to the SAT>IP server.
    /// </summary>
    protected IPAddress _localIpAddress = null;

    /// <summary>
    /// The SAT>IP server's IP address.
    /// </summary>
    protected string _serverIpAddress = string.Empty;

    /// <summary>
    /// An RTSP client, used to communicate with the SAT>IP server.
    /// </summary>
    private RtspClient _rtspClient = null;

    /// <summary>
    /// The current RTSP session ID. Used in the header of all RTSP messages
    /// sent to the server.
    /// </summary>
    private string _rtspSessionId = string.Empty;

    /// <summary>
    /// The time after which the SAT>IP server will stop streaming if it does
    /// not receive some kind of interaction.
    /// </summary>
    private TimeSpan _rtspSessionTimeout = TimeSpan.Zero;

    /// <summary>
    /// The current SAT>IP stream ID. Used as part of all URIs sent to the
    /// SAT>IP server.
    /// </summary>
    private string _satIpStreamId = string.Empty;

    /// <summary>
    /// A thread, used to periodically send RTSP OPTIONS to tell the SAT>IP
    /// server not to stop streaming.
    /// </summary>
    private Thread _streamingKeepAliveThread = null;

    /// <summary>
    /// An event, used to stop the streaming keep-alive thread.
    /// </summary>
    private ManualResetEvent _streamingKeepAliveThreadStopEvent = null;

    /// <summary>
    /// A thread, used to listen for RTCP reports containing signal status
    /// updates.
    /// </summary>
    private Thread _rtcpListenerThread = null;

    /// <summary>
    /// An event, used to stop the RTCP listener thread.
    /// </summary>
    private ManualResetEvent _rtcpListenerThreadStopEvent = null;

    /// <summary>
    /// The port on which the RTCP listener thread listens.
    /// </summary>
    private int _rtcpClientPort = -1;

    /// <summary>
    /// The port that the RTCP listener thread listens to.
    /// </summary>
    private int _rtcpServerPort = -1;

    // PID filter control variables
    private bool _isPidFilterDisabled = false;
    private HashSet<ushort> _pidFilterPidsToRemove = new HashSet<ushort>();
    private HashSet<ushort> _pidFilterPidsToAdd = new HashSet<ushort>();

    // signal status
    private volatile bool _isSignalLocked = false;
    private int _signalStrength = 0;
    private int _signalQuality = 0;

    /// <summary>
    /// Internal stream tuner, used to receive the RTP stream. This allows us
    /// to decouple the stream tuning implementation (eg. DirectShow) from the
    /// SAT>IP implementation.
    /// </summary>
    private ITunerInternal _streamTuner = null;

    /// <summary>
    /// The tuner's channel scanning interface.
    /// </summary>
    private IChannelScannerInternal _channelScanner = null;

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpBase"/> class.
    /// </summary>
    /// <param name="serverDescriptor">The server's UPnP device description.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    /// <param name="typeIndicator">A character identifying the tuner type.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the hardware.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    public TunerSatIpBase(DeviceDescriptor serverDescriptor, int sequenceNumber, string typeIndicator, BroadcastStandard supportedBroadcastStandards, ITunerInternal streamTuner)
      : base(serverDescriptor.FriendlyName + " Tuner " + typeIndicator + sequenceNumber, serverDescriptor.DeviceUUID + typeIndicator + sequenceNumber, sequenceNumber.ToString(), serverDescriptor.DeviceUUID, supportedBroadcastStandards)
    {
      ChannelStream streamChannel = new ChannelStream();
      streamChannel.Url = "rtp://127.0.0.1";
      if (streamTuner == null || !streamTuner.CanTune(streamChannel))
      {
        throw new TvException("Internal tuner implementation is not usable.");
      }

      _serverDescriptor = serverDescriptor;
      _streamTuner = new TunerInternalWrapper(streamTuner);
      _localIpAddress = serverDescriptor.RootDescriptor.SSDPRootEntry.PreferredLink.Endpoint.EndPointIPAddress;
      _serverIpAddress = new Uri(serverDescriptor.RootDescriptor.SSDPRootEntry.PreferredLink.DescriptionLocation).Host;
    }

    ~TunerSatIpBase()
    {
      Dispose(false);
    }

    #endregion

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="parameters">A URI section specifying the tuning parameters.</param>
    protected void PerformTuning(IChannelDvb channel, string parameters)
    {
      this.LogDebug("SAT>IP base: perform tuning");

      RtspRequest request;
      RtspResponse response = null;
      string rtspUri = null;
      string rtpUrl = null;

      if (_isPidFilterDisabled)
      {
        parameters += "&pids=all";
      }
      else
      {
        parameters += "&pids=0";
        _pidFilterPidsToRemove.Clear();
        _pidFilterPidsToRemove.Add(0);
      }

      if (!string.IsNullOrEmpty(_satIpStreamId))
      {
        // Change channel = RTSP PLAY.
        rtspUri = string.Format("rtsp://{0}/stream={1}?{2}", _serverIpAddress, _satIpStreamId, parameters);
        this.LogDebug("SAT>IP base: send RTSP PLAY, URI = {0}", rtspUri);
        request = new RtspRequest(RtspMethod.Play, rtspUri);
        request.Headers.Add("Session", _rtspSessionId);
        if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
        {
          throw new TvException("Failed to tune, non-OK RTSP PLAY status code {0} {1}.", response.StatusCode, response.ReasonPhrase);
        }
        this.LogDebug("SAT>IP base: RTSP PLAY response okay");
        return;
      }

      // First tune = RTSP SETUP.
      // Find free ports for receiving the RTP and RTCP streams.
      int rtpClientPort = 0;
      HashSet<int> usedPorts = new HashSet<int>();
      IPEndPoint[] activeUdpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
      foreach (IPEndPoint listener in activeUdpListeners)
      {
        if (listener.Address.Equals(_localIpAddress))   // Careful! The == operator is not overloaded.
        {
          usedPorts.Add(listener.Port);
        }
      }
      for (int port = 40000; port <= 65534; port += 2)
      {
        // We need two adjacent UDP ports. One for RTP; one for RTCP. By
        // convention, the RTP port is even.
        if (!usedPorts.Contains(port) && !usedPorts.Contains(port + 1))
        {
          rtpClientPort = port;
          _rtcpClientPort = port + 1;
          break;
        }
      }

      // SETUP a session.
      rtspUri = string.Format("rtsp://{0}/?{1}", _serverIpAddress, parameters);
      this.LogDebug("SAT>IP base: send RTSP SETUP, URI = {0}, RTP client port = {1}", rtspUri, rtpClientPort);
      if (_rtspClient == null)
      {
        _rtspClient = new RtspClient(_serverIpAddress);
      }
      request = new RtspRequest(RtspMethod.Setup, rtspUri);
      request.Headers.Add("Transport", string.Format("RTP/AVP;unicast;client_port={0}-{1}", rtpClientPort, rtpClientPort + 1));
      if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
      {
        throw new TvException("Failed to tune, non-OK RTSP SETUP status code {0} {1}.", response.StatusCode, response.ReasonPhrase);
      }

      // Handle the SETUP response.
      // Find the SAT>IP stream ID.
      if (!response.Headers.TryGetValue("com.ses.streamID", out _satIpStreamId))
      {
        throw new TvException("Failed to tune, not able to find stream ID header in RTSP SETUP response.");
      }

      // Find the RTSP session ID and timeout.
      string sessionHeader;
      if (!response.Headers.TryGetValue("Session", out sessionHeader))
      {
        throw new TvException("Failed to tune, not able to find session header in RTSP SETUP response.");
      }
      Match m = REGEX_RTSP_SESSION_HEADER.Match(sessionHeader);
      if (!m.Success)
      {
        throw new TvException("Failed to tune, RTSP SETUP response session header \"{0}\" format not recognised.", sessionHeader);
      }
      _rtspSessionId = m.Groups[1].Captures[0].Value;
      if (m.Groups[3].Captures.Count == 1)
      {
        _rtspSessionTimeout = new TimeSpan(0, 0, int.Parse(m.Groups[3].Captures[0].Value));
      }
      else
      {
        _rtspSessionTimeout = DEFAULT_RTSP_SESSION_TIMEOUT;
      }

      // Find the server's streaming port and check that it registered our
      // preferred local port.
      bool foundRtpTransport = false;
      string rtpServerPort = null;
      _rtcpClientPort = 0;    // If not specified: any available.
      string transportHeader;
      if (!response.Headers.TryGetValue("Transport", out transportHeader))
      {
        throw new TvException("Failed to tune, not able to find transport header in RTSP SETUP response.");
      }
      string[] transports = transportHeader.Split(',');
      foreach (string transport in transports)
      {
        if (transport.Trim().StartsWith("RTP/AVP"))
        {
          foundRtpTransport = true;
          string[] sections = transport.Split(';');
          foreach (string section in sections)
          {
            string[] parts = section.Split('=');
            if (parts[0].Equals("server_port"))
            {
              string[] ports = parts[1].Split('-');
              rtpServerPort = ports[0];
              _rtcpServerPort = int.Parse(ports[1]);
            }
            else if (parts[0].Equals("client_port"))
            {
              string[] ports = parts[1].Split('-');
              if (!ports[0].Equals(rtpClientPort.ToString()))
              {
                this.LogWarn("SAT>IP base: server specified RTP client port {0} instead of {1}", ports[0], rtpClientPort);
              }
              if (!ports[1].Equals(_rtcpClientPort.ToString()))
              {
                this.LogWarn("SAT>IP base: server specified RTCP client port {0} instead of {1}", ports[1], _rtcpClientPort);
              }
              rtpClientPort = int.Parse(ports[0]);
              _rtcpClientPort = int.Parse(ports[1]);
            }
          }
        }
      }
      if (!foundRtpTransport)
      {
        throw new TvException("Failed to tune, not able to find RTP transport details in RTSP SETUP response transport header.");
      }

      // Construct the RTP URL.
      if (string.IsNullOrEmpty(rtpServerPort) || rtpServerPort.Equals("0"))
      {
        rtpUrl = string.Format("rtp://{0}@{1}:{2}", _serverIpAddress, _localIpAddress, rtpClientPort);
      }
      else
      {
        rtpUrl = string.Format("rtp://{0}:{1}@{2}:{3}", _serverIpAddress, rtpServerPort, _localIpAddress, rtpClientPort);
      }
      this.LogDebug("SAT>IP base: RTSP SETUP response okay");
      this.LogDebug("  session ID = {0}", _rtspSessionId);
      this.LogDebug("  time-out   = {0} s", _rtspSessionTimeout.TotalSeconds);
      this.LogDebug("  stream ID  = {0}", _satIpStreamId);
      this.LogDebug("  RTP URL    = {0}", rtpUrl);
      this.LogDebug("  RTCP ports = {0}/{1}", _rtcpClientPort, _rtcpServerPort);

      // Configure the stream source filter to receive the RTP stream.
      this.LogDebug("SAT>IP base: configure stream source filter");
      ChannelStream streamChannel = new ChannelStream();
      streamChannel.Url = rtpUrl;

      // Copy the other channel parameters from the original channel.
      streamChannel.IsEncrypted = channel.IsEncrypted;
      streamChannel.LogicalChannelNumber = channel.LogicalChannelNumber;
      streamChannel.MediaType = channel.MediaType;
      streamChannel.Name = channel.Name;
      streamChannel.OriginalNetworkId = channel.OriginalNetworkId;
      streamChannel.PmtPid = channel.PmtPid;
      streamChannel.Provider = channel.Provider;
      streamChannel.ProgramNumber = channel.ProgramNumber;
      streamChannel.TransportStreamId = channel.TransportStreamId;

      _streamTuner.PerformTuning(streamChannel);
    }

    #endregion

    #region streaming keep-alive thread

    private void StartStreamingKeepAliveThread()
    {
      // Kill the existing thread if it is in "zombie" state.
      if (_streamingKeepAliveThread != null && !_streamingKeepAliveThread.IsAlive)
      {
        StopStreamingKeepAliveThread();
      }

      if (_streamingKeepAliveThread == null)
      {
        this.LogDebug("SAT>IP base: starting new streaming keep-alive thread");
        _streamingKeepAliveThreadStopEvent = new ManualResetEvent(false);
        _streamingKeepAliveThread = new Thread(new ThreadStart(StreamingKeepAlive));
        _streamingKeepAliveThread.Name = string.Format("SAT>IP tuner {0} streaming keep-alive", TunerId);
        _streamingKeepAliveThread.IsBackground = true;
        _streamingKeepAliveThread.Priority = ThreadPriority.Lowest;
        _streamingKeepAliveThread.Start();
      }
    }

    private void StopStreamingKeepAliveThread()
    {
      if (_streamingKeepAliveThread != null)
      {
        if (!_streamingKeepAliveThread.IsAlive)
        {
          this.LogWarn("SAT>IP base: aborting old streaming keep-alive thread");
          _streamingKeepAliveThread.Abort();
        }
        else
        {
          _streamingKeepAliveThreadStopEvent.Set();
          if (!_streamingKeepAliveThread.Join((int)_rtspSessionTimeout.TotalMilliseconds * 2))
          {
            this.LogWarn("SAT>IP base: failed to join streaming keep-alive thread, aborting thread");
            _streamingKeepAliveThread.Abort();
          }
        }
        _streamingKeepAliveThread = null;
        if (_streamingKeepAliveThreadStopEvent != null)
        {
          _streamingKeepAliveThreadStopEvent.Close();
          _streamingKeepAliveThreadStopEvent = null;
        }
      }
    }

    private void StreamingKeepAlive()
    {
      try
      {
        while (!_streamingKeepAliveThreadStopEvent.WaitOne((int)(_rtspSessionTimeout - new TimeSpan(0, 0, 5)).TotalMilliseconds * 1000))  // -5 seconds to avoid time-out
        {
          RtspRequest request = new RtspRequest(RtspMethod.Options, string.Format("rtsp://{0}/", _serverIpAddress));
          request.Headers.Add("Session", _rtspSessionId);
          RtspResponse response;
          if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
          {
            this.LogWarn("SAT>IP base: streaming keep-alive request/response failed, non-OK RTSP OPTIONS status code {0} {1}", response.StatusCode, response.ReasonPhrase);
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "SAT>IP base: streaming keep-alive thread exception");
        return;
      }
      this.LogDebug("SAT>IP base: streaming keep-alive thread stopping");
    }

    #endregion

    #region RTCP listener thread

    private void StartRtcpListenerThread()
    {
      // Kill the existing thread if it is in "zombie" state.
      if (_rtcpListenerThread != null && !_rtcpListenerThread.IsAlive)
      {
        StopRtcpListenerThread();
      }

      if (_rtcpListenerThread == null)
      {
        this.LogDebug("SAT>IP base: starting new RTCP listener thread");
        _rtcpListenerThreadStopEvent = new ManualResetEvent(false);
        _rtcpListenerThread = new Thread(new ThreadStart(RtcpListener));
        _rtcpListenerThread.Name = string.Format("SAT>IP tuner {0} RTCP listener", TunerId);
        _rtcpListenerThread.IsBackground = true;
        _rtcpListenerThread.Priority = ThreadPriority.Lowest;
        _rtcpListenerThread.Start();
      }
    }

    private void StopRtcpListenerThread()
    {
      if (_rtcpListenerThread != null)
      {
        if (!_rtcpListenerThread.IsAlive)
        {
          this.LogWarn("SAT>IP base: aborting old RTCP listener thread");
          _rtcpListenerThread.Abort();
        }
        else
        {
          _rtcpListenerThreadStopEvent.Set();
          if (!_rtcpListenerThread.Join((int)RTCP_REPORT_WAIT_TIMEOUT.TotalMilliseconds * 2))
          {
            this.LogWarn("SAT>IP base: failed to join RTCP listener thread, aborting thread");
            _rtcpListenerThread.Abort();
          }
        }
        _rtcpListenerThread = null;
        if (_rtcpListenerThreadStopEvent != null)
        {
          _rtcpListenerThreadStopEvent.Close();
          _rtcpListenerThreadStopEvent = null;
        }
      }
    }

    private void RtcpListener()
    {
      try
      {
        bool receivedGoodBye = false;
        UdpClient udpClient = new UdpClient(new IPEndPoint(_localIpAddress, _rtcpClientPort));
        try
        {
          udpClient.Client.ReceiveTimeout = (int)RTCP_REPORT_WAIT_TIMEOUT.TotalMilliseconds;
          IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(_serverIpAddress), _rtcpServerPort);
          while (!receivedGoodBye && !_rtcpListenerThreadStopEvent.WaitOne(1))
          {
            byte[] packets = null;
            try
            {
              packets = udpClient.Receive(ref serverEndPoint);
            }
            catch (Exception ex)
            {
              this.LogWarn(ex, "SAT>IP base: failed to receive RTCP packets");
            }
            if (packets == null)
            {
              continue;
            }

            int offset = 0;
            while (offset + 8 <= packets.Length)
            {
              // Refer to RFC 3550.
              // https://www.ietf.org/rfc/rfc3550.txt
              byte packetType = packets[offset + 1];
              int packetByteCount = ((packets[offset + 2] << 8) + packets[offset + 3] + 1) * 4;
              if (offset + packetByteCount > packets.Length)
              {
                this.LogWarn("SAT>IP base: received incomplete RTCP packet, offset = {0}", offset);
                Dump.DumpBinary(packets);
                break;
              }

              if (packetType == 203)  // goodbye
              {
                receivedGoodBye = true;
                break;
              }
              else if (packetType == 204) // application-defined
              {
                int offsetStartOfPacket = offset;
                offset += 8;  // skip to the start of the name SSRC/CSRC
                if (offset + 4 > packets.Length)
                {
                  this.LogWarn("SAT>IP base: received RTCP application-defined packet too short to contain name, offset = {0}", offsetStartOfPacket);
                  Dump.DumpBinary(packets);
                  break;
                }
                string name = System.Text.Encoding.ASCII.GetString(packets, offset, 4);
                offset += 4;
                if (!name.Equals("SES1"))
                {
                  // Not SAT>IP data. Odd but okay.
                  offset = offsetStartOfPacket + packetByteCount;
                  continue;
                }
                if (offset + 4 > packets.Length)
                {
                  this.LogWarn("SAT>IP base: received SAT>IP RTCP packet too short to contain string length, offset = {0}", offsetStartOfPacket);
                  Dump.DumpBinary(packets);
                  break;
                }
                int stringByteCount = (packets[offset + 2] << 8) + packets[offset + 3];
                offset += 4;
                if (offset + stringByteCount > packets.Length)
                {
                  this.LogWarn("SAT>IP base: received SAT>IP RTCP packet too short to contain string, offset = {0}", offsetStartOfPacket);
                  Dump.DumpBinary(packets);
                  break;
                }
                string description = System.Text.Encoding.UTF8.GetString(packets, offset, stringByteCount);
                Match m = REGEX_DESCRIBE_RESPONSE_SIGNAL_INFO.Match(description);
                if (m.Success)
                {
                  _isSignalLocked = m.Groups[2].Captures[0].Value.Equals("1");
                  _signalStrength = int.Parse(m.Groups[1].Captures[0].Value) * 100 / 255;   // strength: 0..255 => 0..100
                  _signalQuality = int.Parse(m.Groups[3].Captures[0].Value) * 100 / 15;     // quality: 0..15 => 0..100
                }
                offset = offsetStartOfPacket + packetByteCount;
              }
              else
              {
                offset += packetByteCount;
              }
            }
          }
        }
        finally
        {
          udpClient.Close();
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "SAT>IP base: RTCP listener thread exception");
        return;
      }
      this.LogDebug("SAT>IP base: RTCP listener thread stopping");
    }

    #endregion

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(Tuner configuration)
    {
      ITuner tuner = _streamTuner as ITuner;
      if (tuner != null)
      {
        tuner.ReloadConfiguration();
      }
      else
      {
        _streamTuner.ReloadConfiguration(configuration);
      }
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      TunerExtensionLoader loader = new TunerExtensionLoader();
      IList<ITunerExtension> extensions = loader.Load(this, _serverDescriptor);

      // Add the stream tuner extensions to our extensions, but don't re-sort
      // by priority afterwards. This ensures that our extensions are always
      // given first consideration.
      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Dvb;
      }
      IList<ITunerExtension> streamTunerExtensions = _streamTuner.PerformLoading(streamFormat);
      foreach (ITunerExtension e in streamTunerExtensions)
      {
        extensions.Add(e);
      }

      _channelScanner = _streamTuner.InternalChannelScanningInterface;
      if (_channelScanner != null)
      {
        _channelScanner.Tuner = this;
      }
      return extensions;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformSetTunerState(TunerState state, bool isFinalising = false)
    {
      this.LogDebug("SAT>IP base: perform set tuner state");
      if (isFinalising)
      {
        return;
      }

      RtspRequest request = null;
      RtspResponse response = null;

      if (_rtspClient != null && !string.IsNullOrEmpty(_satIpStreamId) && !string.IsNullOrEmpty(_rtspSessionId))
      {
        if (state == TunerState.Started)
        {
          request = new RtspRequest(RtspMethod.Play, string.Format("rtsp://{0}/stream={1}", _serverIpAddress, _satIpStreamId));
          request.Headers.Add("Session", _rtspSessionId);
          if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
          {
            throw new TvException("Failed to start tuner, non-OK RTSP PLAY status code {0} {1}.", response.StatusCode, response.ReasonPhrase);
          }

          StartStreamingKeepAliveThread();
          StartRtcpListenerThread();
        }
        else if (state == TunerState.Stopped)
        {
          StopStreamingKeepAliveThread();
          StopRtcpListenerThread();

          request = new RtspRequest(RtspMethod.Teardown, string.Format("rtsp://{0}/stream={1}", _serverIpAddress, _satIpStreamId));
          request.Headers.Add("Session", _rtspSessionId);
          if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
          {
            throw new TvException("Failed to stop tuner, non-OK RTSP TEARDOWN status code {0} {1}.", response.StatusCode, response.ReasonPhrase);
          }

          _rtspClient.Dispose();
          _rtspClient = null;
          _satIpStreamId = string.Empty;
          _rtspSessionId = string.Empty;
        }
      }

      _streamTuner.PerformSetTunerState(state);
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformUnloading(bool isFinalising = false)
    {
      if (!isFinalising)
      {
        _channelScanner = null;
        if (_streamTuner != null)
        {
          _streamTuner.PerformUnloading();
        }
      }
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    public override void GetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock)
    {
      isLocked = _isSignalLocked;
      isPresent = _isSignalLocked;
      strength = _signalStrength;
      quality = _signalQuality;
    }

    #endregion

    #region interfaces

    /// <summary>
    /// Get the tuner's sub-channel manager.
    /// </summary>
    public override ISubChannelManager SubChannelManager
    {
      get
      {
        return _streamTuner.SubChannelManager;
      }
    }

    /// <summary>
    /// Get the tuner's channel linkage scanning interface.
    /// </summary>
    public override IChannelLinkageScanner InternalChannelLinkageScanningInterface
    {
      get
      {
        return _streamTuner.InternalChannelLinkageScanningInterface;
      }
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public override IChannelScannerInternal InternalChannelScanningInterface
    {
      get
      {
        return _channelScanner;
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public override IEpgGrabberInternal InternalEpgGrabberInterface
    {
      get
      {
        return _streamTuner.InternalEpgGrabberInterface;
      }
    }

    #endregion

    #endregion

    #region IMpeg2PidFilter members

    /// <summary>
    /// Should the filter be enabled for a given transmitter.
    /// </summary>
    /// <param name="tuningDetail">The current transmitter tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ShouldEnable(IChannel tuningDetail)
    {
      // SAT>IP tuners are networked tuners. It is desirable to enable PID filtering in order to
      // reduce the network bandwidth used.
      return true;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.Disable()
    {
      if (!_isPidFilterDisabled)
      {
        this.LogDebug("SAT>IP base: disable PID filter");
        _isPidFilterDisabled = ConfigurePidFilter("pids=all");
        if (_isPidFilterDisabled)
        {
          _pidFilterPidsToRemove.Clear();
          _pidFilterPidsToAdd.Clear();
        }
      }
      return _isPidFilterDisabled;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    int IMpeg2PidFilter.MaximumPidCount
    {
      get
      {
        return -1;  // maximum not known
      }
    }

    /// <summary>
    /// Configure the filter to allow one or more streams to pass through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.AllowStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.UnionWith(pids);
      _pidFilterPidsToRemove.ExceptWith(pids);
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.BlockStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.ExceptWith(pids);
      _pidFilterPidsToRemove.UnionWith(pids);
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ApplyConfiguration()
    {
      string uri = null;
      if (_pidFilterPidsToAdd.Count > 0 && _pidFilterPidsToRemove.Count > 0)
      {
        uri = string.Format("addpids={0}&delpids={1}", string.Join(",", _pidFilterPidsToAdd), string.Join(",", _pidFilterPidsToRemove));
      }
      else if (_pidFilterPidsToAdd.Count > 0)
      {
        uri = "addpids=" + string.Join(",", _pidFilterPidsToAdd);
      }
      else if (_pidFilterPidsToRemove.Count > 0)
      {
        uri = "delpids=" + string.Join(",", _pidFilterPidsToRemove);
      }
      else
      {
        return true;
      }
      this.LogDebug("SAT>IP base: apply PID filter configuration");
      bool result = ConfigurePidFilter(uri);
      if (result)
      {
        _pidFilterPidsToAdd.Clear();
        _pidFilterPidsToRemove.Clear();
        _isPidFilterDisabled = false;
      }
      return result;
    }

    private bool ConfigurePidFilter(string parameters)
    {
      try
      {
        RtspRequest request = new RtspRequest(RtspMethod.Play, string.Format("rtsp://{0}/stream={1}?{2}", _serverIpAddress, _satIpStreamId, parameters));
        request.Headers.Add("Accept", "application/sdp");
        request.Headers.Add("Session", _rtspSessionId);
        RtspResponse response = null;
        if (_rtspClient.SendRequest(request, out response) == RtspStatusCode.Ok)
        {
          this.LogDebug("SAT>IP base: result = success");
          return true;
        }

        this.LogError("SAT>IP base: failed to configure PID Filter, non-OK RTSP PLAY status code {0} {1}", response.StatusCode, response.ReasonPhrase);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "SAT>IP base: exception configuring PID filter");
      }
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the tuner is being disposed.</param>
    protected override void Dispose(bool isDisposing)
    {
      base.Dispose(isDisposing);
      if (isDisposing)
      {
        if (_streamTuner != null)
        {
          _streamTuner.Dispose();
          _streamTuner = null;
        }
        if (_rtspClient != null)
        {
          _rtspClient.Dispose();
          _rtspClient = null;
        }
      }
    }

    #endregion
  }
}