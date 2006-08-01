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
      this.groupBoxAccount = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.linkLabel2 = new System.Windows.Forms.LinkLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxASUsername = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.maskedTextBoxASPassword = new System.Windows.Forms.MaskedTextBox();
      this.tabPageRecent = new System.Windows.Forms.TabPage();
      this.buttonRefreshRecent = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewRecentTracks = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageTopArtists = new System.Windows.Forms.TabPage();
      this.buttonArtistsRefresh = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewTopArtists = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabPageTopTracks = new System.Windows.Forms.TabPage();
      this.buttonTopTracks = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewTopTracks = new MediaPortal.UserInterface.Controls.MPListView();
      this.linkLabelMPGroup = new System.Windows.Forms.LinkLabel();
      this.linkLabelNewUser = new System.Windows.Forms.LinkLabel();
      this.labelPassword = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelUser = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxASUser = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.maskedTextBoxASPass = new System.Windows.Forms.MaskedTextBox();
      this.groupBoxOptions = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxdisableTimerThread = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.panelPicBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxASLogo)).BeginInit();
      this.tabControlASSettings.SuspendLayout();
      this.tabPageAccount.SuspendLayout();
      this.groupBoxAccount.SuspendLayout();
      this.tabPageRecent.SuspendLayout();
      this.tabPageTopArtists.SuspendLayout();
      this.tabPageTopTracks.SuspendLayout();
      this.groupBoxOptions.SuspendLayout();
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
      this.tabControlASSettings.Controls.Add(this.tabPageRecent);
      this.tabControlASSettings.Controls.Add(this.tabPageTopArtists);
      this.tabControlASSettings.Controls.Add(this.tabPageTopTracks);
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
      this.groupBoxAccount.Size = new System.Drawing.Size(275, 144);
      this.groupBoxAccount.TabIndex = 1;
      this.groupBoxAccount.TabStop = false;
      this.groupBoxAccount.Text = "last.fm account";
      // 
      // linkLabel1
      // 
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(27, 121);
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
      // tabPageRecent
      // 
      this.tabPageRecent.Controls.Add(this.buttonRefreshRecent);
      this.tabPageRecent.Controls.Add(this.listViewRecentTracks);
      this.tabPageRecent.Location = new System.Drawing.Point(4, 22);
      this.tabPageRecent.Name = "tabPageRecent";
      this.tabPageRecent.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageRecent.Size = new System.Drawing.Size(287, 286);
      this.tabPageRecent.TabIndex = 1;
      this.tabPageRecent.Text = "Recent tracks";
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
      // tabPageTopArtists
      // 
      this.tabPageTopArtists.Controls.Add(this.buttonArtistsRefresh);
      this.tabPageTopArtists.Controls.Add(this.listViewTopArtists);
      this.tabPageTopArtists.Location = new System.Drawing.Point(4, 22);
      this.tabPageTopArtists.Name = "tabPageTopArtists";
      this.tabPageTopArtists.Size = new System.Drawing.Size(287, 286);
      this.tabPageTopArtists.TabIndex = 2;
      this.tabPageTopArtists.Text = "Top artists";
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
      // tabPageTopTracks
      // 
      this.tabPageTopTracks.Controls.Add(this.buttonTopTracks);
      this.tabPageTopTracks.Controls.Add(this.listViewTopTracks);
      this.tabPageTopTracks.Location = new System.Drawing.Point(4, 22);
      this.tabPageTopTracks.Name = "tabPageTopTracks";
      this.tabPageTopTracks.Size = new System.Drawing.Size(287, 286);
      this.tabPageTopTracks.TabIndex = 3;
      this.tabPageTopTracks.Text = "Top tracks";
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
      // groupBoxOptions
      // 
      this.groupBoxOptions.Controls.Add(this.checkBoxdisableTimerThread);
      this.groupBoxOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxOptions.Location = new System.Drawing.Point(6, 156);
      this.groupBoxOptions.Name = "groupBoxOptions";
      this.groupBoxOptions.Size = new System.Drawing.Size(271, 86);
      this.groupBoxOptions.TabIndex = 2;
      this.groupBoxOptions.TabStop = false;
      this.groupBoxOptions.Text = "Options";
      // 
      // checkBoxdisableTimerThread
      // 
      this.checkBoxdisableTimerThread.AutoSize = true;
      this.checkBoxdisableTimerThread.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxdisableTimerThread.Location = new System.Drawing.Point(16, 28);
      this.checkBoxdisableTimerThread.Name = "checkBoxdisableTimerThread";
      this.checkBoxdisableTimerThread.Size = new System.Drawing.Size(241, 17);
      this.checkBoxdisableTimerThread.TabIndex = 0;
      this.checkBoxdisableTimerThread.Text = "Do direct submits only (may avoid spam errors)";
      this.checkBoxdisableTimerThread.UseVisualStyleBackColor = true;
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
      this.groupBoxAccount.ResumeLayout(false);
      this.groupBoxAccount.PerformLayout();
      this.tabPageRecent.ResumeLayout(false);
      this.tabPageTopArtists.ResumeLayout(false);
      this.tabPageTopTracks.ResumeLayout(false);
      this.groupBoxOptions.ResumeLayout(false);
      this.groupBoxOptions.PerformLayout();
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
  }
}