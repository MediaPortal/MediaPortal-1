using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities
{
  public class ScheduleBLL
  {
    private readonly Schedule _entity;

    /// <remarks>
    /// The provided schedule instance must include the associated canceled schedules and channel tuning details.
    /// </remarks>
    public ScheduleBLL(Schedule entity)
    {
      _entity = entity;
    }

    public Schedule Entity
    {
      get { return _entity; }
    }

    // Stored as bits 3 to 16 (zero indexed, bit 0 is LSB) in the Quality property.
    public QualityType QualityType
    {
      // requires: [nothing]
      get
      {
        int value = (_entity.Quality >> 3) & 0x3fff;
        if (Enum.IsDefined(typeof(QualityType), value))
        {
          return (QualityType)value;
        }
        return QualityType.Custom;
      }
      set
      {
        if (value != QualityType.Custom)
        {
          _entity.Quality = ((int)value << 3) | (_entity.Quality & 0x7ffe0007);
        }
      }
    }

    // Stored as bits 3 to 9 (zero indexed, bit 0 is LSB) in the Quality property.
    public int AverageBitRate
    {
      // requires: [nothing]
      get
      {
        return (_entity.Quality >> 3) & 0x7f;
      }
      set
      {
        _entity.Quality = (value << 3) | (_entity.Quality & 0x7ffffc07);
      }
    }

    // Stored as bits 10 to 16 (zero indexed, bit 0 is LSB) in the Quality property.
    public int PeakBitRate
    {
      // requires: [nothing]
      get
      {
        return (_entity.Quality >> 10) & 0x7f;
      }
      set
      {
        _entity.Quality = (value << 10) | (_entity.Quality & 0x7ffe03ff);
      }
    }

    // Stored in the 3 least significant bits of the Quality property.
    public EncodeMode BitRateMode
    {
      // requires: [nothing]
      get
      {
        return (EncodeMode)(_entity.Quality & 7);
      }
      set
      {
        _entity.Quality = (_entity.Quality & 0x7ffffff8) | (int)value;
      }
    }

    /// <summary>
    /// Is schedule a manual one
    /// </summary>
    public bool IsManual
    {
      get
      {
        // requires: [nothing]
        if (_entity.ScheduleType != (int)ScheduleRecordingType.Once)
        {
          return false;
        }

        TimeSpan ts = (_entity.EndTime - _entity.StartTime);
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
      // requires: [nothing]
      return _entity.ScheduleType == (int)ScheduleRecordingType.Once && DateTime.Now > _entity.EndTime;
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
      // requires: CanceledSchedules
      if (program == null)
      {
        return false;
      }
      switch ((ScheduleRecordingType)_entity.ScheduleType)
      {
        case ScheduleRecordingType.Once:
          if (program.StartTime == _entity.StartTime && program.EndTime == _entity.EndTime && program.IdChannel == _entity.IdChannel)
          {
            return !filterCanceledRecordings || _entity.CanceledSchedules.Count <= 0;
          }
          break;
        case ScheduleRecordingType.EveryTimeOnEveryChannel:
          if (program.Title == _entity.ProgramName)
          {
            return !filterCanceledRecordings || !IsSerieIsCanceled(GetSchedStartTimeForProg(program), program.IdChannel);
          }
          break;
        case ScheduleRecordingType.EveryTimeOnThisChannel:
          if (program.Title == _entity.ProgramName && program.IdChannel == _entity.IdChannel)
          {
            return !filterCanceledRecordings || !IsSerieIsCanceled(GetSchedStartTimeForProg(program), program.IdChannel);
          }
          break;
        case ScheduleRecordingType.WeeklyEveryTimeOnThisChannel:
          if (program.Title == _entity.ProgramName && program.IdChannel == _entity.IdChannel &&
              _entity.StartTime.DayOfWeek == program.StartTime.DayOfWeek)
          {
            return !filterCanceledRecordings || !IsSerieIsCanceled(program.StartTime);
          }
          break;
        case ScheduleRecordingType.Daily:
          if (program.IdChannel == _entity.IdChannel)
          {
            return IsRecordingProgramWithinTimeRange(program, filterCanceledRecordings);
          }
          break;
        case ScheduleRecordingType.WorkingDays:
          if (WeekEndTool.IsWorkingDay(program.StartTime.DayOfWeek) && program.IdChannel == _entity.IdChannel)
          {
            return IsRecordingProgramWithinTimeRange(program, filterCanceledRecordings);
          }
          break;
        case ScheduleRecordingType.Weekends:
          if (WeekEndTool.IsWeekend(program.StartTime.DayOfWeek) && program.IdChannel == _entity.IdChannel)
          {
            return IsRecordingProgramWithinTimeRange(program, filterCanceledRecordings);
          }
          break;
        case ScheduleRecordingType.Weekly:
          if (program.IdChannel == _entity.IdChannel)
          {
            return (_entity.StartTime.DayOfWeek == program.StartTime.DayOfWeek &&
                    IsRecordingProgramWithinTimeRange(program, filterCanceledRecordings));
          }
          break;
      }
      return false;
    }

    public bool IsPartialRecording(Program prg)
    {
      // requires: [nothing]
      if (_entity.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnEveryChannel ||
          _entity.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel ||
          _entity.ScheduleType == (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel)
      {
        return false;
      }

      DateTime schStart;
      DateTime schEnd;
      if (GetAdjustedScheduleTimeRange(prg, out schStart, out schEnd))
      {
        return (prg.StartTime < schStart || prg.EndTime > schEnd);
      }

      this.LogInfo(
        "IsPartialRecording: program ({0} {1} - {2} is not (at least partially) included in the schedule {3:hh:mm} - {4:hh:mm}",
        prg.Title, prg.StartTime, prg.EndTime, _entity.StartTime, _entity.EndTime);
      return false;
    }

    /// <summary>
    /// checks if 2 schedules have a common Transponder
    /// depending on tuningdetails of their respective channels
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns>True if a common transponder exists</returns>
    public bool IsSameTransponder(Schedule schedule)
    {
      // requires: ChannelTuningDetails
      IList<TuningDetail> tuningDetailList1 = _entity.Channel.TuningDetails;
      IList<TuningDetail> tuningDetailList2 = schedule.Channel.TuningDetails;
      foreach (TuningDetail td1 in tuningDetailList1)
      {
        IChannel c1 = TuningDetailManagement.GetTuningChannel(td1);
        foreach (TuningDetail td2 in tuningDetailList2)
        {
          if (!c1.IsDifferentTransmitter(TuningDetailManagement.GetTuningChannel(td2)))
          {
            return true;
          }
        }
      }
      return false;
    }

    public bool IsOverlapping(Schedule schedule, int defaultPreRecordInterval, int defaultPostRecordInterval)
    {
      // requires: [nothing]
      DateTime start1 = _entity.StartTime.AddMinutes(-(_entity.PreRecordInterval ?? defaultPreRecordInterval));
      DateTime start2 = schedule.StartTime.AddMinutes(-(schedule.PreRecordInterval ?? defaultPreRecordInterval));
      DateTime end1 = _entity.EndTime.AddMinutes(_entity.PostRecordInterval ?? defaultPostRecordInterval);
      DateTime end2 = schedule.EndTime.AddMinutes(schedule.PostRecordInterval ?? defaultPostRecordInterval);

      // sch_1        s------------------------e
      // sch_2    ---------s-----------------------------
      // sch_2    s--------------------------------e
      // sch_2  ------------------e
      if ((start2 >= start1 && start2 < end1) ||
          (start2 <= start1 && end2 >= end1) ||
          (end2 > start1 && end2 <= end1))
      {
        return true;
      }
      return false;
    }

    public bool IsSerieIsCanceled(DateTime startTimeParam)
    {
      // requires: CanceledSchedules
      return _entity.CanceledSchedules.Any(schedule => schedule.CancelDateTime == startTimeParam);
    }

    public bool IsSerieIsCanceled(DateTime startTimeParam, int idChannel)
    {
      // requires: CanceledSchedules
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
      // requires: [nothing]
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
      // requires: [nothing]
      scheduleStart = new DateTime(program.StartTime.Year, program.StartTime.Month, program.StartTime.Day,
                                   _entity.StartTime.Hour, _entity.StartTime.Minute, 0).AddDays(-1);
      scheduleEnd = scheduleStart.Add(_entity.EndTime.Subtract(_entity.StartTime));

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
      // requires: CanceledSchedules
      DateTime scheduleStartTime;
      DateTime scheduleEndTime;
      if (GetAdjustedScheduleTimeRange(program, out scheduleStartTime, out scheduleEndTime))
      {
        return !(filterCanceledRecordings && IsSerieIsCanceled(scheduleStartTime));
      }
      return false;
    }
  }
}
