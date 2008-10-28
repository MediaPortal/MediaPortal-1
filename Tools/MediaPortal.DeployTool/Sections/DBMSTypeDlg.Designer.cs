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
      this.rbMSSQL.Click += new System.EventHandler(this.bMS_Click);
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
      this.rbMySQL.Click += new System.EventHandler(this.bMySQL_Click);
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
      this.bMS.Location = new System.Drawing.Point(9, 39);
      this.bMS.Name = "bMS";
      this.bMS.Size = new System.Drawing.Size(37, 23);
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
      this.bMySQL.Location = new System.Drawing.Point(9, 109);
      this.bMySQL.Name = "bMySQL";
      this.bMySQL.Size = new System.Drawing.Size(37, 23);
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
      this.bExists.Location = new System.Drawing.Point(9, 179);
      this.bExists.Name = "bExists";
      this.bExists.Size = new System.Drawing.Size(37, 23);
      this.bExists.TabIndex = 28;
      this.bExists.UseVisualStyleBackColor = true;
      this.bExists.Click += new System.EventHandler(this.bExists_Click);
      // 
      // DBMSTypeDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.bExists);
      this.Controls.Add(this.bMySQL);
      this.Controls.Add(this.bMS);
      this.Controls.Add(this.rbDBAlreadyInstalled);
      this.Controls.Add(this.rbMySQL);
      this.Controls.Add(this.rbMSSQL);
      this.Controls.Add(this.labelHeading);
      this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Name = "DBMSTypeDlg";
      this.Size = new System.Drawing.Size(775, 252);
      this.Controls.SetChildIndex(this.labelHeading, 0);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.rbMSSQL, 0);
      this.Controls.SetChildIndex(this.rbMySQL, 0);
      this.Controls.SetChildIndex(this.rbDBAlreadyInstalled, 0);
      this.Controls.SetChildIndex(this.bMS, 0);
      this.Controls.SetChildIndex(this.bMySQL, 0);
      this.Controls.SetChildIndex(this.bExists, 0);
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
  }
}