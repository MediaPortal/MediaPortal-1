using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediaportal.TV.Server.TVService.Interfaces.Enums
{
  [Serializable]
  public enum UserType
  {
    Normal,
    EPG,
    Scheduler,
    Scanner
  }
}
