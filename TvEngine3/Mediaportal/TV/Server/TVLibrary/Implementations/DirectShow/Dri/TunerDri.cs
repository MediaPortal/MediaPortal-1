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

using System.Text.RegularExpressions;
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.DeviceTree;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Service;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Service;
using System;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using UPnP.Infrastructure.Common;
using System.Net;
using System.IO;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using DirectShowLib.BDA;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Dri
{
  public class TunerDri : TunerStream
  {
    #region constants

    private static readonly Regex REGEX_SIGNAL_INFO = new Regex(@"^(\d+(\.\d+)?)[^\d]");

    #endregion

    #region variables

    private DeviceDescriptor _descriptor = null;
    private UPnPControlPoint _controlPoint = null;
    private DeviceConnection _deviceConnection = null;
    private StateVariableChangedDlgt _stateVariableDelegate = null;
    private EventSubscriptionFailedDlgt _eventSubscriptionDelegate = null;

    // services
    private ServiceTuner _serviceTuner = null;        // [physical] tuning, scanning
    private ServiceFdc _serviceFdc = null;            // forward data channel, carries SI and EPG
    private ServiceAux _serviceAux = null;            // auxiliary analog inputs
    private ServiceEncoder _serviceEncoder = null;    // encoder for auxiliary inputs *and* tuner
    private ServiceCas _serviceCas = null;            // conditional access
    private ServiceMux _serviceMux = null;            // PID filtering
    private ServiceSecurity _serviceSecurity = null;  // DRM system
    private ServiceDiag _serviceDiag = null;          // name/value pair info
    private ServiceAvTransport _serviceAvTransport = null;
    private ServiceConnectionManager _serviceConnectionManager = null;

    private int _connectionId = -1;
    private int _avTransportId = -1;
    private string _streamUrl = string.Empty;

    // These variables are used to ensure we don't interrupt another
    // application using the tuner.
    private bool _gotTunerControl = false;
    private AvTransportState _transportState = AvTransportState.Stopped;
    private volatile bool _isTunerSignalLocked = false;

    // Important DRM state variables.
    private CasCardStatus _cardStatus = CasCardStatus.Removed;
    private CasDescramblingStatus _descramblingStatus = CasDescramblingStatus.Unknown;
    private SecurityPairingStatus _pairingStatus = SecurityPairingStatus.Red;

    private IConditionalAccessMenuCallBacks _caMenuCallBacks = null;
    private byte _mmiDialogNumber = 0;
    private IDictionary<int, string> _mmiMenuLinks = new Dictionary<int, string>();

    private readonly bool _isCetonDevice = false;
    private bool _canPause = false;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDri"/> class.
    /// </summary>
    /// <param name="descriptor">The UPnP device description.</param>
    /// <param name="controlPoint">The control point to use to connect to the device.</param>
    public TunerDri(DeviceDescriptor descriptor, UPnPControlPoint controlPoint)
      : base(descriptor.FriendlyName, descriptor.DeviceUUID)
    {
      _tunerType = CardType.Atsc;
      _descriptor = descriptor;
      _controlPoint = controlPoint;
      _isCetonDevice = descriptor.FriendlyName.Contains("Ceton");
    }

    #region IDisposable member

    public override void Dispose()
    {
      base.Dispose();

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
      if (_serviceAvTransport != null)
      {
        if (_gotTunerControl && _transportState != AvTransportState.Stopped)
        {
          _serviceAvTransport.Stop((uint)_avTransportId);
          _transportState = AvTransportState.Stopped;
        }
        _serviceAvTransport.Dispose();
        _serviceAvTransport = null;
      }
      if (_serviceConnectionManager != null)
      {
        _serviceConnectionManager.ConnectionComplete(_connectionId);
        _serviceConnectionManager.Dispose();
        _serviceConnectionManager = null;
      }

      if (_deviceConnection != null)
      {
        _deviceConnection.Disconnect();
        _deviceConnection = null;
      }
      _gotTunerControl = false;
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBacks">The call back delegate.</param>
    public void SetCallBacks(IConditionalAccessMenuCallBacks callBacks)
    {
      lock (_caMenuCallBackLock)
      {
        _caMenuCallBacks = callBacks;
      }
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterMenu()
    {
      this.LogDebug("DRI CableCARD: enter menu");
      // TODO I don't know how to implement this yet.
      return true;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseMenu()
    {
      this.LogDebug("DRI CableCARD: close menu");
      _serviceCas.NotifyMmiClose(_mmiDialogNumber);
      return true;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenuEntry(byte choice)
    {
      this.LogDebug("DRI CableCARD: select menu entry, choice = {0}", choice);
      string url;
      if (_mmiMenuLinks.TryGetValue(choice, out url))
      {
        HandleMmiUrl(url);
      }
      return true;
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool AnswerEnquiry(bool cancel, string answer)
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

    private bool IsTunerInUse()
    {
      if (_gotTunerControl)
      {
        return false;
      }
      AvTransportStatus transportStatus = AvTransportStatus.Ok;
      string speed = string.Empty;
      _serviceAvTransport.GetTransportInfo((uint)_avTransportId, out _transportState, out transportStatus, out speed);
      if (_transportState == AvTransportState.Stopped)
      {
        return false;
      }
      return true;
    }

    private void ReadDeviceInfo()
    {
      try
      {
        this.LogDebug("DRI CableCARD: current tuner status...");
        bool isCarrierLocked = false;
        uint frequency = 0;
        TunerModulation modulation = TunerModulation.All;
        bool isPcrLocked = false;
        int signalLevel = 0;
        uint snr = 0;
        _serviceTuner.GetTunerParameters(out isCarrierLocked, out frequency, out modulation, out isPcrLocked, out signalLevel, out snr);
        this.LogDebug("  carrier lock = {0}", isCarrierLocked);
        this.LogDebug("  frequency    = {0} kHz", frequency);
        this.LogDebug("  modulation   = {0}", modulation.ToString());
        this.LogDebug("  PCR lock     = {0}", isPcrLocked);
        this.LogDebug("  signal level = {0} dBmV", signalLevel);
        this.LogDebug("  SNR          = {0} dB", snr);

        this.LogDebug("DRI CableCARD: current forward data channel status...");
        uint bitrate = 0;
        bool spectrumInversion = false;
        IList<ushort> pids;
        _serviceFdc.GetFdcStatus(out bitrate, out isCarrierLocked, out frequency, out spectrumInversion, out pids);
        this.LogDebug("  bitrate           = {0} kbps", bitrate);
        this.LogDebug("  carrier lock      = {0}", isCarrierLocked);
        this.LogDebug("  frequency         = {0} kHz", frequency);
        this.LogDebug("  spectrum inverted = {0}", spectrumInversion);
        this.LogDebug("  PIDs              = {0}", string.Join(", ", pids.Select(x => x.ToString()).ToArray()));

        IList<AuxFormat> formats;
        byte svideoInputCount = 0;
        byte compositeInputCount = 0;
        if (_serviceAux.GetAuxCapabilities(out formats, out svideoInputCount, out compositeInputCount))
        {
          this.LogDebug("DRI CableCARD: auxiliary input info...");
          this.LogDebug("  supported formats     = {0}", string.Join(", ", formats.Select(x => x.ToString()).ToArray()));
          this.LogDebug("  S-video input count   = {0}", svideoInputCount);
          this.LogDebug("  composite input count = {0}", compositeInputCount);
        }
        else
        {
          this.LogDebug("DRI CableCARD: auxiliary inputs not present/supported");
        }

        IList<EncoderAudioProfile> audioProfiles;
        IList<EncoderVideoProfile> videoProfiles;
        _serviceEncoder.GetEncoderCapabilities(out audioProfiles, out videoProfiles);
        if (audioProfiles.Count > 0)
        {
          this.LogDebug("DRI CableCARD: encoder audio profiles...");
          foreach (EncoderAudioProfile ap in audioProfiles)
          {
            this.LogDebug("  codec = {0}, bit depth = {1}, channel count = {2}, sample rate = {3} Hz", Enum.GetName(typeof(EncoderAudioAlgorithm), ap.AudioAlgorithmCode), ap.BitDepth, ap.NumberChannel, ap.SamplingRate);
          }
        }
        if (videoProfiles.Count > 0)
        {
          this.LogDebug("DRI CableCARD: encoder video profiles...");
          foreach (EncoderVideoProfile vp in videoProfiles)
          {
            this.LogDebug("  hor. pixels = {0}, vert. pixels = {1}, aspect ratio = {2}, frame rate = {3}, {4}", vp.HorizontalSize, vp.VerticalSize, Enum.GetName(typeof(EncoderVideoAspectRatio), vp.AspectRatioInformation), Enum.GetName(typeof(EncoderVideoFrameRate), vp.FrameRateCode));
          }
        }

        uint maxAudioBitrate = 0;
        uint minAudioBitrate = 0;
        EncoderMode audioBitrateMode = EncoderMode.ConstantBitRate;
        uint audioBitrateStepping = 0;
        uint audioBitrate = 0;
        byte audioProfileIndex = 0;
        bool isMuted = false;
        bool sapDetected = false; // second audio program (additional audio stream)
        bool sapActive = false;
        EncoderFieldOrder fieldOrder = EncoderFieldOrder.Higher;
        EncoderInputSelection source = EncoderInputSelection.Aux;
        bool noiseFilterActive = false;
        bool pulldownDetected = false;
        bool pulldownActive = false;
        uint maxVideoBitrate = 0;
        uint minVideoBitrate = 0;
        EncoderMode videoBitrateMode = EncoderMode.ConstantBitRate;
        uint videoBitrate = 0;
        uint videoBitrateStepping = 0;
        byte videoProfileIndex = 0;
        _serviceEncoder.GetEncoderParameters(out maxAudioBitrate, out minAudioBitrate, out audioBitrateMode,
          out audioBitrateStepping, out audioBitrate, out audioProfileIndex, out isMuted,
          out fieldOrder, out source, out noiseFilterActive, out pulldownDetected,
          out pulldownActive, out sapDetected, out sapActive, out maxVideoBitrate, out minVideoBitrate,
          out videoBitrateMode, out videoBitrate, out videoBitrateStepping, out videoProfileIndex);
        this.LogDebug("DRI CableCARD: current encoder audio parameters...");
        this.LogDebug("  max bitrate  = {0} kbps", maxAudioBitrate / 1000);
        this.LogDebug("  min bitrate  = {0} kbps", minAudioBitrate / 1000);
        this.LogDebug("  bitrate mode = {0}", audioBitrateMode);
        this.LogDebug("  bitrate step = {0} kbps", audioBitrateStepping / 1000);
        this.LogDebug("  bitrate      = {0} kbps", audioBitrate / 1000);
        this.LogDebug("  profile      = {0}", audioProfileIndex);
        this.LogDebug("  is muted     = {0}", isMuted);
        this.LogDebug("  SAP detected = {0}", sapDetected);
        this.LogDebug("  SAP active   = {0}", sapActive);
        this.LogDebug("DRI CableCARD: current encoder video parameters...");
        this.LogDebug("  max bitrate  = {0} kbps", maxVideoBitrate / 1000);
        this.LogDebug("  min bitrate  = {0} kbps", minVideoBitrate / 1000);
        this.LogDebug("  bitrate mode = {0}", videoBitrateMode);
        this.LogDebug("  bitrate step = {0} kbps", videoBitrateStepping / 1000);
        this.LogDebug("  bitrate      = {0} kbps", videoBitrate / 1000);
        this.LogDebug("  profile      = {0}", videoProfileIndex);
        this.LogDebug("  field order  = {0}", fieldOrder.ToString());
        this.LogDebug("  source       = {0}", source.ToString());
        this.LogDebug("  noise filter = {0}", noiseFilterActive);
        this.LogDebug("  3:2 detected = {0}", pulldownDetected);
        this.LogDebug("  3:2 active   = {0}", pulldownActive);

        this.LogDebug("DRI CableCARD: card status...");
        CasCardStatus status = CasCardStatus.Removed;
        string manufacturer = string.Empty;
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

        this.LogDebug("DRI CableCARD: diagnostic parameters...");
        string value = string.Empty;
        bool isVolatile = false;
        foreach (DiagParameterDri p in DiagParameterDri.Values)
        {
          _serviceDiag.GetParameter(p, out value, out isVolatile);
          this.LogDebug("  {0}{1} = {2}", p.ToString(), isVolatile ? " [volatile]" : "", value);
        }
        if (_isCetonDevice)
        {
          this.LogDebug("DRI CableCARD: Ceton-specific diagnostic parameters...");
          foreach (DiagParameterCeton p in DiagParameterCeton.Values)
          {
            _serviceDiag.GetParameter(p, out value, out isVolatile);
            this.LogDebug("  {0}{1} = {2}", p.ToString(), isVolatile ? " [volatile]" : "", value);
          }
        }

        _canPause = false;
        IList<AvTransportAction> actions;
        if (_serviceAvTransport.GetCurrentTransportActions((uint)_avTransportId, out actions))
        {
          this.LogDebug("DRI CableCARD: supported AV transport actions = {0}", string.Join(", ", actions.Select(x => x.ToString()).ToArray()));
          if (actions.Contains(AvTransportAction.Pause))
          {
            _canPause = true;
          }
        }
        IList<AvTransportStorageMedium> playMedia;
        IList<AvTransportStorageMedium> recordMedia;
        IList<AvTransportRecordQualityMode> recordQualityModes;
        _serviceAvTransport.GetDeviceCapabilities((uint)_avTransportId, out playMedia, out recordMedia, out recordQualityModes);
        this.LogDebug("DRI CableCARD: supported play media = {0}", string.Join(", ", playMedia.Select(x => x.ToString()).ToArray()));
        this.LogDebug("DRI CableCARD: supported record media = {0}", string.Join(", ", recordMedia.Select(x => x.ToString()).ToArray()));
        this.LogDebug("DRI CableCARD: supported record quality modes = {0}", string.Join(", ", recordQualityModes.Select(x => x.ToString()).ToArray()));

        this.LogDebug("DRI CableCARD: media info...");
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
        this.LogDebug("  track count        = {0}", trackCount);
        this.LogDebug("  duration           = {0}", mediaDuration);
        this.LogDebug("  cur. URI           = {0}", currentUri);
        this.LogDebug("  cur. URI meta data = {0}", currentUriMetaData);
        this.LogDebug("  next URI           = {0}", nextUri);
        this.LogDebug("  next URI meta data = {0}", nextUriMetaData);
        this.LogDebug("  play medium        = {0}", playMedium);
        this.LogDebug("  record medium      = {0}", recordMedium);
        this.LogDebug("  write status       = {0}", writeStatus);
        _streamUrl = currentUri;

        /*
         * Ceton tuners set relCount and absCount to NOT_IMPLEMENTED. Those
         * parameters are meant to be type i4 (32 bit integers), so those
         * values are invalid. This is confirmed in the UPnP specs which state
         * such parameters should be set to the max value for i4.
        this.LogDebug("DRI CableCARD: position info...");
        uint track = 0;
        string duration = string.Empty;
        string metaData = string.Empty;
        string uri = string.Empty;
        string relTime = string.Empty;
        string absTime = string.Empty;
        int relCount = 0;
        int absCount = 0;
        _avTransportService.GetPositionInfo((uint)_avTransportId, out track, out duration, out metaData, out uri,
          out relTime, out absTime, out relCount, out absCount);
        this.LogDebug("  track          = {0}", track);
        this.LogDebug("  duration       = {0}", duration);
        this.LogDebug("  meta data      = {0}", metaData);
        this.LogDebug("  URI            = {0}", uri);
        this.LogDebug("  relative time  = {0}", relTime);
        this.LogDebug("  absolute time  = {0}", absTime);
        this.LogDebug("  relative count = {0}", relCount);
        this.LogDebug("  absolute count = {0}", absCount);*/

        this.LogDebug("DRI CableCARD: transport info...");
        AvTransportState transportState = AvTransportState.NoMediaPresent;
        AvTransportStatus transportStatus = AvTransportStatus.Ok;
        string speed = string.Empty;
        _serviceAvTransport.GetTransportInfo((uint)_avTransportId, out transportState, out transportStatus, out speed);
        this.LogDebug("  state       = {0}", transportState);
        this.LogDebug("  status      = {0}", transportStatus);
        this.LogDebug("  speed       = {0}", speed);

        AvTransportCurrentPlayMode playMode;
        AvTransportRecordQualityMode recordQualityMode;
        _serviceAvTransport.GetTransportSettings((uint)_avTransportId, out playMode, out recordQualityMode);
        this.LogDebug("  play mode   = {0}", playMode);
        this.LogDebug("  record mode = {0}", recordQualityMode);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI CableCARD: failed to read device info");
        throw;
      }
    }

    #region graph building

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    protected override void PerformLoading()
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
      _serviceAvTransport = new ServiceAvTransport(_deviceConnection.Device);
      _serviceConnectionManager = new ServiceConnectionManager(_deviceConnection.Device);

      this.LogDebug("DRI CableCARD: subscribe services");
      _stateVariableDelegate = new StateVariableChangedDlgt(OnStateVariableChanged);
      _eventSubscriptionDelegate = new EventSubscriptionFailedDlgt(OnEventSubscriptionFailed);
      _serviceTuner.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceAux.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceEncoder.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceCas.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceSecurity.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceAvTransport.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);
      _serviceConnectionManager.SubscribeStateVariables(_stateVariableDelegate, _eventSubscriptionDelegate);

      int rcsId = -1;
      _serviceConnectionManager.PrepareForConnection(string.Empty, string.Empty, -1, ConnectionDirection.Output, out _connectionId, out _avTransportId, out rcsId);
      this.LogDebug("DRI CableCARD: PrepareForConnection, connection ID = {0}, AV transport ID = {1}", _connectionId, _avTransportId);

      // Check that the device is not already in use.
      if (IsTunerInUse())
      {
        throw new TvExceptionTunerLoadFailed("Tuner appears to be in use.");
      }

      ReadDeviceInfo();

      base.PerformLoading();
    }

    /// <summary>
    /// Set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    protected override void SetTunerState(TunerState state)
    {
      this.LogDebug("DRI CableCARD: set tuner state, current state = {0}, requested state = {1}", _state, state);

      if (state == _state)
      {
        this.LogDebug("DRI CableCARD: tuner already in required state");
        return;
      }

      ATSCChannel atscChannel = _currentTuningDetail as ATSCChannel;
      if (IsScanning && atscChannel != null && atscChannel.PhysicalChannel == 0)
      {
        // Scanning with a CableCARD doesn't require streaming. The channel
        // info is evented via the forward data channel service.
        return;
      }

      if (_serviceAvTransport != null && _gotTunerControl)
      {
        if (state == TunerState.Stopped || (state == TunerState.Paused && !_canPause))
        {
          _serviceAvTransport.Stop((uint)_avTransportId);
          _transportState = AvTransportState.Stopped;
          _gotTunerControl = false;
        }
        else if (state == TunerState.Paused)
        {
          _serviceAvTransport.Pause((uint)_avTransportId);
          _transportState = AvTransportState.PausedPlayback;
        }
        else
        {
          _serviceAvTransport.Play((uint)_avTransportId, "1");
          _transportState = AvTransportState.Playing;
        }
      }
      base.SetTunerState(state);
    }

    /// <summary>
    /// Stop the tuner. The actual result of this function depends on tuner configuration.
    /// </summary>
    public override void Stop()
    {
      // CableCARD tuners don't forget the tuned channel after they're stopped,
      // and they reject tune requests for the current channel.
      IChannel savedTuningDetail = _currentTuningDetail;
      base.Stop();
      _currentTuningDetail = savedTuningDetail;
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
      return channel is ATSCChannel;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      this.LogDebug("DRI CableCARD: perform tuning");
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      // Is a CableCARD required?
      bool isSignalLocked = false;
      if (!atscChannel.FreeToAir || (IsScanning && atscChannel.PhysicalChannel == 0))
      {
        if (_cardStatus != CasCardStatus.Inserted)
        {
          throw new TvException("CableCARD not available, current status is {0}.", _cardStatus);
        }

        // We only need the OOB tuner when scanning with the CableCARD. We
        // don't even have to start a stream so we don't interrupt other
        // applications. Just check that the OOB tuner is locked.
        if (IsScanning)
        {
          this.LogDebug("DRI CableCARD: check out-of-band tuner lock");
          uint bitrate = 0;
          uint frequency = 0;
          bool spectrumInversion = false;
          IList<ushort> pids;
          _serviceFdc.GetFdcStatus(out bitrate, out isSignalLocked, out frequency, out spectrumInversion, out pids);
          _isSignalLocked = isSignalLocked;
          if (!isSignalLocked)
          {
            throw new TvExceptionNoSignal("Out-of-band tuner not locked.");
          }
          return;
        }
      }

      if (IsTunerInUse())
      {
        throw new TvException("Tuner appears to be in use.");
      }
      _gotTunerControl = true;

      // CableCARD tuning
      if (!atscChannel.FreeToAir)
      {
        if (atscChannel.MajorChannel > 0)
        {
          this.LogDebug("DRI CableCARD: tuning by channel number");
          _serviceCas.SetChannel((uint)atscChannel.MajorChannel, 0, CasCaptureMode.Live, out isSignalLocked);
        }
        else if (atscChannel.NetworkId > 0)
        {
          this.LogDebug("DRI CableCARD: tuning by source ID");
          _serviceCas.SetChannel(0, (uint)atscChannel.NetworkId, CasCaptureMode.Live, out isSignalLocked);
        }
        else
        {
          throw new TvException("Not able to tune encrypted channel without both channel number and source ID.");
        }
      }
      // clear QAM or ATSC tuning
      else
      {
        if (atscChannel.Frequency <= 0)
        {
          throw new TvException("Not able to tune non-encrypted channel without frequency.");
        }
        this.LogDebug("DRI CableCARD: tuning by frequency");
        IList<TunerModulation> moduations = new List<TunerModulation>();
        if (atscChannel.ModulationType == ModulationType.Mod256Qam)
        {
          moduations.Add(TunerModulation.Qam256);
        }
        else if (atscChannel.ModulationType == ModulationType.Mod64Qam)
        {
          moduations.Add(TunerModulation.Qam64);
        }
        else if (atscChannel.ModulationType == ModulationType.Mod8Vsb)
        {
          moduations.Add(TunerModulation.Vsb8);
        }
        else
        {
          this.LogWarn("DRI CableCARD: unsupported modulation {0}, allowing tuner to use any supported modulation", atscChannel.ModulationType);
          moduations.Add(TunerModulation.All);
        }

        uint currentFrequency;
        TunerModulation currentModulation;
        _serviceTuner.SetTunerParameters((uint)atscChannel.Frequency, moduations, out currentFrequency, out currentModulation, out isSignalLocked);
      }
      _isSignalLocked = isSignalLocked;

      if (_transportState != AvTransportState.Playing)
      {
        this.LogDebug("DRI CableCARD: start streaming");
        DVBIPChannel streamChannel = new DVBIPChannel();
        streamChannel.Url = _streamUrl;
        base.PerformTuning(streamChannel);
      }
    }

    #endregion

    #region signal

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    protected override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      if (onlyUpdateLock)
      {
        // When we're asked to only update locked status it means the tuner is
        // trying to lock in on signal. When scanning with the out-of-band
        // tuner, we already checked lock during tuning. In all other cases the
        // signal locked indicator is evented so should be updated
        // automatically. No need to do anything.
        return;
      }

      try
      {
        uint frequency = 0;
        if (IsScanning)
        {
          ATSCChannel atscChannel = _currentTuningDetail as ATSCChannel;
          if (atscChannel != null && atscChannel.PhysicalChannel == 0)
          {
            // If scanning with the out-of-band tuner...
            if (_serviceFdc == null)
            {
              return;
            }

            uint bitrate = 0;
            bool isCarrierLocked = false;
            bool spectrumInversion = false;
            IList<ushort> pids;
            _serviceFdc.GetFdcStatus(out bitrate, out isCarrierLocked, out frequency, out spectrumInversion, out pids);
            _isSignalLocked = isCarrierLocked;
            _isSignalPresent = _isSignalLocked;
            _signalLevel = 0;
            _signalQuality = 0;

            if (_isCetonDevice)
            {
              string value = string.Empty;
              bool isVolatile = false;
              _serviceDiag.GetParameter(DiagParameterCeton.OobSignalLevel, out value, out isVolatile);
              // Example: "0.8 dBmV". Assumed range -25..25.
              Match m = REGEX_SIGNAL_INFO.Match(value);
              if (m.Success)
              {
                _signalLevel = (int)(double.Parse(value) * 2) + 50;
              }
              else
              {
                this.LogWarn("DRI CableCARD: failed to interpret out-of-band signal level {0}", value);
              }
              // Example: "31.9 dB". Use value as-is.
              _serviceDiag.GetParameter(DiagParameterCeton.OobSnr, out value, out isVolatile);
              m = REGEX_SIGNAL_INFO.Match(value);
              if (m.Success)
              {
                _signalQuality = (int)double.Parse(value);
              }
              else
              {
                this.LogWarn("DRI CableCARD: failed to interpret out-of-band signal-to-noise ratio {0}", value);
              }
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
        _serviceTuner.GetTunerParameters(out _isSignalPresent, out frequency, out modulation, out _isSignalLocked, out _signalLevel, out snr);
        _signalLevel = (_signalLevel * 2) + 50;
        _signalQuality = (int)snr;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI CableCARD: exception updating signal status");
      }
    }

    #endregion

    #region scanning

    /// <summary>
    /// Get or set a value indicating whether this tuner is scanning for channels.
    /// </summary>
    /// <value><c>true</c> if the tuner is currently scanning, otherwise <c>false</c></value>
    public override bool IsScanning
    {
      get
      {
        return _isScanning;
      }
      set
      {
        _isScanning = value;
        if (!value)
        {
          _isSignalLocked = _isTunerSignalLocked;
        }
      }
    }

    public override ITVScanning ScanningInterface
    {
      get
      {
        return new ScannerDri(this, _serviceFdc);
      }
    }

    #endregion

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
            this.LogInfo("DRI CableCARD: tuner {0} {1} update, not locked", _cardId, stateVariable.Name);
          }
          _isTunerSignalLocked = (bool)newValue;
          if (!IsScanning)
          {
            _isSignalLocked = _isTunerSignalLocked;
          }
        }
        else if (stateVariable.Name.Equals("CardStatus"))
        {
          CasCardStatus oldStatus = _cardStatus;
          _cardStatus = (CasCardStatus)(string)newValue;
          if (oldStatus != _cardStatus)
          {
            this.LogInfo("DRI CableCARD: tuner {0} CableCARD status update, old status = {1}, new status = {2}", _cardId, oldStatus, _cardStatus);
          }
        }
        else if (stateVariable.Name.Equals("CardMessage"))
        {
          if (!string.IsNullOrEmpty(newValue.ToString()))
          {
            this.LogInfo("DRI CableCARD: tuner {0} received message from the CableCARD, current status = {1}, message = {2}", _cardId, _cardStatus, newValue);
          }
        }
        else if (stateVariable.Name.Equals("MMIMessage"))
        {
          byte[] message = (byte[])newValue;
          if (message == null || message.Length < 3)
          {
            return;
          }

          // DRI specification, page 32 table 6.2-30.
          //Dump.DumpBinary(message, message.Length);
          _mmiDialogNumber = message[0];
          CasMmiDisplayType displayType = (CasMmiDisplayType)message[1];
          CasMmiAction action = (CasMmiAction)message[2];
          this.LogInfo("DRI CableCARD: tuner {0} received MMI message", _cardId);
          this.LogDebug("  dialog number = {0}", _mmiDialogNumber);
          this.LogDebug("  display type  = {0}", displayType);
          this.LogDebug("  action        = {0}", action);
          if (action == CasMmiAction.Close)
          {
            if (_caMenuCallBacks != null)
            {
              try
              {
                _caMenuCallBacks.OnCiCloseDisplay(0);
              }
              catch (Exception ex)
              {
                Log.Log.Error("DRI CC: MMI OnCloseDisplay() exception\r\n{0}", ex);
              }
            }
            return;
          }
          else if (action != CasMmiAction.Open)
          {
            this.LogInfo("DRI CableCARD: unrecognised action {0}, ignoring", action);
            return;
          }

          if (message.Length < 5)
          {
            Log.Log.Error("DRI CC: invalid message, open action with message length {0}", message.Length);
            Dump.DumpBinary(message, message.Length);
            return;
          }

          int urlLength = (message[3] << 8) + message[4] - 1; // URL seems to be NULL terminated
          string url = System.Text.Encoding.ASCII.GetString(message, 5, urlLength);
          this.LogDebug("  URL           = {0}", url);
          HandleMmiUrl(url);
        }
        else if (stateVariable.Name.Equals("DescramblingStatus"))
        {
          CasDescramblingStatus oldStatus = _descramblingStatus;
          _descramblingStatus = (CasDescramblingStatus)(string)newValue;
          if (oldStatus != _descramblingStatus)
          {
            this.LogInfo("DRI CableCARD: tuner {0} descrambling status update, old status = {1}, new status = {2}", _cardId, oldStatus, _descramblingStatus);
          }
        }
        else if (stateVariable.Name.Equals("DrmPairingStatus"))
        {
          SecurityPairingStatus oldStatus = _pairingStatus;
          _pairingStatus = (SecurityPairingStatus)(string)newValue;
          if (oldStatus != _pairingStatus)
          {
            this.LogInfo("DRI CableCARD: tuner {0} pairing status update, old status = {1}, new status = {2}", _cardId, oldStatus, _pairingStatus);
          }
        }
        else
        {
          string unqualifiedServiceName = stateVariable.ParentService.ServiceId.Substring(stateVariable.ParentService.ServiceId.LastIndexOf(":") + 1);
          this.LogDebug("DRI CableCARD: tuner {0} state variable {1} for service {2} changed to {3}", _cardId, stateVariable.Name, unqualifiedServiceName, newValue ?? "[null]");
        }
      }
      catch (Exception ex)
      {
        Log.Log.Error("DRI CC: tuner {0} failed to handle state variable change\r\n{1}", _cardId, ex);
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
      Log.Log.Error("DRI CC: device {0} failed to subscribe to state variable events for service {1}, code = {2}, description = {3}", _cardId, unqualifiedServiceName, error.ErrorCode, error.ErrorDescription);
    }

    private void HandleMmiUrl(string url)
    {
      // CableCARD tuners construct an HTML page containing the messages that
      // we'd expect to receive directly from a DVB CAM via MMI. We retrieve
      // the HTML page contents and try to convert it into a menu as best as
      // possible. Although the DRI specification states the URL will be
      // relative, in practice that is not always the case. Typical!
      Uri uri;
      if (url.StartsWith("http"))
      {
        uri = new Uri(url);
      }
      else
      {
        uri = new Uri(
          new Uri(_deviceConnection.RootDescriptor.SSDPRootEntry.PreferredLink.DescriptionLocation),
          "/get_cc_url?" + url
        );
      }

      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
      request.Timeout = 5000;
      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      string content = string.Empty;
      try
      {
        using (Stream s = response.GetResponseStream())
        {
          using (TextReader textReader = new StreamReader(s))
          {
            content = textReader.ReadToEnd();
            textReader.Close();
          }
          s.Close();
        }
      }
      catch (Exception ex)
      {
        Log.Log.Error("DRI CC: device {0} failed to retrieve MMI HTML content from {1}\r\n{2}", _cardId, uri, ex);
        return;
      }
      finally
      {
        response.Close();
      }

      // Reformat from pure HTML into title and menu items. This is quite
      // hacky, but we have no way to render HTML in MediaPortal.
      this.LogDebug("DRI CableCARD: device {0} retrieved raw MMI HTML {1}", _cardId, content);
      try
      {
        content = Regex.Replace(content, "(<\\/?b>|<center>)", string.Empty, RegexOptions.IgnoreCase);
        content = Regex.Replace(content, "&nbsp;", " ", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @".*<body( [^>]*)?>\s*(.*?)\s*</body>.*", "$2", RegexOptions.IgnoreCase);
        this.LogDebug("DRI CableCARD: pre-split MMI HTML = {0}", content);
        _mmiMenuLinks.Clear();
        string[] sections = content.Split(new string[] { "<br>", "<BR>" }, StringSplitOptions.RemoveEmptyEntries);
        if (_caMenuCallBacks != null)
        {
          _caMenuCallBacks.OnCiMenu(sections[0].Trim(), string.Empty, string.Empty, sections.Length - 1);
        }
        this.LogDebug("  title = {0}", sections[0].Trim());
        for (int i = 1; i < sections.Length; i++)
        {
          string item = sections[i].Trim();
          Match m = Regex.Match(sections[i], "<a href=\"([^\"]+)\">\\s*(.*?)\\s*</a>", RegexOptions.IgnoreCase);
          if (m.Success)
          {
            string itemUrl = m.Groups[1].Captures[0].Value;
            item = m.Groups[2].Captures[0].Value;
            _mmiMenuLinks.Add(i - 1, itemUrl);
            this.LogDebug("  item {0} = {1} [{2}]", i, item, itemUrl);
          }
          else
          {
            item = item.Trim();
            this.LogDebug("  item {0} = {1}", i, item);
          }
          if (_caMenuCallBacks != null)
          {
            _caMenuCallBacks.OnCiMenuChoice(i - 1, item);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Error("DRI CC: device {0} MMI HTML handling failed\r\n{1}\r\n{2}", _cardId, content, ex);
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
      Log.Error("DRI: resolve data type not supported, type name = {0}", dataTypeName);
      dataType = null;
      return true;
    }
  }
}