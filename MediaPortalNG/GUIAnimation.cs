using System;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;

using MediaPortal;
using System.Windows.Media.Imaging;

namespace MediaPortal
{

    public partial class GUIAnimation : Image
    {
        private string _imagePath;
        private double _displayTime;
        private bool _restartAnimation;
        private ObjectAnimationUsingKeyFrames _animation;
        private Storyboard _storyboard;

        private void SetAnimationDir(string imagePath)
        {
            string dir=Directory.GetCurrentDirectory()+@"\"+imagePath;

            if (Directory.Exists(dir) == false)
                return;

            _imagePath = imagePath;
            DirectoryInfo di = new DirectoryInfo(dir);
            FileInfo[] files = di.GetFiles("*.png");
            double nextTime=0;

            _animation = new ObjectAnimationUsingKeyFrames();

            for (int i = 1; i <= files.Length;i++)
            {
               string file=dir+@"\"+i.ToString()+".png";
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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (Repeat == true)
                _storyboard.RepeatBehavior = RepeatBehavior.Forever;

            base.BeginStoryboard(_storyboard);

        }

  
        public GUIAnimation()
        {
            _displayTime = 100;
            _restartAnimation = false;
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
            _restartAnimation=args.NewValue;
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


    }
}