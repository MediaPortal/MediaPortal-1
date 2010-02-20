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
using CSScriptLibrary;
using MpeCore;
using MpeCore.Interfaces;
using MpeCore.Classes;

namespace MpeCore.Classes.SectionPanel
{
  public class GroupCheckScript : Base, ISectionPanel
  {
    private const string Const_script = "Script";

    public string DisplayName
    {
      get { return "[Group] Set state script"; }
    }

    public string Guid
    {
      get { return "{6E83B2AC-92D1-4a45-9A3E-D02AE8E9D8ED}"; }
    }

    public SectionParamCollection Init()
    {
      throw new NotImplementedException();
    }

    public SectionParamCollection GetDefaultParams()
    {
      SectionParamCollection _param = new SectionParamCollection();

      _param.Add(new SectionParam(Const_script, "//css_reference \"MpeCore.dll\";\n" +
                                                "\n" +
                                                "using MpeCore.Classes;\n" +
                                                "using MpeCore;\n" +
                                                "\n" +
                                                "public class Script\n" +
                                                "{\n" +
                                                "    public static bool GetState(PackageClass packageClass, SectionItem sectionItem)\n" +
                                                "    {\n" +
                                                "        return true;\n" +
                                                "    }\n" +
                                                "}\n"
                                  , ValueTypeEnum.Script,
                                  "All included groups will have this state returned by the function \n public static bool GetState(PackageClass packageClass, SectionItem sectionItem)"));
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
      bool state = false;
      try
      {
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        AsmHelper script =
          new AsmHelper(CSScript.LoadCode(sectionItem.Params[Const_script].Value,
                                          Path.GetTempFileName(), true));
        state = (bool)script.Invoke("Script.GetState", packageClass, sectionItem);
        MessageBox.Show("Result of script : " + state.ToString());
      }
      catch (Exception ex)
      {
        MessageBox.Show("Eror in script : " + ex.Message);
      }
    }

    public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
    {
      Base.ActionExecute(packageClass, sectionItem, ActionExecuteLocationEnum.BeforPanelShow);
      Base.ActionExecute(packageClass, sectionItem, ActionExecuteLocationEnum.AfterPanelShow);
      Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
      bool state = false;
      try
      {
        AsmHelper script =
          new AsmHelper(CSScript.LoadCode(sectionItem.Params[Const_script].Value,
                                          Path.GetTempFileName(), true));
        state = (bool)script.Invoke("Script.GetState", packageClass, sectionItem);
      }
      catch (Exception ex)
      {
        if (!packageClass.Silent)
          MessageBox.Show("Eror in script : " + ex.Message);
        state = false;
      }

      foreach (string includedGroup in sectionItem.IncludedGroups)
      {
        packageClass.Groups[includedGroup].Checked = state;
      }
      Base.ActionExecute(packageClass, sectionItem, ActionExecuteLocationEnum.AfterPanelHide);
      return SectionResponseEnum.Ok;
    }
  }
}