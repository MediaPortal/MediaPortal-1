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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// Summary description for ArtistInfoScraper.
  /// </summary>
  public class AllmusicSiteScraper
  {

    #region variables

    private const string BaseURL = "http://www.allmusic.com/search/artists/";
    private const string AlbumRegExpPattern = @"<td class=""title primary_link"".*?<a href=""(?<albumURL>.*?)"" class=""title.*?"" data-tooltip="".*?"">(?<albumName>.*?)</a>";
    private static readonly Regex AlbumURLRegEx = new Regex(AlbumRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string ArtistRegExpPattern = @"<tr class=""search-result artist"">.*?<div class=""name"">\s*<a href=""(?<artistURL>.*?)"".*?>(?<artist>.*?)</a>\s*</div>\s*<div class=""info"">\s*(?<genres>.*?)\s*<br/>\s*(?<years>.*?)\s*</div>";
    private static readonly Regex ArtistURLRegEx = new Regex(ArtistRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex BracketRegEx = new Regex(@"\s*[\(\[\{].*?[\]\)\}]\s*", RegexOptions.Compiled);
    private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]|_", RegexOptions.Compiled);

    #endregion

    #region public method

    public bool GetArtists(string strArtist, out List<AllMusicArtistMatch> artists)
    {
      Log.Debug("AllmusicScraper.  Searching-Artist: {0}", strArtist);
      artists = new List<AllMusicArtistMatch>();
      var strEncodedArtist = EncodeString(strArtist);
      var strURL = BaseURL + strEncodedArtist;

      string artistSearchHtml;
      if (!GetHTML(strURL, out artistSearchHtml))
      {
        return false;
      }

      var matches = ArtistURLRegEx.Matches(artistSearchHtml);
      var strCleanArtist = CleanString(strArtist);

      //TODO needs image url in regexp
      artists.AddRange(from Match m in matches
                       let strCleanMatch = CleanString(m.Groups["artist"].ToString())
                       let strYearsActive = m.Groups["years"].ToString().Trim()
                       let strArtistUrl = m.Groups["artistURL"].ToString()
                       let strGenre = m.Groups["genres"].ToString()
                       where strCleanArtist == strCleanMatch
                       where !string.IsNullOrEmpty(strYearsActive)
                       select
                         new AllMusicArtistMatch
                           {
                             Artist = strArtist,
                             YearsActive = strYearsActive,
                             ArtistUrl = strArtistUrl,
                             Genre = strGenre
                           });

      if (artists.Count == 0)
      {
        // still possible that search returned values but none match our artist
        // try again but this time do not include years active
        artists.AddRange(from Match m in matches
                         let strCleanMatch = CleanString(m.Groups["artist"].ToString())
                         let strYearsActive = m.Groups["years"].ToString().Trim()
                         let strArtistUrl = m.Groups["artistURL"].ToString()
                         let strGenre = m.Groups["genres"].ToString()
                         where strCleanArtist == strCleanMatch
                         select
                           new AllMusicArtistMatch
                             {
                               Artist = strArtist,
                               YearsActive = strYearsActive,
                               ArtistUrl = strArtistUrl,
                               Genre = strGenre
                             });
      }

      Log.Debug("AllmusicScraper.  Searched-Artist: {0} Found: {1} matches", strArtist,
                artists.Count.ToString(CultureInfo.InvariantCulture));

      return artists.Count != 0;
    }

    public bool GetArtistHtml(AllMusicArtistMatch allMusicArtistMatch, out string strHTML)
    {
      return GetHTML(allMusicArtistMatch.ArtistUrl, out strHTML); 
    }

    public bool GetAlbumHtml(string strAlbum, string strArtistUrl, out string strHtml)
    {
      Log.Debug("AllmusicScraper.  Searching-Album: {0}", strAlbum);
      strHtml = string.Empty;
      string strAlbumURL;
      var strURL = strArtistUrl + "/overview/main#discography";
      if(!GetAlbumURL(strURL, strAlbum, out strAlbumURL))
      {
        Log.Debug("AllmusicScraper.  Searching-Album: {0} - not found in main albums.  Checking compilations", strAlbum);
        strURL = strArtistUrl + "/overview/compilations#discography";
        if (!GetAlbumURL(strURL, strAlbum, out strAlbumURL))
        {
          Log.Debug("AllmusicScraper.  Searching-Album: {0} - not found in compilaitions.  Checking singles & EPs", strAlbum);
          strURL = strArtistUrl + "/overview/singles#discography";
          if (!GetAlbumURL(strURL, strAlbum, out strAlbumURL))
          {
            Log.Debug("AllmusicScraper.  Searching-Album: {0} - not found", strAlbum);
            return false;
          }
        }
      }

      return GetHTML(strAlbumURL, out strHtml);
    }

    #endregion

    #region private methods

    private static bool GetAlbumURL(string strArtistURL, string strAlbum, out string strAlbumURL)
    {
      strAlbumURL = string.Empty;
      string discHTML;
      if (!GetHTML(strArtistURL, out discHTML))
      {
        return false;
      }

      var strCleanAlbum = CleanString(strAlbum);

      for (var m = AlbumURLRegEx.Match(discHTML); m.Success; m = m.NextMatch())
      {
        var strFoundValue = CleanString(m.Groups["albumName"].ToString());

        if (strFoundValue != strCleanAlbum)
        {
          continue;
        }
        
        strAlbumURL = m.Groups["albumURL"].ToString();
        break;
      }

      // return true if we have picked up a URL
      return !String.IsNullOrEmpty(strAlbumURL);
    }

    /// <summary>
    /// Attempt to make string searching more helpful.   Removes all accents and puts in lower case
    /// Then escapes characters for use in URI
    /// </summary>
    /// <param name="strUnclean">String to be encoded</param>
    /// <returns>An encoded, cleansed string</returns>
    private static string EncodeString(string strUnclean)
    {
      var stFormD = strUnclean.Normalize(NormalizationForm.FormD);
      var sb = new StringBuilder();

      foreach (var t in from t in stFormD let uc = CharUnicodeInfo.GetUnicodeCategory(t) where uc != UnicodeCategory.NonSpacingMark select t)
      {
        sb.Append(t);
      }
      var strClean = Uri.EscapeDataString(sb.ToString().Normalize(NormalizationForm.FormC)).ToLower(CultureInfo.CurrentCulture);

      return strClean;
    }

    /// <summary>
    /// Improve changes of matching artists and albums by replacing & and + with "and" on both side of comparison
    /// Also remove "The" and normalise output to remove accents and finally html decode
    /// </summary>
    /// <param name="strUncleanString">artist we are searching for</param>
    /// <returns>Cleaned artist string</returns>
    private static string CleanString(string strUncleanString)
    {
      var strDecodedString = System.Web.HttpUtility.HtmlDecode(strUncleanString);

      var stFormD = strDecodedString.Normalize(NormalizationForm.FormD);
      var sb = new StringBuilder();

      foreach (var t in from t in stFormD let uc = CharUnicodeInfo.GetUnicodeCategory(t) where uc != UnicodeCategory.NonSpacingMark select t)
      {
        sb.Append(t);
      }

      var strCleanString = sb.ToString().Normalize(NormalizationForm.FormC).ToLower(CultureInfo.CurrentCulture);
      strCleanString = strCleanString.Replace("&", "and");
      strCleanString = strCleanString.Replace("+", "and");
      strCleanString = Regex.Replace(strCleanString, "^the ", "", RegexOptions.IgnoreCase);
      // attempt to remove stack endings (eg. disc2, (CD2) etc)
      Util.Utils.RemoveStackEndings(ref strCleanString);
      // try and remove any thing else in brackets at end of string eg. (remastered), (special edition), (vinyl) etc
      strCleanString = BracketRegEx.Replace(strCleanString, "$1");
      // try and repalce all punctuation to try and get a match; sometimes you have three dots in one format but two in another
      strCleanString = PunctuationRegex.Replace(strCleanString, "");
      return strCleanString.Trim();
    }

    #endregion

    #region HTTP

    private static bool GetHTML(string strURL, out string strHTML)
    {
      strHTML = string.Empty;
      try 
      {
        var x = (HttpWebRequest)WebRequest.Create(strURL);

        x.ProtocolVersion = HttpVersion.Version10;
        x.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0) Gecko/20100101 Firefox/6.0";
        x.ContentType = "text/html";
        x.Timeout = 30000;
        x.AllowAutoRedirect = false;

        using (var y = (HttpWebResponse)x.GetResponse())
        {
          using (var z = y.GetResponseStream())
          {
            if (z == null)
            {
              x.Abort();
              y.Close();
              return false;
            }
            using (var sr = new StreamReader(z, Encoding.UTF8))
            {
              strHTML = sr.ReadToEnd();
            }

            z.Close();
            x.Abort();
            y.Close();
          }
        }
      }
      catch(Exception ex)
      {
        Log.Error("AMG Scraper: Error retrieving html for: {0}", strURL);
        Log.Error(ex);
        return false;
      }

      return true;
    }

    #endregion

  }
}