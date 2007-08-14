namespace MediaPortal.MPInstaller
{
    partial class start_form
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
          System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(start_form));
          this.button1 = new System.Windows.Forms.Button();
          this.button2 = new System.Windows.Forms.Button();
          this.button3 = new System.Windows.Forms.Button();
          this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
          this.button4 = new System.Windows.Forms.Button();
          this.SuspendLayout();
          // 
          // button1
          // 
          this.button1.Location = new System.Drawing.Point(171, 21);
          this.button1.Name = "button1";
          this.button1.Size = new System.Drawing.Size(109, 32);
          this.button1.TabIndex = 0;
          this.button1.Text = "Create package";
          this.button1.UseVisualStyleBackColor = true;
          this.button1.Click += new System.EventHandler(this.button1_Click);
          // 
          // button2
          // 
          this.button2.Location = new System.Drawing.Point(171, 71);
          this.button2.Name = "button2";
          this.button2.Size = new System.Drawing.Size(109, 32);
          this.button2.TabIndex = 1;
          this.button2.Text = "Install package";
          this.button2.UseVisualStyleBackColor = true;
          this.button2.Click += new System.EventHandler(this.button2_Click);
          // 
          // button3
          // 
          this.button3.Location = new System.Drawing.Point(171, 121);
          this.button3.Name = "button3";
          this.button3.Size = new System.Drawing.Size(109, 32);
          this.button3.TabIndex = 2;
          this.button3.Text = "Control panel";
          this.button3.UseVisualStyleBackColor = true;
          this.button3.Click += new System.EventHandler(this.button3_Click);
          // 
          // openFileDialog1
          // 
          this.openFileDialog1.Filter = "MPI files|*.mpi|ZIP files|*.zip|All files|*.*";
          // 
          // button4
          // 
          this.button4.Location = new System.Drawing.Point(171, 172);
          this.button4.Name = "button4";
          this.button4.Size = new System.Drawing.Size(109, 32);
          this.button4.TabIndex = 3;
          this.button4.Text = "Check for updates";
          this.button4.UseVisualStyleBackColor = true;
          this.button4.Click += new System.EventHandler(this.button4_Click_1);
          // 
          // start_form
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
          this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
          this.ClientSize = new System.Drawing.Size(292, 249);
          this.Controls.Add(this.button4);
          this.Controls.Add(this.button3);
          this.Controls.Add(this.button2);
          this.Controls.Add(this.button1);
          this.DoubleBuffered = true;
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
          this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
          this.MaximizeBox = false;
          this.Name = "start_form";
          this.RightToLeft = System.Windows.Forms.RightToLeft.No;
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
          this.Text = "MediaPortal Plugin Installer";
          this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button4;
    }
}