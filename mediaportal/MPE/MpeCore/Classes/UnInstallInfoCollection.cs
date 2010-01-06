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

using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using MediaPortal.Configuration;


namespace MpeCore.Classes
{
  public class UnInstallInfoCollection
  {
    public UnInstallInfoCollection()
    {
      Items = new List<UnInstallItem>();
    }

    public UnInstallInfoCollection(PackageClass pak)
    {
      Items = new List<UnInstallItem>();
      Version = pak.GeneralInfo.Version;
      ExtensionId = pak.GeneralInfo.Id;
    }


    public List<UnInstallItem> Items { get; set; }
    public string ExtensionId { get; set; }
    public VersionInfo Version { get; set; }

    /// <summary>
    /// Gets the location folder were stored the backup and the uninstall informations
    /// </summary>
    /// <value>The location folder.</value>
    public string LocationFolder
    {
      get { return string.Format("{0}\\V2\\{1}\\{2}\\", Config.GetFolder(Config.Dir.Installer), ExtensionId, Version); }
    }


    public void Init() {}

    public UnInstallItem BackUpFile(FileItem item)
    {
      return BackUpFile(item.ExpandedDestinationFilename, item.InstallType);
    }

    /// <summary>
    /// Backs up file.
    /// </summary>
    /// <param name="dest">The dest.</param>
    /// <param name="installType">Type of the install.</param>
    /// <returns></returns>
    public UnInstallItem BackUpFile(string dest, string installType)
    {
      UnInstallItem unInstallItem = new UnInstallItem();
      if (File.Exists(dest) && !BackUpExist(dest))
      {
        if (MpeInstaller.TransformInTemplatePath(dest).StartsWith("%"))
        {
          unInstallItem.BackUpFile = string.Format("{0}BackUp\\{1}", LocationFolder,
                                                   MpeInstaller.TransformInTemplatePath(dest));
        }
        else
        {
          unInstallItem.BackUpFile = string.Format("{0}BackUp\\Unknow\\{1}", LocationFolder,
                                                   Path.GetFileName(dest));
        }

        string s = Path.GetDirectoryName(unInstallItem.BackUpFile);
        if (!Directory.Exists(s))
          Directory.CreateDirectory(s);
        File.Copy(dest, unInstallItem.BackUpFile, true);
      }
      unInstallItem.OriginalFile = dest;
      unInstallItem.InstallType = installType;
      return unInstallItem;
    }

    /// <summary>
    /// Test if a file already have a backup copy
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public bool BackUpExist(string fileName)
    {
      foreach (UnInstallItem item in Items)
      {
        if (item.OriginalFile.CompareTo(fileName) == 0 && File.Exists(item.BackUpFile))
          return true;
      }
      return false;
    }

    public void Save()
    {
      Save(LocationFolder + "UninstallInfo.xml");
    }

    public void Save(string fileName)
    {
      if (!Directory.Exists(Path.GetDirectoryName(fileName)))
        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      var serializer = new XmlSerializer(typeof (UnInstallInfoCollection));
      TextWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, this);
      writer.Close();
    }

    public UnInstallInfoCollection Load()
    {
      return Load(LocationFolder + "UninstallInfo.xml");
    }

    public UnInstallInfoCollection Load(string fileName)
    {
      if (File.Exists(fileName))
      {
        try
        {
          XmlSerializer serializer = new XmlSerializer(typeof (UnInstallInfoCollection));
          FileStream fs = new FileStream(fileName, FileMode.Open);
          UnInstallInfoCollection unInstallInfoCollection = (UnInstallInfoCollection)serializer.Deserialize(fs);
          fs.Close();
          return unInstallInfoCollection;
        }
        catch
        {
          return null;
        }
      }
      return null;
    }
  }
}