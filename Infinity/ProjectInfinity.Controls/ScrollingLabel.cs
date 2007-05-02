using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ProjectInfinity.Controls
{
  public class ScrollingLabel : System.Windows.Controls.Canvas
  {
    TextBlock _block1;
    TextBlock _block2;

    bool _started = false;
    Storyboard _board;
    DoubleAnimation _animation1;
    DoubleAnimation _animation2;


    public static readonly DependencyProperty TextStyleProperty = DependencyProperty.Register("TextStyle",
                                                                                            typeof(object),
                                                                                            typeof(ScrollingLabel),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (TextStylePropertyChanged)));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
                                                                                            typeof(object),
                                                                                            typeof(ScrollingLabel),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (TextPropertyChanged)));
    public static readonly DependencyProperty ScrollMaskProperty = DependencyProperty.Register("ScrollMask",
                                                                                            typeof(object),
                                                                                            typeof(ScrollingLabel),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (ScrollMaskPropertyChanged)));

    public static readonly DependencyProperty StartElementProperty = DependencyProperty.Register("StartElement",
                                                                                            typeof(object),
                                                                                            typeof(ScrollingLabel),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (StartElementPropertyChanged)));

    public static DependencyProperty ScrollSpeedProperty = DependencyProperty.Register("ScrollSpeed",
                                                                                        typeof(ScrollSpeed),
                                                                                        typeof(ScrollingLabel));

    private double slowScrollSpeed = 20;
    private double mediumScrollSpeed = 70;
    private double fastScrollSpeed = 120;
    public ScrollingLabel()
    {
      this.Loaded += new RoutedEventHandler(ScrollingLabel_Loaded);
      this.LayoutUpdated += new EventHandler(ScrollingLabel_LayoutUpdated);
    }

    void ScrollingLabel_LayoutUpdated(object sender, EventArgs e)
    {
      SetMask();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      SetMask();
    }
    void SetMask()
    {
      if (_block1 == null || _block2 == null || ScrollMask == null) return;
      double tickerSize = _block1.ActualWidth;
      if (tickerSize < this.ActualWidth)
      {
        this.OpacityMask = null;
      }
      else
      {
        this.OpacityMask = ScrollMask as Brush ;
      }
    }
    public void StartScrolling()
    {
      double tickerSize = _block1.ActualWidth;

      if (tickerSize < this.ActualWidth) return;
      _board = new Storyboard();
      _board.Children.Clear();
      tickerSize += 10;

      double speed = slowScrollSpeed;
      switch (ScrollSpeed)
      {
        case ScrollSpeed.Slow:
          speed = slowScrollSpeed;
          break;
        case ScrollSpeed.Medium:
          speed = mediumScrollSpeed;
          break;
        case ScrollSpeed.Fast:
          speed = fastScrollSpeed;
          break;
      }

      _animation1 = new DoubleAnimation(0, -1 * tickerSize, TimeSpan.FromSeconds(tickerSize / speed));
      _animation2 = new DoubleAnimation(tickerSize, 0, TimeSpan.FromSeconds(tickerSize / speed));

      _animation1.RepeatBehavior = RepeatBehavior.Forever;
      _animation2.RepeatBehavior = RepeatBehavior.Forever;
      Storyboard.SetTargetProperty(_animation1, new PropertyPath(Canvas.LeftProperty));
      Storyboard.SetTargetProperty(_animation2, new PropertyPath(Canvas.LeftProperty));


      Storyboard.SetTargetName(_animation1, _block1.Name);
      Storyboard.SetTargetName(_animation2, _block2.Name);

      _started = true;
      _board.Children.Add(_animation1);
      _board.Children.Add(_animation2);
      _board.Begin(this, true);
    }

    public void StopScrolling()
    {
      if (_started)
      {
        _board.Stop(this);
        _board.Remove(this);
        _board = new Storyboard();
        _started = false;
      }

    }
    void ScrollingLabel_Loaded(object sender, RoutedEventArgs e)
    {
      ClipToBounds = true;
      NameScope.SetNameScope(this, new NameScope());
      _block1 = new TextBlock();
      _block2 = new TextBlock();
      _block1.Name = "text1";
      _block2.Name = "text2";
      _block1.Background = this.Background;
      _block2.Background = this.Background;

      _block1.Text = Text as string;
      _block2.Text = Text as string;

      _block1.Style = TextStyle as Style;
      _block2.Style = TextStyle as Style;

      Canvas.SetLeft(_block1, 0);
      Canvas.SetLeft(_block2, 0);
      Canvas.SetTop(_block1, 0);
      Canvas.SetTop(_block2, 0);

      _board = new Storyboard();
      Children.Add(_block1);
      Children.Add(_block2);
      InvalidateVisual();

      this.RegisterName(_block1.Name, _block1);
      this.RegisterName(_block2.Name, _block2);
      SetMask();
    }


    private static void TextStylePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as ScrollingLabel).TextStyle = (e.NewValue as object);
    }

    public object TextStyle
    {
      get
      {
        return GetValue(TextStyleProperty) as object;
      }
      set
      {
        SetValue(TextStyleProperty, value);
        if (_block1 != null)
          _block1.Style = (Style)TextStyle;
        if (_block2 != null)
          _block2.Style = (Style)TextStyle;
        SetMask();
      }
    }

    private static void TextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as ScrollingLabel).Text = (e.NewValue as object);
    }

    public object Text
    {
      get
      {
        return GetValue(TextProperty) as object;
      }
      set
      {
        SetValue(TextProperty, value);
        if (_block1 != null)
          _block2.Text = value as string;
        if (_block1 != null)
          _block2.Text = value as string;
        SetMask();
      }
    }



    private static void ScrollMaskPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as ScrollingLabel).ScrollMask = (e.NewValue as object);
    }

    public object ScrollMask
    {
      get
      {
        return GetValue(ScrollMaskProperty) as object;
      }
      set
      {
        SetValue(ScrollMaskProperty, value);
        SetMask();
      }
    }



    private static void StartElementPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as ScrollingLabel).StartElement = (e.NewValue as object);
    }

    public object StartElement
    {
      get
      {
        return GetValue(StartElementProperty) as object;
      }
      set
      {
        SetValue(StartElementProperty, value);
        FrameworkElement element = value as FrameworkElement;
        if (element != null)
        {
          element.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(element_GotKeyboardFocus);
          element.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(element_LostKeyboardFocus);
        }
      }
    }

    void element_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      StopScrolling();
    }

    void element_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      StopScrolling();
      StartScrolling();
    }

    public ScrollSpeed ScrollSpeed
    {
      get
      {
        return (ScrollSpeed)GetValue(ScrollSpeedProperty);
      }
      set
      {
        SetValue(ScrollSpeedProperty, value);
      }
    }
  }
}
