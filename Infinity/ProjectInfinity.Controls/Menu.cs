using System;
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
  public class Menu : System.Windows.Controls.Canvas
  {
    const int BUTTONHEIGHT = 40;
    #region properties
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

    #region variables
    int _currentOffset;
    Storyboard _storyBoard;
    #endregion

    #region ctor
    public Menu()
    {
      this.Loaded += new RoutedEventHandler(Menu_Loaded);
    }
    #endregion


    #region event handlers
    void Menu_Loaded(object sender, RoutedEventArgs e)
    {
      LayoutMenu();
    }
    #endregion

    #region property handlers
    private static void FocusedMarginPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      //  (dependencyObject as Menu).FocusedMargin = (int)(e.NewValue);
    }

    public Thickness FocusedMargin
    {
      get
      {
        return (Thickness)GetValue(FocusedMarginProperty);
      }
      set
      {
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
        SetValue(ItemsSourceProperty, value);
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
        SetValue(ItemTemplateProperty, value);
      }
    }
    #endregion

    #region event handlers
    void LayoutMenu()
    {
      if (ItemsSource == null) return;
      int maxRows = (int)(this.ActualHeight / ((double)BUTTONHEIGHT));
      maxRows--;
      if (_storyBoard != null)
      {
        _storyBoard.Seek(this, new TimeSpan(0), TimeSeekOrigin.BeginTime);
        _storyBoard.Remove(this);
        _storyBoard = null;
      }
      this.Children.Clear();
      this.Margin = new Thickness(0, 0, 0, 0);
      double yoffset = -BUTTONHEIGHT;
      int selected = (maxRows) / 2;
      for (int i = -1; i < maxRows + 2; ++i)
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
          this.FocusedMargin = new Thickness(0, yoffset, 0, 0);
        }

        menuItem.Content.Width = this.ActualWidth;
        this.Children.Add((UIElement)menuItem.Content);
        yoffset += BUTTONHEIGHT;
      }
      Keyboard.Focus(this.Children[selected]);
    }
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
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
    void OnScrollDown()
    {
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
    }

    void storyBoard_Completed(object sender, EventArgs e)
    {
      LayoutMenu();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      LayoutMenu();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
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
    }
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
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
      base.OnPreviewMouseDown(e);
    }
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
