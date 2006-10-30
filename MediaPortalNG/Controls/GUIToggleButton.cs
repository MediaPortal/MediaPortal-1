using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace MediaPortal
{

  public class GUIToggleButton : CheckBox, IGUIControl
  {
    private string _label;
    private string _texture;
    private string _hyperLink;
    private int _onUp;
    private int _onDown;
    private int _onLeft;
    private int _onRight;
    private int _controlId;
    private string _text;
    private ScrollViewer _scrollViewer;
    private double _displayTime;
    private double _scrollPosition;
    private Storyboard _storyBoard;
    private double _scrollAreaWidth;
    private TextAlignment _align;

    // getting & removing the handler we will call on setting the Click property

    public GUIToggleButton()
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

      this.MouseEnter += new MouseEventHandler(GUIToggleButton_MouseEnter);
      this.MouseLeave += new MouseEventHandler(GUIToggleButton_MouseLeave);
      // start all actions after load is complete
      this.Loaded += new RoutedEventHandler(GUIToggleButton_Loaded);
      this.Unloaded += new RoutedEventHandler(GUIToggleButton_Unloaded);
      // default frame time
      FrameTime = 80;

    }

    protected override void OnToggle()
    {
      base.OnToggle();

    }

    protected override void OnClick()
    {
      base.OnClick();
      Core.OnClick(this);
    }



    void GUIToggleButton_MouseLeave(object sender, MouseEventArgs e)
    {
      AnimateEnd();
    }

    void GUIToggleButton_MouseEnter(object sender, MouseEventArgs e)
    {
      AnimateStart();
    }

    // no setting of an style is allowed


    void GUIToggleButton_Unloaded(object sender, RoutedEventArgs e)
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



    void GUIToggleButton_Loaded(object sender, RoutedEventArgs e)
    {
      Border b = (Border)VisualTreeHelper.GetChild(this, 0);
      _scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(b, 0);

      // prevent to select the scrollviewer
      _scrollViewer.IsEnabled = false;
      _scrollViewer.Focusable = false;

      if (_scrollViewer == null)
        return;
      _scrollViewer.ScrollToHorizontalOffset(0);
      _scrollPosition = 0;
      ScrollAreaWidth = Width - 25;
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

    public static readonly DependencyProperty FrameTimeProperty =DependencyProperty.Register("FrameTime", typeof(double), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFrameTimeChanged)));

    private static void OnFrameTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;

      RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
          (double)args.OldValue, (double)args.NewValue, FrameTimeChangedEvent);
      control.OnFrameTimeChanged(e);
    }

    public static readonly RoutedEvent FrameTimeChangedEvent = EventManager.RegisterRoutedEvent("FrameTimeChanged", RoutingStrategy.Bubble,typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIToggleButton));

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

    protected static readonly DependencyProperty ScrollPositionProperty =DependencyProperty.Register("ScrollPosition", typeof(double), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScrollPositionChanged)));

    private static void OnScrollPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;

      RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>((double)args.OldValue, (double)args.NewValue, ScrollPositionChangedEvent);
      control.OnScrollPositionChanged(e);
    }

    protected static readonly RoutedEvent ScrollPositionChangedEvent = EventManager.RegisterRoutedEvent("ScrollPositionChanged", RoutingStrategy.Bubble,typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIToggleButton));

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


    public string Text
    {
      get
      {
        return _text;
      }
      set
      {
        _text = value;
      }
    }

    public static readonly DependencyProperty TextProperty =DependencyProperty.Register("Text", typeof(string), typeof(GUIToggleButton), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));

    protected event RoutedPropertyChangedEventHandler<string> TextChanged
    {
      add
      {
        AddHandler(TextChangedEvent, value);
      }
      remove
      {
        RemoveHandler(TextChangedEvent, value);
      }
    }

    public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble,typeof(RoutedPropertyChangedEventHandler<string>), typeof(GUIToggleButton));

    private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;

      RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>((string)args.OldValue, (string)args.NewValue, TextChangedEvent);
      control.OnTextChanged(e);
    }

    protected virtual void OnTextChanged(RoutedPropertyChangedEventArgs<string> args)
    {
      Text = args.NewValue;
      RaiseEvent(args);
    }

    // scroll width
    public double ScrollAreaWidth
    {
      get
      {
        return _scrollAreaWidth;
      }
      set
      {
        SetValue(ScrollAreaWidthProperty, value);
      }
    }

    public static readonly DependencyProperty ScrollAreaWidthProperty =DependencyProperty.Register("ScrollAreaWidth", typeof(double), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScrollAreaWidthChanged)));

    private static void OnScrollAreaWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
      control.OnScrollAreaWidthChanged(args);
    }

    protected virtual void OnScrollAreaWidthChanged(DependencyPropertyChangedEventArgs args)
    {

      _scrollAreaWidth = (double)args.NewValue;
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

    public static readonly DependencyProperty IDProperty =DependencyProperty.Register("ID", typeof(int), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIDChanged)));

    private static void OnIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;

      RoutedPropertyChangedEventArgs<int> e = new RoutedPropertyChangedEventArgs<int>(
          (int)args.OldValue, (int)args.NewValue, IDChangedEvent);
      control.OnIDChanged(e);
    }

    public static readonly RoutedEvent IDChangedEvent = EventManager.RegisterRoutedEvent("IDChanged", RoutingStrategy.Bubble,typeof(RoutedPropertyChangedEventHandler<int>), typeof(GUIToggleButton));

    public event RoutedPropertyChangedEventHandler<int> IDChanged
    {
      add { AddHandler(IDChangedEvent, value); }
      remove { RemoveHandler(IDChangedEvent, value); }
    }

    protected virtual void OnIDChanged(RoutedPropertyChangedEventArgs<int> args)
    {
      _controlId = args.NewValue;
      RaiseEvent(args);
    }

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

    public static readonly DependencyProperty OnUpProperty =DependencyProperty.Register("OnUp", typeof(int), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnUpChanged)));

    private static void OnOnUpChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
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

    public static readonly DependencyProperty OnDownProperty =DependencyProperty.Register("OnDown", typeof(int), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnDownChanged)));

    private static void OnOnDownChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
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

    public static readonly DependencyProperty OnLeftProperty =DependencyProperty.Register("OnLeft", typeof(int), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnLeftChanged)));

    private static void OnOnLeftChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
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

    public static readonly DependencyProperty OnRightProperty =DependencyProperty.Register("OnRight", typeof(int), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnRightChanged)));

    private static void OnOnRightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
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

    public static readonly DependencyProperty LabelProperty =DependencyProperty.Register("Label", typeof(string), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLabelChanged)));

    private static void OnLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
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

    public static readonly DependencyProperty HyperlinkProperty =DependencyProperty.Register("Hyperlink", typeof(string), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHyperlinkChanged)));

    private static void OnHyperlinkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
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

    public static readonly DependencyProperty TextureProperty =DependencyProperty.Register("Texture", typeof(string), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTextureChanged)));

    private static void OnTextureChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
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

    public static readonly DependencyProperty AlignProperty =DependencyProperty.Register("Align", typeof(TextAlignment), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAlignChanged)));

    private static void OnAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
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

    public static readonly DependencyProperty DisabledColorProperty =DependencyProperty.Register("DisabledColor", typeof(Brush), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisabledColorChanged)));

    private static void OnDisabledColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
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

    public static readonly DependencyProperty PosYProperty =DependencyProperty.Register("PosY", typeof(double), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosYChanged)));

    private static void OnPosYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
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

    public static readonly DependencyProperty PosXProperty =DependencyProperty.Register("PosX", typeof(double), typeof(GUIToggleButton),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosXChanged)));

    private static void OnPosXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIToggleButton control = (GUIToggleButton)obj;
      control.OnPosXChanged(args);
    }

    protected virtual void OnPosXChanged(DependencyPropertyChangedEventArgs args)
    {
      _positionX = (double)args.NewValue;
      Canvas.SetLeft(this, _positionX);
    }
  }
}

