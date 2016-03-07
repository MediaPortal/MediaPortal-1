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
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using MediaPortal.Util;
using MediaPortal.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for EditShare.
  /// </summary>
  public class EditShareForm : MPConfigForm
  {
    private MPGroupBox groupBox1;
    private MPButton folderButton;
    private MPTextBox folderTextBox;
    private MPLabel label2;
    private MPTextBox nameTextBox;
    private MPLabel label1;
    private MPButton cancelButton;
    private MPButton okButton;
    private FolderBrowserDialog folderBrowserDialog;
    private MPLabel label4;
    private MPTextBox pinCodeTextBox;
    private MPCheckBox checkBoxRemote;
    private MPLabel label5;
    private MPTextBox textBoxServer;
    private MPLabel label6;
    private MPTextBox textBoxLogin;
    private MPLabel label7;
    private MPTextBox textBoxPassword;
    private MPTextBox textBoxPort;
    private MPLabel label8;
    private MPTextBox textBoxRemoteFolder;
    private MPLabel label3;
    private MPLabel label9;
    private MPComboBox comboBox1;
    private MPCheckBox checkBoxPASV;
    public MPCheckBox cbCreateThumbs;
    public MPLabel labelCreateThumbs;
    public MPCheckBox cbEachFolderIsMovie;
    private ToolTip toolTipEditShare;
    private MPCheckBox cbEnableWakeOnLan;
    private MPButton mpButtonLearnMacNow;
    private MPCheckBox mpCBdonotFolderJpgIfPin;
    private IContainer components;

    public EditShareForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // Set password character
      //
      pinCodeTextBox.PasswordChar = (char)0x25CF;
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditShareForm));
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCBdonotFolderJpgIfPin = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpButtonLearnMacNow = new MediaPortal.UserInterface.Controls.MPButton();
      this.cbEnableWakeOnLan = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbEachFolderIsMovie = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbCreateThumbs = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.labelCreateThumbs = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxPASV = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.comboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxRemoteFolder = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxPort = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxPassword = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxLogin = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxServer = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxRemote = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.pinCodeTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.folderButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.nameTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.toolTipEditShare = new System.Windows.Forms.ToolTip(this.components);
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.mpCBdonotFolderJpgIfPin);
      this.groupBox1.Controls.Add(this.mpButtonLearnMacNow);
      this.groupBox1.Controls.Add(this.cbEnableWakeOnLan);
      this.groupBox1.Controls.Add(this.cbEachFolderIsMovie);
      this.groupBox1.Controls.Add(this.cbCreateThumbs);
      this.groupBox1.Controls.Add(this.labelCreateThumbs);
      this.groupBox1.Controls.Add(this.checkBoxPASV);
      this.groupBox1.Controls.Add(this.comboBox1);
      this.groupBox1.Controls.Add(this.label9);
      this.groupBox1.Controls.Add(this.textBoxRemoteFolder);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label8);
      this.groupBox1.Controls.Add(this.textBoxPort);
      this.groupBox1.Controls.Add(this.textBoxPassword);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.textBoxLogin);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.textBoxServer);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.checkBoxRemote);
      this.groupBox1.Controls.Add(this.pinCodeTextBox);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.folderButton);
      this.groupBox1.Controls.Add(this.folderTextBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.nameTextBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(408, 354);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Folder settings";
      this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
      // 
      // mpCBdonotFolderJpgIfPin
      // 
      this.mpCBdonotFolderJpgIfPin.AutoSize = true;
      this.mpCBdonotFolderJpgIfPin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCBdonotFolderJpgIfPin.Location = new System.Drawing.Point(16, 65);
      this.mpCBdonotFolderJpgIfPin.Name = "mpCBdonotFolderJpgIfPin";
      this.mpCBdonotFolderJpgIfPin.Size = new System.Drawing.Size(248, 17);
      this.mpCBdonotFolderJpgIfPin.TabIndex = 33;
      this.mpCBdonotFolderJpgIfPin.Text = "Do not display folder.jpg when a Pin Code is set";
      this.mpCBdonotFolderJpgIfPin.UseVisualStyleBackColor = true;
      // 
      // mpButtonLearnMacNow
      // 
      this.mpButtonLearnMacNow.Location = new System.Drawing.Point(251, 175);
      this.mpButtonLearnMacNow.Name = "mpButtonLearnMacNow";
      this.mpButtonLearnMacNow.Size = new System.Drawing.Size(141, 23);
      this.mpButtonLearnMacNow.TabIndex = 32;
      this.mpButtonLearnMacNow.Text = "Learn MAC address now";
      this.mpButtonLearnMacNow.UseVisualStyleBackColor = true;
      this.mpButtonLearnMacNow.Click += new System.EventHandler(this.mpButton1_Click);
      // 
      // cbEnableWakeOnLan
      // 
      this.cbEnableWakeOnLan.AutoSize = true;
      this.cbEnableWakeOnLan.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbEnableWakeOnLan.Location = new System.Drawing.Point(16, 179);
      this.cbEnableWakeOnLan.Name = "cbEnableWakeOnLan";
      this.cbEnableWakeOnLan.Size = new System.Drawing.Size(127, 17);
      this.cbEnableWakeOnLan.TabIndex = 31;
      this.cbEnableWakeOnLan.Text = "Enable Wake On Lan";
      this.cbEnableWakeOnLan.UseVisualStyleBackColor = true;
      // 
      // cbEachFolderIsMovie
      // 
      this.cbEachFolderIsMovie.AutoSize = true;
      this.cbEachFolderIsMovie.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbEachFolderIsMovie.Location = new System.Drawing.Point(259, 149);
      this.cbEachFolderIsMovie.Name = "cbEachFolderIsMovie";
      this.cbEachFolderIsMovie.Size = new System.Drawing.Size(133, 17);
      this.cbEachFolderIsMovie.TabIndex = 29;
      this.cbEachFolderIsMovie.Text = "Dedicated movie folder";
      this.toolTipEditShare.SetToolTip(this.cbEachFolderIsMovie, resources.GetString("cbEachFolderIsMovie.ToolTip"));
      this.cbEachFolderIsMovie.UseVisualStyleBackColor = true;
      this.cbEachFolderIsMovie.Visible = false;
      // 
      // cbCreateThumbs
      // 
      this.cbCreateThumbs.AutoSize = true;
      this.cbCreateThumbs.Checked = true;
      this.cbCreateThumbs.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbCreateThumbs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbCreateThumbs.Location = new System.Drawing.Point(325, 42);
      this.cbCreateThumbs.Name = "cbCreateThumbs";
      this.cbCreateThumbs.Size = new System.Drawing.Size(13, 12);
      this.cbCreateThumbs.TabIndex = 28;
      this.cbCreateThumbs.UseVisualStyleBackColor = true;
      this.cbCreateThumbs.Visible = false;
      // 
      // labelCreateThumbs
      // 
      this.labelCreateThumbs.Location = new System.Drawing.Point(293, 23);
      this.labelCreateThumbs.Name = "labelCreateThumbs";
      this.labelCreateThumbs.Size = new System.Drawing.Size(87, 16);
      this.labelCreateThumbs.TabIndex = 27;
      this.labelCreateThumbs.Text = "Create Thumbs";
      this.labelCreateThumbs.Visible = false;
      // 
      // checkBoxPASV
      // 
      this.checkBoxPASV.AutoSize = true;
      this.checkBoxPASV.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxPASV.Location = new System.Drawing.Point(343, 244);
      this.checkBoxPASV.Name = "checkBoxPASV";
      this.checkBoxPASV.Size = new System.Drawing.Size(52, 17);
      this.checkBoxPASV.TabIndex = 26;
      this.checkBoxPASV.Text = "PASV";
      this.checkBoxPASV.UseVisualStyleBackColor = true;
      this.checkBoxPASV.Visible = false;
      // 
      // comboBox1
      // 
      this.comboBox1.BorderColor = System.Drawing.Color.Empty;
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.Items.AddRange(new object[] {
            "List",
            "Icons",
            "Big Icons",
            "Album",
            "Filmstrip",
            "Cover Flow"});
      this.comboBox1.Location = new System.Drawing.Point(16, 145);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(184, 21);
      this.comboBox1.TabIndex = 25;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(16, 129);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(75, 13);
      this.label9.TabIndex = 24;
      this.label9.Text = "Default layout:";
      // 
      // textBoxRemoteFolder
      // 
      this.textBoxRemoteFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxRemoteFolder.BorderColor = System.Drawing.Color.Empty;
      this.textBoxRemoteFolder.Location = new System.Drawing.Point(16, 323);
      this.textBoxRemoteFolder.Name = "textBoxRemoteFolder";
      this.textBoxRemoteFolder.Size = new System.Drawing.Size(381, 20);
      this.textBoxRemoteFolder.TabIndex = 8;
      this.textBoxRemoteFolder.Text = "/";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 307);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(168, 23);
      this.label3.TabIndex = 23;
      this.label3.Text = "Folder on the remote ftp server";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(288, 225);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(68, 13);
      this.label8.TabIndex = 22;
      this.label8.Text = "TCP/IP Port:";
      // 
      // textBoxPort
      // 
      this.textBoxPort.BorderColor = System.Drawing.Color.Empty;
      this.textBoxPort.Location = new System.Drawing.Point(291, 243);
      this.textBoxPort.Name = "textBoxPort";
      this.textBoxPort.Size = new System.Drawing.Size(38, 20);
      this.textBoxPort.TabIndex = 7;
      this.textBoxPort.Text = "21";
      this.textBoxPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // textBoxPassword
      // 
      this.textBoxPassword.BorderColor = System.Drawing.Color.Empty;
      this.textBoxPassword.Location = new System.Drawing.Point(213, 283);
      this.textBoxPassword.Name = "textBoxPassword";
      this.textBoxPassword.PasswordChar = '*';
      this.textBoxPassword.Size = new System.Drawing.Size(184, 20);
      this.textBoxPassword.TabIndex = 6;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(213, 267);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(100, 16);
      this.label7.TabIndex = 19;
      this.label7.Text = "Password:";
      // 
      // textBoxLogin
      // 
      this.textBoxLogin.BorderColor = System.Drawing.Color.Empty;
      this.textBoxLogin.Location = new System.Drawing.Point(16, 283);
      this.textBoxLogin.Name = "textBoxLogin";
      this.textBoxLogin.Size = new System.Drawing.Size(184, 20);
      this.textBoxLogin.TabIndex = 5;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 267);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(100, 16);
      this.label6.TabIndex = 17;
      this.label6.Text = "Login:";
      // 
      // textBoxServer
      // 
      this.textBoxServer.BorderColor = System.Drawing.Color.Empty;
      this.textBoxServer.Location = new System.Drawing.Point(16, 243);
      this.textBoxServer.Name = "textBoxServer";
      this.textBoxServer.Size = new System.Drawing.Size(263, 20);
      this.textBoxServer.TabIndex = 4;
      this.textBoxServer.Text = "127.0.0.1";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(16, 225);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(135, 13);
      this.label5.TabIndex = 15;
      this.label5.Text = "FTP Server (without ftp://):";
      // 
      // checkBoxRemote
      // 
      this.checkBoxRemote.AutoSize = true;
      this.checkBoxRemote.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxRemote.Location = new System.Drawing.Point(16, 203);
      this.checkBoxRemote.Name = "checkBoxRemote";
      this.checkBoxRemote.Size = new System.Drawing.Size(153, 17);
      this.checkBoxRemote.TabIndex = 3;
      this.checkBoxRemote.Text = "This is a remote FTP Folder";
      this.checkBoxRemote.UseVisualStyleBackColor = true;
      this.checkBoxRemote.CheckedChanged += new System.EventHandler(this.checkBoxRemote_CheckedChanged);
      // 
      // pinCodeTextBox
      // 
      this.pinCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pinCodeTextBox.BorderColor = System.Drawing.Color.Empty;
      this.pinCodeTextBox.Location = new System.Drawing.Point(216, 39);
      this.pinCodeTextBox.Name = "pinCodeTextBox";
      this.pinCodeTextBox.Size = new System.Drawing.Size(71, 20);
      this.pinCodeTextBox.TabIndex = 1;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(216, 23);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(63, 16);
      this.label4.TabIndex = 12;
      this.label4.Text = "Pin Code";
      // 
      // folderButton
      // 
      this.folderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.folderButton.Location = new System.Drawing.Point(368, 103);
      this.folderButton.Name = "folderButton";
      this.folderButton.Size = new System.Drawing.Size(24, 20);
      this.folderButton.TabIndex = 3;
      this.folderButton.Text = "...";
      this.folderButton.UseVisualStyleBackColor = true;
      this.folderButton.Click += new System.EventHandler(this.folderButton_Click);
      // 
      // folderTextBox
      // 
      this.folderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.folderTextBox.BorderColor = System.Drawing.Color.Empty;
      this.folderTextBox.Location = new System.Drawing.Point(16, 103);
      this.folderTextBox.Name = "folderTextBox";
      this.folderTextBox.Size = new System.Drawing.Size(344, 20);
      this.folderTextBox.TabIndex = 2;
      this.folderTextBox.TextChanged += new System.EventHandler(this.folderTextBox_TextChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 87);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(155, 13);
      this.label2.TabIndex = 8;
      this.label2.Text = "Media folder / optical disk drive";
      // 
      // nameTextBox
      // 
      this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.nameTextBox.BorderColor = System.Drawing.Color.Empty;
      this.nameTextBox.Location = new System.Drawing.Point(16, 39);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(184, 20);
      this.nameTextBox.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 23);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 6;
      this.label1.Text = "Visual name";
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(342, 369);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(262, 369);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 0;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // EditShareForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(426, 402);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(424, 248);
      this.Name = "EditShareForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit Folder";
      this.Load += new System.EventHandler(this.EditShareForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private void folderButton_Click(object sender, EventArgs e)
    {
      using (folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = "Select the location of the share folder";
        folderBrowserDialog.ShowNewFolderButton = true;
        folderBrowserDialog.SelectedPath = folderTextBox.Text;
        DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          folderTextBox.Text = folderBrowserDialog.SelectedPath;
        }
      }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      if (nameTextBox.Text == string.Empty)
      {
        MessageBox.Show("No name specified.Please fill in a name for this folder", "MediaPortal Settings",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }

      if (folderTextBox.Text == string.Empty)
      {
        MessageBox.Show("No local folder specified.Please choose a local folder", "MediaPortal Settings",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      if (checkBoxRemote.Checked)
      {
        if (textBoxRemoteFolder.Text == string.Empty)
        {
          MessageBox.Show("No remote ftp folder specified.Please choose a remote folder", "MediaPortal Settings",
                          MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
        if (textBoxServer.Text == string.Empty)
        {
          MessageBox.Show("No remote ftp server specified.Please choose a remote ftp server", "MediaPortal Settings",
                          MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
        if (textBoxLogin.Text == string.Empty)
        {
          MessageBox.Show("No login for the remote ftp server specified.Please choose a loginname",
                          "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
        if (textBoxPassword.Text == string.Empty)
        {
          MessageBox.Show("No password for the remote ftp server specified.Please choose a password",
                          "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
        if (textBoxPort.Text == string.Empty)
        {
          MessageBox.Show("No TCP/IP port for the remote ftp server specified.Please specify a TCP/IP port",
                          "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
      }
      this.DialogResult = DialogResult.OK;
      this.Hide();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    private void checkBoxRemote_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxRemote.Checked)
      {
        textBoxLogin.Enabled = true;
        textBoxPassword.Enabled = true;
        textBoxPort.Enabled = true;
        textBoxServer.Enabled = true;
        textBoxRemoteFolder.Enabled = true;
      }
      else
      {
        textBoxLogin.Enabled = false;
        textBoxPassword.Enabled = false;
        textBoxPort.Enabled = false;
        textBoxServer.Enabled = false;
        textBoxRemoteFolder.Enabled = false;
      }
    }

    private void groupBox1_Enter(object sender, EventArgs e) { }

    private void EditShareForm_Load(object sender, EventArgs e)
    {
      bool remote = IsRemote;
      checkBoxRemote.Checked = !remote;
      checkBoxRemote.Checked = remote;
      if (comboBox1.SelectedIndex < 0)
      {
        comboBox1.SelectedIndex = 0;
      }
    }

    public string Folder
    {
      get { return folderTextBox.Text; }
      set { folderTextBox.Text = value; }
    }

    public string ShareName
    {
      get { return nameTextBox.Text; }
      set { nameTextBox.Text = value; }
    }

    public string PinCode
    {
      get { return pinCodeTextBox.Text; }
      set { pinCodeTextBox.Text = value; }
    }

    public bool IsRemote
    {
      get { return checkBoxRemote.Checked; }
      set { checkBoxRemote.Checked = value; }
    }

    public bool DonotFolderJpgIfPin
    {
      get { return mpCBdonotFolderJpgIfPin.Checked; }
      set { mpCBdonotFolderJpgIfPin.Checked = value; }
    }

    public int Port
    {
      get
      {
        int port = 21;
        try
        {
          port = Int32.Parse(textBoxPort.Text);
        }
        catch (Exception) { }
        return port;
      }
      set { textBoxPort.Text = value.ToString(); }
    }

    public bool ActiveConnection
    {
      get { return checkBoxPASV.Checked; }
      set { checkBoxPASV.Checked = value; }
    }

    public string Server
    {
      get { return textBoxServer.Text; }
      set { textBoxServer.Text = value; }
    }

    public string LoginName
    {
      get { return textBoxLogin.Text; }
      set { textBoxLogin.Text = value; }
    }

    public string PassWord
    {
      get { return textBoxPassword.Text; }
      set { textBoxPassword.Text = value; }
    }

    public string RemoteFolder
    {
      get { return textBoxRemoteFolder.Text; }
      set { textBoxRemoteFolder.Text = value; }
    }

    public int View
    {
      get { return comboBox1.SelectedIndex; }
      set { comboBox1.SelectedIndex = value; }
    }

    public bool CreateThumbs
    {
      get { return cbCreateThumbs.Checked; }
      set { cbCreateThumbs.Checked = value; }
    }

    public bool EachFolderIsMovie
    {
      get { return cbEachFolderIsMovie.Checked; }
      set { cbEachFolderIsMovie.Checked = value; }
    }

    public bool EnableWakeOnLan
    {
      get { return cbEnableWakeOnLan.Checked; }
      set { cbEnableWakeOnLan.Checked = value; }
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      String macAddress;
      byte[] hwAddress;
      string hostName = "";

      WakeOnLanManager wakeOnLanManager = new WakeOnLanManager();

      IPAddress ipAddress = null;
      string detectedFolderName = "";
      if (!Util.Utils.IsUNCNetwork(folderTextBox.Text))
      {
        // Check if letter drive is a network drive
        detectedFolderName = Util.Utils.FindUNCPaths(folderTextBox.Text);
      }
      if (Util.Utils.IsUNCNetwork(detectedFolderName))
      {
        hostName = Util.Utils.GetServerNameFromUNCPath(detectedFolderName);
      }
      else if (Util.Utils.IsUNCNetwork(folderTextBox.Text))
      {
        hostName = Util.Utils.GetServerNameFromUNCPath(folderTextBox.Text);
      }

      if (string.IsNullOrEmpty(hostName))
      {
        MessageBox.Show("Wrong unc path " + folderTextBox.Text,
          "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        Log.Debug("Wrong unc path {0}", folderTextBox.Text);
        return;
      }

      using (Profile.Settings xmlreader = new MPSettings())
      {
        macAddress = xmlreader.GetValueAsString("macAddress", hostName, null);
      }

      // Check if we already have a valid IP address stored,
      // otherwise try to resolve the IP address
      if (!IPAddress.TryParse(hostName, out ipAddress))
      {
        // Get IP address of the server
        try
        {
          IPAddress[] ips;

          ips = Dns.GetHostAddresses(hostName);

          Log.Debug("WakeUpServer: WOL - GetHostAddresses({0}) returns:", hostName);

          foreach (IPAddress ip in ips)
          {
            Log.Debug("    {0}", ip);

            ipAddress = ip;
            // Check for valid IP address
            if (ipAddress != null)
            {
              // Update the MAC address if possible
              hwAddress = wakeOnLanManager.GetHardwareAddress(ipAddress);

              if (wakeOnLanManager.IsValidEthernetAddress(hwAddress))
              {
                Log.Debug("WakeUpServer: WOL - Valid auto MAC address: {0:x}:{1:x}:{2:x}:{3:x}:{4:x}:{5:x}"
                  , hwAddress[0], hwAddress[1], hwAddress[2], hwAddress[3], hwAddress[4], hwAddress[5]);

                // Store MAC address
                macAddress = BitConverter.ToString(hwAddress).Replace("-", ":");

                Log.Debug("WakeUpServer: WOL - Store MAC address: {0}", macAddress);

                using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.MPSettings())
                {
                  xmlwriter.SetValue("macAddress", hostName, macAddress);
                }
                MessageBox.Show("Stored MAC address: " + macAddress, "MediaPortal Settings",
                  MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
              }
              else
              {
                MessageBox.Show("WakeUpServer: WOL - Not a valid IPv4 address: " + ipAddress, "MediaPortal Settings",
                  MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Log.Debug("WakeUpServer: WOL - Not a valid IPv4 address: {0}", ipAddress);
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("WakeUpServer: WOL - Failed GetHostAddress - {0}", ex.Message);
        }
      }
    }

    private void folderTextBox_TextChanged(object sender, EventArgs e)
    {
      string detectedFolderName = "";
      if (!Util.Utils.IsUNCNetwork(folderTextBox.Text))
      {
        // Check if letter drive is a network drive
        detectedFolderName = Util.Utils.FindUNCPaths(folderTextBox.Text);
      }
      if (Util.Utils.IsUNCNetwork(detectedFolderName) || Util.Utils.IsUNCNetwork(folderTextBox.Text))
      {
        cbEnableWakeOnLan.Enabled = true;
      }
      else
      {
        cbEnableWakeOnLan.Checked = false;
        cbEnableWakeOnLan.Enabled = false;
      }
    }
  }
}