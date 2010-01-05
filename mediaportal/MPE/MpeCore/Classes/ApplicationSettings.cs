using System;
using System.IO;
using System.Xml.Serialization;

namespace MpeCore.Classes
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
            string filename = string.Format("{0}\\InstallerSettings.xml", MpeInstaller.BaseFolder);
            var serializer = new XmlSerializer(typeof(ApplicationSettings));
            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public static ApplicationSettings Load()
        {
            var apls = new ApplicationSettings();
            string filename = string.Format("{0}\\InstallerSettings.xml", MpeInstaller.BaseFolder);

            if (File.Exists(filename))
            {
                FileStream fs = null;
                try
                {
                    var serializer = new XmlSerializer(typeof(ApplicationSettings));
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
