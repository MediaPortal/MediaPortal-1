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

#region Usings

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

using MediaPortal.Util;
using MediaPortal.Services;
using MediaPortal.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

#endregion

namespace MediaPortal.Music.Database
{
  #region argument types

  public enum lastFMFeed
  {
    recenttracks,
    weeklyartistchart,
    weeklytrackchart,
    topartists,
    toptracks,
    friends,
    neighbours,
    similar,
    toptags,
    artisttags,
    chartstoptags,
    taggedartists,
    taggedalbums,
    taggedtracks,
    topartisttags,
    toptracktags,
    albuminfo,
    systemrecs,
    profile,
    recentbannedtracks,
    recentlovedtracks,
    tasteometer
  }

  public enum offlineMode : int
  {
    random = 0,
    timesplayed = 1,
    favorites = 2,
  }

  /// <summary>
  /// Filter by Artist, Album or Track
  /// </summary>
  public enum songFilterType
  {
    Artist,
    Album,
    Track
  }

  /// <summary>
  /// One of these: loveTrack, unLoveTrack, banTrack, unBanTrack, addTrackToUserPlaylist, removeRecentlyListenedTrack, removeFriend
  /// </summary>
  public enum XmlRpcType
  {
    loveTrack,
    unLoveTrack,
    banTrack,
    unBanTrack,
    addTrackToUserPlaylist,
    removeRecentlyListenedTrack,
    removeFriend,
  }

  #endregion

  #region Async request definitions

  public class AlbumInfoRequest : ScrobblerUtilsRequest
  {
    public string ArtistToSearch;
    public string AlbumToSearch;
    public bool SortBestTracks;
    public bool MarkLocalTracks;

    public delegate void AlbumInfoRequestHandler(AlbumInfoRequest request, List<Song> songs);
    public AlbumInfoRequestHandler AlbumInfoRequestCompleted;

    public AlbumInfoRequest(string artistToSearch, string albumToSearch, bool sortBestTracks, bool markLocalTracks)
      : base(RequestType.GetAlbumInfo)
    {
      ArtistToSearch = artistToSearch;
      AlbumToSearch = albumToSearch;
      SortBestTracks = sortBestTracks;
      MarkLocalTracks = markLocalTracks;
    }
    public AlbumInfoRequest(string artistToSearch, string albumToSearch, bool sortBestTracks, bool markLocalTracks, AlbumInfoRequestHandler handler)
      : this(artistToSearch, albumToSearch, sortBestTracks, markLocalTracks)
    {
      AlbumInfoRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getAlbumInfo(ArtistToSearch, AlbumToSearch, SortBestTracks, MarkLocalTracks);
      if (AlbumInfoRequestCompleted != null)
        AlbumInfoRequestCompleted(this, songs);
    }
  }

  public class SimilarArtistRequest : ScrobblerUtilsRequest
  {
    public string ArtistToSearch;
    public bool RandomizeArtists;

    public delegate void SimilarArtistRequestHandler(SimilarArtistRequest request, List<Song> songs);
    public SimilarArtistRequestHandler SimilarArtistRequestCompleted;

    public SimilarArtistRequest(string artistToSearch, bool randomizeArtists)
      : base(RequestType.GetSimilarArtists)
    {
      ArtistToSearch = artistToSearch;
      RandomizeArtists = randomizeArtists;
    }
    public SimilarArtistRequest(string artistToSearch, bool randomizeArtists, SimilarArtistRequestHandler handler)
      : this(artistToSearch, randomizeArtists)
    {
      SimilarArtistRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getSimilarArtists(ArtistToSearch, RandomizeArtists);
      if (SimilarArtistRequestCompleted != null)
        SimilarArtistRequestCompleted(this, songs);
    }
  }


  public class ArtistInfoRequest : ScrobblerUtilsRequest
  {
    public string ArtistToSearch;

    public delegate void ArtistInfoRequestHandler(ArtistInfoRequest request, Song song);
    public ArtistInfoRequestHandler ArtistInfoRequestCompleted;

    public ArtistInfoRequest(string artistToSearch)
      : base(RequestType.GetArtistInfo)
    {
      ArtistToSearch = artistToSearch;
    }
    public ArtistInfoRequest(string artistToSearch, ArtistInfoRequestHandler handler)
      : this(artistToSearch)
    {
      ArtistInfoRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      Song song = AudioscrobblerUtils.Instance.getArtistInfo(ArtistToSearch);
      if (ArtistInfoRequestCompleted != null)
        ArtistInfoRequestCompleted(this, song);
    }
  }

  public class TagInfoRequest : ScrobblerUtilsRequest
  {
    public string ArtistToSearch;
    public string TrackToSearch;
    public bool RandomizeUsedTag;
    public bool SortBestTracks;
    public bool AddAvailableTracksOnly;

    public delegate void TagInfoRequestHandler(TagInfoRequest request, List<Song> songs);
    public TagInfoRequestHandler TagInfoRequestCompleted;

    public TagInfoRequest(string artistToSearch, string trackToSearch, bool randomizeUsedTag, bool sortBestTracks, bool addAvailableTracksOnly)
      : base(RequestType.GetAlbumInfo)
    {
      ArtistToSearch = artistToSearch;
      TrackToSearch = trackToSearch;
      RandomizeUsedTag = randomizeUsedTag;
      SortBestTracks = sortBestTracks;
      AddAvailableTracksOnly = addAvailableTracksOnly;
    }
    public TagInfoRequest(string artistToSearch, string trackToSearch, bool randomizeUsedTag, bool sortBestTracks, bool addAvailableTracksOnly, TagInfoRequestHandler handler)
      : this(artistToSearch, trackToSearch, randomizeUsedTag, sortBestTracks, addAvailableTracksOnly)
    {
      TagInfoRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getTagInfo(ArtistToSearch, TrackToSearch, RandomizeUsedTag, SortBestTracks, AddAvailableTracksOnly);
      if (TagInfoRequestCompleted != null)
        TagInfoRequestCompleted(this, songs);
    }
  }

  // public List<Song> getTagsForTrack(string artistToSearch_, string trackToSearch_)
  public class TagsForTrackRequest : ScrobblerUtilsRequest
  {
    public string ArtistToSearch;
    public string TrackToSearch;

    public delegate void TagsForTrackRequestHandler(TagsForTrackRequest request, List<Song> songs);
    public TagsForTrackRequestHandler TagsForTrackRequestCompleted;

    public TagsForTrackRequest(string artistToSearch, string trackToSearch)
      : base(RequestType.GetTagsForTrack)
    {
      ArtistToSearch = artistToSearch;
      TrackToSearch = trackToSearch;
    }
    public TagsForTrackRequest(string artistToSearch, string trackToSearch, TagsForTrackRequestHandler handler)
      : this(artistToSearch, trackToSearch)
    {
      TagsForTrackRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getTagsForTrack(ArtistToSearch, TrackToSearch);
      if (TagsForTrackRequestCompleted != null)
        TagsForTrackRequestCompleted(this, songs);
    }
  }

  public class UsersTagsRequest : ScrobblerUtilsRequest
  {
    public string UserForFeed;

    public delegate void UsersTagsRequestHandler(UsersTagsRequest request, List<Song> songs);
    public UsersTagsRequestHandler UsersTagsRequestCompleted;

    public UsersTagsRequest(string userForFeed)
      : base(RequestType.GetAudioScrobblerFeed)
    {
      UserForFeed = userForFeed;
    }
    public UsersTagsRequest(string userForFeed, UsersTagsRequestHandler handler)
      : this(userForFeed)
    {
      UsersTagsRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getAudioScrobblerFeed(lastFMFeed.toptags, UserForFeed);
      if (UsersTagsRequestCompleted != null)
        UsersTagsRequestCompleted(this, songs);
    }
  }

  public class UsersFriendsRequest : ScrobblerUtilsRequest
  {
    public string UserForFeed;

    public delegate void UsersFriendsRequestHandler(UsersFriendsRequest request, List<Song> songs);
    public UsersFriendsRequestHandler UsersFriendsRequestCompleted;

    public UsersFriendsRequest(string userForFeed)
      : base(RequestType.GetAudioScrobblerFeed)
    {
      UserForFeed = userForFeed;
    }
    public UsersFriendsRequest(string userForFeed, UsersFriendsRequestHandler handler)
      : this(userForFeed)
    {
      UsersFriendsRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getAudioScrobblerFeed(lastFMFeed.friends, UserForFeed);
      if (UsersFriendsRequestCompleted != null)
        UsersFriendsRequestCompleted(this, songs);
    }
  }

  public class GeneralFeedRequest : ScrobblerUtilsRequest
  {
    public lastFMFeed FeedToSearch;
    public string UserForFeed;

    public delegate void GeneralFeedRequestHandler(GeneralFeedRequest request, List<Song> songs);
    public GeneralFeedRequestHandler GeneralFeedRequestCompleted;

    public GeneralFeedRequest(lastFMFeed feedToSearch, string userForFeed)
      : base(RequestType.GetAudioScrobblerFeed)
    {
      FeedToSearch = feedToSearch;
      UserForFeed = userForFeed;
    }
    public GeneralFeedRequest(lastFMFeed feedToSearch, string userForFeed, GeneralFeedRequestHandler handler)
      : this(feedToSearch, userForFeed)
    {
      GeneralFeedRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getAudioScrobblerFeed(FeedToSearch, UserForFeed);
      if (GeneralFeedRequestCompleted != null)
        GeneralFeedRequestCompleted(this, songs);
    }
  }

  public class NeighboursArtistsRequest : ScrobblerUtilsRequest
  {
    bool _randomizeList;

    public delegate void NeighboursArtistsRequestHandler(NeighboursArtistsRequest request, List<Song> songs);
    public NeighboursArtistsRequestHandler NeighboursArtistsRequestCompleted;

    public NeighboursArtistsRequest(bool randomizeList)
      : base(RequestType.GetNeighboursArtists)
    {
      _randomizeList = randomizeList;
    }
    public NeighboursArtistsRequest(bool randomizeList, NeighboursArtistsRequestHandler handler)
      : this(randomizeList)
    {
      NeighboursArtistsRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getNeighboursArtists(_randomizeList);
      if (NeighboursArtistsRequestCompleted != null)
        NeighboursArtistsRequestCompleted(this, songs);
    }
  }

  public class FriendsArtistsRequest : ScrobblerUtilsRequest
  {
    bool _randomizeList;

    public delegate void FriendsArtistsRequestHandler(FriendsArtistsRequest request, List<Song> songs);
    public FriendsArtistsRequestHandler FriendsArtistsRequestCompleted;

    public FriendsArtistsRequest(bool randomizeList)
      : base(RequestType.GetFriendsArtists)
    {
      _randomizeList = randomizeList;
    }
    public FriendsArtistsRequest(bool randomizeList, FriendsArtistsRequestHandler handler)
      : this(randomizeList)
    {
      FriendsArtistsRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getFriendsArtists(_randomizeList);
      if (FriendsArtistsRequestCompleted != null)
        FriendsArtistsRequestCompleted(this, songs);
    }
  }

  public class RandomTracksRequest : ScrobblerUtilsRequest
  {
    public delegate void RandomTracksRequestHandler(RandomTracksRequest request, List<Song> songs);
    public RandomTracksRequestHandler RandomTracksRequestCompleted;

    public RandomTracksRequest()
      : base(RequestType.GetRandomTracks) { }
    public RandomTracksRequest(RandomTracksRequestHandler handler)
      : this()
    {
      RandomTracksRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getRandomTracks();
      if (RandomTracksRequestCompleted != null)
        RandomTracksRequestCompleted(this, songs);
    }
  }

  public class UnheardTracksRequest : ScrobblerUtilsRequest
  {
    public delegate void UnheardTracksRequestHandler(UnheardTracksRequest request, List<Song> songs);
    public UnheardTracksRequestHandler UnheardTracksRequestCompleted;

    public UnheardTracksRequest()
      : base(RequestType.GetUnhearedTracks) { }
    public UnheardTracksRequest(UnheardTracksRequestHandler handler)
      : this()
    {
      UnheardTracksRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getUnhearedTracks();
      if (UnheardTracksRequestCompleted != null)
        UnheardTracksRequestCompleted(this, songs);
    }
  }

  public class FavoriteTracksRequest : ScrobblerUtilsRequest
  {
    public delegate void FavoriteTracksRequestHandler(FavoriteTracksRequest request, List<Song> songs);
    public FavoriteTracksRequestHandler FavoriteTracksRequestCompleted;

    public FavoriteTracksRequest()
      : base(RequestType.GetFavoriteTracks) { }
    public FavoriteTracksRequest(FavoriteTracksRequestHandler handler)
      : this()
    {
      FavoriteTracksRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getFavoriteTracks();
      if (FavoriteTracksRequestCompleted != null)
        FavoriteTracksRequestCompleted(this, songs);
    }
  }

  public class XspfPlaylistRequest : ScrobblerUtilsRequest
  {
    public bool AttemptRetryOnHttpError;
    public string UrlWithSession;
    public string ListName;
    public bool StartPlayback;

    public delegate void XspfPlaylistRequestHandler(XspfPlaylistRequest request, List<Song> songs, string listname, bool startnow);
    public XspfPlaylistRequestHandler XspfPlaylistRequestCompleted;

    public XspfPlaylistRequest(bool attemptRetryOnHttpError, string urlWithSession, bool startPlay)
      : base(RequestType.GetRadioPlaylist)
    {
      AttemptRetryOnHttpError = attemptRetryOnHttpError;
      UrlWithSession = urlWithSession;
      StartPlayback = startPlay;
    }
    public XspfPlaylistRequest(bool attemptRetryOnHttpError, string urlWithSession, bool startPlay, XspfPlaylistRequestHandler handler)
      : this(attemptRetryOnHttpError, urlWithSession, startPlay)
    {
      XspfPlaylistRequestCompleted += handler;
    }
    public override void PerformRequest()
    {
      List<Song> songs = AudioscrobblerUtils.Instance.getRadioPlaylist(AttemptRetryOnHttpError, UrlWithSession, out ListName);
      if (XspfPlaylistRequestCompleted != null)
        XspfPlaylistRequestCompleted(this, songs, ListName, StartPlayback);
    }
  }

  #endregion

  public class AudioscrobblerUtils
  {
    #region Member variables

    private bool _useDebugLog = false;
    private bool _decodeUtf8 = false;
    private string _defaultUser = "";
    private Object LookupLock;

    // Similar mode intelligence params
    private static int _minimumArtistMatchPercent = 50;
    private int _limitRandomListCount = 10;
    private int _randomNessPercent = 75;

    // Neighbour mode intelligence params
    private lastFMFeed _currentNeighbourMode;
    //private offlineMode _currentOfflineMode;

    private bool _running = false;
    public static readonly AudioscrobblerUtils Instance = new AudioscrobblerUtils();
    private List<ScrobblerUtilsRequest> _requestQueue = new List<ScrobblerUtilsRequest>();
    private object _queueMutex = new object();

    private delegate void AddRequestDelegate(ScrobblerUtilsRequest request);
    private delegate void RemoveRequestDelegate(ScrobblerUtilsRequest request);

    // List<Song> songList = null;
    List<String> _unwantedTags = null;

    #endregion

    #region Constructors

    /// <summary>
    /// Static constructor
    /// </summary>
    static AudioscrobblerUtils()
    {
    }

    /// <summary>
    /// ctor
    /// </summary>
    AudioscrobblerUtils()
    {
      LoadSettings();
    }

    #endregion

    #region Request queueing

    /// <summary>
    /// Adds a request to the request queue.
    /// Also starts the request processing thread if it's not running.
    /// </summary>
    /// <param name="request">ScrobblerUtilsRequest to add to the queue</param>
    public void AddRequest(ScrobblerUtilsRequest request)
    {
      if (request != null)
      {
        lock (_queueMutex)
        {
          // Add request to the request queue
          _requestQueue.Add(request);

          // Start queue processing if not already busy
          if (!_running)
          {
            _running = true;
            GlobalServiceProvider.Get<IThreadPool>().Add(delegate()
            {
              ScrobblerUtilsRequest req;
              while (_requestQueue.Count > 0)
              {
                lock (_queueMutex)
                {
                  req = _requestQueue[0];
                  _requestQueue.Remove(req);
                }
                req.PerformRequest();
              }
              lock (_queueMutex)
                _running = false;
            }, "ScrobblerUtilsRequest", QueuePriority.High, ThreadPriority.Normal);
          }
        }
      }
    }

    /// <summary>
    /// Removes a request from the request queue.
    /// </summary>
    /// <param name="request">ScrobblerUtilsRequest to remove from the queue</param>
    /// <returns></returns>
    public void RemoveRequest(ScrobblerUtilsRequest request)
    {
      if (request != null)
        lock (_queueMutex)
          _requestQueue.Remove(request);
    }

    #endregion

    #region SongComparer

    private static bool IsSongBelowMinPercentage(Song aSong)
    {
      try
      {
        if (Convert.ToDouble(aSong.LastFMMatch, System.Globalization.NumberFormatInfo.InvariantInfo) < _minimumArtistMatchPercent)
          return true;
        else
          return false;
      }
      catch (Exception ex)
      {
        Log.Warn("AudioscrobblerUtils: Could not check percentage match for Song: {0} - {1}", aSong.ToShortString(), ex.Message);
        return false;
      }
    }

    private static int CompareSongsByMatch(Song x, Song y)
    {
      try
      {
        if (x.LastFMMatch == null)
        {
          if (y.LastFMMatch == null)
          {
            // If x is null and y is null, they're equal
            return 0;
          }
          else
          {
            // If x is null and y is not null, y is greater
            return 1;
          }
        }
        else
        {
          // If x is not null...
          if (y.LastFMMatch == null)
          // ...and y is null, x is greater.
          {
            return -1;
          }
          else
          {
            // ...and y is not null, compare 
            int retval = 0;
            if (x.LastFMMatch != string.Empty && y.LastFMMatch != string.Empty)
            {
              if (Convert.ToInt32(x.LastFMMatch) < Convert.ToInt32(x.LastFMMatch))
                retval = 1;
              else
                retval = -1;
            }
            else
              return 0;

            if (retval != 0)
            {
              return retval;
            }
            else
            {
              return 0;
            }
          }
        }
      }
      catch (Exception)
      {
        return 0;
      }
    }

    private static int CompareSongsByTimesPlayed(Song x, Song y)
    {
      // ...and y is not null, compare 
      int retval = 0;
      try
      {
        if (x.TimesPlayed == 0)
          //if (y.TimesPlayed != null && y.TimesPlayed >= 0)
          //{
          //  return 1;
          //}
          //else
          return 0;

        if (y.TimesPlayed == 0)
          return 0;

        if (x.TimesPlayed == y.TimesPlayed)
          return 0;
        else
          if (x.TimesPlayed < y.TimesPlayed)
            retval = 1;
          else
            retval = -1;

        if (retval != 0)
        {
          return retval;
        }
        else
        {
          return 0;
        }
      }

      catch (Exception)
      {
        return 0;
      }
    }

    #endregion

    #region TagBlacklisting

    private List<String> buildTagBlacklist()
    {
      List<String> badTags = new List<string>();

      // these are quite too common :-(
      badTags.Add("rock");
      badTags.Add("indie");
      badTags.Add("alternative");

      // these are useless
      badTags.Add("seen live");
      badTags.Add("favorites");
      badTags.Add("favourites");
      badTags.Add("albums i own");
      badTags.Add("favorite songs");
      badTags.Add("favorite");
      badTags.Add("tracks");
      badTags.Add("good");
      badTags.Add("awesome");
      badTags.Add("favourite");
      badTags.Add("favourite songs");
      badTags.Add("live");

      return badTags;
    }

    #endregion

    #region Serialization

    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _defaultUser = xmlreader.GetValueAsString("audioscrobbler", "user", "");
        _decodeUtf8 = xmlreader.GetValueAsBool("audioscrobbler", "decodeutf8", false);
      }

      MusicDatabase mdb = MusicDatabase.Instance;
      _currentNeighbourMode = lastFMFeed.weeklyartistchart;

      _useDebugLog = (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(_defaultUser)), "iDebugLog", -1) == 1) ? true : false;
      //int tmpRMode = mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(_defaultUser)), "iOfflineMode", -1);
      int tmpRand = mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(_defaultUser)), "iRandomness", -1);
      int tmpNMode = mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(_defaultUser)), "iNeighbourMode", -1);

      switch (tmpNMode)
      {
        case 3:
          _currentNeighbourMode = lastFMFeed.topartists;
          break;
        case 1:
          _currentNeighbourMode = lastFMFeed.weeklyartistchart;
          break;
        case 0:
          _currentNeighbourMode = lastFMFeed.recenttracks;
          break;
        default:
          _currentNeighbourMode = lastFMFeed.weeklyartistchart;
          break;
      }

      _randomNessPercent = (tmpRand >= 25) ? tmpRand : 77;
      ArtistMatchPercent = 100 - (int)(0.9 * _randomNessPercent);
      _unwantedTags = buildTagBlacklist();
      LookupLock = new object();
    }

    #endregion

    #region Public getters and setters

    /// <summary>
    /// Allows to change the minimum match percentage to include similar artists
    /// </summary>
    public int ArtistMatchPercent
    {
      get
      {
        return _minimumArtistMatchPercent;
      }
      set
      {
        if (value != _minimumArtistMatchPercent)
        {
          _minimumArtistMatchPercent = value;
          if (_useDebugLog)
            Log.Info("AudioscrobblerBase: minimum match for similar artists set to {0}", Convert.ToString(_minimumArtistMatchPercent));
        }
      }
    }

    public int LimitRandomListCount
    {
      get
      {
        return _limitRandomListCount;
      }
      set
      {
        if (value != _limitRandomListCount)
        {
          _limitRandomListCount = value;
          if (_useDebugLog)
            Log.Info("AudioscrobblerBase: limit for random result lists set to {0}", Convert.ToString(_limitRandomListCount));
        }
      }
    }

    public int RandomNessPercent
    {
      get
      {
        return _randomNessPercent;
      }
      set
      {
        if (value != _randomNessPercent)
        {
          if (value == 0)
            _randomNessPercent = 1;
          else
            _randomNessPercent = value;
          if (_useDebugLog)
            Log.Info("AudioscrobblerBase: percentage of randomness set to {0}", Convert.ToString(_randomNessPercent));
        }
      }
    }

    public lastFMFeed CurrentNeighbourMode
    {
      get
      {
        return _currentNeighbourMode;
      }
      set
      {
        if (value != _currentNeighbourMode)
        {
          _currentNeighbourMode = value;
          if (_useDebugLog)
            Log.Info("AudioscrobblerBase: {0}", "CurrentNeighbourMode changed");
        }
      }
    }

    #endregion

    #region Public methods

    public Song getMusicBrainzInfo(string artistToSearch_)
    {
      string urlArtist = AudioscrobblerBase.getValidURLLastFMString(AudioscrobblerBase.UndoArtistPrefix(artistToSearch_));

      return GetMbInfoForArtist(urlArtist);
    }

    public List<Song> getAudioScrobblerFeed(lastFMFeed feed_, string asUser_)
    {
      if (string.IsNullOrEmpty(asUser_))
        asUser_ = _defaultUser;

      switch (feed_)
      {
        case lastFMFeed.recenttracks:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "recenttracks.xml", @"//recenttracks/track", feed_);
        case lastFMFeed.topartists:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "topartists.xml", @"//topartists/artist", feed_);
        case lastFMFeed.weeklyartistchart:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "weeklyartistchart.xml", @"//weeklyartistchart/artist", feed_);
        case lastFMFeed.toptracks:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "toptracks.xml", @"//toptracks/track", feed_);
        case lastFMFeed.weeklytrackchart:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "weeklytrackchart.xml", @"//weeklytrackchart/track", feed_);
        case lastFMFeed.neighbours:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "neighbours.xml", @"//neighbours/user", feed_);
        case lastFMFeed.friends:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "friends.xml", @"//friends/user", feed_);
        case lastFMFeed.toptags:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "tags.xml", @"//toptags/tag", feed_);
        case lastFMFeed.chartstoptags:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/tag/toptags.xml", @"//toptags/tag", feed_);
        case lastFMFeed.systemrecs:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "systemrecs.xml", @"//recommendations/artist", feed_);
        case lastFMFeed.profile:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "profile.xml", @"//profile", feed_);
        case lastFMFeed.recentbannedtracks:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "recentbannedtracks.xml", @"//recentbannedtracks/track", feed_);
        case lastFMFeed.recentlovedtracks:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "recentlovedtracks.xml", @"//recentlovedtracks/track", feed_);
        default:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "recenttracks.xml", @"//recenttracks/track", feed_);
      }
    }

    public List<Song> FilterNonLocalTracks(List<Song> aUnfilteredList, bool aOnlyOneTrackPerArtist)
    {
      return filterForLocalSongs(aUnfilteredList, aOnlyOneTrackPerArtist, songFilterType.Track);
    }

    public List<Song> FilterNonLocalAlbums(List<Song> aUnfilteredList, bool aOnlyOneTrackPerArtist)
    {
      return filterForLocalSongs(aUnfilteredList, aOnlyOneTrackPerArtist, songFilterType.Album);
    }

    public List<Song> FilterNonLocalArtists(List<Song> aUnfilteredList, bool aOnlyOneTrackPerArtist)
    {
      return filterForLocalSongs(aUnfilteredList, aOnlyOneTrackPerArtist, songFilterType.Artist);
    }

    private List<Song> filterForLocalSongs(List<Song> unfilteredList_, bool onlyUniqueArtists, songFilterType filterType)
    {
      try
      {
        lock (LookupLock)
        {
          MusicDatabase mdb = MusicDatabase.Instance;
          List<Song> tmpSongs = new List<Song>();

          Song tmpSong = new Song();
          bool foundDoubleEntry = false;
          string tmpArtist = string.Empty;

          for (int s = 0; s < unfilteredList_.Count; s++)
          {
            tmpArtist = unfilteredList_[s].Artist.ToLowerInvariant();
            // only accept other artists than the current playing but include current "tag" since some people tag just using the artist's name..
            //if (tmpArtist != excludeArtist_.ToLowerInvariant() || tmpArtist == currentTag_)
            //{
            switch (filterType)
            {
              case songFilterType.Track:
                {
                  Song dbSong = new Song();
                  // The filename is unique so try if we get a 100% correct result first
                  if (!string.IsNullOrEmpty(unfilteredList_[s].FileName))
                    mdb.GetSongByFileName(unfilteredList_[s].FileName, ref dbSong);
                  else
                    mdb.GetSongByMusicTagInfo(AudioscrobblerBase.StripArtistPrefix(unfilteredList_[s].Artist), unfilteredList_[s].Album, unfilteredList_[s].Title, true, ref dbSong);

                  if (!string.IsNullOrEmpty(dbSong.Artist))
                  {
                    tmpSong = dbSong.Clone();
                    // Log.Debug("Audioscrobber: Track filter for {1} found db song - {0}", tmpSong.FileName, unfilteredList_[s].Title);
                    foundDoubleEntry = false;

                    if (onlyUniqueArtists)
                    {
                      // check and prevent entries from the same artist
                      for (int j = 0; j < tmpSongs.Count; j++)
                      {
                        if (tmpSong.Artist == tmpSongs[j].Artist)
                        {
                          foundDoubleEntry = true;
                          break;
                        }
                      }
                    }

                    // new item therefore add it
                    if (!foundDoubleEntry)
                    {
                      //if (currentTag_ != string.Empty)
                      //  tmpSong.Genre = currentTag_;
                      tmpSongs.Add(tmpSong);
                    }
                  }
                  break;
                }
              case songFilterType.Artist:
                {
                  String[] artistArray = null;
                  List<Song> dbArtists = new List<Song>();
                  ArrayList artistsInDB = new ArrayList();
                  if (mdb.GetArtists(4, unfilteredList_[s].Artist, ref artistsInDB))
                  {
                    artistArray = (String[])artistsInDB.ToArray(typeof(String));
                    foreach (String singleArtist in artistArray)
                    {
                      Song addSong = new Song();
                      addSong.Artist = singleArtist;
                      dbArtists.Add(addSong);
                    }
                    // only use the first hit for now..
                    if (dbArtists.Count > 0)
                    {
                      foundDoubleEntry = false;
                      // check and prevent double entries 
                      for (int j = 0; j < tmpSongs.Count; j++)
                      {
                        if (dbArtists[0].Artist == (tmpSongs[j].Artist))
                        {
                          foundDoubleEntry = true;
                          break;
                        }
                      }
                      // new item therefore add it
                      if (!foundDoubleEntry)
                      {
                        tmpSongs.Add(unfilteredList_[s]);
                      }
                    }
                  }
                  break;
                }
              case songFilterType.Album:
                {
                  AlbumInfo[] albumArray = null;
                  List<Song> dbAlbums = new List<Song>();
                  ArrayList albumsInDB = new ArrayList();
                  if (mdb.GetAlbums(2, unfilteredList_[s].Album, ref albumsInDB))
                  {
                    albumArray = (AlbumInfo[])albumsInDB.ToArray(typeof(AlbumInfo));
                    foreach (AlbumInfo singleAlbum in albumArray)
                    {
                      Song addSong = new Song();
                      addSong.Album = singleAlbum.Album;
                      dbAlbums.Add(addSong);
                    }
                    // only use the first hit for now..
                    if (dbAlbums.Count > 0)
                    {
                      foundDoubleEntry = false;
                      // check and prevent double entries 
                      for (int j = 0; j < tmpSongs.Count; j++)
                      {
                        if (dbAlbums[0].Album == (tmpSongs[j].Album))
                        {
                          foundDoubleEntry = true;
                          break;
                        }
                      }
                      // new item therefore add it
                      if (!foundDoubleEntry)
                      {
                        tmpSongs.Add(unfilteredList_[s]);
                      }
                    }
                  }
                  break;
                }
            }
            //}
            //else
            //  Log.Debug("AudioScrobblerUtils Artist {0} inadequate - skipping", tagTracks[s].Artist);
          }
          return tmpSongs;
        }
      }
      catch (Exception ex)
      {
        Log.Error("AudioScrobblerUtils: filtering for local songs failed - {0}", ex.Message);
        return unfilteredList_;
      }
    }

    public List<Song> getTopAlbums(string artistToSearch_)
    {
      string urlArtist = AudioscrobblerBase.getValidURLLastFMString(AudioscrobblerBase.UndoArtistPrefix(artistToSearch_));
      List<Song> TopAlbums = new List<Song>(50);

      TopAlbums = ParseXMLDocForTopAlbums(urlArtist);

      foreach (Song song in TopAlbums)
      {
        song.Artist = artistToSearch_;
      }

      return TopAlbums;
    }

    /// <summary>
    /// Fetch cover link, release date and album songs sortable by their popularity
    /// </summary>
    /// <param name="artistToSearch_">Band name</param>
    /// <param name="albumToSearch_">Album name</param>
    /// <param name="sortBestTracks">false gives album songs in trackorder, true by popularity</param>
    /// <returns>Song-List of Album Tracks with Title, Artist, Album, TimesPlayed, URL(track), DateTimePlayed (album release), WebImage</returns>
    public List<Song> getAlbumInfo(string artistToSearch_, string albumToSearch_, bool sortBestTracks, bool aMarkLocalInURL)
    {
      int failover = 0;
      string urlArtist = AudioscrobblerBase.getValidURLLastFMString(AudioscrobblerBase.UndoArtistPrefix(artistToSearch_));
      string urlAlbum = AudioscrobblerBase.getValidURLLastFMString(albumToSearch_);
      List<Song> albumTracks = new List<Song>(50);
      do
      {
        albumTracks = ParseXMLDocForAlbumInfo(urlArtist, urlAlbum);

        //if (albumTracks.Count == 0)
        //{
        //  failover++;
        //  switch (failover)
        //  {
        //    case 1:
        //      urlArtist = AudioscrobblerBase.getValidURLLastFMString("The " + artistToSearch_);
        //      break;
        //    case 2:
        //      urlArtist = AudioscrobblerBase.getValidURLLastFMString(artistToSearch_);
        //      urlAlbum = AudioscrobblerBase.getValidURLLastFMString("The " + albumToSearch_);
        //      break;
        //    case 3:
        //      urlArtist = AudioscrobblerBase.getValidURLLastFMString("The " + artistToSearch_);
        //      urlAlbum = AudioscrobblerBase.getValidURLLastFMString("The " + albumToSearch_);
        //      break;
        //    case 4:
        //      urlArtist = artistToSearch_.Replace("oe", "ö");
        //      urlArtist = urlArtist.Replace("ae", "ä");
        //      urlArtist = urlArtist.Replace("ue", "ü");
        //      urlArtist = AudioscrobblerBase.getValidURLLastFMString(urlArtist);
        //      urlAlbum = albumToSearch_.Replace("oe", "ö");
        //      urlAlbum = urlAlbum.Replace("ae", "ä");
        //      urlAlbum = urlAlbum.Replace("ue", "ü");
        //      urlAlbum = AudioscrobblerBase.getValidURLLastFMString(urlAlbum);
        //      break;
        //    case 5:
        //      if (artistToSearch_.IndexOf("&") > 0)
        //        urlArtist = AudioscrobblerBase.getValidURLLastFMString(artistToSearch_.Remove(artistToSearch_.IndexOf("&")));
        //      else
        //        goto default;
        //      break;
        //    default:
        //      Log.Debug("AudioScrobblerUtils: No album info found for {0}", artistToSearch_ + " - " + albumToSearch_);
        //      failover = 0;
        //      break;
        //  }
        //}
        //else
        failover = 0;

      } while (failover != 0);

      if (albumTracks.Count > 0)
      {
        if (sortBestTracks)
          albumTracks.Sort(CompareSongsByTimesPlayed);

        if (aMarkLocalInURL)
        {
          List<Song> filteredSongs = new List<Song>(albumTracks.Count);
          filteredSongs = FilterNonLocalTracks(albumTracks, false);

          for (int i = 0; i < albumTracks.Count; i++)
          {
            albumTracks[i].URL = string.Empty;

            foreach (Song localSong in filteredSongs)
            {
              if (localSong.Artist.ToLowerInvariant() == AudioscrobblerBase.StripArtistPrefix(albumTracks[i].Artist).ToLowerInvariant() && localSong.Title.ToLowerInvariant() == albumTracks[i].Title.ToLowerInvariant())
                albumTracks[i].URL = "local";
            }
          }
        }
      }

      return albumTracks;
    }

    /// <summary>
    /// fetch other Tracks from last.fm which correspond to the tags of the given Track
    /// </summary>
    /// <param name="artistToSearch_">Artist name (can be unparsed)</param>
    /// <param name="trackToSearch_">Track name (can be unparsed)</param>
    /// <param name="randomizeUsedTag_">chose randomly between the 5 top tags</param>
    /// <param name="sortBestTracks_">do not apply randomness on track lookup</param>
    /// <param name="addAvailableTracksOnly">filter all songs not locally available</param>
    /// <returns>List of Song where Song.Genre contains the used tag</returns>
    public List<Song> getTagInfo(string artistToSearch_, string trackToSearch_, bool randomizeUsedTag_, bool sortBestTracks_, bool addAvailableTracksOnly)
    {
      int randomPosition = 0;
      int calcRandValue = 0;
      PseudoRandomNumberGenerator rand = new PseudoRandomNumberGenerator();
      string urlArtist = AudioscrobblerBase.getValidURLLastFMString(AudioscrobblerBase.UndoArtistPrefix(artistToSearch_));
      string urlTrack = AudioscrobblerBase.getValidURLLastFMString(trackToSearch_);
      string tmpGenre = string.Empty;
      List<Song> tagTracks = new List<Song>();

      // fetch the most popular Tags for the current track
      tagTracks = getTagsForTrack(urlArtist, urlTrack);

      // no tags for current track - try artist tags instead
      if (tagTracks.Count < 1)
        tagTracks = getTagsForArtist(urlArtist);

      if (tagTracks.Count > 0)
      {
        if (randomizeUsedTag_)
        {
          // decide which tag to use            
          // only use the "better" 50% for randomness
          //calcRandValue = ((tagTracks.Count / 2) - 1) * _randomNessPercent / 100;

          // only use the top 10 tags
          if (tagTracks.Count > _limitRandomListCount)
            calcRandValue = (_limitRandomListCount) * _randomNessPercent / 100;
          else
            calcRandValue = ((tagTracks.Count) - 1) * _randomNessPercent / 100;

          // make sure calcRandValue is not lower then random(minvalue, )
          calcRandValue = calcRandValue > 0 ? calcRandValue : 0;

          randomPosition = rand.Next(0, calcRandValue);
        }

        if (randomPosition < tagTracks.Count - 1)
        {
          for (int x = 0; x < _limitRandomListCount; x++)
          {
            tmpGenre = tagTracks[randomPosition].Genre.ToLowerInvariant();
            // filter unwanted tags
            if (_unwantedTags.Contains(tmpGenre.ToLowerInvariant()))
            {
              randomPosition = rand.Next(0, calcRandValue);
              // Log.Debug("AudioScrobblerUtils: Tag {0} in blacklist, randomly chosing another one", tmpGenre);
              // do not try to often..
              // if random picking doesn't lead to a result quit the randomness and pick the best
              if (x > tagTracks.Count * 3)
              {
                for (int t = 0; t < tagTracks.Count; t++)
                {
                  tmpGenre = tagTracks[t].Genre.ToLowerInvariant();
                  if (!_unwantedTags.Contains(tmpGenre.ToLowerInvariant()))
                  {
                    Log.Debug("AudioScrobblerUtils: Tag {0} was the first non-blacklisted item", tmpGenre);
                    break;
                  }

                  if (t == tagTracks.Count - 1)
                  {
                    tmpGenre = tagTracks[0].Genre.ToLowerInvariant();
                    //Log.Debug("AudioScrobblerUtils: Random tag picking unsuccessful - selecting {0}", tmpGenre);
                    break;
                  }
                }
              }
            }
            else
            {
              Log.Debug("AudioScrobblerUtils: Tag picking successful - selecting {0}", tmpGenre);
              break;
            }
          }
        }
        else
        {
          //Log.Debug("AudioScrobblerUtils: randomPosition {0} not reasonable for list of {1} tags", randomPosition, tagTracks.Count);
          if (tagTracks.Count == 1)
          {
            tmpGenre = tagTracks[0].Genre.ToLowerInvariant();
            Log.Debug("AudioScrobblerUtils: Tag {0} is the only one found - selecting..", tmpGenre);
          }
        }

        if (tmpGenre != string.Empty)
        {
          // use the best matches for the given track only            
          if (sortBestTracks_)
            tagTracks = getSimilarToTag(lastFMFeed.taggedtracks, tmpGenre, false, addAvailableTracksOnly);
          else
            tagTracks = getSimilarToTag(lastFMFeed.taggedtracks, tmpGenre, true, addAvailableTracksOnly);

          //// filter tracks not available in music database
          //if (addAvailableTracksOnly)
          //{
          //  tagTracks = filterForLocalSongs(tagTracks, artistToSearch_, tmpGenre);
          //}
        }
        else
          tagTracks.Clear();
      }

      // sort list by playcount (times a track was tagged in this case)
      if (sortBestTracks_)
        tagTracks.Sort(CompareSongsByTimesPlayed);

      foreach (Song tagSong in tagTracks)
      {
        tagSong.Genre = tmpGenre;
      }

      return tagTracks;
    }

    /// <summary>
    /// Lookup artist info from last.fm and tries to save an artist thumb
    /// </summary>
    /// <param name="artistToSearch_">the artist to search (will be parsed / cleaned)</param>
    /// <returns>Song object with Artist, WebImage and MusicBrainzID</returns>
    public Song getArtistInfo(string artistToSearch_)
    {
      Song tmpSong = new Song();
      string urlArtist = AudioscrobblerBase.getValidURLLastFMString(AudioscrobblerBase.UndoArtistPrefix(artistToSearch_));

      tmpSong = ParseXMLDocForArtistInfo(urlArtist);

      if (tmpSong.Artist != string.Empty)
      {
        if (tmpSong.WebImage != null || tmpSong.WebImage != string.Empty)
        {
          string coverURL = tmpSong.WebImage;
          // last.fm has higher resolution artist art.

          coverURL = GetLargeLastFmCover(coverURL);

          //coverURL = coverURL.Replace(@"/sidebar/", @"/original/");

          if (artistToSearch_.ToLowerInvariant() != tmpSong.Artist.ToLowerInvariant())
          {
            Log.Warn("AudioScrobblerUtils: alternative artist spelling detected - trying to fetch both thumbs (MP: {0} / official: {1})", artistToSearch_, tmpSong.Artist);
            fetchWebImage(coverURL, artistToSearch_ + ".jpg", Thumbs.MusicArtists);
            fetchWebImage(coverURL, tmpSong.Artist + ".jpg", Thumbs.MusicArtists);
          }
          else
            fetchWebImage(coverURL, tmpSong.Artist + ".jpg", Thumbs.MusicArtists);
        }
      }

      return tmpSong;
    }

    public List<Song> getTagsForArtist(string artistToSearch_)
    {
      string urlArtist = AudioscrobblerBase.getValidURLLastFMString(AudioscrobblerBase.UndoArtistPrefix(artistToSearch_));

      return ParseXMLDocForUsedTags(urlArtist, "", lastFMFeed.topartisttags);
    }

    public List<Song> getTagsForTrack(string artistToSearch_, string trackToSearch_)
    {
      string urlArtist = AudioscrobblerBase.getValidURLLastFMString(AudioscrobblerBase.UndoArtistPrefix(artistToSearch_));
      string urlTrack = AudioscrobblerBase.getValidURLLastFMString(trackToSearch_);

      return ParseXMLDocForUsedTags(urlArtist, urlTrack, lastFMFeed.toptracktags);
    }

    public List<Song> getSimilarToTag(lastFMFeed searchType_, string taggedWith_, bool randomizeList_, bool addAvailableTracksOnly_)
    {
      songFilterType currentFilterType = songFilterType.Track;
      switch (searchType_)
      {
        case lastFMFeed.taggedtracks:
          currentFilterType = songFilterType.Track;
          break;
        case lastFMFeed.taggedartists:
          currentFilterType = songFilterType.Artist;
          break;
        case lastFMFeed.taggedalbums:
          currentFilterType = songFilterType.Album;
          break;
      }

      if (randomizeList_)
      {
        PseudoRandomNumberGenerator rand = new PseudoRandomNumberGenerator();
        List<Song> taggedArtists = new List<Song>(50);
        List<Song> randomTaggedArtists = new List<Song>(_limitRandomListCount);

        int artistsAdded = 0;
        int randomPosition;
        int oldRandomLimit = 10;
        oldRandomLimit = _limitRandomListCount;
        _limitRandomListCount = 50;

        taggedArtists = ParseXMLDocForTags(taggedWith_, searchType_);

        // make sure we do not get an endless loop
        if (taggedArtists.Count > _limitRandomListCount)
        {
          int minRandValue = _limitRandomListCount;
          int calcRandValue = (taggedArtists.Count - 1) * _randomNessPercent / 100;
          while (artistsAdded < _limitRandomListCount)
          {
            bool foundDoubleEntry = false;
            if (calcRandValue > minRandValue)
              randomPosition = rand.Next(0, calcRandValue);
            else
              randomPosition = rand.Next(0, minRandValue);
            // loop current list to find out if randomPos was already inserted
            for (int j = 0; j < randomTaggedArtists.Count; j++)
            {
              if (randomTaggedArtists.Contains(taggedArtists[randomPosition]))
              {
                foundDoubleEntry = true;
                break;
              }
            }
            // new item therefore add it
            if (!foundDoubleEntry)
            {
              //taggedArtists[randomPosition].Genre = taggedWith_;
              randomTaggedArtists.Add(taggedArtists[randomPosition]);
              artistsAdded++;
            }
          }
          _limitRandomListCount = oldRandomLimit;
          // enough similar artists
          if (addAvailableTracksOnly_)
            return filterForLocalSongs(randomTaggedArtists, true, currentFilterType);
          else
            return randomTaggedArtists;
        }
        else
        {
          // limit not reached - return all Artists
          if (addAvailableTracksOnly_)
            return filterForLocalSongs(taggedArtists, true, currentFilterType);
          else
            return taggedArtists;
        }
      }
      else
      {
        if (addAvailableTracksOnly_)
          return filterForLocalSongs(ParseXMLDocForTags(taggedWith_, searchType_), true, currentFilterType);
        else
          return ParseXMLDocForTags(taggedWith_, searchType_);
      }
    }

    public List<Song> getSimilarArtists(string Artist_, bool randomizeList_)
    {
      Artist_ = AudioscrobblerBase.getValidURLLastFMString(AudioscrobblerBase.UndoArtistPrefix(Artist_));
      if (randomizeList_)
      {
        PseudoRandomNumberGenerator rand = new PseudoRandomNumberGenerator();
        List<Song> similarArtists = new List<Song>(50);
        List<Song> randomSimilarArtists = new List<Song>(_limitRandomListCount);
        similarArtists = ParseXMLDocForSimilarArtists(Artist_);
        int artistsAdded = 0;
        int randomPosition;
        // make sure we do not get an endless loop
        if (similarArtists.Count > _limitRandomListCount)
        {
          int minRandValue = _limitRandomListCount;
          int calcRandValue = (similarArtists.Count - 1) * _randomNessPercent / 100;
          while (artistsAdded < _limitRandomListCount)
          {
            bool foundDoubleEntry = false;
            if (calcRandValue > minRandValue)
              randomPosition = rand.Next(0, calcRandValue);
            else
              randomPosition = rand.Next(0, minRandValue);
            // loop current list to find out if randomPos was already inserted
            for (int j = 0; j < randomSimilarArtists.Count; j++)
            {
              if (randomSimilarArtists.Contains(similarArtists[randomPosition]))
              {
                foundDoubleEntry = true;
                break;
              }
            }
            // new item therefore add it
            if (!foundDoubleEntry)
            {
              randomSimilarArtists.Add(similarArtists[randomPosition]);
              artistsAdded++;
            }
          }
          // enough similar artists
          return randomSimilarArtists;
        }
        else
          // limit not reached - return all Artists
          return similarArtists;
      }
      else
        return ParseXMLDocForSimilarArtists(Artist_);
    }

    public List<Song> getNeighboursArtists(bool randomizeList_)
    {
      return getOthersArtists(randomizeList_, lastFMFeed.neighbours);
    }

    public List<Song> getFriendsArtists(bool randomizeList_)
    {
      return getOthersArtists(randomizeList_, lastFMFeed.friends);
    }

    public List<Song> getRandomTracks()
    {
      return fetchRandomTracks(offlineMode.random);
    }

    public List<Song> getUnhearedTracks()
    {
      return fetchRandomTracks(offlineMode.timesplayed);
    }

    public List<Song> getFavoriteTracks()
    {
      return fetchRandomTracks(offlineMode.favorites);
    }

    public string GetSongAlbumImage(Song aSong)
    {
      string ImagePath = String.Empty;
      try
      {
        string albumThumbName = Util.Utils.MakeFileName(string.Format("{0}-{1}{2}", aSong.Artist, aSong.Album, ".jpg"));
        if (fetchWebImage(aSong.WebImage, albumThumbName, Thumbs.MusicAlbum))
          ImagePath = string.Format(@"{0}\{1}", Thumbs.MusicAlbum, albumThumbName);
      }
      catch (Exception ex)
      {
        Log.Error("AudioscrobblerUtils: Error getting album image by song - {0}", ex.Message);
      }
      return ImagePath;
    }

    public List<Song> getRadioPlaylist(bool retryOnHttpError, string fullAdressWithSession, out string playlistName)
    {
      return ParseXSPFtrackList(retryOnHttpError, fullAdressWithSession, out playlistName);
    }

    #endregion

    #region internal fetch routines

    /// <summary>
    /// Downloads images and saves them into the given location
    /// </summary>
    /// <param name="imageUrl">source URL of the image</param>
    /// <param name="fileName">destination filename</param>
    /// <param name="thumbspath">thumb directory</param>
    /// <returns>whether the lookup has been successful</returns>
    private bool fetchWebImage(string imageUrl, string fileName, string thumbspath)
    {
      bool success = false;

      if (!AudioscrobblerBase.IsFetchingCovers)
        return success;

      fileName = Util.Utils.MakeFileName(fileName);

      if (imageUrl != "")
      {
        // do not download last.fm's placeholder
        if ((imageUrl.IndexOf("no_album") <= 0)
         && (imageUrl.IndexOf("no_artist") <= 0)
         && (imageUrl.IndexOf(@"/noimage/") <= 0)
          // almost useless because Last.fm currently has redundant images - TODO: image comparison algo
         && (!imageUrl.EndsWith(@"160/260045.jpg"))
         && (!imageUrl.EndsWith(@"160/2765129.gif"))
         && (!imageUrl.EndsWith(@"160/311112.gif")))
        {
          //Create the album subdir in thumbs if it does not exist.
          if (!Directory.Exists(thumbspath))
            Directory.CreateDirectory(thumbspath);

          string fullPath = Path.Combine(thumbspath, fileName);
          string fullLargePath = Util.Utils.ConvertToLargeCoverArt(fullPath);

          Log.Debug("MyMusic: Trying to get thumb: {0}", imageUrl);
          // Here we get the image from the web and save it to disk
          try
          {
            string tmpFile = DownloadTempFile(imageUrl);

            //temp file downloaded - check if needed
            if (File.Exists(fullLargePath))
            {
              FileInfo oldFile = new FileInfo(fullLargePath);
              FileInfo newFile = new FileInfo(tmpFile);

              if (oldFile.Length >= newFile.Length)
              {
                newFile.Delete();
                Log.Debug("MyMusic: better thumb {0} already exists - do not save", fullLargePath);
              }
              // temp thumb is "better" than old one
              else
              {
                try
                {
                  Util.Picture.CreateThumbnail(tmpFile, fullPath, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall);
                  Util.Picture.CreateThumbnail(tmpFile, fullLargePath, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
                  Log.Debug("MyMusic: fetched better thumb {0} overwriting existing one", fullLargePath);
                }
                catch (IOException ex)
                {
                  newFile.Delete();
                  Log.Debug("MyMusic: could not overwrite existing thumb {0} with better one", fileName, ex.Message);
                }
              }
            }
            else
            {
              Util.Picture.CreateThumbnail(tmpFile, fullPath, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall);
              Util.Picture.CreateThumbnail(tmpFile, fullLargePath, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
              Log.Info("MyMusic: Thumb successfully downloaded: {0}", fullLargePath);
            }
            success = true;
          }
          catch (Exception e)
          {
            Log.Error("MyMusic: Exception while downloading - {0}", e.Message);
          }
        }
        else
          Log.Debug("MyMusic: last.fm only uses a placeholder - do not download thumb");
      }
      else
      {
        Log.Debug("MyMusic: No imageurl. Can't download thumb");
      }
      return success;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static string DownloadTempFile(string imageUrl)
    {
      return DownloadTempFile(imageUrl, true);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static string DownloadTempFile(string imageUrl, bool retryHttpError)
    {
      string tmpFile = PathUtility.GetSecureTempFileName();
      try
      {
        using (WebClient client = new WebClient())
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // client.Proxy = WebProxy.GetDefaultProxy();
          client.Proxy.Credentials = CredentialCache.DefaultCredentials;
          //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
          //client.Headers.Add("user-agent", @"Mozilla/5.0 (X11; U; Linux i686; de-DE; rv:1.8.1) Gecko/20060601 Firefox/2.0 (Ubuntu-edgy)");

          try
          {
            client.DownloadFile(imageUrl, tmpFile);
          }
          catch (WebException wex)
          {
            // If error e.g. 503, server busy, etc wait and try again.
            if (wex.Status == WebExceptionStatus.ProtocolError && retryHttpError)
            {
              HttpWebResponse httpResponse = (HttpWebResponse)wex.Response;
              switch (httpResponse.StatusCode)
              {
                case HttpStatusCode.BadGateway:
                  break;
                case HttpStatusCode.BadRequest:
                  break;
                case HttpStatusCode.Forbidden:
                  break;
                case HttpStatusCode.Gone:
                  break;
                case HttpStatusCode.InternalServerError:
                  break;
                case HttpStatusCode.MethodNotAllowed:
                  break;
                case HttpStatusCode.NotFound:
                  break;
                case HttpStatusCode.PaymentRequired:
                  break;
                case HttpStatusCode.PreconditionFailed:
                  break;
                case HttpStatusCode.ProxyAuthenticationRequired:
                  break;
                case HttpStatusCode.Unauthorized:
                  break;
                default:
                  Log.Warn("AudioscrobblerUtils: Error while downloading on first try: {0} - {1}", imageUrl, wex.Message);
                  Thread.Sleep(3000);
                  client.DownloadFile(imageUrl, tmpFile);
                  break;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("AudioscrobblerUtils: Exception while downloading {0} - {1}", imageUrl, ex.Message);
      }
      return tmpFile;
    }

    private List<Song> fetchRandomTracks(offlineMode randomMode_)
    {
      int addedSongs = 0;
      MusicDatabase dbs = MusicDatabase.Instance;
      List<Song> randomSongList = new List<Song>(_limitRandomListCount);
      Song randomSong = new Song();
      Song lookupSong = new Song();
      int loops = 0;

      // fetch more than needed since there could be double entries
      while (addedSongs < _limitRandomListCount * 3)
      {
        loops++;
        lookupSong.Clear();

        dbs.GetRandomSong(ref lookupSong);
        randomSong = lookupSong.Clone();

        bool found = false;
        for (int i = 0; i < randomSongList.Count; i++)
          if (randomSongList[i].Artist == randomSong.Artist)
          {
            found = true;
            break;
          }

        if (!found)
        {
          switch (randomMode_)
          {
            case offlineMode.timesplayed:
              if (randomSong.TimesPlayed == 0)
              {
                randomSongList.Add(randomSong);
                addedSongs++;
              }
              break;
            case offlineMode.favorites:
              if (randomSong.Favorite)
              {
                randomSongList.Add(randomSong);
                addedSongs++;
              }
              break;
            case offlineMode.random:
              randomSongList.Add(randomSong);
              addedSongs++;
              break;
          }
        }
        // quick check; 3x rlimit times because every pass could try different artists in dbs.GetRandomSong(ref lookupSong);
        if (loops > 20)
        {
          if (randomMode_ == offlineMode.timesplayed)
            Log.Debug("AudioScrobblerUtils: Not enough unique unheard tracks for random mode");
          break;
        }
      }

      return randomSongList;
    }

    private List<Song> getOthersArtists(bool randomizeList_, lastFMFeed neighbourOrFriend_)
    {
      List<Song> myNeighbours = new List<Song>(50);
      List<Song> myRandomNeighbours = new List<Song>();
      List<Song> myNeighboorsArtists = new List<Song>(50);
      List<Song> myRandomNeighboorsArtists = new List<Song>();

      switch (neighbourOrFriend_)
      {
        case lastFMFeed.neighbours:
          myNeighbours = getAudioScrobblerFeed(lastFMFeed.neighbours, "");
          break;
        case lastFMFeed.friends:
          myNeighbours = getAudioScrobblerFeed(lastFMFeed.friends, "");
          break;
      }

      if (randomizeList_)
      {
        PseudoRandomNumberGenerator rand = new PseudoRandomNumberGenerator();
        int neighboursAdded = 0;
        int randomPosition;
        // make sure we do not get an endless loop
        if (myNeighbours.Count > _limitRandomListCount)
        {
          int minRandValue = _limitRandomListCount;
          int calcRandValue = (myNeighbours.Count - 1) * _randomNessPercent / 100;
          while (neighboursAdded < _limitRandomListCount)
          {
            bool foundDoubleEntry = false;
            if (calcRandValue > minRandValue)
              randomPosition = rand.Next(0, calcRandValue);
            else
              randomPosition = rand.Next(0, minRandValue);

            // loop current list to find out if randomPos was already inserted
            for (int j = 0; j < myRandomNeighbours.Count; j++)
            {
              if (myRandomNeighbours.Contains(myNeighbours[randomPosition]))
              {
                foundDoubleEntry = true;
                break;
              }
            }
            // new item therefore add it
            if (!foundDoubleEntry)
            {
              myRandomNeighbours.Add(myNeighbours[randomPosition]);
              neighboursAdded++;
            }
          }
          // now _limitRandomListCount random neighbours are added
          // get artists for these neighbours  
          for (int n = 0; n < myRandomNeighbours.Count; n++)
          {
            myNeighboorsArtists = getAudioScrobblerFeed(_currentNeighbourMode, myRandomNeighbours[n].Artist);

            if (myNeighboorsArtists.Count > 0)
            {
              if (myNeighboorsArtists[0].LastFMMatch != string.Empty)
                myNeighboorsArtists.Sort(CompareSongsByMatch);
              else
                if (myNeighboorsArtists[0].TimesPlayed >= 0)
                  myNeighboorsArtists.Sort(CompareSongsByTimesPlayed);
            }
            // make sure the neighbour has enough top artists
            if (myNeighboorsArtists.Count > _limitRandomListCount)
            {
              // get _limitRandomListCount artists for each random neighbour
              int artistsAdded = 0;
              int artistsPerNeighbour = _limitRandomListCount / myNeighbours.Count;
              // make sure there is at least one song per neighbour
              artistsPerNeighbour = artistsPerNeighbour > 1 ? artistsPerNeighbour : 1;
              int minRandAValue = _limitRandomListCount;
              int calcRandAValue = (myNeighboorsArtists.Count - 1) * _randomNessPercent / 100;
              while (artistsAdded <= artistsPerNeighbour)
              {
                bool foundDoubleEntry = false;
                if (calcRandAValue > minRandAValue)
                  randomPosition = rand.Next(0, calcRandAValue);
                else
                  randomPosition = rand.Next(0, minRandAValue);

                for (int j = 0; j < myRandomNeighboorsArtists.Count; j++)
                {
                  if (myRandomNeighboorsArtists.Contains(myNeighboorsArtists[randomPosition]))
                  {
                    foundDoubleEntry = true;
                    break;
                  }
                }
                // new item therefore add it
                if (!foundDoubleEntry)
                {
                  myRandomNeighboorsArtists.Add(myNeighboorsArtists[randomPosition]);
                  artistsAdded++;
                }
              }
            }
          }
          return myRandomNeighboorsArtists;

        }
        else
        // limit not reached - return all neighbours random artists          
        {
          for (int i = 0; i < myNeighbours.Count; i++)
          {
            // sort by match needed
            myNeighboorsArtists = getAudioScrobblerFeed(_currentNeighbourMode, myNeighbours[i].Artist);
            if (myNeighboorsArtists.Count > 0)
            {
              if (myNeighboorsArtists[0].LastFMMatch != string.Empty)
                myNeighboorsArtists.Sort(CompareSongsByMatch);
              else
                if (myNeighboorsArtists[0].TimesPlayed >= 0)
                  myNeighboorsArtists.Sort(CompareSongsByTimesPlayed);
            }

            // make sure the neighbour has enough top artists
            if (myNeighboorsArtists.Count > _limitRandomListCount)
            {
              // get _limitRandomListCount artists for each neighbour
              int artistsAdded = 0;
              int artistsPerNeighbour = _limitRandomListCount / myNeighbours.Count;
              // make sure there is at least one song per neighbour
              artistsPerNeighbour = artistsPerNeighbour > 1 ? artistsPerNeighbour : 1;
              int minRandAValue = _limitRandomListCount;
              int calcRandAValue = (myNeighboorsArtists.Count - 1) * _randomNessPercent / 100;
              while (artistsAdded <= artistsPerNeighbour)
              {
                bool foundDoubleEntry = false;
                if (calcRandAValue > minRandAValue)
                  randomPosition = rand.Next(0, calcRandAValue);
                else
                  randomPosition = rand.Next(0, minRandAValue);

                for (int j = 0; j < myNeighboorsArtists.Count; j++)
                {
                  if (myRandomNeighboorsArtists.Contains(myNeighboorsArtists[randomPosition]))
                  {
                    foundDoubleEntry = true;
                    break;
                  }
                }
                // new item therefore add it
                if (!foundDoubleEntry)
                {
                  myRandomNeighboorsArtists.Add(myNeighboorsArtists[randomPosition]);
                  artistsAdded++;
                }
              }
            }
          }
          return myRandomNeighboorsArtists;
        }

      }
      // do not randomize
      else
      {
        if (myNeighbours.Count > 4)
          for (int i = 0; i < 4; i++)
            myNeighboorsArtists.AddRange(getAudioScrobblerFeed(_currentNeighbourMode, myNeighbours[i].Artist));
        return myNeighboorsArtists;
      }
    }

    #endregion

    #region XML - Parsers

    private List<Song> ParseXMLDocForAlbumInfo(string artist_, string album_)
    {
      List<Song> AlbumInfoList = new List<Song>(10);
      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/album/" + artist_ + "/" + album_ + "/info.xml"));

        XmlNodeList nodes = doc.SelectNodes(@"//album");
        string tmpCover = string.Empty;
        string tmpArtist = string.Empty;
        string tmpAlbum = string.Empty;
        DateTime tmpRelease = DateTime.MinValue;

        if (!string.IsNullOrEmpty(nodes[0].Attributes["artist"].Value))
          tmpArtist = DecodeUtf8String(nodes[0].Attributes["artist"].Value);
        else
          tmpArtist = artist_;
        if (!string.IsNullOrEmpty(nodes[0].Attributes["title"].Value))
          tmpAlbum = DecodeUtf8String(nodes[0].Attributes["title"].Value);
        else
          tmpAlbum = album_;

        foreach (XmlNode mainchild in nodes[0].ChildNodes)
        {
          if (mainchild.Name == "releasedate" && mainchild.ChildNodes.Count != 0)
            tmpRelease = Convert.ToDateTime(mainchild.ChildNodes[0].Value);
          else if (mainchild.Name == "coverart" && mainchild.ChildNodes.Count != 0)
          {
            foreach (XmlNode coverchild in mainchild.ChildNodes)
            {
              if (coverchild.Name == "large" && coverchild.ChildNodes.Count != 0)
                tmpCover = coverchild.ChildNodes[0].Value;
            }
          }
          //else if (mainchild.Name == "mbid" && mainchild.ChildNodes.Count != 0)
          //  nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
        }

        nodes = doc.SelectNodes(@"//album/tracks/track");

        foreach (XmlNode node in nodes)
        {
          Song nodeSong = new Song();

          nodeSong.Artist = tmpArtist;
          nodeSong.Album = tmpAlbum;
          nodeSong.WebImage = tmpCover;
          nodeSong.DateTimePlayed = tmpRelease;

          if (!string.IsNullOrEmpty(node.Attributes["title"].Value))
          {
            nodeSong.Title = DecodeUtf8String(node.Attributes["title"].Value);
            // last.fm sometimes inserts this - maybe more to come...
            nodeSong.Title = nodeSong.Title.Replace("(Album Version)", "").Trim();
          }

          foreach (XmlNode child in node.ChildNodes)
          {
            if (child.Name == "reach" && child.ChildNodes.Count != 0)
              nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
            else if (child.Name == "url" && child.ChildNodes.Count != 0)
              nodeSong.URL = child.ChildNodes[0].Value;
          }
          AlbumInfoList.Add(nodeSong);
        }

        artist_ = System.Web.HttpUtility.UrlDecode(artist_);
        string thumbPath = artist_ + "-" + tmpAlbum + Util.Utils.GetThumbExtension();

        if (artist_.ToLowerInvariant() != tmpArtist.ToLowerInvariant())
        {
          string thumbPathAlternative = tmpArtist + "-" + tmpAlbum + Util.Utils.GetThumbExtension();
          Log.Warn("AudioScrobblerUtils: alternative album artist spelling detected - fetching both thumbs (MP: {0} / official: {1})", artist_, tmpArtist);
          fetchWebImage(tmpCover, thumbPathAlternative, Thumbs.MusicAlbum);
          fetchWebImage(tmpCover, thumbPath, Thumbs.MusicAlbum);
        }
        else
          fetchWebImage(tmpCover, thumbPath, Thumbs.MusicAlbum);
      }
      catch
      {
        // input nice exception here...
      }

      return AlbumInfoList;
    }

    private List<Song> ParseXMLDocForTopAlbums(string artist_)
    {
      List<Song> TopAlbumList = new List<Song>();
      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/artist/" + artist_ + "/topalbums.xml"));

        XmlNodeList nodes = doc.SelectNodes(@"//topalbums/album");

        foreach (XmlNode node in nodes)
        {
          Song nodeSong = new Song();
          foreach (XmlNode mainchild in node.ChildNodes)
          {
            if (mainchild.Name == "name" && mainchild.ChildNodes.Count != 0)
              nodeSong.Album = DecodeUtf8String(mainchild.ChildNodes[0].Value);
            else if (mainchild.Name == "mbid" && mainchild.ChildNodes.Count != 0)
              nodeSong.MusicBrainzID = mainchild.ChildNodes[0].Value;
            else if (mainchild.Name == "reach" && mainchild.ChildNodes.Count != 0)
              nodeSong.TimesPlayed = Convert.ToInt32(mainchild.ChildNodes[0].Value);
            else if (mainchild.Name == "url" && mainchild.ChildNodes.Count != 0)
              nodeSong.URL = mainchild.ChildNodes[0].Value;
            else if (mainchild.Name == "releasedate" && mainchild.ChildNodes.Count != 0)
              nodeSong.DateTimePlayed = Convert.ToDateTime(mainchild.ChildNodes[0].Value);
            else if (mainchild.Name == "coverart" && mainchild.ChildNodes.Count != 0)
            {
              foreach (XmlNode coverchild in mainchild.ChildNodes)
                if (coverchild.Name == "large" && coverchild.ChildNodes.Count != 0)
                  nodeSong.WebImage = coverchild.ChildNodes[0].Value;
            }
          }
          TopAlbumList.Add(nodeSong);
        }
      }
      catch
      {
        // input nice exception here...
      }

      return TopAlbumList;
    }

    private Song ParseXMLDocForArtistInfo(string artist_)
    {
      Song artistInfo = new Song();
      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/artist/" + artist_ + "/" + "similar.xml"));
        XmlNodeList nodes = doc.SelectNodes(@"//similarartists");

        if (!string.IsNullOrEmpty(nodes[0].Attributes["artist"].Value))
          artistInfo.Artist = DecodeUtf8String(nodes[0].Attributes["artist"].Value);
        if (!string.IsNullOrEmpty(nodes[0].Attributes["picture"].Value))
          artistInfo.WebImage = nodes[0].Attributes["picture"].Value;
        if (!string.IsNullOrEmpty(nodes[0].Attributes["mbid"].Value))
          artistInfo.MusicBrainzID = nodes[0].Attributes["mbid"].Value;
      }
      catch
      {
        // input nice exception here...
      }
      return artistInfo;
    }

    private List<Song> ParseXMLDocForSimilarArtists(string artist_)
    {
      List<Song> SimilarArtistList = new List<Song>(50);
      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/artist/" + artist_ + "/" + "similar.xml"));
        XmlNodeList nodes = doc.SelectNodes(@"//similarartists/artist");

        foreach (XmlNode node in nodes)
        {
          Song nodeSong = new Song();
          foreach (XmlNode child in node.ChildNodes)
          {
            if (child.Name == "name" && child.ChildNodes.Count != 0)
              nodeSong.Artist = DecodeUtf8String(child.ChildNodes[0].Value);
            else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
              nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
            else if (child.Name == "url" && child.ChildNodes.Count != 0)
              nodeSong.URL = child.ChildNodes[0].Value;
            else if (child.Name == "image" && child.ChildNodes.Count != 0)
              nodeSong.WebImage = child.ChildNodes[0].Value;
            else if (child.Name == "match" && child.ChildNodes.Count != 0)
              nodeSong.LastFMMatch = child.ChildNodes[0].Value;
          }
          SimilarArtistList.Add(nodeSong);
        }
      }
      catch (Exception ex)
      {
        Log.Error("AudioscrobblerUtils: Error occurred in ParseXMLDocForSimilarArtists - {0}", ex.Message);
      }
      return TryFilterBelowMinimumMatch(SimilarArtistList);
    }

    /// <summary>
    /// Parses an artist or track for its most used tags
    /// </summary>
    /// <param name="artist_">artist to search</param>
    /// <param name="track_">track to search</param>
    /// <param name="searchType_">topartisttags or toptracktags</param>
    /// <returns>List of Song with Genre, TimesPlayed and URL</returns>
    private List<Song> ParseXMLDocForUsedTags(string artist_, string track_, lastFMFeed searchType_)
    {
      List<Song> UsedTagsList = new List<Song>();
      try
      {
        XmlDocument doc = new XmlDocument();
        XmlNodeList nodes;
        switch (searchType_)
        {
          case lastFMFeed.topartisttags:
            doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/artist/" + artist_ + "/toptags.xml"));
            nodes = doc.SelectNodes(@"//toptags/tag");
            break;
          case lastFMFeed.toptracktags:
            doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/track/" + artist_ + "/" + track_ + "/toptags.xml"));
            nodes = doc.SelectNodes(@"//toptags/tag");
            break;

          default:
            doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/artist/" + artist_ + "/toptags.xml"));
            nodes = doc.SelectNodes(@"//toptags/tag");
            break;
        }

        foreach (XmlNode node in nodes)
        {
          Song nodeSong = new Song();
          foreach (XmlNode child in node.ChildNodes)
          {
            //if (doc.Attributes["artist"].Value != "")
            //  nodeSong.Artist = doc.Attributes["artist"].Value;
            //if (doc.Attributes["track"].Value != "")
            //  nodeSong.Title = doc.Attributes["track"].Value;
            nodeSong.Artist = artist_;
            nodeSong.Title = track_;
            if (child.Name == "name" && child.ChildNodes.Count != 0)
              nodeSong.Genre = DecodeUtf8String(child.ChildNodes[0].Value);
            if (child.Name == "url" && child.ChildNodes.Count != 0)
              nodeSong.URL = child.ChildNodes[0].Value;
            if (child.Name == "count" && child.ChildNodes.Count != 0)
              nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
          }
          UsedTagsList.Add(nodeSong);
        }
      }
      catch
      {
        // input nice exception here...
      }
      return UsedTagsList;
    }

    private List<Song> ParseXMLDocForTags(string taggedWith_, lastFMFeed searchType_)
    {
      List<Song> TagsList = new List<Song>(50);
      try
      {
        XmlDocument doc = new XmlDocument();
        XmlNodeList nodes;
        switch (searchType_)
        {
          case lastFMFeed.taggedartists:
            doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/tag/" + taggedWith_ + "/topartists.xml"));
            nodes = doc.SelectNodes(@"//tag/artist");
            break;
          case lastFMFeed.taggedalbums:
            doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/tag/" + taggedWith_ + "/topalbums.xml"));
            nodes = doc.SelectNodes(@"//tag/album");
            break;
          case lastFMFeed.taggedtracks:
            doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/tag/" + taggedWith_ + "/toptracks.xml"));
            nodes = doc.SelectNodes(@"//tag/track");
            break;
          default:
            doc.Load(DownloadTempFile(@"http://ws.audioscrobbler.com/1.0/tag/" + taggedWith_ + "/topartists.xml"));
            nodes = doc.SelectNodes(@"//tag/artist");
            break;
        }

        foreach (XmlNode node in nodes)
        {
          Song nodeSong = new Song();
          foreach (XmlNode child in node.ChildNodes)
          {
            switch (searchType_)
            {
              case lastFMFeed.taggedartists:
                if (!string.IsNullOrEmpty(node.Attributes["name"].Value))
                  nodeSong.Artist = DecodeUtf8String(node.Attributes["name"].Value);
                if (child.Name == "image" && child.ChildNodes.Count != 0)
                  nodeSong.WebImage = child.ChildNodes[0].Value;
                break;
              case lastFMFeed.taggedalbums:
                if (!string.IsNullOrEmpty(node.Attributes["name"].Value))
                  nodeSong.Album = DecodeUtf8String(node.Attributes["name"].Value);
                if (child.Name == "artist" && child.ChildNodes.Count != 0)
                  if (string.IsNullOrEmpty(child.Attributes["name"].Value))
                    nodeSong.Artist = DecodeUtf8String(child.Attributes["name"].Value);
                break;
              case lastFMFeed.taggedtracks:
                if (!string.IsNullOrEmpty(node.Attributes["name"].Value))
                  nodeSong.Title = DecodeUtf8String(node.Attributes["name"].Value);
                if (child.Name == "artist" && child.ChildNodes.Count != 0)
                  if (!string.IsNullOrEmpty(child.Attributes["name"].Value))
                    nodeSong.Artist = DecodeUtf8String(child.Attributes["name"].Value);
                break;

            }
            if (child.Name == "mbid" && child.ChildNodes.Count != 0)
              nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
            if (child.Name == "url" && child.ChildNodes.Count != 0)
              nodeSong.URL = child.ChildNodes[0].Value;
            if (node.Attributes["count"].Value != "")
              nodeSong.TimesPlayed = Convert.ToInt32(node.Attributes["count"].Value);
          }
          TagsList.Add(nodeSong);
        }
      }
      catch
      {
        // input nice exception here...
      }
      return TagsList;
    }

    private List<Song> ParseXMLDoc(string xmlFileInput, string queryNodePath, lastFMFeed xmlfeed)
    {
      List<Song> songList = new List<Song>(50);
      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(DownloadTempFile(xmlFileInput));
        XmlNodeList nodes = doc.SelectNodes(queryNodePath);

        foreach (XmlNode node in nodes)
        {
          Song nodeSong = new Song();
          foreach (XmlNode child in node.ChildNodes)
          {
            switch (xmlfeed)
            {
              case (lastFMFeed.recenttracks):
                if (child.Name == "artist" && child.ChildNodes.Count != 0)
                  nodeSong.Artist = DecodeUtf8String(child.ChildNodes[0].Value);
                else if (child.Name == "name" && child.ChildNodes.Count != 0)
                  nodeSong.Title = DecodeUtf8String(child.ChildNodes[0].Value);
                else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
                  nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
                else if (child.Name == "url" && child.ChildNodes.Count != 0)
                  nodeSong.URL = child.ChildNodes[0].Value;
                else if (child.Name == "date" && child.ChildNodes.Count != 0)
                  nodeSong.DateTimePlayed = Convert.ToDateTime(child.ChildNodes[0].Value);
                break;
              case (lastFMFeed.recentbannedtracks):
                goto case lastFMFeed.recenttracks;
              case (lastFMFeed.recentlovedtracks):
                goto case lastFMFeed.recenttracks;
              case (lastFMFeed.topartists):
                if (child.Name == "name" && child.ChildNodes.Count != 0)
                  nodeSong.Artist = DecodeUtf8String(child.ChildNodes[0].Value);
                //else if (child.Name == "name" && child.ChildNodes.Count != 0)
                //  nodeSong.Title = ConvertUtf8StringToSystemCodepage(child.ChildNodes[0].Value);
                else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
                  nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
                else if (child.Name == "playcount" && child.ChildNodes.Count != 0)
                  nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
                else if (child.Name == "url" && child.ChildNodes.Count != 0)
                  nodeSong.URL = child.ChildNodes[0].Value;
                else if (child.Name == "match" && child.ChildNodes.Count != 0)
                  nodeSong.LastFMMatch = child.ChildNodes[0].Value;
                break;
              case (lastFMFeed.weeklyartistchart):
                goto case lastFMFeed.topartists;
              case (lastFMFeed.toptracks):
                if (child.Name == "artist" && child.ChildNodes.Count != 0)
                  nodeSong.Artist = DecodeUtf8String(child.ChildNodes[0].Value);
                else if (child.Name == "name" && child.ChildNodes.Count != 0)
                  nodeSong.Title = DecodeUtf8String(child.ChildNodes[0].Value);
                else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
                  nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
                else if (child.Name == "playcount" && child.ChildNodes.Count != 0)
                  nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
                else if (child.Name == "url" && child.ChildNodes.Count != 0)
                  nodeSong.URL = child.ChildNodes[0].Value;
                break;
              case (lastFMFeed.toptags):
                if (child.Name == "name" && child.ChildNodes.Count != 0)
                  nodeSong.Artist = DecodeUtf8String(child.ChildNodes[0].Value);
                else if (child.Name == "count" && child.ChildNodes.Count != 0)
                  nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
                else if (child.Name == "url" && child.ChildNodes.Count != 0)
                  nodeSong.URL = child.ChildNodes[0].Value;
                break;
              // doesn't work atm
              case (lastFMFeed.chartstoptags):
                if (!string.IsNullOrEmpty(node.Attributes["name"].Value))
                  nodeSong.Artist = DecodeUtf8String(node.Attributes["name"].Value);
                if (node.Attributes["count"].Value != "")
                  nodeSong.TimesPlayed = Convert.ToInt32(node.Attributes["count"].Value);
                if (node.Attributes["url"].Value != "")
                  nodeSong.URL = node.Attributes["url"].Value;
                break;
              case (lastFMFeed.neighbours):
                if (node.Attributes["username"].Value != "")
                  nodeSong.Artist = node.Attributes["username"].Value;
                if (child.Name == "url" && child.ChildNodes.Count != 0)
                  nodeSong.URL = child.ChildNodes[0].Value;
                else if (child.Name == "match" && child.ChildNodes.Count != 0)
                  nodeSong.LastFMMatch = child.ChildNodes[0].Value;
                else if (child.Name == "image" && child.ChildNodes.Count != 0)
                  nodeSong.WebImage = child.ChildNodes[0].Value;
                break;
              case (lastFMFeed.friends):
                if (node.Attributes["username"].Value != "")
                  nodeSong.Artist = node.Attributes["username"].Value;
                if (child.Name == "url" && child.ChildNodes.Count != 0)
                  nodeSong.URL = child.ChildNodes[0].Value;
                else if (child.Name == "image" && child.ChildNodes.Count != 0)
                  nodeSong.WebImage = child.ChildNodes[0].Value;
                //else if (child.Name == "connections" && child.ChildNodes.Count != 0)
                //  nodeSong.LastFMMatch = child.ChildNodes[0].Value;
                break;
              case (lastFMFeed.weeklytrackchart):
                goto case lastFMFeed.toptracks;
              case (lastFMFeed.similar):
                goto case lastFMFeed.topartists;
              case (lastFMFeed.systemrecs):
                if (child.Name == "name" && child.ChildNodes.Count != 0)
                  nodeSong.Artist = DecodeUtf8String(child.ChildNodes[0].Value);
                else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
                  nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
                else if (child.Name == "url" && child.ChildNodes.Count != 0)
                  nodeSong.URL = child.ChildNodes[0].Value;
                break;
              case (lastFMFeed.profile):
                if (node.Attributes["id"].Value != String.Empty)
                  nodeSong.Id = Convert.ToInt32(node.Attributes["id"].Value);
                if (node.Attributes["username"].Value != String.Empty)
                  nodeSong.Artist = node.Attributes["username"].Value;
                if (child.Name == "url" && child.ChildNodes.Count != 0)
                  nodeSong.URL = child.ChildNodes[0].Value;
                else if (child.Name == "realname" && child.ChildNodes.Count != 0)
                  nodeSong.Title = DecodeUtf8String(child.ChildNodes[0].Value);
                else if (child.Name == "registered" && child.ChildNodes.Count != 0)
                  nodeSong.DateTimePlayed = Convert.ToDateTime(child.ChildNodes[0].Value);
                else if (child.Name == "statsreset" && child.ChildNodes.Count != 0)
                  nodeSong.DateTimeModified = Convert.ToDateTime(child.ChildNodes[0].Value);
                else if (child.Name == "country" && child.ChildNodes.Count != 0)
                  nodeSong.Genre = child.ChildNodes[0].Value;
                else if (child.Name == "playcount" && child.ChildNodes.Count != 0)
                  nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
                else if (child.Name == "avatar" && child.ChildNodes.Count != 0)
                  nodeSong.WebImage = child.ChildNodes[0].Value;
                break;
            } //switch
          }
          songList.Add(nodeSong);
        }
      }
      catch
      {
        // input nice exception here...
      }
      return songList;
    }
    #endregion

    #region MusicBrainz - Parser

    private Song GetMbInfoForArtist(string aArtist)
    {
      List<Song> MbArtists = new List<Song>(50);
      MbArtists = ParseMusicBrainzXML(string.Format(@"http://musicbrainz.org/ws/1/artist/?type=xml&name={0}", aArtist));

      foreach (Song logSong in MbArtists)
      {
        Log.Debug("AudioscrobblerUtils: Artist found with MusicBrainz: {0}", logSong.ToLastFMMatchString(false));
      }

      return MbArtists.Count > 0 ? MbArtists[0] : new Song();
    }

    private List<Song> ParseMusicBrainzXML(string aLocation)
    {
      List<Song> RESTresults = new List<Song>(25);

      try
      {
        string tempFile = PathUtility.GetSecureTempFileName();
        XmlDocument doc = new XmlDocument();

        doc.Load(DownloadTempFile(aLocation));
        doc.Save(tempFile);

        using (XmlReader reader = XmlReader.Create(tempFile))
        {
          Song mbContainer = null;
          while (reader.Read())
          {
            switch (reader.Depth)
            {
              case 2:
                if (reader.LocalName == "artist")
                {
                  // we reenter level 2 after adding all level 3 info - now we'll add the item
                  if (mbContainer != null && !string.IsNullOrEmpty(mbContainer.MusicBrainzID))
                  {
                    Song addInfo = mbContainer.Clone();
                    RESTresults.Add(addInfo);
                    mbContainer.Clear();
                  }
                  else
                    mbContainer = new Song();

                  mbContainer.MusicBrainzID = reader.GetAttribute("id");
                  mbContainer.LastFMMatch = reader.GetAttribute(@"ext:score");
                }
                break;
              case 3:
                if (string.IsNullOrEmpty(reader.Name))
                  continue;

                if (reader.Name == "name")
                  mbContainer.Artist = reader.ReadString();
                else
                  if (reader.Name == "sort-name")
                    mbContainer.Title = reader.ReadString();
                  else
                    if (reader.Name.Contains("life-span"))
                    {
                      try
                      {
                        // unlikely - most dates are simply years - need to check what is common..
                        DateTime born = DateTime.UtcNow;
                        if (DateTime.TryParse(reader.ReadString(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out born))
                          mbContainer.Year = born.Year;
                      }
                      catch (Exception)
                      {
                      }
                    }
                    else
                      if (reader.Name == "disambiguation")
                        mbContainer.Genre = reader.ReadString();
                break;
              default:
                continue;
              //break;
            }

          } // <-- while reading
        }
        File.Delete(tempFile);
      }
      catch (Exception ex)
      {
        Log.Error("AudioscrobblerUtils: Couldn't fetch MusicBrainz XML - {0},{1}", ex.Message, ex.StackTrace);
      }

      return RESTresults;
    }

    #endregion

    #region XSPF - Parser

    private List<Song> ParseXSPFtrackList(bool aShouldRetry, string aLocation, out string aPlaylistName)
    {
      aPlaylistName = String.Empty;
      List<Song> XSPFPlaylist = new List<Song>(5);
      try
      {
        XmlDocument doc = new XmlDocument();

        try
        {
          string cachedList = DownloadTempFile(aLocation, aShouldRetry);
          doc.Load(cachedList);

          using (XmlReader reader = XmlReader.Create(cachedList))
          {
            reader.ReadToFollowing("title");
            while (reader.Read())
            {
              if (reader.HasValue)
              {
                aPlaylistName = reader.Value;
                break;
              }
            }
          }
        }
        catch (Exception exd)
        {
          Log.Error("AudioscrobblerUtils: Couldn't load XSFP Radio tracklist - {0}", exd.Message);
          return XSPFPlaylist;
        }

        XmlNodeList nodes = doc.SelectNodes(@"//playlist/trackList/track");

        foreach (XmlNode node in nodes)
        {
          Song nodeSong = new Song();
          foreach (XmlNode mainchild in node.ChildNodes)
          {
            if (mainchild.Name == "creator" && mainchild.ChildNodes.Count != 0)
              nodeSong.Artist = DecodeUtf8String(mainchild.ChildNodes[0].Value);
            else if (mainchild.Name == "title" && mainchild.ChildNodes.Count != 0)
              nodeSong.Title = DecodeUtf8String(mainchild.ChildNodes[0].Value);
            else if (mainchild.Name == "id" && mainchild.ChildNodes.Count != 0)
              nodeSong.Id = Convert.ToInt32(mainchild.ChildNodes[0].Value);
            else if (mainchild.Name == "album" && mainchild.ChildNodes.Count != 0)
              nodeSong.Album = DecodeUtf8String(mainchild.ChildNodes[0].Value);
            else if (mainchild.Name == "location" && mainchild.ChildNodes.Count != 0)
              nodeSong.FileName = nodeSong.URL = mainchild.ChildNodes[0].Value;
            else if (mainchild.Name == "image" && mainchild.ChildNodes.Count != 0)
              nodeSong.WebImage = mainchild.ChildNodes[0].Value;
            else if (mainchild.Name == "duration" && mainchild.ChildNodes.Count != 0)
              nodeSong.Duration = Convert.ToInt32(Convert.ToInt32(mainchild.ChildNodes[0].Value) / 1000);
            else if (mainchild.Name == "lastfm:trackauth" && mainchild.ChildNodes.Count != 0)
              nodeSong.AuthToken = mainchild.ChildNodes[0].Value;
          }
          XSPFPlaylist.Add(nodeSong);
        }
      }
      catch (Exception ex)
      {
        Log.Error("AudioscrobblerUtils: Error fetching XSFP Radio tracklist - {0},{1}", ex.Message, ex.StackTrace);
      }

      return XSPFPlaylist;
    }

    #endregion

    #region Last.fm radio actions

    public string GetRadioLoveRequest(string aUser, string aChallenge, string aAuth, string aArtist, string aTitle)
    {
      return BuildXmlRpcRequest(XmlRpcType.loveTrack, aUser, aChallenge, aAuth, aArtist, aTitle, String.Empty);
    }

    public string GetRadioUnLoveRequest(string aUser, string aChallenge, string aAuth, string aArtist, string aTitle)
    {
      return BuildXmlRpcRequest(XmlRpcType.unLoveTrack, aUser, aChallenge, aAuth, aArtist, aTitle, String.Empty);
    }

    public string GetRadioBanRequest(string aUser, string aChallenge, string aAuth, string aArtist, string aTitle)
    {
      return BuildXmlRpcRequest(XmlRpcType.banTrack, aUser, aChallenge, aAuth, aArtist, aTitle, String.Empty);
    }

    public string GetRadioUnBanRequest(string aUser, string aChallenge, string aAuth, string aArtist, string aTitle)
    {
      return BuildXmlRpcRequest(XmlRpcType.unBanTrack, aUser, aChallenge, aAuth, aArtist, aTitle, String.Empty);
    }

    public string GetRadioAddTrackToPlaylistRequest(string aUser, string aChallenge, string aAuth, string aArtist, string aTitle)
    {
      return BuildXmlRpcRequest(XmlRpcType.addTrackToUserPlaylist, aUser, aChallenge, aAuth, aArtist, aTitle, String.Empty);
    }

    /// <summary>
    /// Formats an XML post header for last.fm's api access
    /// </summary>
    /// <param name="aMethodType">Is a type of the method to be called: e.g. LoveTrack, BanTrack, etc</param>
    /// <param name="aUser">The users last.fm username.</param>
    /// <param name="aChallenge">The current UNIX timestamp.</param>
    /// <param name="aAuth">The authentication token, a 32-byte ASCII hexadecimal representation of the MD5 hash of the users last.fm password and the timestamp: md5(md5(password) + timestamp)</param>
    /// <param name="aArtist">The artist to get love/ban.</param>
    /// <param name="aTitle">The track title to get love/ban.</param>
    /// <param name="aFriendsUserName">The friends name to remove.</param>
    /// <returns>The XML data in string format</returns>
    private string BuildXmlRpcRequest(XmlRpcType aMethodType, string aUser, string aChallenge, string aAuth, string aArtist, string aTitle, string aFriendsUserName)
    {
      string ResultXml = String.Empty;
      List<string> AllParams = new List<string>(5);
      try
      {
        if (!String.IsNullOrEmpty(aUser))
          AllParams.Add(aUser);
        if (!String.IsNullOrEmpty(aChallenge))
          AllParams.Add(aChallenge);
        if (!String.IsNullOrEmpty(aAuth))
          AllParams.Add(aAuth);
        if (aMethodType == XmlRpcType.removeFriend)
        {
          if (!String.IsNullOrEmpty(aFriendsUserName))
            AllParams.Add(aFriendsUserName);
        }
        else
        {
          if (!String.IsNullOrEmpty(aArtist))
            AllParams.Add(aArtist);
          if (!String.IsNullOrEmpty(aTitle))
            AllParams.Add(aTitle);
        }

        using (MemoryStream memoryStream = new MemoryStream())
        {
          XmlWriterSettings settings = new XmlWriterSettings();
          //settings.Indent = true;
          //settings.IndentChars = ("    ");
          settings.CloseOutput = true;
          // If i use Encodings.UTF8 the BOM will be prepended...
          settings.Encoding = new UTF8Encoding(false);

          using (XmlWriter writer = XmlWriter.Create(memoryStream, settings))
          {
            writer.WriteStartElement("methodCall");
            writer.WriteElementString("methodName", aMethodType.ToString());
            writer.WriteStartElement("params");
            foreach (string singleParam in AllParams)
            {
              writer.WriteStartElement("param");
              writer.WriteStartElement("value");
              writer.WriteElementString("string", singleParam);
              writer.WriteEndElement();
              writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
          }
          ResultXml = Encoding.UTF8.GetString(memoryStream.GetBuffer());
          Log.Debug("AudioscrobblerUtils: Successfully created XMLRPC call for method {0}", aMethodType.ToString());
        }
      }
      catch (Exception ex)
      {
        Log.Error("AudioscrobblerUtils: Error in BuildXmlRpcRequest - {0}", ex.Message);
      }
      return ResultXml;
    }

    #endregion

    #region Utils

    public string GetLargeLastFmCover(string aOriginalURL)
    {
      if (String.IsNullOrEmpty(aOriginalURL))
        return String.Empty;

      string highResPic = aOriginalURL;
      int resPos = aOriginalURL.LastIndexOf('/');
      if (resPos > 0)
      {
        // transform http://userserve-ak.last.fm/serve/126/15402945.jpg into http://userserve-ak.last.fm/serve/_/15402945.jpg
        highResPic = String.Format("{0}{1}", "http://userserve-ak.last.fm/serve/_/", highResPic.Substring(resPos + 1));
      }

      return highResPic;
    }

    public string DecodeUtf8String(string aUtf8String)
    {
      if (_decodeUtf8)
      {
        try
        {
          byte[] Utf8Array = Encoding.UTF8.GetBytes(aUtf8String);
          Encoding sysencoding = Encoding.Default;
          string WindowsString = Encoding.GetEncoding(sysencoding.WindowsCodePage).GetString(Utf8Array);

          if (WindowsString != aUtf8String)
            Log.Debug("AudioscrobblerUtils: Encoding changed - UTF8: {0}, System: {1}", aUtf8String, WindowsString);

          return WindowsString;
        }
        catch (Exception ex)
        {
          Log.Warn("AudioscrobblerUtils: Could not convert to system encoding - {0}, {1}", aUtf8String, ex.Message);
          return aUtf8String;
        }
      }
      else
        return aUtf8String;
    }

    public List<Song> TryFilterBelowMinimumMatch(List<Song> aUnfilteredList)
    {
      bool allContainMatchValue = true;
      foreach (Song checkSong in aUnfilteredList)
      {
        if (string.IsNullOrEmpty(checkSong.LastFMMatch))
        {
          allContainMatchValue = false;
          break;
        }
      }

      if (allContainMatchValue)
      {
        int removedItems = aUnfilteredList.Count;
        aUnfilteredList.RemoveAll(IsSongBelowMinPercentage);
        removedItems -= aUnfilteredList.Count;
        Log.Debug("AudioscrobblerUtils: TryFilterBelowMinimumMatch removed {0} items which were not exact enough", removedItems.ToString());
      }

      return aUnfilteredList;
    }

    #endregion
  }
}
