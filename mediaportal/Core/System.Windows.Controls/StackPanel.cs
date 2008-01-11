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
using System.Windows;

using MediaPortal.Drawing;
using MediaPortal.Drawing.Layouts;

namespace System.Windows.Controls
{
	public class StackPanel : Panel, IScrollInfo
	{
		#region Constructors

		static StackPanel()
		{
			OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(StackPanel), new PropertyMetadata(Orientation.Vertical));
		}

		public StackPanel()
		{
		}

		#endregion Constructors

		#region Methods

		void ApplyAlignment(FrameworkElement element, double x, double y, double w, double h)
		{
			Rect rect = new Rect(x, y, element.Width, element.Height);
            
			switch(element.HorizontalAlignment)
			{
				case HorizontalAlignment.Center:
					rect.X = x + ((w - element.Width) / 2);
					break;
				case HorizontalAlignment.Right:
					rect.X = x + w  - element.Width;
					break;
				case HorizontalAlignment.Stretch:
					rect.Width = w;
					break;
			}

			switch(element.VerticalAlignment)
			{
				case VerticalAlignment.Center:
					rect.Y = y + ((h - element.Height) / 2);
					break;
				case VerticalAlignment.Bottom:
					rect.Y = y + h  - element.Height;
					break;
				case VerticalAlignment.Stretch:
					rect.Height = h;
					break;
			}
		
			element.Arrange(rect);
		}

		protected override Size ArrangeOverride(Rect finalRect)
		{
			Orientation orientation = Orientation;
			Thickness margin = Margin;

			double x = Location.X + margin.Left;
			double y = Location.Y + margin.Top;
			double w = orientation != Orientation.Horizontal ? Math.Max(0, Width - margin.Width) : 0;
			double h = orientation == Orientation.Horizontal ? Math.Max(0, Height - margin.Height) : 0;

			foreach(FrameworkElement child in Children)
			{
				if(child.Visibility == System.Windows.Visibility.Collapsed)
					continue;

				if(orientation == Orientation.Horizontal)
				{
					ApplyAlignment(child, x, y, w = child.Width, h);

					x += w + child.Margin.Width;
				}
				else
				{
					ApplyAlignment(child, x, y, w, h = child.Height);

					y += h + child.Margin.Height;
				}
			}

			return Size.Empty;
		}

		public void LineDown()
		{
		}

		public void LineLeft()
		{
		}

		public void LineRight()
		{
		}

		public void LineUp()
		{
		}

//		public Rect MakeVisible(Visual visual, Rect rectangle)
//		{
//		}

		protected override Size MeasureOverride(Size availableSize)
		{
			double w = 0;
			double h = 0;

			Orientation orientation = this.Orientation;
			Thickness t = this.Margin;

			foreach(FrameworkElement child in this.Children)
			{
				if(child.Visibility == System.Windows.Visibility.Collapsed)
					continue;

				child.Measure(availableSize);

				w = orientation != Orientation.Horizontal ? Math.Max(w, child.Width) : w + child.Width + t.Width;
				h = orientation == Orientation.Horizontal ? Math.Max(h, child.Height) : h + child.Height + t.Height;
			}

			_size.Width = w + t.Width;
			_size.Height = h + t.Height;

			return _size;
		}

		public void MouseWheelDown()
		{
		}

		public void MouseWheelLeft()
		{
		}

		public void MouseWheelRight()
		{
		}

		public void MouseWheelUp()
		{
		}

		public void PageDown()
		{
		}

		public void PageLeft()
		{
		}

		public void PageRight()
		{
		}

		public void PageUp()
		{
		}

		// are these documented incorrectly?
		public void SetHorizontalOffset(double offset)
		{
		}

		public void SetVerticalOffset(double offset)
		{
		}

		#endregion Methods

		#region Properties

		public bool CanScrollHorizontally
		{
			get { return _isCanScrollHorizontally; }
			set { _isCanScrollHorizontally = value; }
		}

		public bool CanScrollVertically
		{
			get { return _isCanScrollVertically; }
			set { _isCanScrollVertically = value; }
		}

		public double ExtentHeight
		{
			get { return _extentHeight; }
		}

		public double ExtentWidth
		{
			get { return _extentWidth; }
		}

		public double HorizontalOffset
		{
			get { return _horizontalOffset; }
		}

		public Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

//		public ScrollViewer ScrollOwner
//		{
//			get;
//			set;
//		}

		public double VerticalOffset
		{
			get { return _verticalOffset; }
		}

		public double ViewportHeight
		{
			get { return _viewportHeight; }
		}

		public double ViewportWidth
		{
			get { return _viewportWidth; }
		}
		
		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty OrientationProperty;

		#endregion Properties (Dependency)

		#region Fields
		
		bool						_isCanScrollHorizontally = false;
		bool						_isCanScrollVertically = false;
		double						_extentHeight = 0;
		double						_extentWidth = 0;
		double						_horizontalOffset = 0;
		Size						_size = Size.Empty;
		double						_verticalOffset = 0;
		double						_viewportHeight = 0;
		double						_viewportWidth = 0;

		#endregion Fields
	}
}
