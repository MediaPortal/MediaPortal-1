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
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Service;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Service;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.DeviceTree;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for analog tuners which
  /// implement the CableLabs/OpenCable Digital Receiver Interface.
  /// </summary>
  internal class TunerDriAnalog : TunerDriBase, IEncoder
  {
    private const uint AUDIO_SAMPLE_RATE_MATCH_PRECISION = 200;   // +/-

    private ServiceAux _serviceAux = null;                    // auxiliary analog inputs
    private ServiceEncoder _serviceEncoder = null;            // encoder for auxiliary inputs *and* tuner
    private CaptureSourceVideo _tunableSourcesVideo = CaptureSourceVideo.None;
    private CaptureSourceAudio _tunableSourcesAudio = CaptureSourceAudio.None;
    private ExternalTuner _externalTuner = new ExternalTuner();
    private volatile bool _isSignalLocked = false;

    #region encoder settings/info

    private EncoderInputSelection _inputSelection = null;

    // video settings/info
    private EncoderMode _videoBitRateMode = null;
    private uint _videoBitRate = 0;
    private uint _videoBitRateMinimum = 0;
    private uint _videoBitRateMaximum = 0;
    private uint _videoBitRateStepping = 0;
    private IList<EncoderVideoProfile> _videoProfiles = null;
    private byte _videoProfileIndex = 0;
    private EncoderFieldOrder _videoFieldOrder = null;
    private bool _isVideoNoiseFilterActive = false;
    private bool _isVideoPullDownDetected = false;
    private bool _isVideoPullDownActive = false;

    private ushort? _targetVideoHorizontalSize = null;
    private ushort? _targetVideoVerticalSize = null;
    private EncoderVideoFrameRate? _targetVideoFrameRate = null;
    private EncoderVideoProgressiveSequence? _targetVideoProgressiveSequence = null;

    // audio settings/info
    private EncoderMode _audioBitRateMode = null;
    private uint _audioBitRate = 0;
    private uint _audioBitRateMinimum = 0;
    private uint _audioBitRateMaximum = 0;
    private uint _audioBitRateStepping = 0;
    private IList<EncoderAudioProfile> _audioProfiles = null;
    private byte _audioProfileIndex = 0;
    private bool _isAudioMuted = false;
    private bool _isSapDetected = false;  // SAP = second audio program (additional audio stream)
    private bool _isSapActive = false;

    private EncoderAudioAlgorithm? _targetAudioAlgorithm = null;
    private uint? _targetAudioSamplingRate = null;
    private byte? _targetAudioChannelCount = null;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDriAnalog"/> class.
    /// </summary>
    /// <param name="descriptor">The UPnP device description.</param>
    /// <param name="tunerInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single tuner.</param>
    /// <param name="productInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single product.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the hardware.</param>
    /// <param name="controlPoint">The control point to use to connect to the device.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    public TunerDriAnalog(DeviceDescriptor descriptor, string tunerInstanceId, string productInstanceId, BroadcastStandard supportedBroadcastStandards, UPnPControlPoint controlPoint, ITunerInternal streamTuner)
      : base(descriptor, descriptor.DeviceUUID + "ATV", tunerInstanceId, productInstanceId, supportedBroadcastStandards, controlPoint, streamTuner)
    {
    }

    /// <summary>
    /// Handle UPnP evented state variable changes.
    /// </summary>
    /// <param name="stateVariable">The state variable that has changed.</param>
    /// <param name="newValue">The new value of the state variable.</param>
    protected override void OnStateVariableChanged(CpStateVariable stateVariable, object newValue)
    {
      try
      {
        if (stateVariable.Name.Equals("GenLock"))
        {
          bool oldStatus = _isSignalLocked;
          _isSignalLocked = (bool)newValue;
          if (oldStatus != _isSignalLocked)
          {
            this.LogInfo("DRI analog: auxiliary input lock status update, tuner ID = {0}, is locked = {1}", TunerId, _isSignalLocked);
          }
          return;
        }
        base.OnStateVariableChanged(stateVariable, newValue);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI analog: failed to handle state variable change, tuner ID = {0}", TunerId);
      }
    }

    private void ReadEncoderParameters()
    {
      this.LogDebug("DRI analog: encoder parameters");
      _serviceEncoder.GetEncoderParameters(out _audioBitRateMaximum, out _audioBitRateMinimum, out _audioBitRateMode,
                                            out _audioBitRateStepping, out _audioBitRate, out _audioProfileIndex, out _isAudioMuted,
                                            out _videoFieldOrder, out _inputSelection, out _isVideoNoiseFilterActive,
                                            out _isVideoPullDownDetected, out _isVideoPullDownActive,
                                            out _isSapDetected, out _isSapActive,
                                            out _videoBitRateMaximum, out _videoBitRateMinimum, out _videoBitRateMode,
                                            out _videoBitRate, out _videoBitRateStepping, out _videoProfileIndex);
      this.LogDebug("  input selection = {0}", _inputSelection);
      this.LogDebug("  video...");
      this.LogDebug("    bit-rate         = {0} kb/s", _videoBitRate / 1000);
      this.LogDebug("    bit-rate minimum = {0} kb/s", _videoBitRateMinimum / 1000);
      this.LogDebug("    bit-rate maximum = {0} kb/s", _videoBitRateMaximum / 1000);
      this.LogDebug("    bit-rate step    = {0} kb/s", _videoBitRateStepping / 1000);
      this.LogDebug("    bit-rate mode    = {0}", _videoBitRateMode);
      this.LogDebug("    profile index    = {0}", _videoProfileIndex);
      this.LogDebug("    field order      = {0}", _videoFieldOrder);
      this.LogDebug("    de-noise active? = {0}", _isVideoNoiseFilterActive);
      this.LogDebug("    3:2 detected?    = {0}", _isVideoPullDownDetected);
      this.LogDebug("    3:2 active?      = {0}", _isVideoPullDownActive);
      this.LogDebug("  audio...");
      this.LogDebug("    bit-rate         = {0} kb/s", _audioBitRate / 1000);
      this.LogDebug("    bit-rate minimum = {0} kb/s", _audioBitRateMinimum / 1000);
      this.LogDebug("    bit-rate maximum = {0} kb/s", _audioBitRateMaximum / 1000);
      this.LogDebug("    bit-rate step    = {0} kb/s", _audioBitRateStepping / 1000);
      this.LogDebug("    bit-rate mode    = {0}", _audioBitRateMode);
      this.LogDebug("    profile index    = {0}", _audioProfileIndex);
      this.LogDebug("    muted?           = {0}", _isAudioMuted);
      this.LogDebug("    SAP detected?    = {0}", _isSapDetected);
      this.LogDebug("    SAP active?      = {0}", _isSapActive);
    }

    #region configuration

    /// <summary>
    /// Create sensible default configuration based on hardware capabilities.
    /// </summary>
    private AnalogTunerSettings CreateDefaultConfiguration()
    {
      this.LogDebug("DRI analog: first detection, create default configuration");
      AnalogTunerSettings settings = new AnalogTunerSettings();

      settings.IdAnalogTunerSettings = TunerId;
      settings.IdVideoEncoder = null;
      settings.IdAudioEncoder = null;
      settings.EncoderBitRateTimeShifting = 100;
      settings.EncoderBitRatePeakTimeShifting = 100;
      settings.EncoderBitRateRecording = 100;
      settings.EncoderBitRatePeakRecording = 100;

      settings.ExternalTunerProgram = string.Empty;
      settings.ExternalTunerProgramArguments = string.Empty;
      settings.ExternalInputCountryId = CountryCollection.Instance.GetCountryByIsoCode("US").Id;
      settings.ExternalInputPhysicalChannelNumber = 7;

      settings.VideoStandard = (int)Mediaportal.TV.Server.Common.Types.Enum.AnalogVideoStandard.NtscM;
      settings.SupportedVideoStandards = (int)Mediaportal.TV.Server.Common.Types.Enum.AnalogVideoStandard.NtscM;

      // If all goes well, these settings will be updated below.
      settings.EncoderBitRateModeTimeShifting = (int)EncodeMode.Default;
      settings.EncoderBitRateModeRecording = (int)EncodeMode.Default;
      settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.Tuner;
      settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.Tuner;
      settings.FrameRate = (int)FrameRate.Automatic;
      settings.FrameSize = (int)FrameSize.Automatic;
      settings.SupportedAudioSources = (int)CaptureSourceAudio.Tuner;
      settings.SupportedFrameRates = (int)FrameRate.Automatic;
      settings.SupportedFrameSizes = (int)FrameSize.Automatic;
      settings.SupportedVideoSources = (int)CaptureSourceVideo.Tuner;

      // Minimal loading, enough to build configuration.
      DeviceConnection connection = null;
      ServiceAux serviceAux = null;
      ServiceConnectionManager serviceConnectionManager = null;
      ServiceEncoder serviceEncoder = null;
      int connectionId = -1;
      try
      {
        connection = Connect();
        serviceAux = new ServiceAux(connection.Device);
        serviceConnectionManager = new ServiceConnectionManager(connection.Device);
        serviceEncoder = new ServiceEncoder(connection.Device);

        int avTransportId;
        int rcsId;
        serviceConnectionManager.PrepareForConnection(string.Empty, string.Empty, -1, ConnectionDirection.Output, out connectionId, out avTransportId, out rcsId);

        SetDefaultExternalInputConfiguration(serviceAux, settings);
        SetDefaultVideoFrameConfiguration(serviceEncoder, settings);
        SetDefaultEncoderModeConfiguration(serviceEncoder, settings);
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "DRI analog: failed to create default configuration");
      }
      finally
      {
        if (serviceAux != null)
        {
          serviceAux.Dispose();
        }
        if (serviceEncoder != null)
        {
          serviceEncoder.Dispose();
        }
        if (serviceConnectionManager != null)
        {
          if (connectionId != -1)
          {
            serviceConnectionManager.ConnectionComplete(connectionId);
          }
          serviceConnectionManager.Dispose();
        }
        if (connection != null)
        {
          connection.Disconnect();
          connection.Dispose();
        }
      }

      return AnalogTunerSettingsManagement.SaveAnalogTunerSettings(settings);
    }

    private void SetDefaultExternalInputConfiguration(ServiceAux service, AnalogTunerSettings settings)
    {
      IList<AuxFormat> supportedFormats;
      byte inputCountSvideo;
      byte inputCountComposite;
      service.GetAuxCapabilities(out supportedFormats, out inputCountSvideo, out inputCountComposite);

      this.LogDebug("DRI analog: auxiliary input counts, s-video = {0}, composite = {1}", inputCountSvideo, inputCountComposite);
      // S-video and composite inputs are usually linked these days. Assume
      // there's one shared audio line input for each pair.
      CaptureSourceVideo supportedVideoSources = CaptureSourceVideo.None;
      CaptureSourceAudio supportedAudioSources = CaptureSourceAudio.None;
      if (SupportedBroadcastStandards.HasFlag(BroadcastStandard.AnalogTelevision))
      {
        supportedVideoSources |= CaptureSourceVideo.Tuner;
        supportedAudioSources |= CaptureSourceAudio.Tuner;
      }
      if (inputCountSvideo > 0)
      {
        settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.Svideo1;

        supportedVideoSources |= CaptureSourceVideo.Svideo1;
        supportedAudioSources |= CaptureSourceAudio.Line1;
        if (inputCountSvideo > 1)
        {
          supportedVideoSources |= CaptureSourceVideo.Svideo2;
          supportedAudioSources |= CaptureSourceAudio.Line2;
          if (inputCountSvideo > 2)
          {
            supportedVideoSources |= CaptureSourceVideo.Svideo3;
            supportedAudioSources |= CaptureSourceAudio.Line3;
            if (inputCountSvideo > 3)
            {
              this.LogWarn("DRI analog: {0} s-video video inputs detected, only 3 supported", inputCountSvideo);
            }
          }
        }
      }
      if (inputCountComposite > 0)
      {
        if (settings.ExternalInputSourceVideo <= 0)
        {
          settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.Composite1;
        }

        supportedVideoSources |= CaptureSourceVideo.Composite1;
        supportedAudioSources |= CaptureSourceAudio.Line1;
        if (inputCountComposite > 1)
        {
          supportedVideoSources |= CaptureSourceVideo.Composite2;
          supportedAudioSources |= CaptureSourceAudio.Line2;
          if (inputCountComposite > 2)
          {
            supportedVideoSources |= CaptureSourceVideo.Composite3;
            supportedAudioSources |= CaptureSourceAudio.Line3;
            if (inputCountComposite > 3)
            {
              this.LogWarn("DRI analog: {0} composite video inputs detected, only 3 supported", inputCountComposite);
            }
          }
        }
      }
    }

    private void SetDefaultVideoFrameConfiguration(ServiceEncoder service, AnalogTunerSettings settings)
    {
      IList<EncoderAudioProfile> audioProfiles;
      IList<EncoderVideoProfile> videoProfiles;
      service.GetEncoderCapabilities(out audioProfiles, out videoProfiles);
      if (audioProfiles != null && audioProfiles.Count > 0)
      {
        this.LogDebug("DRI analog: encoder audio profiles...");
        foreach (EncoderAudioProfile ap in audioProfiles)
        {
          this.LogDebug("  codec = {0}, bit depth = {1}, channel count = {2}, sample rate = {3} Hz", ap.AudioAlgorithmCode, ap.BitDepth, ap.NumberChannel, ap.SamplingRate);
        }
      }

      FrameRate supportedFrameRates = FrameRate.Automatic;
      FrameSize supportedFrameSizes = FrameSize.Automatic;
      if (videoProfiles != null && videoProfiles.Count > 0)
      {
        this.LogDebug("DRI analog: encoder video profiles...");
        foreach (EncoderVideoProfile vp in videoProfiles)
        {
          this.LogDebug("  resolution = {0}x{1}, aspect ratio = {2}, frame rate = {3}, interlacing = {4}", vp.HorizontalSize, vp.VerticalSize, vp.AspectRatioInformation, vp.FrameRateCode, vp.ProgressiveSequence);

          if (vp.FrameRateCode == EncoderVideoFrameRate.Fr23_976)
          {
            supportedFrameRates |= FrameRate.Fr23_976;
          }
          else if (vp.FrameRateCode == EncoderVideoFrameRate.Fr24)
          {
            supportedFrameRates |= FrameRate.Fr24;
          }
          else if (vp.FrameRateCode == EncoderVideoFrameRate.Fr29_97)
          {
            supportedFrameRates |= FrameRate.Fr29_97;
          }
          else if (vp.FrameRateCode == EncoderVideoFrameRate.Fr30)
          {
            supportedFrameRates |= FrameRate.Fr30;
          }
          else if (vp.FrameRateCode == EncoderVideoFrameRate.Fr59_94)
          {
            supportedFrameRates |= FrameRate.Fr59_94;
          }
          else if (vp.FrameRateCode == EncoderVideoFrameRate.Fr60)
          {
            supportedFrameRates |= FrameRate.Fr60;
          }
          else
          {
            this.LogWarn("DRI analog: unsupported encoder video profile frame rate, rate = {0}", vp.FrameRateCode);
          }

          // According to the DRI standard we only need to consider
          // resolutions specified in SCTE 43. Note that we don't support
          // 544x480, 528x480 or 352x480.
          if (vp.HorizontalSize == 1920 && vp.VerticalSize == 1080)
          {
            supportedFrameSizes = FrameSize.Fs1920_1080;
          }
          else if (vp.HorizontalSize == 1440 && vp.VerticalSize == 1080)
          {
            supportedFrameSizes = FrameSize.Fs1440_1080;
          }
          else if (vp.HorizontalSize == 1280 && vp.VerticalSize == 720)
          {
            supportedFrameSizes = FrameSize.Fs1280_720;
          }
          else if (vp.HorizontalSize == 720 && vp.VerticalSize == 480)
          {
            supportedFrameSizes = FrameSize.Fs720_480;
          }
          else if (vp.HorizontalSize == 704 && vp.VerticalSize == 480)
          {
            supportedFrameSizes = FrameSize.Fs704_480;
          }
          else if (vp.HorizontalSize == 640 && vp.VerticalSize == 480)
          {
            supportedFrameSizes = FrameSize.Fs640_480;
          }
          else
          {
            this.LogWarn("DRI analog: unsupported encoder video profile resolution, resolution = {0}x{1}", vp.HorizontalSize, vp.VerticalSize);
          }
        }
      }

      FrameRate frameRate = FrameRate.Automatic;
      if (supportedFrameRates.HasFlag(FrameRate.Fr60))
      {
        frameRate = FrameRate.Fr60;
      }
      else if (supportedFrameRates.HasFlag(FrameRate.Fr59_94))
      {
        frameRate = FrameRate.Fr59_94;
      }
      else if (supportedFrameRates.HasFlag(FrameRate.Fr30))
      {
        frameRate = FrameRate.Fr30;
      }
      else if (supportedFrameRates.HasFlag(FrameRate.Fr29_97))
      {
        frameRate = FrameRate.Fr29_97;
      }
      else if (supportedFrameRates.HasFlag(FrameRate.Fr24))
      {
        frameRate = FrameRate.Fr24;
      }
      else if (supportedFrameRates.HasFlag(FrameRate.Fr23_976))
      {
        frameRate = FrameRate.Fr23_976;
      }
      settings.FrameRate = (int)frameRate;
      settings.SupportedFrameRates = (int)supportedFrameRates;

      FrameSize frameSize = FrameSize.Automatic;
      if (supportedFrameSizes.HasFlag(FrameSize.Fs1920_1080))
      {
        frameSize = FrameSize.Fs1920_1080;
      }
      else if (supportedFrameSizes.HasFlag(FrameSize.Fs1440_1080))
      {
        frameSize = FrameSize.Fs1440_1080;
      }
      else if (supportedFrameSizes.HasFlag(FrameSize.Fs1280_720))
      {
        frameSize = FrameSize.Fs1280_720;
      }
      else if (supportedFrameSizes.HasFlag(FrameSize.Fs720_480))
      {
        frameSize = FrameSize.Fs720_480;
      }
      else if (supportedFrameSizes.HasFlag(FrameSize.Fs704_480))
      {
        frameSize = FrameSize.Fs704_480;
      }
      else if (supportedFrameSizes.HasFlag(FrameSize.Fs640_480))
      {
        frameSize = FrameSize.Fs640_480;
      }
      settings.FrameSize = (int)frameSize;
      settings.SupportedFrameSizes = (int)supportedFrameSizes;
    }

    private void SetDefaultEncoderModeConfiguration(ServiceEncoder service, AnalogTunerSettings settings)
    {
      IList<EncoderMode> videoModes;
      EncoderMode defaultVideoMode;
      IList<EncoderMode> audioModes;
      EncoderMode defaultAudioMode;
      service.GetEncoderModeDetails(out videoModes, out defaultVideoMode, out audioModes, out defaultAudioMode);
      this.LogDebug("DRI analog: encoder modes...");
      this.LogDebug("  video modes        = [{0}]", videoModes);
      this.LogDebug("  default video mode = {0}", defaultVideoMode);
      this.LogDebug("  audio modes        = [{0}]", audioModes);
      this.LogDebug("  default audio mode = {0}", defaultAudioMode);

      EncodeMode mode = EncodeMode.Default;
      if (videoModes.Contains(EncoderMode.VariableBitRate))
      {
        mode = EncodeMode.VariablePeakBitRate;
      }
      else if (videoModes.Contains(EncoderMode.AverageBitRate))
      {
        mode = EncodeMode.VariableBitRate;
      }
      else
      {
        mode = EncodeMode.ConstantBitRate;
      }
      settings.EncoderBitRateModeTimeShifting = (int)mode;
      settings.EncoderBitRateModeRecording = (int)mode;
    }

    #endregion

    #region encoder parameter conversion

    private static bool GetDriFrameRate(object valueCodecApi, out EncoderVideoFrameRate valueDri)
    {
      ulong codecApiFrameRate = (ulong)valueCodecApi;
      if (codecApiFrameRate == ((24000ul << 32) | 1001))
      {
        valueDri = EncoderVideoFrameRate.Fr23_976;
      }
      else if (codecApiFrameRate == ((24 << 32) | 1))
      {
        valueDri = EncoderVideoFrameRate.Fr24;
      }
      else if (codecApiFrameRate == ((30000ul << 32) | 1001))
      {
        valueDri = EncoderVideoFrameRate.Fr29_97;
      }
      else if (codecApiFrameRate == ((30ul << 32) | 1))
      {
        valueDri = EncoderVideoFrameRate.Fr30;
      }
      else if (codecApiFrameRate == ((60000ul << 32) | 1001))
      {
        valueDri = EncoderVideoFrameRate.Fr59_94;
      }
      else if (codecApiFrameRate == ((60ul << 32) | 1))
      {
        valueDri = EncoderVideoFrameRate.Fr60;
      }
      else
      {
        valueDri = EncoderVideoFrameRate.Fr60;
        return false;
      }
      return true;
    }

    private static bool GetCodecApiFrameRate(EncoderVideoFrameRate valueDri, out ulong valueCodecApi)
    {
      if (valueDri == EncoderVideoFrameRate.Fr23_976)
      {
        valueCodecApi = (24000ul << 32) | 1001;
      }
      else if (valueDri == EncoderVideoFrameRate.Fr24)
      {
        valueCodecApi = (24 << 32) | 1;
      }
      else if (valueDri == EncoderVideoFrameRate.Fr29_97)
      {
        valueCodecApi = (30000ul << 32) | 1001;
      }
      else if (valueDri == EncoderVideoFrameRate.Fr30)
      {
        valueCodecApi = (30ul << 32) | 1;
      }
      else if (valueDri == EncoderVideoFrameRate.Fr59_94)
      {
        valueCodecApi = (60000ul << 32) | 1001;
      }
      else if (valueDri == EncoderVideoFrameRate.Fr60)
      {
        valueCodecApi = (60ul << 32) | 1;
      }
      else
      {
        valueCodecApi = 0;
        return false;
      }
      return true;
    }

    private static bool GetDriEncoderModeForCodecApiMode(object valueCodecApi, out EncoderMode valueDri)
    {
      eAVEncCommonRateControlMode codecApiMode = (eAVEncCommonRateControlMode)(uint)valueCodecApi;
      if (codecApiMode == eAVEncCommonRateControlMode.CBR)
      {
        valueDri = EncoderMode.ConstantBitRate;
      }
      else if (codecApiMode == eAVEncCommonRateControlMode.PeakConstrainedVBR)
      {
        valueDri = EncoderMode.VariableBitRate;
      }
      else if (codecApiMode == eAVEncCommonRateControlMode.UnconstrainedVBR)
      {
        valueDri = EncoderMode.AverageBitRate;
      }
      else
      {
        valueDri = EncoderMode.VariableBitRate;
        return false;
      }
      return true;
    }

    private static bool GetCodecApiEncoderMode(EncoderMode valueDri, out eAVEncCommonRateControlMode valueCodecApi)
    {
      if (valueDri == EncoderMode.AverageBitRate)
      {
        valueCodecApi = eAVEncCommonRateControlMode.UnconstrainedVBR;
      }
      else if (valueDri == EncoderMode.ConstantBitRate)
      {
        valueCodecApi = eAVEncCommonRateControlMode.CBR;
      }
      else if (valueDri == EncoderMode.VariableBitRate)
      {
        valueCodecApi = eAVEncCommonRateControlMode.PeakConstrainedVBR;
      }
      else
      {
        valueCodecApi = eAVEncCommonRateControlMode.CBR;
        return false;
      }
      return true;
    }

    private static bool GetDriEncoderModeForEncoderApiMode(object valueEncoderApi, out EncoderMode valueDri)
    {
      VideoEncoderBitrateMode encoderApiMode = (VideoEncoderBitrateMode)(int)valueEncoderApi;
      if (encoderApiMode == VideoEncoderBitrateMode.ConstantBitRate)
      {
        valueDri = EncoderMode.ConstantBitRate;
      }
      else if (encoderApiMode == VideoEncoderBitrateMode.VariableBitRateAverage)
      {
        valueDri = EncoderMode.AverageBitRate;
      }
      else if (encoderApiMode == VideoEncoderBitrateMode.VariableBitRatePeak)
      {
        valueDri = EncoderMode.VariableBitRate;
      }
      else
      {
        valueDri = EncoderMode.VariableBitRate;
        return false;
      }
      return true;
    }

    private static bool GetEncoderApiEncoderMode(EncoderMode valueDri, out VideoEncoderBitrateMode valueEncoderApi)
    {
      if (valueDri == EncoderMode.AverageBitRate)
      {
        valueEncoderApi = VideoEncoderBitrateMode.VariableBitRateAverage;
      }
      else if (valueDri == EncoderMode.ConstantBitRate)
      {
        valueEncoderApi = VideoEncoderBitrateMode.ConstantBitRate;
      }
      else if (valueDri == EncoderMode.VariableBitRate)
      {
        valueEncoderApi = VideoEncoderBitrateMode.VariableBitRatePeak;
      }
      else
      {
        valueEncoderApi = VideoEncoderBitrateMode.ConstantBitRate;
        return false;
      }
      return true;
    }

    #endregion

    #region IEncoder members

    /// <summary>
    /// Determine whether the encoder can manipulate a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <returns><c>true</c> if the parameter can be manipulated, otherwise <c>false</c></returns>
    public bool IsParameterSupported(Guid parameterId)
    {
      this.LogDebug("DRI analog: is parameter supported, parameter = {0}", parameterId);
      if (
        parameterId == CodecApiParameter.AV_AUDIO_CHANNEL_COUNT ||
        parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE ||
        parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE ||
        parameterId == CodecApiParameter.AV_ENC_CODEC_TYPE ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_DEFAULT_UPPER_FIELD_DOMINANT ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_DISPLAY_DIMENSION ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_SCAN_TYPE ||
        parameterId == PropSetID.ENCAPIPARAM_BitRate ||
        parameterId == PropSetID.ENCAPIPARAM_BitRateMode
      )
      {
        this.LogDebug("DRI analog: supported");
        return true;
      }
      this.LogDebug("DRI analog: not supported");
      return false;
    }

    /// <summary>
    /// Get the extents and resolution for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="minimum">The minimum value that the parameter may take.</param>
    /// <param name="maximum">The maximum value that the parameter may take.</param>
    /// <param name="resolution">The magnitude of the smallest adjustment that can be applied to
    ///   the parameter. In most cases the value of the parameter should be a multiple of th.</param>
    /// <returns><c>true</c> if the parameter extents and resolution are successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterRange(Guid parameterId, out object minimum, out object maximum, out object resolution)
    {
      this.LogDebug("DRI analog: get parameter range, parameter = {0}", parameterId);
      minimum = null;
      maximum = null;
      resolution = null;

      if (
        parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE ||
        parameterId == CodecApiParameter.AV_ENC_CODEC_TYPE ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_DISPLAY_DIMENSION ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_SCAN_TYPE ||
        parameterId == PropSetID.ENCAPIPARAM_BitRateMode
      )
      {
        // Maybe this is a bit lazy for a few of the above parameters...
        this.LogError("DRI analog: cannot get range for enumerated parameters, parameter = {0}", parameterId);
        return false;
      }

      if (parameterId == CodecApiParameter.AV_AUDIO_CHANNEL_COUNT)
      {
        if (_audioProfiles == null || _audioProfiles.Count == 0)
        {
          this.LogError("DRI analog: audio encoder does not have any profiles");
          return false;
        }

        byte minimumChannelCount = 100;
        byte maximumChannelCount = 0;
        HashSet<byte> channelCounts = new HashSet<byte>();
        foreach (EncoderAudioProfile profile in _audioProfiles)
        {
          if (!channelCounts.Contains(profile.NumberChannel))
          {
            channelCounts.Add(profile.NumberChannel);
            if (profile.NumberChannel < minimumChannelCount)
            {
              minimumChannelCount = profile.NumberChannel;
            }
            else if (profile.NumberChannel > maximumChannelCount)
            {
              maximumChannelCount = profile.NumberChannel;
            }
          }
        }
        minimum = (byte)minimumChannelCount;
        maximum = (byte)maximumChannelCount;
        if (channelCounts.Count != 2)
        {
          resolution = (byte)1;
        }
        else
        {
          resolution = (byte)(maximumChannelCount - minimumChannelCount);
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE)
      {
        minimum = (uint)_audioBitRateMinimum;
        maximum = (uint)_audioBitRateMaximum;
        resolution = (uint)_audioBitRateStepping;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_BitRate)
      {
        minimum = (uint)_videoBitRateMinimum;
        maximum = (uint)_videoBitRateMaximum;
        resolution = (uint)_videoBitRateStepping;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_VIDEO_DEFAULT_UPPER_FIELD_DOMINANT)
      {
        // because VARIANT_TRUE is -1 and VARIANT_FALSE is 0
        minimum = true;
        maximum = false;
        resolution = true;
      }
      else
      {
        this.LogDebug("DRI analog: not supported");
        return false;
      }

      this.LogDebug("DRI analog: result = success, minimum = {0}, maximum = {1}, resolution = {2}", minimum, maximum, resolution);
      return true;
    }

    /// <summary>
    /// Get the accepted/supported values for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="values">The possible values that the parameter may take.</param>
    /// <returns><c>true</c> if the parameter values are successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterValues(Guid parameterId, out object[] values)
    {
      this.LogDebug("DRI analog: get parameter values, parameter = {0}", parameterId);
      values = null;

      if (
        parameterId == CodecApiParameter.AV_AUDIO_CHANNEL_COUNT ||
        parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE ||
        parameterId == CodecApiParameter.AV_ENC_CODEC_TYPE
      )
      {
        if (_audioProfiles == null || _audioProfiles.Count == 0)
        {
          values = new object[0];
        }
        else
        {
          List<object> tempValues = new List<object>(_audioProfiles.Count);
          HashSet<byte> channelCounts = new HashSet<byte>();
          HashSet<uint> sampleRates = new HashSet<uint>();
          HashSet<EncoderAudioAlgorithm> codecTypes = new HashSet<EncoderAudioAlgorithm>();
          foreach (EncoderAudioProfile profile in _audioProfiles)
          {
            if (parameterId == CodecApiParameter.AV_AUDIO_CHANNEL_COUNT)
            {
              if (!channelCounts.Contains(profile.NumberChannel))
              {
                channelCounts.Add(profile.NumberChannel);
                tempValues.Add((byte)profile.NumberChannel);
              }
            }
            else if (parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE)
            {
              if (!sampleRates.Contains(profile.SamplingRate))
              {
                sampleRates.Add(profile.SamplingRate);
                tempValues.Add((uint)profile.SamplingRate);
              }
            }
            else
            {
              if (!codecTypes.Contains(profile.AudioAlgorithmCode))
              {
                codecTypes.Add(profile.AudioAlgorithmCode);
                if (profile.AudioAlgorithmCode == EncoderAudioAlgorithm.DobyAc3)
                {
                  tempValues.Add(CodecApiAvEncCodecType.DOLBY_DIGITAL_CONSUMER.ToString());
                }
                else if (profile.AudioAlgorithmCode == EncoderAudioAlgorithm.Mpeg1Layer2)
                {
                  tempValues.Add(CodecApiAvEncCodecType.MPEG1_AUDIO.ToString());
                }
                else
                {
                  this.LogWarn("DRI analog: unexpected audio encoder algorithm, algorithm = {0}", profile.AudioAlgorithmCode);
                }
              }
            }
          }
          values = tempValues.ToArray();
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE)
      {
        uint valueCount = 1 + ((_audioBitRateMaximum - _audioBitRateMinimum) / _audioBitRateStepping);
        values = new object[valueCount];
        uint bitRate = _audioBitRateMinimum;
        for (uint i = 0; i < valueCount; i++)
        {
          values[i] = bitRate;
          bitRate += _audioBitRateStepping;
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_BitRate)
      {
        uint valueCount = 1 + ((_videoBitRateMaximum - _videoBitRateMinimum) / _videoBitRateStepping);
        values = new object[valueCount];
        uint bitRate = _videoBitRateMinimum;
        for (uint i = 0; i < valueCount; i++)
        {
          values[i] = bitRate;
          bitRate += _videoBitRateStepping;
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE || parameterId == PropSetID.ENCAPIPARAM_BitRateMode)
      {
        IList<EncoderMode> supportedModesVideo;
        EncoderMode defaultModeVideo;
        IList<EncoderMode> supportedModesAudio;
        EncoderMode defaultModeAudio;
        _serviceEncoder.GetEncoderModeDetails(out supportedModesVideo, out defaultModeVideo, out supportedModesAudio, out defaultModeAudio);
        List<object> tempValues = new List<object>(supportedModesVideo.Count);
        foreach (EncoderMode mode in supportedModesVideo)
        {
          if (parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE)
          {
            eAVEncCommonRateControlMode tempValue;
            if (GetCodecApiEncoderMode(mode, out tempValue))
            {
              tempValues.Add((uint)tempValue);
            }
            else
            {
              this.LogWarn("DRI analog: unexpected video encoder bit-rate mode, mode = {0}", mode);
            }
          }
          else
          {
            VideoEncoderBitrateMode tempValue;
            if (GetEncoderApiEncoderMode(mode, out tempValue))
            {
              tempValues.Add((int)tempValue);
            }
            else
            {
              this.LogWarn("DRI analog: unexpected video encoder bit-rate mode, mode = {0}", mode);
            }
          }
        }
        values = tempValues.ToArray();
      }
      else if (parameterId == CodecApiParameter.AV_ENC_VIDEO_DEFAULT_UPPER_FIELD_DOMINANT)
      {
        values = new object[2];
        values[0] = true;
        values[1] = false;
      }
      else if (
        parameterId == CodecApiParameter.AV_ENC_VIDEO_DISPLAY_DIMENSION ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_SCAN_TYPE
      )
      {
        if (_videoProfiles == null || _videoProfiles.Count == 0)
        {
          values = new object[0];
        }
        else
        {
          List<object> tempValues = new List<object>(_videoProfiles.Count);
          HashSet<uint> displayDimensions = new HashSet<uint>();
          HashSet<EncoderVideoFrameRate> frameRates = new HashSet<EncoderVideoFrameRate>();
          HashSet<EncoderVideoProgressiveSequence> sequenceTypes = new HashSet<EncoderVideoProgressiveSequence>();
          foreach (EncoderVideoProfile profile in _videoProfiles)
          {
            if (parameterId == CodecApiParameter.AV_ENC_VIDEO_DISPLAY_DIMENSION)
            {
              uint tempValue = (uint)(profile.HorizontalSize << 16) | profile.VerticalSize;
              if (!displayDimensions.Contains(tempValue))
              {
                displayDimensions.Add(tempValue);
                tempValues.Add(tempValue);
              }
            }
            else if (parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE)
            {
              if (!frameRates.Contains(profile.FrameRateCode))
              {
                frameRates.Add(profile.FrameRateCode);
                ulong tempValue;
                if (GetCodecApiFrameRate(profile.FrameRateCode, out tempValue))
                {
                  tempValues.Add(tempValue);
                }
                else
                {
                  this.LogWarn("DRI analog: unexpected video encoder frame rate, rate = {0}", profile.FrameRateCode);
                }
              }
            }
            else
            {
              if (!sequenceTypes.Contains(profile.ProgressiveSequence))
              {
                sequenceTypes.Add(profile.ProgressiveSequence);
                if (profile.ProgressiveSequence == EncoderVideoProgressiveSequence.Interlaced)
                {
                  tempValues.Add((uint)eAVEncVideoOutputScanType.Interlaced);
                }
                else if (profile.ProgressiveSequence == EncoderVideoProgressiveSequence.Progressive)
                {
                  tempValues.Add((uint)eAVEncVideoOutputScanType.Progressive);
                }
                else
                {
                  this.LogWarn("DRI analog: unexpected video encoder progressive sequence type, type = {0}", profile.ProgressiveSequence);
                }
              }
            }
          }
          values = tempValues.ToArray();
        }
      }
      else
      {
        this.LogDebug("DRI analog: not supported");
        return false;
      }

      this.LogDebug("DRI analog: result = success, values = {0}", string.Join(", ", values));
      return true;
    }

    /// <summary>
    /// Get the default value for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The default value for the parameter.</param>
    /// <returns><c>true</c> if the default parameter value is successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterDefaultValue(Guid parameterId, out object value)
    {
      this.LogDebug("DRI analog: get default value, parameter = {0}", parameterId);
      value = null;

      if (parameterId != CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE && parameterId != PropSetID.ENCAPIPARAM_BitRateMode)
      {
        // We don't know and have no way to find out what the defaults are.
        this.LogDebug("DRI analog: not supported");
        return false;
      }

      IList<EncoderMode> supportedModesVideo;
      EncoderMode defaultModeVideo;
      IList<EncoderMode> supportedModesAudio;
      EncoderMode defaultModeAudio;
      _serviceEncoder.GetEncoderModeDetails(out supportedModesVideo, out defaultModeVideo, out supportedModesAudio, out defaultModeAudio);
      if (parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE)
      {
        eAVEncCommonRateControlMode tempValue;
        if (!GetCodecApiEncoderMode(defaultModeVideo, out tempValue))
        {
          this.LogError("DRI analog: unexpected video encoder bit-rate mode, mode = {0}", defaultModeVideo);
          return false;
        }
        value = (uint)tempValue;
      }
      else
      {
        VideoEncoderBitrateMode tempValue;
        if (!GetEncoderApiEncoderMode(defaultModeVideo, out tempValue))
        {
          this.LogError("DRI analog: unexpected video encoder bit-rate mode, mode = {0}", defaultModeVideo);
          return false;
        }
        value = (int)tempValue;
      }

      this.LogDebug("DRI analog: result = success, value = {0}", value);
      return true;
    }

    /// <summary>
    /// Get the current value of a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The current value of the parameter.</param>
    /// <returns><c>true</c> if the current parameter value is successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterValue(Guid parameterId, out object value)
    {
      this.LogDebug("DRI analog: get value, parameter = {0}", parameterId);
      value = null;

      if (
        parameterId == CodecApiParameter.AV_AUDIO_CHANNEL_COUNT ||
        parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE ||
        parameterId == CodecApiParameter.AV_ENC_CODEC_TYPE
      )
      {
        if (_audioProfiles == null || _audioProfileIndex < 0 || _audioProfileIndex >= _audioProfiles.Count)
        {
          this.LogError("DRI analog: invalid current audio encoder profile selection, profile count = {0}, index = {1}", _audioProfiles == null ? 0 : _audioProfiles.Count, _audioProfileIndex);
          return false;
        }
        EncoderAudioProfile profile = _audioProfiles[_audioProfileIndex];
        if (parameterId == CodecApiParameter.AV_AUDIO_CHANNEL_COUNT)
        {
          value = (byte)profile.NumberChannel;
        }
        else if (parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE)
        {
          value = (uint)profile.SamplingRate;
        }
        else
        {
          if (profile.AudioAlgorithmCode == EncoderAudioAlgorithm.DobyAc3)
          {
            value = CodecApiAvEncCodecType.DOLBY_DIGITAL_CONSUMER.ToString();
          }
          else if (profile.AudioAlgorithmCode == EncoderAudioAlgorithm.Mpeg1Layer2)
          {
            value = CodecApiAvEncCodecType.MPEG1_AUDIO.ToString();
          }
          else
          {
            this.LogError("DRI analog: unexpected audio encoder algorithm, algorithm = {0}", profile.AudioAlgorithmCode);
            return false;
          }
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE)
      {
        value = (uint)_audioBitRate;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_BitRate)
      {
        value = (uint)_videoBitRate;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE)
      {
        eAVEncCommonRateControlMode tempValue;
        if (GetCodecApiEncoderMode(_videoBitRateMode, out tempValue))
        {
          value = (uint)tempValue;
        }
        else
        {
          this.LogError("DRI analog: unexpected video encoder bit-rate mode, mode = {0}", _videoBitRateMode);
          return false;
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_VIDEO_DEFAULT_UPPER_FIELD_DOMINANT)
      {
        value = _videoFieldOrder == EncoderFieldOrder.Higher;
      }
      else if (
        parameterId == CodecApiParameter.AV_ENC_VIDEO_DISPLAY_DIMENSION ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_SCAN_TYPE
      )
      {
        if (_videoProfiles == null || _videoProfileIndex < 0 || _videoProfileIndex >= _videoProfiles.Count)
        {
          this.LogError("DRI analog: invalid current video encoder profile selection, profile count = {0}, index = {1}", _videoProfiles == null ? 0 : _videoProfiles.Count, _videoProfileIndex);
          return false;
        }
        EncoderVideoProfile profile = _videoProfiles[_videoProfileIndex];
        if (parameterId == CodecApiParameter.AV_ENC_VIDEO_DISPLAY_DIMENSION)
        {
          value = (uint)((profile.HorizontalSize << 16) | profile.VerticalSize);
        }
        else if (parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE)
        {
          ulong tempValue;
          if (GetCodecApiFrameRate(profile.FrameRateCode, out tempValue))
          {
            value = tempValue;
          }
          else
          {
            this.LogError("DRI analog: unexpected video encoder frame rate, rate = {0}", profile.FrameRateCode);
            return false;
          }
        }
        else
        {
          if (profile.ProgressiveSequence == EncoderVideoProgressiveSequence.Interlaced)
          {
            value = (uint)eAVEncVideoOutputScanType.Interlaced;
          }
          else if (profile.ProgressiveSequence == EncoderVideoProgressiveSequence.Progressive)
          {
            value = (uint)eAVEncVideoOutputScanType.Progressive;
          }
          else
          {
            this.LogError("DRI analog: unexpected video encoder progressive sequence type, type = {0}", profile.ProgressiveSequence);
            return false;
          }
        }
      }
      else if (parameterId == PropSetID.ENCAPIPARAM_BitRateMode)
      {
        VideoEncoderBitrateMode tempValue;
        if (GetEncoderApiEncoderMode(_videoBitRateMode, out tempValue))
        {
          value = (int)tempValue;
        }
        else
        {
          this.LogError("DRI analog: unexpected video encoder bit-rate mode, mode = {0}", _videoBitRateMode);
          return false;
        }
      }
      else
      {
        this.LogDebug("DRI analog: not supported");
        return false;
      }

      this.LogDebug("DRI analog: result = success, value = {0}", value);
      return true;
    }

    /// <summary>
    /// Set the value of a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The new value for the parameter.</param>
    /// <returns><c>true</c> if the parameter value is successfully set, otherwise <c>false</c></returns>
    public bool SetParameterValue(Guid parameterId, object value)
    {
      this.LogDebug("DRI analog: set value, parameter = {0}, value = {1}", parameterId, value);

      uint audioBitRate = _audioBitRate;
      byte audioProfileIndex = _audioProfileIndex;
      uint videoBitRate = _videoBitRate;
      byte videoProfileIndex = _videoProfileIndex;
      EncoderFieldOrder videoFieldOrder = _videoFieldOrder;
      EncoderMode videoBitRateMode = _videoBitRateMode;

      EncoderVideoFrameRate targetFrameRate = EncoderVideoFrameRate.Fr59_94;
      bool setParameters = true;
      if (
        parameterId == CodecApiParameter.AV_AUDIO_CHANNEL_COUNT ||
        parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE ||
        parameterId == CodecApiParameter.AV_ENC_CODEC_TYPE
      )
      {
        if (_audioProfiles == null || _audioProfiles.Count == 0)
        {
          this.LogError("DRI analog: audio encoder does not have any profiles");
          return false;
        }

        // Find the audio profiles that match the target value.
        IList<KeyValuePair<byte, EncoderAudioProfile>> audioProfiles = new List<KeyValuePair<byte, EncoderAudioProfile>>(_audioProfiles.Count);
        for (byte i = 0; i < _audioProfiles.Count; i++)
        {
          EncoderAudioProfile audioProfile = _audioProfiles[i];
          if (
            (parameterId == CodecApiParameter.AV_AUDIO_CHANNEL_COUNT && audioProfile.NumberChannel == (byte)value) ||
            (
              parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE &&
              audioProfile.SamplingRate >= (uint)value - AUDIO_SAMPLE_RATE_MATCH_PRECISION &&
              audioProfile.SamplingRate <= (uint)value + AUDIO_SAMPLE_RATE_MATCH_PRECISION
            ) ||
            (
              parameterId == CodecApiParameter.AV_ENC_CODEC_TYPE &&
              (
                (
                  audioProfile.AudioAlgorithmCode == EncoderAudioAlgorithm.DobyAc3 &&
                  CodecApiAvEncCodecType.DOLBY_DIGITAL_CONSUMER == new Guid((string)value)
                ) ||
                (
                  audioProfile.AudioAlgorithmCode == EncoderAudioAlgorithm.Mpeg1Layer2 &&
                  CodecApiAvEncCodecType.MPEG1_AUDIO == new Guid((string)value)
                )
              )
            )
          )
          {
            audioProfiles.Add(new KeyValuePair<byte, EncoderAudioProfile>(i, audioProfile));
          }
        }

        if (audioProfiles.Count == 0)
        {
          this.LogError("DRI analog: audio encoder does not have any matching profiles");
          return false;
        }

        // Within those profiles, attempt to find a profile that matches all
        // the target values.
        if (parameterId != CodecApiParameter.AV_ENC_CODEC_TYPE && _targetAudioAlgorithm.HasValue)
        {
          bool found = false;
          for (int i = audioProfiles.Count - 1; i >= 0; i--)
          {
            EncoderAudioProfile audioProfile = audioProfiles[i].Value;
            if (audioProfile.AudioAlgorithmCode != _targetAudioAlgorithm.Value)
            {
              audioProfiles.RemoveAt(i);
            }
            else
            {
              found = true;
            }
          }
          if (!found)
          {
            this.LogError("DRI analog: audio encoder does not have any compatible profiles for the target codec type");
            return false;
          }
        }
        if (parameterId != CodecApiParameter.AV_AUDIO_CHANNEL_COUNT && _targetAudioChannelCount.HasValue)
        {
          bool found = false;
          for (int i = audioProfiles.Count - 1; i >= 0; i--)
          {
            EncoderAudioProfile audioProfile = audioProfiles[i].Value;
            if (audioProfile.NumberChannel != _targetAudioChannelCount.Value)
            {
              audioProfiles.RemoveAt(i);
            }
            else
            {
              found = true;
            }
          }
          if (!found)
          {
            this.LogError("DRI analog: audio encoder does not have any compatible profiles for the target channel count");
            return false;
          }
        }
        if (parameterId != CodecApiParameter.AV_AUDIO_SAMPLE_RATE && _targetAudioSamplingRate.HasValue)
        {
          bool found = false;
          for (int i = audioProfiles.Count - 1; i >= 0; i--)
          {
            EncoderAudioProfile audioProfile = audioProfiles[i].Value;
            if (
              audioProfile.SamplingRate < _targetAudioSamplingRate.Value - AUDIO_SAMPLE_RATE_MATCH_PRECISION ||
              audioProfile.SamplingRate > _targetAudioSamplingRate.Value + AUDIO_SAMPLE_RATE_MATCH_PRECISION
            )
            {
              audioProfiles.RemoveAt(i);
            }
            else
            {
              found = true;
            }
          }
          if (!found)
          {
            this.LogError("DRI analog: audio encoder does not have any compatible profiles for the target sample rate");
            return false;
          }
        }

        audioProfileIndex = audioProfiles[0].Key;
        setParameters = _audioProfileIndex != audioProfileIndex;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE)
      {
        audioBitRate = (uint)value;
        setParameters = _audioBitRate != audioBitRate;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_BitRate)
      {
        videoBitRate = (uint)value;
        setParameters = _videoBitRate != videoBitRate;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE)
      {
        if (!GetDriEncoderModeForCodecApiMode(value, out videoBitRateMode))
        {
          this.LogError("DRI analog: unsupported encoder bit-rate mode, mode = {0}", (eAVEncCommonRateControlMode)(uint)value);
          return false;
        }
        setParameters = _videoBitRateMode != videoBitRateMode;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_VIDEO_DEFAULT_UPPER_FIELD_DOMINANT)
      {
        if ((bool)value)
        {
          videoFieldOrder = EncoderFieldOrder.Higher;
        }
        else
        {
          videoFieldOrder = EncoderFieldOrder.Lower;
        }
        setParameters = _videoFieldOrder != videoFieldOrder;
      }
      else if (
        parameterId == CodecApiParameter.AV_ENC_VIDEO_DISPLAY_DIMENSION ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE ||
        parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_SCAN_TYPE
      )
      {
        if (_videoProfiles == null || _videoProfiles.Count == 0)
        {
          this.LogError("DRI analog: video encoder does not have any profiles");
          return false;
        }

        bool isTargetFrameRateAvailable = false;
        if (parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE)
        {
          isTargetFrameRateAvailable = GetDriFrameRate(value, out targetFrameRate);
        }

        // Find the video profiles that match the target value.
        IList<KeyValuePair<byte, EncoderVideoProfile>> videoProfiles = new List<KeyValuePair<byte, EncoderVideoProfile>>(_videoProfiles.Count);
        for (byte i = 0; i < _videoProfiles.Count; i++)
        {
          EncoderVideoProfile videoProfile = _videoProfiles[i];
          if (
            (
              parameterId == CodecApiParameter.AV_ENC_VIDEO_DISPLAY_DIMENSION &&
              videoProfile.HorizontalSize == (((uint)value) >> 16) &&
              videoProfile.VerticalSize == (((uint)value) & 0xffff)
            ) ||
            (
              parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE &&
              isTargetFrameRateAvailable &&
              videoProfile.FrameRateCode == targetFrameRate
            ) ||
            (
              parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_SCAN_TYPE &&
              (
                (
                  videoProfile.ProgressiveSequence == EncoderVideoProgressiveSequence.Interlaced &&
                  (uint)eAVEncVideoOutputScanType.Interlaced == (uint)value
                ) ||
                (
                  videoProfile.ProgressiveSequence == EncoderVideoProgressiveSequence.Progressive &&
                  (uint)eAVEncVideoOutputScanType.Progressive == (uint)value
                )
              )
            )
          )
          {
            videoProfiles.Add(new KeyValuePair<byte, EncoderVideoProfile>(i, videoProfile));
          }
        }

        if (videoProfiles.Count == 0)
        {
          this.LogError("DRI analog: video encoder does not have any matching profiles");
          return false;
        }

        // Within those profiles, attempt to find a profile that matches all
        // the target values.
        if (parameterId != CodecApiParameter.AV_ENC_VIDEO_DISPLAY_DIMENSION && _targetVideoHorizontalSize.HasValue)
        {
          bool found = false;
          for (int i = videoProfiles.Count - 1; i >= 0; i--)
          {
            EncoderVideoProfile videoProfile = videoProfiles[i].Value;
            if (
              videoProfile.HorizontalSize != _targetVideoHorizontalSize.Value ||
              videoProfile.VerticalSize != _targetVideoVerticalSize.Value
            )
            {
              videoProfiles.RemoveAt(i);
            }
            else
            {
              found = true;
            }
          }
          if (!found)
          {
            this.LogError("DRI analog: video encoder does not have any compatible profiles for the target display dimension");
            return false;
          }
        }
        if (parameterId != CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE && _targetVideoFrameRate.HasValue)
        {
          bool found = false;
          for (int i = videoProfiles.Count - 1; i >= 0; i--)
          {
            EncoderVideoProfile videoProfile = videoProfiles[i].Value;
            if (videoProfile.FrameRateCode != _targetVideoFrameRate.Value)
            {
              videoProfiles.RemoveAt(i);
            }
            else
            {
              found = true;
            }
          }
          if (!found)
          {
            this.LogError("DRI analog: video encoder does not have any compatible profiles for the target frame rate");
            return false;
          }
        }
        if (parameterId != CodecApiParameter.AV_ENC_VIDEO_OUTPUT_SCAN_TYPE && _targetVideoProgressiveSequence.HasValue)
        {
          bool found = false;
          for (int i = videoProfiles.Count - 1; i >= 0; i--)
          {
            EncoderVideoProfile videoProfile = videoProfiles[i].Value;
            if (videoProfile.ProgressiveSequence != _targetVideoProgressiveSequence.Value)
            {
              videoProfiles.RemoveAt(i);
            }
            else
            {
              found = true;
            }
          }
          if (!found)
          {
            this.LogError("DRI analog: video encoder does not have any compatible profiles for the target scan type");
            return false;
          }
        }

        videoProfileIndex = videoProfiles[0].Key;
        setParameters = _videoProfileIndex != videoProfileIndex;
      }
      else if (parameterId == PropSetID.ENCAPIPARAM_BitRateMode)
      {
        if (!GetDriEncoderModeForEncoderApiMode(value, out videoBitRateMode))
        {
          this.LogError("DRI analog: unsupported encoder bit-rate mode, mode = {0}", (VideoEncoderBitrateMode)(int)value);
          return false;
        }
        setParameters = _videoBitRateMode != videoBitRateMode;
      }
      else
      {
        this.LogDebug("DRI analog: not supported");
        return false;
      }

      if (!setParameters)
      {
        this.LogDebug("DRI analog: current value matches target");
      }
      else
      {
        try
        {
          _serviceEncoder.SetEncoderParameters(_audioBitRateMode, audioBitRate, audioProfileIndex, _isAudioMuted,
                                                videoFieldOrder, _inputSelection, _isVideoNoiseFilterActive, _isVideoPullDownActive, _isSapActive,
                                                videoBitRateMode, videoBitRate, videoProfileIndex);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "DRI analog: failed to set encoder parameters, parameter = {0}, value = {1}", parameterId, value);
          return false;
        }

        _audioBitRate = audioBitRate;
        _audioProfileIndex = audioProfileIndex;
        _videoBitRate = videoBitRate;
        _videoProfileIndex = videoProfileIndex;
        _videoFieldOrder = videoFieldOrder;
        _videoBitRateMode = videoBitRateMode;
      }

      if (parameterId == CodecApiParameter.AV_AUDIO_CHANNEL_COUNT)
      {
        _targetAudioChannelCount = (byte)value;
      }
      else if (parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE)
      {
        _targetAudioSamplingRate = (uint)value;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_CODEC_TYPE)
      {
        if (CodecApiAvEncCodecType.DOLBY_DIGITAL_CONSUMER == new Guid((string)value))
        {
          _targetAudioAlgorithm = EncoderAudioAlgorithm.DobyAc3;
        }
        else
        {
          _targetAudioAlgorithm = EncoderAudioAlgorithm.Mpeg1Layer2;
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_VIDEO_DISPLAY_DIMENSION)
      {
        _targetVideoHorizontalSize = (ushort)(((uint)value) >> 16);
        _targetVideoVerticalSize = (ushort)(((uint)value) & 0xffff);
      }
      else if (parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_FRAME_RATE)
      {
        _targetVideoFrameRate = targetFrameRate;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_VIDEO_OUTPUT_SCAN_TYPE)
      {
        if ((uint)value == (uint)eAVEncVideoOutputScanType.Progressive)
        {
          _targetVideoProgressiveSequence = EncoderVideoProgressiveSequence.Progressive;
        }
        else
        {
          _targetVideoProgressiveSequence = EncoderVideoProgressiveSequence.Interlaced;
        }
      }

      this.LogDebug("DRI analog: result = success");
      return true;
    }

    #endregion

    #region ITunerInternal overrides

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(Tuner configuration)
    {
      this.LogDebug("DRI analog: reload configuration");
      base.ReloadConfiguration(configuration);

      if (configuration.AnalogTunerSettings == null)
      {
        configuration.AnalogTunerSettings = CreateDefaultConfiguration();
      }

      _tunableSourcesVideo = (CaptureSourceVideo)configuration.AnalogTunerSettings.SupportedVideoSources;
      _tunableSourcesVideo |= CaptureSourceVideo.TunerDefault;
      _tunableSourcesAudio = (CaptureSourceAudio)configuration.AnalogTunerSettings.SupportedAudioSources;
      _tunableSourcesAudio |= CaptureSourceAudio.Automatic | CaptureSourceAudio.TunerDefault;

      _externalTuner.ReloadConfiguration(configuration.AnalogTunerSettings);
    }

    #endregion

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if (!base.CanTune(channel))
      {
        return false;
      }

      // Check that the selected inputs are available for capture channels.
      ChannelCapture captureChannel = channel as ChannelCapture;
      if (captureChannel == null)
      {
        return true;
      }
      return _tunableSourcesVideo.HasFlag(captureChannel.VideoSource) && _tunableSourcesAudio.HasFlag(captureChannel.AudioSource);
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("DRI analog: perform tuning");

      IChannel tuneChannel = _externalTuner.Tune(channel);
      bool isSuccessful = true;
      try
      {
        EncoderInputSelection inputSelection = EncoderInputSelection.Aux;
        ChannelAnalogTv analogTvChannel = tuneChannel as ChannelAnalogTv;
        if (analogTvChannel != null)
        {
          // Usually only physical channel number will be set. Frequency is an
          // override. Where not overriden, convert the physical channel number
          // to a frequency in accordance with the USA standard cable band
          // plan. Assume that the tuner expects us to specify the analog video
          // carrier frequency.
          int frequency = analogTvChannel.Frequency;
          if (frequency <= 0)
          {
            if (analogTvChannel.PhysicalChannelNumber > 158)
            {
              throw new TvException("Physical channel number {0} is invalid.", analogTvChannel.PhysicalChannelNumber);
            }
            else if (analogTvChannel.PhysicalChannelNumber >= 100)
            {
              // jumbo
              frequency = 649250 + (6000 * (analogTvChannel.PhysicalChannelNumber - 100));
            }
            else if (analogTvChannel.PhysicalChannelNumber >= 95)
            {
              // mid 1
              frequency = 91250 + (6000 * (analogTvChannel.PhysicalChannelNumber - 95));
            }
            else if (analogTvChannel.PhysicalChannelNumber >= 23)
            {
              // super, hyper, ultra
              frequency = 217250 + (6000 * (analogTvChannel.PhysicalChannelNumber - 23));
            }
            else if (analogTvChannel.PhysicalChannelNumber >= 14)
            {
              // mid 2
              frequency = 121250 + (6000 * (analogTvChannel.PhysicalChannelNumber - 14));
            }
            else if (analogTvChannel.PhysicalChannelNumber >= 7)
            {
              // high
              frequency = 175250 + (6000 * (analogTvChannel.PhysicalChannelNumber - 7));
            }
            else if (analogTvChannel.PhysicalChannelNumber >= 5)
            {
              // low 2
              frequency = 77250 + (6000 * (analogTvChannel.PhysicalChannelNumber - 5));
            }
            else if (analogTvChannel.PhysicalChannelNumber >= 2)
            {
              // low 1
              frequency = 55250 + (6000 * (analogTvChannel.PhysicalChannelNumber - 2));
            }
            else
            {
              throw new TvException("Physical channel number {0} is invalid.", analogTvChannel.PhysicalChannelNumber);
            }

            // FCC regulatory quirks
            // https://www.gpo.gov/fdsys/pkg/CFR-2011-title47-vol4/xml/CFR-2011-title47-vol4-sec76-612.xml
            if (
              (frequency >= 118000 && frequency <= 137000) ||
              (frequency >= 225000 && frequency <= 328600) ||
              (frequency >= 335400 && frequency <= 400000)
            )
            {
              frequency += 13;  // +/- 12.5 kHz
            }
            else if (
              (frequency >= 108000 && frequency <= 118000) ||
              (frequency >= 328600 && frequency <= 335400)
            )
            {
              frequency += 25;  // +/- 25 kHz
            }
          }
          TuneByFrequency(analogTvChannel.Frequency, TunerModulation.Ntsc);
          inputSelection = EncoderInputSelection.Tuner;
        }
        else
        {
          this.LogDebug("DRI analog: tune by auxiliary input");
          ChannelCapture captureChannel = tuneChannel as ChannelCapture;
          if (captureChannel == null)
          {
            throw new TvException("Received request to tune incompatible channel.");
          }

          AuxInputType auxInputType = AuxInputType.Video;
          byte inputNumber = 1;
          if (captureChannel.VideoSource == CaptureSourceVideo.Composite2)
          {
            inputNumber = 2;
          }
          else if (captureChannel.VideoSource == CaptureSourceVideo.Composite3)
          {
            inputNumber = 3;
          }
          else
          {
            auxInputType = AuxInputType.Svideo;
            if (captureChannel.VideoSource == CaptureSourceVideo.Svideo2)
            {
              inputNumber = 2;
            }
            else if (captureChannel.VideoSource == CaptureSourceVideo.Svideo3)
            {
              inputNumber = 3;
            }
            else if (
              captureChannel.VideoSource != CaptureSourceVideo.Composite1 &&
              captureChannel.VideoSource != CaptureSourceVideo.Svideo1
            )
            {
              throw new TvException("Received request to tune incompatible channel.");
            }
          }

          AuxFormat actualFormat;
          bool isLocked;
          _serviceAux.SetAuxParameters(auxInputType, inputNumber, AuxFormat.NtscM, out actualFormat, out isLocked);
          _isSignalLocked = isLocked;
        }

        if (_inputSelection != inputSelection)
        {
          _serviceEncoder.SetEncoderParameters(_audioBitRateMode, _audioBitRate, _audioProfileIndex, _isAudioMuted,
                                                _videoFieldOrder, inputSelection, _isVideoNoiseFilterActive, _isVideoPullDownActive, _isSapActive,
                                                _videoBitRateMode, _videoBitRate, _videoProfileIndex);
          _inputSelection = inputSelection;
        }
      }
      catch
      {
        isSuccessful = false;
        throw;
      }
      finally
      {
        if (isSuccessful)
        {
          _streamChannel.IsEncrypted = channel.IsEncrypted;
          _streamChannel.LogicalChannelNumber = channel.LogicalChannelNumber;
          _streamChannel.MediaType = channel.MediaType;
          _streamChannel.Name = channel.Name;
          _streamChannel.OriginalNetworkId = -1;
          _streamChannel.Provider = _vendor.ToString();

          // These details are valid for the ATI tuner, which is currently the only
          // DRI-compatible tuner with analog TV and/or auxiliary input support.
          _streamChannel.TransportStreamId = 0;
          _streamChannel.ProgramNumber = 1;
          _streamChannel.PmtPid = 0x40;
        }
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
      this.LogDebug("DRI analog: perform loading");
      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Analog;
      }

      IList<ITunerExtension> extensions = base.PerformLoading(streamFormat);

      this.LogDebug("DRI analog: setup additional services");
      _serviceAux = new ServiceAux(_deviceConnection.Device);
      _serviceEncoder = new ServiceEncoder(_deviceConnection.Device);
      _serviceAux.SubscribeStateVariables(_stateVariableChangeDelegate, _eventSubscriptionFailDelegate);
      _serviceEncoder.SubscribeStateVariables(_stateVariableChangeDelegate, _eventSubscriptionFailDelegate);

      ReadEncoderParameters();
      return extensions;
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformUnloading(bool isFinalising = false)
    {
      this.LogDebug("DRI analog: perform unloading");

      if (!isFinalising)
      {
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
      }

      base.PerformUnloading(isFinalising);
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
      if (_serviceAux == null || _inputSelection != EncoderInputSelection.Aux)
      {
        base.GetSignalStatus(out isLocked, out isPresent, out strength, out quality, onlyGetLock);
        return;
      }

      isLocked = false;
      isPresent = false;
      strength = 0;
      quality = 0;
      if (onlyGetLock)
      {
        // The signal locked indicator is evented so should be current and usable.
        isLocked = _isSignalLocked;
        return;
      }

      try
      {
        isPresent = isLocked;
        strength = SignalStrengthDecibelsToPercentage((int)_serviceAux.QueryStateVariable("SignalLevel"));
        quality = SignalQualitySnrToPercentage((int)_serviceAux.QueryStateVariable("SNR"));
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI analog: exception updating signal status");
      }
    }

    #endregion

    #endregion
  }
}