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
using CSScriptLibrary;


namespace MpeCore.Classes.ActionType
{
  internal class Script : IActionType
  {
    private const string Const_script = "Script";

    public event FileInstalledEventHandler ItemProcessed;

    public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
    {
      return 1;
    }

    public string DisplayName
    {
      get { return "Script"; }
    }

    public string Description
    {
      get { return "Execute a custom CSScript "; }
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
                                                "    public static void Main(PackageClass packageClass, ActionItem actionItem)\n" +
                                                "    {\n" +
                                                "        return;\n" +
                                                "    }\n" +
                                                "}\n"
                                  , ValueTypeEnum.Script,
                                  ""));
      return _param;
    }

    public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
    {
      Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
      try
      {
        AsmHelper script =
          new AsmHelper(CSScript.LoadCode(actionItem.Params[Const_script].Value,
                                          Path.GetTempFileName(), true));
        script.Invoke("Script.Main", packageClass, actionItem);
      }
      catch (Exception ex)
      {
        if (!packageClass.Silent)
          MessageBox.Show("Eror in script : " + ex.Message);
      }
      if (ItemProcessed != null)
        ItemProcessed(this, new InstallEventArgs("Script executed " + actionItem.Name));
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