using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Localisation;

namespace MyWeather
{
    /// <summary>
    /// class that Localizes static My Weather labels
    /// localized Object is exported by the ViewModel
    /// </summary>
    public class WeatherLocalizer
    {
        /// <summary>
        /// Gets the current date
        /// </summary>
        /// <value>The date label.</value>
        public string Date
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
        public string Location
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
        public string Refresh
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 2); //Refresh
            }
        }

        /// <summary>
        /// Gets the the localized version of the header label.
        /// </summary>
        /// <value>The header label.</value>
        public virtual string Header
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 3); //weather
            }
        }
        /// <summary>
        /// Gets the the localized version of the temperature label.
        /// </summary>
        public virtual string Temperature
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 6); //Temp
            }
        }
        /// <summary>
        /// Gets the the localized version of the feelslike label.
        /// </summary>
        public virtual string FeelsLike
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 7); //Feels Like
            }
        }
        /// <summary>
        /// Gets the the localized version of the uvindex label.
        /// </summary>
        public virtual string UVIndex
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 8); //UVIndex
            }
        }
        /// <summary>
        /// Gets the the localized version of the wind label.
        /// </summary>
        public virtual string Wind
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 9); //Wind
            }
        }
        /// <summary>
        /// Gets the the localized version of the humidity label.
        /// </summary>
        public virtual string Humidity
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 10); //Humidity
            }
        }
        /// <summary>
        /// Gets the the localized version of the dewpoint label.
        /// </summary>
        public virtual string DewPoint
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 11); //Dew Point
            }
        }
        /// <summary>
        /// Gets the the localized version of the sunrise label.
        /// </summary>
        public virtual string Sunrise
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 12); //Sunrise
            }
        }
        /// <summary>
        /// Gets the the localized version of the sunset label.
        /// </summary>
        public virtual string Sunset
        {
            get
            {
                return ServiceScope.Get<ILocalisation>().ToString("myweather", 13); //Sunset
            }
        }

    }
}
