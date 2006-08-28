using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaPortal;
using System.Windows.Media.Imaging;

namespace MediaPortal
{

    public partial class GUICheckMark : CheckBox
    {
        private string _text;

        public GUICheckMark()
        {
        }

 
        void GUICheckMark_Loaded(object sender, RoutedEventArgs e)
        {
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
        DependencyProperty.Register("Text", typeof(string), typeof(GUICheckMark), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));

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
        typeof(RoutedPropertyChangedEventHandler<string>), typeof(GUICheckMark));

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUICheckMark control = (GUICheckMark)obj;

            RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>(
                (string)args.OldValue, (string)args.NewValue, TextChangedEvent);
            control.OnTextChanged(e);
        }

        protected virtual void OnTextChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            Text = args.NewValue;
            RaiseEvent(args);
        }

    }
}
