using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ProjectInfinity.Controls
{
  public class Menu : System.Windows.Controls.Canvas, INotifyPropertyChanged
  {
    const int BUTTONHEIGHT = 40;
    public event PropertyChangedEventHandler PropertyChanged;

    #region properties
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
                                                                                            typeof(MenuItem),
                                                                                            typeof(Menu),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (SelectedItemPropertyChanged)));

    public static readonly DependencyProperty FocusElementProperty = DependencyProperty.Register("FocusElement",
                                                                                            typeof(FrameworkElement),
                                                                                            typeof(Menu),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (FocusElementPropertyChanged)));

    public static readonly DependencyProperty FocusedVisibleProperty = DependencyProperty.Register("FocusedVisible",
                                                                                            typeof(object),
                                                                                            typeof(Menu),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (FocusedVisiblePropertyChanged)));

    public static readonly DependencyProperty FocusedHiddenProperty = DependencyProperty.Register("FocusedHidden",
                                                                                            typeof(object),
                                                                                            typeof(Menu),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (FocusedHiddenPropertyChanged)));


    public static readonly DependencyProperty FocusedMarginProperty = DependencyProperty.Register("FocusedMargin",
                                                                                            typeof(object),
                                                                                            typeof(Menu),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (FocusedMarginPropertyChanged)));


    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate",
                                                                                            typeof(DataTemplate),
                                                                                            typeof(Menu),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (ItemTemplatePropertyChanged)));

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
                                                                                            typeof(MenuCollection),
                                                                                            typeof(Menu),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (ItemsSourcePropertyChanged)));

    public static readonly DependencyProperty SubMenuProperty = DependencyProperty.Register("SubMenu",
                                                                                            typeof(MenuCollection),
                                                                                            typeof(Menu),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (SubMenuPropertyChanged)));

    /// <summary>
    /// Identifies the <see cref="Command"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
                                                                                            typeof(ICommand),
                                                                                            typeof(Canvas),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (CommandPropertyChanged)));

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter",
                                                                                                     typeof(object),
                                                                                                     typeof(Canvas),
                                                                                                     new FrameworkPropertyMetadata
                                                                                                       (null));
    /// <summary>
    /// Identifies the <see cref="CommandTarget"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget",
                                                                                                  typeof(IInputElement),
                                                                                                  typeof(Canvas),
                                                                                                  new FrameworkPropertyMetadata
                                                                                                    (null));
    #endregion

    #region routed events
    public static readonly RoutedEvent FocusVisibleEvent = EventManager.RegisterRoutedEvent("OnFocusVisible", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Menu));
    public static readonly RoutedEvent FocusHiddenEvent = EventManager.RegisterRoutedEvent("OnFocusHidden", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Menu));
    #endregion

    #region variables
    int _currentOffset;
    Storyboard _storyBoard;
    bool _mouseEntered = false;
    int _mouseSelectedItem = -1;
    int _currentSelectedItem = 0;
    bool _mouseEventsEnabled = true;
    bool _waitForMouseMove = false;
    Point _previousMousePoint;
    double _yOffset = 0;
    #endregion

    #region ctor
    public Menu()
    {
      FocusedHidden = Visibility.Hidden;
      FocusedVisible = Visibility.Hidden;
      FocusedMargin = new Thickness(0);
      this.Loaded += new RoutedEventHandler(Menu_Loaded);
      Keyboard.AddPreviewLostKeyboardFocusHandler(this, new KeyboardFocusChangedEventHandler(onpreviewKeyboardLost));
      Keyboard.AddPreviewGotKeyboardFocusHandler(this, new KeyboardFocusChangedEventHandler(onpreviewKeyboardGot));
    }
    #endregion


    #region routed events handlers
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
    #endregion

    #region property handlers
    private static void FocusedVisiblePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      //(dependencyObject as Menu).FocusedVisible = (Visibility)(e.NewValue);
    }

    public object FocusedVisible
    {
      get
      {
        if (GetValue(FocusedVisibleProperty) == null) return Visibility.Hidden;
        return (Visibility)GetValue(FocusedVisibleProperty);
      }
      set
      {
        //Trace.WriteLine("Set FocusedVisible");
        if (value != FocusedVisible)
        {
          Visibility vis = (Visibility)value;
          SetValue(FocusedVisibleProperty, value);
          RoutedEventArgs args = new RoutedEventArgs();
          if (vis == Visibility.Visible)
            args.RoutedEvent = Menu.FocusVisibleEvent;
          else
            args.RoutedEvent = Menu.FocusHiddenEvent;
          args.Source = this;
          RaiseEvent(args);
          if (vis == Visibility.Visible)
            FocusedHidden = Visibility.Hidden;
          else
            FocusedHidden = Visibility.Visible;
        }
      }
    }

    private static void FocusedHiddenPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      //(dependencyObject as Menu).FocusedHidden = (Visibility)(e.NewValue);
    }

    public object FocusedHidden
    {
      get
      {
        if (GetValue(FocusedHiddenProperty) == null) return Visibility.Hidden;
        return (Visibility)GetValue(FocusedHiddenProperty);
      }
      set
      {
        //Trace.WriteLine("Set FocusedHidden");
        SetValue(FocusedHiddenProperty, value);
      }
    }
    private static void SelectedItemPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      //(dependencyObject as Menu).FocusElement = (MenuItem)(e.NewValue);
    }

    public MenuItem SelectedItem
    {
      get
      {
        return (MenuItem)GetValue(SelectedItemProperty);
      }
      set
      {
        //Trace.WriteLine("Set SelectedItem");
        SetValue(SelectedItemProperty, value);
      }
    }
    private static void FocusElementPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as Menu).FocusElement = (FrameworkElement)(e.NewValue);
    }

    public FrameworkElement FocusElement
    {
      get
      {
        return (FrameworkElement)GetValue(FocusElementProperty);
      }
      set
      {
        //Trace.WriteLine("Set FocusElement");
        SetValue(FocusElementProperty, value);
      }
    }

    private static void FocusedMarginPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      //  (dependencyObject as Menu).FocusedMargin = (int)(e.NewValue);
    }

    public object FocusedMargin
    {
      get
      {
        return GetValue(FocusedMarginProperty);
      }
      set
      {
        //Trace.WriteLine("Set FocusedMargin");
        SetValue(FocusedMarginProperty, value);
      }
    }




    private static void ItemsSourcePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as Menu).ItemsSource = (e.NewValue as MenuCollection);
    }

    public MenuCollection ItemsSource
    {
      get
      {
        return GetValue(ItemsSourceProperty) as MenuCollection;
      }
      set
      {
        //Trace.WriteLine("Set ItemsSource");
        SetValue(ItemsSourceProperty, value);
      }
    }


    private static void SubMenuPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as Menu).SubMenu = (e.NewValue as MenuCollection);
    }

    public MenuCollection SubMenu
    {
      get
      {
        return GetValue(SubMenuProperty) as MenuCollection;
      }
      set
      {
        //Trace.WriteLine("Set SubMenu");
        SetValue(SubMenuProperty, value);
      }
    }


    private static void ItemTemplatePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as Menu).ItemTemplate = (e.NewValue as DataTemplate);
    }
    /// <summary>
    /// Gets or sets the item template.
    /// </summary>
    /// <value>The item template.</value>
    public DataTemplate ItemTemplate
    {
      get
      {
        return GetValue(ItemTemplateProperty) as DataTemplate;
      }
      set
      {
        //Trace.WriteLine("Set ItemTemplate");
        SetValue(ItemTemplateProperty, value);
      }
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

    void onpreviewKeyboardLost(object sender, KeyboardFocusChangedEventArgs e)
    {
      if (this.IsKeyboardFocusWithin == false)
      {
        return;
      }
      FocusedVisible = Visibility.Hidden;
      //Trace.WriteLine(String.Format("p lost {0} {1}->{2} {3}", this.IsKeyboardFocusWithin, e.OldFocus, e.NewFocus, e.RoutedEvent));
    }
    void onpreviewKeyboardGot(object sender, KeyboardFocusChangedEventArgs e)
    {
      if (this.IsKeyboardFocusWithin)
      {
        return;
      }
      FocusedVisible = Visibility.Visible;
      //Trace.WriteLine(String.Format("p got {0} {1}->{2} {3}", this.IsKeyboardFocusWithin, e.OldFocus, e.NewFocus, e.RoutedEvent));
    }
    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
      //Trace.WriteLine("OnGotKeyboardFocus");

      base.OnGotKeyboardFocus(e);
    }
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
      //Trace.WriteLine("OnLostKeyboardFocus");
      base.OnLostKeyboardFocus(e);
    }
    void Menu_Loaded(object sender, RoutedEventArgs e)
    {
      //Trace.WriteLine("Menu_Loaded");
      LayoutMenu();
    }


    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      //Trace.WriteLine("OnRenderSizeChanged");
      base.OnRenderSizeChanged(sizeInfo);

      if (IsLoaded)
        LayoutMenu();
    }
    void LayoutMenu()
    {
      bool gotKeyboardFocus = IsFocused || IsKeyboardFocused || IsKeyboardFocusWithin;
      //Trace.WriteLine("LayoutMenu");
      _mouseEventsEnabled = false;

      if (ItemsSource == null) return;
      int maxRows = (int)(this.ActualHeight / ((double)BUTTONHEIGHT));
      //maxRows--;
      if (_storyBoard != null)
      {
        _storyBoard.Stop(this);
        _storyBoard.Remove(this);
        _storyBoard = null;
      }
      this.Children.Clear();
      this.Margin = new Thickness(0, 0, 0, 0);
      double yoffset = -BUTTONHEIGHT;
      int maxItems = maxRows;
      if (maxItems >= ItemsSource.Count)
        maxItems = ItemsSource.Count;
      int selected = (maxItems) / 2;
      if (_mouseEntered)
        selected = _mouseSelectedItem;
      else if (_mouseSelectedItem >= 0)
        selected = _mouseSelectedItem;

      if (selected >= maxItems + 2)
      {
        selected = (maxItems) / 2;
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
        yoffset = _yOffset = ((maxRows - ItemsSource.Count) * BUTTONHEIGHT) / 2;
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
        menuItem.Content = (FrameworkElement)ItemTemplate.LoadContent();
        menuItem.Content.DataContext = menuItem;
        menuItem.Content.Margin = new Thickness(0, yoffset, 0, 0);
        if (selected == i)
        {
          ItemsSource.CurrentItem = menuItem;
          SelectedItem = menuItem;
          this.FocusedMargin = new Thickness(0, yoffset, 0, 0);

          SubMenu = menuItem.SubMenus;
        }

        menuItem.Content.Width = this.ActualWidth;
        this.Children.Add((UIElement)menuItem.Content);
        yoffset += BUTTONHEIGHT;
      }
      if (gotKeyboardFocus)
      {
        Keyboard.Focus(this.Children[selected]);
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
    void OnScrollDown()
    {
      //Trace.WriteLine("OnScrollDown");
      _mouseEventsEnabled = false;

      int maxRows = (int)(this.ActualHeight / ((double)BUTTONHEIGHT));
      if (ItemsSource.Count < maxRows)
      {
        if (_currentSelectedItem >0)
          _mouseSelectedItem = _currentSelectedItem - 1;
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
      FrameworkElement element = (FrameworkElement)((FrameworkElement)this.Parent).Parent;
      _storyBoard = (Storyboard)element.Resources["storyBoardScrollDown"];
      _storyBoard.Completed += new EventHandler(storyBoard_Completed);
      _storyBoard.Begin(this, true);

        _currentOffset--;
      if (_currentOffset < 0) _currentOffset += ItemsSource.Count;
    }

    void OnScrollUp()
    {
      //Trace.WriteLine("OnScrollUp");
      _mouseEventsEnabled = false;
      int maxRows = (int)(this.ActualHeight / ((double)BUTTONHEIGHT));
      if (ItemsSource.Count < maxRows)
      {
        if (_currentSelectedItem+1 < ItemsSource.Count)
          _mouseSelectedItem = _currentSelectedItem + 1;
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
      FrameworkElement element = (FrameworkElement)((FrameworkElement)this.Parent).Parent;
      _storyBoard = (Storyboard)element.Resources["storyBoardScrollUp"];
      _storyBoard.Completed += new EventHandler(storyBoard_Completed);
      _storyBoard.Begin(this, true);

      _currentOffset++;
      if (_currentOffset >= ItemsSource.Count) _currentOffset -= ItemsSource.Count;
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
    void storyBoard_Completed(object sender, EventArgs e)
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
    void ScrollFocusedItemToMousePosition()
    {
      //Trace.WriteLine("ScrollFocusedItemToMousePosition");
      _mouseEventsEnabled = false;
      double offsetCurrent = _currentSelectedItem * BUTTONHEIGHT + _yOffset;
      double offsetNext = _mouseSelectedItem * BUTTONHEIGHT + _yOffset;
      ThicknessAnimation animation = new ThicknessAnimation(new Thickness(0, offsetCurrent, 0, 0), new Thickness(0, offsetNext, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 150)));

      animation.Completed += new EventHandler(animation_Completed);
      FocusElement.BeginAnimation(FrameworkElement.MarginProperty, animation);
    }

    void animation_Completed(object sender, EventArgs e)
    {
      //Trace.WriteLine("animation_Completed");
      LayoutMenu();
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
        int maxRows = (int)(this.ActualHeight / ((double)BUTTONHEIGHT));
        if (ItemsSource.Count < maxRows) selectedItemNr = 0;

        foreach (FrameworkElement element in this.Children)
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
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
      //Trace.WriteLine("OnPreviewMouseDown");
      base.OnPreviewMouseDown(e);
    }
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
      (dependencyObject as Menu).HookUpCommand(e.OldValue as ICommand, e.NewValue as ICommand);
    }
    #endregion
  }
}
