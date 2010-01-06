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
using System.Drawing;
using Microsoft.DirectX;

namespace MediaPortal.Drawing
{
  [TypeConverter(typeof (PointConverter))]
  public struct Point
  {
    #region Constructors

    public Point(Size s) : this(s.Width, s.Height) {}

    public Point(double x, double y)
    {
      _x = x;
      _y = y;
    }

    #endregion Constructors

    #region Operators

    public static Point operator +(Point l, Point r)
    {
      return new Point(l.X + r.X, l.Y + r.Y);
    }

    public static Point operator -(Point l, Point r)
    {
      return new Point(l.X - r.X, l.Y - r.Y);
    }

    public static bool operator ==(Point l, Point r)
    {
      return l._x == r._x && l._y == r._y;
    }

    public static bool operator !=(Point l, Point r)
    {
      return !(l._x == r._x && l._y == r._y);
    }

    public static implicit operator Vector2(Point point)
    {
      return new Vector2((float)point.X, (float)point.Y);
    }

    public static implicit operator Vector3(Point point)
    {
      return new Vector3((float)point.X, (float)point.Y, 0);
    }

    #endregion Operators

    #region Methods

    public override bool Equals(object o)
    {
      return o is Point && ((Point)o)._x == _x && ((Point)o)._y == _y;
    }

    public override int GetHashCode()
    {
      return (int)((uint)_x ^ (uint)_y);
    }

    public void Offset(Point point)
    {
      _x += point.X;
      _y += point.Y;
    }

    public void Offset(double dx, double dy)
    {
      _x += dx;
      _y += dy;
    }

    public static Point Lerp(Point a, Point b)
    {
//			TODO:
//			result->x = a->x + ((b->x - a->x) >> 1);
//			result->y = a->y + ((b->y - a->y) >> 1);
      return new Point(a.X + ((int)(b.X - a.X) >> 1), a.Y + ((int)(b.Y - a.Y) >> 1));
    }

    public PointF ToPointF()
    {
      return new PointF(Convert.ToSingle(_x), Convert.ToSingle(_y));
    }

    public override string ToString()
    {
      return string.Format("{{X={0},Y={1}}}", _x, _y);
    }

    public Vector2 ToVector2()
    {
      return new Vector2((float)_x, (float)_y);
    }

    public Vector3 ToVector3()
    {
      return new Vector3((float)_x, (float)_y, 0);
    }

    #endregion Methods

    #region Properties

    public static Point Empty
    {
      get { return _empty; }
    }

    public bool IsEmpty
    {
      get { return _x == 0 && _y == 0; }
    }

    public double X
    {
      get { return _x; }
      set { _x = value; }
    }

    public double Y
    {
      get { return _y; }
      set { _y = value; }
    }

    #endregion Properties

    #region Fields

    private double _x;
    private double _y;
    private static readonly Point _empty = new Point();

    #endregion Fields
  }
}