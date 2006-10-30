using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace MediaPortal
{

  public class GUITextboxScrollUp : UserControl, IGUIControl
  {
    private TextAlignment _align;
    private string _label;
    private string _texture;
    private string _hyperLink;
    private int _onUp;
    private int _onDown;
    private int _onLeft;
    private int _onRight;
    private int _controlId;
    private ScrollViewer _scrollViewer;
    private double _displayTime;
    private double _scrollPosition;
    private Storyboard _storyBoard;

    public GUITextboxScrollUp()
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
      this.Opacity = 0;

      // start all actions after load is complete
      this.Loaded += new RoutedEventHandler(GUITextboxScrollUp_Loaded);
      this.Unloaded += new RoutedEventHandler(GUITextboxScrollUp_Unloaded);

    }

    void GUITextboxScrollUp_Unloaded(object sender, RoutedEventArgs e)
    {
      if (_storyBoard == null)
        return;
      _storyBoard.Stop(this);
      this.Opacity = 1;
      _scrollViewer.ScrollToVerticalOffset(0);
      _scrollPosition = 0;
    }

    void AnimateOpacity()
    {
      DoubleAnimation opacityAnimation = new DoubleAnimation(0f, 1.0f, new Duration(TimeSpan.FromMilliseconds(2000)));
      _storyBoard = new Storyboard();
      _storyBoard.Children.Add(opacityAnimation);
      Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
      _storyBoard.Completed += new EventHandler(opacityAnimation_Completed);
      _storyBoard.Begin(this, false);
    }

    void opacityAnimation_Completed(object sender, EventArgs e)
    {
      DoubleAnimationUsingKeyFrames positionAnimation = new DoubleAnimationUsingKeyFrames();

      double nextTime = 0;
      for (double val = 0; val < _scrollViewer.ExtentHeight; val += 1.0f)
      {
        KeyTime t = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(nextTime));
        DiscreteDoubleKeyFrame dblKeyFrame = new DiscreteDoubleKeyFrame(val, t);
        nextTime += _displayTime;
        positionAnimation.KeyFrames.Add(dblKeyFrame);
      }
      _storyBoard.Stop(this);
      _storyBoard.Completed -= new EventHandler(opacityAnimation_Completed);
      _storyBoard.Children.Clear();
      _storyBoard.Children.Add(positionAnimation);

      Storyboard.SetTargetProperty(positionAnimation, new PropertyPath("ScrollPosition"));
      _storyBoard.Completed += new EventHandler(positionAnimation_Completed);
      _storyBoard.Begin(this, true);
      // anim scrollviewe 
    }

    void positionAnimation_Completed(object sender, EventArgs e)
    {
      // reset and restart
      _storyBoard.Completed -= new EventHandler(positionAnimation_Completed);
      _storyBoard.Stop(this);
      _storyBoard.Children.Clear();
      _storyBoard = null;
      _scrollViewer.ScrollToVerticalOffset(0);
      _scrollPosition = 0;
      Opacity = 0.0f;
      AnimateOpacity();
    }

    void GUITextboxScrollUp_Loaded(object sender, RoutedEventArgs e)
    {
      Border b = (Border)VisualTreeHelper.GetChild(this, 0);
      _scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(b, 0);
      if (_scrollViewer == null)
        return;
      this.Opacity = 0;
      _scrollViewer.ScrollToVerticalOffset(0);
      _scrollPosition = 0;
      AnimateOpacity();
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

    public static readonly DependencyProperty FrameTimeProperty = DependencyProperty.Register("FrameTime", typeof(double), typeof(GUITextboxScrollUp), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFrameTimeChanged)));

    private static void OnFrameTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;

      RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>((double)args.OldValue, (double)args.NewValue, FrameTimeChangedEvent);
      control.OnFrameTimeChanged(e);
    }

    public static readonly RoutedEvent FrameTimeChangedEvent = EventManager.RegisterRoutedEvent("FrameTimeChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUITextboxScrollUp));

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
        return (double)GetValue(FrameTimeProperty);
      }
      set
      {
        SetValue(ScrollPositionProperty, value);
      }
    }

    protected static readonly DependencyProperty ScrollPositionProperty = DependencyProperty.Register("ScrollPosition", typeof(double), typeof(GUITextboxScrollUp), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScrollPositionChanged)));

    private static void OnScrollPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;

      RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>((double)args.OldValue, (double)args.NewValue, ScrollPositionChangedEvent);
      control.OnScrollPositionChanged(e);
    }

    protected static readonly RoutedEvent ScrollPositionChangedEvent = EventManager.RegisterRoutedEvent("ScrollPositionChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUITextboxScrollUp));

    protected event RoutedPropertyChangedEventHandler<double> ScrollPositionChanged
    {
      add { AddHandler(ScrollPositionChangedEvent, value); }
      remove { RemoveHandler(ScrollPositionChangedEvent, value); }
    }

    public virtual void OnScrollPositionChanged(RoutedPropertyChangedEventArgs<double> args)
    {
      _scrollPosition = args.NewValue;
      _scrollViewer.ScrollToVerticalOffset(_scrollPosition);
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

    public static readonly DependencyProperty IDProperty =DependencyProperty.Register("ID", typeof(int), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIDChanged)));

    private static void OnIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;

      RoutedPropertyChangedEventArgs<int> e = new RoutedPropertyChangedEventArgs<int>(
          (int)args.OldValue, (int)args.NewValue, IDChangedEvent);
      control.OnIDChanged(e);
    }

    public static readonly RoutedEvent IDChangedEvent = EventManager.RegisterRoutedEvent("IDChanged", RoutingStrategy.Bubble,typeof(RoutedPropertyChangedEventHandler<int>), typeof(GUITextboxScrollUp));

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

    public static readonly DependencyProperty OnUpProperty =DependencyProperty.Register("OnUp", typeof(int), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnUpChanged)));

    private static void OnOnUpChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
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

    public static readonly DependencyProperty OnDownProperty =DependencyProperty.Register("OnDown", typeof(int), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnDownChanged)));

    private static void OnOnDownChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
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

    public static readonly DependencyProperty OnLeftProperty =DependencyProperty.Register("OnLeft", typeof(int), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnLeftChanged)));

    private static void OnOnLeftChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
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

    public static readonly DependencyProperty OnRightProperty =DependencyProperty.Register("OnRight", typeof(int), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnRightChanged)));

    private static void OnOnRightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
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
        return _label;
      }
      set
      {
        _label = value;
      }
    }

    public static readonly DependencyProperty LabelProperty =DependencyProperty.Register("Label", typeof(string), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLabelChanged)));

    private static void OnLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
      control.OnLabelChanged(args);
    }

    protected virtual void OnLabelChanged(DependencyPropertyChangedEventArgs args)
    {
      _label = (string)args.NewValue;
      Content = _label;
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

    public static readonly DependencyProperty HyperlinkProperty =DependencyProperty.Register("Hyperlink", typeof(string), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHyperlinkChanged)));

    private static void OnHyperlinkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
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

    public static readonly DependencyProperty TextureProperty =DependencyProperty.Register("Texture", typeof(string), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTextureChanged)));

    private static void OnTextureChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
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

    public static readonly DependencyProperty AlignProperty =DependencyProperty.Register("Align", typeof(TextAlignment), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAlignChanged)));

    private static void OnAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
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

    public static readonly DependencyProperty DisabledColorProperty =DependencyProperty.Register("DisabledColor", typeof(Brush), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisabledColorChanged)));

    private static void OnDisabledColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
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

    public static readonly DependencyProperty PosYProperty =DependencyProperty.Register("PosY", typeof(double), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosYChanged)));

    private static void OnPosYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
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

    public static readonly DependencyProperty PosXProperty =DependencyProperty.Register("PosX", typeof(double), typeof(GUITextboxScrollUp),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosXChanged)));

    private static void OnPosXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUITextboxScrollUp control = (GUITextboxScrollUp)obj;
      control.OnPosXChanged(args);
    }

    protected virtual void OnPosXChanged(DependencyPropertyChangedEventArgs args)
    {
      _positionX = (double)args.NewValue;
      Canvas.SetLeft(this, _positionX);
    }
  }
}

