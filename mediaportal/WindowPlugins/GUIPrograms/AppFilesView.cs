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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for AppFilesView.
  /// </summary>
  public class AppFilesView : UserControl
  {
    private IContainer components;
    private ToolTip toolTip;
    private ImageList smallListImages;
    private Panel topPanel;
    private Button backButton;
    private Panel bottomPanel;
    private ListView fileList;
    private ColumnHeader fileTitle;
    private Button btnLaunch;
    private Button updateDatabaseButton;
    private Button btnEdit;
    private Button btnDelete;
    private Button btnNew;
    private Label filePathLabel;
    private Button btnAddToFavourites;
    private ContextMenu popupFavourites;
    private MenuItem menuItem1;
    private MenuItem menuItem2;

    private AppItem mCurApp = null;
    private Button startScraperButton;
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
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.btnLaunch = new MediaPortal.UserInterface.Controls.MPButton();
      this.updateDatabaseButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnEdit = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnDelete = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnNew = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnAddToFavourites = new MediaPortal.UserInterface.Controls.MPButton();
      this.startScraperButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.topPanel = new System.Windows.Forms.Panel();
      this.backButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.filePathLabel = new System.Windows.Forms.Label();
      this.bottomPanel = new System.Windows.Forms.Panel();
      this.fileList = new System.Windows.Forms.ListView();
      this.fileTitle = new System.Windows.Forms.ColumnHeader();
      this.popupFavourites = new System.Windows.Forms.ContextMenu();
      this.menuItem1 = new System.Windows.Forms.MenuItem();
      this.menuItem2 = new System.Windows.Forms.MenuItem();
      this.topPanel.SuspendLayout();
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
      this.toolTip.SetToolTip(this.btnLaunch, "Launch selected fileitem now for testing the settings");
      this.btnLaunch.Click += new System.EventHandler(this.btnLaunch_Click);
      // 
      // updateDatabaseButton
      // 
      this.updateDatabaseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.updateDatabaseButton.Location = new System.Drawing.Point(296, 328);
      this.updateDatabaseButton.Name = "updateDatabaseButton";
      this.updateDatabaseButton.Size = new System.Drawing.Size(88, 40);
      this.updateDatabaseButton.TabIndex = 17;
      this.updateDatabaseButton.Text = "Update Database";
      this.toolTip.SetToolTip(this.updateDatabaseButton, "Import the sourcefile");
      this.updateDatabaseButton.Click += new System.EventHandler(this.UpdateDatabaseButton_Click);
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
      this.toolTip.SetToolTip(this.btnEdit, "Edit a file item");
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
      this.toolTip.SetToolTip(this.btnDelete, "removes a file / filelink from the database");
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
      this.toolTip.SetToolTip(this.btnNew, "Add a new fileitem");
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
      this.toolTip.SetToolTip(this.btnAddToFavourites, "Adds the selected files to a grouper item");
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
      this.toolTip.SetToolTip(this.startScraperButton, "removes a file / filelink from the database");
      this.startScraperButton.Click += new System.EventHandler(this.startScraperButton_Click);
      // 
      // topPanel
      // 
      this.topPanel.Controls.Add(this.backButton);
      this.topPanel.Controls.Add(this.filePathLabel);
      this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.topPanel.Location = new System.Drawing.Point(0, 0);
      this.topPanel.Name = "topPanel";
      this.topPanel.Size = new System.Drawing.Size(392, 32);
      this.topPanel.TabIndex = 14;
      // 
      // backButton
      // 
      this.backButton.Enabled = false;
      this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.backButton.Image = ((System.Drawing.Image)(resources.GetObject("backButton.Image")));
      this.backButton.Location = new System.Drawing.Point(0, 0);
      this.backButton.Name = "backButton";
      this.backButton.Size = new System.Drawing.Size(32, 32);
      this.backButton.TabIndex = 1;
      this.backButton.Click += new System.EventHandler(this.BackButton_Click);
      // 
      // filePathLabel
      // 
      this.filePathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.filePathLabel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.filePathLabel.Location = new System.Drawing.Point(36, 8);
      this.filePathLabel.Name = "filePathLabel";
      this.filePathLabel.Size = new System.Drawing.Size(344, 16);
      this.filePathLabel.TabIndex = 0;
      this.filePathLabel.Text = "Filepath:";
      this.filePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // bottomPanel
      // 
      this.bottomPanel.Controls.Add(this.startScraperButton);
      this.bottomPanel.Controls.Add(this.btnAddToFavourites);
      this.bottomPanel.Controls.Add(this.btnLaunch);
      this.bottomPanel.Controls.Add(this.updateDatabaseButton);
      this.bottomPanel.Controls.Add(this.btnEdit);
      this.bottomPanel.Controls.Add(this.btnDelete);
      this.bottomPanel.Controls.Add(this.btnNew);
      this.bottomPanel.Controls.Add(this.fileList);
      this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.bottomPanel.Location = new System.Drawing.Point(0, 32);
      this.bottomPanel.Name = "bottomPanel";
      this.bottomPanel.Size = new System.Drawing.Size(392, 376);
      this.bottomPanel.TabIndex = 15;
      // 
      // fileList
      // 
      this.fileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.fileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                               this.fileTitle});
      this.fileList.FullRowSelect = true;
      this.fileList.HideSelection = false;
      this.fileList.Location = new System.Drawing.Point(0, 0);
      this.fileList.Name = "fileList";
      this.fileList.Size = new System.Drawing.Size(288, 376);
      this.fileList.SmallImageList = this.smallListImages;
      this.fileList.TabIndex = 12;
      this.fileList.View = System.Windows.Forms.View.Details;
      this.fileList.DoubleClick += new System.EventHandler(this.FileList_DoubleClick);
      this.fileList.SelectedIndexChanged += new System.EventHandler(this.FileList_SelectedIndexChanged);
      // 
      // fileTitle
      // 
      this.fileTitle.Text = "Title";
      this.fileTitle.Width = 280;
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
      this.Controls.Add(this.topPanel);
      this.Name = "AppFilesView";
      this.Size = new System.Drawing.Size(392, 408);
      this.topPanel.ResumeLayout(false);
      this.bottomPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion 

    public void Refresh(AppItem curApp)
    {
      if (curApp != null)
      {
        mCurApp = curApp;
        if (!mCurApp.filesAreLoaded)
        {
          mCurApp.LoadFiles();
        }
        if (!mCurApp.linksAreLoaded)
        {
          mCurApp.LoadFileLinks();
        }
        SyncListView();
      }
    }

    private void SyncListView()
    {
      if (mCurApp == null)
        return;

      fileList.BeginUpdate();
      try
      {
        fileList.Items.Clear();

        // add all files
        foreach (FileItem file in mCurApp.Files)
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


          fileList.Items.Add(curItem);
        }

        // add all filelinks
        foreach (FileItem filelink in mCurApp.Filelinks)
        {
          ListViewItem curItem = new ListViewItem(filelink.Title);
          curItem.Tag = filelink;
          curItem.ImageIndex = 2;
          fileList.Items.Add(curItem);
        }


      }
      finally
      {
        fileList.EndUpdate();
      }
      SyncListViewButtons();
      SyncFilePath();
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
      EditItem();
    }

    private void btnNew_Click(object sender, EventArgs e)
    {
      AddItem();
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
      DeleteItems();
    }

    private void FileList_DoubleClick(object sender, EventArgs e)
    {
      if (fileList.SelectedItems.Count == 1)
      {
        FileItem file = (FileItem) fileList.SelectedItems[0].Tag;
        if (file != null)
        {
          if (file.IsFolder)
          {
            ChangeFilePath(file.Filename); // filename becomes filepath in next view... :)
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
      DialogResult dialogResult = frmFileDetails.ShowDialog(this);
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
      if (fileList.SelectedItems.Count == 1)
      {
        FileItem file = (FileItem) fileList.SelectedItems[0].Tag;
        if (file != null)
        {
          FileDetailsForm frmFileDetails = new FileDetailsForm();
          frmFileDetails.CurApp = mCurApp;
          frmFileDetails.CurFile = file;
          DialogResult dialogResult = frmFileDetails.ShowDialog(this);
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
      if (fileList.SelectedItems.Count == 1)
      {
        FileItem file = (FileItem) fileList.SelectedItems[0].Tag;
        if (file != null)
        {
          mCurApp.LaunchFile(file, false); //launch in non-blocking mode
          if ((mCurApp.LaunchErrorMsg != null) && (mCurApp.LaunchErrorMsg != ""))
          {
            MessageBox.Show(mCurApp.LaunchErrorMsg, "Launch Error: (check LOG for details)");
          }
        }
      }
    }


    private void DeleteItems()
    {
      if (fileList.SelectedItems.Count >= 1)
      {
        DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the selected item(s)?", "Information", MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);
        if (dialogResult == DialogResult.Yes)
        {
          foreach (ListViewItem curItem in fileList.SelectedItems)
            if (curItem.Tag is FilelinkItem)
            {
              FilelinkItem filelink = (FilelinkItem) curItem.Tag;
              if (filelink != null)
              {
                filelink.Delete();
              }
            }
            else
            {
              FileItem file = (FileItem) curItem.Tag;
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

    private void FileList_SelectedIndexChanged(object sender, EventArgs e)
    {
      SyncListViewButtons();
    }

    private void SyncListViewButtons()
    {
      backButton.Enabled = false;
      btnNew.Enabled = false;
      btnEdit.Enabled = false;
      btnDelete.Enabled = false;
      btnLaunch.Enabled = false;
      btnAddToFavourites.Enabled = false;

      if (fileList.SelectedItems.Count == 1)
      {
        FileItem file = (FileItem) fileList.SelectedItems[0].Tag;
        if (file != null)
        {
          if (!file.IsFolder)
          {
            btnEdit.Enabled = true;
            btnDelete.Enabled = true;
            btnLaunch.Enabled = (file.Filename != "");
          }
        }
      }
      else if (fileList.SelectedItems.Count > 1)
      {
        btnDelete.Enabled = true;
      }


      if (mCurApp != null)
      {
        btnNew.Enabled = mCurApp.FileAddAllowed();
        btnAddToFavourites.Enabled = mCurApp.FilesCanBeFavourites() && (fileList.SelectedItems.Count > 0);
        updateDatabaseButton.Visible = mCurApp.RefreshButtonVisible();
        if (mCurApp.FileBrowseAllowed())
        {
          topPanel.Visible = true;
          backButton.Enabled = (mCurApp.CurrentFilePath() != mCurApp.FileDirectory);
        }
        else
        {
          topPanel.Visible = false;
        }
      }
    }

    private void SyncFilePath()
    {
      filePathLabel.Text = mCurApp.CurrentFilePath().Replace("&", "&&");
    }


    private void UpdateDatabaseButton_Click(object sender, EventArgs e)
    {
      if (this.OnRefreshClick != null)
      {
        updateDatabaseButton.Enabled = false;
        try
        {
          OnRefreshClick(this, null);
        }
        finally
        {
          SyncListView();
          updateDatabaseButton.Enabled = true;
        }
      }
    }

    private void btnLaunch_Click(object sender, EventArgs e)
    {
      LaunchItem();
    }

    private void BackButton_Click(object sender, EventArgs e)
    {
      if (mCurApp == null)
      {
        return;
      }
      if (mCurApp.CurrentFilePath() != mCurApp.FileDirectory)
      {
        // it's ok to go one level up!
        string strNewPath = Path.GetDirectoryName(mCurApp.CurrentFilePath());
        ChangeFilePath(strNewPath);
      }
    }

    private void btnAddToFavourites_Click(object sender, EventArgs e)
    {
      popupFavourites.Show(btnAddToFavourites, new Point(0, btnAddToFavourites.Height));
    }

    private void FavouriteGrouperItem_Click(object sender, EventArgs e)
    {
      if (mCurApp == null)
      {
        return;
      }
      int GrouperAppID = ((taggedMenuItem) sender).Tag;
      foreach (ListViewItem curItem in fileList.SelectedItems)
      {
        FileItem curFile = (FileItem) curItem.Tag;
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

    private void popupFavourites_Popup(object sender, EventArgs e)
    {
      popupFavourites.MenuItems.Clear();
      foreach (AppItem app in apps)
      {
        if (app.SourceType == myProgSourceType.GROUPER)
        {
          taggedMenuItem newMenu = new taggedMenuItem(app.Title);
          newMenu.Tag = app.AppID;
          newMenu.Click += new EventHandler(this.FavouriteGrouperItem_Click);
          popupFavourites.MenuItems.Add(newMenu);
        }
      }
    }

    private void startScraperButton_Click(object sender, EventArgs e)
    {
      ShowFileScraper();
    }

    void ShowFileScraper()
    {
      if (mCurApp.FirstImageDirectoryValid())
      {
        FileInfoScraperForm frmScraper = new FileInfoScraperForm();
        frmScraper.CurApp = mCurApp;
        frmScraper.Setup();
        frmScraper.ShowDialog(this);
        SyncListView();
      }
      else
      {
        MessageBox.Show("Please set the first imagedirectory before starting the scraper!", "Missing or Invalid Imagedirectory");
      }
    }


  
  
  }
}