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
  /// <summary>
  /// A base implementation of <see cref="ITuner"/> for tuners which implement
  /// the CableLabs/OpenCable Digital Receiver Interface.
  /// </summary>
  internal abstract class TunerDriBase : TunerBase, IConditionalAccessMenuActions, IMpeg2PidFilter
  {
    #region constants

    private const int VALUE_NOT_SET = -1;

    #endregion

    #region variables

    // UPnP
    private DeviceDescriptor _descriptor = null;
    private UPnPControlPoint _controlPoint = null;
    protected DeviceConnection _deviceConnection = null;
    protected StateVariableChangedDlgt _stateVariableChangeDelegate = null;
    protected EventSubscriptionFailedDlgt _eventSubscriptionFailDelegate = null;

    // services
    private ServiceTuner _serviceTuner = null;                // [physical] tuning, scanning
    private ServiceCas _serviceCas = null;                    // conditional access
    private ServiceMux _serviceMux = null;                    // PID filtering
    private ServiceSecurity _serviceSecurity = null;          // DRM system
    protected ServiceDiag _serviceDiag = null;                // name/value pair info
    private ServiceUserActivity _serviceUserActivity = null;  // activity notification
    private ServiceAvTransport _serviceAvTransport = null;    // streaming control
    private ServiceConnectionManager _serviceConnectionManager = null;

    // RTSP/RTP
    private string _rtspUri = string.Empty;
    private Rtsp.RtspClient _rtspClient = null;
    private string _rtspSessionId = string.Empty;
    private IPAddress _localIpAddress = null;
    protected string _serverIpAddress = string.Empty;
    protected ChannelStream _streamChannel = new ChannelStream();
    private int _rtpPortMinimum = 49152;
    private int _rtpPortMaximum = 65535;

    // UPnP AV
    private int _connectionId = VALUE_NOT_SET;
    private int _avTransportId = VALUE_NOT_SET;

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

    // PID filter variables
    private HashSet<ushort> _pidFilterPids = new HashSet<ushort>();
    private HashSet<ushort> _pidFilterPidsToRemove = new HashSet<ushort>();
    private HashSet<ushort> _pidFilterPidsToAdd = new HashSet<ushort>();
    private bool _isPidFilterDisabled = true;

    private volatile bool _isSignalLocked = false;
    private int _currentFrequency = VALUE_NOT_SET;
    private int _currentVirtualChannelNumber = VALUE_NOT_SET;
    private int _currentSourceId = VALUE_NOT_SET;
    protected readonly TunerVendor _vendor = TunerVendor.Unknown;
    private StreamFormat _streamFormat = StreamFormat.Default;
    private bool _canPause = false;

    /// <summary>
    /// Internal stream tuner, used to receive the RTP stream. This allows us
    /// to decouple the stream tuning implementation (eg. DirectShow) from the
    /// DRI implementation.
    /// </summary>
    private ITunerInternal _streamTuner = null;

    /// <summary>
    /// The tuner's sub-channel manager.
    /// </summary>
    private ISubChannelManager _subChannelManager = null;

    /// <summary>
    /// The tuner's channel scanning interface.
    /// </summary>
    protected IChannelScannerInternal _channelScanner = null;

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDriBase"/> class.
    /// </summary>
    /// <param name="descriptor">The UPnP device description.</param>
    /// <param name="externalId">The tuner's unique external identifier.</param>
    /// <param name="tunerInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single tuner.</param>
    /// <param name="productInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single product.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the hardware.</param>
    /// <param name="controlPoint">The control point to use to connect to the device.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    public TunerDriBase(DeviceDescriptor descriptor, string externalId, string tunerInstanceId, string productInstanceId, BroadcastStandard supportedBroadcastStandards, UPnPControlPoint controlPoint, ITunerInternal streamTuner)
      : base(descriptor.FriendlyName, externalId, tunerInstanceId, productInstanceId, supportedBroadcastStandards)
    {
      ChannelStream streamChannel = new ChannelStream();
      streamChannel.Url = "rtp://127.0.0.1";
      if (streamTuner == null || !streamTuner.CanTune(streamChannel))
      {
        throw new TvException("Internal tuner implementation is not usable.");
      }

      _descriptor = descriptor;
      _controlPoint = controlPoint;
      _streamTuner = new TunerInternalWrapper(streamTuner);
      _caMenuHandler = new CableCardMmiHandler(EnterMenu, CloseDialog);

      _localIpAddress = descriptor.RootDescriptor.SSDPRootEntry.PreferredLink.Endpoint.EndPointIPAddress;
      _serverIpAddress = new Uri(descriptor.RootDescriptor.SSDPRootEntry.PreferredLink.DescriptionLocation).Host;

      if (descriptor.FriendlyName.StartsWith("ATI"))
      {
        _vendor = TunerVendor.Ati;
      }
      else if (descriptor.FriendlyName.StartsWith("Ceton"))
      {
        _vendor = TunerVendor.Ceton;
      }
      else if (descriptor.FriendlyName.StartsWith("Hauppauge"))
      {
        _vendor = TunerVendor.Hauppauge;
      }
      else if (descriptor.FriendlyName.StartsWith("HDHomeRun"))
      {
        _vendor = TunerVendor.SiliconDust;
      }
    }

    ~TunerDriBase()
    {
      Dispose(false);
    }

    #endregion

    private void ReadDeviceInfo()
    {
      string firmwareVersion = string.Empty;
      try
      {
        this.LogDebug("DRI base: diagnostic parameters...");
        List<string> supportedParameters = new List<string>(DiagParameter.Values.Count);
        try
        {
          string csvParameterNames = (string)_serviceDiag.QueryStateVariable("ParameterList");
          supportedParameters.AddRange(csvParameterNames.Split(','));
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "DRI base: failed to read diagnostic parameter list");
          TunerVendor vendor = _vendor;
          if (vendor == TunerVendor.Unknown)
          {
            vendor = TunerVendor.All;
          }
          foreach (DiagParameter p in DiagParameter.Values)
          {
            if (p.SupportedVendors.HasFlag(vendor))
            {
              supportedParameters.Add(p.ToString());
            }
          }
        }
        string value = string.Empty;
        bool isVolatile = false;
        foreach (string p in supportedParameters)
        {
          _serviceDiag.GetParameter(p, out value, out isVolatile);
          this.LogDebug("  {0}{1} = {2}", p, isVolatile ? " [volatile]" : string.Empty, value);
          if (p == DiagParameter.HostFirmware)
          {
            firmwareVersion = value;
          }
        }

        _canPause = false;
        IList<AvTransportAction> actions;
        if (_serviceAvTransport.GetCurrentTransportActions((uint)_avTransportId, out actions))
        {
          this.LogDebug("DRI base: supported AV transport actions = {0}", string.Join(", ", actions.Select(x => x.ToString())));
          _canPause = actions.Contains(AvTransportAction.Pause);
        }

        // Unfortunately we have to use QueryStateVariable again here. Ceton
        // tuners don't properly support GetPositionInfo(). ATI tuners either
        // don't support GetMediaInfo() and GetPositionInfo(), or require that
        // RTSP SETUP have been successful first.
        _rtspUri = (string)_serviceAvTransport.QueryStateVariable("AVTransportURI");
        this.LogDebug("DRI base: AV transport URI = {0}", _rtspUri);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI base: failed to read device info");
        throw;
      }

      // Check SiliconDust and Hauppauge firmware version to ensure they
      // meet minimum compatibility requirements. Examples:
      // HDHR3-CC 20130708beta1
      // WinTV-DCR-2650 20130117
      // All Ceton firmware seems to be compatible.
      // ATI firmware compatibility is currently unknown.
      if (_vendor == TunerVendor.Hauppauge || _vendor == TunerVendor.SiliconDust)
      {
        Match m = Regex.Match(firmwareVersion, @"20(\d{6})");
        if (!m.Success)
        {
          this.LogWarn("DRI base: failed to check firmware version compatibility, version = {0}", firmwareVersion);
        }
        else
        {
          int versionNumber = int.Parse(m.Groups[1].Captures[0].Value);
          if (
            (_vendor == TunerVendor.SiliconDust && versionNumber < 130708) ||
            (_vendor == TunerVendor.Hauppauge && versionNumber < 140121)
          )
          {
            throw new TvException("Please update the tuner's firmware. The current version - {0} - is not compatible.", firmwareVersion);
          }
        }
      }
    }

    protected DeviceConnection Connect()
    {
      bool useKeepAlive = _vendor == TunerVendor.Hauppauge || _vendor == TunerVendor.SiliconDust;   // not supported by ATI or Ceton
      this.LogInfo("DRI base: connect to device, keep-alive = {0}", useKeepAlive);
      return _controlPoint.Connect(_descriptor.RootDescriptor, _descriptor.DeviceUUID, ResolveDataType, useKeepAlive);
    }

    #region signal

    protected static int SignalStrengthDecibelsToPercentage(double strengthDb)
    {
      // Assumed range -25..25; convert to 0..100.
      int strength = (int)((strengthDb * 2) + 50);
      if (strength > 100)
      {
        return 100;
      }
      if (strength < 0)
      {
        return 0;
      }
      return strength;
    }

    protected static int SignalQualitySnrToPercentage(double qualitySnr)
    {
      // By definition should be greater than zero. Expected/normal values are
      // approximately 30. We can simply use the value as-is.
      return (int)qualitySnr;
    }

    #endregion

    #region tuning

    protected void TuneByVirtualChannelNumber(uint virtualChannelNumber)
    {
      this.LogDebug("DRI base: tune by virtual channel number");
      AttemptToTakeControlOfTuner();

      if (_currentVirtualChannelNumber == virtualChannelNumber)
      {
        this.LogDebug("DRI base: already tuned");
        return;
      }

      try
      {
        bool isSignalLocked;
        _serviceCas.SetChannel(virtualChannelNumber, 0, CasCaptureMode.Live, out isSignalLocked);
        _currentFrequency = VALUE_NOT_SET;
        _currentVirtualChannelNumber = (int)virtualChannelNumber;
        _currentSourceId = VALUE_NOT_SET;
        _isSignalLocked = isSignalLocked;
      }
      catch (Exception ex)
      {
        HandleTuningException(ex);
      }
    }

    protected void TuneBySourceId(int sourceId)
    {
      this.LogDebug("DRI base: tune by source ID");
      AttemptToTakeControlOfTuner();

      if (_currentSourceId == sourceId)
      {
        this.LogDebug("DRI base: already tuned");
        return;
      }

      try
      {
        bool isSignalLocked;
        _serviceCas.SetChannel(0, (uint)sourceId, CasCaptureMode.Live, out isSignalLocked);
        _currentFrequency = VALUE_NOT_SET;
        _currentVirtualChannelNumber = VALUE_NOT_SET;
        _currentSourceId = sourceId;
        _isSignalLocked = isSignalLocked;
      }
      catch (Exception ex)
      {
        HandleTuningException(ex);
      }
    }

    protected void TuneByFrequency(int frequency, TunerModulation modulationScheme)
    {
      this.LogDebug("DRI base: tune by frequency/modulation");
      AttemptToTakeControlOfTuner();

      if (
        _currentFrequency == frequency &&
        _currentVirtualChannelNumber == VALUE_NOT_SET &&
        _currentSourceId == VALUE_NOT_SET
      )
      {
        this.LogDebug("DRI base: already tuned");
        return;
      }

      try
      {
        uint currentFrequency;
        TunerModulation currentModulation;
        bool isSignalLocked;
        _serviceTuner.SetTunerParameters((uint)frequency, new List<TunerModulation> { modulationScheme }, out currentFrequency, out currentModulation, out isSignalLocked);
        _currentFrequency = (int)currentFrequency;
        _currentVirtualChannelNumber = VALUE_NOT_SET;
        _currentSourceId = VALUE_NOT_SET;
        _isSignalLocked = isSignalLocked;
      }
      catch (Exception ex)
      {
        HandleTuningException(ex);
      }
    }

    private void AttemptToTakeControlOfTuner()
    {
      if (_gotTunerControl)
      {
        return;
      }
      AvTransportStatus transportStatus = AvTransportStatus.Ok;
      string speed = string.Empty;
      _serviceAvTransport.GetTransportInfo((uint)_avTransportId, out _transportState, out transportStatus, out speed);
      if (transportStatus != AvTransportStatus.Ok)
      {
        this.LogWarn("DRI base: unexpected transport status {0}", transportStatus);
      }
      if (_transportState == AvTransportState.Stopped)
      {
        _gotTunerControl = true;
        return;
      }
      throw new TvException("Tuner appears to be in use.");
    }

    private void HandleTuningException(Exception ex)
    {
      UPnPException upnpException = ex as UPnPException;
      if (upnpException != null)
      {
        UPnPRemoteException rex = ex.InnerException as UPnPRemoteException;
        if (rex != null && rex.Error.ErrorDescription.Equals("Tuner In Use"))
        {
          this.LogInfo("DRI base: some other application or device is currently using the tuner");
        }
      }
      _gotTunerControl = false;
      throw ex;
    }

    #endregion

    #region RTSP

    private void StartStreaming()
    {
      if (!string.IsNullOrEmpty(_rtspSessionId))
      {
        return;
      }
      this.LogDebug("DRI base: start streaming");
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
      for (int port = _rtpPortMinimum + (_rtpPortMinimum % 2); port <= _rtpPortMaximum; port += 2)  // By convention the port should be even.
      {
        if (!usedPorts.Contains(port))
        {
          rtpClientPort = port;
          break;
        }
      }
      if (rtpClientPort == 0)
      {
        throw new TvException("Failed to start streaming, not able to find free port within configured range ({0} to {1}).", _rtpPortMinimum, _rtpPortMaximum);
      }

      this.LogDebug("DRI base: send RTSP SETUP, RTP client port = {0}", rtpClientPort);
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
                this.LogWarn("DRI base: server specified RTP client port {0} instead of {1}", ports[0], rtpClientPort);
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

      _streamChannel.Url = RtpHelper.ConstructUrl(_localIpAddress, rtpClientPort, _serverIpAddress, rtpServerPort);
      this.LogDebug("DRI base: RTSP SETUP response okay, session ID = {0}, RTP URL = {1}", _rtspSessionId, _streamChannel.Url);

      this.LogDebug("DRI base: send RTSP PLAY");
      request = new RtspRequest(RtspMethod.Play, _rtspUri);
      request.Headers.Add("Session", _rtspSessionId);
      if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
      {
        throw new TvException("Failed to start streaming, non-OK RTSP PLAY status code {0} {1}.", response.StatusCode, response.ReasonPhrase);
      }
      this.LogDebug("DRI base: RTSP PLAY response okay");

      _streamTuner.PerformTuning(_streamChannel);
    }

    private void StopStreaming()
    {
      if (_rtspClient == null || string.IsNullOrEmpty(_rtspSessionId))
      {
        return;
      }
      this.LogDebug("DRI base: stop streaming");
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
    protected virtual void OnStateVariableChanged(CpStateVariable stateVariable, object newValue)
    {
      try
      {
        if (stateVariable.Name.Equals("PCRLock") || stateVariable.Name.Equals("Lock"))
        {
          bool oldStatus = _isSignalLocked;
          _isSignalLocked = (bool)newValue;
          if (oldStatus != _isSignalLocked)
          {
            this.LogInfo("DRI base: tuner lock status update, tuner ID = {0}, lock type = {1}, is locked = {2}", TunerId, stateVariable.Name, _isSignalLocked);
          }
        }
        else if (stateVariable.Name.Equals("CardStatus"))
        {
          CasCardStatus oldStatus = _cardStatus;
          _cardStatus = (CasCardStatus)(string)newValue;
          if (oldStatus != _cardStatus)
          {
            this.LogInfo("DRI base: CableCARD status update, tuner ID = {0}, old status = {1}, new status = {2}", TunerId, oldStatus, _cardStatus);
          }
        }
        else if (stateVariable.Name.Equals("CardMessage"))
        {
          if (!string.IsNullOrEmpty(newValue.ToString()))
          {
            this.LogInfo("DRI base: received message from the CableCARD, tuner ID = {0}, current status = {1}, message = {2}", TunerId, _cardStatus, newValue);
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
            this.LogInfo("DRI base: descrambling status update, tuner ID = {0}, old status = {1}, new status = {2}", TunerId, oldStatus, _descramblingStatus);
          }
        }
        else if (stateVariable.Name.Equals("DrmPairingStatus"))
        {
          SecurityPairingStatus oldStatus = _pairingStatus;
          _pairingStatus = (SecurityPairingStatus)(string)newValue;
          if (oldStatus != _pairingStatus)
          {
            this.LogInfo("DRI base: pairing status update, tuner ID = {0}, old status = {1}, new status = {2}", TunerId, oldStatus, _pairingStatus);
          }
        }
        else
        {
          string unqualifiedServiceName = stateVariable.ParentService.ServiceId.Substring(stateVariable.ParentService.ServiceId.LastIndexOf(":") + 1);
          this.LogDebug("DRI base: state variable change, tuner ID = {0}, variable = {1}, service = {2}, new value = {3}", TunerId, stateVariable.Name, unqualifiedServiceName, newValue ?? "[null]");
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI base: failed to handle state variable change, tuner ID = {0}", TunerId);
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
      this.LogError("DRI base: failed to subscribe to state variable events, tuner ID = {0}, service = {1}, code = {2}, description = {3}", TunerId, unqualifiedServiceName, error.ErrorCode, error.ErrorDescription);
      if (SubChannelCount == 0)
      {
        PerformUnloading();
        PerformLoading(_streamFormat);
      }
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
      Log.Error("DRI base: resolve data type not supported, type name = {0}", dataTypeName);
      dataType = null;
      return false;
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
      this.LogDebug("DRI base: reload configuration");

      _rtpPortMinimum = SettingsManagement.GetValue("streamTunersPortMinimum", 49152);
      _rtpPortMaximum = SettingsManagement.GetValue("streamTunersPortMaximum", 65535);
      this.LogDebug("  RTP port range = {0} - {1}", _rtpPortMinimum, _rtpPortMaximum);

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
      this.LogDebug("DRI base: perform loading");

      _streamFormat = streamFormat;
      _deviceConnection = Connect();

      // services
      this.LogDebug("DRI base: setup services");
      _serviceTuner = new ServiceTuner(_deviceConnection.Device);
      _serviceCas = new ServiceCas(_deviceConnection.Device);
      _serviceMux = new ServiceMux(_deviceConnection.Device);
      _serviceSecurity = new ServiceSecurity(_deviceConnection.Device);
      _serviceDiag = new ServiceDiag(_deviceConnection.Device);
      _serviceUserActivity = new ServiceUserActivity(_deviceConnection.Device);
      _serviceAvTransport = new ServiceAvTransport(_deviceConnection.Device);
      _serviceConnectionManager = new ServiceConnectionManager(_deviceConnection.Device);

      this.LogDebug("DRI base: subscribe services");
      _stateVariableChangeDelegate = new StateVariableChangedDlgt(OnStateVariableChanged);
      _eventSubscriptionFailDelegate = new EventSubscriptionFailedDlgt(OnEventSubscriptionFailed);
      _serviceTuner.SubscribeStateVariables(_stateVariableChangeDelegate, _eventSubscriptionFailDelegate);
      _serviceCas.SubscribeStateVariables(_stateVariableChangeDelegate, _eventSubscriptionFailDelegate);
      _serviceSecurity.SubscribeStateVariables(_stateVariableChangeDelegate, _eventSubscriptionFailDelegate);
      _serviceAvTransport.SubscribeStateVariables(_stateVariableChangeDelegate, _eventSubscriptionFailDelegate);
      _serviceConnectionManager.SubscribeStateVariables(_stateVariableChangeDelegate, _eventSubscriptionFailDelegate);

      int rcsId;
      _serviceConnectionManager.PrepareForConnection(string.Empty, string.Empty, -1, ConnectionDirection.Output, out _connectionId, out _avTransportId, out rcsId);
      this.LogDebug("DRI base: connected, connection ID = {0}, AV transport ID = {1}", _connectionId, _avTransportId);

      ReadDeviceInfo();

      _isPidFilterDisabled = true;
      _pidFilterPids.Clear();
      _pidFilterPidsToAdd.Clear();
      _pidFilterPidsToRemove.Clear();
      string csvPids = (string)_serviceMux.QueryStateVariable("PIDList");
      foreach (string pid in csvPids.Split(','))
      {
        _pidFilterPids.Add(Convert.ToUInt16(pid, 16));
      }
      if (_pidFilterPids.Count > 0)
      {
        _isPidFilterDisabled = false;
        this.LogDebug("DRI base: initial mux service PID list = [{0}]", string.Join(", ", _pidFilterPids));
      }

      TunerExtensionLoader loader = new TunerExtensionLoader();
      IList<ITunerExtension> extensions = loader.Load(this, _descriptor);

      // Add the stream tuner extensions to our extensions, but don't re-sort
      // by priority afterwards. This ensures that our extensions are always
      // given first consideration.
      IList<ITunerExtension> streamTunerExtensions = _streamTuner.PerformLoading(streamFormat);
      foreach (ITunerExtension e in streamTunerExtensions)
      {
        extensions.Add(e);
      }

      _subChannelManager = new SubChannelManagerDri(_serviceMux, _streamTuner.SubChannelManager);
      _channelScanner = _streamTuner.InternalChannelScanningInterface;
      _channelScanner.Tuner = this;
      return extensions;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformSetTunerState(TunerState state, bool isFinalising = false)
    {
      this.LogDebug("DRI base: perform set tuner state");
      if (isFinalising)
      {
        return;
      }

      if (_serviceAvTransport != null && _gotTunerControl)
      {
        if (state == TunerState.Stopped || state == TunerState.Paused)
        {
          if (state == TunerState.Stopped || !_canPause)
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
          // ATI tuners require streaming to be started (at least RTSP SETUP)
          // before AVTransport Play() will succeed. If you call Play() before
          // SETUP you'll receive an HTTP 400 "bad request" response containing
          // a UPnP 0x8100300C "not initialized" error.
          StartStreaming();
          _serviceAvTransport.Play((uint)_avTransportId, "1");
          _transportState = AvTransportState.Playing;
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
      this.LogDebug("DRI base: perform unloading");
      if (isFinalising)
      {
        return;
      }

      _subChannelManager = null;
      _channelScanner = null;
      if (_serviceTuner != null)
      {
        _serviceTuner.Dispose();
        _serviceTuner = null;
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
        if (_connectionId != VALUE_NOT_SET)
        {
          // This call can fail if the connection to the tuner is down. We must
          // not allow failure to cause further problems.
          try
          {
            _serviceConnectionManager.ConnectionComplete(_connectionId);
          }
          catch
          {
            this.LogWarn("DRI base: failed to complete connection manager connection");
          }
        }
        _connectionId = VALUE_NOT_SET;
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
      if (_serviceTuner == null)
      {
        return;
      }

      if (onlyGetLock)
      {
        // The signal locked indicator is evented so should be current and usable.
        isLocked = _isSignalLocked;
        return;
      }

      try
      {
        uint frequency;
        TunerModulation modulation;
        uint snr;
        _serviceTuner.GetTunerParameters(out isPresent, out frequency, out modulation, out isLocked, out strength, out snr);
        _isSignalLocked = isLocked;
        strength = SignalStrengthDecibelsToPercentage(strength);
        quality = SignalQualitySnrToPercentage(snr);
        if (_isSignalLocked && _currentFrequency != frequency)
        {
          this.LogDebug("DRI base: current tuning details, frequency = {0} kHz, modulation = {1}", frequency, modulation);
          _currentFrequency = (int)frequency;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI base: exception updating signal status");
      }
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
        return _subChannelManager;
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
      this.LogDebug("DRI base: enter menu");

      if (_serviceCas == null)
      {
        this.LogWarn("DRI base: not initialised or interface not supported");
        return false;
      }

      if (_cardStatus == CasCardStatus.Removed)
      {
        this.LogError("DRI base: CableCARD not present");
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
        this.LogError(ex, "DRI base: failed to read CableCARD status");
        return false;
      }

      byte[] list = null;
      try
      {
        list = (byte[])_serviceCas.QueryStateVariable("ApplicationList");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI base: failed to get application list");
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
      this.LogDebug("DRI base: close menu");
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
      this.LogDebug("DRI base: close dialog, dialog number = {0}", dialogNumber);

      if (_serviceCas == null)
      {
        this.LogWarn("DRI base: not initialised or interface not supported");
        return false;
      }

      try
      {
        _serviceCas.NotifyMmiClose(dialogNumber);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI base: failed to notify the CableCARD that the MMI dialog has been closed");
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
      this.LogDebug("DRI base: select menu entry, choice = {0}", choice);
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
      this.LogDebug("DRI base: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      // TODO I don't know how to implement this yet.
      return true;
    }

    #endregion

    #region IMpeg2PidFilter members

    /// <summary>
    /// Should the filter be enabled for a given transmitter.
    /// </summary>
    /// <param name="tuningDetail">The current transmitter tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ShouldEnable(IChannel tuningDetail)
    {
      // Enable when tuning without a CableCARD. When tuning with a CableCARD
      // (CAS service SetChannel()), the PID filtering is handled automatically
      // by the tuner.
      bool enableFilter = false;
      ChannelScte scteTuningDetail = tuningDetail as ChannelScte;
      if (scteTuningDetail != null)
      {
        enableFilter = !scteTuningDetail.IsCableCardNeededToTune();
      }

      this.LogDebug("DRI base: need PID filter = {0}", enableFilter);
      return enableFilter;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.Disable()
    {
      // It isn't really possible to disable the PID filter. We'll just remove
      // all the PIDs.
      if (_isPidFilterDisabled)
      {
        _pidFilterPids.Clear();
        _pidFilterPidsToRemove.Clear();
        _pidFilterPidsToAdd.Clear();
        return true;
      }

      this.LogDebug("DRI base: disable PID filter");
      if (_pidFilterPids.Count > 0)
      {
        this.LogDebug("  delete {0} current PID(s)...", _pidFilterPids.Count);
        try
        {
          _serviceMux.RemovePid(_pidFilterPids);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "DRI base: failed to remove all mux service PIDs");
          return false;
        }
        _pidFilterPids.Clear();
      }

      _pidFilterPidsToRemove.Clear();
      _pidFilterPidsToAdd.Clear();
      _isPidFilterDisabled = true;
      this.LogDebug("DRI base: result = success");
      return true;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    int IMpeg2PidFilter.MaximumPidCount
    {
      get
      {
        return -1;    // maximum not known
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
      if (_pidFilterPidsToAdd.Count == 0 && _pidFilterPidsToRemove.Count == 0)
      {
        return true;
      }

      this.LogDebug("DRI base: apply PID filter configuration");

      if (_pidFilterPidsToRemove.Count > 0)
      {
        this.LogDebug("  delete {0} current PID(s)...", _pidFilterPidsToRemove.Count);
        try
        {
          _serviceMux.RemovePid(_pidFilterPidsToRemove);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "DRI base: failed to remove mux service PIDs");
          return false;
        }
        _pidFilterPids.ExceptWith(_pidFilterPidsToRemove);
        _pidFilterPidsToRemove.Clear();
      }

      if (_pidFilterPidsToAdd.Count > 0)
      {
        this.LogDebug("  add {0} new PID(s)...", _pidFilterPidsToAdd.Count);
        try
        {
          _serviceMux.AddPid(_pidFilterPidsToAdd);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "DRI base: failed to add mux service PIDs");
          return false;
        }

        _pidFilterPids.UnionWith(_pidFilterPidsToAdd);
        _pidFilterPidsToAdd.Clear();
      }

      this.LogDebug("DRI base: result = success");
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