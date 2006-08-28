using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaPortal;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media;


namespace MediaPortal
{

    public partial class GUIToggleButton : CheckBox
    {
        private string _text;
        private ScrollViewer _scrollViewer;
        private double _displayTime;
        private double _scrollPosition;
        private Storyboard _storyBoard;
        private double _scrollAreaWidth;

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
            DoubleAnimation positionAnimation = new DoubleAnimation(0, _scrollViewer.ScrollableWidth, new Duration(TimeSpan.FromMilliseconds(_displayTime*_scrollViewer.ScrollableWidth)));
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
        DependencyProperty.Register("FrameTime", typeof(double), typeof(GUIToggleButton),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFrameTimeChanged)));

        private static void OnFrameTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIToggleButton control = (GUIToggleButton)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, FrameTimeChangedEvent);
            control.OnFrameTimeChanged(e);
        }

        public static readonly RoutedEvent FrameTimeChangedEvent = EventManager.RegisterRoutedEvent(
    "FrameTimeChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIToggleButton));

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
        DependencyProperty.Register("ScrollPosition", typeof(double), typeof(GUIToggleButton),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScrollPositionChanged)));

        private static void OnScrollPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIToggleButton control = (GUIToggleButton)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, ScrollPositionChangedEvent);
            control.OnScrollPositionChanged(e);
        }

        protected static readonly RoutedEvent ScrollPositionChangedEvent = EventManager.RegisterRoutedEvent(
    "ScrollPositionChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIToggleButton));

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

        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(GUIToggleButton), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));

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

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(
        "TextChanged", RoutingStrategy.Bubble,
        typeof(RoutedPropertyChangedEventHandler<string>), typeof(GUIToggleButton));

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIToggleButton control = (GUIToggleButton)obj;

            RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>(
                (string)args.OldValue, (string)args.NewValue, TextChangedEvent);
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
                _scrollAreaWidth= value;
            }
        }

        public static readonly DependencyProperty ScrollAreaWidthProperty =
        DependencyProperty.Register("ScrollAreaWidth", typeof(double), typeof(GUIToggleButton),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScrollAreaWidthChanged)));

        private static void OnScrollAreaWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIToggleButton control = (GUIToggleButton)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, ScrollAreaWidthChangedEvent);
            control.OnScrollAreaWidthChanged(e);
        }

        public static readonly RoutedEvent ScrollAreaWidthChangedEvent = EventManager.RegisterRoutedEvent(
    "ScrollAreaWidthChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIToggleButton));

        public event RoutedPropertyChangedEventHandler<double> ScrollAreaWidthChanged
        {
            add { AddHandler(ScrollAreaWidthChangedEvent, value); }
            remove { RemoveHandler(ScrollAreaWidthChangedEvent, value); }
        }

        protected virtual void OnScrollAreaWidthChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            
            _scrollAreaWidth = args.NewValue;
            RaiseEvent(args);
        }


    }
}
