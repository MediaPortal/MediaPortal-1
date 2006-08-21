using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaPortal;

namespace MediaPortal
{

    public partial class GUISpinControl : UserControl
    {
        private static RoutedCommand _increaseCommand;
        private static RoutedCommand _decreaseCommand;
        private static int MinValue = -100, MaxValue = 100;

        public void SetMinValue(int val)
        {
            MinValue = val;
            if(Value<val)
                Value = val;
        }

        public void SetMaxValue(int val)
        {
            MaxValue = val;
            if (Value > val)
                Value = val;
        }


 
        public GUISpinControl()
        {
            InitializeCommands();          
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


         public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(int), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(MinValue, new PropertyChangedCallback(OnValueChanged)));
       
        
        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;

            RoutedPropertyChangedEventArgs<int> e = new RoutedPropertyChangedEventArgs<int>(
                (int)args.OldValue, (int)args.NewValue, ValueChangedEvent);
            control.OnValueChanged(e);
        }

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            "ValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>), typeof(GUISpinControl));

        public event RoutedPropertyChangedEventHandler<int> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
 
        
        
        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<int> args)
        {
            RaiseEvent(args);
        }

 
        public static RoutedCommand IncreaseCommand
        {
            get
            {
                return _increaseCommand;
            }
        }
        public static RoutedCommand DecreaseCommand
        {
            get
            {
                return _decreaseCommand;
            }
        }

        private static void InitializeCommands()
        {
            _increaseCommand = new RoutedCommand("IncreaseCommand", typeof(GUISpinControl));
            CommandManager.RegisterClassCommandBinding(typeof(GUISpinControl), new CommandBinding(_increaseCommand, OnIncreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(GUISpinControl), new InputBinding(_increaseCommand, new KeyGesture(Key.Up)));

            _decreaseCommand = new RoutedCommand("DecreaseCommand", typeof(GUISpinControl));
            CommandManager.RegisterClassCommandBinding(typeof(GUISpinControl), new CommandBinding(_decreaseCommand, OnDecreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(GUISpinControl), new InputBinding(_decreaseCommand, new KeyGesture(Key.Down)));
        }

        private static void OnIncreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            GUISpinControl control = sender as GUISpinControl;
            if (control != null)
            {
                control.OnIncrease();
            }
        }
        private static void OnDecreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            GUISpinControl control = sender as GUISpinControl;
            if (control != null)
            {
                control.OnDecrease();
            }
        }

        protected virtual void OnIncrease()
        {
            if (Value < MaxValue)
            {
                Value += 1;
            }
        }
        protected virtual void OnDecrease()
        {
            if (Value > MinValue)
            {
                Value -= 1;
            }
        }


    }
}
