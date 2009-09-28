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
using MpeMaker.Dialogs;
using MpeMaker.Sections;

namespace MpeMaker
{
    public partial class MainForm : Form
    {
        public PackageClass Package { get; set; }
        Dictionary<string, Control> panels = new Dictionary<string, Control>();
        
        public MainForm()
        {
            MpeInstaller.Init();
            InitializeComponent();
            Package = new PackageClass();
            Package.Groups.Add(new GroupItem("Default"));

            treeView1.ExpandAll();
            panels.Add("Node3", new FilesGroupsSection());
            panels.Add("Node4", new InstallSections());
            panels.Add("Node2", new GeneralSection());
            panels.Add("Node6", new BuildSection());
        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (panels.ContainsKey(e.Node.Name))
            {
                splitContainer1.Panel2.Controls.Clear();
                splitContainer1.Panel2.Controls.Add(panels[e.Node.Name]);
                ((ISectionControl)panels[e.Node.Name]).Set(Package);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mnu_save_Click(object sender, EventArgs e)
        {

        }

        private void mnu_saveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "All files|*.*";
            saveFileDialog1.Title = "Save extension installer proiect file";
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Package.Save(saveFileDialog1.FileName);
            }
        }

        private void mnu_open_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "All files|*.*";
            openFileDialog1.Title = "Open extension installer proiect file";
            openFileDialog1.Multiselect = false;
            if(openFileDialog1.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                Package.Load(openFileDialog1.FileName);
                treeView1.SelectedNode = null;
            }
        }
    }
}
