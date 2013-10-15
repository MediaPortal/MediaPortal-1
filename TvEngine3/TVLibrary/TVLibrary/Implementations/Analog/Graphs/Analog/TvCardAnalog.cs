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
using TvLibrary.ChannelLinkage;
using TvLibrary.Epg;
using TvLibrary.Implementations.Analog.GraphComponents;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Implementations.Analog.Components;
using TvLibrary.Implementations.Analog.QualityControl;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Helper;
using Capture = TvLibrary.Implementations.Analog.Components.Capture;
using Crossbar = TvLibrary.Implementations.Analog.Components.Crossbar;
using Tuner = TvLibrary.Implementations.Analog.Components.Tuner;
using TvAudio = TvLibrary.Implementations.Analog.Components.TvAudio;

namespace TvLibrary.Implementations.Analog
{
  /// <summary>
  /// Class for handling various types of Analog TV Cards
  /// </summary>
  public class TvCardAnalog : TvCardBase, ITVCard
  {
    #region constants

    [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
    private class MpTsAnalyzer {}

    // The MediaPortal TS muxer delivers a DVB stream containing a single
    // service with a fixed service ID. The PMT PID starts at 0x20 and is
    // incremented with each channel change up to the fixed limit 0x90 after
    // which it is reset to 0x20. This is done in order to clearly signal
    // channel change transitions. TsWriter suppresses changes in PMT version
    // so we had to be a bit more radical.
    private const int SERVICE_ID = 1;
    private const int PMT_PID_FIRST = 0x20;
    private const int PMT_PID_MAX = 0x90;

    #endregion

    #region variables

    private Guid _mainComponentCategory = Guid.Empty;
    private int _expectedPmtPid = PMT_PID_FIRST;

    private Tuner _tuner;
    private TvAudio _tvAudio;
    private Crossbar _crossbar;
    private Capture _capture;
    private Encoder _encoder;
    private DsROTEntry _rotEntry;
    private ICaptureGraphBuilder2 _capBuilder;
    private IBaseFilter _tsFileSink;
    private IQuality _qualityControl;
    private Configuration _configuration;

    #endregion

    #region ctor

    ///<summary>
    /// Constructor for the device.
    ///</summary>
    ///<param name="device">The main device component.</param>
    ///<param name="deviceCategory">The filter/device category associated with the device component.</param>
    public TvCardAnalog(DsDevice device, Guid deviceCategory)
      : base(device)
    {
      _parameters = new ScanParameters();
      _mapSubChannels = new Dictionary<int, BaseSubChannel>();
      _supportsSubChannels = true;
      _minChannel = 0;
      _maxChannel = 128;
      _camType = CamType.Default;
      _conditionalAccess = null;
      _cardType = CardType.Analog;
      _epgGrabbing = false;
      _configuration = Configuration.readConfiguration(_cardId, _name, _devicePath);
      Configuration.writeConfiguration(_configuration);
      _mainComponentCategory = deviceCategory;
    }

    #endregion

    #region public methods

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {
      if (!(channel is AnalogChannel))
      {
        return false;
      }
      if (channel.IsRadio)
      {
        if (_mainComponentCategory == FilterCategory.AMKSCrossbar)
        {
          // I think the HD-PVR and Colossus aren't capable of audio-only capture.
          return false;
        }
        if (string.IsNullOrEmpty(_configuration.Graph.Tuner.Name))
        {
          BuildGraph();
        }
        return (_configuration.Graph.Tuner.RadioMode & RadioMode.FM) != 0;
      }
      return true;
    }


    /// <summary>
    /// Pause the current graph
    /// </summary>
    /// <returns></returns>
    public override void PauseGraph()
    {
      if (!CheckThreadId())
        return;
      FreeAllSubChannels();
      FilterState state;
      if (_graphBuilder == null)
        return;
      IMediaControl mediaCtl = (_graphBuilder as IMediaControl);
      if (mediaCtl == null)
      {
        throw new TvException("Can not convert graphBuilder to IMediaControl");
      }
      mediaCtl.GetState(10, out state);

      Log.Log.WriteFile("analog: PauseGraph state:{0}", state);
      _isScanning = false;
      if (state != FilterState.Running)
      {
        _graphState = GraphState.Created;
        return;
      }
      int hr = mediaCtl.Pause();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("analog: PauseGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to pause graph");
      }
      Log.Log.WriteFile("analog: Graph paused");
    }


    /// <summary>
    /// Stops the current graph
    /// </summary>
    /// <returns></returns>
    public override void StopGraph()
    {
      if (!CheckThreadId())
        return;
      FreeAllSubChannels();
      FilterState state;
      if (_graphBuilder == null)
        return;
      IMediaControl mediaCtl = (_graphBuilder as IMediaControl);
      if (mediaCtl == null)
      {
        throw new TvException("Can not convert graphBuilder to IMediaControl");
      }
      mediaCtl.GetState(10, out state);

      Log.Log.WriteFile("analog: StopGraph state:{0}", state);
      _isScanning = false;
      if (state == FilterState.Stopped)
      {
        _graphState = GraphState.Created;
        return;
      }
      int hr = mediaCtl.Stop();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("analog: RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to stop graph");
      }
      Log.Log.WriteFile("analog: Graph stopped");
    }

    #endregion

    #region Channel linkage handling

    /// <summary>
    /// Starts scanning for linkage info
    /// </summary>
    public void StartLinkageScanner(BaseChannelLinkageScanner callback) {}

    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    public void ResetLinkageScanner() {}

    /// <summary>
    /// Returns the channel linkages grabbed
    /// </summary>
    public List<PortalChannel> ChannelLinkages
    {
      get { return null; }
    }

    #endregion

    #region epg & scanning

    /// <summary>
    /// Grabs the epg.
    /// </summary>
    /// <param name="callback">The callback which gets called when epg is received or canceled.</param>
    public void GrabEpg(BaseEpgGrabber callback) {}

    /// <summary>
    /// Start grabbing the epg while timeshifting
    /// </summary>
    public void GrabEpg() {}

    /// <summary>
    /// Aborts grabbing the epg. This also triggers the OnEpgReceived callback.
    /// </summary>
    public void AbortGrabbing() {}

    /// <summary>
    /// returns a list of all epg data for each channel found.
    /// </summary>
    /// <value>The epg.</value>
    public List<EpgChannel> Epg
    {
      get { return null; }
    }

    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    public ITVScanning ScanningInterface
    {
      get
      {
        if (!CheckThreadId())
          return null;
        return new AnalogScanning(this, _tsFileSink as ITsChannelScan);
      }
    }

    #endregion

    #region tuning & recording

    /// <summary>
    /// Scans the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      return Tune(subChannelId, channel);
    }

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("analog:  Tune:{0}", channel);
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
        subChannelId = GetNewSubChannel(channel);
        subChannel = _mapSubChannels[subChannelId];
      }
      subChannel.CurrentChannel = channel;
      subChannel.OnBeforeTune();
      AnalogSubChannel analogSubChannel = subChannel as AnalogSubChannel;
      if (analogSubChannel != null)
      {
        _expectedPmtPid++;
        if (_expectedPmtPid == PMT_PID_MAX)
        {
          _expectedPmtPid = PMT_PID_FIRST;
        }
        analogSubChannel.SetServiceParameters(SERVICE_ID, _expectedPmtPid);
      }
      PerformTuning(channel);
      subChannel.OnAfterTune();
      try
      {
        RunGraph(subChannel.SubChannelId);
      }
      catch (Exception)
      {
        FreeSubChannel(subChannel.SubChannelId);
        throw;
      }
      return subChannel;
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Allocates a new instance of TvDvbChannel which handles the new subchannel
    /// </summary>
    /// <returns>handle for to the subchannel</returns>
    protected int GetNewSubChannel(IChannel channel)
    {
      int id = _subChannelId++;
      Log.Log.Info("analog:GetNewSubChannel:{0} #{1}", _mapSubChannels.Count, id);

      AnalogSubChannel subChannel = new AnalogSubChannel(this, id, _tsFileSink, _graphBuilder, _tvAudio);
      subChannel.Parameters = Parameters;
      subChannel.CurrentChannel = channel;
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
      get { return _qualityControl; }
      set { }
    }

    /// <summary>
    /// Property which returns true if card supports quality control
    /// </summary>
    public bool SupportsQualityControl
    {
      get
      {
        if (_graphState == GraphState.Idle)
        {
          BuildGraph();
        }
        return _qualityControl != null;
      }
    }

    /// <summary>
    /// Reloads the card configuration
    /// </summary>
    public void ReloadCardConfiguration()
    {
      if (_qualityControl != null)
      {
        _configuration = Configuration.readConfiguration(_cardId, _name, _devicePath);
        Configuration.writeConfiguration(_configuration);
        _qualityControl.SetConfiguration(_configuration);
        _capture.SetCaptureConfiguration(_configuration.Graph);
      }
    }

    #endregion

    #region properties

    ///<summary>
    ///</summary>
    ///<returns></returns>
    public override void LockInOnSignal()
    {
      // Capture devices don't have a tuner component to lock.
      if (_tuner == null)
      {
        return;
      }

      bool isLocked = false;
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      while (!isLocked && ts.TotalSeconds < 2)
      {
        _tuner.UpdateSignalQuality();
        isLocked = _tuner.TunerLocked;

        if (!isLocked)
        {
          ts = DateTime.Now - timeStart;
          Log.Log.WriteFile("analog:  LockInOnSignal waiting 20ms");
          System.Threading.Thread.Sleep(20);
        }
      }

      if (!isLocked)
      {
        Log.Log.WriteFile("analog:  LockInOnSignal could not lock onto channel - no signal or bad signal");
        throw new TvExceptionNoSignal("Unable to tune to channel - no signal");
      }
      else
      {
        Log.Log.WriteFile("analog:  LockInOnSignal ok");
      }      
    }

    /// <summary>
    /// When the tuner is locked onto a signal this property will return true
    /// otherwise false
    /// </summary>
    protected override void UpdateSignalQuality(bool force)
    {
      if (!force)
      {
        TimeSpan ts = DateTime.Now - _lastSignalUpdate;
        if (ts.TotalMilliseconds < 5000)
        {
          return;
        }
      }

      if (_graphState == GraphState.Idle)
      {
        _tunerLocked = false;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }

      if (_tuner != null)
      {
        _tuner.UpdateSignalQuality();
        _tunerLocked = _tuner.TunerLocked;
        _signalLevel = _tuner.SignalLevel;
        _signalQuality = _tuner.SignalQuality;
      }
      else
      {
        _tunerLocked = true;
        _signalLevel = 100;
        _signalQuality = 100;
      }
      _lastSignalUpdate = DateTime.Now;
    }

    /// <summary>
    /// When the tuner is locked onto a signal this property will return true
    /// otherwise false
    /// </summary>
    protected override void UpdateSignalQuality()
    {
      UpdateSignalQuality(false);
    }

    /// <summary>
    /// Gets or sets the unique id of this card
    /// </summary>
    public override int CardId
    {
      get { return _cardId; }
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
    public virtual void Dispose()
    {
      if (_graphBuilder == null)
        return;
      Log.Log.WriteFile("analog:Dispose()");
      if (!CheckThreadId())
        return;

      if (_graphState == GraphState.TimeShifting || _graphState == GraphState.Recording)
      {
        // Stop the graph first. To ensure that the timeshift files are no longer blocked
        StopGraph();
      }
      FreeAllSubChannels();
      IMediaControl mediaCtl = (_graphBuilder as IMediaControl);
      if (mediaCtl == null)
      {
        throw new TvException("Can not convert graphBuilder to IMediaControl");
      }
      // Decompose the graph
      mediaCtl.Stop();
      FilterGraphTools.RemoveAllFilters(_graphBuilder);
      Log.Log.WriteFile("analog:All filters removed");
      if (_tuner != null)
      {
        _tuner.Dispose();
        _tuner = null;
        _tunerDevice = null;
      }
      if (_crossbar != null)
      {
        _crossbar.Dispose();
        _crossbar = null;
      }
      if (_tvAudio != null)
      {
        _tvAudio.Dispose();
        _tvAudio = null;
      }
      if (_capture != null)
      {
        _capture.Dispose();
        _capture = null;
      }
      if (_encoder != null)
      {
        _encoder.Dispose();
        _encoder = null;
      }
      if (_tsFileSink != null)
      {
        Release.ComObject("tsFileSink filter", _tsFileSink);
        _tsFileSink = null;
      }
      _rotEntry.Dispose();
      _rotEntry = null;
      Release.ComObject("Graphbuilder", _graphBuilder);
      _graphBuilder = null;
      _graphState = GraphState.Idle;
      Log.Log.WriteFile("analog: dispose completed");
    }

    public void CancelTune(int subChannel)
    {
    }

    public event OnNewSubChannelDelegate OnNewSubChannelEvent;

    #endregion

    #region graph handling

    /// <summary>
    /// Builds the directshow graph for this analog tvcard
    /// </summary>
    public override void BuildGraph()
    {
      if (_cardId == 0)
      {
        GetPreloadBitAndCardId();
        _configuration = Configuration.readConfiguration(_cardId, _name, _devicePath);
        Configuration.writeConfiguration(_configuration);
      }
      _lastSignalUpdate = DateTime.MinValue;
      _tunerLocked = false;
      Log.Log.WriteFile("analog: build graph");
      try
      {
        if (_graphState != GraphState.Idle)
        {
          throw new TvException("Graph already built");
        }
        //create a new filter graph
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _rotEntry = new DsROTEntry(_graphBuilder);
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        Graph graph = _configuration.Graph;
        if (_mainComponentCategory == FilterCategory.AMKSTVTuner)
        {
          _tuner = new Tuner(_device);
          if (!_tuner.CreateFilterInstance(graph, _graphBuilder))
          {
            throw new TvException("Analog: unable to add tv tuner filter");
          }
          _minChannel = _tuner.MinChannel;
          _maxChannel = _tuner.MaxChannel;
        }
        //add the wdm crossbar device and connect tvtuner->crossbar
        _crossbar = new Crossbar(_device);
        if (!_crossbar.CreateFilterInstance(graph, _graphBuilder, _tuner))
        {
          throw new TvException("Analog: unable to add tv crossbar filter");
        }
        if (_mainComponentCategory == FilterCategory.AMKSTVTuner)
        {
          //add the tv audio tuner device and connect it to the crossbar
          _tvAudio = new TvAudio();
          if (!_tvAudio.CreateFilterInstance(graph, _graphBuilder, _tuner, _crossbar))
          {
            throw new TvException("Analog: unable to add tv audio tuner filter");
          }
        }
        //add the tv capture device and connect it to the crossbar
        _capture = new Capture();
        if (!_capture.CreateFilterInstance(graph, _capBuilder, _graphBuilder, _tuner, _crossbar, _tvAudio))
        {
          throw new TvException("Analog: unable to add capture filter");
        }
        Configuration.writeConfiguration(_configuration);
        _encoder = new Encoder();
        if (!_encoder.CreateFilterInstance(_graphBuilder, _capture))
        {
          throw new TvException("Analog: unable to add encoder filter(s)");
        }
        Log.Log.WriteFile("analog: Check quality control");
        _qualityControl = QualityControlFactory.createQualityControl(_configuration, _encoder.VideoEncoderFilter,
                                                                     _capture.VideoFilter, _encoder.MultiplexerFilter);
        if (_qualityControl == null)
        {
          Log.Log.WriteFile("analog: No quality control support found");
          //If a hauppauge analog card, set bitrate to default
          //As the graph is stopped, we don't need to pass in the deviceID
          //However, if we wish to change quality for a live graph, the deviceID must be passed in
          if (_tunerDevice != null && _capture.VideoFilter != null)
          {
            if (_capture.VideoName.Contains("Hauppauge"))
            {
              Hauppauge _hauppauge = new Hauppauge(_capture.VideoFilter, string.Empty);
              _hauppauge.SetStream(103);
              _hauppauge.SetAudioBitRate(384);
              _hauppauge.SetVideoBitRate(6000, 8000, true);
              int min, max;
              bool vbr;
              _hauppauge.GetVideoBitRate(out min, out max, out vbr);
              Log.Log.Write("Hauppauge set video parameters - Max kbps: {0}, Min kbps: {1}, VBR {2}", max, min, vbr);
              _hauppauge.Dispose();
              _hauppauge = null;
            }
          }
        }

        if (!AddAndConnectTsWriter())
        {
          throw new TvException("Analog: unable to add mpfilewriter");
        }
        Log.Log.WriteFile("analog: Graph is built");
        FilterGraphTools.SaveGraphFile(_graphBuilder, "analog.grf");
        ReloadCardConfiguration();
        _graphState = GraphState.Created;
      }
      catch (TvExceptionSWEncoderMissing ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _graphState = GraphState.Idle;
        throw;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _graphState = GraphState.Idle;
        throw new TvExceptionGraphBuildingFailed("Graph building failed", ex);
      }
    }

    /// <summary>
    /// Add and connect the MediaPortal TS writer/analyser filter into the graph.
    /// </summary>
    /// <returns></returns>
    private bool AddAndConnectTsWriter()
    {
      Log.Log.Debug("analog: add and connect TsWriter");
      _tsFileSink = (IBaseFilter)new MpTsAnalyzer();
      int hr = 0;
      try
      {
        hr = _graphBuilder.AddFilter(_tsFileSink, "MediaPortal TS Writer");
        DsError.ThrowExceptionForHR(hr);
      }
      catch
      {
        Log.Log.Debug("analog: failed to add, hr = 0x{0:x}", hr);
        return false;
      }
      
      IPin outputPin = DsFindPin.ByDirection(_encoder.TsMultiplexerFilter, PinDirection.Output, 0);
      IPin inputPin = DsFindPin.ByDirection(_tsFileSink, PinDirection.Input, 0);
      try
      {
        hr = _graphBuilder.ConnectDirect(outputPin, inputPin, null);
        DsError.ThrowExceptionForHR(hr);
      }
      finally
      {
        if (outputPin != null)
        {
          Release.ComObject(outputPin);
        }
        if (inputPin != null)
        {
          Release.ComObject(inputPin);
        }
      }
      return true;
    }

    /// <summary>
    /// Methods which starts the graph
    /// </summary>
    public override void RunGraph(int subChannel)
    {
      bool graphRunning = GraphRunning();

      if (!CheckThreadId())
      {
        return;
      }
      if (_mapSubChannels.ContainsKey(subChannel))
      {
        if (graphRunning)
        {
          LockInOnSignal();          
        }
        _mapSubChannels[subChannel].AfterTuneEvent -= new BaseSubChannel.OnAfterTuneDelegate(OnAfterTuneEvent);
        _mapSubChannels[subChannel].AfterTuneEvent += new BaseSubChannel.OnAfterTuneDelegate(OnAfterTuneEvent);
        _mapSubChannels[subChannel].OnGraphStart();
      }

      if (graphRunning)
      {
        return;
      }

      Log.Log.WriteFile("analog: RunGraph");
      int hr = 0;
      IMediaControl mediaCtrl = _graphBuilder as IMediaControl;
      if (mediaCtrl == null)
      {
        Log.Log.WriteFile("analog: RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to start graph");
      }
      hr = mediaCtrl.Run();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("analog: RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to start graph");
      }
      GraphRunning();
      Log.Log.WriteFile("analog: RunGraph succeeded");
      if (!_mapSubChannels.ContainsKey(subChannel))
      {
        return;
      }
      LockInOnSignal();
      _mapSubChannels[subChannel].AfterTuneEvent -= new BaseSubChannel.OnAfterTuneDelegate(OnAfterTuneEvent);
      _mapSubChannels[subChannel].AfterTuneEvent += new BaseSubChannel.OnAfterTuneDelegate(OnAfterTuneEvent);
      _mapSubChannels[subChannel].OnGraphStarted();
    }

    #endregion

    #region private helper

    /// <summary>
    /// Checks the thread id.
    /// </summary>
    /// <returns></returns>
    private static bool CheckThreadId()
    {
      return true;
    }

    private void PerformTuning(IChannel channel)
    {
      AnalogChannel analogChannel = channel as AnalogChannel;
      if (_tuner != null)
      {
        _tuner.PerformTune(analogChannel);
        _minChannel = _tuner.MinChannel;
        _maxChannel = _tuner.MaxChannel;
      }
      _crossbar.PerformTune(analogChannel);
      _capture.PerformTune(analogChannel);
      _encoder.PerformTune(analogChannel);
      _lastSignalUpdate = DateTime.MinValue;
      if (_graphState == GraphState.Idle)
        _graphState = GraphState.Created;
      UpdateSignalQuality(true);
      _lastSignalUpdate = DateTime.MinValue;
    }

    #endregion

    #region scanning interface

    /// <summary>
    /// Gets the video frequency.
    /// </summary>
    /// <value>The video frequency.</value>
    public int VideoFrequency
    {
      get
      {
        if (_tuner != null)
        {
          return _tuner.VideoFrequency;
        }
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
        if (_tuner != null)
        {
          return _tuner.AudioFrequency;
        }
        return 0;
      }
    }

    #endregion

    #region abstract implemented Methods

    /// <summary>
    /// A derrived class should activate / deactivate the scanning
    /// </summary>
    protected override void OnScanning() {}

    /// <summary>
    /// A derrived class should activate / deactivate the epg grabber
    /// </summary>
    /// <param name="value">Mode</param>
    protected override void UpdateEpgGrabber(bool value) {}

    #endregion
  }
}