namespace MPx86Proxy.Logging
{
    partial class SearchForm
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button_Find = new System.Windows.Forms.Button();
            this.checkBox_Case = new System.Windows.Forms.CheckBox();
            this.checkBox_Back = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(268, 20);
            this.textBox1.TabIndex = 0;
            // 
            // button_Find
            // 
            this.button_Find.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Find.Location = new System.Drawing.Point(205, 43);
            this.button_Find.Name = "button_Find";
            this.button_Find.Size = new System.Drawing.Size(75, 23);
            this.button_Find.TabIndex = 1;
            this.button_Find.Text = "Search";
            this.button_Find.UseVisualStyleBackColor = true;
            this.button_Find.Click += new System.EventHandler(this.button_Find_Click);
            // 
            // checkBox_Case
            // 
            this.checkBox_Case.AutoSize = true;
            this.checkBox_Case.Location = new System.Drawing.Point(18, 39);
            this.checkBox_Case.Name = "checkBox_Case";
            this.checkBox_Case.Size = new System.Drawing.Size(94, 17);
            this.checkBox_Case.TabIndex = 2;
            this.checkBox_Case.Text = "Case sensitive";
            this.checkBox_Case.UseVisualStyleBackColor = true;
            // 
            // checkBox_Back
            // 
            this.checkBox_Back.AutoSize = true;
            this.checkBox_Back.Location = new System.Drawing.Point(18, 57);
            this.checkBox_Back.Name = "checkBox_Back";
            this.checkBox_Back.Size = new System.Drawing.Size(109, 17);
            this.checkBox_Back.TabIndex = 3;
            this.checkBox_Back.Text = "Backward search";
            this.checkBox_Back.UseVisualStyleBackColor = true;
            this.checkBox_Back.CheckedChanged += new System.EventHandler(this.checkBox_Back_CheckedChanged);
            // 
            // FindQueryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 78);
            this.Controls.Add(this.checkBox_Back);
            this.Controls.Add(this.checkBox_Case);
            this.Controls.Add(this.button_Find);
            this.Controls.Add(this.textBox1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1920, 105);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 105);
            this.Name = "FindQueryForm";
            this.ShowIcon = false;
            this.Text = "FindQueryForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindQueryForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button_Find;
        private System.Windows.Forms.CheckBox checkBox_Case;
        private System.Windows.Forms.CheckBox checkBox_Back;
    }
}