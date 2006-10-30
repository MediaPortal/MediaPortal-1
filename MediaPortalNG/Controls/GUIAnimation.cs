using System;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaPortal
{

  public class GUIAnimation : Image, IGUIControl
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
    private string _imagePath;
    private double _displayTime;
    private bool _restartAnimation;
    private ObjectAnimationUsingKeyFrames _animation;
    private Storyboard _storyboard;

    private void SetAnimationDir(string imagePath)
    {

      string dir = Directory.GetCurrentDirectory() + @"\" + imagePath;

      if (Directory.Exists(dir) == false)
        return;

      _imagePath = imagePath;
      DirectoryInfo di = new DirectoryInfo(dir);
      FileInfo[] files = di.GetFiles("*.png");
      double nextTime = 0;

      _animation = new ObjectAnimationUsingKeyFrames();

      for (int i = 1; i <= files.Length; i++)
      {
        string file = dir + @"\" + i.ToString() + ".png";
        if (File.Exists(file) == true)
        {
          BitmapSource bmp = new BitmapImage(new Uri(file));
          KeyTime t = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(nextTime));
          DiscreteObjectKeyFrame bmpKeyFrame = new DiscreteObjectKeyFrame(bmp, t);
          nextTime += _displayTime;
          _animation.KeyFrames.Add(bmpKeyFrame);
        }
      }
      _storyboard = new Storyboard();
      _storyboard.Children.Add(_animation);
      Storyboard.SetTargetProperty(_animation, new PropertyPath("Source"));
    }



    public GUIAnimation()
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

      _displayTime = 100;
      _restartAnimation = false;
      this.Loaded += new RoutedEventHandler(GUIAnimation_Loaded);
    }

    void GUIAnimation_Loaded(object sender, RoutedEventArgs e)
    {
      if (_storyboard == null) return;

      if (Repeat == true)
        _storyboard.RepeatBehavior = RepeatBehavior.Forever;

      base.BeginStoryboard(_storyboard);

    }



    #region Properties

    //
    // image folder stuff - sets the base-folder that contains the images for these
    // animation
    //

    public string ImageFolder
    {
      get
      {
        return (string)GetValue(ImageFolderProperty);
      }
      set
      {
        SetValue(ImageFolderProperty, value);
      }
    }

    public static readonly DependencyProperty ImageFolderProperty =
    DependencyProperty.Register("ImageFolder", typeof(string), typeof(GUIAnimation),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnImageFolderChanged)));

    private static void OnImageFolderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;

      RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>(
          (string)args.OldValue, (string)args.NewValue, ImageFolderChangedEvent);
      control.OnImageFolderChanged(e);
    }

    public static readonly RoutedEvent ImageFolderChangedEvent = EventManager.RegisterRoutedEvent(
"ImageFolderChanged", RoutingStrategy.Bubble,
typeof(RoutedPropertyChangedEventHandler<string>), typeof(GUIAnimation));

    public event RoutedPropertyChangedEventHandler<string> ImageFolderChanged
    {
      add { AddHandler(ImageFolderChangedEvent, value); }
      remove { RemoveHandler(ImageFolderChangedEvent, value); }
    }

    protected virtual void OnImageFolderChanged(RoutedPropertyChangedEventArgs<string> args)
    {
      SetAnimationDir(args.NewValue);
      RaiseEvent(args);
    }

    //
    // repeat stuff
    // 
    //

    public bool Repeat
    {
      get
      {
        return (bool)GetValue(RepeatProperty);
      }
      set
      {
        SetValue(RepeatProperty, value);
      }
    }

    public static readonly DependencyProperty RepeatProperty =
    DependencyProperty.Register("Repeat", typeof(bool), typeof(GUIAnimation),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRepeatChanged)));

    private static void OnRepeatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;

      RoutedPropertyChangedEventArgs<bool> e = new RoutedPropertyChangedEventArgs<bool>(
          (bool)args.OldValue, (bool)args.NewValue, RepeatChangedEvent);
      control.OnRepeatChanged(e);
    }

    public static readonly RoutedEvent RepeatChangedEvent = EventManager.RegisterRoutedEvent(
"RepeatChanged", RoutingStrategy.Bubble,
typeof(RoutedPropertyChangedEventHandler<bool>), typeof(GUIAnimation));

    public event RoutedPropertyChangedEventHandler<bool> RepeatChanged
    {
      add { AddHandler(RepeatChangedEvent, value); }
      remove { RemoveHandler(RepeatChangedEvent, value); }
    }

    protected virtual void OnRepeatChanged(RoutedPropertyChangedEventArgs<bool> args)
    {
      _restartAnimation = args.NewValue;
      SetAnimationDir(_imagePath);
      RaiseEvent(args);
    }

    //
    // frame time stuff - sets the time an frame is visible (in milliseconds)
    // 
    //

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

    public static readonly DependencyProperty FrameTimeProperty =
    DependencyProperty.Register("FrameTime", typeof(double), typeof(GUIAnimation),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFrameTimeChanged)));

    private static void OnFrameTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;

      RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
          (double)args.OldValue, (double)args.NewValue, FrameTimeChangedEvent);
      control.OnFrameTimeChanged(e);
    }

    public static readonly RoutedEvent FrameTimeChangedEvent = EventManager.RegisterRoutedEvent(
"FrameTimeChanged", RoutingStrategy.Bubble,
typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIAnimation));

    public event RoutedPropertyChangedEventHandler<double> FrameTimeChanged
    {
      add { AddHandler(FrameTimeChangedEvent, value); }
      remove { RemoveHandler(FrameTimeChangedEvent, value); }
    }

    protected virtual void OnFrameTimeChanged(RoutedPropertyChangedEventArgs<double> args)
    {
      _displayTime = args.NewValue;
      SetAnimationDir(_imagePath);
      RaiseEvent(args);
    }

    #endregion

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

    public static readonly DependencyProperty IDProperty =DependencyProperty.Register("ID", typeof(int), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIDChanged)));

    private static void OnIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;

      RoutedPropertyChangedEventArgs<int> e = new RoutedPropertyChangedEventArgs<int>(
          (int)args.OldValue, (int)args.NewValue, IDChangedEvent);
      control.OnIDChanged(e);
    }

    public static readonly RoutedEvent IDChangedEvent = EventManager.RegisterRoutedEvent("IDChanged", RoutingStrategy.Bubble,typeof(RoutedPropertyChangedEventHandler<int>), typeof(GUIAnimation));

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

    public static readonly DependencyProperty OnUpProperty =DependencyProperty.Register("OnUp", typeof(int), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnUpChanged)));

    private static void OnOnUpChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
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

    public static readonly DependencyProperty OnDownProperty =DependencyProperty.Register("OnDown", typeof(int), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnDownChanged)));

    private static void OnOnDownChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
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

    public static readonly DependencyProperty OnLeftProperty =DependencyProperty.Register("OnLeft", typeof(int), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnLeftChanged)));

    private static void OnOnLeftChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
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

    public static readonly DependencyProperty OnRightProperty =DependencyProperty.Register("OnRight", typeof(int), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnRightChanged)));

    private static void OnOnRightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
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

    public static readonly DependencyProperty LabelProperty =DependencyProperty.Register("Label", typeof(string), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLabelChanged)));

    private static void OnLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
      control.OnLabelChanged(args);
    }

    protected virtual void OnLabelChanged(DependencyPropertyChangedEventArgs args)
    {
      _label = (string)args.NewValue;
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

    public static readonly DependencyProperty HyperlinkProperty =DependencyProperty.Register("Hyperlink", typeof(string), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHyperlinkChanged)));

    private static void OnHyperlinkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
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

    public static readonly DependencyProperty TextureProperty =DependencyProperty.Register("Texture", typeof(string), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTextureChanged)));

    private static void OnTextureChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
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

    public static readonly DependencyProperty AlignProperty =DependencyProperty.Register("Align", typeof(TextAlignment), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAlignChanged)));

    private static void OnAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
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

    public static readonly DependencyProperty DisabledColorProperty =DependencyProperty.Register("DisabledColor", typeof(Brush), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisabledColorChanged)));

    private static void OnDisabledColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
      control.OnDisabledColorChanged(args);
    }

    protected virtual void OnDisabledColorChanged(DependencyPropertyChangedEventArgs args)
    {
      _disabledColor = (Brush)args.NewValue;
    }


    #region IGUIControl Members


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

    #endregion

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

    public static readonly DependencyProperty PosYProperty =DependencyProperty.Register("PosY", typeof(double), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosYChanged)));

    private static void OnPosYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
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

    public static readonly DependencyProperty PosXProperty =DependencyProperty.Register("PosX", typeof(double), typeof(GUIAnimation),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosXChanged)));

    private static void OnPosXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIAnimation control = (GUIAnimation)obj;
      control.OnPosXChanged(args);
    }

    protected virtual void OnPosXChanged(DependencyPropertyChangedEventArgs args)
    {
      _positionX = (double)args.NewValue;
      Canvas.SetLeft(this, _positionX);
    }
  }
}

