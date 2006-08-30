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
using TvLibrary.Interfaces.TsFileSink;
using TvLibrary.Teletext;
using TvLibrary.Epg;
using TvLibrary.Implementations.DVB;
using TvLibrary.Helper;

namespace TvLibrary.Implementations.Analog
{
  public class TvCardAnalogBase : ISampleGrabberCB
  {

    #region constants

    //KSCATEGORY_ENCODER
    public static readonly Guid AMKSEncoder = new Guid("19689BF6-C384-48fd-AD51-90E58C79F70B");
    //STATIC_KSCATEGORY_MULTIPLEXER
    public static readonly Guid AMKSMultiplexer = new Guid("7A5DE1D3-01A1-452c-B481-4FA2B96271E8");

    #endregion

    #region imports
    [DllImport("advapi32", CharSet = CharSet.Auto)]
    protected static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);
    [ComImport, Guid("DB35F5ED-26B2-4A2A-92D3-852E145BF32D")]
    protected class MpFileWriter { }
    #endregion

    #region enums
    protected enum GraphState
    {
      Idle,
      Created,
      TimeShifting,
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
    //protected IBaseFilter _filterDump1;
    //protected IBaseFilter _infTee;
    protected IPin _pinCapture = null;
    protected IPin _pinVideo = null;
    protected IPin _pinAudio = null;
    protected IPin _pinLPCM = null;
    protected IPin _pinVBI = null;
    protected DVBTeletext _teletextDecoder;
    protected string _recordingFileName;
    protected bool _grabTeletext = false;
    protected int _managedThreadId;
    protected DateTime _lastSignalUpdate;
    protected bool _tunerLocked;
    protected bool _isScanning = false;
    protected DateTime _dateTimeShiftStarted = DateTime.MinValue;
    protected DateTime _dateRecordingStarted = DateTime.MinValue;

    protected IChannel _currentChannel;
    string _timeshiftFileName;
    #endregion

    #region ctor

    public TvCardAnalogBase()
    {
    }
    public TvCardAnalogBase(DsDevice device)
    {
      _tunerDevice = device;
      _name = device.Name;
      _graphState = GraphState.Idle;
      _teletextDecoder = new DVBTeletext();
    }
    #endregion

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
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _rotEntry = new DsROTEntry(_graphBuilder);
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        AddTvTunerFilter();
        if (_filterTvTuner == null)
        {
          Log.Log.WriteFile("analog: unable to add tv tuner filter");
          throw new TvException("Analog: unable to add tv tuner filter");
        }
        AddCrossBarFilter();
        if (_filterCrossBar == null)
        {
          Log.Log.WriteFile("analog: unable to add tv crossbar filter");
          throw new TvException("Analog: unable to add tv crossbar filter");
        }
        AddTvAudioFilter();
        if (_filterTvAudioTuner == null)
        {
          Log.Log.WriteFile("analog: unable to add tv audio tuner filter");
          throw new TvException("Analog: unable to add tv audio tuner filter");
        }
        AddTvCaptureFilter();

        FindCapturePin(MediaType.Stream, MediaSubType.Null);
        if (_pinCapture == null)
        {
          if (!AddTvEncoderFilter(true))
          {
            AddTvEncoderFilter(false);
          }
          if (!AddTvMultiPlexer(true))
          {
            AddTvMultiPlexer(false);
          }
        }

        FindCapturePin(MediaType.Stream, MediaSubType.Null);
        if (_pinCapture == null)
          FindCapturePin(MediaType.Video, MediaSubType.Mpeg2Program);

        if (_pinCapture == null)
        {
          Log.Log.WriteFile("analog: FindCapturePin no capture pin found");
          throw new TvException("Unable to find capture pin");
        }
        FindVBIPin();
        if (_pinVBI != null)
        {
          SetupTeletext();
        }
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
        Log.Log.WriteFile("analog: AddTvTunerFilter failed:0x{0:X}", hr);
        throw new TvException("Unable to add tvtuner to graph");
      }
      _filterTvTuner = tmp;
      DevicesInUse.Instance.Add(_tunerDevice);
    }

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
      for (int i = 0; i < devices.Length; i++)
      {
        Log.Log.WriteFile("analog: AddTvAudioFilter try:{0}", devices[i].Name);
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
        int hr;
        try
        {
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }

        if (hr != 0)
        {
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("tvAudioFilter filter", tmp);
          }
          continue;
        }
        if (FilterGraphTools.ConnectFilter(_graphBuilder, _filterTvTuner, tmp))
        {
          // Got it !
          // Connect it to the crossbar
          IPin pin = DsFindPin.ByDirection(tmp, PinDirection.Output, 0);
          hr = _graphBuilder.Connect(pin, pinIn);
          if (hr < 0)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("audiotuner pinin", pin);
            Release.ComObject("audiotuner filter", tmp);
          }
          else
          {
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
          // Try another...
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
        Log.Log.WriteFile("analog: Unable to add TvAudioTuner to graph");
        throw new TvException("Unable to add TvAudioTuner to graph");
      }
    }

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

    void AddCrossBarFilter()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog: AddCrossBarFilter");
      DsDevice[] devices = null;
      IBaseFilter tmp;
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
      for (int i = 0; i < devices.Length; i++)
      {
        Log.Log.WriteFile("analog: AddCrossBarFilter try:{0}", devices[i].Name);
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
        int hr;
        try
        {
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }

        if (hr != 0)
        {
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("CrossBarFilter", tmp);
          }
          continue;
        }

        IPin pinIn = FindCrossBarPin(tmp, PhysicalConnectorType.Video_Tuner, PinDirection.Input);
        if (pinIn == null)
        {
          Log.Log.WriteFile("analog: AddCrossBarFilter no video tuner input pin detected");
          continue;
        }
        if (FilterGraphTools.ConnectFilter(_graphBuilder, _filterTvTuner, pinIn))
        {
          // Got it !
          _filterCrossBar = tmp;
          _crossBarDevice = devices[i];
          DevicesInUse.Instance.Add(_crossBarDevice);
          Release.ComObject("crossbar videotuner pin", pinIn);
          Log.Log.WriteFile("analog: AddCrossBarFilter succeeded");
          break;
        }
        else
        {
          // Try another...
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("crossbar videotuner pin", pinIn);
          Release.ComObject("crossbar filter", tmp);
        }
      }
      if (_filterCrossBar == null)
      {
        Log.Log.WriteFile("analog: Unable to add crossbar to graph");
        throw new TvException("Unable to add crossbar to graph");
      }
    }

    void AddTvCaptureFilter()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog: AddTvCaptureFilter");
      DsDevice[] devices = null;
      IBaseFilter tmp;
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
      for (int i = 0; i < devices.Length; i++)
      {
        Log.Log.WriteFile("analog: AddTvCaptureFilter try:{0}", devices[i].Name);
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
        int hr;
        try
        {
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }

        if (hr != 0)
        {
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("TvCaptureFilter", tmp);
          }
          continue;
        }

        hr = _capBuilder.RenderStream(null, null, _filterCrossBar, null, tmp);
        if (hr == 0)
        {
          // Got it !
          hr = _capBuilder.RenderStream(null, null, _filterCrossBar, null, tmp);
          _filterCapture = tmp;
          _captureDevice = devices[i];
          DevicesInUse.Instance.Add(_captureDevice);
          Log.Log.WriteFile("analog: AddTvCaptureFilter succeeded:0x{0:X}", hr);
          break;
        }
        else
        {
          // Try another...
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("capture filter", tmp);
        }
      }
      if (_filterCapture == null)
      {
        Log.Log.WriteFile("analog: Unable to add TvCaptureFilter to graph");
        throw new TvException("Unable to add TvCaptureFilter to graph");
      }
    }

    bool ConnectEncoderFilter(IBaseFilter filterEncoder, bool isVideo, bool isAudio, bool matchPinNames)
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("analog: ConnectEncoderFilter video:{0} audio:{1}", isVideo, isAudio);
      int hr;
      IPin pinInput1 = DsFindPin.ByDirection(filterEncoder, PinDirection.Input, 0);
      IPin pinInput2 = DsFindPin.ByDirection(filterEncoder, PinDirection.Input, 1);
      string pinName1 = "";
      string pinName2 = "";
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
        _filterCapture.EnumPins(out enumPins);
        enumPins.Next(20, pins, out pinsAvailable);
        Log.Log.WriteFile("analog:  pinsAvailable on capture filter:{0}", pinsAvailable);
        for (int i = 0; i < pinsAvailable; ++i)
        {
          PinDirection pinDir;
          pins[i].QueryDirection(out pinDir);
          if (pinDir == PinDirection.Input) continue;
          Log.Log.WriteFile("analog:  capture pin:{0} {1}", i, FilterGraphTools.LogPinInfo(pins[i]));
          PinInfo info;
          pins[i].QueryPinInfo(out info);
          string pinName = info.name;
          if (info.filter != null)
          {
            Release.ComObject("encoder pin" + i.ToString(), info.filter);
          }

          if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
          {
            hr = _graphBuilder.Connect(pins[i], pinInput1);
            if (hr == 0)
            {
              Log.Log.WriteFile("analog:  connected pin:{0} {1} with pin0", i, pinName);
              pinsConnected++;
            }

            if (pinsConnected == 1 && (isAudio == false || isVideo == false))
            {
              Log.Log.WriteFile("analog: ConnectEncoderFilter succeeded");
              return true;
            }
          }

          if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
          {
            hr = _graphBuilder.Connect(pins[i], pinInput2);
            if (hr == 0)
            {
              Log.Log.WriteFile("analog:  connected pin:{0} {1} with pin1", i, pinName);
              pinsConnected++;
            }
            if (pinsConnected == 2)
            {
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
      Log.Log.WriteFile("analog: ConnectEncoderFilter failed");
      return false;
    }

    bool ConnectMultiplexer(IBaseFilter filterMultiPlexer, bool matchPinNames)
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("analog: ConnectMultiplexer()");
      // options
      // 1) [capture]->[multiplexer]
      // 2) [capture]->[encoder]->[multiplexer]
      // 3) [capture]->[encoder]->[multiplexer]
      //             ->[encoder]->[           ]
      int hr;
      IPin pinInput1 = DsFindPin.ByDirection(filterMultiPlexer, PinDirection.Input, 0);
      IPin pinInput2 = DsFindPin.ByDirection(filterMultiPlexer, PinDirection.Input, 1);
      string pinName1 = "";
      string pinName2 = "";
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
            _filterCapture.EnumPins(out enumPins);
            enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  capture pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input) continue;
              Log.Log.WriteFile("analog:  capture pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));

              PinInfo info;
              pins[i].QueryPinInfo(out info);
              string pinName = info.name;
              if (info.filter != null)
              {
                Release.ComObject("capture pin" + i.ToString(), info.filter);
              }
              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                hr = _graphBuilder.Connect(pins[i], pinInput1);
                if (hr == 0)
                {
                  Log.Log.WriteFile("analog:  connected pin:{0} to pin1", i);
                  pinsConnected++;
                }
              }

              if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
              {
                if (pinInput2 != null)
                {
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0)
                  {
                    Log.Log.WriteFile("analog:  connected pin:{0} to pin2", i);
                    pinsConnected++;
                  }
                }
              }
              if (pinsConnected == 2)
              {
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

        if (_filterAudioEncoder == null || _filterVideoEncoder != null)
        {
          //option 1, connect the multiplexer to a single encoder filter
          Log.Log.WriteFile("analog: ConnectMultiplexer to encoder filter");
          int pinsConnected = 0;
          int pinsAvailable = 0;
          IPin[] pins = new IPin[20];
          IEnumPins enumPins = null;
          try
          {
            _filterVideoEncoder.EnumPins(out enumPins);
            enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  video encoder pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input) continue;
              Log.Log.WriteFile("analog:  videoencoder pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));

              PinInfo info;
              pins[i].QueryPinInfo(out info);
              string pinName = info.name;
              if (info.filter != null)
              {
                Release.ComObject("capture pin" + i.ToString(), info.filter);
              }
              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                hr = _graphBuilder.Connect(pins[i], pinInput1);

                if (hr == 0)
                {
                  Log.Log.WriteFile("analog:  connected pin:{0} to pin1", i);
                  pinsConnected++;
                }
              }
              if (pinInput2 != null)
              {
                if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
                {
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0) pinsConnected++;
                  if (pinsConnected == 2)
                  {
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
            _filterVideoEncoder.EnumPins(out enumPins);
            enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  videoencoder pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input) continue;
              Log.Log.WriteFile("analog:   pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));

              PinInfo info;
              pins[i].QueryPinInfo(out info);
              string pinName = info.name;
              if (info.filter != null)
              {
                Release.ComObject("capture pin" + i.ToString(), info.filter);
              }
              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                hr = _graphBuilder.Connect(pins[i], pinInput1);
                if (hr == 0)
                {
                  Log.Log.WriteFile("analog:  connected pin:{0}", i);
                  pinsConnected++;
                }
              }
              if (pinInput2 != null)
              {
                if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
                {
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0)
                  {
                    Log.Log.WriteFile("analog:  connected pin:{0}", i);
                    pinsConnected++;
                  }
                }
              }
              if (pinsConnected == 1)
              {
                Log.Log.WriteFile("analog: ConnectMultiplexer part 1 succeeded");
                break;
              }
            }
            if (pinsConnected == 0) return false;

            _filterAudioEncoder.EnumPins(out enumPins);
            enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  audioencoder pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input) continue;
              Log.Log.WriteFile("analog: audioencoder  pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));

              PinInfo info;
              pins[i].QueryPinInfo(out info);
              string pinName = info.name;
              if (info.filter != null)
              {
                Release.ComObject("capture pin" + i.ToString(), info.filter);
              }

              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                hr = _graphBuilder.Connect(pins[i], pinInput1);
                if (hr == 0)
                {
                  Log.Log.WriteFile("analog:  connected pin:{0}", i);
                  pinsConnected++;
                }
              }
              if (pinInput2 != null)
              {

                if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
                {
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0)
                  {
                    Log.Log.WriteFile("analog:  connected pin:{0}", i);
                    pinsConnected++;
                  }
                }
              }
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
      Log.Log.WriteFile("analog: ConnectMultiplexer failed");
      return false;
    }

    bool AddTvMultiPlexer(bool matchPinNames)
    {
      if (!CheckThreadId()) return false;
      //several posibilities
      //1. no tv multiplexer needed
      //2. tv multiplexer filter which is connected to a single encoder filter
      //3. tv multiplexer filter which is connected to two encoder filter (audio/video)
      //4. tv multiplexer filter which is connected to the capture filter
      Log.Log.WriteFile("analog: AddTvMultiPlexer");
      DsDevice[] devices = null;
      IBaseFilter tmp;
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
      for (int i = 0; i < devices.Length; i++)
      {
        Log.Log.WriteFile("analog: AddTvMultiPlexer try:{0}", devices[i].Name);
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
        int hr;
        try
        {
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }
        if (hr != 0)
        {
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("multiplexer filter", tmp);
          }
          continue;
        }
        if (ConnectMultiplexer(tmp, matchPinNames))
        {
          // Got it !

          _filterMultiplexer = tmp;
          _multiplexerDevice = devices[i];
          DevicesInUse.Instance.Add(_multiplexerDevice);
          Log.Log.WriteFile("analog: AddTvMultiPlexer succeeded");
          break;
        }
        else
        {
          // Try another...
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

    bool AddTvEncoderFilter(bool matchPinNames)
    {
      if (!CheckThreadId()) return false;
      //several posibilities
      // 1. no encoder filter needed
      // 2. single encoder filter with seperate audio/video inputs
      // 3. single encoder filter with an mpeg2 program stream input (I2S)
      // 4. two encoder filters. one for audio and one for video
      Log.Log.WriteFile("analog: AddTvEncoderFilter");
      bool finished = false;
      DsDevice[] devices = null;
      IBaseFilter tmp;
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
      for (int i = 0; i < devices.Length; i++)
      {
        Log.Log.WriteFile("analog: AddTvEncoderFilter try:{0}", devices[i].Name);
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
        int hr;
        try
        {
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }

        if (hr != 0)
        {
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("TvEncoderFilter", tmp);
          }
          continue;
        }
        if (tmp == null) continue;
        //bool success = false;
        IPin pin1 = DsFindPin.ByDirection(tmp, PinDirection.Input, 0);
        IPin pin2 = DsFindPin.ByDirection(tmp, PinDirection.Input, 1);
        if (pin1 != null) Log.Log.WriteFile("analog: encoder pin1:{0}", FilterGraphTools.LogPinInfo(pin1));
        if (pin2 != null) Log.Log.WriteFile("analog: encoder pin2:{0}", FilterGraphTools.LogPinInfo(pin2));
        if (pin1 != null && pin2 != null)
        {
          if (ConnectEncoderFilter(tmp, true, true, matchPinNames))
          {
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
          //check the media type of pin 1...
          IEnumMediaTypes enumMedia;
          AMMediaType[] media = new AMMediaType[20];
          int fetched;
          pin1.EnumMediaTypes(out enumMedia);
          enumMedia.Next(1, media, out fetched);
          if (fetched == 1)
          {
            Log.Log.WriteFile("analog: AddTvEncoderFilter encoder output major:{0} sub:{1}", media[0].majorType, media[0].subType);
            if (media[0].majorType == MediaType.Audio)
            {
              if (ConnectEncoderFilter(tmp, false, true, matchPinNames))
              {
                _filterAudioEncoder = tmp;
                _audioEncoderDevice = devices[i];
                DevicesInUse.Instance.Add(_audioEncoderDevice);
                //                success = true;
                Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded (audio encoder)");
                if (_filterVideoEncoder != null) finished = true;
                tmp = null;
              }
            }
            else
            {
              if (ConnectEncoderFilter(tmp, true, false, matchPinNames))
              {
                _filterVideoEncoder = tmp;
                _videoEncoderDevice = devices[i];
                DevicesInUse.Instance.Add(_videoEncoderDevice);
                //                success = true;
                Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded (video encoder)");
                if (_filterAudioEncoder != null) finished = true;
                tmp = null; ;
              }
            }
            DsUtils.FreeAMMediaType(media[0]);
          }
          else
          {
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

    void FindCapturePin(Guid mediaType, Guid mediaSubtype)
    {
      if (!CheckThreadId()) return;
      IEnumPins enumPins;
      if (_filterMultiplexer != null)
      {
        Log.Log.WriteFile("analog: FindCapturePin on multiplexer filter");
        _filterMultiplexer.EnumPins(out enumPins);
      }
      else if (_filterVideoEncoder != null)
      {
        Log.Log.WriteFile("analog: FindCapturePin on encoder filter");
        _filterVideoEncoder.EnumPins(out enumPins);
      }
      else
      {
        Log.Log.WriteFile("analog: FindCapturePin on capture filter");
        _filterCapture.EnumPins(out enumPins);
      }

      while (true)
      {
        IPin[] pins = new IPin[2];
        int fetched;
        enumPins.Next(1, pins, out fetched);
        if (fetched != 1) break;

        Log.Log.WriteFile("analog: FindCapturePin pin:{0}", FilterGraphTools.LogPinInfo(pins[0]));
        PinDirection pinDirection;
        pins[0].QueryDirection(out pinDirection);
        if (pinDirection != PinDirection.Output) continue;

        IEnumMediaTypes enumMedia;
        int fetchedMedia;
        AMMediaType[] media = new AMMediaType[2];
        pins[0].EnumMediaTypes(out enumMedia);
        while (true)
        {
          enumMedia.Next(1, media, out fetchedMedia);
          if (fetchedMedia != 1) break;
          Log.Log.WriteFile("analog: FindCapturePin   major:{0} sub:{1}", media[0].majorType, media[0].subType);
          if (media[0].majorType == mediaType)
          {
            if (media[0].subType == mediaSubtype || mediaSubtype == MediaSubType.Null)
            {
              _pinCapture = pins[0];
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

    protected void ConnectFilters()
    {
      if (!CheckThreadId()) return;
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

    void SetupTeletext()
    {
      if (!CheckThreadId()) return;

      //
      //	[							 ]		 [  tee/sink	]			 [	wst			]			[ sample	]
      //	[	capture			 ]		 [		to			]----->[	codec		]---->[ grabber	]
      //	[						vbi]---->[	sink			]			 [					]			[					]

      int hr;
      Log.Log.WriteFile("analog: SetupTeletext()");

      DsDevice[] devices;
      Guid guidBaseFilter = typeof(IBaseFilter).GUID;
      object obj;
      devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSSplitter);
      devices[0].Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
      _teeSink = (IBaseFilter)obj;
      hr = _graphBuilder.AddFilter(_teeSink, devices[0].Name);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:SinkGraphEx.SetupTeletext(): Unable to add tee/sink filter");
        return;
      }

      IPin pin = DsFindPin.ByDirection(_teeSink, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(_pinVBI, pin);
      Marshal.ReleaseComObject(pin);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog: : Unable to connect capture->tee/sink");
        _graphBuilder.RemoveFilter(_teeSink);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = _filterGrabber = null;
        return;
      }

      devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSVBICodec);
      foreach (DsDevice device in devices)
      {
        if (device.Name.IndexOf("WST") >= 0)
        {
          device.Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
          _filterWstDecoder = (IBaseFilter)obj;
          hr = _graphBuilder.AddFilter((IBaseFilter)_filterWstDecoder, device.Name);
          if (hr != 0)
          {
            Log.Log.WriteFile("analog:SinkGraphEx.SetupTeletext(): Unable to add WSTCODEC filter");
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
        Log.Log.WriteFile("analog: : Unable to add WST Codec filter");
        _graphBuilder.RemoveFilter(_teeSink);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = _filterGrabber = null;
        return;
      }

      IPin pinOut = DsFindPin.ByDirection(_teeSink, PinDirection.Output, 0);
      pin = DsFindPin.ByDirection(_filterWstDecoder, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(pinOut, pin);
      Marshal.ReleaseComObject(pin);
      Marshal.ReleaseComObject(pinOut);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog: : Unable to tee/sink->wst codec");
        _graphBuilder.RemoveFilter(_filterWstDecoder);
        _graphBuilder.RemoveFilter(_teeSink);
        Marshal.ReleaseComObject(_filterWstDecoder);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = _filterGrabber = null;
        _teeSink = null;
        return;
      }

      _filterGrabber = (IBaseFilter)new SampleGrabber();
      ISampleGrabber sampleGrabberInterface = (ISampleGrabber)_filterGrabber;
      _graphBuilder.AddFilter(_filterGrabber, "Sample Grabber");


      AMMediaType mt = new AMMediaType();
      mt.majorType = MediaType.VBI;
      mt.subType = MediaSubType.TELETEXT;
      sampleGrabberInterface.SetCallback(this, 1);
      sampleGrabberInterface.SetMediaType(mt);
      sampleGrabberInterface.SetBufferSamples(true);

      pinOut = DsFindPin.ByDirection(_filterWstDecoder, PinDirection.Output, 0);
      pin = DsFindPin.ByDirection(_filterGrabber, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(pinOut, pin);
      Marshal.ReleaseComObject(pin);
      Marshal.ReleaseComObject(pinOut);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog: Unable to wst codec->grabber");
        _graphBuilder.RemoveFilter(_filterGrabber);
        _graphBuilder.RemoveFilter(_filterWstDecoder);
        _graphBuilder.RemoveFilter(_teeSink); ;
        Marshal.ReleaseComObject(_filterGrabber);
        Marshal.ReleaseComObject(_filterWstDecoder);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = _filterGrabber = null;
        return;
      }

      Log.Log.WriteFile("analog: teletext setup");

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

    #region recording
    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="recordingType">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <param name="startTime">time the recording should start (0=now)</param>
    /// <returns></returns>
    protected void StartRecord(string fileName, RecordingType recordingType, ref long startTime)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog:StartRecord({0})", fileName);
      int hr;
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
      int hr;
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
    protected void StopGraph()
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
        Release.ComObject("tuner filter", _filterTvTuner);
        _filterTvTuner = null;
      }
      if (_filterTvAudioTuner != null)
      {
        Release.ComObject("audiotvtuner filter", _filterTvAudioTuner);
        _filterTvAudioTuner = null;
      }
      if (_filterCapture != null)
      {
        Release.ComObject("capture filter", _filterCapture);
        _filterCapture = null;
      }
      if (_filterVideoEncoder != null)
      {
        Release.ComObject("video encoder filter", _filterVideoEncoder);
        _filterVideoEncoder = null;
      }
      if (_filterAudioEncoder != null)
      {
        Release.ComObject("audio encoder filter", _filterAudioEncoder);
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
    }

    #endregion

    #region ISampleGrabberCB Members


    public int SampleCB(double SampleTime, IMediaSample pSample)
    {
      return 0;
    }

    public int BufferCB(double SampleTime, System.IntPtr pBuffer, int BufferLen)
    {
      try
      {
        if (false == _grabTeletext || pBuffer == IntPtr.Zero || BufferLen < 43)
        {
          return 0;
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
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {
      if ((channel as AnalogChannel) == null) return false;
      return true;
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
    /// returns the min/max channel numbers for analog cards
    /// </summary>
    public int MaxChannel
    {
      get
      {
        int minChannel, maxChannel;
        IAMTVTuner tvTuner = _filterTvTuner as IAMTVTuner;
        tvTuner.ChannelMinMax(out minChannel, out maxChannel);
        return maxChannel;
      }
    }
    public int MinChannel
    {
      get
      {
        int minChannel, maxChannel;
        IAMTVTuner tvTuner = _filterTvTuner as IAMTVTuner;
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
  }
}

