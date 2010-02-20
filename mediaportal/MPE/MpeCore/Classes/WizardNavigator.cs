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
using System.Windows.Forms;
using MpeCore.Classes.SectionPanel;

namespace MpeCore.Classes
{
  public class WizardNavigator
  {
    public PackageClass Package { get; set; }
    private Stack<int> stack = new Stack<int>();
    public SectionResponseEnum Response { get; set; }


    public WizardNavigator(PackageClass pak)
    {
      Package = pak;
      Response = SectionResponseEnum.Error;
    }

    public SectionResponseEnum Navigate()
    {
      int pos = 0;
      while (pos < Package.Sections.Items.Count)
      {
        SectionItem currentItem = Package.Sections.Items[pos];
        if (!string.IsNullOrEmpty(currentItem.ConditionGroup))
        {
          if (!Package.Groups[currentItem.ConditionGroup].Checked)
          {
            pos++;
            continue;
          }
        }
        Response = MpeInstaller.SectionPanels[currentItem.PanelName].Execute(Package,
                                                                             currentItem);
        switch (Response)
        {
          case SectionResponseEnum.Back:
            pos = stack.Pop();
            break;
          case SectionResponseEnum.Next:
            stack.Push(pos);
            pos++;
            break;
          case SectionResponseEnum.Cancel:
            if (
              MessageBox.Show(
                "Are you want to quit " + Package.GeneralInfo.Name + " - " + Package.GeneralInfo.Version +
                " setup ? ", "Install extension", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) !=
              DialogResult.Yes)
              continue;
            bool sil = Package.Silent;
            Package.Silent = true;
            Package.UnInstall();
            Package.Silent = sil;
            return SectionResponseEnum.Cancel;
          case SectionResponseEnum.Ok:
            break;
          case SectionResponseEnum.Error:
            //if (!Package.Silent)
            //  MessageBox.Show("Error on installation. Installation aborted !");
            bool sil_ = Package.Silent;
            Package.Silent = true;
            Package.UnInstall();
            Package.Silent = sil_;
            return SectionResponseEnum.Error;
            break;
        }
        if (Response != SectionResponseEnum.Back && Response != SectionResponseEnum.Next)
        {
          break;
        }
      }

      return SectionResponseEnum.Ok;
    }
  }
}