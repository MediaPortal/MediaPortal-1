using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum ScheduleIncludeRelationEnum
  {
    None = 0,
    Channel = 1,
    ChannelTuningDetails = 2,
    Recordings = 4,
    Schedules = 16,
    ConflictingSchedules = 32,
    Conflicts = 64,
    ParentSchedule = 128,
    CanceledSchedules = 256
  }
}
