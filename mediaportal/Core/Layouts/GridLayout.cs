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

		public void Measure(ILayoutComponent component, Size availableSize)
		{
			int w = 0;
			int h = 0;

			foreach(ILayoutComponent childComponent in component.Children)
			{
				childComponent.Measure(availableSize);

				w = Math.Max(w, childComponent.Size.Width);
				h = Math.Max(h, childComponent.Size.Height);
			}

			Rectangle margins = component.Margins;

			_desiredSize = new Size(w, h);
			_desiredSize.Width += margins.Left + margins.Right;
			_desiredSize.Height += margins.Top + margins.Bottom;
		}

		public void Arrange(ILayoutComponent component, Size finalSize)
		{
			Point parentLocation = component.Location;
			Size parentSize = component.Size;
			Rectangle parentMargins = component.Margins;

			int rows = _rows;
			int cols = _cols;

			if(rows > 0)
				cols = (component.Children.Count + rows - 1) / rows;
			else
				rows = (component.Children.Count + cols - 1) / cols;

			double w = (parentSize.Width - (cols - 1) * _spacing.Width) / cols;
			double h = (parentSize.Height - (rows - 1) * _spacing.Height) / rows;
			double y = parentLocation.Y + parentMargins.Y;

			for(int r = 0; r < rows; r++)
			{
				double x = parentLocation.X + parentMargins.X;

				for(int c = 0; c < cols; c++)
				{
					int index = r * cols + c;

					// urghhhhhhhhhh!!!!
					if(index < component.Children.Count)
						((ILayoutComponent)((GUIControlCollection)component.Children)[index]).Arrange(new Rectangle((int)x, (int)y, (int)w, (int)h));

					x += w + _spacing.Width;
				}

				y += h + _spacing.Height;
			}
		}

		#endregion Methods

		#region Properties

		public int Columns
		{
			get { return _cols; }
			set { if(value != _cols) { _cols = value; } }
		}
	
		public Size DesiredSize
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