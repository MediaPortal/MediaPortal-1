#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.Util;

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

    #region ctor

    public MusicArtistInfo() { }

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
      HTMLUtil util = new HTMLUtil();
      int begIndex = 0;
      int endIndex = 0;
      string strHTMLLow = strHTML.ToLower();

      // Get the Artist Name
      string pattern = @"<h1.*class=""title"">(.*)</h1>";
      if (!FindPattern(pattern, strHTML))
      {
        return false;
      }

      _strArtistName = _match.Groups[1].Value;

      // Born
      pattern = @"<h3>.*Born.*</h3>\s*?<p>(.*)</p>";
      if (FindPattern(pattern, strHTML))
      {
        string strValue = _match.Groups[1].Value;
        util.RemoveTags(ref strValue);
        util.ConvertHTMLToAnsi(strValue, out _strBorn);
        _strBorn = _strBorn.Trim();
      }

      // Years Active
      pattern = @"(<span.*?class=""active"">(.*?)</span>)";
      if (FindPattern(pattern, strHTML))
      {
        while (_match.Success)
        {
          _strYearsActive += string.Format("{0}s, ", _match.Groups[2].Value);
          _match = _match.NextMatch();
        }
        _strYearsActive = _strYearsActive.Trim(new[] {' ', ','});
      }

      // Genre
      pattern = @"<div.*?id=""genre-style"">\s*?.*?\s*?<h3>.*?Genres.*?</h3>\s*?.*?(<p>(.*?)</p>)";
      if (FindPattern(pattern, strHTML))
      {
        string data = "";
        while (_match.Success)
        {
          data += string.Format("{0}, ", _match.Groups[2].Value);
          _match = _match.NextMatch();
        }
        util.RemoveTags(ref data);
        util.ConvertHTMLToAnsi(data, out _strGenres);
        _strGenres = _strGenres.Trim(new[] { ' ', ',' });
      }

      // Style
      begIndex = strHTMLLow.IndexOf("<h3>styles</h3>");
      endIndex = strHTMLLow.IndexOf("<!--end genre/styles-->", begIndex + 2);

      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = strHTML.Substring(begIndex, endIndex - begIndex);
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

      // Mood
      begIndex = strHTMLLow.IndexOf("<h3>moods</h3>");
      endIndex = strHTMLLow.IndexOf("</div>", begIndex + 2);
      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = strHTML.Substring(begIndex, endIndex - begIndex);
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
          _strTones = _strTones.Trim(new[] { ' ', ',' });
        }
      }

      // Instruments
      begIndex = strHTMLLow.IndexOf("<h3>instruments</h3>");
      endIndex = strHTMLLow.IndexOf("</div>", begIndex + 2);
      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = strHTML.Substring(begIndex, endIndex - begIndex);
        if (FindPattern(pattern, contentInfo))
        {
          string data = "";
          while (_match.Success)
          {
            data += string.Format("{0}, ", _match.Groups[2].Value);
            _match = _match.NextMatch();
          }
          util.RemoveTags(ref data);
          util.ConvertHTMLToAnsi(data, out _strInstruments);
          _strInstruments = _strInstruments.Trim(new[] { ' ', ',' });
        }
      }

      // picture URL
      pattern = @"<div.*?class=""image"">\s*?.*<img.*id=""artist_image"".*?src=\""(.*?)\""";
      if (FindPattern(pattern, strHTML))
      {
        _strArtistPictureURL = _match.Groups[1].Value;
      }

      // parse AMG BIOGRAPHY
      pattern = @"<td.*?class=""tab_off""><a.*?href=""(.*?)"">.*?Biography.*?</a>";
      if (FindPattern(pattern, strHTML))
      {
        try
        {
          string contentinfo = AllmusicSiteScraper.GetHTTP(_match.Groups[1].Value);
          begIndex = contentinfo.IndexOf("<!--Begin Biography -->");
          endIndex = contentinfo.IndexOf("</div>", begIndex + 2);
          if (begIndex != -1 && endIndex != -1)
          {
            pattern = @"<p.*?class=""text"">(.*?)</p>";
            if (FindPattern(pattern, contentinfo))
            {
              string data = _match.Groups[1].Value;
              util.RemoveTags(ref data);
              util.ConvertHTMLToAnsi(data, out data);
              _strAMGBiography = data.Trim();
            }
          }
        }
        catch (Exception)
        {
        }
      }


      string compilationPage = "";
      string singlesPage = "";
      string dvdPage = "";
      string miscPage = "";    

      // discography (albums)
      pattern = @"<td.*class=""tab_off""><a.*?href=""(.*?)"">.*Discography.*</a>";
      if (FindPattern(pattern, strHTML))
      {
        // Get Link to other sub pages
        compilationPage = _match.Groups[1].Value + "/compilations";
        singlesPage = _match.Groups[1].Value + "/singles-eps";
        dvdPage = _match.Groups[1].Value + "/dvds-videos";
        miscPage = _match.Groups[1].Value + "/other";

        try
        {
          string contentinfo = AllmusicSiteScraper.GetHTTP(_match.Groups[1].Value);
          pattern = @"sorted.*? cell"">(?<year>.*?)</td>\s*?.*?</td>\s*.*?<a.*?"">(?<album>.*?)" + 
                    @"</a>.*?</td>\s*.*?</td>\s*.*?"">(?<label>.*?)</td>";

          if (FindPattern(pattern, contentinfo))
          {
            while (_match.Success)
            {
              string year = _match.Groups["year"].Value;
              string albumTitle = _match.Groups["album"].Value;
              string label = _match.Groups["label"].Value;

              util.RemoveTags(ref year);
              util.ConvertHTMLToAnsi(year, out year);
              util.RemoveTags(ref albumTitle);
              util.ConvertHTMLToAnsi(albumTitle, out albumTitle);
              util.RemoveTags(ref label);
              util.ConvertHTMLToAnsi(label, out label);

              try
              {
                string[] dAlbumInfo = { year.Trim(), albumTitle.Trim(), label.Trim() };
                _discographyAlbum.Add(dAlbumInfo);
              }
              catch { }

              _match = _match.NextMatch();
            }
          }
        }
        catch (Exception)
        {
        }
      }

      // Compilations
      if (compilationPage != "")
      {
        try
        {
          string contentinfo = AllmusicSiteScraper.GetHTTP(compilationPage);
          pattern = @"sorted.*? cell"">(?<year>.*?)</td>\s*?.*?</td>\s*.*?<a.*?"">(?<album>.*?)" +
                    @"</a>.*?</td>\s*.*?</td>\s*.*?"">(?<label>.*?)</td>";

          if (FindPattern(pattern, contentinfo))
          {
            while (_match.Success)
            {
              string year = _match.Groups["year"].Value;
              string albumTitle = _match.Groups["album"].Value;
              string label = _match.Groups["label"].Value;

              util.RemoveTags(ref year);
              util.ConvertHTMLToAnsi(year, out year);
              util.RemoveTags(ref albumTitle);
              util.ConvertHTMLToAnsi(albumTitle, out albumTitle);
              util.RemoveTags(ref label);
              util.ConvertHTMLToAnsi(label, out label);

              try
              {
                string[] dAlbumInfo = { year.Trim(), albumTitle.Trim(), label.Trim() };
                _discographyCompilations.Add(dAlbumInfo);
              }
              catch { }

              _match = _match.NextMatch();
            }
          }
        }
        catch (Exception)
        {
        }
      }

      // Singles
      if (singlesPage != "")
      {
        try
        {
          string contentinfo = AllmusicSiteScraper.GetHTTP(singlesPage);
          pattern = @"sorted.*? cell"">(?<year>.*?)</td>\s*?.*?</td>\s*.*?<a.*?"">(?<album>.*?)" +
                    @"</a>.*?</td>\s*.*?</td>\s*.*?"">(?<label>.*?)</td>";

          if (FindPattern(pattern, contentinfo))
          {
            while (_match.Success)
            {
              string year = _match.Groups["year"].Value;
              string albumTitle = _match.Groups["album"].Value;
              string label = _match.Groups["label"].Value;

              util.RemoveTags(ref year);
              util.ConvertHTMLToAnsi(year, out year);
              util.RemoveTags(ref albumTitle);
              util.ConvertHTMLToAnsi(albumTitle, out albumTitle);
              util.RemoveTags(ref label);
              util.ConvertHTMLToAnsi(label, out label);

              try
              {
                string[] dAlbumInfo = { year.Trim(), albumTitle.Trim(), label.Trim() };
                _discographySingles.Add(dAlbumInfo);
              }
              catch { }

              _match = _match.NextMatch();
            }
          }
        }
        catch (Exception)
        {
        }
      }

      // DVD Videos
      if (dvdPage != "")
      {
        try
        {
          string contentinfo = AllmusicSiteScraper.GetHTTP(dvdPage);
          pattern = @"sorted.*? cell"">(?<year>.*?)</td>\s*?.*?</td>\s*.*?<a.*?"">(?<album>.*?)" +
                    @"</a>.*?</td>\s*.*?</td>\s*.*?"">(?<label>.*?)</td>";

          if (FindPattern(pattern, contentinfo))
          {
            while (_match.Success)
            {
              string year = _match.Groups["year"].Value;
              string albumTitle = _match.Groups["album"].Value;
              string label = _match.Groups["label"].Value;

              util.RemoveTags(ref year);
              util.ConvertHTMLToAnsi(year, out year);
              util.RemoveTags(ref albumTitle);
              util.ConvertHTMLToAnsi(albumTitle, out albumTitle);
              util.RemoveTags(ref label);
              util.ConvertHTMLToAnsi(label, out label);

              try
              {
                string[] dAlbumInfo = { year.Trim(), albumTitle.Trim(), label.Trim() };
                _discographyMisc.Add(dAlbumInfo);
              }
              catch { }

              _match = _match.NextMatch();
            }
          }
        }
        catch (Exception)
        {
        }
      }

      // Other
      if (miscPage != "")
      {
        try
        {
          string contentinfo = AllmusicSiteScraper.GetHTTP(miscPage);
          pattern = @"sorted.*? cell"">(?<year>.*?)</td>\s*?.*?</td>\s*.*?<a.*?"">(?<album>.*?)" +
                    @"</a>.*?</td>\s*.*?</td>\s*.*?"">(?<label>.*?)</td>";

          if (FindPattern(pattern, contentinfo))
          {
            while (_match.Success)
            {
              string year = _match.Groups["year"].Value;
              string albumTitle = _match.Groups["album"].Value;
              string label = _match.Groups["label"].Value;

              util.RemoveTags(ref year);
              util.ConvertHTMLToAnsi(year, out year);
              util.RemoveTags(ref albumTitle);
              util.ConvertHTMLToAnsi(albumTitle, out albumTitle);
              util.RemoveTags(ref label);
              util.ConvertHTMLToAnsi(label, out label);

              try
              {
                string[] dAlbumInfo = { year.Trim(), albumTitle.Trim(), label.Trim() };
                _discographyMisc.Add(dAlbumInfo);
              }
              catch { }

              _match = _match.NextMatch();
            }
          }
        }
        catch (Exception)
        {
        }
      }

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