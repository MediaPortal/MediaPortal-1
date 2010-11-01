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

using System.Diagnostics;
using System;
using System.IO;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;
using IWshRuntimeLibrary;

namespace MpeCore.Classes.ActionType
{
  internal class CreateShortCut : IActionType
  {
    private const string Const_Loc = "ShortCut location";
    private const string Const_Target = "ShortCut target";
    private const string Const_Description = "Description";
    private const string Const_Icon = "Icon of the shortcut";

    public event FileInstalledEventHandler ItemProcessed;


    public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
    {
      return 1;
    }

    public string DisplayName
    {
      get { return "CreateShortCut"; }
    }

    public string Description
    {
      get { return "Creat a shortcut"; }
    }

    public SectionParamCollection GetDefaultParams()
    {
      var Params = new SectionParamCollection();
      Params.Add(new SectionParam(Const_Loc, "", ValueTypeEnum.Template,
                                  "Location of shortcut"));
      Params.Add(new SectionParam(Const_Target, "", ValueTypeEnum.Template,
                                  "Target of short cut"));
      Params.Add(new SectionParam(Const_Description, "", ValueTypeEnum.String,
                                  "Description tooltip text "));
      Params.Add(new SectionParam(Const_Icon, "", ValueTypeEnum.Template,
                                  "Icon of the shortcut, \n if is empty the icon of the target will be used"));

      return Params;
    }

    public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
    {
      if (ItemProcessed != null)
        ItemProcessed(this, new InstallEventArgs("Create ShortCut"));

      try
      {
        UnInstallItem unInstallItem =
          packageClass.UnInstallInfo.BackUpFile(actionItem.Params[Const_Loc].GetValueAsPath(), "CopyFile");

        WshShellClass wshShell = new WshShellClass();
        // Create the shortcut

        IWshShortcut myShortcut = (IWshShortcut)wshShell.CreateShortcut(actionItem.Params[Const_Loc].GetValueAsPath());
        myShortcut.TargetPath = Path.GetFullPath(actionItem.Params[Const_Target].GetValueAsPath());
        myShortcut.WorkingDirectory = Path.GetDirectoryName(myShortcut.TargetPath);
        myShortcut.Description = actionItem.Params[Const_Description].Value;

        if (!string.IsNullOrEmpty(actionItem.Params[Const_Icon].Value))
          myShortcut.IconLocation = actionItem.Params[Const_Icon].GetValueAsPath();
        else
          myShortcut.IconLocation = actionItem.Params[Const_Target].GetValueAsPath();

        myShortcut.Save();

        FileInfo info = new FileInfo(actionItem.Params[Const_Loc].GetValueAsPath());
        unInstallItem.FileDate = info.CreationTimeUtc;
        unInstallItem.FileSize = info.Length;
        packageClass.UnInstallInfo.Items.Add(unInstallItem);
      }
      catch (Exception) {}

      return SectionResponseEnum.Ok;
    }

    public ValidationResponse Validate(PackageClass packageClass, ActionItem actionItem)
    {
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