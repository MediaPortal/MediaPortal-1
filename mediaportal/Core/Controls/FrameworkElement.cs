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
using System.Collections;
using System.Windows;

using MediaPortal.Animation;
using MediaPortal.Drawing;
using MediaPortal.Drawing.Layouts;

namespace MediaPortal.Controls
{
	public class FrameworkElement : UIElement, IResourceHost, ISupportInitialize
	{
		#region Constructors

		static FrameworkElement()
		{
			ActualHeightProperty = DependencyProperty.Register("ActualHeight", typeof(double), typeof(FrameworkElement), new PropertyMetadata(0.0, new PropertyInvalidatedCallback(ActualHeightPropertyInvalidated)));
			ActualWidthProperty = DependencyProperty.Register("ActualWidth", typeof(double), typeof(FrameworkElement), new PropertyMetadata(0.0, new PropertyInvalidatedCallback(ActualWidthPropertyInvalidated)));
			FlowDirectionProperty = DependencyProperty.Register("FlowDirection", typeof(FlowDirection), typeof(FrameworkElement), new PropertyMetadata(FlowDirection.LeftToRight));
			FocusableProperty = DependencyProperty.Register("Focusable", typeof(bool), typeof(FrameworkElement), new PropertyMetadata(true));
			HeightProperty = DependencyProperty.Register("Height", typeof(double), typeof(FrameworkElement), new PropertyMetadata(0.0));
			HorizontalAlignmentProperty = DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(FrameworkElement), new PropertyMetadata(HorizontalAlignment.Stretch));
			MarginProperty = DependencyProperty.Register("Margin", typeof(Thickness), typeof(FrameworkElement), new PropertyMetadata(Thickness.Empty));
			NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(FrameworkElement), new PropertyMetadata(string.Empty));
			StyleProperty = DependencyProperty.Register("Style", typeof(Style), typeof(FrameworkElement));
			VerticalAlignmentProperty = DependencyProperty.Register("VerticalAlignment", typeof(VerticalAlignment), typeof(FrameworkElement), new PropertyMetadata(VerticalAlignment.Stretch));
			WidthProperty = DependencyProperty.Register("Width", typeof(double), typeof(FrameworkElement), new PropertyMetadata(0.0));

			LoadedEvent = EventManager.RegisterRoutedEvent("Loaded", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(FrameworkElement));
			SizeChangedEvent = EventManager.RegisterRoutedEvent("SizeChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(FrameworkElement));
		}	
			
		public FrameworkElement()
		{
		}

		#endregion Constructors

		#region Events

		public event EventHandler Initialized;

		public event RoutedEventHandler Loaded
		{
			add		{ AddHandler(LoadedEvent, value); } 
			remove	{ RemoveHandler(LoadedEvent, value); }
		}

		public event RoutedEventHandler SizeChanged
		{
			add		{ AddHandler(SizeChangedEvent, value); } 
			remove	{ RemoveHandler(SizeChangedEvent, value); }
		}

		public event EventHandler Unloaded;

		#endregion Events

		#region Events (Routed)

		public static readonly RoutedEvent LoadedEvent;
		public static readonly RoutedEvent SizeChangedEvent;

		#endregion Events (Routed)

		#region Methods

		private static void ActualHeightPropertyInvalidated(DependencyObject d)
		{
			FrameworkElement element = (FrameworkElement)d;

			element.RaiseEvent(new RoutedEventArgs(SizeChangedEvent, d));
		}

		private static void ActualWidthPropertyInvalidated(DependencyObject d)
		{
			FrameworkElement element = (FrameworkElement)d;

			element.RaiseEvent(new RoutedEventArgs(SizeChangedEvent, d));
		}

		protected override sealed void ArrangeCore(Rect finalRect)
		{
			ArrangeOverride(finalRect);
		}

		// TODO: finalRect is wrong, this should be using Size finalSize
		protected virtual Size ArrangeOverride(Rect finalRect)
		{
			_location = finalRect.Location;

			SetValue(WidthProperty, finalRect.Width);
			SetValue(HeightProperty, finalRect.Height);

			return finalRect.Size;
		}

		void ISupportInitialize.BeginInit()
		{
		}

		void ISupportInitialize.EndInit()
		{
			OnInitialized(EventArgs.Empty);
		}

		public static FlowDirection GetFlowDirection(DependencyObject d)
		{
			return (FlowDirection)d.GetValue(FlowDirectionProperty);
		}

		object IResourceHost.GetResource(object key)
		{
			if(_resources == null)
				return null;

			return _resources[key];
		}

		protected override object GetValueCore(DependencyProperty dp, object baseValue, PropertyMetadata metadata)
		{
			// no default implementation
			return DependencyProperty.UnsetValue;
		}

		protected override sealed Size MeasureCore(Size availableSize)
		{
			return MeasureOverride(availableSize);
		}

		protected virtual Size MeasureOverride(Size availableSize)
		{
			return new Size((double)GetValue(WidthProperty), (double)GetValue(HeightProperty));
		}

		protected virtual void OnInitialized(EventArgs e)
		{
			_isInitialized = true;

			if(Initialized != null)
				Initialized(this, EventArgs.Empty);

			PrepareTriggers();
		}

		protected internal override void OnVisualParentChanged(Visual oldParent)
		{
			OnInitialized(EventArgs.Empty);
		}
			
		protected void PrepareTriggers()
		{
			if(_triggers == null)
				return;

			MediaPortal.GUI.Library.Log.Write("PrepareTriggers");

			foreach(TriggerBase trigger in _triggers)
			{
				if(trigger is EventTrigger)
					PrepareEventTrigger((EventTrigger)trigger);
			}
		}

		private void PrepareEventTrigger(EventTrigger trigger)
		{
			MediaPortal.GUI.Library.Log.Write("PrepareTriggers: {0}", trigger.RoutedEvent.ToString());

			if(trigger.RoutedEvent == Page.LoadedEvent)
				MediaPortal.GUI.Library.Log.Write("FIRE FIRE FIRE IN THE WHOLE");

			foreach(TriggerAction action in trigger.Actions)
			{
			}
		}

		public static void SetFlowDirection(DependencyObject d, FlowDirection flowDirection)
		{
			d.SetValue(FlowDirectionProperty, flowDirection);
		}

		#endregion Methods

		#region Properties

		// TODO: should not be virtual and must be double
		public virtual int ActualHeight
		{
			get { return (int)(double)GetValue(ActualHeightProperty); }
			set { SetValue(ActualHeightProperty, (double)value); }
		}

		// TODO: should not be virtual and must be double
		public virtual int ActualWidth
		{
			get { return (int)(double)GetValue(ActualWidthProperty); }
			set { SetValue(ActualWidthProperty, (double)value); }
		}

		public FlowDirection FlowDirection
		{
			get { return (FlowDirection)GetValue(FlowDirectionProperty); }
			set { SetValue(FlowDirectionProperty, value); }
		}

		public bool Focusable
		{
			get { return (bool)GetValue(FocusableProperty); }
			set { SetValue(FocusableProperty, value); }
		}

		// TODO: should not be virtual and must be double
		public virtual int Height
		{
			get { return (int)(double)GetValue(HeightProperty); }
			set { SetValue(HeightProperty, (double)value); }
		}

		public HorizontalAlignment HorizontalAlignment
		{
			get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
			set { SetValue(HorizontalAlignmentProperty, value); }
		}

		public bool IsInitialized
		{
			get { return _isInitialized; }
		}

		public bool IsLoaded
		{
			get { return _isLoaded; }
		}

		protected bool IsTreeSeperator
		{
			get { return _isTreeSeperator; }
			set { if(_isLoaded == false) throw new InvalidOperationException(); _isTreeSeperator = value; }
		}

		// TODO: Remove this
		public virtual Point Location
		{
			get { return _location; }
			set { _location = value; }
		}

		protected internal virtual IEnumerator LogicalChildren
		{
			get { return NullEnumerator.Instance; }
		}

		public Thickness Margin
		{
			get { return (Thickness)GetValue(MarginProperty); }
			set { SetValue(MarginProperty, value); }
		}

		public string Name
		{
			get { return (string)GetValue(NameProperty); }
			set { SetValue(NameProperty, value); }
		}

		IResourceHost IResourceHost.ParentResourceHost
		{
			get { return base.VisualParent as IResourceHost; }
		}
		
		public ResourceDictionary Resources
		{
			get { if(_resources == null) _resources = new ResourceDictionary(); return _resources; }
			set { _resources = value; }
		}

		public Style Style
		{
			get { return (Style)GetValue(StyleProperty); }
			set { SetValue(StyleProperty, value); }
		}

		public TriggerCollection Triggers
		{
			get { if(_triggers == null) _triggers = new TriggerCollection(); return _triggers; }
		}

		public VerticalAlignment VerticalAlignment
		{
			get { return (VerticalAlignment)GetValue(VerticalAlignmentProperty); }
			set { SetValue(VerticalAlignmentProperty, value); }
		}

		// TODO: should not be virtual and must be double
		public virtual int Width
		{
			get { return (int)((double)GetValue(WidthProperty)); }
			set { SetValue(WidthProperty, (double)value); }
		}

		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty ActualHeightProperty;
		public static readonly DependencyProperty ActualWidthProperty;
		public static readonly DependencyProperty FlowDirectionProperty;
		public static readonly DependencyProperty FocusableProperty;
		public static readonly DependencyProperty HeightProperty;
		public static readonly DependencyProperty HorizontalAlignmentProperty;
		public static readonly DependencyProperty MarginProperty;
		public static readonly DependencyProperty NameProperty;
		public static readonly DependencyProperty StyleProperty;
		public static readonly DependencyProperty VerticalAlignmentProperty;
		public static readonly DependencyProperty WidthProperty;

		#endregion Properties (Dependency)

		#region Fields

		bool						_isInitialized = false;
		bool						_isLoaded = false;
		bool						_isTreeSeperator = false;
		Point						_location = Point.Empty;
		ResourceDictionary			_resources;
		TriggerCollection			_triggers;

		#endregion Fields
	}
}
