using System;
using ProjectInfinity;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;

namespace MyVideos
{
    [Plugin("My Video", "My Video", ListInMenu = true)]
    public class VideoPlugin : IPlugin
    {
        #region IPlugin Members

        public void Initialize()
        {
            ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyVideos;component/VideoHome.xaml", UriKind.Relative));
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
