#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml;
using MWCommon;
using MWControls;
using Microsoft.Win32;
using MediaPortal.Configuration.Controls;

using SQLite.NET;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using DShowNET;
using DirectShowLib;
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class TVGroups : MediaPortal.Configuration.SectionSettings
  {
    public class ComboCard
    {
      public string FriendlyName;
      public string VideoDevice;
      public int ID;
      public override string ToString()
      {
        return String.Format("{0} - {1}", FriendlyName, VideoDevice);
      }
    };
    private System.ComponentModel.IContainer components = null;
    static bool reloadList = false;
    private System.Windows.Forms.OpenFileDialog XMLOpenDialog;
    private System.Windows.Forms.SaveFileDialog XMLSaveDialog;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage2;
    private MediaPortal.UserInterface.Controls.MPButton btnGroupDown;
    private MediaPortal.UserInterface.Controls.MPButton buttonGroupUp;
    private MediaPortal.UserInterface.Controls.MPButton buttonEditGroup;
    private MediaPortal.UserInterface.Controls.MPButton buttonDeleteGroup;
    private MediaPortal.UserInterface.Controls.MPButton buttonAddGroup;
    private System.Windows.Forms.ListView listViewGroups;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage3;
    private MWControls.MWTreeView treeViewChannels;
    private MediaPortal.UserInterface.Controls.MPButton btnGrpChnDown;
    private MediaPortal.UserInterface.Controls.MPButton btnGrpChnUp;
    private MediaPortal.UserInterface.Controls.MPButton buttonMap;
    private MediaPortal.UserInterface.Controls.MPButton btnUnmap;
    private System.Windows.Forms.ListView listViewTVGroupChannels;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox1;
    private System.Windows.Forms.ColumnHeader columnHeader6;
    private System.Windows.Forms.ColumnHeader columnHeader7;
    private System.Windows.Forms.ColumnHeader columnHeader9;
    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    ListViewColumnSorter _columnSorter;

    //
    // Private members
    //
    bool isDirty = false;

    public TVGroups()
      : this("TV Channel Groups")
    {
    }

    public TVGroups(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      treeViewChannels.MultiSelect = TreeViewMultiSelect.MultiSameBranchAndLevel;
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
      this.components = new System.ComponentModel.Container();
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TVGroups));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.XMLOpenDialog = new System.Windows.Forms.OpenFileDialog();
      this.XMLSaveDialog = new System.Windows.Forms.SaveFileDialog();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage2 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.btnGroupDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonGroupUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonEditGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDeleteGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonAddGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewGroups = new System.Windows.Forms.ListView();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
      this.tabPage3 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.treeViewChannels = new MWControls.MWTreeView();
      this.btnGrpChnDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnGrpChnUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonMap = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnUnmap = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewTVGroupChannels = new System.Windows.Forms.ListView();
      this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.tabControl1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.SuspendLayout();
      // 
      // imageList1
      // 
      this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
      this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // XMLOpenDialog
      // 
      this.XMLOpenDialog.DefaultExt = "xml";
      this.XMLOpenDialog.FileName = "ChannelList";
      this.XMLOpenDialog.Filter = "xml|*.xml";
      this.XMLOpenDialog.InitialDirectory = ".";
      this.XMLOpenDialog.Title = "Open....";
      // 
      // XMLSaveDialog
      // 
      this.XMLSaveDialog.CreatePrompt = true;
      this.XMLSaveDialog.DefaultExt = "xml";
      this.XMLSaveDialog.FileName = "ChannelList";
      this.XMLSaveDialog.Filter = "xml|*.xml";
      this.XMLSaveDialog.InitialDirectory = ".";
      this.XMLSaveDialog.Title = "Save to....";
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPage2
      // 
      this.tabPage2.AutoScroll = true;
      this.tabPage2.Controls.Add(this.btnGroupDown);
      this.tabPage2.Controls.Add(this.buttonGroupUp);
      this.tabPage2.Controls.Add(this.buttonEditGroup);
      this.tabPage2.Controls.Add(this.buttonDeleteGroup);
      this.tabPage2.Controls.Add(this.buttonAddGroup);
      this.tabPage2.Controls.Add(this.listViewGroups);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(464, 382);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "TV Channel Groups";
      // 
      // btnGroupDown
      // 
      this.btnGroupDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnGroupDown.Location = new System.Drawing.Point(376, 348);
      this.btnGroupDown.Name = "btnGroupDown";
      this.btnGroupDown.Size = new System.Drawing.Size(72, 22);
      this.btnGroupDown.TabIndex = 5;
      this.btnGroupDown.Text = "Down";
      this.btnGroupDown.Click += new System.EventHandler(this.btnGroupDown_Click);
      // 
      // buttonGroupUp
      // 
      this.buttonGroupUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonGroupUp.Location = new System.Drawing.Point(296, 348);
      this.buttonGroupUp.Name = "buttonGroupUp";
      this.buttonGroupUp.Size = new System.Drawing.Size(72, 22);
      this.buttonGroupUp.TabIndex = 4;
      this.buttonGroupUp.Text = "Up";
      this.buttonGroupUp.Click += new System.EventHandler(this.buttonGroupUp_Click);
      // 
      // buttonEditGroup
      // 
      this.buttonEditGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonEditGroup.Location = new System.Drawing.Point(216, 348);
      this.buttonEditGroup.Name = "buttonEditGroup";
      this.buttonEditGroup.Size = new System.Drawing.Size(72, 22);
      this.buttonEditGroup.TabIndex = 3;
      this.buttonEditGroup.Text = "Edit";
      this.buttonEditGroup.Click += new System.EventHandler(this.buttonEditGroup_Click);
      // 
      // buttonDeleteGroup
      // 
      this.buttonDeleteGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDeleteGroup.Location = new System.Drawing.Point(136, 348);
      this.buttonDeleteGroup.Name = "buttonDeleteGroup";
      this.buttonDeleteGroup.Size = new System.Drawing.Size(72, 22);
      this.buttonDeleteGroup.TabIndex = 2;
      this.buttonDeleteGroup.Text = "Delete";
      this.buttonDeleteGroup.Click += new System.EventHandler(this.buttonDeleteGroup_Click);
      // 
      // buttonAddGroup
      // 
      this.buttonAddGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonAddGroup.Location = new System.Drawing.Point(56, 348);
      this.buttonAddGroup.Name = "buttonAddGroup";
      this.buttonAddGroup.Size = new System.Drawing.Size(72, 22);
      this.buttonAddGroup.TabIndex = 1;
      this.buttonAddGroup.Text = "Add";
      this.buttonAddGroup.Click += new System.EventHandler(this.buttonAddGroup_Click);
      // 
      // listViewGroups
      // 
      this.listViewGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewGroups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							 this.columnHeader6,
																							 this.columnHeader7});
      this.listViewGroups.FullRowSelect = true;
      this.listViewGroups.HideSelection = false;
      this.listViewGroups.Location = new System.Drawing.Point(16, 16);
      this.listViewGroups.Name = "listViewGroups";
      this.listViewGroups.Size = new System.Drawing.Size(432, 320);
      this.listViewGroups.TabIndex = 0;
      this.listViewGroups.View = System.Windows.Forms.View.Details;
      this.listViewGroups.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewGroups_ColumnClick);
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "Group name";
      this.columnHeader6.Width = 346;
      // 
      // columnHeader7
      // 
      this.columnHeader7.Text = "Pincode";
      this.columnHeader7.Width = 64;
      // 
      // tabPage3
      // 
      this.tabPage3.AutoScroll = true;
      this.tabPage3.Controls.Add(this.listView1);
      this.tabPage3.Controls.Add(this.treeViewChannels);
      this.tabPage3.Controls.Add(this.btnGrpChnDown);
      this.tabPage3.Controls.Add(this.btnGrpChnUp);
      this.tabPage3.Controls.Add(this.buttonMap);
      this.tabPage3.Controls.Add(this.btnUnmap);
      this.tabPage3.Controls.Add(this.listViewTVGroupChannels);
      this.tabPage3.Controls.Add(this.label1);
      this.tabPage3.Controls.Add(this.comboBox1);
      this.tabPage3.Location = new System.Drawing.Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new System.Drawing.Size(464, 382);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Map Channels";
      this.tabPage3.Visible = false;
      // 
      // listView1
      // 
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1});
      this.listView1.FullRowSelect = true;
      this.listView1.HideSelection = false;
      this.listView1.Location = new System.Drawing.Point(16, 56);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(176, 21);
      this.listView1.TabIndex = 2;
      this.listView1.TabStop = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Available TV Groups";
      this.columnHeader1.Width = 154;
      // 
      // treeViewChannels
      // 
      this.treeViewChannels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
      this.treeViewChannels.FullRowSelect = true;
      this.treeViewChannels.ImageIndex = -1;
      this.treeViewChannels.Location = new System.Drawing.Point(16, 76);
      this.treeViewChannels.Name = "treeViewChannels";
      this.treeViewChannels.SelectedImageIndex = -1;
      this.treeViewChannels.Size = new System.Drawing.Size(176, 256);
      this.treeViewChannels.Sorted = true;
      this.treeViewChannels.TabIndex = 3;
      // 
      // btnGrpChnDown
      // 
      this.btnGrpChnDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnGrpChnDown.Location = new System.Drawing.Point(364, 344);
      this.btnGrpChnDown.Name = "btnGrpChnDown";
      this.btnGrpChnDown.Size = new System.Drawing.Size(84, 22);
      this.btnGrpChnDown.TabIndex = 8;
      this.btnGrpChnDown.Text = "Down";
      this.btnGrpChnDown.Click += new System.EventHandler(this.btnGrpChnDown_Click);
      // 
      // btnGrpChnUp
      // 
      this.btnGrpChnUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnGrpChnUp.Location = new System.Drawing.Point(272, 344);
      this.btnGrpChnUp.Name = "btnGrpChnUp";
      this.btnGrpChnUp.Size = new System.Drawing.Size(80, 22);
      this.btnGrpChnUp.TabIndex = 7;
      this.btnGrpChnUp.Text = "Up";
      this.btnGrpChnUp.Click += new System.EventHandler(this.btnGrpChnUp_Click);
      // 
      // buttonMap
      // 
      this.buttonMap.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonMap.Location = new System.Drawing.Point(212, 168);
      this.buttonMap.Name = "buttonMap";
      this.buttonMap.Size = new System.Drawing.Size(40, 22);
      this.buttonMap.TabIndex = 4;
      this.buttonMap.Text = ">>";
      this.buttonMap.Click += new System.EventHandler(this.buttonMap_Click);
      // 
      // btnUnmap
      // 
      this.btnUnmap.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.btnUnmap.Location = new System.Drawing.Point(212, 200);
      this.btnUnmap.Name = "btnUnmap";
      this.btnUnmap.Size = new System.Drawing.Size(40, 22);
      this.btnUnmap.TabIndex = 5;
      this.btnUnmap.Text = "<<";
      this.btnUnmap.Click += new System.EventHandler(this.btnUnmap_Click);
      // 
      // listViewTVGroupChannels
      // 
      this.listViewTVGroupChannels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewTVGroupChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																									  this.columnHeader9});
      this.listViewTVGroupChannels.FullRowSelect = true;
      this.listViewTVGroupChannels.HideSelection = false;
      this.listViewTVGroupChannels.Location = new System.Drawing.Point(272, 56);
      this.listViewTVGroupChannels.Name = "listViewTVGroupChannels";
      this.listViewTVGroupChannels.Size = new System.Drawing.Size(176, 276);
      this.listViewTVGroupChannels.TabIndex = 6;
      this.listViewTVGroupChannels.View = System.Windows.Forms.View.Details;
      this.listViewTVGroupChannels.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewTVGroupChannels_ColumnClick);
      // 
      // columnHeader9
      // 
      this.columnHeader9.Text = "TV Channels in Group";
      this.columnHeader9.Width = 154;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(104, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "TV Channel Group:";
      // 
      // comboBox1
      // 
      this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.Location = new System.Drawing.Point(160, 20);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(288, 21);
      this.comboBox1.TabIndex = 1;
      this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // TVGroups
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "TVGroups";
      this.Size = new System.Drawing.Size(472, 408);
      this.Load += new System.EventHandler(this.TVGroups_Load);
      this.tabControl1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private string GetStandardName(AnalogVideoStandard standard)
    {
      string name = standard.ToString();
      name = name.Replace("_", " ");
      return name == "None" ? "Default" : name;
    }



    public override void LoadSettings()
    {
      LoadTVGroups();
      LoadGroups();
    }

    public override void SaveSettings()
    {
      if (reloadList)
      {
        LoadTVGroups();
        LoadGroups();
        reloadList = false;
        isDirty = true;
      }
      SaveTVGroups();
      SaveGroups();
    }

    private void SaveTVGroups()
    {
    }

    private void AddChannel(ref ArrayList channels, string strName, int iNumber)
    {
      isDirty = true;

      TVChannel channel = new TVChannel();
      channel.Number = iNumber;
      channel.Name = strName;
      channels.Add(channel);
    }

    /// <summary>
    /// 
    /// </summary>
    private void LoadTVGroups()
    {
    }





    string RemoveTrailingSlash(string strLine)
    {
      string strPath = strLine;
      while (strPath.Length > 0)
      {
        if (strPath[strPath.Length - 1] == '\\' || strPath[strPath.Length - 1] == '/')
        {
          strPath = strPath.Substring(0, strPath.Length - 1);
        }
        else break;
      }
      return strPath;
    }



    static public void UpdateList()
    {
      reloadList = true;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
      if (reloadList)
      {
        reloadList = false;
        LoadTVGroups();
        LoadGroups();
      }
      base.OnPaint(e);
    }

    public void LoadGroups()
    {
      listViewGroups.Items.Clear();
      ArrayList groups = new ArrayList();
      TVDatabase.GetGroups(ref groups);
      foreach (TVGroup group in groups)
      {
        string pincode = "No";
        if (group.Pincode != 0)
          pincode = "Yes";
        ListViewItem listItem = new ListViewItem(new string[] { group.GroupName, pincode, });
        listItem.Tag = group;
        listViewGroups.Items.Add(listItem);

      }
      UpdateGroupChannels(null, true);
    }

    private void buttonEditGroup_Click(object sender, System.EventArgs e)
    {
      isDirty = true;

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
            pincode = "Yes";

          listItem.SubItems[0].Text = group.GroupName;
          listItem.SubItems[1].Text = pincode;
          TVDatabase.AddGroup(group);

          SaveTVGroups();
          SaveGroups();
          UpdateGroupChannels(group, true);
        }
      }
    }

    private void buttonDeleteGroup_Click(object sender, System.EventArgs e)
    {

      int itemCount = listViewGroups.SelectedItems.Count;

      for (int index = 0; index < itemCount; index++)
      {
        isDirty = true;
        ListViewItem item = listViewGroups.SelectedItems[0];
        TVGroup group = item.Tag as TVGroup;
        if (group != null) TVDatabase.DeleteGroup(group);
        listViewGroups.Items.RemoveAt(listViewGroups.SelectedIndices[0]);
      }

      SaveTVGroups();
      SaveGroups();
      UpdateGroupChannels(null, true);
    }

    private void buttonAddGroup_Click(object sender, System.EventArgs e)
    {

      EditGroupForm editGroup = new EditGroupForm();
      DialogResult dialogResult = editGroup.ShowDialog(this);
      if (dialogResult == DialogResult.OK)
      {
        isDirty = true;
        TVGroup group = editGroup.Group;
        string pincode = "No";
        if (group.Pincode != 0)
          pincode = "Yes";
        ListViewItem listItem = new ListViewItem(new string[] { group.GroupName, pincode, });
        listItem.Tag = group;
        listViewGroups.Items.Add(listItem);

        SaveGroups();
        LoadGroups();

        SaveTVGroups();
        UpdateGroupChannels(group, true);

      }

    }

    private void buttonGroupUp_Click(object sender, System.EventArgs e)
    {
      isDirty = true;

      for (int index = 0; index < listViewGroups.Items.Count; index++)
      {
        if (listViewGroups.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't smaller than the lowest index (0) in the list view
          //
          if (index > 0)
          {
            ListViewItem listItem = listViewGroups.Items[index];
            listViewGroups.Items.RemoveAt(index);
            listViewGroups.Items.Insert(index - 1, listItem);
          }
        }
      }
    }

    private void btnGroupDown_Click(object sender, System.EventArgs e)
    {
      isDirty = true;

      for (int index = listViewGroups.Items.Count - 1; index >= 0; index--)
      {
        if (listViewGroups.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't greater than the highest index in the list view
          //
          if (index < listViewGroups.Items.Count - 1)
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
      }
    }

    private void SaveGroups()
    {
      if (isDirty == true)
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

    private void buttonMap_Click(object sender, System.EventArgs e)
    {
      if (treeViewChannels.SelNodes == null) return;
      Hashtable htSelNodes = treeViewChannels.SelNodes.Clone() as Hashtable;
      treeViewChannels.SelNodes = null;
      foreach (MWTreeNodeWrapper node in htSelNodes.Values)
      {
        TVChannel chan = node.Node.Tag as TVChannel;
        if (chan == null) return;
        ListViewItem listItem = new ListViewItem(new string[] { chan.Name });
        listItem.Tag = chan;
        listViewTVGroupChannels.Items.Add(listItem);
        treeViewChannels.Nodes.Remove(node.Node);
      }

      SaveTVGroupChannelsAndMapping();
    }

    private void btnUnmap_Click(object sender, System.EventArgs e)
    {
      if (listViewTVGroupChannels.SelectedItems == null) return;
      for (int i = 0; i < listViewTVGroupChannels.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewTVGroupChannels.SelectedItems[i];
        TVChannel chan = (TVChannel)listItem.Tag;

        foreach (TreeNode node in treeViewChannels.Nodes)
        {
          if (node.Text == chan.ProviderName)
          {
            TreeNode subnode = new TreeNode(chan.Name);
            subnode.Tag = chan;
            node.Nodes.Add(subnode);
          }
        }
      }
      TVGroup group = comboBox1.SelectedItem as TVGroup;
      for (int i = listViewTVGroupChannels.SelectedItems.Count - 1; i >= 0; --i)
      {
        ListViewItem listItem = listViewTVGroupChannels.SelectedItems[i];
        TVChannel channel = listItem.Tag as TVChannel;
        if (group != null && channel != null)
          TVDatabase.UnmapChannelFromGroup(group, channel);
        listViewTVGroupChannels.Items.Remove(listItem);
      }
    }

    private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      TVGroup group = (TVGroup)comboBox1.SelectedItem;
      UpdateGroupChannels(group, false);
    }

    void UpdateGroupChannels(TVGroup group, bool reloadgroups)
    {

      if (reloadgroups || comboBox1.Items.Count == 0)
      {
        comboBox1.Items.Clear();
        ArrayList groups = new ArrayList();
        TVDatabase.GetGroups(ref groups);
        foreach (TVGroup grp in groups)
        {
          comboBox1.Items.Add(grp);
        }
        if (comboBox1.Items.Count > 0)
        {
          comboBox1.SelectedIndex = 0;
          group = comboBox1.SelectedItem as TVGroup;
        }
      }

      ArrayList groupChannels = new ArrayList();
      listViewTVGroupChannels.Items.Clear();
      if (group != null)
      {
        foreach (TVChannel chan in group.TvChannels)
        {
          ListViewItem listItem = new ListViewItem(new string[] { chan.Name });
          listItem.Tag = chan;
          listViewTVGroupChannels.Items.Add(listItem);
          groupChannels.Add(chan);
        }
      }

      //fill in treeview with provider/channels
      string lastProvider = "";
      TreeNode node = null;
      treeViewChannels.Nodes.Clear();
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
              treeViewChannels.Nodes.Add(node);
            node = new TreeNode(chan.ProviderName);
            node.Tag = "";
          }
          TreeNode nodeChan = new TreeNode(chan.Name);
          nodeChan.Tag = chan;
          node.Nodes.Add(nodeChan);
        }
      }
      if (node != null && node.Nodes.Count > 0)
        treeViewChannels.Nodes.Add(node);
    }

    private void listViewTVGroupChannels_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
    {
      switch (listViewTVGroupChannels.Sorting)
      {
        case SortOrder.Ascending: listViewTVGroupChannels.Sorting = SortOrder.Descending; break;
        case SortOrder.Descending: listViewTVGroupChannels.Sorting = SortOrder.Ascending; break;
        case SortOrder.None: listViewTVGroupChannels.Sorting = SortOrder.Ascending; break;
      }

      if (e.Column == 1)
        listViewTVGroupChannels.ListViewItemSorter = new ListViewItemComparerInt(e.Column);
      else
        listViewTVGroupChannels.ListViewItemSorter = new ListViewItemComparer(e.Column);

      listViewTVGroupChannels.Sort();
      listViewTVGroupChannels.Update();

    }


    private void btnGrpChnUp_Click(object sender, System.EventArgs e)
    {
      isDirty = true;

      for (int index = 0; index < listViewTVGroupChannels.Items.Count; index++)
      {
        if (listViewTVGroupChannels.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't smaller than the lowest index (0) in the list view
          //
          if (index > 0)
          {
            ListViewItem listItem = listViewTVGroupChannels.Items[index];
            listViewTVGroupChannels.Items.RemoveAt(index);
            listViewTVGroupChannels.Items.Insert(index - 1, listItem);
          }
        }
      }
      
      SaveTVGroupChannelsAndMapping();
    }

    private void btnGrpChnDown_Click(object sender, System.EventArgs e)
    {
      isDirty = true;

      for (int index = listViewTVGroupChannels.Items.Count - 1; index >= 0; index--)
      {
        if (listViewTVGroupChannels.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't greater than the highest index in the list view
          //
          if (index < listViewTVGroupChannels.Items.Count - 1)
          {
            ListViewItem listItem = listViewTVGroupChannels.Items[index];
            listViewTVGroupChannels.Items.RemoveAt(index);

            if (index + 1 < listViewTVGroupChannels.Items.Count)
            {
              listViewTVGroupChannels.Items.Insert(index + 1, listItem);
            }
            else
            {
              listViewTVGroupChannels.Items.Add(listItem);
            }
          }
        }
      }

      SaveTVGroupChannelsAndMapping();
    }


    private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SaveSettings();
      LoadSettings();
    }

    private void TVGroups_Load(object sender, System.EventArgs e)
    {

    }
    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      LoadSettings();
    }

    private void listViewGroups_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
    {
      if (_columnSorter == null)
        listViewGroups.ListViewItemSorter = _columnSorter = new ListViewColumnSorter();

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
    }

    public void SaveTVGroupChannelsAndMapping()
    {
      TVGroup group = (TVGroup)comboBox1.SelectedItem;
      TVDatabase.DeleteChannelsFromGroup(group);
      group.TvChannels.Clear();
      for (int index = 0; index < listViewTVGroupChannels.Items.Count; index++)
      {
        //group.TvChannels.Clear();
        ListViewItem listItem = listViewTVGroupChannels.Items[index];
        group.TvChannels.Add((TVChannel)listItem.Tag);
        TVChannel ch = ((TVChannel)listItem.Tag).Clone();
        ch.Sort = index;
        TVDatabase.MapChannelToGroup(group, ch);
      }
    }

  }


}

