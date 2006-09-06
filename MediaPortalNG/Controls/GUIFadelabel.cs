using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace MediaPortal
{

    public class GUIFadelabel : UserControl,IGUIControl
    {
        private TextAlignment _Align;
        private string _Label;
        private string _Texture;
        private string _Hyperlink;
        private int _onUP;
        private int _onDOWN;
        private int _onLEFT;
        private int _onRIGHT;
        private int _controlID;
        private ScrollViewer _scrollViewer;
        private double _displayTime;
        private double _scrollPosition;
        private Storyboard _storyBoard;

        public GUIFadelabel()
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
            // start all actions after load is complete
            this.Loaded += new RoutedEventHandler(GUIFadelabel_Loaded);
            this.Unloaded += new RoutedEventHandler(GUIFadelabel_Unloaded);
            //
            FrameTime = 80;
            IsEnabled = false;
            Focusable = false;

        }
  

        void GUIFadelabel_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_storyBoard == null)
                return;
            AnimateEnd();
        }

        void AnimateStart()
        {
            DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(1000)));
            DoubleAnimation positionAnimation = new DoubleAnimation(0, _scrollViewer.ScrollableWidth+10, new Duration(TimeSpan.FromMilliseconds(_displayTime * _scrollViewer.ScrollableWidth)));
            _storyBoard = new Storyboard();
            _storyBoard.Children.Add(fadeInAnimation);
            _storyBoard.Children.Add(positionAnimation);
             Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));
             Storyboard.SetTargetProperty(positionAnimation, new PropertyPath("ScrollPosition"));
             _storyBoard.RepeatBehavior = RepeatBehavior.Forever;
            _storyBoard.Begin(this,HandoffBehavior.Compose, true);
            // anim scrollviewe 
        }

        void AnimateEnd()
        {
            // reset and restart
            if (_storyBoard == null) return;
            _storyBoard.Stop(this);
            _storyBoard.Children.Clear();
            _storyBoard = null;
            ScrollPosition = 0;
        }

        void GUIFadelabel_Loaded(object sender, RoutedEventArgs e)
        {
            // set object
            Border b = (Border)VisualTreeHelper.GetChild(this, 0);
            _scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(b, 0);

            //
            if (_scrollViewer == null) return;
            _scrollViewer.ScrollToHorizontalOffset(0);
            _scrollPosition = 0;
           AnimateStart();
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

        public static readonly DependencyProperty FrameTimeProperty =
        DependencyProperty.Register("FrameTime", typeof(double), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFrameTimeChanged)));

        private static void OnFrameTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, FrameTimeChangedEvent);
            control.OnFrameTimeChanged(e);
        }

        public static readonly RoutedEvent FrameTimeChangedEvent = EventManager.RegisterRoutedEvent(
    "FrameTimeChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIFadelabel));

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

        protected static readonly DependencyProperty ScrollPositionProperty =
        DependencyProperty.Register("ScrollPosition", typeof(double), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScrollPositionChanged)));

        private static void OnScrollPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, ScrollPositionChangedEvent);
            control.OnScrollPositionChanged(e);
        }

        protected static readonly RoutedEvent ScrollPositionChangedEvent = EventManager.RegisterRoutedEvent(
    "ScrollPositionChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIFadelabel));

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
        // control id
        public int ID
        {
            get
            {
                return _controlID;
            }
            set
            {
                _controlID = value;
            }
        }

        public static readonly DependencyProperty IDProperty =
        DependencyProperty.Register("ID", typeof(int), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIDChanged)));

        private static void OnIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;

            RoutedPropertyChangedEventArgs<int> e = new RoutedPropertyChangedEventArgs<int>(
                (int)args.OldValue, (int)args.NewValue, IDChangedEvent);
            control.OnIDChanged(e);
        }

        public static readonly RoutedEvent IDChangedEvent = EventManager.RegisterRoutedEvent(
    "IDChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<int>), typeof(GUIFadelabel));

        public event RoutedPropertyChangedEventHandler<int> IDChanged
        {
            add { AddHandler(IDChangedEvent, value); }
            remove { RemoveHandler(IDChangedEvent, value); }
        }

        protected virtual void OnIDChanged(RoutedPropertyChangedEventArgs<int> args)
        {
            _controlID = args.NewValue;
            RaiseEvent(args);
        }

        public int OnUP
        {
            get
            {
                return _onUP;
            }
            set
            {
                _onUP = value;
            }
        }

        public static readonly DependencyProperty OnUPProperty =
        DependencyProperty.Register("OnUP", typeof(int), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnUPChanged)));

        private static void OnOnUPChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnOnUPChanged(args);
        }


        protected virtual void OnOnUPChanged(DependencyPropertyChangedEventArgs args)
        {
            _onUP = (int)args.NewValue;
        }


        // ondown

        public int OnDOWN
        {
            get
            {
                return _onDOWN;
            }
            set
            {
                _onDOWN = value;
            }
        }

        public static readonly DependencyProperty OnDOWNProperty =
        DependencyProperty.Register("OnDOWN", typeof(int), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnDOWNChanged)));

        private static void OnOnDOWNChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnOnDOWNChanged(args);
        }


        protected virtual void OnOnDOWNChanged(DependencyPropertyChangedEventArgs args)
        {
            _onDOWN = (int)args.NewValue;
        }

        // onleft

        public int OnLEFT
        {
            get
            {
                return _onLEFT;
            }
            set
            {
                _onLEFT = value;
            }
        }

        public static readonly DependencyProperty OnLEFTProperty =
        DependencyProperty.Register("OnLEFT", typeof(int), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnLEFTChanged)));

        private static void OnOnLEFTChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnOnLEFTChanged(args);
        }


        protected virtual void OnOnLEFTChanged(DependencyPropertyChangedEventArgs args)
        {
            _onLEFT = (int)args.NewValue;
        }

        // onright

        public int OnRIGHT
        {
            get
            {
                return _onRIGHT;
            }
            set
            {
                _onRIGHT = value;
            }
        }

        public static readonly DependencyProperty OnRIGHTProperty =
        DependencyProperty.Register("OnRIGHT", typeof(int), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnRIGHTChanged)));

        private static void OnOnRIGHTChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnOnRIGHTChanged(args);
        }


        protected virtual void OnOnRIGHTChanged(DependencyPropertyChangedEventArgs args)
        {
            _onRIGHT = (int)args.NewValue;
        }

        public string Label
        {
            get
            {
                return _Label;
            }
            set
            {
                _Label = value;
            }
        }

        public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register("Label", typeof(string), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLabelChanged)));

        private static void OnLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnLabelChanged(args);
        }

        protected virtual void OnLabelChanged(DependencyPropertyChangedEventArgs args)
        {
            _Label = (string)args.NewValue;
        }

        // hyperlink
        public string Hyperlink
        {
            get
            {
                return _Hyperlink;
            }
            set
            {
                _Hyperlink = value;
            }
        }

        public static readonly DependencyProperty HyperlinkProperty =
        DependencyProperty.Register("Hyperlink", typeof(string), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHyperlinkChanged)));

        private static void OnHyperlinkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnHyperlinkChanged(args);
        }

        protected virtual void OnHyperlinkChanged(DependencyPropertyChangedEventArgs args)
        {
            _Hyperlink = (string)args.NewValue;
        }

        // Texture
        public string Texture
        {
            get
            {
                return _Texture;
            }
            set
            {
                _Texture = value;
            }
        }

        public static readonly DependencyProperty TextureProperty =
        DependencyProperty.Register("Texture", typeof(string), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTextureChanged)));

        private static void OnTextureChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnTextureChanged(args);
        }

        protected virtual void OnTextureChanged(DependencyPropertyChangedEventArgs args)
        {
            _Texture = (string)args.NewValue;
        }

        public TextAlignment Align
        {
            get
            {
                return _Align;
            }
            set
            {
                _Align = value;
            }
        }

        public static readonly DependencyProperty AlignProperty =
        DependencyProperty.Register("Align", typeof(TextAlignment), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAlignChanged)));

        private static void OnAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnAlignChanged(args);
        }

        protected virtual void OnAlignChanged(DependencyPropertyChangedEventArgs args)
        {
            _Align = (TextAlignment)args.NewValue;
        }
        // disabled color
        private Brush _DisabledColor;

        public Brush DisabledColor
        {
            get
            {
                return _DisabledColor;
            }
            set
            {
                _DisabledColor = value;
            }
        }

        public static readonly DependencyProperty DisabledColorProperty =
        DependencyProperty.Register("DisabledColor", typeof(Brush), typeof(GUIFadelabel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisabledColorChanged)));

        private static void OnDisabledColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnDisabledColorChanged(args);
        }

        protected virtual void OnDisabledColorChanged(DependencyPropertyChangedEventArgs args)
        {
            _DisabledColor = (Brush)args.NewValue;
        }


        
       protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (_Align == TextAlignment.Right)
                Canvas.SetLeft(this, _PosX - sizeInfo.NewSize.Width);
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
            get { return _controlID; }
        }

        int IGUIControl.OnUP
        {
            get { return _onUP; }
        }

        int IGUIControl.OnDOWN
        {
            get { return _onDOWN; }
        }

        int IGUIControl.OnLEFT
        {
            get { return _onLEFT; }
        }

        int IGUIControl.OnRIGHT
        {
            get { return _onRIGHT; }
        }

           
        //
        // property PosY
        // 
        private double _PosY;

        public double PosY
        {
            get
            {
                return (double)GetValue(PosYProperty);
            }
            set
            {
                SetValue(PosYProperty,value);
            }
        }

        public static readonly DependencyProperty PosYProperty =
        DependencyProperty.Register("PosY", typeof(double), typeof(GUIFadelabel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosYChanged)));

        private static void OnPosYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnPosYChanged(args);
        }

        protected virtual void OnPosYChanged(DependencyPropertyChangedEventArgs args)
        {
            _PosY=(double)args.NewValue;
            Canvas.SetTop(this, _PosY);
        }        
        //
        // property PosX
        // 
        private double _PosX;

        public double PosX
        {
            get
            {
                return (double)GetValue(PosXProperty);
            }
            set
            {
                SetValue(PosXProperty,value);
            }
        }

        public static readonly DependencyProperty PosXProperty =
        DependencyProperty.Register("PosX", typeof(double), typeof(GUIFadelabel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosXChanged)));

        private static void OnPosXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIFadelabel control = (GUIFadelabel)obj;
            control.OnPosXChanged(args);
        }

        protected virtual void OnPosXChanged(DependencyPropertyChangedEventArgs args)
        {
            _PosX=(double)args.NewValue;
            Canvas.SetLeft(this, _PosX);
        }
}
}

