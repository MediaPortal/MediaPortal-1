using System;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.EPG
{
  public class EpgHole
  {
    public DateTime start;
    public DateTime end;

    public EpgHole(DateTime start, DateTime end)
    {
      this.start = start;
      this.end = end;
    }

    public bool FitsInHole(DateTime startParam, DateTime endParam)
    {
      return (startParam >= start && endParam <= end);
    }    
  }
}