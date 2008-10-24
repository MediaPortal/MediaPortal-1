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
using System.Threading;
using System.Runtime.CompilerServices;

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
    // when to submbit the current song
    private int _alertTime;
    // check for skipping
    private int _lastPosition = 0;

    public bool _doSubmit = true;
    public bool _announceNowPlaying = true;

    private System.Timers.Timer SongLengthTimer;

    #region Properties

    ///// <summary>
    ///// The number of seconds at which the current song will be queued
    ///// </summary>
    //public int AlertTime
    //{
    //  get
    //  {
    //    return _alertTime;
    //  }
    //}

    ///// <summary>
    ///// Whether the current song has been added to the queue
    ///// </summary>
    //public bool Queued
    //{
    //  get
    //  {
    //    return queued;
    //  }
    //}

    #endregion

    #region MediaPortal events

    private void OnManualDisconnect(object sender, EventArgs args)
    {
      AudioscrobblerBase.Disconnect();
    }

    private void OnManualConnect(object sender, EventArgs args)
    {
      AudioscrobblerBase.Connect();
    }

    private void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (type == g_Player.MediaType.Music || Util.Utils.IsLastFMStream(filename))
      {
        Thread stateThread = new Thread(new ParameterizedThreadStart(PlaybackStartedThread));
        stateThread.IsBackground = true;
        stateThread.Name = "Scrobbler event";
        stateThread.Start((object)filename);
      }
    }

    private void PlaybackStartedThread(object aParam)
    {
      string eventFile = aParam.ToString();

      if (_currentSong == null)
      {
        _currentSong = new Song();
        _currentSong.AudioScrobblerStatus = SongStatus.Init;
      }
      else
      {
        if (eventFile != _currentSong.FileName)
        {
          // Don't even rely on that event to fire... Abort loop after 10 seconds
          int lameLoopCounter = 0;
          int waitMs = 250;

          // Due to bad player design or crossfading OnPlayBackStarted might be earlier then OnPlayBackEnded
          while (_currentSong.AudioScrobblerStatus == SongStatus.Cached && (lameLoopCounter * waitMs < BassMusicPlayer.Player.CrossFadeIntervalMS))
          {
            Thread.Sleep(waitMs);
            lameLoopCounter++;
            if (lameLoopCounter % 4 == 0)
              Log.Warn("Audioscrobbler plugin: OnPlayBackStarted - waiting {0} s for OnPlayBackEnded to submit track.",(int) (lameLoopCounter * waitMs / 1000));
          }

          QueueLastSong();

          //if ()
          //  QueueLastSong();
          //else
          //  // do not log twice (OnEnded & OnStarted)
          //  if (_currentSong.AudioScrobblerStatus != SongStatus.Queued && !Util.Utils.IsLastFMStream(g_Player.Player.CurrentFile))
          //    Log.Debug("Audioscrobbler plugin: OnPlayBackStarted - NOT submitting song {0} because status was: {1}", _currentSong.ToShortString(), _currentSong.AudioScrobblerStatus.ToString());
        }
      }
      OnStateChangedEvent();
    }

    private void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio)
        SongStoppedHandler();
    }

    private void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio)
        SongStoppedHandler();
    }

    /// <summary>
    /// Enqueues the song for submit if it is legible
    /// </summary>
    private void SongStoppedHandler()
    {
      if (_currentSong != null)
        QueueLastSong();

      startStopSongLengthTimer(false, INFINITE_TIME);
    }

    /// <summary>
    /// Make sure we get all of the ACTION_PLAY events (OnAction only receives the ACTION_PLAY event when the player is not playing)... 
    /// </summary>
    /// <param name="action"></param>    
    private void OnNewAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PAUSE && g_Player.IsMusic)
        OnPause();
    }

    /// <summary>
    /// Gets called if you skip to other tracks - mainly to set the alert time
    /// </summary>
    /// <param name="_currentSong">accepts the current playing Song reference</param>
    private void OnSongChangedEvent(Song currentSong)
    {
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
    [MethodImpl(MethodImplOptions.Synchronized)]
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
            SetStartTime();
            OnSongChangedEvent(_currentSong);
          }
          else
            Log.Info("Audioscrobbler plugin: could not determine last.fm radio track for: {0}", g_Player.CurrentFile);
          return;
        }
        else
        // local DB file
        {
          // If the source is unknown we set in in AudioscrobblerBase as that value is taken right before the submit.
          if (_currentSong.Source == SongSource.U)
          {
            _currentSong.Source = SongSource.P;
            AudioscrobblerBase.CurrentSong.Source = SongSource.P;
          }
          songFound = GetCurrentSong();
        }

      if (songFound)
      {
        //Log.Debug("Audioscrobbler plugin: found database track for: {0}", g_Player.CurrentFile);        
        SetStartTime();
        OnSongChangedEvent(_currentSong);
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

    /// <summary>
    /// Sets the start time needed by submit
    /// </summary>
    private void SetStartTime()
    {
      TimeSpan playingSecs = new TimeSpan(0, 0, 1);
      try
      {
        playingSecs = TimeSpan.FromSeconds(g_Player.CurrentPosition);
        _currentSong.DateTimePlayed = DateTime.UtcNow - playingSecs;
        _lastPosition = Convert.ToInt32(g_Player.Player.CurrentPosition);
      }
      catch (Exception)
      {
        _currentSong.DateTimePlayed = DateTime.UtcNow;
        _lastPosition = 1;
      }
    }

    /// <summary>
    /// Temporarily halts the submit timer as the protocol is based on the total time actively listened to that track
    /// </summary>
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

    /// <summary>
    /// The current song has been listened to long enough - mark it for submit
    /// </summary>
    /// <param name="trash_"></param>
    /// <param name="args_"></param>
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

    /// <summary>
    /// Launch the actual submit
    /// </summary>
    private void QueueLastSong()
    {
      // Submit marked tracks and those which have an action attached
      if ((_currentSong.AudioScrobblerStatus == SongStatus.Cached) ||
          (_currentSong.AudioScrobblerStatus == SongStatus.Loaded && AudioscrobblerBase.CurrentSong.AudioscrobblerAction != SongAction.N))
      {
        // The user could have changed the rating, etc since retrieving the song
        _currentSong.Source = AudioscrobblerBase.CurrentSong.Source;
        _currentSong.AuthToken = AudioscrobblerBase.CurrentSong.AuthToken;
        _currentSong.AudioscrobblerAction = AudioscrobblerBase.CurrentSong.AudioscrobblerAction;

        AudioscrobblerBase.pushQueue(_currentSong);
        _currentSong.AudioScrobblerStatus = SongStatus.Queued;
      }
      else
        // do not log twice (OnEnded & OnStopped)
        if (_currentSong.AudioScrobblerStatus != SongStatus.Queued)
          Log.Debug("Audioscrobbler plugin: OnPlayBackEnded - NOT submitting song {0} because status was: {1}", _currentSong.ToShortString(), _currentSong.AudioScrobblerStatus.ToString());
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

    /// <summary>
    /// Does a lookup by filename on the local music databae
    /// </summary>
    /// <returns>Whether a song could be found</returns>
    private bool GetCurrentSong()
    {
      MusicDatabase dbs = MusicDatabase.Instance;
      string strFile = g_Player.Player.CurrentFile;
      if (strFile != string.Empty)
        return dbs.GetSongByFileName(strFile, ref _currentSong);
      else
        return false;
    }

    /// <summary>
    /// Reads the GUI properties to determine the CD's details
    /// </summary>
    /// <returns>Whether a song could be found</returns>
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

    /// <summary>
    /// Logic about when we should submit a song to Audioscrobbler - http://www.audioscrobbler.net/wiki/Protocol1.2
    /// </summary>
    /// <returns>Time in seconds until the song has been listenend to long enough</returns> 
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

    /// <summary>
    /// Load settings and register player events
    /// </summary>
    public void Start()
    {
      string currentUser = String.Empty;

      _currentSong = null;
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

    /// <summary>
    /// Deregister all events
    /// </summary>
    public void Stop()
    {
      SongStoppedHandler();

      OnManualDisconnect(null, null);
      g_Player.PlayBackStarted -= new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackEnded -= new g_Player.EndedHandler(OnPlayBackEnded);
      g_Player.PlayBackStopped -= new g_Player.StoppedHandler(OnPlayBackStopped);
      GUIWindowManager.OnNewAction -= new OnActionHandler(OnNewAction);
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
