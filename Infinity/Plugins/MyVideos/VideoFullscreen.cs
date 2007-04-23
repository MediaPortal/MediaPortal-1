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
    VideoFullscreenViewModel _model;
    public VideoFullscreen()
    {
      _model = new VideoFullscreenViewModel(); ;
      DataContext = _model;
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
      this.Loaded += new RoutedEventHandler(VideoFullscreen_Loaded);
    }

    void VideoFullscreen_Loaded(object sender, RoutedEventArgs e)
    {
      this.Background = _model.VideoBrush;
    }


  }
}