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

		public void Arrange(ILayoutComponent component, Rectangle finalRectangle)
		{
			Rectangle margins = component.Margins;

			int x = finalRectangle.X + margins.Left;
			int y = finalRectangle.Y + margins.Top;

			foreach(ILayoutComponent childComponent in component.Children)
			{
				childComponent.Arrange(new Rectangle(x, y, finalRectangle.Width - (margins.Left + margins.Width), childComponent.Size.Height));
				
				y += (childComponent.Size.Height + _spacing.Height);
			}
		}

		public void Measure(ILayoutComponent component, Size availableSize)
		{
			int w = 0;
			int h = 0;

			foreach(ILayoutComponent childComponent in component.Children)
			{
				childComponent.Measure(availableSize);

				w = Math.Max(w, childComponent.Size.Width);
				h = h + childComponent.Size.Height + _spacing.Height;
			}

			Rectangle margins = component.Margins;

			_desiredSize = new Size(w, h);
			_desiredSize.Width += margins.Left + margins.Width;
			_desiredSize.Height += margins.Top + margins.Height;
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
