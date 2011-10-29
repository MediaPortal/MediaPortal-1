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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Player.DSP;
using MediaPortal.TagReader;
using MediaPortal.Visualization;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Midi;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.AddOn.Vst;
using Un4seen.Bass.AddOn.WaDsp;
using Un4seen.Bass.Misc;
using Un4seen.BassAsio;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.MusicPlayer.BASS
{
  /// <summary>
  /// Handles playback of Audio files and Internet streams via the BASS Audio Engine.
  /// </summary>
  public class BassAudioEngine : IPlayer
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

    #endregion

    #region Delegates

    public delegate void PlaybackStartHandler(object sender, double duration);

    public event PlaybackStartHandler PlaybackStart;

    public delegate void PlaybackStopHandler(object sender);

    public event PlaybackStopHandler PlaybackStop;

    public delegate void PlaybackProgressHandler(object sender, double duration, double curPosition);

    public delegate void TrackPlaybackCompletedHandler(object sender, string filePath);

    public event TrackPlaybackCompletedHandler TrackPlaybackCompleted;

    public delegate void CrossFadeHandler(object sender, string filePath);

    public event CrossFadeHandler CrossFade;

    public delegate void PlaybackStateChangedDelegate(object sender, PlayState oldState, PlayState newState);

    public event PlaybackStateChangedDelegate PlaybackStateChanged;

    public delegate void InternetStreamSongChangedDelegate(object sender);

    public event InternetStreamSongChangedDelegate InternetStreamSongChanged;

    private delegate void InitializeControlsDelegate();

    private delegate void ShowVisualizationWindowDelegate(bool visible);

    #endregion

    #region Variables

    private List<int> DecoderPluginHandles = new List<int>();

    private MusicStream _currentStream = null;
    private MusicStream _oldStream = null;

    private PlayState _State = PlayState.Init;
    private string FilePath = string.Empty;
    private VisualizationInfo VizPluginInfo = null;
    private int VizFPS = 20;

    private int _DefaultCrossFadeIntervalMS = 4000;
    private bool _Initialized = false;
    private bool _BassFreed = false;
    //private Timer UpdateTimer = new Timer();
    private VisualizationWindow VizWindow = null;
    private VisualizationManager VizManager = null;
    private bool _CrossFading = false; // true if crossfading has started
    private int _playBackType;
    private int _savedPlayBackType = -1;
    private bool _isRadio = false;
    private bool _isLastFMRadio = false;

    private bool _IsFullScreen = false;
    private int _VideoPositionX = 10;
    private int _VideoPositionY = 10;
    private int _VideoWidth = 100;
    private int _VideoHeight = 100;

    private bool NeedUpdate = true;
    private bool NotifyPlaying = true;

    
    private bool _isCDDAFile = false;
    private int _asioDeviceNumber = -1;
    private int _speed = 1;
    private DateTime _seekUpdate = DateTime.Now;
    private BassAsioHandler _asioHandler = null; // Make it Global to prevent GC stealing the object

    private int _mixer = 0;
    // Mixing Matrix
    private float[,] _MixingMatrix = new float[8,2]
                                       {
                                         {1, 0}, // left front out = left in
                                         {0, 1}, // right front out = right in
                                         {1, 0}, // centre out = left in
                                         {0, 1}, // LFE out = right in
                                         {1, 0}, // left rear/side out = left in
                                         {0, 1}, // right rear/side out = right in
                                         {1, 0}, // left-rear center out = left in
                                         {0, 1} // right-rear center out = right in
                                       };

    private StreamCopy _streamcopy; // For Asio Channels

    // CUE Support
    private string currentCueFileName = null;
    private string currentCueFakeTrackFileName = null;
    private CueSheet currentCueSheet = null;
    private float cueTrackStartPos = 0;
    private float cueTrackEndPos = 0;
    private int cueTrackEndEventHandler;

    private TAG_INFO _tagInfo;

    #endregion

    #region Properties

    /// <summary>
    /// Returns, if the player is in initialising stage
    /// </summary>
    public override bool Initializing
    {
      get { return (_State == PlayState.Init); }
    }

    /// <summary>
    /// Returns the Duration of an Audio Stream
    /// </summary>
    public override double Duration
    {
      get
      {
        MusicStream stream = _currentStream;

        if (stream == null)
        {
          return 0;
        }

        double duration = (double)GetTotalStreamSeconds(stream);
        if (currentCueSheet != null)
        {
          if (cueTrackEndPos > 0)
          {
            duration = cueTrackEndPos;
          }
          duration -= cueTrackStartPos;
        }
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
        MusicStream stream = _currentStream;

        if (stream == null)
        {
          return 0;
        }

        long pos = Bass.BASS_ChannelGetPosition(stream.BassStream); // position in bytes

        // In case of last.fm subtract the starting time
        //if (_isLastFMRadio)
        //  pos -= _lastFMSongStartPosition;

        double curPosition = (double)Bass.BASS_ChannelBytes2Seconds(stream.BassStream, pos); // the elapsed time length
        if (currentCueSheet != null && cueTrackStartPos > 0)
        {
          curPosition -= cueTrackStartPos;
        }
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
      get { return (currentCueSheet != null ? currentCueFakeTrackFileName : FilePath); }
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
      get { return Config.StreamVolume; }
      set
      {
        if (Config.StreamVolume != value)
        {
          if (value > 100)
          {
            value = 100;
          }

          if (value < 0)
          {
            value = 0;
          }

          Config.StreamVolume = value;
          Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, Config.StreamVolume);
        }
      }
    }

    /// <summary>
    /// Returns the Playback Speed
    /// </summary>
    public override int Speed
    {
      get { return _speed; }
      set { _speed = value; }
    }

    /// <summary>
    /// Returns the instance of the Visualisation Manager
    /// </summary>
    public IVisualizationManager IVizManager
    {
      get { return VizManager; }
    }

    public override bool IsRadio
    {
      get { return _isRadio; }
    }

    public override bool IsCDA
    {
      get { return _isCDDAFile; }
    }

    public override bool HasVideo
    {
      get { return false; }
    }

    public override bool HasViz
    {
      get { return VizPluginInfo.VisualizationType != VisualizationInfo.PluginType.None; }
    }

    /// <summary>
    /// Indicate that we don't need to get disposed between calls
    /// </summary>
    public override bool SupportsReplay
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
      get { return _playBackType; }
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
      get { return Config.CrossFadeIntervalMs > 0; }
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

    private void OnNewAction(Action action)
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
            {
              _playBackType = 0;
            }

            VizWindow.SelectedPlaybackType = _playBackType;

            string type = "";
            switch (_playBackType)
            {
              case (int)Config.PlayBackType.NORMAL:
                Config.CrossFadeIntervalMs = 100;
                type = "Normal";
                break;

              case (int)Config.PlayBackType.GAPLESS:
                Config.CrossFadeIntervalMs = 200;
                type = "Gapless";
                break;

              case (int)Config.PlayBackType.CROSSFADE:
                Config.CrossFadeIntervalMs = _DefaultCrossFadeIntervalMS == 0 ? 4000 : _DefaultCrossFadeIntervalMS;
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
    private void OnAppFormDisposed(object sender, EventArgs e)
    {
      DisposeAndCleanUp();
    }

    void OnMusicStreamMessage(object sender, MusicStream.StreamAction action)
    {
      if (sender == null)
      {
        return;
      }
      MusicStream musicStream = (MusicStream)sender;

      switch (action)
      {
        case MusicStream.StreamAction.Ended:
          if (TrackPlaybackCompleted != null)
          {
            TrackPlaybackCompleted(this, musicStream.FilePath);
          }
          musicStream.Dispose();
          break;

        case MusicStream.StreamAction.CrossFade:
          if (CrossFade != null)
          {
            CrossFade(this, musicStream.FilePath);
          }
          break;

        case MusicStream.StreamAction.InternetStreamChanged:
          if (InternetStreamSongChanged != null)
          {
            InternetStreamSongChanged(this);
          }
          break;
      }

    }

    #endregion

    #region Initialisation

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
        // Set the Global Volume. 0 = silent, 10000 = Full
        // We get 0 - 100 from Configuration, so multiply by 100
        Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, Config.StreamVolume * 100);

        if (Config.Mixing || Config.UseAsio)
        {
          // In case of mixing use a Buffer of 500ms only, because the Mixer plays the complete bufer, before for example skipping
         Config.BufferingMs = 500;
        }
        else
        {
          Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, Config.BufferingMs);
        }

        InitializeControls();
        Config.LoadAudioDecoderPlugins();
        Config.LoadDSPPlugins();

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
    /// Init BASS, when a Audio file is to be played
    /// </summary>
    public void InitBass()
    {
      try
      {
        Log.Info("BASS: Initializing BASS audio engine...");
        bool initOK = false;
        if (Config.UseAsio)
        {
          Log.Info("BASS: Using ASIO device: {0}", Config.AsioDevice);
          BASS_ASIO_DEVICEINFO[] asioDevices = BassAsio.BASS_ASIO_GetDeviceInfos();
          // Check if the ASIO device read is amongst the one retrieved
          for (int i = 0; i < asioDevices.Length; i++)
          {
            if (asioDevices[i].name == Config.AsioDevice)
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
            initOK = (Bass.BASS_Init(0, 48000, 0, IntPtr.Zero) && BassAsio.BASS_ASIO_Init(_asioDeviceNumber));

            // When used in config the ASIO_INIT fails. Ignore it here, to be able using the visualisations
            if (Application.ExecutablePath.Contains("Configuration"))
            {
              initOK = true;
            }
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

          initOK =
            (Bass.BASS_Init(soundDevice, 44100, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_LATENCY, IntPtr.Zero));
        }
        if (initOK)
        {
          // Create an 8 Channel Mixer, which should be running until stopped.
          // The streams to play are added to the active screen
          if (Config.Mixing && _mixer == 0)
          {
            _mixer = BassMix.BASS_Mixer_StreamCreate(44100, 8,
                                                     BASSFlag.BASS_MIXER_NONSTOP | BASSFlag.BASS_STREAM_AUTOFREE);
          }
          else if (Config.UseAsio && _mixer == 0)
          {
            // For ASIO we neeed an Decoding Mixer with the number of Channels equals the ASIO Channels
            _mixer = BassMix.BASS_Mixer_StreamCreate(44100, 2,
                                                     BASSFlag.BASS_MIXER_NONSTOP | BASSFlag.BASS_STREAM_DECODE);
            // assign ASIO and assume the ASIO format, samplerate and number of channels from the BASS stream
            _asioHandler = new BassAsioHandler(0, 0, _mixer);
          }

          Log.Info("BASS: Initialization done.");
          _Initialized = true;
          _BassFreed = false;
        }
        else
        {
          BASSError error = Bass.BASS_ErrorGetCode();
          if (Config.UseAsio)
          {
            BASSError errorasio = BassAsio.BASS_ASIO_ErrorGetCode();
            Log.Error("BASS: Error initializing BASS audio engine {0} Asio: {1}",
                      Enum.GetName(typeof (BASSError), error), Enum.GetName(typeof (BASSError), errorasio));
          }
          else
            Log.Error("BASS: Error initializing BASS audio engine {0}", Enum.GetName(typeof (BASSError), error));
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
      if (Config.SoundDevice == "Default Sound Device")
      {
        Log.Info("BASS: Using default Sound Device");
        sounddevice = -1;
      }
      else
      {
        BASS_DEVICEINFO[] soundDeviceDescriptions = Bass.BASS_GetDeviceInfos();
        bool foundDevice = false;
        for (int i = 0; i < soundDeviceDescriptions.Length; i++)
        {
          if (soundDeviceDescriptions[i].name == Config.SoundDevice)
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
          Log.Info("BASS: Using Sound Device {0}", Config.SoundDevice);
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
      VizWindow.Visible = false;
      VizManager = new VisualizationManager(this, VizWindow);
      TargetFPS = VizFPS;

      if (VizManager != null)
      {
        if (VizPluginInfo != null)
        {
          this.CreateVisualization(VizPluginInfo);
        }
      }
    }

    /// <summary>
    /// Load Settings
    /// </summary>
    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        int vizType = xmlreader.GetValueAsInt("musicvisualization", "vizType", (int)VisualizationInfo.PluginType.None);
        string vizName = xmlreader.GetValueAsString("musicvisualization", "name", "");
        string vizPath = xmlreader.GetValueAsString("musicvisualization", "path", "");
        string vizClsid = xmlreader.GetValueAsString("musicvisualization", "clsid", "");
        int vizPreset = xmlreader.GetValueAsInt("musicvisualization", "preset", 0);

        VizPluginInfo = new VisualizationInfo((VisualizationInfo.PluginType)vizType, vizPath, vizName, vizClsid,
                                              vizPreset);

        VizFPS = xmlreader.GetValueAsInt("musicvisualization", "fps", 30);
      }
    }


    #endregion

    #region Clenaup / Free Resources

    /// <summary>
    /// Dispose the BASS Audio engine. Free all BASS and Visualisation related resources
    /// </summary>
    public void DisposeAndCleanUp()
    {
      // Clean up BASS Resources
      try
      {
        // Some Winamp dsps might raise an exception when closing
        BassWaDsp.BASS_WADSP_Free();
      }
      catch (Exception) {}
      if (Config.UseAsio)
      {
        BassAsio.BASS_ASIO_Stop();
        BassAsio.BASS_ASIO_Free();
      }
      if (_mixer != 0)
      {
        Bass.BASS_ChannelStop(_mixer);
      }

      if (_currentStream != null)
      {
        _currentStream.Dispose();
      }

      Bass.BASS_Stop();
      Bass.BASS_Free();

      foreach (int pluginHandle in DecoderPluginHandles)
      {
        Bass.BASS_PluginFree(pluginHandle);
      }

      VizManager.SafeDispose();
      VizWindow.SafeDispose();

      GUIGraphicsContext.OnNewAction -= new OnActionHandler(OnNewAction);
    }

    /// <summary>
    /// Free BASS, when not playing Audio content, as it might cause S/PDIF output stop working
    /// </summary>
    public void FreeBass()
    {
      // Remove the Vis Window, as it might interfere with the overlay of other plugins
      RemoveVisualizationWindow();

      if (!_BassFreed)
      {
        Log.Info("BASS: Freeing BASS. Non-audio media playback requested.");
        if (Config.UseAsio)
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
    /// Release the Video Window
    /// </summary>
    public override void Dispose()
    {
      if (VizWindow != null)
      {
        VizWindow.Visible = false;
      }

      if (!Stopped) // Check if stopped already to avoid that Stop() is called two or three times
      {
        Stop();
      }
    }

    #endregion

    #region Visualisation Related

    /// <summary>
    /// Setup the Visualisation Window and add it to the Main Form control collection
    /// </summary>
    private void SetVisualizationWindow()
    {
      if (GUIGraphicsContext.form.InvokeRequired)
      {
        InitializeControlsDelegate d = new InitializeControlsDelegate(SetVisualizationWindow);
        GUIGraphicsContext.form.Invoke(d);
        return;
      }

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
        VizWindow.Location = new Point(8, 16);
        VizWindow.Name = "NativeVisualizationWindow";
        VizWindow.Size = new Size(0, 0);
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
      Thread createVizThread;
      createVizThread = new Thread(new ParameterizedThreadStart(InternalCreateVisualization));
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
      if (vizPluginInfo == null || vizPluginInfo.VisualizationType == VisualizationInfo.PluginType.None ||
          VizWindow == null)
      {
        return false;
      }

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
    /// In all other cases the current active stream is used.
    /// </summary>
    /// <returns></returns>
    internal int GetCurrentVizStream()
    {
      // In case od ASIO return the clone of the stream, because for a decoding channel, we can't get data from the original stream
      if (Config.UseAsio)
      {
        return _streamcopy.Stream;
      }

      if (Config.Mixing)
      {
        return _mixer;
      }
      else
      {
        return _currentStream.BassStream;
      }
    }

    /// <summary>
    /// Show the Visualisation Window
    /// </summary>
    /// <param name="visible"></param>
    private void ShowVisualizationWindow(bool visible)
    {
      if (VizWindow == null)
      {
        return;
      }

      if (VizWindow.InvokeRequired)
      {
        ShowVisualizationWindowDelegate d = new ShowVisualizationWindowDelegate(ShowVisualizationWindow);
        VizWindow.Invoke(d, new object[] {visible});
      }

      else
      {
        VizWindow.Visible = visible;
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Sets the End Position for the CUE Track
    /// </summary>
    /// <param name="stream"></param>
    private void setCueTrackEndPosition(MusicStream stream)
    {
      if (currentCueSheet != null)
      {
        if (cueTrackEndEventHandler != 0)
        {
          Bass.BASS_ChannelRemoveSync(stream.BassStream, cueTrackEndEventHandler);
        }

        Bass.BASS_ChannelSetPosition(stream.BassStream, Bass.BASS_ChannelSeconds2Bytes(stream.BassStream, cueTrackStartPos));
        if (cueTrackEndPos > cueTrackStartPos)
        {
          //cueTrackEndEventHandler = RegisterCueTrackEndEvent(stream, CurrentStreamIndex,
          //                                                   Bass.BASS_ChannelSeconds2Bytes(stream, cueTrackEndPos));
        }
      }
    }

    /// <summary>
    /// Is stream Playing?
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private bool StreamIsPlaying(MusicStream stream)
    {
      return stream != null && (Bass.BASS_ChannelIsActive(stream.BassStream) == BASSActive.BASS_ACTIVE_PLAYING);
    }

    /// <summary>
    /// Get Total Seconds of the Stream
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private double GetTotalStreamSeconds(MusicStream stream)
    {
      if (stream == null)
      {
        return 0;
      }

      // length in bytes
      long len = Bass.BASS_ChannelGetLength(stream.BassStream);

      // the total time length
      double totaltime = Bass.BASS_ChannelBytes2Seconds(stream.BassStream, len);
      return totaltime;
    }

    /// <summary>
    /// Retrieve the elapsed time
    /// </summary>
    /// <returns></returns>
    private double GetStreamElapsedTime()
    {
      return GetStreamElapsedTime(_currentStream);
    }

    /// <summary>
    /// Retrieve the elapsed time
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private double GetStreamElapsedTime(MusicStream stream)
    {
      if (stream.BassStream == 0)
      {
        return 0;
      }

      // position in bytes
      long pos = Bass.BASS_ChannelGetPosition(stream.BassStream);

      // the elapsed time length
      double elapsedtime = Bass.BASS_ChannelBytes2Seconds(stream.BassStream, pos);
      return elapsedtime;
    }

    #endregion

    #region IPlayer Implementation

    /// <summary>
    /// Starts Playback of the given file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public override bool Play(string filePath)
    {
      if (!_Initialized)
      {
        return false;
      }

      MusicStream stream = _oldStream;

      bool doFade = false;
      bool result = true;
      Speed = 1; // Set playback Speed to normal speed

      try
      {
        if (stream != null && filePath.ToLower().CompareTo(stream.FilePath.ToLower()) == 0)
        {
          // Selected file is equal to current stream
          if (_State == PlayState.Paused)
          {
            // Resume paused stream
            if (Config.SoftStop)
            {
              Bass.BASS_ChannelSlideAttribute(stream.BassStream, BASSAttribute.BASS_ATTRIB_VOL, 1, 500);
            }
            else
            {
              Bass.BASS_ChannelSetAttribute(stream.BassStream, BASSAttribute.BASS_ATTRIB_VOL, 1);
            }

            result = Bass.BASS_Start();

            if (Config.UseAsio)
            {
              result = BassAsio.BASS_ASIO_Start(0);
            }

            if (result)
            {
              _State = PlayState.Playing;

              if (PlaybackStateChanged != null)
              {
                PlaybackStateChanged(this, PlayState.Paused, _State);
              }
            }

            return result;
          }
        }
        else
        {
          // Cue support
          cueTrackStartPos = 0;
          cueTrackEndPos = 0;
          if (CueUtil.isCueFakeTrackFile(filePath))
          {
            Log.Debug("BASS: Playing CUE Track: {0}", filePath);
            currentCueFakeTrackFileName = filePath;
            CueFakeTrack cueFakeTrack = CueUtil.parseCueFakeTrackFileName(filePath);
            if (!cueFakeTrack.CueFileName.Equals(currentCueFileName))
            {
              // New CUE. Update chached cue.
              currentCueSheet = new CueSheet(cueFakeTrack.CueFileName);
              currentCueFileName = cueFakeTrack.CueFileName;
            }

            // Get track start position
            Track track = currentCueSheet.Tracks[cueFakeTrack.TrackNumber - currentCueSheet.Tracks[0].TrackNumber];
            Index index = track.Indices[0];
            cueTrackStartPos = CueUtil.cueIndexToFloatTime(index);

            // If single audio file and is not last track, set track end position.
            if (currentCueSheet.Tracks[currentCueSheet.Tracks.Length - 1].TrackNumber > track.TrackNumber)
            {
              Track nextTrack =
                currentCueSheet.Tracks[cueFakeTrack.TrackNumber - currentCueSheet.Tracks[0].TrackNumber + 1];
              if (nextTrack.DataFile.Filename.Equals(track.DataFile.Filename))
              {
                Index nindex = nextTrack.Indices[0];
                cueTrackEndPos = CueUtil.cueIndexToFloatTime(nindex);
              }
            }

            // If audio file is not changed, just set new start/end position and reset pause
            string audioFilePath = System.IO.Path.GetDirectoryName(cueFakeTrack.CueFileName) +
                                   System.IO.Path.DirectorySeparatorChar + track.DataFile.Filename;
            if (audioFilePath.CompareTo(FilePath) == 0 /* && StreamIsPlaying(stream)*/)
            {
              setCueTrackEndPosition(stream);
              return true;
            }
            filePath = audioFilePath;
          }
          else
          {
            currentCueFileName = null;
            currentCueSheet = null;
          }
        }

        if (stream != null && stream.IsPlaying)
        {
          if (!stream.IsCrossFading)
          {
            double oldStreamDuration = GetTotalStreamSeconds(_oldStream);
            double oldStreamElapsedSeconds = GetStreamElapsedTime(_oldStream);
            double crossFadeSeconds = (double)Config.CrossFadeIntervalMs;

            if (crossFadeSeconds > 0)
              crossFadeSeconds = crossFadeSeconds / 1000.0;

            if ((oldStreamDuration - (oldStreamElapsedSeconds + crossFadeSeconds) > -1))
            {
              FadeOutStop(_oldStream);
            }
            else
            {
              Bass.BASS_ChannelStop(_oldStream.BassStream);
            }
          }
        }

        _State = PlayState.Init;

        if (filePath == string.Empty)
        {
          return result;
        }

        _currentStream = new MusicStream(filePath);
        stream = _currentStream;
        
        // Enable events, for various Playback Actions to be handled
        stream.MusicStreamMessage += new MusicStream.MusicStreamMessageHandler(OnMusicStreamMessage);

        // Is Mixing / ASIO enabled, then we create a mixer channel and assign the stream to the mixer
        if ((Config.Mixing || Config.UseAsio) && stream.BassStream != 0)
        {
          // Do an upmix of the stereo according to the matrix.
          // Now Plugin the stream to the mixer and set the mixing matrix
          BassMix.BASS_Mixer_StreamAddChannel(_mixer, stream.BassStream,
                                              BASSFlag.BASS_MIXER_MATRIX | BASSFlag.BASS_STREAM_AUTOFREE |
                                              BASSFlag.BASS_MIXER_NORAMPIN | BASSFlag.BASS_MIXER_BUFFER);
          BassMix.BASS_Mixer_ChannelSetMatrix(stream.BassStream, _MixingMatrix);
        }



        if (filePath != string.Empty)
        {
          


          /*
          
          



          if (stream != 0)
          {
            // When we have a MIDI file, we need to assign the sound banks to the stream
            if (IsMidiFile(filePath) && soundFonts != null)
            {
              BassMidi.BASS_MIDI_StreamSetFonts(stream, soundFonts, soundFonts.Length);
            }

            StreamEventSyncHandles[CurrentStreamIndex] = RegisterPlaybackEvents(stream, CurrentStreamIndex);

            if (doFade && Config.CrossFadeIntervalMs > 0)
            {
              _CrossFading = true;
              // Reduce the stream volume to zero so we can fade it in...
              Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 0);

              // Fade in from 0 to 1 over the Config.CrossFadeIntervalMs duration
              Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 1, Config.CrossFadeIntervalMs);
            }


          }
          else
          {
            Log.Error("BASS: Unable to create Stream for {0}.  Reason: {1}.", filePath,
                      Enum.GetName(typeof (BASSError), Bass.BASS_ErrorGetCode()));
          }
          */

          bool playbackStarted = false;
          if (Config.Mixing)
          {
            if (Bass.BASS_ChannelIsActive(_mixer) == BASSActive.BASS_ACTIVE_PLAYING)
            {
              setCueTrackEndPosition(stream);
              playbackStarted = true;
            }
            else
            {
              playbackStarted = Bass.BASS_ChannelPlay(_mixer, false);
              setCueTrackEndPosition(stream);
            }
          }
          else if (Config.UseAsio)
          {
            // In order to provide data for visualisation we need to clone the stream
            _streamcopy = new StreamCopy();
            _streamcopy.ChannelHandle = stream.BassStream;
            _streamcopy.StreamFlags = BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT;
            // decode the channel, so that we have a Streamcopy

            _asioHandler.Pan = Config.AsioBalance;
            _asioHandler.Volume = (float)Config.StreamVolume / 100f;

            // Set the Sample Rate from the stream
            _asioHandler.SampleRate = (double)stream.ChannelInfo.freq;
            // try to set the device rate too (saves resampling)
            BassAsio.BASS_ASIO_SetRate((double)stream.ChannelInfo.freq);

            try
            {
              _streamcopy.Start(); // start the cloned stream
            }
            catch (Exception)
            {
              Log.Error("Captured an error on StreamCopy start");
            }

            if (BassAsio.BASS_ASIO_IsStarted())
            {
              setCueTrackEndPosition(stream);
              playbackStarted = true;
            }
            else
            {
              BassAsio.BASS_ASIO_Stop();
              playbackStarted = BassAsio.BASS_ASIO_Start(0);
              setCueTrackEndPosition(stream);
            }
          }
          else
          {
            setCueTrackEndPosition(stream);
            playbackStarted = Bass.BASS_ChannelPlay(stream.BassStream, false);
          }

          if (stream.BassStream != 0 && playbackStarted)
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
            {
              PlaybackStateChanged(this, oldState, _State);
            }

            if (PlaybackStart != null)
            {
              PlaybackStart(this, GetTotalStreamSeconds(stream));
            }
          }

          else
          {
            Log.Error("BASS: Unable to play {0}.  Reason: {1}.", filePath,
                      Enum.GetName(typeof (BASSError), Bass.BASS_ErrorGetCode()));

            stream.Dispose();
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
    /// Pause Playback
    /// </summary>
    public override void Pause()
    {
      _CrossFading = false;
      MusicStream stream = _currentStream;

      Log.Debug("BASS: Pause of stream {0}", stream.FilePath);
      try
      {
        PlayState oldPlayState = _State;

        if (oldPlayState == PlayState.Ended || oldPlayState == PlayState.Init)
        {
          return;
        }

        if (oldPlayState == PlayState.Paused)
        {
          _State = PlayState.Playing;

          if (Config.SoftStop)
          {
            // Fade-in over 500ms
            Bass.BASS_ChannelSlideAttribute(stream.BassStream, BASSAttribute.BASS_ATTRIB_VOL, 1, 500);
            Bass.BASS_Start();
          }

          else
          {
            Bass.BASS_ChannelSetAttribute(stream.BassStream, BASSAttribute.BASS_ATTRIB_VOL, 1);
            Bass.BASS_Start();
          }

          if (Config.UseAsio)
          {
            BassAsio.BASS_ASIO_Start(0);
          }
        }

        else
        {
          _State = PlayState.Paused;

          if (Config.SoftStop)
          {
            // Fade-out over 500ms
            Bass.BASS_ChannelSlideAttribute(stream.BassStream, BASSAttribute.BASS_ATTRIB_VOL, 0, 500);

            // Wait until the slide is done
            while (Bass.BASS_ChannelIsSliding(stream.BassStream, BASSAttribute.BASS_ATTRIB_VOL))
              System.Threading.Thread.Sleep(20);

            Bass.BASS_Pause();
          }

          else
          {
            Bass.BASS_Pause();
          }

          if (Config.UseAsio)
          {
            BassAsio.BASS_ASIO_Stop();
          }
        }

        if (oldPlayState != _State)
        {
          if (PlaybackStateChanged != null)
          {
            PlaybackStateChanged(this, oldPlayState, _State);
          }
        }
      }

      catch {}
    }


    /// <summary>
    /// Stopping Playback
    /// </summary>
    public override void Stop()
    {
      //TODO: Mantis 3477
      //Soft stop causing issues with TV when starting TV when music is playing
      //Disabled soft stop as workaround for 1.2 beta

      //int stream = GetCurrentStream();
      //if (Config.SoftStop && !_isLastFMRadio && !_isRadio)
      //{
      //  RegisterStreamSlideEndEvent(stream);
      //  Log.Info("BASS: Stopping song. Fading out.");
      //  Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, -1, _CrossFadeIntervalMS);
      //}
      //else
      //{
        StopInternal();
//      }
    }

    /// <summary>
    /// Internal Stop called by the above method
    /// Needed to handle the sliding for a fade out correctly
    /// </summary>
    private void StopInternal()
    {
      _CrossFading = false;

      MusicStream stream = _currentStream;
      Log.Debug("BASS: Stop of stream {0}. File: {1}", stream, CurrentFile);
      try
      {
        // Unregister all SYNCs and free the channel manually.
        // Otherwise, the HandleSongEnded would be called twice
        //UnregisterPlaybackEvents(stream, StreamEventSyncHandles[CurrentStreamIndex]);

        if (Config.Mixing || Config.UseAsio)
        {
          Bass.BASS_ChannelStop(stream.BassStream);
          BassMix.BASS_Mixer_ChannelRemove(stream.BassStream);
        }
        else
        {
          Bass.BASS_ChannelStop(stream.BassStream);
        }


        if (Config.UseAsio)
        {
          BassAsio.BASS_ASIO_Stop();
        }

        // Free Winamp resources
        try
        {
          // Some Winamp dsps might raise an exception when closing
          //foreach (int waDspPlugin in _waDspPlugins.Values)
          //{
          //  BassWaDsp.BASS_WADSP_Stop(waDspPlugin);
          //}
        }
        catch (Exception) {}

        // If we did a playback of a Audio CD, release the CD, as we might have problems with other CD related functions
        if (_isCDDAFile)
        {
          int driveCount = BassCd.BASS_CD_GetDriveCount();
          for (int i = 0; i < driveCount; i++)
          {
            BassCd.BASS_CD_Release(i);
          }
        }

        stream.Dispose();

        if (PlaybackStop != null)
        {
          PlaybackStop(this);
        }

        HandleSongEnded(true);

        // Remove the Viz Window from the Main Form as it causes troubles to other plugin overlay window
        RemoveVisualizationWindow();

        // Switching back to normal playback mode
        SwitchToDefaultPlaybackMode();
      }

      catch (Exception ex)
      {
        Log.Error("BASS: Stop command caused an exception - {0}", ex.Message);
      }

      NotifyPlaying = false;
    }

    /// <summary>
    /// Handle Stop of a song
    /// </summary>
    /// <param name="bManualStop"></param>
    private void HandleSongEnded(bool bManualStop)
    {
      Log.Debug("BASS: HandleSongEnded - manualStop: {0}, CrossFading: {1}", bManualStop, _CrossFading);
      PlayState oldState = _State;

      if (!Util.Utils.IsAudio(FilePath))
      {
        GUIGraphicsContext.IsFullScreenVideo = false;
      }

      if (bManualStop)
      {
        ShowVisualizationWindow(false);
        VizWindow.Run = false;
      }

      GUIGraphicsContext.IsPlaying = false;

      if (!bManualStop)
      {
        if (_CrossFading)
        {
          _State = PlayState.Playing;
        }
        else
        {
          FilePath = "";
          _State = PlayState.Ended;
        }
      }

      else
      {
        _State = PlayState.Init;
      }

      if (oldState != _State && PlaybackStateChanged != null)
      {
        PlaybackStateChanged(this, oldState, _State);
      }

      _CrossFading = false; // Set crossfading to false, Play() will update it when the next song starts
    }

    /// <summary>
    /// Fade out Song
    /// </summary>
    /// <param name="stream"></param>
    private void FadeOutStop(MusicStream stream)
    {
      Log.Debug("BASS: FadeOutStop of stream {0}", stream.FilePath);

      if (!StreamIsPlaying(stream))
      {
        return;
      }

      int level = Bass.BASS_ChannelGetLevel(stream.BassStream);
      Bass.BASS_ChannelSlideAttribute(stream.BassStream, BASSAttribute.BASS_ATTRIB_VOL, -1, Config.CrossFadeIntervalMs);
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
      if (_speed == 1) // not to exhaust log when ff
        Log.Debug("BASS: SeekForward for {0} ms", Convert.ToString(ms));
      _CrossFading = false;

      if (State != PlayState.Playing)
      {
        return false;
      }

      if (ms <= 0)
      {
        return false;
      }

      bool result = false;

      try
      {
        MusicStream stream = _currentStream;
        long len = Bass.BASS_ChannelGetLength(stream.BassStream); // length in bytes
        double totaltime = Bass.BASS_ChannelBytes2Seconds(stream.BassStream, len); // the total time length

        long pos = 0; // position in bytes
        if (Config.Mixing)
        {
          pos = BassMix.BASS_Mixer_ChannelGetPosition(stream.BassStream);
        }
        else
        {
          pos = Bass.BASS_ChannelGetPosition(stream.BassStream);
        }

        double timePos = Bass.BASS_ChannelBytes2Seconds(stream.BassStream, pos);
        double offsetSecs = (double)ms / 1000.0;

        if (timePos + offsetSecs >= totaltime)
        {
          return false;
        }

        if (Config.Mixing)
        {
          BassMix.BASS_Mixer_ChannelSetPosition(stream.BassStream, Bass.BASS_ChannelSeconds2Bytes(stream.BassStream, timePos + offsetSecs));
          // the elapsed time length
        }
        else
          Bass.BASS_ChannelSetPosition(stream.BassStream, timePos + offsetSecs); // the elapsed time length
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
      if (_speed == 1) // not to exhaust log
        Log.Debug("BASS: SeekReverse for {0} ms", Convert.ToString(ms));
      _CrossFading = false;

      if (State != PlayState.Playing)
      {
        return false;
      }

      if (ms <= 0)
      {
        return false;
      }

      MusicStream stream = _currentStream;
      bool result = false;

      try
      {
        long len = Bass.BASS_ChannelGetLength(stream.BassStream); // length in bytes

        long pos = 0; // position in bytes
        if (Config.Mixing)
        {
          pos = BassMix.BASS_Mixer_ChannelGetPosition(stream.BassStream);
        }
        else
        {
          pos = Bass.BASS_ChannelGetPosition(stream.BassStream);
        }

        double timePos = Bass.BASS_ChannelBytes2Seconds(stream.BassStream, pos);
        double offsetSecs = (double)ms / 1000.0;

        if (timePos - offsetSecs <= 0)
        {
          return false;
        }

        if (Config.Mixing)
        {
          BassMix.BASS_Mixer_ChannelSetPosition(stream.BassStream, Bass.BASS_ChannelSeconds2Bytes(stream.BassStream, timePos - offsetSecs));
          // the elapsed time length
        }
        else
          Bass.BASS_ChannelSetPosition(stream.BassStream, timePos - offsetSecs); // the elapsed time length
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
        MusicStream stream = _currentStream;

        if (StreamIsPlaying(stream))
        {
          if (currentCueSheet != null)
          {
            position += (int)cueTrackStartPos;
          }
          if (Config.Mixing)
          {
            BassMix.BASS_Mixer_ChannelSetPosition(stream.BassStream, Bass.BASS_ChannelSeconds2Bytes(stream.BassStream, position));
          }
          else
          {
            Bass.BASS_ChannelSetPosition(stream.BassStream, (float)position);
          }
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
        {
          dTime = 0.0d;
        }

        if (dTime < Duration)
        {
          SeekToTimePosition((int)dTime);
        }
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
        {
          dTime = 0.0d;
        }

        if (dTime < Duration)
        {
          SeekToTimePosition((int)dTime);
        }
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
        {
          dPositionMS = 0d;
        }

        if (dPositionMS > dDuration)
        {
          dPositionMS = dDuration;
        }

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
        {
          iPercentage = 0;
        }

        if (iPercentage >= 100)
        {
          iPercentage = 100;
        }

        if (iPercentage == 0)
        {
          SeekToTimePosition(0);
        }

        else
        {
          SeekToTimePosition((int)(Duration * ((double)iPercentage / 100d)));
        }
      }
    }

    /// <summary>
    /// Process Method
    /// </summary>
    public override void Process()
    {
      if (!Playing)
      {
        return;
      }

      TimeSpan ts = DateTime.Now - _seekUpdate;
      if (_speed > 1 && ts.TotalMilliseconds > 120)
      {
        SeekForward(80 * _speed);
        _seekUpdate = DateTime.Now;
      }
      else if (_speed < 0 && ts.TotalMilliseconds > 120)
      {
        SeekReverse(80 * -_speed);
        _seekUpdate = DateTime.Now;
      }

      if (VizManager == null)
      {
        return;
      }

      if (GUIGraphicsContext.BlankScreen)
        //BAV || (GUIGraphicsContext.Overlay == false && GUIGraphicsContext.IsFullScreenVideo == false))
      {
        //BAV if (GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW)
        {
          if (VizWindow.Visible)
          {
            VizWindow.Visible = false;
          }
        }
      }

      else if (!VizWindow.Visible && !GUIWindowManager.IsRouted &&
               VizPluginInfo.VisualizationType != VisualizationInfo.PluginType.None)
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
      {
        SetVideoWindow();
      }
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
      {
        return;
      }
      NeedUpdate = false;

      if (_IsFullScreen)
      {
        Log.Debug("BASS: Fullscreen");

        _VideoPositionX = GUIGraphicsContext.OverScanLeft + GUIGraphicsContext.OffsetX;
        _VideoPositionY = GUIGraphicsContext.OverScanTop + GUIGraphicsContext.OffsetY;

        _VideoWidth =
          (int)Math.Round((float)GUIGraphicsContext.OverScanWidth * (float)GUIGraphicsContext.ZoomHorizontal);
        _VideoHeight =
          (int)Math.Round((float)GUIGraphicsContext.OverScanHeight * (float)GUIGraphicsContext.ZoomVertical);

        VizWindow.Location = new Point(_VideoPositionX, _VideoPositionY);
        //VizWindow.Visible = false;

        _videoRectangle = new Rectangle(_VideoPositionX, _VideoPositionY, _VideoWidth, _VideoHeight);
        _sourceRectangle = _videoRectangle;

        VizWindow.Size = new Size(_VideoWidth, _VideoHeight);
        VizWindow.Visible = true;
        return;
      }
      else
      {
        VizWindow.Size = new Size(_VideoWidth, _VideoHeight);

        VizWindow.Location = new Point(_VideoPositionX, _VideoPositionY);
        _videoRectangle = new Rectangle(_VideoPositionX, _VideoPositionY, VizWindow.Size.Width, VizWindow.Size.Height);
        _sourceRectangle = _videoRectangle;
      }

      if (!GUIWindowManager.IsRouted && VizPluginInfo.VisualizationType != VisualizationInfo.PluginType.None)
      {
        VizWindow.Visible = _State == PlayState.Playing;
      }
      else
      {
        VizWindow.Visible = false;
      }
    }

    #endregion

    #region  Public Methods

    /// <summary>
    /// Returns the Tags of an AV Stream
    /// </summary>
    /// <returns></returns>
    public MusicTag GetStreamTags()
    {
      MusicTag tag = new MusicTag();
      if (_tagInfo == null)
      {
        return tag;
      }

      // So let's filter it out ourself
      string title = _tagInfo.title;
      int streamUrlIndex = title.IndexOf("';StreamUrl=");
      if (streamUrlIndex > -1)
      {
        title = _tagInfo.title.Substring(0, streamUrlIndex);
      }

      tag.Album = _tagInfo.album;
      tag.Artist = _tagInfo.artist;
      tag.Title = title;
      tag.Genre = _tagInfo.genre;
      try
      {
        tag.Year = Convert.ToInt32(_tagInfo.year);
      }
      catch (FormatException)
      {
        tag.Year = 0;
      }
      return tag;
    }

    /// <summary>
    /// Switches the Playback to Gapless
    /// Used, if playback of a complete Album is started
    /// </summary>
    public void SwitchToGaplessPlaybackMode()
    {
      if (_playBackType == (int)Config.PlayBackType.CROSSFADE)
      {
        // Store the current settings, so that when the album playback is completed, we can switch back to the default
        if (_savedPlayBackType == -1)
        {
          _savedPlayBackType = _playBackType;
        }

        Log.Info("BASS: Playback of complete Album starting. Switching playbacktype from {0} to {1}",
                 Enum.GetName(typeof (Config.PlayBackType), _playBackType),
                 Enum.GetName(typeof (Config.PlayBackType), (int)Config.PlayBackType.GAPLESS));

        _playBackType = (int)Config.PlayBackType.GAPLESS;
        Config.CrossFadeIntervalMs = 200;
      }
    }

    /// <summary>
    /// Switch back to the default Playback Mode, whoch was saved before starting playback of a complete album
    /// </summary>
    public void SwitchToDefaultPlaybackMode()
    {
      if (_savedPlayBackType > -1)
      {
        Log.Info("BASS: Playback of complete Album stopped. Switching playbacktype from {0} to {1}",
                 Enum.GetName(typeof (Config.PlayBackType), _playBackType),
                 Enum.GetName(typeof (Config.PlayBackType), _savedPlayBackType));

        if (_savedPlayBackType == 0)
        {
          Config.CrossFadeIntervalMs = 100;
        }
        else if (_savedPlayBackType == 1)
        {
          Config.CrossFadeIntervalMs = 200;
        }
        else
        {
          Config.CrossFadeIntervalMs = _DefaultCrossFadeIntervalMS == 0 ? 4000 : _DefaultCrossFadeIntervalMS;
        }

        _playBackType = _savedPlayBackType;
        _savedPlayBackType = -1;
      }
    }

    /// <summary>
    /// Return the dbLevel to be used by a VUMeter
    /// </summary>
    /// <param name="dbLevelL"></param>
    /// <param name="dbLevelR"></param>
    public void RMS(out double dbLevelL, out double dbLevelR)
    {
      int peakL = 0;
      int peakR = 0;
      double dbLeft = 0.0;
      double dbRight = 0.0;

      // Find out with which stream to deal with
      int level = 0;

      MusicStream stream = _currentStream;
      if (Config.Mixing)
      {
        level = BassMix.BASS_Mixer_ChannelGetLevel(stream.BassStream);
      }
      else if (Config.UseAsio)
      {
        float fpeakL = BassAsio.BASS_ASIO_ChannelGetLevel(false, 0);
        float fpeakR = (int)BassAsio.BASS_ASIO_ChannelGetLevel(false, 1);
        dbLeft = 20.0 * Math.Log10(fpeakL);
        dbRight = 20.0 * Math.Log10(fpeakR);
      }
      else
      {
        level = Bass.BASS_ChannelGetLevel(stream.BassStream);
      }

      if (!Config.UseAsio) // For Asio, we already got the peaklevel above
      {
        peakL = Un4seen.Bass.Utils.LowWord32(level); // the left level
        peakR = Un4seen.Bass.Utils.HighWord32(level); // the right level

        dbLeft = Un4seen.Bass.Utils.LevelToDB(peakL, 65535);
        dbRight = Un4seen.Bass.Utils.LevelToDB(peakR, 65535);
      }

      dbLevelL = dbLeft;
      dbLevelR = dbRight;
    }

    #endregion
  }
}