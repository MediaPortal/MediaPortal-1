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
using System.Diagnostics;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;

namespace MpeCore.Classes.ActionType
{
  internal class RunApplication : IActionType
  {
    private const string Const_APP = "Path to application";
    private const string Const_Params = "Parameters for application";
    private const string Const_Wait = "Wait for exit";
    private const string Const_DontRUnOnSilent = "Don't run when silent install";
    private const string Const_Un_APP = "Path to uninstall application";
    private const string Const_Un_Params = "Parameters for uninstall application";
    private const string Const_Un_Wait = "Wait for exit on uninstall";

    public event FileInstalledEventHandler ItemProcessed;

    public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
    {
      return 1;
    }

    public string DisplayName
    {
      get { return "RunApplication"; }
    }

    public string Description
    {
      get { return "Execute the specified application"; }
    }

    public SectionParamCollection GetDefaultParams()
    {
      var Params = new SectionParamCollection();
      Params.Add(new SectionParam(Const_APP, "", ValueTypeEnum.Template,
                                  "Path to the application like \n %Base%\\MediaPortal.exe"));
      Params.Add(new SectionParam(Const_Params, "", ValueTypeEnum.String,
                                  "Command line parameters"));
      Params.Add(new SectionParam(Const_Wait, "", ValueTypeEnum.Bool,
                            "Wait for exit "));
      Params.Add(new SectionParam(Const_DontRUnOnSilent, "", ValueTypeEnum.Bool,
                            "If set to Yes the aplication don't run when the istalation is silent "));
      Params.Add(new SectionParam(Const_Un_APP, "", ValueTypeEnum.Template,
                            "Path to the application which should be executed when uninstall"));
      Params.Add(new SectionParam(Const_Un_Params, "", ValueTypeEnum.String,
                                  "Command line parameters for uninstall app"));
      Params.Add(new SectionParam(Const_Un_Wait, "", ValueTypeEnum.Bool,
                            "Wait for exit on uninstall "));
      return Params;
    }

    public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
    {
      if (actionItem.Params[Const_APP].GetValueAsBool() && packageClass.Silent)
        return SectionResponseEnum.Ok;

      Process myProcess = new Process();

      try
      {
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.FileName = MpeInstaller.TransformInRealPath(actionItem.Params[Const_APP].Value);
        myProcess.StartInfo.Arguments = MpeInstaller.TransformInRealPath(actionItem.Params[Const_Params].Value);
        myProcess.StartInfo.CreateNoWindow = true;

        if (packageClass.Silent)
        {
          myProcess.StartInfo.CreateNoWindow = true;
          myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
        }
        myProcess.Start();
        if (actionItem.Params[Const_Wait].GetValueAsBool())
        {
          myProcess.WaitForExit();
          if (myProcess.ExitCode != 0)
            return SectionResponseEnum.Error;
        }
      }
      catch
      {
        if (ItemProcessed != null)
          ItemProcessed(this, new InstallEventArgs("Error to start application"));
        return SectionResponseEnum.Error;
      }
      UnInstallItem unInstallItem = new UnInstallItem();
      unInstallItem.ActionType = DisplayName;
      unInstallItem.ActionParam = new SectionParamCollection(actionItem.Params);
      unInstallItem.ActionParam[Const_APP].Value = actionItem.Params[Const_APP].GetValueAsPath();
      unInstallItem.ActionParam[Const_Un_APP].Value = actionItem.Params[Const_Un_APP].GetValueAsPath();
      packageClass.UnInstallInfo.Items.Add(unInstallItem);
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
      Process myProcess = new Process();

      try
      {
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.FileName = MpeInstaller.TransformInRealPath(item.ActionParam[Const_Un_APP].Value);
        myProcess.StartInfo.Arguments = MpeInstaller.TransformInRealPath(item.ActionParam[Const_Un_Params].Value);
        if (packageClass.Silent)
        {
          myProcess.StartInfo.CreateNoWindow = true;
          myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
        }
        myProcess.Start();
        if (item.ActionParam[Const_Un_Wait].GetValueAsBool())
          myProcess.WaitForExit();

      }
      catch (Exception e)
      {
        if (ItemProcessed != null)
          ItemProcessed(this, new InstallEventArgs("Error to start application"));
        return SectionResponseEnum.Ok;
      }
      if (ItemProcessed != null)
        ItemProcessed(this, new InstallEventArgs("Application start done"));
      return SectionResponseEnum.Ok;
    }
  }


}