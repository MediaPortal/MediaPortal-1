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
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.MusicPlayer.BASS;
using Un4seen.Bass.AddOn.Cd;

namespace MediaPortal.Player
{
  /// <summary>
  /// This singleton class is responsible for managing the BASS audio Engine object.
  /// </summary>
  public class BassMusicPlayer
  {
    #region Variables

    internal static BassAudioEngine _player;
    private static Thread _bassAsyncLoadThread = null;
    private static bool _isDefaultMusicPlayer = false;
    private static bool _settingsLoaded = false;
    private static object _syncRoot = new Object();

    #endregion

    #region Constructors/Destructors

    // Singleton -- make sure we can't instantiate this class
    private BassMusicPlayer() { }

    #endregion

    #region Properties

    /// <summary>
    /// Returns the BassAudioEngine Object
    /// </summary>
    public static BassAudioEngine Player
    {
      get
      {
        if (_player == null)
        {
          lock (_syncRoot)
          {
            if (_player == null)
            {
              _player = new BassAudioEngine();
            }
          }
        }

        return _player;
      }
    }

    /// <summary>
    /// Returns a Boolean if the BASS Audio Engine is initialised
    /// </summary>
    public static bool Initialized
    {
      get { return _player != null && _player.Initialized; }
    }

    /// <summary>
    /// Returns a Boolean if the BASS Audio Engine is the Default Player selected in Configuration
    /// </summary>
    public static bool IsDefaultMusicPlayer
    {
      get
      {
        if (!_settingsLoaded)
        {
          using (Profile.Settings xmlreader = new Profile.MPSettings())
          {
            string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "playerId", "0");
            AudioPlayer audioPlayer = AudioPlayer.Bass;
            try
            {
              audioPlayer = (AudioPlayer)Enum.Parse(typeof(AudioPlayer), strAudioPlayer);
            }
            catch (Exception) // We end up here in the conversion Phase, where we have still a string ioncluded
            {}

            switch (audioPlayer)
            {
              case AudioPlayer.Bass:
              case AudioPlayer.Asio:
              case AudioPlayer.WasApi:
                _isDefaultMusicPlayer = true;
                break;

              default:
                _isDefaultMusicPlayer = false;
                break;
            }
            _settingsLoaded = true;
          }
        }

        return _isDefaultMusicPlayer;
      }
    }

    /// <summary>
    /// Is the BASS Engine Freed?
    /// </summary>
    public static bool BassFreed
    {
      get { return _player == null || _player.BassFreed; }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Create the BASS Audio Engine Objects
    /// </summary>
    public static void CreatePlayerAsync()
    {
      if (_player != null)
      {
        return;
      }

      ThreadStart ts = new ThreadStart(InternalCreatePlayerAsync);
      _bassAsyncLoadThread = new Thread(ts);
      _bassAsyncLoadThread.Name = "BassAudio";
      _bassAsyncLoadThread.Start();
    }

    /// <summary>
    /// Frees, the BASS Audio Engine.
    /// </summary>
    public static void FreeBass()
    {
      lock (_syncRoot)
      {
        if (_player == null)
        {
          return;
        }

        _player.FreeBass();
      }
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
      if (_player == null)
      {
        try
        {
          _player = new BassAudioEngine();
        }
        catch (Exception ex)
        {
          Log.Error("BASS: InternalCreatePlayerAsync failed {0}", ex);
        }
      }
    }

    #endregion
  }

}
