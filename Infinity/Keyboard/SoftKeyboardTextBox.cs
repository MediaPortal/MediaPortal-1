using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


namespace MCEControls
{
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_SoftKeyboard", Type = typeof(SoftKeyboard))]
    [TemplatePart(Name = "PART_ExpandCollapseButton", Type = typeof(ToggleButton))]
    public class SoftKeyboardTextBox : TextBox
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructors

        static SoftKeyboardTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SoftKeyboardTextBox), new FrameworkPropertyMetadata(typeof(SoftKeyboardTextBox)));
            EventManager.RegisterClassHandler(typeof(SoftKeyboardTextBox), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(SoftKeyboardTextBox), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Methods
        //
        //-------------------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// Called when the Template's tree has been generated
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // unregister the event
            if (_dropDownPopup != null)
            {
                _dropDownPopup.Closed -= OnPopupClosed;
            }

            _expandCollapseButton = GetTemplateChild("PART_ExpandCollapseButton") as ToggleButton;
            _dropDownPopup = GetTemplateChild("PART_Popup") as Popup;
            _softKeyboard = GetTemplateChild("PART_SoftKeyboard") as SoftKeyboard;

            // this is for firing SoftKeyboardOpened
            if (_dropDownPopup != null)
            {
                _dropDownPopup.Closed += OnPopupClosed;
            }
        }

        private void OnPopupClosed(object source, EventArgs e)
        {
            OnSoftKeyboardClosed(new RoutedEventArgs(SoftKeyboardClosedEvent));
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------

        #region IsSoftKeyboardOpened

        /// <summary>
        /// DependencyProperty for IsSoftKeyboardOpened
        /// </summary>
        public static readonly DependencyProperty IsSoftKeyboardOpenedProperty =
                DependencyProperty.Register(
                        "IsSoftKeyboardOpened",
                        typeof(bool),
                        typeof(SoftKeyboardTextBox),
                        new FrameworkPropertyMetadata(
                                false,
                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                new PropertyChangedCallback(OnIsSoftKeyboardOpenedChanged),
                                new CoerceValueCallback(CoerceIsSoftKeyboardOpened)));

        /// <summary>
        /// Whether or not the "popup" for this control is currently open
        /// </summary>
        [Bindable(true), Browsable(false), Category("Appearance")]
        public bool IsSoftKeyboardOpened
        {
            get { return (bool)GetValue(IsSoftKeyboardOpenedProperty); }
            set { SetValue(IsSoftKeyboardOpenedProperty, value); }
        }

        private static object CoerceIsSoftKeyboardOpened(DependencyObject d, object value)
        {
            if ((bool)value)
            {
                SoftKeyboardTextBox textbox = (SoftKeyboardTextBox)d;
                if (!textbox.IsLoaded)
                {
                    textbox.RegisterToOpenOnLoaded();
                    return false;
                }
            }

            return value;
        }

        private void RegisterToOpenOnLoaded()
        {
            Loaded += new RoutedEventHandler(OpenOnLoaded);
        }

        private void OpenOnLoaded(object sender, RoutedEventArgs e)
        {
            // Open softkeyboard after it has rendered (Loaded is fired before 1st render)
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate(object param)
            {
                CoerceValue(IsSoftKeyboardOpenedProperty);

                // acquire focus if SKBTextBox has its Keyboard opened at startup
                if (IsSoftKeyboardOpened)
                {
                    Focus();
                }

                return null;
            }), null);
        }

        private static void OnIsSoftKeyboardOpenedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SoftKeyboardTextBox textbox = (SoftKeyboardTextBox)d;

            if ((bool)e.NewValue)
            {
                // When the drop down opens, take capture
                Mouse.Capture(textbox, CaptureMode.SubTree);

                textbox.Dispatcher.BeginInvoke(
                    DispatcherPriority.Send,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        SoftKeyboardTextBox kb = (SoftKeyboardTextBox)arg;

                        // refocus the TextBox
                        if (kb._expandCollapseButton != null && kb._expandCollapseButton.IsFocused)
                        {
                            kb.Focus();
                        }

                        // NOTE: softkeyboard and textbox are within two different focus domains.
                        kb._softKeyboard.FocusSoftKeyboard(/*useLastFocusedItem =*/ false);

                        return null;
                    },
                    textbox);

                textbox.OnSoftKeyboardOpened(new RoutedEventArgs(SoftKeyboardOpenedEvent));
            }
            else
            {
                if (textbox.HasCapture)
                {
                    Mouse.Capture(null);
                }

                // No Popup in the style so fire closed now
                if (textbox._dropDownPopup == null)
                {
                    textbox.OnSoftKeyboardClosed(new RoutedEventArgs(SoftKeyboardClosedEvent));
                }
            }
        }

        #endregion

        #region SoftKeyContainerStyle

        /// <summary>
        /// SoftKeyContainerStyle DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SoftKeyContainerStyleProperty =
                DependencyProperty.Register(
                        "SoftKeyContainerStyle",
                        typeof(Style),
                        typeof(SoftKeyboardTextBox));

        /// <summary>
        ///     The Style used to display each key.
        /// </summary>
        public Style SoftKeyContainerStyle
        {
            get { return (Style)GetValue(SoftKeyContainerStyleProperty); }
            set { SetValue(SoftKeyContainerStyleProperty, value); }
        }

        #endregion

        #region SoftKeyTemplate

        /// <summary>
        /// SoftKeyTemplate DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SoftKeyTemplateProperty =
                DependencyProperty.Register(
                        "SoftKeyTemplate",
                        typeof(DataTemplate),
                        typeof(SoftKeyboardTextBox));

        /// <summary>
        ///     The data template used to display each key.
        /// </summary>
        public DataTemplate SoftKeyTemplate
        {
            get { return (DataTemplate)GetValue(SoftKeyTemplateProperty); }
            set { SetValue(SoftKeyTemplateProperty, value); }
        }

        #endregion

        #region SoftKeyTemplateSelector

        /// <summary>
        /// SoftKeyTemplateSelector DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SoftKeyTemplateSelectorProperty =
                DependencyProperty.Register(
                        "SoftKeyTemplateSelector",
                        typeof(DataTemplateSelector),
                        typeof(SoftKeyboardTextBox));

        /// <summary>
        ///     The data templateSelector used to display each key.
        /// </summary>
        public DataTemplateSelector SoftKeyTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(SoftKeyTemplateSelectorProperty); }
            set { SetValue(SoftKeyTemplateSelectorProperty, value); }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Events
        //
        //-------------------------------------------------------------------

        #region Events

        /// <summary>
        ///     SoftKeyboardOpened event
        /// </summary>
        public static readonly RoutedEvent SoftKeyboardOpenedEvent = 
            EventManager.RegisterRoutedEvent("SoftKeyboardOpened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SoftKeyboardTextBox));

        /// <summary>
        ///     SoftKeyboardClosed event
        /// </summary>
        public static readonly RoutedEvent SoftKeyboardClosedEvent = 
            EventManager.RegisterRoutedEvent("SoftKeyboardClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SoftKeyboardTextBox));

        /// <summary>
        ///     Add / Remove SoftKeyboardOpened handler
        /// </summary>
        public event RoutedEventHandler SoftKeyboardOpened
        {
            add
            {
                AddHandler(SoftKeyboardOpenedEvent, value);
            }

            remove
            {
                RemoveHandler(SoftKeyboardOpenedEvent, value);
            }
        }

        /// <summary>
        ///     Add / Remove SoftKeyboardClosed handler
        /// </summary>
        public event RoutedEventHandler SoftKeyboardClosed
        {
            add
            {
                AddHandler(SoftKeyboardClosedEvent, value);
            }

            remove
            {
                RemoveHandler(SoftKeyboardClosedEvent, value);
            }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Protected Methods
        //
        //-------------------------------------------------------------------

        #region Protected Methods

        /// <summary>
        /// Raise SoftKeyboardOpened event.
        /// </summary>
        /// <param name="e">the event arguments</param>
        protected virtual void OnSoftKeyboardOpened(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// Raise SoftKeyboardClosed event.
        /// </summary>
        /// <param name="e">the event arguments</param>
        protected virtual void OnSoftKeyboardClosed(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// Redirects arrow key navigation at the ends of the control to shift focus
        /// to neighboring controls.
        /// </summary>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (!IsSoftKeyboardOpened)
            {
                FrameworkElement currentFocus;

                // focus is on the ExpandCollapse button
                if (_expandCollapseButton != null
                    && (_expandCollapseButton.IsFocused || _expandCollapseButton.IsKeyboardFocusWithin))
                {
                    if (e.Key == Key.Left)
                    {
                        Focus();
                        e.Handled = true;
                        return;
                    }
                    else if (e.Key == Key.Space)
                    {
                        // Workaround for 'cannot use space to open soft kb when focus is on 
                        // the ExpandCollapse button (while Enter key work)'. This is b/c 
                        // textbox get this event before the toggle button and marke the event as
                        // handled, so the button will never get the event.
                        IsSoftKeyboardOpened = true;
                        e.Handled = true;
                        return;
                    }

                    currentFocus = _expandCollapseButton;
                }
                // focus is inside textbox
                else
                {
                    if (e.Key == Key.Left && CaretIndex != 0)
                    {
                        return;
                    }
                    else if (e.Key == Key.Right)
                    {
                        // 1. caret is in the middle of text, move caret
                        if (CaretIndex != Text.Length)
                        {
                            return;
                        }
                        // 2. caret is at the end of text, move focus
                        // 2.1 if expandcollapse button exist, move focus to it
                        if (_expandCollapseButton != null)
                        {
                            // focus ExpandCollapse button
                            _expandCollapseButton.Focus();
                            e.Handled = true;
                            return;
                        }
                        // 2.2 otherwise move focus to control at right
                    }

                    currentFocus = this;
                }

                switch (e.Key)
                {
                    case Key.Left:
                        e.Handled = currentFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                        break;

                    case Key.Up:
                        e.Handled = currentFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                        break;

                    case Key.Right:
                        e.Handled = currentFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                        break;

                    case Key.Down:
                        e.Handled = currentFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                        break;

                    case Key.Enter:
                        if (!e.IsRepeat)
                        {
                            IsSoftKeyboardOpened = true;
                        }
                        e.Handled = true;
                        break;

                    case Key.BrowserBack:
                        //NOTE: this code current not work b/c the MCE host also handle this event
                        e.Handled = TextBoxHelper.RemoveOneChar(this);
                        break;
                }
            }
        }

        /// <summary>
        /// some key stroke combination can result in textbox regain the keyboardfocus 
        /// even when Softkeyboard is opened. So re-focus the keyboard.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsSoftKeyboardOpened && _softKeyboard != null)
            {
                _softKeyboard.FocusSoftKeyboard(/*useLastFocusedItem =*/ true);
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// Grab focus when Mouse over
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (!IsSoftKeyboardOpened)
            {
                Focus();
                e.Handled = true;
            }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Private Methods
        //
        //-------------------------------------------------------------------

        #region Private Methods

        private static void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            SoftKeyboardTextBox textbox = (SoftKeyboardTextBox)sender;

            if (Mouse.Captured != textbox)
            {
                if (e.OriginalSource == textbox)
                {
                    // If capture is null or it's not below the textbox, close.
                    if (Mouse.Captured == null || !textbox.Contains(Mouse.Captured as DependencyObject))
                    {
                        textbox.IsSoftKeyboardOpened = false;
                    }
                }
                else
                {
                    if (textbox.Contains(e.OriginalSource as DependencyObject))
                    {
                        // Take capture if one of our children gave up capture (by closing their drop down)
                        if (textbox.IsSoftKeyboardOpened && Mouse.Captured == null)
                        {
                            Mouse.Capture(textbox, CaptureMode.SubTree);
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        textbox.IsSoftKeyboardOpened = false;
                    }
                }
            }
        }

        /// <summary>
        /// whether 'node' is a descendant of 'reference'.
        /// 2 possiblities:
        ///     1. node is in the sub-visual tree of reference
        ///     2. node is in the sub-visual tree of the textbox's SoftKeyboard
        /// </summary>
        private bool Contains(DependencyObject node)
        {
            while (node != null)
            {
                if (node == this || node == this._softKeyboard)
                {
                    return true;
                }

                // walk tree
                node = VisualTreeHelper.GetParent(node);
            }

            return false;
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Private Fields
        //
        //-------------------------------------------------------------------

        #region Private Fields

        private bool HasCapture { get { return Mouse.Captured == this; } }

        private SoftKeyboard _softKeyboard;
        private ToggleButton _expandCollapseButton;
        private Popup _dropDownPopup;

        #endregion
    }
}
