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
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeCore.Classes.Project;
using MpeMaker.Dialogs;

namespace MpeMaker.Sections
{
  public partial class FilesGroupsSection : UserControl, ISectionControl
  {
    #region Fields

    private bool _loading;

    #region ImageKeys for ImageList

    private const string ImageKeyError = "Error";
    private const string ImageKeyFile = "File";
    private const string ImageKeyGroup = "Group";

    #endregion

    #endregion

    #region Constructors

    public FilesGroupsSection()
    {
      InitializeComponent();
      foreach (var keyValuePair in MpeInstaller.InstallerTypeProviders)
      {
        cmb_installtype.Items.Add(keyValuePair.Key);
      }

      imageList.Images.Add(ImageKeyFile, Properties.Resources.text_x_generic_template);
      imageList.Images.Add(ImageKeyError, Properties.Resources.dialog_error);
      imageList.Images.Add(ImageKeyGroup, Properties.Resources.video_display);
    }

    #endregion

    #region Properties

    public PackageClass Package { get; set; }
    public GroupItem SelectedGroup { get; set; }
    public FileItem SelectedItem { get; set; }

    private TreeNode SelectedNode
    {
      get
      {
        if (treeView1.SelectedNodes.Count == 0) return null;

        return treeView1.SelectedNodes[0];
      }
      set
      {
        treeView1.SelectedNodes.Clear();
        treeView1.SelectedNodes.Add(value);
      }
    }

    #endregion

    #region ISectionControl Members

    public void Set(PackageClass pak)
    {
      Package = pak;
      treeView1.Nodes.Clear();
      PopulateTreeView();
      if (treeView1.Nodes.Count > 0)
        SelectedNode = treeView1.Nodes[0];
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
      TreeNode node = new TreeNode
                        {
                          Text = group.Name,
                          Tag = group,
                          ToolTipText = group.DisplayName,
                          ImageKey = ImageKeyGroup,
                          SelectedImageKey = ImageKeyGroup
                        };
      treeView1.Nodes.Add(node);
      return node;
    }

    private static void AddFile(TreeNode node, FileItem file)
    {
      GroupItem group = node.Tag as GroupItem;
      TreeNode newnode = new TreeNode();
      newnode.Text = file.ToString();
      newnode.Tag = file;
      newnode.ImageKey = File.Exists(Path.GetFullPath(file.LocalFileName)) ? ImageKeyFile : ImageKeyError;
      newnode.SelectedImageKey = File.Exists(Path.GetFullPath(file.LocalFileName)) ? ImageKeyFile : ImageKeyError;
      newnode.ToolTipText = Path.GetFullPath(file.LocalFileName);
      node.Nodes.Add(newnode);
    }

    private TreeNode GetSelectedGroupNode()
    {
      return GetGroupNode(SelectedNode);
    }

    private static TreeNode GetGroupNode(TreeNode selectedNode)
    {
      if (selectedNode == null) return null;

      if (selectedNode.Tag.GetType() != typeof (GroupItem))
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

      openFileDialog.Title = "Select files";
      openFileDialog.Multiselect = true;
      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        AddFiles(openFileDialog.FileNames);
      }
    }

    private void AddFiles(IEnumerable<string> files)
    {
      TreeNode selectedNode = GetSelectedGroupNode();
      if (selectedNode == null) return;

      foreach (string item in files)
      {
        if (!File.Exists(item))
          continue;
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

    private void SetProperties(TreeNode node)
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
          if (groupItem.Name.CompareTo(group.Name) != 0)
          {
            cmb_parentGroup.Items.Add(groupItem.Name);
          }
        }
        tabPage_file.Enabled = false;
        txt_description.Text = group.Description;
        chk_default.Checked = group.DefaulChecked;
        txt_displlayName.Text = group.DisplayName;
        cmb_parentGroup.Text = group.ParentGroup;
        tabPage_file.Enabled = group.Files.Items.Count > 0;

        list_folder.Items.Clear();
        foreach (FolderGroup folderGroup in Package.ProjectSettings.GetFolderGroups(group.Name))
        {
          list_folder.Items.Add(folderGroup);
        }
      }

      if (file != null)
      {
        tabControl1.SelectedTab = tabPage_file;
        tabPage_file.Enabled = true;
        btn_set.Enabled = false;
      }
      else if (group != null)
      {
        tabControl1.SelectedTab = tabPage_group;
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
      if (e.Node == null)
      {
        tabControl1.Enabled = false;

        mnu_add_group.Enabled = true;
        mnu_add_files.Enabled = false;
        mnu_add_folder.Enabled = false;

        mnu_remove_group.Enabled = false;
        mnu_remove_files.Enabled = false;
        return;
      }

      // set props on the right and enable tabcontrol
      if (treeView1.SelectedNodes.Count == 1)
      {
        SetProperties(e.Node);
        tabControl1.Enabled = true;
      }
      else
      {
        tabControl1.Enabled = false;
      }

      // if something is selected always allow to add stuff
      mnu_add_group.Enabled = true;
      mnu_add_files.Enabled = true;
      mnu_add_folder.Enabled = true;

      mnu_remove_files.Enabled = e.Node.Tag is FileItem;
      mnu_remove_group.Enabled = e.Node.Tag is GroupItem;
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
        if (SelectedNode != null && SelectedNode.Tag as GroupItem != null)
        {
          SelectedNode.ToolTipText = txt_displlayName.Text;
        }
      }
      if (SelectedItem != null)
      {
        toolTip.SetToolTip(cmb_installtype,
                            MpeInstaller.InstallerTypeProviders[cmb_installtype.Text].Description);
        SelectedItem.InstallType = cmb_installtype.Text;
        SelectedItem.DestinationFilename = txt_installpath.Text;
        SelectedItem.UpdateOption = (UpdateOptionEnum)cmb_overwrite.SelectedIndex;
        SelectedItem.Param1 = txt_param1.Text;
        SelectedItem.Modified = true;
        if (SelectedNode != null && SelectedNode.Tag as FileItem != null)
        {
          SelectedNode.Text = txt_installpath.Text;
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
      // only check selected for GroupItem
      // since we only allow multi select for same level (and parent node),
      // all items will be GroupItem if first item is it
      if (SelectedNode == null) return;
      if (!(SelectedNode.Tag is GroupItem)) return;

      string msgBoxText;
      string msgBoxCaption;
      if (treeView1.SelectedNodes.Count == 1)
      {
        msgBoxCaption = "Delete group";
        msgBoxText = String.Format("Do you want to delete group {0} from list?", SelectedNode.Tag as GroupItem);
      }
      else
      {
        msgBoxCaption = "Delete multiple groups";
        msgBoxText = String.Format("Do you want to delete {0} groups from list?", treeView1.SelectedNodes.Count);
      }

      if (MessageBox.Show(msgBoxText, msgBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

      foreach (TreeNode treeNode in treeView1.SelectedNodes)
      {
        GroupItem groupItem = (GroupItem)treeNode.Tag;
        Package.Groups.Items.Remove(groupItem);
        Package.ProjectSettings.RemoveFolderGroup(groupItem.Name);
      }

      treeView1.Nodes.Clear();
      PopulateTreeView();
      if (treeView1.Nodes.Count > 0)
        SelectedNode = treeView1.Nodes[0];
    }


    private void mnu_remove_files_Click(object sender, EventArgs e)
    {
      // only check selected for FileItem
      // since we only allow multi select for same level (and parent node),
      // all items will be FileItem if first item is it
      if (SelectedNode == null) return;
      if (!(SelectedNode.Tag is FileItem)) return;

      string msgBoxText;
      string msgBoxCaption;
      if (treeView1.SelectedNodes.Count == 1)
      {
        msgBoxCaption = "Delete file";
        msgBoxText = String.Format("Do you want to delete file {0} from list?", SelectedNode.Tag as FileItem);
      }
      else
      {
        msgBoxCaption = "Delete multiple files";
        msgBoxText = String.Format("Do you want to delete {0} files from list?", treeView1.SelectedNodes.Count);
      }

      if (MessageBox.Show(msgBoxText, msgBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

      foreach (TreeNode treeNode in treeView1.SelectedNodes)
      {
        FileItem fileItem = (FileItem)treeNode.Tag;
        SelectedGroup.Files.Items.Remove(fileItem);
        treeNode.Remove();
      }

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
      AddFolder2Group dlg = new AddFolder2Group(Package, SelectedGroup);
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        treeView1.Nodes.Clear();
        PopulateTreeView();
      }
    }

    private void treeView1_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
        e.Effect = DragDropEffects.All;
    }

    private void treeView1_DragOver(object sender, DragEventArgs e)
    {
      Point pt = treeView1.PointToClient(new Point(e.X, e.Y));
      TreeNode targetNode = treeView1.GetNodeAt(pt);
      if (targetNode != null)
      {
        SelectedNode = GetGroupNode(targetNode);
      }
    }

    private void treeView1_DragDrop(object sender, DragEventArgs e)
    {
      string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
      if (Directory.Exists(files[0]))
      {
        AddFolder2Group dlg = new AddFolder2Group(Package, SelectedGroup, files[0]);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          treeView1.Nodes.Clear();
          PopulateTreeView();
          return;
        }
      }
      if (files.Length > 0)
        AddFiles(files);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Delete)
      {
        e.Handled = true;

        if (sender == treeView1 && SelectedNode != null)
        {
          if (SelectedNode.Tag is GroupItem)
            mnu_remove_group_Click(null, null);
          else if (SelectedNode.Tag is FileItem)
            mnu_remove_files_Click(null, null);
        }
      }
    }

    private void treeView1_Leave(object sender, EventArgs e)
    {
      // prevent loosing focus to tabControl1, when
      // switching selected treenode from file to group item and vica versa
      if (_loading)
        treeView1.Focus();
    }
  }
}