using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace MediaPortal
{

  public class GUIDateTime : TextBlock, IGUIControl
  {
    private TextAlignment _align;
    private string _Format;
    private string _label;
    private string _texture;
    private string _hyperLink;
    private int _onUp;
    private int _onDown;
    private int _onLeft;
    private int _onRight;
    private DateTime _dateTime;
    private DispatcherTimer _timer;
    private int _controlId;
    private double _posX;

    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);

      UpdateValue();

      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromMilliseconds(1000 -DateTime.Now.Millisecond);
      _timer.Tick += new EventHandler(TimerTick);
      _timer.Start();
    }



    public GUIDateTime()
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
      _Format = "";
      _dateTime = new DateTime();
      UpdateValue();
      this.Loaded += new RoutedEventHandler(GUIDateTime_Loaded);
    }

    void GUIDateTime_Loaded(object sender, RoutedEventArgs e)
    {
      _posX = 0;
    }

    void TimerTick(object sender, EventArgs e)
    {
      UpdateValue();
      _timer.Start();
    }

    private void UpdateValue()
    {

      _dateTime = DateTime.Now;
      string localizedDay = "";
      switch (_dateTime.DayOfWeek)
      {
        case DayOfWeek.Monday:
          localizedDay = Core.GetLocalizedString("11");
          break;
        case DayOfWeek.Tuesday:
          localizedDay = Core.GetLocalizedString("12");
          break;
        case DayOfWeek.Wednesday:
          localizedDay = Core.GetLocalizedString("13");
          break;
        case DayOfWeek.Thursday:
          localizedDay = Core.GetLocalizedString("14");
          break;
        case DayOfWeek.Friday:
          localizedDay = Core.GetLocalizedString("15");
          break;
        case DayOfWeek.Saturday:
          localizedDay = Core.GetLocalizedString("16");
          break;
        default:
          localizedDay = Core.GetLocalizedString("17");
          break;
      }
      string text = "";// _dateTime.DayOfWeek.ToString() + ", " + _dateTime.ToShortTimeString();
      if (_Format == "#date")
        text = localizedDay;
      if (_Format == "#time")
        text = _dateTime.ToShortTimeString();
      if (_Format == "")
        text = localizedDay + ", " + _dateTime.ToShortTimeString();

      if (Text != text)
        Text = text;
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

    public static readonly DependencyProperty IDProperty =DependencyProperty.Register("ID", typeof(int), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIDChanged)));

    private static void OnIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;

      RoutedPropertyChangedEventArgs<int> e = new RoutedPropertyChangedEventArgs<int>(
          (int)args.OldValue, (int)args.NewValue, IDChangedEvent);
      control.OnIDChanged(e);
    }

    public static readonly RoutedEvent IDChangedEvent = EventManager.RegisterRoutedEvent("IDChanged", RoutingStrategy.Bubble,typeof(RoutedPropertyChangedEventHandler<int>), typeof(GUIDateTime));

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

    public static readonly DependencyProperty OnUpProperty =DependencyProperty.Register("OnUp", typeof(int), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnUpChanged)));

    private static void OnOnUpChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
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

    public static readonly DependencyProperty OnDownProperty =DependencyProperty.Register("OnDown", typeof(int), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnDownChanged)));

    private static void OnOnDownChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
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

    public static readonly DependencyProperty OnLeftProperty =DependencyProperty.Register("OnLeft", typeof(int), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnLeftChanged)));

    private static void OnOnLeftChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
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

    public static readonly DependencyProperty OnRightProperty =DependencyProperty.Register("OnRight", typeof(int), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnRightChanged)));

    private static void OnOnRightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
      control.OnOnRightChanged(args);
    }

    protected virtual void OnOnRightChanged(DependencyPropertyChangedEventArgs args)
    {
      _onRight = (int)args.NewValue;
    }

    //
    // OnRight

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

    public static readonly DependencyProperty LabelProperty =DependencyProperty.Register("Label", typeof(string), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLabelChanged)));

    private static void OnLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
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

    public static readonly DependencyProperty HyperlinkProperty =DependencyProperty.Register("Hyperlink", typeof(string), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHyperlinkChanged)));

    private static void OnHyperlinkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
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

    public static readonly DependencyProperty TextureProperty =DependencyProperty.Register("Texture", typeof(string), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTextureChanged)));

    private static void OnTextureChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
      control.OnTextureChanged(args);
    }

    protected virtual void OnTextureChanged(DependencyPropertyChangedEventArgs args)
    {
      _texture = (string)args.NewValue;
    }

    // format
    public string Format
    {
      get
      {
        return _Format;
      }
      set
      {
        _Format = value;
      }
    }

    public static readonly DependencyProperty FormatProperty =DependencyProperty.Register("Format", typeof(string), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFormatChanged)));

    private static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
      control.OnFormatChanged(args);
    }

    protected virtual void OnFormatChanged(DependencyPropertyChangedEventArgs args)
    {
      _Format = (string)args.NewValue;
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

    public static readonly DependencyProperty AlignProperty =DependencyProperty.Register("Align", typeof(TextAlignment), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAlignChanged)));

    private static void OnAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
      control.OnAlignChanged(args);
    }

    protected virtual void OnAlignChanged(DependencyPropertyChangedEventArgs args)
    {
      _align = (TextAlignment)args.NewValue;
      TextAlignment = _align;
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

    public static readonly DependencyProperty DisabledColorProperty =DependencyProperty.Register("DisabledColor", typeof(Brush), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisabledColorChanged)));

    private static void OnDisabledColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
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

    public static readonly DependencyProperty PosYProperty =DependencyProperty.Register("PosY", typeof(double), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosYChanged)));

    private static void OnPosYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
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

    public static readonly DependencyProperty PosXProperty =DependencyProperty.Register("PosX", typeof(double), typeof(GUIDateTime),new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosXChanged)));

    private static void OnPosXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIDateTime control = (GUIDateTime)obj;
      control.OnPosXChanged(args);
    }

    protected virtual void OnPosXChanged(DependencyPropertyChangedEventArgs args)
    {
      _positionX = (double)args.NewValue;
      Canvas.SetLeft(this, _positionX);
    }
  }
}
