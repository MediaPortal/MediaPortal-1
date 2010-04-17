namespace MpeMaker
{
    partial class MainForm
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
          System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Extension Informations");
          System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("General Informations", new System.Windows.Forms.TreeNode[] {
            treeNode1});
          System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Groups & Files");
          System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Install sections");
          System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Dependencies");
          System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Setup", new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode4,
            treeNode5});
          System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Build");
          System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Generate update xml");
          System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Tools", new System.Windows.Forms.TreeNode[] {
            treeNode8});
          this.mainMenu = new System.Windows.Forms.MenuStrip();
          this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
          this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
          this.mnu_recent = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
          this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
          this.treeView1 = new System.Windows.Forms.TreeView();
          this.splitContainer1 = new System.Windows.Forms.SplitContainer();
          this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
          this.toolStrip1 = new System.Windows.Forms.ToolStrip();
          this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
          this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
          this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
          this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
          this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
          this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
          this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
          this.openDirToolStripButton = new System.Windows.Forms.ToolStripButton();
          this.mnu_new = new System.Windows.Forms.ToolStripMenuItem();
          this.mnu_open = new System.Windows.Forms.ToolStripMenuItem();
          this.mnu_save = new System.Windows.Forms.ToolStripMenuItem();
          this.mnu_saveAs = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
          this.mainMenu.SuspendLayout();
          this.splitContainer1.Panel1.SuspendLayout();
          this.splitContainer1.SuspendLayout();
          this.toolStrip1.SuspendLayout();
          this.SuspendLayout();
          // 
          // mainMenu
          // 
          this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
          this.mainMenu.Location = new System.Drawing.Point(0, 0);
          this.mainMenu.Name = "mainMenu";
          this.mainMenu.Size = new System.Drawing.Size(893, 24);
          this.mainMenu.TabIndex = 1;
          this.mainMenu.Text = "menuStrip1";
          // 
          // fileToolStripMenuItem
          // 
          this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu_new,
            this.mnu_open,
            this.toolStripSeparator2,
            this.mnu_save,
            this.mnu_saveAs,
            this.toolStripSeparator3,
            this.mnu_recent,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
          this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
          this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
          this.fileToolStripMenuItem.Text = "File";
          // 
          // toolStripSeparator2
          // 
          this.toolStripSeparator2.Name = "toolStripSeparator2";
          this.toolStripSeparator2.Size = new System.Drawing.Size(175, 6);
          // 
          // toolStripSeparator3
          // 
          this.toolStripSeparator3.Name = "toolStripSeparator3";
          this.toolStripSeparator3.Size = new System.Drawing.Size(175, 6);
          // 
          // mnu_recent
          // 
          this.mnu_recent.Name = "mnu_recent";
          this.mnu_recent.Size = new System.Drawing.Size(178, 22);
          this.mnu_recent.Text = "Most Recently Used";
          // 
          // toolStripSeparator1
          // 
          this.toolStripSeparator1.Name = "toolStripSeparator1";
          this.toolStripSeparator1.Size = new System.Drawing.Size(175, 6);
          // 
          // exitToolStripMenuItem
          // 
          this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
          this.exitToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
          this.exitToolStripMenuItem.Text = "Exit";
          this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
          // 
          // openFileDialog
          // 
          this.openFileDialog.Title = "Open extension installer project file";
          // 
          // treeView1
          // 
          this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.treeView1.HideSelection = false;
          this.treeView1.HotTracking = true;
          this.treeView1.Location = new System.Drawing.Point(3, 3);
          this.treeView1.Name = "treeView1";
          treeNode1.Name = "Node2";
          treeNode1.Text = "Extension Informations";
          treeNode2.Name = "Node0";
          treeNode2.NodeFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          treeNode2.Text = "General Informations";
          treeNode3.Name = "Node3";
          treeNode3.Text = "Groups & Files";
          treeNode4.Name = "Node4";
          treeNode4.Text = "Install sections";
          treeNode5.Name = "Node5";
          treeNode5.Text = "Dependencies";
          treeNode6.Name = "Node1";
          treeNode6.NodeFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          treeNode6.Text = "Setup";
          treeNode7.Name = "Node6";
          treeNode7.NodeFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          treeNode7.Text = "Build";
          treeNode8.Name = "Node7";
          treeNode8.Text = "Generate update xml";
          treeNode9.Name = "Node1";
          treeNode9.NodeFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          treeNode9.Text = "Tools";
          this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode6,
            treeNode7,
            treeNode9});
          this.treeView1.ShowLines = false;
          this.treeView1.ShowPlusMinus = false;
          this.treeView1.Size = new System.Drawing.Size(175, 427);
          this.treeView1.TabIndex = 2;
          this.treeView1.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeCollapse);
          this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
          // 
          // splitContainer1
          // 
          this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
          this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
          this.splitContainer1.IsSplitterFixed = true;
          this.splitContainer1.Location = new System.Drawing.Point(0, 49);
          this.splitContainer1.Name = "splitContainer1";
          // 
          // splitContainer1.Panel1
          // 
          this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.ControlDark;
          this.splitContainer1.Panel1.Controls.Add(this.treeView1);
          // 
          // splitContainer1.Panel2
          // 
          this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Control;
          this.splitContainer1.Size = new System.Drawing.Size(893, 430);
          this.splitContainer1.SplitterDistance = 181;
          this.splitContainer1.TabIndex = 3;
          // 
          // saveFileDialog
          // 
          this.saveFileDialog.Title = "Save extension installer proiect file";
          // 
          // toolStrip1
          // 
          this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripSeparator6,
            this.toolStripButton3,
            this.toolStripButton4,
            this.toolStripSeparator4,
            this.openDirToolStripButton,
            this.toolStripSeparator5,
            this.toolStripButton5});
          this.toolStrip1.Location = new System.Drawing.Point(0, 24);
          this.toolStrip1.Name = "toolStrip1";
          this.toolStrip1.Size = new System.Drawing.Size(893, 25);
          this.toolStrip1.TabIndex = 4;
          this.toolStrip1.Text = "toolStrip1";
          // 
          // toolStripSeparator4
          // 
          this.toolStripSeparator4.Name = "toolStripSeparator4";
          this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
          // 
          // toolStripSeparator5
          // 
          this.toolStripSeparator5.Name = "toolStripSeparator5";
          this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
          // 
          // toolStripSeparator6
          // 
          this.toolStripSeparator6.Name = "toolStripSeparator6";
          this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
          // 
          // toolStripButton1
          // 
          this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.toolStripButton1.Image = global::MpeMaker.Properties.Resources.document_new;
          this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.toolStripButton1.Name = "toolStripButton1";
          this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
          this.toolStripButton1.Text = "New project...";
          this.toolStripButton1.Click += new System.EventHandler(this.mnu_new_Click);
          // 
          // toolStripButton2
          // 
          this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.toolStripButton2.Image = global::MpeMaker.Properties.Resources.document_open;
          this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.toolStripButton2.Name = "toolStripButton2";
          this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
          this.toolStripButton2.Text = "Open project...";
          this.toolStripButton2.Click += new System.EventHandler(this.mnu_open_Click);
          // 
          // toolStripButton3
          // 
          this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.toolStripButton3.Image = global::MpeMaker.Properties.Resources.document_save;
          this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.toolStripButton3.Name = "toolStripButton3";
          this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
          this.toolStripButton3.Text = "Save project";
          this.toolStripButton3.Click += new System.EventHandler(this.mnu_save_Click);
          // 
          // toolStripButton4
          // 
          this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.toolStripButton4.Image = global::MpeMaker.Properties.Resources.document_save_as;
          this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.toolStripButton4.Name = "toolStripButton4";
          this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
          this.toolStripButton4.Text = "Save project as...";
          this.toolStripButton4.Click += new System.EventHandler(this.mnu_saveAs_Click);
          // 
          // openDirToolStripButton
          // 
          this.openDirToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.openDirToolStripButton.Image = global::MpeMaker.Properties.Resources.folder_page;
          this.openDirToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.openDirToolStripButton.Name = "openDirToolStripButton";
          this.openDirToolStripButton.Size = new System.Drawing.Size(23, 22);
          this.openDirToolStripButton.Text = "Open containing folder...";
          this.openDirToolStripButton.Click += new System.EventHandler(this.toolStripButton5_Click);
          // 
          // mnu_new
          // 
          this.mnu_new.Image = global::MpeMaker.Properties.Resources.document_new;
          this.mnu_new.Name = "mnu_new";
          this.mnu_new.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
          this.mnu_new.Size = new System.Drawing.Size(178, 22);
          this.mnu_new.Text = "New...";
          this.mnu_new.Click += new System.EventHandler(this.mnu_new_Click);
          // 
          // mnu_open
          // 
          this.mnu_open.Image = global::MpeMaker.Properties.Resources.document_open;
          this.mnu_open.Name = "mnu_open";
          this.mnu_open.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
          this.mnu_open.Size = new System.Drawing.Size(178, 22);
          this.mnu_open.Text = "Open...";
          this.mnu_open.Click += new System.EventHandler(this.mnu_open_Click);
          // 
          // mnu_save
          // 
          this.mnu_save.Image = global::MpeMaker.Properties.Resources.document_save;
          this.mnu_save.Name = "mnu_save";
          this.mnu_save.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
          this.mnu_save.Size = new System.Drawing.Size(178, 22);
          this.mnu_save.Text = "Save";
          this.mnu_save.Click += new System.EventHandler(this.mnu_save_Click);
          // 
          // mnu_saveAs
          // 
          this.mnu_saveAs.Image = global::MpeMaker.Properties.Resources.document_save_as;
          this.mnu_saveAs.Name = "mnu_saveAs";
          this.mnu_saveAs.Size = new System.Drawing.Size(178, 22);
          this.mnu_saveAs.Text = "Save As...";
          this.mnu_saveAs.Click += new System.EventHandler(this.mnu_saveAs_Click);
          // 
          // toolStripButton5
          // 
          this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.toolStripButton5.Image = global::MpeMaker.Properties.Resources.Start;
          this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.toolStripButton5.Name = "toolStripButton5";
          this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
          this.toolStripButton5.Text = "Build package";
          this.toolStripButton5.Click += new System.EventHandler(this.toolStripButton5_Click_1);
          // 
          // MainForm
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(893, 479);
          this.Controls.Add(this.splitContainer1);
          this.Controls.Add(this.toolStrip1);
          this.Controls.Add(this.mainMenu);
          this.Name = "MainForm";
          this.Text = "MpeMaker";
          this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
          this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
          this.mainMenu.ResumeLayout(false);
          this.mainMenu.PerformLayout();
          this.splitContainer1.Panel1.ResumeLayout(false);
          this.splitContainer1.ResumeLayout(false);
          this.toolStrip1.ResumeLayout(false);
          this.toolStrip1.PerformLayout();
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem mnu_new;
        private System.Windows.Forms.ToolStripMenuItem mnu_open;
        private System.Windows.Forms.ToolStripMenuItem mnu_save;
        private System.Windows.Forms.ToolStripMenuItem mnu_saveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mnu_recent;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton openDirToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
    }
}

