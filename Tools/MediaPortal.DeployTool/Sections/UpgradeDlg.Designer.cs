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
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelSectionHeader.Location = new System.Drawing.Point(197, 37);
      this.labelSectionHeader.MaximumSize = new System.Drawing.Size(405, 0);
      this.labelSectionHeader.Size = new System.Drawing.Size(316, 16);
      this.labelSectionHeader.Text = "MediaPortal 1.0.0 (build XXXXX) detected.";
      // 
      // rbUpdate
      // 
      this.rbUpdate.AutoSize = true;
      this.rbUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbUpdate.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbUpdate.ForeColor = System.Drawing.Color.White;
      this.rbUpdate.Location = new System.Drawing.Point(244, 90);
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
      this.rbFresh.Location = new System.Drawing.Point(244, 151);
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
      this.bUpdate.Location = new System.Drawing.Point(200, 85);
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
      this.bFresh.Location = new System.Drawing.Point(200, 146);
      this.bFresh.Name = "bFresh";
      this.bFresh.Size = new System.Drawing.Size(32, 23);
      this.bFresh.TabIndex = 20;
      this.bFresh.UseVisualStyleBackColor = true;
      this.bFresh.Click += new System.EventHandler(this.bFresh_Click);
      // 
      // labelNote
      // 
      this.labelNote.AutoSize = true;
      this.labelNote.ForeColor = System.Drawing.Color.White;
      this.labelNote.Location = new System.Drawing.Point(244, 195);
      this.labelNote.Name = "labelNote";
      this.labelNote.Size = new System.Drawing.Size(269, 26);
      this.labelNote.TabIndex = 21;
      this.labelNote.Text = "NOTE: You cannot upgrade from an existing SVN build,\r\n             those are mean" +
          "t for testing purposes only.";
      // 
      // UpgradeDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_TV_install;
      this.Controls.Add(this.labelNote);
      this.Controls.Add(this.bFresh);
      this.Controls.Add(this.bUpdate);
      this.Controls.Add(this.rbFresh);
      this.Controls.Add(this.rbUpdate);
      this.Name = "UpgradeDlg";
      this.Size = new System.Drawing.Size(666, 250);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.rbUpdate, 0);
      this.Controls.SetChildIndex(this.rbFresh, 0);
      this.Controls.SetChildIndex(this.bUpdate, 0);
      this.Controls.SetChildIndex(this.bFresh, 0);
      this.Controls.SetChildIndex(this.labelNote, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label rbUpdate;
    private System.Windows.Forms.Label rbFresh;
    private System.Windows.Forms.Button bUpdate;
    private System.Windows.Forms.Button bFresh;
    private System.Windows.Forms.Label labelNote;


  }
}