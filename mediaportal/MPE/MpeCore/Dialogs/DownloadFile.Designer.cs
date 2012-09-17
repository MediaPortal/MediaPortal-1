namespace MpeCore.Dialogs
{
    partial class DownloadFile
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
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.button1 = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.panelBottom = new System.Windows.Forms.Panel();
      this.panelTop = new System.Windows.Forms.Panel();
      this.panelBottom.SuspendLayout();
      this.panelTop.SuspendLayout();
      this.SuspendLayout();
      // 
      // progressBar1
      // 
      this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.progressBar1.Location = new System.Drawing.Point(3, 27);
      this.progressBar1.MinimumSize = new System.Drawing.Size(0, 23);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(514, 23);
      this.progressBar1.TabIndex = 0;
      // 
      // button1
      // 
      this.button1.AutoSize = true;
      this.button1.Location = new System.Drawing.Point(219, 6);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "Cancel";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // label1
      // 
      this.label1.BackColor = System.Drawing.SystemColors.Control;
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 3);
      this.label1.MinimumSize = new System.Drawing.Size(0, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(514, 24);
      this.label1.TabIndex = 2;
      this.label1.Text = "Download starting ....";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // panelBottom
      // 
      this.panelBottom.AutoSize = true;
      this.panelBottom.Controls.Add(this.button1);
      this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panelBottom.Location = new System.Drawing.Point(0, 53);
      this.panelBottom.MinimumSize = new System.Drawing.Size(0, 34);
      this.panelBottom.Name = "panelBottom";
      this.panelBottom.Size = new System.Drawing.Size(520, 34);
      this.panelBottom.TabIndex = 3;
      // 
      // panelTop
      // 
      this.panelTop.AutoSize = true;
      this.panelTop.Controls.Add(this.label1);
      this.panelTop.Controls.Add(this.progressBar1);
      this.panelTop.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelTop.Location = new System.Drawing.Point(0, 0);
      this.panelTop.MinimumSize = new System.Drawing.Size(0, 53);
      this.panelTop.Name = "panelTop";
      this.panelTop.Padding = new System.Windows.Forms.Padding(3);
      this.panelTop.Size = new System.Drawing.Size(520, 53);
      this.panelTop.TabIndex = 4;
      // 
      // DownloadFile
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.AutoSize = true;
      this.ClientSize = new System.Drawing.Size(520, 87);
      this.ControlBox = false;
      this.Controls.Add(this.panelTop);
      this.Controls.Add(this.panelBottom);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DownloadFile";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Download";
      this.Shown += new System.EventHandler(this.DownloadFile_Shown);
      this.panelBottom.ResumeLayout(false);
      this.panelBottom.PerformLayout();
      this.panelTop.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
    }
}