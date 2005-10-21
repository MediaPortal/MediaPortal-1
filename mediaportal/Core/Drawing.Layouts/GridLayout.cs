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
using System.Drawing;

using MediaPortal.Drawing;

namespace MediaPortal.Drawing.Layouts
{
	public class GridLayout : ILayout
	{
		#region Constructors

		public GridLayout() : this(1, 0, 0, 0)
		{
		}

		public GridLayout(int columns) : this(columns, 0, 0, 0)
		{
		}

		public GridLayout(int columns, int rows) : this(columns, rows, 0, 0)
		{
		}

		public GridLayout(int columns, int rows, double spacing) : this(columns, rows, spacing, spacing)
		{
		}

		public GridLayout(int columns, int rows, double horizontalSpacing, double verticalSpacing)  : this(columns, rows, horizontalSpacing, verticalSpacing, Orientation.Horizontal)
		{
		}

		public GridLayout(int columns, int rows, double horizontalSpacing, double verticalSpacing, Orientation orientation)
		{
			if(rows < 0)
				throw new ArgumentOutOfRangeException("rows");

			if(columns < 0)
				throw new ArgumentOutOfRangeException("columns");

			if(columns == 0 && rows == 0)
				throw new ArgumentException("rows and columns cannot both be zero");

			_cols = columns;
			_rows = rows;
			_spacing.Width = Math.Max(0, horizontalSpacing);
			_spacing.Height = Math.Max(0, verticalSpacing);
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
			Point location = composite.Location;
			Size size = composite.Size;
			Thickness t = composite.Margin;

			int rows = _rows;
			int cols = _cols;

			if(rows > 0)
				cols = (composite.Children.Count + rows - 1) / rows;
			else
				rows = (composite.Children.Count + cols - 1) / cols;

			double w = (size.Width - t.Width - (cols - 1) * _spacing.Width) / cols;
			double h = (size.Height - t.Height - (rows - 1) * _spacing.Height) / rows;
			double y = composite.Location.Y + t.Top;

			for(int row = 0; row < rows; row++)
			{
				double x = composite.Location.X + t.Left;

				for(int col = 0; col < cols; col++)
				{
					int index = _orientation == Orientation.Vertical ? col * rows + row : row * cols + col;

					if(index < composite.Children.Count)
					{
						ILayoutComponent component = null;

						if(composite.Children is IList)
							component = (ILayoutComponent)((IList)composite.Children)[index];

						if(composite.Children is ILayoutComponentCollection)
							component = ((ILayoutComponentCollection)composite.Children)[index];

						if(component.IsVisible == false)
							continue;

						ApplyAlignment(component, t, x, y, w, h); 
					}

					x += w + _spacing.Width;
				}

				y += h + _spacing.Height;
			}
		}

		public Size Measure(ILayoutComposite composite, Size availableSize)
		{
			double w = 0;
			double h = 0;

			int rows = _rows;
			int cols = _cols;

			if(rows > 0)
				cols = (composite.Children.Count + rows - 1) / rows;
			else
				rows = (composite.Children.Count + cols - 1) / cols;

			foreach(ILayoutComponent component in composite.Children)
			{
				if(component.IsVisible == false)
					continue;

				Size s = component.Measure();

				w = Math.Max(w, s.Width);
				h = Math.Max(h, s.Height);
			}

			Thickness t = composite.Margin;

			_size.Width = (w * cols + _spacing.Width * (cols - 1)) + t.Width;
			_size.Height = (h * rows + _spacing.Height * (rows - 1)) + t.Height;

			return _size;
		}

		#endregion Methods

		#region Properties

		public int Columns
		{
			get { return _cols; }
			set { if(value != _cols) { _cols = value; } }
		}

		public Orientation Orientation
		{
			get { return _orientation; }
			set { _orientation = value; }
		}
	
		public int Rows
		{
			get { return _rows; }
			set { if(value != _rows) { _rows = value; } }
		}

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

		int							_cols;
		Orientation					_orientation;
		int							_rows;
		Size						_size = Size.Empty;
		Size						_spacing = Size.Empty;

		#endregion Fields
	}
}