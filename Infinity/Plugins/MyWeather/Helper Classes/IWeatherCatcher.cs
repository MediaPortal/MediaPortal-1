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
        List<CitySetupInfo> FindLocationsByName(string name);
    }
}