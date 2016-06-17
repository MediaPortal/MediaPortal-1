using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Enums
{
  [Flags]
  public enum TunerSatelliteRelation
  {
    None = 0,
    Satellite = 1,
    LnbType = 2,
    Tuner = 4
  }
}