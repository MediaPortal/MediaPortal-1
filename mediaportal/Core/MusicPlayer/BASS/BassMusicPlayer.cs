using System;
using System.Threading;
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
          _player = new BassAudioEngine();
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
            string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "player", "BASS engine");
            _isDefaultMusicPlayer = String.Compare(strAudioPlayer, "BASS engine", true) == 0;
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
      if (_player == null)
      {
        return;
      }

      _player.FreeBass();
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
        _player = new BassAudioEngine();
      }
    }

    #endregion
  }

}
