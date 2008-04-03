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
        this.rbYesHD = new System.Windows.Forms.RadioButton();
        this.rbNoHD = new System.Windows.Forms.RadioButton();
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        this.rbMaybeHD = new System.Windows.Forms.RadioButton();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelSectionHeader.Location = new System.Drawing.Point(53, 28);
        this.labelSectionHeader.Size = new System.Drawing.Size(328, 16);
        this.labelSectionHeader.Text = "Do you want to watch HDTV with MediaPortal ?";
        // 
        // rbYesHD
        // 
        this.rbYesHD.AutoSize = true;
        this.rbYesHD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbYesHD.Location = new System.Drawing.Point(94, 96);
        this.rbYesHD.Name = "rbYesHD";
        this.rbYesHD.Size = new System.Drawing.Size(256, 17);
        this.rbYesHD.TabIndex = 9;
        this.rbYesHD.TabStop = true;
        this.rbYesHD.Text = "Yes, I will use MediaPortal  to watch HD content.";
        this.rbYesHD.UseVisualStyleBackColor = true;
        // 
        // rbNoHD
        // 
        this.rbNoHD.AutoSize = true;
        this.rbNoHD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbNoHD.Location = new System.Drawing.Point(94, 141);
        this.rbNoHD.Name = "rbNoHD";
        this.rbNoHD.Size = new System.Drawing.Size(260, 17);
        this.rbNoHD.TabIndex = 10;
        this.rbNoHD.TabStop = true;
        this.rbNoHD.Text = "No, I won\'t use MediaPortal to watch HD content.";
        this.rbNoHD.UseVisualStyleBackColor = true;
        // 
        // pictureBox1
        // 
        this.pictureBox1.BackgroundImage = global::MediaPortal.DeployTool.Images.MePo_tv;
        this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.MePo_tv;
        this.pictureBox1.InitialImage = global::MediaPortal.DeployTool.Images.MePo_tv;
        this.pictureBox1.Location = new System.Drawing.Point(19, 96);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(69, 82);
        this.pictureBox1.TabIndex = 11;
        this.pictureBox1.TabStop = false;
        // 
        // rbMaybeHD
        // 
        this.rbMaybeHD.AutoSize = true;
        this.rbMaybeHD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbMaybeHD.Location = new System.Drawing.Point(94, 180);
        this.rbMaybeHD.Name = "rbMaybeHD";
        this.rbMaybeHD.Size = new System.Drawing.Size(180, 17);
        this.rbMaybeHD.TabIndex = 12;
        this.rbMaybeHD.TabStop = true;
        this.rbMaybeHD.Text = "I don\'t know what is HD content.";
        this.rbMaybeHD.UseVisualStyleBackColor = true;
        // 
        // WatchHDTvDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.rbMaybeHD);
        this.Controls.Add(this.pictureBox1);
        this.Controls.Add(this.rbNoHD);
        this.Controls.Add(this.rbYesHD);
        this.Name = "WatchHDTvDlg";
        this.Size = new System.Drawing.Size(542, 266);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.rbYesHD, 0);
        this.Controls.SetChildIndex(this.rbNoHD, 0);
        this.Controls.SetChildIndex(this.pictureBox1, 0);
        this.Controls.SetChildIndex(this.rbMaybeHD, 0);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton rbYesHD;
    private System.Windows.Forms.RadioButton rbNoHD;
      private System.Windows.Forms.PictureBox pictureBox1;
      private System.Windows.Forms.RadioButton rbMaybeHD;

  }
}
