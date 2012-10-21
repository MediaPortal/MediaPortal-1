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
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// 
  /// </summary>
  public class MusicArtistInfo
  {
    #region Variables

    private Match _match = null;
    private string _strArtistName = "";
    private string _strArtistPictureURL = "";
    private string _strAKA = "";
    private string _strBorn = "";
    private string _strYearsActive = "";
    private string _strGenres = "";
    private string _strTones = "";
    private string _strStyles = "";
    private string _strInstruments = "";
    private string _strAMGBiography = "";
    private Hashtable _relatedArtists = new Hashtable();
    private ArrayList _discographyAlbum = new ArrayList();
    private ArrayList _discographyCompilations = new ArrayList();
    private ArrayList _discographySingles = new ArrayList();
    private ArrayList _discographyMisc = new ArrayList();
    private bool _bLoaded = false;

    private string _albums = "";
    private string _compilations = "";
    private string _singles = "";
    private string _misc = "";

    #endregion

    #region regexps

    // artist regular expressions
    private const string ArtistRegExp = @"<div class=""artist-name"">(?<artist>.*?)<";
    private static readonly Regex ArtistRegEx = new Regex(ArtistRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string ArtistDetailsRegExp = @"<dl class=""details"">.*</dl>";
    private static readonly Regex ArtistDetailsRegEx = new Regex(ArtistDetailsRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string GenreRegExp = @"<dt>Genres</dt>\s*<dd class=""genres"">\s*<ul>(?<genres>.*?)</ul>";
    private static readonly Regex GenreRegEx = new Regex(GenreRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string StyleRegExp = @"<dt>Styles</dt>\s*<dd class=""styles"">\s*<ul>(?<styles>.*?)</ul>";
    private static readonly Regex StyleRegEx = new Regex(StyleRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string ActiveRegExp = @"<dt>Active</dt>\s*<dd class=""active"">(?<active>.*?)</dd>";
    private static readonly Regex ActiveRegEx = new Regex(ActiveRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string BornRegExp = @"<dd class=""birth"">\s*<span>(?<born>.*?)</span>";
    private static readonly Regex BornRegEx = new Regex(BornRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string TonesRegExp = @"<h4>artist moods</h4>\s*<ul>(?<tones>.*?)</ul>";
    private static readonly Regex TonesRegEx = new Regex(TonesRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string BIORegExp = @"<div id=""bio"">\s*<div class=""heading"">.*?</div>(?<BIO>.*?)<div class=""advertisement leaderboard"">";
    private static readonly Regex BIORegEx = new Regex(BIORegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string ImgRegExp = @"<div class=""artist-image"">\s*<div class=""image-container has-gallery"">\s*<img src=""(?<imgURL>.*?)""";
    private static readonly Regex ImgRegEx = new Regex(ImgRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string AlbumRowRegExp = @"<tr>.*?</tr>";
    private static readonly Regex AlbumRowRegEx = new Regex(AlbumRowRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string ArtistAlbumYearRegExp = @"<td class=""year.*?>(?<year>.*?)</td>";
    private static readonly Regex ArtistAlbumYearRegEx = new Regex(ArtistAlbumYearRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string ArtistAlbumNameRegExp = @"<td class=""title primary_link"".*?<a href="".*?"" class=""title.*?"" data-tooltip="".*?"">(?<albumName>.*?)</a>";
    private static readonly Regex ArtistAlbumNameRegEx = new Regex(ArtistAlbumNameRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string ArtistAlbumLabelRegExp = @"<td class=""label"".*?<span class=""full-title"">(?<label>.*?)</span>";
    private static readonly Regex ArtistAlbumLabelRegEx = new Regex(ArtistAlbumLabelRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);


    // general regular expressions
    private const string HTMLListRegExp = @"<li>.*?</li>";
    private static readonly Regex HTMLListRegEx = new Regex(HTMLListRegExp, RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private const string HTMLRegExp = @"<.*?>";
    private static readonly Regex HTMLRegEx = new Regex(HTMLRegExp, RegexOptions.Singleline | RegexOptions.Compiled);
    private const string SpaceRegExp = @"\s\s+";
    private static readonly Regex SpaceRegex = new Regex(SpaceRegExp, RegexOptions.Singleline | RegexOptions.Compiled);

    #endregion

    #region ctor

    public MusicArtistInfo() {}

    #endregion

    #region Properties

    public bool isLoaded()
    {
      return _bLoaded;
    }

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
      get { return _strAKA; }
      set { _strAKA = value.Trim(); }
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
      get { return _strAMGBiography; }
      set { _strAMGBiography = value.Trim(); }
    }

    public ArrayList DiscographyAlbums
    {
      get { return _discographyAlbum; }
      set { _discographyAlbum = value; }
    }

    public ArrayList DiscographyCompilations
    {
      get { return _discographyCompilations; }
      set { _discographyCompilations = value; }
    }

    public ArrayList DiscographySingles
    {
      get { return _discographySingles; }
      set { _discographySingles = value; }
    }

    public ArrayList DiscographyMisc
    {
      get { return _discographyMisc; }
      set { _discographyMisc = value; }
    }

    public string Albums
    {
      get
      {
        if (_albums != null && _albums.Length > 0)
        {
          return _albums;
        }

        StringBuilder strLine = new StringBuilder(2048);
        string strTmp = null;
        ArrayList list = null;
        list = DiscographyAlbums;
        for (int i = 0; i < list.Count; ++i)
        {
          string[] listInfo = (string[])list[i];
          strTmp = String.Format("{0} - {1} ({2})\n",
                                 listInfo[0], // year 
                                 listInfo[1], // title
                                 listInfo[2]); // label
          strLine.Append(strTmp);
        }
        ;
        strLine.Append('\n');
        _albums = strLine.ToString();
        return _albums;
      }
      set { _albums = value; }
    }

    public string Compilations
    {
      get
      {
        if (_compilations != null && _compilations.Length > 0)
        {
          return _compilations;
        }

        StringBuilder strLine = new StringBuilder(2048);
        string strTmp = null;
        ArrayList list = null;
        list = DiscographyCompilations;
        for (int i = 0; i < list.Count; ++i)
        {
          string[] listInfo = (string[])list[i];
          strTmp = String.Format("{0} - {1} ({2})\n",
                                 listInfo[0], // year 
                                 listInfo[1], // title
                                 listInfo[2]); // label
          strLine.Append(strTmp);
        }
        ;
        strLine.Append('\n');
        _compilations = strLine.ToString();
        return _compilations;
      }
      set { _compilations = value; }
    }

    public string Singles
    {
      get
      {
        if (_singles != null && _singles.Length > 0)
        {
          return _singles;
        }

        StringBuilder strLine = new StringBuilder(2048);
        string strTmp = null;
        ArrayList list = null;
        list = DiscographySingles;
        for (int i = 0; i < list.Count; ++i)
        {
          string[] listInfo = (string[])list[i];
          strTmp = String.Format("{0} - {1} ({2})\n",
                                 listInfo[0], // year 
                                 listInfo[1], // title
                                 listInfo[2]); // label
          strLine.Append(strTmp);
        }
        ;
        strLine.Append('\n');
        _singles = strLine.ToString();
        return _singles;
      }
      set { _singles = value; }
    }

    public string Misc
    {
      get
      {
        if (_misc != null && _misc.Length > 0)
        {
          return _misc;
        }

        StringBuilder strLine = new StringBuilder(2048);
        string strTmp = null;
        ArrayList list = null;
        list = DiscographyMisc;
        for (int i = 0; i < list.Count; ++i)
        {
          string[] listInfo = (string[])list[i];
          strTmp = String.Format("{0} - {1} ({2})\n",
                                 listInfo[0], // year 
                                 listInfo[1], // title
                                 listInfo[2]); // label
          strLine.Append(strTmp);
        }
        ;
        strLine.Append('\n');
        _misc = strLine.ToString();
        return _misc;
      }
      set { _misc = value; }
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

    /// <summary>
    /// Parse the Detail Page returned from the Allmusic Scraper
    /// </summary>
    /// <param name="strHTML"></param>
    /// <returns></returns>
    public bool Parse(string strHTML)
    {
      var match = ArtistDetailsRegEx.Match(strHTML);
      if (!match.Success)
      {
        Log.Debug("Artist HTML does not match expected format, unable to parse");
        return false;
      }
      var artistDetails = match.Value;

      var strArtist = string.Empty;
      var artistMatch = ArtistRegEx.Match(strHTML);
      if (artistMatch.Success)
      {
        strArtist = artistMatch.Groups["artist"].Value.Trim();
      }
      strArtist = System.Web.HttpUtility.HtmlDecode(strArtist);
      Log.Debug("Trying to parse html for artist: {0}", strArtist);


      // build up genres into one string
      var strGenres = string.Empty;
      var genreMatch = GenreRegEx.Match(artistDetails);
      if (genreMatch.Success)
      {
        var genres = HTMLListRegEx.Matches(genreMatch.Groups["genres"].Value.Trim());
        foreach (var genre in genres)
        {
          var cleanGenre = HTMLRegEx.Replace(genre.ToString(), "");
          strGenres += cleanGenre + ", ";
        }
        strGenres = strGenres.TrimEnd(new[] { ' ', ',' });
      }

      // build up styles into one string
      var strStyles = string.Empty;
      var styleMatch = StyleRegEx.Match(artistDetails);
      if (styleMatch.Success)
      {
        var styles = HTMLListRegEx.Matches(styleMatch.Groups["styles"].Value.Trim());
        foreach (var style in styles)
        {
          var cleanStyle = HTMLRegEx.Replace(style.ToString(), "");
          strStyles += cleanStyle + ", ";
        }
        strStyles = strStyles.TrimEnd(new[] { ' ', ',' });
      }

      // years active
      var strActive = string.Empty;
      var activeMatch = ActiveRegEx.Match(artistDetails);
      if (activeMatch.Success)
      {
        strActive = activeMatch.Groups["active"].Value.Trim();
      }

      // born / formed
      var strBorn = string.Empty;
      var bornMatch = BornRegEx.Match(artistDetails);
      if (bornMatch.Success)
      {
        strBorn = bornMatch.Groups["born"].Value.Trim();
        strBorn = strBorn.Replace("\n", ""); 
        strBorn = SpaceRegex.Replace(strBorn, " ");
      }

      // build up tones into one string
      var strTones = string.Empty;
      var tonesMatch = TonesRegEx.Match(strHTML);
      if (tonesMatch.Success)
      {
        var tones = HTMLListRegEx.Matches(tonesMatch.Groups["tones"].Value.Trim());
        foreach (var tone in tones)
        {
          var cleanTone = HTMLRegEx.Replace(tone.ToString(), "");
          strTones += cleanTone + ", ";
        }
        strTones = strTones.TrimEnd(new[] { ' ', ',' });
      }

      // Biography
      var AMGBIO = string.Empty;
      var AMGBioMatch = BIORegEx.Match(strHTML);
      if (AMGBioMatch.Success)
      {
        AMGBIO = AMGBioMatch.Groups["BIO"].Value.Trim();
        AMGBIO = HTMLRegEx.Replace(AMGBIO, "");
      }

      // artist image URL
      var strImg = string.Empty;
      var imgMatch = ImgRegEx.Match(strHTML);
      if (imgMatch.Success)
      {
        strImg = imgMatch.Groups["imgURL"].Value;
      }

      // list albums
      var albumRows = AlbumRowRegEx.Matches(strHTML);
      var albumList = string.Empty;
      foreach (Match albumRow in albumRows)
      {
        var albumNameMatch = ArtistAlbumNameRegEx.Match(albumRow.Value);
        if (!albumNameMatch.Success)
        {
          continue;
        }
        var albumName = albumNameMatch.Groups["albumName"].Value.Trim();
        var albumYear = ArtistAlbumYearRegEx.Match(albumRow.Value).Groups["year"].Value.Trim();
        var albumLabel = ArtistAlbumLabelRegEx.Match(albumRow.Value).Groups["label"].Value.Trim();
        albumList += string.Format("{0} - {1} ({2})", albumYear, albumName, albumLabel) + Environment.NewLine;
      }

      var artistInfo = new ArtistInfo
      {
        AMGBio = AMGBIO,
        Albums = albumList,
        Artist = strArtist,
        Born = strBorn,
        Compilations = string.Empty,
        Genres = strGenres,
        Image = strImg,
        Instruments = string.Empty,
        Misc = string.Empty,
        Singles = string.Empty,
        Styles = strStyles,
        Tones = strTones,
        YearsActive = strActive
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
      ArtistInfo artist = new ArtistInfo();
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