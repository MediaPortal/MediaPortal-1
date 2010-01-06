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
using System.IO;
using MpeCore.Interfaces;

namespace MpeCore.Classes.InstallerType
{
  internal class CopyFile : IInstallerTypeProvider
  {
    public string Name
    {
      get { return "CopyFile"; }
    }

    public virtual string Description
    {
      get { return "Copy the file to specified location"; }
    }

    public void Install(PackageClass packageClass, FileItem fileItem)
    {
      string destination = fileItem.ExpandedDestinationFilename;

      FileItem item = packageClass.UniqueFileList.GetByLocalFileName(fileItem);
      if (item == null)
        return;

      if (File.Exists(destination))
      {
        switch (fileItem.UpdateOption)
        {
          case UpdateOptionEnum.NeverOverwrite:
            return;
          case UpdateOptionEnum.AlwaysOverwrite:
            break;
          case UpdateOptionEnum.OverwriteIfOlder:
            if (File.GetLastWriteTime(destination) > packageClass.ZipProvider.FileDate(item))
              return;
            break;
        }
      }
      if (!Directory.Exists(Path.GetDirectoryName(destination)))
      {
        string dirname = Path.GetDirectoryName(destination);
        Directory.CreateDirectory(dirname);
        if (!dirname.EndsWith("\\"))
          dirname += "\\";
        UnInstallItem unI = new UnInstallItem();
        unI.OriginalFile = dirname;
        unI.InstallType = "CopyFile";
        packageClass.UnInstallInfo.Items.Add(unI);
      }
      UnInstallItem unInstallItem = packageClass.UnInstallInfo.BackUpFile(item);
      packageClass.ZipProvider.Extract(item, destination);
      FileInfo info = new FileInfo(destination);
      unInstallItem.FileDate = info.CreationTimeUtc;
      unInstallItem.FileSize = info.Length;
      packageClass.UnInstallInfo.Items.Add(unInstallItem);
    }


    public void Uninstall(PackageClass packageClass, UnInstallItem fileItem)
    {
      if (fileItem.OriginalFile.EndsWith("\\"))
      {
        try
        {
          DirectoryInfo di = new DirectoryInfo(fileItem.OriginalFile);
          FileInfo[] fileList = di.GetFiles("*.*", SearchOption.AllDirectories);
          if (fileList.Length == 0)
          {
            Directory.Delete(fileItem.OriginalFile, true);
          }
        }
        catch (Exception) {}
      }
      else
      {
        if (!File.Exists(fileItem.OriginalFile))
          return;
        FileInfo fi = new FileInfo(fileItem.OriginalFile);
        if (fileItem.FileDate != fi.CreationTimeUtc || fileItem.FileSize != fi.Length)
          return;
        try
        {
          File.Delete(fileItem.OriginalFile);
          if (File.Exists(fileItem.BackUpFile))
            File.Move(fileItem.BackUpFile, fileItem.OriginalFile);
        }
        catch (Exception) {}
      }
    }

    public string GetZipEntry(FileItem fileItem)
    {
      return string.Format("Installer{{CopyFile}}\\{{{0}}}-{1}", Guid.NewGuid(),
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
      foreach (var pathProvider in MpeInstaller.PathProviders)
      {
        localFile = pathProvider.Value.Colapse(localFile);
      }
      if (!localFile.Contains("%"))
        localFile = "%Base%\\" + Path.GetFileName(localFile);
      return localFile;
    }

    /// <summary>
    /// Transform templated path in a real path based on PathProviders
    /// </summary>
    /// <param name="fileItem">The file item.</param>
    /// <returns></returns>
    public string GetInstallPath(FileItem fileItem)
    {
      string localFile = fileItem.DestinationFilename;
      foreach (var pathProvider in MpeInstaller.PathProviders)
      {
        localFile = pathProvider.Value.Expand(localFile);
      }
      //if (!localFile.Contains("%"))
      //    localFile = string.Empty;
      return localFile;
    }

    public ValidationResponse Validate(FileItem fileItem)
    {
      ValidationResponse response = new ValidationResponse();
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
      if (!fileItem.DestinationFilename.Contains("%"))
      {
        response.Valid = false;
        response.Message = "No template in destination path specified !";
      }
      return response;
    }
  }
}