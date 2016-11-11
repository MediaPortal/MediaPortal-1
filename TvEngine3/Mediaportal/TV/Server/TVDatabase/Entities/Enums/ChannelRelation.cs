using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum ChannelRelation
  {
    None = 0,
    TuningDetails = 1,
    ChannelGroupMappings = 2,
    ChannelGroupMappingsChannelGroup = 4,
    ChannelLinkMapsChannelLink = 8,
    ChannelLinkMapsChannelPortal = 16,
    Recordings = 32
  }
}