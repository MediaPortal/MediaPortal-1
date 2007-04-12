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
    /// <summary>
    /// Gets or sets the <see cref="ICommand"/> to execute whenever an item is activated.
    /// </summary>
    public DataGridCollection ItemsSource
    {
      get 
      {
        return GetValue(ItemsSourceProperty) as DataGridCollection; 
      }
      set 
      { 
        SetValue(ItemsSourceProperty, value);
        UpdateGrid();
      }
    }

    private static void ItemsSourcePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as DataGrid).ItemsSource = (e.NewValue as DataGridCollection);
    }

    public void OnItemSelected(DataGridCell item)
    {
      ItemsSource.CurrentItem = item;
      RaiseEvent(new SelectedItemChangedEventArgs(SelectedItemChanged, item));
    }

    public void UpdateGrid()
    {
      this.Children.Clear();
      if (ItemsSource == null) return;
      int rowNr = 0;
      foreach (DataGridRow row in ItemsSource)
      {
        row.DataGrid = this;
        int columnNr = 0;
        foreach (DataGridCell cell in row.Cells)
        {
          cell.DataGrid = this;
          SetRow(cell.Content, rowNr);
          if (row.RowSpan >= 1)
            SetRowSpan(cell.Content, row.RowSpan);

          if (cell.Column >= 0)
            SetColumn(cell.Content, cell.Column);
          else
            SetColumn(cell.Content, columnNr);

          if (cell.ColumnSpan >= 1)
            SetColumnSpan(cell.Content, cell.ColumnSpan);
          this.Children.Add(cell.Content);

          columnNr++;
        }
        rowNr++;
      }
    } 

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
      base.OnPreviewKeyDown(e);
    }
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
  }
}
