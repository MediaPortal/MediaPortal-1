namespace MpeMaker.Sections
{
    partial class RequirementsSection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RequirementsSection));
            this.list_versions = new System.Windows.Forms.ListBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.mnu_add = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnu_del = new System.Windows.Forms.ToolStripButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_name = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_message = new System.Windows.Forms.TextBox();
            this.chk_warn = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txt_version4_max = new System.Windows.Forms.TextBox();
            this.txt_version3_max = new System.Windows.Forms.TextBox();
            this.txt_version2_max = new System.Windows.Forms.TextBox();
            this.txt_version1_max = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txt_version4_min = new System.Windows.Forms.TextBox();
            this.txt_version3_min = new System.Windows.Forms.TextBox();
            this.txt_version2_min = new System.Windows.Forms.TextBox();
            this.txt_version1_min = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_id = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmb_type = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // list_versions
            // 
            this.list_versions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.list_versions.FormattingEnabled = true;
            this.list_versions.Location = new System.Drawing.Point(3, 28);
            this.list_versions.Name = "list_versions";
            this.list_versions.Size = new System.Drawing.Size(291, 342);
            this.list_versions.TabIndex = 0;
            this.list_versions.SelectedIndexChanged += new System.EventHandler(this.list_versions_SelectedIndexChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu_add,
            this.mnu_del});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(64, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // mnu_add
            // 
            this.mnu_add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnu_add.Image = ((System.Drawing.Image)(resources.GetObject("mnu_add.Image")));
            this.mnu_add.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnu_add.Name = "mnu_add";
            this.mnu_add.Size = new System.Drawing.Size(29, 22);
            this.mnu_add.Text = "toolStripDropDownButton1";
            // 
            // mnu_del
            // 
            this.mnu_del.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnu_del.Image = ((System.Drawing.Image)(resources.GetObject("mnu_del.Image")));
            this.mnu_del.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnu_del.Name = "mnu_del";
            this.mnu_del.Size = new System.Drawing.Size(23, 22);
            this.mnu_del.Text = "toolStripButton2";
            this.mnu_del.Click += new System.EventHandler(this.mnu_del_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txt_name);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txt_message);
            this.groupBox1.Controls.Add(this.chk_warn);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txt_id);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cmb_type);
            this.groupBox1.Location = new System.Drawing.Point(305, 28);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(384, 341);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Name";
            // 
            // txt_name
            // 
            this.txt_name.Location = new System.Drawing.Point(43, 63);
            this.txt_name.Name = "txt_name";
            this.txt_name.Size = new System.Drawing.Size(305, 20);
            this.txt_name.TabIndex = 10;
            this.toolTip1.SetToolTip(this.txt_name, "Name of extension");
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(357, 37);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(21, 20);
            this.button1.TabIndex = 9;
            this.button1.Text = "...";
            this.toolTip1.SetToolTip(this.button1, "Browse for installed extension");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 213);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Message";
            // 
            // txt_message
            // 
            this.txt_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_message.Location = new System.Drawing.Point(42, 229);
            this.txt_message.Multiline = true;
            this.txt_message.Name = "txt_message";
            this.txt_message.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_message.Size = new System.Drawing.Size(301, 106);
            this.txt_message.TabIndex = 7;
            this.toolTip1.SetToolTip(this.txt_message, "The message show if the version condition not met");
            this.txt_message.TextChanged += new System.EventHandler(this.txt_id_TextChanged);
            // 
            // chk_warn
            // 
            this.chk_warn.AutoSize = true;
            this.chk_warn.Location = new System.Drawing.Point(42, 193);
            this.chk_warn.Name = "chk_warn";
            this.chk_warn.Size = new System.Drawing.Size(74, 17);
            this.chk_warn.TabIndex = 6;
            this.chk_warn.Text = "Warn only";
            this.toolTip1.SetToolTip(this.chk_warn, "If cheked only a warning messsage will be displayed\r\nbut the installation will co" +
                    "ntinue");
            this.chk_warn.UseVisualStyleBackColor = true;
            this.chk_warn.CheckedChanged += new System.EventHandler(this.txt_id_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txt_version4_max);
            this.groupBox3.Controls.Add(this.txt_version3_max);
            this.groupBox3.Controls.Add(this.txt_version2_max);
            this.groupBox3.Controls.Add(this.txt_version1_max);
            this.groupBox3.Location = new System.Drawing.Point(42, 138);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(301, 49);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Max Version";
            // 
            // txt_version4_max
            // 
            this.txt_version4_max.Location = new System.Drawing.Point(217, 19);
            this.txt_version4_max.Name = "txt_version4_max";
            this.txt_version4_max.Size = new System.Drawing.Size(61, 20);
            this.txt_version4_max.TabIndex = 12;
            this.txt_version4_max.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txt_version4_max, "Any version number can be replaced with * ");
            this.txt_version4_max.TextChanged += new System.EventHandler(this.txt_id_TextChanged);
            this.txt_version4_max.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_min_KeyDown);
            // 
            // txt_version3_max
            // 
            this.txt_version3_max.Location = new System.Drawing.Point(150, 19);
            this.txt_version3_max.Name = "txt_version3_max";
            this.txt_version3_max.Size = new System.Drawing.Size(61, 20);
            this.txt_version3_max.TabIndex = 11;
            this.txt_version3_max.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txt_version3_max, "Any version number can be replaced with * ");
            this.txt_version3_max.TextChanged += new System.EventHandler(this.txt_id_TextChanged);
            this.txt_version3_max.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_min_KeyDown);
            // 
            // txt_version2_max
            // 
            this.txt_version2_max.Location = new System.Drawing.Point(86, 19);
            this.txt_version2_max.Name = "txt_version2_max";
            this.txt_version2_max.Size = new System.Drawing.Size(61, 20);
            this.txt_version2_max.TabIndex = 10;
            this.txt_version2_max.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txt_version2_max, "Any version number can be replaced with * ");
            this.txt_version2_max.TextChanged += new System.EventHandler(this.txt_id_TextChanged);
            this.txt_version2_max.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_min_KeyDown);
            // 
            // txt_version1_max
            // 
            this.txt_version1_max.Location = new System.Drawing.Point(22, 19);
            this.txt_version1_max.Name = "txt_version1_max";
            this.txt_version1_max.Size = new System.Drawing.Size(61, 20);
            this.txt_version1_max.TabIndex = 9;
            this.txt_version1_max.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txt_version1_max, "Any version number can be replaced with * ");
            this.txt_version1_max.TextChanged += new System.EventHandler(this.txt_id_TextChanged);
            this.txt_version1_max.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_min_KeyDown);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txt_version4_min);
            this.groupBox2.Controls.Add(this.txt_version3_min);
            this.groupBox2.Controls.Add(this.txt_version2_min);
            this.groupBox2.Controls.Add(this.txt_version1_min);
            this.groupBox2.Location = new System.Drawing.Point(42, 89);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(301, 43);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Min Version";
            // 
            // txt_version4_min
            // 
            this.txt_version4_min.Location = new System.Drawing.Point(217, 19);
            this.txt_version4_min.Name = "txt_version4_min";
            this.txt_version4_min.Size = new System.Drawing.Size(61, 20);
            this.txt_version4_min.TabIndex = 12;
            this.txt_version4_min.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txt_version4_min, "Any version number can be replaced with * ");
            this.txt_version4_min.TextChanged += new System.EventHandler(this.txt_id_TextChanged);
            this.txt_version4_min.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_min_KeyDown);
            // 
            // txt_version3_min
            // 
            this.txt_version3_min.Location = new System.Drawing.Point(150, 19);
            this.txt_version3_min.Name = "txt_version3_min";
            this.txt_version3_min.Size = new System.Drawing.Size(61, 20);
            this.txt_version3_min.TabIndex = 11;
            this.txt_version3_min.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txt_version3_min, "Any version number can be replaced with * ");
            this.txt_version3_min.TextChanged += new System.EventHandler(this.txt_id_TextChanged);
            this.txt_version3_min.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_min_KeyDown);
            // 
            // txt_version2_min
            // 
            this.txt_version2_min.Location = new System.Drawing.Point(86, 19);
            this.txt_version2_min.Name = "txt_version2_min";
            this.txt_version2_min.Size = new System.Drawing.Size(61, 20);
            this.txt_version2_min.TabIndex = 10;
            this.txt_version2_min.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txt_version2_min, "Any version number can be replaced with * ");
            this.txt_version2_min.TextChanged += new System.EventHandler(this.txt_id_TextChanged);
            this.txt_version2_min.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_min_KeyDown);
            // 
            // txt_version1_min
            // 
            this.txt_version1_min.Location = new System.Drawing.Point(22, 19);
            this.txt_version1_min.Name = "txt_version1_min";
            this.txt_version1_min.Size = new System.Drawing.Size(61, 20);
            this.txt_version1_min.TabIndex = 9;
            this.txt_version1_min.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txt_version1_min, "Any version number can be replaced with * ");
            this.txt_version1_min.TextChanged += new System.EventHandler(this.txt_id_TextChanged);
            this.txt_version1_min.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_version1_min_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(16, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Id";
            // 
            // txt_id
            // 
            this.txt_id.Location = new System.Drawing.Point(43, 37);
            this.txt_id.Name = "txt_id";
            this.txt_id.Size = new System.Drawing.Size(305, 20);
            this.txt_id.TabIndex = 2;
            this.toolTip1.SetToolTip(this.txt_id, "Used only if the type is Extension");
            this.txt_id.TextChanged += new System.EventHandler(this.txt_id_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Type";
            // 
            // cmb_type
            // 
            this.cmb_type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_type.FormattingEnabled = true;
            this.cmb_type.Location = new System.Drawing.Point(43, 10);
            this.cmb_type.Name = "cmb_type";
            this.cmb_type.Size = new System.Drawing.Size(305, 21);
            this.cmb_type.TabIndex = 0;
            this.cmb_type.SelectedIndexChanged += new System.EventHandler(this.txt_id_TextChanged);
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            // 
            // RequirementsSection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.list_versions);
            this.Name = "RequirementsSection";
            this.Size = new System.Drawing.Size(700, 373);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox list_versions;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton mnu_add;
        private System.Windows.Forms.ToolStripButton mnu_del;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_id;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_type;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txt_version4_min;
        private System.Windows.Forms.TextBox txt_version3_min;
        private System.Windows.Forms.TextBox txt_version2_min;
        private System.Windows.Forms.TextBox txt_version1_min;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txt_version4_max;
        private System.Windows.Forms.TextBox txt_version3_max;
        private System.Windows.Forms.TextBox txt_version2_max;
        private System.Windows.Forms.TextBox txt_version1_max;
        private System.Windows.Forms.TextBox txt_message;
        private System.Windows.Forms.CheckBox chk_warn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txt_name;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
