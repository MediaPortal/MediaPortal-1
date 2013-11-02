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
using HtmlAgilityPack;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// Class that converts data scraped from allmusic.com into database objects <see cref="AlbumInfo"/>
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
    private int _iRating;
    private string _strTracks = string.Empty;
    private bool _bLoaded;

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

    #region Public Methods

    /// <summary>
    /// Take URL of an album details page and scrape details
    /// </summary>
    /// <param name="strUrl">URL of album details page</param>
    /// <returns>True if scrape was successful</returns>
    public bool Parse(string strUrl)
    {
      var albumPage = new HtmlWeb().Load(strUrl);

      // artist
      var strAlbumArtist = AllmusicSiteScraper.CleanInnerText(albumPage.DocumentNode.SelectSingleNode(@"//h3[@class=""album-artist""]/span/a"));
      
      // album
      var strAlbum = AllmusicSiteScraper.CleanInnerText(albumPage.DocumentNode.SelectSingleNode(@"//h2[@class=""album-title""]"));

      // Image URL
      var imgURL =
        AllmusicSiteScraper.CleanAttribute(
          albumPage.DocumentNode.SelectSingleNode(@"//div[@class=""album-cover""]/div[@class=""album-contain""]/img"),
          "src");
    
      // Rating
      var iRating = 0;
      var ratingMatch = AllmusicSiteScraper.CleanInnerText(albumPage.DocumentNode.SelectSingleNode(@"//div[starts-with(@class,""allmusic-rating rating-allmusic"")]"));
      int.TryParse(ratingMatch, out iRating);  
      
      // year
      var iYear = 0;
      var yearMatch = AllmusicSiteScraper.CleanInnerText(albumPage.DocumentNode.SelectSingleNode(@"//div[@class=""release-date""]/span"));
      yearMatch = Regex.Replace(yearMatch, @".*(\d{4})", @"$1");
      int.TryParse(yearMatch, out iYear);

      // review
      var strReview = AllmusicSiteScraper.CleanInnerText(albumPage.DocumentNode.SelectSingleNode(@"//div[@itemprop=""reviewBody""]"));

      // build up track listing into one string
      var strTracks = string.Empty;
      var trackNodes = albumPage.DocumentNode.SelectNodes(@"//tr[@itemprop=""track""]");
      if (trackNodes != null)
      {
        foreach (var track in trackNodes)
        {
          var trackNo = AllmusicSiteScraper.CleanInnerText(track.SelectSingleNode(@"td[@class=""tracknum""]"));
          var title =
            AllmusicSiteScraper.CleanInnerText(
              track.SelectSingleNode(@"td[@class=""title-composer""]/div[@class=""title""]/a"));
          var strDuration = AllmusicSiteScraper.CleanInnerText(track.SelectSingleNode(@"td[@class=""time""]"));
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

          strTracks += trackNo + "@" + title + "@" + iDuration.ToString(CultureInfo.InvariantCulture) + "|";
        }
      }

      // genres
      var strGenres = string.Empty;
      var genreNodes = albumPage.DocumentNode.SelectNodes(@"//section[@class=""basic-info""]/div[@class=""genre""]/div/a");
      if (genreNodes != null)
      {
        strGenres = genreNodes.Aggregate(strGenres, (current, genre) => current + (AllmusicSiteScraper.CleanInnerText(genre) + ", "));
        strGenres = strGenres.TrimEnd(new[] { ',', ' ' }); // remove trailing ", "        
      }

      // build up styles into one string
      var strThemes = string.Empty;
      var themeNodes = albumPage.DocumentNode.SelectNodes(@"//section[@class=""themes""]/div/span[@class=""theme""]/a");
      if (themeNodes != null)
      {
        strThemes = themeNodes.Aggregate(strThemes, (current, theme) => current + (AllmusicSiteScraper.CleanInnerText(theme) + ", "));
        strThemes = strThemes.TrimEnd(new[] { ',', ' ' }); // remove trailing ", "
      }

      // build up moods into one string
      var strMoods = string.Empty;
      var moodNodes = albumPage.DocumentNode.SelectNodes(@"//section[@class=""moods""]/div/span[@class=""mood""]/a");
      if (moodNodes != null)
      {
        strMoods = moodNodes.Aggregate(strMoods, (current, mood) => current + (AllmusicSiteScraper.CleanInnerText(mood) + ", "));
        strMoods = strMoods.TrimEnd(new[] { ',', ' ' }); // remove trailing ", "
      }

      var album = new AlbumInfo
      {
        Album = strAlbum,
        Artist = strAlbumArtist,
        Genre = strGenres,
        Tones = strMoods,
        Styles = strThemes,
        Review = strReview,
        Image = imgURL,
        Rating = iRating,
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