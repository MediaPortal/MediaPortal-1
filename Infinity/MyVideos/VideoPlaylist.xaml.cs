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
using ProjectInfinity.Messaging;
using ProjectInfinity.Logging;

namespace MyVideos
{
  /// <summary>
  /// Interaction logic for VideoPlaylist.xaml
  /// </summary>

  public partial class VideoPlaylist : System.Windows.Controls.Page
  {
    VideoPlaylistViewModel _model;

    public VideoPlaylist()
    {
      InitializeComponent();

      ServiceScope.Get<IMessageBroker>().Register(this);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // View Model
      _model = new VideoPlaylistViewModel(this);
      gridMain.DataContext = _model;

      // Keyboard
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      Keyboard.Focus(buttonPartymix);

      // Mouse
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(OnMouseMoveEvent));

      this.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(OnMouseButtonDownEvent), true);
      this.KeyDown += new KeyEventHandler(onKeyDown);
    }

    private void OnMouseButtonDownEvent(object sender, RoutedEventArgs e)
    {
    }

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

    protected void onKeyDown(object sender, KeyEventArgs e)
    {
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
  }
}