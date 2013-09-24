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

      if (vdbParserStr == null || vdbParserStr.Length != 7)
      {
        return;
      }

      _imageList.Clear();

      if (!string.IsNullOrEmpty(imdbMovieID) && imdbMovieID.StartsWith("tt"))
      {
        //http://api.themoviedb.org/3/movie/imdbTT/images?api_key=APIKEY
        string defaultPosterPageLinkUrl = vdbParserStr[0] +
                                          imdbMovieID +
                                          vdbParserStr[1];
        string strBodyTMDB = GetPage(defaultPosterPageLinkUrl, "utf-8");
        //"posters":\[.*?\]
        string posterBlock = Regex.Match(strBodyTMDB, vdbParserStr[3], RegexOptions.IgnoreCase | RegexOptions.Singleline).Value;
        // Get all cover links and put it in the "cover" group
        //"file_path":"/(?<cover>.*?jpg)"
        MatchCollection covers = Regex.Matches(posterBlock, vdbParserStr[4]);
        
        foreach (Match cover in covers)
        {
          string coverUrl = string.Empty;
          coverUrl = vdbParserStr[6] + cover.Groups["cover"].Value;
          _imageList.Add(coverUrl);
        }
        return;
      }
      if (!string.IsNullOrEmpty(movieTitle))
      {
        // http://api.themoviedb.org/3/search/movie?api_key=APIKEY&query=title
        string defaultPosterPageLinkUrl = vdbParserStr[2] + movieTitle;
        string strBodyTmdb = GetPage(defaultPosterPageLinkUrl, "utf-8");

        // Get all cover links and put it in the "cover" group
        // "backdrop_path":"/(?<BackDrop>.*?jpg)"
        MatchCollection covers = Regex.Matches(strBodyTmdb, vdbParserStr[5]);

        foreach (Match cover in covers)
        {
          string coverUrl = string.Empty;
          coverUrl = vdbParserStr[6] + cover.Groups["cover"].Value;
          _imageList.Add(coverUrl);
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
        // http://api.themoviedb.org/3/search/person?api_key=APIKEYa&query=actorName
        string defaultPosterPageLinkUrl = vdbParserStr[0] + actorName;
        string strXml = GetPage(defaultPosterPageLinkUrl, "utf-8");

        // "profile_path":"/(?<cover>.*?jpg)"
        MatchCollection actorImages = Regex.Matches(strXml, vdbParserStr[1]);
        
        if (actorImages.Count == 0)
        {
          return; 
        }

        foreach (Match actorImage in actorImages)
        {
          string actor = string.Empty;
          actor = vdbParserStr[2] + actorImage.Groups["cover"].Value;
          actorThumbs.Add(actor);
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
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strUrl);
        req.Method = WebRequestMethods.Http.Get;
        req.Accept = "application/json";
        req.Timeout = 20000;

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