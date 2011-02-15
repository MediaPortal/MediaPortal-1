namespace MediaPortal.Configuration.Sections
{
  partial class General
  {
    /// <summary> 
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
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
      this.mpThreadPriority = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lbDebug = new System.Windows.Forms.Label();
      this.groupBoxGeneralSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelPriority = new System.Windows.Forms.Label();
      this.cbDebug = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.watchdogGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.numericUpDownDelay = new System.Windows.Forms.NumericUpDown();
      this.checkBoxAutoRestart = new System.Windows.Forms.CheckBox();
      this.checkBoxEnableWatchdog = new System.Windows.Forms.CheckBox();
      this.label6 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.watchdogGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).BeginInit();
      this.SuspendLayout();
      // 
      // mpThreadPriority
      // 
      this.mpThreadPriority.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpThreadPriority.BorderColor = System.Drawing.Color.Empty;
      this.mpThreadPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpThreadPriority.FormattingEnabled = true;
      this.mpThreadPriority.Items.AddRange(new object[] {
            "High",
            "AboveNormal",
            "Normal",
            "BelowNormal"});
      this.mpThreadPriority.Location = new System.Drawing.Point(329, 19);
      this.mpThreadPriority.MinimumSize = new System.Drawing.Size(100, 0);
      this.mpThreadPriority.Name = "mpThreadPriority";
      this.mpThreadPriority.Size = new System.Drawing.Size(131, 21);
      this.mpThreadPriority.TabIndex = 2;
      // 
      // lbDebug
      // 
      this.lbDebug.AutoSize = true;
      this.lbDebug.Location = new System.Drawing.Point(6, 22);
      this.lbDebug.Name = "lbDebug";
      this.lbDebug.Size = new System.Drawing.Size(73, 13);
      this.lbDebug.TabIndex = 3;
      this.lbDebug.Text = "Log verbosity:";
      // 
      // groupBoxGeneralSettings
      // 
      this.groupBoxGeneralSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGeneralSettings.Controls.Add(this.labelPriority);
      this.groupBoxGeneralSettings.Controls.Add(this.mpThreadPriority);
      this.groupBoxGeneralSettings.Controls.Add(this.lbDebug);
      this.groupBoxGeneralSettings.Controls.Add(this.cbDebug);
      this.groupBoxGeneralSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(3, 0);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(466, 53);
      this.groupBoxGeneralSettings.TabIndex = 1;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "General settings";
      // 
      // labelPriority
      // 
      this.labelPriority.AutoSize = true;
      this.labelPriority.Location = new System.Drawing.Point(242, 22);
      this.labelPriority.Name = "labelPriority";
      this.labelPriority.Size = new System.Drawing.Size(81, 13);
      this.labelPriority.TabIndex = 1;
      this.labelPriority.Text = "Process priority:";
      // 
      // cbDebug
      // 
      this.cbDebug.BorderColor = System.Drawing.Color.Empty;
      this.cbDebug.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDebug.FormattingEnabled = true;
      this.cbDebug.Items.AddRange(new object[] {
            "Error",
            "Warning",
            "Information",
            "Debug"});
      this.cbDebug.Location = new System.Drawing.Point(85, 19);
      this.cbDebug.MinimumSize = new System.Drawing.Size(100, 0);
      this.cbDebug.Name = "cbDebug";
      this.cbDebug.Size = new System.Drawing.Size(131, 21);
      this.cbDebug.TabIndex = 4;
      // 
      // watchdogGroupBox
      // 
      this.watchdogGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.watchdogGroupBox.Controls.Add(this.label1);
      this.watchdogGroupBox.Controls.Add(this.numericUpDownDelay);
      this.watchdogGroupBox.Controls.Add(this.checkBoxAutoRestart);
      this.watchdogGroupBox.Controls.Add(this.checkBoxEnableWatchdog);
      this.watchdogGroupBox.Controls.Add(this.label6);
      this.watchdogGroupBox.Controls.Add(this.label5);
      this.watchdogGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.watchdogGroupBox.Location = new System.Drawing.Point(3, 59);
      this.watchdogGroupBox.Name = "watchdogGroupBox";
      this.watchdogGroupBox.Size = new System.Drawing.Size(466, 145);
      this.watchdogGroupBox.TabIndex = 2;
      this.watchdogGroupBox.TabStop = false;
      this.watchdogGroupBox.Text = "Watchdog settings";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(75, 115);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(216, 13);
      this.label1.TabIndex = 18;
      this.label1.Text = "Delay in seconds after which MP is restarted";
      // 
      // numericUpDownDelay
      // 
      this.numericUpDownDelay.Location = new System.Drawing.Point(26, 111);
      this.numericUpDownDelay.Name = "numericUpDownDelay";
      this.numericUpDownDelay.Size = new System.Drawing.Size(42, 20);
      this.numericUpDownDelay.TabIndex = 17;
      this.numericUpDownDelay.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // checkBoxAutoRestart
      // 
      this.checkBoxAutoRestart.AutoSize = true;
      this.checkBoxAutoRestart.Checked = true;
      this.checkBoxAutoRestart.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAutoRestart.Location = new System.Drawing.Point(10, 87);
      this.checkBoxAutoRestart.Name = "checkBoxAutoRestart";
      this.checkBoxAutoRestart.Size = new System.Drawing.Size(142, 17);
      this.checkBoxAutoRestart.TabIndex = 16;
      this.checkBoxAutoRestart.Text = "Automatically restart MP ";
      this.checkBoxAutoRestart.UseVisualStyleBackColor = true;
      // 
      // checkBoxEnableWatchdog
      // 
      this.checkBoxEnableWatchdog.AutoSize = true;
      this.checkBoxEnableWatchdog.Location = new System.Drawing.Point(10, 64);
      this.checkBoxEnableWatchdog.Name = "checkBoxEnableWatchdog";
      this.checkBoxEnableWatchdog.Size = new System.Drawing.Size(200, 17);
      this.checkBoxEnableWatchdog.TabIndex = 15;
      this.checkBoxEnableWatchdog.Text = "Watchdog enabled - monitor crashes";
      this.checkBoxEnableWatchdog.UseVisualStyleBackColor = true;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(7, 39);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(216, 13);
      this.label6.TabIndex = 14;
      this.label6.Text = "desktop and restart MediaPortal if it crashes.";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(7, 24);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(424, 13);
      this.label5.TabIndex = 13;
      this.label5.Text = "The watchdog monitors MP and can automatically gather the logfiles, make a zip on" +
          " your";
      // 
      // General
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.watchdogGroupBox);
      this.Controls.Add(this.groupBoxGeneralSettings);
      this.Name = "General";
      this.Size = new System.Drawing.Size(472, 402);
      this.groupBoxGeneralSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.PerformLayout();
      this.watchdogGroupBox.ResumeLayout(false);
      this.watchdogGroupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPComboBox mpThreadPriority;
    private System.Windows.Forms.Label lbDebug;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGeneralSettings;
    private System.Windows.Forms.Label labelPriority;
    private MediaPortal.UserInterface.Controls.MPComboBox cbDebug;
    private MediaPortal.UserInterface.Controls.MPGroupBox watchdogGroupBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown numericUpDownDelay;
    private System.Windows.Forms.CheckBox checkBoxAutoRestart;
    private System.Windows.Forms.CheckBox checkBoxEnableWatchdog;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label5;
  }
}
