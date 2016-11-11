using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum TunerRelation
  {
    None = 0,
    TuningDetailMappings = 1,
    TunerGroup = 2,
    TunerProperties = 4,
    AnalogTunerSettings = 8,
    TunerSatellites = 16
  }
}