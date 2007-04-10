using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Pictures
{
  [Plugin("MyPictures", "Show Pictures", ListInMenu = true, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/Pictures.png")]
  class PicturePlugin : IPlugin
  {
    public void Initialize()
    {
      ServiceScope.Replace(new PictureViewModel());
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/ProjectInfinity.Plugins;component/Pictures/PictureView.xaml", UriKind.Relative));
    }

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
    }
  }
}
