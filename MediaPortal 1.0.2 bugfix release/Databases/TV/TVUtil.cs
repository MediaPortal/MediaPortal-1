#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System;
using System.Collections.Generic;

namespace MediaPortal.TV.Database
{
  /// <summary>
  /// Helper class which can be used to determine which tv program is
  /// running at a specific time and date
  /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
  /// </summary>
  public class TVUtil
  {
    private int _days;

    public TVUtil()
    {
      _days = 1;
    }

    public TVUtil(int days)
    {
      _days = days;
    }

    #region IDisposable Members

    #endregion

    public List<TVRecording> GetRecordingTimes(TVRecording rec)
    {
      List<TVRecording> recordings = new List<TVRecording>();

      DateTime dtDay = DateTime.Now;
      if (rec.RecType == TVRecording.RecordingType.Once)
      {
        recordings.Add(rec);
        return recordings;
      }

      if (rec.RecType == TVRecording.RecordingType.Daily)
      {
        for (int i = 0; i < _days; ++i)
        {
          TVRecording recNew = new TVRecording(rec);
          recNew.RecType = TVRecording.RecordingType.Once;
          recNew.Start =
            Util.Utils.datetolong(new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour,
                                               rec.StartTime.Minute, 0));
          if (rec.EndTime.Day > rec.StartTime.Day)
          {
            dtDay = dtDay.AddDays(1);
          }
          recNew.End =
            Util.Utils.datetolong(new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute,
                                               0));
          if (rec.EndTime.Day > rec.StartTime.Day)
          {
            dtDay = dtDay.AddDays(-1);
          }
          recNew.Series = true;
          if (recNew.StartTime >= DateTime.Now)
          {
            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.Start;
            }
            recordings.Add(recNew);
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (rec.RecType == TVRecording.RecordingType.WeekDays)
      {
        for (int i = 0; i < _days; ++i)
        {
          if (dtDay.DayOfWeek != DayOfWeek.Saturday && dtDay.DayOfWeek != DayOfWeek.Sunday)
          {
            TVRecording recNew = new TVRecording(rec);
            recNew.RecType = TVRecording.RecordingType.Once;
            recNew.Start =
              Util.Utils.datetolong(new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour,
                                                 rec.StartTime.Minute, 0));
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.End =
              Util.Utils.datetolong(new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour,
                                                 rec.EndTime.Minute, 0));
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.Start;
            }
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (rec.RecType == TVRecording.RecordingType.WeekEnds)
      {
        List<TVProgram> progList = new List<TVProgram>();
        TVDatabase.SearchMinimalPrograms(Util.Utils.datetolong(dtDay), Util.Utils.datetolong(dtDay.AddDays(_days)),
                                         ref progList, 3, rec.Title, rec.Channel);

        foreach (TVProgram prog in progList)
        {
          if ((rec.IsRecordingProgram(prog, false)) &&
              (prog.StartTime.DayOfWeek == DayOfWeek.Saturday || prog.StartTime.DayOfWeek == DayOfWeek.Sunday))
          {
            TVRecording recNew = new TVRecording(rec);
            recNew.RecType = TVRecording.RecordingType.Once;
            recNew.Start = Util.Utils.datetolong(prog.StartTime);
            recNew.End = Util.Utils.datetolong(prog.EndTime);
            recNew.Series = true;

            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.Start;
            }
            recordings.Add(recNew);
          }
        }
        return recordings;
      }
      if (rec.RecType == TVRecording.RecordingType.Weekly)
      {
        for (int i = 0; i < _days; ++i)
        {
          if (dtDay.DayOfWeek == rec.StartTime.DayOfWeek)
          {
            TVRecording recNew = new TVRecording(rec);
            recNew.RecType = TVRecording.RecordingType.Once;
            recNew.Start =
              Util.Utils.datetolong(new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour,
                                                 rec.StartTime.Minute, 0));
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.End =
              Util.Utils.datetolong(new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour,
                                                 rec.EndTime.Minute, 0));
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.Start;
            }
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      List<TVProgram> programs = new List<TVProgram>();
      if (rec.RecType == TVRecording.RecordingType.EveryTimeOnThisChannel)
      {
        TVDatabase.SearchMinimalPrograms(Util.Utils.datetolong(dtDay), Util.Utils.datetolong(dtDay.AddDays(_days)),
                                         ref programs, 3, rec.Title, rec.Channel);
      }
      else
      {
        TVDatabase.SearchMinimalPrograms(Util.Utils.datetolong(dtDay), Util.Utils.datetolong(dtDay.AddDays(_days)),
                                         ref programs, 3, rec.Title, string.Empty);
      }
      foreach (TVProgram prog in programs)
      {
        if (rec.IsRecordingProgram(prog, false))
        {
          TVRecording recNew = new TVRecording(rec);
          recNew.RecType = TVRecording.RecordingType.Once;
          recNew.Channel = prog.Channel;
          recNew.Start = Util.Utils.datetolong(prog.StartTime);
          recNew.End = Util.Utils.datetolong(prog.EndTime);
          recNew.Series = true;
          if (rec.IsSerieIsCanceled(recNew.StartTime))
          {
            recNew.Canceled = recNew.Start;
          }
          recordings.Add(recNew);
        }
      }
      return recordings;
    }
  }
}