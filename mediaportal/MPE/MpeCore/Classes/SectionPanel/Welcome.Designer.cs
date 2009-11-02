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
            this.button_back = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.lbl_desc1 = new System.Windows.Forms.Label();
            this.lbl_desc2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button_back
            // 
            this.button_back.Location = new System.Drawing.Point(238, 321);
            this.button_back.Name = "button_back";
            this.button_back.Size = new System.Drawing.Size(75, 23);
            this.button_back.TabIndex = 10;
            this.button_back.Text = "< Back";
            this.button_back.UseVisualStyleBackColor = true;
            this.button_back.Visible = false;
            this.button_back.Click += new System.EventHandler(this.button_back_Click);
            // 
            // button_next
            // 
            this.button_next.Location = new System.Drawing.Point(319, 321);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(75, 23);
            this.button_next.TabIndex = 11;
            this.button_next.Text = "Next >";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Location = new System.Drawing.Point(414, 321);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_cancel.TabIndex = 12;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
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
            this.lbl_desc2.Text = "label1";
            // 
            // Welcome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 356);
            this.Controls.Add(this.lbl_desc2);
            this.Controls.Add(this.lbl_desc1);
            this.Controls.Add(this.button_back);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.button_cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Welcome";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Installer";
            this.Load += new System.EventHandler(this.Welcome_Load);
            this.Controls.SetChildIndex(this.button_cancel, 0);
            this.Controls.SetChildIndex(this.button_next, 0);
            this.Controls.SetChildIndex(this.button_back, 0);
            this.Controls.SetChildIndex(this.lbl_desc1, 0);
            this.Controls.SetChildIndex(this.lbl_desc2, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_back;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label lbl_desc1;
        private System.Windows.Forms.Label lbl_desc2;
    }
}