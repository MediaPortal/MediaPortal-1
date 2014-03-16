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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MediaPortal.GUI.Library;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// Embedded logic to scrape artist and album details from alllmusic.com
  /// </summary>
  public class AllmusicSiteScraper
  {

    #region variables

    private const string BaseURL = "http://www.allmusic.com/search/artists/";
    private static readonly Regex BracketRegEx = new Regex(@"\s*[\(\[\{].*?[\]\)\}]\s*", RegexOptions.Compiled);
    private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]|_", RegexOptions.Compiled);

    #endregion

    #region public method

    /// <summary>
    /// Searches for an artist on allmusic.com
    /// </summary>
    /// <param name="strArtist">Artist to search for</param>
    /// <param name="possibleMatches">List of possible matches</param>
    /// <returns>True if matches are found</returns>
    public bool GetArtists(string strArtist, out List<AllMusicArtistMatch> possibleMatches)
    {
      Log.Debug("AllmusicScraper.  Searching-Artist: {0}", strArtist);
      var strEncodedArtist = EncodeString(strArtist);
      var strURL = BaseURL + strEncodedArtist;
      var strCleanArtist = CleanString(strArtist);

      var searchPage = new HtmlWeb().Load(strURL);
      var artistMatches = searchPage.DocumentNode.SelectNodes(@"//ul[@class=""search-results""]/li[@class=""artist""]");
      if (artistMatches == null)
      {
        possibleMatches = new List<AllMusicArtistMatch>();
        return false;
      }

      var allMusicArtistMatches = artistMatches.Select(artist => new AllMusicArtistMatch
        {
          Artist = CleanString(CleanInnerText(artist.SelectSingleNode(@"div[@class=""info""]/div[@class=""name""]"))),
          ArtistUrl = CleanAttribute(artist.SelectSingleNode(@"div[@class=""info""]/div[@class=""name""]/a"), "href"),
          Genre = CleanInnerText(artist.SelectSingleNode(@"div[@class=""info""]/div[@class=""genres""]")),
          ImageUrl = CleanAttribute(artist.SelectSingleNode(@"div[@class=""photo""]/a/img"), "src"),
          YearsActive = CleanInnerText(artist.SelectSingleNode(@"div[@class=""info""]/div[@class=""decades""]"))
        }).ToList();


      possibleMatches = (from a in allMusicArtistMatches
                         where strCleanArtist == a.Artist
                         where !string.IsNullOrEmpty(a.YearsActive)
                         select a).ToList();
      if (possibleMatches.Count == 0)
      {
        // still possible that search returned values but none match our artist
        // try again but this time do not include years active
        possibleMatches = (from a in allMusicArtistMatches
                           where strCleanArtist == a.Artist
                           select a).ToList();
      }

      Log.Debug("AllmusicScraper.  Searched-Artist: {0} Found: {1} matches", strArtist,
                possibleMatches.Count.ToString(CultureInfo.InvariantCulture));

      return possibleMatches.Count != 0;
    }

    /// <summary>
    /// Searches for an album on allmusic.com
    /// </summary>
    /// <param name="strAlbum">Album name to search for</param>
    /// <param name="strArtistUrl">URL of artist page <see cref="GetArtists"/></param>
    /// <param name="strAlbumUrl">URL of album details page</param>
    /// <returns>True if an album was found</returns>
    public bool GetAlbumUrl(string strAlbum, string strArtistUrl, out string strAlbumUrl)
    {
      var artistPage = new HtmlWeb().Load(strArtistUrl);
      var albumPageURL = "http://www.allmusic.com/" + CleanAttribute(artistPage.DocumentNode.SelectSingleNode(@"//ul[@class=""tabs overview""]/li[@class=""tab discography""]/a"), "href");
      if (string.IsNullOrEmpty(albumPageURL))
      {
        Log.Debug("No discography page found");
        strAlbumUrl = string.Empty;
        return false;
      }

      // standard albums
      if (!GetAlbumURL(strArtistUrl + "/discography", strAlbum, out strAlbumUrl))
      {
        // compilations
        Log.Debug("AllmusicScraper.  Searching-Album: {0} - not found in main albums.  Checking compilations", strAlbum);
        if (!GetAlbumURL(strArtistUrl + "/discography/compilations", strAlbum, out strAlbumUrl))
        {
          // Singles / EPs
          Log.Debug("AllmusicScraper.  Searching-Album: {0} - not found in compilations.  Checking singles & EPs", strAlbum);
          if (!GetAlbumURL(strArtistUrl + "/discography/singles", strAlbum, out strAlbumUrl))
          {
            Log.Debug("AllmusicScraper.  Searching-Album: {0} - not found", strAlbum);
            return false;
          }
        }
      }

      return true;
    }

    #endregion

    #region private methods

    /// <summary>
    /// Encapsulates logic to search for the album URL.   Is called for different types
    /// (eg. main albums, compilations, EPs) <see cref="GetAlbumUrl"/>
    /// </summary>
    /// <param name="strURL">The URL of an artist details page to search</param>
    /// <param name="strAlbum">Album name to search for</param>
    /// <param name="strAlbumURL">URL of album details page</param>
    /// <returns></returns>
    private static bool GetAlbumURL(string strURL, string strAlbum, out string strAlbumURL)
    {
      //TODO should return list of album matches
      strAlbumURL = string.Empty;
      var strCleanAlbum = CleanString(strAlbum);

      var discographyPage = new HtmlWeb().Load(strURL);
      var albums = discographyPage.DocumentNode.SelectNodes(@"//section[@class=""discography""]/table/tbody/tr");
      if (albums == null)
      {
        return false;
      }
      var albumlist = albums.Select(album => new AllMusicAlbumMatch
        {
          Album = CleanInnerText(album.SelectSingleNode(@"td[@class=""title""]/a")),
          AlbumURL = CleanAttribute(album.SelectSingleNode(@"td[@class=""title""]/a"), "href"),
          Year = CleanInnerText(album.SelectSingleNode(@"td[@class=""year""]")),
          Label = CleanInnerText(album.SelectSingleNode(@"td[@class=""label""]"))
        }).ToList();

      var matchedAlbum = (from a in albumlist
                          where CleanString(a.Album) == strCleanAlbum
                          select a).ToList();

      if (matchedAlbum.Count() == 1)
      {
        strAlbumURL = matchedAlbum[0].AlbumURL;
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

    #region helper methods

    /// <summary>
    /// Take a HTML node, validate and forma the inner text
    /// </summary>
    /// <param name="node">HTML Node to validate and format</param>
    /// <returns>Formatted innertext</returns>
    protected internal static string CleanInnerText(HtmlNode node)
    {
      if (node == null)
      {
        return string.Empty;
      }

      var retval = node.InnerText.Trim();
      retval = Regex.Replace(retval, @"  +", @" "); // replace multiple spaces with a single one
      retval = Regex.Replace(retval, @"(?m)(^\s+$)+", ""); // sort out indentation

      return retval;
    }

    /// <summary>
    /// Take a HTML node and validate and attribute
    /// </summary>
    /// <param name="node">HTML node to validate</param>
    /// <param name="attributeName">The attribute name to check</param>
    /// <returns>The value of attribute else if it exists else an empty string</returns>
    protected internal static string CleanAttribute(HtmlNode node, string attributeName)
    {
      return node == null ? string.Empty : node.GetAttributeValue(attributeName, string.Empty);
    }

    #endregion

  }
}