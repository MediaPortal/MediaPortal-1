using System;
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class ScheduleFactory
  {
    public static DateTime MinSchedule = new DateTime(2000, 1, 1);
    public static readonly int HighestPriority = Int32.MaxValue;

    public static Schedule Clone(Schedule source)
    {
      return CloneHelper.DeepCopy<Schedule>(source);
    }

    public static Schedule CreateSchedule(int idChannel, string programName, DateTime startTime, DateTime endTime)
    {
      var schedule = new Schedule
      {
        IdChannel = idChannel,
        IdParentSchedule = null,
        ProgramName = programName,
        Canceled = MinSchedule,
        Directory = string.Empty,
        EndTime = endTime,
        KeepDate = MinSchedule,
        KeepMethod = (int)RecordingKeepMethod.UntilSpaceNeeded,
        MaxAirings = Int32.MaxValue,
        PostRecordInterval = null,
        PreRecordInterval = null,
        Priority = 0,
        Quality = 0,
        Series = false,
        StartTime = startTime
      };
      return schedule;
    }
  }
}