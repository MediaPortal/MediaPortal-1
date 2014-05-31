using System;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  internal class SatelliteContext : IComparable<SatelliteContext>
  {
    public string SatelliteName;
    public string Url;
    public string FileName;
    public Satellite Satellite;

    public SatelliteContext()
    {
      Url = "";
      Satellite = null;
      FileName = "";
      SatelliteName = "";
    }

    public SatelliteContext(Satellite satellite)
    {
      Url = satellite.TransponderListUrl;
      Satellite = satellite;
      FileName = satellite.LocalTranspoderFile;
      SatelliteName = satellite.Name;
    }

    public String DisplayName
    {
      get { return System.IO.Path.GetFileNameWithoutExtension(FileName); }
    }

    public override string ToString()
    {
      return SatelliteName;
    }

    public int CompareTo(SatelliteContext other)
    {
      return SatelliteName.CompareTo(other.SatelliteName);
    }

    #region IComparable<SatelliteContext> Members

    int IComparable<SatelliteContext>.CompareTo(SatelliteContext other)
    {
      return SatelliteName.CompareTo(other.SatelliteName);
    }

    #endregion
  }
}