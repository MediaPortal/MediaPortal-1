namespace MediaPortal.DeployTool
{
  partial class ManualDownload
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
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.labelTargetFile = new System.Windows.Forms.Label();
      this.linkURL = new System.Windows.Forms.LinkLabel();
      this.label3 = new System.Windows.Forms.Label();
      this.labelTargetDir = new System.Windows.Forms.Label();
      this.linkDir = new System.Windows.Forms.LinkLabel();
      this.label4 = new System.Windows.Forms.Label();
      this.buttonContinue = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(13, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(284, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "You have to manually download the following file";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(13, 38);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(61, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Filename:";
      // 
      // labelTargetFile
      // 
      this.labelTargetFile.AutoSize = true;
      this.labelTargetFile.Location = new System.Drawing.Point(78, 39);
      this.labelTargetFile.Name = "labelTargetFile";
      this.labelTargetFile.Size = new System.Drawing.Size(35, 13);
      this.labelTargetFile.TabIndex = 2;
      this.labelTargetFile.Text = "label3";
      // 
      // linkURL
      // 
      this.linkURL.AutoSize = true;
      this.linkURL.Location = new System.Drawing.Point(81, 56);
      this.linkURL.Name = "linkURL";
      this.linkURL.Size = new System.Drawing.Size(23, 13);
      this.linkURL.TabIndex = 3;
      this.linkURL.TabStop = true;
      this.linkURL.Text = "link";
      this.linkURL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkURL_LinkClicked);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(13, 76);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(101, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Target directory:";
      // 
      // labelTargetDir
      // 
      this.labelTargetDir.AutoSize = true;
      this.labelTargetDir.Location = new System.Drawing.Point(116, 77);
      this.labelTargetDir.Name = "labelTargetDir";
      this.labelTargetDir.Size = new System.Drawing.Size(35, 13);
      this.labelTargetDir.TabIndex = 5;
      this.labelTargetDir.Text = "label3";
      // 
      // linkDir
      // 
      this.linkDir.AutoSize = true;
      this.linkDir.Location = new System.Drawing.Point(118, 95);
      this.linkDir.Name = "linkDir";
      this.linkDir.Size = new System.Drawing.Size(76, 13);
      this.linkDir.TabIndex = 6;
      this.linkDir.TabStop = true;
      this.linkDir.Text = "Open directory";
      this.linkDir.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkDir_LinkClicked);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(13, 131);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(222, 13);
      this.label4.TabIndex = 7;
      this.label4.Text = "If you finished the download press \"Continue\"";
      // 
      // buttonContinue
      // 
      this.buttonContinue.Location = new System.Drawing.Point(232, 162);
      this.buttonContinue.Name = "buttonContinue";
      this.buttonContinue.Size = new System.Drawing.Size(75, 23);
      this.buttonContinue.TabIndex = 8;
      this.buttonContinue.Text = "Continue";
      this.buttonContinue.UseVisualStyleBackColor = true;
      this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
      // 
      // ManualDownload
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(576, 200);
      this.Controls.Add(this.buttonContinue);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.linkDir);
      this.Controls.Add(this.labelTargetDir);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.linkURL);
      this.Controls.Add(this.labelTargetFile);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "ManualDownload";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Manual download";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label labelTargetFile;
    private System.Windows.Forms.LinkLabel linkURL;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label labelTargetDir;
    private System.Windows.Forms.LinkLabel linkDir;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button buttonContinue;
  }
}