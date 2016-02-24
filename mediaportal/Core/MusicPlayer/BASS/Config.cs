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
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player.DSP;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Midi;
using Un4seen.Bass.AddOn.Vst;
using Un4seen.Bass.Misc;

namespace MediaPortal.MusicPlayer.BASS
{
  /// <summary>
  /// This class holds the various Configuration values, which are needed by the Audio Engine
  /// and the MusicStream Object, to avoid constantly accessing the Settings in MediaPortal.xml
  /// </summary>
  public class Config
  {

    #region Variables

    private static List<int> _decoderPluginHandles = new List<int>();

    private static Config _instance = null;

    private static AudioPlayer _audioPlayer;
    private static string _soundDevice;
    private static string _soundDeviceID;

    private static int _upMixMono;
    private static int _upMixStereo;
    private static int _upMixQuadro;
    private static int _upMixFiveDotOne;

    private static int _streamVolume;
    private static int _bufferingMs;
    private static int _crossFadeIntervalMs;

    private static float _asioBalance;

    private static bool _wasApiExclusiveMode = false;
    private static int _wasApiSpeakers = 2;

    private static bool _softStop;
    private static bool _useSkipSteps;
    private static bool _enableReplaygain;
    private static bool _enableAlbumReplaygain;
    
    private static PlayBackType _playBackType;

    private static string _cdDriveLetters = ""; // Contains the Drive letters of all available CD Drives

    // DSP related variables
    private static bool _dspActive = false;

    // VST Related variables
    private static List<string> _vstPlugins = new List<string>();
    private static Dictionary<string, int> _vstHandles = new Dictionary<string, int>();

    // Midi File support
    private static BASS_MIDI_FONT[] _soundFonts = null;
    private static List<int> _soundFontHandles = new List<int>();

    #endregion

    #region Properties

    public static Config Instance
    {
      get { return _instance; }
    }

    public static string SoundDevice
    {
      get { return _soundDevice; }
    }

    public static string SoundDeviceID
    {
      get { return _soundDeviceID; }
    }

    public static AudioPlayer MusicPlayer
    {
      get { return _audioPlayer; }
    }

    public static int StreamVolume
    {
      get { return _streamVolume;}
      set { _streamVolume = value;}
    }

    public static int BufferingMs
    {
      get { return _bufferingMs; }
      set { _bufferingMs = value; }
    }

    public static int CrossFadeIntervalMs
    {
      get { return _crossFadeIntervalMs; }
      set { _crossFadeIntervalMs = value; }
    }

    public static float AsioBalance
    {
      get { return _asioBalance; }
    }

    public static bool WasApiExclusiveMode
    {
      get { return _wasApiExclusiveMode; }
    }

    public static int WasApiSpeakers
    {
      get { return _wasApiSpeakers; }
    }

    public static bool SoftStop
    {
      get { return _softStop; }
    }

    public static bool UseSkipSteps
    {
      get { return _useSkipSteps; }
    }

    public static bool EnableReplayGain
    {
      get { return _enableReplaygain; }
    }

    public static bool EnableAlbumReplayGain
    {
      get { return _enableAlbumReplaygain; }
    }

    public static PlayBackType PlayBack
    {
      get { return _playBackType; }
    }

    public static string CdDriveLetters
    {
      get
      {
        // Check, if we have the CDDruves loaded yet
        if (_cdDriveLetters == "")
        {
          GetCDDrives();
        }
        return _cdDriveLetters;
      }
    }

    public static bool DSPActive
    {
      get { return _dspActive; }
    }

    public static List<string> VstPlugins
    {
      get { return _vstPlugins; }
    }
 
    public static Dictionary<string, int> VstHandles
    {
      get { return _vstHandles; }
    }
    
    public static BASS_MIDI_FONT[] SoundFonts
    {
      get { return _soundFonts; }
    }

    public static MonoUpMix UpmixMono
    {
      get { return (MonoUpMix)_upMixMono; }
    }

    public static StereoUpMix UpmixStereo
    {
      get { return (StereoUpMix)_upMixStereo; }
    }

    public static QuadraphonicUpMix UpmixQuadro
    {
      get { return (QuadraphonicUpMix)_upMixQuadro; }
    }

    public static FiveDotOneUpMix UpmixFiveDotOne
    {
      get { return (FiveDotOneUpMix)_upMixFiveDotOne; }
    }

    #endregion

    #region Constructor

    // Singleton -- make sure we can't instantiate this class
    static Config()
    {
      _instance = new Config();
    }

    public Config()
    {
      LoadSettings();
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Load Settings
    /// </summary>
    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        Log.Info("BASS: Loading Settings");
        string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "playerId", "0");
        _audioPlayer = (AudioPlayer)Enum.Parse(typeof (AudioPlayer), strAudioPlayer);
        _soundDevice = xmlreader.GetValueAsString("audioplayer", "sounddevice", "Default Sound Device");
        _soundDeviceID = xmlreader.GetValueAsString("audioplayer", "sounddeviceid", "");
        _streamVolume = xmlreader.GetValueAsInt("audioplayer", "streamOutputLevel", 85);
        _bufferingMs = xmlreader.GetValueAsInt("audioplayer", "buffering", 5000);

        if (_bufferingMs <= 0)
        {
          _bufferingMs = 1000;
        }
        else if (_bufferingMs > 8000)
        {
          _bufferingMs = 8000;
        }

        _crossFadeIntervalMs = xmlreader.GetValueAsInt("audioplayer", "crossfade", 4000);

        if (_crossFadeIntervalMs < 0)
        {
          _crossFadeIntervalMs = 0;
        }
        else if (_crossFadeIntervalMs > 16000)
        {
          _crossFadeIntervalMs = 16000;
        }

        _softStop = xmlreader.GetValueAsBool("audioplayer", "fadeOnStartStop", true);
        _useSkipSteps = xmlreader.GetValueAsBool("audioplayer", "useSkipSteps", false);
        _enableReplaygain = xmlreader.GetValueAsBool("audioplayer", "enableReplayGain", false);
        _enableAlbumReplaygain = xmlreader.GetValueAsBool("audioplayer", "enableAlbumReplayGain", false);

        _asioBalance = (float)xmlreader.GetValueAsInt("audioplayer", "asiobalance", 0) / 100.00f;
        _wasApiExclusiveMode = xmlreader.GetValueAsBool("audioplayer", "wasapiExclusive", false);
        _wasApiSpeakers = xmlreader.GetValueAsInt("audioplayer", "wasApiSpeakers", 1);

        bool doGaplessPlayback = xmlreader.GetValueAsBool("audioplayer", "gaplessPlayback", false);

        if (doGaplessPlayback)
        {
          _crossFadeIntervalMs = 0; 
          _playBackType = PlayBackType.GAPLESS;
        }
        else
        {
          if (_crossFadeIntervalMs == 0)
          {
            _playBackType = PlayBackType.NORMAL;
            //_crossFadeIntervalMs = 100;
          }
          else
          {
            _playBackType = PlayBackType.CROSSFADE;
          }
        }

        _upMixMono = xmlreader.GetValueAsInt("audioplayer", "upMixMono", 0);
        _upMixStereo = xmlreader.GetValueAsInt("audioplayer", "upMixStereo", 0);
        _upMixQuadro = xmlreader.GetValueAsInt("audioplayer", "upMixQuadro", 0);
        _upMixFiveDotOne = xmlreader.GetValueAsInt("audioplayer", "upMixFiveDotOne", 0);
      }
    }

    /// <summary>
    /// Load External BASS Audio Decoder Plugins
    /// </summary>
    public static void LoadAudioDecoderPlugins()
    {
      Log.Info("BASS: Loading audio decoder add-ins...");

      string appPath = Application.StartupPath;
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
        if (Path.GetExtension(file.Name).ToLowerInvariant() != ".dll")
        {
          continue;
        }

        pluginHandle = Bass.BASS_PluginLoad(file.FullName);

        if (pluginHandle != 0)
        {
          _decoderPluginHandles.Add(pluginHandle);
          decoderCount++;
          Log.Debug("BASS: Added DecoderPlugin: {0}", file.FullName);
        }

        else
        {
          Log.Debug("BASS: Unable to load: {0}", file.FullName);
        }
      }

      if (decoderCount > 0)
      {
        Log.Info("BASS: Loaded {0} Audio Decoders.", decoderCount);
      }

      else
      {
        Log.Error(
          @"BASS: No audio decoders were loaded. Confirm decoders are present in \musicplayer\plugins\audio decoders folder.");
      }

      // Look for any SF2 files available in the folder.
      // SF2 files contain sound fonts needed for midi playback
      List<BASS_MIDI_FONT> tmpFonts = new List<BASS_MIDI_FONT>();
      foreach (FileInfo file in decoders)
      {
        if (Path.GetExtension(file.Name).ToLowerInvariant() != ".sf2")
        {
          continue;
        }
        int font = BassMidi.BASS_MIDI_FontInit(file.FullName, BASSFlag.BASS_MIDI_FONT_MMAP);
        if (font != 0)
        {
          BASS_MIDI_FONTINFO fontInfo = new BASS_MIDI_FONTINFO();
          BassMidi.BASS_MIDI_FontGetInfo(font, fontInfo);
          Log.Info("BASS: Loading Midi font: {0}", fontInfo.ToString());
          _soundFontHandles.Add(font);
          BASS_MIDI_FONT soundFont = new BASS_MIDI_FONT(font, -1, 0);
          tmpFonts.Add(soundFont);
        }
      }

      if (tmpFonts.Count > 0)
      {
        _soundFonts = tmpFonts.ToArray();
      }
    }

    /// <summary>
    /// Load BASS DSP Plugins specified setup in the Configuration
    /// </summary>
    public static void LoadDSPPlugins()
    {
      Log.Debug("BASS: Loading DSP plugins ...");
      _dspActive = false;

      // VST Plugins
      string vstPluginDir = Path.Combine(Application.StartupPath, @"musicplayer\plugins\dsp");
      int vstHandle = 0;
      foreach (VSTPlugin plugins in Player.DSP.Settings.Instance.VSTPlugins)
      {
        // Get the vst handle and enable it
        string plugin = String.Format(@"{0}\{1}", vstPluginDir, plugins.PluginDll);
        vstHandle = BassVst.BASS_VST_ChannelSetDSP(0, plugin, BASSVSTDsp.BASS_VST_DEFAULT, 1);
        if (vstHandle > 0)
        {
          _dspActive = true;
          _vstPlugins.Add(plugins.PluginDll);
          // Store the handle in the dictionary for later reference
          _vstHandles[plugins.PluginDll] = vstHandle;
          // Set all parameters for the plugin
          foreach (VSTPluginParm paramter in plugins.Parameter)
          {
            NumberFormatInfo format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";
            try
            {
              BassVst.BASS_VST_SetParam(vstHandle, paramter.Index, float.Parse(paramter.Value));
            }
            catch (Exception) { }
          }
        }
        else
        {
          Log.Debug("Couldn't load VST Plugin {0}. Error code: {1}", plugin, Bass.BASS_ErrorGetCode());
        }
      }

      // Winamp Plugins can only be loaded on play to prevent Crashes
      if (Player.DSP.Settings.Instance.WinAmpPlugins.Count > 0)
      {
        _dspActive = true;
      }

      Log.Debug("BASS: Finished loading DSP plugins ...");
    }

    private static void GetCDDrives()
    {
      // Get the number of CD/DVD drives
      int driveCount = BassCd.BASS_CD_GetDriveCount();
      StringBuilder builderDriveLetter = new StringBuilder();
      // Get Drive letters assigned
      for (int i = 0; i < driveCount; i++)
      {
        builderDriveLetter.Append(BassCd.BASS_CD_GetInfo(i).DriveLetter);
        BassCd.BASS_CD_Release(i);
      }
      _cdDriveLetters = builderDriveLetter.ToString();
    }

    #endregion 
  }
}
