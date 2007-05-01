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
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;

namespace MyVideos
{
  /// <summary>
  /// Interaction logic for VideoFullscreen.xaml
  /// </summary>

  public partial class VideoFullscreen : View
  {
    VideoFullscreenViewModel _model;
    public VideoFullscreen()
    {
      this.Loaded += new RoutedEventHandler(VideoFullscreen_Loaded);
      this.Unloaded += new RoutedEventHandler(VideoFullscreen_Unloaded);

    }

    void VideoFullscreen_Unloaded(object sender, RoutedEventArgs e)
    {
      _model.Dispose();
      _model = null;
    }

    void VideoFullscreen_Loaded(object sender, RoutedEventArgs e)
    {
      _model = new VideoFullscreenViewModel(); ;
      DataContext = _model;
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
      this.Background = _model.VideoBrush;
    }


    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
      
      if (e.Key == Key.Up)
      {
        _model.OnChannelUp();
        e.Handled = true;
        return;
      }
      if (e.Key == Key.Down)
      {
        _model.OnChannelDown();
        e.Handled = true;
        return;
      }
      if (e.Key >= Key.D0 && e.Key <= Key.D9)
      {
        _model.OnChannelKey(e.Key);
        e.Handled = true;
        return;
      }
      if (e.Key == Key.Left || e.Key == Key.Right)
      {
        _model.OnSeek(e.Key);
        e.Handled = true;
        return;
      }
      if (e.Key == Key.Escape || e.Key == Key.X)
      {
        //return to previous screen
        e.Handled = true;
        ServiceScope.Get<INavigationService>().GoBack();
        return;
      }
      if (e.Key == Key.Space)
      {
        e.Handled = true;
        _model.Pause();
        return;
      }
    }

  }
}