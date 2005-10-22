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

using MediaPortal.Controls;
using MediaPortal.Drawing;

namespace MediaPortal.Drawing.Layouts
{
	public class DockLayout : ILayout
	{
		#region Constructors

		public DockLayout()
		{
			_spacing = new Size(0, 0);
		}

		public DockLayout(int horizontalSpacing, int verticalSpacing)
		{
			_spacing = new Size(Math.Max(0, horizontalSpacing), Math.Max(0, verticalSpacing));
		}

		#endregion Constructors

		#region Methods

		public void Arrange(FrameworkElement element)
		{
			ILayoutComponent l = null;
			ILayoutComponent t = null;
			ILayoutComponent r = null;
			ILayoutComponent b = null;
			ILayoutComponent f = null;

//			foreach(ILayoutComponent child in element.Children)
			{
//				if(child.Dock == Dock.Left)
//					l = child;

//				if(child.Dock == Dock.Top)
//					t = child;

//				if(child.Dock == Dock.Right)
//					r = child;

//				if(child.Dock == Dock.Bottom)
//					b = child;

//				if(child.Dock == Dock.Fill)
//					f = child;
			}

			Thickness m = element.Margin;
			Size size = element.RenderSize;
			Point location = element.Location;

			double top = location.Y + m.Top;
			double bottom = location.Y + size.Height - m.Bottom;
			double left = location.X + m.Left;
			double right = location.X + size.Width - m.Right;

			if(t != null)
			{
				Size s = t.Size;

				t.Arrange(new Rect(left, top, right - left, s.Height));

				top = top + s.Height + _spacing.Height;
			}

			if(b != null)
			{
				Size s = b.Size;

				b.Arrange(new Rect(left, bottom - s.Height, right - left, s.Height));

				bottom = bottom - (s.Height + _spacing.Height);
			}

			if(r != null)
			{
				Size s = r.Size;

				r.Arrange(new Rect(right - s.Width, top, s.Width, bottom - top));

				right = right - (s.Width + _spacing.Width);
			}

			if(l != null)
			{
				Size s = l.Size;

				l.Arrange(new Rect(left, top, s.Width, bottom - top));

				left = left + s.Width + _spacing.Width;
			}

			if(f != null)
				f.Arrange(new Rect(left, top, right - left, bottom - top));
		}
	
		public Size Measure(FrameworkElement element, Size availableSize)
		{
			ILayoutComponent l = null;
			ILayoutComponent t = null;
			ILayoutComponent r = null;
			ILayoutComponent b = null;
			ILayoutComponent f = null;

//			foreach(ILayoutComponent child in element.Children)
			{
//				if(child.Dock == Dock.Left)
//					l = child;

//				if(child.Dock == Dock.Top)
//					t = child;

//				if(child.Dock == Dock.Right)
//					r = child;

//				if(child.Dock == Dock.Bottom)
//					b = child;

//				if(child.Dock == Dock.Fill)
//					f = child;
			}

			double w = 0;
			double h = 0;

			Size s = Size.Empty;

			if(r != null)
			{
				s = r.Measure();
				w = s.Width + _spacing.Width;
				h = Math.Max(h, s.Height);
			}

			if(l != null)
			{
				s = l.Measure();
				w = s.Width + _spacing.Width;
				h = Math.Max(h, s.Height);
			}

			if(f != null)
			{
				s = f.Measure();
				w = w + s.Width;
				h = Math.Max(h, s.Height);
			}

			if(t != null)
			{
				s = t.Measure();
				w = Math.Max(w, s.Width);
				h = h + s.Height + _spacing.Height;
			}

			if(b != null)
			{
				s = b.Measure();
				w = Math.Max(w, s.Width);
				h = h + s.Height + _spacing.Height;
			}

			Thickness m = element.Margin;

			_size.Width = w + m.Width;
			_size.Height = h + m.Height;

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

		Size						_spacing = Size.Empty;
		Size						_size = Size.Empty;

		#endregion Fields
	}
}
