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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class FileInfoScraperForm : Form
  {
    private AppItem m_CurApp;
    private Panel leftPanel;
    private Panel rightPanel;
    private Splitter splitterVert;
    private ListView FileList;
    private ColumnHeader FileTitle;
    private ColumnHeader columnHeader1;
    private ColumnHeader status;
    private ColumnHeader columnHeader2;
    private ListView MatchList;
    private Label lblFiles;
    private Label lblMatches;
    private Button btnStartSearch;
    private Button btnSaveSearch;
    private Button btnCancel;
    private LinkLabel allGameLink;
    private Button checkAllButton;
    private ToolTip toolTip1;
    private Button uncheckAllButton;
    private Button buttonSelectBestMatch;
    private Label filterLabel;
    private ComboBox filterComboBox;
    private Button ResetFilterButton;
    private NumericUpDown MinRelevanceNum;
    private Label label1;
    private Button LaunchURLButton;
    private Panel bottomPanel;
    private ContextMenu menuFileList;
    private MenuItem mnuCheckWithoutImages;
    private MenuItem mnuCheckWithoutOverview;
    private ProgressBar progressBar;
    private Panel progressPanel;
    private Label progressStatusLabel;
    private Button cancelButton;
    private IContainer components;


    int mStartTime = 0; // timer stuff
    private ContextMenu menuSaveDetails;
    private MenuItem menuItem4;
    private MenuItem menuDataAndImages;
    private MenuItem menuData;
    private MenuItem menuImages;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    bool stopSearching = false;

    public AppItem CurApp
    {
      get
      {
        return m_CurApp;
      }
      set
      {
        SetCurApp(value);
      }
    }

    void SetCurApp(AppItem value)
    {
      m_CurApp = value;
      if (m_CurApp != null)
      {
        filterComboBox.Text = m_CurApp.SystemDefault;
      }
    }

    public FileInfoScraperForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
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

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FileInfoScraperForm));
      this.bottomPanel = new System.Windows.Forms.Panel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressPanel = new System.Windows.Forms.Panel();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.progressStatusLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.MinRelevanceNum = new System.Windows.Forms.NumericUpDown();
      this.ResetFilterButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.filterComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.filterLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonSelectBestMatch = new MediaPortal.UserInterface.Controls.MPButton();
      this.allGameLink = new System.Windows.Forms.LinkLabel();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnSaveSearch = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnStartSearch = new MediaPortal.UserInterface.Controls.MPButton();
      this.leftPanel = new System.Windows.Forms.Panel();
      this.uncheckAllButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkAllButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.lblFiles = new MediaPortal.UserInterface.Controls.MPLabel();
      this.FileList = new System.Windows.Forms.ListView();
      this.FileTitle = new System.Windows.Forms.ColumnHeader();
      this.status = new System.Windows.Forms.ColumnHeader();
      this.menuFileList = new System.Windows.Forms.ContextMenu();
      this.mnuCheckWithoutImages = new System.Windows.Forms.MenuItem();
      this.mnuCheckWithoutOverview = new System.Windows.Forms.MenuItem();
      this.splitterVert = new System.Windows.Forms.Splitter();
      this.rightPanel = new System.Windows.Forms.Panel();
      this.LaunchURLButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.lblMatches = new MediaPortal.UserInterface.Controls.MPLabel();
      this.MatchList = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.menuSaveDetails = new System.Windows.Forms.ContextMenu();
      this.menuDataAndImages = new System.Windows.Forms.MenuItem();
      this.menuItem4 = new System.Windows.Forms.MenuItem();
      this.menuData = new System.Windows.Forms.MenuItem();
      this.menuImages = new System.Windows.Forms.MenuItem();
      this.bottomPanel.SuspendLayout();
      this.progressPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.MinRelevanceNum)).BeginInit();
      this.leftPanel.SuspendLayout();
      this.rightPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // bottomPanel
      // 
      this.bottomPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.bottomPanel.Controls.Add(this.label2);
      this.bottomPanel.Controls.Add(this.progressPanel);
      this.bottomPanel.Controls.Add(this.label1);
      this.bottomPanel.Controls.Add(this.MinRelevanceNum);
      this.bottomPanel.Controls.Add(this.ResetFilterButton);
      this.bottomPanel.Controls.Add(this.filterComboBox);
      this.bottomPanel.Controls.Add(this.filterLabel);
      this.bottomPanel.Controls.Add(this.buttonSelectBestMatch);
      this.bottomPanel.Controls.Add(this.allGameLink);
      this.bottomPanel.Controls.Add(this.btnCancel);
      this.bottomPanel.Controls.Add(this.btnSaveSearch);
      this.bottomPanel.Controls.Add(this.btnStartSearch);
      this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.bottomPanel.Location = new System.Drawing.Point(0, 422);
      this.bottomPanel.Name = "bottomPanel";
      this.bottomPanel.Size = new System.Drawing.Size(752, 112);
      this.bottomPanel.TabIndex = 0;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(520, 8);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(224, 40);
      this.label2.TabIndex = 28;
      this.label2.Text = "Warning: Don\'t overuse the allgame scraper! Do lookups with small sets of games (" +
        "max. 20 games).";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // progressPanel
      // 
      this.progressPanel.Controls.Add(this.cancelButton);
      this.progressPanel.Controls.Add(this.progressStatusLabel);
      this.progressPanel.Controls.Add(this.progressBar);
      this.progressPanel.Enabled = false;
      this.progressPanel.Location = new System.Drawing.Point(8, 69);
      this.progressPanel.Name = "progressPanel";
      this.progressPanel.Size = new System.Drawing.Size(480, 40);
      this.progressPanel.TabIndex = 27;
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(392, 16);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(88, 23);
      this.cancelButton.TabIndex = 29;
      this.cancelButton.Text = "Cancel Search";
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // progressStatusLabel
      // 
      this.progressStatusLabel.Location = new System.Drawing.Point(0, 20);
      this.progressStatusLabel.Name = "progressStatusLabel";
      this.progressStatusLabel.Size = new System.Drawing.Size(392, 16);
      this.progressStatusLabel.TabIndex = 28;
      this.progressStatusLabel.Text = "Progress status";
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 0);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(480, 16);
      this.progressBar.TabIndex = 27;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(344, 6);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(88, 14);
      this.label1.TabIndex = 25;
      this.label1.Text = "Min. Relevance";
      // 
      // MinRelevanceNum
      // 
      this.MinRelevanceNum.Increment = new System.Decimal(new int[] {
                                                                      10,
                                                                      0,
                                                                      0,
                                                                      0});
      this.MinRelevanceNum.Location = new System.Drawing.Point(432, 3);
      this.MinRelevanceNum.Name = "MinRelevanceNum";
      this.MinRelevanceNum.Size = new System.Drawing.Size(56, 20);
      this.MinRelevanceNum.TabIndex = 24;
      this.toolTip1.SetToolTip(this.MinRelevanceNum, "This is the minimal RELEVANCE value to autoselect a match");
      this.MinRelevanceNum.Value = new System.Decimal(new int[] {
                                                                  70,
                                                                  0,
                                                                  0,
                                                                  0});
      this.MinRelevanceNum.ValueChanged += new System.EventHandler(this.MinRelevanceNum_ValueChanged);
      // 
      // ResetFilterButton
      // 
      this.ResetFilterButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.ResetFilterButton.Location = new System.Drawing.Point(282, 3);
      this.ResetFilterButton.Name = "ResetFilterButton";
      this.ResetFilterButton.Size = new System.Drawing.Size(40, 21);
      this.ResetFilterButton.TabIndex = 23;
      this.ResetFilterButton.Text = "Clear";
      this.toolTip1.SetToolTip(this.ResetFilterButton, "Reset Filter");
      this.ResetFilterButton.Click += new System.EventHandler(this.ResetFilterButton_Click);
      // 
      // filterComboBox
      // 
      this.filterComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.filterComboBox.Items.AddRange(new object[] {
                                                        "Arcade",
                                                        "Atari 5200",
                                                        "Atari 7800",
                                                        "Atari Lynx",
                                                        "Atari ST",
                                                        "Atari Video Computer System",
                                                        "Commodore 64/128",
                                                        "Commodore Amiga",
                                                        "Game Boy",
                                                        "Game Boy Advance",
                                                        "Game Boy Color",
                                                        "Neo Geo",
                                                        "Nintendo 64",
                                                        "Nintendo Entertainment System",
                                                        "PlayStation",
                                                        "Sega Dreamcast",
                                                        "Sega Game Gear",
                                                        "Sega Genesis",
                                                        "Sega Master System",
                                                        "Super NES",
                                                        "TurboGrafx-16"});
      this.filterComboBox.Location = new System.Drawing.Point(72, 3);
      this.filterComboBox.Name = "filterComboBox";
      this.filterComboBox.Size = new System.Drawing.Size(208, 21);
      this.filterComboBox.TabIndex = 22;
      this.toolTip1.SetToolTip(this.filterComboBox, "Enter platform to filter results");
      this.filterComboBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.filterComboBox_KeyUp);
      this.filterComboBox.SelectedIndexChanged += new System.EventHandler(this.filterComboBox_SelectedIndexChanged);
      // 
      // filterLabel
      // 
      this.filterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.filterLabel.Location = new System.Drawing.Point(8, 5);
      this.filterLabel.Name = "filterLabel";
      this.filterLabel.Size = new System.Drawing.Size(80, 16);
      this.filterLabel.TabIndex = 21;
      this.filterLabel.Text = "Platform:";
      // 
      // buttonSelectBestMatch
      // 
      this.buttonSelectBestMatch.Enabled = false;
      this.buttonSelectBestMatch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.buttonSelectBestMatch.Location = new System.Drawing.Point(168, 32);
      this.buttonSelectBestMatch.Name = "buttonSelectBestMatch";
      this.buttonSelectBestMatch.Size = new System.Drawing.Size(160, 32);
      this.buttonSelectBestMatch.TabIndex = 20;
      this.buttonSelectBestMatch.Text = "2) Select Best Match";
      this.toolTip1.SetToolTip(this.buttonSelectBestMatch, "Select the best match for all checked files (");
      this.buttonSelectBestMatch.Click += new System.EventHandler(this.buttonSelectBestMatch_Click);
      // 
      // allGameLink
      // 
      this.allGameLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.allGameLink.Location = new System.Drawing.Point(616, 56);
      this.allGameLink.Name = "allGameLink";
      this.allGameLink.Size = new System.Drawing.Size(128, 16);
      this.allGameLink.TabIndex = 3;
      this.allGameLink.TabStop = true;
      this.allGameLink.Text = "http://www.allgame.com";
      this.allGameLink.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.allGameLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.allGameLink_LinkClicked);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.Location = new System.Drawing.Point(668, 76);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Close";
      this.btnCancel.Click += new System.EventHandler(this.button3_Click);
      // 
      // btnSaveSearch
      // 
      this.btnSaveSearch.Enabled = false;
      this.btnSaveSearch.Location = new System.Drawing.Point(328, 32);
      this.btnSaveSearch.Name = "btnSaveSearch";
      this.btnSaveSearch.Size = new System.Drawing.Size(160, 32);
      this.btnSaveSearch.TabIndex = 1;
      this.btnSaveSearch.Text = "3) Download && Save Details";
      this.toolTip1.SetToolTip(this.btnSaveSearch, "Download selected matches and save results to MediaPortal!");
      this.btnSaveSearch.Click += new System.EventHandler(this.btnSaveSearch_Click);
      // 
      // btnStartSearch
      // 
      this.btnStartSearch.Enabled = false;
      this.btnStartSearch.Location = new System.Drawing.Point(8, 32);
      this.btnStartSearch.Name = "btnStartSearch";
      this.btnStartSearch.Size = new System.Drawing.Size(160, 32);
      this.btnStartSearch.TabIndex = 0;
      this.btnStartSearch.Text = "1) Start Search";
      this.toolTip1.SetToolTip(this.btnStartSearch, "Search Details for all the checked files");
      this.btnStartSearch.Click += new System.EventHandler(this.btnStartSearch_Click);
      // 
      // leftPanel
      // 
      this.leftPanel.Controls.Add(this.uncheckAllButton);
      this.leftPanel.Controls.Add(this.checkAllButton);
      this.leftPanel.Controls.Add(this.lblFiles);
      this.leftPanel.Controls.Add(this.FileList);
      this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
      this.leftPanel.Location = new System.Drawing.Point(0, 0);
      this.leftPanel.Name = "leftPanel";
      this.leftPanel.Size = new System.Drawing.Size(360, 422);
      this.leftPanel.TabIndex = 5;
      // 
      // uncheckAllButton
      // 
      this.uncheckAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.uncheckAllButton.Image = ((System.Drawing.Image)(resources.GetObject("uncheckAllButton.Image")));
      this.uncheckAllButton.Location = new System.Drawing.Point(320, 8);
      this.uncheckAllButton.Name = "uncheckAllButton";
      this.uncheckAllButton.Size = new System.Drawing.Size(32, 32);
      this.uncheckAllButton.TabIndex = 16;
      this.toolTip1.SetToolTip(this.uncheckAllButton, "Uncheck all");
      this.uncheckAllButton.Click += new System.EventHandler(this.uncheckAllButton_Click);
      // 
      // checkAllButton
      // 
      this.checkAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.checkAllButton.Image = ((System.Drawing.Image)(resources.GetObject("checkAllButton.Image")));
      this.checkAllButton.Location = new System.Drawing.Point(288, 8);
      this.checkAllButton.Name = "checkAllButton";
      this.checkAllButton.Size = new System.Drawing.Size(32, 32);
      this.checkAllButton.TabIndex = 15;
      this.toolTip1.SetToolTip(this.checkAllButton, "Check all");
      this.checkAllButton.Click += new System.EventHandler(this.checkAllButton_Click);
      // 
      // lblFiles
      // 
      this.lblFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.lblFiles.Location = new System.Drawing.Point(8, 16);
      this.lblFiles.Name = "lblFiles";
      this.lblFiles.Size = new System.Drawing.Size(200, 16);
      this.lblFiles.TabIndex = 14;
      this.lblFiles.Text = "Files";
      // 
      // FileList
      // 
      this.FileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.FileList.CheckBoxes = true;
      this.FileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                               this.FileTitle,
                                                                               this.status});
      this.FileList.ContextMenu = this.menuFileList;
      this.FileList.FullRowSelect = true;
      this.FileList.HideSelection = false;
      this.FileList.LabelEdit = true;
      this.FileList.Location = new System.Drawing.Point(8, 40);
      this.FileList.Name = "FileList";
      this.FileList.Size = new System.Drawing.Size(344, 368);
      this.FileList.TabIndex = 13;
      this.FileList.View = System.Windows.Forms.View.Details;
      this.FileList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FileList_MouseUp);
      this.FileList.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.FileList_AfterLabelEdit);
      this.FileList.SelectedIndexChanged += new System.EventHandler(this.FileList_SelectedIndexChanged);
      // 
      // FileTitle
      // 
      this.FileTitle.Text = "Title";
      this.FileTitle.Width = 218;
      // 
      // status
      // 
      this.status.Text = "Status";
      this.status.Width = 102;
      // 
      // menuFileList
      // 
      this.menuFileList.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                 this.mnuCheckWithoutImages,
                                                                                 this.mnuCheckWithoutOverview});
      // 
      // mnuCheckWithoutImages
      // 
      this.mnuCheckWithoutImages.Index = 0;
      this.mnuCheckWithoutImages.Text = "Check all files without images";
      this.mnuCheckWithoutImages.Click += new System.EventHandler(this.mnuCheckWithoutImages_Click);
      // 
      // mnuCheckWithoutOverview
      // 
      this.mnuCheckWithoutOverview.Index = 1;
      this.mnuCheckWithoutOverview.Text = "Check all files without an overview";
      this.mnuCheckWithoutOverview.Click += new System.EventHandler(this.mnuCheckWithoutOverview_Click);
      // 
      // splitterVert
      // 
      this.splitterVert.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.splitterVert.Location = new System.Drawing.Point(360, 0);
      this.splitterVert.Name = "splitterVert";
      this.splitterVert.Size = new System.Drawing.Size(5, 422);
      this.splitterVert.TabIndex = 6;
      this.splitterVert.TabStop = false;
      // 
      // rightPanel
      // 
      this.rightPanel.Controls.Add(this.LaunchURLButton);
      this.rightPanel.Controls.Add(this.lblMatches);
      this.rightPanel.Controls.Add(this.MatchList);
      this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rightPanel.Location = new System.Drawing.Point(365, 0);
      this.rightPanel.Name = "rightPanel";
      this.rightPanel.Size = new System.Drawing.Size(387, 422);
      this.rightPanel.TabIndex = 7;
      // 
      // LaunchURLButton
      // 
      this.LaunchURLButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.LaunchURLButton.Enabled = false;
      this.LaunchURLButton.Location = new System.Drawing.Point(272, 12);
      this.LaunchURLButton.Name = "LaunchURLButton";
      this.LaunchURLButton.Size = new System.Drawing.Size(104, 24);
      this.LaunchURLButton.TabIndex = 16;
      this.LaunchURLButton.Text = "Launch URL";
      this.toolTip1.SetToolTip(this.LaunchURLButton, "Show allgame-page in your browser");
      this.LaunchURLButton.Click += new System.EventHandler(this.LaunchURLButton_Click);
      // 
      // lblMatches
      // 
      this.lblMatches.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.lblMatches.Location = new System.Drawing.Point(16, 18);
      this.lblMatches.Name = "lblMatches";
      this.lblMatches.Size = new System.Drawing.Size(56, 16);
      this.lblMatches.TabIndex = 15;
      this.lblMatches.Text = "Matches:";
      // 
      // MatchList
      // 
      this.MatchList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.MatchList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                this.columnHeader1,
                                                                                this.columnHeader2});
      this.MatchList.FullRowSelect = true;
      this.MatchList.HideSelection = false;
      this.MatchList.Location = new System.Drawing.Point(16, 40);
      this.MatchList.MultiSelect = false;
      this.MatchList.Name = "MatchList";
      this.MatchList.Size = new System.Drawing.Size(358, 368);
      this.MatchList.TabIndex = 14;
      this.MatchList.View = System.Windows.Forms.View.Details;
      this.MatchList.DoubleClick += new System.EventHandler(this.MatchList_DoubleClick);
      this.MatchList.SelectedIndexChanged += new System.EventHandler(this.MatchList_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Title (Platform)";
      this.columnHeader1.Width = 247;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Relevance";
      this.columnHeader2.Width = 80;
      // 
      // menuSaveDetails
      // 
      this.menuSaveDetails.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                    this.menuDataAndImages,
                                                                                    this.menuItem4,
                                                                                    this.menuData,
                                                                                    this.menuImages});
      // 
      // menuDataAndImages
      // 
      this.menuDataAndImages.Index = 0;
      this.menuDataAndImages.Text = "Save Data and download images";
      this.menuDataAndImages.Click += new System.EventHandler(this.menuDataAndImages_Click);
      // 
      // menuItem4
      // 
      this.menuItem4.Index = 1;
      this.menuItem4.Text = "-";
      // 
      // menuData
      // 
      this.menuData.Index = 2;
      this.menuData.Text = "Save Data only";
      this.menuData.Click += new System.EventHandler(this.menuData_Click);
      // 
      // menuImages
      // 
      this.menuImages.Index = 3;
      this.menuImages.Text = "Download images only";
      this.menuImages.Click += new System.EventHandler(this.menuImages_Click);
      // 
      // FileInfoScraperForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(752, 534);
      this.Controls.Add(this.rightPanel);
      this.Controls.Add(this.splitterVert);
      this.Controls.Add(this.leftPanel);
      this.Controls.Add(this.bottomPanel);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "FileInfoScraperForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Search fileinfo";
      this.bottomPanel.ResumeLayout(false);
      this.progressPanel.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.MinRelevanceNum)).EndInit();
      this.leftPanel.ResumeLayout(false);
      this.rightPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion


    public void Setup()
    {
      SyncListView();
      SyncFileLabel();
    }

    private void SyncListView()
    {
      if (m_CurApp == null)
        return;

      FileList.BeginUpdate();
      try
      {
        FileList.Items.Clear();

        // add all files
        foreach (FileItem file in m_CurApp.Files)
        {
          ListViewItem curItem = new ListViewItem(file.Title);
          curItem.Tag = file;
          if (!file.IsFolder)
          {
            ListViewItem newItem = FileList.Items.Add(curItem);
            newItem.SubItems.Add("<unknown>");
          }
        }
      }
      finally
      {
        FileList.EndUpdate();
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      if ((m_CurApp != null) && (filterComboBox.Text != ""))
      {
        m_CurApp.SystemDefault = filterComboBox.Text;
        m_CurApp.Write();
      }
      this.Close();
    }

    void InitProgressBar(string msg)
    {
      progressPanel.Enabled = true;
      progressBar.Value = 0;
      if (FileList.CheckedItems.Count - 1 > 0)
      {
        progressBar.Maximum = FileList.CheckedItems.Count - 1;
      }
      else
      {
        progressBar.Maximum = 1;
      }
      progressBar.Step = 1;
      progressStatusLabel.Text = msg;
      mStartTime = (int)(DateTime.Now.Ticks / 10000); // reset timer!
    }

    void StepProgressBar()
    {
      string strTimeRemaining = "";
      progressBar.PerformStep();
      if (progressBar.Value > 1)
      {
        int nTimeElapsed = ((int)(DateTime.Now.Ticks / 10000)) - mStartTime;
        double TimePerItem = nTimeElapsed / progressBar.Value - 1;
        int nTotalTime = (int)(progressBar.Maximum * TimePerItem);
        int nTimeRemaining = nTotalTime - nTimeElapsed;
        int nSecondsRemaining = nTimeRemaining / 1000;
        int nMinutesRemaining = nSecondsRemaining / 60;
        nSecondsRemaining = nSecondsRemaining - (nMinutesRemaining * 60);
        strTimeRemaining = String.Format(" ({0}m {1}s remaining)", nMinutesRemaining, nSecondsRemaining);

      }
      progressStatusLabel.Text = String.Format("Searching file {0} of {1} ", progressBar.Value, progressBar.Maximum + 1) + strTimeRemaining;
    }

    void DeInitProgressBar(string msg)
    {
      progressPanel.Enabled = false;
      progressStatusLabel.Text = msg;
    }

    private void btnStartSearch_Click(object sender, EventArgs e)
    {
      int numberOfSearches = 0;
      bool bSuccess = true;
      InitProgressBar("Starting search");
      foreach (ListViewItem curItem in FileList.CheckedItems)
      {
        if (stopSearching)
          break;
        ListViewItem nextItem = null;
        FileItem file = (FileItem)curItem.Tag;
        if (file != null)
        {
          if (curItem.Index < FileList.Items.Count - 1)
          {
            nextItem = FileList.Items[curItem.Index + 1];
          }
          else
          {
            nextItem = curItem;
          }
          nextItem.EnsureVisible();
          //          if (!bSuccess)
          //          {
          //            curItem.SubItems[1].Text = String.Format("waiting for reconnection...");
          //            System.Threading.Thread.Sleep(5126);
          //          }
          numberOfSearches = numberOfSearches + 1;
          if (numberOfSearches > 20)
          {
            curItem.SubItems[1].Text = String.Format("waiting...");
            System.Threading.Thread.Sleep(20000);
            System.Windows.Forms.Application.DoEvents();
            numberOfSearches = 0;
          }
          curItem.SubItems[1].Text = String.Format("searching...");
          curItem.Font = new Font(curItem.Font, curItem.Font.Style | FontStyle.Bold);
          System.Windows.Forms.Application.DoEvents();
          bSuccess = file.FindFileInfo(myProgScraperType.ALLGAME);
          curItem.SubItems[1].Text = String.Format("{0} matches", file.FileInfoList.Count);
          StepProgressBar();
          buttonSelectBestMatch.Enabled = true;
          System.Windows.Forms.Application.DoEvents();
        }
      }
      ChangeFileSelection();
      if (stopSearching)
      {
        DeInitProgressBar("Search aborted");
      }
      else
      {
        DeInitProgressBar("Search finished");
      }
      stopSearching = false;
    }


    private FileItem GetSelectedFileItem()
    {
      FileItem res = null;
      if (FileList.FocusedItem != null)
      {
        if (FileList.FocusedItem.Tag != null)
        {
          res = (FileItem)FileList.FocusedItem.Tag;
        }
      }
      return res;
    }

    private FileInfo GetSelectedMatchItem()
    {
      FileInfo res = null;
      if (MatchList.SelectedItems != null)
      {
        if (MatchList.SelectedItems[0] != null)
        {
          if (MatchList.SelectedItems[0].Tag != null)
          {
            res = (FileInfo)MatchList.SelectedItems[0].Tag;
          }
        }
      }
      return res;
    }


    private bool IsGoodMatch(FileInfo info)
    {
      bool result = (filterComboBox.Text == "") || (info.Platform.ToLower().IndexOf(filterComboBox.Text.ToLower()) == 0);
      if (result)
      {
        result = (info.RelevanceNorm >= MinRelevanceNum.Value);
      }
      return result;
    }

    private void SyncMatchesList(FileItem file)
    {
      LaunchURLButton.Enabled = false;
      MatchList.BeginUpdate();
      try
      {
        MatchList.Items.Clear();
        if (file != null)
        {
          if (file.FileInfoList != null)
          {
            foreach (FileInfo item in file.FileInfoList)
            {
              if (IsGoodMatch(item))
              {
                ListViewItem curItem = new ListViewItem(String.Format("{0} ({1})", item.Title, item.Platform));
                curItem.SubItems.Add(String.Format("{0}%", item.RelevanceNorm));
                //							curItem.SubItems[1].Text = String.Format("{0}%", item.Relevance);
                curItem.Tag = item;
                curItem = MatchList.Items.Add(curItem);
                // selected item?
                if ((file.FileInfoFavourite != null) && (file.FileInfoFavourite == item))
                {
                  curItem.Selected = true;
                  LaunchURLButton.Enabled = true;
                }
              }
            }
          }
        }
      }
      finally
      {
        MatchList.EndUpdate();
      }

    }


    private void ChangeFileSelection()
    {
      FileItem file = GetSelectedFileItem();
      SyncMatchesList(file);
    }

    private void FileList_SelectedIndexChanged(object sender, EventArgs e)
    {
      ChangeFileSelection();
    }

    private void allGameLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (allGameLink.Text == null)
        return;
      if (allGameLink.Text.Length > 0)
      {
        ProcessStartInfo sInfo = new ProcessStartInfo(allGameLink.Text);
        Process.Start(sInfo);
      }
    }

    private void LaunchSelectedMatchURL()
    {
      FileInfo info = GetSelectedMatchItem();
      if (info == null)
        return;
      info.LaunchURL();
    }


    private void MatchList_DoubleClick(object sender, EventArgs e)
    {
      LaunchSelectedMatchURL();
    }

    private void checkAllButton_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem curItem in FileList.Items)
      {
        curItem.Checked = true;
      }
      btnStartSearch.Enabled = (FileList.CheckedItems.Count > 0);
      SyncFileLabel();
    }

    private void uncheckAllButton_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem curItem in FileList.Items)
      {
        curItem.Checked = false;
      }
      btnStartSearch.Enabled = false;
      buttonSelectBestMatch.Enabled = false;
      btnSaveSearch.Enabled = false;
      SyncFileLabel();
    }

    private void MatchList_SelectedIndexChanged(object sender, EventArgs e)
    {
      FileItem file = GetSelectedFileItem();
      if (file == null)
        return;

      if (MatchList.SelectedIndices.Count > 0)
      {
        FileInfo info = GetSelectedMatchItem();
        file.FileInfoFavourite = info;
        LaunchURLButton.Enabled = true;
        btnSaveSearch.Enabled = true;
        FileList.FocusedItem.SubItems[1].Text = String.Format("best: {0}%", file.FileInfoFavourite.RelevanceNorm);
      }
      else
      {
        file.FileInfoFavourite = null;
        LaunchURLButton.Enabled = false;
      }
    }

    private void btnSaveSearch_Click(object sender, EventArgs e)
    {
      menuSaveDetails.Show(btnSaveSearch, new Point(0, btnSaveSearch.Height));
    }

    void SaveSearch(ScraperSaveType saveType)
    {
      int numberOfSearches = 0;
      InitProgressBar("Starting search");
      ListViewItem nextItem = null;
      foreach (ListViewItem curItem in FileList.CheckedItems)
      {
        if (stopSearching)
          break;
        FileItem file = (FileItem)curItem.Tag;
        if (file != null)
        {
          if (curItem.Index < FileList.Items.Count - 1)
          {
            nextItem = FileList.Items[curItem.Index + 1];
          }
          else
          {
            nextItem = curItem;
          }
          nextItem.EnsureVisible();
          StepProgressBar();
          if (file.FileInfoFavourite != null)
          {
            numberOfSearches++;
            numberOfSearches = numberOfSearches + 1;
            if (numberOfSearches > 20)
            {
              curItem.SubItems[1].Text = String.Format("waiting...");
              System.Windows.Forms.Application.DoEvents();
              System.Threading.Thread.Sleep(20000);
              numberOfSearches = 0;
            }
            curItem.SubItems[1].Text = String.Format("<searching...>");
            System.Windows.Forms.Application.DoEvents();
            file.FindFileInfoDetail(m_CurApp, file.FileInfoFavourite, myProgScraperType.ALLGAME, saveType);
            if ((saveType == ScraperSaveType.DataAndImages) || (saveType == ScraperSaveType.Data))
            {
              file.SaveFromFileInfoFavourite();
            }
            curItem.SubItems[1].Text = String.Format("<saved>");
            System.Windows.Forms.Application.DoEvents();
          }
        }
      }
      if (stopSearching)
      {
        DeInitProgressBar("Search aborted");
      }
      else
      {
        DeInitProgressBar("Search finished");
      }
      stopSearching = false;
    }

    private void filterComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      ChangeFileSelection();
    }

    private void filterComboBox_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        ChangeFileSelection();
      }
    }

    private void ResetFilterButton_Click(object sender, EventArgs e)
    {
      filterComboBox.Text = "";
      ChangeFileSelection();
    }

    private void buttonSelectBestMatch_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem curItem in FileList.CheckedItems)
      {
        FileItem file = (FileItem)curItem.Tag;
        if (file != null)
        {
          if (file.FileInfoList != null)
          {
            file.FileInfoFavourite = null;
            foreach (FileInfo item in file.FileInfoList)
            {
              if (IsGoodMatch(item))
              {
                btnSaveSearch.Enabled = true;
                file.FileInfoFavourite = item;
                break;
              }
            }
            if (file.FileInfoFavourite != null)
            {
              curItem.SubItems[1].Text = String.Format("best: {0}%", file.FileInfoFavourite.RelevanceNorm);
            }
            else
            {
              curItem.SubItems[1].Text = "no match";
            }
          }
        }
      }
      ChangeFileSelection();
    }

    private void MinRelevanceNum_ValueChanged(object sender, EventArgs e)
    {
      ChangeFileSelection();
    }

    private void LaunchURLButton_Click(object sender, EventArgs e)
    {
      LaunchSelectedMatchURL();
    }

    private void FileList_MouseUp(object sender, MouseEventArgs e)
    {
      SyncButtons();
    }

    void SyncButtons()
    {
      btnStartSearch.Enabled = (FileList.CheckedItems.Count > 0);
      if (!btnStartSearch.Enabled)
      {
        buttonSelectBestMatch.Enabled = false;
        btnSaveSearch.Enabled = false;
      }
      SyncFileLabel();
    }

    private void mnuCheckWithoutImages_Click(object sender, EventArgs e)
    {
      FileItem curFile;
      foreach (ListViewItem curItem in FileList.Items)
      {
        curFile = (FileItem)curItem.Tag;
        if (curFile != null)
        {
          curItem.Checked = (curFile.Imagefile == "");
        }
        else
        {
          curItem.Checked = false;
        }
      }
      btnStartSearch.Enabled = (FileList.CheckedItems.Count > 0);
      SyncFileLabel();
    }

    private void mnuCheckWithoutOverview_Click(object sender, EventArgs e)
    {
      FileItem curFile;
      foreach (ListViewItem curItem in FileList.Items)
      {
        curFile = (FileItem)curItem.Tag;
        if (curFile != null)
        {
          curItem.Checked = (curFile.Overview == "");
        }
        else
        {
          curItem.Checked = false;
        }
      }
      btnStartSearch.Enabled = (FileList.CheckedItems.Count > 0);
      SyncFileLabel();
    }

    private void SyncFileLabel()
    {
      lblFiles.Text = String.Format("Files: ({0} of {1} selected)", FileList.CheckedItems.Count, FileList.Items.Count);
    }

    private void FileList_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      FileItem curItem = GetSelectedFileItem();
      if (curItem == null)
        return;
      curItem.TitleOptimized = e.Label;
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      stopSearching = true;
    }

    private void menuDataAndImages_Click(object sender, EventArgs e)
    {
      SaveSearch(ScraperSaveType.DataAndImages);
    }

    private void menuData_Click(object sender, EventArgs e)
    {
      SaveSearch(ScraperSaveType.Data);
    }

    private void menuImages_Click(object sender, EventArgs e)
    {
      SaveSearch(ScraperSaveType.Images);
    }

  }
}
