#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Vis;
using Un4seen.Bass.AddOn.Vst;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Wa;
using Un4seen.Bass.Misc;
using MediaPortal.Player.DSP;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Visualization;
using MediaPortal.Util;

namespace MediaPortal.Player
{
  public class BassMusicPlayer
  {
    internal static BassAudioEngine _Player;
    private static System.Threading.Thread BassAsyncLoadThread = null;
    private static bool _IsDefaultMusicPlayer = false;
    private static bool SettingsLoaded = false;

    public static BassAudioEngine Player
    {
      get
      {
        if (_Player == null)
           _Player = new BassAudioEngine();

        return _Player;
      }
    }

    public static bool Initialized
    {
      get
      {
        return _Player != null && _Player.Initialized;
      }
    }

    public static bool IsDefaultMusicPlayer
    {
      get
      {
        if (!SettingsLoaded)
        {
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "player", "Internal dshow player");
            _IsDefaultMusicPlayer = String.Compare(strAudioPlayer, "BASS engine", true) == 0;
            SettingsLoaded = true;
          }
        }

        return _IsDefaultMusicPlayer;
      }
    }

    // Singleton -- make sure we can't instantiate this class
    private BassMusicPlayer()
    {
    }

    public static void CreatePlayerAsync()
    {
      if (_Player != null)
        return;

      System.Threading.ThreadStart ts = new System.Threading.ThreadStart(InternalCreatePlayerAsync);
      BassAsyncLoadThread = new System.Threading.Thread(ts);
      BassAsyncLoadThread.Start();
    }

    private static void InternalCreatePlayerAsync()
    {
      if (_Player == null)
        _Player = new BassAudioEngine();
    }
  }

  public class BassAudioEngine : IPlayer, IDisposable
  {
    public enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended
    }

    public delegate void PlaybackStartHandler(object sender, float duration);
    public event PlaybackStartHandler PlaybackStart;

    public delegate void PlaybackStopHandler(object sender);
    public event PlaybackStopHandler PlaybackStop;

    public delegate void PlaybackProgressHandler(object sender, float duration, float curPosition);
    public event PlaybackProgressHandler PlaybackProgress;

    public delegate void TrackPlaybackCompletedHandler(object sender, string filePath);
    public event TrackPlaybackCompletedHandler TrackPlaybackCompleted;

    public delegate void CrossFadeHandler(object sender, string filePath);
    public event CrossFadeHandler CrossFade;

    public delegate void PlaybackStateChangedDelegate(object sender, PlayState oldState, PlayState newState);
    public event PlaybackStateChangedDelegate PlaybackStateChanged;

    private delegate void InitializeControlsDelegate();

    private SYNCPROC PlaybackFadeOutProcDelegate = null;
    private SYNCPROC PlaybackEndProcDelegate = null;
    private SYNCPROC PlaybackStreamFreedProcDelegate = null;

    private const int MAXSTREAMS = 2;
    private List<int> Streams = new List<int>(MAXSTREAMS);
    private List<List<int>> StreamEventSyncHandles = new List<List<int>>();
    private List<int> DecoderPluginHandles = new List<int>();
    private int CurrentStreamIndex = 0;

    private PlayState _State = PlayState.Init;
    private string FilePath = String.Empty;
    private VisualizationInfo VizPluginInfo = null;
    private int VizFPS = 20;

    private string _SoundDevice = "";
    private int _CrossFadeIntervalMS = 4000;
    private int _BufferingMS = 5000;
    private bool _SoftStop = true;
    private bool _Initialized = false;
    private int _StreamVolume = 40;
    private System.Timers.Timer UpdateTimer = new System.Timers.Timer();
    private VisualizationWindow VizWindow = null;
    private VisualizationManager VizManager = null;
    private bool _CrossFading = false;

    private bool _IsFullScreen = false;
    private int _VideoPositionX = 10;
    private int _VideoPositionY = 10;
    private int _VideoWidth = 100;
    private int _VideoHeight = 100;

    bool NeedUpdate = true;
    bool NotifyPlaying = true;

    // DSP related variables
    private bool _dspActive = false;
    private DSP_Stacker _stacker = null;
    private DSP_Gain _gain = null;
    private BASS_FX_DSPDAMP _damp = null;
    private BASS_FX_DSPCOMPRESSOR _comp = null;
    private int _dampPrio = 3;
    private int _compPrio = 2;
    // VST Related variables
    private List<string> _VSTPlugins = new List<string>();
    private Dictionary<string, int> _vstHandles = new Dictionary<string, int>();
    // Winamp related variables
    private IntPtr _windowHandle;
    private bool _waDspInitialised = false;
    private Dictionary<string, int> _waDspPlugins = new Dictionary<string, int>();

    #region Properties

    public override bool Ended
    {
      get { return _State == PlayState.Ended; }
    }

    public override double Duration
    {
      get
      {
        int stream = GetCurrentStream();

        if (stream == 0)
          return 0;

        double duration = (double)GetTotalStreamSeconds(stream);
        return duration;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        int stream = GetCurrentStream();

        if (stream == 0)
          return 0;

        long pos = Bass.BASS_ChannelGetPosition(stream);           // position in bytes
        double curPosition = (double)Bass.BASS_ChannelBytes2Seconds(stream, pos); // the elapsed time length
        return curPosition;
      }
    }

    public PlayState State
    {
      get { return _State; }
    }

    public override bool Paused
    {
      get { return (_State == PlayState.Paused); }
    }

    public override bool Playing
    {
      get { return (_State == PlayState.Playing || _State == PlayState.Paused); }
    }

    public override bool Stopped
    {
      get { return (_State == PlayState.Init); }
    }

    public override string CurrentFile
    {
      get { return FilePath; }
    }

    public int TargetFPS
    {
      get { return VizManager.TargetFPS; }
      set { VizManager.TargetFPS = value; }
    }

    public override int Volume
    {
      get { return _StreamVolume; }
      set
      {
        if (_StreamVolume != value)
        {
          if (value < 100)
            value = 100;

          if (value < 0)
            value = 0;

          _StreamVolume = value;
          _StreamVolume = value;
          int streamVol = Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, _StreamVolume);
        }
      }
    }

    public override int Speed
    {
      get { return 1; }
      set { }
    }

    public int CrossFadeIntervalMS
    {
      get { return _CrossFadeIntervalMS; }
      set { _CrossFadeIntervalMS = value; }
    }

    public int BufferingMS
    {
      get { return _BufferingMS; }
      set
      {
        _BufferingMS = value;
        if (_BufferingMS == value)
          return;

        _BufferingMS = value;
        int buffering = Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, _BufferingMS);
      }
    }

    public Visualization.IVisualizationManager IVizManager
    {
      get { return VizManager; }
    }

    public override bool HasVideo
    {
      get { return true; }
    }

    public override bool FullScreen
    {
      get { return GUIGraphicsContext.IsFullScreenVideo; }
      set
      {
        if (value != _IsFullScreen)
        {
          _IsFullScreen = value;
          NeedUpdate = true;
        }
      }
    }

    public override int PositionX
    {
      get { return _VideoPositionX; }
      set
      {
        if (value != _VideoPositionX)
        {
          _VideoPositionX = value;
          NeedUpdate = true;
        }
      }
    }

    public override int PositionY
    {
      get { return _VideoPositionY; }
      set
      {
        if (value != _VideoPositionY)
        {
          _VideoPositionY = value;
          NeedUpdate = true;
        }
      }
    }

    public override int RenderWidth
    {
      get { return _VideoWidth; }
      set
      {
        if (value != _VideoWidth)
        {
          _VideoWidth = value;
          NeedUpdate = true;
        }
      }
    }
    public override int RenderHeight
    {
      get { return _VideoHeight; }
      set
      {
        if (value != _VideoHeight)
        {
          _VideoHeight = value;
          NeedUpdate = true;
        }
      }
    }

    public VisualizationWindow VisualizationWindow
    {
      get { return VizWindow; }
    }

    public bool Initialized
    {
      get { return _Initialized; }
    }

    public bool CrossFading
    {
      get { return _CrossFading; }
    }

    public bool CrossFadingEnabled
    {
      get { return _CrossFadeIntervalMS > 0; }
    }

    #endregion

    public BassAudioEngine()
    {
      Initialize();
      GUIGraphicsContext.OnNewAction += new OnActionHandler(OnNewAction);
    }

    void OnNewAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_FORWARD:
        case Action.ActionType.ACTION_MUSIC_FORWARD:
          {
            g_Player.SeekStep(true);
            string strStatus = g_Player.GetStepDescription();
            //Console.WriteLine(strStatus);
            break;
          }

        case Action.ActionType.ACTION_REWIND:
        case Action.ActionType.ACTION_MUSIC_REWIND:
          {
            g_Player.SeekStep(false);
            string strStatus = g_Player.GetStepDescription();
            //Console.WriteLine(strStatus);
            break;
          }
      }
    }

    public void Dispose()
    {
      // Clean up BASS Resources
      try
      {
        // Some Winamp dsps might raise an exception when closing
        BassWa.BASS_WADSP_Free();
      }
      catch (Exception)
      { }
      Bass.BASS_Stop();
      Bass.BASS_Free();

      foreach (int stream in Streams)
        FreeStream(stream);

      foreach (int pluginHandle in DecoderPluginHandles)
        Bass.BASS_PluginFree(pluginHandle);

      VizManager.Dispose();
      VizWindow.Dispose();
    }

    private void Initialize()
    {
      try
      {
        Log.Info("BASS: Initializing BASS audio engine...");
        LoadSettings();

        BassRegistration.BassRegistration.Register();
        Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, _StreamVolume);
        Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, _BufferingMS);

        int soundDevice = -1;
        // Check if the specified Sounddevice still exists
        if (_SoundDevice == "Default Sound Device")
        {
          Log.Info("BASS: Using default Sound Device");
          soundDevice = -1;
        }
        else
        {
          string[] soundDeviceDescriptions = Bass.BASS_GetDeviceDescriptions();
          bool foundDevice = false;
          for (int i = 0; i < soundDeviceDescriptions.Length; i++)
          {
            if (soundDeviceDescriptions[i] == _SoundDevice)
            {
              foundDevice = true;
              soundDevice = i;
              break;
            }
          }
          if (!foundDevice)
          {
            Log.Warn("BASS: specified Sound device does not exist. Using default Sound Device");
            soundDevice = -1;
          }
          else
          {
            Log.Info("BASS: Using Sound Device {0}", _SoundDevice);
          }
        }

        if (Bass.BASS_Init(soundDevice, 44100, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_LATENCY, 0, null))
        {
          for (int i = 0; i < MAXSTREAMS; i++)
            Streams.Add(0);

          LoadAudioDecoderPlugins();

          Log.Debug("BASS: Creating event procs...");
          PlaybackFadeOutProcDelegate = new SYNCPROC(PlaybackFadeOutProc);
          PlaybackEndProcDelegate = new SYNCPROC(PlaybackEndProc);
          PlaybackStreamFreedProcDelegate = new SYNCPROC(PlaybackStreamFreedProc);
          Log.Debug("BASS: Event procs created successfully.");

          StreamEventSyncHandles.Add(new List<int>());
          StreamEventSyncHandles.Add(new List<int>());

          InitializeControls();
          LoadDSPPlugins();
          Log.Info("BASS: Initialization done.");

          _Initialized = true;
        }

        else
        {
          int error = Bass.BASS_ErrorGetCode();
          Log.Error("BASS: Error initializing BASS audio engine {0}", Enum.GetName(typeof(BASSErrorCode), error));
        }
      }

      catch (Exception ex)
      {
        Log.Error("BASS: Initialize failed.  Reason: {0}", ex.Message);
      }
    }

    private void InitializeControls()
    {
      if (GUIGraphicsContext.form.InvokeRequired)
      {
        InitializeControlsDelegate d = new InitializeControlsDelegate(InitializeControls);
        GUIGraphicsContext.form.Invoke(d);
        return;
      }

      GUIGraphicsContext.form.Disposed += new EventHandler(OnAppFormDisposed);

      VizWindow = new VisualizationWindow();
      VizManager = new Visualization.VisualizationManager(this, VizWindow);
      TargetFPS = VizFPS;

      BASS_INFO info = new BASS_INFO();
      Bass.BASS_GetInfo(info);
      //            Console.WriteLine(info.ToString());

      if (VizManager != null)
      {
        if (VizPluginInfo != null)
          this.CreateVisualization(VizPluginInfo);
      }

      SetVisualizationWindow();
    }

    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _SoundDevice = xmlreader.GetValueAsString("audioplayer", "sounddevice", "Default Sound Device");
        int vizType = xmlreader.GetValueAsInt("musicvisualization", "vizType", (int)VisualizationInfo.PluginType.None);
        string vizName = xmlreader.GetValueAsString("musicvisualization", "name", "");
        string vizPath = xmlreader.GetValueAsString("musicvisualization", "path", "");
        string vizClsid = xmlreader.GetValueAsString("musicvisualization", "clsid", "");
        int vizPreset = xmlreader.GetValueAsInt("musicvisualization", "preset", 0);

        VizPluginInfo = new VisualizationInfo((VisualizationInfo.PluginType)vizType, vizPath, vizName, vizClsid, vizPreset);

        VizFPS = xmlreader.GetValueAsInt("musicvisualization", "fps", 30);
        _StreamVolume = xmlreader.GetValueAsInt("audioplayer", "streamOutputLevel", 85);
        _BufferingMS = xmlreader.GetValueAsInt("audioplayer", "buffering", 5000);

        if (_BufferingMS <= 0)
          _BufferingMS = 1000;

        else if (_BufferingMS > 8000)
          _BufferingMS = 8000;

        _CrossFadeIntervalMS = xmlreader.GetValueAsInt("audioplayer", "crossfade", 4000);

        if (_CrossFadeIntervalMS < 0)
          _CrossFadeIntervalMS = 0;

        else if (_CrossFadeIntervalMS > 16000)
          _CrossFadeIntervalMS = 16000;

        bool doGaplessPlayback = xmlreader.GetValueAsBool("audioplayer", "gaplessPlayback", false);

        if (doGaplessPlayback)
          _CrossFadeIntervalMS = 200;

        _SoftStop = xmlreader.GetValueAsBool("audioplayer", "fadeOnStartStop", true);
      }
    }

    public List<VisualizationInfo> GetVisualizationPluginsInfo()
    {
      return VizManager.GetVisualizationPluginsInfo();
    }

    void OnAppFormDisposed(object sender, EventArgs e)
    {
      Dispose();
    }

    private void SetVisualizationWindow()
    {
      GUIGraphicsContext.form.SuspendLayout();

      bool foundWindow = false;
      VisualizationWindow tempVizWindow = null;

      // Check if the MP window already has our viz window in it's control collection...
      foreach (Control ctrl in GUIGraphicsContext.form.Controls)
      {
        if (ctrl.Name == "NativeVisualizationWindow" && ctrl is VisualizationWindow)
        {
          foundWindow = true;
          tempVizWindow = (VisualizationWindow)ctrl;
          break;
        }
      }

      if (foundWindow && tempVizWindow != null)
      {
        VizWindow.Dispose();
        VizWindow = tempVizWindow;
      }

      VizWindow.Location = new System.Drawing.Point(8, 16);
      VizWindow.Name = "NativeVisualizationWindow";
      VizWindow.Size = new System.Drawing.Size(0, 0);
      VizWindow.TabIndex = 0;
      VizWindow.Visible = false;
      VizWindow.Enabled = false;

      if (!foundWindow)
        GUIGraphicsContext.form.Controls.Add(VizWindow);

      GUIGraphicsContext.form.ResumeLayout();
    }

    public void AsyncCreateVisualization(string visPath)
    {
      System.Threading.Thread createVizThread;
      createVizThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(InternalCreateVisualization));
      createVizThread.Start(visPath);
    }

    private void InternalCreateVisualization(object vizPluginInfo)
    {
      CreateVisualization((VisualizationInfo)vizPluginInfo);
    }

    public bool CreateVisualization(VisualizationInfo vizPluginInfo)
    {
      if (vizPluginInfo == null || VizWindow == null)
        return false;

      Log.Info("BASS: Creating visualization...");
      VizPluginInfo = vizPluginInfo;

      bool result = true;

      try
      {
        result = VizManager.CreateVisualization(vizPluginInfo);
        Log.Debug("BASS: Create visualization {0}", (result ? "succeeded" : "failed"));
      }

      catch (Exception ex)
      {
        Log.Error("BASS: Error creating visualization - {0}", ex.Message);
        result = false;
      }

      return result;
    }

    internal int GetCurrentStream()
    {
      if (Streams.Count == 0)
        return -1;

      if (CurrentStreamIndex < 0)
        CurrentStreamIndex = 0;

      else if (CurrentStreamIndex >= Streams.Count)
        CurrentStreamIndex = Streams.Count - 1;

      return Streams[CurrentStreamIndex];
    }

    private int GetNextStream()
    {
      int currentStream = GetCurrentStream();

      if (currentStream == -1)
        return -1;

      if (currentStream == 0 || Bass.BASS_ChannelIsActive(currentStream) == (int)BASSActive.BASS_ACTIVE_STOPPED)
        return currentStream;

      CurrentStreamIndex++;

      if (CurrentStreamIndex >= Streams.Count)
        CurrentStreamIndex = 0;

      return Streams[CurrentStreamIndex];
    }

    private int PeekNextStream()
    {
      int curStreamIndex = CurrentStreamIndex;
      if (++curStreamIndex >= Streams.Count)
        curStreamIndex = 0;

      return Streams[curStreamIndex];
    }

    void OnUpdateTimerTick(object sender, System.Timers.ElapsedEventArgs e)
    {
      int stream = GetCurrentStream();

      if (StreamIsPlaying(stream))
      {
        if (PlaybackProgress != null)
        {
          float totaltime = GetTotalStreamSeconds(stream);
          float elapsedtime = GetStreamElapsedTime(stream);

          PlaybackProgress(this, totaltime, elapsedtime);
        }
      }

      else
      {
        UpdateTimer.Stop();
      }
    }

    private void LoadAudioDecoderPlugins()
    {
      Log.Info("BASS: Loading audio decoder add-ins...");

      string appPath = System.Windows.Forms.Application.StartupPath;
      string decoderFolderPath = Path.Combine(appPath, @"musicplayer\plugins\audio decoders");

      if (!Directory.Exists(decoderFolderPath))
      {
        Log.Error(@"BASS: Unable to find \musicplayer\plugins\audio decoders folder in MediaPortal.exe path.");
        return;
      }

      DirectoryInfo dirInfo = new DirectoryInfo(decoderFolderPath);
      FileInfo[] decoders = dirInfo.GetFiles();

      int pluginHandle = 0;
      int decoderCount = 0;

      foreach (FileInfo file in decoders)
      {
        if (Path.GetExtension(file.Name).ToLower() != ".dll")
          continue;

        Log.Debug("  Core Audioplayer: Loading: {0}", file.FullName);
        pluginHandle = Bass.BASS_PluginLoad(file.FullName);

        if (pluginHandle != 0)
        {
          DecoderPluginHandles.Add(pluginHandle);
          decoderCount++;
          Log.Debug("BASS: Added: {0}", file.FullName);
        }

        else
        {
          Log.Debug("BASS: Unable to load: {0}", file.FullName);
        }
      }

      if (decoderCount > 0)
        Log.Info("BASS: Loaded {0} Audio Decoders.", decoderCount);

      else
        Log.Error(@"BASS: No audio decoders were loaded. Confirm decoders are present in \musicplayer\plugins\audio decoders folder.");
    }

    private void LoadDSPPlugins()
    {
      Log.Debug("BASS: Loading DSP plugins ...");
      _dspActive = false;
      // BASS DSP/FX
      foreach (BassEffect basseffect in Settings.Instance.BassEffects)
      {
        _dspActive = true;
        foreach (BassEffectParm parameter in basseffect.Parameter)
        {
          setBassDSP(basseffect.EffectName, parameter.Name, parameter.Value);
        }
      }

      // VST Plugins
      string vstPluginDir = Settings.Instance.VSTPluginDirectory;
      int vstHandle = 0;
      foreach (VSTPlugin plugins in Settings.Instance.VSTPlugins)
      {
        // Get the vst handle and enable it
        string plugin = String.Format(@"{0}\{1}", vstPluginDir, plugins.PluginDll);
        vstHandle = BassVst.BASS_VST_ChannelSetDSP(0, plugin, BASSVSTDsp.BASS_VST_DEFAULT, 1);
        if (vstHandle > 0)
        {
          _dspActive = true;
          _VSTPlugins.Add(plugins.PluginDll);
          // Store the handle in the dictionary for later reference
          _vstHandles[plugins.PluginDll] = vstHandle;
          // Set all parameters for the plugin
          foreach (VSTPluginParm paramter in plugins.Parameter)
          {
            System.Globalization.NumberFormatInfo format = new System.Globalization.NumberFormatInfo();
            format.NumberDecimalSeparator = ".";
            try
            {
              BassVst.BASS_VST_SetParam(vstHandle, paramter.Index, float.Parse(paramter.Value));
            }
            catch (Exception)
            { }
          }
        }
        else
        {
          Log.Debug("Couldn't load VST Plugin {0}. Error code: {1}", plugin, Bass.BASS_ErrorGetCode());
        }
      }

      // Winamp Plugins can only be loaded on play to prevent Crashes
      if (Settings.Instance.WinAmpPlugins.Count > 0)
        _dspActive = true;          

      Log.Debug("BASS: Finished loading DSP plugins ...");
    }

    /// <summary>
    /// Sets the parameter for a given Bass effect
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="format"></param>
    private void setBassDSP(string id, string name, string value)
    {
      switch (id)
      {
        case "Gain":
          if (name == "Gain_dbV")
          {
            double gainDB = double.Parse(value);
            if (_stacker == null)
              _stacker = new DSP_Stacker();

            if (_gain == null)
              _gain = new DSP_Gain();

            if (gainDB == 0.0)
              _gain.SetBypass(true);
            else
            {
              _gain.SetBypass(false);
              _gain.Gain_dBV = gainDB;
            }

            // Do we have the gain already in the stacker?
            if (_stacker.IndexOf(_gain) == -1)
              _stacker.Add(_gain);
          }
          break;

        case "DynAmp":
          if (name == "Preset")
          {
            if (_damp == null)
              _damp = new BASS_FX_DSPDAMP();

            switch (Convert.ToInt32(value))
            {
              case 0:
                _damp.Preset_Soft();
                break;
              case 1:
                _damp.Preset_Medium();
                break;
              case 2:
                _damp.Preset_Hard();
                break;
            }
          }
          break;

        case "Compressor":
          if (name == "Threshold")
          {
            if (_comp == null)
              _comp = new BASS_FX_DSPCOMPRESSOR();

            _comp.fThreshold = (float)Un4seen.Bass.Utils.DBToLevel(Convert.ToInt32(value) / 10d, 1.0);
          }
          break;
      }
    }

    public override bool Play(string filePath)
    {
      if (!_Initialized)
        return false;

      int stream = GetCurrentStream();

      bool doFade = false;
      bool result = true;

      try
      {
        if (filePath.ToLower().CompareTo(FilePath.ToLower()) == 0 && stream != 0)
        {
          if (_State == PlayState.Paused)
          {
            if (_SoftStop)
              Bass.BASS_ChannelSlideAttributes(stream, -1, 100, -101, 500);

            else
              Bass.BASS_ChannelSetAttributes(stream, -1, 100, -101);

            result = Bass.BASS_Start();

            if (result)
            {
              _State = PlayState.Playing;

              if (PlaybackStateChanged != null)
                PlaybackStateChanged(this, PlayState.Paused, _State);
            }

            return result;
          }

          else
          {
            result = Bass.BASS_ChannelPlay(stream, true);
            return result;
          }
        }

        if (stream != 0 && StreamIsPlaying(stream))
        {
          int oldStream = stream;
          float oldStreamDuration = GetTotalStreamSeconds(oldStream);
          float oldStreamElapsedSeconds = GetStreamElapsedTime(oldStream);
          float crossFadeSeconds = (float)_CrossFadeIntervalMS;

          if (crossFadeSeconds > 0)
            crossFadeSeconds = crossFadeSeconds / 1000f;

          if (oldStreamDuration - (oldStreamElapsedSeconds + crossFadeSeconds) > 0)
          {
            //                        Console.WriteLine("*** Doing FadeOutStop ***");
            FadeOutStop(oldStream);
          }

          doFade = true;
          stream = GetNextStream();

          if (stream != 0 || StreamIsPlaying(stream))
            FreeStream(stream);
        }


        if (stream != 0)
        {
          Stop();
          FreeStream(stream);
        }

        _State = PlayState.Init;

        // Make sure Bass is ready to begin playing again
        Bass.BASS_Start();

        float crossOverSeconds = 0;

        if (_CrossFadeIntervalMS > 0)
          crossOverSeconds = (float)_CrossFadeIntervalMS / 1000f;

        if (filePath != String.Empty)
        {
          FilePath = filePath;

          // create the stream
          if (filePath.Contains(@"http://"))
          {
            stream = Bass.BASS_StreamCreateURL(filePath, 0, BASSStream.BASS_SAMPLE_SOFTWARE | BASSStream.BASS_SAMPLE_FLOAT | BASSStream.BASS_STREAM_AUTOFREE, null, 0);
            Log.Debug("BASSAudio: Webstream found - trying to fetch stream {0}", Convert.ToString(stream));
          }
          else if (IsMODFile(filePath))
            stream = Bass.BASS_MusicLoad(filePath, 0, 0, BASSMusic.BASS_SAMPLE_SOFTWARE | BASSMusic.BASS_SAMPLE_FLOAT | BASSMusic.BASS_MUSIC_AUTOFREE | BASSMusic.BASS_MUSIC_PRESCAN, 0);
          else
            stream = Bass.BASS_StreamCreateFile(filePath, 0, 0, BASSStream.BASS_SAMPLE_SOFTWARE | BASSStream.BASS_SAMPLE_FLOAT | BASSStream.BASS_STREAM_AUTOFREE);

          Streams[CurrentStreamIndex] = stream;

          if (stream != 0)
          {
            StreamEventSyncHandles[CurrentStreamIndex] = RegisterPlaybackEvents(stream, CurrentStreamIndex);

            if (doFade && _CrossFadeIntervalMS > 0)
            {
              _CrossFading = true;

              // Reduce the stream volume to zero so we can fade it in...
              Bass.BASS_ChannelSetAttributes(stream, -1, 0, -101);

              // Fade in from 0 to 100 over the _CrossFadeIntervalMS duration 
              Bass.BASS_ChannelSlideAttributes(stream, -1, 100, -101, _CrossFadeIntervalMS); // will Fadeout a channel's volume over a period of 4 seconds.
            }

            // Attach active DSP effects to the Stream
            if (_dspActive)
            {
              // BASS effects
              if (_stacker != null)
              {
                _stacker.ChannelHandle = stream;
                _stacker.Start();
              }
              if (_damp != null)
              {
                BassFx.BASS_FX_DSP_Set(stream, BASSFXDsp.BASS_FX_DSPFX_DAMP, _dampPrio);
                BassFx.BASS_FX_DSP_SetParameters(stream, _damp);
              }
              if (_comp != null)
              {
                BassFx.BASS_FX_DSP_Set(stream, BASSFXDsp.BASS_FX_DSPFX_COMPRESSOR, _compPrio);
                BassFx.BASS_FX_DSP_SetParameters(stream, _comp);
              }

              // VST Plugins
              foreach (string plugin in _VSTPlugins)
              {
                int vstHandle = BassVst.BASS_VST_ChannelSetDSP(stream, plugin, BASSVSTDsp.BASS_VST_DEFAULT, 1);
                // Copy the parameters of the plugin as loaded on from the settings
                int vstParm = _vstHandles[plugin];
                BassVst.BASS_VST_SetParamCopyParams(vstParm, vstHandle);
              }

              // Init Winamp DSP only if we got a winamp plugin actiavtes
              int waDspPlugin = 0;
              if (Settings.Instance.WinAmpPlugins.Count > 0 && !_waDspInitialised)
              {
                BassWa.BASS_WADSP_Init(GUIGraphicsContext.ActiveForm);
                _waDspInitialised = true;
                foreach (WinAmpPlugin plugins in Settings.Instance.WinAmpPlugins)
                {
                  waDspPlugin = BassWa.BASS_WADSP_Load(plugins.PluginDll, 5, 5, 100, 100, null);
                  if (waDspPlugin > 0)
                  {
                    _waDspPlugins[plugins.PluginDll] = waDspPlugin;
                    BassWa.BASS_WADSP_Start(waDspPlugin, 0, 0);
                  }
                  else
                  {
                    Log.Debug("Couldn't load WinAmp Plugin {0}. Error code: {1}", plugins.PluginDll, Enum.GetName(typeof(BASSErrorCode),Bass.BASS_ErrorGetCode()));
                  }
                }
              }

              foreach (int waPluginHandle in _waDspPlugins.Values)
              {
                BassWa.BASS_WADSP_ChannelSetDSP(waPluginHandle, stream, 1);
              }
            }
          }

          if (stream != 0 && Bass.BASS_ChannelPlay(stream, false))
          {
            Log.Info("BASS: playback started");

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
            msg.Label = FilePath;
            GUIWindowManager.SendThreadMessage(msg);
            NotifyPlaying = true;

            NeedUpdate = true;
            _IsFullScreen = GUIGraphicsContext.IsFullScreenVideo;
            _VideoPositionX = GUIGraphicsContext.VideoWindow.Left;
            _VideoPositionY = GUIGraphicsContext.VideoWindow.Top;
            _VideoWidth = GUIGraphicsContext.VideoWindow.Width;
            _VideoHeight = GUIGraphicsContext.VideoWindow.Height;

            SetVideoWindow();

            PlayState oldState = _State;
            _State = PlayState.Playing;

            if (oldState != _State && PlaybackStateChanged != null)
              PlaybackStateChanged(this, oldState, _State);

            if (PlaybackStart != null)
              PlaybackStart(this, GetTotalStreamSeconds(stream));
          }

          else
          {
            int error = Bass.BASS_ErrorGetCode();

            Log.Error("BASS: Unable to play {0}.  Reason: {1}.", filePath, Enum.GetName(typeof(BASSErrorCode), error));

            // Release all of the sync proc handles
            if (StreamEventSyncHandles[CurrentStreamIndex] != null)
              UnregisterPlaybackEvents(stream, StreamEventSyncHandles[CurrentStreamIndex]);

            result = false;
          }
        }
      }

      catch (Exception ex)
      {
        result = false;
        Log.Error("BASS: Play caused an exception:  {0}.", ex);
      }

      return result;
    }

    // Checks to see, if we got a MOD File
    private bool IsMODFile(string filePath)
    {
      string ext = Path.GetExtension(filePath).ToLower();

      switch (ext)
      {
        case ".mod":
        case ".mo3":
        case ".it":
        case ".xm":
        case ".s3m":
        case ".mtm":
        case ".umx":
          return true;

        default:
          return false;
      }
    }

    private List<int> RegisterPlaybackEvents(int stream, int streamIndex)
    {
      if (stream == 0)
        return null;

      List<int> syncHandles = new List<int>();
      syncHandles.Add(RegisterPlaybackFadeOutEvent(stream, streamIndex, _CrossFadeIntervalMS));
      syncHandles.Add(RegisterPlaybackEndEvent(stream, streamIndex));
      syncHandles.Add(RegisterStreamFreedEvent(stream));

      return syncHandles;
    }

    private int RegisterPlaybackFadeOutEvent(int stream, int streamIndex, int fadeOutMS)
    {
      int syncHandle = 0;
      long len = Bass.BASS_ChannelGetLength(stream); // length in bytes
      float totaltime = Bass.BASS_ChannelBytes2Seconds(stream, len); // the total time length
      float fadeOutSeconds = 0;

      if (fadeOutMS > 0)
        fadeOutSeconds = fadeOutMS / 1000f;

      long bytePos = Bass.BASS_ChannelSeconds2Bytes(stream, totaltime - fadeOutSeconds);

      syncHandle = Bass.BASS_ChannelSetSync(stream,
          BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_POS,
          bytePos, PlaybackFadeOutProcDelegate,
          streamIndex);

      if (syncHandle == 0)
      {
        int error = Bass.BASS_ErrorGetCode();
        //                Console.WriteLine(Enum.GetName(typeof(BASSErrorCode), error));
      }

      return syncHandle;
    }

    private int RegisterPlaybackEndEvent(int stream, int streamIndex)
    {
      int syncHandle = 0;

      syncHandle = Bass.BASS_ChannelSetSync(stream,
          BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_END,
          0, PlaybackEndProcDelegate,
          streamIndex);

      if (syncHandle == 0)
      {
        int error = Bass.BASS_ErrorGetCode();
        //                Console.WriteLine(Enum.GetName(typeof(BASSErrorCode), error));
      }

      return syncHandle;
    }

    private int RegisterStreamFreedEvent(int stream)
    {
      int syncHandle = 0;

      syncHandle = Bass.BASS_ChannelSetSync(stream, BASSSync.BASS_SYNC_FREE, 0, PlaybackStreamFreedProcDelegate, 0);

      if (syncHandle == 0)
      {
        int error = Bass.BASS_ErrorGetCode();
      }

      return syncHandle;
    }

    private bool UnregisterPlaybackEvents(int stream, List<int> syncHandles)
    {
      try
      {
        foreach (int syncHandle in syncHandles)
        {
          if (syncHandle != 0)
            Bass.BASS_ChannelRemoveSync(stream, syncHandle);
        }
      }

      catch
      {
        return false;
      }

      return true;
    }

    private void FreeStream(int stream)
    {
      int streamIndex = -1;

      for (int i = 0; i < Streams.Count; i++)
      {
        if (Streams[i] == stream)
        {
          streamIndex = i;
          break;
        }
      }

      if (streamIndex != -1)
      {
        List<int> eventSyncHandles = StreamEventSyncHandles[streamIndex];

        foreach (int syncHandle in eventSyncHandles)
        {
          Bass.BASS_ChannelRemoveSync(stream, syncHandle);
        }
      }

      Bass.BASS_StreamFree(stream);
      stream = 0;
    }

    private bool StreamIsPlaying(int stream)
    {
      return stream != 0 && (Bass.BASS_ChannelIsActive(stream) == (int)BASSActive.BASS_ACTIVE_PLAYING);
    }

    private float GetTotalStreamSeconds(int stream)
    {
      if (stream == 0)
        return 0;

      // length in bytes
      long len = Bass.BASS_ChannelGetLength(stream);

      // the total time length
      float totaltime = Bass.BASS_ChannelBytes2Seconds(stream, len);
      return totaltime;
    }

    private float GetStreamElapsedTime()
    {
      return GetStreamElapsedTime(GetCurrentStream());
    }

    private float GetStreamElapsedTime(int stream)
    {
      if (stream == 0)
        return 0;

      // position in bytes
      long pos = Bass.BASS_ChannelGetPosition(stream);

      // the elapsed time length
      float elapsedtime = Bass.BASS_ChannelBytes2Seconds(stream, pos);
      return elapsedtime;
    }

    private void PlaybackFadeOutProc(int handle, int stream, int data, int userData)
    {
      //Console.WriteLine("PlaybackFadeOutProc");
      Log.Debug("BASS: PlaybackFadeOutProc of stream {0}", stream);
      _CrossFading = true;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_CROSSFADING, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendThreadMessage(msg);

      if (CrossFade != null)
        CrossFade(this, FilePath);

      Bass.BASS_ChannelSlideAttributes(stream, -1, -2, -101, _CrossFadeIntervalMS);
      bool removed = Bass.BASS_ChannelRemoveSync(stream, handle);
    }

    private void PlaybackEndProc(int handle, int stream, int data, int userData)
    {
      //Console.WriteLine("PlaybackEndProc");
      Log.Debug("BASS: PlaybackEndProc of stream {0}", stream);

      if (TrackPlaybackCompleted != null)
        TrackPlaybackCompleted(this, FilePath);

      PlayState oldState = _State;
      _State = PlayState.Ended;

      if (oldState != _State && PlaybackStateChanged != null)
        PlaybackStateChanged(this, oldState, _State);

      bool removed = Bass.BASS_ChannelRemoveSync(stream, handle);
      HandleSongEnded(false);
    }

    private void PlaybackStreamFreedProc(int handle, int stream, int data, int userData)
    {
      //Console.WriteLine("PlaybackStreamFreedProc");
      Log.Debug("BASS: PlaybackStreamFreedProc of stream {0}", stream);

      for (int i = 0; i < Streams.Count; i++)
      {
        if (stream == Streams[i])
        {
          Streams[i] = 0;
          _CrossFading = false;
          break;
        }
      }
    }

    private void HandleSongEnded(bool bManualStop)
    {
      //Console.WriteLine("BassAudioEngine.HandleSongEnded  manualStop:{0}  CrossFading:{1}", bManualStop, _CrossFading);
      Log.Debug("BASS: HandleSongEnded - manualStop: {0}, CrossFading: {1}", bManualStop, _CrossFading);
      PlayState oldState = _State;

      if (!MediaPortal.Util.Utils.IsAudio(FilePath))
        GUIGraphicsContext.IsFullScreenVideo = false;

      //FilePath = "";

      if (bManualStop)
        ShowVisualizationWindow(false);

      GUIGraphicsContext.IsPlaying = false;

      if (!bManualStop)
      {
        if (_CrossFading)
          _State = PlayState.Playing;

        else
        {
          FilePath = "";
          _State = PlayState.Ended;
        }
      }

      else
        _State = PlayState.Init;

      if (oldState != _State && PlaybackStateChanged != null)
        PlaybackStateChanged(this, oldState, _State);
    }

    private void FadeOutStop(int stream)
    {
      Log.Debug("BASS: FadeOutStop of stream {0}", stream);
      _CrossFading = false;

      if (!StreamIsPlaying(stream))
        return;

      int level = Bass.BASS_ChannelGetLevel(stream);
      Bass.BASS_ChannelSlideAttributes(stream, -1, -2, -101, _CrossFadeIntervalMS);
    }

    public override void Pause()
    {
      _CrossFading = false;
      int stream = GetCurrentStream();

      Log.Debug("BASS: Pause of stream {0}", stream);
      try
      {
        PlayState oldPlayState = _State;

        if (oldPlayState == PlayState.Ended || oldPlayState == PlayState.Init)
          return;

        if (oldPlayState == PlayState.Paused)
        {
          _State = PlayState.Playing;

          if (_SoftStop)
          {
            // Fade-in over 500ms
            Bass.BASS_ChannelSlideAttributes(stream, -1, 100, -101, 500);
            Bass.BASS_Start();
          }

          else
          {
            Bass.BASS_ChannelSetAttributes(stream, -1, 100, -101);
            Bass.BASS_Start();
          }
        }

        else
        {
          _State = PlayState.Paused;

          if (_SoftStop)
          {
            // Fade-out over 500ms
            Bass.BASS_ChannelSlideAttributes(stream, -1, 0, -101, 500);

            // Wait until the slide is done
            while ((Bass.BASS_ChannelIsSliding(stream) & (int)BASSSlide.BASS_SLIDE_VOL) != 0)
              System.Threading.Thread.Sleep(20);

            Bass.BASS_Pause();
          }

          else
            Bass.BASS_Pause();
        }

        if (oldPlayState != _State)
        {
          if (PlaybackStateChanged != null)
            PlaybackStateChanged(this, oldPlayState, _State);
        }
      }

      catch
      {
      }

    }

    public override void Stop()
    {
      _CrossFading = false;

      int stream = GetCurrentStream();
      Log.Debug("BASS: Stop of stream {0}", stream);
      try
      {
        if (_SoftStop)
        {
          Bass.BASS_ChannelSlideAttributes(stream, -1, 0, -101, 500);

          // Wait until the slide is done
          while ((Bass.BASS_ChannelIsSliding(stream) & (int)BASSSlide.BASS_SLIDE_VOL) != 0)
            System.Threading.Thread.Sleep(20);

          Bass.BASS_ChannelStop(stream);
        }

        else
          Bass.BASS_ChannelStop(stream);

        // Free Winamp resources
        try
        {
          // Some Winamp dsps might raise an exception when closing
          foreach (int waDspPlugin in _waDspPlugins.Values)
          {
            BassWa.BASS_WADSP_ChannelRemoveDSP(waDspPlugin);
          }
        }
        catch (Exception)
        { }

        PlayState oldState = _State;
        _State = PlayState.Ended;
        stream = 0;

        if (PlaybackStop != null)
          PlaybackStop(this);

        HandleSongEnded(true);
      }

      catch (Exception ex)
      {
        Log.Error("BASS: Stop command caused an exception - {0}", ex.Message);
      }

      NotifyPlaying = false;
    }

    public override bool CanSeek()
    {
      return true;
    }

    public bool SeekForward(int ms)
    {
      Log.Debug("BASS: SeekForward for {0} ms", Convert.ToString(ms));
      _CrossFading = false;

      if (State != PlayState.Playing)
        return false;

      if (ms <= 0)
        return false;

      bool result = false;

      try
      {
        int stream = GetCurrentStream();
        long len = Bass.BASS_ChannelGetLength(stream);                 // length in bytes
        float totaltime = Bass.BASS_ChannelBytes2Seconds(stream, len); // the total time length
        long pos = Bass.BASS_ChannelGetPosition(stream);               // position in bytes

        float timePos = Bass.BASS_ChannelBytes2Seconds(stream, pos);
        float offsetSecs = (float)ms / 1000f;

        if (timePos + offsetSecs >= totaltime)
          return false;

        Bass.BASS_ChannelSetPosition(stream, (float)(timePos + offsetSecs)); // the elapsed time length
      }

      catch
      {
        result = false;
      }

      return result;
    }

    public bool SeekReverse(int ms)
    {
      Log.Debug("BASS: SeekReverse for {0} ms", Convert.ToString(ms));
      _CrossFading = false;

      if (State != PlayState.Playing)
        return false;

      if (ms <= 0)
        return false;

      int stream = GetCurrentStream();
      bool result = false;

      try
      {
        long len = Bass.BASS_ChannelGetLength(stream);                 // length in bytes
        long pos = Bass.BASS_ChannelGetPosition(stream);               // position in bytes

        float timePos = Bass.BASS_ChannelBytes2Seconds(stream, pos);
        float offsetSecs = (float)ms / 1000f;

        if (timePos - offsetSecs <= 0)
          return false;

        Bass.BASS_ChannelSetPosition(stream, (float)(timePos - offsetSecs)); // the elapsed time length
      }

      catch
      {
        result = false;
      }

      return result;
    }

    public bool SeekToTimePosition(int position)
    {
      Log.Debug("BASS: SeekToTimePosition: {0} ", Convert.ToString(position));
      _CrossFading = false;

      bool result = true;

      try
      {
        int streamIndex = Streams[CurrentStreamIndex];

        if (StreamIsPlaying(streamIndex))
        {
          Bass.BASS_ChannelSetPosition(streamIndex, (float)position);
        }
      }

      catch
      {
        result = false;
      }

      return result;
    }

    public override void SeekRelative(double dTime)
    {
      _CrossFading = false;

      if (_State != PlayState.Init)
      {
        double dCurTime = (double)GetStreamElapsedTime();

        dTime = dCurTime + dTime;

        if (dTime < 0.0d)
          dTime = 0.0d;

        if (dTime < Duration)
          SeekToTimePosition((int)dTime);
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      _CrossFading = false;

      if (_State != PlayState.Init)
      {
        if (dTime < 0.0d)
          dTime = 0.0d;

        if (dTime < Duration)
          SeekToTimePosition((int)dTime);
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      _CrossFading = false;

      if (_State != PlayState.Init)
      {
        double dCurrentPos = (double)GetStreamElapsedTime();
        double dDuration = Duration;
        double fOnePercentDuration = Duration / 100.0d;

        double dSeekPercentageDuration = fOnePercentDuration * (double)iPercentage;
        double dPositionMS = dDuration += dSeekPercentageDuration;

        if (dPositionMS < 0)
          dPositionMS = 0d;

        if (dPositionMS > dDuration)
          dPositionMS = dDuration;

        SeekToTimePosition((int)dDuration);
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      _CrossFading = false;

      if (_State != PlayState.Init)
      {
        if (iPercentage < 0)
          iPercentage = 0;

        if (iPercentage >= 100)
          iPercentage = 100;

        if (iPercentage == 0)
          SeekToTimePosition(0);

        else
          SeekToTimePosition((int)(Duration * ((double)iPercentage / 100d)));
      }
    }

    public override void Process()
    {
      if (!Playing)
        return;

      if (VizManager == null)
        return;

      if (GUIGraphicsContext.BlankScreen) //BAV || (GUIGraphicsContext.Overlay == false && GUIGraphicsContext.IsFullScreenVideo == false))
      {
        //BAV if (GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW)
        {

          if (VizWindow.Visible)
            VizWindow.Visible = false;
        }
      }

      else if (!VizWindow.Visible)
      {
        NeedUpdate = true;
        SetVideoWindow();
        VizWindow.Visible = true;
      }

      if (NotifyPlaying && CurrentPosition >= 10.0)
      {
        NotifyPlaying = false;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC, 0, 0, 0, 0, 0, null);
        msg.Label = CurrentFile;
        GUIWindowManager.SendThreadMessage(msg);
      }

      if (FullScreen != GUIGraphicsContext.IsFullScreenVideo)
        SetVideoWindow();
    }

    public override void SetVideoWindow()
    {
      if (GUIGraphicsContext.IsFullScreenVideo != _IsFullScreen)
      {
        _IsFullScreen = GUIGraphicsContext.IsFullScreenVideo;
        NeedUpdate = true;
      }

      if (!NeedUpdate)
        return;
      NeedUpdate = false;

      if (_IsFullScreen)
      {
        Log.Debug("BASS: Fullscreen");

        _VideoPositionX = GUIGraphicsContext.OverScanLeft;
        _VideoPositionY = GUIGraphicsContext.OverScanTop;

        _VideoWidth = GUIGraphicsContext.OverScanWidth;
        _VideoHeight = GUIGraphicsContext.OverScanHeight;

        VizWindow.Location = new Point(0, 0);
        //VizWindow.Visible = false;

        _videoRectangle = new Rectangle(0, 0, GUIGraphicsContext.Width, GUIGraphicsContext.Height);
        _sourceRectangle = _videoRectangle;

        VizWindow.Size = new System.Drawing.Size(_VideoWidth, _VideoHeight);
        VizWindow.Visible = true;
        Log.Debug("BASS:  Done");

        return;
      }
      else
      {
        VizWindow.Size = new System.Drawing.Size(_VideoWidth, _VideoHeight);

        VizWindow.Location = new Point(_VideoPositionX, _VideoPositionY);
        _videoRectangle = new Rectangle(_VideoPositionX, _VideoPositionY, VizWindow.Size.Width, VizWindow.Size.Height);
        _sourceRectangle = _videoRectangle;
      }

      VizWindow.Visible = _State == PlayState.Playing;
    }

    private delegate void ShowVisualizationWindowDelegate(bool visible);

    private void ShowVisualizationWindow(bool visible)
    {
      if (VizWindow == null)
        return;

      if (VizWindow.InvokeRequired)
      {
        ShowVisualizationWindowDelegate d = new ShowVisualizationWindowDelegate(ShowVisualizationWindow);
        VizWindow.Invoke(d, new object[] { visible });
      }

      else
        VizWindow.Visible = visible;
    }

    public override void Release()
    {
      if (VizWindow != null)
        VizWindow.Visible = false;

      Stop();
    }
  }
}