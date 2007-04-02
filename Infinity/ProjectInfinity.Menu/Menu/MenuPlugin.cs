using System;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu
{
  [Plugin("Menu", "Project Infinity's main menu", AutoStart=true)]
  public class MenuPlugin : IPlugin
  {
    #region IPlugin Members

    public void Initialize()
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/ProjectInfinity.Menu;component/MenuPage.xaml", UriKind.Relative));
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