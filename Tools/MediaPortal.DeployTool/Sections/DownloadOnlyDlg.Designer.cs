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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DownloadOnlyDlg));
        this.rbDownloadOnly = new System.Windows.Forms.RadioButton();
        this.rbInstallNow = new System.Windows.Forms.RadioButton();
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelSectionHeader.Location = new System.Drawing.Point(53, 28);
        this.labelSectionHeader.Size = new System.Drawing.Size(476, 64);
        this.labelSectionHeader.Text = resources.GetString("labelSectionHeader.Text");
        // 
        // rbDownloadOnly
        // 
        this.rbDownloadOnly.AutoSize = true;
        this.rbDownloadOnly.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbDownloadOnly.Location = new System.Drawing.Point(94, 164);
        this.rbDownloadOnly.Name = "rbDownloadOnly";
        this.rbDownloadOnly.Size = new System.Drawing.Size(197, 17);
        this.rbDownloadOnly.TabIndex = 9;
        this.rbDownloadOnly.TabStop = true;
        this.rbDownloadOnly.Text = "Only download required components";
        this.rbDownloadOnly.UseVisualStyleBackColor = true;
        // 
        // rbInstallNow
        // 
        this.rbInstallNow.AutoSize = true;
        this.rbInstallNow.Checked = true;
        this.rbInstallNow.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbInstallNow.Location = new System.Drawing.Point(94, 123);
        this.rbInstallNow.Name = "rbInstallNow";
        this.rbInstallNow.Size = new System.Drawing.Size(162, 17);
        this.rbInstallNow.TabIndex = 10;
        this.rbInstallNow.TabStop = true;
        this.rbInstallNow.Text = "Start with the installation now";
        this.rbInstallNow.UseVisualStyleBackColor = true;
        // 
        // pictureBox1
        // 
        this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.MePo_download;
        this.pictureBox1.Location = new System.Drawing.Point(19, 111);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(69, 82);
        this.pictureBox1.TabIndex = 11;
        this.pictureBox1.TabStop = false;
        // 
        // DownloadOnlyDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.pictureBox1);
        this.Controls.Add(this.rbInstallNow);
        this.Controls.Add(this.rbDownloadOnly);
        this.Name = "DownloadOnlyDlg";
        this.Size = new System.Drawing.Size(542, 266);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.rbDownloadOnly, 0);
        this.Controls.SetChildIndex(this.rbInstallNow, 0);
        this.Controls.SetChildIndex(this.pictureBox1, 0);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton rbDownloadOnly;
    private System.Windows.Forms.RadioButton rbInstallNow;
      private System.Windows.Forms.PictureBox pictureBox1;

  }
}
