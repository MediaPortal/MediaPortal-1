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
  /// Class for handling various types of Analog TV Cards
  /// </summary>
  public class TvCardAnalog : TvCardBase, ITVCard
  {
    #region struct
#pragma warning disable 0649 // All fields are used by the Marshal.PtrToStructure function
    private struct MPEG2VideoInfo		//  MPEG2VideoInfo
    {
      public VideoInfoHeader2 hdr;
      public UInt32 dwStartTimeCode;
      public UInt32 cbSequenceHeader;
      public UInt32 dwProfile;
      public UInt32 dwLevel;
      public UInt32 dwFlags;
      public UInt32 dwSequenceHeader;
    }
#pragma warning restore 0649
    #endregion

    #region constants
    //KSCATEGORY_ENCODER
    private static readonly Guid AMKSEncoder = new Guid("19689BF6-C384-48fd-AD51-90E58C79F70B");
    //STATIC_KSCATEGORY_MULTIPLEXER
    private static readonly Guid AMKSMultiplexer = new Guid("7A5DE1D3-01A1-452c-B481-4FA2B96271E8");
    private static readonly Guid AudioCompressorCategory = new Guid(0x33d9a761, 0x90c8, 0x11d0, 0xbd, 0x43, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);
    private static readonly Guid VideoCompressorCategory = new Guid(0x33d9a760, 0x90c8, 0x11d0, 0xbd, 0x43, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);
    private static readonly Guid LegacyAmFilterCategory = new Guid(0x083863F1, 0x70DE, 0x11d0, 0xBD, 0x40, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);
    private static readonly Guid AMKSMultiplexerSW = new Guid("236C9559-ADCE-4736-BF72-BAB34E392196");
    public static readonly Guid MediaSubtype_Plextor = new Guid(0x30355844, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    #endregion

    #region imports
    [ComImport, Guid("DB35F5ED-26B2-4A2A-92D3-852E145BF32D")]
    private class MpFileWriter { }
    #endregion

    #region variables
    private DsDevice _audioDevice;
    private DsDevice _crossBarDevice;
    private DsDevice _captureDevice;
    private DsDevice _videoEncoderDevice;
    private DsDevice _audioEncoderDevice;
    private DsDevice _multiplexerDevice;    
    private DsROTEntry _rotEntry = null;
    private ICaptureGraphBuilder2 _capBuilder;
    private IBaseFilter _filterTvTuner = null;
    private IBaseFilter _filterTvAudioTuner = null;
    private IBaseFilter _filterCrossBar = null;
    private IBaseFilter _filterCapture = null;
    private IBaseFilter _filterVideoEncoder = null;
    private IBaseFilter _filterAudioEncoder = null;
    private IBaseFilter _filterMultiplexer = null;
    private IBaseFilter _filterMpeg2Demux = null;
    private IBaseFilter _filterGrabber = null;
    private IBaseFilter _filterWstDecoder = null;
    private IBaseFilter _teeSink = null;
    private IBaseFilter _tsFileSink = null;
    private IBaseFilter _filterMpegMuxer;
    private IBaseFilter _filterAnalogMpegMuxer = null;
    private IBaseFilter _filterAudioCompressor = null;
    private IBaseFilter _filterVideoCompressor = null;
    private IPin _pinCapture = null;
    private IPin _pinVideo = null;
    private IPin _pinAudio = null;
    private IPin _pinLPCM = null;
    private IPin _pinVBI = null;
    private IPin _pinAnalogAudio = null;
    private IPin _pinAnalogVideo = null;
    private Hauppauge _haupPauge = null;
    private IAMStreamConfig _interfaceStreamConfigVideoCapture = null;
    private AnalogChannel _previousChannel;
    private bool _isPlextorConvertX = false;
    private bool _pinVideoConnected = false;
    private IQuality _qualityControl = null;
    private Configuration _configuration;
    private int _cardId;
    #endregion

    #region ctor
    public TvCardAnalog(DsDevice device)
      : base(device)
    {
      _parameters = new ScanParameters();
      _previousChannel = null;
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
        if (_graphState == GraphState.Idle)
        {
          BuildGraph();
          // Use Dummy subchannel ID which isn't possible. We need to start the graph and check the abilities of the tuner.
          RunGraph(-1);
        }
        IAMTVTuner tuner = _filterTvTuner as IAMTVTuner;
        AMTunerModeType tunerModes;
        tuner.GetAvailableModes(out tunerModes);
        if ((AMTunerModeType.FMRadio & tunerModes) != 0)
          return true;
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
      (_graphBuilder as IMediaControl).GetState(10, out state);

      Log.Log.WriteFile("analog: StopGraph state:{0}", state);
      _isScanning = false;
      int hr = 0;
      if (_tsFileSink != null)
      {
        IMPRecord record = _tsFileSink as IMPRecord;
        record.StopTimeShifting(_subChannelId);
        record.StopRecord(_subChannelId);
      }
      if (state == FilterState.Stopped)
      {
        _graphState = GraphState.Created;
        return;
      }
      hr = (_graphBuilder as IMediaControl).Stop();
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
      RunGraph(subChannel.SubChannelId);
      subChannel.CurrentChannel = channel;
      subChannel.OnBeforeTune();
      PerformTuning(channel);
      subChannel.OnAfterTune();
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

      AnalogSubChannel subChannel = new AnalogSubChannel(this, id, _filterTvTuner, _filterTvAudioTuner, _pinVBI, _tsFileSink);
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

    public override bool LockedInOnSignal()
    {
      bool isLocked = false;
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      while (!isLocked && ts.TotalSeconds < 2)
      {
        IAMTVTuner tvTuner = _filterTvTuner as IAMTVTuner;
        AMTunerSignalStrength signalStrength;
        tvTuner.SignalPresent(out signalStrength);
        isLocked = (signalStrength == AMTunerSignalStrength.SignalPresent || signalStrength == AMTunerSignalStrength.HasNoSignalStrength);

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
      if (!force)
      {
        TimeSpan ts = DateTime.Now - _lastSignalUpdate;
        if (ts.TotalMilliseconds < 5000 || _graphState == GraphState.Idle)
        {
          _tunerLocked = false;
          return;
        }
      }
      
      IAMTVTuner tvTuner = _filterTvTuner as IAMTVTuner;
      AMTunerSignalStrength signalStrength;
      tvTuner.SignalPresent(out signalStrength);
      _tunerLocked = (signalStrength == AMTunerSignalStrength.SignalPresent || signalStrength == AMTunerSignalStrength.HasNoSignalStrength);
      
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
      _previousChannel = null;
      // Decompose the graph
      int hr = (_graphBuilder as IMediaControl).Stop();
      FilterGraphTools.RemoveAllFilters(_graphBuilder);
      Log.Log.WriteFile("analog:All filters removed");
      if (_filterTvTuner != null)
      {
        while (Marshal.ReleaseComObject(_filterTvTuner) > 0)
          ;
        _filterTvTuner = null;
      }
      if (_filterTvAudioTuner != null)
      {
        while (Marshal.ReleaseComObject(_filterTvAudioTuner) > 0)
          ;
        _filterTvAudioTuner = null;
      }
      if (_filterCapture != null)
      {
        while (Marshal.ReleaseComObject(_filterCapture) > 0)
          ;
        _filterCapture = null;
      }
      if (_filterVideoEncoder != null)
      {
        while (Marshal.ReleaseComObject(_filterVideoEncoder) > 0)
          ;
        _filterVideoEncoder = null;
      }
      if (_filterAudioEncoder != null)
      {
        while (Marshal.ReleaseComObject(_filterAudioEncoder) > 0)
          ;
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
      if (_filterAnalogMpegMuxer != null)
      {
        Release.ComObject("MPEG2 analog mux filter", _filterAnalogMpegMuxer);
        _filterAnalogMpegMuxer = null;
      }
      if (_filterMpegMuxer != null)
      {
        Release.ComObject("MPEG2 mux filter", _filterMpegMuxer);
        _filterMpegMuxer = null;
      }
      if (_tsFileSink != null)
      {
        Release.ComObject("tsFileSink filter", _tsFileSink);
        _tsFileSink = null;
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
      Release.ComObject("Graphbuilder", _graphBuilder);
      _graphBuilder = null;
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
        SetupCaptureFormat();
        // now things get difficult.
        // Here we can have the following situations:
        // 1. we're done, the video capture filter has a mpeg-2 audio output pin
        // 2. we need to add 1 encoder filter which converts both the audio/video output pins
        //    of the video capture filter to mpeg-2
        // 3. we need to potentially mux the mpeg-2 video with audio. i.e. Nvidia NVTV Dual Tuner capture cards.
        // 4. we need to add 2 mpeg-2 encoder filters for software cards. One for audio and one for video 
        //    after the 2 encoder filters, a multiplexer will be added which takes the output of both
        //    encoders and generates mpeg-2

        //situation 1. we look if the video capture device has an mpeg-2 output pin (media type:stream)
        FindCapturePin(MediaType.Stream, MediaSubType.Null);
        //specific workaround for the Plextor COnvertX devices
        if (FilterGraphTools.GetFilterName(_filterTvTuner).Contains("Plextor ConvertX"))
        {
          Log.Log.Info("analog: Plextor ConvertX TV402U detected");
          _isPlextorConvertX = true;
          //fake the capture pin to the Plextor media type & subtype
          FindCapturePin(MediaType.Video, MediaSubtype_Plextor);
          //Find the audio pin
          FindAudioVideoPins();
          //Add the audio encoder
          AddAudioCompressor();
          //Add the Plextor specific InterVideo mux & gets the new capture pin.
          AddInterVideoMuxer();
        }
        if (_pinCapture == null)
        {
          // no it does not. So we have situation 2, 3 or 4 and first need to add 1 or more encoder filters
          // First we try only to add encoders where the encoder pin names are the same as the
          // output pins of the capture filters and we search only for filter which have an mpeg2-program stream output pin
          if (!AddTvEncoderFilter(true, true))
          {
            //if that fails, we try any encoder filter with an mpeg2-program stream output pin
            if (!AddTvEncoderFilter(false, true))
            {
              // If that fails, we try to add encoder where the encoder pin names are the same as the
              // output pins of the capture filters, and now except encoders except with mpeg2-ts output pin
              if (!AddTvEncoderFilter(true, false))
              {
                // If that fails, we try every encoder except encoders except with mpeg2-ts output pin
                AddTvEncoderFilter(false, false);
              }
            }
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
            //for the NVTV filter the pin names dont match .. so check first in bool eval and thus skips 
            // trying AddTvMultiPlexer with matching pinnames when using NVTV
            if (FilterGraphTools.GetFilterName(_filterTvTuner).Contains("NVTV") || !AddTvMultiPlexer(true))
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
            Log.Log.WriteFile("analog:   failed to add audio compressor. you must install a supported audio encoder!");
            throw new Exception("No audio compressor filter found");
          }
          if (!AddVideoCompressor())
          {
            Log.Log.WriteFile("analog:   failed to add video compressor");
            throw new Exception("No video compressor filter found");
          }
          if (FilterGraphTools.GetFilterName(_filterAudioCompressor).Contains("InterVideo Audio Encoder"))
          {
            if (!AddInterVideoMuxer())
            {
              Log.Log.WriteFile("analog:   failed to add intervideo muxer");
              throw new Exception("No intervideo muxer filter found");
            }
          }
          else
          {
            if (!AddAnalogMuxer())
            {
              Log.Log.WriteFile("analog:   failed to add analog muxer");
              throw new Exception("No analog muxer filter found");
            }
          }
        }
        //Certain ATI cards have pin names which don't match etc.
        if (_captureDevice.Name.Contains("ATI AVStream Analog Capture"))
        {
          Log.Log.WriteFile("analog: ATI AVStream Analog Capture card detected adding mux");
          AddTvMultiPlexer(false);
          FindCapturePin(MediaType.Stream, MediaSubType.Mpeg2Program);
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
        //SetupCaptureFormat();
        Log.Log.WriteFile("analog: Check quality control");
        _qualityControl = QualityControlFactory.createQualityControl(_configuration, _filterVideoEncoder, _filterCapture, _filterMultiplexer, _filterVideoCompressor);
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

        //FilterGraphTools.SaveGraphFile(_graphBuilder, "hp.grf");
        if (!AddMpegMuxer())
        {
          throw new TvException("Analog: unable to add mpeg muxer");
        }
        if (!AddTsFileSink())
        {
          throw new TvException("Analog: unable to add mpfilewriter");
        }
        Log.Log.WriteFile("analog: Graph is built");
        _graphState = GraphState.Created;
      } catch (Exception ex)
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
    private void AddTvTunerFilter()
    {
      if (!CheckThreadId())
        return;
      Log.Log.WriteFile("analog: AddTvTunerFilter {0}", _tunerDevice.Name);
      if (DevicesInUse.Instance.IsUsed(_tunerDevice))
        return;
      IBaseFilter tmp;
      int hr;
      try
      {
        hr = _graphBuilder.AddSourceFilterForMoniker(_tunerDevice.Mon, null, _tunerDevice.Name, out tmp);
      } catch (Exception)
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
      UpdateMinMaxChannel();
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
    private void AddTvAudioFilter()
    {
      if (!CheckThreadId())
        return;
      //Log.Log.WriteFile("analog: AddTvAudioFilter");
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
      } catch (Exception)
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
        Log.Log.WriteFile("analog: AddTvAudioFilter try:{0} {1}", devices[i].Name, i);
        //if tv audio tuner is currently in use we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
          continue;
        int hr;
        try
        {
          //add tv audio tuner to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
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
        if (FilterGraphTools.ConnectFilter(_graphBuilder, _filterTvTuner, tmp, devices[i].Name))
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
    private IPin FindCrossBarPin(IBaseFilter crossbarFilter, PhysicalConnectorType connectorType, PinDirection direction)
    {
      if (!CheckThreadId())
        return null;
      //Log.Log.WriteFile("analog: FindCrossBarPin type:{0} direction:{1}", connectorType, direction);
      IAMCrossbar crossbar = crossbarFilter as IAMCrossbar;
      int inputs = 0;
      int outputs = 0;
      crossbar.get_PinCounts(out outputs, out inputs);
      //Log.Log.WriteFile("analog: FindCrossBarPin inputs:{0} outputs:{1}", inputs, outputs);
      int maxPins = inputs;
      if (direction == PinDirection.Output)
        maxPins = outputs;
      for (int i = 0; i < maxPins; ++i)
      {
        int relatedPinIndex;
        PhysicalConnectorType physicalType;
        crossbar.get_CrossbarPinInfo((direction == PinDirection.Input), i, out relatedPinIndex, out physicalType);
        //Log.Log.WriteFile("analog: pin {0} type:{1} ", i, physicalType);
        if (physicalType == connectorType)
        {
          IPin pin = DsFindPin.ByDirection(crossbarFilter, direction, i);
          //Log.Log.WriteFile("analog: FindCrossBarPin found pin at index:{0}", i);
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
    private void AddCrossBarFilter()
    {
      if (!CheckThreadId())
        return;
      //Log.Log.WriteFile("analog: AddCrossBarFilter");
      DsDevice[] devices = null;
      IBaseFilter tmp;
      //get list of all crossbar devices installed on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
        devices = DeviceSorter.Sort(devices, _tunerDevice, _audioDevice, _crossBarDevice, _captureDevice, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
      } catch (Exception)
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
        Log.Log.WriteFile("analog: AddCrossBarFilter try:{0} {1}", devices[i].Name, i);
        //if crossbar is already in use then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
          continue;
        int hr;
        try
        {
          //add the crossbar to the graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
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
    private void AddTvCaptureFilter()
    {
      if (!CheckThreadId())
        return;
      //Log.Log.WriteFile("analog: AddTvCaptureFilter");
      DsDevice[] devices = null;
      IBaseFilter tmp;
      //get a list of all video capture devices
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture); //shouldn't be VideoInputDevice
        devices = DeviceSorter.Sort(devices, _tunerDevice, _audioDevice, _crossBarDevice, _captureDevice, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
      } catch (Exception)
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
        //Don't add NVIDIA DualTV YUV Capture & NVIDIA DualTV YUV Capture 2 filters to graph.
        if (devices[i].Name == "NVIDIA DualTV YUV Capture")
        {
          Log.Log.WriteFile("analog: AddTvCaptureFilter bypassing: {0}", devices[i].Name);
          continue;
        }
        if (devices[i].Name == "NVIDIA DualTV YUV Capture 2")
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
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
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
          Log.Log.WriteFile("analog: AddTvCaptureFilter connected to crossbar successfully");
          //and we're done
          break;
        }
        else
        {
          // cannot connect crossbar->video capture filter, remove filter from graph
          // cand continue with the next vieo capture filter
          Log.Log.WriteFile("analog: AddTvCaptureFilter failed to connect to crossbar");
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("capture filter", tmp);
        }
      }
      if (_filterCapture == null)
      {
        Log.Log.Error("analog: unable to add TvCaptureFilter to graph");
        //throw new TvException("Unable to add TvCaptureFilter to graph");
      }
    }

    /// <summary>
    /// Setups the cross bar.
    /// </summary>
    /// <param name="mode">The crossbar mode.</param>
    private void SetupCrossBar(AnalogChannel.VideoInputType mode)
    {
      if (!CheckThreadId())
        return;
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
        {
          videoOutIndex = i;
        }
        if (connectorType == PhysicalConnectorType.Audio_AudioDecoder)
        {
          audioOutIndex = i;
        }
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
    private void FindCapturePin(Guid mediaType, Guid mediaSubtype)
    {
      if (!CheckThreadId())
        return;
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
        if (fetched != 1)
          break;
        //first check if the pindirection matches
        PinDirection pinDirection;
        pins[0].QueryDirection(out pinDirection);
        if (pinDirection != PinDirection.Output)
          continue;
        //next check if the pin supports the media type requested
        IEnumMediaTypes enumMedia;
        int fetchedMedia;
        AMMediaType[] media = new AMMediaType[2];
        pins[0].EnumMediaTypes(out enumMedia);
        while (true)
        {
          enumMedia.Next(1, media, out fetchedMedia);
          if (fetchedMedia != 1)
            break;
          if (media[0].majorType == mediaType)
          {
            //Log.Log.WriteFile("analog: FindCapturePin major:{0}", media[0].majorType);
            if (media[0].subType == mediaSubtype || media[0].subType == MediaSubType.Mpeg2Program)
            {
              //it does... we're done
              _pinCapture = pins[0];
              //Log.Log.WriteFile("analog: FindCapturePin pin:{0}", FilterGraphTools.LogPinInfo(pins[0]));
              //Log.Log.WriteFile("analog: FindCapturePin   major:{0} sub:{1}", media[0].majorType, media[0].subType);
              Log.Log.WriteFile("analog: FindCapturePin succeeded");
              DsUtils.FreeAMMediaType(media[0]);
              return;
            }
            //Log.Log.WriteFile("analog: FindCapturePin subtype:{0}", media[0].subType);
          }
          DsUtils.FreeAMMediaType(media[0]);
        }
        Release.ComObject("capture pin", pins[0]);
      }
    }

    private object getStreamConfigSetting(IAMStreamConfig streamConfig, string fieldName)
    {
      object returnValue = null;
      try
      {
        if (streamConfig == null)
          throw new NotSupportedException();
        IntPtr pmt = IntPtr.Zero;
        AMMediaType mediaType = new AMMediaType();
        try
        {
          // Get the current format info
          mediaType.formatType = FormatType.VideoInfo2;
          int hr = streamConfig.GetFormat(out mediaType);
          if (hr != 0)
          {
            Log.Log.Info("VideoCaptureDevice:getStreamConfigSetting() FAILED to get:{0} (not supported)", fieldName);
            Marshal.ThrowExceptionForHR(hr);
          }
          // The formatPtr member points to different structures
          // dependingon the formatType
          object formatStruct;
          //Log.Info("  VideoCaptureDevice.getStreamConfigSetting() find formattype"); 
          if (mediaType.formatType == FormatType.WaveEx)
            formatStruct = new WaveFormatEx();
          else if (mediaType.formatType == FormatType.VideoInfo)
            formatStruct = new VideoInfoHeader();
          else if (mediaType.formatType == FormatType.VideoInfo2)
            formatStruct = new VideoInfoHeader2();
          else if (mediaType.formatType == FormatType.Mpeg2Video)
            formatStruct = new MPEG2VideoInfo();
          else if (mediaType.formatType == FormatType.None)
          {
            //Log.Info("VideoCaptureDevice:getStreamConfigSetting() FAILED no format returned");
            //throw new NotSupportedException("This device does not support a recognized format block.");
            return null;
          }
          else
          {
            //Log.Info("VideoCaptureDevice:getStreamConfigSetting() FAILED unknown fmt:{0} {1} {2}", mediaType.formatType, mediaType.majorType, mediaType.subType);
            //throw new NotSupportedException("This device does not support a recognized format block.");
            return null;
          }
          //Log.Info("  VideoCaptureDevice.getStreamConfigSetting() get formatptr");
          // Retrieve the nested structure
          Marshal.PtrToStructure(mediaType.formatPtr, formatStruct);
          // Find the required field
          //Log.Info("  VideoCaptureDevice.getStreamConfigSetting() get field");
          Type structType = formatStruct.GetType();
          FieldInfo fieldInfo = structType.GetField(fieldName);
          if (fieldInfo == null)
          {
            //Log.Info("VideoCaptureDevice.getStreamConfigSetting() FAILED to to find member:{0}", fieldName);
            //throw new NotSupportedException("VideoCaptureDevice:FAILED to find the member '" + fieldName + "' in the format block.");
            return null;
          }
          // Extract the field's current value
          //Log.Info("  VideoCaptureDevice.getStreamConfigSetting() get value");
          returnValue = fieldInfo.GetValue(formatStruct);
          //Log.Info("  VideoCaptureDevice.getStreamConfigSetting() done");	
        }
        finally
        {
          Marshal.FreeCoTaskMem(pmt);
        }
      } catch (Exception)
      {
        Log.Log.Info("  VideoCaptureDevice.getStreamConfigSetting() FAILED ");
      }
      return (returnValue);
    }

    private object setStreamConfigSetting(IAMStreamConfig streamConfig, string fieldName, object newValue)
    {
      try
      {
        object returnValue = null;
        IntPtr pmt = IntPtr.Zero;
        AMMediaType mediaType = new AMMediaType();
        try
        {
          // Get the current format info
          int hr = streamConfig.GetFormat(out mediaType);
          if (hr != 0)
          {
            Log.Log.Info("  VideoCaptureDevice:setStreamConfigSetting() FAILED to set:{0} (getformat) hr:{1}", fieldName, hr);
            return null;//Marshal.ThrowExceptionForHR(hr);
          }
          //Log.Info("  VideoCaptureDevice:setStreamConfigSetting() get formattype");
          // The formatPtr member points to different structures
          // dependingon the formatType
          object formatStruct;
          if (mediaType.formatType == FormatType.WaveEx)
            formatStruct = new WaveFormatEx();
          else if (mediaType.formatType == FormatType.VideoInfo)
            formatStruct = new VideoInfoHeader();
          else if (mediaType.formatType == FormatType.VideoInfo2)
            formatStruct = new VideoInfoHeader2();
          else if (mediaType.formatType == FormatType.Mpeg2Video)
            formatStruct = new MPEG2VideoInfo();
          else if (mediaType.formatType == FormatType.None)
          {
            Log.Log.Info("  VideoCaptureDevice:setStreamConfigSetting() FAILED no format returned");
            return null;// throw new NotSupportedException("This device does not support a recognized format block.");
          }
          else
          {
            Log.Log.Info("  VideoCaptureDevice:setStreamConfigSetting() FAILED unknown fmt");
            return null;//throw new NotSupportedException("This device does not support a recognized format block.");
          }
          //Log.Info("  VideoCaptureDevice.setStreamConfigSetting() get formatptr");
          // Retrieve the nested structure
          Marshal.PtrToStructure(mediaType.formatPtr, formatStruct);
          // Find the required field
          //Log.Info("  VideoCaptureDevice.setStreamConfigSetting() get field");
          Type structType = formatStruct.GetType();
          FieldInfo fieldInfo = structType.GetField(fieldName);
          if (fieldInfo == null)
          {
            Log.Log.Info("  VideoCaptureDevice:setStreamConfigSetting() FAILED to to find member:{0}", fieldName);
            throw new NotSupportedException("FAILED to find the member '" + fieldName + "' in the format block.");
          }
          //Log.Info("  VideoCaptureDevice.setStreamConfigSetting() set value");
          // Update the value of the field
          fieldInfo.SetValue(formatStruct, newValue);
          // PtrToStructure copies the data so we need to copy it back
          Marshal.StructureToPtr(formatStruct, mediaType.formatPtr, false);
          //Log.Info("  VideoCaptureDevice.setStreamConfigSetting() set format");
          // Save the changes
          hr = streamConfig.SetFormat(mediaType);
          if (hr != 0)
          {
            Log.Log.Info("  VideoCaptureDevice:setStreamConfigSetting() FAILED to set:{0} {1}", fieldName, hr);
            return null;//Marshal.ThrowExceptionForHR(hr);
          }
          //else Log.Info("  VideoCaptureDevice.setStreamConfigSetting() set:{0}",fieldName);
          //Log.Info("  VideoCaptureDevice.setStreamConfigSetting() done");
        }
        finally
        {
          Marshal.FreeCoTaskMem(pmt);
        }
        return (returnValue);
      } catch (Exception)
      {
        Log.Log.Info("  VideoCaptureDevice.:setStreamConfigSetting() FAILED ");
      }
      return null;
    }

    private Size GetFrameSize()
    {
      if (_interfaceStreamConfigVideoCapture != null)
      {
        try
        {
          BitmapInfoHeader bmiHeader;
          object obj = getStreamConfigSetting(_interfaceStreamConfigVideoCapture, "BmiHeader");
          if (obj != null)
          {
            bmiHeader = (BitmapInfoHeader)obj;
            return new Size(bmiHeader.Width, bmiHeader.Height);
          }
        } catch (Exception)
        {
        }
      }
      return new Size(720, 576);
    }

    private void SetFrameSize(Size FrameSize)
    {
      if (FrameSize.Width > 0 && FrameSize.Height > 0)
      {
        if (_interfaceStreamConfigVideoCapture != null)
        {
          try
          {
            BitmapInfoHeader bmiHeader;
            object obj = getStreamConfigSetting(_interfaceStreamConfigVideoCapture, "BmiHeader");
            if (obj != null)
            {
              bmiHeader = (BitmapInfoHeader)obj;
              Log.Log.Info("VideoCaptureDevice:change capture Framesize :{0}x{1} ->{2}x{3}", bmiHeader.Width, bmiHeader.Height, FrameSize.Width, FrameSize.Height);
              bmiHeader.Width = FrameSize.Width;
              bmiHeader.Height = FrameSize.Height;
              setStreamConfigSetting(_interfaceStreamConfigVideoCapture, "BmiHeader", bmiHeader);
            }
          } catch (Exception)
          {
            Log.Log.Info("VideoCaptureDevice:FAILED:could not set capture  Framesize to {0}x{1}!", FrameSize.Width, FrameSize.Height);
          }
        }
      }
    }

    private void SetFrameRate(double FrameRate)
    {
      // set the framerate
      if (FrameRate >= 1d && FrameRate < 30d)
      {
        if (_interfaceStreamConfigVideoCapture != null)
        {
          try
          {
            Log.Log.Info("SWGraph:capture FrameRate set to {0}", FrameRate);
            long avgTimePerFrame = (long)(10000000d / FrameRate);
            setStreamConfigSetting(_interfaceStreamConfigVideoCapture, "AvgTimePerFrame", avgTimePerFrame);
            Log.Log.Info("VideoCaptureDevice: capture FrameRate done :{0}", FrameRate);
          } catch (Exception)
          {
            Log.Log.Info("VideoCaptureDevice:captureFAILED:could not set FrameRate to {0}!", FrameRate);
          }
        }
      }
    }

    private void SetupCaptureFormat()
    {
      if (_pinCapture == null)
        return;
      Log.Log.Info("VideoCaptureDevice:get Video stream control interface (IAMStreamConfig)");
      DsGuid cat = new DsGuid(PinCategory.Capture);
      Guid iid = typeof(IAMStreamConfig).GUID;
      object o;
      int hr = _capBuilder.FindInterface(cat, null, (IBaseFilter)_filterCapture, iid, out o);
      if (hr == 0)
      {
        _interfaceStreamConfigVideoCapture = o as IAMStreamConfig;
        if (_interfaceStreamConfigVideoCapture != null)
        {
          SetFrameRate(25d);
          SetFrameSize(new Size(720, 576));
          Size size = GetFrameSize();
          if (size.Width != 720 || size.Height != 576)
          {
            SetFrameSize(new Size(640, 480));
          }
        }
      }
      return;
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
    private bool ConnectEncoderFilter(IBaseFilter filterEncoder, bool isVideo, bool isAudio, bool matchPinNames)
    {
      if (!CheckThreadId())
        return false;
      Log.Log.WriteFile("analog: ConnectEncoderFilter video:{0} audio:{1}", isVideo, isAudio);
      int hr;
      //find the inputs of the encoder. could be 1 or 2 inputs.
      IPin pinInput1 = DsFindPin.ByDirection(filterEncoder, PinDirection.Input, 0);
      IPin pinInput2 = DsFindPin.ByDirection(filterEncoder, PinDirection.Input, 1);
      //log input pins
      if (pinInput1 != null)
        Log.Log.WriteFile("analog:  found pin#0 {0}", FilterGraphTools.LogPinInfo(pinInput1));
      if (pinInput2 != null)
        Log.Log.WriteFile("analog:  found pin#1 {0}", FilterGraphTools.LogPinInfo(pinInput2));
      string pinName1 = FilterGraphTools.GetPinName(pinInput1);
      string pinName2 = FilterGraphTools.GetPinName(pinInput2);
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
          if (pinDir == PinDirection.Input)
            continue;
          //log the pin info...
          Log.Log.WriteFile("analog:  capture pin:{0} {1}", i, FilterGraphTools.LogPinInfo(pins[i]));
          string pinName = FilterGraphTools.GetPinName(pins[i]);
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
            //Log.Log.WriteFile("analog:  ConnectEncoderFilter to Capture {0} failed", pinName2);
          }
        }
      }
      finally
      {
        if (enumPins != null)
          Release.ComObject("ienumpins", enumPins);
        if (pinInput1 != null)
          Release.ComObject("encoder pin0", pinInput1);
        if (pinInput2 != null)
          Release.ComObject("encoder pin1", pinInput2);
        for (int i = 0; i < pinsAvailable; ++i)
        {
          if (pins[i] != null)
            Release.ComObject("capture pin" + i.ToString(), pins[i]);
        }
      }
      Log.Log.Write("analog: ConnectEncoderFilter failed (matchPinNames:{0})", matchPinNames);
      return false;
    }

    /// <summary>
    /// This method tries to connect a multiplexer filter to the encoder filters (or capture filter)
    /// See the remarks in AddTvMultiPlexer() for the possible options
    /// </summary>
    /// <param name="filterMultiPlexer">The multiplexer.</param>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the multiplexer filter should match the pin names of the encoder filter.</param>
    /// <returns>true if multiplexer is connected correctly, otherwise false</returns>
    private bool ConnectMultiplexer(IBaseFilter filterMultiPlexer, bool matchPinNames)
    {
      if (!CheckThreadId())
        return false;
      //Log.Log.WriteFile("analog: ConnectMultiplexer()");
      int hr;
      // get the input pins of the multiplexer filter (can be 1 or 2 input pins)
      IPin pinInput1 = DsFindPin.ByDirection(filterMultiPlexer, PinDirection.Input, 0);
      IPin pinInput2 = DsFindPin.ByDirection(filterMultiPlexer, PinDirection.Input, 1);
      //log the info for each input pin
      if (pinInput1 != null)
        Log.Log.WriteFile("analog:  found pin#0 {0}", FilterGraphTools.LogPinInfo(pinInput1));
      if (pinInput2 != null)
        Log.Log.WriteFile("analog:  found pin#1 {0}", FilterGraphTools.LogPinInfo(pinInput2));
      string pinName1 = FilterGraphTools.GetPinName(pinInput1);
      string pinName2 = FilterGraphTools.GetPinName(pinInput2);
      try
      {
        if (_filterAudioEncoder != null)
          Log.Log.WriteFile("analog: AudioEncoder available");
        if (_filterVideoEncoder != null)
          Log.Log.WriteFile("analog: VideoEncoder available");
        int pinsConnectedOnMultiplexer = 0;
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
              if (pinDir == PinDirection.Input)
                continue;
              //log the pin info
              Log.Log.WriteFile("analog:  capture pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));
              string pinName = FilterGraphTools.GetPinName(pins[i]);
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
                  Log.Log.WriteFile("analog:  connected pin:{0} {1} to pin1:{2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput1));
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
                    Log.Log.WriteFile("analog:  connected pin:{0} {1} to pin2:{2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput2));
                    pinsConnected++;
                  }
                }
              }
              if (FilterGraphTools.GetFilterName(_filterTvTuner).Contains("NVTV") && (pinsConnected == 1) && (_filterVideoEncoder != null))
              {
                Log.Log.WriteFile("analog: ConnectMultiplexer step 1 software audio encoder connected and no need for a software video encoder");
                break;
              }
              else
                if (pinsConnected == 2)
                {
                  //if both pins are connected, we're done..
                  Log.Log.WriteFile("analog: ConnectMultiplexer succeeded at step 1");
                  return true;
                }
                else
                {
                  Log.Log.WriteFile("analog: ConnectMultiplexer no succes yet at step 1 only connected:" + pinsConnected + " pins");
                }
            }
            pinsConnectedOnMultiplexer += pinsConnected;
          }
          finally
          {
            if (enumPins != null)
              Release.ComObject("ienumpins", enumPins);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              if (pins[i] != null)
                Release.ComObject("capture pin" + i.ToString(), pins[i]);
            }
          }
        }
        //if we only have a single video encoder
        if (_filterAudioEncoder == null && _filterVideoEncoder != null)
        {
          //option 1, connect the multiplexer to a single encoder filter
          Log.Log.WriteFile("analog: ConnectMultiplexer to video encoder filter");
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
              if (pinDir == PinDirection.Input)
                continue;
              //log the pin info
              Log.Log.WriteFile("analog:  videoencoder pin:{0} {1}", i, FilterGraphTools.LogPinInfo(pins[i]));
              string pinName = FilterGraphTools.GetPinName(pins[i]);
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
                  Log.Log.WriteFile("analog:  connected pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput1));
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
                  if (hr == 0)
                  {
                    //succeeded
                    Log.Log.WriteFile("analog:  connected pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput2));
                    pinsConnected++;
                  }
                }
              }
              if (pinsConnected == 1)
              {
                // add the already connected pin from the previous step (ConnectMultiplexer to capture filter)
                pinsConnected += pinsConnectedOnMultiplexer;
              }
              if (pinsConnected == 2)
              {
                //succeeded and done...
                Log.Log.WriteFile("analog: ConnectMultiplexer succeeded at step 2");
                return true;
              }
              else
              {
                Log.Log.WriteFile("analog: ConnectMultiplexer no succes yet at step 2 only connected:" + pinsConnected + " pins");
              }
            }
          }
          finally
          {
            if (enumPins != null)
              Release.ComObject("ienumpins", enumPins);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              if (pins[i] != null)
                Release.ComObject("encoder pin" + i.ToString(), pins[i]);
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
              if (pinDir == PinDirection.Input)
                continue;
              //log the pin info
              Log.Log.WriteFile("analog:   videoencoder pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));
              string pinName = FilterGraphTools.GetPinName(pins[i]);
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
                  Log.Log.WriteFile("analog:  connected pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput1));
                  pinsConnected++;
                }
                else
                {
                  Log.Log.WriteFile("Cant connect 0x{0:x}", hr);
                  Log.Log.WriteFile("pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput1));
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
                    Log.Log.WriteFile("analog:  connected pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput2));
                    pinsConnected++;
                  }
                  else
                  {
                    Log.Log.WriteFile("Cant connect 0x{0:x}", hr);
                    Log.Log.WriteFile("pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput2));
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
            if (pinsConnected == 0)// video encoder is not connected, so we fail
            {
              Log.Log.WriteFile("analog: Video not connected to multiplexer (pinsConnected == 0) FAILURE");
              return false;
            }
            Log.Log.WriteFile("analog: (pinsConnected: {0})", pinsConnected);

            if (_filterAudioEncoder != null)
            {
              // for each output pin of the audio encoder filter
              _filterAudioEncoder.EnumPins(out enumPins);
              enumPins.Next(20, pins, out pinsAvailable);
              Log.Log.WriteFile("analog:  audioencoder pins available:{0}", pinsAvailable);
              for (int i = 0; i < pinsAvailable; ++i)
              {
                // check if this is an outpin pin on the audio encoder filter
                PinDirection pinDir;
                pins[i].QueryDirection(out pinDir);
                if (pinDir == PinDirection.Input)
                  continue;
                Log.Log.WriteFile("analog: audioencoder  pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));
                string pinName = FilterGraphTools.GetPinName(pins[i]);
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
          }
          finally
          {
            if (enumPins != null)
              Release.ComObject("ienumpins", enumPins);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              if (pins[i] != null)
                Release.ComObject("audio encoder pin" + i.ToString(), pins[i]);
            }
          }
        }
      }
      finally
      {
        if (pinInput1 != null)
          Release.ComObject("multiplexer pin0", pinInput1);
        if (pinInput2 != null)
          Release.ComObject("multiplexer pin1", pinInput2);
      }
      Log.Log.Error("analog: ConnectMultiplexer failed");
      return false;
    }

    /// <summary>
    /// Adds the multiplexer filter to the graph.
    /// several posibilities
    ///  1. no tv multiplexer needed
    ///  2. tv multiplexer filter which is connected to a single encoder filter
    ///  3. tv multiplexer filter which is connected to two encoder filter (audio/video)
    ///  4. tv multiplexer filter which is connected to the capture filter
    /// at the end this method the graph looks like this:
    /// 
    ///  option 2: single encoder filter
    ///    [                ]----->[                ]      [             ]
    ///    [ capture filter ]      [ encoder filter ]----->[ multiplexer ]
    ///    [                ]----->[                ]      [             ]
    ///
    ///
    ///  option 3: dual encoder filters
    ///    [                ]----->[   video        ]    
    ///    [ capture filter ]      [ encoder filter ]------>[             ]
    ///    [                ]      [                ]       [             ]
    ///    [                ]                               [ multiplexer ]
    ///    [                ]----->[   audio        ]------>[             ]
    ///                            [ encoder filter ]      
    ///                            [                ]
    ///
    ///  option 4: no encoder filter
    ///    [                ]----->[             ]
    ///    [ capture filter ]      [ multiplexer ]
    ///    [                ]----->[             ]
    /// </summary>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the multiplexer filter should match the pin names of the encoder filter.</param>
    /// <returns>true if encoder filters are added, otherwise false</returns>
    private bool AddTvMultiPlexer(bool matchPinNames)
    {
      if (!CheckThreadId())
        return false;
      //Log.Log.WriteFile("analog: AddTvMultiPlexer");
      DsDevice[] devicesHW = null;
      DsDevice[] devicesSW = null;
      DsDevice[] devices = null;
      IBaseFilter tmp;
      //get a list of all multiplexers available on this system
      try
      {
        devicesHW = DsDevice.GetDevicesOfCat(AMKSMultiplexer);
        devicesHW = DeviceSorter.Sort(devicesHW, _tunerDevice, _audioDevice, _crossBarDevice, _captureDevice, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
        // also add the SoftWare Multiplexers in case no compatible HardWare multiplexer is found (NVTV cards)
        if (FilterGraphTools.GetFilterName(_filterTvTuner).Contains("NVTV"))
          devicesSW = DsDevice.GetDevicesOfCat(AMKSMultiplexerSW);// NVTV cards needs a Software Multiplexer
        else
          devicesSW = new DsDevice[0];

        devices = new DsDevice[devicesHW.Length + devicesSW.Length];
        int nr = 0;
        for (int i = 0; i < devicesHW.Length; ++i)
          devices[nr++] = devicesHW[i];
        for (int i = 0; i < devicesSW.Length; ++i)
          devices[nr++] = devicesSW[i];
      } catch (Exception ex)
      {
        Log.Log.WriteFile("analog: AddTvMultiPlexer no multiplexer devices found (Exception) " + ex.Message);
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
        Log.Log.WriteFile("analog: AddTvMultiPlexer try:{0} {1}", devices[i].Name, i);
        // if multiplexer is in use, we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
          continue;
        int hr;
        try
        {
          //add multiplexer to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
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
    ///  several posibilities
    ///  1. no encoder filter needed
    ///  2. single encoder filter with seperate audio/video inputs and 1 (mpeg-2) output
    ///  3. single encoder filter with a mpeg2 program stream input (I2S)
    ///  4. two encoder filters. one for audio and one for video
    ///
    ///  At the end of this method the graph looks like:
    ///
    ///  option 2: one encoder filter, with 2 inputs
    ///    [                ]----->[                ]
    ///    [ capture filter ]      [ encoder filter ]
    ///    [                ]----->[                ]
    ///
    ///
    ///  option 3: one encoder filter, with 1 input
    ///    [                ]      [                ]
    ///    [ capture filter ]----->[ encoder filter ]
    ///    [                ]      [                ]
    ///
    ///
    ///  option 4: 2 encoder filters one for audio and one for video
    ///    [                ]----->[   video        ]
    ///    [ capture filter ]      [ encoder filter ]
    ///    [                ]      [                ]
    ///    [                ]   
    ///    [                ]----->[   audio        ]
    ///                            [ encoder filter ]
    ///                            [                ]
    ///
    /// </summary>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the encoder filter should match the pin names of the capture filter.</param>
    /// <param name="mpeg2ProgramFilter">if set to <c>true</c> than only encoders with an mpeg2 program output pins are accepted</param>
    /// <returns>true if encoder filters are added, otherwise false</returns>
    private bool AddTvEncoderFilter(bool matchPinNames, bool mpeg2ProgramFilter)
    {
      if (!CheckThreadId())
        return false;

      Log.Log.WriteFile("analog: AddTvEncoderFilter - MatchPinNames: {0} - MPEG2ProgramFilter: {1}", matchPinNames, mpeg2ProgramFilter);
      bool finished = false;
      DsDevice[] devices = null;
      IBaseFilter tmp;
      // first get all encoder filters available on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(AMKSEncoder);
        devices = DeviceSorter.Sort(devices, _tunerDevice, _audioDevice, _crossBarDevice, _captureDevice, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
      } catch (Exception)
      {
        Log.Log.WriteFile("analog: AddTvEncoderFilter no encoder devices found (Exception)");
        return false;
      }
      if (devices == null)
      {
        Log.Log.WriteFile("analog: AddTvEncoderFilter no encoder devices found (devices == null)");
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
        Log.Log.WriteFile("analog:  try encoder:{0} {1}", devices[i].Name, i);
        int hr;
        try
        {
          //add encoder filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
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
        if (tmp == null)
          continue;
        // Encoder has been added to the graph
        // Now some cards have 2 encoder types, one for mpeg-2 transport stream and one for
        // mpeg-2 program stream. We dont want the mpeg-2 transport stream !
        // So first we check the output pins...
        // and dont accept filters which have a mpeg-ts output pin..
        // get the output pin
        bool isTsFilter = mpeg2ProgramFilter;
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
                //check if media is mpeg-2 program
                if (mediaTypes[media].majorType == MediaType.Stream &&
                    mediaTypes[media].subType == MediaSubType.Mpeg2Program)
                {
                  isTsFilter = false;
                  break;
                }

                // NVTV dual tuner needs this one to make it work so dont skip it
                if (mediaTypes[media].majorType == MediaType.Video &&
                    mediaTypes[media].subType == new Guid("be626472-fe7c-4a21-9f0b-d8f18b5ab441")) /*MediaSubType.?? */
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
          Log.Log.WriteFile("analog:  filter {0} does not have mpeg-2 ps output or is a mpeg-2 ts filters", devices[i].Name);
          if (tmp != null)
          {
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("TvEncoderFilter", tmp);
            tmp = null;
          }
          continue;
        }
        if (tmp == null)
          continue;
        // get the input pins of the encoder (can be 1 or 2 inputs)
        IPin pin1 = DsFindPin.ByDirection(tmp, PinDirection.Input, 0);
        IPin pin2 = DsFindPin.ByDirection(tmp, PinDirection.Input, 1);
        if (pin1 != null)
          Log.Log.WriteFile("analog: encoder in-pin1:{0}", FilterGraphTools.LogPinInfo(pin1));
        if (pin2 != null)
          Log.Log.WriteFile("analog: encoder in-pin2:{0}", FilterGraphTools.LogPinInfo(pin2));
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
                if (_filterVideoEncoder != null)
                  finished = true;
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
                if (_filterAudioEncoder != null)
                  finished = true;
                tmp = null;
                ;
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
        if (pin1 != null)
          Release.ComObject("encoder pin0", pin1);
        if (pin2 != null)
          Release.ComObject("encoder pin1", pin2);
        pin1 = null;
        pin2 = null;
        if (tmp != null)
        {
          _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("encoder filter", tmp);
          tmp = null;
        }
        if (finished)
        {
          Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded 3");
          return true;
        }
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
    private void FindVBIPin()
    {
      if (!CheckThreadId())
        return;
      Log.Log.WriteFile("analog: FindVBIPin");
      try
      {
        IPin pinVBI = DsFindPin.ByCategory(_filterCapture, PinCategory.VideoPortVBI, 0);
        if (pinVBI != null)
        {
          Log.Log.WriteFile("analog: VideoPortVBI");
          Marshal.ReleaseComObject(pinVBI);
          return;
        }
        pinVBI = DsFindPin.ByCategory(_filterCapture, PinCategory.VBI, 0);
        if (pinVBI != null)
        {
          Log.Log.WriteFile("analog: VBI");
          _pinVBI = pinVBI;
          return;
        }
      } catch (COMException ex)
      {
        if (ex.ErrorCode.Equals(unchecked((Int32)0x80070490)))
        {
          // pin on a NVTV capture filter is named VBI..
          Log.Log.WriteFile("analog: getCategory not supported by collection ? ERROR:0x{0:x} :" + ex.Message, ex.ErrorCode);

          if (_filterCapture == null)
            return;
          Log.Log.WriteFile("analog: find VBI by name");

          IPin pinVBI = DsFindPin.ByName(_filterCapture, "VBI");
          if (pinVBI != null)
          {
            Log.Log.WriteFile("analog: pin named VBI found");
            _pinVBI = pinVBI;
            return;
          };
        }
        else
          throw ex;
      }
      Log.Log.WriteFile("analog: FindVBIPin no vbi pin found");
    }

    /// <summary>
    /// Adds 3 filters to the graph so we can grab teletext
    /// On return the graph looks like this:
    ///
    ///	[							 ]		 [  tee/sink	]			 [	wst or	]			[ MPFile	]
    ///	[	capture			 ]		 [		to			]----->[	vbi 		]---->[ Writer  ]
    ///	[						vbi]---->[	sink			]			 [	codec		]			[					]
    /// </summary>
    private void SetupTeletext()
    {
      if (!CheckThreadId())
        return;
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
          Log.Log.Info("analog:SinkGraphEx.SetupTeletext(): Found WST Codec filter");
          device.Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
          _filterWstDecoder = (IBaseFilter)obj;
          hr = _graphBuilder.AddFilter((IBaseFilter)_filterWstDecoder, device.Name);
          if (hr != 0)
          {
            //failed...
            Log.Log.Error("analog:SinkGraphEx.SetupTeletext(): Unable to add WST Codec filter");
            _graphBuilder.RemoveFilter(_teeSink);
            Marshal.ReleaseComObject(_teeSink);
            _teeSink = _filterWstDecoder = _filterGrabber = null;
            return;
          }
          break;
        }
      }
      //Look for VBI Codec for Vista users as Vista doesn't use WST Codec anymore
      if (_filterWstDecoder == null)
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSMULTIVBICodec);
        foreach (DsDevice device in devices)
          if (device.Name.IndexOf("VBI") >= 0)
          {
            //found it, add it to the graph
            Log.Log.Info("analog:SinkGraphEx.SetupTeletext(): Found VBI Codec filter");
            device.Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
            _filterWstDecoder = (IBaseFilter)obj;
            hr = _graphBuilder.AddFilter((IBaseFilter)_filterWstDecoder, device.Name);
            if (hr != 0)
            {
              //failed...
              Log.Log.Error("analog:SinkGraphEx.SetupTeletext(): Unable to add VBI Codec filter");
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
        Log.Log.Error("analog: unable to find WST Codec or VBI Codec filter");
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
    /// <param name="filter">The filter to find the pin on.</param>
    /// <param name="mediaType">Type of the media.</param>
    /// <param name="mediaSubtype">The media subtype.</param>
    private IPin FindMediaPin(IBaseFilter filter, Guid mediaType, Guid mediaSubtype)
    {
      if (!CheckThreadId())
        return null;
      IEnumPins enumPins;
      filter.EnumPins(out enumPins);
      // loop through all pins
      int pinNr = -1;
      while (true)
      {
        IPin[] pins = new IPin[2];
        int fetched;
        enumPins.Next(1, pins, out fetched);
        if (fetched != 1)
          break;
        //first check if the pindirection matches
        PinDirection pinDirection;
        pins[0].QueryDirection(out pinDirection);
        if (pinDirection != PinDirection.Output)
          continue;
        pinNr++;
        //next check if the pin supports the media type requested
        IEnumMediaTypes enumMedia;
        int fetchedMedia;
        AMMediaType[] media = new AMMediaType[2];
        pins[0].EnumMediaTypes(out enumMedia);
        while (true)
        {
          enumMedia.Next(1, media, out fetchedMedia);
          if (fetchedMedia != 1)
            break;
          if (media[0].majorType == mediaType)
          {
            if (media[0].subType == mediaSubtype || mediaSubtype == MediaSubType.Null)
            {
              //it does... we're done
              Log.Log.WriteFile("analog: FindMediaPin pin:#{0} {1}", pinNr, FilterGraphTools.LogPinInfo(pins[0]));
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
    private bool FindAudioVideoPins()
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
    private bool AddAudioCompressor()
    {
      if (!CheckThreadId())
        return false;
      Log.Log.WriteFile("analog: AddAudioCompressor {0}", FilterGraphTools.LogPinInfo(_pinAnalogAudio));
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(AudioCompressorCategory);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(LegacyAmFilterCategory);
      string[] audioEncoders = new string[] { "InterVideo Audio Encoder", "Ulead MPEG Audio Encoder", "MainConcept MPEG Audio Encoder", "MainConcept Demo MPEG Audio Encoder", "CyberLink Audio Encoder", "CyberLink Audio Encoder(Twinhan)", "Pinnacle MPEG Layer-2 Audio Encoder", "MainConcept (Hauppauge) MPEG Audio Encoder", "NVIDIA Audio Encoder" };
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
        if (audioDevices[i] == null)
          continue;
        Log.Log.WriteFile("analog:  try compressor:{0}", audioDevices[i].Name);
        int hr;
        try
        {
          //add compressor filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(audioDevices[i].Mon, null, audioDevices[i].Name, out tmp);
        } catch (Exception)
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
        if (tmp == null)
          continue;

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
          Log.Log.WriteFile("analog: failed to connect audio pin->audio compressor:{0:X}", hr);
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
    private bool AddVideoCompressor()
    {
      if (!CheckThreadId())
        return false;
      Log.Log.WriteFile("analog: AddVideoCompressor");
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(VideoCompressorCategory);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(LegacyAmFilterCategory);
      string[] videoEncoders = new string[] { "InterVideo Video Encoder", "Ulead MPEG Encoder", "MainConcept MPEG Video Encoder", "MainConcept Demo MPEG Video Encoder", "CyberLink MPEG Video Encoder", "CyberLink MPEG Video Encoder(Twinhan)", "MainConcept (Hauppauge) MPEG Video Encoder", "nanocosmos MPEG Video Encoder", "Pinnacle MPEG 2 Encoder" };
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
        if (videoDevices[i] == null)
          continue;
        Log.Log.WriteFile("analog:  try compressor:{0}", videoDevices[i].Name);
        int hr;
        try
        {
          //add compressor filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(videoDevices[i].Mon, null, videoDevices[i].Name, out tmp);
        } catch (Exception)
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
        if (tmp == null)
          continue;
        // check if this compressor filter has an mpeg audio output pin
        Log.Log.WriteFile("analog:  connect video pin->video compressor");
        IPin pinVideo = DsFindPin.ByDirection(tmp, PinDirection.Input, 0);
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
    private bool AddAnalogMuxer()
    {
      Log.Log.Info("analog:AddAnalogMuxer");
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
        Log.Log.Info("analog:no output pin found on audio compressor");
        throw new TvException("no output pin found on audio compressor");
      }
      if (pinIn == null)
      {
        Log.Log.Info("analog:no input pin found on analog muxer");
        throw new TvException("no input pin found on muxer");
      }
      hr = _graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:unable to connect audio compressor->muxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add unable to connect audio compressor->muxer");
      }
      Log.Log.WriteFile("analog:  connected audio -> muxer");
      // next connect video compressor->muxer
      pinOut = DsFindPin.ByDirection(_filterVideoCompressor, PinDirection.Output, 0);
      pinIn = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Input, 0);
      if (pinOut == null)
      {
        Log.Log.Info("analog:no output pin found on video compressor");
        throw new TvException("no output pin found on video compressor");
      }
      if (pinIn == null)
      {
        Log.Log.Info("analog:no input pin found on analog muxer");
        throw new TvException("no input pin found on muxer");
      }
      hr = _graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:unable to connect video compressor->muxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add unable to connect video compressor->muxer");
      }
      //and finally we have a capture pin...
      Log.Log.WriteFile("analog:  connected video -> muxer");
      _pinCapture = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Output, 0);
      if (_pinCapture == null)
      {
        Log.Log.WriteFile("analog:unable find capture pin");
        throw new TvException("unable find capture pin");
      }
      return true;
    }

    /// <summary>
    /// Adds the InterVideo muxer and connects the compressor to it.
    /// This is the preferred muxer for Plextor cards and others.
    /// It will be used if the InterVideo Audio Encoder is used also.
    /// </summary>
    /// <returns></returns>
    private bool AddInterVideoMuxer()
    {
      IPin pinOut;
      IPin pinIn;
      Log.Log.Info("analog:  using intervideo muxer");
      string muxVideoIn = "video compressor";
      string monikerInterVideoMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{317DDB63-870E-11D3-9C32-00104B3801F7}";
      _filterAnalogMpegMuxer = Marshal.BindToMoniker(monikerInterVideoMuxer) as IBaseFilter;
      int hr = _graphBuilder.AddFilter(_filterAnalogMpegMuxer, "InterVideo MPEG Muxer");
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:  add intervideo muxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add InterVideo Muxer");
      }
      Log.Log.Info("analog:  add intervideo muxer successful");
      // next connect video compressor->muxer
      if (_isPlextorConvertX == true)
      {
        muxVideoIn = "Plextor ConvertX";
        //no video compressor needed with the Plextor device so we use the first capture pin
        pinOut = DsFindPin.ByDirection(_filterCapture, PinDirection.Output, 0);
      }
      else
      {
        pinOut = DsFindPin.ByDirection(_filterVideoCompressor, PinDirection.Output, 0);
      }
      pinIn = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Input, 0);
      if (pinOut == null)
      {
        Log.Log.Info("analog:  no output pin found on {0}", muxVideoIn);
        throw new TvException("no output pin found on video out");
      }
      if (pinIn == null)
      {
        Log.Log.Info("analog:  no input pin found on intervideo muxer");
        throw new TvException("no input pin found on intervideo muxer");
      }
      hr = _graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:  unable to connect {0}-> intervideo muxer returns:0x{1:X}", muxVideoIn, hr);
        throw new TvException("Unable to add unable to connect to video in on intervideo muxer");
      }
      Log.Log.WriteFile("analog:  connected video -> intervideo muxer");
      // next connect audio compressor->muxer
      pinOut = DsFindPin.ByDirection(_filterAudioCompressor, PinDirection.Output, 0);
      pinIn = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Input, 1);
      if (pinOut == null)
      {
        Log.Log.Info("analog:  no output pin found on audio compressor");
        throw new TvException("no output pin found on audio compressor");
      }
      if (pinIn == null)
      {
        Log.Log.Info("analog:  no input pin found on intervideo muxer");
        throw new TvException("no input pin found on intervideo muxer");
      }
      hr = _graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:unable to connect audio compressor->intervideo muxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add unable to connect audio compressor->intervideo muxer");
      }
      Log.Log.WriteFile("analog:  connected audio -> intervideo muxer");
      //and finally we have a capture pin...
      _pinCapture = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Output, 0);
      if (_pinCapture == null)
      {
        Log.Log.WriteFile("analog:unable find capture pin");
        throw new TvException("unable find capture pin");
      }
      return true;
    }
    #endregion

    #region demuxer, muxer and mpfilewriter graph building

    private void AddMpeg2Demultiplexer()
    {
      if (!CheckThreadId())
        return;
      Log.Log.WriteFile("analog: AddMpeg2Demultiplexer");
      if (_filterMpeg2Demux != null)
        return;
      if (_pinCapture == null)
        return;
      int hr = 0;
      _filterMpeg2Demux = (IBaseFilter)new MPEG2Demultiplexer();
      hr = _graphBuilder.AddFilter(_filterMpeg2Demux, "MPEG2 Demultiplexer");
      if (hr != 0)
      {
        Log.Log.WriteFile("analog: AddMPEG2DemuxFilter returns:0x{0:X}", hr);
        throw new TvException("Unable to add MPEG2 demultiplexer");
      }
      Log.Log.WriteFile("analog: connect capture->mpeg2 demux");
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
    /// adds the TsFileSink filter to the graph
    /// </summary>
    /// <returns></returns>
    private bool AddTsFileSink()
    {
      if (!CheckThreadId())
        return false;
      Log.Log.WriteFile("analog:AddTsFileSink");
      _tsFileSink = (IBaseFilter)new MpFileWriter();
      int hr = _graphBuilder.AddFilter((IBaseFilter)_tsFileSink, "TsFileSink");
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:AddTsFileSink returns:0x{0:X}", hr);
        throw new TvException("Unable to add TsFileSink");
      }
      Log.Log.WriteFile("analog:connect muxer->tsfilesink");
      IPin pin = DsFindPin.ByDirection(_filterMpegMuxer, PinDirection.Output, 0);
      if (!FilterGraphTools.ConnectPin(_graphBuilder, pin, (IBaseFilter)_tsFileSink, 0))
      {
        Log.Log.WriteFile("analog:unable to connect muxer->tsfilesink");
      }
      Release.ComObject("mpegmux pinin", pin);
      if (_filterWstDecoder != null)
      {
        Log.Log.WriteFile("analog:connect wst/vbi codec->tsfilesink");
        IPin pinWST_VBI = DsFindPin.ByDirection(_filterWstDecoder, PinDirection.Output, 0);
        if (!FilterGraphTools.ConnectPin(_graphBuilder, pinWST_VBI, (IBaseFilter)_tsFileSink, 1))
        {
          Log.Log.WriteFile("analog:unable to connect wst/vbi->tsfilesink");
        }
        Release.ComObject("wst/vbi codec pinout", pinWST_VBI);
      }
      return true;
    }

    /// <summary>
    /// Adds the MPEG muxer filter
    /// </summary>
    /// <returns></returns>
    private bool AddMpegMuxer()
    {
      if (!CheckThreadId())
        return false;

      Log.Log.WriteFile("analog:AddMpegMuxer()");
      try
      {
        string monikerPowerDirectorMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7F2BBEAF-E11C-4D39-90E8-938FB5A86045}";
        string monikerPowerDvdMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{6770E328-9B73-40C5-91E6-E2F321AEDE57}";
        string monikerPowerDvdMuxer2 = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{370E9701-9DC5-42C8-BE29-4E75F0629EED}";
        _filterMpegMuxer = Marshal.BindToMoniker(monikerPowerDirectorMuxer) as IBaseFilter;
        int hr = _graphBuilder.AddFilter(_filterMpegMuxer, "CyberLink MPEG Muxer");
        if (hr != 0)
        {
          _filterMpegMuxer = Marshal.BindToMoniker(monikerPowerDvdMuxer) as IBaseFilter;
          hr = _graphBuilder.AddFilter(_filterMpegMuxer, "CyberLink MPEG Muxer");
          if (hr != 0)
          {
            _filterMpegMuxer = Marshal.BindToMoniker(monikerPowerDvdMuxer2) as IBaseFilter;
            hr = _graphBuilder.AddFilter(_filterMpegMuxer, "CyberLink MPEG Muxer");
            if (hr != 0)
            {
              Log.Log.WriteFile("analog:AddMpegMuxer returns:0x{0:X}", hr);
              //throw new TvException("Unable to add Cyberlink MPEG Muxer");
            }
          }
        }
        Log.Log.WriteFile("analog:connect pinvideo {0} ->mpeg muxer", FilterGraphTools.LogPinInfo(_pinVideo));
        if (!FilterGraphTools.ConnectPin(_graphBuilder, _pinVideo, _filterMpegMuxer, 0))
        {
          Log.Log.WriteFile("analog: unable to connect pinvideo->mpeg muxer");
        }
        _pinVideoConnected = true;
        Log.Log.WriteFile("analog: connected pinvideo->mpeg muxer");
        //Adaptec devices use the LPCM pin for audio so we check this can connect if applicable.
        bool isAdaptec = false;
        if (_captureDevice.Name.Contains("Adaptec USB Capture Device") || _captureDevice.Name.Contains("Adaptec PCI Capture Device"))
        {
          Log.Log.WriteFile("analog: AddMpegMuxer, Adaptec device found using LPCM");
          isAdaptec = true;
        }
        if (isAdaptec)
        {
          if (!FilterGraphTools.ConnectPin(_graphBuilder, _pinLPCM, _filterMpegMuxer, 1))
          {
            Log.Log.WriteFile("analog: AddMpegMuxer, unable to connect pinLPCM->mpeg muxer");
          }
          Log.Log.WriteFile("analog: AddMpegMuxer, connected pinLPCM->mpeg muxer");
        }
        else
        {
          Log.Log.WriteFile("analog:connect pinaudio {0} ->mpeg muxer", FilterGraphTools.LogPinInfo(_pinAudio));
          if (!FilterGraphTools.ConnectPin(_graphBuilder, _pinAudio, _filterMpegMuxer, 1))
          {
            Log.Log.WriteFile("analog:AddMpegMuxer, unable to connect pinaudio->mpeg muxer");
          }
          Log.Log.WriteFile("analog:AddMpegMuxer, connected pinaudio->mpeg muxer");
        }
        return true;
      } catch (Exception ex)
      {
        throw new TvException("Cyberlink MPEG Muxer filter (mpgmux.ax) not installed " + ex.Message);
      }
    }

    #endregion

    /// <summary>
    /// Updates the video pin to guarantee, that for tv both streams are in the mux
    /// </summary>
    /// <param name="isTv">true, when tv is on; false for radio</param>
    private void UpdatePinVideo(bool isTv)
    {
      if (isTv == _pinVideoConnected)
        return;
      _pinVideoConnected = isTv;
      if (_pinVideoConnected)
      {
        Log.Log.Write("analog: Update pin video: connect");
        FilterGraphTools.ConnectPin(_graphBuilder, _pinVideo, _filterMpegMuxer, 0);
      }
      else
      {
        Log.Log.Write("analog: Update pin video: disconnect");
        _pinVideo.Disconnect();
      }
    }

    /// <summary>
    /// Methods which starts the graph
    /// </summary>
    private void RunGraph(int subChannel)
    {
      if (!CheckThreadId())
        return;
      if (_mapSubChannels.ContainsKey(subChannel))
      {
        _mapSubChannels[subChannel].OnGraphStart();
      }
      FilterState state;
      (_graphBuilder as IMediaControl).GetState(10, out state);
      if (state == FilterState.Running)
        return;
      Log.Log.WriteFile("analog: RunGraph");
      int hr = 0;
      hr = (_graphBuilder as IMediaControl).Run();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("analog: RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to start graph");
      }
      if (_mapSubChannels.ContainsKey(subChannel))
      {
        _mapSubChannels[subChannel].OnGraphStarted();
      }
    }

    private void SetFrequencyOverride(AnalogChannel channel)
    {
      int countryCode = channel.Country.Id;
      string[] registryLocations = new string[] { String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-1", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-0", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-1"),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-0")};
      if (channel.Frequency == 0)
      {
        //remove the frequency override in 
        for (int index = 0; index < registryLocations.Length; index++)
        {
          using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(registryLocations[index]))
          {
            registryKey.DeleteValue(channel.ChannelNumber.ToString(), false);
          }
          using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(registryLocations[index]))
          {
            registryKey.DeleteValue(channel.ChannelNumber.ToString(), false);
          }
        }
        return;
      }
      //set frequency override
      for (int index = 0; index < registryLocations.Length; index++)
      {
        using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(registryLocations[index]))
        {
          registryKey.SetValue(channel.ChannelNumber.ToString(), (int)channel.Frequency);
        }
        using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(registryLocations[index]))
        {
          registryKey.SetValue(channel.ChannelNumber.ToString(), (int)channel.Frequency);
        }
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
            Log.Log.WriteFile("analog:  set to FM radio");
            tvTuner.put_Mode(AMTunerModeType.FMRadio);
          }
          else
          {
            Log.Log.WriteFile("analog:  set to TV");
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
          Log.Log.WriteFile("analog:  set to FM radio");
          tvTuner.put_Mode(AMTunerModeType.FMRadio);
        }
        else
        {
          Log.Log.WriteFile("analog:  set to TV");
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
      if (_graphState == GraphState.Idle)
        _graphState = GraphState.Created;
      Log.Log.WriteFile("Analog: Tuned to country:{0} video:{1} Hz audio:{2} Hz locked:{3}", analogChannel.Country.Id, videoFrequency, audioFrequency, IsTunerLocked);
      _lastSignalUpdate = DateTime.MinValue;
    }

    private void UpdateMinMaxChannel()
    {
      if (_filterTvTuner == null)
        return;
      IAMTVTuner tvTuner = _filterTvTuner as IAMTVTuner;
      if (tvTuner == null)
        return;
      tvTuner.ChannelMinMax(out _minChannel, out _maxChannel);
    }
    #endregion

    #region scanning interface
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
        IAMTVTuner tvTuner = _filterTvTuner as IAMTVTuner;
        int videoFrequency;
        tvTuner.get_VideoFrequency(out videoFrequency);
        return videoFrequency;
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
        IAMTVTuner tvTuner = _filterTvTuner as IAMTVTuner;
        int audioFrequency;
        tvTuner.get_AudioFrequency(out audioFrequency);
        return audioFrequency;
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
