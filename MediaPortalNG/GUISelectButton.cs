using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaPortal;

namespace MediaPortal
{

    public partial class GUISelectButton : System.Windows.Controls.UserControl
    {
        private static RoutedCommand _nextCommand;
        private static RoutedCommand _previousCommand;
        private static RoutedCommand _OpenCloseSelect;

        private System.Collections.ArrayList _items;
        private int _selectedItem;
        private static bool _selectOpen;
        private static int _selectionFieldWidth;

        public GUISelectButton()
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
            _items = new System.Collections.ArrayList();
            _selectedItem = -1;
            InitializeCommands();
        }


        public int SelectionFieldWidth
        {
            get
            {
                return _selectionFieldWidth;
            }
            set
            {
                _selectionFieldWidth = value;
            }
        }
 
        public void AddItem(object item)
        {
            _items.Add(item);
        }

        public void DeleteItem(object item)
        {
            _items.Remove(item);
        }

        public void ClearItems()
        {
            _items.Clear();
        }

        public object SelectedItem
        {
            get 
            {
                return (object)GetValue(SelectedItemProperty); 
            }
            set { SetValue(SelectedItemProperty, value); }
        }



         public static readonly DependencyProperty SelectedItemProperty =
           DependencyProperty.Register(
               "SelectedItem", typeof(object), typeof(GUISelectButton),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedItemChanged)));


        private static void OnSelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISelectButton control = (GUISelectButton)obj;

            RoutedPropertyChangedEventArgs<object> e = new RoutedPropertyChangedEventArgs<object>(
                (object)args.OldValue, (object)args.NewValue, SelectedItemChangedEvent);
            control.OnSelectedItemChanged(e);
        }


        protected virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> args)
        {
            RaiseEvent(args);
        }

        public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectedItemChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<object>), typeof(GUISelectButton));

        public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged
        {
            add { AddHandler(SelectedItemChangedEvent, value); }
            remove { RemoveHandler(SelectedItemChangedEvent, value); }
        }

        private void UpdateValue()
        {
            if (_items != null)
            {
                if (_selectedItem < 0 || _selectedItem > _items.Count)
                    return;
                SelectedItem = _items[_selectedItem];
            }
        }

        public static RoutedCommand OpenCloseSelect
        {
            get
            {
                return _OpenCloseSelect;
            }
        }
 

        public static RoutedCommand NextCommand
        {
            get
            {
                return _nextCommand;
            }
        }
        public static RoutedCommand PreviousCommand
        {
            get
            {
                return _previousCommand;
            }
        }

        private static void InitializeCommands()
        {
            _nextCommand = new RoutedCommand("NextCommand", typeof(GUISelectButton));
            CommandManager.RegisterClassCommandBinding(typeof(GUISelectButton), new CommandBinding(_nextCommand, OnNextCommand));
            CommandManager.RegisterClassInputBinding(typeof(GUISelectButton), new InputBinding(_nextCommand, new KeyGesture(Key.Right)));

            _previousCommand = new RoutedCommand("PreviousCommand", typeof(GUISelectButton));
            CommandManager.RegisterClassCommandBinding(typeof(GUISelectButton), new CommandBinding(_previousCommand, OnPreviousCommand));
            CommandManager.RegisterClassInputBinding(typeof(GUISelectButton), new InputBinding(_previousCommand, new KeyGesture(Key.Left)));

            _OpenCloseSelect = new RoutedCommand("OpenCloseSelect", typeof(GUISelectButton));
            CommandManager.RegisterClassCommandBinding(typeof(GUISelectButton), new CommandBinding(_OpenCloseSelect, OnOpenCloseSelect));
            CommandManager.RegisterClassInputBinding(typeof(GUISelectButton), new InputBinding(_OpenCloseSelect, new KeyGesture(Key.Return)));
        
        }

        private static void OnOpenCloseSelect(object sender, ExecutedRoutedEventArgs e)
        {
            GUISelectButton control = sender as GUISelectButton;
            _selectionFieldWidth = ((int)control.Width) - 50;

            if (control != null)
            {
                ControlTemplate t = control.Template;
                ControlTemplate rtp = (ControlTemplate)control.FindResource("SelectOpen");
                ControlTemplate rtr = (ControlTemplate)control.FindResource("SelectClose");

                if (rtr == null || rtp == null || t == null)
                    return;

                if ( _selectOpen== true)
                {
                    control.Template = rtr;
                    control.ApplyTemplate();
                    _selectOpen = false;
                    return;
                }
                else
                {
                    control.Template = rtp;
                    control.ApplyTemplate();
                    _selectOpen = true;
                    return;
                }

            }
        }

        private static void OnNextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            GUISelectButton control = sender as GUISelectButton;
            if (control != null)
            {
                control.OnNext();
            }
        }

        private static void OnPreviousCommand(object sender, ExecutedRoutedEventArgs e)
        {
            GUISelectButton control = sender as GUISelectButton;
            if (control != null)
            {
                control.OnPrevious();
            }
        }

        protected virtual void OnNext()
        {
            if (_items == null) return;

            if (_selectedItem < _items.Count-1)
            {
                _selectedItem += 1;
                UpdateValue();
            }
        }
        protected virtual void OnPrevious()
        {
            if (_items == null) return;

            if (_selectedItem > 0)
            {
                _selectedItem -= 1;
                UpdateValue();
            }
        }


    }
}
