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
using System.Windows.Forms;
using System.Xml.Serialization;
using MpeCore.Classes;
using MpeCore.Classes.Events;
using MpeCore.Classes.Project;
using MpeCore.Classes.SectionPanel;
using MpeCore.Classes.ZipProvider;

namespace MpeCore
{
  public class PackageClass
  {
    // Executed when a file item is installed.
    public event FileInstalledEventHandler FileInstalled;


    /// <summary>
    /// Occurs when a file item is Uninstalled.
    /// </summary>
    public event FileUnInstalledEventHandler FileUnInstalled;

    public PackageClass()
    {
      Groups = new GroupItemCollection();
      Sections = new SectionItemCollection();
      GeneralInfo = new GeneralInfoItem();
      UniqueFileList = new FileItemCollection();
      Version = "2.0";
      ZipProvider = new ZipProviderClass();
      UnInstallInfo = new UnInstallInfoCollection();
      Dependencies = new DependencyItemCollection();
      ProjectSettings = new ProjectSettings();
      Silent = false;
      IsHiden = false;
    }

    public string Version { get; set; }
    public GroupItemCollection Groups { get; set; }
    public SectionItemCollection Sections { get; set; }
    public DependencyItemCollection Dependencies { get; set; }
    public GeneralInfoItem GeneralInfo { get; set; }
    public FileItemCollection UniqueFileList { get; set; }
    public ProjectSettings ProjectSettings { get; set; }

    [XmlIgnore]
    public ZipProviderClass ZipProvider { get; set; }

    [XmlIgnore]
    public UnInstallInfoCollection UnInstallInfo { get; set; }

    [XmlIgnore]
    public bool Silent { get; set; }

    [XmlIgnore]
    public bool IsHiden { get; set; }

    /// <summary>
    /// Gets the location folder were stored the backup and the uninstall informations. Ended with \
    /// </summary>
    /// <value>The location folder.</value>
    public string LocationFolder
    {
      get
      {
        return string.Format("{0}\\V2\\{1}\\{2}\\", MpeInstaller.TransformInRealPath("%Installer%"),
                             GeneralInfo.Id, GeneralInfo.Version);
      }
    }


    /// <summary>
    /// Copies the defaul group check from  another package
    /// </summary>
    /// <param name="packageClass">The package class.</param>
    public void CopyGroupCheck(PackageClass packageClass)
    {
      foreach (GroupItem groupItem in Groups.Items)
      {
        GroupItem item = packageClass.Groups[groupItem.Name];
        if (item == null)
          continue;
        groupItem.Checked = item.DefaulChecked;
      }
    }

    /// <summary>
    /// Start copy the package file based on group settings
    /// 
    /// </summary>
    public void Install()
    {
      UnInstallInfo = new UnInstallInfoCollection(this);
      foreach (GroupItem groupItem in Groups.Items)
      {
        if (groupItem.Checked)
        {
          foreach (FileItem fileItem in groupItem.Files.Items)
          {
            MpeInstaller.InstallerTypeProviders[fileItem.InstallType].Install(this, fileItem);
            if (FileInstalled != null)
              FileInstalled(this, new InstallEventArgs(groupItem, fileItem));
          }
        }
      }
    }

    /// <summary>
    /// Do the unistall procces. The unistall file info should be alredy loaded.
    /// </summary>
    public void UnInstall()
    {
      for (int i = UnInstallInfo.Items.Count - 1; i > 0; i--)
      {
        UnInstallItem item = UnInstallInfo.Items[i];
        if (string.IsNullOrEmpty(item.ActionType))
        {
          MpeInstaller.InstallerTypeProviders[item.InstallType].Uninstall(this, item);
          if (FileUnInstalled != null)
            FileUnInstalled(this,
                            new UnInstallEventArgs("Removing file " + Path.GetFileName(item.OriginalFile),
                                                   item));
        }
        else
        {
          MpeInstaller.ActionProviders[item.ActionType].UnInstall(this, item);
          if (FileUnInstalled != null)
            FileUnInstalled(this,
                            new UnInstallEventArgs("Removing action " + item.ActionType,
                                                   item));
        }
      }
      UnInstallInfo.Items.Clear();
      DoAdditionalUnInstallTasks();
    }

    /// <summary>
    /// Checks the dependency.
    /// </summary>
    /// <param name="silent">if set to <c>true</c> [silent].</param>
    /// <returns>Return if all dependency met</returns>
    public bool CheckDependency(bool silent)
    {
      foreach (DependencyItem item in Dependencies.Items)
      {
        if (!MpeInstaller.VersionProviders[item.Type].Validate(item))
        {
          if (item.WarnOnly)
          {
            if (!silent)
              MessageBox.Show(item.Message, "Dependency warning", MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
          }
          else
          {
            if (!silent)
              MessageBox.Show(item.Message, "Dependency error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Validates the package.
    /// </summary>
    /// <returns>The package is valid if a zero length list returned, else return list of errors</returns>
    public List<string> ValidatePackage()
    {
      var respList = new List<string>();
      foreach (SectionParam item in GeneralInfo.Params.Items)
      {
        if (item.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(item.Value) && !File.Exists(item.Value))
        {
          respList.Add(string.Format("Params ->{0} file not found {1}", item.Name, item.Value));
        }
      }

      foreach (GroupItem groupItem in Groups.Items)
      {
        foreach (FileItem fileItem in groupItem.Files.Items)
        {
          ValidationResponse resp =
            MpeInstaller.InstallerTypeProviders[fileItem.InstallType].Validate(fileItem);
          if (!resp.Valid)
            respList.Add(string.Format("[{0}][{1}] - {2}", groupItem.Name, fileItem, resp.Message));
        }
      }

      foreach (SectionItem sectionItem in Sections.Items)
      {
        if (!string.IsNullOrEmpty(sectionItem.ConditionGroup) && Groups[sectionItem.ConditionGroup] == null)
          respList.Add(string.Format("[{0}] condition group not found [{1}]", sectionItem.Name,
                                     sectionItem.ConditionGroup));
        foreach (ActionItem actionItem in sectionItem.Actions.Items)
        {
          ValidationResponse resp = MpeInstaller.ActionProviders[actionItem.ActionType].Validate(this,
                                                                                                 actionItem);
          if (!resp.Valid)
            respList.Add(string.Format("[{0}][{1}] - {2}", sectionItem.Name, actionItem.Name, resp.Message));
        }
      }

      return respList;
    }


    private void DoAdditionalInstallTasks()
    {
      if (!Directory.Exists(LocationFolder))
        Directory.CreateDirectory(LocationFolder);
      UnInstallInfo.Save();
      //copy icon file
      if (!string.IsNullOrEmpty(GeneralInfo.Params[ParamNamesConst.ICON].Value) &&
          File.Exists(GeneralInfo.Params[ParamNamesConst.ICON].Value))
        File.Copy(GeneralInfo.Params[ParamNamesConst.ICON].Value,
                  LocationFolder + "icon" + Path.GetExtension(GeneralInfo.Params[ParamNamesConst.ICON].Value),
                  true);
      //copy the package file 
      string newlocation = LocationFolder + GeneralInfo.Id + ".mpe2";
      if (newlocation.CompareTo(GeneralInfo.Location) != 0)
      {
        File.Copy(GeneralInfo.Location, newlocation, true);
        GeneralInfo.Location = newlocation;
      }
      MpeInstaller.InstalledExtensions.Add(this);
      MpeInstaller.KnownExtensions.Add(this);
      MpeInstaller.Save();
    }

    private void DoAdditionalUnInstallTasks()
    {
      if (!Directory.Exists(LocationFolder))
        Directory.CreateDirectory(LocationFolder);
      UnInstallInfo.Save();
      MpeInstaller.InstalledExtensions.Remove(this);
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

    /// <summary>
    /// Starts the install wizard.
    /// </summary>
    /// <returns></returns>
    public bool StartInstallWizard()
    {
      var navigator = new WizardNavigator(this);
      if (navigator.Navigate() == SectionResponseEnum.Ok)
        DoAdditionalInstallTasks();
      return true;
    }

    public void GenerateRelativePath(string path)
    {
      foreach (GroupItem groupItem in Groups.Items)
      {
        foreach (FileItem fileItem in groupItem.Files.Items)
        {
          fileItem.LocalFileName = Util.RelativePathTo(path, fileItem.LocalFileName);
        }
      }

      foreach (SectionItem sectionItem in Sections.Items)
      {
        foreach (SectionParam sectionParam in sectionItem.Params.Items)
        {
          if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
          {
            sectionParam.Value = Util.RelativePathTo(path, sectionParam.Value);
          }
        }
        foreach (ActionItem actionItem in sectionItem.Actions.Items)
        {
          foreach (SectionParam sectionParam in actionItem.Params.Items)
          {
            if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
            {
              sectionParam.Value = Util.RelativePathTo(path, sectionParam.Value);
            }
          }
        }
      }

      foreach (SectionParam sectionParam in GeneralInfo.Params.Items)
      {
        if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
        {
          sectionParam.Value = Util.RelativePathTo(path, sectionParam.Value);
        }
      }

      foreach (FolderGroup folderGroup in ProjectSettings.FolderGroups)
      {
        folderGroup.Folder = Util.RelativePathTo(path, folderGroup.Folder);
      }
    }

    public void GenerateAbsolutePath(string path)
    {
      foreach (GroupItem groupItem in Groups.Items)
      {
        foreach (FileItem fileItem in groupItem.Files.Items)
        {
          if (!Path.IsPathRooted(fileItem.LocalFileName))
            fileItem.LocalFileName = Path.GetFullPath(Path.Combine(path, fileItem.LocalFileName));
        }
      }

      foreach (SectionItem sectionItem in Sections.Items)
      {
        foreach (SectionParam sectionParam in sectionItem.Params.Items)
        {
          if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
          {
            //sectionParam.Value = PathUtil.RelativePathTo(path, sectionParam.Value);
            if (!Path.IsPathRooted(sectionParam.Value))
              sectionParam.Value = Path.GetFullPath(Path.Combine(path, sectionParam.Value));
          }
        }
        foreach (ActionItem actionItem in sectionItem.Actions.Items)
        {
          foreach (SectionParam sectionParam in actionItem.Params.Items)
          {
            if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
            {
              //sectionParam.Value = PathUtil.RelativePathTo(path, sectionParam.Value);
              if (!Path.IsPathRooted(sectionParam.Value))
                sectionParam.Value = Path.GetFullPath(Path.Combine(path, sectionParam.Value));
            }
          }
        }
      }

      foreach (SectionParam sectionParam in GeneralInfo.Params.Items)
      {
        if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
        {
          //sectionParam.Value = PathUtil.RelativePathTo(path, sectionParam.Value);
          if (!Path.IsPathRooted(sectionParam.Value))
            sectionParam.Value = Path.GetFullPath(Path.Combine(path, sectionParam.Value));
        }
      }

      foreach (FolderGroup folderGroup in ProjectSettings.FolderGroups)
      {
        if (!Path.IsPathRooted(folderGroup.Folder))
          folderGroup.Folder = Path.GetFullPath(Path.Combine(path, folderGroup.Folder));
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
            else
              UniqueFileList.GetByLocalFileName(sectionParam.Value).SystemFile = true;
          }
        }
        foreach (ActionItem actionItem in sectionItem.Actions.Items)
        {
          foreach (SectionParam sectionParam in actionItem.Params.Items)
          {
            if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
            {
              if (!UniqueFileList.ExistLocalFileName(sectionParam.Value))
                UniqueFileList.Add(new FileItem(sectionParam.Value, true));
              else
                UniqueFileList.GetByLocalFileName(sectionParam.Value).SystemFile = true;
            }
          }
        }
      }

      foreach (SectionParam sectionParam in GeneralInfo.Params.Items)
      {
        if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
        {
          if (!UniqueFileList.ExistLocalFileName(sectionParam.Value))
            UniqueFileList.Add(new FileItem(sectionParam.Value, true));
          else
            UniqueFileList.GetByLocalFileName(sectionParam.Value).SystemFile = true;
        }
      }
    }

    /// <summary>
    /// Gets the system file paths frome extracted unique file list.
    /// This should caled only if the package is loaded from a zip file
    /// </summary>
    public void GetFilePaths()
    {
      foreach (SectionItem sectionItem in Sections.Items)
      {
        foreach (SectionParam sectionParam in sectionItem.Params.Items)
        {
          if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
          {
            if (UniqueFileList.ExistLocalFileName(sectionParam.Value))
              sectionParam.Value = UniqueFileList.GetByLocalFileName(sectionParam.Value).TempFileLocation;
          }
        }
        foreach (ActionItem actionItem in sectionItem.Actions.Items)
        {
          foreach (SectionParam sectionParam in actionItem.Params.Items)
          {
            if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
            {
              if (UniqueFileList.ExistLocalFileName(sectionParam.Value))
                sectionParam.Value = UniqueFileList.GetByLocalFileName(sectionParam.Value).TempFileLocation;
            }
          }
        }
      }

      foreach (SectionParam sectionParam in GeneralInfo.Params.Items)
      {
        if (sectionParam.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(sectionParam.Value))
        {
          if (UniqueFileList.ExistLocalFileName(sectionParam.Value))
            sectionParam.Value = UniqueFileList.GetByLocalFileName(sectionParam.Value).TempFileLocation;
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

    /// <summary>
    /// Loads the specified file name.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>True if loding was successful else False  </returns>
    public bool Load(string fileName)
    {
      if (File.Exists(fileName))
      {
        try
        {
          var serializer = new XmlSerializer(typeof(PackageClass));
          var fs = new FileStream(fileName, FileMode.Open);
          var packageClass = (PackageClass)serializer.Deserialize(fs);
          fs.Close();
          this.Groups = packageClass.Groups;
          this.Sections = packageClass.Sections;
          this.GeneralInfo = packageClass.GeneralInfo;
          this.UniqueFileList = packageClass.UniqueFileList;
          this.Dependencies = packageClass.Dependencies;
          this.ProjectSettings = packageClass.ProjectSettings;
          var pak = new PackageClass();
          foreach (SectionParam item in pak.GeneralInfo.Params.Items)
          {
            if (!GeneralInfo.Params.Contain(item.Name))
              GeneralInfo.Params.Add(item);
          }
          Reset();
          return true;
        }
        catch
        {
          return false;
        }
      }
      return false;
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

    /// <summary>
    /// Writes the XML file for getting infos about updates.
    /// </summary>
    /// <param name="xmlFile">is the filename where to save the infos to.</param>
    public bool WriteUpdateXml(string xmlFile)
    {
      if (String.IsNullOrEmpty(xmlFile))
      {
        Console.WriteLine("[MpeMaker] Error: Output file for Update.xml is not specified in package.");
        return false;
      }

      ExtensionCollection list = ExtensionCollection.Load(xmlFile);
      list.Add(this);
      list.Save(xmlFile);

      return true;
    }

    /// <summary>
    /// Compares the specified package.
    /// </summary>
    /// <param name="pak1">The pak1.</param>
    /// <param name="pak2">The pak2.</param>
    /// <returns></returns>
    public static int Compare(PackageClass pak1, PackageClass pak2)
    {
      if (pak1.GeneralInfo.Name.ToUpper().CompareTo(pak2.GeneralInfo.Name.ToUpper()) == 0)
      {
        return pak1.GeneralInfo.Version.CompareTo(pak2.GeneralInfo.Version);
      }
      return pak1.GeneralInfo.Name.ToUpper().CompareTo(pak2.GeneralInfo.Name.ToUpper());
    }

  }
}