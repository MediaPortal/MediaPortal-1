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
using ProjectInfinity.Localisation;
using ProjectInfinity.Players;
using ProjectInfinity.Controls;
using System.IO;
using System.Windows.Markup;

namespace MyVideos
{
  /// <summary>
  /// Interaction logic for TestPage.xaml
  /// </summary>

  public partial class VideoHome : View
  {
    VideoHomeViewModel _model;

    public VideoHome()
    {
      _model = new VideoHomeViewModel();
      DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.Fullscreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
      ServiceScope.Get<IMessageBroker>().Register(this);
    }

  }
}