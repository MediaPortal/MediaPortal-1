#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.ComponentModel;
using System.Globalization;

namespace MediaPortal.Drawing.Transforms
{
  public sealed class TransformConverter : TypeConverter
  {
    #region Methods

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
    {
      if (t == typeof (string))
      {
        return true;
      }

      if (t == typeof (TransformCollection))
      {
        return true;
      }

      return base.CanConvertFrom(context, t);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value is string)
      {
        return Parse(context, culture, (string) value);
      }

      if (value is TransformCollection)
      {
        return new TransformGroup((TransformCollection) value);
      }

      if (value is Transform)
      {
        return value;
      }

      return base.ConvertFrom(context, culture, value);
    }

    private object Parse(ITypeDescriptorContext context, CultureInfo culture, string value)
    {
      StringTokenizer tokens = new StringTokenizer(value);

      if (tokens.Count == 0)
      {
        return base.ConvertFrom(context, culture, value);
      }

      if (string.Compare(tokens[0], "Rotate", true) == 0)
      {
        if (tokens.Count == 5)
        {
          return new RotateTransform(double.Parse(tokens[1]),
                                     (Point)
                                     new PointConverter().ConvertFromString(context, culture,
                                                                            tokens[2] + tokens[3] + tokens[4]));
        }

        if (tokens.Count == 4)
        {
          return new RotateTransform(double.Parse(tokens[1]),
                                     (Point)
                                     new PointConverter().ConvertFromString(context, culture, tokens[2] + tokens[3]));
        }

        if (tokens.Count == 3)
        {
          return new RotateTransform(double.Parse(tokens[1]),
                                     (Point) new PointConverter().ConvertFromString(context, culture, tokens[2]));
        }

        if (tokens.Count == 2)
        {
          return new RotateTransform(double.Parse(tokens[1]));
        }

        return base.ConvertFrom(context, culture, value);
      }

      if (string.Compare(tokens[0], "Scale", true) == 0)
      {
        if (tokens.Count == 6)
        {
          return new ScaleTransform(double.Parse(tokens[1]), double.Parse(tokens[2]),
                                    (Point)
                                    new PointConverter().ConvertFromString(context, culture,
                                                                           tokens[3] + tokens[4] + tokens[5]));
        }

        if (tokens.Count == 5)
        {
          return new ScaleTransform(double.Parse(tokens[1]), double.Parse(tokens[2]),
                                    (Point)
                                    new PointConverter().ConvertFromString(context, culture, tokens[3] + tokens[4]));
        }

        if (tokens.Count == 4)
        {
          return new ScaleTransform(double.Parse(tokens[1]), double.Parse(tokens[2]),
                                    (Point) new PointConverter().ConvertFromString(context, culture, tokens[3]));
        }

        if (tokens.Count == 3)
        {
          return new ScaleTransform(double.Parse(tokens[1]), double.Parse(tokens[2]));
        }

        if (tokens.Count == 2)
        {
          return new ScaleTransform(double.Parse(tokens[1]), double.Parse(tokens[1]));
        }

        return base.ConvertFrom(context, culture, value);
      }

      if (string.Compare(tokens[0], "Translate", true) == 0)
      {
        if (tokens.Count == 3)
        {
          return new TranslateTransform(double.Parse(tokens[1]), double.Parse(tokens[2]));
        }

        return base.ConvertFrom(context, culture, value);
      }

      return base.ConvertFrom(context, culture, value);
    }

    #endregion Methods
  }
}