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
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeCore.Classes.Project;
using MpeMaker.Classes;
using MpeMaker.Dialogs;
using MpeMaker.Sections;
using MpeMaker.Wizards;

namespace MpeMaker
{
  public partial class MainForm : Form
  {
    private const string mpeFileDialogFilter = "Mpe project file(*.xmp2)|*.xmp2|All files|*.*";

    private readonly Dictionary<string, Control> _panels = new Dictionary<string, Control>();

    #region Constructors

    public MainForm()
    {
      Init();
      OpenNewFileSelector();
    }

    public MainForm(ProgramArguments arguments)
    {
      Init();

      if (!File.Exists(arguments.ProjectFile))
      {
        MessageBox.Show("Project file not specified or not found !");
        return;
      }

      // load project specified by app arguments
      PackageClass loadedProject = LoadProject(arguments.ProjectFile);
      // if loading failed, stop here and show mainform
      if (loadedProject == null) return;
      // update project
      UpdatePackage(loadedProject);

      if (arguments.SetVersion)
        Package.GeneralInfo.Version = arguments.Version;

      if (arguments.UpdateXML)
        Package.WriteUpdateXml(Package.ProjectSettings.UpdatePath1);

      if (arguments.Build)
      {
        if (string.IsNullOrEmpty(Package.GeneralInfo.Location))
          Console.WriteLine("[MpeMaker] No out file is specified");

        List<string> list = Package.ValidatePackage();
        if (list.Count > 0)
        {
          Console.WriteLine("[MpeMaker] Error in package");
          foreach (string s in list)
          {
            Console.WriteLine("[MpeMaker] " + s);
          }
          Close();
          return;
        }
        MpeInstaller.ZipProvider.Save(Package, Package.ReplaceInfo(Package.GeneralInfo.Location));
        Close();
        return;
      }
    }

    private void Init()
    {
      MpeInstaller.Init();
      InitializeComponent();
      Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

      Package = new PackageClass();

      splitContainer1.Panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      treeView1.ExpandAll();
      _panels.Add("Node0", new WelcomSection());
      _panels.Add("Node2", new GeneralSection());
      _panels.Add("Node3", new FilesGroupsSection());
      _panels.Add("Node4", new InstallSections());
      _panels.Add("Node5", new RequirementsSection());
      _panels.Add("Node6", new BuildSection());
      _panels.Add("Node7", new ToolsUpdateXml());

      openFileDialog.Filter = mpeFileDialogFilter;
      saveFileDialog.Filter = mpeFileDialogFilter;
    }

    #endregion

    #region Properties

    public PackageClass Package { get; set; }

    public string ProjectFileName
    {
      get
      {
        if (Package == null) return string.Empty;
        if (Package.ProjectSettings == null) return string.Empty;

        return Package.ProjectSettings.ProjectFilename;
      }
    }

    #endregion

    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
    {
      if (_panels.ContainsKey(e.Node.Name))
      {
        splitContainer1.Panel2.Controls.Clear();
        _panels[e.Node.Name].Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top |
                                      AnchorStyles.Left);
        _panels[e.Node.Name].Dock = DockStyle.Fill;
        splitContainer1.Panel2.Controls.Add(_panels[e.Node.Name]);
        ((ISectionControl)_panels[e.Node.Name]).Set(Package);
      }
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      e.Cancel = !LoosingChangesConfirmed("Close MpeMaker");
    }

    #region Menu

    private void mnu_new_Click(object sender, EventArgs e)
    {
      if (LoosingChangesConfirmed("New Project"))
      {
        OpenNewFileSelector();
      }
    }

    private void mnu_open_Click(object sender, EventArgs e)
    {
      if (LoosingChangesConfirmed("Open Project"))
      {
        OpenFile();
      }
    }

    private void OpenFile()
    {
      openFileDialog.FileName = ProjectFileName;
      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        LoadProject(openFileDialog.FileName);
      }
    }

    private void mnu_save_Click(object sender, EventArgs e)
    {
      if (File.Exists(ProjectFileName))
        SaveProject(ProjectFileName);
      else
      {
        mnu_saveAs_Click(null, null);
      }
    }

    private void mnu_saveAs_Click(object sender, EventArgs e)
    {
      if (saveFileDialog.ShowDialog() == DialogResult.OK)
      {
        SaveProject(saveFileDialog.FileName);
      }
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Close();
    }

    private static bool LoosingChangesConfirmed(string caption)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("All not saved changes will be lost,");
      stringBuilder.AppendLine("Do you want to continue?");

      return MessageBox.Show(stringBuilder.ToString(), caption, MessageBoxButtons.YesNo) == DialogResult.Yes;
    }

    #endregion

    #region New / Load / Save project

    private static PackageClass GetNewProject()
    {
      PackageClass packageClass = new PackageClass();
      packageClass.Groups.Items.Add(new GroupItem("Default"));
      packageClass.Sections.Add("Welcome Screen");
      packageClass.Sections.Items[0].WizardButtonsEnum = WizardButtonsEnum.NextCancel;
      packageClass.Sections.Add("Install Section");
      var item = new ActionItem("InstallFiles")
      {
        Params =
          new SectionParamCollection(
          MpeInstaller.ActionProviders["InstallFiles"].GetDefaultParams())
      };
      packageClass.Sections.Items[1].Actions.Add(item);
      packageClass.Sections.Items[1].WizardButtonsEnum = WizardButtonsEnum.Next;
      packageClass.Sections.Add("Setup Complete");
      packageClass.Sections.Items[2].WizardButtonsEnum = WizardButtonsEnum.Finish;

      return packageClass;
    }

    private static PackageClass LoadProject(string filename)
    {
      PackageClass pak = new PackageClass();
      if (!pak.Load(filename))
      {
        MessageBox.Show("Error loading package project");
        return null;
      }

      pak.GenerateAbsolutePath(Path.GetDirectoryName(filename));
      foreach (FolderGroup folderGroup in pak.ProjectSettings.FolderGroups)
      {
        ProjectSettings.UpdateFiles(pak, folderGroup);
      }

      pak.ProjectSettings.ProjectFilename = filename;

      return pak;
    }

    private void SaveProject(string filename)
    {
      Package.ProjectSettings.ProjectFilename = filename;
      Package.GenerateRelativePath(Path.GetDirectoryName(filename));
      Package.Save(filename);
      Package.GenerateAbsolutePath(Path.GetDirectoryName(filename));

      SetTitle();
    }

    #endregion

    private void SetTitle()
    {
      Text = "MpeMaker - " + ProjectFileName;
    }

    private void OpenNewFileSelector()
    {
      Hide();

      NewFileSelector newFileSelector = new NewFileSelector();
      DialogResult dialogResult = newFileSelector.ShowDialog();

      Show();
      BringToFront();

      if (dialogResult == DialogResult.OK)
      {
        switch (newFileSelector.MpeStartupResult)
        {
          case MpeStartupResult.NewFile:
            UpdatePackage(GetNewProject());
            break;

          case MpeStartupResult.OpenFile:
            OpenFile();
            break;

          case MpeStartupResult.SkinWizard:
            UpdatePackage(NewSkin.Get(Package));
            break;
        }
      }
    }

    private void UpdatePackage(PackageClass packageClass)
    {
      if (packageClass == null) return;

      Package = packageClass;
      treeView1.SelectedNode = treeView1.Nodes[0];
      SetTitle();
    }
  }
}