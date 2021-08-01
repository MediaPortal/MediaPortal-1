namespace MpeCore.Classes.SectionPanel
{
    partial class Welcome
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
      this.lbl_desc1 = new System.Windows.Forms.Label();
      this.lbl_desc2 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MpeCore.Properties.Resources.left;
      // 
      // button_cancel
      // 
      this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
      // 
      // button_back
      // 
      this.button_back.Click += new System.EventHandler(this.button_back_Click);
      // 
      // button_next
      // 
      this.button_next.Click += new System.EventHandler(this.button_next_Click);
      // 
      // lbl_desc1
      // 
      this.lbl_desc1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_desc1.Location = new System.Drawing.Point(174, 9);
      this.lbl_desc1.Name = "lbl_desc1";
      this.lbl_desc1.Size = new System.Drawing.Size(315, 73);
      this.lbl_desc1.TabIndex = 13;
      // 
      // lbl_desc2
      // 
      this.lbl_desc2.Location = new System.Drawing.Point(175, 106);
      this.lbl_desc2.Name = "lbl_desc2";
      this.lbl_desc2.Size = new System.Drawing.Size(310, 192);
      this.lbl_desc2.TabIndex = 14;
      // 
      // Welcome
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.ClientSize = new System.Drawing.Size(494, 350);
      this.Controls.Add(this.lbl_desc2);
      this.Controls.Add(this.lbl_desc1);
      this.Name = "Welcome";
      this.Text = "Installer";
      this.Load += new System.EventHandler(this.Welcome_Load);
      this.Controls.SetChildIndex(this.lbl_desc1, 0);
      this.Controls.SetChildIndex(this.lbl_desc2, 0);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lbl_desc1;
        private System.Windows.Forms.Label lbl_desc2;
    }
}