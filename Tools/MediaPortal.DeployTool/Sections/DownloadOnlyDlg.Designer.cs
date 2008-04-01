namespace MediaPortal.DeployTool
{
  partial class DownloadOnlyDlg
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
        this.rbYesDlOnly = new System.Windows.Forms.RadioButton();
        this.rbNoDlOnly = new System.Windows.Forms.RadioButton();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelSectionHeader.Location = new System.Drawing.Point(53, 28);
        this.labelSectionHeader.Size = new System.Drawing.Size(330, 16);
        this.labelSectionHeader.Text = "Do you want to only to download components ?";
        // 
        // rbYesDlOnly
        // 
        this.rbYesDlOnly.AutoSize = true;
        this.rbYesDlOnly.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbYesDlOnly.Location = new System.Drawing.Point(94, 96);
        this.rbYesDlOnly.Name = "rbYesDlOnly";
        this.rbYesDlOnly.Size = new System.Drawing.Size(312, 17);
        this.rbYesDlOnly.TabIndex = 9;
        this.rbYesDlOnly.TabStop = true;
        this.rbYesDlOnly.Text = "Yes, I will use them to install MediaPortal on another machine";
        this.rbYesDlOnly.UseVisualStyleBackColor = true;
        // 
        // rbNoDlOnly
        // 
        this.rbNoDlOnly.AutoSize = true;
        this.rbNoDlOnly.Checked = true;
        this.rbNoDlOnly.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbNoDlOnly.Location = new System.Drawing.Point(94, 141);
        this.rbNoDlOnly.Name = "rbNoDlOnly";
        this.rbNoDlOnly.Size = new System.Drawing.Size(277, 17);
        this.rbNoDlOnly.TabIndex = 10;
        this.rbNoDlOnly.TabStop = true;
        this.rbNoDlOnly.Text = "No, download and install MediaPortal on this machine";
        this.rbNoDlOnly.UseVisualStyleBackColor = true;
        // 
        // DownloadOnlyDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.rbNoDlOnly);
        this.Controls.Add(this.rbYesDlOnly);
        this.Name = "DownloadOnlyDlg";
        this.Size = new System.Drawing.Size(542, 266);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.rbYesDlOnly, 0);
        this.Controls.SetChildIndex(this.rbNoDlOnly, 0);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton rbYesDlOnly;
    private System.Windows.Forms.RadioButton rbNoDlOnly;

  }
}
