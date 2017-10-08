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
  public sealed class RegionOpenTvSkyUk
  {
    private readonly int _id;
    private readonly string _country;
    private readonly string _region;

    private static readonly IDictionary<int, RegionOpenTvSkyUk> _values = new Dictionary<int, RegionOpenTvSkyUk>(100);

    #region values

    // England bouquets
    public static readonly RegionOpenTvSkyUk London = new RegionOpenTvSkyUk(1, "England", "London");
    public static readonly RegionOpenTvSkyUk Essex = new RegionOpenTvSkyUk(2, "England", "Essex");
    public static readonly RegionOpenTvSkyUk CentralMidlands = new RegionOpenTvSkyUk(3, "England", "Central Midlands");
    public static readonly RegionOpenTvSkyUk HtvWest = new RegionOpenTvSkyUk(4, "England", "HTV West");
    public static readonly RegionOpenTvSkyUk MeridianSouth = new RegionOpenTvSkyUk(5, "England", "Meridian South");
    public static readonly RegionOpenTvSkyUk Westcountry = new RegionOpenTvSkyUk(6, "England", "Westcountry");
    public static readonly RegionOpenTvSkyUk Granada = new RegionOpenTvSkyUk(7, "England", "Granada");
    public static readonly RegionOpenTvSkyUk NorthWestYorkshire = new RegionOpenTvSkyUk(8, "England", "North West Yorkshire");
    public static readonly RegionOpenTvSkyUk ThamesValley = new RegionOpenTvSkyUk(9, "England", "Thames Valley");
    public static readonly RegionOpenTvSkyUk MeridianSouthEast = new RegionOpenTvSkyUk(10, "England", "Meridian South East");
    public static readonly RegionOpenTvSkyUk MeridianEast = new RegionOpenTvSkyUk(11, "England", "Meridian East");
    public static readonly RegionOpenTvSkyUk BorderEngland = new RegionOpenTvSkyUk(12, "England", "Border England");
    public static readonly RegionOpenTvSkyUk Tyne = new RegionOpenTvSkyUk(13, "England", "Tyne");

    public static readonly RegionOpenTvSkyUk LondonEssex = new RegionOpenTvSkyUk(18, "England", "London/Essex");
    public static readonly RegionOpenTvSkyUk Atherstone = new RegionOpenTvSkyUk(19, "England", "Atherstone");
    public static readonly RegionOpenTvSkyUk EastMidlands = new RegionOpenTvSkyUk(20, "England", "East Midlands");
    public static readonly RegionOpenTvSkyUk Norfolk = new RegionOpenTvSkyUk(21, "England", "Norfolk");

    public static readonly RegionOpenTvSkyUk Gloucester = new RegionOpenTvSkyUk(24, "England", "Gloucester");
    public static readonly RegionOpenTvSkyUk WestAnglia = new RegionOpenTvSkyUk(25, "England", "West Anglia");
    public static readonly RegionOpenTvSkyUk NorthYorkshire = new RegionOpenTvSkyUk(26, "England", "North Yorkshire");
    public static readonly RegionOpenTvSkyUk Tring = new RegionOpenTvSkyUk(27, "England", "Tring");
    public static readonly RegionOpenTvSkyUk SouthLakeland = new RegionOpenTvSkyUk(28, "England", "South Lakeland");
    public static readonly RegionOpenTvSkyUk Humber = new RegionOpenTvSkyUk(29, "England", "Humber");

    public static readonly RegionOpenTvSkyUk Wales = new RegionOpenTvSkyUk(32, "Wales", "Wales");

    // other bouquets - note values are out of order
    public static readonly RegionOpenTvSkyUk NorthernIreland = new RegionOpenTvSkyUk(33, "Northern Ireland", "Northern Ireland");
    public static readonly RegionOpenTvSkyUk ChannelIsles = new RegionOpenTvSkyUk(34, "England", "Channel Isles");
    public static readonly RegionOpenTvSkyUk RepublicOfIreland = new RegionOpenTvSkyUk(50, "Republic Of Ireland", "Republic Of Ireland");

    // Scotland bouquets
    public static readonly RegionOpenTvSkyUk Grampian = new RegionOpenTvSkyUk(35, "Scotland", "Grampian");
    public static readonly RegionOpenTvSkyUk BorderScotland = new RegionOpenTvSkyUk(36, "Scotland", "Border Scotland");
    public static readonly RegionOpenTvSkyUk ScottishWest = new RegionOpenTvSkyUk(37, "Scotland", "Scottish West");
    public static readonly RegionOpenTvSkyUk ScottishEast = new RegionOpenTvSkyUk(38, "Scotland", "Scottish East");
    public static readonly RegionOpenTvSkyUk Dundee = new RegionOpenTvSkyUk(39, "Scotland", "Dundee");

    // Wales bouquets
    public static readonly RegionOpenTvSkyUk RidgeHill = new RegionOpenTvSkyUk(41, "England", "Ridge Hill");
    public static readonly RegionOpenTvSkyUk HtvWales = new RegionOpenTvSkyUk(43, "Wales", "HTV Wales");
    public static readonly RegionOpenTvSkyUk Merseyside = new RegionOpenTvSkyUk(45, "England", "Merseyside");

    public static readonly RegionOpenTvSkyUk Sheffield = new RegionOpenTvSkyUk(60, "England", "Sheffield");
    public static readonly RegionOpenTvSkyUk Scarborough = new RegionOpenTvSkyUk(61, "England", "Scarborough");
    public static readonly RegionOpenTvSkyUk NorthEastMidlands = new RegionOpenTvSkyUk(62, "England", "North East Midlands");
    public static readonly RegionOpenTvSkyUk HtvWestThamesValley = new RegionOpenTvSkyUk(63, "England", "HTV West/Thames Valley");
    public static readonly RegionOpenTvSkyUk Kent = new RegionOpenTvSkyUk(64, "England", "Kent");
    public static readonly RegionOpenTvSkyUk Brighton = new RegionOpenTvSkyUk(65, "England", "Brighton");
    public static readonly RegionOpenTvSkyUk LondonThamesValley = new RegionOpenTvSkyUk(66, "England", "London/Thames Valley");
    public static readonly RegionOpenTvSkyUk WestDorset = new RegionOpenTvSkyUk(67, "England", "West Dorset");
    public static readonly RegionOpenTvSkyUk MeridianNorth = new RegionOpenTvSkyUk(68, "England", "Meridian North");
    public static readonly RegionOpenTvSkyUk Tees = new RegionOpenTvSkyUk(69, "England", "Tees");
    public static readonly RegionOpenTvSkyUk HenleyOnThames = new RegionOpenTvSkyUk(70, "England", "Henley On Thames");
    public static readonly RegionOpenTvSkyUk Oxford = new RegionOpenTvSkyUk(71, "England", "Oxford");
    public static readonly RegionOpenTvSkyUk SouthYorkshire = new RegionOpenTvSkyUk(72, "England", "South Yorkshire");

    #endregion

    private RegionOpenTvSkyUk(int id, string country, string region)
    {
      _id = id;
      _country = country;
      _region = region;
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

    #endregion

    #region object overrides

    public override string ToString()
    {
      if (string.Equals(_country, _region))
      {
        return _country;
      }
      if (string.Equals(_region, "Channel Isles"))
      {
        return _region;
      }
      return string.Format("{0} - {1}", _country, _region);
    }

    public override bool Equals(object obj)
    {
      RegionOpenTvSkyUk region = obj as RegionOpenTvSkyUk;
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

    public static ICollection<RegionOpenTvSkyUk> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator RegionOpenTvSkyUk(int id)
    {
      RegionOpenTvSkyUk value = null;
      if (!_values.TryGetValue(id, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator int(RegionOpenTvSkyUk region)
    {
      return region._id;
    }

    #endregion
  }
}