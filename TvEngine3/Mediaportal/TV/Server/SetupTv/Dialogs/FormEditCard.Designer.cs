using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEditCard
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
      this.label3 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.label4 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpButtonSave = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.numericUpDownDecryptLimit = new System.Windows.Forms.NumericUpDown();
      this.mpButtonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.groupBoxConditionalAccessSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.mpLabel3 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpComboBoxMultiChannelDecryptMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.label5 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpComboBoxCamType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.checkBoxConditionalAccessEnabled = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxAllowEpgGrab = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxPreloadCard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.groupBoxAdvancedSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.checkBoxUseCustomTuning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.mpLabel6 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpLabel5 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxNetworkProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.mpComboBoxPidFilterMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.mpLabel1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpComboBoxIdleMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.mpLabel4 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpTextBoxTunerName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.mpLabel2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxGeneralSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.groupBoxDiseqcSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.mpLabel7 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownDiseqcCommandRepeatCount = new System.Windows.Forms.NumericUpDown();
      this.mpLabel8 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxAlwaysSendDiseqcCommands = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDecryptLimit)).BeginInit();
      this.groupBoxConditionalAccessSettings.SuspendLayout();
      this.groupBoxAdvancedSettings.SuspendLayout();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.groupBoxDiseqcSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiseqcCommandRepeatCount)).BeginInit();
      this.SuspendLayout();
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(14, 43);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(113, 13);
      this.label3.TabIndex = 8;
      this.label3.Text = "This tuner can decrypt";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(169, 43);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(130, 13);
      this.label4.TabIndex = 10;
      this.label4.Text = "channel(s) simultaneously.";
      // 
      // mpButtonSave
      // 
      this.mpButtonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.mpButtonSave.Location = new System.Drawing.Point(249, 493);
      this.mpButtonSave.Name = "mpButtonSave";
      this.mpButtonSave.Size = new System.Drawing.Size(75, 23);
      this.mpButtonSave.TabIndex = 30;
      this.mpButtonSave.Text = "Save";
      this.mpButtonSave.UseVisualStyleBackColor = true;
      this.mpButtonSave.Click += new System.EventHandler(this.mpButtonSave_Click);
      // 
      // numericUpDownDecryptLimit
      // 
      this.numericUpDownDecryptLimit.Location = new System.Drawing.Point(130, 41);
      this.numericUpDownDecryptLimit.Name = "numericUpDownDecryptLimit";
      this.numericUpDownDecryptLimit.Size = new System.Drawing.Size(38, 20);
      this.numericUpDownDecryptLimit.TabIndex = 9;
      this.numericUpDownDecryptLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownDecryptLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(330, 493);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 31;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.mpButtonCancel_Click);
      // 
      // groupBoxConditionalAccessSettings
      // 
      this.groupBoxConditionalAccessSettings.Controls.Add(this.mpLabel3);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.mpComboBoxMultiChannelDecryptMode);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.label5);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.mpComboBoxCamType);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.checkBoxConditionalAccessEnabled);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.label3);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.label4);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.numericUpDownDecryptLimit);
      this.groupBoxConditionalAccessSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxConditionalAccessSettings.Location = new System.Drawing.Point(22, 90);
      this.groupBoxConditionalAccessSettings.Name = "groupBoxConditionalAccessSettings";
      this.groupBoxConditionalAccessSettings.Size = new System.Drawing.Size(383, 133);
      this.groupBoxConditionalAccessSettings.TabIndex = 5;
      this.groupBoxConditionalAccessSettings.TabStop = false;
      this.groupBoxConditionalAccessSettings.Text = "Conditional Access";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(15, 70);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(140, 13);
      this.mpLabel3.TabIndex = 11;
      this.mpLabel3.Text = "Multi-channel decrypt mode:";
      // 
      // mpComboBoxMultiChannelDecryptMode
      // 
      this.mpComboBoxMultiChannelDecryptMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxMultiChannelDecryptMode.FormattingEnabled = true;
      this.mpComboBoxMultiChannelDecryptMode.Items.AddRange(new object[] {
            "Disabled",
            "List",
            "Changes"});
      this.mpComboBoxMultiChannelDecryptMode.Location = new System.Drawing.Point(159, 68);
      this.mpComboBoxMultiChannelDecryptMode.Name = "mpComboBoxMultiChannelDecryptMode";
      this.mpComboBoxMultiChannelDecryptMode.Size = new System.Drawing.Size(103, 21);
      this.mpComboBoxMultiChannelDecryptMode.TabIndex = 12;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(15, 98);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(64, 13);
      this.label5.TabIndex = 13;
      this.label5.Text = "CAM model:";
      // 
      // mpComboBoxCamType
      // 
      this.mpComboBoxCamType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCamType.FormattingEnabled = true;
      this.mpComboBoxCamType.Items.AddRange(new object[] {
            "Default",
            "Astoncrypt2"});
      this.mpComboBoxCamType.Location = new System.Drawing.Point(159, 95);
      this.mpComboBoxCamType.Name = "mpComboBoxCamType";
      this.mpComboBoxCamType.Size = new System.Drawing.Size(103, 21);
      this.mpComboBoxCamType.TabIndex = 14;
      // 
      // checkBoxConditionalAccessEnabled
      // 
      this.checkBoxConditionalAccessEnabled.AutoSize = true;
      this.checkBoxConditionalAccessEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxConditionalAccessEnabled.Location = new System.Drawing.Point(17, 19);
      this.checkBoxConditionalAccessEnabled.Name = "checkBoxConditionalAccessEnabled";
      this.checkBoxConditionalAccessEnabled.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxConditionalAccessEnabled.Size = new System.Drawing.Size(179, 17);
      this.checkBoxConditionalAccessEnabled.TabIndex = 6;
      this.checkBoxConditionalAccessEnabled.Text = "This tuner can decrypt channels.";
      this.checkBoxConditionalAccessEnabled.UseVisualStyleBackColor = true;
      this.checkBoxConditionalAccessEnabled.CheckedChanged += new System.EventHandler(this.checkBoxCAMenabled_CheckedChanged);
      // 
      // checkBoxAllowEpgGrab
      // 
      this.checkBoxAllowEpgGrab.AutoSize = true;
      this.checkBoxAllowEpgGrab.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAllowEpgGrab.Location = new System.Drawing.Point(17, 45);
      this.checkBoxAllowEpgGrab.Name = "checkBoxAllowEpgGrab";
      this.checkBoxAllowEpgGrab.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxAllowEpgGrab.Size = new System.Drawing.Size(212, 17);
      this.checkBoxAllowEpgGrab.TabIndex = 4;
      this.checkBoxAllowEpgGrab.Text = "Allow this tuner to be used to grab EPG.";
      this.checkBoxAllowEpgGrab.UseVisualStyleBackColor = true;
      // 
      // checkBoxPreloadCard
      // 
      this.checkBoxPreloadCard.AutoSize = true;
      this.checkBoxPreloadCard.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxPreloadCard.Location = new System.Drawing.Point(17, 121);
      this.checkBoxPreloadCard.Name = "checkBoxPreloadCard";
      this.checkBoxPreloadCard.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxPreloadCard.Size = new System.Drawing.Size(178, 17);
      this.checkBoxPreloadCard.TabIndex = 28;
      this.checkBoxPreloadCard.Text = "Allow this tuner to be pre-loaded.";
      this.checkBoxPreloadCard.UseVisualStyleBackColor = true;
      // 
      // groupBoxAdvancedSettings
      // 
      this.groupBoxAdvancedSettings.Controls.Add(this.checkBoxUseCustomTuning);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpLabel6);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpLabel5);
      this.groupBoxAdvancedSettings.Controls.Add(this.checkBoxPreloadCard);
      this.groupBoxAdvancedSettings.Controls.Add(this.comboBoxNetworkProvider);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpComboBoxPidFilterMode);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpLabel1);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpComboBoxIdleMode);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpLabel4);
      this.groupBoxAdvancedSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAdvancedSettings.Location = new System.Drawing.Point(22, 308);
      this.groupBoxAdvancedSettings.Name = "groupBoxAdvancedSettings";
      this.groupBoxAdvancedSettings.Size = new System.Drawing.Size(383, 174);
      this.groupBoxAdvancedSettings.TabIndex = 20;
      this.groupBoxAdvancedSettings.TabStop = false;
      this.groupBoxAdvancedSettings.Text = "Advanced";
      // 
      // checkBoxUseCustomTuning
      // 
      this.checkBoxUseCustomTuning.AutoSize = true;
      this.checkBoxUseCustomTuning.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseCustomTuning.Location = new System.Drawing.Point(17, 144);
      this.checkBoxUseCustomTuning.Name = "checkBoxUseCustomTuning";
      this.checkBoxUseCustomTuning.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxUseCustomTuning.Size = new System.Drawing.Size(199, 17);
      this.checkBoxUseCustomTuning.TabIndex = 29;
      this.checkBoxUseCustomTuning.Text = "Use direct/custom tuning if available.";
      this.checkBoxUseCustomTuning.UseVisualStyleBackColor = true;
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(14, 70);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(91, 13);
      this.mpLabel6.TabIndex = 24;
      this.mpLabel6.Text = "Network provider:";
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(14, 97);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(79, 13);
      this.mpLabel5.TabIndex = 26;
      this.mpLabel5.Text = "PID filter mode:";
      // 
      // comboBoxNetworkProvider
      // 
      this.comboBoxNetworkProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxNetworkProvider.FormattingEnabled = true;
      this.comboBoxNetworkProvider.Location = new System.Drawing.Point(158, 67);
      this.comboBoxNetworkProvider.Name = "comboBoxNetworkProvider";
      this.comboBoxNetworkProvider.Size = new System.Drawing.Size(103, 21);
      this.comboBoxNetworkProvider.TabIndex = 25;
      // 
      // mpComboBoxPidFilterMode
      // 
      this.mpComboBoxPidFilterMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxPidFilterMode.FormattingEnabled = true;
      this.mpComboBoxPidFilterMode.Items.AddRange(new object[] {
            "Disabled",
            "Enabled",
            "Auto"});
      this.mpComboBoxPidFilterMode.Location = new System.Drawing.Point(158, 94);
      this.mpComboBoxPidFilterMode.Name = "mpComboBoxPidFilterMode";
      this.mpComboBoxPidFilterMode.Size = new System.Drawing.Size(103, 21);
      this.mpComboBoxPidFilterMode.TabIndex = 27;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(14, 16);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(355, 13);
      this.mpLabel1.TabIndex = 21;
      this.mpLabel1.Text = "Please read the documentation in our wiki before changing these settings.";
      // 
      // mpComboBoxIdleMode
      // 
      this.mpComboBoxIdleMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxIdleMode.FormattingEnabled = true;
      this.mpComboBoxIdleMode.Items.AddRange(new object[] {
            "Pause",
            "Stop",
            "Unload",
            "AlwaysOn"});
      this.mpComboBoxIdleMode.Location = new System.Drawing.Point(158, 40);
      this.mpComboBoxIdleMode.Name = "mpComboBoxIdleMode";
      this.mpComboBoxIdleMode.Size = new System.Drawing.Size(103, 21);
      this.mpComboBoxIdleMode.TabIndex = 23;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(14, 43);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(56, 13);
      this.mpLabel4.TabIndex = 22;
      this.mpLabel4.Text = "Idle mode:";
      // 
      // mpTextBoxDeviceName
      // 
      this.mpTextBoxTunerName.Location = new System.Drawing.Point(96, 19);
      this.mpTextBoxTunerName.Name = "mpTextBoxDeviceName";
      this.mpTextBoxTunerName.Size = new System.Drawing.Size(272, 20);
      this.mpTextBoxTunerName.TabIndex = 3;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(15, 22);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(69, 13);
      this.mpLabel2.TabIndex = 2;
      this.mpLabel2.Text = "Tuner Name:";
      // 
      // groupBoxGeneralSettings
      // 
      this.groupBoxGeneralSettings.Controls.Add(this.mpLabel2);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxAllowEpgGrab);
      this.groupBoxGeneralSettings.Controls.Add(this.mpTextBoxTunerName);
      this.groupBoxGeneralSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(22, 12);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(383, 72);
      this.groupBoxGeneralSettings.TabIndex = 1;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "General";
      // 
      // groupBoxDiseqcSettings
      // 
      this.groupBoxDiseqcSettings.Controls.Add(this.mpLabel7);
      this.groupBoxDiseqcSettings.Controls.Add(this.numericUpDownDiseqcCommandRepeatCount);
      this.groupBoxDiseqcSettings.Controls.Add(this.mpLabel8);
      this.groupBoxDiseqcSettings.Controls.Add(this.checkBoxAlwaysSendDiseqcCommands);
      this.groupBoxDiseqcSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxDiseqcSettings.Location = new System.Drawing.Point(22, 231);
      this.groupBoxDiseqcSettings.Name = "groupBoxDiseqcSettings";
      this.groupBoxDiseqcSettings.Size = new System.Drawing.Size(383, 68);
      this.groupBoxDiseqcSettings.TabIndex = 15;
      this.groupBoxDiseqcSettings.TabStop = false;
      this.groupBoxDiseqcSettings.Text = "DiSEqC";
      // 
      // mpLabel7
      // 
      this.mpLabel7.AutoSize = true;
      this.mpLabel7.Location = new System.Drawing.Point(158, 44);
      this.mpLabel7.Name = "mpLabel7";
      this.mpLabel7.Size = new System.Drawing.Size(40, 13);
      this.mpLabel7.TabIndex = 19;
      this.mpLabel7.Text = "time(s).";
      // 
      // numericUpDownDiseqcCommandRepeatCount
      // 
      this.numericUpDownDiseqcCommandRepeatCount.Location = new System.Drawing.Point(117, 42);
      this.numericUpDownDiseqcCommandRepeatCount.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.numericUpDownDiseqcCommandRepeatCount.Name = "numericUpDownDiseqcCommandRepeatCount";
      this.numericUpDownDiseqcCommandRepeatCount.Size = new System.Drawing.Size(38, 20);
      this.numericUpDownDiseqcCommandRepeatCount.TabIndex = 18;
      this.numericUpDownDiseqcCommandRepeatCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // mpLabel8
      // 
      this.mpLabel8.AutoSize = true;
      this.mpLabel8.Location = new System.Drawing.Point(13, 44);
      this.mpLabel8.Name = "mpLabel8";
      this.mpLabel8.Size = new System.Drawing.Size(102, 13);
      this.mpLabel8.TabIndex = 17;
      this.mpLabel8.Text = "Repeat command(s)";
      // 
      // checkBoxAlwaysSendDiseqcCommands
      // 
      this.checkBoxAlwaysSendDiseqcCommands.AutoSize = true;
      this.checkBoxAlwaysSendDiseqcCommands.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAlwaysSendDiseqcCommands.Location = new System.Drawing.Point(16, 19);
      this.checkBoxAlwaysSendDiseqcCommands.Name = "checkBoxAlwaysSendDiseqcCommands";
      this.checkBoxAlwaysSendDiseqcCommands.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxAlwaysSendDiseqcCommands.Size = new System.Drawing.Size(140, 17);
      this.checkBoxAlwaysSendDiseqcCommands.TabIndex = 16;
      this.checkBoxAlwaysSendDiseqcCommands.Text = "Always send commands.";
      this.checkBoxAlwaysSendDiseqcCommands.UseVisualStyleBackColor = true;
      // 
      // FormEditCard
      // 
      this.AcceptButton = this.mpButtonSave;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(429, 528);
      this.Controls.Add(this.groupBoxDiseqcSettings);
      this.Controls.Add(this.groupBoxAdvancedSettings);
      this.Controls.Add(this.groupBoxGeneralSettings);
      this.Controls.Add(this.groupBoxConditionalAccessSettings);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonSave);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormEditCard";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit Tuner Settings";
      this.Load += new System.EventHandler(this.FormEditCard_Load);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDecryptLimit)).EndInit();
      this.groupBoxConditionalAccessSettings.ResumeLayout(false);
      this.groupBoxConditionalAccessSettings.PerformLayout();
      this.groupBoxAdvancedSettings.ResumeLayout(false);
      this.groupBoxAdvancedSettings.PerformLayout();
      this.groupBoxGeneralSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.PerformLayout();
      this.groupBoxDiseqcSettings.ResumeLayout(false);
      this.groupBoxDiseqcSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiseqcCommandRepeatCount)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MPLabel label3;
    private MPLabel label4;
    private MPButton mpButtonSave;
    private System.Windows.Forms.NumericUpDown numericUpDownDecryptLimit;
    private MPButton mpButtonCancel;
    private MPGroupBox groupBoxConditionalAccessSettings;
    private MPCheckBox checkBoxAllowEpgGrab;
    private MPCheckBox checkBoxPreloadCard;
    private MPGroupBox groupBoxAdvancedSettings;
    private MPCheckBox checkBoxConditionalAccessEnabled;
    private MPLabel label5;
    private MPComboBox mpComboBoxCamType;
    private MPComboBox comboBoxNetworkProvider;
    private MPLabel mpLabel3;
    private MPComboBox mpComboBoxMultiChannelDecryptMode;
    private MPTextBox mpTextBoxTunerName;
    private MPLabel mpLabel2;
    private MPLabel mpLabel6;
    private MPLabel mpLabel5;
    private MPComboBox mpComboBoxPidFilterMode;
    private MPLabel mpLabel1;
    private MPComboBox mpComboBoxIdleMode;
    private MPLabel mpLabel4;
    private MPGroupBox groupBoxGeneralSettings;
    private MPCheckBox checkBoxUseCustomTuning;
    private MPGroupBox groupBoxDiseqcSettings;
    private MPLabel mpLabel7;
    private System.Windows.Forms.NumericUpDown numericUpDownDiseqcCommandRepeatCount;
    private MPLabel mpLabel8;
    private MPCheckBox checkBoxAlwaysSendDiseqcCommands;
  }
}
