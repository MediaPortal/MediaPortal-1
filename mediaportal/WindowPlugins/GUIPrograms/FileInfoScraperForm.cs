using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using MediaPortal.GUI.Library;		
using ProgramsDatabase;
using Programs.Utils;

namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class FileInfoScraperForm : System.Windows.Forms.Form
	{
		private AppItem m_CurApp;
		private System.Windows.Forms.Panel leftPanel;
		private System.Windows.Forms.Panel rightPanel;
		private System.Windows.Forms.Splitter splitterVert;
		private System.Windows.Forms.ListView FileList;
		private System.Windows.Forms.ColumnHeader FileTitle;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader status;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ListView MatchList;
		private System.Windows.Forms.Label lblFiles;
		private System.Windows.Forms.Label lblMatches;
		private System.Windows.Forms.Button btnStartSearch;
		private System.Windows.Forms.Button btnSaveSearch;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.LinkLabel allGameLink;
		private System.Windows.Forms.Button checkAllButton;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Button uncheckAllButton;
		private System.Windows.Forms.Button buttonSelectBestMatch;
		private System.Windows.Forms.Label filterLabel;
		private System.Windows.Forms.ComboBox filterComboBox;
		private System.Windows.Forms.Button ResetFilterButton;
		private System.Windows.Forms.NumericUpDown MinRelevanceNum;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button LaunchURLButton;
		private System.Windows.Forms.Panel bottomPanel;
		private System.Windows.Forms.ContextMenu menuFileList;
		private System.Windows.Forms.MenuItem mnuCheckWithoutImages;
		private System.Windows.Forms.MenuItem mnuCheckWithoutOverview;
		private System.ComponentModel.IContainer components;



		public AppItem CurApp
		{
			get{ return m_CurApp; }
			set{ m_CurApp = value; }
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
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
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
			this.label1 = new System.Windows.Forms.Label();
			this.MinRelevanceNum = new System.Windows.Forms.NumericUpDown();
			this.ResetFilterButton = new System.Windows.Forms.Button();
			this.filterComboBox = new System.Windows.Forms.ComboBox();
			this.filterLabel = new System.Windows.Forms.Label();
			this.buttonSelectBestMatch = new System.Windows.Forms.Button();
			this.allGameLink = new System.Windows.Forms.LinkLabel();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnSaveSearch = new System.Windows.Forms.Button();
			this.btnStartSearch = new System.Windows.Forms.Button();
			this.leftPanel = new System.Windows.Forms.Panel();
			this.uncheckAllButton = new System.Windows.Forms.Button();
			this.checkAllButton = new System.Windows.Forms.Button();
			this.lblFiles = new System.Windows.Forms.Label();
			this.FileList = new System.Windows.Forms.ListView();
			this.FileTitle = new System.Windows.Forms.ColumnHeader();
			this.status = new System.Windows.Forms.ColumnHeader();
			this.splitterVert = new System.Windows.Forms.Splitter();
			this.rightPanel = new System.Windows.Forms.Panel();
			this.LaunchURLButton = new System.Windows.Forms.Button();
			this.lblMatches = new System.Windows.Forms.Label();
			this.MatchList = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.menuFileList = new System.Windows.Forms.ContextMenu();
			this.mnuCheckWithoutImages = new System.Windows.Forms.MenuItem();
			this.mnuCheckWithoutOverview = new System.Windows.Forms.MenuItem();
			this.bottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MinRelevanceNum)).BeginInit();
			this.leftPanel.SuspendLayout();
			this.rightPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// bottomPanel
			// 
			this.bottomPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
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
			this.bottomPanel.Location = new System.Drawing.Point(0, 429);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = new System.Drawing.Size(752, 96);
			this.bottomPanel.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(343, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 24);
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
			this.MinRelevanceNum.Location = new System.Drawing.Point(431, 14);
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
			this.ResetFilterButton.Location = new System.Drawing.Point(287, 13);
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
			this.filterComboBox.Location = new System.Drawing.Point(72, 13);
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
			this.filterLabel.Location = new System.Drawing.Point(8, 16);
			this.filterLabel.Name = "filterLabel";
			this.filterLabel.Size = new System.Drawing.Size(80, 16);
			this.filterLabel.TabIndex = 21;
			this.filterLabel.Text = "Platform:";
			// 
			// buttonSelectBestMatch
			// 
			this.buttonSelectBestMatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonSelectBestMatch.Enabled = false;
			this.buttonSelectBestMatch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonSelectBestMatch.Location = new System.Drawing.Point(168, 52);
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
			this.allGameLink.Location = new System.Drawing.Point(612, 8);
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
			this.btnCancel.Location = new System.Drawing.Point(668, 60);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Close";
			this.btnCancel.Click += new System.EventHandler(this.button3_Click);
			// 
			// btnSaveSearch
			// 
			this.btnSaveSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnSaveSearch.Enabled = false;
			this.btnSaveSearch.Location = new System.Drawing.Point(328, 52);
			this.btnSaveSearch.Name = "btnSaveSearch";
			this.btnSaveSearch.Size = new System.Drawing.Size(160, 32);
			this.btnSaveSearch.TabIndex = 1;
			this.btnSaveSearch.Text = "3) Download && Save Details";
			this.toolTip1.SetToolTip(this.btnSaveSearch, "Download selected matches and save results to MediaPortal!");
			this.btnSaveSearch.Click += new System.EventHandler(this.btnSaveSearch_Click);
			// 
			// btnStartSearch
			// 
			this.btnStartSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnStartSearch.Enabled = false;
			this.btnStartSearch.Location = new System.Drawing.Point(8, 52);
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
			this.leftPanel.Size = new System.Drawing.Size(360, 429);
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
			this.FileList.Location = new System.Drawing.Point(8, 40);
			this.FileList.Name = "FileList";
			this.FileList.Size = new System.Drawing.Size(344, 375);
			this.FileList.TabIndex = 13;
			this.FileList.View = System.Windows.Forms.View.Details;
			this.FileList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FileList_MouseUp);
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
			// splitterVert
			// 
			this.splitterVert.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.splitterVert.Location = new System.Drawing.Point(360, 0);
			this.splitterVert.Name = "splitterVert";
			this.splitterVert.Size = new System.Drawing.Size(5, 429);
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
			this.rightPanel.Size = new System.Drawing.Size(387, 429);
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
			this.MatchList.Size = new System.Drawing.Size(358, 375);
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
			// FileInfoScraperForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(752, 525);
			this.Controls.Add(this.rightPanel);
			this.Controls.Add(this.splitterVert);
			this.Controls.Add(this.leftPanel);
			this.Controls.Add(this.bottomPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FileInfoScraperForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Search fileinfo";
			this.bottomPanel.ResumeLayout(false);
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
			if (m_CurApp == null) return;

			FileList.BeginUpdate();
			try
			{
				FileList.Items.Clear();

				// add all files
				foreach(FileItem file in m_CurApp.Files)
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

		private void button3_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void btnStartSearch_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem curItem in FileList.CheckedItems)
			{
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
					curItem.SubItems[1].Text = String.Format("searching...");
					curItem.Font = new Font(curItem.Font, curItem.Font.Style | FontStyle.Bold);
					Application.DoEvents();
					file.FindFileInfo(myProgScraperType.ALLGAME);
					curItem.SubItems[1].Text = String.Format("{0} matches", file.FileInfoList.Count);
//					curItem.Font = new Font(curItem.Font, curItem.Font.Style | FontStyle.Bold);
					Application.DoEvents();
				}
			}
			ChangeFileSelection();
			buttonSelectBestMatch.Enabled = true;
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

		private void FileList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
				ChangeFileSelection();
		}

		private void allGameLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if(allGameLink.Text==null)
				return;
			if(allGameLink.Text.Length>0)
			{
				System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(allGameLink.Text);
				System.Diagnostics.Process.Start(sInfo);
			}
		}

		private void LaunchSelectedMatchURL()
		{
			FileInfo info = GetSelectedMatchItem();
			if (info == null) return;
			info.LaunchURL();
		}


		private void MatchList_DoubleClick(object sender, System.EventArgs e)
		{
			LaunchSelectedMatchURL();
		}

		private void checkAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem curItem in FileList.Items)
			{
				curItem.Checked = true;
			}
			btnStartSearch.Enabled = (FileList.CheckedItems.Count > 0);
			SyncFileLabel();
		}

		private void uncheckAllButton_Click(object sender, System.EventArgs e)
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

		private void MatchList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			FileItem file = GetSelectedFileItem();
			if (file == null) return;

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

		private void btnSaveSearch_Click(object sender, System.EventArgs e)
		{
			ListViewItem nextItem = null;
			foreach (ListViewItem curItem in FileList.CheckedItems)
			{
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
					if (file.FileInfoFavourite != null)
					{
						curItem.SubItems[1].Text = String.Format("<searching...>");
						Application.DoEvents();
						file.FindFileInfoDetail(m_CurApp, file.FileInfoFavourite, myProgScraperType.ALLGAME);
						file.SaveFromFileInfoFavourite();
						curItem.SubItems[1].Text = String.Format("<saved>");
						Application.DoEvents();
					}
				}
			}
		}

		private void filterComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ChangeFileSelection();
		}

		private void filterComboBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				ChangeFileSelection();
			}
		}

		private void ResetFilterButton_Click(object sender, System.EventArgs e)
		{
			filterComboBox.Text = "";
			ChangeFileSelection();
		}

		private void buttonSelectBestMatch_Click(object sender, System.EventArgs e)
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

		private void MinRelevanceNum_ValueChanged(object sender, System.EventArgs e)
		{
			ChangeFileSelection();
		}

		private void LaunchURLButton_Click(object sender, System.EventArgs e)
		{
			LaunchSelectedMatchURL();
		}

		private void FileList_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			btnStartSearch.Enabled = (FileList.CheckedItems.Count > 0);
			if (!btnStartSearch.Enabled)
			{
				buttonSelectBestMatch.Enabled = false;
				btnSaveSearch.Enabled = false;
			}
			SyncFileLabel();
		}

		private void mnuCheckWithoutImages_Click(object sender, System.EventArgs e)
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

		private void mnuCheckWithoutOverview_Click(object sender, System.EventArgs e)
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


	}
}
