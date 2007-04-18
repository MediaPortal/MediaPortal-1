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
using System.Net;
using System.Xml;
using System.Globalization;
using System.Runtime.InteropServices;
using ProjectInfinity.Settings;
using ProjectInfinity.Logging;
using ProjectInfinity;
using ProjectInfinity.Localisation;
using System.Collections.Generic;

namespace MyWeather
{
    /// <summary>
    ///  WeatherDataModel
    /// </summary>
    public class WeatherDataModel
    {
        private IWeatherCatcher _weatherCatcher;

        /// <summary>
        /// construct the datamodel
        /// </summary>
        /// <param name="catcher"></param>
        public WeatherDataModel(IWeatherCatcher catcher)
        {
            _weatherCatcher = catcher;
        }

        /// <summary>
        /// sets or gets the weathercatcher 
        /// for this datamodel
        /// </summary>
        public IWeatherCatcher WeatherCatcher
        {
            get
            {
                return _weatherCatcher;
            }
            set
            {
                _weatherCatcher = value;
            }
        }

        /// <summary>
        /// this will retrieve data for all locations
        /// that can be found in the configuration file
        /// (configuration.xml)
        /// </summary>
        /// <returns></returns>
        public List<City> LoadLocationsData()
        {
            // look in the settings what locations are configured
            // and create a new list full of data by downloading and setting
            // it...
            WeatherSettings settings = new WeatherSettings();
            ServiceScope.Get<ISettingsManager>().Load(settings, "configuration.xml");
            // check if loading went well, if not return null
            if(settings.LocationsList == null) 
                return null;
            // now we got the provider info with id and name already filled in... lets get the
            // rest of the data! first turn the CityProviderInfo List into a City List
            List<City> citiesList = Helper.CityInfoListToCityObjectList(settings.LocationsList);

            foreach (City c in citiesList)
            {
                WeatherCatcher.GetLocationData(c);
            }
            // nice, we should have it now... cities with none-successful update
            // will have the City.HasData attribute set to false
            return citiesList;
        }
    }
}