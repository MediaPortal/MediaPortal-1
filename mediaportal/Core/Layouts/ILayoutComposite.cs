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

		Margins Margins
		{
			get;
		}

		#endregion Properties
	}
}
