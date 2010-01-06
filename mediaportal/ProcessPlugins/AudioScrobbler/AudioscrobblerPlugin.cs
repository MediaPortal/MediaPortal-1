#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using MediaPortal.AudioScrobbler;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Profile;
using Timer = System.Timers.Timer;

namespace MediaPortal.Audioscrobbler
{
  [PluginIcons("ProcessPlugins.Audioscrobbler.Audioscrobbler.gif",
    "ProcessPlugins.Audioscrobbler.AudioscrobblerDisabled.gif")]
  public class AudioscrobblerPlugin : ISetupForm, IPlugin
  {
    // maybe increase after introduction of x-fading
    private const int STARTED_LATE = 15;

    // songs longer or shorter than this won't be submitted
    private const int MIN_DURATION = 30;
    private const int MAX_DURATION = 86400; // 24h

    private const int INFINITE_TIME = Int32.MaxValue;
    // when to submbit the current song
    private int _alertTime;
    // check for skipping
    private int _lastPosition = 0;
    // initial offset to compensate for a late detection
    private TimeSpan _playingSecs = new TimeSpan(0, 0, 1);

    public bool _doSubmit = true;
    public bool _announceNowPlaying = true;

    private Timer SongLengthTimer;

    #region Events

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
      try
      {
        if (type == g_Player.MediaType.Music || Util.Utils.IsLastFMStream(filename))
        {
          Thread stateThread = new Thread(new ParameterizedThreadStart(PlaybackStartedThread));
          stateThread.IsBackground = true;
          stateThread.Name = "Scrobbler event";
          stateThread.Start((object)filename);

          Thread LoadThread = new Thread(new ThreadStart(OnSongLoadedThread));
          LoadThread.IsBackground = true;
          LoadThread.Name = "Scrobbler loader";
          LoadThread.Start();
        }
      }
      catch (Exception ex)
      {
        Log.Error("Audioscrobbler plugin: Error creating threads on playback start - {0}", ex.Message);
      }
    }

    private void OnSongLoadedThread()
    {
      int i = 0;
      try
      {
        for (i = 0; i < 15; i++)
        {
          if (AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Init)
          {
            break;
          }
          Thread.Sleep(1000);
        }
        Log.Debug("Audioscrobbler plugin: Waited {0} seconds for reinit of submit track", i);

        for (i = 0; i < 15; i++)
        {
          if (AudioscrobblerBase.CurrentPlayingSong.AudioScrobblerStatus == SongStatus.Loaded)
          {
            break;
          }
          Thread.Sleep(1000);
        }
        Log.Debug("Audioscrobbler plugin: Waited {0} seconds for lookup of current track", i);

        if (AudioscrobblerBase.CurrentPlayingSong.Artist != String.Empty)
        {
          // Don't hand over the reference        
          AudioscrobblerBase.CurrentSubmitSong = AudioscrobblerBase.CurrentPlayingSong.Clone();
          Log.Info("Audioscrobbler plugin: Song loading thread sets submit song - {0}",
                   AudioscrobblerBase.CurrentSubmitSong.ToLastFMMatchString(true));
        }
        else
        {
          Log.Debug("Audioscrobbler plugin: Song loading thread could not set the current for submit - {0}",
                    AudioscrobblerBase.CurrentPlayingSong.ToLastFMMatchString(true));
        }
      }
      catch (Exception ex)
      {
        Log.Error("Audioscrobbler plugin: Error in song load thread {0}", ex.Message);
      }
    }

    /// <summary>
    /// Initialize the current song and launch submit of songs which were waiting for BASS events in vain...
    /// </summary>
    /// <param name="aParam"></param>
    private void PlaybackStartedThread(object aParam)
    {
      string eventFile = aParam.ToString();
      if (Util.Utils.IsLastFMStream(eventFile))
      {
        // Last.fm immediately sets the current playing song on its own
      }
      else
      {
        AudioscrobblerBase.CurrentPlayingSong.Clear();
        AudioscrobblerBase.CurrentPlayingSong.FileName = eventFile;
        QueueLastSong();
      }

      OnStateChangedEvent();
    }

    private void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio)
      {
        SongStoppedHandler();
      }
    }

    private void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio)
      {
        SongStoppedHandler();
      }
    }

    /// <summary>
    /// Enqueues the song for submit if it is legible
    /// </summary>
    private void SongStoppedHandler()
    {
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
      {
        OnPause();
      }
    }

    /// <summary>
    /// Gets called if you skip to other tracks - mainly to set the alert time
    /// </summary>
    /// <param name="_currentSong">accepts the current playing Song reference</param>
    private void OnSongChangedEvent()
    {
      try
      {
        _alertTime = INFINITE_TIME;

        if (Util.Utils.IsLastFMStream(AudioscrobblerBase.CurrentPlayingSong.FileName))
        {
          if (!AudioscrobblerBase.IsSubmittingRadioSongs)
          {
            Log.Debug("Audioscrobbler plugin: radio submits disabled - ignore state change");
            return;
          }
        }
        else if (!_doSubmit)
        {
          Log.Debug("Audioscrobbler plugin: submits disabled - ignore state change");
          return;
        }

        // Only submit if we have reasonable info about the song
        if (AudioscrobblerBase.CurrentPlayingSong.Artist == String.Empty ||
            AudioscrobblerBase.CurrentPlayingSong.Title == String.Empty)
        {
          Log.Info("Audioscrobbler plugin: {0}", "no tags found ignoring song");
          return;
        }

        if (_announceNowPlaying)
        {
          for (int i = 0; i < 12; i++)
          {
            // try to wait for 6 seconds to give an maybe ongoing submit a chance to finish before the announce
            // as otherwise the now playing track might not show up on the website
            if (AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Init)
            {
              break;
            }
            Thread.Sleep(500);
          }
          AudioscrobblerBase.DoAnnounceNowPlaying();
        }

        _alertTime = GetAlertTime();

        if (_alertTime != INFINITE_TIME)
        {
          AudioscrobblerBase.CurrentPlayingSong.AudioScrobblerStatus = SongStatus.Loaded;
          startStopSongLengthTimer(true, _alertTime - _playingSecs.Seconds);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Audioscrobbler plugin: Error in song change event - {0}", ex.Message);
      }
    }

    /// <summary>
    /// Gets called everytime when a new song needs to be detected
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void OnStateChangedEvent()
    {
      bool songFound = false;

      try
      {
        if (g_Player.IsCDA)
        {
          songFound = GetCurrentCDASong();
          if (songFound == false)
          {
            return;
          }
        }
        else if (Util.Utils.IsLastFMStream(g_Player.Player.CurrentFile))
        {
          songFound = (g_Player.Player.CurrentFile == AudioscrobblerBase.CurrentPlayingSong.FileName);
        }
        else
          // local DB file
        {
          songFound = GetCurrentSong();
        }

        if (songFound)
        {
          //Log.Debug("Audioscrobbler plugin: found database track for: {0}", g_Player.CurrentFile);        
          SetStartTime();
          OnSongChangedEvent();
        }
          // DB lookup of song failed
        else if (g_Player.IsMusic)
        {
          if (AudioscrobblerBase.CurrentPlayingSong.Title != null &&
              AudioscrobblerBase.CurrentPlayingSong.Title != string.Empty)
          {
            Log.Info("Audioscrobbler plugin: Database does not contain track - ignoring: {0} by {1} from {2}",
                     AudioscrobblerBase.CurrentPlayingSong.Title, AudioscrobblerBase.CurrentPlayingSong.Artist,
                     AudioscrobblerBase.CurrentPlayingSong.Album);
          }
          else
          {
            Log.Info("Audioscrobbler plugin: Unknown track: {0}", g_Player.CurrentFile);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Audioscrobbler plugin: Error on state event: {0}", ex.ToString());
      }
    }

    /// <summary>
    /// Sets the start time needed by submit
    /// </summary>
    private void SetStartTime()
    {
      _playingSecs = new TimeSpan(0, 0, 1);
      try
      {
        _playingSecs = TimeSpan.FromSeconds(g_Player.CurrentPosition);
        AudioscrobblerBase.CurrentPlayingSong.DateTimePlayed = DateTime.UtcNow - _playingSecs;
        _lastPosition = Convert.ToInt32(g_Player.Player.CurrentPosition);
      }
      catch (Exception)
      {
        AudioscrobblerBase.CurrentPlayingSong.DateTimePlayed = DateTime.UtcNow;
        _lastPosition = 1;
      }
      Log.Info("Audioscrobbler plugin: Detected new track as: {0} - {1} started at: {2}",
               AudioscrobblerBase.CurrentPlayingSong.Artist, AudioscrobblerBase.CurrentPlayingSong.Title,
               AudioscrobblerBase.CurrentPlayingSong.DateTimePlayed.ToLocalTime().ToLongTimeString());
    }

    /// <summary>
    /// Temporarily halts the submit timer as the protocol is based on the total time actively listened to that track
    /// </summary>
    private void OnPause()
    {
      try
      {
        // avoid false skip detection
        if (g_Player.Playing && g_Player.CurrentPosition > 0)
        {
          _lastPosition = Convert.ToInt32(g_Player.CurrentPosition);
          if (AudioscrobblerBase.CurrentSubmitSong != null &&
              AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Loaded)
          {
            if (g_Player.Paused)
            {
              startStopSongLengthTimer(false, _alertTime - _playingSecs.Seconds);
              Log.Info("Audioscrobbler plugin: {0}", "Track paused");
            }
            else
            {
              startStopSongLengthTimer(true, _alertTime - (_lastPosition + _playingSecs.Seconds));
              Log.Info("Audioscrobbler plugin: {0}", "Continue track - adjust already listened time");
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Audioscrobbler plugin: Error pausing submit timer {0}", ex.Message);
      }
    }

    /// <summary>
    /// The current song has been listened to long enough - mark it for submit
    /// </summary>
    /// <param name="trash_"></param>
    /// <param name="args_"></param>
    public void OnLengthTickEvent(object trash_, ElapsedEventArgs args_)
    {
      if (AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Loaded)
      {
        AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus = SongStatus.Cached;
        Log.Info("Audioscrobbler plugin: Cached song for submit: {0}",
                 AudioscrobblerBase.CurrentSubmitSong.ToShortString());
      }
      else
      {
        Log.Debug("Audioscrobbler plugin: NOT caching song: {0} because status is {1}",
                  AudioscrobblerBase.CurrentSubmitSong.ToShortString(),
                  AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus.ToString());
      }
    }

    /// <summary>
    /// Launch the actual submit
    /// </summary>
    private void QueueLastSong()
    {
      // Submit marked tracks and those which have an action attached
      if ((AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Cached) ||
          (AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Loaded &&
           AudioscrobblerBase.CurrentSubmitSong.AudioscrobblerAction != SongAction.N))
      {
        AudioscrobblerBase.pushQueue(AudioscrobblerBase.CurrentSubmitSong);
        AudioscrobblerBase.CurrentSubmitSong.Clear();

        //if (SubmitQueued != null)
        //  SubmitQueued();
      }
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
        {
          SongLengthTimer.Close();
        }
        else
        {
          SongLengthTimer = new Timer();
          SongLengthTimer.AutoReset = false;
          SongLengthTimer.Interval = INFINITE_TIME;
          SongLengthTimer.Elapsed += new ElapsedEventHandler(OnLengthTickEvent);
        }

        if (startNow)
        {
          Log.Info("Audioscrobbler plugin: Starting song length timer with an interval of {0} seconds",
                   intervalLength.ToString());
          SongLengthTimer.Interval = intervalLength * 1000;
          SongLengthTimer.Start();
        }
        else
        {
          SongLengthTimer.Stop();
        }
      }
      catch (Exception tex)
      {
        Log.Error("Audioscrobbler plugin: Issue with song length timer - start: {0} interval: {1} error: {2}",
                  startNow.ToString(), intervalLength.ToString(), tex.Message);
      }
    }

    /// <summary>
    /// Does a lookup by filename on the local music databae
    /// </summary>
    /// <returns>Whether a song could be found</returns>
    private bool GetCurrentSong()
    {
      try
      {
        MusicDatabase dbs = MusicDatabase.Instance;
        string strFile = g_Player.Player.CurrentFile;
        Song lookupSong = new Song();
        if (strFile == string.Empty)
        {
          return false;
        }
        else
        {
          if (dbs.GetSongByFileName(strFile, ref lookupSong))
          {
            AudioscrobblerBase.CurrentPlayingSong = lookupSong;
            return true;
          }
          else
          {
            return false;
          }
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>
    /// Reads the GUI properties to determine the CD's details
    /// </summary>
    /// <returns>Whether a song could be found</returns>
    private bool GetCurrentCDASong()
    {
      bool found = false;
      try
      {
        if (g_Player.CurrentFile.IndexOf("Track") > 0 && g_Player.CurrentFile.IndexOf(".cda") > 0)
        {
          AudioscrobblerBase.CurrentPlayingSong.Artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
          AudioscrobblerBase.CurrentPlayingSong.Title = GUIPropertyManager.GetProperty("#Play.Current.Title");
          AudioscrobblerBase.CurrentPlayingSong.Album = GUIPropertyManager.GetProperty("#Play.Current.Album");
          //AudioscrobblerBase.CurrentPlayingSong.Track = Int32.Parse(GUIPropertyManager.GetProperty("#Play.Current.Track"), System.Globalization.NumberStyles.Integer, new System.Globalization.CultureInfo("en-US"));              
          AudioscrobblerBase.CurrentPlayingSong.Duration = Convert.ToInt32(g_Player.Duration);
          AudioscrobblerBase.CurrentPlayingSong.Genre = GUIPropertyManager.GetProperty("#Play.Current.Genre");
          AudioscrobblerBase.CurrentPlayingSong.FileName = g_Player.CurrentFile;

          found = AudioscrobblerBase.CurrentPlayingSong.Artist != String.Empty ? true : false;
        }
      }
      catch (Exception ex)
      {
        Log.Error("Audioscrobbler plugin: Error getting CDDA track - {0}", ex.Message);
      }
      return found;
    }

    /// <summary>
    /// Logic about when we should submit a song to Audioscrobbler - http://www.audioscrobbler.net/wiki/Protocol1.2
    /// </summary>
    /// <returns>Time in seconds until the song has been listenend to long enough</returns> 
    private int GetAlertTime()
    {
      if (AudioscrobblerBase.CurrentPlayingSong.Duration > MAX_DURATION)
      {
        Log.Info("Audioscrobbler plugin: Ignoring long song {0}", AudioscrobblerBase.CurrentPlayingSong.ToShortString());
        return INFINITE_TIME;
      }
      else if (AudioscrobblerBase.CurrentPlayingSong.Duration < MIN_DURATION)
      {
        Log.Info("Audioscrobbler plugin: Ignoring short song {0}", AudioscrobblerBase.CurrentPlayingSong.ToShortString());
        return INFINITE_TIME;
      }
      // If the duration is less then 480 secs, alert when the song
      // is half over, otherwise after 240 seconds.
      if (AudioscrobblerBase.CurrentPlayingSong.Duration < 480)
      {
        return AudioscrobblerBase.CurrentPlayingSong.Duration / 2;
      }
      else
      {
        return 240;
      }
    }

    #endregion

    #region IPlugin Members

    /// <summary>
    /// Load settings and register player events
    /// </summary>
    public void Start()
    {
      string currentUser = String.Empty;
      _alertTime = INFINITE_TIME;

      GUIWindowManager.OnNewAction += new OnActionHandler(OnNewAction);
      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);

      using (Settings xmlreader = new MPSettings())
      {
        currentUser = xmlreader.GetValueAsString("audioscrobbler", "user", String.Empty);
        _announceNowPlaying = xmlreader.GetValueAsBool("audioscrobbler", "EnableNowPlaying", true);
      }

      MusicDatabase mdb = MusicDatabase.Instance;
      _doSubmit = (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(currentUser)), "iSubmitOn", -1) == 1)
                    ? true
                    : false;

      Log.Info("Audioscrobbler plugin: Submit songs: {0}, announce Now Playing: {1}", Convert.ToString(_doSubmit),
               Convert.ToString(_announceNowPlaying));

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
      return
        "The Audioscrobbler plugin populates your profile on http://www.last.fm \nand automatically fills your playlist with songs you'll like.";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
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
      Form assetup = new AudioscrobblerSettings();
      assetup.ShowDialog();
    }

    #endregion
  }
}