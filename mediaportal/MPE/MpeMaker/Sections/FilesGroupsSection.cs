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

        private bool _loading = false;

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
            PopulateTreeView();
            if (treeView1.Nodes.Count > 0)
                treeView1.SelectedNode = treeView1.Nodes[0];
        }

        private void PopulateTreeView()
        {
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
            TreeNode node = new TreeNode {Text = group.Name, Tag = group, ToolTipText = group.DisplayName};
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
                    FileItem fil = GetCommonItem(SelectedGroup); // new FileItem(item, false);
                    fil.LocalFileName = item;
                    fil.DestinationFilename = string.IsNullOrEmpty(fil.DestinationFilename)
                                                  ? MpeInstaller.InstallerTypeProviders[fil.InstallType].GetTemplatePath
                                                        (fil)
                                                  : fil.DestinationFilename + "\\" + Path.GetFileName(item);
                    AddFile(selectedNode, fil);
                    ((GroupItem)selectedNode.Tag).Files.Add(fil);
                }
            }
        }

        void SetProperties(TreeNode node)
        {
            _loading = true;
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
                if(group.Files.Items.Count>0)
                {
                    tabPage_file.Enabled = true;
                }
                else
                {
                    tabPage_file.Enabled = false;
                }
            }

            if (file != null)
            {
                tabControl1.SelectTab(1);
                tabPage_file.Enabled = true;
                btn_set.Enabled = false;
            }
            else if (group != null)
            {
                tabControl1.SelectTab(0);
                file = GetCommonItem(group);
                tabPage_file.Enabled = group.Files.Items.Count > 0;
                btn_set.Enabled = true;
            }

            if (file != null)
            {
                txt_installpath.Text = file.DestinationFilename;
                cmb_installtype.Text = file.InstallType;
                cmb_overwrite.SelectedIndex = (int)file.UpdateOption;
                txt_param1.Text = file.Param1;
            }

            SelectedGroup = group;
            SelectedItem = file;
            _loading = false;
        }

        private static FileItem GetCommonItem(GroupItem groupItem)
        {
            var resp = new FileItem();
            if (groupItem.Files.Items.Count > 0)
            {
                resp.DestinationFilename = string.IsNullOrEmpty(groupItem.Files.Items[0].DestinationFilename)
                                               ? ""
                                               : Path.GetDirectoryName(groupItem.Files.Items[0].DestinationFilename);
                resp.UpdateOption = groupItem.Files.Items[0].UpdateOption;
                resp.InstallType = groupItem.Files.Items[0].InstallType;
                resp.Param1 = groupItem.Files.Items[0].Param1;

                foreach (FileItem item in groupItem.Files.Items)
                {
                    if (string.IsNullOrEmpty(item.DestinationFilename) ||
                        resp.DestinationFilename != Path.GetDirectoryName(item.DestinationFilename))
                        resp.DestinationFilename = "";
                    if (resp.UpdateOption != item.UpdateOption)
                        resp.UpdateOption = UpdateOptionEnum.OverwriteIfOlder;
                    if (resp.InstallType != item.InstallType)
                        resp.InstallType = "CopyFile";
                    if (resp.Param1 != item.Param1)
                        resp.Param1 = string.Empty;
                }
            }
            return resp;
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
            if (_loading)
                return;

            if (SelectedGroup != null)
            {
                SelectedGroup.Description = txt_description.Text;
                SelectedGroup.DefaulChecked = chk_default.Checked;
                SelectedGroup.DisplayName = txt_displlayName.Text;
                SelectedGroup.ParentGroup = cmb_parentGroup.Text;
                if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag as GroupItem != null)
                {
                    treeView1.SelectedNode.ToolTipText = txt_displlayName.Text;
                }
            }
            if (SelectedItem != null)
            {
                toolTip1.SetToolTip(cmb_installtype,
                                    MpeInstaller.InstallerTypeProviders[cmb_installtype.Text].Description);
                SelectedItem.InstallType = cmb_installtype.Text;
                SelectedItem.DestinationFilename = txt_installpath.Text;
                SelectedItem.UpdateOption = (UpdateOptionEnum) cmb_overwrite.SelectedIndex;
                SelectedItem.Param1 = txt_param1.Text;
                if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag as FileItem != null)
                {
                    treeView1.SelectedNode.Text = txt_installpath.Text;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PathTemplateSelector dlg = new PathTemplateSelector();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txt_installpath.Text = dlg.Result + "\\" + Path.GetFileName(SelectedItem.LocalFileName);
            }
        }

 
        private void mnu_remove_group_Click(object sender, EventArgs e)
        {
            if (SelectedGroup == null)
                return;
            if (MessageBox.Show("Do you want to Delete group " + SelectedGroup.Name, "", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            Package.Groups.Items.Remove(SelectedGroup);
            treeView1.Nodes.Clear();
            PopulateTreeView();
            if (treeView1.Nodes.Count > 0)
                treeView1.SelectedNode = treeView1.Nodes[0];
        }

        private void mnu_remove_files_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode == null)
                return;
            FileItem item = selectedNode.Tag as FileItem;
            if (item == null)
                return;
            if (MessageBox.Show("Do you want to Delete file " + item + " from list ?", "", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            SelectedGroup.Files.Items.Remove(item);
            selectedNode.Remove();
            //treeView1.Nodes.Clear();
            //PopulateTreeView();
        }

        private void btn_set_Click(object sender, EventArgs e)
        {
            foreach (FileItem fileItem in SelectedGroup.Files.Items)
            {
                fileItem.InstallType = string.IsNullOrEmpty(SelectedItem.InstallType)
                                           ? fileItem.InstallType
                                           : SelectedItem.InstallType;
                fileItem.UpdateOption = SelectedItem.UpdateOption;
                fileItem.DestinationFilename = string.IsNullOrEmpty(SelectedItem.DestinationFilename)
                                                   ? fileItem.DestinationFilename
                                                   : Path.Combine(SelectedItem.DestinationFilename,
                                                                  Path.GetFileName(fileItem.LocalFileName));

            }
        }

        private void mnu_add_folder_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = GetSelectedGroupNode();
            if (selectedNode == null)
            {
                MessageBox.Show("No node selected !");
                return;
            }
            AddFolder2Group dlg = new AddFolder2Group(SelectedGroup);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                treeView1.Nodes.Clear();
                PopulateTreeView();
            }
        }
    }
}
