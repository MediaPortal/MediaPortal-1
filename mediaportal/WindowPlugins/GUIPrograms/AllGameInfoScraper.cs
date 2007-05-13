#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

#endregion

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;

using MediaPortal.Profile;
using MediaPortal.Services;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Utils.Web;

using Programs.Utils;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for AllGameInfoScraper.
  /// old version: Heavily inspired by Frodo's MusicInfoScraper..... :-)
  /// new version: Heavily inspired by James' WebEPG grabber..... :-)
  /// </summary>
  public class AllGameInfoScraper
  {
    #region Base & Content Variables

    List<FileInfo> gameList = new List<FileInfo>();

    string templateSearch = String.Empty;
    string templateSearchTags = String.Empty;

    string tValue = String.Empty;
    string tValueTags = String.Empty;
    string tValueList = String.Empty;
    string tValueListTags = String.Empty;
    string tText = String.Empty;
    string tTextTags = String.Empty;

    #endregion

    #region Constructor / Destructor

    public AllGameInfoScraper()
    {
      //
      // TODO: Add constructor logic here
      //

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "grabber_AllGame_com.xml")))
      {
        templateSearch = xmlreader.GetValue("templateSearch", "template");
        templateSearchTags = xmlreader.GetValue("templateSearch", "tags");
        tValue = xmlreader.GetValue("templateValue", "template");
        tValueTags = xmlreader.GetValue("templateValue", "tags");
        tValueList = xmlreader.GetValue("templateValueList", "template");
        tValueListTags = xmlreader.GetValue("templateValueList", "tags");
        tText = xmlreader.GetValue("templateText", "template");
        tTextTags = xmlreader.GetValue("templateText", "tags");
      }
    }

    #endregion

    #region Properties / Helper Routines
    
    public string BaseURL
    {
      get
      {
        return "http://www.allgame.com";
      }
    }

    public string GetSearchURL(string gameTitle)
    {
      return String.Format("{0}/cg/agg.dll?sql={1}&P=agg&opt1=31", BaseURL, gameTitle.Replace(' ', '+'));
    }

    public string MakeURL(string url)
    {
      url = url.Replace("&amp;", "&"); ;
      return url;
    }

    public int Count
    {
      get
      {
        return gameList.Count;
      }
    }

    public List<FileInfo> FileInfos
    {
      get
      {
        return gameList;
      }
    }

    #endregion

    private void WriteFileToLog(FileInfo file)
    {
      Log.Info("MyPrograms FileItem:" +
        "\n   Title:    " + file.Title +
        "\n   Year:     " + file.Year +
        "\n   Platform: " + file.Platform +
        "\n   Genre:    " + file.Genre +
        "\n   Style:    " + file.Style +
        "\n   Manufacturer:    " + file.Manufacturer +
        "\n   URL:      " + file.GameURL +
        "\n   RelOrig:  " + file.RelevanceOrig +
        "\n   RelNorm:  " + file.RelevanceNorm.ToString() +
        "\n   RatOrig:  " + file.RatingOrig +
        "\n   RatNorm:  " + file.RatingNorm.ToString() +
        "\n   Overview: " + file.Overview
        );
    }

    public bool FindGameinfo(string gameTitle)
    {
      gameList.Clear();

      // Create a template - can be loaded from xml config file
      HtmlParserTemplate template = new HtmlParserTemplate();
      template.SectionTemplate = new HtmlSectionTemplate();

      /*
       <tr class="visible" id="trlink" onclick="z('1:23413')">
       <td class="sorted-cell"><div class="bar" style="width: 56px;">&nbsp;</div></td>
       <td class="cell">&nbsp;</td>
       <td class="cell" style="width: 214px;"><a href="/cg/agg.dll?p=agg&amp;sql=1:23413">Mario Kart 64 <i>European</i></a></td>
       <td class="cell">&nbsp;</td>
       <td class="cell" style="width: 40px;">1997</td>
       <td class="cell" style="width: 60px;">Racing</td>
       <td class="cell" style="width: 70px;">Go-Kart Racing</td>
       <td class="cell" style="width: 180px;">Nintendo 64</td>
       <td class="cell-img"><img src="/img/stars/st_r7.gif" alt="4 Stars" title="4 Stars"></td>
       */

      // ok here we cycle throuh the 8 columns of one table row:
      // col 0: "Relevance" => see width of the picture to measure this
      // col 1: "Year" 
      // col 2: "buy it"-link
      // col 3: "Title" => includes the detail URL
      // col 4: "Genre"
      // col 5: "Style"
      // col 6: "Platform"
      // col 7: "Rating" => use imagename to get rating: "st_gt1.gif" to "st_gt9.gif"

      // setup the template
      template.SectionTemplate.Tags = templateSearchTags;
      Log.Info("MyPrograms templateSearchTags {0}", templateSearchTags);
      template.SectionTemplate.Template = templateSearch;
      Log.Info("MyPrograms templateSearch {0}", templateSearch);

      HtmlParser parser = new HtmlParser(template, typeof(FileInfo), null);

      // Build a request of the site to parse
      Log.Info("MyPrograms parsing URL: {0}", GetSearchURL(gameTitle));
      HTTPRequest request = new HTTPRequest(GetSearchURL(gameTitle));

      // Load the site and see how many times the template occurs
      int count = parser.ParseUrl(request);
      Log.Info("MyPrograms template was found on url {0} times", count.ToString());

      // now we can get the data for each occurance
      for (int i = 0; i < count; i++)
      {
        FileInfo file = (FileInfo)parser.GetData(i);
        // and here we do something with the data - display it, store it, etc

        // <td class="sorted-cell"><div class="bar" style="width: 56px;">&nbsp;</div></td>
        // the WIDTH attribute is the relevance: maximum value is 56, negative values are possible
        file.RelevanceNorm += 44;

        // <td class="cell" style="width: 214px;"><a href="/cg/agg.dll?p=agg&amp;sql=1:23413">Mario Kart 64 <i>European</i></a></td>
        file.GameURL = String.Format("{0}{1}", BaseURL, file.GameURL);
        file.GameURL = MakeURL(file.GameURL);

        WriteFileToLog(file);

        gameList.Add(file);
      }





      /*
        string gameRelevance = "";
        int startOfWidthTag = -1;
        int endOfWidthTag = -1;
        startOfWidthTag = columnHTML.IndexOf("width=\"");
        if (startOfWidthTag != -1)
        {
          startOfWidthTag = columnHTML.IndexOf("\"", startOfWidthTag);
          if (startOfWidthTag != -1)
          {
            endOfWidthTag = columnHTML.IndexOf("\"", startOfWidthTag + 1);
            if ((endOfWidthTag != -1) && (endOfWidthTag > startOfWidthTag))
            {
              gameRelevance = columnHTML.Substring(startOfWidthTag + 1, endOfWidthTag - startOfWidthTag - 1);
            }
          }
        }
        newGame.RelevanceOrig = gameRelevance;
        newGame.RelevanceNorm = (ProgramUtils.StrToIntDef(gameRelevance, -1) + 44);
        */


      /*
      // ex:
      // "<FONT SIZE=-1><A HREF=/cg/agg.dll?p=agg&SQL=GIH|||||1002>Super Mario 64</A></FONT>"
      string gameURL = "";
      int startOfURLTag = -1;
      int endOfURLTag = -1;
      startOfURLTag = columnHTML.ToLower().IndexOf("<a href");
      if (startOfURLTag != -1)
      {
        startOfURLTag = columnHTML.IndexOf("/", startOfURLTag);
        if (startOfURLTag != -1)
        {
          endOfURLTag = columnHTML.IndexOf(">", startOfURLTag + 1);
          if ((endOfURLTag != -1) && (endOfURLTag > startOfURLTag))
          {
            gameURL = columnHTML.Substring(startOfURLTag, endOfURLTag - startOfURLTag);
            // and add the prefix!
            gameURL = "http://www.allgame.com" + gameURL;
          }
        }
      }
      */


      /*
      string strRating = "";
      // ex.
      // <A HREF=/cg/agg.dll?p=agg&SQL=GRH|||||1002><IMG SRC=/im/agg/st_gt9.gif BORDER=0 WIDTH=75 HEIGHT=15 VSPACE=2></A>
      // the rating is coded in the gif - filename
      // st_gt1.gif is the worst rating
      // st_gt9.gif is the best rating
      strRating = "";
      int startOfRatingTag = -1;
      int endOfRatingTag = -1;
      startOfRatingTag = columnHTML.ToLower().IndexOf("<img src=");
      if (startOfRatingTag != -1)
      {
        startOfRatingTag = columnHTML.IndexOf("/st_gt", startOfRatingTag);
        if (startOfRatingTag != -1)
        {
          endOfRatingTag = columnHTML.IndexOf(".gif", startOfRatingTag);
          if ((endOfRatingTag != -1) && (endOfRatingTag > startOfRatingTag))
          {
            strRating = columnHTML.Substring(startOfRatingTag + 6, 1); // 6 is the length of the IndexOf searchstring...
          }
        }
      }
      */
      if (count > 0)
        return true;
      else
        return false;
    }

    public bool FindGameinfoDetail(AppItem curApp, FileItem curItem, FileInfo curGame, ScraperSaveType saveType)
    {
      try
      {
        // Again we build a request with site URL
        Log.Info("MyPrograms parsing URL: {0}", curGame.GameURL);
        HTTPRequest request = new HTTPRequest(curGame.GameURL);

        //but instead of calling HtmlParser we will first get the source
        HTMLPage page = new HTMLPage(request);
        string source = page.GetPage();


        curGame = ParseValues(curGame, source);
        curGame = ParseValueLists(curGame, source);
        curGame = ParseTexts(curGame, source);

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("MyPrograms error: {0} - {1}", ex.Message, ex.StackTrace);
        return false;
      }

      #region oldstuff
      /*
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
        //	Log.Info("dw scraper: {0}", iStartOfOV);
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
      */
#endregion
    }

    private FileInfo ParseValues(FileInfo curGame, string source)
    {

      // VALUES

      // now that we have the source we can work on it before going to the parser

      // To parser the source we need again a template  
      // This time only a section template and not a parser template
      HtmlSectionTemplate templateValue = new HtmlSectionTemplate();

      templateValue.Tags = tValueTags;
      templateValue.Template = tValue;
      Log.Info("MyPrograms template tags: {0}", tValueTags);
      Log.Info("MyPrograms template \n{0}", tValue);

      // With the template we create a profiler
      HtmlProfiler profilerValue = new HtmlProfiler(templateValue);

      // and use this to get the number of times the template occurs in our source
      int countValue = profilerValue.MatchCount(source);
      Log.Info("MyPrograms template was found on url {0} times", countValue.ToString());

      for (int i = 0; i < countValue; i++)
      {
        // Here we can get the source of each section
        Log.Info("MyPrograms template ::: {0}", profilerValue.GetSource(i));
        string sectionSource = profilerValue.GetSource(i);

        // we must also create a place for the parsed data
        ParserData data = new ParserData();

        IParserData iData = data;

        // to parse each section we use a section parser
        HtmlSectionParser parser = new HtmlSectionParser(templateValue);

        // Finally we can parse the section source
        parser.ParseSection(sectionSource, ref iData);

        for (int j = 0; j < data.Count; j++)
        {
          Log.Info("MyPrograms DATA: {0} --- {1}", data.GetElementName(j), data.GetElementValue(j));
        }

        switch (data.GetElement("#KEY"))
        {
          case "Title":
            curGame.Title = data.GetElement("#VALUE");
            break;
          case "Platform":
            //curGame.Platform = data.GetElement("#VALUE");
            break;
          case "Developer":
            curGame.Manufacturer = data.GetElement("#VALUE");
            break;
          case "Publisher":
            //curGame.Manufacturer = data.GetElement("#VALUE");
            break;
          default:
            break;
        }
      }

      WriteFileToLog(curGame);

      return curGame;
    }

    private FileInfo ParseValueLists(FileInfo curGame, string source)
    {

      // VALUES LISTS

      // now that we have the source we can work on it before going to the parser

      // To parser the source we need again a template  
      // This time only a section template and not a parser template
      HtmlSectionTemplate templateValueList = new HtmlSectionTemplate();

      templateValueList.Tags = tValueListTags;
      templateValueList.Template = tValueList;
      Log.Info("MyPrograms template tags: {0}", tValueListTags);
      Log.Info("MyPrograms template \n{0}", tValueList);

      // With the template we create a profiler
      HtmlProfiler profilerValueList = new HtmlProfiler(templateValueList);

      // and use this to get the number of times the template occurs in our source
      int countValueList = profilerValueList.MatchCount(source);
      Log.Info("MyPrograms template was found on url {0} times", countValueList.ToString());

      for (int i = 0; i < countValueList; i++)
      {
        // Here we can get the source of each section
        Log.Info("MyPrograms template ::: {0}", profilerValueList.GetSource(i));
        string sectionSource = profilerValueList.GetSource(i);

        // we must also create a place for the parsed data
        ParserData data = new ParserData();

        IParserData iData = data;

        // to parse each section we use a section parser
        HtmlSectionParser parser = new HtmlSectionParser(templateValueList);

        // Finally we can parse the section source
        parser.ParseSection(sectionSource, ref iData);

        for (int j = 0; j < data.Count; j++)
        {
          Log.Info("MyPrograms DATA: {0} --- {1}", data.GetElementName(j), data.GetElementValue(j));
        }

        switch (data.GetElement("#KEY"))
        {
          case "Title":
            curGame.Title = data.GetElement("#VALUE");
            break;
          case "Platform":
            //curGame.Platform = data.GetElement("#VALUE");
            break;
          case "Developer":
            curGame.Manufacturer = data.GetElement("#VALUE");
            break;
          case "Publisher":
            //curGame.Manufacturer = data.GetElement("#VALUE");
            break;
          default:
            break;
        }
      }

      WriteFileToLog(curGame);

      return curGame;
    }

    private FileInfo ParseTexts(FileInfo curGame, string source)
    {

      // TEXTS

      // now that we have the source we can work on it before going to the parser

      // To parser the source we need again a template  
      // This time only a section template and not a parser template
      HtmlSectionTemplate templateText = new HtmlSectionTemplate();

      templateText.Tags = tTextTags;
      templateText.Template = tText;
      Log.Info("MyPrograms template tags: {0}", tTextTags);
      Log.Info("MyPrograms template \n{0}", tText);

      // With the template we create a profiler
      HtmlProfiler profilerText = new HtmlProfiler(templateText);

      // and use this to get the number of times the template occurs in our source
      int countText = profilerText.MatchCount(source);
      Log.Info("MyPrograms template was found on url {0} times", countText.ToString());

      for (int i = 0; i < countText; i++)
      {
        // Here we can get the source of each section
        Log.Info("MyPrograms template ::: {0}", profilerText.GetSource(i));
        string sectionSource = profilerText.GetSource(i);

        // we must also create a place for the parsed data
        ParserData data = new ParserData();

        IParserData iData = data;

        // to parse each section we use a section parser
        HtmlSectionParser parser = new HtmlSectionParser(templateText);

        // Finally we can parse the section source
        parser.ParseSection(sectionSource, ref iData);

        for (int j = 0; j < data.Count; j++)
        {
          Log.Info("MyPrograms DATA: {0} --- {1}", data.GetElementName(j), data.GetElementValue(j));
        }

        switch (data.GetElement("#KEY"))
        {
          case "Synopsis":
            curGame.Overview = data.GetElement("#TEXT");
            break;
          default:
            break;
        }
      }

      WriteFileToLog(curGame);

      return curGame;
    }
  }
}
