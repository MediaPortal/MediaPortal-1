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

using System;
using System.Collections.Generic;
using System.IO;
using MpeCore.Interfaces;
using MpeCore.Classes;

namespace MpeCore.Classes.InstallerType
{
  internal class GenericSkinFile : IInstallerTypeProvider
  {
    public string Name
    {
      get { return "GenericSkinFile"; }
    }

    public string Description
    {
      get
      {
        return
          "Copy a skin file to specified location in all installed skins.\n The install location begin with %Skin%\\[DEFAULT]\n Param1 contain the skiped skins separated with , ";
      }
    }

    public void Install(PackageClass packageClass, FileItem file)
    {
      List<string> skinList = GetInstalledSkins(file.Param1.Split(','));
      foreach (string list in skinList)
      {
        FileItem fileItem = new FileItem(file);
        string destination = fileItem.ExpandedDestinationFilename.Replace("[DEFAULT]", list);

        FileItem item = packageClass.UniqueFileList.GetByLocalFileName(fileItem);
        if (item == null)
          return;

        item.InstallType = "CopyFile";

        if (File.Exists(destination))
        {
          switch (fileItem.UpdateOption)
          {
            case UpdateOptionEnum.NeverOverwrite:
              continue;
            case UpdateOptionEnum.AlwaysOverwrite:
              break;
            case UpdateOptionEnum.OverwriteIfOlder:
              if (File.GetLastWriteTime(destination) > packageClass.ZipProvider.FileDate(item))
                continue;
              break;
          }
        }
        if (!Directory.Exists(Path.GetDirectoryName(destination)))
          Directory.CreateDirectory(Path.GetDirectoryName(destination));
        UnInstallItem unInstallItem = packageClass.UnInstallInfo.BackUpFile(destination, item.InstallType);
        packageClass.ZipProvider.Extract(item, destination);
        FileInfo info = new FileInfo(destination);
        unInstallItem.FileDate = info.CreationTimeUtc;
        unInstallItem.FileSize = info.Length;
        packageClass.UnInstallInfo.Items.Add(unInstallItem);
      }
    }

    public void Uninstall(PackageClass packageClass, UnInstallItem fileItem)
    {
      throw new NotImplementedException();
    }

    public string GetZipEntry(FileItem fileItem)
    {
      return string.Format("Installer{{GenericSkinFile}}\\{{{0}}}-{1}", Guid.NewGuid(),
                           Path.GetFileName(fileItem.LocalFileName));
    }

    /// <summary>
    /// Transform real path in a templated path based on PathProviders
    /// </summary>
    /// <param name="fileItem">The file item.</param>
    /// <returns></returns>
    public string GetTemplatePath(FileItem fileItem)
    {
      string localFile = fileItem.LocalFileName;
      //foreach (var pathProvider in MpeCore.MpeInstaller.PathProviders)
      //{
      //    localFile = pathProvider.Value.Colapse(localFile);
      //}
      //if (!localFile.Contains("%"))
      //    localFile = "%Base%\\" + Path.GetFileName(localFile);
      return "%Skin%\\[DEFAULT]\\" + Path.GetFileName(localFile);
      ;
    }

    /// <summary>
    /// Transform templated path in a real path based on PathProviders
    /// </summary>
    /// <param name="fileItem">The file item.</param>
    /// <returns></returns>
    public string GetInstallPath(FileItem fileItem)
    {
      string localFile = fileItem.DestinationFilename;
      foreach (var pathProvider in MpeCore.MpeInstaller.PathProviders)
      {
        localFile = pathProvider.Value.Expand(localFile);
      }
      //if (!localFile.Contains("%"))
      //    localFile = string.Empty;
      return localFile;
    }

    public ValidationResponse Validate(FileItem fileItem)
    {
      var response = new ValidationResponse();
      if (!File.Exists(fileItem.LocalFileName))
      {
        response.Valid = false;
        response.Message = "Source file not found !";
      }
      if (string.IsNullOrEmpty(fileItem.DestinationFilename))
      {
        response.Valid = false;
        response.Message = "No install location specified !";
      }
      if (!fileItem.DestinationFilename.StartsWith("%Skin%\\[DEFAULT]"))
      {
        response.Valid = false;
        response.Message = "Template  not start with %Skin%\\[DEFAULT] in destination path specified !" + fileItem.DestinationFilename;
      }
      return response;
    }

    private static List<string> GetInstalledSkins(IEnumerable<string> skipedSkins)
    {
      var installedSkinList = new List<string>();

      string skinDirectory = MpeInstaller.TransformInRealPath("%Skin%");
      if (!Directory.Exists(skinDirectory)) return installedSkinList;

      string[] skinFolders = Directory.GetDirectories(skinDirectory, "*.*");
      foreach (string skinFolder in skinFolders)
      {
        bool isInvalidDirectory = false;

        string directoryName = skinFolder.Substring(skinDirectory.Length + 1);
        if (string.IsNullOrEmpty(directoryName)) continue;

        foreach (string invalidDirectory in skipedSkins)
        {
          if (invalidDirectory.Trim().Equals(
            directoryName.Trim(),
            StringComparison.OrdinalIgnoreCase))
          {
            isInvalidDirectory = true;
            break;
          }
        }
        if (isInvalidDirectory) continue;

        string filename = Path.Combine(skinDirectory, Path.Combine(directoryName, "references.xml"));
        if (!File.Exists(filename)) continue;

        installedSkinList.Add(directoryName);
      }

      return installedSkinList;
    }
  }
}