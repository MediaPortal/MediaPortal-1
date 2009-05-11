#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration.Controls;
using MediaPortal.Profile;
using MediaPortal.TV.Database;
using MediaPortal.UserInterface.Controls;
using MWCommon;
using MWControls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class TVGroups : SectionSettings
  {
    private IContainer components = null;
    private MPTabControl tabControlTvGroups;
    private MPTabPage tabPageTvChannelGroups;
    private MPButton buttonGroupDown;
    private MPButton buttonGroupUp;
    private MPButton buttonEditGroup;
    private MPButton buttonDeleteGroup;
    private MPButton buttonAddGroup;
    private MPListView listViewGroups;
    private MPTabPage tabPageMapChannels;
    private MWTreeView treeViewProviders;
    private MPButton buttonChannelDown;
    private MPButton buttonChannelUp;
    private MPButton buttonMap;
    private MPButton buttonUnmap;
    private MPListView listViewTVChannelsInGroup;
    private MPLabel labelTvChannelGroup;
    private MPComboBox comboBoxTvChannelGroups;
    private ColumnHeader columnHeaderGroupName;
    private ColumnHeader columnHeaderPincode;
    private ColumnHeader columnHeaderTvChannelsInGroup;
    private MPListView listViewHeaderProviders;
    private ColumnHeader columnHeaderAvailableTvChannels;

    //
    // Private members
    //
    private bool _itemsModified = false;
    private ListViewColumnSorter _columnSorter;
    private static bool _reloadList = false;
    private MPCheckBox cbHideAllChannels;
    private static bool _providerInit = false;

    public TVGroups()
      : this("TV Channel Groups")
    {
    }

    public TVGroups(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      treeViewProviders.MultiSelect = TreeViewMultiSelect.MultiSameBranchAndLevel;

      // Disable if TVE3
      if (File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        this.Enabled = false;
      }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources =
        new System.ComponentModel.ComponentResourceManager(typeof (TVGroups));
      this.tabControlTvGroups = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageTvChannelGroups = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.buttonGroupDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonGroupUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonEditGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDeleteGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonAddGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewGroups = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeaderGroupName = new System.Windows.Forms.ColumnHeader();
      this.columnHeaderPincode = new System.Windows.Forms.ColumnHeader();
      this.tabPageMapChannels = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.listViewHeaderProviders = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeaderAvailableTvChannels = new System.Windows.Forms.ColumnHeader();
      this.treeViewProviders = new MWControls.MWTreeView();
      this.buttonChannelDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonChannelUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonMap = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUnmap = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewTVChannelsInGroup = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeaderTvChannelsInGroup = new System.Windows.Forms.ColumnHeader();
      this.labelTvChannelGroup = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxTvChannelGroups = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cbHideAllChannels = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlTvGroups.SuspendLayout();
      this.tabPageTvChannelGroups.SuspendLayout();
      this.tabPageMapChannels.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControlTvGroups
      // 
      this.tabControlTvGroups.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlTvGroups.Controls.Add(this.tabPageTvChannelGroups);
      this.tabControlTvGroups.Controls.Add(this.tabPageMapChannels);
      this.tabControlTvGroups.Location = new System.Drawing.Point(0, 0);
      this.tabControlTvGroups.Name = "tabControlTvGroups";
      this.tabControlTvGroups.SelectedIndex = 0;
      this.tabControlTvGroups.Size = new System.Drawing.Size(472, 408);
      this.tabControlTvGroups.TabIndex = 0;
      // 
      // tabPageTvChannelGroups
      // 
      this.tabPageTvChannelGroups.AutoScroll = true;
      this.tabPageTvChannelGroups.Controls.Add(this.cbHideAllChannels);
      this.tabPageTvChannelGroups.Controls.Add(this.buttonGroupDown);
      this.tabPageTvChannelGroups.Controls.Add(this.buttonGroupUp);
      this.tabPageTvChannelGroups.Controls.Add(this.buttonEditGroup);
      this.tabPageTvChannelGroups.Controls.Add(this.buttonDeleteGroup);
      this.tabPageTvChannelGroups.Controls.Add(this.buttonAddGroup);
      this.tabPageTvChannelGroups.Controls.Add(this.listViewGroups);
      this.tabPageTvChannelGroups.Location = new System.Drawing.Point(4, 22);
      this.tabPageTvChannelGroups.Name = "tabPageTvChannelGroups";
      this.tabPageTvChannelGroups.Size = new System.Drawing.Size(464, 382);
      this.tabPageTvChannelGroups.TabIndex = 1;
      this.tabPageTvChannelGroups.Text = "TV Channel Groups";
      this.tabPageTvChannelGroups.UseVisualStyleBackColor = true;
      // 
      // buttonGroupDown
      // 
      this.buttonGroupDown.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonGroupDown.Location = new System.Drawing.Point(16, 350);
      this.buttonGroupDown.Name = "buttonGroupDown";
      this.buttonGroupDown.Size = new System.Drawing.Size(72, 22);
      this.buttonGroupDown.TabIndex = 5;
      this.buttonGroupDown.Text = "Down";
      this.buttonGroupDown.UseVisualStyleBackColor = true;
      this.buttonGroupDown.Click += new System.EventHandler(this.buttonGroupDown_Click);
      // 
      // buttonGroupUp
      // 
      this.buttonGroupUp.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonGroupUp.Location = new System.Drawing.Point(16, 322);
      this.buttonGroupUp.Name = "buttonGroupUp";
      this.buttonGroupUp.Size = new System.Drawing.Size(72, 22);
      this.buttonGroupUp.TabIndex = 4;
      this.buttonGroupUp.Text = "Up";
      this.buttonGroupUp.UseVisualStyleBackColor = true;
      this.buttonGroupUp.Click += new System.EventHandler(this.buttonGroupUp_Click);
      // 
      // buttonEditGroup
      // 
      this.buttonEditGroup.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonEditGroup.Location = new System.Drawing.Point(376, 335);
      this.buttonEditGroup.Name = "buttonEditGroup";
      this.buttonEditGroup.Size = new System.Drawing.Size(72, 22);
      this.buttonEditGroup.TabIndex = 3;
      this.buttonEditGroup.Text = "Edit";
      this.buttonEditGroup.UseVisualStyleBackColor = true;
      this.buttonEditGroup.Click += new System.EventHandler(this.buttonEditGroup_Click);
      // 
      // buttonDeleteGroup
      // 
      this.buttonDeleteGroup.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDeleteGroup.Location = new System.Drawing.Point(288, 335);
      this.buttonDeleteGroup.Name = "buttonDeleteGroup";
      this.buttonDeleteGroup.Size = new System.Drawing.Size(72, 22);
      this.buttonDeleteGroup.TabIndex = 2;
      this.buttonDeleteGroup.Text = "Delete";
      this.buttonDeleteGroup.UseVisualStyleBackColor = true;
      this.buttonDeleteGroup.Click += new System.EventHandler(this.buttonDeleteGroup_Click);
      // 
      // buttonAddGroup
      // 
      this.buttonAddGroup.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonAddGroup.Location = new System.Drawing.Point(195, 335);
      this.buttonAddGroup.Name = "buttonAddGroup";
      this.buttonAddGroup.Size = new System.Drawing.Size(72, 22);
      this.buttonAddGroup.TabIndex = 1;
      this.buttonAddGroup.Text = "Add";
      this.buttonAddGroup.UseVisualStyleBackColor = true;
      this.buttonAddGroup.Click += new System.EventHandler(this.buttonAddGroup_Click);
      // 
      // listViewGroups
      // 
      this.listViewGroups.AllowDrop = true;
      this.listViewGroups.AllowRowReorder = true;
      this.listViewGroups.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewGroups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                             {
                                               this.columnHeaderGroupName,
                                               this.columnHeaderPincode
                                             });
      this.listViewGroups.FullRowSelect = true;
      this.listViewGroups.HideSelection = false;
      this.listViewGroups.Location = new System.Drawing.Point(16, 44);
      this.listViewGroups.Name = "listViewGroups";
      this.listViewGroups.Size = new System.Drawing.Size(432, 260);
      this.listViewGroups.TabIndex = 0;
      this.listViewGroups.UseCompatibleStateImageBehavior = false;
      this.listViewGroups.View = System.Windows.Forms.View.Details;
      this.listViewGroups.DoubleClick += new System.EventHandler(this.listViewGroups_DoubleClick);
      this.listViewGroups.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewGroups_DragDrop);
      this.listViewGroups.ColumnClick +=
        new System.Windows.Forms.ColumnClickEventHandler(this.listViewGroups_ColumnClick);
      // 
      // columnHeaderGroupName
      // 
      this.columnHeaderGroupName.Text = "Group name";
      this.columnHeaderGroupName.Width = 346;
      // 
      // columnHeaderPincode
      // 
      this.columnHeaderPincode.Text = "Pincode";
      this.columnHeaderPincode.Width = 64;
      // 
      // tabPageMapChannels
      // 
      this.tabPageMapChannels.AutoScroll = true;
      this.tabPageMapChannels.Controls.Add(this.listViewHeaderProviders);
      this.tabPageMapChannels.Controls.Add(this.treeViewProviders);
      this.tabPageMapChannels.Controls.Add(this.buttonChannelDown);
      this.tabPageMapChannels.Controls.Add(this.buttonChannelUp);
      this.tabPageMapChannels.Controls.Add(this.buttonMap);
      this.tabPageMapChannels.Controls.Add(this.buttonUnmap);
      this.tabPageMapChannels.Controls.Add(this.listViewTVChannelsInGroup);
      this.tabPageMapChannels.Controls.Add(this.labelTvChannelGroup);
      this.tabPageMapChannels.Controls.Add(this.comboBoxTvChannelGroups);
      this.tabPageMapChannels.Location = new System.Drawing.Point(4, 22);
      this.tabPageMapChannels.Name = "tabPageMapChannels";
      this.tabPageMapChannels.Size = new System.Drawing.Size(464, 382);
      this.tabPageMapChannels.TabIndex = 2;
      this.tabPageMapChannels.Text = "Map Channels";
      this.tabPageMapChannels.UseVisualStyleBackColor = true;
      this.tabPageMapChannels.Visible = false;
      // 
      // listViewHeaderProviders
      // 
      this.listViewHeaderProviders.AllowDrop = true;
      this.listViewHeaderProviders.AllowRowReorder = true;
      this.listViewHeaderProviders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                                      {
                                                        this.columnHeaderAvailableTvChannels
                                                      });
      this.listViewHeaderProviders.FullRowSelect = true;
      this.listViewHeaderProviders.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewHeaderProviders.HideSelection = false;
      this.listViewHeaderProviders.Location = new System.Drawing.Point(16, 56);
      this.listViewHeaderProviders.Name = "listViewHeaderProviders";
      this.listViewHeaderProviders.Size = new System.Drawing.Size(176, 21);
      this.listViewHeaderProviders.TabIndex = 2;
      this.listViewHeaderProviders.TabStop = false;
      this.listViewHeaderProviders.UseCompatibleStateImageBehavior = false;
      this.listViewHeaderProviders.View = System.Windows.Forms.View.Details;
      // 
      // columnHeaderAvailableTvChannels
      // 
      this.columnHeaderAvailableTvChannels.Text = "Available TV Channels";
      this.columnHeaderAvailableTvChannels.Width = 154;
      // 
      // treeViewProviders
      // 
      this.treeViewProviders.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Left)));
      this.treeViewProviders.CheckedNodes =
        ((System.Collections.Hashtable) (resources.GetObject("treeViewProviders.CheckedNodes")));
      this.treeViewProviders.FullRowSelect = true;
      this.treeViewProviders.Location = new System.Drawing.Point(16, 76);
      this.treeViewProviders.Name = "treeViewProviders";
      this.treeViewProviders.SelNodes =
        ((System.Collections.Hashtable) (resources.GetObject("treeViewProviders.SelNodes")));
      this.treeViewProviders.Size = new System.Drawing.Size(176, 256);
      this.treeViewProviders.Sorted = true;
      this.treeViewProviders.TabIndex = 3;
      this.treeViewProviders.DoubleClick += new System.EventHandler(this.treeViewProviders_DoubleClick);
      // 
      // buttonChannelDown
      // 
      this.buttonChannelDown.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonChannelDown.Location = new System.Drawing.Point(364, 344);
      this.buttonChannelDown.Name = "buttonChannelDown";
      this.buttonChannelDown.Size = new System.Drawing.Size(84, 22);
      this.buttonChannelDown.TabIndex = 8;
      this.buttonChannelDown.Text = "Down";
      this.buttonChannelDown.UseVisualStyleBackColor = true;
      this.buttonChannelDown.Click += new System.EventHandler(this.buttonChannelDown_Click);
      // 
      // buttonChannelUp
      // 
      this.buttonChannelUp.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonChannelUp.Location = new System.Drawing.Point(272, 344);
      this.buttonChannelUp.Name = "buttonChannelUp";
      this.buttonChannelUp.Size = new System.Drawing.Size(80, 22);
      this.buttonChannelUp.TabIndex = 7;
      this.buttonChannelUp.Text = "Up";
      this.buttonChannelUp.UseVisualStyleBackColor = true;
      this.buttonChannelUp.Click += new System.EventHandler(this.buttonChannelUp_Click);
      // 
      // buttonMap
      // 
      this.buttonMap.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonMap.Location = new System.Drawing.Point(212, 168);
      this.buttonMap.Name = "buttonMap";
      this.buttonMap.Size = new System.Drawing.Size(40, 22);
      this.buttonMap.TabIndex = 4;
      this.buttonMap.Text = ">>";
      this.buttonMap.UseVisualStyleBackColor = true;
      this.buttonMap.Click += new System.EventHandler(this.buttonMap_Click);
      // 
      // buttonUnmap
      // 
      this.buttonUnmap.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonUnmap.Location = new System.Drawing.Point(212, 200);
      this.buttonUnmap.Name = "buttonUnmap";
      this.buttonUnmap.Size = new System.Drawing.Size(40, 22);
      this.buttonUnmap.TabIndex = 5;
      this.buttonUnmap.Text = "<<";
      this.buttonUnmap.UseVisualStyleBackColor = true;
      this.buttonUnmap.Click += new System.EventHandler(this.buttonUnmap_Click);
      // 
      // listViewTVChannelsInGroup
      // 
      this.listViewTVChannelsInGroup.AllowDrop = true;
      this.listViewTVChannelsInGroup.AllowRowReorder = true;
      this.listViewTVChannelsInGroup.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewTVChannelsInGroup.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                                        {
                                                          this.columnHeaderTvChannelsInGroup
                                                        });
      this.listViewTVChannelsInGroup.FullRowSelect = true;
      this.listViewTVChannelsInGroup.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewTVChannelsInGroup.HideSelection = false;
      this.listViewTVChannelsInGroup.Location = new System.Drawing.Point(272, 56);
      this.listViewTVChannelsInGroup.Name = "listViewTVChannelsInGroup";
      this.listViewTVChannelsInGroup.Size = new System.Drawing.Size(176, 276);
      this.listViewTVChannelsInGroup.TabIndex = 6;
      this.listViewTVChannelsInGroup.UseCompatibleStateImageBehavior = false;
      this.listViewTVChannelsInGroup.View = System.Windows.Forms.View.Details;
      this.listViewTVChannelsInGroup.DoubleClick += new System.EventHandler(this.listViewTVChannelsInGroup_DoubleClick);
      this.listViewTVChannelsInGroup.DragDrop +=
        new System.Windows.Forms.DragEventHandler(this.listViewTVChannelsInGroup_DragDrop);
      this.listViewTVChannelsInGroup.ColumnClick +=
        new System.Windows.Forms.ColumnClickEventHandler(this.listViewTVChannelsInGroup_ColumnClick);
      // 
      // columnHeaderTvChannelsInGroup
      // 
      this.columnHeaderTvChannelsInGroup.Text = "TV Channels in Group";
      this.columnHeaderTvChannelsInGroup.Width = 154;
      // 
      // labelTvChannelGroup
      // 
      this.labelTvChannelGroup.AutoSize = true;
      this.labelTvChannelGroup.Location = new System.Drawing.Point(16, 24);
      this.labelTvChannelGroup.Name = "labelTvChannelGroup";
      this.labelTvChannelGroup.Size = new System.Drawing.Size(98, 13);
      this.labelTvChannelGroup.TabIndex = 0;
      this.labelTvChannelGroup.Text = "TV Channel Group:";
      // 
      // comboBoxTvChannelGroups
      // 
      this.comboBoxTvChannelGroups.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxTvChannelGroups.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxTvChannelGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxTvChannelGroups.Location = new System.Drawing.Point(160, 20);
      this.comboBoxTvChannelGroups.Name = "comboBoxTvChannelGroups";
      this.comboBoxTvChannelGroups.Size = new System.Drawing.Size(288, 21);
      this.comboBoxTvChannelGroups.TabIndex = 1;
      this.comboBoxTvChannelGroups.SelectedIndexChanged +=
        new System.EventHandler(this.comboBoxTvChannelGroups_SelectedIndexChanged);
      // 
      // cbHideAllChannels
      // 
      this.cbHideAllChannels.AutoSize = true;
      this.cbHideAllChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbHideAllChannels.Location = new System.Drawing.Point(21, 15);
      this.cbHideAllChannels.Name = "cbHideAllChannels";
      this.cbHideAllChannels.Size = new System.Drawing.Size(149, 17);
      this.cbHideAllChannels.TabIndex = 6;
      this.cbHideAllChannels.Text = "Hide \"All Channels\" Group";
      this.cbHideAllChannels.UseVisualStyleBackColor = true;
      // 
      // TVGroups
      // 
      this.Controls.Add(this.tabControlTvGroups);
      this.Name = "TVGroups";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabControlTvGroups.ResumeLayout(false);
      this.tabPageTvChannelGroups.ResumeLayout(false);
      this.tabPageTvChannelGroups.PerformLayout();
      this.tabPageMapChannels.ResumeLayout(false);
      this.tabPageMapChannels.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        cbHideAllChannels.Checked = xmlreader.GetValueAsBool("mytv", "hideAllChannelsGroup", false);
      }
      LoadGroups();
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("mytv", "hideAllChannelsGroup", cbHideAllChannels.Checked);
      }
      if (_reloadList)
      {
        LoadGroups();
        _reloadList = false;
        _itemsModified = true;
      }
      SaveGroups();
    }

    public static void UpdateList()
    {
      _reloadList = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      if (_reloadList)
      {
        _reloadList = false;
        LoadGroups();
      }
      base.OnPaint(e);
    }

    public void LoadGroups()
    {
      listViewGroups.BeginUpdate();

      listViewGroups.Items.Clear();
      ArrayList groups = new ArrayList();
      TVDatabase.GetGroups(ref groups);
      foreach (TVGroup group in groups)
      {
        string pincode = "No";
        if (group.Pincode != 0)
        {
          pincode = "Yes";
        }
        ListViewItem listItem = new ListViewItem(new string[] {group.GroupName, pincode,});
        listItem.Tag = group;
        listViewGroups.Items.Add(listItem);
      }
      UpdateGroupChannels(null, true);

      listViewGroups.EndUpdate();
    }

    private void buttonEditGroup_Click(object sender, EventArgs e)
    {
      _itemsModified = true;

      foreach (ListViewItem listItem in listViewGroups.SelectedItems)
      {
        EditGroupForm editgroup = new EditGroupForm();
        editgroup.Group = listItem.Tag as TVGroup;
        DialogResult dialogResult = editgroup.ShowDialog(this);
        if (dialogResult == DialogResult.OK)
        {
          TVGroup group = editgroup.Group;
          listItem.Tag = group;
          TVDatabase.DeleteGroup(group);
          group.ID = -1;

          string pincode = "No";
          if (group.Pincode != 0)
          {
            pincode = "Yes";
          }

          listItem.SubItems[0].Text = group.GroupName;
          listItem.SubItems[1].Text = pincode;
          TVDatabase.AddGroup(group);

          SaveGroups();
          UpdateGroupChannels(group, true);
        }
      }
    }

    private void buttonDeleteGroup_Click(object sender, EventArgs e)
    {
      listViewGroups.BeginUpdate();

      int itemCount = listViewGroups.SelectedItems.Count;

      for (int index = 0; index < itemCount; index++)
      {
        _itemsModified = true;
        ListViewItem item = listViewGroups.SelectedItems[0];
        TVGroup group = item.Tag as TVGroup;
        if (group != null)
        {
          TVDatabase.DeleteGroup(group);
        }
        listViewGroups.Items.RemoveAt(listViewGroups.SelectedIndices[0]);
      }
      SaveGroups();
      UpdateGroupChannels(null, false);

      listViewGroups.EndUpdate();
    }

    private void buttonAddGroup_Click(object sender, EventArgs e)
    {
      listViewGroups.BeginUpdate();

      EditGroupForm editGroup = new EditGroupForm();
      DialogResult dialogResult = editGroup.ShowDialog(this);
      if (dialogResult == DialogResult.OK)
      {
        _itemsModified = true;
        TVGroup group = editGroup.Group;
        string pincode = "No";
        if (group.Pincode != 0)
        {
          pincode = "Yes";
        }
        ListViewItem listItem = new ListViewItem(new string[] {group.GroupName, pincode,});
        listItem.Tag = group;
        listViewGroups.Items.Add(listItem);

        SaveGroups();
        LoadGroups();

        UpdateGroupChannels(group, false);
      }

      listViewGroups.EndUpdate();
    }

    private void buttonGroupUp_Click(object sender, EventArgs e)
    {
      listViewGroups.BeginUpdate();

      _itemsModified = true;

      for (int index = 0; index < listViewGroups.Items.Count; index++)
      {
        if (listViewGroups.Items[index].Selected &&
            (index > 0)) // Make sure the current index isn't smaller than the lowest index (0) in the list view
        {
          ListViewItem listItem = listViewGroups.Items[index];
          listViewGroups.Items.RemoveAt(index);
          listViewGroups.Items.Insert(index - 1, listItem);
        }
      }

      SaveGroups();

      listViewGroups.EndUpdate();
    }

    private void buttonGroupDown_Click(object sender, EventArgs e)
    {
      listViewGroups.BeginUpdate();

      _itemsModified = true;

      for (int index = listViewGroups.Items.Count - 1; index >= 0; index--)
      {
        if (listViewGroups.Items[index].Selected &&
            (index < listViewGroups.Items.Count - 1))
          // Make sure the current index isn't greater than the highest index in the list view
        {
          ListViewItem listItem = listViewGroups.Items[index];
          listViewGroups.Items.RemoveAt(index);

          if (index + 1 < listViewGroups.Items.Count)
          {
            listViewGroups.Items.Insert(index + 1, listItem);
          }
          else
          {
            listViewGroups.Items.Add(listItem);
          }
        }
      }

      SaveGroups();

      listViewGroups.EndUpdate();
    }

    private void SaveGroups()
    {
      if (_itemsModified)
      {
        for (int index = 0; index < listViewGroups.Items.Count; index++)
        {
          ListViewItem listItem = listViewGroups.Items[index];
          TVGroup group = listItem.Tag as TVGroup;
          if (group != null)
          {
            group.Sort = index;
            TVDatabase.AddGroup(group);
          }
        }
      }
    }

    private void buttonMap_Click(object sender, EventArgs e)
    {
      if (treeViewProviders.SelNodes == null)
      {
        return;
      }

      treeViewProviders.BeginUpdate();
      listViewTVChannelsInGroup.BeginUpdate();

      Hashtable htSelNodes = treeViewProviders.SelNodes.Clone() as Hashtable;
      treeViewProviders.SelNodes = null;
      foreach (MWTreeNodeWrapper node in htSelNodes.Values)
      {
        TVChannel chan = node.Node.Tag as TVChannel;
        if (chan == null)
        {
          listViewTVChannelsInGroup.EndUpdate();
          treeViewProviders.EndUpdate();
          return;
        }
        ListViewItem listItem = new ListViewItem(new string[] {chan.Name});
        listItem.Tag = chan;
        listViewTVChannelsInGroup.Items.Add(listItem);
        TreeNode parentNode = node.Node.Parent;
        treeViewProviders.Nodes.Remove(node.Node);
        if (parentNode.Nodes.Count == 0)
        {
          treeViewProviders.Nodes.Remove(parentNode);
        }
      }
      SaveTVGroupChannelsAndMapping();
      //UpdateGroupChannels(null, true);

      listViewTVChannelsInGroup.EndUpdate();
      treeViewProviders.EndUpdate();
    }

    private void buttonUnmap_Click(object sender, EventArgs e)
    {
      if (listViewTVChannelsInGroup.SelectedItems == null)
      {
        return;
      }

      treeViewProviders.BeginUpdate();
      listViewTVChannelsInGroup.BeginUpdate();

      int selectedChannelId = 0;

      for (int i = 0; i < listViewTVChannelsInGroup.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewTVChannelsInGroup.SelectedItems[i];
        TVChannel chan = (TVChannel) listItem.Tag;

        foreach (TreeNode node in treeViewProviders.Nodes)
        {
          if (node.Text == chan.ProviderName)
          {
            TreeNode subnode = new TreeNode(chan.Name);
            subnode.Tag = chan;
            node.Nodes.Add(subnode);
          }
        }
        selectedChannelId = chan.ID;
      }
      TVGroup group = comboBoxTvChannelGroups.SelectedItem as TVGroup;
      for (int i = listViewTVChannelsInGroup.SelectedItems.Count - 1; i >= 0; --i)
      {
        ListViewItem listItem = listViewTVChannelsInGroup.SelectedItems[i];
        TVChannel channel = listItem.Tag as TVChannel;
        if (group != null && channel != null)
        {
          TVDatabase.UnmapChannelFromGroup(group, channel);
        }
        listViewTVChannelsInGroup.Items.Remove(listItem);
      }
      SaveTVGroupChannelsAndMapping();
      UpdateGroupChannels(group, true);

      foreach (TreeNode providerNode in treeViewProviders.Nodes)
      {
        foreach (TreeNode channelNode in providerNode.Nodes)
        {
          if (((TVChannel) channelNode.Tag).ID == selectedChannelId)
          {
            providerNode.Expand();
            treeViewProviders.SelectedNode = channelNode;
          }
        }
      }

      listViewTVChannelsInGroup.EndUpdate();
      treeViewProviders.EndUpdate();
    }

    private void comboBoxTvChannelGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
      _providerInit = false;
      TVGroup group = (TVGroup) comboBoxTvChannelGroups.SelectedItem;
      UpdateGroupChannels(group, false);
    }

    private void UpdateGroupChannels(TVGroup group, bool reloadgroups)
    {
      listViewTVChannelsInGroup.BeginUpdate();
      treeViewProviders.BeginUpdate();
      comboBoxTvChannelGroups.BeginUpdate();

      string selectedTvChannelGroup = comboBoxTvChannelGroups.Text;

      if (reloadgroups || comboBoxTvChannelGroups.Items.Count == 0)
      {
        comboBoxTvChannelGroups.Items.Clear();
        ArrayList groups = new ArrayList();
        TVDatabase.GetGroups(ref groups);
        foreach (TVGroup grp in groups)
        {
          comboBoxTvChannelGroups.Items.Add(grp);
        }

        if (comboBoxTvChannelGroups.Items.Count > 0)
        {
          comboBoxTvChannelGroups.SelectedIndex = 0;
          group = comboBoxTvChannelGroups.SelectedItem as TVGroup;
        }
      }

      ArrayList groupChannels = new ArrayList();
      listViewTVChannelsInGroup.Items.Clear();
      if (group != null)
      {
        foreach (TVChannel chan in group.TvChannels)
        {
          ListViewItem listItem = new ListViewItem(new string[] {chan.Name});
          listItem.Tag = chan;
          listViewTVChannelsInGroup.Items.Add(listItem);
          groupChannels.Add(chan);
        }
      }

      ArrayList existingProviders = new ArrayList();
      ArrayList expandedProviders = new ArrayList();
      foreach (TreeNode providerNode in treeViewProviders.Nodes)
      {
        existingProviders.Add(providerNode.Text);
        if (providerNode.IsExpanded)
        {
          expandedProviders.Add(providerNode.Text);
        }
      }

      //fill in treeview with provider/channels
      string lastProvider = "";
      TreeNode node = null;
      treeViewProviders.Nodes.Clear();
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannelsByProvider(ref channels);
      foreach (TVChannel chan in channels)
      {
        bool add = true;
        foreach (TVChannel grpChan in groupChannels)
        {
          if (grpChan.Name == chan.Name)
          {
            add = false;
            break;
          }
        }

        if (add)
        {
          if (lastProvider != chan.ProviderName)
          {
            lastProvider = chan.ProviderName;
            if (node != null)
            {
              treeViewProviders.Nodes.Add(node);
            }
            node = new TreeNode(chan.ProviderName);

            // was provider expanded?
            foreach (string providerName in expandedProviders)
            {
              if (node.Text == providerName)
              {
                node.Expand();
              }
            }

            if (_providerInit)
            {
              // do we have a new provider?
              bool providerExisted = false;
              foreach (string providerName in existingProviders)
              {
                if (node.Text == providerName)
                {
                  providerExisted = true;
                }
              }
              if (!providerExisted)
              {
                node.Expand();
              }
            }
            node.Tag = "";
          }
          TreeNode nodeChan = new TreeNode(chan.Name);
          nodeChan.Tag = chan;
          node.Nodes.Add(nodeChan);
        }
      }
      if (node != null && node.Nodes.Count > 0)
      {
        treeViewProviders.Nodes.Add(node);
      }

      comboBoxTvChannelGroups.Text = selectedTvChannelGroup;

      comboBoxTvChannelGroups.EndUpdate();
      listViewTVChannelsInGroup.EndUpdate();
      treeViewProviders.EndUpdate();

      _providerInit = true;
    }

    private void listViewTVChannelsInGroup_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      listViewTVChannelsInGroup.BeginUpdate();

      switch (listViewTVChannelsInGroup.Sorting)
      {
        case SortOrder.Ascending:
          listViewTVChannelsInGroup.Sorting = SortOrder.Descending;
          break;
        case SortOrder.Descending:
          listViewTVChannelsInGroup.Sorting = SortOrder.Ascending;
          break;
        case SortOrder.None:
          listViewTVChannelsInGroup.Sorting = SortOrder.Ascending;
          break;
      }

      if (e.Column == 1)
      {
        listViewTVChannelsInGroup.ListViewItemSorter = new ListViewItemComparerInt(e.Column);
      }
      else
      {
        listViewTVChannelsInGroup.ListViewItemSorter = new ListViewItemComparer(e.Column);
      }

      listViewTVChannelsInGroup.Sort();
      listViewTVChannelsInGroup.Update();

      listViewTVChannelsInGroup.EndUpdate();
    }

    private void buttonChannelUp_Click(object sender, EventArgs e)
    {
      listViewTVChannelsInGroup.BeginUpdate();

      _itemsModified = true;

      for (int index = 0; index < listViewTVChannelsInGroup.Items.Count; index++)
      {
        if (listViewTVChannelsInGroup.Items[index].Selected &&
            (index > 0)) // Make sure the current index isn't smaller than the lowest index (0) in the list view
        {
          ListViewItem listItem = listViewTVChannelsInGroup.Items[index];
          listViewTVChannelsInGroup.Items.RemoveAt(index);
          listViewTVChannelsInGroup.Items.Insert(index - 1, listItem);
        }
      }

      SaveTVGroupChannelsAndMapping();

      listViewTVChannelsInGroup.EndUpdate();
    }

    private void buttonChannelDown_Click(object sender, EventArgs e)
    {
      listViewTVChannelsInGroup.BeginUpdate();

      _itemsModified = true;

      for (int index = listViewTVChannelsInGroup.Items.Count - 1; index >= 0; index--)
      {
        if (listViewTVChannelsInGroup.Items[index].Selected &&
            (index < listViewTVChannelsInGroup.Items.Count - 1))
          // Make sure the current index isn't greater than the highest index in the list view
        {
          ListViewItem listItem = listViewTVChannelsInGroup.Items[index];
          listViewTVChannelsInGroup.Items.RemoveAt(index);

          if (index + 1 < listViewTVChannelsInGroup.Items.Count)
          {
            listViewTVChannelsInGroup.Items.Insert(index + 1, listItem);
          }
          else
          {
            listViewTVChannelsInGroup.Items.Add(listItem);
          }
        }
      }

      SaveTVGroupChannelsAndMapping();

      listViewTVChannelsInGroup.EndUpdate();
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      SaveSettings();
      LoadSettings();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      LoadSettings();
    }

    private void listViewGroups_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      listViewGroups.BeginUpdate();

      if (_columnSorter == null)
      {
        listViewGroups.ListViewItemSorter = _columnSorter = new ListViewColumnSorter();
      }

      // Determine if clicked column is already the column that is being sorted.
      if (e.Column == _columnSorter.SortColumn)
      {
        _columnSorter.Order = _columnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        _columnSorter.SortColumn = e.Column;
        _columnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      listViewGroups.Sort();

      listViewGroups.EndUpdate();
    }

    public void SaveTVGroupChannelsAndMapping()
    {
      TVGroup group = (TVGroup) comboBoxTvChannelGroups.SelectedItem;
      TVDatabase.DeleteChannelsFromGroup(group);
      group.TvChannels.Clear();
      for (int index = 0; index < listViewTVChannelsInGroup.Items.Count; index++)
      {
        //group.TvChannels.Clear();
        ListViewItem listItem = listViewTVChannelsInGroup.Items[index];
        group.TvChannels.Add((TVChannel) listItem.Tag);
        TVChannel ch = ((TVChannel) listItem.Tag).Clone();
        ch.Sort = index;
        TVDatabase.MapChannelToGroup(group, ch);
      }
    }

    private void listViewTVChannelsInGroup_DragDrop(object sender, DragEventArgs e)
    {
      _itemsModified = true;
      SaveTVGroupChannelsAndMapping();
    }

    private void treeViewProviders_DoubleClick(object sender, EventArgs e)
    {
      buttonMap_Click(sender, e);
    }

    private void listViewTVChannelsInGroup_DoubleClick(object sender, EventArgs e)
    {
      buttonUnmap_Click(sender, e);
    }

    private void listViewGroups_DoubleClick(object sender, EventArgs e)
    {
      if (listViewGroups.SelectedItems.Count > 0)
      {
        buttonEditGroup_Click(sender, e);
      }
      else
      {
        buttonAddGroup_Click(sender, e);
      }
    }

    private void listViewGroups_DragDrop(object sender, DragEventArgs e)
    {
      _itemsModified = true;
      SaveSettings();
    }
  }
}