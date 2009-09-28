namespace MpeMaker.Sections
{
    partial class GeneralSection
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
            this.label1 = new System.Windows.Forms.Label();
            this.txt_name = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_guid = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_version1 = new System.Windows.Forms.TextBox();
            this.txt_version2 = new System.Windows.Forms.TextBox();
            this.txt_version3 = new System.Windows.Forms.TextBox();
            this.txt_version4 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Extension name";
            // 
            // txt_name
            // 
            this.txt_name.Location = new System.Drawing.Point(115, 7);
            this.txt_name.Name = "txt_name";
            this.txt_name.Size = new System.Drawing.Size(229, 20);
            this.txt_name.TabIndex = 1;
            this.txt_name.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Extension GUID";
            // 
            // txt_guid
            // 
            this.txt_guid.Location = new System.Drawing.Point(115, 33);
            this.txt_guid.Name = "txt_guid";
            this.txt_guid.Size = new System.Drawing.Size(229, 20);
            this.txt_guid.TabIndex = 3;
            this.txt_guid.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Version";
            // 
            // txt_version1
            // 
            this.txt_version1.Location = new System.Drawing.Point(114, 59);
            this.txt_version1.Name = "txt_version1";
            this.txt_version1.Size = new System.Drawing.Size(52, 20);
            this.txt_version1.TabIndex = 5;
            this.txt_version1.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // txt_version2
            // 
            this.txt_version2.Location = new System.Drawing.Point(172, 59);
            this.txt_version2.Name = "txt_version2";
            this.txt_version2.Size = new System.Drawing.Size(52, 20);
            this.txt_version2.TabIndex = 6;
            this.txt_version2.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // txt_version3
            // 
            this.txt_version3.Location = new System.Drawing.Point(230, 59);
            this.txt_version3.Name = "txt_version3";
            this.txt_version3.Size = new System.Drawing.Size(52, 20);
            this.txt_version3.TabIndex = 7;
            this.txt_version3.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // txt_version4
            // 
            this.txt_version4.Location = new System.Drawing.Point(292, 59);
            this.txt_version4.Name = "txt_version4";
            this.txt_version4.Size = new System.Drawing.Size(52, 20);
            this.txt_version4.TabIndex = 8;
            this.txt_version4.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // GeneralSection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txt_version4);
            this.Controls.Add(this.txt_version3);
            this.Controls.Add(this.txt_version2);
            this.Controls.Add(this.txt_version1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_guid);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_name);
            this.Controls.Add(this.label1);
            this.Name = "GeneralSection";
            this.Size = new System.Drawing.Size(571, 348);
            this.Load += new System.EventHandler(this.GeneralSection_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_name;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_guid;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_version1;
        private System.Windows.Forms.TextBox txt_version2;
        private System.Windows.Forms.TextBox txt_version3;
        private System.Windows.Forms.TextBox txt_version4;
    }
}
