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
using System.Threading;
using System.Windows.Forms;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Player.DSP;
using MediaPortal.TagReader;
using MediaPortal.Visualization;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.AddOn.WaDsp;
using Un4seen.Bass.Misc;
using Un4seen.BassAsio;
using Un4seen.BassWasapi;
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

    public delegate void PlaybackStartHandler(g_Player.MediaType type, string filename);

    public static event PlaybackStartHandler PlaybackStart;

    public delegate void PlaybackStopHandler(object sender);

    public event PlaybackStopHandler PlaybackStop;

    public delegate void PlaybackProgressHandler(object sender, double duration, double curPosition);

    public delegate void PlaybackStateChangedDelegate(object sender, PlayState oldState, PlayState newState);

    public event PlaybackStateChangedDelegate PlaybackStateChanged;

    public delegate void InternetStreamSongChangedDelegate(object sender);

    public event InternetStreamSongChangedDelegate InternetStreamSongChanged;

    private delegate void InitializeControlsDelegate();

    private delegate void ShowVisualizationWindowDelegate(bool visible);

    #endregion

    #region Variables

    private MixerStream _mixer = null;
    private List<MusicStream> _streams = new List<MusicStream>();
    private int _deviceNumber = -1;

    private BASS_WASAPI_DEVICEINFO _wasapiDeviceInfo = null;

    private int _deviceOutputChannels = 2;

    private List<int> DecoderPluginHandles = new List<int>();

    private PlayState _state = PlayState.Init;
    private string _filePath = string.Empty;
    private VisualizationInfo VizPluginInfo = null;
    private int VizFPS = 20;

    private int _DefaultCrossFadeIntervalMS = 4000;
    private bool _initialized = false;
    private bool _bassFreed = false;
    private VisualizationWindow VizWindow = null;
    private VisualizationManager VizManager = null;
    private int _playBackType;
    private int _savedPlayBackType = -1;

    private bool _IsFullScreen = false;
    private int _VideoPositionX = 10;
    private int _VideoPositionY = 10;
    private int _VideoWidth = 100;
    private int _VideoHeight = 100;

    private bool NeedUpdate = true;
    private bool NotifyPlaying = true;

    private bool _isCDDAFile = false;
    private int _speed = 1;
    private DateTime _seekUpdate = DateTime.Now;

    private StreamCopy _streamcopy; // To Clone the Strem for Visualisation

    // CUE Support
    private string _currentCueFileName = null;
    private string _currentCueFakeTrackFileName = null;
    private CueSheet _currentCueSheet = null;
    private float _cueTrackStartPos = 0;
    private float _cueTrackEndPos = 0;

    private TAG_INFO _tagInfo;

    #endregion

    #region Properties

    /// <summary>
    /// Returns, if the player is in initialising stage
    /// </summary>
    public override bool Initializing
    {
      get { return (_state == PlayState.Init); }
    }

    /// <summary>
    /// Returns the Duration of an Audio Stream
    /// </summary>
    public override double Duration
    {
      get
      {
        MusicStream stream = GetCurrentStream();

        if (stream == null)
        {
          return 0;
        }

        double duration = stream.TotalStreamSeconds;
        if (_currentCueSheet != null)
        {
          if (_cueTrackEndPos > 0)
          {
            duration = _cueTrackEndPos;
          }
          duration -= _cueTrackStartPos;
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
        MusicStream stream = GetCurrentStream();

        if (stream == null)
        {
          return 0;
        }

        long pos = Bass.BASS_ChannelGetPosition(stream.BassStream); // position in bytes

        // In case of last.fm subtract the starting time
        //if (_isLastFMRadio)
        //  pos -= _lastFMSongStartPosition;

        double curPosition = (double)Bass.BASS_ChannelBytes2Seconds(stream.BassStream, pos); // the elapsed time length
        if (_currentCueSheet != null && _cueTrackStartPos > 0)
        {
          curPosition -= _cueTrackStartPos;
        }
        return curPosition;
      }
    }

    /// <summary>
    /// Returns the Devicenumber used
    /// </summary>
    public int DeviceNumber
    {
      get { return _deviceNumber; }
    }

    /// <summary>
    /// Returns the devices number of output channels
    /// </summary>
    public int DeviceChannels
    {
      get { return _deviceOutputChannels; }
    }

    /// <summary>
    /// Returns the Current Play State
    /// </summary>
    public PlayState State
    {
      get { return _state; }
    }

    /// <summary>
    /// Has the Playback Ended?
    /// </summary>
    public override bool Ended
    {
      get { return _state == PlayState.Ended; }
    }

    /// <summary>
    /// Is Playback Paused?
    /// </summary>
    public override bool Paused
    {
      get { return (_state == PlayState.Paused); }
    }

    /// <summary>
    /// Is the Player Playing?
    /// </summary>
    public override bool Playing
    {
      get { return (_state == PlayState.Playing || _state == PlayState.Paused); }
    }

    /// <summary>
    /// Is Player Stopped?
    /// </summary>
    public override bool Stopped
    {
      get { return (_state == PlayState.Init); }
    }

    /// <summary>
    /// Returns the File, currently played
    /// </summary>
    public override string CurrentFile
    {
      get { return (_currentCueSheet != null ? _currentCueFakeTrackFileName : _filePath); }
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
      get
      {
        MusicStream stream = GetCurrentStream();

        if (stream == null)
        {
          return false;
        }

        if (Util.Utils.IsLastFMStream(stream.FilePath))
        {
          return false;
        }

        return stream.Filetype.FileMainType == FileMainType.WebStream;
      }
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
      get { return _initialized; }
    }

    /// <summary>
    /// Is Crossfading enabled
    /// </summary>
    public bool CrossFadingEnabled
    {
      get { return true; }  // We always indicate that we're crossfading. We don't want g_player to stop playback
    }

    /// <summary>
    /// Is BASS freed?
    /// </summary>
    public bool BassFreed
    {
      get { return _bassFreed; }
    }

    /// <summary>
    /// Returns the Stream, currently played
    /// </summary>
    public override int CurrentAudioStream
    {
      get { return _mixer.BassStream; }
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
              if (Config.UseSkipSteps)
              {
                g_Player.SeekStep(true);
                string strStatus = g_Player.GetStepDescription();
                Log.Info("BASS: Skipping in stream for {0}", strStatus);
              }
              else
              {
                Log.Info("BASS: Playback speed {0}", g_Player.Speed);
              }
            }
            break;
          }

        case Action.ActionType.ACTION_REWIND:
        case Action.ActionType.ACTION_MUSIC_REWIND:
          {
            if (g_Player.IsMusic)
            {
              if (Config.UseSkipSteps)
              {
                g_Player.SeekStep(false);
                string strStatus = g_Player.GetStepDescription();
                Log.Info("BASS: Skipping in stream for {0}", strStatus);
              }
              else
              {
                Log.Info("BASS: Playback speed {0}", g_Player.Speed);
              }
            }
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
              case (int)PlayBackType.NORMAL:
                Config.CrossFadeIntervalMs = 100;
                type = "Normal";
                break;

              case (int)PlayBackType.GAPLESS:
                Config.CrossFadeIntervalMs = 0;
                type = "Gapless";
                break;

              case (int)PlayBackType.CROSSFADE:
                Config.CrossFadeIntervalMs = _DefaultCrossFadeIntervalMS == 0 ? 4000 : _DefaultCrossFadeIntervalMS;
                type = "Crossfading";
                break;
            }
            Log.Info("BASS: Playback changed to {0}", type);
            break;
          }

        case Action.ActionType.ACTION_PAGE_UP:
          {
            if (FullScreen && CurrentAudioStream != 0)
            {
              Log.Debug("BASS: Switch to Previous Vis");
              VizManager.GetPrevVis();
            }
            break;
          }

        case Action.ActionType.ACTION_PAGE_DOWN:
          {
            if (FullScreen && CurrentAudioStream != 0)
            {
              Log.Info("BASS: Switch to Next Vis");
              VizManager.GetNextVis();
            }
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

    /// <summary>
    /// This Message is sent from a MusicStream
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="action"></param>
    public void OnMusicStreamMessage(object sender, MusicStream.StreamAction action)
    {
      if (sender == null)
      {
        return;
      }
      MusicStream musicStream = (MusicStream)sender;

      switch (action)
      {
        case MusicStream.StreamAction.Ended:
          break;

        case MusicStream.StreamAction.Crossfading:
          string nextSong = Playlists.PlayListPlayer.SingletonPlayer.GetNextSong();
          if (nextSong != string.Empty)
          {
            g_Player.OnChanged(nextSong);
            PlayInternal(nextSong);
            g_Player.currentMedia = g_Player.MediaType.Music;
            g_Player.currentFilePlaying = nextSong;
            g_Player.OnStarted();
          }
          else
          {
            Log.Debug("BASS: Reached end of playlist.");
            g_Player.OnStopped();
            Stop();
          }
          break;

        case MusicStream.StreamAction.InternetStreamChanged:
          _tagInfo = musicStream.StreamTags;
          if (InternetStreamSongChanged != null)
          {
            InternetStreamSongChanged(this);
          }
          break;

        case MusicStream.StreamAction.Freed:
          {
            musicStream.Dispose();
          }
          break;

        case MusicStream.StreamAction.Disposed:
          // Remove the stream from the active streams and free it
          lock (_streams)
          {
            if (_streams.Contains(musicStream))
            {
              _streams.Remove(musicStream);
            }
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
      bool result = true;
      try
      {
        Log.Info("BASS: Initialize BASS environment ...");
        LoadSettings();

        result = BassRegistration.BassRegistration.Register();

        if (result)
        {
          // BASS_CONFIG_UPDATEPERIOD is set in InitDSDevice in case 
          // we are going to playback over DirectSound.
          Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 0);

          // Set the Global Volume. 0 = silent, 10000 = Full
          // We get 0 - 100 from Configuration, so multiply by 100
          Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, Config.StreamVolume * 100);
          Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, Config.BufferingMs);

          // network buffersize for webstreams should be larger then the playbackbuffer.
          // To insure a stable playback-start.
          int netBufferSize = Config.BufferingMs;

          // Minimize at default value.
          if (netBufferSize < 5000)
            netBufferSize = 5000;

          Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_BUFFER, netBufferSize);

          // PreBuffer() takes care of this.
          Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_PREBUF, 0);
        }

        InitializeControls();
        Config.LoadAudioDecoderPlugins();
        Config.LoadDSPPlugins();

        _playBackType = (int)Config.PlayBack;

        // Create a Stream Copy, if Visualisation is enabled
        if (HasViz)
        {
          Log.Debug("BASS: Create Stream copy for Visualisation");
          _streamcopy = new StreamCopy();
          _streamcopy.StreamCopyFlags = BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE;
        }

        Log.Info("BASS: Initializing BASS environment done.");

        _initialized = true;
        _bassFreed = true;
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

        if (_bassFreed && Config.MusicPlayer != AudioPlayer.Bass)
        {
          Log.Debug("BASS: BASS audio engine was previously freed. Re-Init");
          if (!Bass.BASS_Init(0, 48000, 0, IntPtr.Zero, Guid.Empty))
          {
            if (Bass.BASS_ErrorGetCode() != BASSError.BASS_ERROR_ALREADY)
            {
              HandleBassError("Initialze");
            }
          }
        }

        switch (Config.MusicPlayer)
        {
          case AudioPlayer.Bass:
            initOK = InitDirectSoundDevice();
            break;

          case AudioPlayer.Asio:
            initOK = InitAsio();
            break;

          case AudioPlayer.WasApi:
            initOK = InitWasapi();
            break;
        }

        if (initOK)
        {
          if (Settings.Instance.WinAmpPlugins.Count > 0)
          {
            BassWaDsp.BASS_WADSP_Init(GUIGraphicsContext.ActiveForm);
          }

          Log.Info("BASS: Initialization done.");
          _initialized = true;
          _bassFreed = false;
        }
        else
        {
          BASSError error = Bass.BASS_ErrorGetCode();
          if (Config.MusicPlayer == AudioPlayer.Asio)
          {
            BASSError errorasio = BassAsio.BASS_ASIO_ErrorGetCode();
            Log.Error("BASS: Error initializing BASS audio engine {0} Asio: {1}",
                      Enum.GetName(typeof(BASSError), error), Enum.GetName(typeof(BASSError), errorasio));
          }
          else
            Log.Error("BASS: Error initializing BASS audio engine {0}", Enum.GetName(typeof(BASSError), error));
        }
      }
      catch (Exception ex)
      {
        Log.Error("BASS: Initialize failed. Reason: {0}", ex.Message);
      }
    }


    /// <summary>
    /// Initialise the DirectSound Device
    /// </summary>
    /// <returns></returns>
    private bool InitDirectSoundDevice()
    {
      bool result = false;

      int soundDevice = GetSoundDevice();

      result =
        Bass.BASS_Init(soundDevice, 44100, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_LATENCY, IntPtr.Zero);

      BASS_INFO info = Bass.BASS_GetInfo();
      if (info != null)
      {
        _deviceOutputChannels = info.speakers;

        Log.Info("BASS: Device Information");
        Log.Info("BASS: ---------------------------------------------");
        Log.Info("BASS: Name: {0}", info.ToString());
        Log.Info("BASS: Directsound version: {0}", info.dsver);
        Log.Info("BASS: # Output Channels: {0}", info.speakers);
        Log.Debug("BASS: Minimum Buffer Samples: {0}", info.minbuf);
        Log.Debug("BASS: Current Sample rate: {0}", info.freq);
        Log.Debug("BASS: Maximum Sample rate: {0}", info.maxrate);
        Log.Debug("BASS: Minimum Sample rate: {0}", info.minrate);
        Log.Info("BASS: ---------------------------------------------");
      }

      // Bass will maximize the value to 100 ms
      Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 80);

      if (!result)
      {
        HandleBassError("InitDirectSoundDevice");
      }

      return result;
    }


    /// <summary>
    /// Initialises the Asio Device
    /// </summary>
    /// <returns></returns>
    private bool InitAsio()
    {
      bool result = false;

      Log.Info("BASS: Using ASIO device: {0}", Config.SoundDevice);
      BASS_ASIO_DEVICEINFO[] asioDevices = BassAsio.BASS_ASIO_GetDeviceInfos();
      // Check if the ASIO device read is amongst the one retrieved
      for (int i = 0; i < asioDevices.Length; i++)
      {
        if (asioDevices[i].name == Config.SoundDevice && asioDevices[i].driver == Config.SoundDeviceID)
        {
          _deviceNumber = i;
          break;
        }
      }

      if (_deviceNumber > -1)
      {
        result = BassAsio.BASS_ASIO_Init(_deviceNumber, BASSASIOInit.BASS_ASIO_THREAD);
        if (!result)
        {
          HandleBassError("InitAsio");
        }
        else
        {
          // Get some information about the Device
          BASS_ASIO_INFO info = BassAsio.BASS_ASIO_GetInfo();
          if (info != null)
          {
            _deviceOutputChannels = info.outputs;

            Log.Info("BASS: Device Information");
            Log.Info("BASS: ---------------------------------------------");
            Log.Info("BASS: Name: {0} {1}", info.name, info.version);
            Log.Info("BASS: # Input Channels: {0}", info.inputs);
            Log.Info("BASS: # Output Channels: {0}", info.outputs);
            Log.Debug("BASS: Minimum Buffer Samples: {0}", info.bufmin);
            Log.Debug("BASS: Maximum Buffer Samples: {0}", info.bufmax);
            Log.Debug("BASS: Default Buffer Length: {0}", info.bufpref);
            Log.Info("BASS: ---------------------------------------------");

            Log.Info("BASS: Channel Information");
            Log.Info("BASS: ---------------------------------------------");
            for (int i = 0; i < info.outputs; i++)
            {
              BASS_ASIO_CHANNELINFO chanInfo = BassAsio.BASS_ASIO_ChannelGetInfo(false, i);
              if (chanInfo != null)
              {
                Log.Info("BASS: {0}", chanInfo.ToString());
              }
            }
            Log.Info("BASS: ---------------------------------------------");
          }
          else
          {
            Log.Error("BASS: Error getting Device Info: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
          }
        }

        // When used in config the ASIO_INIT fails. Ignore it here, to be able using the visualisations
        if (Application.ExecutablePath.Contains("Configuration"))
        {
          result = true;
        }
      }
      else
      {
        Log.Error("BASS: Specified ASIO device not found. BASS is disabled.");
      }

      return result;
    }

    /// <summary>
    /// Initialises the Wasapi Device
    /// </summary>
    /// <returns></returns>
    private bool InitWasapi()
    {
      bool result = false;

      Log.Info("BASS: Using WASAPI device: {0}", Config.SoundDevice);
      BASS_WASAPI_DEVICEINFO[] wasapiDevices = BassWasapi.BASS_WASAPI_GetDeviceInfos();

      int i = 0;
      // Check if the WASAPI device read is amongst the one retrieved
      for (i = 0; i < wasapiDevices.Length; i++)
      {
        if (wasapiDevices[i].name == Config.SoundDevice && wasapiDevices[i].id == Config.SoundDeviceID)
        {
          _deviceNumber = i;
          break;
        }
      }

      if (_deviceNumber > -1)
      {
        // Get some information about the Device
        _wasapiDeviceInfo = wasapiDevices[i];
        if (_wasapiDeviceInfo != null)
        {
          // Get the number of supported speakers from Config
          _deviceOutputChannels = GetWasApiSpeakers();

          Log.Info("BASS: Device Information");
          Log.Info("BASS: ---------------------------------------------");
          Log.Info("BASS: Name: {0}", _wasapiDeviceInfo.name);
          Log.Debug("BASS: Id: {0}", _wasapiDeviceInfo.id);
          Log.Debug("BASS: Type: {0}", _wasapiDeviceInfo.type.ToString());
          Log.Info("BASS: Shared Mode Channels: {0}", _wasapiDeviceInfo.mixchans);
          Log.Info("BASS: Shared Mode Samplerate: {0}", _wasapiDeviceInfo.mixfreq);
          GetWasApiFormats();
          Log.Info("BASS: ---------------------------------------------");

          result = true;
        }
        else
        {
          Log.Error("BASS: Error getting Device Info: {0}", Bass.BASS_ErrorGetCode());
        }
      }
      else
      {
        Log.Error("BASS: Specified WASAPI device not found. BASS is disabled.");
      }

      return result;
    }

    /// <summary>
    /// Detect the supported output formats for WASAPI
    /// </summary>
    private void GetWasApiFormats()
    {
      int[] channels = { 1, 2, 3, 4, 5, 6, 7, 8 };
      int[] sampleRates = { 22050, 32000, 44100, 48000, 88200, 96000, 192000 };

      string selectedMode = "Shared";
      BASSWASAPIInit initFlag = BASSWASAPIInit.BASS_WASAPI_SHARED;
      if (Config.WasApiExclusiveMode)
      {
        selectedMode = "Exclusive";
        initFlag = BASSWASAPIInit.BASS_WASAPI_EXCLUSIVE;
      }

      Log.Debug("BASS: This device supports following formats in WASAPI {0} mode:", selectedMode);
      Log.Debug(string.Format("BASS: {0,-6} {1, -2} {2}", "Rate", "Ch", "Maximum Supported"));
      for (int sr = 0; sr < sampleRates.GetLength(0); sr++)
      {
        for (int c = 0; c < channels.GetLength(0); c++)
        {
          BASSWASAPIFormat format = BassWasapi.BASS_WASAPI_CheckFormat(_deviceNumber, sampleRates[sr], channels[c], initFlag);

          if (format != BASSWASAPIFormat.BASS_WASAPI_FORMAT_UNKNOWN)
          {
            Log.Debug(string.Format("BASS: {0,6} {1,2} {2}", sampleRates[sr], channels[c], format));
          }
        }
      }
    }

    /// <summary>
    /// WASAPI doesnj't provide a way to ask the driver on the maximum of supported speakers.
    /// So we try to enumerate the channels and detect the attached speakers.
    /// </summary>
    /// <returns></returns>
    private int GetWasApiSpeakers()
    {
      switch (Config.WasApiSpeakers)
      {
        case 0:
          return 1;

        case 1:
          return 2;

        case 2:
          return 4;

        case 3:
          return 6;

        case 4:
          return 8;
      }

      return 2;
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
          Log.Info("BASS: Device Information");
          Log.Info("BASS: ---------------------------------------------");
          Log.Info("BASS: Name: {0}", soundDeviceDescriptions[sounddevice].name);
          Log.Info("BASS: Default Device: {0}", soundDeviceDescriptions[sounddevice].IsDefault.ToString());
          Log.Info("BASS: ---------------------------------------------");
        }
      }
      return sounddevice;
    }

    /// <summary>
    /// Initialise Visualisation Controls and Create the Visualisation selected in the Configuration
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

      if (VizPluginInfo != null)
      {
        this.CreateVisualization(VizPluginInfo);
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

        if (vizType == (int)VisualizationInfo.PluginType.Sonique)
        {
          VizPluginInfo.UseOpenGL = xmlreader.GetValueAsBool("musicvisualization", "useOpenGL", true);
          VizPluginInfo.RenderTiming = xmlreader.GetValueAsInt("musicvisualization", "renderTiming", 25);
          VizPluginInfo.ViewPortSize = xmlreader.GetValueAsInt("musicvisualization", "viewPort", 0);
        }

        VizFPS = xmlreader.GetValueAsInt("musicvisualization", "fps", 30);
      }
    }


    #endregion

    #region Cleanup / Free Resources

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
      catch (Exception) { }

      if (Config.MusicPlayer == AudioPlayer.Asio)
      {
        BassAsio.BASS_ASIO_Stop();
        BassAsio.BASS_ASIO_Free();
      }

      if (Config.MusicPlayer == AudioPlayer.WasApi)
      {
        BassWasapi.BASS_WASAPI_Free();
      }

      if (_mixer != null)
      {
        _mixer.Dispose();
      }

      for (int i = 0; i < _streams.Count; i++)
      {
        if (_streams[i] != null)
        {
          _streams[i].Dispose();
        }
      }

      Bass.BASS_Stop();
      Bass.BASS_Free();
      _bassFreed = true;

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

      if (!_bassFreed)
      {
        Log.Info("BASS: Freeing BASS. Non-audio media playback requested.");
        if (Config.MusicPlayer == AudioPlayer.Asio)
        {
          BassAsio.BASS_ASIO_Free();
        }

        if (Config.MusicPlayer == AudioPlayer.WasApi)
        {
          BassWasapi.BASS_WASAPI_Free();
        }

        if (_mixer != null)
        {
          _mixer.Dispose();
        }

        Bass.BASS_Free();
        _bassFreed = true;
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
        try
        {
          GUIGraphicsContext.form.Controls.Add(VizWindow);
        }
        catch (Exception)
        {
          Log.Error("BASS: VizWindow exception");
       }
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
      // Return the clone of the stream, because for a decoding channel, we can't get data from the original stream
      return _streamcopy.ChannelHandle;
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
        try
        {
          VizWindow.Invoke(d, new object[] { visible });
        }
        catch { }
      }
      else
      {
        VizWindow.Visible = visible;
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Returns the Current Stream
    /// </summary>
    /// <returns></returns>
    internal MusicStream GetCurrentStream()
    {
      if (_streams.Count == 0)
      {
        return null;
      }

      return _streams[_streams.Count - 1];
    }

    /// <summary>
    /// Parse the Cue file. Return a Fake Cue track and adjest the playback position within the file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private bool HandleCueFile(ref string filePath, bool endOnly)
    {
      try
      {
        _cueTrackStartPos = 0;
        _cueTrackEndPos = 0;
        if (CueUtil.isCueFakeTrackFile(filePath))
        {
          Log.Debug("BASS: Playing CUE Track: {0}", filePath);
          _currentCueFakeTrackFileName = filePath;
          CueFakeTrack cueFakeTrack = CueUtil.parseCueFakeTrackFileName(filePath);
          if (!cueFakeTrack.CueFileName.Equals(_currentCueFileName))
          {
            // New CUE. Update chached cue.
            _currentCueSheet = new CueSheet(cueFakeTrack.CueFileName);
            _currentCueFileName = cueFakeTrack.CueFileName;
          }

          // Get track start position
          Track track = _currentCueSheet.Tracks[cueFakeTrack.TrackNumber - _currentCueSheet.Tracks[0].TrackNumber];
          Index index = track.Indices[0];
          _cueTrackStartPos = CueUtil.cueIndexToFloatTime(index);

          // If single audio file and is not last track, set track end position.
          if (_currentCueSheet.Tracks[_currentCueSheet.Tracks.Length - 1].TrackNumber > track.TrackNumber)
          {
            Track nextTrack =
              _currentCueSheet.Tracks[cueFakeTrack.TrackNumber - _currentCueSheet.Tracks[0].TrackNumber + 1];
            if (nextTrack.DataFile.Filename.Equals(track.DataFile.Filename))
            {
              Index nindex = nextTrack.Indices[0];
              _cueTrackEndPos = CueUtil.cueIndexToFloatTime(nindex);
            }
          }

          // If audio file is not changed, just set new start/end position and reset pause
          string audioFilePath = System.IO.Path.GetDirectoryName(cueFakeTrack.CueFileName) +
                                 System.IO.Path.DirectorySeparatorChar + track.DataFile.Filename;
          if (audioFilePath.CompareTo(_filePath) == 0 /* && StreamIsPlaying(stream)*/)
          {
            SetCueTrackEndPosition(GetCurrentStream(), endOnly);
            return true;
          }
          filePath = audioFilePath;
        }
        else
        {
          _currentCueFileName = null;
          _currentCueSheet = null;
        }
      }
      catch (System.IO.FileNotFoundException)
      {
        // The CUE File may have been moved
        Log.Error("BASS: Cue File cannot be found at the expected location. aborting playback.");
      }
      return false;
    }


    /// <summary>
    /// Sets the End Position for the CUE Track
    /// </summary>
    /// <param name="stream"></param>
    private void SetCueTrackEndPosition(MusicStream stream, bool endOnly)
    {
      if (_currentCueSheet != null)
      {
        stream.SetCueTrackEndPos(_cueTrackStartPos, _cueTrackEndPos, endOnly);
      }
    }

    /// <summary>
    /// Displays Information about a BASS Exception
    /// </summary>
    /// <param name="methodName"></param>
    private void HandleBassError(string methodName)
    {
      Log.Error("BASS: {0}() failed: {1}", methodName, Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
    }

    /// <summary>
    /// Called to start playback of next files internally, without makinbg use of g_player.
    /// This gives us the capability to achieve gapless playback.
    /// </summary>
    /// <param name="filePath"></param>
    private bool PlayInternal(string filePath)
    {
      if (filePath == string.Empty)
      {
        return false;
      }

      // Cue support
      if (HandleCueFile(ref filePath, true))
      {
        return true;
      }

      _filePath = filePath;

      MusicStream stream = new MusicStream(filePath);

      if (stream.BassStream == 0)
      {
        return false;
      }

      _streams.Add(stream);

      if (stream.Filetype.FileMainType == FileMainType.CDTrack)
      {
        _isCDDAFile = true;
      }
      else
      {
        _isCDDAFile = false;
      }

      if (_mixer == null)
      {
        _mixer = new MixerStream(this);
        if (!_mixer.CreateMixer(stream))
        {
          Log.Error("BASS: Could not create Mixer. Aborting playback.");
          return false;
        }

        // Start a StreamCopy for Visualisation purposes
        if (HasViz)
        {
          _streamcopy.ChannelHandle = _mixer.BassStream;
          _streamcopy.Start();
        }
      }
      else
      {
        if (NewMixerNeeded(stream))
        {
          Log.Debug("BASS: New stream has different number of channels or sample rate. Need a new mixer.");
          // Free Mixer
          _mixer.Dispose();
          _mixer = null;
          _mixer = new MixerStream(this);
          if (!_mixer.CreateMixer(stream))
          {
            Log.Error("BASS: Could not create Mixer. Aborting playback.");
            return false;
          }

          if (HasViz)
          {
            _streamcopy.ChannelHandle = _mixer.BassStream;
          }
        }
      }

      // Enable events, for various Playback Actions to be handled
      stream.MusicStreamMessage += new MusicStream.MusicStreamMessageHandler(OnMusicStreamMessage);

      SetCueTrackEndPosition(stream, false);

      // Plug in the stream into the Mixer
      if (!_mixer.AttachStream(stream))
      {
        return false;
      }

      return true;
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
      if (!_initialized)
      {
        return false;
      }

      MusicStream currentStream = GetCurrentStream();

      bool result = true;
      bool playbackStarted = false;
      Speed = 1; // Set playback Speed to normal speed

      try
      {
        if (currentStream != null && filePath.ToLowerInvariant().CompareTo(currentStream.FilePath.ToLowerInvariant()) == 0)
        {
          // Selected file is equal to current stream
          // Extend detection to permit to play the file if it failed.
          if (_state == PlayState.Paused || _state == PlayState.Init)
          {
          if (_state == PlayState.Paused)
          {
            // Resume paused stream
            currentStream.ResumePlayback();
            }

            result = Bass.BASS_Start();

            if (Config.MusicPlayer == AudioPlayer.Asio)
            {
              result = BassAsio.BASS_ASIO_ChannelReset(false, 0, BASSASIOReset.BASS_ASIO_RESET_PAUSE);   // Continue playback of Paused stream
            }
            else if (Config.MusicPlayer == AudioPlayer.WasApi)
            {
              BassWasapi.BASS_WASAPI_Start();
            }

            if (result)
            {
              _state = PlayState.Playing;

              if (PlaybackStateChanged != null)
              {
                PlaybackStateChanged(this, PlayState.Paused, _state);
              }
            }

            return result;
          }
        }
        else
        {
          // Cue support
          if ((currentStream != null && currentStream.IsPlaying))
          {
            if (HandleCueFile(ref filePath, false))
            {
              return true;
            }
          }
        }

        // If we're not Crossfading, we want to stop the current stream at this time
        if (currentStream != null && currentStream.IsPlaying)
        {
          if (!currentStream.IsCrossFading)
          {
            currentStream.FadeOutStop();
          }
        }

        _state = PlayState.Init;

        // If WASAPI is started, we might run into troubles, because of a new stream needed,
        // So let's stop it here
        if (Config.MusicPlayer == AudioPlayer.WasApi && BassWasapi.BASS_WASAPI_IsStarted())
        {
          Log.Debug("BASS: Stop WASAPI Device before start of new playback");
          BassWasapi.BASS_WASAPI_Stop(true);
        }

        if (!PlayInternal(filePath))
        {
          return false;
        }

        if (Config.MusicPlayer == AudioPlayer.Asio && !BassAsio.BASS_ASIO_IsStarted())
        {
          BassAsio.BASS_ASIO_Stop();
          playbackStarted = BassAsio.BASS_ASIO_Start(0);
        }
        else if (Config.MusicPlayer == AudioPlayer.WasApi && !BassWasapi.BASS_WASAPI_IsStarted())
        {
          playbackStarted = BassWasapi.BASS_WASAPI_Start();
        }
        else
        {
          if (Bass.BASS_ChannelIsActive(_mixer.BassStream) == BASSActive.BASS_ACTIVE_PLAYING)
          {
            playbackStarted = true;
          }
          else
          {
            playbackStarted = Bass.BASS_ChannelPlay(_mixer.BassStream, false);
          }
        }

        MusicStream stream = GetCurrentStream();
        
        if (stream.BassStream != 0 && playbackStarted)
        {
          Log.Info("BASS: playback started");

          // Set the Tag Info for Web Streams
          if (stream.Filetype.FileMainType == FileMainType.WebStream)
          {
            _tagInfo = stream.StreamTags;
          }

          // Slide in the Stream over the Cross fade Interval
          stream.SlideIn();

          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
          msg.Label = _filePath;
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

          PlayState oldState = _state;
          _state = PlayState.Playing;

          if (oldState != _state && PlaybackStateChanged != null)
          {
            PlaybackStateChanged(this, oldState, _state);
          }

          if (PlaybackStart != null)
          {
            PlaybackStart(g_Player.MediaType.Music, filePath);
          }
        }
        else
        {
          Log.Error("BASS: Unable to play {0}.  Reason: {1}.", filePath,
                    Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));

          stream.Dispose();
          result = false;
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
      MusicStream stream = GetCurrentStream();

      Log.Debug("BASS: Pause of stream {0}", stream.FilePath);
      try
      {
        PlayState oldPlayState = _state;

        if (oldPlayState == PlayState.Ended || oldPlayState == PlayState.Init)
        {
          return;
        }

        if (oldPlayState == PlayState.Paused)
        {
          _state = PlayState.Playing;

          if (Config.SoftStop)
          {
            // Fade-in over 500ms
            Bass.BASS_ChannelSlideAttribute(_mixer.BassStream, BASSAttribute.BASS_ATTRIB_VOL, 1, 500);
          }
          else
          {
            Bass.BASS_ChannelSetAttribute(_mixer.BassStream, BASSAttribute.BASS_ATTRIB_VOL, 1);
          }

          BassMix.BASS_Mixer_ChannelPlay(_mixer.BassStream);
          Bass.BASS_Start();

          if (Config.MusicPlayer == AudioPlayer.Asio)
          {
            BassAsio.BASS_ASIO_ChannelReset(false, 0, BASSASIOReset.BASS_ASIO_RESET_PAUSE);
          }
          else if (Config.MusicPlayer == AudioPlayer.WasApi)
          {
            BassWasapi.BASS_WASAPI_Start();
          }
        }

        else
        {
          _state = PlayState.Paused;

          if (Config.SoftStop)
          {
            // Fade-out over 500ms
            Bass.BASS_ChannelSlideAttribute(_mixer.BassStream, BASSAttribute.BASS_ATTRIB_VOL, 0, 500);

            // Wait until the slide is done
            while (Bass.BASS_ChannelIsSliding(_mixer.BassStream, BASSAttribute.BASS_ATTRIB_VOL))
              System.Threading.Thread.Sleep(20);
          }

          BassMix.BASS_Mixer_ChannelPause(_mixer.BassStream);
          Bass.BASS_Pause();

          if (Config.MusicPlayer == AudioPlayer.Asio)
          {
            BassAsio.BASS_ASIO_ChannelPause(false, 0);
          }
          else if (Config.MusicPlayer == AudioPlayer.WasApi)
          {
            BassWasapi.BASS_WASAPI_Stop(true);
          }
        }

        if (oldPlayState != _state)
        {
          if (PlaybackStateChanged != null)
          {
            PlaybackStateChanged(this, oldPlayState, _state);
          }
        }
      }

      catch { }
    }

    /// <summary>
    /// Stopping Playback
    /// </summary>
    public override void Stop()
    {
      // We might have performed the Stop already, because the end of the playback list was reached
      // g_Player is calling the Stop a second time. Don't execute the commands in this case
      if (_mixer == null)
      {
        Log.Debug("BASS: Already stopped. Don't execute Stop a second time");
        return;
      }

      // Execute the Stop in a separate thread, so that it doesn't block the Main UI Render thread
      new Thread(() =>
                   {
                     // First deactivate Viz RenderThread, in HandleSongEnded, it's too late
                     VizWindow.Run = false;

                     MusicStream stream = GetCurrentStream();
                     try
                     {
                       if (stream != null && !stream.IsDisposed)
                       {
                         Log.Debug("BASS: Stop of stream {0}.", stream.FilePath);
                         if (Config.SoftStop && !stream.IsDisposed && !stream.IsCrossFading)
                         {
                           if (Config.CrossFadeIntervalMs > 0)
                           {
                             Log.Debug("BASS: Performing Softstop of {0}", stream.FilePath);
                             Bass.BASS_ChannelSlideAttribute(stream.BassStream, BASSAttribute.BASS_ATTRIB_VOL, 0,
                                                             Config.CrossFadeIntervalMs);

                             // Wait until the slide is done
                             // Sometimes the slide is causing troubles, so we wait a maximum of CrossfadeIntervals + 100 ms
                             // Enable only if it's music playing
                             if (g_Player.IsMusic && g_Player._currentMediaForBassEngine != g_Player.MediaType.Video &&
                                 g_Player._currentMediaForBassEngine != g_Player.MediaType.TV &&
                                 g_Player._currentMediaForBassEngine != g_Player.MediaType.Recording)
                             {
                             DateTime start = DateTime.Now;
                             while (Bass.BASS_ChannelIsSliding(stream.BassStream, BASSAttribute.BASS_ATTRIB_VOL))
                             {
                               System.Threading.Thread.Sleep(20);
                               if ((DateTime.Now - start).TotalMilliseconds > Config.CrossFadeIntervalMs + 100)
                               {
                                 break;
                               }
                             }
                           }
                         }
                         }
                         BassMix.BASS_Mixer_ChannelRemove(stream.BassStream);
                         stream.Dispose();
                       }

                       if (Config.MusicPlayer == AudioPlayer.Asio && BassAsio.BASS_ASIO_IsStarted())
                       {
                         Log.Debug("BASS: Stopping ASIO Device");
                         if (!BassAsio.BASS_ASIO_Stop())
                         {
                           Log.Error("BASS: Error freeing ASIO: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
                         }
                         Log.Debug("BASS: unjoin ASIO CHannels");
                         if (!BassAsio.BASS_ASIO_ChannelReset(false, -1, BASSASIOReset.BASS_ASIO_RESET_JOIN))
                         {
                           Log.Error("BASS: Error unjoining Asio Channels: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
                         }
                         Log.Debug("BASS: disabling ASIO CHannels");
                         if (!BassAsio.BASS_ASIO_ChannelReset(false, -1, BASSASIOReset.BASS_ASIO_RESET_ENABLE))
                         {
                           Log.Error("BASS: Error disabling Asio Channels: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
                         }
                       }

                       if (Config.MusicPlayer == AudioPlayer.WasApi && BassWasapi.BASS_WASAPI_IsStarted())
                       {
                         try
                         {
                           Log.Debug("BASS: Stopping WASAPI Device");
                           if (!BassWasapi.BASS_WASAPI_Stop(true))
                           {
                             Log.Error("BASS: Error stopping WASAPI Device: {0}", Bass.BASS_ErrorGetCode());
                           }

                           if (!BassWasapi.BASS_WASAPI_Free())
                           {
                             Log.Error("BASS: Error freeing WASAPI: {0}", Bass.BASS_ErrorGetCode());
                           }
                         }
                         catch (Exception ex)
                         {
                           Log.Error("BASS: Exception freeing WASAPI. {0} {1}", ex.Message, ex.StackTrace);
                         }
                       }

                       if (_mixer != null)
                       {
                         _mixer.Dispose();
                         _mixer = null;
                       }

                       // If we did a playback of a Audio CD, release the CD, as we might have problems with other CD related functions
                       if (_isCDDAFile)
                       {
                         int driveCount = BassCd.BASS_CD_GetDriveCount();
                         for (int i = 0; i < driveCount; i++)
                         {
                           BassCd.BASS_CD_Release(i);
                         }
                       }

                       if (PlaybackStop != null)
                       {
                         PlaybackStop(this);
                       }

                       HandleSongEnded();

                       // Remove the Viz Window from the Main Form as it causes troubles to other plugin overlay window
                       try
                       {
                         RemoveVisualizationWindow();
                       }
                       catch (Exception)
                       {
                         Log.Error("BASS: Stop RemoveVisualizationWindow command caused an exception");
                       }

                       // Switching back to normal playback mode
                       SwitchToDefaultPlaybackMode();
                     }

                     catch (Exception ex)
                     {
                       Log.Error("BASS: Stop command caused an exception - {0}. {1}", ex.Message, ex.StackTrace);
                     }

                     NotifyPlaying = false;
                   }
        ) { Name = "BASS Stop" }.Start();
    }

    /// <summary>
    /// Handle Stop of a song
    /// </summary>
    private void HandleSongEnded()
    {
      PlayState oldState = _state;

      if (!Util.Utils.IsAudio(_filePath) || g_Player._currentMediaForBassEngine == g_Player.MediaType.Video)
      {
        GUIGraphicsContext.IsFullScreenVideo = false;
      }

      ShowVisualizationWindow(false);

      GUIGraphicsContext.IsPlaying = false;

      _filePath = "";
      _state = PlayState.Ended;

      if (oldState != _state && PlaybackStateChanged != null)
      {
        PlaybackStateChanged(this, oldState, _state);
      }
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

      if (State != PlayState.Playing)
      {
        return false;
      }

      if (ms <= 0)
      {
        return false;
      }

      try
      {
        MusicStream stream = GetCurrentStream();
        long pos = Bass.BASS_ChannelGetPosition(stream.BassStream);

        double timePos = Bass.BASS_ChannelBytes2Seconds(stream.BassStream, pos);
        double newPos = timePos + (double)ms / 1000.0;

        if (newPos >= stream.TotalStreamSeconds)
        {
          return false;
        }

        Bass.BASS_ChannelSetPosition(stream.BassStream, Bass.BASS_ChannelSeconds2Bytes(stream.BassStream, newPos));
        Bass.BASS_ChannelSetPosition(_mixer.BassStream, 0, BASSMode.BASS_POS_BYTES); // reset the mixer
        _mixer.SetSyncPos(stream, newPos);
      }
      catch
      {
        return false;
      }

      return true;
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

      if (State != PlayState.Playing)
      {
        return false;
      }

      if (ms <= 0)
      {
        return false;
      }

      try
      {
        MusicStream stream = GetCurrentStream();
        long pos = Bass.BASS_ChannelGetPosition(stream.BassStream);

        double timePos = Bass.BASS_ChannelBytes2Seconds(stream.BassStream, pos);
        double newPos = timePos - (double)ms / 1000.0;

        if (newPos <= 0)
        {
          return false;
        }

        Bass.BASS_ChannelSetPosition(stream.BassStream, Bass.BASS_ChannelSeconds2Bytes(stream.BassStream, newPos));
        Bass.BASS_ChannelSetPosition(_mixer.BassStream, 0, BASSMode.BASS_POS_BYTES); // reset the mixer
        _mixer.SetSyncPos(stream, newPos);
      }

      catch
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Seek to a specific position in the stream
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool SeekToTimePosition(int position)
    {
      Log.Debug("BASS: SeekToTimePosition: {0} ", Convert.ToString(position));

      bool result = true;

      try
      {
        MusicStream stream = GetCurrentStream();

        if (stream.IsPlaying)
        {
          if (_currentCueSheet != null)
          {
            position += (int)_cueTrackStartPos;
          }

          bool isWASAPI = false;
          // For WASAPI output we need to Stop / Start for skipping to work
          if (BassWasapi.BASS_WASAPI_IsStarted())
          {
            isWASAPI = true;
            BassWasapi.BASS_WASAPI_Stop(true);
          }
          
          BassMix.BASS_Mixer_ChannelSetPosition(stream.BassStream, Bass.BASS_ChannelSeconds2Bytes(stream.BassStream, position));
          Bass.BASS_ChannelSetPosition(_mixer.BassStream, 0, BASSMode.BASS_POS_BYTES); // reset the mixer
          _mixer.SetSyncPos(stream, position);
          
          if (isWASAPI)
          {
            BassWasapi.BASS_WASAPI_Start();
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
      if (_state != PlayState.Init)
      {
        double dCurTime = GetCurrentStream().StreamElapsedTime;

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
      if (_state != PlayState.Init)
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
      if (_state != PlayState.Init)
      {
        double dCurrentPos = GetCurrentStream().StreamElapsedTime;
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
      if (_state != PlayState.Init)
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

      if (!Config.UseSkipSteps)
      {
        TimeSpan ts = DateTime.Now - _seekUpdate;
        if (_speed > 1 && ts.TotalMilliseconds > 120)
        {
          SeekForward(80 * _speed);
          _seekUpdate = DateTime.Now;
        }
        else if (_speed < 0 && ts.TotalMilliseconds > 120)
        {
          SeekReverse(80 * -_speed + (int)ts.TotalMilliseconds);
          _seekUpdate = DateTime.Now;
        }
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

        _VideoWidth = GUIGraphicsContext.form.ClientRectangle.Width;
        _VideoHeight = GUIGraphicsContext.form.ClientRectangle.Height;

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
        VizWindow.Visible = _state == PlayState.Playing;
      }
      else
      {
        VizWindow.Visible = false;
      }
    }

    #endregion

    #region  Public Methods

    /// <summary>
    /// Checks, if a new Mixer would be needed, because of changes in Sample Rate or number of channels
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool NewMixerNeeded(MusicStream stream)
    {
      if (!_mixer.UpMixing)
      {
        BASS_CHANNELINFO chinfo = Bass.BASS_ChannelGetInfo(_mixer.BassStream);
        if (!_mixer.WasApiShared &&
            (chinfo.freq != stream.ChannelInfo.freq || (chinfo.chans != stream.ChannelInfo.chans && stream.ChannelInfo.chans != 1)))
        {
          if (stream.ChannelInfo.freq != _mixer.WasApiMixedFreq ||
              stream.ChannelInfo.chans != _mixer.WasApiMixedChans)
          {
            return true;
          }
        }
      }

      return false;
    }

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
      if (_playBackType == (int)PlayBackType.CROSSFADE)
      {
        // Store the current settings, so that when the album playback is completed, we can switch back to the default
        if (_savedPlayBackType == -1)
        {
          _savedPlayBackType = _playBackType;
        }

        Log.Info("BASS: Playback of complete Album starting. Switching playbacktype from {0} to {1}",
                 Enum.GetName(typeof(PlayBackType), _playBackType),
                 Enum.GetName(typeof(PlayBackType), (int)PlayBackType.GAPLESS));

        _playBackType = (int)PlayBackType.GAPLESS;
        Config.CrossFadeIntervalMs = 0;
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
                 Enum.GetName(typeof(PlayBackType), _playBackType),
                 Enum.GetName(typeof(PlayBackType), _savedPlayBackType));

        if (_savedPlayBackType == 0)
        {
          Config.CrossFadeIntervalMs = 100;
        }
        else if (_savedPlayBackType == 1)
        {
          Config.CrossFadeIntervalMs = 0;
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

      MusicStream stream = GetCurrentStream();

      if (Config.MusicPlayer == AudioPlayer.Asio)
      {
        float fpeakL = BassAsio.BASS_ASIO_ChannelGetLevel(false, 0);
        float fpeakR = (int)BassAsio.BASS_ASIO_ChannelGetLevel(false, 1);
        dbLeft = 20.0 * Math.Log10(fpeakL);
        dbRight = 20.0 * Math.Log10(fpeakR);
      }
      else if (Config.MusicPlayer == AudioPlayer.WasApi)
      {
        level = BassWasapi.BASS_WASAPI_GetLevel();
      }
      else
      {
        level = BassMix.BASS_Mixer_ChannelGetLevel(stream.BassStream);
      }

      if (Config.MusicPlayer != AudioPlayer.Asio) // For Asio, we already got the peaklevel above
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