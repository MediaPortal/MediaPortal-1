using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeCore.Classes.Project;
using MpeCore.Interfaces;
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
      listView1.Items[0].Selected = true;
    }

    private void btn_ok_Click(object sender, EventArgs e)
    {
      this.Hide();
      switch (listView1.SelectedIndices[0])
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
      this.DialogResult = DialogResult.OK;
      this.Close();
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
      openFileDialog1.Filter = "Mpe project file(*.xmp2)|*.xmp2|All files|*.*";
      openFileDialog1.Title = "Open extension installer project file";
      openFileDialog1.Multiselect = false;
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        PackageClass pak = new PackageClass();
        if (!pak.Load(openFileDialog1.FileName))
        {
          MessageBox.Show("Error loading package project");
        }
        Package = pak;
        Package.GenerateAbsolutePath(Path.GetDirectoryName(openFileDialog1.FileName));
        foreach (FolderGroup folderGroup in Package.ProjectSettings.FolderGroups)
        {
          ProjectSettings.UpdateFiles(Package, folderGroup);
        }
        Package.ProjectSettings.ProjectFilename = openFileDialog1.FileName;
      }
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      btn_ok.Enabled = listView1.SelectedItems.Count > 0;
    }

    private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
        btn_ok_Click(sender, null);
    }
  }
}