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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallSections));
            this.listBox_sections = new System.Windows.Forms.ListBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.mnu_add = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnu_remove = new System.Windows.Forms.ToolStripButton();
            this.list_groups = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmb_buttons = new System.Windows.Forms.ComboBox();
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btn_group_down = new System.Windows.Forms.Button();
            this.btn_group_up = new System.Windows.Forms.Button();
            this.mnu_group_remove = new System.Windows.Forms.ToolStrip();
            this.mnu_groulist = new System.Windows.Forms.ToolStripComboBox();
            this.mnu_group_add = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.list_actions = new System.Windows.Forms.ListBox();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.mnu_action_add = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnu_action_del = new System.Windows.Forms.ToolStripButton();
            this.mnu_action_edit = new System.Windows.Forms.ToolStripButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btn_down = new System.Windows.Forms.Button();
            this.btn_up = new System.Windows.Forms.Button();
            this.toolStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.mnu_group_remove.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox_sections
            // 
            this.listBox_sections.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox_sections.FormattingEnabled = true;
            this.listBox_sections.Location = new System.Drawing.Point(0, 26);
            this.listBox_sections.Name = "listBox_sections";
            this.listBox_sections.Size = new System.Drawing.Size(254, 394);
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
            this.toolStrip1.Size = new System.Drawing.Size(64, 25);
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
            // list_groups
            // 
            this.list_groups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.list_groups.FormattingEnabled = true;
            this.list_groups.Location = new System.Drawing.Point(3, 33);
            this.list_groups.Name = "list_groups";
            this.list_groups.Size = new System.Drawing.Size(272, 134);
            this.list_groups.TabIndex = 5;
            this.toolTip1.SetToolTip(this.list_groups, "Check the included group item on this section,\r\nnot all sections need groups. ");
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cmb_buttons);
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
            this.groupBox1.Location = new System.Drawing.Point(301, 26);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(315, 190);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 124);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Buttons";
            // 
            // cmb_buttons
            // 
            this.cmb_buttons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmb_buttons.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_buttons.FormattingEnabled = true;
            this.cmb_buttons.Items.AddRange(new object[] {
            " Back/Next/Cancel ",
            " Next/Cancel ",
            " Back/Finish",
            " Cancel ",
            " Next ",
            " Finish "});
            this.cmb_buttons.Location = new System.Drawing.Point(87, 121);
            this.cmb_buttons.Name = "cmb_buttons";
            this.cmb_buttons.Size = new System.Drawing.Size(222, 21);
            this.cmb_buttons.TabIndex = 10;
            this.cmb_buttons.SelectedIndexChanged += new System.EventHandler(this.txt_name_TextChanged);
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
            this.cmb_grupvisibility.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmb_grupvisibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_grupvisibility.FormattingEnabled = true;
            this.cmb_grupvisibility.Location = new System.Drawing.Point(87, 94);
            this.cmb_grupvisibility.Name = "cmb_grupvisibility";
            this.cmb_grupvisibility.Size = new System.Drawing.Size(222, 21);
            this.cmb_grupvisibility.TabIndex = 8;
            this.toolTip1.SetToolTip(this.cmb_grupvisibility, "If the selected group is checked, the section will be shown\r\nIf no group specifie" +
                    "d the section always visible");
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
            this.txt_guid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_guid.Location = new System.Drawing.Point(87, 13);
            this.txt_guid.Name = "txt_guid";
            this.txt_guid.ReadOnly = true;
            this.txt_guid.Size = new System.Drawing.Size(222, 20);
            this.txt_guid.TabIndex = 6;
            // 
            // btn_preview
            // 
            this.btn_preview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_preview.Location = new System.Drawing.Point(219, 161);
            this.btn_preview.Name = "btn_preview";
            this.btn_preview.Size = new System.Drawing.Size(75, 23);
            this.btn_preview.TabIndex = 5;
            this.btn_preview.Text = "Preview";
            this.btn_preview.UseVisualStyleBackColor = true;
            this.btn_preview.Click += new System.EventHandler(this.btn_preview_Click);
            // 
            // btn_params
            // 
            this.btn_params.Location = new System.Drawing.Point(20, 161);
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
            this.cmb_sectiontype.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmb_sectiontype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_sectiontype.Enabled = false;
            this.cmb_sectiontype.FormattingEnabled = true;
            this.cmb_sectiontype.Location = new System.Drawing.Point(87, 65);
            this.cmb_sectiontype.Name = "cmb_sectiontype";
            this.cmb_sectiontype.Size = new System.Drawing.Size(222, 21);
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
            this.txt_name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_name.Location = new System.Drawing.Point(87, 39);
            this.txt_name.Name = "txt_name";
            this.txt_name.Size = new System.Drawing.Size(222, 20);
            this.txt_name.TabIndex = 0;
            this.txt_name.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(301, 222);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(315, 197);
            this.tabControl1.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btn_group_down);
            this.tabPage1.Controls.Add(this.btn_group_up);
            this.tabPage1.Controls.Add(this.mnu_group_remove);
            this.tabPage1.Controls.Add(this.list_groups);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(307, 171);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Groups";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btn_group_down
            // 
            this.btn_group_down.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_group_down.Image = ((System.Drawing.Image)(resources.GetObject("btn_group_down.Image")));
            this.btn_group_down.Location = new System.Drawing.Point(278, 99);
            this.btn_group_down.Name = "btn_group_down";
            this.btn_group_down.Size = new System.Drawing.Size(23, 31);
            this.btn_group_down.TabIndex = 8;
            this.btn_group_down.UseVisualStyleBackColor = true;
            this.btn_group_down.Click += new System.EventHandler(this.btn_group_down_Click);
            // 
            // btn_group_up
            // 
            this.btn_group_up.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_group_up.Image = ((System.Drawing.Image)(resources.GetObject("btn_group_up.Image")));
            this.btn_group_up.Location = new System.Drawing.Point(278, 62);
            this.btn_group_up.Name = "btn_group_up";
            this.btn_group_up.Size = new System.Drawing.Size(23, 31);
            this.btn_group_up.TabIndex = 7;
            this.btn_group_up.UseVisualStyleBackColor = true;
            this.btn_group_up.Click += new System.EventHandler(this.btn_group_up_Click);
            // 
            // mnu_group_remove
            // 
            this.mnu_group_remove.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu_groulist,
            this.mnu_group_add,
            this.toolStripButton2});
            this.mnu_group_remove.Location = new System.Drawing.Point(3, 3);
            this.mnu_group_remove.Name = "mnu_group_remove";
            this.mnu_group_remove.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.mnu_group_remove.Size = new System.Drawing.Size(301, 25);
            this.mnu_group_remove.TabIndex = 6;
            this.mnu_group_remove.Text = "toolStrip3";
            // 
            // mnu_groulist
            // 
            this.mnu_groulist.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mnu_groulist.Name = "mnu_groulist";
            this.mnu_groulist.Size = new System.Drawing.Size(200, 25);
            // 
            // mnu_group_add
            // 
            this.mnu_group_add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnu_group_add.Image = global::MpeMaker.Properties.Resources.list_add;
            this.mnu_group_add.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnu_group_add.Name = "mnu_group_add";
            this.mnu_group_add.Size = new System.Drawing.Size(23, 22);
            this.mnu_group_add.Text = "toolStripButton1";
            this.mnu_group_add.Click += new System.EventHandler(this.mnu_group_add_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::MpeMaker.Properties.Resources.list_remove;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "toolStripButton2";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.list_actions);
            this.tabPage2.Controls.Add(this.toolStrip2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(307, 171);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Actions";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // list_actions
            // 
            this.list_actions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list_actions.FormattingEnabled = true;
            this.list_actions.Location = new System.Drawing.Point(3, 28);
            this.list_actions.Name = "list_actions";
            this.list_actions.Size = new System.Drawing.Size(301, 134);
            this.list_actions.TabIndex = 1;
            this.toolTip1.SetToolTip(this.list_actions, "Atached actions to the section ");
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu_action_add,
            this.mnu_action_del,
            this.mnu_action_edit});
            this.toolStrip2.Location = new System.Drawing.Point(3, 3);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip2.Size = new System.Drawing.Size(301, 25);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // mnu_action_add
            // 
            this.mnu_action_add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnu_action_add.Image = ((System.Drawing.Image)(resources.GetObject("mnu_action_add.Image")));
            this.mnu_action_add.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnu_action_add.Name = "mnu_action_add";
            this.mnu_action_add.Size = new System.Drawing.Size(29, 22);
            this.mnu_action_add.Text = "toolStripButton1";
            this.mnu_action_add.ToolTipText = "Add action";
            // 
            // mnu_action_del
            // 
            this.mnu_action_del.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnu_action_del.Image = ((System.Drawing.Image)(resources.GetObject("mnu_action_del.Image")));
            this.mnu_action_del.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnu_action_del.Name = "mnu_action_del";
            this.mnu_action_del.Size = new System.Drawing.Size(23, 22);
            this.mnu_action_del.Text = "toolStripButton2";
            this.mnu_action_del.ToolTipText = "Remove action";
            this.mnu_action_del.Click += new System.EventHandler(this.mnu_action_del_Click);
            // 
            // mnu_action_edit
            // 
            this.mnu_action_edit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnu_action_edit.Image = ((System.Drawing.Image)(resources.GetObject("mnu_action_edit.Image")));
            this.mnu_action_edit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnu_action_edit.Name = "mnu_action_edit";
            this.mnu_action_edit.Size = new System.Drawing.Size(23, 22);
            this.mnu_action_edit.Text = "toolStripButton3";
            this.mnu_action_edit.ToolTipText = "Edit action";
            this.mnu_action_edit.Click += new System.EventHandler(this.mnu_action_edit_Click);
            // 
            // btn_down
            // 
            this.btn_down.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_down.Image = ((System.Drawing.Image)(resources.GetObject("btn_down.Image")));
            this.btn_down.Location = new System.Drawing.Point(260, 222);
            this.btn_down.Name = "btn_down";
            this.btn_down.Size = new System.Drawing.Size(23, 31);
            this.btn_down.TabIndex = 4;
            this.btn_down.UseVisualStyleBackColor = true;
            this.btn_down.Click += new System.EventHandler(this.btn_down_Click);
            // 
            // btn_up
            // 
            this.btn_up.Image = ((System.Drawing.Image)(resources.GetObject("btn_up.Image")));
            this.btn_up.Location = new System.Drawing.Point(260, 185);
            this.btn_up.Name = "btn_up";
            this.btn_up.Size = new System.Drawing.Size(23, 31);
            this.btn_up.TabIndex = 3;
            this.btn_up.UseVisualStyleBackColor = true;
            this.btn_up.Click += new System.EventHandler(this.btn_up_Click);
            // 
            // InstallSections
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBox1);
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
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.mnu_group_remove.ResumeLayout(false);
            this.mnu_group_remove.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox_sections;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton mnu_remove;
        private System.Windows.Forms.Button btn_up;
        private System.Windows.Forms.Button btn_down;
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
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton mnu_action_del;
        private System.Windows.Forms.ToolStripButton mnu_action_edit;
        private System.Windows.Forms.ListBox list_actions;
        private System.Windows.Forms.ToolStripDropDownButton mnu_action_add;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_buttons;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStrip mnu_group_remove;
        private System.Windows.Forms.ToolStripComboBox mnu_groulist;
        private System.Windows.Forms.ToolStripButton mnu_group_add;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ListBox list_groups;
        private System.Windows.Forms.Button btn_group_down;
        private System.Windows.Forms.Button btn_group_up;
    }
}
