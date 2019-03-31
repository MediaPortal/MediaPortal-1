#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;

namespace Mediaportal.TV.Server.Common.Types.Provider
{
  public sealed class RegionFreesat
  {
    private readonly int _id;
    private readonly string _country;
    private readonly string _region;
    private readonly string _subRegion;

    private static readonly IDictionary<int, RegionFreesat> _values = new Dictionary<int, RegionFreesat>(100);

    #region values

    // England bouquets
    public static readonly RegionFreesat London_London = new RegionFreesat(1, "England", "London", "London");
    public static readonly RegionFreesat EastWest_AngliaWest1 = new RegionFreesat(2, "England", "East (W)", "Anglia West 1");
    public static readonly RegionFreesat EastWest_CentralSouth = new RegionFreesat(3, "England", "East (W)", "Central South");
    public static readonly RegionFreesat EastWest_AngliaWest2 = new RegionFreesat(4, "England", "East (W)", "Anglia West 2");
    public static readonly RegionFreesat WestMidlands_CentralWest = new RegionFreesat(5, "England", "West Midlands", "Central West");
    public static readonly RegionFreesat WestMidlands_CentralSouthWest = new RegionFreesat(6, "England", "West Midlands", "Central South West");
    public static readonly RegionFreesat NorthWest_Granada1 = new RegionFreesat(7, "England", "North West", "Granada 1");
    public static readonly RegionFreesat NorthWest_BorderEngland = new RegionFreesat(8, "England", "North West", "Border England");
    public static readonly RegionFreesat EastYorkshireAndLincolnshire_YorkshireEast = new RegionFreesat(9, "England", "East Yorkshire & Lincolnshire", "Yorkshire East");
    public static readonly RegionFreesat Yorkshire_YorkshireWest1 = new RegionFreesat(10, "England", "Yorkshire", "Yorkshire West 1");
    public static readonly RegionFreesat Yorkshire_YorkshireWest2 = new RegionFreesat(11, "England", "Yorkshire", "Yorkshire West 2");
    public static readonly RegionFreesat Yorkshire_TyneTeesSouth = new RegionFreesat(12, "England", "Yorkshire", "Tyne Tees South");
    public static readonly RegionFreesat EastMidlands_CentralEast1 = new RegionFreesat(13, "England", "East Midlands", "Central East 1");
    public static readonly RegionFreesat EastMidlands_CentralWest = new RegionFreesat(14, "England", "East Midlands", "Central West");
    public static readonly RegionFreesat EastMidlands_CentralEast2 = new RegionFreesat(15, "England", "East Midlands", "Central East 2");
    public static readonly RegionFreesat EastMidlands_YorkshireEast = new RegionFreesat(16, "England", "East Midlands", "Yorkshire East");
    public static readonly RegionFreesat EastEast_AngliaSouth = new RegionFreesat(17, "England", "East (E)", "Anglia South");
    public static readonly RegionFreesat EastEast_London = new RegionFreesat(18, "England", "East (E)", "London");
    public static readonly RegionFreesat EastEast_AngliaEast = new RegionFreesat(19, "England", "East (E)", "Anglia East");
    public static readonly RegionFreesat West_West = new RegionFreesat(20, "England", "West", "West");
    public static readonly RegionFreesat West_CentralSouth = new RegionFreesat(21, "England", "West", "Central South");
    public static readonly RegionFreesat West_MeridianNorth1 = new RegionFreesat(22, "England", "West", "Meridian North 1");
    public static readonly RegionFreesat West_MeridianNorth2 = new RegionFreesat(23, "England", "West", "Meridian North 2");
    public static readonly RegionFreesat SouthEast_MeridianSouth = new RegionFreesat(24, "England", "South East", "Meridian South");
    public static readonly RegionFreesat SouthEast_MeridianSouthEast = new RegionFreesat(25, "England", "South East", "Meridian South East");
    public static readonly RegionFreesat SouthEast_MeridianEast = new RegionFreesat(26, "England", "South East", "Meridian East");
    public static readonly RegionFreesat SouthEast_London = new RegionFreesat(27, "England", "South East", "London");
    public static readonly RegionFreesat South_MeridianSouth = new RegionFreesat(28, "England", "South", "Meridian South");
    public static readonly RegionFreesat South_MeridianSouthEast = new RegionFreesat(29, "England", "South", "Meridian South East");
    public static readonly RegionFreesat South_WestCountry = new RegionFreesat(30, "England", "South", "West Country");
    public static readonly RegionFreesat South_London = new RegionFreesat(31, "England", "South", "London");
    public static readonly RegionFreesat South_MeridianNorth = new RegionFreesat(32, "England", "South", "Meridian North");
    public static readonly RegionFreesat SouthWest_WestCountry = new RegionFreesat(33, "England", "South West", "West Country");
    public static readonly RegionFreesat NorthEastAndCentral_BorderEngland = new RegionFreesat(34, "England", "North East & Central", "Border England");
    public static readonly RegionFreesat NorthEastAndCentral_TyneTeesNorth = new RegionFreesat(35, "England", "North East & Central", "Tyne Tees North");
    public static readonly RegionFreesat NorthEastAndCentral_TyneTeesSouth = new RegionFreesat(36, "England", "North East & Central", "Tyne Tees South");
    public static readonly RegionFreesat Oxford_CentralSouth = new RegionFreesat(37, "England", "Oxford", "Central South");
    public static readonly RegionFreesat Oxford_London = new RegionFreesat(38, "England", "Oxford", "London");
    public static readonly RegionFreesat NorthWest_Granada2 = new RegionFreesat(39, "England", "North West", "Granada 2");
    public static readonly RegionFreesat Yorkshire_YorkshireWest3 = new RegionFreesat(40, "England", "Yorkshire", "Yorkshire West 3");
    public static readonly RegionFreesat ChannelIslands = new RegionFreesat(41, "England", "Channel Islands", "Channel Islands");
    public static readonly RegionFreesat Yorkshire_YorkshireEast = new RegionFreesat(42, "England", "Yorkshire", "Yorkshire East");
    public static readonly RegionFreesat IsleOfMan = new RegionFreesat(43, "England", "Isle Of Man", "Isle Of Man");

    // Scotland bouquets
    public static readonly RegionFreesat Scotland_Grampian1 = new RegionFreesat(60, "Scotland", "Grampian 1", "Grampian 1");
    public static readonly RegionFreesat Scotland_Grampian2 = new RegionFreesat(61, "Scotland", "Grampian 2", "Grampian 2");
    public static readonly RegionFreesat Scotland_BorderScotland = new RegionFreesat(62, "Scotland", "Border Scotland", "Border Scotland");
    public static readonly RegionFreesat Scotland_Glasgow = new RegionFreesat(63, "Scotland", "Glasgow", "Glasgow");
    public static readonly RegionFreesat Scotland_Edinburgh = new RegionFreesat(64, "Scotland", "Edinburgh", "Edinburgh");

    // Wales bouquets
    public static readonly RegionFreesat Wales = new RegionFreesat(80, "Wales", "Wales", "Wales");

    // Northern Ireland bouquets
    public static readonly RegionFreesat NorthernIreland = new RegionFreesat(90, "Northern Ireland", "Northern Ireland", "Northern Ireland");

    // other bouquets
    // These are commented out because we don't want them to show or be
    // selectable in TV Server Configuration.
    //public static readonly RegionFreesat Default = new RegionFreesat(50, "Default", "Default", "Default");
    //public static readonly RegionFreesat None = new RegionFreesat(100, "-", "-", "-");

    #endregion

    private RegionFreesat(int id, string country, string region, string subRegion)
    {
      _id = id;
      _country = country;
      _region = region;
      _subRegion = subRegion;
      _values.Add(id, this);
    }

    #region properties

    public int Id
    {
      get
      {
        return _id;
      }
    }

    public string Country
    {
      get
      {
        return _country;
      }
    }

    public string Region
    {
      get
      {
        return _region;
      }
    }

    public string SubRegion
    {
      get
      {
        return _subRegion;
      }
    }

    #endregion

    #region object overrides

    public override string ToString()
    {
      if (string.Equals(_country, _region))
      {
        return _country;
      }
      if (string.Equals(_region, _subRegion))
      {
        if (string.Equals(_country, "England"))
        {
          return _region;
        }
        return string.Format("{0} - {1}", _country, _region);
      }
      return string.Format("{0} - {1}, {2}", _country, _region, _subRegion);
    }

    public override bool Equals(object obj)
    {
      RegionFreesat region = obj as RegionFreesat;
      if (region != null && this == region)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _id;
    }

    #endregion

    #region static members

    public static ICollection<RegionFreesat> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator RegionFreesat(int id)
    {
      RegionFreesat value = null;
      if (!_values.TryGetValue(id, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator int(RegionFreesat region)
    {
      return region._id;
    }

    #endregion
  }
}