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
using System.Text;
using System.Xml.Serialization;
using System.IO;
using MpeCore.Classes;

namespace MpeCore.Classes
{
  /// <summary>
  /// Represent a single file item wich is included in package
  /// </summary>
  public class FileItem
  {
    public FileItem()
    {
      Init();
    }

    public FileItem(string fileName, bool systemFile)
    {
      Init();
      LocalFileName = fileName;
      SystemFile = systemFile;
    }

    public FileItem(FileItem fileItem)
    {
      LocalFileName = fileItem.LocalFileName;
      ZipFileName = fileItem.ZipFileName;
      InstallType = fileItem.InstallType;
      DestinationFilename = fileItem.DestinationFilename;
      SystemFile = fileItem.SystemFile;
      UpdateOption = fileItem.UpdateOption;
      Param1 = fileItem.Param1;
      Modified = fileItem.Modified;
      //Param2 = fileItem.Param2;
    }

    /// <summary>
    /// Reinitialize the class
    /// </summary>
    public void Init()
    {
      LocalFileName = string.Empty;
      ZipFileName = string.Empty;
      InstallType = "CopyFile";
      SystemFile = false;
      DestinationFilename = string.Empty;
      UpdateOption = UpdateOptionEnum.OverwriteIfOlder;
      Param1 = string.Empty;
      Modified = true;
      //Param2 = string.Empty;
    }

    public string Param1 { get; set; }

    //public string Param2 { get; set; }

    /// <summary>
    /// Gets or sets the update option.
    /// </summary>
    /// <value>The update option.</value>
    public UpdateOptionEnum UpdateOption { get; set; }

    /// <summary>
    /// Gets or sets the name of the local file.
    /// This value used only when creating the package
    /// </summary>
    /// <value>The name of the local file.</value>
    public string LocalFileName { get; set; }

    private string _zipFileName;

    /// <summary>
    /// Gets or sets the name and path of the file from ziped package.
    /// </summary>
    /// <value>The name of the zip file.</value>
    public string ZipFileName
    {
      get
      {
        if (string.IsNullOrEmpty(_zipFileName) && MpeInstaller.InstallerTypeProviders.ContainsKey(InstallType))
          _zipFileName = MpeInstaller.InstallerTypeProviders[InstallType].GetZipEntry(this);
        return _zipFileName;
      }
      set { _zipFileName = value; }
    }

    /// <summary>
    /// Gets or sets the destination path and filename were the file will be installed.This path should contain template 
    /// </summary>
    /// <value>The destination filename.</value>
    public string DestinationFilename { get; set; }


    /// <summary>
    /// Gets the expanded destination filename  based on know PathProviders
    /// </summary>
    /// <value>The expanded destination filename.</value>
    public string ExpandedDestinationFilename
    {
      get { return MpeInstaller.InstallerTypeProviders[InstallType].GetInstallPath(this); }
    }

    /// <summary>
    /// Gets or sets the type of the install. Type is  provided by Installer static class
    /// </summary>
    /// <value>The type of the install.</value>
    [XmlAttribute]
    public string InstallType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the item is used by the installer it self.
    /// </summary>
    /// <value><c>true</c> if [system file]; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    public bool SystemFile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="FileItem"/> is modified.
    /// </summary>
    /// <value><c>true</c> if modified; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    public bool Modified { get; set; }

    /// <summary>
    /// Gets or sets the temp file location. This property have value if the item is extracted in a temporally location
    /// </summary>
    /// <value>The temp file location.</value>
    [XmlIgnore]
    public string TempFileLocation { get; set; }

    #region Overrides

    public override string ToString()
    {
      if (string.IsNullOrEmpty(DestinationFilename))
        return Path.GetFileName(LocalFileName);
      return DestinationFilename;
    }

    #endregion Overrides
  }
}