#region Copyright (C) 2005-2024 Team MediaPortal

// Copyright (C) 2005-2024 Team MediaPortal
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
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Player.LAV;
using MediaPortal.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.Player.Subtitles;
using MediaPortal.Player.PostProcessing;
using System.Globalization;

namespace MediaPortal.Player
{
  public class OnlinePlayer : VideoPlayerVMR9
  {
    #region Constants
    private const string _SOURCE_FILTER_NAME = "LAV Splitter Source";

    const bool LAV_ALWAYS_PREBUFFER = false; //set to true if we wants to always prebuffer at start or after seek
    const int LAV_MIN_BUFFER_LEVEL = 5; //[%]; when frame duration is unknown
    const double LAV_MIN_BUFFER_TIME = 0.5; //[s]
    const int LAV_BUFFERED_LEVEL = 33; //[%]; when frame duration is unknown
    const double LAV_BUFFERED_TIME = 5.0; //[s]
    const double LAV_MIN_BUFFER_TOTAL_TIME = 20.0; //[s]; set LAV buffer size at least to this value(if frame duration is known)
    const double LAV_DEFAULT_BUFFER_TOTAL_TIME = 10.0; //[s]; when frame duration is unknown

    private static readonly Guid _GUID_MEDIASUBTYPE_AVC1 = new Guid("31435641-0000-0010-8000-00aa00389b71");
    #endregion

    #region Types
    private enum BufferStatusEnum
    {
      BufferingNeeded = -1,
      AboveMinLevel = 0,
      BufferedEnough = 1
    };
    #endregion

    #region Private fields
    private string _VideoDecoder = null;
    private string _AudioDecoder = null;
    private string _AudioRenderer = null;

    private OnlinePlayerUrl _MixedUrl = null;
    private int _CurrentAudioStream = 0;
    private int _InternalAudioStreams = 0;


    private float _PercentageBuffered;
    private DateTime _LastProcessCheck = DateTime.MinValue;


    private int _LavMaxQueue = -1;
    private double _LavMaxQueueTime = -1;
    private int _LavMaxQueueMemSize = -1;
    private IBaseFilter _SourceFilter;
    private string _SourceFilterName;
    private int _SourceFilterVideoPinIndex = -1;
    private double _VideoSampleDuration = -1;
    private DateTime _LastProgressCheck;
    private bool _Buffering = false;
    private double _LastCurrentPosition = 0.0d;
    private bool _PlaybackDetected = false;
    private bool _PreBufferNeeded = true;
    private DateTime _SeekTimeStamp = DateTime.MinValue;
    private static CultureInfo _Culture_EN = CultureInfo.GetCultureInfo("en-US");
    #endregion

    #region Overrides
    protected override bool GetInterfaces()
    {
      if (!this.prepareGraph())
        return false;

      if (!this.loadSourceFilter())
        return false;

      return this.finishPreparedGraph();
    }

    public override void Process()
    {
      if ((DateTime.Now - this._LastProcessCheck).TotalMilliseconds > 100) // check progress at maximum 10 times per second
      {
        this._LastProcessCheck = DateTime.Now;

        //LAV buffer monitoring
        this.processLavBufferLevel();
      }

      base.Process();
    }

    public override bool Play(string strFile)
    {
      tagsClear();

      this.updateTimer = DateTime.Now;
      this.m_speedRate = 10000;
      this.m_bVisible = false;
      this.m_iVolume = 100;
      this.m_state = PlayState.Init;

      this.m_strCurrentFile = strFile;

      this._MixedUrl = null;
      OnlinePlayerUrl mixedUrl = new OnlinePlayerUrl(this.m_strCurrentFile);
      if (mixedUrl.Valid)
        this._MixedUrl = mixedUrl;

      this.m_bFullScreen = true;
      this.m_ar = GUIGraphicsContext.ARType;
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      this._updateNeeded = true;
      Log.Info("OnlinePlayer: Play '{0}'", this.m_strCurrentFile);

      this.m_bStarted = false;
      if (!this.GetInterfaces())
      {
        this.m_strCurrentFile = string.Empty;
        this.CloseInterfaces();
        return false;
      }

      //Download subtitles if needed
      string strSubFile = null;
      if (this._MixedUrl?.SubtitleTracks?.Length > 0)
      {
        List<object[]> paths = new List<object[]>();
        StringBuilder sb = new StringBuilder(64);
        strSubFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MediaPortalSubtitles");
        for (int i = 0; i < this._MixedUrl.SubtitleTracks.Length; i++)
        {
          OnlinePlayerUrl.Track track = this._MixedUrl.SubtitleTracks[i];

          sb.Clear();
          sb.Append(strSubFile);

          //Language suffix
          if (!string.IsNullOrWhiteSpace(track.Language))
          {
            sb.Append('.');
            sb.Append(track.Language);
          }

          //Check for existing path
          string strFilename = sb.ToString();
          object[] pair = paths.Find(p => ((string)p[0]).Equals(strFilename, StringComparison.CurrentCultureIgnoreCase));
          if (pair != null)
          {
            //Add number suffix
            int iCnt = (int)pair[1] + 1;
            pair[1] = iCnt;
            sb.Append('.');
            sb.Append(iCnt);
            strFilename = sb.ToString();
          }
          else
            paths.Add(new object[] { strFilename, 1 });
          sb.Append(".txt");
          strFilename = sb.ToString();

          Utils.Web.HTTPTransaction http = new Utils.Web.HTTPTransaction();
          if (!http.HTTPGet(new Utils.Web.HTTPRequest(track.Url), strFilename))
            Log.Error("OnlinePlayer: Play: Failed to download subtitle: {0}", track.Url);
        }

        //Virtual video file for subtitle engine
        strSubFile += ".mp4";
      }

      ISubEngine engine = SubEngine.GetInstance(true);
      if (strSubFile != null)
      {
        if (!engine.LoadSubtitles(this.graphBuilder, strSubFile))
          SubEngine.engine = new SubEngine.DummyEngine();
      }
      else if (string.IsNullOrWhiteSpace(this._MixedUrl?.SubtitleFile) || !engine.LoadSubtitles(this.graphBuilder, this._MixedUrl.SubtitleFile))
        SubEngine.engine = new SubEngine.DummyEngine();
      else
        engine.Enable = true;

      IPostProcessingEngine postengine = PostProcessingEngine.GetInstance(true);
      if (!postengine.LoadPostProcessing(this.graphBuilder))
        PostProcessingEngine.engine = new PostProcessingEngine.DummyEngine();

      IAudioPostEngine audioEngine = AudioPostEngine.GetInstance(true);
      if (audioEngine != null && !audioEngine.LoadPostProcessing(this.graphBuilder))
        AudioPostEngine.engine = new AudioPostEngine.DummyEngine();

      if (this._audioSwitcher != null)
      {
        this.analyseStreams();

        if (this._MixedUrl.DefaultAudio >= 0 && this._MixedUrl.DefaultAudio < this._MixedUrl.AudioTracks.Length)
          this.CurrentAudioStream = this._MixedUrl.DefaultAudio;
        else
          this.SelectAudioLanguage();
      }
      else
      {
        this.AnalyseStreams();
        this.SelectAudioLanguage();
      }

      this.SelectSubtitles();
      this.OnInitialized();

      //Refresh changer (try to get video fps from decoder if needed)
      RefreshRateChanger.AdaptRefreshRateFromVideoDecoder(strFile);


      int hr = this.mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero);
      if (hr < 0)
      {
        Error.SetError("Unable to play movie", "Can not set notifications");
        this.m_strCurrentFile = string.Empty;
        this.CloseInterfaces();
        return false;
      }
      if (this.videoWin != null)
      {
        this.videoWin.put_WindowStyle((WindowStyle)((int)WindowStyle.Child + (int)WindowStyle.ClipChildren + (int)WindowStyle.ClipSiblings));
        this.videoWin.put_MessageDrain(GUIGraphicsContext.form.Handle);
      }

      DirectShowUtil.SetARMode(this.graphBuilder, AspectRatioMode.Stretched);

      try
      {
        hr = this.mediaCtrl.Run();
        DsError.ThrowExceptionForHR(hr);
        if (hr == 1)
        // S_FALSE from IMediaControl::Run means: The graph is preparing to run, but some filters have not completed the transition to a running state.
        {
          // wait max. 20 seconds for the graph to transition to the running state
          DateTime startTime = DateTime.Now;

          do
          {
            Thread.Sleep(100);
            hr = this.mediaCtrl.GetState(100, out FilterState filterState);
            // check with timeout max. 10 times a second if the state changed
          } while ((hr != 0) && ((DateTime.Now - startTime).TotalSeconds <= 20));

          if (hr != 0) // S_OK
          {
            DsError.ThrowExceptionForHR(hr);
            throw new Exception(string.Format("IMediaControl.GetState after 20 seconds: 0x{0} - '{1}'",
                hr.ToString("X8"), DsError.GetErrorText(hr)));
          }
        }
      }
      catch (Exception error)
      {
        Log.Warn("OnlinePlayer: Play: Unable to play with reason: {0}", error.Message);
      }
      if (hr != 0) // S_OK
      {
        Error.SetError("Unable to play movie", "Unable to start movie");
        this.m_strCurrentFile = string.Empty;
        this.CloseInterfaces();
        return false;
      }

      if (this.basicVideo != null)
        this.basicVideo.GetVideoSize(out this.m_iVideoWidth, out this.m_iVideoHeight);

      //Stream tags init
      GUIPropertyManager.SetProperty("#Play.Current.Stream.Audio.Total", this.AudioStreams.ToString());
      GUIPropertyManager.SetProperty("#Play.Current.Stream.Audio.IsMulti", (this.AudioStreams > 1).ToString());

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
      msg.Label = this.CurrentFile;
      GUIWindowManager.SendThreadMessage(msg);
      this.m_state = PlayState.Playing;
      this.m_iPositionX = GUIGraphicsContext.VideoWindow.X;
      this.m_iPositionY = GUIGraphicsContext.VideoWindow.Y;
      this.m_iWidth = GUIGraphicsContext.VideoWindow.Width;
      this.m_iHeight = GUIGraphicsContext.VideoWindow.Height;
      this.m_ar = GUIGraphicsContext.ARType;
      this._updateNeeded = true;
      this.SetVideoWindow();
      this.mediaPos.get_Duration(out this.m_dDuration);
      Log.Info("OnlinePlayer: Play: Duration {0} sec", this.m_dDuration.ToString("F"));

      return true;
    }

    public override void Stop()
    {
      Log.Info("OnlinePlayer: Stop");
      this.m_strCurrentFile = string.Empty;
      this.disposeSourceFilter();
      this.CloseInterfaces();
      this.m_state = PlayState.Init;
      GUIGraphicsContext.IsPlaying = false;
    }

    public override void Dispose()
    {
      base.Dispose();
      GUIPropertyManager.SetProperty("#TV.Record.percent3", 0.0f.ToString());
      tagsClear();
    }

    public override int CurrentAudioStream
    {
      get
      {
        return this._audioSwitcher != null ? this._CurrentAudioStream : base.CurrentAudioStream;
      }

      set
      {
        this.disposeSourceFilter();

        if (this._audioSwitcher != null)
        {
          if (this._CurrentAudioStream != value)
          {
            Log.Debug("OnlinePlayer: CurrentAudioStream:{0} Request:{1}", this._CurrentAudioStream, value);

            int iIdxSwitcher = -1;
            bool bResult = false;

            if (this._InternalAudioStreams == 0)
              iIdxSwitcher = value; //external only
            else if (value >= this._InternalAudioStreams)
              iIdxSwitcher = value - this._InternalAudioStreams + 1; //to external
            else
            {
              //to internal
              FilterStreamInfos fsi = FStreams.GetStreamInfos(StreamType.Audio, value);
              if (!(bResult = EnableStream(fsi.Id, AMStreamSelectEnableFlags.Enable, fsi.Filter)))
                goto log;

              if (this._CurrentAudioStream >= this._InternalAudioStreams)
                iIdxSwitcher = 0; //Switch to first input of AudioSwitcher(internal audio)
            }

            if (iIdxSwitcher >= 0)
            {
              Log.Debug("OnlinePlayer: CurrentAudioStream: Switching AudioSwitcher to input: {0}", iIdxSwitcher);
              bResult = this.EnableStream(iIdxSwitcher, AMStreamSelectEnableFlags.Enable, MEDIAPORTAL_AUDIOSWITCHER_FILTER);
            }

            if (bResult)
              this._CurrentAudioStream = value;
            log:
            Log.Info("OnlinePlayer: CurrentAudioStream: CurrentAudioStream:{0} Result:{1}", value, bResult);
          }
        }
        else
          base.CurrentAudioStream = value;
      }
    }

    public override int AudioStreams
    {
      get
      {
        if (this._audioSwitcher != null)
          return this._MixedUrl.AudioTracks.Length + this._InternalAudioStreams;

        return base.AudioStreams;
      }
    }

    public override string AudioType(int iStream)
    {
      return null;
    }

    public override string AudioLanguage(int iStream)
    {
      if (this._audioSwitcher != null && (this._InternalAudioStreams == 0 || iStream >= this._InternalAudioStreams))
      {
        iStream -= this._InternalAudioStreams;

        CultureInfo ciTrack = null;
        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

        string strLanguage = this._MixedUrl.AudioTracks[iStream].Language;

        if (!string.IsNullOrWhiteSpace(strLanguage))
        {
          if (strLanguage.Length >= 5 && strLanguage[2] == '-')
            strLanguage = strLanguage.Substring(0, 2);

          ciTrack = cultures.FirstOrDefault(ci => ci.Name.Equals(strLanguage, StringComparison.OrdinalIgnoreCase)
              || ci.ThreeLetterISOLanguageName.Equals(strLanguage, StringComparison.OrdinalIgnoreCase)
              || ci.EnglishName.Equals(strLanguage, StringComparison.OrdinalIgnoreCase)
              );
        }

        if (ciTrack == null)
          strLanguage = "Undetermined";
        else
          strLanguage = ciTrack.EnglishName;

        strLanguage = Util.Utils.TranslateLanguageString(strLanguage);

        if (!string.IsNullOrWhiteSpace(this._MixedUrl.AudioTracks[iStream].Description))
          strLanguage += " [" + this._MixedUrl.AudioTracks[iStream].Description + ']';

        return strLanguage;
      }

      return base.AudioLanguage(iStream);
    }

    public override void SeekAbsolute(double dTime)
    {
      //this.onBuffer();
      base.SeekAbsolute(dTime);
      this.onSeek();
    }

    public override void SeekAsolutePercentage(int iPercentage)
    {
      //this.onBuffer();
      base.SeekAsolutePercentage(iPercentage);
      this.onSeek();
    }

    public override void SeekRelative(double dTime)
    {
      //this.onBuffer();
      base.SeekRelative(dTime);
      this.onSeek();
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      //this.onBuffer();
      base.SeekRelativePercentage(iPercentage);
      this.onSeek();
    }

    #endregion

    #region Private methods
    private bool prepareGraph()
    {
      string strSourceFilterName = _SOURCE_FILTER_NAME;

      if (!string.IsNullOrEmpty(strSourceFilterName))
      {
        this.graphBuilder = (IGraphBuilder)new FilterGraph();
        this._rotEntry = new DsROTEntry(this.graphBuilder);

        this.basicVideo = this.graphBuilder as IBasicVideo2;

        this.Vmr9 = new VMR9Util();
        this.Vmr9.AddVMR9(this.graphBuilder);
        this.Vmr9.Enable(false);
        // set VMR9 back to NOT Active -> otherwise GUI is not refreshed while graph is building
        GUIGraphicsContext.Vmr9Active = false;

        // add the audio renderer
        using (Settings settings = new MPSettings())
        {
          this._AudioRenderer = settings.GetValueAsString("movieplayer", "audiorenderer", "Default DirectSound Device");
          DirectShowUtil.ReleaseComObject(DirectShowUtil.AddAudioRendererToGraph(this.graphBuilder, this._AudioRenderer, false));
        }

        // set fields for playback
        this.mediaCtrl = (IMediaControl)this.graphBuilder;
        this.mediaEvt = (IMediaEventEx)this.graphBuilder;
        this.mediaSeek = (IMediaSeeking)this.graphBuilder;
        this.mediaPos = (IMediaPosition)this.graphBuilder;
        this.basicAudio = (IBasicAudio)this.graphBuilder;
        this.videoWin = (IVideoWindow)this.graphBuilder;

        // add the source filter
        IBaseFilter sourceFilter = null;
        IBaseFilter sourceFilterAudio = null;
        try
        {
          if (sourceFilter == null)
          {
            sourceFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, strSourceFilterName);

            if (this._MixedUrl != null)
            {
              for (int i = 0; i < this._MixedUrl.AudioTracks.Length; i++)
              {
                sourceFilterAudio = DirectShowUtil.AddFilterToGraph(graphBuilder, strSourceFilterName);
                DirectShowUtil.ReleaseComObject(sourceFilterAudio, 2000);
                sourceFilterAudio = null;

                Log.Debug("OnlinePlayer: prepareGraph() AudioTrack: ID={0}, Default={5}, Language={1}, Description={2}, Filter={3}, Url={4}",
                        i, this._MixedUrl.AudioTracks[i].Language, this._MixedUrl.AudioTracks[i].Description,
                        strSourceFilterName, this._MixedUrl.AudioTracks[i].Url, this._MixedUrl.AudioTracks[i].IsDefault);
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Warn("OnlinePlayer: prepareGraph() Error adding '{0}' filter to graph: {1}", strSourceFilterName, ex.Message);
          return false;
        }
        finally
        {
          if (sourceFilter != null)
            DirectShowUtil.ReleaseComObject(sourceFilter, 2000);

          if (sourceFilterAudio != null)
            DirectShowUtil.ReleaseComObject(sourceFilterAudio, 2000);
        }
        return true;
      }
      else
      {
        return false;
      }
    }

    private bool loadSourceFilter()
    {
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent; // prevents the BlackRectangle on first time playback
      bool bPlaybackReady = false;
      IBaseFilter sourceFilter = null;
      IBaseFilter sourceFilterAudio = null;
      string strSourceFilterName;

      string strUrlVideo = null;
      if (this._MixedUrl != null)
      {
        strUrlVideo = this._MixedUrl.VideoUrl;

        Log.Info("OnlinePlayer: loadSourceFilter() : using mixedurl scheme: urlVideo:'{0}' audioTacks:'{1}'", strUrlVideo, this._MixedUrl.AudioTracks.Length);
      }

      try
      {
        strSourceFilterName = _SOURCE_FILTER_NAME;

        int iResult = this.graphBuilder.FindFilterByName(strSourceFilterName, out sourceFilter);
        if (iResult != 0)
        {
          string strErrorText = DsError.GetErrorText(iResult);
          if (strErrorText != null)
            strErrorText = strErrorText.Trim();
          Log.Warn("OnlinePlayer: loadSourceFilter() : FindFilterByName returned '{0}'{1}", "0x" + iResult.ToString("X8"), !string.IsNullOrEmpty(strErrorText) ? " : (" + strErrorText + ")" : "");
          return false;
        }

        //Explicit audio streams
        if (this._MixedUrl != null)
        {
          for (int i = 0; i < this._MixedUrl.AudioTracks.Length; i++)
          {
            iResult = graphBuilder.FindFilterByName(strSourceFilterName + ' ' + (i + 1).ToString("0000"), out sourceFilterAudio);
            if (iResult != 0)
            {
              string strErrorText = DsError.GetErrorText(iResult);
              if (strErrorText != null)
                strErrorText = strErrorText.Trim();
              Log.Warn("OnlinePlayer: loadSourceFilter() : FindFilterByName returned '{0}'{1}", "0x" + iResult.ToString("X8"), !string.IsNullOrEmpty(strErrorText) ? " : (" + strErrorText + ")" : "");
              return false;
            }

            try
            {
              Marshal.ThrowExceptionForHR(((IFileSourceFilter)sourceFilterAudio).Load(this._MixedUrl.AudioTracks[i].Url, null));
            }
            finally
            {
              DirectShowUtil.ReleaseComObject(sourceFilterAudio);
              sourceFilterAudio = null;
            }
          }
        }

        this.m_strCurrentFile = strUrlVideo;

        //Load source filter with url
        Marshal.ThrowExceptionForHR(((IFileSourceFilter)sourceFilter).Load(this.m_strCurrentFile, null));

        Thread.Sleep(1000);

        //Get video pin index(for LAV buffer processing)
        int iVideoOutputPinId = -1;
        int iOutPinsCounter = 0;
        if (sourceFilter.EnumPins(out IEnumPins pinEnum) == 0)
        {
          IPin[] pins = new IPin[1];
          while (iVideoOutputPinId < 0 && pinEnum.Next(1, pins, out int iFetched) == 0 && iFetched > 0)
          {
            IPin pin = pins[0];
            if (pin.QueryDirection(out PinDirection pinDirection) == 0 && pinDirection == PinDirection.Output)
            {
              if (pin.EnumMediaTypes(out IEnumMediaTypes enumMediaTypesVideo) == 0)
              {
                AMMediaType[] mediaTypes = new AMMediaType[1];
                while (iVideoOutputPinId < 0 && enumMediaTypesVideo.Next(1, mediaTypes, out int iTypesFetched) == 0 && iTypesFetched > 0)
                {
                  if (mediaTypes[0].majorType == MediaType.Video)
                    iVideoOutputPinId = iOutPinsCounter;
                }
                DirectShowUtil.ReleaseComObject(enumMediaTypesVideo);
              }

              iOutPinsCounter++;
            }
            DirectShowUtil.ReleaseComObject(pin);
          }
          DirectShowUtil.ReleaseComObject(pinEnum);
        }

        if (iVideoOutputPinId < 0)
        {
          Log.Error("OnlinePlayer: loadSourceFilter() Failed to get video pin.");
          return false;
        }

        this._SourceFilterVideoPinIndex = iVideoOutputPinId;

        // add audio and video filter from MP Movie Codec setting section
        this.addPreferredFilters(sourceFilter);
        // connect the pin automatically -> will buffer the full file in cases of bad metadata in the file or request of the audio or video filter
        DirectShowUtil.RenderUnconnectedOutputPins(this.graphBuilder, sourceFilter);

        if (sourceFilterAudio != null)
        {
          this.addPreferredFilters(sourceFilterAudio);
          DirectShowUtil.RenderUnconnectedOutputPins(this.graphBuilder, sourceFilterAudio);
        }

        this._PercentageBuffered = 100.0f; // no progress reporting possible
        GUIPropertyManager.SetProperty("#TV.Record.percent3", this._PercentageBuffered.ToString());
        GUIPropertyManager.SetProperty("#Play.Current.Buffer.bufferedenough", "80");
        bPlaybackReady = true;
      }
      catch (ThreadAbortException)
      {
        Thread.ResetAbort();
      }
      catch (COMException comEx)
      {
        Log.Warn(comEx.ToString());

        string strErrorText = DsError.GetErrorText(comEx.ErrorCode);
        if (strErrorText != null)
          strErrorText = strErrorText.Trim();

        if (!string.IsNullOrEmpty(strErrorText))
        {
          throw new Exception(strErrorText);
        }
      }

      catch (Exception ex)
      {
        Log.Warn(ex.ToString());
      }
      finally
      {
        if (sourceFilter != null)
        {
          // playback is not ready but the source filter is already downloading -> abort the operation
          if (!bPlaybackReady)
          {
            Log.Info("OnlinePlayer: Buffering was aborted.");

            if (sourceFilter is IAMOpenProgress progress)
              progress.AbortOperation();

            if (sourceFilterAudio is IAMOpenProgress progressAudio)
              progressAudio.AbortOperation();

            Thread.Sleep(100); // give it some time
            this.graphBuilder.RemoveFilter(sourceFilter); // remove the filter from the graph to prevent lockup later in Dispose
          }

          // release the COM pointer that we created
          DirectShowUtil.ReleaseComObject(sourceFilter);

          if (sourceFilterAudio != null)
          {
            if (!bPlaybackReady)
              this.graphBuilder.RemoveFilter(sourceFilterAudio);

            DirectShowUtil.ReleaseComObject(sourceFilterAudio);
          }
        }
      }

      return bPlaybackReady;
    }

    private bool finishPreparedGraph()
    {
      try
      {
        //Load all decoders and connect them with MPAudioSwitcher(if needed)
        //This has to be done on main thread(due to use of MPAudioSwitcher)
        this.loadDecoderFilters();

        DirectShowUtil.EnableDeInterlace(graphBuilder);

        if (Vmr9 == null || !Vmr9.IsVMR9Connected)
        {
          Log.Warn("OnlinePlayer: finishPreparedGraph() Failed to render file -> No video renderer connected");
          mediaCtrl = null;
          Cleanup();
          return false;
        }

        try
        {
          // remove filter that are not used from the graph
          DirectShowUtil.RemoveUnusedFiltersFromGraph(graphBuilder);
        }
        catch (Exception ex)
        {
          Log.Warn("OnlinePlayer: finishPreparedGraph() Error during RemoveUnusedFiltersFromGraph: {0}", ex.ToString());
        }

        if (Log.GetLogLevel() >= Services.Level.Debug)
        {
          string strSourceFilterName = _SOURCE_FILTER_NAME;
          if (!string.IsNullOrEmpty(strSourceFilterName))
          {
            if (this.graphBuilder.FindFilterByName(strSourceFilterName, out IBaseFilter sourceFilter) == 0 && sourceFilter != null)
              logOutputPinsConnectionRecursive(sourceFilter);

            if (sourceFilter != null)
              DirectShowUtil.ReleaseComObject(sourceFilter);

            if (this._MixedUrl != null)
            {
              for (int i = 0; i < this._MixedUrl.AudioTracks.Length; i++)
              {
                if (this.graphBuilder.FindFilterByName(strSourceFilterName + ' ' + (i + 1).ToString("0000"), out sourceFilter) == 0 && sourceFilter != null)
                  logOutputPinsConnectionRecursive(sourceFilter);

                if (sourceFilter != null)
                {
                  DirectShowUtil.ReleaseComObject(sourceFilter);
                  sourceFilter = null;
                }
              }
            }
          }
        }

        this.Vmr9.SetDeinterlaceMode();

        // now set VMR9 to Active
        GUIGraphicsContext.Vmr9Active = true;

        // set fields for playback                
        this.m_iVideoWidth = this.Vmr9.VideoWidth;
        this.m_iVideoHeight = this.Vmr9.VideoHeight;

        this.Vmr9.SetDeinterlaceMode();
        return true;
      }
      catch (Exception ex)
      {
        Error.SetError("Unable to play movie", "Unable build graph for VMR9");
        Log.Error("OnlinePlayer: finishPreparedGraph() exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
        return false;
      }
    }

    private void addPreferredFilters(IBaseFilter sourceFilter)
    {
      using (Settings xmlreader = new MPSettings())
      {
        bool autodecodersettings = xmlreader.GetValueAsBool("movieplayer", "autodecodersettings", false);

        if (!autodecodersettings) // the user has not chosen automatic graph building by merits
        {
          bool bVc1ICodec = false, bVc1Codec = false, bXvidCodec = false; //- will come later
          bool aacCodec = false;
          bool h264Codec = false;
          bool videoCodec = false;
          bool audioCodec = false;
          bool bHevcCodec = false;

          // check the output pins of the splitter for known media types
          if (sourceFilter.EnumPins(out IEnumPins pinEnum) == 0)
          {
            IPin[] pins = new IPin[1];
            while (pinEnum.Next(1, pins, out int fetched) == 0 && fetched > 0)
            {
              IPin pin = pins[0];
              if (pin.QueryDirection(out PinDirection pinDirection) == 0 && pinDirection == PinDirection.Output)
              {
                if (pin.EnumMediaTypes(out IEnumMediaTypes enumMediaTypesVideo) == 0)
                {
                  AMMediaType[] mediaTypes = new AMMediaType[1];
                  while (enumMediaTypesVideo.Next(1, mediaTypes, out int typesFetched) == 0 && typesFetched > 0)
                  {
                    if (mediaTypes[0].majorType == MediaType.Video)
                    {
                      if (mediaTypes[0].subType == MediaSubType.HEVC)
                      {
                        Log.Info("OnlinePlayer: addPreferredFilters() Found HEVC video on output pin");
                        bHevcCodec = true;
                      }
                      else if (mediaTypes[0].subType == MediaSubType.H264 || mediaTypes[0].subType == _GUID_MEDIASUBTYPE_AVC1)
                      {
                        Log.Info("OnlinePlayer: addPreferredFilters() Found H264 video on output pin");
                        h264Codec = true;
                      }
                      else if (mediaTypes[0].subType == MediaSubType.VC1)
                      {
                        Log.Info("OnlinePlayer: addPreferredFilters() Found VC1 video on output pin");

                        //if (g_Player.MediaInfo.IsInterlaced)
                        //    bVc1ICodec = true;
                        //else
                        bVc1Codec = true;
                      }
                      else if (mediaTypes[0].subType == MediaSubType.XVID || mediaTypes[0].subType == MediaSubType.xvid)
                      {
                        Log.Info("OnlinePlayer: addPreferredFilters() Found xvid video on output pin");
                        bXvidCodec = true;
                      }
                      else
                        videoCodec = true;

                    }
                    else if (mediaTypes[0].majorType == MediaType.Audio)
                    {
                      if (mediaTypes[0].subType == MediaSubType.LATMAAC || mediaTypes[0].subType == MediaSubType.LATMAACLAF)
                      {
                        Log.Info("OnlinePlayer: addPreferredFilters() Found AAC audio on output pin");
                        aacCodec = true;
                      }
                      else
                        audioCodec = true;
                    }
                  }
                  DirectShowUtil.ReleaseComObject(enumMediaTypesVideo);
                }
              }
              DirectShowUtil.ReleaseComObject(pin);
            }
            DirectShowUtil.ReleaseComObject(pinEnum);
          }

          // add filters for found media types to the graph as configured in MP
          if (h264Codec)
          {
            this._VideoDecoder = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(this.graphBuilder, this._VideoDecoder));
          }
          else if (bHevcCodec)
          {
            this._VideoDecoder = xmlreader.GetValueAsString("movieplayer", "hevcvideocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(this.graphBuilder, this._VideoDecoder));
          }
          else if (bXvidCodec)
          {
            this._VideoDecoder = xmlreader.GetValueAsString("movieplayer", "xvidvideocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(this.graphBuilder, this._VideoDecoder));
          }
          else if (bVc1Codec)
          {
            this._VideoDecoder = xmlreader.GetValueAsString("movieplayer", "vc1videocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(this.graphBuilder, this._VideoDecoder));
          }
          else if (bVc1ICodec)
          {
            this._VideoDecoder = xmlreader.GetValueAsString("movieplayer", "vc1ivideocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(this.graphBuilder, _VideoDecoder));
          }
          else if (videoCodec)
          {
            this._VideoDecoder = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(this.graphBuilder, this._VideoDecoder));
          }

          //Get audio decoder
          if (aacCodec || audioCodec)
          {
            if (aacCodec)
              this._AudioDecoder = xmlreader.GetValueAsString("movieplayer", "aacaudiocodec", "");
            else
              this._AudioDecoder = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");

            DirectShowUtil.ReleaseComObject(DirectShowUtil.AddFilterToGraph(this.graphBuilder, this._AudioDecoder));
          }
        }
      }
    }

    private static void logOutputPinsConnectionRecursive(IBaseFilter filter)
    {
      StringBuilder sb = new StringBuilder(128);
      logOutputPinsConnectionRecursive(filter, sb, true);
      Log.Debug(sb.ToString());
    }
    private static void logOutputPinsConnectionRecursive(IBaseFilter filter, StringBuilder sb, bool bIsRoot)
    {
      if (filter.EnumPins(out IEnumPins pinEnum) == 0)
      {
        int iPinCnt = 0;
        filter.QueryFilterInfo(out FilterInfo sourceFilterInfo);
        IPin[] pins = new IPin[1];
        while (pinEnum.Next(1, pins, out int fetched) == 0 && fetched > 0)
        {
          IPin pin = pins[0];
          if (pin.QueryDirection(out PinDirection pinDirection) == 0 && pinDirection == PinDirection.Output)
          {
            if (pin.ConnectedTo(out IPin connectedPin) == 0 && connectedPin != null)
            {
              connectedPin.QueryPinInfo(out PinInfo connectedPinInfo);
              connectedPinInfo.filter.QueryFilterInfo(out FilterInfo connectedFilterInfo);

              DsUtils.FreePinInfo(connectedPinInfo);

              if (sb.Length == 0)
                sb.Append(sourceFilterInfo.achName);

              DirectShowUtil.ReleaseComObject(connectedPin, 2000);
              if (connectedFilterInfo.pGraph.FindFilterByName(connectedFilterInfo.achName, out IBaseFilter connectedFilter) == 0 && connectedFilter != null)
              {
                if (bIsRoot)
                {
                  sb.Append("\r\n#Pin[");
                  sb.Append(iPinCnt);
                  sb.Append("]");
                }

                sb.Append(" --> ");
                sb.Append(connectedFilterInfo.achName);
                logOutputPinsConnectionRecursive(connectedFilter, sb, false);
                DirectShowUtil.ReleaseComObject(connectedFilter);
              }

              DirectShowUtil.ReleaseComObject(connectedFilterInfo.pGraph);
            }
            DirectShowUtil.ReleaseComObject(pin, 2000);
            iPinCnt++;
          }
        }
        DirectShowUtil.ReleaseComObject(sourceFilterInfo.pGraph);
      }
      DirectShowUtil.ReleaseComObject(pinEnum, 2000);
    }

    private void loadDecoderFilters()
    {
      Log.Debug("OnlinePlayer: loadDecoderFilters() Rendering unconnected output pins of source filter ...");

      string strSourceFilterName = _SOURCE_FILTER_NAME;

      IBaseFilter sourceFilter = null;

      int iAudioSwitcherPinIndex = 0;

      try
      {
        this.graphBuilder.FindFilterByName(strSourceFilterName, out sourceFilter);

        this._InternalAudioStreams = this.getNumberOfAudioStreams(sourceFilter);

        //Multiple audio tracks: we need MPAudioSwitcher
        if (this._MixedUrl != null && (this._MixedUrl.AudioTracks.Length > 1 || (this._MixedUrl.AudioTracks.Length > 0 && this._InternalAudioStreams > 0)))
          this._audioSwitcher = DirectShowUtil.AddFilterToGraph(this.graphBuilder, MEDIAPORTAL_AUDIOSWITCHER_FILTER);

        if (this._audioSwitcher != null && this._InternalAudioStreams > 0)
        {
          IBaseFilter audioDecoder = this.addPreferredFilters(this.graphBuilder, sourceFilter, false);
          try
          {
            //Connect sourcefilter's audio output to audio decoder
            IPin pinIn = DsFindPin.ByDirection(audioDecoder, PinDirection.Input, 0);
            this.tryConnect(sourceFilter, pinIn);
            DirectShowUtil.ReleaseComObject(pinIn, 2000);

            //Connect audiodecoder with audioswitcher
            this.connectAudioToSwitcher(audioDecoder, ref iAudioSwitcherPinIndex);
          }
          finally
          {
            DirectShowUtil.ReleaseComObject(audioDecoder);
          }
        }
        else
        {
          // add audio and video filter from MP Movie Codec setting section
          this.addPreferredFilters(this.graphBuilder, sourceFilter);
        }

        // connect the pin automatically -> will buffer the full file in cases of bad metadata in the file or request of the audio or video filter
        DirectShowUtil.RenderUnconnectedOutputPins(this.graphBuilder, sourceFilter);
      }
      finally
      {
        if (sourceFilter != null)
          DirectShowUtil.ReleaseComObject(sourceFilter);
      }

      if (this._MixedUrl != null)
      {
        for (int i = 0; i < this._MixedUrl.AudioTracks.Length; i++)
        {
          try
          {
            this.graphBuilder.FindFilterByName(strSourceFilterName + ' ' + (i + 1).ToString("0000"), out sourceFilter);

            if (this._MixedUrl.AudioTracks.Length == 1)
            {
              this.addPreferredFilters(this.graphBuilder, sourceFilter);
              DirectShowUtil.RenderUnconnectedOutputPins(this.graphBuilder, sourceFilter);
            }
            else
            {
              IBaseFilter audioDecoder = this.addPreferredFilters(this.graphBuilder, sourceFilter, false);
              IPin pinOut = null;
              IPin pinIn = null;
              try
              {
                //Connect source filter with audiodecoder
                pinOut = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, 0);
                pinIn = DsFindPin.ByDirection(audioDecoder, PinDirection.Input, 0);
                Marshal.ThrowExceptionForHR(this.graphBuilder.Connect(pinOut, pinIn));

                //Connect audiodecoder with audioswitcher
                this.connectAudioToSwitcher(audioDecoder, ref iAudioSwitcherPinIndex);
              }
              finally
              {
                if (audioDecoder != null)
                  DirectShowUtil.ReleaseComObject(audioDecoder);

                if (pinOut != null)
                  DirectShowUtil.ReleaseComObject(pinOut, 2000);

                if (pinIn != null)
                  DirectShowUtil.ReleaseComObject(pinIn, 2000);
              }
            }
          }
          finally
          {
            if (sourceFilter != null)
              DirectShowUtil.ReleaseComObject(sourceFilter);
          }
        }

        //Connect audioswitcher with renderer
        if (this._audioSwitcher != null)
          DirectShowUtil.RenderUnconnectedOutputPins(this.graphBuilder, this._audioSwitcher);
      }
    }

    private bool analyseStreams()
    {
      try
      {
        if (this.FStreams == null)
          this.FStreams = new FilterStreams();

        this.FStreams.DeleteAllStreams();

        string strSourceFilterName = _SOURCE_FILTER_NAME;

        for (int i = 0; i <= this._MixedUrl.AudioTracks.Length; i++)
        {
          string strFilterName = strSourceFilterName + (i > 0 ? " " + i.ToString("0000") : null);
          this.graphBuilder.FindFilterByName(strFilterName, out IBaseFilter sourceFilter);

          if (sourceFilter is IAMStreamSelect pStrm)
          {
            pStrm.Count(out int cStreams);

            //GET STREAMS
            for (int istream = 0; istream < cStreams; istream++)
            {
              //STREAM INFO
              pStrm.Info(istream, out AMMediaType sType, out AMStreamSelectInfoFlags sFlag, out int sPLCid,
                         out int sPDWGroup, out string sName, out _, out _);

              FilterStreamInfos fsInfos = new FilterStreamInfos
              {
                Current = false,
                Filter = strFilterName,
                Name = sName,
                LCID = sPLCid,
                Id = istream,
                Type = StreamType.Unknown,
                sFlag = sFlag
              };

              if (sPDWGroup == 0)
              {
                fsInfos.Type = StreamType.Video;
              }
              else if (sPDWGroup == 1)
              {
                fsInfos.Type = StreamType.Audio;
              }
              else if (sPDWGroup == 2 && sName.LastIndexOf("off", StringComparison.Ordinal) == -1 && sName.LastIndexOf("Hide ", StringComparison.Ordinal) == -1 &&
                  sName.LastIndexOf("No ", StringComparison.Ordinal) == -1 && sName.LastIndexOf("Miscellaneous ", StringComparison.Ordinal) == -1)
              {
                fsInfos.Type = StreamType.Subtitle;
              }
              else
                continue;

              Log.Debug("OnlinePlayer: analyseStreams() FoundStreams: Type={0}; Name={1}, Filter={2}, Id={3}, PDWGroup={4}, LCID={5}",
                        fsInfos.Type.ToString(), fsInfos.Name, fsInfos.Filter, fsInfos.Id.ToString(),
                        sPDWGroup.ToString(), sPLCid.ToString());

              this.FStreams.AddStreamInfos(fsInfos);
            }
          }
          DirectShowUtil.ReleaseComObject(sourceFilter);
        }
      }
      catch { }
      return true;
    }

    private int getNumberOfAudioStreams(IBaseFilter filterSource)
    {
      int iResult = 0;
      if (filterSource is IAMStreamSelect pStrm)
      {
        pStrm.Count(out int iStreams);

        for (int i = 0; i < iStreams; i++)
        {
          //STREAM INFO
          pStrm.Info(i, out _, out _, out _, out int iPDWGroup, out _, out _, out _);

          if (iPDWGroup == 1) //Audio
            iResult++;
        }
      }
      Log.Debug("OnlinePlayer: getNumberOfAudioStreams() Number of audio streams in source filter: {0}", iResult);
      return iResult;
    }

    private bool tryConnect(IBaseFilter filterSource, IPin pinTarget)
    {
      bool bResult = false;
      int iHr = filterSource.EnumPins(out IEnumPins pinEnum);
      DsError.ThrowExceptionForHR(iHr);
      if (iHr == 0 && pinEnum != null)
      {
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        while (!bResult && pinEnum.Next(1, pins, out int iFetchedd) == 0 && iFetchedd > 0)
        {
          pins[0].QueryDirection(out PinDirection pinDir);
          if (pinDir == PinDirection.Output)
            bResult = this.graphBuilder.Connect(pins[0], pinTarget) == 0;
        }
        DirectShowUtil.ReleaseComObject(pins[0]);
      }
      DirectShowUtil.ReleaseComObject(pinEnum);

      return bResult;
    }

    private void connectAudioToSwitcher(IBaseFilter audioDecoder, ref int iInpuPinIIdx)
    {
      //Connect audiodecoder with audioswitcher
      IPin pinOut = DsFindPin.ByDirection(audioDecoder, PinDirection.Output, 0);
      IPin pinIn = DsFindPin.ByDirection(this._audioSwitcher, PinDirection.Input, iInpuPinIIdx++);
      Marshal.ThrowExceptionForHR(this.graphBuilder.Connect(pinOut, pinIn));
      DirectShowUtil.ReleaseComObject(pinOut, 2000);
      DirectShowUtil.ReleaseComObject(pinIn, 2000);
    }

    private IBaseFilter addPreferredFilters(IGraphBuilder graphBuilder, IBaseFilter sourceFilter, bool bReleaseAudioDecoder = true)
    {
      using (Settings xmlreader = new MPSettings())
      {
        bool autodecodersettings = xmlreader.GetValueAsBool("movieplayer", "autodecodersettings", false);

        if (!autodecodersettings) // the user has not chosen automatic graph building by merits
        {
          bool bVc1ICodec = false, bVc1Codec = false, bXvidCodec = false; //- will come later
          bool aacCodec = false;
          bool h264Codec = false;
          bool videoCodec = false;
          bool audioCodec = false;
          bool bHevcCodec = false;

          // check the output pins of the splitter for known media types
          IEnumPins pinEnum = null;
          if (sourceFilter.EnumPins(out pinEnum) == 0)
          {
            int fetched = 0;
            IPin[] pins = new IPin[1];
            while (pinEnum.Next(1, pins, out fetched) == 0 && fetched > 0)
            {
              IPin pin = pins[0];
              PinDirection pinDirection;
              if (pin.QueryDirection(out pinDirection) == 0 && pinDirection == PinDirection.Output)
              {
                IEnumMediaTypes enumMediaTypesVideo = null;
                if (pin.EnumMediaTypes(out enumMediaTypesVideo) == 0)
                {
                  AMMediaType[] mediaTypes = new AMMediaType[1];
                  int typesFetched;
                  while (enumMediaTypesVideo.Next(1, mediaTypes, out typesFetched) == 0 && typesFetched > 0)
                  {
                    if (mediaTypes[0].majorType == MediaType.Video)
                    {
                      if (mediaTypes[0].subType == MediaSubType.HEVC)
                      {
                        Log.Info("[AddPreferredFilters] Found HEVC video on output pin");
                        bHevcCodec = true;
                      }
                      else if (mediaTypes[0].subType == MediaSubType.H264 || mediaTypes[0].subType == _GUID_MEDIASUBTYPE_AVC1)
                      {
                        Log.Info("found H264 video on output pin");
                        h264Codec = true;
                      }
                      else if (mediaTypes[0].subType == MediaSubType.VC1)
                      {
                        Log.Info("[AddPreferredFilters] Found VC1 video on output pin");

                        //if (g_Player.MediaInfo.IsInterlaced)
                        //    bVc1ICodec = true;
                        //else
                        bVc1Codec = true;
                      }
                      else if (mediaTypes[0].subType == MediaSubType.XVID || mediaTypes[0].subType == MediaSubType.xvid)
                      {
                        Log.Info("[AddPreferredFilters] Found xvid video on output pin");
                        bXvidCodec = true;
                      }
                      else
                        videoCodec = true;

                    }
                    else if (mediaTypes[0].majorType == MediaType.Audio)
                    {
                      if (mediaTypes[0].subType == MediaSubType.LATMAAC || mediaTypes[0].subType == MediaSubType.LATMAACLAF)
                      {
                        Log.Info("found AAC audio on output pin");
                        aacCodec = true;
                      }
                      else
                        audioCodec = true;
                    }
                  }
                  DirectShowUtil.ReleaseComObject(enumMediaTypesVideo);
                }
              }
              DirectShowUtil.ReleaseComObject(pin);
            }
            DirectShowUtil.ReleaseComObject(pinEnum);
          }

          // add filters for found media types to the graph as configured in MP
          if (h264Codec)
          {
            _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
          }
          else if (bHevcCodec)
          {
            _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "hevcvideocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
          }
          else if (bXvidCodec)
          {
            _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "xvidvideocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
          }
          else if (bVc1Codec)
          {
            _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "vc1videocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
          }
          else if (bVc1ICodec)
          {
            _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "vc1ivideocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
          }
          else if (videoCodec)
          {
            _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
            DirectShowUtil.ReleaseComObject(
                DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
          }

          //Get audio decoder
          if (aacCodec || audioCodec)
          {
            if (aacCodec)
              _AudioDecoder = xmlreader.GetValueAsString("movieplayer", "aacaudiocodec", "");
            else
              _AudioDecoder = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");

            if (bReleaseAudioDecoder)
              DirectShowUtil.ReleaseComObject(DirectShowUtil.AddFilterToGraph(graphBuilder, _AudioDecoder));
            else
              return DirectShowUtil.AddFilterToGraph(graphBuilder, _AudioDecoder);
          }
        }
      }

      return null;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void onSeek()
    {
      if (this.mediaPos != null)
        this.mediaPos.get_CurrentPosition(out this._LastCurrentPosition);

      this._PlaybackDetected = false;
      this._PreBufferNeeded = true;
      this._LastProgressCheck = DateTime.MinValue;
      this._SeekTimeStamp = DateTime.Now;

      Log.Debug("[onSeek] Current position: {0}", this._LastCurrentPosition);
    }

    private IBaseFilter getSourceFilter()
    {
      if (this._SourceFilter == null)
      {
        Log.Debug("OnlinePlayer: getSourceFilte()]");

        if (this._SourceFilterName == null)
          this._SourceFilterName = _SOURCE_FILTER_NAME;

        int iResult = this.graphBuilder.FindFilterByName(this._SourceFilterName, out this._SourceFilter);
        if (iResult != 0)
        {
          string strErrorText = DsError.GetErrorText(iResult);
          if (strErrorText != null)
            strErrorText = strErrorText.Trim();

          Log.Warn("OnlinePlayer: getSourceFilter() FindFilterByName returned '{0}'{1}", "0x" + iResult.ToString("X8"), !string.IsNullOrEmpty(strErrorText) ? " : (" + strErrorText + ")" : "");
          return null;
        }
      }
      return this._SourceFilter;
    }

    private bool disposeSourceFilter()
    {
      if (this._SourceFilter != null)
      {
        DirectShowUtil.ReleaseComObject(this._SourceFilter);
        this._SourceFilter = null;
        return true;
      }
      return false;
    }

    private static string printFileSize(long lValue)
    {
      return printFileSize(lValue, "0");
    }
    private static string printFileSize(long lValue, string strFormat)
    {
      if (lValue < 0)
        return string.Empty;

      string strSuffix, strValue;

      if (lValue < 1024)
      {
        strValue = lValue.ToString();
        strSuffix = " B";
      }
      else if (lValue < 1048576)
      {
        strValue = ((float)lValue / 1024).ToString(strFormat, _Culture_EN);
        strSuffix = " kB";
      }
      else if (lValue < 1073741824)
      {
        strValue = ((float)lValue / 1048576).ToString(strFormat, _Culture_EN);
        strSuffix = " MB";
      }
      else
      {
        strValue = ((float)lValue / 1073741824).ToString("0.00", _Culture_EN);
        strSuffix = " GB";
      }

      return strValue + strSuffix;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void onBuffer()
    {
      if (!g_Player.Paused)
        g_Player.Pause();

      GUIPropertyManager.SetProperty("#Play.Current.Buffer.IsBuffering", "True");
      this._Buffering = true;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void processLavBufferLevel()
    {
      if (this._LavMaxQueue < -1)
        return; //non LAV filter

      if ((DateTime.Now - this._LastProgressCheck).TotalMilliseconds >= 1000) //once per second is enough
      {
        this._LastProgressCheck = DateTime.Now;

        try
        {
          int iLAVsize = -1;
          int iLAVlevel = -1;
          double dLAVtime = -1;
          bool bPlaybackEnds;
          BufferStatusEnum bufferStatus;

        get:
          IBaseFilter sourceFilter = this.getSourceFilter();

          if (this._LavMaxQueue < 0 && sourceFilter != null)
          {
            #region LAV init

            try
            {
              ILAVSplitterSettings lav = (ILAVSplitterSettings)sourceFilter;
              this._LavMaxQueue = lav.GetMaxQueueSize(); //Samples
              this._LavMaxQueueMemSize = lav.GetMaxQueueMemSize(); //MB

              this._VideoSampleDuration = 1.0d / RefreshRateChanger.RefreshRateChangeWorkerFps;
              if (this._VideoSampleDuration > 0)
              {
                //Check min buffer total time
                int iQueue = (int)Math.Ceiling(LAV_MIN_BUFFER_TOTAL_TIME / this._VideoSampleDuration);
                if (iQueue > this._LavMaxQueue)
                {
                  //Modify MaxQueueSize (for runtime only)
                  lav.SetRuntimeConfig(true);
                  lav.SetMaxQueueSize(iQueue);
                  this._LavMaxQueue = iQueue;
                }

                this._LavMaxQueueTime = this._VideoSampleDuration * this._LavMaxQueue;
              }

              Log.Debug("OnlinePlayer: processLavBufferLevel() MaxQueue: {0} / {1:0.0}s / {2}MB",
                  this._LavMaxQueue, this._LavMaxQueueTime, this._LavMaxQueueMemSize);
            }
            catch (Exception ex)
            {
              Log.Error("OnlinePlayer: processLavBufferLevel() Error: {0}", ex.Message);
              this._LavMaxQueue = -2; //no success; never use LAV IBuffer
            }

            if (this._LavMaxQueue < 0)
            {
              //Non LAV filter

              this.disposeSourceFilter();
              sourceFilter = null;

              Log.Error("OnlinePlayer: processLavBufferLevel() LAV filter not available.");
            }
            else
              GUIPropertyManager.SetProperty("#Play.Current.Buffer.IsAvailable", "True");
            #endregion
          }

          if (sourceFilter != null)
          {
            #region Buffer status

            ILAVBufferInfo buffer;
            try
            {
              buffer = (ILAVBufferInfo)sourceFilter;
            }
            catch (Exception ex)
            {
              Log.Error("OnlinePlayer: processLavBufferLevel() Error: {0}", ex.Message);
              this._LavMaxQueue = -1;
              this.disposeSourceFilter();
              sourceFilter = null;
              goto get; //try get source filter again
            }

            //Number of buffers (splitter's output pins)
            uint wCnt = buffer.GetCount();
            iLAVsize = 0;
            uint wSamplesVideo = 0;
            for (uint i = 0; i < wCnt; i++)
            {
              if (buffer.GetStatus(i, out uint wSamples, out uint wSize) != 0)
              {
                iLAVsize = -1;
                break;
              }

              //Total buffer size
              iLAVsize += (int)wSize;

              //Sample count of video output pin
              if (i == this._SourceFilterVideoPinIndex)
                wSamplesVideo = wSamples;
            }

            //Current LAV video buffer size
            GUIPropertyManager.SetProperty("#Play.Current.Buffer.BufferSize", printFileSize(iLAVsize));

            //Current LAV video buffer time
            if (this._LavMaxQueue > 0)
            {
              //Default MaxQueue is 350
              //Each sample represents 1 video frame(progressive)
              iLAVlevel = (int)(float)wSamplesVideo * 100 / this._LavMaxQueue;

              if (this._LavMaxQueueTime > 0)
              {
                dLAVtime = this._VideoSampleDuration * wSamplesVideo;
                GUIPropertyManager.SetProperty("#Play.Current.Buffer.BufferTime", string.Format(_Culture_EN, "{0,4:0.0}", dLAVtime));
              }
            }

            //Current LAV video buffer level
            if (iLAVlevel >= 0)
              GUIPropertyManager.SetProperty("#Play.Current.Buffer.BufferLevel", string.Format("{0,3}", iLAVlevel));
            else
            {
              Log.Error("OnlinePlayer: processLavBufferLevel() Unknown buffer level.");
              return;
            }

            #endregion
          }
          else
          {
            Log.Error("OnlinePlayer: processLavBufferLevel() Unknown filter.");
            return;
          }


          if (dLAVtime >= 0)
          {
            //Frame duration is known

            if (dLAVtime < LAV_MIN_BUFFER_TIME)
              bufferStatus = BufferStatusEnum.BufferingNeeded;
            else if (dLAVtime >= LAV_BUFFERED_TIME)
              bufferStatus = BufferStatusEnum.BufferedEnough;
            else
              bufferStatus = BufferStatusEnum.AboveMinLevel;

            //Check ending of the stream(to avoid pausing the playback at the end)
            bPlaybackEnds = this.Duration - this.CurrentPosition < this._LavMaxQueueTime;

            GUIPropertyManager.SetProperty("#TV.Record.percent3", ((this.CurrentPosition + (this._LavMaxQueueTime * iLAVlevel / 100)) / this.Duration * 100).ToString("0.000"));
          }
          else
          {
            //Use % level

            if (iLAVlevel < LAV_MIN_BUFFER_LEVEL)
              bufferStatus = BufferStatusEnum.BufferingNeeded;
            else if (iLAVlevel >= LAV_BUFFERED_LEVEL)
              bufferStatus = BufferStatusEnum.BufferedEnough;
            else
              bufferStatus = BufferStatusEnum.AboveMinLevel;

            //Check ending of the stream(to avoid pausing the playback at the end)
            bPlaybackEnds = this.Duration - this.CurrentPosition < LAV_DEFAULT_BUFFER_TOTAL_TIME;

            GUIPropertyManager.SetProperty("#TV.Record.percent3", ((this.CurrentPosition + (LAV_DEFAULT_BUFFER_TOTAL_TIME * iLAVlevel / 100)) / this.Duration * 100).ToString("0.000"));
          }

          //The memory size reached 75% of MaxMemSize(256MB by default); consider the level as enough
          if (bufferStatus < BufferStatusEnum.BufferedEnough && ((float)iLAVsize / 0x100000 / this._LavMaxQueueMemSize >= 0.75f))
            bufferStatus = BufferStatusEnum.BufferedEnough;

          //Playback detection: to avoid buffering at start or after seek (if prebuffering is disabled)
          if (this._SeekTimeStamp == DateTime.MinValue)
            this._SeekTimeStamp = DateTime.Now;

          if (!this._PlaybackDetected && this.m_state == PlayState.Playing &&
              (bufferStatus >= BufferStatusEnum.AboveMinLevel || (DateTime.Now - this._SeekTimeStamp).TotalMilliseconds >= 5000) &&
              this.CurrentPosition - this._LastCurrentPosition >= 2.0)
          {
            this._PlaybackDetected = true;
            Log.Debug("OnlinePlayer: processLavBufferLevel() Playback detected.");
          }

          //GUI Buffering indicator
          if (this.m_state == PlayState.Playing && !this._Buffering && !bPlaybackEnds &&
              ((LAV_ALWAYS_PREBUFFER && (bufferStatus < BufferStatusEnum.AboveMinLevel || this._PreBufferNeeded))
              || (!LAV_ALWAYS_PREBUFFER && bufferStatus < BufferStatusEnum.AboveMinLevel && this._PlaybackDetected))
              )
          {
            if (this._PreBufferNeeded)
            {
              this._PreBufferNeeded = false;

              if (bufferStatus >= BufferStatusEnum.BufferedEnough) //allready enough
                return;
            }

            //Buffer is too low; pause the playback

            Log.Debug("OnlinePlayer: processLavBufferLevel() Buffering activated:  {0} / {1:0.0}s / {2}",
                iLAVlevel, dLAVtime, printFileSize(iLAVsize));

            GUIPropertyManager.SetProperty("#Play.Current.Buffer.IsBuffering", "True");
            this._Buffering = true;

            if (!g_Player.Paused)
              g_Player.Pause();
          }
          else if (this._Buffering && (bPlaybackEnds || bufferStatus >= BufferStatusEnum.BufferedEnough))
          {
            //Buffer has sufficient level; resume the playback

            Log.Debug("OnlinePlayer: processLavBufferLevel() Buffering deactivated.");

            GUIPropertyManager.SetProperty("#Play.Current.Buffer.IsBuffering", "False");
            this._Buffering = false;

            if (g_Player.Paused)
              g_Player.Pause();
          }
        }
        catch (Exception ex)
        {
          Log.Error("OnlinePlayer: processLavBufferLevel() Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        }
      }
    }

    private static void tagsClear()
    {
      //Tags init
      GUIPropertyManager.SetProperty("#Play.Current.Buffer.IsAvailable", "False");
      GUIPropertyManager.SetProperty("#Play.Current.Buffer.IsBuffering", "False");
      GUIPropertyManager.SetProperty("#Play.Current.Buffer.BufferSize", "0 B");
      GUIPropertyManager.SetProperty("#Play.Current.Buffer.BufferTime", "0");
      GUIPropertyManager.SetProperty("#Play.Current.Buffer.BufferLevel", "0");
      GUIPropertyManager.SetProperty("#Play.Current.Stream.Audio.Total", "0");
      GUIPropertyManager.SetProperty("#Play.Current.Stream.Audio.IsMulti", "False");
    }
    #endregion
  }
}

