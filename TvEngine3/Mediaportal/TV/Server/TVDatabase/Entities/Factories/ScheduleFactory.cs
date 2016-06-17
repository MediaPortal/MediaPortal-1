using System;
using System.Data.SqlTypes;
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class ScheduleFactory
  {
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
        Canceled = SqlDateTime.MinValue.Value,
        Directory = string.Empty,
        EndTime = endTime,
        KeepDate = SqlDateTime.MinValue.Value,
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