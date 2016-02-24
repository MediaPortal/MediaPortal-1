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
  partial class LastFMAuthentication
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LastFMAuthentication));
      this.txtUserName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.txtPassword = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.btnSubmit = new MediaPortal.UserInterface.Controls.MPButton();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.lblUserName = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblPassword = new MediaPortal.UserInterface.Controls.MPLabel();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // txtUserName
      // 
      this.txtUserName.BorderColor = System.Drawing.Color.Empty;
      this.txtUserName.Location = new System.Drawing.Point(90, 82);
      this.txtUserName.Name = "txtUserName";
      this.txtUserName.Size = new System.Drawing.Size(161, 20);
      this.txtUserName.TabIndex = 0;
      // 
      // txtPassword
      // 
      this.txtPassword.BorderColor = System.Drawing.Color.Empty;
      this.txtPassword.Location = new System.Drawing.Point(90, 117);
      this.txtPassword.Name = "txtPassword";
      this.txtPassword.Size = new System.Drawing.Size(161, 20);
      this.txtPassword.TabIndex = 1;
      this.txtPassword.UseSystemPasswordChar = true;
      this.txtPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPassword_KeyPress);
      // 
      // btnSubmit
      // 
      this.btnSubmit.Location = new System.Drawing.Point(197, 24);
      this.btnSubmit.Name = "btnSubmit";
      this.btnSubmit.Size = new System.Drawing.Size(75, 23);
      this.btnSubmit.TabIndex = 2;
      this.btnSubmit.Text = "Submit";
      this.btnSubmit.UseVisualStyleBackColor = true;
      this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(12, 12);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(157, 48);
      this.pictureBox1.TabIndex = 5;
      this.pictureBox1.TabStop = false;
      // 
      // lblUserName
      // 
      this.lblUserName.AutoSize = true;
      this.lblUserName.Location = new System.Drawing.Point(25, 82);
      this.lblUserName.Name = "lblUserName";
      this.lblUserName.Size = new System.Drawing.Size(60, 13);
      this.lblUserName.TabIndex = 6;
      this.lblUserName.Text = "UserName:";
      // 
      // lblPassword
      // 
      this.lblPassword.AutoSize = true;
      this.lblPassword.Location = new System.Drawing.Point(25, 120);
      this.lblPassword.Name = "lblPassword";
      this.lblPassword.Size = new System.Drawing.Size(56, 13);
      this.lblPassword.TabIndex = 7;
      this.lblPassword.Text = "Password:";
      // 
      // LastFMAuthentication
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 163);
      this.Controls.Add(this.lblPassword);
      this.Controls.Add(this.lblUserName);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.btnSubmit);
      this.Controls.Add(this.txtPassword);
      this.Controls.Add(this.txtUserName);
      this.Name = "LastFMAuthentication";
      this.Text = "LastFM Authentication";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPTextBox txtUserName;
    private MediaPortal.UserInterface.Controls.MPTextBox txtPassword;
    private MediaPortal.UserInterface.Controls.MPButton btnSubmit;
    private System.Windows.Forms.PictureBox pictureBox1;
    private MediaPortal.UserInterface.Controls.MPLabel lblUserName;
    private MediaPortal.UserInterface.Controls.MPLabel lblPassword;
  }
}