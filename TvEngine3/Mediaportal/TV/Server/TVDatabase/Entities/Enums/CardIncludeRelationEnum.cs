using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum CardIncludeRelationEnum
  {
    None = 0,
    ChannelMaps = 1,
    ChannelMapsChannelTuningDetails = 2,
    CardGroupMaps = 4,
    DisEqcMotors = 8
  }
}
