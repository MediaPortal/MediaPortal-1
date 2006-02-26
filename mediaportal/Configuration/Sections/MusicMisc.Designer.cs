namespace MediaPortal.Configuration.Sections
{
    partial class MusicMisc
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.PlayNowJumpToCmbBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.PlayNowJumpToCmbBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(472, 79);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Play Now Settings";
            // 
            // PlayNowJumpToCmbBox
            // 
            this.PlayNowJumpToCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PlayNowJumpToCmbBox.FormattingEnabled = true;
            this.PlayNowJumpToCmbBox.Location = new System.Drawing.Point(182, 27);
            this.PlayNowJumpToCmbBox.Name = "PlayNowJumpToCmbBox";
            this.PlayNowJumpToCmbBox.Size = new System.Drawing.Size(264, 21);
            this.PlayNowJumpToCmbBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Jump to window on Play Now:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // MusicMisc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "MusicMisc";
            this.Size = new System.Drawing.Size(472, 408);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox PlayNowJumpToCmbBox;
    }
}
