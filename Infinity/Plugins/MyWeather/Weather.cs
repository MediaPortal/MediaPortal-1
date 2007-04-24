using ProjectInfinity.Controls;
using System.Windows.Input;
using ProjectInfinity;
using ProjectInfinity.Messaging;

namespace MyWeather
{
    /// <summary>
    /// Interaction logic for Weather.xaml
    /// </summary>
    public partial class Weather : View
    {
        public Weather()
        {
            WeatherViewModel _model = new WeatherViewModel();
            DataContext = _model;
            this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
        }
    }
}