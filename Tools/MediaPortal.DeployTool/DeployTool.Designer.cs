namespace MediaPortal.DeployTool
{
  partial class DeployTool
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
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.bHelp = new System.Windows.Forms.Button();
      this.bExit = new System.Windows.Forms.Button();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.labelHeading = new System.Windows.Forms.Label();
      this.nextButton = new System.Windows.Forms.Button();
      this.backButton = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(31)))), ((int)(((byte)(73)))));
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.BackColor = System.Drawing.Color.Transparent;
      this.splitContainer1.Panel1.Controls.Add(this.bHelp);
      this.splitContainer1.Panel1.Controls.Add(this.bExit);
      this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
      this.splitContainer1.Panel1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.BackColor = System.Drawing.Color.Transparent;
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
      this.splitContainer1.Size = new System.Drawing.Size(1000, 550);
      this.splitContainer1.SplitterDistance = 70;
      this.splitContainer1.SplitterWidth = 1;
      this.splitContainer1.TabIndex = 0;
      // 
      // bHelp
      // 
      this.bHelp.BackColor = System.Drawing.Color.Transparent;
      this.bHelp.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_Help_button;
      this.bHelp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.bHelp.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bHelp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(31)))), ((int)(((byte)(73)))));
      this.bHelp.FlatAppearance.BorderSize = 0;
      this.bHelp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(31)))), ((int)(((byte)(73)))));
      this.bHelp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(31)))), ((int)(((byte)(73)))));
      this.bHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bHelp.Image = global::MediaPortal.DeployTool.Images.helpIcon;
      this.bHelp.Location = new System.Drawing.Point(945, 12);
      this.bHelp.Name = "bHelp";
      this.bHelp.Size = new System.Drawing.Size(23, 24);
      this.bHelp.TabIndex = 21;
      this.bHelp.UseVisualStyleBackColor = false;
      this.bHelp.Click += new System.EventHandler(this.bHelp_Click);
      // 
      // bExit
      // 
      this.bExit.BackColor = System.Drawing.Color.Transparent;
      this.bExit.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_Exit_button;
      this.bExit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.bExit.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bExit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(31)))), ((int)(((byte)(73)))));
      this.bExit.FlatAppearance.BorderSize = 0;
      this.bExit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(31)))), ((int)(((byte)(73)))));
      this.bExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(31)))), ((int)(((byte)(73)))));
      this.bExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bExit.Image = global::MediaPortal.DeployTool.Images.exitIcon;
      this.bExit.Location = new System.Drawing.Point(970, 11);
      this.bExit.Margin = new System.Windows.Forms.Padding(0);
      this.bExit.Name = "bExit";
      this.bExit.Size = new System.Drawing.Size(23, 24);
      this.bExit.TabIndex = 0;
      this.bExit.UseVisualStyleBackColor = false;
      this.bExit.Click += new System.EventHandler(this.bExit_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
      this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.Background_top;
      this.pictureBox1.Location = new System.Drawing.Point(0, 0);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(1000, 70);
      this.pictureBox1.TabIndex = 20;
      this.pictureBox1.TabStop = false;
      this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DeployTool_MouseDown);
      // 
      // splitContainer2
      // 
      this.splitContainer2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(31)))), ((int)(((byte)(73)))));
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.IsSplitterFixed = true;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Margin = new System.Windows.Forms.Padding(0);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.BackColor = System.Drawing.Color.Transparent;
      this.splitContainer2.Panel1.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.BackColor = System.Drawing.Color.WhiteSmoke;
      this.splitContainer2.Panel2.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_bottom;
      this.splitContainer2.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.splitContainer2.Panel2.Controls.Add(this.labelHeading);
      this.splitContainer2.Panel2.Controls.Add(this.nextButton);
      this.splitContainer2.Panel2.Controls.Add(this.backButton);
      this.splitContainer2.Size = new System.Drawing.Size(1000, 479);
      this.splitContainer2.SplitterDistance = 439;
      this.splitContainer2.SplitterWidth = 1;
      this.splitContainer2.TabIndex = 0;
      // 
      // labelHeading
      // 
      this.labelHeading.BackColor = System.Drawing.Color.Transparent;
      this.labelHeading.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold);
      this.labelHeading.ForeColor = System.Drawing.Color.White;
      this.labelHeading.Location = new System.Drawing.Point(111, 7);
      this.labelHeading.Name = "labelHeading";
      this.labelHeading.Size = new System.Drawing.Size(778, 23);
      this.labelHeading.TabIndex = 3;
      this.labelHeading.Text = "Press the \"Install\" button to perform all necessary actions to install your setup" +
    "";
      this.labelHeading.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // nextButton
      // 
      this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.nextButton.AutoSize = true;
      this.nextButton.BackColor = System.Drawing.Color.Transparent;
      this.nextButton.Cursor = System.Windows.Forms.Cursors.Hand;
      this.nextButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.nextButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.nextButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.nextButton.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.nextButton.ForeColor = System.Drawing.Color.White;
      this.nextButton.Location = new System.Drawing.Point(895, 3);
      this.nextButton.Name = "nextButton";
      this.nextButton.Size = new System.Drawing.Size(93, 34);
      this.nextButton.TabIndex = 2;
      this.nextButton.Text = "next";
      this.nextButton.UseVisualStyleBackColor = false;
      this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
      // 
      // backButton
      // 
      this.backButton.AutoSize = true;
      this.backButton.BackColor = System.Drawing.Color.Transparent;
      this.backButton.Cursor = System.Windows.Forms.Cursors.Hand;
      this.backButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.backButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.backButton.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.backButton.ForeColor = System.Drawing.Color.White;
      this.backButton.Location = new System.Drawing.Point(12, 3);
      this.backButton.Name = "backButton";
      this.backButton.Size = new System.Drawing.Size(99, 34);
      this.backButton.TabIndex = 1;
      this.backButton.Text = "previous";
      this.backButton.UseVisualStyleBackColor = false;
      this.backButton.Click += new System.EventHandler(this.backButton_Click);
      // 
      // DeployTool
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(1000, 550);
      this.Controls.Add(this.splitContainer1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximizeBox = false;
      this.Name = "DeployTool";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MediaPortal Deploy Tool";
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DeployTool_MouseDown);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.splitContainer2.Panel2.ResumeLayout(false);
      this.splitContainer2.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.Button nextButton;
    private System.Windows.Forms.Button backButton;
    private System.Windows.Forms.Button bExit;
    private System.Windows.Forms.Button bHelp;
    private System.Windows.Forms.Label labelHeading;
  }
}

