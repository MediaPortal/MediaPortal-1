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

    bool _extensiveLogging = true;

    #endregion

    #region Constructor / Destructor

    public AllGameInfoScraper()
    {
      //
      // TODO: Add constructor logic here
      //

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "grabber_AllGame_com.xml")))
      {
        _extensiveLogging = xmlreader.GetValueAsBool("general", "extendedLogging", true);
        templateSearch = xmlreader.GetValue("templateSearch", "template");
        templateSearchTags = xmlreader.GetValue("templateSearch", "tags");
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
      string cleanTitle = String.Empty;

      gameTitle = gameTitle.ToLower();

      for (int i = 0; i < gameTitle.Length; i++)
      {
        if (gameTitle[i] >= 'a' && gameTitle[i] <= 'z')
          cleanTitle += gameTitle[i];
        else if (gameTitle[i] >= 'A' && gameTitle[i] <= 'Z')
          cleanTitle += gameTitle[i];
        else if (gameTitle[i] >= '0' && gameTitle[i] <= '9')
          cleanTitle += gameTitle[i];
        else if (!cleanTitle.EndsWith("+"))
          cleanTitle += '+';
      }

      return String.Format("{0}/cg/agg.dll?sql={1}&P=agg&opt1=31", BaseURL, cleanTitle);
    }

    public string GetScreenshotURL(FileInfo curGame)
    {
      string cleanTitle = String.Empty;

      return String.Format("{0}~T5", curGame.GameURL);
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

    private void WriteFileToLog(FileInfo file)
    {
      if (_extensiveLogging)
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

        foreach (string imgURL in file.ImageURLs)
          Log.Info("MyPrograms FileItem-ImageURL: {0}", imgURL);
      }
    }

    #endregion

    public bool FindGameinfo(string gameTitle)
    {
      gameList.Clear();

      // Create a template - can be loaded from xml config file
      HtmlParserTemplate template = new HtmlParserTemplate();
      template.SectionTemplate = new HtmlSectionTemplate();

      // setup the template
      template.SectionTemplate.Tags = templateSearchTags;
      if (_extensiveLogging) Log.Info("MyPrograms templateSearchTags {0}", templateSearchTags);
      template.SectionTemplate.Template = templateSearch;
      if (_extensiveLogging) Log.Info("MyPrograms templateSearch {0}", templateSearch);

      HtmlParser parser = new HtmlParser(template, typeof(FileInfo), null);

      // Build a request of the site to parse
      if (_extensiveLogging) Log.Info("MyPrograms parsing URL: {0}", GetSearchURL(gameTitle));
      HTTPRequest request = new HTTPRequest(GetSearchURL(gameTitle));

      // Load the site and see how many times the template occurs
      int count = parser.ParseUrl(request);
      if (_extensiveLogging) Log.Info("MyPrograms template was found on url {0} times", count.ToString());

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
        if (curItem == null) return false;
        if (curGame == null) return false;
        if (curGame.GameURL == "") return false;

        string sourceData = GetSourceFromURL(curGame.GameURL);

        if (saveType == ScraperSaveType.DataAndImages || saveType == ScraperSaveType.Data)
        {
          curGame = ParseValues(curGame, sourceData);
          curGame = ParseValueRating(curGame, sourceData);
          curGame = ParseValueLists(curGame, sourceData);
          curGame = ParseTexts(curGame, sourceData);
        }

        if (saveType == ScraperSaveType.DataAndImages || saveType == ScraperSaveType.Images)
        {
          curGame.ImageURLs.Clear();
          curGame = ParsePackshot(curGame, sourceData);

          string sourceImages = GetSourceFromURL(GetScreenshotURL(curGame));
          curGame = ParseScreenshots(curGame, sourceImages);

          // no screenshots are found for this game
          // will try to download screenshots with same or better relevance
          // for example, other country versions
          if (curGame.ImageURLs.Count <= 1)
          {
            Log.Info("MyPrograms: AllGameInfoScraper: No Screenshots found, will try to find, screenies for a similar game now.");
            if (curItem != null)
              if (curItem.FileInfoList != null)
                foreach (FileInfo info in curItem.FileInfoList)
                {
                  // is it the same game again?
                  if (info.GameURL == curGame.GameURL) continue;
                  // is this game worse that the current-selected?
                  if (info.RelevanceNorm < curGame.RelevanceNorm) continue;

                  string nextSource = GetSourceFromURL(GetScreenshotURL(info));
                  FileInfo tempInfo = ParseScreenshots(info, nextSource);

                  if (tempInfo.ImageURLs.Count > 1)
                  {
                    curGame.ImageURLs.AddRange(tempInfo.ImageURLs);
                    break;
                  }
                }
          }
          WriteFileToLog(curGame);
          curGame.DownloadImages(curApp, curItem);
        }

        curGame.Loaded = true;
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("MyPrograms error: {0} - {1}", ex.Message, ex.StackTrace);
        return false;
      }

      #region oldstuff
      /*
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
      */
#endregion
    }

    private string GetSourceFromURL(string url)
    {
      // Again we build a request with site URL
      if (_extensiveLogging) Log.Info("MyPrograms: AllGameInfoScraper: downloading URL: {0}", url);
      HTTPRequest request = new HTTPRequest(url);

      //but instead of calling HtmlParser we will first get the source
      HTMLPage page = new HTMLPage(request);

      return page.GetPage();
    }

    private FileInfo ParseValues(FileInfo curGame, string source)
    {
      if (_extensiveLogging) Log.Info("MyPrograms: AllGameInfoScraper: ParseValues()");

      string template = String.Empty;
      string tags = String.Empty;

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "grabber_AllGame_com.xml")))
      {
        template = xmlreader.GetValue("templateValue", "template");
        tags = xmlreader.GetValue("templateValue", "tags");
      }

      // VALUES

      // now that we have the source we can work on it before going to the parser

      // To parser the source we need again a template  
      // This time only a section template and not a parser template
      HtmlSectionTemplate templateValue = new HtmlSectionTemplate();

      templateValue.Tags = tags;
      templateValue.Template = template;
      if (_extensiveLogging)
      {
        Log.Info("MyPrograms template tags: {0}", tags);
        Log.Info("MyPrograms template \n{0}", template);
      }

      // With the template we create a profiler
      HtmlProfiler profilerValue = new HtmlProfiler(templateValue);

      // and use this to get the number of times the template occurs in our source
      int countValue = profilerValue.MatchCount(source);
      if (_extensiveLogging) Log.Info("MyPrograms template was found on url {0} times", countValue.ToString());

      for (int i = 0; i < countValue; i++)
      {
        // Here we can get the source of each section
        if (_extensiveLogging) Log.Info("MyPrograms template ::: {0}", profilerValue.GetSource(i));
        string sectionSource = profilerValue.GetSource(i);

        // we must also create a place for the parsed data
        ParserData data = new ParserData();

        IParserData iData = data;

        // to parse each section we use a section parser
        HtmlSectionParser parser = new HtmlSectionParser(templateValue);

        // Finally we can parse the section source
        parser.ParseSection(sectionSource, ref iData);

        if (_extensiveLogging)
          for (int j = 0; j < data.Count; j++)
            Log.Info("MyPrograms DATA: {0} --- {1}", data.GetElementName(j), data.GetElementValue(j));

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

    private FileInfo ParseValueRating(FileInfo curGame, string source)
    {
      if (_extensiveLogging) Log.Info("MyPrograms: AllGameInfoScraper: ParseValueRating()");

      string template = String.Empty;
      string tags = String.Empty;

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "grabber_AllGame_com.xml")))
      {
        template = xmlreader.GetValue("templateValueRating", "template");
        tags = xmlreader.GetValue("templateValueRating", "tags");
      }

      // VALUES

      // now that we have the source we can work on it before going to the parser

      // To parser the source we need again a template  
      // This time only a section template and not a parser template
      HtmlSectionTemplate templateValue = new HtmlSectionTemplate();

      templateValue.Tags = tags;
      templateValue.Template = template;
      if (_extensiveLogging)
      {
        Log.Info("MyPrograms template tags: {0}", tags);
        Log.Info("MyPrograms template \n{0}", template);
      }

      // With the template we create a profiler
      HtmlProfiler profilerValue = new HtmlProfiler(templateValue);

      // and use this to get the number of times the template occurs in our source
      int countValue = profilerValue.MatchCount(source);
      if (_extensiveLogging) Log.Info("MyPrograms template was found on url {0} times", countValue.ToString());

      for (int i = 0; i < countValue; i++)
      {
        // Here we can get the source of each section
        if (_extensiveLogging) Log.Info("MyPrograms template ::: {0}", profilerValue.GetSource(i));
        string sectionSource = profilerValue.GetSource(i);

        // we must also create a place for the parsed data
        ParserData data = new ParserData();

        IParserData iData = data;

        // to parse each section we use a section parser
        HtmlSectionParser parser = new HtmlSectionParser(templateValue);

        // Finally we can parse the section source
        parser.ParseSection(sectionSource, ref iData);

        if (_extensiveLogging)
          for (int j = 0; j < data.Count; j++)
            Log.Info("MyPrograms DATA: {0} --- {1}", data.GetElementName(j), data.GetElementValue(j));

        switch (data.GetElement("#KEY"))
        {
          case "AMG Rating":
            curGame.RatingOrig = data.GetElement("#VALUE");
            curGame.RatingNorm = curGame.GetNumber(data.GetElement("#VALUE")) + 1;
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
      if (_extensiveLogging) Log.Info("MyPrograms: AllGameInfoScraper: ParseValueLists()");

      string template = String.Empty;
      string tags = String.Empty;

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "grabber_AllGame_com.xml")))
      {
        template = xmlreader.GetValue("templateValueList", "template");
        tags = xmlreader.GetValue("templateValueList", "tags");
      }

      // VALUES LISTS

      // now that we have the source we can work on it before going to the parser

      // To parser the source we need again a template  
      // This time only a section template and not a parser template
      HtmlSectionTemplate templateValueList = new HtmlSectionTemplate();

      templateValueList.Tags = tags;
      templateValueList.Template = template;
      if (_extensiveLogging)
      {
        Log.Info("MyPrograms template tags: {0}", tags);
        Log.Info("MyPrograms template \n{0}", template);
      }

      // With the template we create a profiler
      HtmlProfiler profilerValueList = new HtmlProfiler(templateValueList);

      // and use this to get the number of times the template occurs in our source
      int countValueList = profilerValueList.MatchCount(source);
      if (_extensiveLogging) Log.Info("MyPrograms template was found on url {0} times", countValueList.ToString());

      for (int i = 0; i < countValueList; i++)
      {
        // Here we can get the source of each section
        if (_extensiveLogging) Log.Info("MyPrograms template ::: {0}", profilerValueList.GetSource(i));
        string sectionSource = profilerValueList.GetSource(i);

        // we must also create a place for the parsed data
        ParserData data = new ParserData();

        IParserData iData = data;

        // to parse each section we use a section parser
        HtmlSectionParser parser = new HtmlSectionParser(templateValueList);

        // Finally we can parse the section source
        parser.ParseSection(sectionSource, ref iData);

        if (_extensiveLogging)
          for (int j = 0; j < data.Count; j++)
            Log.Info("MyPrograms DATA: {0} --- {1}", data.GetElementName(j), data.GetElementValue(j));

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
      if (_extensiveLogging) Log.Info("MyPrograms: AllGameInfoScraper: ParseTexts()");

      string template = String.Empty;
      string tags = String.Empty;

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "grabber_AllGame_com.xml")))
      {
        template = xmlreader.GetValue("templateText", "template");
        tags = xmlreader.GetValue("templateText", "tags");
      }

      // TEXTS

      // now that we have the source we can work on it before going to the parser

      // To parser the source we need again a template  
      // This time only a section template and not a parser template
      HtmlSectionTemplate templateText = new HtmlSectionTemplate();

      templateText.Tags = tags;
      templateText.Template = template;
      if (_extensiveLogging)
      {
        Log.Info("MyPrograms template tags: {0}", tags);
        Log.Info("MyPrograms template \n{0}", template);
      }

      // With the template we create a profiler
      HtmlProfiler profilerText = new HtmlProfiler(templateText);

      // and use this to get the number of times the template occurs in our source
      int countText = profilerText.MatchCount(source);
      if (_extensiveLogging) Log.Info("MyPrograms template was found on url {0} times", countText.ToString());

      for (int i = 0; i < countText; i++)
      {
        // Here we can get the source of each section
        if (_extensiveLogging) Log.Info("MyPrograms template ::: {0}", profilerText.GetSource(i));
        string sectionSource = profilerText.GetSource(i);

        // we must also create a place for the parsed data
        ParserData data = new ParserData();

        IParserData iData = data;

        // to parse each section we use a section parser
        HtmlSectionParser parser = new HtmlSectionParser(templateText);

        // Finally we can parse the section source
        parser.ParseSection(sectionSource, ref iData);

        if (_extensiveLogging)
          for (int j = 0; j < data.Count; j++)
            Log.Info("MyPrograms DATA: {0} --- {1}", data.GetElementName(j), data.GetElementValue(j));

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

    private FileInfo ParsePackshot(FileInfo curGame, string source)
    {
      if (_extensiveLogging) Log.Info("MyPrograms: AllGameInfoScraper: ParsePackshot()");

      string template = String.Empty;
      string tags = String.Empty;

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "grabber_AllGame_com.xml")))
      {
        template = xmlreader.GetValue("packshot", "template");
        tags = xmlreader.GetValue("packshot", "tags");
      }

      // TEXTS

      // now that we have the source we can work on it before going to the parser

      // To parser the source we need again a template  
      // This time only a section template and not a parser template
      HtmlSectionTemplate templateText = new HtmlSectionTemplate();

      templateText.Tags = tags;
      templateText.Template = template;
      if (_extensiveLogging)
      {
        Log.Info("MyPrograms template tags: {0}", tags);
        Log.Info("MyPrograms template \n{0}", template);
      }

      // With the template we create a profiler
      HtmlProfiler profilerText = new HtmlProfiler(templateText);

      // and use this to get the number of times the template occurs in our source
      int countText = profilerText.MatchCount(source);
      if (_extensiveLogging) Log.Info("MyPrograms template was found on url {0} times", countText.ToString());

      for (int i = 0; i < countText; i++)
      {
        // Here we can get the source of each section
        if (_extensiveLogging) Log.Info("MyPrograms template ::: {0}", profilerText.GetSource(i));
        string sectionSource = profilerText.GetSource(i);

        // we must also create a place for the parsed data
        ParserData data = new ParserData();

        IParserData iData = data;

        // to parse each section we use a section parser
        HtmlSectionParser parser = new HtmlSectionParser(templateText);

        // Finally we can parse the section source
        parser.ParseSection(sectionSource, ref iData);

        if (_extensiveLogging)
          for (int j = 0; j < data.Count; j++)
            Log.Info("MyPrograms DATA: {0} --- {1}", data.GetElementName(j), data.GetElementValue(j));
        
        curGame.ImageURLs.Add(data.GetElement("#URL"));
      }

      WriteFileToLog(curGame);
      return curGame;
    }

    private FileInfo ParseScreenshots(FileInfo curGame, string source)
    {
      if (_extensiveLogging) Log.Info("MyPrograms: AllGameInfoScraper: ParseScreenshots()");

      string section = "screenshots";

      string KeyName = String.Empty;
      string KeyString = String.Empty;
      string ValueName = String.Empty;
      string template = String.Empty;
      string tags = String.Empty;

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "grabber_AllGame_com.xml")))
      {
        KeyName = xmlreader.GetValueAsString(section, "KeyName", null);
        KeyString = xmlreader.GetValueAsString(section, "KeyString", null);
        ValueName = xmlreader.GetValueAsString(section, "ValueName", null);
        template = xmlreader.GetValueAsString(section, "template", null);
        tags = xmlreader.GetValueAsString(section, "tags", null);
      }

      if (KeyString == null || KeyString == string.Empty) return curGame;
      if (template == null || template == string.Empty) return curGame;
      if (tags == null || tags == string.Empty) return curGame;
      if (_extensiveLogging)
      {
        Log.Info("MyPrograms KeyName: {0}", KeyName);
        Log.Info("MyPrograms KeyString: {0}", KeyString);
        Log.Info("MyPrograms ValueName: {0}", ValueName);
        Log.Info("MyPrograms template tags: {0}", tags);
        Log.Info("MyPrograms template \n{0}", template);
      }

      // To parser the source we need again a template  
      // This time only a section template and not a parser template
      HtmlSectionTemplate templateText = new HtmlSectionTemplate();

      templateText.Tags = tags;
      templateText.Template = template;
      // With the template we create a profiler
      HtmlProfiler profilerText = new HtmlProfiler(templateText);

      // and use this to get the number of times the template occurs in our source
      int countText = profilerText.MatchCount(source);
      if (_extensiveLogging) Log.Info("MyPrograms template was found on url {0} times", countText.ToString());

      for (int i = 0; i < countText; i++)
      {
        // Here we can get the source of each section
        if (_extensiveLogging) Log.Info("MyPrograms template ::: {0}", profilerText.GetSource(i));
        string sectionSource = profilerText.GetSource(i);

        // we must also create a place for the parsed data
        ParserData data = new ParserData();

        IParserData iData = data;

        // to parse each section we use a section parser
        HtmlSectionParser parser = new HtmlSectionParser(templateText);

        // Finally we can parse the section source
        parser.ParseSection(sectionSource, ref iData);

        if (data.Count != 2) continue;
        if (_extensiveLogging)
          for (int j = 0; j < data.Count; j++)
          {
            Log.Info("MyPrograms DATA name : {0} {1}", j.ToString(), data.GetElement(KeyName));
            Log.Info("MyPrograms DATA value: {0} {1}", j.ToString(), data.GetElementValue(j));
          }

        if (data.GetElement(KeyName).StartsWith(KeyString))
          curGame.ImageURLs.Add(data.GetElement(ValueName));
      }
      return curGame;
    }
  }
}
