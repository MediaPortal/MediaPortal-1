#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

#region Usings

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using TvControl;
using TvDatabase;
using TvEngine.Events;
using TvEngine.PowerScheduler.Interfaces;
using TvService;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Handles wakeup of the system for scheduled recordings
  /// </summary>
  public class ScheduledRecordingsWakeupHandler : IWakeupHandler
  {
    #region Variables

    /// <summary>
    /// Reference to tvservice's TVController
    /// </summary>
    private TVController _controller;

    #endregion

    #region Constructor

    public ScheduledRecordingsWakeupHandler(IController controller)
    {
      // Save controller
      _controller = controller as TVController;

      // Register handler for TV server events
      _controller.OnTvServerEvent += OnTvServerEvent;
    }

    ~ScheduledRecordingsWakeupHandler()
    {
      // Unregister handler for TV server events
      _controller.OnTvServerEvent -= OnTvServerEvent;
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
      get { return "Scheduled Recordings"; }
    }

    #endregion

    #region Private methods

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
          if (WeekEndTool.IsWeekend(now.DayOfWeek))
          {
            // check if schedule has been due already today
            if (now > stop.AddMinutes(schedule.PostRecordInterval))
            {
              // if so, add appropriate days to wakeup time
              start = WeekEndTool.IsFirstWeekendDay(now.DayOfWeek) ? start.AddDays(1) : start.AddDays(6);
            }
          }
          else
          {
            // it's not a weekend so calculate number of days to add to current time
            int days = (int)WeekEndTool.FirstWeekendDay - (int)now.DayOfWeek;
            start = start.AddDays(days);
          }
          return start.AddMinutes(-schedule.PreRecordInterval);
        case ScheduleRecordingType.WorkingDays:
          // check if current time is in weekend; if so add appropriate number of days
          if (now.DayOfWeek == WeekEndTool.FirstWeekendDay)
            start = start.AddDays(2);
          else if (now.DayOfWeek == WeekEndTool.SecondWeekendDay)
            start = start.AddDays(1);
          else
          {
            // current time is on a working days; check if schedule has already been due
            if (now > stop.AddMinutes(schedule.PostRecordInterval))
            {
              // schedule has been due, so add appropriate number of days
              start = now.DayOfWeek < (WeekEndTool.FirstWeekendDay - 1) ? start.AddDays(1) : start.AddDays(3);
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

    /// <summary>
    /// Triggers PowerScheduler on schedule changes to set the next wakeup time
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      TvServerEventArgs tvArgs = eventArgs as TvServerEventArgs;
      if (eventArgs == null)
        return;
      if (tvArgs != null)
      {
        switch (tvArgs.EventType)
        {
          case TvServerEventType.ScheduledAdded:
          case TvServerEventType.ScheduleDeleted:

            // Trigger PowerScheduler's StandbyWakeupThread to set the next wakeup time
            EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "TvEngine.PowerScheduler.StandbyWakeupTriggered");
            eventWaitHandle.Set();
            break;
        }
      }
    }

    #endregion
  }
}