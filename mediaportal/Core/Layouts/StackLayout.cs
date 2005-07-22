using System;
using System.Collections;
using System.Drawing;

namespace MediaPortal.Layouts
{
	public class StackLayout : ILayout
	{
		#region Constructors

		public StackLayout()
		{
			_spacing = new Size(0, 0);
		}

		public StackLayout(int gap)
		{
			_spacing = new Size(0, gap);
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
			int w = composite.Size.Width - m.Right;
			int h = 0;

			foreach(ILayoutComponent child in composite.Children)
			{
				child.Arrange(new Rectangle(x, y, w, h = child.Size.Height));
				y += h + _spacing.Height;
			}
		}

		public void Measure(ILayoutComposite composite, Size availableSize)
		{
			int w = 0;
			int h = 0;

			foreach(ILayoutComponent child in composite.Children)
			{
				child.Measure();

				w = Math.Max(w, child.Size.Width);
				h = h + child.Size.Height + _spacing.Height;
			}

			Margins margins = composite.Margins;

			_desiredSize = new Size(w, h);
			_desiredSize.Width += margins.Width;
			_desiredSize.Height += margins.Height;
		}

		#endregion Methods

		#region Properties

		public Size Size
		{
			get { return _desiredSize; }
		}

		public Size Spacing
		{
			get { return _spacing; }
			set { if(Size.Equals(_spacing, value) == false) { _spacing = value; } }
		}

		#endregion Properties

		#region Fields

		Size						_spacing = Size.Empty;
		Size						_desiredSize = Size.Empty;

		#endregion Fields
	}
}
