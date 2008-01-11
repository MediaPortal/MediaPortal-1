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
using System.Windows.Serialization;

using MediaPortal.Drawing.Transforms;

namespace MediaPortal.Drawing
{
	public class CombinedGeometry : Geometry, IAddChild
	{
		#region Constructors

		public CombinedGeometry()
		{
		}

		public CombinedGeometry(Geometry geometry1, Geometry geometry2)
		{
			_geometry1 = geometry1;
			_geometry2 = geometry2;
		}

		public CombinedGeometry(GeometryCombineMode mode, Geometry geometry1, Geometry geometry2)
		{
			_mode = mode;
			_geometry1 = geometry1;
			_geometry2 = geometry2;
		}

		public CombinedGeometry(GeometryCombineMode mode, Geometry geometry1, Geometry geometry2, Transform transform)
		{
			_mode = mode;
			_geometry1 = geometry1;
			_geometry2 = geometry2;
			_transform = transform;
		}

		#endregion Constructors

		#region Methods

		void IAddChild.AddChild(object child)
		{
			if(child == null)
				throw new ArgumentNullException("child");

			if(child is Geometry == false)
				throw new ArgumentException("child");

			if(_geometryIndex == 0)
				_geometry1 = (Geometry)child;

			if(_geometryIndex == 1)
				_geometry2 = (Geometry)child;

			if(_geometryIndex++ == 1)
				_geometryIndex = 0;
		}

		void IAddChild.AddText(string text)
		{
		}

		#endregion Methods

		#region Properties

		public GeometryCombineMode GeometryCombineMode
		{
			get { return _mode; }
			set { }
		}

		public Geometry Geometry1
		{
			get { return _geometry1; }
			set { }
		}

		public Geometry Geometry2
		{
			get { return _geometry2; }
			set { }
		}

		#endregion Properties

		#region Fields

		GeometryCombineMode			_mode;
		Geometry					_geometry1;
		Geometry					_geometry2;
		int							_geometryIndex;
		Transform					_transform;

		#endregion Fields
	}
}