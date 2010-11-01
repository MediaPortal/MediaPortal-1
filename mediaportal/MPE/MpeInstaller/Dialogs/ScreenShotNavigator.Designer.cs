namespace MpeInstaller.Dialogs
{
  partial class ScreenShotNavigator
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
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.btn_next = new System.Windows.Forms.Button();
      this.btn_prev = new System.Windows.Forms.Button();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBox
      // 
      this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox.BackColor = System.Drawing.Color.Black;
      this.pictureBox.ImageLocation = "";
      this.pictureBox.Location = new System.Drawing.Point(-1, -1);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(700, 387);
      this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox.TabIndex = 0;
      this.pictureBox.TabStop = false;
      // 
      // btn_next
      // 
      this.btn_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_next.Location = new System.Drawing.Point(609, 392);
      this.btn_next.Name = "btn_next";
      this.btn_next.Size = new System.Drawing.Size(75, 23);
      this.btn_next.TabIndex = 1;
      this.btn_next.Text = ">>";
      this.btn_next.UseVisualStyleBackColor = true;
      this.btn_next.Click += new System.EventHandler(this.btn_next_Click);
      // 
      // btn_prev
      // 
      this.btn_prev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btn_prev.Location = new System.Drawing.Point(11, 392);
      this.btn_prev.Name = "btn_prev";
      this.btn_prev.Size = new System.Drawing.Size(75, 23);
      this.btn_prev.TabIndex = 2;
      this.btn_prev.Text = "<<";
      this.btn_prev.UseVisualStyleBackColor = true;
      this.btn_prev.Click += new System.EventHandler(this.btn_prev_Click);
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(92, 392);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(511, 23);
      this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
      this.progressBar1.TabIndex = 3;
      // 
      // ScreenShotNavigator
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.BackColor = System.Drawing.Color.Black;
      this.ClientSize = new System.Drawing.Size(697, 420);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.btn_prev);
      this.Controls.Add(this.btn_next);
      this.Controls.Add(this.pictureBox);
      this.MinimizeBox = false;
      this.Name = "ScreenShotNavigator";
      this.Text = "ScreenShotNavigator";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScreenShotNavigator_FormClosing);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.Button btn_next;
    private System.Windows.Forms.Button btn_prev;
    private System.Windows.Forms.ProgressBar progressBar1;
  }
}