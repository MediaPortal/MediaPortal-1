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
using System.Windows.Serialization;

using MediaPortal.Drawing.Shapes;

namespace MediaPortal.Drawing.Paths
{
	public class PathGeometry : Geometry, IAddChild
	{
		#region Constructors

		public PathGeometry()
		{
		}

		#endregion Constructors

		#region Methods

		void IAddChild.AddChild(object child)
		{

		}

		void IAddChild.AddText(string text)
		{
			throw new NotSupportedException();
		}

		#endregion Methods

		#region Properties

		public override Rect Bounds
		{
			get { return _bounds; }
		}

		public FillRule FillRule
		{
			get { return _fillRule; }
			set { if(FillRule.Equals(_fillRule, value) == false) { _fillRule = value; RaiseChanged(); } }
		}

		public PathFigureCollection Figures
		{ 
			get { if(_figures == null) _figures = new PathFigureCollection(); return _figures; }
//			set { throw new NotImplementedException(); }
		}

		#endregion Properties

		#region Fields

		Rect						_bounds = Rect.Empty;
		FillRule					_fillRule;
		PathFigureCollection		_figures;

		#endregion Fields
	}
}
