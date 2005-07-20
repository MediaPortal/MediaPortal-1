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

		public void Arrange(ILayoutComponent component, Size finalSize)
		{
			Rectangle margins = component.Margins;

			int x = component.Location.X;
			int y = component.Location.Y + margins.Top;

			foreach(ILayoutComponent childComponent in component.Children)
			{
				childComponent.Arrange(new Rectangle(component.Location.X + margins.Left, y, finalSize.Width, childComponent.Size.Height));
				
				y += (childComponent.Size.Height + _spacing.Height);
			}
		}

		public void Measure(ILayoutComponent component, Size availableSize)
		{
			Rectangle margins = component.Margins;

			int w = 0;
			int h = 0;

			foreach(ILayoutComponent childComponent in component.Children)
			{
				childComponent.Measure(availableSize);

				w = Math.Max(w, childComponent.Size.Width);
				h = h + childComponent.Size.Height + h == 0 ? 0 : _spacing.Height;
			}

			_desiredSize = new Size(w, h);
			_desiredSize.Width += margins.Left + margins.Right;
			_desiredSize.Height += margins.Top + margins.Bottom;
		}

		#endregion Methods

		#region Properties

		public Size DesiredSize
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

		Point						_location = Point.Empty;
		Size						_spacing = Size.Empty;
		Size						_desiredSize = Size.Empty;

		#endregion Fields
	}
}
