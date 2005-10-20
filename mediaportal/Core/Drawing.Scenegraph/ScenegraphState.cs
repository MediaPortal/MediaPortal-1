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

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Drawing.Scenegraph
{
	public struct ScenegraphState : IDisposable
	{
		#region Constructors

		internal ScenegraphState(ScenegraphContext context)
		{
			_context = context;
			_opacity = context.Opacity;
			_projection = context.Projection;
			_view = context.View;
			_world = context.World;
		}

		#endregion Constructors

		#region Methods

		public void Dispose()
		{
			Restore();
		}

		public void Restore()
		{
			_context.Projection = _projection;
			_context.View = _view;
			_context.World = _world;
			_context = null;
		}

		#endregion Methods

		#region Fields

		ScenegraphContext			_context;
		double						_opacity;
		Matrix						_projection;
		Matrix						_view;
		Matrix						_world;

		#endregion Fields
	}
}
