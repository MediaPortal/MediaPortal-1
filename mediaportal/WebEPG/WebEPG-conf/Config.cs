#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.Services;
using MediaPortal.TV.Database;
using MediaPortal.Util;
using MediaPortal.WebEPG.Config.Grabber;
using MediaPortal.WebEPG.config;
using MediaPortal.EPG.config;

namespace WebEPG_conf
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class fChannels : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox gbChannels;
    private MediaPortal.UserInterface.Controls.MPLabel Grabber;
    private MediaPortal.UserInterface.Controls.MPLabel l_cID;
    private MediaPortal.UserInterface.Controls.MPButton bAdd;
    private MediaPortal.UserInterface.Controls.MPButton bImport;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbChannelDetails;
    private MediaPortal.UserInterface.Controls.MPTextBox tbGrabSite;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbGrabber;
    private MediaPortal.UserInterface.Controls.MPLabel lGrabDay;
    private MediaPortal.UserInterface.Controls.MPButton bSave;
    private MediaPortal.UserInterface.Controls.MPTextBox tbCount;
    private MediaPortal.UserInterface.Controls.MPLabel lCount;
    private MediaPortal.UserInterface.Controls.MPButton bRemove;
    private MediaPortal.UserInterface.Controls.MPButton bChannelID;
    private MediaPortal.UserInterface.Controls.MPButton bGrabber;
    private MediaPortal.UserInterface.Controls.MPTextBox tbChannelName;
    private System.Windows.Forms.NumericUpDown nMaxGrab;
    private MediaPortal.UserInterface.Controls.MPLabel lGuideDays;
    private MediaPortal.UserInterface.Controls.MPTextBox tbGrabDays;
    private ListView lvMapping;
    private GroupBox gbGlobal;
    private GroupBox gbImport;
    private Label label1;
    private ComboBox cbSource;
    private GroupBox gbMapping;
    private Label label3;
    private ComboBox cbCountry;
    private Button bAutoMap;
    private Button bClearMapping;
    private Label label2;
    private MaskedTextBox mtbNewChannel;

    private string startDirectory;
    private WebepgConfigFile _configFile;
    private Dictionary<string, ChannelMap> _channelMapping;
    private fSelection selection;
    private MergedChannelDetails _mergeConfig;
    private TreeNode tChannels;
    private TreeNode tGrabbers;
    private SortedList CountryList;
    private SortedList ChannelList;
    private Hashtable hChannelConfigInfo;
    private Hashtable hGrabberConfigInfo;
    private ILog _log;
    private ChannelsList _channelInfo;
    private Dictionary<string, string> _countryList;
    private TabControl tcMappingDetails;
    private TabPage tpSingle;
    private TabPage tpMultiple;
    private ListView lvMerged;
    private Button bMergedRemove;
    private Button bMergedAdd;
    private Button bMergedEdit;
    private ListViewColumnSorter lvwColumnSorter;

    public fChannels()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      lvMapping.Columns.Add("EPG Name", 100, HorizontalAlignment.Left);
      lvMapping.Columns.Add("Channel Name", 100, HorizontalAlignment.Left);
      lvMapping.Columns.Add("Channel ID", 80, HorizontalAlignment.Left);
      lvMapping.Columns.Add("Grabber", 120, HorizontalAlignment.Left);

      lvwColumnSorter = new ListViewColumnSorter();
      lvMapping.ListViewItemSorter = lvwColumnSorter;

      lvMerged.Columns.Add("Channel", 100, HorizontalAlignment.Left);
      lvMerged.Columns.Add("Grabber", 105, HorizontalAlignment.Left);
      lvMerged.Columns.Add("Start", 50, HorizontalAlignment.Left);
      lvMerged.Columns.Add("End", 50, HorizontalAlignment.Left);

      cbSource.SelectedIndex = 0;

      _log = GlobalServiceProvider.Get<ILog>();

      startDirectory = Environment.CurrentDirectory;

      startDirectory += "\\WebEPG";

      LoadCountries();
      LoadConfig();
      LoadWebepgConfigFile();

      RedrawList(null);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        //				if(components != null)
        //				{
        //					components.Dispose();
        //				}
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.nMaxGrab = new System.Windows.Forms.NumericUpDown();
      this.gbGlobal = new System.Windows.Forms.GroupBox();
      this.lGrabDay = new MediaPortal.UserInterface.Controls.MPLabel();
      this.gbImport = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbSource = new System.Windows.Forms.ComboBox();
      this.bImport = new MediaPortal.UserInterface.Controls.MPButton();
      this.gbMapping = new System.Windows.Forms.GroupBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cbCountry = new System.Windows.Forms.ComboBox();
      this.bAutoMap = new System.Windows.Forms.Button();
      this.gbChannelDetails = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tcMappingDetails = new System.Windows.Forms.TabControl();
      this.tpSingle = new System.Windows.Forms.TabPage();
      this.gbGrabber = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tbGrabDays = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lGuideDays = new MediaPortal.UserInterface.Controls.MPLabel();
      this.bGrabber = new MediaPortal.UserInterface.Controls.MPButton();
      this.Grabber = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbGrabSite = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bChannelID = new MediaPortal.UserInterface.Controls.MPButton();
      this.l_cID = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbChannelName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tpMultiple = new System.Windows.Forms.TabPage();
      this.bMergedEdit = new System.Windows.Forms.Button();
      this.bMergedRemove = new System.Windows.Forms.Button();
      this.bMergedAdd = new System.Windows.Forms.Button();
      this.lvMerged = new System.Windows.Forms.ListView();
      this.gbChannels = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.mtbNewChannel = new System.Windows.Forms.MaskedTextBox();
      this.lvMapping = new System.Windows.Forms.ListView();
      this.bClearMapping = new System.Windows.Forms.Button();
      this.lCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbCount = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.bAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.bSave = new MediaPortal.UserInterface.Controls.MPButton();
      ((System.ComponentModel.ISupportInitialize)(this.nMaxGrab)).BeginInit();
      this.gbGlobal.SuspendLayout();
      this.gbImport.SuspendLayout();
      this.gbMapping.SuspendLayout();
      this.gbChannelDetails.SuspendLayout();
      this.tcMappingDetails.SuspendLayout();
      this.tpSingle.SuspendLayout();
      this.gbGrabber.SuspendLayout();
      this.tpMultiple.SuspendLayout();
      this.gbChannels.SuspendLayout();
      this.SuspendLayout();
      // 
      // nMaxGrab
      // 
      this.nMaxGrab.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.nMaxGrab.Location = new System.Drawing.Point(86, 22);
      this.nMaxGrab.Maximum = new decimal(new int[] {
            14,
            0,
            0,
            0});
      this.nMaxGrab.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nMaxGrab.Name = "nMaxGrab";
      this.nMaxGrab.Size = new System.Drawing.Size(62, 20);
      this.nMaxGrab.TabIndex = 13;
      this.nMaxGrab.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
      // 
      // gbGlobal
      // 
      this.gbGlobal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.gbGlobal.Controls.Add(this.nMaxGrab);
      this.gbGlobal.Controls.Add(this.lGrabDay);
      this.gbGlobal.Location = new System.Drawing.Point(404, 179);
      this.gbGlobal.Name = "gbGlobal";
      this.gbGlobal.Size = new System.Drawing.Size(330, 59);
      this.gbGlobal.TabIndex = 15;
      this.gbGlobal.TabStop = false;
      this.gbGlobal.Text = "Global Settings";
      // 
      // lGrabDay
      // 
      this.lGrabDay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.lGrabDay.Location = new System.Drawing.Point(9, 26);
      this.lGrabDay.Name = "lGrabDay";
      this.lGrabDay.Size = new System.Drawing.Size(72, 16);
      this.lGrabDay.TabIndex = 9;
      this.lGrabDay.Text = "Grab Days";
      // 
      // gbImport
      // 
      this.gbImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.gbImport.Controls.Add(this.label1);
      this.gbImport.Controls.Add(this.cbSource);
      this.gbImport.Controls.Add(this.bImport);
      this.gbImport.Location = new System.Drawing.Point(404, 308);
      this.gbImport.Name = "gbImport";
      this.gbImport.Size = new System.Drawing.Size(330, 54);
      this.gbImport.TabIndex = 17;
      this.gbImport.TabStop = false;
      this.gbImport.Text = "Import Channel Data";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 25);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(44, 13);
      this.label1.TabIndex = 13;
      this.label1.Text = "Source:";
      // 
      // cbSource
      // 
      this.cbSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbSource.FormattingEnabled = true;
      this.cbSource.Items.AddRange(new object[] {
            "MediaPortal",
            "TV Server"});
      this.cbSource.Location = new System.Drawing.Point(86, 19);
      this.cbSource.Name = "cbSource";
      this.cbSource.Size = new System.Drawing.Size(151, 21);
      this.cbSource.TabIndex = 12;
      this.cbSource.SelectedIndexChanged += new System.EventHandler(this.cbSource_SelectedIndexChanged);
      // 
      // bImport
      // 
      this.bImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.bImport.Location = new System.Drawing.Point(244, 19);
      this.bImport.Name = "bImport";
      this.bImport.Size = new System.Drawing.Size(72, 24);
      this.bImport.TabIndex = 11;
      this.bImport.Text = "Import";
      this.bImport.UseVisualStyleBackColor = true;
      this.bImport.Click += new System.EventHandler(this.bImport_Click);
      // 
      // gbMapping
      // 
      this.gbMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.gbMapping.Controls.Add(this.label3);
      this.gbMapping.Controls.Add(this.cbCountry);
      this.gbMapping.Controls.Add(this.bAutoMap);
      this.gbMapping.Location = new System.Drawing.Point(404, 244);
      this.gbMapping.Name = "gbMapping";
      this.gbMapping.Size = new System.Drawing.Size(330, 58);
      this.gbMapping.TabIndex = 19;
      this.gbMapping.TabStop = false;
      this.gbMapping.Text = "Auto Mapping";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(9, 26);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(43, 13);
      this.label3.TabIndex = 22;
      this.label3.Text = "Country";
      // 
      // cbCountry
      // 
      this.cbCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCountry.FormattingEnabled = true;
      this.cbCountry.Location = new System.Drawing.Point(86, 23);
      this.cbCountry.Name = "cbCountry";
      this.cbCountry.Size = new System.Drawing.Size(151, 21);
      this.cbCountry.Sorted = true;
      this.cbCountry.TabIndex = 21;
      // 
      // bAutoMap
      // 
      this.bAutoMap.Location = new System.Drawing.Point(244, 21);
      this.bAutoMap.Name = "bAutoMap";
      this.bAutoMap.Size = new System.Drawing.Size(72, 23);
      this.bAutoMap.TabIndex = 19;
      this.bAutoMap.Text = "Auto Map";
      this.bAutoMap.UseVisualStyleBackColor = true;
      this.bAutoMap.Click += new System.EventHandler(this.bAutoMap_Click);
      // 
      // gbChannelDetails
      // 
      this.gbChannelDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.gbChannelDetails.Controls.Add(this.tcMappingDetails);
      this.gbChannelDetails.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbChannelDetails.Location = new System.Drawing.Point(404, 8);
      this.gbChannelDetails.Name = "gbChannelDetails";
      this.gbChannelDetails.Size = new System.Drawing.Size(330, 163);
      this.gbChannelDetails.TabIndex = 14;
      this.gbChannelDetails.TabStop = false;
      this.gbChannelDetails.Text = "Mapping Details";
      // 
      // tcMappingDetails
      // 
      this.tcMappingDetails.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
      this.tcMappingDetails.Controls.Add(this.tpSingle);
      this.tcMappingDetails.Controls.Add(this.tpMultiple);
      this.tcMappingDetails.Location = new System.Drawing.Point(6, 17);
      this.tcMappingDetails.Name = "tcMappingDetails";
      this.tcMappingDetails.SelectedIndex = 0;
      this.tcMappingDetails.Size = new System.Drawing.Size(318, 140);
      this.tcMappingDetails.TabIndex = 16;
      this.tcMappingDetails.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tcMappingDetails_Selecting);
      // 
      // tpSingle
      // 
      this.tpSingle.Controls.Add(this.gbGrabber);
      this.tpSingle.Controls.Add(this.bChannelID);
      this.tpSingle.Controls.Add(this.l_cID);
      this.tpSingle.Controls.Add(this.tbChannelName);
      this.tpSingle.Location = new System.Drawing.Point(4, 25);
      this.tpSingle.Name = "tpSingle";
      this.tpSingle.Padding = new System.Windows.Forms.Padding(3);
      this.tpSingle.Size = new System.Drawing.Size(310, 111);
      this.tpSingle.TabIndex = 0;
      this.tpSingle.Text = "Single";
      this.tpSingle.UseVisualStyleBackColor = true;
      // 
      // gbGrabber
      // 
      this.gbGrabber.Controls.Add(this.tbGrabDays);
      this.gbGrabber.Controls.Add(this.lGuideDays);
      this.gbGrabber.Controls.Add(this.bGrabber);
      this.gbGrabber.Controls.Add(this.Grabber);
      this.gbGrabber.Controls.Add(this.tbGrabSite);
      this.gbGrabber.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbGrabber.Location = new System.Drawing.Point(6, 43);
      this.gbGrabber.Name = "gbGrabber";
      this.gbGrabber.Size = new System.Drawing.Size(298, 65);
      this.gbGrabber.TabIndex = 15;
      this.gbGrabber.TabStop = false;
      this.gbGrabber.Text = "Grabber Details";
      // 
      // tbGrabDays
      // 
      this.tbGrabDays.BorderColor = System.Drawing.Color.Empty;
      this.tbGrabDays.Location = new System.Drawing.Point(68, 39);
      this.tbGrabDays.Name = "tbGrabDays";
      this.tbGrabDays.ReadOnly = true;
      this.tbGrabDays.Size = new System.Drawing.Size(116, 20);
      this.tbGrabDays.TabIndex = 7;
      // 
      // lGuideDays
      // 
      this.lGuideDays.Location = new System.Drawing.Point(3, 42);
      this.lGuideDays.Name = "lGuideDays";
      this.lGuideDays.Size = new System.Drawing.Size(71, 17);
      this.lGuideDays.TabIndex = 8;
      this.lGuideDays.Text = "Guide Days";
      // 
      // bGrabber
      // 
      this.bGrabber.Location = new System.Drawing.Point(262, 12);
      this.bGrabber.Name = "bGrabber";
      this.bGrabber.Size = new System.Drawing.Size(22, 20);
      this.bGrabber.TabIndex = 6;
      this.bGrabber.Text = "...";
      this.bGrabber.UseVisualStyleBackColor = true;
      this.bGrabber.Click += new System.EventHandler(this.bGrabber_Click);
      // 
      // Grabber
      // 
      this.Grabber.Location = new System.Drawing.Point(3, 16);
      this.Grabber.Name = "Grabber";
      this.Grabber.Size = new System.Drawing.Size(56, 23);
      this.Grabber.TabIndex = 1;
      this.Grabber.Text = "Site";
      // 
      // tbGrabSite
      // 
      this.tbGrabSite.BorderColor = System.Drawing.Color.Empty;
      this.tbGrabSite.Location = new System.Drawing.Point(68, 13);
      this.tbGrabSite.Name = "tbGrabSite";
      this.tbGrabSite.ReadOnly = true;
      this.tbGrabSite.Size = new System.Drawing.Size(188, 20);
      this.tbGrabSite.TabIndex = 0;
      // 
      // bChannelID
      // 
      this.bChannelID.Location = new System.Drawing.Point(268, 17);
      this.bChannelID.Name = "bChannelID";
      this.bChannelID.Size = new System.Drawing.Size(22, 20);
      this.bChannelID.TabIndex = 9;
      this.bChannelID.Text = "...";
      this.bChannelID.UseVisualStyleBackColor = true;
      this.bChannelID.Click += new System.EventHandler(this.bChannelID_Click);
      // 
      // l_cID
      // 
      this.l_cID.Location = new System.Drawing.Point(12, 20);
      this.l_cID.Name = "l_cID";
      this.l_cID.Size = new System.Drawing.Size(56, 20);
      this.l_cID.TabIndex = 8;
      this.l_cID.Text = "Channel";
      // 
      // tbChannelName
      // 
      this.tbChannelName.BorderColor = System.Drawing.Color.Empty;
      this.tbChannelName.Location = new System.Drawing.Point(73, 17);
      this.tbChannelName.Name = "tbChannelName";
      this.tbChannelName.ReadOnly = true;
      this.tbChannelName.Size = new System.Drawing.Size(189, 20);
      this.tbChannelName.TabIndex = 7;
      // 
      // tpMultiple
      // 
      this.tpMultiple.Controls.Add(this.bMergedEdit);
      this.tpMultiple.Controls.Add(this.bMergedRemove);
      this.tpMultiple.Controls.Add(this.bMergedAdd);
      this.tpMultiple.Controls.Add(this.lvMerged);
      this.tpMultiple.Location = new System.Drawing.Point(4, 25);
      this.tpMultiple.Name = "tpMultiple";
      this.tpMultiple.Padding = new System.Windows.Forms.Padding(3);
      this.tpMultiple.Size = new System.Drawing.Size(310, 111);
      this.tpMultiple.TabIndex = 1;
      this.tpMultiple.Text = "Multiple (Merged)";
      this.tpMultiple.UseVisualStyleBackColor = true;
      // 
      // bMergedEdit
      // 
      this.bMergedEdit.Location = new System.Drawing.Point(116, 80);
      this.bMergedEdit.Name = "bMergedEdit";
      this.bMergedEdit.Size = new System.Drawing.Size(75, 23);
      this.bMergedEdit.TabIndex = 3;
      this.bMergedEdit.Text = "Edit";
      this.bMergedEdit.UseVisualStyleBackColor = true;
      this.bMergedEdit.Click += new System.EventHandler(this.bMergedEdit_Click);
      // 
      // bMergedRemove
      // 
      this.bMergedRemove.Location = new System.Drawing.Point(228, 81);
      this.bMergedRemove.Name = "bMergedRemove";
      this.bMergedRemove.Size = new System.Drawing.Size(75, 23);
      this.bMergedRemove.TabIndex = 2;
      this.bMergedRemove.Text = "Remove";
      this.bMergedRemove.UseVisualStyleBackColor = true;
      this.bMergedRemove.Click += new System.EventHandler(this.bMergedRemove_Click);
      // 
      // bMergedAdd
      // 
      this.bMergedAdd.Location = new System.Drawing.Point(7, 80);
      this.bMergedAdd.Name = "bMergedAdd";
      this.bMergedAdd.Size = new System.Drawing.Size(75, 23);
      this.bMergedAdd.TabIndex = 1;
      this.bMergedAdd.Text = "Add";
      this.bMergedAdd.UseVisualStyleBackColor = true;
      this.bMergedAdd.Click += new System.EventHandler(this.bMergedAdd_Click);
      // 
      // lvMerged
      // 
      this.lvMerged.FullRowSelect = true;
      this.lvMerged.HideSelection = false;
      this.lvMerged.Location = new System.Drawing.Point(0, 5);
      this.lvMerged.MultiSelect = false;
      this.lvMerged.Name = "lvMerged";
      this.lvMerged.Size = new System.Drawing.Size(310, 70);
      this.lvMerged.TabIndex = 0;
      this.lvMerged.UseCompatibleStateImageBehavior = false;
      this.lvMerged.View = System.Windows.Forms.View.Details;
      // 
      // gbChannels
      // 
      this.gbChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbChannels.Controls.Add(this.label2);
      this.gbChannels.Controls.Add(this.mtbNewChannel);
      this.gbChannels.Controls.Add(this.lvMapping);
      this.gbChannels.Controls.Add(this.bClearMapping);
      this.gbChannels.Controls.Add(this.lCount);
      this.gbChannels.Controls.Add(this.tbCount);
      this.gbChannels.Controls.Add(this.bRemove);
      this.gbChannels.Controls.Add(this.bAdd);
      this.gbChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbChannels.Location = new System.Drawing.Point(8, 8);
      this.gbChannels.Name = "gbChannels";
      this.gbChannels.Size = new System.Drawing.Size(390, 391);
      this.gbChannels.TabIndex = 13;
      this.gbChannels.TabStop = false;
      this.gbChannels.Text = "Channel Mapping";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 22);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(71, 13);
      this.label2.TabIndex = 19;
      this.label2.Text = "New Channel";
      // 
      // mtbNewChannel
      // 
      this.mtbNewChannel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mtbNewChannel.Location = new System.Drawing.Point(85, 19);
      this.mtbNewChannel.Name = "mtbNewChannel";
      this.mtbNewChannel.Size = new System.Drawing.Size(206, 20);
      this.mtbNewChannel.TabIndex = 18;
      // 
      // lvMapping
      // 
      this.lvMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lvMapping.FullRowSelect = true;
      this.lvMapping.HideSelection = false;
      this.lvMapping.Location = new System.Drawing.Point(6, 47);
      this.lvMapping.Name = "lvMapping";
      this.lvMapping.Size = new System.Drawing.Size(376, 304);
      this.lvMapping.TabIndex = 17;
      this.lvMapping.UseCompatibleStateImageBehavior = false;
      this.lvMapping.View = System.Windows.Forms.View.Details;
      this.lvMapping.SelectedIndexChanged += new System.EventHandler(this.lvMapping_SelectedIndexChanged);
      this.lvMapping.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvMapping_ColumnClick);
      // 
      // bClearMapping
      // 
      this.bClearMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bClearMapping.Location = new System.Drawing.Point(205, 358);
      this.bClearMapping.Name = "bClearMapping";
      this.bClearMapping.Size = new System.Drawing.Size(90, 23);
      this.bClearMapping.TabIndex = 20;
      this.bClearMapping.Text = "Clear Mapping";
      this.bClearMapping.UseVisualStyleBackColor = true;
      this.bClearMapping.Click += new System.EventHandler(this.bClearMapping_Click);
      // 
      // lCount
      // 
      this.lCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lCount.Location = new System.Drawing.Point(67, 363);
      this.lCount.Name = "lCount";
      this.lCount.Size = new System.Drawing.Size(80, 16);
      this.lCount.TabIndex = 1;
      this.lCount.Text = "Channel Count";
      // 
      // tbCount
      // 
      this.tbCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tbCount.BorderColor = System.Drawing.Color.Empty;
      this.tbCount.Location = new System.Drawing.Point(6, 360);
      this.tbCount.Name = "tbCount";
      this.tbCount.ReadOnly = true;
      this.tbCount.Size = new System.Drawing.Size(55, 20);
      this.tbCount.TabIndex = 0;
      // 
      // bRemove
      // 
      this.bRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bRemove.Location = new System.Drawing.Point(301, 357);
      this.bRemove.Name = "bRemove";
      this.bRemove.Size = new System.Drawing.Size(81, 24);
      this.bRemove.TabIndex = 17;
      this.bRemove.Text = "Remove";
      this.bRemove.UseVisualStyleBackColor = true;
      this.bRemove.Click += new System.EventHandler(this.bRemove_Click);
      // 
      // bAdd
      // 
      this.bAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bAdd.Location = new System.Drawing.Point(297, 17);
      this.bAdd.Name = "bAdd";
      this.bAdd.Size = new System.Drawing.Size(81, 24);
      this.bAdd.TabIndex = 12;
      this.bAdd.Text = "Add";
      this.bAdd.UseVisualStyleBackColor = true;
      this.bAdd.Click += new System.EventHandler(this.bAdd_Click);
      // 
      // bSave
      // 
      this.bSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bSave.Location = new System.Drawing.Point(648, 371);
      this.bSave.Name = "bSave";
      this.bSave.Size = new System.Drawing.Size(72, 24);
      this.bSave.TabIndex = 16;
      this.bSave.Text = "Save";
      this.bSave.UseVisualStyleBackColor = true;
      this.bSave.Click += new System.EventHandler(this.bSave_Click);
      // 
      // fChannels
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(746, 411);
      this.Controls.Add(this.gbChannelDetails);
      this.Controls.Add(this.gbMapping);
      this.Controls.Add(this.gbImport);
      this.Controls.Add(this.gbGlobal);
      this.Controls.Add(this.gbChannels);
      this.Controls.Add(this.bSave);
      this.MaximizeBox = false;
      this.Name = "fChannels";
      this.Text = "WebEPG Config";
      ((System.ComponentModel.ISupportInitialize)(this.nMaxGrab)).EndInit();
      this.gbGlobal.ResumeLayout(false);
      this.gbImport.ResumeLayout(false);
      this.gbImport.PerformLayout();
      this.gbMapping.ResumeLayout(false);
      this.gbMapping.PerformLayout();
      this.gbChannelDetails.ResumeLayout(false);
      this.tcMappingDetails.ResumeLayout(false);
      this.tpSingle.ResumeLayout(false);
      this.tpSingle.PerformLayout();
      this.gbGrabber.ResumeLayout(false);
      this.gbGrabber.PerformLayout();
      this.tpMultiple.ResumeLayout(false);
      this.gbChannels.ResumeLayout(false);
      this.gbChannels.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    #region Main
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.Run(new fChannels());
    }
    #endregion

    #region Private
    private void getTVChannels()
    {
      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Button: Import");
      try
      {
        ArrayList channels = new ArrayList();

        TVDatabase.GetChannels(ref channels);
        for (int i = 0; i < channels.Count; i++)
        {
          TVChannel chan = (TVChannel)channels[i];
          if (!_channelMapping.ContainsKey(chan.Name))
          {
            ChannelMap channel = new ChannelMap();
            channel.displayName = chan.Name;
            _channelMapping.Add(chan.Name, channel);

          }
          RedrawList(null);
        }
      }
      catch (Exception ex)
      {
        _log.WriteFile(LogType.WebEPG, Level.Error, "WebEPG Config: Import failed - {0}", ex.Message);
      }
    }

    private void RedrawList(string selectName)
    {
      int selectedIndex = 0;
      if (lvMapping.SelectedIndices.Count > 0)
        selectedIndex = lvMapping.SelectedIndices[0];

      lvMapping.Items.Clear();

      //add all channels
      foreach (ChannelMap channel in _channelMapping.Values)
      {
        ListViewItem channelItem = new ListViewItem(channel.displayName);
        string name = string.Empty;
        if (channel.id != null)
        {
          ChannelConfigInfo info = (ChannelConfigInfo)hChannelConfigInfo[channel.id];
          if (info != null)
            name = info.FullName;
        }
        else
        {
          if (channel.merged != null)
            name = "[Merged]";
        }
        channelItem.SubItems.Add(name);
        channelItem.SubItems.Add(channel.id);
        channelItem.SubItems.Add(channel.grabber);
        lvMapping.Items.Add(channelItem);
      }

      if (lvMapping.Items.Count > 0)
      {
        if (lvMapping.Items.Count > selectedIndex)
          lvMapping.Items[selectedIndex].Selected = true;
        else
          lvMapping.Items[lvMapping.Items.Count - 1].Selected = true;
      }

      tbCount.Text = lvMapping.Items.Count.ToString();
      lvMapping.Select();
    }

    private void UpdateList()
    {
      //update existing channels
      foreach (ListViewItem channel in lvMapping.Items)
      {
        if (_channelMapping.ContainsKey(channel.Text))
        {
          ChannelMap channelDetails = _channelMapping[channel.Text];
          string name = string.Empty;
          if (channelDetails.id != null)
          {
            ChannelConfigInfo info = (ChannelConfigInfo)hChannelConfigInfo[channelDetails.id];
            if (info != null)
              name = info.FullName;
          }
          else
          {
            if (channelDetails.merged != null)
              name = "[Merged]";
          }
          channel.SubItems[1].Text = name;
          channel.SubItems[2].Text = channelDetails.id;
          channel.SubItems[3].Text = channelDetails.grabber;
        }
        else
        {
          int selectedIndex = 0;
          if (lvMapping.SelectedIndices.Count > 0)
            selectedIndex = lvMapping.SelectedIndices[0];

          lvMapping.Items.Remove(channel);

          if (lvMapping.Items.Count > 0)
          {
            if (lvMapping.Items.Count > selectedIndex)
            {
              lvMapping.Items[selectedIndex].Selected = true;
            }
            else
            {
              lvMapping.Items[lvMapping.Items.Count - 1].Selected = true;
            }
          }
        }
      }
      lvMapping.Select();
    }

    private void UpdateMergedList(ChannelMap channelMap)
    {
      lvMerged.Items.Clear();

      if (channelMap.merged != null)
      {
        //add all channels
        foreach (MergedChannel channel in channelMap.merged)
        {
          ListViewItem channelItem = new ListViewItem(channel.id);
          channelItem.Tag = channel;
          channelItem.SubItems.Add(channel.grabber);
          channelItem.SubItems.Add(channel.start);
          channelItem.SubItems.Add(channel.end);
          lvMerged.Items.Add(channelItem);
        }
      }
    }

    private void LoadCountries()
    {
      _channelInfo = new ChannelsList(startDirectory);
      string[] countries = _channelInfo.GetCountries();
      _countryList = new Dictionary<string, string>();

      foreach (string country in countries)
      {
        try
        {
          RegionInfo region = new RegionInfo(country);
          cbCountry.Items.Add(region.EnglishName);
          _countryList.Add(region.EnglishName, country);
        }
        catch (Exception)
        {
        }
      }

      for (int i = 0; i < cbCountry.Items.Count; i++)
      {
        if (cbCountry.Items[i].ToString() == RegionInfo.CurrentRegion.EnglishName)
          cbCountry.SelectedIndex = i;
      }
    }

    private void LoadConfig()
    {
      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Loading Channels");
      hChannelConfigInfo = new Hashtable();

      if (System.IO.File.Exists(startDirectory + "\\channels\\channels.xml"))
      {
        _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Loading Existing channels.xml");
        MediaPortal.Webepg.Profile.Xml xmlreader = new MediaPortal.Webepg.Profile.Xml(startDirectory + "\\channels\\channels.xml");
        int channelCount = xmlreader.GetValueAsInt("ChannelInfo", "TotalChannels", 0);

        for (int i = 0; i < channelCount; i++)
        {
          ChannelConfigInfo channel = new ChannelConfigInfo();
          channel.ChannelID = xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
          channel.FullName = xmlreader.GetValueAsString(i.ToString(), "FullName", "");
          hChannelConfigInfo.Add(channel.ChannelID, channel);
        }
      }

      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Loading Grabbers");
      hGrabberConfigInfo = new Hashtable();
      CountryList = new SortedList();
      tGrabbers = new TreeNode("Web Sites");
      if (System.IO.Directory.Exists(startDirectory + "\\Grabbers"))
        GetTreeGrabbers(ref tGrabbers, startDirectory + "\\Grabbers");
      else
        _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Cannot find grabbers directory");


      IDictionaryEnumerator Enumerator = hChannelConfigInfo.GetEnumerator();
      while (Enumerator.MoveNext())
      {
        ChannelConfigInfo info = (ChannelConfigInfo)Enumerator.Value;
        if (info.ChannelID != null && info.FullName != null)
        {
          if (info.GrabberList != null)
          {
            IDictionaryEnumerator grabEnum = info.GrabberList.GetEnumerator();
            while (grabEnum.MoveNext())
            {
              GrabberConfigInfo gInfo = (GrabberConfigInfo)grabEnum.Value;
              SortedList chList = (SortedList)CountryList[gInfo.Country];
              if (chList[info.ChannelID] == null)
              {
                chList.Add(info.ChannelID, gInfo.GrabberID);
                //CountryList.Remove(gInfo.Country);
                //CountryList.Add(gInfo.Country, chList);
              }
            }
          }
        }
      }

      tChannels = new TreeNode("Channels");
      IDictionaryEnumerator countryEnum = CountryList.GetEnumerator();
      while (countryEnum.MoveNext())
      {
        SortedList chList = (SortedList)countryEnum.Value;
        TreeNode cNode = new TreeNode();
        cNode.Text = (string)countryEnum.Key;

        IDictionaryEnumerator chEnum = chList.GetEnumerator();
        while (chEnum.MoveNext())
        {
          TreeNode chNode = new TreeNode();

          ChannelConfigInfo info = (ChannelConfigInfo)hChannelConfigInfo[chEnum.Key];
          chNode.Text = info.FullName;
          string[] tag = new string[2];
          tag[0] = info.ChannelID;
          tag[1] = (string)chEnum.Value;
          chNode.Tag = tag;

          cNode.Nodes.Add(chNode);
        }

        tChannels.Nodes.Add(cNode);
      }

      ChannelList = new SortedList();
    }

    private void LoadWebepgConfigFile()
    {
      if (System.IO.File.Exists(startDirectory + "\\WebEPG.xml"))
      {
        _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Loading Existing WebEPG.xml");

        try
        {
          XmlSerializer s = new XmlSerializer(typeof(WebepgConfigFile));
          TextReader r = new StreamReader(startDirectory + "\\WebEPG.xml");
          _configFile = (WebepgConfigFile)s.Deserialize(r);
          r.Close();
        }
        catch (InvalidOperationException ex)
        {
          _log.Error(LogType.WebEPG, "WebEPG: Error loading config {0}: {1}", startDirectory + "\\WebEPG.xml", ex.Message);
          LoadOldConfigFile();
        }
      }

      // no file found set defaults
      if (_configFile == null)
      {
        _configFile = new WebepgConfigFile();
        _configFile.Channels = new List<ChannelMap>();
        _configFile.Info = new WebepgInfo();
        _configFile.Info.GrabDays = 2;
      }

      _channelMapping = new Dictionary<string, ChannelMap>();

      foreach (ChannelMap channel in _configFile.Channels)
      {
        _channelMapping.Add(channel.displayName, channel);
      }

      nMaxGrab.Value = _configFile.Info.GrabDays;
    }

    private void LoadOldConfigFile()
    {
      _log.Info(LogType.WebEPG, "Trying to load old config file format");

      _configFile = new WebepgConfigFile();

      MediaPortal.Webepg.Profile.Xml xmlreader = new MediaPortal.Webepg.Profile.Xml(startDirectory + "\\WebEPG.xml");

      _configFile.Info = new WebepgInfo();
      _configFile.Info.GrabDays = xmlreader.GetValueAsInt("General", "MaxDays", 2);
      _configFile.Info.GrabberDir = xmlreader.GetValueAsString("General", "GrabberDir", null);

      int AuthCount = xmlreader.GetValueAsInt("AuthSites", "Count", 0);
      if (AuthCount > 0)
      {
        _configFile.Sites = new List<SiteAuth>();
        for (int i = 1; i <= AuthCount; i++)
        {
          SiteAuth site = new SiteAuth();
          site.id = xmlreader.GetValueAsString("Auth" + i.ToString(), "Site", "");
          site.username = xmlreader.GetValueAsString("Auth" + i.ToString(), "Login", "");
          site.password = xmlreader.GetValueAsString("Auth" + i.ToString(), "Password", "");
          _configFile.Sites.Add(site);
        }
      }

      int mergeCount = xmlreader.GetValueAsInt("MergeChannels", "Count", 0);
      Dictionary<string, List<MergedChannel>> mergedList = new Dictionary<string,List<MergedChannel>>();

      if (mergeCount > 0)
      {
        for (int i = 1; i <= mergeCount; i++)
        {
          int channelcount = xmlreader.GetValueAsInt("Merge" + i.ToString(), "Channels", 0);
          if (channelcount > 0)
          {
            List<MergedChannel> mergedChannels = new List<MergedChannel>();
            string displayName = xmlreader.GetValueAsString("Merge" + i.ToString(), "DisplayName", "");

            for (int c = 1; c <= channelcount; c++)
            {
              MergedChannel channel = new MergedChannel();
              channel.id = xmlreader.GetValueAsString("Merge" + i.ToString(), "Channel" + c.ToString(), "");
              channel.start = xmlreader.GetValueAsString("Merge" + i.ToString(), "Start" + c.ToString(), "0:0");
              channel.end = xmlreader.GetValueAsString("Merge" + i.ToString(), "End" + c.ToString(), "0:0");
              mergedChannels.Add(channel);
            }
            mergedList.Add(displayName, mergedChannels);
          }
        }
      }

      _configFile.Channels = new List<ChannelMap>();

      int channelCount = xmlreader.GetValueAsInt("ChannelMap", "Count", 0);

      for (int i = 1; i <= channelCount; i++)
      {
        ChannelMap channel = new ChannelMap();
        channel.displayName = xmlreader.GetValueAsString(i.ToString(), "DisplayName", "");
        string grabber = xmlreader.GetValueAsString(i.ToString(), "Grabber1", "");;
        if (mergedList.ContainsKey(channel.displayName))
        {
          channel.merged = mergedList[channel.displayName];
          foreach (MergedChannel mergedChannel in channel.merged)
            mergedChannel.grabber = grabber;
        }
        else
        {
          channel.id = xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
          channel.grabber = grabber;
        }
        _configFile.Channels.Add(channel);
      }

      xmlreader.Dispose();
    }

    private void GetGrabbers(ref TreeNode Main, string Location)
    {
      System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Directory: {0}", Location);
      GrabberConfigInfo gInfo;
      foreach (System.IO.FileInfo file in dir.GetFiles("*.xml"))
      {
        gInfo = new GrabberConfigInfo();
        //XmlDocument xml = new XmlDocument();
        GrabberConfigFile grabberXml;
        try
        {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File: {0}", file.Name);

          XmlSerializer s = new XmlSerializer(typeof(GrabberConfigFile));
          TextReader r = new StreamReader(file.FullName);
          grabberXml = (GrabberConfigFile)s.Deserialize(r);
        }
        catch (Exception)
        {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File open failed - XML error");
          return;
        }

        gInfo.GrabDays = grabberXml.Info.GrabDays;

        string GrabberSite = file.Name.Replace(".xml", "");
        GrabberSite = GrabberSite.Replace("_", ".");

        gInfo.GrabberID = file.Directory.Name + "\\" + file.Name;
        gInfo.GrabberName = GrabberSite;
        gInfo.Country = file.Directory.Name;
        hGrabberConfigInfo.Add(gInfo.GrabberID, gInfo);

        if (CountryList[file.Directory.Name] == null)
          CountryList.Add(file.Directory.Name, new SortedList());

        TreeNode gNode = new TreeNode(GrabberSite);
        Main.Nodes.Add(gNode);
        //XmlNode cl=sectionList.Attributes.GetNamedItem("ChannelList");

        foreach (ChannelInfo channel in grabberXml.Channels)
        {
          if (channel.id != null)
          {
            ChannelConfigInfo info = (ChannelConfigInfo)hChannelConfigInfo[channel.id];
            if (info != null) // && info.GrabberList[gInfo.GrabberID] != null)
            {
              TreeNode tNode = new TreeNode(info.FullName);
              string[] tag = new string[2];
              tag[0] = info.ChannelID;
              tag[1] = gInfo.GrabberID;
              tNode.Tag = tag;
              gNode.Nodes.Add(tNode);
              if (info.GrabberList == null)
                info.GrabberList = new SortedList();
              if (info.GrabberList[gInfo.GrabberID] == null)
                info.GrabberList.Add(gInfo.GrabberID, gInfo);
            }
            else
            {
              info = new ChannelConfigInfo();
              info.ChannelID = channel.id;
              info.FullName = info.ChannelID;
              info.GrabberList = new SortedList();
              info.GrabberList.Add(gInfo.GrabberID, gInfo);
              hChannelConfigInfo.Add(info.ChannelID, info);

              TreeNode tNode = new TreeNode(info.FullName);
              string[] tag = new string[2];
              tag[0] = info.ChannelID;
              tag[1] = gInfo.GrabberID;
              tNode.Tag = tag;
              gNode.Nodes.Add(tNode);
            }
          }
        }
      }
    }

    private void GetTreeGrabbers(ref TreeNode Main, string Location)
    {
      System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
      System.IO.DirectoryInfo[] dirList = dir.GetDirectories();
      if (dirList.Length > 0)
      {
        if (dirList.Length == 1)
        {
          System.IO.DirectoryInfo g = dirList[0];
          if (g.Name == ".svn")
            GetGrabbers(ref Main, Location);
        }
        else
        {
          for (int i = 0; i < dirList.Length; i++)
          {
            //LOAD FOLDERS
            System.IO.DirectoryInfo g = dirList[i];
            TreeNode MainNext = new TreeNode(g.Name);
            GetTreeGrabbers(ref MainNext, g.FullName);
            Main.Nodes.Add(MainNext);
            //MainNext.Tag = (g.FullName);
          }
        }
      }
      else
      {
        GetGrabbers(ref Main, Location);
      }
    }
    #endregion

    #region Event handlers
    private void bImport_Click(object sender, EventArgs e)
    {
      getTVChannels();
    }

    private void bSave_Click(object sender, EventArgs e)
    {
      _configFile.Info.GrabDays = (int)nMaxGrab.Value;

      _configFile.Channels = new List<ChannelMap>();

      foreach (ChannelMap channel in _channelMapping.Values)
      {
        _configFile.Channels.Add(channel);
      }

      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Button: Save");
      string confFile = startDirectory + "\\WebEPG.xml";
      if (System.IO.File.Exists(confFile))
      {
        System.IO.File.Delete(confFile.Replace(".xml", ".bak"));
        System.IO.File.Move(confFile, confFile.Replace(".xml", ".bak"));
      }

      XmlSerializer s = new XmlSerializer(typeof(WebepgConfigFile));
      TextWriter w = new StreamWriter(confFile);
      s.Serialize(w, _configFile);
      w.Close();
    }

    private void cbSource_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (cbSource.SelectedItem.ToString())
      {
        case "MediaPortal":
          bImport.Enabled = true;
          break;
        case "TV Server":
          bImport.Enabled = false;
          break;
        default:
          break;
      }
    }

    private void bAdd_Click(object sender, EventArgs e)
    {
      if (!_channelMapping.ContainsKey(mtbNewChannel.Text))
      {
        ChannelMap channel = new ChannelMap();
        channel.displayName = mtbNewChannel.Text;
        _channelMapping.Add(channel.displayName, channel);
        RedrawList(channel.displayName);
      }
      else
      {
        MessageBox.Show("Channel with that name already exists", "Name Entry Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    private void bRemove_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem channel in lvMapping.SelectedItems)
      {
        if (_channelMapping.ContainsKey(channel.Text))
          _channelMapping.Remove(channel.Text);
      }

      UpdateList();
    }

    private void bClearMapping_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem channel in lvMapping.SelectedItems)
      {
        if (_channelMapping.ContainsKey(channel.Text))
        {
          ChannelMap channelMap = _channelMapping[channel.Text];
          channelMap.id = null;
          channelMap.grabber = null;
          _channelMapping.Remove(channel.Text);
          _channelMapping.Add(channel.Text, channelMap);
        }
      }

      UpdateList();
    }

    private void bChannelID_Click(object sender, EventArgs e)
    {
      if (selection == null)
      {
        selection = new fSelection(tChannels, tGrabbers, true, this.DoSelect);
        selection.MinimizeBox = false;
        selection.Closed += new System.EventHandler(this.CloseSelect);
        selection.Show();
      }
      else
      {
        selection.BringToFront();
      }
    }

    private void bGrabber_Click(object sender, EventArgs e)
    {
      if (selection == null)
      {
        selection = new fSelection(tChannels, tGrabbers, false, this.DoSelect);
        selection.MinimizeBox = false;
        selection.Closed += new System.EventHandler(this.CloseSelect);
        selection.Show();
      }
      else
      {
        selection.BringToFront();
      }
    }

    private bool UpdateGrabberDetails(string channelId, string grabberId)
    {
      tbChannelName.Text = null;
      tbGrabSite.Text = null;
      tbGrabDays.Text = null;

      if (channelId != null && grabberId != null)
      {
        tbChannelName.Tag = channelId;
        ChannelConfigInfo info = (ChannelConfigInfo)hChannelConfigInfo[channelId];
        if (info != null)
        {
          tbChannelName.Text = info.FullName;
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Selection: {0}", info.FullName);

          GrabberConfigInfo gInfo = (GrabberConfigInfo)info.GrabberList[grabberId];
          if (gInfo != null)
          {
            tbGrabSite.Text = gInfo.GrabberName;
            //tbGrabSite.Tag = gInfo.GrabberID;
            tbGrabDays.Text = gInfo.GrabDays.ToString();
            return true;
          }
          else
          {
            tbGrabSite.Text = "(Unknown)";
          }
        }
      }
      return false;
    }

    private void DisplayChannelGrabberInfo(ChannelMap channel)
    {
      if (channel.merged != null && channel.merged.Count > 0)
      {
        tcMappingDetails.SelectedIndex = 1;
        UpdateMergedList(channel);
      }
      else
      {
        tcMappingDetails.SelectedIndex = 0;
        UpdateGrabberDetails(channel.id, channel.grabber);
      }

      lvMapping.Select();
    }

    private void DoSelect(Object source, EventArgs e)
    {
      this.Activate();
      string[] id = selection.Selected;

      if (id != null)
      {
        if (UpdateGrabberDetails(id[0], id[1]))
        {
          foreach (ListViewItem channel in lvMapping.SelectedItems)
          {
            if (_channelMapping.ContainsKey(channel.Text))
            {
              ChannelMap channelMap = _channelMapping[channel.Text];
              channelMap.id = id[0];
              channelMap.grabber = id[1];
              _channelMapping.Remove(channel.Text);
              _channelMapping.Add(channel.Text, channelMap);
            }
          }
        }

        UpdateList();
      }
    }

    private void CloseSelect(Object source, EventArgs e)
    {
      if (source == selection)
        selection = null;
    }

    private void lvMapping_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (lvMapping.SelectedItems.Count > 0)
      {
        ChannelMap channel = _channelMapping[lvMapping.SelectedItems[0].Text];
        DisplayChannelGrabberInfo(channel);
      }
    }

    private void bAutoMap_Click(object sender, EventArgs e)
    {
      lvMapping.Select();
      if (cbCountry.SelectedItem != null)
      {
        Cursor.Current = Cursors.WaitCursor;
        List<ChannelGrabberInfo> channels = _channelInfo.GetChannelArrayList(_countryList[cbCountry.SelectedItem.ToString()]);
        foreach (ListViewItem channel in lvMapping.Items)
        {
          ChannelMap channelMap = _channelMapping[channel.Text];
          if (channelMap.id == null)
          {
            lvMapping.SelectedItems.Clear();
            channel.Selected = true;
            channel.EnsureVisible();
            lvMapping.Refresh();
            int channelNumb = _channelInfo.FindChannel(channel.Text, _countryList[cbCountry.SelectedItem.ToString()]);
            if (channelNumb >= 0)
            {
              ChannelGrabberInfo channelDetails = channels[channelNumb];
              if (channelDetails.GrabberList != null)
              {
                channelMap.id = channelDetails.ChannelID;
                channelMap.grabber = channelDetails.GrabberList[0].GrabberID;
                _channelMapping.Remove(channel.Text);
                _channelMapping.Add(channel.Text, channelMap);
              }
              UpdateList();
            }
          }
        }
        Cursor.Current = Cursors.Default;
      }
    }

    private void lvMapping_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      // Determine if clicked column is already the column that is being sorted.
      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter.Order == SortOrder.Ascending)
        {
          lvwColumnSorter.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter.SortColumn = e.Column;
        lvwColumnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.lvMapping.Sort();
    }
    #endregion

    private void bMergedAdd_Click(object sender, EventArgs e)
    {
      lvMerged.SelectedItems.Clear();
      _mergeConfig = new MergedChannelDetails(tChannels, tGrabbers, null, this.bMergedOk_Click);
      _mergeConfig.MinimizeBox = false;
      _mergeConfig.Show();
    }

    private void bMergedOk_Click(object sender, EventArgs e)
    {
      if (lvMapping.SelectedItems.Count == 1)
      {
        ChannelMap channelMap = _channelMapping[lvMapping.SelectedItems[0].Text];
        if (lvMerged.SelectedItems.Count == 1)
        {
          MergedChannel channelDetails = (MergedChannel)lvMerged.SelectedItems[0].Tag;

          channelDetails.id = _mergeConfig.ChannelDetails.id;
          channelDetails.grabber = _mergeConfig.ChannelDetails.grabber;
          channelDetails.start = _mergeConfig.ChannelDetails.start;
          channelDetails.end = _mergeConfig.ChannelDetails.end;
        }
        else
        {
          channelMap.merged.Add(_mergeConfig.ChannelDetails);
        }
        UpdateMergedList(channelMap);
      }
      _mergeConfig.Close();
    }

    private void bMergedRemove_Click(object sender, EventArgs e)
    {
      if (lvMerged.SelectedItems.Count == 1 && lvMapping.SelectedItems.Count == 1)
      {
        ChannelMap channelMap = _channelMapping[lvMapping.SelectedItems[0].Text];
        channelMap.merged.Remove((MergedChannel)lvMerged.SelectedItems[0].Tag);
        UpdateMergedList(channelMap);
      }
    }

    private void bMergedEdit_Click(object sender, EventArgs e)
    {
      if (lvMerged.SelectedItems.Count == 1 && lvMapping.SelectedItems.Count == 1)
      {
        MergedChannel channel = (MergedChannel) lvMerged.SelectedItems[0].Tag;
        _mergeConfig = new MergedChannelDetails(tChannels, tGrabbers, channel, this.bMergedOk_Click);
        _mergeConfig.MinimizeBox = false;
        _mergeConfig.Show();
      }
    }

    private void tcMappingDetails_Selecting(object sender, TabControlCancelEventArgs e)
    {
      if (tcMappingDetails.SelectedIndex == 1)
      {
        if (lvMapping.SelectedItems.Count == 1)
        {
          if (_channelMapping.ContainsKey(lvMapping.SelectedItems[0].Text))
          {
            ChannelMap channelMap = _channelMapping[lvMapping.SelectedItems[0].Text];
            if (channelMap.merged == null || channelMap.merged.Count == 0)
            {
              channelMap.merged = new List<MergedChannel>();
              MergedChannel channel = new MergedChannel();
              channel.id = channelMap.id;
              channelMap.id = null;
              channel.grabber = channelMap.grabber;
              channelMap.grabber = null;
              channelMap.merged.Add(channel);
              //_channelMapping.Remove(channel.Text);
              //_channelMapping.Add(channel.Text, channelMap);
            }
            UpdateMergedList(channelMap);
            UpdateList();
          }
        }
        else
        {
          e.Cancel = true;
          MessageBox.Show("Only one channel can be mapped to multiple channels at a time.", "Multiple Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
      else
      {
        if (lvMapping.SelectedItems.Count == 1)
        {
          if (_channelMapping.ContainsKey(lvMapping.SelectedItems[0].Text))
          {
            if (_channelMapping[lvMapping.SelectedItems[0].Text].merged.Count <= 1)
            {
              ChannelMap channelMap = _channelMapping[lvMapping.SelectedItems[0].Text];
              if (channelMap.merged != null && channelMap.merged.Count > 0)
              {
                channelMap.id = channelMap.merged[0].id;
                channelMap.grabber = channelMap.merged[0].grabber;
                channelMap.merged = null;
                //_channelMapping.Remove(channel.Text);
                //_channelMapping.Add(channel.Text, channelMap);
              }
              UpdateMergedList(channelMap);
              UpdateList();
            }
            else
            {
              e.Cancel = true;
              MessageBox.Show("Cannot convert multiple channels to single channel. Please remove one.", "Multiple Channel Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
          }
        }
      }
    }
  }
}