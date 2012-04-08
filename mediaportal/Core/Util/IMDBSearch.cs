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
      //string defaultPosterPageLinkURL = "http://www.imdb.com/title/" + imdbID;
      string defaultPosterPageLinkURL = vdbParserStr[0] + imdbID;
      string strBodyPicDefault = GetPage(defaultPosterPageLinkURL, "utf-8");
      
      //Match posterPageLink = Regex.Match(strBodyPicDefault,
      //                                   @"id=""img_primary"">.*?src='/rg/title-overview/primary/images.*?href=""(?<defaultPic>.*?)""",
      //                                   RegexOptions.Singleline);
      Match posterPageLink = Regex.Match(strBodyPicDefault, vdbParserStr[1], RegexOptions.Singleline);

      // Now parse default cover picture html page to get default cover
      
      //strBodyPicDefault = GetPage("http://www.imdb.com" + posterPageLink.Groups["defaultPic"].Value, "utf-8");
      strBodyPicDefault = GetPage(vdbParserStr[2] + posterPageLink.Groups["defaultPic"].Value, "utf-8");
      
      //Match jpgDefault = Regex.Match(strBodyPicDefault, @"<img[\s]id=.*?alt=.*?src=""(?<jpg>.*?jpg)");
      Match jpgDefault = Regex.Match(strBodyPicDefault, vdbParserStr[3]);

      if (jpgDefault.Success)
      {
        _imageList.Add(HttpUtility.HtmlDecode(jpgDefault.Groups["jpg"].Value));
        // Remember default PIC, maybe it is in the Product Group so we can escape duplicate
        defaultPic = HttpUtility.HtmlDecode(jpgDefault.Groups["jpg"].Value);
      }

      if (defaultOnly)
        return;

      // Then get all we can from IMDB Product thumbs group for movie
      
      //string posterPageLinkURL = "http://www.imdb.com/title/" + imdbID + "/mediaindex?refine=product";
      string posterPageLinkURL = vdbParserStr[4] + imdbID + vdbParserStr[5];
      string strBodyThumbs = GetPage(posterPageLinkURL, "utf-8");
      
      // Get all thumbs links and put it in the PIC group
      //MatchCollection thumbs = Regex.Matches(strBodyThumbs, @"(?<PIC>/media/rm\d*/tt\d*)");
      MatchCollection thumbs = Regex.Matches(strBodyThumbs, vdbParserStr[6]);

      foreach (Match thumb in thumbs)
      {
        // Get picture
        string posterUrl = "http://www.imdb.com" + HttpUtility.HtmlDecode(thumb.Groups["PIC"].Value);
        string strBodyPic = GetPage(posterUrl, "utf-8");
        
        //Match jpg = Regex.Match(strBodyPic, @"<img[\s]id=.*?alt=.*?src=""(?<jpg>.*?jpg)");
        Match jpg = Regex.Match(strBodyPic, vdbParserStr[7]);
        
        // No default Picture again if it's here
        if (HttpUtility.HtmlDecode(jpg.Groups["jpg"].Value) != defaultPic &
            HttpUtility.HtmlDecode(jpg.Groups["jpg"].Value) != "")
        {
          _imageList.Add(HttpUtility.HtmlDecode(jpg.Groups["jpg"].Value));
        }
      }
    }

    /// <summary>
    /// Helper function for fetching actorsID from IMDB movie page using IMDBmovieID.
    /// Parameter imdbMovieID must be in IMDB format (ie. tt0123456 including leading zeros)
    /// </summary>
    /// <param name="imdbMovieID"></param>
    /// <param name="actorList"></param>
    public void SearchActors(string imdbMovieID, ref ArrayList actorList)
    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }
      if (imdbMovieID == null) return;
      if (imdbMovieID == string.Empty | !imdbMovieID.StartsWith("tt")) return;

      string[] vdbParserStr = VdbParserStringIMDBActors();

      if (vdbParserStr == null || vdbParserStr.Length != 7)
      {
        return;
      }

      bool shortActorsListSize = true;

      using (Settings xmlreader = new MPSettings())
      {
        if (xmlreader.GetValueAsString("moviedatabase", "actorslistsize", "Short") != "Short")
        {
          shortActorsListSize = false;
        }
      }
      actorList.Clear();

      //string movieURL = "http://www.imdb.com/title/" + imdbMovieID + @"/fullcredits#cast";
      string movieURL = "http://www.imdb.com/title/" + imdbMovieID + vdbParserStr[0];
      
      string strBodyActors = GetPage(movieURL, "utf-8");
      movieURL = "http://www.imdb.com/title/" + imdbMovieID;
      string strBody = GetPage(movieURL, "utf-8");


      if (strBodyActors == string.Empty)
        return;

      // Director
      string strDirectorImdbId = string.Empty;
      string strDirectorName = string.Empty;
      
      //string regexBlockPattern =
      //  @"name=""director[s]""(?<directors_block>.*?)<h5>";
      string regexBlockPattern = vdbParserStr[1];
      
      //string regexPattern = @"<a\s+href=""/name/(?<idDirector>nm\d{7})/""[^>]*>(?<movieDirectors>[^<]+)</a>";
      string regexPattern = vdbParserStr[2];
      string regexBlock =
        Regex.Match(HttpUtility.HtmlDecode(strBodyActors), regexBlockPattern, RegexOptions.Singleline).Groups["directors_block"].Value;
      strDirectorImdbId = Regex.Match(regexBlock, regexPattern, RegexOptions.Singleline).Groups["idDirector"].Value;
      strDirectorName = Regex.Match(regexBlock, regexPattern, RegexOptions.Singleline).Groups["movieDirectors"].Value;

      if (strDirectorImdbId != string.Empty)
      {
        // Add prefix that it's director, will be removed on fetching details
        actorList.Add("*d" + strDirectorName + "|" + strDirectorImdbId + "|" + GUILocalizeStrings.Get(199).Replace(":", string.Empty));
      }

      //Writers
      regexBlockPattern = vdbParserStr[3];
      regexBlock = Regex.Match(HttpUtility.HtmlDecode(strBody), regexBlockPattern, RegexOptions.Singleline).Groups["writers_block"].Value;

      //regexPattern = @"/writer-\d/.*?/name/(?<imdbWriterId>nm\d{7})/""[\s]+>(?<writer>.*?)</a>";
      regexPattern = vdbParserStr[4];
      MatchCollection mc = Regex.Matches(regexBlock, regexPattern);

      if (mc.Count != 0)
      {
        foreach (Match m in mc)
        {
          string writerId = string.Empty;
          writerId = HttpUtility.HtmlDecode(m.Groups["imdbWriterId"].Value.Trim());
          
          string strWriterName = string.Empty;
          strWriterName = HttpUtility.HtmlDecode(m.Groups["writer"].Value.Trim());

          string writerRole = string.Empty;
          writerRole = HttpUtility.HtmlDecode(m.Groups["wrole"].Value.Trim());

          bool found = false;
          
          for (int i = 0; i < actorList.Count; i++)
          {
            if (writerId != null)
            {
              if (actorList[i].ToString().Contains(writerId))
              {
                // Check if writer is also director and add new role
                if (!string.IsNullOrEmpty(writerRole))
                {
                  actorList[i] = actorList[i] + ", " + GUILocalizeStrings.Get(200)+ " " + writerRole.Replace("(", string.Empty).Replace(")", string.Empty);
                  found = true;
                }
                else
                {
                  actorList[i] = actorList[i] + ", " + GUILocalizeStrings.Get(200).Replace(":", string.Empty);
                  found = true;
                }
                break;
              }
            }
          }
          
          if (!found && writerId != string.Empty)
          {
            if (!string.IsNullOrEmpty(writerRole))
            {
              actorList.Add(strWriterName + "|" + writerId + "|" +
                          GUILocalizeStrings.Get(200)+ " " + writerRole.Replace("(", string.Empty).Replace(")", string.Empty));
            }
            else
            {
              actorList.Add(strWriterName + "|" + writerId + "|" +
                          GUILocalizeStrings.Get(200).Replace(":", string.Empty));
            }
          }
        }
      }

      // cast
      
      //regexBlockPattern = @"<table class=""cast"">.*?</table>|<table class=""cast_list"">.*?</table>";
      regexBlockPattern = vdbParserStr[5];
      
      //regexPattern = @"<td[^<]*<a\s+href=""/name/(?<imdbActorID>nm\d{7})/""[^>]*>(?<actor>[^<]*)</a>.*?<td.class=""char"">(?<role>.*?)<*?</td>";
      regexPattern = vdbParserStr[6];

      Match castBlock = Regex.Match(strBodyActors, regexBlockPattern, RegexOptions.Singleline);
      string strCastBlock = HttpUtility.HtmlDecode(castBlock.Value);

      if (strCastBlock != null)
      {
        mc = Regex.Matches(strCastBlock, regexPattern, RegexOptions.Singleline);

        if (mc.Count != 0)
        {
          int actorsCount = 0;

          foreach (Match m in mc)
          {
            string strActorID = string.Empty;
            strActorID = m.Groups["imdbActorID"].Value;
            strActorID = Utils.stripHTMLtags(strActorID).Trim();

            string strActorName = string.Empty;
            strActorName = m.Groups["actor"].Value;
            strActorName = Utils.stripHTMLtags(strActorName).Trim();

            string strRole = string.Empty;
            strRole = m.Groups["role"].Value;
            strRole = HttpUtility.HtmlDecode(strRole);
            strRole = Utils.stripHTMLtags(strRole).Trim().Replace("\n", "");
            strRole = strRole.Replace(",", ";").Replace("  ", "");
            
            if (!strRole.StartsWith("("))
            {
              string regex = "(\\(.*\\))";
              strRole = Regex.Replace(strRole, regex, "").Trim();
            }
            else
            {
              string regex = "(\\(as.*\\))";
              strRole = Regex.Replace(strRole, regex, "").Trim().
                              Replace("(", string.Empty).
                              Replace(")", string.Empty);
            }
            
            // Check if we have allready actor as director (actor also is director for movie)
            bool found = false;
            
            for (int i = 0; i < actorList.Count; i++)
            {
              if (actorList[i].ToString().Contains(strActorID))
              {
                if (strRole != string.Empty)
                {
                  actorList[i] = actorList[i] + ", " + strRole;
                }
                
                found = true;
                break;
              }
            }
            
            if (!found && strActorID != string.Empty)
            {
              actorList.Add(strActorName + "|" + strActorID + "|" + strRole);
            }

            actorsCount++;
            
            if (shortActorsListSize)
            {
              if (actorsCount >= 15)
              {
                return;
              }
            }
          }
        }
      }
    }

    private string[] VdbParserStringIMDBPoster()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("IMDBPoster");
      return vdbParserStr;
    }

    private string[] VdbParserStringIMDBActors()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("IMDBActors");
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
        req.Timeout = 10000;
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