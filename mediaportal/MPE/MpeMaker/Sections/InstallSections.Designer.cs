namespace MpeMaker.Sections
{
    partial class InstallSections
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallSections));
            this.listBox_sections = new System.Windows.Forms.ListBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.mnu_add = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnu_remove = new System.Windows.Forms.ToolStripButton();
            this.btn_up = new System.Windows.Forms.Button();
            this.btn_down = new System.Windows.Forms.Button();
            this.list_groups = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmb_grupvisibility = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_guid = new System.Windows.Forms.TextBox();
            this.btn_preview = new System.Windows.Forms.Button();
            this.btn_params = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmb_sectiontype = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_name = new System.Windows.Forms.TextBox();
            this.toolStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox_sections
            // 
            this.listBox_sections.FormattingEnabled = true;
            this.listBox_sections.Location = new System.Drawing.Point(0, 26);
            this.listBox_sections.Name = "listBox_sections";
            this.listBox_sections.Size = new System.Drawing.Size(192, 394);
            this.listBox_sections.TabIndex = 0;
            this.listBox_sections.SelectedIndexChanged += new System.EventHandler(this.listBox_sections_SelectedIndexChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu_add,
            this.mnu_remove});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(95, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // mnu_add
            // 
            this.mnu_add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnu_add.Image = ((System.Drawing.Image)(resources.GetObject("mnu_add.Image")));
            this.mnu_add.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnu_add.Name = "mnu_add";
            this.mnu_add.Size = new System.Drawing.Size(29, 22);
            this.mnu_add.Text = "toolStripButton1";
            // 
            // mnu_remove
            // 
            this.mnu_remove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnu_remove.Image = ((System.Drawing.Image)(resources.GetObject("mnu_remove.Image")));
            this.mnu_remove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnu_remove.Name = "mnu_remove";
            this.mnu_remove.Size = new System.Drawing.Size(23, 22);
            this.mnu_remove.Text = "toolStripButton1";
            this.mnu_remove.Click += new System.EventHandler(this.mnu_remove_Click);
            // 
            // btn_up
            // 
            this.btn_up.Image = ((System.Drawing.Image)(resources.GetObject("btn_up.Image")));
            this.btn_up.Location = new System.Drawing.Point(198, 170);
            this.btn_up.Name = "btn_up";
            this.btn_up.Size = new System.Drawing.Size(23, 31);
            this.btn_up.TabIndex = 3;
            this.btn_up.UseVisualStyleBackColor = true;
            this.btn_up.Click += new System.EventHandler(this.btn_up_Click);
            // 
            // btn_down
            // 
            this.btn_down.Image = ((System.Drawing.Image)(resources.GetObject("btn_down.Image")));
            this.btn_down.Location = new System.Drawing.Point(198, 207);
            this.btn_down.Name = "btn_down";
            this.btn_down.Size = new System.Drawing.Size(23, 31);
            this.btn_down.TabIndex = 4;
            this.btn_down.UseVisualStyleBackColor = true;
            this.btn_down.Click += new System.EventHandler(this.btn_down_Click);
            // 
            // list_groups
            // 
            this.list_groups.FormattingEnabled = true;
            this.list_groups.Location = new System.Drawing.Point(239, 236);
            this.list_groups.Name = "list_groups";
            this.list_groups.Size = new System.Drawing.Size(277, 184);
            this.list_groups.TabIndex = 5;
            this.list_groups.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.list_groups_ItemCheck);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(236, 219);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Included file groups";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cmb_grupvisibility);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txt_guid);
            this.groupBox1.Controls.Add(this.btn_preview);
            this.groupBox1.Controls.Add(this.btn_params);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cmb_sectiontype);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txt_name);
            this.groupBox1.Location = new System.Drawing.Point(239, 26);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(276, 190);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 97);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Visible ";
            // 
            // cmb_grupvisibility
            // 
            this.cmb_grupvisibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_grupvisibility.FormattingEnabled = true;
            this.cmb_grupvisibility.Location = new System.Drawing.Point(88, 94);
            this.cmb_grupvisibility.Name = "cmb_grupvisibility";
            this.cmb_grupvisibility.Size = new System.Drawing.Size(182, 21);
            this.cmb_grupvisibility.TabIndex = 8;
            this.cmb_grupvisibility.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Section GUID";
            // 
            // txt_guid
            // 
            this.txt_guid.Location = new System.Drawing.Point(87, 13);
            this.txt_guid.Name = "txt_guid";
            this.txt_guid.ReadOnly = true;
            this.txt_guid.Size = new System.Drawing.Size(183, 20);
            this.txt_guid.TabIndex = 6;
            // 
            // btn_preview
            // 
            this.btn_preview.Location = new System.Drawing.Point(171, 152);
            this.btn_preview.Name = "btn_preview";
            this.btn_preview.Size = new System.Drawing.Size(75, 23);
            this.btn_preview.TabIndex = 5;
            this.btn_preview.Text = "Preview";
            this.btn_preview.UseVisualStyleBackColor = true;
            this.btn_preview.Click += new System.EventHandler(this.btn_preview_Click);
            // 
            // btn_params
            // 
            this.btn_params.Location = new System.Drawing.Point(21, 152);
            this.btn_params.Name = "btn_params";
            this.btn_params.Size = new System.Drawing.Size(75, 23);
            this.btn_params.TabIndex = 4;
            this.btn_params.Text = "Edit params";
            this.btn_params.UseVisualStyleBackColor = true;
            this.btn_params.Click += new System.EventHandler(this.btn_params_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Section type";
            // 
            // cmb_sectiontype
            // 
            this.cmb_sectiontype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_sectiontype.Enabled = false;
            this.cmb_sectiontype.FormattingEnabled = true;
            this.cmb_sectiontype.Location = new System.Drawing.Point(87, 65);
            this.cmb_sectiontype.Name = "cmb_sectiontype";
            this.cmb_sectiontype.Size = new System.Drawing.Size(183, 21);
            this.cmb_sectiontype.TabIndex = 2;
            this.cmb_sectiontype.SelectedIndexChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Section name";
            // 
            // txt_name
            // 
            this.txt_name.Location = new System.Drawing.Point(87, 39);
            this.txt_name.Name = "txt_name";
            this.txt_name.Size = new System.Drawing.Size(183, 20);
            this.txt_name.TabIndex = 0;
            this.txt_name.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // InstallSections
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.list_groups);
            this.Controls.Add(this.btn_down);
            this.Controls.Add(this.btn_up);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listBox_sections);
            this.Name = "InstallSections";
            this.Size = new System.Drawing.Size(683, 427);
            this.Load += new System.EventHandler(this.InstallSections_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox_sections;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton mnu_remove;
        private System.Windows.Forms.Button btn_up;
        private System.Windows.Forms.Button btn_down;
        private System.Windows.Forms.CheckedListBox list_groups;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripDropDownButton mnu_add;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_name;
        private System.Windows.Forms.Button btn_preview;
        private System.Windows.Forms.Button btn_params;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmb_sectiontype;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txt_guid;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmb_grupvisibility;
    }
}
