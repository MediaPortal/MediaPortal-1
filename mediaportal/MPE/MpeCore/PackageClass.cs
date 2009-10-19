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

        // Executed when a file item is installed.
        public event FileInstalledEventHandler FileInstalled;

        public PackageClass()
        {
            Groups = new GroupItemCollection();
            Sections = new SectionItemCollection();
            GeneralInfo = new GeneralInfoItem();
            UniqueFileList = new FileItemCollection();
            Version = "2.0";
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
                            FileInstalled(this, new InstallEventArgs(groupItem, fileItem));
                    }
                }
            }
            UnInstallInfo.Save();
            MpeInstaller.InstalledExtensions.Add(this);
            MpeInstaller.KnownExtensions.Add(this);
            MpeInstaller.Save();
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

        public void GenerateRelativePath(string path)
        {
            foreach (GroupItem groupItem in Groups.Items)
            {
                foreach (FileItem fileItem in groupItem.Files.Items)
                {
                    fileItem.LocalFileName = PathUtil.RelativePathTo(path, fileItem.LocalFileName);
                }
            }

            foreach (SectionItem sectionItem in Sections.Items)
            {
                foreach (SectionParam sectionParam in sectionItem.Params.Items)
                {
                    if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
                    {
                        sectionParam.Value = PathUtil.RelativePathTo(path, sectionParam.Value);
                    }
                }
            }
        }

        public void GenerateAbsolutePath(string path)
        {
            foreach (GroupItem groupItem in Groups.Items)
            {
                foreach (FileItem fileItem in groupItem.Files.Items)
                {
                    if (!Path.IsPathRooted(fileItem.LocalFileName))
                        fileItem.LocalFileName = Path.Combine(path, fileItem.LocalFileName);
                }
            }

            foreach (SectionItem sectionItem in Sections.Items)
            {
                foreach (SectionParam sectionParam in sectionItem.Params.Items)
                {
                    if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
                    {
                        sectionParam.Value = PathUtil.RelativePathTo(path, sectionParam.Value);
                        if (!Path.IsPathRooted(sectionParam.Value))
                            sectionParam.Value = Path.Combine(path, sectionParam.Value);

                    }
                }
            }
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

        public string ReplaceInfo(string str)
        {
            str = str.Replace("[Name]", GeneralInfo.Name);
            str = str.Replace("[Version]", GeneralInfo.Version.ToString());
            str = str.Replace("[DevelopmentStatus]", GeneralInfo.DevelopmentStatus);
            str = str.Replace("[Author]", GeneralInfo.Author);
            str = str.Replace("[Description]", GeneralInfo.ExtensionDescription);
            str = str.Replace("[VersionDescription]", GeneralInfo.VersionDescription);
            return str;
        }
    }
}
