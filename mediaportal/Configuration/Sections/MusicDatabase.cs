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
        private MediaPortal.UserInterface.Controls.MPCheckBox buildThumbsCheckBox;
        private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
        private System.Windows.Forms.ProgressBar progressBar;
        private MediaPortal.UserInterface.Controls.MPLabel fileLabel;
        private MediaPortal.UserInterface.Controls.MPButton cancelButton;
        private System.ComponentModel.IContainer components = null;

        private MediaPortal.UserInterface.Controls.MPCheckBox scanForVariousArtistsCheckBox;
        private MediaPortal.UserInterface.Controls.MPCheckBox folderAsAlbumCheckBox;

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

        MediaPortal.Music.Database.MusicDatabase m_dbs = new MediaPortal.Music.Database.MusicDatabase();

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
            SectionSettings section = SectionSettings.GetSection("MusicShares");

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
            section = SectionSettings.GetSection("MusicExtensions");

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
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
            {
                buildThumbsCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "buildThumbs", false);
                folderAsAlbumCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "treatFolderAsAlbum", false);
                scanForVariousArtistsCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "scanForVariousArtists", true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void SaveSettings()
        {
          using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
            {
                xmlwriter.SetValueAsBool("musicfiles", "buildThumbs", buildThumbsCheckBox.Checked);

                xmlwriter.SetValueAsBool("musicfiles", "treatFolderAsAlbum", folderAsAlbumCheckBox.Checked);
                xmlwriter.SetValueAsBool("musicfiles", "scanForVariousArtists", scanForVariousArtistsCheckBox.Checked);
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
            this.scanForVariousArtistsCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.folderAsAlbumCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.buildThumbsCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.startButton = new MediaPortal.UserInterface.Controls.MPButton();
            this.sharesListBox = new System.Windows.Forms.CheckedListBox();
            this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
            this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.fileLabel = new MediaPortal.UserInterface.Controls.MPLabel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.scanForVariousArtistsCheckBox);
            this.groupBox1.Controls.Add(this.folderAsAlbumCheckBox);
            this.groupBox1.Controls.Add(this.buildThumbsCheckBox);
            this.groupBox1.Controls.Add(this.startButton);
            this.groupBox1.Controls.Add(this.sharesListBox);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(472, 234);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Scan Music Folders";
            // 
            // scanForVariousArtistsCheckBox
            // 
            this.scanForVariousArtistsCheckBox.AutoSize = true;
            this.scanForVariousArtistsCheckBox.Checked = true;
            this.scanForVariousArtistsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.scanForVariousArtistsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scanForVariousArtistsCheckBox.Location = new System.Drawing.Point(16, 169);
            this.scanForVariousArtistsCheckBox.Name = "scanForVariousArtistsCheckBox";
            this.scanForVariousArtistsCheckBox.Size = new System.Drawing.Size(169, 17);
            this.scanForVariousArtistsCheckBox.TabIndex = 3;
            this.scanForVariousArtistsCheckBox.Text = "Scan albums for Various Artists";
            this.scanForVariousArtistsCheckBox.UseVisualStyleBackColor = true;
            // 
            // folderAsAlbumCheckBox
            // 
            this.folderAsAlbumCheckBox.AutoSize = true;
            this.folderAsAlbumCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.folderAsAlbumCheckBox.Location = new System.Drawing.Point(16, 148);
            this.folderAsAlbumCheckBox.Name = "folderAsAlbumCheckBox";
            this.folderAsAlbumCheckBox.Size = new System.Drawing.Size(243, 17);
            this.folderAsAlbumCheckBox.TabIndex = 2;
            this.folderAsAlbumCheckBox.Text = "Treat tracks in an individual folder as an album";
            this.folderAsAlbumCheckBox.UseVisualStyleBackColor = true;
            // 
            // buildThumbsCheckBox
            // 
            this.buildThumbsCheckBox.AutoSize = true;
            this.buildThumbsCheckBox.Checked = true;
            this.buildThumbsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.buildThumbsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buildThumbsCheckBox.Location = new System.Drawing.Point(16, 127);
            this.buildThumbsCheckBox.Name = "buildThumbsCheckBox";
            this.buildThumbsCheckBox.Size = new System.Drawing.Size(247, 17);
            this.buildThumbsCheckBox.TabIndex = 1;
            this.buildThumbsCheckBox.Text = "Use coverart embedded in MP3s for thumbnails";
            this.buildThumbsCheckBox.UseVisualStyleBackColor = true;
            // 
            // startButton
            // 
            this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.startButton.Location = new System.Drawing.Point(240, 202);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(216, 22);
            this.startButton.TabIndex = 4;
            this.startButton.Text = "Update database from selected shares";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // sharesListBox
            // 
            this.sharesListBox.CheckOnClick = true;
            this.sharesListBox.Location = new System.Drawing.Point(16, 24);
            this.sharesListBox.Name = "sharesListBox";
            this.sharesListBox.Size = new System.Drawing.Size(440, 94);
            this.sharesListBox.TabIndex = 0;
            this.sharesListBox.DoubleClick += new System.EventHandler(this.sharesListBox_DoubleClick);
            this.sharesListBox.SelectedIndexChanged += new System.EventHandler(this.sharesListBox_SelectedIndexChanged);
            this.sharesListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.sharesListBox_ItemCheck);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(384, 328);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(72, 22);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Visible = false;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.fileLabel);
            this.groupBox2.Controls.Add(this.progressBar);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBox2.Location = new System.Drawing.Point(0, 240);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(472, 80);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Progress";
            // 
            // fileLabel
            // 
            this.fileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fileLabel.Location = new System.Drawing.Point(16, 24);
            this.fileLabel.Name = "fileLabel";
            this.fileLabel.Size = new System.Drawing.Size(440, 16);
            this.fileLabel.TabIndex = 0;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(16, 48);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(440, 16);
            this.progressBar.TabIndex = 1;
            // 
            // MusicDatabase
            // 
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cancelButton);
            this.Name = "MusicDatabase";
            this.Size = new System.Drawing.Size(472, 408);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
                    shares.Add(path);
                }
            }
            m_dbs.DatabaseReorgChanged += new MusicDBReorgEventHandler(SetPercentDonebyEvent);
            groupBox1.Enabled = false;
            groupBox2.Enabled = true;

            //RebuildDatabase();
            progressBar.Maximum = 100;

            int appel = m_dbs.MusicDatabaseReorg(shares, folderAsAlbumCheckBox.Checked, scanForVariousArtistsCheckBox.Checked);
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
              string database = Config.Get(Config.Dir.Config) + "MusicDatabaseV7.db3";
                
                if (File.Exists(database))
                    File.Delete(database);

                MessageBox.Show("Music database has been cleared");
            }
        }
    }
}

