namespace MpeCore.Dialogs
{
  partial class DownloadInfo
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
      this.btnCancel = new System.Windows.Forms.Button();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.panel3 = new System.Windows.Forms.Panel();
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.Location = new System.Drawing.Point(200, 31);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.CancelClick);
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(4, 4);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(435, 22);
      this.progressBar1.TabIndex = 2;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.btnCancel);
      this.panel3.Controls.Add(this.progressBar1);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel3.Location = new System.Drawing.Point(0, 151);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(444, 60);
      this.panel3.TabIndex = 7;
      // 
      // listBox1
      // 
      this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBox1.FormattingEnabled = true;
      this.listBox1.Location = new System.Drawing.Point(0, 0);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(444, 151);
      this.listBox1.TabIndex = 0;
      // 
      // DownloadInfo
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(444, 211);
      this.ControlBox = false;
      this.Controls.Add(this.listBox1);
      this.Controls.Add(this.panel3);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MinimizeBox = false;
      this.Name = "DownloadInfo";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Download Update Info";
      this.Shown += new System.EventHandler(this.DownloadInfo_Shown);
      this.panel3.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.ListBox listBox1;
  }
}