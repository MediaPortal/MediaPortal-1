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

namespace MyWeather
{
    /// <summary>
    /// Interaction logic for Weather.xaml
    /// </summary>

    public partial class Weather : System.Windows.Controls.Page
    {
        WeatherViewModel _model;

        public Weather()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            gridMain.Children.Clear();
            using (FileStream steam = new FileStream(@"skin\default\myweather\weather.xaml", FileMode.Open, FileAccess.Read))
            {
                UIElement documentRoot = (UIElement)XamlReader.Load(steam);
                gridMain.Children.Add(documentRoot);
            }

            _model = new WeatherViewModel(this);
            gridMain.DataContext = _model;

            // Add keybindings for "back" action
            this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

            // Keyboard events
            Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
            Keyboard.Focus(gridMain);

            // Mouse events
            Mouse.AddMouseMoveHandler(this, new MouseEventHandler(OnMouseMoveEvent));

            this.KeyDown += new KeyEventHandler(onKeyDown);

            Keyboard.Focus(gridMain);
        }

        private void OnMouseButtonDownEvent(object sender, RoutedEventArgs e)
        {
        }

        private void OnMouseMoveEvent(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Occures when a user presses a button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                NavigationService.GoBack();
                e.Handled = true;
                return;
            }
        }

    }
}