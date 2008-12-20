#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using SQLite.NET;

using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Database;
using MediaPortal.Music.Database;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class MusicDatabase : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private System.Windows.Forms.CheckedListBox sharesListBox;
    private MediaPortal.UserInterface.Controls.MPButton startButton;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private System.Windows.Forms.ProgressBar progressBar;
    private MediaPortal.UserInterface.Controls.MPLabel fileLabel;
    private System.ComponentModel.IContainer components = null;
    private MediaPortal.UserInterface.Controls.MPCheckBox folderAsAlbumCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox monitorSharesCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxUpdateSinceLastImport;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStripArtistPrefix;
    private MediaPortal.UserInterface.Controls.MPTextBox tbPrefixes;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxUseForThumbs;
    private MediaPortal.UserInterface.Controls.MPCheckBox buildThumbsCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxUseFolderThumb;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxUseAlbumThumbs;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxCreateArtist;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxCreateFolderThumb;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxCreateGenre;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAllImages;

    public class MusicData
    {
      public string FilePath;
      public MusicTag Tag;

      public MusicData(string filePath, MusicTag tag)
      {
        this.FilePath = filePath;
        this.Tag = tag;
      }
    }

    MediaPortal.Music.Database.MusicDatabase m_dbs = MediaPortal.Music.Database.MusicDatabase.Instance;

    public MusicDatabase()
      : this("Music Database")
    {
    }

    public MusicDatabase(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      groupBox2.Enabled = false;
    }

    private string[] Extensions
    {
      get { return extensions; }
      set { extensions = value; }
    }
    string[] extensions = new string[] { ".mp3" };

    public override void OnSectionActivated()
    {
      //
      // Clear any existing entries
      //
      sharesListBox.Items.Clear();

      //
      // Load selected shares
      //
      SectionSettings section = SectionSettings.GetSection("Music Folders");

      if (section != null)
      {
        ArrayList shares = (ArrayList)section.GetSetting("shares");

        foreach (string share in shares)
        {
          //
          // Add to share to list box and default to selected
          //
          sharesListBox.Items.Add(share, CheckState.Checked);
        }
      }

      //
      // Fetch extensions
      //
      section = SectionSettings.GetSection("Music Extensions");

      if (section != null)
      {
        string extensions = (string)section.GetSetting("extensions");
        Extensions = extensions.Split(new char[] { ',' });
      }

      UpdateControlStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        buildThumbsCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "extractthumbs", false);
        checkBoxCreateArtist.Checked = xmlreader.GetValueAsBool("musicfiles", "createartistthumbs", false);
        checkBoxCreateGenre.Checked = xmlreader.GetValueAsBool("musicfiles", "creategenrethumbs", true);
        checkBoxUseFolderThumb.Checked = xmlreader.GetValueAsBool("musicfiles", "useFolderThumbs", true);
        checkBoxAllImages.Checked = xmlreader.GetValueAsBool("musicfiles", "useAllImages", checkBoxUseFolderThumb.Checked);
        folderAsAlbumCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "treatFolderAsAlbum", false);
        checkBoxCreateFolderThumb.Checked = xmlreader.GetValueAsBool("musicfiles", "createMissingFolderThumbs", folderAsAlbumCheckBox.Checked);
        monitorSharesCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "monitorShares", false);
        checkBoxUpdateSinceLastImport.Checked = xmlreader.GetValueAsBool("musicfiles", "updateSinceLastImport", true);
        checkBoxUpdateSinceLastImport.Text = String.Format("Only update new / changed files after {0}", xmlreader.GetValueAsString("musicfiles", "lastImport", "1900-01-01 00:00:00"));
        checkBoxStripArtistPrefix.Checked = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);
        tbPrefixes.Text = xmlreader.GetValueAsString("musicfiles", "artistprefixes", "The, Les, Die");
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("musicfiles", "extractthumbs", buildThumbsCheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "createartistthumbs", checkBoxCreateArtist.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "creategenrethumbs", checkBoxCreateGenre.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "useFolderThumbs", checkBoxUseFolderThumb.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "useAllImages", checkBoxAllImages.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "createMissingFolderThumbs", checkBoxCreateFolderThumb.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "treatFolderAsAlbum", folderAsAlbumCheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "monitorShares", monitorSharesCheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "updateSinceLastImport", checkBoxUpdateSinceLastImport.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "stripartistprefixes", checkBoxStripArtistPrefix.Checked);
        xmlwriter.SetValue("musicfiles", "artistprefixes", tbPrefixes.Text);
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBoxUseAlbumThumbs = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxCreateGenre = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxCreateFolderThumb = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxCreateArtist = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxUseForThumbs = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxAllImages = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxUseFolderThumb = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buildThumbsCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tbPrefixes = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.checkBoxStripArtistPrefix = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxUpdateSinceLastImport = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.monitorSharesCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.folderAsAlbumCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.startButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.sharesListBox = new System.Windows.Forms.CheckedListBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.fileLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.groupBox1.SuspendLayout();
      this.groupBoxUseAlbumThumbs.SuspendLayout();
      this.groupBoxUseForThumbs.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.groupBoxUseAlbumThumbs);
      this.groupBox1.Controls.Add(this.groupBoxUseForThumbs);
      this.groupBox1.Controls.Add(this.tbPrefixes);
      this.groupBox1.Controls.Add(this.checkBoxStripArtistPrefix);
      this.groupBox1.Controls.Add(this.checkBoxUpdateSinceLastImport);
      this.groupBox1.Controls.Add(this.monitorSharesCheckBox);
      this.groupBox1.Controls.Add(this.folderAsAlbumCheckBox);
      this.groupBox1.Controls.Add(this.startButton);
      this.groupBox1.Controls.Add(this.sharesListBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 339);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Scan music shares";
      // 
      // groupBoxUseAlbumThumbs
      // 
      this.groupBoxUseAlbumThumbs.Controls.Add(this.checkBoxCreateGenre);
      this.groupBoxUseAlbumThumbs.Controls.Add(this.checkBoxCreateFolderThumb);
      this.groupBoxUseAlbumThumbs.Controls.Add(this.checkBoxCreateArtist);
      this.groupBoxUseAlbumThumbs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxUseAlbumThumbs.Location = new System.Drawing.Point(246, 165);
      this.groupBoxUseAlbumThumbs.Name = "groupBoxUseAlbumThumbs";
      this.groupBoxUseAlbumThumbs.Size = new System.Drawing.Size(210, 96);
      this.groupBoxUseAlbumThumbs.TabIndex = 14;
      this.groupBoxUseAlbumThumbs.TabStop = false;
      this.groupBoxUseAlbumThumbs.Text = "Use existing album thumbs to:";
      // 
      // checkBoxCreateGenre
      // 
      this.checkBoxCreateGenre.AutoSize = true;
      this.checkBoxCreateGenre.Checked = true;
      this.checkBoxCreateGenre.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxCreateGenre.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxCreateGenre.Location = new System.Drawing.Point(15, 22);
      this.checkBoxCreateGenre.Name = "checkBoxCreateGenre";
      this.checkBoxCreateGenre.Size = new System.Drawing.Size(161, 17);
      this.checkBoxCreateGenre.TabIndex = 7;
      this.checkBoxCreateGenre.Text = "create genre preview thumbs";
      this.checkBoxCreateGenre.UseVisualStyleBackColor = true;
      // 
      // checkBoxCreateFolderThumb
      // 
      this.checkBoxCreateFolderThumb.AutoSize = true;
      this.checkBoxCreateFolderThumb.Enabled = false;
      this.checkBoxCreateFolderThumb.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxCreateFolderThumb.Location = new System.Drawing.Point(15, 68);
      this.checkBoxCreateFolderThumb.Name = "checkBoxCreateFolderThumb";
      this.checkBoxCreateFolderThumb.Size = new System.Drawing.Size(137, 17);
      this.checkBoxCreateFolderThumb.TabIndex = 9;
      this.checkBoxCreateFolderThumb.Text = "create missing folder.jpg";
      this.checkBoxCreateFolderThumb.UseVisualStyleBackColor = true;
      // 
      // checkBoxCreateArtist
      // 
      this.checkBoxCreateArtist.AutoSize = true;
      this.checkBoxCreateArtist.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxCreateArtist.Location = new System.Drawing.Point(15, 45);
      this.checkBoxCreateArtist.Name = "checkBoxCreateArtist";
      this.checkBoxCreateArtist.Size = new System.Drawing.Size(156, 17);
      this.checkBoxCreateArtist.TabIndex = 8;
      this.checkBoxCreateArtist.Text = "create artist preview thumbs";
      this.checkBoxCreateArtist.UseVisualStyleBackColor = true;
      // 
      // groupBoxUseForThumbs
      // 
      this.groupBoxUseForThumbs.Controls.Add(this.checkBoxAllImages);
      this.groupBoxUseForThumbs.Controls.Add(this.checkBoxUseFolderThumb);
      this.groupBoxUseForThumbs.Controls.Add(this.buildThumbsCheckBox);
      this.groupBoxUseForThumbs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxUseForThumbs.Location = new System.Drawing.Point(16, 165);
      this.groupBoxUseForThumbs.Name = "groupBoxUseForThumbs";
      this.groupBoxUseForThumbs.Size = new System.Drawing.Size(210, 96);
      this.groupBoxUseForThumbs.TabIndex = 13;
      this.groupBoxUseForThumbs.TabStop = false;
      this.groupBoxUseForThumbs.Text = "Use for thumb creation:";
      // 
      // checkBoxAllImages
      // 
      this.checkBoxAllImages.AutoSize = true;
      this.checkBoxAllImages.Checked = true;
      this.checkBoxAllImages.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAllImages.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAllImages.Location = new System.Drawing.Point(15, 45);
      this.checkBoxAllImages.Name = "checkBoxAllImages";
      this.checkBoxAllImages.Size = new System.Drawing.Size(181, 17);
      this.checkBoxAllImages.TabIndex = 5;
      this.checkBoxAllImages.Text = "*.png, *.jpg (prefers \"front\" cover)";
      this.checkBoxAllImages.UseVisualStyleBackColor = true;
      // 
      // checkBoxUseFolderThumb
      // 
      this.checkBoxUseFolderThumb.AutoSize = true;
      this.checkBoxUseFolderThumb.Checked = true;
      this.checkBoxUseFolderThumb.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUseFolderThumb.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseFolderThumb.Location = new System.Drawing.Point(15, 22);
      this.checkBoxUseFolderThumb.Name = "checkBoxUseFolderThumb";
      this.checkBoxUseFolderThumb.Size = new System.Drawing.Size(143, 17);
      this.checkBoxUseFolderThumb.TabIndex = 4;
      this.checkBoxUseFolderThumb.Text = "folder.jpg (recommended)";
      this.checkBoxUseFolderThumb.UseVisualStyleBackColor = true;
      this.checkBoxUseFolderThumb.CheckedChanged += new System.EventHandler(this.checkBoxUseFolderThumb_CheckedChanged);
      // 
      // buildThumbsCheckBox
      // 
      this.buildThumbsCheckBox.AutoSize = true;
      this.buildThumbsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.buildThumbsCheckBox.Location = new System.Drawing.Point(15, 68);
      this.buildThumbsCheckBox.Name = "buildThumbsCheckBox";
      this.buildThumbsCheckBox.Size = new System.Drawing.Size(177, 17);
      this.buildThumbsCheckBox.TabIndex = 6;
      this.buildThumbsCheckBox.Text = "cover art embedded in ID3-Tags";
      this.buildThumbsCheckBox.UseVisualStyleBackColor = true;
      // 
      // tbPrefixes
      // 
      this.tbPrefixes.BorderColor = System.Drawing.Color.Empty;
      this.tbPrefixes.Location = new System.Drawing.Point(301, 109);
      this.tbPrefixes.Name = "tbPrefixes";
      this.tbPrefixes.Size = new System.Drawing.Size(155, 20);
      this.tbPrefixes.TabIndex = 2;
      // 
      // checkBoxStripArtistPrefix
      // 
      this.checkBoxStripArtistPrefix.AutoSize = true;
      this.checkBoxStripArtistPrefix.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxStripArtistPrefix.Location = new System.Drawing.Point(16, 110);
      this.checkBoxStripArtistPrefix.Name = "checkBoxStripArtistPrefix";
      this.checkBoxStripArtistPrefix.Size = new System.Drawing.Size(279, 17);
      this.checkBoxStripArtistPrefix.TabIndex = 1;
      this.checkBoxStripArtistPrefix.Text = "Strip artist prefix (i.e. \"The Beatles\" ->  \"Beatles, The\")";
      this.checkBoxStripArtistPrefix.UseVisualStyleBackColor = true;
      // 
      // checkBoxUpdateSinceLastImport
      // 
      this.checkBoxUpdateSinceLastImport.AutoSize = true;
      this.checkBoxUpdateSinceLastImport.Checked = true;
      this.checkBoxUpdateSinceLastImport.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUpdateSinceLastImport.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUpdateSinceLastImport.Location = new System.Drawing.Point(16, 278);
      this.checkBoxUpdateSinceLastImport.Name = "checkBoxUpdateSinceLastImport";
      this.checkBoxUpdateSinceLastImport.Size = new System.Drawing.Size(178, 17);
      this.checkBoxUpdateSinceLastImport.TabIndex = 10;
      this.checkBoxUpdateSinceLastImport.Text = "Only update new / changed files";
      this.checkBoxUpdateSinceLastImport.UseVisualStyleBackColor = true;
      // 
      // monitorSharesCheckBox
      // 
      this.monitorSharesCheckBox.AutoSize = true;
      this.monitorSharesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.monitorSharesCheckBox.Location = new System.Drawing.Point(16, 310);
      this.monitorSharesCheckBox.Name = "monitorSharesCheckBox";
      this.monitorSharesCheckBox.Size = new System.Drawing.Size(199, 17);
      this.monitorSharesCheckBox.TabIndex = 11;
      this.monitorSharesCheckBox.Text = "Auto-update DB on changes in share";
      this.monitorSharesCheckBox.UseVisualStyleBackColor = true;
      // 
      // folderAsAlbumCheckBox
      // 
      this.folderAsAlbumCheckBox.AutoSize = true;
      this.folderAsAlbumCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.folderAsAlbumCheckBox.Location = new System.Drawing.Point(16, 133);
      this.folderAsAlbumCheckBox.Name = "folderAsAlbumCheckBox";
      this.folderAsAlbumCheckBox.Size = new System.Drawing.Size(243, 17);
      this.folderAsAlbumCheckBox.TabIndex = 3;
      this.folderAsAlbumCheckBox.Text = "Treat tracks in an individual folder as an album";
      this.folderAsAlbumCheckBox.UseVisualStyleBackColor = true;
      this.folderAsAlbumCheckBox.CheckedChanged += new System.EventHandler(this.folderAsAlbumCheckBox_CheckedChanged);
      // 
      // startButton
      // 
      this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.startButton.Location = new System.Drawing.Point(246, 307);
      this.startButton.Name = "startButton";
      this.startButton.Size = new System.Drawing.Size(210, 22);
      this.startButton.TabIndex = 12;
      this.startButton.Text = "Update database from selected shares";
      this.startButton.UseVisualStyleBackColor = true;
      this.startButton.Click += new System.EventHandler(this.startButton_Click);
      // 
      // sharesListBox
      // 
      this.sharesListBox.CheckOnClick = true;
      this.sharesListBox.Location = new System.Drawing.Point(16, 24);
      this.sharesListBox.Name = "sharesListBox";
      this.sharesListBox.Size = new System.Drawing.Size(440, 79);
      this.sharesListBox.TabIndex = 0;
      this.sharesListBox.SelectedIndexChanged += new System.EventHandler(this.sharesListBox_SelectedIndexChanged);
      this.sharesListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.sharesListBox_ItemCheck);
      this.sharesListBox.DoubleClick += new System.EventHandler(this.sharesListBox_DoubleClick);
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.fileLabel);
      this.groupBox2.Controls.Add(this.progressBar);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(0, 345);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 51);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Progress";
      // 
      // fileLabel
      // 
      this.fileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.fileLabel.Location = new System.Drawing.Point(16, 23);
      this.fileLabel.Name = "fileLabel";
      this.fileLabel.Size = new System.Drawing.Size(440, 16);
      this.fileLabel.TabIndex = 0;
      // 
      // progressBar
      // 
      this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar.Location = new System.Drawing.Point(16, 23);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(440, 16);
      this.progressBar.TabIndex = 1;
      // 
      // MusicDatabase
      // 
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "MusicDatabase";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBoxUseAlbumThumbs.ResumeLayout(false);
      this.groupBoxUseAlbumThumbs.PerformLayout();
      this.groupBoxUseForThumbs.ResumeLayout(false);
      this.groupBoxUseForThumbs.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void sharesListBox_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
    {
      UpdateControlStatus();
    }

    private void sharesListBox_DoubleClick(object sender, System.EventArgs e)
    {
      UpdateControlStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateControlStatus()
    {
      startButton.Enabled = sharesListBox.CheckedItems.Count > 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void sharesListBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      UpdateControlStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 

    void SetPercentDonebyEvent(object sender, DatabaseReorgEventArgs e)
    {
      progressBar.Value = e.progress;
      SetStatus(e.phase);
    }

    private void startButton_Click(object sender, System.EventArgs e)
    {
      ArrayList shares = new ArrayList();
      for (int index = 0; index < sharesListBox.CheckedIndices.Count; index++)
      {
        string path = sharesListBox.Items[(int)sharesListBox.CheckedIndices[index]].ToString();
        if (Directory.Exists(path))
        {
          try
          {
            string driveName = path.Substring(0, 1);
            if (path.StartsWith(@"\\"))
              // we have the path in unc notation
              driveName = path;

            ulong FreeBytesAvailable = MediaPortal.Util.Utils.GetFreeDiskSpace(driveName);

            if (FreeBytesAvailable > 0)
            {
              ulong DiskSpace = FreeBytesAvailable / 1048576;
              if (DiskSpace > 100) // > 100MB left for creation of thumbs, etc
              {
                Log.Info("MusicDatabase: adding share {0} for scanning - available disk space: {1} MB", path, DiskSpace.ToString());
                shares.Add(path);
              }
              else
                Log.Warn("MusicDatabase: NOT scanning share {0} because of low disk space: {1} MB", path, DiskSpace.ToString());
            }
          }
          catch (Exception)
          {
            // Drive not ready, etc
          }
        }
      }
      MediaPortal.Music.Database.MusicDatabase.DatabaseReorgChanged += new MusicDBReorgEventHandler(SetPercentDonebyEvent);
      groupBox1.Enabled = false;
      groupBox2.Enabled = true;

      //RebuildDatabase();
      progressBar.Maximum = 100;

      // Now create a Settings Object with the Settings checked to pass to the Import
      MusicDatabaseSettings setting = new MusicDatabaseSettings();
      setting.CreateMissingFolderThumb = checkBoxCreateFolderThumb.Checked;
      setting.ExtractEmbeddedCoverArt = buildThumbsCheckBox.Checked;
      setting.StripArtistPrefixes = checkBoxStripArtistPrefix.Checked;
      setting.TreatFolderAsAlbum = folderAsAlbumCheckBox.Checked;
      setting.UseFolderThumbs = checkBoxUseFolderThumb.Checked;
      setting.UseAllImages = checkBoxAllImages.Checked;
      setting.CreateArtistPreviews = checkBoxCreateArtist.Checked;
      setting.CreateGenrePreviews = checkBoxCreateGenre.Checked;
      setting.UseLastImportDate = checkBoxUpdateSinceLastImport.Checked;
      // ToDo - add GUI setting if wanted
      setting.ExcludeHiddenFiles = false;

      int appel = m_dbs.MusicDatabaseReorg(shares, setting);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        checkBoxUpdateSinceLastImport.Text = String.Format("Only update new / changed files after {0}", xmlreader.GetValueAsString("musicfiles", "lastImport", "1900-01-01 00:00:00"));
      }
      progressBar.Value = 100;

      groupBox1.Enabled = true;
      groupBox2.Enabled = false;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    private void SetStatus(string status)
    {
      fileLabel.Text = status;
      System.Windows.Forms.Application.DoEvents();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cancelButton_Click(object sender, System.EventArgs e)
    {
      //Not yet an option to stop rebuildin the database
      //stopRebuild = true;
    }

    private void clearButton_Click(object sender, System.EventArgs e)
    {
      DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the entire music database?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

      if (dialogResult == DialogResult.Yes)
      {
        string database = Config.GetFile(Config.Dir.Config, "MusicDatabaseV7.db3");

        if (File.Exists(database))
          File.Delete(database);

        MessageBox.Show("Music database has been cleared");
      }
    }

    private void folderAsAlbumCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (folderAsAlbumCheckBox.Checked)
        checkBoxCreateFolderThumb.Enabled = true;
      else
      {
        checkBoxCreateFolderThumb.Checked = false;
        checkBoxCreateFolderThumb.Enabled = false;
      }
    }

    private void checkBoxUseFolderThumb_CheckedChanged(object sender, EventArgs e)
    {    
      if (checkBoxUseFolderThumb.Checked)
        checkBoxAllImages.Enabled = true;
      else
      {
        checkBoxAllImages.Checked = false;
        checkBoxAllImages.Enabled = false;
      }
    }

  }
}