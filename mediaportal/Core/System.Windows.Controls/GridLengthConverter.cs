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

using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Controls
{
  public class GridLengthConverter : TypeConverter
  {
    #region Methods

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
    {
      if (t == typeof (string))
      {
        return true;
      }

      return base.CanConvertFrom(context, t);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value is string)
      {
        if (string.Compare((string)value, "*") == 0)
        {
          return new GridLength(GridUnitType.Star);
        }

        if (string.Compare((string)value, "Auto", true) == 0)
        {
          return new GridLength(GridUnitType.Auto);
        }

        return new GridLength(double.Parse((string)value));
      }

      return base.ConvertFrom(context, culture, value);
    }

    #endregion Methods
  }
}