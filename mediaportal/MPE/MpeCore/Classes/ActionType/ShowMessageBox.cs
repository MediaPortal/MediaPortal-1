using System;
using System.Windows.Forms;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;

namespace MpeCore.Classes.ActionType
{
  internal class ShowMessageBox : IActionType
  {
    private const string Const_MESSAGE = "Message";
    public event FileInstalledEventHandler ItemProcessed;

    public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
    {
      return 1;
    }

    public string DisplayName
    {
      get { return "MessageBox"; }
    }

    public string Description
    {
      get { return "Display a message box with specified message"; }
    }

    public SectionParamCollection GetDefaultParams()
    {
      var Params = new SectionParamCollection();
      Params.Add(new SectionParam(Const_MESSAGE, "", ValueTypeEnum.String,
                                  "Message text to show"));
      return Params;
    }

    public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
    {
      if (ItemProcessed != null)
        ItemProcessed(this, new InstallEventArgs("Message box"));
      MessageBox.Show(actionItem.Params[Const_MESSAGE].Value);
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