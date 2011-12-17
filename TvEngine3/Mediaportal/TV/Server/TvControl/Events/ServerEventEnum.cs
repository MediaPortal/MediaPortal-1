using System;

namespace Mediaportal.TV.Server.TVControl.Events
{
  [Flags]
  public enum ServerEventEnum
  {
    None,
    TvServerEventEnum,
    HeartbeatEventEnum,
    CiMenuEventEnum
  }
}