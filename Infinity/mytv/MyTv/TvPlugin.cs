using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;

namespace MyTv
{
  [Plugin("My Tv", "My Tv", ListInMenu = true, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/tv.png")]
  public class TvPlugin: IPlugin, IMenuCommand
  {
    #region IPlugin Members

    public void Initialize()
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvHome.xaml", UriKind.Relative));
    }

    #endregion

    #region IMenuCommand Members

    public void Run()
    {
      Initialize();
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
