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

using System;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

using TvDatabase;
using Gentle.Common;
using Gentle.Framework;

namespace TvPlugin
{
  /// <summary>
  /// Helper class which can be used to determine which tv program is
  /// running at a specific time and date
  /// <seealso cref="MediaPortal.TV.Database.Program"/>
  /// </summary>
  public class TVUtil
  {
    int _days;
    public TVUtil()
    {
      _days = 15;
    }

    public TVUtil(int days)
    {
      _days = days;
    }

    #region IDisposable Members


    #endregion

    public List<Schedule> GetRecordingTimes(Schedule rec)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      List<Schedule> recordings = new List<Schedule>();

      DateTime dtDay = DateTime.Now;
      if (rec.ScheduleType == (int)ScheduleRecordingType.Once)
      {
        recordings.Add(rec);
        return recordings;
      }

      if (rec.ScheduleType == (int)ScheduleRecordingType.Daily)
      {
        for (int i = 0; i < _days; ++i)
        {
          Schedule recNew = rec.Clone();
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute, 0);
          if (rec.EndTime.Day > rec.StartTime.Day)
            dtDay = dtDay.AddDays(1);
          recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
          if (rec.EndTime.Day > rec.StartTime.Day)
            dtDay = dtDay.AddDays(-1);
          recNew.Series = true;
          if (recNew.StartTime >= DateTime.Now)
          {
            if (rec.IsSerieIsCanceled(recNew.StartTime))
              recNew.Canceled = recNew.StartTime;
            recordings.Add(recNew);
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (rec.ScheduleType == (int)ScheduleRecordingType.WorkingDays)
      {
        for (int i = 0; i < _days; ++i)
        {
          if (dtDay.DayOfWeek != DayOfWeek.Saturday && dtDay.DayOfWeek != DayOfWeek.Sunday)
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
              dtDay = dtDay.AddDays(1);
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
              dtDay = dtDay.AddDays(-1);
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
              recNew.Canceled = recNew.StartTime;
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (rec.ScheduleType == (int)ScheduleRecordingType.Weekends)
      {
        IList<Program> progList;
        progList = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(_days), rec.ProgramName, rec.ReferencedChannel());

        foreach (Program prog in progList)
        {
          if ((rec.IsRecordingProgram(prog, false)) &&
                      (prog.StartTime.DayOfWeek == DayOfWeek.Saturday || prog.StartTime.DayOfWeek == DayOfWeek.Sunday))
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = prog.StartTime;
            recNew.EndTime = prog.EndTime;
            recNew.Series = true;

            if (rec.IsSerieIsCanceled(recNew.StartTime))
              recNew.Canceled = recNew.StartTime;
            recordings.Add(recNew);
          }

        }
        return recordings;
      }
      if (rec.ScheduleType == (int)ScheduleRecordingType.Weekly)
      {
        for (int i = 0; i < _days; ++i)
        {
          if (dtDay.DayOfWeek == rec.StartTime.DayOfWeek)
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
              dtDay = dtDay.AddDays(1);
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
              dtDay = dtDay.AddDays(-1);
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
              recNew.Canceled = recNew.StartTime;
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }


      IList<Program> programs;
      if (rec.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel)
      {
        Log.Debug("get {0} {1} EveryTimeOnThisChannel", rec.ProgramName, rec.ReferencedChannel().DisplayName);
        programs = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(_days), rec.ProgramName, rec.ReferencedChannel());
      }
      else
      {
        Log.Debug("get {0} EveryTimeOnAllChannels", rec.ProgramName );

        programs = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(_days), rec.ProgramName, null);
      }
      foreach (Program prog in programs)
      {
        if (rec.IsRecordingProgram(prog, false))
        {
          Schedule recNew = rec.Clone();
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.IdChannel= prog.IdChannel;
          recNew.StartTime = prog.StartTime;
          recNew.EndTime = prog.EndTime;
          recNew.Series = true;
          if (rec.IsSerieIsCanceled(recNew.StartTime))
            recNew.Canceled = recNew.StartTime;
          recordings.Add(recNew);
        }
      }
      return recordings;
    }

    //bool _isSeries = false;
  }
}
