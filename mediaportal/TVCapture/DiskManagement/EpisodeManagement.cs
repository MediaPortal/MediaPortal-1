/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.DiskSpace
{
  public class EpisodeManagement
  {
    static EpisodeManagement()
    {
      Recorder.OnTvRecordingEnded += new MediaPortal.TV.Recording.Recorder.OnTvRecordingHandler(EpisodeManagement.Recorder_OnTvRecordingEnded);
    }
    static bool DoesUseEpisodeManagement(TVRecording recording)
    {
      if (recording.RecType == TVRecording.RecordingType.Once) return false;
      if (recording.EpisodesToKeep == Int32.MaxValue) return false;
      if (recording.EpisodesToKeep < 1) return false;
      return true;
    }

    static List<TVRecorded> GetEpisodes(string title, List<TVRecorded> recordings)
    {
      List<TVRecorded> episodes = new List<TVRecorded>();
      foreach (TVRecorded recording in recordings)
      {
        if (String.Compare(title, recording.Title, true) == 0)
        {
          episodes.Add(recording);
        }
      }
      return episodes;
    }

    static TVRecorded GetOldestEpisode(List<TVRecorded> recordings)
    {
      TVRecorded oldestEpisode = null;
      DateTime oldestDateTime = DateTime.MaxValue;
      foreach (TVRecorded rec in recordings)
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
    static private void Recorder_OnTvRecordingEnded(string recordingFilename, TVRecording recording, TVProgram program)
    {
      Log.WriteFile(Log.LogType.Recorder, "diskmanagement: recording {0} ended. type:{1} max episodes:{2}",
          recording.Title, recording.RecType.ToString(), recording.EpisodesToKeep);

      if (!DoesUseEpisodeManagement(recording)) return;

      //check how many episodes we got
      while (true)
      {
        List<TVRecorded> recordings = new List<TVRecorded>();
        TVDatabase.GetRecordedTV(ref recordings);

        List<TVRecorded> episodes = GetEpisodes(recording.Title, recordings);
        if (episodes.Count <= recording.EpisodesToKeep) return;

        TVRecorded oldestEpisode = GetOldestEpisode(episodes);
        if (oldestEpisode == null) return;
        Log.WriteFile(Log.LogType.Recorder, false, "diskmanagement:   Delete episode {0} {1} {2} {3}",
                             oldestEpisode.Channel,
                             oldestEpisode.Title,
                             oldestEpisode.StartTime.ToLongDateString(),
                             oldestEpisode.StartTime.ToLongTimeString());

        DiskManagement.DeleteRecording(oldestEpisode);
      }
    }
    #endregion
  }
}
