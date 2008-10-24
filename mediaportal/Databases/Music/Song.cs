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
using System.Text;
using MediaPortal.TagReader;


namespace MediaPortal.Music.Database
{
  /// <summary>
  /// The audioscrobbler submit queue status
  /// </summary>
  public enum SongStatus
  {
    Init,
    Loaded,
    Cached,
    Queued,
    Submitted,
    Short
  }

  /// <summary>
  /// The source which "suggested" the track
  /// </summary>
  public enum SongSource
  {
    /// <summary>
    /// Chosen by the user
    /// </summary>
    P,
    /// <summary>
    /// Non-personalised broadcast (e.g. Shoutcast, BBC Radio 1).
    /// </summary>
    R,
    /// <summary>
    /// Personalised recommendation except Last.fm (e.g. Pandora, Launchcast).
    /// </summary>
    E,
    /// <summary>
    /// Last.fm (any mode). prove validity of submission by sourceID (for example, "o[0]=L1b48a").
    /// </summary>
    L,
    /// <summary>
    /// Source unknown
    /// </summary>
    U,
  }

  /// <summary>
  /// A single character denoting the rating of the track. Empty if not applicable
  /// </summary>
  public enum SongAction
  {
    /// <summary>
    /// None
    /// </summary>
    N,
    /// <summary>
    /// Love (on any mode if the user has manually loved the track). This implies a listen.
    /// </summary>
    L,
    /// <summary>
    /// Ban (only if source=L). This implies a skip, and the client should skip to the next track when a ban happens.
    /// </summary>
    B,
    /// <summary>
    /// Skip (only if source=L)
    /// </summary>
    S,
  }

  [Serializable()]
  public class Song
  {
    int _iTrackId = -1;
    string _strFileName = String.Empty;
    string _strTitle = String.Empty;
    string _strArtist = String.Empty;
    string _strAlbum = String.Empty;
    string _strAlbumArtist = String.Empty;
    string _strGenre = String.Empty;
    int _iTrack = 0;
    int _iNumTracks = 0;
    int _iDuration = 0;
    int _iYear = 0;
    int _iTimedPlayed = 0;
    int _iRating = 0;
    int _iResumeAt = 0;
    bool _favorite = false;
    DateTime _dateTimeModified = DateTime.MinValue;
    DateTime _dateTimePlayed = DateTime.MinValue;
    SongStatus _audioScrobblerStatus;
    string _musicBrainzID;
    string _strURL = String.Empty;
    string _webImage = String.Empty;
    string _lastFMMatch = String.Empty;
    int _iDisc = 0;
    int _iNumDisc = 0;
    string _strLyrics = String.Empty;
    SongSource _songSource = SongSource.P;
    string _authToken = String.Empty;
    SongAction _songAction = SongAction.N;

    public Song()
    {
    }

    public Song Clone()
    {
      Song newsong = new Song();
      newsong.Id = Id;
      newsong.Album = Album;
      newsong.Artist = Artist;
      newsong.AlbumArtist = AlbumArtist;
      newsong.Duration = Duration;
      newsong.FileName = FileName;
      newsong.Genre = Genre;
      newsong.TimesPlayed = TimesPlayed;
      newsong.Title = Title;
      newsong.Track = Track;
      newsong.TrackTotal = TrackTotal;
      newsong.Year = Year;
      newsong.Rating = Rating;
      newsong.Favorite = Favorite;
      newsong.DateTimeModified = DateTimeModified;
      newsong.DateTimePlayed = DateTimePlayed;
      newsong.AudioScrobblerStatus = AudioScrobblerStatus;
      newsong.MusicBrainzID = MusicBrainzID;
      newsong.URL = URL;
      newsong.WebImage = WebImage;
      newsong.LastFMMatch = LastFMMatch;
      newsong.ResumeAt = ResumeAt;
      newsong.DiscId = DiscId;
      newsong.DiscTotal = DiscTotal;
      newsong.Lyrics = Lyrics;
      newsong.Source = Source;
      newsong.AuthToken = AuthToken;
      newsong.AudioscrobblerAction = AudioscrobblerAction;

      return newsong;
    }

    public void Clear()
    {
      _iTrackId = -1;
      _favorite = false;
      _strFileName = String.Empty;
      _strTitle = String.Empty;
      _strArtist = String.Empty;
      _strAlbum = String.Empty;
      _strAlbumArtist = String.Empty;
      _strGenre = String.Empty;
      _iTrack = 0;
      _iNumTracks = 0;
      _iDuration = 0;
      _iYear = 0;
      _iTimedPlayed = 0;
      _iRating = 0;
      _dateTimeModified = DateTime.MinValue;
      _dateTimePlayed = DateTime.MinValue;
      _audioScrobblerStatus = SongStatus.Init;
      _musicBrainzID = String.Empty;
      _strURL = String.Empty;
      _webImage = String.Empty;
      _lastFMMatch = String.Empty;
      _iResumeAt = 0;
      _iDisc = 0;
      _iNumDisc = 0;
      _strLyrics = String.Empty;
      _songSource = SongSource.P;
      _authToken = String.Empty;
      _songAction = SongAction.N;
    }

    #region Getters & Setters

    public int Id
    {
      get { return _iTrackId; }
      set { _iTrackId = value; }
    }

    public string FileName
    {
      get { return _strFileName; }
      set { _strFileName = value; }
    }

    public string Artist
    {
      get { return _strArtist; }
      set
      {
        _strArtist = value;
        //remove 01. artist name
        if (_strArtist.Length > 4)
        {
          if (Char.IsDigit(_strArtist[0]) &&
              Char.IsDigit(_strArtist[1]) &&
              _strArtist[2] == '.' &&
              _strArtist[3] == ' ')
          {
            _strArtist = _strArtist.Substring(4);
          }
        }
        //remove artist name [dddd]
        int pos = _strArtist.IndexOf("[");
        if (pos > 0)
        {
          _strArtist = _strArtist.Substring(pos);
        }
        _strArtist = _strArtist.Trim();
      }
    }

    public string AlbumArtist
    {
      get { return _strAlbumArtist; }
      set { _strAlbumArtist = value; }
    }

    public string Album
    {
      get { return _strAlbum; }
      set { _strAlbum = value; }
    }

    public string Genre
    {
      get { return _strGenre; }
      set { _strGenre = value; }
    }

    public string Title
    {
      get { return _strTitle; }
      set { _strTitle = value; }
    }

    /// <summary>
    /// Track number
    /// </summary>
    public int Track
    {
      get { return _iTrack; }
      set
      {
        _iTrack = value;
        if (_iTrack < 0)
          _iTrack = 0;
      }
    }

    public int TrackTotal
    {
      get { return _iNumTracks; }
      set
      {
        _iNumTracks = value;
        if (_iNumTracks < 0)
          _iNumTracks = 0;
      }
    }

    /// <summary>
    /// Length of song in total seconds
    /// </summary>
    public int Duration
    {
      get { return _iDuration; }
      set
      {
        _iDuration = value;
        if (_iDuration < 0)
          _iDuration = 0;
      }
    }

    public int Year
    {
      get { return _iYear; }
      set
      {
        _iYear = value;
        if (_iYear < 0)
          _iYear = 0;
        else
        {
          if (_iYear > 0 && _iYear < 100)
            _iYear += 1900;
        }
      }
    }

    public int TimesPlayed
    {
      get { return _iTimedPlayed; }
      set { _iTimedPlayed = value; }
    }

    public int Rating
    {
      get { return _iRating; }
      set { _iRating = value; }
    }

    public bool Favorite
    {
      get { return _favorite; }
      set { _favorite = value; }
    }

    public DateTime DateTimeModified
    {
      get { return _dateTimeModified; }
      set { _dateTimeModified = value; }
    }

    /// <summary>
    /// Last UTC time the song was played
    /// </summary>
    public DateTime DateTimePlayed
    {
      get { return _dateTimePlayed; }
      set { _dateTimePlayed = value; }
    }

    /// <summary>
    /// Determines whether the song has been submitted, is waiting for submit, etc
    /// </summary>
    public SongStatus AudioScrobblerStatus
    {
      get { return _audioScrobblerStatus; }
      set { _audioScrobblerStatus = value; }
    }

    /// <summary>
    /// A unique ID to indentify each track
    /// </summary>
    public string MusicBrainzID
    {
      get { return _musicBrainzID; }
      set { _musicBrainzID = value; }
    }

    public string URL
    {
      get { return _strURL; }
      set { _strURL = value; }
    }

    public string WebImage
    {
      get { return _webImage; }
      set { _webImage = value; }
    }

    public string LastFMMatch
    {
      get { return _lastFMMatch; }
      set { _lastFMMatch = value; }
    }

    public int ResumeAt
    {
      get { return _iResumeAt; }
      set
      {
        _iResumeAt = value;
        if (_iResumeAt < 0)
          _iResumeAt = 0;
      }
    }

    public int DiscId
    {
      get { return _iDisc; }
      set
      {
        _iDisc = value;
        if (_iDisc < 0)
          _iDisc = 0;
      }
    }

    public int DiscTotal
    {
      get { return _iNumDisc; }
      set
      {
        _iNumDisc = value;
        if (_iNumDisc < 0)
          _iNumDisc = 0;
      }
    }

    public string Lyrics
    {
      get { return _strLyrics; }
      set { _strLyrics = value; }
    }

    /// <summary>
    /// Indicates whether the track has been choosen by the user / a radio station / a suggestion method
    /// </summary>
    public SongSource Source
    {
      get { return _songSource; }
      set { _songSource = value; }
    }

    public string AuthToken
    {
      get { return _authToken; }
      set { _authToken = value; }
    }

    /// <summary>
    /// A submit action like BAN / LOVE / SKIP
    /// </summary>
    public SongAction AudioscrobblerAction
    {
      get { return _songAction; }
      set { _songAction = value; }
    }

    #endregion

    #region Methods

    public MusicTag ToMusicTag()
    {
      MusicTag tmpTag = new MusicTag();

      tmpTag.Title = this.Title;
      tmpTag.Album = this.Album;
      tmpTag.AlbumArtist = this.AlbumArtist;
      tmpTag.Artist = this.Artist;
      tmpTag.Duration = this.Duration;
      tmpTag.Genre = this.Genre;
      tmpTag.Track = this.Track;
      tmpTag.Year = this.Year;
      tmpTag.Rating = this.Rating;
      tmpTag.TimesPlayed = this.TimesPlayed;
      tmpTag.Lyrics = this.Lyrics;

      return tmpTag;
    }

    public string ToShortString()
    {
      StringBuilder s = new StringBuilder();

      if (_strTitle != String.Empty)
        s.Append(_strTitle);
      else
        s.Append("(Untitled)");
      if (_strArtist != String.Empty)
        s.Append(" - " + _strArtist);
      if (_strAlbum != String.Empty)
        s.Append(" (" + _strAlbum + ")");

      return s.ToString();
    }

    public string ToLastFMString()
    {
      StringBuilder s = new StringBuilder();

      if (_strArtist != String.Empty)
      {
        s.Append(_strArtist);
        s.Append(" - ");
      }
      if (_strTitle != String.Empty)
        s.Append(_strTitle);
      if (_iDuration > 0)
      {
        s.Append(" [");
        s.Append(Util.Utils.SecondsToHMSString(_iDuration));
        s.Append("]");
      }
      if (_iTimedPlayed > 0)
        s.Append(" (played: " + Convert.ToString(_iTimedPlayed) + " times)");

      return s.ToString();
    }

    public string ToLastFMMatchString(bool showURL_)
    {
      StringBuilder s = new StringBuilder();
      if (_strArtist != String.Empty)
      {
        s.Append(_strArtist);
        if (_strAlbum != String.Empty)
          s.Append(" - " + _strAlbum);
        else
        {
          if (_strTitle != String.Empty)
            s.Append(" - " + _strTitle);
          if (_strGenre != String.Empty)
            s.Append(" (tagged: " + _strGenre + ")");
        }
      }
      else
        if (_strAlbum != String.Empty)
          s.Append(_strAlbum);
      if (_lastFMMatch != String.Empty)
        if (_lastFMMatch.IndexOf(".") == -1)
          s.Append(" (match: " + _lastFMMatch + "%)");
        else
          s.Append(" (match: " + _lastFMMatch.Remove(_lastFMMatch.IndexOf(".") + 2) + "%)");
      if (showURL_)
        if (_strURL != String.Empty)
          s.Append(" (link: " + _strURL + ")");

      return s.ToString();
    }

    public string ToURLArtistString()
    {
      return System.Web.HttpUtility.UrlEncode(_strArtist);
    }

    public override string ToString()
    {
      return _strArtist + "\t" +
        _strTitle + "\t" +
        _strAlbum + "\t" +
        _musicBrainzID + "\t" +
        _iDuration + "\t" +
        _dateTimePlayed.ToString("s");
    }

    public string getQueueTime(bool asUnixTime)
    {
      string queueTime = string.Empty;

      if (asUnixTime)
        queueTime = Convert.ToString(Util.Utils.GetUnixTime(DateTimePlayed.ToUniversalTime()));
      else
      {
        queueTime = String.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}",
                                  _dateTimePlayed.Year,
                                  _dateTimePlayed.Month,
                                  _dateTimePlayed.Day,
                                  _dateTimePlayed.Hour,
                                  _dateTimePlayed.Minute,
                                  _dateTimePlayed.Second);
      }
      return queueTime;
    }

    public string getRateActionParam()
    {
      switch (_songAction)
      {
        case SongAction.N:
          return String.Empty;
        case SongAction.L:
          return "L";
        case SongAction.B:
          return "B";
        case SongAction.S:
          return "S";
        default:
          return String.Empty;
      }
    }

    public string getSourceParam()
    {
      switch (_songSource)
      {
        case SongSource.P:
          return "P";
        case SongSource.R:
          return "R";
        case SongSource.E:
          return "E";
        case SongSource.L:
          return "L" + _authToken;
        case SongSource.U:
          return "U";
        default:
          return "P";
      }
    }

    #endregion
  }

    
  public class SongMap
  {
    public string m_strPath;
    public Song m_song;
  }
}