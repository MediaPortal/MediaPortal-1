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
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.Common.Types.Country
{
  /// <summary>
  /// A class which holds relevant identfying details for a country, including
  /// its ITU E.123/164 code, full name and ISO code.
  /// </summary>
  [Serializable]
  public class Country
  {
    private int _id;
    private readonly string _name;
    private readonly int _ituCode;
    private readonly string _isoCode;
    private AnalogVideoStandard _analogVideoStandard;

    /// <summary>
    /// Initialise a new instance of the <see cref="Country"/> class.
    /// </summary>
    /// <param name="ituCode">The country's ITU E123/164 code.</param>
    /// <param name="name">The country's name.</param>
    /// <param name="isoCode">The country's ISO code.</param>
    /// <param name="analogVideoStandard">The video standard used for analog television broadcast in the country.</param>
    public Country(int ituCode, string name, string isoCode, AnalogVideoStandard analogVideoStandard)
    {
      _ituCode = ituCode;
      _name = name;
      _isoCode = isoCode;
      _analogVideoStandard = analogVideoStandard;
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return _name;
    }

    /// <summary>
    /// Get or set the identifier that MediaPortal associates with the country.
    /// </summary>
    public int Id
    {
      get
      {
        return _id;
      }
      internal set
      {
        _id = value;
      }
    }

    /// <summary>
    /// Get the country's ITU E.123/164 code.
    /// </summary>
    public int ItuCode
    {
      get
      {
        return _ituCode;
      }
    }

    /// <summary>
    /// Get the country's name.
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
    }

    /// <summary>
    /// Get the country's ISO code.
    /// </summary>
    public string IsoCode
    {
      get
      {
        return _isoCode;
      }
    }

    /// <summary>
    /// Get the video standard used for analog television broadcasting in the country.
    /// </summary>
    public AnalogVideoStandard VideoStandard
    {
      get
      {
        return _analogVideoStandard;
      }
    }
  }
}