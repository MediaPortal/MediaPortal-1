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
    #region Enum

    /// <summary>
    /// States, how the Playback is handled
    /// </summary>
    public enum PlayBackType : int
    {
      NORMAL = 0,
      GAPLESS = 1,
      CROSSFADE = 2
    }

    #endregion

    #region Variables

    private static List<int> _decoderPluginHandles = new List<int>();

    private static Config _instance = null;

    private static string _soundDevice;
    private static string _asioDevice;

    private static int _streamVolume;
    private static int _bufferingMs;
    private static int _crossFadeIntervalMs;

    private static float _asioBalance;

    private static bool _softStop;
    private static bool _mixing;
    private static bool _useASIO;

    private static PlayBackType _playBackType;

    private static string _cdDriveLetters = ""; // Contains the Drive letters of all available CD Drives

    // DSP related variables
    private static bool _dspActive = false;

    // DSP related Variables
    private static DSP_Gain _gain = null;
    private static BASS_BFX_DAMP _damp = null;
    private static BASS_BFX_COMPRESSOR _comp = null;
    private static int _dampPrio = 3;
    private static int _compPrio = 2;

    // VST Related variables
    private static List<string> _vstPlugins = new List<string>();
    private static Dictionary<string, int> _vstHandles = new Dictionary<string, int>();

    // Winamp related variables
    private static bool _waDspInitialised = false;
    private static Dictionary<string, int> _waDspPlugins = new Dictionary<string, int>();

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

    public static string AsioDevice
    {
      get { return _asioDevice; }
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

    public static bool SoftStop
    {
      get { return _softStop; }
    }

    public static bool Mixing
    {
      get { return _mixing; }
    }

    public static bool UseAsio
    {
      get { return _useASIO; }
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
        Log.Info("BASS: LOading Settings");
        _soundDevice = xmlreader.GetValueAsString("audioplayer", "sounddevice", "Default Sound Device");
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

        _mixing = xmlreader.GetValueAsBool("audioplayer", "mixing", false);

        _useASIO = xmlreader.GetValueAsBool("audioplayer", "asio", false);
        _asioDevice = xmlreader.GetValueAsString("audioplayer", "asiodevice", "None");
        _asioBalance = (float)xmlreader.GetValueAsInt("audioplayer", "asiobalance", 0) / 100.00f;

        bool doGaplessPlayback = xmlreader.GetValueAsBool("audioplayer", "gaplessPlayback", false);

        if (doGaplessPlayback)
        {
          _crossFadeIntervalMs = 200;
          _playBackType = PlayBackType.GAPLESS;
        }
        else
        {
          if (_crossFadeIntervalMs == 0)
          {
            _playBackType = PlayBackType.NORMAL;
            _crossFadeIntervalMs = 100;
          }
          else
          {
            _playBackType = PlayBackType.CROSSFADE;
          }
        }
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
        if (Path.GetExtension(file.Name).ToLower() != ".dll")
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
        if (Path.GetExtension(file.Name).ToLower() != ".sf2")
        {
          continue;
        }
        int font = BassMidi.BASS_MIDI_FontInit(file.FullName);
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
      // BASS DSP/FX
      foreach (BassEffect basseffect in Player.DSP.Settings.Instance.BassEffects)
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

    /// <summary>
    /// Sets the parameter for a given Bass effect
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="format"></param>
    private static void setBassDSP(string id, string name, string value)
    {
      switch (id)
      {
        case "Gain":
          if (name == "Gain_dbV")
          {
            double gainDB = double.Parse(value);
            if (_gain == null)
            {
              _gain = new DSP_Gain();
            }

            if (gainDB == 0.0)
            {
              _gain.SetBypass(true);
            }
            else
            {
              _gain.SetBypass(false);
              _gain.Gain_dBV = gainDB;
            }
          }
          break;

        case "DynAmp":
          if (name == "Preset")
          {
            if (_damp == null)
            {
              _damp = new BASS_BFX_DAMP();
            }

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
            {
              _comp = new BASS_BFX_COMPRESSOR();
            }

            _comp.Preset_Medium();
            _comp.fThreshold = (float)Un4seen.Bass.Utils.DBToLevel(Convert.ToInt32(value) / 10d, 1.0);
          }
          break;
      }
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
