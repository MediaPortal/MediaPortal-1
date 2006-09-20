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
	public class ArcSegment : PathSegment
	{
		#region Constructors

		public ArcSegment()
		{
			_point = Point.Empty;
			_size = Size.Empty;
			_rotationAngle = 0;
			_isLargeArc = false;
			_sweepDirection = SweepDirection.Clockwise;
		}

		public ArcSegment(Point endPoint, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked) : base(isStroked)
		{
			_point = endPoint;
			_size = size;
			_rotationAngle = rotationAngle;
			_isLargeArc = isLargeArc;
			_sweepDirection = sweepDirection;
		}

		#endregion Constructors

		#region Properties

		public bool LargeArc
		{
			get { return _isLargeArc; }
			set { if(!bool.Equals(_isLargeArc, value)) { _isLargeArc = value; RaiseChanged(); } }
		}

		public Point Point
		{
			get { return _point; }
			set { if(!Point.Equals(_point, value)) { _point = value; RaiseChanged(); } }
		}

		public double Rotation
		{
			get { return _rotationAngle; }
			set { if(!double.Equals(_rotationAngle, value)) { _rotationAngle = value; RaiseChanged(); } }
		}

		public Size Size
		{
			get { return _size; }
			set { if(!Size.Equals(_size, value)) { _size = value; RaiseChanged(); } }
		}

		public SweepDirection SweepDirection
		{
			get { return _sweepDirection; }
			set { if(!Size.Equals(_sweepDirection, value)) { _sweepDirection = value; RaiseChanged(); } }
		}

		#endregion Properties

		#region Fields
		
		Point						_point;
		Size						_size;
		double						_rotationAngle;
		bool						_isLargeArc;
		SweepDirection				_sweepDirection;

		#endregion Fields
	}
}
