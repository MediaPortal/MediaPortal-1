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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_name = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_guid = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_version1 = new System.Windows.Forms.TextBox();
            this.txt_version2 = new System.Windows.Forms.TextBox();
            this.txt_version3 = new System.Windows.Forms.TextBox();
            this.txt_version4 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_author = new System.Windows.Forms.TextBox();
            this.txt_homepage = new System.Windows.Forms.TextBox();
            this.cmb_status = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txt_forum = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txt_update = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txt_description = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txt_versiondesc = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.btn_gen_guid = new System.Windows.Forms.Button();
            this.txt_online = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btn_params = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.txt_tags = new System.Windows.Forms.TextBox();
            this.img_logo = new System.Windows.Forms.PictureBox();
            this.date_release = new System.Windows.Forms.DateTimePicker();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.img_logo)).BeginInit();
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
            this.txt_name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_name.Location = new System.Drawing.Point(115, 7);
            this.txt_name.Name = "txt_name";
            this.txt_name.Size = new System.Drawing.Size(258, 20);
            this.txt_name.TabIndex = 0;
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
            this.txt_guid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_guid.Location = new System.Drawing.Point(115, 33);
            this.txt_guid.Name = "txt_guid";
            this.txt_guid.Size = new System.Drawing.Size(236, 20);
            this.txt_guid.TabIndex = 1;
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
            this.txt_version1.Size = new System.Drawing.Size(61, 20);
            this.txt_version1.TabIndex = 2;
            this.txt_version1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_version1.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            this.txt_version1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_KeyDown);
            // 
            // txt_version2
            // 
            this.txt_version2.Location = new System.Drawing.Point(178, 59);
            this.txt_version2.Name = "txt_version2";
            this.txt_version2.Size = new System.Drawing.Size(61, 20);
            this.txt_version2.TabIndex = 3;
            this.txt_version2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_version2.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            this.txt_version2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_KeyDown);
            // 
            // txt_version3
            // 
            this.txt_version3.Location = new System.Drawing.Point(242, 59);
            this.txt_version3.Name = "txt_version3";
            this.txt_version3.Size = new System.Drawing.Size(61, 20);
            this.txt_version3.TabIndex = 4;
            this.txt_version3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_version3.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            this.txt_version3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_KeyDown);
            // 
            // txt_version4
            // 
            this.txt_version4.Location = new System.Drawing.Point(309, 59);
            this.txt_version4.Name = "txt_version4";
            this.txt_version4.Size = new System.Drawing.Size(61, 20);
            this.txt_version4.TabIndex = 5;
            this.txt_version4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_version4.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            this.txt_version4.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_KeyDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Author";
            // 
            // txt_author
            // 
            this.txt_author.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_author.Location = new System.Drawing.Point(114, 85);
            this.txt_author.Name = "txt_author";
            this.txt_author.Size = new System.Drawing.Size(259, 20);
            this.txt_author.TabIndex = 6;
            this.txt_author.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // txt_homepage
            // 
            this.txt_homepage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_homepage.Location = new System.Drawing.Point(115, 139);
            this.txt_homepage.Name = "txt_homepage";
            this.txt_homepage.Size = new System.Drawing.Size(258, 20);
            this.txt_homepage.TabIndex = 8;
            this.txt_homepage.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // cmb_status
            // 
            this.cmb_status.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmb_status.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_status.FormattingEnabled = true;
            this.cmb_status.Items.AddRange(new object[] {
            "Alpha",
            "Beta",
            "Rc",
            "Stable"});
            this.cmb_status.Location = new System.Drawing.Point(115, 111);
            this.cmb_status.Name = "cmb_status";
            this.cmb_status.Size = new System.Drawing.Size(258, 21);
            this.cmb_status.TabIndex = 7;
            this.cmb_status.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 114);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Status";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 142);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Home page";
            // 
            // txt_forum
            // 
            this.txt_forum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_forum.Location = new System.Drawing.Point(115, 165);
            this.txt_forum.Name = "txt_forum";
            this.txt_forum.Size = new System.Drawing.Size(258, 20);
            this.txt_forum.TabIndex = 9;
            this.txt_forum.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 168);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(50, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Forum url";
            // 
            // txt_update
            // 
            this.txt_update.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_update.Location = new System.Drawing.Point(115, 191);
            this.txt_update.Name = "txt_update";
            this.txt_update.Size = new System.Drawing.Size(258, 20);
            this.txt_update.TabIndex = 10;
            this.toolTip1.SetToolTip(this.txt_update, "Onlie lication if the xml file were the update if is stored");
            this.txt_update.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 194);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Update url";
            // 
            // txt_description
            // 
            this.txt_description.AcceptsReturn = true;
            this.txt_description.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_description.Location = new System.Drawing.Point(379, 26);
            this.txt_description.Multiline = true;
            this.txt_description.Name = "txt_description";
            this.txt_description.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_description.Size = new System.Drawing.Size(318, 211);
            this.txt_description.TabIndex = 14;
            this.txt_description.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(376, 10);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(107, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "Extension description";
            // 
            // txt_versiondesc
            // 
            this.txt_versiondesc.AcceptsReturn = true;
            this.txt_versiondesc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_versiondesc.Location = new System.Drawing.Point(379, 268);
            this.txt_versiondesc.Multiline = true;
            this.txt_versiondesc.Name = "txt_versiondesc";
            this.txt_versiondesc.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_versiondesc.Size = new System.Drawing.Size(318, 360);
            this.txt_versiondesc.TabIndex = 15;
            this.txt_versiondesc.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(379, 247);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(118, 13);
            this.label10.TabIndex = 22;
            this.label10.Text = "This version description";
            // 
            // btn_gen_guid
            // 
            this.btn_gen_guid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_gen_guid.Location = new System.Drawing.Point(357, 31);
            this.btn_gen_guid.Name = "btn_gen_guid";
            this.btn_gen_guid.Size = new System.Drawing.Size(16, 23);
            this.btn_gen_guid.TabIndex = 23;
            this.btn_gen_guid.Text = ".";
            this.btn_gen_guid.UseVisualStyleBackColor = true;
            this.btn_gen_guid.Click += new System.EventHandler(this.btn_gen_guid_Click);
            // 
            // txt_online
            // 
            this.txt_online.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_online.Location = new System.Drawing.Point(114, 217);
            this.txt_online.Name = "txt_online";
            this.txt_online.Size = new System.Drawing.Size(259, 20);
            this.txt_online.TabIndex = 11;
            this.toolTip1.SetToolTip(this.txt_online, "Online location of the package it self");
            this.txt_online.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(13, 220);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 13);
            this.label11.TabIndex = 25;
            this.label11.Text = "Online location";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btn_params
            // 
            this.btn_params.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_params.Location = new System.Drawing.Point(115, 634);
            this.btn_params.Name = "btn_params";
            this.btn_params.Size = new System.Drawing.Size(135, 23);
            this.btn_params.TabIndex = 16;
            this.btn_params.Text = "Aditional params";
            this.btn_params.UseVisualStyleBackColor = true;
            this.btn_params.Click += new System.EventHandler(this.btn_params_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            // 
            // txt_tags
            // 
            this.txt_tags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.txt_tags.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
            this.txt_tags.Location = new System.Drawing.Point(114, 269);
            this.txt_tags.Multiline = true;
            this.txt_tags.Name = "txt_tags";
            this.txt_tags.Size = new System.Drawing.Size(259, 359);
            this.txt_tags.TabIndex = 13;
            this.toolTip1.SetToolTip(this.txt_tags, "Tags separated by \",\"");
            this.txt_tags.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // img_logo
            // 
            this.img_logo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.img_logo.Location = new System.Drawing.Point(3, 593);
            this.img_logo.Name = "img_logo";
            this.img_logo.Size = new System.Drawing.Size(64, 64);
            this.img_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.img_logo.TabIndex = 26;
            this.img_logo.TabStop = false;
            // 
            // date_release
            // 
            this.date_release.Location = new System.Drawing.Point(114, 243);
            this.date_release.Name = "date_release";
            this.date_release.Size = new System.Drawing.Size(259, 20);
            this.date_release.TabIndex = 12;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(13, 247);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(70, 13);
            this.label12.TabIndex = 28;
            this.label12.Text = "Release date";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(13, 272);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(31, 13);
            this.label13.TabIndex = 30;
            this.label13.Text = "Tags";
            // 
            // GeneralSection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txt_tags);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.date_release);
            this.Controls.Add(this.img_logo);
            this.Controls.Add(this.btn_params);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txt_online);
            this.Controls.Add(this.btn_gen_guid);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txt_versiondesc);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txt_description);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txt_update);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txt_forum);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmb_status);
            this.Controls.Add(this.txt_homepage);
            this.Controls.Add(this.txt_author);
            this.Controls.Add(this.label4);
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
            this.Size = new System.Drawing.Size(700, 660);
            this.Load += new System.EventHandler(this.GeneralSection_Load);
            ((System.ComponentModel.ISupportInitialize)(this.img_logo)).EndInit();
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
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txt_author;
        private System.Windows.Forms.TextBox txt_homepage;
        private System.Windows.Forms.ComboBox cmb_status;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txt_forum;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txt_update;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txt_description;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txt_versiondesc;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btn_gen_guid;
        private System.Windows.Forms.TextBox txt_online;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btn_params;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PictureBox img_logo;
        private System.Windows.Forms.DateTimePicker date_release;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txt_tags;
        private System.Windows.Forms.Label label13;
    }
}
