namespace MpeMaker.Wizards.Skin_wizard
{
    partial class WizardSkinSelect
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
            this.components = new System.ComponentModel.Container();
            this.txt_skinpath = new System.Windows.Forms.TextBox();
            this.btn_browse_skin = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btn_browse_font = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btn_next1 = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btn_next2 = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btn_next3 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_skin_font = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btn_next4 = new System.Windows.Forms.Button();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // txt_skinpath
            // 
            this.txt_skinpath.AllowDrop = true;
            this.txt_skinpath.Enabled = false;
            this.txt_skinpath.Location = new System.Drawing.Point(6, 39);
            this.txt_skinpath.Name = "txt_skinpath";
            this.txt_skinpath.Size = new System.Drawing.Size(436, 24);
            this.txt_skinpath.TabIndex = 5;
            this.toolTip1.SetToolTip(this.txt_skinpath, "Folder of the skin, can use drag&drop+");
            this.txt_skinpath.DragDrop += new System.Windows.Forms.DragEventHandler(this.txt_skinpath_DragDrop);
            this.txt_skinpath.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skinpath_DragEnter);
            // 
            // btn_browse_skin
            // 
            this.btn_browse_skin.Location = new System.Drawing.Point(450, 39);
            this.btn_browse_skin.Name = "btn_browse_skin";
            this.btn_browse_skin.Size = new System.Drawing.Size(75, 24);
            this.btn_browse_skin.TabIndex = 6;
            this.btn_browse_skin.Text = "Browse";
            this.toolTip1.SetToolTip(this.btn_browse_skin, "Folder of the skin, can use drag&drop+");
            this.btn_browse_skin.UseVisualStyleBackColor = true;
            this.btn_browse_skin.Click += new System.EventHandler(this.btn_browse_skin_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Select the skin folder ";
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 18);
            this.label1.TabIndex = 7;
            this.label1.Text = "Skin folder";
            this.toolTip1.SetToolTip(this.label1, "Folder of the skin, can use drag&drop+");
            // 
            // btn_browse_font
            // 
            this.btn_browse_font.Location = new System.Drawing.Point(450, 38);
            this.btn_browse_font.Name = "btn_browse_font";
            this.btn_browse_font.Size = new System.Drawing.Size(75, 26);
            this.btn_browse_font.TabIndex = 12;
            this.btn_browse_font.Text = "Browse";
            this.toolTip1.SetToolTip(this.btn_browse_font, "Folder of the skin, can use drag&drop+");
            this.btn_browse_font.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(541, 327);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btn_next1);
            this.tabPage1.Location = new System.Drawing.Point(4, 27);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(533, 296);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "1.Welcome";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btn_next1
            // 
            this.btn_next1.Image = global::MpeMaker.Properties.Resources.go_next;
            this.btn_next1.Location = new System.Drawing.Point(469, 237);
            this.btn_next1.Name = "btn_next1";
            this.btn_next1.Size = new System.Drawing.Size(56, 51);
            this.btn_next1.TabIndex = 0;
            this.btn_next1.UseVisualStyleBackColor = true;
            this.btn_next1.Click += new System.EventHandler(this.btn_next1_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btn_next2);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.txt_skinpath);
            this.tabPage2.Controls.Add(this.btn_browse_skin);
            this.tabPage2.Location = new System.Drawing.Point(4, 27);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(533, 296);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "2.Skin";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btn_next2
            // 
            this.btn_next2.Image = global::MpeMaker.Properties.Resources.go_next;
            this.btn_next2.Location = new System.Drawing.Point(469, 237);
            this.btn_next2.Name = "btn_next2";
            this.btn_next2.Size = new System.Drawing.Size(56, 51);
            this.btn_next2.TabIndex = 8;
            this.btn_next2.UseVisualStyleBackColor = true;
            this.btn_next2.Click += new System.EventHandler(this.btn_next1_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btn_next3);
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Controls.Add(this.btn_browse_font);
            this.tabPage3.Controls.Add(this.txt_skin_font);
            this.tabPage3.Location = new System.Drawing.Point(4, 27);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(533, 296);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "3.Font";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btn_next3
            // 
            this.btn_next3.Image = global::MpeMaker.Properties.Resources.go_next;
            this.btn_next3.Location = new System.Drawing.Point(469, 237);
            this.btn_next3.Name = "btn_next3";
            this.btn_next3.Size = new System.Drawing.Size(56, 51);
            this.btn_next3.TabIndex = 14;
            this.btn_next3.UseVisualStyleBackColor = true;
            this.btn_next3.Click += new System.EventHandler(this.btn_next1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 18);
            this.label2.TabIndex = 13;
            this.label2.Text = "Font folder";
            // 
            // txt_skin_font
            // 
            this.txt_skin_font.AllowDrop = true;
            this.txt_skin_font.Location = new System.Drawing.Point(8, 40);
            this.txt_skin_font.Name = "txt_skin_font";
            this.txt_skin_font.Size = new System.Drawing.Size(436, 24);
            this.txt_skin_font.TabIndex = 11;
            this.txt_skin_font.DragDrop += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragDrop);
            this.txt_skin_font.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragEnter);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.btn_next4);
            this.tabPage4.Location = new System.Drawing.Point(4, 27);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(533, 296);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "4.Dll\'s";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btn_next4
            // 
            this.btn_next4.Image = global::MpeMaker.Properties.Resources.go_next;
            this.btn_next4.Location = new System.Drawing.Point(469, 237);
            this.btn_next4.Name = "btn_next4";
            this.btn_next4.Size = new System.Drawing.Size(56, 51);
            this.btn_next4.TabIndex = 1;
            this.btn_next4.UseVisualStyleBackColor = true;
            this.btn_next4.Click += new System.EventHandler(this.btn_next1_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Location = new System.Drawing.Point(4, 27);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(533, 296);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "5.Done";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // WizardSkinSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(541, 327);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WizardSkinSelect";
            this.Text = "WizardSkinSelect";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txt_skinpath;
        private System.Windows.Forms.Button btn_browse_skin;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btn_next1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_browse_font;
        private System.Windows.Forms.TextBox txt_skin_font;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Button btn_next2;
        private System.Windows.Forms.Button btn_next3;
        private System.Windows.Forms.Button btn_next4;
    }
}