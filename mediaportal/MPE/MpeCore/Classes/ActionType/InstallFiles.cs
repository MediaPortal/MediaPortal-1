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
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;

namespace MpeCore.Classes.ActionType
{
  internal class InstallFiles : IActionType
  {
    public event FileInstalledEventHandler ItemProcessed;

    public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
    {
      return packageClass.GetInstallableFileCount();
    }

    public string DisplayName
    {
      get { return "InstallFiles"; }
    }

    public string Description
    {
      get { return "Install all files witch have group checked"; }
    }


    public SectionParamCollection GetDefaultParams()
    {
      return new SectionParamCollection();
    }

    public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
    {
      packageClass.FileInstalled += packageClass_FileInstalled;
      packageClass.Install();
      packageClass.FileInstalled -= packageClass_FileInstalled;
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

    private void packageClass_FileInstalled(object sender, InstallEventArgs e)
    {
      if (ItemProcessed != null)
        ItemProcessed(sender, e);
    }
  }
}