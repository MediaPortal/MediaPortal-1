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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using DirectShowLib;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.UserInterface.Controls;
using Microsoft.Win32;

#pragma warning disable 108

namespace MediaPortal.Configuration.TVE2.Sections
{
  public class TVChannels : SectionSettings
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
    } ;

    private IContainer components = null;
    private OpenFileDialog xmlOpenDialog;
    private SaveFileDialog xmlSaveDialog;
    private ImageList imageListLocks;
    private MPTabControl tabControlTvChannels;
    private MPTabPage tabPageTvChannels;
    private MPButton buttonLookup;
    private MPButton buttonRestore;
    private MPButton buttonBackup;
    private MPButton buttonAddCvbsSvhs;
    private MPListView listViewTvChannels;
    private MPButton buttonImportFromTvGuide;
    private MPButton buttonClearChannels;
    private MPButton buttonAddChannel;
    private MPButton buttonDeleteChannel;
    private MPButton buttonEditChannel;
    private MPButton buttonChannelUp;
    private MPButton buttonChannelDown;
    private MPTabPage tabPageTvCards;
    private MPButton buttonMap;
    private MPButton buttonUnmap;
    private MPListView listViewTVChannelsForCard;
    private MPListView listViewTVChannelsCard;
    private MPLabel labelMapChannelsToTvCard;
    private MPComboBox comboBoxCard;
    private MPButton buttonCombine;
    private MPButton buttonDeleteScrambled;
    private ColumnHeader columnHeaderChannelName;
    private ColumnHeader columnHeaderChannel;
    private ColumnHeader columnHeaderStandard;
    private ColumnHeader columnHeaderType;
    private ColumnHeader columnHeaderAssignedTvChannels;
    private ColumnHeader columnHeaderAvailableTvChannels;

    //
    // Private members
    //
    private bool _init = false;
    private bool _itemsModified = false;
    private ListViewColumnSorter _columnSorter;
    private static bool _reloadList = false;

    private enum ImportError
    {
      CHANNEL_ANALOG_FAILED = -1,
      CHANNEL_ATSC_FAILED = -2,
      CHANNEL_DVBC_FAILED = -3,
      CHANNEL_DVBS_FAILED = -4,
      CHANNEL_DVBT_FAILED = -5
    }


    public TVChannels()
      : this("TV Channels")
    {
    }

    public TVChannels(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // Disable it TVE3
      if (File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        this.Enabled = false;
        _init = true;
      }
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
      System.ComponentModel.ComponentResourceManager resources =
        new System.ComponentModel.ComponentResourceManager(typeof (TVChannels));
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
      this.imageListLocks.ImageStream =
        ((System.Windows.Forms.ImageListStreamer) (resources.GetObject("imageListLocks.ImageStream")));
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
      this.tabControlTvChannels.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlTvChannels.Controls.Add(this.tabPageTvChannels);
      this.tabControlTvChannels.Controls.Add(this.tabPageTvCards);
      this.tabControlTvChannels.Location = new System.Drawing.Point(0, 0);
      this.tabControlTvChannels.Name = "tabControlTvChannels";
      this.tabControlTvChannels.SelectedIndex = 0;
      this.tabControlTvChannels.Size = new System.Drawing.Size(472, 408);
      this.tabControlTvChannels.TabIndex = 0;
      this.tabControlTvChannels.SelectedIndexChanged +=
        new System.EventHandler(this.tabControlTvChannels_SelectedIndexChanged);
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
      this.buttonDeleteScrambled.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDeleteScrambled.Location = new System.Drawing.Point(328, 352);
      this.buttonDeleteScrambled.Name = "buttonDeleteScrambled";
      this.buttonDeleteScrambled.Size = new System.Drawing.Size(119, 20);
      this.buttonDeleteScrambled.TabIndex = 13;
      this.buttonDeleteScrambled.Text = "Delete scrambled";
      this.buttonDeleteScrambled.UseVisualStyleBackColor = true;
      this.buttonDeleteScrambled.Click += new System.EventHandler(this.buttonDeleteScrambled_Click);
      // 
      // buttonCombine
      // 
      this.buttonCombine.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCombine.Location = new System.Drawing.Point(16, 308);
      this.buttonCombine.Name = "buttonCombine";
      this.buttonCombine.Size = new System.Drawing.Size(72, 20);
      this.buttonCombine.TabIndex = 1;
      this.buttonCombine.Text = "Combine";
      this.buttonCombine.UseVisualStyleBackColor = true;
      this.buttonCombine.Click += new System.EventHandler(this.buttonCombine_Click);
      // 
      // buttonLookup
      // 
      this.buttonLookup.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonLookup.Location = new System.Drawing.Point(250, 308);
      this.buttonLookup.Name = "buttonLookup";
      this.buttonLookup.Size = new System.Drawing.Size(72, 20);
      this.buttonLookup.TabIndex = 8;
      this.buttonLookup.Text = "Lookup";
      this.buttonLookup.UseVisualStyleBackColor = true;
      this.buttonLookup.Click += new System.EventHandler(this.buttonLookup_Click);
      // 
      // buttonRestore
      // 
      this.buttonRestore.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonRestore.Location = new System.Drawing.Point(250, 352);
      this.buttonRestore.Name = "buttonRestore";
      this.buttonRestore.Size = new System.Drawing.Size(72, 20);
      this.buttonRestore.TabIndex = 10;
      this.buttonRestore.Text = "Restore";
      this.buttonRestore.UseVisualStyleBackColor = true;
      this.buttonRestore.Click += new System.EventHandler(this.buttonRestore_Click);
      // 
      // buttonBackup
      // 
      this.buttonBackup.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBackup.Location = new System.Drawing.Point(250, 330);
      this.buttonBackup.Name = "buttonBackup";
      this.buttonBackup.Size = new System.Drawing.Size(72, 20);
      this.buttonBackup.TabIndex = 9;
      this.buttonBackup.Text = "Backup";
      this.buttonBackup.UseVisualStyleBackColor = true;
      this.buttonBackup.Click += new System.EventHandler(this.buttonBackup_Click);
      // 
      // buttonAddCvbsSvhs
      // 
      this.buttonAddCvbsSvhs.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonAddCvbsSvhs.Location = new System.Drawing.Point(328, 330);
      this.buttonAddCvbsSvhs.Name = "buttonAddCvbsSvhs";
      this.buttonAddCvbsSvhs.Size = new System.Drawing.Size(119, 20);
      this.buttonAddCvbsSvhs.TabIndex = 12;
      this.buttonAddCvbsSvhs.Text = "Add CVBS/SVHS";
      this.buttonAddCvbsSvhs.UseVisualStyleBackColor = true;
      this.buttonAddCvbsSvhs.Click += new System.EventHandler(this.buttonAddCvbsSvhs_Click);
      // 
      // buttonImportFromTvGuide
      // 
      this.buttonImportFromTvGuide.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonImportFromTvGuide.Location = new System.Drawing.Point(328, 308);
      this.buttonImportFromTvGuide.Name = "buttonImportFromTvGuide";
      this.buttonImportFromTvGuide.Size = new System.Drawing.Size(120, 20);
      this.buttonImportFromTvGuide.TabIndex = 11;
      this.buttonImportFromTvGuide.Text = "Import from TV guide";
      this.buttonImportFromTvGuide.UseVisualStyleBackColor = true;
      this.buttonImportFromTvGuide.Click += new System.EventHandler(this.buttonImportFromTvGuide_Click);
      // 
      // buttonClearChannels
      // 
      this.buttonClearChannels.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonClearChannels.Location = new System.Drawing.Point(94, 352);
      this.buttonClearChannels.Name = "buttonClearChannels";
      this.buttonClearChannels.Size = new System.Drawing.Size(72, 20);
      this.buttonClearChannels.TabIndex = 5;
      this.buttonClearChannels.Text = "Clear";
      this.buttonClearChannels.UseVisualStyleBackColor = true;
      this.buttonClearChannels.Click += new System.EventHandler(this.buttonClearChannels_Click);
      // 
      // buttonAddChannel
      // 
      this.buttonAddChannel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonAddChannel.Location = new System.Drawing.Point(16, 330);
      this.buttonAddChannel.Name = "buttonAddChannel";
      this.buttonAddChannel.Size = new System.Drawing.Size(72, 20);
      this.buttonAddChannel.TabIndex = 2;
      this.buttonAddChannel.Text = "Add";
      this.buttonAddChannel.UseVisualStyleBackColor = true;
      this.buttonAddChannel.Click += new System.EventHandler(this.buttonAddChannel_Click);
      // 
      // buttonDeleteChannel
      // 
      this.buttonDeleteChannel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDeleteChannel.Enabled = false;
      this.buttonDeleteChannel.Location = new System.Drawing.Point(16, 352);
      this.buttonDeleteChannel.Name = "buttonDeleteChannel";
      this.buttonDeleteChannel.Size = new System.Drawing.Size(72, 20);
      this.buttonDeleteChannel.TabIndex = 3;
      this.buttonDeleteChannel.Text = "Delete";
      this.buttonDeleteChannel.UseVisualStyleBackColor = true;
      this.buttonDeleteChannel.Click += new System.EventHandler(this.buttonDeleteChannel_Click);
      // 
      // buttonEditChannel
      // 
      this.buttonEditChannel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonEditChannel.Enabled = false;
      this.buttonEditChannel.Location = new System.Drawing.Point(94, 330);
      this.buttonEditChannel.Name = "buttonEditChannel";
      this.buttonEditChannel.Size = new System.Drawing.Size(72, 20);
      this.buttonEditChannel.TabIndex = 4;
      this.buttonEditChannel.Text = "Edit";
      this.buttonEditChannel.UseVisualStyleBackColor = true;
      this.buttonEditChannel.Click += new System.EventHandler(this.buttonEditChannel_Click);
      // 
      // buttonChannelUp
      // 
      this.buttonChannelUp.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonChannelUp.Enabled = false;
      this.buttonChannelUp.Location = new System.Drawing.Point(172, 330);
      this.buttonChannelUp.Name = "buttonChannelUp";
      this.buttonChannelUp.Size = new System.Drawing.Size(72, 20);
      this.buttonChannelUp.TabIndex = 6;
      this.buttonChannelUp.Text = "Up";
      this.buttonChannelUp.UseVisualStyleBackColor = true;
      this.buttonChannelUp.Click += new System.EventHandler(this.buttonChannelUp_Click);
      // 
      // buttonChannelDown
      // 
      this.buttonChannelDown.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonChannelDown.Enabled = false;
      this.buttonChannelDown.Location = new System.Drawing.Point(172, 352);
      this.buttonChannelDown.Name = "buttonChannelDown";
      this.buttonChannelDown.Size = new System.Drawing.Size(72, 20);
      this.buttonChannelDown.TabIndex = 7;
      this.buttonChannelDown.Text = "Down";
      this.buttonChannelDown.UseVisualStyleBackColor = true;
      this.buttonChannelDown.Click += new System.EventHandler(this.buttonChannelDown_Click);
      // 
      // listViewTvChannels
      // 
      this.listViewTvChannels.AllowDrop = true;
      this.listViewTvChannels.AllowRowReorder = true;
      this.listViewTvChannels.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewTvChannels.CheckBoxes = true;
      this.listViewTvChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                                 {
                                                   this.columnHeaderChannelName,
                                                   this.columnHeaderChannel,
                                                   this.columnHeaderStandard,
                                                   this.columnHeaderType
                                                 });
      this.listViewTvChannels.FullRowSelect = true;
      this.listViewTvChannels.HideSelection = false;
      this.listViewTvChannels.Location = new System.Drawing.Point(16, 16);
      this.listViewTvChannels.Name = "listViewTvChannels";
      this.listViewTvChannels.Size = new System.Drawing.Size(432, 280);
      this.listViewTvChannels.SmallImageList = this.imageListLocks;
      this.listViewTvChannels.TabIndex = 0;
      this.listViewTvChannels.UseCompatibleStateImageBehavior = false;
      this.listViewTvChannels.View = System.Windows.Forms.View.Details;
      this.listViewTvChannels.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewTvChannels_DragDrop);
      this.listViewTvChannels.ItemChecked +=
        new System.Windows.Forms.ItemCheckedEventHandler(this.listViewTvChannels_ItemChecked);
      this.listViewTvChannels.DoubleClick += new System.EventHandler(this.listViewTvChannels_DoubleClick);
      this.listViewTvChannels.SelectedIndexChanged +=
        new System.EventHandler(this.listViewTvChannels_SelectedIndexChanged);
      this.listViewTvChannels.ColumnClick +=
        new System.Windows.Forms.ColumnClickEventHandler(this.listViewTvChannels_ColumnClick);
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
      this.listViewTVChannelsForCard.AllowRowReorder = false;
      this.listViewTVChannelsForCard.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewTVChannelsForCard.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                                        {
                                                          this.columnHeaderAssignedTvChannels
                                                        });
      this.listViewTVChannelsForCard.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
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
      this.listViewTVChannelsCard.AllowRowReorder = false;
      this.listViewTVChannelsCard.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewTVChannelsCard.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                                     {
                                                       this.columnHeaderAvailableTvChannels
                                                     });
      this.listViewTVChannelsCard.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
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
      this.comboBoxCard.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
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

    private void buttonAddChannel_Click(object sender, EventArgs e)
    {
      _itemsModified = true;

      EditTVChannelForm editChannel = new EditTVChannelForm();

      editChannel.SortingPlace = listViewTvChannels.Items.Count;
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

    private void buttonEditChannel_Click(object sender, EventArgs e)
    {
      _itemsModified = true;

      foreach (ListViewItem listItem in listViewTvChannels.SelectedItems)
      {
        EditTVChannelForm editChannel = new EditTVChannelForm();
        editChannel.Channel = listItem.Tag as TelevisionChannel;
        editChannel.SortingPlace = listItem.Index;
        ;

        DialogResult dialogResult = editChannel.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          TelevisionChannel editedChannel = editChannel.Channel;
          listItem.Tag = editedChannel;

          listItem.SubItems[0].Text = editedChannel.Name;
          listItem.SubItems[1].Text = editedChannel.External
                                        ? String.Format("{0}/{1}", editedChannel.Channel,
                                                        editedChannel.ExternalTunerChannel)
                                        : editedChannel.Channel.ToString();
          listItem.SubItems[2].Text = GetStandardName(editedChannel.standard);
          listItem.SubItems[3].Text = editedChannel.External ? "External" : "Internal";
          listItem.ImageIndex = 0;
          if (editedChannel.Scrambled)
          {
            listItem.ImageIndex = 1;
          }

          SaveSettings();
        }
      }
    }

    private void buttonDeleteChannel_Click(object sender, EventArgs e)
    {
      listViewTvChannels.BeginUpdate();

      _itemsModified = true;

      int itemCount = listViewTvChannels.SelectedItems.Count;

      for (int index = 0; index < itemCount; index++)
      {
        listViewTvChannels.Items.RemoveAt(listViewTvChannels.SelectedIndices[0]);
      }

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
              (channel.Channel < (int) ExternalInputs.svhs) &&
              (channel.Channel > highestChannelNumber))
          {
            highestChannelNumber = channel.Channel;
          }
        }
        return highestChannelNumber;
      }
    }

    public override void LoadSettings()
    {
      if (!_init)
      {
        return;
      }

      LoadTVChannels();
      LoadCards();
    }

    public override void SaveSettings()
    {
      if (!_init)
      {
        return;
      }

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
        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          countryCode = xmlreader.GetValueAsInt("capture", "country", 31);
        }

        string[] registryLocations = new string[]
                                       {
                                         String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-1",
                                                       countryCode),
                                         String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-0",
                                                       countryCode),
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
          foreach (TVChannel channel in channels)
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
            {
              TVDatabase.RemoveChannel(channel.Name);
            }
          }

          //
          // Remove channel frequencies from the registry
          //
          for (int index = 0; index < registryLocations.Length; index++)
          {
            using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(registryLocations[index]))
            {
              for (int channelIndex = 0; channelIndex < 200; channelIndex++)
              {
                registryKey.DeleteValue(channelIndex.ToString(), false);
              }
            }
          }
        }

        //
        // Add current channels
        //
        TVDatabase.GetChannels(ref channels);
        foreach (ListViewItem listItem in listViewTvChannels.Items)
        {
          TVChannel channel = new TVChannel();
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
            if (tvChannel.Frequency.Hertz < 1000)
            {
              tvChannel.Frequency.Hertz *= 1000000L;
            }

            channel.Frequency = tvChannel.Frequency.Hertz;
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
          using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(registryLocations[index]))
          {
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
                    tvChannel.Channel != (int) ExternalInputs.svhs &&
                    tvChannel.Channel != (int) ExternalInputs.cvbs1 &&
                    tvChannel.Channel != (int) ExternalInputs.cvbs2 &&
                    tvChannel.Channel != (int) ExternalInputs.rgb)
                {
                  registryKey.SetValue(tvChannel.Channel.ToString(), (int) tvChannel.Frequency.Hertz);
                }
              }
            }
          }
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
        ListViewItem listItem = new ListViewItem(new string[]
                                                   {
                                                     tvChannel.Name,
                                                     tvChannel.External
                                                       ? String.Format("{0}/{1}", tvChannel.Channel,
                                                                       tvChannel.ExternalTunerChannel)
                                                       : tvChannel.Channel.ToString(),
                                                     GetStandardName(tvChannel.standard),
                                                     tvChannel.External ? "External" : "Internal"
                                                   });
        listItem.Checked = tvChannel.VisibleInGuide;
        listItem.ImageIndex = 0;
        if (tvChannel.Scrambled)
        {
          listItem.ImageIndex = 1;
        }

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
    private void listViewTvChannels_DoubleClick(object sender, EventArgs e)
    {
      if (listViewTvChannels.SelectedItems.Count > 0)
      {
        listViewTvChannels.SelectedItems[0].Checked = !listViewTvChannels.SelectedItems[0].Checked;
        buttonEditChannel_Click(sender, e);
      }
    }

    private void buttonChannelUp_Click(object sender, EventArgs e)
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

      SaveSettings();

      listViewTvChannels.EndUpdate();
    }

    private void buttonChannelDown_Click(object sender, EventArgs e)
    {
      listViewTvChannels.BeginUpdate();

      _itemsModified = true;

      for (int index = listViewTvChannels.Items.Count - 1; index >= 0; index--)
      {
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
            {
              listViewTvChannels.Items.Insert(index + 1, listItem);
            }
            else
            {
              listViewTvChannels.Items.Add(listItem);
            }
          }
        }
      }

      SaveSettings();

      listViewTvChannels.EndUpdate();
    }

    private void listViewTvChannels_SelectedIndexChanged(object sender, EventArgs e)
    {
      buttonDeleteChannel.Enabled =
        buttonEditChannel.Enabled =
        buttonChannelUp.Enabled = buttonChannelDown.Enabled = (listViewTvChannels.SelectedItems.Count > 0);
      buttonCombine.Enabled = (listViewTvChannels.SelectedItems.Count == 2);
    }

    private void buttonImportFromTvGuide_Click(object sender, EventArgs e)
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
      {
        if (strPath[strPath.Length - 1] == '\\' || strPath[strPath.Length - 1] == '/')
        {
          strPath = strPath.Substring(0, strPath.Length - 1);
        }
        else
        {
          break;
        }
      }

      return strPath;
    }

    private void buttonClearChannels_Click(object sender, EventArgs e)
    {
      DialogResult result = MessageBox.Show(this, "Are you sure you want to delete all channels?", "Delete channels",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (result != DialogResult.Yes)
      {
        return;
      }

      listViewTvChannels.Items.Clear();

      SaveSettings();
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
        LoadTVChannels();
        LoadCards();
      }
      base.OnPaint(e);
    }

    private void listViewTvChannels_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (_columnSorter == null)
      {
        listViewTvChannels.ListViewItemSorter = _columnSorter = new ListViewColumnSorter();
      }

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
      listViewTvChannels.Sort();
      listViewTvChannels.Update();

      //SaveSettings();
    }

    private void buttonAddCvbsSvhs_Click(object sender, EventArgs e)
    {
      SaveSettings();
      TVChannel chan = new TVChannel();

      chan.Name = "CVBS#1";
      chan.Number = (int) ExternalInputs.cvbs1;
      TVDatabase.AddChannel(chan);

      chan.Name = "CVBS#2";
      chan.Number = (int) ExternalInputs.cvbs2;
      TVDatabase.AddChannel(chan);

      chan.Name = "SVHS";
      chan.Number = (int) ExternalInputs.svhs;
      TVDatabase.AddChannel(chan);

      chan.Name = "RGB";
      chan.Number = (int) ExternalInputs.rgb;
      TVDatabase.AddChannel(chan);

      LoadSettings();
    }

    private void LoadCards()
    {
      comboBoxCard.BeginUpdate();

      comboBoxCard.Items.Clear();

      if (File.Exists(Config.GetFile(Config.Dir.Config, "capturecards.xml")))
      {
        using (
          FileStream fileStream = new FileStream(Config.GetFile(Config.Dir.Config, "capturecards.xml"), FileMode.Open,
                                                 FileAccess.Read, FileShare.ReadWrite))
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
            captureCards = (ArrayList) formatter.Deserialize(fileStream);
            for (int i = 0; i < captureCards.Count; i++)
            {
              ((TVCaptureDevice) captureCards[i]).ID = (i + 1);

              TVCaptureDevice device = (TVCaptureDevice) captureCards[i];
              ComboCard combo = new ComboCard();
              combo.FriendlyName = device.FriendlyName;
              combo.VideoDevice = device.VideoDevice;
              combo.ID = device.ID;
              comboBoxCard.Items.Add(combo);
            }
            fileStream.Close();
          }
          catch
          {
            MessageBox.Show(
              "Failed to load previously configured capture card(s), you have to reconfigure your device(s).",
              "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Log.Error("Recorder: LoadCards()");
          }
        }
      }

      if (comboBoxCard.Items.Count != 0)
      {
        comboBoxCard.SelectedIndex = 0;
      }

      comboBoxCard.EndUpdate();

      FillInChannelCardMappings();
    }

    private void FillInChannelCardMappings()
    {
      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard) comboBoxCard.Items[index];
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

      listViewTVChannelsForCard.EndUpdate();
      listViewTVChannelsCard.EndUpdate();
    }

    private void comboBoxCard_SelectedIndexChanged(object sender, EventArgs e)
    {
      FillInChannelCardMappings();
    }

    private void buttonMap_Click(object sender, EventArgs e)
    {
      if (listViewTVChannelsCard.SelectedItems == null)
      {
        return;
      }

      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard) comboBoxCard.Items[index];
        card = combo.ID;
      }

      listViewTVChannelsForCard.BeginUpdate();
      listViewTVChannelsCard.BeginUpdate();

      for (int i = 0; i < listViewTVChannelsCard.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewTVChannelsCard.SelectedItems[i];
        TVChannel chan = (TVChannel) listItem.Tag;

        listItem = new ListViewItem(new string[] {chan.Name});
        listItem.Tag = chan;
        listViewTVChannelsForCard.Items.Add(listItem);
        if (chan != null)
        {
          TVDatabase.MapChannelToCard(chan.ID, card);
        }
      }

      for (int i = listViewTVChannelsCard.SelectedItems.Count - 1; i >= 0; i--)
      {
        ListViewItem listItem = listViewTVChannelsCard.SelectedItems[i];
        listViewTVChannelsCard.Items.Remove(listItem);
      }

      //SaveSettings();

      listViewTVChannelsCard.EndUpdate();
      listViewTVChannelsForCard.EndUpdate();
    }

    private void buttonUnmap_Click(object sender, EventArgs e)
    {
      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard) comboBoxCard.Items[index];
        card = combo.ID;
      }

      if (listViewTVChannelsForCard.SelectedItems == null)
      {
        return;
      }

      listViewTVChannelsForCard.BeginUpdate();
      listViewTVChannelsCard.BeginUpdate();

      for (int i = 0; i < listViewTVChannelsForCard.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewTVChannelsForCard.SelectedItems[i];
        TVChannel chan = (TVChannel) listItem.Tag;

        listItem = new ListViewItem(new string[] {chan.Name});
        listItem.Tag = chan;
        listViewTVChannelsCard.Items.Add(listItem);
      }

      for (int i = listViewTVChannelsForCard.SelectedItems.Count - 1; i >= 0; --i)
      {
        ListViewItem listItem = listViewTVChannelsForCard.SelectedItems[i];
        TVChannel channel = listItem.Tag as TVChannel;
        if (channel != null)
        {
          TVDatabase.UnmapChannelFromCard(channel, card);
        }
        listViewTVChannelsForCard.Items.Remove(listItem);
      }

      //SaveSettings();

      listViewTVChannelsCard.EndUpdate();
      listViewTVChannelsForCard.EndUpdate();
    }

    private void tabControlTvChannels_SelectedIndexChanged(object sender, EventArgs e)
    {
      SaveSettings();
      LoadSettings();
    }

    private int ExportChannels(Settings exportFile)
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
          exportFile.SetValue(listItem.Index.ToString(), "External Tuner Channel",
                              selectedChannel.ExternalTunerChannel.ToString());
          exportFile.SetValue(listItem.Index.ToString(), "Frequency", selectedChannel.Frequency.Hertz.ToString());
          exportFile.SetValue(listItem.Index.ToString(), "Analog Standard Index", selectedChannel.standard.ToString());
          exportFile.SetValueAsBool(listItem.Index.ToString(), "Visible in Guide", selectedChannel.VisibleInGuide);

          if (selectedChannel.Channel >= 0)
          {
            int bandWidth,
                freq,
                onId,
                tsId,
                sId,
                symbolRate,
                innerFec,
                modulation,
                audioPid,
                videoPid,
                teletextPid,
                pmtPid;
            string provider;
            int audio1, audio2, audio3, ac3Pid, pcrPid;
            string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
            bool hasEitPresentFollow, hasEitSchedule;
            //DVB-T
            TVDatabase.GetDVBTTuneRequest(selectedChannel.ID, out provider, out freq, out onId, out tsId, out sId,
                                          out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth,
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
            TVDatabase.GetDVBCTuneRequest(selectedChannel.ID, out provider, out freq, out symbolRate, out innerFec,
                                          out modulation,
                                          out onId, out tsId, out sId, out audioPid, out videoPid, out teletextPid,
                                          out pmtPid,
                                          out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage,
                                          out audioLanguage1,
                                          out audioLanguage2, out audioLanguage3, out hasEitPresentFollow,
                                          out hasEitSchedule, out pcrPid);
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
            TVDatabase.GetATSCTuneRequest(selectedChannel.ID, out physicalChannel, out provider, out freq,
                                          out symbolRate, out innerFec,
                                          out modulation, out onId, out tsId, out sId, out audioPid, out videoPid,
                                          out teletextPid, out pmtPid,
                                          out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage,
                                          out audioLanguage1,
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

    private int ExportGroups(Settings exportFile)
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

    private int ExportCards(Settings exportFile)
    {
      int exportedCardsCounter = 0;

      //Card mapping data
      ArrayList cards = new ArrayList();
      TVDatabase.GetCards(ref cards);

      //If we have no cards skip this
      if (cards.Count > 0)
      {
        exportFile.SetValue("CARDS", "TOTAL", cards.Count.ToString());
      }

      for (int i = 1; i < cards.Count + 1; i++)
      {
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
        catch
        {
        }
      }

      return exportedCardsCounter;
    }

    private int ExportRecordedTv(Settings exportFile)
    {
      int exportedRecordedTvCounter = 0;

      //Backup recorded shows information
      ArrayList Recorded = new ArrayList();
      TVDatabase.GetRecordedTV(ref Recorded);

      if (Recorded.Count > 0)
      {
        exportFile.SetValue("RECORDED", "TOTAL", Recorded.Count.ToString());
      }

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

    private int ExportScheduledRecordings(Settings exportFile)
    {
      int exportedScheduledRecordings = 0;

      //Backup recording shows information
      ArrayList Recordings = new ArrayList();
      TVDatabase.GetRecordings(ref Recordings);

      if (Recordings.Count > 0)
      {
        exportFile.SetValue("RECORDINGS", "TOTAL", Recordings.Count.ToString());
      }

      for (int i = 1; i < Recordings.Count + 1; i++)
      {
        TV.Database.TVRecording scheduledRecording = (TV.Database.TVRecording) Recordings[i - 1];
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
        exportFile.SetValue("Recording " + i.ToString(), "CANCELED SERIES TOTAL",
                            scheduledRecording.CanceledSeries.Count.ToString());
        if (scheduledRecording.CanceledSeries.Count > 0)
        {
          int canceledCount = 0;
          List<long> getScheduledRecording = scheduledRecording.CanceledSeries;
          foreach (long canxScheduledRecording in getScheduledRecording)
          {
            exportFile.SetValue("Recording " + i.ToString(), "CANCELED SERIES CANCELEDTIME " + canceledCount.ToString(),
                                canxScheduledRecording.ToString());
            canceledCount++;
          }
        }
        exportedScheduledRecordings++;
      }

      return exportedScheduledRecordings;
    }

    private void ExportToXml(string fileName)
    {
      if (File.Exists(fileName))
      {
        File.Delete(fileName);
      }

      int exportedChannels = 0;
      int exportedGroups = 0;
      int exportedCards = 0;
      int exportedRecordedTv = 0;
      int exportedScheduledRecordings = 0;

      //Current version number of this exporter (change when needed)
      int currentVersion = 1; //<--- Make sure this same number is given to Import_from_XML

      SaveSettings();

      using (Settings exportFile = new Settings(fileName, false))
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
        MessageBox.Show("No items could be exported", "Export Results", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
      else
      {
        string messageText = "Exported successfully:                  \n";
        if (exportedChannels > 0)
        {
          messageText += string.Format("\n{0} TV channel", exportedChannels);
          if (exportedChannels > 1)
          {
            messageText += "s";
          }
        }
        if (exportedGroups > 0)
        {
          messageText += string.Format("\n{0} TV channel group", exportedGroups);
          if (exportedGroups > 1)
          {
            messageText += "s";
          }
        }
        if (exportedCards > 0)
        {
          messageText += string.Format("\n{0} assigned capture card", exportedCards);
          if (exportedCards > 1)
          {
            messageText += "s";
          }
        }
        if (exportedRecordedTv > 0)
        {
          messageText += string.Format("\n{0} completed recording", exportedRecordedTv);
          if (exportedRecordedTv > 1)
          {
            messageText += "s";
          }
        }
        if (exportedScheduledRecordings > 0)
        {
          messageText += string.Format("\n{0} scheduled recording", exportedScheduledRecordings);
          if (exportedScheduledRecordings > 1)
          {
            messageText += "s";
          }
        }

        MessageBox.Show(messageText, "Export Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private bool ImportChannelDvbT(Settings importFile, TelevisionChannel importChannel, int channelIndex)
    {
      int freq, ONID, TSID, SID;
      int bandWidth, pmtPid, audioPid, videoPid, teletextPid;
      int audio1, audio2, audio3, ac3Pid, pcrPid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      string provider;
      bool HasEITPresentFollow, HasEITSchedule;

      try
      {
        freq = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTFreq", 0);
        ONID = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTONID", 0);
        TSID = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTTSID", 0);
        SID = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTSID", 0);
        audioPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTAudioPid", 0);
        videoPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTVideoPid", 0);
        teletextPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTTeletextPid", 0);
        pmtPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTPmtPid", 0);
        provider = importFile.GetValueAsString(channelIndex.ToString(), "DVBTProvider", "");
        bandWidth = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTBandwidth", -1);
        audio1 = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTAudio1Pid", -1);
        audio2 = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTAudio2Pid", -1);
        audio3 = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTAudio3Pid", -1);
        ac3Pid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTAC3Pid", -1);
        pcrPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBTPCRPid", -1);
        audioLanguage = importFile.GetValueAsString(channelIndex.ToString(), "DVBTAudioLanguage", "");
        audioLanguage1 = importFile.GetValueAsString(channelIndex.ToString(), "DVBTAudioLanguage1", "");
        audioLanguage2 = importFile.GetValueAsString(channelIndex.ToString(), "DVBTAudioLanguage2", "");
        audioLanguage3 = importFile.GetValueAsString(channelIndex.ToString(), "DVBTAudioLanguage3", "");
        HasEITPresentFollow = importFile.GetValueAsBool(channelIndex.ToString(), "DVBTHasEITPresentFollow", false);
        HasEITSchedule = importFile.GetValueAsBool(channelIndex.ToString(), "DVBTHasEITSchedule", false);
        if (ONID > 0 && TSID > 0 && SID > 0 && freq > 0)
        {
          TVDatabase.MapDVBTChannel(importChannel.Name, provider, importChannel.ID, freq, ONID, TSID, SID, audioPid,
                                    videoPid, teletextPid, pmtPid, bandWidth, audio1, audio2, audio3, ac3Pid, pcrPid,
                                    audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3, HasEITPresentFollow,
                                    HasEITSchedule);
        }
      }
      catch (Exception)
      {
        return false;
      }
      return true;
    }

    private bool ImportChannelDvbC(Settings importFile, TelevisionChannel importChannel, int channelIndex)
    {
      int freq, ONID, TSID, SID, symbolrate, innerFec, modulation;
      int pmtPid, audioPid, videoPid, teletextPid;
      int audio1, audio2, audio3, ac3Pid, pcrPid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      string provider;
      bool HasEITPresentFollow, HasEITSchedule;

      try
      {
        freq = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCFreq", 0);
        ONID = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCONID", 0);
        TSID = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCTSID", 0);
        SID = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCSID", 0);
        symbolrate = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCSR", 0);
        innerFec = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCInnerFeq", 0);
        modulation = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCModulation", 0);
        provider = importFile.GetValueAsString(channelIndex.ToString(), "DVBCProvider", "");
        audioPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCAudioPid", 0);
        videoPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCVideoPid", 0);
        teletextPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCTeletextPid", 0);
        pmtPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCPmtPid", 0);
        audio1 = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCAudio1Pid", -1);
        audio2 = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCAudio2Pid", -1);
        audio3 = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCAudio3Pid", -1);
        ac3Pid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCAC3Pid", -1);
        pcrPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBCPCRPid", -1);
        audioLanguage = importFile.GetValueAsString(channelIndex.ToString(), "DVBCAudioLanguage", "");
        audioLanguage1 = importFile.GetValueAsString(channelIndex.ToString(), "DVBCAudioLanguage1", "");
        audioLanguage2 = importFile.GetValueAsString(channelIndex.ToString(), "DVBCAudioLanguage2", "");
        audioLanguage3 = importFile.GetValueAsString(channelIndex.ToString(), "DVBCAudioLanguage3", "");
        HasEITPresentFollow = importFile.GetValueAsBool(channelIndex.ToString(), "DVBCHasEITPresentFollow", false);
        HasEITSchedule = importFile.GetValueAsBool(channelIndex.ToString(), "DVBCHasEITSchedule", false);

        if (ONID > 0 && TSID > 0 && SID > 0 && freq > 0)
        {
          TVDatabase.MapDVBCChannel(importChannel.Name, provider, importChannel.ID, freq, symbolrate, innerFec,
                                    modulation, ONID, TSID, SID, audioPid, videoPid, teletextPid, pmtPid, audio1, audio2,
                                    audio3, ac3Pid, pcrPid, audioLanguage, audioLanguage1, audioLanguage2,
                                    audioLanguage3, HasEITPresentFollow, HasEITSchedule);
        }
      }
      catch (Exception)
      {
        return false;
      }
      return true;
    }

    private bool ImportChannelDvbS(Settings importFile, TelevisionChannel importChannel, int channelIndex)
    {
      int freq, ONID, TSID, SID, symbolrate, innerFec, polarisation;
      int pmtPid, audioPid, videoPid, teletextPid;
      int audio1, audio2, audio3, ac3Pid, pcrPid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      string provider;
      bool HasEITPresentFollow, HasEITSchedule;

      try
      {
        DVBChannel ch = new DVBChannel();
        TVDatabase.GetSatChannel(importChannel.ID, 1, ref ch);

        freq = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSFreq", 0);
        ONID = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSONID", 0);
        TSID = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSTSID", 0);
        SID = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSSID", 0);
        symbolrate = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSSymbolrate", 0);
        innerFec = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSInnerFec", 0);
        polarisation = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSPolarisation", 0);
        provider = importFile.GetValueAsString(channelIndex.ToString(), "DVBSProvider", "");
        audioPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSAudioPid", 0);
        videoPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSVideoPid", 0);
        teletextPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSTeletextPid", 0);
        pmtPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSPmtPid", 0);
        audio1 = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSAudio1Pid", -1);
        audio2 = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSAudio2Pid", -1);
        audio3 = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSAudio3Pid", -1);
        ac3Pid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSAC3Pid", -1);
        pcrPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSPCRPid", -1);
        audioLanguage = importFile.GetValueAsString(channelIndex.ToString(), "DVBSAudioLanguage", "");
        audioLanguage1 = importFile.GetValueAsString(channelIndex.ToString(), "DVBSAudioLanguage1", "");
        audioLanguage2 = importFile.GetValueAsString(channelIndex.ToString(), "DVBSAudioLanguage2", "");
        audioLanguage3 = importFile.GetValueAsString(channelIndex.ToString(), "DVBSAudioLanguage3", "");
        HasEITPresentFollow = importFile.GetValueAsBool(channelIndex.ToString(), "DVBSHasEITPresentFollow", false);
        HasEITSchedule = importFile.GetValueAsBool(channelIndex.ToString(), "DVBSHasEITSchedule", false);

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
          ch.ECMPid = importFile.GetValueAsInt(channelIndex.ToString(), "DVBSECMpid", 0);
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
        return false;
      }
      return true;
    }

    private bool ImportChannelAtsc(Settings importFile, TelevisionChannel importChannel, int channelIndex)
    {
      int freq, ONID, TSID, SID, symbolrate, innerFec, modulation;
      int pmtPid, audioPid, videoPid, teletextPid;
      int audio1, audio2, audio3, ac3Pid, pcrPid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      string provider;
      bool HasEITPresentFollow, HasEITSchedule;

      try
      {
        int minorChannel, majorChannel, physicalChannel;
        physicalChannel = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCPhysical", -1);
        provider = importFile.GetValueAsString(channelIndex.ToString(), "ATSCProvider", "");
        freq = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCFreq", 0);
        symbolrate = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCSymbolrate", 0);
        innerFec = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCInnerFec", 0);
        modulation = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCModulation", 0);

        ONID = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCONID", 0);
        TSID = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCTSID", 0);
        SID = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCSID", 0);
        audioPid = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCAudioPid", 0);
        videoPid = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCVideoPid", 0);
        teletextPid = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCTeletextPid", 0);
        pmtPid = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCPmtPid", 0);
        audio1 = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCAudio1Pid", -1);
        audio2 = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCAudio2Pid", -1);
        audio3 = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCAudio3Pid", -1);
        ac3Pid = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCAC3Pid", -1);
        audioLanguage = importFile.GetValueAsString(channelIndex.ToString(), "ATSCAudioLanguage", "");
        audioLanguage1 = importFile.GetValueAsString(channelIndex.ToString(), "ATSCAudioLanguage1", "");
        audioLanguage2 = importFile.GetValueAsString(channelIndex.ToString(), "ATSCAudioLanguage2", "");
        audioLanguage3 = importFile.GetValueAsString(channelIndex.ToString(), "ATSCAudioLanguage3", "");
        minorChannel = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCMinor", -1);
        majorChannel = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCMajor", -1);
        HasEITPresentFollow = importFile.GetValueAsBool(channelIndex.ToString(), "ATSCHasEITPresentFollow", false);
        HasEITSchedule = importFile.GetValueAsBool(channelIndex.ToString(), "ATSCHasEITSchedule", false);
        pcrPid = importFile.GetValueAsInt(channelIndex.ToString(), "ATSCPCRPid", -1);

        if (physicalChannel > 0 && minorChannel >= 0 && majorChannel >= 0)
        {
          TVDatabase.MapATSCChannel(importChannel.Name, physicalChannel, minorChannel, majorChannel, provider,
                                    importChannel.ID,
                                    freq, symbolrate, innerFec, modulation, ONID, TSID, SID, audioPid, videoPid,
                                    teletextPid,
                                    pmtPid, audio1, audio2, audio3, ac3Pid, pcrPid, audioLanguage, audioLanguage1,
                                    audioLanguage2,
                                    audioLanguage3, HasEITPresentFollow, HasEITSchedule);
        }
      }
      catch (Exception)
      {
        return false;
      }
      return true;
    }

    private int ImportChannels(Settings importFile)
    {
      //Count how many channels we have to import
      int counter = 0;
      for (int i = 0;; i++)
      {
        if (importFile.GetValueAsInt(i.ToString(), "INDEX", -1) == -1)
        {
          if (counter == 0)
          {
            return counter;
          }
          else
          {
            break;
          }
        }
        else
        {
          counter++;
        }
      }

      listViewTvChannels.BeginUpdate();

      for (int channelIndex = 0; channelIndex < counter; channelIndex++)
      {
        TelevisionChannel importChannel = new TelevisionChannel();
        importChannel.ID = importFile.GetValueAsInt(channelIndex.ToString(), "ID", 0);
        importChannel.Channel = importFile.GetValueAsInt(channelIndex.ToString(), "Number", 0);
        importChannel.Name = importFile.GetValueAsString(channelIndex.ToString(), "Name", "");
        importChannel.Country = importFile.GetValueAsInt(channelIndex.ToString(), "Country", 0);
        importChannel.External = importFile.GetValueAsBool(channelIndex.ToString(), "External", false);
        importChannel.ExternalTunerChannel = importFile.GetValueAsString(channelIndex.ToString(),
                                                                         "External Tuner Channel", "");
        importChannel.Frequency.Hertz = importFile.GetValueAsInt(channelIndex.ToString(), "Frequency", 0);
        importChannel.standard =
          ConvertAvs(importFile.GetValueAsString(channelIndex.ToString(), "Analog Standard Index", "None"));
        importChannel.VisibleInGuide = importFile.GetValueAsBool(channelIndex.ToString(), "Visible in Guide", false);
        importChannel.Scrambled = importFile.GetValueAsBool(channelIndex.ToString(), "Scrambled", false);

        TelevisionChannel editedChannel = importChannel;
        ListViewItem listItem = new ListViewItem(new string[]
                                                   {
                                                     editedChannel.Name,
                                                     editedChannel.External
                                                       ? String.Format("{0}/{1}", editedChannel.Channel,
                                                                       editedChannel.ExternalTunerChannel)
                                                       : editedChannel.Channel.ToString(),
                                                     GetStandardName(editedChannel.standard),
                                                     editedChannel.External ? "External" : "Internal"
                                                   });
        listItem.Tag = editedChannel;

        listViewTvChannels.Items.Add(listItem);
        listViewTvChannels.Items[listViewTvChannels.Items.IndexOf(listItem)].Checked = true;

        if (importChannel.Channel >= 0)
        {
          ImportChannelDvbT(importFile, importChannel, channelIndex);
          ImportChannelDvbC(importFile, importChannel, channelIndex);
          ImportChannelDvbS(importFile, importChannel, channelIndex);
          ImportChannelAtsc(importFile, importChannel, channelIndex);
        }
      }
      SaveTVChannels();
      _reloadList = true;
      SaveSettings();
      listViewTvChannels.EndUpdate();

      return counter;
    }

    private int ImportGroups(Settings importFile)
    {
      int totalGroups = importFile.GetValueAsInt("GROUPS", "TOTAL", -1);

      if (totalGroups > 0)
      {
        for (int groupIndex = 1; groupIndex <= totalGroups; groupIndex++)
        {
          TVGroup importGroup = new TVGroup();
          importGroup.ID = importFile.GetValueAsInt("Group " + groupIndex.ToString(), "ID", 0);
          importGroup.GroupName = importFile.GetValueAsString("Group " + groupIndex.ToString(), "NAME", "");
          importGroup.Pincode = importFile.GetValueAsInt("Group " + groupIndex.ToString(), "PINCODE", 0);
          importGroup.Sort = importFile.GetValueAsInt("Group " + groupIndex.ToString(), "SORT", 0);

          TVDatabase.AddGroup(importGroup);

          ArrayList groupChannels = new ArrayList();
          TVDatabase.GetChannels(ref groupChannels);

          int totalChannels = importFile.GetValueAsInt("Group " + groupIndex.ToString(), "TOTAL CHANNELS", 0);

          if (totalChannels > 0)
          {
            for (int channelIndex = 0; channelIndex < totalChannels; channelIndex++)
            {
              int channelId = importFile.GetValueAsInt("Group " + groupIndex.ToString(),
                                                       "CHANNEL " + channelIndex.ToString(), 0);

              //Locate Channel so it can be added to group
              foreach (TVChannel databaseChannel in groupChannels)
              {
                if (databaseChannel.ID == channelId)
                {
                  importGroup.TvChannels.Add(databaseChannel);

                  //Have to re-grab group from database in order to map correctly
                  ArrayList databaseGroups = new ArrayList();
                  TVDatabase.GetGroups(ref databaseGroups);

                  foreach (TVGroup databaseGroup in databaseGroups)
                  {
                    if (importGroup.ID == databaseGroup.ID)
                    {
                      TVDatabase.MapChannelToGroup(databaseGroup, databaseChannel);
                    }
                  }
                }
              }
            }
          }
        }
      }
      return totalGroups;
    }

    private int ImportCards(Settings importFile)
    {
      int totalCards = importFile.GetValueAsInt("CARDS", "TOTAL", -1);

      if (totalCards > 0)
      {
        for (int cardsIndex = 1; cardsIndex < totalCards + 1; cardsIndex++)
        {
          //Re-Map channels to available cards
          ArrayList cardChannels = new ArrayList();
          TVDatabase.GetChannels(ref cardChannels);
          int channelIndex = importFile.GetValueAsInt("Card " + cardsIndex.ToString(), "TOTAL CHANNELS", 0);

          if (channelIndex > 0)
          {
            for (int j = 0; j < channelIndex; j++)
            {
              int channelId = importFile.GetValueAsInt("Card " + cardsIndex.ToString(), "CHANNEL " + j.ToString(), 0);

              //Locate Channel so it can be added to Card
              foreach (TVChannel databaseChannel in cardChannels)
              {
                if (databaseChannel.ID == channelId)
                {
                  TVDatabase.MapChannelToCard(databaseChannel.ID, cardsIndex);
                }
              }
            }
          }
        }
      }
      return totalCards;
    }

    private int ImportRecordedTv(Settings importFile)
    {
      int totalRecTv = importFile.GetValueAsInt("RECORDED", "TOTAL", -1);

      if (totalRecTv > 0)
      {
        for (int recTvIndex = 1; recTvIndex < totalRecTv + 1; recTvIndex++)
        {
          //Create temp TVRecorded to hold data to import
          TVRecorded importedRecTv = new TVRecorded();
          importedRecTv.ID = importFile.GetValueAsInt("Recorded " + recTvIndex.ToString(), "ID", 0);
          importedRecTv.Title = importFile.GetValueAsString("Recorded " + recTvIndex.ToString(), "TITLE", "");
          importedRecTv.Channel = importFile.GetValueAsString("Recorded " + recTvIndex.ToString(), "CHANNEL", "");
          importedRecTv.Description = importFile.GetValueAsString("Recorded " + recTvIndex.ToString(), "DESC", "");
          importedRecTv.Genre = importFile.GetValueAsString("Recorded " + recTvIndex.ToString(), "GENRE", "");
          importedRecTv.FileName = importFile.GetValueAsString("Recorded " + recTvIndex.ToString(), "FILENAME", "");
          importedRecTv.Start =
            Convert.ToInt64(importFile.GetValueAsString("Recorded " + recTvIndex.ToString(), "STARTTIME", "0"));
          if (importedRecTv.Start == 0)
          {
            importedRecTv.Start =
              Convert.ToInt64(importFile.GetValueAsString("Recorded " + recTvIndex.ToString(), "START", "0"));
          }
          importedRecTv.End =
            Convert.ToInt64(importFile.GetValueAsString("Recorded " + recTvIndex.ToString(), "ENDTIME", "0"));
          importedRecTv.Played = importFile.GetValueAsInt("Recorded " + recTvIndex.ToString(), "PLAYED", 0);

          //Add or gathered info to the TVDatabase
          ArrayList recordedTv = new ArrayList();
          TVDatabase.GetRecordedTV(ref recordedTv);
          TVRecorded databaseRecordedTv = new TVRecorded();
          TVDatabase.AddRecordedTV(importedRecTv);
        }
      }
      return totalRecTv;
    }

    private int ImportScheduledRecordings(Settings importFile)
    {
      int totalScheduled = importFile.GetValueAsInt("RECORDINGS", "TOTAL", -1);

      if (totalScheduled > 0)
      {
        for (int scheduledIndex = 1; scheduledIndex < totalScheduled + 1; scheduledIndex++)
        {
          //Create temp TVRecording to hold data to import
          TV.Database.TVRecording importedScheduled = new TV.Database.TVRecording();
          importedScheduled.ID = importFile.GetValueAsInt("Recording " + scheduledIndex.ToString(), "ID", 0);
          importedScheduled.Title = importFile.GetValueAsString("Recording " + scheduledIndex.ToString(), "TITLE", "");
          importedScheduled.Channel = importFile.GetValueAsString("Recording " + scheduledIndex.ToString(), "CHANNEL",
                                                                  "");
          importedScheduled.Start =
            Convert.ToInt64(importFile.GetValueAsString("Recording " + scheduledIndex.ToString(), "STARTTIME", "0"));
          importedScheduled.End =
            Convert.ToInt64(importFile.GetValueAsString("Recording " + scheduledIndex.ToString(), "ENDTIME", "0"));
          importedScheduled.Canceled =
            Convert.ToInt64(importFile.GetValueAsString("Recording " + scheduledIndex.ToString(), "CANCELEDTIME", "0"));
          importedScheduled.RecType =
            ConvertRecordingType(importFile.GetValueAsString("Recording " + scheduledIndex.ToString(), "TYPE", ""));
          importedScheduled.Priority = importFile.GetValueAsInt("Recording " + scheduledIndex.ToString(), "PRIORITY", 0);
          importedScheduled.Quality =
            ConvertQualityType(importFile.GetValueAsString("Recording " + scheduledIndex.ToString(), "QUALITY", ""));
          importedScheduled.IsContentRecording = importFile.GetValueAsBool("Recording " + scheduledIndex.ToString(),
                                                                           "ISCONTENTREC", false);
          importedScheduled.Series = importFile.GetValueAsBool("Recording " + scheduledIndex.ToString(), "SERIES", false);
          importedScheduled.EpisodesToKeep = importFile.GetValueAsInt("Recording " + scheduledIndex.ToString(),
                                                                      "EPISODES", Int32.MaxValue);

          //Add this recording to TVDatabase
          ArrayList checkRecordingList = new ArrayList();
          TVDatabase.GetRecordings(ref checkRecordingList);
          TV.Database.TVRecording checkRecording = new TV.Database.TVRecording();

          //Check if this recording has had any cancels
          int canceledCount = importFile.GetValueAsInt("Recording " + scheduledIndex.ToString(), "CANCELED SERIES TOTAL",
                                                       0);
          if (canceledCount > 0)
          {
            importedScheduled.CanceledSeries.Clear();
            long lastCanceledTime = 0;
            for (int canceledIndex = 0; canceledIndex < canceledCount; canceledIndex++)
            {
              //Add the canceled time to TVDatabase
              long canceledTime = 0;
              canceledTime =
                Convert.ToInt64(importFile.GetValueAsString("Recording " + scheduledIndex.ToString(),
                                                            "CANCELED SERIES CANCELEDTIME " + canceledIndex.ToString(),
                                                            "0"));
              //Check if we had the same time from before if so stop adding
              if (canceledTime == lastCanceledTime)
              {
                break;
              }
              importedScheduled.CanceledSeries.Add((long) canceledTime);
              lastCanceledTime = canceledTime;
            }
          }
          TVDatabase.AddRecording(ref importedScheduled);
        }
      }
      return totalScheduled;
    }

    private void ImportFromXml(string fileName)
    {
      //Check if we have a file just in case
      if (!File.Exists(fileName))
      {
        return;
      }

      int importedChannels = 0;
      int importedGroups = 0;
      int importedCards = 0;
      int importedRecordedTv = 0;
      int importedScheduledRecordings = 0;

      //Current Version change to reflect the above exporter in order for compatibility
      int currentVersion = 1; // <--- Make sure that is the same number as in Export_to_XML

      using (Settings importFile = new Settings(fileName))
      {
        //Check version if not the right version prompt/do stuff/accomodate/change
        int versionCheck = importFile.GetValueAsInt("MP channel export list", "version", -1);
        if (versionCheck == -1)
        {
          //Not a valid channel list
          MessageBox.Show("This is not a valid channel list.", "Import Error", MessageBoxButtons.OK,
                          MessageBoxIcon.Error);
          return;
        }
        else if (versionCheck >= 0 && versionCheck < currentVersion)
        {
          //Older file
          MessageBox.Show("This is an older channel list. Trying to import.", "Possible Import Problem",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        else if (versionCheck == currentVersion)
        {
          //Current file, this is good stuff
        }
        else if (versionCheck > currentVersion)
        {
          //Newer? This person lives in a cave
          MessageBox.Show(
            "Detected channel list created by a later MediaPortal version.\nTrying to import. Consider upgrading to a later MediaPortal version.",
            "Possible Import Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        TVDatabase.ClearAll();
        LoadSettings();

        importedChannels = ImportChannels(importFile);
        importedGroups = ImportGroups(importFile);
        importedCards = ImportCards(importFile);
        importedRecordedTv = ImportRecordedTv(importFile);
        importedScheduledRecordings = ImportScheduledRecordings(importFile);
      }

      if ((importedChannels > 0) ||
          (importedGroups > 0) ||
          (importedCards > 0) ||
          (importedRecordedTv > 0) ||
          (importedScheduledRecordings > 0))
      {
        string messageText = "Imported successfully:                  \n";
        if (importedChannels > 0)
        {
          messageText += string.Format("\n{0} TV channel", importedChannels);
          if (importedChannels > 1)
          {
            messageText += "s";
          }
        }
        if (importedGroups > 0)
        {
          messageText += string.Format("\n{0} TV channel group", importedGroups);
          if (importedGroups > 1)
          {
            messageText += "s";
          }
        }
        if (importedCards > 0)
        {
          messageText += string.Format("\n{0} assigned capture card", importedCards);
          if (importedCards > 1)
          {
            messageText += "s";
          }
        }
        if (importedRecordedTv > 0)
        {
          messageText += string.Format("\n{0} completed recording", importedRecordedTv);
          if (importedRecordedTv > 1)
          {
            messageText += "s";
          }
        }
        if (importedScheduledRecordings > 0)
        {
          messageText += string.Format("\n{0} scheduled recording", importedScheduledRecordings);
          if (importedScheduledRecordings > 1)
          {
            messageText += "s";
          }
        }

        _reloadList = true;
        SaveSettings();

        MessageBox.Show(messageText, "Import Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else
      {
        MessageBox.Show("No items could be imported.", "Import Results", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }

    private AnalogVideoStandard ConvertAvs(object avs)
    {
      if ((string) avs == "None")
      {
        return AnalogVideoStandard.None;
      }
      if ((string) avs == "NTSC_M")
      {
        return AnalogVideoStandard.NTSC_M;
      }
      if ((string) avs == "NTSC_M_J")
      {
        return AnalogVideoStandard.NTSC_M_J;
      }
      if ((string) avs == "NTSC_433")
      {
        return AnalogVideoStandard.NTSC_433;
      }
      if ((string) avs == "PAL_B")
      {
        return AnalogVideoStandard.PAL_B;
      }
      if ((string) avs == "PAL_D")
      {
        return AnalogVideoStandard.PAL_D;
      }
      if ((string) avs == "PAL_G")
      {
        return AnalogVideoStandard.PAL_G;
      }
      if ((string) avs == "PAL_H")
      {
        return AnalogVideoStandard.PAL_H;
      }
      if ((string) avs == "PAL_I")
      {
        return AnalogVideoStandard.PAL_I;
      }
      if ((string) avs == "PAL_M")
      {
        return AnalogVideoStandard.PAL_M;
      }
      if ((string) avs == "PAL_N")
      {
        return AnalogVideoStandard.PAL_N;
      }
      if ((string) avs == "PAL_60")
      {
        return AnalogVideoStandard.PAL_60;
      }
      if ((string) avs == "SECAM_B")
      {
        return AnalogVideoStandard.SECAM_B;
      }
      if ((string) avs == "SECAM_D")
      {
        return AnalogVideoStandard.SECAM_D;
      }
      if ((string) avs == "SECAM_G")
      {
        return AnalogVideoStandard.SECAM_G;
      }
      if ((string) avs == "SECAM_H")
      {
        return AnalogVideoStandard.SECAM_H;
      }
      if ((string) avs == "SECAM_K")
      {
        return AnalogVideoStandard.SECAM_K;
      }
      if ((string) avs == "SECAM_K1")
      {
        return AnalogVideoStandard.SECAM_K1;
      }
      if ((string) avs == "SECAM_L")
      {
        return AnalogVideoStandard.SECAM_L;
      }
      if ((string) avs == "SECAM_L1")
      {
        return AnalogVideoStandard.SECAM_L1;
      }
      if ((string) avs == "PAL_N_COMBO")
      {
        return AnalogVideoStandard.PAL_N_COMBO;
      }

      //If nothing return Default
      return AnalogVideoStandard.None;
    }

    private TV.Database.TVRecording.QualityType ConvertQualityType(object quality)
    {
      if ((string) quality == "NotSet")
      {
        return TV.Database.TVRecording.QualityType.NotSet;
      }
      if ((string) quality == "Portable")
      {
        return TV.Database.TVRecording.QualityType.Portable;
      }
      if ((string) quality == "Low")
      {
        return TV.Database.TVRecording.QualityType.Low;
      }
      if ((string) quality == "Medium")
      {
        return TV.Database.TVRecording.QualityType.Medium;
      }
      if ((string) quality == "High")
      {
        return TV.Database.TVRecording.QualityType.High;
      }

      //If nothing return Default
      return TV.Database.TVRecording.QualityType.NotSet;
    }

    private TV.Database.TVRecording.RecordingType ConvertRecordingType(object recType)
    {
      if ((string) recType == "Once")
      {
        return TV.Database.TVRecording.RecordingType.Once;
      }
      if ((string) recType == "EveryTimeOnThisChannel")
      {
        return TV.Database.TVRecording.RecordingType.EveryTimeOnThisChannel;
      }
      if ((string) recType == "EveryTimeOnEveryChannel")
      {
        return TV.Database.TVRecording.RecordingType.EveryTimeOnEveryChannel;
      }
      if ((string) recType == "Daily")
      {
        return TV.Database.TVRecording.RecordingType.Daily;
      }
      if ((string) recType == "Weekly")
      {
        return TV.Database.TVRecording.RecordingType.Weekly;
      }
      if ((string) recType == "WeekDays")
      {
        return TV.Database.TVRecording.RecordingType.WeekDays;
      }
      if ((string) recType == "WeekEnds")
      {
        return TV.Database.TVRecording.RecordingType.WeekEnds;
      }

      //If nothing return Default
      return TV.Database.TVRecording.RecordingType.Once;
    }

    private void buttonBackup_Click(object sender, EventArgs e)
    {
      xmlSaveDialog.RestoreDirectory = true;
      if (xmlSaveDialog.ShowDialog(this) == DialogResult.OK)
      {
        ExportToXml(xmlSaveDialog.FileName.ToString());
      }
    }

    private void buttonRestore_Click(object sender, EventArgs e)
    {
      xmlOpenDialog.RestoreDirectory = true;
      if (xmlOpenDialog.ShowDialog(this) == DialogResult.OK)
      {
        ImportFromXml(xmlOpenDialog.FileName.ToString());
      }
    }

    private void buttonLookup_Click(object sender, EventArgs e)
    {
      TvChannelLookupService dlg = new TvChannelLookupService();
      dlg.ShowDialog(this);
      _reloadList = true;
      RadioStations.UpdateList();
    }

    private void buttonCombine_Click(object sender, EventArgs e)
    {
      if (listViewTvChannels.SelectedItems.Count != 2)
      {
        return;
      }

      TelevisionChannel ch1 = listViewTvChannels.SelectedItems[0].Tag as TelevisionChannel;
      TelevisionChannel ch2 = listViewTvChannels.SelectedItems[1].Tag as TelevisionChannel;

      if (ch1.Channel > 0 && ch1.Channel < 255)
      {
        ch2.Channel = ch1.Channel;
        ch2.Frequency = ch1.Frequency;
      }

      if (ch1.ExternalTunerChannel != string.Empty)
      {
        ch2.ExternalTunerChannel = ch1.ExternalTunerChannel;
        ch2.External = ch1.External;
      }
      //remap all atsc,dvb mappings from ch1->ch2...
      if (TVDatabase.IsDigitalChannel(TVDatabase.GetChannelById(ch1.ID)))
      {
        TVDatabase.ReMapDigitalMapping(ch1.ID, ch2.ID);
      }

      listViewTvChannels.BeginUpdate();

      ListViewItem listItem = listViewTvChannels.SelectedItems[1];
      listItem.SubItems[0].Text = ch2.Name;
      listItem.SubItems[1].Text = ch2.External
                                    ? String.Format("{0}/{1}", ch2.Channel, ch2.ExternalTunerChannel)
                                    : ch2.Channel.ToString();
      listItem.SubItems[2].Text = GetStandardName(ch2.standard);
      listItem.SubItems[3].Text = ch2.External ? "External" : "Internal";
      listItem.ImageIndex = 0;

      if (ch2.Scrambled)
      {
        listItem.ImageIndex = 1;
      }

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

    private void buttonDeleteScrambled_Click(object sender, EventArgs e)
    {
      listViewTvChannels.BeginUpdate();
      _itemsModified = true;

      int itemCount = listViewTvChannels.Items.Count;
      int deletedChans = 0;
      int loopedNTimes = 0;
      bool containsScrambledChannels = true;

      Log.Info("Scanning {0} channels for scrambled status", Convert.ToString(itemCount));

      while (containsScrambledChannels)
      {
        loopedNTimes += 1;
        for (int index = 0; index < itemCount; index++)
        {
          if (((TelevisionChannel) listViewTvChannels.Items[index].Tag).Scrambled) // channel is scrambled
          {
            Log.Info("Deleting scrambled channel: {0}", ((TelevisionChannel) listViewTvChannels.Items[index].Tag).Name);
            listViewTvChannels.Items.RemoveAt(index);
            itemCount -= 1;
            deletedChans += 1;
          }
        }
        containsScrambledChannels = false;
        Log.Info("Looped list {0} time(s) to delete all scrambled channels", Convert.ToString(loopedNTimes));
        for (int index = 0; index < listViewTvChannels.Items.Count; index++)
        {
          if (((TelevisionChannel) listViewTvChannels.Items[index].Tag).Scrambled)
          {
            containsScrambledChannels = true;
          }
        }
      }

      SaveSettings();
      Log.Info("Deleted {0} scrambled channels", Convert.ToString(deletedChans));
      listViewTvChannels.EndUpdate();
    }

    private void listViewTvChannels_DragDrop(object sender, DragEventArgs e)
    {
      SaveSettings();
    }
  }
}