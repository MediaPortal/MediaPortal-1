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
using System.Threading;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class SectionTvChannels : MediaPortal.Configuration.SectionSettings
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
    private System.Windows.Forms.OpenFileDialog xmlOpenDialog;
    private System.Windows.Forms.SaveFileDialog xmlSaveDialog;
    private System.Windows.Forms.ImageList imageListLocks;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControlTvChannels;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageTvChannels;
    private MediaPortal.UserInterface.Controls.MPButton buttonLookup;
    private MediaPortal.UserInterface.Controls.MPButton buttonRestore;
    private MediaPortal.UserInterface.Controls.MPButton buttonBackup;
    private MediaPortal.UserInterface.Controls.MPButton buttonAddCvbsSvhs;
    private MediaPortal.UserInterface.Controls.MPListView listViewTvChannels;
    private MediaPortal.UserInterface.Controls.MPButton buttonImportFromTvGuide;
    private MediaPortal.UserInterface.Controls.MPButton buttonClearChannels;
    private MediaPortal.UserInterface.Controls.MPButton buttonAddChannel;
    private MediaPortal.UserInterface.Controls.MPButton buttonDeleteChannel;
    private MediaPortal.UserInterface.Controls.MPButton buttonEditChannel;
    private MediaPortal.UserInterface.Controls.MPButton buttonChannelUp;
    private MediaPortal.UserInterface.Controls.MPButton buttonChannelDown;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageTvCards;
    private MediaPortal.UserInterface.Controls.MPButton buttonMap;
    private MediaPortal.UserInterface.Controls.MPButton buttonUnmap;
    private MediaPortal.UserInterface.Controls.MPListView listViewTVChannelsForCard;
    private MediaPortal.UserInterface.Controls.MPListView listViewTVChannelsCard;
    private MediaPortal.UserInterface.Controls.MPLabel labelMapChannelsToTvCard;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxCard;
    private MediaPortal.UserInterface.Controls.MPButton buttonCombine;
    private System.Windows.Forms.ColumnHeader columnHeaderChannelName;
    private System.Windows.Forms.ColumnHeader columnHeaderChannel;
    private System.Windows.Forms.ColumnHeader columnHeaderStandard;
    private System.Windows.Forms.ColumnHeader columnHeaderType;
    private System.Windows.Forms.ColumnHeader columnHeaderAssignedTvChannels;
    private System.Windows.Forms.ColumnHeader columnHeaderAvailableTvChannels;

    //
    // Private members
    //
    private bool _init = false;
    private bool _itemsModified = false;
    private ListViewColumnSorter _columnSorter;
    private MediaPortal.UserInterface.Controls.MPButton buttonDeleteScrambled;
    private static bool _reloadList = false;

    public SectionTvChannels()
      : this("TV Channels")
    {
    }

    public SectionTvChannels(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_init == false)
      {
        _init = true;
        LoadSettings();
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
    this.components = new System.ComponentModel.Container();
    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SectionTvChannels));
    this.imageListLocks = new System.Windows.Forms.ImageList(this.components);
    this.xmlOpenDialog = new System.Windows.Forms.OpenFileDialog();
    this.xmlSaveDialog = new System.Windows.Forms.SaveFileDialog();
    this.tabControlTvChannels = new MediaPortal.UserInterface.Controls.MPTabControl();
    this.tabPageTvChannels = new MediaPortal.UserInterface.Controls.MPTabPage();
    this.buttonDeleteScrambled = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonCombine = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonLookup = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonRestore = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonBackup = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonAddCvbsSvhs = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonImportFromTvGuide = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonClearChannels = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonAddChannel = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonDeleteChannel = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonEditChannel = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonChannelUp = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonChannelDown = new MediaPortal.UserInterface.Controls.MPButton();
    this.listViewTvChannels = new MediaPortal.UserInterface.Controls.MPListView();
    this.columnHeaderChannelName = new System.Windows.Forms.ColumnHeader();
    this.columnHeaderChannel = new System.Windows.Forms.ColumnHeader();
    this.columnHeaderStandard = new System.Windows.Forms.ColumnHeader();
    this.columnHeaderType = new System.Windows.Forms.ColumnHeader();
    this.tabPageTvCards = new MediaPortal.UserInterface.Controls.MPTabPage();
    this.buttonMap = new MediaPortal.UserInterface.Controls.MPButton();
    this.buttonUnmap = new MediaPortal.UserInterface.Controls.MPButton();
    this.listViewTVChannelsForCard = new MediaPortal.UserInterface.Controls.MPListView();
    this.columnHeaderAssignedTvChannels = new System.Windows.Forms.ColumnHeader();
    this.listViewTVChannelsCard = new MediaPortal.UserInterface.Controls.MPListView();
    this.columnHeaderAvailableTvChannels = new System.Windows.Forms.ColumnHeader();
    this.labelMapChannelsToTvCard = new MediaPortal.UserInterface.Controls.MPLabel();
    this.comboBoxCard = new MediaPortal.UserInterface.Controls.MPComboBox();
    this.tabControlTvChannels.SuspendLayout();
    this.tabPageTvChannels.SuspendLayout();
    this.tabPageTvCards.SuspendLayout();
    this.SuspendLayout();
    // 
    // imageListLocks
    // 
    this.imageListLocks.ImageStream = ( (System.Windows.Forms.ImageListStreamer)( resources.GetObject("imageListLocks.ImageStream") ) );
    this.imageListLocks.TransparentColor = System.Drawing.Color.Transparent;
    this.imageListLocks.Images.SetKeyName(0, "");
    this.imageListLocks.Images.SetKeyName(1, "");
    // 
    // xmlOpenDialog
    // 
    this.xmlOpenDialog.DefaultExt = "xml";
    this.xmlOpenDialog.FileName = "ChannelList";
    this.xmlOpenDialog.Filter = "xml|*.xml";
    this.xmlOpenDialog.InitialDirectory = ".";
    this.xmlOpenDialog.Title = "Open....";
    // 
    // xmlSaveDialog
    // 
    this.xmlSaveDialog.CreatePrompt = true;
    this.xmlSaveDialog.DefaultExt = "xml";
    this.xmlSaveDialog.FileName = "ChannelList";
    this.xmlSaveDialog.Filter = "xml|*.xml";
    this.xmlSaveDialog.InitialDirectory = ".";
    this.xmlSaveDialog.Title = "Save to....";
    // 
    // tabControlTvChannels
    // 
    this.tabControlTvChannels.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.tabControlTvChannels.Controls.Add(this.tabPageTvChannels);
    this.tabControlTvChannels.Controls.Add(this.tabPageTvCards);
    this.tabControlTvChannels.Location = new System.Drawing.Point(0, 0);
    this.tabControlTvChannels.Name = "tabControlTvChannels";
    this.tabControlTvChannels.SelectedIndex = 0;
    this.tabControlTvChannels.Size = new System.Drawing.Size(472, 408);
    this.tabControlTvChannels.TabIndex = 0;
    this.tabControlTvChannels.SelectedIndexChanged += new System.EventHandler(this.tabControlTvChannels_SelectedIndexChanged);
    // 
    // tabPageTvChannels
    // 
    this.tabPageTvChannels.AutoScroll = true;
    this.tabPageTvChannels.Controls.Add(this.buttonDeleteScrambled);
    this.tabPageTvChannels.Controls.Add(this.buttonCombine);
    this.tabPageTvChannels.Controls.Add(this.buttonLookup);
    this.tabPageTvChannels.Controls.Add(this.buttonRestore);
    this.tabPageTvChannels.Controls.Add(this.buttonBackup);
    this.tabPageTvChannels.Controls.Add(this.buttonAddCvbsSvhs);
    this.tabPageTvChannels.Controls.Add(this.buttonImportFromTvGuide);
    this.tabPageTvChannels.Controls.Add(this.buttonClearChannels);
    this.tabPageTvChannels.Controls.Add(this.buttonAddChannel);
    this.tabPageTvChannels.Controls.Add(this.buttonDeleteChannel);
    this.tabPageTvChannels.Controls.Add(this.buttonEditChannel);
    this.tabPageTvChannels.Controls.Add(this.buttonChannelUp);
    this.tabPageTvChannels.Controls.Add(this.buttonChannelDown);
    this.tabPageTvChannels.Controls.Add(this.listViewTvChannels);
    this.tabPageTvChannels.Location = new System.Drawing.Point(4, 22);
    this.tabPageTvChannels.Name = "tabPageTvChannels";
    this.tabPageTvChannels.Size = new System.Drawing.Size(464, 382);
    this.tabPageTvChannels.TabIndex = 0;
    this.tabPageTvChannels.Text = "TV Channels";
    this.tabPageTvChannels.UseVisualStyleBackColor = true;
    // 
    // buttonDeleteScrambled
    // 
    this.buttonDeleteScrambled.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.buttonDeleteScrambled.Location = new System.Drawing.Point(333, 355);
    this.buttonDeleteScrambled.Name = "buttonDeleteScrambled";
    this.buttonDeleteScrambled.Size = new System.Drawing.Size(115, 20);
    this.buttonDeleteScrambled.TabIndex = 13;
    this.buttonDeleteScrambled.Text = "Delete scrambled";
    this.buttonDeleteScrambled.UseVisualStyleBackColor = true;
    this.buttonDeleteScrambled.Click += new System.EventHandler(this.buttonDeleteScrambled_Click);
    // 
    // buttonCombine
    // 
    this.buttonCombine.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
    this.buttonCombine.Location = new System.Drawing.Point(16, 312);
    this.buttonCombine.Name = "buttonCombine";
    this.buttonCombine.Size = new System.Drawing.Size(60, 20);
    this.buttonCombine.TabIndex = 1;
    this.buttonCombine.Text = "Combine";
    this.buttonCombine.UseVisualStyleBackColor = true;
    this.buttonCombine.Click += new System.EventHandler(this.buttonCombine_Click);
    // 
    // buttonLookup
    // 
    this.buttonLookup.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.buttonLookup.Location = new System.Drawing.Point(256, 312);
    this.buttonLookup.Name = "buttonLookup";
    this.buttonLookup.Size = new System.Drawing.Size(60, 20);
    this.buttonLookup.TabIndex = 8;
    this.buttonLookup.Text = "Lookup";
    this.buttonLookup.UseVisualStyleBackColor = true;
    this.buttonLookup.Click += new System.EventHandler(this.buttonLookup_Click);
    // 
    // buttonRestore
    // 
    this.buttonRestore.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.buttonRestore.Location = new System.Drawing.Point(256, 355);
    this.buttonRestore.Name = "buttonRestore";
    this.buttonRestore.Size = new System.Drawing.Size(60, 20);
    this.buttonRestore.TabIndex = 10;
    this.buttonRestore.Text = "Restore";
    this.buttonRestore.UseVisualStyleBackColor = true;
    this.buttonRestore.Click += new System.EventHandler(this.buttonRestore_Click);
    // 
    // buttonBackup
    // 
    this.buttonBackup.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.buttonBackup.Location = new System.Drawing.Point(256, 333);
    this.buttonBackup.Name = "buttonBackup";
    this.buttonBackup.Size = new System.Drawing.Size(60, 20);
    this.buttonBackup.TabIndex = 9;
    this.buttonBackup.Text = "Backup";
    this.buttonBackup.UseVisualStyleBackColor = true;
    this.buttonBackup.Click += new System.EventHandler(this.buttonBackup_Click);
    // 
    // buttonAddCvbsSvhs
    // 
    this.buttonAddCvbsSvhs.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.buttonAddCvbsSvhs.Location = new System.Drawing.Point(333, 333);
    this.buttonAddCvbsSvhs.Name = "buttonAddCvbsSvhs";
    this.buttonAddCvbsSvhs.Size = new System.Drawing.Size(115, 20);
    this.buttonAddCvbsSvhs.TabIndex = 12;
    this.buttonAddCvbsSvhs.Text = "Add CVBS/SVHS";
    this.buttonAddCvbsSvhs.UseVisualStyleBackColor = true;
    this.buttonAddCvbsSvhs.Click += new System.EventHandler(this.buttonAddCvbsSvhs_Click);
    // 
    // buttonImportFromTvGuide
    // 
    this.buttonImportFromTvGuide.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.buttonImportFromTvGuide.Location = new System.Drawing.Point(333, 312);
    this.buttonImportFromTvGuide.Name = "buttonImportFromTvGuide";
    this.buttonImportFromTvGuide.Size = new System.Drawing.Size(115, 20);
    this.buttonImportFromTvGuide.TabIndex = 11;
    this.buttonImportFromTvGuide.Text = "Import from tvguide";
    this.buttonImportFromTvGuide.UseVisualStyleBackColor = true;
    this.buttonImportFromTvGuide.Click += new System.EventHandler(this.buttonImportFromTvGuide_Click);
    // 
    // buttonClearChannels
    // 
    this.buttonClearChannels.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
    this.buttonClearChannels.Location = new System.Drawing.Point(16, 355);
    this.buttonClearChannels.Name = "buttonClearChannels";
    this.buttonClearChannels.Size = new System.Drawing.Size(60, 20);
    this.buttonClearChannels.TabIndex = 3;
    this.buttonClearChannels.Text = "Clear";
    this.buttonClearChannels.UseVisualStyleBackColor = true;
    this.buttonClearChannels.Click += new System.EventHandler(this.buttonClearChannels_Click);
    // 
    // buttonAddChannel
    // 
    this.buttonAddChannel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
    this.buttonAddChannel.Location = new System.Drawing.Point(16, 333);
    this.buttonAddChannel.Name = "buttonAddChannel";
    this.buttonAddChannel.Size = new System.Drawing.Size(60, 20);
    this.buttonAddChannel.TabIndex = 2;
    this.buttonAddChannel.Text = "Add";
    this.buttonAddChannel.UseVisualStyleBackColor = true;
    this.buttonAddChannel.Click += new System.EventHandler(this.buttonAddChannel_Click);
    // 
    // buttonDeleteChannel
    // 
    this.buttonDeleteChannel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
    this.buttonDeleteChannel.Enabled = false;
    this.buttonDeleteChannel.Location = new System.Drawing.Point(86, 355);
    this.buttonDeleteChannel.Name = "buttonDeleteChannel";
    this.buttonDeleteChannel.Size = new System.Drawing.Size(60, 20);
    this.buttonDeleteChannel.TabIndex = 5;
    this.buttonDeleteChannel.Text = "Delete";
    this.buttonDeleteChannel.UseVisualStyleBackColor = true;
    this.buttonDeleteChannel.Click += new System.EventHandler(this.buttonDeleteChannel_Click);
    // 
    // buttonEditChannel
    // 
    this.buttonEditChannel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
    this.buttonEditChannel.Enabled = false;
    this.buttonEditChannel.Location = new System.Drawing.Point(86, 333);
    this.buttonEditChannel.Name = "buttonEditChannel";
    this.buttonEditChannel.Size = new System.Drawing.Size(60, 20);
    this.buttonEditChannel.TabIndex = 4;
    this.buttonEditChannel.Text = "Edit";
    this.buttonEditChannel.UseVisualStyleBackColor = true;
    this.buttonEditChannel.Click += new System.EventHandler(this.buttonEditChannel_Click);
    // 
    // buttonChannelUp
    // 
    this.buttonChannelUp.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
    this.buttonChannelUp.Enabled = false;
    this.buttonChannelUp.Location = new System.Drawing.Point(156, 333);
    this.buttonChannelUp.Name = "buttonChannelUp";
    this.buttonChannelUp.Size = new System.Drawing.Size(60, 20);
    this.buttonChannelUp.TabIndex = 6;
    this.buttonChannelUp.Text = "Up";
    this.buttonChannelUp.UseVisualStyleBackColor = true;
    this.buttonChannelUp.Click += new System.EventHandler(this.buttonChannelUp_Click);
    // 
    // buttonChannelDown
    // 
    this.buttonChannelDown.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
    this.buttonChannelDown.Enabled = false;
    this.buttonChannelDown.Location = new System.Drawing.Point(156, 355);
    this.buttonChannelDown.Name = "buttonChannelDown";
    this.buttonChannelDown.Size = new System.Drawing.Size(60, 20);
    this.buttonChannelDown.TabIndex = 7;
    this.buttonChannelDown.Text = "Down";
    this.buttonChannelDown.UseVisualStyleBackColor = true;
    this.buttonChannelDown.Click += new System.EventHandler(this.buttonChannelDown_Click);
    // 
    // listViewTvChannels
    // 
    this.listViewTvChannels.AllowDrop = true;
    this.listViewTvChannels.AllowRowReorder = true;
    this.listViewTvChannels.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.listViewTvChannels.CheckBoxes = true;
    this.listViewTvChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderChannelName,
            this.columnHeaderChannel,
            this.columnHeaderStandard,
            this.columnHeaderType});
    this.listViewTvChannels.FullRowSelect = true;
    this.listViewTvChannels.HideSelection = false;
    this.listViewTvChannels.Location = new System.Drawing.Point(16, 16);
    this.listViewTvChannels.Name = "listViewTvChannels";
    this.listViewTvChannels.Size = new System.Drawing.Size(432, 294);
    this.listViewTvChannels.SmallImageList = this.imageListLocks;
    this.listViewTvChannels.TabIndex = 0;
    this.listViewTvChannels.UseCompatibleStateImageBehavior = false;
    this.listViewTvChannels.View = System.Windows.Forms.View.Details;
    this.listViewTvChannels.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewTvChannels_ItemChecked);
    this.listViewTvChannels.DoubleClick += new System.EventHandler(this.listViewTvChannels_DoubleClick);
    this.listViewTvChannels.SelectedIndexChanged += new System.EventHandler(this.listViewTvChannels_SelectedIndexChanged);
    this.listViewTvChannels.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewTvChannels_ColumnClick);
    // 
    // columnHeaderChannelName
    // 
    this.columnHeaderChannelName.Text = "Channel name";
    this.columnHeaderChannelName.Width = 236;
    // 
    // columnHeaderChannel
    // 
    this.columnHeaderChannel.Text = "Channel";
    this.columnHeaderChannel.Width = 64;
    // 
    // columnHeaderStandard
    // 
    this.columnHeaderStandard.Text = "Standard";
    this.columnHeaderStandard.Width = 64;
    // 
    // columnHeaderType
    // 
    this.columnHeaderType.Text = "Type";
    this.columnHeaderType.Width = 64;
    // 
    // tabPageTvCards
    // 
    this.tabPageTvCards.AutoScroll = true;
    this.tabPageTvCards.Controls.Add(this.buttonMap);
    this.tabPageTvCards.Controls.Add(this.buttonUnmap);
    this.tabPageTvCards.Controls.Add(this.listViewTVChannelsForCard);
    this.tabPageTvCards.Controls.Add(this.listViewTVChannelsCard);
    this.tabPageTvCards.Controls.Add(this.labelMapChannelsToTvCard);
    this.tabPageTvCards.Controls.Add(this.comboBoxCard);
    this.tabPageTvCards.Location = new System.Drawing.Point(4, 22);
    this.tabPageTvCards.Name = "tabPageTvCards";
    this.tabPageTvCards.Size = new System.Drawing.Size(464, 382);
    this.tabPageTvCards.TabIndex = 3;
    this.tabPageTvCards.Text = "TV Cards";
    this.tabPageTvCards.UseVisualStyleBackColor = true;
    this.tabPageTvCards.Visible = false;
    // 
    // buttonMap
    // 
    this.buttonMap.Anchor = System.Windows.Forms.AnchorStyles.None;
    this.buttonMap.Location = new System.Drawing.Point(212, 168);
    this.buttonMap.Name = "buttonMap";
    this.buttonMap.Size = new System.Drawing.Size(40, 22);
    this.buttonMap.TabIndex = 3;
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
    this.buttonUnmap.TabIndex = 4;
    this.buttonUnmap.Text = "<<";
    this.buttonUnmap.UseVisualStyleBackColor = true;
    this.buttonUnmap.Click += new System.EventHandler(this.buttonUnmap_Click);
    // 
    // listViewTVChannelsForCard
    // 
    this.listViewTVChannelsForCard.AllowDrop = true;
    this.listViewTVChannelsForCard.AllowRowReorder = true;
    this.listViewTVChannelsForCard.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.listViewTVChannelsForCard.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAssignedTvChannels});
    this.listViewTVChannelsForCard.Location = new System.Drawing.Point(272, 56);
    this.listViewTVChannelsForCard.Name = "listViewTVChannelsForCard";
    this.listViewTVChannelsForCard.Size = new System.Drawing.Size(176, 304);
    this.listViewTVChannelsForCard.TabIndex = 5;
    this.listViewTVChannelsForCard.UseCompatibleStateImageBehavior = false;
    this.listViewTVChannelsForCard.View = System.Windows.Forms.View.Details;
    this.listViewTVChannelsForCard.DoubleClick += new System.EventHandler(this.listViewTVChannelsForCard_DoubleClick);
    // 
    // columnHeaderAssignedTvChannels
    // 
    this.columnHeaderAssignedTvChannels.Text = "Assigned TV Channels";
    this.columnHeaderAssignedTvChannels.Width = 154;
    // 
    // listViewTVChannelsCard
    // 
    this.listViewTVChannelsCard.AllowDrop = true;
    this.listViewTVChannelsCard.AllowRowReorder = true;
    this.listViewTVChannelsCard.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                | System.Windows.Forms.AnchorStyles.Left ) ) );
    this.listViewTVChannelsCard.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAvailableTvChannels});
    this.listViewTVChannelsCard.Location = new System.Drawing.Point(16, 56);
    this.listViewTVChannelsCard.Name = "listViewTVChannelsCard";
    this.listViewTVChannelsCard.Size = new System.Drawing.Size(176, 304);
    this.listViewTVChannelsCard.TabIndex = 2;
    this.listViewTVChannelsCard.UseCompatibleStateImageBehavior = false;
    this.listViewTVChannelsCard.View = System.Windows.Forms.View.Details;
    this.listViewTVChannelsCard.DoubleClick += new System.EventHandler(this.listViewTVChannelsCard_DoubleClick);
    // 
    // columnHeaderAvailableTvChannels
    // 
    this.columnHeaderAvailableTvChannels.Text = "Available TV Channels";
    this.columnHeaderAvailableTvChannels.Width = 154;
    // 
    // labelMapChannelsToTvCard
    // 
    this.labelMapChannelsToTvCard.AutoSize = true;
    this.labelMapChannelsToTvCard.Location = new System.Drawing.Point(16, 24);
    this.labelMapChannelsToTvCard.Name = "labelMapChannelsToTvCard";
    this.labelMapChannelsToTvCard.Size = new System.Drawing.Size(130, 13);
    this.labelMapChannelsToTvCard.TabIndex = 0;
    this.labelMapChannelsToTvCard.Text = "Map channels to TV card:";
    // 
    // comboBoxCard
    // 
    this.comboBoxCard.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.comboBoxCard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
    this.comboBoxCard.Location = new System.Drawing.Point(160, 20);
    this.comboBoxCard.Name = "comboBoxCard";
    this.comboBoxCard.Size = new System.Drawing.Size(288, 21);
    this.comboBoxCard.TabIndex = 1;
    this.comboBoxCard.SelectedIndexChanged += new System.EventHandler(this.comboBoxCard_SelectedIndexChanged);
    // 
    // SectionTvChannels
    // 
    this.Controls.Add(this.tabControlTvChannels);
    this.Name = "SectionTvChannels";
    this.Size = new System.Drawing.Size(472, 408);
    this.tabControlTvChannels.ResumeLayout(false);
    this.tabPageTvChannels.ResumeLayout(false);
    this.tabPageTvCards.ResumeLayout(false);
    this.tabPageTvCards.PerformLayout();
    this.ResumeLayout(false);

    }
    #endregion


    private void buttonAddChannel_Click(object sender, System.EventArgs e)
    {
      _itemsModified = true;

      EditTVChannelForm editChannel = new EditTVChannelForm();

      editChannel.SortingPlace = listViewTvChannels.Items.Count;
      DialogResult dialogResult = editChannel.ShowDialog(this);

      if (dialogResult == DialogResult.OK)
        LoadSettings();
    }

    private string GetStandardName(AnalogVideoStandard standard)
    {
      string name = standard.ToString();
      name = name.Replace("_", " ");
      return name == "None" ? "Default" : name;
    }

    private void buttonEditChannel_Click(object sender, System.EventArgs e)
    {
      _itemsModified = true;

      foreach (ListViewItem listItem in listViewTvChannels.SelectedItems)
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

    private void buttonDeleteChannel_Click(object sender, System.EventArgs e)
    {
      listViewTvChannels.BeginUpdate();

      _itemsModified = true;

      int itemCount = listViewTvChannels.SelectedItems.Count;

      for (int index = 0; index < itemCount; index++)
        listViewTvChannels.Items.RemoveAt(listViewTvChannels.SelectedIndices[0]);

      SaveSettings();

      listViewTvChannels.EndUpdate();
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

        foreach (ListViewItem item in listViewTvChannels.Items)
        {
          TelevisionChannel channel = item.Tag as TelevisionChannel;
          if ((channel != null) &&
            (channel.Channel < (int)ExternalInputs.svhs) &&
            (channel.Channel > highestChannelNumber))
            highestChannelNumber = channel.Channel;
        }
        return highestChannelNumber;
      }
    }

    public override void LoadSettings()
    {
      if (!_init)
        return;

      LoadTVChannels();
      LoadCards();
    }

    public override void SaveSettings()
    {
      if (!_init)
        return;

      if (_reloadList)
      {
        LoadTVChannels();
        LoadCards();
        _reloadList = false;
        _itemsModified = true;
      }
      SaveTVChannels();
    }

    private void SaveTVChannels()
    {
      if (_itemsModified)
      {
        int countryCode = 31;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          countryCode = xmlreader.GetValueAsInt("capture", "country", 31);

        RegistryKey registryKey = Registry.LocalMachine;

        string[] registryLocations = new string[] { String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-1", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-0", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-1"),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-0")};
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
            foreach (ListViewItem listItem in listViewTvChannels.Items)
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
              registryKey.DeleteValue(channelIndex.ToString(), false);

            registryKey.Close();
          }
        }

        //
        // Add current channels
        //
        TVDatabase.GetChannels(ref channels);
        foreach (ListViewItem listItem in listViewTvChannels.Items)
        {
          MediaPortal.TV.Database.TVChannel channel = new MediaPortal.TV.Database.TVChannel();
          TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;

          if (tvChannel != null)
          {
            channel.Name = tvChannel.Name;

            //does channel already exists in database?
            bool exists = false;
            foreach (TVChannel chan in channels)
              if (String.Compare(chan.Name, channel.Name, true) == 0)
              {
                exists = true;
                channel = chan.Clone();
                break;
              }

            channel.Number = tvChannel.Channel;
            channel.VisibleInGuide = tvChannel.VisibleInGuide;
            channel.Country = tvChannel.Country;
            channel.ID = tvChannel.ID;

            //
            // Calculate frequency
            //
            if (tvChannel.Frequency.Hertz < 1000)
              tvChannel.Frequency.Hertz *= 1000000L;

            channel.Frequency = tvChannel.Frequency.Hertz;
            channel.External = tvChannel.External;
            channel.ExternalTunerChannel = tvChannel.ExternalTunerChannel;
            channel.TVStandard = tvChannel.standard;
            channel.Scrambled = tvChannel.Scrambled;

            if (exists)
              TVDatabase.UpdateChannel(channel, listItem.Index);
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

          foreach (ListViewItem listItem in listViewTvChannels.Items)
          {
            TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;

            if (tvChannel != null)
            {
              //
              // Don't add frequency to the registry if it has no frequency or if we have the predefined
              // channels for Composite and SVIDEO
              //
              if (tvChannel.Frequency.Hertz > 0 &&
                tvChannel.Channel != (int)ExternalInputs.svhs &&
                tvChannel.Channel != (int)ExternalInputs.cvbs1 &&
                tvChannel.Channel != (int)ExternalInputs.cvbs2 &&
                tvChannel.Channel != (int)ExternalInputs.rgb)
              {
                registryKey.SetValue(tvChannel.Channel.ToString(), (int)tvChannel.Frequency.Hertz);
              }
            }
          }
          registryKey.Close();
        }
      }
    }

    private void AddChannel(ref ArrayList channels, string strName, int iNumber)
    {
      _itemsModified = true;

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
      listViewTvChannels.BeginUpdate();

      listViewTvChannels.Items.Clear();
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
                                    tvChannel.External ? "External" : "Internal"});
        listItem.Checked = tvChannel.VisibleInGuide;
        listItem.ImageIndex = 0;
        if (tvChannel.Scrambled)
          listItem.ImageIndex = 1;

        listItem.Tag = tvChannel;

        listViewTvChannels.Items.Add(listItem);
      }

      listViewTvChannels.EndUpdate();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void listViewTvChannels_DoubleClick(object sender, System.EventArgs e)
    {
      listViewTvChannels.SelectedItems[0].Checked = !listViewTvChannels.SelectedItems[0].Checked;
      buttonEditChannel_Click(sender, e);
    }

    private void buttonChannelUp_Click(object sender, System.EventArgs e)
    {
      listViewTvChannels.BeginUpdate();

      _itemsModified = true;

      for (int index = 0; index < listViewTvChannels.Items.Count; index++)
      {
        if (listViewTvChannels.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't smaller than the lowest index (0) in the list view
          //
          if (index > 0)
          {
            ListViewItem listItem = listViewTvChannels.Items[index];
            listViewTvChannels.Items.RemoveAt(index);
            listViewTvChannels.Items.Insert(index - 1, listItem);
          }
        }
      }

      listViewTvChannels.EndUpdate();
    }

    private void buttonChannelDown_Click(object sender, System.EventArgs e)
    {
      listViewTvChannels.BeginUpdate();

      _itemsModified = true;

      for (int index = listViewTvChannels.Items.Count - 1; index >= 0; index--)
        if (listViewTvChannels.Items[index].Selected)
        {
          //
          // Make sure the current index isn't greater than the highest index in the list view
          //
          if (index < listViewTvChannels.Items.Count - 1)
          {
            ListViewItem listItem = listViewTvChannels.Items[index];
            listViewTvChannels.Items.RemoveAt(index);

            if (index + 1 < listViewTvChannels.Items.Count)
              listViewTvChannels.Items.Insert(index + 1, listItem);
            else
              listViewTvChannels.Items.Add(listItem);
          }
        }

      listViewTvChannels.EndUpdate();
    }

    private void listViewTvChannels_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      buttonDeleteChannel.Enabled = buttonEditChannel.Enabled = buttonChannelUp.Enabled = buttonChannelDown.Enabled = (listViewTvChannels.SelectedItems.Count > 0);
      buttonCombine.Enabled = (listViewTvChannels.SelectedItems.Count == 2);
    }

    private void buttonImportFromTvGuide_Click(object sender, System.EventArgs e)
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
          MessageBox.Show(this, strtext, "TV Guide", MessageBoxButtons.OK, MessageBoxIcon.Error);

          _itemsModified = true;
          LoadTVChannels();
        }
        else
        {
          string strError = String.Format("Error importing TV guide from:\r{0}\rError: {1}",
                              strTVGuideFile, import.ImportStats.Status);
          MessageBox.Show(this, strError, "Error importing TV guide", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    private string RemoveTrailingSlash(string strLine)
    {
      string strPath = strLine;

      while (strPath.Length > 0)
        if (strPath[strPath.Length - 1] == '\\' || strPath[strPath.Length - 1] == '/')
          strPath = strPath.Substring(0, strPath.Length - 1);
        else
          break;

      return strPath;
    }

    private void buttonClearChannels_Click(object sender, System.EventArgs e)
    {
      DialogResult result = MessageBox.Show(this, "Are you sure you want to delete all channels?", "Delete channels", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (result != DialogResult.Yes)
        return;
      listViewTvChannels.Items.Clear();
      SaveSettings();
    }

    static public void UpdateList()
    {
      _reloadList = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      if (_reloadList)
      {
        _reloadList = false;
        LoadTVChannels();
        LoadCards();
      }
      base.OnPaint(e);
    }

    private void listViewTvChannels_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
    {
      if (_columnSorter == null)
        listViewTvChannels.ListViewItemSorter = _columnSorter = new ListViewColumnSorter();

      _columnSorter.IsColumnNumeric = e.Column == 1;

      // Determine if clicked column is already the column that is being sorted.
      if (e.Column == _columnSorter.SortColumn)
        _columnSorter.Order = _columnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        _columnSorter.SortColumn = e.Column;
        _columnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      listViewTvChannels.Sort();
      listViewTvChannels.Update();
    }

    private void buttonAddCvbsSvhs_Click(object sender, System.EventArgs e)
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
      comboBoxCard.BeginUpdate();

      comboBoxCard.Items.Clear();
      if (File.Exists("capturecards.xml"))
        using (FileStream fileStream = new FileStream("capturecards.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

              TVCaptureDevice device = (TVCaptureDevice)captureCards[i];
              ComboCard combo = new ComboCard();
              combo.FriendlyName = device.FriendlyName;
              combo.VideoDevice = device.VideoDevice;
              combo.ID = device.ID;
              comboBoxCard.Items.Add(combo);
            }
          }
          catch
          {
            MessageBox.Show("Failed to load previously configured capture card(s), you have to reconfigure your device(s).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }

      if (comboBoxCard.Items.Count != 0)
        comboBoxCard.SelectedIndex = 0;

      comboBoxCard.EndUpdate();

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

      listViewTVChannelsCard.BeginUpdate();
      listViewTVChannelsForCard.BeginUpdate();

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
          if (chanCard.Name == chan.Name)
          {
            mapped = true;
            break;
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

      listViewTVChannelsForCard.EndUpdate();
      listViewTVChannelsCard.EndUpdate();
    }

    private void comboBoxCard_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      FillInChannelCardMappings();
    }

    private void buttonMap_Click(object sender, System.EventArgs e)
    {
      if (listViewTVChannelsCard.SelectedItems == null)
        return;

      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard)comboBoxCard.Items[index];
        card = combo.ID;
      }

      listViewTVChannelsForCard.BeginUpdate();
      listViewTVChannelsCard.BeginUpdate();

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

      listViewTVChannelsCard.EndUpdate();
      listViewTVChannelsForCard.EndUpdate();
    }

    private void buttonUnmap_Click(object sender, System.EventArgs e)
    {
      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard)comboBoxCard.Items[index];
        card = combo.ID;
      }

      if (listViewTVChannelsForCard.SelectedItems == null)
        return;

      listViewTVChannelsForCard.BeginUpdate();
      listViewTVChannelsCard.BeginUpdate();

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

      listViewTVChannelsCard.EndUpdate();
      listViewTVChannelsForCard.EndUpdate();
    }

    private void tabControlTvChannels_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SaveSettings();
      LoadSettings();
    }

    private int ExportChannels(MediaPortal.Profile.Settings exportFile)
    {
      int exportedChannelsCounter = 0;

      if (listViewTvChannels.Items.Count > 0)
      {
        foreach (ListViewItem listItem in listViewTvChannels.Items)
        {
          TelevisionChannel selectedChannel = listItem.Tag as TelevisionChannel;

          //Set index
          exportFile.SetValue(listItem.Index.ToString(), "INDEX", listItem.Index.ToString());

          //Channel data
          exportFile.SetValueAsBool(listItem.Index.ToString(), "Scrambled", selectedChannel.Scrambled);
          exportFile.SetValue(listItem.Index.ToString(), "ID", selectedChannel.ID.ToString());
          exportFile.SetValue(listItem.Index.ToString(), "Number", selectedChannel.Channel.ToString());
          exportFile.SetValue(listItem.Index.ToString(), "Name", selectedChannel.Name.ToString());
          exportFile.SetValue(listItem.Index.ToString(), "Country", selectedChannel.Country.ToString());
          exportFile.SetValueAsBool(listItem.Index.ToString(), "External", selectedChannel.External);
          exportFile.SetValue(listItem.Index.ToString(), "External Tuner Channel", selectedChannel.ExternalTunerChannel.ToString());
          exportFile.SetValue(listItem.Index.ToString(), "Frequency", selectedChannel.Frequency.Hertz.ToString());
          exportFile.SetValue(listItem.Index.ToString(), "Analog Standard Index", selectedChannel.standard.ToString());
          exportFile.SetValueAsBool(listItem.Index.ToString(), "Visible in Guide", selectedChannel.VisibleInGuide);

          if (selectedChannel.Channel >= 0)
          {
            int bandWidth, freq, onId, tsId, sId, symbolRate, innerFec, modulation, audioPid, videoPid, teletextPid, pmtPid;
            string provider;
            int audio1, audio2, audio3, ac3Pid, pcrPid;
            string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
            bool hasEitPresentFollow, hasEitSchedule;
            //DVB-T
            TVDatabase.GetDVBTTuneRequest(selectedChannel.ID, out provider, out freq, out onId, out tsId, out sId, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth,
                                          out audio1, out audio2, out audio3, out ac3Pid,
                                          out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3,
                                          out hasEitPresentFollow, out hasEitSchedule, out pcrPid);
            exportFile.SetValue(listItem.Index.ToString(), "DVBTProvider", provider);
            exportFile.SetValue(listItem.Index.ToString(), "DVBTFreq", freq.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTONID", onId.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTTSID", tsId.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTSID", sId.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTAudioPid", audioPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTVideoPid", videoPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTTeletextPid", teletextPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTPmtPid", pmtPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTBandwidth", bandWidth.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTAudio1Pid", audio1.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTAudio2Pid", audio2.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTAudio3Pid", audio3.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTAC3Pid", ac3Pid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBTAudioLanguage", audioLanguage);
            exportFile.SetValue(listItem.Index.ToString(), "DVBTAudioLanguage1", audioLanguage1);
            exportFile.SetValue(listItem.Index.ToString(), "DVBTAudioLanguage2", audioLanguage2);
            exportFile.SetValue(listItem.Index.ToString(), "DVBTAudioLanguage3", audioLanguage3);
            exportFile.SetValueAsBool(listItem.Index.ToString(), "DVBTHasEITPresentFollow", hasEitPresentFollow);
            exportFile.SetValueAsBool(listItem.Index.ToString(), "DVBTHasEITSchedule", hasEitSchedule);
            exportFile.SetValue(listItem.Index.ToString(), "DVBTPCRPid", pcrPid.ToString());

            //DVB-C
            TVDatabase.GetDVBCTuneRequest(selectedChannel.ID, out provider, out freq, out symbolRate, out innerFec, out modulation,
                                          out onId, out tsId, out sId, out audioPid, out videoPid, out teletextPid, out pmtPid,
                                          out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1,
                                          out audioLanguage2, out audioLanguage3, out hasEitPresentFollow, out hasEitSchedule, out pcrPid);
            exportFile.SetValue(listItem.Index.ToString(), "DVBCProvider", provider);
            exportFile.SetValue(listItem.Index.ToString(), "DVBCFreq", freq.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCSR", symbolRate.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCInnerFeq", innerFec.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCModulation", modulation.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCONID", onId.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCTSID", tsId.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCSID", sId.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCAudioPid", audioPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCVideoPid", videoPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCTeletextPid", teletextPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCPmtPid", pmtPid.ToString());

            exportFile.SetValue(listItem.Index.ToString(), "DVBCAudio1Pid", audio1.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCAudio2Pid", audio2.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCAudio3Pid", audio3.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCAC3Pid", ac3Pid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBCAudioLanguage", audioLanguage);
            exportFile.SetValue(listItem.Index.ToString(), "DVBCAudioLanguage1", audioLanguage1);
            exportFile.SetValue(listItem.Index.ToString(), "DVBCAudioLanguage2", audioLanguage2);
            exportFile.SetValue(listItem.Index.ToString(), "DVBCAudioLanguage3", audioLanguage3);
            exportFile.SetValueAsBool(listItem.Index.ToString(), "DVBCHasEITPresentFollow", hasEitPresentFollow);
            exportFile.SetValueAsBool(listItem.Index.ToString(), "DVBCHasEITSchedule", hasEitSchedule);
            exportFile.SetValue(listItem.Index.ToString(), "DVBCPCRPid", pcrPid.ToString());

            //DVB-S
            DVBChannel channel = new DVBChannel();
            TVDatabase.GetSatChannel(selectedChannel.ID, 1, ref channel);
            exportFile.SetValue(listItem.Index.ToString(), "DVBSFreq", channel.Frequency.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSONID", channel.NetworkID.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSTSID", channel.TransportStreamID.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSSID", channel.ProgramNumber.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSSymbolrate", channel.Symbolrate.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSInnerFec", channel.FEC.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSPolarisation", channel.Polarity.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSProvider", channel.ServiceProvider);
            exportFile.SetValue(listItem.Index.ToString(), "DVBSAudioPid", channel.AudioPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSVideoPid", channel.VideoPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSTeletextPid", channel.TeletextPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSECMpid", channel.ECMPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSPmtPid", channel.PMTPid.ToString()); //giovortu

            exportFile.SetValue(listItem.Index.ToString(), "DVBSAudio1Pid", channel.Audio1.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSAudio2Pid", channel.Audio2.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSAudio3Pid", channel.Audio3.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSAC3Pid", channel.AC3Pid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSPCRPid", channel.PCRPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "DVBSAudioLanguage", channel.AudioLanguage);
            exportFile.SetValue(listItem.Index.ToString(), "DVBSAudioLanguage1", channel.AudioLanguage1);
            exportFile.SetValue(listItem.Index.ToString(), "DVBSAudioLanguage2", channel.AudioLanguage2);
            exportFile.SetValue(listItem.Index.ToString(), "DVBSAudioLanguage3", channel.AudioLanguage3);
            exportFile.SetValueAsBool(listItem.Index.ToString(), "DVBSHasEITPresentFollow", channel.HasEITPresentFollow);
            exportFile.SetValueAsBool(listItem.Index.ToString(), "DVBSHasEITSchedule", channel.HasEITSchedule);

            int minorChannel, majorChannel, physicalChannel;
            TVDatabase.GetATSCTuneRequest(selectedChannel.ID, out physicalChannel, out provider, out freq, out symbolRate, out innerFec,
                                          out modulation, out onId, out tsId, out sId, out audioPid, out videoPid, out teletextPid, out pmtPid,
                                          out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1,
                                          out audioLanguage2, out audioLanguage3, out minorChannel, out majorChannel,
                                          out hasEitPresentFollow, out hasEitSchedule, out pcrPid);

            exportFile.SetValue(listItem.Index.ToString(), "ATSCPhysical", physicalChannel.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCProvider", provider);
            exportFile.SetValue(listItem.Index.ToString(), "ATSCFreq", freq.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCSymbolrate", symbolRate.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCInnerFec", innerFec.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCModulation", modulation.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCONID", onId.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCTSID", tsId.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCSID", sId.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCAudioPid", audioPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCVideoPid", videoPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCTeletextPid", teletextPid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCPmtPid", pmtPid.ToString());

            exportFile.SetValue(listItem.Index.ToString(), "ATSCAudio1Pid", audio1.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCAudio2Pid", audio2.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCAudio3Pid", audio3.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCAC3Pid", ac3Pid.ToString());
            exportFile.SetValue(listItem.Index.ToString(), "ATSCAudioLanguage", audioLanguage);
            exportFile.SetValue(listItem.Index.ToString(), "ATSCAudioLanguage1", audioLanguage1);
            exportFile.SetValue(listItem.Index.ToString(), "ATSCAudioLanguage2", audioLanguage2);
            exportFile.SetValue(listItem.Index.ToString(), "ATSCAudioLanguage3", audioLanguage3);
            exportFile.SetValue(listItem.Index.ToString(), "ATSCMinor", minorChannel);
            exportFile.SetValue(listItem.Index.ToString(), "ATSCMajor", majorChannel);
            exportFile.SetValueAsBool(listItem.Index.ToString(), "ATSCHasEITPresentFollow", hasEitPresentFollow);
            exportFile.SetValueAsBool(listItem.Index.ToString(), "ATSCHasEITSchedule", hasEitSchedule);
            exportFile.SetValue(listItem.Index.ToString(), "ATSCPCRPid", pcrPid.ToString());
          }
          exportedChannelsCounter++;
        }
      }
      return exportedChannelsCounter;
    }

    private int ExportGroups(MediaPortal.Profile.Settings exportFile)
    {
      int exportedGroupsCounter = 0;

      //Group Data and channel maping
      ArrayList groups = new ArrayList();
      TVDatabase.GetGroups(ref groups);
      //Save total groups for reference
      if (groups.Count > 0)
      {
        exportFile.SetValue("GROUPS", "TOTAL", groups.Count.ToString());
      }
      int count = 1;
      foreach (TVGroup group in groups)
      {
        exportFile.SetValue("Group " + count.ToString(), "ID", group.ID.ToString());
        exportFile.SetValue("Group " + count.ToString(), "NAME", group.GroupName);
        exportFile.SetValue("Group " + count.ToString(), "PINCODE", group.Pincode.ToString());
        exportFile.SetValue("Group " + count.ToString(), "SORT", group.Sort.ToString());

        //Save total channels added to this group
        //TVDatabase.GetTVChannelsForGroup(group);
        exportFile.SetValue("Group " + count.ToString(), "TOTAL CHANNELS", group.TvChannels.Count.ToString());

        //Save current channel ID's under this group
        int channelIndex = 0;
        foreach (TVChannel tvChannel in group.TvChannels)
        {
          exportFile.SetValue("Group " + count.ToString(), "CHANNEL " + channelIndex.ToString(), tvChannel.ID.ToString());
          channelIndex++;
        }
        count++;
      }

      exportedGroupsCounter = count - 1;

      return exportedGroupsCounter;
    }

    private int ExportCards(MediaPortal.Profile.Settings exportFile)
    {
      int exportedCardsCounter = 0;

      //Card mapping data
      ArrayList cards = new ArrayList();
      TVDatabase.GetCards(ref cards);

      //If we have no cards skip this
      if (cards.Count > 0)
        exportFile.SetValue("CARDS", "TOTAL", cards.Count.ToString());

      for (int i = 1; i < cards.Count + 1; i++)
        try
        {
          //ArrayList Channels = new ArrayList();
          //TVDatabase.GetChannels(ref Channels);
          ArrayList tmpChannels = new ArrayList();
          TVDatabase.GetChannelsForCard(ref tmpChannels, i);
          exportFile.SetValue("Card " + i.ToString(), "ID", i.ToString());
          exportFile.SetValue("Card " + i.ToString(), "TOTAL CHANNELS", tmpChannels.Count.ToString());
          int channelIndex = 0;
          foreach (TVChannel tmpChan in tmpChannels)
          {
            exportFile.SetValue("Card " + i.ToString(), "CHANNEL " + channelIndex.ToString(), tmpChan.ID.ToString());
            channelIndex++;
          }
          exportedCardsCounter++;
        }
        catch { }

      return exportedCardsCounter;
    }

    private int ExportRecordedTv(MediaPortal.Profile.Settings exportFile)
    {
      int exportedRecordedTvCounter = 0;

      //Backup recorded shows information
      ArrayList Recorded = new ArrayList();
      TVDatabase.GetRecordedTV(ref Recorded);

      if (Recorded.Count > 0)
        exportFile.SetValue("RECORDED", "TOTAL", Recorded.Count.ToString());

      int count = 1; //mjsystem
      foreach (TVRecorded show in Recorded)
      {
        exportFile.SetValue("Recorded " + count.ToString(), "ID", show.ID.ToString());
        exportFile.SetValue("Recorded " + count.ToString(), "TITLE", show.Title);
        exportFile.SetValue("Recorded " + count.ToString(), "CHANNEL", show.Channel);
        exportFile.SetValue("Recorded " + count.ToString(), "DESC", show.Description);
        exportFile.SetValue("Recorded " + count.ToString(), "GENRE", show.Genre);
        exportFile.SetValue("Recorded " + count.ToString(), "FILENAME", show.FileName);
        exportFile.SetValue("Recorded " + count.ToString(), "STARTTIME", show.Start.ToString());
        exportFile.SetValue("Recorded " + count.ToString(), "ENDTIME", show.End.ToString());
        exportFile.SetValue("Recorded " + count.ToString(), "PLAYED", show.Played.ToString());
        count++;
      }

      exportedRecordedTvCounter = count - 1;

      return exportedRecordedTvCounter;
    }

    private int ExportScheduledRecordings(MediaPortal.Profile.Settings exportFile)
    {
      int exportedScheduledRecordings = 0;

      //Backup recording shows information
      ArrayList Recordings = new ArrayList();
      TVDatabase.GetRecordings(ref Recordings);

      if (Recordings.Count > 0)
        exportFile.SetValue("RECORDINGS", "TOTAL", Recordings.Count.ToString());

      for (int i = 1; i < Recordings.Count + 1; i++)
      {
        MediaPortal.TV.Database.TVRecording scheduledRecording = (MediaPortal.TV.Database.TVRecording)Recordings[i - 1];
        exportFile.SetValue("Recording " + i.ToString(), "ID", scheduledRecording.ID.ToString());
        exportFile.SetValue("Recording " + i.ToString(), "TITLE", scheduledRecording.Title);
        exportFile.SetValue("Recording " + i.ToString(), "CHANNEL", scheduledRecording.Channel);
        exportFile.SetValue("Recording " + i.ToString(), "STARTTIME", scheduledRecording.Start.ToString());
        exportFile.SetValue("Recording " + i.ToString(), "ENDTIME", scheduledRecording.End.ToString());
        exportFile.SetValue("Recording " + i.ToString(), "CANCELEDTIME", scheduledRecording.Canceled.ToString());
        exportFile.SetValue("Recording " + i.ToString(), "TYPE", scheduledRecording.RecType.ToString());
        exportFile.SetValue("Recording " + i.ToString(), "PRIORITY", scheduledRecording.Priority.ToString());
        exportFile.SetValue("Recording " + i.ToString(), "QUALITY", scheduledRecording.Quality.ToString());
        exportFile.SetValueAsBool("Recording " + i.ToString(), "ISCONTENTREC", scheduledRecording.IsContentRecording);
        exportFile.SetValueAsBool("Recording " + i.ToString(), "SERIES", scheduledRecording.Series);
        exportFile.SetValue("Recording " + i.ToString(), "EPISODES", scheduledRecording.EpisodesToKeep.ToString());

        //Check if this recording has had any cancels
        exportFile.SetValue("Recording " + i.ToString(), "CANCELED SERIES TOTAL", scheduledRecording.CanceledSeries.Count.ToString());
        if (scheduledRecording.CanceledSeries.Count > 0)
        {
          int canxCount = 0;
          List<long> getScheduledRecording = scheduledRecording.CanceledSeries;
          foreach (long canxScheduledRecording in getScheduledRecording)
          {
            exportFile.SetValue("Recording " + i.ToString(), "CANCELED SERIES CANCELEDTIME " + canxCount.ToString(), canxScheduledRecording.ToString());
            canxCount++;
          }
        }
        exportedScheduledRecordings++;
      }

      return exportedScheduledRecordings;
    }

    private void ExportToXml(string fileName)
    {
      int exportedChannels = 0;
      int exportedGroups = 0;
      int exportedCards = 0;
      int exportedRecordedTv = 0;
      int exportedScheduledRecordings = 0;

      //Current version number of this exporter (change when needed)
      int currentVersion = 1;  //<--- Make sure this same number is given to Import_from_XML

      using (MediaPortal.Profile.Settings exportFile = new MediaPortal.Profile.Settings(fileName, false))
      {
        exportFile.SetValue("MP channel export list", "version", currentVersion.ToString());

        exportedChannels = ExportChannels(exportFile);
        exportedGroups = ExportGroups(exportFile);
        exportedCards = ExportCards(exportFile);
        exportedRecordedTv = ExportRecordedTv(exportFile);
        exportedScheduledRecordings = ExportScheduledRecordings(exportFile);
      }

      // No data exported? - Delete file
      if ((exportedChannels == 0) &&
        (exportedGroups == 0) &&
        (exportedCards == 0) &&
        (exportedRecordedTv == 0) &&
        (exportedScheduledRecordings == 0))
      {
        File.Delete(fileName);
      }
      else
      {
        string messageText = "Exported successfully:                  \n";
        if (exportedChannels > 0)
        {
          messageText += string.Format("\n{0} TV channel", exportedChannels);
          if (exportedChannels > 1)
            messageText += "s";
        }
        if (exportedGroups > 0)
        {
          messageText += string.Format("\n{0} TV channel group", exportedGroups);
          if (exportedGroups > 1)
            messageText += "s";
        }
        if (exportedCards > 0)
        {
          messageText += string.Format("\n{0} assigned capture card", exportedCards);
          if (exportedCards > 1)
            messageText += "s";
        }
        if (exportedRecordedTv > 0)
        {
          messageText += string.Format("\n{0} completed recording", exportedRecordedTv);
          if (exportedRecordedTv > 1)
            messageText += "s";
        }
        if (exportedScheduledRecordings > 0)
        {
          messageText += string.Format("\n{0} scheduled recording", exportedScheduledRecordings);
          if (exportedScheduledRecordings > 1)
            messageText += "s";
        }

        MessageBox.Show(messageText, "Export Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void ImportFromXml(string fileName)
    {
      //Check if we have a file just in case
      if (!File.Exists(fileName))
        return;

      TVDatabase.ClearAll();
      LoadSettings();
      //Current Version change to reflect the above exporter in order for compatibility
      int currentVersion = 1;   //<--- Make sure that is the same number as in Export_to_XML
      int version = 1;			 //Set to:  0 = old ; 1 = current ; 2 = newer
      using (MediaPortal.Profile.Settings channels = new MediaPortal.Profile.Settings(fileName))
      {
        //Check version if not the right version prompt/do stuff/accomodate/change
        int versionCheck = channels.GetValueAsInt("MP channel export list", "version", -1);
        if (versionCheck == -1)
        {
          //Not a valid channel list
          MessageBox.Show("This is not a valid channel list.");
          return;
        }
        else if (versionCheck >= 0 && versionCheck < currentVersion)
        {
          //Older file
          MessageBox.Show("This is an older channel list. Trying to import.");
          version = 0;
        }
        else if (versionCheck == currentVersion)
        {
          //Current file, this is good stuff
          version = 1;
        }
        else if (versionCheck > currentVersion)
        {
          //Newer? This person lives in a cave
          MessageBox.Show("This is a newer channel list. Trying to import.\nConsider upgrading to a later MediaPortal version.");
          version = 2;
        }

        //Count how many channels we have to import
        int counter = 0;
        for (int i = 0; ; i++)
        {
          if (channels.GetValueAsInt(i.ToString(), "INDEX", -1) == -1)
          {
            if (counter == 0)
            {
              MessageBox.Show("No channels found in channel list.");
              return;
            }
            else
              break;
          }
          else
            counter++;
        }
        MessageBox.Show("Importing " + counter.ToString() + " channels.");

        listViewTvChannels.BeginUpdate();

        for (int i = 0; i < counter; i++)
        {
          int overwrite = 0;
          int overwriteIndex = 0;
          TelevisionChannel importChannel = new TelevisionChannel();
          importChannel.ID = channels.GetValueAsInt(i.ToString(), "ID", 0);
          importChannel.Channel = channels.GetValueAsInt(i.ToString(), "Number", 0);
          importChannel.Name = channels.GetValueAsString(i.ToString(), "Name", "");
          importChannel.Country = channels.GetValueAsInt(i.ToString(), "Country", 0);
          importChannel.External = channels.GetValueAsBool(i.ToString(), "External", false);
          importChannel.ExternalTunerChannel = channels.GetValueAsString(i.ToString(), "External Tuner Channel", "");
          importChannel.Frequency.Hertz = channels.GetValueAsInt(i.ToString(), "Frequency", 0);
          importChannel.standard = ConvertAvs(channels.GetValueAsString(i.ToString(), "Analog Standard Index", "None"));
          importChannel.VisibleInGuide = channels.GetValueAsBool(i.ToString(), "Visible in Guide", false);
          importChannel.Scrambled = channels.GetValueAsBool(i.ToString(), "Scrambled", false);

          //Check to see if this channel exists prompt to overwrite
          foreach (ListViewItem listItem in listViewTvChannels.Items)
          {
            TelevisionChannel Check_Chan = listItem.Tag as TelevisionChannel;
            if (Check_Chan.ID == importChannel.ID && Check_Chan.Name == importChannel.Name)
            {
              if (MessageBox.Show(importChannel.Name + " (Channel " + importChannel.Channel.ToString() + ") already exists.\nWould you like to overwrite this channel?", "Channel Conflict", MessageBoxButtons.YesNo) == DialogResult.Yes)
              {
                overwrite = 1;
                overwriteIndex = listItem.Index;
              }
              else
                overwrite = -1;

              break;
            }
          }

          if (overwrite != -1)
          {
            if (overwrite == 1)
            {
              TelevisionChannel editedChannel = importChannel;
              listViewTvChannels.Items[overwriteIndex].Tag = editedChannel;

              listViewTvChannels.Items[overwriteIndex].SubItems[0].Text = editedChannel.Name;
              listViewTvChannels.Items[overwriteIndex].SubItems[1].Text = editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString();
              listViewTvChannels.Items[overwriteIndex].SubItems[2].Text = GetStandardName(editedChannel.standard);
              listViewTvChannels.Items[overwriteIndex].SubItems[3].Text = editedChannel.External ? "External" : "Internal";
            }
            else
            {
              TelevisionChannel editedChannel = importChannel;
              ListViewItem listItem = new ListViewItem(new string[] { editedChannel.Name, 
																					  editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString(),
																					  GetStandardName(editedChannel.standard),
																					  editedChannel.External ? "External" : "Internal"});
              listItem.Tag = editedChannel;

              listViewTvChannels.Items.Add(listItem);
              listViewTvChannels.Items[listViewTvChannels.Items.IndexOf(listItem)].Checked = true;
            }

            //Check if required to do anything specific for compatibility reasons
            switch (version)
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
            if (version == 0 || version == 1 || version == 2)
            {
              if (importChannel.Channel >= 0)
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
                    TVDatabase.MapDVBTChannel(importChannel.Name, provider, importChannel.ID, freq, ONID, TSID, SID, audioPid, videoPid, teletextPid, pmtPid, bandWidth, audio1, audio2, audio3, ac3Pid, pcrPid, audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3, HasEITPresentFollow, HasEITSchedule);
                }
                catch (Exception)
                {
                  MessageBox.Show("Importing DVB-T failed.");
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
                    TVDatabase.MapDVBCChannel(importChannel.Name, provider, importChannel.ID, freq, symbolrate, innerFec, modulation, ONID, TSID, SID, audioPid, videoPid, teletextPid, pmtPid, audio1, audio2, audio3, ac3Pid, pcrPid, audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3, HasEITPresentFollow, HasEITSchedule);
                }
                catch (Exception)
                {
                  MessageBox.Show("Importing DVB-C data failed.");
                }

                //dvb-S
                try
                {
                  DVBChannel ch = new DVBChannel();
                  TVDatabase.GetSatChannel(importChannel.ID, 1, ref ch);

                  freq = channels.GetValueAsInt(i.ToString(), "DVBSFreq", 0);
                  ONID = channels.GetValueAsInt(i.ToString(), "DVBSONID", 0);
                  TSID = channels.GetValueAsInt(i.ToString(), "DVBSTSID", 0);
                  SID = channels.GetValueAsInt(i.ToString(), "DVBSSID", 0);
                  symbolrate = channels.GetValueAsInt(i.ToString(), "DVBSSymbolrate", 0);
                  innerFec = channels.GetValueAsInt(i.ToString(), "DVBSInnerFec", 0);
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
                    ch.ServiceName = importChannel.Name;
                    ch.ID = importChannel.ID;
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
                    ch.PMTPid = pmtPid;

                    TVDatabase.RemoveSatChannel(ch);
                    TVDatabase.AddSatChannel(ch);
                  }
                }
                catch (Exception)
                {
                  MessageBox.Show("Importing DVB-S data failed.");
                }

                //ATSC
                try
                {
                  int minorChannel, majorChannel, physicalChannel;
                  physicalChannel = channels.GetValueAsInt(i.ToString(), "ATSCPhysical", -1);
                  provider = channels.GetValueAsString(i.ToString(), "ATSCProvider", "");
                  freq = channels.GetValueAsInt(i.ToString(), "ATSCFreq", 0);
                  symbolrate = channels.GetValueAsInt(i.ToString(), "ATSCSymbolrate", 0);
                  innerFec = channels.GetValueAsInt(i.ToString(), "ATSCInnerFec", 0);
                  modulation = channels.GetValueAsInt(i.ToString(), "ATSCModulation", 0);

                  ONID = channels.GetValueAsInt(i.ToString(), "ATSCONID", 0);
                  TSID = channels.GetValueAsInt(i.ToString(), "ATSCTSID", 0);
                  SID = channels.GetValueAsInt(i.ToString(), "ATSCSID", 0);
                  audioPid = channels.GetValueAsInt(i.ToString(), "ATSCAudioPid", 0);
                  videoPid = channels.GetValueAsInt(i.ToString(), "ATSCVideoPid", 0);
                  teletextPid = channels.GetValueAsInt(i.ToString(), "ATSCTeletextPid", 0);
                  pmtPid = channels.GetValueAsInt(i.ToString(), "ATSCPmtPid", 0);
                  audio1 = channels.GetValueAsInt(i.ToString(), "ATSCAudio1Pid", -1);
                  audio2 = channels.GetValueAsInt(i.ToString(), "ATSCAudio2Pid", -1);
                  audio3 = channels.GetValueAsInt(i.ToString(), "ATSCAudio3Pid", -1);
                  ac3Pid = channels.GetValueAsInt(i.ToString(), "ATSCAC3Pid", -1);
                  audioLanguage = channels.GetValueAsString(i.ToString(), "ATSCAudioLanguage", "");
                  audioLanguage1 = channels.GetValueAsString(i.ToString(), "ATSCAudioLanguage1", "");
                  audioLanguage2 = channels.GetValueAsString(i.ToString(), "ATSCAudioLanguage2", "");
                  audioLanguage3 = channels.GetValueAsString(i.ToString(), "ATSCAudioLanguage3", "");
                  minorChannel = channels.GetValueAsInt(i.ToString(), "ATSCMinor", -1);
                  majorChannel = channels.GetValueAsInt(i.ToString(), "ATSCMajor", -1);
                  HasEITPresentFollow = channels.GetValueAsBool(i.ToString(), "ATSCHasEITPresentFollow", false);
                  HasEITSchedule = channels.GetValueAsBool(i.ToString(), "ATSCHasEITSchedule", false);
                  pcrPid = channels.GetValueAsInt(i.ToString(), "ATSCPCRPid", -1);

                  if (physicalChannel > 0 && minorChannel >= 0 && majorChannel >= 0)
                    TVDatabase.MapATSCChannel(importChannel.Name, physicalChannel, minorChannel, majorChannel, provider, importChannel.ID,
                                              freq, symbolrate, innerFec, modulation, ONID, TSID, SID, audioPid, videoPid, teletextPid,
                                              pmtPid, audio1, audio2, audio3, ac3Pid, pcrPid, audioLanguage, audioLanguage1, audioLanguage2,
                                              audioLanguage3, HasEITPresentFollow, HasEITSchedule);
                }
                catch (Exception)
                {
                  MessageBox.Show("Importing ATSC data failed");
                }
              }
            }
          }
        }
        SaveTVChannels();
        _reloadList = true;
        SaveSettings();

        listViewTvChannels.EndUpdate();

        //Grab group Data and channel maping
        int groupIndex = 0, channelIndex = 0;

        //Grab total groups for reference
        groupIndex = channels.GetValueAsInt("GROUPS", "TOTAL", -1);
        //Check if we have any groups
        if (groupIndex > 0)
        {
          MessageBox.Show("Importing " + groupIndex.ToString() + " groups.");

          for (int i = 1; i <= groupIndex; i++) //mjsystem
          {
            int overwrite = 0;
            TVGroup importGroup = new TVGroup();
            importGroup.ID = channels.GetValueAsInt("Group " + i.ToString(), "ID", 0);
            importGroup.GroupName = channels.GetValueAsString("Group " + i.ToString(), "NAME", "");
            importGroup.Pincode = channels.GetValueAsInt("Group " + i.ToString(), "PINCODE", 0);
            importGroup.Sort = channels.GetValueAsInt("Group " + i.ToString(), "SORT", 0);

            if (overwrite != -1)
            {
              //Add Group to database
              TVDatabase.AddGroup(importGroup);
              //This is done for every version regardless
              if (version == 0 || version == 1 || version == 2)
              {
                //Add channels to this group
                ArrayList Group_Channels = new ArrayList();
                TVDatabase.GetChannels(ref Group_Channels);
                channelIndex = channels.GetValueAsInt("Group " + i.ToString(), "TOTAL CHANNELS", 0);

                if (channelIndex > 0)
                  for (int j = 0; j < channelIndex; j++)
                  {
                    int tmpID = channels.GetValueAsInt("Group " + i.ToString(), "CHANNEL " + j.ToString(), 0);

                    //Locate Channel so it can be added to group

                    foreach (TVChannel FindChan in Group_Channels)
                      if (FindChan.ID == tmpID)
                      {
                        //Add channel to group
                        importGroup.TvChannels.Add(FindChan);

                        //Have to re-grab group from database in order to map correctly :|
                        ArrayList GrabGroup = new ArrayList();
                        TVDatabase.GetGroups(ref GrabGroup);

                        foreach (TVGroup tmpGroup in GrabGroup)
                          if (importGroup.ID == tmpGroup.ID)
                            TVDatabase.MapChannelToGroup(tmpGroup, FindChan);
                      }
                  }
              }
            }
          }
          SaveSettings();
        }

        //Grab Saved Card mapping

        //Check if we have cards first
        int cardsIndex = 0;
        channelIndex = 0;

        //Grab total cards for reference
        cardsIndex = channels.GetValueAsInt("CARDS", "TOTAL", -1);

        //Check if we have any cards
        for (int i = 1; i < cardsIndex + 1; i++)
        {
          //This is done for every version regardless
          //Re-Map channels to available cards
          ArrayList Card_Channels = new ArrayList();
          TVDatabase.GetChannels(ref Card_Channels);
          channelIndex = channels.GetValueAsInt("Card " + i.ToString(), "TOTAL CHANNELS", 0);

          if (channelIndex > 0)
            for (int j = 0; j < channelIndex; j++)
            {
              int tmpID = channels.GetValueAsInt("Card " + i.ToString(), "CHANNEL " + j.ToString(), 0);

              //Locate Channel so it can be added to Card
              foreach (TVChannel FindChan in Card_Channels)
                if (FindChan.ID == tmpID)
                  //Map it
                  TVDatabase.MapChannelToCard(FindChan.ID, i);
            }
        }

        //Grab recorded show information
        int recordedCount = 0;

        //Grab recorded shows saved for referrence
        recordedCount = channels.GetValueAsInt("RECORDED", "TOTAL", -1);
        if (recordedCount > 0)
        {
          MessageBox.Show("Importing " + recordedCount.ToString() + " recorded items.");

          //Check if required to do anything specific for compatibility reasons
          switch (version)
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
          if (version == 0 || version == 1 || version == 2)
          {
            for (int i = 1; i < recordedCount + 1; i++)
            {
              //Create temp TVRecorded to hold data to import
              TVRecorded tempRecorded = new TVRecorded();
              tempRecorded.ID = channels.GetValueAsInt("Recorded " + i.ToString(), "ID", 0);
              tempRecorded.Title = channels.GetValueAsString("Recorded " + i.ToString(), "TITLE", "");
              tempRecorded.Channel = channels.GetValueAsString("Recorded " + i.ToString(), "CHANNEL", "");
              tempRecorded.Description = channels.GetValueAsString("Recorded " + i.ToString(), "DESC", "");
              tempRecorded.Genre = channels.GetValueAsString("Recorded " + i.ToString(), "GENRE", "");
              tempRecorded.FileName = channels.GetValueAsString("Recorded " + i.ToString(), "FILENAME", "");
              tempRecorded.Start = Convert.ToInt64(channels.GetValueAsString("Recorded " + i.ToString(), "STARTTIME", "0"));
              if (tempRecorded.Start == 0)
                tempRecorded.Start = Convert.ToInt64(channels.GetValueAsString("Recorded " + i.ToString(), "START", "0"));
              tempRecorded.End = Convert.ToInt64(channels.GetValueAsString("Recorded " + i.ToString(), "ENDTIME", "0"));
              tempRecorded.Played = channels.GetValueAsInt("Recorded " + i.ToString(), "PLAYED", 0);

              //Add or gathered info to the TVDatabase
              bool recordedOverwrite = false;
              ArrayList checkRecordedList = new ArrayList();
              TVDatabase.GetRecordedTV(ref checkRecordedList);
              TVRecorded check_recorded = new TVRecorded();
              foreach (TVRecorded checkRecorded in checkRecordedList)
                if (checkRecorded.ID == tempRecorded.ID && checkRecorded.Start.ToString() == tempRecorded.Start.ToString())
                {
                  check_recorded = checkRecorded;
                  recordedOverwrite = true;
                  break;
                }
              if (recordedOverwrite)
                //Ask if user if overwrite ok
                if (MessageBox.Show("Would you like to overwrite the entry for \"" + check_recorded.Title + "\" (start time: " + check_recorded.Start.ToString() + ")\nwith the entry \"" + tempRecorded.Title + "\" (start time: " + tempRecorded.Start.ToString() + ")?",
                "Import Conflict", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                  TVDatabase.RemoveRecordedTV(check_recorded);
                  TVDatabase.AddRecordedTV(tempRecorded);
                }
                else
                  //Check if this file exists first, if not ask user to locate it or no update
                  TVDatabase.AddRecordedTV(tempRecorded);
            }
          }
        }

        //Grab recording shows information
        int recordingsCount = 0;

        //Grab recorded shows saved for referrence
        recordingsCount = channels.GetValueAsInt("RECORDINGS", "TOTAL", -1);

        if (recordingsCount > 0)
        {
          MessageBox.Show("Importing " + recordingsCount.ToString() + " scheduled recordings.");

          //Check if required to do anything specific for compatibility reasons
          switch (version)
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
          if (version == 0 || version == 1 || version == 2)
            for (int i = 1; i < recordingsCount + 1; i++)
            {
              //Create temp TVRecording to hold data to import
              MediaPortal.TV.Database.TVRecording tempRecording = new MediaPortal.TV.Database.TVRecording();
              tempRecording.ID = channels.GetValueAsInt("Recording " + i.ToString(), "ID", 0);
              tempRecording.Title = channels.GetValueAsString("Recording " + i.ToString(), "TITLE", "");
              tempRecording.Channel = channels.GetValueAsString("Recording " + i.ToString(), "CHANNEL", "");
              tempRecording.Start = Convert.ToInt64(channels.GetValueAsString("Recording " + i.ToString(), "STARTTIME", "0"));
              tempRecording.End = Convert.ToInt64(channels.GetValueAsString("Recording " + i.ToString(), "ENDTIME", "0"));
              tempRecording.Canceled = Convert.ToInt64(channels.GetValueAsString("Recording " + i.ToString(), "CANCELEDTIME", "0"));
              tempRecording.RecType = ConvertRecordingType(channels.GetValueAsString("Recording " + i.ToString(), "TYPE", ""));
              tempRecording.Priority = channels.GetValueAsInt("Recording " + i.ToString(), "PRIORITY", 0);
              tempRecording.Quality = ConvertQualityType(channels.GetValueAsString("Recording " + i.ToString(), "QUALITY", ""));
              tempRecording.IsContentRecording = channels.GetValueAsBool("Recording " + i.ToString(), "ISCONTENTREC", false);
              tempRecording.Series = channels.GetValueAsBool("Recording " + i.ToString(), "SERIES", false);
              tempRecording.EpisodesToKeep = channels.GetValueAsInt("Recording " + i.ToString(), "EPISODES", Int32.MaxValue);

              //Add this recording to TVDatabase
              bool recordingOverwrite = false;
              ArrayList checkRecordingList = new ArrayList();
              TVDatabase.GetRecordings(ref checkRecordingList);
              MediaPortal.TV.Database.TVRecording checkRecording = new MediaPortal.TV.Database.TVRecording();
              foreach (MediaPortal.TV.Database.TVRecording tvRecording in checkRecordingList)
                if (tvRecording.ID == tempRecording.ID && tvRecording.Start.ToString() == tempRecording.Start.ToString())
                {
                  checkRecording = tvRecording;
                  recordingOverwrite = true;
                  break;
                }
              if (recordingOverwrite)
              {
                //Ask if user if overwrite ok
                if (MessageBox.Show("Would you like to overwrite the entry for \"" + checkRecording.Title + "\" (start time: " + checkRecording.Start.ToString() + ")\nwith the entry \"" + tempRecording.Title + "\" (start time: " + tempRecording.Start.ToString() + ")?",
                  "Import Conflict", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                  //Delete Canceled series information
                  for (int z = 0; z < checkRecording.CanceledSeries.Count; ++z)
                    TVDatabase.DeleteCanceledSeries(checkRecording);
                  //Check if this recording has had any cancels
                  int canxCount = 0;
                  canxCount = channels.GetValueAsInt("Recording " + i.ToString(), "CANCELED SERIES TOTAL", 0);
                  if (canxCount > 0)
                  {
                    tempRecording.CanceledSeries.Clear();
                    long lastCanxTime = 0;
                    for (int j = 0; j < canxCount; j++)
                    {
                      //Add the canceled time to TVDatabase
                      long canxTime = 0;
                      canxTime = Convert.ToInt64(channels.GetValueAsString("Recording " + i.ToString(), "CANCELED SERIES CANCELEDTIME " + j.ToString(), "0"));
                      //Check if we had the same time from before if so stop adding
                      if (canxTime == lastCanxTime) break;
                      //TVDatabase.AddCanceledSerie(temp_recording,canx_time);
                      tempRecording.CanceledSeries.Add((long)canxTime);
                      lastCanxTime = canxTime;
                    }
                  }
                  //Delete old entry
                  TVDatabase.RemoveRecording(checkRecording);
                  //Add new overwrite entry
                  TVDatabase.AddRecording(ref tempRecording);
                }
              }
              else
              {
                //Check if this recording has had any cancels
                int canxCount = 0;
                canxCount = channels.GetValueAsInt("Recording " + i.ToString(), "CANCELED SERIES TOTAL", 0);
                if (canxCount > 0)
                {
                  tempRecording.CanceledSeries.Clear();
                  long lastCanxTime = 0;
                  for (int j = 0; j < canxCount; j++)
                  {
                    //Add the canceled time to TVDatabase
                    long canxTime = 0;
                    canxTime = Convert.ToInt64(channels.GetValueAsString("Recording " + i.ToString(), "CANCELED SERIES CANCELEDTIME " + j.ToString(), "0"));
                    //Check if we had the same time from before if so stop adding
                    if (canxTime == lastCanxTime) break;
                    //TVDatabase.AddCanceledSerie(temp_recording,canx_time);
                    tempRecording.CanceledSeries.Add((long)canxTime);
                    lastCanxTime = canxTime;
                  }
                }
                //Add new entry
                TVDatabase.AddRecording(ref tempRecording);
              }
            }
        }
      }
    }

    private AnalogVideoStandard ConvertAvs(object avs)
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

    private TV.Database.TVRecording.QualityType ConvertQualityType(object quality)
    {
      if ((string)quality == "NotSet") return TV.Database.TVRecording.QualityType.NotSet;
      if ((string)quality == "Portable") return TV.Database.TVRecording.QualityType.Portable;
      if ((string)quality == "Low") return TV.Database.TVRecording.QualityType.Low;
      if ((string)quality == "Medium") return TV.Database.TVRecording.QualityType.Medium;
      if ((string)quality == "High") return TV.Database.TVRecording.QualityType.High;

      //If nothing return Default
      return TV.Database.TVRecording.QualityType.NotSet;
    }

    private TV.Database.TVRecording.RecordingType ConvertRecordingType(object recType)
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

    private void buttonBackup_Click(object sender, System.EventArgs e)
    {
      xmlSaveDialog.RestoreDirectory = true;
      if (xmlSaveDialog.ShowDialog(this) == DialogResult.OK)
        ExportToXml(xmlSaveDialog.FileName.ToString());
    }

    private void buttonRestore_Click(object sender, System.EventArgs e)
    {
      xmlOpenDialog.RestoreDirectory = true;
      if (xmlOpenDialog.ShowDialog(this) == DialogResult.OK)
        ImportFromXml(xmlOpenDialog.FileName.ToString());
    }

    private void buttonLookup_Click(object sender, System.EventArgs e)
    {
      TvChannelLookupService dlg = new TvChannelLookupService();
      dlg.ShowDialog(this);
      _reloadList = true;
      RadioStations.UpdateList();
    }

    private void buttonCombine_Click(object sender, EventArgs e)
    {
      if (listViewTvChannels.SelectedItems.Count != 2)
        return;

      TelevisionChannel ch1 = listViewTvChannels.SelectedItems[0].Tag as TelevisionChannel;
      TelevisionChannel ch2 = listViewTvChannels.SelectedItems[1].Tag as TelevisionChannel;

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

      listViewTvChannels.BeginUpdate();

      ListViewItem listItem = listViewTvChannels.SelectedItems[1];
      listItem.SubItems[0].Text = ch2.Name;
      listItem.SubItems[1].Text = ch2.External ? String.Format("{0}/{1}", ch2.Channel, ch2.ExternalTunerChannel) : ch2.Channel.ToString();
      listItem.SubItems[2].Text = GetStandardName(ch2.standard);
      listItem.SubItems[3].Text = ch2.External ? "External" : "Internal";
      listItem.ImageIndex = 0;

      if (ch2.Scrambled)
        listItem.ImageIndex = 1;

      listViewTvChannels.Items.RemoveAt(listViewTvChannels.SelectedIndices[0]);

      listViewTvChannels.EndUpdate();

      SaveSettings();
    }

    private void listViewTvChannels_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      _itemsModified = true;

      //
      // Fetch checked item
      //
      if (e.Item.Index < listViewTvChannels.Items.Count)
      {
        TelevisionChannel tvChannel = e.Item.Tag as TelevisionChannel;
        tvChannel.VisibleInGuide = (e.Item.Checked);
        e.Item.Tag = tvChannel;
      }
    }

    private void listViewTVChannelsCard_DoubleClick(object sender, EventArgs e)
    {
      buttonMap_Click(sender, e);
    }

    private void listViewTVChannelsForCard_DoubleClick(object sender, EventArgs e)
    {
      buttonUnmap_Click(sender, e);
    }

    private void buttonDeleteScrambled_Click( object sender, EventArgs e )
    {
       listViewTvChannels.BeginUpdate();
       _itemsModified = true;
            
       int itemCount = listViewTvChannels.Items.Count;

       for ( int index = 0; index < itemCount; index++ )
       {
         if ( listViewTvChannels.Items[index].ImageIndex == 1 ) // channel is scrambled
         {
           listViewTvChannels.Items.RemoveAt(index);
           itemCount -= 1;
         }
       }

       SaveSettings();
       listViewTvChannels.EndUpdate();
    }

  }
}

