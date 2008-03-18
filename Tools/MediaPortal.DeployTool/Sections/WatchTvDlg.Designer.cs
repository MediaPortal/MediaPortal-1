namespace MediaPortal.DeployTool
{
  partial class WatchTVDlg
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
        this.rbYesWatchTv = new System.Windows.Forms.RadioButton();
        this.rbNoWatchTv = new System.Windows.Forms.RadioButton();
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelSectionHeader.Location = new System.Drawing.Point(53, 28);
        this.labelSectionHeader.Size = new System.Drawing.Size(290, 16);
        this.labelSectionHeader.Text = "Do you want to watch TV with MediaPortal ?";
        // 
        // rbYesWatchTv
        // 
        this.rbYesWatchTv.AutoSize = true;
        this.rbYesWatchTv.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbYesWatchTv.Location = new System.Drawing.Point(94, 96);
        this.rbYesWatchTv.Name = "rbYesWatchTv";
        this.rbYesWatchTv.Size = new System.Drawing.Size(217, 18);
        this.rbYesWatchTv.TabIndex = 9;
        this.rbYesWatchTv.TabStop = true;
        this.rbYesWatchTv.Text = "Yes, I will use MediaPortal to watch TV.";
        this.rbYesWatchTv.UseVisualStyleBackColor = true;
        // 
        // rbNoWatchTv
        // 
        this.rbNoWatchTv.AutoSize = true;
        this.rbNoWatchTv.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbNoWatchTv.Location = new System.Drawing.Point(94, 141);
        this.rbNoWatchTv.Name = "rbNoWatchTv";
        this.rbNoWatchTv.Size = new System.Drawing.Size(221, 18);
        this.rbNoWatchTv.TabIndex = 10;
        this.rbNoWatchTv.TabStop = true;
        this.rbNoWatchTv.Text = "No, I won\'t use MediaPortal to watch TV.";
        this.rbNoWatchTv.UseVisualStyleBackColor = true;
        // 
        // pictureBox1
        // 
        this.pictureBox1.BackgroundImage = global::MediaPortal.DeployTool.Images.MyTV;
        this.pictureBox1.InitialImage = global::MediaPortal.DeployTool.Images.MyTV;
        this.pictureBox1.Location = new System.Drawing.Point(22, 96);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(56, 63);
        this.pictureBox1.TabIndex = 11;
        this.pictureBox1.TabStop = false;
        // 
        // WatchTVDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.pictureBox1);
        this.Controls.Add(this.rbNoWatchTv);
        this.Controls.Add(this.rbYesWatchTv);
        this.Name = "WatchTVDlg";
        this.Size = new System.Drawing.Size(542, 266);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.rbYesWatchTv, 0);
        this.Controls.SetChildIndex(this.rbNoWatchTv, 0);
        this.Controls.SetChildIndex(this.pictureBox1, 0);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton rbYesWatchTv;
    private System.Windows.Forms.RadioButton rbNoWatchTv;
    private System.Windows.Forms.PictureBox pictureBox1;

  }
}
