using System;
using ProjectInfinity;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;
using ProjectInfinity.Playlist;

namespace MyVideos
{
  [Plugin("My Video", "My Video", ListInMenu = true, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/Video.png")]
  public class VideoPlugin : IPlugin
  {
    #region IPlugin Members

    public void Initialize()
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyVideos;component/VideoHome.xaml", UriKind.Relative));

      // Add playlist service
      IPlaylistManager playlist = new PlaylistManager();
      ServiceScope.Add<IPlaylistManager>(playlist);
    }

    #endregion

    #region IDisposable Members

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
    }

    #endregion
  }
}
