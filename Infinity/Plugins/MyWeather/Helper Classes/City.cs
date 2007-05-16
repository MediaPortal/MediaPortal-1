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
    /// this is a basic field where
    /// the information for the provider
    /// are stored (usually the location name and unique id)
    /// </summary>
    public class CitySetupInfo
    {
        private string name, id;

        public CitySetupInfo(string name, string id)
        {
            this.name = name;
            this.id = id;
        }

        public CitySetupInfo() { }

        /// <summary>
        /// Get the Name of the City
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Get the Location ID
        /// </summary>
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public override string ToString()
        {
            return name;
        }
    }

    #region City class
    /// <summary>
    /// holds Information on the City
    /// </summary>
    public class City : CitySetupInfo
    {
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
            _updateSuccessful = false;
            
        }

        public City(CitySetupInfo info)
        {
            Name = info.Name;
            Id = info.Id;
            _updateSuccessful = false;
        }

        public City(string name, string id)
        {
            this.Name = name;
            this.Id = id;
            _updateSuccessful = false;
        }

        #region properties

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
    }
    #endregion

    #region structs
    #region LocInfo struct
    /// <summary>
    /// provides info of the Location
    /// </summary>
    public struct LocInfo  // <loc>
    {
        public void FillWithDummyData()
        {
            cityCode = "DUMMY002351";
            city = "Dummytown, Dummyland";
            time = "23:15";
            lat = "49.02";
            lon = "12.1";
            sunRise = "7:14 AM";
            sunSet = "5:38 PM";
            zone = "1";
        }
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
        public void FillWithDummyData()
        {
            city = "Dummytown, Dummyland";
            lastUpdate = "Friday, 12 May 2007";
            temperature = "12";
            feelsLikeTemp = "16";
            condition = "Partly \n Cloudy";
            icon = "30.png";
            humidity = "70%";
            wind = "200 mph";
            uVindex = "10";
            dewPoint = "30%";
        }
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


        /// <summary>
        /// constructor for designer
        /// </summary>
        /// <param name="day"></param>
        public DayForeCast(int day)
        {
            if (day == 0)
            {
                iconImageNameLow = "16.png";
                iconImageNameHigh = "16.png";
                overview = "this is an overview";
                this.day = "Monday";
                high = "13";
                low = "8";
                sunRise = "5:00 AM";
                sunSet = "7:00 PM";
                precipitation = "Partly\nCloudy";
                humidity = "54%";
                wind = "30 mph";
            }
            else if (day == 1)
            {
                iconImageNameLow = "36.png";
                iconImageNameHigh = "36.png";
                overview = "this is an overview";
                this.day = "Tuesday";
                high = "25";
                low = "20";
                sunRise = "5:10 AM";
                sunSet = "7:10 PM";
                precipitation = "Sunny";
                humidity = "30%";
                wind = "0 mph";
            }
            else if (day == 2)
            {
                iconImageNameLow = "34.png";
                iconImageNameHigh = "34.png";
                overview = "this is an overview";
                this.day = "Wednesday";
                high = "20";
                low = "11";
                sunRise = "5:20 AM";
                sunSet = "7:20 PM";
                precipitation = "Fog";
                humidity = "84%";
                wind = "55 mph";
            }
            else
            {
                iconImageNameLow = "38.png";
                iconImageNameHigh = "38.png";
                overview = "this is an overview";
                this.day = "Thursday";
                high = "12";
                low = "0";
                sunRise = "5:40 AM";
                sunSet = "7:40 PM";
                precipitation = "Storm and\nThunder";
                humidity = "89%";
                wind = "100 mph";
            }
        }

        #region Dummy Data for Designer
        /// <summary>
        /// fills up some dummy data
        /// </summary>
        /// <param name="day"></param>
        public void FillWithDummyData(int day)
        {
            if (day == 0)
            {
                iconImageNameLow = "16.png";
                iconImageNameHigh = "16.png";
                overview = "this is an overview";
                this.day = "Monday";
                high = "13";
                low = "8";
                sunRise = "5:00 AM";
                sunSet = "7:00 PM";
                precipitation = "Partly\nCloudy";
                humidity = "54%";
                wind = "30 mph";
            }
            else if (day == 1)
            {
                iconImageNameLow = "36.png";
                iconImageNameHigh = "36.png";
                overview = "this is an overview";
                this.day = "Tuesday";
                high = "25";
                low = "20";
                sunRise = "5:10 AM";
                sunSet = "7:10 PM";
                precipitation = "Sunny";
                humidity = "30%";
                wind = "0 mph";
            }
            else if (day == 2)
            {
                iconImageNameLow = "34.png";
                iconImageNameHigh = "34.png";
                overview = "this is an overview";
                this.day = "Wednesday";
                high = "20";
                low = "11";
                sunRise = "5:20 AM";
                sunSet = "7:20 PM";
                precipitation = "Fog";
                humidity = "84%";
                wind = "55 mph";
            }
            else
            {
                iconImageNameLow = "38.png";
                iconImageNameHigh = "38.png";
                overview = "this is an overview";
                this.day = "Thursday";
                high = "12";
                low = "0";
                sunRise = "5:40 AM";
                sunSet = "7:40 PM";
                precipitation = "Storm and\nThunder";
                humidity = "89%";
                wind = "100 mph";
            }

        }
        #endregion

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
                if (iconImageNameHigh ==  String.Empty)
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
