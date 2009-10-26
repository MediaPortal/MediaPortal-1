using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Interfaces;
using Microsoft.Win32;

namespace MpeCore.Classes.PathProvider
{
    class TvServerPaths : IPathProvider
    {
        private Dictionary<string, string> _paths;

        public TvServerPaths()
        {
            _paths = new Dictionary<string, string>();
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal TV Server");
            string path = string.Empty;
            if (key != null)
            {
                path = (string)key.GetValue("InstallPath", null);
                _paths.Add("%TvServerBase%", path);
                _paths.Add("%TvServerPlugins%", path + "\\Plugins");
                key.Close();
            }
            else
            {
                _paths.Add("%TvServerBase%", "");
                _paths.Add("%TvServerPlugins%", "");
            }
        }

        public string Name
        {
            get { return "TvServer paths"; }
        }

        public Dictionary<string, string> Paths
        {
            get { return _paths; }
        }

        public string Colapse(string fileName)
        {
            foreach (KeyValuePair<string, string> path in Paths)
            {
                if (!string.IsNullOrEmpty(path.Key) && !string.IsNullOrEmpty(path.Value))
                {
                    fileName = fileName.Replace(path.Value, path.Key);
                }
            }
            return fileName;
        }

        public string Expand(string filenameTemplate)
        {
            foreach (KeyValuePair<string, string> path in Paths)
            {
                if (!string.IsNullOrEmpty(path.Key) && !string.IsNullOrEmpty(path.Value))
                {
                    filenameTemplate = filenameTemplate.Replace(path.Key, path.Value);
                }
            }
            return filenameTemplate;
        }
    }
}
