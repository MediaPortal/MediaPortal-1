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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  internal class XMLTVImport : IComparer
  {
    private readonly ProgramManagement _programManagement = new ProgramManagement();
    
    public delegate void ShowProgressHandler(string status, ImportStats stats);

    private class ChannelPrograms
    {
      public string Name;
      public string externalId;
      //public ArrayList programs = new ArrayList();
      public readonly ProgramList programs = new ProgramList();
    } ;

    internal class ImportStats
    {
      // Totals over all files:
      public int TotalChannelCountFiles = 0;
      public int TotalProgramCountFiles = 0;
      public int TotalChannelCountFilesUnmapped = 0;
      public int TotalProgramCountFilesUnmapped = 0;
      public int TotalChannelCountDb = 0;
      public int TotalProgramCountDb = 0;

      // For the current file:
      public int FileChannelCount = 0;
      public int FileProgramCount = 0;
      public int FileChannelCountUnmapped = 0;
      public int FileProgramCountUnmapped = 0;
      public int FileChannelCountDb = 0;
      public int FileProgramCountDb = 0;

      public void ResetFileStats()
      {
        FileChannelCount = 0;
        FileProgramCount = 0;
        FileChannelCountUnmapped = 0;
        FileProgramCountUnmapped = 0;
        FileChannelCountDb = 0;
        FileProgramCountDb = 0;
      }

      public string GetTotalChannelDescription()
      {
        return string.Format("file = {0}, unmapped = {1}, DB = {2}", TotalChannelCountFiles, TotalChannelCountFilesUnmapped, TotalChannelCountDb);
      }
      public string GetTotalProgramDescription()
      {
        return string.Format("file = {0}, unmapped = {1}, DB = {2}", TotalProgramCountFiles, TotalProgramCountFilesUnmapped, TotalProgramCountDb);
      }
    }

    private int _backgroundDelay = 0;
    
    private static bool _isImporting = false;

    public XMLTVImport()
      : this(0) {}

    public XMLTVImport(int backgroundDelay)
    {
      _backgroundDelay = backgroundDelay;
    }

    private int ParseStarRating(string epgRating)
    {
      int Rating = -1;
      try
      {
        // format = 5.2/10
        // check if the epgRating is within a xml tag
        epgRating = epgRating.Trim();
        if (string.IsNullOrEmpty(epgRating))
          return Rating;

        if (epgRating.StartsWith("<"))
        {
          int endStartTagIdx = epgRating.IndexOf(">") + 1;
          int length = epgRating.IndexOf("</", endStartTagIdx) - endStartTagIdx;
          epgRating = epgRating.Substring(endStartTagIdx, length);
        }
        string strRating = epgRating;
        int slashPos = strRating.IndexOf('/');
        // Some EPG providers only supply the value without n/10
        if (slashPos > 0)
          strRating = strRating.Remove(slashPos);

        decimal tmpRating = -1;
        NumberFormatInfo NFO = NumberFormatInfo.InvariantInfo;
        NumberStyles NStyle = NumberStyles.Float;

        if (Decimal.TryParse(strRating, NStyle, NFO, out tmpRating))
          Rating = Convert.ToInt16(tmpRating);
        else
          this.LogInfo("XMLTVImport: star-rating could not be used - {0},({1})", epgRating, strRating);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "XMLTVImport: Error parsing star-rating - {0}", epgRating);
      }
      return Rating;
    }

    public bool Import(string fileName, bool deleteBeforeImport, ShowProgressHandler showProgress, bool useTimeCorrection, int timeCorrectionHours, int timeCorrectionMinutes, ref ImportStats stats)
    {
      this.LogDebug("XMLTV: import file \"{0}\"", fileName);
      //System.Diagnostics.Debugger.Launch();
      if (_isImporting == true)
      {
        this.LogWarn("XMLTV: attempted to run multiple imports simultaneously");
        showProgress("already importing", stats);
        return false;
      }
      _isImporting = true;

      try
      {
        // Make sure the file exists before we try to do any processing
        if (!File.Exists(fileName))
        {
          showProgress("data file not found", stats);
          this.LogError("XMLTV: data file \"{0}\" was not found", fileName);
          return false;
        }

        ///////////////////////////////////////////////////////////////////////////
        /*  design:
          * 1. create a Dictionary<string,Channel> using the externalid as the key,
          *    add all channels to this Dictionary 
          *    Note: guidechannel -> channel is a one-to-many relationship. 
          * 2. Read all programs from the xml file
          * 3. Create a program for each mapped channel
          */
        ///////////////////////////////////////////////////////////////////////////

        #region read the available guide channels and find the DB channels they're mapped to

        this.LogDebug("XMLTV: reading channels");
        showProgress("loading channel list", stats);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        IList<Channel> allDbChannelsWithGuideChannelMappings = ChannelManagement.GetAllChannelsWithExternalId();

        // DB channel ID => mapped guide channel programs
        var dbChannelPrograms = new Dictionary<int, ChannelPrograms>();
        // XMLTV IDs
        HashSet<string> unmappedGuideChannelIds = new HashSet<string>();
        // XMLTV ID => mapped DB channels
        Dictionary<string, List<Channel>> allMappedChannelsByGuideChannelId = new Dictionary<string, List<Channel>>();

        using (var xmlReader = new XmlTextReader(fileName))
        {
          xmlReader.DtdProcessing = DtdProcessing.Ignore;
          if (xmlReader.ReadToDescendant("tv") && xmlReader.ReadToDescendant("channel"))
          {
            int iChannel = 0;
            do
            {
              stats.TotalChannelCountFiles++;
              stats.FileChannelCount++;
              iChannel++;

              string id = xmlReader.GetAttribute("id");
              if (string.IsNullOrEmpty(id))
              {
                this.LogError("XMLTV: channel #{0} in file \"{1}\" doesnt contain an ID", iChannel, fileName);
                continue;
              }

              id = string.Format("xmltv|{0}|{1}", fileNameWithoutExtension, id);

              // a guide channel can be mapped to multiple DB channels
              bool isMapped = false;
              foreach (Channel dbChannel in allDbChannelsWithGuideChannelMappings)
              {
                if (!dbChannel.ExternalId.Equals(id))
                {
                  continue;
                }
                isMapped = true;

                List<Channel> dbChannelsMappedToGuideChannel;
                if (allMappedChannelsByGuideChannelId.TryGetValue(id, out dbChannelsMappedToGuideChannel))
                {
                  dbChannelsMappedToGuideChannel.Add(dbChannel);
                }
                else
                {
                  allMappedChannelsByGuideChannelId.Add(id, new List<Channel>(5) { dbChannel });
                }
                ChannelPrograms newProgChan = new ChannelPrograms();
                newProgChan.Name = dbChannel.DisplayName;
                newProgChan.externalId = dbChannel.ExternalId;
                dbChannelPrograms.Add(dbChannel.IdChannel, newProgChan);
                this.LogDebug("  channel #{0}, ID = {1}, name = {2}, DB ID = {3}", iChannel, dbChannel.ExternalId, dbChannel.DisplayName, dbChannel.IdChannel);
              }

              if (!isMapped)
              {
                stats.TotalChannelCountFilesUnmapped++;
                stats.FileChannelCountUnmapped++;
                unmappedGuideChannelIds.Add(id);
              }

            } while (xmlReader.ReadToNextSibling("channel"));

            showProgress("loading channel list", stats);
          }
        }

        #endregion

        #region read the available programme details and build a structure linking each programme to each mapped channel

        // Remove programs that have already shown from the DB.
        this.LogDebug("XMLTV: removing expired DB programs");
        showProgress("removing old programs", stats);
        ProgramManagement.DeleteOldPrograms();

        this.LogDebug("XMLTV: reading programmes");
        showProgress("loading programs", stats);

        int timeZoneCorrection = 0;
        if (useTimeCorrection)
        {
          timeZoneCorrection = (timeCorrectionHours * 60) + timeCorrectionMinutes;
        }

        IDictionary<string, ProgramCategory> dbCategories = new Dictionary<string, ProgramCategory>();
        foreach (var programCategory in ProgramCategoryManagement.ListAllProgramCategories())
        {
          dbCategories.Add(programCategory.Category, programCategory);
        }

        using (var xmlReader = new XmlTextReader(fileName))
        {
          xmlReader.DtdProcessing = DtdProcessing.Ignore;
          if (xmlReader.ReadToDescendant("tv") && xmlReader.ReadToDescendant("programme"))
          {
            do
            {
              stats.TotalProgramCountFiles++;
              stats.FileProgramCount++;
              if (stats.FileProgramCount % 100 == 0)
              {
                showProgress("loading programs", stats);
              }

              #region read programme node

              // Check for basic requirements.
              String nodeChannel = xmlReader.GetAttribute("channel");
              String nodeStart = xmlReader.GetAttribute("start");
              if (string.IsNullOrEmpty(nodeStart) || string.IsNullOrEmpty(nodeChannel))
              {
                this.LogWarn("XMLTV: found programme without valid channel ID or start time");
                continue;
              }

              // Check that the channel associated with this programme is
              // mapped to at least one DB channel. If not, there is no point
              // in parsing this program.
              nodeChannel = string.Format("xmltv|{0}|{1}", fileNameWithoutExtension, nodeChannel);
              if (unmappedGuideChannelIds.Contains(nodeChannel))
              {
                stats.TotalProgramCountFilesUnmapped++;
                stats.FileProgramCountUnmapped++;
                continue;
              }

              String nodeStop = xmlReader.GetAttribute("stop");
              String nodeTitle = null;
              String nodeDescription = null;
              String nodeEpisode = null;
              String nodeRepeat = null;
              String nodeEpisodeNum = null;
              String nodeEpisodeNumSystem = null;
              String nodeDate = null;
              String nodeStarRating = null;
              String nodeClassification = null;
              List<string> categories = new List<string>(10);
              List<ProgramCredit> credits = new List<ProgramCredit>(10);

              using (XmlReader xmlProg = xmlReader.ReadSubtree())
              {
                xmlProg.ReadStartElement(); // read programme
                // now, xmlProg is positioned on the first sub-element of <programme>
                while (!xmlProg.EOF)
                {
                  if (xmlProg.NodeType != XmlNodeType.Element)
                  {
                    xmlProg.Read();
                  }
                  switch (xmlProg.Name)
                  {
                    case "title":
                      if (nodeTitle == null)
                        nodeTitle = xmlProg.ReadString();
                      else
                        xmlProg.Skip();
                      break;
                    case "category":
                      categories.Add(xmlProg.ReadString());
                      break;
                    case "credits":
                      using (XmlReader nodeCredits = xmlProg.ReadSubtree())
                      {
                        credits.AddRange(ParseCredits(nodeCredits));
                      }
                      break;
                    case "desc":
                      if (nodeDescription == null)
                        nodeDescription = xmlProg.ReadString();
                      else
                        xmlProg.Skip();
                      break;
                    case "sub-title":
                      if (nodeEpisode == null)
                        nodeEpisode = xmlProg.ReadString();
                      else
                        xmlProg.Skip();
                      break;
                    case "previously-shown":
                      if (nodeRepeat == null)
                        nodeRepeat = xmlProg.ReadString();
                      else
                        xmlProg.Skip();
                      break;
                    case "episode-num":
                      if (nodeEpisodeNum == null)
                      {
                        nodeEpisodeNumSystem = xmlProg.GetAttribute("system");
                        nodeEpisodeNum = xmlProg.ReadString();
                      }
                      else
                        xmlProg.Skip();
                      break;
                    case "date":
                      if (nodeDate == null)
                        nodeDate = xmlProg.ReadString();
                      else
                        xmlProg.Skip();
                      break;
                    case "star-rating":
                      if (nodeStarRating == null)
                        nodeStarRating = xmlProg.ReadInnerXml();
                      else
                        xmlProg.Skip();
                      break;
                    case "rating":
                      if (nodeClassification == null)
                        nodeClassification = xmlProg.ReadInnerXml();
                      else
                        xmlProg.Skip();
                      break;
                    default:
                      // unknown, skip entire node
                      xmlProg.Skip();
                      break;
                  }
                }
              }

              #endregion

              #region verify/convert programme properties

              string description = "";
              int idCategory = -1;
              string serEpNum = "";
              string date = "";
              string seriesNum = "";
              string episodeNum = "";
              string episodeName = "";
              string episodePart = "";
              int starRating = -1;
              string classification = "";
              bool isRepeat = false;

              string title = ReplaceLineBreaks(ConvertHTMLToAnsi(nodeTitle));

              long startDate = 0;
              if (nodeStart.Length >= 14)
              {
                if (Char.IsDigit(nodeStart[12]) && Char.IsDigit(nodeStart[13]))
                  startDate = Int64.Parse(nodeStart.Substring(0, 14)); //20040331222000
                else
                  startDate = 100 * Int64.Parse(nodeStart.Substring(0, 12)); //200403312220
              }
              else if (nodeStart.Length >= 12)
              {
                startDate = 100 * Int64.Parse(nodeStart.Substring(0, 12)); //200403312220
              }

              long stopDate = startDate;
              if (nodeStop != null)
              {
                if (nodeStop.Length >= 14)
                {
                  if (Char.IsDigit(nodeStop[12]) && Char.IsDigit(nodeStop[13]))
                    stopDate = Int64.Parse(nodeStop.Substring(0, 14)); //20040331222000
                  else
                    stopDate = 100 * Int64.Parse(nodeStop.Substring(0, 12)); //200403312220
                }
                else if (nodeStop.Length >= 12)
                {
                  stopDate = 100 * Int64.Parse(nodeStop.Substring(0, 12)); //200403312220
                }
              }

              startDate = CorrectIllegalDateTime(startDate);
              stopDate = CorrectIllegalDateTime(stopDate);
              string timeZoneStart = "";
              string timeZoneEnd = "";
              if (nodeStart.Length > 14)
              {
                timeZoneStart = nodeStart.Substring(14);
                timeZoneStart = timeZoneStart.Trim();
                timeZoneEnd = timeZoneStart;
              }
              if (nodeStop != null && nodeStop.Length > 14)
              {
                timeZoneEnd = nodeStop.Substring(14);
                timeZoneEnd = timeZoneEnd.Trim();
              }

              //
              // time compensation and correction/adjustment
              //
              // The compensation automatically adjusts for daylight
              // savings and/or location. To work correctly, it relies on
              // accurate Windows date/time settings and the time offset
              // specified in the XMLTV file itself.
              //
              // Manual correction should be unnecessary unless the two
              // conditions above can't be met.
              DateTime dateTimeStart = longtodate(startDate);
              dateTimeStart = ConvertToLocalTime(dateTimeStart, timeZoneStart);   // compensation
              dateTimeStart = dateTimeStart.AddMinutes(timeZoneCorrection);       // correction

              DateTime dateTimeEnd;
              if (nodeStop != null)
              {
                dateTimeEnd = longtodate(stopDate);
              }
              else
              {
                dateTimeEnd = longtodate(startDate);
              }
              dateTimeEnd = ConvertToLocalTime(dateTimeEnd, timeZoneEnd);
              dateTimeEnd = dateTimeEnd.AddMinutes(timeZoneCorrection);

              foreach (string c in categories)
              {
                // If this is a new category, add it to the DB.
                ProgramCategory dbProgramCategory;
                if (!dbCategories.TryGetValue(c, out dbProgramCategory))
                {
                  dbProgramCategory = new ProgramCategory { Category = c };
                  ProgramCategoryManagement.AddCategory(dbProgramCategory);
                  dbCategories[c] = dbProgramCategory;
                }

                if (idCategory <= 0)
                {
                  idCategory = dbProgramCategory.IdProgramCategory;
                }
              }

              if (nodeDescription != null)
              {
                description = ReplaceLineBreaks(ConvertHTMLToAnsi(nodeDescription));
              }
              if (nodeEpisode != null)
              {
                episodeName = ReplaceLineBreaks(ConvertHTMLToAnsi(nodeEpisode));
                if (title.Length == 0)
                  title = episodeName;
              }

              if (nodeEpisodeNum != null)
              {
                // http://xml.coverpages.org/XMLTV-DTD-20021210.html
                if (nodeEpisodeNumSystem != null && nodeEpisodeNumSystem == "xmltv_ns")
                {
                  serEpNum = ConvertHTMLToAnsi(nodeEpisodeNum.Replace(" ", ""));
                  int dot1 = serEpNum.IndexOf(".", 0);
                  int dot2 = serEpNum.IndexOf(".", dot1 + 1);
                  seriesNum = serEpNum.Substring(0, dot1);
                  episodeNum = serEpNum.Substring(dot1 + 1, dot2 - (dot1 + 1));
                  episodePart = serEpNum.Substring(dot2 + 1, serEpNum.Length - (dot2 + 1));
                  //xmltv_ns is theorically zero-based number will be increased by one
                  seriesNum = CorrectEpisodeNum(seriesNum, 1);
                  episodeNum = CorrectEpisodeNum(episodeNum, 1);
                  episodePart = CorrectEpisodeNum(episodePart, 1);
                }
                else if (nodeEpisodeNumSystem != null && nodeEpisodeNumSystem == "onscreen")
                {
                  // example: 'Episode #FFEE' 
                  serEpNum = ConvertHTMLToAnsi(nodeEpisodeNum);
                  int num1 = serEpNum.IndexOf("#", 0);
                  if (num1 < 0)
                    num1 = 0;
                  episodeNum = CorrectEpisodeNum(serEpNum.Substring(num1, serEpNum.Length - num1), 0);
                }

                // non-standard system
                else
                {
                  serEpNum = ConvertHTMLToAnsi(nodeEpisodeNum.Replace(" ", ""));
                  episodeNum = CorrectEpisodeNum(serEpNum, 0);
                }
              }

              if (nodeDate != null)
              {
                date = nodeDate;
              }

              isRepeat = (nodeRepeat != null);

              if (nodeStarRating != null)
              {
                starRating = ParseStarRating(nodeStarRating);
              }

              if (nodeClassification != null)
              {
                classification = nodeClassification;
              }

              #endregion

              #region create a copy of the program for each mapped channel

              List<Channel> mappedChannels;
              if (allMappedChannelsByGuideChannelId.TryGetValue(nodeChannel, out mappedChannels) && mappedChannels != null && mappedChannels.Count > 0)
              {
                foreach (Channel chan in mappedChannels)
                {
                  // get the channel program
                  ChannelPrograms channelPrograms;
                  if (!dbChannelPrograms.TryGetValue(chan.IdChannel, out channelPrograms))
                  {
                    continue;
                  }

                  var prg = new Program();
                  prg.IdChannel = chan.IdChannel;
                  prg.StartTime = dateTimeStart;
                  prg.EndTime = dateTimeEnd;
                  prg.Title = title;
                  prg.Description = description;
                  prg.State = (int)ProgramState.None;
                  prg.OriginalAirDate = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
                  prg.SeriesNum = seriesNum;
                  prg.EpisodeNum = episodeNum;
                  prg.EpisodeName = episodeName;
                  prg.EpisodePart = episodePart;
                  prg.StarRating = starRating;
                  prg.Classification = classification;
                  prg.ParentalRating = -1;
                  prg.PreviouslyShown = isRepeat;
                  foreach (var credit in credits)
                  {
                    prg.ProgramCredits.Add(credit);
                  }
                  if (idCategory >= 0)
                  {
                    prg.IdProgramCategory = idCategory;
                  }

                  channelPrograms.programs.Add(prg);
                }
              }

              #endregion

              // get the next programme
            } while (xmlReader.ReadToNextSibling("programme"));
          }
        }

        #endregion

        #region sort and remove invalid programmes, then save all valid programmes

        this.LogDebug("XMLTV: sorting and filtering programs");
        showProgress("sorting programs", stats);
        DateTime dtStartDate = DateTime.Today;

        foreach (ChannelPrograms progChan in dbChannelPrograms.Values)
        {
          if (progChan.programs.Count == 0)
          {
            continue;
          }

          // Be sure that we don't have any overlapping programs within the
          // set of programs we're about to import.
          progChan.programs.Sort();
          progChan.programs.AlreadySorted = true;
          progChan.programs.FixEndTimes();
          progChan.programs.RemoveOverlappingPrograms();

          // Don't import programs which have already ended.
          for (int i = 0; i < progChan.programs.Count; ++i)
          {
            var prog = progChan.programs[i];
            if (prog.EndTime <= dtStartDate)
            {
              progChan.programs.RemoveAt(i);
              i--;
            }
          }

          stats.TotalChannelCountDb++;
          stats.FileChannelCountDb++;
          stats.TotalProgramCountDb += progChan.programs.Count;
          stats.FileProgramCountDb += progChan.programs.Count;
          showProgress("storing programs", stats);
          this.LogInfo("XMLTV: inserting {0} programs for {1}", progChan.programs.Count, progChan.Name);
          _programManagement.InsertPrograms(progChan.programs,
                                deleteBeforeImport
                                  ? DeleteBeforeImportOption.ProgramsOnSameChannel
                                  : DeleteBeforeImportOption.OverlappingPrograms, ThreadPriority.BelowNormal);
        }

        #endregion

        this.LogDebug("XMLTV: file import completed");
        return true;
      }
      catch (Exception ex)
      {
        showProgress("invalid data content/format, check error log for details", stats);
        this.LogError(ex, "XMLTV: failed to import file \"{0}\"", fileName);
      }
      finally
      {
        _isImporting = false;        
      }
      return false;
    }

    private DateTime ConvertToLocalTime(DateTime dateTime, string timeZone)
    {
      int off = GetTimeOffset(timeZone);
      int h = off / 100; // 220 -> 2,  -220 -> -2
      int m = off - (h * 100); // 220 -> 20, -220 -> -20

      dateTime = dateTime.AddHours(-h);
      dateTime = dateTime.AddMinutes(-m);
      return dateTime.ToLocalTime();
    }

    private static List<ProgramCredit> ParseCredits(XmlReader nodeCredits)
    {
      var programcredits = new List<ProgramCredit>();

      using (nodeCredits)
      {
        nodeCredits.ReadStartElement();
        while (!nodeCredits.EOF)
        {
          if (nodeCredits.NodeType == XmlNodeType.Element)
          {
            string creditRole = nodeCredits.Name;
            if (creditRole.Length > 50)
            {
              creditRole = creditRole.Substring(0, 50);  
            }
            string creditPerson = nodeCredits.ReadString();
            if (creditPerson.Length > 200)
            {
              creditPerson = creditPerson.Substring(0, 200);
            }
            var credit = new ProgramCredit {Role = creditRole, Person = creditPerson};
            programcredits.Add(credit);
          }
          nodeCredits.Read();
        }
      }
      return programcredits;
    }

    /// <summary>
    /// Parse and correct ep. # in the episode string
    /// </summary>
    /// <param name="episodenum"></param>
    /// <param name="nodeEpisodeNumSystemBase">int to add to the parsed episode num (depends on 0-based or not xmltv files)</param>
    /// <returns></returns>
    private string CorrectEpisodeNum(string episodenum, int nodeEpisodeNumSystemBase)
    {
      if (episodenum == "")
        return episodenum;

      // Find format of the episode number
      int slashpos = episodenum.IndexOf("/", 0);
      if (slashpos == -1)
      {
        // No slash found => assume it's just a plain number
        try
        {
          int epnum = Convert.ToInt32(episodenum);
          return Convert.ToString(epnum + nodeEpisodeNumSystemBase);
        }
        catch (Exception)
        {
          this.LogDebug("XMLTVImport::CorrectEpisodeNum, could not parse '{0}' as plain number", episodenum);
        }
      }
      else
      {
        try
        {
          // Slash found -> assume it's formatted as <episode number>/<episodes>
          int epnum = Convert.ToInt32(episodenum.Substring(0, slashpos));
          int epcount = Convert.ToInt32(episodenum.Substring(slashpos + 1));
          return Convert.ToString(epnum + nodeEpisodeNumSystemBase) + "/" + Convert.ToString(epcount);
        }
        catch (Exception)
        {
          this.LogDebug("XMLTVImport::CorrectEpisodeNum, could not parse '{0}' as episode/episodes", episodenum);
        }
      }
      return "";
    }

    private int GetTimeOffset(string timeZone)
    {
      // timezone can b in format:
      // GMT +0100 or GMT -0500
      // or just +0300
      if (timeZone.Length == 0) return 0;
      timeZone = timeZone.ToLowerInvariant();

      // just ignore GMT offsets, since we're calculating everything from GMT anyway
      if (timeZone.IndexOf("gmt") >= 0)
      {
        int ipos = timeZone.IndexOf("gmt");
        timeZone = timeZone.Substring(ipos + "GMT".Length);
      }

      timeZone = timeZone.Trim();
      if (timeZone[0] == '+' || timeZone[0] == '-')
      {
        string strOff = timeZone.Substring(1);
        try
        {
          int iOff = Int32.Parse(strOff);
          if (timeZone[0] == '-') return -iOff;
          else return iOff;
        }
        catch (Exception) {}
      }
      return 0;
    }

    private long CorrectIllegalDateTime(long datetime)
    {
      //format : 20050710245500
      long sec = datetime % 100;
      datetime /= 100;
      long min = datetime % 100;
      datetime /= 100;
      long hour = datetime % 100;
      datetime /= 100;
      long day = datetime % 100;
      datetime /= 100;
      long month = datetime % 100;
      datetime /= 100;
      long year = datetime;
      DateTime dt = new DateTime((int)year, (int)month, (int)day, 0, 0, 0);
      dt = dt.AddHours(hour);
      dt = dt.AddMinutes(min);
      dt = dt.AddSeconds(sec);


      long newDateTime = datetolong(dt);
      if (sec < 0 || sec > 59 ||
          min < 0 || min > 59 ||
          hour < 0 || hour >= 24 ||
          day < 0 || day > 31 ||
          month < 0 || month > 12)
      {
        //this.LogDebug(LogType.EPG, true, "epg-import:tvguide.xml contains invalid date/time :{0} converted it to:{1}",
        //              orgDateTime, newDateTime);
      }

      return newDateTime;
    }

    public long datetolong(DateTime dt)
    {
      try
      {
        long iSec = 0; //(long)dt.Second;
        long iMin = (long)dt.Minute;
        long iHour = (long)dt.Hour;
        long iDay = (long)dt.Day;
        long iMonth = (long)dt.Month;
        long iYear = (long)dt.Year;

        long lRet = (iYear);
        lRet = lRet * 100L + iMonth;
        lRet = lRet * 100L + iDay;
        lRet = lRet * 100L + iHour;
        lRet = lRet * 100L + iMin;
        lRet = lRet * 100L + iSec;
        return lRet;
      }
      catch (Exception) {}
      return 0;
    }

    public DateTime longtodate(long ldate)
    {
      try
      {
        if (ldate < 0) return DateTime.MinValue;
        ldate /= 100L;
        int minute = (int)(ldate % 100L);
        ldate /= 100L;
        int hour = (int)(ldate % 100L);
        ldate /= 100L;
        int day = (int)(ldate % 100L);
        ldate /= 100L;
        int month = (int)(ldate % 100L);
        ldate /= 100L;
        int year = (int)ldate;
        DateTime dt = new DateTime(year, month, day, hour, minute, 0, 0);
        return dt;
      }
      catch (Exception) {}
      return DateTime.Now;
    }

    private static string ConvertHTMLToAnsi(string html)
    {
      string strippedHtml = string.Empty;
      if (!string.IsNullOrEmpty(html))
      {
        using (var writer = new StringWriter())
        {
          System.Web.HttpUtility.HtmlDecode(html, writer);
          strippedHtml = writer.ToString().Replace("<br>", "\n");
        }
      }
      return strippedHtml;
    }

    private static string ReplaceLineBreaks(string s)
    {
      if (!string.IsNullOrEmpty(s))
      {
        s = s.Replace("\r\n", " ");
        s = s.Replace("\n\r", " ");
        s = s.Replace("\r", " ");
        s = s.Replace("\n", " ");
        s = s.Replace("  ", " ");
      }
      return s;
    }

    #region Sort Members

    public int Compare(object x, object y)
    {
      if (x == y) return 0;
      Program item1 = (Program)x;
      Program item2 = (Program)y;
      if (item1 == null) return -1;
      if (item2 == null) return -1;

      if (item1.IdChannel != item2.IdChannel)
      {
        return String.Compare(item1.Channel.DisplayName, item2.Channel.DisplayName, true);
      }
      if (item1.StartTime > item2.StartTime) return 1;
      if (item1.StartTime < item2.StartTime) return -1;
      return 0;
    }

    #endregion
  }
}