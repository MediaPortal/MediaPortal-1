using System;
using System.Collections;
using System.Drawing;

using MediaPortal.GUI.Library;

namespace MediaPortal.Layouts
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

		public GridLayout(int columns, int rows, int horizontalSpacing, int verticalSpacing)
		{
			if(rows < 0)
				throw new ArgumentOutOfRangeException("rows");

			if(columns < 0)
				throw new ArgumentOutOfRangeException("columns");

			if(columns == 0 && rows == 0)
				throw new ArgumentException("rows and columns cannot both be zero");

			_cols = columns;
			_rows = rows;
			_spacing = new Size(Math.Max(0, horizontalSpacing), Math.Max(0, verticalSpacing));
		}

		#endregion Constructors

		#region Methods

		public void Arrange(ILayoutComponent component, Rectangle finalRectangle)
		{
			Point parentLocation = finalRectangle.Location;
			Size parentSize = finalRectangle.Size;
			Rectangle parentMargins = component.Margins;

			int rows = _rows;
			int cols = _cols;

			if(rows > 0)
				cols = (component.Children.Count + rows - 1) / rows;
			else
				rows = (component.Children.Count + cols - 1) / cols;

			double w = (parentSize.Width - parentMargins.Width - (cols - 1) * _spacing.Width) / cols;
			double h = (parentSize.Height - parentMargins.Height - (rows - 1) * _spacing.Height) / rows;
			double y = parentLocation.Y + parentMargins.Y;

			for(int row = 0; row < rows; row++)
			{
				double x = parentLocation.X + parentMargins.X;

				for(int col = 0; col < cols; col++)
				{
					int index = row * cols + col;

					// urghhhhhhhhhh!!!!
					if(index < component.Children.Count)
						((ILayoutComponent)((GUIControlCollection)component.Children)[index]).Arrange(new Rectangle((int)x, (int)y, (int)w, (int)h));

					x += w + _spacing.Width;
				}

				y += h + _spacing.Height;
			}
		}

		public void Measure(ILayoutComponent component, Size availableSize)
		{
			int w = 0;
			int h = 0;

			int rows = _rows;
			int cols = _cols;

			if(rows > 0)
				cols = (component.Children.Count + rows - 1) / rows;
			else
				rows = (component.Children.Count + cols - 1) / cols;

			foreach(ILayoutComponent childComponent in component.Children)
			{
				childComponent.Measure(availableSize);

				w = Math.Max(w, childComponent.Size.Width);
				h = Math.Max(h, childComponent.Size.Height);
			}

			Rectangle margins = component.Margins;

			_desiredSize = new Size(w * cols + _spacing.Width * (cols - 1), h * rows + _spacing.Height * (rows - 1));
			_desiredSize.Width += margins.Left + margins.Width;
			_desiredSize.Height += margins.Top + margins.Height;
		}

		#endregion Methods

		#region Properties

		public int Columns
		{
			get { return _cols; }
			set { if(value != _cols) { _cols = value; } }
		}
	
		public Size Size
		{
			get { return _desiredSize; }
		}

		public int Rows
		{
			get { return _rows; }
			set { if(value != _rows) { _rows = value; } }
		}

		public Size Spacing
		{
			get { return _spacing; }
			set { if(Size.Equals(_spacing, value) == false) { _spacing = value; } }
		}

		#endregion Properties

		#region Fields

		int							_cols;
		int							_rows;
		Size						_desiredSize = Size.Empty;
		Size						_spacing = Size.Empty;

		#endregion Fields
	}
}