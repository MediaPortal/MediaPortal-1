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

using MediaPortal.Animation;
using MediaPortal.Input;

namespace MediaPortal.Controls
{
	public class ContentElement : DependencyObject, IInputElement, IAnimatable
	{
		#region Constructors

		static ContentElement()
		{
			IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ContentElement), new PropertyMetadata(true));
			IsFocusedProperty = DependencyProperty.Register("IsFocused", typeof(bool), typeof(ContentElement), new PropertyMetadata(false));
		}

		public ContentElement()
		{
			// prevent never used compiler warning
			if(IsEnabledChanged != null)
				IsEnabledChanged(this, null);
		}

		#endregion Constructors

		#region Events

		public event DependencyPropertyChangedEventHandler IsEnabledChanged;

		#endregion Events

		#region Methods

		public void AddHandler(RoutedEvent routedEvent, Delegate handler)
		{
		}

		public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
		{
		}

		public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock)
		{
		}

		public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock, HandoffBehavior handoffBehavior)
		{
		}

		public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation)
		{
		}

		public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation, HandoffBehavior handoffBehavior)
		{
		}

		public object GetAnimationBaseValue(DependencyProperty dp)
		{
			throw new NotImplementedException();
		}

		public void RaiseEvent(RoutedEventArgs e)
		{
		}

		public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
		{
		}
			
		#endregion Methods

		#region Properties

		public bool HasAnimatedProperties
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty IsEnabledProperty;
		public static readonly DependencyProperty IsFocusedProperty;

		#endregion Properties (Dependency)
	}
}
