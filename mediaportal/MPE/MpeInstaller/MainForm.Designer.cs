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
          this.extensionListControl = new MpeInstaller.Controls.ExtensionListControl();
          this.tab_known = new System.Windows.Forms.TabPage();
          this.extensionListContro_all = new MpeInstaller.Controls.ExtensionListControl();
          this.tab_options = new System.Windows.Forms.TabPage();
          this.button2 = new System.Windows.Forms.Button();
          this.groupBox1 = new System.Windows.Forms.GroupBox();
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
          this.tabControl1.Size = new System.Drawing.Size(589, 480);
          this.tabControl1.TabIndex = 0;
          // 
          // tab_extensions
          // 
          this.tab_extensions.Controls.Add(this.extensionListControl);
          this.tab_extensions.Location = new System.Drawing.Point(4, 22);
          this.tab_extensions.Name = "tab_extensions";
          this.tab_extensions.Padding = new System.Windows.Forms.Padding(3);
          this.tab_extensions.Size = new System.Drawing.Size(581, 454);
          this.tab_extensions.TabIndex = 0;
          this.tab_extensions.Text = "Installed extensions";
          this.tab_extensions.UseVisualStyleBackColor = true;
          // 
          // extensionListControl
          // 
          this.extensionListControl.AllowDrop = true;
          this.extensionListControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.extensionListControl.AutoSize = true;
          this.extensionListControl.Location = new System.Drawing.Point(3, 3);
          this.extensionListControl.Name = "extensionListControl";
          this.extensionListControl.SelectedItem = null;
          this.extensionListControl.Size = new System.Drawing.Size(575, 448);
          this.extensionListControl.TabIndex = 0;
          this.extensionListControl.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
          this.extensionListControl.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
          // 
          // tab_known
          // 
          this.tab_known.Controls.Add(this.extensionListContro_all);
          this.tab_known.Location = new System.Drawing.Point(4, 22);
          this.tab_known.Name = "tab_known";
          this.tab_known.Padding = new System.Windows.Forms.Padding(3);
          this.tab_known.Size = new System.Drawing.Size(578, 454);
          this.tab_known.TabIndex = 2;
          this.tab_known.Text = "Known extensions";
          this.tab_known.UseVisualStyleBackColor = true;
          // 
          // extensionListContro_all
          // 
          this.extensionListContro_all.AllowDrop = true;
          this.extensionListContro_all.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.extensionListContro_all.AutoSize = true;
          this.extensionListContro_all.Location = new System.Drawing.Point(3, 3);
          this.extensionListContro_all.Name = "extensionListContro_all";
          this.extensionListContro_all.SelectedItem = null;
          this.extensionListContro_all.Size = new System.Drawing.Size(572, 448);
          this.extensionListContro_all.TabIndex = 0;
          this.extensionListContro_all.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
          this.extensionListContro_all.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
          // 
          // tab_options
          // 
          this.tab_options.AllowDrop = true;
          this.tab_options.Controls.Add(this.button2);
          this.tab_options.Controls.Add(this.groupBox1);
          this.tab_options.Controls.Add(this.btn_online_update);
          this.tab_options.Controls.Add(this.button1);
          this.tab_options.Location = new System.Drawing.Point(4, 22);
          this.tab_options.Name = "tab_options";
          this.tab_options.Padding = new System.Windows.Forms.Padding(3);
          this.tab_options.Size = new System.Drawing.Size(578, 454);
          this.tab_options.TabIndex = 1;
          this.tab_options.Text = "Options";
          this.tab_options.UseVisualStyleBackColor = true;
          this.tab_options.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
          this.tab_options.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
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
          this.groupBox1.Controls.Add(this.chk_updateExtension);
          this.groupBox1.Controls.Add(this.label2);
          this.groupBox1.Controls.Add(this.label1);
          this.groupBox1.Controls.Add(this.numeric_Days);
          this.groupBox1.Controls.Add(this.chk_update);
          this.groupBox1.Location = new System.Drawing.Point(8, 76);
          this.groupBox1.Name = "groupBox1";
          this.groupBox1.Size = new System.Drawing.Size(562, 67);
          this.groupBox1.TabIndex = 2;
          this.groupBox1.TabStop = false;
          this.groupBox1.Text = "Startup";
          this.groupBox1.UseCompatibleTextRendering = true;
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
          this.AutoSize = true;
          this.ClientSize = new System.Drawing.Size(589, 480);
          this.Controls.Add(this.tabControl1);
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
          this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
          this.MaximizeBox = false;
          this.Name = "MainForm";
          this.Text = "Media Portal Extension Installer";
          this.Load += new System.EventHandler(this.MainForm_Load);
          this.Shown += new System.EventHandler(this.MainForm_Shown);
          this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
          this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
          this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
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
        private MpeInstaller.Controls.ExtensionListControl extensionListControl;
        private System.Windows.Forms.Button btn_online_update;
        private System.Windows.Forms.TabPage tab_known;
        private MpeInstaller.Controls.ExtensionListControl extensionListContro_all;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chk_update;
        private System.Windows.Forms.CheckBox chk_updateExtension;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numeric_Days;
        private System.Windows.Forms.Button button2;
    }
}

