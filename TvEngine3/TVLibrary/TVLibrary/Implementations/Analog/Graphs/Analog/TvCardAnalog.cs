/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using Capture=TvLibrary.Implementations.Analog.Components.Capture;
using Crossbar=TvLibrary.Implementations.Analog.Components.Crossbar;
using Tuner=TvLibrary.Implementations.Analog.Components.Tuner;
using TvAudio=TvLibrary.Implementations.Analog.Components.TvAudio;

namespace TvLibrary.Implementations.Analog
{
  /// <summary>
  /// Class for handling various types of Analog TV Cards
  /// </summary>
  public class TvCardAnalog : TvCardBase, ITVCard
  {
    #region imports
    [ComImport, Guid("DB35F5ED-26B2-4A2A-92D3-852E145BF32D")]
    private class MpFileWriter { }
    #endregion

    #region variables

    private Tuner _tuner;
    private TvAudio _tvAudio;
    private Crossbar _crossbar;
    private Capture _capture;
    private Encoder _encoder;
    private TeletextComponent _teletext;
    private DsROTEntry _rotEntry;
    private ICaptureGraphBuilder2 _capBuilder;
    private IBaseFilter _tsFileSink;
    private Hauppauge _haupPauge;
    private IQuality _qualityControl;
    private Configuration _configuration;
    private int _cardId;
    #endregion

    #region ctor
    ///<summary>
    /// Constrcutor for the analog
    ///</summary>
    ///<param name="device">Tuner Device</param>
    public TvCardAnalog(DsDevice device)
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
    }
    #endregion

    #region public methods

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {
      if ((channel as AnalogChannel) == null)
        return false;
      if (channel.IsRadio)
      {
        if(_tuner == null)
        {
          BuildGraph();
        }
        return _tuner.SupportsFMRadio;
      }
      return true;
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
      if (_tsFileSink != null)
      {
        IMPRecord record = _tsFileSink as IMPRecord;
        if (record != null)
        {
          record.StopTimeShifting(_subChannelId);
          record.StopRecord(_subChannelId);
        }
      }
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
        if (!CheckThreadId())
          return null;
        return new AnalogScanning(this);
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
      PerformTuning(channel);
      subChannel.OnAfterTune();
      RunGraph(subChannel.SubChannelId);
      _encoder.UpdatePinVideo(channel.IsTv,_graphBuilder);
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

      AnalogSubChannel subChannel = new AnalogSubChannel(this, id, _tvAudio, _capture.SupportsTeletext, _tsFileSink);
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
        return _qualityControl;
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
    public override bool LockedInOnSignal()
    {
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
          Log.Log.WriteFile("analog:  LockedInOnSignal waiting 20ms");
          System.Threading.Thread.Sleep(20);
        }
      }

      if (!isLocked)
      {
        Log.Log.WriteFile("analog:  LockedInOnSignal could not lock onto channel - no signal or bad signal");
      }
      else
      {
        Log.Log.WriteFile("analog:  LockedInOnSignal ok");
      }
      return isLocked;
    }

    /// <summary>
    /// When the tuner is locked onto a signal this property will return true
    /// otherwise false
    /// </summary>
    protected override void UpdateSignalQuality(bool force)
    {
      _tunerLocked = false;
      _signalLevel = 0;
      _signalQuality = 0;
      if (!force)
      {
        TimeSpan ts = DateTime.Now - _lastSignalUpdate;
        if (ts.TotalMilliseconds < 5000 || _graphState == GraphState.Idle)
        {
          _tunerLocked = false;
          return;
        }
      }
      _tuner.UpdateSignalQuality();
      _tunerLocked = _tuner.TunerLocked;
      _signalLevel = _tuner.SignalLevel;
      _signalQuality = _tuner.SignalQuality;
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
    public int CardId
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
    virtual public void Dispose()
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
      _tuner.Dispose();
      _crossbar.Dispose();
      _tvAudio.Dispose();
      _capture.Dispose();
      _encoder.Dispose();
      _teletext.Dispose();
      if (_tsFileSink != null)
      {
        Release.ComObject("tsFileSink filter", _tsFileSink);
        _tsFileSink = null;
      }
      _rotEntry.Dispose();
      Release.ComObject("Graphbuilder", _graphBuilder);
      _graphBuilder = null;
      DevicesInUse.Instance.Remove(_tunerDevice);
      _graphState = GraphState.Idle;
      Log.Log.WriteFile("analog: dispose completed");
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
      Log.Log.WriteFile("analog: build graph");
      try
      {
        if (_graphState != GraphState.Idle)
        {
          Log.Log.WriteFile("analog: Graph already build");
          throw new TvException("Graph already build");
        }
        //create a new filter graph
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _rotEntry = new DsROTEntry(_graphBuilder);
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        Graph graph = _configuration.Graph;
        _tuner = new Tuner(_tunerDevice);
        if (!_tuner.CreateFilterInstance(graph, _graphBuilder))
        {
          Log.Log.Error("analog: unable to add tv tuner filter");
          throw new TvException("Analog: unable to add tv tuner filter");
        }
        _minChannel = _tuner.MinChannel;
        _maxChannel = _tuner.MaxChannel;
        //add the wdm crossbar device and connect tvtuner->crossbar
        _crossbar = new Crossbar();
        if (!_crossbar.CreateFilterInstance(graph,_graphBuilder, _tuner))
        {
          Log.Log.Error("analog: unable to add tv crossbar filter");
          throw new TvException("Analog: unable to add tv crossbar filter");
        }
        //add the tv audio tuner device and connect it to the crossbar
        _tvAudio = new TvAudio();
        if (!_tvAudio.CreateFilterInstance(graph,_graphBuilder, _tuner, _crossbar))
        {
          Log.Log.Error("analog: unable to add tv audio tuner filter");
          throw new TvException("Analog: unable to add tv audio tuner filter");
        }
        //add the tv capture device and connect it to the crossbar
        _capture = new Capture();
        if (!_capture.CreateFilterInstance(graph,_capBuilder, _graphBuilder, _tuner, _crossbar, _tvAudio))
        {
          Log.Log.Error("analog: unable to add capture filter");
          throw new TvException("Analog: unable to add capture filter");
        }
        _teletext = new TeletextComponent();
        if (_capture.SupportsTeletext)
        {
          if (!_teletext.CreateFilterInstance(graph,_graphBuilder, _capture))
          {
            Log.Log.Error("analog: unable to setup teletext filters");
            throw new TvException("Analog: unable to setup teletext filters");
          }
        }
        Configuration.writeConfiguration(_configuration);
        _encoder = new Encoder();
        if(!_encoder.CreateFilterInstance(_graphBuilder,_tuner,_tvAudio,_crossbar,_capture))
        {
          Log.Log.Error("analog: unable to add encoding filter");
          throw new TvException("Analog: unable to add capture filter");
        }
        Log.Log.WriteFile("analog: Check quality control");
        _qualityControl = QualityControlFactory.createQualityControl(_configuration, _encoder.VideoEncoderFilter, _capture.VideoFilter, _encoder.MultiplexerFilter, _encoder.VideoCompressorFilter);
        if (_qualityControl == null)
        {
          Log.Log.WriteFile("analog: No quality control support found");
          //If a hauppauge analog card, set bitrate to default
          //As the graph is stopped, we don't need to pass in the deviceID
          //However, if we wish to change quality for a live graph, the deviceID must be passed in
          if (_tunerDevice != null && _capture.VideoFilter != null)
          {
            if (_capture.VideoCaptureName.Contains("Hauppauge"))
            {
              _haupPauge = new Hauppauge(_capture.VideoFilter, string.Empty);
              _haupPauge.SetStream(103);
              _haupPauge.SetAudioBitRate(384);
              _haupPauge.SetVideoBitRate(6000, 8000, true);
              int min, max;
              bool vbr;
              _haupPauge.GetVideoBitRate(out min, out max, out vbr);
              Log.Log.Write("Hauppauge set video parameters - Max kbps: {0}, Min kbps: {1}, VBR {2}", max, min, vbr);
              _haupPauge.Dispose();
              _haupPauge = null;
            }
          }
        }

        if (!AddTsFileSink())
        {
          throw new TvException("Analog: unable to add mpfilewriter");
        }
        Log.Log.WriteFile("analog: Graph is built");
        _graphState = GraphState.Created;
      } catch(TvExceptionSWEncoderMissing ex)
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

    #region demuxer, muxer and mpfilewriter graph building

    /// <summary>
    /// adds the TsFileSink filter to the graph
    /// </summary>
    /// <returns></returns>
    private bool AddTsFileSink()
    {
      if (!CheckThreadId())
        return false;
      Log.Log.WriteFile("analog:AddTsFileSink");
      _tsFileSink = (IBaseFilter)new MpFileWriter();
      int hr = _graphBuilder.AddFilter(_tsFileSink, "TsFileSink");
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:AddTsFileSink returns:0x{0:X}", hr);
        throw new TvException("Unable to add TsFileSink");
      }
      Log.Log.WriteFile("analog:connect muxer->tsfilesink");
      IPin pin = DsFindPin.ByDirection(_encoder.MultiplexerFilter, PinDirection.Output, 0);
      if (!FilterGraphTools.ConnectPin(_graphBuilder, pin, _tsFileSink, 0))
      {
        Log.Log.WriteFile("analog:unable to connect muxer->tsfilesink");
        throw new TvException("Unable to connect pins");
      }
      Release.ComObject("mpegmux pinin", pin);
      if (_capture.SupportsTeletext)
      {
        Log.Log.WriteFile("analog:connect wst/vbi codec->tsfilesink");
        if (!FilterGraphTools.ConnectPin(_graphBuilder, _teletext.WST_VBI_Pin, _tsFileSink, 1))
        {
          Log.Log.WriteFile("analog:unable to connect wst/vbi->tsfilesink");
          throw new TvException("Unable to connect pins");
        }
      }
      return true;
    }

    #endregion

    /// <summary>
    /// Methods which starts the graph
    /// </summary>
    private void RunGraph(int subChannel)
    {
      bool graphRunning = GraphRunning();

      if (!CheckThreadId())
        return;
      if (_mapSubChannels.ContainsKey(subChannel))
      {
        if (graphRunning)
        {
          if (!LockedInOnSignal())
          {
            throw new TvExceptionNoSignal("Unable to tune to channel - no signal");
          }
        }
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
        return;
      if (!LockedInOnSignal())
      {
        throw new TvExceptionNoSignal("Unable to tune to channel - no signal");
      }
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
      _tuner.PerformTune(analogChannel);
      _minChannel = _tuner.MinChannel;
      _maxChannel = _tuner.MaxChannel;
      _crossbar.PerformTune(analogChannel);
      _capture.PerformTune(analogChannel);
      _lastSignalUpdate = DateTime.MinValue;
      if (_graphState == GraphState.Idle)
        _graphState = GraphState.Created;
      UpdateSignalQuality(true);
      _lastSignalUpdate = DateTime.MinValue;
    }

    #endregion

    #region scanning interface
    ///<summary>
    /// Returns the channel scanner interface
    ///</summary>
    ///<returns></returns>
    public IAnalogChanelScan GetChannelScanner()
    {
      IAnalogChanelScan channelScanner = null;
      if (_tsFileSink != null)
      {
        channelScanner = (IAnalogChanelScan)_tsFileSink;
      }
      return channelScanner;
    }

    /// <summary>
    /// Gets the video frequency.
    /// </summary>
    /// <value>The video frequency.</value>
    public int VideoFrequency
    {
      get
      {
        return _tuner.VideoFrequency;
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
        return _tuner.AudioFrequency;
      }
    }
    #endregion

    #region abstract implemented Methods
    /// <summary>
    /// A derrived class should activate / deactivate the scanning
    /// </summary>
    protected override void OnScanning()
    {
    }
    /// <summary>
    /// A derrived class should activate / deactivate the epg grabber
    /// </summary>
    /// <param name="value">Mode</param>
    protected override void UpdateEpgGrabber(bool value)
    {
    }
    #endregion
  }
}
