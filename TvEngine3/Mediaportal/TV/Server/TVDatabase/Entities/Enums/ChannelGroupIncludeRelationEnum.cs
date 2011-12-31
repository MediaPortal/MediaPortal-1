using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum ChannelGroupIncludeRelationEnum
  {
    None,
    GroupMaps,
    KeywordMap,
    GroupMapsChannel,
    GroupMapsTuningDetails,    
  }
}