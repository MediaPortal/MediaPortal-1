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
using System.Runtime.InteropServices;
using DirectShowLib;
using TvLibrary.Implementations.DVB;
using TvDatabase;

namespace TvLibrary.Implementations.Analog.Components
{
  internal class Encoder
  {
    #region constants

    [ComImport, Guid("511d13f0-8a56-42fa-b151-b72a325cf71a")]
    private class MpTsMuxer
    {
    }

    private static readonly HashSet<string> CYBERLINK_MULTIPLEXERS = new HashSet<string>
    {
      @"@device:sw:{083863f1-70de-11d0-bd40-00a0c911ce86}\{370e9701-9dc5-42c8-be29-4e75f0629eed}",
      @"@device:sw:{083863f1-70de-11d0-bd40-00a0c911ce86}\{6770e328-9b73-40c5-91e6-e2f321aede57}",
      @"@device:sw:{083863f1-70de-11d0-bd40-00a0c911ce86}\{7f2bbeaf-e11c-4d39-90e8-938fb5a86045}"
    };

    private static IList<AMMediaType> CAPTURE_MEDIA_TYPES = new List<AMMediaType>();
    private static IList<AMMediaType> VIDEO_MEDIA_TYPES = new List<AMMediaType>();
    private static IList<AMMediaType> AUDIO_MEDIA_TYPES = new List<AMMediaType>();
    private static IList<AMMediaType> VBI_MEDIA_TYPES = new List<AMMediaType>();
    private static IList<AMMediaType> TELETEXT_MEDIA_TYPES = new List<AMMediaType>();
    private static IList<AMMediaType> CLOSED_CAPTIONS_MEDIA_TYPES = new List<AMMediaType>();

    #endregion

    #region variables

    /// <summary>
    /// The hardware video (or combined video-audio) encoder device.
    /// </summary>
    private DsDevice _videoEncoderDevice = null;

    /// <summary>
    /// The hardware audio encoder device.
    /// </summary>
    private DsDevice _audioEncoderDevice = null;

    /// <summary>
    /// The software video (or combined video-audio) encoder device.
    /// </summary>
    private DsDevice _videoCompressorDevice = null;

    /// <summary>
    /// The software audio encoder device.
    /// </summary>
    private DsDevice _audioCompressorDevice = null;

    /// <summary>
    /// The hardware multiplexer/encoder device.
    /// </summary>
    private DsDevice _multiplexerDevice = null;

    /// <summary>
    /// The hardware or software video (or combined video-audio) encoder/multiplexer filter.
    /// </summary>
    private IBaseFilter _filterVideoEncoder = null;

    /// <summary>
    /// The hardware or software audio encoder filter.
    /// </summary>
    private IBaseFilter _filterAudioEncoder = null;

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

    #endregion

    #region properties

    /// <summary>
    /// Get the hardware video encoder filter.
    /// </summary>
    public IBaseFilter VideoEncoderFilter
    {
      get
      {
        return _filterVideoEncoder;
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

    public Encoder()
    {
      AMMediaType mt1 = new AMMediaType();
      mt1.majorType = MediaType.Stream;
      mt1.subType = MediaSubType.Null;
      AMMediaType mt2 = new AMMediaType();
      mt2.majorType = MediaType.Video;
      mt2.subType = MediaSubType.Mpeg2Transport;
      AMMediaType mt3 = new AMMediaType();
      mt3.majorType = MediaType.Video;
      mt3.subType = MediaSubType.Mpeg2Program;
      AMMediaType mt4 = new AMMediaType();
      mt4.majorType = MediaType.Video;
      mt4.subType = MediaSubType.Null;
      AMMediaType mt5 = new AMMediaType();
      mt5.majorType = MediaType.Audio;
      mt5.subType = MediaSubType.Null;
      AMMediaType mt6 = new AMMediaType();
      mt6.majorType = MediaType.VBI;
      mt6.subType = MediaSubType.Null;
      AMMediaType mt7 = new AMMediaType();
      mt7.majorType = MediaType.VBI;
      mt7.subType = MediaSubType.TELETEXT;
      AMMediaType mt8 = new AMMediaType();
      mt8.majorType = MediaType.AuxLine21Data;
      mt8.subType = MediaSubType.Line21_BytePair;

      if (CAPTURE_MEDIA_TYPES.Count == 0)
      {
        CAPTURE_MEDIA_TYPES.Add(mt1);
        CAPTURE_MEDIA_TYPES.Add(mt2);
        CAPTURE_MEDIA_TYPES.Add(mt3);

        VIDEO_MEDIA_TYPES.Add(mt4);
        AUDIO_MEDIA_TYPES.Add(mt5);
        VBI_MEDIA_TYPES.Add(mt6);
        TELETEXT_MEDIA_TYPES.Add(mt7);
        CLOSED_CAPTIONS_MEDIA_TYPES.Add(mt8);
      }
    }

    public void Dispose()
    {
      if (_filterVideoEncoder != null)
      {
        Release.ComObject(_filterVideoEncoder);
        _filterVideoEncoder = null;
      }
      if (_filterAudioEncoder != null)
      {
        Release.ComObject(_filterAudioEncoder);
        _filterAudioEncoder = null;
      }
      if (_filterMultiplexer != null)
      {
        Release.ComObject(_filterMultiplexer);
        _filterMultiplexer = null;
      }
      if (_filterTsMultiplexer != null)
      {
        Release.ComObject(_filterTsMultiplexer);
        _filterTsMultiplexer = null;
      }
      if (_filterVbiSplitter != null)
      {
        Release.ComObject(_filterVbiSplitter);
        _filterVbiSplitter = null;
      }
      if (_videoEncoderDevice != null)
      {
        DevicesInUse.Instance.Remove(_videoEncoderDevice);
        _videoEncoderDevice = null;
      }
      if (_audioEncoderDevice != null)
      {
        DevicesInUse.Instance.Remove(_audioEncoderDevice);
        _audioEncoderDevice = null;
      }
      if (_multiplexerDevice != null)
      {
        DevicesInUse.Instance.Remove(_multiplexerDevice);
        _multiplexerDevice = null;
      }
      if (_videoCompressorDevice != null)
      {
        EncodersInUse.Instance.Remove(_videoCompressorDevice);
        _videoCompressorDevice = null;
      }
      if (_audioCompressorDevice != null)
      {
        EncodersInUse.Instance.Remove(_audioCompressorDevice);
        _audioCompressorDevice = null;
      }
    }

    private static string GetPinName(IPin pin)
    {
      PinInfo pinInfo;
      int hr = pin.QueryPinInfo(out pinInfo);
      DsError.ThrowExceptionForHR(hr);
      if (pinInfo.filter != null)
      {
        Release.ComObject(pinInfo.filter);
      }
      return pinInfo.name;
    }

    /// <summary>
    /// Creates the encoder and teletext component.
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
    /// </summary>
    /// <param name="graph">The graph builder.</param>
    /// <param name="crossbar">The crossbar component.</param>
    /// <param name="capture">The capture component.</param>
    /// <returns><c>true</c> if graph building was successful, otherwise <c>false</c></returns>
    public bool CreateFilterInstance(IFilterGraph2 graph, Crossbar crossbar, Capture capture)
    {
      // ------------------------------------------------
      // STAGE 1
      // Add all hardware filters.
      // ------------------------------------------------
      string requiredDevicePathSection = string.Empty;
      string[] devicePathSections = crossbar.DevicePath.Split('#');
      if (devicePathSections.Length == 4 && !devicePathSections[0].Contains("stream"))
      {
        requiredDevicePathSection = devicePathSections[2];
        Log.Log.Debug("encoder: required device path section = {0}", requiredDevicePathSection);
      }

      AddAndConnectHardwareFilters(graph, capture, requiredDevicePathSection);

      IPin capturePin = null;
      IPin videoPin = null;
      IPin audioPin = null;
      IPin teletextPin = null;
      IPin closedCaptionPin = null;
      try
      {
        // ------------------------------------------------
        // STAGE 2
        // Add VBI splitter/decoders for teletext and
        // closed caption support.
        // ------------------------------------------------
        if (capture.VideoFilter != null)
        {
          AddAndConnectVbiFilter(graph, capture, out teletextPin, out closedCaptionPin);
        }

        // ------------------------------------------------
        // STAGE 3
        // Add software encoders if necessary.
        // ------------------------------------------------
        if (_filterMultiplexer == null && _filterVideoEncoder == null && _filterAudioEncoder == null)
        {
          // We have no hardware filters. If the capture filter doesn't have an
          // MPEG PS or TS output (ie. capture filter is not an encoder filter)
          // then we need software encoders.
          if (capture.VideoFilter == null || capture.AudioFilter == null)
          {
            IBaseFilter captureFilter = capture.VideoFilter ?? capture.AudioFilter;
            FindPinByCategoryOrMediaType(captureFilter, Guid.Empty, CAPTURE_MEDIA_TYPES, out capturePin);
          }
          if (capturePin == null)
          {
            // Software encoder(s) (and maybe a multiplexer for compatibility)
            // are required.
            AddAndConnectSoftwareFilters(graph, capture, closedCaptionPin, out videoPin, out audioPin);
          }
        }

        // ------------------------------------------------
        // STAGE 4
        // Find pins to connect to the TS multiplexer.
        // ------------------------------------------------
        Log.Log.Debug("encoder: find pin(s) for multiplexer");
        if (_filterMultiplexer != null)
        {
          if (!FindPinByCategoryOrMediaType(_filterMultiplexer, Guid.Empty, CAPTURE_MEDIA_TYPES, out capturePin))
          {
            throw new TvExceptionGraphBuildingFailed("Failed to locate capture pin on multiplexer filter.");
          }
        }
        else if (_filterVideoEncoder != null || _filterAudioEncoder != null)
        {
          if (_filterVideoEncoder == null || _filterAudioEncoder == null)
          {
            IBaseFilter encoderFilter = _filterVideoEncoder ?? _filterAudioEncoder;
            FindPinByCategoryOrMediaType(encoderFilter, Guid.Empty, CAPTURE_MEDIA_TYPES, out capturePin);
          }

          if (capturePin == null)
          {
            if (_filterVideoEncoder != null)
            {
              if (!FindPinByCategoryOrMediaType(_filterVideoEncoder, Guid.Empty, VIDEO_MEDIA_TYPES, out videoPin))
              {
                throw new TvExceptionGraphBuildingFailed("Failed to locate capture or video pin on video encoder filter.");
              }
            }
            IBaseFilter audioEncoderFilter = _filterAudioEncoder ?? _filterVideoEncoder;
            if (audioEncoderFilter != null)
            {
              FindPinByCategoryOrMediaType(audioEncoderFilter, Guid.Empty, AUDIO_MEDIA_TYPES, out audioPin);
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
          pinsToConnect.Add(teletextPin);
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
          throw new TvExceptionGraphBuildingFailed("Failed to add and connect TS multiplexer.");
        }
      }
      finally
      {
        if (capturePin != null)
        {
          Release.ComObject(capturePin);
        }
        if (videoPin != null)
        {
          Release.ComObject(videoPin);
        }
        if (audioPin != null)
        {
          Release.ComObject(audioPin);
        }
        if (teletextPin != null)
        {
          Release.ComObject(teletextPin);
        }
        if (closedCaptionPin != null)
        {
          Release.ComObject(closedCaptionPin);
        }
      }
      return true;
    }

    private void AddAndConnectHardwareFilters(IFilterGraph2 graph, Capture capture, string requiredDevicePathSection)
    {
      Log.Log.Info("encoder: add hardware encoder(s)");
      if (capture.VideoFilter != null)
      {
        AddAndConnectFilterFromCategory(FilterCategory.WDMStreamingEncoderDevices, graph, capture.VideoFilter, requiredDevicePathSection, out _filterVideoEncoder, out _videoEncoderDevice);
      }
      if (capture.AudioFilter != null)
      {
        // If we have a separate audio capture filter, try to connect the video
        // encoder filter in case it also handles audio. Otherwise, attempt to
        // connect another filter.
        if (_filterVideoEncoder == null || ConnectFiltersByPinName(graph, capture.AudioFilter, _filterVideoEncoder) == 0)
        {
          AddAndConnectFilterFromCategory(FilterCategory.WDMStreamingEncoderDevices, graph, capture.AudioFilter, requiredDevicePathSection, out _filterAudioEncoder, out _audioEncoderDevice);
        }
      }

      // Connect a hardware multiplexer filter.
      Log.Log.Info("encoder: add hardware multiplexer");
      IBaseFilter videoFilter = _filterVideoEncoder ?? capture.VideoFilter;
      if (videoFilter != null)
      {
        AddAndConnectFilterFromCategory(FilterCategory.WDMStreamingMultiplexerDevices, graph, videoFilter, requiredDevicePathSection, out _filterMultiplexer, out _multiplexerDevice);
      }
      if (_filterAudioEncoder != null || capture.AudioFilter != null)
      {
        // If we have a separate audio chain, try to connect it into the
        // multiplexer.
        IBaseFilter audioFilter = _filterAudioEncoder ?? capture.AudioFilter;
        if (_filterMultiplexer != null || ConnectFiltersByPinName(graph, audioFilter, _filterMultiplexer) == 0)
        {
          AddAndConnectFilterFromCategory(FilterCategory.WDMStreamingMultiplexerDevices, graph, audioFilter, requiredDevicePathSection, out _filterMultiplexer, out _multiplexerDevice);
        }
      }
    }

    private void AddAndConnectVbiFilter(IFilterGraph2 graph, Capture capture, out IPin teletextPin, out IPin closedCaptionPin)
    {
      teletextPin = null;
      closedCaptionPin = null;
      IPin vbiPin = null;
      if (FindPinByCategoryOrMediaType(capture.VideoFilter, PinCategory.VBI, VBI_MEDIA_TYPES, out vbiPin))
      {
        // We have a VBI pin. Connect the VBI codec/splitter filter.
        Log.Log.Info("encoder: add VBI codec");
        int hr = 0;
        try
        {
          DsDevice[] devices1 = DsDevice.GetDevicesOfCat(FilterCategory.AMKSMULTIVBICodec);
          DsDevice[] devices2 = DsDevice.GetDevicesOfCat(FilterCategory.AMKSVBICodec);
          DsDevice[] devices = new DsDevice[devices1.Length + devices2.Length];
          devices1.CopyTo(devices, 0);
          devices2.CopyTo(devices, devices1.Length);
          foreach (DsDevice d in devices)
          {
            if (d == null || d.Name == null || (!d.Name.Equals("VBI Codec") && !d.Name.Equals("WST Codec")))
            {
              continue;
            }

            Log.Log.Debug("encoder: attempt to add {0} {1}", d.Name, d.DevicePath);
            try
            {
              hr = graph.AddSourceFilterForMoniker(d.Mon, null, d.Name, out _filterVbiSplitter);
              DsError.ThrowExceptionForHR(hr);
            }
            catch
            {
              Log.Log.Debug("encoder: failed to add, hr = 0x{0:x}", hr);
              _filterVbiSplitter = null;
              continue;
            }

            IPin inputPin = null;
            try
            {
              inputPin = DsFindPin.ByDirection(_filterVbiSplitter, PinDirection.Input, 0);
              hr = graph.ConnectDirect(vbiPin, inputPin, null);
              DsError.ThrowExceptionForHR(hr);
              Log.Log.Debug("encoder: connected!");
              break;
            }
            catch
            {
              Log.Log.Debug("encoder: failed to connect, hr = 0x{0:x}", hr);
              graph.RemoveFilter(_filterVbiSplitter);
              Release.ComObject(_filterVbiSplitter);
              _filterVbiSplitter = null;
            }
            finally
            {
              if (inputPin != null)
              {
                Release.ComObject(inputPin);
              }
            }
          }
        }
        finally
        {
          Release.ComObject(vbiPin);
        }
      }

      IBaseFilter vbiFilter = _filterVbiSplitter ?? capture.VideoFilter;
      if (FindPinByCategoryOrMediaType(vbiFilter, PinCategory.TeleText, TELETEXT_MEDIA_TYPES, out teletextPin))
      {
        Log.Log.Debug("encoder: found teletext pin");
      }
      if (FindPinByCategoryOrMediaType(vbiFilter, PinCategory.CC, CLOSED_CAPTIONS_MEDIA_TYPES, out closedCaptionPin))
      {
        Log.Log.Debug("encoder: found closed caption pin");
      }
    }

    private void AddAndConnectSoftwareFilters(IFilterGraph2 graph, Capture capture, IPin closedCaptionPin, out IPin videoPin, out IPin audioPin)
    {
      Log.Log.Info("encoder: add software encoder(s)");
      videoPin = null;
      audioPin = null;

      if (capture.VideoFilter != null)
      {
        if (!FindPinByCategoryOrMediaType(capture.VideoFilter, Guid.Empty, VIDEO_MEDIA_TYPES, out videoPin))
        {
          throw new TvExceptionGraphBuildingFailed("Failed to locate capture or video pin on video capture filter.");
        }
      }
      IBaseFilter audioCaptureFilter = capture.AudioFilter ?? capture.VideoFilter;
      if (audioCaptureFilter != null)
      {
        FindPinByCategoryOrMediaType(audioCaptureFilter, Guid.Empty, AUDIO_MEDIA_TYPES, out audioPin);
      }

      // The Plextor capture filter is a video encoder filter, but not an audio
      // encoder filter.
      TvBusinessLayer layer = new TvBusinessLayer();
      if (videoPin != null && !capture.VideoName.StartsWith("Plextor ConvertX"))
      {
        Log.Log.Debug("encoder: add video encoder...");
        if (!AddAndConnectSoftwareEncoder(graph, FilterCategory.VideoCompressorCategory, layer.GetSofwareEncodersVideo(), videoPin, out _filterVideoEncoder, out _videoCompressorDevice))
        {
          throw new TvExceptionSWEncoderMissing("Failed to add a software video encoder.");
        }
      }
      if (audioPin != null || closedCaptionPin != null)
      {
        // Sometimes the video encoder may also encode audio or closed
        // captions. Unfortunately the encoders I've seen don't usually
        // advertise the supported media types on their input pins. We're
        // forced to check the pin name...
        bool connectedAudio = false;
        if (_filterVideoEncoder != null)
        {
          IPin inputPin = DsFindPin.ByDirection(_filterVideoEncoder, PinDirection.Input, 1);
          if (inputPin != null)
          {
            try
            {
              string pinName = GetPinName(inputPin);
              IPin pinToConnect = null;
              string pinType = string.Empty;
              Log.Log.Debug("encoder: detected additional input pin {0}, on video encoder", pinName);
              if (DsUtils.GetPinCategory(inputPin) == PinCategory.CC || pinName.ToLowerInvariant().Contains("cc"))
              {
                pinType = "closed caption";
                pinToConnect = closedCaptionPin;
              }
              else
              {
                pinType = "audio";
                pinToConnect = audioPin;
              }
              Log.Log.Debug("encoder: pin is {0} pin, connect...", pinType);
              if (graph.ConnectDirect(pinToConnect, inputPin, null) == 0)
              {
                Log.Log.Debug("encoder: connected!");
                connectedAudio = pinType.Equals("audio");
              }
            }
            finally
            {
              Release.ComObject(inputPin);
            }
          }
        }
        if (!connectedAudio && audioPin != null)
        {
          Log.Log.Debug("encoder: add audio encoder...");
          if (!AddAndConnectSoftwareEncoder(graph, FilterCategory.AudioCompressorCategory, layer.GetSofwareEncodersAudio(), audioPin, out _filterAudioEncoder, out _audioCompressorDevice))
          {
            throw new TvExceptionSWEncoderMissing("Failed to add a software audio encoder.");
          }
        }
      }

      // Add and connect a Cyberlink multiplexer. This is required if a
      // Cyberlink audio encoder is used. If the muxer isn't in the graph, the
      // audio encoder causes an access violation exception when you attempt to
      // start the graph. My guess is that the encoder interacts with the
      // muxer. I tried to mimic interfaces requested via QueryInterface() but
      // ultimately I never managed to make it work.
      if (_audioCompressorDevice == null || _audioCompressorDevice.Name == null || !_audioCompressorDevice.Name.ToLowerInvariant().Contains("cyberlink"))
      {
        return;
      }
      Log.Log.Info("encoder: add software multiplexer");
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      int hr = 0;
      for (int i = 0; i < devices.Length; i++)
      {
        DsDevice d = devices[i];
        if (d == null || d.Name == null || d.DevicePath == null)
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
          Log.Log.Debug("encoder: attempt to add {0} {1}", d.Name, d.DevicePath);
          hr = graph.AddSourceFilterForMoniker(d.Mon, null, d.Name, out _filterMultiplexer);
          DsError.ThrowExceptionForHR(hr);
        }
        catch
        {
          Log.Log.Debug("encoder: failed to add, hr = 0x{0:x}", hr);
          _filterMultiplexer = null;
          continue;
        }

        int connectedPinCount = 0;
        try
        {
          if (_filterVideoEncoder != null)
          {
            connectedPinCount += ConnectFiltersByPinName(graph, _filterVideoEncoder, _filterMultiplexer);
          }
          if (_filterAudioEncoder != null)
          {
            connectedPinCount += ConnectFiltersByPinName(graph, _filterAudioEncoder, _filterMultiplexer);
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
            Release.ComObject(_filterMultiplexer);
            _filterMultiplexer = null;
          }
        }
      }
    }

    private int AddAndConnectFilterFromCategory(Guid category, IFilterGraph2 graph, IBaseFilter upstreamFilter, string deviceIdentifier, out IBaseFilter filter, out DsDevice device)
    {
      filter = null;
      device = null;
      int connectedPinCount = 0;
      int hr = 0;

      // Sort the devices in the category of interest based on whether the
      // device path matches.
      DsDevice[] devices = DsDevice.GetDevicesOfCat(category);
      Log.Log.Debug("encoder: add and connect filter from category {0}, device count = {1}", category, devices.Length);
      if (!string.IsNullOrEmpty(deviceIdentifier))
      {
        Array.Sort(devices, delegate(DsDevice d1, DsDevice d2)
        {
          bool d1Result = false;
          bool d2Result = false;
          if (d1 != null && d1.DevicePath != null)
          {
            d1Result = d1.DevicePath.Contains(deviceIdentifier);
          }
          if (d2 != null && d2.DevicePath != null)
          {
            d2Result = d2.DevicePath.Contains(deviceIdentifier);
          }
          if (d1Result && !d2Result)
          {
            return -1;
          }
          if (!d1Result && d2Result)
          {
            return 1;
          }
          return 0;
        });
      }

      // For each device...
      foreach (DsDevice d in devices)
      {
        if (d == null || DevicesInUse.Instance.IsUsed(d))
        {
          Log.Log.Debug("encoder: device null or in use");
          continue;
        }

        // Instantiate the corresponding filter.
        Log.Log.Debug("encoder: attempt to add {0} {1}", d.Name, d.DevicePath);
        try
        {
          hr = graph.AddSourceFilterForMoniker(d.Mon, null, d.Name, out filter);
          DsError.ThrowExceptionForHR(hr);
        }
        catch
        {
          Log.Log.Debug("encoder: failed to add, hr = 0x{0:x}", hr);
          filter = null;
          continue;
        }
        DevicesInUse.Instance.Add(d);
        device = d;

        try
        {
          connectedPinCount = ConnectFiltersByPinName(graph, upstreamFilter, filter);
          if (connectedPinCount != 0)
          {
            break;
          }
        }
        finally
        {
          if (connectedPinCount == 0)
          {
            DevicesInUse.Instance.Remove(d);
            graph.RemoveFilter(filter);
            Release.ComObject(filter);
            filter = null;
            device = null;
          }
        }
      }

      return connectedPinCount;
    }

    private int ConnectFiltersByPinName(IFilterGraph2 graph, IBaseFilter upstreamFilter, IBaseFilter filter)
    {
      Log.Log.Debug("encoder: connect filters by pin name");
      int connectedPinCount = 0;

      // Check if we can connect one or more of the filter's input pins to
      // one or more of the upstream filter's output pins.
      IEnumPins targetFilterPinEnum;
      int hr = filter.EnumPins(out targetFilterPinEnum);
      DsError.ThrowExceptionForHR(hr);
      try
      {
        // Attempt to connect each input pin on the filter. We prefer to
        // connect pins with matching names.
        int pinCount = 0;
        IPin[] inputPins = new IPin[2];
        while (targetFilterPinEnum.Next(1, inputPins, out pinCount) == 0 && pinCount == 1)
        {
          bool connected = false;
          IPin inputPin = inputPins[0];
          try
          {
            // We're not interested in output pins.
            PinDirection direction;
            hr = inputPin.QueryDirection(out direction);
            DsError.ThrowExceptionForHR(hr);
            if (direction == PinDirection.Output)
            {
              continue;
            }

            string inputPinName = GetPinName(inputPin);
            Log.Log.Debug("encoder: next input pin {0}...", inputPinName);
            IEnumPins upstreamFilterPinEnum;
            hr = upstreamFilter.EnumPins(out upstreamFilterPinEnum);
            DsError.ThrowExceptionForHR(hr);
            IPin[] outputPins = new IPin[20];
            hr = upstreamFilterPinEnum.Next(20, outputPins, out pinCount);
            Release.ComObject(upstreamFilterPinEnum);
            DsError.ThrowExceptionForHR(hr);
            Log.Log.Debug("encoder: upstream filter pin count = {0}", pinCount);

            // Try to connect the upstream pin with an input pin that has
            // an identical name.
            IList<IPin> skippedPins = new List<IPin>();
            for (int p = 0; p < pinCount; p++)
            {
              IPin outputPin = outputPins[p];
              bool pinSkipped = false;
              try
              {
                // We're not interested in input pins.
                hr = outputPin.QueryDirection(out direction);
                DsError.ThrowExceptionForHR(hr);
                if (direction == PinDirection.Input)
                {
                  Log.Log.Debug("encoder: pin {0} is an input pin", p);
                  continue;
                }

                // We can't use pins that are already connected.
                IPin tempPin = null;
                outputPin.ConnectedTo(out tempPin);
                if (tempPin != null)
                {
                  Log.Log.Debug("encoder: output pin {0} already connected", p);
                  Release.ComObject(tempPin);
                  continue;
                }

                // If the pin names don't match then skip the pin for now.
                string pinName = GetPinName(outputPin);
                Log.Log.Debug("encoder: output pin {0} name = {1}", p, pinName);
                if (!inputPinName.Equals(pinName))
                {
                  Log.Log.Debug("encoder: skipped for now...");
                  pinSkipped = true;
                  skippedPins.Add(outputPin);
                  continue;
                }
                try
                {
                  hr = graph.ConnectDirect(outputPin, inputPin, null);
                  DsError.ThrowExceptionForHR(hr);
                  Log.Log.Debug("encoder: connected!");
                  connected = true;
                  break;
                }
                catch
                {
                  // connection failed, move on to next output pin
                }
              }
              finally
              {
                if (!pinSkipped)
                {
                  Release.ComObject(outputPin);
                }
              }
            }

            // Fallback: try to connect with the pins we skipped previously.
            if (!connected)
            {
              Log.Log.Debug("encoder: fallback to non-matching pins");
              foreach (IPin outputPin in skippedPins)
              {
                Log.Log.Debug("encoder: output pin...");
                try
                {
                  hr = graph.ConnectDirect(outputPin, inputPin, null);
                  DsError.ThrowExceptionForHR(hr);
                  Log.Log.Debug("encoder: connected!");
                  connected = true;
                  break;
                }
                catch
                {
                  // connection failed, move on to next output pin
                }
                finally
                {
                  Release.ComObject(outputPin);
                }
              }
            }

            if (connected)
            {
              connectedPinCount++;
            }
          }
          catch (Exception ex)
          {
            Log.Log.Error("Unexpected error in Encoder.ConnectFiltersByPinName()\r\n{0}", ex.ToString());
          }
          finally
          {
            Release.ComObject(inputPin);
          }
        }
      }
      finally
      {
        Release.ComObject(targetFilterPinEnum);
      }
      Log.Log.Debug("encoder: connected {0} pin(s)", connectedPinCount);
      return connectedPinCount;
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
      DsError.ThrowExceptionForHR(hr);
      try
      {
        // For each output pin...
        int pinCount;
        IPin[] pins = new IPin[2];
        while (enumPins.Next(1, pins, out pinCount) == 0 && pinCount == 1)
        {
          IPin pin = pins[0];
          try
          {
            // We're not interested in input pins.
            PinDirection direction;
            hr = pin.QueryDirection(out direction);
            DsError.ThrowExceptionForHR(hr);
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
            DsError.ThrowExceptionForHR(hr);
            try
            {
              int mediaTypeCount;
              AMMediaType[] mediaTypes = new AMMediaType[2];
              while (enumMediaTypes.Next(1, mediaTypes, out mediaTypeCount) == 0 && mediaTypeCount == 1)
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
                  DsUtils.FreeAMMediaType(mediaType);
                }
              }
            }
            finally
            {
              Release.ComObject(enumMediaTypes);
            }
          }
          catch (Exception ex)
          {
            Log.Log.Error("Unexpected error in Encoder.FindPinByMediaType()\r\n{0}", ex.ToString());
          }
          finally
          {
            if (matchedPin == null && pin != null)
            {
              Release.ComObject(pin);
            }
          }
        }
      }
      finally
      {
        Release.ComObject(enumPins);
      }
      return false;
    }

    private bool AddAndConnectSoftwareEncoder(IFilterGraph2 graph, Guid category, IList<SoftwareEncoder> compatibleEncoders, IPin outputPin, out IBaseFilter filter, out DsDevice device)
    {
      int hr = 0;
      filter = null;
      device = null;

      int installedEncoderCount = 0;
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(category);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      DsDevice[] devices = new DsDevice[compatibleEncoders.Count];
      for (int x = 0; x < compatibleEncoders.Count; ++x)
      {
        devices[x] = null;
      }
      for (int i = 0; i < devices1.Length; i++)
      {
        for (int x = 0; x < compatibleEncoders.Count; ++x)
        {
          if (compatibleEncoders[x].Name == devices1[i].Name)
          {
            devices[x] = devices1[i];
            installedEncoderCount++;
            break;
          }
        }
      }
      for (int i = 0; i < devices2.Length; i++)
      {
        for (int x = 0; x < compatibleEncoders.Count; ++x)
        {
          if (compatibleEncoders[x].Name == devices2[i].Name)
          {
            if (devices[x] == null)
            {
              installedEncoderCount++;
            }
            devices[x] = devices2[i];
            break;
          }
        }
      }
      Log.Log.Debug("encoder: add and connect software encoder, compressor count = {0}, legacy filter count = {1}, DB count = {2}, installed count = {3}", devices1.Length, devices2.Length, devices.Length, installedEncoderCount);

      for (int i = 0; i < devices.Length; i++)
      {
        DsDevice d = devices[i];
        if (d == null || !EncodersInUse.Instance.Add(d, compatibleEncoders[i]))
        {
          continue;
        }

        try
        {
          Log.Log.Debug("encoder: attempt to add {0} {1}", d.Name, d.DevicePath);
          hr = graph.AddSourceFilterForMoniker(d.Mon, null, d.Name, out filter);
          DsError.ThrowExceptionForHR(hr);
        }
        catch
        {
          Log.Log.Debug("encoder: failed to add, hr = 0x{0:x}", hr);
          EncodersInUse.Instance.Remove(d);
          filter = null;
          continue;
        }

        Log.Log.Debug("encoder: connect...");
        bool connected = false;
        IPin inputPin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
        try
        {
          hr = graph.ConnectDirect(outputPin, inputPin, null);
          DsError.ThrowExceptionForHR(hr);
          Log.Log.Debug("encoder: connected!");
          device = d;
          connected = true;
          return true;
        }
        catch
        {
        }
        finally
        {
          if (inputPin != null)
          {
            Release.ComObject(inputPin);
          }
          if (!connected)
          {
            Log.Log.Debug("encoder: failed to connect, hr = 0x{0:x}", hr);
            EncodersInUse.Instance.Remove(d);
            graph.RemoveFilter(filter);
            Release.ComObject(filter);
            filter = null;
          }
        }
      }
      return false;
    }

    private bool AddAndConnectTsMultiplexer(IFilterGraph2 graph, IList<IPin> pinsToConnect)
    {
      Log.Log.Debug("encoder: add and connect TS multiplexer");
      int hr = 0;
      _filterTsMultiplexer = (IBaseFilter)new MpTsMuxer();
      try
      {
        hr = graph.AddFilter(_filterTsMultiplexer, "MediaPortal TS Multiplexer");
        DsError.ThrowExceptionForHR(hr);
      }
      catch
      {
        Log.Log.Debug("encoder: failed to add, hr = 0x{0:x}", hr);
        return false;
      }

      IPin inputPin = null;
      int pinIndex = 0;
      foreach (IPin pinToConnect in pinsToConnect)
      {
        try
        {
          inputPin = DsFindPin.ByDirection(_filterTsMultiplexer, PinDirection.Input, pinIndex++);
          hr = graph.ConnectDirect(pinToConnect, inputPin, null);
          DsError.ThrowExceptionForHR(hr);
        }
        catch
        {
          Log.Log.Debug("encoder: failed to connect pin {0}, hr = 0x{1:x}", pinIndex, hr);
          return false;
        }
        finally
        {
          if (inputPin != null)
          {
            Release.ComObject(inputPin);
          }
        }
      }
      return true;
    }
  }
}