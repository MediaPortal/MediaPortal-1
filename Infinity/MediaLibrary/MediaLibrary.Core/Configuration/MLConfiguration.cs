using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MediaLibrary.Configuration
{
    #region public class MLConfiguration
    /// <summary>
    /// Stores the plugin information for the library plug-ins.  
    /// </summary>
    public class MLConfiguration
    {
        #region Members

        private string _RootDirectory;
        private string _DataDirectory;
        private string _LibraryDirectory;
        private string _PluginsDirectory;
        private List<MLPluginConfiguration> _Plugins;

        #endregion

        #region Properties

        #region Serializable Properties

        #region public string RootDirectory
        /// <summary>
        /// Get/Sets the RootDirectory of the MLConfiguration
        /// </summary>
        /// <value></value>
        public string RootDirectory
        {
            get { return _RootDirectory; }
            set { _RootDirectory = value; }
        }
        #endregion

        #region public string DataDirectory
        /// <summary>
        /// Get/Sets the DataDirectory of the MLConfiguration
        /// </summary>
        /// <value></value>
        public string DataDirectory
        {
            get { return _DataDirectory; }
            set { _DataDirectory = value; }
        }
        #endregion

        #region public string LibraryDirectory
        /// <summary>
        /// Get/Sets the LibraryDirectory of the MLConfiguration
        /// </summary>
        /// <value></value>
        public string LibraryDirectory
        {
            get { return _LibraryDirectory; }
            set { _LibraryDirectory = value; }
        }
        #endregion

        #region public string PluginsDirectory
        /// <summary>
        /// Get/Sets the PluginsDirectory of the MLConfiguration
        /// </summary>
        /// <value></value>
        public string PluginsDirectory
        {
            get { return _PluginsDirectory; }
            set { _PluginsDirectory = value; }
        }
        #endregion

        #region public List<MLPluginConfiguration> Plugins
        /// <summary>
        /// Get/Sets the Plugins of the MLConfiguration
        /// </summary>
        /// <value></value>
        public List<MLPluginConfiguration> Plugins
        {
            get { return _Plugins; }
            set { _Plugins = value; }
        }
        #endregion
        
        #endregion

        #region Non-Serializable Properties

        #region public MLPluginConfiguration ActivePlugin
        /// <summary>
        /// Gets the ActivePlugin of the MLConfiguration
        /// </summary>
        /// <value></value>
        [XmlIgnore]
        public MLPluginConfiguration ActivePlugin
        {
            get
            {
                foreach (MLPluginConfiguration plugin in Plugins)
                    if ((bool)plugin.description["enabled"])
                        return plugin;
                return null;
            }
        }
        #endregion

        #endregion

        #endregion

        #region Constructors

        #region public MLConfiguration()
        /// <summary>
        /// Initializes a new instance of the <b>MLConfiguration</b> class.
        /// </summary>
        public MLConfiguration()
        {
            _RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            _LibraryDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "library");
            _PluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            _Plugins = new List<MLPluginConfiguration>();
            MLPluginConfiguration plugin = new MLPluginConfiguration();
            plugin.description["enabled"] = true;
            plugin.description["plugin_id"] = new Guid("f59e6640-a604-4df7-be43-c16f637551ea");  //SQLite plugin id
            plugin.plugin_properties["Path"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "library");
            _Plugins.Add(plugin);
        }
        #endregion

        #endregion

        #region Methods

        #region Static Methods

        #region public static MLConfiguration Deserialize(string filename)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static MLConfiguration Deserialize(string filename)
        {
            if (!System.IO.File.Exists(filename))
                return new MLConfiguration();

            XmlSerializer deserializer = new XmlSerializer(typeof(MLConfiguration));
            TextReader reader = new StreamReader(filename);
            MLConfiguration config = (MLConfiguration)deserializer.Deserialize(reader);
            reader.Close();
            return config;
        }
        #endregion

        #region public static void Serialize(MLConfiguration config, string filename)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="filename"></param>
        public static void Serialize(MLConfiguration config, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MLConfiguration));
            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, config);
            writer.Close();
        }
        #endregion

        #endregion

        #endregion
    } 
    #endregion
}
