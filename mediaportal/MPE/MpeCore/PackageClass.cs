using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Xml.Serialization;

using MpeCore.Classes;

namespace MpeCore
{
    public class PackageClass
    {
        public string Version { get; set; }

        public PackageClass()
        {
            Groups = new GroupItemCollection();
            Sections = new SectionItemCollection();
            GeneralInfo = new GeneralInfoItem();
            UniqueFileList = new FileItemCollection();
            this.Version = "2.0";
        }

        public GroupItemCollection Groups { get; set; }
        public SectionItemCollection Sections { get; set; }
        public GeneralInfoItem GeneralInfo { get; set; }
        public FileItemCollection UniqueFileList { get; set; }


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
