using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace MediaPortal
{
  public class GUIButton : Button, IGUIControl
  {
    private TextAlignment _align;
    private string _label;
    private string _texture;
    private string _hyperLink;
    private int _controlId;
    private int _onUp;
    private int _onDown;
    private int _onLeft;
    private int _onRight;

    private ScrollViewer _scrollViewer;
    private double _displayTime;
    private double _scrollPosition;
    private Storyboard _storyBoard;
    private bool _isButton;

    // getting & removing the handler we will call on setting the Click property

    protected override void OnClick()
    {
      base.OnClick();
      Core.OnClick(this);
    }

    public GUIButton()
    {
      string styleName = this.GetType().ToString() + "Style";
      styleName = styleName.Replace("MediaPortal.", "");
      object resource = null;
      try
      {
        resource = this.FindResource(styleName);
        if (resource != null)
        {
          this.Style = resource as Style;
        }
      }
      catch { }

      this.MouseEnter += new MouseEventHandler(GUIButton_MouseEnter);
      this.MouseLeave += new MouseEventHandler(GUIButton_MouseLeave);
      // start all actions after load is complete
      this.Loaded += new RoutedEventHandler(GUIButton_Loaded);
      this.Unloaded += new RoutedEventHandler(GUIButton_Unloaded);
      // default frame time
      FrameTime = 80;


    }
    protected override void OnMouseEnter(MouseEventArgs e)
    {
      base.OnMouseEnter(e);
      if (IsMouseOver == true && IsKeyboardFocused == false)
      {
        this.Focus();
      }
    }


    void GUIButton_MouseLeave(object sender, MouseEventArgs e)
    {
      AnimateEnd();
    }

    void GUIButton_MouseEnter(object sender, MouseEventArgs e)
    {
      AnimateStart();
    }

    // no setting of an style is allowed

    void GUIButton_Unloaded(object sender, RoutedEventArgs e)
    {
      AnimateEnd();
    }

    void AnimateEnd()
    {
      if (_storyBoard == null)
        return;
      _storyBoard.Stop(this);
      this.Opacity = 1;
      _scrollViewer.ScrollToHorizontalOffset(0);
      _scrollPosition = 0;
    }

    void AnimateStart()
    {
      DoubleAnimation positionAnimation = new DoubleAnimation(0, _scrollViewer.ScrollableWidth, new Duration(TimeSpan.FromMilliseconds(_displayTime * _scrollViewer.ScrollableWidth)));
      _storyBoard = new Storyboard();
      _storyBoard.Children.Add(positionAnimation);
      Storyboard.SetTargetProperty(positionAnimation, new PropertyPath("ScrollPosition"));
      _storyBoard.RepeatBehavior = RepeatBehavior.Forever;
      _storyBoard.AutoReverse = true;
      _storyBoard.Begin(this, HandoffBehavior.Compose, true);
    }



    void GUIButton_Loaded(object sender, RoutedEventArgs e)
    {
      Border b = (Border)VisualTreeHelper.GetChild(this, 0);
      try
      {
        _scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(b, 0);
      }
      catch { }
      // prevent to select the scrollviewer
      _scrollViewer.IsEnabled = false;
      _scrollViewer.Focusable = false;

      if (_scrollViewer == null)
        return;
      _scrollViewer.ScrollToHorizontalOffset(0);
      _scrollPosition = 0;
    }




    // properties

    // sets the speed for the animation
    public double FrameTime
    {
      get
      {
        return (double)GetValue(FrameTimeProperty);
      }
      set
      {
        SetValue(FrameTimeProperty, value);
      }
    }

    public static readonly DependencyProperty FrameTimeProperty =DependencyProperty.Register("FrameTime", typeof(double), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFrameTimeChanged)));

    private static void OnFrameTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;

      RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>((double)args.OldValue, (double)args.NewValue, FrameTimeChangedEvent);
      control.OnFrameTimeChanged(e);
    }

    public static readonly RoutedEvent FrameTimeChangedEvent = EventManager.RegisterRoutedEvent("FrameTimeChanged", RoutingStrategy.Bubble,typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIButton));

    public event RoutedPropertyChangedEventHandler<double> FrameTimeChanged
    {
      add { AddHandler(FrameTimeChangedEvent, value); }
      remove { RemoveHandler(FrameTimeChangedEvent, value); }
    }

    protected virtual void OnFrameTimeChanged(RoutedPropertyChangedEventArgs<double> args)
    {
      _displayTime = args.NewValue;

      RaiseEvent(args);
    }



    // the property to animate the scrolling
    protected double ScrollPosition
    {
      get
      {
        return (double)GetValue(ScrollPositionProperty);
      }
      set
      {
        SetValue(ScrollPositionProperty, value);
      }
    }

    protected static readonly DependencyProperty ScrollPositionProperty =DependencyProperty.Register("ScrollPosition", typeof(double), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScrollPositionChanged)));

    private static void OnScrollPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;

      RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>((double)args.OldValue, (double)args.NewValue, ScrollPositionChangedEvent);
      control.OnScrollPositionChanged(e);
    }

    protected static readonly RoutedEvent ScrollPositionChangedEvent = EventManager.RegisterRoutedEvent("ScrollPositionChanged", RoutingStrategy.Bubble,typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIButton));

    protected event RoutedPropertyChangedEventHandler<double> ScrollPositionChanged
    {
      add { AddHandler(ScrollPositionChangedEvent, value); }
      remove { RemoveHandler(ScrollPositionChangedEvent, value); }
    }

    protected virtual void OnScrollPositionChanged(RoutedPropertyChangedEventArgs<double> args)
    {
      _scrollPosition = args.NewValue;
      _scrollViewer.ScrollToHorizontalOffset(_scrollPosition);
      RaiseEvent(args);
    }

    // 
    public bool IsButton
    {
      get
      {
        return _isButton;
      }
      set
      {
        _isButton = value;
      }
    }

    public static readonly DependencyProperty IsButtonProperty =
    DependencyProperty.Register("IsButton", typeof(bool), typeof(GUIButton), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsButtonChanged)));

    protected event RoutedPropertyChangedEventHandler<bool> IsButtonChanged
    {
      add
      {
        AddHandler(IsButtonChangedEvent, value);
      }
      remove
      {
        RemoveHandler(IsButtonChangedEvent, value);
      }
    }

    public static readonly RoutedEvent IsButtonChangedEvent = EventManager.RegisterRoutedEvent(
    "IsButtonChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<bool>), typeof(GUIButton));

    private static void OnIsButtonChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;

      RoutedPropertyChangedEventArgs<bool> e = new RoutedPropertyChangedEventArgs<bool>((bool)args.OldValue, (bool)args.NewValue, IsButtonChangedEvent);
      control.OnIsButtonChanged(e);
    }

    protected virtual void OnIsButtonChanged(RoutedPropertyChangedEventArgs<bool> args)
    {
      IsButton = args.NewValue;
      RaiseEvent(args);
    }

    // control id

    public int ID
    {
      get
      {
        return _controlId;
      }
      set
      {
        _controlId = value;
      }
    }

    public static readonly DependencyProperty IDProperty =DependencyProperty.Register("ID", typeof(int), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIDChanged)));

    private static void OnIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnIDChanged(args);
    }


    protected virtual void OnIDChanged(DependencyPropertyChangedEventArgs args)
    {
      _controlId = (int)args.NewValue;
    }

    // OnUp

    public int OnUp
    {
      get
      {
        return _onUp;
      }
      set
      {
        _onUp = value;
      }
    }

    public static readonly DependencyProperty OnUpProperty =DependencyProperty.Register("OnUp", typeof(int), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnUpChanged)));

    private static void OnOnUpChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnOnUpChanged(args);
    }


    protected virtual void OnOnUpChanged(DependencyPropertyChangedEventArgs args)
    {
      _onUp = (int)args.NewValue;
    }


    // OnDown

    public int OnDown
    {
      get
      {
        return _onDown;
      }
      set
      {
        _onDown = value;
      }
    }

    public static readonly DependencyProperty OnDownProperty =DependencyProperty.Register("OnDown", typeof(int), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnDownChanged)));

    private static void OnOnDownChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnOnDownChanged(args);
    }


    protected virtual void OnOnDownChanged(DependencyPropertyChangedEventArgs args)
    {
      _onDown = (int)args.NewValue;
    }

    // OnLeft

    public int OnLeft
    {
      get
      {
        return _onLeft;
      }
      set
      {
        _onLeft = value;
      }
    }

    public static readonly DependencyProperty OnLeftProperty =DependencyProperty.Register("OnLeft", typeof(int), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnLeftChanged)));

    private static void OnOnLeftChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnOnLeftChanged(args);
    }


    protected virtual void OnOnLeftChanged(DependencyPropertyChangedEventArgs args)
    {
      _onLeft = (int)args.NewValue;
    }

    // OnRight

    public int OnRight
    {
      get
      {
        return _onRight;
      }
      set
      {
        _onRight = value;
      }
    }

    public static readonly DependencyProperty OnRightProperty =DependencyProperty.Register("OnRight", typeof(int), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnRightChanged)));

    private static void OnOnRightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnOnRightChanged(args);
    }


    protected virtual void OnOnRightChanged(DependencyPropertyChangedEventArgs args)
    {
      _onRight = (int)args.NewValue;
    }

    public string Label
    {
      get
      {
        return (string)GetValue(LabelProperty);
      }
      set
      {
        SetValue(LabelProperty, value);
      }
    }

    public static readonly DependencyProperty LabelProperty =DependencyProperty.Register("Label", typeof(string), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLabelChanged)));

    private static void OnLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnLabelChanged(args);
    }

    protected virtual void OnLabelChanged(DependencyPropertyChangedEventArgs args)
    {
      _label = (string)args.NewValue;
      Content = Core.GetLocalizedString((string)args.NewValue);
    }



    // hyperlink
    public string Hyperlink
    {
      get
      {
        return _hyperLink;
      }
      set
      {
        _hyperLink = value;
      }
    }

    public static readonly DependencyProperty HyperlinkProperty =DependencyProperty.Register("Hyperlink", typeof(string), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHyperlinkChanged)));

    private static void OnHyperlinkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnHyperlinkChanged(args);
    }

    protected virtual void OnHyperlinkChanged(DependencyPropertyChangedEventArgs args)
    {
      _hyperLink = (string)args.NewValue;
    }

    // Texture
    public string Texture
    {
      get
      {
        return _texture;
      }
      set
      {
        _texture = value;
      }
    }

    public static readonly DependencyProperty TextureProperty =DependencyProperty.Register("Texture", typeof(string), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTextureChanged)));

    private static void OnTextureChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnTextureChanged(args);
    }

    protected virtual void OnTextureChanged(DependencyPropertyChangedEventArgs args)
    {
      _texture = (string)args.NewValue;
    }

    public TextAlignment Align
    {
      get
      {
        return _align;
      }
      set
      {
        _align = value;
      }
    }

    public static readonly DependencyProperty AlignProperty =DependencyProperty.Register("Align", typeof(TextAlignment), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAlignChanged)));

    private static void OnAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnAlignChanged(args);
    }

    protected virtual void OnAlignChanged(DependencyPropertyChangedEventArgs args)
    {
      _align = (TextAlignment)args.NewValue;
    }
    // disabled color
    private Brush _disabledColor;

    public Brush DisabledColor
    {
      get
      {
        return _disabledColor;
      }
      set
      {
        _disabledColor = value;
      }
    }

    public static readonly DependencyProperty DisabledColorProperty =DependencyProperty.Register("DisabledColor", typeof(Brush), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisabledColorChanged)));

    private static void OnDisabledColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnDisabledColorChanged(args);
    }

    protected virtual void OnDisabledColorChanged(DependencyPropertyChangedEventArgs args)
    {
      _disabledColor = (Brush)args.NewValue;
    }



    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      if (_align == TextAlignment.Right)
        Canvas.SetLeft(this, _positionX - sizeInfo.NewSize.Width);
    }
    string IGUIControl.Label
    {
      get
      {
        return (string)GetValue(LabelProperty);
      }
      set
      {
        SetValue(LabelProperty, value);
      }
    }


    int IGUIControl.ID
    {
      get { return _controlId; }
    }

    int IGUIControl.OnUp
    {
      get { return _onUp; }
    }

    int IGUIControl.OnDown
    {
      get { return _onDown; }
    }

    int IGUIControl.OnLeft
    {
      get { return _onLeft; }
    }

    int IGUIControl.OnRight
    {
      get { return _onRight; }
    }


    //
    // property PosY
    // 
    private double _positionY;

    public double PosY
    {
      get
      {
        return (double)GetValue(PosYProperty);
      }
      set
      {
        SetValue(PosYProperty, value);
      }
    }

    public static readonly DependencyProperty PosYProperty =DependencyProperty.Register("PosY", typeof(double), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosYChanged)));

    private static void OnPosYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnPosYChanged(args);
    }

    protected virtual void OnPosYChanged(DependencyPropertyChangedEventArgs args)
    {
      _positionY = (double)args.NewValue;
      Canvas.SetTop(this, _positionY);
    }

    //
    // property PosX
    // 
    private double _positionX;

    public double PosX
    {
      get
      {
        return (double)GetValue(PosXProperty);
      }
      set
      {
        SetValue(PosXProperty, value);
      }
    }

    public static readonly DependencyProperty PosXProperty =DependencyProperty.Register("PosX", typeof(double), typeof(GUIButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosXChanged)));

    private static void OnPosXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIButton control = (GUIButton)obj;
      control.OnPosXChanged(args);
    }

    protected virtual void OnPosXChanged(DependencyPropertyChangedEventArgs args)
    {
      _positionX = (double)args.NewValue;
      Canvas.SetLeft(this, _positionX);
    }
  }
}

