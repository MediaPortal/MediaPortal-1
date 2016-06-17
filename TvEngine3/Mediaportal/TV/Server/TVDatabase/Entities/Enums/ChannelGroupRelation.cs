using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum ChannelGroupRelation
  {
    None = 0,
    GroupMaps = 1,
    GroupMapsChannel = 2,
    GroupMapsTuningDetails = 4
  }
}