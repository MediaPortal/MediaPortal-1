namespace MediaPortal.DeployTool.Sections
{
  partial class DBMSTypeDlg
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
      this.rbMSSQL = new System.Windows.Forms.Label();
      this.rbMySQL = new System.Windows.Forms.Label();
      this.rbDBAlreadyInstalled = new System.Windows.Forms.Label();
      this.bMS = new System.Windows.Forms.Button();
      this.bMySQL = new System.Windows.Forms.Button();
      this.bExists = new System.Windows.Forms.Button();
      this.lbMSSQL = new System.Windows.Forms.LinkLabel();
      this.pbMS = new System.Windows.Forms.PictureBox();
      this.pbMySQL = new System.Windows.Forms.PictureBox();
      this.label1 = new System.Windows.Forms.Label();
      this.pbMariaDBa = new System.Windows.Forms.PictureBox();
      this.pbMySQLa = new System.Windows.Forms.PictureBox();
      this.pbMSa = new System.Windows.Forms.PictureBox();
      this.pbMariaDB = new System.Windows.Forms.PictureBox();
      this.bMariaDB = new System.Windows.Forms.Button();
      this.rbMariaDB = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.pbMS)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbMySQL)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbMariaDBa)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbMySQLa)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbMSa)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbMariaDB)).BeginInit();
      this.SuspendLayout();
      // 
      // labelHeading
      // 
      this.labelHeading.AutoSize = true;
      this.labelHeading.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelHeading.ForeColor = System.Drawing.Color.White;
      this.labelHeading.Location = new System.Drawing.Point(328, 83);
      this.labelHeading.Name = "labelHeading";
      this.labelHeading.Size = new System.Drawing.Size(367, 17);
      this.labelHeading.TabIndex = 1;
      this.labelHeading.Text = "Please select the SQL-Server you want to use:";
      // 
      // rbMSSQL
      // 
      this.rbMSSQL.AutoSize = true;
      this.rbMSSQL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbMSSQL.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbMSSQL.ForeColor = System.Drawing.Color.White;
      this.rbMSSQL.Location = new System.Drawing.Point(381, 135);
      this.rbMSSQL.Name = "rbMSSQL";
      this.rbMSSQL.Size = new System.Drawing.Size(275, 13);
      this.rbMSSQL.TabIndex = 23;
      this.rbMSSQL.Text = "Microsoft SQL-Server Express is disabled";
      this.rbMSSQL.Click += new System.EventHandler(this.bMS_Click);
      // 
      // rbMySQL
      // 
      this.rbMySQL.AutoSize = true;
      this.rbMySQL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbMySQL.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbMySQL.ForeColor = System.Drawing.Color.White;
      this.rbMySQL.Location = new System.Drawing.Point(381, 190);
      this.rbMySQL.Name = "rbMySQL";
      this.rbMySQL.Size = new System.Drawing.Size(121, 13);
      this.rbMySQL.TabIndex = 24;
      this.rbMySQL.Text = "MySQL Server 5.x";
      this.rbMySQL.Click += new System.EventHandler(this.bMySQL_Click);
      // 
      // rbDBAlreadyInstalled
      // 
      this.rbDBAlreadyInstalled.AutoSize = true;
      this.rbDBAlreadyInstalled.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbDBAlreadyInstalled.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbDBAlreadyInstalled.ForeColor = System.Drawing.Color.White;
      this.rbDBAlreadyInstalled.Location = new System.Drawing.Point(381, 298);
      this.rbDBAlreadyInstalled.Name = "rbDBAlreadyInstalled";
      this.rbDBAlreadyInstalled.Size = new System.Drawing.Size(403, 13);
      this.rbDBAlreadyInstalled.TabIndex = 25;
      this.rbDBAlreadyInstalled.Text = "SQL server is already present and will be used for TV-Server";
      this.rbDBAlreadyInstalled.Click += new System.EventHandler(this.bExists_Click);
      // 
      // bMS
      // 
      this.bMS.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bMS.FlatAppearance.BorderSize = 0;
      this.bMS.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bMS.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bMS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bMS.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bMS.Location = new System.Drawing.Point(331, 130);
      this.bMS.Name = "bMS";
      this.bMS.Size = new System.Drawing.Size(32, 23);
      this.bMS.TabIndex = 26;
      this.bMS.UseVisualStyleBackColor = true;
      this.bMS.Click += new System.EventHandler(this.bMS_Click);
      // 
      // bMySQL
      // 
      this.bMySQL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bMySQL.FlatAppearance.BorderSize = 0;
      this.bMySQL.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bMySQL.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bMySQL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bMySQL.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bMySQL.Location = new System.Drawing.Point(331, 185);
      this.bMySQL.Name = "bMySQL";
      this.bMySQL.Size = new System.Drawing.Size(32, 23);
      this.bMySQL.TabIndex = 27;
      this.bMySQL.UseVisualStyleBackColor = true;
      this.bMySQL.Click += new System.EventHandler(this.bMySQL_Click);
      // 
      // bExists
      // 
      this.bExists.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bExists.FlatAppearance.BorderSize = 0;
      this.bExists.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bExists.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bExists.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bExists.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bExists.Location = new System.Drawing.Point(331, 293);
      this.bExists.Name = "bExists";
      this.bExists.Size = new System.Drawing.Size(32, 23);
      this.bExists.TabIndex = 28;
      this.bExists.UseVisualStyleBackColor = true;
      this.bExists.Click += new System.EventHandler(this.bExists_Click);
      // 
      // lbMSSQL
      // 
      this.lbMSSQL.AutoSize = true;
      this.lbMSSQL.Enabled = false;
      this.lbMSSQL.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F);
      this.lbMSSQL.ForeColor = System.Drawing.Color.White;
      this.lbMSSQL.LinkColor = System.Drawing.Color.White;
      this.lbMSSQL.Location = new System.Drawing.Point(381, 154);
      this.lbMSSQL.Name = "lbMSSQL";
      this.lbMSSQL.Size = new System.Drawing.Size(88, 13);
      this.lbMSSQL.TabIndex = 29;
      this.lbMSSQL.TabStop = true;
      this.lbMSSQL.Text = "( learn why here )";
      this.lbMSSQL.Visible = false;
      this.lbMSSQL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbMSSQL_LinkClicked);
      // 
      // pbMS
      // 
      this.pbMS.Image = global::MediaPortal.DeployTool.Images.MSSQL;
      this.pbMS.Location = new System.Drawing.Point(256, 118);
      this.pbMS.Name = "pbMS";
      this.pbMS.Size = new System.Drawing.Size(50, 50);
      this.pbMS.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbMS.TabIndex = 30;
      this.pbMS.TabStop = false;
      // 
      // pbMySQL
      // 
      this.pbMySQL.Image = global::MediaPortal.DeployTool.Images.MySQL;
      this.pbMySQL.Location = new System.Drawing.Point(256, 174);
      this.pbMySQL.Name = "pbMySQL";
      this.pbMySQL.Size = new System.Drawing.Size(50, 50);
      this.pbMySQL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbMySQL.TabIndex = 31;
      this.pbMySQL.TabStop = false;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.label1.ForeColor = System.Drawing.Color.White;
      this.label1.Location = new System.Drawing.Point(381, 326);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(244, 13);
      this.label1.TabIndex = 32;
      this.label1.Text = "Support MS SQL, MySQL, MariaDB server";
      // 
      // pbMariaDBa
      // 
      this.pbMariaDBa.Image = global::MediaPortal.DeployTool.Images.MariaDB;
      this.pbMariaDBa.Location = new System.Drawing.Point(256, 289);
      this.pbMariaDBa.Name = "pbMariaDBa";
      this.pbMariaDBa.Size = new System.Drawing.Size(50, 50);
      this.pbMariaDBa.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbMariaDBa.TabIndex = 33;
      this.pbMariaDBa.TabStop = false;
      // 
      // pbMySQLa
      // 
      this.pbMySQLa.Image = global::MediaPortal.DeployTool.Images.MySQL;
      this.pbMySQLa.Location = new System.Drawing.Point(200, 289);
      this.pbMySQLa.Name = "pbMySQLa";
      this.pbMySQLa.Size = new System.Drawing.Size(50, 50);
      this.pbMySQLa.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbMySQLa.TabIndex = 34;
      this.pbMySQLa.TabStop = false;
      // 
      // pbMSa
      // 
      this.pbMSa.Image = global::MediaPortal.DeployTool.Images.MSSQL;
      this.pbMSa.Location = new System.Drawing.Point(144, 289);
      this.pbMSa.Name = "pbMSa";
      this.pbMSa.Size = new System.Drawing.Size(50, 50);
      this.pbMSa.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbMSa.TabIndex = 35;
      this.pbMSa.TabStop = false;
      // 
      // pbMariaDB
      // 
      this.pbMariaDB.Image = global::MediaPortal.DeployTool.Images.MariaDB;
      this.pbMariaDB.Location = new System.Drawing.Point(256, 230);
      this.pbMariaDB.Name = "pbMariaDB";
      this.pbMariaDB.Size = new System.Drawing.Size(50, 50);
      this.pbMariaDB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbMariaDB.TabIndex = 38;
      this.pbMariaDB.TabStop = false;
      // 
      // bMariaDB
      // 
      this.bMariaDB.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bMariaDB.FlatAppearance.BorderSize = 0;
      this.bMariaDB.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bMariaDB.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bMariaDB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bMariaDB.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bMariaDB.Location = new System.Drawing.Point(331, 240);
      this.bMariaDB.Name = "bMariaDB";
      this.bMariaDB.Size = new System.Drawing.Size(32, 23);
      this.bMariaDB.TabIndex = 37;
      this.bMariaDB.UseVisualStyleBackColor = true;
      this.bMariaDB.Click += new System.EventHandler(this.bMariaDB_Click);
      // 
      // rbMariaDB
      // 
      this.rbMariaDB.AutoSize = true;
      this.rbMariaDB.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbMariaDB.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbMariaDB.ForeColor = System.Drawing.Color.White;
      this.rbMariaDB.Location = new System.Drawing.Point(381, 245);
      this.rbMariaDB.Name = "rbMariaDB";
      this.rbMariaDB.Size = new System.Drawing.Size(140, 13);
      this.rbMariaDB.TabIndex = 36;
      this.rbMariaDB.Text = "MariaDB Server 10.x";
      // 
      // DBMSTypeDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.pbMariaDB);
      this.Controls.Add(this.bMariaDB);
      this.Controls.Add(this.rbMariaDB);
      this.Controls.Add(this.pbMSa);
      this.Controls.Add(this.pbMySQLa);
      this.Controls.Add(this.pbMariaDBa);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.pbMySQL);
      this.Controls.Add(this.pbMS);
      this.Controls.Add(this.lbMSSQL);
      this.Controls.Add(this.bExists);
      this.Controls.Add(this.bMySQL);
      this.Controls.Add(this.bMS);
      this.Controls.Add(this.rbDBAlreadyInstalled);
      this.Controls.Add(this.rbMySQL);
      this.Controls.Add(this.rbMSSQL);
      this.Controls.Add(this.labelHeading);
      this.Name = "DBMSTypeDlg";
      this.Controls.SetChildIndex(this.labelHeading, 0);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.rbMSSQL, 0);
      this.Controls.SetChildIndex(this.rbMySQL, 0);
      this.Controls.SetChildIndex(this.rbDBAlreadyInstalled, 0);
      this.Controls.SetChildIndex(this.bMS, 0);
      this.Controls.SetChildIndex(this.bMySQL, 0);
      this.Controls.SetChildIndex(this.bExists, 0);
      this.Controls.SetChildIndex(this.lbMSSQL, 0);
      this.Controls.SetChildIndex(this.pbMS, 0);
      this.Controls.SetChildIndex(this.pbMySQL, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.pbMariaDBa, 0);
      this.Controls.SetChildIndex(this.pbMySQLa, 0);
      this.Controls.SetChildIndex(this.pbMSa, 0);
      this.Controls.SetChildIndex(this.rbMariaDB, 0);
      this.Controls.SetChildIndex(this.bMariaDB, 0);
      this.Controls.SetChildIndex(this.pbMariaDB, 0);
      ((System.ComponentModel.ISupportInitialize)(this.pbMS)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbMySQL)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbMariaDBa)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbMySQLa)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbMSa)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbMariaDB)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.Label rbMSSQL;
    private System.Windows.Forms.Label rbMySQL;
    private System.Windows.Forms.Label rbDBAlreadyInstalled;
    private System.Windows.Forms.Button bMS;
    private System.Windows.Forms.Button bMySQL;
    private System.Windows.Forms.Button bExists;
    private System.Windows.Forms.LinkLabel lbMSSQL;
    private System.Windows.Forms.PictureBox pbMS;
    private System.Windows.Forms.PictureBox pbMySQL;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.PictureBox pbMariaDBa;
    private System.Windows.Forms.PictureBox pbMySQLa;
    private System.Windows.Forms.PictureBox pbMSa;
    private System.Windows.Forms.PictureBox pbMariaDB;
    private System.Windows.Forms.Button bMariaDB;
    private System.Windows.Forms.Label rbMariaDB;
  }
}