using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;
using ProjectInfinity.MenuManager;

namespace ProjectInfinity.Pictures
{
  //[Plugin("MyPictures", "Show Pictures", ListInMenu = true, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/Pictures.png")]
  class PicturePlugin : IPlugin, IAutoStart, IMenuCommand
  {
    public PicturePlugin()
    {
      Debug.WriteLine("MyPictures started");
    }

    #region IPlugin Members
    public void Initialize(string id)
    {
    }
    #endregion

    #region IAutoStart Members
    public void Startup()
    {
      Run();
    }
    #endregion

    #region IMenuCommand Members
    public void Run()
    {
      ServiceScope.Get<INavigationService>().Navigate(new PictureView());
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
