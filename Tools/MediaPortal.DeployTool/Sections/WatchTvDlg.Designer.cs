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
        this.imgYes = new System.Windows.Forms.PictureBox();
        this.rbYesWatchTv = new System.Windows.Forms.Label();
        this.rbNoWatchTv = new System.Windows.Forms.Label();
        this.imgNo = new System.Windows.Forms.PictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.imgYes)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.imgNo)).BeginInit();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelSectionHeader.Location = new System.Drawing.Point(230, 29);
        this.labelSectionHeader.MaximumSize = new System.Drawing.Size(350, 0);
        this.labelSectionHeader.Size = new System.Drawing.Size(324, 16);
        this.labelSectionHeader.Text = "Do you want to watch TV with MediaPortal ?";
        // 
        // imgYes
        // 
        this.imgYes.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
        this.imgYes.Location = new System.Drawing.Point(233, 78);
        this.imgYes.Name = "imgYes";
        this.imgYes.Size = new System.Drawing.Size(21, 21);
        this.imgYes.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.imgYes.TabIndex = 14;
        this.imgYes.TabStop = false;
        this.imgYes.Click += new System.EventHandler(this.imgYes_Click);
        // 
        // rbYesWatchTv
        // 
        this.rbYesWatchTv.AutoSize = true;
        this.rbYesWatchTv.ForeColor = System.Drawing.Color.White;
        this.rbYesWatchTv.Location = new System.Drawing.Point(265, 82);
        this.rbYesWatchTv.MaximumSize = new System.Drawing.Size(300, 0);
        this.rbYesWatchTv.Name = "rbYesWatchTv";
        this.rbYesWatchTv.Size = new System.Drawing.Size(231, 13);
        this.rbYesWatchTv.TabIndex = 17;
        this.rbYesWatchTv.Text = "Yes, I will use MediaPortal to watch TV.";
        this.rbYesWatchTv.Click += new System.EventHandler(this.imgYes_Click);
        // 
        // rbNoWatchTv
        // 
        this.rbNoWatchTv.AutoSize = true;
        this.rbNoWatchTv.ForeColor = System.Drawing.Color.White;
        this.rbNoWatchTv.Location = new System.Drawing.Point(265, 132);
        this.rbNoWatchTv.MaximumSize = new System.Drawing.Size(300, 0);
        this.rbNoWatchTv.Name = "rbNoWatchTv";
        this.rbNoWatchTv.Size = new System.Drawing.Size(238, 13);
        this.rbNoWatchTv.TabIndex = 19;
        this.rbNoWatchTv.Text = "No, I won\'t use MediaPortal to watch TV.";
        this.rbNoWatchTv.Click += new System.EventHandler(this.imgNo_Click);
        // 
        // imgNo
        // 
        this.imgNo.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
        this.imgNo.Location = new System.Drawing.Point(233, 128);
        this.imgNo.Name = "imgNo";
        this.imgNo.Size = new System.Drawing.Size(21, 21);
        this.imgNo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.imgNo.TabIndex = 18;
        this.imgNo.TabStop = false;
        this.imgNo.Click += new System.EventHandler(this.imgNo_Click);
        // 
        // WatchTVDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_TV__yes_no;
        this.Controls.Add(this.rbNoWatchTv);
        this.Controls.Add(this.imgNo);
        this.Controls.Add(this.rbYesWatchTv);
        this.Controls.Add(this.imgYes);
        this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ForeColor = System.Drawing.Color.White;
        this.Name = "WatchTVDlg";
        this.Size = new System.Drawing.Size(632, 248);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.imgYes, 0);
        this.Controls.SetChildIndex(this.rbYesWatchTv, 0);
        this.Controls.SetChildIndex(this.imgNo, 0);
        this.Controls.SetChildIndex(this.rbNoWatchTv, 0);
        ((System.ComponentModel.ISupportInitialize)(this.imgYes)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.imgNo)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox imgYes;
    private System.Windows.Forms.Label rbYesWatchTv;
    private System.Windows.Forms.Label rbNoWatchTv;
    private System.Windows.Forms.PictureBox imgNo;

  }
}
