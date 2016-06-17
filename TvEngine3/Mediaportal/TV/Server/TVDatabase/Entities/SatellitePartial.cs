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

using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Satellite
  {
    public static int? DefaultSatelliteLongitude
    {
      get
      {
        string countryName = System.Globalization.RegionInfo.CurrentRegion.EnglishName;
        if (string.Equals(countryName, "Australia"))
        {
          return 1560;    // Optus C1/D3: Foxtel, VAST
        }
        else if (string.Equals(countryName, "Germany"))
        {
          return 192;
        }
        else if (string.Equals(countryName, "New Zealand"))
        {
          return 1600;    // Optus D1: Freeview, Sky
        }
        else if (string.Equals(countryName, "United Kingdom"))
        {
          return 282;     // 28.2E: Freesat
        }
        return null;
      }
    }

    public static string LongitudeString(int longitude)
    {
      return string.Format("{0:#.#}° {1}", Math.Abs(longitude / 10), longitude < 0 ? "W" : "E");
    }

    public string LongitudeString()
    {
      return LongitudeString(Longitude);
    }

    public override string ToString()
    {
      return string.Format("{0} {2}", LongitudeString(), Name);
    }
  }
}