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
			_spacing.Width = Math.Max(0, horizontalSpacing);
			_spacing.Height = Math.Max(0, verticalSpacing);
		}

		#endregion Constructors

		#region Methods

		public void Arrange(ILayoutComposite composite)
		{
			Point location = composite.Location;
			Size size = composite.Size;
			Margins margins = composite.Margins;

			int rows = _rows;
			int cols = _cols;

			if(rows > 0)
				cols = (composite.Children.Count + rows - 1) / rows;
			else
				rows = (composite.Children.Count + cols - 1) / cols;

			double w = (size.Width - margins.Width - (cols - 1) * _spacing.Width) / cols;
			double h = (size.Height - margins.Height - (rows - 1) * _spacing.Height) / rows;
			double y = location.Y + margins.Top;

			for(int row = 0; row < rows; row++)
			{
				double x = location.X + margins.Left;

				for(int col = 0; col < cols; col++)
				{
					int index = row * cols + col;

					if(index < composite.Children.Count)
					{
						// ugly
						if(composite.Children is GUIControlCollection)
						{
							ILayoutComponent component = ((GUIControlCollection)composite.Children)[index];

							component.Arrange(new Rectangle((int)x, (int)y, (int)w, (int)h));
						}
						
						if(composite.Children is ILayoutComponentCollection)
						{
							ILayoutComponent component = ((ILayoutComponentCollection)composite.Children)[index];

							component.Arrange(new Rectangle((int)x, (int)y, (int)w, (int)h));
						}
					}

					x += w + _spacing.Width;
				}

				y += h + _spacing.Height;
			}
		}

		public void Measure(ILayoutComposite composite, Size availableSize)
		{
			int w = 0;
			int h = 0;

			int rows = _rows;
			int cols = _cols;

			if(rows > 0)
				cols = (composite.Children.Count + rows - 1) / rows;
			else
				rows = (composite.Children.Count + cols - 1) / cols;

			foreach(ILayoutComponent child in composite.Children)
			{
				child.Measure();

				Size s = child.Size;

				w = Math.Max(w, s.Width);
				h = Math.Max(h, s.Height);
			}

			Margins margins = composite.Margins;

			_size = new Size(w * cols + _spacing.Width * (cols - 1), h * rows + _spacing.Height * (rows - 1));
			_size.Width += margins.Width;
			_size.Height += margins.Height;
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
			get { return _size; }
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
		Size						_size = Size.Empty;
		Size						_spacing = Size.Empty;

		#endregion Fields
	}
}