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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;
using Core.Util;

namespace MyMail
{
  /// <summary>
  /// Summary description for MailDetailSetup.
  /// </summary>
  public class MailDetailSetup : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPButton btnOk;
    private MediaPortal.UserInterface.Controls.MPButton btnCancel;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbConnection;
    private MediaPortal.UserInterface.Controls.MPLabel lblServerAddress;
    private MediaPortal.UserInterface.Controls.MPLabel lblUsername;
    private MediaPortal.UserInterface.Controls.MPLabel lblPassword;
    private MediaPortal.UserInterface.Controls.MPLabel lblPort;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbStorage;
    private MediaPortal.UserInterface.Controls.MPLabel lblMailboxFolder;
    private MediaPortal.UserInterface.Controls.MPLabel lblAttachmentFolder;
    private MediaPortal.UserInterface.Controls.MPTextBox tbServerAddress;
    private MediaPortal.UserInterface.Controls.MPTextBox tbPort;
    private MediaPortal.UserInterface.Controls.MPTextBox tbUsername;
    private MediaPortal.UserInterface.Controls.MPTextBox tbPassword;
    private MediaPortal.UserInterface.Controls.MPTextBox tbMailboxFolder;
    private MediaPortal.UserInterface.Controls.MPTextBox tbAttachmentFolder;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbGeneral;
    private MediaPortal.UserInterface.Controls.MPTextBox tbBoxLabel;
    private MediaPortal.UserInterface.Controls.MPLabel lblBoxLabel;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbEnabled;
    private System.Windows.Forms.FolderBrowserDialog dialogFolder;
    private MediaPortal.UserInterface.Controls.MPButton btnMailboxFolder;
    private MediaPortal.UserInterface.Controls.MPButton btnAttachmentFolder;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    MailBox m_CurMailBox = null;
    private MediaPortal.UserInterface.Controls.MPButton btnTest;
    private RadioButton rdbSTLSCommand;
    private RadioButton rdbSSLPort;
    private RadioButton rdbNoSSL;
    private MediaPortal.UserInterface.Controls.MPLabel lblSSL;
    private Panel panelSSL;

    ConditionChecker checker = new ConditionChecker();

    public MailDetailSetup()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MailDetailSetup));
      this.btnOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.gbConnection = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.panelSSL = new System.Windows.Forms.Panel();
      this.rdbSTLSCommand = new System.Windows.Forms.RadioButton();
      this.rdbSSLPort = new System.Windows.Forms.RadioButton();
      this.rdbNoSSL = new System.Windows.Forms.RadioButton();
      this.lblSSL = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnTest = new MediaPortal.UserInterface.Controls.MPButton();
      this.tbPassword = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbUsername = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbPort = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbServerAddress = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblPort = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblPassword = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblUsername = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblServerAddress = new MediaPortal.UserInterface.Controls.MPLabel();
      this.gbStorage = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.btnAttachmentFolder = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnMailboxFolder = new MediaPortal.UserInterface.Controls.MPButton();
      this.tbAttachmentFolder = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbMailboxFolder = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblAttachmentFolder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblMailboxFolder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.gbGeneral = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tbBoxLabel = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblBoxLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.dialogFolder = new System.Windows.Forms.FolderBrowserDialog();
      this.gbConnection.SuspendLayout();
      this.panelSSL.SuspendLayout();
      this.gbStorage.SuspendLayout();
      this.gbGeneral.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.Location = new System.Drawing.Point(296, 372);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(75, 23);
      this.btnOk.TabIndex = 3;
      this.btnOk.Text = "OK";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(376, 372);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 4;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // gbConnection
      // 
      this.gbConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbConnection.Controls.Add(this.panelSSL);
      this.gbConnection.Controls.Add(this.lblSSL);
      this.gbConnection.Controls.Add(this.btnTest);
      this.gbConnection.Controls.Add(this.tbPassword);
      this.gbConnection.Controls.Add(this.tbUsername);
      this.gbConnection.Controls.Add(this.tbPort);
      this.gbConnection.Controls.Add(this.tbServerAddress);
      this.gbConnection.Controls.Add(this.lblPort);
      this.gbConnection.Controls.Add(this.lblPassword);
      this.gbConnection.Controls.Add(this.lblUsername);
      this.gbConnection.Controls.Add(this.lblServerAddress);
      this.gbConnection.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbConnection.Location = new System.Drawing.Point(8, 88);
      this.gbConnection.Name = "gbConnection";
      this.gbConnection.Size = new System.Drawing.Size(448, 191);
      this.gbConnection.TabIndex = 1;
      this.gbConnection.TabStop = false;
      this.gbConnection.Text = "Connection";
      // 
      // panelSSL
      // 
      this.panelSSL.Controls.Add(this.rdbSTLSCommand);
      this.panelSSL.Controls.Add(this.rdbSSLPort);
      this.panelSSL.Controls.Add(this.rdbNoSSL);
      this.panelSSL.Location = new System.Drawing.Point(112, 56);
      this.panelSSL.Name = "panelSSL";
      this.panelSSL.Size = new System.Drawing.Size(316, 20);
      this.panelSSL.TabIndex = 4;
      // 
      // rdbSTLSCommand
      // 
      this.rdbSTLSCommand.AutoSize = true;
      this.rdbSTLSCommand.Location = new System.Drawing.Point(156, 0);
      this.rdbSTLSCommand.Name = "rdbSTLSCommand";
      this.rdbSTLSCommand.Size = new System.Drawing.Size(106, 17);
      this.rdbSTLSCommand.TabIndex = 3;
      this.rdbSTLSCommand.Text = "Explicit SSL/TLS";
      this.rdbSTLSCommand.UseVisualStyleBackColor = true;
      this.rdbSTLSCommand.CheckedChanged += new System.EventHandler(this.rdbSTLSCommand_CheckedChanged);
      // 
      // rdbSSLPort
      // 
      this.rdbSSLPort.AutoSize = true;
      this.rdbSSLPort.Location = new System.Drawing.Point(45, 0);
      this.rdbSSLPort.Name = "rdbSSLPort";
      this.rdbSSLPort.Size = new System.Drawing.Size(105, 17);
      this.rdbSSLPort.TabIndex = 2;
      this.rdbSSLPort.Text = "Implicit SSL/TLS";
      this.rdbSSLPort.UseVisualStyleBackColor = true;
      this.rdbSSLPort.CheckedChanged += new System.EventHandler(this.rdbSSLPort_CheckedChanged);
      // 
      // rdbNoSSL
      // 
      this.rdbNoSSL.AutoSize = true;
      this.rdbNoSSL.Checked = true;
      this.rdbNoSSL.Location = new System.Drawing.Point(0, 0);
      this.rdbNoSSL.Name = "rdbNoSSL";
      this.rdbNoSSL.Size = new System.Drawing.Size(39, 17);
      this.rdbNoSSL.TabIndex = 1;
      this.rdbNoSSL.TabStop = true;
      this.rdbNoSSL.Text = "No";
      this.rdbNoSSL.UseVisualStyleBackColor = true;
      this.rdbNoSSL.CheckedChanged += new System.EventHandler(this.rdbNoSSL_CheckedChanged);
      // 
      // lblSSL
      // 
      this.lblSSL.Location = new System.Drawing.Point(11, 47);
      this.lblSSL.Name = "lblSSL";
      this.lblSSL.Size = new System.Drawing.Size(102, 29);
      this.lblSSL.TabIndex = 13;
      this.lblSSL.Text = "Secure Authentication:";
      // 
      // btnTest
      // 
      this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTest.Enabled = false;
      this.btnTest.Location = new System.Drawing.Point(353, 160);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new System.Drawing.Size(75, 23);
      this.btnTest.TabIndex = 8;
      this.btnTest.Text = "Test";
      this.btnTest.UseVisualStyleBackColor = true;
      // 
      // tbPassword
      // 
      this.tbPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPassword.Location = new System.Drawing.Point(112, 134);
      this.tbPassword.Name = "tbPassword";
      this.tbPassword.PasswordChar = '*';
      this.tbPassword.Size = new System.Drawing.Size(316, 20);
      this.tbPassword.TabIndex = 7;
      this.tbPassword.Text = "textBox5";
      // 
      // tbUsername
      // 
      this.tbUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbUsername.Location = new System.Drawing.Point(112, 108);
      this.tbUsername.Name = "tbUsername";
      this.tbUsername.Size = new System.Drawing.Size(316, 20);
      this.tbUsername.TabIndex = 6;
      this.tbUsername.Text = "textBox4";
      // 
      // tbPort
      // 
      this.tbPort.Location = new System.Drawing.Point(112, 82);
      this.tbPort.Name = "tbPort";
      this.tbPort.Size = new System.Drawing.Size(64, 20);
      this.tbPort.TabIndex = 5;
      this.tbPort.Text = "textBox3";
      // 
      // tbServerAddress
      // 
      this.tbServerAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbServerAddress.Location = new System.Drawing.Point(112, 21);
      this.tbServerAddress.Name = "tbServerAddress";
      this.tbServerAddress.Size = new System.Drawing.Size(316, 20);
      this.tbServerAddress.TabIndex = 0;
      this.tbServerAddress.Text = "textBox2";
      // 
      // lblPort
      // 
      this.lblPort.Location = new System.Drawing.Point(12, 85);
      this.lblPort.Name = "lblPort";
      this.lblPort.Size = new System.Drawing.Size(90, 23);
      this.lblPort.TabIndex = 8;
      this.lblPort.Text = "Port:";
      // 
      // lblPassword
      // 
      this.lblPassword.Location = new System.Drawing.Point(12, 137);
      this.lblPassword.Name = "lblPassword";
      this.lblPassword.Size = new System.Drawing.Size(90, 23);
      this.lblPassword.TabIndex = 7;
      this.lblPassword.Text = "Password:";
      // 
      // lblUsername
      // 
      this.lblUsername.Location = new System.Drawing.Point(12, 111);
      this.lblUsername.Name = "lblUsername";
      this.lblUsername.Size = new System.Drawing.Size(90, 23);
      this.lblUsername.TabIndex = 6;
      this.lblUsername.Text = "Username:";
      // 
      // lblServerAddress
      // 
      this.lblServerAddress.Location = new System.Drawing.Point(12, 24);
      this.lblServerAddress.Name = "lblServerAddress";
      this.lblServerAddress.Size = new System.Drawing.Size(90, 23);
      this.lblServerAddress.TabIndex = 5;
      this.lblServerAddress.Text = "Server Address:";
      // 
      // gbStorage
      // 
      this.gbStorage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbStorage.Controls.Add(this.btnAttachmentFolder);
      this.gbStorage.Controls.Add(this.btnMailboxFolder);
      this.gbStorage.Controls.Add(this.tbAttachmentFolder);
      this.gbStorage.Controls.Add(this.tbMailboxFolder);
      this.gbStorage.Controls.Add(this.lblAttachmentFolder);
      this.gbStorage.Controls.Add(this.lblMailboxFolder);
      this.gbStorage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbStorage.Location = new System.Drawing.Point(8, 285);
      this.gbStorage.Name = "gbStorage";
      this.gbStorage.Size = new System.Drawing.Size(448, 80);
      this.gbStorage.TabIndex = 2;
      this.gbStorage.TabStop = false;
      this.gbStorage.Text = "Storage";
      // 
      // btnAttachmentFolder
      // 
      this.btnAttachmentFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAttachmentFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnAttachmentFolder.Image")));
      this.btnAttachmentFolder.Location = new System.Drawing.Point(408, 44);
      this.btnAttachmentFolder.Name = "btnAttachmentFolder";
      this.btnAttachmentFolder.Size = new System.Drawing.Size(20, 20);
      this.btnAttachmentFolder.TabIndex = 3;
      this.btnAttachmentFolder.UseVisualStyleBackColor = true;
      this.btnAttachmentFolder.Click += new System.EventHandler(this.btnAttachmentFolder_Click);
      // 
      // btnMailboxFolder
      // 
      this.btnMailboxFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnMailboxFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnMailboxFolder.Image")));
      this.btnMailboxFolder.Location = new System.Drawing.Point(408, 16);
      this.btnMailboxFolder.Name = "btnMailboxFolder";
      this.btnMailboxFolder.Size = new System.Drawing.Size(20, 20);
      this.btnMailboxFolder.TabIndex = 1;
      this.btnMailboxFolder.UseVisualStyleBackColor = true;
      this.btnMailboxFolder.Click += new System.EventHandler(this.btnMailboxFolder_Click);
      // 
      // tbAttachmentFolder
      // 
      this.tbAttachmentFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbAttachmentFolder.Location = new System.Drawing.Point(111, 42);
      this.tbAttachmentFolder.Name = "tbAttachmentFolder";
      this.tbAttachmentFolder.Size = new System.Drawing.Size(289, 20);
      this.tbAttachmentFolder.TabIndex = 2;
      this.tbAttachmentFolder.Text = "textBox7";
      // 
      // tbMailboxFolder
      // 
      this.tbMailboxFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbMailboxFolder.Location = new System.Drawing.Point(111, 17);
      this.tbMailboxFolder.Name = "tbMailboxFolder";
      this.tbMailboxFolder.Size = new System.Drawing.Size(289, 20);
      this.tbMailboxFolder.TabIndex = 0;
      this.tbMailboxFolder.Text = "tbMailboxFolder";
      // 
      // lblAttachmentFolder
      // 
      this.lblAttachmentFolder.Location = new System.Drawing.Point(11, 44);
      this.lblAttachmentFolder.Name = "lblAttachmentFolder";
      this.lblAttachmentFolder.Size = new System.Drawing.Size(104, 23);
      this.lblAttachmentFolder.TabIndex = 9;
      this.lblAttachmentFolder.Text = "Attachment-Folder:";
      // 
      // lblMailboxFolder
      // 
      this.lblMailboxFolder.Location = new System.Drawing.Point(11, 20);
      this.lblMailboxFolder.Name = "lblMailboxFolder";
      this.lblMailboxFolder.Size = new System.Drawing.Size(104, 23);
      this.lblMailboxFolder.TabIndex = 8;
      this.lblMailboxFolder.Text = "Mailbox-Folder:";
      // 
      // gbGeneral
      // 
      this.gbGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbGeneral.Controls.Add(this.tbBoxLabel);
      this.gbGeneral.Controls.Add(this.lblBoxLabel);
      this.gbGeneral.Controls.Add(this.cbEnabled);
      this.gbGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbGeneral.Location = new System.Drawing.Point(8, 8);
      this.gbGeneral.Name = "gbGeneral";
      this.gbGeneral.Size = new System.Drawing.Size(448, 72);
      this.gbGeneral.TabIndex = 0;
      this.gbGeneral.TabStop = false;
      this.gbGeneral.Text = "General";
      // 
      // tbBoxLabel
      // 
      this.tbBoxLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbBoxLabel.Location = new System.Drawing.Point(112, 42);
      this.tbBoxLabel.Name = "tbBoxLabel";
      this.tbBoxLabel.Size = new System.Drawing.Size(316, 20);
      this.tbBoxLabel.TabIndex = 0;
      this.tbBoxLabel.Text = "textBox1";
      // 
      // lblBoxLabel
      // 
      this.lblBoxLabel.Location = new System.Drawing.Point(12, 45);
      this.lblBoxLabel.Name = "lblBoxLabel";
      this.lblBoxLabel.Size = new System.Drawing.Size(64, 16);
      this.lblBoxLabel.TabIndex = 9;
      this.lblBoxLabel.Text = "Box-Label:";
      // 
      // cbEnabled
      // 
      this.cbEnabled.AutoSize = true;
      this.cbEnabled.Checked = true;
      this.cbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbEnabled.Location = new System.Drawing.Point(12, 16);
      this.cbEnabled.Name = "cbEnabled";
      this.cbEnabled.Size = new System.Drawing.Size(63, 17);
      this.cbEnabled.TabIndex = 8;
      this.cbEnabled.Text = "Enabled";
      this.cbEnabled.UseVisualStyleBackColor = true;
      // 
      // MailDetailSetup
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(464, 402);
      this.Controls.Add(this.gbGeneral);
      this.Controls.Add(this.gbStorage);
      this.Controls.Add(this.gbConnection);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "MailDetailSetup";
      this.Text = "Edit Mailbox Details";
      this.Load += new System.EventHandler(this.MailDetailSetup_Load);
      this.gbConnection.ResumeLayout(false);
      this.gbConnection.PerformLayout();
      this.panelSSL.ResumeLayout(false);
      this.panelSSL.PerformLayout();
      this.gbStorage.ResumeLayout(false);
      this.gbStorage.PerformLayout();
      this.gbGeneral.ResumeLayout(false);
      this.gbGeneral.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    public MailBox CurMailBox
    {
      get { return m_CurMailBox; }
      set { m_CurMailBox = value; }
    }


    private void btnMailboxFolder_Click(object sender, System.EventArgs e)
    {
      dialogFolder.SelectedPath = tbMailboxFolder.Text;
      if (dialogFolder.ShowDialog(null) == DialogResult.OK)
      {
        tbMailboxFolder.Text = dialogFolder.SelectedPath;
      }
    }

    private void btnAttachmentFolder_Click(object sender, System.EventArgs e)
    {
      dialogFolder.SelectedPath = tbAttachmentFolder.Text;
      if (dialogFolder.ShowDialog(null) == DialogResult.OK)
      {
        tbAttachmentFolder.Text = dialogFolder.SelectedPath;
      }
    }

    private void MailDetailSetup_Load(object sender, System.EventArgs e)
    {
      UpdateDisplay();
    }

    void UpdateDisplay()
    {
      if (m_CurMailBox != null)
      {
        this.cbEnabled.Checked = m_CurMailBox.Enabled;
        this.tbBoxLabel.Text = m_CurMailBox.BoxLabel;
        this.tbServerAddress.Text = m_CurMailBox.ServerAddress;
        this.tbPort.Text = m_CurMailBox.Port.ToString();
        if (m_CurMailBox.TLS == MailBox.SSL_PORT)
          this.rdbSSLPort.Checked = true;
        else if (m_CurMailBox.TLS == MailBox.STLS)
          this.rdbSTLSCommand.Checked = true;
        else
          this.rdbNoSSL.Checked = true;
        this.tbUsername.Text = m_CurMailBox.Username;
        this.tbPassword.Text = m_CurMailBox.Password;
        this.tbMailboxFolder.Text = m_CurMailBox.MailboxFolder;
        this.tbAttachmentFolder.Text = m_CurMailBox.AttachmentFolder;
      }
    }

    private void btnOk_Click(object sender, System.EventArgs e)
    {
      m_CurMailBox.Enabled = cbEnabled.Checked;
      m_CurMailBox.BoxLabel = tbBoxLabel.Text;
      m_CurMailBox.ServerAddress = tbServerAddress.Text;
      try
      { m_CurMailBox.Port = Int32.Parse(this.tbPort.Text); }
      catch (System.FormatException)
      { m_CurMailBox.Port = 110; }
      if (rdbSSLPort.Checked)
        m_CurMailBox.TLS = MailBox.SSL_PORT;
      else if (rdbSTLSCommand.Checked)
        m_CurMailBox.TLS = MailBox.STLS;
      else
        m_CurMailBox.TLS = MailBox.NO_SSL;

      m_CurMailBox.Username = tbUsername.Text;
      m_CurMailBox.Password = tbPassword.Text;
      m_CurMailBox.MailboxFolder = tbMailboxFolder.Text;
      m_CurMailBox.AttachmentFolder = tbAttachmentFolder.Text;
      if (EntriesOk())
      {
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
    }

    bool EntriesOk()
    {
      checker.Clear();
      checker.DoCheck(m_CurMailBox.BoxLabel != "", "BoxLabel property must not be empty!");
      if (checker.DoCheck(m_CurMailBox.ServerAddress != "", "Server Address must not be empty!"))
      {
        checker.DoCheck(ServerExists(m_CurMailBox), "Server Address could not be resolved!");
        checker.DoCheck(m_CurMailBox.Port > 0, "Port Number cannot be negative!");
      }
      if (!checker.IsOk)
      {
        string strHeader = "The following entries are invalid: \r\n\r\n";
        MessageBox.Show(strHeader + checker.Problems, "Invalid Entries");
      }
      return checker.IsOk;
    }

    bool ServerExists(MailBox mb)
    {
      try
      {
        IPHostEntry hostIP = Dns.GetHostEntry(mb.ServerAddress);
        IPAddress[] addr = hostIP.AddressList;
      }
      catch
      {
        return false;
      }
      return true;
    }

    private void rdbNoSSL_CheckedChanged(object sender, EventArgs e)
    {
      if (rdbNoSSL.Checked) tbPort.Text = "110";
    }

    private void rdbSSLPort_CheckedChanged(object sender, EventArgs e)
    {
      if (rdbSSLPort.Checked) tbPort.Text = "995";
    }

    private void rdbSTLSCommand_CheckedChanged(object sender, EventArgs e)
    {
      if (rdbSTLSCommand.Checked) tbPort.Text = "110";
    }

  }
}
