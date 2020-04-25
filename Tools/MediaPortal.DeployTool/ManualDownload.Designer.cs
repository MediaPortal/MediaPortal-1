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
      this.labelHeading = new System.Windows.Forms.Label();
      this.labelFile = new System.Windows.Forms.Label();
      this.labelTargetFile = new System.Windows.Forms.Label();
      this.linkURL = new System.Windows.Forms.LinkLabel();
      this.labelDir = new System.Windows.Forms.Label();
      this.labelTargetDir = new System.Windows.Forms.Label();
      this.linkDir = new System.Windows.Forms.LinkLabel();
      this.labelDesc = new System.Windows.Forms.Label();
      this.buttonContinue = new System.Windows.Forms.Button();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.SuspendLayout();
      // 
      // labelHeading
      // 
      this.labelHeading.AutoSize = true;
      this.labelHeading.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold);
      this.labelHeading.ForeColor = System.Drawing.Color.White;
      this.labelHeading.Location = new System.Drawing.Point(15, 13);
      this.labelHeading.Name = "labelHeading";
      this.labelHeading.Size = new System.Drawing.Size(398, 17);
      this.labelHeading.TabIndex = 0;
      this.labelHeading.Text = "You have to manually download the following file:";
      // 
      // labelFile
      // 
      this.labelFile.AutoSize = true;
      this.labelFile.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold);
      this.labelFile.ForeColor = System.Drawing.Color.White;
      this.labelFile.Location = new System.Drawing.Point(15, 44);
      this.labelFile.Name = "labelFile";
      this.labelFile.Size = new System.Drawing.Size(82, 17);
      this.labelFile.TabIndex = 1;
      this.labelFile.Text = "Filename:";
      // 
      // labelTargetFile
      // 
      this.labelTargetFile.AutoSize = true;
      this.labelTargetFile.ForeColor = System.Drawing.Color.White;
      this.labelTargetFile.Location = new System.Drawing.Point(111, 48);
      this.labelTargetFile.Name = "labelTargetFile";
      this.labelTargetFile.Size = new System.Drawing.Size(41, 13);
      this.labelTargetFile.TabIndex = 2;
      this.labelTargetFile.Text = "label3";
      // 
      // linkURL
      // 
      this.linkURL.AutoSize = true;
      this.linkURL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.linkURL.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.linkURL.Location = new System.Drawing.Point(111, 66);
      this.linkURL.Name = "linkURL";
      this.linkURL.Size = new System.Drawing.Size(98, 13);
      this.linkURL.TabIndex = 3;
      this.linkURL.TabStop = true;
      this.linkURL.Text = "Download link";
      this.linkURL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkURL_LinkClicked);
      // 
      // labelDir
      // 
      this.labelDir.AutoSize = true;
      this.labelDir.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold);
      this.labelDir.ForeColor = System.Drawing.Color.White;
      this.labelDir.Location = new System.Drawing.Point(15, 82);
      this.labelDir.Name = "labelDir";
      this.labelDir.Size = new System.Drawing.Size(91, 17);
      this.labelDir.TabIndex = 4;
      this.labelDir.Text = "Target dir:";
      // 
      // labelTargetDir
      // 
      this.labelTargetDir.AutoSize = true;
      this.labelTargetDir.ForeColor = System.Drawing.Color.White;
      this.labelTargetDir.Location = new System.Drawing.Point(111, 86);
      this.labelTargetDir.Name = "labelTargetDir";
      this.labelTargetDir.Size = new System.Drawing.Size(41, 13);
      this.labelTargetDir.TabIndex = 5;
      this.labelTargetDir.Text = "label3";
      // 
      // linkDir
      // 
      this.linkDir.AutoSize = true;
      this.linkDir.Cursor = System.Windows.Forms.Cursors.Hand;
      this.linkDir.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.linkDir.Location = new System.Drawing.Point(111, 105);
      this.linkDir.Name = "linkDir";
      this.linkDir.Size = new System.Drawing.Size(104, 13);
      this.linkDir.TabIndex = 6;
      this.linkDir.TabStop = true;
      this.linkDir.Text = "Open directory";
      this.linkDir.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkDir_LinkClicked);
      // 
      // labelDesc
      // 
      this.labelDesc.AutoSize = true;
      this.labelDesc.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelDesc.ForeColor = System.Drawing.Color.White;
      this.labelDesc.Location = new System.Drawing.Point(15, 131);
      this.labelDesc.Name = "labelDesc";
      this.labelDesc.Size = new System.Drawing.Size(413, 13);
      this.labelDesc.TabIndex = 7;
      this.labelDesc.Text = "Hit \"Continue\", when you have finished downloading the requested file.";
      // 
      // buttonContinue
      // 
      this.buttonContinue.Cursor = System.Windows.Forms.Cursors.Hand;
      this.buttonContinue.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonContinue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonContinue.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.buttonContinue.ForeColor = System.Drawing.Color.White;
      this.buttonContinue.Location = new System.Drawing.Point(279, 162);
      this.buttonContinue.Name = "buttonContinue";
      this.buttonContinue.Size = new System.Drawing.Size(115, 30);
      this.buttonContinue.TabIndex = 8;
      this.buttonContinue.Text = "Continue";
      this.buttonContinue.UseVisualStyleBackColor = true;
      this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
      // 
      // ManualDownload
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.AutoSize = true;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(111)))), ((int)(((byte)(152)))));
      this.ClientSize = new System.Drawing.Size(672, 200);
      this.Controls.Add(this.buttonContinue);
      this.Controls.Add(this.labelDesc);
      this.Controls.Add(this.linkDir);
      this.Controls.Add(this.labelTargetDir);
      this.Controls.Add(this.labelDir);
      this.Controls.Add(this.linkURL);
      this.Controls.Add(this.labelTargetFile);
      this.Controls.Add(this.labelFile);
      this.Controls.Add(this.labelHeading);
      this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ManualDownload";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Manual download";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.Label labelFile;
    private System.Windows.Forms.Label labelTargetFile;
    private System.Windows.Forms.LinkLabel linkURL;
    private System.Windows.Forms.Label labelDir;
    private System.Windows.Forms.Label labelTargetDir;
    private System.Windows.Forms.LinkLabel linkDir;
    private System.Windows.Forms.Label labelDesc;
    private System.Windows.Forms.Button buttonContinue;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
  }
}