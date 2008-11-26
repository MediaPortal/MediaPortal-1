#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

using Un4seen.Bass;
using Un4seen.BassAsio;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Vst;
using Un4seen.Bass.AddOn.Wa;
using Un4seen.Bass.Misc;

using MediaPortal.Player.DSP;
using MediaPortal.GUI.Library;
using MediaPortal.Visualization;
using MediaPortal.Configuration;

namespace MediaPortal.Player
{
  /// <summary>
  /// This singleton class is responsible for managing the BASS audio Engine object. 
  /// </summary>
  public class BassMusicPlayer
  {
    #region Variables
    internal static BassAudioEngine _Player;
    private static System.Threading.Thread BassAsyncLoadThread = null;
    private static bool _IsDefaultMusicPlayer = false;
    private static bool SettingsLoaded = false;
    #endregion

    #region Constructors/Destructors
    // Singleton -- make sure we can't instantiate this class
    private BassMusicPlayer()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Returns the BassAudioEngine Object
    /// </summary>
    public static BassAudioEngine Player
    {
      get
      {
        if (_Player == null)
          _Player = new BassAudioEngine();

        return _Player;
      }
    }

    /// <summary>
    /// Returns a Boolean if the BASS Audio Engine is initialised
    /// </summary>
    public static bool Initialized
    {
      get
      {
        return _Player != null && _Player.Initialized;
      }
    }

    /// <summary>
    /// Returns a Boolean if the BASS Audio Engine is the Default Player selected in Configuration
    /// </summary>
    public static bool IsDefaultMusicPlayer
    {
      get
      {
        if (!SettingsLoaded)
        {
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "player", "BASS engine");
            _IsDefaultMusicPlayer = String.Compare(strAudioPlayer, "BASS engine", true) == 0;
            SettingsLoaded = true;
          }
        }

        return _IsDefaultMusicPlayer;
      }
    }

    /// <summary>
    /// Is the BASS Engine Freed?
    /// </summary>
    public static bool BassFreed
    {
      get { return _Player.BassFreed; }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Create the BASS Audio Engine Objects
    /// </summary>
    public static void CreatePlayerAsync()
    {
      if (_Player != null)
        return;

      System.Threading.ThreadStart ts = new System.Threading.ThreadStart(InternalCreatePlayerAsync);
      BassAsyncLoadThread = new System.Threading.Thread(ts);
      BassAsyncLoadThread.Name = "BassAudio";
      BassAsyncLoadThread.Start();
    }

    /// <summary>
    /// Frees, the BASS Audio Engine.
    /// </summary>
    public static void FreeBass()
    {
      if (_Player == null)
        return;

      _Player.FreeBass();
    }

    public static void ReleaseCDDrives()
    {
      int driveCount = BassCd.BASS_CD_GetDriveCount();
      for (int i = 0; i < driveCount; i++)
      {
        BassCd.BASS_CD_Release(i);
      }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Thread for Creating the BASS Audio Engine objects.
    /// </summary>
    private static void InternalCreatePlayerAsync()
    {
      if (_Player == null)
        _Player = new BassAudioEngine();
    }
    #endregion
  }

  /// <summary>
  /// Handles playback of Audio files and Internet streams via the BASS Audio Engine.
  /// </summary>
  public class BassAudioEngine : IPlayer, IDisposable
  {
    #region Enums
    /// <summary>
    /// The various States for Playback
    /// </summary>
    public enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended
    }

    /// <summary>
    /// States, how the Playback is handled
    /// </summary>
    private enum PlayBackType : int
    {
      NORMAL = 0,
      GAPLESS = 1,
      CROSSFADE = 2
    }
    #endregion

    #region Delegates
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

    public delegate void InternetStreamSongChangedDelegate(object sender);
    public event InternetStreamSongChangedDelegate InternetStreamSongChanged;

    private delegate void InitializeControlsDelegate();

    private SYNCPROC PlaybackFadeOutProcDelegate = null;
    private SYNCPROC PlaybackEndProcDelegate = null;
    private SYNCPROC PlaybackStreamFreedProcDelegate = null;
    private SYNCPROC MetaTagSyncProcDelegate = null;
    #endregion

    #region Variables
    private const int MAXSTREAMS = 2;
    private List<int> Streams = new List<int>(MAXSTREAMS);
    private List<List<int>> StreamEventSyncHandles = new List<List<int>>();
    private List<int> DecoderPluginHandles = new List<int>();
    private int CurrentStreamIndex = 0;

    private PlayState _State = PlayState.Init;
    private string FilePath = string.Empty;
    private VisualizationInfo VizPluginInfo = null;
    private int VizFPS = 20;

    private string _SoundDevice = "";
    private int _CrossFadeIntervalMS = 4000;
    private int _DefaultCrossFadeIntervalMS = 4000;
    private int _BufferingMS = 5000;
    private bool _SoftStop = true;
    private bool _Initialized = false;
    private bool _BassFreed = false;
    private int _StreamVolume = 40;
    private System.Timers.Timer UpdateTimer = new System.Timers.Timer();
    private VisualizationWindow VizWindow = null;
    private VisualizationManager VizManager = null;
    private bool _CrossFading = false;          // true if crossfading has started
    private bool _Mixing = false;
    private int _playBackType;
    private string _prevMetaTag;
    private bool _isRadio = false;
    private bool _isLastFMRadio = false;

    private bool _IsFullScreen = false;
    private int _VideoPositionX = 10;
    private int _VideoPositionY = 10;
    private int _VideoWidth = 100;
    private int _VideoHeight = 100;

    bool NeedUpdate = true;
    bool NotifyPlaying = true;

    private string _cdDriveLetters;               // Contains the Druve letters of all available CD Drives
    private bool _isCDDAFile = false;
    private bool _useASIO = false;
    private string _asioDevice = string.Empty;
    private int _asioDeviceNumber = -1;
    private int _asioNumberChannels = 2;
    private double _asioBalance = 0.00;
    private BassAsioHandler _asioHandler = null; // Make it Global to prevent GC stealing the object

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
    private bool _waDspInitialised = false;
    private Dictionary<string, int> _waDspPlugins = new Dictionary<string, int>();

    private int _mixer = 0;
    // Mixing Matrix
    private float[,] _MixingMatrix = new float[8, 2]{
        {1, 0}, // left front out = left in
        {0, 1}, // right front out = right in
        {1, 0}, // centre out = left in
        {0, 1}, // LFE out = right in
        {1, 0}, // left rear/side out = left in
        {0, 1}, // right rear/side out = right in
        {1, 0}, // left-rear center out = left in
        {0, 1}  // right-rear center out = right in
    };

    private StreamCopy _streamcopy;
    #endregion

    #region Properties
    /// <summary>
    /// Returns the Duration of an Audio Stream
    /// </summary>
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

    /// <summary>
    /// Returns the Current Position in the Stream
    /// </summary>
    public override double CurrentPosition
    {
      get
      {
        int stream = GetCurrentStream();

        if (stream == 0)
          return 0;

        long pos = Bass.BASS_ChannelGetPosition(stream);           // position in bytes

        // In case of last.fm subtract the starting time
        //if (_isLastFMRadio)
        //  pos -= _lastFMSongStartPosition;

        double curPosition = (double)Bass.BASS_ChannelBytes2Seconds(stream, pos); // the elapsed time length
        return curPosition;
      }
    }

    /// <summary>
    /// Returns the Current Play State
    /// </summary>
    public PlayState State
    {
      get { return _State; }
    }

    /// <summary>
    /// Has the Playback Ended?
    /// </summary>
    public override bool Ended
    {
      get { return _State == PlayState.Ended; }
    }

    /// <summary>
    /// Is Playback Paused?
    /// </summary>
    public override bool Paused
    {
      get { return (_State == PlayState.Paused); }
    }

    /// <summary>
    /// Is the Player Playing?
    /// </summary>
    public override bool Playing
    {
      get { return (_State == PlayState.Playing || _State == PlayState.Paused); }
    }

    /// <summary>
    /// Is Player Stopped?
    /// </summary>
    public override bool Stopped
    {
      get { return (_State == PlayState.Init); }
    }

    /// <summary>
    /// Returns the File, currently played
    /// </summary>
    public override string CurrentFile
    {
      get { return FilePath; }
    }

    /// <summary>
    /// Gets/Sets the FPS for Visualisation
    /// </summary>
    public int TargetFPS
    {
      get { return VizManager.TargetFPS; }
      set { VizManager.TargetFPS = value; }
    }

    /// <summary>
    /// Gets/Sets the Playback Volume
    /// </summary>
    public override int Volume
    {
      get { return _StreamVolume; }
      set
      {
        if (_StreamVolume != value)
        {
          if (value > 100)
            value = 100;

          if (value < 0)
            value = 0;

          _StreamVolume = value;
          _StreamVolume = value;
          int streamVol = Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, _StreamVolume);
        }
      }
    }

    /// <summary>
    /// Returns the Playback Speed
    /// </summary>
    public override int Speed
    {
      get { return 1; }
      set { }
    }

    /// <summary>
    /// Gets/Sets the Crossfading Interval
    /// </summary>
    public int CrossFadeIntervalMS
    {
      get { return _CrossFadeIntervalMS; }
      set { _CrossFadeIntervalMS = value; }
    }

    /// <summary>
    /// Gets/Sets the Buffering of BASS Streams
    /// </summary>
    public int BufferingMS
    {
      get { return _BufferingMS; }
      set
      {
        if (_BufferingMS == value)
          return;

        _BufferingMS = value;
        int buffering = Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, _BufferingMS);
      }
    }

    /// <summary>
    /// Returns the instance of the Visualisation Manager
    /// </summary>
    public Visualization.IVisualizationManager IVizManager
    {
      get { return VizManager; }
    }

    public override bool IsRadio
    {
      get
      {
        return _isRadio;
      }
    }

    public override bool IsCDA
    {
      get
      {
        return _isCDDAFile;
      }
    }

    public override bool HasVideo
    {
      get { return true; }
    }

    /// <summary>
    /// Gets/Sets Fullscreen Status
    /// </summary>
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

    /// <summary>
    /// Gets/sets the X Coordiante for Video
    /// </summary>
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

    /// <summary>
    /// Gets/sets the Y Coordinate for Video
    /// </summary>
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

    /// <summary>
    /// Gets/Sets the Width of the Video
    /// </summary>
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

    /// <summary>
    /// Gets/sets the Height of the Video
    /// </summary>
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

    /// <summary>
    /// Returns the Playback Type
    /// </summary>
    public override int PlaybackType
    {
      get
      {
        return _playBackType;
      }
    }

    /// <summary>
    /// Returns the instance of the Video Window
    /// </summary>
    public VisualizationWindow VisualizationWindow
    {
      get { return VizWindow; }
    }

    /// <summary>
    /// Is the Audio Engine initialised
    /// </summary>
    public bool Initialized
    {
      get { return _Initialized; }
    }

    /// <summary>
    /// Is Crossfading enabled
    /// </summary>
    public bool CrossFading
    {
      get { return _CrossFading; }
    }

    /// <summary>
    /// Is Crossfading enabled
    /// </summary>
    public bool CrossFadingEnabled
    {
      get { return _CrossFadeIntervalMS > 0; }
    }

    /// <summary>
    /// Is BASS freed?
    /// </summary>
    public bool BassFreed
    {
      get { return _BassFreed; }
    }

    /// <summary>
    /// Returns the Stream, currently played
    /// </summary>
    public override int CurrentAudioStream
    {
      get { return GetCurrentVizStream(); }
    }
    #endregion

    #region Constructors/Destructors
    public BassAudioEngine()
    {
      Initialize();
      GUIGraphicsContext.OnNewAction += new OnActionHandler(OnNewAction);
    }
    #endregion

    #region ActionHandler
    void OnNewAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_FORWARD:
        case Action.ActionType.ACTION_MUSIC_FORWARD:
          {
            if (g_Player.IsMusic)
            {
              g_Player.SeekStep(true);
              string strStatus = g_Player.GetStepDescription();
            }
            //Console.WriteLine(strStatus);
            break;
          }

        case Action.ActionType.ACTION_REWIND:
        case Action.ActionType.ACTION_MUSIC_REWIND:
          {
            if (g_Player.IsMusic)
            {
              g_Player.SeekStep(false);
              string strStatus = g_Player.GetStepDescription();
            }
            //Console.WriteLine(strStatus);
            break;
          }
        case Action.ActionType.ACTION_TOGGLE_MUSIC_GAP:
          {
            _playBackType++;
            if (_playBackType > 2)
              _playBackType = 0;

            VizWindow.SelectedPlaybackType = _playBackType;

            string type = "";
            switch (_playBackType)
            {
              case (int)PlayBackType.NORMAL:
                _CrossFadeIntervalMS = 100;
                type = "Normal";
                break;

              case (int)PlayBackType.GAPLESS:
                _CrossFadeIntervalMS = 200;
                type = "Gapless";
                break;

              case (int)PlayBackType.CROSSFADE:
                _CrossFadeIntervalMS = _DefaultCrossFadeIntervalMS == 0 ? 4000 : _DefaultCrossFadeIntervalMS;
                type = "Crossfading";
                break;
            }
            Log.Info("BASS: Playback changed to {0}", type);
            break;
          }
      }
    }

    /// <summary>
    /// The Main Form is disposed, so dispose the player
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnAppFormDisposed(object sender, EventArgs e)
    {
      Dispose();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Dispose the BASS Audio engine. Free all BASS and Visualisation related resources
    /// </summary>
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
      if (_useASIO)
      {
        BassAsio.BASS_ASIO_Stop();
        BassAsio.BASS_ASIO_Free();
      }
      if (_mixer != 0)
        Bass.BASS_ChannelStop(_mixer);

      Bass.BASS_Stop();
      Bass.BASS_Free();

      foreach (int stream in Streams)
        FreeStream(stream);

      foreach (int pluginHandle in DecoderPluginHandles)
        Bass.BASS_PluginFree(pluginHandle);

      VizManager.Dispose();
      VizWindow.Dispose();
    }

    /// <summary>
    /// Initialise the Visualisation Window and Load Decoder/DSP Plugins
    /// The BASS engine itself is not initialised at this stage, since it may cause S/PDIF for Movies not working on some systems.
    /// </summary>
    private void Initialize()
    {
      try
      {
        Log.Info("BASS: Initialize BASS environment ...");
        LoadSettings();

        BassRegistration.BassRegistration.Register();
        Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, _StreamVolume);

        if (_Mixing || _useASIO)
          // In case of mixing use a Buffer of 500ms only, because the Mixer plays the complete bufer, before for example skipping
          BufferingMS = 500;
        else
          Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, _BufferingMS);

        for (int i = 0; i < MAXSTREAMS; i++)
          Streams.Add(0);

        PlaybackFadeOutProcDelegate = new SYNCPROC(PlaybackFadeOutProc);
        PlaybackEndProcDelegate = new SYNCPROC(PlaybackEndProc);
        PlaybackStreamFreedProcDelegate = new SYNCPROC(PlaybackStreamFreedProc);
        MetaTagSyncProcDelegate = new SYNCPROC(MetaTagSyncProc);

        StreamEventSyncHandles.Add(new List<int>());
        StreamEventSyncHandles.Add(new List<int>());

        InitializeControls();
        LoadAudioDecoderPlugins();
        LoadDSPPlugins();
        GetCDDrives();
        Log.Info("BASS: Initializing BASS environment done.");

        _Initialized = true;
        _BassFreed = true;
      }

      catch (Exception ex)
      {
        Log.Error("BASS: Initialize thread failed.  Reason: {0}", ex.Message);
      }
    }

    /// <summary>
    /// Free BASS, when not playing Audio content, as it might cause S/PDIF output stop working
    /// </summary>
    public void FreeBass()
    {
      if (!_BassFreed)
      {
        Log.Info("BASS: Freeing BASS. Non-audio media playback requested.");
        if (_useASIO)
        {
          BassAsio.BASS_ASIO_Stop();
          BassAsio.BASS_ASIO_Free();
        }
        if (_mixer != 0)
        {
          Bass.BASS_ChannelStop(_mixer);
          _mixer = 0;
        }

        Bass.BASS_Free();
        _BassFreed = true;
      }
    }

    /// <summary>
    /// Init BASS, when a Audio file is to be played
    /// </summary>
    public void InitBass()
    {
      try
      {
        Log.Info("BASS: Initializing BASS audio engine...");
        bool initOK = false;
        if (_useASIO)
        {
          Log.Info("BASS: Using ASIO device: {0}", _asioDevice);
          string[] asioDevices = BassAsio.BASS_ASIO_GetDeviceDescriptions();
          // Check if the ASIO device read is amongst the one retrieved
          for (int i = 0; i < asioDevices.Length; i++)
          {
            if (asioDevices[i] == _asioDevice)
            {
              _asioDeviceNumber = i;
              break;
            }
          }
          if (_asioDeviceNumber > -1)
          {
            // not playing anything via BASS, so don't need an update thread
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 0);
            // setup BASS - "no sound" device but 48000 (default for ASIO)
            initOK = (Bass.BASS_Init(0, 48000, 0, 0, null) && BassAsio.BASS_ASIO_Init(_asioDeviceNumber));

            // Get number of available output Channels
            BASS_ASIO_INFO info = BassAsio.BASS_ASIO_GetInfo();
            if (info != null)
              _asioNumberChannels = info.outputs;

            Log.Info("BASS: ASIO output to {0} channels", _asioNumberChannels);

            // When used in config the ASIO_INIT fails. Ignore it here, to be able using the visualisations
            if (Application.ExecutablePath.Contains("Configuration"))
              initOK = true;
          }
          else
          {
            initOK = false;
            Log.Error("BASS: Specified ASIO device not found. BASS is disabled.");
          }
        }
        else
        {
          int soundDevice = GetSoundDevice();

          initOK = (Bass.BASS_Init(soundDevice, 44100, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_LATENCY, 0, null));
        }
        if (initOK)
        {
          // Create an 8 Channel Mixer, which should be running until stopped.
          // The streams to play are added to the active screen
          if (_Mixing && _mixer == 0)
          {
            _mixer = BassMix.BASS_Mixer_StreamCreate(44100, 8, BASSStream.BASS_MIXER_NONSTOP | BASSStream.BASS_STREAM_AUTOFREE);
          }
          else if (_useASIO && _mixer == 0)
          {
            // For ASIO we neeed an Decoding Mixer with the number of Channels equals the ASIO Channels
            _mixer = BassMix.BASS_Mixer_StreamCreate(44100, _asioNumberChannels, BASSStream.BASS_MIXER_NONSTOP | BASSStream.BASS_STREAM_DECODE);
            // assign ASIO and assume the ASIO format, samplerate and number of channels from the BASS stream
            _asioHandler = new BassAsioHandler(0, 0, _mixer);
          }

          Log.Info("BASS: Initialization done.");
          _Initialized = true;
          _BassFreed = false;
        }
        else
        {
          int error = Bass.BASS_ErrorGetCode();
          if (_useASIO)
          {
            int errorasio = BassAsio.BASS_ASIO_ErrorGetCode();
            Log.Error("BASS: Error initializing BASS audio engine {0} Asio: {1}", Enum.GetName(typeof(BASSErrorCode), error), Enum.GetName(typeof(BASSErrorCode), errorasio));
          }
          else
            Log.Error("BASS: Error initializing BASS audio engine {0}", Enum.GetName(typeof(BASSErrorCode), error));
        }
      }
      catch (Exception ex)
      {
        Log.Error("BASS: Initialize failed. Reason: {0}", ex.Message);
      }
    }

    /// <summary>
    /// Get the Sound devive as set in the Configuartion
    /// </summary>
    /// <returns></returns>
    private int GetSoundDevice()
    {
      int sounddevice = -1;
      // Check if the specified Sounddevice still exists
      if (_SoundDevice == "Default Sound Device")
      {
        Log.Info("BASS: Using default Sound Device");
        sounddevice = -1;
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
            sounddevice = i;
            break;
          }
        }
        if (!foundDevice)
        {
          Log.Warn("BASS: specified Sound device does not exist. Using default Sound Device");
          sounddevice = -1;
        }
        else
        {
          Log.Info("BASS: Using Sound Device {0}", _SoundDevice);
        }
      }
      return sounddevice;
    }

    /// <summary>
    /// Initialise Visulaisation Controls and Create the Visualisation selected in the Configuration
    /// </summary>
    private void InitializeControls()
    {
      if (GUIGraphicsContext.form.InvokeRequired)
      {
        InitializeControlsDelegate d = new InitializeControlsDelegate(InitializeControls);
        GUIGraphicsContext.form.Invoke(d);
        return;
      }

      GUIGraphicsContext.form.Disposed += new EventHandler(OnAppFormDisposed);

      VizWindow = new VisualizationWindow(this);
      VizManager = new Visualization.VisualizationManager(this, VizWindow);
      TargetFPS = VizFPS;

      if (VizManager != null)
      {
        if (VizPluginInfo != null)
          this.CreateVisualization(VizPluginInfo);
      }
    }

    /// <summary>
    /// Load Settings 
    /// </summary>
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

        _DefaultCrossFadeIntervalMS = _CrossFadeIntervalMS;

        _SoftStop = xmlreader.GetValueAsBool("audioplayer", "fadeOnStartStop", true);

        _Mixing = xmlreader.GetValueAsBool("audioplayer", "mixing", false);

        _useASIO = xmlreader.GetValueAsBool("audioplayer", "asio", false);
        _asioDevice = xmlreader.GetValueAsString("audioplayer", "asiodevice", "None");
        _asioBalance = (double)xmlreader.GetValueAsInt("audioplayer", "asiobalance", 0) / 100.00;

        bool doGaplessPlayback = xmlreader.GetValueAsBool("audioplayer", "gaplessPlayback", false);

        if (doGaplessPlayback)
        {
          _CrossFadeIntervalMS = 200;
          _playBackType = (int)PlayBackType.GAPLESS;
        }
        else
        {
          if (_CrossFadeIntervalMS == 0)
          {
            _playBackType = (int)PlayBackType.NORMAL;
            _CrossFadeIntervalMS = 100;
          }
          else
            _playBackType = (int)PlayBackType.CROSSFADE;
        }
      }
    }

    /// <summary>
    /// Setup the Visualisation Window and add it to the Main Form control collection
    /// </summary>
    private void SetVisualizationWindow()
    {
      GUIGraphicsContext.form.SuspendLayout();

      bool foundWindow = false;

      // Check if the MP window already has our viz window in it's control collection...
      foreach (Control ctrl in GUIGraphicsContext.form.Controls)
      {
        if (ctrl.Name == "NativeVisualizationWindow" && ctrl is VisualizationWindow)
        {
          foundWindow = true;
          break;
        }
      }

      if (!foundWindow)
      {
        VizWindow.Visible = false;
        VizWindow.Location = new System.Drawing.Point(8, 16);
        VizWindow.Name = "NativeVisualizationWindow";
        VizWindow.Size = new System.Drawing.Size(0, 0);
        VizWindow.TabIndex = 0;
        VizWindow.Enabled = false;
        GUIGraphicsContext.form.Controls.Add(VizWindow);
      }

      GUIGraphicsContext.form.ResumeLayout();
    }

    /// <summary>
    /// Remove the Visualisation Window from the Main Form control collection when playback has stopped
    /// It was causing troubles to other Controls. For example the WMP player.
    /// </summary>
    private void RemoveVisualizationWindow()
    {
      GUIGraphicsContext.form.SuspendLayout();

      // Check if the MP window already has our viz window in it's control collection...
      foreach (Control ctrl in GUIGraphicsContext.form.Controls)
      {
        if (ctrl.Name == "NativeVisualizationWindow" && ctrl is VisualizationWindow)
        {
          GUIGraphicsContext.form.Controls.Remove(VizWindow);
          break;
        }
      }

      GUIGraphicsContext.form.ResumeLayout();
    }

    /// <summary>
    /// Start the thread for Creating the Visualisation async
    /// </summary>
    /// <param name="visPath"></param>
    public void AsyncCreateVisualization(string visPath)
    {
      System.Threading.Thread createVizThread;
      createVizThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(InternalCreateVisualization));
      createVizThread.IsBackground = true;
      createVizThread.Name = "BASS Viz starter";
      createVizThread.Start(visPath);
    }

    /// <summary>
    /// Thread for creating the Visualisation
    /// </summary>
    /// <param name="vizPluginInfo"></param>
    private void InternalCreateVisualization(object vizPluginInfo)
    {
      CreateVisualization((VisualizationInfo)vizPluginInfo);
    }

    /// <summary>
    /// Create the Visualisation 
    /// </summary>
    /// <param name="vizPluginInfo"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Return the BASS Stream to be used for Visualisation purposes.
    /// We will extract the WAVE and FFT data to be provided to the Visualisation Plugins
    /// In case of Mixer active, we need to return the Mixer Stream. 
    /// In all other cases the current actove stream is used.
    /// </summary>
    /// <returns></returns>
    internal int GetCurrentVizStream()
    {
      if (Streams.Count == 0)
        return -1;

      // In case od ASIO return the clone of the stream, because for a decoding channel, we can't get data from the original stream
      if (_useASIO)
        return _streamcopy.Stream;

      if (_Mixing)
      {
        return _mixer;
      }
      else
      {
        return GetCurrentStream();
      }
    }

    /// <summary>
    /// Returns the Current Stream 
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Returns the Next Stream
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Timer to update the Plaback Process
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

    /// <summary>
    /// Load External BASS Audio Decoder Plugins
    /// </summary>
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

        pluginHandle = Bass.BASS_PluginLoad(file.FullName);

        if (pluginHandle != 0)
        {
          DecoderPluginHandles.Add(pluginHandle);
          decoderCount++;
          Log.Debug("BASS: Added DecoderPlugin: {0}", file.FullName);
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

    /// <summary>
    /// Load BASS DSP Plugins specified setup in the Configuration
    /// </summary>
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
      string vstPluginDir = Path.Combine(Application.StartupPath, @"musicplayer\plugins\dsp");
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

            _comp.Preset_Medium();
            _comp.fThreshold = (float)Un4seen.Bass.Utils.DBToLevel(Convert.ToInt32(value) / 10d, 1.0);
          }
          break;
      }
    }

    private void GetCDDrives()
    {
      // Get the number of CD/DVD drives
      int driveCount = BassCd.BASS_CD_GetDriveCount();
      StringBuilder builderDriveLetter = new StringBuilder();
      // Get Drive letters assigned
      for (int i = 0; i < driveCount; i++)
      {
        builderDriveLetter.Append(BassCd.BASS_CD_GetDriveLetterChar(i));
      }
      _cdDriveLetters = builderDriveLetter.ToString();
    }

    /// <summary>
    /// Starts Playback of the given file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
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
          // Selected file is equal to current stream
          if (_State == PlayState.Paused)
          {
            // Resume paused stream
            if (_SoftStop)
              Bass.BASS_ChannelSlideAttributes(stream, -1, 100, -101, 500);
            else
              Bass.BASS_ChannelSetAttributes(stream, -1, 100, -101);

            result = Bass.BASS_Start();

            if (_useASIO)
              result = BassAsio.BASS_ASIO_Start(0);

            if (result)
            {
              _State = PlayState.Playing;

              if (PlaybackStateChanged != null)
                PlaybackStateChanged(this, PlayState.Paused, _State);
            }

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

          if ((oldStreamDuration - (oldStreamElapsedSeconds + crossFadeSeconds) > -1))
          {
            FadeOutStop(oldStream);
          }
          else
            Bass.BASS_ChannelStop(oldStream);

          doFade = true;
          stream = GetNextStream();

          if (stream != 0 || StreamIsPlaying(stream))
            FreeStream(stream);
        }

        if (stream != 0)
        {
          if (!Stopped)       // Check if stopped already to avoid that Stop() is called two or three times
            Stop();
          FreeStream(stream);
        }

        _State = PlayState.Init;

        // Make sure Bass is ready to begin playing again
        Bass.BASS_Start();

        float crossOverSeconds = 0;

        if (_CrossFadeIntervalMS > 0)
          crossOverSeconds = (float)_CrossFadeIntervalMS / 1000f;

        if (filePath != string.Empty)
        {
          // Turn on parsing of ASX files
          Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_WMA_ASX, 1);

          // We need different flags for standard BASS and ASIO / Mixing
          BASSStream streamFlags;
          if (_useASIO || _Mixing)
            streamFlags = BASSStream.BASS_STREAM_DECODE | BASSStream.BASS_SAMPLE_FLOAT;  // Don't use the BASS_STREAM_AUTOFREE flag on a decoding channel. will produce a BASS_ERROR_NOTAVAIL
          else
            streamFlags = BASSStream.BASS_STREAM_AUTOFREE;

          FilePath = filePath;

          // create the stream
          _isCDDAFile = false;
          _isRadio = false;
          _isLastFMRadio = false;
          if (MediaPortal.Util.Utils.IsCDDA(filePath))
          {
            _isCDDAFile = true;

            // StreamCreateFile causes problems with Multisession disks, so use StreamCreate with driveindex and track index
            int driveindex = _cdDriveLetters.IndexOf(filePath.Substring(0, 1));
            int tracknum = Convert.ToInt16(filePath.Substring(filePath.IndexOf(".cda") - 2, 2));
            stream = BassCd.BASS_CD_StreamCreate(driveindex, tracknum - 1, streamFlags);
            if (stream == 0)
              Log.Error("BASS: CD: {0}.", Enum.GetName(typeof(BASSErrorCode), Bass.BASS_ErrorGetCode()));
          }
          else if (filePath.ToLower().Contains(@"http://") || filePath.ToLower().Contains(@"https://") ||
                   filePath.ToLower().StartsWith("mms") || filePath.ToLower().StartsWith("rtsp"))
          {
            _isRadio = true;  // We're playing Internet Radio Stream
            _isLastFMRadio = Util.Utils.IsLastFMStream(filePath);

            stream = Bass.BASS_StreamCreateURL(filePath, 0, streamFlags, null, 0);

            if (stream != 0)
            {
              // Get the Tags and set the Meta Tag SyncProc
              _prevMetaTag = string.Empty;
              SetStreamTags(stream);
              GetMetaTags(Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_META));
              Bass.BASS_ChannelSetSync(stream, BASSSync.BASS_SYNC_META, 0, MetaTagSyncProcDelegate, 0);
            }
            Log.Debug("BASSAudio: Webstream found - trying to fetch stream {0}", Convert.ToString(stream));
          }
          else if (IsMODFile(filePath))
            // Load a Mod file
            stream = Bass.BASS_MusicLoad(filePath, 0, 0, BASSMusic.BASS_SAMPLE_SOFTWARE | BASSMusic.BASS_SAMPLE_FLOAT | BASSMusic.BASS_MUSIC_AUTOFREE | BASSMusic.BASS_MUSIC_PRESCAN, 0);
          else
            // Create a Standard Stream
            stream = Bass.BASS_StreamCreateFile(filePath, 0, 0, streamFlags);

          // Is Mixing / ASIO enabled, then we create a mixer channel and assign the stream to the mixer
          if ((_Mixing || _useASIO) && stream != 0)
          {
            // Do an upmix of the stereo according to the matrix. 
            // Now Plugin the stream to the mixer and set the mixing matrix
            BassMix.BASS_Mixer_StreamAddChannel(_mixer, stream, BASSStream.BASS_MIXER_MATRIX | BASSStream.BASS_STREAM_AUTOFREE | BASSStream.BASS_MIXER_NORAMPIN);
            BassMix.BASS_Mixer_ChannelSetMatrix(stream, ref _MixingMatrix[0, 0]);
          }

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
                    Log.Debug("Couldn't load WinAmp Plugin {0}. Error code: {1}", plugins.PluginDll, Enum.GetName(typeof(BASSErrorCode), Bass.BASS_ErrorGetCode()));
                  }
                }
              }

              foreach (int waPluginHandle in _waDspPlugins.Values)
              {
                BassWa.BASS_WADSP_ChannelSetDSP(waPluginHandle, stream, 1);
              }
            }
          }
          else
          {
            int error = Bass.BASS_ErrorGetCode();
            Log.Error("BASS: Unable to create Stream for {0}.  Reason: {1}.", filePath, Enum.GetName(typeof(BASSErrorCode), error));
          }

          bool playbackStarted = false;
          if (_Mixing)
          {
            if (Bass.BASS_ChannelIsActive(_mixer) == (int)BASSActive.BASS_ACTIVE_PLAYING)
              playbackStarted = true;
            else
              playbackStarted = Bass.BASS_ChannelPlay(_mixer, false);
          }
          else if (_useASIO)
          {
            // Get some information about the stream
            BASS_CHANNELINFO info = new BASS_CHANNELINFO();
            Bass.BASS_ChannelGetInfo(stream, info);

            // In order to provide data for visualisation we need to clone the stream
            _streamcopy = new StreamCopy();
            _streamcopy.ChannelHandle = stream;
            _streamcopy.StreamFlags = BASSStream.BASS_STREAM_DECODE | BASSStream.BASS_SAMPLE_FLOAT; // decode the channel, so that we have a Streamcopy

            _asioHandler.Pan = _asioBalance;
            _asioHandler.Volume = (double)_StreamVolume / 100;

            // Set the Sample Rate from the stream
            _asioHandler.SampleRate = (double)info.freq;
            // try to set the device rate too (saves resampling)
            BassAsio.BASS_ASIO_SetRate((double)info.freq);

            try
            {
              _streamcopy.Start();   // start the cloned stream
            }
            catch (Exception)
            {
              Log.Error("Captured an error on StreamCopy start");
            }

            if (BassAsio.BASS_ASIO_IsStarted())
              playbackStarted = true;
            else
            {
              BassAsio.BASS_ASIO_Stop();
              playbackStarted = BassAsio.BASS_ASIO_Start(0);
            }
          }
          else
            playbackStarted = Bass.BASS_ChannelPlay(stream, false);

          if (stream != 0 && playbackStarted)
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

            // Re-Add the Viswindow to the Mainform Control (It got removed on a manual Stop)
            SetVisualizationWindow();
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

    /// <summary>
    /// Is this a MOD file?
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Register the various Playback Events
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="streamIndex"></param>
    /// <returns></returns>
    private List<int> RegisterPlaybackEvents(int stream, int streamIndex)
    {
      if (stream == 0)
        return null;

      List<int> syncHandles = new List<int>();

      // Don't register the fade out event for last.fm radio, as it causes problems
      // if (!_isLastFMRadio)
        syncHandles.Add(RegisterPlaybackFadeOutEvent(stream, streamIndex, _CrossFadeIntervalMS));

      syncHandles.Add(RegisterPlaybackEndEvent(stream, streamIndex));
      syncHandles.Add(RegisterStreamFreedEvent(stream));

      return syncHandles;
    }

    /// <summary>
    /// Register the Fade out Event
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="streamIndex"></param>
    /// <param name="fadeOutMS"></param>
    /// <returns></returns>
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
        Log.Debug("BASS: RegisterPlaybackFadeOutEvent of stream {0} failed with error {1}", stream, error);
      }

      return syncHandle;
    }

    /// <summary>
    /// Register the Playback end Event
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="streamIndex"></param>
    /// <returns></returns>
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
        Log.Debug("BASS: RegisterPlaybackEndEvent of stream {0} failed with error {1}", stream, error);
      }

      return syncHandle;
    }

    /// <summary>
    /// Register Stream Free Event
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private int RegisterStreamFreedEvent(int stream)
    {
      int syncHandle = 0;

      syncHandle = Bass.BASS_ChannelSetSync(stream, BASSSync.BASS_SYNC_FREE, 0, PlaybackStreamFreedProcDelegate, 0);

      if (syncHandle == 0)
      {
        int error = Bass.BASS_ErrorGetCode();
        Log.Debug("BASS: RegisterStreamFreedEvent of stream {0} failed with error {1}", stream, error);
      }

      return syncHandle;
    }

    /// <summary>
    /// Unregister the Playback Events
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="syncHandles"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Free a Stream
    /// </summary>
    /// <param name="stream"></param>
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

      _CrossFading = false;       // Set crossfading to false, Play() will update it when the next song starts
    }

    /// <summary>
    /// Is stream Playing?
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private bool StreamIsPlaying(int stream)
    {
      return stream != 0 && (Bass.BASS_ChannelIsActive(stream) == (int)BASSActive.BASS_ACTIVE_PLAYING);
    }

    /// <summary>
    /// Get Total Seconds of the Stream
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Retrieve the elapsed time
    /// </summary>
    /// <returns></returns>
    private float GetStreamElapsedTime()
    {
      return GetStreamElapsedTime(GetCurrentStream());
    }

    /// <summary>
    /// Retrieve the elapsed time
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Fade Out  Procedure
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    /// <param name="userData"></param>
    private void PlaybackFadeOutProc(int handle, int stream, int data, int userData)
    {
      Log.Debug("BASS: PlaybackFadeOutProc of stream {0}", stream);

      if (_CrossFadeIntervalMS > 0)
      {
        // Only sent GUI_MSG_PLAYBACK_CROSSFADING when gapless/crossfading mode is used
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_CROSSFADING, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendThreadMessage(msg);
      }

      if (CrossFade != null)
        CrossFade(this, FilePath);

      Bass.BASS_ChannelSlideAttributes(stream, -1, -2, -101, _CrossFadeIntervalMS);
      bool removed = Bass.BASS_ChannelRemoveSync(stream, handle);
      if (removed)
        Log.Debug("BassAudio: *** BASS_ChannelRemoveSync in PlaybackFadeOutProc");
    }

    /// <summary>
    /// Playback end Procedure
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    /// <param name="userData"></param>
    private void PlaybackEndProc(int handle, int stream, int data, int userData)
    {
      Log.Debug("BASS: PlaybackEndProc of stream {0}", stream);

      if (TrackPlaybackCompleted != null)
        TrackPlaybackCompleted(this, FilePath);

      bool removed = Bass.BASS_ChannelRemoveSync(stream, handle);
      if (removed)
        Log.Debug("BassAudio: *** BASS_ChannelRemoveSync in PlaybackEndProc");
    }

    /// <summary>
    /// Stream Freed Proc
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    /// <param name="userData"></param>
    private void PlaybackStreamFreedProc(int handle, int stream, int data, int userData)
    {
      //Console.WriteLine("PlaybackStreamFreedProc");
      Log.Debug("BASS: PlaybackStreamFreedProc of stream {0}", stream);

      HandleSongEnded(false);

      for (int i = 0; i < Streams.Count; i++)
      {
        if (stream == Streams[i])
        {
          Streams[i] = 0;
          break;
        }
      }
    }

    /// <summary>
    /// Gets the tags from the Internet Stream.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="length"></param>
    /// <param name="user"></param>
    private void SetStreamTags(int stream)
    {
      string[] tags = Bass.BASS_ChannelGetTagsICY(stream);
      if (tags != null)
      {
        foreach (string item in tags)
        {
          if (item.ToLower().StartsWith("icy-name:"))
            GUIPropertyManager.SetProperty("#Play.Current.Album", item.Substring(9));

          if (item.ToLower().StartsWith("icy-genre:"))
            GUIPropertyManager.SetProperty("#Play.Current.Genre", item.Substring(10));

          Log.Debug("BASS: Connection Information: {0}", item);
        }
      }
    }

    /// <summary>
    /// This Callback Procedure is called by BASS, once a song changes.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="channel"></param>
    /// <param name="data"></param>
    /// <param name="user"></param>
    private void MetaTagSyncProc(int handle, int channel, int data, int user)
    {
      // BASS_SYNC_META delivers a pointer to the metadata in data parameter...
      if (data != 0)
      {
        GetMetaTags(new IntPtr(data));
      }
    }

    /// <summary>
    /// Set the Properties out of the Tags
    /// </summary>
    /// <param name="tagPtr"></param>
    private void GetMetaTags(IntPtr tagPtr)
    {
      string title = string.Empty;
      string artist = string.Empty;
      try
      {
        string tag = Marshal.PtrToStringAnsi(tagPtr);
        if (tag != null)
        {
          if (tag == _prevMetaTag)
            return;

          _prevMetaTag = tag;
          Log.Info("BASS: Title sent via Stream: {0}", tag);
          Regex r = new Regex("StreamTitle='(.+?)';StreamUrl=", RegexOptions.IgnoreCase);
          Match m = r.Match(tag);
          if (m.Success)
          {
            Group g1 = m.Groups[1];
            CaptureCollection captures = g1.Captures;
            Capture c = captures[0];
            Regex r1 = new Regex("( - )");
            string[] s = r1.Split(c.ToString());
            if (s != null)
            {
              artist = s[0];
              title = s[2];
            }
            else
              title = c.ToString();   // Set the Title as sent via the stream
          }
        }
      }
      catch (Exception)
      {
        // No need to do anything, just leave the title and artist empty.
      }
      GUIPropertyManager.SetProperty("#Play.Current.Title", title);
      GUIPropertyManager.SetProperty("#Play.Current.Artist", artist);

      if (InternetStreamSongChanged != null)
        InternetStreamSongChanged(this);
    }

    /// <summary>
    /// Handle Stop of a song
    /// </summary>
    /// <param name="bManualStop"></param>
    private void HandleSongEnded(bool bManualStop)
    {
      Log.Debug("BASS: HandleSongEnded - manualStop: {0}, CrossFading: {1}", bManualStop, _CrossFading);
      PlayState oldState = _State;

      if (!MediaPortal.Util.Utils.IsAudio(FilePath))
        GUIGraphicsContext.IsFullScreenVideo = false;

      if (bManualStop)
      {
        ShowVisualizationWindow(false);
        VizWindow.Run = false;
      }

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

      _CrossFading = false;       // Set crossfading to false, Play() will update it when the next song starts
    }

    /// <summary>
    /// Fade out Song
    /// </summary>
    /// <param name="stream"></param>
    private void FadeOutStop(int stream)
    {
      Log.Debug("BASS: FadeOutStop of stream {0}", stream);

      if (!StreamIsPlaying(stream))
        return;

      int level = Bass.BASS_ChannelGetLevel(stream);
      Bass.BASS_ChannelSlideAttributes(stream, -1, -2, -101, _CrossFadeIntervalMS);

      // When a Channel added to the Mixer is stopped by the above slide command, the end sync proc is not invoked.
      // This causes the song to continue with Volume 0 to be played until it ends, hich causes the next song to be interupted as well
      if (_Mixing || _useASIO)
      {
        System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(CheckForFadeOutEnded), stream);
      }
    }

    /// <summary>
    /// Checks, if the channel slided to Volume 0.
    /// Then the stream should be freed and the syncprocs unregistered.
    /// </summary>
    /// <param name="fadeoutstream"></param>
    private void CheckForFadeOutEnded(object fadeoutstream)
    {
      int stream = (int)fadeoutstream;
      // Wait until the slide is done
      while ((Bass.BASS_ChannelIsSliding(stream) & (int)BASSSlide.BASS_SLIDE_VOL) != 0)
        System.Threading.Thread.Sleep(20);

      // Let the EndProc first do it's job
      System.Threading.Thread.Sleep(1500);
      FreeStream(stream);
    }

    /// <summary>
    /// Pause Playback
    /// </summary>
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

          if (_useASIO)
            BassAsio.BASS_ASIO_Start(0);
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

          if (_useASIO)
            BassAsio.BASS_ASIO_Stop();
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

    /// <summary>
    /// Stopping Playback
    /// </summary>
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
        }
        if (_Mixing || _useASIO)
        {
          Bass.BASS_ChannelStop(stream);
          BassMix.BASS_Mixer_ChannelRemove(stream);
        }
        else
          Bass.BASS_ChannelStop(stream);


        if (_useASIO)
          BassAsio.BASS_ASIO_Stop();

        // Free Winamp resources
        try
        {
          // Some Winamp dsps might raise an exception when closing
          foreach (int waDspPlugin in _waDspPlugins.Values)
          {
            BassWa.BASS_WADSP_Stop(waDspPlugin);
          }
        }
        catch (Exception)
        { }

        // If we did a playback of a Audio CD, release the CD, as we might have problems with other CD related functions
        if (_isCDDAFile)
        {
          int driveCount = BassCd.BASS_CD_GetDriveCount();
          for (int i = 0; i < driveCount; i++)
          {
            BassCd.BASS_CD_Release(i);
          }
        }

        stream = 0;

        if (PlaybackStop != null)
          PlaybackStop(this);

        HandleSongEnded(true);

        // Remove the Viz Window from the Main Form as it causes troubles to other plugin overlay window
        RemoveVisualizationWindow();
      }

      catch (Exception ex)
      {
        Log.Error("BASS: Stop command caused an exception - {0}", ex.Message);
      }

      NotifyPlaying = false;
    }

    /// <summary>
    /// Is Seeking enabled 
    /// </summary>
    /// <returns></returns>
    public override bool CanSeek()
    {
      return true;
    }

    /// <summary>
    /// Seek Forward in the Stream
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
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

        long pos = 0;                                                  // position in bytes
        if (_Mixing)
          pos = BassMix.BASS_Mixer_ChannelGetPosition(stream);
        else
          pos = Bass.BASS_ChannelGetPosition(stream);

        float timePos = Bass.BASS_ChannelBytes2Seconds(stream, pos);
        float offsetSecs = (float)ms / 1000f;

        if (timePos + offsetSecs >= totaltime)
          return false;

        if (_Mixing)
          BassMix.BASS_Mixer_ChannelSetPosition(stream, Bass.BASS_ChannelSeconds2Bytes(stream, timePos + offsetSecs)); // the elapsed time length
        else
          Bass.BASS_ChannelSetPosition(stream, (float)(timePos + offsetSecs)); // the elapsed time length
      }

      catch
      {
        result = false;
      }

      return result;
    }

    /// <summary>
    /// Seek Backwards within the stream
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
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

        long pos = 0;                                                  // position in bytes
        if (_Mixing)
          pos = BassMix.BASS_Mixer_ChannelGetPosition(stream);
        else
          pos = Bass.BASS_ChannelGetPosition(stream);

        float timePos = Bass.BASS_ChannelBytes2Seconds(stream, pos);
        float offsetSecs = (float)ms / 1000f;

        if (timePos - offsetSecs <= 0)
          return false;

        if (_Mixing)
          BassMix.BASS_Mixer_ChannelSetPosition(stream, Bass.BASS_ChannelSeconds2Bytes(stream, timePos - offsetSecs)); // the elapsed time length
        else
          Bass.BASS_ChannelSetPosition(stream, (float)(timePos - offsetSecs)); // the elapsed time length
      }

      catch
      {
        result = false;
      }

      return result;
    }

    /// <summary>
    /// Seek to a specific position in the stream
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool SeekToTimePosition(int position)
    {
      Log.Debug("BASS: SeekToTimePosition: {0} ", Convert.ToString(position));
      _CrossFading = false;

      bool result = true;

      try
      {
        int stream = GetCurrentStream();

        if (StreamIsPlaying(stream))
        {
          if (_Mixing)
            BassMix.BASS_Mixer_ChannelSetPosition(stream, Bass.BASS_ChannelSeconds2Bytes(stream, position));
          else
            Bass.BASS_ChannelSetPosition(stream, (float)position);
        }
      }

      catch
      {
        result = false;
      }

      return result;
    }

    /// <summary>
    /// Seek Relative in the Stream
    /// </summary>
    /// <param name="dTime"></param>
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

    /// <summary>
    /// Seek Absoluet in the Stream
    /// </summary>
    /// <param name="dTime"></param>
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

    /// <summary>
    /// Seek Relative Percentage
    /// </summary>
    /// <param name="iPercentage"></param>
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

    /// <summary>
    /// Seek Absolute Percentage
    /// </summary>
    /// <param name="iPercentage"></param>
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

    /// <summary>
    /// Process Method
    /// </summary>
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

      else if (!VizWindow.Visible && !GUIWindowManager.IsRouted)
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

    /// <summary>
    /// Sets the Video Window
    /// </summary>
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

        _VideoPositionX = GUIGraphicsContext.OverScanLeft + GUIGraphicsContext.OffsetX;
        _VideoPositionY = GUIGraphicsContext.OverScanTop + GUIGraphicsContext.OffsetY;

        _VideoWidth = (int)Math.Round((float)GUIGraphicsContext.OverScanWidth * (float)GUIGraphicsContext.ZoomHorizontal);
        _VideoHeight = (int)Math.Round((float)GUIGraphicsContext.OverScanHeight * (float)GUIGraphicsContext.ZoomVertical);

        VizWindow.Location = new Point(_VideoPositionX, _VideoPositionY);
        //VizWindow.Visible = false;

        _videoRectangle = new Rectangle(_VideoPositionX, _VideoPositionY, _VideoWidth, _VideoHeight);
        _sourceRectangle = _videoRectangle;

        VizWindow.Size = new System.Drawing.Size(_VideoWidth, _VideoHeight);
        VizWindow.Visible = true;
        return;
      }
      else
      {
        VizWindow.Size = new System.Drawing.Size(_VideoWidth, _VideoHeight);

        VizWindow.Location = new Point(_VideoPositionX, _VideoPositionY);
        _videoRectangle = new Rectangle(_VideoPositionX, _VideoPositionY, VizWindow.Size.Width, VizWindow.Size.Height);
        _sourceRectangle = _videoRectangle;
      }

      if (!GUIWindowManager.IsRouted)
        VizWindow.Visible = _State == PlayState.Playing;
    }

    private delegate void ShowVisualizationWindowDelegate(bool visible);

    /// <summary>
    /// Show the Visualisation Window
    /// </summary>
    /// <param name="visible"></param>
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

    /// <summary>
    /// Release the Video Window
    /// </summary>
    public override void Release()
    {
      if (VizWindow != null)
        VizWindow.Visible = false;

      if (!Stopped)         // Check if stopped already to avoid that Stop() is called two or three times
        Stop();
    }
    #endregion
  }
}