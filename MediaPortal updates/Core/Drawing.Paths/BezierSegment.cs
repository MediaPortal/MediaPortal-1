#region Copyright (C) 2005 Team MediaPortal

/* 
 *	Copyright (C) 2005 Team MediaPortal
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

namespace MediaPortal.Drawing.Paths
{
	public class BezierSegment : PathSegment
	{
		#region Constructors

		public BezierSegment()
		{
			_points[0] = Point.Empty;
			_points[1] = Point.Empty;
			_points[2] = Point.Empty;
		}

		public BezierSegment(Point controlPoint1, Point controlPoint2, Point endpoint, bool isStroked) : base(isStroked)
		{
			_points[0] = controlPoint1;
			_points[1] = controlPoint2;
			_points[2] = endpoint;
		}

		#endregion Constructors

		#region Properties
	
		public Point Point1
		{
			get { return _points[0]; }
			set { if(Point.Equals(_points[0], value) == false) { _points[0] = value; RaiseChanged(); } }
		}
		
		public Point Point2
		{
			get { return _points[1]; }
			set { if(Point.Equals(_points[1], value) == false) { _points[1] = value; RaiseChanged(); } }
		}

		public Point Point3
		{
			get { return _points[2]; }
			set { if(Point.Equals(_points[2], value) == false) { _points[2] = value; RaiseChanged(); } }
		}

		#endregion Properties

		#region Fields

		Point[]						_points = new Point[3];

		#endregion Fields
	}
}
