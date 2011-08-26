namespace MpeMaker.Dialogs
{
    partial class EditScript
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.validateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox_error = new System.Windows.Forms.TextBox();
            this.textBox_code = new System.Windows.Forms.RichTextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.validateToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(921, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // validateToolStripMenuItem
            // 
            this.validateToolStripMenuItem.Name = "validateToolStripMenuItem";
            this.validateToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.validateToolStripMenuItem.Text = "Validate";
            this.validateToolStripMenuItem.Click += new System.EventHandler(this.validateToolStripMenuItem_Click);
            // 
            // textBox_error
            // 
            this.textBox_error.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_error.Location = new System.Drawing.Point(0, 507);
            this.textBox_error.Multiline = true;
            this.textBox_error.Name = "textBox_error";
            this.textBox_error.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_error.Size = new System.Drawing.Size(920, 75);
            this.textBox_error.TabIndex = 3;
            // 
            // textBox_code
            // 
            this.textBox_code.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_code.Location = new System.Drawing.Point(0, 27);
            this.textBox_code.Name = "textBox_code";
            this.textBox_code.ShowSelectionMargin = true;
            this.textBox_code.Size = new System.Drawing.Size(920, 474);
            this.textBox_code.TabIndex = 4;
            this.textBox_code.Text = "";
            // 
            // EditScript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(921, 580);
            this.Controls.Add(this.textBox_code);
            this.Controls.Add(this.textBox_error);
            this.Controls.Add(this.menuStrip1);
            this.Name = "EditScript";
            this.Text = "EditScript";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem validateToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox_error;
        private System.Windows.Forms.RichTextBox textBox_code;
    }
}