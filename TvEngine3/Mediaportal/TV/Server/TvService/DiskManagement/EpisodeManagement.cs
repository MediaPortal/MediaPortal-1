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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVService.DiskManagement
{
  public class EpisodeManagement
  {
    public List<Recording> GetEpisodes(string title, IList<Recording> recordings)
    {
      List<Recording> episodes = new List<Recording>();
      foreach (Recording recording in recordings)
      {
        if (String.Compare(title, recording.title, true) == 0)
        {
          episodes.Add(recording);
        }
      }
      return episodes;
    }

    public Recording GetOldestEpisode(List<Recording> episodes)
    {
      Recording oldestEpisode = null;
      DateTime oldestDateTime = DateTime.MaxValue;
      foreach (Recording rec in episodes)
      {
        if (rec.startTime < oldestDateTime)
        {
          oldestDateTime = rec.startTime;
          oldestEpisode = rec;
        }
      }
      return oldestEpisode;
    }

    #region episode disk management

    public void OnScheduleEnded(string recordingFilename, Schedule recording, Program program)
    {
      Log.Write("diskmanagement: recording {0} ended. type:{1} max episodes:{2}",
                program.title, (ScheduleRecordingType)recording.scheduleType, recording.maxAirings);

      CheckEpsiodesForRecording(recording, program);
    }

    private void CheckEpsiodesForRecording(Schedule schedule, Program program)
    {

      if (!ScheduleManagement.DoesScheduleUseEpisodeManagement(schedule))
      {
        return;
      }

      //check how many episodes we got
      while (true)
      {
        IList<Recording> recordings = TVDatabase.TVBusinessLayer.RecordingManagement.ListAllRecordingsByMediaType(MediaTypeEnum.TV);

        List<Recording> episodes = GetEpisodes(program.title, recordings);
        if (episodes.Count <= schedule.maxAirings)
          return;

        Recording oldestEpisode = GetOldestEpisode(episodes);
        if (oldestEpisode == null)
          return;
        Log.Write("diskmanagement:   Delete episode {0} {1} {2} {3}",
                  oldestEpisode.Channel,
                  oldestEpisode.title,
                  oldestEpisode.startTime.ToLongDateString(),
                  oldestEpisode.startTime.ToLongTimeString());

        // Delete the file from disk and the recording entry from the database.        
        bool result = RecordingFileHandler.DeleteRecordingOnDisk(oldestEpisode.fileName);
        if (result)
        {
          TVDatabase.TVBusinessLayer.RecordingManagement.DeleteRecording(oldestEpisode.idRecording);
        }
      }
    }

    #endregion
  }
}