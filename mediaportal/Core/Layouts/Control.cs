using System;
using System.Drawing;

namespace MediaPortal.Layouts
{
	public abstract class Control : ILayoutComponent
	{
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

		public abstract Size Size
		{
			get;
			set;
		}

		public abstract Point Location
		{
			get;
			set;
		}

		#endregion Properties

		#region Fields

//		protected Size				_size = Size.Empty;
//		protected Point				_location = Point.Empty;

		#endregion Fields
	}
}
