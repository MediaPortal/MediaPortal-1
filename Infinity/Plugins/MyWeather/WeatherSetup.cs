using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjectInfinity;
using ProjectInfinity.Logging;
using System.IO;
using System.Windows.Markup;
using ProjectInfinity.Plugins;
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;

namespace MyWeather
{
    /// <summary>
    /// Interaction logic for WeatherSetup.xaml
    /// </summary>

    public class WeatherSetup : View, IMenuCommand, IDisposable
    {
        WeatherSetupViewModel _model;

        public WeatherSetup()
        {
            WeatherSetupViewModel _model = new WeatherSetupViewModel();
            DataContext = _model;
            this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
        }

        public void Run()
        {
            ServiceScope.Get<INavigationService>().Navigate(new WeatherSetup());
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}