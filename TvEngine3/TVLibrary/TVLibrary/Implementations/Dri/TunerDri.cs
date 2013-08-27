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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Implementations.Dri.Service;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Helper;
using TvLibrary.Interfaces;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri
{
  public class TunerDri : TvCardATSC, ICiMenuActions
  {
    #region constants

    private static readonly Guid SourceFilterClsid = new Guid(0xd3dd4c59, 0xd3a7, 0x4b82, 0x97, 0x27, 0x7b, 0x92, 0x03, 0xeb, 0x67, 0xc0);

    #endregion

    #region variables

    private DeviceDescriptor _descriptor = null;
    private UPnPControlPoint _controlPoint = null;
    private DeviceConnection _deviceConnection = null;
    private StateVariableChangedDlgt _stateVariableDelegate = null;

    // services
    private TunerService _tunerService = null;        // [physical] tuning, scanning
    private FdcService _fdcService = null;            // forward data channel, carries SI and EPG
    private AuxService _auxService = null;            // auxiliary analog inputs
    private EncoderService _encoderService = null;    // encoder for auxiliary inputs *and* tuner
    private CasService _casService = null;            // conditional access
    private MuxService _muxService = null;            // PID filtering
    private SecurityService _securityService = null;  // DRM system
    private DiagService _diagService = null;          // name/value pair info
    private AvTransportService _avTransportService = null;
    private ConnectionManagerService _connectionManagerService = null;

    private IBaseFilter _filterStreamSource = null;
    private AMMediaType _mpeg2TransportStream = null;

    private int _connectionId = -1;
    private int _avTransportId = -1;
    private string _streamUrl = string.Empty;

    // These variables are used to ensure we don't interrupt another
    // application using the tuner.
    private bool _gotTunerControl = false;
    private UpnpAvTransportState _transportState = UpnpAvTransportState.STOPPED;

    // Important DRM state variables.
    private DriCasCardStatus _cardStatus = DriCasCardStatus.Removed;
    private DriCasDescramblingStatus _descramblingStatus = DriCasDescramblingStatus.Unknown;
    private DriSecurityPairingStatus _pairingStatus = DriSecurityPairingStatus.Red;

    private ICiMenuCallbacks _ciMenuCallbacks = null;
    private IDictionary<int, string> _menuLinks = new Dictionary<int, string>();

    private ManualResetEvent _eventSignalLock = null;

    private readonly bool _isCetonDevice = false;
    private bool _canPause = false;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDri"/> class.
    /// </summary>
    /// <param name="descriptor">The device description. Essentially an XML document describing the device interface.</param>
    /// <param name="controlPoint">The control point to use to connect to the device.</param>
    public TunerDri(DeviceDescriptor descriptor, UPnPControlPoint controlPoint)
      : base(null)
    {
      _descriptor = descriptor;
      _controlPoint = controlPoint;
      _name = descriptor.FriendlyName;
      _devicePath = descriptor.DeviceUDN;   // unique device name is as good as a device path for a unique identifier
      _isCetonDevice = _name.Contains("Ceton");

      GetPreloadBitAndCardId();
      GetSupportsPauseGraph();
    }

    #region IDisposable member

    public override void Dispose()
    {
      if (_eventSignalLock != null)
      {
        _eventSignalLock.Close();
        _eventSignalLock = null;
      }

      base.Dispose();

      RemoveStreamSourceFilter();
      if (_mpeg2TransportStream != null)
      {
        DsUtils.FreeAMMediaType(_mpeg2TransportStream);
        _mpeg2TransportStream = null;
      }
      if (_tunerService != null)
      {
        _tunerService.Dispose();
        _tunerService = null;
      }
      if (_fdcService != null)
      {
        _fdcService.Dispose();
        _fdcService = null;
      }
      if (_auxService != null)
      {
        _auxService.Dispose();
        _auxService = null;
      }
      if (_encoderService != null)
      {
        _encoderService.Dispose();
        _encoderService = null;
      }
      if (_casService != null)
      {
        _casService.Dispose();
        _casService = null;
      }
      if (_muxService != null)
      {
        _muxService.Dispose();
        _muxService = null;
      }
      if (_securityService != null)
      {
        _securityService.Dispose();
        _securityService = null;
      }
      if (_diagService != null)
      {
        _diagService.Dispose();
        _diagService = null;
      }
      if (_avTransportService != null)
      {
        if (_gotTunerControl && _transportState != UpnpAvTransportState.STOPPED)
        {
          _avTransportService.Stop((uint)_avTransportId);
          _transportState = UpnpAvTransportState.STOPPED;
        }
        _avTransportService.Dispose();
        _avTransportService = null;
      }
      if (_connectionManagerService != null)
      {
        _connectionManagerService.ConnectionComplete(_connectionId);
        _connectionManagerService.Dispose();
        _connectionManagerService = null;
      }

      if (_deviceConnection != null)
      {
        _deviceConnection.Disconnect();
        _deviceConnection = null;
      }
      _gotTunerControl = false;
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Set the CAM callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      Log.Log.Debug("DRI CC: enter menu");
      // TODO I don't know how to implement this yet.
      return true;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      Log.Log.Debug("DRI CC: close menu");
      _casService.NotifyMmiClose(0);
      return true;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      Log.Log.Debug("DRI CC: select menu entry, choice = {0}", choice);
      // TODO I don't know how to implement this yet.
      return true;
    }

    /// <summary>
    /// Send a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Log.Debug("DRI CC: send menu answer, answer = {0}, cancel = {1}", answer, cancel);
      // TODO I don't know how to implement this yet.
      return true;
    }

    #endregion

    public DriCasCardStatus CardStatus
    {
      get
      {
        return _cardStatus;
      }
    }

    private bool IsTunerInUse()
    {
      if (_gotTunerControl)
      {
        return false;
      }
      UpnpAvTransportStatus transportStatus = UpnpAvTransportStatus.OK;
      string speed = string.Empty;
      _avTransportService.GetTransportInfo((uint)_avTransportId, out _transportState, out transportStatus, out speed);
      if (_transportState == UpnpAvTransportState.STOPPED)
      {
        return false;
      }
      return true;
    }

    private void ReadDeviceInfo()
    {
      try
      {
        Log.Log.Debug("DRI CC: current tuner status...");
        bool isCarrierLocked = false;
        uint frequency = 0;
        DriTunerModulation modulation = DriTunerModulation.All;
        bool isPcrLocked = false;
        int signalLevel = 0;
        uint snr = 0;
        _tunerService.GetTunerParameters(out isCarrierLocked, out frequency, out modulation, out isPcrLocked, out signalLevel, out snr);
        Log.Log.Debug("  carrier lock = {0}", isCarrierLocked);
        Log.Log.Debug("  frequency    = {0} kHz", frequency);
        Log.Log.Debug("  modulation   = {0}", modulation.ToString());
        Log.Log.Debug("  PCR lock     = {0}", isPcrLocked);
        Log.Log.Debug("  signal level = {0} dBmV", signalLevel);
        Log.Log.Debug("  SNR          = {0} dB", snr);

        Log.Log.Debug("DRI CC: current forward data channel status...");
        uint bitrate = 0;
        bool spectrumInversion = false;
        IList<ushort> pids;
        _fdcService.GetFdcStatus(out bitrate, out isCarrierLocked, out frequency, out spectrumInversion, out pids);
        Log.Log.Debug("  bitrate           = {0} kbps", bitrate);
        Log.Log.Debug("  carrier lock      = {0}", isCarrierLocked);
        Log.Log.Debug("  frequency         = {0} kHz", frequency);
        Log.Log.Debug("  spectrum inverted = {0}", spectrumInversion);
        Log.Log.Debug("  PIDs              = {0}", string.Join(", ", pids.Select(x => x.ToString()).ToArray()));

        IList<DriAuxFormat> formats;
        byte svideoInputCount = 0;
        byte compositeInputCount = 0;
        if (_auxService.GetAuxCapabilities(out formats, out svideoInputCount, out compositeInputCount))
        {
          Log.Log.Debug("DRI CC: auxiliary input info...");
          Log.Log.Debug("  supported formats     = {0}", string.Join(", ", formats.Select(x => x.ToString()).ToArray()));
          Log.Log.Debug("  S-video input count   = {0}", svideoInputCount);
          Log.Log.Debug("  composite input count = {0}", compositeInputCount);
        }
        else
        {
          Log.Log.Debug("DRI CC: auxiliary inputs not present/supported");
        }

        IList<DriEncoderAudioProfile> audioProfiles;
        IList<DriEncoderVideoProfile> videoProfiles;
        _encoderService.GetEncoderCapabilities(out audioProfiles, out videoProfiles);
        if (audioProfiles.Count > 0)
        {
          Log.Log.Debug("DRI CC: encoder audio profiles...");
          foreach (DriEncoderAudioProfile ap in audioProfiles)
          {
            Log.Log.Debug("  codec = {0}, bit depth = {1}, channel count = {2}, sample rate = {3} Hz", Enum.GetName(typeof(DriEncoderAudioAlgorithm), ap.AudioAlgorithmCode), ap.BitDepth, ap.NumberChannel, ap.SamplingRate);
          }
        }
        if (videoProfiles.Count > 0)
        {
          Log.Log.Debug("DRI CC: encoder video profiles...");
          foreach (DriEncoderVideoProfile vp in videoProfiles)
          {
            Log.Log.Debug("  hor. pixels = {0}, vert. pixels = {1}, aspect ratio = {2}, frame rate = {3}, {4}", vp.HorizontalSize, vp.VerticalSize, Enum.GetName(typeof(DriEncoderVideoAspectRatio), vp.AspectRatioInformation), Enum.GetName(typeof(DriEncoderVideoFrameRate), vp.FrameRateCode));
          }
        }

        uint maxAudioBitrate = 0;
        uint minAudioBitrate = 0;
        DriEncoderAudioMode audioBitrateMode = DriEncoderAudioMode.CBR;
        uint audioBitrateStepping = 0;
        uint audioBitrate = 0;
        byte audioProfileIndex = 0;
        bool isMuted = false;
        bool sapDetected = false; // second audio program (additional audio stream)
        bool sapActive = false;
        DriEncoderFieldOrder fieldOrder = DriEncoderFieldOrder.Higher;
        DriEncoderInputSelection source = DriEncoderInputSelection.Aux;
        bool noiseFilterActive = false;
        bool pulldownDetected = false;
        bool pulldownActive = false;
        uint maxVideoBitrate = 0;
        uint minVideoBitrate = 0;
        DriEncoderVideoMode videoBitrateMode = DriEncoderVideoMode.CBR;
        uint videoBitrate = 0;
        uint videoBitrateStepping = 0;
        byte videoProfileIndex = 0;
        _encoderService.GetEncoderParameters(out maxAudioBitrate, out minAudioBitrate, out audioBitrateMode,
          out audioBitrateStepping, out audioBitrate, out audioProfileIndex, out isMuted,
          out fieldOrder, out source, out noiseFilterActive, out pulldownDetected,
          out pulldownActive, out sapDetected, out sapActive, out maxVideoBitrate, out minVideoBitrate,
          out videoBitrateMode, out videoBitrate, out videoBitrateStepping, out videoProfileIndex);
        Log.Log.Debug("DRI CC: current encoder audio parameters...");
        Log.Log.Debug("  max bitrate  = {0} kbps", maxAudioBitrate / 1000);
        Log.Log.Debug("  min bitrate  = {0} kbps", minAudioBitrate / 1000);
        Log.Log.Debug("  bitrate mode = {0}", Enum.GetName(typeof(DriEncoderAudioMode), audioBitrateMode));
        Log.Log.Debug("  bitrate step = {0} kbps", audioBitrateStepping / 1000);
        Log.Log.Debug("  bitrate      = {0} kbps", audioBitrate / 1000);
        Log.Log.Debug("  profile      = {0}", audioProfileIndex);
        Log.Log.Debug("  is muted     = {0}", isMuted);
        Log.Log.Debug("  SAP detected = {0}", sapDetected);
        Log.Log.Debug("  SAP active   = {0}", sapActive);
        Log.Log.Debug("DRI CC: current encoder video parameters...");
        Log.Log.Debug("  max bitrate  = {0} kbps", maxVideoBitrate / 1000);
        Log.Log.Debug("  min bitrate  = {0} kbps", minVideoBitrate / 1000);
        Log.Log.Debug("  bitrate mode = {0}", Enum.GetName(typeof(DriEncoderVideoMode), videoBitrateMode));
        Log.Log.Debug("  bitrate step = {0} kbps", videoBitrateStepping / 1000);
        Log.Log.Debug("  bitrate      = {0} kbps", videoBitrate / 1000);
        Log.Log.Debug("  profile      = {0}", videoProfileIndex);
        Log.Log.Debug("  field order  = {0}", Enum.GetName(typeof(DriEncoderFieldOrder), fieldOrder));
        Log.Log.Debug("  source       = {0}", Enum.GetName(typeof(DriEncoderInputSelection), source));
        Log.Log.Debug("  noise filter = {0}", noiseFilterActive);
        Log.Log.Debug("  3:2 detected = {0}", pulldownDetected);
        Log.Log.Debug("  3:2 active   = {0}", pulldownActive);

        Log.Log.Debug("DRI CC: card status...");
        DriCasCardStatus status = DriCasCardStatus.Removed;
        string manufacturer = string.Empty;
        string version = string.Empty;
        bool isDst = false;
        uint eaLocationCode = 0;
        byte ratingRegion = 0;
        int timeZone = 0;
        _casService.GetCardStatus(out status, out manufacturer, out version, out isDst, out eaLocationCode, out ratingRegion, out timeZone);
        Log.Log.Debug("  status        = {0}", status.ToString());
        Log.Log.Debug("  manufacturer  = {0}", manufacturer);
        Log.Log.Debug("  version       = {0}", version);
        Log.Log.Debug("  time zone     = {0}", timeZone);
        Log.Log.Debug("  DST           = {0}", isDst);
        Log.Log.Debug("  EA loc. code  = {0}", eaLocationCode);  // EA = emergency alert
        Log.Log.Debug("  rating region = {0}", ratingRegion);

        Log.Log.Debug("DRI CC: diagnostic parameters...");
        string value = string.Empty;
        bool isVolatile = false;
        foreach (DriDiagParameter p in DriDiagParameter.Values)
        {
          _diagService.GetParameter(p, out value, out isVolatile);
          Log.Log.Debug("  {0}{1} = {2}", p.ToString(), isVolatile ? " [volatile]" : "", value);
        }
        if (_isCetonDevice)
        {
          Log.Log.Debug("DRI CC: Ceton-specific diagnostic parameters...");
          foreach (CetonDiagParameter p in CetonDiagParameter.Values)
          {
            _diagService.GetParameter(p, out value, out isVolatile);
            Log.Log.Debug("  {0}{1} = {2}", p.ToString(), isVolatile ? " [volatile]" : "", value);
          }
        }

        _canPause = false;
        IList<UpnpAvTransportAction> actions;
        if (_avTransportService.GetCurrentTransportActions((uint)_avTransportId, out actions))
        {
          Log.Log.Debug("DRI CC: supported AV transport actions = {0}", string.Join(", ", actions.Select(x => x.ToString()).ToArray()));
          if (actions.Contains(UpnpAvTransportAction.Pause))
          {
            _canPause = true;
          }
        }
        IList<UpnpAvStorageMedium> playMedia;
        IList<UpnpAvStorageMedium> recordMedia;
        IList<UpnpAvRecordQualityMode> recordQualityModes;
        _avTransportService.GetDeviceCapabilities((uint)_avTransportId, out playMedia, out recordMedia, out recordQualityModes);
        Log.Log.Debug("DRI CC: supported play media = {0}", string.Join(", ", playMedia.Select(x => x.ToString()).ToArray()));
        Log.Log.Debug("DRI CC: supported record media = {0}", string.Join(", ", recordMedia.Select(x => x.ToString()).ToArray()));
        Log.Log.Debug("DRI CC: supported record quality modes = {0}", string.Join(", ", recordQualityModes.Select(x => x.ToString()).ToArray()));

        Log.Log.Debug("DRI CC: media info...");
        uint trackCount = 0;
        string mediaDuration = string.Empty;
        string currentUri = string.Empty;
        string currentUriMetaData = string.Empty;
        string nextUri = string.Empty;
        string nextUriMetaData = string.Empty;
        UpnpAvStorageMedium playMedium = UpnpAvStorageMedium.Unknown;
        UpnpAvStorageMedium recordMedium = UpnpAvStorageMedium.Unknown;
        UpnpAvRecordMediumWriteStatus writeStatus = UpnpAvRecordMediumWriteStatus.NOT_IMPLEMENTED;
        _avTransportService.GetMediaInfo((uint)_avTransportId, out trackCount, out mediaDuration, out currentUri,
          out currentUriMetaData, out nextUri, out nextUriMetaData, out playMedium, out recordMedium, out writeStatus);
        Log.Log.Debug("  track count        = {0}", trackCount);
        Log.Log.Debug("  duration           = {0}", mediaDuration);
        Log.Log.Debug("  cur. URI           = {0}", currentUri);
        Log.Log.Debug("  cur. URI meta data = {0}", currentUriMetaData);
        Log.Log.Debug("  next URI           = {0}", nextUri);
        Log.Log.Debug("  next URI meta data = {0}", nextUriMetaData);
        Log.Log.Debug("  play medium        = {0}", playMedium);
        Log.Log.Debug("  record medium      = {0}", recordMedium);
        Log.Log.Debug("  write status       = {0}", writeStatus);
        _streamUrl = currentUri;

        /*
         * Ceton tuners set relCount and absCount to NOT_IMPLEMENTED. Those
         * parameters are meant to be type i4 (32 bit integers), so those
         * values are invalid. This is confirmed in the UPnP specs which state
         * such parameters should be set to the max value for i4.
        Log.Log.Debug("DRI CC: position info...");
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
        Log.Log.Debug("  track          = {0}", track);
        Log.Log.Debug("  duration       = {0}", duration);
        Log.Log.Debug("  meta data      = {0}", metaData);
        Log.Log.Debug("  URI            = {0}", uri);
        Log.Log.Debug("  relative time  = {0}", relTime);
        Log.Log.Debug("  absolute time  = {0}", absTime);
        Log.Log.Debug("  relative count = {0}", relCount);
        Log.Log.Debug("  absolute count = {0}", absCount);*/

        Log.Log.Debug("DRI CC: transport info...");
        UpnpAvTransportState transportState = UpnpAvTransportState.NO_MEDIA_PRESENT;
        UpnpAvTransportStatus transportStatus = UpnpAvTransportStatus.OK;
        string speed = string.Empty;
        _avTransportService.GetTransportInfo((uint)_avTransportId, out transportState, out transportStatus, out speed);
        Log.Log.Debug("  state       = {0}", transportState);
        Log.Log.Debug("  status      = {0}", transportStatus);
        Log.Log.Debug("  speed       = {0}", speed);

        UpnpAvCurrentPlayMode playMode;
        UpnpAvRecordQualityMode recordQualityMode;
        _avTransportService.GetTransportSettings((uint)_avTransportId, out playMode, out recordQualityMode);
        Log.Log.Debug("  play mode   = {0}", playMode);
        Log.Log.Debug("  record mode = {0}", recordQualityMode);
      }
      catch (Exception ex)
      {
        Log.Log.Error("DRI CC: failed to read device info\r\n{0}", ex);
        throw;
      }
    }

    #region graph handling

    /// <summary>
    /// Build the graph.
    /// </summary>
    public override void BuildGraph()
    {
      try
      {
        if (_graphState != GraphState.Idle)
        {
          Log.Log.Info("DRI CC: device already initialised");
          return;
        }

        bool useKeepAlive = !_isCetonDevice;
        Log.Log.Info("DRI CC: connect to device, keep-alive = {0}", useKeepAlive);
        _deviceConnection = _controlPoint.Connect(_descriptor.RootDescriptor, _descriptor.DeviceUUID, ResolveDataType, useKeepAlive);

        // services
        Log.Log.Debug("DRI CC: setup services");
        _tunerService = new TunerService(_deviceConnection.Device);
        _fdcService = new FdcService(_deviceConnection.Device);
        _auxService = new AuxService(_deviceConnection.Device);
        _encoderService = new EncoderService(_deviceConnection.Device);
        _casService = new CasService(_deviceConnection.Device);
        _muxService = new MuxService(_deviceConnection.Device);
        _securityService = new SecurityService(_deviceConnection.Device);
        _diagService = new DiagService(_deviceConnection.Device);
        _avTransportService = new AvTransportService(_deviceConnection.Device);
        _connectionManagerService = new ConnectionManagerService(_deviceConnection.Device);

        Log.Log.Debug("DRI CC: subscribe services");
        _stateVariableDelegate = new StateVariableChangedDlgt(OnStateVariableChanged);
        _tunerService.SubscribeStateVariables(_stateVariableDelegate);
        _auxService.SubscribeStateVariables(_stateVariableDelegate);
        _encoderService.SubscribeStateVariables(_stateVariableDelegate);
        _casService.SubscribeStateVariables(_stateVariableDelegate);
        _securityService.SubscribeStateVariables(_stateVariableDelegate);
        _avTransportService.SubscribeStateVariables(_stateVariableDelegate);
        _connectionManagerService.SubscribeStateVariables(_stateVariableDelegate);

        // Give time for the device to notify us about initial state variable values.
        // Attempting to continue with other actions now can overload the puny device
        // processors.
        Thread.Sleep(2000);

        int rcsId = -1;
        _connectionManagerService.PrepareForConnection(string.Empty, string.Empty, -1, UpnpConnectionDirection.Output, out _connectionId, out _avTransportId, out rcsId);
        Log.Log.Debug("DRI CC: PrepareForConnection, connection ID = {0}, AV transport ID = {1}", _connectionId, _avTransportId);

        // Check that the device is not already in use.
        if (IsTunerInUse())
        {
          throw new TvExceptionGraphBuildingFailed("DRI CC: tuner appears to be in use");
        }

        ReadDeviceInfo();

        Log.Log.Info("DRI CC: build graph");
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);
        AddTsWriterFilterToGraph();
        AddStreamSourceFilter();

        // This shouldn't be required, but it enables us to reuse TvCardDvbBase
        // and use CI menus for delivering messages from the CableCARD to the
        // user.
        _conditionalAccess = new ConditionalAccess(null, null, null, this);

        _graphState = GraphState.Created;
        if (_eventSignalLock == null)
        {
          _eventSignalLock = new ManualResetEvent(false);
        }
      }
      catch (Exception)
      {
        Dispose();
        throw;
      }
    }

    public override void PauseGraph()
    {
      if (_avTransportService != null && _gotTunerControl)
      {
        if (_canPause)
        {
          _avTransportService.Pause((uint)_avTransportId);
          _transportState = UpnpAvTransportState.PAUSED_PLAYBACK;
        }
        else
        {
          _avTransportService.Stop((uint)_avTransportId);
          _transportState = UpnpAvTransportState.STOPPED;
        }
      }
      base.PauseGraph();
    }

    public override void StopGraph()
    {
      if (_avTransportService != null && _gotTunerControl)
      {
        _avTransportService.Stop((uint)_avTransportId);
        _transportState = UpnpAvTransportState.STOPPED;
      }
      _gotTunerControl = false;
      base.StopGraph();
      _previousChannel = null;
    }

    /// <summary>
    /// Add the stream source filter to the graph.
    /// </summary>
    private void AddStreamSourceFilter()
    {
      Log.Log.Info("DRI CC: add source filter");
      _filterStreamSource = FilterGraphTools.AddFilterFromClsid(_graphBuilder, SourceFilterClsid,
                                                                  "DRI Source Filter");
      _mpeg2TransportStream = new AMMediaType();
      _mpeg2TransportStream.majorType = MediaType.Stream;
      _mpeg2TransportStream.subType = MediaSubType.Mpeg2Transport;
      _mpeg2TransportStream.unkPtr = IntPtr.Zero;
      _mpeg2TransportStream.sampleSize = 0;
      _mpeg2TransportStream.temporalCompression = false;
      _mpeg2TransportStream.fixedSizeSamples = true;
      _mpeg2TransportStream.formatType = FormatType.None;
      _mpeg2TransportStream.formatSize = 0;
      _mpeg2TransportStream.formatPtr = IntPtr.Zero;

      // (Re)connect the source filter to the first filter in the chain.
      IPin tsWriterInputPin = DsFindPin.ByDirection(_filterTsWriter, PinDirection.Input, 0);
      if (tsWriterInputPin == null)
      {
        throw new TvExceptionGraphBuildingFailed("DRI CC: failed to find TsWriter filter input pin");
      }
      IPin sourceOutputPin = null;
      try
      {
        sourceOutputPin = DsFindPin.ByDirection(_filterStreamSource, PinDirection.Output, 0);
        if (sourceOutputPin == null)
        {
          throw new TvExceptionGraphBuildingFailed("DRI CC: failed to find source filter output pin");
        }
        int hr = _graphBuilder.Connect(sourceOutputPin, tsWriterInputPin);
        if (hr != 0)
        {
          throw new TvExceptionGraphBuildingFailed(string.Format("DRI CC: failed to connect source filter into graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr)));
        }
        Log.Log.Debug("DRI CC: result = success");
      }
      finally
      {
        Release.ComObject("DRI TsWriter filter input pin", tsWriterInputPin);
        Release.ComObject("DRI source filter output pin", sourceOutputPin);
      }
    }

    /// <summary>
    /// Remove the stream source filter from the graph.
    /// </summary>
    private void RemoveStreamSourceFilter()
    {
      Log.Log.Info("DRI CC: remove source filter");
      if (_filterStreamSource != null)
      {
        if (_graphBuilder != null)
        {
          _graphBuilder.RemoveFilter(_filterStreamSource);
        }
        Release.ComObject("DRI source filter", _filterStreamSource);
        _filterStreamSource = null;
      }
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
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        return false;
      }
      // Major channel holds the virtual channel number that we use for tuning.
      return atscChannel.MajorChannel > 0;
    }

    protected override bool BeforeTune(IChannel channel)
    {
      if (_graphState == GraphState.Idle)
      {
        BuildGraph();
      }
      else if (IsTunerInUse())
      {
        throw new TvException("DRI CC: tuner appears to be in use");
      }
      _gotTunerControl = true;
      return true;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override ITvSubChannel SubmitTuneRequest(int subChannelId, IChannel channel, ITuneRequest tuneRequest, bool performTune)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        throw new TvException("DRI CC: received tune request for unsupported channel");
      }
      Log.Log.Info("DRI CC: tune channel {0} \"{1}\", sub channel ID {2}", atscChannel.MajorChannel, channel.Name, subChannelId);

      bool newSubChannel = false;
      BaseSubChannel subChannel;
      if (!_mapSubChannels.TryGetValue(subChannelId, out subChannel))
      {
        Log.Log.Debug("  new subchannel");
        newSubChannel = true;
        subChannelId = GetNewSubChannel(channel);
        subChannel = _mapSubChannels[subChannelId];
      }
      else
      {
        Log.Log.Debug("  existing subchannel {0}", subChannelId);
      }

      subChannel.CurrentChannel = channel;
      try
      {
        subChannel.OnBeforeTune();
        if (_interfaceEpgGrabber != null)
        {
          _interfaceEpgGrabber.Reset();
        }

        if (performTune)
        {
          Log.Log.Debug("DRI CC: tuning...");
          // Note that this call is not blocking, so usually the isPcrLocked
          // variable return value is false... then shortly afterwards we'll
          // receive an update if the tuner locks on signal. It may be possible
          // to use the Tuner service TimeToBlock SV to delay long enough to
          // get lock, however that is probably slower on average than
          // receiving asynchronous updates.
          _eventSignalLock.Reset();
          _casService.SetChannel((uint)atscChannel.MajorChannel, 0, DriCasCaptureMode.Live, out _tunerLocked);
        }
        else
        {
          Log.Log.Info("DRI CC: already tuned");
        }

        if (_transportState != UpnpAvTransportState.PLAYING)
        {
          Log.Log.Debug("DRI CC: play...");
          ((IFileSourceFilter)_filterStreamSource).Load(_streamUrl, _mpeg2TransportStream);
          _avTransportService.Play((uint)_avTransportId, "1");
          _transportState = UpnpAvTransportState.PLAYING;
        }

        _lastSignalUpdate = DateTime.MinValue;
        subChannel.OnAfterTune();
      }
      catch (Exception)
      {
        if (newSubChannel)
        {
          Log.Log.Info("DRI CC: tuning failed, removing subchannel");
          _mapSubChannels.Remove(subChannelId);
        }
        throw;
      }

      return subChannel;
    }

    #endregion

    #region signal

    public override void LockInOnSignal()
    {
      if (_tunerLocked)
      {
        Log.Log.Info("DRI CC: signal locked immediately");
        return;
      }
      _eventSignalLock.WaitOne(_parameters.TimeOutTune * 1000);
      if (!_tunerLocked)
      {
        throw new TvExceptionNoSignal("DRI CC: failed to lock in on signal");
      }
      Log.Log.Info("DRI CC: signal locked");
    }

    protected override void UpdateSignalQuality(bool force)
    {
      if (!force)
      {
        TimeSpan ts = DateTime.Now - _lastSignalUpdate;
        if (ts.TotalMilliseconds < 5000)
        {
          return;
        }
      }
      try
      {
        if (!GraphRunning() || CurrentChannel == null || _tunerService == null)
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }

        uint frequency = 0;
        DriTunerModulation modulation = DriTunerModulation.All;
        uint snr;
        _tunerService.GetTunerParameters(out _signalPresent, out frequency, out modulation, out _tunerLocked, out _signalLevel, out snr);
        _signalQuality = (int)snr;
        /*Log.Log.Debug("  carrier lock = {0}", _signalPresent);
        Log.Log.Debug("  frequency    = {0} kHz", frequency);
        Log.Log.Debug("  modulation   = {0}", modulation.ToString());
        Log.Log.Debug("  PCR lock     = {0}", _tunerLocked);
        Log.Log.Debug("  signal level = {0} dBmV", _signalLevel);
        Log.Log.Debug("  SNR          = {0} dB", _signalQuality);*/
        // Ideally we shouldn'i touch these readings, however DVB tuners tend
        // to use 0..100% ratings.
        if (_signalLevel > 100)
        {
          _signalLevel = 100;
        }
        else if (_signalLevel < 0)
        {
          _signalLevel = 0;
        }
        if (_signalQuality > 100)
        {
          _signalQuality = 100;
        }
        else if (_signalQuality < 0)
        {
          _signalQuality = 0;
        }
      }
      finally
      {
        _lastSignalUpdate = DateTime.Now;
      }
    }

    #endregion

    #region scanning

    public override ITVScanning ScanningInterface
    {
      get { return new ScannerDri(this, _fdcService); }
    }

    public override ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      Log.Log.Info("DRI CC: scan, sub channel ID {0}", subChannelId);
      BeforeTune(channel);

      bool newSubChannel = false;
      BaseSubChannel subChannel;
      if (!_mapSubChannels.TryGetValue(subChannelId, out subChannel))
      {
        Log.Log.Debug("  new subchannel");
        newSubChannel = true;
        subChannelId = GetNewSubChannel(channel);
        subChannel = _mapSubChannels[subChannelId];
      }
      else
      {
        Log.Log.Debug("  existing subchannel {0}", subChannelId);
      }

      try
      {
        if (_graphState == GraphState.Idle)
        {
          BuildGraph();
        }

        Log.Log.Debug("DRI CC: check OOB tuner lock...");
        uint bitrate = 0;
        bool isCarrierLocked = false;
        uint frequency = 0;
        bool spectrumInversion = false;
        IList<ushort> pids;
        _fdcService.GetFdcStatus(out bitrate, out isCarrierLocked, out frequency, out spectrumInversion, out pids);
        Log.Log.Debug("  carrier lock = {0}", isCarrierLocked);
        Log.Log.Debug("  frequency    = {0} kHz", frequency);
        if (_isCetonDevice)
        {
          string value = string.Empty;
          bool isVolatile = false;
          _diagService.GetParameter(CetonDiagParameter.OobSignalLevel, out value, out isVolatile);
          Log.Log.Debug("  signal level = {0}", value);
          _diagService.GetParameter(CetonDiagParameter.OobSnr, out value, out isVolatile);
          Log.Log.Debug("  SNR          = {0}", value);
        }

        if (!isCarrierLocked)
        {
          throw new TvExceptionNoSignal();
        }

        return subChannel;
      }
      catch (Exception)
      {
        if (newSubChannel)
        {
          Log.Log.Info("DRI CC: scan initialisation failed, removing subchannel");
          _mapSubChannels.Remove(subChannelId);
        }
        throw;
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
          _tunerLocked = (bool)newValue;
          if (_eventSignalLock != null)
          {
            _eventSignalLock.Set();
          }
        }
        else if (stateVariable.Name.Equals("CardStatus"))
        {
          DriCasCardStatus oldStatus = _cardStatus;
          _cardStatus = (DriCasCardStatus)(string)newValue;
          if (oldStatus != _cardStatus)
          {
            Log.Log.Info("DRI CC: device {0} CableCARD status update, old status = {1}, new status = {2}", _cardId, oldStatus, _cardStatus);
          }
        }
        else if (stateVariable.Name.Equals("CardMessage"))
        {
          if (!string.IsNullOrEmpty(newValue.ToString()))
          {
            Log.Log.Info("DRI CC: device {0} received message from the CableCARD, current status = {1}, message = {2}", _cardId, _cardStatus, newValue);
          }
        }
        else if (stateVariable.Name.Equals("MMIMessage"))
        {
          HandleMmiMessage((byte[])newValue);
        }
        else if (stateVariable.Name.Equals("DescramblingStatus"))
        {
          DriCasDescramblingStatus oldStatus = _descramblingStatus;
          _descramblingStatus = (DriCasDescramblingStatus)(string)newValue;
          if (oldStatus != _descramblingStatus)
          {
            Log.Log.Info("DRI CC: device {0} descrambling status update, old status = {1}, new status = {2}", _cardId, oldStatus, _descramblingStatus);
          }
        }
        else if (stateVariable.Name.Equals("DrmPairingStatus"))
        {
          DriSecurityPairingStatus oldStatus = _pairingStatus;
          _pairingStatus = (DriSecurityPairingStatus)(string)newValue;
          if (oldStatus != _pairingStatus)
          {
            Log.Log.Info("DRI CC: device {0} pairing status update, old status = {1}, new status = {2}", _cardId, oldStatus, _pairingStatus);
          }
        }
        else
        {
          Log.Log.Debug("DRI CC: device {0} state variable {1} for service {2} changed to {3}", _cardId, stateVariable.Name, stateVariable.ParentService.FullQualifiedName, newValue ?? "[null]");
        }
      }
      catch (Exception ex)
      {
        Log.Log.Error("DRI CC: failed to handle state variable change\r\n{0}", ex);
      }
    }

    private void HandleMmiMessage(byte[] message)
    {
      if (message == null || message.Length < 3)
      {
        return;
      }

      // DRI specification, page 32 table 6.2-30.
      //DVB_MMI.DumpBinary(message, 0, message.Length);
      byte dialogNumber = message[0];
      DriCasMmiDisplayType displayType = (DriCasMmiDisplayType)message[1];
      DriCasMmiAction action = (DriCasMmiAction)message[2];
      Log.Log.Debug("DRI CC: device {0} received MMI message", _cardId);
      Log.Log.Debug("  dialog number = {0}", dialogNumber);
      Log.Log.Debug("  display type  = {0}", displayType);
      Log.Log.Debug("  action        = {0}", action);
      if (action == DriCasMmiAction.Close)
      {
        if (_ciMenuCallbacks != null)
        {
          try
          {
            _ciMenuCallbacks.OnCiCloseDisplay(0);
          }
          catch (Exception ex)
          {
            Log.Log.Error("DRI CC: MMI OnCloseDisplay() exception\r\n{0}", ex);
          }
        }
        return;
      }

      if (message.Length < 5)
      {
        Log.Log.Error("DRI CC: invalid MMI message, open action with message length {0}", message.Length);
        return;
      }

      // CableCARD tuners construct an HTML page containing the messages that
      // we'd expect to receive directly from a DVB CAM via MMI. We retrieve
      // the HTML page contents and try and convert it into a menu as best as possible.
      int urlLength = (message[3] << 8) + message[4] - 1; // URL seems to be NULL terminated
      string uriString = System.Text.Encoding.ASCII.GetString(message, 5, urlLength);
      // Although the DRI specification states the URL will be relative, in
      // practice that is not always the case. Typical!
      Uri url;
      if (uriString.StartsWith("http"))
      {
        url = new Uri(uriString);
      }
      else
      {
        url = new Uri(
          new Uri(_deviceConnection.RootDescriptor.SSDPRootEntry.PreferredLink.DescriptionLocation),
          "/get_cc_url?" + uriString
        );
      }
      Log.Log.Debug("  URL           = {0}", url);


      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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
        Log.Log.Error("DRI CC: failed to retrieve actual MMI message content from {0}\r\n{1}", url, ex);
        return;
      }
      finally
      {
        response.Close();
      }

      // Reformat from pure HTML into title and menu items. This is quite
      // hacky, but we have no way to render HTML in MediaPortal.
      Log.Log.Debug(content);
      content = content.Replace("<b>", "").Replace("</b>", "").Replace("&nbsp;", " ");
      content = content.Substring(content.IndexOf("<body") + 5);
      content = content.Substring(content.IndexOf(">") + 1);
      content = content.Substring(0, content.IndexOf("</body>"));
      content = content.Trim();
      //Log.Log.Debug(content);
      _menuLinks.Clear();
      string[] sections = content.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
      if (_ciMenuCallbacks != null)
      {
        _ciMenuCallbacks.OnCiMenu(sections[0].Trim(), string.Empty, string.Empty, sections.Length - 1);
      }
      Log.Log.Debug("  title = {0}", sections[0].Trim());
      for (int i = 1; i < sections.Length; i++)
      {
        string item = sections[i].Trim();
        if (item.StartsWith("<a href=\"") && item.EndsWith("</a>"))
        {
          string itemUrl = item.Substring(item.IndexOf("\"") + 1);
          itemUrl = itemUrl.Substring(0, itemUrl.IndexOf("\""));
          item = item.Substring(item.IndexOf(">") + 1);
          item = item.Substring(0, item.IndexOf("<")).Trim();
          _menuLinks.Add(i - 1, itemUrl);
          Log.Log.Debug("  item {0} = {1} [{2}]", i, item, itemUrl);
        }
        else
        {
          Log.Log.Debug("  item {0} = {1}", i, item);
        }
        if (_ciMenuCallbacks != null)
        {
          _ciMenuCallbacks.OnCiMenuChoice(i - 1, item);
        }
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
      Log.Log.Error("DRI: resolve data type not supported, type name = {0}", dataTypeName);
      dataType = null;
      return true;
    }
  }
}
