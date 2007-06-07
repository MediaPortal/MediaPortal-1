#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion
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
using Dialogs;
using System.Windows.Threading;


namespace MyWeather
{
    /// <summary>
    /// ViewModel Class for Weather.xaml
    /// </summary>
    public class WeatherViewModel : DispatcherObject, INotifyPropertyChanged
    {
        #region variables

        bool _isBusy = false;           // used for the wait cursor
        City _currCity;                 // holds the currently selected city
        WeatherLocalizer _locals;       // databinding source for the localisations
        WeatherDataModel _dataModel;
        ICommand _updateWeatherCommand; // command to refresh the data of all cities
        ICommand _changeLocationCommand;// command to change to a new location (changes _currCity)
        ICommand _hyperlinkCommand;     // command to guide to another screen (class needs to be given as string parameter)
        List<City> _availableLocations; // list of all available locations + data
        public event PropertyChangedEventHandler PropertyChanged;
        
        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherViewModel"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        public WeatherViewModel()
        {
            // we are in design mode, call a method
            // to fill up some dummy data :)
            if (Core.IsDesignMode)
            {
                CreateDummyData();
            }
            // create localisation instance
            _locals = new WeatherLocalizer();
            // create the datamodel :)
            _dataModel = new WeatherDataModel(new WeatherDotComCatcher());
            UpdateWeather.Execute(null);
        }
        #endregion

        #region methods
        /// <summary>
        /// creates some Dummydata for the Designer
        /// </summary>
        public void CreateDummyData()
        {
            // Create lists
            _availableLocations = new List<City>();
            // Create 2 City Objects
            City city1 = new City("Dummytown, Dummyland", "DUM200512");
            City city2 = new City("Amsterdam, Netherlands", "NL3005132");
            city1.currCondition = new CurrentCondition();
            city1.currCondition.FillWithDummyData();
            city1.locationInfo = new LocInfo();
            city1.locationInfo.FillWithDummyData();
            city1.forecast = new List<DayForeCast>();
            city1.forecast.Add(new DayForeCast(0));
            city1.forecast.Add(new DayForeCast(1));
            city1.forecast.Add(new DayForeCast(2));
            city1.forecast.Add(new DayForeCast(3));
            city2.currCondition = new CurrentCondition();
            city2.currCondition.FillWithDummyData();
            city2.locationInfo = new LocInfo();
            city2.locationInfo.FillWithDummyData();
            city2.forecast = new List<DayForeCast>();
            city2.forecast.Add(new DayForeCast(0));
            city2.forecast.Add(new DayForeCast(1));
            city2.forecast.Add(new DayForeCast(2));
            city2.forecast.Add(new DayForeCast(3));
            _availableLocations.Add(city1);
            _availableLocations.Add(city2);
            _currCity = city1;
            LocationPropertiesUpdated();
        }
        /// <summary>
        /// load all configured locations from settings
        /// </summary>
        public void LoadAvailableLocations()
        {
            _availableLocations = _dataModel.LoadLocationsData();
            LocationPropertiesUpdated();
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

        /// <summary>
        /// Tells the GUI that our properties have been updated
        /// </summary>
        public void LocationPropertiesUpdated()
        {
            ChangeProperty("AvailableLocations");
            ChangeProperty("CurrentLocation");
        }
        /// <summary>
        /// Gets the window.
        /// </summary>
        /// <value>The window.</value>
        public Window Window
        {
            get
            {
                return ServiceScope.Get<INavigationService>().GetWindow();
            }
        }
        
        /// <summary>
        /// Sets/Gets the busy Status
        /// </summary>
        /// <param name="busy"></param>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                ChangeProperty("IsBusy");
                ChangeProperty("IsLoadingCursor");
            }
        }
        /// <summary>
        /// gets the visiblity status of the loading cursor
        /// </summary>
        public Visibility IsLoadingCursor
        {
            get
            {
                return _isBusy ? Visibility.Visible : Visibility.Hidden;
            }
        }
        #region properties of the current location
        /// <summary>
        /// Gets the current Location as Typed City object 
        /// </summary>
        /// <value>current location</value>
        public City CurrentLocation
        {
            get
            {
                return _currCity;
            }
            set
            {
                _currCity = value;
                ChangeProperty("CurrentLocation");
            }
        }

        /// <summary>
        /// Gets the all availbe locations
        /// </summary>
        /// <value>list of locations</value>
        public List<City> AvailableLocations
        {
            get
            {
                return _availableLocations;
            }
        }

        /// <summary>
        /// Gets localized versions of the labels
        /// </summary>
        /// <value>WeatherLocalizer object</value>
        public WeatherLocalizer Localisation
        {
            get
            {
                return _locals;
            }
        }
        #endregion

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
        public ICommand ChangeLocation
        {
            get
            {
                if (_changeLocationCommand == null)
                {
                    _changeLocationCommand = new ChangeLocationCommand(this, _dataModel);
                }
                return _changeLocationCommand;
            }
        }

        /// <summary>
        /// Returns a ICommand for updating the Location
        /// </summary>
        /// <value>The command.</value>
        public ICommand Hyperlink
        {
            get
            {
                if (_hyperlinkCommand == null)
                {
                    _hyperlinkCommand = new HyperlinkCommand();
                }
                return _hyperlinkCommand;
            }
        }

        #endregion

        #region Commands subclasses

        #region base command class

        /// <summary>
        /// This is the Basecommand class for
        /// My Weather
        /// </summary>
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

        #region ChangeLocationCommand  class
        /// <summary>
        /// ChangeLocationCommand will set a new location
        /// </summary> 
        public class ChangeLocationCommand : WeatherBaseCommand
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LocationChangedCommand"/> class.
            /// </summary>
            /// <param name="viewModel">The view model.</param>
            public ChangeLocationCommand(WeatherViewModel viewModel, WeatherDataModel dataModel)
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
                if (_viewModel.AvailableLocations == null) return;      // may happen when the threading failed or the user made an input too early
                if (_viewModel.AvailableLocations.Count == 0) return;
                MpMenu menu = new MpMenu();
                menu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                menu.Owner = _viewModel.Window;
                menu.Items.Clear();
                menu.Header = ServiceScope.Get<ILocalisation>().ToString("myweather.config", 3);// "Please select your City";
                menu.SubTitle = "";
                // add items to the menu
                foreach (City c in _viewModel.AvailableLocations)
                {
                    menu.Items.Add(new DialogMenuItem(c.Name));
                }

                menu.ShowDialog();

                if (menu.SelectedIndex < 0) return;    // no menu item selected

                // get the id that belongs to the selected city and set the property
                _viewModel.CurrentLocation = ((List<City>)(_viewModel.AvailableLocations))[menu.SelectedIndex];
                // save the selected location code to settings
                WeatherSettings settings = new WeatherSettings();
                ServiceScope.Get<ISettingsManager>().Load(settings);
                settings.LocationCode = _viewModel.CurrentLocation.Id;
                ServiceScope.Get<ISettingsManager>().Save(settings);
                // if the city doesn't have information yet, try to download them again
                if (_viewModel.CurrentLocation.HasData == false)
                    _viewModel.UpdateWeather.Execute(null);
            }
        }
        #endregion

        #region UpdateWeatherCommand  class
        /// <summary>
        /// UpdateWeatherCommand will fetch new weather data
        /// </summary> 
        public class UpdateWeatherCommand : WeatherBaseCommand
        {
            private delegate void UpdateWeatherDelegate();

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
                // set to busy state
                _viewModel.IsBusy = true;
                // update weather data and labels go in here
                // _viewModel.LoadAvailableLocations();
                UpdateWeatherDelegate starter = new UpdateWeatherDelegate(this.UpdateBackGroundWorker);
                starter.BeginInvoke(null, null);
                _viewModel.LocationPropertiesUpdated();
            }

            /// <summary>
            /// Starts the timeshifting 
            /// this is done in the background so the GUI stays responsive
            /// </summary>
            /// <param name="channel">The channel.</param>
            private void UpdateBackGroundWorker()
            {
                // update weather data and labels go in here
                _viewModel.LoadAvailableLocations();
                // get all cities from the settings
                WeatherSettings settings = new WeatherSettings();
                ServiceScope.Get<ISettingsManager>().Load(settings);
                foreach (City c in _viewModel.AvailableLocations)
                {
                    if (c.Id.Equals(settings.LocationCode))
                    {
                        // okay, we found it, so let's set it
                        _viewModel.CurrentLocation = c;
                        break;
                    }
                }
                // Schedule the update function in the UI thread.
                _viewModel.LocationPropertiesUpdated();
                _viewModel.IsBusy = false;
            }
        }
        #endregion

        #region HyperlinkCommand  class
        /// <summary>
        /// UpdateWeatherCommand will fetch new weather data
        /// </summary> 
        public class HyperlinkCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            /// <summary>
            /// Initializes a new instance of the <see cref="LocationChangedCommand"/> class.
            /// </summary>
            /// <param name="viewModel">The view model.</param>
            public HyperlinkCommand()
            {
            }

            /// <summary>
            /// Executes the command.
            /// </summary>
            /// <param name="parameter">The parameter.</param>
            public void Execute(object parameter)
            {
                ServiceScope.Get<INavigationService>().Navigate(parameter);
            }

            #region ICommand Members
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

            #endregion
        }

        #endregion
        #endregion
        #endregion
    }
}

