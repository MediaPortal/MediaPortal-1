namespace MediaPortal.DeployTool.Sections
{
  partial class MySQLChoice
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
      this.checkMySQL = new System.Windows.Forms.CheckBox();
      this.lblMySQLText = new System.Windows.Forms.Label();
      this.grpMySQL = new System.Windows.Forms.GroupBox();
      this.linkMySQL = new System.Windows.Forms.LinkLabel();
      this.lblRecommended = new System.Windows.Forms.Label();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.grpMySQL.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // checkMySQL
      // 
      this.checkMySQL.AutoSize = true;
      this.checkMySQL.Checked = true;
      this.checkMySQL.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkMySQL.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.checkMySQL.ForeColor = System.Drawing.Color.White;
      this.checkMySQL.Location = new System.Drawing.Point(15, 49);
      this.checkMySQL.Name = "checkMySQL";
      this.checkMySQL.Size = new System.Drawing.Size(65, 17);
      this.checkMySQL.TabIndex = 12;
      this.checkMySQL.Text = "MySQL";
      this.checkMySQL.UseVisualStyleBackColor = true;
      // 
      // lblMySQLText
      // 
      this.lblMySQLText.AutoSize = true;
      this.lblMySQLText.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblMySQLText.ForeColor = System.Drawing.Color.White;
      this.lblMySQLText.Location = new System.Drawing.Point(12, 16);
      this.lblMySQLText.Name = "lblMySQLText";
      this.lblMySQLText.Size = new System.Drawing.Size(562, 13);
      this.lblMySQLText.TabIndex = 10;
      this.lblMySQLText.Text = "Check above value if you want to upgrade your current MySQL 5.1 to 5.6 (old db wi" +
    "ll not be lost)";
      // 
      // grpMySQL
      // 
      this.grpMySQL.Controls.Add(this.checkMySQL);
      this.grpMySQL.Controls.Add(this.lblMySQLText);
      this.grpMySQL.Controls.Add(this.linkMySQL);
      this.grpMySQL.Location = new System.Drawing.Point(333, 80);
      this.grpMySQL.Name = "grpMySQL";
      this.grpMySQL.Size = new System.Drawing.Size(591, 80);
      this.grpMySQL.TabIndex = 12;
      this.grpMySQL.TabStop = false;
      // 
      // linkMySQL
      // 
      this.linkMySQL.AutoSize = true;
      this.linkMySQL.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.linkMySQL.LinkColor = System.Drawing.Color.White;
      this.linkMySQL.Location = new System.Drawing.Point(512, 60);
      this.linkMySQL.Name = "linkMySQL";
      this.linkMySQL.Size = new System.Drawing.Size(62, 13);
      this.linkMySQL.TabIndex = 11;
      this.linkMySQL.TabStop = true;
      this.linkMySQL.Text = "More Info";
      this.linkMySQL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkMySQL_LinkClicked);
      // 
      // lblRecommended
      // 
      this.lblRecommended.AutoSize = true;
      this.lblRecommended.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold);
      this.lblRecommended.ForeColor = System.Drawing.Color.White;
      this.lblRecommended.Location = new System.Drawing.Point(330, 47);
      this.lblRecommended.Name = "lblRecommended";
      this.lblRecommended.Size = new System.Drawing.Size(195, 16);
      this.lblRecommended.TabIndex = 14;
      this.lblRecommended.Text = "MySQL Upgrade 5.1 to 5.6";
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.MySQL;
      this.pictureBox1.Location = new System.Drawing.Point(238, 83);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(80, 80);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 15;
      this.pictureBox1.TabStop = false;
      // 
      // MySQLChoice
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.lblRecommended);
      this.Controls.Add(this.grpMySQL);
      this.Name = "MySQLChoice";
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.grpMySQL, 0);
      this.Controls.SetChildIndex(this.lblRecommended, 0);
      this.Controls.SetChildIndex(this.pictureBox1, 0);
      this.grpMySQL.ResumeLayout(false);
      this.grpMySQL.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox grpMySQL;
    private System.Windows.Forms.Label lblMySQLText;
    private System.Windows.Forms.LinkLabel linkMySQL;
    private System.Windows.Forms.Label lblRecommended;
    private System.Windows.Forms.CheckBox checkMySQL;
    private System.Windows.Forms.PictureBox pictureBox1;
  }
}
