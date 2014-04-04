#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

namespace MediaPortal.ProcessPlugins.LastFMScrobbler
{
  partial class LastFMConfig
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LastFMConfig));
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.chkScrobble = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.chkAnnounce = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.pbLastFMUser = new System.Windows.Forms.PictureBox();
      this.btnAddUser = new System.Windows.Forms.Button();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.chkAvoidDuplicates = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.chkDiferentVersions = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.chkAutoDJ = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.numRandomness = new System.Windows.Forms.NumericUpDown();
      this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.mpGroupBox3.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbLastFMUser)).BeginInit();
      this.mpGroupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numRandomness)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(13, 13);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(157, 48);
      this.pictureBox1.TabIndex = 4;
      this.pictureBox1.TabStop = false;
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Controls.Add(this.chkScrobble);
      this.mpGroupBox3.Controls.Add(this.chkAnnounce);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(257, 111);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(205, 104);
      this.mpGroupBox3.TabIndex = 8;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Scrobbling";
      // 
      // chkScrobble
      // 
      this.chkScrobble.AutoSize = true;
      this.chkScrobble.Checked = true;
      this.chkScrobble.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkScrobble.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkScrobble.Location = new System.Drawing.Point(20, 58);
      this.chkScrobble.Name = "chkScrobble";
      this.chkScrobble.Size = new System.Drawing.Size(168, 17);
      this.chkScrobble.TabIndex = 1;
      this.chkScrobble.Text = "Scrobble Tracks to user profile";
      this.chkScrobble.UseVisualStyleBackColor = true;
      // 
      // chkAnnounce
      // 
      this.chkAnnounce.AutoSize = true;
      this.chkAnnounce.Checked = true;
      this.chkAnnounce.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkAnnounce.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkAnnounce.Location = new System.Drawing.Point(20, 31);
      this.chkAnnounce.Name = "chkAnnounce";
      this.chkAnnounce.Size = new System.Drawing.Size(163, 17);
      this.chkAnnounce.TabIndex = 0;
      this.chkAnnounce.Text = "Announce Tracks on website";
      this.chkAnnounce.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.pbLastFMUser);
      this.mpGroupBox2.Controls.Add(this.btnAddUser);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(187, 14);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(275, 88);
      this.mpGroupBox2.TabIndex = 7;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Authentication";
      // 
      // pbLastFMUser
      // 
      this.pbLastFMUser.Location = new System.Drawing.Point(20, 19);
      this.pbLastFMUser.Name = "pbLastFMUser";
      this.pbLastFMUser.Size = new System.Drawing.Size(100, 50);
      this.pbLastFMUser.TabIndex = 6;
      this.pbLastFMUser.TabStop = false;
      // 
      // btnAddUser
      // 
      this.btnAddUser.Location = new System.Drawing.Point(156, 46);
      this.btnAddUser.Name = "btnAddUser";
      this.btnAddUser.Size = new System.Drawing.Size(99, 23);
      this.btnAddUser.TabIndex = 5;
      this.btnAddUser.Text = "Add User";
      this.btnAddUser.UseVisualStyleBackColor = true;
      this.btnAddUser.Click += new System.EventHandler(this.btnWebAuthenticate_Click);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.chkAvoidDuplicates);
      this.mpGroupBox1.Controls.Add(this.chkDiferentVersions);
      this.mpGroupBox1.Controls.Add(this.chkAutoDJ);
      this.mpGroupBox1.Controls.Add(this.numRandomness);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(13, 111);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(238, 149);
      this.mpGroupBox1.TabIndex = 6;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Auto DJ";
      // 
      // chkAvoidDuplicates
      // 
      this.chkAvoidDuplicates.AutoSize = true;
      this.chkAvoidDuplicates.Checked = true;
      this.chkAvoidDuplicates.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkAvoidDuplicates.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkAvoidDuplicates.Location = new System.Drawing.Point(12, 87);
      this.chkAvoidDuplicates.Name = "chkAvoidDuplicates";
      this.chkAvoidDuplicates.Size = new System.Drawing.Size(206, 17);
      this.chkAvoidDuplicates.TabIndex = 6;
      this.chkAvoidDuplicates.Text = "Avoid adding same track multiple times";
      this.chkAvoidDuplicates.UseVisualStyleBackColor = true;
      // 
      // chkDiferentVersions
      // 
      this.chkDiferentVersions.AutoSize = true;
      this.chkDiferentVersions.Checked = true;
      this.chkDiferentVersions.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkDiferentVersions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkDiferentVersions.Location = new System.Drawing.Point(12, 58);
      this.chkDiferentVersions.Name = "chkDiferentVersions";
      this.chkDiferentVersions.Size = new System.Drawing.Size(217, 17);
      this.chkDiferentVersions.TabIndex = 5;
      this.chkDiferentVersions.Text = "Allow different versions of the same track";
      this.chkDiferentVersions.UseVisualStyleBackColor = true;
      // 
      // chkAutoDJ
      // 
      this.chkAutoDJ.AutoSize = true;
      this.chkAutoDJ.Checked = true;
      this.chkAutoDJ.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkAutoDJ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkAutoDJ.Location = new System.Drawing.Point(12, 31);
      this.chkAutoDJ.Name = "chkAutoDJ";
      this.chkAutoDJ.Size = new System.Drawing.Size(92, 17);
      this.chkAutoDJ.TabIndex = 4;
      this.chkAutoDJ.Text = "Auto DJ Mode";
      this.chkAutoDJ.UseVisualStyleBackColor = true;
      // 
      // numRandomness
      // 
      this.numRandomness.Location = new System.Drawing.Point(12, 114);
      this.numRandomness.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.numRandomness.Name = "numRandomness";
      this.numRandomness.Size = new System.Drawing.Size(120, 20);
      this.numRandomness.TabIndex = 3;
      this.numRandomness.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(295, 237);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 9;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(387, 237);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 10;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // LastFMConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(491, 287);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.mpGroupBox3);
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.pictureBox1);
      this.Name = "LastFMConfig";
      this.Text = "Scrobbler Configuration";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pbLastFMUser)).EndInit();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numRandomness)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.NumericUpDown numRandomness;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Button btnAddUser;
    private UserInterface.Controls.MPGroupBox mpGroupBox1;
    private UserInterface.Controls.MPGroupBox mpGroupBox2;
    private System.Windows.Forms.PictureBox pbLastFMUser;
    private UserInterface.Controls.MPGroupBox mpGroupBox3;
    private UserInterface.Controls.MPCheckBox chkScrobble;
    private UserInterface.Controls.MPCheckBox chkAnnounce;
    private UserInterface.Controls.MPButton btnOK;
    private UserInterface.Controls.MPButton btnCancel;
    private UserInterface.Controls.MPCheckBox chkAutoDJ;
    private UserInterface.Controls.MPCheckBox chkDiferentVersions;
    private UserInterface.Controls.MPCheckBox chkAvoidDuplicates;
  }
}