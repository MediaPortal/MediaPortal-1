#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Linq;
using System.Net;
using System.Text;
using MediaPortal.Database;
using MediaPortal.Music.Database;
using MediaPortal.GUI.Library;
using System.Xml.Linq;
using MediaPortal.Util;

namespace MediaPortal.LastFM
{
  public class LastFMLibrary
  {

    private static string _sessionKey = String.Empty;
    private static string _currentUser = string.Empty;
    internal static string BaseURL = "http://ws.audioscrobbler.com/2.0/";
    internal static string BaseURLHttps = "https://ws.audioscrobbler.com/2.0/";

    #region ctor

    public LastFMLibrary()
    {
      // Expect 100 continue headers cause the last.fm server to not respond
      var baseURI = new Uri(BaseURL);
      ServicePointManager.Expect100Continue = false;
      var servicePoint = ServicePointManager.FindServicePoint(baseURI);
      servicePoint.Expect100Continue = false;

      LoadSettings();
    }

    #endregion

    #region properties

    public static string CurrentUser
    {
      get
      {
        return !string.IsNullOrEmpty(_currentUser) ? _currentUser : string.Empty;
      }
    }

    #endregion

    #region settings

    private static void LoadSettings()
    {
      var mdb = MusicDatabase.Instance;
      _sessionKey = mdb.GetLastFMSK();
      _currentUser = mdb.GetLastFMUser();
    }

    private static void SaveSettings()
    {
      var mdb = MusicDatabase.Instance;
      mdb.AddLastFMUser(_currentUser, _sessionKey);
    }

    #endregion

    #region lastFM Authentication

    public static bool AuthGetMobileSession(string username, string password)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "auth.getMobileSession";
      parms.Add("username", username);
      parms.Add("password", password);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpPost(buildLastFMString, true);

      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
        return false;
      }

      var lastFMXML = XDocument.Parse(lastFMResponseXML);

      var lfmElement = lastFMXML.Element("lfm");
      if (lfmElement != null)
      {
        var sessionElement = lfmElement.Element("session");
        if (sessionElement != null)
        {
          var sk = sessionElement.Element("key");
          Log.Info("Saved last.fm session key for: {0}", username);

          if (sk != null) _sessionKey = sk.Value;
          _currentUser = username;

          return MusicDatabase.Instance.AddLastFMUser(username, _sessionKey);
        }
      }

      Log.Error("AuthGetMobileSession: Valid response but unexpeceted xml");
      Log.Error("{0}", lastFMResponseXML);

      return false;      

    }

    #endregion

    #region scrobbling methods

    /// <summary>
    /// Announce track as now playing on user profile on last.fm website
    /// </summary>
    /// <param name="strArtist">artist of track being played</param>
    /// <param name="strTrack">name of track being played</param>
    /// <param name="strAlbum">album track being played is part of</param>
    /// <param name="strDuration">duration of track being played</param>
    public static void UpdateNowPlaying(String strArtist, String strTrack, String strAlbum, String strDuration)
    {
      if (string.IsNullOrEmpty(_sessionKey))
      {
        Log.Warn("Attempted to announce track: {0} - {1}", strArtist, strTrack);
        Log.Warn("But last.fm has not been authorised so aborting");
        return;
      }

      var parms = new Dictionary<string, string>();
      const string methodName = "track.updateNowPlaying";
      parms.Add("artist", strArtist);
      parms.Add("track", strTrack);
      if (!String.IsNullOrEmpty(strAlbum))
      {
        parms.Add("album", strAlbum);
      }
      if (!String.IsNullOrEmpty(strDuration))
      {
        parms.Add("duration", strDuration);
      }
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpPost(buildLastFMString);
      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
      }
      else
      {
        Log.Info("Submitted last.fm now playing update for: {0} - {1}", strArtist, strTrack);
      }
    }


    /// <summary>
    /// Scrobble track to last.fm showing as played on user profile on last.fm website.   
    /// Assumes track is selected by user and scrobble is for current time
    /// </summary>
    /// <param name="strArtist">artist of track that was played</param>
    /// <param name="strTrack">name of track that was played</param>
    /// <param name="strAlbum">album that track that was played is part of</param>
    public static void Scrobble(String strArtist, String strTrack, String strAlbum)
    {
      var track = new LastFMScrobbleTrack
                    {
                      ArtistName = strArtist,
                      TrackTitle = strTrack,
                      AlbumName = strAlbum,
                      DatePlayed = DateTime.UtcNow,
                      UserSelected = true
                    };
      var tracks = new List<LastFMScrobbleTrack> {track};
      ScrobbleTracks(tracks);
    }

    /// <summary>
    /// Scrobble track to last.fm showing as played on user profile on last.fm website
    /// </summary>
    /// <param name="strArtist">artist of track that was played</param>
    /// <param name="strTrack">name of track that was played</param>
    /// <param name="strAlbum">album that track that was played is part of</param>
    /// <param name="isUserSubmitted">True if track was selected by user or false if by system (radio / auto DJ etc)</param> 
    /// <param name="dtPlayed">Date track was played</param>
    public static void Scrobble(String strArtist, String strTrack, String strAlbum, bool isUserSubmitted, DateTime dtPlayed)
    {
      var track = new LastFMScrobbleTrack
      {
        ArtistName = strArtist,
        TrackTitle = strTrack,
        AlbumName = strAlbum,
        DatePlayed = DateTime.UtcNow,
        UserSelected = true
      };

      if (Win32API.IsConnectedToInternet())
      {
        var tracks = new List<LastFMScrobbleTrack> { track };
        ScrobbleTracks(tracks);        
      }
      else
      {
        CacheScrobble(strArtist, strTrack, strAlbum, isUserSubmitted, dtPlayed);
      }
    }

    /// <summary>
    /// Scrobble a collection of tracks
    /// </summary>
    /// <param name="tracks">List of tracks to scrobble</param>
    public static void ScrobbleTracks(List<LastFMScrobbleTrack> tracks)
    {
      // only able to scrobble tracks in batches of 50 so split into multiple chunks if needed
      foreach (var trackList in tracks.InSetsOf(50))
      {
        ScrobbleTracksInternal(trackList);
      }
    }

    /// <summary>
    /// We are only able to scrobble at most 50 tracks each time
    /// Previous step must ensure that list contains 50 or less items
    /// </summary>
    /// <param name="tracks">List of tracks to scrobble</param>
    private static void ScrobbleTracksInternal(IEnumerable<LastFMScrobbleTrack> tracks)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.scrobble";
      int i = 0;

      var scrobbleTracks = tracks as IList<LastFMScrobbleTrack> ?? tracks.ToList();
      foreach (var track in scrobbleTracks)
      {
        var span = (track.DatePlayed.ToUniversalTime() - new DateTime(1970, 1, 1));
        var unixEpoch = (int) span.TotalSeconds;

        parms.Add(string.Format("timestamp[{0}]", i), unixEpoch.ToString(CultureInfo.InvariantCulture));
        parms.Add(string.Format("artist[{0}]", i), track.ArtistName);
        parms.Add(string.Format("track[{0}]",i), track.TrackTitle);
        if (!String.IsNullOrEmpty(track.AlbumName))
        {
          parms.Add(string.Format("album[{0}]",i), track.AlbumName);
        }
        if (!track.UserSelected)
        {
          // parameter used to identify that track has been scrobbled but user did not select it
          // eg. radio or auto DJ
          parms.Add(string.Format("chosenByUser[{0}]", i), "0");
        }

        i++;
      }

      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpPost(buildLastFMString);

      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
      }
      else
      {
        foreach (var track in scrobbleTracks)
        {
          Log.Info("Submitted last.fm scrobble for: {0}-{1} @{3} - {4}", track.ArtistName, track.AlbumName, track.DatePlayed.ToShortDateString(), track.DatePlayed.ToLongTimeString());
        }
      }

    }

    private static void CacheScrobble(string strArtist, string strTrack, String strAlbum, bool isUserSubmitted, DateTime dtPlayed)
    {
      //TODO: write to cache file (or database?)
    }

    private static void SumbitCachedScrobbles()
    {
      var tracks = new List<LastFMScrobbleTrack>();


      string strArtist = string.Empty;
      string strAlbum = string.Empty;
      string strTrack = string.Empty;
      DateTime dtPlayed = DateTime.UtcNow;
      bool bUserSelected = true;

      //TODO: read cache and turn into LastFMScrobbleTrack instances
      {
        var track = new LastFMScrobbleTrack
          {
            ArtistName = strArtist,
            TrackTitle = strTrack,
            AlbumName = strAlbum,
            DatePlayed = dtPlayed,
            UserSelected = bUserSelected
          };
        tracks.Add(track);
      }
      ScrobbleTracks(tracks);
    }

    #endregion

    #region radio methods

    /// <summary>
    /// Tune to a radio station.   After runing call GetRadioPlaylist to get the track listing
    /// </summary>
    /// <param name="strStationName"></param>
    public static bool TuneRadio(string strStationName)
    {
      Log.Debug("LastFM.TuneRadio: Attempting to tune radio to: {0}", strStationName);

      var parms = new Dictionary<string, string>();
      const string methodName = "radio.tune";
      parms.Add("station", strStationName);
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpPost(buildLastFMString);
      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
        return false;
      }

      Log.Info("LastFM.TuneRadio: Tuned to last.fm Radio Station: {0}", strStationName);
      return true;
    }

    /// <summary>
    /// Gets the playlist of radio station (will only be a small number of tracks)
    /// </summary>
    /// <returns>A list of tracks</returns>
    public static bool GetRadioPlaylist(out List<LastFMStreamingTrack> tracks)
    {
      tracks = new List<LastFMStreamingTrack>();
      var parms = new Dictionary<string, string>();
      const string methodName = "radio.getPlaylist";
      parms.Add("bitrate", "128");
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpPost(buildLastFMString);
      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
        return false;
      }

      var y = XDocument.Parse(lastFMResponseXML);
      XNamespace ns = "http://xspf.org/ns/0/";
      var z = (from a in y.Descendants(ns + "track")
               select new LastFMStreamingTrack
                        {
                          ArtistName = (string) a.Element(ns + "creator"),
                          TrackTitle = (string) a.Element(ns + "title"),
                          TrackURL = (string) a.Element(ns + "location"),
                          Duration = Int32.Parse((string) a.Element(ns + "duration")) / 1000,
                          Identifier = Int32.Parse((string) a.Element(ns + "identifier")),
                          ImageURL = (string) a.Element(ns + "image")
                        }).ToList();
      tracks = z;
      return true;

    }

    #endregion

    #region track methods

    /// <summary>
    /// Get info about the track
    /// </summary>
    /// <param name="strArtist">Artist Name</param>
    /// <param name="strTrack">Track Title</param>
    public static void GetTrackInfo(string strArtist, string strTrack)
    {
      //TODO: what to return?
      var parms = new Dictionary<string, string>();
      const string methodName = "track.getInfo";
      parms.Add("artist", strArtist);
      parms.Add("track", strTrack);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpGet(buildLastFMString);

      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
      }

      var xDoc = XDocument.Parse(lastFMResponseXML);      
    }


    /// <summary>
    /// Pickup similar tracks to the details provided.   These tracks may not
    /// </summary>
    /// <param name="strTrack">name of track to use for lookup</param>
    /// <param name="strArtist">artist of track to use for lookup</param>
    /// <returns>A list of similar tracks from last.fm.  
    ///          There is no check to see if the user has access to these tracks
    ///          See GetSimilarTracksInDatabase to only return tracks in music database</returns>
    public static List<LastFMTrack> GetSimilarTracks(string strTrack, string strArtist)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.getSimilar";
      parms.Add("track", strTrack);
      parms.Add("artist", strArtist);
      parms.Add("autocorrect", "1");

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, false);
      var lastFMResponseXML = HttpGet(buildLastFMString);

      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
        return null;
      }

      //turn response into XML doc
      var lastFMXML = XDocument.Parse(lastFMResponseXML);

      //get a collection of tracks returned by last.fm API
      var tracks = (
                     from n in lastFMXML.Descendants("track")
                     let artistElement = n.Element("artist")
                     where artistElement != null
                     let trackElement = n.Element("name")
                     where trackElement != null
                     let artistNameElement = artistElement.Element("name")
                     where artistNameElement != null
                     select new LastFMTrack(
                       (string) artistNameElement,
                       (string) trackElement
                       )).ToList();

      return tracks;
    }

    /// <summary>
    /// Marks a track as loved on last.fm
    /// </summary>
    /// <param name="strArtist">Artist Name</param>
    /// <param name="strTrack">Track Title</param>
    /// <returns>True if successful</returns>
    public static bool LoveTrack(string strArtist, string strTrack)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.love";
      parms.Add("artist", strArtist);
      parms.Add("track", strTrack);
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpPost(buildLastFMString);
      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
        return false;
      }
      else
      {
        Log.Info("Loved Track: {0} - {1}", strArtist, strTrack);
      }
      return true;
    }

    /// <summary>
    /// Unmarks a track as loved on last.fm
    /// </summary>
    /// <param name="strArtist">Artist Name</param>
    /// <param name="strTrack">Track Title</param>
    /// <returns>True if successful</returns>
    public static bool UnLoveTrack(string strArtist, string strTrack)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.unlove";
      parms.Add("artist", strArtist);
      parms.Add("track", strTrack);
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpPost(buildLastFMString);
      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
        return false;
      }
      else
      {
        Log.Info("Unloved Track: {0} - {1}", strArtist, strTrack);
      }
      return true;
    }

    /// <summary>
    /// Marks track as banned on last.fm
    /// </summary>
    /// <param name="strArtist">Artist Name</param>
    /// <param name="strTrack">Track Title</param>
    /// <returns>True if successful</returns>
    public static bool BanTrack(string strArtist, string strTrack)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.ban";
      parms.Add("artist", strArtist);
      parms.Add("track", strTrack);
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpPost(buildLastFMString);
      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
        return false;
      }
      else
      {
        Log.Info("Banned Track: {0} - {1}", strArtist, strTrack);
      }
      return true;
    }

    /// <summary>
    /// Unmarks a track as banned on last.fm
    /// </summary>
    /// <param name="strArtist">Artist Name</param>
    /// <param name="strTrack">Track Title</param>
    /// <returns>True if successful</returns>
    public static bool UnBanTrack(string strArtist, string strTrack)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.unban";
      parms.Add("artist", strArtist);
      parms.Add("track", strTrack);
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpPost(buildLastFMString);
      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
        return false;
      }
      else
      {
        Log.Info("Unbanned Track: {0} - {1}", strArtist, strTrack);
      }
      return true;
    }

    #endregion

    #region artist methods

    /// <summary>
    /// Get artist details from last.fm
    /// </summary>
    /// <param name="strArtist">Artist Name</param>
    public static void GetArtistInfo(string strArtist)
    {
      //TODO: what to return?
      var parms = new Dictionary<string, string>();
      const string methodName = "artist.getInfo";
      parms.Add("artist", strArtist);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpGet(buildLastFMString);

      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
      }

      var xDoc = XDocument.Parse(lastFMResponseXML);
    }

    #endregion

    #region album methods

    /// <summary>
    /// Get album details
    /// </summary>
    /// <param name="strArtist">Artist Naae</param>
    /// <param name="strAlbum">Album Name</param>
    public static void GetAlbumInfo(string strArtist, string strAlbum)
    {
      //TODO: what to return?
      var parms = new Dictionary<string, string>();
      const string methodName = "album.getInfo";
      parms.Add("artist", strArtist);
      parms.Add("album", strAlbum);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      var lastFMResponseXML = HttpGet(buildLastFMString);

      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
      }

      var xDoc = XDocument.Parse(lastFMResponseXML);
    }
    
    #endregion

    #region user methods

    /// <summary>
    /// Get details of the current user
    /// </summary>
    /// <returns>User details returned from last.fm</returns>
    public static LastFMUser GetUserInfo()
    {
      // Last.fm API states that if no user is specified that details of the current user will be returned
      // This is not quite true as it only works if the user is authenticated and the getUserInfo API
      // call is by default not authenticated.   Passing an empty string to internal function will authenticate
      // the call so this will return details of current user
      return GetUserInfo(string.Empty);
    }

    /// <summary>
    /// Get details of named user
    /// </summary>
    /// <param name="strUser">Name of user</param>
    /// <returns>User details returned from last.fm</returns>
    public static LastFMUser GetUserInfo(string strUser)
    {
      Log.Debug("LastFM.GetUserInfo: get info for: {0}", strUser);

      var parms = new Dictionary<string, string>();
      const string methodName = "user.getInfo";
      if (!String.IsNullOrEmpty(strUser))
      {
        parms.Add("user", strUser);
      }
      else
      {
        parms.Add("sk", _sessionKey);
      }
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, false);

      var lastFMResponseXML = HttpGet(buildLastFMString);

      if (!IsValidReponse(lastFMResponseXML))
      {
        Log.Error("Invalid Response from last.fm request");
        Log.Error("{0}", lastFMResponseXML);
      }

      var xDoc = XDocument.Parse(lastFMResponseXML);

      //TODO: This needs to be descendants ??
      var user = xDoc.Root.Element("user");
      if (user == null) return null;

      var userName = (string) user.Element("name");
      var subscriber = ((string) user.Element("subscriber")) == "1";
      int playcount;
      int.TryParse((string) user.Element("playcount"), out playcount);
      var userImgURL = (from img in user.Elements("image")
                        where (string) img.Attribute("size") == "medium"
                        select img.Value).First();

      var lastFMUser = new LastFMUser
                         {
                           Username = userName,
                           UserImgURL = userImgURL,
                           Subscriber = subscriber,
                           PlayCount = playcount
                         };

      return lastFMUser;

    }

    #endregion

    #region HTTP methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="querystring"></param>
    /// <returns></returns>
    private static XDocument GetXml(string querystring)
    {
      var xDoc = new XDocument();
      try
      {
        xDoc = XDocument.Load(BaseURL + "?" + querystring);
      }
      catch (Exception e)
      {
        Log.Error(e);
        throw;
      }


      return xDoc;
    }


    /// <summary>
    /// Calls the last.fm API using a HTTP GET call
    /// </summary>
    /// <param name="querystring">querystring to pass to server</param>
    /// <returns>Text response from server</returns>
    private static string HttpGet(string querystring)
    {
      
      // TODO this should be replaced with a simple call to XDocument.Load (only for GET calls)
      var lastFMResponse = String.Empty;
      var myWebClient = new WebClient {Encoding = Encoding.UTF8};
      try
      {
        var st = myWebClient.OpenRead(BaseURL + "?" + querystring);
        if (st != null)
        {
          var sr = new StreamReader(st, Encoding.UTF8);
          lastFMResponse = sr.ReadToEnd();
        }
      }
      catch (WebException ex)
      {
        // last.fm returns a HTTP error if not successful but still returns a response with details of the error.
        var res = (HttpWebResponse)ex.Response;
        var st = res.GetResponseStream();
        if (st != null)
        {
          var reader = new StreamReader(st);
          var ttt = reader.ReadToEnd();
          lastFMResponse = ttt;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      finally
      {
        myWebClient.Dispose();
      }

#if DEBUG
      Log.Debug(lastFMResponse);
#endif

      return lastFMResponse;
    }

    /// <summary>
    /// Calls the last.fm API using a HTTP POST call
    /// </summary>
    /// <param name="postData">data to be sent to server</param>
    /// <returns>Text response from server</returns>
    private static string HttpPost(string postData)
    {
      return HttpPost(postData, false);
    }

    private static string HttpPost(string postData, bool useHttps)
    {
      var lastFMResponse = String.Empty;
      var postArray = Encoding.UTF8.GetBytes(postData);
      using(var myWebClient = new WebClient {Encoding = Encoding.UTF8})
      {
        try
        {
          myWebClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

          var url = useHttps ? BaseURLHttps : BaseURL;
          var responseArray = myWebClient.UploadData(url, postArray);
          lastFMResponse = Encoding.UTF8.GetString(responseArray);
        }
        catch (WebException ex)
        {
          try
          {
            //last.fm API returns a HTTP error is not successful but still returns a response
            var res = (HttpWebResponse) ex.Response;
            var st = res.GetResponseStream();
            if (st != null)
            {
              var reader = new StreamReader(st, Encoding.UTF8);
              var ttt = reader.ReadToEnd();
              lastFMResponse = ttt;
            }
          }
          catch (Exception)
          {
            Log.Error(ex);
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
      }

#if DEBUG
      Log.Debug("HttpPost Response");
      Log.Debug(lastFMResponse);
#endif
      //TODO should return XDocument rather than string ???
      return lastFMResponse;
    }

    /// <summary>
    /// Checks that the response from last.fm is valid and contains data
    /// </summary>
    /// <param name="lastFMResponse">xml returned from last.fm API call</param>
    /// <returns>Whether response includes a status="ok" element</returns>
    public static bool IsValidReponse(String lastFMResponse)
    {//TODO this should use XML to Linq
      return lastFMResponse.Contains("<lfm status=\"ok\">");
    }

    #endregion

    #region Database

    /// <summary>
    /// Takes a list of tracks supplied by last.fm and matches them to tracks in the database
    /// </summary>
    /// <param name="tracks">List of last FM tracks to check</param>
    /// <returns>List of matched songs from input that exist in the users database</returns>
    public static List<Song> GetSimilarTracksInDatabase(List<LastFMTrack> tracks)
    {
      // list contains songs which exist in users collection
      var dbTrackListing = new List<Song>();

      //identify which are available in users collection (ie. we can use they for auto DJ mode)
      foreach (var strSQL in tracks.Select(track => String.Format("select * from tracks where strartist like '%| {0} |%' and strTitle = '{1}'",
                                                                  DatabaseUtility.RemoveInvalidChars(track.ArtistName),
                                                                  DatabaseUtility.RemoveInvalidChars(track.TrackTitle))))
      {
        List<Song> trackListing;
        MusicDatabase.Instance.GetSongsBySQL(strSQL, out trackListing);

        dbTrackListing.AddRange(trackListing);
      }

      return dbTrackListing;
    }

    #endregion

  }


  internal static class ExtensionMethods
  {
    /// <summary>
    /// LINQ extension method to chunk a collection into smaller lists of a fixed size
    /// Eg. last.fm submissions can contain at most 50 tracks but there might be more than
    /// 50 tracks to submit so chunk the input into n lists of max size 50
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">Collection to split into chunks</param>
    /// <param name="max">Max number of elements list can contain</param>
    /// <returns>Lists that have at most max elements</returns>
    internal static IEnumerable<List<T>> InSetsOf<T>(this IEnumerable<T> source, int max)
    {
      var toReturn = new List<T>(max);
      foreach (var item in source)
      {
        toReturn.Add(item);
        if (toReturn.Count != max) continue;
        yield return toReturn;
        toReturn = new List<T>(max);
      }
      if (toReturn.Any())
      {
        yield return toReturn;
      }
    }
  }

}
