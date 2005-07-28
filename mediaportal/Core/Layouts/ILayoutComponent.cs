using System;
using System.Collections;
using System.Drawing;

namespace MediaPortal.Layouts
{
	public interface ILayoutComponent
	{
		#region Methods

		void Arrange(Rectangle rect);
		Size Measure();

		#endregion Methods

		#region Properties

		Size Size
		{
			get;
		}

		Point Location
		{
			get;
		}

		Dock Dock
		{
			get;
			set;
		}

		bool Visible
		{
			get;
		}

		#endregion Properties
	}
}
