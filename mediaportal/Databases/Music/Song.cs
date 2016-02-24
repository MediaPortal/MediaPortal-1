#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Text;
using MediaPortal.TagReader;
using MediaPortal.Playlists;

namespace MediaPortal.Music.Database
{
  [Serializable()]
  public class Song
  {
    private string _strArtist = String.Empty;
    private int _iTrack = 0;
    private int _iNumTracks = 0;
    private int _iDuration = 0;
    private int _iYear = 0;
    private int _iResumeAt = 0;
    private int _iDisc = 0;
    private int _iNumDisc = 0;

    public Song()
    {
      AuthToken = String.Empty;
      Lyrics = String.Empty;
      URL = String.Empty;
      DateTimeModified = DateTime.MinValue;
      Favorite = false;
      Rating = 0;
      TimesPlayed = 0;
      Title = String.Empty;
      Conductor = String.Empty;
      Genre = String.Empty;
      Album = String.Empty;
      AlbumArtist = String.Empty;
      FileName = String.Empty;
      Id = -1;
      Composer = String.Empty;
      Comment = String.Empty;
      FileType = String.Empty;
      Codec = String.Empty;
      BitRateMode = String.Empty;
      BPM = 0;
      BitRate = 0;
      Channels = 0;
      SampleRate = 0;
      DateTimePlayed = DateTime.MinValue;
    }

    public Song Clone()
    {
      var newsong = new Song
                      {
                        Id = Id,
                        Album = Album,
                        Artist = Artist,
                        AlbumArtist = AlbumArtist,
                        Duration = Duration,
                        FileName = FileName,
                        Genre = Genre,
                        Composer = Composer,
                        Conductor = Conductor,
                        TimesPlayed = TimesPlayed,
                        Title = Title,
                        Track = Track,
                        TrackTotal = TrackTotal,
                        Year = Year,
                        Rating = Rating,
                        Favorite = Favorite,
                        DateTimeModified = DateTimeModified,
                        DateTimePlayed = DateTimePlayed,
                        URL = URL,
                        ResumeAt = ResumeAt,
                        DiscId = DiscId,
                        DiscTotal = DiscTotal,
                        Lyrics = Lyrics,
                        AuthToken = AuthToken,
                        Comment = Comment,
                        FileType = FileType,
                        Codec = Codec,
                        BitRateMode = BitRateMode,
                        BPM = BPM,
                        Channels = Channels,
                        SampleRate = SampleRate
                      };

      return newsong;
    }

    public void Clear()
    {
      Id = -1;
      Favorite = false;
      FileName = String.Empty;
      Title = String.Empty;
      _strArtist = String.Empty;
      Album = String.Empty;
      AlbumArtist = String.Empty;
      Genre = String.Empty;
      Composer = String.Empty;
      Conductor = String.Empty;
      _iTrack = 0;
      _iNumTracks = 0;
      _iDuration = 0;
      _iYear = 0;
      TimesPlayed = 0;
      Rating = 0;
      DateTimeModified = DateTime.MinValue;
      DateTimePlayed = DateTime.MinValue;
      URL = String.Empty;
      _iResumeAt = 0;
      _iDisc = 0;
      _iNumDisc = 0;
      Lyrics = String.Empty;
      AuthToken = String.Empty;
      Comment = String.Empty;
      FileType = String.Empty;
      Codec = String.Empty;
      BitRateMode = String.Empty;
      BPM = 0;
      BitRate = 0;
      Channels = 0;
      SampleRate = 0;
    }

    #region Getters & Setters

    public int Id { get; set; }

    public string FileName { get; set; }

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

    public string AlbumArtist { get; set; }

    public string Album { get; set; }

    public string Genre { get; set; }

    public string Composer { get; set; }

    public string Conductor { get; set; }

    public string Title { get; set; }

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
        {
          _iTrack = 0;
        }
      }
    }

    public int TrackTotal
    {
      get { return _iNumTracks; }
      set
      {
        _iNumTracks = value;
        if (_iNumTracks < 0)
        {
          _iNumTracks = 0;
        }
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
        {
          _iDuration = 0;
        }
      }
    }

    public int Year
    {
      get { return _iYear; }
      set
      {
        _iYear = value;
        if (_iYear < 0)
        {
          _iYear = 0;
        }
        else
        {
          if (_iYear > 0 && _iYear < 100)
          {
            _iYear += 1900;
          }
        }
      }
    }

    public int TimesPlayed { get; set; }

    public int Rating { get; set; }

    public bool Favorite { get; set; }

    public DateTime DateTimeModified { get; set; }

    /// <summary>
    /// Last UTC time the song was played
    /// </summary>
    public DateTime DateTimePlayed { get; set; }

    public string URL { get; set; }

    public int ResumeAt
    {
      get { return _iResumeAt; }
      set
      {
        _iResumeAt = value;
        if (_iResumeAt < 0)
        {
          _iResumeAt = 0;
        }
      }
    }

    public int DiscId
    {
      get { return _iDisc; }
      set
      {
        _iDisc = value;
        if (_iDisc < 0)
        {
          _iDisc = 0;
        }
      }
    }

    public int DiscTotal
    {
      get { return _iNumDisc; }
      set
      {
        _iNumDisc = value;
        if (_iNumDisc < 0)
        {
          _iNumDisc = 0;
        }
      }
    }

    public string Lyrics { get; set; }

    public string Comment { get; set; }

    public string FileType { get; set; }

    public string Codec { get; set; }

    public string BitRateMode { get; set; }

    public int BPM { get; set; }

    public int BitRate { get; set; }

    public int Channels { get; set; }

    public int SampleRate { get; set; }

    public string AuthToken { get; set; }

    #endregion

    #region Methods

    public MusicTag ToMusicTag()
    {
      var tmpTag = new MusicTag
                     {
                       Title = this.Title,
                       Album = this.Album,
                       DiscID = this.DiscId,
                       DiscTotal = this.DiscTotal,
                       AlbumArtist = this.AlbumArtist,
                       Artist = this.Artist,
                       Duration = this.Duration,
                       Genre = this.Genre,
                       Composer = this.Composer,
                       Conductor = this.Conductor,
                       Track = this.Track,
                       TrackTotal = this.TrackTotal,
                       Year = this.Year,
                       Rating = this.Rating,
                       TimesPlayed = this.TimesPlayed,
                       Lyrics = this.Lyrics,
                       DateTimeModified = this.DateTimeModified,
                       DateTimePlayed = this.DateTimePlayed,
                       Comment = this.Comment,
                       FileType = this.FileType,
                       Codec = this.Codec,
                       BitRateMode = this.BitRateMode,
                       BPM = this.BPM,
                       BitRate = this.BitRate,
                       Channels = this.Channels,
                       SampleRate = this.SampleRate,
                       HasAlbumArtist = string.IsNullOrEmpty(this.AlbumArtist)
                     };

      return tmpTag;
    }

    public PlayListItem ToPlayListItem()
    {
      var pli = new PlayListItem
                  {
                    Type = PlayListItem.PlayListItemType.Audio,
                    FileName = this.FileName,
                    Description = this.Title,
                    Duration = this.Duration
                  };

      MusicTag tag = this.ToMusicTag();
      pli.MusicTag = tag;

      return pli;
    }

    public string ToShortString()
    {
      StringBuilder s = new StringBuilder();

      if (Title != String.Empty)
      {
        s.Append(Title);
      }
      else
      {
        s.Append("(Untitled)");
      }
      if (_strArtist != String.Empty)
      {
        s.Append(" - " + _strArtist);
      }
      if (Album != String.Empty)
      {
        s.Append(" (" + Album + ")");
      }

      return s.ToString();
    }

    public override string ToString()
    {
      return _strArtist + "\t" +
             Title + "\t" +
             Album + "\t" +
             _iDuration + "\t" +
             DateTimePlayed.ToString("s");
    }

    #endregion
  }


  public class SongMap
  {
    public string m_strPath;
    public Song m_song;
  }
}