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
    [ComImport, Guid("511d13f0-8a56-42fa-b151-b72a325cf71a")]
    private class MpTsMuxer
    {
    }

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
    /// The hardware or software video (or combined video-audio) encoder filter.
    /// </summary>
    private IBaseFilter _filterVideoEncoder = null;

    /// <summary>
    /// The hardware or software audio encoder filter.
    /// </summary>
    private IBaseFilter _filterAudioEncoder = null;

    /// <summary>
    /// The hardware multiplexer/encoder filter.
    /// </summary>
    private IBaseFilter _filterMultiplexer = null;

    /// <summary>
    /// The MediaPortal transport stream multiplexer filter.
    /// </summary>
    private IBaseFilter _filterTsMultiplexer = null;

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

    #region Dispose

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

    #endregion

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
    /// Creates the encoder component.
    /// This function handles a huge variety of filter and pin connection
    /// arrangements, including:
    /// - 1 or 2 capture filters
    /// - 1 or 2 encoder filters
    /// - hardware multiplexer filter
    /// - hardware or software encoders
    /// - combined or separate video and/or audio paths throughout
    /// The objective is to connect a combination of required (ie. hardware
    /// filters) and optional filters, such that we can eventually connect our
    /// transport stream multiplexer.
    /// </summary>
    /// <param name="graph">The graph builder</param>
    /// <param name="crossbar">The crossbar component</param>
    /// <param name="capture">The capture component</param>
    /// <returns>true, if the building was successful; false otherwise</returns>
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

      // Connect hardware encoder filter(s).
      Log.Log.Debug("encoder: add hardware encoder(s)");
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
      Log.Log.Debug("encoder: add hardware multiplexer");
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

      // ------------------------------------------------
      // STAGE 2
      // Add software encoders if necessary.
      // ------------------------------------------------
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
      IList<AMMediaType> captureMediaTypes = new List<AMMediaType>();
      captureMediaTypes.Add(mt1);
      captureMediaTypes.Add(mt2);
      captureMediaTypes.Add(mt3);
      IList<AMMediaType> videoMediaTypes = new List<AMMediaType>();
      videoMediaTypes.Add(mt4);
      IList<AMMediaType> audioMediaTypes = new List<AMMediaType>();
      audioMediaTypes.Add(mt5);
      Guid g1;
      Guid g2;

      IPin capturePin = null;
      Guid captureMediaMajorType = Guid.Empty;
      Guid captureMediaSubType = Guid.Empty;
      IPin videoPin = null;
      IPin audioPin = null;
      try
      {
        if (_filterMultiplexer == null && _filterVideoEncoder == null && _filterAudioEncoder == null)
        {
          // We have no hardware filters. If the capture filter doesn't have an
          // MPEG PS or TS output (ie. capture filter is not an encoder filter)
          // then we need software encoders. There are a couple of special cases
          // too.
          if (capture.VideoFilter == null || capture.AudioFilter == null)
          {
            IBaseFilter captureFilter = capture.VideoFilter ?? capture.AudioFilter;
            FindPinByMediaType(captureFilter, captureMediaTypes, out capturePin, out captureMediaMajorType, out captureMediaSubType);
          }
          if (capturePin == null)
          {
            // Software encoder(s) required.
            Log.Log.Debug("encoder: add software encoder(s)");
            if (capture.VideoFilter != null)
            {
              if (!FindPinByMediaType(capture.VideoFilter, videoMediaTypes, out videoPin, out g1, out g2))
              {
                throw new TvExceptionGraphBuildingFailed("Failed to locate capture or video pin on video capture filter.");
              }
            }
            IBaseFilter audioCaptureFilter = capture.AudioFilter ?? capture.VideoFilter;
            if (audioCaptureFilter != null)
            {
              FindPinByMediaType(audioCaptureFilter, audioMediaTypes, out audioPin, out g1, out g2);
            }

            // The Plextor capture filter is a video encoder filter, but not an
            // audio encoder filter.
            bool connectedAudio = false;
            if (videoPin != null && !capture.VideoName.StartsWith("Plextor ConvertX"))
            {
              if (!AddAndConnectSoftwareVideoEncoder(graph, videoPin, audioPin, out _filterVideoEncoder, out _videoCompressorDevice, out connectedAudio))
              {
                throw new TvExceptionSWEncoderMissing("Failed to add a software video encoder.");
              }
            }
            if (!connectedAudio && audioPin != null)
            {
              if (!AddAndConnectSoftwareAudioEncoder(graph, audioPin, out _filterAudioEncoder, out _audioCompressorDevice))
              {
                throw new TvExceptionSWEncoderMissing("Failed to add a software audio encoder.");
              }
            }
          }
        }

        // ------------------------------------------------
        // STAGE 3
        // Find pins to connect to the TS multiplexer.
        // ------------------------------------------------
        Log.Log.Debug("encoder: find pin(s) for TS multiplexer");
        if (_filterMultiplexer != null)
        {
          if (!FindPinByMediaType(_filterMultiplexer, captureMediaTypes, out capturePin, out captureMediaMajorType, out captureMediaSubType))
          {
            throw new TvExceptionGraphBuildingFailed("Failed to locate capture pin on multiplexer filter.");
          }
        }
        else if (_filterVideoEncoder != null || _filterAudioEncoder != null)
        {
          if (_filterVideoEncoder == null || _filterAudioEncoder == null)
          {
            IBaseFilter encoderFilter = _filterVideoEncoder ?? _filterAudioEncoder;
            FindPinByMediaType(encoderFilter, captureMediaTypes, out capturePin, out captureMediaMajorType, out captureMediaSubType);
          }

          if (capturePin == null)
          {
            if (_filterVideoEncoder != null)
            {
              if (!FindPinByMediaType(_filterVideoEncoder, videoMediaTypes, out videoPin, out g1, out g2))
              {
                throw new TvExceptionGraphBuildingFailed("Failed to locate capture or video pin on video encoder filter.");
              }
            }
            IBaseFilter audioEncoderFilter = _filterAudioEncoder ?? _filterVideoEncoder;
            if (audioEncoderFilter != null)
            {
              FindPinByMediaType(audioEncoderFilter, audioMediaTypes, out audioPin, out g1, out g2);
            }
          }
        }

        // ------------------------------------------------
        // STAGE 4
        // Add and connect the MediaPortal multiplexer.
        // ------------------------------------------------
        if (!AddAndConnectTsMultiplexer(graph, videoPin, audioPin, capturePin, captureMediaSubType))
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
      }
      return true;
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

    private bool FindPinByMediaType(IBaseFilter filter, IList<AMMediaType> matchMediaTypes, out IPin matchPin, out Guid matchMediaMajorType, out Guid matchMediaSubType)
    {
      matchPin = null;
      matchMediaMajorType = Guid.Empty;
      matchMediaSubType = Guid.Empty;

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
                      matchPin = pin;
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
            if (matchMediaSubType == Guid.Empty)
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

    #region s/w encoding card specific graph building

    private bool AddAndConnectSoftwareAudioEncoder(IFilterGraph2 graph, IPin audioPin, out IBaseFilter filter, out DsDevice device)
    {
      int hr = 0;
      filter = null;
      device = null;

      int installedEncoderCount = 0;
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(FilterCategory.AudioCompressorCategory);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      IList<SoftwareEncoder> encoders = new TvBusinessLayer().GetSofwareEncodersAudio();
      DsDevice[] devices = new DsDevice[encoders.Count];
      for (int x = 0; x < encoders.Count; ++x)
      {
        devices[x] = null;
      }
      for (int i = 0; i < devices1.Length; i++)
      {
        for (int x = 0; x < encoders.Count; ++x)
        {
          if (encoders[x].Name == devices1[i].Name)
          {
            devices[x] = devices1[i];
            installedEncoderCount++;
            break;
          }
        }
      }
      for (int i = 0; i < devices2.Length; i++)
      {
        for (int x = 0; x < encoders.Count; ++x)
        {
          if (encoders[x].Name == devices2[i].Name)
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
      Log.Log.Debug("encoder: add and connect software audio encoder, compressor count = {0}, legacy filter count = {1}, DB count = {2}, installed count = {3}", devices1.Length, devices2.Length, devices.Length, installedEncoderCount);

      for (int i = 0; i < devices.Length; i++)
      {
        DsDevice d = devices[i];
        if (d == null || !EncodersInUse.Instance.Add(d, encoders[i]))
        {
          continue;
        }

        try
        {
          Log.Log.Debug("encoder: attempt to add {0} {1}", d.Name, d.DevicePath);
          hr = graph.AddSourceFilterForMoniker(d.Mon, null, d.Name, out filter);
          DsError.ThrowExceptionForHR(hr);
        }
        catch (Exception)
        {
          Log.Log.Debug("encoder: failed to add, hr = 0x{0:x}", hr);
          EncodersInUse.Instance.Remove(d);
          filter = null;
          continue;
        }

        Log.Log.Debug("encoder: connect...");
        bool connected = false;
        IPin inputPin1 = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
        IPin inputPin2 = DsFindPin.ByDirection(filter, PinDirection.Input, 1);
        try
        {
          hr = graph.ConnectDirect(audioPin, inputPin1, null);
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
          if (inputPin1 != null)
          {
            Release.ComObject(inputPin1);
          }
          if (inputPin2 != null)
          {
            Release.ComObject(inputPin2);
          }
          if (!connected)
          {
            Log.Log.Debug("encoder: failed to connect");
            EncodersInUse.Instance.Remove(d);
            graph.RemoveFilter(filter);
            Release.ComObject(filter);
            filter = null;
          }
        }
      }
      return false;
    }

    private bool AddAndConnectSoftwareVideoEncoder(IFilterGraph2 graph, IPin videoPin, IPin audioPin, out IBaseFilter filter, out DsDevice device, out bool connectedAudio)
    {
      int hr = 0;
      filter = null;
      device = null;
      connectedAudio = false;

      int installedEncoderCount = 0;
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(FilterCategory.VideoCompressorCategory);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      IList<SoftwareEncoder> encoders = new TvBusinessLayer().GetSofwareEncodersVideo();
      DsDevice[] devices = new DsDevice[encoders.Count];
      for (int x = 0; x < encoders.Count; ++x)
      {
        devices[x] = null;
      }
      for (int i = 0; i < devices1.Length; i++)
      {
        for (int x = 0; x < encoders.Count; ++x)
        {
          if (encoders[x].Name == devices1[i].Name)
          {
            devices[x] = devices1[i];
            installedEncoderCount++;
            break;
          }
        }
      }
      for (int i = 0; i < devices2.Length; i++)
      {
        for (int x = 0; x < encoders.Count; ++x)
        {
          if (encoders[x].Name == devices2[i].Name)
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
      Log.Log.Debug("encoder: add and connect software video encoder, compressor count = {0}, legacy filter count = {1}, DB count = {2}, installed count = {3}", devices1.Length, devices2.Length, devices.Length, installedEncoderCount);

      for (int i = 0; i < devices.Length; i++)
      {
        DsDevice d = devices[i];
        if (d == null || !EncodersInUse.Instance.Add(d, encoders[i]))
        {
          continue;
        }

        try
        {
          Log.Log.Debug("encoder: attempt to add {0} {1}", d.Name, d.DevicePath);
          hr = graph.AddSourceFilterForMoniker(d.Mon, null, d.Name, out filter);
          DsError.ThrowExceptionForHR(hr);
        }
        catch (Exception)
        {
          Log.Log.Debug("encoder: failed to add, hr = 0x{0:x}", hr);
          EncodersInUse.Instance.Remove(d);
          filter = null;
          continue;
        }

        Log.Log.Debug("encoder: connect...");
        bool connected = false;
        IPin inputPin1 = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
        IPin inputPin2 = DsFindPin.ByDirection(filter, PinDirection.Input, 1);
        try
        {
          hr = graph.ConnectDirect(videoPin, inputPin1, null);
          DsError.ThrowExceptionForHR(hr);
          Log.Log.Debug("encoder: connected [video]!");

          // Unfortunately the encoders I've seen don't usually advertise the
          // supported media types on their input pins. We're forced to check
          // the pin name to confirm that the second pin is not a closed
          // caption pin.
          if (inputPin2 != null)
          {
            Log.Log.Debug("encoder: detected additional input pin");
            if (!GetPinName(inputPin2).ToLowerInvariant().Contains("cc"))
            {
              Log.Log.Debug("encoder: connect...");
              hr = graph.ConnectDirect(audioPin, inputPin2, null);
              if (hr == 0)
              {
                Log.Log.Debug("encoder: connected [audio]!");
                connectedAudio = true;
              }
            }
          }

          device = d;
          connected = true;
          return true;
        }
        catch
        {
        }
        finally
        {
          if (inputPin1 != null)
          {
            Release.ComObject(inputPin1);
          }
          if (inputPin2 != null)
          {
            Release.ComObject(inputPin2);
          }
          if (!connected)
          {
            Log.Log.Debug("encoder: failed to connect");
            EncodersInUse.Instance.Remove(d);
            graph.RemoveFilter(filter);
            Release.ComObject(filter);
            filter = null;
          }
        }
      }
      return false;
    }

    #endregion

    private bool AddAndConnectTsMultiplexer(IFilterGraph2 graph, IPin videoOutputPin, IPin audioOutputPin, IPin captureOutputPin, Guid captureMediaSubType)
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
      if (captureOutputPin != null)
      {
        try
        {
          if (captureMediaSubType == MediaSubType.Mpeg2Transport)
          {
            inputPin = DsFindPin.ByDirection(_filterTsMultiplexer, PinDirection.Input, 0);
          }
          else if (captureMediaSubType == MediaSubType.Mpeg2Program)
          {
            inputPin = DsFindPin.ByDirection(_filterTsMultiplexer, PinDirection.Input, 1);
          }
          else
          {
            Log.Log.Debug("encoder: unexpected capture pin media subtype {0}, not sure how to connect", captureMediaSubType);
            return false;
          }
          hr = graph.ConnectDirect(captureOutputPin, inputPin, null);
        }
        catch
        {
          Log.Log.Debug("encoder: failed to connect capture pin, hr = 0x{0:x}", hr);
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
      else
      {
        if (videoOutputPin != null)
        {
          try
          {
            inputPin = DsFindPin.ByDirection(_filterTsMultiplexer, PinDirection.Input, 2);
            hr = graph.ConnectDirect(videoOutputPin, inputPin, null);
            DsError.ThrowExceptionForHR(hr);
          }
          catch
          {
            Log.Log.Debug("encoder: failed to connect video pin, hr = 0x{0:x}", hr);
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
        if (audioOutputPin != null)
        {
          try
          {
            inputPin = DsFindPin.ByDirection(_filterTsMultiplexer, PinDirection.Input, 3);
            hr = graph.ConnectDirect(audioOutputPin, inputPin, null);
            DsError.ThrowExceptionForHR(hr);
          }
          catch
          {
            Log.Log.Debug("encoder: failed to connect audio pin, hr = 0x{0:x}", hr);
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
      }
      return true;
    }
  }
}