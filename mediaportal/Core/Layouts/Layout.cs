using System;
using System.ComponentModel;
using System.Drawing;

namespace MediaPortal.Layouts
{
	[TypeConverter(typeof(LayoutConverter))]
	public interface ILayout
	{
		#region Methods

		void Arrange(ILayoutComponent element, Rectangle finalRectangle);
		void Measure(ILayoutComponent element, Size availableSize);

		#endregion Methods

		#region Properties

		Size Size
		{
			get;
		}

		#endregion Properties
	}
}
