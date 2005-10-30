#region Copyright (C) 2005 Media Portal

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

#endregion

using System;
using System.Windows;

namespace MediaPortal.Controls
{
	public abstract class MenuBase : ItemsControl
	{
		#region Constructors

		protected MenuBase()
		{
		}

		#endregion Constructors

		#region Methods

//		protected override DependencyObject GetContainerForItemOverride(object item)
//		{
//			throw new NotImplementedException();
//		}

		// HandleMouseButton

//		protected override bool IsItemItsOwnContainerOverride(object item)
//		{
//			// Returns true if the item is its own ItemContainer;
//			throw new NotImplementedException();
//		}

		protected override void OnInitialized(EventArgs e)
		{
		}
		
//		protected internal override void OnIsFocusWithinChanged(DependencyPropertyChangedEventArgs e)
//		{
//		}

//		protected override void OnKeyDown(KeyEventArgs e)
//		{
//		}

//		protected override void OnMouseLeave(MouseEventArgs e)
//		{
//		}

		#endregion Methods
	}
}
