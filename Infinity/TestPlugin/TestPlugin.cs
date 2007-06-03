using System;
using ProjectInfinity;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;
using ProjectInfinity.MenuManager;

namespace TestPlugin
{
  public class TestPlugin : IPlugin, IMenuCommand
  {
    #region IPlugin Members

    public void Initialize(string id)
    {
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

    public void Run()
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/TestPlugin;component/TestPage.xaml", UriKind.Relative));
    }
  }
}
