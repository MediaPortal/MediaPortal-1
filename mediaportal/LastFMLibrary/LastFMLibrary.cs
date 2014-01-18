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
using System.Xml.Linq;

namespace MediaPortal.LastFM
{
  public class LastFMLibrary
  {

    private static string _sessionKey = string.Empty;
    private static string _currentUser = string.Empty;
    internal static string BaseURL = "http://ws.audioscrobbler.com/2.0/";
    internal static string BaseURLHttps = "https://ws.audioscrobbler.com/2.0/";

    #region ctor

    public LastFMLibrary(string sessionkey, string currentUser)
    {
      _sessionKey = sessionkey;
      _currentUser = currentUser;
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

    #region lastFM Authentication

    /// <summary>
    /// Gets a session key (lasts forever) to identify a user to last.fm
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <returns>The session key</returns>
    /// <exception cref="LastFMException">when things go wrong.</exception>
    public static string AuthGetMobileSession(string username, string password)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "auth.getMobileSession";
      parms.Add("username", username);
      parms.Add("password", password);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      var xDoc = GetXml(buildLastFMString, "POST", true);

      if (xDoc != null)
      {
        var sk = xDoc.Descendants("key").FirstOrDefault();
        if (sk != null) _sessionKey = sk.Value;
        _currentUser = username;
      }

      return _sessionKey;

    }

    #endregion

    #region scrobbling methods

    /// <summary>
    /// Announce track as now playing on user profile on last.fm website
    /// </summary>
    /// <param name="artist">artist of track being played</param>
    /// <param name="track">name of track being played</param>
    /// <param name="album">album track being played is part of</param>
    /// <param name="duration">duration of track being played</param>
    /// <exception cref="LastFMException">when things go wrong.</exception>
    public static void UpdateNowPlaying(string artist, string track, string album, string duration)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.updateNowPlaying";
      parms.Add("artist", artist);
      parms.Add("track", track);
      if (!String.IsNullOrEmpty(album))
      {
        parms.Add("album", album);
      }
      if (!String.IsNullOrEmpty(duration))
      {
        parms.Add("duration", duration);
      }
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      GetXml(buildLastFMString, "POST", false);
    }


    /// <summary>
    /// Scrobble track to last.fm showing as played on user profile on last.fm website.   
    /// Assumes track is selected by user and scrobble is for current time
    /// </summary>
    /// <param name="artist">artist of track that was played</param>
    /// <param name="trackTitle">name of track that was played</param>
    /// <param name="album">album that track that was played is part of</param>
    /// <exception cref="LastFMException">when things go wrong.</exception>
    public static void Scrobble(string artist, string trackTitle, string album)
    {
      Scrobble(artist,trackTitle,album,true,DateTime.UtcNow);
    }

    /// <summary>
    /// Scrobble track to last.fm showing as played on user profile on last.fm website
    /// </summary>
    /// <param name="artist">artist of track that was played</param>
    /// <param name="trackTitle">name of track that was played</param>
    /// <param name="album">album that track that was played is part of</param>
    /// <param name="isUserSubmitted">True if track was selected by user or false if by system (radio / auto DJ etc)</param> 
    /// <param name="dtPlayed">Date track was played</param>
    /// <exception cref="LastFMException">when things go wrong.</exception>
    public static void Scrobble(string artist, string trackTitle, string album, bool isUserSubmitted,
                                DateTime dtPlayed)
    {
      var track = new LastFMScrobbleTrack
        {
          ArtistName = artist,
          TrackTitle = trackTitle,
          AlbumName = album,
          DatePlayed = dtPlayed,
          UserSelected = isUserSubmitted
        };

      var tracks = new List<LastFMScrobbleTrack> {track};
      ScrobbleTracks(tracks);
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
    /// <exception cref="LastFMException">when things go wrong.</exception>
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
        parms.Add(string.Format("track[{0}]", i), track.TrackTitle);
        if (!String.IsNullOrEmpty(track.AlbumName))
        {
          parms.Add(string.Format("album[{0}]", i), track.AlbumName);
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
      GetXml(buildLastFMString, "POST", false);
    }

    #endregion

    #region radio methods

    /// <summary>
    /// Tune to a radio station.   After runing call GetRadioPlaylist to get the track listing
    /// </summary>
    /// <param name="stationURL"></param>
    public static bool TuneRadio(string stationURL)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "radio.tune";
      parms.Add("station", stationURL);
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      GetXml(buildLastFMString, "POST", false);

      return true;
    }

    /// <summary>
    /// Gets the playlist of radio station (will only be a small number of tracks)
    /// </summary>
    /// <returns>A list of tracks</returns>
    public static List<LastFMStreamingTrack> GetRadioPlaylist()
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "radio.getPlaylist";
      parms.Add("bitrate", "128");
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      var xDoc = GetXml(buildLastFMString, "GET", false);

      if (xDoc != null)
      {
        XNamespace ns = "http://xspf.org/ns/0/";
        var tracks = (from a in xDoc.Descendants(ns + "track")
                      select new LastFMStreamingTrack
                               {
                                 ArtistName = (string) a.Element(ns + "creator"),
                                 TrackTitle = (string) a.Element(ns + "title"),
                                 TrackStreamingURL = (string) a.Element(ns + "location"),
                                 Duration = Int32.Parse((string) a.Element(ns + "duration"))/1000,
                                 Identifier = Int32.Parse((string) a.Element(ns + "identifier")),
                                 ImageURL = (string) a.Element(ns + "image")
                               }).ToList();
        return tracks;
      }
      return null;
    }

    #endregion

    #region track methods

    /// <summary>
    /// Get info about the track
    /// </summary>
    /// <param name="artist">Artist Name</param>
    /// <param name="track">Track Title</param>
    public static LastFMTrackInfo GetTrackInfo(string artist, string track)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.getInfo";
      parms.Add("artist", artist);
      parms.Add("track", track);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      var xDoc = GetXml(buildLastFMString, "GET", false);
      var trackInfo = new LastFMTrackInfo(xDoc);

      return trackInfo;
    }


    /// <summary>
    /// Pickup similar tracks to the details provided.   These tracks may not
    /// </summary>
    /// <param name="track">name of track to use for lookup</param>
    /// <param name="artist">artist of track to use for lookup</param>
    /// <returns>A list of similar tracks from last.fm.  
    ///          There is no check to see if the user has access to these tracks
    ///          See GetSimilarTracksInDatabase to only return tracks in music database</returns>
    public static List<LastFMSimilarTrack> GetSimilarTracks(string track, string artist)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.getSimilar";
      parms.Add("track", track);
      parms.Add("artist", artist);
      parms.Add("autocorrect", "1");

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, false);
      var xDoc = GetXml(buildLastFMString, "GET", false);

      if (xDoc != null)
      {
        return LastFMSimilarTrack.GetSimilarTracks(xDoc);
      }
      return null;
    }

    /// <summary>
    /// Marks a track as loved on last.fm
    /// </summary>
    /// <param name="artist">Artist Name</param>
    /// <param name="track">Track Title</param>
    /// <returns>True if successful</returns>
    public static bool LoveTrack(string artist, string track)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.love";
      parms.Add("artist", artist);
      parms.Add("track", track);
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      GetXml(buildLastFMString, "POST", false);

      return true;
    }

    /// <summary>
    /// Unmarks a track as loved on last.fm
    /// </summary>
    /// <param name="artist">Artist Name</param>
    /// <param name="track">Track Title</param>
    /// <returns>True if successful</returns>
    public static bool UnLoveTrack(string artist, string track)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.unlove";
      parms.Add("artist", artist);
      parms.Add("track", track);
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      GetXml(buildLastFMString, "POST", false);

      return true;
    }

    /// <summary>
    /// Marks track as banned on last.fm
    /// </summary>
    /// <param name="artist">Artist Name</param>
    /// <param name="track">Track Title</param>
    /// <returns>True if successful</returns>
    public static bool BanTrack(string artist, string track)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.ban";
      parms.Add("artist", artist);
      parms.Add("track", track);
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      GetXml(buildLastFMString, "POST", false);

      return true;
    }

    /// <summary>
    /// Unmarks a track as banned on last.fm
    /// </summary>
    /// <param name="artist">Artist Name</param>
    /// <param name="track">Track Title</param>
    /// <returns>True if successful</returns>
    public static bool UnBanTrack(string artist, string track)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "track.unban";
      parms.Add("artist", artist);
      parms.Add("track", track);
      parms.Add("sk", _sessionKey);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      GetXml(buildLastFMString, "POST", false);

      return true;
    }

    #endregion

    #region artist methods

    /// <summary>
    /// Get artist details from last.fm
    /// </summary>
    /// <param name="artist">Artist Name</param>
    public static LastFMFullArtist GetArtistInfo(string artist)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "artist.getInfo";
      parms.Add("artist", artist);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      var xDoc = GetXml(buildLastFMString, "GET", false);
      var lastFMFullArtist = new LastFMFullArtist(xDoc);

      return lastFMFullArtist;
    }

    /// <summary>
    /// Get top tracks for artist
    /// </summary>
    /// <param name="artist">artist to lookup</param>
    /// <returns>List of top tracks</returns>
    public static List<LastFMSimilarTrack> GetArtistTopTracks(string artist)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "artist.getTopTracks";
      parms.Add("artist", artist);
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      var xDoc = GetXml(buildLastFMString, "GET", false);

      if (xDoc != null)
      {
        return LastFMSimilarTrack.GetSimilarTracks(xDoc);
      }
      return null;
    }

    #endregion

    #region album methods

    /// <summary>
    /// Get album details
    /// </summary>
    /// <param name="artist">Artist Naae</param>
    /// <param name="album">Album Name</param>
    /// <exception cref="LastFMException">when things go wrong.</exception>
    public static LastFMAlbum GetAlbumInfo(string artist, string album)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "album.getInfo";
      parms.Add("artist", artist);
      parms.Add("album", album);

      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, true);
      var xDoc = GetXml(buildLastFMString, "GET", false);
      var lastFMAlbum = new LastFMAlbum(xDoc);

      return lastFMAlbum;
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
    /// <param name="username">Name of user</param>
    /// <returns>User details returned from last.fm</returns>
    public static LastFMUser GetUserInfo(string username)
    {
      var parms = new Dictionary<string, string>();
      const string methodName = "user.getInfo";
      if (!String.IsNullOrEmpty(username))
      {
        parms.Add("user", username);
      }
      else
      {
        parms.Add("sk", _sessionKey);
      }
      var buildLastFMString = LastFMHelper.LastFMHelper.BuildLastFMString(parms, methodName, false);
      var xDoc = GetXml(buildLastFMString, "GET", false);
      var lastFMUser = new LastFMUser(xDoc);

      return lastFMUser;
    }

    #endregion

    #region HTTP methods

    /// <summary>
    /// Attempt to get XML web response via HTTP
    /// </summary>
    /// <param name="querystring">Querystring to be passed to webservice</param>
    /// <param name="httpMethod">GET or POST</param>
    /// <param name="useHttps">Whether to use HTTPS</param>
    /// <returns>The xml returned by Webservice</returns>
    /// <exception cref="LastFMException">Details of last.fm error or will wrap actual exception as inner exception</exception>
    private static XDocument GetXml(string querystring, string httpMethod, bool useHttps)
    {
      HttpWebResponse response;
      XDocument xDoc;
      var url = useHttps ? BaseURLHttps : BaseURL;
      if (httpMethod == "GET")
      {
        url = url + "?" + querystring;
      }

      bool webExceptionStatus = false;
      var postArray = Encoding.UTF8.GetBytes(querystring);
      var request = (HttpWebRequest) WebRequest.Create(url);
      request.Method = httpMethod;
      request.ServicePoint.Expect100Continue = false;
      if (httpMethod == "POST")
      {
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = postArray.Length;
        var s = request.GetRequestStream();
        s.Write(postArray, 0, postArray.Length);
        s.Close();
      }
      try
      {
        response = (HttpWebResponse) request.GetResponse();
      }
      catch (WebException ex)
      {
        if (ex.Status == WebExceptionStatus.ProtocolError)
        {
          // errors on last.fm side such as invalid API key, are returned as HTTP errors
          // just process these as a standard return
          response = (HttpWebResponse) ex.Response;
          //webExceptionStatus = true;
        }
        else
        {
          throw;
        }
      }

      if (!webExceptionStatus)
      {
        using (var stream = response.GetResponseStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        {
          var resp = reader.ReadToEnd();
          xDoc = XDocument.Parse(resp);
        }

        if ((string) xDoc.Root.Attribute("status") != "ok")
        {
          throw GetLastFMException(xDoc);
        }
        return xDoc;
      }
      return null;
    }

    #endregion

    #region exception handling

    /// <summary>
    /// Parse xml error returned from last.fm and convert into an exception
    /// </summary>
    /// <param name="xDoc">xml error returned from last.fm</param>
    /// <returns></returns>
    private static LastFMException GetLastFMException(XContainer xDoc)
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
