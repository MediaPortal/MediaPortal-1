using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Programs.Utils;
using ProgramsDatabase;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for AppFilesView.
	/// </summary>
	public class AppFilesView : System.Windows.Forms.UserControl
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ToolTip ToolTip;
		private System.Windows.Forms.ImageList smallListImages;
		private System.Windows.Forms.Panel TopPanel;
		private System.Windows.Forms.Button BackButton;
		private System.Windows.Forms.Panel bottomPanel;
		private System.Windows.Forms.ListView FileList;
		private System.Windows.Forms.ColumnHeader FileTitle;
		private System.Windows.Forms.Button btnLaunch;
		private System.Windows.Forms.Button UpdateDatabaseButton;
		private System.Windows.Forms.Button btnEdit;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.Button btnNew;
		private System.Windows.Forms.Label FilePathLabel;
		private System.Windows.Forms.Button btnAddToFavourites;
		private System.Windows.Forms.ContextMenu popupFavourites;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;

		private AppItem mCurApp = null;
		private System.Windows.Forms.Button startScraperButton; 
		private Applist apps = ProgramDatabase.AppList;
		public event EventHandler OnRefreshClick;

		public AppFilesView()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AppFilesView));
			this.smallListImages = new System.Windows.Forms.ImageList(this.components);
			this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.btnLaunch = new System.Windows.Forms.Button();
			this.UpdateDatabaseButton = new System.Windows.Forms.Button();
			this.btnEdit = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.btnNew = new System.Windows.Forms.Button();
			this.btnAddToFavourites = new System.Windows.Forms.Button();
			this.startScraperButton = new System.Windows.Forms.Button();
			this.TopPanel = new System.Windows.Forms.Panel();
			this.BackButton = new System.Windows.Forms.Button();
			this.FilePathLabel = new System.Windows.Forms.Label();
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.FileList = new System.Windows.Forms.ListView();
			this.FileTitle = new System.Windows.Forms.ColumnHeader();
			this.popupFavourites = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.TopPanel.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// smallListImages
			// 
			this.smallListImages.ImageSize = new System.Drawing.Size(16, 16);
			this.smallListImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("smallListImages.ImageStream")));
			this.smallListImages.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// btnLaunch
			// 
			this.btnLaunch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnLaunch.Enabled = false;
			this.btnLaunch.Location = new System.Drawing.Point(296, 88);
			this.btnLaunch.Name = "btnLaunch";
			this.btnLaunch.Size = new System.Drawing.Size(88, 23);
			this.btnLaunch.TabIndex = 18;
			this.btnLaunch.Text = "Launch";
			this.ToolTip.SetToolTip(this.btnLaunch, "Launch selected fileitem now for testing the settings");
			this.btnLaunch.Click += new System.EventHandler(this.btnLaunch_Click);
			// 
			// UpdateDatabaseButton
			// 
			this.UpdateDatabaseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateDatabaseButton.Location = new System.Drawing.Point(296, 328);
			this.UpdateDatabaseButton.Name = "UpdateDatabaseButton";
			this.UpdateDatabaseButton.Size = new System.Drawing.Size(88, 40);
			this.UpdateDatabaseButton.TabIndex = 17;
			this.UpdateDatabaseButton.Text = "Update Database";
			this.ToolTip.SetToolTip(this.UpdateDatabaseButton, "Import the sourcefile");
			this.UpdateDatabaseButton.Click += new System.EventHandler(this.UpdateDatabaseButton_Click);
			// 
			// btnEdit
			// 
			this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnEdit.Enabled = false;
			this.btnEdit.Location = new System.Drawing.Point(296, 24);
			this.btnEdit.Name = "btnEdit";
			this.btnEdit.Size = new System.Drawing.Size(88, 23);
			this.btnEdit.TabIndex = 16;
			this.btnEdit.Text = "&Edit...";
			this.ToolTip.SetToolTip(this.btnEdit, "Edit a file item");
			this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
			// 
			// btnDelete
			// 
			this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDelete.Enabled = false;
			this.btnDelete.Location = new System.Drawing.Point(296, 48);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(88, 23);
			this.btnDelete.TabIndex = 15;
			this.btnDelete.Text = "&Delete...";
			this.ToolTip.SetToolTip(this.btnDelete, "removes a file / filelink from the database");
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// btnNew
			// 
			this.btnNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnNew.Location = new System.Drawing.Point(296, 0);
			this.btnNew.Name = "btnNew";
			this.btnNew.Size = new System.Drawing.Size(88, 23);
			this.btnNew.TabIndex = 14;
			this.btnNew.Text = "&New...";
			this.ToolTip.SetToolTip(this.btnNew, "Add a new fileitem");
			this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
			// 
			// btnAddToFavourites
			// 
			this.btnAddToFavourites.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAddToFavourites.Enabled = false;
			this.btnAddToFavourites.Location = new System.Drawing.Point(296, 248);
			this.btnAddToFavourites.Name = "btnAddToFavourites";
			this.btnAddToFavourites.Size = new System.Drawing.Size(88, 40);
			this.btnAddToFavourites.TabIndex = 19;
			this.btnAddToFavourites.Text = "Add to Favourites...";
			this.ToolTip.SetToolTip(this.btnAddToFavourites, "Adds the selected files to a grouper item");
			this.btnAddToFavourites.Click += new System.EventHandler(this.btnAddToFavourites_Click);
			// 
			// startScraperButton
			// 
			this.startScraperButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.startScraperButton.Location = new System.Drawing.Point(296, 288);
			this.startScraperButton.Name = "startScraperButton";
			this.startScraperButton.Size = new System.Drawing.Size(88, 40);
			this.startScraperButton.TabIndex = 20;
			this.startScraperButton.Text = "Internet Search...";
			this.ToolTip.SetToolTip(this.startScraperButton, "removes a file / filelink from the database");
			this.startScraperButton.Click += new System.EventHandler(this.startScraperButton_Click);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.BackButton);
			this.TopPanel.Controls.Add(this.FilePathLabel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = new System.Drawing.Point(0, 0);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = new System.Drawing.Size(392, 32);
			this.TopPanel.TabIndex = 14;
			// 
			// BackButton
			// 
			this.BackButton.Enabled = false;
			this.BackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.BackButton.Image = ((System.Drawing.Image)(resources.GetObject("BackButton.Image")));
			this.BackButton.Location = new System.Drawing.Point(0, 0);
			this.BackButton.Name = "BackButton";
			this.BackButton.Size = new System.Drawing.Size(32, 32);
			this.BackButton.TabIndex = 1;
			this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
			// 
			// FilePathLabel
			// 
			this.FilePathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.FilePathLabel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FilePathLabel.Location = new System.Drawing.Point(36, 8);
			this.FilePathLabel.Name = "FilePathLabel";
			this.FilePathLabel.Size = new System.Drawing.Size(344, 16);
			this.FilePathLabel.TabIndex = 0;
			this.FilePathLabel.Text = "Filepath:";
			this.FilePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.startScraperButton);
			this.bottomPanel.Controls.Add(this.btnAddToFavourites);
			this.bottomPanel.Controls.Add(this.btnLaunch);
			this.bottomPanel.Controls.Add(this.UpdateDatabaseButton);
			this.bottomPanel.Controls.Add(this.btnEdit);
			this.bottomPanel.Controls.Add(this.btnDelete);
			this.bottomPanel.Controls.Add(this.btnNew);
			this.bottomPanel.Controls.Add(this.FileList);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bottomPanel.Location = new System.Drawing.Point(0, 32);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = new System.Drawing.Size(392, 376);
			this.bottomPanel.TabIndex = 15;
			// 
			// FileList
			// 
			this.FileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.FileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																							 this.FileTitle});
			this.FileList.FullRowSelect = true;
			this.FileList.HideSelection = false;
			this.FileList.Location = new System.Drawing.Point(0, 0);
			this.FileList.Name = "FileList";
			this.FileList.Size = new System.Drawing.Size(288, 376);
			this.FileList.SmallImageList = this.smallListImages;
			this.FileList.TabIndex = 12;
			this.FileList.View = System.Windows.Forms.View.Details;
			this.FileList.DoubleClick += new System.EventHandler(this.FileList_DoubleClick);
			this.FileList.SelectedIndexChanged += new System.EventHandler(this.FileList_SelectedIndexChanged);
			// 
			// FileTitle
			// 
			this.FileTitle.Text = "Title";
			this.FileTitle.Width = 280;
			// 
			// popupFavourites
			// 
			this.popupFavourites.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																										this.menuItem1,
																																										this.menuItem2});
			this.popupFavourites.Popup += new System.EventHandler(this.popupFavourites_Popup);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "Grouper A";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.Text = "Grouper B";
			// 
			// AppFilesView
			// 
			this.Controls.Add(this.bottomPanel);
			this.Controls.Add(this.TopPanel);
			this.Name = "AppFilesView";
			this.Size = new System.Drawing.Size(392, 408);
			this.TopPanel.ResumeLayout(false);
			this.bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		public void Refresh(AppItem curApp)
		{
			if (curApp != null)
			{
				mCurApp = curApp;
				mCurApp.Files.Load(mCurApp.AppID, "");
				mCurApp.Filelinks.Load(mCurApp.AppID, "");
				SyncListView();
			}
		}

		private void SyncListView()
		{
			if (mCurApp == null) return;

			FileList.BeginUpdate();
			try
			{
				FileList.Items.Clear();

				// add all files
				foreach(FileItem file in mCurApp.Files)
				{
					ListViewItem curItem = new ListViewItem(file.Title);
					curItem.Tag = file;
					if (file.IsFolder)
					{
						curItem.ImageIndex = 0;
					}
					else
					{
						curItem.ImageIndex = 1;
					}


					FileList.Items.Add(curItem);
				}

				// add all filelinks
				foreach(FileItem filelink in mCurApp.Filelinks)
				{
					ListViewItem curItem = new ListViewItem(filelink.Title);
					curItem.Tag = filelink;
					curItem.ImageIndex = 2;
					FileList.Items.Add(curItem);
				}



			}
			finally
			{
				FileList.EndUpdate();
			}
			SyncListViewButtons();
			SyncFilePath();
		}

		private void btnEdit_Click(object sender, System.EventArgs e)
		{
			EditItem();
		}

		private void btnNew_Click(object sender, System.EventArgs e)
		{
			AddItem();
		}

		private void btnDelete_Click(object sender, System.EventArgs e)
		{
			DeleteItems();
		}

		private void FileList_DoubleClick(object sender, System.EventArgs e)
		{
			if (FileList.SelectedItems.Count == 1)
			{
				FileItem file = (FileItem)FileList.SelectedItems[0].Tag;
				if (file != null)
				{
					if (file.IsFolder)
					{
						ChangeFilePath(file.Filename);// filename becomes filepath in next view... :)
					}
					else
					{
						EditItem();
					}
				}
			}
		}

		private void AddItem()
		{
			FileItem file = new FileItem(mCurApp.db);
			file.AppID = mCurApp.AppID; // CRUCIAL!! :-)
			FileDetailsForm frmFileDetails = new FileDetailsForm();
			frmFileDetails.CurApp = mCurApp;
			frmFileDetails.CurFile = file;
			DialogResult dialogResult = frmFileDetails.ShowDialog( this );
			if (dialogResult == DialogResult.OK)
			{
				file.Write();
				mCurApp.Files.Load(mCurApp.AppID, "");
				mCurApp.Filelinks.Load(mCurApp.AppID, "");
				SyncListView();
			}
		}

		private void ChangeFilePath(string NewPath)
		{
			mCurApp.Files.Load(mCurApp.AppID, NewPath);
			mCurApp.Filelinks.Load(mCurApp.AppID, "");
			SyncListView();
		}

		private void EditItem()
		{
			if (FileList.SelectedItems.Count == 1)
			{
				FileItem file = (FileItem)FileList.SelectedItems[0].Tag;
				if (file != null)
				{
					FileDetailsForm frmFileDetails = new FileDetailsForm();
					frmFileDetails.CurApp = mCurApp;
					frmFileDetails.CurFile = file;
					DialogResult dialogResult = frmFileDetails.ShowDialog( this );
					if (dialogResult == DialogResult.OK)
					{
						file.Write();
						mCurApp.Files.Load(mCurApp.AppID, "");
						mCurApp.Filelinks.Load(mCurApp.AppID, "");
						SyncListView();
					}
				}

			}
		}

		private void LaunchItem()
		{
			if (FileList.SelectedItems.Count == 1)
			{
				FileItem file = (FileItem)FileList.SelectedItems[0].Tag;
				if (file != null)
				{
					mCurApp.LaunchFile(file, false); //launch in non-blocking mode
					if ((mCurApp.LaunchErrorMsg != null) && (mCurApp.LaunchErrorMsg != ""))
					{
						System.Windows.Forms.MessageBox.Show(mCurApp.LaunchErrorMsg, "Launch Error: (check LOG for details)");
					}
				}
			}
		}


		private void DeleteItems()
		{
			if (FileList.SelectedItems.Count >= 1)
			{
				DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the selected item(s)?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dialogResult == DialogResult.Yes)
				{
					foreach (ListViewItem curItem in FileList.SelectedItems)
					if (curItem.Tag is FilelinkItem)
					{
						FilelinkItem filelink = (FilelinkItem)curItem.Tag;
						if (filelink != null)
						{
							filelink.Delete();
						}
					}
					else
					{
						FileItem file = (FileItem)curItem.Tag;
						if (file != null)
						{
							file.Delete();
						}
					}
				}
			}
			mCurApp.Files.Load(mCurApp.AppID, "");
			mCurApp.Filelinks.Load(mCurApp.AppID, "");
			SyncListView();
		}

		private void FileList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SyncListViewButtons();
		}

		private void SyncListViewButtons()
		{
			BackButton.Enabled = false;
			btnNew.Enabled = false;
			btnEdit.Enabled = false;
			btnDelete.Enabled = false;
			btnLaunch.Enabled = false;
			btnAddToFavourites.Enabled = false;

			if (FileList.SelectedItems.Count == 1)
			{
				FileItem file = (FileItem)FileList.SelectedItems[0].Tag;
				if (file != null)
				{
					if (!file.IsFolder)
					{
						btnEdit.Enabled = true;
						btnDelete.Enabled = true;
						btnLaunch.Enabled = true;
					}
				}
			}
			else if (FileList.SelectedItems.Count > 1)
			{
				btnDelete.Enabled = true;
			}



			if (mCurApp != null)
			{
				btnNew.Enabled = mCurApp.FileAddAllowed();
				btnAddToFavourites.Enabled = mCurApp.FilesCanBeFavourites()
					                         && (FileList.SelectedItems.Count > 0);
				UpdateDatabaseButton.Visible = mCurApp.RefreshButtonVisible();
				if (mCurApp.FileBrowseAllowed())
				{
					TopPanel.Visible = true;
					BackButton.Enabled =(mCurApp.CurrentFilePath() != mCurApp.FileDirectory);
				}
				else
				{
					TopPanel.Visible = false;
				}
			}
		}

		private void SyncFilePath()
		{
			FilePathLabel.Text = mCurApp.CurrentFilePath().Replace("&", "&&");
		}


		private void UpdateDatabaseButton_Click(object sender, System.EventArgs e)
		{
			if (this.OnRefreshClick != null)
			{
				UpdateDatabaseButton.Enabled = false;
				try
				{
					OnRefreshClick(this, null);
				}
				finally
				{
					SyncListView();
					UpdateDatabaseButton.Enabled = true;
				}
			}
		}

		private void btnLaunch_Click(object sender, System.EventArgs e)
		{
			LaunchItem();
		}

		private void BackButton_Click(object sender, System.EventArgs e)
		{
			if (mCurApp == null) { return; }
			if (mCurApp.CurrentFilePath() != mCurApp.FileDirectory)
			{
				// it's ok to go one level up!
				string strNewPath = System.IO.Path.GetDirectoryName(mCurApp.CurrentFilePath());
				ChangeFilePath(strNewPath);
			}
		}

		private void btnAddToFavourites_Click(object sender, System.EventArgs e)
		{
			popupFavourites.Show(btnAddToFavourites, new System.Drawing.Point(0, btnAddToFavourites.Height));
		}

		private void FavouriteGrouperItem_Click(object sender, System.EventArgs e)
		{
			if (mCurApp == null) { return; }
			int GrouperAppID = ((taggedMenuItem)sender).Tag;
			foreach (ListViewItem curItem in FileList.SelectedItems)
			{
				FileItem curFile = (FileItem)curItem.Tag;
				FilelinkItem newLink = new FilelinkItem(mCurApp.db);
				// example: "add the 'MAME' game 'r-type' to the 'top 20 shooters'"
				//          'MAME' :           targetAppID
				//          'r-type':          fileid
				//          'top 20 shooters': GrouperAppID; 
				newLink.AppID = GrouperAppID; // the app where the link belongs
				newLink.FileID = curFile.FileID;
				newLink.TargetAppID = curFile.AppID; // the app where the launch will effectively happen....
				newLink.Filename = curFile.Filename;
				newLink.Write();
//				Log.Write("Add to Favourites groupAppID:{0} Title:{1} fileID:{2} appID:{3}", GrouperAppID, curFile.Title, curFile.FileID, curFile.AppID);
			}
		}

		private void popupFavourites_Popup(object sender, System.EventArgs e)
		{
			popupFavourites.MenuItems.Clear();
			foreach(AppItem app in apps)
			{
				if (app.SourceType == myProgSourceType.GROUPER)
				{
					taggedMenuItem newMenu = new taggedMenuItem(app.Title);
					newMenu.Tag = app.AppID;
					newMenu.Click += new System.EventHandler(this.FavouriteGrouperItem_Click);
					popupFavourites.MenuItems.Add(newMenu);
				}
			}
		}

		private void btnRefresh_Click(object sender, System.EventArgs e)
		{
			SyncListView();
		}

		private void startScraperButton_Click(object sender, System.EventArgs e)
		{
			ShowFileScraper();
		}

		void ShowFileScraper()
		{
			FileInfoScraperForm frmScraper = new FileInfoScraperForm();
			frmScraper.CurApp = mCurApp;
			frmScraper.Setup();
			frmScraper.ShowDialog( this );
			SyncListView();
		}


	}
}
