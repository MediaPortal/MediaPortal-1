#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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
using System.IO;
using System.Net;
using System.Text;
using System.Timers;
using System.Threading;

using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.TagReader;
using MediaPortal.Utils.Web;


namespace MediaPortal.GUI.RADIOLASTFM
{

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
  }

  class StreamControl
  {
    public delegate void SongChangedHandler(MusicTag newCurrentSong, DateTime startTime);
    public event SongChangedHandler StreamSongChanged;
    //static public event SongChangedHandler StreamSongChanged;

    private AudioscrobblerUtils InfoScrobbler = null;

    private string _currentRadioURL = String.Empty;
    private string _currentSession = String.Empty;
    private string _currentUser = String.Empty;
    private string _currentStreamsUser = String.Empty;
    private bool _isSubscriber = false;
    private bool _recordToProfile = true;
    private bool _discoveryMode = false;

    private bool _isInit = false;

    private MusicTag CurrentSongTag;
    private System.Timers.Timer _nowPlayingTimer;
    private StreamPlaybackState _currentState = StreamPlaybackState.offline;
    private StreamType _currentTuneType = StreamType.Recommended;

    private DateTime _lastConnectAttempt = DateTime.MinValue;
    private TimeSpan _minConnectWaitTime = new TimeSpan(0, 0, 1);
    private int _retryFetchCount = 0;
    private Object BadLock = null;

    private AsyncGetRequest httpcommand = null;

    #region Examples
    // 5. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/np.php?session=e5b0c80f5b5d0937d407fb77a913cb6a
    // 6. http.request.uri = Request URI: http://ws.audioscrobbler.com/ass/artistmetadata.php?artist=Sportfreunde%20Stiller&lang=en
    // 7. http.request.uri = Request URI: http://ws.audioscrobbler.com/ass/metadata.php?artist=Sportfreunde%20Stiller&track=Alles%20Das&album=Macht%20doch%20was%20ihr%20wollt%20-%20Ich%20geh%2527%20jetzt%2521

    // SKIP Button
    // 8. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/control.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&command=skip

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
      BadLock = new object();

      CurrentSongTag = new MusicTag();
      _nowPlayingTimer = new System.Timers.Timer();
      _nowPlayingTimer.Interval = 180000;
      _nowPlayingTimer.Elapsed += new ElapsedEventHandler(OnTimerTick);

      InfoScrobbler = AudioscrobblerUtils.Instance;
      httpcommand = new AsyncGetRequest();
      httpcommand.workerFinished += new AsyncGetRequest.AsyncGetRequestCompleted(OnParseAsyncResponse);
      httpcommand.workerError += new AsyncGetRequest.AsyncGetRequestError(OnAsyncRequestError);

      _currentUser = AudioscrobblerBase.Username;

      if (_currentUser.Length > 0)
      {
        _currentSession = AudioscrobblerBase.RadioSession;
        // for now..
        _currentStreamsUser = _currentUser;

        if (_currentSession != String.Empty)
        {
          _isSubscriber = AudioscrobblerBase.Subscriber;
          _currentRadioURL = "http://streamer1.last.fm/last.mp3?Session=" + _currentSession;
          _currentState = StreamPlaybackState.initialized;

          //List<String> MyTags = new List<string>();
          //MyTags.Add("cover");
          //MyTags.Add("melodic death metal");
          //TuneIntoTags(MyTags);
          //TuneIntoPersonalRadio();
          //TuneIntoPersonalRadio(_currentStreamsUser);  <-- subscriber only
          //TuneIntoGroupRadio(_currentStreamsUser);
          //TuneIntoRecommendedRadio(_currentStreamsUser); 
          _isInit = true;
        }
      }
    }
    #endregion

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



    public MusicTag CurrentTrackTag
    {
      get { return CurrentSongTag; }
    }

    /// <summary>
    /// Get/Set if you like your radio songs to appear in your last.fm profile
    /// </summary>
    public bool SubmitRadioSongs
    {
      get { return _recordToProfile; }

      set
      {
        if (value != _recordToProfile)
        {
          ToggleRecordToProfile(value);
        }
      }
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
    

    #region Control functions

    public bool PlayStream()
    {
      _currentState = StreamPlaybackState.starting;
      // often the buffer is too slow for the playback to start
      for (int i = 0; i < 3; i++)
      {
        if (g_Player.Play(_currentRadioURL))
        {
          _currentState = StreamPlaybackState.streaming;
          ToggleRecordToProfile(_recordToProfile);
          ToggleDiscoveryMode(_discoveryMode);
          _nowPlayingTimer.Start();
          SendCommandRequest(@"http://ws.audioscrobbler.com/radio/np.php?session=" + _currentSession);

          return true;
        }
      }
      _currentState = StreamPlaybackState.initialized;
      return false;
    }

    //public override bool Playing
    //{
    //  get
    //  {
    //    return _currentState == StreamPlaybackState.streaming;
    //  }
    //}

    //public override string CurrentFile
    //{
    //  get
    //  {
    //    return _currentRadioURL;
    //  }
    //}

    //public override double Duration
    //{
    //  get
    //  {
    //    double tmpDuration = 0;
    //    if (CurrentSongTag != null)
    //    {
    //      tmpDuration = Convert.ToDouble(CurrentSongTag.Duration);
    //    }
    //    return tmpDuration;
    //  }
    //}

    //public override bool HasVideo
    //{
    //  get
    //  {
    //    return false;
    //  }
    //}

    //public override bool IsRadio
    //{
    //  get
    //  {
    //    return true;
    //  }
    //}

    //public override bool Paused
    //{
    //  get
    //  {
    //    return _currentState == StreamPlaybackState.paused;
    //  }
    //}

    public void LoadConfig()
    {
      //if (!_isInit)
        LoadSettings();
    }

    public void UpdateNowPlaying()
    {
      // give the site some time to update
      SendDelayedCommandRequest(@"http://ws.audioscrobbler.com/radio/np.php?session=" + _currentSession, 3000);
    }

    public bool ToggleRecordToProfile(bool submitTracks_)
    {
      bool success = false;

      if (submitTracks_)
      {
        if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession + "&command=rtp"))
        {
          success = true;
          _recordToProfile = true;
          Log.Info("StreamControl: Enabled submitting of radio tracks to profile");
        }
      }
      else
        if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession + "&command=nortp"))
        {
          success = true;
          _recordToProfile = false;
          Log.Info("StreamControl: Disabled submitting of radio tracks to profile");
        }
      return success;
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
      string TuneUser = InfoScrobbler.getValidURLLastFMString(username_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/personal"))
      {
        _currentTuneType = StreamType.Personal;
        Log.Info("StreamControl: Tune into personal station of: {0}", username_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoLovedTracks(string username_)
    {
      string TuneUser = InfoScrobbler.getValidURLLastFMString(username_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/loved"))
      {
        _currentTuneType = StreamType.Loved;
        Log.Info("StreamControl: Tune into loved tracks of: {0}", username_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoGroupRadio(string groupname_)
    {
      string TuneGroup = InfoScrobbler.getValidURLLastFMString(groupname_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://group/" + TuneGroup))
      {
        _currentTuneType = StreamType.Group;
        Log.Info("StreamControl: Tune into group radio for: {0}", groupname_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoRecommendedRadio(string username_)
    {
      string TuneUser = InfoScrobbler.getValidURLLastFMString(username_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://user/" + TuneUser + "/recommended"))
      {
        _currentTuneType = StreamType.Recommended;
        Log.Info("StreamControl: Tune into recommended station for: {0}", username_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoArtist(string artist_)
    {
      string TuneArtist = InfoScrobbler.getValidURLLastFMString(artist_);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://artist/" + TuneArtist + "/similarartists"))
      {
        _currentTuneType = StreamType.Artist;
        Log.Info("StreamControl: Tune into artists similar to: {0}", artist_);
        return true;
      }
      else
        return false;
    }

    public bool TuneIntoTags(List<String> tags_)
    {
      string TuneTags = String.Empty;

      foreach (string singleTag in tags_)
      {
        TuneTags += InfoScrobbler.getValidURLLastFMString(singleTag) + ",";
      }
      // remove trailing comma
      TuneTags = TuneTags.Remove(TuneTags.Length - 1);

      if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://globaltags/" + TuneTags))
      {
        _currentTuneType = StreamType.Tags;
        Log.Info("StreamControl: Tune into tags: {0}", TuneTags);
        return true;
      }
      else
        return false;
    }
    #endregion

    #region Network related
    private bool SendCommandRequest(string url_)
    {
      // Enforce a minimum wait time between connects.
      DateTime nextconnect = _lastConnectAttempt.Add(_minConnectWaitTime);
      if (DateTime.Now < nextconnect)
      {
        TimeSpan waittime = nextconnect - DateTime.Now;
        Log.Debug("StreamControl: Avoiding too fast connects for {0} - sleeping until {1}", url_, nextconnect.ToString());
        Thread.Sleep(waittime);
      }
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
        lock (BadLock)
        {
          string responseMessage = String.Empty;
          if (responseList.Count > 0)
          {
            List<string> responseStrings = new List<string>(responseList);

            //foreach (String responsestr in responseList)
            //{
            //  responseStrings.Add(responsestr);
            //}

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
                  //return true;
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
                //return false;
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
        //return false;
        return;
      }

      //return false;
      return;
    }
    # endregion


    #region Response parser
    private void ParseSuccessful(List<String> responseList_, String formerRequest_)
    {
      if (formerRequest_.Contains(@"&command=skip"))
      {
        Log.Info("StreamControl: Successfully send skip command");
        UpdateNowPlaying();
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
        UpdateNowPlaying();
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
        {
          NowPlayingInfo.Add(respStr);
        }
          

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

              if (trackLength > 0)
              {
                _nowPlayingTimer.Stop();
                _nowPlayingTimer.Interval = trackLength * 1000;
                _nowPlayingTimer.Start();
              }
            }
          }
        }

        GUIPropertyManager.SetProperty("#Play.Current.Artist", CurrentSongTag.Artist);
        GUIPropertyManager.SetProperty("#Play.Current.Album", CurrentSongTag.Album);
        GUIPropertyManager.SetProperty("#Play.Current.Title", CurrentSongTag.Title);
        GUIPropertyManager.SetProperty("#Play.Current.Genre", CurrentSongTag.Genre);
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", CurrentSongTag.Comment);
        GUIPropertyManager.SetProperty("#trackduration", Convert.ToString(CurrentSongTag.Duration));

        if (CurrentSongTag.Title != prevTitle)
        {
          if (StreamSongChanged != null)
            StreamSongChanged(CurrentSongTag, DateTime.Now);

          _retryFetchCount = 0;

          // Send msg for Ballon Tip on song change
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_BALLONTIP_SONGCHANGE, 0, 0, 0, 0, 0, null);
          msg.Label = GUIPropertyManager.GetProperty("#Play.Current.Title");
          msg.Label2 = GUIPropertyManager.GetProperty("#Play.Current.Artist") + " (" + GUIPropertyManager.GetProperty("#Play.Current.Album") + ")";
          msg.Param1 = 5;
          GUIGraphicsContext.SendMessage(msg);
          msg = null;
        }
        else // maybe we asked to early - try again
        {
          //Thread.Sleep(2000);
          _retryFetchCount++;
          if (_retryFetchCount < 3)
          {
            Log.Debug("StreamControl: Same title found ({0}) - trying again now..", _retryFetchCount);
            UpdateNowPlaying();
          }
        }

        Log.Info("StreamControl: Current track: {0} [{1}] - {2} ({3})", CurrentSongTag.Artist, CurrentSongTag.Album, CurrentSongTag.Title, Util.Utils.SecondsToHMSString(CurrentSongTag.Duration));
      }
      catch (Exception ex)
      {
        Log.Error("StreamControl: Error parsing now playing info: {0}", ex.Message);
      }
    }
    #endregion

    #region Utils
    private void OnTimerTick(object trash_, ElapsedEventArgs args_)
    {
      UpdateNowPlaying();
    }

    #endregion

  }
}
