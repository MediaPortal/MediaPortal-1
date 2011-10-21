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

using MediaPortal.GUI.Library;

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

    #endregion 
  }
}
