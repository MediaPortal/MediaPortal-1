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
using System.Collections;

namespace MediaPortal.Drawing.Layouts
{
	public class StackLayout : ILayout
	{
		#region Constructors

		public StackLayout()
		{
		}

		public StackLayout(int spacing)
		{
			_spacing = new Size(Math.Max(0, spacing), Math.Max(0, spacing));
		}

		public StackLayout(int spacing, Orientation orientation)
		{
			_spacing = new Size(Math.Max(0, spacing), Math.Max(0, spacing));
			_orientation = orientation;
		}

		#endregion Constructors

		#region Methods

		void ApplyAlignment(ILayoutComponent child, Thickness t, double x, double y, double w, double h)
		{
			Rect rect = new Rect(x, y, child.Size.Width, child.Size.Height);
            
			switch(child.HorizontalAlignment)
			{
				case HorizontalAlignment.Center:
					rect.X = x + ((w - child.Size.Width) / 2);
					break;
				case HorizontalAlignment.Right:
					rect.X = x + w  - child.Size.Width;
					break;
				case HorizontalAlignment.Stretch:
					rect.Width = w;
					break;
			}

			switch(child.VerticalAlignment)
			{
				case VerticalAlignment.Center:
					rect.Y = y + ((h - child.Size.Height) / 2);
					break;
				case VerticalAlignment.Bottom:
					rect.Y = y + h  - child.Size.Height;
					break;
				case VerticalAlignment.Stretch:
					rect.Height = h;
					break;
			}
		
			child.Arrange(rect);
		}

		public void Arrange(ILayoutComposite composite)
		{
			Thickness t = composite.Margin;
			Point l = composite.Location;

			double x = composite.Location.X + t.Left;
			double y = composite.Location.Y + t.Top;
			double w = _orientation != Orientation.Horizontal ? Math.Max(0, composite.Size.Width - t.Width) : 0;
			double h = _orientation == Orientation.Horizontal ? Math.Max(0, composite.Size.Height - t.Height) : 0;

			foreach(ILayoutComponent child in composite.Children)
			{
				if(child.IsVisible == false)
					continue;

				if(_orientation == Orientation.Horizontal)
				{
					ApplyAlignment(child, t, x, y, w = child.Size.Width, h);

					x += w + _spacing.Width;

					continue;
				}

				ApplyAlignment(child, t, x, y, w, h = child.Size.Height);

				y += h + _spacing.Height;
			}
		}

		public Size Measure(ILayoutComposite composite, Size availableSize)
		{
			double w = 0;
			double h = 0;

			foreach(ILayoutComponent child in composite.Children)
			{
				if(child.IsVisible == false)
					continue;

				Size s = child.Measure();

				w = _orientation != Orientation.Horizontal ? Math.Max(w, s.Width) : w + s.Width + _spacing.Width;
				h = _orientation == Orientation.Horizontal ? Math.Max(h, s.Height) : h + s.Height + _spacing.Height;
			}

			Thickness t = composite.Margin;

			_size.Width = w + t.Width;
			_size.Height = h + t.Height;

			return _size;
		}

		#endregion Methods

		#region Properties

		public Orientation Orientation
		{
			get { return _orientation; }
			set { _orientation = value; }
		}

		public Size Size
		{
			get { return _size; }
		}

		public Size Spacing
		{
			get { return _spacing; }
			set { _spacing = value; }
		}

		#endregion Properties

		#region Fields

		Orientation					_orientation = Orientation.Vertical;
		Size						_spacing = Size.Empty;
		Size						_size = Size.Empty;

		#endregion Fields
	}
}
