using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum ChannelRelation
  {
    None = 0,
    TuningDetails = 1,
    ChannelMaps = 2,
    ChannelMapsTuner = 4,
    GroupMaps = 8,
    GroupMapsChannelGroup = 16,
    ChannelLinkMapsChannelLink = 32,
    ChannelLinkMapsChannelPortal = 64,
    Recordings = 128
  }
}