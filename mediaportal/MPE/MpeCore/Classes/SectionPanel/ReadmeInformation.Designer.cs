namespace MpeCore.Classes.SectionPanel
{
    partial class ReadmeInformation
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
          this.richTextBox1 = new System.Windows.Forms.RichTextBox();
          this.SuspendLayout();
          // 
          // button_next
          // 
          this.button_next.Text = "Next>";
          // 
          // richTextBox1
          // 
          this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
          this.richTextBox1.Location = new System.Drawing.Point(-1, 69);
          this.richTextBox1.Name = "richTextBox1";
          this.richTextBox1.ReadOnly = true;
          this.richTextBox1.Size = new System.Drawing.Size(499, 229);
          this.richTextBox1.TabIndex = 14;
          this.richTextBox1.Text = "";
          // 
          // ReadmeInformation
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(495, 350);
          this.Controls.Add(this.richTextBox1);
          this.Name = "ReadmeInformation";
          this.Text = "Extension Installer for   - 0.0.0.0";
          this.Controls.SetChildIndex(this.richTextBox1, 0);
          this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}