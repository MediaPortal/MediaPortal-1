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

using MediaPortal.Drawing.Scenegraph;
using MediaPortal.Drawing.Shapes;

namespace MediaPortal.Drawing
{
	public abstract class TileBrush : Brush, IScenegraphResource
	{
		#region Methods

		void IScenegraphResource.PrepareResource(ScenegraphContext context)
		{
		}

		void IScenegraphResource.ReleaseResource(ScenegraphContext context)
		{
		}

		#endregion Methods

		#region Properties

		public AlignmentX AlignmentX
		{
			get { return _alignmentX; }
			set { if(AlignmentX.Equals(_alignmentX, value) == false) { _alignmentX = value; RaiseChanged(); } }
		}

		public AlignmentY AlignmentY
		{
			get { return _alignmentY; }
			set { if(AlignmentY.Equals(_alignmentY, value) == false) { _alignmentY = value; RaiseChanged(); } }
		}

		public Stretch Stretch
		{
			get { return _stretch; }
			set { if(Stretch.Equals(_stretch, value) == false) { _stretch = value; RaiseChanged(); } }
		}

		public TileMode	TileMode
		{
			get { return _tileMode; }
			set { if(TileMode.Equals(_tileMode, value) == false) { _tileMode = value; RaiseChanged(); } }
		}

		public Rect Viewbox
		{ 
			get { return _viewbox; }
			set { if(Rect.Equals(_viewbox, value) == false) { _viewbox = value; RaiseChanged(); } }
		}

		public BrushMappingMode ViewboxUnits
		{
			get { return _viewboxUnits; }
			set { if(BrushMappingMode.Equals(_viewboxUnits, value) == false) { _viewboxUnits = value; RaiseChanged(); } }
		}

		public Rect Viewport
		{
			get { return _viewport; }
			set { if(Rect.Equals(_viewport, value) == false) { _viewport = value; RaiseChanged(); } }
		}

		public BrushMappingMode ViewportUnits
		{
			get { return _viewportUnits; }
			set { if(BrushMappingMode.Equals(_viewportUnits, value) == false) { _viewportUnits = value; RaiseChanged(); } }
		}

		#endregion Properties

		#region Fields

		AlignmentX					_alignmentX;
		AlignmentY					_alignmentY;
		Stretch						_stretch;
		TileMode					_tileMode;
		Rect						_viewbox;
		BrushMappingMode			_viewboxUnits;
		Rect						_viewport;
		BrushMappingMode			_viewportUnits;

		#endregion Fields
	}
}
