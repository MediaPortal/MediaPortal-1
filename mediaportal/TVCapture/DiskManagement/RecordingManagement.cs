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
  public class RecordingManagement
  {
    static DateTime _deleteOldRecordingTimer = DateTime.MinValue;

    /// <summary>
    /// This method will get all the tv-recordings present in the tv database
    /// For each recording it looks at the Keep until settings. If the recording should be
    /// deleted by date, then it will delete the recording from the database, and harddisk
    /// if the the current date > keep until date
    /// </summary>
    /// <remarks>Note, this method will only work after a day-change has occured(and at startup)
    /// </remarks>
    static public void DeleteOldRecordings()
    {
      if (!TimeToDeleteOldRecordings(DateTime.Now)) return;
      List<TVRecorded> recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      foreach (TVRecorded rec in recordings)
      {
        if (!ShouldDeleteRecording(rec)) continue;

        Log.WriteFile(Log.LogType.Recorder, "Recorder: delete old recording:{0} date:{1}",
                          rec.FileName,
                          rec.StartTime.ToShortDateString());
        DiskManagement.DeleteRecording(rec);
      }
    }

    static public bool TimeToDeleteOldRecordings(DateTime dateTime)
    {
      if (dateTime.Date == _deleteOldRecordingTimer.Date) return false;
      _deleteOldRecordingTimer = dateTime;
      return true;
    }

    static public bool ShouldDeleteRecording(TVRecorded rec)
    {
      if (rec.KeepRecordingMethod != TVRecorded.KeepMethod.TillDate) return false;
      if (rec.KeepRecordingTill.Date > DateTime.Now.Date) return false;
      return true;
    }
  }
}
