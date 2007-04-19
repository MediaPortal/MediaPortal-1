using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Collections.Generic;
using MediaLibrary.Configuration;

namespace MediaLibrary
{
    public class MLSystem : IMLSystem
    {
        private MLConfiguration _Configuration;

        public MLSystem()
        {
            _Configuration = new MLConfiguration();
        }


        [XmlIgnore]
        public MLConfiguration Configuration
        {
            get { return _Configuration; }
            set { _Configuration = value; }
        }

        public static MLSystem Deserialize(string ConfigPath)
        {
            string filename = Path.Combine(ConfigPath, "MediaLibrary.xml");
            if (!System.IO.File.Exists(filename))
                return new MLSystem();

            MLSystem system = new MLSystem();
            system.Configuration = MLConfiguration.Deserialize(filename);
            return system;
        }

        public static void Serialize(MLSystem Config, string ConfigPath)
        {
            string filename = Path.Combine(ConfigPath, "MediaLibrary.xml");
            MLConfiguration.Serialize(Config.Configuration, ConfigPath);
        }

        public static Type GetTypeFromPlugin(string path)
        {
            Assembly SampleAssembly;

            SampleAssembly = Assembly.LoadFile(Path.GetFullPath(path));

            Type[] types = SampleAssembly.GetTypes();

            foreach (Type t in types)
            {
                if (t.GetInterface("MediaLibrary.IMLDatabasePlugin") != null)
                {
                    return t;
                }
                if (t.GetInterface("MediaLibrary.IMLImportPlugin") != null)
                {
                    return t;
                }
            }
            return null;
        }

        private string GetDirectory(string Value, string DefaultValue, string CreateSubdirectory)
        {
            string s;
            if (string.IsNullOrEmpty(Value))
            {
                if(string.IsNullOrEmpty(CreateSubdirectory))
                    s = DefaultValue;
                else
                    s = Path.Combine(DefaultValue, CreateSubdirectory);
                if (!Directory.Exists(s))
                {
                    Directory.CreateDirectory(s);
                }
                return s;
            }
            else
            {
                if (string.IsNullOrEmpty(CreateSubdirectory))
                    s = Value;
                else
                    s = Path.Combine(Value, CreateSubdirectory);
                if (!Directory.Exists(s))
                {
                    Directory.CreateDirectory(s);
                }
                return s;
            }
        }

        public IMLHashItemList GetInstalledPlugins(string PluginType)
        {
            // PluginType: "", "import", or "database"
            IMLHashItemList pluginlist = new MLHashItemList();
            IMLHashItem plugin;
            MLPluginDescription desc;
            string[] sa;

            sa = Directory.GetFiles(GetPluginsDirectory(PluginType), "*.mlpd", SearchOption.AllDirectories);

            foreach (string s in sa)
            {
                desc = MLPluginDescription.Deserialize(s);

                plugin = new MLHashItem();
                plugin["path"] = Path.Combine(Path.GetDirectoryName(s), desc.installation.main_file);
                plugin["name"] = desc.information.plugin_name;
                plugin["description"] = desc;

                pluginlist.Add(plugin);
            }
            return pluginlist;
        }

        public string GetRootDirectory(string CreateSubdirectory)
        {
            return GetDirectory(this.Configuration.RootDirectory, ".\\", CreateSubdirectory);
        }

        public string GetDataDirectory(string CreateSubdirectory)
        {
            return GetDirectory(this.Configuration.DataDirectory, ".\\data", CreateSubdirectory);
        }

        public string GetLibraryDirectory(string CreateSubdirectory)
        {
            return GetDirectory(this.Configuration.LibraryDirectory, ".\\library", CreateSubdirectory);
        }

        public string GetPluginsDirectory(string CreateSubdirectory)
        {
            return GetDirectory(this.Configuration.PluginsDirectory, ".\\plugins", CreateSubdirectory);
        }

        public IMLHashItem NewHashItem()
        {
            return new MLHashItem();
        }

        public IMLHashItemList NewHashItemList()
        {
            return new MLHashItemList();
        }

    }
}
