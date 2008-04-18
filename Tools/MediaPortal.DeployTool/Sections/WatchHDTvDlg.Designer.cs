namespace MediaPortal.DeployTool
{
  partial class WatchHDTvDlg
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
      this.imgYesHD = new System.Windows.Forms.PictureBox();
      this.imgNoHD = new System.Windows.Forms.PictureBox();
      this.imgMaybeHD = new System.Windows.Forms.PictureBox();
      this.rbYesHD = new System.Windows.Forms.Label();
      this.rbNoHD = new System.Windows.Forms.Label();
      this.rbMaybeHD = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.imgYesHD)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgNoHD)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgMaybeHD)).BeginInit();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelSectionHeader.Location = new System.Drawing.Point(170, 39);
      this.labelSectionHeader.Size = new System.Drawing.Size(328, 16);
      this.labelSectionHeader.Text = "Do you want to watch HDTV with MediaPortal ?";
      // 
      // imgYesHD
      // 
      this.imgYesHD.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.imgYesHD.Location = new System.Drawing.Point(227, 78);
      this.imgYesHD.Name = "imgYesHD";
      this.imgYesHD.Size = new System.Drawing.Size(21, 21);
      this.imgYesHD.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.imgYesHD.TabIndex = 13;
      this.imgYesHD.TabStop = false;
      this.imgYesHD.Click += new System.EventHandler(this.imgYesHD_Click);
      // 
      // imgNoHD
      // 
      this.imgNoHD.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.imgNoHD.Location = new System.Drawing.Point(227, 128);
      this.imgNoHD.Name = "imgNoHD";
      this.imgNoHD.Size = new System.Drawing.Size(21, 21);
      this.imgNoHD.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.imgNoHD.TabIndex = 14;
      this.imgNoHD.TabStop = false;
      this.imgNoHD.Click += new System.EventHandler(this.imgNoHD_Click);
      // 
      // imgMaybeHD
      // 
      this.imgMaybeHD.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.imgMaybeHD.Location = new System.Drawing.Point(227, 176);
      this.imgMaybeHD.Name = "imgMaybeHD";
      this.imgMaybeHD.Size = new System.Drawing.Size(21, 21);
      this.imgMaybeHD.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.imgMaybeHD.TabIndex = 15;
      this.imgMaybeHD.TabStop = false;
      this.imgMaybeHD.Click += new System.EventHandler(this.imgMaybeHD_Click);
      // 
      // rbYesHD
      // 
      this.rbYesHD.AutoSize = true;
      this.rbYesHD.ForeColor = System.Drawing.Color.White;
      this.rbYesHD.Location = new System.Drawing.Point(255, 82);
      this.rbYesHD.Name = "rbYesHD";
      this.rbYesHD.Size = new System.Drawing.Size(238, 13);
      this.rbYesHD.TabIndex = 16;
      this.rbYesHD.Text = "Yes, I will use MediaPortal  to watch HD content.";
      this.rbYesHD.Click += new System.EventHandler(this.imgYesHD_Click);
      // 
      // rbNoHD
      // 
      this.rbNoHD.AutoSize = true;
      this.rbNoHD.ForeColor = System.Drawing.Color.White;
      this.rbNoHD.Location = new System.Drawing.Point(254, 132);
      this.rbNoHD.Name = "rbNoHD";
      this.rbNoHD.Size = new System.Drawing.Size(242, 13);
      this.rbNoHD.TabIndex = 17;
      this.rbNoHD.Text = "No, I won\'t use MediaPortal to watch HD content.";
      this.rbNoHD.Click += new System.EventHandler(this.imgNoHD_Click);
      // 
      // rbMaybeHD
      // 
      this.rbMaybeHD.AutoSize = true;
      this.rbMaybeHD.ForeColor = System.Drawing.Color.White;
      this.rbMaybeHD.Location = new System.Drawing.Point(255, 180);
      this.rbMaybeHD.Name = "rbMaybeHD";
      this.rbMaybeHD.Size = new System.Drawing.Size(162, 13);
      this.rbMaybeHD.TabIndex = 18;
      this.rbMaybeHD.Text = "I don\'t know what is HD content.";
      this.rbMaybeHD.Click += new System.EventHandler(this.imgMaybeHD_Click);
      // 
      // WatchHDTvDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_TV_install;
      this.Controls.Add(this.rbMaybeHD);
      this.Controls.Add(this.rbNoHD);
      this.Controls.Add(this.rbYesHD);
      this.Controls.Add(this.imgMaybeHD);
      this.Controls.Add(this.imgNoHD);
      this.Controls.Add(this.imgYesHD);
      this.Name = "WatchHDTvDlg";
      this.Size = new System.Drawing.Size(542, 251);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.imgYesHD, 0);
      this.Controls.SetChildIndex(this.imgNoHD, 0);
      this.Controls.SetChildIndex(this.imgMaybeHD, 0);
      this.Controls.SetChildIndex(this.rbYesHD, 0);
      this.Controls.SetChildIndex(this.rbNoHD, 0);
      this.Controls.SetChildIndex(this.rbMaybeHD, 0);
      ((System.ComponentModel.ISupportInitialize)(this.imgYesHD)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgNoHD)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgMaybeHD)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox imgYesHD;
    private System.Windows.Forms.PictureBox imgNoHD;
    private System.Windows.Forms.PictureBox imgMaybeHD;
    private System.Windows.Forms.Label rbYesHD;
    private System.Windows.Forms.Label rbNoHD;
    private System.Windows.Forms.Label rbMaybeHD;


  }
}
