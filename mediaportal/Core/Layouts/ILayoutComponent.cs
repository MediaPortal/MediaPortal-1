using System;
using System.Collections;
using System.Drawing;

namespace MediaPortal.Layouts
{
	public interface ILayoutComponent
	{
		#region Methods

		void Arrange(Rectangle finalRectangle);
		void Measure(Size availableSize);

		#endregion Methods

		#region Properties

		ICollection Children
		{
			get;
		}

//		Constraints Constraints
//		{
//			get;
//		}

		Rectangle Margins
		{
			get;
		}

		Size Size
		{
			get;
		}

		Point Location
		{
			get;
		}

		#endregion Properties
	}
}
