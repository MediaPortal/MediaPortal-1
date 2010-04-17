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

namespace MediaPortal.Drawing
{
  [TypeConverter(typeof (RectConverter))]
  public struct Rect
  {
    #region Constructors

    public Rect(Point location, Size size) : this(location.X, location.Y, size.Width, size.Height) {}

    public Rect(double x, double y, double w, double h)
    {
      _location = new Point(x, y);
      _size = new Size(w, h);
    }

    #endregion Constructors

    #region Operators

    public static bool operator ==(Rect l, Rect r)
    {
      return l._location.X == r._location.X && l._location.Y == r._location.Y && l._size.Width == r._size.Width &&
             l._size.Height == r._size.Height;
    }

    public static bool operator !=(Rect l, Rect r)
    {
      return
        !(l._location.X == r._location.X && l._location.Y == r._location.Y && l._size.Width == r._size.Width &&
          l._size.Height == r._size.Height);
    }

    #endregion Operators

    #region Methods

    public bool Contains(Point point)
    {
      return point.X >= _location.X && point.Y >= _location.Y && point.X <= _location.X + _size.Width &&
             point.Y <= _location.Y + _size.Height;
    }

    public bool Contains(double x, double y)
    {
      return x >= _location.X && y >= _location.Y && x <= _location.X + _size.Width && y <= _location.Y + _size.Height;
    }

    public bool Contains(Point point, Matrix matrix)
    {
      return Contains(point.X, point.Y, matrix);
    }

    public bool Contains(double x, double y, Matrix matrix)
    {
/*			Vector3 n = new Vector3((float)x, (float)y, 0);
			Vector3 f = new Vector3((float)x, (float)y, 1);

			float r = (float)_size.Width;
			float b = (float)_size.Height;

			Vector3 tr = new Vector3(r, 0, 0);
			Vector3 bl = new Vector3(0, b, 0);
			Vector3 br = new Vector3(r, b, 0);

			n.Unproject(DrawingContext.Device.Viewport, DrawingContext.Device.Transform.Projection, DrawingContext.Device.Transform.View, matrix);
			f.Unproject(DrawingContext.Device.Viewport, DrawingContext.Device.Transform.Projection, DrawingContext.Device.Transform.View, matrix);
			f.Subtract(n);

			IntersectInformation intersect;

			if(Microsoft.DirectX.Direct3D.Geometry.IntersectTri(Vector3.Empty, tr, bl, n, f, out intersect))
				return true;

			if(Microsoft.DirectX.Direct3D.Geometry.IntersectTri(bl, br, tr, n, f, out intersect))
				return true;

*/
      return false;
    }

    public override bool Equals(object o)
    {
      return o is Rect && ((Rect)o)._location.X == _location.X && ((Rect)o)._location.Y == _location.Y &&
             ((Rect)o)._size.Width == _size.Width && ((Rect)o)._size.Height == _size.Height;
    }

    public override int GetHashCode()
    {
      return (int)((uint)_location.X ^ (uint)_location.Y ^ (uint)_size.Width ^ (uint)_size.Height);
    }

    public override string ToString()
    {
      return string.Format("{{X={0},Y={1},Width={2},Height={3}}}", _location.X, _location.Y, _size.Width, _size.Height);
    }

    #endregion Methods

    #region Properties

    public static Rect Empty
    {
      get { return _empty; }
    }

    public double Bottom
    {
      get { return _location.Y + _size.Height; }
    }

    public double Height
    {
      get { return _size.Height; }
      set { _size.Height = value; }
    }

    public bool IsEmpty
    {
      get { return _location.X == 0 && _location.Y == 0 && _size.Width == 0 && _size.Height == 0; }
    }

    public double Left
    {
      get { return _location.X; }
    }

    public Point Location
    {
      get { return _location; }
      set { _location = value; }
    }

    public double Right
    {
      get { return _location.X + _size.Width; }
    }

    public Size Size
    {
      get { return _size; }
      set { _size = value; }
    }

    public double Top
    {
      get { return _location.Y; }
    }

    public double Width
    {
      get { return _size.Width; }
      set { _size.Width = value; }
    }

    public double X
    {
      get { return _location.X; }
      set { _location.X = value; }
    }

    public double Y
    {
      get { return _location.Y; }
      set { _location.Y = value; }
    }

    #endregion Properties

    #region Fields

    private Point _location;
    private Size _size;
    private static readonly Rect _empty = new Rect();

    #endregion Fields
  }
}