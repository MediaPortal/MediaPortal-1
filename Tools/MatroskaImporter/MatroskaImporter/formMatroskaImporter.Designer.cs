namespace MatroskaImporter
{
  partial class MatroskaImporter
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btnLookup = new System.Windows.Forms.Button();
      this.tvTagRecs = new System.Windows.Forms.TreeView();
      this.label1 = new System.Windows.Forms.Label();
      this.tvDbRecs = new System.Windows.Forms.TreeView();
      this.btnImport = new System.Windows.Forms.Button();
      this.cbRecPaths = new System.Windows.Forms.ComboBox();
      this.btnExit = new System.Windows.Forms.Button();
      this.lblTags = new System.Windows.Forms.Label();
      this.lblDbRecs = new System.Windows.Forms.Label();
      this.cbConfirmImport = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // btnLookup
      // 
      this.btnLookup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLookup.Enabled = false;
      this.btnLookup.Location = new System.Drawing.Point(230, 518);
      this.btnLookup.Name = "btnLookup";
      this.btnLookup.Size = new System.Drawing.Size(115, 21);
      this.btnLookup.TabIndex = 1;
      this.btnLookup.Text = "Lookup";
      this.btnLookup.UseVisualStyleBackColor = true;
      this.btnLookup.Click += new System.EventHandler(this.btnLookup_Click);
      // 
      // tvTagRecs
      // 
      this.tvTagRecs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tvTagRecs.Location = new System.Drawing.Point(12, 30);
      this.tvTagRecs.Name = "tvTagRecs";
      this.tvTagRecs.ShowNodeToolTips = true;
      this.tvTagRecs.Size = new System.Drawing.Size(333, 459);
      this.tvTagRecs.TabIndex = 2;
      this.tvTagRecs.TabStop = false;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 496);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(194, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Recording path as specified in SetupTv";
      // 
      // tvDbRecs
      // 
      this.tvDbRecs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tvDbRecs.Location = new System.Drawing.Point(367, 30);
      this.tvDbRecs.Name = "tvDbRecs";
      this.tvDbRecs.ShowNodeToolTips = true;
      this.tvDbRecs.Size = new System.Drawing.Size(333, 459);
      this.tvDbRecs.TabIndex = 4;
      this.tvDbRecs.TabStop = false;
      // 
      // btnImport
      // 
      this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnImport.Enabled = false;
      this.btnImport.Location = new System.Drawing.Point(367, 518);
      this.btnImport.Name = "btnImport";
      this.btnImport.Size = new System.Drawing.Size(115, 21);
      this.btnImport.TabIndex = 3;
      this.btnImport.Text = "Import";
      this.btnImport.UseVisualStyleBackColor = true;
      this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
      // 
      // cbRecPaths
      // 
      this.cbRecPaths.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbRecPaths.FormattingEnabled = true;
      this.cbRecPaths.Location = new System.Drawing.Point(15, 519);
      this.cbRecPaths.Name = "cbRecPaths";
      this.cbRecPaths.Size = new System.Drawing.Size(191, 21);
      this.cbRecPaths.TabIndex = 0;
      this.cbRecPaths.TextUpdate += new System.EventHandler(this.cbRecPaths_TextUpdate);
      // 
      // btnExit
      // 
      this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnExit.Location = new System.Drawing.Point(585, 518);
      this.btnExit.Name = "btnExit";
      this.btnExit.Size = new System.Drawing.Size(115, 21);
      this.btnExit.TabIndex = 4;
      this.btnExit.Text = "E&xit";
      this.btnExit.UseVisualStyleBackColor = true;
      this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
      // 
      // lblTags
      // 
      this.lblTags.AutoSize = true;
      this.lblTags.Location = new System.Drawing.Point(12, 14);
      this.lblTags.Name = "lblTags";
      this.lblTags.Size = new System.Drawing.Size(107, 13);
      this.lblTags.TabIndex = 8;
      this.lblTags.Text = "Recordings from tags";
      // 
      // lblDbRecs
      // 
      this.lblDbRecs.AutoSize = true;
      this.lblDbRecs.Location = new System.Drawing.Point(364, 14);
      this.lblDbRecs.Name = "lblDbRecs";
      this.lblDbRecs.Size = new System.Drawing.Size(131, 13);
      this.lblDbRecs.TabIndex = 9;
      this.lblDbRecs.Text = "Recordings from database";
      // 
      // cbConfirmImport
      // 
      this.cbConfirmImport.AutoSize = true;
      this.cbConfirmImport.Checked = true;
      this.cbConfirmImport.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbConfirmImport.Location = new System.Drawing.Point(367, 495);
      this.cbConfirmImport.Name = "cbConfirmImport";
      this.cbConfirmImport.Size = new System.Drawing.Size(125, 17);
      this.cbConfirmImport.TabIndex = 2;
      this.cbConfirmImport.Text = "Ask for every import?";
      this.cbConfirmImport.UseVisualStyleBackColor = true;
      // 
      // MatroskaImporter
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(712, 552);
      this.Controls.Add(this.cbConfirmImport);
      this.Controls.Add(this.lblDbRecs);
      this.Controls.Add(this.lblTags);
      this.Controls.Add(this.btnExit);
      this.Controls.Add(this.cbRecPaths);
      this.Controls.Add(this.btnImport);
      this.Controls.Add(this.tvDbRecs);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.tvTagRecs);
      this.Controls.Add(this.btnLookup);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "MatroskaImporter";
      this.Text = "TV Service recordings importer";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnLookup;
    private System.Windows.Forms.TreeView tvTagRecs;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TreeView tvDbRecs;
    private System.Windows.Forms.Button btnImport;
    private System.Windows.Forms.ComboBox cbRecPaths;
    private System.Windows.Forms.Button btnExit;
    private System.Windows.Forms.Label lblTags;
    private System.Windows.Forms.Label lblDbRecs;
    private System.Windows.Forms.CheckBox cbConfirmImport;
  }
}

