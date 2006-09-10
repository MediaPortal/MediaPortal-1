////#region Copyright (C) 2005-2006 Team MediaPortal

/////* 
//// *	Copyright (C) 2005-2006 Team MediaPortal
//// *	http://www.team-mediaportal.com
//// *
//// *  This Program is free software; you can redistribute it and/or modify
//// *  it under the terms of the GNU General Public License as published by
//// *  the Free Software Foundation; either version 2, or (at your option)
//// *  any later version.
//// *   
//// *  This Program is distributed in the hope that it will be useful,
//// *  but WITHOUT ANY WARRANTY; without even the implied warranty of
//// *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// *  GNU General Public License for more details.
//// *   
//// *  You should have received a copy of the GNU General Public License
//// *  along with GNU Make; see the file COPYING.  If not, write to
//// *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
//// *  http://www.gnu.org/copyleft/gpl.html
//// *
//// */

////#endregion

////using System;
////using System.Collections;
////using System.ComponentModel;
////using System.Drawing;
////using System.Windows.Forms;
////using MediaPortal.Util;

////#pragma warning disable 108
////namespace MediaPortal.Configuration.Sections
////{
////    public class Music : MediaPortal.Configuration.SectionSettings
////    {
////        private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
////        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
////        private MediaPortal.UserInterface.Controls.MPLabel label1;
////        private MediaPortal.UserInterface.Controls.MPButton playlistButton;
////        private MediaPortal.UserInterface.Controls.MPTextBox playlistFolderTextBox;
////        private MediaPortal.UserInterface.Controls.MPCheckBox autoShuffleCheckBox;
////        private MediaPortal.UserInterface.Controls.MPCheckBox repeatPlaylistCheckBox;
////        private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
////        private MediaPortal.UserInterface.Controls.MPCheckBox showID3CheckBox;
////        private MediaPortal.UserInterface.Controls.MPComboBox audioPlayerComboBox;
////        private MediaPortal.UserInterface.Controls.MPLabel label4;
////        private MediaPortal.UserInterface.Controls.MPLabel label2;
////        private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
////        private MediaPortal.UserInterface.Controls.MPComboBox autoPlayComboBox;
////        private MediaPortal.UserInterface.Controls.MPLabel labelAutoPlay;
////        private System.ComponentModel.IContainer components = null;
////        private CheckBox enableVisualisation;

////        string[] autoPlayOptions = new string[] { "Autoplay, never ask", "Don't autoplay, never ask", "Ask every time a CD is inserted" };

////        public Music()
////            : this("Music")
////        {
////        }

////        public Music(string name)
////            : base(name)
////        {
////            // This call is required by the Windows Form Designer.
////            InitializeComponent();

////            //
////            // Set available media players
////            //
////            audioPlayerComboBox.Items.AddRange(new string[] { "Windows Media Player 9",
////                                                                "DirectShow" });

////            autoPlayComboBox.Items.Clear();
////            autoPlayComboBox.Items.AddRange(autoPlayOptions);
////        }

////        /// <summary>
////        /// 
////        /// </summary>
////        public override void LoadSettings()
////        {
////          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
////            {
////                repeatPlaylistCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "repeat", true);
////                showID3CheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "showid3", false);
////                autoShuffleCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", true);
////                enableVisualisation.Checked = xmlreader.GetValueAsBool("musicfiles", "doVisualisation", true);

////                string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
////                playListFolder += @"\My Playlists";
////                playlistFolderTextBox.Text = xmlreader.GetValueAsString("music", "playlists", playListFolder);

////                if (string.Compare(playlistFolderTextBox.Text, playListFolder) == 0)
////                {
////                    if (System.IO.Directory.Exists(playListFolder) == false)
////                    {
////                        try
////                        {
////                            System.IO.Directory.CreateDirectory(playListFolder);
////                        }
////                        catch (Exception) { }
////                    }
////                }

////                audioPlayerComboBox.Text = xmlreader.GetValueAsString("audioplayer", "player", "Windows Media Player 9");
////                string autoPlayText = xmlreader.GetValueAsString("audioplayer", "autoplay", "Yes");

////                switch (autoPlayText)
////                {
////                    case "No": autoPlayComboBox.Text = autoPlayOptions[1];
////                        break;
////                    case "Ask": autoPlayComboBox.Text = autoPlayOptions[2];
////                        break;
////                    default: autoPlayComboBox.Text = autoPlayOptions[0];
////                        break;
////                }
////            }
////        }

////        public override void SaveSettings()
////        {
////          using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
////            {
////                xmlwriter.SetValueAsBool("musicfiles", "repeat", repeatPlaylistCheckBox.Checked);
////                xmlwriter.SetValueAsBool("musicfiles", "showid3", showID3CheckBox.Checked);
////                xmlwriter.SetValueAsBool("musicfiles", "autoshuffle", autoShuffleCheckBox.Checked);
////                xmlwriter.SetValueAsBool("musicfiles", "doVisualisation", enableVisualisation.Checked);

////                xmlwriter.SetValue("music", "playlists", playlistFolderTextBox.Text);

////                xmlwriter.SetValue("audioplayer", "player", audioPlayerComboBox.Text);

////                string autoPlayText;

////                if (autoPlayComboBox.Text == autoPlayOptions[1])
////                    autoPlayText = "No";
////                else if (autoPlayComboBox.Text == autoPlayOptions[2])
////                    autoPlayText = "Ask";
////                else
////                    autoPlayText = "Yes";

////                xmlwriter.SetValue("audioplayer", "autoplay", autoPlayText);
////            }
////        }

////        /// <summary>
////        /// Clean up any resources being used.
////        /// </summary>
////        protected override void Dispose(bool disposing)
////        {
////            if (disposing)
////            {
////                if (components != null)
////                {
////                    components.Dispose();
////                }
////            }
////            base.Dispose(disposing);
////        }

////        #region Designer generated code
////        /// <summary>
////        /// Required method for Designer support - do not modify
////        /// the contents of this method with the code editor.
////        /// </summary>
////        private void InitializeComponent()
////        {
////            this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
////            this.autoShuffleCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
////            this.repeatPlaylistCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
////            this.playlistButton = new MediaPortal.UserInterface.Controls.MPButton();
////            this.playlistFolderTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
////            this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
////            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
////            this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
////            this.enableVisualisation = new System.Windows.Forms.CheckBox();
////            this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
////            this.showID3CheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
////            this.audioPlayerComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
////            this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
////            this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
////            this.labelAutoPlay = new MediaPortal.UserInterface.Controls.MPLabel();
////            this.autoPlayComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
////            this.groupBox1.SuspendLayout();
////            this.mpGroupBox1.SuspendLayout();
////            this.mpGroupBox2.SuspendLayout();
////            this.SuspendLayout();
////            // 
////            // groupBox1
////            // 
////            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
////                        | System.Windows.Forms.AnchorStyles.Right)));
////            this.groupBox1.Controls.Add(this.autoShuffleCheckBox);
////            this.groupBox1.Controls.Add(this.repeatPlaylistCheckBox);
////            this.groupBox1.Controls.Add(this.playlistButton);
////            this.groupBox1.Controls.Add(this.playlistFolderTextBox);
////            this.groupBox1.Controls.Add(this.label1);
////            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
////            this.groupBox1.Location = new System.Drawing.Point(0, 118);
////            this.groupBox1.Name = "groupBox1";
////            this.groupBox1.Size = new System.Drawing.Size(472, 104);
////            this.groupBox1.TabIndex = 1;
////            this.groupBox1.TabStop = false;
////            this.groupBox1.Text = "Playlist Settings";
////            // 
////            // autoShuffleCheckBox
////            // 
////            this.autoShuffleCheckBox.AutoSize = true;
////            this.autoShuffleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
////            this.autoShuffleCheckBox.Location = new System.Drawing.Point(162, 44);
////            this.autoShuffleCheckBox.Name = "autoShuffleCheckBox";
////            this.autoShuffleCheckBox.Size = new System.Drawing.Size(119, 17);
////            this.autoShuffleCheckBox.TabIndex = 1;
////            this.autoShuffleCheckBox.Text = "Auto shuffle playlists";
////            this.autoShuffleCheckBox.UseVisualStyleBackColor = true;
////            // 
////            // repeatPlaylistCheckBox
////            // 
////            this.repeatPlaylistCheckBox.AutoSize = true;
////            this.repeatPlaylistCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
////            this.repeatPlaylistCheckBox.Location = new System.Drawing.Point(162, 20);
////            this.repeatPlaylistCheckBox.Name = "repeatPlaylistCheckBox";
////            this.repeatPlaylistCheckBox.Size = new System.Drawing.Size(219, 17);
////            this.repeatPlaylistCheckBox.TabIndex = 0;
////            this.repeatPlaylistCheckBox.Text = "Repeat/loop music playlists (m3u, b4, pls)";
////            this.repeatPlaylistCheckBox.UseVisualStyleBackColor = true;
////            // 
////            // playlistButton
////            // 
////            this.playlistButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
////            this.playlistButton.Location = new System.Drawing.Point(378, 68);
////            this.playlistButton.Name = "playlistButton";
////            this.playlistButton.Size = new System.Drawing.Size(72, 22);
////            this.playlistButton.TabIndex = 4;
////            this.playlistButton.Text = "Browse";
////            this.playlistButton.UseVisualStyleBackColor = true;
////            this.playlistButton.Click += new System.EventHandler(this.playlistButton_Click);
////            // 
////            // playlistFolderTextBox
////            // 
////            this.playlistFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
////                        | System.Windows.Forms.AnchorStyles.Right)));
////            this.playlistFolderTextBox.Location = new System.Drawing.Point(162, 68);
////            this.playlistFolderTextBox.Name = "playlistFolderTextBox";
////            this.playlistFolderTextBox.Size = new System.Drawing.Size(208, 20);
////            this.playlistFolderTextBox.TabIndex = 3;
////            // 
////            // label1
////            // 
////            this.label1.AutoSize = true;
////            this.label1.Location = new System.Drawing.Point(15, 72);
////            this.label1.Name = "label1";
////            this.label1.Size = new System.Drawing.Size(101, 13);
////            this.label1.TabIndex = 2;
////            this.label1.Text = "Music playlist folder:";
////            // 
////            // mpGroupBox1
////            // 
////            this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
////                        | System.Windows.Forms.AnchorStyles.Right)));
////            this.mpGroupBox1.Controls.Add(this.enableVisualisation);
////            this.mpGroupBox1.Controls.Add(this.label2);
////            this.mpGroupBox1.Controls.Add(this.showID3CheckBox);
////            this.mpGroupBox1.Controls.Add(this.audioPlayerComboBox);
////            this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
////            this.mpGroupBox1.Location = new System.Drawing.Point(0, 0);
////            this.mpGroupBox1.Name = "mpGroupBox1";
////            this.mpGroupBox1.Size = new System.Drawing.Size(472, 114);
////            this.mpGroupBox1.TabIndex = 0;
////            this.mpGroupBox1.TabStop = false;
////            this.mpGroupBox1.Text = "General Settings";
////            // 
////            // enableVisualisation
////            // 
////            this.enableVisualisation.AutoSize = true;
////            this.enableVisualisation.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
////            this.enableVisualisation.Location = new System.Drawing.Point(162, 74);
////            this.enableVisualisation.Name = "enableVisualisation";
////            this.enableVisualisation.Size = new System.Drawing.Size(258, 30);
////            this.enableVisualisation.TabIndex = 3;
////            this.enableVisualisation.Text = "Enable visualisation (Windows \r\nMedia Player only)";
////            this.enableVisualisation.UseVisualStyleBackColor = true;
////            // 
////            // label2
////            // 
////            this.label2.AutoSize = true;
////            this.label2.Location = new System.Drawing.Point(16, 49);
////            this.label2.Name = "label2";
////            this.label2.Size = new System.Drawing.Size(106, 13);
////            this.label2.TabIndex = 1;
////            this.label2.Text = "Internal music player:";
////            // 
////            // showID3CheckBox
////            // 
////            this.showID3CheckBox.AutoSize = true;
////            this.showID3CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
////            this.showID3CheckBox.Location = new System.Drawing.Point(162, 20);
////            this.showID3CheckBox.Name = "showID3CheckBox";
////            this.showID3CheckBox.Size = new System.Drawing.Size(289, 17);
////            this.showID3CheckBox.TabIndex = 0;
////            this.showID3CheckBox.Text = "Load ID3 tags from file if it\'s not in music database (slow)";
////            this.showID3CheckBox.UseVisualStyleBackColor = true;
////            this.showID3CheckBox.CheckedChanged += new System.EventHandler(this.showID3CheckBox_CheckedChanged);
////            // 
////            // audioPlayerComboBox
////            // 
////            this.audioPlayerComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
////                        | System.Windows.Forms.AnchorStyles.Right)));
////            this.audioPlayerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
////            this.audioPlayerComboBox.Location = new System.Drawing.Point(162, 45);
////            this.audioPlayerComboBox.Name = "audioPlayerComboBox";
////            this.audioPlayerComboBox.Size = new System.Drawing.Size(288, 21);
////            this.audioPlayerComboBox.TabIndex = 2;
////            // 
////            // label4
////            // 
////            this.label4.Location = new System.Drawing.Point(0, 0);
////            this.label4.Name = "label4";
////            this.label4.Size = new System.Drawing.Size(100, 23);
////            this.label4.TabIndex = 0;
////            // 
////            // mpGroupBox2
////            // 
////            this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
////                        | System.Windows.Forms.AnchorStyles.Right)));
////            this.mpGroupBox2.Controls.Add(this.labelAutoPlay);
////            this.mpGroupBox2.Controls.Add(this.autoPlayComboBox);
////            this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
////            this.mpGroupBox2.Location = new System.Drawing.Point(0, 226);
////            this.mpGroupBox2.Name = "mpGroupBox2";
////            this.mpGroupBox2.Size = new System.Drawing.Size(472, 64);
////            this.mpGroupBox2.TabIndex = 2;
////            this.mpGroupBox2.TabStop = false;
////            this.mpGroupBox2.Text = "Autoplay";
////            // 
////            // labelAutoPlay
////            // 
////            this.labelAutoPlay.AutoSize = true;
////            this.labelAutoPlay.Location = new System.Drawing.Point(16, 28);
////            this.labelAutoPlay.Name = "labelAutoPlay";
////            this.labelAutoPlay.Size = new System.Drawing.Size(69, 13);
////            this.labelAutoPlay.TabIndex = 0;
////            this.labelAutoPlay.Text = "Autoplay CD:";
////            // 
////            // autoPlayComboBox
////            // 
////            this.autoPlayComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
////            this.autoPlayComboBox.Location = new System.Drawing.Point(162, 24);
////            this.autoPlayComboBox.Name = "autoPlayComboBox";
////            this.autoPlayComboBox.Size = new System.Drawing.Size(288, 21);
////            this.autoPlayComboBox.TabIndex = 1;
////            // 
////            // Music
////            // 
////            this.Controls.Add(this.mpGroupBox2);
////            this.Controls.Add(this.mpGroupBox1);
////            this.Controls.Add(this.groupBox1);
////            this.Name = "Music";
////            this.Size = new System.Drawing.Size(472, 408);
////            this.groupBox1.ResumeLayout(false);
////            this.groupBox1.PerformLayout();
////            this.mpGroupBox1.ResumeLayout(false);
////            this.mpGroupBox1.PerformLayout();
////            this.mpGroupBox2.ResumeLayout(false);
////            this.mpGroupBox2.PerformLayout();
////            this.ResumeLayout(false);

////        }
////        #endregion

////        /// <summary>
////        /// 
////        /// </summary>
////        /// <param name="sender"></param>
////        /// <param name="e"></param>
////        private void playlistButton_Click(object sender, System.EventArgs e)
////        {
////            using (folderBrowserDialog = new FolderBrowserDialog())
////            {
////                folderBrowserDialog.Description = "Select the folder where music playlists will be stored";
////                folderBrowserDialog.ShowNewFolderButton = true;
////                folderBrowserDialog.SelectedPath = playlistFolderTextBox.Text;
////                DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

////                if (dialogResult == DialogResult.OK)
////                {
////                    playlistFolderTextBox.Text = folderBrowserDialog.SelectedPath;
////                }
////            }
////        }

////        private void showID3CheckBox_CheckedChanged(object sender, System.EventArgs e)
////        {

////        }
////        private void Yestext_Click(object sender, System.EventArgs e)
////        {

////        }

////        private void label3_Click(object sender, System.EventArgs e)
////        {

////        }
////    }
////}

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Util;
using MediaPortal.Visualization;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class Music : MediaPortal.Configuration.SectionSettings
  {
    private delegate void LoadVisualizationListDelegate(List<VisualizationInfo> vizPluginsInfo);

    #region Variables

    private const string JumpToValue0 = "none";
    private const string JumpToValue1 = "nowPlayingAlways";
    private const string JumpToValue2 = "nowPlayingMultipleItems";
    private const string JumpToValue3 = "currentPlaylistAlways";
    private const string JumpToValue4 = "currentPlaylistMultipleItems";
    private const string JumpToValue5 = "fullscreenAlways";
    private const string JumpToValue6 = "fullscreenMultipleItems";

    private const string JumpToOption0 = "None";
    private const string JumpToOption1 = "Now Playing [always]";
    private const string JumpToOption2 = "Now Playing [if multiple items]";
    private const string JumpToOption3 = "Current playlist [always]";
    private const string JumpToOption4 = "Current playlist [if multiple items]";
    private const string JumpToOption5 = "Fullscreen [always] (internal music player only)";
    private const string JumpToOption6 = "Fullscreen [if multiple items] (internal music player only)";

    string[] JumpToValues = new string[] { 
            JumpToValue0,
	        JumpToValue1,
            JumpToValue2,
            JumpToValue3,
            JumpToValue4,
            JumpToValue5,
            JumpToValue6,
        };

    string[] JumpToOptions = new string[] { 
            JumpToOption0,
            JumpToOption1,
            JumpToOption2,
            JumpToOption3,
            JumpToOption4,
            JumpToOption5,
            JumpToOption6,
        };

    string[] autoPlayOptions = new string[] { 
            "Autoplay, never ask", 
            "Don't autoplay, never ask", 
            "Ask every time a CD is inserted" 
        };

    string[] PlayerOptions = new string[]{
            "Internal Music Player",
            "Windows Media Player 9",
            "DirectShow" 
        };

    private const string LyricsValue0 = "never";
    private const string LyricsValue1 = "asOverlay";
    private const string LyricsValue2 = "asVisualCue";

    private const string LyricsOption0 = "Never";
    private const string LyricsOption1 = "Display as an overlay";
    private const string LyricsOption2 = "Show visual cue that lyrics are available";

    string[] ShowLyricsOptions = new string[]{
            LyricsOption0,
            LyricsOption1,
            LyricsOption2
        };

    private Visualization.IVisualizationManager IVizMgr = null;
    private VisualizationInfo VizPluginInfo = null;
    private bool VisualizationsInitialized = false;
    private bool SuppressVisualizationRestart = false;
    private MediaPortal.Visualization.VisualizationWindow VizWindow;

    #endregion

    #region Controls

    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private System.ComponentModel.IContainer components = null;
    private MediaPortal.UserInterface.Controls.MPTabControl MusicSettingsTabCtl;
    private TabPage PlayerTabPg;
    private MediaPortal.UserInterface.Controls.MPGroupBox PlaybackSettingsGrpBox;
    private Label BufferingSecondsLbl;
    private Label CrossFadeSecondsLbl;
    private NumericUpDown StreamOutputLevelNud;
    private CheckBox FadeOnStartStopChkbox;
    private Label label12;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel CrossFadingLbl;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private CheckBox enableVisualisation;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPCheckBox showID3CheckBox;
    private MediaPortal.UserInterface.Controls.MPComboBox audioPlayerComboBox;
    private TabPage VisualizationsTabPg;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
    private LinkLabel SoundSpectrumLnkLbl;
    private MediaPortal.UserInterface.Controls.MPCheckBox EnableStatusOverlaysChkBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox ShowTrackInfoChkBox;
    private MediaPortal.UserInterface.Controls.MPLabel VizPlaceHolderLbl;
    private Label label11;
    private Label label10;
    private ComboBox VizPresetsCmbBox;
    private ComboBox VisualizationsCmbBox;
    private Label label6;
    private Label label7;
    private Label label5;
    private NumericUpDown VisualizationFpsNud;
    private TabPage PlaylistTabPg;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox autoShuffleCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox ResumePlaylistChkBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox SavePlaylistOnExitChkBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox repeatPlaylistCheckBox;
    private MediaPortal.UserInterface.Controls.MPButton playlistButton;
    private MediaPortal.UserInterface.Controls.MPTextBox playlistFolderTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private TabPage MiscTabPg;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox4;
    private MediaPortal.UserInterface.Controls.MPCheckBox ShowVizInNowPlayingChkBox;
    private ComboBox ShowLyricsCmbBox;
    private Label label9;
    private GroupBox groupBox2;
    private ComboBox PlayNowJumpToCmbBox;
    private Label label8;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPLabel labelAutoPlay;
    private CheckBox GaplessPlaybackChkBox;
    private HScrollBar hScrollBarCrossFade;
    private HScrollBar hScrollBarBuffering;
    private MediaPortal.UserInterface.Controls.MPComboBox autoPlayComboBox;

    #endregion

    public Music()
      : this("Music")
    {
    }

    public Music(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // Set available media players
      audioPlayerComboBox.Items.Clear();
      audioPlayerComboBox.Items.AddRange(PlayerOptions);

      autoPlayComboBox.Items.Clear();
      autoPlayComboBox.Items.AddRange(autoPlayOptions);

      PlayNowJumpToCmbBox.Items.Clear();
      PlayNowJumpToCmbBox.Items.AddRange(JumpToOptions);

      ShowLyricsCmbBox.Items.Clear();
      ShowLyricsCmbBox.Items.AddRange(ShowLyricsOptions);
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      hScrollBarBuffering_ValueChanged(null, null);
      hScrollBarCrossFade_ValueChanged(null, null);
      audioPlayerComboBox_SelectedIndexChanged(null, null);
      GaplessPlaybackChkBox_CheckedChanged(null, null);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        // Player Settings
        audioPlayerComboBox.Text = xmlreader.GetValueAsString("audioplayer", "player", "Internal Music Player");
        showID3CheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "showid3", false);
        enableVisualisation.Checked = xmlreader.GetValueAsBool("musicfiles", "doVisualisation", true);

        int crossFadeMS = xmlreader.GetValueAsInt("audioplayer", "crossfade", 4000);

        if (crossFadeMS < 0)
          crossFadeMS = 4000;

        else if (crossFadeMS > hScrollBarCrossFade.Maximum)
          crossFadeMS = hScrollBarCrossFade.Maximum;

        hScrollBarCrossFade.Value = crossFadeMS;

        int bufferingMS = xmlreader.GetValueAsInt("audioplayer", "buffering", 5000);

        if (bufferingMS < hScrollBarBuffering.Minimum)
          bufferingMS = hScrollBarBuffering.Minimum;

        else if (bufferingMS > hScrollBarBuffering.Maximum)
          bufferingMS = hScrollBarBuffering.Maximum;

        hScrollBarBuffering.Value = bufferingMS;

        GaplessPlaybackChkBox.Checked = xmlreader.GetValueAsBool("audioplayer", "gaplessPlayback", false);
        FadeOnStartStopChkbox.Checked = xmlreader.GetValueAsBool("audioplayer", "fadeOnStartStop", true);
        StreamOutputLevelNud.Value = (decimal)xmlreader.GetValueAsInt("audioplayer", "streamOutputLevel", 85);

        // Visualization Settings
        int vizType = xmlreader.GetValueAsInt("musicvisualization", "vizType", (int)VisualizationInfo.PluginType.None);
        string vizName = xmlreader.GetValueAsString("musicvisualization", "name", "None");
        string vizPath = xmlreader.GetValueAsString("musicvisualization", "path", "");
        string vizClsid = xmlreader.GetValueAsString("musicvisualization", "clsid", "");
        int vizPreset = xmlreader.GetValueAsInt("musicvisualization", "preset", 0);

        if (vizType == (int)VisualizationInfo.PluginType.None
            && vizName == "None")
        {
          VizPluginInfo = new VisualizationInfo("None", true);
        }

        else
          VizPluginInfo = new VisualizationInfo((VisualizationInfo.PluginType)vizType, vizPath, vizName, vizClsid, vizPreset);

        int fps = xmlreader.GetValueAsInt("musicvisualization", "fps", 30);

        if (fps < (int)VisualizationFpsNud.Minimum)
          fps = (int)VisualizationFpsNud.Minimum;

        else if (fps > VisualizationFpsNud.Maximum)
          fps = (int)VisualizationFpsNud.Maximum;

        VisualizationFpsNud.Value = fps;

        EnableStatusOverlaysChkBox.Checked = xmlreader.GetValueAsBool("musicvisualization", "enableStatusOverlays", false);
        ShowTrackInfoChkBox.Checked = xmlreader.GetValueAsBool("musicvisualization", "showTrackInfo", true);
        EnableStatusOverlaysChkBox_CheckedChanged(null, null);

        // Playlist Settings
        string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        playListFolder += @"\My Playlists";
        playlistFolderTextBox.Text = xmlreader.GetValueAsString("music", "playlists", playListFolder);

        if (string.Compare(playlistFolderTextBox.Text, playListFolder) == 0)
        {
          if (System.IO.Directory.Exists(playListFolder) == false)
          {
            try
            {
              System.IO.Directory.CreateDirectory(playListFolder);
            }
            catch (Exception) { }
          }
        }

        repeatPlaylistCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "repeat", true);
        autoShuffleCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", true);

        SavePlaylistOnExitChkBox.Checked = xmlreader.GetValueAsBool("musicfiles", "savePlaylistOnExit", true);
        ResumePlaylistChkBox.Checked = xmlreader.GetValueAsBool("musicfiles", "resumePlaylistOnMusicEnter", true);


        // Misc Settings
        string autoPlayText = xmlreader.GetValueAsString("audioplayer", "autoplay", "Yes");

        switch (autoPlayText)
        {
          case "No":
            autoPlayComboBox.Text = autoPlayOptions[1];
            break;

          case "Ask":
            autoPlayComboBox.Text = autoPlayOptions[2];
            break;

          default:
            autoPlayComboBox.Text = autoPlayOptions[0];
            break;
        }

        string playNowJumpTo = xmlreader.GetValueAsString("musicmisc", "playnowjumpto", JumpToValue0);

        switch (playNowJumpTo)
        {
          case JumpToValue0:
            PlayNowJumpToCmbBox.Text = JumpToOptions[0];
            break;

          case JumpToValue1:
            PlayNowJumpToCmbBox.Text = JumpToOptions[1];
            break;

          case JumpToValue2:
            PlayNowJumpToCmbBox.Text = JumpToOptions[2];
            break;

          case JumpToValue3:
            PlayNowJumpToCmbBox.Text = JumpToOptions[3];
            break;

          case JumpToValue4:
            PlayNowJumpToCmbBox.Text = JumpToOptions[4];
            break;

          case JumpToValue5:
            PlayNowJumpToCmbBox.Text = JumpToOptions[5];
            break;

          case JumpToValue6:
            PlayNowJumpToCmbBox.Text = JumpToOptions[6];
            break;

          default:
            PlayNowJumpToCmbBox.Text = JumpToOptions[0];
            break;
        }

        string showLyrics = xmlreader.GetValueAsString("musicmisc", "lyrics", LyricsValue0);

        switch (showLyrics)
        {
          case LyricsValue0:
            ShowLyricsCmbBox.Text = ShowLyricsOptions[0];
            break;

          case LyricsValue1:
            ShowLyricsCmbBox.Text = ShowLyricsOptions[1];
            break;

          case LyricsValue2:
            ShowLyricsCmbBox.Text = ShowLyricsOptions[2];
            break;

          default:
            ShowLyricsCmbBox.Text = ShowLyricsOptions[0];
            break;
        }

        ShowVizInNowPlayingChkBox.Checked = xmlreader.GetValueAsBool("musicmisc", "showVisInNowPlaying", true);
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        // Player Settings
        xmlwriter.SetValue("audioplayer", "player", audioPlayerComboBox.Text);
        xmlwriter.SetValueAsBool("musicfiles", "showid3", showID3CheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "doVisualisation", enableVisualisation.Checked);

        xmlwriter.SetValue("audioplayer", "crossfade", hScrollBarCrossFade.Value);
        xmlwriter.SetValue("audioplayer", "buffering", hScrollBarBuffering.Value);
        xmlwriter.SetValueAsBool("audioplayer", "fadeOnStartStop", FadeOnStartStopChkbox.Checked);
        xmlwriter.SetValueAsBool("audioplayer", "gaplessPlayback", GaplessPlaybackChkBox.Checked);
        xmlwriter.SetValue("audioplayer", "streamOutputLevel", StreamOutputLevelNud.Value);

        // Visualization Settings
        if (IVizMgr != null)
        {
          List<VisualizationInfo> vizPluginsInfo = IVizMgr.VisualizationPluginsInfo;
          int selIndex = VisualizationsCmbBox.SelectedIndex;

          if (selIndex < 0 || selIndex >= vizPluginsInfo.Count)
            selIndex = 0;

          xmlwriter.SetValue("musicvisualization", "name", vizPluginsInfo[selIndex].Name);
          xmlwriter.SetValue("musicvisualization", "vizType", ((int)vizPluginsInfo[selIndex].VisualizationType).ToString());
          xmlwriter.SetValue("musicvisualization", "path", vizPluginsInfo[selIndex].FilePath);
          xmlwriter.SetValue("musicvisualization", "clsid", vizPluginsInfo[selIndex].CLSID);
          xmlwriter.SetValue("musicvisualization", "preset", vizPluginsInfo[selIndex].PresetIndex.ToString());
        }

        xmlwriter.SetValue("musicvisualization", "fps", VisualizationFpsNud.Value);

        xmlwriter.SetValueAsBool("musicvisualization", "enableStatusOverlays", EnableStatusOverlaysChkBox.Checked);
        xmlwriter.SetValueAsBool("musicvisualization", "showTrackInfo", ShowTrackInfoChkBox.Checked);


        // Playlist Settings
        xmlwriter.SetValue("music", "playlists", playlistFolderTextBox.Text);
        xmlwriter.SetValueAsBool("musicfiles", "repeat", repeatPlaylistCheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "autoshuffle", autoShuffleCheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "savePlaylistOnExit", SavePlaylistOnExitChkBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "resumePlaylistOnMusicEnter", ResumePlaylistChkBox.Checked);


        // Misc Settings
        string autoPlayText;

        if (autoPlayComboBox.Text == autoPlayOptions[1])
          autoPlayText = "No";

        else if (autoPlayComboBox.Text == autoPlayOptions[2])
          autoPlayText = "Ask";

        else
          autoPlayText = "Yes";

        xmlwriter.SetValue("audioplayer", "autoplay", autoPlayText);

        string playNowJumpTo = string.Empty;

        switch (PlayNowJumpToCmbBox.Text)
        {
          case JumpToOption0:
            playNowJumpTo = JumpToValue0;
            break;

          case JumpToOption1:
            playNowJumpTo = JumpToValue1;
            break;

          case JumpToOption2:
            playNowJumpTo = JumpToValue2;
            break;

          case JumpToOption3:
            playNowJumpTo = JumpToValue3;
            break;

          case JumpToOption4:
            playNowJumpTo = JumpToValue4;
            break;

          case JumpToOption5:
            playNowJumpTo = JumpToValue5;
            break;

          case JumpToOption6:
            playNowJumpTo = JumpToValue6;
            break;

          default:
            playNowJumpTo = JumpToValue0;
            break;
        }

        xmlwriter.SetValue("musicmisc", "playnowjumpto", playNowJumpTo);

        string showLyrics = string.Empty;

        switch (ShowLyricsCmbBox.Text)
        {
          case LyricsOption0:
            showLyrics = LyricsValue0;
            break;

          case LyricsOption1:
            showLyrics = LyricsValue1;
            break;

          case LyricsOption2:
            showLyrics = LyricsValue2;
            break;
        }

        xmlwriter.SetValue("musicmisc", "lyrics", showLyrics);
        xmlwriter.SetValueAsBool("musicmisc", "showVisInNowPlaying", ShowVizInNowPlayingChkBox.Checked);
      }

      // Make sure we shut down the viz engine
      if (IVizMgr != null)
      {
        IVizMgr.Stop();
        IVizMgr.ShutDown();
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

        // Make sure we shut down the viz engine
        if (IVizMgr != null)
        {
          IVizMgr.Stop();
          IVizMgr.ShutDown();
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
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.MusicSettingsTabCtl = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.PlayerTabPg = new System.Windows.Forms.TabPage();
      this.PlaybackSettingsGrpBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.hScrollBarBuffering = new System.Windows.Forms.HScrollBar();
      this.hScrollBarCrossFade = new System.Windows.Forms.HScrollBar();
      this.GaplessPlaybackChkBox = new System.Windows.Forms.CheckBox();
      this.BufferingSecondsLbl = new System.Windows.Forms.Label();
      this.CrossFadeSecondsLbl = new System.Windows.Forms.Label();
      this.StreamOutputLevelNud = new System.Windows.Forms.NumericUpDown();
      this.FadeOnStartStopChkbox = new System.Windows.Forms.CheckBox();
      this.label12 = new System.Windows.Forms.Label();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.CrossFadingLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.enableVisualisation = new System.Windows.Forms.CheckBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.showID3CheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.audioPlayerComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.VisualizationsTabPg = new System.Windows.Forms.TabPage();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.SoundSpectrumLnkLbl = new System.Windows.Forms.LinkLabel();
      this.EnableStatusOverlaysChkBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.ShowTrackInfoChkBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.VizPlaceHolderLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label11 = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.VizPresetsCmbBox = new System.Windows.Forms.ComboBox();
      this.VisualizationsCmbBox = new System.Windows.Forms.ComboBox();
      this.label6 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.VisualizationFpsNud = new System.Windows.Forms.NumericUpDown();
      this.PlaylistTabPg = new System.Windows.Forms.TabPage();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.autoShuffleCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.ResumePlaylistChkBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.SavePlaylistOnExitChkBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.repeatPlaylistCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.playlistButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.playlistFolderTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.MiscTabPg = new System.Windows.Forms.TabPage();
      this.mpGroupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.ShowVizInNowPlayingChkBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.ShowLyricsCmbBox = new System.Windows.Forms.ComboBox();
      this.label9 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.PlayNowJumpToCmbBox = new System.Windows.Forms.ComboBox();
      this.label8 = new System.Windows.Forms.Label();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelAutoPlay = new MediaPortal.UserInterface.Controls.MPLabel();
      this.autoPlayComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.MusicSettingsTabCtl.SuspendLayout();
      this.PlayerTabPg.SuspendLayout();
      this.PlaybackSettingsGrpBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.StreamOutputLevelNud)).BeginInit();
      this.mpGroupBox1.SuspendLayout();
      this.VisualizationsTabPg.SuspendLayout();
      this.mpGroupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.VisualizationFpsNud)).BeginInit();
      this.PlaylistTabPg.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.MiscTabPg.SuspendLayout();
      this.mpGroupBox4.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(0, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(100, 23);
      this.label4.TabIndex = 0;
      // 
      // MusicSettingsTabCtl
      // 
      this.MusicSettingsTabCtl.Controls.Add(this.PlayerTabPg);
      this.MusicSettingsTabCtl.Controls.Add(this.VisualizationsTabPg);
      this.MusicSettingsTabCtl.Controls.Add(this.PlaylistTabPg);
      this.MusicSettingsTabCtl.Controls.Add(this.MiscTabPg);
      this.MusicSettingsTabCtl.Location = new System.Drawing.Point(0, 8);
      this.MusicSettingsTabCtl.Name = "MusicSettingsTabCtl";
      this.MusicSettingsTabCtl.SelectedIndex = 0;
      this.MusicSettingsTabCtl.Size = new System.Drawing.Size(472, 400);
      this.MusicSettingsTabCtl.TabIndex = 1;
      this.MusicSettingsTabCtl.SelectedIndexChanged += new System.EventHandler(this.MusicSettingsTabCtl_SelectedIndexChanged);
      // 
      // PlayerTabPg
      // 
      this.PlayerTabPg.Controls.Add(this.PlaybackSettingsGrpBox);
      this.PlayerTabPg.Controls.Add(this.mpGroupBox1);
      this.PlayerTabPg.Location = new System.Drawing.Point(4, 22);
      this.PlayerTabPg.Name = "PlayerTabPg";
      this.PlayerTabPg.Padding = new System.Windows.Forms.Padding(3);
      this.PlayerTabPg.Size = new System.Drawing.Size(464, 374);
      this.PlayerTabPg.TabIndex = 1;
      this.PlayerTabPg.Text = "Player Settings";
      this.PlayerTabPg.UseVisualStyleBackColor = true;
      // 
      // PlaybackSettingsGrpBox
      // 
      this.PlaybackSettingsGrpBox.Controls.Add(this.hScrollBarBuffering);
      this.PlaybackSettingsGrpBox.Controls.Add(this.hScrollBarCrossFade);
      this.PlaybackSettingsGrpBox.Controls.Add(this.GaplessPlaybackChkBox);
      this.PlaybackSettingsGrpBox.Controls.Add(this.BufferingSecondsLbl);
      this.PlaybackSettingsGrpBox.Controls.Add(this.CrossFadeSecondsLbl);
      this.PlaybackSettingsGrpBox.Controls.Add(this.StreamOutputLevelNud);
      this.PlaybackSettingsGrpBox.Controls.Add(this.FadeOnStartStopChkbox);
      this.PlaybackSettingsGrpBox.Controls.Add(this.label12);
      this.PlaybackSettingsGrpBox.Controls.Add(this.mpLabel1);
      this.PlaybackSettingsGrpBox.Controls.Add(this.CrossFadingLbl);
      this.PlaybackSettingsGrpBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.PlaybackSettingsGrpBox.Location = new System.Drawing.Point(16, 144);
      this.PlaybackSettingsGrpBox.Name = "PlaybackSettingsGrpBox";
      this.PlaybackSettingsGrpBox.Size = new System.Drawing.Size(432, 180);
      this.PlaybackSettingsGrpBox.TabIndex = 1;
      this.PlaybackSettingsGrpBox.TabStop = false;
      this.PlaybackSettingsGrpBox.Text = "Playback Settings (Internal player only)";
      // 
      // hScrollBarBuffering
      // 
      this.hScrollBarBuffering.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.hScrollBarBuffering.LargeChange = 500;
      this.hScrollBarBuffering.Location = new System.Drawing.Point(84, 136);
      this.hScrollBarBuffering.Maximum = 8499;
      this.hScrollBarBuffering.Name = "hScrollBarBuffering";
      this.hScrollBarBuffering.Size = new System.Drawing.Size(248, 17);
      this.hScrollBarBuffering.SmallChange = 100;
      this.hScrollBarBuffering.TabIndex = 11;
      this.hScrollBarBuffering.Value = 5000;
      this.hScrollBarBuffering.ValueChanged += new System.EventHandler(this.hScrollBarBuffering_ValueChanged);
      // 
      // hScrollBarCrossFade
      // 
      this.hScrollBarCrossFade.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.hScrollBarCrossFade.LargeChange = 500;
      this.hScrollBarCrossFade.Location = new System.Drawing.Point(84, 112);
      this.hScrollBarCrossFade.Maximum = 16499;
      this.hScrollBarCrossFade.Name = "hScrollBarCrossFade";
      this.hScrollBarCrossFade.Size = new System.Drawing.Size(248, 17);
      this.hScrollBarCrossFade.SmallChange = 100;
      this.hScrollBarCrossFade.TabIndex = 10;
      this.hScrollBarCrossFade.Value = 4000;
      this.hScrollBarCrossFade.ValueChanged += new System.EventHandler(this.hScrollBarCrossFade_ValueChanged);
      // 
      // GaplessPlaybackChkBox
      // 
      this.GaplessPlaybackChkBox.AutoSize = true;
      this.GaplessPlaybackChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.GaplessPlaybackChkBox.Location = new System.Drawing.Point(87, 82);
      this.GaplessPlaybackChkBox.Name = "GaplessPlaybackChkBox";
      this.GaplessPlaybackChkBox.Size = new System.Drawing.Size(108, 17);
      this.GaplessPlaybackChkBox.TabIndex = 3;
      this.GaplessPlaybackChkBox.Text = "Gapless playback";
      this.GaplessPlaybackChkBox.UseVisualStyleBackColor = true;
      this.GaplessPlaybackChkBox.CheckedChanged += new System.EventHandler(this.GaplessPlaybackChkBox_CheckedChanged);
      // 
      // BufferingSecondsLbl
      // 
      this.BufferingSecondsLbl.Location = new System.Drawing.Point(341, 139);
      this.BufferingSecondsLbl.Name = "BufferingSecondsLbl";
      this.BufferingSecondsLbl.Size = new System.Drawing.Size(80, 13);
      this.BufferingSecondsLbl.TabIndex = 9;
      this.BufferingSecondsLbl.Text = "00.0 Seconds";
      this.BufferingSecondsLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // CrossFadeSecondsLbl
      // 
      this.CrossFadeSecondsLbl.Location = new System.Drawing.Point(341, 115);
      this.CrossFadeSecondsLbl.Name = "CrossFadeSecondsLbl";
      this.CrossFadeSecondsLbl.Size = new System.Drawing.Size(80, 13);
      this.CrossFadeSecondsLbl.TabIndex = 6;
      this.CrossFadeSecondsLbl.Text = "00.0 Seconds";
      this.CrossFadeSecondsLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // StreamOutputLevelNud
      // 
      this.StreamOutputLevelNud.Location = new System.Drawing.Point(87, 32);
      this.StreamOutputLevelNud.Name = "StreamOutputLevelNud";
      this.StreamOutputLevelNud.Size = new System.Drawing.Size(52, 20);
      this.StreamOutputLevelNud.TabIndex = 1;
      this.StreamOutputLevelNud.Value = new decimal(new int[] {
            85,
            0,
            0,
            0});
      // 
      // FadeOnStartStopChkbox
      // 
      this.FadeOnStartStopChkbox.AutoSize = true;
      this.FadeOnStartStopChkbox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.FadeOnStartStopChkbox.Location = new System.Drawing.Point(87, 61);
      this.FadeOnStartStopChkbox.Name = "FadeOnStartStopChkbox";
      this.FadeOnStartStopChkbox.Size = new System.Drawing.Size(185, 17);
      this.FadeOnStartStopChkbox.TabIndex = 2;
      this.FadeOnStartStopChkbox.Text = "Fade-in on start / fade-out on stop";
      this.FadeOnStartStopChkbox.UseVisualStyleBackColor = true;
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(31, 139);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(52, 13);
      this.label12.TabIndex = 7;
      this.label12.Text = "Buffering:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(16, 34);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(67, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Output level:";
      // 
      // CrossFadingLbl
      // 
      this.CrossFadingLbl.AutoSize = true;
      this.CrossFadingLbl.Location = new System.Drawing.Point(15, 115);
      this.CrossFadingLbl.Name = "CrossFadingLbl";
      this.CrossFadingLbl.Size = new System.Drawing.Size(68, 13);
      this.CrossFadingLbl.TabIndex = 4;
      this.CrossFadingLbl.Text = "Cross-fading:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.enableVisualisation);
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.Controls.Add(this.showID3CheckBox);
      this.mpGroupBox1.Controls.Add(this.audioPlayerComboBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(432, 120);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "General Settings";
      // 
      // enableVisualisation
      // 
      this.enableVisualisation.AutoSize = true;
      this.enableVisualisation.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.enableVisualisation.Location = new System.Drawing.Point(87, 89);
      this.enableVisualisation.Name = "enableVisualisation";
      this.enableVisualisation.Size = new System.Drawing.Size(265, 17);
      this.enableVisualisation.TabIndex = 3;
      this.enableVisualisation.Text = "Enable visualization (Internal player and WMP only)";
      this.enableVisualisation.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(44, 36);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(39, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Player:";
      // 
      // showID3CheckBox
      // 
      this.showID3CheckBox.AutoSize = true;
      this.showID3CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.showID3CheckBox.Location = new System.Drawing.Point(87, 68);
      this.showID3CheckBox.Name = "showID3CheckBox";
      this.showID3CheckBox.Size = new System.Drawing.Size(289, 17);
      this.showID3CheckBox.TabIndex = 2;
      this.showID3CheckBox.Text = "Load ID3 tags from file if it\'s not in music database (slow)";
      this.showID3CheckBox.UseVisualStyleBackColor = true;
      // 
      // audioPlayerComboBox
      // 
      this.audioPlayerComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioPlayerComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioPlayerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioPlayerComboBox.Location = new System.Drawing.Point(87, 32);
      this.audioPlayerComboBox.Name = "audioPlayerComboBox";
      this.audioPlayerComboBox.Size = new System.Drawing.Size(289, 21);
      this.audioPlayerComboBox.TabIndex = 1;
      this.audioPlayerComboBox.SelectedIndexChanged += new System.EventHandler(this.audioPlayerComboBox_SelectedIndexChanged);
      // 
      // VisualizationsTabPg
      // 
      this.VisualizationsTabPg.Controls.Add(this.mpGroupBox3);
      this.VisualizationsTabPg.Location = new System.Drawing.Point(4, 22);
      this.VisualizationsTabPg.Name = "VisualizationsTabPg";
      this.VisualizationsTabPg.Size = new System.Drawing.Size(464, 374);
      this.VisualizationsTabPg.TabIndex = 4;
      this.VisualizationsTabPg.Text = "Visualizations";
      this.VisualizationsTabPg.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Controls.Add(this.SoundSpectrumLnkLbl);
      this.mpGroupBox3.Controls.Add(this.EnableStatusOverlaysChkBox);
      this.mpGroupBox3.Controls.Add(this.ShowTrackInfoChkBox);
      this.mpGroupBox3.Controls.Add(this.VizPlaceHolderLbl);
      this.mpGroupBox3.Controls.Add(this.label11);
      this.mpGroupBox3.Controls.Add(this.label10);
      this.mpGroupBox3.Controls.Add(this.VizPresetsCmbBox);
      this.mpGroupBox3.Controls.Add(this.VisualizationsCmbBox);
      this.mpGroupBox3.Controls.Add(this.label6);
      this.mpGroupBox3.Controls.Add(this.label7);
      this.mpGroupBox3.Controls.Add(this.label5);
      this.mpGroupBox3.Controls.Add(this.VisualizationFpsNud);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(432, 343);
      this.mpGroupBox3.TabIndex = 0;
      this.mpGroupBox3.TabStop = false;
      // 
      // SoundSpectrumLnkLbl
      // 
      this.SoundSpectrumLnkLbl.AutoSize = true;
      this.SoundSpectrumLnkLbl.Location = new System.Drawing.Point(105, 242);
      this.SoundSpectrumLnkLbl.Name = "SoundSpectrumLnkLbl";
      this.SoundSpectrumLnkLbl.Size = new System.Drawing.Size(256, 13);
      this.SoundSpectrumLnkLbl.TabIndex = 11;
      this.SoundSpectrumLnkLbl.TabStop = true;
      this.SoundSpectrumLnkLbl.Text = "Get more great visualizations at SoundSpectrum.com";
      this.SoundSpectrumLnkLbl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SoundSpectrumLnkLbl_LinkClicked);
      // 
      // EnableStatusOverlaysChkBox
      // 
      this.EnableStatusOverlaysChkBox.AutoSize = true;
      this.EnableStatusOverlaysChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.EnableStatusOverlaysChkBox.Location = new System.Drawing.Point(105, 293);
      this.EnableStatusOverlaysChkBox.Name = "EnableStatusOverlaysChkBox";
      this.EnableStatusOverlaysChkBox.Size = new System.Drawing.Size(299, 17);
      this.EnableStatusOverlaysChkBox.TabIndex = 9;
      this.EnableStatusOverlaysChkBox.Text = "Enable status display in fullscreen mode (fast systems only)";
      this.EnableStatusOverlaysChkBox.UseVisualStyleBackColor = true;
      this.EnableStatusOverlaysChkBox.CheckedChanged += new System.EventHandler(this.EnableStatusOverlaysChkBox_CheckedChanged);
      // 
      // ShowTrackInfoChkBox
      // 
      this.ShowTrackInfoChkBox.AutoSize = true;
      this.ShowTrackInfoChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ShowTrackInfoChkBox.Location = new System.Drawing.Point(124, 314);
      this.ShowTrackInfoChkBox.Name = "ShowTrackInfoChkBox";
      this.ShowTrackInfoChkBox.Size = new System.Drawing.Size(178, 17);
      this.ShowTrackInfoChkBox.TabIndex = 10;
      this.ShowTrackInfoChkBox.Text = "Show song info on track change";
      this.ShowTrackInfoChkBox.UseVisualStyleBackColor = true;
      // 
      // VizPlaceHolderLbl
      // 
      this.VizPlaceHolderLbl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.VizPlaceHolderLbl.Location = new System.Drawing.Point(105, 72);
      this.VizPlaceHolderLbl.Name = "VizPlaceHolderLbl";
      this.VizPlaceHolderLbl.Size = new System.Drawing.Size(292, 164);
      this.VizPlaceHolderLbl.TabIndex = 5;
      this.VizPlaceHolderLbl.Text = "Visualization window placeholder";
      this.VizPlaceHolderLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(61, 52);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(40, 13);
      this.label11.TabIndex = 2;
      this.label11.Text = "Preset:";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(33, 28);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(68, 13);
      this.label10.TabIndex = 0;
      this.label10.Text = "Visualization:";
      // 
      // VizPresetsCmbBox
      // 
      this.VizPresetsCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.VizPresetsCmbBox.FormattingEnabled = true;
      this.VizPresetsCmbBox.Location = new System.Drawing.Point(105, 48);
      this.VizPresetsCmbBox.Name = "VizPresetsCmbBox";
      this.VizPresetsCmbBox.Size = new System.Drawing.Size(292, 21);
      this.VizPresetsCmbBox.TabIndex = 3;
      this.VizPresetsCmbBox.SelectedIndexChanged += new System.EventHandler(this.VizPresetsCmbBox_SelectedIndexChanged);
      // 
      // VisualizationsCmbBox
      // 
      this.VisualizationsCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.VisualizationsCmbBox.FormattingEnabled = true;
      this.VisualizationsCmbBox.Location = new System.Drawing.Point(105, 24);
      this.VisualizationsCmbBox.Name = "VisualizationsCmbBox";
      this.VisualizationsCmbBox.Size = new System.Drawing.Size(292, 21);
      this.VisualizationsCmbBox.TabIndex = 1;
      this.VisualizationsCmbBox.SelectedIndexChanged += new System.EventHandler(this.VisualizationsCmbBox_SelectedIndexChanged);
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(53, 72);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(48, 13);
      this.label6.TabIndex = 4;
      this.label6.Text = "Preview:";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(163, 267);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(166, 13);
      this.label7.TabIndex = 8;
      this.label7.Text = "(use lower value for slow systems)";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(37, 267);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(64, 13);
      this.label5.TabIndex = 6;
      this.label5.Text = "Target FPS:";
      // 
      // VisualizationFpsNud
      // 
      this.VisualizationFpsNud.Location = new System.Drawing.Point(105, 265);
      this.VisualizationFpsNud.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.VisualizationFpsNud.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.VisualizationFpsNud.Name = "VisualizationFpsNud";
      this.VisualizationFpsNud.Size = new System.Drawing.Size(52, 20);
      this.VisualizationFpsNud.TabIndex = 7;
      this.VisualizationFpsNud.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.VisualizationFpsNud.ValueChanged += new System.EventHandler(this.VisualizationFpsNud_ValueChanged);
      // 
      // PlaylistTabPg
      // 
      this.PlaylistTabPg.Controls.Add(this.groupBox1);
      this.PlaylistTabPg.Location = new System.Drawing.Point(4, 22);
      this.PlaylistTabPg.Name = "PlaylistTabPg";
      this.PlaylistTabPg.Size = new System.Drawing.Size(464, 374);
      this.PlaylistTabPg.TabIndex = 2;
      this.PlaylistTabPg.Text = "Playlist Settings";
      this.PlaylistTabPg.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.autoShuffleCheckBox);
      this.groupBox1.Controls.Add(this.ResumePlaylistChkBox);
      this.groupBox1.Controls.Add(this.SavePlaylistOnExitChkBox);
      this.groupBox1.Controls.Add(this.repeatPlaylistCheckBox);
      this.groupBox1.Controls.Add(this.playlistButton);
      this.groupBox1.Controls.Add(this.playlistFolderTextBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(16, 16);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(432, 169);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // autoShuffleCheckBox
      // 
      this.autoShuffleCheckBox.AutoSize = true;
      this.autoShuffleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.autoShuffleCheckBox.Location = new System.Drawing.Point(88, 83);
      this.autoShuffleCheckBox.Name = "autoShuffleCheckBox";
      this.autoShuffleCheckBox.Size = new System.Drawing.Size(119, 17);
      this.autoShuffleCheckBox.TabIndex = 4;
      this.autoShuffleCheckBox.Text = "Auto shuffle playlists";
      this.autoShuffleCheckBox.UseVisualStyleBackColor = true;
      // 
      // ResumePlaylistChkBox
      // 
      this.ResumePlaylistChkBox.AutoSize = true;
      this.ResumePlaylistChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ResumePlaylistChkBox.Location = new System.Drawing.Point(88, 129);
      this.ResumePlaylistChkBox.Name = "ResumePlaylistChkBox";
      this.ResumePlaylistChkBox.Size = new System.Drawing.Size(234, 17);
      this.ResumePlaylistChkBox.TabIndex = 5;
      this.ResumePlaylistChkBox.Text = "Resume last playlist when entering My Music";
      this.ResumePlaylistChkBox.UseVisualStyleBackColor = true;
      // 
      // SavePlaylistOnExitChkBox
      // 
      this.SavePlaylistOnExitChkBox.AutoSize = true;
      this.SavePlaylistOnExitChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.SavePlaylistOnExitChkBox.Location = new System.Drawing.Point(88, 106);
      this.SavePlaylistOnExitChkBox.Name = "SavePlaylistOnExitChkBox";
      this.SavePlaylistOnExitChkBox.Size = new System.Drawing.Size(211, 17);
      this.SavePlaylistOnExitChkBox.TabIndex = 5;
      this.SavePlaylistOnExitChkBox.Text = "Save playlist and playlist position on exit";
      this.SavePlaylistOnExitChkBox.UseVisualStyleBackColor = true;
      // 
      // repeatPlaylistCheckBox
      // 
      this.repeatPlaylistCheckBox.AutoSize = true;
      this.repeatPlaylistCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.repeatPlaylistCheckBox.Location = new System.Drawing.Point(88, 59);
      this.repeatPlaylistCheckBox.Name = "repeatPlaylistCheckBox";
      this.repeatPlaylistCheckBox.Size = new System.Drawing.Size(219, 17);
      this.repeatPlaylistCheckBox.TabIndex = 3;
      this.repeatPlaylistCheckBox.Text = "Repeat/loop music playlists (m3u, b4, pls)";
      this.repeatPlaylistCheckBox.UseVisualStyleBackColor = true;
      // 
      // playlistButton
      // 
      this.playlistButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.playlistButton.Location = new System.Drawing.Point(352, 30);
      this.playlistButton.Name = "playlistButton";
      this.playlistButton.Size = new System.Drawing.Size(61, 22);
      this.playlistButton.TabIndex = 2;
      this.playlistButton.Text = "Browse";
      this.playlistButton.UseVisualStyleBackColor = true;
      this.playlistButton.Click += new System.EventHandler(this.playlistButton_Click);
      // 
      // playlistFolderTextBox
      // 
      this.playlistFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.playlistFolderTextBox.BorderColor = System.Drawing.Color.Empty;
      this.playlistFolderTextBox.Location = new System.Drawing.Point(88, 31);
      this.playlistFolderTextBox.Name = "playlistFolderTextBox";
      this.playlistFolderTextBox.Size = new System.Drawing.Size(264, 20);
      this.playlistFolderTextBox.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 35);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(71, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Playlist folder:";
      // 
      // MiscTabPg
      // 
      this.MiscTabPg.Controls.Add(this.mpGroupBox4);
      this.MiscTabPg.Controls.Add(this.groupBox2);
      this.MiscTabPg.Controls.Add(this.mpGroupBox2);
      this.MiscTabPg.Location = new System.Drawing.Point(4, 22);
      this.MiscTabPg.Name = "MiscTabPg";
      this.MiscTabPg.Size = new System.Drawing.Size(464, 374);
      this.MiscTabPg.TabIndex = 3;
      this.MiscTabPg.Text = "Misc Settings";
      this.MiscTabPg.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox4
      // 
      this.mpGroupBox4.Controls.Add(this.ShowVizInNowPlayingChkBox);
      this.mpGroupBox4.Controls.Add(this.ShowLyricsCmbBox);
      this.mpGroupBox4.Controls.Add(this.label9);
      this.mpGroupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox4.Location = new System.Drawing.Point(16, 152);
      this.mpGroupBox4.Name = "mpGroupBox4";
      this.mpGroupBox4.Size = new System.Drawing.Size(432, 89);
      this.mpGroupBox4.TabIndex = 2;
      this.mpGroupBox4.TabStop = false;
      this.mpGroupBox4.Text = "Now Playing";
      // 
      // ShowVizInNowPlayingChkBox
      // 
      this.ShowVizInNowPlayingChkBox.AutoSize = true;
      this.ShowVizInNowPlayingChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ShowVizInNowPlayingChkBox.Location = new System.Drawing.Point(114, 19);
      this.ShowVizInNowPlayingChkBox.Name = "ShowVizInNowPlayingChkBox";
      this.ShowVizInNowPlayingChkBox.Size = new System.Drawing.Size(206, 17);
      this.ShowVizInNowPlayingChkBox.TabIndex = 4;
      this.ShowVizInNowPlayingChkBox.Text = "Show visualzation (Internal player only)";
      this.ShowVizInNowPlayingChkBox.UseVisualStyleBackColor = true;
      // 
      // ShowLyricsCmbBox
      // 
      this.ShowLyricsCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.ShowLyricsCmbBox.Enabled = false;
      this.ShowLyricsCmbBox.FormattingEnabled = true;
      this.ShowLyricsCmbBox.Location = new System.Drawing.Point(114, 47);
      this.ShowLyricsCmbBox.Name = "ShowLyricsCmbBox";
      this.ShowLyricsCmbBox.Size = new System.Drawing.Size(293, 21);
      this.ShowLyricsCmbBox.TabIndex = 1;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Enabled = false;
      this.label9.Location = new System.Drawing.Point(47, 50);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(63, 13);
      this.label9.TabIndex = 0;
      this.label9.Text = "Show lyrics:";
      this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.PlayNowJumpToCmbBox);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Location = new System.Drawing.Point(16, 84);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(432, 64);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Play Now Behavior";
      // 
      // PlayNowJumpToCmbBox
      // 
      this.PlayNowJumpToCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.PlayNowJumpToCmbBox.FormattingEnabled = true;
      this.PlayNowJumpToCmbBox.Location = new System.Drawing.Point(114, 25);
      this.PlayNowJumpToCmbBox.Name = "PlayNowJumpToCmbBox";
      this.PlayNowJumpToCmbBox.Size = new System.Drawing.Size(293, 21);
      this.PlayNowJumpToCmbBox.TabIndex = 1;
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(12, 28);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(98, 13);
      this.label8.TabIndex = 0;
      this.label8.Text = "Jump on Play Now:";
      this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.labelAutoPlay);
      this.mpGroupBox2.Controls.Add(this.autoPlayComboBox);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(432, 64);
      this.mpGroupBox2.TabIndex = 0;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Autoplay";
      // 
      // labelAutoPlay
      // 
      this.labelAutoPlay.AutoSize = true;
      this.labelAutoPlay.Location = new System.Drawing.Point(41, 29);
      this.labelAutoPlay.Name = "labelAutoPlay";
      this.labelAutoPlay.Size = new System.Drawing.Size(69, 13);
      this.labelAutoPlay.TabIndex = 0;
      this.labelAutoPlay.Text = "Autoplay CD:";
      // 
      // autoPlayComboBox
      // 
      this.autoPlayComboBox.BorderColor = System.Drawing.Color.Empty;
      this.autoPlayComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.autoPlayComboBox.Location = new System.Drawing.Point(114, 25);
      this.autoPlayComboBox.Name = "autoPlayComboBox";
      this.autoPlayComboBox.Size = new System.Drawing.Size(293, 21);
      this.autoPlayComboBox.TabIndex = 1;
      // 
      // Music
      // 
      this.Controls.Add(this.MusicSettingsTabCtl);
      this.Name = "Music";
      this.Size = new System.Drawing.Size(472, 408);
      this.MusicSettingsTabCtl.ResumeLayout(false);
      this.PlayerTabPg.ResumeLayout(false);
      this.PlaybackSettingsGrpBox.ResumeLayout(false);
      this.PlaybackSettingsGrpBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.StreamOutputLevelNud)).EndInit();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.VisualizationsTabPg.ResumeLayout(false);
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.VisualizationFpsNud)).EndInit();
      this.PlaylistTabPg.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.MiscTabPg.ResumeLayout(false);
      this.mpGroupBox4.ResumeLayout(false);
      this.mpGroupBox4.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void playlistButton_Click(object sender, System.EventArgs e)
    {
      using (folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = "Select the folder where music playlists will be stored";
        folderBrowserDialog.ShowNewFolderButton = true;
        folderBrowserDialog.SelectedPath = playlistFolderTextBox.Text;
        DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          playlistFolderTextBox.Text = folderBrowserDialog.SelectedPath;
        }
      }
    }

    private void MusicSettingsTabCtl_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (MusicSettingsTabCtl.SelectedTab.Equals(VisualizationsTabPg))
      {
        if (audioPlayerComboBox.SelectedIndex == 0)
        {
          VisualizationsTabPg.Enabled = true;

          if (!VisualizationsInitialized)
          {
            VizPlaceHolderLbl.Text = "Getting installed visualizations. This may\r\ntake a minute Please wait...";
            Application.DoEvents();

            InitializeVizEngine();
          }

          else
          {
            mpGroupBox3.Controls.Add(VizWindow);
            mpGroupBox3.Controls.Remove(VizPlaceHolderLbl);

            VizWindow.Text = "";
            VizWindow.BackColor = Color.Black;
          }
        }

        else
        {
          mpGroupBox3.Controls.Add(VizPlaceHolderLbl);
          mpGroupBox3.Controls.Remove(VizWindow);

          VizPlaceHolderLbl.Text = "Not available.";

          VisualizationsTabPg.Enabled = false;
          MessageBox.Show(this, "Visualization settings are only available with the Internal Music Player.",
              "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
    }

    private void audioPlayerComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool useInternalPlayer = audioPlayerComboBox.SelectedIndex == 0;
      PlaybackSettingsGrpBox.Enabled = useInternalPlayer;
    }

    private void hScrollBarCrossFade_ValueChanged(object sender, EventArgs e)
    {
      float xFadeSecs = 0;

      if (hScrollBarCrossFade.Value > 0)
        xFadeSecs = (float)hScrollBarCrossFade.Value / 1000f;

      CrossFadeSecondsLbl.Text = string.Format("{0:f2} Seconds", xFadeSecs);
    }

    private void hScrollBarBuffering_ValueChanged(object sender, EventArgs e)
    {
      float bufferingSecs = 0;

      if (hScrollBarBuffering.Value > 0)
        bufferingSecs = (float)hScrollBarBuffering.Value / 1000f;

      BufferingSecondsLbl.Text = string.Format("{0:f2} Seconds", bufferingSecs);
    }

    private void VisualizationsCmbBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      int selIndex = VisualizationsCmbBox.SelectedIndex;

      if (VisualizationsCmbBox.Items.Count <= 1 || selIndex < 0)
        return;

      if (IVizMgr == null)
        return;

      // Stop the visualization
      VizWindow.Run = false;

      VizPluginInfo = (VisualizationInfo)VisualizationsCmbBox.SelectedItem;

      if (VizPluginInfo == null || VizPluginInfo.IsDummyPlugin)
        return;

      bool vizCreated = IVizMgr.CreatePreviewVisualization(VizPluginInfo);

      if (!vizCreated)
      {
        string msg = string.Format("Unable to load the following visualization:\r\nName:{0}\r\nCLSID:{1}\r\nType:{2}\r\nPreset Count:{3}",
            VizPluginInfo.Name, VizPluginInfo.CLSID, VizPluginInfo.VisualizationType, VizPluginInfo.PresetCount);

        MessageBox.Show(this, msg, "Visualization Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }

      else
      {
        VizPresetsCmbBox.Items.Clear();

        if (VizPluginInfo.HasPresets)
        {
          VizPresetsCmbBox.Items.AddRange(VizPluginInfo.PresetNames.ToArray());
          SuppressVisualizationRestart = true;
          VizPresetsCmbBox.SelectedIndex = VizPluginInfo.PresetIndex;
          VizPresetsCmbBox.Enabled = VizPresetsCmbBox.Items.Count > 1;
          SuppressVisualizationRestart = false;
        }

        else
          VizPresetsCmbBox.Enabled = false;

        // Force a Resize event to ensure the viz engine is notified of window size
        IVizMgr.ResizeVisualizationWindow(VizWindow.Size);
        VizWindow.Run = true;
      }
    }

    private void VizPresetsCmbBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (VizPluginInfo == null)
        return;

      if (VizPresetsCmbBox.SelectedIndex == -1)
        return;

      int selIndex = VizPresetsCmbBox.SelectedIndex;

      if (selIndex < 0 || selIndex >= VizPluginInfo.PresetCount)
        selIndex = 0;

      VizPluginInfo.PresetIndex = selIndex;
      bool vizCreated = false;

      if (!SuppressVisualizationRestart)
      {
        VizWindow.Run = false;
        vizCreated = IVizMgr.CreatePreviewVisualization(VizPluginInfo);
        SuppressVisualizationRestart = false;
        VizWindow.Run = true;
      }
    }

    private void SoundSpectrumLnkLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start(@"http://www.SoundSpectrum.com");
    }

    private void VisualizationFpsNud_ValueChanged(object sender, EventArgs e)
    {
      if (IVizMgr != null)
        IVizMgr.TargetFPS = (int)VisualizationFpsNud.Value;
    }

    private void GaplessPlaybackChkBox_CheckedChanged(object sender, EventArgs e)
    {
      bool gaplessEnabled = GaplessPlaybackChkBox.Checked;
      CrossFadingLbl.Enabled = !gaplessEnabled;
      hScrollBarCrossFade.Enabled = !gaplessEnabled;
      CrossFadeSecondsLbl.Enabled = !gaplessEnabled;
    }

    private void EnableStatusOverlaysChkBox_CheckedChanged(object sender, EventArgs e)
    {
      ShowTrackInfoChkBox.Enabled = EnableStatusOverlaysChkBox.Checked;
    }

    private void InitializeVizEngine()
    {
      //System.Diagnostics.Debugger.Launch();
      Cursor.Current = Cursors.WaitCursor;

      MediaPortal.Player.BassAudioEngine bassEngine = MediaPortal.Player.CoreMusicPlayer.Player;

      IVizMgr = bassEngine.IVizManager;
      VizWindow = bassEngine.VisualizationWindow;
      List<VisualizationInfo> vizPluginsInfo = null;

      if (IVizMgr != null)
        vizPluginsInfo = IVizMgr.VisualizationPluginsInfo;

      LoadVisualizationList(vizPluginsInfo);
    }

    private void LoadVisualizationList(List<VisualizationInfo> vizPluginsInfo)
    {
      // If we're already populated the list we don't need to do it again so bail out
      if (VisualizationsInitialized)
        return;

      if (InvokeRequired)
      {
        LoadVisualizationListDelegate d = new LoadVisualizationListDelegate(LoadVisualizationList);
        Invoke(d, vizPluginsInfo);
        return;
      }

      VisualizationsCmbBox.Items.Clear();

      if (IVizMgr == null || vizPluginsInfo.Count == 0)
      {
        VisualizationsCmbBox.Items.Add(new VisualizationInfo("None", true));
        VisualizationsCmbBox.SelectedIndex = 0;
        return;
      }

      VisualizationsInitialized = true;
      SetVisualizationWindow();
      int selectedIndex = -1;

      for (int i = 0; i < vizPluginsInfo.Count; i++)
      {
        VisualizationInfo pluginInfo = vizPluginsInfo[i];

        if (pluginInfo.IsIdenticalTo(VizPluginInfo, true))
          selectedIndex = i;

        VisualizationsCmbBox.Items.Add(pluginInfo);
      }

      if (selectedIndex == -1 && VisualizationsCmbBox.Items.Count > 0)
        selectedIndex = 0;

      // If only one item was added it's the dummy "None" entry and we didn't find any other visualizations
      if (VisualizationsCmbBox.Items.Count == 1)
        VizPlaceHolderLbl.Text = "No compatible visualizations were found.";

      VisualizationsCmbBox.SelectedIndex = selectedIndex;
    }

    private void SetVisualizationWindow()
    {
      VizWindow.Location = VizPlaceHolderLbl.Location;
      VizWindow.Size = VizPlaceHolderLbl.Size;
      mpGroupBox3.Controls.Add(VizWindow);
      mpGroupBox3.Controls.Remove(VizPlaceHolderLbl);
      VizWindow.Visible = true;
    }

  }
}