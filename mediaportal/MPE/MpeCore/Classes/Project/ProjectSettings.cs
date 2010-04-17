#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using System.IO;

namespace MpeCore.Classes.Project
{
  public class ProjectSettings
  {
    public List<FolderGroup> FolderGroups { get; set; }
    public string ProjectFilename { get; set; }
    public string UpdatePath1 { get; set; }
    public string UpdatePath2 { get; set; }
    public string UpdatePath3 { get; set; }

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
      folderGroup.Folder = Path.GetFullPath(folderGroup.Folder);
      GroupItem _groupItem = packageClass.Groups[folderGroup.Group];
      DirectoryInfo di = new DirectoryInfo(Path.GetFullPath(folderGroup.Folder));
      FileInfo[] fileList;
      fileList = folderGroup.Recursive
                   ? di.GetFiles("*.*", SearchOption.AllDirectories)
                   : di.GetFiles("*.*", SearchOption.TopDirectoryOnly);
      string dir = Path.GetFullPath(folderGroup.Folder);
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