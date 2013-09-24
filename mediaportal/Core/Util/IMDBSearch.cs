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
using System.IO;
using System.Net;
using MediaPortal.GUI.Library;
using System.Web;
using MediaPortal.Profile;

namespace MediaPortal.Util
{
  /// <summary>
  /// Search IMDB.com for movie-posters and actors
  /// </summary>
  public class IMDBSearch
  {
    private ArrayList _imageList = new ArrayList();

    public int Count
    {
      get { return _imageList.Count; }
    }

    public string this[int index]
    {
      get
      {
        if (index < 0 || index >= _imageList.Count) return string.Empty;
        return (string)_imageList[index];
      }
    }

    /// <summary>
    /// Cover search on IMDB movie page.
    /// Parameter imdbMovieID must be in IMDB format (ie. tt0123456 including leading zeros).
    /// Parameter defaultOnly = TRUE will download only IMDB movie default cover.
    /// </summary>
    /// <param name="imdbID"></param>
    /// <param name="defaultOnly"></param>
    public void SearchCovers(string imdbID, bool defaultOnly)

    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }

      if (imdbID == null) return;
      if (imdbID == string.Empty | !imdbID.StartsWith("tt")) return;

      string[] vdbParserStr = VdbParserStringIMDBPoster();

      if (vdbParserStr == null || vdbParserStr.Length != 8)
      {
        return;
      }
      
      _imageList.Clear();
      string defaultPic = "";

      // First lets take default IMDB cover because maybe it is not in the IMDB Product thumbs group
      // Get Main Movie page and find default poster link
      string defaultPosterPageLinkURL = vdbParserStr[0] + imdbID;
      string strBodyPicDefault = GetPage(defaultPosterPageLinkURL, "utf-8");
      
      string regexBlockPattern = vdbParserStr[1];
      string posterBlock = Regex.Match(strBodyPicDefault, regexBlockPattern, RegexOptions.Singleline).Value;
      Match jpgDefault = Regex.Match(posterBlock, vdbParserStr[2], RegexOptions.Singleline);
      
      if (jpgDefault.Success)
      {
        string posterUrl = HttpUtility.HtmlDecode(jpgDefault.Groups["image"].Value);
        if (!string.IsNullOrEmpty(posterUrl))
        {
          _imageList.Add(posterUrl + vdbParserStr[3]);
          // Remember default PIC, maybe it is in the Product Group so we can escape duplicate
          defaultPic = posterUrl + vdbParserStr[3];
        }
      }

      if (defaultOnly)
      {
        return;
      }

      // Then get all we can from IMDB Product thumbs group for movie
      string posterPageLinkURL = vdbParserStr[4] + imdbID + vdbParserStr[5];
      string strBodyThumbs = GetPage(posterPageLinkURL, "utf-8");
      
      // Get all thumbs links and put it in the PIC group
      MatchCollection thumbs = Regex.Matches(strBodyThumbs, vdbParserStr[6]);
      foreach (Match thumb in thumbs)
      {
        // Get picture
        string posterUrl = HttpUtility.HtmlDecode(thumb.Groups["PIC"].Value) + vdbParserStr[3];
        
        // No default Picture again if it's here
        if (!string.IsNullOrEmpty(posterUrl) && posterUrl != defaultPic)
        {
          _imageList.Add(posterUrl);
        }
      }
    }
    
    private string[] VdbParserStringIMDBPoster()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("IMDBPoster");
      return vdbParserStr;
    }
    
    // Get HTML Page
    private string GetPage(string strURL, string strEncode)
    {
      string strBody = "";

      Stream receiveStream = null;
      StreamReader sr = null;
      WebResponse result = null;
      try
      {
        // Make the Webrequest
        //Log.Info("IMDB: get page:{0}", strURL);
        WebRequest req = WebRequest.Create(strURL);
        req.Headers.Add("Accept-Language", "en-US");
        req.Timeout = 20000;
        result = req.GetResponse();
        receiveStream = result.GetResponseStream();

        // Encoding: depends on selected page
        Encoding encode = Encoding.GetEncoding(strEncode);
        sr = new StreamReader(receiveStream, encode);
        strBody = sr.ReadToEnd();
      }
      catch (Exception)
      {
        Log.Info("IMDBSearch: {0} unavailable.", strURL);
      }

      finally
      {
        if (sr != null)
        {
          try
          {
            sr.Close();
          }
          catch (Exception) {}
        }
        if (receiveStream != null)
        {
          try
          {
            receiveStream.Close();
          }
          catch (Exception) {}
        }
        if (result != null)
        {
          try
          {
            result.Close();
          }
          catch (Exception) {}
        }
      }
      return strBody;
    }
  }
}