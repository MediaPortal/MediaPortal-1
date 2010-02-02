namespace MpeMaker.Sections
{
    partial class FilesGroupsSection
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
          this.imageList = new System.Windows.Forms.ImageList(this.components);
          this.toolStrip = new System.Windows.Forms.ToolStrip();
          this.mnu_add_group = new System.Windows.Forms.ToolStripButton();
          this.mnu_remove_group = new System.Windows.Forms.ToolStripButton();
          this.toolStripButton3 = new System.Windows.Forms.ToolStripSeparator();
          this.mnu_add_files = new System.Windows.Forms.ToolStripButton();
          this.mnu_add_folder = new System.Windows.Forms.ToolStripButton();
          this.mnu_remove_files = new System.Windows.Forms.ToolStripButton();
          this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
          this.tabControl1 = new System.Windows.Forms.TabControl();
          this.tabPage_group = new System.Windows.Forms.TabPage();
          this.btn_clearat = new System.Windows.Forms.Button();
          this.label8 = new System.Windows.Forms.Label();
          this.list_folder = new System.Windows.Forms.ListBox();
          this.label5 = new System.Windows.Forms.Label();
          this.cmb_parentGroup = new System.Windows.Forms.ComboBox();
          this.txt_displlayName = new System.Windows.Forms.TextBox();
          this.label4 = new System.Windows.Forms.Label();
          this.chk_default = new System.Windows.Forms.CheckBox();
          this.label3 = new System.Windows.Forms.Label();
          this.txt_description = new System.Windows.Forms.TextBox();
          this.tabPage_file = new System.Windows.Forms.TabPage();
          this.label7 = new System.Windows.Forms.Label();
          this.txt_param1 = new System.Windows.Forms.TextBox();
          this.btn_set = new System.Windows.Forms.Button();
          this.label6 = new System.Windows.Forms.Label();
          this.cmb_overwrite = new System.Windows.Forms.ComboBox();
          this.button1 = new System.Windows.Forms.Button();
          this.label2 = new System.Windows.Forms.Label();
          this.txt_installpath = new System.Windows.Forms.TextBox();
          this.label1 = new System.Windows.Forms.Label();
          this.cmb_installtype = new System.Windows.Forms.ComboBox();
          this.toolTip = new System.Windows.Forms.ToolTip(this.components);
          this.treeView1 = new CodersLab.Windows.Controls.TreeView();
          this.toolStrip.SuspendLayout();
          this.tabControl1.SuspendLayout();
          this.tabPage_group.SuspendLayout();
          this.tabPage_file.SuspendLayout();
          this.SuspendLayout();
          // 
          // imageList
          // 
          this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
          this.imageList.ImageSize = new System.Drawing.Size(16, 16);
          this.imageList.TransparentColor = System.Drawing.Color.Transparent;
          // 
          // toolStrip
          // 
          this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu_add_group,
            this.mnu_remove_group,
            this.toolStripButton3,
            this.mnu_add_files,
            this.mnu_add_folder,
            this.mnu_remove_files});
          this.toolStrip.Location = new System.Drawing.Point(0, 0);
          this.toolStrip.Name = "toolStrip";
          this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
          this.toolStrip.Size = new System.Drawing.Size(702, 25);
          this.toolStrip.TabIndex = 2;
          this.toolStrip.Text = "toolStrip1";
          // 
          // mnu_add_group
          // 
          this.mnu_add_group.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.mnu_add_group.Image = global::MpeMaker.Properties.Resources.folder_add;
          this.mnu_add_group.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.mnu_add_group.Name = "mnu_add_group";
          this.mnu_add_group.Size = new System.Drawing.Size(23, 22);
          this.mnu_add_group.Text = "toolStripButton1";
          this.mnu_add_group.ToolTipText = "Add group";
          this.mnu_add_group.Click += new System.EventHandler(this.mnu_add_group_Click);
          // 
          // mnu_remove_group
          // 
          this.mnu_remove_group.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.mnu_remove_group.Image = global::MpeMaker.Properties.Resources.folder_delete;
          this.mnu_remove_group.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.mnu_remove_group.Name = "mnu_remove_group";
          this.mnu_remove_group.Size = new System.Drawing.Size(23, 22);
          this.mnu_remove_group.Text = "Remove group";
          this.mnu_remove_group.Click += new System.EventHandler(this.mnu_remove_group_Click);
          // 
          // toolStripButton3
          // 
          this.toolStripButton3.Name = "toolStripButton3";
          this.toolStripButton3.Size = new System.Drawing.Size(6, 25);
          // 
          // mnu_add_files
          // 
          this.mnu_add_files.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.mnu_add_files.Image = global::MpeMaker.Properties.Resources.page_add;
          this.mnu_add_files.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.mnu_add_files.Name = "mnu_add_files";
          this.mnu_add_files.Size = new System.Drawing.Size(23, 22);
          this.mnu_add_files.Text = "Add files";
          this.mnu_add_files.Click += new System.EventHandler(this.mnu_add_files_Click);
          // 
          // mnu_add_folder
          // 
          this.mnu_add_folder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.mnu_add_folder.Image = global::MpeMaker.Properties.Resources.folder_page;
          this.mnu_add_folder.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.mnu_add_folder.Name = "mnu_add_folder";
          this.mnu_add_folder.Size = new System.Drawing.Size(23, 22);
          this.mnu_add_folder.Text = "toolStripButton1";
          this.mnu_add_folder.ToolTipText = "Add folder";
          this.mnu_add_folder.Click += new System.EventHandler(this.mnu_add_folder_Click);
          // 
          // mnu_remove_files
          // 
          this.mnu_remove_files.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.mnu_remove_files.Image = global::MpeMaker.Properties.Resources.page_delete;
          this.mnu_remove_files.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.mnu_remove_files.Name = "mnu_remove_files";
          this.mnu_remove_files.Size = new System.Drawing.Size(23, 22);
          this.mnu_remove_files.Text = "Remove files";
          this.mnu_remove_files.Click += new System.EventHandler(this.mnu_remove_files_Click);
          // 
          // openFileDialog
          // 
          this.openFileDialog.FileName = "openFileDialog1";
          // 
          // tabControl1
          // 
          this.tabControl1.Controls.Add(this.tabPage_group);
          this.tabControl1.Controls.Add(this.tabPage_file);
          this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
          this.tabControl1.Location = new System.Drawing.Point(342, 25);
          this.tabControl1.Name = "tabControl1";
          this.tabControl1.SelectedIndex = 0;
          this.tabControl1.Size = new System.Drawing.Size(360, 403);
          this.tabControl1.TabIndex = 3;
          // 
          // tabPage_group
          // 
          this.tabPage_group.Controls.Add(this.btn_clearat);
          this.tabPage_group.Controls.Add(this.label8);
          this.tabPage_group.Controls.Add(this.list_folder);
          this.tabPage_group.Controls.Add(this.label5);
          this.tabPage_group.Controls.Add(this.cmb_parentGroup);
          this.tabPage_group.Controls.Add(this.txt_displlayName);
          this.tabPage_group.Controls.Add(this.label4);
          this.tabPage_group.Controls.Add(this.chk_default);
          this.tabPage_group.Controls.Add(this.label3);
          this.tabPage_group.Controls.Add(this.txt_description);
          this.tabPage_group.Location = new System.Drawing.Point(4, 22);
          this.tabPage_group.Name = "tabPage_group";
          this.tabPage_group.Padding = new System.Windows.Forms.Padding(3);
          this.tabPage_group.Size = new System.Drawing.Size(352, 377);
          this.tabPage_group.TabIndex = 0;
          this.tabPage_group.Text = "Group";
          this.tabPage_group.UseVisualStyleBackColor = true;
          // 
          // btn_clearat
          // 
          this.btn_clearat.Location = new System.Drawing.Point(6, 289);
          this.btn_clearat.Name = "btn_clearat";
          this.btn_clearat.Size = new System.Drawing.Size(142, 23);
          this.btn_clearat.TabIndex = 10;
          this.btn_clearat.Text = "Clear attached folders";
          this.btn_clearat.UseVisualStyleBackColor = true;
          this.btn_clearat.Click += new System.EventHandler(this.btn_clearat_Click);
          // 
          // label8
          // 
          this.label8.AutoSize = true;
          this.label8.Location = new System.Drawing.Point(6, 185);
          this.label8.Name = "label8";
          this.label8.Size = new System.Drawing.Size(84, 13);
          this.label8.TabIndex = 9;
          this.label8.Text = "Attached folders";
          // 
          // list_folder
          // 
          this.list_folder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.list_folder.FormattingEnabled = true;
          this.list_folder.Location = new System.Drawing.Point(6, 201);
          this.list_folder.Name = "list_folder";
          this.list_folder.Size = new System.Drawing.Size(340, 82);
          this.list_folder.TabIndex = 8;
          this.toolTip.SetToolTip(this.list_folder, "All files in this directory will be recursively added to this group.");
          // 
          // label5
          // 
          this.label5.AutoSize = true;
          this.label5.Location = new System.Drawing.Point(6, 130);
          this.label5.Name = "label5";
          this.label5.Size = new System.Drawing.Size(68, 13);
          this.label5.TabIndex = 7;
          this.label5.Text = "Parent group";
          // 
          // cmb_parentGroup
          // 
          this.cmb_parentGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.cmb_parentGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.cmb_parentGroup.FormattingEnabled = true;
          this.cmb_parentGroup.Location = new System.Drawing.Point(80, 127);
          this.cmb_parentGroup.Name = "cmb_parentGroup";
          this.cmb_parentGroup.Size = new System.Drawing.Size(266, 21);
          this.cmb_parentGroup.TabIndex = 6;
          this.cmb_parentGroup.SelectedIndexChanged += new System.EventHandler(this.txt_description_TextChanged);
          // 
          // txt_displlayName
          // 
          this.txt_displlayName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.txt_displlayName.Location = new System.Drawing.Point(6, 19);
          this.txt_displlayName.Name = "txt_displlayName";
          this.txt_displlayName.Size = new System.Drawing.Size(340, 20);
          this.txt_displlayName.TabIndex = 5;
          this.txt_displlayName.TextChanged += new System.EventHandler(this.txt_description_TextChanged);
          // 
          // label4
          // 
          this.label4.AutoSize = true;
          this.label4.Location = new System.Drawing.Point(6, 3);
          this.label4.Name = "label4";
          this.label4.Size = new System.Drawing.Size(70, 13);
          this.label4.TabIndex = 4;
          this.label4.Text = "Display name";
          // 
          // chk_default
          // 
          this.chk_default.AutoSize = true;
          this.chk_default.Location = new System.Drawing.Point(6, 154);
          this.chk_default.Name = "chk_default";
          this.chk_default.Size = new System.Drawing.Size(103, 17);
          this.chk_default.TabIndex = 2;
          this.chk_default.Text = "Default selected";
          this.chk_default.UseVisualStyleBackColor = true;
          this.chk_default.CheckedChanged += new System.EventHandler(this.txt_description_TextChanged);
          // 
          // label3
          // 
          this.label3.AutoSize = true;
          this.label3.Location = new System.Drawing.Point(6, 42);
          this.label3.Name = "label3";
          this.label3.Size = new System.Drawing.Size(60, 13);
          this.label3.TabIndex = 0;
          this.label3.Text = "Description";
          // 
          // txt_description
          // 
          this.txt_description.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.txt_description.Location = new System.Drawing.Point(6, 58);
          this.txt_description.Multiline = true;
          this.txt_description.Name = "txt_description";
          this.txt_description.Size = new System.Drawing.Size(340, 63);
          this.txt_description.TabIndex = 1;
          this.txt_description.TextChanged += new System.EventHandler(this.txt_description_TextChanged);
          // 
          // tabPage_file
          // 
          this.tabPage_file.Controls.Add(this.label7);
          this.tabPage_file.Controls.Add(this.txt_param1);
          this.tabPage_file.Controls.Add(this.btn_set);
          this.tabPage_file.Controls.Add(this.label6);
          this.tabPage_file.Controls.Add(this.cmb_overwrite);
          this.tabPage_file.Controls.Add(this.button1);
          this.tabPage_file.Controls.Add(this.label2);
          this.tabPage_file.Controls.Add(this.txt_installpath);
          this.tabPage_file.Controls.Add(this.label1);
          this.tabPage_file.Controls.Add(this.cmb_installtype);
          this.tabPage_file.Location = new System.Drawing.Point(4, 22);
          this.tabPage_file.Name = "tabPage_file";
          this.tabPage_file.Padding = new System.Windows.Forms.Padding(3);
          this.tabPage_file.Size = new System.Drawing.Size(352, 377);
          this.tabPage_file.TabIndex = 1;
          this.tabPage_file.Text = "File";
          this.tabPage_file.UseVisualStyleBackColor = true;
          // 
          // label7
          // 
          this.label7.AutoSize = true;
          this.label7.Location = new System.Drawing.Point(3, 117);
          this.label7.Name = "label7";
          this.label7.Size = new System.Drawing.Size(73, 13);
          this.label7.TabIndex = 10;
          this.label7.Text = "Ignored skins:";
          // 
          // txt_param1
          // 
          this.txt_param1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.txt_param1.Location = new System.Drawing.Point(95, 114);
          this.txt_param1.Name = "txt_param1";
          this.txt_param1.Size = new System.Drawing.Size(251, 20);
          this.txt_param1.TabIndex = 8;
          this.toolTip.SetToolTip(this.txt_param1, "Add all skins here, which should be ignored when installing Generic Skin files,\r\n" +
                  "separated by comma (\',\').");
          this.txt_param1.TextChanged += new System.EventHandler(this.txt_description_TextChanged);
          // 
          // btn_set
          // 
          this.btn_set.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.btn_set.Location = new System.Drawing.Point(271, 140);
          this.btn_set.Name = "btn_set";
          this.btn_set.Size = new System.Drawing.Size(75, 23);
          this.btn_set.TabIndex = 7;
          this.btn_set.Text = "Set";
          this.toolTip.SetToolTip(this.btn_set, "Set these options to all file items of this group.");
          this.btn_set.UseVisualStyleBackColor = true;
          this.btn_set.Click += new System.EventHandler(this.btn_set_Click);
          // 
          // label6
          // 
          this.label6.AutoSize = true;
          this.label6.Location = new System.Drawing.Point(3, 62);
          this.label6.Name = "label6";
          this.label6.Size = new System.Drawing.Size(76, 13);
          this.label6.TabIndex = 6;
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
          this.cmb_overwrite.Location = new System.Drawing.Point(95, 59);
          this.cmb_overwrite.Name = "cmb_overwrite";
          this.cmb_overwrite.Size = new System.Drawing.Size(251, 21);
          this.cmb_overwrite.TabIndex = 5;
          this.cmb_overwrite.SelectedIndexChanged += new System.EventHandler(this.txt_description_TextChanged);
          // 
          // button1
          // 
          this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.button1.Location = new System.Drawing.Point(327, 86);
          this.button1.Name = "button1";
          this.button1.Size = new System.Drawing.Size(19, 23);
          this.button1.TabIndex = 4;
          this.button1.Text = ".";
          this.button1.UseVisualStyleBackColor = true;
          this.button1.Click += new System.EventHandler(this.button1_Click);
          // 
          // label2
          // 
          this.label2.AutoSize = true;
          this.label2.Location = new System.Drawing.Point(3, 91);
          this.label2.Name = "label2";
          this.label2.Size = new System.Drawing.Size(74, 13);
          this.label2.TabIndex = 3;
          this.label2.Text = "Install location";
          // 
          // txt_installpath
          // 
          this.txt_installpath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.txt_installpath.Location = new System.Drawing.Point(95, 88);
          this.txt_installpath.Name = "txt_installpath";
          this.txt_installpath.Size = new System.Drawing.Size(228, 20);
          this.txt_installpath.TabIndex = 2;
          this.txt_installpath.TextChanged += new System.EventHandler(this.txt_description_TextChanged);
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.Location = new System.Drawing.Point(3, 35);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(57, 13);
          this.label1.TabIndex = 1;
          this.label1.Text = "Install type";
          // 
          // cmb_installtype
          // 
          this.cmb_installtype.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.cmb_installtype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.cmb_installtype.FormattingEnabled = true;
          this.cmb_installtype.Location = new System.Drawing.Point(95, 32);
          this.cmb_installtype.Name = "cmb_installtype";
          this.cmb_installtype.Size = new System.Drawing.Size(251, 21);
          this.cmb_installtype.TabIndex = 0;
          this.toolTip.SetToolTip(this.cmb_installtype, "1");
          this.cmb_installtype.SelectedIndexChanged += new System.EventHandler(this.txt_description_TextChanged);
          // 
          // treeView1
          // 
          this.treeView1.AllowDrop = true;
          this.treeView1.Dock = System.Windows.Forms.DockStyle.Left;
          this.treeView1.HideSelection = false;
          this.treeView1.ImageIndex = 0;
          this.treeView1.ImageList = this.imageList;
          this.treeView1.Location = new System.Drawing.Point(0, 25);
          this.treeView1.Name = "treeView1";
          this.treeView1.SelectedImageIndex = 0;
          this.treeView1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
          this.treeView1.SelectionMode = CodersLab.Windows.Controls.TreeViewSelectionMode.MultiSelectSameParent;
          this.treeView1.ShowNodeToolTips = true;
          this.treeView1.Size = new System.Drawing.Size(342, 403);
          this.treeView1.TabIndex = 1;
          this.treeView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView1_DragDrop);
          this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
          this.treeView1.Leave += new System.EventHandler(this.treeView1_Leave);
          this.treeView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView1_DragEnter);
          this.treeView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
          this.treeView1.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView1_DragOver);
          // 
          // FilesGroupsSection
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.BackColor = System.Drawing.SystemColors.Control;
          this.Controls.Add(this.tabControl1);
          this.Controls.Add(this.treeView1);
          this.Controls.Add(this.toolStrip);
          this.Name = "FilesGroupsSection";
          this.Size = new System.Drawing.Size(702, 428);
          this.toolStrip.ResumeLayout(false);
          this.toolStrip.PerformLayout();
          this.tabControl1.ResumeLayout(false);
          this.tabPage_group.ResumeLayout(false);
          this.tabPage_group.PerformLayout();
          this.tabPage_file.ResumeLayout(false);
          this.tabPage_file.PerformLayout();
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private CodersLab.Windows.Controls.TreeView treeView1;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton mnu_add_group;
        private System.Windows.Forms.ToolStripButton mnu_remove_group;
        private System.Windows.Forms.ToolStripSeparator toolStripButton3;
        private System.Windows.Forms.ToolStripButton mnu_add_files;
        private System.Windows.Forms.ToolStripButton mnu_remove_files;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_group;
        private System.Windows.Forms.TabPage tabPage_file;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_installpath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_installtype;
        private System.Windows.Forms.CheckBox chk_default;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_description;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmb_parentGroup;
        private System.Windows.Forms.TextBox txt_displlayName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmb_overwrite;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btn_set;
        private System.Windows.Forms.ToolStripButton mnu_add_folder;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txt_param1;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ListBox list_folder;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button btn_clearat;

    }
}
