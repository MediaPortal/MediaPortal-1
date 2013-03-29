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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// 
  /// </summary>
  public class MusicAlbumInfo
  {
    #region Variables

    private string _artist = string.Empty;
    private string _strTitle = string.Empty;
    private string _strTitle2 = string.Empty;
    private string _strDateOfRelease = string.Empty;
    private string _strGenre = string.Empty;
    private string _strTones = string.Empty;
    private string _strStyles = string.Empty;
    private string _strReview = string.Empty;
    private string _strImageURL = string.Empty;
    private string _albumUrl = string.Empty;
    private string _strAlbumPath = string.Empty;
    private int _iRating = 0;
    private string _strTracks = string.Empty;
    private bool _bLoaded = false;
    private Match _match = null;

    #endregion

    #region regexps

    // album regular expressions
    private const string ArtistRegExp = @"<div class=""album-artist""><a href="".*?"">(?<artist>.*?)</a></div>";
    private static Regex ArtistRegEx = new Regex(ArtistRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumRegExp = @"<div class=""album-title"">(?<album>.*?)<";
    private static readonly Regex AlbumRegEx = new Regex(AlbumRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumImgURLRegExp = @"<div class=""image-container"" data-large="".*?http(?<imageURL>.*?)&quot;,&quot;author";
    private static readonly Regex AlbumImgURLRegEx = new Regex(AlbumImgURLRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumRatingRegExp = @"<span class=""hidden"" itemprop=""rating"">(?<rating>.*?)</span>";
    private static readonly Regex AlbumRatingRegEx = new Regex(AlbumRatingRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumYearRegExp = @"<dd class=""release-date"">.*(?<year>\d{4}?)</dd>";
    private static readonly Regex AlbumYearRegEx = new Regex(AlbumYearRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumGenreRegExp = @"<dd class=""genres"">\s*<ul>(?<genres>.*?)</ul>";
    private static readonly Regex AlbumGenreRegEx = new Regex(AlbumGenreRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumStylesRegExp = @"<dd class=""styles"">\s*<ul>(?<styles>.*?)</ul>.*</dl>";
    private static readonly Regex AlbumStylesRegEx = new Regex(AlbumStylesRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumMoodsRegExp = @"<h4>album moods</h4>\s*<ul>(?<moods>.*?)</ul>";
    private static readonly Regex AlbumMoodsRegEx = new Regex(AlbumMoodsRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumReviewRegExp = @"<div class=""editorial-text collapsible-content"" itemprop=""description"">(?<review>.*?)</div>";
    private static readonly Regex AlbumReviewRegEx = new Regex(AlbumReviewRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumTracksRegExp = @"<div id=""tracks"">.*?<tbody>(?<tracks>.*?)</tbody>";
    private static readonly Regex AlbumTracksRegEx = new Regex(AlbumTracksRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string TrackRegExp = @"<tr>.*?<td class=""tracknum"">(?<trackNo>.*?)</td>.*?<div class=""title"">\s*<a.*?>\s*(?<title>.*?)?\s*</a>.*?<td class=""time"">\s*(?<time>.*?)\s*</td>.*?</tr>";
    private static readonly Regex TrackRegEx = new Regex(TrackRegExp, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // general regular expressions
    private const string HTMLListRegExp = @"<li>.*?</li>";
    private static readonly Regex HTMLListRegEx = new Regex(HTMLListRegExp, RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private const string HTMLRegExp = @"<.*?>";
    private static readonly Regex HTMLRegEx = new Regex(HTMLRegExp, RegexOptions.Singleline | RegexOptions.Compiled);

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

    public string Tracks
    {
      get
      {
        string strFormattedTracks = string.Empty;
        string[] strTracks = _strTracks.Split(new char[] {'|'});
        foreach (string track in strTracks)
        {
          string[] strTrackParts = track.Split(new char[] {'@'});

          if( strTrackParts.Count() < 3)
          {
            continue;
          }
          int iDuration;
          int.TryParse(strTrackParts[2], out iDuration);
          string strDuration = MediaPortal.Util.Utils.SecondsToHMSString(iDuration);
          strFormattedTracks = strFormattedTracks + strTrackParts[0] + " - " + strTrackParts[1] + " " + strDuration + "\n";
        }
        return strFormattedTracks;
      }
      set { _strTracks = value; }
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

    public bool Parse(string html)
    {
      // artist
      var strAlbumArtist = string.Empty;
      var artistMatch = ArtistRegEx.Match(html);
      if (artistMatch.Success)
      {
        strAlbumArtist = artistMatch.Groups["artist"].Value.Trim();
      }

      // album
      var strAlbum = string.Empty;
      var albumMatch = AlbumRegEx.Match(html);
      if (albumMatch.Success)
      {
        strAlbum = albumMatch.Groups["album"].Value.Trim();
      }

      // Image URL
      var imgURL = string.Empty;
      var imgMatch = AlbumImgURLRegEx.Match(html);
      if (imgMatch.Success)
      {
        imgURL = imgMatch.Groups["imageURL"].Value;
        imgURL = imgURL.Replace(@"\", @"");
        if (!string.IsNullOrEmpty(imgURL))
        {
          imgURL = "http" + imgURL;
        }
      }

      // Rating
      var dRating = 0.0;
      var ratingMatch = AlbumRatingRegEx.Match(html);
      if (ratingMatch.Success)
      {
        double.TryParse(ratingMatch.Groups["rating"].Value.Trim(), out dRating);  
      }
      
      // year
      var iYear = 0;
      var yearMatch = AlbumYearRegEx.Match(html);
      if (yearMatch.Success)
      {
        int.TryParse(yearMatch.Groups["year"].Value.Trim(), out iYear);
      }

      // review
      var reviewMatch = AlbumReviewRegEx.Match(html);
      var strReview = string.Empty;
      if (reviewMatch.Success)
      {
        strReview = HTMLRegEx.Replace(reviewMatch.Groups["review"].Value.Trim(), "");
      }

      // build up track listing into one string
      var strTracks = string.Empty;
      var trackMatch = AlbumTracksRegEx.Match(html);
      if (trackMatch.Success)
      {
        var tracks = TrackRegEx.Matches(trackMatch.Groups["tracks"].Value.Trim());
        foreach (Match track in tracks)
        {
          var strDuration = track.Groups["time"].Value;
          var iDuration = 0;
          var iPos = strDuration.IndexOf(":", StringComparison.Ordinal);
          if (iPos >= 0)
          {
            var strMin = strDuration.Substring(0, iPos);
            var strSec = strDuration.Substring(iPos + 1);
            int iMin = 0, iSec = 0;
            Int32.TryParse(strMin, out iMin);
            Int32.TryParse(strSec, out iSec);
            iDuration = (iMin*60) + iSec;
          }

          strTracks += track.Groups["trackNo"].Value + "@" + track.Groups["title"].Value + "@" +
                       iDuration.ToString(CultureInfo.InvariantCulture) + "|";
        }
      }

      // build up genres into one string
      var strGenres = string.Empty;
      var genreMatch = AlbumGenreRegEx.Match(html);
      if (genreMatch.Success)
      {
        var genres = HTMLListRegEx.Matches(genreMatch.Groups["genres"].Value.Trim());
        foreach (var genre in genres)
        {
          var cleanGenre = HTMLRegEx.Replace(genre.ToString(), "");
          strGenres += cleanGenre + ", ";
        }
        strGenres = strGenres.TrimEnd(new[] {' ', ','});
      }

      // build up styles into one string
      var strStyles = string.Empty;
      var styleMatch = AlbumStylesRegEx.Match(html);
      if (styleMatch.Success)
      {
        var styles = HTMLListRegEx.Matches(styleMatch.Groups["styles"].Value.Trim());
        foreach (var style in styles)
        {
          var cleanStyle = HTMLRegEx.Replace(style.ToString(), "");
          strStyles += cleanStyle + ", ";
        }
        strStyles = strStyles.TrimEnd(new[] {' ', ','});
      }

      // build up moods into one string
      var strMoods = string.Empty;
      var moodMatch = AlbumMoodsRegEx.Match(html);
      if (moodMatch.Success)
      {
        var moods = HTMLListRegEx.Matches(moodMatch.Groups["moods"].Value.Trim());
        foreach (var mood in moods)
        {
          var cleanMood = HTMLRegEx.Replace(mood.ToString(), "");
          strMoods += cleanMood + ", ";
        }
        strMoods = strMoods.TrimEnd(new[] {' ', ','});
      }

      var album = new AlbumInfo
      {
        Album = strAlbum,
        Artist = strAlbumArtist,
        Genre = strGenres,
        Tones = strMoods,
        Styles = strStyles,
        Review = strReview,
        Image = imgURL,
        Rating = (int)(dRating * 2),
        Tracks = strTracks,
        AlbumArtist = strAlbumArtist,
        Year = iYear
      };

      Set(album);

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

    /// <summary>
    /// Return the Album Info
    /// </summary>
    /// <returns></returns>
    public AlbumInfo Get()
    {
     var album = new AlbumInfo();
      if (_bLoaded)
      {
        int iYear;

        album.Artist = _artist;
        album.Album = _strTitle;
        Int32.TryParse(_strDateOfRelease, out iYear);
        album.Year = iYear;
        album.Genre = _strGenre;
        album.Tones = _strTones;
        album.Styles = _strStyles;
        album.Review = _strReview;
        album.Image = _strImageURL;
        album.Rating = _iRating;
        album.Tracks = _strTracks;

      }
      return album;
    }

    #endregion
  }
}