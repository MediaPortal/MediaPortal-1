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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MpeCore.Interfaces;
using MpeCore.Classes;

namespace MpeCore.Classes.SectionPanel
{
  internal class GroupCheck : Base, ISectionPanel
  {
    private const string Const_state = "State";

    public string DisplayName
    {
      get { return "[Group] Set state"; }
    }

    public string Guid
    {
      get { return "{91B3F10F-B91F-4c17-98E4-5D53B47E2BBD}"; }
    }

    public SectionParamCollection Init()
    {
      throw new NotImplementedException();
    }

    public SectionParamCollection GetDefaultParams()
    {
      SectionParamCollection _param = new SectionParamCollection();

      _param.Add(new SectionParam(Const_state, "", ValueTypeEnum.Bool,
                                  "All included groups will have this state"));
      return _param;
    }

    /// <summary>
    /// Previews the section form, but no change made.
    /// </summary>
    /// <param name="packageClass">The package class.</param>
    /// <param name="sectionItem">The param collection.</param>
    public void Preview(PackageClass packageClass, SectionItem sectionItem)
    {
      MessageBox.Show("This is a non visual Section. Nothing to show");
      ;
    }

    public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
    {
      Base.ActionExecute(packageClass, sectionItem, ActionExecuteLocationEnum.BeforPanelShow);
      Base.ActionExecute(packageClass, sectionItem, ActionExecuteLocationEnum.AfterPanelShow);
      foreach (string includedGroup in sectionItem.IncludedGroups)
      {
        packageClass.Groups[includedGroup].Checked = sectionItem.Params[Const_state].GetValueAsBool();
      }
      Base.ActionExecute(packageClass, sectionItem, ActionExecuteLocationEnum.AfterPanelHide);
      return SectionResponseEnum.Ok;
    }
  }
}