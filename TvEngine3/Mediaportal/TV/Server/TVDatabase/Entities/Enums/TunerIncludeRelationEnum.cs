using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum TunerIncludeRelationEnum
  {
    None = 0,
    ChannelMaps = 1,
    ChannelMapsChannelTuningDetails = 2,
    TunerGroup = 4,
    DiseqcMotors = 8,
    TunerProperties = 16,
    AnalogTunerSettings = 32
  }
}
