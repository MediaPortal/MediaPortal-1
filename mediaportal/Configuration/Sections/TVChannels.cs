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
using System.Collections.Generic;
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

namespace MediaPortal.Configuration.Sections
{
  public class TVChannels : MediaPortal.Configuration.SectionSettings
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
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPButton buttonLookup;
    private MediaPortal.UserInterface.Controls.MPButton xmlImport;
    private MediaPortal.UserInterface.Controls.MPButton xmlExport;
    private MediaPortal.UserInterface.Controls.MPButton buttonCVS;
    private MediaPortal.UserInterface.Controls.MPListView channelsListView;
    private MediaPortal.UserInterface.Controls.MPButton btnImport;
    private MediaPortal.UserInterface.Controls.MPButton btnClear;
    private MediaPortal.UserInterface.Controls.MPButton addButton;
    private MediaPortal.UserInterface.Controls.MPButton deleteButton;
    private MediaPortal.UserInterface.Controls.MPButton editButton;
    private MediaPortal.UserInterface.Controls.MPButton upButton;
    private MediaPortal.UserInterface.Controls.MPButton downButton;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage4;
    private MediaPortal.UserInterface.Controls.MPButton btnMapChannelToCard;
    private MediaPortal.UserInterface.Controls.MPButton btnUnmapChannelFromCard;
    private System.Windows.Forms.ListView listViewTVChannelsForCard;
    private System.Windows.Forms.ListView listViewTVChannelsCard;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxCard;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.ColumnHeader columnHeader10;
    private System.Windows.Forms.ColumnHeader columnHeader11;
    ListViewColumnSorter _columnSorter;
    private MediaPortal.UserInterface.Controls.MPButton buttonCombine;

    //
    // Private members
    //
    bool isDirty = false;

    public TVChannels()
      : this("TV Channels")
    {
    }

    public TVChannels(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TVChannels));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.XMLOpenDialog = new System.Windows.Forms.OpenFileDialog();
      this.XMLSaveDialog = new System.Windows.Forms.SaveFileDialog();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.buttonCombine = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonLookup = new MediaPortal.UserInterface.Controls.MPButton();
      this.xmlImport = new MediaPortal.UserInterface.Controls.MPButton();
      this.xmlExport = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonCVS = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnImport = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnClear = new MediaPortal.UserInterface.Controls.MPButton();
      this.addButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.deleteButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.editButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.upButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.downButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.channelsListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.tabPage4 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.btnMapChannelToCard = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnUnmapChannelFromCard = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewTVChannelsForCard = new System.Windows.Forms.ListView();
      this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
      this.listViewTVChannelsCard = new System.Windows.Forms.ListView();
      this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxCard = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage4.SuspendLayout();
      this.SuspendLayout();
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "");
      this.imageList1.Images.SetKeyName(1, "");
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
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage4);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.TabIndex = 0;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.AutoScroll = true;
      this.tabPage1.Controls.Add(this.buttonCombine);
      this.tabPage1.Controls.Add(this.buttonLookup);
      this.tabPage1.Controls.Add(this.xmlImport);
      this.tabPage1.Controls.Add(this.xmlExport);
      this.tabPage1.Controls.Add(this.buttonCVS);
      this.tabPage1.Controls.Add(this.btnImport);
      this.tabPage1.Controls.Add(this.btnClear);
      this.tabPage1.Controls.Add(this.addButton);
      this.tabPage1.Controls.Add(this.deleteButton);
      this.tabPage1.Controls.Add(this.editButton);
      this.tabPage1.Controls.Add(this.upButton);
      this.tabPage1.Controls.Add(this.downButton);
      this.tabPage1.Controls.Add(this.channelsListView);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(464, 382);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "TV Channels";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // buttonCombine
      // 
      this.buttonCombine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCombine.Location = new System.Drawing.Point(16, 312);
      this.buttonCombine.Name = "buttonCombine";
      this.buttonCombine.Size = new System.Drawing.Size(64, 20);
      this.buttonCombine.TabIndex = 12;
      this.buttonCombine.Text = "Combine";
      this.buttonCombine.UseVisualStyleBackColor = true;
      this.buttonCombine.Click += new System.EventHandler(this.buttonCombine_Click);
      // 
      // buttonLookup
      // 
      this.buttonLookup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonLookup.Location = new System.Drawing.Point(232, 353);
      this.buttonLookup.Name = "buttonLookup";
      this.buttonLookup.Size = new System.Drawing.Size(64, 20);
      this.buttonLookup.TabIndex = 7;
      this.buttonLookup.Text = "Lookup";
      this.buttonLookup.UseVisualStyleBackColor = true;
      this.buttonLookup.Click += new System.EventHandler(this.buttonLookup_Click);
      // 
      // xmlImport
      // 
      this.xmlImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.xmlImport.Location = new System.Drawing.Point(308, 353);
      this.xmlImport.Name = "xmlImport";
      this.xmlImport.Size = new System.Drawing.Size(64, 20);
      this.xmlImport.TabIndex = 10;
      this.xmlImport.Text = "Restore";
      this.xmlImport.UseVisualStyleBackColor = true;
      this.xmlImport.Click += new System.EventHandler(this.xmlImport_Click);
      // 
      // xmlExport
      // 
      this.xmlExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.xmlExport.Location = new System.Drawing.Point(384, 353);
      this.xmlExport.Name = "xmlExport";
      this.xmlExport.Size = new System.Drawing.Size(64, 20);
      this.xmlExport.TabIndex = 11;
      this.xmlExport.Text = "Backup";
      this.xmlExport.UseVisualStyleBackColor = true;
      this.xmlExport.Click += new System.EventHandler(this.xmlExport_Click);
      // 
      // buttonCVS
      // 
      this.buttonCVS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCVS.Location = new System.Drawing.Point(344, 332);
      this.buttonCVS.Name = "buttonCVS";
      this.buttonCVS.Size = new System.Drawing.Size(104, 20);
      this.buttonCVS.TabIndex = 8;
      this.buttonCVS.Text = "Add CVBS/SVHS";
      this.buttonCVS.UseVisualStyleBackColor = true;
      this.buttonCVS.Click += new System.EventHandler(this.buttonCVS_Click);
      // 
      // btnImport
      // 
      this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnImport.Location = new System.Drawing.Point(232, 332);
      this.btnImport.Name = "btnImport";
      this.btnImport.Size = new System.Drawing.Size(110, 20);
      this.btnImport.TabIndex = 9;
      this.btnImport.Text = "Import from tvguide";
      this.btnImport.UseVisualStyleBackColor = true;
      this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
      // 
      // btnClear
      // 
      this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClear.Location = new System.Drawing.Point(16, 353);
      this.btnClear.Name = "btnClear";
      this.btnClear.Size = new System.Drawing.Size(64, 20);
      this.btnClear.TabIndex = 4;
      this.btnClear.Text = "Clear";
      this.btnClear.UseVisualStyleBackColor = true;
      this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
      // 
      // addButton
      // 
      this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.addButton.Location = new System.Drawing.Point(16, 332);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(64, 20);
      this.addButton.TabIndex = 1;
      this.addButton.Text = "Add";
      this.addButton.UseVisualStyleBackColor = true;
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // deleteButton
      // 
      this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.deleteButton.Enabled = false;
      this.deleteButton.Location = new System.Drawing.Point(156, 332);
      this.deleteButton.Name = "deleteButton";
      this.deleteButton.Size = new System.Drawing.Size(64, 20);
      this.deleteButton.TabIndex = 3;
      this.deleteButton.Text = "Delete";
      this.deleteButton.UseVisualStyleBackColor = true;
      this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
      // 
      // editButton
      // 
      this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.editButton.Enabled = false;
      this.editButton.Location = new System.Drawing.Point(86, 332);
      this.editButton.Name = "editButton";
      this.editButton.Size = new System.Drawing.Size(64, 20);
      this.editButton.TabIndex = 2;
      this.editButton.Text = "Edit";
      this.editButton.UseVisualStyleBackColor = true;
      this.editButton.Click += new System.EventHandler(this.editButton_Click);
      // 
      // upButton
      // 
      this.upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.upButton.Enabled = false;
      this.upButton.Location = new System.Drawing.Point(86, 353);
      this.upButton.Name = "upButton";
      this.upButton.Size = new System.Drawing.Size(64, 20);
      this.upButton.TabIndex = 5;
      this.upButton.Text = "Up";
      this.upButton.UseVisualStyleBackColor = true;
      this.upButton.Click += new System.EventHandler(this.upButton_Click);
      // 
      // downButton
      // 
      this.downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.downButton.Enabled = false;
      this.downButton.Location = new System.Drawing.Point(156, 353);
      this.downButton.Name = "downButton";
      this.downButton.Size = new System.Drawing.Size(64, 20);
      this.downButton.TabIndex = 6;
      this.downButton.Text = "Down";
      this.downButton.UseVisualStyleBackColor = true;
      this.downButton.Click += new System.EventHandler(this.downButton_Click);
      // 
      // channelsListView
      // 
      this.channelsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.channelsListView.CheckBoxes = true;
      this.channelsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader5,
            this.columnHeader4});
      this.channelsListView.FullRowSelect = true;
      this.channelsListView.HideSelection = false;
      this.channelsListView.Location = new System.Drawing.Point(16, 16);
      this.channelsListView.Name = "channelsListView";
      this.channelsListView.Size = new System.Drawing.Size(432, 294);
      this.channelsListView.SmallImageList = this.imageList1;
      this.channelsListView.TabIndex = 0;
      this.channelsListView.UseCompatibleStateImageBehavior = false;
      this.channelsListView.View = System.Windows.Forms.View.Details;
      this.channelsListView.DoubleClick += new System.EventHandler(this.channelsListView_DoubleClick);
      this.channelsListView.SelectedIndexChanged += new System.EventHandler(this.channelsListView_SelectedIndexChanged);
      this.channelsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.channelsListView_ItemCheck);
      this.channelsListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.channelsListView_ColumnClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Channel name";
      this.columnHeader1.Width = 236;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Channel";
      this.columnHeader2.Width = 64;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Standard";
      this.columnHeader5.Width = 64;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Type";
      this.columnHeader4.Width = 64;
      // 
      // tabPage4
      // 
      this.tabPage4.AutoScroll = true;
      this.tabPage4.Controls.Add(this.btnMapChannelToCard);
      this.tabPage4.Controls.Add(this.btnUnmapChannelFromCard);
      this.tabPage4.Controls.Add(this.listViewTVChannelsForCard);
      this.tabPage4.Controls.Add(this.listViewTVChannelsCard);
      this.tabPage4.Controls.Add(this.label6);
      this.tabPage4.Controls.Add(this.comboBoxCard);
      this.tabPage4.Location = new System.Drawing.Point(4, 22);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new System.Drawing.Size(464, 382);
      this.tabPage4.TabIndex = 3;
      this.tabPage4.Text = "TV Cards";
      this.tabPage4.UseVisualStyleBackColor = true;
      this.tabPage4.Visible = false;
      // 
      // btnMapChannelToCard
      // 
      this.btnMapChannelToCard.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.btnMapChannelToCard.Location = new System.Drawing.Point(212, 168);
      this.btnMapChannelToCard.Name = "btnMapChannelToCard";
      this.btnMapChannelToCard.Size = new System.Drawing.Size(40, 22);
      this.btnMapChannelToCard.TabIndex = 3;
      this.btnMapChannelToCard.Text = ">>";
      this.btnMapChannelToCard.UseVisualStyleBackColor = true;
      this.btnMapChannelToCard.Click += new System.EventHandler(this.btnMapChannelToCard_Click);
      // 
      // btnUnmapChannelFromCard
      // 
      this.btnUnmapChannelFromCard.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.btnUnmapChannelFromCard.Location = new System.Drawing.Point(212, 200);
      this.btnUnmapChannelFromCard.Name = "btnUnmapChannelFromCard";
      this.btnUnmapChannelFromCard.Size = new System.Drawing.Size(40, 22);
      this.btnUnmapChannelFromCard.TabIndex = 4;
      this.btnUnmapChannelFromCard.Text = "<<";
      this.btnUnmapChannelFromCard.UseVisualStyleBackColor = true;
      this.btnUnmapChannelFromCard.Click += new System.EventHandler(this.btnUnmapChannelFromCard_Click);
      // 
      // listViewTVChannelsForCard
      // 
      this.listViewTVChannelsForCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewTVChannelsForCard.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader10});
      this.listViewTVChannelsForCard.Location = new System.Drawing.Point(272, 56);
      this.listViewTVChannelsForCard.Name = "listViewTVChannelsForCard";
      this.listViewTVChannelsForCard.Size = new System.Drawing.Size(176, 304);
      this.listViewTVChannelsForCard.TabIndex = 5;
      this.listViewTVChannelsForCard.UseCompatibleStateImageBehavior = false;
      this.listViewTVChannelsForCard.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader10
      // 
      this.columnHeader10.Text = "Assigned TV Channels";
      this.columnHeader10.Width = 154;
      // 
      // listViewTVChannelsCard
      // 
      this.listViewTVChannelsCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewTVChannelsCard.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader11});
      this.listViewTVChannelsCard.Location = new System.Drawing.Point(16, 56);
      this.listViewTVChannelsCard.Name = "listViewTVChannelsCard";
      this.listViewTVChannelsCard.Size = new System.Drawing.Size(176, 304);
      this.listViewTVChannelsCard.TabIndex = 2;
      this.listViewTVChannelsCard.UseCompatibleStateImageBehavior = false;
      this.listViewTVChannelsCard.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader11
      // 
      this.columnHeader11.Text = "Available TV Channels";
      this.columnHeader11.Width = 154;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 24);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(136, 16);
      this.label6.TabIndex = 0;
      this.label6.Text = "Map channels to TV card:";
      // 
      // comboBoxCard
      // 
      this.comboBoxCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxCard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCard.Location = new System.Drawing.Point(160, 20);
      this.comboBoxCard.Name = "comboBoxCard";
      this.comboBoxCard.Size = new System.Drawing.Size(288, 21);
      this.comboBoxCard.TabIndex = 1;
      this.comboBoxCard.SelectedIndexChanged += new System.EventHandler(this.comboBoxCard_SelectedIndexChanged);
      // 
      // TVChannels
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "TVChannels";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage4.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion


    private void addButton_Click(object sender, System.EventArgs e)
    {
      isDirty = true;

      EditTVChannelForm editChannel = new EditTVChannelForm();

      editChannel.SortingPlace = channelsListView.Items.Count;
      DialogResult dialogResult = editChannel.ShowDialog(this);

      if (dialogResult == DialogResult.OK)
      {
        LoadSettings();
      }
    }

    private string GetStandardName(AnalogVideoStandard standard)
    {
      string name = standard.ToString();
      name = name.Replace("_", " ");
      return name == "None" ? "Default" : name;
    }

    private void editButton_Click(object sender, System.EventArgs e)
    {
      isDirty = true;

      foreach (ListViewItem listItem in channelsListView.SelectedItems)
      {
        EditTVChannelForm editChannel = new EditTVChannelForm();
        editChannel.Channel = listItem.Tag as TelevisionChannel;
        editChannel.SortingPlace = listItem.Index; ;

        DialogResult dialogResult = editChannel.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          TelevisionChannel editedChannel = editChannel.Channel;
          listItem.Tag = editedChannel;

          listItem.SubItems[0].Text = editedChannel.Name;
          listItem.SubItems[1].Text = editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString();
          listItem.SubItems[2].Text = GetStandardName(editedChannel.standard);
          listItem.SubItems[3].Text = editedChannel.External ? "External" : "Internal";
          listItem.ImageIndex = 0;
          if (editedChannel.Scrambled)
            listItem.ImageIndex = 1;

          SaveSettings();
        }
      }
    }

    private void deleteButton_Click(object sender, System.EventArgs e)
    {
      isDirty = true;

      int itemCount = channelsListView.SelectedItems.Count;

      for (int index = 0; index < itemCount; index++)
      {
        channelsListView.Items.RemoveAt(channelsListView.SelectedIndices[0]);
      }
      SaveSettings();
    }

    public override object GetSetting(string name)
    {
      switch (name)
      {
        case "channel.highest":
          return HighestChannelNumber;
      }

      return null;
    }


    private int HighestChannelNumber
    {
      get
      {
        int highestChannelNumber = 0;

        foreach (ListViewItem item in channelsListView.Items)
        {
          TelevisionChannel channel = item.Tag as TelevisionChannel;

          if (channel != null)
          {
            if (channel.Channel < (int)ExternalInputs.svhs && channel.Channel > highestChannelNumber)
              highestChannelNumber = channel.Channel;
          }
        }

        return highestChannelNumber;
      }
    }

    public override void LoadSettings()
    {
      LoadTVChannels();
      LoadCards();
    }

    public override void SaveSettings()
    {
      if (reloadList)
      {
        LoadTVChannels();
        LoadCards();
        reloadList = false;
        isDirty = true;
      }
      SaveTVChannels();
    }

    private void SaveTVChannels()
    {
      if (isDirty == true)
      {
          int countryCode = 31;
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          {
              countryCode = xmlreader.GetValueAsInt("capture", "country", 31);
          }


          RegistryKey registryKey = Registry.LocalMachine;

          string[] registryLocations = new string[] { String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-1", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-0", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-1"),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-0")
															  };

          //
          // Start by removing any old tv channels from the database and from the registry.
          // Information stored in the registry is the channel frequency.
          //
          ArrayList channels = new ArrayList();
          TVDatabase.GetChannels(ref channels);

          if (channels != null && channels.Count > 0)
          {
            foreach (MediaPortal.TV.Database.TVChannel channel in channels)
            {
              bool found = false;
              foreach (ListViewItem listItem in channelsListView.Items)
              {
                TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;
                if (channel.Name.ToLower() == tvChannel.Name.ToLower())
                {
                  found = true;
                  break;
                }
              }
              if (!found)
                TVDatabase.RemoveChannel(channel.Name);
            }

            //
            // Remove channel frequencies from the registry
            //
            for (int index = 0; index < registryLocations.Length; index++)
            {
              registryKey = Registry.LocalMachine;
              registryKey = registryKey.CreateSubKey(registryLocations[index]);

              for (int channelIndex = 0; channelIndex < 200; channelIndex++)
              {
                registryKey.DeleteValue(channelIndex.ToString(), false);
              }

              registryKey.Close();
            }
          }

          //
          // Add current channels
          //
          TVDatabase.GetChannels(ref channels);
          foreach (ListViewItem listItem in channelsListView.Items)
          {
            MediaPortal.TV.Database.TVChannel channel = new MediaPortal.TV.Database.TVChannel();
            TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;

            if (tvChannel != null)
            {
              channel.Name = tvChannel.Name;

              //does channel already exists in database?
              bool exists = false;
              foreach (TVChannel chan in channels)
              {
                if (String.Compare(chan.Name, channel.Name, true) == 0)
                {
                  exists = true;
                  channel = chan.Clone();
                  break;
                }
              }
              channel.Number = tvChannel.Channel;
              channel.VisibleInGuide = tvChannel.VisibleInGuide;
              channel.Country = tvChannel.Country;
              channel.ID = tvChannel.ID;

              //
              // Calculate frequency
              //
              if (tvChannel.Frequency.Herz < 1000)
                tvChannel.Frequency.Herz *= 1000000L;

              channel.Frequency = tvChannel.Frequency.Herz;

              channel.External = tvChannel.External;
              channel.ExternalTunerChannel = tvChannel.ExternalTunerChannel;
              channel.TVStandard = tvChannel.standard;
              channel.Scrambled = tvChannel.Scrambled;

              if (exists)
              {
                TVDatabase.UpdateChannel(channel, listItem.Index);
              }
              else
              {
                TVDatabase.AddChannel(channel);

                //
                // Set the sort order
                //
                TVDatabase.SetChannelSort(channel.Name, listItem.Index);
              }
            }
          }

          //
          // Add frequencies to the registry
          //
          for (int index = 0; index < registryLocations.Length; index++)
          {
            registryKey = Registry.LocalMachine;
            registryKey = registryKey.CreateSubKey(registryLocations[index]);

            foreach (ListViewItem listItem in channelsListView.Items)
            {
              TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;

              if (tvChannel != null)
              {
                //
                // Don't add frequency to the registry if it has no frequency or if we have the predefined
                // channels for Composite and SVIDEO
                //
                if (tvChannel.Frequency.Herz > 0 &&
                  tvChannel.Channel != (int)ExternalInputs.svhs &&
                  tvChannel.Channel != (int)ExternalInputs.cvbs1 &&
                  tvChannel.Channel != (int)ExternalInputs.cvbs2 &&
                  tvChannel.Channel != (int)ExternalInputs.rgb)
                {
                  registryKey.SetValue(tvChannel.Channel.ToString(), (int)tvChannel.Frequency.Herz);
                }
              }
            }

            registryKey.Close();
          }
        
      }
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
    private void LoadTVChannels()
    {
      channelsListView.Items.Clear();
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);

      foreach (TVChannel channel in channels)
      {
        TelevisionChannel tvChannel = new TelevisionChannel();

        tvChannel.ID = channel.ID;
        tvChannel.Channel = channel.Number;
        tvChannel.Name = channel.Name;
        tvChannel.Frequency = channel.Frequency;
        tvChannel.External = channel.External;
        tvChannel.ExternalTunerChannel = channel.ExternalTunerChannel;
        tvChannel.VisibleInGuide = channel.VisibleInGuide;
        tvChannel.Country = channel.Country;
        tvChannel.standard = channel.TVStandard;
        tvChannel.Scrambled = channel.Scrambled;
        ListViewItem listItem = new ListViewItem(new string[] { tvChannel.Name, 
																		tvChannel.External ? String.Format("{0}/{1}", tvChannel.Channel, tvChannel.ExternalTunerChannel) : tvChannel.Channel.ToString(),
                                    GetStandardName(tvChannel.standard),
                                    tvChannel.External ? "External" : "Internal"
																	  });

        listItem.Checked = tvChannel.VisibleInGuide;
        listItem.ImageIndex = 0;
        if (tvChannel.Scrambled)
          listItem.ImageIndex = 1;

        listItem.Tag = tvChannel;

        channelsListView.Items.Add(listItem);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void channelsListView_DoubleClick(object sender, System.EventArgs e)
    {
      editButton_Click(sender, e);
    }

    private void MoveSelectionDown()
    {
      isDirty = true;

      for (int index = channelsListView.Items.Count - 1; index >= 0; index--)
      {
        if (channelsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't greater than the highest index in the list view
          //
          if (index < channelsListView.Items.Count - 1)
          {
            ListViewItem listItem = channelsListView.Items[index];
            channelsListView.Items.RemoveAt(index);

            if (index + 1 < channelsListView.Items.Count)
            {
              channelsListView.Items.Insert(index + 1, listItem);
            }
            else
            {
              channelsListView.Items.Add(listItem);
            }
          }
        }
      }
    }

    private void MoveSelectionUp()
    {
      isDirty = true;

      for (int index = 0; index < channelsListView.Items.Count; index++)
      {
        if (channelsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't smaller than the lowest index (0) in the list view
          //
          if (index > 0)
          {
            ListViewItem listItem = channelsListView.Items[index];
            channelsListView.Items.RemoveAt(index);
            channelsListView.Items.Insert(index - 1, listItem);
          }
        }
      }
    }

    private void upButton_Click(object sender, System.EventArgs e)
    {
      MoveSelectionUp();
    }

    private void downButton_Click(object sender, System.EventArgs e)
    {
      MoveSelectionDown();
    }

    private void channelsListView_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      deleteButton.Enabled = editButton.Enabled = upButton.Enabled = downButton.Enabled = (channelsListView.SelectedItems.Count > 0);
      buttonCombine.Enabled = (channelsListView.SelectedItems.Count == 2);

    }

    private void channelsListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
    {
      isDirty = true;

      //
      // Fetch checked item
      //
      if (e.Index < channelsListView.Items.Count)
      {
        TelevisionChannel tvChannel = channelsListView.Items[e.Index].Tag as TelevisionChannel;

        tvChannel.VisibleInGuide = (e.NewValue == System.Windows.Forms.CheckState.Checked);

        channelsListView.Items[e.Index].Tag = tvChannel;
      }
    }

    private void btnImport_Click(object sender, System.EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        string strTVGuideFile = xmlreader.GetValueAsString("xmltv", "folder", "xmltv");
        strTVGuideFile = RemoveTrailingSlash(strTVGuideFile);
        strTVGuideFile += @"\tvguide.xml";
        XMLTVImport import = new XMLTVImport();
        bool bSucceeded = import.Import(strTVGuideFile, true);
        if (bSucceeded)
        {
          string strtext = String.Format("Imported:{0} channels\r{1} programs\r{2}",
                                import.ImportStats.Channels,
                                import.ImportStats.Programs,
                                import.ImportStats.Status);
          MessageBox.Show(this, strtext, "tvguide", MessageBoxButtons.OK, MessageBoxIcon.Error);

          isDirty = true;
          LoadTVChannels();
        }
        else
        {
          string strError = String.Format("Error importing tvguide from:\r{0}\rerror:{1}",
                              strTVGuideFile, import.ImportStats.Status);
          MessageBox.Show(this, strError, "Error importing tvguide", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
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

    private void btnClear_Click(object sender, System.EventArgs e)
    {
      DialogResult result = MessageBox.Show(this, "Are you sure you want to delete all channels?", "Delete channels", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (result != DialogResult.Yes) return;
      channelsListView.Items.Clear();
      SaveSettings();
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
        LoadTVChannels();
        LoadCards();
      }
      base.OnPaint(e);
    }




    private void channelsListView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
    {
      if (_columnSorter == null)
        channelsListView.ListViewItemSorter = _columnSorter = new ListViewColumnSorter();

      _columnSorter.IsColumnNumeric = e.Column == 1;

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
      channelsListView.Sort();
      channelsListView.Update();
    }

    private void buttonCVS_Click(object sender, System.EventArgs e)
    {
      SaveSettings();
      TVChannel chan = new TVChannel();
      chan.Name = "CVBS#1"; chan.Number = (int)ExternalInputs.cvbs1; TVDatabase.AddChannel(chan);
      chan.Name = "CVBS#2"; chan.Number = (int)ExternalInputs.cvbs2; TVDatabase.AddChannel(chan);
      chan.Name = "SVHS"; chan.Number = (int)ExternalInputs.svhs; TVDatabase.AddChannel(chan);
      chan.Name = "RGB"; chan.Number = (int)ExternalInputs.rgb; TVDatabase.AddChannel(chan);
      LoadSettings();
    }

    void LoadCards()
    {
      comboBoxCard.Items.Clear();
      if (File.Exists("capturecards.xml"))
      {
        using (FileStream fileStream = new FileStream("capturecards.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          try
          {
            //
            // Create Soap Formatter
            //
            SoapFormatter formatter = new SoapFormatter();

            //
            // Serialize
            //
            ArrayList captureCards = new ArrayList();
            captureCards = (ArrayList)formatter.Deserialize(fileStream);
            for (int i = 0; i < captureCards.Count; i++)
            {
              ((TVCaptureDevice)captureCards[i]).ID = (i + 1);
              ((TVCaptureDevice)captureCards[i]).LoadDefinitions();


              TVCaptureDevice device = (TVCaptureDevice)captureCards[i];
              ComboCard combo = new ComboCard();
              combo.FriendlyName = device.FriendlyName;
              combo.VideoDevice = device.VideoDevice;
              combo.ID = device.ID;
              comboBoxCard.Items.Add(combo);

            }
            //
            // Finally close our file stream
            //
            fileStream.Close();
          }
          catch
          {
            MessageBox.Show("Failed to load previously configured capture card(s), you will need to re-configure your device(s).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
        }
      }


      if (comboBoxCard.Items.Count != 0)
        comboBoxCard.SelectedIndex = 0;
      FillInChannelCardMappings();
    }

    void FillInChannelCardMappings()
    {
      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard)comboBoxCard.Items[index];
        card = combo.ID;
      }

      listViewTVChannelsCard.Items.Clear();
      listViewTVChannelsForCard.Items.Clear();
      ArrayList cardChannels = new ArrayList();
      TVDatabase.GetChannelsForCard(ref cardChannels, card);

      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        bool mapped = false;
        foreach (TVChannel chanCard in cardChannels)
        {
          if (chanCard.Name == chan.Name)
          {
            mapped = true;
            break;
          }
        }
        if (!mapped)
        {
          ListViewItem newItem = new ListViewItem(chan.Name);
          newItem.Tag = chan;
          listViewTVChannelsCard.Items.Add(newItem);
        }
      }

      foreach (TVChannel chanCard in cardChannels)
      {
        ListViewItem newItemCard = new ListViewItem(chanCard.Name);
        newItemCard.Tag = chanCard;
        listViewTVChannelsForCard.Items.Add(newItemCard);
      }
    }

    private void comboBoxCard_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      FillInChannelCardMappings();
    }

    private void btnMapChannelToCard_Click(object sender, System.EventArgs e)
    {
      if (listViewTVChannelsCard.SelectedItems == null) return;
      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard)comboBoxCard.Items[index];
        card = combo.ID;
      }

      for (int i = 0; i < listViewTVChannelsCard.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewTVChannelsCard.SelectedItems[i];
        TVChannel chan = (TVChannel)listItem.Tag;

        listItem = new ListViewItem(new string[] { chan.Name });
        listItem.Tag = chan;
        listViewTVChannelsForCard.Items.Add(listItem);
        if (chan != null)
          TVDatabase.MapChannelToCard(chan.ID, card);
      }

      for (int i = listViewTVChannelsCard.SelectedItems.Count - 1; i >= 0; i--)
      {
        ListViewItem listItem = listViewTVChannelsCard.SelectedItems[i];

        listViewTVChannelsCard.Items.Remove(listItem);
      }
    }

    private void btnUnmapChannelFromCard_Click(object sender, System.EventArgs e)
    {
      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard)comboBoxCard.Items[index];
        card = combo.ID;
      }
      if (listViewTVChannelsForCard.SelectedItems == null) return;
      for (int i = 0; i < listViewTVChannelsForCard.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewTVChannelsForCard.SelectedItems[i];
        TVChannel chan = (TVChannel)listItem.Tag;

        listItem = new ListViewItem(new string[] { chan.Name });
        listItem.Tag = chan;
        listViewTVChannelsCard.Items.Add(listItem);
      }

      for (int i = listViewTVChannelsForCard.SelectedItems.Count - 1; i >= 0; --i)
      {
        ListViewItem listItem = listViewTVChannelsForCard.SelectedItems[i];
        TVChannel channel = listItem.Tag as TVChannel;
        if (channel != null)
          TVDatabase.UnmapChannelFromCard(channel, card);
        listViewTVChannelsForCard.Items.Remove(listItem);
      }
    }

    private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SaveSettings();
      LoadSettings();
    }

    private void Export_to_XML(string fileStr)
    {
      //Create flags to delete file if there is no data to export
      bool CHANNEL_EXPORT = false;
      bool GROUP_EXPORT = false;
      bool CARD_EXPORT = false;
      bool RECORDED_EXPORT = false;
      bool RECORDINGS_EXPORT = false;

      //Current version number of this exporter (change when needed)
      int CURRENT_VERSION = 1;  //<--- Make sure this same number is given to Import_from_XML

      using (MediaPortal.Profile.Settings channels = new MediaPortal.Profile.Settings(fileStr, false))
      {
        //Channel Data
        //channels.Clear();
        channels.SetValue("MP channel export list", "version", CURRENT_VERSION.ToString());
        if (channelsListView.Items.Count == 0)
        {
          MessageBox.Show("No channels to export");
          CHANNEL_EXPORT = false;
        }
        else
        {
          foreach (ListViewItem listItem in channelsListView.Items)
          {
            TelevisionChannel Selected_Chan = listItem.Tag as TelevisionChannel;

            //Set index
            channels.SetValue(listItem.Index.ToString(), "INDEX", listItem.Index.ToString());

            //Channel data
            channels.SetValueAsBool(listItem.Index.ToString(), "Scrambled", Selected_Chan.Scrambled);
            channels.SetValue(listItem.Index.ToString(), "ID", Selected_Chan.ID.ToString());
            channels.SetValue(listItem.Index.ToString(), "Number", Selected_Chan.Channel.ToString());
            channels.SetValue(listItem.Index.ToString(), "Name", Selected_Chan.Name.ToString());
            channels.SetValue(listItem.Index.ToString(), "Country", Selected_Chan.Country.ToString());
            channels.SetValueAsBool(listItem.Index.ToString(), "External", Selected_Chan.External);
            channels.SetValue(listItem.Index.ToString(), "External Tuner Channel", Selected_Chan.ExternalTunerChannel.ToString());
            channels.SetValue(listItem.Index.ToString(), "Frequency", Selected_Chan.Frequency.ToString());
            channels.SetValue(listItem.Index.ToString(), "Analog Standard Index", Selected_Chan.standard.ToString());
            channels.SetValueAsBool(listItem.Index.ToString(), "Visible in Guide", Selected_Chan.VisibleInGuide);
            if (Selected_Chan.Channel >= 0)
            {
              int bandWidth, freq, ONID, TSID, SID, symbolrate, innerFec, modulation, audioPid, videoPid, teletextPid, pmtPid;
              string provider;
              int audio1, audio2, audio3, ac3Pid, pcrPid;
              string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
              bool HasEITPresentFollow, HasEITSchedule;
              //DVB-T
              TVDatabase.GetDVBTTuneRequest(Selected_Chan.ID, out provider, out freq, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
              channels.SetValue(listItem.Index.ToString(), "DVBTFreq", freq.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTONID", ONID.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTTSID", TSID.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTSID", SID.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTProvider", provider);
              channels.SetValue(listItem.Index.ToString(), "DVBTAudioPid", audioPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTVideoPid", videoPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTTeletextPid", teletextPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTPmtPid", pmtPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTBandwidth", bandWidth.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTAudio1Pid", audio1.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTAudio2Pid", audio2.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTAudio3Pid", audio3.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTAC3Pid", ac3Pid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTPCRPid", pcrPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBTAudioLanguage", audioLanguage);
              channels.SetValue(listItem.Index.ToString(), "DVBTAudioLanguage1", audioLanguage1);
              channels.SetValue(listItem.Index.ToString(), "DVBTAudioLanguage2", audioLanguage2);
              channels.SetValue(listItem.Index.ToString(), "DVBTAudioLanguage3", audioLanguage3);
              channels.SetValueAsBool(listItem.Index.ToString(), "DVBTHasEITPresentFollow", HasEITPresentFollow);
              channels.SetValueAsBool(listItem.Index.ToString(), "DVBTHasEITSchedule", HasEITSchedule);

              //DVB-C
              TVDatabase.GetDVBCTuneRequest(Selected_Chan.ID, out provider, out freq, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
              channels.SetValue(listItem.Index.ToString(), "DVBCFreq", freq.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCONID", ONID.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCTSID", TSID.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCSID", SID.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCSR", symbolrate.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCInnerFeq", innerFec.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCModulation", modulation.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCProvider", provider);
              channels.SetValue(listItem.Index.ToString(), "DVBCAudioPid", audioPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCVideoPid", videoPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCTeletextPid", teletextPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCPmtPid", pmtPid.ToString());

              channels.SetValue(listItem.Index.ToString(), "DVBCAudio1Pid", audio1.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCAudio2Pid", audio2.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCAudio3Pid", audio3.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCAC3Pid", ac3Pid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCPCRPid", pcrPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCAudioLanguage", audioLanguage);
              channels.SetValue(listItem.Index.ToString(), "DVBCAudioLanguage1", audioLanguage1);
              channels.SetValue(listItem.Index.ToString(), "DVBCAudioLanguage2", audioLanguage2);
              channels.SetValue(listItem.Index.ToString(), "DVBCAudioLanguage3", audioLanguage3);
              channels.SetValueAsBool(listItem.Index.ToString(), "DVBCHasEITPresentFollow", HasEITPresentFollow);
              channels.SetValueAsBool(listItem.Index.ToString(), "DVBCHasEITSchedule", HasEITSchedule);

              //DVB-S
              DVBChannel ch = new DVBChannel();
              TVDatabase.GetSatChannel(Selected_Chan.ID, 1, ref ch);
              channels.SetValue(listItem.Index.ToString(), "DVBSFreq", ch.Frequency.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSONID", ch.NetworkID.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSTSID", ch.TransportStreamID.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSSID", ch.ProgramNumber.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSSymbolrate", ch.Symbolrate.ToString());
              channels.SetValue(listItem.Index.ToString(), "DvbSInnerFec", ch.FEC.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSPolarisation", ch.Polarity.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSProvider", ch.ServiceProvider);
              channels.SetValue(listItem.Index.ToString(), "DVBSAudioPid", ch.AudioPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSVideoPid", ch.VideoPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSTeletextPid", ch.TeletextPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSECMpid", ch.ECMPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBCPmtPid", ch.PMTPid.ToString());

              channels.SetValue(listItem.Index.ToString(), "DVBSAudio1Pid", ch.Audio1.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSAudio2Pid", ch.Audio2.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSAudio3Pid", ch.Audio3.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSAC3Pid", ch.AC3Pid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSPCRPid", ch.PCRPid.ToString());
              channels.SetValue(listItem.Index.ToString(), "DVBSAudioLanguage", ch.AudioLanguage);
              channels.SetValue(listItem.Index.ToString(), "DVBSAudioLanguage1", ch.AudioLanguage1);
              channels.SetValue(listItem.Index.ToString(), "DVBSAudioLanguage2", ch.AudioLanguage2);
              channels.SetValue(listItem.Index.ToString(), "DVBSAudioLanguage3", ch.AudioLanguage3);
              channels.SetValueAsBool(listItem.Index.ToString(), "DVBSHasEITPresentFollow", ch.HasEITPresentFollow);
              channels.SetValueAsBool(listItem.Index.ToString(), "DVBSHasEITSchedule", ch.HasEITSchedule);

            }
          }
          CHANNEL_EXPORT = true;
        }

        //Group Data and channel maping
        ArrayList groups = new ArrayList();
        TVDatabase.GetGroups(ref groups);
        //Save total groups for reference
        if (groups.Count == 0)
        {
          MessageBox.Show("No groups to export");
          GROUP_EXPORT = false;
        }
        else
        {
          channels.SetValue("GROUPS", "TOTAL", groups.Count.ToString());
          GROUP_EXPORT = true;
        }
        for (int i = 0; i < groups.Count; i++)
        {
          foreach (TVGroup group in groups)
          {
            if (group.Sort == i)
            {
              channels.SetValue("Group " + i.ToString(), "ID", group.ID.ToString());
              channels.SetValue("Group " + i.ToString(), "NAME", group.GroupName);
              channels.SetValue("Group " + i.ToString(), "PINCODE", group.Pincode.ToString());
              channels.SetValue("Group " + i.ToString(), "SORT", group.Sort.ToString());

              //Save total channels added to this group
              //TVDatabase.GetTVChannelsForGroup(group);
              channels.SetValue("Group " + i.ToString(), "TOTAL CHANNELS", group.TvChannels.Count.ToString());

              //Save current channel ID's under this group
              int channel_index = 0;
              foreach (TVChannel tvChan in group.TvChannels)
              {
                channels.SetValue("Group " + i.ToString(), "CHANNEL " + channel_index.ToString(), tvChan.ID.ToString());
                channel_index++;
              }
              break;
            }
          }
        }

        //Card mapping data
        ArrayList Cards = new ArrayList();
        TVDatabase.GetCards(ref Cards);

        //If we have no cards skip this
        if (Cards.Count == 0)
        {
          MessageBox.Show("No card data to export");
          CARD_EXPORT = false;
        }
        else
        {
          channels.SetValue("CARDS", "TOTAL", Cards.Count.ToString());
          CARD_EXPORT = true;
        }
        for (int i = 1; i < Cards.Count + 1; i++)
        {
          try
          {
            //ArrayList Channels = new ArrayList();
            //TVDatabase.GetChannels(ref Channels);
            ArrayList tmpChannels = new ArrayList();
            TVDatabase.GetChannelsForCard(ref tmpChannels, i);
            channels.SetValue("Card " + i.ToString(), "ID", i.ToString());
            channels.SetValue("Card " + i.ToString(), "TOTAL CHANNELS", tmpChannels.Count.ToString());
            int channel_index = 0;
            foreach (TVChannel tmpChan in tmpChannels)
            {
              channels.SetValue("Card " + i.ToString(), "CHANNEL " + channel_index.ToString(), tmpChan.ID.ToString());
              channel_index++;
            }
          }
          catch { }
        }

        //Backup recorded shows information
        ArrayList Recorded = new ArrayList();
        TVDatabase.GetRecordedTV(ref Recorded);

        if (Recorded.Count == 0)
        {
          MessageBox.Show("There is no Recorded TV data to export");
          RECORDED_EXPORT = false;
        }
        else
        {
          channels.SetValue("RECORDED", "TOTAL", Recorded.Count.ToString());
          RECORDED_EXPORT = true;
        }

        int count = 0;
        foreach (TVRecorded show in Recorded)
        {
          channels.SetValue("Recorded " + count.ToString(), "ID", show.ID.ToString());
          channels.SetValue("Recorded " + count.ToString(), "TITLE", show.Title);
          channels.SetValue("Recorded " + count.ToString(), "CHANNEL", show.Channel);
          channels.SetValue("Recorded " + count.ToString(), "DESC", show.Description);
          channels.SetValue("Recorded " + count.ToString(), "GENRE", show.Genre);
          channels.SetValue("Recorded " + count.ToString(), "FILENAME", show.FileName);
          channels.SetValue("Recorded " + count.ToString(), "STARTTIME", show.Start.ToString());
          channels.SetValue("Recorded " + count.ToString(), "ENDTIME", show.End.ToString());
          channels.SetValue("Recorded " + count.ToString(), "PLAYED", show.Played.ToString());
          count++;
        }

        //Backup recording shows information
        ArrayList Recordings = new ArrayList();
        TVDatabase.GetRecordings(ref Recordings);

        if (Recordings.Count == 0)
        {
          MessageBox.Show("There is no Recording TV data to export");
          RECORDINGS_EXPORT = false;
        }
        else
        {
          channels.SetValue("RECORDINGS", "TOTAL", Recordings.Count.ToString());
          RECORDINGS_EXPORT = true;
        }

        for (int i = 1; i < Recordings.Count + 1; i++)
        {
          MediaPortal.TV.Database.TVRecording show = (MediaPortal.TV.Database.TVRecording)Recordings[i - 1];
          channels.SetValue("Recording " + i.ToString(), "ID", show.ID.ToString());
          channels.SetValue("Recording " + i.ToString(), "TITLE", show.Title);
          channels.SetValue("Recording " + i.ToString(), "CHANNEL", show.Channel);
          channels.SetValue("Recording " + i.ToString(), "STARTTIME", show.Start.ToString());
          channels.SetValue("Recording " + i.ToString(), "ENDTIME", show.End.ToString());
          channels.SetValue("Recording " + i.ToString(), "CANCELEDTIME", show.Canceled.ToString());
          channels.SetValue("Recording " + i.ToString(), "TYPE", show.RecType.ToString());
          channels.SetValue("Recording " + i.ToString(), "PRIORITY", show.Priority.ToString());
          channels.SetValue("Recording " + i.ToString(), "QUALITY", show.Quality.ToString());
          //channels.SetValue("Recording "+i.ToString(),"STATUS",show.Status.ToString());
          channels.SetValueAsBool("Recording " + i.ToString(), "ISCONTENTREC", show.IsContentRecording);
          channels.SetValueAsBool("Recording " + i.ToString(), "SERIES", show.Series);
          channels.SetValue("Recording " + i.ToString(), "EPISODES", show.EpisodesToKeep.ToString());


          //Check if this recording has had any cancels
          channels.SetValue("Recording " + i.ToString(), "CANCELED SERIES TOTAL", show.CanceledSeries.Count.ToString());
          if (show.CanceledSeries.Count > 0)
          {
            int canx_count = 0;
            List<long> get_show = show.CanceledSeries;
            foreach (long canx_show in get_show)
            {
              channels.SetValue("Recording " + i.ToString(), "CANCELED SERIES CANCELEDTIME " + canx_count.ToString(), canx_show.ToString());
              canx_count++;
            }
          }
        }

      }

      //Check to see if we need to delete file
      if (!CHANNEL_EXPORT && !GROUP_EXPORT && !CARD_EXPORT && !RECORDED_EXPORT && !RECORDINGS_EXPORT)
      {
        //Delete file
        File.Delete(fileStr);
        return;
      }
    }

    private void Import_From_XML(string fileStr)
    {
      //Check if we have a file just in case
      if (!File.Exists(fileStr)) return;

      TVDatabase.ClearAll();
      LoadSettings();
      //Current Version change to reflect the above exporter in order for compatibility
      int CURRENT_VER = 1;   //<--- Make sure that is the same number as in Export_to_XML
      int VER = 1;			 //Set to:  0 = old ; 1 = current ; 2 = newer
      using (MediaPortal.Profile.Settings channels = new MediaPortal.Profile.Settings(fileStr))
      {
        //Check version if not the right version prompt/do stuff/accomodate/change
        int version_check = channels.GetValueAsInt("MP channel export list", "version", -1);
        if (version_check == -1)
        {
          //Not a valid channel list
          MessageBox.Show("This is not a valid channel list!");
          return;
        }
        else if (version_check >= 0 && version_check < CURRENT_VER)
        {
          //Older file
          MessageBox.Show("This is an older channel list file I will attempt to import what I can.");
          VER = 0;
        }
        else if (version_check == CURRENT_VER)
        {
          //Current file, this is good stuff
          VER = 1;
        }
        else if (version_check > CURRENT_VER)
        {
          //Newer? This person lives in a cave
          MessageBox.Show("This is a newer channel list file I will attempt to get what I can.\nConsider upgrading to a newer version of MP.");
          VER = 2;
        }

        //Count how many channels we have to import
        int counter = 0;
        for (int i = 0; ; i++)
        {
          if (channels.GetValueAsInt(i.ToString(), "INDEX", -1) == -1)
          {
            if (counter == 0)
            {
              MessageBox.Show("No channels found");
              return;
            }
            else break;
          }
          else counter++;
        }
        MessageBox.Show("There is a total of " + counter.ToString() + " stations to import");

        for (int i = 0; i < counter; i++)
        {
          int overwrite = 0;
          int overwrite_index = 0;
          TelevisionChannel Import_Chan = new TelevisionChannel();
          Import_Chan.ID = channels.GetValueAsInt(i.ToString(), "ID", 0);
          Import_Chan.Channel = channels.GetValueAsInt(i.ToString(), "Number", 0);
          Import_Chan.Name = channels.GetValueAsString(i.ToString(), "Name", "");
          Import_Chan.Country = channels.GetValueAsInt(i.ToString(), "Country", 0);
          Import_Chan.External = channels.GetValueAsBool(i.ToString(), "External", false);
          Import_Chan.ExternalTunerChannel = channels.GetValueAsString(i.ToString(), "External Tuner Channel", "");
          Import_Chan.Frequency.MegaHerz = Convert.ToDouble(channels.GetValueAsFloat(i.ToString(), "Frequency", 0));
          Import_Chan.standard = Convert_AVS(channels.GetValueAsString(i.ToString(), "Analog Standard Index", "None"));
          Import_Chan.VisibleInGuide = channels.GetValueAsBool(i.ToString(), "Visible in Guide", false);
          Import_Chan.Scrambled = channels.GetValueAsBool(i.ToString(), "Scrambled", false);

          //Check to see if this channel exists prompt to overwrite
          foreach (ListViewItem listItem in channelsListView.Items)
          {
            TelevisionChannel Check_Chan = listItem.Tag as TelevisionChannel;
            if (Check_Chan.ID == Import_Chan.ID && Check_Chan.Name == Import_Chan.Name)
            {
              if (MessageBox.Show(Import_Chan.Name + " (Channel " + Import_Chan.Channel.ToString() + ") Already exists.\nWould you like to overwrite?", "Warning!", MessageBoxButtons.YesNo) == DialogResult.Yes)
              {
                overwrite = 1;
                overwrite_index = listItem.Index;
              }
              else
              {
                overwrite = -1;
              }
              break;
            }
          }

          if (overwrite == 0 || overwrite == 1)
          {
            if (overwrite == 1)
            {
              TelevisionChannel editedChannel = Import_Chan;
              channelsListView.Items[overwrite_index].Tag = editedChannel;

              channelsListView.Items[overwrite_index].SubItems[0].Text = editedChannel.Name;
              channelsListView.Items[overwrite_index].SubItems[1].Text = editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString();
              channelsListView.Items[overwrite_index].SubItems[2].Text = GetStandardName(editedChannel.standard);
              channelsListView.Items[overwrite_index].SubItems[3].Text = editedChannel.External ? "External" : "Internal";
            }
            else
            {
              TelevisionChannel editedChannel = Import_Chan;
              ListViewItem listItem = new ListViewItem(new string[] { editedChannel.Name, 
																					  editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString(),
																					  GetStandardName(editedChannel.standard),
																					  editedChannel.External ? "External" : "Internal"
																				  });
              listItem.Tag = editedChannel;

              channelsListView.Items.Add(listItem);
              channelsListView.Items[channelsListView.Items.IndexOf(listItem)].Checked = true;
            }

            //Check if required to do anything specific for compatibility reasons
            switch (VER)
            {
              //Do stuff for backward compatibility if needed
              case 0:

                break;
              //Do stuff for current version only
              case 1:

                break;
              //Do stuff for forward compatibility if needed
              case 2:

                break;
            }

            //This is done for every version regardless
            if (VER == 0 || VER == 1 || VER == 2)
            {
              if (Import_Chan.Channel >= 0)
              {
                int freq, ONID, TSID, SID, symbolrate, innerFec, modulation, polarisation;
                int bandWidth, pmtPid, audioPid, videoPid, teletextPid;
                int audio1, audio2, audio3, ac3Pid, pcrPid;
                string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
                string provider;
                bool HasEITPresentFollow, HasEITSchedule;
                //dvb-T
                try
                {
                  freq = channels.GetValueAsInt(i.ToString(), "DVBTFreq", 0);
                  ONID = channels.GetValueAsInt(i.ToString(), "DVBTONID", 0);
                  TSID = channels.GetValueAsInt(i.ToString(), "DVBTTSID", 0);
                  SID = channels.GetValueAsInt(i.ToString(), "DVBTSID", 0);
                  audioPid = channels.GetValueAsInt(i.ToString(), "DVBTAudioPid", 0);
                  videoPid = channels.GetValueAsInt(i.ToString(), "DVBTVideoPid", 0);
                  teletextPid = channels.GetValueAsInt(i.ToString(), "DVBTTeletextPid", 0);
                  pmtPid = channels.GetValueAsInt(i.ToString(), "DVBTPmtPid", 0);
                  provider = channels.GetValueAsString(i.ToString(), "DVBTProvider", "");
                  bandWidth = channels.GetValueAsInt(i.ToString(), "DVBTBandwidth", -1);
                  audio1 = channels.GetValueAsInt(i.ToString(), "DVBTAudio1Pid", -1);
                  audio2 = channels.GetValueAsInt(i.ToString(), "DVBTAudio2Pid", -1);
                  audio3 = channels.GetValueAsInt(i.ToString(), "DVBTAudio3Pid", -1);
                  ac3Pid = channels.GetValueAsInt(i.ToString(), "DVBTAC3Pid", -1);
                  pcrPid = channels.GetValueAsInt(i.ToString(), "DVBTPCRPid", -1);
                  audioLanguage = channels.GetValueAsString(i.ToString(), "DVBTAudioLanguage", "");
                  audioLanguage1 = channels.GetValueAsString(i.ToString(), "DVBTAudioLanguage1", "");
                  audioLanguage2 = channels.GetValueAsString(i.ToString(), "DVBTAudioLanguage2", "");
                  audioLanguage3 = channels.GetValueAsString(i.ToString(), "DVBTAudioLanguage3", "");
                  HasEITPresentFollow = channels.GetValueAsBool(i.ToString(), "DVBTHasEITPresentFollow", false);
                  HasEITSchedule = channels.GetValueAsBool(i.ToString(), "DVBTHasEITSchedule", false);
                  if (ONID > 0 && TSID > 0 && SID > 0 && freq > 0)
                  {
                    TVDatabase.MapDVBTChannel(Import_Chan.Name, provider, Import_Chan.ID, freq, ONID, TSID, SID, audioPid, videoPid, teletextPid, pmtPid, bandWidth, audio1, audio2, audio3, ac3Pid, pcrPid, audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3, HasEITPresentFollow, HasEITSchedule);
                  }
                }
                catch (Exception)
                {
                  MessageBox.Show("OOPS! Something odd happened.\nCouldn't import DVB-T data.");
                }

                //dvb-C
                try
                {
                  freq = channels.GetValueAsInt(i.ToString(), "DVBCFreq", 0);
                  ONID = channels.GetValueAsInt(i.ToString(), "DVBCONID", 0);
                  TSID = channels.GetValueAsInt(i.ToString(), "DVBCTSID", 0);
                  SID = channels.GetValueAsInt(i.ToString(), "DVBCSID", 0);
                  symbolrate = channels.GetValueAsInt(i.ToString(), "DVBCSR", 0);
                  innerFec = channels.GetValueAsInt(i.ToString(), "DVBCInnerFeq", 0);
                  modulation = channels.GetValueAsInt(i.ToString(), "DVBCModulation", 0);
                  provider = channels.GetValueAsString(i.ToString(), "DVBCProvider", "");
                  audioPid = channels.GetValueAsInt(i.ToString(), "DVBCAudioPid", 0);
                  videoPid = channels.GetValueAsInt(i.ToString(), "DVBCVideoPid", 0);
                  teletextPid = channels.GetValueAsInt(i.ToString(), "DVBCTeletextPid", 0);
                  pmtPid = channels.GetValueAsInt(i.ToString(), "DVBCPmtPid", 0);
                  audio1 = channels.GetValueAsInt(i.ToString(), "DVBCAudio1Pid", -1);
                  audio2 = channels.GetValueAsInt(i.ToString(), "DVBCAudio2Pid", -1);
                  audio3 = channels.GetValueAsInt(i.ToString(), "DVBCAudio3Pid", -1);
                  ac3Pid = channels.GetValueAsInt(i.ToString(), "DVBCAC3Pid", -1);
                  pcrPid = channels.GetValueAsInt(i.ToString(), "DVBCPCRPid", -1);
                  audioLanguage = channels.GetValueAsString(i.ToString(), "DVBCAudioLanguage", "");
                  audioLanguage1 = channels.GetValueAsString(i.ToString(), "DVBCAudioLanguage1", "");
                  audioLanguage2 = channels.GetValueAsString(i.ToString(), "DVBCAudioLanguage2", "");
                  audioLanguage3 = channels.GetValueAsString(i.ToString(), "DVBCAudioLanguage3", "");
                  HasEITPresentFollow = channels.GetValueAsBool(i.ToString(), "DVBCHasEITPresentFollow", false);
                  HasEITSchedule = channels.GetValueAsBool(i.ToString(), "DVBCHasEITSchedule", false);

                  if (ONID > 0 && TSID > 0 && SID > 0 && freq > 0)
                  {
                    TVDatabase.MapDVBCChannel(Import_Chan.Name, provider, Import_Chan.ID, freq, symbolrate, innerFec, modulation, ONID, TSID, SID, audioPid, videoPid, teletextPid, pmtPid, audio1, audio2, audio3, ac3Pid, pcrPid, audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3, HasEITPresentFollow, HasEITSchedule);
                  }
                }
                catch (Exception)
                {
                  MessageBox.Show("OOPS! Something odd happened.\nCouldn't import DVB-C data.");
                }

                //dvb-S
                try
                {
                  DVBChannel ch = new DVBChannel();
                  TVDatabase.GetSatChannel(Import_Chan.ID, 1, ref ch);

                  freq = channels.GetValueAsInt(i.ToString(), "DVBSFreq", 0);
                  ONID = channels.GetValueAsInt(i.ToString(), "DVBSONID", 0);
                  TSID = channels.GetValueAsInt(i.ToString(), "DVBSTSID", 0);
                  SID = channels.GetValueAsInt(i.ToString(), "DVBSSID", 0);
                  symbolrate = channels.GetValueAsInt(i.ToString(), "DVBSSymbolrate", 0);
                  innerFec = channels.GetValueAsInt(i.ToString(), "DvbSInnerFec", 0);
                  polarisation = channels.GetValueAsInt(i.ToString(), "DVBSPolarisation", 0);
                  provider = channels.GetValueAsString(i.ToString(), "DVBSProvider", "");
                  audioPid = channels.GetValueAsInt(i.ToString(), "DVBSAudioPid", 0);
                  videoPid = channels.GetValueAsInt(i.ToString(), "DVBSVideoPid", 0);
                  teletextPid = channels.GetValueAsInt(i.ToString(), "DVBSTeletextPid", 0);
                  pmtPid = channels.GetValueAsInt(i.ToString(), "DVBSPmtPid", 0);
                  audio1 = channels.GetValueAsInt(i.ToString(), "DVBSAudio1Pid", -1);
                  audio2 = channels.GetValueAsInt(i.ToString(), "DVBSAudio2Pid", -1);
                  audio3 = channels.GetValueAsInt(i.ToString(), "DVBSAudio3Pid", -1);
                  ac3Pid = channels.GetValueAsInt(i.ToString(), "DVBSAC3Pid", -1);
                  pcrPid = channels.GetValueAsInt(i.ToString(), "DVBSPCRPid", -1);
                  audioLanguage = channels.GetValueAsString(i.ToString(), "DVBSAudioLanguage", "");
                  audioLanguage1 = channels.GetValueAsString(i.ToString(), "DVBSAudioLanguage1", "");
                  audioLanguage2 = channels.GetValueAsString(i.ToString(), "DVBSAudioLanguage2", "");
                  audioLanguage3 = channels.GetValueAsString(i.ToString(), "DVBSAudioLanguage3", "");
                  HasEITPresentFollow = channels.GetValueAsBool(i.ToString(), "DVBSHasEITPresentFollow", false);
                  HasEITSchedule = channels.GetValueAsBool(i.ToString(), "DVBSHasEITSchedule", false);

                  if (ONID > 0 && TSID > 0 && SID > 0 && freq > 0)
                  {
                    ch.ServiceType = 1;
                    ch.Frequency = freq;
                    ch.NetworkID = ONID;
                    ch.TransportStreamID = TSID;
                    ch.ProgramNumber = SID;
                    ch.Symbolrate = symbolrate;
                    ch.FEC = innerFec;
                    ch.Polarity = polarisation;
                    ch.ServiceProvider = provider;
                    ch.ServiceName = Import_Chan.Name;
                    ch.ID = Import_Chan.ID;
                    ch.AudioPid = audioPid;
                    ch.VideoPid = videoPid;
                    ch.TeletextPid = teletextPid;
                    ch.PCRPid = pcrPid;
                    ch.ECMPid = channels.GetValueAsInt(i.ToString(), "DVBSECMpid", 0);
                    ch.Audio1 = audio1;
                    ch.Audio2 = audio2;
                    ch.Audio3 = audio3;
                    ch.AudioLanguage = audioLanguage;
                    ch.AudioLanguage1 = audioLanguage1;
                    ch.AudioLanguage2 = audioLanguage2;
                    ch.AudioLanguage3 = audioLanguage3;
                    ch.HasEITSchedule = HasEITSchedule;
                    ch.HasEITPresentFollow = HasEITPresentFollow;
                    TVDatabase.UpdateSatChannel(ch);
                  }
                }
                catch (Exception)
                {
                  MessageBox.Show("OOPS! Something odd happened.\nCouldn't import DVB-S data.");
                }
              }
            }
          }
          else if (overwrite == -1)
          {
            //Go to the next channel do nothing
          }
        }
        SaveTVChannels();
        SaveSettings();


        //Grab group Data and channel maping
        int group_index = 0, channel_index = 0;

        //Grab total groups for reference
        group_index = channels.GetValueAsInt("GROUPS", "TOTAL", -1);
        //Check if we have any groups
        if (group_index == -1 || group_index == 0)
        {
          MessageBox.Show("There are no groups to import");
        }
        else
        {
          MessageBox.Show("There is a total of " + group_index.ToString() + " groups to import");

          for (int i = 0; i < group_index; i++)
          {
            int overwrite = 0;
            TVGroup Import_Group = new TVGroup();
            Import_Group.ID = channels.GetValueAsInt("Group " + i.ToString(), "ID", 0);
            Import_Group.GroupName = channels.GetValueAsString("Group " + i.ToString(), "NAME", "");
            Import_Group.Pincode = channels.GetValueAsInt("Group " + i.ToString(), "PINCODE", 0);
            Import_Group.Sort = channels.GetValueAsInt("Group " + i.ToString(), "SORT", 0);

            if (overwrite == 0 || overwrite == 1)
            {
              //Add Group to database
              TVDatabase.AddGroup(Import_Group);
              //This is done for every version regardless
              if (VER == 0 || VER == 1 || VER == 2)
              {
                //Add channels to this group
                ArrayList Group_Channels = new ArrayList();
                TVDatabase.GetChannels(ref Group_Channels);
                channel_index = channels.GetValueAsInt("Group " + i.ToString(), "TOTAL CHANNELS", 0);
                if (channel_index > 0)
                {

                  for (int j = 0; j < channel_index; j++)
                  {
                    int tmpID = channels.GetValueAsInt("Group " + i.ToString(), "CHANNEL " + j.ToString(), 0);

                    //Locate Channel so it can be added to group
                    foreach (TVChannel FindChan in Group_Channels)
                    {
                      if (FindChan.ID == tmpID)
                      {
                        //Add channel to group
                        Import_Group.TvChannels.Add(FindChan);

                        //Have to re-grab group from database in order to map correctly :|
                        ArrayList GrabGroup = new ArrayList();
                        TVDatabase.GetGroups(ref GrabGroup);

                        foreach (TVGroup tmpGroup in GrabGroup)
                        {
                          if (Import_Group.ID == tmpGroup.ID)
                          {
                            TVDatabase.MapChannelToGroup(tmpGroup, FindChan);
                          }
                        }
                      }
                    }
                  }
                }
                else
                {
                  //Add Group to database
                  //TVDatabase.AddGroup(Import_Group);
                }
              }
            }
            else if (overwrite == -1)
            {
              //Go to the next group do nothing
            }
          }
          SaveSettings();
        }

        //Grab Saved Card mapping

        //Check if we have cards first
        int cards_index = 0;
        channel_index = 0;

        //Grab total cards for reference
        cards_index = channels.GetValueAsInt("CARDS", "TOTAL", -1);

        //Check if we have any cards
        for (int i = 1; i < cards_index + 1; i++)
        {
          //This is done for every version regardless
          //Re-Map channels to available cards
          ArrayList Card_Channels = new ArrayList();
          TVDatabase.GetChannels(ref Card_Channels);
          channel_index = channels.GetValueAsInt("Card " + i.ToString(), "TOTAL CHANNELS", 0);

          if (channel_index > 0)
          {
            for (int j = 0; j < channel_index; j++)
            {
              int tmpID = channels.GetValueAsInt("Card " + i.ToString(), "CHANNEL " + j.ToString(), 0);

              //Locate Channel so it can be added to Card
              foreach (TVChannel FindChan in Card_Channels)
              {
                if (FindChan.ID == tmpID)
                {
                  //Map it
                  TVDatabase.MapChannelToCard(FindChan.ID, i);
                }
              }
            }
          }
        }

        //Grab recorded show information
        int recorded_count = 0;

        //Grab recorded shows saved for referrence
        recorded_count = channels.GetValueAsInt("RECORDED", "TOTAL", -1);
        if (recorded_count == -1 || recorded_count == 0)
        {
          MessageBox.Show("There is no Recorded TV data to import");
        }
        else
        {
          MessageBox.Show("There is a total of " + recorded_count.ToString() + " recorded items to import");

          //Check if required to do anything specific for compatibility reasons
          switch (VER)
          {
            //Do stuff for backward compatibility if needed
            case 0:

              break;
            //Do stuff for current version only
            case 1:

              break;
            //Do stuff for forward compatibility if needed
            case 2:

              break;
          }

          //This is done for every version regardless
          if (VER == 0 || VER == 1 || VER == 2)
          {
            for (int i = 1; i < recorded_count + 1; i++)
            {
              //Create temp TVRecorded to hold data to import
              TVRecorded temp_recorded = new TVRecorded();
              temp_recorded.ID = channels.GetValueAsInt("Recorded " + i.ToString(), "ID", 0);
              temp_recorded.Title = channels.GetValueAsString("Recorded " + i.ToString(), "TITLE", "");
              temp_recorded.Channel = channels.GetValueAsString("Recorded " + i.ToString(), "CHANNEL", "");
              temp_recorded.Description = channels.GetValueAsString("Recorded " + i.ToString(), "DESC", "");
              temp_recorded.Genre = channels.GetValueAsString("Recorded " + i.ToString(), "GENRE", "");
              temp_recorded.FileName = channels.GetValueAsString("Recorded " + i.ToString(), "FILENAME", "");
              temp_recorded.Start = Convert.ToInt64(channels.GetValueAsString("Recorded " + i.ToString(), "STARTTIME", "0"));
              if (temp_recorded.Start == 0)
                temp_recorded.Start = Convert.ToInt64(channels.GetValueAsString("Recorded " + i.ToString(), "START", "0"));
              temp_recorded.End = Convert.ToInt64(channels.GetValueAsString("Recorded " + i.ToString(), "ENDTIME", "0"));
              temp_recorded.Played = channels.GetValueAsInt("Recorded " + i.ToString(), "PLAYED", 0);

              //Add or gathered info to the TVDatabase
              bool recorded_overwrite = false;
              ArrayList check_recorded_list = new ArrayList();
              TVDatabase.GetRecordedTV(ref check_recorded_list);
              TVRecorded check_recorded = new TVRecorded();
              foreach (TVRecorded check_me in check_recorded_list)
              {
                if (check_me.ID == temp_recorded.ID && check_me.Start.ToString() == temp_recorded.Start.ToString())
                {
                  check_recorded = check_me;
                  recorded_overwrite = true;
                  break;
                }
              }
              if (recorded_overwrite)
              {
                //Ask if user if overwrite ok
                if (MessageBox.Show("Would you like to overwrite the entry for " + check_recorded.Title + " - Start Time: " + check_recorded.Start.ToString() + "\nWith the entry " + temp_recorded.Title + " - Start Time: " + temp_recorded.Start.ToString() + "\nProceed with overwrite?",
                "Overwrite?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                  TVDatabase.RemoveRecordedTV(check_recorded);
                  TVDatabase.AddRecordedTV(temp_recorded);
                }
              }
              else
              {
                //Check if this file exists first, if not ask user to locate it or no update
                TVDatabase.AddRecordedTV(temp_recorded);
              }
            }
          }
        }


        //Grab recording shows information
        int recordings_count = 0;

        //Grab recorded shows saved for referrence
        recordings_count = channels.GetValueAsInt("RECORDINGS", "TOTAL", -1);

        if (recordings_count == -1 || recordings_count == 0)
        {
          MessageBox.Show("There is no Recording TV data to import");
        }
        else
        {
          MessageBox.Show("There is " + recordings_count.ToString() + " Recording TV data items to import");

          //Check if required to do anything specific for compatibility reasons
          switch (VER)
          {
            //Do stuff for backward compatibility if needed
            case 0:

              break;
            //Do stuff for current version only
            case 1:

              break;
            //Do stuff for forward compatibility if needed
            case 2:

              break;
          }

          //This is done for every version regardless
          if (VER == 0 || VER == 1 || VER == 2)
          {
            for (int i = 1; i < recordings_count + 1; i++)
            {
              //Create temp TVRecording to hold data to import
              MediaPortal.TV.Database.TVRecording temp_recording = new MediaPortal.TV.Database.TVRecording();
              temp_recording.ID = channels.GetValueAsInt("Recording " + i.ToString(), "ID", 0);
              temp_recording.Title = channels.GetValueAsString("Recording " + i.ToString(), "TITLE", "");
              temp_recording.Channel = channels.GetValueAsString("Recording " + i.ToString(), "CHANNEL", "");
              temp_recording.Start = Convert.ToInt64(channels.GetValueAsString("Recording " + i.ToString(), "STARTTIME", "0"));
              temp_recording.End = Convert.ToInt64(channels.GetValueAsString("Recording " + i.ToString(), "ENDTIME", "0"));
              temp_recording.Canceled = Convert.ToInt64(channels.GetValueAsString("Recording " + i.ToString(), "CANCELEDTIME", "0"));
              temp_recording.RecType = Convert_RecordingType(channels.GetValueAsString("Recording " + i.ToString(), "TYPE", ""));
              temp_recording.Priority = channels.GetValueAsInt("Recording " + i.ToString(), "PRIORITY", 0);
              temp_recording.Quality = Convert_QualityType(channels.GetValueAsString("Recording " + i.ToString(), "QUALITY", ""));
              //temp_recording.Status=(MediaPortal.TV.Database.TVRecording.RecordingStatus)channels.GetValue("Recording "+i.ToString(),"STATUS");
              temp_recording.IsContentRecording = channels.GetValueAsBool("Recording " + i.ToString(), "ISCONTENTREC", false);
              temp_recording.Series = channels.GetValueAsBool("Recording " + i.ToString(), "SERIES", false);
              temp_recording.EpisodesToKeep = channels.GetValueAsInt("Recording " + i.ToString(), "EPISODES", Int32.MaxValue);

              //Add this recording to TVDatabase
              bool recording_overwrite = false;
              ArrayList check_recording_list = new ArrayList();
              TVDatabase.GetRecordings(ref check_recording_list);
              MediaPortal.TV.Database.TVRecording check_recording = new MediaPortal.TV.Database.TVRecording();
              foreach (MediaPortal.TV.Database.TVRecording check_me in check_recording_list)
              {
                if (check_me.ID == temp_recording.ID && check_me.Start.ToString() == temp_recording.Start.ToString())
                {
                  check_recording = check_me;
                  recording_overwrite = true;
                  break;
                }
              }
              if (recording_overwrite)
              {
                //Ask if user if overwrite ok
                if (MessageBox.Show("Would you like to overwrite the entry for " + check_recording.Title + " - Start Time: " + check_recording.Start.ToString() + "\nWith the entry " + temp_recording.Title + " - Start Time: " + temp_recording.Start.ToString() + "\nProceed with overwrite?",
                  "Overwrite?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                  //Delete Canceled series information
                  for (int z = 0; z < check_recording.CanceledSeries.Count; ++z)
                  {
                    TVDatabase.DeleteCanceledSeries(check_recording);
                  }
                  //Check if this recording has had any cancels
                  int canx_count = 0;
                  canx_count = channels.GetValueAsInt("Recording " + i.ToString(), "CANCELED SERIES TOTAL", 0);
                  if (canx_count > 0)
                  {
                    temp_recording.CanceledSeries.Clear();
                    long last_canx_time = 0;
                    for (int j = 0; j < canx_count; j++)
                    {
                      //Add the canceled time to TVDatabase
                      long canx_time = 0;
                      canx_time = Convert.ToInt64(channels.GetValueAsString("Recording " + i.ToString(), "CANCELED SERIES CANCELEDTIME " + j.ToString(), "0"));
                      //Check if we had the same time from before if so stop adding
                      if (canx_time == last_canx_time) break;
                      //TVDatabase.AddCanceledSerie(temp_recording,canx_time);
                      temp_recording.CanceledSeries.Add((long)canx_time);
                      last_canx_time = canx_time;
                    }
                  }
                  //Delete old entry
                  TVDatabase.RemoveRecording(check_recording);
                  //Add new overwrite entry
                  TVDatabase.AddRecording(ref temp_recording);
                }
              }
              else
              {
                //Check if this recording has had any cancels
                int canx_count = 0;
                canx_count = channels.GetValueAsInt("Recording " + i.ToString(), "CANCELED SERIES TOTAL", 0);
                if (canx_count > 0)
                {
                  temp_recording.CanceledSeries.Clear();
                  long last_canx_time = 0;
                  for (int j = 0; j < canx_count; j++)
                  {
                    //Add the canceled time to TVDatabase
                    long canx_time = 0;
                    canx_time = Convert.ToInt64(channels.GetValueAsString("Recording " + i.ToString(), "CANCELED SERIES CANCELEDTIME " + j.ToString(), "0"));
                    //Check if we had the same time from before if so stop adding
                    if (canx_time == last_canx_time) break;
                    //TVDatabase.AddCanceledSerie(temp_recording,canx_time);
                    temp_recording.CanceledSeries.Add((long)canx_time);
                    last_canx_time = canx_time;
                  }
                }
                //Add new entry
                TVDatabase.AddRecording(ref temp_recording);
              }
            }
          }
        }
      }
    }

    private AnalogVideoStandard Convert_AVS(object avs)
    {
      if ((string)avs == "None") return AnalogVideoStandard.None;
      if ((string)avs == "NTSC_M") return AnalogVideoStandard.NTSC_M;
      if ((string)avs == "NTSC_M_J") return AnalogVideoStandard.NTSC_M_J;
      if ((string)avs == "NTSC_433") return AnalogVideoStandard.NTSC_433;
      if ((string)avs == "PAL_B") return AnalogVideoStandard.PAL_B;
      if ((string)avs == "PAL_D") return AnalogVideoStandard.PAL_D;
      if ((string)avs == "PAL_G") return AnalogVideoStandard.PAL_G;
      if ((string)avs == "PAL_H") return AnalogVideoStandard.PAL_H;
      if ((string)avs == "PAL_I") return AnalogVideoStandard.PAL_I;
      if ((string)avs == "PAL_M") return AnalogVideoStandard.PAL_M;
      if ((string)avs == "PAL_N") return AnalogVideoStandard.PAL_N;
      if ((string)avs == "PAL_60") return AnalogVideoStandard.PAL_60;
      if ((string)avs == "SECAM_B") return AnalogVideoStandard.SECAM_B;
      if ((string)avs == "SECAM_D") return AnalogVideoStandard.SECAM_D;
      if ((string)avs == "SECAM_G") return AnalogVideoStandard.SECAM_G;
      if ((string)avs == "SECAM_H") return AnalogVideoStandard.SECAM_H;
      if ((string)avs == "SECAM_K") return AnalogVideoStandard.SECAM_K;
      if ((string)avs == "SECAM_K1") return AnalogVideoStandard.SECAM_K1;
      if ((string)avs == "SECAM_L") return AnalogVideoStandard.SECAM_L;
      if ((string)avs == "SECAM_L1") return AnalogVideoStandard.SECAM_L1;
      if ((string)avs == "PAL_N_COMBO") return AnalogVideoStandard.PAL_N_COMBO;

      //If nothing return Default
      return AnalogVideoStandard.None;
    }

    private TV.Database.TVRecording.QualityType Convert_QualityType(object quality)
    {
      if ((string)quality == "NotSet") return TV.Database.TVRecording.QualityType.NotSet;
      if ((string)quality == "Portable") return TV.Database.TVRecording.QualityType.Portable;
      if ((string)quality == "Low") return TV.Database.TVRecording.QualityType.Low;
      if ((string)quality == "Medium") return TV.Database.TVRecording.QualityType.Medium;
      if ((string)quality == "High") return TV.Database.TVRecording.QualityType.High;

      //If nothing return Default
      return TV.Database.TVRecording.QualityType.NotSet;
    }

    private TV.Database.TVRecording.RecordingType Convert_RecordingType(object recType)
    {
      if ((string)recType == "Once") return TV.Database.TVRecording.RecordingType.Once;
      if ((string)recType == "EveryTimeOnThisChannel") return TV.Database.TVRecording.RecordingType.EveryTimeOnThisChannel;
      if ((string)recType == "EveryTimeOnEveryChannel") return TV.Database.TVRecording.RecordingType.EveryTimeOnEveryChannel;
      if ((string)recType == "Daily") return TV.Database.TVRecording.RecordingType.Daily;
      if ((string)recType == "Weekly") return TV.Database.TVRecording.RecordingType.Weekly;
      if ((string)recType == "WeekDays") return TV.Database.TVRecording.RecordingType.WeekDays;
      if ((string)recType == "WeekEnds") return TV.Database.TVRecording.RecordingType.WeekEnds;

      //If nothing return Default
      return TV.Database.TVRecording.RecordingType.Once;
    }
    private void xmlExport_Click(object sender, System.EventArgs e)
    {

      XMLSaveDialog.RestoreDirectory = true;
      if (XMLSaveDialog.ShowDialog(this) == DialogResult.OK)
      {
        Export_to_XML(XMLSaveDialog.FileName.ToString());
      }
    }

    private void xmlImport_Click(object sender, System.EventArgs e)
    {
      XMLOpenDialog.RestoreDirectory = true;
      if (XMLOpenDialog.ShowDialog(this) == DialogResult.OK)
      {
        Import_From_XML(XMLOpenDialog.FileName.ToString());
      }
    }

    private void buttonLookup_Click(object sender, System.EventArgs e)
    {
      TvChannelLookupService dlg = new TvChannelLookupService();
      dlg.ShowDialog(this);
      reloadList = true;
      RadioStations.UpdateList();
    }

    private void buttonCombine_Click(object sender, EventArgs e)
    {
      if (channelsListView.SelectedItems.Count != 2) return;

      TelevisionChannel ch1 = channelsListView.SelectedItems[0].Tag as TelevisionChannel;
      TelevisionChannel ch2 = channelsListView.SelectedItems[1].Tag as TelevisionChannel;

      if (ch1.Channel > 0 && ch1.Channel < 255)
      {
        ch2.Channel = ch1.Channel;
        ch2.Frequency = ch1.Frequency;
      }

      if (ch1.ExternalTunerChannel != String.Empty)
      {
        ch2.ExternalTunerChannel = ch1.ExternalTunerChannel;
        ch2.External = ch1.External;
      }
      //remap all atsc,dvb mappings from ch1->ch2...
      TVDatabase.ReMapDigitalMapping(ch1.ID, ch2.ID);

      ListViewItem listItem = channelsListView.SelectedItems[1];
      listItem.SubItems[0].Text = ch2.Name;
      listItem.SubItems[1].Text = ch2.External ? String.Format("{0}/{1}", ch2.Channel, ch2.ExternalTunerChannel) : ch2.Channel.ToString();
      listItem.SubItems[2].Text = GetStandardName(ch2.standard);
      listItem.SubItems[3].Text = ch2.External ? "External" : "Internal";
      listItem.ImageIndex = 0;
      if (ch2.Scrambled)
        listItem.ImageIndex = 1;

      channelsListView.Items.RemoveAt(channelsListView.SelectedIndices[0]);
      SaveSettings();
    }
  }
}

