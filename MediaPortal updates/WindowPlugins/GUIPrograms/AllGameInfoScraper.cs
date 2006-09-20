/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using MediaPortal.Util;
using Programs.Utils;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for AllGameInfoScraper.
  /// Heavily inspired by Frodo's MusicInfoScraper..... :-)
  /// </summary>
  public class AllGameInfoScraper
  {
    ArrayList gameList = new ArrayList();

    public AllGameInfoScraper()
    {
      //
      // TODO: Add constructor logic here
      //
    }

    public int Count
    {
      get
      {
        return gameList.Count;
      }
    }

    public FileInfo this[int index]
    {
      get
      {
        return (FileInfo)gameList[index];
      }
    }

    public ArrayList FileInfos
    {
      get
      {
        return gameList;
      }
    }

    string AddMissingRowTags(string htmlTableText)
    {
      // poor man's replace.... let's try:
      string origText = "</TD>\r\n<TR";
      string replaceText = "</TD>\r\n</TR>\r\n<TR";
      htmlTableText = htmlTableText.Replace(origText, replaceText);
      return htmlTableText;
    }

    public bool FindGameinfo(string gameTitle)
    {
      gameList.Clear();

      // make request
      // type is 
      // http://www.allgame.com/cg/agg.dll?p=agg&type=1&SRCH=SuperMario64

      string httpPostLine = String.Format("P=agg&TYPE=1&SRCH={0}", gameTitle);

      string htmlText = PostHTTP("http://www.allgame.com/cg/agg.dll", httpPostLine);
      if (htmlText.Length == 0)
        return false;

      string htmlLowText = htmlText;
      htmlLowText = htmlLowText.ToLower();
      int startOfTable = htmlLowText.IndexOf(">games with titles matching");
      if (startOfTable < 0)
        return false;
      startOfTable = htmlLowText.IndexOf("<table", startOfTable);
      if (startOfTable < 0)
        return false;

      HTMLUtil util = new HTMLUtil();
      HTMLTable table = new HTMLTable();
      string htmlTableText = htmlText.Substring(startOfTable);

      // now the allgame thing is that <tr> tags are not closed
      // for the decisive rows.... so I add them manually
      // otherwise the parser doesn't split up the string correctly
      htmlTableText = AddMissingRowTags(htmlTableText);
      table.Parse(htmlTableText); // call frodo's html parser
      for (int i = 1; i < table.Rows; ++i)
      // skip first row (contains table header)
      {
        FileInfo newGame = new FileInfo();

        //							FileItem newGameInfo = new FileItem(null);  // todo: initSqlDB necessary????
        //							util.ConvertHTMLToAnsi(strAlbumName, out strAlbumNameStripped);
        //							newGameInfo.Title2=strAlbumNameStripped;
        //							newGameInfo.URL=strAlbumURL;
        //							m_games.Add(newGameInfo);


        HTMLTable.HTMLRow row = table.GetRow(i);
        for (int column = 0; column < row.Columns; ++column)
        {
          string columnHTML = row.GetColumValue(column);

          // ok here we cycle throuh the 8 columns of one table row:
          // col 0: "Relevance" => see width of the picture to measure this
          // col 1: "Year" 
          // col 2: "buy it"-link
          // col 3: "Title" => includes the detail URL
          // col 4: "Genre"
          // col 5: "Style"
          // col 6: "Platform"
          // col 7: "Rating" => use imagename to get rating: "st_gt1.gif" to "st_gt9.gif" 

          if (column == 0)
          {
            string gameRelevance = "";
            int startOfWidthTag =  - 1;
            int endOfWidthTag =  - 1;
            // ex:
            // "<img src="/im/agg/red_dot.jpg" valign=center width="56" height=5 border=0>&nbsp;"
            // the WIDTH attribute is the relevance: maximum value is 56, negative values are possible
            startOfWidthTag = columnHTML.IndexOf("width=\"");
            if (startOfWidthTag !=  - 1)
            {
              startOfWidthTag = columnHTML.IndexOf("\"", startOfWidthTag);
              if (startOfWidthTag !=  - 1)
              {
                endOfWidthTag = columnHTML.IndexOf("\"", startOfWidthTag + 1);
                if ((endOfWidthTag !=  - 1) && (endOfWidthTag > startOfWidthTag))
                {
                  gameRelevance = columnHTML.Substring(startOfWidthTag + 1, endOfWidthTag - startOfWidthTag - 1);
                }
              }
            }
            newGame.RelevanceOrig = gameRelevance;
            newGame.RelevanceNorm = (ProgramUtils.StrToIntDef(gameRelevance,  - 1) + 44);
          }
          else if (column == 1)
          {
            string gameYear = "";
            util.RemoveTags(ref columnHTML);
            gameYear = columnHTML.Replace("&nbsp;", "");
            newGame.Year = gameYear;
          }
          else if (column == 2)
          {
            // NOTHING TO DO, skip the bloody "buy-it" link ;-)
          }
          else if (column == 3)
          {
            // ex:
            // "<FONT SIZE=-1><A HREF=/cg/agg.dll?p=agg&SQL=GIH|||||1002>Super Mario 64</A></FONT>"
            string gameURL = "";
            int startOfURLTag =  - 1;
            int endOfURLTag =  - 1;
            startOfURLTag = columnHTML.ToLower().IndexOf("<a href");
            if (startOfURLTag !=  - 1)
            {
              startOfURLTag = columnHTML.IndexOf("/", startOfURLTag);
              if (startOfURLTag !=  - 1)
              {
                endOfURLTag = columnHTML.IndexOf(">", startOfURLTag + 1);
                if ((endOfURLTag !=  - 1) && (endOfURLTag > startOfURLTag))
                {
                  gameURL = columnHTML.Substring(startOfURLTag, endOfURLTag - startOfURLTag);
                  // and add the prefix!
                  gameURL = "http://www.allgame.com" + gameURL;
                }
              }
            }

            string gameTitleHTML = "";
            util.RemoveTags(ref columnHTML);
            gameTitleHTML = columnHTML.Replace("&nbsp;", "");
            newGame.Title = gameTitleHTML;
            newGame.GameURL = gameURL;

          }
          else if (column == 4)
          {
            string strGenre = "";
            util.RemoveTags(ref columnHTML);
            strGenre = columnHTML.Replace("&nbsp;", "");
            newGame.Genre = strGenre;
          }
          else if (column == 5)
          {
            string strStyle = "";
            util.RemoveTags(ref columnHTML);
            strStyle = columnHTML.Replace("&nbsp;", "");
            newGame.Style = strStyle;
          }
          else if (column == 6)
          {
            string strPlatform = "";
            util.RemoveTags(ref columnHTML);
            strPlatform = columnHTML.Replace("&nbsp;", "");
            newGame.Platform = strPlatform;
          }
          else if (column == 7)
          {
            string strRating = "";
            // ex.
            // <A HREF=/cg/agg.dll?p=agg&SQL=GRH|||||1002><IMG SRC=/im/agg/st_gt9.gif BORDER=0 WIDTH=75 HEIGHT=15 VSPACE=2></A>
            // the rating is coded in the gif - filename
            // st_gt1.gif is the worst rating
            // st_gt9.gif is the best rating
            strRating = "";
            int startOfRatingTag =  - 1;
            int endOfRatingTag =  - 1;
            startOfRatingTag = columnHTML.ToLower().IndexOf("<img src=");
            if (startOfRatingTag !=  - 1)
            {
              startOfRatingTag = columnHTML.IndexOf("/st_gt", startOfRatingTag);
              if (startOfRatingTag !=  - 1)
              {
                endOfRatingTag = columnHTML.IndexOf(".gif", startOfRatingTag);
                if ((endOfRatingTag !=  - 1) && (endOfRatingTag > startOfRatingTag))
                {
                  strRating = columnHTML.Substring(startOfRatingTag + 6, 1); // 6 is the length of the IndexOf searchstring...
                }
              }
            }
            newGame.RatingOrig = strRating;
            newGame.RatingNorm = (ProgramUtils.StrToIntDef(strRating, 4) + 1); // add 1 to get a max rating of 10!
            gameList.Add(newGame);
          }
        }
      }
      return true;
    }


    public bool FindGameinfoDetail(AppItem curApp, FileItem curItem, FileInfo curGame, ScraperSaveType saveType)
    {
      if (curItem == null)
        return false;
      if (curGame == null)
        return false;
      if (curGame.GameURL == "")
        return false;

      HTMLUtil util = new HTMLUtil();

      // query string is as in the following example:
      // ALTERED BEAST for sega genesis
      // http://www.allgame.com/cg/agg.dll?p=agg&SQL=GIH|||||||66
      // To use PostHTTP, we have to split the parameters from the full url

      //string strPostData="p=agg&SQL=GIH|||||||66";
      string strPostData = curGame.GetGameURLPostParams();
      if (strPostData == "")
        return false;
      string strHTML = PostHTTP("http://www.allgame.com/cg/agg.dll", strPostData);

      if (strHTML.Length == 0)
        return false;

      string strHTMLLow = strHTML;
      strHTMLLow = strHTMLLow.ToLower();


      // 1) get MANUFACTURER
      string strManufacturer = "";
      int startOfManuTag =  - 1;
      int endOfManuTag =  - 1;
      // ex:
      // <TR><TD ALIGN=RIGHT BGCOLOR="#FF9933" WIDTH=122><FONT COLOR="#000000" SIZE=-1>Developer</FONT></TD>
      // <TD WIDTH=482 BGCOLOR="#D8D8D8" VALIGN=bottom><TABLE WIDTH=484 BGCOLOR="#D8D8D8" BORDER=0 CELLSPACING=0 CELLPADDING=0><TR>
      // <TD WIDTH=4><IMG SRC=/im/agg/1.gif WIDTH=4 HEIGHT=1></TD><TD WIDTH=478><A HREF=/cg/agg.dll?p=agg&SQL=CIB||||||970>Mythos Games, Ltd.</A></TD></TR>

      // a) FIND the "DEVELOPER" text
      // b) FIND the next table row
      // c) remove tags, trim "developer" away
      startOfManuTag = strHTMLLow.IndexOf(">developer<");
      if (startOfManuTag !=  - 1)
      {
        startOfManuTag = strHTMLLow.IndexOf("<tr>", startOfManuTag);
        endOfManuTag = strHTMLLow.IndexOf("</tr>", startOfManuTag);
        if ((endOfManuTag !=  - 1) && (endOfManuTag > startOfManuTag))
        {
          strManufacturer = strHTML.Substring(startOfManuTag, endOfManuTag - startOfManuTag);
          util.RemoveTags(ref strManufacturer);

          if (strManufacturer != "")
          {
            curGame.Manufacturer = strManufacturer;
          }

        }
      }

      curGame.ImageURLs = ""; // clear all imageurls

      // 2) get OVERVIEW / COVERSHOT
      string strOverview = "";
      string strCovershot = "";
      int startOfOvTag =  - 1;
      int endOfOvTag =  - 1;
      startOfOvTag = strHTMLLow.IndexOf("<img src=\"http://image.allmusic.com/00/agg/cov200");
      if (startOfOvTag ==  - 1)
      {
        // no covershot found: maybe there's still a review there....
        startOfOvTag = strHTMLLow.IndexOf("<table border=0 bgcolor=\"#d8d8d8\"");
        //	Log.Write("dw scraper: {0}", iStartOfOV);
      }
      if (startOfOvTag !=  - 1)
      {
        endOfOvTag = strHTMLLow.IndexOf("</tr>", startOfOvTag);
        if ((endOfOvTag !=  - 1) && (endOfOvTag > startOfOvTag))
        {
          strOverview = strHTML.Substring(startOfOvTag, endOfOvTag - startOfOvTag);
          util.RemoveTags(ref strOverview);

          if (strOverview != "")
          {
            strOverview = strOverview.Replace("\r", "\r\n");
            strOverview = strOverview.Replace("\n", "\r\n");
            strOverview = strOverview.Replace("&#151;", "\r\n");
            curGame.Overview = strOverview;
          }

        }
        int startOfCovershot = startOfOvTag;
        int endOfCovershot =  - 1;
        if (startOfCovershot !=  - 1)
        {
          startOfCovershot = strHTMLLow.IndexOf("\"", startOfCovershot);
          if (startOfCovershot !=  - 1)
          {
            endOfCovershot = strHTMLLow.IndexOf("\"", startOfCovershot + 1);
            if ((endOfCovershot !=  - 1) && (endOfCovershot > startOfCovershot))
            {
              strCovershot = strHTML.Substring(startOfCovershot + 1, endOfCovershot - startOfCovershot - 1);
              curGame.AddImageURL(strCovershot);
            }
          }
        }
      }


      // 3) get SCREENSHOTS
      string strCurScreen = "";
      int startOfImgTag =  - 1;
      int startOfLink =  - 1;
      int endOfLink =  - 1;
      bool bGoOn = true;
      startOfImgTag = strHTMLLow.IndexOf("<a href=http://image.allmusic.com/00/agg/screen250");
      bGoOn = (startOfImgTag !=  - 1);
      while (bGoOn)
      {
        startOfLink = strHTMLLow.IndexOf("=", startOfImgTag);
        if (startOfLink !=  - 1)
        {
          startOfLink++;
          endOfLink = strHTMLLow.IndexOf(">", startOfLink);
          if ((endOfLink !=  - 1) && (endOfLink > startOfLink))
          {
            strCurScreen = strHTML.Substring(startOfLink, endOfLink - startOfLink);
            if (strCurScreen != "")
            {
              curGame.AddImageURL(strCurScreen);
            }
          }
        }

        startOfImgTag = strHTMLLow.IndexOf("<a href=http://image.allmusic.com/00/agg/screen250", startOfImgTag + 1);
        bGoOn = (startOfImgTag !=  - 1);
      }

      if ((saveType == ScraperSaveType.DataAndImages) || (saveType == ScraperSaveType.Images))
      {
        curGame.DownloadImages(curApp, curItem);
      }
      return true;
    }


    string PostHTTP(string strURL, string strData)
    {
      try
      {
        string strBody;
        WebRequest req = WebRequest.Create(strURL);
        req.Method = "POST";
        req.ContentType = "application/x-www-form-urlencoded";

        byte[] bytes = null;
        // Get the data that is being posted (or sent) to the server
        bytes = Encoding.ASCII.GetBytes(strData);
        req.ContentLength = bytes.Length;
        // 1. Get an output stream from the request object
        Stream outputStream = req.GetRequestStream();

        // 2. Post the data out to the stream
        outputStream.Write(bytes, 0, bytes.Length);

        // 3. Close the output stream and send the data out to the web server
        outputStream.Close();


        WebResponse result = req.GetResponse();
        Stream ReceiveStream = result.GetResponseStream();
        Encoding encode = Encoding.GetEncoding("utf-8");
        StreamReader sr = new StreamReader(ReceiveStream, encode);
        strBody = sr.ReadToEnd();

        outputStream.Close();
        ReceiveStream.Close();
        sr.Close();
        result.Close();


        req = null;
        outputStream = null;
        result = null;
        ReceiveStream = null;
        sr = null;

        return strBody;
      }
      //			catch(Exception ex)
      catch (Exception)
      {
        //				Log.Write("AllGameInfoScraper exception: {0}", ex.ToString());
      }

      return "";
    }
  }

}
