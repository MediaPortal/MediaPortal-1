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
        this.imgInstallNow = new System.Windows.Forms.PictureBox();
        this.imgDownloadOnly = new System.Windows.Forms.PictureBox();
        this.rbInstallNow = new System.Windows.Forms.Label();
        this.rbDownloadOnly = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.imgInstallNow)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.imgDownloadOnly)).BeginInit();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelSectionHeader.Location = new System.Drawing.Point(200, 14);
        this.labelSectionHeader.MaximumSize = new System.Drawing.Size(350, 0);
        this.labelSectionHeader.Size = new System.Drawing.Size(348, 112);
        this.labelSectionHeader.Text = resources.GetString("labelSectionHeader.Text");
        this.labelSectionHeader.Click += new System.EventHandler(this.labelSectionHeader_Click);
        // 
        // imgInstallNow
        // 
        this.imgInstallNow.Cursor = System.Windows.Forms.Cursors.Hand;
        this.imgInstallNow.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
        this.imgInstallNow.Location = new System.Drawing.Point(200, 164);
        this.imgInstallNow.Name = "imgInstallNow";
        this.imgInstallNow.Size = new System.Drawing.Size(21, 21);
        this.imgInstallNow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.imgInstallNow.TabIndex = 11;
        this.imgInstallNow.TabStop = false;
        this.imgInstallNow.Click += new System.EventHandler(this.imgInstallNow_Click);
        // 
        // imgDownloadOnly
        // 
        this.imgDownloadOnly.Cursor = System.Windows.Forms.Cursors.Hand;
        this.imgDownloadOnly.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
        this.imgDownloadOnly.Location = new System.Drawing.Point(200, 199);
        this.imgDownloadOnly.Name = "imgDownloadOnly";
        this.imgDownloadOnly.Size = new System.Drawing.Size(21, 21);
        this.imgDownloadOnly.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.imgDownloadOnly.TabIndex = 12;
        this.imgDownloadOnly.TabStop = false;
        this.imgDownloadOnly.Click += new System.EventHandler(this.imgDownloadOnly_Click);
        // 
        // rbInstallNow
        // 
        this.rbInstallNow.AutoSize = true;
        this.rbInstallNow.Cursor = System.Windows.Forms.Cursors.Hand;
        this.rbInstallNow.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbInstallNow.ForeColor = System.Drawing.Color.White;
        this.rbInstallNow.Location = new System.Drawing.Point(230, 168);
        this.rbInstallNow.Name = "rbInstallNow";
        this.rbInstallNow.Size = new System.Drawing.Size(176, 13);
        this.rbInstallNow.TabIndex = 13;
        this.rbInstallNow.Text = "Start with the installation now";
        this.rbInstallNow.Click += new System.EventHandler(this.imgInstallNow_Click);
        // 
        // rbDownloadOnly
        // 
        this.rbDownloadOnly.AutoSize = true;
        this.rbDownloadOnly.Cursor = System.Windows.Forms.Cursors.Hand;
        this.rbDownloadOnly.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbDownloadOnly.ForeColor = System.Drawing.Color.White;
        this.rbDownloadOnly.Location = new System.Drawing.Point(230, 203);
        this.rbDownloadOnly.Name = "rbDownloadOnly";
        this.rbDownloadOnly.Size = new System.Drawing.Size(216, 13);
        this.rbDownloadOnly.TabIndex = 14;
        this.rbDownloadOnly.Text = "Only download required components";
        this.rbDownloadOnly.Click += new System.EventHandler(this.imgDownloadOnly_Click);
        // 
        // DownloadOnlyDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_internet_connection;
        this.Controls.Add(this.rbDownloadOnly);
        this.Controls.Add(this.rbInstallNow);
        this.Controls.Add(this.imgDownloadOnly);
        this.Controls.Add(this.imgInstallNow);
        this.Name = "DownloadOnlyDlg";
        this.Size = new System.Drawing.Size(669, 251);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.imgInstallNow, 0);
        this.Controls.SetChildIndex(this.imgDownloadOnly, 0);
        this.Controls.SetChildIndex(this.rbInstallNow, 0);
        this.Controls.SetChildIndex(this.rbDownloadOnly, 0);
        ((System.ComponentModel.ISupportInitialize)(this.imgInstallNow)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.imgDownloadOnly)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox imgInstallNow;
    private System.Windows.Forms.PictureBox imgDownloadOnly;
    private System.Windows.Forms.Label rbInstallNow;
    private System.Windows.Forms.Label rbDownloadOnly;

  }
}
