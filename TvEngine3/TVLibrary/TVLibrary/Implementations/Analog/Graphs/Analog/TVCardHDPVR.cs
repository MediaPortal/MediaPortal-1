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
using System.Runtime.InteropServices;
using DirectShowLib;
using TvLibrary.ChannelLinkage;
using TvLibrary.Epg;
using TvLibrary.Implementations.Analog.QualityControl;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Helper;

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

    #region variables
    private DsROTEntry _rotEntry;
    private ICaptureGraphBuilder2 _capBuilder;
    private DsDevice _captureDevice;
    private IBaseFilter _filterCrossBar;
    private IBaseFilter _filterCapture;
    private IBaseFilter _filterEncoder;
    private IBaseFilter _filterTsWriter;
    private Configuration _configuration;
    private AnalogChannel _previousChannel;
    private IQuality _qualityControl;
    private int _cardId;
    private Hauppauge _haupPauge;
    #endregion

    #region ctor

    ///<summary>
    /// Constrcutor for the HD PVR device
    ///</summary>
    ///<param name="device">HDPVR encoder device</param>
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
      Log.Log.WriteFile("HDPVR: StopGraph state:{0}", state);
      _isScanning = false;
      if (state == FilterState.Stopped)
      {
        _graphState = GraphState.Created;
        return;
      }
      int hr = mediaCtl.Stop();
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
        return null;
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
        subChannelId = GetNewSubChannel(channel);
        subChannel = _mapSubChannels[subChannelId];
      }
      subChannel.CurrentChannel = channel;
      subChannel.OnBeforeTune();
      PerformTuning(channel);
      subChannel.OnAfterTune();
      RunGraph(subChannel.SubChannelId);
      return subChannel;
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Allocates a new instance of HDPVRChannel which handles the new subchannel
    /// </summary>
    /// <returns>handle for to the subchannel</returns>
    private Int32 GetNewSubChannel(IChannel channel)
    {
      int id = _subChannelId++;
      Log.Log.Info("HDPVR:GetNewSubChannel:{0} #{1}", _mapSubChannels.Count, id);
      HDPVRChannel subChannel = new HDPVRChannel(this, _graphBuilder, id, channel, _filterTsWriter);
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
    /// Reloads the quality control configuration
    /// </summary>
    public void ReloadQualityControlConfiguration()
    {
      if (_qualityControl != null)
      {
        _configuration = Configuration.readConfiguration(_cardId, _name, _devicePath);
        Configuration.writeConfiguration(_configuration);
        _qualityControl.SetConfiguration(_configuration);
      }
    }

    #endregion

    #region properties

    ///<summary>
    ///</summary>
    ///<returns></returns>
    public override bool LockedInOnSignal()
    {
      Log.Log.WriteFile("HDPVR:  LockedInOnSignal ok");
      return true;
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
      _tunerLocked = true;
      _signalLevel = 100;
      _signalQuality = 100;
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
      if (_graphBuilder == null)
        return;
      Log.Log.WriteFile("HDPVR:  Dispose()");
      if (!CheckThreadId())
        return;
      if (_graphState == GraphState.TimeShifting || _graphState == GraphState.Recording)
      {
        // Stop the graph first. To ensure that the timeshift files are no longer blocked
        StopGraph();
      }
      FreeAllSubChannels();
      // Decompose the graph
      IMediaControl mediaCtl = (_graphBuilder as IMediaControl);
      if (mediaCtl == null)
      {
        throw new TvException("Can not convert graphBuilder to IMediaControl");
      }
      // Decompose the graph
      mediaCtl.Stop();
      FilterGraphTools.RemoveAllFilters(_graphBuilder);
      Log.Log.WriteFile("HDPVR:  All filters removed");
      if (_filterCrossBar != null)
      {
        while (Marshal.ReleaseComObject(_filterCrossBar) > 0)
        {
        }
        _filterCrossBar = null;
      }
      if (_filterCapture != null)
      {
        while (Marshal.ReleaseComObject(_filterCapture) > 0)
        {
        }
        _filterCapture = null;
      }
      if (_filterEncoder != null)
      {
        while (Marshal.ReleaseComObject(_filterEncoder) > 0)
        {
        }
        _filterEncoder = null;
      }
      if (_filterTsWriter != null)
      {
        while (Marshal.ReleaseComObject(_filterTsWriter) > 0)
        {
        }
        _filterTsWriter = null;
      }
      _rotEntry.Dispose();
      Release.ComObject("Graphbuilder", _graphBuilder);
      _graphBuilder = null;
      DevicesInUse.Instance.Remove(_tunerDevice);
      _graphState = GraphState.Idle;
      Log.Log.WriteFile("HDPVR:  dispose completed");
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
          Log.Log.WriteFile("HDPVR: graph already built!");
          throw new TvException("Graph already built");
        }
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _rotEntry = new DsROTEntry(_graphBuilder);
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        AddCrossBarFilter();
        AddCaptureFilter();
        AddEncoderFilter();
        AddTsWriterFilterToGraph();
        _qualityControl = QualityControlFactory.createQualityControl(_configuration, _filterEncoder, _filterCapture, null, null);
        if (_qualityControl == null)
        {
          Log.Log.WriteFile("analog: No quality control support found");
          //If a hauppauge analog card, set bitrate to default
          //As the graph is stopped, we don't need to pass in the deviceID
          //However, if we wish to change quality for a live graph, the deviceID must be passed in
          if (_tunerDevice != null && _captureDevice != null)
          {
            if (_captureDevice.Name.Contains("Hauppauge"))
            {
              _haupPauge = new Hauppauge(_filterCapture, string.Empty);
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

        _graphState = GraphState.Created;
      } catch (Exception ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _graphState = GraphState.Idle;
        throw;
      }
    }

    private void AddCrossBarFilter()
    {
      DsDevice[] devices;
      if (!CheckThreadId())
        return;
      Log.Log.WriteFile("HDPVR: add Crossbar Filter");
      //get list of all crossbar devices installed on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
      } catch (Exception)
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
        if (devices[i].Name != "Hauppauge HD PVR Crossbar")
        {
          continue;
        }
        Log.Log.WriteFile("HDPVR: AddCrossBarFilter try:{0}", devices[i].Name);
        //if crossbar is already in use then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
        {
          continue;
        }
        IBaseFilter tmp;
        int hr;
        try
        {
          //add the crossbar to the graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
        {
          Log.Log.WriteFile("HDPVR: cannot add filter to graph");
          continue;
        }
        if (hr == 0)
        {
          _filterCrossBar = tmp;
          break;
        }
        //failed. try next crossbar
        if (tmp != null)
        {
          _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("CrossBarFilter", tmp);
        }
        continue;
      }
      if (_filterCrossBar == null)
      {
        Log.Log.Error("HDPVR: unable to add crossbar to graph");
        throw new TvException("Unable to add crossbar to graph");
      }
    }

    private void AddCaptureFilter()
    {
      DsDevice[] devices;
      if (!CheckThreadId())
        return;
      Log.Log.WriteFile("HDPVR: add Capture Filter");
      //get a list of all video capture devices
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      } catch (Exception)
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
          //Log.Log.WriteFile("HDPVR: AddTvCaptureFilter bypassing: {0}", devices[i].Name);
          continue;
        }
        Log.Log.WriteFile("HDPVR: AddTvCaptureFilter try:{0}", devices[i].Name);
        // if video capture filter is in use, then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
        {
          continue;
        }
        IBaseFilter tmp;
        int hr;
        try
        {
          // add video capture filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
        {
          Log.Log.WriteFile("HDPVR: cannot add filter to graph");
          continue;
        }
        if (hr != 0)
        {
          //cannot add video capture filter to graph, try next one
          if (tmp != null)
          {
            _graphBuilder.RemoveFilter(tmp);
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
          _capBuilder.RenderStream(null, null, _filterCrossBar, null, tmp);
          _filterCapture = tmp;
          _captureDevice = devices[i];
          DevicesInUse.Instance.Add(_captureDevice);
          Log.Log.WriteFile("HDPVR: AddTvCaptureFilter connected to crossbar successfully");
          break;
        }
        // cannot connect crossbar->video capture filter, remove filter from graph
        // cand continue with the next vieo capture filter
        Log.Log.WriteFile("HDPVR: AddTvCaptureFilter failed to connect to crossbar");
        _graphBuilder.RemoveFilter(tmp);
        Release.ComObject("capture filter", tmp);
      }
      if (_filterCapture == null)
      {
        Log.Log.Error("HDPVR: unable to add TvCaptureFilter to graph");
        //throw new TvException("Unable to add TvCaptureFilter to graph");
      }
    }

    private void AddEncoderFilter()
    {
      DsDevice[] devices;
      if (!CheckThreadId())
        return;
      Log.Log.WriteFile("HDPVR: add Encoder Filter");
      // first get all encoder filters available on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.WDMStreamingEncoderDevices);
      } catch (Exception)
      {
        Log.Log.WriteFile("HDPVR: AddTvEncoderFilter no encoder devices found (Exception)");
        return;
      }
      if (devices.Length <= 0)
      {
        Log.Log.WriteFile("HDPVR: AddTvEncoderFilter no encoder devices found");
        return;
      }
      //for each encoder
      for (int i = 0; i < devices.Length; i++)
      {
        if (devices[i].Name != "Hauppauge HD PVR Encoder")
        {
          continue;
        }
        //if encoder is in use, we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
        {
          Log.Log.WriteFile("HDPVR:  skip :{0} (inuse)", devices[i].Name);
          continue;
        }
        Log.Log.WriteFile("HDPVR: AddEncoderFilter try:{0}", devices[i].Name);
        //add encoder filter to graph
        IBaseFilter tmp;
        int hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        if (hr != 0)
        {
          //failed to add filter to graph, continue with the next one
          if (tmp != null)
          {
            _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("TvEncoderFilter", tmp);
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
          _capBuilder.RenderStream(null, null, _filterCapture, null, tmp);
          _filterEncoder = tmp;
          Log.Log.WriteFile("HDPVR: AddTvEncoderFilter connected to capture successfully");
          //and we're done
          break;
        }
        // cannot connect crossbar->video capture filter, remove filter from graph
        // cand continue with the next vieo capture filter
        Log.Log.WriteFile("HDPVR: AddTvEncoderFilter failed to connect to capture");
        _graphBuilder.RemoveFilter(tmp);
        Release.ComObject("TvEncoderFilter", tmp);
      }
      if (_filterEncoder == null)
      {
        Log.Log.WriteFile("HDPVR: AddTvEncoderFilter no encoder found");
      }
    }

    private void AddTsWriterFilterToGraph()
    {
      if (!CheckThreadId())
        return;
      if (_filterTsWriter == null)
      {
        Log.Log.WriteFile("HDPVR: add Mediaportal TsWriter filter");
        _filterTsWriter = (IBaseFilter)new MpTsAnalyzer();
        int hr = _graphBuilder.AddFilter(_filterTsWriter, "MediaPortal Ts Analyzer");
        if (hr != 0)
        {
          Log.Log.Error("HDPVR: add main Ts Analyzer returns:0x{0:X}", hr);
          throw new TvException("Unable to add Ts Analyzer filter");
        }
        IPin pinOut = DsFindPin.ByDirection(_filterEncoder, PinDirection.Output, 0);
        if (pinOut == null)
        {
          Log.Log.Error("HDPVR: Unable to find output pin on the encoder filter");
          throw new TvException("unable to find output pin on the encoder filter");
        }
        IPin pinIn = DsFindPin.ByDirection(_filterTsWriter, PinDirection.Input, 0);
        if (pinIn == null)
        {
          Log.Log.Error("HDPVR:  Unable to find the input pin on ts analyzer filter");
          throw new TvException("Unable to find the input pin on ts analyzer filter");
        }
        //Log.Log.Info("HDPVR: Render [Encoder]->[TsWriter]");
        hr = _graphBuilder.Connect(pinOut, pinIn);
        Release.ComObject("pinTsWriterIn", pinIn);
        if (hr != 0)
        {
          Log.Log.Error("HDPVR:  Unable to connect encoder to ts analyzer filter :0x{0:X}", hr);
          throw new TvException("unable to connect encoder to ts analyzer filter");
        }
        Log.Log.WriteFile("HDPVR: addTsWriterFilterToGraph connected to encoder successfully");
      }
    }

    /// <summary>
    /// Sets up the cross bar.
    /// </summary>
    /// <param name="vmode">The crossbar video mode.</param>
    /// <param name="amode">The crossbar audio mode.</param>
    private void SetupCrossBar(AnalogChannel.VideoInputType vmode, AnalogChannel.AudioInputType amode)
    {
      if (!CheckThreadId())
        return;
      Log.Log.WriteFile("HDPVR: SetupCrossBar:{0}", vmode);
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
        {
          videoOutIndex = i;
        }
        if (connectorType == PhysicalConnectorType.Audio_AudioDecoder)
        {
          audioOutIndex = i;
        }
      }
      int audioLine = 0;
      int audioSPDIF = 0;
      int audioAux = 0;
      int videoCvbsNr = 0;
      int videoSvhsNr = 0;
      int videoYrYbYNr = 0;
      for (int i = 0; i < inputs; ++i)
      {
        int relatedPinIndex;
        PhysicalConnectorType connectorType;
        crossbar.get_CrossbarPinInfo(true, i, out relatedPinIndex, out connectorType);
        Log.Log.Write("HDPVR: crossbar pin:{0} type:{1}", i, connectorType);
        if (connectorType == PhysicalConnectorType.Audio_Line)
        {
          audioLine++;
        }
        if (connectorType == PhysicalConnectorType.Audio_SPDIFDigital)
        {
          audioSPDIF++;
        }
        if (connectorType == PhysicalConnectorType.Audio_AUX)
        {
          audioAux++;
        }
        if (connectorType == PhysicalConnectorType.Video_Composite)
        {
          videoCvbsNr++;
        }
        if (connectorType == PhysicalConnectorType.Video_SVideo)
        {
          videoSvhsNr++;
        }
        if (connectorType == PhysicalConnectorType.Video_YRYBY)
        {
          videoYrYbYNr++;
        }
        int hr;
        switch (vmode)
        {
          case AnalogChannel.VideoInputType.VideoInput1:
            if (connectorType == PhysicalConnectorType.Video_Composite && videoCvbsNr == 1)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (amode == AnalogChannel.AudioInputType.AUXInput1)
            {
              if (connectorType == PhysicalConnectorType.Audio_AUX && audioAux == 1)
              {
                hr = crossbar.Route(audioOutIndex, i);
                Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
              }
            }

            if (amode == AnalogChannel.AudioInputType.LineInput1)
            {
              if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 1)
              {
                hr = crossbar.Route(audioOutIndex, i);
                Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
              }
            }

            if (amode == AnalogChannel.AudioInputType.SPDIFInput1)
            {
              if (connectorType == PhysicalConnectorType.Audio_SPDIFDigital && audioSPDIF == 1)
              {
                hr = crossbar.Route(audioOutIndex, i);
                Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
              }
            }
            break;

          case AnalogChannel.VideoInputType.VideoInput2:
            break;

          case AnalogChannel.VideoInputType.VideoInput3:
            break;

          case AnalogChannel.VideoInputType.SvhsInput1:
            if (connectorType == PhysicalConnectorType.Video_SVideo && videoSvhsNr == 1)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (amode == AnalogChannel.AudioInputType.AUXInput1)
            {
              if (connectorType == PhysicalConnectorType.Audio_AUX && audioAux == 1)
              {
                hr = crossbar.Route(audioOutIndex, i);
                Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
              }
            }
            if (amode == AnalogChannel.AudioInputType.LineInput1)
            {
              if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 1)
              {
                hr = crossbar.Route(audioOutIndex, i);
                Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
              }
            }
            if (amode == AnalogChannel.AudioInputType.SPDIFInput1)
            {
              if (connectorType == PhysicalConnectorType.Audio_SPDIFDigital && audioSPDIF == 1)
              {
                hr = crossbar.Route(audioOutIndex, i);
                Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
              }
            }
            break;

          case AnalogChannel.VideoInputType.SvhsInput2:
            break;

          case AnalogChannel.VideoInputType.SvhsInput3:
            break;

          case AnalogChannel.VideoInputType.RgbInput1:
            break;

          case AnalogChannel.VideoInputType.RgbInput2:
            break;

          case AnalogChannel.VideoInputType.RgbInput3:
            break;

          case AnalogChannel.VideoInputType.YRYBYInput1:
            if (connectorType == PhysicalConnectorType.Video_YRYBY && videoYrYbYNr == 1)
            {
              hr = crossbar.Route(videoOutIndex, i);
              Log.Log.Write("  route input:{0} -> video output result:{1:X}", connectorType, hr);
            }
            if (amode == AnalogChannel.AudioInputType.AUXInput1)
            {
              if (connectorType == PhysicalConnectorType.Audio_AUX && audioAux == 1)
              {
                hr = crossbar.Route(audioOutIndex, i);
                Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
              }
            }
            if (amode == AnalogChannel.AudioInputType.LineInput1)
            {
              if (connectorType == PhysicalConnectorType.Audio_Line && audioLine == 1)
              {
                hr = crossbar.Route(audioOutIndex, i);
                Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
              }
            }
            if (amode == AnalogChannel.AudioInputType.SPDIFInput1)
            {
              if (connectorType == PhysicalConnectorType.Audio_SPDIFDigital && audioSPDIF == 1)
              {
                hr = crossbar.Route(audioOutIndex, i);
                Log.Log.Write("  route input:{0} -> audio output result:{1:X}", connectorType, hr);
              }
            }
            break;

          case AnalogChannel.VideoInputType.YRYBYInput2:
            break;

          case AnalogChannel.VideoInputType.YRYBYInput3:
            break;
        }
      }
    }

    /// <summary>
    /// Method which starts the graph
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
      if (analogChannel == null)
      {
        throw new NullReferenceException();
      }
      if (_previousChannel != null)
      {
        if (_previousChannel.VideoSource != analogChannel.VideoSource)
        {
          SetupCrossBar(analogChannel.VideoSource, analogChannel.AudioSource);
        }
      }
      else
      {
        SetupCrossBar(analogChannel.VideoSource, analogChannel.AudioSource);
      }
      _lastSignalUpdate = DateTime.MinValue;
      _tunerLocked = false;
      _previousChannel = analogChannel;
      Log.Log.WriteFile("HDPVR: Tuned to channel {0}", channel.Name);
      if (_graphState == GraphState.Idle)
      {
        _graphState = GraphState.Created;
      }
      _lastSignalUpdate = DateTime.MinValue;
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
