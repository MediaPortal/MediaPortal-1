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

using System.ComponentModel;

namespace MediaPortal.Drawing
{
  [TypeConverter(typeof (SizeConverter))]
  public struct Size
  {
    #region Constructors

    public Size(System.Drawing.Size size) : this(size.Width, size.Height)
    {
    }

    public Size(Point location) : this(location.X, location.Y)
    {
    }

    public Size(System.Drawing.Point location) : this(location.X, location.Y)
    {
    }

    public Size(double w, double h)
    {
      _w = w;
      _h = h;
    }

    #endregion Constructors

    #region Operators

    public static Size operator +(Size l, Size r)
    {
      return Add(l, r);
    }

    public static bool operator ==(Size l, Size r)
    {
      return l._w == r._w && l._h == r._h;
    }

    public static bool operator !=(Size l, Size r)
    {
      return !(l._w == r._w && l._h == r._h);
    }

    #endregion Operators

    #region Methods

    public static Size Add(Size l, Size r)
    {
      return new Size(l._w + r._w, l._h + r._h);
    }

    public override bool Equals(object o)
    {
      return o is Size && ((Size) o)._w == _h && ((Size) o)._w == _h;
    }

    public override int GetHashCode()
    {
      return (int) ((uint) _w ^ (uint) _h);
    }

    public override string ToString()
    {
      return string.Format("{{Width={0},Height={1}}}", _w, _h);
    }

    #endregion Methods

    #region Properties

    public static Size Empty
    {
      get { return _empty; }
    }

    public double Height
    {
      get { return _h; }
      set { _h = value; }
    }

    public bool IsEmpty
    {
      get { return _w == 0 && _h == 0; }
    }

    public double Width
    {
      get { return _w; }
      set { _w = value; }
    }

    #endregion Properties

    #region Fields

    private double _h;
    private double _w;
    private static readonly Size _empty = new Size();

    #endregion Fields
  }
}