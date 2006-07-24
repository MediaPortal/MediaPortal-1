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
using System.ComponentModel;

namespace MediaPortal.Drawing.Paths
{
	[TypeConverter(typeof(PathSegmentConverter))]
	public class PathSegment
	{
		#region Constructors

		public PathSegment()
		{
		}

		public PathSegment(bool isStroked)
		{
			_isStroked = isStroked;
		}

		#endregion Constructors

		#region Methods

		protected void RaiseChanged()
		{
			_isDirty = true;

			// prevent compiler warnings
			if(_isDirty)
				_isDirty = true;
		}

		#endregion Methods

		#region Properties

		public bool IsSmoothJoin
		{
			get { return _isSmoothJoin; }
			set { _isSmoothJoin = value; }
		}

		public bool IsStroked
		{
			get { return _isStroked; }
			set { _isStroked = value; }
		}

		#endregion Properties

		#region Fields

		bool						_isDirty = true;
		bool						_isSmoothJoin;
		bool						_isStroked;

		#endregion Fields
	}
}
