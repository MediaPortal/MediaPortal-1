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
	public class EditShareForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button folderButton;
		private System.Windows.Forms.TextBox folderTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox pinCodeTextBox;
    private System.Windows.Forms.CheckBox checkBoxRemote;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox textBoxServer;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox textBoxLogin;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox textBoxPassword;
    private System.Windows.Forms.TextBox textBoxPort;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox textBoxRemoteFolder;
    private System.Windows.Forms.Label label3;
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label8 = new System.Windows.Forms.Label();
      this.textBoxPort = new System.Windows.Forms.TextBox();
      this.textBoxPassword = new System.Windows.Forms.TextBox();
      this.label7 = new System.Windows.Forms.Label();
      this.textBoxLogin = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.textBoxServer = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.checkBoxRemote = new System.Windows.Forms.CheckBox();
      this.pinCodeTextBox = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.folderButton = new System.Windows.Forms.Button();
      this.folderTextBox = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cancelButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.textBoxRemoteFolder = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
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
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(408, 354);
      this.groupBox1.TabIndex = 6;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Share settings";
      this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(16, 256);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(100, 16);
      this.label8.TabIndex = 22;
      this.label8.Text = "Port:";
      // 
      // textBoxPort
      // 
      this.textBoxPort.Location = new System.Drawing.Point(16, 272);
      this.textBoxPort.Name = "textBoxPort";
      this.textBoxPort.TabIndex = 21;
      this.textBoxPort.Text = "21";
      // 
      // textBoxPassword
      // 
      this.textBoxPassword.Location = new System.Drawing.Point(224, 232);
      this.textBoxPassword.Name = "textBoxPassword";
      this.textBoxPassword.PasswordChar = '*';
      this.textBoxPassword.TabIndex = 20;
      this.textBoxPassword.Text = "";
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(224, 216);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(100, 16);
      this.label7.TabIndex = 19;
      this.label7.Text = "Password:";
      // 
      // textBoxLogin
      // 
      this.textBoxLogin.Location = new System.Drawing.Point(16, 232);
      this.textBoxLogin.Name = "textBoxLogin";
      this.textBoxLogin.TabIndex = 18;
      this.textBoxLogin.Text = "";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 216);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(100, 16);
      this.label6.TabIndex = 17;
      this.label6.Text = "Login:";
      // 
      // textBoxServer
      // 
      this.textBoxServer.Location = new System.Drawing.Point(16, 192);
      this.textBoxServer.Name = "textBoxServer";
      this.textBoxServer.Size = new System.Drawing.Size(344, 20);
      this.textBoxServer.TabIndex = 16;
      this.textBoxServer.Text = "127.0.0.1";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 176);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(100, 16);
      this.label5.TabIndex = 15;
      this.label5.Text = "FTP Server:";
      // 
      // checkBoxRemote
      // 
      this.checkBoxRemote.Location = new System.Drawing.Point(16, 152);
      this.checkBoxRemote.Name = "checkBoxRemote";
      this.checkBoxRemote.Size = new System.Drawing.Size(136, 16);
      this.checkBoxRemote.TabIndex = 14;
      this.checkBoxRemote.Text = "Remote FTP Share";
      this.checkBoxRemote.CheckedChanged += new System.EventHandler(this.checkBoxRemote_CheckedChanged);
      // 
      // pinCodeTextBox
      // 
      this.pinCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.pinCodeTextBox.Location = new System.Drawing.Point(216, 48);
      this.pinCodeTextBox.MaxLength = 4;
      this.pinCodeTextBox.Name = "pinCodeTextBox";
      this.pinCodeTextBox.Size = new System.Drawing.Size(63, 20);
      this.pinCodeTextBox.TabIndex = 13;
      this.pinCodeTextBox.Text = "";
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
      this.folderButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.folderButton.Location = new System.Drawing.Point(368, 96);
      this.folderButton.Name = "folderButton";
      this.folderButton.Size = new System.Drawing.Size(24, 20);
      this.folderButton.TabIndex = 10;
      this.folderButton.Text = "...";
      this.folderButton.Click += new System.EventHandler(this.folderButton_Click);
      // 
      // folderTextBox
      // 
      this.folderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.folderTextBox.Location = new System.Drawing.Point(16, 96);
      this.folderTextBox.Name = "folderTextBox";
      this.folderTextBox.Size = new System.Drawing.Size(344, 20);
      this.folderTextBox.TabIndex = 9;
      this.folderTextBox.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 80);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(168, 23);
      this.label2.TabIndex = 8;
      this.label2.Text = "local folder";
      // 
      // nameTextBox
      // 
      this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.nameTextBox.Location = new System.Drawing.Point(16, 48);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(184, 20);
      this.nameTextBox.TabIndex = 7;
      this.nameTextBox.Text = "";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(168, 23);
      this.label1.TabIndex = 6;
      this.label1.Text = "Name";
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cancelButton.Location = new System.Drawing.Point(342, 369);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.TabIndex = 7;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(262, 369);
      this.okButton.Name = "okButton";
      this.okButton.TabIndex = 8;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // textBoxRemoteFolder
      // 
      this.textBoxRemoteFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxRemoteFolder.Location = new System.Drawing.Point(16, 320);
      this.textBoxRemoteFolder.Name = "textBoxRemoteFolder";
      this.textBoxRemoteFolder.Size = new System.Drawing.Size(344, 20);
      this.textBoxRemoteFolder.TabIndex = 24;
      this.textBoxRemoteFolder.Text = "/";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 304);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(168, 23);
      this.label3.TabIndex = 23;
      this.label3.Text = "remote folder";
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
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(424, 248);
      this.Name = "EditShareForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "EditShare";
      this.Load += new System.EventHandler(this.EditShareForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		private void folderButton_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the location of the share folder";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = folderTextBox.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					folderTextBox.Text = folderBrowserDialog.SelectedPath;
				}
			}
		}

		private void okButton_Click(object sender, System.EventArgs e)
		{
			if (nameTextBox.Text==String.Empty)
			{
				MessageBox.Show("No name specified.Please fill in a name for this share", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;					

			}
			
			if (folderTextBox.Text==String.Empty)
			{
				MessageBox.Show("No local folder specified.Please choose a local folder", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;					

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
      if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }        
    }

    private void checkBoxRemote_CheckedChanged(object sender, System.EventArgs e)
    {
      if (checkBoxRemote.Checked)
      {
        textBoxLogin.Enabled=true;
        textBoxPassword.Enabled=true;
        textBoxPort.Enabled=true;
        textBoxServer.Enabled=true;
        textBoxRemoteFolder.Enabled=true;
      }
      else
      {
        textBoxLogin.Enabled=false;
        textBoxPassword.Enabled=false;
        textBoxPort.Enabled=false;
        textBoxServer.Enabled=false;
        textBoxRemoteFolder.Enabled=false;
      }
    }

    private void groupBox1_Enter(object sender, System.EventArgs e)
    {
    
    }

    private void EditShareForm_Load(object sender, System.EventArgs e)
    {
      bool remote=IsRemote;
      checkBoxRemote.Checked=!remote;
      checkBoxRemote.Checked=remote;
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
      get { return checkBoxRemote.Checked;}
      set { checkBoxRemote.Checked=value; }
    }
    public int Port
    {
      get {
        int port=21;
        try
        {
          port=Int32.Parse(textBoxPort.Text);
        }
        catch(Exception){}
        return port;
      }
      set { textBoxPort.Text=value.ToString(); }
    }
    public string Server
    {
      get { return textBoxServer.Text;}
      set { textBoxServer.Text=value; }
    }
    public string LoginName
    {
      get { return textBoxLogin.Text;}
      set { textBoxLogin.Text=value; }
    }
    public string PassWord
    {
      get { return textBoxPassword.Text;}
      set { textBoxPassword.Text=value; }
    }
    
    public string RemoteFolder
    {
      get { return textBoxRemoteFolder.Text;}
      set { textBoxRemoteFolder.Text=value; }
    }
	}
}
