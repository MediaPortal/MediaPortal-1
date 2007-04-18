using System;
using System.Collections.Generic;
using System.Text;

namespace MyWeather
{
    /// <summary>
    /// this is an Interface for catching weather data
    /// from a website...
    /// any Implementation will take a City object and populate it with data
    /// </summary>
    public interface IWeatherCatcher
    {
        /// <summary>
        /// downloads data from the internet and populates
        /// the city object with it.
        /// the city object should already hold id and name
        /// (f.e. from FindLoationsByName or Settings)
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        bool GetLocationData(City city);
        /// <summary>
        /// returns a new List of City objects if the searched
        /// location was found... the unique id (City.Id) and the
        /// location name will be set for each City...
        /// </summary>
        /// <param name="name">name of the location to search for</param>
        /// <returns>new City List</returns>
        List<City> FindLocationsByName(string name);
    }
}