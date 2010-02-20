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
using System.Windows.Forms;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;

namespace MpeCore.Classes.ActionType
{
  internal class ClearSkinCache : IActionType
  {
    public event FileInstalledEventHandler ItemProcessed;

    public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
    {
      return 2;
    }

    public string DisplayName
    {
      get { return "ClearSkinCache"; }
    }

    public string Description
    {
      get { return "Delete MediaPortal Skin cache folder"; }
    }

    public SectionParamCollection GetDefaultParams()
    {
      return new SectionParamCollection();
    }

    public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
    {
      if (ItemProcessed != null)
        ItemProcessed(this, new InstallEventArgs("Clear skin cache"));
      try
      {
        Directory.Delete(MpeInstaller.TransformInRealPath("%Cache%"), true);
        Directory.CreateDirectory(MpeInstaller.TransformInRealPath("%Cache%"));
      }
      catch (Exception)
      {
        if (ItemProcessed != null)
          ItemProcessed(this, new InstallEventArgs("Error to clear skin cache"));
        return SectionResponseEnum.Ok;
      }
      if (ItemProcessed != null)
        ItemProcessed(this, new InstallEventArgs("Clear skin cache done"));
      return SectionResponseEnum.Ok;
    }

    public ValidationResponse Validate(PackageClass packageClass, ActionItem actionItem)
    {
      if (!string.IsNullOrEmpty(actionItem.ConditionGroup) && packageClass.Groups[actionItem.ConditionGroup] == null)
        return new ValidationResponse
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