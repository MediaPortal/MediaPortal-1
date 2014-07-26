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
using System.Text.RegularExpressions;
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using UPnP.Infrastructure.CP.Description;
using RtspClient = Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp.RtspClient;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for SAT>IP tuners.
  /// </summary>
  internal abstract class TunerSatIpBase : TunerBase, IMpeg2PidFilter
  {
    #region constants

    private static readonly Regex REGEX_DESCRIBE_RESPONSE_SIGNAL_INFO = new Regex(@";tuner=\d+,(\d+),(\d+),(\d+),", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex REGEX_RTSP_SESSION_HEADER = new Regex(@"\s*([^\s;]+)(;timeout=(\d+))?");
    private const int DEFAULT_RTSP_SESSION_TIMEOUT = 60;    // unit = s

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
    /// The time (in seconds) after which the SAT>IP server will stop streaming
    /// if it does not receive some kind of interaction.
    /// </summary>
    private int _rtspSessionTimeout = -1;

    /// <summary>
    /// The current SAT>IP stream ID. Used as part of all URIs sent to the
    /// SAT>IP server.
    /// </summary>
    private string _satIpStreamId = string.Empty;

    /// <summary>
    /// A thread, used to periodically send RTSP OPTIONS to tell the SAT>IP
    /// server not to stop the stream.
    /// </summary>
    private Thread _keepAliveThread = null;

    /// <summary>
    /// An event, used to stop the stream keep-alive thread.
    /// </summary>
    private AutoResetEvent _keepAliveThreadStopEvent = null;

    // PID filter control variables
    private bool _isPidFilterDisabled = false;
    private HashSet<ushort> _pidFilterPidsToRemove = new HashSet<ushort>();
    private HashSet<ushort> _pidFilterPidsToAdd = new HashSet<ushort>();

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

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpBase"/> class.
    /// </summary>
    /// <param name="serverDescriptor">The server's UPnP device description.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    /// <param name="type">The tuner type.</param>
    public TunerSatIpBase(DeviceDescriptor serverDescriptor, int sequenceNumber, ITunerInternal streamTuner, CardType type)
      : base(serverDescriptor.FriendlyName + " Tuner " + sequenceNumber, serverDescriptor.DeviceUUID + sequenceNumber + type.ToString()[type.ToString().Length - 1], type)
    {
      DVBIPChannel streamChannel = new DVBIPChannel();
      streamChannel.Url = "rtp://127.0.0.1";
      if (streamTuner == null || !streamTuner.CanTune(streamChannel))
      {
        throw new TvException("Internal tuner implementation is not usable.");
      }

      _serverDescriptor = serverDescriptor;
      _streamTuner = streamTuner;
      _localIpAddress = serverDescriptor.RootDescriptor.SSDPRootEntry.PreferredLink.Endpoint.EndPointIPAddress;
      _serverIpAddress = new Uri(serverDescriptor.RootDescriptor.SSDPRootEntry.PreferredLink.DescriptionLocation).Host;
      _productInstanceId = serverDescriptor.DeviceUUID;
      _tunerInstanceId = sequenceNumber.ToString();
    }

    #endregion

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="parameters">A URI section specifying the tuning parameters.</param>
    protected void PerformTuning(string parameters)
    {
      this.LogDebug("SAT>IP base: perform tuning");

      RtspRequest request;
      RtspResponse response = null;
      string rtpUrl = null;

      if (!string.IsNullOrEmpty(_satIpStreamId))
      {
        // Change channel = RTSP PLAY.
        this.LogDebug("SAT>IP base: send RTSP PLAY");
        string uri = string.Format("rtsp://{0}/stream={1}?{2}", _serverIpAddress, _satIpStreamId, parameters);
        request = new RtspRequest(RtspMethod.Play, uri);
        request.Headers.Add("Session", _rtspSessionId);
        if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
        {
          throw new TvException("Failed to tune, non-OK RTSP PLAY status code {0} {1}", response.StatusCode, response.ReasonPhrase);
        }
        this.LogDebug("SAT>IP base: RTSP PLAY response okay");
        return;
      }

      // First tune = RTSP SETUP.
      // Find a free port for receiving the RTP stream.
      int rtpClientPort = 0;
      TcpConnectionInformation[] activeTcpConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
      HashSet<int> usedPorts = new HashSet<int>();
      foreach (TcpConnectionInformation connection in activeTcpConnections)
      {
        if (connection.LocalEndPoint.Address == _localIpAddress)
        {
          usedPorts.Add(connection.LocalEndPoint.Port);
        }
        if (connection.RemoteEndPoint.Address == _localIpAddress)
        {
          usedPorts.Add(connection.RemoteEndPoint.Port);
        }
      }
      for (int port = 40000; port <= 65534; port += 2)
      {
        // We need two adjacent ports. One for RTP; one for RTCP. By
        // convention, the RTP port is even.
        if (!usedPorts.Contains(port) && !usedPorts.Contains(port + 1))
        {
          rtpClientPort = port;
          break;
        }
      }
      this.LogDebug("SAT>IP base: send RTSP SETUP, RTP client port = {0}", rtpClientPort);

      // SETUP a session.
      _rtspClient = new RtspClient(_serverIpAddress);
      request = new RtspRequest(RtspMethod.Setup, string.Format("rtsp://{0}/?{1}", _serverIpAddress, parameters));
      request.Headers.Add("Transport", string.Format("RTP/AVP;unicast;client_port={0}-{1}", rtpClientPort, rtpClientPort + 1));
      if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
      {
        throw new TvException("Failed to tune, non-OK RTSP SETUP status code {0} {1}", response.StatusCode, response.ReasonPhrase);
      }

      // Handle the SETUP response.
      // Find the SAT>IP stream ID.
      if (!response.Headers.TryGetValue("com.ses.streamID", out _satIpStreamId))
      {
        throw new TvException("Failed to tune, not able to find stream ID header in RTSP SETUP response");
      }

      // Find the RTSP session ID and timeout.
      string sessionHeader;
      if (!response.Headers.TryGetValue("Session", out sessionHeader))
      {
        throw new TvException("Failed to tune, not able to find session header in RTSP SETUP response");
      }
      Match m = REGEX_RTSP_SESSION_HEADER.Match(sessionHeader);
      if (!m.Success)
      {
        throw new TvException("Failed to tune, RTSP SETUP response session header {0} format not recognised");
      }
      _rtspSessionId = m.Groups[1].Captures[0].Value;
      if (m.Groups[3].Captures.Count == 1)
      {
        _rtspSessionTimeout = int.Parse(m.Groups[3].Captures[0].Value);
      }
      else
      {
        _rtspSessionTimeout = DEFAULT_RTSP_SESSION_TIMEOUT;
      }

      // Find the server's streaming port and check that it registered our
      // preferred local port.
      bool foundRtpTransport = false;
      string rtpServerPort = null;
      string transportHeader;
      if (!response.Headers.TryGetValue("Transport", out transportHeader))
      {
        throw new TvException("Failed to tune, not able to find transport header in RTSP SETUP response");
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
            }
            else if (parts[0].Equals("client_port"))
            {
              string[] ports = parts[1].Split('-');
              if (!ports[0].Equals(rtpClientPort.ToString()))
              {
                this.LogWarn("SAT>IP base: server specified RTP client port {0} instead of {1}", ports[0], rtpClientPort);
              }
              rtpClientPort = int.Parse(ports[0]);
            }
          }
        }
      }
      if (!foundRtpTransport)
      {
        throw new TvException("Failed to tune, not able to find RTP transport details in RTSP SETUP response transport header");
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
      this.LogDebug("  timeout    = {0}", _rtspSessionTimeout);
      this.LogDebug("  stream ID  = {0}", _satIpStreamId);
      this.LogDebug("  RTP URL    = {0}", rtpUrl);

      // Configure the stream source filter to receive the RTP stream.
      this.LogDebug("SAT>IP base: configure stream source filter");
      DVBIPChannel streamChannel = new DVBIPChannel();
      streamChannel.Url = rtpUrl;
      _streamTuner.PerformTuning(streamChannel);
    }

    #endregion

    #region keep-alive thread

    private void StartKeepAliveThread()
    {
      // Kill the existing thread if it is in "zombie" state.
      if (_keepAliveThread != null && !_keepAliveThread.IsAlive)
      {
        StopKeepAliveThread();
      }

      if (_keepAliveThread == null)
      {
        this.LogDebug("SAT>IP base: starting new keep-alive thread");
        _keepAliveThreadStopEvent = new AutoResetEvent(false);
        _keepAliveThread = new Thread(new ThreadStart(KeepAlive));
        _keepAliveThread.Name = string.Format("SAT>IP tuner {0} keep alive", TunerId);
        _keepAliveThread.IsBackground = true;
        _keepAliveThread.Priority = ThreadPriority.Lowest;
        _keepAliveThread.Start();
      }
    }

    private void StopKeepAliveThread()
    {
      if (_keepAliveThread != null)
      {
        if (!_keepAliveThread.IsAlive)
        {
          this.LogWarn("SAT>IP base: aborting old keep-alive thread");
          _keepAliveThread.Abort();
        }
        else
        {
          _keepAliveThreadStopEvent.Set();
          if (!_keepAliveThread.Join(_rtspSessionTimeout * 2))
          {
            this.LogWarn("SAT>IP base: failed to join keep-alive thread, aborting thread");
            _keepAliveThread.Abort();
          }
        }
        _keepAliveThread = null;
        if (_keepAliveThreadStopEvent != null)
        {
          _keepAliveThreadStopEvent.Close();
          _keepAliveThreadStopEvent = null;
        }
      }
    }

    private void KeepAlive()
    {
      try
      {
        while (!_keepAliveThreadStopEvent.WaitOne((_rtspSessionTimeout - 5) * 1000))  // -5 seconds to avoid timeout
        {
          RtspRequest request = new RtspRequest(RtspMethod.Options, string.Format("rtsp://{0}/", _serverIpAddress));
          request.Headers.Add("Session", _rtspSessionId);
          RtspResponse response;
          if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
          {
            this.LogWarn("SAT>IP base: keep-alive request/response failed, non-OK RTSP OPTIONS status code {0} {1}", response.StatusCode, response.ReasonPhrase);
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "SAT>IP base: keep-alive thread exception");
        return;
      }
      this.LogDebug("SAT>IP base: keep-alive thread stopping");
    }

    #endregion

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public override void ReloadConfiguration()
    {
      base.ReloadConfiguration();
      _streamTuner.ReloadConfiguration();
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    public override void PerformLoading()
    {
      LoadExtensions(_serverDescriptor);
      _streamTuner.PerformLoading();
      _channelScanner = _streamTuner.InternalChannelScanningInterface;
      if (_channelScanner != null)
      {
        _channelScanner.Tuner = this;
        _channelScanner.Helper = new ChannelScannerHelperDvb();
      }
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    public override void PerformSetTunerState(TunerState state)
    {
      this.LogDebug("SAT>IP base: perform set tuner state");

      RtspRequest request = null;
      RtspResponse response = null;

      if (_rtspClient != null && !string.IsNullOrEmpty(_satIpStreamId) && !string.IsNullOrEmpty(_rtspSessionId))
      {
        if (state == TunerState.Started)
        {
          // PLAY to start a previously SETUP stream.
          string pidFilterPhrase = "0";   // If you use "none" the source filter will not receive data => fail to start graph.
          if (_isPidFilterDisabled)
          {
            pidFilterPhrase = "all";
          }
          string uri = string.Format("rtsp://{0}/stream={1}?pids={2}", _serverIpAddress, _satIpStreamId, pidFilterPhrase);
          request = new RtspRequest(RtspMethod.Play, uri);
          request.Headers.Add("Session", _rtspSessionId);
          if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
          {
            throw new TvException("Failed to start tuner, non-OK RTSP PLAY status code {0} {1}", response.StatusCode, response.ReasonPhrase);
          }
          StartKeepAliveThread();
        }
        else if (state == TunerState.Stopped)
        {
          StopKeepAliveThread();

          request = new RtspRequest(RtspMethod.Teardown, string.Format("rtsp://{0}/stream={1}", _serverIpAddress, _satIpStreamId));
          request.Headers.Add("Session", _rtspSessionId);
          if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
          {
            throw new TvException("Failed to stop tuner, non-OK RTSP TEARDOWN status code {0} {1}.", response.StatusCode, response.ReasonPhrase);
          }

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
    public override void PerformUnloading()
    {
      _channelScanner = null;
      if (_streamTuner != null)
      {
        _streamTuner.PerformUnloading();
      }
    }

    #endregion

    #region tuning

    /// <summary>
    /// Allocate a new sub-channel instance.
    /// </summary>
    /// <param name="id">The identifier for the sub-channel.</param>
    /// <returns>the new sub-channel instance</returns>
    public override ITvSubChannel CreateNewSubChannel(int id)
    {
      return _streamTuner.CreateNewSubChannel(id);
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    public override void GetSignalStatus(bool onlyGetLock, out bool isLocked, out bool isPresent, out int strength, out int quality)
    {
      isLocked = false;
      isPresent = false;
      strength = 0;
      quality = 0;

      try
      {
        RtspRequest request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}/stream={1}", _serverIpAddress, _satIpStreamId));
        request.Headers.Add("Accept", "application/sdp");
        request.Headers.Add("Session", _rtspSessionId);
        RtspResponse response = null;
        if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
        {
          this.LogError("SAT>IP base: failed to get signal status, non-OK RTSP DESCRIBE status code {0} {1}", response.StatusCode, response.ReasonPhrase);
          return;
        }

        // Find the first stream information. We assume that all signal statistics apply equally as
        // we're only meant to be using one front end.
        Match m = REGEX_DESCRIBE_RESPONSE_SIGNAL_INFO.Match(response.Body);
        if (m.Success)
        {
          isLocked = m.Groups[2].Captures[0].Value.Equals("1");
          isPresent = isLocked;
          strength = int.Parse(m.Groups[1].Captures[0].Value) * 100 / 255;    // strength: 0..255 => 0..100
          quality = int.Parse(m.Groups[3].Captures[0].Value) * 100 / 15;      // quality: 0..15 => 0..100
          return;
        }

        this.LogError("SAT>IP base: failed to find signal status information in RTSP DESCRIBE response");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "SAT>IP base: exception updating signal status");
      }
    }

    #endregion

    #region interfaces

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
    public override IEpgGrabber InternalEpgGrabberInterface
    {
      get
      {
        return _streamTuner.InternalEpgGrabberInterface;
      }
    }

    #endregion

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public byte Priority
    {
      get
      {
        return 50;
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      // This is a "special" implementation. We do initialisation in other functions.
      return true;
    }

    #region device state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnLoaded(ITVCard tuner, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before the device is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnAfterTune(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the tuner is started but
    /// before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public virtual void OnStarted(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that the TV Engine wants to take; as an output, the action to take.</param>
    public virtual void OnStop(ITVCard tuner, ref TunerAction action)
    {
    }

    #endregion

    #endregion

    #region IMpeg2PidFilter members

    /// <summary>
    /// Should the filter be enabled for the current multiplex.
    /// </summary>
    /// <param name="tuningDetail">The current multiplex/transponder tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    public bool ShouldEnableFilter(IChannel tuningDetail)
    {
      // SAT>IP tuners are networked tuners. It is desirable to enable PID filtering in order to
      // reduce the network bandwidth used.
      return true;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    public bool DisableFilter()
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
    public int MaximumPidCount
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
    /// <returns><c>true</c> if the filter is successfully configured, otherwise <c>false</c></returns>
    public bool AllowStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.UnionWith(pids);
      _pidFilterPidsToRemove.ExceptWith(pids);
      return true;
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    /// <returns><c>true</c> if the filter is successfully configured, otherwise <c>false</c></returns>
    public bool BlockStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.ExceptWith(pids);
      _pidFilterPidsToRemove.UnionWith(pids);
      return true;
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    public bool ApplyFilter()
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
      this.LogDebug("SAT>IP base: apply PID filter");
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
    public override void Dispose()
    {
      base.Dispose();
      if (_streamTuner != null)
      {
        _streamTuner.Dispose();
        _streamTuner = null;
      }
    }

    #endregion
  }
}