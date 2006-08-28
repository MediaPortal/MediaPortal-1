using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MediaPortal;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace MediaPortal
{

    public partial class GUIFadelabel : UserControl
    {
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

    }
}
