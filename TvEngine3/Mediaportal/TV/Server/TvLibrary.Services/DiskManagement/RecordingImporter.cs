#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Data.SqlTypes;
using System.IO;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.DiskManagement
{
  internal class RecordingImporter
  {
    private RecordingImporter()
    {
    }

    public static void Import(string directory)
    {
      Log.Debug("recording importer: import, directory = {0}", directory ?? string.Empty);
      if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
      {
        return;
      }

      IList<Channel> dbChannels = ChannelManagement.ListAllChannels(ChannelIncludeRelationEnum.None);
      Dictionary<string, IList<Channel>> channelsByName = new Dictionary<string, IList<Channel>>(dbChannels.Count);
      IList<Channel> channels;
      foreach (Channel c in dbChannels)
      {
        string name = c.Name.ToLowerInvariant();
        if (!channelsByName.TryGetValue(name, out channels))
        {
          channels = new List<Channel>(5);
          channelsByName.Add(name, channels);
        }
        channels.Add(c);
      }

      IList<ProgramCategory> dbCategories = ProgramCategoryManagement.ListAllProgramCategories();
      Dictionary<string, ProgramCategory> categoriesByName = new Dictionary<string, ProgramCategory>(dbCategories.Count);
      foreach (ProgramCategory c in dbCategories)
      {
        categoriesByName[c.Category.ToLowerInvariant()] = c;
      }

      IList<Recording> dbRecordings = TVDatabase.TVBusinessLayer.RecordingManagement.ListAllRecordings();
      Dictionary<string, Recording> recordingsByFileName = new Dictionary<string, Recording>(dbRecordings.Count);
      foreach (Recording r in dbRecordings)
      {
        recordingsByFileName[Path.GetFileNameWithoutExtension(r.FileName)] = r;
      }

      try
      {
        IList<string> recognisedExtensions = new List<string> { ".ts", ".wtv", ".dvr-ms", ".mkv", ".mp4", ".mpeg", ".mpg", ".avi", ".flac", ".mp3" };
        foreach (string xmlFileName in Directory.GetFiles(directory, "*.xml", SearchOption.AllDirectories))
        {
          foreach (string ext in recognisedExtensions)
          {
            string recordingFileName = Path.ChangeExtension(xmlFileName, ext);
            if (File.Exists(recordingFileName))
            {
              Recording existingRecording;
              if (recordingsByFileName.TryGetValue(Path.GetFileNameWithoutExtension(recordingFileName), out existingRecording))
              {
                if (!string.Equals(recordingFileName, existingRecording.FileName))
                {
                  Log.Debug("  update file name, ID = {0}, current = {1}, new = {2}", existingRecording.IdRecording, existingRecording.FileName, recordingFileName);
                  existingRecording.FileName = recordingFileName;
                  TVDatabase.TVBusinessLayer.RecordingManagement.SaveRecording(existingRecording);
                }
                break;
              }

              Recording r = CreateRecordingFromMatroskaMetaInfo(channelsByName, categoriesByName, xmlFileName);
              if (r != null)
              {
                r.FileName = recordingFileName;
                Log.Debug("  add, file name = {0}", recordingFileName);
                TVDatabase.TVBusinessLayer.RecordingManagement.SaveRecording(r);
                break;
              }
            }
          }
        }

        foreach (string recordingFileName in Directory.GetFiles(directory, "*.wtv", SearchOption.AllDirectories))
        {
          if (File.Exists(Path.ChangeExtension(recordingFileName, ".xml")))
          {
            continue; // imported above
          }

          Recording existingRecording;
          if (recordingsByFileName.TryGetValue(Path.GetFileNameWithoutExtension(recordingFileName), out existingRecording))
          {
            if (!string.Equals(recordingFileName, existingRecording.FileName))
            {
              Log.Debug("  update file name, ID = {0}, current = {1}, new = {2}", existingRecording.IdRecording, existingRecording.FileName, recordingFileName);
              existingRecording.FileName = recordingFileName;
              TVDatabase.TVBusinessLayer.RecordingManagement.SaveRecording(existingRecording);
            }
            continue;
          }

          Recording r = CreateRecordingFromWmcRecording(channelsByName, categoriesByName, recordingFileName);
          if (r != null)
          {
            Log.Debug("  add, file name = {0}", recordingFileName);
            TVDatabase.TVBusinessLayer.RecordingManagement.SaveRecording(r);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "recording importer: failed to import, directory = {0}", directory);
      }
    }

    private static Recording CreateRecordingFromMatroskaMetaInfo(IDictionary<string, IList<Channel>> channelsByName, IDictionary<string, ProgramCategory> categoriesByName, string xmlFileName)
    {
      MatroskaMetaInfo info = MatroskaMetaInfo.Read(xmlFileName);
      if (info == null)
      {
        return null;
      }

      Recording r = RecordingFactory.CreateRecording(string.Empty, info.MediaType, info.StartTime, info.EndTime, info.Title);
      r.Description = info.Description;
      r.EpisodeName = info.EpisodeName;
      r.SeriesId = info.SeriesId;
      r.SeasonNumber = info.SeasonNumber;
      r.EpisodeId = info.EpisodeId;
      r.EpisodeNumber = info.EpisodeNumber;
      r.EpisodePartNumber = info.EpisodePartNumber;
      r.IsPreviouslyShown = info.IsPreviouslyShown;
      r.OriginalAirDate = info.OriginalAirDate;
      r.Classification = info.Classification;
      r.Advisories = (int)info.Advisories;
      r.IsHighDefinition = info.IsHighDefinition;
      r.IsThreeDimensional = info.IsThreeDimensional;
      r.IsLive = info.IsLive;
      r.ProductionYear = info.ProductionYear;
      r.ProductionCountry = info.ProductionCountry;
      r.StarRating = info.StarRating;
      r.StarRatingMaximum = info.StarRatingMaximum;
      r.WatchedCount = info.WatchedCount;

      Channel channel = GetChannelByMediaTypeAndName(channelsByName, info.MediaType, new List<string> { info.ChannelName });
      if (channel != null)
      {
        r.IdChannel = channel.IdChannel;
      }

      ProgramCategory category = GetCategoryByName(categoriesByName, new List<string> { info.ProgramCategory });
      if (category != null)
      {
        r.IdProgramCategory = category.IdProgramCategory;
      }

      foreach (var credit in info.Credits)
      {
        r.RecordingCredits.Add(new RecordingCredit { Person = credit.Key, Role = credit.Value });
      }

      FixTimes(r);
      return r;
    }

    private static Recording CreateRecordingFromWmcRecording(IDictionary<string, IList<Channel>> channelsByName, IDictionary<string, ProgramCategory> categoriesByName, string recordingFileName)
    {
      WmcMetaInfo info = WmcMetaInfo.Read(recordingFileName);
      if (info == null || info.IsProtected)
      {
        return null;
      }

      Recording r = RecordingFactory.CreateRecording(recordingFileName, info.MediaType, info.StartTime, info.EndTime, info.Title);
      r.EpisodeName = info.EpisodeName;
      r.SeasonNumber = info.SeasonNumber;
      r.EpisodeNumber = info.EpisodeNumber;
      if (info.IsPremiere.HasValue && info.IsPremiere.Value)
      {
        r.IsPreviouslyShown = false;
      }
      else if (info.IsRepeat.HasValue && info.IsRepeat.Value)
      {
        r.IsPreviouslyShown = true;
      }
      r.Classification = info.Classification;
      r.IsHighDefinition = info.IsHighDefinition;
      r.IsLive = info.IsLive;
      r.ProductionYear = info.ProductionYear;
      r.WatchedCount = info.IsWatched.HasValue && info.IsWatched.Value ? 1 : 0;

      Channel channel = GetChannelByMediaTypeAndName(channelsByName, info.MediaType, info.ChannelNames);
      if (channel != null)
      {
        r.IdChannel = channel.IdChannel;
      }

      ProgramCategory category = GetCategoryByName(categoriesByName, info.Categories);
      if (category != null)
      {
        r.IdProgramCategory = category.IdProgramCategory;
      }

      if ((string.IsNullOrEmpty(info.Description) && !string.IsNullOrEmpty(info.EpisodeDescription)) || string.Equals(info.Description, info.EpisodeDescription))
      {
        r.Description = info.EpisodeDescription;
      }
      else if (!string.IsNullOrEmpty(info.Description) && string.IsNullOrEmpty(info.EpisodeDescription))
      {
        r.Description = info.Description;
      }
      else if (!string.IsNullOrEmpty(info.Description) && !string.IsNullOrEmpty(info.EpisodeDescription))
      {
        if (info.EpisodeDescription.Contains(info.Description))
        {
          r.Description = info.EpisodeDescription;
        }
        else if (info.Description.Contains(info.EpisodeDescription))
        {
          r.Description = info.Description;
        }
        else
        {
          r.Description = string.Format("{0}{1}{2}", info.Description, Environment.NewLine, info.EpisodeDescription);
        }
      }

      foreach (var credit in info.Credits)
      {
        r.RecordingCredits.Add(new RecordingCredit { Person = credit.Key, Role = credit.Value });
      }

      FixTimes(r);
      return r;
    }

    private static Channel GetChannelByMediaTypeAndName(IDictionary<string, IList<Channel>> channelsByName, MediaType mediaType, IEnumerable<string> names)
    {
      IList<Channel> channels;
      foreach (string name in names)
      {
        if (!string.IsNullOrEmpty(name) && channelsByName.TryGetValue(name.ToLowerInvariant(), out channels))
        {
          foreach (Channel channel in channels)
          {
            if ((int)mediaType == channel.MediaType)
            {
              return channel;
            }
          }
        }
      }

      foreach (var pair in channelsByName)
      {
        foreach (string name in names)
        {
          if (!string.IsNullOrEmpty(name) && (name.ToLowerInvariant().Contains(pair.Key) || pair.Key.Contains(name.ToLowerInvariant())))
          {
            foreach (Channel channel in pair.Value)
            {
              if ((int)mediaType == channel.MediaType)
              {
                return channel;
              }
            }
          }
        }
      }
      return null;
    }

    private static ProgramCategory GetCategoryByName(IDictionary<string, ProgramCategory> categoriesByName, IEnumerable<string> names)
    {
      ProgramCategory category = null;
      IList<ProgramCategory> newCategories = new List<ProgramCategory>();
      foreach (string name in names)
      {
        if (string.IsNullOrEmpty(name))
        {
          continue;
        }
        ProgramCategory tempCategory;
        if (!categoriesByName.TryGetValue(name.ToLowerInvariant(), out tempCategory))
        {
          tempCategory = new ProgramCategory { Category = name };
          tempCategory = ProgramCategoryManagement.AddCategory(tempCategory);
          newCategories.Add(tempCategory);
        }
        else if (category == null || (!category.IdGuideCategory.HasValue && tempCategory.IdGuideCategory.HasValue))
        {
          category = tempCategory;
        }
      }

      foreach (ProgramCategory c in newCategories)
      {
        categoriesByName[c.Category.ToLowerInvariant()] = c;
      }

      if (category != null)
      {
        return category;
      }

      if (newCategories.Count > 0)
      {
        return newCategories[0];
      }
      return null;
    }

    private static void FixTimes(Recording recording)
    {
      if (recording.StartTime == SqlDateTime.MinValue.Value || recording.EndTime == SqlDateTime.MinValue.Value)
      {
        try
        {
          FileInfo fileInfo = new FileInfo(recording.FileName);
          if (recording.StartTime == SqlDateTime.MinValue.Value)
          {
            recording.StartTime = fileInfo.CreationTime;
          }
          if (recording.EndTime == SqlDateTime.MinValue.Value)
          {
            recording.EndTime = fileInfo.LastWriteTime;
          }
        }
        catch (Exception ex)
        {
          Log.Warn(ex, "recording importer: failed to get time details for file, file name = {0}", recording.FileName);
        }
      }
    }
  }
}