using System;
using ProjectInfinity;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;

namespace TestPlugin
{
  [Plugin("Test", "Plugin for testing purposes", ListInMenu = true, ImagePath = @"pack://siteoforigin:,,,/media/images/Music.png")]
  public class TestPlugin : IPlugin
  {
    #region IPlugin Members

    public void Initialize()
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/TestPlugin;component/TestPage.xaml", UriKind.Relative));
    }

    #endregion

    #region IDisposable Members

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
