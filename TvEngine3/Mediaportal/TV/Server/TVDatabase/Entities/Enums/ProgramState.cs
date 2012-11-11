using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum ProgramState
  {
    None = 0,
    Notify = 1,
    RecordOnce = 2,
    RecordSeries = 4,
    RecordManual = 8,
    Conflict = 16,
    RecordOncePending = 32, // used to indicate recording icon on tvguide, even though it hasnt begun yet.
    RecordSeriesPending = 64, // used to indicate recording icon on tvguide, even though it hasnt begun yet.
    PartialRecordSeriesPending = 128
    // used to indicate partial recording icon on tvguide, even though it hasnt begun yet.
  }
}
