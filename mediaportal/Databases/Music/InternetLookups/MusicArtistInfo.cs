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
using System.Linq;
using HtmlAgilityPack;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// Class that converts data scraped from allmusic.com into database objects <see cref="ArtistInfo"/>
  /// </summary>
  public class MusicArtistInfo
  {

    #region Variables

    private string _strArtistName = string.Empty;
    private string _strArtistPictureURL = string.Empty;
    private string _strAka = string.Empty;
    private string _strBorn = string.Empty;
    private string _strYearsActive = string.Empty;
    private string _strGenres = string.Empty;
    private string _strTones = string.Empty;
    private string _strStyles = string.Empty;
    private string _strInstruments = string.Empty;
    private string _strAmgBiography = string.Empty;
    private bool _bLoaded;

    private string _albums = string.Empty;
    private string _compilations = string.Empty;
    private string _singles = string.Empty;
    private string _misc = string.Empty;

    #endregion

    #region Properties

    public string Artist
    {
      get { return _strArtistName; }
      set { _strArtistName = value.Trim(); }
    }

    public string ImageURL
    {
      get { return _strArtistPictureURL; }
      set { _strArtistPictureURL = value.Trim(); }
    }

    public string Aka
    {
      get { return _strAka; }
      set { _strAka = value.Trim(); }
    }

    public string Born
    {
      get { return _strBorn; }
      set { _strBorn = value.Trim(); }
    }

    public string YearsActive
    {
      get { return _strYearsActive; }
      set { _strYearsActive = value.Trim(); }
    }

    public string Genres
    {
      get { return _strGenres; }
      set { _strGenres = value.Trim(); }
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

    public string Instruments
    {
      get { return _strInstruments; }
      set { _strInstruments = value.Trim(); }
    }

    public string AMGBiography
    {
      get { return _strAmgBiography; }
      set { _strAmgBiography = value.Trim(); }
    }

    public string Albums
    {
      get { return _albums; }
      set { _albums = value; }
    }

    public string Compilations
    {
      get { return _compilations; }
      set { _compilations = value; }
    }

    public string Singles
    {
      get { return _singles; }
      set { _singles = value; }
    }

    public string Misc
    {
      get
      { return _misc; }
      set { _misc = value; }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Parse the Detail Page returned from the Allmusic Scraper
    /// </summary>
    /// <param name="strUrl">URL of artist details page</param>
    /// <returns>True is scrape was sucessful</returns>
    public bool Parse(string strUrl)
    {
      var mainPage = new HtmlWeb().Load(strUrl);

      // moods
      var moods = string.Empty;
      var moodNodes = mainPage.DocumentNode.SelectNodes(@"//section[@class=""moods""]/ul/*");
      if (moodNodes != null)
      {
        moods = moodNodes.Aggregate(moods, (current, mood) => current + (AllmusicSiteScraper.CleanInnerText(mood) + ", "));
        moods = moods.TrimEnd(new[] {',', ' '});
      }

      // artist name
      var artistName = AllmusicSiteScraper.CleanInnerText(mainPage.DocumentNode.SelectSingleNode(@"//h2[@clas=""artist-name""]"));

      // artist image URL
      var artistImg = AllmusicSiteScraper.CleanAttribute(mainPage.DocumentNode.SelectSingleNode(@"//div[@class=""artist-image""]/img"), "src");

      //years active
      var yearsActive = AllmusicSiteScraper.CleanInnerText(mainPage.DocumentNode.SelectSingleNode(@"//section[@class=""basic-info""]/div[@class=""active-dates""]/div"));

      //genre
      var genres = string.Empty;
      var genreNodes = mainPage.DocumentNode.SelectNodes(@"//section[@class=""basic-info""]/div[@class=""genre""]/div/a");
      if (genreNodes != null)
      {
        genres = genreNodes.Aggregate(genres, (current, genre) => current + (AllmusicSiteScraper.CleanInnerText(genre) + ", "));
        genres = genres.TrimEnd(new[] { ',', ' ' }); // remove trailing ", "        
      }

      // born / formed
      var born = AllmusicSiteScraper.CleanInnerText(mainPage.DocumentNode.SelectSingleNode(@"//section[@class=""basic-info""]/div[@class=""birth""]/div"));

      // styles
      var styles = string.Empty;
      var styleNodes = mainPage.DocumentNode.SelectNodes(@"//section[@class=""basic-info""]/div[@class=""styles""]/div/a");
      if (styleNodes != null)
      {
        styles = styleNodes.Aggregate(styles, (current, style) => current + (AllmusicSiteScraper.CleanInnerText(style) + ", "));
        styles = styles.TrimEnd(new[] { ',', ' ' }); // remove trailing ", "
      }
      
      // bio
      var bio = string.Empty;
      var bioURL = "http://www.allmusic.com/" + AllmusicSiteScraper.CleanAttribute(mainPage.DocumentNode.SelectSingleNode(@"//ul[@class=""tabs overview""]/li[@class=""tab biography""]/a"), "href");
      if (!string.IsNullOrEmpty(bioURL))
      {
        var bioPage = new HtmlWeb().Load(bioURL);
        bio = AllmusicSiteScraper.CleanInnerText(bioPage.DocumentNode.SelectSingleNode(@"//section[@class=""biography""]/div[@class=""text""]"));
      }

      // albums
      var albumList = string.Empty;
      var albumPageURL = "http://www.allmusic.com/" + AllmusicSiteScraper.CleanAttribute(mainPage.DocumentNode.SelectSingleNode(@"//ul[@class=""tabs overview""]/li[@class=""tab discography""]/a"), "href");
      if (!string.IsNullOrEmpty(albumPageURL))
      {
        var albumPage = new HtmlWeb().Load(albumPageURL);
        var albums = albumPage.DocumentNode.SelectNodes(@"//section[@class=""discography""]/table/tbody/tr");
        if (albums != null)
        {
          foreach (var album in albums)
          {
            var year = AllmusicSiteScraper.CleanInnerText(album.SelectSingleNode(@"td[@class=""year""]"));
            var title = AllmusicSiteScraper.CleanInnerText(album.SelectSingleNode(@"td[@class=""title""]/a"));
            var label = AllmusicSiteScraper.CleanInnerText(album.SelectSingleNode(@"td[@class=""label""]"));

            albumList += year + " - " + title + " (" + label + ")" + Environment.NewLine;
          }
        }
      }


      var artistInfo = new ArtistInfo
      {
        AMGBio = bio,
        Albums = albumList,
        Artist = artistName,
        Born = born,
        Compilations = string.Empty,
        Genres = genres,
        Image = artistImg,
        Instruments = string.Empty,
        Misc = string.Empty,
        Singles = string.Empty,
        Styles = styles,
        Tones = moods,
        YearsActive = yearsActive
      };

      Set(artistInfo);

      _bLoaded = true;
      return _bLoaded;
    }

    /// <summary>
    /// Set the ArtistInfo 
    /// </summary>
    /// <param name="artist"></param>
    public void Set(ArtistInfo artist)
    {
      Artist = artist.Artist;
      Born = artist.Born;
      YearsActive = artist.YearsActive;
      Genres = artist.Genres;
      Tones = artist.Tones;
      Styles = artist.Styles;
      Instruments = artist.Instruments;
      AMGBiography = artist.AMGBio;
      ImageURL = artist.Image;
      Albums = artist.Albums;
      Compilations = artist.Compilations;
      Singles = artist.Singles;
      Misc = artist.Misc;

      _bLoaded = true;
    }

    /// <summary>
    /// Return the Artist Info
    /// </summary>
    /// <returns></returns>
    public ArtistInfo Get()
    {
      var artist = new ArtistInfo();
      if (_bLoaded)
      {
        artist.Artist = Artist;
        artist.Born = Born;
        artist.YearsActive = YearsActive;
        artist.Genres = Genres;
        artist.Tones = Tones;
        artist.Styles = Styles;
        artist.Instruments = Instruments;
        artist.AMGBio = AMGBiography;
        artist.Image = ImageURL;
        artist.Albums = Albums;
        artist.Compilations = Compilations;
        artist.Singles = Singles;
        artist.Misc = Misc;
      }
      return artist;
    }

    #endregion
  }
}