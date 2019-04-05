using System;

namespace Mediaportal.TV.Server.TVService.Interfaces.Enums
{
  [Serializable]
  public enum UserType
  {
    Normal,
    EpgGrabber,
    Scheduler,
    ChannelScanner
  }
}