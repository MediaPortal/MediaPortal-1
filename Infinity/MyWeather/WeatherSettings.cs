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
        private List<string> _locationsList = new List<string>();
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
        // RefreshInterval in Millisecs
        [Setting(SettingScope.User, "60000")]
        public int RefreshInterval
        {
            get { return this._refreshInterval; }
            set { this._refreshInterval = value; }
        }
        // Location of the XML file to parse to
        [Setting(SettingScope.User, "c:\\xmlWeather.xml")]
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
        [Setting(SettingScope.User, "none")]
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
        [Setting(SettingScope.User, "")]
        public List<string> LocationsList
        {
            get { return this._locationsList; }
            set { this._locationsList = value; }
        }
    }
}
