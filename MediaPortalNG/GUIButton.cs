using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MediaPortal;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace MediaPortal
{
    public class GUIButton : UserControl
    {
        private ScrollViewer _scrollViewer;
        private double _displayTime;
        private double _scrollPosition;
        private Storyboard _storyBoard;
        private Delegate _delegate;

        // getting & removing the handler we will call on setting the Click property
        public event RoutedEventHandler Click
        {
            add 
            { 
                _delegate = value;
            }
            remove 
            {
                _delegate = null;
            }
        }
 
        
        public GUIButton()
        {

            this.MouseEnter += new MouseEventHandler(GUIButton_MouseEnter);
            this.MouseLeave += new MouseEventHandler(GUIButton_MouseLeave);
            // start all actions after load is complete
            this.Loaded += new RoutedEventHandler(GUIButton_Loaded);
            this.Unloaded += new RoutedEventHandler(GUIButton_Unloaded);
           // default frame time
            FrameTime = 80;
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
        new public Style Style
        {
            get { return this.Style; }
        }


        void GUIButton_Click(object sender, RoutedEventArgs e)
        {
            if (_delegate != null)
            {
                _delegate.DynamicInvoke(sender,e);
            }
        }
 

   
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
            DoubleAnimation positionAnimation = new DoubleAnimation(0, _scrollViewer.ScrollableWidth, new Duration(TimeSpan.FromMilliseconds(_displayTime*_scrollViewer.ScrollableWidth)));
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
            _scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(b, 0);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(GUIButton_Click);
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
        DependencyProperty.Register("FrameTime", typeof(double), typeof(GUIButton),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFrameTimeChanged)));

        private static void OnFrameTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIButton control = (GUIButton)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, FrameTimeChangedEvent);
            control.OnFrameTimeChanged(e);
        }

        public static readonly RoutedEvent FrameTimeChangedEvent = EventManager.RegisterRoutedEvent(
    "FrameTimeChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIButton));

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
        DependencyProperty.Register("ScrollPosition", typeof(double), typeof(GUIButton),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScrollPositionChanged)));

        private static void OnScrollPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIButton control = (GUIButton)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, ScrollPositionChangedEvent);
            control.OnScrollPositionChanged(e);
        }

        protected static readonly RoutedEvent ScrollPositionChangedEvent = EventManager.RegisterRoutedEvent(
    "ScrollPositionChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUIButton));

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

        // the property to animate the scrolling

        public string Caption
        {
            get
            {
                return (string)GetValue(CaptionProperty);
            }
            set
            {
                SetValue(CaptionProperty, value);
            }
        }

        public static readonly DependencyProperty CaptionProperty =
        DependencyProperty.Register("Caption", typeof(string), typeof(GUIButton),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCaptionChanged)));

        private static void OnCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIButton control = (GUIButton)obj;

            RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>(
                (string)args.OldValue, (string)args.NewValue, CaptionChangedEvent);
            control.OnCaptionChanged(e);
        }

        public static readonly RoutedEvent CaptionChangedEvent = EventManager.RegisterRoutedEvent(
    "CaptionChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<string>), typeof(GUIButton));

        public event RoutedPropertyChangedEventHandler<string> CaptionChanged
        {
            add { AddHandler(CaptionChangedEvent, value); }
            remove { RemoveHandler(CaptionChangedEvent, value); }
        }

        protected virtual void OnCaptionChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }




    }
}
