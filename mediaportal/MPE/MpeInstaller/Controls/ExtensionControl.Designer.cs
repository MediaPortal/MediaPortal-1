namespace MpeInstaller.Controls
{
    partial class ExtensionControl
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
            this.lbl_name = new System.Windows.Forms.Label();
            this.btn_uninstall = new System.Windows.Forms.Button();
            this.lbl_description = new System.Windows.Forms.Label();
            this.lbl_version = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btn_update = new System.Windows.Forms.Button();
            this.img_dep = new System.Windows.Forms.PictureBox();
            this.img_update = new System.Windows.Forms.PictureBox();
            this.img_logo = new System.Windows.Forms.PictureBox();
            this.btn_more_info = new System.Windows.Forms.Button();
            this.btn_conf = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.img_dep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.img_update)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.img_logo)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_name
            // 
            this.lbl_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_name.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbl_name.Location = new System.Drawing.Point(5, 3);
            this.lbl_name.Name = "lbl_name";
            this.lbl_name.Size = new System.Drawing.Size(435, 18);
            this.lbl_name.TabIndex = 0;
            this.lbl_name.Text = "label1";
            this.lbl_name.Click += new System.EventHandler(this.lbl_name_Click);
            // 
            // btn_uninstall
            // 
            this.btn_uninstall.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btn_uninstall.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_uninstall.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btn_uninstall.Location = new System.Drawing.Point(153, 91);
            this.btn_uninstall.Name = "btn_uninstall";
            this.btn_uninstall.Size = new System.Drawing.Size(75, 23);
            this.btn_uninstall.TabIndex = 1;
            this.btn_uninstall.Text = "Uninstall";
            this.btn_uninstall.UseVisualStyleBackColor = false;
            this.btn_uninstall.Click += new System.EventHandler(this.btn_uninstall_Click);
            // 
            // lbl_description
            // 
            this.lbl_description.Location = new System.Drawing.Point(113, 21);
            this.lbl_description.Name = "lbl_description";
            this.lbl_description.Size = new System.Drawing.Size(432, 64);
            this.lbl_description.TabIndex = 3;
            this.lbl_description.Text = "label1";
            this.lbl_description.Click += new System.EventHandler(this.lbl_description_Click);
            // 
            // lbl_version
            // 
            this.lbl_version.AutoSize = true;
            this.lbl_version.Location = new System.Drawing.Point(498, 0);
            this.lbl_version.Name = "lbl_version";
            this.lbl_version.Size = new System.Drawing.Size(35, 13);
            this.lbl_version.TabIndex = 4;
            this.lbl_version.Text = "label1";
            this.lbl_version.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lbl_version.Click += new System.EventHandler(this.lbl_version_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            // 
            // btn_update
            // 
            this.btn_update.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btn_update.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_update.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btn_update.Location = new System.Drawing.Point(72, 91);
            this.btn_update.Name = "btn_update";
            this.btn_update.Size = new System.Drawing.Size(75, 23);
            this.btn_update.TabIndex = 6;
            this.btn_update.Text = "Update";
            this.btn_update.UseVisualStyleBackColor = false;
            this.btn_update.Click += new System.EventHandler(this.btn_update_Click);
            // 
            // img_dep
            // 
            this.img_dep.BackColor = System.Drawing.Color.Transparent;
            this.img_dep.Image = global::MpeInstaller.Properties.Resources.software_update_urgent;
            this.img_dep.Location = new System.Drawing.Point(75, 21);
            this.img_dep.Name = "img_dep";
            this.img_dep.Size = new System.Drawing.Size(32, 32);
            this.img_dep.TabIndex = 7;
            this.img_dep.TabStop = false;
            this.toolTip1.SetToolTip(this.img_dep, "Some of the dependency not met.\r\nThe extension may not work properlly\r\n Use More " +
                    "info button");
            this.img_dep.Click += new System.EventHandler(this.img_dep_Click);
            // 
            // img_update
            // 
            this.img_update.BackColor = System.Drawing.Color.Transparent;
            this.img_update.Image = global::MpeInstaller.Properties.Resources.software_update_available;
            this.img_update.Location = new System.Drawing.Point(75, 53);
            this.img_update.Name = "img_update";
            this.img_update.Size = new System.Drawing.Size(32, 32);
            this.img_update.TabIndex = 5;
            this.img_update.TabStop = false;
            this.toolTip1.SetToolTip(this.img_update, "New update available ");
            this.img_update.Click += new System.EventHandler(this.img_update_Click);
            // 
            // img_logo
            // 
            this.img_logo.Location = new System.Drawing.Point(6, 21);
            this.img_logo.Name = "img_logo";
            this.img_logo.Size = new System.Drawing.Size(64, 64);
            this.img_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.img_logo.TabIndex = 2;
            this.img_logo.TabStop = false;
            this.img_logo.Click += new System.EventHandler(this.img_logo_Click);
            // 
            // btn_more_info
            // 
            this.btn_more_info.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_more_info.Location = new System.Drawing.Point(237, 91);
            this.btn_more_info.Name = "btn_more_info";
            this.btn_more_info.Size = new System.Drawing.Size(75, 23);
            this.btn_more_info.TabIndex = 8;
            this.btn_more_info.Text = "More info.";
            this.btn_more_info.UseVisualStyleBackColor = true;
            // 
            // btn_conf
            // 
            this.btn_conf.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_conf.Location = new System.Drawing.Point(318, 91);
            this.btn_conf.Name = "btn_conf";
            this.btn_conf.Size = new System.Drawing.Size(75, 23);
            this.btn_conf.TabIndex = 9;
            this.btn_conf.Text = "Configure";
            this.btn_conf.UseVisualStyleBackColor = true;
            this.btn_conf.Click += new System.EventHandler(this.btn_conf_Click);
            // 
            // ExtensionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.btn_conf);
            this.Controls.Add(this.btn_more_info);
            this.Controls.Add(this.img_dep);
            this.Controls.Add(this.btn_update);
            this.Controls.Add(this.img_update);
            this.Controls.Add(this.lbl_version);
            this.Controls.Add(this.lbl_description);
            this.Controls.Add(this.img_logo);
            this.Controls.Add(this.btn_uninstall);
            this.Controls.Add(this.lbl_name);
            this.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Name = "ExtensionControl";
            this.Size = new System.Drawing.Size(548, 123);
            this.Click += new System.EventHandler(this.ExtensionControl_Click);
            ((System.ComponentModel.ISupportInitialize)(this.img_dep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.img_update)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.img_logo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_name;
        private System.Windows.Forms.Button btn_uninstall;
        private System.Windows.Forms.PictureBox img_logo;
        private System.Windows.Forms.Label lbl_description;
        private System.Windows.Forms.Label lbl_version;
        private System.Windows.Forms.PictureBox img_update;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btn_update;
        private System.Windows.Forms.PictureBox img_dep;
        private System.Windows.Forms.Button btn_more_info;
        private System.Windows.Forms.Button btn_conf;
    }
}
