namespace MpeInstaller.Controls
{
    partial class ExtensionListControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
          this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
          this.SuspendLayout();
          // 
          // flowLayoutPanel1
          // 
          this.flowLayoutPanel1.AutoScroll = true;
          this.flowLayoutPanel1.BackColor = System.Drawing.Color.White;
          this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
          this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
          this.flowLayoutPanel1.ForeColor = System.Drawing.SystemColors.ButtonFace;
          this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
          this.flowLayoutPanel1.Name = "flowLayoutPanel1";
          this.flowLayoutPanel1.Size = new System.Drawing.Size(574, 448);
          this.flowLayoutPanel1.TabIndex = 0;
          this.flowLayoutPanel1.WrapContents = false;
          this.flowLayoutPanel1.Click += new System.EventHandler(this.flowLayoutPanel1_Click);
          // 
          // ExtensionListControl
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
          this.AutoSize = true;
          this.Controls.Add(this.flowLayoutPanel1);
          this.Name = "ExtensionListControl";
          this.Size = new System.Drawing.Size(574, 448);
          this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
