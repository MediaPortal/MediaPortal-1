namespace SetupTv
{
  partial class SetupDatabaseForm
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
      this.gbConnectionSetup = new System.Windows.Forms.GroupBox();
      this.gbDbLogon = new System.Windows.Forms.GroupBox();
      this.lblUserId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbPassword = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblPassword = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbUserID = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.gbServerLocation = new System.Windows.Forms.GroupBox();
      this.tbDatabaseName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblDbName = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbServiceDependency = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblServiceName = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lbServerHostname = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbServerHostName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblDBChoice = new System.Windows.Forms.LinkLabel();
      this.pbMySQL = new System.Windows.Forms.PictureBox();
      this.pbSQLServer = new System.Windows.Forms.PictureBox();
      this.rbMySQL = new System.Windows.Forms.RadioButton();
      this.rbSQLServer = new System.Windows.Forms.RadioButton();
      this.btnTest = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnDrop = new MediaPortal.UserInterface.Controls.MPButton();
      this.gbConnectionSetup.SuspendLayout();
      this.gbDbLogon.SuspendLayout();
      this.gbServerLocation.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbMySQL)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbSQLServer)).BeginInit();
      this.SuspendLayout();
      // 
      // gbConnectionSetup
      // 
      this.gbConnectionSetup.Controls.Add(this.gbDbLogon);
      this.gbConnectionSetup.Controls.Add(this.gbServerLocation);
      this.gbConnectionSetup.Controls.Add(this.lblDBChoice);
      this.gbConnectionSetup.Controls.Add(this.pbMySQL);
      this.gbConnectionSetup.Controls.Add(this.pbSQLServer);
      this.gbConnectionSetup.Controls.Add(this.rbMySQL);
      this.gbConnectionSetup.Controls.Add(this.rbSQLServer);
      this.gbConnectionSetup.Location = new System.Drawing.Point(12, 12);
      this.gbConnectionSetup.Name = "gbConnectionSetup";
      this.gbConnectionSetup.Size = new System.Drawing.Size(374, 375);
      this.gbConnectionSetup.TabIndex = 0;
      this.gbConnectionSetup.TabStop = false;
      this.gbConnectionSetup.Text = "Connection settings";
      // 
      // gbDbLogon
      // 
      this.gbDbLogon.Controls.Add(this.lblUserId);
      this.gbDbLogon.Controls.Add(this.tbPassword);
      this.gbDbLogon.Controls.Add(this.lblPassword);
      this.gbDbLogon.Controls.Add(this.tbUserID);
      this.gbDbLogon.Enabled = false;
      this.gbDbLogon.Location = new System.Drawing.Point(19, 277);
      this.gbDbLogon.Name = "gbDbLogon";
      this.gbDbLogon.Size = new System.Drawing.Size(335, 82);
      this.gbDbLogon.TabIndex = 2;
      this.gbDbLogon.TabStop = false;
      this.gbDbLogon.Text = "Database logon: ";
      // 
      // lblUserId
      // 
      this.lblUserId.AutoSize = true;
      this.lblUserId.Location = new System.Drawing.Point(6, 25);
      this.lblUserId.Name = "lblUserId";
      this.lblUserId.Size = new System.Drawing.Size(49, 13);
      this.lblUserId.TabIndex = 5;
      this.lblUserId.Text = "User ID: ";
      // 
      // tbPassword
      // 
      this.tbPassword.Location = new System.Drawing.Point(99, 48);
      this.tbPassword.Name = "tbPassword";
      this.tbPassword.PasswordChar = '*';
      this.tbPassword.Size = new System.Drawing.Size(220, 20);
      this.tbPassword.TabIndex = 7;
      this.tbPassword.UseSystemPasswordChar = true;
      this.tbPassword.TextChanged += new System.EventHandler(this.tbPassword_TextChanged);
      this.tbPassword.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbPassword_KeyUp);
      // 
      // lblPassword
      // 
      this.lblPassword.AutoSize = true;
      this.lblPassword.Location = new System.Drawing.Point(6, 51);
      this.lblPassword.Name = "lblPassword";
      this.lblPassword.Size = new System.Drawing.Size(56, 13);
      this.lblPassword.TabIndex = 7;
      this.lblPassword.Text = "Password:";
      // 
      // tbUserID
      // 
      this.tbUserID.Location = new System.Drawing.Point(99, 22);
      this.tbUserID.Name = "tbUserID";
      this.tbUserID.Size = new System.Drawing.Size(220, 20);
      this.tbUserID.TabIndex = 6;
      this.tbUserID.TextChanged += new System.EventHandler(this.tbUserID_TextChanged);
      // 
      // gbServerLocation
      // 
      this.gbServerLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbServerLocation.Controls.Add(this.tbDatabaseName);
      this.gbServerLocation.Controls.Add(this.lblDbName);
      this.gbServerLocation.Controls.Add(this.tbServiceDependency);
      this.gbServerLocation.Controls.Add(this.lblServiceName);
      this.gbServerLocation.Controls.Add(this.lbServerHostname);
      this.gbServerLocation.Controls.Add(this.tbServerHostName);
      this.gbServerLocation.Enabled = false;
      this.gbServerLocation.Location = new System.Drawing.Point(19, 154);
      this.gbServerLocation.Name = "gbServerLocation";
      this.gbServerLocation.Size = new System.Drawing.Size(335, 109);
      this.gbServerLocation.TabIndex = 1;
      this.gbServerLocation.TabStop = false;
      this.gbServerLocation.Text = "Database location: ";
      // 
      // tbDatabaseName
      // 
      this.tbDatabaseName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbDatabaseName.Location = new System.Drawing.Point(99, 74);
      this.tbDatabaseName.Name = "tbDatabaseName";
      this.tbDatabaseName.Size = new System.Drawing.Size(220, 20);
      this.tbDatabaseName.TabIndex = 5;
      this.tbDatabaseName.TextChanged += new System.EventHandler(this.tbDatabaseName_TextChanged);
      // 
      // lblDbName
      // 
      this.lblDbName.AutoSize = true;
      this.lblDbName.Location = new System.Drawing.Point(6, 77);
      this.lblDbName.Name = "lblDbName";
      this.lblDbName.Size = new System.Drawing.Size(78, 13);
      this.lblDbName.TabIndex = 15;
      this.lblDbName.Text = "Schema name:";
      // 
      // tbServiceDependency
      // 
      this.tbServiceDependency.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbServiceDependency.Location = new System.Drawing.Point(99, 48);
      this.tbServiceDependency.Name = "tbServiceDependency";
      this.tbServiceDependency.Size = new System.Drawing.Size(220, 20);
      this.tbServiceDependency.TabIndex = 4;
      this.tbServiceDependency.TextChanged += new System.EventHandler(this.tbServiceDependency_TextChanged);
      // 
      // lblServiceName
      // 
      this.lblServiceName.AutoSize = true;
      this.lblServiceName.Location = new System.Drawing.Point(6, 51);
      this.lblServiceName.Name = "lblServiceName";
      this.lblServiceName.Size = new System.Drawing.Size(71, 13);
      this.lblServiceName.TabIndex = 13;
      this.lblServiceName.Text = "Dependency:";
      // 
      // lbServerHostname
      // 
      this.lbServerHostname.AutoSize = true;
      this.lbServerHostname.Location = new System.Drawing.Point(6, 25);
      this.lbServerHostname.Name = "lbServerHostname";
      this.lbServerHostname.Size = new System.Drawing.Size(58, 13);
      this.lbServerHostname.TabIndex = 12;
      this.lbServerHostname.Text = "Hostname:";
      // 
      // tbServerHostName
      // 
      this.tbServerHostName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbServerHostName.Location = new System.Drawing.Point(99, 22);
      this.tbServerHostName.Name = "tbServerHostName";
      this.tbServerHostName.Size = new System.Drawing.Size(220, 20);
      this.tbServerHostName.TabIndex = 3;
      this.tbServerHostName.TextChanged += new System.EventHandler(this.tbServerHostName_TextChanged);
      // 
      // lblDBChoice
      // 
      this.lblDBChoice.AutoSize = true;
      this.lblDBChoice.LinkArea = new System.Windows.Forms.LinkArea(26, 15);
      this.lblDBChoice.Location = new System.Drawing.Point(16, 22);
      this.lblDBChoice.Name = "lblDBChoice";
      this.lblDBChoice.Size = new System.Drawing.Size(327, 17);
      this.lblDBChoice.TabIndex = 0;
      this.lblDBChoice.TabStop = true;
      this.lblDBChoice.Text = "Please select the type of database system you are going to use: ";
      this.lblDBChoice.UseCompatibleTextRendering = true;
      this.lblDBChoice.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblDBChoice_LinkClicked);
      // 
      // pbMySQL
      // 
      this.pbMySQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pbMySQL.Image = global::SetupTv.Properties.Resources.logo_MySQL;
      this.pbMySQL.Location = new System.Drawing.Point(204, 41);
      this.pbMySQL.Name = "pbMySQL";
      this.pbMySQL.Size = new System.Drawing.Size(150, 75);
      this.pbMySQL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pbMySQL.TabIndex = 12;
      this.pbMySQL.TabStop = false;
      this.pbMySQL.Click += new System.EventHandler(this.pbMySQL_Click);
      // 
      // pbSQLServer
      // 
      this.pbSQLServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pbSQLServer.Image = global::SetupTv.Properties.Resources.logo_MSSQL;
      this.pbSQLServer.Location = new System.Drawing.Point(19, 41);
      this.pbSQLServer.Name = "pbSQLServer";
      this.pbSQLServer.Size = new System.Drawing.Size(150, 75);
      this.pbSQLServer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pbSQLServer.TabIndex = 11;
      this.pbSQLServer.TabStop = false;
      this.pbSQLServer.Click += new System.EventHandler(this.pbSQLServer_Click);
      // 
      // rbMySQL
      // 
      this.rbMySQL.AutoSize = true;
      this.rbMySQL.Location = new System.Drawing.Point(204, 122);
      this.rbMySQL.Name = "rbMySQL";
      this.rbMySQL.Size = new System.Drawing.Size(72, 17);
      this.rbMySQL.TabIndex = 0;
      this.rbMySQL.TabStop = true;
      this.rbMySQL.Text = "My SQL 5";
      this.rbMySQL.UseVisualStyleBackColor = true;
      this.rbMySQL.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
      // 
      // rbSQLServer
      // 
      this.rbSQLServer.AutoSize = true;
      this.rbSQLServer.Location = new System.Drawing.Point(19, 122);
      this.rbSQLServer.Name = "rbSQLServer";
      this.rbSQLServer.Size = new System.Drawing.Size(153, 17);
      this.rbSQLServer.TabIndex = 0;
      this.rbSQLServer.TabStop = true;
      this.rbSQLServer.Text = "Microsoft SQL Server 2005";
      this.rbSQLServer.UseVisualStyleBackColor = true;
      this.rbSQLServer.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // btnTest
      // 
      this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTest.Location = new System.Drawing.Point(229, 401);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new System.Drawing.Size(75, 23);
      this.btnTest.TabIndex = 3;
      this.btnTest.Text = "&Test";
      this.btnTest.UseVisualStyleBackColor = true;
      this.btnTest.Click += new System.EventHandler(this.mpButtonTest_Click);
      // 
      // btnSave
      // 
      this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSave.Enabled = false;
      this.btnSave.Location = new System.Drawing.Point(310, 401);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(75, 23);
      this.btnSave.TabIndex = 3;
      this.btnSave.Text = "&Save";
      this.btnSave.UseVisualStyleBackColor = true;
      this.btnSave.Click += new System.EventHandler(this.mpButtonSave_Click);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(23, 34);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(41, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Server:";
      // 
      // btnDrop
      // 
      this.btnDrop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDrop.Enabled = false;
      this.btnDrop.Location = new System.Drawing.Point(310, 401);
      this.btnDrop.Name = "btnDrop";
      this.btnDrop.Size = new System.Drawing.Size(75, 23);
      this.btnDrop.TabIndex = 3;
      this.btnDrop.Text = "&Delete";
      this.btnDrop.UseVisualStyleBackColor = true;
      this.btnDrop.Visible = false;
      this.btnDrop.Click += new System.EventHandler(this.btnDrop_Click);
      // 
      // SetupDatabaseForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(397, 436);
      this.Controls.Add(this.gbConnectionSetup);
      this.Controls.Add(this.btnTest);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.btnSave);
      this.Controls.Add(this.btnDrop);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SetupDatabaseForm";
      this.Text = "Setup database connection";
      this.Load += new System.EventHandler(this.SetupDatabaseForm_Load);
      this.gbConnectionSetup.ResumeLayout(false);
      this.gbConnectionSetup.PerformLayout();
      this.gbDbLogon.ResumeLayout(false);
      this.gbDbLogon.PerformLayout();
      this.gbServerLocation.ResumeLayout(false);
      this.gbServerLocation.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbMySQL)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbSQLServer)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPButton btnSave;
    private MediaPortal.UserInterface.Controls.MPButton btnTest;
    private System.Windows.Forms.GroupBox gbConnectionSetup;
    private System.Windows.Forms.RadioButton rbSQLServer;
    private System.Windows.Forms.RadioButton rbMySQL;
    private System.Windows.Forms.PictureBox pbSQLServer;
    private System.Windows.Forms.PictureBox pbMySQL;
    private System.Windows.Forms.LinkLabel lblDBChoice;
    private System.Windows.Forms.GroupBox gbServerLocation;
    private MediaPortal.UserInterface.Controls.MPTextBox tbServiceDependency;
    private MediaPortal.UserInterface.Controls.MPLabel lblServiceName;
    private MediaPortal.UserInterface.Controls.MPLabel lbServerHostname;
    private MediaPortal.UserInterface.Controls.MPTextBox tbServerHostName;
    private System.Windows.Forms.GroupBox gbDbLogon;
    private MediaPortal.UserInterface.Controls.MPLabel lblUserId;
    private MediaPortal.UserInterface.Controls.MPTextBox tbPassword;
    private MediaPortal.UserInterface.Controls.MPLabel lblPassword;
    private MediaPortal.UserInterface.Controls.MPTextBox tbUserID;
    private MediaPortal.UserInterface.Controls.MPTextBox tbDatabaseName;
    private MediaPortal.UserInterface.Controls.MPLabel lblDbName;
    private MediaPortal.UserInterface.Controls.MPButton btnDrop;
  }
}