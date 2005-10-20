#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using MediaPortal.Drawing.Transforms;

namespace MediaPortal.Drawing
{
	public class EllipseGeometry : Geometry
	{
		#region Constructors

		public EllipseGeometry()
		{
		}

		public EllipseGeometry(Rect rect)
		{
			_center.X = rect.Left + (rect.Width / 2);
			_center.Y = rect.Top + (rect.Height / 2);
			_radiusX = rect.Width;
			_radiusY = rect.Height;
		}

		public EllipseGeometry(Point center, double radiusX, double radiusY)
		{
			_center = center;
			_radiusX = radiusX;
			_radiusY = radiusY;
		}
		
		public EllipseGeometry(Point center, double radiusX, double radiusY, Transform transform)
		{
			_center = center;
			_radiusX = radiusX;
			_radiusY = radiusY;
			_transform = transform;
		}
		
		#endregion Constructors

		#region Properties

//		public override Rect Bounds
//		{
//			get ;
//		}

		public Point Center
		{
			get { return _center; }
			set { if(!Point.Equals(_center, value)) { _center = value; RaiseChanged(); } }
		}

		public double RadiusX
		{
			get { return _radiusX; }
			set { if(!double.Equals(_radiusX, value)) { _radiusX = value; RaiseChanged(); } }
		}

		public double RadiusY
		{
			get { return _radiusY; }
			set { if(!double.Equals(_radiusY, value)) { _radiusY = value; RaiseChanged(); } }
		}

		public Rect Rect
		{
			get { return _rect; }
			set { if(!Rect.Equals(_rect, value)) { _rect = value; RaiseChanged(); } }
		}

		#endregion Properties

		#region Fields

		Point						_center;
		Transform					_transform;
		double						_radiusX;
		double						_radiusY;
		Rect						_rect;
		
		#endregion Fields
	}
}
