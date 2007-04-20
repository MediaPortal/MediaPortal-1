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
using ProjectInfinity.Players;
using ProjectInfinity.Logging;

namespace MyVideos
{
  /// <summary>
  /// Interaction logic for VideoFullscreen.xaml
  /// </summary>

  public partial class VideoFullscreen : System.Windows.Controls.Page
  {
    VideoFullscreenViewModel _dataModel;

    public VideoFullscreen()
    {
      InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      _dataModel = new VideoFullscreenViewModel(this);
      gridMain.DataContext = _dataModel;

      // Add a input binding for the "back"-command
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

      // Keyboard events
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      Keyboard.Focus(gridMain);

      // Mouse events
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(OnMouseMoveEvent));

      this.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(OnMouseButtonDownEvent), true);
      this.KeyDown += new KeyEventHandler(onKeyDown);

      if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
      {
        ServiceScope.Get<ILogger>().Info("Video: fullscreen mode toggled.");

        MediaPlayer player = (MediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0].UnderlyingPlayer;
        player.MediaEnded += new EventHandler(player_MediaEnded);
        VideoDrawing videoDrawing = new VideoDrawing();
        videoDrawing.Player = player;
        videoDrawing.Rect = new Rect(0, 0, gridMain.ActualWidth, gridMain.ActualHeight);
        DrawingBrush videoBrush = new DrawingBrush();
        videoBrush.Drawing = videoDrawing;
        gridMain.Background = videoBrush;

        ServiceScope.Get<ILogger>().Info("Video: drawing video.");
      }
      Keyboard.Focus(gridMain);
    }

    /// <summary>
    /// When the playback has stopped this function gets called.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void player_MediaEnded(object sender, EventArgs e)
    {
      NavigationService.GoBack();
    }

    private void OnMouseButtonDownEvent(object sender, RoutedEventArgs e)
    {
    }

    private void OnMouseMoveEvent(object sender, MouseEventArgs e)
    {
    }

    /// <summary>
    /// Occures when a user presses a button.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
      {
        NavigationService.GoBack();
        e.Handled = true;
        return;
      }
    }

  }
}