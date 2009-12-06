using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeCore.Classes.Project;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;
using MpeMaker.Dialogs;
using MpeMaker.Sections;
using MpeMaker.Classes;

namespace MpeMaker
{
    public partial class MainForm : Form
    {
        public PackageClass Package { get; set; }
        Dictionary<string, Control> panels = new Dictionary<string, Control>();
        public string ProjectFileName = "";
        
        public MainForm()
        {
            MpeInstaller.Init();
            InitializeComponent();
            Package = new PackageClass();
            Init();
            NewProject();
        }

        public MainForm(ProgramArguments arguments)
        {
            MpeInstaller.Init();
            InitializeComponent();
            Package = new PackageClass();
            Init();
            //NewProject();
            if (File.Exists(arguments.ProjectFile))
            {
                if(LoadProject(arguments.ProjectFile))
                {
                    if (arguments.SetVersion)
                        Package.GeneralInfo.Version = arguments.Version;
                    if(arguments.Build)
                    {
                        if (string.IsNullOrEmpty(Package.GeneralInfo.Location))
                            MessageBox.Show("No out file is specified");
                        List<string> list = Package.ValidatePackage();
                        if(Package.ValidatePackage().Count>0)
                        {
                            MessageBox.Show("Error in package");
                            Close();
                            return;
                        }
                        MpeInstaller.ZipProvider.Save(Package, Package.ReplaceInfo(Package.GeneralInfo.Location));
                        Close();
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Project file not specified or not found !");
            }
        }


        private void Init()
        {
            splitContainer1.Panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            treeView1.ExpandAll();
            panels.Add("Node0", new WelcomSection());
            panels.Add("Node2", new GeneralSection());
            panels.Add("Node3", new FilesGroupsSection());
            panels.Add("Node4", new InstallSections());
            panels.Add("Node5", new RequirementsSection());
            panels.Add("Node6", new BuildSection());
            panels.Add("Node7", new ToolsUpdateXml());
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (panels.ContainsKey(e.Node.Name))
            {
                splitContainer1.Panel2.Controls.Clear();
                panels[e.Node.Name].Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top |
                                              AnchorStyles.Left);
                panels[e.Node.Name].Dock = DockStyle.Fill;
                splitContainer1.Panel2.Controls.Add(panels[e.Node.Name]);
                ((ISectionControl)panels[e.Node.Name]).Set(Package);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnu_save_Click(object sender, EventArgs e)
        {
            if (File.Exists(ProjectFileName))
                Save(ProjectFileName);
            else
            {
                mnu_saveAs_Click(null, null);
            }
        }

        private void mnu_saveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Mpe project file(*.xmp2)|*.xmp2|All files|*.*";
            saveFileDialog1.Title = "Save extension installer proiect file";
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Save(saveFileDialog1.FileName);
            }
        }

        private void Save(string file)
        {
            Package.GenerateRelativePath(Path.GetDirectoryName(file));
            Package.Save(file);
            Package.GenerateAbsolutePath(Path.GetDirectoryName(file));
            ProjectFileName = file;
            SetTitle();
        }

        private void SetTitle()
        {
            this.Text = "MpeMaker - " + ProjectFileName;
        }

        private void mnu_open_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Mpe project file(*.xmp2)|*.xmp2|All files|*.*";
            openFileDialog1.Title = "Open extension installer project file";
            openFileDialog1.FileName = ProjectFileName;
            openFileDialog1.Multiselect = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if(!LoadProject(openFileDialog1.FileName))
                {
                    MessageBox.Show("Wrong project file format !");
                    return;
                }
            }
            ProjectFileName = openFileDialog1.FileName;
            SetTitle();
        }

        private bool LoadProject(string filename)
        {
            PackageClass pak = new PackageClass();
            if (!pak.Load(filename))
            {
                MessageBox.Show("Error loading package project");
                return false;
            }
            Package = pak;
            Package.GenerateAbsolutePath(Path.GetDirectoryName(filename));
            foreach (FolderGroup folderGroup in Package.ProjectSettings.FolderGroups)
            {
                ProjectSettings.UpdateFiles(Package, folderGroup);
            }
            ProjectFileName = filename;
            treeView1.SelectedNode = treeView1.Nodes[0];
            SetTitle();
            return true;
        }

        private void mnu_new_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("All not saved changes will be lost, \n Do you want to continue ?", "New project", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                NewProject();
            }
        }

        private void NewProject()
        {
            NewFileSelector newf = new NewFileSelector(Package);
            if (newf.ShowDialog() == DialogResult.OK)
            {
                treeView1.SelectedNode = treeView1.Nodes[0];
                Package = newf.Package;
                ProjectFileName = Package.ProjectSettings.ProjectFilename;
                SetTitle();
            }
        }


    }
}
