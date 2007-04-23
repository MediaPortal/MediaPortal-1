namespace MediaPortal.Plugins.Process
{
  partial class SetupForm
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
      this.modeLabel = new System.Windows.Forms.Label();
      this.timeoutLabel = new System.Windows.Forms.Label();
      this.idleNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.generalGroupBox = new System.Windows.Forms.GroupBox();
      this.checkIntervalLabel2 = new System.Windows.Forms.Label();
      this.checkNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.checkIntervalLabel1 = new System.Windows.Forms.Label();
      this.extLogCheckBox = new System.Windows.Forms.CheckBox();
      this.homeOnlyCheckBox = new System.Windows.Forms.CheckBox();
      this.multiGroupBox = new System.Windows.Forms.GroupBox();
      this.wakeupNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.preWakeupLabel = new System.Windows.Forms.Label();
      this.forceCheckBox = new System.Windows.Forms.CheckBox();
      this.shutModeComboBox = new System.Windows.Forms.ComboBox();
      this.okButton = new System.Windows.Forms.Button();
      this.noShutdownNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.idleNumericUpDown)).BeginInit();
      this.generalGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.checkNumericUpDown)).BeginInit();
      this.multiGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.wakeupNumericUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.noShutdownNumericUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // modeLabel
      // 
      this.modeLabel.AutoSize = true;
      this.modeLabel.Location = new System.Drawing.Point(36, 23);
      this.modeLabel.Name = "modeLabel";
      this.modeLabel.Size = new System.Drawing.Size(87, 13);
      this.modeLabel.TabIndex = 5;
      this.modeLabel.Text = "Shutdown mode:";
      // 
      // timeoutLabel
      // 
      this.timeoutLabel.AutoSize = true;
      this.timeoutLabel.Location = new System.Drawing.Point(16, 47);
      this.timeoutLabel.Name = "timeoutLabel";
      this.timeoutLabel.Size = new System.Drawing.Size(164, 13);
      this.timeoutLabel.TabIndex = 7;
      this.timeoutLabel.Text = "Shutdown idle timeout in minutes:";
      // 
      // idleNumericUpDown
      // 
      this.idleNumericUpDown.Location = new System.Drawing.Point(202, 45);
      this.idleNumericUpDown.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
      this.idleNumericUpDown.Name = "idleNumericUpDown";
      this.idleNumericUpDown.Size = new System.Drawing.Size(46, 20);
      this.idleNumericUpDown.TabIndex = 8;
      this.idleNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // generalGroupBox
      // 
      this.generalGroupBox.Controls.Add(this.checkIntervalLabel2);
      this.generalGroupBox.Controls.Add(this.checkNumericUpDown);
      this.generalGroupBox.Controls.Add(this.checkIntervalLabel1);
      this.generalGroupBox.Controls.Add(this.extLogCheckBox);
      this.generalGroupBox.Controls.Add(this.homeOnlyCheckBox);
      this.generalGroupBox.Location = new System.Drawing.Point(13, 13);
      this.generalGroupBox.Name = "generalGroupBox";
      this.generalGroupBox.Size = new System.Drawing.Size(263, 91);
      this.generalGroupBox.TabIndex = 11;
      this.generalGroupBox.TabStop = false;
      this.generalGroupBox.Text = "General settings";
      // 
      // checkIntervalLabel2
      // 
      this.checkIntervalLabel2.AutoSize = true;
      this.checkIntervalLabel2.Location = new System.Drawing.Point(144, 66);
      this.checkIntervalLabel2.Name = "checkIntervalLabel2";
      this.checkIntervalLabel2.Size = new System.Drawing.Size(47, 13);
      this.checkIntervalLabel2.TabIndex = 4;
      this.checkIntervalLabel2.Text = "seconds";
      // 
      // checkNumericUpDown
      // 
      this.checkNumericUpDown.Location = new System.Drawing.Point(97, 64);
      this.checkNumericUpDown.Name = "checkNumericUpDown";
      this.checkNumericUpDown.Size = new System.Drawing.Size(44, 20);
      this.checkNumericUpDown.TabIndex = 3;
      this.checkNumericUpDown.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
      // 
      // checkIntervalLabel1
      // 
      this.checkIntervalLabel1.AutoSize = true;
      this.checkIntervalLabel1.Location = new System.Drawing.Point(22, 66);
      this.checkIntervalLabel1.Name = "checkIntervalLabel1";
      this.checkIntervalLabel1.Size = new System.Drawing.Size(78, 13);
      this.checkIntervalLabel1.TabIndex = 2;
      this.checkIntervalLabel1.Text = "Check interval:";
      // 
      // extLogCheckBox
      // 
      this.extLogCheckBox.AutoSize = true;
      this.extLogCheckBox.Location = new System.Drawing.Point(22, 42);
      this.extLogCheckBox.Name = "extLogCheckBox";
      this.extLogCheckBox.Size = new System.Drawing.Size(144, 17);
      this.extLogCheckBox.TabIndex = 1;
      this.extLogCheckBox.Text = "Enable extensive logging";
      this.extLogCheckBox.UseVisualStyleBackColor = true;
      // 
      // homeOnlyCheckBox
      // 
      this.homeOnlyCheckBox.AutoSize = true;
      this.homeOnlyCheckBox.Checked = true;
      this.homeOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.homeOnlyCheckBox.Location = new System.Drawing.Point(22, 19);
      this.homeOnlyCheckBox.Name = "homeOnlyCheckBox";
      this.homeOnlyCheckBox.Size = new System.Drawing.Size(226, 17);
      this.homeOnlyCheckBox.TabIndex = 0;
      this.homeOnlyCheckBox.Text = "Only allow standby when on home window";
      this.homeOnlyCheckBox.UseVisualStyleBackColor = true;
      // 
      // multiGroupBox
      // 
      this.multiGroupBox.Controls.Add(this.noShutdownNumericUpDown);
      this.multiGroupBox.Controls.Add(this.label1);
      this.multiGroupBox.Controls.Add(this.wakeupNumericUpDown);
      this.multiGroupBox.Controls.Add(this.preWakeupLabel);
      this.multiGroupBox.Controls.Add(this.forceCheckBox);
      this.multiGroupBox.Controls.Add(this.shutModeComboBox);
      this.multiGroupBox.Controls.Add(this.modeLabel);
      this.multiGroupBox.Controls.Add(this.timeoutLabel);
      this.multiGroupBox.Controls.Add(this.idleNumericUpDown);
      this.multiGroupBox.Location = new System.Drawing.Point(13, 111);
      this.multiGroupBox.Name = "multiGroupBox";
      this.multiGroupBox.Size = new System.Drawing.Size(263, 145);
      this.multiGroupBox.TabIndex = 12;
      this.multiGroupBox.TabStop = false;
      this.multiGroupBox.Text = "Client/server local settings";
      // 
      // wakeupNumericUpDown
      // 
      this.wakeupNumericUpDown.Location = new System.Drawing.Point(202, 91);
      this.wakeupNumericUpDown.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.wakeupNumericUpDown.Name = "wakeupNumericUpDown";
      this.wakeupNumericUpDown.Size = new System.Drawing.Size(46, 20);
      this.wakeupNumericUpDown.TabIndex = 11;
      this.wakeupNumericUpDown.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
      // 
      // preWakeupLabel
      // 
      this.preWakeupLabel.AutoSize = true;
      this.preWakeupLabel.Location = new System.Drawing.Point(38, 94);
      this.preWakeupLabel.Name = "preWakeupLabel";
      this.preWakeupLabel.Size = new System.Drawing.Size(143, 13);
      this.preWakeupLabel.TabIndex = 10;
      this.preWakeupLabel.Text = "Pre-wakeup time in seconds:";
      // 
      // forceCheckBox
      // 
      this.forceCheckBox.AutoSize = true;
      this.forceCheckBox.Location = new System.Drawing.Point(39, 71);
      this.forceCheckBox.Name = "forceCheckBox";
      this.forceCheckBox.Size = new System.Drawing.Size(195, 17);
      this.forceCheckBox.TabIndex = 9;
      this.forceCheckBox.Text = "Force the system into standby mode";
      this.forceCheckBox.UseVisualStyleBackColor = true;
      // 
      // shutModeComboBox
      // 
      this.shutModeComboBox.FormattingEnabled = true;
      this.shutModeComboBox.Items.AddRange(new object[] {
            "Suspend",
            "Hibernate",
            "Stay On"});
      this.shutModeComboBox.Location = new System.Drawing.Point(138, 20);
      this.shutModeComboBox.Name = "shutModeComboBox";
      this.shutModeComboBox.Size = new System.Drawing.Size(110, 21);
      this.shutModeComboBox.TabIndex = 6;
      this.shutModeComboBox.Text = "suspend";
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(110, 262);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 12;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // noShutdownNumericUpDown
      // 
      this.noShutdownNumericUpDown.Location = new System.Drawing.Point(202, 114);
      this.noShutdownNumericUpDown.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.noShutdownNumericUpDown.Name = "noShutdownNumericUpDown";
      this.noShutdownNumericUpDown.Size = new System.Drawing.Size(46, 20);
      this.noShutdownNumericUpDown.TabIndex = 13;
      this.noShutdownNumericUpDown.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
      this.noShutdownNumericUpDown.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(38, 117);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(163, 13);
      this.label1.TabIndex = 12;
      this.label1.Text = "Pre-no-shutdown time in seconds";
      this.label1.Click += new System.EventHandler(this.label1_Click);
      // 
      // SetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(288, 310);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.multiGroupBox);
      this.Controls.Add(this.generalGroupBox);
      this.Name = "SetupForm";
      this.Text = "PowerScheduler settings";
      ((System.ComponentModel.ISupportInitialize)(this.idleNumericUpDown)).EndInit();
      this.generalGroupBox.ResumeLayout(false);
      this.generalGroupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.checkNumericUpDown)).EndInit();
      this.multiGroupBox.ResumeLayout(false);
      this.multiGroupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.wakeupNumericUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.noShutdownNumericUpDown)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label modeLabel;
    private System.Windows.Forms.Label timeoutLabel;
    private System.Windows.Forms.NumericUpDown idleNumericUpDown;
    private System.Windows.Forms.GroupBox generalGroupBox;
    private System.Windows.Forms.CheckBox homeOnlyCheckBox;
    private System.Windows.Forms.CheckBox extLogCheckBox;
    private System.Windows.Forms.GroupBox multiGroupBox;
    private System.Windows.Forms.CheckBox forceCheckBox;
    private System.Windows.Forms.ComboBox shutModeComboBox;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Label checkIntervalLabel2;
    private System.Windows.Forms.NumericUpDown checkNumericUpDown;
    private System.Windows.Forms.Label checkIntervalLabel1;
    private System.Windows.Forms.NumericUpDown wakeupNumericUpDown;
    private System.Windows.Forms.Label preWakeupLabel;
    private System.Windows.Forms.NumericUpDown noShutdownNumericUpDown;
    private System.Windows.Forms.Label label1;
  }
}