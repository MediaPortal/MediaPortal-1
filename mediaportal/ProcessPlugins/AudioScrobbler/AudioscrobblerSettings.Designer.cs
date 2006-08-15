#region Copyright (C) 2006 Team MediaPortal

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

namespace MediaPortal.AudioScrobbler
{
  partial class AudioscrobblerSettings
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung.
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioscrobblerSettings));
      this.buttonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.panelPicBox = new System.Windows.Forms.Panel();
      this.pictureBoxASLogo = new System.Windows.Forms.PictureBox();
      this.linkLabelMPGroup = new System.Windows.Forms.LinkLabel();
      this.linkLabelNewUser = new System.Windows.Forms.LinkLabel();
      this.labelPassword = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelUser = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxASUser = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.maskedTextBoxASPass = new System.Windows.Forms.MaskedTextBox();
      this.tabControlSettings = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageLastFMSettings = new System.Windows.Forms.TabPage();
      this.labelPluginBannerHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.linkLabel2 = new System.Windows.Forms.LinkLabel();
      this.groupBoxOptions = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxEnableSubmits = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxLogVerbose = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxDismissOnError = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxdisableTimerThread = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxAccount = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxASUsername = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.maskedTextBoxASPassword = new System.Windows.Forms.MaskedTextBox();
      this.tabPageMusicSettings = new System.Windows.Forms.TabPage();
      this.labelBannerHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxAdvOptions = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelTracksArtistHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelsimilarArtistsHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelTracksPerArtistUpDown = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelSimilarArtistsUpDown = new MediaPortal.UserInterface.Controls.MPLabel();
      this.numericUpDownTracksPerArtist = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownSimilarArtist = new System.Windows.Forms.NumericUpDown();
      this.groupBoxMusicSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelNModeDesc = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelNModeCombo = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxNModeSelect = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxScrobbleDefault = new System.Windows.Forms.CheckBox();
      this.labelPercRandHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelPercRand = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelRandomness = new MediaPortal.UserInterface.Controls.MPLabel();
      this.trackBarRandomness = new System.Windows.Forms.TrackBar();
      this.tabPageLiveData = new System.Windows.Forms.TabPage();
      this.tabControlASSettings = new System.Windows.Forms.TabControl();
      this.tabPageRecent = new System.Windows.Forms.TabPage();
      this.buttonRefreshRecent = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewRecentTracks = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageNeighbours = new System.Windows.Forms.TabPage();
      this.comboBoxNeighbourMode = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.buttonNeighboursFilter = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonRefreshNeigboursArtists = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonRefreshNeighbours = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewNeighbours = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageSuggestions = new System.Windows.Forms.TabPage();
      this.labelTrackBarValue = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelArtistMatch = new System.Windows.Forms.Label();
      this.trackBarArtistMatch = new System.Windows.Forms.TrackBar();
      this.progressBarSuggestions = new System.Windows.Forms.ProgressBar();
      this.buttonRefreshSuggestions = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewSuggestions = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageTopArtists = new System.Windows.Forms.TabPage();
      this.buttonArtistsRefresh = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewTopArtists = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageWeeklyArtists = new System.Windows.Forms.TabPage();
      this.buttonRefreshWeeklyArtists = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewWeeklyArtists = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageTopTracks = new System.Windows.Forms.TabPage();
      this.buttonTopTracks = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewTopTracks = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageWeeklyTracks = new System.Windows.Forms.TabPage();
      this.buttonRefreshWeeklyTracks = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewWeeklyTracks = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageTags = new System.Windows.Forms.TabPage();
      this.checkBoxTagRandomize = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonTaggedTracks = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonTaggedAlbums = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelTagDesc = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxTagToSearch = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.buttonGetTaggedArtists = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonTagsRefresh = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewTags = new MediaPortal.UserInterface.Controls.MPListView();
      this.panelPicBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxASLogo)).BeginInit();
      this.tabControlSettings.SuspendLayout();
      this.tabPageLastFMSettings.SuspendLayout();
      this.groupBoxOptions.SuspendLayout();
      this.groupBoxAccount.SuspendLayout();
      this.tabPageMusicSettings.SuspendLayout();
      this.groupBoxAdvOptions.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTracksPerArtist)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSimilarArtist)).BeginInit();
      this.groupBoxMusicSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarRandomness)).BeginInit();
      this.tabPageLiveData.SuspendLayout();
      this.tabControlASSettings.SuspendLayout();
      this.tabPageRecent.SuspendLayout();
      this.tabPageNeighbours.SuspendLayout();
      this.tabPageSuggestions.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarArtistMatch)).BeginInit();
      this.tabPageTopArtists.SuspendLayout();
      this.tabPageWeeklyArtists.SuspendLayout();
      this.tabPageTopTracks.SuspendLayout();
      this.tabPageWeeklyTracks.SuspendLayout();
      this.tabPageTags.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonOk
      // 
      this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOk.Location = new System.Drawing.Point(507, 391);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(75, 23);
      this.buttonOk.TabIndex = 1;
      this.buttonOk.Text = "Save";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.Location = new System.Drawing.Point(426, 391);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // panelPicBox
      // 
      this.panelPicBox.Controls.Add(this.pictureBoxASLogo);
      this.panelPicBox.Location = new System.Drawing.Point(-1, -1);
      this.panelPicBox.Name = "panelPicBox";
      this.panelPicBox.Size = new System.Drawing.Size(595, 50);
      this.panelPicBox.TabIndex = 4;
      // 
      // pictureBoxASLogo
      // 
      this.pictureBoxASLogo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBoxASLogo.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxASLogo.Image")));
      this.pictureBoxASLogo.Location = new System.Drawing.Point(0, 0);
      this.pictureBoxASLogo.Name = "pictureBoxASLogo";
      this.pictureBoxASLogo.Size = new System.Drawing.Size(595, 50);
      this.pictureBoxASLogo.TabIndex = 0;
      this.pictureBoxASLogo.TabStop = false;
      // 
      // linkLabelMPGroup
      // 
      this.linkLabelMPGroup.AutoSize = true;
      this.linkLabelMPGroup.Location = new System.Drawing.Point(26, 151);
      this.linkLabelMPGroup.Name = "linkLabelMPGroup";
      this.linkLabelMPGroup.Size = new System.Drawing.Size(213, 13);
      this.linkLabelMPGroup.TabIndex = 5;
      this.linkLabelMPGroup.TabStop = true;
      this.linkLabelMPGroup.Text = "Please join the MediaPortal group on last.fm";
      // 
      // linkLabelNewUser
      // 
      this.linkLabelNewUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.linkLabelNewUser.AutoSize = true;
      this.linkLabelNewUser.Location = new System.Drawing.Point(194, 117);
      this.linkLabelNewUser.Name = "linkLabelNewUser";
      this.linkLabelNewUser.Size = new System.Drawing.Size(58, 13);
      this.linkLabelNewUser.TabIndex = 4;
      this.linkLabelNewUser.TabStop = true;
      this.linkLabelNewUser.Text = "New user..";
      // 
      // labelPassword
      // 
      this.labelPassword.AutoSize = true;
      this.labelPassword.Location = new System.Drawing.Point(16, 67);
      this.labelPassword.Name = "labelPassword";
      this.labelPassword.Size = new System.Drawing.Size(53, 13);
      this.labelPassword.TabIndex = 3;
      this.labelPassword.Text = "Password";
      // 
      // labelUser
      // 
      this.labelUser.AutoSize = true;
      this.labelUser.Location = new System.Drawing.Point(16, 23);
      this.labelUser.Name = "labelUser";
      this.labelUser.Size = new System.Drawing.Size(55, 13);
      this.labelUser.TabIndex = 2;
      this.labelUser.Text = "Username";
      // 
      // textBoxASUser
      // 
      this.textBoxASUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxASUser.BorderColor = System.Drawing.Color.Empty;
      this.textBoxASUser.Location = new System.Drawing.Point(16, 41);
      this.textBoxASUser.Name = "textBoxASUser";
      this.textBoxASUser.Size = new System.Drawing.Size(236, 20);
      this.textBoxASUser.TabIndex = 1;
      // 
      // maskedTextBoxASPass
      // 
      this.maskedTextBoxASPass.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.maskedTextBoxASPass.Culture = new System.Globalization.CultureInfo("");
      this.maskedTextBoxASPass.Location = new System.Drawing.Point(16, 85);
      this.maskedTextBoxASPass.Name = "maskedTextBoxASPass";
      this.maskedTextBoxASPass.PasswordChar = '*';
      this.maskedTextBoxASPass.Size = new System.Drawing.Size(236, 20);
      this.maskedTextBoxASPass.TabIndex = 0;
      // 
      // tabControlSettings
      // 
      this.tabControlSettings.Controls.Add(this.tabPageLastFMSettings);
      this.tabControlSettings.Controls.Add(this.tabPageMusicSettings);
      this.tabControlSettings.Controls.Add(this.tabPageLiveData);
      this.tabControlSettings.Location = new System.Drawing.Point(12, 55);
      this.tabControlSettings.Name = "tabControlSettings";
      this.tabControlSettings.SelectedIndex = 0;
      this.tabControlSettings.Size = new System.Drawing.Size(570, 330);
      this.tabControlSettings.TabIndex = 6;
      // 
      // tabPageLastFMSettings
      // 
      this.tabPageLastFMSettings.Controls.Add(this.labelPluginBannerHint);
      this.tabPageLastFMSettings.Controls.Add(this.linkLabel1);
      this.tabPageLastFMSettings.Controls.Add(this.linkLabel2);
      this.tabPageLastFMSettings.Controls.Add(this.groupBoxOptions);
      this.tabPageLastFMSettings.Controls.Add(this.groupBoxAccount);
      this.tabPageLastFMSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageLastFMSettings.Name = "tabPageLastFMSettings";
      this.tabPageLastFMSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageLastFMSettings.Size = new System.Drawing.Size(562, 304);
      this.tabPageLastFMSettings.TabIndex = 0;
      this.tabPageLastFMSettings.Text = "Plugin settings";
      this.tabPageLastFMSettings.UseVisualStyleBackColor = true;
      // 
      // labelPluginBannerHint
      // 
      this.labelPluginBannerHint.Location = new System.Drawing.Point(305, 12);
      this.labelPluginBannerHint.Name = "labelPluginBannerHint";
      this.labelPluginBannerHint.Size = new System.Drawing.Size(246, 194);
      this.labelPluginBannerHint.TabIndex = 7;
      this.labelPluginBannerHint.Text = resources.GetString("labelPluginBannerHint.Text");
      // 
      // linkLabel1
      // 
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(305, 268);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(213, 13);
      this.linkLabel1.TabIndex = 6;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "Please join the MediaPortal group on last.fm";
      // 
      // linkLabel2
      // 
      this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.linkLabel2.AutoSize = true;
      this.linkLabel2.Location = new System.Drawing.Point(305, 240);
      this.linkLabel2.Name = "linkLabel2";
      this.linkLabel2.Size = new System.Drawing.Size(104, 13);
      this.linkLabel2.TabIndex = 5;
      this.linkLabel2.TabStop = true;
      this.linkLabel2.Text = "Sign up a new user..";
      // 
      // groupBoxOptions
      // 
      this.groupBoxOptions.Controls.Add(this.checkBoxEnableSubmits);
      this.groupBoxOptions.Controls.Add(this.checkBoxLogVerbose);
      this.groupBoxOptions.Controls.Add(this.checkBoxDismissOnError);
      this.groupBoxOptions.Controls.Add(this.checkBoxdisableTimerThread);
      this.groupBoxOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxOptions.Location = new System.Drawing.Point(10, 129);
      this.groupBoxOptions.Name = "groupBoxOptions";
      this.groupBoxOptions.Size = new System.Drawing.Size(275, 77);
      this.groupBoxOptions.TabIndex = 4;
      this.groupBoxOptions.TabStop = false;
      this.groupBoxOptions.Text = "Scrobbler options";
      // 
      // checkBoxEnableSubmits
      // 
      this.checkBoxEnableSubmits.AutoSize = true;
      this.checkBoxEnableSubmits.Checked = true;
      this.checkBoxEnableSubmits.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxEnableSubmits.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEnableSubmits.Location = new System.Drawing.Point(16, 22);
      this.checkBoxEnableSubmits.Name = "checkBoxEnableSubmits";
      this.checkBoxEnableSubmits.Size = new System.Drawing.Size(239, 17);
      this.checkBoxEnableSubmits.TabIndex = 4;
      this.checkBoxEnableSubmits.Text = "Improve my profile at last.fm - submits enabled";
      this.checkBoxEnableSubmits.UseVisualStyleBackColor = true;
      // 
      // checkBoxLogVerbose
      // 
      this.checkBoxLogVerbose.AutoSize = true;
      this.checkBoxLogVerbose.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxLogVerbose.Location = new System.Drawing.Point(16, 45);
      this.checkBoxLogVerbose.Name = "checkBoxLogVerbose";
      this.checkBoxLogVerbose.Size = new System.Drawing.Size(135, 17);
      this.checkBoxLogVerbose.TabIndex = 2;
      this.checkBoxLogVerbose.Text = "Show debug log entries";
      this.checkBoxLogVerbose.UseVisualStyleBackColor = true;
      // 
      // checkBoxDismissOnError
      // 
      this.checkBoxDismissOnError.AutoSize = true;
      this.checkBoxDismissOnError.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDismissOnError.Location = new System.Drawing.Point(16, 53);
      this.checkBoxDismissOnError.Name = "checkBoxDismissOnError";
      this.checkBoxDismissOnError.Size = new System.Drawing.Size(228, 17);
      this.checkBoxDismissOnError.TabIndex = 1;
      this.checkBoxDismissOnError.Text = "Dismiss cached song on error and continue";
      this.checkBoxDismissOnError.UseVisualStyleBackColor = true;
      this.checkBoxDismissOnError.Visible = false;
      // 
      // checkBoxdisableTimerThread
      // 
      this.checkBoxdisableTimerThread.AutoSize = true;
      this.checkBoxdisableTimerThread.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxdisableTimerThread.Location = new System.Drawing.Point(6, 13);
      this.checkBoxdisableTimerThread.Name = "checkBoxdisableTimerThread";
      this.checkBoxdisableTimerThread.Size = new System.Drawing.Size(241, 17);
      this.checkBoxdisableTimerThread.TabIndex = 0;
      this.checkBoxdisableTimerThread.Text = "Do direct submits only (may avoid spam errors)";
      this.checkBoxdisableTimerThread.UseVisualStyleBackColor = true;
      this.checkBoxdisableTimerThread.Visible = false;
      // 
      // groupBoxAccount
      // 
      this.groupBoxAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAccount.Controls.Add(this.mpLabel1);
      this.groupBoxAccount.Controls.Add(this.mpLabel2);
      this.groupBoxAccount.Controls.Add(this.textBoxASUsername);
      this.groupBoxAccount.Controls.Add(this.maskedTextBoxASPassword);
      this.groupBoxAccount.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAccount.Location = new System.Drawing.Point(10, 6);
      this.groupBoxAccount.Name = "groupBoxAccount";
      this.groupBoxAccount.Size = new System.Drawing.Size(275, 117);
      this.groupBoxAccount.TabIndex = 3;
      this.groupBoxAccount.TabStop = false;
      this.groupBoxAccount.Text = "Last.fm account";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(16, 63);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(53, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Password";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(16, 19);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(55, 13);
      this.mpLabel2.TabIndex = 0;
      this.mpLabel2.Text = "Username";
      // 
      // textBoxASUsername
      // 
      this.textBoxASUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxASUsername.BorderColor = System.Drawing.Color.Empty;
      this.textBoxASUsername.Location = new System.Drawing.Point(16, 37);
      this.textBoxASUsername.Name = "textBoxASUsername";
      this.textBoxASUsername.Size = new System.Drawing.Size(243, 20);
      this.textBoxASUsername.TabIndex = 0;
      this.textBoxASUsername.Leave += new System.EventHandler(this.textBoxASUsername_Leave);
      // 
      // maskedTextBoxASPassword
      // 
      this.maskedTextBoxASPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.maskedTextBoxASPassword.Culture = new System.Globalization.CultureInfo("");
      this.maskedTextBoxASPassword.Location = new System.Drawing.Point(16, 81);
      this.maskedTextBoxASPassword.Name = "maskedTextBoxASPassword";
      this.maskedTextBoxASPassword.PasswordChar = '*';
      this.maskedTextBoxASPassword.Size = new System.Drawing.Size(243, 20);
      this.maskedTextBoxASPassword.TabIndex = 1;
      // 
      // tabPageMusicSettings
      // 
      this.tabPageMusicSettings.Controls.Add(this.labelBannerHint);
      this.tabPageMusicSettings.Controls.Add(this.groupBoxAdvOptions);
      this.tabPageMusicSettings.Controls.Add(this.groupBoxMusicSettings);
      this.tabPageMusicSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageMusicSettings.Name = "tabPageMusicSettings";
      this.tabPageMusicSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageMusicSettings.Size = new System.Drawing.Size(562, 304);
      this.tabPageMusicSettings.TabIndex = 2;
      this.tabPageMusicSettings.Text = "MyMusic settings";
      this.tabPageMusicSettings.UseVisualStyleBackColor = true;
      // 
      // labelBannerHint
      // 
      this.labelBannerHint.Location = new System.Drawing.Point(304, 12);
      this.labelBannerHint.Name = "labelBannerHint";
      this.labelBannerHint.Size = new System.Drawing.Size(247, 72);
      this.labelBannerHint.TabIndex = 8;
      this.labelBannerHint.Text = "The pre-configured values should work well for you without any changes - just try" +
          " the plugin!\r\n\r\nAfter you get used to it you may change these values to your nee" +
          "ds.";
      // 
      // groupBoxAdvOptions
      // 
      this.groupBoxAdvOptions.Controls.Add(this.labelTracksArtistHint);
      this.groupBoxAdvOptions.Controls.Add(this.labelsimilarArtistsHint);
      this.groupBoxAdvOptions.Controls.Add(this.labelTracksPerArtistUpDown);
      this.groupBoxAdvOptions.Controls.Add(this.labelSimilarArtistsUpDown);
      this.groupBoxAdvOptions.Controls.Add(this.numericUpDownTracksPerArtist);
      this.groupBoxAdvOptions.Controls.Add(this.numericUpDownSimilarArtist);
      this.groupBoxAdvOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAdvOptions.Location = new System.Drawing.Point(291, 131);
      this.groupBoxAdvOptions.Name = "groupBoxAdvOptions";
      this.groupBoxAdvOptions.Size = new System.Drawing.Size(258, 161);
      this.groupBoxAdvOptions.TabIndex = 7;
      this.groupBoxAdvOptions.TabStop = false;
      this.groupBoxAdvOptions.Text = "Advanced options (do not change for fun)";
      // 
      // labelTracksArtistHint
      // 
      this.labelTracksArtistHint.Location = new System.Drawing.Point(11, 120);
      this.labelTracksArtistHint.Name = "labelTracksArtistHint";
      this.labelTracksArtistHint.Size = new System.Drawing.Size(224, 30);
      this.labelTracksArtistHint.TabIndex = 14;
      this.labelTracksArtistHint.Text = "Changing not recommended!\r\nResult: many tracks from the same artist";
      // 
      // labelsimilarArtistsHint
      // 
      this.labelsimilarArtistsHint.Location = new System.Drawing.Point(11, 50);
      this.labelsimilarArtistsHint.Name = "labelsimilarArtistsHint";
      this.labelsimilarArtistsHint.Size = new System.Drawing.Size(224, 30);
      this.labelsimilarArtistsHint.TabIndex = 13;
      this.labelsimilarArtistsHint.Text = "Increase if you do not get enough songs\r\nLower if the playlist grows too fast";
      // 
      // labelTracksPerArtistUpDown
      // 
      this.labelTracksPerArtistUpDown.AutoSize = true;
      this.labelTracksPerArtistUpDown.Location = new System.Drawing.Point(64, 99);
      this.labelTracksPerArtistUpDown.Name = "labelTracksPerArtistUpDown";
      this.labelTracksPerArtistUpDown.Size = new System.Drawing.Size(150, 13);
      this.labelTracksPerArtistUpDown.TabIndex = 12;
      this.labelTracksPerArtistUpDown.Text = "add this many  tracks per artist";
      // 
      // labelSimilarArtistsUpDown
      // 
      this.labelSimilarArtistsUpDown.AutoSize = true;
      this.labelSimilarArtistsUpDown.Location = new System.Drawing.Point(64, 29);
      this.labelSimilarArtistsUpDown.Name = "labelSimilarArtistsUpDown";
      this.labelSimilarArtistsUpDown.Size = new System.Drawing.Size(155, 13);
      this.labelSimilarArtistsUpDown.TabIndex = 11;
      this.labelSimilarArtistsUpDown.Text = "consider this many similar artists";
      // 
      // numericUpDownTracksPerArtist
      // 
      this.numericUpDownTracksPerArtist.Location = new System.Drawing.Point(14, 97);
      this.numericUpDownTracksPerArtist.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.numericUpDownTracksPerArtist.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownTracksPerArtist.Name = "numericUpDownTracksPerArtist";
      this.numericUpDownTracksPerArtist.Size = new System.Drawing.Size(44, 20);
      this.numericUpDownTracksPerArtist.TabIndex = 10;
      this.numericUpDownTracksPerArtist.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // numericUpDownSimilarArtist
      // 
      this.numericUpDownSimilarArtist.Location = new System.Drawing.Point(14, 27);
      this.numericUpDownSimilarArtist.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.numericUpDownSimilarArtist.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownSimilarArtist.Name = "numericUpDownSimilarArtist";
      this.numericUpDownSimilarArtist.Size = new System.Drawing.Size(44, 20);
      this.numericUpDownSimilarArtist.TabIndex = 9;
      this.numericUpDownSimilarArtist.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
      // 
      // groupBoxMusicSettings
      // 
      this.groupBoxMusicSettings.Controls.Add(this.labelNModeDesc);
      this.groupBoxMusicSettings.Controls.Add(this.labelNModeCombo);
      this.groupBoxMusicSettings.Controls.Add(this.comboBoxNModeSelect);
      this.groupBoxMusicSettings.Controls.Add(this.checkBoxScrobbleDefault);
      this.groupBoxMusicSettings.Controls.Add(this.labelPercRandHint);
      this.groupBoxMusicSettings.Controls.Add(this.labelPercRand);
      this.groupBoxMusicSettings.Controls.Add(this.labelRandomness);
      this.groupBoxMusicSettings.Controls.Add(this.trackBarRandomness);
      this.groupBoxMusicSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMusicSettings.Location = new System.Drawing.Point(10, 6);
      this.groupBoxMusicSettings.Name = "groupBoxMusicSettings";
      this.groupBoxMusicSettings.Size = new System.Drawing.Size(275, 286);
      this.groupBoxMusicSettings.TabIndex = 6;
      this.groupBoxMusicSettings.TabStop = false;
      this.groupBoxMusicSettings.Text = "My Music settings";
      // 
      // labelNModeDesc
      // 
      this.labelNModeDesc.Location = new System.Drawing.Point(14, 166);
      this.labelNModeDesc.Name = "labelNModeDesc";
      this.labelNModeDesc.Size = new System.Drawing.Size(246, 117);
      this.labelNModeDesc.TabIndex = 9;
      this.labelNModeDesc.Text = resources.GetString("labelNModeDesc.Text");
      // 
      // labelNModeCombo
      // 
      this.labelNModeCombo.AutoSize = true;
      this.labelNModeCombo.Location = new System.Drawing.Point(14, 143);
      this.labelNModeCombo.Name = "labelNModeCombo";
      this.labelNModeCombo.Size = new System.Drawing.Size(85, 13);
      this.labelNModeCombo.TabIndex = 8;
      this.labelNModeCombo.Text = "Neighbour mode";
      // 
      // comboBoxNModeSelect
      // 
      this.comboBoxNModeSelect.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxNModeSelect.FormattingEnabled = true;
      this.comboBoxNModeSelect.Items.AddRange(new object[] {
            "Overall top artists",
            "Weekly top artists",
            "Recent artists"});
      this.comboBoxNModeSelect.Location = new System.Drawing.Point(122, 140);
      this.comboBoxNModeSelect.Name = "comboBoxNModeSelect";
      this.comboBoxNModeSelect.Size = new System.Drawing.Size(138, 21);
      this.comboBoxNModeSelect.TabIndex = 7;
      this.comboBoxNModeSelect.Text = "Weekly top artists";
      this.comboBoxNModeSelect.SelectedIndexChanged += new System.EventHandler(this.comboBoxNModeSelect_SelectedIndexChanged);
      // 
      // checkBoxScrobbleDefault
      // 
      this.checkBoxScrobbleDefault.AutoSize = true;
      this.checkBoxScrobbleDefault.Location = new System.Drawing.Point(17, 20);
      this.checkBoxScrobbleDefault.Name = "checkBoxScrobbleDefault";
      this.checkBoxScrobbleDefault.Size = new System.Drawing.Size(191, 17);
      this.checkBoxScrobbleDefault.TabIndex = 4;
      this.checkBoxScrobbleDefault.Text = "Scrobble mode enabled per default";
      this.checkBoxScrobbleDefault.UseVisualStyleBackColor = true;
      // 
      // labelPercRandHint
      // 
      this.labelPercRandHint.Location = new System.Drawing.Point(14, 95);
      this.labelPercRandHint.Name = "labelPercRandHint";
      this.labelPercRandHint.Size = new System.Drawing.Size(255, 35);
      this.labelPercRandHint.TabIndex = 3;
      this.labelPercRandHint.Text = "If you lower the percentage value you\'ll get \r\nsuggestions which are more similar" +
          " to each other";
      // 
      // labelPercRand
      // 
      this.labelPercRand.Location = new System.Drawing.Point(204, 44);
      this.labelPercRand.Name = "labelPercRand";
      this.labelPercRand.Size = new System.Drawing.Size(56, 13);
      this.labelPercRand.TabIndex = 2;
      this.labelPercRand.Text = "77";
      this.labelPercRand.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // labelRandomness
      // 
      this.labelRandomness.AutoSize = true;
      this.labelRandomness.Location = new System.Drawing.Point(14, 44);
      this.labelRandomness.Name = "labelRandomness";
      this.labelRandomness.Size = new System.Drawing.Size(104, 13);
      this.labelRandomness.TabIndex = 1;
      this.labelRandomness.Text = "Random percentage";
      // 
      // trackBarRandomness
      // 
      this.trackBarRandomness.BackColor = System.Drawing.SystemColors.Window;
      this.trackBarRandomness.LargeChange = 25;
      this.trackBarRandomness.Location = new System.Drawing.Point(5, 58);
      this.trackBarRandomness.Maximum = 100;
      this.trackBarRandomness.Minimum = 25;
      this.trackBarRandomness.Name = "trackBarRandomness";
      this.trackBarRandomness.Size = new System.Drawing.Size(264, 40);
      this.trackBarRandomness.SmallChange = 5;
      this.trackBarRandomness.TabIndex = 0;
      this.trackBarRandomness.TickFrequency = 15;
      this.trackBarRandomness.Value = 77;
      this.trackBarRandomness.ValueChanged += new System.EventHandler(this.trackBarRandomness_ValueChanged);
      // 
      // tabPageLiveData
      // 
      this.tabPageLiveData.Controls.Add(this.tabControlASSettings);
      this.tabPageLiveData.Location = new System.Drawing.Point(4, 22);
      this.tabPageLiveData.Name = "tabPageLiveData";
      this.tabPageLiveData.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageLiveData.Size = new System.Drawing.Size(562, 304);
      this.tabPageLiveData.TabIndex = 1;
      this.tabPageLiveData.Text = "Live data";
      this.tabPageLiveData.UseVisualStyleBackColor = true;
      // 
      // tabControlASSettings
      // 
      this.tabControlASSettings.Controls.Add(this.tabPageRecent);
      this.tabControlASSettings.Controls.Add(this.tabPageNeighbours);
      this.tabControlASSettings.Controls.Add(this.tabPageSuggestions);
      this.tabControlASSettings.Controls.Add(this.tabPageTopArtists);
      this.tabControlASSettings.Controls.Add(this.tabPageWeeklyArtists);
      this.tabControlASSettings.Controls.Add(this.tabPageTopTracks);
      this.tabControlASSettings.Controls.Add(this.tabPageWeeklyTracks);
      this.tabControlASSettings.Controls.Add(this.tabPageTags);
      this.tabControlASSettings.HotTrack = true;
      this.tabControlASSettings.Location = new System.Drawing.Point(6, 6);
      this.tabControlASSettings.Name = "tabControlASSettings";
      this.tabControlASSettings.SelectedIndex = 0;
      this.tabControlASSettings.Size = new System.Drawing.Size(550, 292);
      this.tabControlASSettings.TabIndex = 6;
      // 
      // tabPageRecent
      // 
      this.tabPageRecent.Controls.Add(this.buttonRefreshRecent);
      this.tabPageRecent.Controls.Add(this.listViewRecentTracks);
      this.tabPageRecent.Location = new System.Drawing.Point(4, 22);
      this.tabPageRecent.Name = "tabPageRecent";
      this.tabPageRecent.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageRecent.Size = new System.Drawing.Size(542, 266);
      this.tabPageRecent.TabIndex = 1;
      this.tabPageRecent.Text = "Recent";
      this.tabPageRecent.UseVisualStyleBackColor = true;
      // 
      // buttonRefreshRecent
      // 
      this.buttonRefreshRecent.Location = new System.Drawing.Point(452, 12);
      this.buttonRefreshRecent.Name = "buttonRefreshRecent";
      this.buttonRefreshRecent.Size = new System.Drawing.Size(75, 23);
      this.buttonRefreshRecent.TabIndex = 1;
      this.buttonRefreshRecent.Text = "Refresh";
      this.buttonRefreshRecent.UseVisualStyleBackColor = true;
      this.buttonRefreshRecent.Click += new System.EventHandler(this.buttonRefreshRecent_Click);
      // 
      // listViewRecentTracks
      // 
      this.listViewRecentTracks.AllowColumnReorder = true;
      this.listViewRecentTracks.AllowDrop = true;
      this.listViewRecentTracks.AllowRowReorder = false;
      this.listViewRecentTracks.Location = new System.Drawing.Point(6, 12);
      this.listViewRecentTracks.Name = "listViewRecentTracks";
      this.listViewRecentTracks.ShowGroups = false;
      this.listViewRecentTracks.Size = new System.Drawing.Size(440, 250);
      this.listViewRecentTracks.TabIndex = 0;
      this.listViewRecentTracks.UseCompatibleStateImageBehavior = false;
      this.listViewRecentTracks.View = System.Windows.Forms.View.List;
      // 
      // tabPageNeighbours
      // 
      this.tabPageNeighbours.Controls.Add(this.comboBoxNeighbourMode);
      this.tabPageNeighbours.Controls.Add(this.buttonNeighboursFilter);
      this.tabPageNeighbours.Controls.Add(this.buttonRefreshNeigboursArtists);
      this.tabPageNeighbours.Controls.Add(this.buttonRefreshNeighbours);
      this.tabPageNeighbours.Controls.Add(this.listViewNeighbours);
      this.tabPageNeighbours.Location = new System.Drawing.Point(4, 22);
      this.tabPageNeighbours.Name = "tabPageNeighbours";
      this.tabPageNeighbours.Size = new System.Drawing.Size(542, 266);
      this.tabPageNeighbours.TabIndex = 7;
      this.tabPageNeighbours.Text = "Neighbours";
      this.tabPageNeighbours.UseVisualStyleBackColor = true;
      // 
      // comboBoxNeighbourMode
      // 
      this.comboBoxNeighbourMode.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxNeighbourMode.Enabled = false;
      this.comboBoxNeighbourMode.FormattingEnabled = true;
      this.comboBoxNeighbourMode.Items.AddRange(new object[] {
            "Top",
            "Weekly",
            "Recent"});
      this.comboBoxNeighbourMode.Location = new System.Drawing.Point(452, 183);
      this.comboBoxNeighbourMode.Name = "comboBoxNeighbourMode";
      this.comboBoxNeighbourMode.Size = new System.Drawing.Size(75, 21);
      this.comboBoxNeighbourMode.TabIndex = 6;
      this.comboBoxNeighbourMode.Text = "Weekly";
      this.comboBoxNeighbourMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxNeighbourMode_SelectedIndexChanged);
      // 
      // buttonNeighboursFilter
      // 
      this.buttonNeighboursFilter.Enabled = false;
      this.buttonNeighboursFilter.Location = new System.Drawing.Point(452, 239);
      this.buttonNeighboursFilter.Name = "buttonNeighboursFilter";
      this.buttonNeighboursFilter.Size = new System.Drawing.Size(75, 23);
      this.buttonNeighboursFilter.TabIndex = 5;
      this.buttonNeighboursFilter.Text = "Suggestions";
      this.buttonNeighboursFilter.UseVisualStyleBackColor = true;
      this.buttonNeighboursFilter.Click += new System.EventHandler(this.buttonNeighboursFilter_Click);
      // 
      // buttonRefreshNeigboursArtists
      // 
      this.buttonRefreshNeigboursArtists.Enabled = false;
      this.buttonRefreshNeigboursArtists.Location = new System.Drawing.Point(452, 210);
      this.buttonRefreshNeigboursArtists.Name = "buttonRefreshNeigboursArtists";
      this.buttonRefreshNeigboursArtists.Size = new System.Drawing.Size(75, 23);
      this.buttonRefreshNeigboursArtists.TabIndex = 4;
      this.buttonRefreshNeigboursArtists.Text = "Their artists";
      this.buttonRefreshNeigboursArtists.UseVisualStyleBackColor = true;
      this.buttonRefreshNeigboursArtists.Click += new System.EventHandler(this.buttonRefreshNeigboursArtists_Click);
      // 
      // buttonRefreshNeighbours
      // 
      this.buttonRefreshNeighbours.Location = new System.Drawing.Point(452, 12);
      this.buttonRefreshNeighbours.Name = "buttonRefreshNeighbours";
      this.buttonRefreshNeighbours.Size = new System.Drawing.Size(75, 23);
      this.buttonRefreshNeighbours.TabIndex = 3;
      this.buttonRefreshNeighbours.Text = "Refresh";
      this.buttonRefreshNeighbours.UseVisualStyleBackColor = true;
      this.buttonRefreshNeighbours.Click += new System.EventHandler(this.buttonRefreshNeighbours_Click);
      // 
      // listViewNeighbours
      // 
      this.listViewNeighbours.AllowColumnReorder = true;
      this.listViewNeighbours.AllowDrop = true;
      this.listViewNeighbours.AllowRowReorder = false;
      this.listViewNeighbours.Location = new System.Drawing.Point(6, 12);
      this.listViewNeighbours.Name = "listViewNeighbours";
      this.listViewNeighbours.ShowGroups = false;
      this.listViewNeighbours.Size = new System.Drawing.Size(440, 250);
      this.listViewNeighbours.TabIndex = 2;
      this.listViewNeighbours.UseCompatibleStateImageBehavior = false;
      this.listViewNeighbours.View = System.Windows.Forms.View.List;
      // 
      // tabPageSuggestions
      // 
      this.tabPageSuggestions.Controls.Add(this.labelTrackBarValue);
      this.tabPageSuggestions.Controls.Add(this.labelArtistMatch);
      this.tabPageSuggestions.Controls.Add(this.trackBarArtistMatch);
      this.tabPageSuggestions.Controls.Add(this.progressBarSuggestions);
      this.tabPageSuggestions.Controls.Add(this.buttonRefreshSuggestions);
      this.tabPageSuggestions.Controls.Add(this.listViewSuggestions);
      this.tabPageSuggestions.Location = new System.Drawing.Point(4, 22);
      this.tabPageSuggestions.Name = "tabPageSuggestions";
      this.tabPageSuggestions.Size = new System.Drawing.Size(542, 266);
      this.tabPageSuggestions.TabIndex = 4;
      this.tabPageSuggestions.Text = "Suggestions";
      this.tabPageSuggestions.UseVisualStyleBackColor = true;
      // 
      // labelTrackBarValue
      // 
      this.labelTrackBarValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.labelTrackBarValue.AutoSize = true;
      this.labelTrackBarValue.Location = new System.Drawing.Point(423, 242);
      this.labelTrackBarValue.Name = "labelTrackBarValue";
      this.labelTrackBarValue.Size = new System.Drawing.Size(19, 13);
      this.labelTrackBarValue.TabIndex = 11;
      this.labelTrackBarValue.Text = "90";
      // 
      // labelArtistMatch
      // 
      this.labelArtistMatch.AutoSize = true;
      this.labelArtistMatch.Location = new System.Drawing.Point(9, 242);
      this.labelArtistMatch.Name = "labelArtistMatch";
      this.labelArtistMatch.Size = new System.Drawing.Size(48, 13);
      this.labelArtistMatch.TabIndex = 10;
      this.labelArtistMatch.Text = "Match %";
      // 
      // trackBarArtistMatch
      // 
      this.trackBarArtistMatch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.trackBarArtistMatch.AutoSize = false;
      this.trackBarArtistMatch.BackColor = System.Drawing.SystemColors.Window;
      this.trackBarArtistMatch.LargeChange = 10;
      this.trackBarArtistMatch.Location = new System.Drawing.Point(63, 237);
      this.trackBarArtistMatch.Maximum = 100;
      this.trackBarArtistMatch.Minimum = 50;
      this.trackBarArtistMatch.Name = "trackBarArtistMatch";
      this.trackBarArtistMatch.Size = new System.Drawing.Size(354, 23);
      this.trackBarArtistMatch.SmallChange = 5;
      this.trackBarArtistMatch.TabIndex = 9;
      this.trackBarArtistMatch.TickFrequency = 10;
      this.trackBarArtistMatch.Value = 90;
      this.trackBarArtistMatch.ValueChanged += new System.EventHandler(this.trackBarArtistMatch_ValueChanged);
      // 
      // progressBarSuggestions
      // 
      this.progressBarSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSuggestions.Location = new System.Drawing.Point(6, 237);
      this.progressBarSuggestions.Name = "progressBarSuggestions";
      this.progressBarSuggestions.Size = new System.Drawing.Size(440, 23);
      this.progressBarSuggestions.TabIndex = 8;
      this.progressBarSuggestions.Visible = false;
      // 
      // buttonRefreshSuggestions
      // 
      this.buttonRefreshSuggestions.Location = new System.Drawing.Point(452, 12);
      this.buttonRefreshSuggestions.Name = "buttonRefreshSuggestions";
      this.buttonRefreshSuggestions.Size = new System.Drawing.Size(75, 23);
      this.buttonRefreshSuggestions.TabIndex = 7;
      this.buttonRefreshSuggestions.Text = "Refresh";
      this.buttonRefreshSuggestions.UseVisualStyleBackColor = true;
      this.buttonRefreshSuggestions.Click += new System.EventHandler(this.buttonRefreshSuggestions_Click);
      // 
      // listViewSuggestions
      // 
      this.listViewSuggestions.Activation = System.Windows.Forms.ItemActivation.OneClick;
      this.listViewSuggestions.AllowColumnReorder = true;
      this.listViewSuggestions.AllowDrop = true;
      this.listViewSuggestions.AllowRowReorder = false;
      this.listViewSuggestions.AutoArrange = false;
      this.listViewSuggestions.Location = new System.Drawing.Point(6, 12);
      this.listViewSuggestions.Name = "listViewSuggestions";
      this.listViewSuggestions.ShowGroups = false;
      this.listViewSuggestions.Size = new System.Drawing.Size(440, 220);
      this.listViewSuggestions.TabIndex = 6;
      this.listViewSuggestions.UseCompatibleStateImageBehavior = false;
      this.listViewSuggestions.View = System.Windows.Forms.View.List;
      // 
      // tabPageTopArtists
      // 
      this.tabPageTopArtists.Controls.Add(this.buttonArtistsRefresh);
      this.tabPageTopArtists.Controls.Add(this.listViewTopArtists);
      this.tabPageTopArtists.Location = new System.Drawing.Point(4, 22);
      this.tabPageTopArtists.Name = "tabPageTopArtists";
      this.tabPageTopArtists.Size = new System.Drawing.Size(542, 266);
      this.tabPageTopArtists.TabIndex = 2;
      this.tabPageTopArtists.Text = "Top artists";
      this.tabPageTopArtists.UseVisualStyleBackColor = true;
      // 
      // buttonArtistsRefresh
      // 
      this.buttonArtistsRefresh.Location = new System.Drawing.Point(452, 12);
      this.buttonArtistsRefresh.Name = "buttonArtistsRefresh";
      this.buttonArtistsRefresh.Size = new System.Drawing.Size(75, 23);
      this.buttonArtistsRefresh.TabIndex = 3;
      this.buttonArtistsRefresh.Text = "Refresh";
      this.buttonArtistsRefresh.UseVisualStyleBackColor = true;
      this.buttonArtistsRefresh.Click += new System.EventHandler(this.buttonArtistsRefresh_Click);
      // 
      // listViewTopArtists
      // 
      this.listViewTopArtists.Activation = System.Windows.Forms.ItemActivation.OneClick;
      this.listViewTopArtists.AllowColumnReorder = true;
      this.listViewTopArtists.AllowDrop = true;
      this.listViewTopArtists.AllowRowReorder = false;
      this.listViewTopArtists.AutoArrange = false;
      this.listViewTopArtists.Location = new System.Drawing.Point(6, 12);
      this.listViewTopArtists.Name = "listViewTopArtists";
      this.listViewTopArtists.ShowGroups = false;
      this.listViewTopArtists.Size = new System.Drawing.Size(440, 250);
      this.listViewTopArtists.TabIndex = 2;
      this.listViewTopArtists.UseCompatibleStateImageBehavior = false;
      this.listViewTopArtists.View = System.Windows.Forms.View.List;
      // 
      // tabPageWeeklyArtists
      // 
      this.tabPageWeeklyArtists.Controls.Add(this.buttonRefreshWeeklyArtists);
      this.tabPageWeeklyArtists.Controls.Add(this.listViewWeeklyArtists);
      this.tabPageWeeklyArtists.Location = new System.Drawing.Point(4, 22);
      this.tabPageWeeklyArtists.Name = "tabPageWeeklyArtists";
      this.tabPageWeeklyArtists.Size = new System.Drawing.Size(542, 266);
      this.tabPageWeeklyArtists.TabIndex = 5;
      this.tabPageWeeklyArtists.Text = "Weekly artists";
      this.tabPageWeeklyArtists.UseVisualStyleBackColor = true;
      // 
      // buttonRefreshWeeklyArtists
      // 
      this.buttonRefreshWeeklyArtists.Location = new System.Drawing.Point(452, 12);
      this.buttonRefreshWeeklyArtists.Name = "buttonRefreshWeeklyArtists";
      this.buttonRefreshWeeklyArtists.Size = new System.Drawing.Size(75, 23);
      this.buttonRefreshWeeklyArtists.TabIndex = 3;
      this.buttonRefreshWeeklyArtists.Text = "Refresh";
      this.buttonRefreshWeeklyArtists.UseVisualStyleBackColor = true;
      this.buttonRefreshWeeklyArtists.Click += new System.EventHandler(this.buttonRefreshWeeklyArtists_Click);
      // 
      // listViewWeeklyArtists
      // 
      this.listViewWeeklyArtists.AllowColumnReorder = true;
      this.listViewWeeklyArtists.AllowDrop = true;
      this.listViewWeeklyArtists.AllowRowReorder = false;
      this.listViewWeeklyArtists.Location = new System.Drawing.Point(6, 12);
      this.listViewWeeklyArtists.Name = "listViewWeeklyArtists";
      this.listViewWeeklyArtists.ShowGroups = false;
      this.listViewWeeklyArtists.Size = new System.Drawing.Size(440, 250);
      this.listViewWeeklyArtists.TabIndex = 2;
      this.listViewWeeklyArtists.UseCompatibleStateImageBehavior = false;
      this.listViewWeeklyArtists.View = System.Windows.Forms.View.List;
      // 
      // tabPageTopTracks
      // 
      this.tabPageTopTracks.Controls.Add(this.buttonTopTracks);
      this.tabPageTopTracks.Controls.Add(this.listViewTopTracks);
      this.tabPageTopTracks.Location = new System.Drawing.Point(4, 22);
      this.tabPageTopTracks.Name = "tabPageTopTracks";
      this.tabPageTopTracks.Size = new System.Drawing.Size(542, 266);
      this.tabPageTopTracks.TabIndex = 3;
      this.tabPageTopTracks.Text = "Top tracks";
      this.tabPageTopTracks.UseVisualStyleBackColor = true;
      // 
      // buttonTopTracks
      // 
      this.buttonTopTracks.Location = new System.Drawing.Point(452, 12);
      this.buttonTopTracks.Name = "buttonTopTracks";
      this.buttonTopTracks.Size = new System.Drawing.Size(75, 23);
      this.buttonTopTracks.TabIndex = 5;
      this.buttonTopTracks.Text = "Refresh";
      this.buttonTopTracks.UseVisualStyleBackColor = true;
      this.buttonTopTracks.Click += new System.EventHandler(this.buttonTopTracks_Click);
      // 
      // listViewTopTracks
      // 
      this.listViewTopTracks.Activation = System.Windows.Forms.ItemActivation.OneClick;
      this.listViewTopTracks.AllowColumnReorder = true;
      this.listViewTopTracks.AllowDrop = true;
      this.listViewTopTracks.AllowRowReorder = false;
      this.listViewTopTracks.AutoArrange = false;
      this.listViewTopTracks.Location = new System.Drawing.Point(6, 12);
      this.listViewTopTracks.Name = "listViewTopTracks";
      this.listViewTopTracks.ShowGroups = false;
      this.listViewTopTracks.Size = new System.Drawing.Size(440, 250);
      this.listViewTopTracks.TabIndex = 4;
      this.listViewTopTracks.UseCompatibleStateImageBehavior = false;
      this.listViewTopTracks.View = System.Windows.Forms.View.List;
      // 
      // tabPageWeeklyTracks
      // 
      this.tabPageWeeklyTracks.Controls.Add(this.buttonRefreshWeeklyTracks);
      this.tabPageWeeklyTracks.Controls.Add(this.listViewWeeklyTracks);
      this.tabPageWeeklyTracks.Location = new System.Drawing.Point(4, 22);
      this.tabPageWeeklyTracks.Name = "tabPageWeeklyTracks";
      this.tabPageWeeklyTracks.Size = new System.Drawing.Size(542, 266);
      this.tabPageWeeklyTracks.TabIndex = 6;
      this.tabPageWeeklyTracks.Text = "Weekly tracks";
      this.tabPageWeeklyTracks.UseVisualStyleBackColor = true;
      // 
      // buttonRefreshWeeklyTracks
      // 
      this.buttonRefreshWeeklyTracks.Location = new System.Drawing.Point(452, 12);
      this.buttonRefreshWeeklyTracks.Name = "buttonRefreshWeeklyTracks";
      this.buttonRefreshWeeklyTracks.Size = new System.Drawing.Size(75, 23);
      this.buttonRefreshWeeklyTracks.TabIndex = 3;
      this.buttonRefreshWeeklyTracks.Text = "Refresh";
      this.buttonRefreshWeeklyTracks.UseVisualStyleBackColor = true;
      this.buttonRefreshWeeklyTracks.Click += new System.EventHandler(this.buttonRefreshWeeklyTracks_Click);
      // 
      // listViewWeeklyTracks
      // 
      this.listViewWeeklyTracks.AllowColumnReorder = true;
      this.listViewWeeklyTracks.AllowDrop = true;
      this.listViewWeeklyTracks.AllowRowReorder = false;
      this.listViewWeeklyTracks.Location = new System.Drawing.Point(6, 12);
      this.listViewWeeklyTracks.Name = "listViewWeeklyTracks";
      this.listViewWeeklyTracks.ShowGroups = false;
      this.listViewWeeklyTracks.Size = new System.Drawing.Size(440, 250);
      this.listViewWeeklyTracks.TabIndex = 2;
      this.listViewWeeklyTracks.UseCompatibleStateImageBehavior = false;
      this.listViewWeeklyTracks.View = System.Windows.Forms.View.List;
      // 
      // tabPageTags
      // 
      this.tabPageTags.Controls.Add(this.checkBoxTagRandomize);
      this.tabPageTags.Controls.Add(this.buttonTaggedTracks);
      this.tabPageTags.Controls.Add(this.buttonTaggedAlbums);
      this.tabPageTags.Controls.Add(this.labelTagDesc);
      this.tabPageTags.Controls.Add(this.textBoxTagToSearch);
      this.tabPageTags.Controls.Add(this.buttonGetTaggedArtists);
      this.tabPageTags.Controls.Add(this.buttonTagsRefresh);
      this.tabPageTags.Controls.Add(this.listViewTags);
      this.tabPageTags.Location = new System.Drawing.Point(4, 22);
      this.tabPageTags.Name = "tabPageTags";
      this.tabPageTags.Size = new System.Drawing.Size(542, 266);
      this.tabPageTags.TabIndex = 8;
      this.tabPageTags.Text = "Tags";
      this.tabPageTags.UseVisualStyleBackColor = true;
      // 
      // checkBoxTagRandomize
      // 
      this.checkBoxTagRandomize.AutoSize = true;
      this.checkBoxTagRandomize.Checked = true;
      this.checkBoxTagRandomize.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxTagRandomize.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxTagRandomize.Location = new System.Drawing.Point(454, 161);
      this.checkBoxTagRandomize.Name = "checkBoxTagRandomize";
      this.checkBoxTagRandomize.Size = new System.Drawing.Size(73, 17);
      this.checkBoxTagRandomize.TabIndex = 9;
      this.checkBoxTagRandomize.Text = "Random 5";
      this.checkBoxTagRandomize.UseVisualStyleBackColor = true;
      // 
      // buttonTaggedTracks
      // 
      this.buttonTaggedTracks.Location = new System.Drawing.Point(452, 239);
      this.buttonTaggedTracks.Name = "buttonTaggedTracks";
      this.buttonTaggedTracks.Size = new System.Drawing.Size(83, 23);
      this.buttonTaggedTracks.TabIndex = 8;
      this.buttonTaggedTracks.Text = "Get tracks";
      this.buttonTaggedTracks.UseVisualStyleBackColor = true;
      this.buttonTaggedTracks.Click += new System.EventHandler(this.buttonTaggedTracks_Click);
      // 
      // buttonTaggedAlbums
      // 
      this.buttonTaggedAlbums.Location = new System.Drawing.Point(452, 210);
      this.buttonTaggedAlbums.Name = "buttonTaggedAlbums";
      this.buttonTaggedAlbums.Size = new System.Drawing.Size(83, 23);
      this.buttonTaggedAlbums.TabIndex = 7;
      this.buttonTaggedAlbums.Text = "Get albums";
      this.buttonTaggedAlbums.UseVisualStyleBackColor = true;
      this.buttonTaggedAlbums.Click += new System.EventHandler(this.buttonTaggedAlbums_Click);
      // 
      // labelTagDesc
      // 
      this.labelTagDesc.AutoSize = true;
      this.labelTagDesc.Location = new System.Drawing.Point(452, 117);
      this.labelTagDesc.Name = "labelTagDesc";
      this.labelTagDesc.Size = new System.Drawing.Size(62, 13);
      this.labelTagDesc.TabIndex = 6;
      this.labelTagDesc.Text = "Search tag:";
      // 
      // textBoxTagToSearch
      // 
      this.textBoxTagToSearch.BorderColor = System.Drawing.Color.Empty;
      this.textBoxTagToSearch.Location = new System.Drawing.Point(454, 135);
      this.textBoxTagToSearch.Name = "textBoxTagToSearch";
      this.textBoxTagToSearch.Size = new System.Drawing.Size(81, 20);
      this.textBoxTagToSearch.TabIndex = 5;
      this.textBoxTagToSearch.Text = "Viking Metal";
      this.textBoxTagToSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxTagToSearch_KeyUp);
      // 
      // buttonGetTaggedArtists
      // 
      this.buttonGetTaggedArtists.Location = new System.Drawing.Point(452, 181);
      this.buttonGetTaggedArtists.Name = "buttonGetTaggedArtists";
      this.buttonGetTaggedArtists.Size = new System.Drawing.Size(83, 23);
      this.buttonGetTaggedArtists.TabIndex = 4;
      this.buttonGetTaggedArtists.Text = "Get artists";
      this.buttonGetTaggedArtists.UseVisualStyleBackColor = true;
      this.buttonGetTaggedArtists.Click += new System.EventHandler(this.buttonGetTaggedArtists_Click);
      // 
      // buttonTagsRefresh
      // 
      this.buttonTagsRefresh.Location = new System.Drawing.Point(452, 12);
      this.buttonTagsRefresh.Name = "buttonTagsRefresh";
      this.buttonTagsRefresh.Size = new System.Drawing.Size(83, 23);
      this.buttonTagsRefresh.TabIndex = 3;
      this.buttonTagsRefresh.Text = "Refresh";
      this.buttonTagsRefresh.UseVisualStyleBackColor = true;
      this.buttonTagsRefresh.Click += new System.EventHandler(this.buttonTagsRefresh_Click);
      // 
      // listViewTags
      // 
      this.listViewTags.AllowColumnReorder = true;
      this.listViewTags.AllowDrop = true;
      this.listViewTags.AllowRowReorder = false;
      this.listViewTags.Location = new System.Drawing.Point(6, 12);
      this.listViewTags.Name = "listViewTags";
      this.listViewTags.ShowGroups = false;
      this.listViewTags.Size = new System.Drawing.Size(440, 250);
      this.listViewTags.TabIndex = 2;
      this.listViewTags.UseCompatibleStateImageBehavior = false;
      this.listViewTags.View = System.Windows.Forms.View.List;
      // 
      // AudioscrobblerSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(594, 426);
      this.Controls.Add(this.tabControlSettings);
      this.Controls.Add(this.panelPicBox);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "AudioscrobblerSettings";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Audioscrobbler settings";
      this.panelPicBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxASLogo)).EndInit();
      this.tabControlSettings.ResumeLayout(false);
      this.tabPageLastFMSettings.ResumeLayout(false);
      this.tabPageLastFMSettings.PerformLayout();
      this.groupBoxOptions.ResumeLayout(false);
      this.groupBoxOptions.PerformLayout();
      this.groupBoxAccount.ResumeLayout(false);
      this.groupBoxAccount.PerformLayout();
      this.tabPageMusicSettings.ResumeLayout(false);
      this.groupBoxAdvOptions.ResumeLayout(false);
      this.groupBoxAdvOptions.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTracksPerArtist)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSimilarArtist)).EndInit();
      this.groupBoxMusicSettings.ResumeLayout(false);
      this.groupBoxMusicSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarRandomness)).EndInit();
      this.tabPageLiveData.ResumeLayout(false);
      this.tabControlASSettings.ResumeLayout(false);
      this.tabPageRecent.ResumeLayout(false);
      this.tabPageNeighbours.ResumeLayout(false);
      this.tabPageSuggestions.ResumeLayout(false);
      this.tabPageSuggestions.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarArtistMatch)).EndInit();
      this.tabPageTopArtists.ResumeLayout(false);
      this.tabPageWeeklyArtists.ResumeLayout(false);
      this.tabPageTopTracks.ResumeLayout(false);
      this.tabPageWeeklyTracks.ResumeLayout(false);
      this.tabPageTags.ResumeLayout(false);
      this.tabPageTags.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPButton buttonOk;
    private MediaPortal.UserInterface.Controls.MPButton buttonCancel;
    private System.Windows.Forms.Panel panelPicBox;
    private System.Windows.Forms.PictureBox pictureBoxASLogo;
    private System.Windows.Forms.LinkLabel linkLabelMPGroup;
    private System.Windows.Forms.LinkLabel linkLabelNewUser;
    private MediaPortal.UserInterface.Controls.MPLabel labelPassword;
    private MediaPortal.UserInterface.Controls.MPLabel labelUser;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxASUser;
    private System.Windows.Forms.MaskedTextBox maskedTextBoxASPass;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControlSettings;
    private System.Windows.Forms.TabPage tabPageLastFMSettings;
    private System.Windows.Forms.TabPage tabPageLiveData;
    private System.Windows.Forms.TabControl tabControlASSettings;
    private System.Windows.Forms.TabPage tabPageSuggestions;
    private MediaPortal.UserInterface.Controls.MPLabel labelTrackBarValue;
    private System.Windows.Forms.Label labelArtistMatch;
    private System.Windows.Forms.TrackBar trackBarArtistMatch;
    private System.Windows.Forms.ProgressBar progressBarSuggestions;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshSuggestions;
    private MediaPortal.UserInterface.Controls.MPListView listViewSuggestions;
    private System.Windows.Forms.TabPage tabPageRecent;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshRecent;
    private MediaPortal.UserInterface.Controls.MPListView listViewRecentTracks;
    private System.Windows.Forms.TabPage tabPageNeighbours;
    private MediaPortal.UserInterface.Controls.MPButton buttonNeighboursFilter;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshNeigboursArtists;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshNeighbours;
    private MediaPortal.UserInterface.Controls.MPListView listViewNeighbours;
    private System.Windows.Forms.TabPage tabPageTopArtists;
    private MediaPortal.UserInterface.Controls.MPButton buttonArtistsRefresh;
    private MediaPortal.UserInterface.Controls.MPListView listViewTopArtists;
    private System.Windows.Forms.TabPage tabPageWeeklyArtists;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshWeeklyArtists;
    private MediaPortal.UserInterface.Controls.MPListView listViewWeeklyArtists;
    private System.Windows.Forms.TabPage tabPageTopTracks;
    private MediaPortal.UserInterface.Controls.MPButton buttonTopTracks;
    private MediaPortal.UserInterface.Controls.MPListView listViewTopTracks;
    private System.Windows.Forms.TabPage tabPageWeeklyTracks;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshWeeklyTracks;
    private MediaPortal.UserInterface.Controls.MPListView listViewWeeklyTracks;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxOptions;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxLogVerbose;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDismissOnError;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxdisableTimerThread;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxAccount;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxASUsername;
    private System.Windows.Forms.MaskedTextBox maskedTextBoxASPassword;
    private System.Windows.Forms.TabPage tabPageTags;
    private MediaPortal.UserInterface.Controls.MPButton buttonTagsRefresh;
    private MediaPortal.UserInterface.Controls.MPListView listViewTags;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableSubmits;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxNeighbourMode;
    private System.Windows.Forms.TabPage tabPageMusicSettings;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxAdvOptions;
    private MediaPortal.UserInterface.Controls.MPLabel labelTracksArtistHint;
    private MediaPortal.UserInterface.Controls.MPLabel labelsimilarArtistsHint;
    private MediaPortal.UserInterface.Controls.MPLabel labelTracksPerArtistUpDown;
    private MediaPortal.UserInterface.Controls.MPLabel labelSimilarArtistsUpDown;
    private System.Windows.Forms.NumericUpDown numericUpDownTracksPerArtist;
    private System.Windows.Forms.NumericUpDown numericUpDownSimilarArtist;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxMusicSettings;
    private System.Windows.Forms.CheckBox checkBoxScrobbleDefault;
    private MediaPortal.UserInterface.Controls.MPLabel labelPercRandHint;
    private MediaPortal.UserInterface.Controls.MPLabel labelPercRand;
    private MediaPortal.UserInterface.Controls.MPLabel labelRandomness;
    private System.Windows.Forms.TrackBar trackBarRandomness;
    private MediaPortal.UserInterface.Controls.MPLabel labelBannerHint;
    private MediaPortal.UserInterface.Controls.MPLabel labelNModeCombo;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxNModeSelect;
    private MediaPortal.UserInterface.Controls.MPLabel labelNModeDesc;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.LinkLabel linkLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel labelPluginBannerHint;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxTagToSearch;
    private MediaPortal.UserInterface.Controls.MPButton buttonGetTaggedArtists;
    private MediaPortal.UserInterface.Controls.MPLabel labelTagDesc;
    private MediaPortal.UserInterface.Controls.MPButton buttonTaggedAlbums;
    private MediaPortal.UserInterface.Controls.MPButton buttonTaggedTracks;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxTagRandomize;
  }
}