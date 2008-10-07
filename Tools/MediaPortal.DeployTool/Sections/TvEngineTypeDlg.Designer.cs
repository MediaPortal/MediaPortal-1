namespace MediaPortal.DeployTool
{
  partial class TvEngineTypeDlg
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvEngineTypeDlg));
        this.labelTV3 = new System.Windows.Forms.Label();
        this.imgTVE2 = new System.Windows.Forms.PictureBox();
        this.imgTVE3 = new System.Windows.Forms.PictureBox();
        this.rbTV2 = new System.Windows.Forms.Label();
        this.rbTV3 = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.imgTVE2)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.imgTVE3)).BeginInit();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Location = new System.Drawing.Point(120, 0);
        this.labelSectionHeader.Size = new System.Drawing.Size(254, 13);
        this.labelSectionHeader.Text = "Which TV-Engine do you want to use ?";
        // 
        // labelTV3
        // 
        this.labelTV3.AutoSize = true;
        this.labelTV3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
        this.labelTV3.ForeColor = System.Drawing.Color.White;
        this.labelTV3.Location = new System.Drawing.Point(190, 77);
        this.labelTV3.MaximumSize = new System.Drawing.Size(475, 0);
        this.labelTV3.Name = "labelTV3";
        this.labelTV3.Size = new System.Drawing.Size(436, 169);
        this.labelTV3.TabIndex = 11;
        this.labelTV3.Text = resources.GetString("labelTV3.Text");
        // 
        // imgTVE2
        // 
        this.imgTVE2.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
        this.imgTVE2.Location = new System.Drawing.Point(223, 23);
        this.imgTVE2.Name = "imgTVE2";
        this.imgTVE2.Size = new System.Drawing.Size(21, 21);
        this.imgTVE2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.imgTVE2.TabIndex = 20;
        this.imgTVE2.TabStop = false;
        this.imgTVE2.Click += new System.EventHandler(this.imgTVE2_Click);
        // 
        // imgTVE3
        // 
        this.imgTVE3.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
        this.imgTVE3.Location = new System.Drawing.Point(223, 50);
        this.imgTVE3.Name = "imgTVE3";
        this.imgTVE3.Size = new System.Drawing.Size(21, 21);
        this.imgTVE3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.imgTVE3.TabIndex = 21;
        this.imgTVE3.TabStop = false;
        this.imgTVE3.Click += new System.EventHandler(this.imgTVE3_Click);
        // 
        // rbTV2
        // 
        this.rbTV2.AutoSize = true;
        this.rbTV2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbTV2.ForeColor = System.Drawing.Color.White;
        this.rbTV2.Location = new System.Drawing.Point(251, 26);
        this.rbTV2.Name = "rbTV2";
        this.rbTV2.Size = new System.Drawing.Size(220, 13);
        this.rbTV2.TabIndex = 22;
        this.rbTV2.Text = "In-build TV-Engine of MediaPortal 1.0";
        this.rbTV2.Click += new System.EventHandler(this.imgTVE2_Click);
        // 
        // rbTV3
        // 
        this.rbTV3.AutoSize = true;
        this.rbTV3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbTV3.ForeColor = System.Drawing.Color.White;
        this.rbTV3.Location = new System.Drawing.Point(251, 54);
        this.rbTV3.Name = "rbTV3";
        this.rbTV3.Size = new System.Drawing.Size(158, 13);
        this.rbTV3.TabIndex = 23;
        this.rbTV3.Text = "MediaPortal TV-Server 1.0";
        this.rbTV3.Click += new System.EventHandler(this.imgTVE3_Click);
        // 
        // TvEngineTypeDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_TV_install;
        this.Controls.Add(this.rbTV3);
        this.Controls.Add(this.rbTV2);
        this.Controls.Add(this.imgTVE3);
        this.Controls.Add(this.imgTVE2);
        this.Controls.Add(this.labelTV3);
        this.Name = "TvEngineTypeDlg";
        this.Size = new System.Drawing.Size(666, 252);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.labelTV3, 0);
        this.Controls.SetChildIndex(this.imgTVE2, 0);
        this.Controls.SetChildIndex(this.imgTVE3, 0);
        this.Controls.SetChildIndex(this.rbTV2, 0);
        this.Controls.SetChildIndex(this.rbTV3, 0);
        ((System.ComponentModel.ISupportInitialize)(this.imgTVE2)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.imgTVE3)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelTV3;
    private System.Windows.Forms.PictureBox imgTVE2;
    private System.Windows.Forms.PictureBox imgTVE3;
    private System.Windows.Forms.Label rbTV2;
    private System.Windows.Forms.Label rbTV3;

  }
}
