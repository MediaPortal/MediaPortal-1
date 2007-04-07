//
// TODO: - View Film strip, list, icon
//       - Switch view
//       - Playlist
//

using System;
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

using ProjectInfinity;
using ProjectInfinity.Menu;
using ProjectInfinity.Messaging;

namespace MyVideos
{
  /// <summary>
  /// Interaction logic for TestPage.xaml
  /// </summary>

  public partial class VideoHome : System.Windows.Controls.Page
  {
    VideoHomeViewModel _model;

    public VideoHome()
    {
      InitializeComponent();

      ServiceScope.Get<IMessageBroker>().Register(this);
    }

    /// <summary>
    /// When the page is loaded, this event is called to do some
    /// basic things like viewmodels, keybindings etc etc
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // View Model
      _model = new VideoHomeViewModel(this);
      gridMain.DataContext = _model;

      // Keyboard
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      Keyboard.Focus(buttonView);

      // Mouse
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(OnMouseMoveEvent));

      this.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(OnMouseButtonDownEvent), true);
      this.KeyDown += new KeyEventHandler(onKeyDown);
    }

    /// <summary>
    /// When we click the video window, this event is called
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnVideoWindowClicked(object sender, EventArgs args)
    {
      this.NavigationService.Navigate(new Uri("/MyVideos;component/VideoFullscreen.xaml", UriKind.Relative));
    }

    /// <summary>
    /// Handles mouse clicks
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMouseButtonDownEvent(object sender, RoutedEventArgs e)
    {
      if ((e.Source as ListBox) != null)
      {
        ListBox box = (ListBox)e.Source;
        OnVideoItemClicked(box);
        e.Handled = true;

        return;
      }
    }

    /// <summary>
    /// Handles mouse moves, focusing of objects etc
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMouseMoveEvent(object sender, MouseEventArgs e)
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
    /// When a button gets pressed, this event is called
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      // If we've pressed enter, call for VideoItemClicked()
      if ((e.Key == Key.Enter) &&
          (e.Source as ListBox) != null)
      {
        ListBox box = (ListBox)e.Source;
        OnVideoItemClicked(box);
        e.Handled = true;

        return;
      }
    }

    /// <summary>
    /// Called when a user clicks on a video in the listbox.
    /// </summary>
    /// <param name="listBox"></param>
    private void OnVideoItemClicked(ListBox listBox)
    {
      VideoModel item = (VideoModel)listBox.SelectedItem;

      ICommand playCommand = _model.Play;
      playCommand.Execute(item);

      OnVideoWindowClicked(null, new EventArgs());
    }
  }
}