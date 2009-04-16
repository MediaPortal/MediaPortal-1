#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal - diehard2
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using TvLibrary.Implementations.Analog.GraphComponents;
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Implementations.Analog.Components
{
  /// <summary>
  /// The capture component of the graph
  /// </summary>
  internal class Capture : IDisposable
  {
    #region struct
#pragma warning disable 649,169,0649 // All fields are used by the Marshal.PtrToStructure function
    private struct MPEG2VideoInfo		//  MPEG2VideoInfo
    {
      internal VideoInfoHeader2 hdr;
      internal UInt32 dwStartTimeCode;
      internal UInt32 cbSequenceHeader;
      internal UInt32 dwProfile;
      internal UInt32 dwLevel;
      internal UInt32 dwFlags;
      internal UInt32 dwSequenceHeader;
    }
#pragma warning restore 649,169, 0649
    #endregion

    #region variables
    /// <summary>
    /// The video capture filter
    /// </summary>
    private IBaseFilter _filterVideoCapture;
    /// <summary>
    /// The audio capture filter
    /// </summary>
    private IBaseFilter _filterAudioCapture;
    /// <summary>
    /// The analog video decoder interface, needed for the VCR signal and video format
    /// </summary>
    private IAMAnalogVideoDecoder _analogVideoDecoder;
    /// <summary>
    /// A bitmask of available video formats
    /// </summary>
    private AnalogVideoStandard _videoFormats;
    /// <summary>
    /// The current video format
    /// </summary>
    private AnalogVideoStandard _currentVideoFormat;
    /// <summary>
    /// The video proc amp interface for the vide quality
    /// </summary>
    private IAMVideoProcAmp _videoProcAmp;
    /// <summary>
    /// A map of the default and current video proc amp
    /// </summary>
    private Dictionary<VideoProcAmpProperty, VideoQuality> _defaultVideoProcAmpValues;
    /// <summary>
    /// The stream config interface for setting the frame rate and frame size
    /// </summary>
    private IAMStreamConfig _streamConfig;
    /// <summary>
    /// The video capture device
    /// </summary>
    private DsDevice _videoCaptureDevice;
    /// <summary>
    /// The audio capture device
    /// </summary>
    private DsDevice _audioCaptureDevice;
    /// <summary>
    /// List of bad capture device
    /// </summary>
    private readonly List<String> _badCaptureDevices;
    /// <summary>
    /// The teletext output pin
    /// </summary>
    private IPin _pinVBI;
    /// <summary>
    /// The current image width
    /// </summary>
    private int _imageWidth = 720;
    /// <summary>
    /// The current image height
    /// </summary>
    private int _imageHeight = 576;
    /// <summary>
    /// The current frame rate
    /// </summary>
    private double _frameRate = 25.0;
    #endregion

    #region properties
    /// <summary>
    /// Gets the video capture name
    /// </summary>
    public String VideoCaptureName
    {
      get { return _videoCaptureDevice.Name; }
    }

    /// <summary>
    /// Gets the audio capture name
    /// </summary>
    public String AudioCaptureName
    {
      get { return _audioCaptureDevice.Name; }
    }

    /// <summary>
    /// Get the video capture filter
    /// </summary>
    public IBaseFilter VideoFilter
    {
      get { return _filterVideoCapture; }
    }

    /// <summary>
    /// Gt the audio capture filter
    /// </summary>
    public IBaseFilter AudioFilter
    {
      get { return _filterAudioCapture; }
    }

    /// <summary>
    /// Gets/Set the current video format
    /// </summary>
    public AnalogVideoStandard CurrentVideoFormat
    {
      get { return _currentVideoFormat; }
      set
      {
        _currentVideoFormat = value;
        if (_analogVideoDecoder != null)
        {
          _analogVideoDecoder.put_TVFormat(_currentVideoFormat);
        }
      }
    }

    /// <summary>
    /// Gets the teletext ping
    /// </summary>
    public IPin VBIPin
    {
      get { return _pinVBI; }
    }

    /// <summary>
    /// Gets if the capture device supports teletext
    /// </summary>
    public bool SupportsTeletext
    {
      get { return _pinVBI != null; }
    }
    #endregion 

    #region ctor
    /// <summary>
    /// Constructor, which set the list of bad capture devices
    /// </summary>
    public Capture()
    {
      _badCaptureDevices = new List<string>();
      //Don't use NVIDIA DualTV YUV Capture & NVIDIA DualTV YUV Capture 2 filters in the graph.
      _badCaptureDevices.Add("NVIDIA DualTV YUV Capture");
      _badCaptureDevices.Add("NVIDIA DualTV YUV Capture 2");
    }
    #endregion

    #region Dispose
    /// <summary>
    /// Disposes the capture component
    /// </summary>
    public void Dispose()
    {
      if (_pinVBI != null)
      {
        Release.ComObject("vbipin filter", _pinVBI);
        _pinVBI = null;
      }
      if (_filterAudioCapture != null && _filterVideoCapture != _filterAudioCapture)
      {
        Release.ComObject("audio capture filter", _filterAudioCapture);
        _filterAudioCapture = null;
      }
      if (_filterVideoCapture != null)
      {
        Release.ComObject("video capture filter", _filterVideoCapture);
        _filterVideoCapture = null;
      }
      if (_audioCaptureDevice != null && _audioCaptureDevice != _videoCaptureDevice)
      {
        DevicesInUse.Instance.Remove(_audioCaptureDevice);
        _audioCaptureDevice = null;
      }
      if (_videoCaptureDevice != null)
      {
        DevicesInUse.Instance.Remove(_videoCaptureDevice);
        _videoCaptureDevice = null;
      }
    }
    #endregion

    #region CreateFilterInstance method
    /// <summary>
    /// Adds the tv capture to the graph and connects it to the crossbar.
    /// At the end of this method the graph looks like:
    /// [          ] ------------------------->[           ]------>[               ]
    /// [ tvtuner  ]                           [ crossbar  ]       [ video capture ]
    /// [          ]----[            ]-------->[           ]------>[  filter       ]
    ///                 [ tvaudio    ]
    ///                 [   tuner    ]
    /// </summary>
    /// <param name="tvAudio">The tvaudio component</param>
    /// <param name="crossbar">The crossbar componen</param>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <param name="capBuilder">The Capture graph builder</param>
    /// <returns>true, if the graph building was successful</returns>
    public bool CreateFilterInstance(Graph graph, ICaptureGraphBuilder2 capBuilder, IFilterGraph2 graphBuilder, Tuner tuner, Crossbar crossbar, TvAudio tvAudio)
    {
      if (!string.IsNullOrEmpty(graph.Capture.Name))
      {
        Log.Log.WriteFile("analog: Using Capture configuration from stored graph");
        if (CreateConfigurationBasedFilterInstance(graph, capBuilder, graphBuilder, tuner, crossbar, tvAudio))
        {
          Log.Log.WriteFile("analog: Using Capture configuration from stored graph succeeded");
          return true;
        }
      }
      Log.Log.WriteFile("analog: No stored or invalid graph for Capture component - Trying to detect");
      return CreateAutomaticFilterInstance(graph, capBuilder, graphBuilder, tuner, crossbar, tvAudio);
    }
    #endregion

    #region private helper methods
    /// <summary>
    /// Creates the filter based on the configuration file
    /// </summary>
    /// </summary>
    /// <param name="tvAudio">The tvaudio component</param>
    /// <param name="crossbar">The crossbar componen</param>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <param name="capBuilder">The Capture graph builder</param>
    /// <returns>true, if the graph building was successful</returns>
    private bool CreateConfigurationBasedFilterInstance(Graph graph, ICaptureGraphBuilder2 capBuilder, IFilterGraph2 graphBuilder, Tuner tuner, Crossbar crossbar, TvAudio tvAudio)
    {
      string videoDeviceName = graph.Capture.AudioCaptureName;
      string audioDeviceName = graph.Capture.Name;
      DsDevice[] devices;
      bool videoConnected = false;
      bool audioConnected = false;
      //get a list of all video capture devices
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture); //shouldn't be VideoInputDevice
        devices = DeviceSorter.Sort(devices, tuner.TunerName, tvAudio.TvAudioName, crossbar.CrossBarName);
      } catch (Exception)
      {
        Log.Log.WriteFile("analog: AddTvCaptureFilter no tvcapture devices found");
        return false;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddTvCaptureFilter no tvcapture devices found");
        return false;
      }
      //try each video capture filter
      for (int i = 0; i < devices.Length; i++)
      {
        bool filterUsed = false;
        IBaseFilter tmp;
        if (_badCaptureDevices.Contains(devices[i].Name))
        {
          Log.Log.WriteFile("analog: AddTvCaptureFilter bypassing: {0}", devices[i].Name);
          continue;
        }
        Log.Log.WriteFile("analog: AddTvCaptureFilter try:{0} {1}", devices[i].Name, i);
        // if video capture filter is in use, then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
          continue;
        if (!videoDeviceName.Equals(devices[i].Name) && !audioDeviceName.Equals(devices[i].Name))
          continue;
        int hr;
        try
        {
          // add video capture filter to graph
          hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }
        if (hr != 0)
        {
          //cannot add video capture filter to graph, try next one
          if (tmp != null)
          {
            graphBuilder.RemoveFilter(tmp);
            Release.ComObject("TvCaptureFilter", tmp);
          }
          continue;
        }
        // connect crossbar->video capture filter
        if (videoDeviceName.Equals(devices[i].Name) && FilterGraphTools.ConnectPin(graphBuilder, crossbar.VideoOut, tmp, graph.Capture.VideoIn))
        {
          _filterVideoCapture = tmp;
          _videoCaptureDevice = devices[i];
          if (_audioCaptureDevice != _videoCaptureDevice)
          {
            DevicesInUse.Instance.Add(_videoCaptureDevice);
          }
          Log.Log.WriteFile("analog: AddTvCaptureFilter connected video to crossbar successfully");
          videoConnected = true;
          filterUsed = true;
        }
        // crossbar->audio capture filter
        // Many video capture are also the audio capture filter, so we can always try it again
        if (audioDeviceName.Equals(devices[i].Name) && FilterGraphTools.ConnectPin(graphBuilder, crossbar.AudioOut, tmp, graph.Capture.AudioIn))
        {
          _filterAudioCapture = tmp;
          _audioCaptureDevice = devices[i];
          if (_audioCaptureDevice != _videoCaptureDevice)
          {
            DevicesInUse.Instance.Add(_audioCaptureDevice);
          }
          Log.Log.WriteFile("analog: AddTvCaptureFilter connected audio to crossbar successfully");
          audioConnected = true;
          filterUsed = true;
        }

        if (!filterUsed)
        {
          // cannot connect crossbar->video capture filter, remove filter from graph
          // cand continue with the next vieo capture filter
          Log.Log.WriteFile("analog: AddTvCaptureFilter failed to connect to crossbar");
          graphBuilder.RemoveFilter(tmp);
          Release.ComObject("capture filter", tmp);
        }
        else
        {
          i = 0;
        }
        if (videoConnected && audioConnected)
        {
          break;
        }
      }
      if (_filterVideoCapture != null)
      {
        if (graph.Capture.TeletextPin != -1)
        {
          _pinVBI = DsFindPin.ByDirection(_filterVideoCapture, PinDirection.Output,
                                          graph.Capture.TeletextPin);
        }
        _videoProcAmp = _filterVideoCapture as IAMVideoProcAmp;
        _analogVideoDecoder = _filterVideoCapture as IAMAnalogVideoDecoder;
        _streamConfig = _filterVideoCapture as IAMStreamConfig;
        _videoFormats = graph.Capture.AvailableVideoStandard;
        _defaultVideoProcAmpValues = graph.Capture.VideoProcAmpValues;
        _frameRate = graph.Capture.FrameRate;
        _imageWidth = graph.Capture.ImageWidth;
        _imageHeight = graph.Capture.ImageHeight;
        CheckCapabilitiesStreamConfig(graph, capBuilder);
        SetCaptureConfiguration(graph);
      }
      return _filterVideoCapture != null;
    }

    /// <summary>
    /// Creates the filter by trying to detect it
    /// </summary>
    /// </summary>
    /// <param name="tvAudio">The tvaudio component</param>
    /// <param name="crossbar">The crossbar componen</param>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <param name="capBuilder">The Capture graph builder</param>
    /// <returns>true, if the graph building was successful</returns>
    private bool CreateAutomaticFilterInstance(Graph graph, ICaptureGraphBuilder2 capBuilder, IFilterGraph2 graphBuilder, Tuner tuner, Crossbar crossbar, TvAudio tvAudio)
    {
      DsDevice[] devices;
      bool videoConnected = false;
      bool audioConnected = false;
      //get a list of all video capture devices
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture); //shouldn't be VideoInputDevice
        devices = DeviceSorter.Sort(devices, tuner.TunerName, tvAudio.TvAudioName, crossbar.CrossBarName);
      } catch (Exception)
      {
        Log.Log.WriteFile("analog: AddTvCaptureFilter no tvcapture devices found");
        return false;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddTvCaptureFilter no tvcapture devices found");
        return false;
      }
      //try each video capture filter
      for (int i = 0; i < devices.Length; i++)
      {
        bool filterUsed = false;
        IBaseFilter tmp;
        if (_badCaptureDevices.Contains(devices[i].Name))
        {
          Log.Log.WriteFile("analog: AddTvCaptureFilter bypassing: {0}", devices[i].Name);
          continue;
        }
        Log.Log.WriteFile("analog: AddTvCaptureFilter try:{0} {1}", devices[i].Name, i);
        // if video capture filter is in use, then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
          continue;
        int hr;
        try
        {
          // add video capture filter to graph
          hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }
        if (hr != 0)
        {
          //cannot add video capture filter to graph, try next one
          if (tmp != null)
          {
            graphBuilder.RemoveFilter(tmp);
            Release.ComObject("TvCaptureFilter", tmp);
          }
          continue;
        }

        int destinationIndex;
        // connect crossbar->video capture filter
        if (!videoConnected && FilterGraphTools.ConnectFilter(graphBuilder, crossbar.VideoOut, tmp, out destinationIndex))
        {
          _filterVideoCapture = tmp;
          _videoCaptureDevice = devices[i];
          if (_audioCaptureDevice != _videoCaptureDevice)
          {
            DevicesInUse.Instance.Add(_videoCaptureDevice);
          }
          Log.Log.WriteFile("analog: AddTvCaptureFilter connected video to crossbar successfully");
          graph.Capture.Name = devices[i].Name;
          graph.Capture.VideoIn = destinationIndex;
          videoConnected = true;
          filterUsed = true;
        }
        // crossbar->audio capture filter
        // Many video capture are also the audio capture filter, so we can always try it again

        if (videoConnected && FilterGraphTools.ConnectFilter(graphBuilder, crossbar.AudioOut, tmp, out destinationIndex))
        {
          _filterAudioCapture = tmp;
          _audioCaptureDevice = devices[i];
          if (_audioCaptureDevice != _videoCaptureDevice)
          {
            DevicesInUse.Instance.Add(_audioCaptureDevice);
          }
          Log.Log.WriteFile("analog: AddTvCaptureFilter connected audio to crossbar successfully");
          graph.Capture.AudioCaptureName = devices[i].Name;
          graph.Capture.AudioIn = destinationIndex;
          audioConnected = true;
          filterUsed = true;
        }

        if (!filterUsed)
        {
          // cannot connect crossbar->video capture filter, remove filter from graph
          // cand continue with the next vieo capture filter
          Log.Log.WriteFile("analog: AddTvCaptureFilter failed to connect to crossbar");
          graphBuilder.RemoveFilter(tmp);
          Release.ComObject("capture filter", tmp);
        }
        else
        {
          i = 0;
        }
        if (videoConnected && audioConnected)
        {
          break;
        }
      }
      if (_filterVideoCapture != null)
      {
        FindVBIPin(graph);
        CheckCapabilities(graph, capBuilder);
      }
      return _filterVideoCapture != null;
    }

    /// <summary>
    /// Finds the VBI pin on the video capture device.
    /// If it existst the pin is stored in _pinVBI
    /// </summary>
    /// <param name="graph">The stored graph</param>
    private void FindVBIPin(Graph graph)
    {
      Log.Log.WriteFile("analog: FindVBIPin on VideoCapture");
      int pinIndex;
      try
      {
        IPin pinVBI = FilterGraphTools.GetPinByCategoryAndDirection(_filterVideoCapture, PinCategory.VideoPortVBI, 0, PinDirection.Output, out pinIndex);
        if (pinVBI != null)
        {
          Log.Log.WriteFile("analog: VideoPortVBI pin found");
          Marshal.ReleaseComObject(pinVBI);
          return;
        }
        pinVBI = FilterGraphTools.GetPinByCategoryAndDirection(_filterVideoCapture, PinCategory.VBI, 0, PinDirection.Output, out pinIndex);
        if (pinVBI != null)
        {
          Log.Log.WriteFile("analog: VBI pin found");
          graph.Capture.TeletextPin = pinIndex;
          _pinVBI = pinVBI;
          return;
        }
      } catch (COMException ex)
      {
        if (ex.ErrorCode.Equals(unchecked((Int32)0x80070490)))
        {
          // pin on a NVTV capture filter is named VBI..
          Log.Log.WriteFile("analog: getCategory not supported by collection ? ERROR:0x{0:x} :" + ex.Message, ex.ErrorCode);

          if (_filterVideoCapture == null)
            return;
          Log.Log.WriteFile("analog: find VBI pin by name");

          IPin pinVBI = FilterGraphTools.GetPinByName(_filterVideoCapture, "VBI", PinDirection.Output, out pinIndex);
          if (pinVBI != null)
          {
            Log.Log.WriteFile("analog: pin named VBI found");
            graph.Capture.TeletextPin = pinIndex;
            _pinVBI = pinVBI;
            return;
          }
        }
        Log.Log.WriteFile("analog: Error in searching vbi pin - Skipping error");
      }
      Log.Log.WriteFile("analog: FindVBIPin on VideoCapture no vbi pin found");
    }

    /// <summary>
    /// Checks the capabilities of the capture component
    /// </summary>
    /// <param name="graph">The stored graph</param>
    /// <param name="capBuilder">The capture graph builder</param>
    private void CheckCapabilities(Graph graph, ICaptureGraphBuilder2 capBuilder)
    {
      CheckCapabilitiesAnalogVideoDecoder(graph);
      CheckCapabilitiesVideoProcAmp(graph);
      CheckCapabilitiesStreamConfig(graph, capBuilder);
    }

    /// <summary>
    /// Checks the capabilites of a possible available analog video decoder interface
    /// </summary>
    /// <param name="graph">The stored graph</param>
    private void CheckCapabilitiesAnalogVideoDecoder(Graph graph)
    {
      _analogVideoDecoder = _filterVideoCapture as IAMAnalogVideoDecoder;
      if (_analogVideoDecoder != null)
      {
        _analogVideoDecoder.get_AvailableTVFormats(out _videoFormats);
        _analogVideoDecoder.get_TVFormat(out _currentVideoFormat);
        graph.Capture.CurrentVideoStandard = _currentVideoFormat;
        graph.Capture.AvailableVideoStandard = _videoFormats;
      }
      else
      {
        graph.Capture.AvailableVideoStandard = AnalogVideoStandard.None;
        graph.Capture.CurrentVideoStandard = AnalogVideoStandard.None;
      }
    }

    /// <summary>
    /// Checks the capabilites of a possbile available video proc amp interface
    /// </summary>
    /// <param name="graph">The stored graph</param>
    private void CheckCapabilitiesVideoProcAmp(Graph graph)
    {
      _videoProcAmp = _filterVideoCapture as IAMVideoProcAmp;
      _defaultVideoProcAmpValues = new Dictionary<VideoProcAmpProperty, VideoQuality>();
      if (_videoProcAmp != null)
      {
        int min, max, steppingDelta, defaultValue;
        VideoProcAmpFlags flags;
        VideoQuality tempValue;
        int hr = _videoProcAmp.GetRange(VideoProcAmpProperty.Brightness, out min, out max, out steppingDelta, out defaultValue,
                                        out flags);
        if (hr == 0)
        {
          tempValue = new VideoQuality(min, max, steppingDelta, defaultValue, flags == VideoProcAmpFlags.Manual, defaultValue);
          _defaultVideoProcAmpValues.Add(VideoProcAmpProperty.Brightness, tempValue);
        }
        hr = _videoProcAmp.GetRange(VideoProcAmpProperty.Contrast, out min, out max, out steppingDelta, out defaultValue,
                               out flags);
        if (hr == 0)
        {
          tempValue = new VideoQuality(min, max, steppingDelta, defaultValue, flags == VideoProcAmpFlags.Manual, defaultValue);
          _defaultVideoProcAmpValues.Add(VideoProcAmpProperty.Contrast, tempValue);
        }
        hr = _videoProcAmp.GetRange(VideoProcAmpProperty.Hue, out min, out max, out steppingDelta, out defaultValue,
                               out flags);
        if (hr == 0)
        {
          tempValue = new VideoQuality(min, max, steppingDelta, defaultValue, flags == VideoProcAmpFlags.Manual, defaultValue);
          _defaultVideoProcAmpValues.Add(VideoProcAmpProperty.Hue, tempValue);
        }
        hr = _videoProcAmp.GetRange(VideoProcAmpProperty.Saturation, out min, out max, out steppingDelta, out defaultValue,
                               out flags);
        if (hr == 0)
        {
          tempValue = new VideoQuality(min, max, steppingDelta, defaultValue, flags == VideoProcAmpFlags.Manual, defaultValue);
          _defaultVideoProcAmpValues.Add(VideoProcAmpProperty.Saturation, tempValue);
        }
        hr = _videoProcAmp.GetRange(VideoProcAmpProperty.Sharpness, out min, out max, out steppingDelta, out defaultValue,
                               out flags);
        if (hr == 0)
        {
          tempValue = new VideoQuality(min, max, steppingDelta, defaultValue, flags == VideoProcAmpFlags.Manual, defaultValue);
          _defaultVideoProcAmpValues.Add(VideoProcAmpProperty.Sharpness, tempValue);
        }
        hr = _videoProcAmp.GetRange(VideoProcAmpProperty.Gamma, out min, out max, out steppingDelta, out defaultValue,
                               out flags);
        if (hr == 0)
        {
          tempValue = new VideoQuality(min, max, steppingDelta, defaultValue, flags == VideoProcAmpFlags.Manual, defaultValue);
          _defaultVideoProcAmpValues.Add(VideoProcAmpProperty.Gamma, tempValue);
        }
        hr = _videoProcAmp.GetRange(VideoProcAmpProperty.ColorEnable, out min, out max, out steppingDelta, out defaultValue,
                               out flags);
        if (hr == 0)
        {
          tempValue = new VideoQuality(min, max, steppingDelta, defaultValue, flags == VideoProcAmpFlags.Manual, defaultValue);
          _defaultVideoProcAmpValues.Add(VideoProcAmpProperty.ColorEnable, tempValue);
        }
        hr = _videoProcAmp.GetRange(VideoProcAmpProperty.WhiteBalance, out min, out max, out steppingDelta, out defaultValue,
                               out flags);
        if (hr == 0)
        {
          tempValue = new VideoQuality(min, max, steppingDelta, defaultValue, flags == VideoProcAmpFlags.Manual, defaultValue);
          _defaultVideoProcAmpValues.Add(VideoProcAmpProperty.WhiteBalance, tempValue);
        }
        hr = _videoProcAmp.GetRange(VideoProcAmpProperty.BacklightCompensation, out min, out max, out steppingDelta, out defaultValue,
                               out flags);
        if (hr == 0)
        {
          tempValue = new VideoQuality(min, max, steppingDelta, defaultValue, flags == VideoProcAmpFlags.Manual, defaultValue);
          _defaultVideoProcAmpValues.Add(VideoProcAmpProperty.BacklightCompensation, tempValue);
        }
        hr = _videoProcAmp.GetRange(VideoProcAmpProperty.Gain, out min, out max, out steppingDelta, out defaultValue,
                               out flags);
        if (hr == 0)
        {
          tempValue = new VideoQuality(min, max, steppingDelta, defaultValue, flags == VideoProcAmpFlags.Manual, defaultValue);
          _defaultVideoProcAmpValues.Add(VideoProcAmpProperty.Gain, tempValue);
        }
        graph.Capture.VideoProcAmpValues = _defaultVideoProcAmpValues;
      }
    }

    /// <summary>
    /// Checks the capabilites of a possibe available stream config interface
    /// </summary>
    /// <param name="graph">The stored graph</param>
    /// <param name="capBuilder">The capture graph builder</param>
    private void CheckCapabilitiesStreamConfig(Graph graph, ICaptureGraphBuilder2 capBuilder)
    {
      DsGuid cat = new DsGuid(PinCategory.Capture);
      Guid iid = typeof(IAMStreamConfig).GUID;
      object o;
      int hr = capBuilder.FindInterface(cat, null, _filterVideoCapture, iid, out o);
      if (hr == 0)
      {
        _streamConfig = o as IAMStreamConfig;
        if (_streamConfig == null)
        {
          _imageWidth = -1;
          _frameRate = -1;
        }
      }
      else
      {
        _imageWidth = -1;
        _frameRate = -1;
      }
      graph.Capture.ImageWidth = _imageWidth;
      graph.Capture.ImageHeight = _imageHeight;
      graph.Capture.FrameRate = _frameRate;
    }

    /// <summary>
    /// Sets the new video format on the video decoder interface
    /// </summary>
    /// <param name="newVideoFormat">The new video deocder format</param>
    private void SetVideoDecoder(AnalogVideoStandard newVideoFormat)
    {
      if (_analogVideoDecoder != null && (newVideoFormat != AnalogVideoStandard.None))
      {
        int hr = _analogVideoDecoder.put_TVFormat(newVideoFormat);
        if (hr == 0)
        {
          _currentVideoFormat = newVideoFormat;
          Log.Log.Info("Set new video format to: {0}", _currentVideoFormat);
        }
      }
    }

    /// <summary>
    /// Sets the new video proc amp configuration
    /// </summary>
    /// <param name="map">A map with the new video quality settings</param>
    private void SetVideoProcAmpValues(IDictionary<VideoProcAmpProperty, VideoQuality> map)
    {
      if (_videoProcAmp != null && map != null)
      {
        foreach (VideoProcAmpProperty prop in map.Keys)
        {
          VideoQuality quality = map[prop];
          _videoProcAmp.Set(prop, quality.Value, quality.IsManual ? VideoProcAmpFlags.Manual : VideoProcAmpFlags.Auto);
          Log.Log.Info("Set VideoProcAmp - {0} to value: {1}", prop, quality.Value);
        }
      }
    }

    /// <summary>
    /// Sets the new stream config configuration
    /// </summary>
    /// <param name="imageWidth">The new image width</param>
    /// <param name="imageHeight">The new image height</param>
    /// <param name="frameRate">The new frame rate</param>
    private void SetStreamConfigSetting(int imageWidth, int imageHeight, double frameRate)
    {
      if (_streamConfig != null)
      {
        if(SetFrameRate((long)(10000000d / frameRate)))
        {
          Log.Log.Info("Set Framerate to {0} succeeded", frameRate);
          _frameRate = frameRate;
        }
        BitmapInfoHeader bmiHeader = GetFrameSize();
        if(bmiHeader!=null)
        {
          bmiHeader.Width = imageWidth;
          bmiHeader.Height = imageHeight;
          if(SetFrameSize(bmiHeader))
          {
            Log.Log.Info("Set Framesize to {0}x{1} succeeded", imageWidth,imageHeight);
            _imageWidth = imageWidth;
            _imageHeight = imageHeight;
          }
        }
      }
    }

    /// <summary>
    /// Sets the frame rate
    /// </summary>
    /// <param name="frameRate">The frame rate to set</param>
    /// <returns>true, if it was successful; false otherwise</returns>
    private bool SetFrameRate(long frameRate)
    {
      try
      {
        IntPtr pmt = IntPtr.Zero;
        AMMediaType mediaType;
        try
        {
          // Get the current format info
          int hr = _streamConfig.GetFormat(out mediaType);
          if (hr != 0)
          {
            Log.Log.Info("SetFrameRate: Failed to get the video format - {0}", hr);
            return false;
          }
          // The formatPtr member points to different structures
          // dependingon the formatType
          object formatStruct;
          if (mediaType.formatType == FormatType.VideoInfo)
          {
            VideoInfoHeader temp = new VideoInfoHeader();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            temp.AvgTimePerFrame = frameRate;
            formatStruct = temp;
          }
          else if (mediaType.formatType == FormatType.VideoInfo2)
          {
            VideoInfoHeader2 temp = new VideoInfoHeader2();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            temp.AvgTimePerFrame = frameRate;
            formatStruct = temp;
          }
          else if (mediaType.formatType == FormatType.Mpeg2Video)
          {
            MPEG2VideoInfo temp = new MPEG2VideoInfo();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            temp.hdr.AvgTimePerFrame = frameRate;
            formatStruct = temp;
          }
          else if (mediaType.formatType == FormatType.MpegVideo)
          {
            MPEG1VideoInfo temp = new MPEG1VideoInfo();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            temp.hdr.AvgTimePerFrame = frameRate;
            formatStruct = temp;
          }
          else if (mediaType.formatType == FormatType.None)
          {
            Log.Log.Info("SetFrameRate: FAILED no format returned");
            return false;
          }
          else
          {
            Log.Log.Info("SetFrameRate:  FAILED unknown fmt");
            return false;
          }
          // PtrToStructure copies the data so we need to copy it back
          Marshal.StructureToPtr(formatStruct, mediaType.formatPtr, false);
          // Save the changes
          hr = _streamConfig.SetFormat(mediaType);
          if (hr != 0)
          {
            Log.Log.Info("SetFrameRate:  FAILED to set:{0}",hr);
            return false;
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(pmt);
        }
        return true;
      } catch (Exception)
      {
        Log.Log.Info("SetFrameRate:  FAILED ");
      }
      return false;
    }

    /// <summary>
    /// Sets the frame size
    /// </summary>
    /// <param name="bmiHeader">The bitmap info header with the frame size</param>
    /// <returns>true, if it was successful; false otherwise</returns>
    private bool SetFrameSize(BitmapInfoHeader bmiHeader)
    {
      try
      {
        IntPtr pmt = IntPtr.Zero;
        AMMediaType mediaType;
        try
        {
          // Get the current format info
          int hr = _streamConfig.GetFormat(out mediaType);
          if (hr != 0)
          {
            Log.Log.Info("SetFrameSize: Failed to get the video format - {0}", hr);
            return false;
          }
          // The formatPtr member points to different structures
          // dependingon the formatType
          object formatStruct;
          if (mediaType.formatType == FormatType.VideoInfo)
          {
            VideoInfoHeader temp = new VideoInfoHeader();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            temp.BmiHeader = bmiHeader;
            formatStruct = temp;
          }
          else if (mediaType.formatType == FormatType.VideoInfo2)
          {
            VideoInfoHeader2 temp = new VideoInfoHeader2();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            temp.BmiHeader = bmiHeader;
            formatStruct = temp;
          }
          else if (mediaType.formatType == FormatType.Mpeg2Video)
          {
            MPEG2VideoInfo temp = new MPEG2VideoInfo();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            temp.hdr.BmiHeader = bmiHeader;
            formatStruct = temp;
          }
          else if (mediaType.formatType == FormatType.MpegVideo)
          {
            MPEG1VideoInfo temp = new MPEG1VideoInfo();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            temp.hdr.BmiHeader = bmiHeader;
            formatStruct = temp;
          }
          else if (mediaType.formatType == FormatType.None)
          {
            Log.Log.Info("SetFrameSize: FAILED no format returned");
            return false;
          }
          else
          {
            Log.Log.Info("SetFrameSize:  FAILED unknown fmt");
            return false;
          }
          // PtrToStructure copies the data so we need to copy it back
          Marshal.StructureToPtr(formatStruct, mediaType.formatPtr, false);
          // Save the changes
          hr = _streamConfig.SetFormat(mediaType);
          if (hr != 0)
          {
            Log.Log.Info("SetFrameSize:  FAILED to set:{0}", hr);
            return false;
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(pmt);
        }
        return true;
      } catch (Exception)
      {
        Log.Log.Info("SetFrameSize:  FAILED ");
      }
      return false;
    }

    /// <summary>
    /// Gets the current frame size
    /// </summary>
    /// <returns>The frame size in a bitmap info header</returns>
    private BitmapInfoHeader GetFrameSize()
    {
      BitmapInfoHeader bmiHeader = null;
      try
      {
        IntPtr pmt = IntPtr.Zero;
        AMMediaType mediaType = new AMMediaType();
        try
        {
          // Get the current format info
          mediaType.formatType = FormatType.VideoInfo2;
          int hr = _streamConfig.GetFormat(out mediaType);
          if (hr != 0)
          {
            Log.Log.Info("GetFrameSize: FAILED to get format - {0}",hr);
            Marshal.ThrowExceptionForHR(hr);
            return bmiHeader;
          }
          // The formatPtr member points to different structures
          // dependingon the formatType
          if (mediaType.formatType == FormatType.VideoInfo)
          {
            VideoInfoHeader temp = new VideoInfoHeader();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            bmiHeader = temp.BmiHeader;
          }
          else if (mediaType.formatType == FormatType.VideoInfo2)
          {
            VideoInfoHeader2 temp = new VideoInfoHeader2();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            bmiHeader = temp.BmiHeader;
          }
          else if (mediaType.formatType == FormatType.Mpeg2Video)
          {
            MPEG2VideoInfo temp = new MPEG2VideoInfo();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            bmiHeader = temp.hdr.BmiHeader;
          }
          else if (mediaType.formatType == FormatType.MpegVideo)
          {
            MPEG1VideoInfo temp = new MPEG1VideoInfo();
            Marshal.PtrToStructure(mediaType.formatPtr, temp);
            bmiHeader = temp.hdr.BmiHeader;
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(pmt);
        }
      } catch (Exception)
      {
        Log.Log.Info("  VideoCaptureDevice.getStreamConfigSetting() FAILED ");
      }
      return bmiHeader;
    }
    #endregion

    #region public methods
    /// <summary>
    /// Sets the new capture component configurations
    /// </summary>
    /// <param name="graph">The stored grpah with the configuration</param>
    public void SetCaptureConfiguration(Graph graph)
    {
      SetVideoDecoder(graph.Capture.CurrentVideoStandard);
      SetVideoProcAmpValues(graph.Capture.VideoProcAmpValues);
      SetStreamConfigSetting(graph.Capture.ImageWidth, graph.Capture.ImageHeight, graph.Capture.FrameRate);
    }

    /// <summary>
    /// Performs a tuning to the given channel
    /// </summary>
    /// <param name="analogChannel">The channel to tune to</param>
    public void PerformTune(AnalogChannel analogChannel)
    {
      if (_analogVideoDecoder != null && analogChannel.IsTv)
      {
        _analogVideoDecoder.put_VCRHorizontalLocking(analogChannel.IsVCRSignal);
      }

    }
    #endregion
  }
}
