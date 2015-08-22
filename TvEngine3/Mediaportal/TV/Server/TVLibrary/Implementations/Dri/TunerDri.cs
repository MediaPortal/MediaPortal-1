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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Service;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Service;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.DeviceTree;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  internal class TunerDri : TunerBase, IConditionalAccessMenuActions
  {
    #region constants

    private static readonly Regex REGEX_SIGNAL_INFO = new Regex(@"^(\d+(\.\d+)?)[^\d]");
    private const int CONNECTION_ID_NOT_CONNECTED = -1;

    #endregion

    #region variables

    // UPnP
    private DeviceDescriptor _descriptor = null;
    private UPnPControlPoint _controlPoint = null;
    private DeviceConnection _deviceConnection = null;
    private StateVariableChangedDlgt _stateVariableDelegate = null;
    private EventSubscriptionFailedDlgt _eventSubscriptionDelegate = null;

    // services
    private ServiceTuner _serviceTuner = null;                // [physical] tuning, scanning
    private ServiceFdc _serviceFdc = null;                    // forward data channel, carries SI and EPG
    private ServiceAux _serviceAux = null;                    // auxiliary analog inputs
    private ServiceEncoder _serviceEncoder = null;            // encoder for auxiliary inputs *and* tuner
    private ServiceCas _serviceCas = null;                    // conditional access
    private ServiceMux _serviceMux = null;                    // PID filtering
    private ServiceSecurity _serviceSecurity = null;          // DRM system
    private ServiceDiag _serviceDiag = null;                  // name/value pair info
    private ServiceUserActivity _serviceUserActivity = null;  // activity notification
    private ServiceAvTransport _serviceAvTransport = null;
    private ServiceConnectionManager _serviceConnectionManager = null;

    // RTSP/RTP
    private string _rtspUri = string.Empty;
    private Rtsp.RtspClient _rtspClient = null;
    private string _rtspSessionId = string.Empty;
    private IPAddress _localIpAddress = null;
    private string _serverIpAddress = string.Empty;

    // UPnP AV
    private int _connectionId = CONNECTION_ID_NOT_CONNECTED;
    private int _avTransportId = -1;

    // These variables are used to ensure we don't interrupt another
    // application using the tuner.
    private bool _gotTunerControl = false;
    private AvTransportState _transportState = AvTransportState.Stopped;

    // Important DRM state variables.
    private CasCardStatus _cardStatus = CasCardStatus.Removed;
    private CasDescramblingStatus _descramblingStatus = CasDescramblingStatus.Unknown;
    private SecurityPairingStatus _pairingStatus = SecurityPairingStatus.Red;

    // CA menu variables
    private object _caMenuCallBackLock = new object();
    private IConditionalAccessMenuCallBack _caMenuCallBack = null;
    private CableCardMmiHandler _caMenuHandler = null;

    private volatile bool _isSignalLocked = false;
    private int _currentFrequency = -1;
    private readonly bool _isCetonDevice = false;
    private bool _canPause = false;
    private ICollection<TunerModulation> _supportedModulationSchemes = null;
    private int _programNumberTimeOut = 2500;   // unit = milliseconds

    /// <summary>
    /// Internal stream tuner, used to receive the RTP stream. This allows us
    /// to decouple the stream tuning implementation (eg. DirectShow) from the
    /// DRI implementation.
    /// </summary>
    private ITunerInternal _streamTuner = null;

    /// <summary>
    /// The tuner's channel scanning interface.
    /// </summary>
    private IChannelScannerInternal _channelScanner = null;

    /// <summary>
    /// The tuner's current tuning parameter values.
    /// </summary>
    private IChannel _currentTuningDetail = null;

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDri"/> class.
    /// </summary>
    /// <param name="descriptor">The UPnP device description.</param>
    /// <param name="tunerInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single tuner.</param>
    /// <param name="productInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single product.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the hardware.</param>
    /// <param name="supportedModulationSchemes">The modulation schemes supported by the hardware.</param>
    /// <param name="controlPoint">The control point to use to connect to the device.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    public TunerDri(DeviceDescriptor descriptor, string tunerInstanceId, string productInstanceId, BroadcastStandard supportedBroadcastStandards, ICollection<TunerModulation> supportedModulationSchemes, UPnPControlPoint controlPoint, ITunerInternal streamTuner)
      : base(descriptor.FriendlyName, descriptor.DeviceUUID, tunerInstanceId, productInstanceId, supportedBroadcastStandards)
    {
      ChannelStream streamChannel = new ChannelStream();
      streamChannel.Url = "rtp://127.0.0.1";
      if (streamTuner == null || !streamTuner.CanTune(streamChannel))
      {
        throw new TvException("Internal tuner implementation is not usable.");
      }

      _descriptor = descriptor;
      _controlPoint = controlPoint;
      _supportedModulationSchemes = supportedModulationSchemes;
      _streamTuner = new TunerInternalWrapper(streamTuner);
      _isCetonDevice = descriptor.FriendlyName.Contains("Ceton");
      _caMenuHandler = new CableCardMmiHandler(EnterMenu, CloseDialog);

      _localIpAddress = descriptor.RootDescriptor.SSDPRootEntry.PreferredLink.Endpoint.EndPointIPAddress;
      _serverIpAddress = new Uri(descriptor.RootDescriptor.SSDPRootEntry.PreferredLink.DescriptionLocation).Host;

      // CableCARD tuners are limited to one channel per tuner, even for
      // non-encrypted channels:
      // The DRIT SHALL output the selected program content as a single program
      // MPEG-TS in RTP packets according to [RTSP] and [RTP].
      // - OpenCable DRI I04 specification, 10 September 2010
      _supportsSubChannels = false;
    }

    ~TunerDri()
    {
      Dispose(false);
    }

    #endregion

    private void ReadDeviceInfo()
    {
      try
      {
        this.LogDebug("DRI CableCARD: diagnostic parameters...");
        string value = string.Empty;
        bool isVolatile = false;
        foreach (DiagParameterDri p in DiagParameterDri.Values)
        {
          _serviceDiag.GetParameter(p, out value, out isVolatile);
          this.LogDebug("  {0}{1} = {2}", p.ToString(), isVolatile ? " [volatile]" : string.Empty, value);
        }
        if (_isCetonDevice)
        {
          this.LogDebug("DRI CableCARD: Ceton-specific diagnostic parameters...");
          foreach (DiagParameterCeton p in DiagParameterCeton.Values)
          {
            _serviceDiag.GetParameter(p, out value, out isVolatile);
            this.LogDebug("  {0}{1} = {2}", p.ToString(), isVolatile ? " [volatile]" : string.Empty, value);
          }
        }

        _canPause = false;
        IList<AvTransportAction> actions;
        if (_serviceAvTransport.GetCurrentTransportActions((uint)_avTransportId, out actions))
        {
          this.LogDebug("DRI CableCARD: supported AV transport actions = {0}", string.Join(", ", actions.Select(x => x.ToString())));
          _canPause = actions.Contains(AvTransportAction.Pause);
        }

        uint trackCount = 0;
        string mediaDuration = string.Empty;
        string currentUri = string.Empty;
        string currentUriMetaData = string.Empty;
        string nextUri = string.Empty;
        string nextUriMetaData = string.Empty;
        AvTransportStorageMedium playMedium = AvTransportStorageMedium.Unknown;
        AvTransportStorageMedium recordMedium = AvTransportStorageMedium.Unknown;
        AvTransportRecordMediumWriteStatus writeStatus = AvTransportRecordMediumWriteStatus.NotImplemented;
        _serviceAvTransport.GetMediaInfo((uint)_avTransportId, out trackCount, out mediaDuration, out currentUri,
          out currentUriMetaData, out nextUri, out nextUriMetaData, out playMedium, out recordMedium, out writeStatus);
        this.LogDebug("DRI CableCARD: current URI = {0}", currentUri);
        _rtspUri = currentUri;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI CableCARD: failed to read device info");
        throw;
      }
    }

    #region tuning

    private bool IsTunerInUse()
    {
      if (_gotTunerControl)
      {
        return false;
      }
      AvTransportStatus transportStatus = AvTransportStatus.Ok;
      string speed = string.Empty;
      _serviceAvTransport.GetTransportInfo((uint)_avTransportId, out _transportState, out transportStatus, out speed);
      if (transportStatus != AvTransportStatus.Ok)
      {
        this.LogWarn("DRI CableCARD: unexpected transport status {0}", transportStatus);
      }
      if (_transportState == AvTransportState.Stopped)
      {
        return false;
      }
      return true;
    }

    private void TuneWithCableCard(IChannel channel, out bool isSignalLocked)
    {
      _currentFrequency = -1;
      try
      {
        uint virtualChannelNumber;
        if (uint.TryParse(channel.LogicalChannelNumber, out virtualChannelNumber))
        {
          this.LogDebug("DRI CableCARD: tuning by channel number");
          _serviceCas.SetChannel(virtualChannelNumber, 0, CasCaptureMode.Live, out isSignalLocked);
        }
        else
        {
          int sourceId = 0;
          ChannelAtsc atscChannel = channel as ChannelAtsc;
          if (atscChannel != null)
          {
            sourceId = atscChannel.SourceId;
          }
          else
          {
            ChannelScte scteChannel = channel as ChannelScte;
            sourceId = scteChannel.SourceId;
          }
          if (sourceId > 0)
          {
            this.LogDebug("DRI CableCARD: tuning by source ID");
            _serviceCas.SetChannel(0, (uint)sourceId, CasCaptureMode.Live, out isSignalLocked);
          }
          else
          {
            throw new TvException("Not able to tune using CableCARD without either channel number or source ID.");
          }
        }

        // When switched digital video (SDV) is active - ie. the tuner uses a
        // tuning adaptor/resolver to ask the cable system which frequency
        // and program number to tune - we have to ask the tuner what the
        // correct program number is.
        // Note that SiliconDust and Hauppauge tuners actually deliver a TS
        // with a single program. For them it would be enough to set the
        // program number to 0. However Ceton tuners deliver the PAT and PMT
        // for all the programs in the full transport stream, only excluding
        // extra video and audio streams. Therefore we have to do this...
        ChannelMpeg2Base mpeg2Channel = channel as ChannelMpeg2Base;
        if (_isCetonDevice || mpeg2Channel.ProgramNumber == 0)
        {
          DateTime start = DateTime.Now;
          while ((DateTime.Now - start).TotalMilliseconds < _programNumberTimeOut)
          {
            try
            {
              mpeg2Channel.ProgramNumber = (int)_serviceMux.QueryStateVariable("ProgramNumber");
              if (mpeg2Channel.ProgramNumber != 0)
              {
                this.LogDebug("DRI CableCARD: actual program number is {0}", mpeg2Channel.ProgramNumber);
                break;
              }
              System.Threading.Thread.Sleep(20);
            }
            catch (Exception ex)
            {
              this.LogError(ex, "DRI CableCARD: failed to get actual program number");
              break;
            }
          }
        }
      }
      catch (UPnPException ex)
      {
        UPnPRemoteException rex = ex.InnerException as UPnPRemoteException;
        if (rex != null && rex.Error.ErrorDescription.Equals("Tuner In Use"))
        {
          this.LogInfo("DRI CableCARD: some other application or device is currently using the tuner");
          _gotTunerControl = false;
        }
        throw;
      }
    }

    private void TuneWithoutCableCard(IChannel channel, out bool isSignalLocked)
    {
      int frequency;
      IList<TunerModulation> modulationSchemes = new List<TunerModulation>(2);
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (atscChannel != null)
      {
        this.LogDebug("DRI CableCARD: tuning ATSC by frequency");
        frequency = atscChannel.Frequency;
        if (atscChannel.ModulationScheme == ModulationSchemeVsb.Vsb8 && _supportedModulationSchemes.Contains(TunerModulation.Vsb8))
        {
          modulationSchemes.Add(TunerModulation.Vsb8);
          modulationSchemes.Add(TunerModulation.Vsb8_2);
        }
        else
        {
          this.LogWarn("DRI CableCARD: unsupported ATSC modulation scheme {0}, allowing tuner to use any supported modulation scheme", atscChannel.ModulationScheme);
          modulationSchemes.Add(TunerModulation.All);
        }
      }
      else
      {
        this.LogDebug("DRI CableCARD: tuning SCTE by frequency");
        ChannelScte scteChannel = channel as ChannelScte;
        frequency = scteChannel.Frequency;
        if (scteChannel.ModulationScheme == ModulationSchemeQam.Qam256 && _supportedModulationSchemes.Contains(TunerModulation.Qam256))
        {
          modulationSchemes.Add(TunerModulation.Qam256);
          modulationSchemes.Add(TunerModulation.Qam256_2);
        }
        else if (scteChannel.ModulationScheme == ModulationSchemeQam.Qam64 && _supportedModulationSchemes.Contains(TunerModulation.Qam64))
        {
          modulationSchemes.Add(TunerModulation.Qam64);
          modulationSchemes.Add(TunerModulation.Qam64_2);
        }
        else
        {
          this.LogWarn("DRI CableCARD: unsupported SCTE modulation scheme {0}, allowing tuner to use any supported modulation scheme", scteChannel.ModulationScheme);
          modulationSchemes.Add(TunerModulation.All);
        }
      }

      _currentFrequency = -1;
      uint currentFrequency;
      TunerModulation currentModulation;
      _serviceTuner.SetTunerParameters((uint)frequency, modulationSchemes, out currentFrequency, out currentModulation, out isSignalLocked);
    }

    #endregion

    #region RTSP

    private void StartStreaming()
    {
      if (!string.IsNullOrEmpty(_rtspSessionId))
      {
        return;
      }
      this.LogDebug("DRI CableCARD: start streaming");
      if (_rtspClient == null)
      {
        Uri uri = new Uri(_rtspUri);
        if (uri.IsDefaultPort)
        {
          _rtspClient = new Rtsp.RtspClient(uri.Host);
        }
        else
        {
          _rtspClient = new Rtsp.RtspClient(uri.Host, uri.Port);
        }
      }

      // Find a free port for receiving the RTP stream.
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
      for (int port = 40000; port <= 65534; port += 2)  // By convention the port should be even.
      {
        if (!usedPorts.Contains(port))
        {
          rtpClientPort = port;
          break;
        }
      }
      this.LogDebug("DRI CableCARD: send RTSP SETUP, RTP client port = {0}", rtpClientPort);
      RtspRequest request = new RtspRequest(RtspMethod.Setup, _rtspUri);
      request.Headers.Add("Transport", string.Format("RTP/AVP;unicast;client_port={0}-{1}", rtpClientPort, rtpClientPort + 1));
      Rtsp.RtspResponse response;
      if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
      {
        throw new TvException("Failed to start streaming, non-OK RTSP SETUP status code {0} {1}.", response.StatusCode, response.ReasonPhrase);
      }

      if (!response.Headers.TryGetValue("Session", out _rtspSessionId))
      {
        throw new TvException("Failed to start streaming, not able to find session header in RTSP SETUP response.");
      }

      bool foundRtpTransport = false;
      string rtpServerPort = null;
      string transportHeader;
      if (!response.Headers.TryGetValue("Transport", out transportHeader))
      {
        throw new TvException("Failed to start streaming, not able to find transport header in RTSP SETUP response.");
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
              if (!ports[0].Equals(rtpClientPort))
              {
                this.LogWarn("DRI CableCARD: server specified RTP client port {0} instead of {1}", ports[0], rtpClientPort);
              }
              rtpClientPort = int.Parse(ports[0]);
            }
          }
        }
      }
      if (!foundRtpTransport)
      {
        throw new TvException("Failed to start streaming, not able to find RTP transport details in RTSP SETUP response transport header \"{0}\".", transportHeader);
      }
      ChannelStream streamChannel = new ChannelStream();
      if (string.IsNullOrEmpty(rtpServerPort) || rtpServerPort.Equals("0"))
      {
        streamChannel.Url = string.Format("rtp://{0}@{1}:{2}", _serverIpAddress, _localIpAddress, rtpClientPort);
      }
      else
      {
        streamChannel.Url = string.Format("rtp://{0}:{1}@{2}:{3}", _serverIpAddress, rtpServerPort, _localIpAddress, rtpClientPort);
      }
      this.LogDebug("DRI CableCARD: RTSP SETUP response okay, session ID = {0}, RTP URL = {1}", _rtspSessionId, streamChannel.Url);

      this.LogDebug("DRI CableCARD: send RTSP PLAY");
      request = new RtspRequest(RtspMethod.Play, _rtspUri);
      request.Headers.Add("Session", _rtspSessionId);
      if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
      {
        throw new TvException("Failed to start streaming, non-OK RTSP PLAY status code {0} {1}.", response.StatusCode, response.ReasonPhrase);
      }
      this.LogDebug("DRI CableCARD: RTSP PLAY response okay");

      // Copy the other channel parameters from the original channel.
      streamChannel.IsEncrypted = _currentTuningDetail.IsEncrypted;
      streamChannel.LogicalChannelNumber = _currentTuningDetail.LogicalChannelNumber;
      streamChannel.MediaType = _currentTuningDetail.MediaType;
      streamChannel.Name = _currentTuningDetail.Name;
      streamChannel.OriginalNetworkId = -1;

      ChannelMpeg2Base mpeg2Channel = _currentTuningDetail as ChannelMpeg2Base;
      if (mpeg2Channel != null)
      {
        streamChannel.PmtPid = mpeg2Channel.PmtPid;
        streamChannel.Provider = mpeg2Channel.Provider;
        streamChannel.ProgramNumber = mpeg2Channel.ProgramNumber;
        streamChannel.TransportStreamId = mpeg2Channel.TransportStreamId;
      }

      _streamTuner.PerformTuning(streamChannel);
    }

    private void StopStreaming()
    {
      if (_rtspClient == null || string.IsNullOrEmpty(_rtspSessionId))
      {
        return;
      }
      this.LogDebug("DRI CableCARD: stop streaming");
      RtspRequest request = new RtspRequest(RtspMethod.Teardown, _rtspUri);
      request.Headers.Add("Session", _rtspSessionId);
      RtspResponse response;
      if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
      {
        throw new TvException("Failed to stop streaming, non-OK RTSP TEARDOWN status code {0} {1}.", response.StatusCode, response.ReasonPhrase);
      }
      _rtspSessionId = string.Empty;
    }

    #endregion

    #region UPnP

    /// <summary>
    /// Handle UPnP evented state variable changes.
    /// </summary>
    /// <param name="stateVariable">The state variable that has changed.</param>
    /// <param name="newValue">The new value of the state variable.</param>
    private void OnStateVariableChanged(CpStateVariable stateVariable, object newValue)
    {
      try
      {
        if (stateVariable.Name.Equals("PCRLock") || stateVariable.Name.Equals("Lock"))
        {
          if (!(bool)newValue)
          {
            this.LogInfo("DRI CableCARD: tuner {0} {1} update, not locked", TunerId, stateVariable.Name);
          }
          if (InternalChannelScanningInterface == null || !InternalChannelScanningInterface.IsScanning)
          {
            _isSignalLocked = (bool)newValue;
          }
        }
        else if (stateVariable.Name.Equals("TableSection"))
        {
          byte[] section = (byte[])newValue;
          if (section != null && section.Length > 3)
          {
            byte tableId = section[2];
            if (tableId == 0xc2 || tableId == 0xc3 || tableId == 0xc4 || tableId == 0xc8 || tableId == 0xc9)
            {
              ChannelScannerDri scanner = InternalChannelScanningInterface as ChannelScannerDri;
              if (scanner != null)
              {
                scanner.OnTableSection(section);
              }
            }
            else if ((tableId != 0xc5 || section.Length != 16) && tableId != 0xfd) // not a standard system time or stuffing table
            {
              this.LogDebug("DRI CableCARD: unhandled table section, table ID = {0}", tableId);
              Dump.DumpBinary(section);
            }
          }
        }
        else if (stateVariable.Name.Equals("CardStatus"))
        {
          CasCardStatus oldStatus = _cardStatus;
          _cardStatus = (CasCardStatus)(string)newValue;
          if (oldStatus != _cardStatus)
          {
            this.LogInfo("DRI CableCARD: tuner {0} CableCARD status update, old status = {1}, new status = {2}", TunerId, oldStatus, _cardStatus);
          }
        }
        else if (stateVariable.Name.Equals("CardMessage"))
        {
          if (!string.IsNullOrEmpty(newValue.ToString()))
          {
            this.LogInfo("DRI CableCARD: tuner {0} received message from the CableCARD, current status = {1}, message = {2}", TunerId, _cardStatus, newValue);
          }
        }
        else if (stateVariable.Name.Equals("MMIMessage"))
        {
          lock (_caMenuCallBackLock)
          {
            _caMenuHandler.HandleDialog((byte[])newValue, _caMenuCallBack);
          }
        }
        else if (stateVariable.Name.Equals("DescramblingStatus"))
        {
          CasDescramblingStatus oldStatus = _descramblingStatus;
          _descramblingStatus = (CasDescramblingStatus)(string)newValue;
          if (oldStatus != _descramblingStatus)
          {
            this.LogInfo("DRI CableCARD: tuner {0} descrambling status update, old status = {1}, new status = {2}", TunerId, oldStatus, _descramblingStatus);
          }
        }
        else if (stateVariable.Name.Equals("DrmPairingStatus"))
        {
          SecurityPairingStatus oldStatus = _pairingStatus;
          _pairingStatus = (SecurityPairingStatus)(string)newValue;
          if (oldStatus != _pairingStatus)
          {
            this.LogInfo("DRI CableCARD: tuner {0} pairing status update, old status = {1}, new status = {2}", TunerId, oldStatus, _pairingStatus);
          }
        }
        else
        {
          string unqualifiedServiceName = stateVariable.ParentService.ServiceId.Substring(stateVariable.ParentService.ServiceId.LastIndexOf(":") + 1);
          this.LogDebug("DRI CableCARD: tuner {0} state variable {1} for service {2} changed to {3}", TunerId, stateVariable.Name, unqualifiedServiceName, newValue ?? "[null]");
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI CableCARD: tuner {0} failed to handle state variable change", TunerId);
      }
    }

    /// <summary>
    /// Handle UPnP state variable event subscription failure notifications.
    /// </summary>
    /// <param name="service">The service that the subscription relates to.</param>
    /// <param name="error">Failure details.</param>
    private void OnEventSubscriptionFailed(CpService service, UPnPError error)
    {
      string unqualifiedServiceName = service.ServiceId.Substring(service.ServiceId.LastIndexOf(":") + 1);
      this.LogError("DRI CableCARD: tuner {0} failed to subscribe to state variable events for service {1}, code = {2}, description = {3}", TunerId, unqualifiedServiceName, error.ErrorCode, error.ErrorDescription);
    }

    /// <summary>
    /// Resolve a DRI-specific data type.
    /// </summary>
    /// <param name="dataTypeName">The fully qualified name of the data type.</param>
    /// <param name="dataType">The data type.</param>
    /// <returns><c>true</c> if the data type has been resolved, otherwise <c>false</c></returns>
    public static bool ResolveDataType(string dataTypeName, out UPnPExtendedDataType dataType)
    {
      // All the DRI variable types are standard, so we don't expect to be asked to resolve any data types.
      Log.Error("DRI: resolve data type not supported, type name = {0}", dataTypeName);
      dataType = null;
      return true;
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
      // Use the PMT time out as a limit on the program number query time.
      _programNumberTimeOut = SettingsManagement.GetValue("timeOutProgramMapTable", 5000);

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
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading()
    {
      this.LogDebug("DRI CableCARD: perform loading");

      bool useKeepAlive = !_isCetonDevice;
      this.LogInfo("DRI CableCARD: connect to device, keep-alive = {0}", useKeepAlive);
      _deviceConnection = _controlPoint.Connect(_descriptor.RootDescriptor, _descriptor.DeviceUUID, ResolveDataType, useKeepAlive);

      // services
      this.LogDebug("DRI CableCARD: setup services");
      _serviceTuner = new ServiceTuner(_deviceConnection.Device);
      _serviceFdc = new ServiceFdc(_deviceConnection.Device);
      _serviceAux = new ServiceAux(_deviceConnection.Device);
      _serviceEncoder = new ServiceEncoder(_deviceConnection.Device);
      _serviceCas = new ServiceCas(_deviceConnection.Device);
      _serviceMux = new ServiceMux(_deviceConnection.Device);
      _serviceSecurity = new ServiceSecurity(_deviceConnection.Device);
      _serviceDiag = new ServiceDiag(_deviceConnection.Device);
      _serviceUserActivity = new ServiceUserActivity(_deviceConnection.Device);
      _serviceAvTransport = new ServiceAvTransport(_deviceConnection.Device);
      _serviceConnectionManager = new ServiceConnectionManager(_deviceConnection.Device);

      this.LogDebug("DRI CableCARD: subscribe services");
      _stateVariableDelegate = new StateVariableChangedDlgt(OnStateVariableChanged);
      _eventSubscriptionDelegate = new EventSubscriptionFailedDlgt(OnEventSubscriptionFailed);
      _serviceTuner.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceFdc.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceAux.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceEncoder.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceCas.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceSecurity.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceAvTransport.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceConnectionManager.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);

      int rcsId = -1;
      _serviceConnectionManager.PrepareForConnection(string.Empty, string.Empty, -1, ConnectionDirection.Output, out _connectionId, out _avTransportId, out rcsId);
      this.LogDebug("DRI CableCARD: connected, connection ID = {0}, AV transport ID = {1}", _connectionId, _avTransportId);

      ReadDeviceInfo();
      TunerExtensionLoader loader = new TunerExtensionLoader();
      IList<ITunerExtension> extensions = loader.Load(this, _descriptor);

      // Add the stream tuner extensions to our extensions, but don't re-sort
      // by priority afterwards. This ensures that our extensions are always
      // given first consideration.
      IList<ITunerExtension> streamTunerExtensions = _streamTuner.PerformLoading();
      foreach (ITunerExtension e in streamTunerExtensions)
      {
        extensions.Add(e);
      }

      _channelScanner = new ChannelScannerDri(this, _serverIpAddress, _serviceFdc.RequestTables);
      return extensions;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformSetTunerState(TunerState state, bool isFinalising = false)
    {
      this.LogDebug("DRI CableCARD: perform set tuner state");
      if (isFinalising)
      {
        return;
      }

      ChannelAtsc atscChannel = _currentTuningDetail as ChannelAtsc;
      if (InternalChannelScanningInterface != null && InternalChannelScanningInterface.IsScanning && atscChannel != null && atscChannel.PhysicalChannelNumber == 0)
      {
        // Scanning with a CableCARD doesn't require streaming. The channel
        // info is evented via the forward data channel service.
        return;
      }

      if (_serviceAvTransport != null && _gotTunerControl)
      {
        if (state == TunerState.Stopped || state == TunerState.Paused)
        {
          if (state == TunerState.Stopped)
          {
            _serviceAvTransport.Stop((uint)_avTransportId);
            _transportState = AvTransportState.Stopped;
            _gotTunerControl = false;
          }
          else
          {
            _serviceAvTransport.Pause((uint)_avTransportId);
            _transportState = AvTransportState.PausedPlayback;
          }
          StopStreaming();
        }
        else if (state == TunerState.Started)
        {
          _serviceAvTransport.Play((uint)_avTransportId, "1");
          _transportState = AvTransportState.Playing;
          StartStreaming();
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
      this.LogDebug("DRI CableCARD: perform unloading");
      if (isFinalising)
      {
        return;
      }

      _channelScanner = null;
      if (_serviceTuner != null)
      {
        _serviceTuner.Dispose();
        _serviceTuner = null;
      }
      if (_serviceFdc != null)
      {
        _serviceFdc.Dispose();
        _serviceFdc = null;
      }
      if (_serviceAux != null)
      {
        _serviceAux.Dispose();
        _serviceAux = null;
      }
      if (_serviceEncoder != null)
      {
        _serviceEncoder.Dispose();
        _serviceEncoder = null;
      }
      if (_serviceCas != null)
      {
        _serviceCas.Dispose();
        _serviceCas = null;
      }
      if (_serviceMux != null)
      {
        _serviceMux.Dispose();
        _serviceMux = null;
      }
      if (_serviceSecurity != null)
      {
        _serviceSecurity.Dispose();
        _serviceSecurity = null;
      }
      if (_serviceDiag != null)
      {
        _serviceDiag.Dispose();
        _serviceDiag = null;
      }
      if (_serviceUserActivity != null)
      {
        _serviceUserActivity.Dispose();
        _serviceUserActivity = null;
      }
      if (_serviceAvTransport != null)
      {
        _serviceAvTransport.Dispose();
        _serviceAvTransport = null;
      }
      if (_serviceConnectionManager != null)
      {
        // This call can fail if the connection to the tuner is down. We must
        // not allow failure to cause further problems.
        try
        {
          _serviceConnectionManager.ConnectionComplete(_connectionId);
        }
        catch
        {
          this.LogWarn("DRI CableCARD: failed to complete connection manager connection");
        }
        _connectionId = CONNECTION_ID_NOT_CONNECTED;
        _serviceConnectionManager.Dispose();
        _serviceConnectionManager = null;
      }

      if (_deviceConnection != null)
      {
        _deviceConnection.Disconnect();
        _deviceConnection.Dispose();
        _deviceConnection = null;
      }

      _streamTuner.PerformUnloading();
    }

    #endregion

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if (!base.CanTune(channel))
      {
        return false;
      }

      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (atscChannel != null && atscChannel.ModulationScheme == ModulationSchemeVsb.Vsb8 && _supportedModulationSchemes.Contains(TunerModulation.Vsb8))
      {
        return true;
      }

      ChannelScte scteChannel = channel as ChannelScte;
      if (scteChannel == null)
      {
        return false;
      }

      // Tuning physically possible?
      if (
        scteChannel.ModulationScheme == ModulationSchemeQam.Automatic ||  // switched digital video - modulation unknown, assume supported
        (scteChannel.ModulationScheme == ModulationSchemeQam.Qam64 && _supportedModulationSchemes.Contains(TunerModulation.Qam64)) ||
        (scteChannel.ModulationScheme == ModulationSchemeQam.Qam256 && _supportedModulationSchemes.Contains(TunerModulation.Qam256))
      )
      {
        // Tune request can be assembled and could succeed?
        uint virtualChannelNumber;
        if (
          // SCTE channels can be tuned with or without a CableCARD as long as
          // the channel frequency is specified (ie. not switched digital
          // video).
          (!scteChannel.IsEncrypted && scteChannel.Frequency > 0) ||
          // ...otherwise, for encrypted and/or SDV channels:
          (
            // CableCARD must be present...
            (_connectionId == CONNECTION_ID_NOT_CONNECTED || _cardStatus == CasCardStatus.Inserted) &&
            // ...and we must have a valid source ID and/or virtual channel number.
            (
              scteChannel.SourceId > 0 ||
              (
                uint.TryParse(channel.LogicalChannelNumber, out virtualChannelNumber) &&
                (
                  virtualChannelNumber > 0 ||
                  // Special case: scanning using the CableCARD.
                  (InternalChannelScanningInterface != null && InternalChannelScanningInterface.IsScanning)
                )
              )
            )
          )
        )
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("DRI CableCARD: perform tuning");
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      ChannelScte scteChannel = channel as ChannelScte;
      if (atscChannel == null && scteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      // Is a CableCARD required? (CableCARD scan, encrypted channel, or SDV channel)
      IChannelPhysical physicalChannel = channel as IChannelPhysical;
      bool isSignalLocked;
      bool isCableCardRequired = false;
      if (channel.IsEncrypted || physicalChannel.Frequency <= 0)
      {
        if (_cardStatus != CasCardStatus.Inserted)
        {
          throw new TvException("CableCARD not available, current status is {0}.", _cardStatus);
        }

        // We only need the OOB tuner when scanning with the CableCARD. We
        // don't even have to start a stream so we don't interrupt other
        // applications. Just check that the OOB tuner is locked.
        if (InternalChannelScanningInterface.IsScanning && physicalChannel.Frequency <= 0)
        {
          this.LogDebug("DRI CableCARD: check out-of-band tuner lock");
          uint bitrate = 0;
          uint frequency = 0;
          bool spectrumInversion = false;
          IList<ushort> pids;
          _serviceFdc.GetFdcStatus(out bitrate, out isSignalLocked, out frequency, out spectrumInversion, out pids);
          _isSignalLocked = isSignalLocked;
          if (isSignalLocked)
          {
            throw new TvExceptionNoSignal(TunerId, channel, "Out-of-band tuner not locked.");
          }
          this.LogDebug("  frequency = {0} kHz", frequency);
          this.LogDebug("  bitrate   = {0} kbps", bitrate);
          this.LogDebug("  PIDs      = {0}", string.Join(", ", pids.Select(x => x.ToString())));
          return;
        }

        isCableCardRequired = true;
      }

      if (IsTunerInUse())
      {
        throw new TvException("Tuner appears to be in use.");
      }
      _gotTunerControl = true;

      // CableCARD tuners don't forget the tuned channel after they're stopped,
      // and they reject tune requests for the current channel.
      if (_currentTuningDetail != null && _currentTuningDetail.IsDifferentTransmitter(channel))
      {
        this.LogDebug("DRI CableCARD: already tuned");
        return;
      }

      // Prefer CableCARD tuning. The other tuning method has not been tested.
      if (isCableCardRequired || _cardStatus == CasCardStatus.Inserted)
      {
        TuneWithCableCard(channel, out isSignalLocked);
      }
      else
      {
        TuneWithoutCableCard(channel, out isSignalLocked);
      }
      _currentTuningDetail = channel;
      _isSignalLocked = isSignalLocked;
    }

    /// <summary>
    /// Allocate a new sub-channel instance.
    /// </summary>
    /// <param name="id">The identifier for the sub-channel.</param>
    /// <returns>the new sub-channel instance</returns>
    public override ISubChannelInternal CreateNewSubChannel(int id)
    {
      return _streamTuner.CreateNewSubChannel(id);
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
      isLocked = false;
      isPresent = false;
      strength = 0;
      quality = 0;
      if (onlyGetLock)
      {
        // When we're asked to only update locked status it means the tuner is
        // trying to lock in on signal. When scanning with the out-of-band
        // tuner, we already checked lock during tuning. In all other cases the
        // signal locked indicator is evented so should be current and usable.
        isLocked = _isSignalLocked;
        return;
      }

      try
      {
        uint frequency = 0;
        if (InternalChannelScanningInterface != null && InternalChannelScanningInterface.IsScanning)
        {
          // If scanning with the out-of-band tuner...
          IChannelPhysical physicalChannel = _currentTuningDetail as IChannelPhysical;
          if (physicalChannel != null && physicalChannel.Frequency <= 0)
          {
            if (_serviceFdc == null)
            {
              return;
            }

            uint bitrate = 0;
            bool spectrumInversion = false;
            IList<ushort> pids;
            _serviceFdc.GetFdcStatus(out bitrate, out isLocked, out frequency, out spectrumInversion, out pids);
            isPresent = _isSignalLocked;

            if (_isCetonDevice)
            {
              string value = string.Empty;
              bool isVolatile = false;
              _serviceDiag.GetParameter(DiagParameterCeton.OobSignalLevel, out value, out isVolatile);
              // Example: "0.8 dBmV". Assumed range -25..25.
              Match m = REGEX_SIGNAL_INFO.Match(value);
              if (m.Success)
              {
                strength = (int)(double.Parse(value) * 2) + 50;
                if (strength < 0)
                {
                  strength = 0;
                }
                else if (strength > 100)
                {
                  strength = 100;
                }
              }
              else
              {
                this.LogWarn("DRI CableCARD: failed to interpret out-of-band signal level {0}", value);
                strength = 0;
              }
              // Example: "31.9 dB". Use value as-is.
              _serviceDiag.GetParameter(DiagParameterCeton.OobSnr, out value, out isVolatile);
              m = REGEX_SIGNAL_INFO.Match(value);
              if (m.Success)
              {
                quality = (int)double.Parse(value);
                if (quality < 0)
                {
                  quality = 0;
                }
                else if (quality > 100)
                {
                  quality = 100;
                }
              }
              else
              {
                this.LogWarn("DRI CableCARD: failed to interpret out-of-band signal-to-noise ratio {0}", value);
                quality = 0;
              }
            }
            else if (isLocked)
            {
              strength = 100;
              quality = 100;
            }
            return;
          }
        }

        // Otherwise...
        if (_serviceTuner == null)
        {
          return;
        }

        TunerModulation modulation = TunerModulation.All;
        uint snr;
        _serviceTuner.GetTunerParameters(out isPresent, out frequency, out modulation, out isLocked, out strength, out snr);
        _isSignalLocked = isLocked;
        strength = (strength * 2) + 50;
        quality = (int)snr;
        if (_currentFrequency == -1)
        {
          this.LogDebug("DRI CableCARD: current tuning details, frequency = {0} kHz, modulation = {1}", frequency, modulation);
          _currentFrequency = (int)frequency;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI CableCARD: exception updating signal status");
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

    #endregion

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBack">The call back delegate.</param>
    void IConditionalAccessMenuActions.SetCallBack(IConditionalAccessMenuCallBack callBack)
    {
      lock (_caMenuCallBackLock)
      {
        _caMenuCallBack = callBack;
      }
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Enter()
    {
      return EnterMenu();
    }

    private bool EnterMenu()
    {
      this.LogDebug("DRI CableCARD: enter menu");

      if (_serviceCas == null)
      {
        this.LogWarn("DRI CableCARD: not initialised or interface not supported");
        return false;
      }

      if (_cardStatus == CasCardStatus.Removed)
      {
        this.LogError("DRI CableCARD: CableCARD not present");
        return false;
      }

      string manufacturer = string.Empty;
      try
      {
        CasCardStatus status = CasCardStatus.Removed;
        string version = string.Empty;
        bool isDst = false;
        uint eaLocationCode = 0;
        byte ratingRegion = 0;
        int timeZone = 0;
        _serviceCas.GetCardStatus(out status, out manufacturer, out version, out isDst, out eaLocationCode, out ratingRegion, out timeZone);
        this.LogDebug("  status        = {0}", status.ToString());
        this.LogDebug("  manufacturer  = {0}", manufacturer);
        this.LogDebug("  version       = {0}", version);
        this.LogDebug("  time zone     = {0}", timeZone);
        this.LogDebug("  DST           = {0}", isDst);
        this.LogDebug("  EA loc. code  = {0}", eaLocationCode);  // EA = emergency alert
        this.LogDebug("  rating region = {0}", ratingRegion);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI CableCARD: failed to read CableCARD status");
        return false;
      }

      byte[] list = null;
      try
      {
        list = (byte[])_serviceCas.QueryStateVariable("ApplicationList");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI CableCARD: failed to get application list");
        return false;
      }

      lock (_caMenuCallBackLock)
      {
        return _caMenuHandler.EnterMenu(manufacturer, string.Empty, string.Empty, list, _caMenuCallBack);
      }
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Close()
    {
      this.LogDebug("DRI CableCARD: close menu");
      CloseDialog(0);
      return true;
    }

    /// <summary>
    /// Inform the CableCARD that the user has closed a dialog.
    /// </summary>
    /// <param name="dialogNumber">The identifier for the dialog that has been closed.</param>
    /// <returns><c>true</c> if the CableCARD is successfully notified, otherwise <c>false</c></returns>
    private bool CloseDialog(byte dialogNumber)
    {
      this.LogDebug("DRI CableCARD: close dialog, dialog number = {0}", dialogNumber);

      if (_serviceCas == null)
      {
        this.LogWarn("DRI CableCARD: not initialised or interface not supported");
        return false;
      }

      try
      {
        _serviceCas.NotifyMmiClose(dialogNumber);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI CableCARD: failed to notify the CableCARD that the MMI dialog has been closed");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      this.LogDebug("DRI CableCARD: select menu entry, choice = {0}", choice);
      lock (_caMenuCallBackLock)
      {
        return _caMenuHandler.SelectMenuEntry(choice, _caMenuCallBack);
      }
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.AnswerEnquiry(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = string.Empty;
      }
      this.LogDebug("DRI CableCARD: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      // TODO I don't know how to implement this yet.
      return true;
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