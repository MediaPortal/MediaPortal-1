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
      this.imgMS = new System.Windows.Forms.PictureBox();
      this.imgMySQL = new System.Windows.Forms.PictureBox();
      this.imgExists = new System.Windows.Forms.PictureBox();
      this.rbMSSQL = new System.Windows.Forms.Label();
      this.rbMySQL = new System.Windows.Forms.Label();
      this.rbDBAlreadyInstalled = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.imgMS)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgMySQL)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgExists)).BeginInit();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Location = new System.Drawing.Point(5, 4);
      // 
      // labelHeading
      // 
      this.labelHeading.AutoSize = true;
      this.labelHeading.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelHeading.ForeColor = System.Drawing.Color.White;
      this.labelHeading.Location = new System.Drawing.Point(5, 4);
      this.labelHeading.Name = "labelHeading";
      this.labelHeading.Size = new System.Drawing.Size(307, 13);
      this.labelHeading.TabIndex = 1;
      this.labelHeading.Text = "Please select the SQL-Server you want to use:";
      // 
      // imgMS
      // 
      this.imgMS.Cursor = System.Windows.Forms.Cursors.Hand;
      this.imgMS.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.imgMS.Location = new System.Drawing.Point(25, 40);
      this.imgMS.Name = "imgMS";
      this.imgMS.Size = new System.Drawing.Size(21, 21);
      this.imgMS.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.imgMS.TabIndex = 20;
      this.imgMS.TabStop = false;
      this.imgMS.Click += new System.EventHandler(this.imgMS_Click);
      // 
      // imgMySQL
      // 
      this.imgMySQL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.imgMySQL.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.imgMySQL.Location = new System.Drawing.Point(25, 110);
      this.imgMySQL.Name = "imgMySQL";
      this.imgMySQL.Size = new System.Drawing.Size(21, 21);
      this.imgMySQL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.imgMySQL.TabIndex = 21;
      this.imgMySQL.TabStop = false;
      this.imgMySQL.Click += new System.EventHandler(this.imgMySQL_Click);
      // 
      // imgExists
      // 
      this.imgExists.Cursor = System.Windows.Forms.Cursors.Hand;
      this.imgExists.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.imgExists.Location = new System.Drawing.Point(25, 180);
      this.imgExists.Name = "imgExists";
      this.imgExists.Size = new System.Drawing.Size(21, 21);
      this.imgExists.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.imgExists.TabIndex = 22;
      this.imgExists.TabStop = false;
      this.imgExists.Click += new System.EventHandler(this.imgExists_Click);
      // 
      // rbMSSQL
      // 
      this.rbMSSQL.AutoSize = true;
      this.rbMSSQL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbMSSQL.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbMSSQL.ForeColor = System.Drawing.Color.White;
      this.rbMSSQL.Location = new System.Drawing.Point(50, 44);
      this.rbMSSQL.Name = "rbMSSQL";
      this.rbMSSQL.Size = new System.Drawing.Size(201, 13);
      this.rbMSSQL.TabIndex = 23;
      this.rbMSSQL.Text = "Microsoft SQL-Server Express";
      this.rbMSSQL.Click += new System.EventHandler(this.imgMS_Click);
      // 
      // rbMySQL
      // 
      this.rbMySQL.AutoSize = true;
      this.rbMySQL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbMySQL.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbMySQL.ForeColor = System.Drawing.Color.White;
      this.rbMySQL.Location = new System.Drawing.Point(52, 114);
      this.rbMySQL.Name = "rbMySQL";
      this.rbMySQL.Size = new System.Drawing.Size(61, 13);
      this.rbMySQL.TabIndex = 24;
      this.rbMySQL.Text = "MySQL 5";
      this.rbMySQL.Click += new System.EventHandler(this.imgMySQL_Click);
      // 
      // rbDBAlreadyInstalled
      // 
      this.rbDBAlreadyInstalled.AutoSize = true;
      this.rbDBAlreadyInstalled.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbDBAlreadyInstalled.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbDBAlreadyInstalled.ForeColor = System.Drawing.Color.White;
      this.rbDBAlreadyInstalled.Location = new System.Drawing.Point(52, 184);
      this.rbDBAlreadyInstalled.Name = "rbDBAlreadyInstalled";
      this.rbDBAlreadyInstalled.Size = new System.Drawing.Size(403, 13);
      this.rbDBAlreadyInstalled.TabIndex = 25;
      this.rbDBAlreadyInstalled.Text = "SQL server is already present and will be used for TV-Server";
      this.rbDBAlreadyInstalled.Click += new System.EventHandler(this.imgExists_Click);
      // 
      // DBMSTypeDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.rbDBAlreadyInstalled);
      this.Controls.Add(this.rbMySQL);
      this.Controls.Add(this.rbMSSQL);
      this.Controls.Add(this.imgExists);
      this.Controls.Add(this.imgMySQL);
      this.Controls.Add(this.imgMS);
      this.Controls.Add(this.labelHeading);
      this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Name = "DBMSTypeDlg";
      this.Size = new System.Drawing.Size(775, 252);
      this.Controls.SetChildIndex(this.labelHeading, 0);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.imgMS, 0);
      this.Controls.SetChildIndex(this.imgMySQL, 0);
      this.Controls.SetChildIndex(this.imgExists, 0);
      this.Controls.SetChildIndex(this.rbMSSQL, 0);
      this.Controls.SetChildIndex(this.rbMySQL, 0);
      this.Controls.SetChildIndex(this.rbDBAlreadyInstalled, 0);
      ((System.ComponentModel.ISupportInitialize)(this.imgMS)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgMySQL)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgExists)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.PictureBox imgMS;
    private System.Windows.Forms.PictureBox imgMySQL;
    private System.Windows.Forms.PictureBox imgExists;
    private System.Windows.Forms.Label rbMSSQL;
    private System.Windows.Forms.Label rbMySQL;
    private System.Windows.Forms.Label rbDBAlreadyInstalled;
  }
}