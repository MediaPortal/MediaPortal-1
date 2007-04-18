using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Settings;

namespace MyWeather
{
    /// <summary>
    /// Sample settings class wich will implement your own settings object in your code/plugin
    /// Only public properties are stored/retrieved
    /// </summary>
    public class WeatherSettings
    {
        private int _refreshInterval;
        private List<CitySetupInfo> _locationsList = new List<CitySetupInfo>();
        private string _parseFileLocation;
        private string _temperatureFahrenheit;
        private string _windSpeed;
        private string _locationCode;
        private bool _skipConnectionTest;

        /// <summary>
        /// Default Ctor
        /// </summary>
        public WeatherSettings()
        {
        }
        /// <summary>
        /// Scope and default value attribute
        /// </summary>
        // RefreshInterval in Seconds
        [Setting(SettingScope.User, "600")]
        public int RefreshInterval
        {
            get { return this._refreshInterval; }
            set { this._refreshInterval = value; }
        }
        // Location of the XML files to parse to
        [Setting(SettingScope.User, "Media/Weather/location{0}.xml")]
        public string ParsefileLocation
        {
            get { return this._parseFileLocation; }
            set { this._parseFileLocation = value; }
        }
        [Setting(SettingScope.User, "C")]
        public string TemperatureFahrenheit
        {
            get { return this._temperatureFahrenheit; }
            set { this._temperatureFahrenheit = value; }
        }
        [Setting(SettingScope.User, "mph")]
        public string WindSpeed
        {
            get { return this._windSpeed; }
            set { this._windSpeed = value; }
        }
        [Setting(SettingScope.User, "<none>")]
        public string LocationCode
        {
            get { return this._locationCode; }
            set { this._locationCode = value; }
        }
        [Setting(SettingScope.User, "false")]
        public bool SkipConnectionTest
        {
            get { return this._skipConnectionTest; }
            set { this._skipConnectionTest = value; }
        }
        [Setting(SettingScope.User, "null")]
        public List<CitySetupInfo> LocationsList
        {
            get { return this._locationsList; }
            set { this._locationsList = value; }
        }
    }
}
