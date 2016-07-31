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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Multiplexer;
using MediaPortal.Common.Utils;
using MediaType = DirectShowLib.MediaType;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component
{
  /// <summary>
  /// A WDM analog DirectShow encoder graph component.
  /// </summary>
  internal class Encoder
  {
    #region constants

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

    private static IList<AMMediaType> MEDIA_TYPES_CAPTURE = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_CAPTURE_VIDEO = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_CAPTURE_AUDIO = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_VIDEO = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_AUDIO = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_AUDIO_PCM = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_VBI = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_TELETEXT = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_VPS = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_WSS = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_CLOSED_CAPTIONS = new List<AMMediaType>();

    #endregion

    #region variables

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
    /// The VBI or WST codec/splitter filter.
    /// </summary>
    private IBaseFilter _filterVbiSplitter = null;

    /// <summary>
    /// The upstream VBI output pin, usually associated with a video capture
    /// filter.
    /// </summary>
    private IPin _pinUpstreamVbiOutput = null;

    #region settings

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

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="Encoder"/> class.
    /// </summary>
    public Encoder()
    {
      if (MEDIA_TYPES_CAPTURE.Count != 0)
      {
        return;
      }

      MEDIA_TYPES_CAPTURE.Add(new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.Mpeg2Transport });
      MEDIA_TYPES_CAPTURE.Add(new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.Mpeg2Program });
      MEDIA_TYPES_CAPTURE.Add(new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.MPEG1System });

      MEDIA_TYPES_CAPTURE_VIDEO.Add(new AMMediaType() { majorType = MediaType.Video, subType = MediaSubType.MPEG1Payload });
      MEDIA_TYPES_CAPTURE_VIDEO.Add(new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.MPEG1Video });
      MEDIA_TYPES_CAPTURE_VIDEO.Add(new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.Mpeg2Video });

      MEDIA_TYPES_CAPTURE_AUDIO.Add(new AMMediaType() { majorType = MediaType.Audio, subType = MediaSubType.MPEG1Payload });
      MEDIA_TYPES_CAPTURE_AUDIO.Add(new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.MPEG1Audio });
      MEDIA_TYPES_CAPTURE_AUDIO.Add(new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.MPEG1AudioPayload });
      MEDIA_TYPES_CAPTURE_AUDIO.Add(new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.Mpeg2Audio });

      MEDIA_TYPES_VIDEO.Add(new AMMediaType() { majorType = MediaType.Video, subType = MediaSubType.Null });
      MEDIA_TYPES_AUDIO.Add(new AMMediaType() { majorType = MediaType.Audio, subType = MediaSubType.Null });
      MEDIA_TYPES_AUDIO_PCM.Add(new AMMediaType() { majorType = MediaType.Audio, subType = MediaSubType.PCM });
      MEDIA_TYPES_VBI.Add(new AMMediaType() { majorType = MediaType.VBI, subType = MediaSubType.Null });
      MEDIA_TYPES_TELETEXT.Add(new AMMediaType() { majorType = MediaType.VBI, subType = MediaSubType.TELETEXT });
      MEDIA_TYPES_VPS.Add(new AMMediaType() { majorType = MediaType.VBI, subType = MediaSubType.VPS });
      MEDIA_TYPES_WSS.Add(new AMMediaType() { majorType = MediaType.VBI, subType = MediaSubType.WSS });
      MEDIA_TYPES_CLOSED_CAPTIONS.Add(new AMMediaType() { majorType = MediaType.AuxLine21Data, subType = MediaSubType.Line21_BytePair });
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

      _encoderVideo = configuration.AnalogTunerSettings.VideoEncoder;
      _encoderAudio = configuration.AnalogTunerSettings.AudioEncoder;
      _tsMuxerInputDumpMask = configuration.TsWriterInputDumpMask;

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
    /// Add and connect the encoder and teletext filters into the tuner graph.
    /// This function handles a huge variety of filter and pin connection
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
      IList<IPin> pinsToConnect = new List<IPin>();
      IPin capturePin = null;
      IPin videoPin = null;
      bool isVideoPinCapture = false;
      IPin audioPin = null;
      bool isAudioPinCapture = false;
      IPin teletextPin = null;
      IPin vpsPin = null;
      IPin wssPin = null;
      IPin closedCaptionsPin = null;
      try
      {
        // ------------------------------------------------
        // STAGE 1
        // Add a VBI splitter/decoder for teletext and
        // closed caption support.
        // ------------------------------------------------
        if (capture.VideoFilter != null)
        {
          IBaseFilter vbiFilter = capture.VideoFilter;
          IPin vbiPin = null;
          try
          {
            if (
              FindPinByCategoryOrMediaType(vbiFilter, PinCategory.VBI, MEDIA_TYPES_VBI, out vbiPin) &&
              AddAndConnectVbiFilter(graph, vbiPin, out _filterVbiSplitter)
            )
            {
              _isVbiEnabled = true;
              _pinUpstreamVbiOutput = vbiPin;
              vbiFilter = _filterVbiSplitter;
            }
          }
          finally
          {
            if (_pinUpstreamVbiOutput == null && vbiPin != null)
            {
              Release.ComObject("WDM analog encoder capture VBI output pin", ref vbiPin);
            }
          }
          if (FindPinByCategoryOrMediaType(vbiFilter, PinCategory.TeleText, MEDIA_TYPES_TELETEXT, out teletextPin))
          {
            this.LogDebug("WDM analog encoder: found teletext output");
            pinsToConnect.Add(teletextPin);
          }
          if (FindPinByCategoryOrMediaType(vbiFilter, Guid.Empty, MEDIA_TYPES_VPS, out vpsPin))
          {
            this.LogDebug("WDM analog encoder: found video program system output");
            pinsToConnect.Add(vpsPin);
          }
          if (FindPinByCategoryOrMediaType(vbiFilter, Guid.Empty, MEDIA_TYPES_WSS, out wssPin))
          {
            this.LogDebug("WDM analog encoder: found wide screen signalling output");
            pinsToConnect.Add(wssPin);
          }
          if (FindPinByCategoryOrMediaType(vbiFilter, PinCategory.CC, MEDIA_TYPES_CLOSED_CAPTIONS, out closedCaptionsPin))
          {
            this.LogDebug("WDM analog encoder: found closed captions output");
          }
        }

        // ------------------------------------------------
        // STAGE 2
        // Find relevant pins and the stream config
        // interface.
        // ------------------------------------------------
        this.LogDebug("WDM analog encoder: find capture outputs");
        bool isCyberLinkEncoder = false;
        FindPins(capture.VideoFilter, capture.AudioFilter, ref capturePin, ref videoPin, ref isVideoPinCapture, ref audioPin, ref isAudioPinCapture);
        bool capturePinHasStreamConfigInterface = false;
        bool videoPinHasStreamConfigInterface = false;
        IAMStreamConfig streamConfig = capturePin as IAMStreamConfig;
        if (streamConfig == null)
        {
          streamConfig = videoPin as IAMStreamConfig;
          videoPinHasStreamConfigInterface = streamConfig != null;
        }
        else
        {
          capturePinHasStreamConfigInterface = true;
        }
        capture.SetStreamConfigInterface(streamConfig);

        // ------------------------------------------------
        // STAGE 3
        // Connect hardware or software encoder(s) if
        // required.
        // ------------------------------------------------
        if (capturePin != null)
        {
          if (PinRequiresHardwareFilterConnection(capturePin))
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

            if (result && capture.VideoFilter == capture.AudioFilter)
            {
              _filterEncoderAudio = _filterEncoderVideo;
              _deviceEncoderAudio = _deviceEncoderVideo;
            }
          }
        }
        else
        {
          if (capture.VideoFilter != null)
          {
            this.LogDebug("WDM analog encoder: connect capture video output");
            AddAndConnectEncoder(graph, true, videoPin, isVideoPinCapture, productInstanceId, out _filterEncoderVideo, out _deviceEncoderVideo, out isCyberLinkEncoder);
            if (_filterEncoderVideo != null)
            {
              if (_deviceEncoderVideo == null)
              {
                if (ConnectSoftwareVideoEncoderExtraPins(graph, audioPin, closedCaptionsPin, _filterEncoderVideo))
                {
                  _filterEncoderAudio = _filterEncoderVideo;
                }
              }
              else if (capture.AudioFilter != null)
              {
                this.LogDebug("WDM analog encoder: connect capture audio output to existing encoder");
                if (FilterGraphTools.ConnectFilterWithPin(graph, audioPin, PinDirection.Output, _filterEncoderVideo))
                {
                  _filterEncoderAudio = _filterEncoderVideo;
                  _deviceEncoderAudio = _deviceEncoderVideo;
                }
                else
                {
                  this.LogDebug("WDM analog encoder: separate encoder required");
                }
              }
            }
          }

          if (_filterEncoderAudio == null && capture.AudioFilter != null)
          {
            this.LogDebug("WDM analog encoder: connect capture audio output");
            AddAndConnectEncoder(graph, false, audioPin, isAudioPinCapture, productInstanceId, out _filterEncoderAudio, out _deviceEncoderAudio, out isCyberLinkEncoder);
            if (_filterEncoderAudio != null && capture.VideoFilter == capture.AudioFilter && _deviceEncoderVideo == null)
            {
              // If we have a shared video and audio capture, software video
              // encoder, and hardware audio encoder... it is possible that
              // the audio encoder is actually a capture.
              Release.ComObject("WDM analog encoder capture audio output pin", ref audioPin);
              if (FindPinByCategoryOrMediaType(_filterEncoderAudio, Guid.Empty, MEDIA_TYPES_AUDIO_PCM, out audioPin))
              {
                this.LogDebug("WDM analog encoder: detected chained audio capture, connect another audio encoder");
                capture.SetAudioCapture(_filterEncoderAudio, _deviceEncoderAudio);
                _filterEncoderAudio = null;
                _deviceEncoderAudio = null;
                AddAndConnectEncoder(graph, false, audioPin, isAudioPinCapture, productInstanceId, out _filterEncoderAudio, out _deviceEncoderAudio, out isCyberLinkEncoder);
              }
            }
          }
        }

        // ------------------------------------------------
        // STAGE 4
        // Cleanup pins.
        // ------------------------------------------------
        if (_filterEncoderVideo != null || _filterEncoderAudio != null)
        {
          if (!capturePinHasStreamConfigInterface)
          {
            Release.ComObject("WDM analog encoder capture output pin", ref capturePin);
          }
          else
          {
            capturePin = null;
          }
          if (_filterEncoderVideo != null)
          {
            if (!videoPinHasStreamConfigInterface)
            {
              Release.ComObject("WDM analog encoder capture video output pin", ref videoPin);
            }
            else
            {
              videoPin = null;
            }
          }
          if (_filterEncoderAudio != null)
          {
            Release.ComObject("WDM analog encoder capture audio output pin", ref audioPin);
          }
        }

        // ------------------------------------------------
        // STAGE 5
        // Connect a hardware or software multiplexer if
        // required.
        // ------------------------------------------------
        this.LogDebug("WDM analog encoder: find encoder outputs");
        FindPins(_filterEncoderVideo, _filterEncoderAudio, ref capturePin, ref videoPin, ref isVideoPinCapture, ref audioPin, ref isAudioPinCapture);
        AddAndConnectMultiplexer(graph, capturePin, videoPin, audioPin, productInstanceId, out _filterMultiplexer, out _deviceMultiplexer);
        if (_filterMultiplexer == null && _deviceEncoderAudio != null && isCyberLinkEncoder)
        {
          // Add and connect a CyberLink multiplexer. This is required if a
          // CyberLink audio encoder is used. If the muxer isn't in the
          // graph, the audio encoder causes an access violation exception
          // when you attempt to start the graph. My guess is that the
          // encoder interacts with the muxer. I tried to mimic interfaces
          // requested via QueryInterface() with our TS multiplexer but
          // ultimately I never managed to make the encoder work without
          // the CyberLink multiplexer.
          if (!AddAndConnectCyberLinkMultiplexer(graph, videoPin, audioPin, out _filterMultiplexer))
          {
            throw new TvException("Failed to add and connect CyberLink multiplexer.");
          }
        }

        // ------------------------------------------------
        // STAGE 6
        // Cleanup and connect the TS multiplexer.
        // ------------------------------------------------
        if (_filterMultiplexer != null)
        {
          Release.ComObject("WDM analog encoder capture output", ref capturePin);
          Release.ComObject("WDM analog encoder video output", ref videoPin);
          Release.ComObject("WDM analog encoder audio output", ref audioPin);
          if (!FindPinByCategoryOrMediaType(_filterMultiplexer, Guid.Empty, MEDIA_TYPES_CAPTURE, out capturePin))
          {
            throw new TvException("Failed to find capture output on multiplexer.");
          }
          this.LogDebug("WDM analog encoder: found capture output on multiplexer");
        }
        if (capturePin != null)
        {
          pinsToConnect.Add(capturePin);
        }
        else
        {
          if (videoPin != null)
          {
            pinsToConnect.Add(videoPin);
          }
          if (audioPin != null)
          {
            pinsToConnect.Add(audioPin);
          }
        }
        if (!AddAndConnectTsMultiplexer(graph, pinsToConnect))
        {
          throw new TvException("Failed to add and connect TS multiplexer.");
        }
      }
      finally
      {
        Release.ComObject("WDM analog encoder final capture pin", ref capturePin);
        Release.ComObject("WDM analog encoder final video pin", ref videoPin);
        Release.ComObject("WDM analog encoder final audio pin", ref audioPin);
        Release.ComObject("WDM analog encoder final teletext pin", ref teletextPin);
        Release.ComObject("WDM analog encoder final video program system pin", ref vpsPin);
        Release.ComObject("WDM analog encoder final wide screen signalling pin", ref wssPin);
        Release.ComObject("WDM analog encoder final closed caption pin", ref closedCaptionsPin);
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

    private bool FindPinByCategoryOrMediaType(IBaseFilter filter, Guid matchCategory, IList<AMMediaType> matchMediaTypes, out IPin matchedPin)
    {
      if (matchMediaTypes == null)
      {
        matchMediaTypes = new List<AMMediaType>();
      }
      matchedPin = null;
      Guid matchMediaMajorType = Guid.Empty;
      Guid matchMediaSubType = Guid.Empty;

      IEnumPins enumPins;
      int hr = filter.EnumPins(out enumPins);
      TvExceptionDirectShowError.Throw(hr, "Failed to obtain pin enumerator.");
      try
      {
        // For each output pin...
        int pinCount;
        IPin[] pins = new IPin[2];
        while (enumPins.Next(1, pins, out pinCount) == (int)NativeMethods.HResult.S_OK && pinCount == 1)
        {
          IPin pin = pins[0];
          try
          {
            // We're not interested in input pins.
            PinDirection direction;
            hr = pin.QueryDirection(out direction);
            TvExceptionDirectShowError.Throw(hr, "Failed to query direction for pin.");
            if (direction == PinDirection.Input)
            {
              continue;
            }

            // Does the category match?
            if (matchCategory != Guid.Empty && matchCategory == DsUtils.GetPinCategory(pin))
            {
              matchedPin = pin;
              return true;
            }

            // For each pin media type...
            IEnumMediaTypes enumMediaTypes;
            hr = pin.EnumMediaTypes(out enumMediaTypes);
            TvExceptionDirectShowError.Throw(hr, "Failed to obtain media type enumerator for pin.");
            try
            {
              int mediaTypeCount;
              AMMediaType[] mediaTypes = new AMMediaType[2];
              while (enumMediaTypes.Next(1, mediaTypes, out mediaTypeCount) == (int)NativeMethods.HResult.S_OK && mediaTypeCount == 1)
              {
                AMMediaType mediaType = mediaTypes[0];
                try
                {
                  foreach (AMMediaType mt in matchMediaTypes)
                  {
                    if ((mediaType.majorType == mt.majorType || mt.majorType == MediaType.Null) && (mediaType.subType == mt.subType || mt.subType == MediaSubType.Null))
                    {
                      matchedPin = pin;
                      matchMediaMajorType = mediaType.majorType;
                      matchMediaSubType = mediaType.subType;
                      return true;
                    }
                  }
                }
                finally
                {
                  Release.AmMediaType(ref mediaType);
                }
              }
            }
            finally
            {
              Release.ComObject("WDM analog encoder pin media type enumerator", ref enumMediaTypes);
            }
          }
          catch (Exception ex)
          {
            this.LogError(ex, "WDM analog encoder: unexpected error in FindPinByMediaType()");
          }
          finally
          {
            if (matchedPin == null)
            {
              Release.ComObject("WDM analog encoder non-matched pin", ref pin);
            }
          }
        }
      }
      finally
      {
        Release.ComObject("WDM analog encoder pin enumerator", ref enumPins);
      }
      return false;
    }

    private bool AddAndConnectVbiFilter(IFilterGraph2 graph, IPin vbiPin, out IBaseFilter filter)
    {
      this.LogDebug("WDM analog encoder: add and connect VBI splitter");
      filter = null;
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(TveGuid.AM_KS_CATEGORY_MULTI_VBI_CODEC);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(FilterCategory.AMKSVBICodec);
      DsDevice[] devices = new DsDevice[devices1.Length + devices2.Length];
      devices1.CopyTo(devices, 0);
      devices2.CopyTo(devices, devices1.Length);
      try
      {
        for (int i = 0; i < devices.Length; i++)
        {
          DsDevice d = devices[i];
          string deviceName = d.Name;
          if (deviceName == null || (!deviceName.Contains("VBI") && !deviceName.Contains("WST")))
          {
            continue;
          }

          try
          {
            this.LogDebug("WDM analog encoder:   try {0}", deviceName);
            filter = FilterGraphTools.AddFilterFromDevice(graph, d);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "WDM analog encoder: failed to add VBI splitter {0}", deviceName);
            continue;
          }
          IPin pin = null;
          try
          {
            pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
            int hr = graph.ConnectDirect(vbiPin, pin, null);
            TvExceptionDirectShowError.Throw(hr, "Failed to connect VBI splitter.");
            this.LogDebug("WDM analog encoder:     connected!");
            return true;
          }
          catch (Exception ex)
          {
            this.LogError(ex, "WDM analog encoder: failed to connect VBI splitter {0}", deviceName);
            graph.RemoveFilter(filter);
            Release.ComObject("WDM analog encoder VBI splitter filter candidate", ref filter);
          }
          finally
          {
            Release.ComObject("WDM analog encoder VBI splitter input pin", ref pin);
          }
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          d.Dispose();
        }
      }
      return false;
    }

    private void FindPins(IBaseFilter filterVideo, IBaseFilter filterAudio, ref IPin pinCapture, ref IPin pinVideo, ref bool isVideoPinCapture, ref IPin pinAudio, ref bool isAudioPinCapture)
    {
      if (pinCapture == null && (filterVideo == null || filterAudio == null || filterVideo == filterAudio))
      {
        if (FindPinByCategoryOrMediaType(filterVideo ?? filterAudio, Guid.Empty, MEDIA_TYPES_CAPTURE, out pinCapture))
        {
          this.LogDebug("WDM analog encoder:   found capture output");
          return;
        }
      }

      if (pinVideo == null && filterVideo != null)
      {
        if (FindPinByCategoryOrMediaType(filterVideo, Guid.Empty, MEDIA_TYPES_CAPTURE_VIDEO, out pinVideo))
        {
          isVideoPinCapture = true;
          this.LogDebug("WDM analog encoder:   found video capture output");
        }
        else
        {
          isVideoPinCapture = false;
          if (FindPinByCategoryOrMediaType(filterVideo, Guid.Empty, MEDIA_TYPES_VIDEO, out pinVideo))
          {
            this.LogDebug("WDM analog encoder:   found video output");
          }
        }
      }

      if (pinAudio == null && filterAudio != null)
      {
        if (FindPinByCategoryOrMediaType(filterAudio, Guid.Empty, MEDIA_TYPES_CAPTURE_AUDIO, out pinAudio))
        {
          isAudioPinCapture = true;
          this.LogDebug("WDM analog encoder:   found audio capture output");
        }
        else
        {
          isAudioPinCapture = false;
          if (FindPinByCategoryOrMediaType(filterAudio, Guid.Empty, MEDIA_TYPES_AUDIO, out pinAudio))
          {
            this.LogDebug("WDM analog encoder:   found audio output");
          }
        }
      }
    }

    private void AddAndConnectEncoder(IFilterGraph2 graph, bool isVideo, IPin pin, bool isCapturePin, string productInstanceId, out IBaseFilter filter, out DsDevice device, out bool isCyberLink)
    {
      filter = null;
      device = null;
      isCyberLink = false;

      if (pin == null)
      {
        this.LogWarn("WDM analog encoder: failed to find output");
        return;
      }
      else if (PinRequiresHardwareFilterConnection(pin))
      {
        this.LogDebug("WDM analog encoder:   hardware encoder (and/or multiplexer) required");
        FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, pin, FilterCategory.WDMStreamingEncoderDevices, out filter, out device, productInstanceId);
      }
      else if (!isCapturePin)
      {
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
      else
      {
        this.LogDebug("WDM analog encoder:   found capture output, encoder not required");
      }
    }

    private bool ConnectSoftwareVideoEncoderExtraPins(IFilterGraph2 graph, IPin audioPin, IPin closedCaptionsPin, IBaseFilter videoEncoder)
    {
      // Sometimes the video encoder may also encode audio and/or closed
      // captions. Unfortunately the encoders I've seen don't advertise the
      // supported media types on their input pins. We're forced to check the
      // pin name to determine the type.
      bool connectedAudio = false;
      int i = 1;
      while (true)
      {
        IPin inputPin = DsFindPin.ByDirection(videoEncoder, PinDirection.Input, i);
        if (inputPin == null)
        {
          return connectedAudio;
        }
        try
        {
          if (i == 1)
          {
            this.LogDebug("WDM analog encoder: connect software video encoder extra pins");
          }
          string pinName = FilterGraphTools.GetPinName(inputPin);
          this.LogDebug("WDM analog encoder:   pin {0}...", pinName);
          if (DsUtils.GetPinCategory(inputPin) == PinCategory.CC || pinName.ToLowerInvariant().Contains("cc"))
          {
            if (closedCaptionsPin != null && graph.ConnectDirect(closedCaptionsPin, inputPin, null) == (int)NativeMethods.HResult.S_OK)
            {
              this.LogDebug("WDM analog encoder:     connected closed captions!");
              closedCaptionsPin = null;
            }
          }
          else
          {
            if (audioPin != null && graph.ConnectDirect(audioPin, inputPin, null) == (int)NativeMethods.HResult.S_OK)
            {
              this.LogDebug("WDM analog encoder:     connected audio!");
              audioPin = null;
            }
          }
          i++;
        }
        finally
        {
          Release.ComObject("WDM analog encoder software video encoder extra input pin", ref inputPin);
        }
      }
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
          if (!FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, pinCapture, FilterCategory.WDMStreamingMultiplexerDevices, out filter, out device, productInstanceId))
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

    private bool AddAndConnectTsMultiplexer(IFilterGraph2 graph, IList<IPin> pinsToConnect)
    {
      this.LogDebug("WDM analog encoder: add and connect TS multiplexer");
      try
      {
        _filterTsMultiplexer = FilterGraphTools.AddFilterFromFile(graph, "TsMuxer.ax", typeof(MediaPortalTsMultiplexer).GUID, "MediaPortal TS Multiplexer");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "WDM analog encoder: failed to add TS multiplexer filter to graph");
        return false;
      }

      IPin inputPin = null;
      int pinIndex = 0;
      foreach (IPin pinToConnect in pinsToConnect)
      {
        try
        {
          inputPin = DsFindPin.ByDirection(_filterTsMultiplexer, PinDirection.Input, pinIndex++);
          int hr = graph.ConnectDirect(pinToConnect, inputPin, null);
          TvExceptionDirectShowError.Throw(hr, "Failed to connect TS multiplexer.");
        }
        catch (Exception ex)
        {
          this.LogError(ex, "WDM analog encoder: failed to connect TS multiplexer pin {0}", pinIndex);
          return false;
        }
        finally
        {
          Release.ComObject("WDM analog encoder TS muxer input pin " + pinIndex, ref inputPin);
        }
      }

      ApplyTsMuxerConfig();
      return true;
    }

    private void ApplyTsMuxerConfig()
    {
      ITsMultiplexer tsMuxer = _filterTsMultiplexer as ITsMultiplexer;
      if (tsMuxer != null)
      {
        tsMuxer.DumpInput(_tsMuxerInputDumpMask);
      }
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
      if (_pinUpstreamVbiOutput != null && _filterVbiSplitter != null)
      {
        bool isScanning = string.IsNullOrEmpty(channel.Name);
        if ((!isScanning && !_isVbiEnabled) || (isScanning && _isVbiEnabled))
        {
          FilterInfo filterInfo;
          int hr = _filterVbiSplitter.QueryFilterInfo(out filterInfo);
          if (hr != (int)NativeMethods.HResult.S_OK || filterInfo.pGraph == null)
          {
            this.LogWarn("WDM analog encoder: failed to get VBI splitter filter info, hr = 0x{0:x}", hr);
          }
          else
          {
            try
            {
              if (isScanning && _isVbiEnabled)
              {
                this.LogDebug("WDM analog encoder: disconnect VBI");
                hr = filterInfo.pGraph.Disconnect(_pinUpstreamVbiOutput);
                _isVbiEnabled = hr != (int)NativeMethods.HResult.S_OK;
                if (_isVbiEnabled)
                {
                  this.LogWarn("WDM analog encoder: failed to disconnect VBI splitter, hr = 0x{0:x}", hr);
                }
              }
              else
              {
                this.LogDebug("WDM analog encoder: reconnect VBI");
                IPin pin = null;
                try
                {
                  pin = DsFindPin.ByDirection(_filterVbiSplitter, PinDirection.Input, 0);
                  hr = filterInfo.pGraph.ConnectDirect(_pinUpstreamVbiOutput, pin, null);
                  _isVbiEnabled = hr == (int)NativeMethods.HResult.S_OK;
                  if (!_isVbiEnabled)
                  {
                    this.LogWarn("WDM analog encoder: failed to reconnect VBI splitter, hr = 0x{0:x}", hr);
                  }
                }
                catch (Exception ex)
                {
                  this.LogWarn(ex, "WDM analog encoder: failed to reconnect VBI splitter");
                }
                finally
                {
                  Release.ComObject("WDM analog encoder VBI splitter filter input pin", ref pin);
                }
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
        muxer.SetActiveComponents(true, true, _isVbiEnabled, true, true);
      }
      else
      {
        muxer.SetActiveComponents(false, true, false, false, false);
      }
    }

    /// <summary>
    /// Unload the component.
    /// </summary>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    public void PerformUnloading(IFilterGraph2 graph)
    {
      this.LogDebug("WDM analog encoder: perform unloading");
      if (graph != null)
      {
        graph.RemoveFilter(_filterTsMultiplexer);
        graph.RemoveFilter(_filterVbiSplitter);
      }

      Release.ComObject("WDM analog encoder TS multiplexer filter", ref _filterTsMultiplexer);
      Release.ComObject("WDM analog encoder upstream VBI output pin", ref _pinUpstreamVbiOutput);
      Release.ComObject("WDM analog encoder VBI splitter filter", ref _filterVbiSplitter);
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