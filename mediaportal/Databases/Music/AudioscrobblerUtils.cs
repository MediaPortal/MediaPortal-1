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
using System.Text;
using System.Xml;

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
    toptracktags
  }
  #endregion

  public class AudioscrobblerUtils
  {
    private bool _useDebugLog = false;
    private string _defaultUser = "";

    // Similar mode intelligence params
    private int _minimumArtistMatchPercent = 50;
    private int _limitRandomListCount = 5;
    private int _randomNessPercent = 75;
        
    // Neighbour mode intelligence params
    private lastFMFeed _currentNeighbourMode = lastFMFeed.weeklyartistchart;

    List<Song> songList = null;

    /// <summary>
    /// ctor
    /// </summary>
    public AudioscrobblerUtils()
    {
      LoadSettings();
    }

    #region Serialization
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        _randomNessPercent = xmlreader.GetValueAsInt("audioscrobbler", "randomness", 77);
        _useDebugLog = xmlreader.GetValueAsBool("audioscrobbler", "usedebuglog", false);
        _defaultUser = xmlreader.GetValueAsString("audioscrobbler", "user", "");
        int tmpNMode = xmlreader.GetValueAsInt("audioscrobbler", "neighbourmode", 1);

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
      }
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
            Log.Write("AudioscrobblerBase: minimum match for similar artists set to {0}", Convert.ToString(_minimumArtistMatchPercent));
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
            Log.Write("AudioscrobblerBase: limit for random result lists set to {0}", Convert.ToString(_limitRandomListCount));
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
            Log.Write("AudioscrobblerBase: percentage of randomness set to {0}", Convert.ToString(_randomNessPercent));
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
            Log.Write("AudioscrobblerBase: {0}", "CurrentNeighbourMode changed");
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

    public List<Song> getTagsForArtist(string artistToSearch_)
    {
      string urlArtist = System.Web.HttpUtility.UrlEncode(artistToSearch_);
      return ParseXMLDocForUsedTags(urlArtist, "", lastFMFeed.topartisttags);
    }

    public List<Song> getTagsForTrack(string artistToSearch_, string trackToSearch_)
    {
      string urlArtist = System.Web.HttpUtility.UrlEncode(artistToSearch_);
      string urlTrack = System.Web.HttpUtility.UrlEncode(trackToSearch_);
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
                foundDoubleEntry = true;
            }
            // new item therefore add it
            if (!foundDoubleEntry)
            {
              randomTaggedArtists.Add(taggedArtists[randomPosition]);
              artistsAdded++;
            }
          }
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
                foundDoubleEntry = true;
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
      List<Song> myNeighbours = new List<Song>();
      List<Song> myRandomNeighbours = new List<Song>();
      List<Song> myNeighboorsArtists = new List<Song>();
      List<Song> myRandomNeighboorsArtists = new List<Song>();
      myNeighbours = getAudioScrobblerFeed(lastFMFeed.neighbours, "");

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
                foundDoubleEntry = true;
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

            // make sure the neighbour has enough top artists
            if (myNeighboorsArtists.Count > _limitRandomListCount)
            {
              // get _limitRandomListCount artists for each random neighbour
              int artistsAdded = 0;
              int minRandAValue = _limitRandomListCount;
              int calcRandAValue = (myNeighboorsArtists.Count - 1) * _randomNessPercent / 100;
              while (artistsAdded < _limitRandomListCount)
              {
                bool foundDoubleEntry = false;
                if (calcRandAValue > minRandAValue)
                  randomPosition = rand.Next(0, calcRandAValue);
                else
                  randomPosition = rand.Next(0, minRandAValue);

                for (int j = 0; j < myRandomNeighboorsArtists.Count; j++)
                {
                  if (myRandomNeighboorsArtists.Contains(myNeighboorsArtists[randomPosition]))
                    foundDoubleEntry = true;
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
        // limit not reached - return all neighbours artists          
        {
          for (int i = 0; i < myNeighboorsArtists.Count; i++)
            myNeighboorsArtists.AddRange(getAudioScrobblerFeed(_currentNeighbourMode, myNeighbours[i].Artist));
          return myNeighboorsArtists;
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
    /// <returns>List of Song</returns>
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
  }
}
