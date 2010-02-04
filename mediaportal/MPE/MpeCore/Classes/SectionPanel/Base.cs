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

namespace MpeCore.Classes.SectionPanel
{
  public class Base
  {
    public static SectionResponseEnum ActionExecute(PackageClass packageClass, SectionItem sectionItem,
                                     ActionExecuteLocationEnum locationEnum)
    {
      SectionResponseEnum responseEnum = SectionResponseEnum.Ok;
      foreach (ActionItem list in sectionItem.Actions.Items)
      {
        if (list.ExecuteLocation != locationEnum)
          continue;
        if (!string.IsNullOrEmpty(list.ConditionGroup) && !packageClass.Groups[list.ConditionGroup].Checked)
          continue;
        responseEnum = MpeInstaller.ActionProviders[list.ActionType].Execute(packageClass, list);
        if (responseEnum != SectionResponseEnum.Ok)
        {
          break;
        }
      }
      return responseEnum;
    }

  }
}