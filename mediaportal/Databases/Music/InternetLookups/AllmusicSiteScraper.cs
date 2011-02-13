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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MediaPortal.Util;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// Summary description for ArtistInfoScraper.
  /// </summary>
  public class AllmusicSiteScraper
  {
    #region Enum

    public enum SearchBy : int
    {
      Artists = 1,
      Albums
    } ;

    #endregion

    #region Variables 

    internal const string MAINURL = "http://www.allmusic.com";
    internal const string URLPROGRAM = "/search";
    internal const string ARTISTSEARCH = "search_term={0}&x=34&y=8&search_type=artist";
    internal const string ALBUMSEARCH = "search_term={0}&x=34&y=8&search_type=album";
    protected List<string> _codes = new List<string>(); // if multiple..
    protected List<string> _values = new List<string>(); // if multiple..
    protected List<MusicAlbumInfo> _albumList = new List<MusicAlbumInfo>();
    protected bool _multiple = false;
    protected string _htmlCode = null;
    protected string _queryString = "";
    protected SearchBy _searchby = SearchBy.Artists;

    #endregion

    #region ctor

    public AllmusicSiteScraper() {}

    #endregion

    #region Public Methods

    /// <summary>
    /// Do we have Multiple hits on the searchg
    /// </summary>
    /// <returns></returns>
    public bool IsMultiple()
    {
      return _multiple;
    }

    /// <summary>
    /// Retrieve the Items found
    /// </summary>
    /// <returns></returns>
    public List<string> GetItemsFound()
    {
      return _values;
    }

    /// <summary>
    /// Retrieve the Albums Found
    /// </summary>
    /// <returns></returns>
    public List<MusicAlbumInfo> GetAlbumsFound()
    {
      return _albumList;
    }

    /// <summary>
    /// Get the HTML Content
    /// </summary>
    /// <returns></returns>
    public string GetHtmlContent()
    {
      return _htmlCode;
    }

    /// <summary>
    /// Get page as per selected index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool FindInfoByIndex(int index)
    {
      if (index < 0)
      {
        return false;
      }

      string url = "";
      if (_searchby == SearchBy.Artists)
      {
        url = _codes[index];
      }
      else
      {
        url = _albumList[index].AlbumURL;
      }

      string strHTML = GetHTTP(url);
      if (strHTML.Length == 0)
      {
        return false;
      }

      _htmlCode = strHTML; // save the html content...
      return true;
    }

    public bool FindAlbumInfo(string strAlbum, string artistName, int releaseYear)
    {
      _searchby = SearchBy.Albums;
      _albumList.Clear();
      if (FindInfo(SearchBy.Albums, strAlbum))
      {
        // Sort the Album
        artistName = SwitchArtist(artistName);
        _albumList.Sort(new AlbumSort(strAlbum, artistName, releaseYear));
        return true;
      }
      return false;
    }

    /// <summary>
    /// Search on Allmusic for the requested string
    /// </summary>
    /// <param name="searchBy"></param>
    /// <param name="searchStr"></param>
    /// <returns></returns>
    public bool FindInfo(SearchBy searchBy, string searchStr)
    {
      _searchby = searchBy;
      HTMLUtil util = new HTMLUtil();
      string strPostData = "";
      if (SearchBy.Albums == searchBy)
      {
        strPostData = string.Format(ALBUMSEARCH, HttpUtility.UrlEncode(searchStr));
      }
      else
      {
        searchStr = SwitchArtist(searchStr);
        strPostData = string.Format(ARTISTSEARCH, HttpUtility.UrlEncode(searchStr));
      }

      string strHTML = PostHTTP(MAINURL + URLPROGRAM, strPostData);
      if (strHTML.Length == 0)
      {
        return false;
      }

      _htmlCode = strHTML; // save the html content...

      Regex multiples = new Regex(
        @"\sSearch\sResults\sfor:",
        RegexOptions.IgnoreCase
        | RegexOptions.Multiline
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.Compiled
        );

      if (multiples.IsMatch(strHTML))
      {
        string pattern = "";
        if (searchBy.ToString().Equals("Artists"))
        {
          pattern = @"<tr.*?>\s*?.*?<td\s*?class=""relevance\stext-center"">\s*?.*\s*?.*</td>" +
                    @"\s*?.*<td.*\s*?.*</td>\s*?.*<td>.*<a.*href=""(?<code>.*?)"">(?<name>.*)</a>.*</td>" +
                    @"\s*?.*<td>(?<detail>.*)</td>\s*?.*<td>(?<detail2>.*)</td>";
        }
        else if (searchBy.ToString().Equals("Albums"))
        {
          pattern = @"<tr.*?>\s*?.*?<td\s*?class=""relevance\stext-center"">\s*?.*\s*?.*</td>" +
                    @"\s*?.*<td.*\s*?.*</td>\s*?.*<td>.*<a.*href=""(?<code>.*?)"">(?<name>.*)</a>.*</td>" +
                    @"\s*?.*<td>(?<detail>.*)</td>\s*?.*<td>.*</td>\s*?.*<td>(?<detail2>.*)</td>";
        }


        Match m;
        Regex itemsFoundFromSite = new Regex(
          pattern,
          RegexOptions.IgnoreCase
          | RegexOptions.Multiline
          | RegexOptions.IgnorePatternWhitespace
          | RegexOptions.Compiled
          );


        for (m = itemsFoundFromSite.Match(strHTML); m.Success; m = m.NextMatch())
        {
          string code = m.Groups["code"].ToString();
          string name = m.Groups["name"].ToString();
          string detail = m.Groups["detail"].ToString();
          string detail2 = m.Groups["detail2"].ToString();

          util.RemoveTags(ref name);
          util.ConvertHTMLToAnsi(name, out name);

          util.RemoveTags(ref detail);
          util.ConvertHTMLToAnsi(detail, out detail);

          util.RemoveTags(ref detail2);
          util.ConvertHTMLToAnsi(detail2, out detail2);

          if (SearchBy.Artists == searchBy)
          {
            detail += " - " + detail2;
            if (detail.Length > 0)
            {
              _codes.Add(code);
              _values.Add(name + " - " + detail);
            }
            else
            {
              _codes.Add(code);
              _values.Add(name);
            }
          }
          else
          {
            MusicAlbumInfo albumInfo = new MusicAlbumInfo();
            albumInfo.AlbumURL = code;
            albumInfo.Artist = detail;
            albumInfo.Title = name;
            albumInfo.DateOfRelease = detail2;
            _albumList.Add(albumInfo);
          }
        }
        _multiple = true;
      }
      else // found the right one
      {}
      return true;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Post method for posting te search form
    /// </summary>
    /// <param name="strURL"></param>
    /// <param name="strData"></param>
    /// <returns></returns>
    internal static string PostHTTP(string strURL, string strData)
    {
      try
      {
        string strBody;

        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strURL);
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          req.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception) {}

        req.Method = "POST";
        req.ProtocolVersion = HttpVersion.Version11;
        req.UserAgent =
          "Mozilla/5.0 (Windows; U; Windows NT 5.1; de; rv:1.9.1.5) Gecko/20091102 Firefox/3.5.5";
        req.ContentType = "application/x-www-form-urlencoded";
        req.ContentLength = strData.Length;

        // Post the Data
        StreamWriter sw = new StreamWriter(req.GetRequestStream());
        sw.Write(strData);
        sw.Close();

        HttpWebResponse result = (HttpWebResponse)req.GetResponse();

        try
        {
          Stream ReceiveStream = result.GetResponseStream();

          using (StreamReader sr = new StreamReader(ReceiveStream, Encoding.Default))
          {
            strBody = sr.ReadToEnd();
          }

          return strBody;
        }
        finally
        {
          if (result != null)
          {
            result.Close();
          }
        }
      }
      catch (Exception) {}
      return "";
    }

    /// <summary>
    /// Retrieve HTTP content as per given URL
    /// </summary>
    /// <param name="strURL"></param>
    /// <returns></returns>
    internal static string GetHTTP(string strURL)
    {
      string retval = null;

      // Initialize the WebRequest.
      WebRequest myRequest = WebRequest.Create(strURL);

      try
      {
        // Use the current user in case an NTLM Proxy or similar is used.
        // wr.Proxy = WebProxy.GetDefaultProxy();
        myRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
      }
      catch (Exception) {}

      // Return the response. 
      WebResponse myResponse = myRequest.GetResponse();

      Stream ReceiveStream = myResponse.GetResponseStream();

      using (StreamReader sr = new StreamReader(ReceiveStream, Encoding.Default))
      {
        retval = sr.ReadToEnd();
      }

      // Close the response to free resources.
      myResponse.Close();

      return retval;
    }

    /// <summary>
    /// AllMusic has problems finding the right artist, if it is tagged e.g. Collins, Phil
    /// so we return Phil Collins for the search
    /// </summary>
    /// <param name="artist"></param>
    /// <returns></returns>
    internal static string SwitchArtist(string artist)
    {
      int iPos = artist.IndexOf(',');
      if (iPos > 0)
      {
        artist = String.Format("{0} {1}", artist.Substring(iPos + 2), artist.Substring(0, iPos));
      }
      return artist;
    }

    #endregion
  }
}