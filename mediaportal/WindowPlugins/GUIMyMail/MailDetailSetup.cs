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
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.GroupBox gbConnection;
    private System.Windows.Forms.Label lblServerAddress;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.Label lblPort;
    private System.Windows.Forms.GroupBox gbStorage;
    private System.Windows.Forms.Label lblMailboxFolder;
    private System.Windows.Forms.Label lblAttachmentFolder;
    private System.Windows.Forms.TextBox tbServerAddress;
    private System.Windows.Forms.TextBox tbPort;
    private System.Windows.Forms.TextBox tbUsername;
    private System.Windows.Forms.TextBox tbPassword;
    private System.Windows.Forms.TextBox tbMailboxFolder;
    private System.Windows.Forms.TextBox tbAttachmentFolder;
    private System.Windows.Forms.GroupBox gbGeneral;
    private System.Windows.Forms.TextBox tbBoxLabel;
    private System.Windows.Forms.Label lblBoxLabel;
    private System.Windows.Forms.CheckBox cbEnabled;
    private System.Windows.Forms.FolderBrowserDialog dialogFolder;
    private System.Windows.Forms.Button btnMailboxFolder;
    private System.Windows.Forms.Button btnAttachmentFolder;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

    MailBox m_CurMailBox = null;
    private System.Windows.Forms.Button btnTest;
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
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MailDetailSetup));
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.gbConnection = new System.Windows.Forms.GroupBox();
      this.tbPassword = new System.Windows.Forms.TextBox();
      this.tbUsername = new System.Windows.Forms.TextBox();
      this.tbPort = new System.Windows.Forms.TextBox();
      this.tbServerAddress = new System.Windows.Forms.TextBox();
      this.lblPort = new System.Windows.Forms.Label();
      this.lblPassword = new System.Windows.Forms.Label();
      this.lblUsername = new System.Windows.Forms.Label();
      this.lblServerAddress = new System.Windows.Forms.Label();
      this.gbStorage = new System.Windows.Forms.GroupBox();
      this.btnAttachmentFolder = new System.Windows.Forms.Button();
      this.btnMailboxFolder = new System.Windows.Forms.Button();
      this.tbAttachmentFolder = new System.Windows.Forms.TextBox();
      this.tbMailboxFolder = new System.Windows.Forms.TextBox();
      this.lblAttachmentFolder = new System.Windows.Forms.Label();
      this.lblMailboxFolder = new System.Windows.Forms.Label();
      this.gbGeneral = new System.Windows.Forms.GroupBox();
      this.tbBoxLabel = new System.Windows.Forms.TextBox();
      this.lblBoxLabel = new System.Windows.Forms.Label();
      this.cbEnabled = new System.Windows.Forms.CheckBox();
      this.dialogFolder = new System.Windows.Forms.FolderBrowserDialog();
      this.btnTest = new System.Windows.Forms.Button();
      this.gbConnection.SuspendLayout();
      this.gbStorage.SuspendLayout();
      this.gbGeneral.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnOk.Location = new System.Drawing.Point(296, 344);
      this.btnOk.Name = "btnOk";
      this.btnOk.TabIndex = 3;
      this.btnOk.Text = "OK";
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnCancel.Location = new System.Drawing.Point(376, 344);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.TabIndex = 4;
      this.btnCancel.Text = "Cancel";
      // 
      // gbConnection
      // 
      this.gbConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.gbConnection.Controls.Add(this.btnTest);
      this.gbConnection.Controls.Add(this.tbPassword);
      this.gbConnection.Controls.Add(this.tbUsername);
      this.gbConnection.Controls.Add(this.tbPort);
      this.gbConnection.Controls.Add(this.tbServerAddress);
      this.gbConnection.Controls.Add(this.lblPort);
      this.gbConnection.Controls.Add(this.lblPassword);
      this.gbConnection.Controls.Add(this.lblUsername);
      this.gbConnection.Controls.Add(this.lblServerAddress);
      this.gbConnection.Location = new System.Drawing.Point(8, 88);
      this.gbConnection.Name = "gbConnection";
      this.gbConnection.Size = new System.Drawing.Size(448, 160);
      this.gbConnection.TabIndex = 1;
      this.gbConnection.TabStop = false;
      this.gbConnection.Text = "Connection";
      // 
      // tbPassword
      // 
      this.tbPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPassword.Location = new System.Drawing.Point(112, 94);
      this.tbPassword.Name = "tbPassword";
      this.tbPassword.PasswordChar = '*';
      this.tbPassword.Size = new System.Drawing.Size(316, 20);
      this.tbPassword.TabIndex = 3;
      this.tbPassword.Text = "textBox5";
      // 
      // tbUsername
      // 
      this.tbUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbUsername.Location = new System.Drawing.Point(112, 70);
      this.tbUsername.Name = "tbUsername";
      this.tbUsername.Size = new System.Drawing.Size(316, 20);
      this.tbUsername.TabIndex = 2;
      this.tbUsername.Text = "textBox4";
      // 
      // tbPort
      // 
      this.tbPort.Location = new System.Drawing.Point(112, 45);
      this.tbPort.Name = "tbPort";
      this.tbPort.Size = new System.Drawing.Size(64, 20);
      this.tbPort.TabIndex = 1;
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
      this.lblPort.Location = new System.Drawing.Point(12, 48);
      this.lblPort.Name = "lblPort";
      this.lblPort.Size = new System.Drawing.Size(90, 23);
      this.lblPort.TabIndex = 8;
      this.lblPort.Text = "Port:";
      // 
      // lblPassword
      // 
      this.lblPassword.Location = new System.Drawing.Point(12, 96);
      this.lblPassword.Name = "lblPassword";
      this.lblPassword.Size = new System.Drawing.Size(90, 23);
      this.lblPassword.TabIndex = 7;
      this.lblPassword.Text = "Password:";
      // 
      // lblUsername
      // 
      this.lblUsername.Location = new System.Drawing.Point(12, 72);
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
      this.gbStorage.Location = new System.Drawing.Point(8, 256);
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
      this.cbEnabled.Checked = true;
      this.cbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbEnabled.Location = new System.Drawing.Point(12, 16);
      this.cbEnabled.Name = "cbEnabled";
      this.cbEnabled.Size = new System.Drawing.Size(136, 24);
      this.cbEnabled.TabIndex = 8;
      this.cbEnabled.Text = "Enabled";
      // 
      // btnTest
      // 
      this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTest.Enabled = false;
      this.btnTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnTest.Location = new System.Drawing.Point(353, 128);
      this.btnTest.Name = "btnTest";
      this.btnTest.TabIndex = 9;
      this.btnTest.Text = "Test";
      this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // MailDetailSetup
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(464, 374);
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
      this.gbStorage.ResumeLayout(false);
      this.gbGeneral.ResumeLayout(false);
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
        IPHostEntry hostIP = Dns.Resolve(mb.ServerAddress); 
        IPAddress[] addr = hostIP.AddressList;
      }
      catch
      {
        return false;
      }
      return true;
    }

    private void btnTest_Click(object sender, System.EventArgs e)
    {
    
    }



	}
}
