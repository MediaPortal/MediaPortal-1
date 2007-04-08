using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TvDatabase;
using TvControl;
using Dialogs;
using MCEControls;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvNewSchedule.xaml
  /// </summary>

  public partial class TvSearch : System.Windows.Controls.Page
  {
    TvSearchViewModel _model;
    public TvSearch()
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
      _model = new TvSearchViewModel(this);
      gridMain.DataContext = _model;
      Keyboard.Focus(textboxSearch);
      this.InputBindings.Add(new KeyBinding(_model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onPreviewKeyDown));
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(OnMouseMoveEvent));
      this.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(OnMouseButtonDownEvent), true);
      this.KeyDown += new KeyEventHandler(onKeyDown);


    }
    /// <summary>
    /// Event handler for mouse events
    /// When mouse enters an control, this method will give the control keyboardfocus
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
    void OnMouseMoveEvent(object sender, MouseEventArgs e)
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
    /// <summary>
    /// Event handler for OnKeyDown
    /// Handles some basic navigation
    /// Guess this should be done via command binding?
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    protected void onPreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.X)
      {
        ICommand command = _model.FullScreenTv;
        if (command.CanExecute(this))
        {
          command.Execute(this);
          e.Handled = true;
        }
        return;
      }
    }
    /// <summary>
    /// Handles the KeyDown event 
    /// When keydown=enter, OnRecordingClicked() gets called
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    void onKeyDown(object sender, KeyEventArgs e)
    {
      if ((e.Source as ListBox) == null) return;
      if (e.Key == System.Windows.Input.Key.Enter)
      {
        ListBox box = e.Source as ListBox;
        OnProgramClicked(box);
        e.Handled = true;
        return;
      }
    }
    /// <summary>
    /// Handles the mouse button down event
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnMouseButtonDownEvent(object sender, RoutedEventArgs e)
    {
      if ((e.Source as ListBox) == null) return;
      ListBox box = e.Source as ListBox;
      OnProgramClicked(box);
    }


    void OnProgramClicked(ListBox box)
    {
      ProgramModel model = box.SelectedItem as ProgramModel;
      TvProgramInfo info = new TvProgramInfo(model.Program);
      ServiceScope.Get<INavigationService>().Navigate(info);
    }

  }
}