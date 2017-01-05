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
    private readonly bool _isSatellite;

    private static readonly IDictionary<int, RegionOpenTvFoxtel> _values = new Dictionary<int, RegionOpenTvFoxtel>(50);

    #region values

    public static readonly RegionOpenTvFoxtel Satellite_Adelaide = new RegionOpenTvFoxtel(1, BouquetOpenTvFoxtel.SdBouquet, "Adelaide", true);
    public static readonly RegionOpenTvFoxtel Satellite_Brisbane = new RegionOpenTvFoxtel(2, BouquetOpenTvFoxtel.SdBouquet, "Brisbane", true);
    public static readonly RegionOpenTvFoxtel Satellite_Canberra = new RegionOpenTvFoxtel(3, BouquetOpenTvFoxtel.SdBouquet, "Canberra", true);
    public static readonly RegionOpenTvFoxtel Satellite_Melbourne = new RegionOpenTvFoxtel(6, BouquetOpenTvFoxtel.SdBouquet, "Melbourne", true);
    public static readonly RegionOpenTvFoxtel Satellite_CentralCoast = new RegionOpenTvFoxtel(10, BouquetOpenTvFoxtel.SdBouquet, "Central Coast", true);
    public static readonly RegionOpenTvFoxtel Satellite_GoldCoast = new RegionOpenTvFoxtel(11, BouquetOpenTvFoxtel.SdBouquet, "Gold Coast", true);
    public static readonly RegionOpenTvFoxtel Satellite_Perth = new RegionOpenTvFoxtel(13, BouquetOpenTvFoxtel.SdBouquet, "Perth", true);
    public static readonly RegionOpenTvFoxtel Satellite_Sydney = new RegionOpenTvFoxtel(17, BouquetOpenTvFoxtel.SdBouquet, "Sydney", true);
    public static readonly RegionOpenTvFoxtel Satellite_Darwin = new RegionOpenTvFoxtel(24, BouquetOpenTvFoxtel.SdBouquet, "Darwin", true);

    public static readonly RegionOpenTvFoxtel Cable_Adelaide = new RegionOpenTvFoxtel(21, BouquetOpenTvFoxtel.SdBouquet, "Adelaide", false);
    public static readonly RegionOpenTvFoxtel Cable_Brisbane = new RegionOpenTvFoxtel(22, BouquetOpenTvFoxtel.SdBouquet, "Brisbane", false);
    public static readonly RegionOpenTvFoxtel Cable_Melbourne = new RegionOpenTvFoxtel(26, BouquetOpenTvFoxtel.SdBouquet, "Melbourne", false);
    public static readonly RegionOpenTvFoxtel Cable_GoldCoast = new RegionOpenTvFoxtel(31, BouquetOpenTvFoxtel.SdBouquet, "Gold Coast", false);
    public static readonly RegionOpenTvFoxtel Cable_Perth = new RegionOpenTvFoxtel(33, BouquetOpenTvFoxtel.SdBouquet, "Perth", false);
    public static readonly RegionOpenTvFoxtel Cable_Sydney = new RegionOpenTvFoxtel(37, BouquetOpenTvFoxtel.SdBouquet, "Sydney", false);

    // OPTUS regions (Optus TV featuring Foxtel)
    // These values are only kept to enable resolution of the OPTUS region
    // names. In time I expect the OPTUS bouquet will be phased out. When that
    // happens these values may be safely removed and the code amended to use
    // the region ID as a unique identifier.
    public static readonly RegionOpenTvFoxtel Optus_Adelaide = new RegionOpenTvFoxtel(33, BouquetOpenTvFoxtel.Optus, "Adelaide", true);
    public static readonly RegionOpenTvFoxtel Optus_Brisbane = new RegionOpenTvFoxtel(34, BouquetOpenTvFoxtel.Optus, "Brisbane", true);
    public static readonly RegionOpenTvFoxtel Optus_Canberra = new RegionOpenTvFoxtel(35, BouquetOpenTvFoxtel.Optus, "Canberra", true);
    public static readonly RegionOpenTvFoxtel Optus_Melbourne = new RegionOpenTvFoxtel(38, BouquetOpenTvFoxtel.Optus, "Melbourne", true);
    public static readonly RegionOpenTvFoxtel Optus_CentralCoast = new RegionOpenTvFoxtel(42, BouquetOpenTvFoxtel.Optus, "Central Coast", true);
    public static readonly RegionOpenTvFoxtel Optus_GoldCoast = new RegionOpenTvFoxtel(43, BouquetOpenTvFoxtel.Optus, "Gold Coast", true);
    public static readonly RegionOpenTvFoxtel Optus_Perth = new RegionOpenTvFoxtel(45, BouquetOpenTvFoxtel.Optus, "Perth", true);
    public static readonly RegionOpenTvFoxtel Optus_Sydney = new RegionOpenTvFoxtel(49, BouquetOpenTvFoxtel.Optus, "Sydney", true);

    #endregion

    private RegionOpenTvFoxtel(int id, BouquetOpenTvFoxtel bouquet, string region, bool isSatellite)
    {
      _id = id;
      _bouquet = bouquet;
      _region = region;
      _isSatellite = isSatellite;
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
      else if (!_isSatellite)
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
      return _id.GetHashCode() ^ _bouquet.GetHashCode() ^ _region.GetHashCode() ^ _isSatellite.GetHashCode();
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
      if (bouquet != BouquetOpenTvFoxtel.Optus)
      {
        bouquet = BouquetOpenTvFoxtel.SdBouquet;
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