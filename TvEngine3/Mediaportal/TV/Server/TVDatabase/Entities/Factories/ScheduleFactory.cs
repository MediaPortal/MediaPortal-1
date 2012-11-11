using System;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class ScheduleFactory
  {
    public static DateTime MinSchedule = new DateTime(2000, 1, 1);
    public static readonly int HighestPriority = Int32.MaxValue;

    public static Schedule Clone(Schedule source)
    {
      return CloneHelper.DeepCopy<Schedule>(source);

      /*Schedule schedule = new Schedule();
                                       IdChannel, idParentSchedule, scheduleType, ProgramName, StartTime, EndTime,
                                       MaxAirings, Priority,
                                       Directory, Quality, KeepMethod, KeepDate, PreRecordInterval, PostRecordInterval,
                                       Canceled);

      schedule.idChannel = source.idChannel;
      schedule.idParentSchedule = source.idParentSchedule;
      schedule.scheduleType = source.scheduleType;
      schedule.programName = source.programName;
      schedule.startTime = source.startTime;
      schedule.endTime = source.endTime;
      schedule.maxAirings = schedule.maxAirings;
      schedule.priority = source.priority;
      schedule.directory = source.directory;
      schedule.quality = source.quality;
      schedule.keepMethod = source.keepMethod;w
      schedule.

      schedule.series = source.series;
      schedule.id_Schedule = source.id_Schedule;

      return schedule;*/
    }

    public static Schedule CreateSchedule(int idChannel, string programName, DateTime startTime, DateTime endTime)
    {
      var schedule = new Schedule
                       {
                         IdChannel = idChannel,
                         IdParentSchedule = null,
                         ProgramName = programName,
                         Canceled = MinSchedule,
                         Directory = "",
                         EndTime = endTime,
                         KeepDate = MinSchedule,
                         KeepMethod = (int) KeepMethodType.UntilSpaceNeeded,
                         MaxAirings = Int32.MaxValue,
                         PostRecordInterval = 0,
                         PreRecordInterval = 0,
                         Priority = 0,
                         Quality = 0,
                         Series = false,
                         StartTime = startTime
                       };


      return schedule;
    }

   
     
      
  }
}
