using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
namespace ProjectInfinity.Controls
{
  public class ListBox : System.Windows.Controls.ListBox, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    bool _firstTime = true;
    private Point _previousMousePoint;
    public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register(
                                                                                                  "ViewMode",
                                                                                                  typeof(string),
                                                                                                  typeof(ListBox),
                                                                                                  new FrameworkPropertyMetadata(null));


    /// <summary>
    /// Identifies the <see cref="ContextMenuCommand"/> property.
    /// </summary>
    public static readonly DependencyProperty ContextMenuCommandProperty = DependencyProperty.Register("ContextMenuCommand",
                                                                                            typeof(ICommand),
                                                                                            typeof(ListBox),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (ContextMenuCommandPropertyChanged)));

    /// <summary>
    /// Identifies the <see cref="ContextMenuCommandParameter"/> property.
    /// </summary>
    public static readonly DependencyProperty ContextMenuCommandParameterProperty = DependencyProperty.Register("ContextMenuCommandParameter",
                                                                                                     typeof(object),
                                                                                                     typeof(ListBox),
                                                                                                     new FrameworkPropertyMetadata
                                                                                                      (null,
                                                                                                       new PropertyChangedCallback
                                                                                                         (ContextMenuCommandParameterPropertyChanged)));
    /// <summary>
    /// Identifies the <see cref="ContextMenuCommandTarget"/> property.
    /// </summary>
    public static readonly DependencyProperty ContextMenuCommandTargetProperty = DependencyProperty.Register("ContextMenuCommandTarget",
                                                                                                  typeof(IInputElement),
                                                                                                  typeof(ListBox),
                                                                                                  new FrameworkPropertyMetadata
                                                                                                    (null));




    /// <summary>
    /// Identifies the <see cref="Command"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
                                                                                            typeof(ICommand),
                                                                                            typeof(ListBox),
                                                                                            new FrameworkPropertyMetadata
                                                                                              (null,
                                                                                               new PropertyChangedCallback
                                                                                                 (CommandPropertyChanged)));

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter",
                                                                                                     typeof(object),
                                                                                                     typeof(ListBox),
                                                                                                     new FrameworkPropertyMetadata
                                                                                                      (null,
                                                                                                       new PropertyChangedCallback
                                                                                                         (CommandParameterPropertyChanged)));
    /// <summary>
    /// Identifies the <see cref="CommandTarget"/> property.
    /// </summary>
    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget",
                                                                                                  typeof(IInputElement),
                                                                                                  typeof(ListBox),
                                                                                                  new FrameworkPropertyMetadata
                                                                                                    (null));

    public string ViewMode
    {
      get { return (string)GetValue(ViewModeProperty); }
      set { SetValue(ViewModeProperty, value); }
    }

    bool DidMouseMove
    {
      get
      {
        Point point = Mouse.GetPosition(Window.GetWindow(this));
        if (Math.Abs(point.X - _previousMousePoint.X) >= 10 || Math.Abs(point.Y - _previousMousePoint.Y) >= 10)
        {
          _previousMousePoint = point; 
          return true;
        }
        return false;
      }
    }

    protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
    {
      if (!IsInitialized || !IsLoaded) return;
      if (DidMouseMove == false) return;
      FrameworkElement element = Mouse.DirectlyOver as FrameworkElement;
      while (element != null)
      {
        if (element as ListBoxItem != null)
        {
          this.SelectedItem = element;
          //Trace.WriteLine(String.Format("{0} {1}", this.SelectedIndex, this.SelectedItem));
          Keyboard.Focus((System.Windows.Controls.ListBoxItem)element);
          for (int i = 0; i < Items.Count; ++i)
          {
            IInputElement inpElement = ItemContainerGenerator.ContainerFromIndex(i) as IInputElement;
            if (inpElement != null)
            {
              if (inpElement.IsKeyboardFocused)
              {
                this.SelectedIndex = i;
                break;
              }
            }
          }
          e.Handled = true;
          return;
        }
        element = element.TemplatedParent as FrameworkElement;
      }
      base.OnMouseMove(e);
    }
    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
      if ((e.Source as ListBox) == null) return;
      if (!IsInitialized || !IsLoaded) return;

      if (e.Key == System.Windows.Input.Key.Enter)
      {
        ListBox box = e.Source as ListBox;

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
      if (e.Key == System.Windows.Input.Key.F9)
      {
        ListBox box = e.Source as ListBox;

        //execute the command if there is one
        if (ContextMenuCommand != null)
        {
          RoutedCommand routedCommand = ContextMenuCommand as RoutedCommand;

          if (routedCommand != null)
          {
            routedCommand.Execute(ContextMenuCommandParameter, ContextMenuCommandTarget);
          }
          else
          {
            ContextMenuCommand.Execute(ContextMenuCommandParameter);
          }
        }
        e.Handled = true;
        return;
      }
      base.OnKeyDown(e);
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed)
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
      if (e.RightButton == MouseButtonState.Pressed)
      {
        //execute the command if there is one
        if (ContextMenuCommand != null)
        {
          RoutedCommand routedCommand = ContextMenuCommand as RoutedCommand;

          if (routedCommand != null)
          {
            routedCommand.Execute(ContextMenuCommandParameter, ContextMenuCommandTarget);
          }
          else
          {
            ContextMenuCommand.Execute(ContextMenuCommandParameter);
          }
        }
        e.Handled = true;
        return;
      }
    }
    protected override void OnInitialized(EventArgs e)
    {

      ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);
    }
    void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
    {
      if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
      {
        if (_firstTime == true)
        {
          _firstTime = false;
          if (SelectedIndex >= 0)
          {
            DependencyObject obj = ItemContainerGenerator.ContainerFromIndex(SelectedIndex);
            if (this.IsKeyboardFocused || this.IsKeyboardFocusWithin || this.IsFocused)
            {
              //Trace.WriteLine("Listbox:focus itemgen" + SelectedIndex.ToString());
              Keyboard.Focus((ListBoxItem)obj);
            }
          }
        }
      }
    }
    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
      if (e.NewFocus == this)
      {
        e.Handled = true;
        if (SelectedIndex >= 0)
        {
          DependencyObject obj = ItemContainerGenerator.ContainerFromIndex(SelectedIndex);
          if (this.IsKeyboardFocused || this.IsKeyboardFocusWithin || this.IsFocused)
          {
            //Trace.WriteLine("Listbox:focus item" + SelectedIndex.ToString());
            Keyboard.Focus((ListBoxItem)obj);
          }
        }
        return;

      }
      base.OnGotKeyboardFocus(e);
      //Trace.WriteLine("Listbox:OnGotKeyboardFocus" + e.NewFocus);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
      //Trace.WriteLine("OnSelectionChanged "+SelectedIndex.ToString());
      ICurrentItem currentItem = ItemsSource as ICurrentItem;
      if (currentItem != null)
      {
        currentItem.CurrentItem = SelectedItem;
      }
      
      base.OnSelectionChanged(e);
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs("SelectedItem"));
      }
    }

    #region command
    /// <summary>
    /// Gets or sets the <see cref="ICommand"/> to execute whenever an item is activated.
    /// </summary>
    public ICommand Command
    {
      get { return GetValue(CommandProperty) as ICommand; }
      set
      {
        SetValue(CommandProperty, value);
      }
    }

    /// <summary>
    /// Gets or sets the parameter to be passed to the executed <see cref="Command"/>.
    /// </summary>
    public object CommandParameter
    {
      get
      {
        return GetValue(CommandParameterProperty);
      }
      set
      {
        SetValue(CommandParameterProperty, value);
      }
    }
    private static void CommandParameterPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      //(dependencyObject as ListBox).CommandParameter = (object)(e.NewValue);
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
      (dependencyObject as ListBox).HookUpCommand(e.OldValue as ICommand, e.NewValue as ICommand);
    }
    #endregion

    #region context menucommand
    /// <summary>
    /// Gets or sets the <see cref="ICommand"/> to execute whenever an item is activated.
    /// </summary>
    public ICommand ContextMenuCommand
    {
      get { return GetValue(ContextMenuCommandProperty) as ICommand; }
      set
      {
        SetValue(ContextMenuCommandProperty, value);
      }
    }

    /// <summary>
    /// Gets or sets the parameter to be passed to the executed <see cref="Command"/>.
    /// </summary>
    public object ContextMenuCommandParameter
    {
      get
      {
        return GetValue(ContextMenuCommandParameterProperty);
      }
      set
      {
        SetValue(ContextMenuCommandParameterProperty, value);
      }
    }
    private static void ContextMenuCommandParameterPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      //(dependencyObject as ListBox).CommandParameter = (object)(e.NewValue);
    }

    /// <summary>
    /// Gets or sets the element on which to raise the specified <see cref="Command"/>.
    /// </summary>
    public IInputElement ContextMenuCommandTarget
    {
      get { return GetValue(ContextMenuCommandTargetProperty) as IInputElement; }
      set { SetValue(ContextMenuCommandTargetProperty, value); }
    }


    private static void ContextMenuCommandPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      (dependencyObject as ListBox).ContextMenuCommand = (e.NewValue as ICommand);
    }
    #endregion
  }
}
