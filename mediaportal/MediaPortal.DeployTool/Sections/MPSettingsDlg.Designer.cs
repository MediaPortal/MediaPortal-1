namespace MediaPortal.DeployTool
{
  partial class MPSettingsDlg
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
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.textBoxDir = new System.Windows.Forms.TextBox();
      this.labelInstDir = new System.Windows.Forms.Label();
      this.labelHeading = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Location = new System.Drawing.Point(441, 71);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(99, 23);
      this.buttonBrowse.TabIndex = 14;
      this.buttonBrowse.Text = "browse";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // textBoxDir
      // 
      this.textBoxDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxDir.Location = new System.Drawing.Point(9, 73);
      this.textBoxDir.Name = "textBoxDir";
      this.textBoxDir.Size = new System.Drawing.Size(417, 20);
      this.textBoxDir.TabIndex = 13;
      // 
      // labelInstDir
      // 
      this.labelInstDir.AutoSize = true;
      this.labelInstDir.Location = new System.Drawing.Point(6, 57);
      this.labelInstDir.Name = "labelInstDir";
      this.labelInstDir.Size = new System.Drawing.Size(109, 13);
      this.labelInstDir.TabIndex = 12;
      this.labelInstDir.Text = "MediaPortal install dir:";
      // 
      // labelHeading
      // 
      this.labelHeading.AutoSize = true;
      this.labelHeading.Location = new System.Drawing.Point(4, 30);
      this.labelHeading.Name = "labelHeading";
      this.labelHeading.Size = new System.Drawing.Size(297, 13);
      this.labelHeading.TabIndex = 11;
      this.labelHeading.Text = "Please set the needed options for the MediaPortal installation:";
      // 
      // MPSettingsDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.buttonBrowse);
      this.Controls.Add(this.textBoxDir);
      this.Controls.Add(this.labelInstDir);
      this.Controls.Add(this.labelHeading);
      this.Name = "MPSettingsDlg";
      this.Size = new System.Drawing.Size(620, 172);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.labelHeading, 0);
      this.Controls.SetChildIndex(this.labelInstDir, 0);
      this.Controls.SetChildIndex(this.textBoxDir, 0);
      this.Controls.SetChildIndex(this.buttonBrowse, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.TextBox textBoxDir;
    private System.Windows.Forms.Label labelInstDir;
    private System.Windows.Forms.Label labelHeading;
  }
}
