namespace MediaPortal.DeployTool
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
        this.rbMSSQL = new System.Windows.Forms.RadioButton();
        this.rbMySQL = new System.Windows.Forms.RadioButton();
        this.rbDBAlreadyInstalled = new System.Windows.Forms.RadioButton();
        this.SuspendLayout();
        // 
        // labelHeading
        // 
        this.labelHeading.AutoSize = true;
        this.labelHeading.Location = new System.Drawing.Point(4, 34);
        this.labelHeading.Name = "labelHeading";
        this.labelHeading.Size = new System.Drawing.Size(227, 13);
        this.labelHeading.TabIndex = 1;
        this.labelHeading.Text = "Please select the SQL-Server you want to use:";
        // 
        // rbMSSQL
        // 
        this.rbMSSQL.AutoSize = true;
        this.rbMSSQL.Checked = true;
        this.rbMSSQL.Location = new System.Drawing.Point(7, 58);
        this.rbMSSQL.Name = "rbMSSQL";
        this.rbMSSQL.Size = new System.Drawing.Size(166, 17);
        this.rbMSSQL.TabIndex = 2;
        this.rbMSSQL.TabStop = true;
        this.rbMSSQL.Text = "Microsoft SQL-Server Express";
        this.rbMSSQL.UseVisualStyleBackColor = true;
        // 
        // rbMySQL
        // 
        this.rbMySQL.AutoSize = true;
        this.rbMySQL.Location = new System.Drawing.Point(7, 81);
        this.rbMySQL.Name = "rbMySQL";
        this.rbMySQL.Size = new System.Drawing.Size(69, 17);
        this.rbMySQL.TabIndex = 3;
        this.rbMySQL.Text = "MySQL 5";
        this.rbMySQL.UseVisualStyleBackColor = true;
        // 
        // rbDBAlreadyInstalled
        // 
        this.rbDBAlreadyInstalled.AutoSize = true;
        this.rbDBAlreadyInstalled.Location = new System.Drawing.Point(7, 104);
        this.rbDBAlreadyInstalled.Name = "rbDBAlreadyInstalled";
        this.rbDBAlreadyInstalled.Size = new System.Drawing.Size(308, 17);
        this.rbDBAlreadyInstalled.TabIndex = 4;
        this.rbDBAlreadyInstalled.TabStop = true;
        this.rbDBAlreadyInstalled.Text = "SQL server is already present and will be used for TV-Server";
        this.rbDBAlreadyInstalled.UseVisualStyleBackColor = true;
        // 
        // DBMSTypeDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.rbDBAlreadyInstalled);
        this.Controls.Add(this.rbMySQL);
        this.Controls.Add(this.rbMSSQL);
        this.Controls.Add(this.labelHeading);
        this.Name = "DBMSTypeDlg";
        this.Controls.SetChildIndex(this.labelHeading, 0);
        this.Controls.SetChildIndex(this.rbMSSQL, 0);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.rbMySQL, 0);
        this.Controls.SetChildIndex(this.rbDBAlreadyInstalled, 0);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.RadioButton rbMSSQL;
    private System.Windows.Forms.RadioButton rbMySQL;
      private System.Windows.Forms.RadioButton rbDBAlreadyInstalled;
  }
}
