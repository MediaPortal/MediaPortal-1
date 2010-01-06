using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes.SectionPanel
{
  public class Base
  {
    public static void ActionExecute(PackageClass packageClass, SectionItem sectionItem,
                                     ActionExecuteLocationEnum locationEnum)
    {
      foreach (ActionItem list in sectionItem.Actions.Items)
      {
        if (list.ExecuteLocation != locationEnum)
          continue;
        if (!string.IsNullOrEmpty(list.ConditionGroup) && !packageClass.Groups[list.ConditionGroup].Checked)
          continue;
        MpeInstaller.ActionProviders[list.ActionType].Execute(packageClass, list);
      }
    }
  }
}