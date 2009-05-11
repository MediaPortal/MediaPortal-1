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
          this.label1 = new System.Windows.Forms.Label();
          this.label2 = new System.Windows.Forms.Label();
          this.label3 = new System.Windows.Forms.Label();
          this.label4 = new System.Windows.Forms.Label();
          this.SuspendLayout();
          // 
          // button1
          // 
          this.button1.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.button1.Location = new System.Drawing.Point(12, 112);
          this.button1.Name = "button1";
          this.button1.Size = new System.Drawing.Size(137, 32);
          this.button1.TabIndex = 0;
          this.button1.Text = "&Create extension";
          this.button1.UseVisualStyleBackColor = true;
          this.button1.Visible = false;
          this.button1.Click += new System.EventHandler(this.button1_Click);
          // 
          // button2
          // 
          this.button2.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.button2.Location = new System.Drawing.Point(12, 162);
          this.button2.Name = "button2";
          this.button2.Size = new System.Drawing.Size(137, 32);
          this.button2.TabIndex = 1;
          this.button2.Text = "&Install extension";
          this.button2.UseVisualStyleBackColor = true;
          this.button2.Visible = false;
          this.button2.Click += new System.EventHandler(this.button2_Click);
          // 
          // button3
          // 
          this.button3.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.button3.Location = new System.Drawing.Point(12, 212);
          this.button3.Name = "button3";
          this.button3.Size = new System.Drawing.Size(137, 32);
          this.button3.TabIndex = 2;
          this.button3.Text = "Extension &manager";
          this.button3.UseVisualStyleBackColor = true;
          this.button3.Visible = false;
          this.button3.Click += new System.EventHandler(this.button3_Click);
          // 
          // openFileDialog1
          // 
          this.openFileDialog1.Filter = "MPI files|*.mpi|ZIP files|*.zip|All files|*.*";
          // 
          // button4
          // 
          this.button4.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.button4.Location = new System.Drawing.Point(12, 263);
          this.button4.Name = "button4";
          this.button4.Size = new System.Drawing.Size(137, 32);
          this.button4.TabIndex = 3;
          this.button4.Text = "&Check for updates";
          this.button4.UseVisualStyleBackColor = true;
          this.button4.Visible = false;
          this.button4.Click += new System.EventHandler(this.button4_Click_1);
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.BackColor = System.Drawing.Color.Transparent;
          this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
          this.label1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label1.ForeColor = System.Drawing.Color.White;
          this.label1.Location = new System.Drawing.Point(501, 339);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(139, 16);
          this.label1.TabIndex = 4;
          this.label1.Text = "&Check for updates";
          this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
          this.label1.MouseLeave += new System.EventHandler(this.label1_MouseLeave);
          this.label1.Click += new System.EventHandler(this.button4_Click_1);
          this.label1.MouseEnter += new System.EventHandler(this.label1_MouseEnter);
          // 
          // label2
          // 
          this.label2.AutoSize = true;
          this.label2.BackColor = System.Drawing.Color.Transparent;
          this.label2.Cursor = System.Windows.Forms.Cursors.Hand;
          this.label2.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label2.ForeColor = System.Drawing.Color.White;
          this.label2.Location = new System.Drawing.Point(273, 136);
          this.label2.Name = "label2";
          this.label2.Size = new System.Drawing.Size(133, 16);
          this.label2.TabIndex = 5;
          this.label2.Text = "&Create extension";
          this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
          this.label2.MouseLeave += new System.EventHandler(this.label1_MouseLeave);
          this.label2.Click += new System.EventHandler(this.button1_Click);
          this.label2.MouseEnter += new System.EventHandler(this.label1_MouseEnter);
          // 
          // label3
          // 
          this.label3.AutoSize = true;
          this.label3.BackColor = System.Drawing.Color.Transparent;
          this.label3.Cursor = System.Windows.Forms.Cursors.Hand;
          this.label3.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label3.ForeColor = System.Drawing.Color.White;
          this.label3.Location = new System.Drawing.Point(273, 178);
          this.label3.Name = "label3";
          this.label3.Size = new System.Drawing.Size(130, 16);
          this.label3.TabIndex = 6;
          this.label3.Text = "&Install extension";
          this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
          this.label3.MouseLeave += new System.EventHandler(this.label1_MouseLeave);
          this.label3.Click += new System.EventHandler(this.button2_Click);
          this.label3.MouseEnter += new System.EventHandler(this.label1_MouseEnter);
          // 
          // label4
          // 
          this.label4.AutoSize = true;
          this.label4.BackColor = System.Drawing.Color.Transparent;
          this.label4.Cursor = System.Windows.Forms.Cursors.Hand;
          this.label4.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label4.ForeColor = System.Drawing.Color.White;
          this.label4.Location = new System.Drawing.Point(273, 220);
          this.label4.Name = "label4";
          this.label4.Size = new System.Drawing.Size(149, 16);
          this.label4.TabIndex = 7;
          this.label4.Text = "Extension &manager";
          this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
          this.label4.MouseLeave += new System.EventHandler(this.label1_MouseLeave);
          this.label4.Click += new System.EventHandler(this.button3_Click);
          this.label4.MouseEnter += new System.EventHandler(this.label1_MouseEnter);
          // 
          // start_form
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
          this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
          this.ClientSize = new System.Drawing.Size(694, 373);
          this.Controls.Add(this.label4);
          this.Controls.Add(this.label3);
          this.Controls.Add(this.label2);
          this.Controls.Add(this.label1);
          this.Controls.Add(this.button4);
          this.Controls.Add(this.button3);
          this.Controls.Add(this.button2);
          this.Controls.Add(this.button1);
          this.DoubleBuffered = true;
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
          this.MaximizeBox = false;
          this.Name = "start_form";
          this.RightToLeft = System.Windows.Forms.RightToLeft.No;
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
          this.Text = "MediaPortal Extension Installer";
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button4;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Label label4;
    }
}