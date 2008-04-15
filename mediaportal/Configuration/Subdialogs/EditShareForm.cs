#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for EditShare.
  /// </summary>
  public class EditShareForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPButton folderButton;
    private MediaPortal.UserInterface.Controls.MPTextBox folderTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPTextBox nameTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPButton cancelButton;
    private MediaPortal.UserInterface.Controls.MPButton okButton;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPTextBox pinCodeTextBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxRemote;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxServer;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxLogin;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxPassword;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxPort;
    private MediaPortal.UserInterface.Controls.MPLabel label8;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxRemoteFolder;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label9;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxPASV;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
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
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
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
      // checkBoxPASV
      // 
      this.checkBoxPASV.AutoSize = true;
      this.checkBoxPASV.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxPASV.Location = new System.Drawing.Point(343, 241);
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
            "Filmstrip"});
      this.comboBox1.Location = new System.Drawing.Point(16, 136);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(184, 21);
      this.comboBox1.TabIndex = 25;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(16, 120);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(69, 13);
      this.label9.TabIndex = 24;
      this.label9.Text = "Default view:";
      // 
      // textBoxRemoteFolder
      // 
      this.textBoxRemoteFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxRemoteFolder.BorderColor = System.Drawing.Color.Empty;
      this.textBoxRemoteFolder.Location = new System.Drawing.Point(16, 320);
      this.textBoxRemoteFolder.Name = "textBoxRemoteFolder";
      this.textBoxRemoteFolder.Size = new System.Drawing.Size(381, 20);
      this.textBoxRemoteFolder.TabIndex = 8;
      this.textBoxRemoteFolder.Text = "/";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 304);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(168, 23);
      this.label3.TabIndex = 23;
      this.label3.Text = "folder on the remote ftp server";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(288, 222);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(68, 13);
      this.label8.TabIndex = 22;
      this.label8.Text = "TCP/IP Port:";
      // 
      // textBoxPort
      // 
      this.textBoxPort.BorderColor = System.Drawing.Color.Empty;
      this.textBoxPort.Location = new System.Drawing.Point(291, 240);
      this.textBoxPort.Name = "textBoxPort";
      this.textBoxPort.Size = new System.Drawing.Size(38, 20);
      this.textBoxPort.TabIndex = 7;
      this.textBoxPort.Text = "21";
      this.textBoxPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // textBoxPassword
      // 
      this.textBoxPassword.BorderColor = System.Drawing.Color.Empty;
      this.textBoxPassword.Location = new System.Drawing.Point(213, 280);
      this.textBoxPassword.Name = "textBoxPassword";
      this.textBoxPassword.PasswordChar = '*';
      this.textBoxPassword.Size = new System.Drawing.Size(184, 20);
      this.textBoxPassword.TabIndex = 6;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(213, 264);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(100, 16);
      this.label7.TabIndex = 19;
      this.label7.Text = "Password:";
      // 
      // textBoxLogin
      // 
      this.textBoxLogin.BorderColor = System.Drawing.Color.Empty;
      this.textBoxLogin.Location = new System.Drawing.Point(16, 280);
      this.textBoxLogin.Name = "textBoxLogin";
      this.textBoxLogin.Size = new System.Drawing.Size(184, 20);
      this.textBoxLogin.TabIndex = 5;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 264);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(100, 16);
      this.label6.TabIndex = 17;
      this.label6.Text = "Login:";
      // 
      // textBoxServer
      // 
      this.textBoxServer.BorderColor = System.Drawing.Color.Empty;
      this.textBoxServer.Location = new System.Drawing.Point(16, 240);
      this.textBoxServer.Name = "textBoxServer";
      this.textBoxServer.Size = new System.Drawing.Size(263, 20);
      this.textBoxServer.TabIndex = 4;
      this.textBoxServer.Text = "127.0.0.1";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(16, 222);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(135, 13);
      this.label5.TabIndex = 15;
      this.label5.Text = "FTP Server (without ftp://):";
      // 
      // checkBoxRemote
      // 
      this.checkBoxRemote.AutoSize = true;
      this.checkBoxRemote.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxRemote.Location = new System.Drawing.Point(16, 200);
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
      this.pinCodeTextBox.Location = new System.Drawing.Point(216, 40);
      this.pinCodeTextBox.MaxLength = 4;
      this.pinCodeTextBox.Name = "pinCodeTextBox";
      this.pinCodeTextBox.Size = new System.Drawing.Size(63, 20);
      this.pinCodeTextBox.TabIndex = 1;
      this.pinCodeTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.pinCodeTextBox_KeyPress);
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(216, 24);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(168, 16);
      this.label4.TabIndex = 12;
      this.label4.Text = "Pin Code";
      // 
      // folderButton
      // 
      this.folderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.folderButton.Location = new System.Drawing.Point(368, 88);
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
      this.folderTextBox.Location = new System.Drawing.Point(16, 88);
      this.folderTextBox.Name = "folderTextBox";
      this.folderTextBox.Size = new System.Drawing.Size(344, 20);
      this.folderTextBox.TabIndex = 2;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 72);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(62, 13);
      this.label2.TabIndex = 8;
      this.label2.Text = "Local folder";
      // 
      // nameTextBox
      // 
      this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.nameTextBox.BorderColor = System.Drawing.Color.Empty;
      this.nameTextBox.Location = new System.Drawing.Point(16, 40);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(184, 20);
      this.nameTextBox.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 24);
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
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
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

    private void folderButton_Click(object sender, System.EventArgs e)
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

    private void okButton_Click(object sender, System.EventArgs e)
    {
      if (nameTextBox.Text == string.Empty)
      {
        MessageBox.Show("No name specified.Please fill in a name for this folder", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;

      }

      if (folderTextBox.Text == string.Empty)
      {
        MessageBox.Show("No local folder specified.Please choose a local folder", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      if (checkBoxRemote.Checked)
      {
        if (textBoxRemoteFolder.Text == string.Empty)
        {
          MessageBox.Show("No remote ftp folder specified.Please choose a remote folder", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
        if (textBoxServer.Text == string.Empty)
        {
          MessageBox.Show("No remote ftp server specified.Please choose a remote ftp server", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
        if (textBoxLogin.Text == string.Empty)
        {
          MessageBox.Show("No login for the remote ftp server specified.Please choose a loginname", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
        if (textBoxPassword.Text == string.Empty)
        {
          MessageBox.Show("No password for the remote ftp server specified.Please choose a password", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
        if (textBoxPort.Text == string.Empty)
        {
          MessageBox.Show("No TCP/IP port for the remote ftp server specified.Please specify a TCP/IP port", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
      }
      this.DialogResult = DialogResult.OK;
      this.Hide();
    }

    private void cancelButton_Click(object sender, System.EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    private void pinCodeTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, and backspace.
      //
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void checkBoxRemote_CheckedChanged(object sender, System.EventArgs e)
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

    private void groupBox1_Enter(object sender, System.EventArgs e)
    {

    }

    private void EditShareForm_Load(object sender, System.EventArgs e)
    {
      bool remote = IsRemote;
      checkBoxRemote.Checked = !remote;
      checkBoxRemote.Checked = remote;
      if (comboBox1.SelectedIndex < 0) comboBox1.SelectedIndex = 0;
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
  }
}
