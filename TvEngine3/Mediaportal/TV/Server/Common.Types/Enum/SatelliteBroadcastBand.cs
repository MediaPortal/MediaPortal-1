#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  /// <summary>
  /// Satellite broadcast bands (frequency ranges).
  /// </summary>
  public sealed class SatelliteBroadcastBand
  {
    private readonly int _id;
    private readonly string _name;
    private readonly int _rangeLimitLow;    // unit = MHz
    private readonly int _rangeLimitHigh;

    private static readonly IDictionary<int, SatelliteBroadcastBand> _values = new Dictionary<int, SatelliteBroadcastBand>(3);

    #region values

    public static readonly SatelliteBroadcastBand Ku = new SatelliteBroadcastBand(0, "Ku Band", 10700, 12750);
    public static readonly SatelliteBroadcastBand C = new SatelliteBroadcastBand(1, "C Band", 3400, 4800);
    public static readonly SatelliteBroadcastBand Ka = new SatelliteBroadcastBand(2, "Ka Band", 18300, 20250);

    #endregion

    private SatelliteBroadcastBand(int id, string name, int rangeLimitLow, int rangeLimitHigh)
    {
      _id = id;
      _name = name;
      _rangeLimitLow = rangeLimitLow;
      _rangeLimitHigh = rangeLimitHigh;
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

    public string Name
    {
      get
      {
        return _name;
      }
    }

    public int RangeLimitLow
    {
      get
      {
        return _rangeLimitLow;
      }
    }

    public int RangeLimitHigh
    {
      get
      {
        return _rangeLimitHigh;
      }
    }

    #endregion

    #region object overrides

    public override string ToString()
    {
      return string.Format("{0} ({1} - {2} MHz)", _name, _rangeLimitLow, _rangeLimitHigh);
    }

    public override bool Equals(object obj)
    {
      SatelliteBroadcastBand band = obj as SatelliteBroadcastBand;
      return band != null && this == band;
    }

    public override int GetHashCode()
    {
      return _id;
    }

    #endregion

    #region static members

    public static ICollection<SatelliteBroadcastBand> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator SatelliteBroadcastBand(int id)
    {
      SatelliteBroadcastBand value = null;
      if (!_values.TryGetValue(id, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator int(SatelliteBroadcastBand band)
    {
      return band._id;
    }

    #endregion
  }
}