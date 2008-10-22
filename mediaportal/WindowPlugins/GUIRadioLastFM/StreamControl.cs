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
using System.Collections.Generic;
using System.Net;
using System.Threading;

using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.TagReader;
using MediaPortal.Utils.Web;
using MediaPortal.Playlists;


namespace MediaPortal.GUI.RADIOLASTFM
{
  #region enums
  public enum PlaybackType
  {
    Unknown = 0,
    Continuously = 1,
    PlaylistPlayer = 2,
  }

  public enum StreamPlaybackState : int
  {
    offline = 0,
    initialized = 1,
    starting = 2,
    streaming = 3,
    paused = 4,
    nocontent = 5,
  }

  public enum StreamControls : int
  {
    skiptrack = 0,
    lovetrack = 1,
    bantrack = 2,
  }

  public enum StreamType : int
  {
    Artist = 0,
    Group = 1,
    Loved = 2,
    Personal = 3,
    Recommended = 4,
    Tags = 5,
    Neighbours = 6,
    Playlist = 7,
  }
  #endregion

  class StreamControl
  {
    #region Event delegates
    public delegate void SongChangedHandler(MusicTag newCurrentSong, DateTime startTime);
    public event SongChangedHandler StreamSongChanged;

    public delegate void RadioSettingsLoaded();
    public event RadioSettingsLoaded RadioSettingsSuccess;

    public delegate void RadioSettingsFailed();
    public event RadioSettingsFailed RadioSettingsError;
    #endregion

    #region Variables
    private PlayListPlayer PlaylistPlayer = null;

    /// <summary>
    /// The "filename" used by the player to access the stream
    /// </summary>
    private string _currentRadioURL = string.Empty;
    /// <summary>
    /// The user associated Session ID - from the response to the Audioscrobbler handshake
    /// </summary>
    private string _currentSession = string.Empty;
    /// <summary>
    /// The last.fm user from you configured in the Audioscrobbler plugin
    /// </summary>
    private string _currentUser = string.Empty;
    /// <summary>
    /// The last.fm user which stream will be tuned to
    /// </summary>
    private string _currentStreamsUser = string.Empty;
    /// <summary>
    /// Did you pay for exclusive member options
    /// </summary>
    private bool _isSubscriber = false;
    /// <summary>
    /// Discovery mode tries to avoid stream tracks you've already listened to
    /// </summary>
    private bool _discoveryMode = false;
    /// <summary>
    /// Settings loaded
    /// </summary>
    private bool _isInit = false;
    /// <summary>
    /// Contains all playlist relevant track informations
    /// </summary>
    private MusicTag CurrentSongTag;

    private StreamPlaybackState _currentState = StreamPlaybackState.offline;
    private StreamType _currentTuneType = StreamType.Recommended;
    private PlaybackType _currentPlaybackType = PlaybackType.Unknown;
    /// <summary>
    /// The time of the last http access
    /// </summary>
    private DateTime _lastConnectAttempt = DateTime.MinValue;
    /// <summary>
    /// Sets the minimum timespan between each http access to avoid hammering
    /// </summary>
    private TimeSpan _minConnectWaitTime = new TimeSpan(0, 0, 1);

    private AsyncGetRequest httpcommand = null;
    #endregion

    // constructor
    public StreamControl()
    {
      AudioscrobblerBase.RadioHandshakeSuccess += new AudioscrobblerBase.RadioHandshakeCompleted(OnRadioLoginSuccess);
      AudioscrobblerBase.RadioHandshakeError += new AudioscrobblerBase.RadioHandshakeFailed(OnRadioLoginFailed);

      PlaylistPlayer = PlayListPlayer.SingletonPlayer;
    }

    #region Examples
    // 4. http.request.uri = Request URI: http://ws.audioscrobbler.com/ass/upgrade.php?platform=win&version=1.0.7&lang=en&user=f1n4rf1n
    // 5. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/np.php?session=e5b0c80f5b5d0937d407fb77a913cb6a
    // 6. http.request.uri = Request URI: http://ws.audioscrobbler.com/ass/artistmetadata.php?artist=Sportfreunde%20Stiller&lang=en
    // 7. http.request.uri = Request URI: http://ws.audioscrobbler.com/ass/metadata.php?artist=Sportfreunde%20Stiller&track=Alles%20Das&album=Macht%20doch%20was%20ihr%20wollt%20-%20Ich%20geh%2527%20jetzt%2521

    // SKIP Button
    // 8. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/control.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&command=skip

    // XSPF Playlist
    // GET  ws.audioscrobbler.com/radio/xspf.php?sk=e5b0c80f5b5d0937d407fb77a913cb6a&discovery=0&desktop=1.3.1.1


    //price=
    //shopname=
    //clickthrulink=
    //streaming=true
    //discovery=0
    //station=Global Tag Radio: metal, viking metal, Melodic Death Metal
    //artist=Sonata Arctica
    //artist_url=http://www.last.fm/music/Sonata+Arctica
    //track=8th Commandment
    //track_url=http://www.last.fm/music/Sonata+Arctica/_/8th+Commandment
    //album=Ecliptica
    //album_url=http://www.last.fm/music/Sonata+Arctica/Ecliptica
    //albumcover_small=http://images.amazon.com/images/P/B00004T40X.01._SCMZZZZZZZ_.jpg
    //albumcover_medium=http://images.amazon.com/images/P/B00004T40X.01._SCMZZZZZZZ_.jpg
    //albumcover_large=http://images.amazon.com/images/P/B00004T40X.01._SCMZZZZZZZ_.jpg
    //trackduration=222
    //radiomode=1
    //recordtoprofile=1
    #endregion

    #region Serialisation
    private void LoadSettings()
    {
      CurrentSongTag = new MusicTag();

      httpcommand = new AsyncGetRequest();
      httpcommand.workerFinished += new AsyncGetRequest.AsyncGetRequestCompleted(OnParseAsyncResponse);
      httpcommand.workerError += new AsyncGetRequest.AsyncGetRequestError(OnAsyncRequestError);

      _currentUser = AudioscrobblerBase.Username;

      if (_currentUser.Length > 0)
      {
        AudioscrobblerBase.DoRadioHandshake(true);
      }
      else
      {
        OnRadioLoginFailed();
      }
    }

    private void OnRadioLoginSuccess()
    {
      // for now..
      _currentStreamsUser = _currentUser;
      _currentSession = AudioscrobblerBase.RadioSession;

      if (_currentSession != string.Empty)
      {
        _isSubscriber = AudioscrobblerBase.Subscriber;
        //_currentRadioURL = "http://streamer1.last.fm/last.mp3?Session=" + _currentSession;
        _currentRadioURL = AudioscrobblerBase.RadioStreamLocation;
        _currentState = StreamPlaybackState.initialized;
        _isInit = true;
        RadioSettingsSuccess();
      }
      else
        RadioSettingsError();
    }

    private void OnRadioLoginFailed()
    {
      _currentState = StreamPlaybackState.offline;
      _currentSession = string.Empty;
      _isInit = false; // need to check that..
      RadioSettingsError();
    }
    #endregion

    #region getters & setters
    public string AccountUser
    {
      get { return _currentUser; }
    }

    public string StreamsUser
    {
      get { return _currentStreamsUser; }

      set
      {
        if (value != _currentStreamsUser)
        {
          _currentStreamsUser = value;
          Log.Debug("StreamControl: Setting StreamsUser to {0}", _currentStreamsUser);
        }
      }
    }

    /// <summary>
    /// URL for playback with buffering audioplayers
    /// </summary>
    public string CurrentStream
    {
      get { return _currentRadioURL; }

      set
      {
        if (value != _currentRadioURL)
        {
          _currentRadioURL = value;
          Log.Debug("StreamControl: Setting RadioURL to {0}", _currentRadioURL);
        }
      }
    }

    public StreamType CurrentTuneType
    {
      get { return _currentTuneType; }

      //set
      //{
      //  if (value != _currentTuneType)
      //    _currentTuneType = value;
      //  Log.Debug("StreamControl: Setting CurrentTuneType to {0}", _currentTuneType.ToString());
      //}
    }

    public StreamPlaybackState CurrentStreamState
    {
      get { return _currentState; }

      set
      {
        if (value != _currentState)
          _currentState = value;
        Log.Debug("StreamControl: Setting CurrentStreamState to {0}", _currentState.ToString());
      }
    }

    public PlaybackType CurrentPlaybackType
    {
      get { return _currentPlaybackType; }

      set
      {
        if (value != _currentPlaybackType)
          _currentPlaybackType = value;
        Log.Debug("StreamControl: Setting CurrentPlaybackType to {0}", _currentPlaybackType.ToString());
      }
    }

    public MusicTag CurrentTrackTag
    {
      get { return CurrentSongTag; }
    }

    public bool DiscoveryMode
    {
      get { return _discoveryMode; }

      set
      {
        if (value != _discoveryMode)
        {
          ToggleDiscoveryMode(value);
        }
      }
    }

    public int DiscoveryEnabledInt
    {
      get
      {
        return _discoveryMode ? 1 : 0;
      }
    }

    /// <summary>
    /// Property to check if the settings are loaded and a session is available
    /// </summary>
    public bool IsInit
    {
      get { return _isInit; }
    }

    /// <summary>
    /// Determines if the user has access to restricted streams
    /// </summary>
    public bool IsSubscriber
    {
      get { return _isSubscriber; }
    }

    #endregion

    #region Control functions
    public bool PlayStream()
    {
      GUIWaitCursor.Show();

      if (g_Player.Playing)
        g_Player.Stop();

      _currentState = StreamPlaybackState.starting;
      // often the buffer is too slow for the playback to start
      for (int i = 0; i < 3; i++)
      {
        if (g_Player.Play(_currentRadioURL))
        {
          GUIWaitCursor.Hide();
          _currentState = StreamPlaybackState.streaming;
          ToggleRecordToProfile(AudioscrobblerBase.SubmitRadioSongs);
          ToggleDiscoveryMode(_discoveryMode);

          return true;
        }
      }
      GUIWaitCursor.Hide();
      _currentState = StreamPlaybackState.initialized;
      return false;
    }

    public bool PlayPlayListStreams(string aStreamURL)
    {
      GUIWaitCursor.Show();

      if (g_Player.Playing)
      {
        g_Player.Stop();
        //if (BassMusicPlayer.Player.CrossFadingEnabled)
        //  Thread.Sleep(BassMusicPlayer.Player.CrossFadeIntervalMS);
      }

      _currentState = StreamPlaybackState.starting;

      PlaylistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_RADIO_STREAMS;
      PlayList Playlist = PlaylistPlayer.GetPlaylist(PlaylistPlayer.CurrentPlaylistType);

      if (Playlist == null)
        return false;

      // I found out, you have to send "Cookie: Session=[sessionID]" in the header of the request of the MP3 file. 
      PlaylistPlayer.Play(aStreamURL);

      GUIWaitCursor.Hide();
      _currentState = StreamPlaybackState.streaming;
      //ToggleRecordToProfile(false);
      ToggleDiscoveryMode(_discoveryMode);

      return true;
    }

    public void LoadConfig()
    {
      LoadSettings();
    }

    public void UpdateNowPlaying(bool delayed)
    {
      if (delayed)
        // give the site some time to update and sync with the stream switch
        SendDelayedCommandRequest(@"http://ws.audioscrobbler.com/radio/np.php?session=" + _currentSession, 4750);
      else
        SendCommandRequest(@"http://ws.audioscrobbler.com/radio/np.php?session=" + _currentSession);
    }

    public void ToggleRecordToProfile(bool submitTracks_)
    {
      if (submitTracks_)
      {
        if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession + "&command=rtp"))
        {
          AudioscrobblerBase.SubmitRadioSongs = true;
          Log.Info("StreamControl: Enabled submitting of radio tracks to profile");
        }
      }
      else
        if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession + "&command=nortp"))
        {
          if (CurrentPlaybackType != PlaybackType.PlaylistPlayer)
            Log.Info("StreamControl: Disabled submitting of radio tracks to profile");
        }
    }

    public bool ToggleDiscoveryMode(bool enableDiscovery_)
    {
      bool success = false;
      string actionCommand = enableDiscovery_ ? "on" : "off";

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://settings/discovery/" + actionCommand))
      {
        success = true;
        _discoveryMode = enableDiscovery_;
        Log.Info("StreamControl: Toggled discovery mode {0}", actionCommand);
      }

      return success;
    }

    public void SendControlCommand(StreamControls command_)
    {
      if (_currentState == StreamPlaybackState.streaming)
      {
        string baseUrl = @"http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession;

        switch (command_)
        {
          case StreamControls.skiptrack:
            SendCommandRequest(baseUrl + @"&command=skip");
            break;
          case StreamControls.lovetrack:
            SendCommandRequest(baseUrl + @"&command=love");
            break;
          case StreamControls.bantrack:
            SendCommandRequest(baseUrl + @"&command=ban");
            break;
        }
      }
      else
        Log.Info("StreamControl: Currently not streaming - ignoring command");
    }
    #endregion

    #region Tuning functions
    public bool TuneIntoPersonalRadio(string username_)
    {
      string TuneUser = AudioscrobblerBase.getValidURLLastFMString(username_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/personal"))
      {
        _currentTuneType = StreamType.Personal;
        Log.Info("StreamControl: Tune into personal station of: {0}", username_);
        GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34043) + username_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoNeighbourRadio(string username_)
    {
      string TuneUser = AudioscrobblerBase.getValidURLLastFMString(username_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/neighbours"))
      {
        _currentTuneType = StreamType.Neighbours;
        Log.Info("StreamControl: Tune into neighbour station of: {0}", username_);
        GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34048)); // My neighbour radio
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoLovedTracks(string username_)
    {
      string TuneUser = AudioscrobblerBase.getValidURLLastFMString(username_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/loved"))
      {
        _currentTuneType = StreamType.Loved;
        Log.Info("StreamControl: Tune into loved tracks of: {0}", username_);
        GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34044) + username_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoGroupRadio(string groupname_)
    {
      string TuneGroup = AudioscrobblerBase.getValidURLLastFMString(groupname_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://group/" + TuneGroup))
      {
        _currentTuneType = StreamType.Group;
        Log.Info("StreamControl: Tune into group radio for: {0}", groupname_);

        GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", "Group radio of: " + groupname_);

        return true;
      }
      else
        return false;
    }

    public bool TuneIntoRecommendedRadio(string username_)
    {
      string TuneUser = AudioscrobblerBase.getValidURLLastFMString(username_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/recommended"))
      {
        _currentTuneType = StreamType.Recommended;
        Log.Info("StreamControl: Tune into recommended station for: {0}", username_);
        GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34040));
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoArtist(string artist_)
    {
      string TuneArtist = AudioscrobblerBase.getValidURLLastFMString(artist_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://artist/" + TuneArtist + "/similarartists"))
      {
        _currentTuneType = StreamType.Artist;
        Log.Info("StreamControl: Tune into artists similar to: {0}", artist_);

        GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", "Artists similar to: " + artist_);

        return true;
      }
      else
        return false;
    }

    public bool TuneIntoTags(List<String> tags_)
    {
      string TuneTags = string.Empty;

      foreach (string singleTag in tags_)
      {
        TuneTags += AudioscrobblerBase.getValidURLLastFMString(singleTag) + ",";
      }
      // remove trailing comma
      TuneTags = TuneTags.Remove(TuneTags.Length - 1);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://globaltags/" + TuneTags))
      {
        _currentTuneType = StreamType.Tags;
        Log.Info("StreamControl: Tune into tags: {0}", TuneTags);
        GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34041) + TuneTags);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoWebPlaylist(string username_)
    {
      string TuneUser = AudioscrobblerBase.getValidURLLastFMString(username_);
      //ext.last.fm/
      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/playlist"))
      {
        _currentTuneType = StreamType.Playlist;
        Log.Info("StreamControl: Tune into web playlist of: {0}", username_);
        GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34049));
        return true;
      }
      else
        return false;
    }
    #endregion

    #region Network related
    private bool SendCommandRequest(string url_)
    {
      try
      {
        // Enforce a minimum wait time between connects.
        DateTime nextconnect = _lastConnectAttempt.Add(_minConnectWaitTime);
        if (DateTime.Now < nextconnect)
        {
          TimeSpan waittime = nextconnect - DateTime.Now;
          Log.Debug("StreamControl: Avoiding too fast connects for {0} - sleeping until {1}", url_, nextconnect.ToString());
          Thread.Sleep(waittime);
        }
      }
      // While debugging you might get a waittime which is no longer a valid integer.
      catch (Exception) { }

      _lastConnectAttempt = DateTime.Now;

      httpcommand.SendAsyncGetRequest(url_);

      return true;
    }

    private void SendDelayedCommandRequest(string url_, int delayMSecs_)
    {
      httpcommand.SendDelayedAsyncGetRequest(url_, delayMSecs_);
    }

    private void OnAsyncRequestError(String urlCommand, Exception errorReason)
    {
      Log.Warn("StreamControl: Async request for {0} unsuccessful: {1}", urlCommand, errorReason.Message);
    }

    private void OnParseAsyncResponse(List<string> responseList, HttpStatusCode responseCode, String requestedURLCommand)
    {
      // parse the response
      try
      {
        lock (this)
        {
          string responseMessage = string.Empty;
          if (responseList.Count > 0)
          {
            List<string> responseStrings = new List<string>(responseList);

            if (responseCode == HttpStatusCode.OK)
            {
              responseMessage = responseStrings[0];
              {
                if (responseMessage.StartsWith("response=OK"))
                {
                  ParseSuccessful(responseStrings, requestedURLCommand);
                  return;
                }

                if (responseMessage.StartsWith("price="))
                {
                  ParseNowPlaying(responseStrings);
                  return;
                }
              }
            }
            else
            {
              string logmessage = "StreamControl: ***** Unknown response! - " + responseMessage;
              foreach (String unkStr in responseStrings)
              {
                logmessage += "\n" + unkStr;
              }

              if (logmessage.Contains("Not enough content"))
              {
                _currentState = StreamPlaybackState.nocontent;
                Log.Warn("StreamControl: Not enough content left to play this station");
                return;
              }
              else
                Log.Warn(logmessage);
            }
          }
          else
            Log.Debug("StreamControl: SendCommandRequest: Reader object already destroyed");
        }
      }
      catch (Exception e)
      {
        Log.Error("StreamControl: SendCommandRequest: Parsing response failed {0}", e.Message);
        return;
      }

      return;
    }
    # endregion

    #region Response parser
    private void ParseSuccessful(List<String> responseList_, String formerRequest_)
    {
      if (formerRequest_.Contains(@"&command=skip"))
      {
        Log.Info("StreamControl: Successfully send skip command");
        return;
      }

      if (formerRequest_.Contains(@"&command=love"))
      {
        Log.Info("StreamControl: Track added to loved tracks list");
        return;
      }

      if (formerRequest_.Contains(@"&command=ban"))
      {
        Log.Info("StreamControl: Track added to banned tracks list");
        return;
      }
    }

    private void ParseNowPlaying(List<String> responseList_)
    {
      List<String> NowPlayingInfo = new List<string>();
      String prevTitle = CurrentSongTag.Title;
      CurrentSongTag.Clear();

      try
      {
        foreach (String respStr in responseList_)
          NowPlayingInfo.Add(respStr);

        foreach (String token in NowPlayingInfo)
        {
          if (token.StartsWith("artist="))
          {
            if (token.Length > 7)
              CurrentSongTag.Artist = token.Substring(7);
          }
          else if (token.StartsWith("album="))
          {
            if (token.Length > 6)
              CurrentSongTag.Album = token.Substring(6);
          }
          else if (token.StartsWith("track="))
          {
            if (token.Length > 6)
              CurrentSongTag.Title = token.Substring(6);
          }
          else if (token.StartsWith("station="))
          {
            if (token.Length > 8)
              CurrentSongTag.Genre = token.Substring(8);
          }
          else if (token.StartsWith("albumcover_large="))
          {
            if (token.Length > 17)
              CurrentSongTag.Comment = token.Substring(17);
          }
          else if (token.StartsWith("trackduration="))
          {
            if (token.Length > 14)
            {
              int trackLength = Convert.ToInt32(token.Substring(14));
              CurrentSongTag.Duration = trackLength;
            }
          }
        }

        if (CurrentSongTag.Title != prevTitle)
        {
          AudioscrobblerBase.CurrentSong.Clear();
          AudioscrobblerBase.CurrentSong.Artist = CurrentSongTag.Artist;
          AudioscrobblerBase.CurrentSong.Album = CurrentSongTag.Album;
          AudioscrobblerBase.CurrentSong.Title = CurrentSongTag.Title;
          AudioscrobblerBase.CurrentSong.Genre = CurrentSongTag.Genre;
          AudioscrobblerBase.CurrentSong.Duration = CurrentSongTag.Duration;
          AudioscrobblerBase.CurrentSong.WebImage = CurrentSongTag.Comment;
          AudioscrobblerBase.CurrentSong.FileName = g_Player.Player.CurrentFile;

          // fire the event
          if (StreamSongChanged != null)
            StreamSongChanged(CurrentSongTag, DateTime.Now);

          GUIPropertyManager.SetProperty("#Play.Current.Artist", CurrentSongTag.Artist);
          GUIPropertyManager.SetProperty("#Play.Current.Album", CurrentSongTag.Album);
          GUIPropertyManager.SetProperty("#Play.Current.Title", CurrentSongTag.Title);
          GUIPropertyManager.SetProperty("#Play.Current.Genre", CurrentSongTag.Genre);
          GUIPropertyManager.SetProperty("#Play.Current.Thumb", CurrentSongTag.Comment);
          GUIPropertyManager.SetProperty("#trackduration", Util.Utils.SecondsToHMSString(CurrentSongTag.Duration));

          // Send msg for Ballon Tip on song change
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_BALLONTIP_SONGCHANGE, 0, 0, 0, 0, 0, null);
          msg.Label = CurrentSongTag.Title;
          msg.Label2 = CurrentSongTag.Artist + " (" + CurrentSongTag.Album + ")";
          msg.Param1 = 5;
          GUIGraphicsContext.SendMessage(msg);
          msg = null;

          Log.Info("StreamControl: Current track: {0} [{1}] - {2} ({3})", CurrentSongTag.Artist, CurrentSongTag.Album, CurrentSongTag.Title, Util.Utils.SecondsToHMSString(CurrentSongTag.Duration));
        }

      }
      catch (Exception ex)
      {
        Log.Error("StreamControl: Error parsing now playing info: {0}", ex.Message);
      }
    }
    #endregion
  }
}
