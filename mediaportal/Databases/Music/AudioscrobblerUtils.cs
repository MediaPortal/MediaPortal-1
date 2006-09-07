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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

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
    albuminfo
  }

  public enum offlineMode: int
  {
    random = 0,
    timesplayed = 1,
    favorites = 2,
  }
  #endregion

  public class AudioscrobblerUtils
  {
    private bool _useDebugLog = false;
    private string _defaultUser = "";
    private Object LookupLock;

    // Similar mode intelligence params
    private int _minimumArtistMatchPercent = 50;
    private int _limitRandomListCount = 5;
    private int _randomNessPercent = 75;

    // Neighbour mode intelligence params
    private lastFMFeed _currentNeighbourMode;
    //private offlineMode _currentOfflineMode;

    List<Song> songList = null;
    List<String> _unwantedTags = null;

    /// <summary>
    /// ctor
    /// </summary>
    public AudioscrobblerUtils()
    {
      LoadSettings();
    }

    #region SongComparer
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
            if (x.LastFMMatch != String.Empty && y.LastFMMatch != String.Empty)
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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        _defaultUser = xmlreader.GetValueAsString("audioscrobbler", "user", "");
      }
      MusicDatabase mdb = new MusicDatabase();
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

      //switch (tmpRMode)
      //{
      //  case 0: _currentOfflineMode = offlineMode.random; break;
      //  case 1: _currentOfflineMode = offlineMode.timesplayed; break;
      //  case 2: _currentOfflineMode = offlineMode.favorites; break;
      //  default: _currentOfflineMode = offlineMode.random; break;
      //}

      _randomNessPercent = (tmpRand >= 25) ? tmpRand : 77;
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

    public List<Song> getAudioScrobblerFeed(lastFMFeed feed_, string asUser_)
    {
      if (asUser_ == "")
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
        default:
          return ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + asUser_ + "/" + "recenttracks.xml", @"//recenttracks/track", feed_);
      }
    }

    public string getValidURLLastFMString(string lastFMString)
    {
      int dotIndex = 0;
      int lastIndex = -1;
      string outString = String.Empty;
      string cleanString = String.Empty;
      string urlString = System.Web.HttpUtility.UrlEncode(lastFMString);

      try
      {
        cleanString = lastFMString;
        // remove CD1, CD2, CDn from Tracks
        if (Util.Utils.ShouldStack(cleanString, cleanString))
          Util.Utils.RemoveStackEndings(ref cleanString);
        //dotIndex = cleanString.IndexOf("CD");
        //if (dotIndex > 0)
        //  cleanString = cleanString.Remove(dotIndex);
        // remove [DJ Spacko MIX 2000]
        dotIndex = cleanString.IndexOf("[");
        if (dotIndex > 0)
          cleanString = cleanString.Remove(dotIndex);
        dotIndex = cleanString.IndexOf("(");
        if (dotIndex > 0)
          cleanString = cleanString.Remove(dotIndex);

        dotIndex = 0;
        if (cleanString != String.Empty)
        {
          // build a clean end
          dotIndex = cleanString.LastIndexOf('-');
          if (dotIndex >= cleanString.Length - 2)
            outString = cleanString.Remove(dotIndex);
          dotIndex = cleanString.LastIndexOf('+');
          if (dotIndex >= cleanString.Length - 2)
            outString = cleanString.Remove(dotIndex);
          urlString = System.Web.HttpUtility.UrlEncode(cleanString);
        }

        outString = urlString;

        List<Char> invalidSingleChars = new List<Char>();
        invalidSingleChars.Add('.');
        invalidSingleChars.Add(',');
        foreach (Char singleChar in invalidSingleChars)
        {
          do
          {
            dotIndex = urlString.IndexOf(singleChar);
            if (dotIndex > 0)
              if (dotIndex > lastIndex)
              {
                if (dotIndex < urlString.Length -1)
                {
                  lastIndex = dotIndex;
                  outString = urlString.Insert(dotIndex + 1, "+");
                  urlString = outString;
                }                
              }
              else
                break;
          }
          while (dotIndex > 0);
        } 
        // build a clean end
        dotIndex = outString.LastIndexOf('-');
        if (dotIndex >= outString.Length - 2)
          outString = outString.Remove(dotIndex);
        dotIndex = outString.LastIndexOf('+');
        if (dotIndex >= outString.Length - 2)
          outString = outString.Remove(dotIndex);
      }
      catch (Exception ex)
      {
        Log.Error("Audioscrobber: Error while building valid url string {0}", ex.Message);
        return urlString;
      }

      return outString;
    }

    public List<Song> filterForLocalSongs(List<Song> unfilteredList_, string excludeArtist_, string currentTag_)
    {
      try
      {
        MusicDatabase mdb = new MusicDatabase();
        List<Song> tmpSongs = new List<Song>();
        Song dbSong = new Song();
        Song tmpSong = new Song();
        bool foundDoubleEntry = false;
        string tmpArtist = String.Empty;

        for (int s = 0; s < unfilteredList_.Count; s++)
        {
          // only accept other artists than the current playing
          tmpArtist = unfilteredList_[s].Artist.ToLowerInvariant();
          if (tmpArtist != excludeArtist_.ToLowerInvariant() || tmpArtist == currentTag_)
          {
            if (mdb.GetSong(unfilteredList_[s].Title, ref dbSong))
            {
              tmpSong = dbSong.Clone();
              // check and prevent entries from the same artist
              for (int j = 0; j < tmpSongs.Count; j++)
              {
                if (tmpSong.Artist == tmpSongs[j].Artist)
                {
                  foundDoubleEntry = true;
                  break;
                }
              }
              // new item therefore add it
              if (!foundDoubleEntry)
              {
                if (currentTag_ != String.Empty)
                  tmpSong.Genre = currentTag_;
                tmpSongs.Add(tmpSong);
              }
            }
          }
          //else
          //  Log.Debug("Audioscrobbler: Artist {0} inadequate - skipping", tagTracks[s].Artist);
        }
        return tmpSongs;
      }
      catch (Exception ex)
      {
        Log.Error("Audioscrobbler: filtering for local songs failed - {0}", ex.Message);
        return unfilteredList_;
      }
    }

    /// <summary>
    /// Fetch Amazon cover link, release date and album songs sortable by their popularity
    /// </summary>
    /// <param name="artistToSearch_">Band name</param>
    /// <param name="albumToSearch_">Album name</param>
    /// <param name="sortBestTracks">false gives album songs in trackorder, true by popularity</param>
    /// <returns>Song-List of Album Tracks with Title, Artist, Album, Playcount, URL(track), DateTimePlayed (album release), WebImage</returns>
    public List<Song> getAlbumInfo(string artistToSearch_, string albumToSearch_, bool sortBestTracks)
    {
      int failover = 0;
      string urlArtist = getValidURLLastFMString(artistToSearch_);
      string urlAlbum = getValidURLLastFMString(albumToSearch_);
      List<Song> albumTracks = new List<Song>();
      do
      {
        lock (LookupLock)
          albumTracks = ParseXMLDocForAlbumInfo(urlArtist, urlAlbum);

        if (sortBestTracks)
          albumTracks.Sort(CompareSongsByTimesPlayed);

        if (albumTracks.Count == 0)
        {
          failover++;
          switch (failover)
          {
            case 1:
              urlArtist = getValidURLLastFMString("The " + artistToSearch_);
              break;
            case 2:
              urlArtist = getValidURLLastFMString(artistToSearch_);
              urlAlbum = getValidURLLastFMString("The " + albumToSearch_);
              break;
            case 3:
              urlArtist = getValidURLLastFMString("The " + artistToSearch_);
              urlAlbum = getValidURLLastFMString("The " + albumToSearch_);
              break;
            default:
              Log.Debug("Audioscrobbler: No album info for {1} found after {0} tries", failover, artistToSearch_ + " - " + albumToSearch_);
              failover = 0;
              break;
          }
        }
        else
          failover = 0;

      } while (failover != 0);

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
      lock (LookupLock)
      {
        int randomPosition = 0;
        int calcRandValue = 0;
        Random rand = new Random();
        string urlArtist = getValidURLLastFMString(artistToSearch_);
        string urlTrack = getValidURLLastFMString(trackToSearch_);
        string tmpGenre = String.Empty;
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

            // only use the top 5 tags
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
              if (_unwantedTags.Contains(tmpGenre))
              {
                randomPosition = rand.Next(0, calcRandValue);
                Log.Debug("Audioscrobbler: Tag {0} in blacklist, randomly chosing another one", tmpGenre);
              }
              else
                break;
            }
          }

          if (tmpGenre != String.Empty)
          {
            // use the best matches for the given track only            
            if (sortBestTracks_)
              tagTracks = getSimilarToTag(lastFMFeed.taggedtracks, tmpGenre, false);
            else
            {
              // increase the result list
              _limitRandomListCount *= 3;
              tagTracks = getSimilarToTag(lastFMFeed.taggedtracks, tmpGenre, true);
              _limitRandomListCount /= 3;
            }

            // filter tracks not available in music database
            if (addAvailableTracksOnly)
            {
              tagTracks = filterForLocalSongs(tagTracks, artistToSearch_, tmpGenre);
            }
          }
          else
            tagTracks.Clear();
        }

        // sort list by playcount (times a track was tagged in this case)
        if (sortBestTracks_)
          tagTracks.Sort(CompareSongsByTimesPlayed);

        return tagTracks;
      }
    }

    public List<Song> getTagsForArtist(string artistToSearch_)
    {
      string urlArtist = getValidURLLastFMString(artistToSearch_);

      return ParseXMLDocForUsedTags(urlArtist, "", lastFMFeed.topartisttags);
    }

    public List<Song> getTagsForTrack(string artistToSearch_, string trackToSearch_)
    {
      string urlArtist = getValidURLLastFMString(artistToSearch_);
      string urlTrack = getValidURLLastFMString(trackToSearch_);

      return ParseXMLDocForUsedTags(urlArtist, urlTrack, lastFMFeed.toptracktags);
    }

    public List<Song> getSimilarToTag(lastFMFeed searchType_, string taggedWith_, bool randomizeList_)
    {
      if (randomizeList_)
      {
        Random rand = new Random();
        List<Song> taggedArtists = new List<Song>();
        List<Song> randomTaggedArtists = new List<Song>();
        taggedArtists = ParseXMLDocForTags(taggedWith_, searchType_);
        int artistsAdded = 0;
        int randomPosition;
        _limitRandomListCount *= 5;
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
              randomTaggedArtists.Add(taggedArtists[randomPosition]);
              artistsAdded++;
            }
          }
          _limitRandomListCount /= 5;
          // enough similar artists
          return randomTaggedArtists;
        }
        else
          // limit not reached - return all Artists
          return taggedArtists;
      }
      else
        return ParseXMLDocForTags(taggedWith_, searchType_);
    }

    public List<Song> getSimilarArtists(string Artist_, bool randomizeList_)
    {
      if (randomizeList_)
      {
        Random rand = new Random();
        List<Song> similarArtists = new List<Song>();
        List<Song> randomSimilarArtists = new List<Song>();
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

    #endregion

    #region internal fetch routines

    /// <summary>downloads the large thumb from amazon</summary>
    private bool fetchAlbumImage(string imageUrl, string fileName)
    {
      bool success = false;
      //string singleFileName = null;
      //string singlePathName = null;

      //Util.Utils.Split(fileName, singlePathName, singleFileName);
      // remove invalid chars
      //singleFileName = Util.Utils.MakeFileName(singleFileName);
      // combine again
      //fileName = System.IO.Path.Combine(singlePathName, singleFileName);
      fileName = Util.Utils.MakeFileName(fileName);

      if (imageUrl != "")
      {
        // do not download last.fm's placeholder
        if (imageUrl.IndexOf("no_album") <= 0)
        {
          //Check if we already have the file.
          string thumbspath = @"Thumbs\music\albums\";

          //Create the album subdir in thumbs if it does not exist.
          if (!System.IO.Directory.Exists(thumbspath))
            System.IO.Directory.CreateDirectory(thumbspath);

          if (!System.IO.File.Exists(thumbspath + fileName))
          {
            Log.Debug("Audioscrobbler: Trying to get thumb: {0}", imageUrl);
            // Here we get the image from the web and save it to disk
            try
            {
              WebClient client = new WebClient();
              client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
              client.DownloadFile(imageUrl, thumbspath + fileName);
              Log.Info("Audioscrobbler: Thumb successfully downloaded.");
              success = true;
            }
            catch (Exception e)
            {
              Log.Error("Audioscrobbler: Exception during downloading - {0}", e.Message);
            }
          }
          else
          {
            Log.Debug("Audioscrobbler: Thumb exists - do not download.");
          }
        }
        else
          Log.Debug("Audioscrobbler: last.fm only uses a placeholder - do not download thumb.");
      }
      else
      {
        Log.Debug("Audioscrobbler: No imageurl. Can't download thumb.");
      }
      return success;
    }


    private List<Song> fetchRandomTracks(offlineMode randomMode_)
    {
      int addedSongs = 0;
      //      Random thisOne = new Random();
      MusicDatabase dbs = new MusicDatabase();
      List<Song> songList = new List<Song>();
      Song randomSong = new Song();
      Song lookupSong = new Song();

      // fetch more than needed since there could be double entries
      while (addedSongs < _limitRandomListCount * 2)
      {
        lookupSong.Clear();

        dbs.GetRandomSong(ref lookupSong);
        randomSong = lookupSong.Clone();

        // dirty hack to improve .NET's shitty random.next()
        //if (thisOne.Next(0, 6) == thisOne.Next(0, 6))
        bool found = false;
        for (int i = 0; i < songList.Count; i++)
          if (songList[i].Artist == randomSong.Artist)
            found = true;
        if (!found)
        {
          switch (randomMode_)
          {
            case offlineMode.timesplayed:
              if (randomSong.TimesPlayed == 0)
              {
                songList.Add(randomSong);
                addedSongs++;
              }
              break;
            case offlineMode.favorites:
              if (randomSong.Favorite)
              {
                songList.Add(randomSong);
                addedSongs++;
              }
              break;
            case offlineMode.random:
              songList.Add(randomSong);
              addedSongs++;
              break;
          }
        }
      }
      return songList;
    }


    private List<Song> getOthersArtists(bool randomizeList_, lastFMFeed neighbourOrFriend_)
    {
      List<Song> myNeighbours = new List<Song>();
      List<Song> myRandomNeighbours = new List<Song>();
      List<Song> myNeighboorsArtists = new List<Song>();
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
        Random rand = new Random();
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
              if (myNeighboorsArtists[0].LastFMMatch != String.Empty)
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
              if (myNeighboorsArtists[0].LastFMMatch != String.Empty)
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
      songList = new List<Song>();
      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(@"http://ws.audioscrobbler.com/1.0/album/" + artist_ + "/" + album_ + "/info.xml");

        XmlNodeList nodes = doc.SelectNodes(@"//album");
        string tmpCover = String.Empty;
        string tmpArtist = String.Empty;
        string tmpAlbum = String.Empty;
        DateTime tmpRelease = DateTime.MinValue;
        
        if (nodes[0].Attributes["artist"].Value != "")
          tmpArtist = nodes[0].Attributes["artist"].Value;
        else
          tmpArtist = artist_;
        if (nodes[0].Attributes["title"].Value != "")
          tmpAlbum = nodes[0].Attributes["title"].Value;
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

          if (node.Attributes["title"].Value != "")
            nodeSong.Title = node.Attributes["title"].Value;

          foreach (XmlNode child in node.ChildNodes)
          {
            if (child.Name == "reach" && child.ChildNodes.Count != 0)
              nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
            else if (child.Name == "url" && child.ChildNodes.Count != 0)
              nodeSong.URL = child.ChildNodes[0].Value;
          }
          songList.Add(nodeSong);
        }
        fetchAlbumImage(tmpCover, tmpArtist + "-" + tmpAlbum + ".jpg");
      }
      catch
      {
        // input nice exception here...
      }     

      return songList;
    }

    private List<Song> ParseXMLDocForSimilarArtists(string artist_)
    {
      songList = new List<Song>();
      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(@"http://ws.audioscrobbler.com/1.0/artist/" + artist_ + "/" + "similar.xml");
        XmlNodeList nodes = doc.SelectNodes(@"//similarartists/artist");

        foreach (XmlNode node in nodes)
        {
          Song nodeSong = new Song();
          foreach (XmlNode child in node.ChildNodes)
          {
            if (child.Name == "name" && child.ChildNodes.Count != 0)
              nodeSong.Artist = child.ChildNodes[0].Value;
            else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
              nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
            else if (child.Name == "url" && child.ChildNodes.Count != 0)
              nodeSong.URL = child.ChildNodes[0].Value;
            else if (child.Name == "image" && child.ChildNodes.Count != 0)
              nodeSong.WebImage = child.ChildNodes[0].Value;
            else if (child.Name == "match" && child.ChildNodes.Count != 0)
              nodeSong.LastFMMatch = child.ChildNodes[0].Value;
          }
          if (Convert.ToInt32(nodeSong.LastFMMatch) > _minimumArtistMatchPercent)
            songList.Add(nodeSong);
        }
      }
      catch
      {
        // input nice exception here...
      }
      return songList;
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
      songList = new List<Song>();
      try
      {
        XmlDocument doc = new XmlDocument();
        XmlNodeList nodes;
        switch (searchType_)
        {
          case lastFMFeed.topartisttags:
            doc.Load(@"http://ws.audioscrobbler.com/1.0/artist/" + artist_ + "/toptags.xml");
            nodes = doc.SelectNodes(@"//toptags/tag");
            break;
          case lastFMFeed.toptracktags:
            doc.Load(@"http://ws.audioscrobbler.com/1.0/track/" + artist_ + "/" + track_ + "/toptags.xml");
            nodes = doc.SelectNodes(@"//toptags/tag");
            break;

          default:
            doc.Load(@"http://ws.audioscrobbler.com/1.0/artist/" + artist_ + "/toptags.xml");
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
              nodeSong.Genre = child.ChildNodes[0].Value;
            if (child.Name == "url" && child.ChildNodes.Count != 0)
              nodeSong.URL = child.ChildNodes[0].Value;
            if (child.Name == "count" && child.ChildNodes.Count != 0)
              nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
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

    private List<Song> ParseXMLDocForTags(string taggedWith_, lastFMFeed searchType_)
    {
      songList = new List<Song>();
      try
      {
        XmlDocument doc = new XmlDocument();
        XmlNodeList nodes;
        switch (searchType_)
        {
          case lastFMFeed.taggedartists:
            doc.Load(@"http://ws.audioscrobbler.com/1.0/tag/" + taggedWith_ + "/topartists.xml");
            nodes = doc.SelectNodes(@"//tag/artist");
            break;
          case lastFMFeed.taggedalbums:
            doc.Load(@"http://ws.audioscrobbler.com/1.0/tag/" + taggedWith_ + "/topalbums.xml");
            nodes = doc.SelectNodes(@"//tag/album");
            break;
          case lastFMFeed.taggedtracks:
            doc.Load(@"http://ws.audioscrobbler.com/1.0/tag/" + taggedWith_ + "/toptracks.xml");
            nodes = doc.SelectNodes(@"//tag/track");
            break;
          default:
            doc.Load(@"http://ws.audioscrobbler.com/1.0/tag/" + taggedWith_ + "/topartists.xml");
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
                if (node.Attributes["name"].Value != "")
                  nodeSong.Artist = node.Attributes["name"].Value;
                if (child.Name == "image" && child.ChildNodes.Count != 0)
                  nodeSong.WebImage = child.ChildNodes[0].Value;
                break;
              case lastFMFeed.taggedalbums:
                if (node.Attributes["name"].Value != "")
                  nodeSong.Album = node.Attributes["name"].Value;
                if (child.Name == "artist" && child.ChildNodes.Count != 0)
                  if (child.Attributes["name"].Value != "")
                    nodeSong.Artist = child.Attributes["name"].Value;
                break;
              case lastFMFeed.taggedtracks:
                if (node.Attributes["name"].Value != "")
                  nodeSong.Title = node.Attributes["name"].Value;
                if (child.Name == "artist" && child.ChildNodes.Count != 0)
                  if (child.Attributes["name"].Value != "")
                    nodeSong.Artist = child.Attributes["name"].Value;
                break;

            }
            if (child.Name == "mbid" && child.ChildNodes.Count != 0)
              nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
            if (child.Name == "url" && child.ChildNodes.Count != 0)
              nodeSong.URL = child.ChildNodes[0].Value;
            if (node.Attributes["count"].Value != "")
              nodeSong.TimesPlayed = Convert.ToInt32(node.Attributes["count"].Value);
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

    private List<Song> ParseXMLDoc(string xmlFileInput, string queryNodePath, lastFMFeed xmlfeed)
    {
      songList = new List<Song>();
      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(xmlFileInput);
        XmlNodeList nodes = doc.SelectNodes(queryNodePath);

        foreach (XmlNode node in nodes)
        {
          Song nodeSong = new Song();
          foreach (XmlNode child in node.ChildNodes)
          {
            switch (xmlfeed)
            {
              case (lastFMFeed.recenttracks):
                {
                  if (child.Name == "artist" && child.ChildNodes.Count != 0)
                    nodeSong.Artist = child.ChildNodes[0].Value;
                  else if (child.Name == "name" && child.ChildNodes.Count != 0)
                    nodeSong.Title = child.ChildNodes[0].Value;
                  else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
                    nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
                  else if (child.Name == "url" && child.ChildNodes.Count != 0)
                    nodeSong.URL = child.ChildNodes[0].Value;
                  else if (child.Name == "date" && child.ChildNodes.Count != 0)
                    nodeSong.DateTimePlayed = Convert.ToDateTime(child.ChildNodes[0].Value);
                }
                break;
              case (lastFMFeed.topartists):
                {
                  if (child.Name == "name" && child.ChildNodes.Count != 0)
                    nodeSong.Artist = child.ChildNodes[0].Value;
                  //else if (child.Name == "name" && child.ChildNodes.Count != 0)
                  //  nodeSong.Title = child.ChildNodes[0].Value;
                  else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
                    nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
                  else if (child.Name == "playcount" && child.ChildNodes.Count != 0)
                    nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
                  else if (child.Name == "url" && child.ChildNodes.Count != 0)
                    nodeSong.URL = child.ChildNodes[0].Value;
                  else if (child.Name == "match" && child.ChildNodes.Count != 0)
                    nodeSong.LastFMMatch = child.ChildNodes[0].Value;
                }
                break;
              case (lastFMFeed.weeklyartistchart):
                goto case lastFMFeed.topartists;
              case (lastFMFeed.toptracks):
                {
                  if (child.Name == "artist" && child.ChildNodes.Count != 0)
                    nodeSong.Artist = child.ChildNodes[0].Value;
                  else if (child.Name == "name" && child.ChildNodes.Count != 0)
                    nodeSong.Title = child.ChildNodes[0].Value;
                  else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
                    nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
                  else if (child.Name == "playcount" && child.ChildNodes.Count != 0)
                    nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
                  else if (child.Name == "url" && child.ChildNodes.Count != 0)
                    nodeSong.URL = child.ChildNodes[0].Value;
                }
                break;
              case (lastFMFeed.toptags):
                {
                  if (child.Name == "name" && child.ChildNodes.Count != 0)
                    nodeSong.Artist = child.ChildNodes[0].Value;
                  else if (child.Name == "count" && child.ChildNodes.Count != 0)
                    nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
                  else if (child.Name == "url" && child.ChildNodes.Count != 0)
                    nodeSong.URL = child.ChildNodes[0].Value;
                }
                break;
              // doesn't work atm
              case (lastFMFeed.chartstoptags):
                {
                  if (node.Attributes["name"].Value != "")
                    nodeSong.Artist = node.Attributes["name"].Value;
                  if (node.Attributes["count"].Value != "")
                    nodeSong.TimesPlayed = Convert.ToInt32(node.Attributes["count"].Value);
                  if (node.Attributes["url"].Value != "")
                    nodeSong.URL = node.Attributes["url"].Value;
                }
                break;
              case (lastFMFeed.neighbours):
                {
                  if (node.Attributes["username"].Value != "")
                    nodeSong.Artist = node.Attributes["username"].Value;
                  if (child.Name == "url" && child.ChildNodes.Count != 0)
                    nodeSong.URL = child.ChildNodes[0].Value;
                  else if (child.Name == "match" && child.ChildNodes.Count != 0)
                    nodeSong.LastFMMatch = child.ChildNodes[0].Value;
                  else if (child.Name == "image" && child.ChildNodes.Count != 0)
                    nodeSong.WebImage = child.ChildNodes[0].Value;
                }
                break;
              case (lastFMFeed.friends):
                {
                  if (node.Attributes["username"].Value != "")
                    nodeSong.Artist = node.Attributes["username"].Value;
                  if (child.Name == "url" && child.ChildNodes.Count != 0)
                    nodeSong.URL = child.ChildNodes[0].Value;
                  else if (child.Name == "image" && child.ChildNodes.Count != 0)
                    nodeSong.WebImage = child.ChildNodes[0].Value;
                  //else if (child.Name == "connections" && child.ChildNodes.Count != 0)
                  //  nodeSong.LastFMMatch = child.ChildNodes[0].Value;
                }
                break;
              case (lastFMFeed.weeklytrackchart):
                goto case lastFMFeed.toptracks;
              case (lastFMFeed.similar):
                goto case lastFMFeed.topartists;

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

  }
}
