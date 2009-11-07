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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tab_extensions = new System.Windows.Forms.TabPage();
            this.tab_options = new System.Windows.Forms.TabPage();
            this.btn_online_update = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.tab_known = new System.Windows.Forms.TabPage();
            this.extensionListControl = new MpeInstaller.Controls.ExtensionListControl();
            this.extensionListContro_all = new MpeInstaller.Controls.ExtensionListControl();
            this.tabControl1.SuspendLayout();
            this.tab_extensions.SuspendLayout();
            this.tab_options.SuspendLayout();
            this.tab_known.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tab_extensions);
            this.tabControl1.Controls.Add(this.tab_known);
            this.tabControl1.Controls.Add(this.tab_options);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(572, 480);
            this.tabControl1.TabIndex = 0;
            // 
            // tab_extensions
            // 
            this.tab_extensions.Controls.Add(this.extensionListControl);
            this.tab_extensions.Location = new System.Drawing.Point(4, 22);
            this.tab_extensions.Name = "tab_extensions";
            this.tab_extensions.Padding = new System.Windows.Forms.Padding(3);
            this.tab_extensions.Size = new System.Drawing.Size(564, 454);
            this.tab_extensions.TabIndex = 0;
            this.tab_extensions.Text = "Installed extensions";
            this.tab_extensions.UseVisualStyleBackColor = true;
            // 
            // tab_options
            // 
            this.tab_options.Controls.Add(this.btn_online_update);
            this.tab_options.Controls.Add(this.button1);
            this.tab_options.Location = new System.Drawing.Point(4, 22);
            this.tab_options.Name = "tab_options";
            this.tab_options.Padding = new System.Windows.Forms.Padding(3);
            this.tab_options.Size = new System.Drawing.Size(564, 454);
            this.tab_options.TabIndex = 1;
            this.tab_options.Text = "Options";
            this.tab_options.UseVisualStyleBackColor = true;
            // 
            // btn_online_update
            // 
            this.btn_online_update.Location = new System.Drawing.Point(8, 35);
            this.btn_online_update.Name = "btn_online_update";
            this.btn_online_update.Size = new System.Drawing.Size(243, 23);
            this.btn_online_update.TabIndex = 1;
            this.btn_online_update.Text = "Download online update info";
            this.btn_online_update.UseVisualStyleBackColor = true;
            this.btn_online_update.Click += new System.EventHandler(this.btn_online_update_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(8, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(243, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Install local extension";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tab_known
            // 
            this.tab_known.Controls.Add(this.extensionListContro_all);
            this.tab_known.Location = new System.Drawing.Point(4, 22);
            this.tab_known.Name = "tab_known";
            this.tab_known.Padding = new System.Windows.Forms.Padding(3);
            this.tab_known.Size = new System.Drawing.Size(564, 454);
            this.tab_known.TabIndex = 2;
            this.tab_known.Text = "Known extensions";
            this.tab_known.UseVisualStyleBackColor = true;
            // 
            // extensionListControl
            // 
            this.extensionListControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionListControl.Location = new System.Drawing.Point(3, 3);
            this.extensionListControl.Name = "extensionListControl";
            this.extensionListControl.SelectedItem = null;
            this.extensionListControl.Size = new System.Drawing.Size(558, 448);
            this.extensionListControl.TabIndex = 0;
            // 
            // extensionListContro_all
            // 
            this.extensionListContro_all.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionListContro_all.Location = new System.Drawing.Point(3, 3);
            this.extensionListContro_all.Name = "extensionListContro_all";
            this.extensionListContro_all.SelectedItem = null;
            this.extensionListContro_all.Size = new System.Drawing.Size(558, 448);
            this.extensionListContro_all.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 480);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Media Portal Extension Installer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tab_extensions.ResumeLayout(false);
            this.tab_options.ResumeLayout(false);
            this.tab_known.ResumeLayout(false);
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
    }
}

