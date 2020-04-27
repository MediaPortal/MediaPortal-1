namespace MediaPortal.DeployTool.Sections
{
  partial class UpgradeDlg
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
      this.rbUpdate = new System.Windows.Forms.Label();
      this.rbFresh = new System.Windows.Forms.Label();
      this.bUpdate = new System.Windows.Forms.Button();
      this.bFresh = new System.Windows.Forms.Button();
      this.labelNote = new System.Windows.Forms.Label();
      this.bReinstall = new System.Windows.Forms.Button();
      this.rbReinstall = new System.Windows.Forms.Label();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Location = new System.Drawing.Point(332, 106);
      this.labelSectionHeader.MaximumSize = new System.Drawing.Size(405, 0);
      this.labelSectionHeader.Size = new System.Drawing.Size(340, 17);
      this.labelSectionHeader.Text = "MediaPortal 1.0.0 (build XXXXX) detected.";
      // 
      // rbUpdate
      // 
      this.rbUpdate.AutoSize = true;
      this.rbUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbUpdate.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbUpdate.ForeColor = System.Drawing.Color.White;
      this.rbUpdate.Location = new System.Drawing.Point(383, 155);
      this.rbUpdate.MaximumSize = new System.Drawing.Size(300, 0);
      this.rbUpdate.Name = "rbUpdate";
      this.rbUpdate.Size = new System.Drawing.Size(205, 13);
      this.rbUpdate.TabIndex = 16;
      this.rbUpdate.Text = "Update current installation to 1.0.1";
      this.rbUpdate.Click += new System.EventHandler(this.bUpdate_Click);
      // 
      // rbFresh
      // 
      this.rbFresh.AutoSize = true;
      this.rbFresh.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbFresh.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbFresh.ForeColor = System.Drawing.Color.White;
      this.rbFresh.Location = new System.Drawing.Point(383, 216);
      this.rbFresh.MaximumSize = new System.Drawing.Size(300, 0);
      this.rbFresh.Name = "rbFresh";
      this.rbFresh.Size = new System.Drawing.Size(75, 13);
      this.rbFresh.TabIndex = 17;
      this.rbFresh.Text = "Fresh install";
      this.rbFresh.Click += new System.EventHandler(this.bFresh_Click);
      // 
      // bUpdate
      // 
      this.bUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bUpdate.FlatAppearance.BorderSize = 0;
      this.bUpdate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bUpdate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bUpdate.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bUpdate.Location = new System.Drawing.Point(339, 150);
      this.bUpdate.Name = "bUpdate";
      this.bUpdate.Size = new System.Drawing.Size(32, 23);
      this.bUpdate.TabIndex = 19;
      this.bUpdate.UseVisualStyleBackColor = true;
      this.bUpdate.Click += new System.EventHandler(this.bUpdate_Click);
      // 
      // bFresh
      // 
      this.bFresh.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bFresh.FlatAppearance.BorderSize = 0;
      this.bFresh.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bFresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bFresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bFresh.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bFresh.Location = new System.Drawing.Point(339, 211);
      this.bFresh.Name = "bFresh";
      this.bFresh.Size = new System.Drawing.Size(32, 23);
      this.bFresh.TabIndex = 20;
      this.bFresh.UseVisualStyleBackColor = true;
      this.bFresh.Click += new System.EventHandler(this.bFresh_Click);
      // 
      // labelNote
      // 
      this.labelNote.AutoSize = true;
      this.labelNote.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.labelNote.ForeColor = System.Drawing.Color.White;
      this.labelNote.Location = new System.Drawing.Point(383, 260);
      this.labelNote.Name = "labelNote";
      this.labelNote.Size = new System.Drawing.Size(317, 26);
      this.labelNote.TabIndex = 21;
      this.labelNote.Text = "NOTE: You cannot upgrade from an existing GIT build,\r\n          those are meant f" +
    "or testing purposes only.";
      // 
      // bReinstall
      // 
      this.bReinstall.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bReinstall.FlatAppearance.BorderSize = 0;
      this.bReinstall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bReinstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bReinstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bReinstall.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bReinstall.Location = new System.Drawing.Point(339, 181);
      this.bReinstall.Name = "bReinstall";
      this.bReinstall.Size = new System.Drawing.Size(32, 23);
      this.bReinstall.TabIndex = 23;
      this.bReinstall.UseVisualStyleBackColor = true;
      this.bReinstall.Click += new System.EventHandler(this.bReinstall_Click);
      // 
      // rbReinstall
      // 
      this.rbReinstall.AutoSize = true;
      this.rbReinstall.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbReinstall.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbReinstall.ForeColor = System.Drawing.Color.White;
      this.rbReinstall.Location = new System.Drawing.Point(383, 186);
      this.rbReinstall.MaximumSize = new System.Drawing.Size(300, 0);
      this.rbReinstall.Name = "rbReinstall";
      this.rbReinstall.Size = new System.Drawing.Size(213, 13);
      this.rbReinstall.TabIndex = 22;
      this.rbReinstall.Text = "Reinstall current installation to 1.0.1";
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.Mediaportal_Box_White;
      this.pictureBox1.Location = new System.Drawing.Point(-50, 50);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(374, 357);
      this.pictureBox1.TabIndex = 24;
      this.pictureBox1.TabStop = false;
      // 
      // UpgradeDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.bReinstall);
      this.Controls.Add(this.rbReinstall);
      this.Controls.Add(this.labelNote);
      this.Controls.Add(this.bFresh);
      this.Controls.Add(this.bUpdate);
      this.Controls.Add(this.rbFresh);
      this.Controls.Add(this.rbUpdate);
      this.Name = "UpgradeDlg";
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.rbUpdate, 0);
      this.Controls.SetChildIndex(this.rbFresh, 0);
      this.Controls.SetChildIndex(this.bUpdate, 0);
      this.Controls.SetChildIndex(this.bFresh, 0);
      this.Controls.SetChildIndex(this.labelNote, 0);
      this.Controls.SetChildIndex(this.rbReinstall, 0);
      this.Controls.SetChildIndex(this.bReinstall, 0);
      this.Controls.SetChildIndex(this.pictureBox1, 0);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label rbUpdate;
    private System.Windows.Forms.Label rbFresh;
    private System.Windows.Forms.Button bUpdate;
    private System.Windows.Forms.Button bFresh;
    private System.Windows.Forms.Label labelNote;
    private System.Windows.Forms.Button bReinstall;
    private System.Windows.Forms.Label rbReinstall;
    private System.Windows.Forms.PictureBox pictureBox1;
  }
}