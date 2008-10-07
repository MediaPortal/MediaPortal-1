namespace MediaPortal.DeployTool
{
  partial class TvServerSettingsDlg
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
        this.buttonBrowse.Location = new System.Drawing.Point(524, 64);
        this.buttonBrowse.Name = "buttonBrowse";
        this.buttonBrowse.Size = new System.Drawing.Size(120, 23);
        this.buttonBrowse.TabIndex = 21;
        this.buttonBrowse.Text = "browse";
        this.buttonBrowse.UseVisualStyleBackColor = true;
        this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
        // 
        // textBoxDir
        // 
        this.textBoxDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.textBoxDir.Location = new System.Drawing.Point(10, 66);
        this.textBoxDir.Name = "textBoxDir";
        this.textBoxDir.Size = new System.Drawing.Size(486, 21);
        this.textBoxDir.TabIndex = 20;
        // 
        // labelInstDir
        // 
        this.labelInstDir.AutoSize = true;
        this.labelInstDir.ForeColor = System.Drawing.Color.White;
        this.labelInstDir.Location = new System.Drawing.Point(7, 50);
        this.labelInstDir.Name = "labelInstDir";
        this.labelInstDir.Size = new System.Drawing.Size(127, 13);
        this.labelInstDir.TabIndex = 19;
        this.labelInstDir.Text = "TV-Server install dir:";
        // 
        // labelHeading
        // 
        this.labelHeading.AutoSize = true;
        this.labelHeading.ForeColor = System.Drawing.Color.White;
        this.labelHeading.Location = new System.Drawing.Point(5, 23);
        this.labelHeading.Name = "labelHeading";
        this.labelHeading.Size = new System.Drawing.Size(353, 13);
        this.labelHeading.TabIndex = 18;
        this.labelHeading.Text = "Please set the needed options for the TV-Server installation:";
        // 
        // checkBoxFirewall
        // 
        this.checkBoxFirewall.AutoSize = true;
        this.checkBoxFirewall.Checked = true;
        this.checkBoxFirewall.CheckState = System.Windows.Forms.CheckState.Checked;
        this.checkBoxFirewall.ForeColor = System.Drawing.Color.White;
        this.checkBoxFirewall.Location = new System.Drawing.Point(10, 118);
        this.checkBoxFirewall.Name = "checkBoxFirewall";
        this.checkBoxFirewall.Size = new System.Drawing.Size(396, 17);
        this.checkBoxFirewall.TabIndex = 22;
        this.checkBoxFirewall.Text = "Configure Windows Firewall to allow external access to TvServer";
        this.checkBoxFirewall.UseVisualStyleBackColor = true;
        // 
        // TvServerSettingsDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.checkBoxFirewall);
        this.Controls.Add(this.buttonBrowse);
        this.Controls.Add(this.textBoxDir);
        this.Controls.Add(this.labelInstDir);
        this.Controls.Add(this.labelHeading);
        this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Name = "TvServerSettingsDlg";
        this.Size = new System.Drawing.Size(723, 172);
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
