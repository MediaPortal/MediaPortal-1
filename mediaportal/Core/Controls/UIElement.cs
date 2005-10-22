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
using MediaPortal.GUI.Library;

namespace MediaPortal.Controls
{
	public class UIElement
	{
		#region Constructors

		public UIElement()
		{
		}

		#endregion Constructors

		#region Methods

		public void AddHandler(RoutedEvent routedEvent, Delegate handler)
		{
		}

		public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
		{
		}

		public void Arrange(Rect finalRect)
		{
			ArrangeCore(finalRect);
		}

		protected virtual void ArrangeCore(Rect finalRect)
		{
		}
			
		public void BeginAnimation(DependencyBinding dp, AnimationTimeline animation)
		{
		}

		public void BeginAnimation(DependencyBinding dp, AnimationTimeline animation, HandoffBehavior handoffBehavior)
		{
		}

		public void Measure(Size availableSize)
		{
			MeasureCore(availableSize);
		}

		protected virtual Size MeasureCore(Size availableSize)
		{
			return Size.Empty;
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

		public bool IsArrangeValid
		{
			get { return _isArrangeValid; }
		}

		public bool IsMeasureValid
		{
			get { return _isMeasureValid; }
		}

		[XMLSkinElement("visible")]
		public bool IsVisible
		{
			get { return _visibility == Visibility.Visible; }

			// TODO: there should be no set accessor
			set { _visibility = value ? Visibility.Visible : Visibility.Hidden; }
		}

		public virtual double Opacity
		{
			get { return _opacity; }
			set { _opacity = value; }
		}

		public Brush OpacityMask
		{
			get { return _opacityMask; }
			set { _opacityMask = value; }
		}

		public Size RenderSize
		{
			get { return _renderSize; }
			set { _renderSize = value; }
		}

		public Visibility Visibility
		{
			get { return _visibility; }
			set { _visibility = value; }
		}

		#endregion Properties

		#region Fields

		bool						_isArrangeValid = false;
		bool						_isMeasureValid = false;
		double						_opacity = 1;
		Brush						_opacityMask;
		Size						_renderSize = Size.Empty;
		Visibility					_visibility = Visibility.Visible;

		#endregion Fields
	}
}
