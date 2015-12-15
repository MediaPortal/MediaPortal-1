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
  public sealed class RegionOpenTvFoxtel
  {
    private readonly int _id;
    private readonly BouquetOpenTvFoxtel _bouquet;
    private readonly string _region;

    private static readonly IDictionary<int, RegionOpenTvFoxtel> _values = new Dictionary<int, RegionOpenTvFoxtel>(50);

    #region values

    public static readonly RegionOpenTvFoxtel Satellite_Adelaide = new RegionOpenTvFoxtel(1, BouquetOpenTvFoxtel.Residential, "Adelaide");
    public static readonly RegionOpenTvFoxtel Satellite_Brisbane = new RegionOpenTvFoxtel(2, BouquetOpenTvFoxtel.Residential, "Brisbane");
    public static readonly RegionOpenTvFoxtel Satellite_Canberra = new RegionOpenTvFoxtel(3, BouquetOpenTvFoxtel.Residential, "Canberra");
    public static readonly RegionOpenTvFoxtel Satellite_Melbourne = new RegionOpenTvFoxtel(6, BouquetOpenTvFoxtel.Residential, "Melbourne");
    public static readonly RegionOpenTvFoxtel Satellite_CentralCoast = new RegionOpenTvFoxtel(10, BouquetOpenTvFoxtel.Residential, "Central Coast");
    public static readonly RegionOpenTvFoxtel Satellite_GoldCoast = new RegionOpenTvFoxtel(11, BouquetOpenTvFoxtel.Residential, "Gold Coast");
    public static readonly RegionOpenTvFoxtel Satellite_Perth = new RegionOpenTvFoxtel(13, BouquetOpenTvFoxtel.Residential, "Perth");
    public static readonly RegionOpenTvFoxtel Satellite_Sydney = new RegionOpenTvFoxtel(17, BouquetOpenTvFoxtel.Residential, "Sydney");

    public static readonly RegionOpenTvFoxtel Cable_Adelaide = new RegionOpenTvFoxtel(21, BouquetOpenTvFoxtel.Residential, "Adelaide");
    public static readonly RegionOpenTvFoxtel Cable_Brisbane = new RegionOpenTvFoxtel(22, BouquetOpenTvFoxtel.Residential, "Brisbane");
    public static readonly RegionOpenTvFoxtel Cable_Melbourne = new RegionOpenTvFoxtel(26, BouquetOpenTvFoxtel.Residential, "Melbourne");
    public static readonly RegionOpenTvFoxtel Cable_GoldCoast = new RegionOpenTvFoxtel(31, BouquetOpenTvFoxtel.Residential, "Gold Coast");
    public static readonly RegionOpenTvFoxtel Cable_Perth = new RegionOpenTvFoxtel(33, BouquetOpenTvFoxtel.Residential, "Perth");
    public static readonly RegionOpenTvFoxtel Cable_Sydney = new RegionOpenTvFoxtel(37, BouquetOpenTvFoxtel.Residential, "Sydney");

    // OPTUS bouquets (Optus TV featuring Foxtel)
    public static readonly RegionOpenTvFoxtel Optus_Adelaide = new RegionOpenTvFoxtel(33, BouquetOpenTvFoxtel.Optus, "Adelaide");
    public static readonly RegionOpenTvFoxtel Optus_Brisbane = new RegionOpenTvFoxtel(34, BouquetOpenTvFoxtel.Optus, "Brisbane");
    public static readonly RegionOpenTvFoxtel Optus_Canberra = new RegionOpenTvFoxtel(35, BouquetOpenTvFoxtel.Optus, "Canberra");
    public static readonly RegionOpenTvFoxtel Optus_Melbourne = new RegionOpenTvFoxtel(38, BouquetOpenTvFoxtel.Optus, "Melbourne");
    public static readonly RegionOpenTvFoxtel Optus_CentralCoast = new RegionOpenTvFoxtel(42, BouquetOpenTvFoxtel.Optus, "Central Coast");
    public static readonly RegionOpenTvFoxtel Optus_GoldCoast = new RegionOpenTvFoxtel(43, BouquetOpenTvFoxtel.Optus, "Gold Coast");
    public static readonly RegionOpenTvFoxtel Optus_Perth = new RegionOpenTvFoxtel(45, BouquetOpenTvFoxtel.Optus, "Perth");
    public static readonly RegionOpenTvFoxtel Optus_Sydney = new RegionOpenTvFoxtel(49, BouquetOpenTvFoxtel.Optus, "Sydney");

    #endregion

    private RegionOpenTvFoxtel(int id, BouquetOpenTvFoxtel bouquet, string region)
    {
      _id = id;
      _bouquet = bouquet;
      _region = region;
      _values.Add(((int)bouquet << 16) | id, this);
    }

    #region properties

    public int Id
    {
      get
      {
        return _id;
      }
    }

    public BouquetOpenTvFoxtel Bouquet
    {
      get
      {
        return _bouquet;
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
      string network = "Satellite";
      if (_bouquet == BouquetOpenTvFoxtel.Optus)
      {
        network = "Optus";
      }
      else if (_id >= RegionOpenTvFoxtel.Cable_Adelaide.Id)
      {
        network = "Cable";
      }
      return string.Format("{0} - {1}", network, _region);
    }

    public override bool Equals(object obj)
    {
      RegionOpenTvFoxtel region = obj as RegionOpenTvFoxtel;
      if (region != null && this == region)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _id.GetHashCode() ^ _bouquet.GetHashCode() ^ _region.GetHashCode();
    }

    #endregion

    #region static members

    public static ICollection<RegionOpenTvFoxtel> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static RegionOpenTvFoxtel GetValue(int id, BouquetOpenTvFoxtel bouquet)
    {
      if (bouquet == BouquetOpenTvFoxtel.Optus || bouquet == BouquetOpenTvFoxtel.HdOptus)
      {
        bouquet = BouquetOpenTvFoxtel.Optus;
      }
      else
      {
        bouquet = BouquetOpenTvFoxtel.Residential;
      }
      RegionOpenTvFoxtel region;
      if (!_values.TryGetValue(((int)bouquet << 16) | id, out region))
      {
        return null;
      }
      return region;
    }

    #endregion
  }
}