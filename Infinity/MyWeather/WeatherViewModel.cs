using System;
using System.Collections;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity;
using ProjectInfinity.Settings;

namespace MyWeather
{
    public class WeatherViewModel : INotifyPropertyChanged
    {
        #region variables
        Window _window;
        Page _page;
        WeatherDataModel _dataModel;
        ICommand _updateWeatherCommand;
        ICommand _locationChangedCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherViewModel"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        public WeatherViewModel(Page page)
        {

            //store page & window
            _page = page;
            _window = Window.GetWindow(_page);
            // create the datamodel :)
//            WeatherSettings setting = new WeatherSettings();
            _dataModel = new WeatherDataModel("UKXX0085");
        }

        void OnLocationChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WeatherLocation")
            {
                ChangeProperty("WeatherCurrImage");
                ChangeProperty("WeatherCurrTemperature");
                ChangeProperty("WeatherCurrCondition");
                ChangeProperty("WeatherLocation"); // <-- not sure yet if to be called (endless loop?)
            }
        }
        #endregion

        #region properties



        /// <summary>
        /// Notifies subscribers that property has been changed
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void ChangeProperty(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region properties of the current location
        /// <summary>
        /// Gets the current Condition Image.
        /// </summary>
        /// <value>current weather image.</value>
        public string WeatherCurrImage
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Gets the current Condition
        /// </summary>
        /// <value>current condition</value>
        public string WeatherCurrCondition
        {
            get
            {
                // return the condiition
                return "";
            }
        }

        /// <summary>
        /// Gets the current Temperature 
        /// </summary>
        /// <value>current temperature</value>
        public int WeatherCurrTemperature
        {
            get
            {
                // return the data
                return 0;
            }
        }

        /// <summary>
        /// Gets the current Location 
        /// </summary>
        /// <value>current location</value>
        public string WeatherCurrLocation
        {
            get
            {
                // return the data
                return "";
            }
        }
        #endregion

        /// <summary>
        /// Gets the window.
        /// </summary>
        /// <value>The window.</value>
        public Window Window
        {
            get
            {
                return _window;
            }
        }
        /// <summary>
        /// Gets the current Page.
        /// </summary>
        /// <value>The page.</value>
        public Page Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
                _window = Window.GetWindow(_page);
            }
        }

        #region button label properties
        /// <summary>
        /// Gets the current date
        /// </summary>
        /// <value>The date label.</value>
        public string DateLabel
        {
            get
            {
                return DateTime.Now.ToString("dd-MM HH:mm");
            }
        }
        /// <summary>
        /// Gets the localized version of the location label.
        /// </summary>
        /// <value>The location button label.</value>
        public string LocationLabel
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 1);//Location
            }
        }

        /// <summary>
        /// Gets the localized version of the refresh label.
        /// </summary>
        /// <value>The refresh button label.</value>
        public string RefreshLabel
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 2);//Location
            }
        }


        #region commands
        /// <summary>
        /// Returns a ICommand for updating the Weather
        /// </summary>
        /// <value>The command.</value>
        public ICommand UpdateWeather
        {
            get
            {
                if (_updateWeatherCommand == null)
                {
                    _updateWeatherCommand = new UpdateWeatherCommand(this, _dataModel);
                }
                return _updateWeatherCommand;
            }
        }

        /// <summary>
        /// Returns a ICommand for updating the Location
        /// </summary>
        /// <value>The command.</value>
        public ICommand LocationChanged
        {
            get
            {
                if (_locationChangedCommand == null)
                {
                    _locationChangedCommand = new LocationChangedCommand(this, _dataModel);
                }
                return _locationChangedCommand;
            }
        }
        #endregion

        #region Commands subclasses

        #region base command class

        public abstract class WeatherBaseCommand : ICommand
        {
            protected WeatherViewModel _viewModel;
            protected WeatherDataModel _dataModel;
            public event EventHandler CanExecuteChanged;

            public WeatherBaseCommand(WeatherViewModel viewModel, WeatherDataModel dataModel)
            {
                _viewModel = viewModel;
                _dataModel = dataModel;
            }

            public abstract void Execute(object parameter);

            public virtual bool CanExecute(object parameter)
            {
                return true;
            }

            protected void OnCanExecuteChanged()
            {
                if (this.CanExecuteChanged != null)
                {
                    this.CanExecuteChanged(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region LocationChangedCommand  class
        /// <summary>
        /// LocationChangedCommand will set a new location
        /// </summary> 
        public class LocationChangedCommand : WeatherBaseCommand
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LocationChangedCommand"/> class.
            /// </summary>
            /// <param name="viewModel">The view model.</param>
            public LocationChangedCommand(WeatherViewModel viewModel, WeatherDataModel dataModel)
                : base(viewModel, dataModel)
            {
            }

            /// <summary>
            /// Executes the command.
            /// </summary>
            /// <param name="parameter">The parameter.</param>
            public override void Execute(object parameter)
            {
                // update weather data for new location and update labels go in here
            }
        }
        #endregion

        #region UpdateWeatherCommand  class
        /// <summary>
        /// UpdateWeatherCommand will fetch new weather data
        /// </summary> 
        public class UpdateWeatherCommand : WeatherBaseCommand
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LocationChangedCommand"/> class.
            /// </summary>
            /// <param name="viewModel">The view model.</param>
            public UpdateWeatherCommand(WeatherViewModel viewModel, WeatherDataModel dataModel)
                : base(viewModel,dataModel)
            {
            }

            /// <summary>
            /// Executes the command.
            /// </summary>
            /// <param name="parameter">The parameter.</param>
            public override void Execute(object parameter)
            {
                // update weather data and labels go in here
                System.Windows.MessageBox.Show("wetter wird aktualisiert!");
                _dataModel.Update();
            }
        }
        #endregion


        #endregion
        #endregion
        #endregion
    }
}

