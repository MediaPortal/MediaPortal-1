namespace MediaPortal.DeployTool.Sections
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
      this.rbYesHD = new System.Windows.Forms.Label();
      this.rbNoHD = new System.Windows.Forms.Label();
      this.rbMaybeHD = new System.Windows.Forms.Label();
      this.bYesHD = new System.Windows.Forms.Button();
      this.bNoHD = new System.Windows.Forms.Button();
      this.bMaybeHD = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelSectionHeader.Location = new System.Drawing.Point(230, 29);
      this.labelSectionHeader.MaximumSize = new System.Drawing.Size(350, 0);
      this.labelSectionHeader.Size = new System.Drawing.Size(345, 16);
      this.labelSectionHeader.Text = "Do you want to watch HDTV with MediaPortal ?";
      // 
      // rbYesHD
      // 
      this.rbYesHD.AutoSize = true;
      this.rbYesHD.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbYesHD.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbYesHD.ForeColor = System.Drawing.Color.White;
      this.rbYesHD.Location = new System.Drawing.Point(271, 82);
      this.rbYesHD.MaximumSize = new System.Drawing.Size(350, 0);
      this.rbYesHD.Name = "rbYesHD";
      this.rbYesHD.Size = new System.Drawing.Size(283, 13);
      this.rbYesHD.TabIndex = 16;
      this.rbYesHD.Text = "Yes, I will use MediaPortal  to watch HD content.";
      this.rbYesHD.Click += new System.EventHandler(this.bYesHD_Click);
      // 
      // rbNoHD
      // 
      this.rbNoHD.AutoSize = true;
      this.rbNoHD.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbNoHD.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbNoHD.ForeColor = System.Drawing.Color.White;
      this.rbNoHD.Location = new System.Drawing.Point(271, 132);
      this.rbNoHD.MaximumSize = new System.Drawing.Size(350, 0);
      this.rbNoHD.Name = "rbNoHD";
      this.rbNoHD.Size = new System.Drawing.Size(286, 13);
      this.rbNoHD.TabIndex = 17;
      this.rbNoHD.Text = "No, I won\'t use MediaPortal to watch HD content.";
      this.rbNoHD.Click += new System.EventHandler(this.bNoHD_Click);
      // 
      // rbMaybeHD
      // 
      this.rbMaybeHD.AutoSize = true;
      this.rbMaybeHD.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbMaybeHD.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbMaybeHD.ForeColor = System.Drawing.Color.White;
      this.rbMaybeHD.Location = new System.Drawing.Point(271, 182);
      this.rbMaybeHD.MaximumSize = new System.Drawing.Size(350, 0);
      this.rbMaybeHD.Name = "rbMaybeHD";
      this.rbMaybeHD.Size = new System.Drawing.Size(193, 13);
      this.rbMaybeHD.TabIndex = 18;
      this.rbMaybeHD.Text = "I don\'t know what is HD content.";
      this.rbMaybeHD.Click += new System.EventHandler(this.bMaybeHD_Click);
      // 
      // bYesHD
      // 
      this.bYesHD.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bYesHD.FlatAppearance.BorderSize = 0;
      this.bYesHD.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bYesHD.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bYesHD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bYesHD.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bYesHD.Location = new System.Drawing.Point(228, 77);
      this.bYesHD.Name = "bYesHD";
      this.bYesHD.Size = new System.Drawing.Size(37, 23);
      this.bYesHD.TabIndex = 19;
      this.bYesHD.UseVisualStyleBackColor = true;
      this.bYesHD.Click += new System.EventHandler(this.bYesHD_Click);
      // 
      // bNoHD
      // 
      this.bNoHD.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bNoHD.FlatAppearance.BorderSize = 0;
      this.bNoHD.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bNoHD.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bNoHD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bNoHD.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bNoHD.Location = new System.Drawing.Point(228, 127);
      this.bNoHD.Name = "bNoHD";
      this.bNoHD.Size = new System.Drawing.Size(37, 23);
      this.bNoHD.TabIndex = 20;
      this.bNoHD.UseVisualStyleBackColor = true;
      this.bNoHD.Click += new System.EventHandler(this.bNoHD_Click);
      // 
      // bMaybeHD
      // 
      this.bMaybeHD.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bMaybeHD.FlatAppearance.BorderSize = 0;
      this.bMaybeHD.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bMaybeHD.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bMaybeHD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bMaybeHD.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bMaybeHD.Location = new System.Drawing.Point(228, 177);
      this.bMaybeHD.Name = "bMaybeHD";
      this.bMaybeHD.Size = new System.Drawing.Size(37, 23);
      this.bMaybeHD.TabIndex = 21;
      this.bMaybeHD.UseVisualStyleBackColor = true;
      this.bMaybeHD.Click += new System.EventHandler(this.bMaybeHD_Click);
      // 
      // WatchHDTvDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_TV_install;
      this.Controls.Add(this.bMaybeHD);
      this.Controls.Add(this.bNoHD);
      this.Controls.Add(this.bYesHD);
      this.Controls.Add(this.rbMaybeHD);
      this.Controls.Add(this.rbNoHD);
      this.Controls.Add(this.rbYesHD);
      this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Name = "WatchHDTvDlg";
      this.Size = new System.Drawing.Size(632, 251);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.rbYesHD, 0);
      this.Controls.SetChildIndex(this.rbNoHD, 0);
      this.Controls.SetChildIndex(this.rbMaybeHD, 0);
      this.Controls.SetChildIndex(this.bYesHD, 0);
      this.Controls.SetChildIndex(this.bNoHD, 0);
      this.Controls.SetChildIndex(this.bMaybeHD, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label rbYesHD;
    private System.Windows.Forms.Label rbNoHD;
    private System.Windows.Forms.Label rbMaybeHD;
    private System.Windows.Forms.Button bYesHD;
    private System.Windows.Forms.Button bNoHD;
    private System.Windows.Forms.Button bMaybeHD;


  }
}