namespace MPTvClient
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupDatabaseForm));
            this.gbConnectionSetup = new System.Windows.Forms.GroupBox();
            this.gbDbLogon = new System.Windows.Forms.GroupBox();
            this.lblUserId = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.tbUserID = new System.Windows.Forms.TextBox();
            this.gbServerLocation = new System.Windows.Forms.GroupBox();
            this.tbDatabaseName = new System.Windows.Forms.TextBox();
            this.lblDbName = new System.Windows.Forms.Label();
            this.lbServerHostname = new System.Windows.Forms.Label();
            this.tbServerHostName = new System.Windows.Forms.TextBox();
            this.lblDBChoice = new System.Windows.Forms.LinkLabel();
            this.pbMySQL = new System.Windows.Forms.PictureBox();
            this.pbSQLServer = new System.Windows.Forms.PictureBox();
            this.rbMySQL = new System.Windows.Forms.RadioButton();
            this.rbSQLServer = new System.Windows.Forms.RadioButton();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.mpLabel1 = new System.Windows.Forms.Label();
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
            this.gbConnectionSetup.Location = new System.Drawing.Point(18, 15);
            this.gbConnectionSetup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbConnectionSetup.Name = "gbConnectionSetup";
            this.gbConnectionSetup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbConnectionSetup.Size = new System.Drawing.Size(561, 522);
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
            this.gbDbLogon.Location = new System.Drawing.Point(28, 377);
            this.gbDbLogon.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbDbLogon.Name = "gbDbLogon";
            this.gbDbLogon.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbDbLogon.Size = new System.Drawing.Size(502, 126);
            this.gbDbLogon.TabIndex = 2;
            this.gbDbLogon.TabStop = false;
            this.gbDbLogon.Text = "Database logon: ";
            // 
            // lblUserId
            // 
            this.lblUserId.AutoSize = true;
            this.lblUserId.Location = new System.Drawing.Point(9, 38);
            this.lblUserId.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(72, 20);
            this.lblUserId.TabIndex = 5;
            this.lblUserId.Text = "User ID: ";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(148, 74);
            this.tbPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(328, 26);
            this.tbPassword.TabIndex = 7;
            this.tbPassword.UseSystemPasswordChar = true;
            this.tbPassword.TextChanged += new System.EventHandler(this.tbPassword_TextChanged);
            this.tbPassword.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbPassword_KeyUp);
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(9, 78);
            this.lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(82, 20);
            this.lblPassword.TabIndex = 7;
            this.lblPassword.Text = "Password:";
            // 
            // tbUserID
            // 
            this.tbUserID.Location = new System.Drawing.Point(148, 34);
            this.tbUserID.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbUserID.Name = "tbUserID";
            this.tbUserID.Size = new System.Drawing.Size(328, 26);
            this.tbUserID.TabIndex = 6;
            this.tbUserID.TextChanged += new System.EventHandler(this.tbUserID_TextChanged);
            // 
            // gbServerLocation
            // 
            this.gbServerLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbServerLocation.Controls.Add(this.tbDatabaseName);
            this.gbServerLocation.Controls.Add(this.lblDbName);
            this.gbServerLocation.Controls.Add(this.lbServerHostname);
            this.gbServerLocation.Controls.Add(this.tbServerHostName);
            this.gbServerLocation.Enabled = false;
            this.gbServerLocation.Location = new System.Drawing.Point(28, 237);
            this.gbServerLocation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbServerLocation.Name = "gbServerLocation";
            this.gbServerLocation.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbServerLocation.Size = new System.Drawing.Size(502, 128);
            this.gbServerLocation.TabIndex = 1;
            this.gbServerLocation.TabStop = false;
            this.gbServerLocation.Text = "Database location: ";
            // 
            // tbDatabaseName
            // 
            this.tbDatabaseName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDatabaseName.Location = new System.Drawing.Point(148, 75);
            this.tbDatabaseName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbDatabaseName.Name = "tbDatabaseName";
            this.tbDatabaseName.Size = new System.Drawing.Size(328, 26);
            this.tbDatabaseName.TabIndex = 5;
            this.tbDatabaseName.TextChanged += new System.EventHandler(this.tbDatabaseName_TextChanged);
            // 
            // lblDbName
            // 
            this.lblDbName.AutoSize = true;
            this.lblDbName.Location = new System.Drawing.Point(9, 80);
            this.lblDbName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDbName.Name = "lblDbName";
            this.lblDbName.Size = new System.Drawing.Size(116, 20);
            this.lblDbName.TabIndex = 15;
            this.lblDbName.Text = "Schema name:";
            // 
            // lbServerHostname
            // 
            this.lbServerHostname.AutoSize = true;
            this.lbServerHostname.Location = new System.Drawing.Point(9, 38);
            this.lbServerHostname.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbServerHostname.Name = "lbServerHostname";
            this.lbServerHostname.Size = new System.Drawing.Size(87, 20);
            this.lbServerHostname.TabIndex = 12;
            this.lbServerHostname.Text = "Hostname:";
            // 
            // tbServerHostName
            // 
            this.tbServerHostName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbServerHostName.Location = new System.Drawing.Point(148, 34);
            this.tbServerHostName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbServerHostName.Name = "tbServerHostName";
            this.tbServerHostName.Size = new System.Drawing.Size(328, 26);
            this.tbServerHostName.TabIndex = 3;
            this.tbServerHostName.TextChanged += new System.EventHandler(this.tbServerHostName_TextChanged);
            // 
            // lblDBChoice
            // 
            this.lblDBChoice.AutoSize = true;
            this.lblDBChoice.LinkArea = new System.Windows.Forms.LinkArea(26, 15);
            this.lblDBChoice.Location = new System.Drawing.Point(24, 34);
            this.lblDBChoice.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDBChoice.Name = "lblDBChoice";
            this.lblDBChoice.Size = new System.Drawing.Size(475, 24);
            this.lblDBChoice.TabIndex = 0;
            this.lblDBChoice.TabStop = true;
            this.lblDBChoice.Text = "Please select the type of database system you are going to use: ";
            this.lblDBChoice.UseCompatibleTextRendering = true;
            this.lblDBChoice.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblDBChoice_LinkClicked);
            // 
            // pbMySQL
            // 
            this.pbMySQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbMySQL.Image = global::MPTvClient.Properties.Resources.logo_MySQL;
            this.pbMySQL.Location = new System.Drawing.Point(306, 63);
            this.pbMySQL.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbMySQL.Name = "pbMySQL";
            this.pbMySQL.Size = new System.Drawing.Size(224, 114);
            this.pbMySQL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbMySQL.TabIndex = 12;
            this.pbMySQL.TabStop = false;
            this.pbMySQL.Click += new System.EventHandler(this.pbMySQL_Click);
            // 
            // pbSQLServer
            // 
            this.pbSQLServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbSQLServer.Image = global::MPTvClient.Properties.Resources.logo_MSSQL;
            this.pbSQLServer.Location = new System.Drawing.Point(28, 63);
            this.pbSQLServer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbSQLServer.Name = "pbSQLServer";
            this.pbSQLServer.Size = new System.Drawing.Size(224, 114);
            this.pbSQLServer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbSQLServer.TabIndex = 11;
            this.pbSQLServer.TabStop = false;
            this.pbSQLServer.Click += new System.EventHandler(this.pbSQLServer_Click);
            // 
            // rbMySQL
            // 
            this.rbMySQL.AutoSize = true;
            this.rbMySQL.Location = new System.Drawing.Point(306, 188);
            this.rbMySQL.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbMySQL.Name = "rbMySQL";
            this.rbMySQL.Size = new System.Drawing.Size(103, 24);
            this.rbMySQL.TabIndex = 0;
            this.rbMySQL.TabStop = true;
            this.rbMySQL.Text = "My SQL 5";
            this.rbMySQL.UseVisualStyleBackColor = true;
            this.rbMySQL.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // rbSQLServer
            // 
            this.rbSQLServer.AutoSize = true;
            this.rbSQLServer.Location = new System.Drawing.Point(28, 188);
            this.rbSQLServer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbSQLServer.Name = "rbSQLServer";
            this.rbSQLServer.Size = new System.Drawing.Size(225, 24);
            this.rbSQLServer.TabIndex = 0;
            this.rbSQLServer.TabStop = true;
            this.rbSQLServer.Text = "Microsoft SQL Server 2005";
            this.rbSQLServer.UseVisualStyleBackColor = true;
            this.rbSQLServer.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // btnTest
            // 
            this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTest.Location = new System.Drawing.Point(344, 549);
            this.btnTest.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(112, 35);
            this.btnTest.TabIndex = 3;
            this.btnTest.Text = "&Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.mpButtonTest_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(465, 549);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(112, 35);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.mpButtonSave_Click);
            // 
            // mpLabel1
            // 
            this.mpLabel1.AutoSize = true;
            this.mpLabel1.Location = new System.Drawing.Point(34, 52);
            this.mpLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mpLabel1.Name = "mpLabel1";
            this.mpLabel1.Size = new System.Drawing.Size(59, 20);
            this.mpLabel1.TabIndex = 0;
            this.mpLabel1.Text = "Server:";
            // 
            // SetupDatabaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 602);
            this.Controls.Add(this.gbConnectionSetup);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.mpLabel1);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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

    private System.Windows.Forms.Label mpLabel1;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Button btnTest;
    private System.Windows.Forms.GroupBox gbConnectionSetup;
    private System.Windows.Forms.RadioButton rbSQLServer;
    private System.Windows.Forms.RadioButton rbMySQL;
    private System.Windows.Forms.PictureBox pbSQLServer;
    private System.Windows.Forms.PictureBox pbMySQL;
    private System.Windows.Forms.LinkLabel lblDBChoice;
    private System.Windows.Forms.GroupBox gbServerLocation;
    private System.Windows.Forms.Label lbServerHostname;
    private System.Windows.Forms.TextBox tbServerHostName;
    private System.Windows.Forms.GroupBox gbDbLogon;
    private System.Windows.Forms.Label lblUserId;
    private System.Windows.Forms.TextBox tbPassword;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox tbUserID;
    private System.Windows.Forms.TextBox tbDatabaseName;
    private System.Windows.Forms.Label lblDbName;
  }
}