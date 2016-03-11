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
using System.IO;
using System.Windows.Forms;
using CSScriptLibrary;
using MpeCore.Interfaces;

namespace MpeCore.Classes.SectionPanel
{
  public partial class UserVericalLayout : BaseVerticalLayout, ISectionPanel
  {
    private const string CONST_TEXT1 = "Header text";
    private const string CONST_IMAGE = "Left part image";
    private const string CONST_SHOW_SCRIPT = "On Show Script";
    private const string CONST_HIDE_SCRIPT = "On Hide Script";

    private SectionItem Section = new SectionItem();
    private PackageClass _packageClass = new PackageClass();

    #region ISectionPanel Members

    public string DisplayName
    {
      get { return "Custom Vertical layout"; }
    }

    public string Guid
    {
      get { return "{437ED9A5-B960-488c-9278-1BF7FB6E24B8}"; }
    }

    public SectionParamCollection GetDefaultParams()
    {
      SectionParamCollection param = new SectionParamCollection();
      param.Add(new SectionParam(CONST_TEXT1, "",
                                 ValueTypeEnum.String, ""));
      param.Add(new SectionParam(CONST_IMAGE, "", ValueTypeEnum.File, ""));
      param.Add(new SectionParam(ParamNamesConst.SECTION_ICON, "", ValueTypeEnum.File,
                                 "Image in upper right part"));
      param.Add(new SectionParam(CONST_SHOW_SCRIPT, "//css_reference \"MpeCore.dll\";\n" +
                                                    "\n" +
                                                    "using MpeCore;\n" +
                                                    "using MpeCore.Classes;\n" +
                                                    "using MpeCore.Classes.SectionPanel;\n" +
                                                    "\n" +
                                                    "public class Script\n" +
                                                    "{\n" +
                                                    "    public static void Main(PackageClass packageClass, UserVericalLayout form)\n" +
                                                    "    {\n" +
                                                    "        return;\n" +
                                                    "    }\n" +
                                                    "}\n"
                                 , ValueTypeEnum.Script,
                                 ""));
      param.Add(new SectionParam(CONST_HIDE_SCRIPT, "//css_reference \"MpeCore.dll\";\n" +
                                                    "\n" +
                                                    "using MpeCore;\n" +
                                                    "using MpeCore.Classes;\n" +
                                                    "using MpeCore.Classes.SectionPanel;\n" +
                                                    "\n" +
                                                    "public class Script\n" +
                                                    "{\n" +
                                                    "    public static void Main(PackageClass packageClass, SectionItem sectionItem, UserVericalLayout form)\n" +
                                                    "    {\n" +
                                                    "        return;\n" +
                                                    "    }\n" +
                                                    "}\n"
                                 , ValueTypeEnum.Script,
                                 ""));
      return param;
    }

    public void Preview(PackageClass packageClass, SectionItem sectionItem)
    {
      Section = sectionItem;
      _packageClass = packageClass;
      SetValues(sectionItem);
      ShowDialog();
    }

    public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
    {
      throw new NotImplementedException();
    }

    #endregion

    public UserVericalLayout()
    {
      InitializeComponent();
    }

    private void SetValues(SectionItem sectionItem)
    {
      Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
      try
      {
        AsmHelper script =
          new AsmHelper(CSScript.LoadCode(sectionItem.Params[CONST_SHOW_SCRIPT].Value,
                                          Path.GetTempFileName(), true));
        script.Invoke("Script.Main", _packageClass, sectionItem, this);
      }
      catch (Exception ex)
      {
        if (!_packageClass.Silent)
          MessageBox.Show("Eror in script : " + ex.Message);
      }
    }
  }
}