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
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// 
  /// </summary>
  public class MusicAlbumInfo
  {
    #region Variables

    private string _artist = "";
    private string _strTitle = "";
    private string _strTitle2 = "";
    private string _strDateOfRelease = "";
    private string _strGenre = "";
    private string _strTones = "";
    private string _strStyles = "";
    private string _strReview = "";
    private string _strImageURL = "";
    private string _albumUrl = "";
    private string _strAlbumPath = "";
    private int _iRating = 0;
    private ArrayList _songs = new ArrayList();
    private bool _bLoaded = false;
    private Match _match = null;

    #endregion

    #region ctor

    public MusicAlbumInfo() {}

    #endregion

    #region Properties

    public string Artist
    {
      get { return _artist; }
      set { _artist = value.Trim(); }
    }

    public string Title
    {
      get { return _strTitle; }
      set { _strTitle = value.Trim(); }
    }

    public string Title2
    {
      get { return _strTitle2; }
      set { _strTitle2 = value.Trim(); }
    }

    public string DateOfRelease
    {
      get { return _strDateOfRelease; }
      set
      {
        _strDateOfRelease = value.Trim();
        try
        {
          int iYear = Int32.Parse(_strDateOfRelease);
        }
        catch (Exception)
        {
          _strDateOfRelease = "0";
        }
      }
    }

    public string Genre
    {
      get { return _strGenre; }
      set { _strGenre = value.Trim(); }
    }

    public string Tones
    {
      get { return _strTones; }
      set { _strTones = value.Trim(); }
    }

    public string Styles
    {
      get { return _strStyles; }
      set { _strStyles = value; }
    }

    public string Review
    {
      get { return _strReview; }
      set { _strReview = value.Trim(); }
    }

    public string ImageURL
    {
      get { return _strImageURL; }
      set { _strImageURL = value.Trim(); }
    }

    public string AlbumURL
    {
      get { return _albumUrl; }
      set { _albumUrl = value.Trim(); }
    }

    public string AlbumPath
    {
      get { return _strAlbumPath; }
      set { _strAlbumPath = value.Trim(); }
    }

    public int Rating
    {
      get { return _iRating; }
      set { _iRating = value; }
    }

    public int NumberOfSongs
    {
      get { return _songs.Count; }
    }

    public string Tracks
    {
      get
      {
        string strTracks = "";
        foreach (MusicSong song in _songs)
        {
          string strTmp = String.Format("{0}@{1}@{2}|", song.Track, song.SongName, song.Duration);
          strTracks = strTracks + strTmp;
        }
        return strTracks;
      }
      set
      {
        _songs.Clear();
        Tokens token = new Tokens(value, new char[] {'|'});
        foreach (string strToken in token)
        {
          Tokens token2 = new Tokens(strToken, new char[] {'@'});
          MusicSong song = new MusicSong();
          int iTok = 0;
          foreach (string strCol in token2)
          {
            switch (iTok)
            {
              case 0:
                try
                {
                  song.Track = Int32.Parse(strCol);
                }
                catch (Exception) {}
                break;
              case 1:
                song.SongName = strCol;
                break;

              case 2:
                try
                {
                  song.Duration = Int32.Parse(strCol);
                }
                catch (Exception) {}
                break;
            }
            iTok++;
          }
          if (song.Track > 0)
          {
            _songs.Add(song);
          }
        }
      }
    }

    public bool Loaded
    {
      get { return _bLoaded; }
      set { _bLoaded = value; }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Do a Regex search with the given pattern and fill the Match object
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="searchString"></param>
    /// <returns></returns>
    private bool FindPattern(string pattern, string searchString)
    {
      Regex itemsFound = new Regex(
        pattern,
        RegexOptions.IgnoreCase
        | RegexOptions.Multiline
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.Compiled
        );

      _match = itemsFound.Match(searchString);
      if (_match.Success)
      {
        return true;
      }

      return false;
    }

    #endregion

    #region Public Methods

    public MusicSong GetSong(int iSong)
    {
      return (MusicSong)_songs[iSong];
    }

    public bool Parse(string html)
    {
      _songs.Clear();
      HTMLUtil util = new HTMLUtil();
      string strHtmlLow = html.ToLower();

      int begIndex = 0;
      int endIndex = 0;

      //	Extract Cover URL
      string pattern = @"<!--Begin.*?Album.*?Photo-->\s*?.*?<img.*?src=\""(.*?)\""";
      if (FindPattern(pattern, html))
      {
        _strImageURL = _match.Groups[1].Value;
      }

      //	Extract Review
      pattern = @"<td.*?class=""tab_off""><a.*?href=""(.*?)"">.*?Review.*?</a>";
      if (FindPattern(pattern, html))
      {
        try
        {
          string contentinfo = AllmusicSiteScraper.GetHTTP(_match.Groups[1].Value);
          pattern = @"<p.*?class=""author"">.*\s*?.*?<p.*?class=""text"">(.*?)</p>";
          if (FindPattern(pattern, contentinfo))
          {
            string data = _match.Groups[1].Value;
            util.RemoveTags(ref data);
            util.ConvertHTMLToAnsi(data, out data);
            _strReview = data.Trim();
          }
        }
        catch (Exception) {}
      }

      //	Extract Artist
      pattern = @"<h3.*?artist</h3>\s*?.*?<a.*"">(.*)</a>";
      if (FindPattern(pattern, html))
      {
        _artist = _match.Groups[1].Value;
        util.RemoveTags(ref _artist);
      }

      //	Extract Album
      pattern = @"<h3.*?album</h3>\s*?.*?<p>(.*)</P>";
      if (FindPattern(pattern, html))
      {
        _strTitle = _match.Groups[1].Value;
        util.RemoveTags(ref _strTitle);
      }

      // Extract Rating
      pattern = @"<h3.*?rating</h3>\s*?.*?src=""(.*?)""";
      if (FindPattern(pattern, html))
      {
        string strRating = _match.Groups[1].Value;
        util.RemoveTags(ref strRating);
        strRating = strRating.Substring(26, 1);
        try
        {
          _iRating = Int32.Parse(strRating);
        }
        catch (Exception) {}
      }

      //	Release Date
      pattern = @"<h3.*?release.*?date</h3>\s*?.*?<p>(.*)</P>";
      if (FindPattern(pattern, html))
      {
        _strDateOfRelease = _match.Groups[1].Value;
        util.RemoveTags(ref _strDateOfRelease);

        //	extract the year out of something like "1998 (release)" or "12 feb 2003"
        int nPos = _strDateOfRelease.IndexOf("19");
        if (nPos > -1)
        {
          if ((int)_strDateOfRelease.Length >= nPos + 3 && Char.IsDigit(_strDateOfRelease[nPos + 2]) &&
              Char.IsDigit(_strDateOfRelease[nPos + 3]))
          {
            string strYear = _strDateOfRelease.Substring(nPos, 4);
            _strDateOfRelease = strYear;
          }
          else
          {
            nPos = _strDateOfRelease.IndexOf("19", nPos + 2);
            if (nPos > -1)
            {
              if ((int)_strDateOfRelease.Length >= nPos + 3 && Char.IsDigit(_strDateOfRelease[nPos + 2]) &&
                  Char.IsDigit(_strDateOfRelease[nPos + 3]))
              {
                string strYear = _strDateOfRelease.Substring(nPos, 4);
                _strDateOfRelease = strYear;
              }
            }
          }
        }

        nPos = _strDateOfRelease.IndexOf("20");
        if (nPos > -1)
        {
          if ((int)_strDateOfRelease.Length > nPos + 3 && Char.IsDigit(_strDateOfRelease[nPos + 2]) &&
              Char.IsDigit(_strDateOfRelease[nPos + 3]))
          {
            string strYear = _strDateOfRelease.Substring(nPos, 4);
            _strDateOfRelease = strYear;
          }
          else
          {
            nPos = _strDateOfRelease.IndexOf("20", nPos + 1);
            if (nPos > -1)
            {
              if ((int)_strDateOfRelease.Length > nPos + 3 && Char.IsDigit(_strDateOfRelease[nPos + 2]) &&
                  Char.IsDigit(_strDateOfRelease[nPos + 3]))
              {
                string strYear = _strDateOfRelease.Substring(nPos, 4);
                _strDateOfRelease = strYear;
              }
            }
          }
        }
      }

      // Extract Genre
      begIndex = strHtmlLow.IndexOf("<h3>genre</h3>");
      endIndex = strHtmlLow.IndexOf("</div>", begIndex + 2);
      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = html.Substring(begIndex, endIndex - begIndex);
        pattern = @"(<li>(.*?)</li>)";
        if (FindPattern(pattern, contentInfo))
        {
          string data = "";
          while (_match.Success)
          {
            data += string.Format("{0}, ", _match.Groups[2].Value);
            _match = _match.NextMatch();
          }
          util.RemoveTags(ref data);
          util.ConvertHTMLToAnsi(data, out _strGenre);
          _strGenre = _strGenre.Trim(new[] {' ', ','});
        }
      }

      // Extract Styles
      begIndex = strHtmlLow.IndexOf("<h3>style</h3>");
      endIndex = strHtmlLow.IndexOf("</div>", begIndex + 2);
      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = html.Substring(begIndex, endIndex - begIndex);
        pattern = @"(<li>(.*?)</li>)";
        if (FindPattern(pattern, contentInfo))
        {
          string data = "";
          while (_match.Success)
          {
            data += string.Format("{0}, ", _match.Groups[2].Value);
            _match = _match.NextMatch();
          }
          util.RemoveTags(ref data);
          util.ConvertHTMLToAnsi(data, out _strStyles);
          _strStyles = _strStyles.Trim(new[] {' ', ','});
        }
      }

      // Extract Moods
      begIndex = strHtmlLow.IndexOf("<h3>moods</h3>");
      endIndex = strHtmlLow.IndexOf("</div>", begIndex + 2);
      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = html.Substring(begIndex, endIndex - begIndex);
        pattern = @"(<li>(.*?)</li>)";
        if (FindPattern(pattern, contentInfo))
        {
          string data = "";
          while (_match.Success)
          {
            data += string.Format("{0}, ", _match.Groups[2].Value);
            _match = _match.NextMatch();
          }
          util.RemoveTags(ref data);
          util.ConvertHTMLToAnsi(data, out _strTones);
          _strTones = _strTones.Trim(new[] {' ', ','});
        }
      }

      // Extract Songs
      begIndex = strHtmlLow.IndexOf("<!-- tracks table -->");
      endIndex = strHtmlLow.IndexOf("<!-- end tracks table -->", begIndex + 2);
      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = html.Substring(begIndex, endIndex - begIndex);
        pattern = @"<tr.*class=""visible"".*?\s*?<td.*</td>\s*?.*<td.*</td>\s*?.*<td.*?>(?<track>.*)</td>" +
                  @"\s*?.*<td.*</td>\s*?.*<td.*?>(?<title>.*)</td>\s*?.*?<td.*?>\s*?.*</td>\s*?.*?<td.*?>(?<duration>.*)</td>";

        if (FindPattern(pattern, contentInfo))
        {
          while (_match.Success)
          {
            //	Tracknumber
            int iTrack = 0;
            try
            {
              iTrack = Int32.Parse(_match.Groups["track"].Value);
            }
            catch (Exception) {}

            // Song Title
            string strTitle = _match.Groups["title"].Value;
            util.RemoveTags(ref strTitle);
            util.ConvertHTMLToAnsi(strTitle, out strTitle);

            //	Duration
            int iDuration = 0;
            string strDuration = _match.Groups["duration"].Value;
            int iPos = strDuration.IndexOf(":");
            if (iPos >= 0)
            {
              string strMin, strSec;
              strMin = strDuration.Substring(0, iPos);
              iPos++;
              strSec = strDuration.Substring(iPos);
              int iMin = 0, iSec = 0;
              try
              {
                iMin = Int32.Parse(strMin);
                iSec = Int32.Parse(strSec);
              }
              catch (Exception) {}
              iDuration = iMin * 60 + iSec;
            }

            //	Create new song object
            MusicSong newSong = new MusicSong();
            newSong.Track = iTrack;
            newSong.SongName = strTitle;
            newSong.Duration = iDuration;
            _songs.Add(newSong);

            _match = _match.NextMatch();
          }
        }
      }

      //	Set to "Not available" if no value from web
      if (_artist.Length == 0)
      {
        _artist = GUILocalizeStrings.Get(416);
      }
      if (_strDateOfRelease.Length == 0)
      {
        _strDateOfRelease = GUILocalizeStrings.Get(416);
      }
      if (_strGenre.Length == 0)
      {
        _strGenre = GUILocalizeStrings.Get(416);
      }
      if (_strTones.Length == 0)
      {
        _strTones = GUILocalizeStrings.Get(416);
      }
      if (_strStyles.Length == 0)
      {
        _strStyles = GUILocalizeStrings.Get(416);
      }
      if (_strTitle.Length == 0)
      {
        _strTitle = GUILocalizeStrings.Get(416);
      }

      if (_strTitle2.Length == 0)
      {
        _strTitle2 = _strTitle;
      }

      Loaded = true;
      return true;
    }

    public void Set(AlbumInfo album)
    {
      Artist = album.Artist;
      Title = album.Album;
      _strDateOfRelease = String.Format("{0}", album.Year);
      Genre = album.Genre;
      Tones = album.Tones;
      Styles = album.Styles;
      Review = album.Review;
      ImageURL = album.Image;
      Rating = album.Rating;
      Tracks = album.Tracks;
      Title2 = "";
      Loaded = true;
    }

    public void SetSongs(ArrayList list)
    {
      _songs.Clear();
      foreach (MusicSong song in list)
      {
        _songs.Add(song);
      }
    }

    #endregion
  }
}