namespace MpeInstaller.Dialogs
{
  partial class DependencyForm
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel3 = new System.Windows.Forms.Panel();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.dataGridView2 = new System.Windows.Forms.DataGridView();
      this.panel4 = new System.Windows.Forms.Panel();
      this.pluginLabel = new System.Windows.Forms.Label();
      this.versionLabel = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.skindepLabel = new System.Windows.Forms.Label();
      this.MPdepLabel = new System.Windows.Forms.Label();
      this.panel1.SuspendLayout();
      this.panel3.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      this.tabPage2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
      this.panel4.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.panel3);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(584, 312);
      this.panel1.TabIndex = 0;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.tabControl1);
      this.panel3.Controls.Add(this.panel4);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel3.Location = new System.Drawing.Point(0, 0);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(584, 312);
      this.panel3.TabIndex = 1;
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(584, 236);
      this.tabControl1.TabIndex = 2;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.dataGridView1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(576, 210);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "General";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // dataGridView1
      // 
      this.dataGridView1.AllowUserToAddRows = false;
      this.dataGridView1.AllowUserToDeleteRows = false;
      this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Window;
      this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView1.Location = new System.Drawing.Point(3, 3);
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.ReadOnly = true;
      this.dataGridView1.Size = new System.Drawing.Size(570, 204);
      this.dataGridView1.TabIndex = 0;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.dataGridView2);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(576, 210);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Plugins";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // dataGridView2
      // 
      this.dataGridView2.AllowUserToAddRows = false;
      this.dataGridView2.AllowUserToDeleteRows = false;
      this.dataGridView2.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.dataGridView2.BackgroundColor = System.Drawing.SystemColors.Window;
      this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView2.Location = new System.Drawing.Point(3, 3);
      this.dataGridView2.Name = "dataGridView2";
      this.dataGridView2.ReadOnly = true;
      this.dataGridView2.Size = new System.Drawing.Size(570, 204);
      this.dataGridView2.TabIndex = 0;
      // 
      // panel4
      // 
      this.panel4.Controls.Add(this.pluginLabel);
      this.panel4.Controls.Add(this.versionLabel);
      this.panel4.Controls.Add(this.label1);
      this.panel4.Controls.Add(this.skindepLabel);
      this.panel4.Controls.Add(this.MPdepLabel);
      this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel4.Location = new System.Drawing.Point(0, 236);
      this.panel4.Name = "panel4";
      this.panel4.Size = new System.Drawing.Size(584, 76);
      this.panel4.TabIndex = 1;
      // 
      // pluginLabel
      // 
      this.pluginLabel.AutoSize = true;
      this.pluginLabel.ForeColor = System.Drawing.Color.Red;
      this.pluginLabel.Location = new System.Drawing.Point(4, 36);
      this.pluginLabel.Name = "pluginLabel";
      this.pluginLabel.Size = new System.Drawing.Size(155, 13);
      this.pluginLabel.TabIndex = 5;
      this.pluginLabel.Text = "No plugin dependencies found!";
      this.pluginLabel.Visible = false;
      // 
      // versionLabel
      // 
      this.versionLabel.AutoSize = true;
      this.versionLabel.Location = new System.Drawing.Point(85, 58);
      this.versionLabel.Name = "versionLabel";
      this.versionLabel.Size = new System.Drawing.Size(0, 13);
      this.versionLabel.TabIndex = 4;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(4, 58);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(81, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Current version:";
      // 
      // skindepLabel
      // 
      this.skindepLabel.AutoSize = true;
      this.skindepLabel.ForeColor = System.Drawing.Color.Red;
      this.skindepLabel.Location = new System.Drawing.Point(4, 22);
      this.skindepLabel.Name = "skindepLabel";
      this.skindepLabel.Size = new System.Drawing.Size(138, 13);
      this.skindepLabel.TabIndex = 2;
      this.skindepLabel.Text = "No skin dependency found!";
      this.skindepLabel.Visible = false;
      // 
      // MPdepLabel
      // 
      this.MPdepLabel.AutoSize = true;
      this.MPdepLabel.ForeColor = System.Drawing.Color.Red;
      this.MPdepLabel.Location = new System.Drawing.Point(3, 9);
      this.MPdepLabel.Name = "MPdepLabel";
      this.MPdepLabel.Size = new System.Drawing.Size(175, 13);
      this.MPdepLabel.TabIndex = 1;
      this.MPdepLabel.Text = "No MediaPortal dependency found!";
      this.MPdepLabel.Visible = false;
      // 
      // DependencyForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(584, 312);
      this.Controls.Add(this.panel1);
      this.MinimumSize = new System.Drawing.Size(455, 263);
      this.Name = "DependencyForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Dependencies";
      this.panel1.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      this.tabPage2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
      this.panel4.ResumeLayout(false);
      this.panel4.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.DataGridView dataGridView1;
    private System.Windows.Forms.Label skindepLabel;
    private System.Windows.Forms.Label MPdepLabel;
    private System.Windows.Forms.Panel panel4;
    private System.Windows.Forms.Label versionLabel;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.DataGridView dataGridView2;
    private System.Windows.Forms.Label pluginLabel;
  }
}
