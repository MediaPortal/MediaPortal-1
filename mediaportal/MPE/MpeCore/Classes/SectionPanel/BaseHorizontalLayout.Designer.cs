namespace MpeCore.Classes.SectionPanel
{
    partial class BaseHorizontalLayout
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseHorizontalLayout));
      this.panel1 = new System.Windows.Forms.Panel();
      this.button_cancel = new System.Windows.Forms.Button();
      this.button_back = new System.Windows.Forms.Button();
      this.button_next = new System.Windows.Forms.Button();
      this.panel3 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.lbl_small = new System.Windows.Forms.Label();
      this.lbl_large = new System.Windows.Forms.Label();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.SystemColors.Control;
      this.panel1.Controls.Add(this.button_cancel);
      this.panel1.Controls.Add(this.button_back);
      this.panel1.Controls.Add(this.button_next);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 306);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(494, 44);
      this.panel1.TabIndex = 22;
      // 
      // button_cancel
      // 
      this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button_cancel.Location = new System.Drawing.Point(412, 13);
      this.button_cancel.Name = "button_cancel";
      this.button_cancel.Size = new System.Drawing.Size(72, 23);
      this.button_cancel.TabIndex = 21;
      this.button_cancel.Text = "Cancel";
      this.button_cancel.UseVisualStyleBackColor = true;
      this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
      // 
      // button_back
      // 
      this.button_back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button_back.Location = new System.Drawing.Point(236, 13);
      this.button_back.Name = "button_back";
      this.button_back.Size = new System.Drawing.Size(72, 23);
      this.button_back.TabIndex = 19;
      this.button_back.Text = "< Back";
      this.button_back.UseVisualStyleBackColor = true;
      this.button_back.Click += new System.EventHandler(this.button_back_Click);
      // 
      // button_next
      // 
      this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button_next.Location = new System.Drawing.Point(317, 13);
      this.button_next.Name = "button_next";
      this.button_next.Size = new System.Drawing.Size(72, 23);
      this.button_next.TabIndex = 20;
      this.button_next.Text = "Next >";
      this.button_next.UseVisualStyleBackColor = true;
      this.button_next.Click += new System.EventHandler(this.button_next_Click);
      // 
      // panel3
      // 
      this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel3.Location = new System.Drawing.Point(0, 65);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(494, 241);
      this.panel3.TabIndex = 23;
      // 
      // panel2
      // 
      this.panel2.BackColor = System.Drawing.Color.White;
      this.panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel2.BackgroundImage")));
      this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.panel2.Controls.Add(this.lbl_small);
      this.panel2.Controls.Add(this.lbl_large);
      this.panel2.Controls.Add(this.pictureBox1);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(494, 65);
      this.panel2.TabIndex = 0;
      // 
      // lbl_small
      // 
      this.lbl_small.BackColor = System.Drawing.Color.Transparent;
      this.lbl_small.ForeColor = System.Drawing.Color.Azure;
      this.lbl_small.Location = new System.Drawing.Point(194, 32);
      this.lbl_small.Name = "lbl_small";
      this.lbl_small.Size = new System.Drawing.Size(232, 31);
      this.lbl_small.TabIndex = 2;
      this.lbl_small.Text = "This is a longer description Text that even goes over two lines and has more text" +
    " to show now!";
      // 
      // lbl_large
      // 
      this.lbl_large.BackColor = System.Drawing.Color.Transparent;
      this.lbl_large.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_large.ForeColor = System.Drawing.Color.Azure;
      this.lbl_large.Location = new System.Drawing.Point(84, 8);
      this.lbl_large.Name = "lbl_large";
      this.lbl_large.Size = new System.Drawing.Size(342, 22);
      this.lbl_large.TabIndex = 1;
      this.lbl_large.Text = "Please wait while installing this Extension for MediaPortal";
      // 
      // pictureBox1
      // 
      this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
      this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.pictureBox1.Location = new System.Drawing.Point(432, 0);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(64, 64);
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      // 
      // BaseHorizontalLayout
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(494, 350);
      this.Controls.Add(this.panel3);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.panel2);
      this.Name = "BaseHorizontalLayout";
      this.Text = "BaseHorizontalLayout";
      this.Load += new System.EventHandler(this.BaseVerticalLayout_Load);
      this.Shown += new System.EventHandler(this.BaseHorizontalLayout_Shown);
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lbl_small;
        private System.Windows.Forms.Label lbl_large;
        public System.Windows.Forms.Button button_back;
        public System.Windows.Forms.Button button_next;
        public System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Panel panel3;
    }
}