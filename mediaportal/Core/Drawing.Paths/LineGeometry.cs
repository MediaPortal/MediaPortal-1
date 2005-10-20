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

namespace MediaPortal.Drawing.Paths
{
	public class LineGeometry : Geometry
	{
		#region Constructors

		public LineGeometry()
		{
			_startPoint = Point.Empty;
			_endPoint = Point.Empty;
		}

		public LineGeometry(Point startPoint, Point endPoint)
		{
			_startPoint = startPoint;
			_endPoint = endPoint;
		}

		public LineGeometry(Point startPoint, Point endPoint, Transform transform)
		{
			_startPoint = startPoint;
			_endPoint = endPoint;
			_transform = transform;
		}

		#endregion Constructors

		#region Methods

//		public override bool MayHaveCurves()
//		{
//			// a line cannot have curves
//			return false;
//		}

//		public override double GetArea(double tolerance, ToleranceType type)
//		{
//			return 0;
//		}
			
//		public override bool IsEmpty()
//		{
//			return Point.Equals(_startPoint, _endPoint);
//		}

		#endregion Methods

		#region Fields

		Point						_endPoint;
		Point						_startPoint;
		Transform					_transform;

		#endregion Fields
	}
}
