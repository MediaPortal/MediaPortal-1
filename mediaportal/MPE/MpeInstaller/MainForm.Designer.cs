namespace MpeInstaller
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
      System.Windows.Forms.ToolStripStatusLabel divider;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tab_extensions = new System.Windows.Forms.TabPage();
      this.extensionListControlInstalled = new MpeInstaller.Controls.ExtensionListControl();
      this.tab_known = new System.Windows.Forms.TabPage();
      this.extensionListControlKnown = new MpeInstaller.Controls.ExtensionListControl();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripLabelWarn = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripLastUpdate = new System.Windows.Forms.ToolStripStatusLabel();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.refreshUpdateInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.updateAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.cleanCacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.onlyStableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.onlyCompatibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.wikiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      divider = new System.Windows.Forms.ToolStripStatusLabel();
      this.tabControl1.SuspendLayout();
      this.tab_extensions.SuspendLayout();
      this.tab_known.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // divider
      // 
      divider.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
      divider.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      divider.Name = "divider";
      divider.Size = new System.Drawing.Size(344, 17);
      divider.Spring = true;
      // 
      // tabControl1
      // 
      this.tabControl1.AllowDrop = true;
      this.tabControl1.Controls.Add(this.tab_extensions);
      this.tabControl1.Controls.Add(this.tab_known);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 24);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(592, 434);
      this.tabControl1.TabIndex = 0;
      // 
      // tab_extensions
      // 
      this.tab_extensions.Controls.Add(this.extensionListControlInstalled);
      this.tab_extensions.Location = new System.Drawing.Point(4, 22);
      this.tab_extensions.Name = "tab_extensions";
      this.tab_extensions.Padding = new System.Windows.Forms.Padding(3);
      this.tab_extensions.Size = new System.Drawing.Size(584, 408);
      this.tab_extensions.TabIndex = 0;
      this.tab_extensions.Text = "Installed extensions";
      this.tab_extensions.UseVisualStyleBackColor = true;
      // 
      // extensionListControlInstalled
      // 
      this.extensionListControlInstalled.AllowDrop = true;
      this.extensionListControlInstalled.AutoSize = true;
      this.extensionListControlInstalled.Dock = System.Windows.Forms.DockStyle.Fill;
      this.extensionListControlInstalled.Location = new System.Drawing.Point(3, 3);
      this.extensionListControlInstalled.Name = "extensionListControlInstalled";
      this.extensionListControlInstalled.SelectedItem = null;
      this.extensionListControlInstalled.Size = new System.Drawing.Size(578, 402);
      this.extensionListControlInstalled.TabIndex = 0;
      this.extensionListControlInstalled.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
      this.extensionListControlInstalled.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
      // 
      // tab_known
      // 
      this.tab_known.Controls.Add(this.extensionListControlKnown);
      this.tab_known.Location = new System.Drawing.Point(4, 22);
      this.tab_known.Name = "tab_known";
      this.tab_known.Padding = new System.Windows.Forms.Padding(3);
      this.tab_known.Size = new System.Drawing.Size(584, 408);
      this.tab_known.TabIndex = 2;
      this.tab_known.Text = "Known extensions";
      this.tab_known.UseVisualStyleBackColor = true;
      // 
      // extensionListControlKnown
      // 
      this.extensionListControlKnown.AllowDrop = true;
      this.extensionListControlKnown.AutoSize = true;
      this.extensionListControlKnown.Dock = System.Windows.Forms.DockStyle.Fill;
      this.extensionListControlKnown.Location = new System.Drawing.Point(3, 3);
      this.extensionListControlKnown.Name = "extensionListControlKnown";
      this.extensionListControlKnown.SelectedItem = null;
      this.extensionListControlKnown.Size = new System.Drawing.Size(578, 402);
      this.extensionListControlKnown.TabIndex = 0;
      this.extensionListControlKnown.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
      this.extensionListControlKnown.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
      // 
      // statusStrip1
      // 
      this.statusStrip1.Font = new System.Drawing.Font("Segoe UI", 8F);
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelWarn,
            divider,
            this.toolStripLastUpdate});
      this.statusStrip1.Location = new System.Drawing.Point(0, 458);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(592, 22);
      this.statusStrip1.TabIndex = 6;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripLabelWarn
      // 
      this.toolStripLabelWarn.BackColor = System.Drawing.Color.Transparent;
      this.toolStripLabelWarn.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
      this.toolStripLabelWarn.Name = "toolStripLabelWarn";
      this.toolStripLabelWarn.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.toolStripLabelWarn.Size = new System.Drawing.Size(158, 17);
      this.toolStripLabelWarn.Text = "Some extensions are hidden";
      // 
      // toolStripLastUpdate
      // 
      this.toolStripLastUpdate.Name = "toolStripLastUpdate";
      this.toolStripLastUpdate.Size = new System.Drawing.Size(75, 17);
      this.toolStripLastUpdate.Text = "Last Updated";
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(592, 24);
      this.menuStrip1.TabIndex = 7;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.refreshUpdateInfoToolStripMenuItem,
            this.updateAllToolStripMenuItem,
            this.cleanCacheToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
      this.fileToolStripMenuItem.Text = "File";
      // 
      // openToolStripMenuItem
      // 
      this.openToolStripMenuItem.Image = global::MpeInstaller.Properties.Resources.open;
      this.openToolStripMenuItem.Name = "openToolStripMenuItem";
      this.openToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.openToolStripMenuItem.Text = "Open";
      this.openToolStripMenuItem.Click += new System.EventHandler(this.FileOpen_Click);
      // 
      // refreshUpdateInfoToolStripMenuItem
      // 
      this.refreshUpdateInfoToolStripMenuItem.Image = global::MpeInstaller.Properties.Resources.refresh;
      this.refreshUpdateInfoToolStripMenuItem.Name = "refreshUpdateInfoToolStripMenuItem";
      this.refreshUpdateInfoToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.refreshUpdateInfoToolStripMenuItem.Text = "Refresh Update Info";
      this.refreshUpdateInfoToolStripMenuItem.Click += new System.EventHandler(this.RefreshUpdateInfo_Click);
      // 
      // updateAllToolStripMenuItem
      // 
      this.updateAllToolStripMenuItem.Image = global::MpeInstaller.Properties.Resources.system_software_update;
      this.updateAllToolStripMenuItem.Name = "updateAllToolStripMenuItem";
      this.updateAllToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.updateAllToolStripMenuItem.Text = "Update All Installed";
      this.updateAllToolStripMenuItem.Click += new System.EventHandler(this.UpdateAll_Click);
      // 
      // cleanCacheToolStripMenuItem
      // 
      this.cleanCacheToolStripMenuItem.Image = global::MpeInstaller.Properties.Resources.recycle_bin;
      this.cleanCacheToolStripMenuItem.Name = "cleanCacheToolStripMenuItem";
      this.cleanCacheToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.cleanCacheToolStripMenuItem.Text = "Clean Cache";
      this.cleanCacheToolStripMenuItem.Click += new System.EventHandler(this.CleanCache_Click);
      // 
      // viewToolStripMenuItem
      // 
      this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.onlyStableToolStripMenuItem,
            this.onlyCompatibleToolStripMenuItem});
      this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
      this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.viewToolStripMenuItem.Text = "View";
      // 
      // onlyStableToolStripMenuItem
      // 
      this.onlyStableToolStripMenuItem.CheckOnClick = true;
      this.onlyStableToolStripMenuItem.Name = "onlyStableToolStripMenuItem";
      this.onlyStableToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
      this.onlyStableToolStripMenuItem.Text = "only stable";
      this.onlyStableToolStripMenuItem.CheckedChanged += new System.EventHandler(this.chk_stable_CheckedChanged);
      // 
      // onlyCompatibleToolStripMenuItem
      // 
      this.onlyCompatibleToolStripMenuItem.CheckOnClick = true;
      this.onlyCompatibleToolStripMenuItem.Name = "onlyCompatibleToolStripMenuItem";
      this.onlyCompatibleToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
      this.onlyCompatibleToolStripMenuItem.Text = "only compatible";
      this.onlyCompatibleToolStripMenuItem.CheckedChanged += new System.EventHandler(this.chk_dependency_CheckedChanged);
      // 
      // settingsToolStripMenuItem
      // 
      this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem1});
      this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
      this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
      this.settingsToolStripMenuItem.Text = "Options";
      // 
      // settingsToolStripMenuItem1
      // 
      this.settingsToolStripMenuItem1.Image = global::MpeInstaller.Properties.Resources.settings;
      this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
      this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(116, 22);
      this.settingsToolStripMenuItem1.Text = "Settings";
      this.settingsToolStripMenuItem1.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
      // 
      // helpToolStripMenuItem
      // 
      this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wikiToolStripMenuItem});
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.helpToolStripMenuItem.Text = "Help";
      // 
      // wikiToolStripMenuItem
      // 
      this.wikiToolStripMenuItem.Image = global::MpeInstaller.Properties.Resources.help;
      this.wikiToolStripMenuItem.Name = "wikiToolStripMenuItem";
      this.wikiToolStripMenuItem.Size = new System.Drawing.Size(97, 22);
      this.wikiToolStripMenuItem.Text = "Wiki";
      this.wikiToolStripMenuItem.Click += new System.EventHandler(this.wikiToolStripMenuItem_Click);
      // 
      // MainForm
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(592, 480);
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.menuStrip1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.menuStrip1;
      this.MinimumSize = new System.Drawing.Size(600, 400);
      this.Name = "MainForm";
      this.Text = "MediaPortal Extensions Manager";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.Shown += new System.EventHandler(this.MainForm_Shown);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
      this.tabControl1.ResumeLayout(false);
      this.tab_extensions.ResumeLayout(false);
      this.tab_extensions.PerformLayout();
      this.tab_known.ResumeLayout(false);
      this.tab_known.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tab_extensions;
        private MpeInstaller.Controls.ExtensionListControl extensionListControlInstalled;
        private System.Windows.Forms.TabPage tab_known;
        private MpeInstaller.Controls.ExtensionListControl extensionListControlKnown;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripLastUpdate;
        private System.Windows.Forms.ToolStripStatusLabel toolStripLabelWarn;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem onlyStableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem onlyCompatibleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cleanCacheToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wikiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshUpdateInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
    }
}

