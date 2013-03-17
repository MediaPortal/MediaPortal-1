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

      XDocument xDoc;
      try
      {
        xDoc = GetXmlPost(buildLastFMString, true);
      }
      catch (LastFMException ex)
      {
        Log.Error("Error Getting Last.FM Session");
        Log.Error("Error: {0} - {1}", ex.LastFMError, ex.Message);
        return false;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        return false;
      }

      var sk = xDoc.Descendants("key").FirstOrDefault();

      Log.Info("Saving last.fm session key for: {0}", username);

      if (sk != null) _sessionKey = sk.Value;
      _currentUser = username;

      return MusicDatabase.Instance.AddLastFMUser(username, _sessionKey);

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

      try
      {
        GetXmlPost(buildLastFMString);
      }
      catch (LastFMException ex)
      {
        Log.Error("Error updating now playing status update for: {0} - {1}", strArtist, strTrack);
        Log.Error("Error returned was: {0} - {1}", ex.LastFMError, ex.Message);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }

      Log.Info("Submitted last.fm now playing update for: {0} - {1}", strArtist, strTrack);
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

      try
      {
        GetXmlPost(buildLastFMString);
      }
      catch (LastFMException ex)
      {
        Log.Error("Error scrobbling track(s)");
        Log.Error("Error was: {0} - {1}", ex.LastFMError, ex.Message);
        //TODO need to handle offline scrobbling here and cache failed scrobbles
        throw;
      }

      foreach (var track in scrobbleTracks)
      {
        Log.Info("Submitted last.fm scrobble for: {0}-{1} @{3} - {4}", track.ArtistName, track.AlbumName,
                 track.DatePlayed.ToShortDateString(), track.DatePlayed.ToLongTimeString());
      }

    }

    private static void CacheScrobble(string strArtist, string strTrack, String strAlbum, bool isUserSubmitted, DateTime dtPlayed)
    {
      //TODO: write to cache file (or database?)
    }

    public static void SumbitCachedScrobbles()
    {
      var tracks = new List<LastFMScrobbleTrack>();
      //TODO: code to load cached tracks
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

      try
      {
        var xDoc = GetXmlPost(buildLastFMString);
      }
      catch (LastFMException ex)
      {
        //TODO: need to just throw and let client handle ??
        // or check for situations such as not subscriber and then throw ???
        Log.Error("Error in Tune Radio: {0} - {1}", ex.LastFMError, ex.Message);
        return false;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
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

      XDocument xDoc;
      try
      {
        xDoc = GetXml(buildLastFMString);
      }
      catch (LastFMException ex)
      {
        //Should not get here as only get playlist after successful tune???
        Log.Error(ex);
        return false;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        return false;
      }


      XNamespace ns = "http://xspf.org/ns/0/";
      var z = (from a in xDoc.Descendants(ns + "track")
               select new LastFMStreamingTrack
                        {
                          ArtistName = (string) a.Element(ns + "creator"),
                          TrackTitle = (string) a.Element(ns + "title"),
                          TrackStreamingURL = (string) a.Element(ns + "location"),
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
    public static LastFMTrackInfo GetTrackInfo(string strArtist, string strTrack)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.getInfo";
      parms.Add("artist", strArtist);
      parms.Add("track", strTrack);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      LastFMTrackInfo trackInfo;
      try
      {
        var xDoc = GetXml(buildLastFMString);
        trackInfo = new LastFMTrackInfo(xDoc);
      }
      catch (Exception e)
      {
        Log.Error(e);
        return null;
      }
      
      return trackInfo;
    }


    /// <summary>
    /// Pickup similar tracks to the details provided.   These tracks may not
    /// </summary>
    /// <param name="strTrack">name of track to use for lookup</param>
    /// <param name="strArtist">artist of track to use for lookup</param>
    /// <returns>A list of similar tracks from last.fm.  
    ///          There is no check to see if the user has access to these tracks
    ///          See GetSimilarTracksInDatabase to only return tracks in music database</returns>
    public static List<LastFMSimilarTrack> GetSimilarTracks(string strTrack, string strArtist)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.getSimilar";
      parms.Add("track", strTrack);
      parms.Add("artist", strArtist);
      parms.Add("autocorrect", "1");

      XDocument xDoc;
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, false);
      try
      {
        xDoc = GetXml(buildLastFMString);
      }
      catch (Exception e)
      {
        Log.Error(e);
        return null;
      }

      var tracks = (from t in xDoc.Descendants("track")
                     let trackName = (string) t.Element("name")
                     let playcount = (int) t.Element("playcount")
                     let mbid = (string) t.Element("mbid")
                     let duration = (int) t.Element("duration")
                     let match = (float) t.Element("match")
                     let trackURL = (string) t.Element("url")
                     let artistElement = t.Element("artist")
                     where artistElement != null
                     let artistName = (string) artistElement.Element("name")
                     let images = (
                                    from i in t.Elements("image")
                                    select new LastFMImage(
                                      LastFMImage.GetImageSizeEnum((string) i.Attribute("size")),
                                      (string) i
                                      )
                                  ).ToList()
                     select new LastFMSimilarTrack
                       {
                         TrackTitle = trackName,
                         Playcount = playcount,
                         MusicBrainzID = mbid,
                         Duration = duration,
                         Match = match,
                         TrackURL = trackURL,
                         ArtistName = artistName,
                         Images = images
                       }
                    ).ToList();

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

      try
      {
        GetXmlPost(buildLastFMString);
      }
      catch (LastFMException ex)
      {
        Log.Error("Error whilst loving track: {0} - {1}", strArtist, strTrack);
        Log.Error("Error was : {0} - {1}", ex.LastFMError, ex.Message);
        return false;
      }

      Log.Info("Loved Track: {0} - {1}", strArtist, strTrack);
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

      try
      {
        GetXmlPost(buildLastFMString);
      }
      catch (LastFMException ex)
      {
        Log.Error("Error whilst loving track: {0} - {1}", strArtist, strTrack);
        Log.Error("Error was : {0} - {1}", ex.LastFMError, ex.Message);
        return false;
      }

      Log.Info("Unloved Track: {0} - {1}", strArtist, strTrack);
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

      try
      {
        GetXmlPost(buildLastFMString);
      }
      catch (LastFMException ex)
      {
        Log.Error("Error whilst loving track: {0} - {1}", strArtist, strTrack);
        Log.Error("Error was : {0} - {1}", ex.LastFMError, ex.Message);
        return false;
      }

      Log.Info("Banned Track: {0} - {1}", strArtist, strTrack);
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

      try
      {
        GetXmlPost(buildLastFMString);
      }
      catch (LastFMException ex)
      {
        Log.Error("Error whilst loving track: {0} - {1}", strArtist, strTrack);
        Log.Error("Error was : {0} - {1}", ex.LastFMError, ex.Message);
        return false;
      }

      Log.Info("Unbanned Track: {0} - {1}", strArtist, strTrack);
      return true;
    }

    #endregion

    #region artist methods

    /// <summary>
    /// Get artist details from last.fm
    /// </summary>
    /// <param name="strArtist">Artist Name</param>
    public static LastFMFullArtist GetArtistInfo(string strArtist)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "artist.getInfo";
      parms.Add("artist", strArtist);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      LastFMFullArtist artist;
      try
      {
        var xDoc = GetXml(buildLastFMString);
        artist = new LastFMFullArtist(xDoc);
      }
      catch (Exception e)
      {
        Log.Error(e);
        return null;
      }

      return artist;
    }

    #endregion

    #region album methods

    /// <summary>
    /// Get album details
    /// </summary>
    /// <param name="strArtist">Artist Naae</param>
    /// <param name="strAlbum">Album Name</param>
    public static LastFMAlbum GetAlbumInfo(string strArtist, string strAlbum)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "album.getInfo";
      parms.Add("artist", strArtist);
      parms.Add("album", strAlbum);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);

      LastFMAlbum album;
      try
      {
        var xDoc = GetXml(buildLastFMString);
        album = new LastFMAlbum(xDoc);
      }
      catch (Exception e)
      {
        Log.Error(e);
        return null;
      }

      return album;
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

      LastFMUser lastFMUser;
      try
      {
        var xDoc = GetXml(buildLastFMString);
        lastFMUser = new LastFMUser(xDoc);
      }
      catch (Exception e)
      {
        Log.Error(e);
        //TODO: Parse errors
        return null;
      }

      return lastFMUser;

    }

    #endregion

    #region HTTP methods

    /// <summary>
    /// Connect to last.fm webservice and convert results in an XDocument
    /// Only possible for HTTP GET methods (non write or radio ones)
    /// </summary>
    /// <param name="querystring">method signature required by last.fm API</param>
    /// <returns>xml returned by last.fm on success</returns>
    private static XDocument GetXml(string querystring)
    {
      XDocument xDoc;
      try
      {
        xDoc = XDocument.Load(BaseURL + "?" + querystring);
      }
      catch (Exception e)
      {
        var ex = new LastFMException("Error in HTTP Get", e);
        throw ex;
      }

      if ((string) xDoc.Root.Attribute("status") != "ok")
      {
        throw GetException(xDoc);
      }

      return xDoc;

    }


    /// <summary>
    /// Connect to last.fm webservice and convert results in an XDocument
    /// via HTTP POST request using standard HTTP
    /// </summary>
    /// <param name="postData">data to be sent to last.fm web service</param>
    /// <returns>xml returned by last.fm on success</returns>
    private static XDocument GetXmlPost(string postData)
    {
      return GetXmlPost(postData, false);
    }

    /// <summary>
    /// Connect to last.fm webservice and convert results in an XDocument
    /// via HTTP POST request
    /// </summary>
    /// <param name="postData">data to be sent to last.fm web service</param>
    /// <param name="useHttps">Whether to use HTTPS or not</param>
    /// <returns>xml returned by last.fm on success</returns>
    private static XDocument GetXmlPost(string postData, bool useHttps)
    {
      var postArray = Encoding.UTF8.GetBytes(postData);
      var url = useHttps ? BaseURLHttps : BaseURL;
      HttpWebResponse response;
      XDocument xDoc;

      try
      {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = postArray.Length;
        request.ServicePoint.Expect100Continue = false;
        var s = request.GetRequestStream();
        s.Write(postArray, 0, postArray.Length);
        s.Close();
        response = (HttpWebResponse) request.GetResponse();
      }
      catch (WebException ex)
      {
        if (ex.Status == WebExceptionStatus.ProtocolError)
        {
          // errors on last.fm side such as invalid API key, are returned as HTTP errors
          // just process these as a standard return
          response = (HttpWebResponse) ex.Response;
        }
        else
        {
          throw new LastFMException("Error in HTTP Post", ex);
        }
      }

      try
      {
        var s = response.GetResponseStream();
        var sr = new StreamReader(s);
        var xml = sr.ReadToEnd();
        xDoc = XDocument.Parse(xml);
      }
      catch (Exception ex)
      {
        throw new LastFMException("Error in HTTP Post response", ex);
      }

      if ((string)xDoc.Root.Attribute("status") != "ok")
      {
        throw GetException(xDoc);
      }

      return xDoc;
    }

    #endregion

    #region Database

    /// <summary>
    /// Takes a list of tracks supplied by last.fm and matches them to tracks in the database
    /// </summary>
    /// <param name="tracks">List of last FM tracks to check</param>
    /// <returns>List of matched songs from input that exist in the users database</returns>
    public static List<Song> GetSimilarTracksInDatabase(List<LastFMSimilarTrack> tracks)
    {
      // list contains songs which exist in users collection
      var dbTrackListing = new List<Song>();

      //identify which are available in users collection (ie. we can use they for auto DJ mode)
      foreach (var strSql in tracks.Select(track => String.Format("select * from tracks where strartist like '%| {0} |%' and strTitle = '{1}'",
                                                                  DatabaseUtility.RemoveInvalidChars(track.ArtistName),
                                                                  DatabaseUtility.RemoveInvalidChars(track.TrackTitle))))
      {
        List<Song> trackListing;
        MusicDatabase.Instance.GetSongsBySQL(strSql, out trackListing);

        dbTrackListing.AddRange(trackListing);
      }

      return dbTrackListing;
    }

    #endregion

    #region exception handling

    /// <summary>
    /// Parse xml error returned from last.fm and convert into an exception
    /// </summary>
    /// <param name="xDoc">xml error returned from last.fm</param>
    /// <returns></returns>
    private static LastFMException GetException(XContainer xDoc)
    {
      //default values just in case xml is malformed or corrupted
      var errorMsg = "Last.fm Error";
      int i = 999; 
      var error = xDoc.Descendants("error").FirstOrDefault();
      if (error != null)
      {
        i = (int)error.Attribute("code");
        errorMsg = (string) error;
      }
      return new LastFMException(errorMsg) { LastFMError = (LastFMException.LastFMErrorCode)i };
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
