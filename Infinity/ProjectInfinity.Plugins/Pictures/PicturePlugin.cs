using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Pictures
{
  [Plugin("MyPictures", "Show Pictures", ListInMenu = true, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/Pictures.png")]
  class PicturePlugin : IPlugin, IMenuCommand
  {
    public void Initialize()
    {
      ServiceScope.Get<INavigationService>().Navigate(new PictureView());
    }

    #region IMenuCommand Members

    public void Run()
    {
      Initialize();
    }

    #endregion

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
    }
  }
}
