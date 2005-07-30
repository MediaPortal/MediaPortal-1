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
using System.Collections;
using System.Drawing;

namespace MediaPortal.Layouts
{
	public class StackLayout : ILayout
	{
		#region Constructors

		public StackLayout() : this(0, Orientation.Vertical)
		{
		}

		public StackLayout(int spacing) : this(spacing, Orientation.Vertical)
		{
		}

		public StackLayout(int spacing, Orientation orientation)
		{
			_spacing = new Size(Math.Max(0, spacing), Math.Max(0, spacing));
			_orientation = orientation;
		}

		#endregion Constructors

		#region Methods

		public void Arrange(ILayoutComposite composite)
		{
			Margins m = composite.Margins;
			Point l = composite.Location;

			Rectangle r = new Rectangle();

			int x = l.X + m.Left;
			int y = l.Y + m.Top;
			int w = _orientation != Orientation.Horizontal ? composite.Size.Width - m.Right : 0;
			int h = _orientation == Orientation.Horizontal ? composite.Size.Height - m.Bottom : 0;

			foreach(ILayoutComponent child in composite.Children)
			{
				if(child.Visible == false)
					continue;

				if(_orientation != Orientation.Horizontal)
				{
					child.Arrange(new Rectangle(x, y, w, h = child.Size.Height));
					y += h + _spacing.Height;
				}

				if(_orientation == Orientation.Horizontal)
				{
					child.Arrange(new Rectangle(x, y, w = child.Size.Width, h));
					x += w + _spacing.Width;
				}
			}
		}

		public Size Measure(ILayoutComposite composite, Size availableSize)
		{
			int w = 0;
			int h = 0;

			foreach(ILayoutComponent child in composite.Children)
			{
				if(child.Visible == false)
					continue;

				Size s = child.Measure();

				w = _orientation != Orientation.Horizontal ? Math.Max(w, s.Width) : w + s.Width + _spacing.Width;
				h = _orientation == Orientation.Horizontal ? Math.Max(h, s.Height) : h + s.Height + _spacing.Height;
			}

			Margins margins = composite.Margins;

			_size.Width += w + margins.Width;
			_size.Height += h + margins.Height;

			return _size;
		}

		#endregion Methods

		#region Properties

		public Size Size
		{
			get { return _size; }
		}

		public Size Spacing
		{
			get { return _spacing; }
			set { if(Size.Equals(_spacing, value) == false) { _spacing = value; } }
		}

		#endregion Properties

		#region Fields

		Orientation					_orientation = Orientation.Vertical;
		Size						_spacing = Size.Empty;
		Size						_size = Size.Empty;

		#endregion Fields
	}
}
