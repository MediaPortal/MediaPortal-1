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
using TvLibrary.Implementations.Analog.Components;
using TvLibrary.Implementations.Analog.GraphComponents;
using TvLibrary.Implementations.Analog.QualityControl;
using TvLibrary.Implementations.DVB;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
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
    #region imports

    [ComImport, Guid("DB35F5ED-26B2-4A2A-92D3-852E145BF32D")]
    private class MpFileWriter {}

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
    private IQuality _qualityControl;
    private Configuration _configuration;
    // Maximum and minimum channel numbers that the tuner is physically capable of tuning to.
    private int _minChannel = -1;
    private int _maxChannel = -1;

    #endregion

    #region ctor

    ///<summary>
    /// Constructor for the analog
    ///</summary>
    ///<param name="device">Tuner Device</param>
    public TvCardAnalog(DsDevice device)
      : base(device)
    {
      _supportsSubChannels = true;
      _minChannel = 0;
      _maxChannel = 128;
      _tunerType = CardType.Analog;
      _configuration = Configuration.readConfiguration(_cardId, _name, _devicePath);
      Configuration.writeConfiguration(_configuration);
    }

    #endregion

    #region public methods

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if (!(channel is AnalogChannel))
      {
        return false;
      }
      if (channel.IsRadio)
      {
        if (string.IsNullOrEmpty(_configuration.Graph.Tuner.Name))
        {
          BuildGraph();
        }
        return (_configuration.Graph.Tuner.RadioMode & RadioMode.FM) != 0;
      }
      return true;
    }

    #endregion

    #region scanning

    /// <summary>
    /// Get the device's channel scanning interface.
    /// </summary>
    public override ITVScanning ScanningInterface
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
    public override ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      ITvSubChannel subChannel = base.Tune(subChannelId, channel);
      if (_encoder != null)
      {
        _encoder.UpdatePinVideo(channel.IsTv, _graphBuilder);
      }
      return subChannel;
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="channel">The service or channel to associate with the subchannel.</param>
    /// <returns>a handle for the subchannel</returns>
    protected override int CreateNewSubChannel(IChannel channel)
    {
      int id = _subChannelId++;
      Log.Log.Info("TvCardAnalog: new subchannel, ID = {0}, subchannel count = {1}", id, _mapSubChannels.Count);
      AnalogSubChannel subChannel = new AnalogSubChannel(id, this, _tvAudio, _capture.SupportsTeletext, _tsFileSink);
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
    public override IQuality Quality
    {
      get { return _qualityControl; }
    }

    /// <summary>
    /// Property which returns true if card supports quality control
    /// </summary>
    public override bool SupportsQualityControl
    {
      get
      {
        if (!_isGraphBuilt)
        {
          BuildGraph();
        }
        return _qualityControl != null;
      }
    }

    /// <summary>
    /// Reloads the card configuration
    /// </summary>
    public override void ReloadCardConfiguration()
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

    /// <summary>
    /// Update the tuner signal status statistics.
    /// </summary>
    /// <param name="force"><c>True</c> to force the status to be updated (status information may be cached).</param>
    protected override void UpdateSignalStatus(bool force)
    {
      if (!force)
      {
        TimeSpan ts = DateTime.Now - _lastSignalUpdate;
        if (ts.TotalMilliseconds < 5000)
        {
          return;
        }
      }
      _lastSignalUpdate = DateTime.Now;
      if (!GraphRunning() || _tuner == null)
      {
        _tunerLocked = false;
        _signalPresent = false;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }
      _tuner.UpdateSignalQuality();
      _tunerLocked = _tuner.TunerLocked;
      _signalPresent = _tunerLocked;
      _signalLevel = _tuner.SignalLevel;
      _signalQuality = _tuner.SignalQuality;
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
    public override void Dispose()
    {
      if (_graphBuilder == null)
        return;
      Log.Log.WriteFile("analog:Dispose()");
      if (!CheckThreadId())
        return;

      FreeAllSubChannels();
      IMediaControl mediaCtl = (_graphBuilder as IMediaControl);
      if (mediaCtl == null)
      {
        throw new TvException("Can not convert graphBuilder to IMediaControl");
      }
      // Decompose the graph
      mediaCtl.Stop();

      base.Dispose();

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
      if (_teletext != null)
      {
        _teletext.Dispose();
        _teletext = null;
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
      _isGraphBuilt = false;
      Log.Log.WriteFile("analog: dispose completed");
    }

    #endregion

    #region graph handling

    /// <summary>
    /// Builds the directshow graph for this analog tvcard
    /// </summary>
    public override void BuildGraph()
    {
      if (_cardId == 0)
      {
        _configuration = Configuration.readConfiguration(_cardId, _name, _devicePath);
        Configuration.writeConfiguration(_configuration);
      }
      _lastSignalUpdate = DateTime.MinValue;
      _tunerLocked = false;
      Log.Log.WriteFile("analog: build graph");
      try
      {
        if (_isGraphBuilt)
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
        _tuner = new Tuner(_device);
        if (!_tuner.CreateFilterInstance(graph, _graphBuilder))
        {
          Log.Log.Error("analog: unable to add tv tuner filter");
          throw new TvException("Analog: unable to add tv tuner filter");
        }
        _minChannel = _tuner.MinChannel;
        _maxChannel = _tuner.MaxChannel;
        //add the wdm crossbar device and connect tvtuner->crossbar
        _crossbar = new Crossbar();
        if (!_crossbar.CreateFilterInstance(graph, _graphBuilder, _tuner))
        {
          Log.Log.Error("analog: unable to add tv crossbar filter");
          throw new TvException("Analog: unable to add tv crossbar filter");
        }
        //add the tv audio tuner device and connect it to the crossbar
        _tvAudio = new TvAudio();
        if (!_tvAudio.CreateFilterInstance(graph, _graphBuilder, _tuner, _crossbar))
        {
          Log.Log.Error("analog: unable to add tv audio tuner filter");
          throw new TvException("Analog: unable to add tv audio tuner filter");
        }
        //add the tv capture device and connect it to the crossbar
        _capture = new Capture();
        if (!_capture.CreateFilterInstance(graph, _capBuilder, _graphBuilder, _tuner, _crossbar, _tvAudio))
        {
          Log.Log.Error("analog: unable to add capture filter");
          throw new TvException("Analog: unable to add capture filter");
        }
        Configuration.writeConfiguration(_configuration);
        _teletext = new TeletextComponent();
        if (_capture.SupportsTeletext)
        {
          if (!_teletext.CreateFilterInstance(graph, _graphBuilder, _capture))
          {
            Log.Log.Error("analog: unable to setup teletext filters");
            throw new TvException("Analog: unable to setup teletext filters");
          }
        }
        Configuration.writeConfiguration(_configuration);
        _encoder = new Encoder();
        if (!_encoder.CreateFilterInstance(_graphBuilder, _tuner, _tvAudio, _crossbar, _capture))
        {
          Log.Log.Error("analog: unable to add encoding filter");
          throw new TvException("Analog: unable to add capture filter");
        }
        Log.Log.WriteFile("analog: Check quality control");
        _qualityControl = QualityControlFactory.createQualityControl(_configuration, _encoder.VideoEncoderFilter,
                                                                     _capture.VideoFilter, _encoder.MultiplexerFilter,
                                                                     _encoder.VideoCompressorFilter);
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

        if (!AddTsFileSink())
        {
          throw new TvException("Analog: unable to add mpfilewriter");
        }
        Log.Log.WriteFile("analog: Graph is built");
        FilterGraphTools.SaveGraphFile(_graphBuilder, "analog.grf");
        ReloadCardConfiguration();
        _isGraphBuilt = true;
      }
      catch (TvExceptionSWEncoderMissing ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _isGraphBuilt = false;
        throw;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _isGraphBuilt = false;
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

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      AnalogChannel analogChannel = channel as AnalogChannel;
      _tuner.PerformTune(analogChannel);
      _minChannel = _tuner.MinChannel;
      _maxChannel = _tuner.MaxChannel;
      _crossbar.PerformTune(analogChannel);
      _capture.PerformTune(analogChannel);
      _lastSignalUpdate = DateTime.MinValue;
      UpdateSignalStatus(true);
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
      get { return _tuner.VideoFrequency; }
    }

    /// <summary>
    /// Gets the audio frequency.
    /// </summary>
    /// <value>The audio frequency.</value>
    public int AudioFrequency
    {
      get { return _tuner.AudioFrequency; }
    }

    #endregion
  }
}