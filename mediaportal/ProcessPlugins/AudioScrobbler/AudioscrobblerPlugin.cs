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
using System.Timers;
using System.Windows.Forms;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;

namespace MediaPortal.Audioscrobbler
{
  [PluginIcons("ProcessPlugins.Audioscrobbler.Audioscrobbler.gif", "ProcessPlugins.Audioscrobbler.AudioscrobblerDisabled.gif")]
  public class AudioscrobblerPlugin : ISetupForm, IPlugin
  {
    //private int _timerTickSecs = 15;
    //private int _skipThreshold = 6;
    // maybe increase after introduction of x-fading
    private const int STARTED_LATE = 15;

    // songs longer or shorter than this won't be submitted
    private const int MIN_DURATION = 30;
    private const int MAX_DURATION = 86400; // 24h

    private const int INFINITE_TIME = Int32.MaxValue;

    private Song _currentSong;
    // whether the current song has been submitted    
    private bool queued;
    // when to submbit the current song
    private int _alertTime;
    // check for skipping
    private int _lastPosition = 0;

    public bool _doSubmit = true;
    public bool _announceNowPlaying = true;

    private System.Timers.Timer SongLengthTimer;


    #region Properties
    /* The number of seconds at which the current song will be queued */
    public int AlertTime
    {
      get
      {
        return _alertTime;
      }
    }

    /* Whether the current song has been added to the queue */
    public bool Queued
    {
      get
      {
        return queued;
      }
    }

    private void OnManualDisconnect(object sender, EventArgs args)
    {
      AudioscrobblerBase.Disconnect();
    }

    private void OnManualConnect(object sender, EventArgs args)
    {
      if (!AudioscrobblerBase.Connected)
        AudioscrobblerBase.Connect();
    }

    //private void OnEnabledChangedEvent(bool isEnabled)
    //{
    //  if (isEnabled)
    //    OnManualConnect(null, null);
    //  else
    //  {
    //    AudioscrobblerBase.Disconnect();
    //  }
    //}
    #endregion

    #region MediaPortal events

    void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (type == g_Player.MediaType.Music || Util.Utils.IsLastFMStream(filename))
      {
        if (_currentSong == null)
        {
          _currentSong = new Song();
          _currentSong.AudioScrobblerStatus = SongStatus.Init;
        }
        else
        {
          if (filename != _currentSong.FileName)
            if (_currentSong.AudioScrobblerStatus == SongStatus.Cached)
              QueueLastSong();
            else
              // do not log twice (OnEnded & OnStarted)
              if (_currentSong.AudioScrobblerStatus != SongStatus.Queued && !Util.Utils.IsLastFMStream(g_Player.Player.CurrentFile))
                Log.Debug("Audioscrobbler plugin: OnPlayBackStarted - NOT submitting song {0} because status was: {1}", _currentSong.ToShortString(), _currentSong.AudioScrobblerStatus.ToString());
        }

        OnStateChangedEvent();
      }
      //else
      //  Log.Debug("Audioscrobbler plugin: no music playing - ignore media type of {0}", filename);

    }

    void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio)
        SongStoppedHandler();
    }

    void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio)
        SongStoppedHandler();
    }

    void SongStoppedHandler()
    {
      if (_currentSong != null)
      {
        if (_currentSong.AudioScrobblerStatus == SongStatus.Cached)
        {
          // The user could have changed the rating, etc since retrieving the song
          _currentSong.Source = AudioscrobblerBase.CurrentSong.Source;
          _currentSong.AuthToken = AudioscrobblerBase.CurrentSong.AuthToken;
          _currentSong.AudioscrobblerAction = AudioscrobblerBase.CurrentSong.AudioscrobblerAction;
          QueueLastSong();
        }
        else
          // do not log twice (OnEnded & OnStopped)
          if (_currentSong.AudioScrobblerStatus != SongStatus.Queued)
            Log.Debug("Audioscrobbler plugin: OnPlayBackEnded - NOT submitting song {0} because status was: {1}", _currentSong.ToShortString(), _currentSong.AudioScrobblerStatus.ToString());
      }
      startStopSongLengthTimer(false, INFINITE_TIME);
    }

    // Make sure we get all of the ACTION_PLAY events (OnAction only receives the ACTION_PLAY event when 
    // the player is not playing)...     
    void OnNewAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PAUSE && g_Player.IsMusic)
        OnPause();

      //if (action.wID == Action.ActionType.ACTION_NEXT_ITEM || action.wID == Action.ActionType.ACTION_PREV_ITEM)
      //{
      //  OnStateChangedEvent(true);
      //}
    }

    /// <summary>
    /// Gets called if you skip to other tracks - mainly to set the alert time
    /// </summary>
    /// <param name="_currentSong">accepts the current playing Song reference</param>
    private void OnSongChangedEvent(Song currentSong)
    {
      queued = false;
      _alertTime = INFINITE_TIME;

      if (currentSong == null)
      {
        Log.Error("Audioscrobbler plugin: currentSong == null - ignore state change");
        return;
      }

      if (Util.Utils.IsLastFMStream(currentSong.FileName))
      {
        if (!AudioscrobblerBase.SubmitRadioSongs)
        {
          Log.Debug("Audioscrobbler plugin: radio submits disabled - ignore state change");
          return;
        }
      }
      else
        if (!_doSubmit)
        {
          Log.Debug("Audioscrobbler plugin: submits disabled - ignore state change");
          return;
        }

      // Only submit if we have reasonable info about the song
      if (currentSong.Artist == String.Empty || currentSong.Title == String.Empty)
      {
        Log.Info("Audioscrobbler plugin: {0}", "no tags found ignoring song");
        return;
      }

      AudioscrobblerBase.CurrentSong = currentSong;
      if (_announceNowPlaying)
        AudioscrobblerBase.AnnounceNowPlaying();

      _alertTime = GetAlertTime();

      if (_alertTime != INFINITE_TIME)
      {
        _currentSong.AudioScrobblerStatus = SongStatus.Loaded;
        startStopSongLengthTimer(true, _alertTime);
      }
    }

    /// <summary>
    /// Gets called everytime the playback status of MyMusic changes.
    /// </summary>
    /// <param name="playing">on true it does a Song lookup for new Tracks if necessary</param>
    private void OnStateChangedEvent()
    {
      bool songFound = false;

      if (g_Player.IsCDA)
      {
        songFound = GetCurrentCDASong();
        if (songFound == false)
          return;
      }
      else
        if (Util.Utils.IsLastFMStream(g_Player.Player.CurrentFile))
        {
          // Wait up to 15 seconds for the NowPlayingParser to get a response from last.fm
          for (int i = 0; i < 30; i++)
          {
            _currentSong = AudioscrobblerBase.CurrentSong.Clone();

            if (_currentSong.FileName == g_Player.Player.CurrentFile)
            {
              songFound = true;
              Log.Info("Audioscrobbler plugin: detected new last.fm radio track as: {0} - {1} after {2} seconds", _currentSong.Artist, _currentSong.Title, Convert.ToString(i / 2));
              break;
            }
            System.Threading.Thread.Sleep(500);
          }
          if (songFound)
          {
            _currentSong.DateTimePlayed = DateTime.UtcNow;
            _lastPosition = 1;
            OnSongChangedEvent(_currentSong);
          }
          else
            Log.Info("Audioscrobbler plugin: could not determine last.fm radio track for: {0}", g_Player.CurrentFile);
          return;
        }
        else
        // local DB file
        {
          if (_currentSong.Source == SongSource.U)
            _currentSong.Source = SongSource.P;
          songFound = GetCurrentSong();
        }

      if (songFound)
      {
        //Log.Debug("Audioscrobbler plugin: found database track for: {0}", g_Player.CurrentFile);
        // playback couuuuld be stopped in theory - sometimes g_player's IsPlaying status isn't set in time (e.g. crossfading)
        if (g_Player.CurrentPosition > 0)
        {
          _currentSong.DateTimePlayed = DateTime.UtcNow - TimeSpan.FromSeconds(g_Player.CurrentPosition);
          // avoid false skip detection            
          _lastPosition = Convert.ToInt32(g_Player.Player.CurrentPosition);
          OnSongChangedEvent(_currentSong);
        }
        else
        {
          _currentSong.DateTimePlayed = DateTime.UtcNow;
          _lastPosition = 1;
          OnSongChangedEvent(_currentSong);
        }
      }
      // DB lookup of song failed
      else
        if (g_Player.IsMusic)
        {
          if (_currentSong.Title != null && _currentSong.Title != string.Empty)
            Log.Info("Audioscrobbler plugin: database does not contain track - ignoring track: {0} by {1} from {2}", _currentSong.Title, _currentSong.Artist, _currentSong.Album);
          else
            Log.Info("Audioscrobbler plugin: database does not contain track: {0}", g_Player.CurrentFile);
        }
    }

    private void OnPause()
    {
      // avoid false skip detection
      if (g_Player.Playing && g_Player.CurrentPosition > 0)
      {
        _lastPosition = Convert.ToInt32(g_Player.CurrentPosition);
        if (_currentSong != null && _currentSong.AudioScrobblerStatus == SongStatus.Loaded)
          if (g_Player.Paused)
          {
            startStopSongLengthTimer(false, _alertTime);
            Log.Info("Audioscrobbler plugin: {0}", "track paused");
          }
          else
          {
            startStopSongLengthTimer(true, _alertTime - _lastPosition);
            Log.Info("Audioscrobbler plugin: {0}", "continue track - adjust already listened time");
          }
      }
    }

    public void OnLengthTickEvent(object trash_, ElapsedEventArgs args_)
    {
      if (_currentSong.AudioScrobblerStatus == SongStatus.Loaded)
      {
        _currentSong.AudioScrobblerStatus = SongStatus.Cached;
        Log.Info("Audioscrobbler plugin: cached song for submit: {0}", _currentSong.ToShortString());
      }
      else
        Log.Info("Audioscrobbler plugin: NOT caching song: {0} because status is {1}", _currentSong.ToShortString(), _currentSong.AudioScrobblerStatus.ToString());
    }

    private void QueueLastSong()
    {
      AudioscrobblerBase.pushQueue(_currentSong);
      _currentSong.AudioScrobblerStatus = SongStatus.Queued;
      queued = true;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Starts and stops the timer to check how long a track has been played
    /// </summary>
    /// <param name="startNow">should the timer start or stop</param>
    /// <returns>Elapsed timer seconds</returns>
    private void startStopSongLengthTimer(bool startNow, int intervalLength)
    {
      try
      {
        if (SongLengthTimer != null)
          SongLengthTimer.Close();
        else
        {
          SongLengthTimer = new System.Timers.Timer();
          SongLengthTimer.AutoReset = false;
          SongLengthTimer.Interval = INFINITE_TIME;
          SongLengthTimer.Elapsed += new ElapsedEventHandler(OnLengthTickEvent);
        }

        if (startNow)
        {
          Log.Info("Audioscrobbler plugin: starting song length timer with an interval of {0} seconds", intervalLength.ToString());
          SongLengthTimer.Interval = intervalLength * 1000;
          SongLengthTimer.Start();
        }
        else
          SongLengthTimer.Stop();
      }
      catch (Exception tex)
      {
        Log.Error("Audioscrobbler plugin: Issue with song length timer - start: {0} interval: {1} error: {2}", startNow.ToString(), intervalLength.ToString(), tex.Message);
      }
    }

    private bool GetCurrentSong()
    {
      MusicDatabase dbs = MusicDatabase.Instance;
      string strFile = g_Player.Player.CurrentFile;
      if (strFile != string.Empty)
        return dbs.GetSongByFileName(strFile, ref _currentSong);
      else
        return false;
    }

    private bool GetCurrentCDASong()
    {
      bool found = false;
      if (g_Player.CurrentFile.IndexOf("Track") > 0 && g_Player.CurrentFile.IndexOf(".cda") > 0)
      {
        if (_currentSong != null)
        {
          _currentSong.Artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
          _currentSong.Title = GUIPropertyManager.GetProperty("#Play.Current.Title");
          _currentSong.Album = GUIPropertyManager.GetProperty("#Play.Current.Album");
          //_currentSong.Track = Int32.Parse(GUIPropertyManager.GetProperty("#Play.Current.Track"), System.Globalization.NumberStyles.Integer, new System.Globalization.CultureInfo("en-US"));              
          _currentSong.Duration = Convert.ToInt32(g_Player.Duration);
          _currentSong.Genre = GUIPropertyManager.GetProperty("#Play.Current.Genre");
          _currentSong.FileName = g_Player.CurrentFile;

          found = _currentSong.Artist != String.Empty ? true : false;
        }
      }
      return found;
    }

    // Logic about when we should submit a song to Audioscrobbler - http://www.audioscrobbler.net/wiki/Protocol1.1   
    private int GetAlertTime()
    {
      if (_currentSong.Duration > MAX_DURATION)
      {
        Log.Info("Audioscrobbler plugin: ignoring long song {0}", _currentSong.ToShortString());
        return INFINITE_TIME;
      }
      else if (_currentSong.Duration < MIN_DURATION)
      {
        Log.Info("Audioscrobbler plugin: ignoring short song {0}", _currentSong.ToShortString());
        return INFINITE_TIME;
      }
      // If the duration is less then 480 secs, alert when the song
      // is half over, otherwise after 240 seconds.
      if (_currentSong.Duration < 480)
        return _currentSong.Duration / 2;
      else
        return 240;
    }

    #endregion

    #region IPlugin Members

    public void Start()
    {
      string currentUser = String.Empty;

      _currentSong = null;
      queued = false;
      _alertTime = INFINITE_TIME;

      GUIWindowManager.OnNewAction += new OnActionHandler(OnNewAction);
      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);

      // startStopSongCheckTimer(true);

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        currentUser = xmlreader.GetValueAsString("audioscrobbler", "user", String.Empty);
        _announceNowPlaying = xmlreader.GetValueAsBool("audioscrobbler", "EnableNowPlaying", true);
      }

      MusicDatabase mdb = MusicDatabase.Instance;
      _doSubmit = (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(currentUser)), "iSubmitOn", -1) == 1) ? true : false;

      Log.Info("Audioscrobbler plugin: Submit songs: {0}, announce Now Playing: {1}", Convert.ToString(_doSubmit), Convert.ToString(_announceNowPlaying));

      if (_doSubmit)
      {
        OnManualConnect(null, null);
      }
    }

    public void Stop()
    {
      SongStoppedHandler();

      OnManualDisconnect(null, null);
      g_Player.PlayBackStarted -= new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackEnded -= new g_Player.EndedHandler(OnPlayBackEnded);
      g_Player.PlayBackStopped -= new g_Player.StoppedHandler(OnPlayBackStopped);
      GUIWindowManager.OnNewAction -= new OnActionHandler(OnNewAction);
      // startStopSongCheckTimer(false);
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "The Audioscrobbler plugin populates your profile on http://www.last.fm \nand automatically fills your playlist with songs you'll like.";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public string Author()
    {
      return "rtv";
    }

    public string PluginName()
    {
      return "Audioscrobbler";
    }

    public bool HasSetup()
    {
      return true;
    }

    public void ShowPlugin()
    {
      Form assetup = new AudioScrobbler.AudioscrobblerSettings();
      assetup.ShowDialog();
    }

    #endregion
  }
}
