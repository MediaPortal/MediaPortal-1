using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum ChannelGroupRelation
  {
    None = 0,
    ChannelMappings = 1,
    ChannelMappingsChannel = 2,
    ChannelMappingsTuningDetails = 4
  }
}