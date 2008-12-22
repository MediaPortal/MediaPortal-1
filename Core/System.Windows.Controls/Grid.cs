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
using System.Collections;
using System.Windows;
using System.Windows.Serialization;

using MediaPortal.Drawing;

namespace System.Windows.Controls
{
	public class Grid : Panel, IAddChild
	{
		#region Constructors

		static Grid()
		{
			ColumnProperty = DependencyProperty.RegisterAttached("Column", typeof(int), typeof(Grid));
			ColumnSpanProperty = DependencyProperty.RegisterAttached("ColumnSpan", typeof(int), typeof(Grid), new PropertyMetadata((int)1));
			IsSharedSizeScopeProperty = DependencyProperty.RegisterAttached("IsSharedSizeScope", typeof(bool), typeof(Grid), new PropertyMetadata(false));
			RowProperty = DependencyProperty.RegisterAttached("Row", typeof(int), typeof(Grid));
			RowSpanProperty = DependencyProperty.RegisterAttached("RowSpan", typeof(int), typeof(Grid), new PropertyMetadata((int)1));
			ShowGridLinesProperty = DependencyProperty.Register("ShowGridLines", typeof(bool), typeof(Grid), new PropertyMetadata(false));
		}

		public Grid()
		{
		}

		#endregion Constructors

		#region Methods

		void IAddChild.AddChild(object child)
		{
			if(child is ColumnDefinition)
			{
				ColumnDefinitions.Add((ColumnDefinition)child);
			}
			else if(child is RowDefinition)
			{
				RowDefinitions.Add((RowDefinition)child);
			}
			else if(child is UIElement)
			{
				Children.Add((UIElement)child);
			}
			else
			{
				throw new ArgumentException("");
			}
		}

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
			int rows = _rowDefinitions.Count;
			int cols = _colDefinitions.Count;

			if(rows > 0)
				cols = (Children.Count + rows - 1) / rows;
			else
				rows = (Children.Count + cols - 1) / cols;

			double w = (Width - Margin.Width - (cols - 1) * _spacing.Width) / cols;
			double h = (Height - Margin.Height - (rows - 1) * _spacing.Height) / rows;
			double y = Location.Y + Margin.Top;

			for(int row = 0; row < rows; row++)
			{
				double x = Location.X + Margin.Left;

				for(int col = 0; col < cols; col++)
				{
					int index = _orientation == Orientation.Vertical ? col * rows + row : row * cols + col;

					if(index < Children.Count)
					{
						FrameworkElement element = (FrameworkElement)Children[index];

						if(element.Visibility == System.Windows.Visibility.Collapsed)
							continue;

						ApplyAlignment(element, x, y, w, h); 
					}

					x += w + _spacing.Width;
				}

				y += h + _spacing.Height;
			}

			return Size.Empty;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			double w = 0;
			double h = 0;

			int rows = RowDefinitions.Count;
			int cols = ColumnDefinitions.Count;

			if(rows > 0)
				cols = (Children.Count + rows - 1) / rows;
			else
				rows = (Children.Count + cols - 1) / cols;

			foreach(FrameworkElement element in Children)
			{
				if(element.Visibility == System.Windows.Visibility.Collapsed)
					continue;

				element.Measure(availableSize);

				w = Math.Max(w, element.Width);
				h = Math.Max(h, element.Height);
			}

			w = (w * cols + _spacing.Width * (cols - 1)) + Margin.Width;
			h = (h * rows + _spacing.Height * (rows - 1)) + Margin.Height;

			return new Size(w, h);
		}

		#endregion Methods

		#region Properties

		public ColumnDefinitionCollection ColumnDefinitions
		{
			get { return _colDefinitions; }
		}

		protected internal override IEnumerator LogicalChildren
		{
			get { throw new NotImplementedException(); }
		}

		public RowDefinitionCollection RowDefinitions
		{
			get { return _rowDefinitions; }
		}

		public bool ShowGridLines
		{
			get { return (bool)GetValue(ShowGridLinesProperty); }
			set { SetValue(ShowGridLinesProperty, value); }
		}

		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty ColumnProperty;
		public static readonly DependencyProperty ColumnSpanProperty;
		public static readonly DependencyProperty IsSharedSizeScopeProperty;
		public static readonly DependencyProperty RowProperty;
		public static readonly DependencyProperty RowSpanProperty;
		public static readonly DependencyProperty ShowGridLinesProperty;

		#endregion Properties (Dependency)

		#region Fields

		ColumnDefinitionCollection	_colDefinitions = new ColumnDefinitionCollection();
		RowDefinitionCollection		_rowDefinitions = new RowDefinitionCollection();

		#endregion Fields

		// temporary
		Size _spacing = Size.Empty;
		Orientation _orientation = Orientation.Vertical;
	}
}
