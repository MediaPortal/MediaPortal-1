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

namespace MyWeather
{

    /// <summary>
    ///  Weather Forecast Class.
    /// </summary>
    public class WeatherDataModel
    {
        #region structs
        public struct LocInfo  // <loc>
        {
            public string CityCode;  // <loc id="GMXX0223">
            public string City;      // <dnam>Regensburg, Germany</dnam>
            public string Time;      // <tm>1:12 AM</tm>
            public string Lat;       // <lat>49.02</lat>
            public string Lon;       // <lon>12.1</lon>
            public string SunRise;   // <sunr>7:14 AM</sunr> 
            public string SunSet;    // <suns>5:38 PM</suns>
            public string Zone;      // <zone>1</zone>

        }

        public struct CurrentCondition
        {
            public string City;              // <obst>
            public string lastUpdate;        // <lsup>
            public string Temperature;       // <temp> 
            public string FeelsLikeTemp;     // <flik>
            public string Condition;         // <t>
            public string Icon;              // <icon> 
            public string Humidity;          // <hmid>
            public string Wind;              // <wind>
            public string UVindex;           // <uv> 
            public string DewPoint;          // <dewp> 
        }

        public struct DayForeCast
        {
            public string iconImageNameLow;
            public string iconImageNameHigh;
            public string Overview;
            public string Day;
            public string High;
            public string Low;
            public string SunRise;
            public string SunSet;
            public string Precipitation;
            public string Humidity;
            public string Wind;
        };
        #endregion


        #region Variables
        const int NUM_DAYS = 4;
        const char DEGREE_CHARACTER = (char)176;			//the degree 'o' character
        const string PARTNER_ID = "1004124588";			    //weather.com partner id
        const string PARTNER_KEY = "079f24145f208494";		//weather.com partner key

        // Private Variables
        string _settingsPath = "C:\\WeatherPlugin.xml";
        string _locationCode = "UKXX0085";
        string _temperatureFarenheit = "C";
        string _windSpeed = "K";
        string unitTemperature = String.Empty;
        string unitSpeed = String.Empty;


        // Public Variables
        public LocInfo LocalInfo;
        public CurrentCondition CurCondition;
        public DayForeCast[] DaysForeCast = new DayForeCast[NUM_DAYS];
        #endregion

        #region Constructors/Destructors

        public WeatherDataModel(string LocationCode)
        {
            _locationCode = LocationCode;
            LoadSettings();
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// updates the weatherinformation
        /// </summary>
        /// <returns>success state</returns>
        public bool Update()
        {
            WeatherSettings setting = new WeatherSettings();
            ServiceScope.Get<ISettingsManager>().Load(setting, _settingsPath);
            string weatherFile = setting.ParsefileLocation;
            if (Download(weatherFile))
            {
                if (ParseFile(weatherFile)) return true;
            }
            return false;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Load Settings for this plugin
        /// </summary>
        void LoadSettings()
        {
            WeatherSettings settings = new WeatherSettings();
            ServiceScope.Get<ISettingsManager>().Load(settings, _settingsPath);
            _temperatureFarenheit = settings.TemperatureFahrenheit;
            _windSpeed = settings.WindSpeed;
            _locationCode = settings.LocationCode;
        }

        /// <summary>
        /// dll Import to check Internet Connection
        /// </summary>
        /// <param name="Description"></param>
        /// <param name="ReservedValue"></param>
        /// <returns></returns>
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        /// <summary>
        /// check if we have an Internetconnection
        /// </summary>
        /// <param name="code"></param>
        /// <returns>true if Internetconnection is available</returns>
        public static bool IsConnectedToInternet(ref int code)
        {
            return InternetGetConnectedState(out code, 0);
        }

        /// <summary>
        /// download weather information to an xml file
        /// </summary>
        /// <param name="weatherFile">xml file to be downloaded to</param>
        /// <returns>success status</returns>
        bool Download(string weatherFile)
        {
            string url;

            bool skipConnectionTest = false;
            WeatherSettings settings = new WeatherSettings();
            ServiceScope.Get<ISettingsManager>().Load(settings, _settingsPath);
            skipConnectionTest = settings.SkipConnectionTest;
            ServiceScope.Get<ILogger>().Info("WeatherForecast.SkipConnectionTest: {0}", skipConnectionTest);

            int code = 0;
            
            if (!IsConnectedToInternet(ref code))
            {
                if (System.IO.File.Exists(weatherFile)) return true;

                ServiceScope.Get<ILogger>().Info("WeatherForecast.Download: No internet connection {0}", code);

                if (skipConnectionTest == false)
                    return false;
            }

            char c_units = _temperatureFarenheit[0];	//convert from temp units to metric/standard
            if (c_units == 'F')	//we'll convert the speed later depending on what thats set to
                c_units = 's';
            else
                c_units = 'm';

            url = String.Format("http://xoap.weather.com/weather/local/{0}?cc=*&unit={1}&dayf=4&prod=xoap&par={2}&key={3}",
              _locationCode, c_units.ToString(), PARTNER_ID, PARTNER_KEY);

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, weatherFile);
                    return true;
                }
                catch (Exception ex)
                {
                    ServiceScope.Get<ILogger>().Info("WeatherForecast: Failed to download weather:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
                }
            }
            return false;
        }

        /// <summary>
        /// parses the downloaded XML file
        /// </summary>
        /// <param name="weatherFile">the xml file that contains weather information</param>
        /// <returns>success state</returns>
        bool ParseFile(string weatherFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(weatherFile);
            if (doc.DocumentElement == null) return false;
            XmlNode xmlElement = doc.DocumentElement;

            if (doc.DocumentElement.Name == "error")
            {
                ParseError(xmlElement);
                return false;
            }

            unitTemperature = _temperatureFarenheit;
            if (_windSpeed[0] == 'M') unitSpeed = "mph";
            else if (_windSpeed[0] == 'K') unitSpeed = "km/h";
            else unitSpeed = "m/s";

            ParseLocation(xmlElement.SelectSingleNode("loc"));
            ParseCurrentCondition(xmlElement.SelectSingleNode("cc"));
            ParseDayForeCast(xmlElement.SelectSingleNode("dayf"));
            return true;
        }

        /// <summary>
        /// writes an error message to the log
        /// if parsing went wrong
        /// </summary>
        /// <param name="xmlElement"></param>
        void ParseError(XmlNode xmlElement)
        {
            ServiceScope.Get<ILogger>().Info("WeatherForecast.ParseFile: Error = " + GetString(xmlElement, "err", "Unknown Error"));
        }

        /// <summary>
        /// parses the locations
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <returns></returns>
        bool ParseLocation(XmlNode xmlElement)
        {
            if (xmlElement == null) return false;

            LocalInfo.CityCode = GetString(xmlElement, "loc", String.Empty);  // <loc id="GMXX0223">
            LocalInfo.City = GetString(xmlElement, "dnam", String.Empty);     // <dnam>Regensburg, Germany</dnam>
            LocalInfo.Time = GetString(xmlElement, "tm", String.Empty);       // <tm>1:12 AM</tm>
            LocalInfo.Lat = GetString(xmlElement, "lat", String.Empty);       // <lat>49.02</lat>
            LocalInfo.Lon = GetString(xmlElement, "lon", String.Empty);       // <lon>12.1</lon>
            LocalInfo.SunRise = GetString(xmlElement, "sunr", String.Empty);  // <sunr>7:14 AM</sunr> 
            LocalInfo.SunSet = GetString(xmlElement, "suns", String.Empty);   // <suns>5:38 PM</suns>
            LocalInfo.Zone = GetString(xmlElement, "zone", String.Empty);     // <zone>1</zone>

            return true;
        }

        /// <summary>
        /// parses the current condition
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <returns></returns>
        bool ParseCurrentCondition(XmlNode xmlElement)
        {
            if (xmlElement == null) return false;

            CurCondition.lastUpdate = RelocalizeDateTime(GetString(xmlElement, "lsup", String.Empty));
            CurCondition.City = GetString(xmlElement, "obst", String.Empty);
            //CurCondition.Icon = Config.GetFile(Config.Dir.Weather, String.Format(@"128x128\{0}.png", GetInteger(xmlElement, "icon")));
            CurCondition.Condition = LocalizeOverview(GetString(xmlElement, "t", String.Empty));
            SplitLongString(ref CurCondition.Condition, 8, 15);				//split to 2 lines if needed
            CurCondition.Temperature = String.Format("{0}{1}{2}", GetInteger(xmlElement, "tmp"), DEGREE_CHARACTER, unitTemperature);
            CurCondition.FeelsLikeTemp = String.Format("{0}{1}{2}", GetInteger(xmlElement, "flik"), DEGREE_CHARACTER, unitTemperature);
            CurCondition.Wind = ParseWind(xmlElement.SelectSingleNode("wind"), unitSpeed);
            CurCondition.Humidity = String.Format("{0}%", GetInteger(xmlElement, "hmid"));
            CurCondition.UVindex = ParseUVIndex(xmlElement.SelectSingleNode("uv"));
            CurCondition.DewPoint = String.Format("{0}{1}{2}", GetInteger(xmlElement, "dewp"), DEGREE_CHARACTER, unitTemperature);

            return true;
        }

        /// <summary>
        /// parses the dayforecast
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <returns></returns>
        bool ParseDayForeCast(XmlNode xmlElement)
        {
            if (xmlElement == null) return false;
            XmlNode element = xmlElement.SelectSingleNode("day");
            for (int i = 0; i < NUM_DAYS; i++)
            {
                if (element != null)
                {
                    DaysForeCast[i].Day = LocalizeDay(element.Attributes.GetNamedItem("t").InnerText);

                    DaysForeCast[i].High = GetString(element, "hi", String.Empty);	//string cause i've seen it return N/A
                    if (DaysForeCast[i].High == "N/A") DaysForeCast[i].High = String.Empty;
                    else DaysForeCast[i].High = String.Format("{0}{1}{2}", DaysForeCast[i].High, DEGREE_CHARACTER, unitTemperature);

                    DaysForeCast[i].Low = GetString(element, "low", String.Empty);
                    if (DaysForeCast[i].Low == "N/A") DaysForeCast[i].Low = String.Empty;
                    else DaysForeCast[i].Low = String.Format("{0}{1}{2}", DaysForeCast[i].Low, DEGREE_CHARACTER, unitTemperature);

                    DaysForeCast[i].SunRise = GetString(element, "sunr", String.Empty);
                    if (DaysForeCast[i].SunRise == "N/A") DaysForeCast[i].SunRise = String.Empty;
                    else DaysForeCast[i].SunRise = String.Format("{0}", RelocalizeTime(DaysForeCast[i].SunRise));

                    DaysForeCast[i].SunSet = GetString(element, "suns", String.Empty);
                    if (DaysForeCast[i].SunSet == "N/A") DaysForeCast[i].SunSet = String.Empty;
                    else DaysForeCast[i].SunSet = String.Format("{0}", RelocalizeTime(DaysForeCast[i].SunSet));

                    XmlNode dayElement = element.SelectSingleNode("part");	//grab the first day/night part (should be day)
                    if (dayElement != null && i == 0)
                    {
                        // If day forecast is not available (at the end of the day), show night forecast
                        if (GetString(dayElement, "t", String.Empty) == "N/A") dayElement = dayElement.NextSibling;
                    }

                    if (dayElement != null)
                    {
                        //_dayForeCast[i].iconImageNameLow = Config.GetFile(Config.Dir.Weather, String.Format("64x64\\{0}.png", GetInteger(dayElement, "icon")));
                        //_dayForeCast[i].iconImageNameHigh = Config.GetFile(Config.Dir.Weather, String.Format("128x128\\{0}.png", GetInteger(dayElement, "icon")));
                        DaysForeCast[i].Overview = LocalizeOverview(GetString(dayElement, "t", String.Empty));
                        SplitLongString(ref DaysForeCast[i].Overview, 6, 15);
                        DaysForeCast[i].Humidity = String.Format("{0}%", GetInteger(dayElement, "hmid"));
                        DaysForeCast[i].Precipitation = String.Format("{0}%", GetInteger(dayElement, "ppcp"));
                    }
                    DaysForeCast[i].Wind = ParseWind(dayElement.SelectSingleNode("wind"), unitSpeed);
                }
                element = element.NextSibling;//Element("day");
            }

            return true;
        }
        /// <summary>
        /// helper function
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <param name="tagName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string GetString(XmlNode xmlElement, string tagName, string defaultValue)
        {
            string value = String.Empty;

            try
            {
                XmlNode node = xmlElement.SelectSingleNode(tagName);
                if (node != null)
                {
                    if (node.InnerText != null)
                    {
                        if (node.InnerText != "-")
                            value = node.InnerText;
                    }
                }
            }
            catch { }
            if (value.Length == 0) return defaultValue;
            return value;
        }

        /// <summary>
        /// helper function
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <param name="tagName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        int GetInteger(XmlNode xmlElement, string tagName)
        {
            int value = 0;
            XmlNode node = xmlElement.SelectSingleNode(tagName);
            if (node != null)
            {
                if (node.InnerText != null)
                {
                    try
                    {
                        value = Int32.Parse(node.InnerText);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return value;
        }

        //splitStart + End are the chars to search between for a space to replace with a \n
        void SplitLongString(ref string lineString, int splitStart, int splitEnd)
        {
            //search chars 10 to 15 for a space
            //if we find one, replace it with a newline
            for (int i = splitStart; i < splitEnd && i < (int)lineString.Length; i++)
            {
                if (lineString[i] == ' ')
                {
                    lineString = lineString.Substring(0, i) + "\n" + lineString.Substring(i + 1);
                    return;
                }
            }
        }

        /// <summary>
        /// relocalizes the timezone
        /// </summary>
        /// <param name="usFormatTime"></param>
        /// <returns></returns>
        string RelocalizeTime(string usFormatTime)
        {
            string result = usFormatTime;

            string[] tokens = result.Split(' ');

            if (tokens.Length == 2)
            {
                try
                {
                    string[] timePart = tokens[0].Split(':');
                    DateTime now = DateTime.Now;
                    DateTime time = new DateTime(
                              now.Year,
                              now.Month,
                              now.Day,
                              Int32.Parse(timePart[0]) + (String.Compare(tokens[1], "PM", true) == 0 ? 12 : 0),
                              Int32.Parse(timePart[1]),
                              0
                    );

                    result = time.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
                }
                catch (Exception)
                {
                    // default value is ok
                }
            }
            return result;
        }

        /// <summary>
        /// convert weather.com day strings into localized string id's
        /// </summary>
        /// <param name="dayName"></param>
        /// <returns></returns>
        string LocalizeDay(string dayName)
        {
            switch (dayName)
            {
                case "Sunday": return ServiceScope.Get<ILocalisation>().ToString("days",0);
                case "Monday": return ServiceScope.Get<ILocalisation>().ToString("days",1);
                case "Tuesday": return ServiceScope.Get<ILocalisation>().ToString("days",2);
                case "Wednesday": return ServiceScope.Get<ILocalisation>().ToString("days",3);
                case "Thursday": return ServiceScope.Get<ILocalisation>().ToString("days",4);
                case "Frieday": return ServiceScope.Get<ILocalisation>().ToString("days",5);
                case "Satureday": return ServiceScope.Get<ILocalisation>().ToString("days",6);
                default: return String.Empty;
            }
            /*
             * string localizedDay = String.Empty;
            if (dayName == "Monday")			//monday is localized string 11
              localizedDay = GUILocalizeStrings.Get(11);
            else if (dayName == "Tuesday")
              localizedDay = GUILocalizeStrings.Get(12);
            else if (dayName == "Wednesday")
              localizedDay = GUILocalizeStrings.Get(13);
            else if (dayName == "Thursday")
              localizedDay = GUILocalizeStrings.Get(14);
            else if (dayName == "Friday")
              localizedDay = GUILocalizeStrings.Get(15);
            else if (dayName == "Saturday")
              localizedDay = GUILocalizeStrings.Get(16);
            else if (dayName == "Sunday")
              localizedDay = GUILocalizeStrings.Get(17);
            else
              localizedDay = String.Empty;

            return localizedDay;
             */
        }
        /// <summary>
        /// relocalizes the date time
        /// </summary>
        /// <param name="usFormatDateTime"></param>
        /// <returns>localized string</returns>
        string RelocalizeDateTime(string usFormatDateTime)
        {
            string result = usFormatDateTime;

            string[] tokens = result.Split(' ');

            // A safety check
            if ((tokens.Length == 5) &&
                 (String.Compare(tokens[3], "Local", true) == 0) && (String.Compare(tokens[4], "Time", true) == 0))
            {
                try
                {
                    string[] datePart = tokens[0].Split('/');
                    string[] timePart = tokens[1].Split(':');
                    DateTime time = new DateTime(
                        2000 + Int32.Parse(datePart[2]),
                        Int32.Parse(datePart[0]),
                        Int32.Parse(datePart[1]),
                        Int32.Parse(timePart[0]) + (String.Compare(tokens[2], "PM", true) == 0 ? 12 : 0),
                        Int32.Parse(timePart[1]),
                        0
                    );
                    result = time.ToString("f", CultureInfo.CurrentCulture.DateTimeFormat);
                }
                catch (Exception)
                {
                    // default value is ok
                }
            }
            return result;
        }
        /// <summary>
        /// localizes overview strings
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        string LocalizeOverview(string token)
        {
            string localizedLine = String.Empty;

            foreach (string tokenSplit in token.Split(' '))
            {
                string localizedWord = String.Empty;

                if (String.Compare(tokenSplit, "T-Storms", true) == 0 || String.Compare(tokenSplit, "T-Storm", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",370);
                else if (String.Compare(tokenSplit, "Partly", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",371);
                else if (String.Compare(tokenSplit, "Mostly", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",372);
                else if (String.Compare(tokenSplit, "Sunny", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",373);
                else if (String.Compare(tokenSplit, "Cloudy", true) == 0 || String.Compare(tokenSplit, "Clouds", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",374);
                else if (String.Compare(tokenSplit, "Snow", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",375);
                else if (String.Compare(tokenSplit, "Rain", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",376);
                else if (String.Compare(tokenSplit, "Light", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",377);
                else if (String.Compare(tokenSplit, "AM", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",378);
                else if (String.Compare(tokenSplit, "PM", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",379);
                else if (String.Compare(tokenSplit, "Showers", true) == 0 || String.Compare(tokenSplit, "Shower", true) == 0 || String.Compare(tokenSplit, "T-Showers", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",380);
                else if (String.Compare(tokenSplit, "Few", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",381);
                else if (String.Compare(tokenSplit, "Scattered", true) == 0 || String.Compare(tokenSplit, "Isolated", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",382);
                else if (String.Compare(tokenSplit, "Wind", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",383);
                else if (String.Compare(tokenSplit, "Strong", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",384);
                else if (String.Compare(tokenSplit, "Fair", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",385);
                else if (String.Compare(tokenSplit, "Clear", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",386);
                else if (String.Compare(tokenSplit, "Early", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",387);
                else if (String.Compare(tokenSplit, "and", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",388);
                else if (String.Compare(tokenSplit, "Fog", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",389);
                else if (String.Compare(tokenSplit, "Haze", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",390);
                else if (String.Compare(tokenSplit, "Windy", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",391);
                else if (String.Compare(tokenSplit, "Drizzle", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",392);
                else if (String.Compare(tokenSplit, "Freezing", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",393);
                else if (String.Compare(tokenSplit, "N/A", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",394);
                else if (String.Compare(tokenSplit, "Mist", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",395);
                else if (String.Compare(tokenSplit, "High", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",799);
                else if (String.Compare(tokenSplit, "Low", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",798);
                else if (String.Compare(tokenSplit, "Moderate", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",534);
                else if (String.Compare(tokenSplit, "Late", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",553);
                else if (String.Compare(tokenSplit, "Very", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",554);
                // wind directions
                else if (String.Compare(tokenSplit, "N", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",535);
                else if (String.Compare(tokenSplit, "E", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",536);
                else if (String.Compare(tokenSplit, "S", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",537);
                else if (String.Compare(tokenSplit, "W", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",538);
                else if (String.Compare(tokenSplit, "NE", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",539);
                else if (String.Compare(tokenSplit, "SE", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",540);
                else if (String.Compare(tokenSplit, "SW", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",541);
                else if (String.Compare(tokenSplit, "NW", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",542);
                else if (String.Compare(tokenSplit, "Thunder", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",543);
                else if (String.Compare(tokenSplit, "NNE", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",544);
                else if (String.Compare(tokenSplit, "ENE", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",545);
                else if (String.Compare(tokenSplit, "ESE", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",546);
                else if (String.Compare(tokenSplit, "SSE", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",547);
                else if (String.Compare(tokenSplit, "SSW", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",548);
                else if (String.Compare(tokenSplit, "WSW", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",549);
                else if (String.Compare(tokenSplit, "WNW", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",551);
                else if (String.Compare(tokenSplit, "NNW", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",552);
                else if (String.Compare(tokenSplit, "VAR", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",556);
                else if (String.Compare(tokenSplit, "CALM", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",557);
                else if (String.Compare(tokenSplit, "Storm", true) == 0 || String.Compare(tokenSplit, "Gale", true) == 0 || String.Compare(tokenSplit, "Tempest", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",599);
                else if (String.Compare(tokenSplit, "in the Vicinity", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",559);
                else if (String.Compare(tokenSplit, "Clearing", true) == 0)
                    localizedWord = ServiceScope.Get<ILocalisation>().ToString("myweather",560);

                if (localizedWord == String.Empty)
                    localizedWord = tokenSplit;	//if not found, let fallback

                localizedLine = localizedLine + localizedWord;
                localizedLine += " ";
            }
            return localizedLine;
        }

        /// <summary>
        /// converts speed to other metrics
        /// </summary>
        /// <param name="curSpeed"></param>
        /// <returns></returns>
        int ConvertSpeed(int curSpeed)
        {
            //we might not need to convert at all
            if ((_temperatureFarenheit[0] == 'C' && _windSpeed[0] == 'K') ||
              (_temperatureFarenheit[0] == 'F' && _windSpeed[0] == 'M'))
                return curSpeed;

            //got through that so if temp is C, speed must be M or S
            if (_temperatureFarenheit[0] == 'C')
            {
                if (_windSpeed[0] == 'S')
                    return (int)(curSpeed * (1000.0 / 3600.0) + 0.5);		//mps
                else
                    return (int)(curSpeed / (8.0 / 5.0));		//mph
            }
            else
            {
                if (_windSpeed[0] == 'S')
                    return (int)(curSpeed * (8.0 / 5.0) * (1000.0 / 3600.0) + 0.5);		//mps
                else
                    return (int)(curSpeed * (8.0 / 5.0));		//kph
            }
        }

        /// <summary>
        /// parses the uv index
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        string ParseUVIndex(XmlNode element)
        {
            if (element == null) return String.Empty;
            return String.Format("{0} {1}", GetInteger(element, "i"), LocalizeOverview(GetString(element, "t", String.Empty)));
        }

        /// <summary>
        /// parses the wind
        /// </summary>
        /// <param name="node"></param>
        /// <param name="unitSpeed"></param>
        /// <returns></returns>
        string ParseWind(XmlNode node, string unitSpeed)
        {
            if (node == null) return String.Empty;

            string wind = String.Empty;
            int tempInteger = ConvertSpeed(GetInteger(node, "s"));				   //convert speed if needed
            string tempString = LocalizeOverview(GetString(node, "t", "N")); //current wind direction

            if (tempInteger != 0) // Have wind
            {
                //From <dir eg NW> at <speed> km/h	
                string format = ServiceScope.Get<ILocalisation>().ToString("myweather", 555);
                if (format == "")
                    format = "From {0} at {1} {2}";
                wind = String.Format(format, tempString, tempInteger, unitSpeed);
            }
            else // Calm
            {
                wind = ServiceScope.Get<ILocalisation>().ToString("myweather", 558);
                if (wind == "")
                    wind = "No wind";
            }
            return wind;
        }

        #endregion


        #region <Interface> Implementations
        // region for each interface
        #endregion
    }
}