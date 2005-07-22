using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace MediaPortal.Layouts
{
	public interface ILayoutComposite : ILayoutComponent, ISupportInitialize
	{
		#region Properties

		ICollection Children
		{
			get;
		}

		ILayout Layout
		{
			get;
			set;
		}

		Point Location
		{
			get;
		}

		Margins Margins
		{
			get;
		}

		Size Size
		{
			get;
		}

		#endregion Properties
	}
}
