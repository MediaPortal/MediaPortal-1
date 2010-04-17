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

using System.ComponentModel;

namespace System.Windows.Controls
{
  [TypeConverter(typeof (GridLengthConverter))]
  public struct GridLength
  {
    #region Constructors

    public GridLength(double pixels)
    {
      _value = pixels;
      _unit = GridUnitType.Pixel;
    }

    public GridLength(GridUnitType unit)
    {
      _value = 0;
      _unit = unit;
    }

    #endregion Constructors

    #region Properties

    public GridUnitType GridUnitType
    {
      get { return _unit; }
      set { _unit = value; }
    }

    public bool IsAbsolute
    {
      get { return _unit == GridUnitType.Pixel; }
    }

    public bool IsAuto
    {
      get { return _unit == GridUnitType.Auto; }
    }

    public bool IsStar
    {
      get { return _unit == GridUnitType.Star; }
    }

    public double Value
    {
      get { return _value; }
    }

    #endregion Properties

    #region Fields

    private double _value;
    private GridUnitType _unit;

    #endregion Fields
  }
}