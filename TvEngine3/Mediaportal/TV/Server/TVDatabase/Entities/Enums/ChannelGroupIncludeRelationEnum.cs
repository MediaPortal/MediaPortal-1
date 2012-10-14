using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum ChannelGroupIncludeRelationEnum
  {
    None = 0,
    GroupMaps = 1,
    KeywordMap = 2,
    GroupMapsChannel = 4,
    GroupMapsTuningDetails = 8,    
  }
}