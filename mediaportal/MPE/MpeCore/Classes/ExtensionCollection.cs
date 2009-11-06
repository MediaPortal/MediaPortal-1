using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MpeCore.Classes
{
    public class ExtensionCollection
    {
        public ExtensionCollection()
        {
            Items = new List<PackageClass>();
        }

        public List<PackageClass> Items { get; set; }

        /// <summary>
        /// Gets a list with unique update urls
        /// </summary>
        /// <returns></returns>
        public List<string> GetUpdateUrls()
        {
            List<string> urls = new List<string>();
            foreach (PackageClass item in Items)
            {
                if (string.IsNullOrEmpty(item.GeneralInfo.UpdateUrl))
                    continue;
                if (!urls.Contains(item.GeneralInfo.UpdateUrl))
                    urls.Add(item.GeneralInfo.UpdateUrl);
            }
            return urls;
        }

        
        public void Add(ExtensionCollection collection)
        {
            foreach (PackageClass item in collection.Items)
            {
                Add(item);
            }
        }


        /// <summary>
        /// Adds the specified package class. But without copy all the sructure the GeneralInfo and the group Names
        /// If the package with same version number already exist, will be replaced
        /// </summary>
        /// <param name="packageClass">The package class.</param>
        public void Add(PackageClass packageClass)
        {
            PackageClass oldpak = Get(packageClass);
            if (oldpak != null)
                Items.Remove(oldpak);
            PackageClass pak = new PackageClass();
            pak.GeneralInfo = packageClass.GeneralInfo;
            foreach (GroupItem groupItem in packageClass.Groups.Items)
            {
                pak.Groups.Items.Add(new GroupItem(groupItem.Name, groupItem.Checked));
            }
            Items.Add(pak);
        }

        /// <summary>
        /// Removes the specified package class fromr the list.
        /// </summary>
        /// <param name="packageClass">The package class.</param>
        public void Remove(PackageClass packageClass)
        {
            PackageClass pak = Get(packageClass);
            if ( pak== null)
                return;
            Items.Remove(pak);
        }

        public PackageClass Get(PackageClass pak)
        {
            return Get(pak.GeneralInfo.Id, pak.GeneralInfo.Version.ToString());
        }

        /// <summary>
        /// Gets the specified id.
        /// </summary>
        /// <param name="id">The package GUID.</param>
        /// <param name="version">The version.</param>
        /// <returns>If found a package with specified Version and GUID return the package else NULL</returns>
        public PackageClass Get(string id, string version)
        {
            foreach (PackageClass item in Items)
            {
                if (item.GeneralInfo.Id == id && item.GeneralInfo.Version.CompareTo(version) == 0)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Gets the hightes version number package with specified id.
        /// </summary>
        /// <param name="id">The package GUID.</param>
        /// <returns>If found a package with specified GUID return the package else NULL</returns>
        public PackageClass Get(string id)
        {
            PackageClass ret = null;
            foreach (PackageClass item in Items)
            {
                if (item.GeneralInfo.Id == id )
                {
                    if (ret == null)
                    {
                        ret = item;
                    }
                    else
                    {
                        if (item.GeneralInfo.Version.CompareTo(ret.GeneralInfo.Version) > 0)
                            ret = item;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Gets the latest version of a package. If not found or no new version return Null
        /// </summary>
        /// <param name="pak">The package</param>
        /// <returns></returns>
        public PackageClass GetUpdate(PackageClass pak)
        {
            PackageClass ret = Get(pak.GeneralInfo.Id);
            if (ret == null)
                return null;
            if (ret.GeneralInfo.Version.CompareTo(pak.GeneralInfo.Version) > 0)
                return ret;
            return null;
        }


        public void Save(string fileName)
        {
            var serializer = new XmlSerializer(typeof(ExtensionCollection));
            TextWriter writer = new StreamWriter(fileName);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        /// <summary>
        /// Loads the specified file name.
        /// if some error ocure, empty list will be returned
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        static public ExtensionCollection Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                FileStream fs = null;
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ExtensionCollection));
                    fs = new FileStream(fileName, FileMode.Open);
                    ExtensionCollection extensionCollection = (ExtensionCollection)serializer.Deserialize(fs);
                    fs.Close();
                    return extensionCollection;
                }
                catch
                {
                    if (fs != null)
                        fs.Dispose();
                    return new ExtensionCollection();
                }
            }
            return new ExtensionCollection();
        }
    }
}
