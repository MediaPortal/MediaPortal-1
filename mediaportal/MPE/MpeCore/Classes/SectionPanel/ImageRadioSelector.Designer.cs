namespace MpeCore.Classes.SectionPanel
{
    partial class ImageRadioSelector
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
          this.label1 = new System.Windows.Forms.Label();
          this.pictureBox3 = new System.Windows.Forms.PictureBox();
          this.radioButton2 = new System.Windows.Forms.RadioButton();
          this.radioButton1 = new System.Windows.Forms.RadioButton();
          this.pictureBox1 = new System.Windows.Forms.PictureBox();
          ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
          ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
          this.SuspendLayout();
          // 
          // button_next
          // 
          this.button_next.Text = "Next>";
          // 
          // label1
          // 
          this.label1.Location = new System.Drawing.Point(13, 72);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(459, 47);
          this.label1.TabIndex = 27;
          this.label1.Text = "label1";
          // 
          // pictureBox3
          // 
          this.pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
          this.pictureBox3.Location = new System.Drawing.Point(259, 153);
          this.pictureBox3.Name = "pictureBox3";
          this.pictureBox3.Size = new System.Drawing.Size(225, 127);
          this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
          this.pictureBox3.TabIndex = 26;
          this.pictureBox3.TabStop = false;
          this.pictureBox3.WaitOnLoad = true;
          this.pictureBox3.Click += new System.EventHandler(this.pictureBox3_Click);
          // 
          // radioButton2
          // 
          this.radioButton2.AutoSize = true;
          this.radioButton2.Location = new System.Drawing.Point(259, 130);
          this.radioButton2.Name = "radioButton2";
          this.radioButton2.Size = new System.Drawing.Size(85, 17);
          this.radioButton2.TabIndex = 25;
          this.radioButton2.TabStop = true;
          this.radioButton2.Text = "radioButton2";
          this.radioButton2.UseVisualStyleBackColor = true;
          this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
          // 
          // radioButton1
          // 
          this.radioButton1.AutoSize = true;
          this.radioButton1.Location = new System.Drawing.Point(13, 130);
          this.radioButton1.Name = "radioButton1";
          this.radioButton1.Size = new System.Drawing.Size(85, 17);
          this.radioButton1.TabIndex = 24;
          this.radioButton1.TabStop = true;
          this.radioButton1.Text = "radioButton1";
          this.radioButton1.UseVisualStyleBackColor = true;
          this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
          // 
          // pictureBox1
          // 
          this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
          this.pictureBox1.Location = new System.Drawing.Point(13, 153);
          this.pictureBox1.Name = "pictureBox1";
          this.pictureBox1.Size = new System.Drawing.Size(225, 127);
          this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
          this.pictureBox1.TabIndex = 23;
          this.pictureBox1.TabStop = false;
          this.pictureBox1.WaitOnLoad = true;
          this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
          // 
          // ImageRadioSelector
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(499, 354);
          this.Controls.Add(this.label1);
          this.Controls.Add(this.pictureBox3);
          this.Controls.Add(this.radioButton2);
          this.Controls.Add(this.radioButton1);
          this.Controls.Add(this.pictureBox1);
          this.Name = "ImageRadioSelector";
          this.Text = "Extension Installer for   - 0.0.0.0";
          this.Controls.SetChildIndex(this.pictureBox1, 0);
          this.Controls.SetChildIndex(this.radioButton1, 0);
          this.Controls.SetChildIndex(this.radioButton2, 0);
          this.Controls.SetChildIndex(this.pictureBox3, 0);
          this.Controls.SetChildIndex(this.label1, 0);
          ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
          ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.PictureBox pictureBox1;

    }
}