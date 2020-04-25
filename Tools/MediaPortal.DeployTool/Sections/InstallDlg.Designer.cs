namespace MediaPortal.DeployTool.Sections
{
  partial class InstallDlg
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
      this.flpApplication = new System.Windows.Forms.FlowLayoutPanel();
      this.progressInstall = new System.Windows.Forms.ProgressBar();
      this.SuspendLayout();
      // 
      // flpApplication
      // 
      this.flpApplication.Location = new System.Drawing.Point(10, 13);
      this.flpApplication.Name = "flpApplication";
      this.flpApplication.Size = new System.Drawing.Size(980, 405);
      this.flpApplication.TabIndex = 1;
      // 
      // progressInstall
      // 
      this.progressInstall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(140)))), ((int)(((byte)(184)))));
      this.progressInstall.Location = new System.Drawing.Point(1, 422);
      this.progressInstall.Name = "progressInstall";
      this.progressInstall.Size = new System.Drawing.Size(998, 5);
      this.progressInstall.TabIndex = 0;
      this.progressInstall.Visible = false;
      // 
      // InstallDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.progressInstall);
      this.Controls.Add(this.flpApplication);
      this.Name = "InstallDlg";
      this.Size = new System.Drawing.Size(1002, 430);
      this.ParentChanged += new System.EventHandler(this.RequirementsDlg_ParentChanged);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.flpApplication, 0);
      this.Controls.SetChildIndex(this.progressInstall, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.FlowLayoutPanel flpApplication;
    private System.Windows.Forms.ProgressBar progressInstall;
  }
}