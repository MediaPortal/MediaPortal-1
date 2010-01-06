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

using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MediaPortal.Drawing;

namespace System.Windows
{
  public class FrameworkElement : UIElement, IFrameworkInputElement, IInputElement, ISupportInitialize, IResourceHost
  {
    #region Constructors

    static FrameworkElement()
    {
      FrameworkPropertyMetadata metadata;

      ActualHeightProperty = DependencyProperty.Register("ActualHeight", typeof (double), typeof (FrameworkElement),
                                                         new FrameworkPropertyMetadata(0.0,
                                                                                       new PropertyInvalidatedCallback(
                                                                                         ActualHeightPropertyInvalidated)));
      ActualWidthProperty = DependencyProperty.Register("ActualWidth", typeof (double), typeof (FrameworkElement),
                                                        new FrameworkPropertyMetadata(0.0,
                                                                                      new PropertyInvalidatedCallback(
                                                                                        ActualWidthPropertyInvalidated)));

      #region ContextMenu

      ContextMenuProperty = ContextMenuService.ContextMenuProperty.AddOwner(typeof (FrameworkElement));

      #endregion ContextMenu

      FlowDirectionProperty = DependencyProperty.Register("FlowDirection", typeof (FlowDirection),
                                                          typeof (FrameworkElement),
                                                          new FrameworkPropertyMetadata(FlowDirection.LeftToRight));

      #region Focusable

      metadata = new FrameworkPropertyMetadata();
      metadata.DefaultValue = true;
      metadata.PropertyInvalidatedCallback = new PropertyInvalidatedCallback(OnFocusablePropertyInvalidated);
      metadata.GetValueOverride = new GetValueOverride(OnFocusablePropertyGetValue);

      FocusableProperty = DependencyProperty.Register("Focusable", typeof (bool), typeof (FrameworkElement), metadata);

      #endregion Focusable

      #region HeightProperty

      metadata = new FrameworkPropertyMetadata();
      metadata.DefaultValue = 0.0;
      metadata.PropertyInvalidatedCallback = new PropertyInvalidatedCallback(OnHeightPropertyInvalidated);
      metadata.GetValueOverride = new GetValueOverride(OnHeightPropertyGetValue);
      metadata.AffectsArrange = true;
      metadata.AffectsMeasure = true;
      metadata.AffectsParentArrange = true;
      metadata.AffectsParentMeasure = true;

      HeightProperty = DependencyProperty.Register("Height", typeof (double), typeof (FrameworkElement), metadata);

      #endregion HeightProperty

      HorizontalAlignmentProperty = DependencyProperty.Register("HorizontalAlignment", typeof (HorizontalAlignment),
                                                                typeof (FrameworkElement),
                                                                new FrameworkPropertyMetadata(
                                                                  HorizontalAlignment.Stretch,
                                                                  FrameworkPropertyMetadataOptions.AffectsArrange));
      MarginProperty = DependencyProperty.Register("Margin", typeof (Thickness), typeof (FrameworkElement),
                                                   new FrameworkPropertyMetadata(Thickness.Empty));
      NameProperty = DependencyProperty.Register("Name", typeof (string), typeof (FrameworkElement),
                                                 new FrameworkPropertyMetadata(string.Empty));
      StyleProperty = DependencyProperty.Register("Style", typeof (Style), typeof (FrameworkElement),
                                                  new FrameworkPropertyMetadata());
      VerticalAlignmentProperty = DependencyProperty.Register("VerticalAlignment", typeof (VerticalAlignment),
                                                              typeof (FrameworkElement),
                                                              new FrameworkPropertyMetadata(VerticalAlignment.Stretch));

      #region WidthProperty

      metadata = new FrameworkPropertyMetadata();
      metadata.DefaultValue = 0.0;
      metadata.PropertyInvalidatedCallback = new PropertyInvalidatedCallback(OnWidthPropertyInvalidated);
      metadata.GetValueOverride = new GetValueOverride(OnWidthPropertyGetValue);
      metadata.AffectsArrange = true;
      metadata.AffectsMeasure = true;
      metadata.AffectsParentArrange = true;
      metadata.AffectsParentMeasure = true;

      WidthProperty = DependencyProperty.Register("Width", typeof (double), typeof (FrameworkElement), metadata);

      #endregion WidthProperty

      LoadedEvent = EventManager.RegisterRoutedEvent("Loaded", RoutingStrategy.Direct, typeof (RoutedEventHandler),
                                                     typeof (FrameworkElement));
      RequestBringIntoViewEvent = EventManager.RegisterRoutedEvent("RequestBringIntoView", RoutingStrategy.Direct,
                                                                   typeof (RequestBringIntoViewEventHandler),
                                                                   typeof (FrameworkElement));
      SizeChangedEvent = EventManager.RegisterRoutedEvent("SizeChanged", RoutingStrategy.Direct,
                                                          typeof (RoutedEventHandler), typeof (FrameworkElement));
      UnloadedEvent = EventManager.RegisterRoutedEvent("Unloaded", RoutingStrategy.Direct, typeof (RoutedEventHandler),
                                                       typeof (FrameworkElement));
    }

    public FrameworkElement() {}

    #endregion Constructors

    #region Events

    public event EventHandler Initialized;

    public event RoutedEventHandler Loaded
    {
      add { AddHandler(LoadedEvent, value); }
      remove { RemoveHandler(LoadedEvent, value); }
    }

    public event RequestBringIntoViewEventHandler RequestBringIntoView
    {
      add { AddHandler(RequestBringIntoViewEvent, value); }
      remove { RemoveHandler(RequestBringIntoViewEvent, value); }
    }

    public event RoutedEventHandler SizeChanged
    {
      add { AddHandler(SizeChangedEvent, value); }
      remove { RemoveHandler(SizeChangedEvent, value); }
    }

    public event RoutedEventHandler Unloaded
    {
      add { AddHandler(UnloadedEvent, value); }
      remove { RemoveHandler(UnloadedEvent, value); }
    }

    #endregion Events

    #region Events (Routed)

    public static readonly RoutedEvent LoadedEvent;
    public static readonly RoutedEvent RequestBringIntoViewEvent;
    public static readonly RoutedEvent SizeChangedEvent;
    public static readonly RoutedEvent UnloadedEvent;

    #endregion Events (Routed)

    #region Methods

    protected internal void AddLogicalChild(object child) {}

    protected internal override object AdjustEventSource(RoutedEventArgs args)
    {
      // return null if no change occurred
      return null;
    }

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
      BeginInit();
    }

    public virtual void BeginInit()
    {
      if (_isInitializing)
      {
        throw new InvalidOperationException();
      }

      _isInitializing = true;
    }

    public void BeginStoryboard(Storyboard storyboard)
    {
      BeginStoryboard(storyboard, HandoffBehavior.SnapshotAndReplace, false);
    }

    public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior)
    {
      BeginStoryboard(storyboard, handoffBehavior, false);
    }

    public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior, bool isControllable)
    {
      // For the signatures that do not use the isControllable, parameter, or when that 
      // parameter is specified false, the timeline clocks associated with the animation
      // are removed as soon as it reaches the "Fill" period. Therefore the animation can't
      // be restarted after running once. Note that controlling an animation also requires
      // that the storyboard be named or accessible as an instance in procedural code.
    }

    public void BringIntoView()
    {
      BringIntoView(new Rect(Point.Empty, RenderSize));
    }

    public void BringIntoView(Rect targetRectangle)
    {
      // ScrollContentPresenter.MakeVisible
    }

    protected override sealed bool BuildRouteCore(EventRoute route, RoutedEventArgs args)
    {
      return VisualParent == null;
    }

    void ISupportInitialize.EndInit()
    {
      EndInit();
    }

    public virtual void EndInit()
    {
      if (_isInitializing == false)
      {
        return;
      }

      RaiseEvent(new RoutedEventArgs(LoadedEvent, this));

      OnInitialized(EventArgs.Empty);
    }

    public virtual bool EnsureVisuals()
    {
      return false;
    }

    public object FindName(string name)
    {
      object namedObject = null;

      if (_names != null)
      {
        namedObject = _names[name];
      }

//			if(namedObject == null)
//				namedObject = LogicalTreeHelper.GetChildren(this);

      return namedObject;
    }

    public object FindResource(object key)
    {
      object resource = null;

      if (_resources != null)
      {
        resource = _resources[key];
      }

      if (resource == null)
      {
        resource = LogicalTreeHelper.FindLogicalNode(this, (string)key);
      }

      return resource;
    }

//		protected internal override IAutomationPropertyProvider GetAutomationProvider ()
//		{
//		}

//		public BindingExpression GetBindingExpression(DependencyProperty dp)
//		{
//		}

    public static FlowDirection GetFlowDirection(DependencyObject d)
    {
      return (FlowDirection)d.GetValue(FlowDirectionProperty);
    }

    protected override Geometry GetLayoutClip(Size layoutSlotSize)
    {
      return null;
    }

    object IResourceHost.GetResource(object key)
    {
      return FindResource(key);
    }

    // MSDN states to use FindName instead
    protected internal DependencyObject GetTemplateChild(string childName)
    {
      return FindName(childName) as DependencyObject;
    }

    protected internal override DependencyObject GetUIParentCore()
    {
      return this.VisualParent;
    }

    protected override object GetValueCore(DependencyProperty property, object baseValue, PropertyMetadata metadata)
    {
      return base.GetValueCore(property, baseValue, metadata);
    }

    protected override sealed Size MeasureCore(Size availableSize)
    {
      return MeasureOverride(availableSize);
    }

    protected virtual Size MeasureOverride(Size availableSize)
    {
      return availableSize;
    }

    private static object OnHeightPropertyGetValue(DependencyObject d)
    {
      return ((FrameworkElement)d).Height;
    }

    private static void OnHeightPropertyInvalidated(DependencyObject d)
    {
      ((FrameworkElement)d)._heightDirty = true;
    }

    protected virtual void OnInitialized(EventArgs e)
    {
      _isInitialized = true;

      if (Initialized != null)
      {
        Initialized(this, EventArgs.Empty);
      }
    }

    private static object OnFocusablePropertyGetValue(DependencyObject d)
    {
      return ((FrameworkElement)d)._focusableCache;
    }

    private static void OnFocusablePropertyInvalidated(DependencyObject d)
    {
      ((FrameworkElement)d)._focusableDirty = true;
    }

    protected override void OnPropertyInvalidated(DependencyProperty property, PropertyMetadata metadata) {}

    protected internal override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {}

    protected internal override void OnVisualParentChanged(Visual oldParent)
    {
      // the default implementation of this virtual method queries for the new parent,
      // and raises various initialization events/sets internal flags about initialization
      // state as appropriate. Always call base() to preserve this behavior.

      OnInitialized(EventArgs.Empty);
    }

    private static object OnWidthPropertyGetValue(DependencyObject d)
    {
      return ((FrameworkElement)d).Width;
    }

    private static void OnWidthPropertyInvalidated(DependencyObject d)
    {
      ((FrameworkElement)d)._widthDirty = true;
    }

    protected internal virtual void ParentLayoutInvalidated(UIElement child) {}

    protected internal void RemoveLogicalChild(object child) {}

    public static void SetFlowDirection(DependencyObject d, FlowDirection flowDirection)
    {
      d.SetValue(FlowDirectionProperty, flowDirection);
    }

    public void SetResourceReference(DependencyProperty property, object name) {}

    #endregion Methods

    #region Properties

    // TODO: should not be virtual and must be double
    public virtual int ActualHeight
    {
      get { return (int)(double)GetValue(ActualHeightProperty); }
    }

    // TODO: should not be virtual and must be double
    public virtual int ActualWidth
    {
      get { return (int)(double)GetValue(ActualWidthProperty); }
    }

    public ContextMenu ContextMenu
    {
      get { return GetValue(ContextMenuProperty) as ContextMenu; }
      set
      {
        SetValue(ContextMenuProperty, value);
        InsertTestAnimation();
      }
    }

    private void InsertTestAnimation()
    {
      DoubleAnimation myDoubleAnimation = new DoubleAnimation();

      myDoubleAnimation.From = 75;
      myDoubleAnimation.To = 300;
      myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(20000));
      myDoubleAnimation.AutoReverse = true;
      myDoubleAnimation.RepeatBehavior = RepeatBehavior.Forever;

      BeginAnimation(WidthProperty, myDoubleAnimation);
      ApplyAnimationClock(WidthProperty, myDoubleAnimation.CreateClock());
    }

    public FlowDirection FlowDirection
    {
      get { return (FlowDirection)GetValue(FlowDirectionProperty); }
      set { SetValue(FlowDirectionProperty, value); }
    }

    public bool Focusable
    {
      get
      {
        if (_focusableDirty)
        {
          _focusableCache = (bool)GetValueBase(FocusableProperty);
          _focusableDirty = false;
        }
        return _focusableCache;
      }
      set { SetValue(FocusableProperty, value); }
    }

    // TODO: should not be virtual and must be double
    public virtual int Height
    {
      get
      {
        if (_heightDirty)
        {
          _heightCache = (double)GetValueBase(HeightProperty);
          _heightDirty = false;
        }
        return (int)_heightCache;
      }
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
      set
      {
        if (_isLoaded == false)
        {
          throw new InvalidOperationException();
        }
        _isTreeSeperator = value;
      }
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

    public DependencyObject Parent
    {
      get { throw new NotImplementedException(); }
    }

    IResourceHost IResourceHost.ParentResourceHost
    {
      get { return base.VisualParent as IResourceHost; }
    }

    public ResourceDictionary Resources
    {
      get
      {
        if (_resources == null)
        {
          _resources = new ResourceDictionary();
        }
        return _resources;
      }
      set { _resources = value; }
    }

    public Style Style
    {
      get { return (Style)GetValue(StyleProperty); }
      set { SetValue(StyleProperty, value); }
    }

    public DependencyObject TemplatedParent
    {
      get { return _templatedParent; }
    }

    public TriggerCollection Triggers
    {
      get
      {
        if (_triggers == null)
        {
          _triggers = new TriggerCollection();
        }
        return _triggers;
      }
    }

    public VerticalAlignment VerticalAlignment
    {
      get { return (VerticalAlignment)GetValue(VerticalAlignmentProperty); }
      set { SetValue(VerticalAlignmentProperty, value); }
    }

    // TODO: should not be virtual and must be double
    public virtual int Width
    {
      get
      {
        if (_widthDirty)
        {
          _widthCache = (double)GetValueBase(WidthProperty);
          _widthDirty = false;
        }
        return (int)_widthCache;
      }
      set { SetValue(WidthProperty, (double)value); }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty ActualHeightProperty;
    public static readonly DependencyProperty ActualWidthProperty;
    public static readonly DependencyProperty ContextMenuProperty;
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

    private bool _focusableCache;
    private bool _focusableDirty = true;
    private double _heightCache;
    private bool _heightDirty = true;
    private bool _isInitialized = false;
    private bool _isInitializing = false;
    private bool _isLoaded = false;
    private bool _isTreeSeperator = false;
    private Hashtable _names = null;
    private Point _location = Point.Empty;
    private ResourceDictionary _resources;
    private DependencyObject _templatedParent = null;
    private TriggerCollection _triggers;
    private double _widthCache;
    private bool _widthDirty = true;

    #endregion Fields
  }
}