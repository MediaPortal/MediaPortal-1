#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using System.Collections;
using TvDatabase;
using TvLibrary.Log;

namespace TvService
{
  public class RecordingManagement
  {
    readonly System.Timers.Timer _timer;
    public RecordingManagement()
    {
      _timer = new System.Timers.Timer();
      _timer.Interval = 4 * 60 * 60 * 1000;
      _timer.Enabled = false;
      _timer.Elapsed += _timer_Elapsed;
    }

    static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
    static void DeleteOldRecordings()
    {
      IList recordings = Recording.ListAll();
      foreach (Recording rec in recordings)
      {
        if (!rec.ShouldBeDeleted)
          continue;

        Log.Write("Recorder: delete old recording:{0} date:{1}",
                          rec.FileName,
                          rec.StartTime.ToShortDateString());
        recordings.Remove(rec);
        rec.Remove();
        break;
      }
    }
  }
}
