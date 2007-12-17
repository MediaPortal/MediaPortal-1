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
using ProjectInfinity.Playlist;
using ProjectInfinity.Controls;
using System.IO;
using System.Windows.Markup;

namespace MyVideos
{
  /// <summary>
  /// Interaction logic for VideoPlaylist.xaml
  /// </summary>

  public partial class VideoPlaylist  : View
  {
    VideoPlaylistViewModel _model;

    public VideoPlaylist()
    {
      _model = new VideoPlaylistViewModel();
      DataContext = _model;
      //this.InputBindings.Add(new KeyBinding(_model.Fullscreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
      

      ServiceScope.Get<IMessageBroker>().Register(this);
    }
#if NOTUSED
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // load the external xaml
      gridMain.Children.Clear();
      using (FileStream steam = new FileStream(@"skin\default\myvideos\VideoPlaylist.xaml", FileMode.Open, FileAccess.Read))
      {
        UIElement documentRoot = (UIElement)XamlReader.Load(steam);
        gridMain.Children.Add(documentRoot);
      }

      // Add a input binding for the "back"-command
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

      // View Model
      _model = new VideoPlaylistViewModel(this);
      gridMain.DataContext = _model;

      // Keyboard
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));

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
      if (((e.Key == Key.Add) || (e.Key == Key.Subtract)) &&
          (e.Source as ProjectInfinity.Controls.ListBox) != null)
      {
        ProjectInfinity.Controls.ListBox _listBox = (ProjectInfinity.Controls.ListBox)e.Source;
        PlaylistManager _manager = (PlaylistManager)ServiceScope.Get<IPlaylistManager>();

        // move the item upwards
        if (e.Key == Key.Subtract)
        {
          _manager.MovePlaylistItemUp(_listBox.Items.CurrentItem);
          _model.ChangeProperty("VideoPlaylist");
          _listBox.UpdateLayout();
          _listBox.Items.Refresh();
        }
        // move the item downwards
        else if (e.Key == Key.Add)
        {
          _manager.MovePlaylistItemDown(_listBox.Items.CurrentItem);
          _model.ChangeProperty("VideoPlaylist");
          _listBox.UpdateLayout();
          _listBox.Items.Refresh();
        }
      }
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
#endif
  }
}