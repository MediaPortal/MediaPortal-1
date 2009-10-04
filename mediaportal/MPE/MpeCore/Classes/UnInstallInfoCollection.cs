using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

using MediaPortal.Configuration;


namespace MpeCore.Classes
{
    public class UnInstallInfoCollection
    {
        public UnInstallInfoCollection()
        {
            Items = new List<UnInstallItem>();
        }

        public UnInstallInfoCollection(PackageClass pak)
        {
            Items = new List<UnInstallItem>();
            Version = pak.GeneralInfo.Version;
            ExtensionId = pak.GeneralInfo.Id;
        }


        public List<UnInstallItem> Items { get; set; }
        public string ExtensionId { get; set; }
        public VersionInfo Version { get; set; }

        /// <summary>
        /// Gets the location folder were stored the backup and the uninstall informations
        /// </summary>
        /// <value>The location folder.</value>
        public string LocationFolder
        {
            get
            {
                return string.Format("{0}\\V2\\{1}\\{2}\\",Config.GetFolder(Config.Dir.Installer), ExtensionId, Version);
            }
        }
	

        public void Init()
        {
            
        }
        
        public UnInstallItem BackUpFile(FileItem item)
        {
            UnInstallItem unInstallItem = new UnInstallItem();
            if (File.Exists(item.ExpandedDestinationFilename)&& !BackUpExist(item.ExpandedDestinationFilename))
            {
                unInstallItem.BackUpFile = string.Format("{0}BackUp\\{1}", LocationFolder, MpeInstaller.TransformInTemplatePath(item.ExpandedDestinationFilename));
                string s = Path.GetDirectoryName(unInstallItem.BackUpFile);
                if (!Directory.Exists(s))
                    Directory.CreateDirectory(s);
                File.Copy(item.ExpandedDestinationFilename, unInstallItem.BackUpFile, true);
            }
            unInstallItem.OriginalFile = item.ExpandedDestinationFilename;
            unInstallItem.InstallType = item.InstallType;
            return unInstallItem;
        }

        /// <summary>
        /// Test if a file alredy have a backup copy
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public bool BackUpExist(string fileName)
        {
            foreach (UnInstallItem item in Items)
            {
                if (item.OriginalFile.CompareTo(fileName) == 0 && File.Exists(item.BackUpFile))
                    return true;
            }
            return false;
        }

        public void Save()
        {
            Save(LocationFolder + "UninstallInfo.xml");
        }

        public void Save(string fileName)
        {
            var serializer = new XmlSerializer(typeof(UnInstallInfoCollection));
            TextWriter writer = new StreamWriter(fileName);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public UnInstallInfoCollection Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(UnInstallInfoCollection));
                    FileStream fs = new FileStream(fileName, FileMode.Open);
                    UnInstallInfoCollection unInstallInfoCollection = (UnInstallInfoCollection)serializer.Deserialize(fs);
                    fs.Close();
                    return unInstallInfoCollection;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    }
}
