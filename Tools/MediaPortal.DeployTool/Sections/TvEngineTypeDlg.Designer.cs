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
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        this.rbTV2 = new System.Windows.Forms.RadioButton();
        this.rbTV3 = new System.Windows.Forms.RadioButton();
        this.labelTV3 = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Location = new System.Drawing.Point(13, 0);
        this.labelSectionHeader.Size = new System.Drawing.Size(229, 13);
        this.labelSectionHeader.Text = "Which TV-Engine do you want to use ?";
        // 
        // pictureBox1
        // 
        this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.MePo_tv;
        this.pictureBox1.Location = new System.Drawing.Point(16, 36);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(69, 82);
        this.pictureBox1.TabIndex = 4;
        this.pictureBox1.TabStop = false;
        // 
        // rbTV2
        // 
        this.rbTV2.AutoSize = true;
        this.rbTV2.Location = new System.Drawing.Point(105, 16);
        this.rbTV2.Name = "rbTV2";
        this.rbTV2.Size = new System.Drawing.Size(201, 17);
        this.rbTV2.TabIndex = 9;
        this.rbTV2.TabStop = true;
        this.rbTV2.Text = "In-build TV-Engine of MediaPortal 1.0";
        this.rbTV2.UseVisualStyleBackColor = true;
        // 
        // rbTV3
        // 
        this.rbTV3.AutoSize = true;
        this.rbTV3.Location = new System.Drawing.Point(105, 49);
        this.rbTV3.Name = "rbTV3";
        this.rbTV3.Size = new System.Drawing.Size(150, 17);
        this.rbTV3.TabIndex = 10;
        this.rbTV3.TabStop = true;
        this.rbTV3.Text = "MediaPortal TV-Server 1.0";
        this.rbTV3.UseVisualStyleBackColor = true;
        // 
        // labelTV3
        // 
        this.labelTV3.AutoSize = true;
        this.labelTV3.Location = new System.Drawing.Point(122, 78);
        this.labelTV3.Name = "labelTV3";
        this.labelTV3.Size = new System.Drawing.Size(444, 169);
        this.labelTV3.TabIndex = 11;
        this.labelTV3.Text = resources.GetString("labelTV3.Text");
        // 
        // TvEngineTypeDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.labelTV3);
        this.Controls.Add(this.rbTV3);
        this.Controls.Add(this.rbTV2);
        this.Controls.Add(this.pictureBox1);
        this.Name = "TvEngineTypeDlg";
        this.Size = new System.Drawing.Size(542, 266);
        this.Controls.SetChildIndex(this.pictureBox1, 0);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.rbTV2, 0);
        this.Controls.SetChildIndex(this.rbTV3, 0);
        this.Controls.SetChildIndex(this.labelTV3, 0);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

      private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.RadioButton rbTV2;
    private System.Windows.Forms.RadioButton rbTV3;
      private System.Windows.Forms.Label labelTV3;

  }
}
