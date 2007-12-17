using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectInfinity.Controls
{
  public class DataGrid : System.Windows.Controls.Grid
  {
    #region variables and properties
    /// <summary>
    /// Identifies the <see cref="Command"/> property.
    /// </summary>
    public static readonly DependencyProperty ScrollProperty = DependencyProperty.Register("Scroll",
                                                                                            typeof(ICommand),
                                                                                            typeof(DataGrid),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (ScrollPropertyChanged)));

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly DependencyProperty ScrollParameterProperty = DependencyProperty.Register("ScrollParameter",
                                                                                                     typeof(object),
                                                                                                     typeof(DataGrid),
                                                                                                     new FrameworkPropertyMetadata
                                                                                                       (null));

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate",
                                                                                            typeof(DataTemplate),
                                                                                            typeof(DataGrid),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (ItemTemplatePropertyChanged)));

    public static readonly DependencyProperty ItemTemplateHeaderProperty = DependencyProperty.Register("ItemTemplateHeader",
                                                                                            typeof(DataTemplate),
                                                                                            typeof(DataGrid),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (ItemTemplateHeaderPropertyChanged)));

    public static readonly DependencyProperty ItemTemplateLeftProperty = DependencyProperty.Register("ItemTemplateLeft",
                                                                                                typeof(DataTemplate),
                                                                                                typeof(DataGrid),
                                                                                                new FrameworkPropertyMetadata
                                                                                                  (null,
                                                                                                   new PropertyChangedCallback
                                                                                                     (ItemTemplateLeftPropertyChanged)));

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
                                                                                            typeof(DataGridCollection),
                                                                                            typeof(DataGrid),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (ItemsSourcePropertyChanged)));


    public static readonly RoutedEvent SelectedItemChanged = EventManager.RegisterRoutedEvent("SelectedItemChanged",
                                                                                             RoutingStrategy.Bubble,
                                                                                             typeof(
                                                                                               EventHandler
                                                                                               <SelectedItemChangedEventArgs>),
                                                                                             typeof(DataGrid));

    /// <summary>
    /// Identifies the <see cref="Command"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
                                                                                            typeof(ICommand),
                                                                                            typeof(DataGrid),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (CommandPropertyChanged)));

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter",
                                                                                                     typeof(object),
                                                                                                     typeof(DataGrid),
                                                                                                     new FrameworkPropertyMetadata
                                                                                                       (null));
    /// <summary>
    /// Identifies the <see cref="CommandTarget"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget",
                                                                                                  typeof(IInputElement),
                                                                                                  typeof(DataGrid),
                                                                                                  new FrameworkPropertyMetadata
                                                                                                    (null));
    #endregion

    public DataGrid()
    {
      this.Loaded += new RoutedEventHandler(DataGrid_Loaded);
    }

    void DataGrid_Loaded(object sender, RoutedEventArgs e)
    {
      UpdateGrid();
    }

    /// <summary>
    /// Gets or sets the items source.
    /// </summary>
    /// <value>The items source.</value>
    public DataGridCollection ItemsSource
    {
      get
      {
        return GetValue(ItemsSourceProperty) as DataGridCollection;
      }
      set
      {
        SetValue(ItemsSourceProperty, value);
        ItemsSource.DataGrid = this;
        //UpdateGrid();
      }
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
       // UpdateGrid();
      }
    }
    public DataTemplate ItemTemplateHeader
    {
      get
      {
        return GetValue(ItemTemplateHeaderProperty) as DataTemplate;
      }
      set
      {
        SetValue(ItemTemplateHeaderProperty, value);
       // UpdateGrid();
      }
    }
    public DataTemplate ItemTemplateLeft
    {
      get
      {
        return GetValue(ItemTemplateLeftProperty) as DataTemplate;
      }
      set
      {
        SetValue(ItemTemplateLeftProperty, value);
       // UpdateGrid();
      }
    }


    /// <summary>
    /// called when itemssource property is changed
    /// </summary>
    /// <param name="dependencyObject">The dependency object.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void ItemsSourcePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as DataGrid).ItemsSource = (e.NewValue as DataGridCollection);
    }
    private static void ItemTemplatePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as DataGrid).ItemTemplate = (e.NewValue as DataTemplate);
    }
    private static void ItemTemplateHeaderPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as DataGrid).ItemTemplateHeader = (e.NewValue as DataTemplate);
    }
    private static void ItemTemplateLeftPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as DataGrid).ItemTemplateLeft = (e.NewValue as DataTemplate);
    }

    /// <summary>
    /// Called when the selected item changes
    /// </summary>
    /// <param name="item">The item.</param>
    public void OnItemSelected(DataGridCell item)
    {
      ItemsSource.CurrentItem = item;
      RaiseEvent(new SelectedItemChangedEventArgs(SelectedItemChanged, item));
    }

    /// <summary>
    /// Updates the grid so it shows the collection.
    /// </summary>
    public void UpdateGrid()
    {
      this.Children.Clear();
      this.RowDefinitions.Clear();
      if (ItemsSource == null) return;
      int rowNr = 0;
      foreach (DataGridRow row in ItemsSource)
      {
        this.RowDefinitions.Add(new RowDefinition());
        row.DataGrid = this;
        int columnNr = 0;
        foreach (DataGridCell cell in row.Cells)
        {
          cell.DataGrid = this;

          bool templateApplied = false;
          if (rowNr == 0)
          {
            if (ItemTemplateHeader != null)
            {
              templateApplied = true;
              cell.Content = (FrameworkElement)ItemTemplateHeader.LoadContent();
            }
          }

          if (columnNr == 0 && !templateApplied)
          {
            if (ItemTemplateLeft != null)
            {
              templateApplied = true;
              cell.Content = (FrameworkElement)ItemTemplateLeft.LoadContent();
            }
          }

          if (ItemTemplate != null && !templateApplied)
          {
            templateApplied = true;
            cell.Content = (FrameworkElement)ItemTemplate.LoadContent();
          }

          SetRow(cell.Content, rowNr);
          if (row.RowSpan >= 1)
            SetRowSpan(cell.Content, row.RowSpan);

          if (cell.Column >= 0)
            SetColumn(cell.Content, cell.Column);
          else
            SetColumn(cell.Content, columnNr);

          if (cell.ColumnSpan >= 1)
            SetColumnSpan(cell.Content, cell.ColumnSpan);

          cell.Content.DataContext = cell;
          this.Children.Add(cell.Content);

          columnNr++;
        }
        rowNr++;
      }
    }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    /// <value>The selected item.</value>
    public DataGridCell SelectedItem
    {
      get
      {
        if (ItemsSource != null) return null;
        return ItemsSource.CurrentItem;
      }
      set
      {

        if (ItemsSource != null) return;
        ItemsSource.CurrentItem = value;
      }
    }

    /// <summary>
    /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseDown"></see> attached routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"></see> that contains the event data. The event data reports that one or more mouse buttons were pressed.</param>
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
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

    /// <summary>
    /// Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown"></see> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs"></see> that contains the event data.</param>
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
      if (e.Key == Key.Down)
      {
        if (SelectedRow + 1 >= ItemsSource.Count)
        {
          ScrollDirection = "Down";
          e.Handled = true;
          return;
        }
      }
      if (e.Key == Key.Up)
      {
        if (SelectedRow - 1 < 1)
        {
          ScrollDirection = "Up";
          e.Handled = true;
          return;
        }
      }
      if (e.Key == Key.Right && SelectedRow >= 0)
      {
        if (SelectedColumn + 1 >= ItemsSource[SelectedRow].Cells.Count)
        {
          ScrollDirection = "Right";
          e.Handled = true;
          return;
        }
      }
      if (e.Key == Key.Left)
      {
        if (SelectedColumn - 1 < 0)
        {
          ScrollDirection = "Left";
          e.Handled = true;
          return;
        }
      }
      base.OnPreviewKeyDown(e);
    }
    /// <summary>
    /// Gets the selected row.
    /// </summary>
    /// <value>The selected row.</value>
    public int SelectedRow
    {
      get
      {
        if (ItemsSource == null) return -1;
        int rowNr = 0;
        foreach (DataGridRow row in ItemsSource)
        {
          foreach (DataGridCell cell in row.Cells)
          {
            if (cell.Content != null)
            {
              if (cell.Content.IsKeyboardFocused)
              {
                return rowNr;
              }
            }
          }
          rowNr++;
        }
        return -1;
      }
    }

    /// <summary>
    /// Gets the selected column.
    /// </summary>
    /// <value>The selected column.</value>
    public int SelectedColumn
    {
      get
      {
        if (ItemsSource == null) return -1;
        foreach (DataGridRow row in ItemsSource)
        {
          int cellNr = 0;
          foreach (DataGridCell cell in row.Cells)
          {
            if (cell.Content != null)
            {
              if (cell.Content.IsKeyboardFocused)
              {
                return cellNr;
              }
            }
            cellNr++;
          }
        }
        return -1;
      }
    }

    /// <summary>
    /// Gets or sets the scroll direction.
    /// </summary>
    /// <value>The scroll direction.</value>
    public string ScrollDirection
    {
      get
      {
        return ScrollParameter as string;
      }
      set
      {
        ScrollParameter = value;
        if (Scroll != null)
        {
          RoutedCommand routedCommand = Scroll as RoutedCommand;
          if (routedCommand != null)
          {
            routedCommand.Execute(ScrollParameter, null);
          }
          else
          {
            Scroll.Execute(ScrollParameter);
          }
        }
      }
    }

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
      (dependencyObject as DataGrid).HookUpCommand(e.OldValue as ICommand, e.NewValue as ICommand);
    }
    #endregion


    #region scroll
    /// <summary>
    /// Gets or sets the <see cref="ICommand"/> to execute whenever an item is activated.
    /// </summary>
    public ICommand Scroll
    {
      get { return GetValue(ScrollProperty) as ICommand; }
      set { SetValue(ScrollProperty, value); }
    }

    /// <summary>
    /// Gets or sets the parameter to be passed to the executed <see cref="Command"/>.
    /// </summary>
    public object ScrollParameter
    {
      get { return GetValue(ScrollParameterProperty); }
      set { SetValue(ScrollParameterProperty, value); }
    }



    private void HookUpScroll(ICommand oldScroll, ICommand newScroll)
    {
      if (oldScroll != null)
      {
        RemoveScroll(oldScroll, newScroll);
      }
      AddScroll(oldScroll, newScroll);
    }

    private void RemoveScroll(ICommand oldScroll, ICommand newScroll)
    {
    }

    private void AddScroll(ICommand oldScroll, ICommand newScroll)
    {
    }

    private static void ScrollPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as DataGrid).HookUpScroll(e.OldValue as ICommand, e.NewValue as ICommand);
    }
    #endregion
  }
}
