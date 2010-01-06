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

using MediaPortal.Drawing.Transforms;

namespace MediaPortal.Drawing
{
  public class RectangleGeometry : Geometry
  {
    #region Constructors

    public RectangleGeometry() {}

    public RectangleGeometry(Rect rect)
    {
      _rect = rect;
    }

    public RectangleGeometry(Rect rect, double radiusX, double radiusY)
    {
      _rect = rect;
      _radiusX = radiusX;
      _radiusY = radiusY;
    }

    public RectangleGeometry(Rect rect, double radiusX, double radiusY, Transform transform)
    {
      _rect = rect;
      _radiusX = radiusX;
      _radiusY = radiusY;
      _transform = transform;
    }

    #endregion Constructors

    #region Methods

    public void blah()
    {
//			_svg_cairo_length_to_pixel (svg_cairo, x_len, &x);
//			_svg_cairo_length_to_pixel (svg_cairo, y_len, &y);
//			_svg_cairo_length_to_pixel (svg_cairo, width_len, &width);
//			_svg_cairo_length_to_pixel (svg_cairo, height_len, &height);
//			_svg_cairo_length_to_pixel (svg_cairo, _radiusX_len, &_radiusX);
//			_svg_cairo_length_to_pixel (svg_cairo, _radiusY_len, &_radiusY);

      if (_radiusX > _rect.Width / 2.0)
      {
        _radiusX = _rect.Width / 2.0;
      }

      if (_radiusY > _rect.Height / 2.0)
      {
        _radiusY = _rect.Height / 2.0;
      }

/*			PathFigure figure = new PathFigure();

			figure.IsFilled = true;
			figure.IsClosed = true;

			if(_radiusX > 0 || _radiusY > 0)
			{
				figure.StartPoint = new Point(_rect.Left + _radiusX, _rect.Top);
				figure.Segments.Add(new LineSegment(new Point(_rect.Right - _radiusX, _rect.Top), true));
				figure.Segments.Add(new ArcSegment(new Point(_rect.Right, _rect.Top + _radiusY), new Size(_radiusX, _radiusY), 0, false, SweepDirection.Clockwise, true));
				figure.Segments.Add(new LineSegment(new Point(_rect.Right, _rect.Bottom - _radiusY), true));
				figure.Segments.Add(new ArcSegment(new Point(_rect.Right - _radiusX, _rect.Bottom), new Size(_radiusX, _radiusY), 0, false, SweepDirection.Clockwise, true));
				figure.Segments.Add(new LineSegment(new Point(_rect.Left + _radiusX, _rect.Bottom), true));
				figure.Segments.Add(new ArcSegment(new Point(_rect.Left, _rect.Bottom - _radiusY), new Size(_radiusX, _radiusY), 0, false, SweepDirection.Clockwise, true));
				figure.Segments.Add(new LineSegment(new Point(_rect.Left, _rect.Top + _radiusY), true));
				figure.Segments.Add(new ArcSegment(new Point(_rect.Left + _radiusX, _rect.Top), new Size(_radiusX, _radiusY), 0, false, SweepDirection.Clockwise, true));
				figure.Segments.Add(new CloseSegment(true));
			}
			else
			{
				figure.StartPoint = new Point(_rect.Left, _rect.Top);
				figure.Segments.Add(new LineSegment(new Point(_rect.Right, _rect.Top), true));
				figure.Segments.Add(new LineSegment(new Point(_rect.Right, _rect.Bottom), true));
				figure.Segments.Add(new LineSegment(new Point(_rect.Left, _rect.Bottom), true));
				figure.Segments.Add(new CloseSegment(true));
			}
*/
    }

    #endregion Methods

    #region Properties

//		public override Rect Bounds
//		{
//			get ;
//		}

    public double RadiusX
    {
      get { return _radiusX; }
      set { }
    }

    public double RadiusY
    {
      get { return _radiusY; }
      set { }
    }

    public Rect Rect
    {
      get { return _rect; }
      set { }
    }

    #endregion Properties

    #region Fields

    private Transform _transform;
    private double _radiusX;
    private double _radiusY;
    private Rect _rect;
//		PathFigure					_pathFigure;

    #endregion Fields
  }
}