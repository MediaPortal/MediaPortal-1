using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using MediaPortal.Configuration;

namespace MpeInstaller.Classes
{
    public class ApplicationSettings
    {

        public ApplicationSettings()
        {
            LastUpdate = DateTime.MinValue;
            UpdateDays = 0;
            UpdateAll = false;
            DoUpdateInStartUp = true;
        }

        public DateTime LastUpdate { get; set; }
        public int UpdateDays { get; set; }
        public bool UpdateAll { get; set; }
        public bool DoUpdateInStartUp { get; set; }

        public void Save()
        {
            string filename = string.Format("{0}\\V2\\InstallerSettings.xml", Config.GetFolder(Config.Dir.Installer));
            var serializer = new XmlSerializer(typeof(ApplicationSettings));
            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public static ApplicationSettings Load()
        {
            ApplicationSettings apls = new ApplicationSettings();
            string filename = string.Format("{0}\\V2\\InstallerSettings.xml", Config.GetFolder(Config.Dir.Installer));

            if (File.Exists(filename))
            {
                FileStream fs = null;
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
                    fs = new FileStream(filename, FileMode.Open);
                    apls = (ApplicationSettings)serializer.Deserialize(fs);
                    fs.Close();
                    return apls;
                }
                catch
                {
                    if (fs != null)
                        fs.Dispose();
                    return new ApplicationSettings();
                }
            }
            return apls;
        }
    }
}
