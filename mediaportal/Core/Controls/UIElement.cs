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
using System.ComponentModel;
using System.Windows;

using MediaPortal.Animation;
using MediaPortal.Drawing;
using MediaPortal.Drawing.Layouts;
using MediaPortal.Input;

namespace MediaPortal.Controls
{
	public class UIElement : Visual, IInputElement, IAnimatable
	{
		#region Constructors

		static UIElement()
		{
			IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(UIElement), new PropertyMetadata(true));
			IsFocusedProperty = DependencyProperty.Register("IsFocused", typeof(bool), typeof(UIElement), new PropertyMetadata(false));
			IsKeyboardFocusedProperty = DependencyProperty.Register("IsKeyboardFocused", typeof(bool), typeof(UIElement), new PropertyMetadata(false));
			IsKeyboardFocusWithinProperty = DependencyProperty.Register("IsKeyboardFocusWithinProperty", typeof(bool), typeof(UIElement), new PropertyMetadata(false));
			IsVisibleProperty = DependencyProperty.Register("IsVisible", typeof(bool), typeof(UIElement), new PropertyMetadata(true));
			OpacityMaskProperty = DependencyProperty.Register("OpacityMask", typeof(Brush), typeof(UIElement));
			OpacityProperty = DependencyProperty.Register("Opacity", typeof(double), typeof(UIElement), new PropertyMetadata(1.0));
			VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(UIElement), new PropertyMetadata(Visibility.Visible));

			GotKeyboardFocusEvent = EventManager.RegisterRoutedEvent("GotKeyboardFocus", RoutingStrategy.Direct, typeof(KeyboardFocusChangedEventHandler), typeof(UIElement));
			LostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("LostKeyboardFocus", RoutingStrategy.Direct, typeof(KeyboardFocusChangedEventHandler), typeof(UIElement));
			IsEnabledChangedEvent = EventManager.RegisterRoutedEvent("IsEnabledChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(UIElement));
			IsKeyboardFocusedChangedEvent = EventManager.RegisterRoutedEvent("IsEnabledChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(UIElement));
			IsKeyboardFocusWithinChangedEvent = EventManager.RegisterRoutedEvent("IsEnabledChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(UIElement));
			IsVisibleChangedEvent = EventManager.RegisterRoutedEvent("IsEnabledChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(UIElement));
			PreviewLostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("PreviewLostKeyboardFocus", RoutingStrategy.Direct, typeof(KeyboardFocusChangedEventHandler), typeof(UIElement));
			LostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("PreviewGotKeyboardFocus", RoutingStrategy.Direct, typeof(KeyboardFocusChangedEventHandler), typeof(UIElement));

//			EventManager.RegisterClassHandler(typeof(UIElement), Keyboard.KeyDownEvent, new KeyEventHandler(UIElement.OnKeyDownThunk));
//			EventManager.RegisterClassHandler(
		}

		public UIElement()
		{
		}

		#endregion Constructors

		#region Events (Routed)

		public static readonly RoutedEvent GotKeyboardFocusEvent;
		public static readonly RoutedEvent LostKeyboardFocusEvent;
		public static readonly RoutedEvent IsEnabledChangedEvent;
		public static readonly RoutedEvent IsKeyboardFocusedChangedEvent;
		public static readonly RoutedEvent IsKeyboardFocusWithinChangedEvent;
		public static readonly RoutedEvent IsVisibleChangedEvent;
		public static readonly RoutedEvent PreviewLostKeyboardFocusEvent;
		public static readonly RoutedEvent PreviewGotKeyboardFocusEvent;

		#endregion Events (Routed)

		#region Methods

		public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock)
		{
			ApplyAnimationClock(dp, clock, HandoffBehavior.SnapshotAndReplace);
		}

		public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock, HandoffBehavior handoffBehavior)
		{
			throw new NotImplementedException();
		}
	
		public object GetAnimationBaseValue(DependencyProperty dp)
		{
			return GetValue(dp);
		}

		public void AddHandler(RoutedEvent routedEvent, Delegate handler)
		{
			AddHandler(routedEvent, handler, true);
		}

		public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
		{
			throw new NotImplementedException();
		}

		public void Arrange(Rect finalRect)
		{
			ArrangeCore(finalRect);
		}

		protected virtual void ArrangeCore(Rect finalRect)
		{
			// no default implementation
		}
			
		public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation)
		{
			BeginAnimation(dp, animation, HandoffBehavior.SnapshotAndReplace);
		}

		public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation, HandoffBehavior handoffBehavior)
		{
			throw new NotImplementedException();
		}

		public void Measure(Size availableSize)
		{
			MeasureCore(availableSize);
		}

		protected virtual Size MeasureCore(Size availableSize)
		{
			// no default implementation
			return Size.Empty;
		}
			
		public bool Focus()
		{
/*			if(IsFocused)
				return true;

			if(Focusable && IsEnabled)
				return false;

			if(!IsKeyboardFocused)
			{
			}

			if(!IsKeyboardFocusWithin)
			{
				IsKeyboardFocusWithin = true;

				new RoutedEventArgs(UIElement.IsKeyboardFocusedChanged);
				RaiseEvent();
				
			}

			IsKeyboardFocused = true;
			IsKeyboardFocusWithin = true;

			// If the related properties were not already true, then calling this method may also 
			// raise these events in the order given:
			// PreviewLostKeyboardFocus
			// PreviewGotKeyboardFocus (source is the new focus target)
			// IsKeyboardFocusedChanged
			// IsKeyboardFocusWithinChanged
			// LostKeyboardFocus
			// GotKeyboardFocus (source is the new focus target).
*/

			return true;
		}			
			
		protected override void OnPropertyInvalidated(DependencyProperty dp, PropertyMetadata metadata)
		{
			// assume that this is used to set the dirty flag on cached properties?
//			MediaPortal.GUI.Library.Log.Write("UIElement.OnPropertyInvalidated: {0}", dp.Name);
		}
			
		protected internal virtual void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
		{
		}
			
		protected virtual void OnRender(DrawingContext dc)
		{
			// no default implementation
		}

		public void RaiseEvent(RoutedEventArgs e)
		{
		}

		public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
		{
			if(routedEvent == null)
				throw new ArgumentNullException("routedEvent");

			if(handler == null)
				throw new ArgumentNullException("handler");
		}

		#endregion Methods

		#region Properties

		public Size DesiredSize
		{
			get { return _desiredSize; }
		}

		public bool HasAnimatedProperties
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsArrangeValid
		{
			get { return _isArrangeValid; }
		}

		[DependencyProperty(DefaultValue=true)]
		public bool IsEnabled
		{
			get { return IsEnabledCore; }
			set { SetValue(IsEnabledProperty, value); }
		}

		protected virtual bool IsEnabledCore
		{
			get { return (bool)GetValue(IsEnabledProperty); }
		}

		[DependencyProperty(DefaultValue=false)]
		public bool IsFocused
		{
			get { return (bool)GetValue(IsFocusedProperty); }
		}

		public bool IsMeasureValid
		{
			get { return _isMeasureValid; }
		}

		[MediaPortal.GUI.Library.XMLSkinElement("visible")]
		public bool IsVisible
		{
			// TODO: there should be no set accessor
			get { return (Visibility)GetValue(VisibilityProperty) == Visibility.Visible; }
			set { SetValue(VisibilityProperty, value ? Visibility.Visible : Visibility.Hidden); }
		}

		[DependencyProperty(DefaultValue=1.0)]
		public virtual double Opacity
		{
			get { return (double)GetValue(OpacityProperty); }
			set { SetValue(OpacityProperty, value); }
		}

		[DependencyProperty]
		public Brush OpacityMask
		{
			get { return (Brush)GetValue(OpacityMaskProperty); }
			set { SetValue(OpacityMaskProperty, value); }
		}

		public Size RenderSize
		{
			get { return _renderSize; }
			set { _renderSize = value; }
		}

		[DependencyProperty(DefaultValue=Visibility.Visible)]
		public Visibility Visibility
		{
			get { return (Visibility)GetValue(VisibilityProperty); }
			set { SetValue(VisibilityProperty, value); }
		}

		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty IsEnabledProperty;
		public static readonly DependencyProperty IsFocusedProperty;
		public static readonly DependencyProperty IsKeyboardFocusedProperty;
		public static readonly DependencyProperty IsKeyboardFocusWithinProperty;
		public static readonly DependencyProperty IsVisibleProperty;
		public static readonly DependencyProperty OpacityProperty;
		public static readonly DependencyProperty OpacityMaskProperty;
		public static readonly DependencyProperty VisibilityProperty;

		#endregion Properties (Dependency)

		#region Fields

		Size						_desiredSize = Size.Empty;
		bool						_isArrangeValid = false;
		bool						_isMeasureValid = false;
		Size						_renderSize = Size.Empty;

		#endregion Fields
	}
}
