using System;
using System.ComponentModel;
using System.Drawing;

namespace MediaPortal.Layouts
{
	[TypeConverter(typeof(LayoutConverter))]
	public interface ILayout
	{
		#region Methods

		void Arrange(ILayoutComposite composite);
		Size Measure(ILayoutComposite composite, Size availableSize);

		#endregion Methods

		#region Properties

		Size Size
		{
			get;
		}

		#endregion Properties
	}
}
