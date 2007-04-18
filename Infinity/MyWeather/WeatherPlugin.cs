using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;

namespace MyWeather
{
  [Plugin("My Weather", "My Weather", ListInMenu = true, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/tv.png")]
  public class WeatherPlugin : IPlugin, IMenuCommand, IDisposable
  {
    #region IPlugin Members

    public void Initialize()
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyWeather;component/Weather.xaml", UriKind.Relative));
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
