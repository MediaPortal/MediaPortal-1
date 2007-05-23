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
using System.Xml;
using System.Collections;
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
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Settings;
using System.Collections.Specialized;
using System.ComponentModel;
using Dialogs;
using System.Collections.Generic;

namespace MyWeather
{
    public class WeatherSetupViewModel
    {
        #region variables
        string _labelError;
        ICommand _saveCommand;
        ICommand _searchCommand;                        // Opens a Popup Menu
        string _searchLocation;                         // the location we type in for searching
        CitySetupInfo _selectedLocation;                // the selected found location as City type
        LocationCollectionView _citiesCollView;
        LocationCollectionView _citiesAddedView;
        WeatherSetupDataModel _dataModel;
        WeatherSetupDataModel _dataModelAddedLocs;
        IWeatherCatcher _catcher;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ctor
        public WeatherSetupViewModel()
            : base()
        {
            _labelError = "";
            _searchLocation = "";
            // Load Settings and look if  there are any locations set already.
            // if so, update the datamodel
            WeatherSettings settings = new WeatherSettings();
            ServiceScope.Get<ISettingsManager>().Load(settings);
            _catcher = new WeatherDotComCatcher();
            _dataModel = new WeatherSetupDataModel(_catcher);
            _dataModelAddedLocs = new WeatherSetupDataModel(_catcher);
            // add already configured cities from the settings to our datamodel
            if (settings.LocationsList != null)
            {
                foreach (CitySetupInfo c in settings.LocationsList)
                {
                    if (c != null)
                    {
                        _dataModelAddedLocs.AddCity(c);
                    }
                }
                ChangeProperty("LocationsAdded");
            }
            // create Collectionviews...
            _citiesCollView = new LocationCollectionView(_dataModel);
            _citiesAddedView = new LocationCollectionView(_dataModelAddedLocs);
            // generate some sample data for the designer if
            // this is run in design mode
            if (Core.IsDesignMode) GenerateSampleData();
        }

        #endregion

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
        /// generate some dummy data for the designer when we are
        /// in design mode.
        /// </summary>
        public void GenerateSampleData()
        {
            // add data to the found locations
            _dataModel.AddCity("Amsterdam, Netherlands", "AX2005");
            _dataModel.AddCity("Dresden, Germany", "UL1235005");
            _dataModel.AddCity("Hamburg, Germany", "UL55335");
            _dataModel.AddCity("New York, United States", "US99625");
            // add data to the added locations
            _dataModelAddedLocs.AddCity("Dresden, Germany", "UL1235005");
            _dataModelAddedLocs.AddCity("Berlin, Germany", "UL12005");
            _dataModelAddedLocs.AddCity("Cologne, Germany", "UL212625");
            _dataModelAddedLocs.AddCity("Helsinki, Sweden", "SW881273");
            // selected location
            _selectedLocation = (CitySetupInfo)_dataModelAddedLocs.Locations[0];
            _searchLocation = "Dummytown";
            // update properties
            ChangeProperty("Locations");
            ChangeProperty("LocationsAdded");
            ChangeProperty("SelectedLocation");
            ChangeProperty("SearchLocation");
        }

        #region properties
        /// <summary>
        /// Gets the localized label header.
        /// </summary>
        /// <value>The label header.</value>
        public string LabelHeader
        {
            get
            {
                if (Core.IsDesignMode) return "setup weather";
                return ServiceScope.Get<ILocalisation>().ToString("myweather.config", 0);//setup weather location;
            }
        }
        /// <summary>
        /// Gets the  date.
        /// </summary>
        /// <value>The label date.</value>
        public string LabelDate
        {
            get
            {
                return DateTime.Now.ToString("dd-MM HH:mm");
            }
        }
        /// <summary>
        /// Gets the localized label for save.
        /// </summary>
        /// <value>The label save.</value>
        public string LabelSearch
        {
            get
            {
                if(Core.IsDesignMode) return "Search";
                return ServiceScope.Get<ILocalisation>().ToString("myweather.config", 1);//Search
            }
        }
        /// <summary>
        /// Gets the localized label for the text.
        /// </summary>
        /// <value>The label text.</value>
        public string LabelText
        {
            get
            {
                if (Core.IsDesignMode) return "Please enter the name of your city";
                return ServiceScope.Get<ILocalisation>().ToString("myweather.config", 2);//Please enter the name of your city
            }
        }
        /// <summary>
        /// Gets or sets the label error.
        /// </summary>
        /// <value>The label error.</value>
        public string LabelError
        {
            get
            {
                if (Core.IsDesignMode) return "this is the error label";
                return _labelError;
            }
            set
            {
                _labelError = value;
                ChangeProperty("LabelError");

            }
        }
        /// <summary>
        /// Gets or sets the label for the location to add
        /// </summary>
        /// <value>The location to be added label :).</value>
        public string LabelAddLocation
        {
            get
            {
                if (Core.IsDesignMode) return "Add";
                return ServiceScope.Get<ILocalisation>().ToString("myweather.config", 5);
            }
        }
        /// <summary>
        /// Gets or sets the save label
        /// </summary>
        /// <value>The save label</value>
        public string LabelSave
        {
            get
            {
                if (Core.IsDesignMode) return "Save";
                return ServiceScope.Get<ILocalisation>().ToString("myweather.config", 6);
            }
        }
        /// <summary>
        /// Returns the ListViewCollection containing the found cities
        /// </summary>
        /// <value>The Cities.</value>
        public CollectionView Locations
        {
            get
            {
                if (_citiesCollView == null)
                {
                    _citiesCollView = new LocationCollectionView(_dataModel);
                }
                return _citiesCollView;
            }
        }

        /// <summary>
        /// Returns the ListViewCollection containing the found cities
        /// </summary>
        /// <value>The Cities.</value>
        public CollectionView LocationsAdded
        {
            get
            {
                if (_citiesAddedView == null)
                {
                    _citiesAddedView = new LocationCollectionView(_dataModelAddedLocs);
                }
                return _citiesAddedView;
            }
        }

        /// <summary>
        /// Gets or sets the location to Search for
        /// </summary>
        /// <value>location name</value>
        public string SearchLocation
        {
            get
            {
                return _searchLocation;
            }
            set
            {
                _searchLocation = value;
                ChangeProperty("SearchLocation");
            }
        }
        /// <summary>
        /// Gets or sets the location which has been
        /// selected by the user
        /// </summary>
        /// <value>location name</value>
        public CitySetupInfo SelectedLocation
        {
            get
            {
                return _selectedLocation;
            }
            set
            {
                _selectedLocation = value;
                ChangeProperty("SelectedLocation");
            }
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
        #endregion
        #region commands

        /// <summary>
        /// Returns ICommand to save the settings
        /// </summary>
        /// <value>The save.</value>
        public ICommand Save
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new SaveCommand(this);
                }
                return _saveCommand;
            }
        }
        /// <summary>
        /// Returns ICommand to search for LocationIDs
        /// </summary>
        /// <value>The search</value>
        public ICommand Search
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new SearchCommand(this);
                }
                return _searchCommand;
            }
        }
        #endregion

        /// <summary>
        /// searches for cities and populates the cities list
        /// </summary>
        protected void SearchCities()
        {
            try
            {
                //
                // Check for Internetconnection
                //
                int code = 0;
                if(!Helper.IsConnectedToInternet(ref code))
                {
                    LabelError = "Failed to perform city search, make sure you are connected to the internet.";
                    return;
                }

                //
                // Perform actual search
                //
                _dataModel.SearchCity(SearchLocation);

                if(_dataModel.Locations.Count==0)
                    LabelError = ServiceScope.Get<ILocalisation>().ToString("myweather.config",4);

                // data bound to the LocationCollectionView updated
                // so lets refresh that view as well
                ChangeProperty("Locations");
                // clear the search text
                SearchLocation = "";

            }
            catch (Exception ex)
            {
                ServiceScope.Get<ILogger>().Error(ex);
            }
        }

        #region LocationCollectionView class
        /// <summary>
        /// This class represents the locations view
        /// </summary>
        public class LocationCollectionView : ListCollectionView
        {
            #region variables
            private WeatherSetupDataModel _model;
            #endregion

            /// <summary>
            /// Initializes a new instance of the <see cref="LocationCollectionView"/> class.
            /// </summary>
            /// <param name="model">The database model.</param>
            public LocationCollectionView(WeatherSetupDataModel datamodel)
                : base(datamodel.Locations)
            {
                _model = datamodel;
                _model.PropertyChanged += new PropertyChangedEventHandler(OnDataChanged);
            }

            void OnDataChanged(object sender, PropertyChangedEventArgs e)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        #endregion

        #region command classes
        /// <summary>
        /// This command will save the location
        /// </summary>
        public class SaveCommand : ICommand
        {
            WeatherSetupViewModel _viewModel;
            /// <summary>
            /// Initializes a new instance of the <see cref="SaveCommand"/> class.
            /// </summary>
            /// <param name="model">The model.</param>
            public SaveCommand(WeatherSetupViewModel model)
            {
                _viewModel = model;
            }

            #region ICommand Members

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                // Save all locations to the Settings
                WeatherSettings settings = new WeatherSettings();
                settings.LocationsList.Clear();
                List<CitySetupInfo> l = (List<CitySetupInfo>)_viewModel.LocationsAdded.SourceCollection;

                foreach (CitySetupInfo c in l)
                {
                    settings.LocationsList.Add(c);
                }
                if (l.Count > 0)
                    settings.LocationCode = l[0].Id;
                // save
                ServiceScope.Get<ISettingsManager>().Save(settings);
                // navigate back
                ServiceScope.Get<INavigationService>().GoBack();
            }

            #endregion
        }

        /// <summary>
        /// search for a location and open a popupmenu to
        /// select a location
        /// </summary>
        public class SearchCommand : ICommand
        {
            WeatherSetupViewModel _viewModel;

            public SearchCommand(WeatherSetupViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                _viewModel.LabelError = "";
                _viewModel.SearchCities();
                
                // nothing found?
                if (((List<CitySetupInfo>)(_viewModel.Locations.SourceCollection)).Count <= 0) { MessageBox.Show("no cities in collection"); return; }

                // Setup the Dialog Menu we wanna show (Containing the found locations)
                MpMenu dlgMenu = new MpMenu();
                dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlgMenu.Owner = _viewModel.Window;
                dlgMenu.Items.Clear();
                dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("myweather.config", 3);// "Please select your City";
                dlgMenu.SubTitle = "";

                foreach (CitySetupInfo c in _viewModel.Locations.SourceCollection)
                {
                    dlgMenu.Items.Add(new DialogMenuItem(c.Name + ", " + c.Id));
                }

                // show dialog menu
                dlgMenu.ShowDialog();
                if (dlgMenu.SelectedIndex < 0) return;    // no menu item selected

                // get the id that belongs to the selected city and set the property
                CitySetupInfo buff = ((List<CitySetupInfo>)(_viewModel.Locations.SourceCollection))[dlgMenu.SelectedIndex];
                _viewModel.SelectedLocation = buff;
                // add location directly to the datamodel
                _viewModel._dataModelAddedLocs.AddCity(_viewModel.SelectedLocation);
                _viewModel.ChangeProperty("LocationsAdded");
            }
        }
        #endregion
    }
}
