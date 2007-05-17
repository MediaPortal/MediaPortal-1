using System;
using ProjectInfinity;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;

namespace MediaModule
{
    [Plugin("Media Module", "Media Module", ListInMenu = true, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/Video.png")]
    public class MediaModulePlugin : IPlugin, IMenuCommand, IDisposable
    {

        #region IPlugin Members

        public void Initialize(string id)
        {
        }

        #endregion

        #region IMenuCommand Members

        public void Run()
        {
            ServiceScope.Get<INavigationService>().Navigate(new MediaHome());
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
