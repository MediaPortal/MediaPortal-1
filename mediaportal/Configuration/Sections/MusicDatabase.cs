#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class MusicDatabase : SectionSettings
  {
    #region Variables

    private MPGroupBox groupBox1;
    private CheckedListBox sharesListBox;
    private MPButton startButton;
    private MPGroupBox groupBox2;
    private MPLabel fileLabel;
    private IContainer components = null;
    private MPCheckBox folderAsAlbumCheckBox;
    private MPCheckBox monitorSharesCheckBox;
    private MPCheckBox checkBoxUpdateSinceLastImport;
    private MPCheckBox checkBoxStripArtistPrefix;
    private MPTextBox tbPrefixes;
    private MPGroupBox groupBoxUseForThumbs;
    private MPCheckBox buildThumbsCheckBox;
    private MPCheckBox checkBoxUseFolderThumb;
    private MPGroupBox groupBoxUseAlbumThumbs;
    private MPCheckBox checkBoxCreateArtist;
    private MPCheckBox checkBoxCreateFolderThumb;
    private MPCheckBox checkBoxCreateGenre;
    private MPCheckBox checkBoxAllImages;

    private MediaPortal.Music.Database.MusicDatabase m_dbs = MediaPortal.Music.Database.MusicDatabase.Instance;

    private List<BaseShares.ShareData> sharesData = null;
    private BackgroundWorker _scanThread = null;
    private MPComboBox comboBoxDateAdded;
    private MPLabel lblDate;
    private bool _scanRunning = false;

    #endregion

    #region ctor / dtor

    public MusicDatabase()
      : this("Music Database") {}

    public MusicDatabase(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      groupBox2.Enabled = false;
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

    #endregion

    #region Properties

    private string[] Extensions
    {
      get { return extensions; }
      set { extensions = value; }
    }

    private string[] extensions = new string[] {".mp3"};

    #endregion

    #region Overrides

    public override void OnSectionActivated()
    {
      //
      // Clear any existing entries
      //
      sharesListBox.Items.Clear();

      //
      // Load selected shares
      //
      SectionSettings section = GetSection("Music Folders");

      if (section != null)
      {
        sharesData = (List<BaseShares.ShareData>)section.GetSetting("sharesdata");

        foreach (BaseShares.ShareData share in sharesData)
        {
          //
          // Add to share to list box and select them based on the settings
          //
          sharesListBox.Items.Add(share.Folder, share.ScanShare);
        }
      }

      //
      // Fetch extensions
      //
      section = GetSection("Music Extensions");

      if (section != null)
      {
        string extensions = (string)section.GetSetting("extensions");
        Extensions = extensions.Split(new char[] {','});
      }

      UpdateControlStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        buildThumbsCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "extractthumbs", true);
        checkBoxCreateArtist.Checked = xmlreader.GetValueAsBool("musicfiles", "createartistthumbs", false);
        checkBoxCreateGenre.Checked = xmlreader.GetValueAsBool("musicfiles", "creategenrethumbs", true);
        checkBoxUseFolderThumb.Checked = xmlreader.GetValueAsBool("musicfiles", "useFolderThumbs", false);
        checkBoxAllImages.Checked = xmlreader.GetValueAsBool("musicfiles", "useAllImages",
                                                             checkBoxUseFolderThumb.Checked);
        folderAsAlbumCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "treatFolderAsAlbum", false);
        checkBoxCreateFolderThumb.Checked = xmlreader.GetValueAsBool("musicfiles", "createMissingFolderThumbs",
                                                                     folderAsAlbumCheckBox.Checked);
        monitorSharesCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "monitorShares", false);
        checkBoxUpdateSinceLastImport.Checked = xmlreader.GetValueAsBool("musicfiles", "updateSinceLastImport", true);
        checkBoxUpdateSinceLastImport.Text = String.Format("Only update new / changed files after {0}",
                                                           xmlreader.GetValueAsString("musicfiles", "lastImport",
                                                                                      "1900-01-01 00:00:00"));
        checkBoxStripArtistPrefix.Checked = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);
        tbPrefixes.Text = xmlreader.GetValueAsString("musicfiles", "artistprefixes", "The, Les, Die");
        comboBoxDateAdded.SelectedIndex = xmlreader.GetValueAsInt("musicfiles", "dateadded", 0);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
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
        xmlwriter.SetValue("musicfiles", "dateadded", comboBoxDateAdded.SelectedIndex);
      }
    }

    public override object GetSetting(string name)
    {
      switch (name.ToLowerInvariant())
      {
        case "folderscanning":
          return _scanRunning;
      }

      return null;
    }

    #endregion

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxDateAdded = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lblDate = new MediaPortal.UserInterface.Controls.MPLabel();
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
      this.groupBox1.SuspendLayout();
      this.groupBoxUseAlbumThumbs.SuspendLayout();
      this.groupBoxUseForThumbs.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.comboBoxDateAdded);
      this.groupBox1.Controls.Add(this.lblDate);
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
      this.groupBox1.Location = new System.Drawing.Point(6, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(462, 339);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Scan music folders";
      // 
      // comboBoxDateAdded
      // 
      this.comboBoxDateAdded.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxDateAdded.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDateAdded.FormattingEnabled = true;
      this.comboBoxDateAdded.Items.AddRange(new object[]
                                              {
                                                "Current Date",
                                                "Creation Date",
                                                "Last Write Date"
                                              });
      this.comboBoxDateAdded.Location = new System.Drawing.Point(246, 257);
      this.comboBoxDateAdded.Name = "comboBoxDateAdded";
      this.comboBoxDateAdded.Size = new System.Drawing.Size(210, 21);
      this.comboBoxDateAdded.TabIndex = 16;
      // 
      // lblDate
      // 
      this.lblDate.AutoSize = true;
      this.lblDate.Location = new System.Drawing.Point(16, 261);
      this.lblDate.Name = "lblDate";
      this.lblDate.Size = new System.Drawing.Size(226, 13);
      this.lblDate.TabIndex = 15;
      this.lblDate.Text = "Set date in database for new/changed files to:";
      // 
      // groupBoxUseAlbumThumbs
      // 
      this.groupBoxUseAlbumThumbs.Controls.Add(this.checkBoxCreateGenre);
      this.groupBoxUseAlbumThumbs.Controls.Add(this.checkBoxCreateFolderThumb);
      this.groupBoxUseAlbumThumbs.Controls.Add(this.checkBoxCreateArtist);
      this.groupBoxUseAlbumThumbs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxUseAlbumThumbs.Location = new System.Drawing.Point(246, 156);
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
      this.groupBoxUseForThumbs.Location = new System.Drawing.Point(16, 156);
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
      this.checkBoxUseFolderThumb.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseFolderThumb.Location = new System.Drawing.Point(15, 68);
      this.checkBoxUseFolderThumb.Name = "checkBoxUseFolderThumb";
      this.checkBoxUseFolderThumb.Size = new System.Drawing.Size(67, 17);
      this.checkBoxUseFolderThumb.TabIndex = 4;
      this.checkBoxUseFolderThumb.Text = "folder.jpg";
      this.checkBoxUseFolderThumb.UseVisualStyleBackColor = true;
      this.checkBoxUseFolderThumb.CheckedChanged += new System.EventHandler(this.checkBoxUseFolderThumb_CheckedChanged);
      // 
      // buildThumbsCheckBox
      // 
      this.buildThumbsCheckBox.AutoSize = true;
      this.buildThumbsCheckBox.Checked = true;
      this.buildThumbsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.buildThumbsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.buildThumbsCheckBox.Location = new System.Drawing.Point(15, 22);
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
      this.checkBoxUpdateSinceLastImport.Location = new System.Drawing.Point(19, 287);
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
      this.monitorSharesCheckBox.Location = new System.Drawing.Point(19, 311);
      this.monitorSharesCheckBox.Name = "monitorSharesCheckBox";
      this.monitorSharesCheckBox.Size = new System.Drawing.Size(199, 17);
      this.monitorSharesCheckBox.TabIndex = 11;
      this.monitorSharesCheckBox.Text = "Auto-update DB on changes in share";
      this.monitorSharesCheckBox.UseVisualStyleBackColor = true;
      this.monitorSharesCheckBox.CheckedChanged += new System.EventHandler(this.monitorSharesCheckBox_CheckedChanged);
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
      this.startButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.startButton.Location = new System.Drawing.Point(236, 307);
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
      this.groupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.fileLabel);
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
      this.fileLabel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.fileLabel.Location = new System.Drawing.Point(16, 23);
      this.fileLabel.Name = "fileLabel";
      this.fileLabel.Size = new System.Drawing.Size(440, 16);
      this.fileLabel.TabIndex = 0;
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

    #region Private Methods

    private void sharesListBox_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      BaseShares.ShareData share = sharesData[e.Index];
      share.ScanShare = e.NewValue == CheckState.Checked ? true : false;
      UpdateControlStatus();
    }

    private void sharesListBox_DoubleClick(object sender, EventArgs e)
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
    private void sharesListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateControlStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void SetStatus(object sender, DatabaseReorgEventArgs e)
    {
      _scanThread.ReportProgress(e.progress, e.phase);
    }

    private void startButton_Click(object sender, EventArgs e)
    {
      groupBox1.Enabled = false;
      groupBox2.Enabled = true;
      // Now create a Settings Object with the Settings checked to pass to the Import
      MusicDatabaseSettings setting = new MusicDatabaseSettings
      {
        CreateMissingFolderThumb = checkBoxCreateFolderThumb.Checked,
        ExtractEmbeddedCoverArt = buildThumbsCheckBox.Checked,
        StripArtistPrefixes = checkBoxStripArtistPrefix.Checked,
        TreatFolderAsAlbum = folderAsAlbumCheckBox.Checked,
        UseFolderThumbs = checkBoxUseFolderThumb.Checked,
        UseAllImages = checkBoxAllImages.Checked,
        CreateArtistPreviews = checkBoxCreateArtist.Checked,
        CreateGenrePreviews = checkBoxCreateGenre.Checked,
        UseLastImportDate = checkBoxUpdateSinceLastImport.Checked,
        ExcludeHiddenFiles = false,
        DateAddedValue = comboBoxDateAdded.SelectedIndex
      };

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
            {
              // we have the path in unc notation
              driveName = path;
            }

            ulong freeBytesAvailable = Util.Utils.GetFreeDiskSpace(driveName);

            if (freeBytesAvailable > 0)
            {
              ulong diskSpace = freeBytesAvailable / 1048576;
              if (diskSpace > 100) // > 100MB left for creation of thumbs, etc
              {
                Log.Info("MusicDatabase: adding share {0} for scanning - available disk space: {1} MB", path,
                         diskSpace.ToString());
                shares.Add(path);
              }
              else
              {
                Log.Warn("MusicDatabase: NOT scanning share {0} because of low disk space: {1} MB", path,
                         diskSpace.ToString());
              }
            }
          }
          catch (Exception)
          {
            // Drive not ready, etc
          }
        }
      }

      _scanThread = new BackgroundWorker();
      _scanThread.WorkerReportsProgress = true;
      _scanThread.DoWork += FolderScan;
      _scanThread.RunWorkerCompleted += FolderScanCompleted;
      _scanThread.ProgressChanged += FolderScanProgress;
      _scanThread.RunWorkerAsync(new object[] { shares, setting });
    }

    /// <summary>
    /// Backgroundworker to scan folders
    /// </summary>
    private void FolderScan(object sender, DoWorkEventArgs e)
    {
      _scanRunning = true;

      // Get the arguments
      object[] args = e.Argument as object[];
      var shares = args[0] as ArrayList;
      var setting = args[1] as MusicDatabaseSettings;

      MediaPortal.Music.Database.MusicDatabase.DatabaseReorgChanged += SetStatus;

      try
      {
        m_dbs.MusicDatabaseReorg(shares, setting);
      }
      catch (Exception ex)
      {
        Log.Error("Folder Scan: Exception during processing: ", ex.Message);
        _scanRunning = false;
      }
    }

    /// <summary>
    /// Backgroundworker ended
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FolderScanCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      _scanRunning = false;
      groupBox1.Enabled = true;
      groupBox2.Enabled = false;

      using (Settings xmlreader = new MPSettings())
      {
        checkBoxUpdateSinceLastImport.Text = String.Format("Only update new / changed files after {0}",
                                                           xmlreader.GetValueAsString("musicfiles", "lastImport",
                                                                                      "1900-01-01 00:00:00"));
      }
    }

    private void FolderScanProgress(object sender, ProgressChangedEventArgs e)
    {
      fileLabel.Text = e.UserState as string;
    }

    private void folderAsAlbumCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (folderAsAlbumCheckBox.Checked)
      {
        checkBoxCreateFolderThumb.Enabled = true;
      }
      else
      {
        checkBoxCreateFolderThumb.Checked = false;
        checkBoxCreateFolderThumb.Enabled = false;
      }
    }

    private void checkBoxUseFolderThumb_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxUseFolderThumb.Checked)
      {
        checkBoxAllImages.Enabled = true;
      }
      else
      {
        checkBoxAllImages.Checked = false;
        checkBoxAllImages.Enabled = false;
      }
    }

    /// <summary>
    /// When enabled then we need to enable the Music Share Watcher Plugin
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void monitorSharesCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      using (Settings xmlwriter = new MPSettings())
      {
        if (monitorSharesCheckBox.Checked)
        {
          xmlwriter.SetValueAsBool("plugins", "Music Share Watcher", true);
        }
        else
        {
          xmlwriter.SetValueAsBool("plugins", "Music Share Watcher", false);
        }
      }
    }

    #endregion
  }
}