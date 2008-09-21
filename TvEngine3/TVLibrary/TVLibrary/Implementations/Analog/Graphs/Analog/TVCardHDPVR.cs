/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.SBE;
using Microsoft.Win32;
using TvDatabase;
using TvLibrary.ChannelLinkage;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Helper;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Implementations;
using TvLibrary.Implementations.Analog.QualityControl;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Implementations.Helper;
using TvLibrary.Teletext;

namespace TvLibrary.Implementations.Analog
{
  /// <summary>
  /// Class for handling HDPVR
  /// </summary>
  public class TvCardHDPVR : TvCardBase, ITVCard
  {
    #region imports

    [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
    class MpTsAnalyzer { }

    #endregion

    #region constants

    private static readonly Guid WDMStreamingCrossbarDevices = new Guid("a799a801-a46d-11d0-a18c-00a02401dcd4");
    private static readonly Guid WDMStreamingCaptureDevices = new Guid("65e8773d-8f56-11d0-a3b9-00a0c9223196");

    #endregion

    #region variables
    private IFilterGraph2 _graphBuilder = null;
    private DsROTEntry _rotEntry = null;
    private ICaptureGraphBuilder2 _capBuilder;
    private DsDevice _captureDevice = null;
    private IBaseFilter _filterCrossBar = null;
    private IBaseFilter _filterCapture = null;
    private IBaseFilter _filterEncoder = null;
    private IBaseFilter _filterTsWriter;
    private Configuration _configuration;
    private int _cardId;
    #endregion

    #region ctor

    public TvCardHDPVR(DsDevice device)
      : base(device)
    {
      _mapSubChannels = new Dictionary<Int32, BaseSubChannel>();
      _supportsSubChannels = true;
      _minChannel = 0;
      _maxChannel = 128;
      _camType = CamType.Default;
      _conditionalAccess = null;
      _cardType = CardType.Analog;
      _epgGrabbing = false;
    }

    #endregion

    #region public methods

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {
      if ((channel as AnalogChannel) == null || channel.IsRadio)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Stops the current graph
    /// </summary>
    /// <returns></returns>
    public override void StopGraph()
    {
      if (!CheckThreadId()) return;
      FreeAllSubChannels();
      FilterState state;
      if (_graphBuilder == null) return;
      (_graphBuilder as IMediaControl).GetState(10, out state);
      Log.Log.WriteFile("HDPVR: StopGraph state:{0}", state);
      _isScanning = false;
      int hr = 0;
      if (state == FilterState.Stopped)
      {
        _graphState = GraphState.Created;
        return;
      }
      hr = (_graphBuilder as IMediaControl).Stop();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("HDPVR: RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to stop graph");
      }
      Log.Log.WriteFile("HDPVR: Graph stopped");
    }
    #endregion

    #region Channel linkage handling

    /// <summary>
    /// Starts scanning for linkage info
    /// </summary>
    public void StartLinkageScanner(BaseChannelLinkageScanner callback)
    {
    }

    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    public void ResetLinkageScanner()
    {
    }

    /// <summary>
    /// Returns the channel linkages grabbed
    /// </summary>
    public List<PortalChannel> ChannelLinkages
    {
      get
      {
        return null;
      }
    }

    #endregion

    #region epg & scanning

    /// <summary>
    /// Grabs the epg.
    /// </summary>
    /// <param name="callback">The callback which gets called when epg is received or canceled.</param>
    public void GrabEpg(BaseEpgGrabber callback)
    {
    }

    /// <summary>
    /// Start grabbing the epg while timeshifting
    /// </summary>
    public void GrabEpg()
    {
    }

    /// <summary>
    /// Aborts grabbing the epg. This also triggers the OnEpgReceived callback.
    /// </summary>
    public void AbortGrabbing()
    {
    }

    /// <summary>
    /// returns a list of all epg data for each channel found.
    /// </summary>
    /// <value>The epg.</value>
    public List<EpgChannel> Epg
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    public ITVScanning ScanningInterface
    {
      get
      {
        /*
        if (!CheckThreadId()) return null;
        return new AnalogScanning(this);
        */
        return null;    //todo
      }
    }

    #endregion

    #region tuning & recording

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("HDPVR: Tune:{0}, {1}", subChannelId, channel);
      if (_graphState == GraphState.Idle)
      {
        BuildGraph();
      }
      BaseSubChannel subChannel;
      if (_mapSubChannels.ContainsKey(subChannelId))
      {
        subChannel = _mapSubChannels[subChannelId];
      }
      else
      {
        subChannelId = getNewSubChannel(channel);
        subChannel = _mapSubChannels[subChannelId];
      }
      runGraph(subChannelId);
      subChannel.CurrentChannel = channel;
      subChannel.OnBeforeTune();
      PerformTuning(channel);
      subChannel.OnAfterTune();
      return subChannel;
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Allocates a new instance of HDPVRChannel which handles the new subchannel
    /// </summary>
    /// <returns>handle for to the subchannel</returns>
    private Int32 getNewSubChannel(IChannel channel)
    {
      int id = _subChannelId++;
      Log.Log.Info("HDPVR :getNewSubChannel");
      HDPVRChannel subChannel = new HDPVRChannel(this, id, channel, _filterTsWriter);
      _mapSubChannels[id] = subChannel;
      return id;
    }

    #endregion

    #region quality control

    /// <summary>
    /// Get/Set the quality
    /// </summary>
    public IQuality Quality
    {
      get
      {
        //TODO
        return null;
      }
      set
      {
      }
    }

    /// <summary>
    /// Property which returns true if card supports quality control
    /// </summary>
    public bool SupportsQualityControl
    {
      get
      {
        //TODO
        return false;
      }
    }

    /// <summary>
    /// Reloads the quality control configuration
    /// </summary>
    public void ReloadQualityControlConfiguration()
    {
    }

    #endregion

    #region properties

    /// <summary>
    /// When the tuner is locked onto a signal this property will return true
    /// otherwise false
    /// </summary>
    protected override void UpdateSignalQuality()
    {
      TimeSpan ts = DateTime.Now - _lastSignalUpdate;
      if (ts.TotalMilliseconds < 5000 || _graphState == GraphState.Idle)
      {
        _tunerLocked = false;
      }
      else
      {
        _tunerLocked = true;
      }
      if (_tunerLocked)
      {
        _signalLevel = 100;
        _signalQuality = 100;
      }
      else
      {
        _signalLevel = 0;
        _signalQuality = 0;
      }
    }

    /// <summary>
    /// Gets or sets the unique id of this card
    /// </summary>
    public int CardId
    {
      get
      {
        return _cardId;
      }
      set
      {
        _cardId = value;
        _configuration = Configuration.readConfiguration(_cardId, _name, _devicePath);
        Configuration.writeConfiguration(_configuration);
      }
    }

    #endregion

    #region Disposable
    /// <summary>
    /// Disposes this instance.
    /// </summary>
    virtual public void Dispose()
    {
      if (_graphBuilder == null) return;
      Log.Log.WriteFile("HDPVR:Dispose()");
      if (!CheckThreadId()) return;
      if (_graphState == GraphState.TimeShifting || _graphState == GraphState.Recording)
      {
        // Stop the graph first. To ensure that the timeshift files are no longer blocked
        StopGraph();
      }
      FreeAllSubChannels();
      // Decompose the graph
      int hr = (_graphBuilder as IMediaControl).Stop();
      FilterGraphTools.RemoveAllFilters(_graphBuilder);
      Log.Log.WriteFile("HDPVR:All filters removed");
      if (_filterCrossBar != null)
      {
        while (Marshal.ReleaseComObject(_filterCrossBar) > 0);
        _filterCrossBar = null;
      }
      if (_filterCapture != null)
      {
        while (Marshal.ReleaseComObject(_filterCapture) > 0);
        _filterCapture = null;
      }
      if (_filterEncoder != null)
      {
        while (Marshal.ReleaseComObject(_filterEncoder) > 0);
        _filterEncoder = null;
      }
      if (_filterTsWriter != null)
      {
        while (Marshal.ReleaseComObject(_filterTsWriter) > 0);
        _filterTsWriter = null;
      }
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      Release.ComObject("Graphbuilder", _graphBuilder); _graphBuilder = null;
      DevicesInUse.Instance.Remove(_tunerDevice);
      _graphState = GraphState.Idle;
      Log.Log.WriteFile("HDPVR: dispose completed");
    }

    #endregion

    #region graph handling

    /// <summary>
    /// Builds the directshow graph for this analog tvcard
    /// </summary>
    public override void BuildGraph()
    {
      _lastSignalUpdate = DateTime.MinValue;
      _tunerLocked = false;
      Log.Log.WriteFile("HDPVR: build graph");
      try
      {
        if (_graphState != GraphState.Idle)
        {
          Log.Log.WriteFile("HDPVR: Graph already build");
          throw new TvException("Graph already build");
        }
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        addCrossBarFilter();
        addCaptureFilter();
        addEncoderFilter();
        addTsWriterFilterToGraph();
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

    private void addCrossBarFilter()
    {
      Int32 hr;
      DsDevice[] devices = null;
      IBaseFilter tmp;
      if (!CheckThreadId())
      {
        return;
      }
      //get list of all crossbar devices installed on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(WDMStreamingCrossbarDevices);
      }
      catch (Exception)
      {
        Log.Log.WriteFile("HDPVR: AddCrossBarFilter no crossbar devices found");
        return;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("HDPVR: AddCrossBarFilter no crossbar devices found");
        return;
      }
      //try each crossbar
      for (int i = 0; i < devices.Length; i++)
      {
        Log.Log.WriteFile("HDPVR: AddCrossBarFilter try:{0} {1}", devices[i].Name, i);
        //if crossbar is already in use then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
        {
          continue;
        }
        if (devices[i].Name != "Hauppauge HD PVR Crossbar")
        {
          Log.Log.WriteFile("HDPVR: AddTvCaptureFilter bypassing: {0}", devices[i].Name);
          continue;
        }
        try
        {
          //add the crossbar to the graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("HDPVR: cannot add filter to graph");
          continue;
        }
        if (hr == 0)
        {
          _filterCrossBar = tmp;
          break;
        }
        else
        {
          //failed. try next crossbar
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("CrossBarFilter", tmp);
          }
          continue;
        }
      }
      if (_filterCrossBar == null)
      {
        Log.Log.Error("HDPVR: unable to add crossbar to graph");
        throw new TvException("Unable to add crossbar to graph");
      }
    }

    private void addCaptureFilter()
    {
      DsDevice[] devices = null;
      IBaseFilter tmp;
      Int32 hr;
      if (!CheckThreadId())
      {
        return;
      }
      //get a list of all video capture devices
      try
      {
        devices = DsDevice.GetDevicesOfCat(WDMStreamingCaptureDevices);
      }
      catch (Exception)
      {
        Log.Log.WriteFile("HDPVR: AddTvCaptureFilter no tvcapture devices found");
        return;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("HDPVR: AddTvCaptureFilter no tvcapture devices found");
        return;
      }
      //try each video capture filter
      for (int i = 0; i < devices.Length; i++)
      {
        if (devices[i].Name != "Hauppauge HD PVR Capture Device")
        {
          Log.Log.WriteFile("HDPVR: AddTvCaptureFilter bypassing: {0}", devices[i].Name);
          continue;
        }
        Log.Log.WriteFile("HDPVR: AddTvCaptureFilter try:{0} {1}", devices[i].Name, i);
        // if video capture filter is in use, then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
        {
          continue;
        }
        try
        {
          // add video capture filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("HDPVR: cannot add filter to graph");
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
          Log.Log.WriteFile("HDPVR: AddTvCaptureFilter connected to crossbar successfully");
          //and we're done
          //FilterGraphTools.SaveGraphFile(_graphBuilder, "c:\\tmp.grf");
          break;
        }
        else
        {
          // cannot connect crossbar->video capture filter, remove filter from graph
          // cand continue with the next vieo capture filter
          Log.Log.WriteFile("HDPVR: AddTvCaptureFilter failed to connect to crossbar");
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("capture filter", tmp);
        }
      }
      if (_filterCapture == null)
      {
        Log.Log.Error("HDPVR: unable to add TvCaptureFilter to graph");
        //throw new TvException("Unable to add TvCaptureFilter to graph");
      }
    }

    private bool addEncoderFilter()
    {
      bool finished = false;
      DsDevice[] devices = null;
      IBaseFilter tmp;
      Int32 hr;
      if (!CheckThreadId())
      {
        return false;
      }
      Log.Log.WriteFile("HDPVR: AddEncoderFilter");
      // first get all encoder filters available on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.WDMStreamingEncoderDevices);
      }
      catch (Exception)
      {
        Log.Log.WriteFile("HDPVR: AddTvEncoderFilter no encoder devices found (Exception)");
        return false;
      }
      if (devices == null)
      {
        Log.Log.WriteFile("HDPVR: AddTvEncoderFilter no encoder devices found (devices == null)");
        return false;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("HDPVR: AddTvEncoderFilter no encoder devices found");
        return false;
      }
      //for each encoder
      Log.Log.WriteFile("HDPVR: AddTvEncoderFilter found:{0} encoders", devices.Length);
      for (int i = 0; i < devices.Length; i++)
      {
        //if encoder is in use, we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
        {
          Log.Log.WriteFile("HDPVR:  skip :{0} (inuse)", devices[i].Name);
          continue;
        }
        Log.Log.WriteFile("HDPVR:  try encoder:{0} {1}", devices[i].Name, i);
        try
        {
          //add encoder filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          Log.Log.WriteFile("HDPVR: cannot add filter {0} to graph", devices[i].Name);
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
        if (tmp == null)
        {
          continue;
        }
        hr = _capBuilder.RenderStream(null, null, _filterCapture, null, tmp);
        if (hr == 0)
        {
          // That worked. Since most crossbar devices require 2 connections from
          // crossbar->video capture filter, we do it again to connect the 2nd pin
          hr = _capBuilder.RenderStream(null, null, _filterCapture, null, tmp);
          _filterEncoder = tmp;
          DevicesInUse.Instance.Add(_captureDevice);
          Log.Log.WriteFile("HDPVR: AddTvCaptureFilter connected to crossbar successfully");
          //and we're done
          //FilterGraphTools.SaveGraphFile(_graphBuilder, "c:\\tmp.grf");
          break;
        }
        else
        {
          // cannot connect crossbar->video capture filter, remove filter from graph
          // cand continue with the next vieo capture filter
          Log.Log.WriteFile("HDPVR: AddTvCaptureFilter failed to connect to crossbar");
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("capture filter", tmp);
        }
        if (finished)
        {
          Log.Log.WriteFile("HDPVR: AddTvEncoderFilter succeeded 3");
          return true;
        }
      }
      Log.Log.WriteFile("HDPVR: AddTvEncoderFilter no encoder found");
      return false;
    }

    private void addTsWriterFilterToGraph()
    {
      if (_filterTsWriter == null)
      {
        Log.Log.WriteFile("HDPVR:  Add Mediaportal TsWriter filter");
        _filterTsWriter = (IBaseFilter)new MpTsAnalyzer();
        int hr = _graphBuilder.AddFilter(_filterTsWriter, "MediaPortal Ts Analyzer");
        if (hr != 0)
        {
          Log.Log.Error("HDPVR:  Add main Ts Analyzer returns:0x{0:X}", hr);
          throw new TvException("Unable to add Ts Analyzer filter");
        }
        IBaseFilter encoder;
        _graphBuilder.FindFilterByName("Hauppauge HD PVR Encoder", out encoder);
        IPin pinTee = DsFindPin.ByDirection(encoder, PinDirection.Output, 0);
        if (pinTee == null)
        {
          if (hr != 0)
          {
            Log.Log.Error("HDPVR:  Unable to find pin#2 on inftee filter");
            throw new TvException("unable to find pin#2 on inftee filter");
          }
        }
        IPin pin = DsFindPin.ByDirection(_filterTsWriter, PinDirection.Input, 0);
        if (pin == null)
        {
          if (hr != 0)
          {
            Log.Log.Error("HDPVR:  Unable to find pin on ts analyzer filter");
            throw new TvException("unable to find pin on ts analyzer filter");
          }
        }
        Log.Log.Info("HDPVR:  Render [InfTee]->[TsWriter]");
        hr = _graphBuilder.Connect(pinTee, pin);
        Release.ComObject("pinTsWriterIn", pin);
        if (hr != 0)
        {
          Log.Log.Error("HDPVR:  Unable to connect inftee to analyzer filter :0x{0:X}", hr);
          throw new TvException("unable to connect inftee to analyzer filter");
        }
      }
    }

    /// <summary>
    /// Method which starts the graph
    /// </summary>
    private void runGraph(int subChannel)
    {
      if (!CheckThreadId()) return;
      if (_mapSubChannels.ContainsKey(subChannel))
      {
        _mapSubChannels[subChannel].OnGraphStart();
      }
      FilterState state;
      (_graphBuilder as IMediaControl).GetState(10, out state);
      if (state == FilterState.Running) return;
      Log.Log.WriteFile("HDPVR: RunGraph");
      int hr = 0;
      hr = (_graphBuilder as IMediaControl).Run();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("HDPVR: RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to start graph");
      }
      if (_mapSubChannels.ContainsKey(subChannel))
      {
        _mapSubChannels[subChannel].OnGraphStarted();
      }
    }

    #endregion

    #region private helper
    /// <summary>
    /// Checks the thread id.
    /// </summary>
    /// <returns></returns>
    private bool CheckThreadId()
    {
      return true;
    }

    private void PerformTuning(IChannel channel)
    {
      /*
      UpdatePinVideo(channel.IsTv);
      AnalogChannel analogChannel = channel as AnalogChannel;
      if (analogChannel.IsTv)
      {
          SetFrequencyOverride(analogChannel);
      }
      IAMTVTuner tvTuner = _filterTvTuner as IAMTVTuner;
      if (_previousChannel != null)
      {
          if (_previousChannel.VideoSource != analogChannel.VideoSource)
          {
              SetupCrossBar(analogChannel.VideoSource);
          }
          if (analogChannel.IsRadio != _previousChannel.IsRadio)
          {
              if (analogChannel.IsRadio)
              {
                  Log.Log.WriteFile("HDPVR:  set to FM radio");
                  tvTuner.put_Mode(AMTunerModeType.FMRadio);
              }
              else
              {
                  Log.Log.WriteFile("HDPVR:  set to TV");
                  tvTuner.put_Mode(AMTunerModeType.TV);
              }
          }
          if (analogChannel.Country.Id != _previousChannel.Country.Id)
          {
              tvTuner.put_TuningSpace(analogChannel.Country.Id);
              tvTuner.put_CountryCode(analogChannel.Country.Id);
          }
          if (analogChannel.TunerSource != _previousChannel.TunerSource)
          {
              tvTuner.put_InputType(0, analogChannel.TunerSource);
          }
          if (analogChannel.IsRadio)
          {
              if (analogChannel.Frequency != _previousChannel.Frequency)
              {
                  tvTuner.put_Channel((int)analogChannel.Frequency, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
              }
          }
          else
          {
              if (analogChannel.ChannelNumber != _previousChannel.ChannelNumber)
              {
                  tvTuner.put_Channel(analogChannel.ChannelNumber, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
              }
          }
      }
      else
      {
          if (channel.IsRadio)
          {
              Log.Log.WriteFile("HDPVR:  set to FM radio");
              tvTuner.put_Mode(AMTunerModeType.FMRadio);
          }
          else
          {
              Log.Log.WriteFile("HDPVR:  set to TV");
              tvTuner.put_Mode(AMTunerModeType.TV);
          }
          tvTuner.put_TuningSpace(analogChannel.Country.Id);
          tvTuner.put_CountryCode(analogChannel.Country.Id);
          tvTuner.put_InputType(0, analogChannel.TunerSource);
          if (analogChannel.IsRadio)
          {
              tvTuner.put_Channel((int)analogChannel.Frequency, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
          }
          else
          {
              tvTuner.put_Channel(analogChannel.ChannelNumber, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
          }
          SetupCrossBar(analogChannel.VideoSource);
      }
      int videoFrequency;
      int audioFrequency;
      tvTuner.get_VideoFrequency(out videoFrequency);
      tvTuner.get_AudioFrequency(out audioFrequency);
      _lastSignalUpdate = DateTime.MinValue;
      _tunerLocked = false;
      _previousChannel = analogChannel;
     */
      Log.Log.WriteFile("HDPVR: Tuned to channel {0}", channel.Name);

      if (_graphState == GraphState.Idle)
      {
        _graphState = GraphState.Created;
      }
      _lastSignalUpdate = DateTime.MinValue;
    }

    #endregion

    #region scanning interface

    public IAnalogChanelScan GetChannelScanner()
    {
      return null;
    }

    /// <summary>
    /// Gets the video frequency.
    /// </summary>
    /// <value>The video frequency.</value>
    public int VideoFrequency
    {
      get
      {
        return 0;
      }
    }

    /// <summary>
    /// Gets the audio frequency.
    /// </summary>
    /// <value>The audio frequency.</value>
    public int AudioFrequency
    {
      get
      {
        return 0;
      }
    }

    #endregion

    #region abstract implemented Methods

    protected override void OnScanning()
    {
    }

    protected override void UpdateEpgGrabber(bool value)
    {
    }

    #endregion
  }
}
