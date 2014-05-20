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
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Multiplexer;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component
{
  /// <summary>
  /// A WDM analog DirectShow encoder graph component.
  /// </summary>
  internal class Encoder : ComponentBase
  {
    #region constants

    private static readonly HashSet<string> CYBERLINK_MULTIPLEXERS = new HashSet<string>
    {
      @"@device:sw:{083863f1-70de-11d0-bd40-00a0c911ce86}\{370e9701-9dc5-42c8-be29-4e75f0629eed}",  // Power Cinema muxer
      @"@device:sw:{083863f1-70de-11d0-bd40-00a0c911ce86}\{6770e328-9b73-40c5-91e6-e2f321aede57}",
      @"@device:sw:{083863f1-70de-11d0-bd40-00a0c911ce86}\{7f2bbeaf-e11c-4d39-90e8-938fb5a86045}"   // Power Director muxer
      //@"@device:sw:{083863f1-70de-11d0-bd40-00a0c911ce86}\{bc650178-0de4-47df-af50-bbd9c7aef5a9}"
    };

    private static IList<AMMediaType> MEDIA_TYPES_CAPTURE = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_VIDEO = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_AUDIO = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_AUDIO_PCM = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_VBI = new List<AMMediaType>();
    private static IList<AMMediaType> MEDIA_TYPES_TELETEXT = new List<AMMediaType>();
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
    /// The software video (or combined video-audio) encoder device.
    /// </summary>
    private DsDevice _deviceCompressorVideo = null;

    /// <summary>
    /// The software audio encoder device.
    /// </summary>
    private DsDevice _deviceCompressorAudio = null;

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

    #region temporary variables for filter connection

    // These allow us to modify the behaviour of the filter connection method
    // in ComponentBase.
    private bool _isVbiConnectAttempt = false;
    private int _filterConnectionCount = 0;
    private IBaseFilter _filterConnectFilters = null;

    #endregion

    #endregion

    #region properties

    /// <summary>
    /// Get the hardware video encoder filter.
    /// </summary>
    public IBaseFilter VideoEncoderFilter
    {
      get
      {
        return _filterEncoderVideo;
      }
    }

    /// <summary>
    /// Get the hardware multiplexer/encoder filter.
    /// </summary>
    public IBaseFilter MultiplexerFilter
    {
      get
      {
        return _filterMultiplexer;
      }
    }

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

      MEDIA_TYPES_CAPTURE.Add(new AMMediaType() { majorType = MediaType.Stream, subType = MediaSubType.Null });
      MEDIA_TYPES_CAPTURE.Add(new AMMediaType() { majorType = MediaType.Video, subType = MediaSubType.Mpeg2Transport });
      MEDIA_TYPES_CAPTURE.Add(new AMMediaType() { majorType = MediaType.Video, subType = MediaSubType.Mpeg2Program });

      MEDIA_TYPES_VIDEO.Add(new AMMediaType() { majorType = MediaType.Video, subType = MediaSubType.Null });
      MEDIA_TYPES_AUDIO.Add(new AMMediaType() { majorType = MediaType.Audio, subType = MediaSubType.Null });
      MEDIA_TYPES_AUDIO_PCM.Add(new AMMediaType() { majorType = MediaType.Audio, subType = MediaSubType.PCM });
      MEDIA_TYPES_VBI.Add(new AMMediaType() { majorType = MediaType.VBI, subType = MediaSubType.Null });
      MEDIA_TYPES_TELETEXT.Add(new AMMediaType() { majorType = MediaType.VBI, subType = MediaSubType.TELETEXT });
      MEDIA_TYPES_CLOSED_CAPTIONS.Add(new AMMediaType() { majorType = MediaType.AuxLine21Data, subType = MediaSubType.Line21_BytePair });
    }

    #endregion

    #region configure

    /// <summary>
    /// Reload the component's configuration.
    /// </summary>
    /// <param name="tunerId">The identifier for the associated tuner.</param>
    public void ReloadConfiguration(int tunerId)
    {
      this.LogDebug("WDM analog encoder: reload configuration");
      ITsMultiplexer multiplexer = _filterTsMultiplexer as ITsMultiplexer;
      if (multiplexer != null)
      {
        int mask = 0;
        if (SettingsManagement.GetValue("tsMuxerDumpInputs", false))
        {
          this.LogDebug("WDM analog encoder: enable TsMuxer input dumping");
          unchecked
          {
            mask = (int)0xffffffff;
          }
        }
        multiplexer.DumpInput(mask);
      }
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
      IPin capturePin = null;
      IPin videoPin = null;
      IPin audioPin = null;
      IPin teletextPin = null;
      IPin closedCaptionPin = null;
      try
      {
        // ------------------------------------------------
        // STAGE 1
        // Add VBI splitter/decoders for teletext and
        // closed caption support.
        // ------------------------------------------------
        if (capture.VideoFilter != null)
        {
          AddAndConnectVbiFilter(graph, capture, out teletextPin, out closedCaptionPin);
        }

        // ------------------------------------------------
        // STAGE 2
        // Add all hardware filters.
        // ------------------------------------------------
        AddAndConnectHardwareFilters(graph, capture, productInstanceId);

        // ------------------------------------------------
        // STAGE 3
        // Add software encoders if necessary.
        // ------------------------------------------------
        // We need software encoder(s) if:
        // - we have no hardware filters and the capture filter doesn't have an
        //   MPEG PS or TS output (ie. capture filter is not an encoder filter)
        // - we have a video encoder but no audio encoder (or vice-versa) and
        //   we know the capture filter produces video and audio
        if (_filterMultiplexer == null && (_filterEncoderVideo == null || _filterEncoderAudio == null))
        {
          if (_filterEncoderVideo == null && _filterEncoderAudio == null)
          {
            if (capture.VideoFilter == null || capture.AudioFilter == null)
            {
              IBaseFilter captureFilter = capture.VideoFilter ?? capture.AudioFilter;
              FindPinByCategoryOrMediaType(captureFilter, Guid.Empty, MEDIA_TYPES_CAPTURE, out capturePin);
            }
            if (capturePin == null)
            {
              AddAndConnectSoftwareFilters(graph, capture, closedCaptionPin);
            }
          }
          else if (capture.VideoFilter != null && capture.AudioFilter != null)
          {
            AddAndConnectSoftwareFilters(graph, capture, closedCaptionPin);
          }
        }

        // ------------------------------------------------
        // STAGE 4
        // Find pins to connect to the TS multiplexer.
        // ------------------------------------------------
        this.LogDebug("WDM analog encoder: find pin(s) for multiplexer");
        if (_filterMultiplexer != null)
        {
          if (!FindPinByCategoryOrMediaType(_filterMultiplexer, Guid.Empty, MEDIA_TYPES_CAPTURE, out capturePin))
          {
            throw new TvException("Failed to find capture pin on multiplexer filter.");
          }
        }
        else if (_filterEncoderVideo != null || _filterEncoderAudio != null)
        {
          if (_filterEncoderVideo == null || _filterEncoderAudio == null)
          {
            IBaseFilter encoderFilter = _filterEncoderVideo ?? _filterEncoderAudio;
            FindPinByCategoryOrMediaType(encoderFilter, Guid.Empty, MEDIA_TYPES_CAPTURE, out capturePin);
          }

          if (capturePin == null)
          {
            if (_filterEncoderVideo != null && !FindPinByCategoryOrMediaType(_filterEncoderVideo, Guid.Empty, MEDIA_TYPES_VIDEO, out videoPin))
            {
              throw new TvException("Failed to find capture or video pin on video encoder filter.");
            }
            if (_filterEncoderAudio != null && !FindPinByCategoryOrMediaType(_filterEncoderAudio, Guid.Empty, MEDIA_TYPES_AUDIO, out audioPin))
            {
              throw new TvException("Failed to find capture or audio pin on audio encoder filter.");
            }
          }
        }

        // ------------------------------------------------
        // STAGE 5
        // Add and connect the MediaPortal multiplexer.
        // ------------------------------------------------
        IList<IPin> pinsToConnect = new List<IPin>();
        if (teletextPin != null)
        {
          this.LogDebug("  teletext pin");
          pinsToConnect.Add(teletextPin);
        }
        if (capturePin != null)
        {
          this.LogDebug("  capture pin");
          pinsToConnect.Add(capturePin);
        }
        else
        {
          if (videoPin != null)
          {
            this.LogDebug("  video pin");
            pinsToConnect.Add(videoPin);
          }
          if (audioPin != null)
          {
            this.LogDebug("  audio pin");
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
        Release.ComObject("encoder final capture pin", ref capturePin);
        Release.ComObject("encoder final video pin", ref videoPin);
        Release.ComObject("encoder final audio pin", ref audioPin);
        Release.ComObject("encoder final teletext pin", ref teletextPin);
        Release.ComObject("encoder final closed caption pin", ref closedCaptionPin);
      }
    }

    private void AddAndConnectHardwareFilters(IFilterGraph2 graph, Capture capture, string productInstanceId)
    {
      this.LogInfo("WDM analog encoder: add hardware encoder(s)");
      if (capture.VideoFilter != null)
      {
        int connectionCount = AddAndConnectFilterFromCategory(graph, FilterCategory.WDMStreamingEncoderDevices, capture.VideoFilter, productInstanceId, out _filterEncoderVideo, out _deviceEncoderVideo);
        if (connectionCount > 1)
        {
          // We assume the video capture and encoder filters also handle audio
          // if more than one connection was made.
          _filterEncoderAudio = _filterEncoderVideo;
          _deviceEncoderAudio = _deviceEncoderVideo;
        }
        else if (connectionCount == 1 && capture.VideoFilter == capture.AudioFilter)
        {
          // We have to ensure that we don't mix up the video and audio
          // encoders. If we have a shared video and audio capture filter but
          // only connected 1 pin to the above encoder, it might be the audio
          // encoder. There is also a chance that it is an audio capture
          // filter.
          this.LogDebug("WDM analog encoder: added and connected filter with one connection, confirm audio/video and encoder/capture");
          IPin pin = null;
          try
          {
            if (FindPinByCategoryOrMediaType(_filterEncoderVideo, Guid.Empty, MEDIA_TYPES_AUDIO_PCM, out pin))
            {
              this.LogDebug("WDM analog encoder: detected audio capture filter");
              capture.SetAudioCapture(_filterEncoderVideo, _deviceEncoderVideo);
              AddAndConnectFilterFromCategory(graph, FilterCategory.WDMStreamingEncoderDevices, capture.VideoFilter, productInstanceId, out _filterEncoderVideo, out _deviceEncoderVideo);
            }
            else if (FindPinByCategoryOrMediaType(_filterEncoderVideo, Guid.Empty, MEDIA_TYPES_AUDIO, out pin))
            {
              this.LogDebug("WDM analog encoder: detected audio encoder");
              _filterEncoderAudio = _filterEncoderVideo;
              _deviceEncoderAudio = _deviceEncoderVideo;
              AddAndConnectFilterFromCategory(graph, FilterCategory.WDMStreamingEncoderDevices, capture.VideoFilter, productInstanceId, out _filterEncoderVideo, out _deviceEncoderVideo);
            }
          }
          finally
          {
            Release.ComObject("encoder video encoder test audio pin", ref pin);
          }
        }
      }
      if (capture.AudioFilter != null && _filterEncoderAudio == null)
      {
        AddAndConnectFilterFromCategory(graph, FilterCategory.WDMStreamingEncoderDevices, capture.AudioFilter, productInstanceId, out _filterEncoderAudio, out _deviceEncoderAudio);
      }

      // Connect a hardware multiplexer filter.
      this.LogInfo("WDM analog encoder: add hardware multiplexer");
      IBaseFilter videoFilter = _filterEncoderVideo ?? capture.VideoFilter;
      if (videoFilter != null)
      {
        AddAndConnectFilterFromCategory(graph, FilterCategory.WDMStreamingMultiplexerDevices, videoFilter, productInstanceId, out _filterMultiplexer, out _deviceMultiplexer);
      }
      // If the video capture or encoder filters also handle audio then audio
      // will be already connected to the multiplexer. However, we may have a
      // separate audio encoder or some other strange case.
      if ((_filterEncoderAudio != null && _filterEncoderVideo != _filterEncoderAudio) || (_filterEncoderAudio == null && capture.AudioFilter != null))
      {
        IBaseFilter audioFilter = _filterEncoderAudio ?? capture.AudioFilter;
        if (_filterMultiplexer == null)
        {
          AddAndConnectFilterFromCategory(graph, FilterCategory.WDMStreamingMultiplexerDevices, audioFilter, productInstanceId, out _filterMultiplexer, out _deviceMultiplexer);
        }
        else
        {
          if (ConnectFiltersByPinName(graph, audioFilter, _filterMultiplexer) == 0)
          {
            // We don't support connecting software audio encoders into
            // hardware multiplexers.
            throw new TvException("Failed to connect separate audio path into hardware multiplexer filter.");
          }
        }
      }
    }

    private void AddAndConnectVbiFilter(IFilterGraph2 graph, Capture capture, out IPin teletextPin, out IPin closedCaptionPin)
    {
      teletextPin = null;
      closedCaptionPin = null;
      IPin vbiPin = null;
      if (FindPinByCategoryOrMediaType(capture.VideoFilter, PinCategory.VBI, MEDIA_TYPES_VBI, out vbiPin))
      {
        try
        {
          // We have a VBI pin. Connect the VBI codec/splitter filter.
          this.LogInfo("WDM analog encoder: add VBI splitter");
          DsDevice d = null;
          _isVbiConnectAttempt = true;
          if (!AddAndConnectFilterFromCategory(graph, MediaPortalGuid.AM_KS_CATEGORY_MULTI_VBI_CODEC, vbiPin, PinDirection.Output, null, out _filterVbiSplitter, out d))
          {
            if (!AddAndConnectFilterFromCategory(graph, FilterCategory.AMKSVBICodec, vbiPin, PinDirection.Output, null, out _filterVbiSplitter, out d))
            {
              this.LogWarn("WDM analog encoder: failed to connect VBI splitter");
            }
          }
          if (d != null)
          {
            d.Dispose();
          }
        }
        finally
        {
          Release.ComObject("encoder upstream VBI source pin", ref vbiPin);
          _isVbiConnectAttempt = false;
        }
      }

      IBaseFilter vbiFilter = _filterVbiSplitter ?? capture.VideoFilter;
      if (FindPinByCategoryOrMediaType(vbiFilter, PinCategory.TeleText, MEDIA_TYPES_TELETEXT, out teletextPin))
      {
        this.LogDebug("WDM analog encoder: found teletext pin");
      }
      if (FindPinByCategoryOrMediaType(vbiFilter, PinCategory.CC, MEDIA_TYPES_CLOSED_CAPTIONS, out closedCaptionPin))
      {
        this.LogDebug("WDM analog encoder: found closed caption pin");
      }
    }

    private void AddAndConnectSoftwareFilters(IFilterGraph2 graph, Capture capture, IPin closedCaptionPin)
    {
      this.LogInfo("WDM analog encoder: add software encoder(s)");
      IPin videoPin = null;
      IPin audioPin = null;

      if (_filterEncoderVideo == null && capture.VideoFilter != null)
      {
        if (!FindPinByCategoryOrMediaType(capture.VideoFilter, Guid.Empty, MEDIA_TYPES_VIDEO, out videoPin))
        {
          throw new TvException("Failed to find capture or video pin on video capture filter.");
        }
      }
      if (_filterEncoderAudio == null && capture.AudioFilter != null)
      {
        if (!FindPinByCategoryOrMediaType(capture.AudioFilter, Guid.Empty, MEDIA_TYPES_AUDIO, out audioPin))
        {
          throw new TvException("Failed to find capture or audio pin on audio capture filter.");
        }
      }

      if (videoPin != null)
      {
        try
        {
          this.LogDebug("WDM analog encoder: add video encoder...");
          if (!AddAndConnectSoftwareEncoder(graph, FilterCategory.VideoCompressorCategory, AnalogManagement.GetSofwareEncodersVideo(), videoPin, out _filterEncoderVideo, out _deviceCompressorVideo))
          {
            throw new TvExceptionSWEncoderMissing("Failed to add a software video encoder.");
          }
        }
        finally
        {
          Release.ComObject("encoder capture filter video output pin", ref videoPin);
        }
      }
      if (audioPin != null || closedCaptionPin != null)
      {
        try
        {
          // Sometimes the video encoder may also encode audio or closed
          // captions. Unfortunately the encoders I've seen don't usually
          // advertise the supported media types on their input pins. We're
          // forced to check the pin name...
          if (_filterEncoderVideo != null)
          {
            IPin inputPin = DsFindPin.ByDirection(_filterEncoderVideo, PinDirection.Input, 1);
            if (inputPin != null)
            {
              try
              {
                string pinName = FilterGraphTools.GetPinName(inputPin);
                IPin pinToConnect = null;
                string pinType = string.Empty;
                this.LogDebug("WDM analog encoder: detected additional input pin {0}, on video encoder", pinName);
                if (DsUtils.GetPinCategory(inputPin) == PinCategory.CC || pinName.ToLowerInvariant().Contains("cc"))
                {
                  pinType = "closed caption";
                  pinToConnect = closedCaptionPin;
                }
                else
                {
                  pinType = "audio";
                  pinToConnect = audioPin;
                  _filterEncoderAudio = _filterEncoderVideo;
                  _deviceCompressorAudio = _deviceCompressorVideo;
                }
                this.LogDebug("WDM analog encoder: pin is {0} pin, connect...", pinType);
                if (pinToConnect != null && graph.ConnectDirect(pinToConnect, inputPin, null) == (int)HResult.Severity.Success)
                {
                  this.LogDebug("WDM analog encoder: connected!");
                }
              }
              finally
              {
                Release.ComObject("encoder software video encoder extra input pin", ref inputPin);
              }
            }
          }
          if (_filterEncoderAudio == null && audioPin != null)
          {
            this.LogDebug("WDM analog encoder: add audio encoder...");
            if (!AddAndConnectSoftwareEncoder(graph, FilterCategory.AudioCompressorCategory, AnalogManagement.GetSofwareEncodersAudio(), audioPin, out _filterEncoderAudio, out _deviceCompressorAudio))
            {
              throw new TvExceptionSWEncoderMissing("Failed to add a software audio encoder.");
            }
          }
        }
        finally
        {
          if (audioPin != null)
          {
            Release.ComObject("encoder capture filter audio output pin", ref audioPin);
          }
        }
      }

      // Add and connect a Cyberlink multiplexer. This is required if a
      // Cyberlink audio encoder is used. If the muxer isn't in the graph, the
      // audio encoder causes an access violation exception when you attempt to
      // start the graph. My guess is that the encoder interacts with the
      // muxer. I tried to mimic interfaces requested via QueryInterface() but
      // ultimately I never managed to make it work.
      if (_deviceCompressorAudio == null || _deviceCompressorAudio.Name == null || !_deviceCompressorAudio.Name.ToLowerInvariant().Contains("cyberlink"))
      {
        return;
      }
      this.LogInfo("WDM analog encoder: add software multiplexer");
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      try
      {
        for (int i = 0; i < devices.Length; i++)
        {
          DsDevice d = devices[i];
          if (d == null || d.Name == null || d.DevicePath == null || d.Mon == null)
          {
            continue;
          }
          string devicePath = d.DevicePath.ToLowerInvariant();
          if (!CYBERLINK_MULTIPLEXERS.Contains(devicePath))
          {
            continue;
          }

          try
          {
            this.LogDebug("WDM analog encoder: attempt to add {0} {1}", d.Name, devicePath);
            _filterMultiplexer = FilterGraphTools.AddFilterFromDevice(graph, d);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "WDM analog encoder: failed to add software multiplexer filter to graph");
            _filterMultiplexer = null;
            continue;
          }

          int connectedPinCount = 0;
          try
          {
            if (_filterEncoderVideo != null)
            {
              connectedPinCount += ConnectFiltersByPinName(graph, _filterEncoderVideo, _filterMultiplexer);
            }
            if (_filterEncoderAudio != null)
            {
              connectedPinCount += ConnectFiltersByPinName(graph, _filterEncoderAudio, _filterMultiplexer);
            }
            if (connectedPinCount != 0)
            {
              break;
            }
          }
          finally
          {
            if (connectedPinCount == 0)
            {
              graph.RemoveFilter(_filterMultiplexer);
              Release.ComObject("encoder compatibility software multiplexer", ref _filterMultiplexer);
              _filterMultiplexer = null;
            }
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
      HResult.ThrowException(hr, "Failed to obtain pin enumerator.");
      try
      {
        // For each output pin...
        int pinCount;
        IPin[] pins = new IPin[2];
        while (enumPins.Next(1, pins, out pinCount) == (int)HResult.Severity.Success && pinCount == 1)
        {
          IPin pin = pins[0];
          try
          {
            // We're not interested in input pins.
            PinDirection direction;
            hr = pin.QueryDirection(out direction);
            HResult.ThrowException(hr, "Failed to query direction for pin.");
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
            HResult.ThrowException(hr, "Failed to obtain media type enumerator for pin.");
            try
            {
              int mediaTypeCount;
              AMMediaType[] mediaTypes = new AMMediaType[2];
              while (enumMediaTypes.Next(1, mediaTypes, out mediaTypeCount) == (int)HResult.Severity.Success && mediaTypeCount == 1)
              {
                AMMediaType mediaType = mediaTypes[0];
                try
                {
                  foreach (AMMediaType mt in matchMediaTypes)
                  {
                    if (mediaType.majorType == mt.majorType && (mediaType.subType == mt.subType || mt.subType == MediaSubType.Null))
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
              Release.ComObject("encoder pin media type enumerator", ref enumMediaTypes);
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
              Release.ComObject("encoder non-matched pin", ref pin);
            }
          }
        }
      }
      finally
      {
        Release.ComObject("encoder pin enumerator", ref enumPins);
      }
      return false;
    }

    private bool AddAndConnectSoftwareEncoder(IFilterGraph2 graph, Guid category, IList<SoftwareEncoder> compatibleEncoders, IPin outputPin, out IBaseFilter filter, out DsDevice device)
    {
      int hr = (int)HResult.Severity.Success;
      filter = null;
      device = null;

      int installedEncoderCount = 0;
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(category);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      DsDevice[] devices = new DsDevice[compatibleEncoders.Count];
      try
      {
        for (int x = 0; x < compatibleEncoders.Count; x++)
        {
          devices[x] = null;
        }
        for (int i = 0; i < devices1.Length; i++)
        {
          bool dispose = true;
          for (int x = 0; x < compatibleEncoders.Count; x++)
          {
            if (devices1[i] != null && compatibleEncoders[x].Name == devices1[i].Name)
            {
              if (devices[x] == null)
              {
                installedEncoderCount++;
              }
              dispose = false;
              devices[x] = devices1[i];
              break;
            }
          }
          if (dispose)
          {
            devices1[i].Dispose();
          }
        }
        for (int i = 0; i < devices2.Length; i++)
        {
          bool dispose = true;
          for (int x = 0; x < compatibleEncoders.Count; x++)
          {
            if (devices2[i] != null && compatibleEncoders[x].Name == devices2[i].Name)
            {
              if (devices[x] == null)
              {
                installedEncoderCount++;
              }
              dispose = false;
              devices[x] = devices2[i];
              break;
            }
          }
          if (dispose)
          {
            devices2[i].Dispose();
          }
        }
        this.LogDebug("WDM analog encoder: add and connect software encoder, compressor count = {0}, legacy filter count = {1}, DB count = {2}, installed count = {3}", devices1.Length, devices2.Length, devices.Length, installedEncoderCount);

        for (int i = 0; i < devices.Length; i++)
        {
          DsDevice d = devices[i];
          if (d == null || !EncodersInUse.Instance.Add(d, compatibleEncoders[i]))
          {
            continue;
          }

          try
          {
            this.LogDebug("WDM analog encoder: attempt to add {0} {1}", d.Name, d.DevicePath);
            filter = FilterGraphTools.AddFilterFromDevice(graph, d);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "WDM analog encoder: failed to add software encoder filter to graph");
            EncodersInUse.Instance.Remove(d);
            filter = null;
            continue;
          }

          this.LogDebug("WDM analog encoder: connect...");
          IPin inputPin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
          try
          {
            hr = graph.ConnectDirect(outputPin, inputPin, null);
            HResult.ThrowException(hr, "Failed to connect software encoder.");
            this.LogDebug("WDM analog encoder: connected!");
            device = d;
            return true;
          }
          catch (Exception ex)
          {
            this.LogError(ex, "WDM analog encoder: failed to connect software encoder");
            EncodersInUse.Instance.Remove(d);
            graph.RemoveFilter(filter);
            Release.ComObject("encoder software encoder filter candidate", ref filter);
          }
          finally
          {
            Release.ComObject("encoder software encoder filter candidate input pin", ref inputPin);
          }
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          if (d != null && d != device)
          {
            d.Dispose();
          }
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
          HResult.ThrowException(hr, "Failed to connect TS multiplexer.");
        }
        catch (Exception ex)
        {
          this.LogError(ex, "WDM analog encoder: failed to connect TS multiplexer pin {0}", pinIndex);
          return false;
        }
        finally
        {
          Release.ComObject("encoder TS muxer input pin " + pinIndex, ref inputPin);
        }
      }
      return true;
    }

    protected int AddAndConnectFilterFromCategory(IFilterGraph2 graph, Guid category, IBaseFilter upstreamFilter, string productInstanceId, out IBaseFilter filter, out DsDevice device)
    {
      _filterConnectFilters = upstreamFilter;
      AddAndConnectFilterFromCategory(graph, category, null, PinDirection.Output, productInstanceId, out filter, out device);
      return _filterConnectionCount;
    }

    protected override bool ConnectFilterWithPin(IFilterGraph2 graph, IPin pinToConnect, PinDirection pinToConnectDirection, IBaseFilter filter)
    {
      if (_isVbiConnectAttempt)
      {
        this.LogDebug("WDM analog encoder: connect filter with pin");
        string filterName = FilterGraphTools.GetFilterName(filter);
        if (!filterName.Equals("VBI Codec") && !filterName.Equals("WST Codec"))
        {
          this.LogDebug("WDM analog encoder: filter \"{0}\" is not on whitelist, ignoring", filterName);
          return false;
        }
        return base.ConnectFilterWithPin(graph, pinToConnect, pinToConnectDirection, filter);
      }
      _filterConnectionCount = ConnectFiltersByPinName(graph, _filterConnectFilters, filter);
      return _filterConnectionCount != 0;
    }

    #endregion

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public void PerformTuning(AnalogChannel channel)
    {
      ITsMultiplexer muxer = _filterTsMultiplexer as ITsMultiplexer;
      if (channel.MediaType == MediaTypeEnum.TV)
      {
        muxer.SetActiveComponents(true, true, true);
      }
      else
      {
        muxer.SetActiveComponents(false, true, false);
      }
    }

    /// <summary>
    /// Unload the component.
    /// </summary>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    public void PerformUnloading(IFilterGraph2 graph)
    {
      if (graph != null)
      {
        graph.RemoveFilter(_filterTsMultiplexer);
        graph.RemoveFilter(_filterVbiSplitter);
      }

      Release.ComObject("encoder TS multiplexer filter", ref _filterTsMultiplexer);
      Release.ComObject("encoder VBI splitter filter", ref _filterVbiSplitter);

      if (_filterEncoderAudio != null && _filterEncoderAudio != _filterEncoderVideo)
      {
        if (graph != null)
        {
          graph.RemoveFilter(_filterEncoderAudio);
        }
        Release.ComObject("encoder audio encoder filter", ref _filterEncoderAudio);

        if (_deviceEncoderAudio != null)
        {
          DevicesInUse.Instance.Remove(_deviceEncoderAudio);
          _deviceEncoderAudio.Dispose();
          _deviceEncoderAudio = null;
        }
        else
        {
          EncodersInUse.Instance.Remove(_deviceCompressorAudio);
          _deviceCompressorAudio.Dispose();
          _deviceCompressorAudio = null;
        }
      }
      if (_filterEncoderVideo != null)
      {
        if (graph != null)
        {
          graph.RemoveFilter(_filterEncoderVideo);
        }
        Release.ComObject("encoder video encoder filter", ref _filterEncoderVideo);

        if (_deviceEncoderVideo != null)
        {
          DevicesInUse.Instance.Remove(_deviceEncoderVideo);
          _deviceEncoderVideo.Dispose();
          _deviceEncoderVideo = null;
        }
        else
        {
          EncodersInUse.Instance.Remove(_deviceCompressorVideo);
          _deviceCompressorVideo.Dispose();
          _deviceCompressorVideo = null;
        }
      }
      if (_filterMultiplexer != null)
      {
        if (graph != null)
        {
          graph.RemoveFilter(_filterMultiplexer);
        }
        Release.ComObject("encoder hardware multiplexer filter", ref _filterMultiplexer);

        DevicesInUse.Instance.Remove(_deviceMultiplexer);
        _deviceMultiplexer.Dispose();
        _deviceMultiplexer = null;
      }
    }
  }
}