using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;
using ProjectInfinity.Settings;

namespace MyWeather
{
  [Plugin("My Weather", "My Weather", ListInMenu = true, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/tv.png")]
  public class WeatherPlugin : IPlugin, IMenuCommand, IDisposable
  {
    #region IPlugin Members

    public void Initialize(string id)
    {
    }

    #endregion

    #region IMenuCommand Members

    public void Run()
    {
        // Check if there are any settings available already
        WeatherSettings settings = new WeatherSettings();
        ServiceScope.Get<ISettingsManager>().Load(settings);
        if (settings.LocationCode.Equals("<none>"))
        {
            // No Settings found, navigate to Settings first!
            ServiceScope.Get<INavigationService>().Navigate(new WeatherSetup());
        }
        else
        {
            // We already have settings, so lets go to Weather directly
            ServiceScope.Get<INavigationService>().Navigate(new Weather());
        }
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
