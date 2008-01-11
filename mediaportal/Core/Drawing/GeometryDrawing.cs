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

using System;

namespace MediaPortal.Drawing
{
	public sealed class GeometryDrawing
	{
		#region Constructors

		public GeometryDrawing()
		{
		}

		public GeometryDrawing(Brush brush, Pen pen, Geometry geometry)
		{
		}

		#endregion Constructors

		#region Methods

		void RaiseChanged()
		{
		}

		#endregion Methods

		#region Properties

		public Brush Brush
		{
			get { return _brush; }
			set { if(!Brush.Equals(_brush, value)) { _brush = value; RaiseChanged(); } }
		}

		public Geometry Geometry
		{
			get { return _geometry; }
			set { if(!Geometry.Equals(_geometry, value)) { _geometry = value; RaiseChanged(); } }
		}

		public Pen Pen
		{
			get { return _pen; }
			set { if(!Pen.Equals(_pen, value)) { _pen = value; RaiseChanged(); } }
		}

		#endregion Properties

		#region Fields

		Brush						_brush;
		Geometry					_geometry;
		Pen							_pen;

		#endregion Fields
	}
}
