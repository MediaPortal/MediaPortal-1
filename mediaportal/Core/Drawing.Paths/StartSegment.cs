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
using System.Drawing;

namespace MediaPortal.Drawing.Paths
{
	public class StartSegment : PathSegment
	{
		#region Constructors

		public StartSegment()
		{
			_point = Point.Empty;
		}

		public StartSegment(Point point, bool isStroked) : base(isStroked)
		{
			_point = point;
		}

		#endregion Constructors

		#region Properties

		public Point Point
		{
			get { return _point; }
			set { if(Point.Equals(_point, value) == false) { _point = value; RaiseChanged(); } }
		}

		#endregion Properties

		#region Fields

		Point						_point;

		#endregion Fields
	}
}
