#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

namespace MediaPortal.Drawing
{
  [TypeConverter(typeof (ThicknessConverter))]
  public struct Thickness
  {
    #region Constructors

    public Thickness(double thickness) : this(thickness, thickness, thickness, thickness)
    {
    }

    public Thickness(double w, double h) : this(w, h, w, h)
    {
    }

    public Thickness(double l, double t, double r, double b)
    {
      _l = l;
      _t = t;
      _r = r;
      _b = b;
    }

    #endregion Constructors

    #region Operators

    public static bool operator ==(Thickness l, Thickness r)
    {
      return l._l == r._l && l._t == r._t && l._r == r._r && l._b == r._b;
    }

    public static bool operator !=(Thickness l, Thickness r)
    {
      return !(l._l == r._l && l._r == r._r && l._r == r._r && l._b == r._b);
    }

    #endregion Operators

    #region Methods

    public override bool Equals(object o)
    {
      return o is Thickness && ((Thickness) o)._l == _l && ((Thickness) o)._t == _t && ((Thickness) o)._r == _r &&
             ((Thickness) o)._b == _b;
    }

    public override int GetHashCode()
    {
      return (int) ((uint) _l ^ (uint) _t ^ (uint) _r ^ (uint) _b);
    }

    public static Thickness Parse(string source)
    {
      return Parse(source, null);
    }

    public static Thickness Parse(string source, IFormatProvider formatProvider)
    {
      string[] tokens = source.Split(',');

      if (tokens.Length == 4)
      {
        return new Thickness(double.Parse(tokens[0], formatProvider), double.Parse(tokens[1], formatProvider),
                             double.Parse(tokens[2], formatProvider), double.Parse(tokens[3], formatProvider));
      }

      if (tokens.Length == 2)
      {
        return new Thickness(double.Parse(tokens[0], formatProvider), double.Parse(tokens[1], formatProvider));
      }

      if (tokens.Length == 1)
      {
        return new Thickness(double.Parse(tokens[0], formatProvider));
      }

      return Empty;
    }

    public override string ToString()
    {
      return string.Format("{{Left={0},Top={1},Right={2},Bottom={3}}}", _l, _t, _r, _b);
    }

    #endregion Methods

    #region Properties

    public double Bottom
    {
      get { return _b; }
      set { _b = value; }
    }

    public double Height
    {
      get { return _t + _b; }
    }

    public double Left
    {
      get { return _l; }
      set { _l = value; }
    }

    public double Right
    {
      get { return _r; }
      set { _r = value; }
    }

    public double Top
    {
      get { return _t; }
      set { _t = value; }
    }

    public double Width
    {
      get { return _l + _r; }
    }

    #endregion Properties

    #region Fields

    private double _b;
    private double _l;
    private double _r;
    private double _t;
    public static readonly Thickness Empty = new Thickness();

    #endregion Fields
  }
}