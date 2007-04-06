using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using Dialogs;
using TvControl;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvScheduled.xaml
  /// </summary>

  public partial class TvScheduled : System.Windows.Controls.Page
  {
    TvScheduledViewModel _model;
    public TvScheduled()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      _model = new TvScheduledViewModel(this);
      gridMain.DataContext = _model;

      //this.InputBindings.Add(new KeyBinding(_model.FullScreenTv, new KeyGesture(System.Windows.Input.Key.X, ModifierKeys.None)));
      this.InputBindings.Add(new KeyBinding(_model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

      // Sets keyboard focus on the first Button in the sample.
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(OnPreviewKeyDown));
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(OnMouseMove));
      Keyboard.Focus(buttonSort);
      this.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(OnMouseDownEvent), true);
      this.KeyDown += new KeyEventHandler(OnKeyDown);
    }

    void OnMouseMove(object sender, MouseEventArgs e)
    {
      FrameworkElement element = Mouse.DirectlyOver as FrameworkElement;
      while (element != null)
      {
        if (element as Button != null)
        {
          Keyboard.Focus((Button)element);
          return;
        }
        if (element as ListBoxItem != null)
        {
          Keyboard.Focus((ListBoxItem)element);
          return;
        }
        element = element.TemplatedParent as FrameworkElement;
      }
    }
    void OnKeyDown(object sender, KeyEventArgs e)
    {
      if ((e.Source as ListBox) == null) return;
      if (e.Key == System.Windows.Input.Key.Enter)
      {
        ListBox box = e.Source as ListBox;
        ICommand cmd = _model.ContextMenu;
        cmd.Execute(box.SelectedItem);
        e.Handled = true;
        return;
      }
    }
    void OnMouseDownEvent(object sender, RoutedEventArgs e)
    {
      if ((e.Source as ListBox) == null) return;
      ListBox box = e.Source as ListBox;
      ICommand cmd = _model.ContextMenu;
      cmd.Execute(box.SelectedItem);
    }
    protected void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Left)
      {
        Keyboard.Focus(buttonSort);
        e.Handled = true;
        return;
      }
      if (e.Key == System.Windows.Input.Key.X)
      {
        if (TvPlayerCollection.Instance.Count > 0)
        {
          this.NavigationService.Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
          return;
        }
      }
    }

  }
}