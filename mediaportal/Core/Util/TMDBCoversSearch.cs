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
using System.Web;
using MediaPortal.GUI.Library;

namespace MediaPortal.Util
{
  /// <summary>
  /// Search IMDB.com for movie-posters
  /// </summary>
  public class TMDBCoverSearch
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
    /// Cover search using TMDB API by IMDBmovieID for accuracy.
    /// Parameter imdbMovieID must be in IMDB format (ie. tt0123456 including leading zeros).
    /// Or if no IMDBid movie title can be used with lesser accuracy .
    /// </summary>
    /// <param name="imdbMovieID"></param>
    /// <param name="movieTitle"></param>
    public void SearchCovers(string movieTitle, string imdbMovieID)

    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }

      string[] vdbParserStr = VdbParserStringPoster();

      if (vdbParserStr == null || vdbParserStr.Length != 5)
      {
        return;
      }

      _imageList.Clear();

      if (!string.IsNullOrEmpty(imdbMovieID) && imdbMovieID.StartsWith("tt"))
      {
        // Use IDMB ID - no wild goose chase
        //string defaultPosterPageLinkUrl =
        //  "http://api.themoviedb.org/2.1/Movie.getImages/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a/" + imdbMovieID;
        string defaultPosterPageLinkUrl = vdbParserStr[0] + imdbMovieID;
        string strBodyTMDB = GetPage(defaultPosterPageLinkUrl, "utf-8");
        //string posterBlock = Regex.Match(strBodyTMDB, "<poster.*</poster>", RegexOptions.Singleline).Value;
        string posterBlock = Regex.Match(strBodyTMDB, vdbParserStr[1], RegexOptions.Singleline).Value;
        // Get all cover links and put it in the "cover" group
        //MatchCollection covers = Regex.Matches(posterBlock, @"<image\surl=""(?<cover>http://cf2.imgobject.com/t/p/w500/.*?)""");
        MatchCollection covers = Regex.Matches(posterBlock, vdbParserStr[2]);
        
        if (covers.Count == 0)
        {
          return;
        }

        foreach (Match cover in covers)
        {
          // Get cover - using mid quality cover
          //if (HttpUtility.HtmlDecode(cover.Groups["cover"].Value).ToLower().Contains("mid.jpg"))
          //{
          _imageList.Add(HttpUtility.HtmlDecode(cover.Groups["cover"].Value));
          //}
        }
        return;
      }
      if (!string.IsNullOrEmpty(movieTitle))
      {
        //string defaultPosterPageLinkUrl =
        //  "http://api.themoviedb.org/2.1/Movie.search/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a/" + movieTitle;
        string defaultPosterPageLinkUrl = vdbParserStr[3] + movieTitle;
        string strBodyTMDB = GetPage(defaultPosterPageLinkUrl, "utf-8");

        // Get all cover links and put it in the "cover" group
        //MatchCollection covers = Regex.Matches(strBodyTMDB,
        //                                       @"<image\stype=""poster""\surl=""(?<cover>http://cf2.imgobject.com/t/p/w500/.*?jpg)""");
        MatchCollection covers = Regex.Matches(strBodyTMDB, vdbParserStr[4]);

        foreach (Match cover in covers)
        {
          // Get cover - using mid quality cover
          //if (HttpUtility.HtmlDecode(cover.Groups["cover"].Value).ToLower().Contains("mid.jpg"))
          //{
          _imageList.Add(HttpUtility.HtmlDecode(cover.Groups["cover"].Value));
          //}
        }
      }
    }

    public void SearchActorImage(string actorName, ref ArrayList actorThumbs)
    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }

      string[] vdbParserStr = VdbParserStringActorImage();

      if (vdbParserStr == null || vdbParserStr.Length != 2)
      {
        return;
      }

      actorThumbs.Clear();

      if (!string.IsNullOrEmpty(actorName))
      {
        // Use IDMB ID - no wild goose chase
        //string defaultPosterPageLinkUrl =
        //  "http://api.themoviedb.org/2.1/Person.search/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a/" + actorName;
        string defaultPosterPageLinkUrl = vdbParserStr[0] + actorName;
        string strXml = GetPage(defaultPosterPageLinkUrl, "utf-8");

        // Get all cover links and put it in the "cover" group
        //MatchCollection actorImages = Regex.Matches(strXml, @"<image\stype=""profile""\surl=""(?<cover>.*?)""");
        MatchCollection actorImages = Regex.Matches(strXml, vdbParserStr[1]);
        
        if (actorImages.Count == 0)
        {
          return; 
        }

        foreach (Match actorImage in actorImages)
        {
          // Get cover - using mid quality cover
          if (HttpUtility.HtmlDecode(actorImage.Groups["cover"].Value).ToLower().Contains("original"))
          {
            actorThumbs.Add(HttpUtility.HtmlDecode(actorImage.Groups["cover"].Value));
          }
        }
        return;
      }
    }

    private string[] VdbParserStringPoster()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("TMDBPosters");
      return vdbParserStr;
    }

    private string[] VdbParserStringActorImage()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("TMDBActorImages");
      return vdbParserStr;
    }

    // Get HTML Page
    private string GetPage(string strUrl, string strEncode)
    {
      string strBody = "";

      Stream receiveStream = null;
      StreamReader sr = null;
      WebResponse result = null;
      try
      {
        // Make the Webrequest
        //Log.Info("IMDB: get page:{0}", strURL);
        WebRequest req = WebRequest.Create(strUrl);
        req.Timeout = 10000;
        result = req.GetResponse();
        receiveStream = result.GetResponseStream();

        // Encoding: depends on selected page
        Encoding encode = System.Text.Encoding.GetEncoding(strEncode);
        sr = new StreamReader(receiveStream, encode);
        strBody = sr.ReadToEnd();
      }
      catch (Exception)
      {
        Log.Info("TMDBCoverSearch: {0} unavailable.", strUrl);
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