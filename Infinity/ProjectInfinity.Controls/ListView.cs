using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectInfinity.Controls
{
  /// <summary>
  /// Implements a customized <see cref="System.Windows.Controls.ListView"/>.
  /// </summary>
  public class ListView : System.Windows.Controls.ListView, ICommandSource
  {
    /// <summary>
    /// Identifies the <see cref="Command"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
                                                                                            typeof (ICommand),
                                                                                            typeof(ListView),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (CommandPropertyChanged)));

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter",
                                                                                                     typeof (object),
                                                                                                     typeof(ListView),
                                                                                                     new FrameworkPropertyMetadata
                                                                                                       (null));

    /// <summary>
    /// Identifies the <see cref="CommandTarget"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget",
                                                                                                  typeof (IInputElement),
                                                                                                  typeof(ListView),
                                                                                                  new FrameworkPropertyMetadata
                                                                                                    (null));

    /// <summary>
    /// Identifies the <see cref="ItemActivated"/> event.
    /// </summary>
    public static readonly RoutedEvent ItemActivatedEvent = EventManager.RegisterRoutedEvent("ItemActivated",
                                                                                             RoutingStrategy.Bubble,
                                                                                             typeof (
                                                                                               EventHandler
                                                                                               <ItemActivatedEventArgs>),
                                                                                             typeof(ListView));

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

    /// <summary>
    /// Occurs whenever an item in this <c>MediaList</c> is activated.
    /// </summary>
    public event EventHandler<ItemActivatedEventArgs> ItemActivated
    {
      add { AddHandler(ItemActivatedEvent, value); }
      remove { RemoveHandler(ItemActivatedEvent, value); }
    }

    /// <summary>
    /// Constructs an instance of the <c>MediaList</c> class.
    /// </summary>
    public ListView()
    {
      ItemContainerGenerator.StatusChanged += delegate { RePopulate(); };
      Loaded += delegate { RePopulate(); };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);

      //hitting enter activates an item too
      if ((e.Key == Key.Enter) && (SelectedItem != null))
      {
        OnItemActivated(SelectedItem);
      }
    }

    private void RePopulate()
    {
      //inefficient at best
      foreach (object item in Items)
      {
        ListViewItem listViewItem = ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;

        if (listViewItem == null)
        {
          continue;
        }

        listViewItem.MouseDoubleClick -= listViewItem_MouseDoubleClick;
        listViewItem.MouseDoubleClick += listViewItem_MouseDoubleClick;
      }
    }

    private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      object item = ItemContainerGenerator.ItemFromContainer(sender as DependencyObject);
      OnItemActivated(item);
    }

    private void OnItemActivated(object item)
    {
      RaiseEvent(new ItemActivatedEventArgs(ItemActivatedEvent, item));

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
      (dependencyObject as ListView).HookUpCommand(e.OldValue as ICommand, e.NewValue as ICommand);
    }
  }
}