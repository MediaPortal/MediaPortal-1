namespace MpeInstaller.Controls
{
  partial class ExtensionControlCollapsed
  {
    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null) components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Komponenten-Designer generierter Code

    /// <summary> 
    /// Erforderliche Methode für die Designerunterstützung. 
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.lbl_version = new System.Windows.Forms.Label();
      this.lbl_name = new System.Windows.Forms.Label();
      this.img_dep = new System.Windows.Forms.PictureBox();
      this.img_update = new System.Windows.Forms.PictureBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.lblAuthors = new System.Windows.Forms.Label();
      this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
      ((System.ComponentModel.ISupportInitialize)(this.img_dep)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.img_update)).BeginInit();
      this.panel1.SuspendLayout();
      this.flowLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // lbl_version
      // 
      this.lbl_version.Dock = System.Windows.Forms.DockStyle.Right;
      this.lbl_version.ForeColor = System.Drawing.Color.Black;
      this.lbl_version.Location = new System.Drawing.Point(50, 0);
      this.lbl_version.MaximumSize = new System.Drawing.Size(100, 0);
      this.lbl_version.MinimumSize = new System.Drawing.Size(100, 0);
      this.lbl_version.Name = "lbl_version";
      this.lbl_version.Size = new System.Drawing.Size(100, 20);
      this.lbl_version.TabIndex = 40;
      this.lbl_version.Text = "3.2.55.2365";
      this.lbl_version.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.lbl_version.Click += new System.EventHandler(this.ExtensionControlCollapsed_Click);
      // 
      // lbl_name
      // 
      this.lbl_name.AutoEllipsis = true;
      this.lbl_name.AutoSize = true;
      this.lbl_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_name.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lbl_name.Location = new System.Drawing.Point(0, 2);
      this.lbl_name.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.lbl_name.Name = "lbl_name";
      this.lbl_name.Padding = new System.Windows.Forms.Padding(1, 3, 0, 0);
      this.lbl_name.Size = new System.Drawing.Size(99, 16);
      this.lbl_name.TabIndex = 0;
      this.lbl_name.Text = "Extension Name";
      this.lbl_name.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lbl_name.Click += new System.EventHandler(this.ExtensionControlCollapsed_Click);
      // 
      // img_dep
      // 
      this.img_dep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.img_dep.BackColor = System.Drawing.Color.Transparent;
      this.img_dep.Image = global::MpeInstaller.Properties.Resources.software_update_urgent;
      this.img_dep.Location = new System.Drawing.Point(25, 1);
      this.img_dep.Name = "img_dep";
      this.img_dep.Size = new System.Drawing.Size(20, 20);
      this.img_dep.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.img_dep.TabIndex = 42;
      this.img_dep.TabStop = false;
      this.img_dep.Click += new System.EventHandler(this.img_dep_Click);
      // 
      // img_update
      // 
      this.img_update.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.img_update.BackColor = System.Drawing.Color.Transparent;
      this.img_update.Image = global::MpeInstaller.Properties.Resources.software_update_available;
      this.img_update.Location = new System.Drawing.Point(0, 1);
      this.img_update.Name = "img_update";
      this.img_update.Size = new System.Drawing.Size(20, 20);
      this.img_update.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.img_update.TabIndex = 41;
      this.img_update.TabStop = false;
      this.img_update.Click += new System.EventHandler(this.ExtensionControlCollapsed_Click);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.img_dep);
      this.panel1.Controls.Add(this.lbl_version);
      this.panel1.Controls.Add(this.img_update);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel1.Location = new System.Drawing.Point(399, 1);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(150, 20);
      this.panel1.TabIndex = 43;
      this.panel1.Click += new System.EventHandler(this.ExtensionControlCollapsed_Click);
      // 
      // lblAuthors
      // 
      this.lblAuthors.AutoEllipsis = true;
      this.lblAuthors.AutoSize = true;
      this.lblAuthors.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblAuthors.ForeColor = System.Drawing.Color.DimGray;
      this.lblAuthors.Location = new System.Drawing.Point(99, 2);
      this.lblAuthors.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.lblAuthors.Name = "lblAuthors";
      this.lblAuthors.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.lblAuthors.Size = new System.Drawing.Size(42, 16);
      this.lblAuthors.TabIndex = 44;
      this.lblAuthors.Text = "authors";
      this.lblAuthors.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      this.lblAuthors.Click += new System.EventHandler(this.ExtensionControlCollapsed_Click);
      // 
      // flowLayoutPanel1
      // 
      this.flowLayoutPanel1.Controls.Add(this.lbl_name);
      this.flowLayoutPanel1.Controls.Add(this.lblAuthors);
      this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.flowLayoutPanel1.Location = new System.Drawing.Point(1, 1);
      this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.flowLayoutPanel1.Name = "flowLayoutPanel1";
      this.flowLayoutPanel1.Size = new System.Drawing.Size(398, 20);
      this.flowLayoutPanel1.TabIndex = 45;
      this.flowLayoutPanel1.WrapContents = false;
      this.flowLayoutPanel1.Click += new System.EventHandler(this.ExtensionControlCollapsed_Click);
      // 
      // ExtensionControlCollapsed
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.AutoSize = true;
      this.Controls.Add(this.flowLayoutPanel1);
      this.Controls.Add(this.panel1);
      this.MinimumSize = new System.Drawing.Size(0, 22);
      this.Name = "ExtensionControlCollapsed";
      this.Padding = new System.Windows.Forms.Padding(1);
      this.Size = new System.Drawing.Size(550, 22);
      this.Click += new System.EventHandler(this.ExtensionControlCollapsed_Click);
      ((System.ComponentModel.ISupportInitialize)(this.img_dep)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.img_update)).EndInit();
      this.panel1.ResumeLayout(false);
      this.flowLayoutPanel1.ResumeLayout(false);
      this.flowLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox img_dep;
    private System.Windows.Forms.PictureBox img_update;
    private System.Windows.Forms.Label lbl_version;
    private System.Windows.Forms.Label lbl_name;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label lblAuthors;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
  }
}
