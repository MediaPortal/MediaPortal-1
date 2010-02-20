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
      Params.Add(new SectionParam(Const_Silent, "", ValueTypeEnum.Bool,
                                  "Silent install, No wizard screen will be displayed "));
      return Params;
    }

    public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
    {
      PackageClass pak = new PackageClass();
      pak = pak.ZipProvider.Load(actionItem.Params[Const_Loc].Value);
      if (pak == null)
        return SectionResponseEnum.Ok;

      if (ItemProcessed != null)
        ItemProcessed(this, new InstallEventArgs("Install extension " + pak.GeneralInfo.Name));

      PackageClass installedPak = MpeInstaller.InstalledExtensions.Get(pak.GeneralInfo.Id);
      if (installedPak != null)
      {
        int i = installedPak.GeneralInfo.Version.CompareTo(pak.GeneralInfo.Version);
        if (installedPak.GeneralInfo.Version.CompareTo(pak.GeneralInfo.Version) >= 0)
          return SectionResponseEnum.Ok;
        installedPak.Silent = true;
        installedPak.UnInstallInfo = new UnInstallInfoCollection(installedPak);
        installedPak.UnInstallInfo = installedPak.UnInstallInfo.Load();
        if (installedPak.UnInstallInfo == null)
          installedPak.UnInstallInfo = new UnInstallInfoCollection();
        installedPak.UnInstall();
        pak.CopyGroupCheck(installedPak);
      }
      if (actionItem.Params[Const_Silent].GetValueAsBool())
      {
        pak.Silent = true;
      }
      pak.StartInstallWizard();
      return SectionResponseEnum.Ok;
    }

    public ValidationResponse Validate(PackageClass packageClass, ActionItem actionItem)
    {
      if (!File.Exists(actionItem.Params[Const_Loc].Value))
        return new ValidationResponse()
                 {Valid = false, Message = " [Install Extension] File not found " + actionItem.Params[Const_Loc].Value};
      if (!string.IsNullOrEmpty(actionItem.ConditionGroup) && packageClass.Groups[actionItem.ConditionGroup] == null)
        return new ValidationResponse()
                 {
                   Message = actionItem.Name + " condition group not found " + actionItem.ConditionGroup,
                   Valid = false
                 };

      return new ValidationResponse();
    }

    public SectionResponseEnum UnInstall(PackageClass packageClass, UnInstallItem item)
    {
      return SectionResponseEnum.Ok;
    }
  }
}