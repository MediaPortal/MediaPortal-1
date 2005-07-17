using System;
using System.Collections;
using System.Drawing;

namespace MediaPortal.GUI.Layouts
{
	public interface IFrameworkElement
	{
		#region Methods

		void Measure(Size availableSize);
		void Arrange(Rectangle finalRectangle);

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

		#endregion Properties
	}
}
