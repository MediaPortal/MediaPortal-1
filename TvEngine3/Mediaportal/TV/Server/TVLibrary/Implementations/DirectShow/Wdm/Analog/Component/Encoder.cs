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
using System.Text.RegularExpressions;
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Multiplexer;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component
{
  /// <summary>
  /// A WDM analog DirectShow encoder graph component.
  /// </summary>
  internal class Encoder : BaseComponent
  {
    #region constants

    private static readonly Regex HAUPPAUGE_SAA7164_DEVICE_ID = new Regex("pci#ven_1131&dev_7164&subsys_[0-9a-f]{4}0070");

    private static readonly IEnumerable<Guid> CYBERLINK_MULTIPLEXERS = new List<Guid>
    {
      // name = CyberLink MPEG Muxer
      // package = PowerCinema, Lenovo ShuttleCenter
      // file name = MDMpgMux.ax
      new Guid(0x370e9701, 0x9dc5, 0x42c8, 0xbe, 0x29, 0x4e, 0x75, 0xf0, 0x62, 0x9e, 0xed),

      // name = CyberLink MPEG Muxer
      // package = AzureWave/Twinhan DigitalTV
      // file name = THMpgMux.ax
      new Guid(0x6770e328, 0x9b73, 0x40c5, 0x91, 0xe6, 0xe2, 0xf3, 0x21, 0xae, 0xde, 0x57),

      // name = CyberLink MPEG Muxer
      // package = KWorld HyperMedia
      // file name = MpgMux.ax
      new Guid(0x4df35815, 0x79c5, 0x44c8, 0x87, 0x53, 0x84, 0x7d, 0x5c, 0x9c, 0x3c, 0xf5),

      // name = PDR MPEG Muxer
      // package = PowerDirector
      // file name = PDMpgMux.ax
      new Guid(0x7f2bbeaf, 0xe11c, 0x4d39, 0x90, 0xe8, 0x93, 0x8f, 0xb5, 0xa8, 0x60, 0x45),

      // name = CyberLink MPEG Muxer
      // package = PowerEncoder
      // file name = PEMpgMux.ax
      new Guid(0xffbc4098, 0xfef1, 0x4207, 0x82, 0x2e, 0x57, 0x4d, 0xd3, 0x11, 0x93, 0xee),

      // name = PP MPEG Muxer
      // package = PowerProducer
      // file name = MpgMux.ax
      new Guid(0x6708234e, 0xddfe, 0x4b29, 0xa5, 0x9e, 0xe5, 0x5a, 0x3f, 0xe5, 0x2b, 0x69),

      // name = CyberLink MPEG Muxer
      // package = Medion???
      // file name = MpgMux.ax
      new Guid(0xbc650178, 0x0de4, 0x47df, 0xaf, 0x50, 0xbb, 0xd9, 0xc7, 0xae, 0xf5, 0xa9),

      // name = CyberLink MPEG Muxer
      // package = Power2Go
      // file name = P2GMpgMux.ax
      new Guid(0xcf6ed441, 0xfc79, 0x4f1a, 0x9d, 0x91, 0x4a, 0xe0, 0x1c, 0x57, 0x0b, 0x81),

      // name = CyberLink MPEG Muxer
      // package = PowerVCR II
      // file name = MpgMux.ax
      new Guid(0x4b5c6bc0, 0xd60e, 0x11d2, 0x8f, 0x3f, 0x00, 0x80, 0xc8, 0x4e, 0x98, 0x06),

      // name = CyberLink MPEG Muxer
      // package = Dell Media Experience
      // file name = PDMpgMux.ax
      new Guid(0x2ff4bfb8, 0x7d35, 0x44cf, 0xaa, 0x67, 0xc5, 0x96, 0x61, 0xdf, 0x89, 0x29)
    };

    #endregion

    #region variables

    /// <summary>
    /// The upstream capture component.
    /// </summary>
    private Capture _capture = null;

    /// <summary>
    /// The hardware video (or combined video-audio) encoder device.
    /// </summary>
    private DsDevice _deviceEncoderVideo = null;

    /// <summary>
    /// The hardware audio encoder device.
    /// </summary>
    private DsDevice _deviceEncoderAudio = null;

    /// <summary>
    /// The hardware multiplexer/encoder device.
    /// </summary>
    private DsDevice _deviceMultiplexer = null;

    /// <summary>
    /// The hardware or software video (or combined video-audio) encoder/multiplexer filter.
    /// </summary>
    private IBaseFilter _filterEncoderVideo = null;

    /// <summary>
    /// The hardware or software audio encoder filter.
    /// </summary>
    private IBaseFilter _filterEncoderAudio = null;

    /// <summary>
    /// The hardware multiplexer/encoder or software multiplexer filter.
    /// </summary>
    private IBaseFilter _filterMultiplexer = null;

    /// <summary>
    /// The MediaPortal transport stream multiplexer filter.
    /// </summary>
    private IBaseFilter _filterTsMultiplexer = null;

    /// <summary>
    /// The vertical blanking interval (VBI) codec or tee/sink-to-sink filter.
    /// </summary>
    private IBaseFilter _filterVbiSplitter = null;

    /// <summary>
    /// The closed captions (CC) decoder filter.
    /// </summary>
    private IBaseFilter _filterCcDecoder = null;

    /// <summary>
    /// The world standard teletext (WST) codec filter.
    /// </summary>
    private IBaseFilter _filterWstCodec = null;

    #region settings

    /// <summary>
    /// Indicator for whether this encoder is associated with a Hauppauge
    /// product designed around the NXP SAA7164 chipset (HVR-22**).
    /// </summary>
    private bool _isHauppaugeSaa7164Based = false;

    /// <summary>
    /// Enable or disable vertical blanking interval data handling.
    /// </summary>
    private bool _isVbiEnabled = true;

    /// <summary>
    /// The preferred software video encoder.
    /// </summary>
    private VideoEncoder _encoderVideo = null;

    /// <summary>
    /// The preferred software audio encoder.
    /// </summary>
    private AudioEncoder _encoderAudio = null;

    /// <summary>
    /// A binary mask specifying which (if any) TsMuxer inputs to dump.
    /// </summary>
    private int _tsMuxerInputDumpMask = 0;

    #endregion

    #endregion

    #region properties

    /// <summary>
    /// Get the TS multiplexer filter.
    /// </summary>
    public IBaseFilter TsMultiplexerFilter
    {
      get
      {
        return _filterTsMultiplexer;
      }
    }

    #endregion

    #region configuration

    /// <summary>
    /// Reload the component's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public void ReloadConfiguration(TVDatabase.Entities.Tuner configuration)
    {
      this.LogDebug("WDM analog encoder: reload configuration");

      if (configuration == null)
      {
        _encoderVideo = null;
        _encoderAudio = null;
        _tsMuxerInputDumpMask = 0;
      }
      else
      {
        _encoderVideo = configuration.AnalogTunerSettings.VideoEncoder;
        _encoderAudio = configuration.AnalogTunerSettings.AudioEncoder;
        _tsMuxerInputDumpMask = configuration.TsMuxerInputDumpMask;
      }

      this.LogDebug("  software video encoder  = {0}", _encoderVideo == null ? "[auto]" : string.Format("{0} ({1})", _encoderVideo.Name, _encoderVideo.ClassId));
      this.LogDebug("  software audio encoder  = {0}", _encoderAudio == null ? "[auto]" : string.Format("{0} ({1})", _encoderAudio.Name, _encoderAudio.ClassId));
      this.LogDebug("  TsMuxer input dump mask = 0x{0:x}", _tsMuxerInputDumpMask);

      ApplyTsMuxerConfig();
    }

    #endregion

    #region graph building

    /// <summary>
    /// Load the encoder component.
    /// </summary>
    /// <remarks>
    /// Add and connect the encoder and VBI filters into the graph. This
    /// function handles a huge variety of filter and pin connection
    /// arrangements, including:
    /// - 1 or 2 capture filters
    /// - 1 or 2 encoder filters
    /// - hardware or software encoders
    /// - hardware or software multiplexer filter
    /// - combined or separate video and/or audio paths throughout
    /// The objective is to connect a combination of required (ie. hardware)
    /// and optional filters (including teletext and/or closed captions
    /// filters), such that we can eventually connect our transport stream
    /// multiplexer.
    /// </remarks>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    /// <param name="productInstanceId">A common identifier shared by the tuner's components.</param>
    /// <param name="capture">The capture component.</param>
    public void PerformLoading(IFilterGraph2 graph, string productInstanceId, Capture capture)
    {
      this.LogDebug("WDM analog encoder: perform loading");
      _capture = capture;
      IPin capturePin = capture.PinOutputCapture;
      IPin videoPin = null;
      bool isVideoPinCapture = false;
      IPin audioPin = null;
      bool isAudioPinCapture = false;
      IPin closedCaptionsPin = null;
      IDictionary<PinType, IPin> encoderOutputPins = new Dictionary<PinType, IPin>(5);
      IDictionary<PinType, IPin> vbiOutputPins = new Dictionary<PinType, IPin>(5);
      try
      {
        // ------------------------------------------------
        // STAGE 1
        // Add VBI splitter and/or decoder filters for
        // closed captions, teletext, VPS and WSS support.
        // ------------------------------------------------
        IPin vbiPin = capture.PinOutputVbi;
        if (vbiPin != null && AddAndConnectVbiFilters(graph, vbiPin, vbiOutputPins))
        {
          _isVbiEnabled = true;
          vbiOutputPins.TryGetValue(PinType.ClosedCaptions, out closedCaptionsPin);
          vbiOutputPins.Remove(PinType.ClosedCaptions);
        }

        // ------------------------------------------------
        // STAGE 2
        // Connect hardware or software encoder(s) if
        // required.
        // ------------------------------------------------
        bool isCyberLinkEncoder = false;
        if (capturePin == null)
        {
          videoPin = capture.GetVideoOutputDetail(out isVideoPinCapture);
          if (videoPin != null)
          {
            this.LogDebug("WDM analog encoder: connect capture video output");
            AddAndConnectEncoder(graph, true, videoPin, isVideoPinCapture, productInstanceId, out _filterEncoderVideo, out _deviceEncoderVideo, out isCyberLinkEncoder);
            if (_filterEncoderVideo != null)
            {
              videoPin = null;
            }
          }

          audioPin = capture.GetAudioOutputDetail(out isAudioPinCapture);
          if (audioPin != null)
          {
            if (_filterEncoderVideo != null)
            {
              if (_deviceEncoderVideo != null)
              {
                this.LogDebug("WDM analog encoder: connect capture audio output to existing encoder");
                if (FilterGraphTools.ConnectFilterWithPin(graph, audioPin, PinDirection.Output, _filterEncoderVideo))
                {
                  _filterEncoderAudio = _filterEncoderVideo;
                  _deviceEncoderAudio = _deviceEncoderVideo;
                  audioPin = null;
                }
                else
                {
                  this.LogDebug("WDM analog encoder: separate encoder required");
                }
              }
              else if (ConnectSoftwareVideoEncoderExtraPins(graph, audioPin, closedCaptionsPin, _filterEncoderVideo))
              {
                _filterEncoderAudio = _filterEncoderVideo;
                audioPin = null;
              }
            }

            if (_filterEncoderAudio == null)
            {
              this.LogDebug("WDM analog encoder: connect capture audio output");
              AddAndConnectEncoder(graph, false, audioPin, isAudioPinCapture, productInstanceId, out _filterEncoderAudio, out _deviceEncoderAudio, out isCyberLinkEncoder);
              if (_filterEncoderAudio != null)
              {
                audioPin = null;
              }
            }
          }
        }
        else if (PinRequiresHardwareFilterConnection(capturePin))
        {
          this.LogDebug("WDM analog encoder: hardware encoder(s) and/or multiplexer required");
          bool result;
          if (capture.VideoFilter == null)
          {
            result = FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, capturePin, FilterCategory.WDMStreamingEncoderDevices, out _filterEncoderAudio, out _deviceEncoderAudio, productInstanceId);
          }
          else
          {
            result = FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, capturePin, FilterCategory.WDMStreamingEncoderDevices, out _filterEncoderVideo, out _deviceEncoderVideo, productInstanceId);
          }
          if (result)
          {
            capturePin = null;
            if (capture.VideoFilter == capture.AudioFilter)
            {
              _filterEncoderAudio = _filterEncoderVideo;
              _deviceEncoderAudio = _deviceEncoderVideo;
            }
          }
        }

        // ------------------------------------------------
        // STAGE 3
        // Find the encoder outputs.
        // ------------------------------------------------
        PinType findPinTypes = PinType.Video;
        if (_filterEncoderVideo != null)
        {
          if (_filterEncoderVideo == _filterEncoderAudio)
          {
            findPinTypes |= PinType.Audio | PinType.Capture;
          }
          else if (_filterEncoderAudio == null)
          {
            findPinTypes |= PinType.Capture;
          }
          FindOutputPins("video encoder", _filterEncoderVideo, findPinTypes, encoderOutputPins, ref isVideoPinCapture, ref isAudioPinCapture);
          encoderOutputPins.TryGetValue(PinType.Video, out videoPin);
          if (findPinTypes.HasFlag(PinType.Audio))
          {
            encoderOutputPins.TryGetValue(PinType.Audio, out audioPin);
          }
        }
        if (_filterEncoderAudio != null && _filterEncoderAudio != _filterEncoderVideo)
        {
          findPinTypes = PinType.Audio;
          if (_filterEncoderVideo == null)
          {
            findPinTypes |= PinType.Capture;
          }
          FindOutputPins("audio encoder", _filterEncoderAudio, findPinTypes, encoderOutputPins, ref isVideoPinCapture, ref isAudioPinCapture);
          encoderOutputPins.TryGetValue(PinType.Audio, out audioPin);
        }
        if (findPinTypes.HasFlag(PinType.Capture))
        {
          encoderOutputPins.TryGetValue(PinType.Capture, out capturePin);
        }

        // ------------------------------------------------
        // STAGE 4
        // Connect a hardware or software multiplexer if
        // required.
        // ------------------------------------------------
        AddAndConnectMultiplexer(graph, capturePin, videoPin, audioPin, productInstanceId, out _filterMultiplexer, out _deviceMultiplexer);
        if (_filterMultiplexer == null && _deviceEncoderAudio != null && isCyberLinkEncoder)
        {
          // A CyberLink multiplexer is required if a CyberLink audio encoder
          // is used. If the muxer isn't in the graph, the audio encoder causes
          // an access violation exception when you attempt to start the graph.
          // My guess is that the encoder interacts with the muxer. I tried to
          // mimic interfaces requested via QueryInterface() with our TS
          // multiplexer but ultimately I never managed to make the encoder
          // work without the CyberLink multiplexer.
          if (!AddAndConnectCyberLinkMultiplexer(graph, videoPin, audioPin, out _filterMultiplexer))
          {
            throw new TvException("Failed to add and connect CyberLink multiplexer.");
          }
        }

        // ------------------------------------------------
        // STAGE 5
        // Finally, complete the graph with the TS
        // multiplexer.
        // ------------------------------------------------
        List<IPin> pinsToConnect = new List<IPin>(5);
        if (_filterMultiplexer != null)
        {
          if (encoderOutputPins.Remove(PinType.Capture))
          {
            Release.ComObject("WDM analog encoder capture output pin", ref capturePin);
          }
          FindOutputPins("multiplexer", _filterMultiplexer, PinType.Capture, encoderOutputPins, ref isVideoPinCapture, ref isAudioPinCapture);
          encoderOutputPins.TryGetValue(PinType.Capture, out capturePin);
          if (capturePin == null)
          {
            throw new TvException("Failed to find capture output on multiplexer.");
          }
          pinsToConnect.Add(capturePin);
        }
        else if (capturePin != null)
        {
          pinsToConnect.Add(capturePin);
        }
        else
        {
          pinsToConnect.Add(videoPin);
          pinsToConnect.Add(audioPin);
        }
        pinsToConnect.AddRange(vbiOutputPins.Values);
        pinsToConnect.Add(capture.PinOutputRds);
        try
        {
          _filterTsMultiplexer = FilterGraphTools.AddFilterFromFile(graph, "TsMuxer.ax", typeof(MediaPortalTsMultiplexer).GUID, "MediaPortal TS Multiplexer");
        }
        catch (Exception ex)
        {
          this.LogError(ex, "WDM analog encoder: failed to add TS multiplexer filter to graph");
          throw;
        }
        ConnectTsMultiplexer(graph, pinsToConnect);

        _isHauppaugeSaa7164Based = _deviceEncoderVideo != null && HAUPPAUGE_SAA7164_DEVICE_ID.IsMatch(_deviceEncoderVideo.DevicePath);
      }
      finally
      {
        foreach (IPin pin in encoderOutputPins.Values)
        {
          IPin tempPin = pin;
          Release.ComObject("WDM analog encoder encoder output pin", ref tempPin);
        }
        foreach (IPin pin in vbiOutputPins.Values)
        {
          IPin tempPin = pin;
          Release.ComObject("WDM analog encoder VBI output pin", ref tempPin);
        }
        Release.ComObject("WDM analog encoder closed captions output pin", ref closedCaptionsPin);
      }
    }

    private bool PinRequiresHardwareFilterConnection(IPin pin)
    {
      ICollection<RegPinMedium> mediums = FilterGraphTools.GetPinMediums(pin);
      if (mediums == null || mediums.Count == 0)
      {
        return false;
      }
      return true;
    }

    private delegate bool OnAvailableInputPin(int index, string name, IPin pin);

    private void EnumerateUnconnectedFilterInputs(IBaseFilter filter, OnAvailableInputPin availableInputPinDelegate)
    {
      IEnumPins pinEnum;
      int hr = filter.EnumPins(out pinEnum);
      TvExceptionDirectShowError.Throw(hr, "Failed to obtain pin enumerator for unconnected filter.");

      int pinIndex = 0;
      int pinCount = 0;
      IPin[] pins = new IPin[2];
      while (pinEnum.Next(1, pins, out pinCount) == (int)NativeMethods.HResult.S_OK && pinCount == 1)
      {
        IPin pin = pins[0];
        try
        {
          PinInfo pinInfo;
          hr = pin.QueryPinInfo(out pinInfo);
          TvExceptionDirectShowError.Throw(hr, "Failed to obtain pin information for unconnected filter pin {0}.", pinIndex);
          Release.PinInfo(ref pinInfo);

          if (pinInfo.dir != PinDirection.Input)
          {
            pinIndex--;     // compensate for the undesirable increment in the finally clause
            continue;
          }

          IPin connectedPin;
          hr = pin.ConnectedTo(out connectedPin);
          if (hr == (int)NativeMethods.HResult.S_OK && connectedPin != null)
          {
            Release.ComObject("WDM analog encoder unconnected filter connected pin", ref connectedPin);
            continue;
          }

          if (availableInputPinDelegate(pinIndex, pinInfo.name, pin))
          {
            break;
          }
        }
        finally
        {
          Release.ComObject("WDM analog encoder unconnected filter input pin", ref pin);
          pinIndex++;
        }
      }
    }

    private bool AddAndConnectVbiFilters(IFilterGraph2 graph, IPin vbiPin, IDictionary<PinType, IPin> vbiOutputPins)
    {
      this.LogDebug("WDM analog encoder: add and connect VBI splitter");

      // Note: the exact names of these filters varies depending on OS language.
      bool isNewCodec = true;
      try
      {
        _filterVbiSplitter = FilterGraphTools.AddFilterFromCategory(graph, TveGuid.AM_KS_CATEGORY_MULTI_VBI_CODEC, delegate(DsDevice device)
        {
          return device.Name.Contains("VBI");     // VBI Codec
        });
        if (_filterVbiSplitter == null)
        {
          isNewCodec = false;
          if (Environment.OSVersion.Version.Major < 6)
          {
            _filterVbiSplitter = FilterGraphTools.AddFilterFromCategory(graph, FilterCategory.AMKSSplitter, delegate
            {
              return true;                        // Tee/Sink-to-Sink Converter
            });
          }
          if (_filterVbiSplitter == null)
          {
            this.LogWarn("WDM analog encoder: failed to add VBI splitter");
            return false;
          }
        }
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "WDM analog encoder: failed to add VBI splitter, is new = {0}", isNewCodec);
        return false;
      }

      IPin pin = null;
      try
      {
        pin = DsFindPin.ByDirection(_filterVbiSplitter, PinDirection.Input, 0);
        int hr = graph.ConnectDirect(vbiPin, pin, null);
        TvExceptionDirectShowError.Throw(hr, "Failed to connect VBI splitter.");
        this.LogDebug("WDM analog encoder:   connected!");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "WDM analog encoder: failed to connect VBI splitter, is new = {0}", isNewCodec);
        graph.RemoveFilter(_filterVbiSplitter);
        Release.ComObject("WDM analog encoder VBI splitter filter candidate", ref _filterVbiSplitter);
        return false;
      }
      finally
      {
        Release.ComObject("WDM analog encoder VBI splitter input pin", ref pin);
      }

      bool dummyBool = false;
      if (isNewCodec)
      {
        FindOutputPins("VBI splitter", _filterVbiSplitter, PinType.AnyVbiSubType, vbiOutputPins, ref dummyBool, ref dummyBool);
        return true;
      }

      // The CC decoder must be connected before the WST codec. It won't
      // connect if you connect the WST codec first.
      try
      {
        this.LogDebug("WDM analog encoder: add and connect closed captions decoder");
        _filterCcDecoder = FilterGraphTools.AddFilterFromCategory(graph, FilterCategory.AMKSVBICodec, delegate(DsDevice device)
        {
          return device.Name.Contains("CC");    // CC Decoder
        });
        if (_filterCcDecoder == null)
        {
          this.LogWarn("WDM analog encoder: failed to add closed captions decoder");
        }
        else
        {
          FilterGraphTools.ConnectFilters(graph, _filterVbiSplitter, 0, _filterCcDecoder, 0);
          FindOutputPins("closed captions decoder", _filterCcDecoder, PinType.ClosedCaptions, vbiOutputPins, ref dummyBool, ref dummyBool);
        }
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "WDM analog encoder: failed to add and/or connect closed captions decoder");
      }

      try
      {
        this.LogDebug("WDM analog encoder: add and connect world standard teletext codec");
        _filterWstCodec = FilterGraphTools.AddFilterFromCategory(graph, FilterCategory.AMKSVBICodec, delegate(DsDevice device)
        {
          return device.Name.Contains("WST");   // WST Codec
        });
        if (_filterWstCodec == null)
        {
          this.LogWarn("WDM analog encoder: failed to add world standard teletext codec");
        }
        else
        {
          // Yes, you must connect output pin 0 again. The Tee/Sink-to-Sink
          // Converter is weird. It moves the CC decoder connection to pin 1.
          FilterGraphTools.ConnectFilters(graph, _filterVbiSplitter, 0, _filterWstCodec, 0);
          FindOutputPins("world standard teletext codec", _filterWstCodec, PinType.Teletext, vbiOutputPins, ref dummyBool, ref dummyBool);
        }
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "WDM analog encoder: failed to add and/or connect world standard teletext codec");
      }
      return true;
    }

    private void AddAndConnectEncoder(IFilterGraph2 graph, bool isVideo, IPin pin, bool isCapturePin, string productInstanceId, out IBaseFilter filter, out DsDevice device, out bool isCyberLink)
    {
      filter = null;
      device = null;
      isCyberLink = false;

      if (PinRequiresHardwareFilterConnection(pin))
      {
        this.LogDebug("WDM analog encoder:   hardware encoder (and/or multiplexer) required");
        FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, pin, FilterCategory.WDMStreamingEncoderDevices, out filter, out device, productInstanceId);
        return;
      }
      else if (isCapturePin)
      {
        this.LogDebug("WDM analog encoder:   encoder not required for capture output");
        return;
      }

      this.LogDebug("WDM analog encoder:   software encoder required");
      System.Collections.IEnumerable softwareEncoders;
      bool isPreferredEncoder = false;
      if (isVideo)
      {
        if (_encoderVideo == null)
        {
          softwareEncoders = SoftwareEncoderManagement.ListAllSofwareEncodersVideo();
        }
        else
        {
          softwareEncoders = new List<VideoEncoder> { _encoderVideo };
          isPreferredEncoder = true;
        }
      }
      else
      {
        if (_encoderAudio == null)
        {
          softwareEncoders = SoftwareEncoderManagement.ListAllSofwareEncodersAudio();
        }
        else
        {
          softwareEncoders = new List<AudioEncoder> { _encoderAudio };
          isPreferredEncoder = true;
        }
      }

      string name;
      string classId;
      foreach (var encoder in softwareEncoders)
      {
        name = encoder.ToString();
        if (isVideo)
        {
          VideoEncoder videoEncoder = encoder as VideoEncoder;
          classId = videoEncoder.ClassId;
        }
        else
        {
          AudioEncoder audioEncoder = encoder as AudioEncoder;
          classId = audioEncoder.ClassId;
        }
        bool success = false;
        try
        {
          this.LogDebug("WDM analog encoder:     try {0}, CLSID = {1}", name, classId);
          filter = FilterGraphTools.AddFilterFromRegisteredClsid(graph, new Guid(classId), string.Format("{0} {1}", name, isVideo ? "Video" : "Audio"));
          if (FilterGraphTools.ConnectFilterWithPin(graph, pin, PinDirection.Output, filter))
          {
            success = true;
            isCyberLink = name.ToLowerInvariant().Contains("cyberlink");
            return;
          }
        }
        catch
        {
        }
        finally
        {
          if (!success && filter != null)
          {
            graph.RemoveFilter(filter);
            Release.ComObject("WDM analog encoder software encoder candidate", ref filter);
          }
        }
      }

      throw new TvExceptionNeedSoftwareEncoder(isVideo, isPreferredEncoder);
    }

    private bool ConnectSoftwareVideoEncoderExtraPins(IFilterGraph2 graph, IPin audioPin, IPin closedCaptionsPin, IBaseFilter videoEncoder)
    {
      this.LogDebug("WDM analog encoder: connect software video encoder extra pins");

      // Sometimes the video encoder may also encode audio and/or closed
      // captions. Unfortunately the encoders I've seen don't advertise the
      // supported media types on their input pins. We're forced to check the
      // pin name to determine the type.
      bool connectedAudio = false;
      EnumerateUnconnectedFilterInputs(videoEncoder, delegate(int index, string name, IPin pin)
      {
        this.LogDebug("WDM analog encoder:   pin, index = {0}, name = {1}...", index, name);
        if (DsUtils.GetPinCategory(pin) == PinCategory.CC || name.ToLowerInvariant().Contains("cc"))
        {
          if (closedCaptionsPin != null && graph.ConnectDirect(closedCaptionsPin, pin, null) == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("WDM analog encoder:     connected closed captions!");
            closedCaptionsPin = null;
          }
        }
        else if (audioPin != null && graph.ConnectDirect(audioPin, pin, null) == (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("WDM analog encoder:     connected audio!");
          audioPin = null;
          connectedAudio = true;
        }
        return closedCaptionsPin != null || audioPin != null;
      });
      return connectedAudio;
    }

    private void AddAndConnectMultiplexer(IFilterGraph2 graph, IPin pinCapture, IPin pinVideo, IPin pinAudio, string productInstanceId, out IBaseFilter filter, out DsDevice device)
    {
      filter = null;
      device = null;
      if (pinCapture != null)
      {
        if (PinRequiresHardwareFilterConnection(pinCapture))
        {
          this.LogDebug("WDM analog encoder: hardware multiplexer required");
          if (!FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, pinCapture, FilterCategory.WDMStreamingMultiplexerDevices, out filter, out device, productInstanceId))
          {
            throw new TvException("Failed to find hardware encoder or multiplexer to connect to capture output.");
          }
        }
        return;
      }

      if (pinVideo != null)
      {
        this.LogDebug("WDM analog encoder: connect encoder video output");
        if (PinRequiresHardwareFilterConnection(pinVideo))
        {
          this.LogDebug("WDM analog encoder: hardware multiplexer required");
          if (!FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, pinVideo, FilterCategory.WDMStreamingMultiplexerDevices, out filter, out device, productInstanceId))
          {
            throw new TvException("Failed to find hardware encoder or multiplexer to connect to video output.");
          }
        }
      }
      if (pinAudio != null)
      {
        this.LogDebug("WDM analog encoder: connect encoder audio output");
        if (PinRequiresHardwareFilterConnection(pinAudio))
        {
          if (_filterMultiplexer != null)
          {
            this.LogDebug("WDM analog encoder: connect audio output to existing multiplexer");
            if (!FilterGraphTools.ConnectFilterWithPin(graph, pinAudio, PinDirection.Output, _filterMultiplexer))
            {
              throw new TvException("Failed to connect audio output to hardware multiplexer.");
            }
          }
          else
          {
            this.LogDebug("WDM analog encoder: hardware multiplexer required");
            if (!FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, pinAudio, FilterCategory.WDMStreamingMultiplexerDevices, out filter, out device, productInstanceId))
            {
              throw new TvException("Failed to find hardware encoder or multiplexer to connect to audio output.");
            }
          }
        }
      }
    }

    private bool AddAndConnectCyberLinkMultiplexer(IFilterGraph2 graph, IPin videoPin, IPin audioPin, out IBaseFilter filter)
    {
      this.LogDebug("WDM analog encoder: add and connect CyberLink multiplexer");
      filter = null;
      foreach (Guid clsid in CYBERLINK_MULTIPLEXERS)
      {
        try
        {
          this.LogDebug("WDM analog encoder:   try {0}", clsid);
          filter = FilterGraphTools.AddFilterFromRegisteredClsid(graph, clsid, "CyberLink MPEG Muxer");
        }
        catch
        {
          // Failed to add or not installed.
          continue;
        }
        try
        {
          if (videoPin != null && !FilterGraphTools.ConnectFilterWithPin(graph, videoPin, PinDirection.Output, filter))
          {
            throw new TvException("Failed to connect video to CyberLink multiplexer.");
          }
          if (audioPin != null && !FilterGraphTools.ConnectFilterWithPin(graph, audioPin, PinDirection.Output, filter))
          {
            throw new TvException("Failed to connect audio to CyberLink multiplexer.");
          }
          this.LogDebug("WDM analog encoder:     connected!");
          return true;
        }
        catch (Exception ex)
        {
          this.LogError(ex, "WDM analog encoder: failed to connect CyberLink multiplexer {0}", clsid);
          graph.RemoveFilter(filter);
          Release.ComObject("WDM analog encoder CyberLink multiplexer filter candidate", ref filter);
        }
      }
      return false;
    }

    private void ConnectTsMultiplexer(IFilterGraph2 graph, IEnumerable<IPin> pinsToConnect)
    {
      this.LogDebug("WDM analog encoder: connect TS multiplexer");
      IEnumerator<IPin> pinEnumerator = pinsToConnect.GetEnumerator();
      try
      {
        EnumerateUnconnectedFilterInputs(_filterTsMultiplexer, delegate(int index, string name, IPin pin)
        {
          IPin pinToConnect = null;
          while (pinToConnect == null && pinEnumerator.MoveNext())
          {
            pinToConnect = pinEnumerator.Current;
          }
          if (pinToConnect != null)
          {
            int hr = graph.ConnectDirect(pinToConnect, pin, null);
            TvExceptionDirectShowError.Throw(hr, "Failed to connect TS multiplexer pin {0}.", index);
            return false;
          }
          return true;
        });
      }
      catch (Exception ex)
      {
        this.LogError(ex, "WDM analog encoder: failed to connect TS multiplexer");
        throw;
      }

      ApplyTsMuxerConfig();
    }

    private void ApplyTsMuxerConfig()
    {
      ITsMultiplexer tsMuxer = _filterTsMultiplexer as ITsMultiplexer;
      if (tsMuxer != null)
      {
        tsMuxer.DumpInput(_tsMuxerInputDumpMask);
      }
    }

    private void EnableVbi(IFilterGraph2 graph)
    {
      this.LogDebug("WDM analog encoder: enable VBI");
      IDictionary<PinType, IPin> pins = new Dictionary<PinType, IPin>(5);
      if (!AddAndConnectVbiFilters(graph, _capture.PinOutputVbi, pins))
      {
        return;
      }
      IPin closedCaptionsPin = null;
      try
      {
        pins.TryGetValue(PinType.ClosedCaptions, out closedCaptionsPin);
        pins.Remove(PinType.ClosedCaptions);
        ConnectTsMultiplexer(graph, pins.Values);
        // All Hauppauge SAA7164-based cards have hardware encoding.
        if (_deviceEncoderVideo != null)
        {
          ConnectSoftwareVideoEncoderExtraPins(graph, null, closedCaptionsPin, _filterEncoderVideo);
        }
        _isVbiEnabled = true;
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "wDM analog encoder: failed to connect VBI output pins");
      }
      finally
      {
        Release.ComObject("WDM analog encoder closed captions output pin", ref closedCaptionsPin);
        foreach (IPin pin in pins.Values)
        {
          IPin tempPin = pin;
          Release.ComObject("WDM analog encoder VBI output pin", ref tempPin);
        }
      }
    }

    private void DisableVbi(IFilterGraph2 graph)
    {
      this.LogDebug("WDM analog encoder: disable VBI");
      int hr = (int)NativeMethods.HResult.S_OK;
      if (_filterCcDecoder != null)
      {
        hr |= graph.RemoveFilter(_filterCcDecoder);
      }
      if (_filterWstCodec != null)
      {
        hr |= graph.RemoveFilter(_filterWstCodec);
      }
      hr |= graph.RemoveFilter(_filterVbiSplitter);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogWarn("WDM analog encoder: failed to remove one or more VBI filters, hr = 0x{0:x}", hr);
        return;
      }

      Release.ComObject("WDM analog encoder VBI splitter filter", ref _filterVbiSplitter);
      Release.ComObject("WDM analog encoder CC decoder filter", ref _filterCcDecoder);
      Release.ComObject("WDM analog encoder WST codec filter", ref _filterWstCodec);
      _isVbiEnabled = false;
    }

    #endregion

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public void PerformTuning(IChannel channel)
    {
      this.LogDebug("WDM analog encoder: perform tuning");

      // Hauppauge have a bug in the driver for their HVR-22** products which
      // will probably never be fixed. The bug causes a BSOD when the capture
      // filter's VBI output pin is connected and the tuner transitions from
      // being locked onto signal to unlocked. Due to the nature of the
      // pre-conditions, the bug mainly affects scanning:
      // http://forum.team-mediaportal.com/threads/hvr-2250-scanning-channels-causes-crash.102858/
      // http://forum.team-mediaportal.com/threads/bsod-when-scanning-channels-analog-cable-hvr-2255-f111.132566/
      //
      // The following work-around should avoid BSODs in ideal conditions.
      if (_isHauppaugeSaa7164Based)
      {
        bool isScanning = string.IsNullOrEmpty(channel.Name);
        if ((!isScanning && !_isVbiEnabled) || (isScanning && _isVbiEnabled))
        {
          FilterInfo filterInfo;
          int hr = _filterTsMultiplexer.QueryFilterInfo(out filterInfo);
          if (hr != (int)NativeMethods.HResult.S_OK || filterInfo.pGraph == null)
          {
            this.LogWarn("WDM analog encoder: failed to get TS multiplexer filter info, hr = 0x{0:x}", hr);
          }
          else
          {
            IFilterGraph2 graph = (IFilterGraph2)filterInfo.pGraph;
            try
            {
              if (isScanning && _isVbiEnabled)
              {
                DisableVbi(graph);
              }
              else
              {
                EnableVbi(graph);
              }
            }
            finally
            {
              Release.FilterInfo(ref filterInfo);
            }
          }
        }
      }

      this.LogDebug("WDM analog encoder: set TS multiplexer active components");
      ITsMultiplexer muxer = _filterTsMultiplexer as ITsMultiplexer;
      if (channel.MediaType == Mediaportal.TV.Server.Common.Types.Enum.MediaType.Television)
      {
        muxer.SetActiveComponents(true, true, false, _isVbiEnabled, true, true);
      }
      else
      {
        muxer.SetActiveComponents(false, true, true, false, false, false);
      }
    }

    /// <summary>
    /// Unload the component.
    /// </summary>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    public void PerformUnloading(IFilterGraph2 graph)
    {
      this.LogDebug("WDM analog encoder: perform unloading");

      _capture = null;
      if (graph != null)
      {
        graph.RemoveFilter(_filterTsMultiplexer);
        graph.RemoveFilter(_filterVbiSplitter);
        graph.RemoveFilter(_filterCcDecoder);
        graph.RemoveFilter(_filterWstCodec);
      }

      Release.ComObject("WDM analog encoder TS multiplexer filter", ref _filterTsMultiplexer);
      Release.ComObject("WDM analog encoder VBI splitter filter", ref _filterVbiSplitter);
      Release.ComObject("WDM analog encoder CC decoder filter", ref _filterCcDecoder);
      Release.ComObject("WDM analog encoder WST codec filter", ref _filterWstCodec);
      _isVbiEnabled = false;

      if (_filterEncoderAudio != null && _filterEncoderAudio != _filterEncoderVideo)
      {
        if (graph != null)
        {
          graph.RemoveFilter(_filterEncoderAudio);
        }
        Release.ComObject("WDM analog encoder audio encoder filter", ref _filterEncoderAudio);

        if (_deviceEncoderAudio != null)
        {
          DevicesInUse.Instance.Remove(_deviceEncoderAudio);
          _deviceEncoderAudio.Dispose();
        }
      }

      if (_filterEncoderVideo != null)
      {
        if (graph != null)
        {
          graph.RemoveFilter(_filterEncoderVideo);
        }
        Release.ComObject("WDM analog encoder video encoder filter", ref _filterEncoderVideo);

        if (_deviceEncoderVideo != null)
        {
          DevicesInUse.Instance.Remove(_deviceEncoderVideo);
          _deviceEncoderVideo.Dispose();
          _deviceEncoderVideo = null;
        }
      }
      _filterEncoderAudio = null;
      _deviceEncoderAudio = null;

      if (_filterMultiplexer != null)
      {
        if (graph != null)
        {
          graph.RemoveFilter(_filterMultiplexer);
        }
        Release.ComObject("WDM analog encoder multiplexer filter", ref _filterMultiplexer);

        if (_deviceMultiplexer != null)
        {
          DevicesInUse.Instance.Remove(_deviceMultiplexer);
          _deviceMultiplexer.Dispose();
          _deviceMultiplexer = null;
        }
      }
    }
  }
}