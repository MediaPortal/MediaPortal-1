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
      this.label1 = new System.Windows.Forms.Label();
      this.rbMSSQL = new System.Windows.Forms.RadioButton();
      this.rbMySQL = new System.Windows.Forms.RadioButton();
      this.SuspendLayout();
      // 
      // HeaderLabel
      // 
      this.HeaderLabel.Size = new System.Drawing.Size(202, 13);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(4, 34);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(227, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Please select the SQL-Server you want to use:";
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
      this.rbMySQL.Size = new System.Drawing.Size(60, 17);
      this.rbMySQL.TabIndex = 3;
      this.rbMySQL.Text = "MySQL";
      this.rbMySQL.UseVisualStyleBackColor = true;
      // 
      // DBMSTypeDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.rbMySQL);
      this.Controls.Add(this.rbMSSQL);
      this.Controls.Add(this.label1);
      this.Name = "DBMSTypeDlg";
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.rbMSSQL, 0);
      this.Controls.SetChildIndex(this.HeaderLabel, 0);
      this.Controls.SetChildIndex(this.rbMySQL, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.RadioButton rbMSSQL;
    private System.Windows.Forms.RadioButton rbMySQL;
  }
}
