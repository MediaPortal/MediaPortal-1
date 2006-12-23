#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace MediaPortal.GUI.Dreambox
{
    public partial class DreamboxSetupForm : Form
    {
        public DreamboxSetupForm()
        {
            InitializeComponent();
        }

        private void DreamboxSetupForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                MessageBox.Show(xmlreader.GetValue("mydreambox", "IP"));
                //edtUserName.Text = xmlreader.GetValue("mydreambox", "UserName");
                //edtPassword.Text = xmlreader.GetValue("mydreambox", "Password");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (SaveConfigFile() == true)
                this.Close();
        }

        bool SaveConfigFile()
        {
            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
              {
                  //xmlwriter.SetValue("mydreambox", "ipaddress", edtIPAddress.Text);
                  //xmlwriter.SetValue("mydreambox", "userName", edtUserName.Text);
                  //xmlwriter.SetValue("mydreambox", "password", edtPassword.Text);
              }
            return true;
        }

        private void InitializeComponent()
        {
            this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtIP = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.edtUserName = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtPassword = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mpGroupBox1
            // 
            this.mpGroupBox1.Controls.Add(this.edtPassword);
            this.mpGroupBox1.Controls.Add(this.mpLabel3);
            this.mpGroupBox1.Controls.Add(this.edtUserName);
            this.mpGroupBox1.Controls.Add(this.mpLabel2);
            this.mpGroupBox1.Controls.Add(this.edtIP);
            this.mpGroupBox1.Controls.Add(this.mpLabel1);
            this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox1.Location = new System.Drawing.Point(12, 12);
            this.mpGroupBox1.Name = "mpGroupBox1";
            this.mpGroupBox1.Size = new System.Drawing.Size(310, 144);
            this.mpGroupBox1.TabIndex = 0;
            this.mpGroupBox1.TabStop = false;
            this.mpGroupBox1.Text = "Dreambox Login Settings";
            // 
            // mpLabel1
            // 
            this.mpLabel1.AutoSize = true;
            this.mpLabel1.Location = new System.Drawing.Point(6, 45);
            this.mpLabel1.Name = "mpLabel1";
            this.mpLabel1.Size = new System.Drawing.Size(111, 13);
            this.mpLabel1.TabIndex = 0;
            this.mpLabel1.Text = "IP address Dreambox:";
            // 
            // edtIP
            // 
            this.edtIP.BorderColor = System.Drawing.Color.Empty;
            this.edtIP.Location = new System.Drawing.Point(123, 42);
            this.edtIP.Name = "edtIP";
            this.edtIP.Size = new System.Drawing.Size(157, 20);
            this.edtIP.TabIndex = 1;
            // 
            // edtUserName
            // 
            this.edtUserName.BorderColor = System.Drawing.Color.Empty;
            this.edtUserName.Location = new System.Drawing.Point(123, 68);
            this.edtUserName.Name = "edtUserName";
            this.edtUserName.Size = new System.Drawing.Size(157, 20);
            this.edtUserName.TabIndex = 3;
            // 
            // mpLabel2
            // 
            this.mpLabel2.AutoSize = true;
            this.mpLabel2.Location = new System.Drawing.Point(56, 71);
            this.mpLabel2.Name = "mpLabel2";
            this.mpLabel2.Size = new System.Drawing.Size(61, 13);
            this.mpLabel2.TabIndex = 2;
            this.mpLabel2.Text = "User name:";
            // 
            // edtPassword
            // 
            this.edtPassword.BorderColor = System.Drawing.Color.Empty;
            this.edtPassword.Location = new System.Drawing.Point(123, 94);
            this.edtPassword.Name = "edtPassword";
            this.edtPassword.Size = new System.Drawing.Size(157, 20);
            this.edtPassword.TabIndex = 5;
            // 
            // mpLabel3
            // 
            this.mpLabel3.AutoSize = true;
            this.mpLabel3.Location = new System.Drawing.Point(61, 97);
            this.mpLabel3.Name = "mpLabel3";
            this.mpLabel3.Size = new System.Drawing.Size(56, 13);
            this.mpLabel3.TabIndex = 4;
            this.mpLabel3.Text = "Password:";
            // 
            // mpButton1
            // 
            this.mpButton1.Location = new System.Drawing.Point(247, 163);
            this.mpButton1.Name = "mpButton1";
            this.mpButton1.Size = new System.Drawing.Size(75, 23);
            this.mpButton1.TabIndex = 1;
            this.mpButton1.Text = "Close";
            this.mpButton1.UseVisualStyleBackColor = true;
            this.mpButton1.Click += new System.EventHandler(this.mpButton1_Click);
            // 
            // DreamboxSetupForm
            // 
            this.ClientSize = new System.Drawing.Size(334, 201);
            this.ControlBox = false;
            this.Controls.Add(this.mpButton1);
            this.Controls.Add(this.mpGroupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DreamboxSetupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dreambox Settings";
            this.mpGroupBox1.ResumeLayout(false);
            this.mpGroupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        private void mpButton1_Click(object sender, EventArgs e)
        {
            SaveSettings();
            this.Close();
        }

        void LoadSettings()
        {
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                edtIP.Text = xmlreader.GetValue("mydreambox", "IP");
                edtUserName.Text = xmlreader.GetValue("mydreambox", "UserName");
                edtPassword.Text = xmlreader.GetValue("mydreambox", "Password");
            }
        }

        void SaveSettings()
        {
            if (edtIP.Text.Length == 0)
                return;

            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                xmlwriter.SetValue("mydreambox", "IP", edtIP.Text);
                xmlwriter.SetValue("mydreambox", "UserName", edtUserName.Text);
                xmlwriter.SetValue("mydreambox", "Password", edtPassword.Text);
            }
        }
    }
}