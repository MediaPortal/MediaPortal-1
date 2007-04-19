using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MediaLibrary.Configuration
{
    #region public class MLPluginConfiguration
    /// <summary>
    /// MLPluginConfiguration holds the configured properties of each plugin
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    public class MLPluginConfiguration
    {
        #region serializable properties

        private MLHashItem _description;
        public IMLHashItem description
        {
            get { return this._description; }
            set { this._description = (value as MLHashItem); }
        }

        private MLHashItem _plugin_properties;
        public IMLHashItem plugin_properties
        {
            get { return this._plugin_properties; }
            set { this._plugin_properties = (value as MLHashItem); }
        }

        #endregion

        #region public MLPluginConfiguration()
        /// <summary>
        /// Initializes a new instance of the <b>MLPluginConfiguration</b> class.
        /// </summary>
        public MLPluginConfiguration()
        {
            this._description = new MLHashItem();
            this._plugin_properties = new MLHashItem();
        }
        #endregion
    } 
    #endregion

}
