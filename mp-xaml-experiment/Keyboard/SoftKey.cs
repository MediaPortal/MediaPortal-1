using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;


namespace MCEControls
{
    /// <summary>
    /// Represent a key on softkeyboard.
    /// </summary>
    public class SoftKey : Button
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructors

        static SoftKey()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SoftKey), new FrameworkPropertyMetadata(typeof(SoftKey)));
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------

        #region ControlKeyProperty

        private static readonly DependencyPropertyKey ControlKeyPropertyKey =
            DependencyProperty.RegisterReadOnly("ControlKey", typeof(ControlKey), typeof(SoftKey),
                                                new FrameworkPropertyMetadata(ControlKey.None));

        /// <summary>
        /// The DependencyProperty for the ControlKeyProperty
        /// </summary>
        public static readonly DependencyProperty ControlKeyProperty = ControlKeyPropertyKey.DependencyProperty;

        /// <summary>
        /// the ControlKey property
        /// </summary>
        public ControlKey ControlKey
        {
            get { return (ControlKey)GetValue(ControlKeyProperty); }
            private set { SetValue(ControlKeyPropertyKey, value); }
        }

        #endregion

        #region CharProperty

        private static readonly DependencyPropertyKey CharPropertyKey =
            DependencyProperty.RegisterReadOnly("Char", typeof(Char), typeof(SoftKey),
                                                new FrameworkPropertyMetadata());

        /// <summary>
        /// The DependencyProperty for the CharProperty
        /// </summary>
        /// <remarks>
        /// Value of this property is meaningless for a ControlKey
        /// </remarks>
        public static readonly DependencyProperty CharProperty = CharPropertyKey.DependencyProperty;

        /// <summary>
        /// the Char property
        /// </summary>
        public Char Char
        {
            get { return (Char)GetValue(CharProperty); }
            private set { SetValue(CharPropertyKey, value); }
        }

        #endregion

        #region IsCheckedProperty

        /// <summary>
        ///     The DependencyProperty for the IsChecked property.
        ///     Default Value: false
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
                ToggleButton.IsCheckedProperty.AddOwner(typeof(SoftKey),
                        new FrameworkPropertyMetadata(
                                false /* default value */,
                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                                new PropertyChangedCallback(OnIsCheckedChanged)));

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SoftKey softKey = (SoftKey)d;

            if ((bool)e.NewValue)
            {
                // Update the DisplayName and Char property on this ToggleKey
                softKey.UpdateNameKeyPair(1);

                softKey.OnChecked(new RoutedEventArgs(CheckedEvent));
            }
            else
            {
                // Update the DisplayName and Char property on this ToggleKey
                softKey.UpdateNameKeyPair(0);

                softKey.OnUnchecked(new RoutedEventArgs(UncheckedEvent));
            }
        }

        /// <summary>
        /// IsChecked indicates whether the SoftKey is currently locked.
        /// </summary>
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        #endregion

        #region IsToggleKeyProperty

        private static readonly DependencyPropertyKey IsToggleKeyPropertyKey =
            DependencyProperty.RegisterReadOnly("IsToggleKey", typeof(bool), typeof(SoftKey),
                                                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// The DependencyProperty for the IsToggleKeyProperty
        /// </summary>
        public static readonly DependencyProperty IsToggleKeyProperty = IsToggleKeyPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicate whether this is a togglable softkey.
        /// </summary>
        /// <remarks>
        /// Togglable softkey includes the 'shift', 'caps', and 'intl.' keys.
        /// </remarks>
        public bool IsToggleKey
        {
            get { return (bool)GetValue(IsToggleKeyProperty); }
            private set { SetValue(IsToggleKeyPropertyKey, value); }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Events
        //
        //-------------------------------------------------------------------

        #region Events

        /// <summary>
        ///     Checked event
        /// </summary>
        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent("Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SoftKey));

        /// <summary>
        ///     Unchecked event
        /// </summary>
        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent("Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SoftKey));

        /// <summary>
        ///     Add / Remove Checked handler
        /// </summary>
        public event RoutedEventHandler Checked
        {
            add
            {
                AddHandler(CheckedEvent, value);
            }

            remove
            {
                RemoveHandler(CheckedEvent, value);
            }
        }

        /// <summary>
        ///     Add / Remove Unchecked handler
        /// </summary>
        public event RoutedEventHandler Unchecked
        {
            add
            {
                AddHandler(UncheckedEvent, value);
            }

            remove
            {
                RemoveHandler(UncheckedEvent, value);
            }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Protected Methods
        //
        //-------------------------------------------------------------------

        #region Protected Methods

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            Focus();
            e.Handled = true;
        }

        /// <summary>
        ///     Called when IsChecked becomes true.
        /// </summary>
        /// <param name="e">Event arguments for the routed event that is raised by the default implementation of this method.</param>
        protected virtual void OnChecked(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        ///     Called when IsChecked becomes false.
        /// </summary>
        /// <param name="e">Event arguments for the routed event that is raised by the default implementation of this method.</param>
        protected virtual void OnUnchecked(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// This override method is called when the control is clicked by mouse or keyboard
        /// </summary>
        /// <remarks>
        /// SoftKey override this method to toggle the IsChecked property if this is a togglable key.
        /// </remarks>
        protected override void OnClick()
        {
            if (IsToggleKey)
            {
                IsChecked = !IsChecked;
            }

            base.OnClick();
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Internal Methods
        //
        //-------------------------------------------------------------------

        #region Internal Methods

        internal void LoadData(SoftKeyData data)
        {
            _data = data;

            ControlKey = data.ControlKey;

            switch (data.ControlKey)
            {
                case ControlKey.Shift:
                case ControlKey.Caps:
                    IsToggleKey = true;
                    break;
            }

            UpdateNameKeyPair(0);
        }

        internal void UpdateNameKeyPair(int index)
        {
            if (_data.NameKeyPairs.Count == 0)
            {
                return;
            }

            // coerce the index
            if (index >= _data.NameKeyPairs.Count)
            {
                index = 0;
            }

            NameKeyPair pair = _data.NameKeyPairs[index];

            Char = pair.Key;
            Content = pair.DisplayName ?? pair.Key.ToString();
        }
 
       #endregion

        private SoftKeyData _data;
    }
}
