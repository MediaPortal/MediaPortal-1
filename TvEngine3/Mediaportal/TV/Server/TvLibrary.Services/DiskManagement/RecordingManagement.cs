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
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.DiskManagement
{
  public class RecordingManagement
  {
    private readonly System.Timers.Timer _timer;    

    public RecordingManagement()
    {
      _timer = new System.Timers.Timer();
      _timer.Interval = 4 * 60 * 60 * 1000;
      _timer.Enabled = false;
      _timer.Elapsed += _timer_Elapsed;
    }

    private static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      DeleteOldRecordings();
    }

    /// <summary>
    /// This method will get all the tv-recordings present in the tv database
    /// For each recording it looks at the Keep until settings. If the recording should be
    /// deleted by date, then it will delete the recording from the database, and harddisk
    /// if the the current date > keep until date
    /// </summary>
    /// <remarks>Note, this method will only work after a day-change has occured(and at startup)
    /// </remarks>
    private static void DeleteOldRecordings()
    {
      IList<Recording> recordings = TVDatabase.TVBusinessLayer.RecordingManagement.ListAllRecordingsByMediaType(MediaTypeEnum.TV);
      foreach (Recording recording in recordings.Where(ShouldBeDeleted))
      {
        Log.Write("Recorder: delete old recording:{0} date:{1}",
                  recording.FileName,
                  recording.StartTime.ToShortDateString());
        recordings.Remove(recording);        
        TVDatabase.TVBusinessLayer.RecordingManagement.DeleteRecording(recording.IdRecording);
        break;
      }
    }

    private static bool ShouldBeDeleted(Recording recording)
    {
      if (recording.KeepUntil != (int)KeepMethodType.TillDate)
      {
        return false;
      }
      if (recording.KeepUntilDate.GetValueOrDefault(DateTime.MinValue) > DateTime.Now)
      {
        return false;
      }
      return true;
    }
  }
}