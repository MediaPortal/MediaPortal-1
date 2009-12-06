using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MpeCore.Classes.Project
{
    public class ProjectSettings
    {
        public List<FolderGroup> FolderGroups { get; set; }
        public string ProjectFilename { get; set; }

        public ProjectSettings()
        {
            FolderGroups = new List<FolderGroup>();
        }

        public void Add(FolderGroup folderGroup)
        {
            foreach (FolderGroup list in FolderGroups)
            {
                if (list.Group == folderGroup.Group && list.Folder == folderGroup.Folder)
                {
                    list.DestinationFilename = folderGroup.DestinationFilename;
                    list.InstallType = folderGroup.InstallType;
                    list.UpdateOption = folderGroup.UpdateOption;
                    return;
                }
            }
            FolderGroups.Add(folderGroup);
        }

        public List<FolderGroup> GetFolderGroups(string group)
        {
            List<FolderGroup> list = new List<FolderGroup>();
            foreach (FolderGroup folderGroup in FolderGroups)
            {
                if (folderGroup.Group == group)
                    list.Add(folderGroup);
            }
            return list;
        }

        public void RemoveFolderGroup(string group)
        {
            List<FolderGroup> list = new List<FolderGroup>();
            foreach (FolderGroup folderGroup in FolderGroups)
            {
                if (folderGroup.Group == group)
                    list.Add(folderGroup);
            }
            foreach (FolderGroup folderGroup in list)
            {
                FolderGroups.Remove(folderGroup);
            }
        }


        public static void UpdateFiles(PackageClass packageClass, FolderGroup folderGroup)
        {
            if (string.IsNullOrEmpty(folderGroup.Folder))
                return;
            GroupItem _groupItem = packageClass.Groups[folderGroup.Group];
            DirectoryInfo di = new DirectoryInfo(folderGroup.Folder);
            FileInfo[] fileList;
            fileList = folderGroup.Recursive ? di.GetFiles("*.*", SearchOption.AllDirectories) : di.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            string dir = folderGroup.Folder;
            string templ = folderGroup.DestinationFilename;
            if (!dir.EndsWith("\\"))
            {
                dir += "\\";
            }
            if (!templ.EndsWith("\\"))
            {
                templ += "\\";
            }

            foreach (FileInfo f in fileList)
            {
                if (!f.DirectoryName.Contains(".svn"))
                {
                    FileItem fileItem = new FileItem(f.FullName, false);
                    fileItem.UpdateOption = folderGroup.UpdateOption;
                    fileItem.Param1 = folderGroup.Param1;
                    fileItem.InstallType = folderGroup.InstallType;
                    fileItem.Modified = false;
                    fileItem.DestinationFilename = f.FullName.Replace(dir, templ);
                    if (_groupItem.Files.Get(f.FullName, fileItem.DestinationFilename) != null)
                        continue;
                    _groupItem.Files.Add(fileItem);
                }
            }
            List<FileItem> misingFiles = new List<FileItem>();
            foreach (FileItem item in _groupItem.Files.Items)
            {
                if (!File.Exists(item.LocalFileName) && !item.Modified)
                    misingFiles.Add(item);
            }

            foreach (FileItem item in misingFiles)
            {
                _groupItem.Files.Items.Remove(item);
            }
        }
    }
}
