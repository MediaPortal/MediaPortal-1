using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum TuningDetailRelation
  {
    None = 0,
    Channel = 1,
    Satellite = 2
  }
}