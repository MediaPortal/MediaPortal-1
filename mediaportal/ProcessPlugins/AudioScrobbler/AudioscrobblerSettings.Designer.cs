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
      this.groupBoxAccount = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelPassword = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelUser = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxASUser = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.maskedTextBoxASPass = new System.Windows.Forms.MaskedTextBox();
      this.buttonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxASSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabelMPGroup = new System.Windows.Forms.LinkLabel();
      this.checkBoxTeamStats = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.panelPicBox = new System.Windows.Forms.Panel();
      this.pictureBoxASLogo = new System.Windows.Forms.PictureBox();
      this.linkLabelNewUser = new System.Windows.Forms.LinkLabel();
      this.groupBoxAccount.SuspendLayout();
      this.groupBoxASSettings.SuspendLayout();
      this.panelPicBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxASLogo)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxAccount
      // 
      this.groupBoxAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAccount.Controls.Add(this.linkLabelNewUser);
      this.groupBoxAccount.Controls.Add(this.labelPassword);
      this.groupBoxAccount.Controls.Add(this.labelUser);
      this.groupBoxAccount.Controls.Add(this.textBoxASUser);
      this.groupBoxAccount.Controls.Add(this.maskedTextBoxASPass);
      this.groupBoxAccount.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAccount.Location = new System.Drawing.Point(12, 55);
      this.groupBoxAccount.Name = "groupBoxAccount";
      this.groupBoxAccount.Size = new System.Drawing.Size(268, 139);
      this.groupBoxAccount.TabIndex = 0;
      this.groupBoxAccount.TabStop = false;
      this.groupBoxAccount.Text = "last.fm account";
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
      // buttonOk
      // 
      this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOk.Location = new System.Drawing.Point(205, 285);
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
      this.buttonCancel.Location = new System.Drawing.Point(124, 285);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // groupBoxASSettings
      // 
      this.groupBoxASSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxASSettings.Controls.Add(this.linkLabelMPGroup);
      this.groupBoxASSettings.Controls.Add(this.checkBoxTeamStats);
      this.groupBoxASSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxASSettings.Location = new System.Drawing.Point(12, 209);
      this.groupBoxASSettings.Name = "groupBoxASSettings";
      this.groupBoxASSettings.Size = new System.Drawing.Size(268, 61);
      this.groupBoxASSettings.TabIndex = 3;
      this.groupBoxASSettings.TabStop = false;
      this.groupBoxASSettings.Text = "Settings";
      // 
      // linkLabelMPGroup
      // 
      this.linkLabelMPGroup.AutoSize = true;
      this.linkLabelMPGroup.Location = new System.Drawing.Point(13, 26);
      this.linkLabelMPGroup.Name = "linkLabelMPGroup";
      this.linkLabelMPGroup.Size = new System.Drawing.Size(213, 13);
      this.linkLabelMPGroup.TabIndex = 1;
      this.linkLabelMPGroup.TabStop = true;
      this.linkLabelMPGroup.Text = "Please join the MediaPortal group on last.fm";
      this.linkLabelMPGroup.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelMPGroup_LinkClicked);
      // 
      // checkBoxTeamStats
      // 
      this.checkBoxTeamStats.AutoSize = true;
      this.checkBoxTeamStats.Enabled = false;
      this.checkBoxTeamStats.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxTeamStats.Location = new System.Drawing.Point(16, 42);
      this.checkBoxTeamStats.Name = "checkBoxTeamStats";
      this.checkBoxTeamStats.Size = new System.Drawing.Size(181, 17);
      this.checkBoxTeamStats.TabIndex = 0;
      this.checkBoxTeamStats.Text = "Participate the MediaPortal group";
      this.checkBoxTeamStats.UseVisualStyleBackColor = true;
      this.checkBoxTeamStats.Visible = false;
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
      this.linkLabelNewUser.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelNewUser_LinkClicked);
      // 
      // AudioscrobblerSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 320);
      this.Controls.Add(this.panelPicBox);
      this.Controls.Add(this.groupBoxASSettings);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.groupBoxAccount);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "AudioscrobblerSettings";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Audioscrobbler settings";
      this.groupBoxAccount.ResumeLayout(false);
      this.groupBoxAccount.PerformLayout();
      this.groupBoxASSettings.ResumeLayout(false);
      this.groupBoxASSettings.PerformLayout();
      this.panelPicBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxASLogo)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxAccount;
    private MediaPortal.UserInterface.Controls.MPLabel labelUser;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxASUser;
    private System.Windows.Forms.MaskedTextBox maskedTextBoxASPass;
    private MediaPortal.UserInterface.Controls.MPButton buttonOk;
    private MediaPortal.UserInterface.Controls.MPButton buttonCancel;
    private MediaPortal.UserInterface.Controls.MPLabel labelPassword;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxASSettings;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxTeamStats;
    private System.Windows.Forms.LinkLabel linkLabelMPGroup;
    private System.Windows.Forms.LinkLabel linkLabelNewUser;
    private System.Windows.Forms.Panel panelPicBox;
    private System.Windows.Forms.PictureBox pictureBoxASLogo;
  }
}