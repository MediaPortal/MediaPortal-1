using System;
using System.Collections;
using System.Drawing;

namespace MediaPortal.GUI.Layouts
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

		public void Measure(IFrameworkElement element, Size availableSize)
		{
			int w = 0;
			int h = 0;

			foreach(IFrameworkElement childElement in element.Children)
			{
				childElement.Measure(availableSize);

				w = Math.Max(w, childElement.DesiredSize.Width);
				h = h + childElement.DesiredSize.Height + h == 0 ? 0 : _spacing.Height;
			}

			_desiredSize = new Size(w, h);
		}

		public void Arrange(IFrameworkElement element, Size finalSize)
		{
			int y = 0;

			foreach(IFrameworkElement childElement in element.Children)
			{
				childElement.Arrange(new Rectangle(0, y, finalSize.Width, childElement.DesiredSize.Height));
				
				y += (childElement.DesiredSize.Height + _spacing.Height);
			}
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
