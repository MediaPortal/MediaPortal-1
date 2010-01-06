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