#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Utils.Services;
using MediaPortal.Player;

namespace MediaPortal.Audioscrobbler 
{
  public class AudioscrobblerPlugin : ISetupForm, IPlugin
  {
    private int _timerTickSecs = 15;
    private int skipThreshold = 2;
    // maybe increase after introduction of x-fading
    private const int STARTED_LATE = 5;
  
    // songs longer or shorter than this won't be submitted
    private const int MIN_DURATION = 30;
    private const int MAX_DURATION = 2700;

    private const int INFINITE_TIME = Int32.MaxValue;

    // how many events to store in the history textfield
    private const int MAX_HISTORY_LINES = 250;

    private Song currentSong;
    // whether the current song has been submitted    
    private bool queued; 
    // when to submbit the current song
    private int alertTime;
    // check for skipping
    private int lastPosition = 0;
    
    public bool _doSubmit = true;

    private AudioscrobblerBase scrobbler;
    private System.Timers.Timer SongCheckTimer;

    #region Properties
    /* The number of seconds at which the current song will be queued */
    public int AlertTime
    {
      get {
        return alertTime;
      }
    }

    /* Whether the current song has been added to the queue */
    public bool Queued
    {
      get {
        return queued;
      }
    }

    public AudioscrobblerBase Audioscrobbler
    {
      get {
        return scrobbler;
      }
    }

    private void OnManualDisconnect(object sender, EventArgs args)
    {
      scrobbler.Disconnect();
    }

    private void OnManualConnect(object sender, EventArgs args)
    {
      scrobbler.Connect();
    }

    private void OnManualClearCache(object sender, EventArgs args)
    {
      scrobbler.ClearQueue();
      //CacheSizeLabel.Text = scrobbler.QueueLength.ToString();
      AddToHistory("Cache cleared", null);
    }
    
    //private void OnNameChangedEvent(string ASUsername)
    //{
    //  scrobbler.Username = ASUsername;
    //}

    //private void OnPassChangedEvent(string ASPassword)
    //{
    //  scrobbler.Password = ASPassword;
    //}

    private void OnEnabledChangedEvent(bool isEnabled)
    {
      if (isEnabled)
        OnManualConnect(null, null);
      else {
        scrobbler.Disconnect();
      }
    }
    #endregion

    #region Audioscrobbler events
    private void OnAuthErrorEvent(AuthErrorEventArgs args)
    {
      string report = "Audioscrobbler did not recognize your username/password."
                    + " Please check your details (File/Audioscrobbler...)";
      MessageBox.Show(report);
      OnManualDisconnect(null, null);
    }

    private void OnNetworkErrorEvent(NetworkErrorEventArgs args)
    {
      //StatusBar.Pop(StatusID);
      //StatusBar.Push(StatusID, args.Details);
      OnManualDisconnect(null, null);
    }
    
    private void OnSubmitEvent (SubmitEventArgs args)
    {
      string title = args.song.Artist + " - " + args.song.Title;

      //StatusBar.Push(StatusID, "Submitted: " + title);

      // Consider the song to be submitted if a connection was made -
      // AS can't reject individual songs.
      AddToHistory("Submitted", args.song);
    }
  
    private void OnConnectEvent(ConnectEventArgs args)
    {
      //StatusBar.Pop(StatusID);
      //StatusBar.Push(StatusID, "Connected.");
    }

    private void OnDisconnectEvent(DisconnectEventArgs args)
    {
      //StatusBar.Pop(StatusID);
      //StatusBar.Push(StatusID, "Disconnected.");
    }
    #endregion

    #region MediaPortal events
    //public void OnThreadMessage(GUIMessage message)
    //{
    //  switch (message.Message)
    //  {
    //    case GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC:

    //      string strFile = message.Label;
    //      if (MediaPortal.Util.Utils.IsAudio(strFile))
    //      {
    //        // no longer needed
    //      }
    //      break;
    //  }
    //  return;
    //}

    // Make sure we get all of the ACTION_PLAY events (OnAction only receives the ACTION_PLAY event when 
    // the player is not playing)...     
    void OnNewAction(Action action)
    {
      if ((action.wID == Action.ActionType.ACTION_PLAY || action.wID == Action.ActionType.ACTION_MUSIC_PLAY || action.wID == Action.ActionType.ACTION_PAUSE))
      {
        // finalize stuff before new song starts?
        OnStateChangedEvent(true);
      }
      if (action.wID == Action.ActionType.ACTION_STOP)
        OnStateChangedEvent(false);
      if (action.wID == Action.ActionType.ACTION_NEXT_ITEM || action.wID == Action.ActionType.ACTION_PREV_ITEM)
      {
        OnStateChangedEvent(true);
      }
      //// if music is started via "OK / Enter"
      //if (action.wID == Action.ActionType.ACTION_SELECT_ITEM)
      //  if (g_Player.Playing)
      //    if (g_Player.IsMusic)
      //      OnStateChangedEvent(true);
    }


    /// <summary>
    /// Gets called if you skip to other tracks - mainly to set the alert time
    /// </summary>
    /// <param name="currentSong">accepts the current playing Song reference</param>
    public void OnSongChangedEvent(Song currentSong)
    {
      queued = false;
      alertTime   = INFINITE_TIME;

      if (!_doSubmit || currentSong == null)
        return;      

      // Only submit if we have reasonable info about the song
      if (currentSong.Artist == "" || currentSong.Title == "")
        return;

      // Don't queue if the song didn't start at 0
      if (Convert.ToInt32(g_Player.Player.CurrentPosition) <= (STARTED_LATE + _timerTickSecs))
      {
        alertTime = GetAlertTime();
        currentSong.AudioScrobblerStatus = SongStatus.Loaded;
        return;
      }
    }
    
    /// <summary>
    /// Gets called everytime the playback status of MyMusic changes.
    /// </summary>
    /// <param name="playing">on true it does a Song lookup for new Tracks if necessary</param>
    public void OnStateChangedEvent(bool playing)
    {
      if (playing)
      {
        if (currentSong == null)
          currentSong = new Song();
        // Track has changed
        if (g_Player.CurrentFile != currentSong.FileName)
        {
          MusicDatabase dbs = new MusicDatabase();
          string strFile = g_Player.Player.CurrentFile;
          bool songFound = dbs.GetSongByFileName(strFile, ref currentSong);
          if (songFound)
          {
            currentSong.AudioScrobblerStatus = SongStatus.Init;
            currentSong.DateTimePlayed = DateTime.UtcNow;
            // avoid false skip detection
            lastPosition = Convert.ToInt32(g_Player.Player.CurrentPosition);
            OnSongChangedEvent(currentSong);
          }
        }
        else // Track was paused
        {
          // avoid false skip detection
          lastPosition = Convert.ToInt32(g_Player.Player.CurrentPosition);
        }
      }
    }

    public void OnTickEvent(object trash_, ElapsedEventArgs args_)
    {
      if (!_doSubmit)
        return;
      int position = 0;
      if (g_Player.Playing)
      {
        position = Convert.ToInt32(g_Player.Player.CurrentPosition);
        // attempt to detect skipping
        if (currentSong != null)
        {
          // manually chose song with "SELECT" action
          if (g_Player.CurrentFile != currentSong.FileName)
          {
            OnStateChangedEvent(true);
            return;
          }

          if (!queued)
          {
            if (alertTime < INFINITE_TIME && position > (lastPosition + skipThreshold + _timerTickSecs))
            {
              //        _log.Info("Audioscrobbler: OnTickEvent {0}", "Skipping detected from " + lastPosition + " to " + position + ") - not queueing");
              alertTime = INFINITE_TIME;
              AddToHistory("Ignored (skipping)", currentSong);
            }

            // then actually queue the song if we're that far along
            if (position >= alertTime && alertTime > 14)
            {
              scrobbler.pushQueue(currentSong);
              queued = true;
              currentSong.AudioScrobblerStatus = SongStatus.Cached;
            }

            /* We don't set lastPosition back to 0 in SongChanged.  
               This avoids problems where songs start late.  It might also 
               mean the user can somehow start a song late and skipping won't 
               be detected
            */
            lastPosition = position;
          }
        }
        else // Playing but no Song? Action missed!
          OnStateChangedEvent(true);
      }
    }
    #endregion

    #region Utilities
    private void startStopSongCheckTimer(bool startNow)
    {
      if (SongCheckTimer == null)
        SongCheckTimer = new System.Timers.Timer();
      if (startNow)
      {
        SongCheckTimer.Interval = _timerTickSecs * 1000;
        SongCheckTimer.Elapsed += new ElapsedEventHandler(OnTickEvent);
        SongCheckTimer.Start();
      }
      else
        SongCheckTimer.Stop();
    }

    // Logic about when we should submit a song to Audioscrobbler - http://www.audioscrobbler.net/wiki/Protocol1.1   
    private int GetAlertTime()
    {
      if (currentSong.Duration > MAX_DURATION) {
        //SubmitTimeLabel.Text = "Current song is too long. Not submitting.";
        AddToHistory("Ignored (too long)", currentSong);
        return INFINITE_TIME;
      }
      else if (currentSong.Duration < MIN_DURATION) {
        //SubmitTimeLabel.Text = "Current song is too short. Not submitting.";
        AddToHistory("Ignored (too short)", currentSong);
        return INFINITE_TIME;
      }

      // If the duration is less then 480 secs, alert when the song
      // is half over, otherwise after 240 seconds.
      if (currentSong.Duration < 480)
        return currentSong.Duration / 2;
      else
        return 240;

      //return INFINITE_TIME;
    }

    // Add a message to the submissions history text area
    private void AddToHistory(string desc, Song song)
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      ILog _log = services.Get<ILog>();
      if (desc != "")
      {
        _log.Info("Audioscrobbler: desc: {0}", desc);
      }
    
      if (song != null)
        _log.Info("Audioscrobbler: song: {0}", song.ToShortString());
    }
    #endregion

    #region IPlugin Members

    public void Start()
    {
      currentSong = null;
      queued = false;
      alertTime = INFINITE_TIME;
      scrobbler = new AudioscrobblerBase();
      GUIWindowManager.OnNewAction += new OnActionHandler(OnNewAction);
      //GUIWindowManager.Receivers += new SendMessageHandler(OnThreadMessage);
      startStopSongCheckTimer(true);
      // connect to Audioscrobbler (in a new thread)
      if (_doSubmit)
      {
        OnManualConnect(null, null);
      }
    }

    public void Stop()
    {
      OnManualDisconnect(null, null);
      startStopSongCheckTimer(false);
      scrobbler = null;
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      // This offers the possibility to get a radio stream based on your own music taste and "smart playlists"
      return "The Audioscrobbler plugin populates your profile on http://www.last.fm";
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
