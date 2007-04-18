using System;
using System.Collections.Generic;
using System.Text;

namespace MyWeather
{
    /// <summary>
    /// this is a basic field where
    /// the information for the provider
    /// are stored (usually the location name and unique id)
    /// </summary>
    public class CitySetupInfo
    {
        public string name, id;

        public CitySetupInfo(string name, string id)
        {
            this.name = name;
            this.id = id;
        }

        public CitySetupInfo() { }

        public override string ToString()
        {
            return name;
        }
    }

    #region City class
    /// <summary>
    /// holds Information on the City
    /// </summary>
    public class City
    {
        private CitySetupInfo _cityInfo;
        public LocInfo locationInfo;
        public CurrentCondition currCondition;
        public List<DayForeCast> forecast;
        private bool _updateSuccessful;
        /// <summary>
        /// parameterless constructor
        /// needed for serialization
        /// </summary>
        public City() 
        {
            _cityInfo = new CitySetupInfo();
            _updateSuccessful = false;
            
        }

        public City(CitySetupInfo info)
        {
            _cityInfo = info;
            _updateSuccessful = false;
        }

        public City(string name, string id)
        {
            _cityInfo = new CitySetupInfo();
            _cityInfo.name = name;
            _cityInfo.id = id;
            _updateSuccessful = false;
        }

        #region properties
        /// <summary>
        /// Get the Name of the City
        /// </summary>
        public string Name
        {
            get
            {
                return _cityInfo.name;
            }
        }

        /// <summary>
        /// LocationInfo
        /// </summary>
        public LocInfo LocationInfo
        {
            get
            {
                return locationInfo;
            }
        }
        /// <summary>
        /// Current Condition
        /// </summary>
        public CurrentCondition Condition
        {
            get
            {
                return currCondition;
            }
        }
        /// <summary>
        /// Current Condition
        /// </summary>
        public List<DayForeCast> Forecast
        {
            get
            {
                return forecast;
            }
        }
        /// <summary>
        /// Get the Location ID
        /// </summary>
        public string Id
        {
            get
            {
                return _cityInfo.id;
            }
        }

        /// <summary>
        /// Identifies if the update was successful
        /// </summary>
        public bool HasData
        {
            get
            {
                return _updateSuccessful;
            }
            set
            {
                _updateSuccessful = value;
            }
        }
        #endregion

        /// <summary>
        /// output
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
    #endregion

    #region structs
    #region LocInfo struct
    /// <summary>
    /// provides info of the Location
    /// </summary>
    public struct LocInfo  // <loc>
    {
        public string cityCode;  // <loc id="GMXX0223">
        public string city;      // <dnam>Regensburg, Germany</dnam>
        public string time;      // <tm>1:12 AM</tm>
        public string lat;       // <lat>49.02</lat>
        public string lon;       // <lon>12.1</lon>
        public string sunRise;   // <sunr>7:14 AM</sunr> 
        public string sunSet;    // <suns>5:38 PM</suns>
        public string zone;      // <zone>1</zone>
        // Getters
        public string CityCode { get { return cityCode; } }
        public string City { get { return city; } }
        public string Time { get { return time; } }
        public string Lat { get { return lat; } }
        public string Lon { get { return lon; } }
        public string SunRise { get { return sunRise; } }
        public string SunSet { get { return sunSet; } }
        public string Zone { get { return zone; } }
    }
    #endregion

    #region CurrentCondition struct
    /// <summary>
    /// current condition
    /// </summary>
    public struct CurrentCondition
    {
        public string city;              // <obst>
        public string lastUpdate;        // <lsup>
        public string temperature;       // <temp> 
        public string feelsLikeTemp;     // <flik>
        public string condition;         // <t>
        public string icon;              // <icon> 
        public string humidity;          // <hmid>
        public string wind;              // <wind>
        public string uVindex;           // <uv> 
        public string dewPoint;          // <dewp>
        // Getters :P
        public string City { get { return city; } }
        public string LastUpdate { get { return lastUpdate; } }
        public string Temperature { get { return temperature; } }
        public string FeelsLikeTemp { get { return feelsLikeTemp; } }
        public string Condition { get { return condition; } }
        public Uri Icon 
        {
            get
            {
                if (icon == String.Empty)
                    return new Uri(@"pack://siteoforigin:,,/Media/Weather/128x128/WEATHERALERT.png");
                // return the data
                return new Uri(@"pack://siteoforigin:,,/Media/Weather/128x128/" + icon.Replace('\\', '/'));
            }
        }
        public string Humidity { get { return humidity; } }
        public string Wind { get { return wind; } }
        public string UVIndex { get { return uVindex; } }
        public string DewPoint { get { return dewPoint; } }
    } 
    #endregion

    #region DayForeCast struct
    /// <summary>
    /// day forecast
    /// </summary>
    public struct DayForeCast
    {
        public string iconImageNameLow;
        public string iconImageNameHigh;
        public string overview;
        public string day;
        public string high;
        public string low;
        public string sunRise;
        public string sunSet;
        public string precipitation;
        public string humidity;
        public string wind;
        // Getters :P
        public Uri IconImageNameLow
        {
            get
            {
                if (iconImageNameLow == String.Empty)
                    return new Uri(@"pack://siteoforigin:,,/Media/Weather/128x128/WEATHERALERT.png");
                // return the data
                return new Uri(@"pack://siteoforigin:,,/Media/Weather/128x128/" + iconImageNameLow.Replace('\\', '/'));
            }
        }
        public Uri IconImageNameHigh
        {
            get
            {
                if (iconImageNameHigh == String.Empty)
                    return new Uri(@"pack://siteoforigin:,,/Media/Weather/128x128/WEATHERALERT.png");
                // return the data
                return new Uri(@"pack://siteoforigin:,,/Media/Weather/128x128/" + iconImageNameHigh.Replace('\\', '/'));
            }
        }
        public string Overview { get { return overview; } }
        public string Day { get { return day; } }
        public string High { get { return high; } }
        public string Low { get { return low; } }
        public string SunRise { get { return sunRise; } }
        public string SunSet { get { return sunSet; } }
        public string Precipitation { get { return precipitation; } }
        public string Humidity { get { return humidity; } }
        public string Wind { get { return wind; } }
    };

    #endregion    
    #endregion
}
