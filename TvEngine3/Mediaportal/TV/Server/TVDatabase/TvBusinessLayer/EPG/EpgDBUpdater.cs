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
using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.EPG
{
  public class EpgDBUpdater
  {
    public delegate void OnImportProgramsForChannel(int channelId);

    #region Variables

    private readonly OnImportProgramsForChannel _importDelegate;
    private string _epgLanguages;
    private readonly string _grabberName;

    #endregion

    #region ctor

    public EpgDBUpdater(OnImportProgramsForChannel epgCallBack, string grabberName)
    {
      _importDelegate = epgCallBack;
      _grabberName = grabberName;
      ReloadConfig();
    }

    #endregion

    #region Public members

    public void ReloadConfig()
    {
      _epgLanguages = SettingsManagement.GetValue("epgLanguages", string.Empty);
    }

    public void UpdateEpgForChannel(IChannel tuningDetail, KeyValuePair<IChannel, IList<EpgProgram>> epgChannel)
    {
      Channel dbChannel = IsInsertAllowed(tuningDetail, epgChannel);
      if (dbChannel == null)
      {
        return;
      }
      this.LogDebug("{0}: update channel {1} ({2}), last update {3}...", _grabberName, dbChannel.IdChannel, dbChannel.Name, dbChannel.LastGrabTime);

      int countReplaced = 0;
      int countUnchanged = 0;
      int countDeleted = 0;

      ProgramManagement.DeleteOldPrograms(dbChannel.IdChannel);
      IList<Program> existingPrograms = ProgramManagement.GetPrograms(dbChannel.IdChannel, epgChannel.Value[0].StartTime);
      IList<Program> programsToAdd = new List<Program>();
      int existingProgramIndex = 0;
      foreach (EpgProgram program in epgChannel.Value)
      {
        if (existingProgramIndex >= existingPrograms.Count)
        {
          programsToAdd.Add(CreateOrReplaceProgram(dbChannel, program));
          continue;
        }

        Program existingProgram = existingPrograms[existingProgramIndex];
        if (program.StartTime < existingProgram.StartTime)
        {
          // insert new program
          programsToAdd.Add(CreateOrReplaceProgram(dbChannel, program));
        }
        else
        {
          // compare and replace if not eqivalent
          Program newProgram = CreateOrReplaceProgram(dbChannel, program, existingPrograms[existingProgramIndex]);
          if (newProgram != null)
          {
            programsToAdd.Add(newProgram);
            ProgramManagement.DeleteProgram(existingProgram.IdProgram);
            countReplaced++;
          }
          else
          {
            countUnchanged++;
          }
          existingProgramIndex++;
        }

        // delete obsolete existing programs
        while (true)
        {
          if (existingProgramIndex >= existingPrograms.Count)
          {
            break;
          }
          existingProgram = existingPrograms[existingProgramIndex];
          if (existingProgram.StartTime < program.EndTime)
          {
            // delete existing program
            ProgramManagement.DeleteProgram(existingProgram.IdProgram);
            existingProgramIndex++;
            countDeleted++;
          }
          else
          {
            break;
          }
        }
      }

      if (programsToAdd.Count > 0)
      {
        ProgramManagement.SavePrograms(programsToAdd);
        dbChannel.LastGrabTime = DateTime.Now;
        ChannelManagement.SaveChannel(dbChannel);
      }
      this.LogDebug("{0}: updated channel {1} ({2}), new = {3}, replaced = {4}, deleted = {5}, unchanged = {6}", _grabberName, dbChannel.IdChannel, dbChannel.Name, programsToAdd.Count - countReplaced, countReplaced, countDeleted, countUnchanged);

      // Raise an event with the data so that other plugins can handle the data on their own.
      if (_importDelegate != null)
      {
        _importDelegate(dbChannel.IdChannel);
      }
    }

    #endregion

    #region private functions

    private Channel IsInsertAllowed(IChannel sourceTuningDetail, KeyValuePair<IChannel, IList<EpgProgram>> epgChannel)
    {
      ChannelDvbBase dvbChannel = epgChannel.Key as ChannelDvbBase;
      if (dvbChannel == null)
      {
        this.LogError("{0}: failed assumption, grabbing not supported for non-DVB channels");
        return null;
      }

      if (epgChannel.Value.Count == 0)
      {
        this.LogInfo("{0}: no info found for service, ONID = {1}, TSID = {2}, service ID = {3}",
                      _grabberName, dvbChannel.OriginalNetworkId, dvbChannel.TransportStreamId, dvbChannel.ServiceId);
        return null;
      }

      int? satelliteId = null;
      IChannelSatellite satelliteChannel = epgChannel.Key as IChannelSatellite;
      if (satelliteChannel != null)
      {
        // TODO this isn't the real satellite ID that we want
        satelliteId = satelliteChannel.DiseqcPositionerSatelliteIndex;
      }

      //do we have a channel with these service details?
      BroadcastStandard broadcastStandard = TuningDetailManagement.GetBroadcastStandardFromChannelInstance(epgChannel.Key);
      IList<TuningDetail> tuningDetails = TuningDetailManagement.GetDvbTuningDetails(
        broadcastStandard,
        dvbChannel.OriginalNetworkId,
        dvbChannel.ServiceId,
        dvbChannel.TransportStreamId,
        null,
        satelliteId
      );
      if (tuningDetails == null || tuningDetails.Count == 0)
      {
        this.LogInfo("{0}: zero matching tuning details found for service, broadcast standard = {1}, ONID = {2}, TSID = {3}, service ID = {4}",
                      _grabberName, broadcastStandard, dvbChannel.OriginalNetworkId, dvbChannel.TransportStreamId, dvbChannel.ServiceId);
        return null;
      }
      if (tuningDetails.Count != 1)
      {
        this.LogInfo("{0}: discard {1} program(s) for service which matches {2} tuning details, broadcast standard = {3}, ONID = {4}, TSID = {5}, service ID = {6}",
                      _grabberName, epgChannel.Value.Count, tuningDetails.Count, broadcastStandard, dvbChannel.OriginalNetworkId, dvbChannel.TransportStreamId, dvbChannel.ServiceId);
        return null;
      }

      //should we store EPG for this channel?
      Channel dbChannel = tuningDetails[0].Channel;
      if (!string.IsNullOrEmpty(dbChannel.ExternalId))
      {
        this.LogInfo("{0}: discard {1} program(s) for channel {2} ({3}), handled by plugin", _grabberName, epgChannel.Value.Count, dbChannel.IdChannel, dbChannel.Name);
        return null;
      }

      return dbChannel;
    }

    private void GetPreferredText(IDictionary<string, string> text, out string selectedText)
    {
      selectedText = string.Empty;

      if (text.Count == 0)
      {
        return;
      }

      int priority = -1;
      foreach (var t in text)
      {
        if (string.IsNullOrWhiteSpace(_epgLanguages))
        {
          selectedText = t.Value;
          break;
        }
        int index = _epgLanguages.ToLowerInvariant().IndexOf(t.Key.ToLowerInvariant());
        if (priority < 0 || (index >= 0 && index < priority))
        {
          selectedText = t.Value;
          priority = index;
        }
      }
    }

    private Program CreateOrReplaceProgram(Channel dbChannel, EpgProgram epgProgram, Program dbProgram = null)
    {
      string title;
      string description;

      GetPreferredText(epgProgram.Titles, out title);
      GetPreferredText(epgProgram.Descriptions, out description);

      if (
        dbProgram != null &&
        epgProgram.StartTime == dbProgram.StartTime &&
        epgProgram.EndTime == dbProgram.EndTime &&
        string.Equals(title, dbProgram.Title) &&
        description.Length <= dbProgram.Description.Length
      )
      {
        // Existing program is a close enough match.
        return null;
      }

      // Create a new program.
      int idCategory = -1;
      bool isCategoryMapped = false;
      foreach (string c in epgProgram.Categories)
      {
        // If this is a new category, add it to the DB.
        ProgramCategory programCategory = EntityCacheHelper.Instance.ProgramCategoryCache.GetFromCache(c);
        if (programCategory == null)
        {
          programCategory = new ProgramCategory { Category = c };
          programCategory = ProgramCategoryManagement.SaveProgramCategory(programCategory);
          EntityCacheHelper.Instance.ProgramCategoryCache.AddOrUpdateCache(c, programCategory);
        }

        // We can only link the program to one category. Prefer a category that is mapped to a guide category.
        if (idCategory <= 0 || (!isCategoryMapped && programCategory.IdGuideCategory.HasValue))
        {
          idCategory = programCategory.IdProgramCategory;
          isCategoryMapped = programCategory.IdGuideCategory.HasValue;
        }
      }

      var program = ProgramFactory.CreateEmptyProgram();
      program.IdChannel = dbChannel.IdChannel;
      program.StartTime = epgProgram.StartTime;
      program.EndTime = epgProgram.EndTime;
      program.Title = title;
      program.Description = description;
      if (idCategory != -1)
      {
        program.IdProgramCategory = idCategory;
      }
      foreach (var c in epgProgram.Classifications)
      {
        program.Classification = c.Value;
        break;    // TODO we want to use the preferred classification system
      }

      // TODO ensure star rating doesn't get assigned unless it has a value
      //program.StarRating = starRating;
      //program.StarRatingMaximum = 4;
      return program;
    }

    #endregion
  }
}