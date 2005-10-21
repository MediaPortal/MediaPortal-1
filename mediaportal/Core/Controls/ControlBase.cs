/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;

using MediaPortal.Drawing;
using MediaPortal.Drawing.Layouts;

namespace MediaPortal.Controls
{
	public abstract class ControlBase : ILayoutComponent
	{
		#region Methods

		void ILayoutComponent.Arrange(Rect rect)
		{
			ArrangeCore(rect);
		}

		protected virtual void ArrangeCore(Rect rect)
		{
		}

		Size ILayoutComponent.Measure()
		{
			return MeasureCore();
		}

		protected virtual Size MeasureCore()
		{
			return Size.Empty;
		}

		#endregion Methods

		#region Properties

		public abstract HorizontalAlignment HorizontalAlignment
		{
			get;
			set;
		}

		public abstract bool IsVisible
		{
			get;
			set;
		}

		public abstract Thickness Margin
		{
			get;
			set;
		}

		public abstract Point Location
		{
			get;
			set;
		}

		public abstract Size Size
		{
			get;
			set;
		}

		public abstract VerticalAlignment VerticalAlignment
		{
			get;
			set;
		}

		#endregion Properties
	}
}
