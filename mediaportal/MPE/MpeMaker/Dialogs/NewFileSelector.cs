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
using System.Drawing;
using System.Windows.Forms;

namespace MpeMaker.Dialogs
{
  public enum MpeStartupResult
  {
    NewFile,
    OpenFile,
    SkinWizard
  }

  public partial class NewFileSelector : Form
  {
    public MpeStartupResult MpeStartupResult;

    public NewFileSelector()
    {
      InitializeComponent();
      Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

      imageList.Images.Add(Properties.Resources.document_new);
      imageList.Images.Add(Properties.Resources.document_open);
      imageList.Images.Add(Properties.Resources.applications_graphics);

      ListViewGroup wizardGroup = new ListViewGroup("Wizards", HorizontalAlignment.Left);
      listView.Groups.Add(wizardGroup);
      listView.Items.Add(new ListViewItem("New Project", 0));
      listView.Items.Add(new ListViewItem("Open project", 1));
      listView.Items.Add(new ListViewItem("New Skin Project Wizard", 2, wizardGroup));

      listView.Items[0].Selected = true;
    }

    private void btn_ok_Click(object sender, EventArgs e)
    {
      Hide();

      switch (listView.SelectedIndices[0])
      {
        case 0:
          MpeStartupResult = MpeStartupResult.NewFile;
          break;
        case 1:
          MpeStartupResult = MpeStartupResult.OpenFile;
          break;
        case 2:
          MpeStartupResult = MpeStartupResult.SkinWizard;
          break;
        default:
          break;
      }

      DialogResult = DialogResult.OK;
      Close();
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      btn_ok.Enabled = listView.SelectedItems.Count > 0;
    }

    private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (listView.SelectedItems.Count > 0)
        btn_ok_Click(sender, null);
    }
  }
}