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
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace System.Windows.Controls
{
	public abstract class ButtonBase : ContentControl
	{
		#region Constructors

		static ButtonBase()
		{
			ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));

			ClickModeProperty = DependencyProperty.Register("ClickMode", typeof(ClickMode), typeof(ButtonBase), new PropertyMetadata(ClickMode.OnRelease));
			CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(ButtonBase));
			CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ButtonBase), new PropertyMetadata(ClickMode.OnRelease));
			IsPressedProperty = DependencyProperty.Register("IsPressed", typeof(bool), typeof(ButtonBase), new PropertyMetadata(false));
		}

		protected ButtonBase()
		{
		}

		#endregion Constructors

		#region Events

		public event RoutedEventHandler Click
		{
			add		{ AddHandler(ClickEvent, value); } 
			remove	{ RemoveHandler(ClickEvent, value); }
		}

		#endregion Events

		#region Events (Routed)

		public static readonly RoutedEvent ClickEvent;

		#endregion Events (Routed)

		#region Methods

		protected virtual void OnClick()
		{
		}

		#endregion Methods

		#region Properties

		[BindableAttribute(true)] 
		public ClickMode ClickMode
		{
			get { return (ClickMode)GetValue(ClickModeProperty); }
			set { SetValue(ClickModeProperty, value); }
		}

		[BindableAttribute(true)] 
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value);  }
		}

		[BindableAttribute(true)] 
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		[BindableAttribute(true)] 
		public bool IsPressed
		{
			get { return (bool)GetValue(IsPressedProperty); }
			set { SetValue(IsPressedProperty, value); }
		}

		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty ClickModeProperty;
		public static readonly DependencyProperty CommandParameterProperty;
		public static readonly DependencyProperty CommandProperty;
		public static readonly DependencyProperty IsPressedProperty;

		#endregion Properties (Dependency)
	}
}
