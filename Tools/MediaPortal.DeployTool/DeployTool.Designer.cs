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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeployTool));
        this.splitContainer1 = new System.Windows.Forms.SplitContainer();
        this.splitContainer2 = new System.Windows.Forms.SplitContainer();
        this.nextButton = new System.Windows.Forms.Button();
        this.backButton = new System.Windows.Forms.Button();
        this.pictureBox2 = new System.Windows.Forms.PictureBox();
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        this.splitContainer1.Panel1.SuspendLayout();
        this.splitContainer1.Panel2.SuspendLayout();
        this.splitContainer1.SuspendLayout();
        this.splitContainer2.Panel2.SuspendLayout();
        this.splitContainer2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.SuspendLayout();
        // 
        // splitContainer1
        // 
        this.splitContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(198)))), ((int)(((byte)(198)))));
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
        this.splitContainer1.Panel1.Controls.Add(this.pictureBox2);
        this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
        this.splitContainer1.Panel1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        // 
        // splitContainer1.Panel2
        // 
        this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
        this.splitContainer1.Size = new System.Drawing.Size(666, 416);
        this.splitContainer1.SplitterDistance = 122;
        this.splitContainer1.SplitterWidth = 1;
        this.splitContainer1.TabIndex = 3;
        // 
        // splitContainer2
        // 
        this.splitContainer2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
        this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitContainer2.IsSplitterFixed = true;
        this.splitContainer2.Location = new System.Drawing.Point(0, 0);
        this.splitContainer2.Margin = new System.Windows.Forms.Padding(0);
        this.splitContainer2.Name = "splitContainer2";
        this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // splitContainer2.Panel1
        // 
        this.splitContainer2.Panel1.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
        this.splitContainer2.Panel1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.splitContainer2.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel1_Paint);
        // 
        // splitContainer2.Panel2
        // 
        this.splitContainer2.Panel2.BackColor = System.Drawing.Color.WhiteSmoke;
        this.splitContainer2.Panel2.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_bottom;
        this.splitContainer2.Panel2.Controls.Add(this.nextButton);
        this.splitContainer2.Panel2.Controls.Add(this.backButton);
        this.splitContainer2.Panel2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.splitContainer2.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel2_Paint);
        this.splitContainer2.Size = new System.Drawing.Size(666, 293);
        this.splitContainer2.SplitterDistance = 250;
        this.splitContainer2.SplitterWidth = 1;
        this.splitContainer2.TabIndex = 0;
        // 
        // nextButton
        // 
        this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.nextButton.AutoSize = true;
        this.nextButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.nextButton.Location = new System.Drawing.Point(561, 8);
        this.nextButton.Name = "nextButton";
        this.nextButton.Size = new System.Drawing.Size(75, 25);
        this.nextButton.TabIndex = 1;
        this.nextButton.Text = "next";
        this.nextButton.UseVisualStyleBackColor = true;
        this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
        // 
        // backButton
        // 
        this.backButton.AutoSize = true;
        this.backButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.backButton.Location = new System.Drawing.Point(446, 8);
        this.backButton.Name = "backButton";
        this.backButton.Size = new System.Drawing.Size(78, 24);
        this.backButton.TabIndex = 0;
        this.backButton.Text = "previous";
        this.backButton.UseVisualStyleBackColor = true;
        this.backButton.Click += new System.EventHandler(this.backButton_Click);
        // 
        // pictureBox2
        // 
        this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
        this.pictureBox2.Image = global::MediaPortal.DeployTool.Images.exit41;
        this.pictureBox2.Location = new System.Drawing.Point(623, 12);
        this.pictureBox2.Name = "pictureBox2";
        this.pictureBox2.Size = new System.Drawing.Size(31, 30);
        this.pictureBox2.TabIndex = 4;
        this.pictureBox2.TabStop = false;
        this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
        // 
        // pictureBox1
        // 
        this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(198)))), ((int)(((byte)(198)))), ((int)(((byte)(198)))));
        this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.Background_top;
        this.pictureBox1.Location = new System.Drawing.Point(0, 0);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(666, 122);
        this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.pictureBox1.TabIndex = 3;
        this.pictureBox1.TabStop = false;
        this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
        // 
        // DeployTool
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(666, 416);
        this.Controls.Add(this.splitContainer1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.Name = "DeployTool";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "MediaPortal Deploy Tool";
        this.splitContainer1.Panel1.ResumeLayout(false);
        this.splitContainer1.Panel1.PerformLayout();
        this.splitContainer1.Panel2.ResumeLayout(false);
        this.splitContainer1.ResumeLayout(false);
        this.splitContainer2.Panel2.ResumeLayout(false);
        this.splitContainer2.Panel2.PerformLayout();
        this.splitContainer2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.Button nextButton;
      private System.Windows.Forms.Button backButton;
      private System.Windows.Forms.PictureBox pictureBox2;


  }
}

