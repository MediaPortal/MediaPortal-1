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
        /// Adds the specified package class. But without copy all the sructure the GeneralInfo and the group Names
        /// </summary>
        /// <param name="packageClass">The package class.</param>
        public void Add(PackageClass packageClass)
        {
            if(Get(packageClass)!=null)
                return;

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
        /// Gets the specified id.
        /// </summary>
        /// <param name="id">The package GUID.</param>
        /// <returns>If found a package with specified GUID return the package else NULL</returns>
        public PackageClass Get(string id)
        {
            foreach (PackageClass item in Items)
            {
                if (item.GeneralInfo.Id == id )
                    return item;
            }
            return null;
        }


        public void Save(string fileName)
        {
            var serializer = new XmlSerializer(typeof(ExtensionCollection));
            TextWriter writer = new StreamWriter(fileName);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        static public ExtensionCollection Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ExtensionCollection));
                    FileStream fs = new FileStream(fileName, FileMode.Open);
                    ExtensionCollection extensionCollection = (ExtensionCollection)serializer.Deserialize(fs);
                    fs.Close();
                    return extensionCollection;
                }
                catch
                {
                    return new ExtensionCollection();
                }
            }
            return new ExtensionCollection();
        }
    }
}
