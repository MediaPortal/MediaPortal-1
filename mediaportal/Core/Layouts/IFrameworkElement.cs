using System;
using System.Collections;
using System.Drawing;

namespace MediaPortal.GUI.Layouts
{
	public interface IFrameworkElement
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

		Size DesiredSize
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
