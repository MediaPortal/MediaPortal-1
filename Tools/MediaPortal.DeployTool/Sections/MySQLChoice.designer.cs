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
      this.grpMySQL.SuspendLayout();
      this.SuspendLayout();
      // 
      // checkMySQL
      // 
      this.checkMySQL.AutoSize = true;
      this.checkMySQL.Checked = true;
      this.checkMySQL.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkMySQL.ForeColor = System.Drawing.Color.White;
      this.checkMySQL.Location = new System.Drawing.Point(15, 49);
      this.checkMySQL.Name = "checkMySQL";
      this.checkMySQL.Size = new System.Drawing.Size(61, 17);
      this.checkMySQL.TabIndex = 12;
      this.checkMySQL.Text = "MySQL";
      this.checkMySQL.UseVisualStyleBackColor = true;
      // 
      // lblMySQLText
      // 
      this.lblMySQLText.AutoSize = true;
      this.lblMySQLText.ForeColor = System.Drawing.Color.White;
      this.lblMySQLText.Location = new System.Drawing.Point(12, 16);
      this.lblMySQLText.Name = "lblMySQLText";
      this.lblMySQLText.Size = new System.Drawing.Size(460, 13);
      this.lblMySQLText.TabIndex = 10;
      this.lblMySQLText.Text = "Check above value if you want to upgrade your current MySQL 5.1 to 5.6 (old db wi" +
    "ll not be lost)";
      // 
      // grpMySQL
      // 
      this.grpMySQL.Controls.Add(this.checkMySQL);
      this.grpMySQL.Controls.Add(this.lblMySQLText);
      this.grpMySQL.Controls.Add(this.linkMySQL);
      this.grpMySQL.Location = new System.Drawing.Point(53, 59);
      this.grpMySQL.Name = "grpMySQL";
      this.grpMySQL.Size = new System.Drawing.Size(513, 104);
      this.grpMySQL.TabIndex = 12;
      this.grpMySQL.TabStop = false;
      // 
      // linkMySQL
      // 
      this.linkMySQL.AutoSize = true;
      this.linkMySQL.LinkColor = System.Drawing.Color.White;
      this.linkMySQL.Location = new System.Drawing.Point(455, 78);
      this.linkMySQL.Name = "linkMySQL";
      this.linkMySQL.Size = new System.Drawing.Size(52, 13);
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
      this.lblRecommended.Location = new System.Drawing.Point(50, 29);
      this.lblRecommended.Name = "lblRecommended";
      this.lblRecommended.Size = new System.Drawing.Size(195, 16);
      this.lblRecommended.TabIndex = 14;
      this.lblRecommended.Text = "MySQL Upgrade 5.1 to 5.6";
      // 
      // MySQLChoice
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.lblRecommended);
      this.Controls.Add(this.grpMySQL);
      this.Name = "MySQLChoice";
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.grpMySQL, 0);
      this.Controls.SetChildIndex(this.lblRecommended, 0);
      this.grpMySQL.ResumeLayout(false);
      this.grpMySQL.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox grpMySQL;
    private System.Windows.Forms.Label lblMySQLText;
    private System.Windows.Forms.LinkLabel linkMySQL;
    private System.Windows.Forms.Label lblRecommended;
    private System.Windows.Forms.CheckBox checkMySQL;

  }
}
