namespace MpeInstaller.Controls
{
  partial class ExtensionControlExpanded
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
      Package = null;
      UpdatePackage = null;
      if (disposing)
      {
        if (components != null) components.Dispose();
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
      this.lbl_description = new System.Windows.Forms.Label();
      this.lbl_version = new System.Windows.Forms.Label();
      this.img_dep = new System.Windows.Forms.PictureBox();
      this.img_update = new System.Windows.Forms.PictureBox();
      this.btn_home = new System.Windows.Forms.Button();
      this.btn_forum = new System.Windows.Forms.Button();
      this.btn_update = new System.Windows.Forms.Button();
      this.btn_conf = new System.Windows.Forms.Button();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.btn_install = new System.Windows.Forms.ToolStripSplitButton();
      this.img_logo = new System.Windows.Forms.PictureBox();
      this.btn_screenshot = new System.Windows.Forms.Button();
      this.chk_ignore = new System.Windows.Forms.CheckBox();
      this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
      this.panelTop = new System.Windows.Forms.Panel();
      this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
      this.lblAuthors = new System.Windows.Forms.Label();
      this.panelBottom = new System.Windows.Forms.Panel();
      this.panelMiddle = new System.Windows.Forms.Panel();
      this.panelMiddleLeft = new System.Windows.Forms.Panel();
      this.panelMiddleRight = new System.Windows.Forms.Panel();
      ((System.ComponentModel.ISupportInitialize)(this.img_dep)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.img_update)).BeginInit();
      this.toolStrip1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.img_logo)).BeginInit();
      this.flowLayoutPanel1.SuspendLayout();
      this.panelTop.SuspendLayout();
      this.flowLayoutPanel2.SuspendLayout();
      this.panelBottom.SuspendLayout();
      this.panelMiddle.SuspendLayout();
      this.panelMiddleLeft.SuspendLayout();
      this.panelMiddleRight.SuspendLayout();
      this.SuspendLayout();
      // 
      // lbl_name
      // 
      this.lbl_name.AutoEllipsis = true;
      this.lbl_name.AutoSize = true;
      this.lbl_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_name.ForeColor = System.Drawing.Color.Blue;
      this.lbl_name.Location = new System.Drawing.Point(0, 2);
      this.lbl_name.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.lbl_name.Name = "lbl_name";
      this.lbl_name.Padding = new System.Windows.Forms.Padding(1, 3, 0, 0);
      this.lbl_name.Size = new System.Drawing.Size(99, 16);
      this.lbl_name.TabIndex = 0;
      this.lbl_name.Text = "Extension Name";
      this.lbl_name.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btn_uninstall
      // 
      this.btn_uninstall.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.btn_uninstall.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btn_uninstall.ForeColor = System.Drawing.SystemColors.ControlText;
      this.btn_uninstall.Location = new System.Drawing.Point(84, 3);
      this.btn_uninstall.Name = "btn_uninstall";
      this.btn_uninstall.Size = new System.Drawing.Size(75, 23);
      this.btn_uninstall.TabIndex = 1;
      this.btn_uninstall.Text = "Uninstall";
      this.btn_uninstall.UseVisualStyleBackColor = false;
      this.btn_uninstall.Click += new System.EventHandler(this.btn_uninstall_Click);
      // 
      // lbl_description
      // 
      this.lbl_description.AutoEllipsis = true;
      this.lbl_description.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lbl_description.ForeColor = System.Drawing.Color.Blue;
      this.lbl_description.Location = new System.Drawing.Point(110, 0);
      this.lbl_description.Name = "lbl_description";
      this.lbl_description.Padding = new System.Windows.Forms.Padding(1);
      this.lbl_description.Size = new System.Drawing.Size(402, 73);
      this.lbl_description.TabIndex = 3;
      this.lbl_description.Text = "Here goes a good description.";
      // 
      // lbl_version
      // 
      this.lbl_version.Dock = System.Windows.Forms.DockStyle.Right;
      this.lbl_version.ForeColor = System.Drawing.Color.Black;
      this.lbl_version.Location = new System.Drawing.Point(448, 0);
      this.lbl_version.MaximumSize = new System.Drawing.Size(100, 0);
      this.lbl_version.MinimumSize = new System.Drawing.Size(100, 0);
      this.lbl_version.Name = "lbl_version";
      this.lbl_version.Size = new System.Drawing.Size(100, 20);
      this.lbl_version.TabIndex = 4;
      this.lbl_version.Text = "3.2.55.2365";
      this.lbl_version.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // img_dep
      // 
      this.img_dep.BackColor = System.Drawing.Color.Transparent;
      this.img_dep.Image = global::MpeInstaller.Properties.Resources.software_update_urgent;
      this.img_dep.Location = new System.Drawing.Point(74, 4);
      this.img_dep.Name = "img_dep";
      this.img_dep.Size = new System.Drawing.Size(32, 32);
      this.img_dep.TabIndex = 7;
      this.img_dep.TabStop = false;
      this.img_dep.Click += new System.EventHandler(this.img_dep_Click);
      // 
      // img_update
      // 
      this.img_update.BackColor = System.Drawing.Color.Transparent;
      this.img_update.Image = global::MpeInstaller.Properties.Resources.software_update_available;
      this.img_update.Location = new System.Drawing.Point(74, 36);
      this.img_update.Name = "img_update";
      this.img_update.Size = new System.Drawing.Size(32, 32);
      this.img_update.TabIndex = 5;
      this.img_update.TabStop = false;
      // 
      // btn_home
      // 
      this.btn_home.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btn_home.Image = global::MpeInstaller.Properties.Resources.internet_web_browser;
      this.btn_home.Location = new System.Drawing.Point(0, 39);
      this.btn_home.Name = "btn_home";
      this.btn_home.Size = new System.Drawing.Size(32, 32);
      this.btn_home.TabIndex = 12;
      this.btn_home.UseVisualStyleBackColor = true;
      this.btn_home.Click += new System.EventHandler(this.btn_home_Click);
      // 
      // btn_forum
      // 
      this.btn_forum.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btn_forum.Image = global::MpeInstaller.Properties.Resources.internet_group_chat;
      this.btn_forum.Location = new System.Drawing.Point(0, 2);
      this.btn_forum.Name = "btn_forum";
      this.btn_forum.Size = new System.Drawing.Size(32, 32);
      this.btn_forum.TabIndex = 32;
      this.btn_forum.UseVisualStyleBackColor = true;
      this.btn_forum.Click += new System.EventHandler(this.btn_forum_Click);
      // 
      // btn_update
      // 
      this.btn_update.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.btn_update.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btn_update.ForeColor = System.Drawing.SystemColors.ControlText;
      this.btn_update.Location = new System.Drawing.Point(165, 3);
      this.btn_update.Name = "btn_update";
      this.btn_update.Size = new System.Drawing.Size(75, 23);
      this.btn_update.TabIndex = 6;
      this.btn_update.Text = "Update";
      this.btn_update.UseVisualStyleBackColor = false;
      this.btn_update.Click += new System.EventHandler(this.btn_update_Click);
      // 
      // btn_conf
      // 
      this.btn_conf.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btn_conf.Location = new System.Drawing.Point(246, 3);
      this.btn_conf.Name = "btn_conf";
      this.btn_conf.Size = new System.Drawing.Size(75, 23);
      this.btn_conf.TabIndex = 9;
      this.btn_conf.Text = "Configure";
      this.btn_conf.UseVisualStyleBackColor = true;
      this.btn_conf.Click += new System.EventHandler(this.btn_conf_Click);
      // 
      // toolStrip1
      // 
      this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
      this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btn_install});
      this.toolStrip1.Location = new System.Drawing.Point(4, 2);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Padding = new System.Windows.Forms.Padding(1);
      this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
      this.toolStrip1.Size = new System.Drawing.Size(79, 25);
      this.toolStrip1.TabIndex = 10;
      this.toolStrip1.Text = "toolStrip1";
      this.toolStrip1.Visible = false;
      // 
      // btn_install
      // 
      this.btn_install.DropDownButtonWidth = 16;
      this.btn_install.ForeColor = System.Drawing.SystemColors.ControlText;
      this.btn_install.Image = global::MpeInstaller.Properties.Resources.system_software_update;
      this.btn_install.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_install.Margin = new System.Windows.Forms.Padding(0);
      this.btn_install.Name = "btn_install";
      this.btn_install.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
      this.btn_install.Size = new System.Drawing.Size(75, 25);
      this.btn_install.Text = "Install";
      this.btn_install.ButtonClick += new System.EventHandler(this.btn_install_ButtonClick);
      // 
      // img_logo
      // 
      this.img_logo.Image = global::MpeInstaller.Properties.Resources.package_x_generic;
      this.img_logo.Location = new System.Drawing.Point(4, 4);
      this.img_logo.Name = "img_logo";
      this.img_logo.Size = new System.Drawing.Size(64, 64);
      this.img_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.img_logo.TabIndex = 2;
      this.img_logo.TabStop = false;
      // 
      // btn_screenshot
      // 
      this.btn_screenshot.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btn_screenshot.Location = new System.Drawing.Point(3, 3);
      this.btn_screenshot.Name = "btn_screenshot";
      this.btn_screenshot.Size = new System.Drawing.Size(75, 23);
      this.btn_screenshot.TabIndex = 13;
      this.btn_screenshot.Text = "ScreenShots";
      this.btn_screenshot.UseVisualStyleBackColor = true;
      this.btn_screenshot.Click += new System.EventHandler(this.btn_screenshot_Click);
      // 
      // chk_ignore
      // 
      this.chk_ignore.AutoSize = true;
      this.chk_ignore.Dock = System.Windows.Forms.DockStyle.Right;
      this.chk_ignore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.chk_ignore.ForeColor = System.Drawing.SystemColors.ControlText;
      this.chk_ignore.Location = new System.Drawing.Point(454, 0);
      this.chk_ignore.Name = "chk_ignore";
      this.chk_ignore.Size = new System.Drawing.Size(94, 30);
      this.chk_ignore.TabIndex = 35;
      this.chk_ignore.Text = "Ignore updates";
      this.chk_ignore.UseVisualStyleBackColor = true;
      this.chk_ignore.CheckedChanged += new System.EventHandler(this.chk_ignore_CheckedChanged);
      // 
      // flowLayoutPanel1
      // 
      this.flowLayoutPanel1.AutoSize = true;
      this.flowLayoutPanel1.Controls.Add(this.btn_screenshot);
      this.flowLayoutPanel1.Controls.Add(this.btn_uninstall);
      this.flowLayoutPanel1.Controls.Add(this.btn_update);
      this.flowLayoutPanel1.Controls.Add(this.btn_conf);
      this.flowLayoutPanel1.Location = new System.Drawing.Point(81, 0);
      this.flowLayoutPanel1.Name = "flowLayoutPanel1";
      this.flowLayoutPanel1.Size = new System.Drawing.Size(328, 30);
      this.flowLayoutPanel1.TabIndex = 36;
      // 
      // panelTop
      // 
      this.panelTop.Controls.Add(this.flowLayoutPanel2);
      this.panelTop.Controls.Add(this.lbl_version);
      this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelTop.Location = new System.Drawing.Point(1, 1);
      this.panelTop.Name = "panelTop";
      this.panelTop.Size = new System.Drawing.Size(548, 20);
      this.panelTop.TabIndex = 37;
      // 
      // flowLayoutPanel2
      // 
      this.flowLayoutPanel2.Controls.Add(this.lbl_name);
      this.flowLayoutPanel2.Controls.Add(this.lblAuthors);
      this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 0);
      this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
      this.flowLayoutPanel2.Name = "flowLayoutPanel2";
      this.flowLayoutPanel2.Size = new System.Drawing.Size(448, 20);
      this.flowLayoutPanel2.TabIndex = 5;
      this.flowLayoutPanel2.WrapContents = false;
      // 
      // lblAuthors
      // 
      this.lblAuthors.AutoEllipsis = true;
      this.lblAuthors.AutoSize = true;
      this.lblAuthors.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblAuthors.ForeColor = System.Drawing.Color.RoyalBlue;
      this.lblAuthors.Location = new System.Drawing.Point(99, 2);
      this.lblAuthors.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.lblAuthors.Name = "lblAuthors";
      this.lblAuthors.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.lblAuthors.Size = new System.Drawing.Size(42, 16);
      this.lblAuthors.TabIndex = 45;
      this.lblAuthors.Text = "authors";
      this.lblAuthors.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // panelBottom
      // 
      this.panelBottom.Controls.Add(this.toolStrip1);
      this.panelBottom.Controls.Add(this.flowLayoutPanel1);
      this.panelBottom.Controls.Add(this.chk_ignore);
      this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panelBottom.Location = new System.Drawing.Point(1, 94);
      this.panelBottom.Name = "panelBottom";
      this.panelBottom.Size = new System.Drawing.Size(548, 30);
      this.panelBottom.TabIndex = 38;
      // 
      // panelMiddle
      // 
      this.panelMiddle.Controls.Add(this.lbl_description);
      this.panelMiddle.Controls.Add(this.panelMiddleLeft);
      this.panelMiddle.Controls.Add(this.panelMiddleRight);
      this.panelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelMiddle.Location = new System.Drawing.Point(1, 21);
      this.panelMiddle.Name = "panelMiddle";
      this.panelMiddle.Size = new System.Drawing.Size(548, 73);
      this.panelMiddle.TabIndex = 39;
      // 
      // panelMiddleLeft
      // 
      this.panelMiddleLeft.Controls.Add(this.img_update);
      this.panelMiddleLeft.Controls.Add(this.img_dep);
      this.panelMiddleLeft.Controls.Add(this.img_logo);
      this.panelMiddleLeft.Dock = System.Windows.Forms.DockStyle.Left;
      this.panelMiddleLeft.Location = new System.Drawing.Point(0, 0);
      this.panelMiddleLeft.Name = "panelMiddleLeft";
      this.panelMiddleLeft.Size = new System.Drawing.Size(110, 73);
      this.panelMiddleLeft.TabIndex = 1;
      // 
      // panelMiddleRight
      // 
      this.panelMiddleRight.Controls.Add(this.btn_home);
      this.panelMiddleRight.Controls.Add(this.btn_forum);
      this.panelMiddleRight.Dock = System.Windows.Forms.DockStyle.Right;
      this.panelMiddleRight.Location = new System.Drawing.Point(512, 0);
      this.panelMiddleRight.Name = "panelMiddleRight";
      this.panelMiddleRight.Padding = new System.Windows.Forms.Padding(0, 2, 4, 2);
      this.panelMiddleRight.Size = new System.Drawing.Size(36, 73);
      this.panelMiddleRight.TabIndex = 0;
      // 
      // ExtensionControlExpanded
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.AutoSize = true;
      this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
      this.Controls.Add(this.panelMiddle);
      this.Controls.Add(this.panelBottom);
      this.Controls.Add(this.panelTop);
      this.ForeColor = System.Drawing.SystemColors.ButtonFace;
      this.MinimumSize = new System.Drawing.Size(0, 125);
      this.Name = "ExtensionControlExpanded";
      this.Padding = new System.Windows.Forms.Padding(1);
      this.Size = new System.Drawing.Size(550, 125);
      ((System.ComponentModel.ISupportInitialize)(this.img_dep)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.img_update)).EndInit();
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.img_logo)).EndInit();
      this.flowLayoutPanel1.ResumeLayout(false);
      this.panelTop.ResumeLayout(false);
      this.flowLayoutPanel2.ResumeLayout(false);
      this.flowLayoutPanel2.PerformLayout();
      this.panelBottom.ResumeLayout(false);
      this.panelBottom.PerformLayout();
      this.panelMiddle.ResumeLayout(false);
      this.panelMiddleLeft.ResumeLayout(false);
      this.panelMiddleRight.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lbl_name;
    private System.Windows.Forms.Button btn_uninstall;
    private System.Windows.Forms.PictureBox img_logo;
    private System.Windows.Forms.Label lbl_description;
    private System.Windows.Forms.Label lbl_version;
    private System.Windows.Forms.PictureBox img_update;
    private System.Windows.Forms.Button btn_update;
    private System.Windows.Forms.PictureBox img_dep;
    private System.Windows.Forms.Button btn_conf;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripSplitButton btn_install;
    private System.Windows.Forms.Button btn_forum;
    private System.Windows.Forms.Button btn_home;
    private System.Windows.Forms.Button btn_screenshot;
    private System.Windows.Forms.CheckBox chk_ignore;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.Panel panelBottom;
    private System.Windows.Forms.Panel panelMiddle;
    private System.Windows.Forms.Panel panelMiddleLeft;
    private System.Windows.Forms.Panel panelMiddleRight;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    private System.Windows.Forms.Label lblAuthors;
  }
}
