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
using System.Net;

namespace MediaPortal.Util
{
  /// <summary>
  /// Search IMPAwards.com for movie covers
  /// </summary>
  public class IMPAwardsSearch
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
    /// Cover search in IMPAwards.com through google domain search with
    /// movieName parameter as the search term.
    /// IMPAward page result is compared by IMDBid number for 100% accuracy.
    /// Parameter imdbMovieID must be in IMDB format (ie. tt0123456 including leading zeros).
    /// </summary>
    /// <param name="movieName"></param>
    /// <param name="imdbMovieID"></param>
    public void SearchCovers(string movieName, string imdbMovieID)

    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }
      if (movieName == null) return;
      if (movieName == string.Empty) return;

      string[] vdbParserStr = VdbParserString();

      if (vdbParserStr == null || vdbParserStr.Length != 7)
      {
        return;
      }

      _imageList.Clear();
      movieName = movieName.Replace(" ", "+");
      string resultGoogle = string.Empty;
      string resultImpAw = string.Empty;

      //string url = "http://www.google.com/search?as_q=" + movieName + "+poster&as_sitesearch=www.impawards.com";
      string url = vdbParserStr[0] + movieName + vdbParserStr[1];

      IMPAwardsSearch x = new IMPAwardsSearch();

      WebClient wc = new WebClient();
      try
      {
        wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
        byte[] buffer = wc.DownloadData(url);
        resultGoogle = Encoding.UTF8.GetString(buffer);
      }
      catch (Exception)
      {
        return;
      }
      finally
      {
        wc.Dispose();
      }
      //Match mGoogle = Regex.Match(resultGoogle, @"www.impawards.com[^""& <].*?(?<year>\d{4}/).*?html");
      Match mGoogle = Regex.Match(resultGoogle, vdbParserStr[2]);

      while (mGoogle.Success)
      {
        // We need all links on Google page not only first because it can be wrong movie
        // All links is checked against ttnumber so no wrong cover anymore
        Match mImpAw = mGoogle;
        // Check if /year/ is in link, if no that is no cover
        string year = mImpAw.Groups["year"].Value.Replace("/", "");
        if (year != "")
        {
          string url2 = mImpAw.Value;
          url2 = "http://" + url2;
          try
          {
            byte[] buffer = wc.DownloadData(url2);
            resultImpAw = Encoding.UTF8.GetString(buffer);
          }
          catch (Exception)
          {
            return;
          }
          finally
          {
            wc.Dispose();
          }
          // Check if IMDB number on poster page is equal to  IMDB ttnumber, if not-> next link
          //Match ttcheck = Regex.Match(resultImpAw, @"tt\d{7}");
          Match ttcheck = Regex.Match(resultImpAw, vdbParserStr[3]);
          if (ttcheck.Value != imdbMovieID)
          {
            break;
          }

          //Match urlImpAw = Regex.Match(url2, @".*?\d{4}./*?");
          Match urlImpAw = Regex.Match(url2, vdbParserStr[4]);
          // get main poster displayed on html-page
          //mImpAw = Regex.Match(resultImpAw, @"posters/.*?.jpg");
          mImpAw = Regex.Match(resultImpAw, vdbParserStr[5]);
          if (mImpAw.Success)
          {
            // Check duplicate entries because Google page links can point to
            // same cover more than once so we don't need them
            int check = 0;
            foreach (string text in _imageList)
            {
              if (text == urlImpAw + mImpAw.Value)
              {
                check = 1;
                break;
              }
            }
            // No duplicates (check=0)
            if (check == 0)
            {
              _imageList.Add(urlImpAw + mImpAw.Value);
            }
            // get other posters displayed on this html-page as thumbs
            //MatchCollection mcImpAw = Regex.Matches(resultImpAw, @"thumbs/imp_(?<poster>.*?.jpg)");
            MatchCollection mcImpAw = Regex.Matches(resultImpAw, vdbParserStr[6]);
            foreach (Match m1 in mcImpAw)
            {
              // Check duplicate entries because Google page links can point to
              // same cover more than once so we don't need them
              check = 0;
              foreach (string text in _imageList)
              {
                if (text == urlImpAw + "posters/" + m1.Groups["poster"])
                {
                  check = 1;
                  break;
                }
              }
              if (check == 0)
              {
                _imageList.Add(urlImpAw + "posters/" + m1.Groups["poster"].Value);
              }
            }
          }
        }
        mGoogle = mGoogle.NextMatch();
      }
      return;
    }

    private string[] VdbParserString()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("IMPAwardsposter");
      return vdbParserStr;
    }
  }
}