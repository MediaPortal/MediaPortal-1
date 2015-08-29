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
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.DiskManagement
{
  public class EpisodeManagement
  {
    private static List<Recording> GetEpisodes(string title, IList<Recording> recordings)
    {
      List<Recording> episodes = new List<Recording>();
      foreach (Recording recording in recordings)
      {
        if (String.Compare(title, recording.Title, true) == 0)
        {
          episodes.Add(recording);
        }
      }
      return episodes;
    }

    private static Recording GetOldestEpisode(List<Recording> episodes)
    {
      Recording oldestEpisode = null;
      DateTime oldestDateTime = DateTime.MaxValue;
      foreach (Recording rec in episodes)
      {
        if (rec.StartTime < oldestDateTime)
        {
          oldestDateTime = rec.StartTime;
          oldestEpisode = rec;
        }
      }
      return oldestEpisode;
    }

    #region episode disk management

    public void OnScheduleEnded(string recordingFilename, Schedule recording, Program program)
    {
      this.LogDebug("diskmanagement: recording {0} ended. type:{1} max episodes:{2}",
                program.Title, (ScheduleRecordingType)recording.ScheduleType, recording.MaxAirings);

      if (!ScheduleManagement.DoesScheduleUseEpisodeManagement(recording))
      {
        return;
      }

      // Delete the oldest episode if we've exceeded the number of episodes
      // that the schedule says to keep.
      while (true)
      {
        IList<Recording> recordings = TVDatabase.TVBusinessLayer.RecordingManagement.ListAllRecordingsByMediaType(MediaType.Television);

        List<Recording> episodes = GetEpisodes(program.Title, recordings);
        if (episodes.Count <= recording.MaxAirings)
          return;

        Recording oldestEpisode = GetOldestEpisode(episodes);
        if (oldestEpisode == null)
          return;
        this.LogDebug("diskmanagement:   Delete episode {0} {1} {2} {3}",
                  oldestEpisode.Channel,
                  oldestEpisode.Title,
                  oldestEpisode.StartTime.ToLongDateString(),
                  oldestEpisode.StartTime.ToLongTimeString());

        // Delete the file from disk and the recording entry from the database.        
        bool result = RecordingFileHandler.DeleteRecordingOnDisk(oldestEpisode.FileName);
        if (result)
        {
          TVDatabase.TVBusinessLayer.RecordingManagement.DeleteRecording(oldestEpisode.IdRecording);
        }
      }
    }

    #endregion
  }
}