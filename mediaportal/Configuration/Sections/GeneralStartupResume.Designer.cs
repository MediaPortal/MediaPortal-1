namespace MediaPortal.Configuration.Sections
{
  partial class GeneralStartupResume
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
      this.groupBoxStartupResumeSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBoxDelays = new System.Windows.Forms.GroupBox();
      this.mpCheckBoxMpResume = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxMpStartup = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.nudDelay = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbWaitForTvService = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.lbScreen = new System.Windows.Forms.Label();
      this.cbScreen = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.settingsCheckedListBox = new System.Windows.Forms.CheckedListBox();
      this.groupBoxStartupResumeSettings.SuspendLayout();
      this.groupBoxDelays.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxStartupResumeSettings
      // 
      this.groupBoxStartupResumeSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxStartupResumeSettings.Controls.Add(this.groupBoxDelays);
      this.groupBoxStartupResumeSettings.Controls.Add(this.groupBox1);
      this.groupBoxStartupResumeSettings.Controls.Add(this.lbScreen);
      this.groupBoxStartupResumeSettings.Controls.Add(this.cbScreen);
      this.groupBoxStartupResumeSettings.Controls.Add(this.settingsCheckedListBox);
      this.groupBoxStartupResumeSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxStartupResumeSettings.Location = new System.Drawing.Point(6, 0);
      this.groupBoxStartupResumeSettings.Name = "groupBoxStartupResumeSettings";
      this.groupBoxStartupResumeSettings.Size = new System.Drawing.Size(462, 396);
      this.groupBoxStartupResumeSettings.TabIndex = 1;
      this.groupBoxStartupResumeSettings.TabStop = false;
      this.groupBoxStartupResumeSettings.Text = "Startup/resume settings";
      // 
      // groupBoxDelays
      // 
      this.groupBoxDelays.Controls.Add(this.mpCheckBoxMpResume);
      this.groupBoxDelays.Controls.Add(this.mpCheckBoxMpStartup);
      this.groupBoxDelays.Controls.Add(this.label2);
      this.groupBoxDelays.Controls.Add(this.nudDelay);
      this.groupBoxDelays.Controls.Add(this.label1);
      this.groupBoxDelays.Location = new System.Drawing.Point(11, 280);
      this.groupBoxDelays.Name = "groupBoxDelays";
      this.groupBoxDelays.Size = new System.Drawing.Size(445, 61);
      this.groupBoxDelays.TabIndex = 11;
      this.groupBoxDelays.TabStop = false;
      this.groupBoxDelays.Text = "Delay settings";
      // 
      // mpCheckBoxMpResume
      // 
      this.mpCheckBoxMpResume.AutoSize = true;
      this.mpCheckBoxMpResume.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxMpResume.Location = new System.Drawing.Point(261, 35);
      this.mpCheckBoxMpResume.Name = "mpCheckBoxMpResume";
      this.mpCheckBoxMpResume.Size = new System.Drawing.Size(134, 17);
      this.mpCheckBoxMpResume.TabIndex = 9;
      this.mpCheckBoxMpResume.Text = "On MediaPortal resume";
      this.mpCheckBoxMpResume.UseVisualStyleBackColor = true;
      // 
      // mpCheckBoxMpStartup
      // 
      this.mpCheckBoxMpStartup.AutoSize = true;
      this.mpCheckBoxMpStartup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxMpStartup.Location = new System.Drawing.Point(261, 17);
      this.mpCheckBoxMpStartup.Name = "mpCheckBoxMpStartup";
      this.mpCheckBoxMpStartup.Size = new System.Drawing.Size(132, 17);
      this.mpCheckBoxMpStartup.TabIndex = 8;
      this.mpCheckBoxMpStartup.Text = "On MediaPortal startup";
      this.mpCheckBoxMpStartup.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(158, 19);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(53, 13);
      this.label2.TabIndex = 6;
      this.label2.Text = "second(s)";
      // 
      // nudDelay
      // 
      this.nudDelay.Location = new System.Drawing.Point(96, 17);
      this.nudDelay.Name = "nudDelay";
      this.nudDelay.Size = new System.Drawing.Size(56, 20);
      this.nudDelay.TabIndex = 7;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(7, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(87, 13);
      this.label1.TabIndex = 5;
      this.label1.Text = "Delay startup for ";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbWaitForTvService);
      this.groupBox1.Location = new System.Drawing.Point(11, 230);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(445, 44);
      this.groupBox1.TabIndex = 10;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Wait settings (autodetected)";
      // 
      // cbWaitForTvService
      // 
      this.cbWaitForTvService.AutoSize = true;
      this.cbWaitForTvService.Enabled = false;
      this.cbWaitForTvService.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbWaitForTvService.Location = new System.Drawing.Point(11, 19);
      this.cbWaitForTvService.Name = "cbWaitForTvService";
      this.cbWaitForTvService.Size = new System.Drawing.Size(174, 17);
      this.cbWaitForTvService.TabIndex = 4;
      this.cbWaitForTvService.Text = "Wait until TV Server has started";
      this.cbWaitForTvService.UseVisualStyleBackColor = true;
      // 
      // lbScreen
      // 
      this.lbScreen.AutoSize = true;
      this.lbScreen.Location = new System.Drawing.Point(6, 197);
      this.lbScreen.Name = "lbScreen";
      this.lbScreen.Size = new System.Drawing.Size(67, 13);
      this.lbScreen.TabIndex = 5;
      this.lbScreen.Text = "Start screen:";
      this.lbScreen.Click += new System.EventHandler(this.lbScreen_Click);
      // 
      // cbScreen
      // 
      this.cbScreen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbScreen.BorderColor = System.Drawing.Color.Empty;
      this.cbScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbScreen.FormattingEnabled = true;
      this.cbScreen.Location = new System.Drawing.Point(85, 194);
      this.cbScreen.MinimumSize = new System.Drawing.Size(100, 0);
      this.cbScreen.Name = "cbScreen";
      this.cbScreen.Size = new System.Drawing.Size(371, 21);
      this.cbScreen.TabIndex = 6;
      this.cbScreen.SelectedIndexChanged += new System.EventHandler(this.cbScreen_SelectedIndexChanged);
      // 
      // settingsCheckedListBox
      // 
      this.settingsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.settingsCheckedListBox.CheckOnClick = true;
      this.settingsCheckedListBox.Items.AddRange(new object[] {
            "Start MediaPortal in fullscreen mode",
            "Keep MediaPortal fullscreen mode (don\'t rely on windows resolution change)",
            "Use alternative fullscreen Splashscreen (fullscreen only)",
            "Keep MediaPortal always on top",
            "Hide taskbar in fullscreen mode",
            "Autostart MediaPortal on Windows startup",
            "Minimize to tray on start up",
            "Minimize to tray on GUI exit",
            "Minimize to tray on focus loss (fullscreen only)",
            "Turn off monitor when blanking screen",
            "Show last active module when starting / resuming from standby",
            "Stop playback on removal of an audio renderer"});
      this.settingsCheckedListBox.Location = new System.Drawing.Point(6, 19);
      this.settingsCheckedListBox.Name = "settingsCheckedListBox";
      this.settingsCheckedListBox.Size = new System.Drawing.Size(450, 169);
      this.settingsCheckedListBox.TabIndex = 0;
      this.settingsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.settingsCheckedListBox_ItemCheck);
      this.settingsCheckedListBox.SelectedIndexChanged += new System.EventHandler(this.settingsCheckedListBox_SelectedIndexChanged);
      // 
      // GeneralStartupResume
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBoxStartupResumeSettings);
      this.Name = "GeneralStartupResume";
      this.Size = new System.Drawing.Size(472, 402);
      this.groupBoxStartupResumeSettings.ResumeLayout(false);
      this.groupBoxStartupResumeSettings.PerformLayout();
      this.groupBoxDelays.ResumeLayout(false);
      this.groupBoxDelays.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxStartupResumeSettings;
    private System.Windows.Forms.CheckedListBox settingsCheckedListBox;
    private MediaPortal.UserInterface.Controls.MPComboBox cbScreen;
    private System.Windows.Forms.Label lbScreen;
    private System.Windows.Forms.GroupBox groupBoxDelays;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxMpResume;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxMpStartup;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.NumericUpDown nudDelay;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbWaitForTvService;
  }
}
