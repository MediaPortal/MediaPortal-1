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
using MpeCore;
using MpeCore.Classes;
using MpeCore.Classes.Project;
using MpeMaker.Wizards;

namespace MpeMaker.Dialogs
{
  public partial class NewFileSelector : Form
  {
    public PackageClass Package;

    public NewFileSelector(PackageClass packageClass)
    {
      Package = packageClass;
      InitializeComponent();

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
          New();
          break;
        case 1:
          Open();
          break;
        case 2:
          Package = NewSkin.Get(Package);
          break;
        default:
          break;
      }

      DialogResult = DialogResult.OK;
      Close();
    }

    private void New()
    {
      Package = new PackageClass();
      Package.Groups.Items.Add(new GroupItem("Default"));
      Package.Sections.Add("Welcome Screen");
      Package.Sections.Items[0].WizardButtonsEnum = WizardButtonsEnum.NextCancel;
      Package.Sections.Add("Install Section");
      var item = new ActionItem("InstallFiles")
                   {
                     Params =
                       new SectionParamCollection(
                       MpeInstaller.ActionProviders["InstallFiles"].GetDefaultParams())
                   };
      Package.Sections.Items[1].Actions.Add(item);
      Package.Sections.Items[1].WizardButtonsEnum = WizardButtonsEnum.Next;
      Package.Sections.Add("Setup Complete");
      Package.Sections.Items[2].WizardButtonsEnum = WizardButtonsEnum.Finish;
    }

    private void Open()
    {
      openFileDialog.Filter = "Mpe project file(*.xmp2)|*.xmp2|All files|*.*";
      openFileDialog.Title = "Open extension installer project file";
      openFileDialog.Multiselect = false;
      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        PackageClass pak = new PackageClass();
        if (!pak.Load(openFileDialog.FileName))
        {
          MessageBox.Show("Error loading package project");
        }
        Package = pak;
        Package.GenerateAbsolutePath(Path.GetDirectoryName(openFileDialog.FileName));
        foreach (FolderGroup folderGroup in Package.ProjectSettings.FolderGroups)
        {
          ProjectSettings.UpdateFiles(Package, folderGroup);
        }
        Package.ProjectSettings.ProjectFilename = openFileDialog.FileName;
      }
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