namespace MediaPortal.DeployTool.Sections
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
      this.rbInstallNow = new System.Windows.Forms.Label();
      this.rbDownloadOnly = new System.Windows.Forms.Label();
      this.bInstallNow = new System.Windows.Forms.Button();
      this.bDownloadOnly = new System.Windows.Forms.Button();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Location = new System.Drawing.Point(330, 92);
      this.labelSectionHeader.MaximumSize = new System.Drawing.Size(600, 0);
      this.labelSectionHeader.Size = new System.Drawing.Size(599, 85);
      this.labelSectionHeader.Text = resources.GetString("labelSectionHeader.Text");
      // 
      // rbInstallNow
      // 
      this.rbInstallNow.AutoSize = true;
      this.rbInstallNow.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbInstallNow.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbInstallNow.ForeColor = System.Drawing.Color.White;
      this.rbInstallNow.Location = new System.Drawing.Point(376, 228);
      this.rbInstallNow.Name = "rbInstallNow";
      this.rbInstallNow.Size = new System.Drawing.Size(176, 13);
      this.rbInstallNow.TabIndex = 20;
      this.rbInstallNow.Text = "Start with the installation now";
      this.rbInstallNow.Click += new System.EventHandler(this.bInstallNow_Click);
      // 
      // rbDownloadOnly
      // 
      this.rbDownloadOnly.AutoSize = true;
      this.rbDownloadOnly.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbDownloadOnly.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbDownloadOnly.ForeColor = System.Drawing.Color.White;
      this.rbDownloadOnly.Location = new System.Drawing.Point(376, 276);
      this.rbDownloadOnly.Name = "rbDownloadOnly";
      this.rbDownloadOnly.Size = new System.Drawing.Size(216, 13);
      this.rbDownloadOnly.TabIndex = 20;
      this.rbDownloadOnly.Text = "Only download required components";
      this.rbDownloadOnly.Click += new System.EventHandler(this.bDownloadOnly_Click);
      // 
      // bInstallNow
      // 
      this.bInstallNow.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bInstallNow.FlatAppearance.BorderSize = 0;
      this.bInstallNow.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bInstallNow.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bInstallNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bInstallNow.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bInstallNow.Location = new System.Drawing.Point(333, 223);
      this.bInstallNow.Name = "bInstallNow";
      this.bInstallNow.Size = new System.Drawing.Size(37, 23);
      this.bInstallNow.TabIndex = 1;
      this.bInstallNow.UseVisualStyleBackColor = true;
      this.bInstallNow.Click += new System.EventHandler(this.bInstallNow_Click);
      // 
      // bDownloadOnly
      // 
      this.bDownloadOnly.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bDownloadOnly.FlatAppearance.BorderSize = 0;
      this.bDownloadOnly.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bDownloadOnly.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bDownloadOnly.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bDownloadOnly.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bDownloadOnly.Location = new System.Drawing.Point(333, 271);
      this.bDownloadOnly.Name = "bDownloadOnly";
      this.bDownloadOnly.Size = new System.Drawing.Size(37, 23);
      this.bDownloadOnly.TabIndex = 2;
      this.bDownloadOnly.UseVisualStyleBackColor = true;
      this.bDownloadOnly.Click += new System.EventHandler(this.bDownloadOnly_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.Internet_connection;
      this.pictureBox1.Location = new System.Drawing.Point(3, 62);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(324, 309);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox1.TabIndex = 21;
      this.pictureBox1.TabStop = false;
      // 
      // DownloadOnlyDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.bDownloadOnly);
      this.Controls.Add(this.bInstallNow);
      this.Controls.Add(this.rbDownloadOnly);
      this.Controls.Add(this.rbInstallNow);
      this.Name = "DownloadOnlyDlg";
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.rbInstallNow, 0);
      this.Controls.SetChildIndex(this.rbDownloadOnly, 0);
      this.Controls.SetChildIndex(this.bInstallNow, 0);
      this.Controls.SetChildIndex(this.bDownloadOnly, 0);
      this.Controls.SetChildIndex(this.pictureBox1, 0);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label rbInstallNow;
    private System.Windows.Forms.Label rbDownloadOnly;
    private System.Windows.Forms.Button bInstallNow;
    private System.Windows.Forms.Button bDownloadOnly;
    private System.Windows.Forms.PictureBox pictureBox1;
  }
}