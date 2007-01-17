/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Text;
using DirectShowLib;
using TvLibrary.Implementations;
using DirectShowLib.SBE;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Teletext;
using TvLibrary.Epg;
using TvLibrary.Implementations.DVB;
using TvLibrary.Helper;

namespace TvLibrary.Implementations.Analog
{
  /// <summary>
  /// base class for analog tv cards
  /// </summary>
  public class TvCardAnalogBase : ISampleGrabberCB
  {

    #region constants

    //KSCATEGORY_ENCODER
    public static readonly Guid AMKSEncoder = new Guid("19689BF6-C384-48fd-AD51-90E58C79F70B");
    //STATIC_KSCATEGORY_MULTIPLEXER
    public static readonly Guid AMKSMultiplexer = new Guid("7A5DE1D3-01A1-452c-B481-4FA2B96271E8");
    public static readonly Guid AudioCompressorCategory = new Guid(0x33d9a761, 0x90c8, 0x11d0, 0xbd, 0x43, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);
    public static readonly Guid VideoCompressorCategory = new Guid(0x33d9a760, 0x90c8, 0x11d0, 0xbd, 0x43, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);
    public static readonly Guid LegacyAmFilterCategory = new Guid(0x083863F1, 0x70DE, 0x11d0, 0xBD, 0x40, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);
    #endregion

    #region imports
    [DllImport("advapi32", CharSet = CharSet.Auto)]
    protected static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);
    [ComImport, Guid("DB35F5ED-26B2-4A2A-92D3-852E145BF32D")]
    protected class MpFileWriter { }
    #endregion

    #region enums
    /// <summary>
    /// Different states of the card
    /// </summary>
    protected enum GraphState
    {
      /// <summary>
      /// Card is idle
      /// </summary>
      Idle,
      /// <summary>
      /// Card is idle, but graph is created
      /// </summary>
      Created,
      /// <summary>
      /// Card is timeshifting
      /// </summary>
      TimeShifting,
      /// <summary>
      /// Card is recording
      /// </summary>
      Recording
    }
    #endregion

    #region variables
    protected string _name;
    protected DsDevice _tunerDevice;
    protected DsDevice _audioDevice;
    protected DsDevice _crossBarDevice;
    protected DsDevice _captureDevice;
    protected DsDevice _videoEncoderDevice;
    protected DsDevice _audioEncoderDevice;
    protected DsDevice _multiplexerDevice;
    protected GraphState _graphState;
    protected IFilterGraph2 _graphBuilder = null;
    protected DsROTEntry _rotEntry = null;
    protected ICaptureGraphBuilder2 _capBuilder;
    protected IBaseFilter _filterTvTuner = null;
    protected IBaseFilter _filterTvAudioTuner = null;
    protected IBaseFilter _filterCrossBar = null;
    protected IBaseFilter _filterCapture = null;
    protected IBaseFilter _filterVideoEncoder = null;
    protected IBaseFilter _filterAudioEncoder = null;
    protected IBaseFilter _filterMultiplexer = null;
    protected IBaseFilter _filterMpeg2Demux = null;
    protected IBaseFilter _filterGrabber = null;
    protected IBaseFilter _filterWstDecoder = null;
    protected IBaseFilter _teeSink = null;
    protected IBaseFilter _tsFileSink = null;
    protected IBaseFilter _filterMpegMuxer;
    protected IBaseFilter _filterAnalogMpegMuxer = null;
    protected IBaseFilter _filterAudioCompressor = null;
    protected IBaseFilter _filterVideoCompressor = null;
    //protected IBaseFilter _filterDump1;
    //protected IBaseFilter _infTee;
    protected IPin _pinCapture = null;
    protected IPin _pinVideo = null;
    protected IPin _pinAudio = null;
    protected IPin _pinLPCM = null;
    protected IPin _pinVBI = null;
    protected IPin _pinAnalogAudio = null;
    protected IPin _pinAnalogVideo = null;
    protected DVBTeletext _teletextDecoder;
    protected string _recordingFileName;
    protected bool _grabTeletext = false;
    protected int _managedThreadId;
    protected DateTime _lastSignalUpdate;
    protected bool _tunerLocked;
    protected bool _isScanning = false;
    protected DateTime _dateTimeShiftStarted = DateTime.MinValue;
    protected DateTime _dateRecordingStarted = DateTime.MinValue;
    protected object m_context = null;
    protected IChannel _currentChannel;
    string _timeshiftFileName;
    protected IVbiCallback _teletextCallback = null;
    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardAnalogBase"/> class.
    /// </summary>
    public TvCardAnalogBase()
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardAnalogBase"/> class.
    /// </summary>
    /// <param name="device">The device.</param>
    public TvCardAnalogBase(DsDevice device)
    {
      _tunerDevice = device;
      _name = device.Name;
      _graphState = GraphState.Idle;
      _teletextDecoder = new DVBTeletext();
    }
    #endregion

    /// <summary>
    /// Checks the thread id.
    /// </summary>
    /// <returns></returns>
    protected bool CheckThreadId()
    {
      return true;
      if (_managedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
      {

        Log.Log.WriteFile("analog:Invalid thread id!!!");
        return false;
      }
      return true;
    }
    #region graph building

    /// <summary>
    /// Builds the directshow graph for this analog tvcard
    /// </summary>
    protected void BuildGraph()
    {
      _lastSignalUpdate = DateTime.MinValue;
      _tunerLocked = false;
      Log.Log.WriteFile("analog: build graph");
      try
      {
        if (_graphState != GraphState.Idle)
        {
          Log.Log.WriteFile("analog: Graph already build");
          throw new TvException("Graph already build");
        }
        _managedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

        ///create a new filter graph
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _rotEntry = new DsROTEntry(_graphBuilder);
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);

        //add the wdm tv tuner device
        AddTvTunerFilter();
        if (_filterTvTuner == null)
        {
          Log.Log.Error("analog: unable to add tv tuner filter");
          throw new TvException("Analog: unable to add tv tuner filter");
        }
        //add the wdm crossbar device and connect tvtuner->crossbar
        AddCrossBarFilter();
        if (_filterCrossBar == null)
        {
          Log.Log.Error("analog: unable to add tv crossbar filter");
          throw new TvException("Analog: unable to add tv crossbar filter");
        }

        //add the tv audio tuner device and connect it to the crossbar
        AddTvAudioFilter();
        if (_filterTvAudioTuner == null)
        {
          Log.Log.Error("analog: unable to add tv audio tuner filter");
          throw new TvException("Analog: unable to add tv audio tuner filter");
        }

        //add the tv capture device and connect it to the crossbar
        AddTvCaptureFilter();

        // now things get difficult.
        // Here we can have the following situations:
        // 1. we're done, the video capture filter has a mpeg-2 audio output pin
        // 2. we need to add 1 encoder filter which converts both the audio/video output pins
        //    of the video capture filter to mpeg-2 
        // 3. we need to add 2 encoder filters. One for audio and one for video 
        //    after the 2 encoder filters, a multiplexer will be added which takes the output of both
        //    encoders and generates mpeg-2

        //situation 1. we look if the video capture device has an mpeg-2 output pin (media type:stream)
        FindCapturePin(MediaType.Stream, MediaSubType.Null);
        if (_pinCapture == null)
        {
          // no it does not. So we have situation 2 or 3 and first need to add 1 or more encoder filters
          // First we try only to add encoders where the encoder pin names are the same as the
          // output pins of the capture filters
          if (!AddTvEncoderFilter(true))
          {
            //if that fails, we try any encoder filter
            AddTvEncoderFilter(false);
          }

          // 1 or 2 encoder filters have been added. 
          // check if the encoder filters supply a mpeg-2 output pin
          FindCapturePin(MediaType.Stream, MediaSubType.Null);

          // not as a stream, but perhaps its supplied with another media type
          if (_pinCapture == null)
            FindCapturePin(MediaType.Video, MediaSubType.Mpeg2Program);

          if (_pinCapture == null)
          {
            //still no mpeg output found, we move on to situation 3. We need to add a multiplexer
            // First we try only to add multiplexers where the multiplexer pin names are the same as the
            // output pins of the encoder filters
            if (!AddTvMultiPlexer(true))
            {
              //if that fails, we try any multiplexer filter
              AddTvMultiPlexer(false);
            }
          }
        }

        // multiplexer filter now has been added.
        // check if the encoder multiplexer supply a mpeg-2 output pin
        if (_pinCapture == null)
        {
          FindCapturePin(MediaType.Stream, MediaSubType.Null);
          if (_pinCapture == null)
            FindCapturePin(MediaType.Video, MediaSubType.Mpeg2Program);
        }

        if (_pinCapture == null)
        {
          // Still no mpeg-2 output pin found
          // looks like this is a s/w encoding card
          if (!FindAudioVideoPins())
          {
            Log.Log.WriteFile("analog:   failed to find audio/video pins");
            throw new Exception("No analog audio/video pins found");
          }
          if (!AddAudioCompressor())
          {
            Log.Log.WriteFile("analog:   failed to add audio compressor");
            throw new Exception("No audio compressor filter found");
          }
          if (!AddVideoCompressor())
          {
            Log.Log.WriteFile("analog:   failed to add video compressor");
            throw new Exception("No video compressor filter found");
          }
          if (!AddAnalogMuxer())
          {
            Log.Log.WriteFile("analog:   failed to add analog muxer");
            throw new Exception("No analog muxer filter found");
          }
        }

        //find the vbi output pin 
        FindVBIPin();
        if (_pinVBI != null)
        {
          //and if it exists setup the teletext grabber
          SetupTeletext();
        }

        //add the mpeg-2 demultiplexer filter
        AddMpeg2Demultiplexer();

        SetupCaptureFormat();
        //FilterGraphTools.SaveGraphFile(_graphBuilder, "hp.grf");
        Log.Log.WriteFile("analog: Graph is build");
        _graphState = GraphState.Created;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _graphState = GraphState.Idle;
        throw ex;
      }
    }

    #region tuner, crossbar and capture device graph building
    /// <summary>
    /// Adds the tv tuner device to the graph
    /// </summary>
    void AddTvTunerFilter()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog: AddTvTunerFilter {0}", _tunerDevice.Name);
      if (DevicesInUse.Instance.IsUsed(_tunerDevice)) return;
      IBaseFilter tmp;
      int hr;
      try
      {
        hr = _graphBuilder.AddSourceFilterForMoniker(_tunerDevice.Mon, null, _tunerDevice.Name, out tmp);
      }
      catch (Exception)
      {
        Log.Log.WriteFile("analog: cannot add filter to graph");
        return;
      }
      if (hr != 0)
      {
        Log.Log.Error("analog: AddTvTunerFilter failed:0x{0:X}", hr);
        throw new TvException("Unable to add tvtuner to graph");
      }
      _filterTvTuner = tmp;
      DevicesInUse.Instance.Add(_tunerDevice);
    }

    /// <summary>
    /// Adds the tv audio tuner to the graph and connects it to the crossbar.
    /// At the end of this method the graph looks like:
    /// [          ] ------------------------->[           ]
    /// [ tvtuner  ]                           [ crossbar  ]
    /// [          ]----[            ]-------->[           ]
    ///                 [ tvaudio    ]
    ///                 [   tuner    ]
    /// </summary>
    void AddTvAudioFilter()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog: AddTvAudioFilter");
      //find crossbar audio tuner input
      IPin pinIn = FindCrossBarPin(_filterCrossBar, PhysicalConnectorType.Audio_Tuner, PinDirection.Input);
      if (pinIn == null)
      {
        Log.Log.WriteFile("analog: AddTvAudioFilter audio tuner input pin on crossbar not found");
        return;
      }

      //get all tv audio tuner devices on this system
      DsDevice[] devices = null;
      IBaseFilter tmp;
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSTVAudio);
        devices = DeviceSorter.Sort(devices, _tunerDevice, _audioDevice, _crossBarDevice, _captureDevice, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
      }
      catch (Exception)
      {
        Log.Log.WriteFile("analog: AddTvAudioFilter no tv audio devices found");
        Release.ComObject("crossbar audio tuner pinin", pinIn);
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddTvAudioFilter no tv audio devices found");
        Release.ComObject("crossbar audio tuner pinin", pinIn);
        return;
      }
      // try each tv audio tuner
      for (int i = 0; i < devices.Length; i++)
      {
        Log.Log.WriteFile("analog: AddTvAudioFilter try:{0}", devices[i].Name);
        //if tv audio tuner is currently in use we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
        int hr;
        try
        {
          //add tv audio tuner to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }

        if (hr != 0)
        {
          //failed to add tv audio tuner to graph, continue with the next one
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("tvAudioFilter filter", tmp);
          }
          continue;
        }
        // try connecting the tv tuner-> tv audio tuner
        if (FilterGraphTools.ConnectFilter(_graphBuilder, _filterTvTuner, tmp))
        {
          // Got it !
          // Connect tv audio tuner to the crossbar
          IPin pin = DsFindPin.ByDirection(tmp, PinDirection.Output, 0);
          hr = _graphBuilder.Connect(pin, pinIn);
          if (hr < 0)
          {
            //failed
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("audiotuner pinin", pin);
            Release.ComObject("audiotuner filter", tmp);
          }
          else
          {
            //succeeded. we're done
            Log.Log.WriteFile("analog: AddTvAudioFilter succeeded:{0}", devices[i].Name);
            Release.ComObject("audiotuner pinin", pin);
            _filterTvAudioTuner = tmp;
            _audioDevice = devices[i];
            DevicesInUse.Instance.Add(_audioDevice);
            break;
          }
        }
        else
        {
          // cannot connect tv tuner-> tv audio tuner, try next one...
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("audiotuner filter", tmp);
        }
      }

      if (pinIn != null)
      {
        Release.ComObject("crossbar audiotuner pin", pinIn);
      }

      if (_filterTvAudioTuner == null)
      {
        Log.Log.Error("analog: unable to add TvAudioTuner to graph");
        throw new TvException("Unable to add TvAudioTuner to graph");
      }
    }

    /// <summary>
    /// Finds a specific pin on the crossbar filter.
    /// </summary>
    /// <param name="crossbarFilter">The crossbar filter.</param>
    /// <param name="connectorType">Type of the connector.</param>
    /// <param name="direction">The pin-direction.</param>
    /// <returns>IPin when the pin is found or null if pin is not found</returns>
    IPin FindCrossBarPin(IBaseFilter crossbarFilter, PhysicalConnectorType connectorType, PinDirection direction)
    {
      if (!CheckThreadId()) return null;
      Log.Log.WriteFile("analog: FindCrossBarPin type:{0} direction:{1}", connectorType, direction);
      IAMCrossbar crossbar = crossbarFilter as IAMCrossbar;
      int inputs = 0;
      int outputs = 0;
      crossbar.get_PinCounts(out outputs, out inputs);
      Log.Log.WriteFile("analog: FindCrossBarPin inputs:{0} outputs:{1}", inputs, outputs);
      int maxPins = inputs;
      if (direction == PinDirection.Output) maxPins = outputs;
      for (int i = 0; i < maxPins; ++i)
      {
        int relatedPinIndex;
        PhysicalConnectorType physicalType;
        crossbar.get_CrossbarPinInfo((direction == PinDirection.Input), i, out relatedPinIndex, out physicalType);
        Log.Log.WriteFile("analog: pin {0} type:{1} ", i, physicalType);
        if (physicalType == connectorType)
        {
          IPin pin = DsFindPin.ByDirection(crossbarFilter, direction, i);
          Log.Log.WriteFile("analog: FindCrossBarPin found pin at index:{0}", i);

          return pin;
        }
      }
      Log.Log.WriteFile("analog: FindCrossBarPin pin not found");
      return null;
    }

    /// <summary>
    /// Adds the cross bar filter to the graph and connects the tv tuner to the crossbar.
    /// at the end of this method the graph looks like:
    /// [tv tuner]----->[crossbar]
    /// </summary>
    void AddCrossBarFilter()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog: AddCrossBarFilter");
      DsDevice[] devices = null;
      IBaseFilter tmp;
      //get list of all crossbar devices installed on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
        devices = DeviceSorter.Sort(devices, _tunerDevice, _audioDevice, _crossBarDevice, _captureDevice, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
      }
      catch (Exception)
      {
        Log.Log.WriteFile("analog: AddCrossBarFilter no crossbar devices found");
        return;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddCrossBarFilter no crossbar devices found");
        return;
      }
      //try each crossbar
      for (int i = 0; i < devices.Length; i++)
      {
        Log.Log.WriteFile("analog: AddCrossBarFilter try:{0}", devices[i].Name);
        //if crossbar is already in use then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
        int hr;
        try
        {
          //add the crossbar to the graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }

        if (hr != 0)
        {
          //failed. try next crossbar
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("CrossBarFilter", tmp);
          }
          continue;
        }

        //find video tuner input pin of the crossbar
        IPin pinIn = FindCrossBarPin(tmp, PhysicalConnectorType.Video_Tuner, PinDirection.Input);
        if (pinIn == null)
        {
          // no pin found, continue with next crossbar
          Log.Log.WriteFile("analog: AddCrossBarFilter no video tuner input pin detected");
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("CrossBarFilter", tmp);
          }
          continue;
        }

        //connect tv tuner->crossbar
        if (FilterGraphTools.ConnectFilter(_graphBuilder, _filterTvTuner, pinIn))
        {
          // Got it, we're done
          _filterCrossBar = tmp;
          _crossBarDevice = devices[i];
          DevicesInUse.Instance.Add(_crossBarDevice);
          Release.ComObject("crossbar videotuner pin", pinIn);
          Log.Log.WriteFile("analog: AddCrossBarFilter succeeded");
          break;
        }
        else
        {
          // cannot connect tv tuner to crossbar, try next crossbar device
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("crossbar videotuner pin", pinIn);
          Release.ComObject("crossbar filter", tmp);
        }
      }
      if (_filterCrossBar == null)
      {
        Log.Log.Error("analog: unable to add crossbar to graph");
        throw new TvException("Unable to add crossbar to graph");
      }
    }

    /// <summary>
    /// Adds the tv capture to the graph and connects it to the crossbar.
    /// At the end of this method the graph looks like:
    /// [          ] ------------------------->[           ]------>[               ]
    /// [ tvtuner  ]                           [ crossbar  ]       [ video capture ]
    /// [          ]----[            ]-------->[           ]------>[  filter       ]
    ///                 [ tvaudio    ]
    ///                 [   tuner    ]
    /// </summary>
    void AddTvCaptureFilter()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog: AddTvCaptureFilter");
      DsDevice[] devices = null;
      IBaseFilter tmp;
      //get a list of all video capture devices
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        devices = DeviceSorter.Sort(devices, _tunerDevice, _audioDevice, _crossBarDevice, _captureDevice, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
      }
      catch (Exception)
      {
        Log.Log.WriteFile("analog: AddTvCaptureFilter no tvcapture devices found");
        return;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddTvCaptureFilter no tvcapture devices found");
        return;
      }
      //try each video capture filter
      for (int i = 0; i < devices.Length; i++)
      {
        Log.Log.WriteFile("analog: AddTvCaptureFilter try:{0}", devices[i].Name);
        // if video capture filter is in use, then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
        int hr;
        try
        {
          // add video capture filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }

        if (hr != 0)
        {
          //cannot add video capture filter to graph, try next one
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("TvCaptureFilter", tmp);
          }
          continue;
        }

        // connect crossbar->video capture filter
        hr = _capBuilder.RenderStream(null, null, _filterCrossBar, null, tmp);
        if (hr == 0)
        {
          // That worked. Since most crossbar devices require 2 connections from
          // crossbar->video capture filter, we do it again to connect the 2nd pin
          hr = _capBuilder.RenderStream(null, null, _filterCrossBar, null, tmp);
          _filterCapture = tmp;
          _captureDevice = devices[i];
          DevicesInUse.Instance.Add(_captureDevice);
          Log.Log.WriteFile("analog: AddTvCaptureFilter succeeded:0x{0:X}", hr);

          //and we're done
          break;
        }
        else
        {
          // cannot connect crossbar->video capture filter, remove filter from graph
          // cand continue with the next vieo capture filter
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("capture filter", tmp);
        }
      }
      if (_filterCapture == null)
      {
        Log.Log.Error("analog: unable to add TvCaptureFilter to graph");
        throw new TvException("Unable to add TvCaptureFilter to graph");
      }
    }

    /// <summary>
    /// Setups the cross bar.
    /// </summary>
    /// <param name="mode">The crossbar mode.</param>
    protected void SetupCrossBar(AnalogChannel.VideoInputType mode)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog: SetupCrossBar:{0}", mode);
      int outputs, inputs;
      IAMCrossbar crossbar = (IAMCrossbar)_filterCrossBar;
      crossbar.get_PinCounts(out outputs, out inputs);

      int audioOutIndex = 0, videoOutIndex = 0;
      for (int i = 0; i < outputs; ++i)
      {
        int relatedPinIndex;
        PhysicalConnectorType connectorType;
        crossbar.get_CrossbarPinInfo(false, i, out relatedPinIndex, out connectorType);
        if (connectorType == PhysicalConnectorType.Video_VideoDecoder)
          videoOutIndex = i;
        if (connectorType == PhysicalConnectorType.Audio_AudioDecoder)
          audioOutIndex = i;
      }

      int audioLine = 0;
      int videoCvbsNr = 0;
      int videoSvhsNr = 0;
      int videoRgbNr = 0;
      for (int i = 0; i < inputs; ++i)
      {
        int relatedPinIndex;
        PhysicalConnectorType connectorType;
        crossbar.get_CrossbarPinInfo(true, i, out relatedPinIndex, out connectorType);
        Log.Log.Write(" crossbar pin:{0} type:{1}", i, connectorType);
        if (connectorType == PhysicalConnectorType.Audio_Line)
        {
          audioLine++;
        }
        if (connectorType == PhysicalConnectorType.Video_Composite)
        {
          videoCvbsNr++;
        }
        if (connectorType == PhysicalConnectorType.Video_SVideo)
        {
          videoSvhsNr++;
        }
        if (connectorType == PhysicalConnectorType.Video_RGB)
        {
          videoRgbNr++;
        }

        int hr;
        switch (mode)
        {
          case AnalogChannel.VideoInputType.Tuner:
            if (connectorType == PhysicalConnectorType.Audio_Tuner)
            {
              hr = crossbar.Route(audioOutIndex, i);
              Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
            }
            if (connectorType == PhysicalConnectorType.Video_Tuner)
            {
              hr = hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            break;

          case AnalogChannel.VideoInputType.VideoInput1:
            if (connectorType == PhysicalConnectorType.Video_Composite && videoCvbsNr == 1)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 1)
            {
              hr = crossbar.Route(audioOutIndex, i);
              Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
            }
            break;
          case AnalogChannel.VideoInputType.VideoInput2:
            if (connectorType == PhysicalConnectorType.Video_Composite && videoCvbsNr == 2)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 2)
            {
              hr = crossbar.Route(audioOutIndex, i);
              Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
            }
            break;
          case AnalogChannel.VideoInputType.VideoInput3:
            if (connectorType == PhysicalConnectorType.Video_Composite && videoCvbsNr == 3)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 3)
            {
              hr = crossbar.Route(audioOutIndex, i);
              Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
            }
            break;

          case AnalogChannel.VideoInputType.SvhsInput1:
            if (connectorType == PhysicalConnectorType.Video_SVideo && videoSvhsNr == 1)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 1)
            {
              hr = crossbar.Route(audioOutIndex, i);
              Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
            }
            break;
          case AnalogChannel.VideoInputType.SvhsInput2:
            if (connectorType == PhysicalConnectorType.Video_SVideo && videoSvhsNr == 2)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 2)
            {
              hr = crossbar.Route(audioOutIndex, i);
              Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
            }
            break;
          case AnalogChannel.VideoInputType.SvhsInput3:
            if (connectorType == PhysicalConnectorType.Video_SVideo && videoSvhsNr == 3)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 3)
            {
              hr = crossbar.Route(audioOutIndex, i);
              Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
            }
            break;

          case AnalogChannel.VideoInputType.RgbInput1:
            if (connectorType == PhysicalConnectorType.Video_RGB && videoRgbNr == 1)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 1)
            {
              hr = crossbar.Route(audioOutIndex, i);
              Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
            }
            break;
          case AnalogChannel.VideoInputType.RgbInput2:
            if (connectorType == PhysicalConnectorType.Video_RGB && videoRgbNr == 2)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 2)
            {
              hr = crossbar.Route(audioOutIndex, i);
              Log.Log.Write("  route input:{0} -> audio output result:{0:X}", connectorType, hr);
            }
            break;
          case AnalogChannel.VideoInputType.RgbInput3:
            if (connectorType == PhysicalConnectorType.Video_RGB && videoRgbNr == 3)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 3)
            {
              hr = crossbar.Route(audioOutIndex, i);
              Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
            }
            break;
        }
      }
    }

    /// <summary>
    /// Find a pin on the multiplexer, video encoder or capture filter
    /// which can supplies the mediatype and mediasubtype specified
    /// if found the pin is stored in _pinCapture
    /// When a multiplexer is present then this method will try to find the capture pin on the multiplexer filter
    /// If no multiplexer is present then this method will try to find the capture pin on the video encoder filter
    /// If no video encoder is present then this method will try to find the capture pin on the video capture filter
    /// </summary>
    /// <param name="mediaType">Type of the media.</param>
    /// <param name="mediaSubtype">The media subtype.</param>
    void FindCapturePin(Guid mediaType, Guid mediaSubtype)
    {
      if (!CheckThreadId()) return;
      IEnumPins enumPins;
      // is there a multiplexer
      if (_filterMultiplexer != null)
      {
        //yes then we try to find the capture pin on the multiplexer 
        Log.Log.WriteFile("analog: FindCapturePin on multiplexer filter");
        _filterMultiplexer.EnumPins(out enumPins);
      }
      else if (_filterVideoEncoder != null)
      {
        // no multiplexer available, but a video encoder filter exists
        // try to find the capture pin on the video encoder 
        Log.Log.WriteFile("analog: FindCapturePin on encoder filter");
        _filterVideoEncoder.EnumPins(out enumPins);
      }
      else
      {
        // no multiplexer available, and no video encoder filter exists
        // try to find the capture pin on the video capture filter 
        Log.Log.WriteFile("analog: FindCapturePin on capture filter");
        _filterCapture.EnumPins(out enumPins);
      }

      // loop through all pins
      while (true)
      {
        IPin[] pins = new IPin[2];
        int fetched;
        enumPins.Next(1, pins, out fetched);
        if (fetched != 1) break;

        //first check if the pindirection matches
        PinDirection pinDirection;
        pins[0].QueryDirection(out pinDirection);
        if (pinDirection != PinDirection.Output) continue;

        //next check if the pin supports the media type requested
        IEnumMediaTypes enumMedia;
        int fetchedMedia;
        AMMediaType[] media = new AMMediaType[2];
        pins[0].EnumMediaTypes(out enumMedia);
        while (true)
        {
          enumMedia.Next(1, media, out fetchedMedia);
          if (fetchedMedia != 1) break;
          if (media[0].majorType == mediaType)
          {
            if (media[0].subType == mediaSubtype || mediaSubtype == MediaSubType.Null)
            {
              //it does... we're done
              _pinCapture = pins[0];
              Log.Log.WriteFile("analog: FindCapturePin pin:{0}", FilterGraphTools.LogPinInfo(pins[0]));
              Log.Log.WriteFile("analog: FindCapturePin   major:{0} sub:{1}", media[0].majorType, media[0].subType);
              Log.Log.WriteFile("analog: FindCapturePin succeeded");
              DsUtils.FreeAMMediaType(media[0]);
              return;
            }
          }
          DsUtils.FreeAMMediaType(media[0]);
        }
        Release.ComObject("capture pin", pins[0]);
      }
    }

    void SetupCaptureFormat()
    {
      return;
      /*
      IVideoEncoder encoder = null;
      if (_filterMultiplexer != null)
        encoder = _filterMultiplexer as IVideoEncoder;

      if (encoder == null && _filterVideoEncoder != null)
        encoder = _filterVideoEncoder as IVideoEncoder;

      if (encoder == null && _filterCapture != null)
        encoder = _filterCapture as IVideoEncoder;
      if (encoder == null) return;
      int hr = encoder.IsSupported(PropSetID.ENCAPIPARAM_BitRate);
      hr = encoder.IsSupported(PropSetID.ENCAPIPARAM_BitRateMode);
      hr = encoder.IsSupported(PropSetID.ENCAPIPARAM_PeakBitRate);
      */

    }
    #endregion

    #region encoder and multiplexer graph building
    /// <summary>
    /// This method tries to connect a encoder filter to the capture filter
    /// See the remarks in AddTvEncoderFilter() for the possible options
    /// </summary>
    /// <param name="filterEncoder">The filter encoder.</param>
    /// <param name="isVideo">if set to <c>true</c> the filterEncoder is used for video.</param>
    /// <param name="isAudio">if set to <c>true</c> the filterEncoder is used for audio.</param>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the encoder filter should match the pin names of the capture filter.</param>
    /// <returns>
    /// true if encoder is connected correctly, otherwise false
    /// </returns>
    bool ConnectEncoderFilter(IBaseFilter filterEncoder, bool isVideo, bool isAudio, bool matchPinNames)
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("analog: ConnectEncoderFilter video:{0} audio:{1}", isVideo, isAudio);
      int hr;
      //find the inputs of the encoder. could be 1 or 2 inputs.
      IPin pinInput1 = DsFindPin.ByDirection(filterEncoder, PinDirection.Input, 0);
      IPin pinInput2 = DsFindPin.ByDirection(filterEncoder, PinDirection.Input, 1);
      string pinName1 = "";
      string pinName2 = "";
      //log input pins
      if (pinInput1 != null)
      {

        Log.Log.WriteFile("analog:  found pin#0 {0}", FilterGraphTools.LogPinInfo(pinInput1));
        PinInfo info;
        pinInput1.QueryPinInfo(out info);
        pinName1 = info.name;
        if (info.filter != null)
        {
          Release.ComObject("encoder pin0", info.filter);
        }
      }
      if (pinInput2 != null)
      {
        Log.Log.WriteFile("analog:  found pin#1 {0}", FilterGraphTools.LogPinInfo(pinInput2));
        PinInfo info;
        pinInput2.QueryPinInfo(out info);
        pinName2 = info.name;
        if (info.filter != null)
        {
          Release.ComObject("encoder pin1", info.filter);
        }
      }

      int pinsConnected = 0;
      int pinsAvailable = 0;
      IPin[] pins = new IPin[20];
      IEnumPins enumPins = null;
      try
      {
        // for each output pin of the capture device
        _filterCapture.EnumPins(out enumPins);
        enumPins.Next(20, pins, out pinsAvailable);
        Log.Log.WriteFile("analog:  pinsAvailable on capture filter:{0}", pinsAvailable);
        for (int i = 0; i < pinsAvailable; ++i)
        {
          // check if this is an output pin
          PinDirection pinDir;
          pins[i].QueryDirection(out pinDir);
          if (pinDir == PinDirection.Input) continue;

          //log the pin info...
          Log.Log.WriteFile("analog:  capture pin:{0} {1}", i, FilterGraphTools.LogPinInfo(pins[i]));
          PinInfo info;
          pins[i].QueryPinInfo(out info);
          string pinName = info.name;
          if (info.filter != null)
          {
            Release.ComObject("encoder pin" + i.ToString(), info.filter);
          }

          // first lets try to connect this output pin of the capture filter to the 1st input pin
          // of the encoder
          // only try to connect when pin name matching is turned off
          // or when the pin names are the same
          if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
          {
            //try to connect the output pin of the capture filter to the first input pin of the encoder
            hr = _graphBuilder.Connect(pins[i], pinInput1);
            if (hr == 0)
            {
              //succeeded!
              Log.Log.WriteFile("analog:  connected pin:{0} {1} with pin0", i, pinName);
              pinsConnected++;
            }

            //check if all pins are connected
            if (pinsConnected == 1 && (isAudio == false || isVideo == false))
            {
              //yes, then we are done
              Log.Log.WriteFile("analog: ConnectEncoderFilter succeeded");
              return true;
            }
          }

          // next lets try to connect this output pin of the capture filter to the 2nd input pin
          // of the encoder
          // only try to connect when pin name matching is turned off
          // or when the pin names are the same
          if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
          {
            //try to connect the output pin of the capture filter to the 2nd input pin of the encoder
            hr = _graphBuilder.Connect(pins[i], pinInput2);
            if (hr == 0)
            {
              //succeeded!
              Log.Log.WriteFile("analog:  connected pin:{0} {1} with pin1", i, pinName);
              pinsConnected++;
            }
            //check if all pins are connected
            if (pinsConnected == 2)
            {
              //yes, then we are done
              Log.Log.WriteFile("analog: ConnectEncoderFilter succeeded");
              return true;
            }
          }
        }
      }
      finally
      {
        if (enumPins != null) Release.ComObject("ienumpins", enumPins);
        if (pinInput1 != null) Release.ComObject("encoder pin0", pinInput1);
        if (pinInput2 != null) Release.ComObject("encoder pin1", pinInput2);
        for (int i = 0; i < pinsAvailable; ++i)
        {
          if (pins[i] != null) Release.ComObject("capture pin" + i.ToString(), pins[i]);
        }
      }
      Log.Log.Error("analog: ConnectEncoderFilter failed");
      return false;
    }

    /// <summary>
    /// This method tries to connect a multiplexer filter to the encoder filters (or capture filter)
    /// See the remarks in AddTvMultiPlexer() for the possible options
    /// </summary>
    /// <param name="filterMultiPlexer">The multiplexer.</param>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the multiplexer filter should match the pin names of the encoder filter.</param>
    /// <returns>true if multiplexer is connected correctly, otherwise false</returns>
    bool ConnectMultiplexer(IBaseFilter filterMultiPlexer, bool matchPinNames)
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("analog: ConnectMultiplexer()");
      int hr;
      // get the input pins of the multiplexer filter (can be 1 or 2 input pins)
      IPin pinInput1 = DsFindPin.ByDirection(filterMultiPlexer, PinDirection.Input, 0);
      IPin pinInput2 = DsFindPin.ByDirection(filterMultiPlexer, PinDirection.Input, 1);
      string pinName1 = "";
      string pinName2 = "";
      //log the info for each input pin
      if (pinInput1 != null)
      {
        PinInfo info;
        pinInput1.QueryPinInfo(out info);
        pinName1 = info.name;
        if (info.filter != null)
        {
          Release.ComObject("encoder pin0", info.filter);
        }
        Log.Log.WriteFile("analog:  found pin#0 {0}", FilterGraphTools.LogPinInfo(pinInput1));
      }
      if (pinInput2 != null)
      {
        PinInfo info;
        pinInput2.QueryPinInfo(out info);
        pinName2 = info.name;
        if (info.filter != null)
        {
          Release.ComObject("encoder pin0", info.filter);
        }
        Log.Log.WriteFile("analog:  found pin#1 {0}", FilterGraphTools.LogPinInfo(pinInput2));
      }

      try
      {
        // if we have no encoder filters, the multiplexer should be connected directly to the capture filter
        if (_filterAudioEncoder == null || _filterVideoEncoder == null)
        {
          Log.Log.WriteFile("analog: ConnectMultiplexer to capture filter");
          //option 1, connect the multiplexer to the capture filter
          int pinsConnected = 0;
          int pinsAvailable = 0;
          IPin[] pins = new IPin[20];
          IEnumPins enumPins = null;
          try
          {
            // for each output pin of the capture filter
            _filterCapture.EnumPins(out enumPins);
            enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  capture pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              // check if this is an outpin pin on the capture filter
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input) continue;
              Log.Log.WriteFile("analog:  capture pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));

              //log the pin info
              PinInfo info;
              pins[i].QueryPinInfo(out info);
              string pinName = info.name;
              if (info.filter != null)
              {
                Release.ComObject("capture pin" + i.ToString(), info.filter);
              }


              // try to connect this output pin of the capture filter to the 1st input pin
              // of the multiplexer
              // only try to connect when pin name matching is turned off
              // or when the pin names are the same
              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                //try to connect the output pin of the capture filter to the 1st input pin of the multiplexer
                hr = _graphBuilder.Connect(pins[i], pinInput1);
                if (hr == 0)
                {
                  //succeeded
                  Log.Log.WriteFile("analog:  connected pin:{0} to pin1", i);
                  pinsConnected++;
                }
              }

              // next try to connect this output pin of the capture filter to the 2nd input pin
              // of the multiplexer
              // only try to connect when pin name matching is turned off
              // or when the pin names are the same
              if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
              {
                // check if multiplexer has 2 input pins
                if (pinInput2 != null)
                {
                  //try to connect the output pin of the capture filter to the 2nd input pin of the multiplexer
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0)
                  {
                    //succeeded
                    Log.Log.WriteFile("analog:  connected pin:{0} to pin2", i);
                    pinsConnected++;
                  }
                }
              }
              if (pinsConnected == 2)
              {
                //if both pins are connected, we're done..
                Log.Log.WriteFile("analog: ConnectMultiplexer succeeded");
                return true;
              }
            }
          }
          finally
          {
            if (enumPins != null) Release.ComObject("ienumpins", enumPins);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              if (pins[i] != null) Release.ComObject("capture pin" + i.ToString(), pins[i]);
            }
          }
        }

        //if we only have a single video encoder
        if (_filterAudioEncoder == null && _filterVideoEncoder != null)
        {
          //option 1, connect the multiplexer to a single encoder filter
          Log.Log.WriteFile("analog: ConnectMultiplexer to encoder filter");
          int pinsConnected = 0;
          int pinsAvailable = 0;
          IPin[] pins = new IPin[20];
          IEnumPins enumPins = null;
          try
          {
            // for each output pin of the video encoder filter
            _filterVideoEncoder.EnumPins(out enumPins);
            enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  video encoder pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              // check if this is an outpin pin on the video encoder filter
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input) continue;
              Log.Log.WriteFile("analog:  videoencoder pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));

              //log the pin info
              PinInfo info;
              pins[i].QueryPinInfo(out info);
              string pinName = info.name;
              if (info.filter != null)
              {
                Release.ComObject("capture pin" + i.ToString(), info.filter);
              }

              // try to connect this output pin of the video encoder filter to the 1st input pin
              // of the multiplexer
              // only try to connect when pin name matching is turned off
              // or when the pin names are the same
              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                //try to connect the output pin of the video encoder filter to the 1st input pin of the multiplexer filter
                hr = _graphBuilder.Connect(pins[i], pinInput1);
                if (hr == 0)
                {
                  //succeeded
                  Log.Log.WriteFile("analog:  connected pin:{0} to pin1", i);
                  pinsConnected++;
                }
              }

              //if the multiplexer has 2 input pins
              if (pinInput2 != null)
              {
                // next try to connect this output pin of the video encoder to the 2nd input pin
                // of the multiplexer
                // only try to connect when pin name matching is turned off
                // or when the pin names are the same
                if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
                {
                  //try to connect the output pin of the video encoder filter to the 1st input pin of the multiplexer filter
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0) pinsConnected++;
                  if (pinsConnected == 2)
                  {
                    //succeeded and done...
                    Log.Log.WriteFile("analog: ConnectMultiplexer succeeded");
                    return true;
                  }
                }
              }
            }
          }
          finally
          {
            if (enumPins != null) Release.ComObject("ienumpins", enumPins);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              if (pins[i] != null) Release.ComObject("encoder pin" + i.ToString(), pins[i]);
            }
          }
        }
        //if we have a video encoder and an audio encoder filter
        if (_filterAudioEncoder != null || _filterVideoEncoder != null)
        {
          Log.Log.WriteFile("analog: ConnectMultiplexer to audio/video encoder filters");
          //option 3, connect the multiplexer to the audio/video encoder filters
          int pinsConnected = 0;
          int pinsAvailable = 0;
          IPin[] pins = new IPin[20];
          IEnumPins enumPins = null;
          try
          {
            // for each output pin of the video encoder filter
            _filterVideoEncoder.EnumPins(out enumPins);
            enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  videoencoder pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              // check if this is an outpin pin on the video encoder filter
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input) continue;
              Log.Log.WriteFile("analog:   pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));

              //log the pin info
              PinInfo info;
              pins[i].QueryPinInfo(out info);
              string pinName = info.name;
              if (info.filter != null)
              {
                Release.ComObject("capture pin" + i.ToString(), info.filter);
              }
              // try to connect this output pin of the video encoder filter to the 1st input pin
              // of the multiplexer
              // only try to connect when pin name matching is turned off
              // or when the pin names are the same
              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                //try to connect the output pin of the video encoder filter to the 1st input pin of the multiplexer filter
                hr = _graphBuilder.Connect(pins[i], pinInput1);
                if (hr == 0)
                {
                  //succeeded
                  Log.Log.WriteFile("analog:  connected pin:{0}", i);
                  pinsConnected++;
                }
              }
              //if multiplexer has 2 inputs..
              if (pinInput2 != null)
              {
                // next try to connect this output pin of the video encoder to the 2nd input pin
                // of the multiplexer
                // only try to connect when pin name matching is turned off
                // or when the pin names are the same
                if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
                {
                  //try to connect the output pin of the video encoder filter to the 2nd input pin of the multiplexer filter
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0)
                  {
                    //succeeded
                    Log.Log.WriteFile("analog:  connected pin:{0}", i);
                    pinsConnected++;
                  }
                }
              }
              if (pinsConnected == 1)
              {
                //we are done with the video encoder when there is 1 connection between video encoder filter and multiplexer
                //next, continue with the audio encoder...
                Log.Log.WriteFile("analog: ConnectMultiplexer part 1 succeeded");
                break;
              }
            }
            if (pinsConnected == 0) return false; // video encoder is not connected, so we fail

            // for each output pin of the audio encoder filter
            _filterAudioEncoder.EnumPins(out enumPins);
            enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  audioencoder pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              // check if this is an outpin pin on the audio encoder filter
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input) continue;
              Log.Log.WriteFile("analog: audioencoder  pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));

              //log the pin info
              PinInfo info;
              pins[i].QueryPinInfo(out info);
              string pinName = info.name;
              if (info.filter != null)
              {
                Release.ComObject("capture pin" + i.ToString(), info.filter);
              }

              // try to connect this output pin of the audio encoder filter to the 1st input pin
              // of the multiplexer
              // only try to connect when pin name matching is turned off
              // or when the pin names are the same
              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                //try to connect the output pin of the audio encoder filter to the 1st input pin of the multiplexer filter
                hr = _graphBuilder.Connect(pins[i], pinInput1);
                if (hr == 0)
                {
                  //succeeded
                  Log.Log.WriteFile("analog:  connected pin:{0}", i);
                  pinsConnected++;
                }
              }
              //if multiplexer has 2 input pins
              if (pinInput2 != null)
              {
                // next try to connect this output pin of the audio encoder to the 2nd input pin
                // of the multiplexer
                // only try to connect when pin name matching is turned off
                // or when the pin names are the same
                if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
                {
                  //try to connect the output pin of the audio encoder filter to the 2nd input pin of the multiplexer filter
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0)
                  {
                    //succeeded
                    Log.Log.WriteFile("analog:  connected pin:{0}", i);
                    pinsConnected++;
                  }
                }
              }

              //when both pins on the multiplexer are connected, we're done
              if (pinsConnected == 2)
              {
                Log.Log.WriteFile("analog:  part 2 succeeded");
                return true;
              }
            }
          }
          finally
          {
            if (enumPins != null) Release.ComObject("ienumpins", enumPins);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              if (pins[i] != null) Release.ComObject("audio encoder pin" + i.ToString(), pins[i]);
            }
          }
        }
      }
      finally
      {
        if (pinInput1 != null) Release.ComObject("multiplexer pin0", pinInput1);
        if (pinInput2 != null) Release.ComObject("multiplexer pin1", pinInput2);
      }
      Log.Log.Error("analog: ConnectMultiplexer failed");
      return false;
    }

    /// <summary>
    /// Adds the multiplexer filter to the graph.
    // several posibilities
    //  1. no tv multiplexer needed
    //  2. tv multiplexer filter which is connected to a single encoder filter
    //  3. tv multiplexer filter which is connected to two encoder filter (audio/video)
    //  4. tv multiplexer filter which is connected to the capture filter
    // at the end this method the graph looks like this:

    //  option 2: single encoder filter
    //    [                ]----->[                ]      [             ]
    //    [ capture filter ]      [ encoder filter ]----->[ multiplexer ]
    //    [                ]----->[                ]      [             ]
    //
    //
    //  option 3: dual encoder filters
    //    [                ]----->[   video        ]    
    //    [ capture filter ]      [ encoder filter ]------>[             ]
    //    [                ]      [                ]       [             ]
    //    [                ]                               [ multiplexer ]
    //    [                ]----->[   audio        ]------>[             ]
    //                            [ encoder filter ]      
    //                            [                ]
    //
    //  option 4: no encoder filter
    //    [                ]----->[             ]
    //    [ capture filter ]      [ multiplexer ]
    //    [                ]----->[             ]
    /// </summary>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the multiplexer filter should match the pin names of the encoder filter.</param>
    /// <returns>true if encoder filters are added, otherwise false</returns>
    bool AddTvMultiPlexer(bool matchPinNames)
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("analog: AddTvMultiPlexer");
      DsDevice[] devices = null;
      IBaseFilter tmp;
      //get a list of all multiplexers available on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(AMKSMultiplexer);
        devices = DeviceSorter.Sort(devices, _tunerDevice, _audioDevice, _crossBarDevice, _captureDevice, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
      }
      catch (Exception)
      {
        Log.Log.WriteFile("analog: AddTvMultiPlexer no multiplexer devices found");
        return false;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddTvMultiPlexer no multiplexer devices found");
        return false;
      }
      //for each multiplexer
      for (int i = 0; i < devices.Length; i++)
      {
        Log.Log.WriteFile("analog: AddTvMultiPlexer try:{0}", devices[i].Name);
        // if multiplexer is in use, we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
        int hr;
        try
        {
          //add multiplexer to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }
        if (hr != 0)
        {
          //failed to add it to graph, continue with the next multiplexer
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("multiplexer filter", tmp);
          }
          continue;
        }
        // try to connect the multiplexer to encoders/capture devices
        if (ConnectMultiplexer(tmp, matchPinNames))
        {
          // succeeded, we're done
          _filterMultiplexer = tmp;
          _multiplexerDevice = devices[i];
          DevicesInUse.Instance.Add(_multiplexerDevice);
          Log.Log.WriteFile("analog: AddTvMultiPlexer succeeded");
          break;
        }
        else
        {
          // unable to connect it, remove the filter and continue with the next one
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("multiplexer filter", tmp);
        }
      }
      if (_filterMultiplexer == null)
      {
        Log.Log.WriteFile("analog: no TvMultiPlexer found");

        return false;
      }
      return true;
    }

    /// <summary>
    /// Adds one or 2 encoder filters to the graph
    //  several posibilities
    //  1. no encoder filter needed
    //  2. single encoder filter with seperate audio/video inputs and 1 (mpeg-2) output
    //  3. single encoder filter with a mpeg2 program stream input (I2S)
    //  4. two encoder filters. one for audio and one for video
    //
    //  At the end of this method the graph looks like:
    //
    //  option 2: one encoder filter, with 2 inputs
    //    [                ]----->[                ]
    //    [ capture filter ]      [ encoder filter ]
    //    [                ]----->[                ]
    //
    //
    //  option 3: one encoder filter, with 1 input
    //    [                ]      [                ]
    //    [ capture filter ]----->[ encoder filter ]
    //    [                ]      [                ]
    //
    //
    //  option 4: 2 encoder filters one for audio and one for video
    //    [                ]----->[   video        ]
    //    [ capture filter ]      [ encoder filter ]
    //    [                ]      [                ]
    //    [                ]   
    //    [                ]----->[   audio        ]
    //                            [ encoder filter ]
    //                            [                ]
    //
    /// </summary>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the encoder filter should match the pin names of the capture filter.</param>
    /// <returns>true if encoder filters are added, otherwise false</returns>
    bool AddTvEncoderFilter(bool matchPinNames)
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("analog: AddTvEncoderFilter");
      bool finished = false;
      DsDevice[] devices = null;
      IBaseFilter tmp;
      // first get all encoder filters available on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(AMKSEncoder);
        devices = DeviceSorter.Sort(devices, _tunerDevice, _audioDevice, _crossBarDevice, _captureDevice, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
      }
      catch (Exception)
      {
        Log.Log.WriteFile("analog: AddTvEncoderFilter no encoder devices found");
        return false;
      }
      if (devices == null)
      {
        Log.Log.WriteFile("analog: AddTvEncoderFilter no encoder devices found");
        return false;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddTvEncoderFilter no encoder devices found");
        return false;
      }
      //for each encoder
      Log.Log.WriteFile("analog: AddTvEncoderFilter found:{0} encoders", devices.Length);
      for (int i = 0; i < devices.Length; i++)
      {
        //if encoder is in use, we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
        {
          Log.Log.WriteFile("analog:  skip :{0} (inuse)", devices[i].Name);
          continue;
        }
        Log.Log.WriteFile("analog:  try encoder:{0}", devices[i].Name);
        int hr;
        try
        {
          //add encoder filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter {0} to graph", devices[i].Name);
          continue;
        }

        if (hr != 0)
        {
          //failed to add filter to graph, continue with the next one
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("TvEncoderFilter", tmp);
            tmp = null;
          }
          continue;
        }
        if (tmp == null) continue;

        // Encoder has been added to the graph
        // Now some cards have 2 encoder types, one for mpeg-2 transport stream and one for
        // mpeg-2 program stream. We dont want the mpeg-2 transport stream !
        // So first we check the output pins...
        // and dont accept filters which have a mpeg-ts output pin..

        //get the output pin
        bool isTsFilter = false;
        IPin pinOut = DsFindPin.ByDirection(tmp, PinDirection.Output, 0);
        if (pinOut != null)
        {
          //check which media types it support
          IEnumMediaTypes enumMediaTypes;
          pinOut.EnumMediaTypes(out enumMediaTypes);
          if (enumMediaTypes != null)
          {
            int fetched = 0;
            AMMediaType[] mediaTypes = new AMMediaType[20];
            enumMediaTypes.Next(20, mediaTypes, out fetched);
            if (fetched > 0)
            {
              for (int media = 0; media < fetched; ++media)
              {
                //check if media is mpeg-2 transport
                if (mediaTypes[media].majorType == MediaType.Stream &&
                    mediaTypes[media].subType == MediaSubType.Mpeg2Transport)
                {
                  isTsFilter = true;
                }
                if (mediaTypes[media].majorType == MediaType.Stream &&
                    mediaTypes[media].subType == MediaSubType.Mpeg2Program)
                {
                  isTsFilter = false;
                  break;
                }
              }
            }
          }
          Release.ComObject("pinout", pinOut);
        }

        //if encoder has mpeg-2 ts output pin, then we skip it and continue with the next one
        if (isTsFilter)
        {
          Log.Log.WriteFile("analog:  filter {0} has mpeg-2 ts outputs", devices[i].Name);
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("TvEncoderFilter", tmp);
            tmp = null;
          }
          continue;
        }
        if (tmp == null) continue;

        // get the input pins of the encoder (can be 1 or 2 inputs)
        IPin pin1 = DsFindPin.ByDirection(tmp, PinDirection.Input, 0);
        IPin pin2 = DsFindPin.ByDirection(tmp, PinDirection.Input, 1);
        if (pin1 != null) Log.Log.WriteFile("analog: encoder in-pin1:{0}", FilterGraphTools.LogPinInfo(pin1));
        if (pin2 != null) Log.Log.WriteFile("analog: encoder in-pin2:{0}", FilterGraphTools.LogPinInfo(pin2));

        // if the encoder has 2 input pins then this means it has seperate inputs for audio and video
        if (pin1 != null && pin2 != null)
        {
          // try to connect the capture device -> encoder filters..
          if (ConnectEncoderFilter(tmp, true, true, matchPinNames))
          {
            //succeeded, encoder has been added and we are done
            _filterVideoEncoder = tmp;
            _videoEncoderDevice = devices[i];
            DevicesInUse.Instance.Add(_videoEncoderDevice);
            Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded (encoder with 2 inputs)");
            //            success = true;
            finished = true;
            tmp = null;
          }
        }
        else if (pin1 != null)
        {
          //encoder filter only has 1 input pin.
          //First we get the media type of this pin to determine if its audio of video
          IEnumMediaTypes enumMedia;
          AMMediaType[] media = new AMMediaType[20];
          int fetched;
          pin1.EnumMediaTypes(out enumMedia);
          enumMedia.Next(1, media, out fetched);
          if (fetched == 1)
          {
            //media type found
            Log.Log.WriteFile("analog: AddTvEncoderFilter encoder output major:{0} sub:{1}", media[0].majorType, media[0].subType);
            //is it audio?
            if (media[0].majorType == MediaType.Audio)
            {
              //yes, pin is audio
              //then connect the encoder to the audio output pin of the capture filter
              if (ConnectEncoderFilter(tmp, false, true, matchPinNames))
              {
                //this worked. but we're not done yet. We probably need to add a video encoder also
                _filterAudioEncoder = tmp;
                _audioEncoderDevice = devices[i];
                DevicesInUse.Instance.Add(_audioEncoderDevice);
                //                success = true;
                Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded (audio encoder)");

                // if video encoder was already added, then we're done.
                if (_filterVideoEncoder != null) finished = true;
                tmp = null;
              }
            }
            else
            {
              //pin is video
              //then connect the encoder to the video output pin of the capture filter
              if (ConnectEncoderFilter(tmp, true, false, matchPinNames))
              {
                //this worked. but we're not done yet. We probably need to add a audio encoder also
                _filterVideoEncoder = tmp;
                _videoEncoderDevice = devices[i];
                DevicesInUse.Instance.Add(_videoEncoderDevice);
                //                success = true;
                Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded (video encoder)");
                // if audio encoder was already added, then we're done.
                if (_filterAudioEncoder != null) finished = true;
                tmp = null; ;
              }
            }
            DsUtils.FreeAMMediaType(media[0]);
          }
          else
          {
            // filter does not report any media type (which is strange)
            // we must do something, so we treat it as a video input pin
            Log.Log.WriteFile("analog: AddTvEncoderFilter no media types for pin1"); //??
            if (ConnectEncoderFilter(tmp, true, false, matchPinNames))
            {
              _filterVideoEncoder = tmp;
              _videoEncoderDevice = devices[i];
              DevicesInUse.Instance.Add(_videoEncoderDevice);
              //              success = true;
              Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded");
              finished = true;
              tmp = null;
            }
          }
        }
        else
        {
          Log.Log.WriteFile("analog: AddTvEncoderFilter no pin1");
        }
        if (pin1 != null) Release.ComObject("encoder pin0", pin1);
        if (pin2 != null) Release.ComObject("encoder pin1", pin2);
        pin1 = null;
        pin2 = null;
        if (tmp != null)
        {
          _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("encoder filter", tmp);
          tmp = null;
        }
        if (finished) return true;
      }//for (int i = 0; i < devices.Length; i++)
      Log.Log.WriteFile("analog: AddTvEncoderFilter no encoder found");
      return false;
    }
    #endregion

    #region teletext graph building
    /// <summary>
    /// Finds the VBI pin on the video capture device.
    /// If it existst the pin is stored in _pinVBI
    /// </summary>
    void FindVBIPin()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog: FindVBIPin");
      IPin pinVBI = DsFindPin.ByCategory(_filterCapture, PinCategory.VideoPortVBI, 0);
      if (pinVBI != null)
      {
        Marshal.ReleaseComObject(pinVBI);
        return;
      }
      pinVBI = DsFindPin.ByCategory(_filterCapture, PinCategory.VBI, 0);
      if (pinVBI != null)
      {
        _pinVBI = pinVBI;
        return;
      }
      Log.Log.WriteFile("analog: FindVBIPin no vbi pin found");
    }
    /// <summary>
    /// Adds 3 filters to the graph so we can grab teletext
    /// On return the graph looks like this:
    //
    //	[							 ]		 [  tee/sink	]			 [	wst			]			[ sample	]
    //	[	capture			 ]		 [		to			]----->[	codec		]---->[ grabber	]
    //	[						vbi]---->[	sink			]			 [					]			[					]
    /// </summary>
    void SetupTeletext()
    {
      if (!CheckThreadId()) return;

      int hr;
      Log.Log.WriteFile("analog: SetupTeletext()");

      DsDevice[] devices;
      Guid guidBaseFilter = typeof(IBaseFilter).GUID;
      object obj;
      //find and add tee/sink to sink filter
      devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSSplitter);
      devices[0].Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
      _teeSink = (IBaseFilter)obj;
      hr = _graphBuilder.AddFilter(_teeSink, devices[0].Name);
      if (hr != 0)
      {
        Log.Log.Error("analog:SinkGraphEx.SetupTeletext(): Unable to add tee/sink filter");
        return;
      }

      //connect capture filter -> tee sink filter
      IPin pin = DsFindPin.ByDirection(_teeSink, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(_pinVBI, pin);
      Marshal.ReleaseComObject(pin);
      if (hr != 0)
      {
        //failed...
        Log.Log.Error("analog: unable  to connect capture->tee/sink");
        _graphBuilder.RemoveFilter(_teeSink);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = _filterGrabber = null;
        return;
      }

      //find the WST codec filter
      devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSVBICodec);
      foreach (DsDevice device in devices)
      {
        if (device.Name.IndexOf("WST") >= 0)
        {
          //found it, add it to the graph
          device.Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
          _filterWstDecoder = (IBaseFilter)obj;
          hr = _graphBuilder.AddFilter((IBaseFilter)_filterWstDecoder, device.Name);
          if (hr != 0)
          {
            //failed...
            Log.Log.Error("analog:SinkGraphEx.SetupTeletext(): Unable to add WSTCODEC filter");
            _graphBuilder.RemoveFilter(_teeSink);
            Marshal.ReleaseComObject(_teeSink);
            _teeSink = _filterWstDecoder = _filterGrabber = null;
            return;
          }
          break;
        }
      }
      if (_filterWstDecoder == null)
      {
        Log.Log.Error("analog: unable  to add WST Codec filter");
        _graphBuilder.RemoveFilter(_teeSink);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = _filterGrabber = null;
        return;
      }

      //connect tee sink filter-> wst codec filter
      IPin pinOut = DsFindPin.ByDirection(_teeSink, PinDirection.Output, 0);
      pin = DsFindPin.ByDirection(_filterWstDecoder, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(pinOut, pin);
      Marshal.ReleaseComObject(pin);
      Marshal.ReleaseComObject(pinOut);
      if (hr != 0)
      {
        //failed
        Log.Log.Error("analog: unable  to tee/sink->wst codec");
        _graphBuilder.RemoveFilter(_filterWstDecoder);
        _graphBuilder.RemoveFilter(_teeSink);
        Marshal.ReleaseComObject(_filterWstDecoder);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = _filterGrabber = null;
        _teeSink = null;
        return;
      }

      //create and add the sample grabber filter to the graph
      _filterGrabber = (IBaseFilter)new SampleGrabber();
      ISampleGrabber sampleGrabberInterface = (ISampleGrabber)_filterGrabber;
      _graphBuilder.AddFilter(_filterGrabber, "Sample Grabber");


      //setup the sample grabber filter
      AMMediaType mt = new AMMediaType();
      mt.majorType = MediaType.VBI;
      mt.subType = MediaSubType.TELETEXT;
      sampleGrabberInterface.SetCallback(this, 1);
      sampleGrabberInterface.SetMediaType(mt);
      sampleGrabberInterface.SetBufferSamples(true);

      //connect the wst codec filter->sample grabber filter
      pinOut = DsFindPin.ByDirection(_filterWstDecoder, PinDirection.Output, 0);
      pin = DsFindPin.ByDirection(_filterGrabber, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(pinOut, pin);
      Marshal.ReleaseComObject(pin);
      Marshal.ReleaseComObject(pinOut);
      if (hr != 0)
      {
        //failed
        Log.Log.Error("analog: unable to wst codec->grabber");
        _graphBuilder.RemoveFilter(_filterGrabber);
        _graphBuilder.RemoveFilter(_filterWstDecoder);
        _graphBuilder.RemoveFilter(_teeSink); ;
        Marshal.ReleaseComObject(_filterGrabber);
        Marshal.ReleaseComObject(_filterWstDecoder);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = _filterGrabber = null;
        return;
      }

      //done
      Log.Log.WriteFile("analog: teletext setup");

    }
    #endregion

    #region s/w encoding card specific graph building


    /// <summary>
    /// Find a pin on the filter specified
    /// which can supplies the mediatype and mediasubtype specified
    /// if found the pin is returned
    /// </summary>
    /// <param name="mediaType">Type of the media.</param>
    /// <param name="mediaSubtype">The media subtype.</param>
    IPin FindMediaPin(IBaseFilter filter, Guid mediaType, Guid mediaSubtype)
    {
      if (!CheckThreadId()) return null;
      IEnumPins enumPins;
      filter.EnumPins(out enumPins);

      // loop through all pins
      int pinNr = -1;
      while (true)
      {
        IPin[] pins = new IPin[2];
        int fetched;
        enumPins.Next(1, pins, out fetched);
        if (fetched != 1) break;
        //first check if the pindirection matches
        PinDirection pinDirection;
        pins[0].QueryDirection(out pinDirection);
        if (pinDirection != PinDirection.Output) continue;
        pinNr++;

        //next check if the pin supports the media type requested
        IEnumMediaTypes enumMedia;
        int fetchedMedia;
        AMMediaType[] media = new AMMediaType[2];
        pins[0].EnumMediaTypes(out enumMedia);
        while (true)
        {
          enumMedia.Next(1, media, out fetchedMedia);
          if (fetchedMedia != 1) break;
          if (media[0].majorType == mediaType)
          {
            if (media[0].subType == mediaSubtype || mediaSubtype == MediaSubType.Null)
            {
              //it does... we're done
              Log.Log.WriteFile("analog: FindMediaPin pin:#{0} {1}", pinNr,FilterGraphTools.LogPinInfo(pins[0]));
              Log.Log.WriteFile("analog: FindMediaPin   major:{0} sub:{1}", media[0].majorType, media[0].subType);
              Log.Log.WriteFile("analog: FindMediaPin succeeded");
              DsUtils.FreeAMMediaType(media[0]);
              return pins[0];
            }
          }
          DsUtils.FreeAMMediaType(media[0]);
        }
        Release.ComObject("capture pin", pins[0]);
      }
      return null;
    }

    /// <summary>
    /// Finds the analog audio/video output pins
    /// </summary>
    /// <returns></returns>
    bool FindAudioVideoPins()
    {
      Log.Log.WriteFile("analog: FindAudioVideoPins");
      if (_filterMultiplexer != null)
      {
        Log.Log.WriteFile("analog:   find pins on multiplexer");
        if (_pinAnalogAudio == null)
          _pinAnalogAudio = FindMediaPin(_filterMultiplexer, MediaType.Audio, MediaSubType.Null);
        if (_pinAnalogVideo == null)
          _pinAnalogVideo = FindMediaPin(_filterMultiplexer, MediaType.Video, MediaSubType.Null);
      }
      if (_filterVideoEncoder != null)
      {
        Log.Log.WriteFile("analog:   find pins on video encoder");
        if (_pinAnalogAudio == null)
          _pinAnalogAudio = FindMediaPin(_filterVideoEncoder, MediaType.Audio, MediaSubType.Null);
        if (_pinAnalogVideo == null)
          _pinAnalogVideo = FindMediaPin(_filterVideoEncoder, MediaType.Video, MediaSubType.Null);
      }
      if (_filterAudioEncoder != null)
      {
        Log.Log.WriteFile("analog:   find pins on audio encoder");
        if (_pinAnalogAudio == null)
          _pinAnalogAudio = FindMediaPin(_filterAudioEncoder, MediaType.Audio, MediaSubType.Null);
        if (_pinAnalogVideo == null)
          _pinAnalogVideo = FindMediaPin(_filterAudioEncoder, MediaType.Video, MediaSubType.Null);
      }
      if (_filterCapture != null)
      {
        Log.Log.WriteFile("analog:   find pins on capture filter");
        if (_pinAnalogAudio == null)
          _pinAnalogAudio = FindMediaPin(_filterCapture, MediaType.Audio, MediaSubType.Null);
        if (_pinAnalogVideo == null)
          _pinAnalogVideo = FindMediaPin(_filterCapture, MediaType.Video, MediaSubType.Null);
      }

      if (_pinAnalogVideo == null || _pinAnalogAudio == null)
        return false;
      return true;
    }

    /// <summary>
    /// Adds the audio compressor.
    /// </summary>
    /// <returns></returns>
    bool AddAudioCompressor()
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("analog: AddAudioCompressor {0}", FilterGraphTools.LogPinInfo(_pinAnalogAudio));
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(AudioCompressorCategory);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(LegacyAmFilterCategory);
      string[] audioEncoders = new string[] { "InterVideo Audio Encoder", "Ulead MPEG Audio Encoder", "MainConcept MPEG Audio Encoder", "MainConcept Demo MPEG Audio Encoder", "CyberLink Audio Encoder", "CyberLink Audio Encoder(Twinhan)","Pinnacle MPEG Layer-2 Audio Encoder","MainConcept (Hauppauge) MPEG Audio Encoder" };
      DsDevice[] audioDevices = new DsDevice[audioEncoders.Length];
      for (int x = 0; x < audioEncoders.Length; ++x)
      {
        audioDevices[x] = null;
      }

      for (int i = 0; i < devices1.Length; i++)
      {
        for (int x = 0; x < audioEncoders.Length; ++x)
        {
          if (audioEncoders[x] == devices1[i].Name)
          {
            audioDevices[x] = devices1[i];
            break;
          }
        }
      }
      for (int i = 0; i < devices2.Length; i++)
      {
        for (int x = 0; x < audioEncoders.Length; ++x)
        {
          if (audioEncoders[x] == devices2[i].Name)
          {
            audioDevices[x] = devices2[i];
            break;
          }
        }
      }
      IBaseFilter tmp;
      //for each compressor
      Log.Log.WriteFile("analog: AddAudioCompressor found:{0} compressor", audioDevices.Length);
      for (int i = 0; i < audioDevices.Length; ++i)
      {
        if (audioDevices[i] == null) continue;
        Log.Log.WriteFile("analog:  try compressor:{0}", audioDevices[i].Name);
        int hr;
        try
        {
          //add compressor filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(audioDevices[i].Mon, null, audioDevices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter {0} to graph", audioDevices[i].Name);
          continue;
        }

        if (hr != 0)
        {
          //failed to add filter to graph, continue with the next one
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("audiocompressor", tmp);
            tmp = null;
          }
          continue;
        }
        if (tmp == null) continue;

        Log.Log.WriteFile("analog: connect audio pin->audio compressor");
        // check if this compressor filter has an mpeg audio output pin
        IPin pinAudio = DsFindPin.ByDirection(tmp, PinDirection.Input, 0);
        if (pinAudio == null)
        {
          Log.Log.WriteFile("analog: cannot find audio pin on compressor");
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("audiocompressor", tmp);
          tmp = null;
          continue;
        }

        // we found a nice compressor, lets try to connect the analog audio pin to the compressor
        hr = _graphBuilder.Connect(_pinAnalogAudio, pinAudio);
        if (hr != 0)
        {
          Log.Log.WriteFile("analog: failed to connect audio pin->audio compressor:{0:X}",hr);
          //unable to connec the pin, remove it and continue with next compressor
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("audiocompressor", tmp);
          tmp = null;
          continue;
        }
        Log.Log.WriteFile("analog: connected audio pin->audio compressor");
        //succeeded.
        _filterAudioCompressor = tmp;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Adds the video compressor.
    /// </summary>
    /// <returns></returns>
    bool AddVideoCompressor()
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("analog: AddVideoCompressor");

      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(VideoCompressorCategory);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(LegacyAmFilterCategory);
      string[] videoEncoders = new string[] { "InterVideo Video Encoder", "Ulead MPEG Encoder", "MainConcept MPEG Video Encoder", "MainConcept Demo MPEG Video Encoder", "CyberLink MPEG Video Encoder", "CyberLink MPEG Video Encoder(Twinhan)","MainConcept (Hauppauge) MPEG Video Encoder","nanocosmos MPEG Video Encoder","Pinnacle MPEG 2 Encoder" };
      DsDevice[] videoDevices = new DsDevice[videoEncoders.Length];
      for (int x = 0; x < videoEncoders.Length; ++x)
      {
        videoDevices[x] = null;
      }

      for (int i = 0; i < devices1.Length; i++)
      {
        for (int x = 0; x < videoEncoders.Length; ++x)
        {
          if (videoEncoders[x] == devices1[i].Name)
          {
            videoDevices[x] = devices1[i];
            break;
          }
        }
      }
      for (int i = 0; i < devices2.Length; i++)
      {
        for (int x = 0; x < videoEncoders.Length; ++x)
        {
          if (videoEncoders[x] == devices2[i].Name)
          {
            videoDevices[x] = devices2[i];
            break;
          }
        }
      }
      //for each compressor
      IBaseFilter tmp;
      Log.Log.WriteFile("analog: AddVideoCompressor found:{0} compressor", videoDevices.Length);
      for (int i = 0; i < videoDevices.Length; i++)
      {
        if (videoDevices[i] == null) continue;
        Log.Log.WriteFile("analog:  try compressor:{0}", videoDevices[i].Name);
        int hr;
        try
        {
          //add compressor filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(videoDevices[i].Mon, null, videoDevices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter {0} to graph", videoDevices[i].Name);
          continue;
        }

        if (hr != 0)
        {
          //failed to add filter to graph, continue with the next one
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("videocompressor", tmp);
            tmp = null;
          }
          continue;
        }
        if (tmp == null) continue;

        // check if this compressor filter has an mpeg audio output pin
        Log.Log.WriteFile("analog:  connect video pin->video compressor");
        IPin pinVideo = DsFindPin.ByDirection(tmp,PinDirection.Input,0);

        // we found a nice compressor, lets try to connect the analog video pin to the compressor
        hr = _graphBuilder.Connect(_pinAnalogVideo, pinVideo);
        if (hr != 0)
        {
          Log.Log.WriteFile("analog: failed to connect video pin->video compressor");
          //unable to connec the pin, remove it and continue with next compressor
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("videocompressor", tmp);
          tmp = null;
          continue;
        }
        //succeeded.
        _filterVideoCompressor = tmp;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Adds the mpeg muxer
    /// </summary>
    /// <returns></returns>
    bool AddAnalogMuxer()
    {
      string monikerPowerDirectorMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7F2BBEAF-E11C-4D39-90E8-938FB5A86045}";
      _filterAnalogMpegMuxer = Marshal.BindToMoniker(monikerPowerDirectorMuxer) as IBaseFilter;
      int hr = _graphBuilder.AddFilter(_filterAnalogMpegMuxer, "Analog MPEG Muxer");
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:AddAnalogMuxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add AddAnalogMuxer");
      }
      // next connect audio compressor->muxer
      IPin pinOut = DsFindPin.ByDirection(_filterAudioCompressor, PinDirection.Output, 0);
      IPin pinIn = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Input, 1);
      if (pinOut == null)
      {
        throw new TvException("no output pin found on audio compressor");
      } if (pinIn == null)
      {
        throw new TvException("no input pin found on muxer");
      }

      hr = _graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:unable to connect audio compressor->muxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add unable to connect audio compressor->muxer");
      }

      // next connect video compressor->muxer
      pinOut = DsFindPin.ByDirection(_filterVideoCompressor, PinDirection.Output, 0);
      pinIn = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Input, 0);
      if (pinOut == null)
      {
        throw new TvException("no output pin found on video compressor");
      }
      if (pinIn == null)
      {
        throw new TvException("no input pin found on muxer");
      }

      hr = _graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:unable to connect video compressor->muxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add unable to connect video compressor->muxer");
      }

      //and finally we have a capture pin...
      _pinCapture = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Output, 0);
      return true;
    }
    #endregion

    #region timeshifting and recording
    void AddMpeg2Demultiplexer()
    {
      if (!CheckThreadId()) return;
      if (_filterMpeg2Demux != null) return;
      if (_pinCapture == null) return;
      Log.Log.WriteFile("analog: AddMPEG2DemuxFilter");
      int hr = 0;

      _filterMpeg2Demux = (IBaseFilter)new MPEG2Demultiplexer();

      hr = _graphBuilder.AddFilter(_filterMpeg2Demux, "MPEG2 Demultiplexer");
      if (hr != 0)
      {
        Log.Log.WriteFile("analog: AddMPEG2DemuxFilter returns:0x{0:X}", hr);
        throw new TvException("Unable to add MPEG2 demultiplexer");
      }

      IPin pin = DsFindPin.ByDirection(_filterMpeg2Demux, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(_pinCapture, pin);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog: ConnectFilters returns:0x{0:X}", hr);
        throw new TvException("Unable to connect capture-> MPEG2 demultiplexer");
      }

      IMpeg2Demultiplexer demuxer = (IMpeg2Demultiplexer)_filterMpeg2Demux;

      hr = demuxer.CreateOutputPin(FilterGraphTools.GetVideoMpg2Media(), "Video", out _pinVideo);
      hr = demuxer.CreateOutputPin(FilterGraphTools.GetAudioMpg2Media(), "Audio", out _pinAudio);
      hr = demuxer.CreateOutputPin(FilterGraphTools.GetAudioLPCMMedia(), "LPCM", out _pinLPCM);

      IMPEG2StreamIdMap map = (IMPEG2StreamIdMap)_pinVideo;
      hr = map.MapStreamId(224, MPEG2Program.ElementaryStream, 0, 0);

      map = (IMPEG2StreamIdMap)_pinAudio;
      hr = map.MapStreamId(0xC0, MPEG2Program.ElementaryStream, 0, 0);

      map = (IMPEG2StreamIdMap)_pinLPCM;
      hr = map.MapStreamId(0xBD, MPEG2Program.ElementaryStream, 0xA0, 7);

    }


    /// <summary>
    /// sets the filename used for timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected void SetTimeShiftFileName(string fileName)
    {
      if (!CheckThreadId()) return;
      _timeshiftFileName = fileName;
      Log.Log.WriteFile("analog:SetTimeShiftFileName:{0}", fileName);
      int hr;
      if (_tsFileSink != null)
      {
        Log.Log.WriteFile("analog:SetTimeShiftFileName: uses .ts");
        IMPRecord record = _tsFileSink as IMPRecord;
        record.SetTimeShiftFileName(fileName);
        record.StartTimeShifting();
      }

    }

    /// <summary>
    /// adds the TsFileSink filter to the graph
    /// </summary>
    protected void AddTsFileSink(bool isTv)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog:AddTsFileSink");
      _tsFileSink = (IBaseFilter)new MpFileWriter();
      int hr = _graphBuilder.AddFilter((IBaseFilter)_tsFileSink, "TsFileSink");
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:AddTsFileSink returns:0x{0:X}", hr);
        throw new TvException("Unable to add TsFileSink");
      }

      IPin pin = DsFindPin.ByDirection(_filterMpegMuxer, PinDirection.Output, 0);
      FilterGraphTools.ConnectPin(_graphBuilder, pin, (IBaseFilter)_tsFileSink, 0);
      Release.ComObject("mpegmux pinin", pin);
    }

    /// <summary>
    /// Adds the MPEG muxer filter
    /// </summary>
    /// <param name="isTv">if set to <c>true</c> [is tv].</param>
    protected void AddMpegMuxer(bool isTv)
    {
      if (!CheckThreadId()) return;
      if (_filterMpegMuxer != null) return;
      Log.Log.WriteFile("analog:AddMpegMuxer()");
      try
      {
        string monikerPowerDirectorMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7F2BBEAF-E11C-4D39-90E8-938FB5A86045}";
        //string monikerPowerDvdMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{6770E328-9B73-40C5-91E6-E2F321AEDE57}";
        _filterMpegMuxer = Marshal.BindToMoniker(monikerPowerDirectorMuxer) as IBaseFilter;
        int hr = _graphBuilder.AddFilter(_filterMpegMuxer, "CyberLink MPEG Muxer");
        if (hr != 0)
        {
          Log.Log.WriteFile("analog:AddMpegMuxer returns:0x{0:X}", hr);
          throw new TvException("Unable to add Cyberlink MPEG Muxer");
        }
        if (isTv)
        {
          FilterGraphTools.ConnectPin(_graphBuilder, _pinVideo, _filterMpegMuxer, 0);
        }
        FilterGraphTools.ConnectPin(_graphBuilder, _pinAudio, _filterMpegMuxer, 1);

        //_infTee = (IBaseFilter)new InfTee();
        //hr = _graphBuilder.AddFilter(_infTee, "Inf Tee");
        //if (hr != 0)
        //{
        //  Log.Log.WriteFile("analog:Add InfTee returns:0x{0:X}", hr);
        //  throw new TvException("Unable to add InfTee");
        //}
        //IPin pin = DsFindPin.ByDirection(_filterMpegMuxer, PinDirection.Output, 0);
        //FilterGraphTools.ConnectPin(_graphBuilder, pin, _infTee, 0);
        //Release.ComObject("mpegmux out", pin);
      }
      catch (Exception)
      {
        throw new TvException("Cyberlink MPEG Muxer filter (mpgmux.ax) not installed");
      }
    }
    #endregion

    #endregion

    #region recording
    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="recordingType">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <param name="startTime">time the recording should start (0=now)</param>
    /// <returns></returns>
    protected void StartRecord(bool transportStream, string fileName)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog:StartRecord({0})", fileName);
      //int hr;
      if (_tsFileSink != null)
      {
        Log.Log.WriteFile("dvb:SetRecording: uses .mpg");
        IMPRecord record = _tsFileSink as IMPRecord;
        record.SetRecordingFileName(fileName);
        record.StartRecord();
      }
      _dateRecordingStarted = DateTime.Now;
    }


    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected void StopRecord()
    {
      if (!CheckThreadId()) return;
      //int hr;
      Log.Log.WriteFile("analog:StopRecord()");

      if (_tsFileSink != null)
      {
        IMPRecord record = _tsFileSink as IMPRecord;
        record.StopRecord();
      }
      _recordingFileName = "";
    }
    #endregion

    #region start/stop graph
    /// <summary>
    /// Methods which starts the graph
    /// </summary>
    protected void RunGraph()
    {
      if (!CheckThreadId()) return;
      FilterState state;
      (_graphBuilder as IMediaControl).GetState(10, out state);
      if (state == FilterState.Running) return;
      Log.Log.WriteFile("analog: RunGraph");
      _teletextDecoder.ClearBuffer();
      int hr = 0;
      hr = (_graphBuilder as IMediaControl).Run();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("analog: RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to start graph");
      }
      if (_tsFileSink != null)
      {
        _dateTimeShiftStarted = DateTime.Now;
      }
    }

    /// <summary>
    /// Methods which stops the graph
    /// </summary>
    public void StopGraph()
    {
      if (!CheckThreadId()) return;
      FilterState state;
      if (_graphBuilder == null) return;
      (_graphBuilder as IMediaControl).GetState(10, out state);

      Log.Log.WriteFile("analog: StopGraph");
      _teletextDecoder.ClearBuffer();

      _isScanning = false;
      _recordingFileName = "";
      _timeshiftFileName = "";
      _dateTimeShiftStarted = DateTime.MinValue;
      _dateRecordingStarted = DateTime.MinValue;
      int hr = 0;
      //hr = (_graphBuilder as IMediaControl).StopWhenReady();
      if (state == FilterState.Stopped) return;
      hr = (_graphBuilder as IMediaControl).Stop();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("analog: RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to stop graph");
      }
    }
    #endregion


    #region IDisposable Members

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
      if (_graphBuilder == null) return;
      Log.Log.WriteFile("analog:Dispose()");
      if (!CheckThreadId()) return;
      // Decompose the graph
      //int hr = (_graphBuilder as IMediaControl).StopWhenReady();
      int hr = (_graphBuilder as IMediaControl).Stop();

      FilterGraphTools.RemoveAllFilters(_graphBuilder);


      if (_filterTvTuner != null)
      {
        while (Marshal.ReleaseComObject(_filterTvTuner) > 0) ;
        _filterTvTuner = null;
      }
      if (_filterTvAudioTuner != null)
      {
        while (Marshal.ReleaseComObject(_filterTvAudioTuner) > 0) ;
        //Release.ComObject("audiotvtuner filter", _filterTvAudioTuner);
        _filterTvAudioTuner = null;
      }
      if (_filterCapture != null)
      {
        while (Marshal.ReleaseComObject(_filterCapture) > 0) ;
        //Release.ComObject("capture filter", _filterCapture);
        _filterCapture = null;
      }
      if (_filterVideoEncoder != null)
      {
        while (Marshal.ReleaseComObject(_filterVideoEncoder) > 0) ;
        //Release.ComObject("video encoder filter", _filterVideoEncoder);
        _filterVideoEncoder = null;
      }
      if (_filterAudioEncoder != null)
      {
        while (Marshal.ReleaseComObject(_filterAudioEncoder) > 0) ;
        //Release.ComObject("audio encoder filter", _filterAudioEncoder);
        _filterAudioEncoder = null;
      }
      if (_filterMpeg2Demux != null)
      {
        Release.ComObject("mpeg2 demux filter", _filterMpeg2Demux);
        _filterMpeg2Demux = null;
      }
      if (_filterGrabber != null)
      {
        Release.ComObject("grabber filter", _filterGrabber);
        _filterGrabber = null;
      }
      if (_filterWstDecoder != null)
      {
        Release.ComObject("wst codec filter", _filterWstDecoder);
        _filterWstDecoder = null;
      }
      if (_teeSink != null)
      {
        Release.ComObject("teesink filter", _teeSink);
        _teeSink = null;
      }

      //if (_filterDump1 != null)
      //{
      //  Release.ComObject("Dump1 filter", _filterDump1); _filterDump1 = null;
      //}
      //if (_infTee != null)
      //{
      //  Release.ComObject("InfTee filter", _infTee); _infTee = null;
      //}

      if (_filterAnalogMpegMuxer != null)
      {
        Release.ComObject("MPEG2 analog mux filter", _filterAnalogMpegMuxer); _filterAnalogMpegMuxer = null;
      }
      if (_filterMpegMuxer != null)
      {
        Release.ComObject("MPEG2 mux filter", _filterMpegMuxer); _filterMpegMuxer = null;
      }
      if (_tsFileSink != null)
      {
        Release.ComObject("tsFileSink filter", _tsFileSink); _tsFileSink = null;
      }
      if (_filterCrossBar != null)
      {
        Release.ComObject("crossbar filter", _filterCrossBar);
        _filterCrossBar = null;
      }

      if (_filterMultiplexer != null)
      {
        Release.ComObject("multiplexer filter", _filterMultiplexer);
        _filterMultiplexer = null;
      }

      if (_filterAudioCompressor != null)
      {
        Release.ComObject("_filterAudioCompressor", _filterAudioCompressor);
        _filterAudioCompressor = null;
      }
      if (_filterVideoCompressor != null)
      {
        Release.ComObject("_filterVideoCompressor", _filterVideoCompressor);
        _filterVideoCompressor = null;
      }

      if (_pinAnalogAudio != null)
      {
        Release.ComObject("_pinAnalogAudio", _pinAnalogAudio);
        _pinAnalogAudio = null;
      }
      if (_pinAnalogVideo != null)
      {
        Release.ComObject("_pinAnalogVideo", _pinAnalogVideo);
        _pinAnalogVideo = null;
      }
      if (_pinCapture != null)
      {
        Release.ComObject("capturepin filter", _pinCapture);
        _pinCapture = null;
      }
      if (_pinVideo != null)
      {
        Release.ComObject("videopin filter", _pinVideo);
        _pinVideo = null;
      }
      if (_pinAudio != null)
      {
        Release.ComObject("audiopin filter", _pinAudio);
        _pinAudio = null;
      }
      if (_pinLPCM != null)
      {
        Release.ComObject("lpcmpin filter", _pinLPCM);
        _pinLPCM = null;
      }

      if (_pinVBI != null)
      {
        Release.ComObject("vbipin filter", _pinVBI);
        _pinVBI = null;
      }
      _rotEntry.Dispose();
      Release.ComObject("Graphbuilder", _graphBuilder); _graphBuilder = null;


      DevicesInUse.Instance.Remove(_tunerDevice);
      if (_audioDevice != null)
      {
        DevicesInUse.Instance.Remove(_audioDevice);
        _audioDevice = null;
      }
      if (_crossBarDevice != null)
      {
        DevicesInUse.Instance.Remove(_crossBarDevice);
        _crossBarDevice = null;
      }
      if (_captureDevice != null)
      {
        DevicesInUse.Instance.Remove(_captureDevice);
        _captureDevice = null;
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
      _graphState = GraphState.Idle;
    }

    #endregion

    #region ISampleGrabberCB Members

    public IVbiCallback TeletextCallback
    {
      get
      {
        return _teletextCallback;
      }
      set
      {
        _teletextCallback = value;
      }
    }

    /// <summary>
    /// callback from ISampleGrabber filter
    /// </summary>
    /// <param name="SampleTime">media sample timestamp</param>
    /// <param name="pSample">IMediaSample</param>
    /// <returns></returns>
    public int SampleCB(double SampleTime, IMediaSample pSample)
    {
      return 0;
    }

    /// <summary>
    /// callback from ISampleGrabber filter
    /// </summary>
    /// <param name="SampleTime">The sample time.</param>
    /// <param name="pBuffer">The buffer.</param>
    /// <param name="BufferLen">The buffer length</param>
    /// <returns></returns>
    public int BufferCB(double SampleTime, System.IntPtr pBuffer, int BufferLen)
    {
      try
      {
        if (false == _grabTeletext || pBuffer == IntPtr.Zero || BufferLen < 43)
        {
          return 0;
        }
        if (_teletextCallback != null)
        {
          _teletextCallback.OnVbiData(pBuffer, BufferLen, true);
        }
        _teletextDecoder.SaveAnalogData(pBuffer, BufferLen);
      }
      catch (Exception ex)
      {
        Log.Log.WriteFile(ex.ToString());
      }
      return 0;
    }

    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _name;
    }
    /// <summary>
    /// Gets/sets the card device
    /// </summary>
    public string DevicePath
    {
      get
      {
        return _tunerDevice.DevicePath;
      }
    }
    /// <summary>
    /// gets the current filename used for timeshifting
    /// </summary>
    public string TimeShiftFileName
    {
      get
      {
        return _timeshiftFileName;
      }
    }



    /// <summary>
    /// returns true if card is currently grabbing the epg
    /// </summary>
    public bool IsEpgGrabbing
    {
      get
      {
        return false;
      }
      set
      {
      }
    }

    /// <summary>
    /// returns true if we timeshift in transport stream mode
    /// false we timeshift in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public bool IsTimeshiftingTransportStream
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// returns true if we record in transport stream mode
    /// false we record in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public bool IsRecordingTransportStream
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// returns true if card is currently scanning
    /// </summary>
    public bool IsScanning
    {
      get
      {
        return _isScanning;
      }
      set
      {
        _isScanning = value;
      }
    }
    /// <summary>
    /// returns the max. channel numbers for analog cards
    /// </summary>
    public int MaxChannel
    {
      get
      {
        if (_filterTvTuner == null) return 128;
        int minChannel, maxChannel;
        IAMTVTuner tvTuner = _filterTvTuner as IAMTVTuner;
        if (tvTuner == null) return 128;
        tvTuner.ChannelMinMax(out minChannel, out maxChannel);
        return maxChannel;
      }
    }
    /// <summary>
    /// returns the min. channel numbers for analog cards
    /// </summary>
    /// <value>The min channel.</value>
    public int MinChannel
    {
      get
      {
        if (_filterTvTuner == null) return 0;
        int minChannel, maxChannel;
        IAMTVTuner tvTuner = _filterTvTuner as IAMTVTuner;
        if (tvTuner == null) return 0;
        tvTuner.ChannelMinMax(out minChannel, out maxChannel);
        return minChannel;
      }
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime StartOfTimeShift
    {
      get
      {
        return _dateTimeShiftStarted;
      }
    }
    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted
    {
      get
      {
        return _dateRecordingStarted;
      }
    }
    #region audio streams
    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    public List<IAudioStream> AvailableAudioStreams
    {
      get
      {
        List<IAudioStream> streams = new List<IAudioStream>();
        if (_filterTvAudioTuner == null) return streams;
        IAMTVAudio tvAudioTunerInterface = _filterTvAudioTuner as IAMTVAudio;
        TVAudioMode availableAudioModes;
        tvAudioTunerInterface.GetAvailableTVAudioModes(out availableAudioModes);
        if ((availableAudioModes & (TVAudioMode.Stereo)) != 0)
        {
          AnalogAudioStream stream = new AnalogAudioStream();
          stream.AudioMode = TVAudioMode.Stereo;
          stream.Language = "Stereo";
          streams.Add(stream);
        }
        if ((availableAudioModes & (TVAudioMode.Mono)) != 0)
        {
          AnalogAudioStream stream = new AnalogAudioStream();
          stream.AudioMode = TVAudioMode.Mono;
          stream.Language = "Mono";
          streams.Add(stream);
        }
        if ((availableAudioModes & (TVAudioMode.LangA)) != 0)
        {
          AnalogAudioStream stream = new AnalogAudioStream();
          stream.AudioMode = TVAudioMode.LangA;
          stream.Language = "LangA";
          streams.Add(stream);
        }
        if ((availableAudioModes & (TVAudioMode.LangB)) != 0)
        {
          AnalogAudioStream stream = new AnalogAudioStream();
          stream.AudioMode = TVAudioMode.LangB;
          stream.Language = "LangB";
          streams.Add(stream);
        }
        if ((availableAudioModes & (TVAudioMode.LangC)) != 0)
        {
          AnalogAudioStream stream = new AnalogAudioStream();
          stream.AudioMode = TVAudioMode.LangC;
          stream.Language = "LangC";
          streams.Add(stream);
        }

        return streams;
      }
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public IAudioStream CurrentAudioStream
    {
      get
      {
        if (_filterTvAudioTuner == null) return null;
        IAMTVAudio tvAudioTunerInterface = _filterTvAudioTuner as IAMTVAudio;
        TVAudioMode mode;
        tvAudioTunerInterface.get_TVAudioMode(out mode);
        List<IAudioStream> streams = AvailableAudioStreams;
        foreach (AnalogAudioStream stream in streams)
        {
          if (stream.AudioMode == mode) return stream;
        }
        return null;
      }
      set
      {
        AnalogAudioStream stream = value as AnalogAudioStream;
        if (stream != null && _filterTvAudioTuner != null)
        {
          IAMTVAudio tvAudioTunerInterface = _filterTvAudioTuner as IAMTVAudio;
          tvAudioTunerInterface.put_TVAudioMode(stream.AudioMode);
        }
      }
    }
    #endregion
    /// <summary>
    /// Deletes the time shifting filters
    /// </summary>
    protected void DeleteTimeShifting()
    {

      if (_tsFileSink != null)
      {
        IMPRecord record = _tsFileSink as IMPRecord;
        record.StopTimeShifting();
        _graphBuilder.RemoveFilter((IBaseFilter)_tsFileSink);
        Release.ComObject("tsfilesink filter", _tsFileSink); ;
        _tsFileSink = null;
      }

      if (_filterMpegMuxer != null)
      {
        _graphBuilder.RemoveFilter((IBaseFilter)_filterMpegMuxer);
        Release.ComObject("mpeg2 mux filter", _filterMpegMuxer); ;
        _filterMpegMuxer = null;
      }

      //if (_filterDump1 != null)
      //{
      //  _graphBuilder.RemoveFilter((IBaseFilter)_filterDump1);
      //  Release.ComObject("dump1 filter", _filterDump1); ;
      //  _filterDump1 = null;
      //}
      //if (_infTee != null)
      //{
      //  _graphBuilder.RemoveFilter((IBaseFilter)_infTee);
      //  Release.ComObject("_infTee filter", _infTee); ;
      //  _infTee = null;
      //}
    }

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    public bool IsReceivingAudioVideo
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Gets or sets the type of the cam.
    /// </summary>
    /// <value>The type of the cam.</value>
    public CamType CamType
    {
      get
      {
        return CamType.Default;
      }
      set
      {
      }
    }
    public object Context
    {
      get
      {
        return m_context;
      }
      set
      {
        m_context = value;
      }
    }
  }
}

