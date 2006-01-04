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

    #region episode disk management
    static private void Recorder_OnTvRecordingEnded(string recordingFilename, TVRecording recording, TVProgram program)
    {
      Log.WriteFile(Log.LogType.Recorder, "diskmanagement: recording {0} ended. type:{1} max episodes:{2}",
          recording.Title, recording.RecType.ToString(), recording.EpisodesToKeep);

      if (recording.EpisodesToKeep == Int32.MaxValue) return;
      if (recording.RecType == TVRecording.RecordingType.Once) return;

      //check how many episodes we got
      List<TVRecorded> recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      while (true)
      {
        Log.WriteFile(Log.LogType.Recorder, "got:{0} recordings", recordings.Count);
        int recordingsFound = 0;
        DateTime oldestRecording = DateTime.MaxValue;
        string oldestFileName = String.Empty;
        TVRecorded oldestRec = null;
        foreach (TVRecorded rec in recordings)
        {
          Log.WriteFile(Log.LogType.Recorder, "check:{0}", rec.Title);
          if (String.Compare(rec.Title, recording.Title, true) == 0)
          {
            recordingsFound++;
            if (rec.StartTime < oldestRecording)
            {
              oldestRecording = rec.StartTime;
              oldestFileName = rec.FileName;
              oldestRec = rec;
            }
          }
        }
        Log.WriteFile(Log.LogType.Recorder, "diskmanagement:   total episodes now:{0}", recordingsFound);
        if (oldestRec != null)
        {
          Log.WriteFile(Log.LogType.Recorder, "diskmanagement:   oldest episode:{0} {1}", oldestRec.StartTime.ToShortDateString(), oldestRec.StartTime.ToLongTimeString());
        }

        if (oldestRec == null) return;
        if (recordingsFound == 0) return;
        if (recordingsFound <= recording.EpisodesToKeep) return;
        Log.WriteFile(Log.LogType.Recorder, false, "diskmanagement:   Delete episode {0} {1} {2} {3}",
                             oldestRec.Channel,
                             oldestRec.Title,
                             oldestRec.StartTime.ToLongDateString(),
                             oldestRec.StartTime.ToLongTimeString());

        if (Utils.FileDelete(oldestFileName))
        {
          DiskManagement.DeleteRecording(oldestFileName);

          VideoDatabase.DeleteMovie(oldestFileName);
          VideoDatabase.DeleteMovieInfo(oldestFileName);
          recordings.Remove(oldestRec);
          TVDatabase.RemoveRecordedTV(oldestRec);
        }
      }
    }
    #endregion
  }
}
