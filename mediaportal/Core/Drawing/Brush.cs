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
using System.ComponentModel;

using MediaPortal.Drawing.Transforms;
using MediaPortal.Drawing.Scenegraph;

namespace MediaPortal.Drawing
{
	[TypeConverter(typeof(BrushConverter))]
	public abstract class Brush : BrushBase, IScenegraphResource
	{
		#region Methods

		void IScenegraphResource.PrepareResource(ScenegraphContext context)
		{
			_isDirty = false;
		}

		void IScenegraphResource.ReleaseResource(ScenegraphContext context)
		{
			_isDirty = true;
		}

		protected void RaiseChanged()
		{
			RaiseChanged(EventArgs.Empty);
		}

		protected void RaiseChanged(EventArgs e)
		{
			_isDirty = true;

			Console.WriteLine("A object of type '{0}' has raised its Changed event", this.GetType().Name);
		}

		#endregion Methods

		#region Properties

		public double Opacity
		{
			get { return _opacity; }
			set { if(double.Equals(_opacity, value) == false) { _opacity = value; RaiseChanged(); } }
		}

		public Transform RelativeTransform
		{
			get { return _relativeTransform; }
			set { if(Transform.Equals(_relativeTransform, value) == false) { _relativeTransform = value; RaiseChanged(); } }
		}

		public Transform Transform
		{
			get { return _transform; }
			set { if(Transform.Equals(_transform, value) == false) { _transform = value; RaiseChanged(); } }
		}

		#endregion Properties

		#region Fields

		bool						_isDirty;
		double						_opacity;
		Transform					_relativeTransform;
		Transform					_transform;

		#endregion Fields
	}
}
