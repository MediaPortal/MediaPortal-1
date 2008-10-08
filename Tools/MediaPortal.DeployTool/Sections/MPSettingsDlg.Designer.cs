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
        this.checkBoxFirewall = new System.Windows.Forms.CheckBox();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Location = new System.Drawing.Point(5, 4);
        // 
        // buttonBrowse
        // 
        this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.buttonBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
        this.buttonBrowse.ForeColor = System.Drawing.Color.White;
        this.buttonBrowse.Location = new System.Drawing.Point(514, 66);
        this.buttonBrowse.Name = "buttonBrowse";
        this.buttonBrowse.Size = new System.Drawing.Size(115, 23);
        this.buttonBrowse.TabIndex = 14;
        this.buttonBrowse.Text = "browse";
        this.buttonBrowse.UseVisualStyleBackColor = true;
        this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
        // 
        // textBoxDir
        // 
        this.textBoxDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.textBoxDir.Location = new System.Drawing.Point(10, 68);
        this.textBoxDir.MaximumSize = new System.Drawing.Size(490, 0);
        this.textBoxDir.Name = "textBoxDir";
        this.textBoxDir.Size = new System.Drawing.Size(486, 21);
        this.textBoxDir.TabIndex = 13;
        // 
        // labelInstDir
        // 
        this.labelInstDir.AutoSize = true;
        this.labelInstDir.ForeColor = System.Drawing.Color.White;
        this.labelInstDir.Location = new System.Drawing.Point(7, 52);
        this.labelInstDir.Name = "labelInstDir";
        this.labelInstDir.Size = new System.Drawing.Size(134, 13);
        this.labelInstDir.TabIndex = 12;
        this.labelInstDir.Text = "MediaPortal install dir:";
        // 
        // labelHeading
        // 
        this.labelHeading.AutoSize = true;
        this.labelHeading.ForeColor = System.Drawing.Color.White;
        this.labelHeading.Location = new System.Drawing.Point(8, 25);
        this.labelHeading.Name = "labelHeading";
        this.labelHeading.Size = new System.Drawing.Size(360, 13);
        this.labelHeading.TabIndex = 11;
        this.labelHeading.Text = "Please set the needed options for the MediaPortal installation:";
        // 
        // checkBoxFirewall
        // 
        this.checkBoxFirewall.AutoSize = true;
        this.checkBoxFirewall.Checked = true;
        this.checkBoxFirewall.CheckState = System.Windows.Forms.CheckState.Checked;
        this.checkBoxFirewall.Cursor = System.Windows.Forms.Cursors.Hand;
        this.checkBoxFirewall.ForeColor = System.Drawing.Color.White;
        this.checkBoxFirewall.Location = new System.Drawing.Point(12, 117);
        this.checkBoxFirewall.Name = "checkBoxFirewall";
        this.checkBoxFirewall.Size = new System.Drawing.Size(409, 17);
        this.checkBoxFirewall.TabIndex = 23;
        this.checkBoxFirewall.Text = "Configure Windows Firewall to allow external access to MediaPortal";
        this.checkBoxFirewall.UseVisualStyleBackColor = true;
        // 
        // MPSettingsDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
        this.Controls.Add(this.checkBoxFirewall);
        this.Controls.Add(this.buttonBrowse);
        this.Controls.Add(this.textBoxDir);
        this.Controls.Add(this.labelInstDir);
        this.Controls.Add(this.labelHeading);
        this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Name = "MPSettingsDlg";
        this.Size = new System.Drawing.Size(723, 172);
        this.Load += new System.EventHandler(this.MPSettingsDlg_Load);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.labelHeading, 0);
        this.Controls.SetChildIndex(this.labelInstDir, 0);
        this.Controls.SetChildIndex(this.textBoxDir, 0);
        this.Controls.SetChildIndex(this.buttonBrowse, 0);
        this.Controls.SetChildIndex(this.checkBoxFirewall, 0);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.TextBox textBoxDir;
    private System.Windows.Forms.Label labelInstDir;
    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.CheckBox checkBoxFirewall;
  }
}
