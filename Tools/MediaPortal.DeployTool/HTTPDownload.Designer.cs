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
        this.labelSourceURL.Font = new System.Drawing.Font("Verdana", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelSourceURL.Location = new System.Drawing.Point(13, 13);
        this.labelSourceURL.Name = "labelSourceURL";
        this.labelSourceURL.Size = new System.Drawing.Size(96, 13);
        this.labelSourceURL.TabIndex = 0;
        this.labelSourceURL.Text = "Download URL:";
        // 
        // labelTargetFile
        // 
        this.labelTargetFile.AutoSize = true;
        this.labelTargetFile.Font = new System.Drawing.Font("Verdana", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelTargetFile.Location = new System.Drawing.Point(13, 57);
        this.labelTargetFile.Name = "labelTargetFile";
        this.labelTargetFile.Size = new System.Drawing.Size(48, 13);
        this.labelTargetFile.TabIndex = 1;
        this.labelTargetFile.Text = "Target:";
        // 
        // labelURL
        // 
        this.labelURL.AutoSize = true;
        this.labelURL.Location = new System.Drawing.Point(13, 35);
        this.labelURL.Name = "labelURL";
        this.labelURL.Size = new System.Drawing.Size(35, 13);
        this.labelURL.TabIndex = 2;
        this.labelURL.Text = "label3";
        // 
        // labelTarget
        // 
        this.labelTarget.AutoSize = true;
        this.labelTarget.Location = new System.Drawing.Point(13, 80);
        this.labelTarget.Name = "labelTarget";
        this.labelTarget.Size = new System.Drawing.Size(35, 13);
        this.labelTarget.TabIndex = 3;
        this.labelTarget.Text = "label3";
        // 
        // progressBar
        // 
        this.progressBar.Location = new System.Drawing.Point(16, 110);
        this.progressBar.Name = "progressBar";
        this.progressBar.Size = new System.Drawing.Size(454, 23);
        this.progressBar.TabIndex = 4;
        // 
        // buttonCancel
        // 
        this.buttonCancel.Location = new System.Drawing.Point(204, 146);
        this.buttonCancel.Name = "buttonCancel";
        this.buttonCancel.Size = new System.Drawing.Size(98, 23);
        this.buttonCancel.TabIndex = 5;
        this.buttonCancel.Text = "Cancel";
        this.buttonCancel.UseVisualStyleBackColor = true;
        this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
        // 
        // HTTPDownload
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(494, 196);
        this.ControlBox = false;
        this.Controls.Add(this.buttonCancel);
        this.Controls.Add(this.progressBar);
        this.Controls.Add(this.labelTarget);
        this.Controls.Add(this.labelURL);
        this.Controls.Add(this.labelTargetFile);
        this.Controls.Add(this.labelSourceURL);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "HTTPDownload";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Downloading file...";
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