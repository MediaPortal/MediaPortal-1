using System;
using System.Drawing;

namespace MediaPortal.Layouts
{
	public class Control : ILayoutComponent
	{
		#region Constructors

		public Control()
		{
		}

		#endregion Constructors

		#region Methods

		void ILayoutComponent.Arrange(Rectangle rect)
		{
			ArrangeCore(rect);
		}

		protected virtual void ArrangeCore(Rectangle rect)
		{
		}

		void ILayoutComponent.Measure()
		{
			MeasureCore();
		}

		protected virtual void MeasureCore()
		{
		}

		#endregion Methods

		#region Properties

		public Size Size
		{
			get { return _size; }
			set { _size = value; }
		}

		public Point Location
		{
			get { return _location; }
			set { _location = value; }
		}

		#endregion Properties

		#region Fields

		protected Size				_size = Size.Empty;
		protected Point				_location = Point.Empty;

		#endregion Fields
	}
}
