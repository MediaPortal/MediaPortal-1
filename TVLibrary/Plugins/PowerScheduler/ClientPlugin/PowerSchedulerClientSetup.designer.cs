namespace MediaPortal.Plugins.Process
{
  partial class PowerSchedulerClientSetup
  {
    private System.Windows.Forms.ToolTip toolTip1;
    private System.ComponentModel.IContainer components;
  
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
      this.components = new System.ComponentModel.Container();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.extLogCheckBox = new System.Windows.Forms.CheckBox();
      this.forceCheckBox = new System.Windows.Forms.CheckBox();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.homeOnlyCheckBox = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.shutModeComboBox = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.idleNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.enableShutdownCheckBox = new System.Windows.Forms.CheckBox();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.cancelButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.tabPage2.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.idleNumericUpDown)).BeginInit();
      this.tabControl1.SuspendLayout();
      this.SuspendLayout();
      // 
      // toolTip1
      // 
      this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
      this.toolTip1.ToolTipTitle = "Help";
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
      this.openFileDialog1.Title = "Choose command";
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.groupBox2);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(430, 153);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Advanced";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.extLogCheckBox);
      this.groupBox2.Controls.Add(this.forceCheckBox);
      this.groupBox2.Location = new System.Drawing.Point(6, 6);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(418, 99);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Advanced settings";
      // 
      // extLogCheckBox
      // 
      this.extLogCheckBox.AutoSize = true;
      this.extLogCheckBox.Location = new System.Drawing.Point(38, 65);
      this.extLogCheckBox.Name = "extLogCheckBox";
      this.extLogCheckBox.Size = new System.Drawing.Size(144, 17);
      this.extLogCheckBox.TabIndex = 1;
      this.extLogCheckBox.Text = "Enable extensive logging";
      this.extLogCheckBox.UseVisualStyleBackColor = true;
      // 
      // forceCheckBox
      // 
      this.forceCheckBox.AutoSize = true;
      this.forceCheckBox.Location = new System.Drawing.Point(38, 32);
      this.forceCheckBox.Name = "forceCheckBox";
      this.forceCheckBox.Size = new System.Drawing.Size(363, 17);
      this.forceCheckBox.TabIndex = 0;
      this.forceCheckBox.Text = "Forced shutdown (shutdown even when prevented by another process)";
      this.forceCheckBox.UseVisualStyleBackColor = true;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.groupBox1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(430, 153);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "General";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.homeOnlyCheckBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.shutModeComboBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.idleNumericUpDown);
      this.groupBox1.Controls.Add(this.enableShutdownCheckBox);
      this.groupBox1.Location = new System.Drawing.Point(6, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(418, 139);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Standby / Wakeup settings";
      // 
      // homeOnlyCheckBox
      // 
      this.homeOnlyCheckBox.AutoSize = true;
      this.homeOnlyCheckBox.Checked = true;
      this.homeOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.homeOnlyCheckBox.Location = new System.Drawing.Point(37, 67);
      this.homeOnlyCheckBox.Name = "homeOnlyCheckBox";
      this.homeOnlyCheckBox.Size = new System.Drawing.Size(226, 17);
      this.homeOnlyCheckBox.TabIndex = 6;
      this.homeOnlyCheckBox.Text = "Only allow standby when on home window";
      this.homeOnlyCheckBox.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(37, 102);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(84, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "Shutdown mode";
      // 
      // shutModeComboBox
      // 
      this.shutModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.shutModeComboBox.FormattingEnabled = true;
      this.shutModeComboBox.Items.AddRange(new object[] {
            "Suspend",
            "Hibernate"});
      this.shutModeComboBox.Location = new System.Drawing.Point(127, 99);
      this.shutModeComboBox.Name = "shutModeComboBox";
      this.shutModeComboBox.Size = new System.Drawing.Size(137, 21);
      this.shutModeComboBox.TabIndex = 4;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(270, 36);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(43, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "minutes";
      // 
      // idleNumericUpDown
      // 
      this.idleNumericUpDown.Location = new System.Drawing.Point(213, 31);
      this.idleNumericUpDown.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
      this.idleNumericUpDown.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
      this.idleNumericUpDown.Name = "idleNumericUpDown";
      this.idleNumericUpDown.Size = new System.Drawing.Size(51, 20);
      this.idleNumericUpDown.TabIndex = 1;
      this.idleNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // enableShutdownCheckBox
      // 
      this.enableShutdownCheckBox.AutoSize = true;
      this.enableShutdownCheckBox.Location = new System.Drawing.Point(37, 32);
      this.enableShutdownCheckBox.Name = "enableShutdownCheckBox";
      this.enableShutdownCheckBox.Size = new System.Drawing.Size(174, 17);
      this.enableShutdownCheckBox.TabIndex = 0;
      this.enableShutdownCheckBox.Text = "Shutdown client after being idle";
      this.enableShutdownCheckBox.UseVisualStyleBackColor = true;
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.ShowToolTips = true;
      this.tabControl1.Size = new System.Drawing.Size(438, 179);
      this.tabControl1.TabIndex = 0;
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.Location = new System.Drawing.Point(336, 196);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 15;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.okButton.Location = new System.Drawing.Point(45, 196);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 14;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // PowerSchedulerClientSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(441, 231);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.tabControl1);
      this.Name = "PowerSchedulerClientSetup";
      this.Text = "PowerSchedulerClientSetup";
      this.tabPage2.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.tabPage1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.idleNumericUpDown)).EndInit();
      this.tabControl1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox extLogCheckBox;
    private System.Windows.Forms.CheckBox forceCheckBox;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox shutModeComboBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown idleNumericUpDown;
    private System.Windows.Forms.CheckBox enableShutdownCheckBox;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.CheckBox homeOnlyCheckBox;
  }
}