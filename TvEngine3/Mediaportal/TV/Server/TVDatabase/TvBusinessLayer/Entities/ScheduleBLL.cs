using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities
{
  public class ScheduleBLL
  {
    private Schedule _entity;    
    public ScheduleBLL(Schedule entity)
    {      
      _entity = entity;      
    }

    public Schedule Entity
    {
      get { return _entity; }
      set { _entity = value; }
    }

    public QualityType QualityType
    {
      get { return (QualityType)(_entity.quality / 10); }
      set
      {
        int type = ((int)value);
        _entity.quality = (type * 10) + (_entity.quality % 10);
      }
    }

    public VIDEOENCODER_BITRATE_MODE BitRateMode
    {
      get { return (VIDEOENCODER_BITRATE_MODE)(_entity.quality % 10); }
      set
      {
        int mode = ((int)value);
        _entity.quality = mode + ((_entity.quality / 10) * 10);
      }
    }

    /// <summary>
    /// Is schedule a manual one
    /// </summary>
    public bool IsManual
    {
      get
      {
        if (_entity.scheduleType != (int)ScheduleRecordingType.Once)
        {
          return false;
        }

        TimeSpan ts = (_entity.endTime - _entity.startTime);
        return (ts.TotalHours == 24);
      }
    }

    /// <summary>
    /// Checks whether this recording is finished and can be deleted
    /// 
    /// </summary>
    /// <returns>true:Recording is finished can be deleted
    ///          false:Recording is not done yet, or needs to be done multiple times
    /// </returns>
    public bool IsDone()
    {
      if (_entity.scheduleType != (int)ScheduleRecordingType.Once)
      {
        return false;
      }
      if (DateTime.Now > _entity.endTime)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Checks if the recording should record the specified tvprogram
    /// </summary>
    /// <param name="program">TVProgram to check</param>
    /// <returns>true if the specified tvprogram should be recorded</returns>
    /// <returns>filterCanceledRecordings (true/false)
    /// if true then  we'll return false if recording has been canceled for this program</returns>
    /// if false then we'll return true if recording has been not for this program</returns>
    public bool IsRecordingProgram(Program program, bool filterCanceledRecordings)
    {
      if (program == null)
      {
        return false;
      }
      ScheduleRecordingType scheduleRecordingType = (ScheduleRecordingType)_entity.scheduleType;
      switch (scheduleRecordingType)
      {
        case ScheduleRecordingType.Once:
          {
            if (program.StartTime == _entity.startTime && program.EndTime == _entity.endTime && program.IdChannel == _entity.idChannel)
            {
              if (filterCanceledRecordings)
              {
                if (_entity.CanceledSchedules.Count > 0)
                {
                  return false;
                }
              }
              return true;
            }
          }
          break;
        case ScheduleRecordingType.EveryTimeOnEveryChannel:
          if (program.Title == _entity.programName)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(GetSchedStartTimeForProg(program), program.IdChannel))
            {
              return false;
            }
            return true;
          }
          break;
        case ScheduleRecordingType.EveryTimeOnThisChannel:
          if (program.Title == _entity.programName && program.IdChannel == _entity.idChannel)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(GetSchedStartTimeForProg(program), program.IdChannel))
            {
              return false;
            }
            return true;
          }
          break;
        case ScheduleRecordingType.WeeklyEveryTimeOnThisChannel:
          if (program.Title == _entity.programName && program.IdChannel == _entity.idChannel &&
              _entity.startTime.DayOfWeek == program.StartTime.DayOfWeek)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime))
            {
              return false;
            }
            return true;
          }
          break;
        case ScheduleRecordingType.Daily:
          if (program.IdChannel == _entity.idChannel)
          {
            return IsRecordingProgramWithinTimeRange(program, filterCanceledRecordings);
          }
          break;
        case ScheduleRecordingType.WorkingDays:
          if (WeekEndTool.IsWorkingDay(program.StartTime.DayOfWeek))
          {
            if (program.IdChannel == _entity.idChannel)
            {
              return IsRecordingProgramWithinTimeRange(program, filterCanceledRecordings);
            }
          }
          break;

        case ScheduleRecordingType.Weekends:
          if (WeekEndTool.IsWeekend(program.StartTime.DayOfWeek))
          {
            if (program.IdChannel == _entity.idChannel)
            {
              return IsRecordingProgramWithinTimeRange(program, filterCanceledRecordings);
            }
          }
          break;

        case ScheduleRecordingType.Weekly:
          if (program.IdChannel == _entity.idChannel)
          {
            return (_entity.startTime.DayOfWeek == program.StartTime.DayOfWeek &&
                    IsRecordingProgramWithinTimeRange(program, filterCanceledRecordings));
          }
          break;
      }
      return false;
    }

    public bool IsPartialRecording(Program prg)
    {
      if (_entity.scheduleType == (int)ScheduleRecordingType.EveryTimeOnEveryChannel ||
          _entity.scheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel ||
          _entity.scheduleType == (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel)
      {
        return false;
      }

      DateTime schStart;
      DateTime schEnd;

      if (GetAdjustedScheduleTimeRange(prg, out schStart, out schEnd))
      {
        return (prg.StartTime < schStart || prg.EndTime > schEnd);
      }
      else
      {
        Log.Info(
          "IsPartialRecording: program ({0} {1} - {2} is not (at least partially) included in the schedule {3:hh:mm} - {4:hh:mm}",
          prg.Title, prg.StartTime, prg.EndTime, _entity.startTime, _entity.endTime);
        return false;
      }
    }

    /// <summary>
    /// checks if 2 schedules have a common Transponder
    /// depending on tuningdetails of their respective channels
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns>True if a common transponder exists</returns>
    public bool IsSameTransponder(Schedule schedule)
    {
      IList<TuningDetail> tuningList1 = _entity.Channel.TuningDetails;
      IList<TuningDetail> tuningList2 = schedule.Channel.TuningDetails;
      foreach (TuningDetail tun1 in tuningList1)
      {
        foreach (TuningDetail tun2 in tuningList2)
        {
          if (tun1.frequency == tun2.frequency)
          {
            return true;
          }
        }
      }
      return false;
    }

    public bool IsOverlapping(Schedule schedule)
    {
      DateTime Start1 = _entity.startTime.AddMinutes(-_entity.preRecordInterval);
      DateTime Start2 = schedule.startTime.AddMinutes(-schedule.preRecordInterval);
      DateTime End1 = _entity.endTime.AddMinutes(_entity.postRecordInterval);
      DateTime End2 = schedule.endTime.AddMinutes(schedule.postRecordInterval);

      // sch_1        s------------------------e
      // sch_2    ---------s-----------------------------
      // sch_2    s--------------------------------e
      // sch_2  ------------------e
      if ((Start2 >= Start1 && Start2 < End1) ||
          (Start2 <= Start1 && End2 >= End1) ||
          (End2 > Start1 && End2 <= End1))
      {
        return true;
      }
      return false;
    }

    public bool IsSerieIsCanceled(DateTime startTimeParam)
    {
      return _entity.CanceledSchedules.Any(schedule => schedule.CancelDateTime == startTimeParam);
    }

    public bool IsSerieIsCanceled(DateTime startTimeParam, int idChannel)
    {
      return _entity.CanceledSchedules.Any(schedule => schedule.CancelDateTime == startTimeParam && schedule.IdChannel == idChannel);
    }

    /// <summary>
    /// This takes a program as an argument and overlaps it with the schedule start time
    /// The date element of the start time of a schedule is the date of the first episode
    /// but for cancelling episodes of a time based schedule the cancellation row in the database 
    /// needs the start time of the episode rather than a program.
    /// eg. if a schedule is setup 20:00 until 21:00 every day starting on 1st April and on 3rd April the 
    /// there is no program for this period and user selects a program at 20:30 to 21:30 and asks to 
    /// cancel the epsiode, we need to return 3rd April 20:00 
    /// 
    /// If program does not fall within schedule (eg. you call with a program that starts 21:30)
    /// this will return that start time of the program
    /// </summary>
    /// <param name="prog">The program to check</param>
    /// <returns>The start time of the episode within a schedule that overlaps with program</returns>
    public DateTime GetSchedStartTimeForProg(Program prog)
    {
      DateTime dtSchedStart;
      DateTime dtSchedEnd;
      if (GetAdjustedScheduleTimeRange(prog, out dtSchedStart, out dtSchedEnd))
      {
        return dtSchedStart;
      }
      return prog.StartTime;
    }

    /// <summary>
    /// Try to offset this schedule's time range by an integral number
    /// of days so that it overlaps the <paramref name="program"/> time range.
    /// </summary>
    /// <param name="program">The program to use for adjusting the timerange</param>
    /// <param name="scheduleStart">The adjusted start date/time</param>
    /// <param name="scheduleEnd">The adjusted end date/time</param>
    /// <returns>True if a suitable adjustment was found</returns>
    private bool GetAdjustedScheduleTimeRange(Program program, out DateTime scheduleStart, out DateTime scheduleEnd)
    {
      scheduleStart = new DateTime(program.StartTime.Year, program.StartTime.Month, program.StartTime.Day,
                                   _entity.startTime.Hour, _entity.startTime.Minute, 0).AddDays(-1);
      scheduleEnd = scheduleStart.Add(_entity.endTime.Subtract(_entity.startTime));

      // Try to find on which day schedule should start in order to overlap the program
      // First try <program start day>-1
      // e.g. schedule 23:00-01:00, program 00:30-01:30
      if (program.StartTime >= scheduleEnd || program.EndTime <= scheduleStart)
      {
        // Then try <program start day>
        // e.g. schedule 18:00-20:00, program 17:30-19:30 (this is the most usual case)
        scheduleEnd = scheduleEnd.AddDays(1);
        scheduleStart = scheduleStart.AddDays(1);
        if (program.StartTime >= scheduleEnd || program.EndTime <= scheduleStart)
        {
          // Finally try <program start day>+1
          // e.g. schedule 00:30-01:30, program 23:00-01:00
          scheduleEnd = scheduleEnd.AddDays(1);
          scheduleStart = scheduleStart.AddDays(1);
          if (program.StartTime >= scheduleEnd || program.EndTime <= scheduleStart)
          {
            return false; // no overlap found
          }
        }
      }
      return true;
    }
    private bool IsRecordingProgramWithinTimeRange(Program program, bool filterCanceledRecordings)
    {
      DateTime scheduleStartTime;
      DateTime scheduleEndTime;

      bool isProgramWithinStartEndTimes = GetAdjustedScheduleTimeRange(program, out scheduleStartTime,
                                                                       out scheduleEndTime);
      bool isSerieNotCanceled = false;

      if (isProgramWithinStartEndTimes)
      {
        isSerieNotCanceled = !(filterCanceledRecordings && IsSerieIsCanceled(scheduleStartTime));
      }
      return isSerieNotCanceled;
    }   
  }
}
