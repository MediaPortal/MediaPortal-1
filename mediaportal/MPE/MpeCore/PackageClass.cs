using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Xml.Serialization;

using MpeCore.Classes;
using MpeCore.Classes.Events;
using MpeCore.Classes.ZipProvider;

namespace MpeCore
{
    public class PackageClass
    {
        public delegate void FileInstalledEventHandler(object sender, InstallEventArgs e);

        // Declare the event.
        public event FileInstalledEventHandler FileInstalled;

        public PackageClass()
        {
            Groups = new GroupItemCollection();
            Sections = new SectionItemCollection();
            GeneralInfo = new GeneralInfoItem();
            UniqueFileList = new FileItemCollection();
            this.Version = "2.0";
            ZipProvider = new ZipProviderClass();
            UnInstallInfo = new UnInstallInfoCollection();
        }

        public string Version { get; set; }
        public GroupItemCollection Groups { get; set; }
        public SectionItemCollection Sections { get; set; }
        public GeneralInfoItem GeneralInfo { get; set; }
        public FileItemCollection UniqueFileList { get; set; }

        [XmlIgnore]
        public ZipProviderClass ZipProvider { get; set; }

        [XmlIgnore]
        public UnInstallInfoCollection UnInstallInfo { get; set; }


        public void Install()
        {
            UnInstallInfo = new UnInstallInfoCollection(this);
            foreach (GroupItem groupItem in Groups.Items)
            {
                if(groupItem.Checked)
                {
                    foreach (FileItem fileItem in groupItem.Files.Items)
                    {
                        MpeInstaller.InstallerTypeProviders[fileItem.InstallType].Install(this, fileItem);
                        if (FileInstalled != null)
                            FileInstalled(this, new InstallEventArgs());
                    }
                }
            }
            UnInstallInfo.Save();
        }

        /// <summary>
        /// Gets the installable file count, based on group settings
        /// </summary>
        /// <returns>Number of files to be copyed</returns>
        public int GetInstallableFileCount()
        {
            int i = 0;
            foreach (GroupItem groupItem in Groups.Items)
            {
                if (groupItem.Checked)
                {
                    i += groupItem.Files.Items.Count;
                }
            }
            return i;
        }

        public void Reset()
        {
            foreach (GroupItem item in Groups.Items)
            {
                item.Checked = item.DefaulChecked;
            }
        }

        public bool StartInstallWizard()
        {
            WizardNavigator navigator=new WizardNavigator(this);
            navigator.Navigate();
            return true;
        }

        private void GenerateUniqueFileList()
        {
            UniqueFileList.Items.Clear();
            foreach (GroupItem groupItem in Groups.Items)
            {
                foreach (FileItem fileItem in groupItem.Files.Items)
                {
                    if (!UniqueFileList.ExistLocalFileName(fileItem))
                        UniqueFileList.Add(new FileItem(fileItem));
                }
            }

            foreach (SectionItem sectionItem in Sections.Items)
            {
                foreach (SectionParam sectionParam in sectionItem.Params.Items)
                {
                    if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
                    {
                        if (!UniqueFileList.ExistLocalFileName(sectionParam.Value))
                            UniqueFileList.Add(new FileItem(sectionParam.Value, true));
                    }
                }
            }
        }

        public void Save(string fileName)
        {
            GenerateUniqueFileList();
            var serializer = new XmlSerializer(typeof(PackageClass));
            TextWriter writer = new StreamWriter(fileName);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public void Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PackageClass));
                    FileStream fs = new FileStream(fileName, FileMode.Open);
                    PackageClass packageClass = (PackageClass)serializer.Deserialize(fs);
                    fs.Close();
                    this.Groups = packageClass.Groups;
                    this.Sections = packageClass.Sections;
                    this.GeneralInfo = packageClass.GeneralInfo;
                    this.UniqueFileList = packageClass.UniqueFileList;
                    Reset();
                }
                catch
                {
                }
            }
        }
    }
}
