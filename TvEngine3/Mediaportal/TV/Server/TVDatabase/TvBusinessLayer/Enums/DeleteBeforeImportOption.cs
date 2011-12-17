using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  public enum DeleteBeforeImportOption
  {
    None,
    OverlappingPrograms,
    ProgramsOnSameChannel,
    //All
  }
}
