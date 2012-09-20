namespace MpeCore.Classes.SectionPanel
{
    partial class BaseVerticalLayout
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
      this.panel2 = new System.Windows.Forms.Panel();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.etchedLine1 = new MpeCore.Classes.SectionPanel.EtchedLine();
      this.panel3 = new System.Windows.Forms.Panel();
      this.panel5 = new System.Windows.Forms.Panel();
      this.panel4 = new System.Windows.Forms.Panel();
      this.panel2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.panel1.SuspendLayout();
      this.panel3.SuspendLayout();
      this.panel5.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.pictureBox1);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(168, 306);
      this.panel2.TabIndex = 6;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox1.Image = global::MpeCore.Properties.Resources.left;
      this.pictureBox1.Location = new System.Drawing.Point(0, 0);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(168, 306);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 5;
      this.pictureBox1.TabStop = false;
      // 
      // groupBox1
      // 
      this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(0);
      this.groupBox1.Size = new System.Drawing.Size(495, 46);
      this.groupBox1.TabIndex = 7;
      this.groupBox1.TabStop = false;
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.SystemColors.Control;
      this.panel1.Controls.Add(this.etchedLine1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 306);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(494, 44);
      this.panel1.TabIndex = 8;
      // 
      // etchedLine1
      // 
      this.etchedLine1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.etchedLine1.Edge = MpeCore.Classes.SectionPanel.EtchEdge.Top;
      this.etchedLine1.Location = new System.Drawing.Point(0, 0);
      this.etchedLine1.Name = "etchedLine1";
      this.etchedLine1.Size = new System.Drawing.Size(494, 44);
      this.etchedLine1.TabIndex = 0;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.panel5);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel3.Location = new System.Drawing.Point(0, 0);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(494, 306);
      this.panel3.TabIndex = 9;
      // 
      // panel5
      // 
      this.panel5.Controls.Add(this.panel4);
      this.panel5.Controls.Add(this.panel2);
      this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel5.Location = new System.Drawing.Point(0, 0);
      this.panel5.Name = "panel5";
      this.panel5.Size = new System.Drawing.Size(494, 306);
      this.panel5.TabIndex = 0;
      // 
      // panel4
      // 
      this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel4.Location = new System.Drawing.Point(168, 0);
      this.panel4.Name = "panel4";
      this.panel4.Size = new System.Drawing.Size(326, 306);
      this.panel4.TabIndex = 7;
      // 
      // BaseVerticalLayout
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.ClientSize = new System.Drawing.Size(494, 350);
      this.Controls.Add(this.panel3);
      this.Controls.Add(this.panel1);
      this.Name = "BaseVerticalLayout";
      this.Text = "BaseVerticalLayout";
      this.panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.panel5.ResumeLayout(false);
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private EtchedLine etchedLine1;
    }
}