using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ProjectInfinity.Controls
{
  public class Menu : Canvas, INotifyPropertyChanged
  {
    private const double BUTTONHEIGHT = 40;

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
                                                                                                 typeof (MenuItem),
                                                                                                 typeof (Menu),
                                                                                                 new FrameworkPropertyMetadata
                                                                                                   (null,
                                                                                                    SelectedItemPropertyChanged));

    public static readonly DependencyProperty FocusElementProperty = DependencyProperty.Register("FocusElement",
                                                                                                 typeof (
                                                                                                   FrameworkElement),
                                                                                                 typeof (Menu),
                                                                                                 new FrameworkPropertyMetadata
                                                                                                   (null,
                                                                                                    FocusElementPropertyChanged));

    public static readonly DependencyProperty FocusedVisibleProperty = DependencyProperty.Register("FocusedVisible",
                                                                                                   typeof (object),
                                                                                                   typeof (Menu),
                                                                                                   new FrameworkPropertyMetadata
                                                                                                     (null,
                                                                                                      FocusedVisiblePropertyChanged));

    public static readonly DependencyProperty FocusedHiddenProperty = DependencyProperty.Register("FocusedHidden",
                                                                                                  typeof (object),
                                                                                                  typeof (Menu),
                                                                                                  new FrameworkPropertyMetadata
                                                                                                    (null,
                                                                                                     FocusedHiddenPropertyChanged));


    public static readonly DependencyProperty FocusedMarginProperty = DependencyProperty.Register("FocusedMargin",
                                                                                                  typeof (object),
                                                                                                  typeof (Menu),
                                                                                                  new FrameworkPropertyMetadata
                                                                                                    (null,
                                                                                                     FocusedMarginPropertyChanged));


    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate",
                                                                                                 typeof (DataTemplate),
                                                                                                 typeof (Menu),
                                                                                                 new FrameworkPropertyMetadata
                                                                                                   (null,
                                                                                                    ItemTemplatePropertyChanged));

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
                                                                                                typeof (MenuCollection),
                                                                                                typeof (Menu),
                                                                                                new FrameworkPropertyMetadata
                                                                                                  (null,
                                                                                                   ItemsSourcePropertyChanged));

    public static readonly DependencyProperty SubMenuProperty = DependencyProperty.Register("SubMenu",
                                                                                            typeof (MenuCollection),
                                                                                            typeof (Menu),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               SubMenuPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="Command"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
                                                                                            typeof (ICommand),
                                                                                            typeof (Canvas),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               CommandPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter",
                                                                                                     typeof (object),
                                                                                                     typeof (Canvas),
                                                                                                     new FrameworkPropertyMetadata
                                                                                                       (null));

    /// <summary>
    /// Identifies the <see cref="CommandTarget"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget",
                                                                                                  typeof (IInputElement),
                                                                                                  typeof (Canvas),
                                                                                                  new FrameworkPropertyMetadata
                                                                                                    (null));

    #region routed events

    public static readonly RoutedEvent FocusVisibleEvent =
      EventManager.RegisterRoutedEvent("OnFocusVisible", RoutingStrategy.Bubble, typeof (RoutedEventHandler),
                                       typeof (Menu));

    public static readonly RoutedEvent FocusHiddenEvent =
      EventManager.RegisterRoutedEvent("OnFocusHidden", RoutingStrategy.Bubble, typeof (RoutedEventHandler),
                                       typeof (Menu));

    #endregion

    private int _currentOffset;
    private Storyboard _storyBoard;
    private bool _mouseEntered = false;
    private int _mouseSelectedItem = -1;
    private int _currentSelectedItem = 0;
    private bool _mouseEventsEnabled = true;
    private bool _waitForMouseMove = false;
    private Point _previousMousePoint;
    private double _yOffset = 0;
    private Brush _originalOpacityMask;

    public Menu()
    {
      FocusedHidden = Visibility.Hidden;
      FocusedVisible = Visibility.Hidden;
      FocusedMargin = new Thickness(0);
      Loaded += Menu_Loaded;
      Keyboard.AddPreviewLostKeyboardFocusHandler(this, onpreviewKeyboardLost);
      Keyboard.AddPreviewGotKeyboardFocusHandler(this, onpreviewKeyboardGot);
    }

    public object FocusedVisible
    {
      get
      {
        if (GetValue(FocusedVisibleProperty) == null)
        {
          return Visibility.Hidden;
        }
        return (Visibility) GetValue(FocusedVisibleProperty);
      }
      set
      {
        //Trace.WriteLine("Set FocusedVisible");
        if (value != FocusedVisible)
        {
          Visibility vis = (Visibility) value;
          SetValue(FocusedVisibleProperty, value);
          RoutedEventArgs args = new RoutedEventArgs();
          if (vis == Visibility.Visible)
          {
            args.RoutedEvent = FocusVisibleEvent;
          }
          else
          {
            args.RoutedEvent = FocusHiddenEvent;
          }
          args.Source = this;
          RaiseEvent(args);
          if (vis == Visibility.Visible)
          {
            FocusedHidden = Visibility.Hidden;
          }
          else
          {
            FocusedHidden = Visibility.Visible;
          }
        }
      }
    }

    public object FocusedHidden
    {
      get
      {
        if (GetValue(FocusedHiddenProperty) == null)
        {
          return Visibility.Hidden;
        }
        return (Visibility) GetValue(FocusedHiddenProperty);
      }
      set
      {
        //Trace.WriteLine("Set FocusedHidden");
        SetValue(FocusedHiddenProperty, value);
      }
    }

    public MenuItem SelectedItem
    {
      get { return (MenuItem) GetValue(SelectedItemProperty); }
      set
      {
        //Trace.WriteLine("Set SelectedItem");
        SetValue(SelectedItemProperty, value);
      }
    }

    public FrameworkElement FocusElement
    {
      get { return (FrameworkElement) GetValue(FocusElementProperty); }
      set
      {
        //Trace.WriteLine("Set FocusElement");
        SetValue(FocusElementProperty, value);
      }
    }

    public object FocusedMargin
    {
      get { return GetValue(FocusedMarginProperty); }
      set
      {
        //Trace.WriteLine("Set FocusedMargin");
        SetValue(FocusedMarginProperty, value);
      }
    }

    public MenuCollection ItemsSource
    {
      get { return GetValue(ItemsSourceProperty) as MenuCollection; }
      set
      {
        //Trace.WriteLine("Set ItemsSource");
        SetValue(ItemsSourceProperty, value);
      }
    }

    public MenuCollection SubMenu
    {
      get { return GetValue(SubMenuProperty) as MenuCollection; }
      set
      {
        //Trace.WriteLine("Set SubMenu");
        SetValue(SubMenuProperty, value);
      }
    }

    /// <summary>
    /// Gets or sets the item template.
    /// </summary>
    /// <value>The item template.</value>
    public DataTemplate ItemTemplate
    {
      get { return GetValue(ItemTemplateProperty) as DataTemplate; }
      set
      {
        //Trace.WriteLine("Set ItemTemplate");
        SetValue(ItemTemplateProperty, value);
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="ICommand"/> to execute whenever an item is activated.
    /// </summary>
    public ICommand Command
    {
      get { return GetValue(CommandProperty) as ICommand; }
      set { SetValue(CommandProperty, value); }
    }

    /// <summary>
    /// Gets or sets the parameter to be passed to the executed <see cref="Command"/>.
    /// </summary>
    public object CommandParameter
    {
      get { return GetValue(CommandParameterProperty); }
      set { SetValue(CommandParameterProperty, value); }
    }


    /// <summary>
    /// Gets or sets the element on which to raise the specified <see cref="Command"/>.
    /// </summary>
    public IInputElement CommandTarget
    {
      get { return GetValue(CommandTargetProperty) as IInputElement; }
      set { SetValue(CommandTargetProperty, value); }
    }

    #region Event Handlers

    private void animation_Completed(object sender, EventArgs e)
    {
      //Trace.WriteLine("animation_Completed");
      LayoutMenu();
    }

    private void Menu_Loaded(object sender, RoutedEventArgs e)
    {
      //Trace.WriteLine("Menu_Loaded");
      _originalOpacityMask = OpacityMask;
      LayoutMenu();
    }

    private void storyBoard_Completed(object sender, EventArgs e)
    {
      //Trace.WriteLine("storyBoard_Completed");
      if (_storyBoard != null)
      {
        _storyBoard.Remove(this);
        _storyBoard = null;
        LayoutMenu();
      }
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    public event RoutedEventHandler OnFocusVisible
    {
      add { AddHandler(FocusVisibleEvent, value); }
      remove { RemoveHandler(FocusVisibleEvent, value); }
    }

    public event RoutedEventHandler OnFocusHidden
    {
      add { AddHandler(FocusHiddenEvent, value); }
      remove { RemoveHandler(FocusHiddenEvent, value); }
    }

    #region property handlers

    private static void FocusedVisiblePropertyChanged(DependencyObject dependencyObject,
                                                      DependencyPropertyChangedEventArgs e)
    {
      //(dependencyObject as Menu).FocusedVisible = (Visibility)(e.NewValue);
    }

    private static void FocusedHiddenPropertyChanged(DependencyObject dependencyObject,
                                                     DependencyPropertyChangedEventArgs e)
    {
      //(dependencyObject as Menu).FocusedHidden = (Visibility)(e.NewValue);
    }

    private static void SelectedItemPropertyChanged(DependencyObject dependencyObject,
                                                    DependencyPropertyChangedEventArgs e)
    {
      //(dependencyObject as Menu).FocusElement = (MenuItem)(e.NewValue);
    }

    private static void FocusElementPropertyChanged(DependencyObject dependencyObject,
                                                    DependencyPropertyChangedEventArgs e)
    {
      ((Menu) dependencyObject).FocusElement = (FrameworkElement) (e.NewValue);
    }

    private static void FocusedMarginPropertyChanged(DependencyObject dependencyObject,
                                                     DependencyPropertyChangedEventArgs e)
    {
      //  (dependencyObject as Menu).FocusedMargin = (int)(e.NewValue);
    }


    private static void ItemsSourcePropertyChanged(DependencyObject dependencyObject,
                                                   DependencyPropertyChangedEventArgs e)
    {
      ((Menu) dependencyObject).ItemsSource = (e.NewValue as MenuCollection);
    }


    private static void SubMenuPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      ((Menu) dependencyObject).SubMenu = (e.NewValue as MenuCollection);
    }


    private static void ItemTemplatePropertyChanged(DependencyObject dependencyObject,
                                                    DependencyPropertyChangedEventArgs e)
    {
      ((Menu) dependencyObject).ItemTemplate = (e.NewValue as DataTemplate);
    }

    #endregion

    #region event handlers

    protected override void OnGotFocus(RoutedEventArgs e)
    {
      //Trace.WriteLine("OnGotFocus");
      if (!IsKeyboardFocusWithin)
      {
        Keyboard.Focus(this);
      }
      base.OnGotFocus(e);
    }

    private void onpreviewKeyboardLost(object sender, KeyboardFocusChangedEventArgs e)
    {
      if (IsKeyboardFocusWithin == false)
      {
        return;
      }
      FocusedVisible = Visibility.Hidden;
      //Trace.WriteLine(String.Format("p lost {0} {1}->{2} {3}", this.IsKeyboardFocusWithin, e.OldFocus, e.NewFocus, e.RoutedEvent));
    }

    private void onpreviewKeyboardGot(object sender, KeyboardFocusChangedEventArgs e)
    {
      if (IsKeyboardFocusWithin)
      {
        return;
      }
      FocusedVisible = Visibility.Visible;
      //Trace.WriteLine(String.Format("p got {0} {1}->{2} {3}", this.IsKeyboardFocusWithin, e.OldFocus, e.NewFocus, e.RoutedEvent));
    }

    //protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    //{
    //  //Trace.WriteLine("OnGotKeyboardFocus");

    //  base.OnGotKeyboardFocus(e);
    //}

    //protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    //{
    //  //Trace.WriteLine("OnLostKeyboardFocus");
    //  base.OnLostKeyboardFocus(e);
    //}


    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      //Trace.WriteLine("OnRenderSizeChanged");
      base.OnRenderSizeChanged(sizeInfo);

      if (IsLoaded)
      {
        LayoutMenu();
      }
    }

    private void LayoutMenu()
    {
      bool gotKeyboardFocus = IsFocused || IsKeyboardFocused || IsKeyboardFocusWithin;
      //Trace.WriteLine("LayoutMenu");
      _mouseEventsEnabled = false;

      if (ItemsSource == null)
      {
        return;
      }
      int maxRows = (int) (ActualHeight/BUTTONHEIGHT);
      //maxRows--;
      if (_storyBoard != null)
      {
        _storyBoard.Stop(this);
        _storyBoard.Remove(this);
        _storyBoard = null;
      }
      Children.Clear();
      Margin = new Thickness(0, 0, 0, 0);
      double yoffset = -BUTTONHEIGHT;
      int maxItems = maxRows;
      if (maxItems >= ItemsSource.Count)
      {
        maxItems = ItemsSource.Count;
        OpacityMask = null;
      }
      else
      {
        OpacityMask = _originalOpacityMask;
      }
      int selected = (maxItems)/2;
      if (_mouseEntered)
      {
        selected = _mouseSelectedItem;
      }
      else if (_mouseSelectedItem >= 0)
      {
        selected = _mouseSelectedItem;
      }

      if (selected >= maxItems + 2)
      {
        selected = (maxItems)/2;
      }
      _currentSelectedItem = selected;
      _mouseSelectedItem = selected;


      int start = -1;
      int end = maxRows + 2;
      _yOffset = 0;
      if (maxRows >= ItemsSource.Count)
      {
        start = 0;
        end = ItemsSource.Count;
        yoffset = _yOffset = ((maxRows - ItemsSource.Count)*BUTTONHEIGHT)/2;
      }

      for (int i = start; i < end; ++i)
      {
        int itemNr = _currentOffset + i;
        while (itemNr < 0)
        {
          itemNr += ItemsSource.Count;
        }
        while (itemNr >= ItemsSource.Count)
        {
          itemNr -= ItemsSource.Count;
        }
        MenuItem menuItem = ItemsSource[itemNr];
        menuItem.Content = (FrameworkElement) ItemTemplate.LoadContent();
        menuItem.Content.DataContext = menuItem;
        menuItem.Content.Margin = new Thickness(0, yoffset, 0, 0);
        if (selected == i)
        {
          ItemsSource.CurrentItem = menuItem;
          SelectedItem = menuItem;
          FocusedMargin = new Thickness(0, yoffset, 0, 0);

          SubMenu = menuItem.SubMenus;
        }

        menuItem.Content.Width = ActualWidth;
        Children.Add(menuItem.Content);
        yoffset += BUTTONHEIGHT;
      }
      if (gotKeyboardFocus)
      {
        Keyboard.Focus(Children[selected]);
      }
      else
      {
        FocusedVisible = Visibility.Hidden;
      }
      _mouseEventsEnabled = true;
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs("SelectedItem"));
      }
    }

    #region keyboard

    private void OnScrollDown()
    {
      //Trace.WriteLine("OnScrollDown");
      _mouseEventsEnabled = false;

      int maxRows = (int) (ActualHeight/BUTTONHEIGHT);
      if (ItemsSource.Count <= maxRows)
      {
        if (_currentSelectedItem > 0)
        {
          _mouseSelectedItem = _currentSelectedItem - 1;
        }
        ScrollFocusedItemToMousePosition();
        return;
      }
      if (_storyBoard != null)
      {
        _storyBoard.Stop(this);
        _storyBoard.Seek(this, new TimeSpan(0), TimeSeekOrigin.BeginTime);
        _storyBoard.Remove(this);
        LayoutMenu();
        _storyBoard = null;
      }
      FrameworkElement element = (FrameworkElement) ((FrameworkElement) Parent).Parent;
      _storyBoard = (Storyboard) element.Resources["storyBoardScrollDown"];
      _storyBoard.Completed += storyBoard_Completed;
      _storyBoard.Begin(this, true);

      _currentOffset--;
      if (_currentOffset < 0)
      {
        _currentOffset += ItemsSource.Count;
      }
    }

    private void OnScrollUp()
    {
      //Trace.WriteLine("OnScrollUp");
      _mouseEventsEnabled = false;
      int maxRows = (int) (ActualHeight/BUTTONHEIGHT);
      if (ItemsSource.Count <= maxRows)
      {
        if (_currentSelectedItem + 1 < ItemsSource.Count)
        {
          _mouseSelectedItem = _currentSelectedItem + 1;
        }
        ScrollFocusedItemToMousePosition();
        return;
      }

      if (_storyBoard != null)
      {
        _storyBoard.Stop(this);
        _storyBoard.Seek(this, new TimeSpan(0), TimeSeekOrigin.BeginTime);
        _storyBoard.Remove(this);
        LayoutMenu();
        _storyBoard = null;
      }
      FrameworkElement element = (FrameworkElement) ((FrameworkElement) Parent).Parent;
      _storyBoard = (Storyboard) element.Resources["storyBoardScrollUp"];
      _storyBoard.Completed += storyBoard_Completed;
      _storyBoard.Begin(this, true);

      _currentOffset++;
      if (_currentOffset >= ItemsSource.Count)
      {
        _currentOffset -= ItemsSource.Count;
      }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      //Trace.WriteLine("OnKeyDown");
      if (e.Key == Key.Left || e.Key == Key.Right)
      {
        _waitForMouseMove = true;
        _mouseEntered = false;
      }
      if (e.Key == Key.Up)
      {
        OnScrollDown();
        e.Handled = true;
        return;
      }
      if (e.Key == Key.Down)
      {
        OnScrollUp();
        e.Handled = true;
        return;
      }
      base.OnKeyDown(e);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
      //Trace.WriteLine("OnPreviewKeyDown");
      if (e.Key == Key.Enter)
      {
        //execute the command if there is one
        if (Command != null)
        {
          RoutedCommand routedCommand = Command as RoutedCommand;
          if (routedCommand != null)
          {
            routedCommand.Execute(CommandParameter, CommandTarget);
          }
          else
          {
            Command.Execute(CommandParameter);
          }
        }
        e.Handled = true;
        return;
      }
      base.OnPreviewKeyDown(e);
    }

    #endregion

    #region mouse

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
      //Trace.WriteLine("OnMouseWheel");
      if (e.Delta < 0)
      {
        OnScrollUp();
      }
      else if (e.Delta > 0)
      {
        OnScrollDown();
      }
      base.OnMouseWheel(e);
    }

    private void ScrollFocusedItemToMousePosition()
    {
      //Trace.WriteLine("ScrollFocusedItemToMousePosition");
      _mouseEventsEnabled = false;
      double offsetCurrent = _currentSelectedItem*BUTTONHEIGHT + _yOffset;
      double offsetNext = _mouseSelectedItem*BUTTONHEIGHT + _yOffset;
      ThicknessAnimation animation =
        new ThicknessAnimation(new Thickness(0, offsetCurrent, 0, 0), new Thickness(0, offsetNext, 0, 0),
                               new Duration(new TimeSpan(0, 0, 0, 0, 150)));

      animation.Completed += animation_Completed;
      FocusElement.BeginAnimation(MarginProperty, animation);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      //Trace.WriteLine("OnMouseMove");
      Point point = Mouse.GetPosition(Window.GetWindow(this));
      if (_waitForMouseMove)
      {
        if (Math.Abs(point.X - _previousMousePoint.X) >= 10 || Math.Abs(point.Y - _previousMousePoint.Y) >= 10)
        {
          _waitForMouseMove = false;
        }
        else
        {
          e.Handled = true;
          return;
        }
      }
      _previousMousePoint = point;
      if (_mouseEntered && _mouseEventsEnabled)
      {
        int selectedItemNr = -1;
        int maxRows = (int) (ActualHeight/BUTTONHEIGHT);
        if (ItemsSource.Count <= maxRows)
        {
          selectedItemNr = 0;
        }

        foreach (FrameworkElement element in Children)
        {
          if (element.IsMouseOver)
          {
            _mouseSelectedItem = selectedItemNr;
            if (_mouseSelectedItem != _currentSelectedItem)
            {
              ScrollFocusedItemToMousePosition();
            }
            return;
          }
          selectedItemNr++;
        }
      }
      base.OnMouseMove(e);
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
      //Trace.WriteLine("OnMouseEnter");
      if (_waitForMouseMove)
      {
        e.Handled = true;
        return;
      }
      _mouseEntered = true;
      base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      //Trace.WriteLine("OnMouseLeave");
      _mouseEntered = false;
      // LayoutMenu();
      base.OnMouseLeave(e);
    }

    //protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    //{
    //  //Trace.WriteLine("OnPreviewMouseDown");
    //  base.OnPreviewMouseDown(e);
    //}

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      //Trace.WriteLine("OnPreviewMouseLeftButtonDown");
      if (Command != null)
      {
        RoutedCommand routedCommand = Command as RoutedCommand;
        if (routedCommand != null)
        {
          routedCommand.Execute(CommandParameter, CommandTarget);
        }
        else
        {
          Command.Execute(CommandParameter);
        }
      }
      base.OnPreviewMouseLeftButtonDown(e);
    }

    #endregion

    #endregion

    #region command

    private void HookUpCommand(ICommand oldCommand, ICommand newCommand)
    {
      if (oldCommand != null)
      {
        RemoveCommand(oldCommand, newCommand);
      }

      AddCommand(oldCommand, newCommand);
    }

    private void RemoveCommand(ICommand oldCommand, ICommand newCommand)
    {
      //there's nothing to do really if CanExecute changes - the listview should still be enabled
      //			oldCommand.CanExecuteChanged -= CanExecuteChanged;
    }

    private void AddCommand(ICommand oldCommand, ICommand newCommand)
    {
      if (newCommand != null)
      {
        //				newCommand.CanExecuteChanged += CanExecuteChanged;
      }
    }

    private static void CommandPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      ((Menu) dependencyObject).HookUpCommand(e.OldValue as ICommand, e.NewValue as ICommand);
    }

    #endregion
  }
}