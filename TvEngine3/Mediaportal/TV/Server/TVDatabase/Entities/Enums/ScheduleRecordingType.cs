using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  public enum ScheduleRecordingType
  {
    Once,
    Daily,
    Weekly,
    EveryTimeOnThisChannel,
    EveryTimeOnEveryChannel,
    Weekends,
    WorkingDays,
    WeeklyEveryTimeOnThisChannel
  }
}
