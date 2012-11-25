using System;
using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.EPG
{
  public class EpgHoleCollection : List<EpgHole>
  {
    public bool FitsInAnyHole(DateTime start, DateTime end)
    {
      foreach (EpgHole hole in this)
      {
        if (hole.FitsInHole(start, end))
        {
          return true;
        }
      }
      return false;
    }
  }
}