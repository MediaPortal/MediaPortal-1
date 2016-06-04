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
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.Localisation;

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  internal class Importer
  {
    private static readonly Regex REGEX_COMMON_SEASON_EPISODE_FORMAT = new Regex(@"^S(\d+)E(\d+)$");

    private bool _isImportRunning = false;
    private bool _isImportCancelled = false;

    public delegate void ShowProgressHandler(string status, ImportStats stats);

    private class MappedChannel
    {
      public int ChannelId;
      public string ChannelName;
      public readonly ProgramList Programs = new ProgramList();
    }

    public bool Import(string fileName, ShowProgressHandler showProgress, int timeCorrectionMinutes, ref ImportStats stats)
    {
      this.LogInfo("XMLTV import: import file \"{0}\"", fileName);
      _isImportRunning = true;
      _isImportCancelled = false;

      try
      {
        // Make sure the file exists before we try to do any processing
        if (!File.Exists(fileName))
        {
          showProgress("data file not found", stats);
          this.LogError("XMLTV import: data file \"{0}\" was not found", fileName);
          return false;
        }

        ///////////////////////////////////////////////////////////////////////
        // design:
        // 1. Create a a dictionary mapping guide channels to DB channels.
        //    Each guide channel can be mapped to one or more DB channels.
        // 2. Read all programs from the XMLTV file.
        // 3. Create a program for each mapped channel.
        ///////////////////////////////////////////////////////////////////////

        #region read the available guide channels and find the DB channels they're mapped to

        this.LogDebug("XMLTV import: reading channels");
        showProgress("loading channel list", stats);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        IList<Channel> tempChannels = ChannelManagement.GetAllChannelsWithExternalId();
        IList<Channel> allDbChannelsWithGuideChannelMappings = new List<Channel>(tempChannels.Count);
        foreach (Channel channel in tempChannels)
        {
          if (XmlTvImportId.HasXmlTvMapping(channel.ExternalId))
          {
            allDbChannelsWithGuideChannelMappings.Add(channel);
          }
        }

        // XMLTV ID => mapped channels (programs etc.)
        Dictionary<string, IList<MappedChannel>> allMappedChannelsByGuideChannelId = new Dictionary<string, IList<MappedChannel>>();

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
                this.LogError("XMLTV import: channel #{0} in file \"{1}\" doesn't contain an ID", iChannel, fileName);
                continue;
              }

              string xmlTvChannelId1 = id;
              id = XmlTvImportId.GetQualifiedIdForChannel(fileNameWithoutExtension, id);

              // a guide channel can be mapped to more than one DB channel
              bool isMapped = false;
              foreach (Channel dbChannel in allDbChannelsWithGuideChannelMappings)
              {
                if (!dbChannel.ExternalId.Equals(id))
                {
                  string tempFileName;
                  string xmlTvChannelId2;
                  XmlTvImportId.GetQualifiedIdComponents(dbChannel.ExternalId, out tempFileName, out xmlTvChannelId2);
                  if (!string.Equals(tempFileName, fileNameWithoutExtension))
                  {
                    continue;
                  }

                  if (Mc2XmlId.IsMatch(xmlTvChannelId1, xmlTvChannelId2))
                  {
                    this.LogDebug("XMLTV import: fixing mc2xml mapping, original ID = {0}, new ID = {1}", dbChannel.ExternalId, id);
                    dbChannel.ExternalId = id;
                    ChannelManagement.SaveChannel(dbChannel);
                  }
                  else
                  {
                    continue;
                  }
                }
                isMapped = true;

                MappedChannel mappedChannel = new MappedChannel();
                mappedChannel.ChannelId = dbChannel.IdChannel;
                mappedChannel.ChannelName = dbChannel.Name;

                IList<MappedChannel> dbChannelsMappedToGuideChannel;
                if (!allMappedChannelsByGuideChannelId.TryGetValue(id, out dbChannelsMappedToGuideChannel))
                {
                  dbChannelsMappedToGuideChannel = new List<MappedChannel>(5);
                  allMappedChannelsByGuideChannelId.Add(id, dbChannelsMappedToGuideChannel);
                }
                dbChannelsMappedToGuideChannel.Add(mappedChannel);

                this.LogDebug("  channel #{0}, ID = {1}, name = {2}, DB ID = {3}", iChannel, dbChannel.ExternalId, dbChannel.Name, dbChannel.IdChannel);
              }

              if (!isMapped)
              {
                stats.TotalChannelCountFilesUnmapped++;
                stats.FileChannelCountUnmapped++;
              }

            } while (xmlReader.ReadToNextSibling("channel"));

            showProgress("loading channel list", stats);
          }
        }

        #endregion

        #region read the available programme details and build a structure linking each programme to each mapped channel

        if (_isImportCancelled)
        {
          showProgress("import cancelled", stats);
          return true;
        }

        // Remove programs that have already shown from the DB.
        this.LogDebug("XMLTV import: removing expired DB programs");
        showProgress("removing old programs", stats);
        ProgramManagement.DeleteOldPrograms();

        if (_isImportCancelled)
        {
          showProgress("import cancelled", stats);
          return true;
        }

        this.LogDebug("XMLTV import: reading programmes");
        showProgress("loading programs", stats);

        List<string> preferredLanguageCodes = new List<string>(SettingsManagement.GetValue("epgPreferredLanguages", string.Empty).Split('|'));
        List<string> preferredClassificationSystems = new List<string>(SettingsManagement.GetValue("epgPreferredClassificationSystems", string.Empty).Split('|'));
        List<string> preferredRatingSystems = new List<string>(SettingsManagement.GetValue("epgPreferredRatingSystems", string.Empty).Split('|'));
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
                if (_isImportCancelled)
                {
                  showProgress("import cancelled", stats);
                  return true;
                }
                showProgress("loading programs", stats);
              }

              // Check for basic requirements.
              string nodeChannel = xmlReader.GetAttribute("channel");
              string nodeStart = xmlReader.GetAttribute("start");
              if (string.IsNullOrEmpty(nodeStart) || string.IsNullOrEmpty(nodeChannel))
              {
                this.LogWarn("XMLTV import: found programme without valid channel ID or start time");
                continue;
              }

              // Check that the channel associated with this programme is
              // mapped to at least one DB channel. If not, there is no point
              // in parsing this program.
              nodeChannel = XmlTvImportId.GetQualifiedIdForChannel(fileNameWithoutExtension, nodeChannel);
              IList<MappedChannel> mappedChannels;
              if (!allMappedChannelsByGuideChannelId.TryGetValue(nodeChannel, out mappedChannels) || mappedChannels == null || mappedChannels.Count == 0)
              {
                stats.TotalProgramCountFilesUnmapped++;
                stats.FileProgramCountUnmapped++;
                continue;
              }

              DateTime start = ParseDateTime(nodeStart);
              if (start == DateTime.MinValue)   // invalid
              {
                continue;
              }
              // Manual correction should be unnecessary unless the two conditions
              // for compensation can't be met.
              start = start.AddMinutes(timeCorrectionMinutes);

              DateTime stop = start;
              string nodeStop = xmlReader.GetAttribute("stop");
              if (nodeStop != null)
              {
                stop = ParseDateTime(nodeStop);
                if (stop == DateTime.MinValue)
                {
                  stop = start;
                }
                else
                {
                  stop = stop.AddMinutes(timeCorrectionMinutes);
                }
              }

              using (XmlReader xmlProg = xmlReader.ReadSubtree())
              {
                ParseProgramme(xmlProg, start, stop, preferredLanguageCodes, preferredClassificationSystems, preferredRatingSystems, dbCategories, mappedChannels);
              }

              // get the next programme
            } while (xmlReader.ReadToNextSibling("programme"));
          }
        }

        #endregion

        #region sort and remove invalid programmes, then save all valid programmes

        this.LogDebug("XMLTV import: sorting and filtering programs");
        showProgress("sorting programs", stats);
        DateTime dtStartDate = DateTime.Today;

        foreach (IList<MappedChannel> mappedChannelSet in allMappedChannelsByGuideChannelId.Values)
        {
          foreach (MappedChannel mappedChannel in mappedChannelSet)
          {
            if (mappedChannel.Programs.Count == 0)
            {
              continue;
            }

            // Be sure that we don't have any overlapping programs within the
            // set of programs we're about to import.
            mappedChannel.Programs.SortIfNeeded();
            mappedChannel.Programs.FixEndTimes();
            mappedChannel.Programs.RemoveOverlappingPrograms();

            // Don't import programs which have already ended.
            for (int i = 0; i < mappedChannel.Programs.Count; ++i)
            {
              var prog = mappedChannel.Programs[i];
              if (prog.EndTime <= dtStartDate)
              {
                mappedChannel.Programs.RemoveAt(i);
                i--;
              }
            }

            if (_isImportCancelled)
            {
              showProgress("import cancelled", stats);
              return true;
            }

            stats.TotalChannelCountDb++;
            stats.FileChannelCountDb++;
            stats.TotalProgramCountDb += mappedChannel.Programs.Count;
            stats.FileProgramCountDb += mappedChannel.Programs.Count;
            showProgress("storing programs", stats);
            this.LogInfo("XMLTV import: inserting {0} programs for {1}", mappedChannel.Programs.Count, mappedChannel.ChannelName);
            ProgramManagement.InsertPrograms(mappedChannel.Programs, EpgDeleteBeforeImportOption.ProgramsOnSameChannel, ThreadPriority.BelowNormal);
          }
        }

        #endregion

        this.LogDebug("XMLTV import: file import completed");
        return true;
      }
      catch (Exception ex)
      {
        showProgress("invalid data content/format, check error log for details", stats);
        this.LogError(ex, "XMLTV import: failed to import file \"{0}\"", fileName);
      }
      finally
      {
        _isImportRunning = false;
      }

      return false;
    }

    public void CancelImport()
    {
      if (_isImportRunning)
      {
        this.LogInfo("XMLTV import: cancelling import...");
      }
      _isImportCancelled = true;
    }

    private static void ParseProgramme(XmlReader xmlProg, DateTime startDateTime, DateTime endDateTime,
                                        List<string> preferredLanguageCodes, List<string> preferredClassificationSystems, List<string> preferredRatingSystems,
                                        IDictionary<string, ProgramCategory> dbCategories, IList<MappedChannel> mappedChannels)
    {
      string title = null;
      int priorityTitle = -1;
      string episodeName = null;
      int priorityEpisodeName = -1;
      string description = null;
      int priorityDescription = -1;
      List<ProgramCredit> credits = new List<ProgramCredit>(10);
      int productionYear = -1;
      List<string> categories = new List<string>(10);
      List<string> audioLanguages = new List<string>(10);
      List<string> countries = new List<string>(5);
      int seasonNumber = -1;
      int episodeNumber = -1;
      int episodePartNumber = -1;
      string seriesId = null;
      string episodeId = null;
      bool? isHighDefinition = null;
      bool? isPreviouslyShown = null;
      List<string> subtitlesLanguages = new List<string>(10);
      string classification = null;
      int priorityClassification = -1;
      decimal starRating = -1;
      decimal starRatingMaximum = -1;
      int priorityStarRating = -1;

      // Read to get to the first sub-element of <programme>.
      xmlProg.ReadStartElement();
      while (!xmlProg.EOF)
      {
        if (xmlProg.NodeType != XmlNodeType.Element)
        {
          xmlProg.Read();
        }
        int languagePriority = -1;
        string language = xmlProg.GetAttribute("lang");
        if (!string.IsNullOrEmpty(language))
        {
          languagePriority = preferredLanguageCodes.IndexOf(language);
        }
        switch (xmlProg.Name)
        {
          case "title":
            if (string.IsNullOrEmpty(title) || (languagePriority >= 0 && (priorityTitle < 0 || (priorityTitle >= 0 && languagePriority < priorityTitle))))
            {
              title = TidyString(ConvertHtmlToAnsi(xmlProg.ReadString()));
              if (!string.IsNullOrEmpty(title))
              {
                priorityTitle = languagePriority;
              }
            }
            else
            {
              xmlProg.Skip();
            }
            break;
          case "sub-title":
            if (string.IsNullOrEmpty(episodeName) || (languagePriority >= 0 && (priorityEpisodeName < 0 || (priorityEpisodeName >= 0 && languagePriority < priorityEpisodeName))))
            {
              episodeName = TidyString(ConvertHtmlToAnsi(xmlProg.ReadString()));
              if (!string.IsNullOrEmpty(episodeName))
              {
                priorityEpisodeName = languagePriority;
              }
            }
            else
            {
              xmlProg.Skip();
            }
            break;
          case "desc":
            if (string.IsNullOrEmpty(description) || (languagePriority >= 0 && (priorityDescription < 0 || (priorityDescription >= 0 && languagePriority < priorityDescription))))
            {
              description = TidyString(ConvertHtmlToAnsi(xmlProg.ReadString()));
              if (!string.IsNullOrEmpty(description))
              {
                priorityDescription = languagePriority;
              }
            }
            else
            {
              xmlProg.Skip();
            }
            break;
          case "credits":
            using (XmlReader nodeCredits = xmlProg.ReadSubtree())
            {
              credits.AddRange(ParseCredits(nodeCredits));
            }
            break;
          case "date":
            if (productionYear < 0)
            {
              string productionDate = xmlProg.ReadString();
              if (productionDate.Length >= 4 && !int.TryParse(productionDate.Substring(0, 4), out productionYear))
              {
                productionYear = -1;
              }
            }
            else
            {
              xmlProg.Skip();
            }
            break;
          case "category":
            categories.Add(xmlProg.ReadString());
            break;
          case "language":
            string languageCode = ParseLanguage(xmlProg.ReadString());
            if (!audioLanguages.Contains(languageCode))
            {
              audioLanguages.Add(languageCode);
            }
            break;
          case "country":
            countries.Add(xmlProg.ReadString());
            break;
          case "episode-num":
            int tempSeasonNumber;
            int tempEpisodeNumber;
            int tempEpisodePartNumber;
            string tempSeriesId;
            string tempEpisodeId;
            ParseEpisodeNumber(xmlProg.GetAttribute("system"),
                                xmlProg.ReadString(),
                                out tempSeasonNumber,
                                out tempEpisodeNumber,
                                out tempEpisodePartNumber,
                                out tempSeriesId,
                                out tempEpisodeId);
            if ((seasonNumber < 0 || episodeNumber < 0) && (tempSeasonNumber > 0 || tempEpisodeNumber > 0))
            {
              seasonNumber = tempSeasonNumber;
              episodeNumber = tempEpisodeNumber;
            }
            else if (string.IsNullOrEmpty(episodeId) && !string.IsNullOrEmpty(tempEpisodeId))
            {
              seriesId = tempSeriesId;
              episodeId = tempEpisodeId;
            }
            break;
          case "video":
            using (XmlReader nodeVideo = xmlProg.ReadSubtree())
            {
              isHighDefinition = ParseVideoDetails(nodeVideo);
            }
            break;
          case "previously-shown":
            isPreviouslyShown = true;
            xmlProg.Skip();
            break;
          case "new":
            isPreviouslyShown = false;
            xmlProg.Skip();
            break;
          case "subtitles":
            string subtitlesLanguage = ExtractValueWithOptionalElement(xmlProg.ReadInnerXml(), "language");
            if (string.IsNullOrEmpty(subtitlesLanguage))
            {
              subtitlesLanguage = "und";
            }
            else
            {
              subtitlesLanguage = ParseLanguage(subtitlesLanguage);
            }
            if (!subtitlesLanguages.Contains(subtitlesLanguage))
            {
              subtitlesLanguages.Add(subtitlesLanguage);
            }
            break;
          case "rating":
            int classificationSystemPriority = -1;
            string classificationSystem = xmlProg.GetAttribute("system");
            if (!string.IsNullOrEmpty(classificationSystem))
            {
              classificationSystemPriority = preferredClassificationSystems.IndexOf(classificationSystem);
            }
            if (string.IsNullOrEmpty(classification) || (classificationSystemPriority >= 0 && (priorityClassification < 0 || (priorityClassification >= 0 && classificationSystemPriority < priorityClassification))))
            {
              classification = ExtractValueWithOptionalElement(xmlProg.ReadInnerXml(), "value");
              if (!string.IsNullOrEmpty(classification))
              {
                priorityClassification = classificationSystemPriority;
              }
            }
            else
            {
              xmlProg.Skip();
            }
            break;
          case "star-rating":
            int ratingSystemPriority = -1;
            string ratingSystem = xmlProg.GetAttribute("system");
            if (!string.IsNullOrEmpty(ratingSystem))
            {
              ratingSystemPriority = preferredRatingSystems.IndexOf(ratingSystem);
            }
            if (starRating < 0 || (ratingSystemPriority >= 0 && (priorityStarRating < 0 || (priorityStarRating >= 0 && ratingSystemPriority < priorityStarRating))))
            {
              ParseStarRating(xmlProg.ReadInnerXml(), out starRating, out starRatingMaximum);
              if (starRating >= 0)
              {
                priorityStarRating = ratingSystemPriority;
              }
            }
            else
            {
              xmlProg.Skip();
            }
            break;
          default:
            // unknown/unsupported, skip entire node
            xmlProg.Skip();
            break;
        }
      }

      int idCategory = -1;
      bool isCategoryMapped = false;
      foreach (string c in categories)
      {
        // If this is a new category, add it to the DB.
        ProgramCategory dbProgramCategory;
        if (!dbCategories.TryGetValue(c, out dbProgramCategory))
        {
          dbProgramCategory = new ProgramCategory { Category = c };
          dbProgramCategory = ProgramCategoryManagement.AddCategory(dbProgramCategory);
          dbCategories[c] = dbProgramCategory;
        }

        // We can only link the program to one category. Prefer a category that is mapped to a guide category.
        if (idCategory <= 0 || (!isCategoryMapped && dbProgramCategory.IdGuideCategory.HasValue))
        {
          idCategory = dbProgramCategory.IdProgramCategory;
          isCategoryMapped = dbProgramCategory.IdGuideCategory.HasValue;
        }
      }

      foreach (MappedChannel mappedChannel in mappedChannels)
      {
        var program = ProgramFactory.CreateProgram(mappedChannel.ChannelId, startDateTime, endDateTime, title ?? string.Empty);
        if (!string.IsNullOrEmpty(description))
        {
          program.Description = description;
        }
        if (!string.IsNullOrEmpty(episodeName))
        {
          program.EpisodeName = episodeName;
        }
        if (!string.IsNullOrEmpty(seriesId))
        {
          program.SeriesId = seriesId;
        }
        if (seasonNumber > 0)
        {
          program.SeasonNumber = seasonNumber;
        }
        if (!string.IsNullOrEmpty(episodeId))
        {
          program.EpisodeId = episodeId;
        }
        if (episodeNumber > 0)
        {
          program.EpisodeNumber = episodeNumber;
        }
        if (episodePartNumber > 0)
        {
          program.EpisodePartNumber = episodePartNumber;
        }
        if (isPreviouslyShown.HasValue)
        {
          program.IsPreviouslyShown = isPreviouslyShown.Value;
        }
        if (!string.IsNullOrEmpty(classification))
        {
          program.Classification = classification;
        }
        if (isHighDefinition.HasValue)
        {
          program.IsHighDefinition = isHighDefinition.Value;
        }
        if (audioLanguages != null && audioLanguages.Count > 0)
        {
          program.AudioLanguages = string.Join(",", audioLanguages);
        }
        if (subtitlesLanguages != null && subtitlesLanguages.Count > 0)
        {
          program.SubtitlesLanguages = string.Join(",", subtitlesLanguages);
        }
        if (productionYear > 0)
        {
          program.ProductionYear = productionYear;
        }
        if (countries.Count > 0)
        {
          program.ProductionCountry = string.Join("; ", countries);
        }
        if (starRating >= 0)
        {
          program.StarRating = starRating;
          program.StarRatingMaximum = starRatingMaximum;
        }
        foreach (var credit in credits)
        {
          program.ProgramCredits.Add(credit);
        }
        if (idCategory >= 0)
        {
          program.IdProgramCategory = idCategory;
        }

        mappedChannel.Programs.Add(program);
      }
    }

    private static DateTime ParseDateTime(string dateTimeString)
    {
      try
      {
        dateTimeString = dateTimeString.Trim();
        DateTime dateTime = new DateTime(int.Parse(dateTimeString.Substring(0, 4)), int.Parse(dateTimeString.Substring(4, 2)), int.Parse(dateTimeString.Substring(6, 2)));
        dateTime = dateTime.AddHours(int.Parse(dateTimeString.Substring(8, 2)));
        dateTime = dateTime.AddMinutes(int.Parse(dateTimeString.Substring(10, 2)));

        // This compensation automatically adjusts for daylight savings and/or
        // location. To work correctly, it relies on accurate Windows date/time
        // settings and the time offset specified in the XMLTV file itself.
        // Time zone can be specified as "GMT -0100", "+0300" or just "0500".
        int position = dateTimeString.IndexOf(" ");
        if (position > 0)
        {
          string timeZone = dateTimeString.Substring(position).ToLowerInvariant();
          int gmtPosition = timeZone.IndexOf("gmt");
          if (gmtPosition >= 0)
          {
            timeZone = timeZone.Substring(gmtPosition + "gmt".Length);
          }
          timeZone = timeZone.Trim();

          int offset = 0;
          if (!int.TryParse(timeZone, out offset))
          {
            Log.Warn("XMLTV import: failed to interpret time zone, date/time = {0}", dateTimeString);
          }

          dateTime = dateTime.AddHours(-offset / 100);
          dateTime = dateTime.AddMinutes(-offset % 100);
          dateTime = dateTime.ToLocalTime();
        }

        return dateTime;
      }
      catch (Exception ex)
      {
        Log.Error(ex, "XMLTV import: failed to interpret date/time, date/time = {0}", dateTimeString);
        return DateTime.MinValue;
      }
    }

    private static List<ProgramCredit> ParseCredits(XmlReader nodeCredits)
    {
      var programCredits = new List<ProgramCredit>();

      using (nodeCredits)
      {
        nodeCredits.ReadStartElement();
        while (!nodeCredits.EOF)
        {
          if (nodeCredits.NodeType == XmlNodeType.Element)
          {
            programCredits.Add(new ProgramCredit { Role = nodeCredits.Name, Person = nodeCredits.ReadString() });
          }
          nodeCredits.Read();
        }
      }
      return programCredits;
    }

    private static bool? ParseVideoDetails(XmlReader nodeVideo)
    {
      using (nodeVideo)
      {
        nodeVideo.ReadStartElement();
        while (!nodeVideo.EOF)
        {
          if (nodeVideo.NodeType == XmlNodeType.Element && nodeVideo.Name.Equals("quality"))
          {
            string quality = nodeVideo.ReadString().Replace(" ", "");
            if (quality.Equals("HDTV"))
            {
              return true;
            }
            string[] parts = quality.Split('x');
            int verticalResolution;
            if (parts.Length == 2 && int.TryParse(parts[1], out verticalResolution))
            {
              return verticalResolution >= 720;
            }
          }
          nodeVideo.Read();
        }
      }
      return null;
    }

    private static string ParseLanguage(string language)
    {
      if (language.Length == 2)
      {
        foreach (Iso639Language l in Iso639LanguageCollection.Instance.Languages)
        {
          if (string.Equals(language, l.TwoLetterCode))
          {
            return l.BibliographicCode;
          }
        }
      }
      else
      {
        foreach (Iso639Language l in Iso639LanguageCollection.Instance.Languages)
        {
          if (string.Equals(language, l.Name))  // name is English, so may not match (eg. Deutch vs. German, or Allemand vs. French)
          {
            return l.BibliographicCode;
          }
        }
      }
      return "und";
    }

    private static void ParseStarRating(string ratingString, out decimal rating, out decimal ratingMaximum)
    {
      string originalRatingString = ratingString;
      rating = -1;
      ratingMaximum = -1;
      try
      {
        // format = 5.2/10
        // Check if the rating is encoded inside an XML value tag.
        ratingString = ExtractValueWithOptionalElement(ratingString, "value");
        if (string.IsNullOrEmpty(ratingString))
        {
          return;
        }

        // Some EPG providers only supply the rating (ie. n from n/m without m).
        int slashPos = ratingString.IndexOf('/');
        if (slashPos > 0)
        {
          if (!decimal.TryParse(ratingString.Substring(slashPos + 1), NumberStyles.Float, NumberFormatInfo.InvariantInfo, out ratingMaximum))
          {
            Log.Debug("XMLTV import: star-rating maximum from \"{0}\" (originally \"{1}\") could not be used", ratingString, originalRatingString);
            ratingMaximum = -1;
          }
          ratingString.Remove(slashPos);
        }

        if (!decimal.TryParse(ratingString, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out rating))
        {
          Log.Debug("XMLTV import: star-rating \"{0}\" (originally \"{1}\") could not be used", ratingString, originalRatingString);
        }

        // Guess the maximum rating if necessary.
        if (ratingMaximum < 0)
        {
          if (rating < 5)
          {
            ratingMaximum = 5;
          }
          else if (rating < 10)
          {
            ratingMaximum = 10;
          }
          else if (rating < 100)
          {
            ratingMaximum = 100;
          }
          else
          {
            ratingMaximum = rating;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "XMLTV import: failed to parse star-rating \"{0}\"", originalRatingString);
      }
    }

    private static void ParseEpisodeNumber(string system, string number,
                                            out int seasonNumber, out int episodeNumber, out int episodePartNumber,
                                            out string seriesId, out string episodeId)
    {
      number = ConvertHtmlToAnsi(number).Replace(" ", string.Empty);
      seasonNumber = -1;
      episodeNumber = -1;
      episodePartNumber = -1;
      seriesId = null;
      episodeId = null;

      int tempCount;
      if (string.Equals(system, "xmltv_ns"))
      {
        int dot1Idx = number.IndexOf(".", 0);
        int dot2Idx = number.IndexOf(".", dot1Idx + 1);
        if (dot1Idx < 0 || dot2Idx < 0)
        {
          Log.Warn("XMLTV import: failed to interpret xmltv_ns episode number, number = {0}", number);
        }
        else
        {
          ParseXmlTvNsEpisodeNumberSection(number.Substring(0, dot1Idx), out seasonNumber, out tempCount);
          ParseXmlTvNsEpisodeNumberSection(number.Substring(dot1Idx + 1, dot2Idx - (dot1Idx + 1)), out episodeNumber, out tempCount);
          ParseXmlTvNsEpisodeNumberSection(number.Substring(dot2Idx + 1), out episodePartNumber, out tempCount);
          if (episodePartNumber <= 1 && tempCount <= 1)
          {
            // Episode part number should not be stored unless there are actually parts.
            episodePartNumber = -1;
          }
        }
        return;
      }

      if (string.Equals(system, "dd_progid"))
      {
        // Tribune Media Services
        // http://developer.tmsapi.com/docs/data_v1_1/
        // We should be compatible with the Dish Network series and episode IDs
        // produced by TsWriter (which are also sourced from TMS).
        // Examples:
        // series/episode = EP00316978.0051
        // movie = MV00022473.0000
        // show = SH01110845.0000
        // sport = SP00319125.0000
        if (
          number.Length != 15 ||
          !(
            number.StartsWith("EP") ||
            number.StartsWith("MV") ||
            number.StartsWith("SH") ||
            number.StartsWith("SP")
          )
        )
        {
          Log.Warn("XMLTV import: failed to interpret dd_progid episode number, number = {0}", number);
        }
        else
        {
          episodeId = number.Remove(10, 1);
          if (episodeId.StartsWith("EP"))
          {
            seriesId = number.Substring(0, 10);
          }
        }
        return;
      }

      if (string.Equals(system, "common"))
      {
        // Example: S02E02
        Match m = REGEX_COMMON_SEASON_EPISODE_FORMAT.Match(number);
        if (!m.Success)
        {
          Log.Warn("XMLTV import: failed to interpret common episode number, number = {0}", number);
        }
        else
        {
          seasonNumber = int.Parse(m.Groups[1].Captures[0].Value);
          episodeNumber = int.Parse(m.Groups[2].Captures[0].Value);
        }
        return;
      }

      int idx = 0;
      if (string.Equals(system, "onscreen"))
      {
        // example: 'Episode #1234' 
        idx = number.IndexOf("#") + 1;
      }

      // Assumption: if the value is a number, it's the human readable episode
      // number. Otherwise it must be the episode's identifier.
      string episodeIdCandidate = number.Substring(idx);
      int episodeNumberCandidate;
      if (!int.TryParse(episodeIdCandidate, out episodeNumberCandidate))
      {
        episodeId = episodeIdCandidate;
        return;
      }

      episodeNumber = episodeNumberCandidate;
    }

    private static void ParseXmlTvNsEpisodeNumberSection(string section, out int sectionNumber, out int sectionCount)
    {
      sectionNumber = -1;
      sectionCount = -1;
      if (string.IsNullOrEmpty(section))
      {
        return;
      }

      // Check the number format.
      int slashPos = section.IndexOf("/");
      if (slashPos == -1)
      {
        // No slash found => should be just a plain number.
        try
        {
          sectionNumber = Convert.ToInt32(section) + 1;
        }
        catch (Exception)
        {
          Log.Debug("XMLTV import: failed to convert xmltv_ns episode number section, section = {0}", section);
        }
        return;
      }

      try
      {
        // Slash found => assume it's formatted as <number>/<count>.
        sectionNumber = Convert.ToInt32(section.Substring(0, slashPos)) + 1;
        sectionCount = Convert.ToInt32(section.Substring(slashPos + 1));
      }
      catch (Exception)
      {
        Log.Debug("XMLTV import: failed to interpret 2 part xmltv_ns episode number section, section = {0}", section);
      }
    }

    private static string ExtractValueWithOptionalElement(string xmlFragment, string optionalElementName)
    {
      if (xmlFragment == null)
      {
        return string.Empty;
      }
      xmlFragment = xmlFragment.Trim();
      if (string.IsNullOrEmpty(xmlFragment))
      {
        return string.Empty;
      }

      if (xmlFragment.StartsWith("<"))
      {
        int endStartTagIdx = xmlFragment.IndexOf("<" + optionalElementName + ">") + optionalElementName.Length + 2;
        int length = xmlFragment.IndexOf("</" + optionalElementName + ">", endStartTagIdx) - endStartTagIdx;
        return xmlFragment.Substring(endStartTagIdx, length).Trim();
      }
      return xmlFragment;
    }

    private static string ConvertHtmlToAnsi(string html)
    {
      string strippedHtml = string.Empty;
      if (!string.IsNullOrEmpty(html))
      {
        using (var writer = new StringWriter())
        {
          System.Web.HttpUtility.HtmlDecode(html, writer);
          strippedHtml = writer.ToString().Replace("<br>", Environment.NewLine);
        }
      }
      return strippedHtml;
    }

    private static string TidyString(string s)
    {
      if (!string.IsNullOrEmpty(s))
      {
        s = s.Replace("\r\n", "[[0]]");
        s = s.Replace("\n\r", "[[0]]");
        s = s.Replace("\r", "[[0]]");
        s = s.Replace("\n", "[[0]]");
        s = s.Replace("[[0]]", Environment.NewLine);
        s = s.Replace("  ", " ");
        s = s.Trim();
      }
      return s;
    }
  }
}