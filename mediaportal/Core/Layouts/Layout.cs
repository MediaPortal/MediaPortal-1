using System;
using System.ComponentModel;
using System.Collections;
using System.Drawing;

namespace MediaPortal.Layouts
{
	[TypeConverter(typeof(LayoutConverter))]
	public interface ILayout
	{
		#region Methods

		void Arrange(ILayoutComponent component, Size availableSize);
		void Measure(ILayoutComponent component, Size availableSize);

		#endregion Methods
	}
}
