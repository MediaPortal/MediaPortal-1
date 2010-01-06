#region Copyright (C) 2007-2009 Team MediaPortal

/* 
 *	Copyright (C) 2007-2009 Team MediaPortal
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

#region Usings

using System;
using System.Collections.Generic;
using TvDatabase;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler.Interfaces;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Handles wakeup of the system for scheduled recordings
  /// </summary>
  public class ScheduledRecordingsHandler : IWakeupHandler
  {
    #region Variables

    private int _idleTimeout = 5;

    #endregion

    #region Ctor

    public ScheduledRecordingsHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
      {
        IPowerScheduler ips = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
        if (ips != null)
        {
          ips.OnPowerSchedulerEvent += ScheduledRecordingsHandler_OnPowerSchedulerEvent;
          if (ips.Settings != null)
          {
            _idleTimeout = ips.Settings.IdleTimeout;
          }
        }
      }
    }

    #endregion

    #region IWakeupHandler implementation

    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Schedule recSchedule = null;
      DateTime scheduleWakeupTime = DateTime.MaxValue;
      DateTime nextWakeuptime = DateTime.MaxValue;
      foreach (Schedule schedule in Schedule.ListAll())
      {
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        List<Schedule> schedules = layer.GetRecordingTimes(schedule);
        if (schedules.Count > 0)
        {
          int i = 0;
          // Take first occurrence of this schedule if not a canceled serie
          while (i < schedules.Count)
          {
            recSchedule = schedules[i];
            if (!recSchedule.IsSerieIsCanceled(recSchedule.StartTime))
              break;
            i++;
          }
          if (recSchedule != null)
          {
            scheduleWakeupTime = recSchedule.StartTime.AddMinutes(-recSchedule.PreRecordInterval);
          }
        }
        if (recSchedule == null)
        {
          // manually determine schedule's wakeup time of no guide data is present
          scheduleWakeupTime = GetWakeupTime(schedule);
        }
        if (scheduleWakeupTime < nextWakeuptime && scheduleWakeupTime >= earliestWakeupTime)
          nextWakeuptime = scheduleWakeupTime;
      }
      return nextWakeuptime;
    }

    public string HandlerName
    {
      get { return "ScheduledRecordingsHandler"; }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Handles settings changed events
    /// </summary>
    /// <param name="args">PowerScheduler event arguments</param>
    private void ScheduledRecordingsHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.SettingsChanged:
          PowerSettings settings = args.GetData<PowerSettings>();
          if (settings != null)
          {
            _idleTimeout = settings.IdleTimeout;
          }
          break;
      }
    }

    /// <summary>
    /// GetWakeupTime determines the wakeup time for a Schedule when no guide data is present
    /// Note that this obviously only works for the following ScheduleRecordingsType's:
    /// - Once
    /// - Daily
    /// - Weekends
    /// - WorkingDays
    /// - Weekly
    /// </summary>
    /// <param name="schedule">Schedule to determine next wakeup time for</param>
    /// <returns>DateTime indicating the wakeup time for this Schedule</returns>
    private static DateTime GetWakeupTime(Schedule schedule)
    {
      ScheduleRecordingType type = (ScheduleRecordingType)schedule.ScheduleType;
      DateTime now = DateTime.Now;
      DateTime start = new DateTime(now.Year, now.Month, now.Day, schedule.StartTime.Hour, schedule.StartTime.Minute,
                                    schedule.StartTime.Second);
      DateTime stop = new DateTime(now.Year, now.Month, now.Day, schedule.EndTime.Hour, schedule.EndTime.Minute,
                                   schedule.EndTime.Second);
      WeekEndTool weekEndTool = Setting.GetWeekEndTool();
      switch (type)
      {
        case ScheduleRecordingType.Once:
          return schedule.StartTime.AddMinutes(-schedule.PreRecordInterval);
        case ScheduleRecordingType.Daily:
          // if schedule was already due today, then run tomorrow
          if (now > stop.AddMinutes(schedule.PostRecordInterval))
            start = start.AddDays(1);
          return start.AddMinutes(-schedule.PreRecordInterval);
        case ScheduleRecordingType.Weekends:
          // check if it's a weekend currently
          if (weekEndTool.IsWeekend(now.DayOfWeek))
          {
            // check if schedule has been due already today
            if (now > stop.AddMinutes(schedule.PostRecordInterval))
            {
              // if so, add appropriate days to wakeup time
              start = weekEndTool.IsFirstWeekendDay(now.DayOfWeek) ? start.AddDays(1) : start.AddDays(6);
            }
          }
          else
          {
            // it's not a weekend so calculate number of days to add to current time
            int days = (int)weekEndTool.FirstWeekendDay - (int)now.DayOfWeek;
            start = start.AddDays(days);
          }
          return start.AddMinutes(-schedule.PreRecordInterval);
        case ScheduleRecordingType.WorkingDays:
          // check if current time is in weekend; if so add appropriate number of days
          if (now.DayOfWeek == weekEndTool.FirstWeekendDay)
            start = start.AddDays(2);
          else if (now.DayOfWeek == weekEndTool.SecondWeekendDay)
            start = start.AddDays(1);
          else
          {
            // current time is on a working days; check if schedule has already been due
            if (now > stop.AddMinutes(schedule.PostRecordInterval))
            {
              // schedule has been due, so add appropriate number of days
              start = now.DayOfWeek < (weekEndTool.FirstWeekendDay - 1) ? start.AddDays(1) : start.AddDays(3);
            }
          }
          return start.AddMinutes(-schedule.PreRecordInterval);
        case ScheduleRecordingType.Weekly:
          // check if current day of week is same as schedule's day of week
          if (now.DayOfWeek == schedule.StartTime.DayOfWeek)
          {
            // check if schedule has been due
            if (now > stop.AddMinutes(schedule.PostRecordInterval))
            {
              // schedule has been due, so record again next week
              start = start.AddDays(7);
            }
          }
          else
          {
            // current day of week isn't schedule's day of week, so
            // add appropriate number of days
            if (now.DayOfWeek < schedule.StartTime.DayOfWeek)
            {
              // schedule is due this week
              int days = schedule.StartTime.DayOfWeek - now.DayOfWeek;
              start = start.AddDays(days);
            }
            else
            {
              // schedule should start next week
              int days = 7 - (now.DayOfWeek - schedule.StartTime.DayOfWeek);
              start = start.AddDays(days);
            }
          }
          return start.AddMinutes(-schedule.PreRecordInterval);
      }
      // other recording types cannot be determined manually (every time on ...)
      return DateTime.MaxValue;
    }

    #endregion
  }
}