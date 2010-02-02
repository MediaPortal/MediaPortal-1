namespace MediaPortal.DeployTool.Sections
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
      this.rbYesWatchTv = new System.Windows.Forms.Label();
      this.rbNoWatchTv = new System.Windows.Forms.Label();
      this.bYes = new System.Windows.Forms.Button();
      this.bNo = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelSectionHeader.Location = new System.Drawing.Point(197, 42);
      this.labelSectionHeader.MaximumSize = new System.Drawing.Size(405, 0);
      this.labelSectionHeader.Size = new System.Drawing.Size(324, 16);
      this.labelSectionHeader.Text = "Do you want to watch TV with MediaPortal ?";
      // 
      // rbYesWatchTv
      // 
      this.rbYesWatchTv.AutoSize = true;
      this.rbYesWatchTv.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbYesWatchTv.ForeColor = System.Drawing.Color.White;
      this.rbYesWatchTv.Location = new System.Drawing.Point(261, 106);
      this.rbYesWatchTv.MaximumSize = new System.Drawing.Size(257, 0);
      this.rbYesWatchTv.Name = "rbYesWatchTv";
      this.rbYesWatchTv.Size = new System.Drawing.Size(194, 13);
      this.rbYesWatchTv.TabIndex = 17;
      this.rbYesWatchTv.Text = "Yes, I will use MediaPortal to watch TV.";
      this.rbYesWatchTv.Click += new System.EventHandler(this.bYes_Click);
      // 
      // rbNoWatchTv
      // 
      this.rbNoWatchTv.AutoSize = true;
      this.rbNoWatchTv.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbNoWatchTv.ForeColor = System.Drawing.Color.White;
      this.rbNoWatchTv.Location = new System.Drawing.Point(261, 159);
      this.rbNoWatchTv.MaximumSize = new System.Drawing.Size(257, 0);
      this.rbNoWatchTv.Name = "rbNoWatchTv";
      this.rbNoWatchTv.Size = new System.Drawing.Size(201, 13);
      this.rbNoWatchTv.TabIndex = 19;
      this.rbNoWatchTv.Text = "No, I won\'t use MediaPortal to watch TV.";
      this.rbNoWatchTv.Click += new System.EventHandler(this.bNo_Click);
      // 
      // bYes
      // 
      this.bYes.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bYes.FlatAppearance.BorderSize = 0;
      this.bYes.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bYes.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bYes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bYes.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bYes.Location = new System.Drawing.Point(200, 101);
      this.bYes.Name = "bYes";
      this.bYes.Size = new System.Drawing.Size(32, 23);
      this.bYes.TabIndex = 20;
      this.bYes.UseVisualStyleBackColor = true;
      this.bYes.Click += new System.EventHandler(this.bYes_Click);
      // 
      // bNo
      // 
      this.bNo.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bNo.FlatAppearance.BorderSize = 0;
      this.bNo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bNo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bNo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bNo.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bNo.Location = new System.Drawing.Point(200, 154);
      this.bNo.Name = "bNo";
      this.bNo.Size = new System.Drawing.Size(32, 23);
      this.bNo.TabIndex = 21;
      this.bNo.UseVisualStyleBackColor = true;
      this.bNo.Click += new System.EventHandler(this.bNo_Click);
      // 
      // WatchTVDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_TV__yes_no;
      this.Controls.Add(this.bNo);
      this.Controls.Add(this.bYes);
      this.Controls.Add(this.rbNoWatchTv);
      this.Controls.Add(this.rbYesWatchTv);
      this.Name = "WatchTVDlg";
      this.Size = new System.Drawing.Size(666, 250);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.rbYesWatchTv, 0);
      this.Controls.SetChildIndex(this.rbNoWatchTv, 0);
      this.Controls.SetChildIndex(this.bYes, 0);
      this.Controls.SetChildIndex(this.bNo, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label rbYesWatchTv;
    private System.Windows.Forms.Label rbNoWatchTv;
    private System.Windows.Forms.Button bYes;
    private System.Windows.Forms.Button bNo;

  }
}