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
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelSectionHeader.Location = new System.Drawing.Point(192, 33);
      this.labelSectionHeader.MaximumSize = new System.Drawing.Size(450, 0);
      this.labelSectionHeader.Size = new System.Drawing.Size(438, 84);
      this.labelSectionHeader.Text = resources.GetString("labelSectionHeader.Text");
      // 
      // rbInstallNow
      // 
      this.rbInstallNow.AutoSize = true;
      this.rbInstallNow.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbInstallNow.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbInstallNow.ForeColor = System.Drawing.Color.White;
      this.rbInstallNow.Location = new System.Drawing.Point(238, 151);
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
      this.rbDownloadOnly.Location = new System.Drawing.Point(238, 204);
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
      this.bInstallNow.Location = new System.Drawing.Point(195, 146);
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
      this.bDownloadOnly.Location = new System.Drawing.Point(195, 199);
      this.bDownloadOnly.Name = "bDownloadOnly";
      this.bDownloadOnly.Size = new System.Drawing.Size(37, 23);
      this.bDownloadOnly.TabIndex = 2;
      this.bDownloadOnly.UseVisualStyleBackColor = true;
      this.bDownloadOnly.Click += new System.EventHandler(this.bDownloadOnly_Click);
      // 
      // DownloadOnlyDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_internet_connection;
      this.Controls.Add(this.bDownloadOnly);
      this.Controls.Add(this.bInstallNow);
      this.Controls.Add(this.rbDownloadOnly);
      this.Controls.Add(this.rbInstallNow);
      this.Name = "DownloadOnlyDlg";
      this.Size = new System.Drawing.Size(666, 250);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.rbInstallNow, 0);
      this.Controls.SetChildIndex(this.rbDownloadOnly, 0);
      this.Controls.SetChildIndex(this.bInstallNow, 0);
      this.Controls.SetChildIndex(this.bDownloadOnly, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label rbInstallNow;
    private System.Windows.Forms.Label rbDownloadOnly;
    private System.Windows.Forms.Button bInstallNow;
    private System.Windows.Forms.Button bDownloadOnly;

  }
}