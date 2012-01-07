using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum CardIncludeRelationEnum
  {
    None,
    ChannelMaps,
    ChannelMapsChannelTuningDetails,
    CardGroupMaps,
    DisEqcMotors
  }
}
