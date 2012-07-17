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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player.Subtitles;
using MediaPortal.Player.Teletext;
using MediaPortal.Profile;
using MediaPortal.Player.PostProcessing;
using System.Collections;
using System.Collections.Generic;

namespace MediaPortal.Player
{
  internal class TSReaderPlayer : BaseTSReaderPlayer
  {
    #region variables

    private VMR9Util _vmr9 = null;
    protected IAudioStream _audioStream = null;
    protected ISubtitleStream _subtitleStream = null;
    protected TeletextReceiver _ttxtReceiver = null;
    protected ITeletextSource _teletextSource = null;
    
    //Set to false to use normal CoreCCParser - Set to true for using CoreCCParser H264 Build Test
    protected bool CoreCCParserH264 = false;

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
      if (filterCodec._audioSwitcherFilter == null)
      {
        Log.Info("TsReaderPlayer: AudioDualMonoMode switching not available. Audioswitcher filter not loaded");
        return eAudioDualMonoMode.UNSUPPORTED;
      }
      IMPAudioSwitcherFilter mpSwitcher = filterCodec._audioSwitcherFilter as IMPAudioSwitcherFilter;
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
      if (filterCodec._audioSwitcherFilter == null)
      {
        Log.Info("TsReaderPlayer: AudioDualMonoMode switching not available. Audioswitcher filter not loaded");
        return false;
      }
      IMPAudioSwitcherFilter mpSwitcher = filterCodec._audioSwitcherFilter as IMPAudioSwitcherFilter;
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
    }

    protected override string MatchFilters(string format)
    {
      if (filterConfig != null && format == "Video")
      {
        if (_videoFormat.streamType == VideoStreamType.MPEG2)
        {
          return this.filterConfig.Video;
        }
        else
        {
          return this.filterConfig.VideoH264;
        }
      }
      else
      {
        if (filterConfig != null && AudioType(CurrentAudioStream).Contains("AAC"))
        {
          return this.filterConfig.AudioAAC;
        }
        else
        {
          if (filterConfig != null && AudioType(CurrentAudioStream).Equals("AC3plus"))
          {
            return this.filterConfig.AudioDDPlus;
          }
          else
          {
            return this.filterConfig.Audio;
          }
        }
      }
    }

    protected override void PostProcessAddVideo()
    {
      if (filterConfig != null)
      {
        foreach (string filter in this.filterConfig.OtherFilters)
        {
          if (FilterHelper.GetVideoCodec().Contains(filter.ToString()) && filter.ToString() != "Core CC Parser")
          {
            var comObject = DirectShowUtil.AddFilterToGraph(_graphBuilder, filter);
            if (comObject != null)
            {
              PostProcessFilterVideo.Add(filter, comObject);
            }
          }
        }
      }
    }

    protected override void PostProcessAddAudio()
    {
      if (filterConfig != null)
      {
        foreach (string filter in this.filterConfig.OtherFilters)
        {
          if (FilterHelper.GetAudioCodec().Contains(filter.ToString()))
          {
            var comObject = DirectShowUtil.AddFilterToGraph(_graphBuilder, filter);
            if (comObject != null)
            {
              PostProcessFilterAudio.Add(filter, comObject);
            }
          }
        }
      }
    }

    protected override void AudioRendererAdd()
    {
      if (filterConfig != null && this.filterConfig.AudioRenderer.Length > 0) //audio renderer must be in graph before audio switcher
      {
        filterCodec._audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, filterConfig.AudioRenderer, true);
      }
    }    

    protected override void MPAudioSwitcherAdd()
    {
      if (filterConfig != null && filterConfig.enableMPAudioSwitcher) //audio switcher must be in graph before tsreader audiochangecallback
      {
        if (filterCodec._audioSwitcherFilter != null)
        {
          DirectShowUtil.ReleaseComObject(filterCodec._audioSwitcherFilter);
          filterCodec._audioSwitcherFilter = null;
        }
        filterCodec._audioSwitcherFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, "MediaPortal AudioSwitcher");
        if (filterCodec._audioSwitcherFilter == null)
        {
          Log.Error("TSReaderPlayer: Failed to add AudioSwitcher to graph");
        }
      }
    }

    protected override void SyncAudioRenderer()
    {
      if (filterCodec._audioRendererFilter != null)
      {
        //Log.Info("BDPlayer:set reference clock");
        IMediaFilter mp = (IMediaFilter)_graphBuilder;
        IReferenceClock clock = (IReferenceClock)filterCodec._audioRendererFilter;
        int hr = mp.SetSyncSource(null);
        hr = mp.SetSyncSource(clock);
        //Log.Info("BDPlayer:set reference clock:{0:X}", hr);
        _basicAudio = (IBasicAudio)_graphBuilder;
      }
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
      Log.Info("TSReaderPlayer: GetInterfaces()");
      try
      {
        _graphBuilder = (IGraphBuilder)new FilterGraph();
        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);

        //Get filterCodecName and filterConfig
        filterConfig = GetFilterConfiguration();
        filterCodec = GetFilterCodec();

        #region add AudioRenderer

        //Add Audio Renderer
        AudioRendererAdd();

        #endregion

        #region add AudioSwitcher

        MPAudioSwitcherAdd();

        #endregion

        #region add TsReader

        TsReader reader = new TsReader();
        _fileSource = (IBaseFilter)reader;
        _ireader = (ITSReader)reader;
        _interfaceTSReader = _fileSource;
        if (filterConfig != null)
        _ireader.SetRelaxedMode(filterConfig.relaxTsReader); // enable/disable continousity filtering
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

        // does .ts file contain video?
        // default is _isRadio=false which prevents recorded radio file playing
        if (!_videoFormat.IsValid)
        {
          Log.Debug("TSReaderPlayer: Stream is Radio");
          _isRadio = true;
        }

        if (!_isRadio)
        {
          _vmr9 = new VMR9Util();
          _vmr9.AddVMR9(_graphBuilder);
          _vmr9.Enable(false);

          // Add preferred video filters
          UpdateFilters("Video");
          Log.Debug("TSReaderPlayer: UpdateFilters Video done");

          if (filterConfig != null && filterConfig.enableDVBBitmapSubtitles)
          {
            try
            {
              SubtitleRenderer.GetInstance().AddSubtitleFilter(_graphBuilder);
              Log.Debug("TSReaderPlayer: SubtitleRenderer AddSubtitleFilter");
            }
            catch (Exception e)
            {
              Log.Error(e);
            }
          }
        }

        // Add preferred audio filters
        UpdateFilters("Audio");
        Log.Debug("TSReaderPlayer: UpdateFilters Audio done");

        #endregion

        #region PostProcessingEngine Detection

        IPostProcessingEngine postengine = PostProcessingEngine.GetInstance(true);
        if (!postengine.LoadPostProcessing(_graphBuilder))
        {
          PostProcessingEngine.engine = new PostProcessingEngine.DummyEngine();
          Log.Debug("TSReaderPlayer: PostProcessingEngine to DummyEngine");
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
          DirectShowUtil.RenderGraphBuilderOutputPins(_graphBuilder, _fileSource);
          if (filterConfig != null && !filterConfig.enableCCSubtitles)
          {
            CleanupCC();
            Log.Debug("TSReaderPlayer: CleanupCC filter (Tv/Recorded Stream Detected)");
          }
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
          if (filterConfig != null && filterConfig.enableDVBTtxtSubtitles || filterConfig.enableDVBBitmapSubtitles)
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
          if (filterConfig != null && filterConfig.enableDVBBitmapSubtitles)
          {
            _subtitleStream = (ISubtitleStream)_fileSource;
            if (_subtitleStream == null)
            {
              Log.Error("TSReaderPlayer: Unable to get ISubtitleStream interface");
            }
          }
          if (filterConfig != null && filterConfig.enableDVBTtxtSubtitles)
          {
            //Log.Debug("TSReaderPlayer: Obtaining TeletextSource");
            _teletextSource = (ITeletextSource)_fileSource;
            if (_teletextSource == null)
            {
              Log.Error("TSReaderPlayer: Unable to get ITeletextSource interface");
            }
            Log.Debug("TSReaderPlayer: Creating Teletext Receiver");
            try
            {
                using (MPSettings xmlreader = new MPSettings())
                    xmlreader.SetValue("tvservice", "dvbdefttxtsubtitles", "999;999");
            }
            catch { }
            TeletextSubtitleDecoder ttxtDecoder = new TeletextSubtitleDecoder(_dvbSubRenderer);
            _ttxtReceiver = new TeletextReceiver(_teletextSource, ttxtDecoder);
            // regardless of whether dvb subs are enabled, the following call is okay
            // if _subtitleStream is null the subtitle will just not setup for bitmap subs 
            _subSelector = new SubtitleSelector(_subtitleStream, _dvbSubRenderer, ttxtDecoder);
          }
          else if (filterConfig != null && filterConfig.enableDVBBitmapSubtitles)
          {
            // if only dvb subs are enabled, pass null for ttxtDecoder
            _subSelector = new SubtitleSelector(_subtitleStream, _dvbSubRenderer, null);
          }
        }
        if (filterCodec._audioRendererFilter != null)
        {
          //Log.Info("TSReaderPlayer:set reference clock");
          /*IMediaFilter mp = (IMediaFilter)_graphBuilder;
          IReferenceClock clock = (IReferenceClock)filterCodec._audioRendererFilter;
          hr = mp.SetSyncSource(null);
          hr = mp.SetSyncSource(clock);*/
          //Log.Info("TSReaderPlayer:set reference clock:{0:X}", hr);
          SyncAudioRenderer();
          _basicAudio = (IBasicAudio)_graphBuilder;
        }
        if (!_isRadio)
        {
          if (filterConfig != null && filterConfig.enableCCSubtitles)
          {
            CleanupCC();
            ReleaseCC();
            ReleaseCC2();
            CoreCCParserCheck();
            DirectShowUtil.RenderUnconnectedOutputPins(_graphBuilder, filterCodec.VideoCodec);
            Log.Debug("TSReaderPlayer: Render VideoCodec filter (Tv/Recorded Stream Detected)");
            EnableCC();
            Log.Debug("TSReaderPlayer: EnableCC");
            if (CoreCCPresent)
            {
              DirectShowUtil.RenderUnconnectedOutputPins(_graphBuilder, filterCodec.CoreCCParser);
              Log.Debug("TSReaderPlayer: Render CoreCCParser filter (Tv/Recorded Stream Detected)");
              EnableCC2();
              Log.Debug("TSReaderPlayer: EnableCC2");
            }
          }
          if (!_vmr9.IsVMR9Connected)
          {
            Log.Error("TSReaderPlayer: Failed vmr9 not connected");
            Cleanup();
            return false;
          }
          DirectShowUtil.EnableDeInterlace(_graphBuilder);
          _vmr9.SetDeinterlaceMode();
        }

        using (MPSettings xmlreader = new MPSettings())
        {
          int lastSubIndex = xmlreader.GetValueAsInt("tvservice", "lastsubtitleindex", 0);
          Log.Debug("TSReaderPlayer: Last subtitle index: {0}", lastSubIndex);
          CurrentSubtitleStream = lastSubIndex;
        }

        if (filterConfig != null && !filterConfig.autoShowSubWhenTvStarts)
        {
          Log.Debug("TSReaderPlayer: Automatically show subtitles when TV starts is set to {0}", filterConfig.autoShowSubWhenTvStarts);
          EnableSubtitle = filterConfig.autoShowSubWhenTvStarts;
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

    protected override void CoreCCParserCheck()
    {
      #region find Core CC Parser
      if (filterCodec.CoreCCParser != null)
      {
        DirectShowUtil.ReleaseComObject(filterCodec.CoreCCParser);
        filterCodec.CoreCCParser = null;
      }
      filterCodec.CoreCCParser = DirectShowUtil.GetFilterByName(_graphBuilder, "Core CC Parser");
      if (filterCodec.CoreCCParser == null && _videoFormat.streamType == VideoStreamType.MPEG2 && !CoreCCParserH264)
      {
        if (filterConfig != null && filterConfig.OtherFilters.Contains("Core CC Parser") && filterConfig.enableCCSubtitles)
        {
          filterCodec.CoreCCParser = DirectShowUtil.AddFilterToGraph(_graphBuilder, "Core CC Parser");
          CoreCCPresent = true;
        }
      }
      else if (filterCodec.CoreCCParser == null && CoreCCParserH264)
      {
        if (filterConfig != null && filterConfig.OtherFilters.Contains("Core CC Parser") && filterConfig.enableCCSubtitles)
        {
          filterCodec.CoreCCParser = DirectShowUtil.AddFilterToGraph(_graphBuilder, "Core CC Parser");
          CoreCCPresent = true;
        }
      }
      else
      {
        if (filterCodec.CoreCCParser != null && _videoFormat.streamType == VideoStreamType.MPEG2 && !CoreCCParserH264)
        {
          CoreCCPresent = true;
        }
        else if (filterCodec.CoreCCParser != null && CoreCCParserH264)
        {
          CoreCCPresent = true;
        }
        else if (filterCodec.CoreCCParser != null && _videoFormat.streamType == VideoStreamType.H264 && !CoreCCParserH264)
        {
          _graphBuilder.RemoveFilter(filterCodec.CoreCCParser);
          DirectShowUtil.ReleaseComObject(filterCodec.CoreCCParser);
          filterCodec.CoreCCParser = null;
        }
      }
      #endregion
    }

    protected override void CleanupCC()
    {
      //Need to remove and release each Line21 in the graph for both Class ID.
      while (true)
      {
        IBaseFilter basefilter;
        DirectShowUtil.FindFilterByClassID(_graphBuilder, ClassId.Line21_1, out basefilter);
        if (basefilter == null)
          DirectShowUtil.FindFilterByClassID(_graphBuilder, ClassId.Line21_2, out basefilter);
        if (basefilter != null)
        {
          _graphBuilder.RemoveFilter(basefilter);
          DirectShowUtil.ReleaseComObject(basefilter);
          basefilter = null;
          Log.Info("TSReaderPlayer: Cleanup Captions");
        }
        else
        {
          break;
        }
      }
    }

    protected override void ReleaseCC()
    {
      //Cleanup Line 21 Analog
      /*int hr;
      if (_mediaCtrl != null)
      hr = _mediaCtrl.Stop();*/

      if (filterCodec.line21VideoCodec != null)
      {
        DirectShowUtil.ReleaseComObject(filterCodec.line21VideoCodec);
        filterCodec.line21VideoCodec = null;
      }
      if (_line21DecoderAnalog != null)
      {
        DirectShowUtil.ReleaseComObject(_line21DecoderAnalog);
        _line21DecoderAnalog = null;
      }
    }

    protected override void ReleaseCC2()
    {
      //Cleanup Line 21 Digital
      /*int hr;
      if (_mediaCtrl != null)
      hr = _mediaCtrl.Stop();*/

      if (filterCodec.line21CoreCCParser != null)
      {
        DirectShowUtil.ReleaseComObject(filterCodec.line21CoreCCParser);
        filterCodec.line21CoreCCParser = null;
      }
      if (_line21DecoderDigital != null)
      {
        DirectShowUtil.ReleaseComObject(_line21DecoderDigital);
        _line21DecoderDigital = null;
      }
    }

    protected override void EnableCC()
    {
      if (!_isRadio)
      {
        IPin pinFromVideo = DsFindPin.ByDirection((IBaseFilter)filterCodec.VideoCodec, PinDirection.Output, 1);
        if (pinFromVideo != null)
        {
          IPin pinToFilter;
          int hr = pinFromVideo.ConnectedTo(out pinToFilter);
          if (hr >= 0 && pinToFilter != null)
          {
            PinInfo pInfo;
            pinToFilter.QueryPinInfo(out pInfo);
            FilterInfo fInfo;
            pInfo.filter.QueryFilterInfo(out fInfo);
            if (!fInfo.achName.Contains("Enhanced Video Renderer") && !fInfo.achName.Contains("Video Mixing Renderer 9"))
            {
              Log.Debug("TSReaderPlayer: Enable loading line21VideoCodec - {0}", fInfo.achName);
              filterCodec.line21VideoCodec = DirectShowUtil.GetFilterByName(_graphBuilder, fInfo.achName);
            }
            DsUtils.FreePinInfo(pInfo);
            DirectShowUtil.ReleaseComObject(fInfo.pGraph); //Sebastiii Test
            DirectShowUtil.ReleaseComObject(pinToFilter); pinToFilter = null;
          }
          DirectShowUtil.ReleaseComObject(pinFromVideo); pinFromVideo = null;

          _line21DecoderAnalog = (IAMLine21Decoder)filterCodec.line21VideoCodec;
          if (_line21DecoderAnalog != null)
          {
            AMLine21CCState state = AMLine21CCState.Off;
            hr = _line21DecoderAnalog.SetServiceState(state);
            if (hr == 0)
            {
              Log.Info("TSReaderPlayer: Closed Captions Analog state change successful");
            }
            else
            {
              Log.Info("TSReaderPlayer: Failed to change Closed Captions Analog state");
            }
          }
        }
      }
      using (MPSettings xmlreader = new MPSettings())
      {
        int lastSubIndex = xmlreader.GetValueAsInt("tvservice", "lastsubtitleindex", 0);
        Log.Debug("TSReaderPlayer: Last subtitle index: {0}", lastSubIndex);
        CurrentSubtitleStream = lastSubIndex;
      }
    }

    protected override void EnableCC2()
    {
      if (!_isRadio)
      {
        if (CoreCCPresent)
        {
          IPin pinFromSplitter = DsFindPin.ByDirection((IBaseFilter)filterCodec.CoreCCParser, PinDirection.Output, 1);
          IPin pinToFilterCC;
          int hr = pinFromSplitter.ConnectedTo(out pinToFilterCC);
          if (hr >= 0 && pinToFilterCC != null)
          {
            PinInfo pInfo;
            pinToFilterCC.QueryPinInfo(out pInfo);
            FilterInfo fInfo;
            pInfo.filter.QueryFilterInfo(out fInfo);
            if (!fInfo.achName.Contains("Enhanced Video Renderer") && !fInfo.achName.Contains("Video Mixing Renderer 9"))
            {
              Log.Debug("TSReaderPlayer: Enable loading line21CoreCCParser - {0}", fInfo.achName);
              filterCodec.line21CoreCCParser = DirectShowUtil.GetFilterByName(_graphBuilder, fInfo.achName);
            }
            DsUtils.FreePinInfo(pInfo);
            DirectShowUtil.ReleaseComObject(fInfo.pGraph); //Sebastiii Test
            DirectShowUtil.ReleaseComObject(pinToFilterCC); pinToFilterCC = null;
          }
          DirectShowUtil.ReleaseComObject(pinFromSplitter); pinFromSplitter = null;

          _line21DecoderDigital = (IAMLine21Decoder)filterCodec.line21CoreCCParser;
          if (_line21DecoderDigital != null)
          {
            AMLine21CCState stateCC2 = AMLine21CCState.Off;
            hr = _line21DecoderDigital.SetServiceState(stateCC2);
            if (hr == 0)
            {
              Log.Info("TSReaderPlayer: Closed Captions Digital state change successful");
            }
            else
            {
              Log.Info("TSReaderPlayer: Failed to change Closed Captions Digital state");
            }
          }
        }
      }
      using (MPSettings xmlreader = new MPSettings())
      {
        int lastSubIndex = xmlreader.GetValueAsInt("tvservice", "lastsubtitleindex", 0);
        Log.Debug("TSReaderPlayer: Last subtitle index: {0}", lastSubIndex);
        CurrentSubtitleStream = lastSubIndex;
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
        Log.Info("TSReaderPlayer: Cleanup DShow graph {0}", GUIGraphicsContext.InVmr9Render);
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

          if (_vmr9 != null)
          {
            _vmr9.Enable(false);
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
            Log.Info("TSReaderPlayer: Cleanup Get hr value {0}", hr);
            hr = _videoWin.put_Owner(IntPtr.Zero);
            _videoWin = null;
          }

          _mediaSeeking = null;
          _basicAudio = null;
          _basicVideo = null;
          _ireader = null;

          if (filterCodec != null && filterCodec._audioRendererFilter != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(filterCodec._audioRendererFilter)) > 0) ;
            //DirectShowUtil.ReleaseComObject(filterCodec._audioRendererFilter, 5000);
            filterCodec._audioRendererFilter = null;
            Log.Debug("TSReaderPlayer: Cleanup _audioRendererFilter");
          }

          if (_fileSource != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(_fileSource)) > 0) ;
            //DirectShowUtil.ReleaseComObject(_fileSource, 5000);
            _fileSource = null;
            Log.Debug("TSReaderPlayer: Cleanup _fileSource");
          }

          PostProcessingEngine.GetInstance().FreePostProcess();
          Log.Debug("TSReaderPlayer: Cleanup FreePostProcess");

          //ReleaseComObject from PostProcessFilter list objects.
          foreach (var ppFilter in PostProcessFilterVideo)
          {
            if (ppFilter.Value != null)
              while ((hr = DirectShowUtil.ReleaseComObject(ppFilter.Value)) > 0) ; 
            //DirectShowUtil.ReleaseComObject(ppFilter.Value, 5000);
          }
          PostProcessFilterVideo.Clear();
          foreach (var ppFilter in PostProcessFilterAudio)
          {
            if (ppFilter.Value != null)
              while ((hr = DirectShowUtil.ReleaseComObject(ppFilter.Value)) > 0) ;
            //DirectShowUtil.ReleaseComObject(ppFilter.Value, 5000);
          }
          PostProcessFilterAudio.Clear();
          Log.Debug("TSReaderPlayer: Cleanup PostProcess");

          if (filterCodec != null && filterCodec._audioSwitcherFilter != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(filterCodec._audioSwitcherFilter)) > 0) ;
            //DirectShowUtil.ReleaseComObject(filterCodec._audioSwitcherFilter, 5000);
            filterCodec._audioSwitcherFilter = null;
            Log.Debug("TSReaderPlayer: Cleanup _audioSwitcherFilter");
          }

          if (_line21DecoderAnalog != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(_line21DecoderAnalog)) > 0) ;
            //DirectShowUtil.ReleaseComObject(_line21DecoderAnalog, 5000);
            _line21DecoderAnalog = null;
            Log.Debug("TSReaderPlayer: Cleanup _line21DecoderAnalog");
          }

          if (_line21DecoderDigital != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(_line21DecoderDigital)) > 0) ;
            //hr = DirectShowUtil.ReleaseComObject(_line21DecoderDigital, 5000);
            _line21DecoderDigital = null;
            Log.Debug("TSReaderPlayer: Cleanup _line21DecoderDigital");
          }

          if (filterCodec != null && filterCodec.CoreCCParser != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(filterCodec.CoreCCParser)) > 0) ;
            //DirectShowUtil.ReleaseComObject(filterCodec.CoreCCParser, 5000);
            filterCodec.CoreCCParser = null;
            Log.Debug("TSReaderPlayer: Cleanup CoreCCParser");
          }

          if (filterCodec != null && filterCodec.AudioCodec != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(filterCodec.AudioCodec)) > 0) ;
            //DirectShowUtil.ReleaseComObject(filterCodec.AudioCodec, 5000);
            filterCodec.AudioCodec = null;
            Log.Debug("TSReaderPlayer: Cleanup AudioCodec");
          }

          if (filterCodec != null && filterCodec.VideoCodec != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(filterCodec.VideoCodec)) > 0) ;
            //DirectShowUtil.ReleaseComObject(filterCodec.VideoCodec, 5000);
            filterCodec.VideoCodec = null;
            Log.Debug("TSReaderPlayer: Cleanup VideoCodec");
          }

          if (filterCodec != null && filterCodec.line21VideoCodec != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(filterCodec.line21VideoCodec)) > 0) ;
            //DirectShowUtil.ReleaseComObject(filterCodec.line21VideoCodec, 5000);
            filterCodec.line21VideoCodec = null;
            Log.Debug("TSReaderPlayer: Cleanup line21VideoCodec");
          }

          if (filterCodec != null && filterCodec.line21CoreCCParser != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(filterCodec.line21CoreCCParser)) > 0) ;
            //DirectShowUtil.ReleaseComObject(filterCodec.line21CoreCCParser, 5000);
            filterCodec.line21CoreCCParser = null;
            Log.Debug("TSReaderPlayer: Cleanup line21CoreCCParser");
          }
          if (_graphBuilder != null)
          {
            DirectShowUtil.RemoveFilters(_graphBuilder);
            if (_rotEntry != null)
            {
              _rotEntry.SafeDispose();
              _rotEntry = null;
            }
            while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0) ;
            //DirectShowUtil.ReleaseComObject(_graphBuilder, 5000);
            _graphBuilder = null;
            Log.Debug("TSReaderPlayer: Cleanup _graphBuilder");
          }

          if (_dvbSubRenderer != null)
          {
            _dvbSubRenderer.SetPlayer(null);
            _dvbSubRenderer = null;
          }

          if (_vmr9 != null)
          {
            _vmr9.SafeDispose();
            _vmr9 = null;
          }

          GUIGraphicsContext.form.Invalidate(true);
          _state = PlayState.Init;
        }
        catch (Exception ex)
        {
          Log.Error("TSReaderPlayer: Exception while cleaning DShow graph - {0} {1}", ex.Message, ex.StackTrace);
        }

        try
        {
            using (MPSettings xmlreader = new MPSettings())
                xmlreader.SetValue("tvservice", "dvbdefttxtsubtitles", "999;999");
        }
        catch { }

        //switch back to directx windowed mode
        //Log.Info("TSReaderPlayer: Disabling DX9 exclusive mode Cleanup");
        //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        //GUIWindowManager.SendMessage(msg);
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

            while (SeekTries > 0)
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
                Log.Info("TsReaderPlayer seek again : pos: {0} lower than start:{1} end:{2} ( Cnt {3} )", lStreamPos,
                         lContentStart, lContentEnd, SeekTries);
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
        if (SupportsCC && (UseCC || UseCC2))
        {
          return -1;
        }
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
        if (SupportsCC)
        {
          if (value == -1)
          {
            UseCC2 = true;
            UseCC = true;
            FFDShowEngine.EnableFFDShowSubtitles(_graphBuilder);
            if (_subSelector != null)
            {
              _dvbSubRenderer.RenderSubtitles = false;
              return;
            }
          }
          else
          {
            UseCC = false;
            UseCC2 = false;
            FFDShowEngine.DisableFFDShowSubtitles(_graphBuilder);
            if (_subSelector != null)
            {
              _dvbSubRenderer.RenderSubtitles = true;
            }
          }
        }

        if (_subSelector != null)
        {
          if (value != -1)
          _subSelector.SetOption(value);
        }
      }
    }

    /// <summary>
    /// Property to get the language for a subtitle stream
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
    /// Property to get the name for a subtitle stream
    /// </summary>
    public override string SubtitleName(int iStream)
    {
      return SubtitleLanguage(iStream);
    }

    /// <summary>
    /// Property to enable/disable subtitles
    /// </summary>
    public override bool EnableSubtitle
    {
      get
      {
        if (SupportsCC && (UseCC || UseCC2))
        {
          return true;
        }
        else if (_subSelector != null)
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
        if (SupportsCC)
        {
          if (CurrentSubtitleStream == -1)
          {
            if (_subSelector != null)
            {
              _dvbSubRenderer.RenderSubtitles = false;
            }
            UseCC = value;
            UseCC2 = value;
            if (value)
            {
              FFDShowEngine.EnableFFDShowSubtitles(_graphBuilder);
            }
            else
            {
              FFDShowEngine.DisableFFDShowSubtitles(_graphBuilder);
            }
          }
          else
          {
            if (_subSelector != null)
            {
              _dvbSubRenderer.RenderSubtitles = value;
            }
          }
        }

        else if (_subSelector != null)
        {
          _dvbSubRenderer.RenderSubtitles = value;
          if (value)
          {
            FFDShowEngine.EnableFFDShowSubtitles(_graphBuilder);
          }
          else
          {
            FFDShowEngine.DisableFFDShowSubtitles(_graphBuilder);

          }
        }
      }
    }

    public bool SupportsCC
    {
      get
      {
        if (filterConfig != null && filterConfig.enableCCSubtitles)
        {
          return (_line21DecoderDigital != null || _line21DecoderAnalog != null);
        }
        else
          return false;
      }
    }

    public bool UseCC
    {
      get
      {
        if (_line21DecoderAnalog == null)
        {
          return false;
        }
        AMLine21CCState state;
        _line21DecoderAnalog.GetServiceState(out state);
        return (state == AMLine21CCState.On);
      }
      set
      {
        if (_line21DecoderAnalog != null)
        {
          AMLine21CCState state = AMLine21CCState.Off;
          if (value)
          {
            state = AMLine21CCState.On;
          }
          int hr = _line21DecoderAnalog.SetServiceState(state);
          if (hr == 0)
          {
            Log.Info("TSReaderPlayer: Closed Captions Analog state change successful");
          }
          else
          {
            Log.Info("TSReaderPlayer: Failed to change Closed Captions state");
          }
        }
      }
    }

    public bool UseCC2
    {
      get
      {
        if (_line21DecoderDigital == null)
        {
          return false;
        }
        AMLine21CCState stateCC2;
        _line21DecoderDigital.GetServiceState(out stateCC2);
        return (stateCC2 == AMLine21CCState.On);
      }
      set
      {
        if (_line21DecoderDigital != null)
        {
          AMLine21CCState stateCC2 = AMLine21CCState.Off;
          if (value)
          {
            stateCC2 = AMLine21CCState.On;
          }
          int hr = _line21DecoderDigital.SetServiceState(stateCC2);
          if (hr == 0)
          {
            Log.Info("TSReaderPlayer: Closed Captions Digital state change successful");
          }
          else
          {
            Log.Info("TSReaderPlayer: Failed to change Closed Captions state");
          }
        }
      }
    }

    /// <summary>
    /// Property to Get Postprocessing
    /// </summary>
    public override bool HasPostprocessing
    {
      get { return PostProcessingEngine.GetInstance().HasPostProcessing; }
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
      try
      {
          using (MPSettings xmlreader = new MPSettings())
              xmlreader.SetValue("tvservice", "dvbdefttxtsubtitles", "999;999");
      }
      catch { }
    }
  }
}