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

namespace MyWeather
{
    public class WeatherSetupViewModel : WeatherViewModel
    {
        #region variables
        string _labelError;
        ICommand _saveCommand;
        ICommand _searchCommand;
        string _location;
        ArrayList _cities;
        CollectionView _citiesCollView;
        #endregion

        #region ctor
        public WeatherSetupViewModel(Page page)
            : base(page)
        {
            _labelError = "";
            _location = "";
            _cities = new ArrayList();
            _citiesCollView = new CollectionView(_cities);
        }
        #endregion
        #region properties
        /// <summary>
        /// Gets the localized label header.
        /// </summary>
        /// <value>The label header.</value>
        public string LabelHeader
        {
            get
            {
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
                return _labelError;
            }
            set
            {
                _labelError = value;
                ChangeProperty("LabelError");

            }
        }
        /// <summary>
        /// holds a collection of found Locations
        /// </summary>
        public CollectionView FoundLocations
        {
            get {
                return _citiesCollView;
                //return _cities; 
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
        /// <summary>
        /// Gets or sets the location
        /// </summary>
        /// <value>location name</value>
        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;
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
                // Perform actual search
                //
                WeatherChannel weather = new WeatherChannel();
                _cities.Clear();
                _cities = weather.SearchCity(Location);
                System.Windows.MessageBox.Show("Input: " + Location);
                //
                // Clear previous results
                //
                int size = 0;
                foreach (City city in _cities)
                {
                    System.Windows.MessageBox.Show(city.Name + ", " + city.Id);
                    size++;
                }
                if(size==0)
                    LabelError = ServiceScope.Get<ILocalisation>().ToString("myweather.config",4);

                //_citiesCollView.SourceCollection = _cities;
                foreach (City c in _citiesCollView.SourceCollection)
                {
                    System.Windows.MessageBox.Show("in collection: " + c.Name);
                }
                ChangeProperty("FoundLocations");
            }
            catch (Exception ex)
            {
                ServiceScope.Get<ILogger>().Error(ex);
            }
        }


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
                // Save Settings to xml file

            }

            #endregion
        }
        /// <summary>
        /// This command will save the location
        /// </summary>
        public class SearchCommand : ICommand
        {
            WeatherSetupViewModel _viewModel;
            /// <summary>
            /// Initializes a new instance of the <see cref="SaveCommand"/> class.
            /// </summary>
            /// <param name="model">The model.</param>
            public SearchCommand(WeatherSetupViewModel model)
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
                // Search for locations
                _viewModel.SearchCities();
            }

            #endregion
        }

        #endregion
    }
}
