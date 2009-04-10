#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MediaPortal.Drawing;
using MediaPortal.GUI.Library;
using Geometry=MediaPortal.Drawing.Geometry;

namespace System.Windows
{
  public class UIElement : Visual, IInputElement, IAnimatable
  {
    #region Constructors

    static UIElement()
    {
      UIPropertyMetadata metadata;

      GotKeyboardFocusEvent = EventManager.RegisterRoutedEvent("GotKeyboardFocus", RoutingStrategy.Direct,
                                                               typeof (KeyboardFocusChangedEventHandler),
                                                               typeof (UIElement));
      LostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("LostKeyboardFocus", RoutingStrategy.Direct,
                                                                typeof (KeyboardFocusChangedEventHandler),
                                                                typeof (UIElement));
      IsEnabledChangedEvent = EventManager.RegisterRoutedEvent("IsEnabledChanged", RoutingStrategy.Direct,
                                                               typeof (RoutedEventHandler), typeof (UIElement));
      IsKeyboardFocusedChangedEvent = EventManager.RegisterRoutedEvent("IsKeyboardFocusedChanged",
                                                                       RoutingStrategy.Direct,
                                                                       typeof (RoutedEventHandler), typeof (UIElement));
      IsKeyboardFocusWithinChangedEvent = EventManager.RegisterRoutedEvent("IsKeyboardFocusWithinChanged",
                                                                           RoutingStrategy.Direct,
                                                                           typeof (RoutedEventHandler),
                                                                           typeof (UIElement));
      IsVisibleChangedEvent = EventManager.RegisterRoutedEvent("IsVisibleChanged", RoutingStrategy.Direct,
                                                               typeof (RoutedEventHandler), typeof (UIElement));
      PreviewLostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("PreviewLostKeyboardFocus",
                                                                       RoutingStrategy.Direct,
                                                                       typeof (KeyboardFocusChangedEventHandler),
                                                                       typeof (UIElement));
      LostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("PreviewGotKeyboardFocus", RoutingStrategy.Direct,
                                                                typeof (KeyboardFocusChangedEventHandler),
                                                                typeof (UIElement));

      #region IsEnabled

      metadata = new UIPropertyMetadata();
      metadata.DefaultValue = true;
      metadata.GetValueOverride = new GetValueOverride(OnIsEnabledPropertyGetValue);
      metadata.PropertyInvalidatedCallback = new PropertyInvalidatedCallback(OnIsEnabledPropertyInvalidated);

      IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof (bool), typeof (UIElement), metadata);

      #endregion IsEnabled

      #region IsFocused

      metadata = new UIPropertyMetadata();
      metadata.DefaultValue = false;
      metadata.GetValueOverride = new GetValueOverride(OnIsFocusedPropertyGetValue);
      metadata.PropertyInvalidatedCallback = new PropertyInvalidatedCallback(OnIsFocusedPropertyInvalidated);

      IsFocusedProperty = DependencyProperty.Register("IsFocused", typeof (bool), typeof (UIElement), metadata);

      #endregion IsFocused

      IsKeyboardFocusedPropertyKey = DependencyProperty.RegisterReadOnly("IsKeyboardFocused", typeof (bool),
                                                                         typeof (UIElement),
                                                                         new FrameworkPropertyMetadata(false));
      IsKeyboardFocusedProperty = IsKeyboardFocusedPropertyKey.DependencyProperty;

      IsKeyboardFocusWithinPropertyKey = DependencyProperty.RegisterReadOnly("IsKeyboardFocusWithin", typeof (bool),
                                                                             typeof (UIElement),
                                                                             new FrameworkPropertyMetadata(false));
      IsKeyboardFocusWithinProperty = IsKeyboardFocusWithinPropertyKey.DependencyProperty;

      IsVisiblePropertyKey = DependencyProperty.RegisterReadOnly("IsVisible", typeof (bool), typeof (UIElement),
                                                                 new FrameworkPropertyMetadata(true));
      IsVisibleProperty = IsVisiblePropertyKey.DependencyProperty;

      OpacityMaskProperty = DependencyProperty.Register("OpacityMask", typeof (Brush), typeof (UIElement));
      OpacityProperty = DependencyProperty.Register("Opacity", typeof (double), typeof (UIElement),
                                                    new FrameworkPropertyMetadata(1.0));

      #region IsFocused

      metadata = new UIPropertyMetadata();
      metadata.DefaultValue = Visibility.Visible;
      metadata.GetValueOverride = new GetValueOverride(OnVisibilityPropertyGetValue);
      metadata.PropertyInvalidatedCallback = new PropertyInvalidatedCallback(OnVisibilityPropertyInvalidated);

      VisibilityProperty = DependencyProperty.Register("Visibility", typeof (Visibility), typeof (UIElement), metadata);

      #endregion IsFocused

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

    public void AddHandler(RoutedEvent routedEvent, Delegate handler)
    {
      AddHandler(routedEvent, handler, true);
    }

    public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
    {
      if (_eventHandlersStore == null)
      {
        _eventHandlersStore = new EventHandlersStore();
      }

      _eventHandlersStore.AddRoutedEventHandler(routedEvent, handler, handledEventsToo);
    }

    public void AddToEventRoute(EventRoute route, RoutedEventArgs e)
    {
      BuildRouteCore(route, e);
    }

    protected internal virtual object AdjustEventSource(RoutedEventArgs args)
    {
      // default implementation always returns null
      return null;
    }

    public void ApplyAnimationClock(DependencyProperty property, AnimationClock clock)
    {
      ApplyAnimationClock(property, clock, HandoffBehavior.SnapshotAndReplace);
    }

    public void ApplyAnimationClock(DependencyProperty property, AnimationClock clock, HandoffBehavior handoffBehavior)
    {
      if (clock == null)
      {
        // if clock parameter is null we are to remove the animation from the property
        if (_animationStore != null)
        {
          _animationStore.RemoveAnimationClock(property, clock);
        }

        return;
      }

      if (_animationStore == null)
      {
        _animationStore = new AnimationStore();
      }

      _animationStore.ApplyAnimationClock(property, clock, handoffBehavior);
    }

    public void Arrange(Rect finalRect)
    {
      ArrangeCore(finalRect);
    }

    protected virtual void ArrangeCore(Rect finalRect)
    {
      // no default implementation
    }

    public void BeginAnimation(DependencyProperty property, AnimationTimeline animation)
    {
      BeginAnimation(property, animation, HandoffBehavior.SnapshotAndReplace);
    }

    public void BeginAnimation(DependencyProperty property, AnimationTimeline animation, HandoffBehavior handoffBehavior)
    {
      // If the animation's BeginTime is NULL, then any current animations will be removed 
      // and the current value of the property will be held. If the entire animation value 
      // is given as NULL, all animations will be removed from the property and the property value 
      // will revert back to its base value.

//			if(animation == null)
//				_animatedStore.RemoveAnimations(property);
//			else if(animation.BeginTime.HasValue)
//				throw new NotImplementedException();
//			else
//				throw new NotImplementedException();


      if (_animationStore == null)
      {
        _animationStore = new AnimationStore();
      }

      _animationStore.BeginAnimation(property, animation, handoffBehavior);
    }

    protected virtual bool BuildRouteCore(EventRoute route, RoutedEventArgs args)
    {
      return true;
    }

    public bool Focus()
    {
      return true;

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
    }

    public object GetAnimationBaseValue(DependencyProperty property)
    {
      return GetValueBase(property);
    }

//		protected internal virtual IAutomationPropertyProvider GetAutomationProvider()

    protected virtual Geometry GetLayoutClip(Size layoutSlotSize)
    {
      return new RectangleGeometry(new Rect(Point.Empty, RenderSize));
    }

    protected internal virtual DependencyObject GetUIParentCore()
    {
      return null;
    }

    protected override object GetValueCore(DependencyProperty property, object baseValue, PropertyMetadata metadata)
    {
      if (HasAnimatedProperties)
      {
        baseValue = _animationStore.GetValue(property, baseValue, metadata);
      }

      return base.GetValueCore(property, baseValue, metadata);
    }

    public void InvalidateArrange()
    {
      _isArrangeValid = false;

      // after the invalidation, the element will have its layout updated, which will occur asynchronously.
      ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateLayoutWorker), this);
    }

    public void InvalidateMeasure()
    {
      _isMeasureValid = false;

      InvalidateArrange();
    }

    public void InvalidateVisual()
    {
      InvalidateArrange();
    }

    public void Measure(Size availableSize)
    {
      // http://winfx.msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/T_System_Windows_UIElement_Members.asp
      MeasureCore(availableSize);
    }

    protected virtual Size MeasureCore(Size availableSize)
    {
      // http://winfx.msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/T_System_Windows_UIElement_Members.asp
      return Size.Empty;
    }

    protected virtual void OnChildDesiredSizeChanged(UIElement child)
    {
      InvalidateMeasure();
    }

    protected virtual void OnKeyDown(KeyEventArgs e)
    {
      // no default implementation
    }

    protected virtual void OnKeyUp(KeyEventArgs e)
    {
      // no default implementation
    }

    private static object OnIsEnabledPropertyGetValue(DependencyObject d)
    {
      return ((UIElement) d).IsEnabled;
    }

    private static void OnIsEnabledPropertyInvalidated(DependencyObject d)
    {
      ((UIElement) d)._isEnabledDirty = true;
    }

    private static object OnIsFocusedPropertyGetValue(DependencyObject d)
    {
      return ((UIElement) d).IsFocused;
    }

    private static void OnIsFocusedPropertyInvalidated(DependencyObject d)
    {
      ((UIElement) d)._isFocusedDirty = true;
    }

    protected internal virtual void OnIsFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
      throw new NotImplementedException();
    }

    protected internal virtual void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
    }

    protected virtual void OnMouseEnter(MouseEventArgs e)
    {
      // no default implementation
    }

    protected virtual void OnMouseLeave(MouseEventArgs e)
    {
      // no default implementation
    }

    protected virtual void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      throw new NotImplementedException();
    }

    protected virtual void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
      throw new NotImplementedException();
    }

    protected virtual void OnMouseMove(MouseEventArgs e)
    {
      throw new NotImplementedException();
    }

    protected virtual void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
      throw new NotImplementedException();
    }

    protected virtual void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
      throw new NotImplementedException();
    }

    protected virtual void OnMouseWheel(MouseWheelEventArgs e)
    {
      throw new NotImplementedException();
    }

    protected virtual void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
    }

    protected virtual void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
    }

    protected virtual void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
    }

    protected override void OnPropertyInvalidated(DependencyProperty property, PropertyMetadata metadata)
    {
      // no default implementation?!?!?
    }

    protected virtual void OnQueryEnabled(object sender, QueryEnabledEventArgs e)
    {
      if (_commandBindings == null)
      {
        return;
      }

      foreach (CommandBinding binding in _commandBindings)
      {
        if (binding.Command == e.Command)
        {
          e.IsEnabled = binding.Command.IsEnabled;
        }
      }
    }

    protected virtual void OnRender(DrawingContext dc)
    {
      // no default implementation
    }

    protected internal virtual void OnRenderSizeChanged(SizeChangedInfo info)
    {
      // no default implementation
    }

    private static object OnVisibilityPropertyGetValue(DependencyObject d)
    {
      return ((UIElement) d).Visibility;
    }

    private static void OnVisibilityPropertyInvalidated(DependencyObject d)
    {
      ((UIElement) d)._visibilityDirty = true;
    }

    public void RaiseEvent(RoutedEventArgs e)
    {
      if (_eventHandlersStore == null)
      {
        return;
      }

      foreach (RoutedEventHandlerInfo handler in _eventHandlersStore.GetRoutedEventHandlers(e.RoutedEvent))
      {
        if (e.Handled && handler.InvokeHandledEventsToo == false)
        {
          continue;
        }

        handler.InvokeHandler(this, e);
      }
    }

    public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
    {
      if (_eventHandlersStore != null)
      {
        _eventHandlersStore.RemoveRoutedEventHandler(routedEvent, handler);
      }
    }

    public DrawingContext RenderOpen()
    {
      throw new NotImplementedException();
    }

    public Point TranslatePoint(Point pt, UIElement relativeTo)
    {
      throw new NotImplementedException();
    }

    public void UpdateLayout()
    {
      // only update when necessary to do so
      if (_isMeasureValid && _isArrangeValid)
      {
        return;
      }

      // This ensures that elements with IsMeasureValid==false or IsArrangeValid==false will call 
      // element-specific MeasureCore and ArrangeCore methods, forces layout update, and all computed
      // sizes will be validated.

      // This method does nothing if layout is unchanged, or if neither arrangement nor measurement 
      // of a layout is invalid. However, if layout is invalid in either respect, the method call will 
      // redo the entire layout, therefore avoid calling it after each minute change in the element tree. 
      // In fact, it makes sense to either never call it (the layout system will do this in a deferred manner, 
      // in a way that balances performance and currency) or only call it if you absolutely need updated sizes 
      // and positions after you do all changes to properties that may affect layout. 
    }

    protected void UpdateLayoutWorker(object state)
    {
//			UIElement element = (UIElement)state;
//			element.Arrange(
    }

    #endregion Methods

    #region Properties

    public Size DesiredSize
    {
      get { return _desiredSize; }
    }

    public bool HasAnimatedProperties
    {
      get { return _animationStore != null && _animationStore.HasAnimatedProperties; }
    }

    public bool IsArrangeValid
    {
      get { return _isArrangeValid; }
    }

    public bool IsEnabled
    {
      get { return IsEnabledCore; }
      set { SetValue(IsEnabledProperty, value); }
    }

    protected virtual bool IsEnabledCore
    {
      get
      {
        if (_isEnabledDirty)
        {
          _isEnabledCache = (bool) GetValueBase(IsEnabledProperty);
          _isEnabledDirty = false;
        }
        return _isEnabledCache;
      }
    }

    public bool IsFocused
    {
      get
      {
        if (_isFocusedDirty)
        {
          _isFocusedCache = (bool) GetValueBase(IsFocusedProperty);
          _isFocusedDirty = false;
        }
        return _isFocusedCache;
      }
    }

    public bool IsMeasureValid
    {
      get { return _isMeasureValid; }
    }

    public CommandBindingCollection CommandBindings
    {
      get
      {
        if (_commandBindings == null)
        {
          _commandBindings = new CommandBindingCollection();
        }
        return _commandBindings;
      }
    }

    [XMLSkinElement("visible")]
    public virtual bool IsVisible
    {
      // TODO: there should be no set accessor
      get { return Visibility == Visibility.Visible; }
      set { Visibility = value ? Visibility.Visible : Visibility.Hidden; }
    }

    public virtual double Opacity
    {
      get { return (double) GetValue(OpacityProperty); }
      set { SetValue(OpacityProperty, value); }
    }

    public Brush OpacityMask
    {
      get { return (Brush) GetValue(OpacityMaskProperty); }
      set { SetValue(OpacityMaskProperty, value); }
    }

    public Size RenderSize
    {
      get { return _renderSize; }
      set { _renderSize = value; }
    }

    public Visibility Visibility
    {
      get
      {
        if (_visibilityDirty)
        {
          _visibilityCache = (Visibility) GetValueBase(VisibilityProperty);
          _visibilityDirty = false;
        }
        return _visibilityCache;
      }
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

    private static readonly DependencyPropertyKey IsKeyboardFocusedPropertyKey;
    private static readonly DependencyPropertyKey IsKeyboardFocusWithinPropertyKey;
    private static readonly DependencyPropertyKey IsVisiblePropertyKey;

    #endregion Properties (Dependency)

    #region Fields

    private AnimationStore _animationStore;
    private CommandBindingCollection _commandBindings;
    private Size _desiredSize = Size.Empty;
    private EventHandlersStore _eventHandlersStore;
    private bool _isArrangeValid = false;
    private bool _isEnabledCache;
    private bool _isEnabledDirty = true;
    private bool _isFocusedCache;
    private bool _isFocusedDirty = true;
    private bool _isMeasureValid = false;
    private Size _renderSize = Size.Empty;
    private Visibility _visibilityCache;
    private bool _visibilityDirty = true;

    #endregion Fields
  }
}