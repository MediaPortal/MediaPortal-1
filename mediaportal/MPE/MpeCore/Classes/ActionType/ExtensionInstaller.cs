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
using System.Text;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;

namespace MpeCore.Classes.ActionType
{
  public class ExtensionInstaller : IActionType
  {
    public event FileInstalledEventHandler ItemProcessed;

    private const string Const_Loc = "Extension package";
    private const string Const_Guid = "Extension Id";
    private const string Const_Version = "Extension Version";
    private const string Const_Silent = "Silent Install";
    //private const string Const_Remove = "Remove on Uninstall";

    public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
    {
      return 1;
    }

    public string DisplayName
    {
      get { return "Extension Installer"; }
    }

    public string Description
    {
      get { return "Install a extension.\n If the extension is installed and it is older, first will be uninstalled \n"; }
    }

    public SectionParamCollection GetDefaultParams()
    {
      var Params = new SectionParamCollection();
      Params.Add(new SectionParam(Const_Loc, "", ValueTypeEnum.File,
                                  "Location of the extension package"));
      Params.Add(new SectionParam(Const_Guid, "", ValueTypeEnum.String,
                                  "Global Id of the extension package - it will be downloaded when no file is set."));
      Params.Add(new SectionParam(Const_Version, "", ValueTypeEnum.String,
                                  "Minimum Version of the extension package - older versions will be updated (only used when downloading via Id)."));
      Params.Add(new SectionParam(Const_Silent, "", ValueTypeEnum.Bool,
                                  "Silent install, No wizard screen will be displayed "));
      return Params;
    }

    public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
    {
      // load extension from zip if provided
      PackageClass embeddedPackage = null;
      if (!string.IsNullOrEmpty(actionItem.Params[Const_Loc].Value))
      {
        embeddedPackage = new PackageClass().ZipProvider.Load(actionItem.Params[Const_Loc].Value);
        if (embeddedPackage == null && string.IsNullOrEmpty(actionItem.Params[Const_Guid].Value))
          return SectionResponseEnum.Ok;
      }

      // check if there is already an installed version with a higher version than the embedded
      PackageClass installedPak = MpeInstaller.InstalledExtensions.Get(embeddedPackage != null ? embeddedPackage.GeneralInfo.Id : actionItem.Params[Const_Guid].Value);
      if (installedPak != null && embeddedPackage != null && installedPak.GeneralInfo.Version.CompareTo(embeddedPackage.GeneralInfo.Version) >= 0)
      {
          return SectionResponseEnum.Ok;
      }

      // download new version
      if (embeddedPackage == null && !string.IsNullOrEmpty(actionItem.Params[Const_Guid].Value) &&
        (installedPak == null ||
        (!string.IsNullOrEmpty(actionItem.Params[Const_Version].Value) && installedPak.GeneralInfo.Version.CompareTo(new Version(actionItem.Params[Const_Version].Value)) >= 0)))
      {
        // we don't want incompatible versions
        MpeInstaller.KnownExtensions.HideByDependencies();
        PackageClass knownPackage = MpeInstaller.KnownExtensions.Get(actionItem.Params[Const_Guid].Value);
        if (knownPackage == null && (DateTime.Now - ApplicationSettings.Instance.LastUpdate).TotalHours > 12)
        {
          // package unknown and last download of update info was over 12 hours ago -> update the list first
          ExtensionUpdateDownloader.UpdateList(false, false, null, null);
          // search for the package again - we don't want incompatible versions
          MpeInstaller.KnownExtensions.HideByDependencies();
          knownPackage = MpeInstaller.KnownExtensions.Get(actionItem.Params[Const_Guid].Value);
        }
        if (knownPackage != null)
        {
          // make sure the package has at least the asked version
          if (knownPackage.GeneralInfo.Version.CompareTo(new Version(actionItem.Params[Const_Version].Value)) >= 0)
          {
            // download extension package
            string newPackageLoacation = ExtensionUpdateDownloader.GetPackageLocation(knownPackage, null, null);
            if (File.Exists(newPackageLoacation))
              embeddedPackage = new PackageClass().ZipProvider.Load(newPackageLoacation);
          }
        }
      }

      if (embeddedPackage == null) // no package was embedded or downloaded
        return SectionResponseEnum.Ok;

      if (ItemProcessed != null)
        ItemProcessed(this, new InstallEventArgs("Install extension " + embeddedPackage.GeneralInfo.Name));

      if (installedPak != null)
      {
        // uninstall previous version, if the new package has the setting to force uninstall of previous version on update
        if (embeddedPackage.GeneralInfo.Params[ParamNamesConst.FORCE_TO_UNINSTALL_ON_UPDATE].GetValueAsBool())
        {
          installedPak.Silent = true;
          installedPak.UnInstallInfo = new UnInstallInfoCollection(installedPak);
          installedPak.UnInstallInfo = installedPak.UnInstallInfo.Load();
          if (installedPak.UnInstallInfo == null)
            installedPak.UnInstallInfo = new UnInstallInfoCollection();
          installedPak.UnInstall();
          embeddedPackage.CopyGroupCheck(installedPak);
          installedPak = null;
        }
      }

      embeddedPackage.Silent = actionItem.Params[Const_Silent].GetValueAsBool();
      if (embeddedPackage.StartInstallWizard())
      {
        if (installedPak != null)
        {
          MpeCore.MpeInstaller.InstalledExtensions.Remove(installedPak);
          MpeCore.MpeInstaller.Save();
        }
      }
      return SectionResponseEnum.Ok;
    }

    public ValidationResponse Validate(PackageClass packageClass, ActionItem actionItem)
    {
      if (string.IsNullOrEmpty(actionItem.Params[Const_Loc].Value) && string.IsNullOrEmpty(actionItem.Params[Const_Guid].Value))
        return new ValidationResponse() 
          { Valid = false, Message = "No file location and no Id specified" };
      if (!string.IsNullOrEmpty(actionItem.Params[Const_Loc].Value) && !File.Exists(actionItem.Params[Const_Loc].Value))
        return new ValidationResponse()
          { Valid = false, Message = "File not found " + actionItem.Params[Const_Loc].Value};
      if (!string.IsNullOrEmpty(actionItem.Params[Const_Guid].Value) && MpeInstaller.KnownExtensions.Get(actionItem.Params[Const_Guid].Value) == null)
        return new ValidationResponse() 
          { Valid = false, Message = "Extension with Id " + actionItem.Params[Const_Loc].Value + " unknown" };
      if (!string.IsNullOrEmpty(actionItem.ConditionGroup) && packageClass.Groups[actionItem.ConditionGroup] == null)
        return new ValidationResponse()
          { Valid = false,  Message = actionItem.Name + " condition group not found " + actionItem.ConditionGroup };
      return new ValidationResponse();
    }

    public SectionResponseEnum UnInstall(PackageClass packageClass, UnInstallItem item)
    {
      return SectionResponseEnum.Ok;
    }
  }
}