#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player.Subtitles;
using MediaPortal.Player.Teletext;
using MediaPortal.Profile;

namespace MediaPortal.Player
{
  internal class TSReaderPlayer : BaseTSReaderPlayer
  {
    #region variables

    private VMR9Util _vmr9 = null;
    protected IBaseFilter _audioSwitcherFilter = null;
    protected IAudioStream _audioStream = null;
    protected ISubtitleStream _subtitleStream = null;
    protected TeletextReceiver _ttxtReceiver = null;
    protected ITeletextSource _teletextSource = null;
    private bool enableDVBBitmapSubtitles = false;
    private bool enableDVBTtxtSubtitles = false;
    private bool enableMPAudioSwitcher = false;
    private int relaxTsReader = 0; // Disable dropping of discontinued dvb packets    
    string strVideoCodec = "";
    string strAudioCodec = "";
    string strAACAudioCodec = "";
    string strDDPLUSAudioCodec = "";
    string strH264VideoCodec = ""; 
    #endregion

    [Guid("558D9EA6-B177-4c30-9ED5-BF2D714BCBCA"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioStream
    {
      void GetAudioStream(ref Int32 stream);
    }

    /// <summary>
    /// Interface to the TsReader filter wich provides information about the 
    /// subtitle streams and allows us to change the current subtitle stream
    /// </summary>
    /// 
    [Guid("43FED769-C5EE-46aa-912D-7EBDAE4EE93A"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISubtitleStream
    {
      void SetSubtitleStream(Int32 stream);
      void GetSubtitleStreamType(Int32 stream, ref Int32 type);
      void GetSubtitleStreamCount(ref Int32 count);
      void GetCurrentSubtitleStream(ref Int32 stream);
      void GetSubtitleStreamLanguage(Int32 stream, ref SUBTITLE_LANGUAGE szLanguage);
      void SetSubtitleResetCallback(IntPtr callBack);
    }

    /// <summary>
    /// Structure to pass the subtitle language data from TsReader to this class
    /// </summary>
    /// 
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SUBTITLE_LANGUAGE
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public string lang;
    }

    #region ctor

    public TSReaderPlayer()
      : base() {}

    public TSReaderPlayer(g_Player.MediaType type)
      : base(type) {}

    #endregion

    public override eAudioDualMonoMode GetAudioDualMonoMode()
    {
      if (_audioSwitcherFilter == null)
      {
        Log.Info("TsReaderPlayer: AudioDualMonoMode switching not available. Audioswitcher filter not loaded");
        return eAudioDualMonoMode.UNSUPPORTED;
      }
      IMPAudioSwitcherFilter mpSwitcher = _audioSwitcherFilter as IMPAudioSwitcherFilter;
      if (mpSwitcher == null)
      {
        Log.Info("TsReaderPlayer: AudioDualMonoMode switching not available. Audioswitcher filter not loaded");
        return eAudioDualMonoMode.UNSUPPORTED;
      }
      uint iMode = 0;
      int hr = mpSwitcher.GetAudioDualMonoMode(out iMode);
      if (hr != 0)
      {
        Log.Info("TsReaderPlayer: GetAudioDualMonoMode failed 0x{0:X}", hr);
        return eAudioDualMonoMode.UNSUPPORTED;
      }
      eAudioDualMonoMode mode = (eAudioDualMonoMode)iMode;
      Log.Info("TsReaderPlayer: GetAudioDualMonoMode mode={0} succeeded", iMode.ToString(), hr);
      return mode;
    }

    public override bool SetAudioDualMonoMode(eAudioDualMonoMode mode)
    {
      if (_audioSwitcherFilter == null)
      {
        Log.Info("TsReaderPlayer: AudioDualMonoMode switching not available. Audioswitcher filter not loaded");
        return false;
      }
      IMPAudioSwitcherFilter mpSwitcher = _audioSwitcherFilter as IMPAudioSwitcherFilter;
      if (mpSwitcher == null)
      {
        Log.Info("TsReaderPlayer: AudioDualMonoMode switching not available. Audioswitcher filter not loaded");
        return false;
      }
      int hr = mpSwitcher.SetAudioDualMonoMode((uint)mode);
      if (hr != 0)
      {
        Log.Info("TsReaderPlayer: SetAudioDualMonoMode mode={0} failed 0x{1:X}", mode.ToString(), hr);
      }
      else
      {
        Log.Info("TsReaderPlayer: SetAudioDualMonoMode mode={0} succeeded", mode.ToString(), hr);
      }
      return (hr == 0);
    }

    protected void LoadMyTvFilterSettings(ref int intFilters, ref string strFilters, ref string strVideoCodec,
                                          ref string strAudioCodec, ref string strAACAudioCodec, ref string strDDPLUSAudioCodec,
                                          ref string strH264VideoCodec, ref string strAudioRenderer,
                                          ref bool enableDVBBitmapSubtitles, ref bool enableDVBTtxtSubtitles,
                                          ref int relaxTsReader)
    {
      using (Settings xmlreader = new MPSettings())
      {
        // FlipGer: load infos for custom filters
        int intCount = 0;
        while (xmlreader.GetValueAsString("mytv", "filter" + intCount.ToString(), "undefined") != "undefined")
        {
          if (xmlreader.GetValueAsBool("mytv", "usefilter" + intCount.ToString(), false))
          {
            strFilters += xmlreader.GetValueAsString("mytv", "filter" + intCount.ToString(), "undefined") + ";";
            intFilters++;
          }
          intCount++;
        }
        strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
        strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
        strAACAudioCodec = xmlreader.GetValueAsString("mytv", "aacaudiocodec", "");
        strDDPLUSAudioCodec = xmlreader.GetValueAsString("mytv", "ddplusaudiocodec", "");
        strH264VideoCodec = xmlreader.GetValueAsString("mytv", "h264videocodec", "");
        strAudioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "Default DirectSound Device");
        enableDVBBitmapSubtitles = xmlreader.GetValueAsBool("tvservice", "dvbbitmapsubtitles", false);
        enableDVBTtxtSubtitles = xmlreader.GetValueAsBool("tvservice", "dvbttxtsubtitles", false);
        enableMPAudioSwitcher = xmlreader.GetValueAsBool("tvservice", "audiodualmono", false);
        relaxTsReader = (xmlreader.GetValueAsBool("mytv", "relaxTsReader", false) == false ? 0 : 1);
        // as int for passing through interface
        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "Normal");
        GUIGraphicsContext.ARType = Util.Utils.GetAspectRatio(strValue);
        _CodecSupportsFastSeeking = xmlreader.GetValueAsBool("debug", "CodecSupportsFastSeeking", true);
      }
    }

    protected override void OnInitialized()
    {
      Log.Info("TSReaderPlayer: OnInitialized");
      if (_vmr9 != null)
      {
        _vmr9.Enable(true);
        _updateNeeded = true;
        SetVideoWindow();
      }
    }

    public override void SetVideoWindow()
    {
      if (GUIGraphicsContext.IsFullScreenVideo != _isFullscreen)
      {
        _isFullscreen = GUIGraphicsContext.IsFullScreenVideo;
        _updateNeeded = true;
      }
      if (!_updateNeeded)
      {
        return;
      }
      _updateNeeded = false;
      _isStarted = true;
    }

    protected override void MatchFilters(string format)
    {
      if (format == "Video")
      {
        if (_videoFormat.streamType == VideoStreamType.MPEG2)
        {
          videoFilter = strVideoCodec;          
        }
        else
        {
          videoFilter = strH264VideoCodec;          
        }
      }
      else
      {
        if (AudioType(CurrentAudioStream).Contains("AAC"))
        {
          audioFilter = strAACAudioCodec;
        }
        else
        {
          if (AudioType(CurrentAudioStream).Contains("DD+"))
          {
            audioFilter = strDDPLUSAudioCodec;
          }
          else
          {
            audioFilter = strAudioCodec;
          }
        }
      }
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
      Log.Info("TSReaderPlayer: GetInterfaces()");
      try
      {
        string strAudioRenderer = "";
        int intFilters = 0; // FlipGer: count custom filters
        string strFilters = ""; // FlipGer: collect custom filters

        LoadMyTvFilterSettings(ref intFilters, ref strFilters, ref strVideoCodec, ref strAudioCodec,
                               ref strAACAudioCodec, ref strDDPLUSAudioCodec, ref strH264VideoCodec, ref strAudioRenderer,
                               ref enableDVBBitmapSubtitles, ref enableDVBTtxtSubtitles, ref relaxTsReader);
        
        _graphBuilder = (IGraphBuilder)new FilterGraph();
        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);

        #region add vmr9
        if (!_isRadio)
        {
          _vmr9 = new VMR9Util();
          _vmr9.AddVMR9(_graphBuilder);
          _vmr9.Enable(false);
        }

        #endregion

        if (strAudioRenderer.Length > 0) //audio renderer must be in graph before audio switcher
        {
          _audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudioRenderer, true);
        }

        #region add AudioSwitcher

        if (enableMPAudioSwitcher) //audio switcher must be in graph before tsreader audiochangecallback
        {
          _audioSwitcherFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, "MediaPortal AudioSwitcher");
          if (_audioSwitcherFilter == null)
          {
            Log.Error("TSReaderPlayer: Failed to add AudioSwitcher to graph");
          }
        }

        #endregion
        
        #region add TsReader

        TsReader reader = new TsReader();
        _fileSource = (IBaseFilter)reader;
        _ireader = (ITSReader)reader;
        _interfaceTSReader = _fileSource;
        _ireader.SetRelaxedMode(relaxTsReader); // enable/disable continousity filtering
        _ireader.SetTsReaderCallback(this);
        _ireader.SetRequestAudioChangeCallback(this);
        Log.Info("TSReaderPlayer: Add TsReader to graph");
        int hr = _graphBuilder.AddFilter((IBaseFilter)_fileSource, "TsReader");
        DsError.ThrowExceptionForHR(hr);

        #endregion

        #region load file in TsReader

        IFileSourceFilter interfaceFile = (IFileSourceFilter)_fileSource;
        if (interfaceFile == null)
        {
          Log.Error("TSReaderPlayer: Failed to get IFileSourceFilter");
          Cleanup();
          return false;
        }
        Log.Info("TSReaderPlayer: Open file: {0}", filename);
        hr = interfaceFile.Load(filename, null);
        if (hr != 0)
        {
          Log.Error("TSReaderPlayer: Failed to open file:{0} :0x{1:x}", filename, hr);
          Cleanup();
          return false;
        }

        #endregion

        #region add codecs

        Log.Info("TSReaderPlayer: Add codecs");
        // add preferred video & audio codecs
        
        MatchFilters("Video");
        MatchFilters("Audio");

        if (!_isRadio)
        {
          DirectShowUtil.AddFilterToGraph(_graphBuilder, videoFilter);
          if (enableDVBBitmapSubtitles)
          {
            try
            {
              SubtitleRenderer.GetInstance().AddSubtitleFilter(_graphBuilder);
            }
            catch (Exception e)
            {
              Log.Error(e);
            }
          }
        }

        DirectShowUtil.AddFilterToGraph(_graphBuilder, audioFilter);

        // FlipGer: add custom filters to graph
        string[] arrFilters = strFilters.Split(';');
        for (int i = 0; i < intFilters; i++)
        {
          DirectShowUtil.AddFilterToGraph(_graphBuilder, arrFilters[i]);
        }

        #endregion

        #region render TsReader output pins

        Log.Info("TSReaderPlayer: Render TsReader outputs");
        if (_isRadio)
        {
          IEnumPins enumPins;
          hr = _fileSource.EnumPins(out enumPins);
          DsError.ThrowExceptionForHR(hr);
          IPin[] pins = new IPin[1];
          int fetched = 0;
          while (enumPins.Next(1, pins, out fetched) == 0)
          {
            if (fetched != 1)
            {
              break;
            }
            PinDirection direction;
            pins[0].QueryDirection(out direction);
            if (direction == PinDirection.Output)
            {
              IEnumMediaTypes enumMediaTypes;
              pins[0].EnumMediaTypes(out enumMediaTypes);
              AMMediaType[] mediaTypes = new AMMediaType[20];
              int fetchedTypes;
              enumMediaTypes.Next(20, mediaTypes, out fetchedTypes);
              for (int i = 0; i < fetchedTypes; ++i)
              {
                if (mediaTypes[i].majorType == MediaType.Audio)
                {
                  hr = _graphBuilder.Render(pins[0]);
                  DsError.ThrowExceptionForHR(hr);
                  break;
                }
              }
            }
            DirectShowUtil.ReleaseComObject(pins[0]);
          }
          DirectShowUtil.ReleaseComObject(enumPins);
        }
        else
        {
          DirectShowUtil.RenderUnconnectedOutputPins(_graphBuilder, _fileSource);
        }        
        DirectShowUtil.RemoveUnusedFiltersFromGraph(_graphBuilder);

        #endregion

        _mediaCtrl = (IMediaControl)_graphBuilder;
        _mediaEvt = (IMediaEventEx)_graphBuilder;
        _mediaSeeking = (IMediaSeeking)_graphBuilder;
        if (_mediaSeeking == null)
        {
          Log.Error("TSReaderPlayer: Unable to get IMediaSeeking interface");
        }
        _audioStream = (IAudioStream)_fileSource;
        if (_audioStream == null)
        {
          Log.Error("TSReaderPlayer: Unable to get IAudioStream interface");
        }
        _audioSelector = new AudioSelector(_audioStream);

        if (!_isRadio)
        {
          if (enableDVBTtxtSubtitles || enableDVBBitmapSubtitles)
          {
            try
            {
              SubtitleRenderer.GetInstance().SetPlayer(this);
              _dvbSubRenderer = SubtitleRenderer.GetInstance();
            }
            catch (Exception e)
            {
              Log.Error(e);
            }
          }
          if (enableDVBBitmapSubtitles)
          {
            _subtitleStream = (ISubtitleStream)_fileSource;
            if (_subtitleStream == null)
            {
              Log.Error("TSReaderPlayer: Unable to get ISubtitleStream interface");
            }
          }
          if (enableDVBTtxtSubtitles)
          {
            //Log.Debug("TSReaderPlayer: Obtaining TeletextSource");
            _teletextSource = (ITeletextSource)_fileSource;
            if (_teletextSource == null)
            {
              Log.Error("TSReaderPlayer: Unable to get ITeletextSource interface");
            }
            Log.Debug("TSReaderPlayer: Creating Teletext Receiver");
            TeletextSubtitleDecoder ttxtDecoder = new TeletextSubtitleDecoder(_dvbSubRenderer);
            _ttxtReceiver = new TeletextReceiver(_teletextSource, ttxtDecoder);
            // regardless of whether dvb subs are enabled, the following call is okay
            // if _subtitleStream is null the subtitle will just not setup for bitmap subs 
            _subSelector = new SubtitleSelector(_subtitleStream, _dvbSubRenderer, ttxtDecoder);
          }
          else if (enableDVBBitmapSubtitles)
          {
            // if only dvb subs are enabled, pass null for ttxtDecoder
            _subSelector = new SubtitleSelector(_subtitleStream, _dvbSubRenderer, null);
          }
        }
        if (_audioRendererFilter != null)
        {
          //Log.Info("TSReaderPlayer:set reference clock");
          IMediaFilter mp = (IMediaFilter)_graphBuilder;
          IReferenceClock clock = (IReferenceClock)_audioRendererFilter;
          hr = mp.SetSyncSource(null);
          hr = mp.SetSyncSource(clock);
          //Log.Info("TSReaderPlayer:set reference clock:{0:X}", hr);
          _basicAudio = (IBasicAudio)_graphBuilder;
        }
        if (!_isRadio)
        {
          if (!_vmr9.IsVMR9Connected)
          {
            Log.Error("TSReaderPlayer: Failed vmr9 not connected");
            Cleanup();
            return false;
          }
          DirectShowUtil.EnableDeInterlace(_graphBuilder);
          _vmr9.SetDeinterlaceMode();
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("TSReaderPlayer: Exception while creating DShow graph {0}", ex.Message);
        Cleanup();
        return false;
      }
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
    {
      Cleanup();
    }

    protected override void ExclusiveMode(bool onOff)
    {
      GUIMessage msg = null;
      if (onOff)
      {
        Log.Info("TSReaderPlayer: Enabling DX9 exclusive mode");
        if (_isRadio == false)
        {
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        }
      }
      else
      {
        Log.Info("TSReaderPlayer: Disabling DX9 exclusive mode");
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      }
      GUIWindowManager.SendMessage(msg);
    }

    private object lockObj = new object();
    private void Cleanup()
    {
      lock (lockObj)
      {
        if (_graphBuilder == null)
        {
          return;
        }
        int hr;
        Log.Info("TSReaderPlayer: cleanup DShow graph {0}", GUIGraphicsContext.InVmr9Render);
        try
        {
          if (_mediaCtrl != null)
          {
            int counter = 0;
            FilterState state;
            hr = _mediaCtrl.Stop();
            hr = _mediaCtrl.GetState(10, out state);
            while (state != FilterState.Stopped || GUIGraphicsContext.InVmr9Render)
            {
              Thread.Sleep(100);
              hr = _mediaCtrl.GetState(10, out state);
              counter++;
              if (counter >= 30)
              {
                if (state != FilterState.Stopped)
                  Log.Error("TSReaderPlayer: graph still running");
                if (GUIGraphicsContext.InVmr9Render)
                  Log.Error("TSReaderPlayer: in renderer");
                break;
              }
            }
            _mediaCtrl = null;
          }

          if (_mediaEvt != null)
          {
            hr = _mediaEvt.SetNotifyWindow(IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero);
            _mediaEvt = null;
          }

          _videoWin = _graphBuilder as IVideoWindow;
          if (_videoWin != null)
          {
            hr = _videoWin.put_Visible(OABool.False);
            hr = _videoWin.put_Owner(IntPtr.Zero);
            _videoWin = null;
          }
          
          _mediaSeeking = null;
          _basicAudio = null;
          _basicVideo = null;
          _ireader = null;

          if (_audioRendererFilter != null)
          {
            while (DirectShowUtil.ReleaseComObject(_audioRendererFilter) > 0) ;
            _audioRendererFilter = null;
          }

          if (_fileSource != null)
          {
            DirectShowUtil.ReleaseComObject(_fileSource, 5000);
            _fileSource = null;
          }

          if (_vmr9 != null)
          {
            _vmr9.Enable(false);
            _vmr9.Dispose();
            _vmr9 = null;
          }

          if (_graphBuilder != null)
          {
            DirectShowUtil.RemoveFilters(_graphBuilder);
            if (_rotEntry != null)
            {
              _rotEntry.Dispose();
              _rotEntry = null;
            }
            while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0) ;          
            _graphBuilder = null;
          }

          if (_dvbSubRenderer != null)
          {            
            _dvbSubRenderer.SetPlayer(null);
            _dvbSubRenderer = null;
          }          

          GUIGraphicsContext.form.Invalidate(true);
          _state = PlayState.Init;
        }
        catch (Exception ex)
        {
          Log.Error("TSReaderPlayer: Exception while cleaning DShow graph - {0} {1}", ex.Message, ex.StackTrace);
        }
        //switch back to directx windowed mode
        Log.Info("TSReaderPlayer: Disabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
    }

    protected override void OnProcess()
    {
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      if (_vmr9 != null)
      {
        _videoWidth = _vmr9.VideoWidth;
        _videoHeight = _vmr9.VideoHeight;
      }
    }

    public override void SeekAbsolute(double dTimeInSecs)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          lock (_mediaCtrl)
          {
            int SeekTries = 3;

            UpdateCurrentPosition();
            if (dTimeInSecs < 0)
            {
              dTimeInSecs = 0;
            }
            dTimeInSecs *= 10000000d;

            long lTime = (long)dTimeInSecs;

            while (SeekTries>0)
            {
              long pStop = 0;
              long lContentStart, lContentEnd;
              _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
              lTime += lContentStart;
              Log.Info("TsReaderPlayer:seekabs:{0} start:{1} end:{2}", lTime, lContentStart, lContentEnd);
              /*if (lTime > lContentEnd)
              {
                System.Threading.Thread.Sleep(500);
                _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
                lTime = lContentEnd;
              }*/
              int hr = _mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning,
                                                  new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
              long lStreamPos;
              _mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
              _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
              Log.Info("TsReaderPlayer: pos: {0} start:{1} end:{2}", lStreamPos, lContentStart, lContentEnd);
              if (lStreamPos > lContentStart)
              {
                Log.Info("TsReaderPlayer seek done:{0:X}", hr);
                SeekTries = 0;
              }
              else
              {
                // This could happen in LiveTv/Rstp when TsBuffers are reused and requested position is before "start"
                // Only way to recover correct position is to seek again on "start"
                SeekTries--;
                lTime = 0;
                Log.Info("TsReaderPlayer seek again : pos: {0} lower than start:{1} end:{2} ( Cnt {3} )", lStreamPos, lContentStart, lContentEnd, SeekTries);
              }
            }

            if (VMR9Util.g_vmr9 != null)
            {
              VMR9Util.g_vmr9.FrameCounter = 123;
            }
          }
        }

        UpdateCurrentPosition();
        if (_dvbSubRenderer != null)
        {
          _dvbSubRenderer.OnSeek(CurrentPosition);
        }
        _state = PlayState.Playing;
        Log.Info("TSReaderPlayer: current pos:{0} dur:{1}", CurrentPosition, Duration);
      }
    }

    /// <summary>
    /// Property to get the total number of subtitle streams
    /// </summary>
    public override int SubtitleStreams
    {
      get
      {
        if (_subSelector != null)
        {
          return _subSelector.CountOptions();
        }
        else
        {
          return 0;
        }
      }
    }

    /// <summary>
    /// Property to get/set the current subtitle stream
    /// </summary>
    public override int CurrentSubtitleStream
    {
      get
      {
        if (_subSelector != null)
        {
          return _subSelector.GetCurrentOption();
        }
        else
        {
          return 0;
        }
      }
      set
      {
        if (_subSelector != null)
        {
          _subSelector.SetOption(value);
        }
      }
    }

    /// <summary>
    /// Property to get the name for a subtitle stream
    /// </summary>
    public override string SubtitleLanguage(int iStream)
    {
      if (_subSelector != null)
      {
        return _subSelector.GetLanguage(iStream);
      }
      else
      {
        return Strings.Unknown;
      }
    }

    /// <summary>
    /// Property to enable/disable subtitles
    /// </summary>
    public override bool EnableSubtitle
    {
      get
      {
        if (_subSelector != null)
        {
          return _dvbSubRenderer.RenderSubtitles;
        }
        else
        {
          return false;
        }
      }
      set
      {
        if (_subSelector != null)
        {
          _dvbSubRenderer.RenderSubtitles = value;
        }
      }
    }

    /// <summary>
    /// Property to specify channel change
    /// </summary>
    public override void OnZapping(int info)
    {
      if (_ireader != null)
      {
        _ireader.OnZapping(info);
      }
      Log.Info("TSReaderPlayer: OnZapping :{0}", info);
    }
  }
}