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
using MpeCore.Interfaces;
using MpeMaker.Dialogs;

namespace MpeMaker.Sections
{
    public partial class FilesGroupsSection : UserControl, ISectionControl
    {
        public PackageClass Package { get; set; }

        public GroupItem SelectedGroup { get; set; }

        public FileItem SelectedItem { get; set; }

        public FilesGroupsSection()
        {
            InitializeComponent();
            foreach (var keyValuePair in MpeInstaller.InstallerTypeProviders)
            {
                cmb_installtype.Items.Add(keyValuePair.Key);
            }
        }

        #region ISectionControl Members

        public void Set(PackageClass pak)
        {
            Package = pak;
            treeView1.Nodes.Clear();
            foreach (GroupItem group in Package.Groups.Items)
            {
                TreeNode node = AddGroup(group);
                foreach (FileItem item in group.Files.Items)
                {
                    AddFile(node, item);
                }
            }
        }

        public PackageClass Get()
        {
            return Package;
        }

        #endregion



        private TreeNode AddGroup(GroupItem group)
        {
            TreeNode node = new TreeNode {Text = group.Name, Tag = group};
            treeView1.Nodes.Add(node);
            return node;
        }

        private void AddFile(TreeNode node, FileItem file)
        {
            GroupItem group = node.Tag as GroupItem;
            TreeNode newnode = new TreeNode();
            newnode.Text = file.ToString();
            newnode.Tag = file;
            newnode.ToolTipText = Path.GetFullPath(file.LocalFileName);
            node.Nodes.Add(newnode);
        }

        private TreeNode GetSelectedGroupNode()
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode == null)
                return null;
            if (selectedNode.Tag.GetType() != typeof(GroupItem))
            {
                if (selectedNode.Parent != null)
                {
                    return selectedNode.Parent;
                }
                return null;
            }
            return selectedNode;
        }

        private void mnu_add_group_Click(object sender, EventArgs e)
        {
            GroupEdit dlg = new GroupEdit();
            GroupItem group = new GroupItem();
            group.Name = string.Format("Group{0}", Package.Groups.Items.Count);
            dlg.Set(group);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                AddGroup(dlg.Get());
                Package.Groups.Add(group);
            }
        }

        private void mnu_add_files_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = GetSelectedGroupNode();
            if (selectedNode == null)
            {
                MessageBox.Show("No node selected !");
                return;
            }

            openFileDialog1.Title = "Select files";
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string item in openFileDialog1.FileNames)
                {
                    FileItem fil = new FileItem(item, false);
                    AddFile(selectedNode, fil);
                    ((GroupItem)selectedNode.Tag).Files.Add(fil);
                }
            }
        }

        void SetProperties(TreeNode node)
        {
            var group = GetSelectedGroupNode().Tag as GroupItem;
            var file = node.Tag as FileItem;
            SelectedGroup = null;
            SelectedItem = null;
            if (group != null)
            {
                cmb_parentGroup.Items.Clear();
                cmb_parentGroup.Items.Add(string.Empty);
                foreach (GroupItem groupItem in Package.Groups.Items)
                {
                    if(groupItem.Name.CompareTo(group.Name)!=0)
                    {
                        cmb_parentGroup.Items.Add(groupItem.Name);
                    }
                }
                tabPage_file.Enabled = false;
                txt_description.Text = group.Description;
                chk_default.Checked = group.DefaulChecked;
                txt_displlayName.Text = group.DisplayName;
                cmb_parentGroup.Text = group.ParentGroup;
            }
            if (file != null)
            {
                tabControl1.SelectTab(1);
                tabPage_file.Enabled = true;
                txt_installpath.Text = file.DestinationFilename;
                cmb_installtype.Text = file.InstallType;
            }
            else
            {
                tabControl1.SelectTab(0);
            }
            SelectedGroup = group;
            SelectedItem = file;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                SetProperties(e.Node);
            }
        }

        private void txt_description_TextChanged(object sender, EventArgs e)
        {
            if (SelectedGroup != null)
            {
                SelectedGroup.Description = txt_description.Text;
                SelectedGroup.DefaulChecked = chk_default.Checked;
                SelectedGroup.DisplayName = txt_displlayName.Text;
                SelectedGroup.ParentGroup = cmb_parentGroup.Text;
            }
            if (SelectedItem != null)
            {
                SelectedItem.InstallType = cmb_installtype.Text;
                SelectedItem.DestinationFilename = txt_installpath.Text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PathTemplateSelector dlg = new PathTemplateSelector();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (txt_intall_path.Text.Contains("%"))
                    txt_installpath.Text = dlg.Result + "\\" + Path.GetFileName(SelectedItem.LocalFileName);
                else
                    txt_installpath.Text = dlg.Result + txt_installpath.Text;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PathTemplateSelector dlg = new PathTemplateSelector();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txt_intall_path.Text = dlg.Result;
            }
        }

        private void btn_set_path_Click(object sender, EventArgs e)
        {
            SelectedGroup.SetPath(txt_intall_path.Text);
        }
    }
}
