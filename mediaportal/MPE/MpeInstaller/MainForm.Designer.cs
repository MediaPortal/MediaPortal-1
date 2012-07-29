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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tab_extensions = new System.Windows.Forms.TabPage();
      this.extensionListControlInstalled = new MpeInstaller.Controls.ExtensionListControl();
      this.tab_known = new System.Windows.Forms.TabPage();
      this.extensionListControlKnown = new MpeInstaller.Controls.ExtensionListControl();
      this.tab_options = new System.Windows.Forms.TabPage();
      this.lbl_warn = new System.Windows.Forms.Label();
      this.btn_clean = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.chk_dependency = new System.Windows.Forms.CheckBox();
      this.lbl_lastupdate = new System.Windows.Forms.Label();
      this.chk_stable = new System.Windows.Forms.CheckBox();
      this.chk_updateExtension = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.numeric_Days = new System.Windows.Forms.NumericUpDown();
      this.chk_update = new System.Windows.Forms.CheckBox();
      this.btn_online_update = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.tabControl1.SuspendLayout();
      this.tab_extensions.SuspendLayout();
      this.tab_known.SuspendLayout();
      this.tab_options.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numeric_Days)).BeginInit();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.AllowDrop = true;
      this.tabControl1.Controls.Add(this.tab_extensions);
      this.tabControl1.Controls.Add(this.tab_known);
      this.tabControl1.Controls.Add(this.tab_options);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(592, 480);
      this.tabControl1.TabIndex = 0;
      // 
      // tab_extensions
      // 
      this.tab_extensions.Controls.Add(this.extensionListControlInstalled);
      this.tab_extensions.Location = new System.Drawing.Point(4, 22);
      this.tab_extensions.Name = "tab_extensions";
      this.tab_extensions.Padding = new System.Windows.Forms.Padding(3);
      this.tab_extensions.Size = new System.Drawing.Size(584, 454);
      this.tab_extensions.TabIndex = 0;
      this.tab_extensions.Text = "Installed extensions";
      this.tab_extensions.UseVisualStyleBackColor = true;
      // 
      // extensionListControl
      // 
      this.extensionListControlInstalled.AllowDrop = true;
      this.extensionListControlInstalled.AutoSize = true;
      this.extensionListControlInstalled.Dock = System.Windows.Forms.DockStyle.Fill;
      this.extensionListControlInstalled.Location = new System.Drawing.Point(3, 3);
      this.extensionListControlInstalled.Name = "extensionListControl";
      this.extensionListControlInstalled.SelectedItem = null;
      this.extensionListControlInstalled.Size = new System.Drawing.Size(578, 448);
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
      this.tab_known.Size = new System.Drawing.Size(584, 454);
      this.tab_known.TabIndex = 2;
      this.tab_known.Text = "Known extensions";
      this.tab_known.UseVisualStyleBackColor = true;
      // 
      // extensionListContro_all
      // 
      this.extensionListControlKnown.AllowDrop = true;
      this.extensionListControlKnown.AutoSize = true;
      this.extensionListControlKnown.Dock = System.Windows.Forms.DockStyle.Fill;
      this.extensionListControlKnown.Location = new System.Drawing.Point(3, 3);
      this.extensionListControlKnown.Name = "extensionListContro_all";
      this.extensionListControlKnown.SelectedItem = null;
      this.extensionListControlKnown.Size = new System.Drawing.Size(578, 448);
      this.extensionListControlKnown.TabIndex = 0;
      this.extensionListControlKnown.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
      this.extensionListControlKnown.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
      // 
      // tab_options
      // 
      this.tab_options.AllowDrop = true;
      this.tab_options.Controls.Add(this.lbl_warn);
      this.tab_options.Controls.Add(this.btn_clean);
      this.tab_options.Controls.Add(this.button2);
      this.tab_options.Controls.Add(this.groupBox1);
      this.tab_options.Controls.Add(this.btn_online_update);
      this.tab_options.Controls.Add(this.button1);
      this.tab_options.Location = new System.Drawing.Point(4, 22);
      this.tab_options.Name = "tab_options";
      this.tab_options.Padding = new System.Windows.Forms.Padding(3);
      this.tab_options.Size = new System.Drawing.Size(584, 454);
      this.tab_options.TabIndex = 1;
      this.tab_options.Text = "Options";
      this.tab_options.UseVisualStyleBackColor = true;
      this.tab_options.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
      this.tab_options.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
      // 
      // lbl_warn
      // 
      this.lbl_warn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_warn.BackColor = System.Drawing.Color.Red;
      this.lbl_warn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_warn.Location = new System.Drawing.Point(11, 188);
      this.lbl_warn.Name = "lbl_warn";
      this.lbl_warn.Size = new System.Drawing.Size(559, 21);
      this.lbl_warn.TabIndex = 5;
      this.lbl_warn.Text = "Some extension are hidden";
      this.lbl_warn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // btn_clean
      // 
      this.btn_clean.AllowDrop = true;
      this.btn_clean.Location = new System.Drawing.Point(327, 35);
      this.btn_clean.Name = "btn_clean";
      this.btn_clean.Size = new System.Drawing.Size(243, 23);
      this.btn_clean.TabIndex = 4;
      this.btn_clean.Text = "Clean installation cache";
      this.btn_clean.UseVisualStyleBackColor = true;
      this.btn_clean.Click += new System.EventHandler(this.btn_clean_Click);
      // 
      // button2
      // 
      this.button2.AllowDrop = true;
      this.button2.Location = new System.Drawing.Point(327, 6);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(243, 23);
      this.button2.TabIndex = 3;
      this.button2.Text = "Update all extensions";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.chk_dependency);
      this.groupBox1.Controls.Add(this.lbl_lastupdate);
      this.groupBox1.Controls.Add(this.chk_stable);
      this.groupBox1.Controls.Add(this.chk_updateExtension);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.numeric_Days);
      this.groupBox1.Controls.Add(this.chk_update);
      this.groupBox1.Location = new System.Drawing.Point(8, 76);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(562, 109);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Startup";
      this.groupBox1.UseCompatibleTextRendering = true;
      // 
      // chk_dependency
      // 
      this.chk_dependency.AutoSize = true;
      this.chk_dependency.Location = new System.Drawing.Point(6, 86);
      this.chk_dependency.Name = "chk_dependency";
      this.chk_dependency.Size = new System.Drawing.Size(185, 17);
      this.chk_dependency.TabIndex = 7;
      this.chk_dependency.Text = "Show only  compatible extensions";
      this.chk_dependency.UseVisualStyleBackColor = true;
      this.chk_dependency.CheckedChanged += new System.EventHandler(this.chk_dependency_CheckedChanged);
      // 
      // lbl_lastupdate
      // 
      this.lbl_lastupdate.AutoSize = true;
      this.lbl_lastupdate.Location = new System.Drawing.Point(284, 18);
      this.lbl_lastupdate.Name = "lbl_lastupdate";
      this.lbl_lastupdate.Size = new System.Drawing.Size(35, 13);
      this.lbl_lastupdate.TabIndex = 6;
      this.lbl_lastupdate.Text = "label3";
      // 
      // chk_stable
      // 
      this.chk_stable.AutoSize = true;
      this.chk_stable.Location = new System.Drawing.Point(6, 65);
      this.chk_stable.Name = "chk_stable";
      this.chk_stable.Size = new System.Drawing.Size(148, 17);
      this.chk_stable.TabIndex = 5;
      this.chk_stable.Text = "Show only stable releases";
      this.chk_stable.UseVisualStyleBackColor = true;
      this.chk_stable.CheckedChanged += new System.EventHandler(this.chk_stable_CheckedChanged);
      // 
      // chk_updateExtension
      // 
      this.chk_updateExtension.AutoSize = true;
      this.chk_updateExtension.Location = new System.Drawing.Point(6, 42);
      this.chk_updateExtension.Name = "chk_updateExtension";
      this.chk_updateExtension.Size = new System.Drawing.Size(155, 17);
      this.chk_updateExtension.TabIndex = 4;
      this.chk_updateExtension.Text = "Update installed extensions";
      this.chk_updateExtension.UseVisualStyleBackColor = true;
      this.chk_updateExtension.CheckedChanged += new System.EventHandler(this.chk_update_CheckedChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(249, 20);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(29, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "days";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(163, 20);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(34, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Every";
      // 
      // numeric_Days
      // 
      this.numeric_Days.Location = new System.Drawing.Point(204, 16);
      this.numeric_Days.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.numeric_Days.Name = "numeric_Days";
      this.numeric_Days.Size = new System.Drawing.Size(39, 20);
      this.numeric_Days.TabIndex = 1;
      this.numeric_Days.ValueChanged += new System.EventHandler(this.chk_update_CheckedChanged);
      // 
      // chk_update
      // 
      this.chk_update.AutoSize = true;
      this.chk_update.Location = new System.Drawing.Point(6, 19);
      this.chk_update.Name = "chk_update";
      this.chk_update.Size = new System.Drawing.Size(149, 17);
      this.chk_update.TabIndex = 0;
      this.chk_update.Text = "Get update info on startup";
      this.chk_update.UseVisualStyleBackColor = true;
      this.chk_update.CheckedChanged += new System.EventHandler(this.chk_update_CheckedChanged);
      // 
      // btn_online_update
      // 
      this.btn_online_update.AllowDrop = true;
      this.btn_online_update.Location = new System.Drawing.Point(8, 35);
      this.btn_online_update.Name = "btn_online_update";
      this.btn_online_update.Size = new System.Drawing.Size(243, 23);
      this.btn_online_update.TabIndex = 1;
      this.btn_online_update.Text = "Download online update info";
      this.btn_online_update.UseVisualStyleBackColor = true;
      this.btn_online_update.Click += new System.EventHandler(this.btn_online_update_Click);
      this.btn_online_update.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
      this.btn_online_update.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
      // 
      // button1
      // 
      this.button1.AllowDrop = true;
      this.button1.Location = new System.Drawing.Point(8, 6);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(243, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "Install local extension";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      this.button1.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
      this.button1.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
      // 
      // MainForm
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(592, 480);
      this.Controls.Add(this.tabControl1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
      this.tab_options.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numeric_Days)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tab_extensions;
        private System.Windows.Forms.TabPage tab_options;
        private System.Windows.Forms.Button button1;
        private MpeInstaller.Controls.ExtensionListControl extensionListControlInstalled;
        private System.Windows.Forms.Button btn_online_update;
        private System.Windows.Forms.TabPage tab_known;
        private MpeInstaller.Controls.ExtensionListControl extensionListControlKnown;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chk_update;
        private System.Windows.Forms.CheckBox chk_updateExtension;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numeric_Days;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox chk_stable;
        private System.Windows.Forms.Button btn_clean;
        private System.Windows.Forms.Label lbl_lastupdate;
        private System.Windows.Forms.CheckBox chk_dependency;
        private System.Windows.Forms.Label lbl_warn;
    }
}

