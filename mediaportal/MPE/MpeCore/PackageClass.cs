#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Xml;
using MpeCore.Classes;
using MpeCore.Classes.Events;
using MpeCore.Classes.Project;
using MpeCore.Classes.SectionPanel;
using MpeCore.Classes.ZipProvider;
using System.Text;

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
      PluginDependencies = new PluginDependencyItemCollection();
      ProjectSettings = new ProjectSettings();
      Silent = false;
      IsHiden = false;
      Parent = null;
    }

    public string Version { get; set; }
    public GroupItemCollection Groups { get; set; }
    public SectionItemCollection Sections { get; set; }
    public DependencyItemCollection Dependencies { get; set; }
    public PluginDependencyItemCollection PluginDependencies { get; set; }
    public GeneralInfoItem GeneralInfo { get; set; }
    public FileItemCollection UniqueFileList { get; set; }
    public ProjectSettings ProjectSettings { get; set; }
    public bool IsSkin { get; set; }

    [XmlIgnore]
    public ExtensionCollection Parent { get; set; }

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
        return string.Format("{0}\\V2\\{1}\\{2}\\", Util.InstallerConfigDir,
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
      if (UnInstallInfo == null)
      {
        UnInstallInfo = new UnInstallInfoCollection(this);
      }
      else
        UnInstallInfo.SetInfo(this);
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
      for (int i = UnInstallInfo.Items.Count - 1; i >= 0; i--)
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
      bool hasMPDependency = false;
      MpeCore.Classes.VersionProvider.MediaPortalVersion MPDependency = new Classes.VersionProvider.MediaPortalVersion();
      MpeCore.Classes.VersionProvider.SkinVersion skinDependency = new Classes.VersionProvider.SkinVersion();
      bool hasSkinDependency = false;
      foreach (DependencyItem item in Dependencies.Items)
      {
        if (!hasMPDependency && item.Type == MPDependency.DisplayName)
        {
          hasMPDependency = true;
        }
        if (item.Type == skinDependency.DisplayName)
        {
          hasSkinDependency = true;
          if (!skinDependency.Validate(item))
          {
            if (!silent)
              MessageBox.Show("Skin is not compatible with current MediaPortal version.", "Dependency error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
          }
          continue;
        }
        if (!MpeInstaller.VersionProviders[item.Type].Validate(item))
        {
          if (item.WarnOnly && item.Type != MPDependency.DisplayName)
          {
            if (!silent)
              MessageBox.Show(string.Format("{0}", item.Message), "Dependency warning", MessageBoxButtons.OK,
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
      if (!hasMPDependency)
      {
        return false;
      }
      if (IsSkin && !hasSkinDependency)
      {
        return false;
      }
      if (!CheckPluginsDependencies())
      {
        return false;
      }
      return true;
    }

    public bool CheckPluginsDependencies()
    {
      foreach (PluginDependencyItem dep in PluginDependencies.Items)
      {
        if (!CheckPluginDependency(dep))
          return false;
      }
      return true;
    }

    public static bool CheckPluginDependency(PluginDependencyItem dep)
    {
      XmlSerializer xs = new XmlSerializer(typeof(PluginDependencyItem));
      String XmlizedString = null;
      MemoryStream memoryStream = new MemoryStream();
      using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.ASCII))
      {
        xs.Serialize(xmlTextWriter, dep);
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
      }
      XmlizedString = ByteArrayToString(Encoding.ASCII, memoryStream.ToArray());
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(XmlizedString);
      return MediaPortal.Common.Utils.CompatibilityManager.IsPluginCompatible(doc.DocumentElement);
    }

    /// <summary>
    /// Checks wether package includes a skin.
    /// </summary>
    /// <returns>Returns true if package conatins a skin</returns>
    public bool ProvidesSkin(out FileItem referenceFile)
    {
      IsSkin = false;
      referenceFile = null;
      System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
      foreach (FileItem file in UniqueFileList.Items)
      {
        if (file.DestinationFilename.StartsWith("%" + MediaPortal.Configuration.Config.Dir.Skin + "%", true, invariantCulture) 
          && file.DestinationFilename.EndsWith("references.xml", true, invariantCulture))
        {
          referenceFile = file;
          IsSkin = true;
          return true;
        }
      }
      return false;
    }

    public bool ProvidesPlugins()
    {
      System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
      foreach (FileItem file in UniqueFileList.Items)
      {
        if (file.DestinationFilename.StartsWith("%" + MediaPortal.Configuration.Config.Dir.Plugins + "%", true, invariantCulture)
          && file.DestinationFilename.EndsWith(".dll", true, invariantCulture))
        {
          return true;
        }
      }
      return false;
    }

    private static string ByteArrayToString(Encoding encoding, byte[] byteArray)
    {
      return encoding.GetString(byteArray);
    }

    /// <summary>
    /// Checks if package has a MediaPortal dependency.
    /// </summary>
    /// <returns>Returns true if package has the dependency</returns>
    public bool CheckMPDependency(out DependencyItem dep)
    {
      MpeCore.Classes.VersionProvider.MediaPortalVersion MPDependency = new Classes.VersionProvider.MediaPortalVersion();
      if (CheckDependency(MPDependency, out dep))
      {
        if (dep.MaxVersion.CompareTo(MPDependency.Version(null)) > 0)
        {
          dep.MaxVersion = MPDependency.Version(null);
        }
        return true;
      }
      return false;
    }

    /// <summary>
    /// Checks if package has a Skin dependency.
    /// </summary>
    /// <returns>Returns true if package has the dependency</returns>
    public bool CheckSkinDependency(out DependencyItem dep)
    {
      MpeCore.Classes.VersionProvider.SkinVersion SkinDependency = new Classes.VersionProvider.SkinVersion();
      return CheckDependency(SkinDependency, out dep);
    }

    /// <summary>
    /// Checks if package has a dependency of the specified type.
    /// </summary>
    /// <param name="depType">Type of VersionProvider to check for</param>
    /// <param name="depItem">Specific dependency item in dpendencies collection that is of the desired type</param>
    /// <returns>Returns true if package has the dependency</returns>
    public bool CheckDependency(MpeCore.Interfaces.IVersionProvider depType, out DependencyItem depItem)
    {
      depItem = null;
      foreach (DependencyItem item in Dependencies.Items)
      {
        if (item.Type == depType.DisplayName)
        {
          depItem = item;
          return true;
        }
      }
      return false;
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

    public void GenerateUniqueFileList()
    {
      FileItemCollection copy = new FileItemCollection();
      foreach (FileItem item in UniqueFileList.Items)
        copy.Add(item);
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
            {
              FileItem existingItem = copy.GetByLocalFileName(sectionParam.Value);
              if (existingItem != null)
                UniqueFileList.Add(existingItem);
              else
                UniqueFileList.Add(new FileItem(sectionParam.Value, true));
            }
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
              {
                FileItem existingItem = copy.GetByLocalFileName(sectionParam.Value);
                if (existingItem != null)
                  UniqueFileList.Add(existingItem);
                else
                  UniqueFileList.Add(new FileItem(sectionParam.Value, true));
              }
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
          {
            FileItem existingItem = copy.GetByLocalFileName(sectionParam.Value);
            if (existingItem != null)
              UniqueFileList.Add(existingItem);
            else
              UniqueFileList.Add(new FileItem(sectionParam.Value, true));
          }
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
      var serializer = new XmlSerializer(typeof (PackageClass));
      using (TextWriter writer = new StreamWriter(fileName))
      {
        serializer.Serialize(writer, this);
        writer.Close();
      }
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
          var serializer = new XmlSerializer(typeof (PackageClass));
          var fs = new FileStream(fileName, FileMode.Open);
          var packageClass = (PackageClass)serializer.Deserialize(fs);
          fs.Close();
          this.Groups = packageClass.Groups;
          this.Sections = packageClass.Sections;
          this.GeneralInfo = packageClass.GeneralInfo;
          this.UniqueFileList = packageClass.UniqueFileList;
          this.Dependencies = packageClass.Dependencies;
          this.ProjectSettings = packageClass.ProjectSettings;
          this.PluginDependencies = packageClass.PluginDependencies;
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

      if (!Path.IsPathRooted(xmlFile))
        xmlFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.ProjectSettings.ProjectFilename), xmlFile));

      ExtensionCollection list = ExtensionCollection.Load(xmlFile);

      PackageClass pakToAdd = this;
      pakToAdd.GeneralInfo.OnlineLocation = ReplaceInfo(pakToAdd.GeneralInfo.OnlineLocation);

      SectionParam iconParam = pakToAdd.GeneralInfo.Params[ParamNamesConst.ICON];
      pakToAdd.GeneralInfo.Params.Items.Remove(pakToAdd.GeneralInfo.Params[ParamNamesConst.ICON]);

      list.Add(pakToAdd);

      list.Save(xmlFile);

      pakToAdd.GeneralInfo.Params.Items.Insert(0, iconParam);

      return true;
    }

    public void SetPluginsDependencies()
    {
      PluginDependencies.Items.Clear();
      List<string> providedPlugins = new List<string>();
      System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
      foreach (FileItem file in UniqueFileList.Items)
      {
        if (file.DestinationFilename.StartsWith("%" + MediaPortal.Configuration.Config.Dir.Plugins + "%", true, invariantCulture)
          && file.DestinationFilename.EndsWith(".dll", true, invariantCulture))
        {
          string asm = file.LocalFileName;
          string assemblyPath = asm;
          if (!Path.IsPathRooted(asm))
          {
            assemblyPath = Path.Combine(Path.GetDirectoryName(ProjectSettings.ProjectFilename), assemblyPath);
            assemblyPath = Path.GetFullPath(assemblyPath);
          }
          if (Util.IsPlugin(assemblyPath))
          {
            providedPlugins.Add(asm);
          }
        }
      }
      foreach (string asm in providedPlugins)
      {
        PluginDependencyItem dep = new PluginDependencyItem();
        dep.AssemblyName = Path.GetFileName(asm);
        string assemblyPath = asm;
        if (!Path.IsPathRooted(asm))
        {
          assemblyPath = Path.Combine(Path.GetDirectoryName(ProjectSettings.ProjectFilename), assemblyPath);
          assemblyPath = Path.GetFullPath(assemblyPath);
        }
        if (dep.ScanVersionInfo(assemblyPath))
        {
          PluginDependencies.Add(dep);
        }
      }
    }

    private static DependencyItem CreateStrictDependency(MpeCore.Interfaces.IVersionProvider depType)
    {
      DependencyItem depItem = new DependencyItem();
      depItem.Type = depType.DisplayName;
      depItem.WarnOnly = false;
      depItem.MinVersion = depType.Version(null);
      depItem.MaxVersion = depType.Version(null);
      depItem.Name = depType.DisplayName;
      return depItem;
    }

    public void CreateMPDependency()
    {
      MpeCore.Classes.VersionProvider.MediaPortalVersion MPVersion = new MpeCore.Classes.VersionProvider.MediaPortalVersion();
      Dependencies.Add(CreateStrictDependency(MPVersion));
    }

    public void CreateSkinDependency(FileItem referenceFile)
    {
      MpeCore.Classes.VersionProvider.SkinVersion skinVersion = new MpeCore.Classes.VersionProvider.SkinVersion();
      DependencyItem dep;
      CheckSkinDependency(out dep);
      if (dep != null)
      {
        Dependencies.Items.Remove(dep);
      }
      dep = CreateStrictDependency(skinVersion);
      Version versionSkin = null;
      string fileName = referenceFile.LocalFileName;
      if (!Path.IsPathRooted(fileName))
      {
        fileName = Path.Combine(Path.GetDirectoryName(ProjectSettings.ProjectFilename), fileName);
        fileName = Path.GetFullPath(fileName);
      }
      if (File.Exists(fileName))
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);
        XmlNode node = doc.SelectSingleNode("/controls/skin/version");
        if (node != null && node.InnerText != null)
        {
          versionSkin = new Version(node.InnerText);
          dep.MinVersion = new VersionInfo(versionSkin);
          dep.MaxVersion = new VersionInfo(versionSkin);
        }
      }
      Dependencies.Add(dep);
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