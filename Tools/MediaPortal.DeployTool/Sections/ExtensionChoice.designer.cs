namespace MediaPortal.DeployTool.Sections
{
  partial class ExtensionChoice
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
      this.grpLAV = new System.Windows.Forms.GroupBox();
      this.linkLAV = new System.Windows.Forms.LinkLabel();
      this.lblLAV = new System.Windows.Forms.Label();
      this.chkLAV = new System.Windows.Forms.CheckBox();
      this.linkExtensions = new System.Windows.Forms.LinkLabel();
      this.lblRecommended = new System.Windows.Forms.Label();
      this.grpLAV.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpLAV
      // 
      this.grpLAV.Controls.Add(this.linkLAV);
      this.grpLAV.Controls.Add(this.lblLAV);
      this.grpLAV.Controls.Add(this.chkLAV);
      this.grpLAV.Location = new System.Drawing.Point(53, 59);
      this.grpLAV.Name = "grpLAV";
      this.grpLAV.Size = new System.Drawing.Size(513, 70);
      this.grpLAV.TabIndex = 12;
      this.grpLAV.TabStop = false;
      // 
      // linkLAV
      // 
      this.linkLAV.AutoSize = true;
      this.linkLAV.LinkColor = System.Drawing.Color.White;
      this.linkLAV.Location = new System.Drawing.Point(437, 50);
      this.linkLAV.Name = "linkLAV";
      this.linkLAV.Size = new System.Drawing.Size(52, 13);
      this.linkLAV.TabIndex = 11;
      this.linkLAV.TabStop = true;
      this.linkLAV.Text = "More Info";
      this.linkLAV.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLAV_LinkClicked);
      // 
      // lblLAV
      // 
      this.lblLAV.AutoSize = true;
      this.lblLAV.ForeColor = System.Drawing.Color.White;
      this.lblLAV.Location = new System.Drawing.Point(36, 39);
      this.lblLAV.Name = "lblLAV";
      this.lblLAV.Size = new System.Drawing.Size(379, 13);
      this.lblLAV.TabIndex = 10;
      this.lblLAV.Text = "Install LAV Filters to enable playback of many common audio and video formats";
      // 
      // chkLAV
      // 
      this.chkLAV.AutoSize = true;
      this.chkLAV.Checked = true;
      this.chkLAV.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkLAV.ForeColor = System.Drawing.Color.White;
      this.chkLAV.Location = new System.Drawing.Point(15, 19);
      this.chkLAV.Name = "chkLAV";
      this.chkLAV.Size = new System.Drawing.Size(76, 17);
      this.chkLAV.TabIndex = 9;
      this.chkLAV.Text = "LAV Filters";
      this.chkLAV.UseVisualStyleBackColor = true;
      // 
      // linkExtensions
      // 
      this.linkExtensions.AutoSize = true;
      this.linkExtensions.LinkColor = System.Drawing.Color.White;
      this.linkExtensions.Location = new System.Drawing.Point(89, 222);
      this.linkExtensions.Name = "linkExtensions";
      this.linkExtensions.Size = new System.Drawing.Size(122, 13);
      this.linkExtensions.TabIndex = 13;
      this.linkExtensions.TabStop = true;
      this.linkExtensions.Text = "Browse other extensions";
      this.linkExtensions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkExtensions_LinkClicked_1);
      // 
      // lblRecommended
      // 
      this.lblRecommended.AutoSize = true;
      this.lblRecommended.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold);
      this.lblRecommended.ForeColor = System.Drawing.Color.White;
      this.lblRecommended.Location = new System.Drawing.Point(50, 31);
      this.lblRecommended.Name = "lblRecommended";
      this.lblRecommended.Size = new System.Drawing.Size(200, 16);
      this.lblRecommended.TabIndex = 14;
      this.lblRecommended.Text = "Recommended Extensions";
      // 
      // ExtensionChoice
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.lblRecommended);
      this.Controls.Add(this.linkExtensions);
      this.Controls.Add(this.grpLAV);
      this.Name = "ExtensionChoice";
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.grpLAV, 0);
      this.Controls.SetChildIndex(this.linkExtensions, 0);
      this.Controls.SetChildIndex(this.lblRecommended, 0);
      this.grpLAV.ResumeLayout(false);
      this.grpLAV.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox grpLAV;
    private System.Windows.Forms.CheckBox chkLAV;
    private System.Windows.Forms.Label lblLAV;
    private System.Windows.Forms.LinkLabel linkLAV;
    private System.Windows.Forms.LinkLabel linkExtensions;
    private System.Windows.Forms.Label lblRecommended;

  }
}
