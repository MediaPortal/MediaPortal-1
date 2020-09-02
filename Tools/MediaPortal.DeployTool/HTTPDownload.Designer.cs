namespace MediaPortal.DeployTool
{
  partial class HTTPDownload
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
      this.labelSourceURL = new System.Windows.Forms.Label();
      this.labelTargetFile = new System.Windows.Forms.Label();
      this.labelURL = new System.Windows.Forms.Label();
      this.labelTarget = new System.Windows.Forms.Label();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // labelSourceURL
      // 
      this.labelSourceURL.AutoSize = true;
      this.labelSourceURL.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelSourceURL.ForeColor = System.Drawing.Color.White;
      this.labelSourceURL.Location = new System.Drawing.Point(15, 13);
      this.labelSourceURL.Name = "labelSourceURL";
      this.labelSourceURL.Size = new System.Drawing.Size(128, 17);
      this.labelSourceURL.TabIndex = 0;
      this.labelSourceURL.Text = "Download URL:";
      // 
      // labelTargetFile
      // 
      this.labelTargetFile.AutoSize = true;
      this.labelTargetFile.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelTargetFile.ForeColor = System.Drawing.Color.White;
      this.labelTargetFile.Location = new System.Drawing.Point(15, 57);
      this.labelTargetFile.Name = "labelTargetFile";
      this.labelTargetFile.Size = new System.Drawing.Size(65, 17);
      this.labelTargetFile.TabIndex = 1;
      this.labelTargetFile.Text = "Target:";
      // 
      // labelURL
      // 
      this.labelURL.AutoSize = true;
      this.labelURL.ForeColor = System.Drawing.Color.White;
      this.labelURL.Location = new System.Drawing.Point(15, 35);
      this.labelURL.Name = "labelURL";
      this.labelURL.Size = new System.Drawing.Size(41, 13);
      this.labelURL.TabIndex = 2;
      this.labelURL.Text = "label3";
      // 
      // labelTarget
      // 
      this.labelTarget.AutoSize = true;
      this.labelTarget.ForeColor = System.Drawing.Color.White;
      this.labelTarget.Location = new System.Drawing.Point(15, 80);
      this.labelTarget.Name = "labelTarget";
      this.labelTarget.Size = new System.Drawing.Size(41, 13);
      this.labelTarget.TabIndex = 3;
      this.labelTarget.Text = "label3";
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(17, 110);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(543, 23);
      this.progressBar.TabIndex = 4;
      // 
      // buttonCancel
      // 
      this.buttonCancel.Cursor = System.Windows.Forms.Cursors.Hand;
      this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonCancel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.buttonCancel.ForeColor = System.Drawing.Color.White;
      this.buttonCancel.Location = new System.Drawing.Point(231, 151);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(115, 30);
      this.buttonCancel.TabIndex = 5;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // HTTPDownload
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(111)))), ((int)(((byte)(152)))));
      this.ClientSize = new System.Drawing.Size(576, 196);
      this.ControlBox = false;
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.progressBar);
      this.Controls.Add(this.labelTarget);
      this.Controls.Add(this.labelURL);
      this.Controls.Add(this.labelTargetFile);
      this.Controls.Add(this.labelSourceURL);
      this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "HTTPDownload";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Downloading file...";
      this.Load += new System.EventHandler(this.HTTPDownload_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelSourceURL;
    private System.Windows.Forms.Label labelTargetFile;
    private System.Windows.Forms.Label labelURL;
    private System.Windows.Forms.Label labelTarget;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.Button buttonCancel;
  }
}