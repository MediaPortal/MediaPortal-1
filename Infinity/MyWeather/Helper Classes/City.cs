using System;
using System.Collections.Generic;
using System.Text;

namespace MyWeather
{
    #region City Class
    /// <summary>
    /// holds Information on the City
    /// </summary>
    public class City
    {
        public string name;
        public string id;

        /// <summary>
        /// parameterless constructor
        /// needed for serialization
        /// </summary>
        public City() { }

        public City(string name, string id)
        {
            this.name = name;
            this.id = id;
        }

        /// <summary>
        /// Get the Name of the City
        /// </summary>
        public string Name
        {
            get
            {
                return name;
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
        }

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

}
