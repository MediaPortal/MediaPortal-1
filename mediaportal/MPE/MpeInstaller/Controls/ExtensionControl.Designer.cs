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
            this.lbl_name = new System.Windows.Forms.Label();
            this.btn_uninstall = new System.Windows.Forms.Button();
            this.img_logo = new System.Windows.Forms.PictureBox();
            this.lbl_description = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.img_logo)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_name
            // 
            this.lbl_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_name.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbl_name.Location = new System.Drawing.Point(3, 0);
            this.lbl_name.Name = "lbl_name";
            this.lbl_name.Size = new System.Drawing.Size(544, 18);
            this.lbl_name.TabIndex = 0;
            this.lbl_name.Text = "label1";
            this.lbl_name.Click += new System.EventHandler(this.lbl_name_Click);
            // 
            // btn_uninstall
            // 
            this.btn_uninstall.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_uninstall.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btn_uninstall.Location = new System.Drawing.Point(6, 97);
            this.btn_uninstall.Name = "btn_uninstall";
            this.btn_uninstall.Size = new System.Drawing.Size(75, 23);
            this.btn_uninstall.TabIndex = 1;
            this.btn_uninstall.Text = "Uninstall";
            this.btn_uninstall.UseVisualStyleBackColor = true;
            this.btn_uninstall.Click += new System.EventHandler(this.btn_uninstall_Click);
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
            // lbl_description
            // 
            this.lbl_description.Location = new System.Drawing.Point(113, 21);
            this.lbl_description.Name = "lbl_description";
            this.lbl_description.Size = new System.Drawing.Size(432, 64);
            this.lbl_description.TabIndex = 3;
            this.lbl_description.Text = "label1";
            this.lbl_description.Click += new System.EventHandler(this.lbl_description_Click);
            // 
            // ExtensionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.lbl_description);
            this.Controls.Add(this.img_logo);
            this.Controls.Add(this.btn_uninstall);
            this.Controls.Add(this.lbl_name);
            this.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Name = "ExtensionControl";
            this.Size = new System.Drawing.Size(548, 123);
            this.Click += new System.EventHandler(this.ExtensionControl_Click);
            ((System.ComponentModel.ISupportInitialize)(this.img_logo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbl_name;
        private System.Windows.Forms.Button btn_uninstall;
        private System.Windows.Forms.PictureBox img_logo;
        private System.Windows.Forms.Label lbl_description;
    }
}
