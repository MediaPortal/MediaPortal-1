using System;
using System.ComponentModel;
using System.Collections;
using System.Drawing;

namespace MediaPortal.GUI.Layouts
{
	[TypeConverter(typeof(LayoutConverter))]
	public interface ILayout
	{
		#region Methods

		void Measure(IFrameworkElement element, Size availableSize);
		void Arrange(IFrameworkElement element, Size availableSize);

		#endregion Methods
	}
}
