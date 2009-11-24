namespace MpeMaker.Dialogs
{
    partial class AddFolder2Group
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
            this.txt_folder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chk_recurs = new System.Windows.Forms.CheckBox();
            this.add_folder = new System.Windows.Forms.Button();
            this.btn_add_template = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.txt_template = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txt_param1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmb_overwrite = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmb_installtype = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // txt_folder
            // 
            this.txt_folder.Location = new System.Drawing.Point(12, 24);
            this.txt_folder.Name = "txt_folder";
            this.txt_folder.Size = new System.Drawing.Size(361, 20);
            this.txt_folder.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Folder";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Template";
            // 
            // chk_recurs
            // 
            this.chk_recurs.AutoSize = true;
            this.chk_recurs.Checked = true;
            this.chk_recurs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_recurs.Location = new System.Drawing.Point(15, 89);
            this.chk_recurs.Name = "chk_recurs";
            this.chk_recurs.Size = new System.Drawing.Size(134, 17);
            this.chk_recurs.TabIndex = 4;
            this.chk_recurs.Text = "Recurse subdirectories";
            this.chk_recurs.UseVisualStyleBackColor = true;
            // 
            // add_folder
            // 
            this.add_folder.Location = new System.Drawing.Point(379, 24);
            this.add_folder.Name = "add_folder";
            this.add_folder.Size = new System.Drawing.Size(32, 20);
            this.add_folder.TabIndex = 5;
            this.add_folder.Text = "...";
            this.add_folder.UseVisualStyleBackColor = true;
            this.add_folder.Click += new System.EventHandler(this.add_folder_Click);
            // 
            // btn_add_template
            // 
            this.btn_add_template.Location = new System.Drawing.Point(379, 63);
            this.btn_add_template.Name = "btn_add_template";
            this.btn_add_template.Size = new System.Drawing.Size(32, 20);
            this.btn_add_template.TabIndex = 6;
            this.btn_add_template.Text = "...";
            this.btn_add_template.UseVisualStyleBackColor = true;
            this.btn_add_template.Click += new System.EventHandler(this.btn_add_template_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(130, 213);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Add";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(211, 213);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // txt_template
            // 
            this.txt_template.FormattingEnabled = true;
            this.txt_template.Items.AddRange(new object[] {
            "%Skin%\\[DEFAULT]\\"});
            this.txt_template.Location = new System.Drawing.Point(12, 62);
            this.txt_template.Name = "txt_template";
            this.txt_template.Size = new System.Drawing.Size(361, 21);
            this.txt_template.TabIndex = 9;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 173);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "Param 1";
            // 
            // txt_param1
            // 
            this.txt_param1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_param1.Location = new System.Drawing.Point(104, 170);
            this.txt_param1.Name = "txt_param1";
            this.txt_param1.Size = new System.Drawing.Size(269, 20);
            this.txt_param1.TabIndex = 18;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 146);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Update Option";
            // 
            // cmb_overwrite
            // 
            this.cmb_overwrite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmb_overwrite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_overwrite.FormattingEnabled = true;
            this.cmb_overwrite.Items.AddRange(new object[] {
            "Newer overwrite",
            "Always overwrite",
            "Overwrite if older"});
            this.cmb_overwrite.Location = new System.Drawing.Point(104, 143);
            this.cmb_overwrite.Name = "cmb_overwrite";
            this.cmb_overwrite.Size = new System.Drawing.Size(269, 21);
            this.cmb_overwrite.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 119);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Install type";
            // 
            // cmb_installtype
            // 
            this.cmb_installtype.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmb_installtype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_installtype.FormattingEnabled = true;
            this.cmb_installtype.Location = new System.Drawing.Point(104, 116);
            this.cmb_installtype.Name = "cmb_installtype";
            this.cmb_installtype.Size = new System.Drawing.Size(269, 21);
            this.cmb_installtype.TabIndex = 11;
            // 
            // AddFolder2Group
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 249);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txt_param1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cmb_overwrite);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmb_installtype);
            this.Controls.Add(this.txt_template);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_add_template);
            this.Controls.Add(this.add_folder);
            this.Controls.Add(this.chk_recurs);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_folder);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddFolder2Group";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add folder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_folder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chk_recurs;
        private System.Windows.Forms.Button add_folder;
        private System.Windows.Forms.Button btn_add_template;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ComboBox txt_template;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txt_param1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmb_overwrite;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmb_installtype;
    }
}