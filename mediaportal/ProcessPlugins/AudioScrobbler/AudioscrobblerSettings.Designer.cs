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
      this.tabControlASSettings = new System.Windows.Forms.TabControl();
      this.tabPageAccount = new System.Windows.Forms.TabPage();
      this.groupBoxOptions = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonClearCache = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxLogVerbose = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxDismissOnError = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxdisableTimerThread = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxAccount = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.linkLabel2 = new System.Windows.Forms.LinkLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxASUsername = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.maskedTextBoxASPassword = new System.Windows.Forms.MaskedTextBox();
      this.tabPageSuggestions = new System.Windows.Forms.TabPage();
      this.labelTrackBarValue = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelArtistMatch = new System.Windows.Forms.Label();
      this.trackBarArtistMatch = new System.Windows.Forms.TrackBar();
      this.progressBarSuggestions = new System.Windows.Forms.ProgressBar();
      this.buttonRefreshSuggestions = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewSuggestions = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageRecent = new System.Windows.Forms.TabPage();
      this.buttonRefreshRecent = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewRecentTracks = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageNeighbours = new System.Windows.Forms.TabPage();
      this.buttonRefreshNeighbours = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewNeighbours = new MediaPortal.UserInterface.Controls.MPListView();
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
      this.linkLabelMPGroup = new System.Windows.Forms.LinkLabel();
      this.linkLabelNewUser = new System.Windows.Forms.LinkLabel();
      this.labelPassword = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelUser = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxASUser = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.maskedTextBoxASPass = new System.Windows.Forms.MaskedTextBox();
      this.buttonRefreshNeigboursArtists = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonNeighboursFilter = new MediaPortal.UserInterface.Controls.MPButton();
      this.panelPicBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxASLogo)).BeginInit();
      this.tabControlASSettings.SuspendLayout();
      this.tabPageAccount.SuspendLayout();
      this.groupBoxOptions.SuspendLayout();
      this.groupBoxAccount.SuspendLayout();
      this.tabPageSuggestions.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarArtistMatch)).BeginInit();
      this.tabPageRecent.SuspendLayout();
      this.tabPageNeighbours.SuspendLayout();
      this.tabPageTopArtists.SuspendLayout();
      this.tabPageWeeklyArtists.SuspendLayout();
      this.tabPageTopTracks.SuspendLayout();
      this.tabPageWeeklyTracks.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonOk
      // 
      this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOk.Location = new System.Drawing.Point(205, 377);
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
      this.buttonCancel.Location = new System.Drawing.Point(124, 377);
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
      this.panelPicBox.Size = new System.Drawing.Size(295, 50);
      this.panelPicBox.TabIndex = 4;
      // 
      // pictureBoxASLogo
      // 
      this.pictureBoxASLogo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBoxASLogo.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxASLogo.Image")));
      this.pictureBoxASLogo.Location = new System.Drawing.Point(0, 0);
      this.pictureBoxASLogo.Name = "pictureBoxASLogo";
      this.pictureBoxASLogo.Size = new System.Drawing.Size(295, 50);
      this.pictureBoxASLogo.TabIndex = 0;
      this.pictureBoxASLogo.TabStop = false;
      // 
      // tabControlASSettings
      // 
      this.tabControlASSettings.Controls.Add(this.tabPageAccount);
      this.tabControlASSettings.Controls.Add(this.tabPageSuggestions);
      this.tabControlASSettings.Controls.Add(this.tabPageRecent);
      this.tabControlASSettings.Controls.Add(this.tabPageNeighbours);
      this.tabControlASSettings.Controls.Add(this.tabPageTopArtists);
      this.tabControlASSettings.Controls.Add(this.tabPageWeeklyArtists);
      this.tabControlASSettings.Controls.Add(this.tabPageTopTracks);
      this.tabControlASSettings.Controls.Add(this.tabPageWeeklyTracks);
      this.tabControlASSettings.HotTrack = true;
      this.tabControlASSettings.Location = new System.Drawing.Point(-1, 55);
      this.tabControlASSettings.Name = "tabControlASSettings";
      this.tabControlASSettings.SelectedIndex = 0;
      this.tabControlASSettings.Size = new System.Drawing.Size(295, 312);
      this.tabControlASSettings.TabIndex = 5;
      // 
      // tabPageAccount
      // 
      this.tabPageAccount.Controls.Add(this.groupBoxOptions);
      this.tabPageAccount.Controls.Add(this.groupBoxAccount);
      this.tabPageAccount.Location = new System.Drawing.Point(4, 22);
      this.tabPageAccount.Name = "tabPageAccount";
      this.tabPageAccount.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageAccount.Size = new System.Drawing.Size(287, 286);
      this.tabPageAccount.TabIndex = 0;
      this.tabPageAccount.Text = "Account";
      this.tabPageAccount.UseVisualStyleBackColor = true;
      // 
      // groupBoxOptions
      // 
      this.groupBoxOptions.Controls.Add(this.buttonClearCache);
      this.groupBoxOptions.Controls.Add(this.checkBoxLogVerbose);
      this.groupBoxOptions.Controls.Add(this.checkBoxDismissOnError);
      this.groupBoxOptions.Controls.Add(this.checkBoxdisableTimerThread);
      this.groupBoxOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxOptions.Location = new System.Drawing.Point(6, 147);
      this.groupBoxOptions.Name = "groupBoxOptions";
      this.groupBoxOptions.Size = new System.Drawing.Size(271, 128);
      this.groupBoxOptions.TabIndex = 2;
      this.groupBoxOptions.TabStop = false;
      this.groupBoxOptions.Text = "Options";
      // 
      // buttonClearCache
      // 
      this.buttonClearCache.Location = new System.Drawing.Point(16, 93);
      this.buttonClearCache.Name = "buttonClearCache";
      this.buttonClearCache.Size = new System.Drawing.Size(241, 23);
      this.buttonClearCache.TabIndex = 3;
      this.buttonClearCache.Text = "Clear cache";
      this.buttonClearCache.UseVisualStyleBackColor = true;
      this.buttonClearCache.Click += new System.EventHandler(this.buttonClearCache_Click);
      // 
      // checkBoxLogVerbose
      // 
      this.checkBoxLogVerbose.AutoSize = true;
      this.checkBoxLogVerbose.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxLogVerbose.Location = new System.Drawing.Point(16, 66);
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
      this.checkBoxDismissOnError.Location = new System.Drawing.Point(16, 43);
      this.checkBoxDismissOnError.Name = "checkBoxDismissOnError";
      this.checkBoxDismissOnError.Size = new System.Drawing.Size(228, 17);
      this.checkBoxDismissOnError.TabIndex = 1;
      this.checkBoxDismissOnError.Text = "Dismiss cached song on error and continue";
      this.checkBoxDismissOnError.UseVisualStyleBackColor = true;
      // 
      // checkBoxdisableTimerThread
      // 
      this.checkBoxdisableTimerThread.AutoSize = true;
      this.checkBoxdisableTimerThread.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxdisableTimerThread.Location = new System.Drawing.Point(16, 20);
      this.checkBoxdisableTimerThread.Name = "checkBoxdisableTimerThread";
      this.checkBoxdisableTimerThread.Size = new System.Drawing.Size(241, 17);
      this.checkBoxdisableTimerThread.TabIndex = 0;
      this.checkBoxdisableTimerThread.Text = "Do direct submits only (may avoid spam errors)";
      this.checkBoxdisableTimerThread.UseVisualStyleBackColor = true;
      // 
      // groupBoxAccount
      // 
      this.groupBoxAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAccount.Controls.Add(this.linkLabel1);
      this.groupBoxAccount.Controls.Add(this.linkLabel2);
      this.groupBoxAccount.Controls.Add(this.mpLabel1);
      this.groupBoxAccount.Controls.Add(this.mpLabel2);
      this.groupBoxAccount.Controls.Add(this.textBoxASUsername);
      this.groupBoxAccount.Controls.Add(this.maskedTextBoxASPassword);
      this.groupBoxAccount.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAccount.Location = new System.Drawing.Point(6, 6);
      this.groupBoxAccount.Name = "groupBoxAccount";
      this.groupBoxAccount.Size = new System.Drawing.Size(275, 135);
      this.groupBoxAccount.TabIndex = 1;
      this.groupBoxAccount.TabStop = false;
      this.groupBoxAccount.Text = "last.fm account";
      // 
      // linkLabel1
      // 
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(27, 113);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(213, 13);
      this.linkLabel1.TabIndex = 2;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "Please join the MediaPortal group on last.fm";
      // 
      // linkLabel2
      // 
      this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.linkLabel2.AutoSize = true;
      this.linkLabel2.Location = new System.Drawing.Point(84, 23);
      this.linkLabel2.Name = "linkLabel2";
      this.linkLabel2.Size = new System.Drawing.Size(58, 13);
      this.linkLabel2.TabIndex = 3;
      this.linkLabel2.TabStop = true;
      this.linkLabel2.Text = "New user..";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(16, 67);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(53, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Password";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(16, 23);
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
      this.textBoxASUsername.Location = new System.Drawing.Point(16, 41);
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
      this.maskedTextBoxASPassword.Location = new System.Drawing.Point(16, 85);
      this.maskedTextBoxASPassword.Name = "maskedTextBoxASPassword";
      this.maskedTextBoxASPassword.PasswordChar = '*';
      this.maskedTextBoxASPassword.Size = new System.Drawing.Size(243, 20);
      this.maskedTextBoxASPassword.TabIndex = 1;
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
      this.tabPageSuggestions.Size = new System.Drawing.Size(287, 286);
      this.tabPageSuggestions.TabIndex = 4;
      this.tabPageSuggestions.Text = "Suggestions";
      this.tabPageSuggestions.UseVisualStyleBackColor = true;
      // 
      // labelTrackBarValue
      // 
      this.labelTrackBarValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.labelTrackBarValue.AutoSize = true;
      this.labelTrackBarValue.Location = new System.Drawing.Point(168, 262);
      this.labelTrackBarValue.Name = "labelTrackBarValue";
      this.labelTrackBarValue.Size = new System.Drawing.Size(19, 13);
      this.labelTrackBarValue.TabIndex = 11;
      this.labelTrackBarValue.Text = "90";
      // 
      // labelArtistMatch
      // 
      this.labelArtistMatch.AutoSize = true;
      this.labelArtistMatch.Location = new System.Drawing.Point(9, 262);
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
      this.trackBarArtistMatch.Location = new System.Drawing.Point(63, 257);
      this.trackBarArtistMatch.Maximum = 100;
      this.trackBarArtistMatch.Minimum = 50;
      this.trackBarArtistMatch.Name = "trackBarArtistMatch";
      this.trackBarArtistMatch.Size = new System.Drawing.Size(99, 23);
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
      this.progressBarSuggestions.Location = new System.Drawing.Point(6, 257);
      this.progressBarSuggestions.Name = "progressBarSuggestions";
      this.progressBarSuggestions.Size = new System.Drawing.Size(190, 23);
      this.progressBarSuggestions.TabIndex = 8;
      this.progressBarSuggestions.Visible = false;
      // 
      // buttonRefreshSuggestions
      // 
      this.buttonRefreshSuggestions.Location = new System.Drawing.Point(202, 257);
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
      this.listViewSuggestions.Size = new System.Drawing.Size(275, 239);
      this.listViewSuggestions.TabIndex = 6;
      this.listViewSuggestions.UseCompatibleStateImageBehavior = false;
      this.listViewSuggestions.View = System.Windows.Forms.View.List;
      // 
      // tabPageRecent
      // 
      this.tabPageRecent.Controls.Add(this.buttonRefreshRecent);
      this.tabPageRecent.Controls.Add(this.listViewRecentTracks);
      this.tabPageRecent.Location = new System.Drawing.Point(4, 22);
      this.tabPageRecent.Name = "tabPageRecent";
      this.tabPageRecent.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageRecent.Size = new System.Drawing.Size(287, 286);
      this.tabPageRecent.TabIndex = 1;
      this.tabPageRecent.Text = "Recent";
      this.tabPageRecent.UseVisualStyleBackColor = true;
      // 
      // buttonRefreshRecent
      // 
      this.buttonRefreshRecent.Location = new System.Drawing.Point(202, 257);
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
      this.listViewRecentTracks.Size = new System.Drawing.Size(275, 239);
      this.listViewRecentTracks.TabIndex = 0;
      this.listViewRecentTracks.UseCompatibleStateImageBehavior = false;
      this.listViewRecentTracks.View = System.Windows.Forms.View.List;
      // 
      // tabPageNeighbours
      // 
      this.tabPageNeighbours.Controls.Add(this.buttonNeighboursFilter);
      this.tabPageNeighbours.Controls.Add(this.buttonRefreshNeigboursArtists);
      this.tabPageNeighbours.Controls.Add(this.buttonRefreshNeighbours);
      this.tabPageNeighbours.Controls.Add(this.listViewNeighbours);
      this.tabPageNeighbours.Location = new System.Drawing.Point(4, 22);
      this.tabPageNeighbours.Name = "tabPageNeighbours";
      this.tabPageNeighbours.Size = new System.Drawing.Size(287, 286);
      this.tabPageNeighbours.TabIndex = 7;
      this.tabPageNeighbours.Text = "Neighbours";
      this.tabPageNeighbours.UseVisualStyleBackColor = true;
      // 
      // buttonRefreshNeighbours
      // 
      this.buttonRefreshNeighbours.Location = new System.Drawing.Point(202, 257);
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
      this.listViewNeighbours.Size = new System.Drawing.Size(275, 239);
      this.listViewNeighbours.TabIndex = 2;
      this.listViewNeighbours.UseCompatibleStateImageBehavior = false;
      this.listViewNeighbours.View = System.Windows.Forms.View.List;
      // 
      // tabPageTopArtists
      // 
      this.tabPageTopArtists.Controls.Add(this.buttonArtistsRefresh);
      this.tabPageTopArtists.Controls.Add(this.listViewTopArtists);
      this.tabPageTopArtists.Location = new System.Drawing.Point(4, 22);
      this.tabPageTopArtists.Name = "tabPageTopArtists";
      this.tabPageTopArtists.Size = new System.Drawing.Size(287, 286);
      this.tabPageTopArtists.TabIndex = 2;
      this.tabPageTopArtists.Text = "Artists";
      this.tabPageTopArtists.UseVisualStyleBackColor = true;
      // 
      // buttonArtistsRefresh
      // 
      this.buttonArtistsRefresh.Location = new System.Drawing.Point(202, 257);
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
      this.listViewTopArtists.Size = new System.Drawing.Size(275, 239);
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
      this.tabPageWeeklyArtists.Size = new System.Drawing.Size(287, 286);
      this.tabPageWeeklyArtists.TabIndex = 5;
      this.tabPageWeeklyArtists.Text = "Weekly artists";
      this.tabPageWeeklyArtists.UseVisualStyleBackColor = true;
      // 
      // buttonRefreshWeeklyArtists
      // 
      this.buttonRefreshWeeklyArtists.Location = new System.Drawing.Point(202, 257);
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
      this.listViewWeeklyArtists.Size = new System.Drawing.Size(275, 239);
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
      this.tabPageTopTracks.Size = new System.Drawing.Size(287, 286);
      this.tabPageTopTracks.TabIndex = 3;
      this.tabPageTopTracks.Text = "Tracks";
      this.tabPageTopTracks.UseVisualStyleBackColor = true;
      // 
      // buttonTopTracks
      // 
      this.buttonTopTracks.Location = new System.Drawing.Point(202, 257);
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
      this.listViewTopTracks.Size = new System.Drawing.Size(275, 239);
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
      this.tabPageWeeklyTracks.Size = new System.Drawing.Size(287, 286);
      this.tabPageWeeklyTracks.TabIndex = 6;
      this.tabPageWeeklyTracks.Text = "Weekly tracks";
      this.tabPageWeeklyTracks.UseVisualStyleBackColor = true;
      // 
      // buttonRefreshWeeklyTracks
      // 
      this.buttonRefreshWeeklyTracks.Location = new System.Drawing.Point(202, 257);
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
      this.listViewWeeklyTracks.Size = new System.Drawing.Size(275, 239);
      this.listViewWeeklyTracks.TabIndex = 2;
      this.listViewWeeklyTracks.UseCompatibleStateImageBehavior = false;
      this.listViewWeeklyTracks.View = System.Windows.Forms.View.List;
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
      // buttonRefreshNeigboursArtists
      // 
      this.buttonRefreshNeigboursArtists.Enabled = false;
      this.buttonRefreshNeigboursArtists.Location = new System.Drawing.Point(6, 257);
      this.buttonRefreshNeigboursArtists.Name = "buttonRefreshNeigboursArtists";
      this.buttonRefreshNeigboursArtists.Size = new System.Drawing.Size(90, 23);
      this.buttonRefreshNeigboursArtists.TabIndex = 4;
      this.buttonRefreshNeigboursArtists.Text = "Get their artists";
      this.buttonRefreshNeigboursArtists.UseVisualStyleBackColor = true;
      this.buttonRefreshNeigboursArtists.Click += new System.EventHandler(this.buttonRefreshNeigboursArtists_Click);
      // 
      // buttonNeighboursFilter
      // 
      this.buttonNeighboursFilter.Enabled = false;
      this.buttonNeighboursFilter.Location = new System.Drawing.Point(102, 257);
      this.buttonNeighboursFilter.Name = "buttonNeighboursFilter";
      this.buttonNeighboursFilter.Size = new System.Drawing.Size(75, 23);
      this.buttonNeighboursFilter.TabIndex = 5;
      this.buttonNeighboursFilter.Text = "Suggestions";
      this.buttonNeighboursFilter.UseVisualStyleBackColor = true;
      this.buttonNeighboursFilter.Click += new System.EventHandler(this.buttonNeighboursFilter_Click);
      // 
      // AudioscrobblerSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 412);
      this.Controls.Add(this.tabControlASSettings);
      this.Controls.Add(this.panelPicBox);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "AudioscrobblerSettings";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Audioscrobbler settings";
      this.panelPicBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxASLogo)).EndInit();
      this.tabControlASSettings.ResumeLayout(false);
      this.tabPageAccount.ResumeLayout(false);
      this.groupBoxOptions.ResumeLayout(false);
      this.groupBoxOptions.PerformLayout();
      this.groupBoxAccount.ResumeLayout(false);
      this.groupBoxAccount.PerformLayout();
      this.tabPageSuggestions.ResumeLayout(false);
      this.tabPageSuggestions.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarArtistMatch)).EndInit();
      this.tabPageRecent.ResumeLayout(false);
      this.tabPageNeighbours.ResumeLayout(false);
      this.tabPageTopArtists.ResumeLayout(false);
      this.tabPageWeeklyArtists.ResumeLayout(false);
      this.tabPageTopTracks.ResumeLayout(false);
      this.tabPageWeeklyTracks.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPButton buttonOk;
    private MediaPortal.UserInterface.Controls.MPButton buttonCancel;
    private System.Windows.Forms.Panel panelPicBox;
    private System.Windows.Forms.PictureBox pictureBoxASLogo;
    private System.Windows.Forms.TabControl tabControlASSettings;
    private System.Windows.Forms.TabPage tabPageAccount;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxAccount;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.LinkLabel linkLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxASUsername;
    private System.Windows.Forms.MaskedTextBox maskedTextBoxASPassword;
    private System.Windows.Forms.TabPage tabPageRecent;
    private System.Windows.Forms.LinkLabel linkLabelMPGroup;
    private System.Windows.Forms.LinkLabel linkLabelNewUser;
    private MediaPortal.UserInterface.Controls.MPLabel labelPassword;
    private MediaPortal.UserInterface.Controls.MPLabel labelUser;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxASUser;
    private System.Windows.Forms.MaskedTextBox maskedTextBoxASPass;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshRecent;
    private MediaPortal.UserInterface.Controls.MPListView listViewRecentTracks;
    private System.Windows.Forms.TabPage tabPageTopArtists;
    private MediaPortal.UserInterface.Controls.MPButton buttonArtistsRefresh;
    private MediaPortal.UserInterface.Controls.MPListView listViewTopArtists;
    private System.Windows.Forms.TabPage tabPageTopTracks;
    private MediaPortal.UserInterface.Controls.MPButton buttonTopTracks;
    private MediaPortal.UserInterface.Controls.MPListView listViewTopTracks;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxOptions;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxdisableTimerThread;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxLogVerbose;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDismissOnError;
    private MediaPortal.UserInterface.Controls.MPButton buttonClearCache;
    private System.Windows.Forms.TabPage tabPageSuggestions;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshSuggestions;
    private MediaPortal.UserInterface.Controls.MPListView listViewSuggestions;
    private System.Windows.Forms.ProgressBar progressBarSuggestions;
    private System.Windows.Forms.TrackBar trackBarArtistMatch;
    private System.Windows.Forms.Label labelArtistMatch;
    private MediaPortal.UserInterface.Controls.MPLabel labelTrackBarValue;
    private System.Windows.Forms.TabPage tabPageWeeklyArtists;
    private System.Windows.Forms.TabPage tabPageWeeklyTracks;
    private System.Windows.Forms.TabPage tabPageNeighbours;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshNeighbours;
    private MediaPortal.UserInterface.Controls.MPListView listViewNeighbours;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshWeeklyArtists;
    private MediaPortal.UserInterface.Controls.MPListView listViewWeeklyArtists;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshWeeklyTracks;
    private MediaPortal.UserInterface.Controls.MPListView listViewWeeklyTracks;
    private MediaPortal.UserInterface.Controls.MPButton buttonRefreshNeigboursArtists;
    private MediaPortal.UserInterface.Controls.MPButton buttonNeighboursFilter;
  }
}