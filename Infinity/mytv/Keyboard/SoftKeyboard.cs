using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Data;


namespace MCEControls
{
  /// <summary>
  /// SoftKeyboard class.
  /// </summary>
  public class SoftKeyboard : FrameworkElement
  {
    //-------------------------------------------------------------------
    //
    //  Constructors
    //
    //-------------------------------------------------------------------

    #region Constructors

    static SoftKeyboard()
    {
      EventManager.RegisterClassHandler(typeof(SoftKeyboard), SoftKey.ClickEvent, new RoutedEventHandler(OnSoftKeyClicked));
      EventManager.RegisterClassHandler(typeof(SoftKeyboard), SoftKey.CheckedEvent, new RoutedEventHandler(OnSoftKeyCheckChanged));
      EventManager.RegisterClassHandler(typeof(SoftKeyboard), SoftKey.UncheckedEvent, new RoutedEventHandler(OnSoftKeyCheckChanged));
      //EventManager.RegisterClassHandler(typeof(SoftKeyboard), RemoteInputManager.RemoteInputEvent, new RemoteInputEventHandler(OnRemoteInput));
      EventManager.RegisterClassHandler(typeof(SoftKeyboard), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
    }

    ///// <summary>
    ///// default constructor
    ///// </summary>
    //public SoftKeyboard()
    //{
    //    Loaded += delegate(object sender, RoutedEventArgs e)
    //        {
    //            RemoteInputManager.AddHwndSourceHook(sender as Visual);
    //            KeyboardMessageHelper.AddHook();
    //        };

    //    Unloaded += delegate(object sender, RoutedEventArgs e)
    //        {
    //            RemoteInputManager.RemoveHwndSourceHook();
    //            KeyboardMessageHelper.RemoveHook();
    //        };
    //}

    #endregion

    //-------------------------------------------------------------------
    //
    //  Public Properties
    //
    //-------------------------------------------------------------------

    #region SoftKeyContainerStyle

    /// <summary>
    /// SoftKeyContainerStyle DependencyProperty
    /// </summary>
    public static readonly DependencyProperty SoftKeyContainerStyleProperty =
                SoftKeyboardTextBox.SoftKeyContainerStyleProperty.AddOwner(typeof(SoftKeyboard),
                        new PropertyMetadata(new PropertyChangedCallback(OnSoftKeyContainerStylePropertyChanged)));

    /// <summary>
    ///     SoftKeyContainerStyle is the style used to display each key.
    /// </summary>
    public Style SoftKeyContainerStyle
    {
      get { return (Style)GetValue(SoftKeyContainerStyleProperty); }
      set { SetValue(SoftKeyContainerStyleProperty, value); }
    }

    private static void OnSoftKeyContainerStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      UpdateSoftKeyProperty(((SoftKeyboard)d)._softKeyList, SoftKey.StyleProperty, e.NewValue);
    }

    #endregion

    #region SoftKeyTemplate

    /// <summary>
    /// SoftKeyTemplate DependencyProperty
    /// </summary>
    public static readonly DependencyProperty SoftKeyTemplateProperty =
                SoftKeyboardTextBox.SoftKeyTemplateProperty.AddOwner(typeof(SoftKeyboard),
                        new PropertyMetadata(new PropertyChangedCallback(OnSoftKeyTemplatePropertyChanged)));

    /// <summary>
    ///     SoftKeyTemplate is the template used to display each key.
    /// </summary>
    public DataTemplate SoftKeyTemplate
    {
      get { return (DataTemplate)GetValue(SoftKeyTemplateProperty); }
      set { SetValue(SoftKeyTemplateProperty, value); }
    }

    private static void OnSoftKeyTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      UpdateSoftKeyProperty(((SoftKeyboard)d)._softKeyList, SoftKey.ContentTemplateProperty, e.NewValue);
    }

    #endregion

    #region SoftKeyTemplateSelector

    /// <summary>
    /// SoftKeyTemplateSelector DependencyProperty
    /// </summary>
    public static readonly DependencyProperty SoftKeyTemplateSelectorProperty =
                SoftKeyboardTextBox.SoftKeyTemplateSelectorProperty.AddOwner(typeof(SoftKeyboard),
                        new PropertyMetadata(new PropertyChangedCallback(OnSoftKeyTemplateSelectorPropertyChanged)));

    /// <summary>
    ///     SoftKeyTemplateSelector is the template used to display each key.
    /// </summary>
    public DataTemplateSelector SoftKeyTemplateSelector
    {
      get { return (DataTemplateSelector)GetValue(SoftKeyTemplateSelectorProperty); }
      set { SetValue(SoftKeyTemplateSelectorProperty, value); }
    }

    private static void OnSoftKeyTemplateSelectorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      UpdateSoftKeyProperty(((SoftKeyboard)d)._softKeyList, SoftKey.ContentTemplateSelectorProperty, e.NewValue);
    }

    #endregion

    #region TargetTextBox

    /// <summary>
    /// the target of this softkeyboard
    /// </summary>
    public static readonly DependencyProperty TargetTextBoxProperty =
                DependencyProperty.Register("TargetTextBox", typeof(TextBox), typeof(SoftKeyboard));

    public TextBox TargetTextBox
    {
      get { return (TextBox)GetValue(TargetTextBoxProperty); }
      set { SetValue(TargetTextBoxProperty, value); }
    }

    #endregion

    //-------------------------------------------------------------------
    //
    //  Protected Methods
    //
    //-------------------------------------------------------------------

    #region Protected Methods

    /// <summary>
    /// Process arrow key input by moving focus to neighboring keys.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);

      if (e.Handled)
      {
        return;
      }

      SoftKey softkey = e.OriginalSource as SoftKey;
      bool handled = false;

      switch (e.Key)
      {
        case Key.Left:
          handled = softkey.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
          break;

        case Key.Right:
          handled = softkey.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
          break;

        case Key.Up:
          handled = softkey.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
          break;

        case Key.Down:
          handled = softkey.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
          break;

        case Key.Enter:
        case Key.Space:
          // Workaround due to TextBox always handle key input.
          //
          // Consider this scenario:
          //  1. the focus is on 'A' key of the soft keyboard
          //  2. user press 'Space' and hold for a while
          //  3. SoftKey (Button) handle the KeyDown to generate a click
          //  4. Since the 'Space' is still pressed, Input keep generate key events
          // 
          // If we don't catch the events here, eventually TextBox will get them 
          // and take them as inputs.
          //
          if (e.IsRepeat)
            handled = true;
          break;

        case Key.PageDown:
        case Key.PageUp:
          // handle Page Up / Down, since we only have 2 maps, page up/down are the same.
          NextKeymap();
          handled = true;
          break;

        case Key.Escape:
        case Key.Delete:
          handled = CloseSoftKeyboard();
          break;

        case Key.Back:
        case Key.BrowserBack:
          handled = OnBackspace();
          break;
      }

      if (handled)
      {
        e.Handled = true;
      }
    }

    /// <summary>
    /// Pass text input to TextBox
    /// </summary>
    /// <param name="e"></param>
    protected override void OnTextInput(TextCompositionEventArgs e)
    {
      base.OnTextInput(e);

      if (IsValidInput(e.Text))
      {
        AddString(e.Text);
        e.Handled = true;
      }
    }

    /// <summary>
    /// Do a basic validation for input. 
    /// Filter out "\b", "\t", "\n", "\v", "\f", "\r"
    /// </summary>
    /// <param name="input">the input string</param>
    /// <returns>true for valid input, otherwise return false</returns>
    private bool IsValidInput(string input)
    {
      if (!string.IsNullOrEmpty(input))
      {
        int k = (int)input[0];
        return (k < 8) || (k > 13);
      }

      return false;
    }

    /// <summary>
    /// override this method to always handle the event.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      base.OnMouseDown(e);

      // always handle it otherwise TextBox will grap the keyboard focus and we won't be 
      /// able to drive the softkeyboard any more.
      e.Handled = true;
    }

    /// <summary>
    /// Updates DesiredSize of the SoftKeyboard. Called by parent UIElement. This is the first pass of layout.
    /// </summary>
    /// <remarks>
    /// SoftKeyboard override this method to ensure keymap is loaded.
    /// </remarks>
    /// <param name="constraint">Constraint size is an "upper limit" that the return value should not exceed.</param>
    /// <returns>The SoftKeyboard's desired size.</returns>
    protected override Size MeasureOverride(Size constraint)
    {
      UIElement child = Child;
      SoftKeyboardTextBox textbox = TargetTextBox as SoftKeyboardTextBox;

      if (child == null
          && (textbox == null || textbox.IsSoftKeyboardOpened || TemplatedParent != textbox))
      {
        // only load the keys at the first time SoftKeyboard show up
        LoadKeyboard();
        child = Child;
      }

      if (child != null)
      {
        child.Measure(constraint);
        return (child.DesiredSize);
      }

      return (new Size());
    }

    /// <summary>
    /// SoftKeyboard computes the position of its single child inside child's Margin and calls Arrange
    /// on the child.
    /// </summary>
    /// <param name="arrangeSize">Size the SoftKeyboard will assume.</param>
    protected override Size ArrangeOverride(Size arrangeSize)
    {
      UIElement child = Child;
      if (child != null)
      {
        child.Arrange(new Rect(arrangeSize));
      }
      return (arrangeSize);
    }

    /// <summary>
    /// Returns the child at the specified index.
    /// </summary>
    protected override Visual GetVisualChild(int index)
    {
      if ((_child == null)
          || (index != 0))
      {
        throw new ArgumentOutOfRangeException("index", index, "argument out of range.");
      }

      return _child;
    }

    #endregion

    //-------------------------------------------------------------------
    //
    //  Protected Properties
    //
    //-------------------------------------------------------------------

    #region Protected Properties

    /// <summary>
    /// The panel for the SoftKeys
    /// </summary>
    protected UIElement Child
    {
      get
      {
        return _child;
      }

      set
      {
        if (_child != value)
        {
          // notify the visual layer that the old child has been removed.
          RemoveVisualChild(_child);

          //need to remove old element from logical tree
          RemoveLogicalChild(_child);

          _child = value;

          AddLogicalChild(value);
          // notify the visual layer about the new child.
          AddVisualChild(value);

          InvalidateMeasure();
        }
      }
    }

    /// <summary> 
    /// Returns enumerator to logical children.
    /// </summary>
    protected override IEnumerator LogicalChildren
    {
      get
      {
        if (_child == null)
        {
          return EmptyEnumerator.Instance;
        }

        return new SingleChildEnumerator(_child);
      }
    }

    /// <summary>
    /// Returns the Visual children count.
    /// </summary>
    protected override int VisualChildrenCount
    {
      get { return (_child == null) ? 0 : 1; }
    }

    #endregion

    //-------------------------------------------------------------------
    //
    //  Internal Methods
    //
    //-------------------------------------------------------------------

    #region Internal Methods

    /// <summary>
    /// Move focus on the 'Close' key.
    /// </summary>
    internal void FocusSoftKeyboard(bool useLastFocusedItem)
    {
      // find the close key
      SoftKey key = null;

      if (useLastFocusedItem) // use the last focused key
      {
        key = _lastFocusedKey;
      }

      if (key == null) // use 'Close' key
      {
        key = FindSoftKey(ControlKey.Close);
      }

      // use the first key if there is no close key
      if (key == null && _softKeyList != null && _softKeyList.Count > 0)
      {
        key = _softKeyList[0];
      }

      if (key != null)
      {
        Keyboard.Focus(key);
      }
    }

    #endregion

    //-------------------------------------------------------------------
    //
    //  Private Methods
    //
    //-------------------------------------------------------------------

    #region Private Methods

    /// <summary>
    /// keep track of the last focused softkey, store in the _lastFocusedKey
    /// </summary>
    private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      SoftKeyboard kb = (SoftKeyboard)sender;
      SoftKey key = e.NewFocus as SoftKey;
      if (key != null)
      {
        kb._lastFocusedKey = key;
      }
    }

    /// <summary>
    /// Navigate to next keymap
    /// </summary>
    private void NextKeymap()
    {
      if (CapsKey != null)
      {
        CapsKey.IsChecked = !CapsKey.IsChecked;
      }
    }

    /// <summary>
    /// handle SoftKey.Checked and Unchecked event.
    /// </summary>
    private static void OnSoftKeyCheckChanged(object sender, RoutedEventArgs e)
    {
      SoftKeyboard softKeyboard = (SoftKeyboard)sender;
      SoftKey softKey = e.OriginalSource as SoftKey;

      if (softKeyboard != null && softKey != null && softKey.IsToggleKey)
      {
        // Update the DisplayName and Key properties of the non-control keys on softkeyboard.
        softKeyboard.UpdateKeyboardData();

        // handle it. If user want to receive this event, register class handler 
        // and pass true for the 'handledEventToo' prarameter.
        e.Handled = true;
      }
    }

    /// <summary>
    /// handle SoftKey.Clicked event.
    /// </summary>
    private static void OnSoftKeyClicked(object sender, RoutedEventArgs e)
    {
      SoftKeyboard softKeyboard = (SoftKeyboard)sender;
      SoftKey softKey = e.OriginalSource as SoftKey;

      if (softKey != null && softKeyboard != null)
      {
        softKeyboard.SoftKeyClicked(softKey);

        // handle it. If user want to receive this event, register class handler 
        // and pass true for the 'handledEventToo' prarameter.
        e.Handled = true;

        //workaround due to after this, focus lost to TextBox
        softKey.Focus();
      }
    }

    private void SoftKeyClicked(SoftKey softKey)
    {
      switch (softKey.ControlKey)
      {
        case ControlKey.Back:
          TextBoxHelper.RemoveOneChar(TargetTextBox);
          break;

        case ControlKey.Close:
          CloseSoftKeyboard();
          break;

        case ControlKey.Space:
        case ControlKey.None:
          AddString(softKey.Char.ToString());
          break;

        default: // Shift and Caps are handled in OnSoftKeyCheckChanged
          break;
      }

      if (softKey.ControlKey == ControlKey.Shift)
      {
        _isShiftKeyDown = softKey.IsChecked;
      }
      else if (_isShiftKeyDown && softKey.ControlKey == ControlKey.None) // Shift is down last time, reset keymap
      {
        _isShiftKeyDown = ShiftKey.IsChecked = false;
      }
    }

    /// <summary>
    /// process input of Backspace
    /// </summary>
    /// <returns>return true if success, otherwise false</returns>
    private bool OnBackspace()
    {
      bool handled;

      // remove one char
      handled = TextBoxHelper.RemoveOneChar(TargetTextBox);

      // If press Back/BrowerBack when CaretIndex is 0, above function will return false.
      // close softkeyboard.
      if (!handled)
      {
        handled = CloseSoftKeyboard();
      }

      return handled;
    }

    /// <summary>
    /// process input of Escape
    /// </summary>
    /// <returns>return true if success, otherwise false</returns>
    private bool OnEscape()
    {
      return CloseSoftKeyboard();
    }

    /// <summary>
    /// Insert string to parent Caret position
    /// </summary>
    /// <param name="input">the string of input</param>
    private void AddString(string input)
    {
      if (TargetTextBox != null && !TargetTextBox.IsReadOnly)
      {
        string text = TargetTextBox.Text;

        if (TargetTextBox.SelectionLength != 0)
        {
          text = text.Remove(TargetTextBox.SelectionStart, TargetTextBox.SelectionLength);
        }

        // backup index b/c TextBox reset it on every Text change
        int caretIndex = TargetTextBox.CaretIndex;

        TargetTextBox.Text = text.Insert(caretIndex, input);

        TargetTextBox.CaretIndex = caretIndex + 1;
      }
    }

    private bool CloseSoftKeyboard()
    {
      SoftKeyboardTextBox stb = TargetTextBox as SoftKeyboardTextBox;

      if (stb != null)
      {
        stb.IsSoftKeyboardOpened = false;
        stb.Focus();

        return true;
      }

      return false;
    }

    private void LoadKeyboard()
    {
      Child = CreateKeyboard(DefaultKeyMap);

      //
      // load Template\Style properties to each key if set.
      //
      if (SoftKeyContainerStyle != null)
      {
        UpdateSoftKeyProperty(_softKeyList, SoftKey.StyleProperty, SoftKeyContainerStyle);
      }
      if (SoftKeyTemplate != null)
      {
        UpdateSoftKeyProperty(_softKeyList, SoftKey.ContentTemplateProperty, SoftKeyTemplate);
      }
      if (SoftKeyTemplateSelector != null)
      {
        UpdateSoftKeyProperty(_softKeyList, SoftKey.ContentTemplateSelectorProperty, SoftKeyTemplateSelector);
      }
    }

    private Grid CreateKeyboard(SoftKeyDataList keyDataList)
    {
      Grid grid = new Grid();

      _softKeyList = new List<SoftKey>(keyDataList.Count);

      int maxColumn = 1;
      int maxRow = 1;

      foreach (SoftKeyData data in keyDataList)
      {
        maxColumn = Math.Max(maxColumn, data.Column + data.ColumnSpan);
        maxRow = Math.Max(maxRow, data.Row + data.RowSpan);

        SoftKey softKey = new SoftKey();
        _softKeyList.Add(softKey);

        if (data.Column != 0)
          Grid.SetColumn(softKey, data.Column);

        if (data.Row != 0)
          Grid.SetRow(softKey, data.Row);

        if (data.ColumnSpan != 1)
          Grid.SetColumnSpan(softKey, data.ColumnSpan);

        if (data.RowSpan != 1)
          Grid.SetRowSpan(softKey, data.RowSpan);

        // load data list to 
        softKey.LoadData(data);

        grid.Children.Add(softKey);
      }

      for (int i = 0; i < maxColumn; i++)
      {
        ColumnDefinition cd = new ColumnDefinition();
        cd.Width = new GridLength(1.0, GridUnitType.Star);
        grid.ColumnDefinitions.Add(cd);
      }

      for (int i = 0; i < maxRow; i++)
      {
        RowDefinition rd = new RowDefinition();
        rd.Height = new GridLength(1.0, GridUnitType.Star);
        grid.RowDefinitions.Add(rd);
      }

      return grid;
    }

    /// <summary>
    /// Update 'dp' property with 'newValue'
    /// Used by property change callback of SoftKeyContainerStyle\KeyTemplate\KeyTemplateSelector properties
    /// </summary>
    /// <param name="keys">list of all soft keys</param>
    /// <param name="dp">the DP which has value changed</param>
    /// <param name="newValue">the new value of the DP</param>
    private static void UpdateSoftKeyProperty(List<SoftKey> keys, DependencyProperty dp, object newValue)
    {
      if (keys == null)
      {
        return;
      }

      if (newValue != null)
      {
        foreach (SoftKey key in keys)
        {
          key.SetValue(dp, newValue);
        }
      }
      else
      {
        foreach (SoftKey key in keys)
        {
          key.ClearValue(dp);
        }
      }
    }

    /// <summary>
    /// Anyone of the Shift/Caps/Intl toggle key changed will
    /// call this method to update the key map.
    /// </summary>
    private void UpdateKeyboardData()
    {
      int shift = ShiftKey.IsChecked ? 1 : 0;
      int caps = CapsKey.IsChecked ? 1 : 0;

      // the 2 list of keymap data are
      //  0: EnLowerCase 
      //  1: EnUpperCase 
      UpdateSoftKeys(shift ^ caps);
    }

    private void UpdateSoftKeys(int index)
    {
      foreach (SoftKey key in _softKeyList)
      {
        if (key.ControlKey == ControlKey.None)
        {
          key.UpdateNameKeyPair(index);
        }
      }
    }

    private SoftKey FindSoftKey(ControlKey controlKey)
    {
      if (_softKeyList != null)
      {
        foreach (SoftKey key in _softKeyList)
        {
          if (key.ControlKey == controlKey)
          {
            return key;
          }
        }
      }

      return null;
    }

    #endregion

    //-------------------------------------------------------------------
    //
    //  Private Properties
    //
    //-------------------------------------------------------------------

    #region Private Properties

    private SoftKeyDataList DefaultKeyMap
    {
      get
      {
        if (s_defaultKeyList == null)
        {
          ComponentResourceKey keymapKey = new ComponentResourceKey(typeof(SoftKeyboard), "SoftKeyDataList");
          s_defaultKeyList = FindResource(keymapKey) as SoftKeyDataList;
        }

        return s_defaultKeyList;
      }
    }

    private SoftKey ShiftKey
    {
      get
      {
        if (_shiftKey == null)
        {
          _shiftKey = FindSoftKey(ControlKey.Shift);
        }

        return _shiftKey;
      }
    }

    private SoftKey CapsKey
    {
      get
      {
        if (_capsKey == null)
        {
          _capsKey = FindSoftKey(ControlKey.Caps);
        }

        return _capsKey;
      }
    }

    #endregion

    //-------------------------------------------------------------------
    //
    //  Private Fields
    //
    //-------------------------------------------------------------------

    #region Private Fields

    private SoftKey _lastFocusedKey;
    private UIElement _child;
    private List<SoftKey> _softKeyList;
    private SoftKey _shiftKey;
    private SoftKey _capsKey;
    private bool _isShiftKeyDown;

    private static SoftKeyDataList s_defaultKeyList;

    #endregion
  }
}
