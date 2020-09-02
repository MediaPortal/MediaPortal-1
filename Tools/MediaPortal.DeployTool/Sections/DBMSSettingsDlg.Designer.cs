namespace MediaPortal.DeployTool.Sections
{
  partial class DBMSSettingsDlg
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.labelHeading = new System.Windows.Forms.Label();
      this.labelInstDir = new System.Windows.Forms.Label();
      this.textBoxDir = new System.Windows.Forms.TextBox();
      this.textBoxPassword = new System.Windows.Forms.TextBox();
      this.labelPassword = new System.Windows.Forms.Label();
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.checkBoxFirewall = new System.Windows.Forms.CheckBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // labelHeading
      // 
      this.labelHeading.AutoSize = true;
      this.labelHeading.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelHeading.ForeColor = System.Drawing.Color.White;
      this.labelHeading.Location = new System.Drawing.Point(330, 100);
      this.labelHeading.Name = "labelHeading";
      this.labelHeading.Size = new System.Drawing.Size(487, 17);
      this.labelHeading.TabIndex = 2;
      this.labelHeading.Text = "Please set the needed options for the SQL-Server installation:";
      // 
      // labelInstDir
      // 
      this.labelInstDir.AutoSize = true;
      this.labelInstDir.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelInstDir.ForeColor = System.Drawing.Color.White;
      this.labelInstDir.Location = new System.Drawing.Point(330, 140);
      this.labelInstDir.Name = "labelInstDir";
      this.labelInstDir.Size = new System.Drawing.Size(66, 13);
      this.labelInstDir.TabIndex = 3;
      this.labelInstDir.Text = "Install dir:";
      // 
      // textBoxDir
      // 
      this.textBoxDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxDir.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.textBoxDir.Location = new System.Drawing.Point(333, 156);
      this.textBoxDir.Name = "textBoxDir";
      this.textBoxDir.Size = new System.Drawing.Size(550, 21);
      this.textBoxDir.TabIndex = 4;
      this.textBoxDir.Text = "C:\\Programme\\MSSQL";
      this.textBoxDir.TextChanged += new System.EventHandler(this.textBoxDir_TextChanged);
      // 
      // textBoxPassword
      // 
      this.textBoxPassword.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.textBoxPassword.HideSelection = false;
      this.textBoxPassword.Location = new System.Drawing.Point(333, 226);
      this.textBoxPassword.Name = "textBoxPassword";
      this.textBoxPassword.Size = new System.Drawing.Size(132, 21);
      this.textBoxPassword.TabIndex = 8;
      this.textBoxPassword.Text = "MediaPortal";
      // 
      // labelPassword
      // 
      this.labelPassword.AutoSize = true;
      this.labelPassword.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelPassword.ForeColor = System.Drawing.Color.White;
      this.labelPassword.Location = new System.Drawing.Point(330, 210);
      this.labelPassword.Name = "labelPassword";
      this.labelPassword.Size = new System.Drawing.Size(66, 13);
      this.labelPassword.TabIndex = 7;
      this.labelPassword.Text = "Password:";
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
      this.buttonBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonBrowse.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.buttonBrowse.ForeColor = System.Drawing.SystemColors.ControlText;
      this.buttonBrowse.Location = new System.Drawing.Point(889, 156);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(73, 23);
      this.buttonBrowse.TabIndex = 10;
      this.buttonBrowse.Text = "Browse";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // checkBoxFirewall
      // 
      this.checkBoxFirewall.AutoSize = true;
      this.checkBoxFirewall.Checked = true;
      this.checkBoxFirewall.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxFirewall.Cursor = System.Windows.Forms.Cursors.Hand;
      this.checkBoxFirewall.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxFirewall.ForeColor = System.Drawing.Color.White;
      this.checkBoxFirewall.Location = new System.Drawing.Point(333, 267);
      this.checkBoxFirewall.Name = "checkBoxFirewall";
      this.checkBoxFirewall.Size = new System.Drawing.Size(436, 17);
      this.checkBoxFirewall.TabIndex = 11;
      this.checkBoxFirewall.Text = "Configure Windows Firewall to allow external access to database server";
      this.checkBoxFirewall.UseVisualStyleBackColor = true;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.Database;
      this.pictureBox1.Location = new System.Drawing.Point(28, 61);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(286, 274);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 12;
      this.pictureBox1.TabStop = false;
      // 
      // DBMSSettingsDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.checkBoxFirewall);
      this.Controls.Add(this.labelPassword);
      this.Controls.Add(this.textBoxPassword);
      this.Controls.Add(this.labelHeading);
      this.Controls.Add(this.buttonBrowse);
      this.Controls.Add(this.textBoxDir);
      this.Controls.Add(this.labelInstDir);
      this.Name = "DBMSSettingsDlg";
      this.Controls.SetChildIndex(this.labelInstDir, 0);
      this.Controls.SetChildIndex(this.textBoxDir, 0);
      this.Controls.SetChildIndex(this.buttonBrowse, 0);
      this.Controls.SetChildIndex(this.labelHeading, 0);
      this.Controls.SetChildIndex(this.textBoxPassword, 0);
      this.Controls.SetChildIndex(this.labelPassword, 0);
      this.Controls.SetChildIndex(this.checkBoxFirewall, 0);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.pictureBox1, 0);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.Label labelInstDir;
    private System.Windows.Forms.TextBox textBoxDir;
    private System.Windows.Forms.TextBox textBoxPassword;
    private System.Windows.Forms.Label labelPassword;
    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.CheckBox checkBoxFirewall;
    private System.Windows.Forms.PictureBox pictureBox1;
  }
}