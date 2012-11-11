using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum ChannelIncludeRelationEnum
  {
    None = 0,
    TuningDetails = 1,
    ChannelMaps = 2,
    ChannelMapsCard = 4,
    GroupMaps = 16,
    GroupMapsChannelGroup = 32,
    ChannelLinkMapsChannelLink = 64,
    ChannelLinkMapsChannelPortal = 128
    } 
}
