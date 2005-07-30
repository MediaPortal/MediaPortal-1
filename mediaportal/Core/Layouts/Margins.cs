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
using System;

namespace MediaPortal.Layouts
{
	public struct Margins
	{
		#region Constructors

		public Margins(int horizontal, int vertical)
		{
			_left = _right = horizontal;
			_top = _bottom = vertical;
		}

		public Margins(int left, int top, int right, int bottom)
		{
			_left = left;
			_top = top;
			_right = right;
			_bottom = bottom;
		}

		#endregion Constructors

		#region Methods

		public override int GetHashCode()
		{
			return _left ^ _top ^ _right ^ _bottom;
		}

		#endregion Methods

		#region Properties

		public int Bottom
		{
			get { return _bottom; }
			set { _bottom = value; }
		}

		public int Height
		{
			get { return _top + _bottom; }
		}

		public int Left
		{
			get { return _left; }
			set { _left = value; }
		}

		public int Right
		{
			get { return _right; }
			set { _right = value; }
		}

		public int Top
		{
			get { return _top; }
			set { _top = value; }
		}

		public int Width
		{
			get { return _left + _right; }
		}

		#endregion Properties

		#region Fields

		int								_bottom;
		int								_left;
		int								_right;
		int								_top;
		public static readonly Margins	Empty = new Margins();

		#endregion Fields
	}
}
