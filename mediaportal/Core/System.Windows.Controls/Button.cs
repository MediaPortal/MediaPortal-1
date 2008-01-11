#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
using System.Windows.Automation;

using MediaPortal.Drawing;

namespace System.Windows.Controls
{
	public class Button : ButtonBase, IInvokeProvider
	{
		#region Constructors
		
		static Button()
		{
			IsCancelProperty = DependencyProperty.Register("IsCancel", typeof(bool), typeof(Button), new PropertyMetadata(false));
			IsDefaultedProperty = DependencyProperty.Register("IsDefaulted", typeof(bool), typeof(Button), new PropertyMetadata(false));
			IsDefaultProperty = DependencyProperty.Register("IsDefault", typeof(bool), typeof(Button), new PropertyMetadata(false));
		}

		public Button()
		{
		}

		#endregion Constructors

		#region Methods

		void IInvokeProvider.Invoke()
		{
			throw new NotImplementedException();
		}

		protected override void OnClick()
		{
			throw new NotImplementedException();
		}

		#endregion Methods

		#region Properties

		public bool IsCancel
		{
			get { return (bool)GetValue(IsCancelProperty); }
			set { SetValue(IsCancelProperty, value); }
		}

		public bool IsDefault
		{
			get { return (bool)GetValue(IsDefaultProperty); }
			set { SetValue(IsDefaultProperty, value); }
		}

		public bool IsDefaulted
		{
			get { return (bool)GetValue(IsDefaultedProperty); }
			set { SetValue(IsDefaultedProperty, value); }
		}
		
		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty IsCancelProperty;
		public static readonly DependencyProperty IsDefaultedProperty;
		public static readonly DependencyProperty IsDefaultProperty;

		#endregion Properties (Dependency)
	}
}
