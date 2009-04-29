namespace MediaPortal.DeployTool.Sections
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
      this.rbTV2 = new System.Windows.Forms.Label();
      this.rbTV3 = new System.Windows.Forms.Label();
      this.bTVE2 = new System.Windows.Forms.Button();
      this.bTVE3 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Location = new System.Drawing.Point(197, 0);
      this.labelSectionHeader.Size = new System.Drawing.Size(254, 13);
      this.labelSectionHeader.Text = "Which TV-Engine do you want to use ?";
      // 
      // labelTV3
      // 
      this.labelTV3.AutoSize = true;
      this.labelTV3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
      this.labelTV3.ForeColor = System.Drawing.Color.White;
      this.labelTV3.Location = new System.Drawing.Point(197, 77);
      this.labelTV3.MaximumSize = new System.Drawing.Size(475, 0);
      this.labelTV3.Name = "labelTV3";
      this.labelTV3.Size = new System.Drawing.Size(436, 169);
      this.labelTV3.TabIndex = 11;
      this.labelTV3.Text = resources.GetString("labelTV3.Text");
      // 
      // rbTV2
      // 
      this.rbTV2.AutoSize = true;
      this.rbTV2.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbTV2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbTV2.ForeColor = System.Drawing.Color.White;
      this.rbTV2.Location = new System.Drawing.Point(231, 26);
      this.rbTV2.Name = "rbTV2";
      this.rbTV2.Size = new System.Drawing.Size(220, 13);
      this.rbTV2.TabIndex = 22;
      this.rbTV2.Text = "In-build TV-Engine of MediaPortal 1.0";
      this.rbTV2.Click += new System.EventHandler(this.bTVE2_Click);
      // 
      // rbTV3
      // 
      this.rbTV3.AutoSize = true;
      this.rbTV3.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbTV3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbTV3.ForeColor = System.Drawing.Color.White;
      this.rbTV3.Location = new System.Drawing.Point(231, 54);
      this.rbTV3.Name = "rbTV3";
      this.rbTV3.Size = new System.Drawing.Size(158, 13);
      this.rbTV3.TabIndex = 23;
      this.rbTV3.Text = "MediaPortal TV-Server 1.0";
      this.rbTV3.Click += new System.EventHandler(this.bTVE3_Click);
      // 
      // bTVE2
      // 
      this.bTVE2.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bTVE2.FlatAppearance.BorderSize = 0;
      this.bTVE2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bTVE2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bTVE2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bTVE2.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bTVE2.Location = new System.Drawing.Point(188, 21);
      this.bTVE2.Name = "bTVE2";
      this.bTVE2.Size = new System.Drawing.Size(37, 23);
      this.bTVE2.TabIndex = 24;
      this.bTVE2.UseVisualStyleBackColor = true;
      this.bTVE2.Click += new System.EventHandler(this.bTVE2_Click);
      // 
      // bTVE3
      // 
      this.bTVE3.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bTVE3.FlatAppearance.BorderSize = 0;
      this.bTVE3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bTVE3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bTVE3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bTVE3.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bTVE3.Location = new System.Drawing.Point(188, 49);
      this.bTVE3.Name = "bTVE3";
      this.bTVE3.Size = new System.Drawing.Size(37, 23);
      this.bTVE3.TabIndex = 25;
      this.bTVE3.UseVisualStyleBackColor = true;
      this.bTVE3.Click += new System.EventHandler(this.bTVE3_Click);
      // 
      // TvEngineTypeDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_TV_install;
      this.Controls.Add(this.bTVE3);
      this.Controls.Add(this.bTVE2);
      this.Controls.Add(this.rbTV3);
      this.Controls.Add(this.rbTV2);
      this.Controls.Add(this.labelTV3);
      this.Name = "TvEngineTypeDlg";
      this.Size = new System.Drawing.Size(666, 252);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.labelTV3, 0);
      this.Controls.SetChildIndex(this.rbTV2, 0);
      this.Controls.SetChildIndex(this.rbTV3, 0);
      this.Controls.SetChildIndex(this.bTVE2, 0);
      this.Controls.SetChildIndex(this.bTVE3, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelTV3;
    private System.Windows.Forms.Label rbTV2;
    private System.Windows.Forms.Label rbTV3;
    private System.Windows.Forms.Button bTVE2;
    private System.Windows.Forms.Button bTVE3;

  }
}