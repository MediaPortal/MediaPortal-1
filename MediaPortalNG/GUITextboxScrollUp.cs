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

    public partial class GUITextboxScrollUp : UserControl
    {
        private ScrollViewer _scrollViewer;
        private double _displayTime;
        private double _scrollPosition;
        private Storyboard _storyBoard;

        public GUITextboxScrollUp()
        {
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

        public static readonly DependencyProperty FrameTimeProperty =
        DependencyProperty.Register("FrameTime", typeof(double), typeof(GUITextboxScrollUp),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFrameTimeChanged)));

        private static void OnFrameTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUITextboxScrollUp control = (GUITextboxScrollUp)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, FrameTimeChangedEvent);
            control.OnFrameTimeChanged(e);
        }

        public static readonly RoutedEvent FrameTimeChangedEvent = EventManager.RegisterRoutedEvent(
    "FrameTimeChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUITextboxScrollUp));

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

        protected static readonly DependencyProperty ScrollPositionProperty =
        DependencyProperty.Register("ScrollPosition", typeof(double), typeof(GUITextboxScrollUp),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScrollPositionChanged)));

        private static void OnScrollPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUITextboxScrollUp control = (GUITextboxScrollUp)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, ScrollPositionChangedEvent);
            control.OnScrollPositionChanged(e);
        }

        protected static readonly RoutedEvent ScrollPositionChangedEvent = EventManager.RegisterRoutedEvent(
    "ScrollPositionChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<double>), typeof(GUITextboxScrollUp));

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





    }
}