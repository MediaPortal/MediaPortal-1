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
using System.Drawing;

namespace MediaPortal.Layouts
{
	public abstract class Control : ILayoutComponent
	{
		#region Methods

		void ILayoutComponent.Arrange(Rectangle rect)
		{
			ArrangeCore(rect);
		}

		protected virtual void ArrangeCore(Rectangle rect)
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

		Dock ILayoutComponent.Dock
		{
			get { return _dock; }
			set { _dock = value; }
		}

		public abstract Size Size
		{
			get;
			set;
		}

		public abstract Point Location
		{
			get;
			set;
		}

		public abstract bool Visible
		{
			get;
			set;
		}

		#endregion Properties

		#region Fields

		Dock						_dock = Dock.None;

//		protected Size				_size = Size.Empty;
//		protected Point				_location = Point.Empty;

		#endregion Fields
	}
}
