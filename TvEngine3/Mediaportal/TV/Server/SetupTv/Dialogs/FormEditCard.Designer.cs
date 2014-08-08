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
      this.labelDecryptLimit1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelDecryptLimit2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonSave = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.numericUpDownDecryptLimit = new System.Windows.Forms.NumericUpDown();
      this.buttonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.groupBoxConditionalAccessSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelMultiChannelDecryptMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxMultiChannelDecryptMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelCamType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxCamType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.checkBoxConditionalAccessEnabled = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxEpgGrabEnabled = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxPreloadTuner = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.groupBoxAdvancedSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.checkBoxUseCustomTuning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelNetworkProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelPidFilterMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxNetworkProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.comboBoxPidFilterMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelReadDocumentation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxIdleMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelIdleMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.textBoxTunerName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.labelTunerName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxGeneralSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.groupBoxDiseqcSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelDiseqcRepeatCount2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownDiseqcCommandRepeatCount = new System.Windows.Forms.NumericUpDown();
      this.labelDiseqcRepeatCount1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxAlwaysSendDiseqcCommands = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDecryptLimit)).BeginInit();
      this.groupBoxConditionalAccessSettings.SuspendLayout();
      this.groupBoxAdvancedSettings.SuspendLayout();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.groupBoxDiseqcSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiseqcCommandRepeatCount)).BeginInit();
      this.SuspendLayout();
      // 
      // labelDecryptLimit1
      // 
      this.labelDecryptLimit1.AutoSize = true;
      this.labelDecryptLimit1.Location = new System.Drawing.Point(13, 43);
      this.labelDecryptLimit1.Name = "labelDecryptLimit1";
      this.labelDecryptLimit1.Size = new System.Drawing.Size(113, 13);
      this.labelDecryptLimit1.TabIndex = 1;
      this.labelDecryptLimit1.Text = "This tuner can decrypt";
      // 
      // labelDecryptLimit2
      // 
      this.labelDecryptLimit2.AutoSize = true;
      this.labelDecryptLimit2.Location = new System.Drawing.Point(169, 43);
      this.labelDecryptLimit2.Name = "labelDecryptLimit2";
      this.labelDecryptLimit2.Size = new System.Drawing.Size(130, 13);
      this.labelDecryptLimit2.TabIndex = 3;
      this.labelDecryptLimit2.Text = "channel(s) simultaneously.";
      // 
      // buttonSave
      // 
      this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonSave.Location = new System.Drawing.Point(249, 493);
      this.buttonSave.Name = "buttonSave";
      this.buttonSave.Size = new System.Drawing.Size(75, 23);
      this.buttonSave.TabIndex = 4;
      this.buttonSave.Text = "Save";
      this.buttonSave.UseVisualStyleBackColor = true;
      this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
      // 
      // numericUpDownDecryptLimit
      // 
      this.numericUpDownDecryptLimit.Location = new System.Drawing.Point(130, 41);
      this.numericUpDownDecryptLimit.Name = "numericUpDownDecryptLimit";
      this.numericUpDownDecryptLimit.Size = new System.Drawing.Size(38, 20);
      this.numericUpDownDecryptLimit.TabIndex = 2;
      this.numericUpDownDecryptLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownDecryptLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(330, 493);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 5;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // groupBoxConditionalAccessSettings
      // 
      this.groupBoxConditionalAccessSettings.Controls.Add(this.labelMultiChannelDecryptMode);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.comboBoxMultiChannelDecryptMode);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.labelCamType);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.comboBoxCamType);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.checkBoxConditionalAccessEnabled);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.labelDecryptLimit1);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.labelDecryptLimit2);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.numericUpDownDecryptLimit);
      this.groupBoxConditionalAccessSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxConditionalAccessSettings.Location = new System.Drawing.Point(22, 113);
      this.groupBoxConditionalAccessSettings.Name = "groupBoxConditionalAccessSettings";
      this.groupBoxConditionalAccessSettings.Size = new System.Drawing.Size(383, 133);
      this.groupBoxConditionalAccessSettings.TabIndex = 1;
      this.groupBoxConditionalAccessSettings.TabStop = false;
      this.groupBoxConditionalAccessSettings.Text = "Conditional Access";
      // 
      // labelMultiChannelDecryptMode
      // 
      this.labelMultiChannelDecryptMode.AutoSize = true;
      this.labelMultiChannelDecryptMode.Location = new System.Drawing.Point(13, 71);
      this.labelMultiChannelDecryptMode.Name = "labelMultiChannelDecryptMode";
      this.labelMultiChannelDecryptMode.Size = new System.Drawing.Size(140, 13);
      this.labelMultiChannelDecryptMode.TabIndex = 4;
      this.labelMultiChannelDecryptMode.Text = "Multi-channel decrypt mode:";
      // 
      // comboBoxMultiChannelDecryptMode
      // 
      this.comboBoxMultiChannelDecryptMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxMultiChannelDecryptMode.FormattingEnabled = true;
      this.comboBoxMultiChannelDecryptMode.Location = new System.Drawing.Point(159, 68);
      this.comboBoxMultiChannelDecryptMode.Name = "comboBoxMultiChannelDecryptMode";
      this.comboBoxMultiChannelDecryptMode.Size = new System.Drawing.Size(103, 21);
      this.comboBoxMultiChannelDecryptMode.TabIndex = 5;
      // 
      // labelCamType
      // 
      this.labelCamType.AutoSize = true;
      this.labelCamType.Location = new System.Drawing.Point(13, 98);
      this.labelCamType.Name = "labelCamType";
      this.labelCamType.Size = new System.Drawing.Size(56, 13);
      this.labelCamType.TabIndex = 6;
      this.labelCamType.Text = "CAM type:";
      // 
      // comboBoxCamType
      // 
      this.comboBoxCamType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCamType.FormattingEnabled = true;
      this.comboBoxCamType.Location = new System.Drawing.Point(159, 95);
      this.comboBoxCamType.Name = "comboBoxCamType";
      this.comboBoxCamType.Size = new System.Drawing.Size(103, 21);
      this.comboBoxCamType.TabIndex = 7;
      // 
      // checkBoxConditionalAccessEnabled
      // 
      this.checkBoxConditionalAccessEnabled.AutoSize = true;
      this.checkBoxConditionalAccessEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxConditionalAccessEnabled.Location = new System.Drawing.Point(16, 19);
      this.checkBoxConditionalAccessEnabled.Name = "checkBoxConditionalAccessEnabled";
      this.checkBoxConditionalAccessEnabled.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxConditionalAccessEnabled.Size = new System.Drawing.Size(179, 17);
      this.checkBoxConditionalAccessEnabled.TabIndex = 0;
      this.checkBoxConditionalAccessEnabled.Text = "This tuner can decrypt channels.";
      this.checkBoxConditionalAccessEnabled.UseVisualStyleBackColor = true;
      this.checkBoxConditionalAccessEnabled.CheckedChanged += new System.EventHandler(this.checkBoxConditionalAccessEnabled_CheckedChanged);
      // 
      // checkBoxEpgGrabEnabled
      // 
      this.checkBoxEpgGrabEnabled.AutoSize = true;
      this.checkBoxEpgGrabEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEpgGrabEnabled.Location = new System.Drawing.Point(16, 45);
      this.checkBoxEpgGrabEnabled.Name = "checkBoxEpgGrabEnabled";
      this.checkBoxEpgGrabEnabled.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxEpgGrabEnabled.Size = new System.Drawing.Size(271, 17);
      this.checkBoxEpgGrabEnabled.TabIndex = 2;
      this.checkBoxEpgGrabEnabled.Text = "Use this tuner to grab electronic program guide data.";
      this.checkBoxEpgGrabEnabled.UseVisualStyleBackColor = true;
      // 
      // checkBoxPreloadTuner
      // 
      this.checkBoxPreloadTuner.AutoSize = true;
      this.checkBoxPreloadTuner.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxPreloadTuner.Location = new System.Drawing.Point(16, 68);
      this.checkBoxPreloadTuner.Name = "checkBoxPreloadTuner";
      this.checkBoxPreloadTuner.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxPreloadTuner.Size = new System.Drawing.Size(214, 17);
      this.checkBoxPreloadTuner.TabIndex = 3;
      this.checkBoxPreloadTuner.Text = "Load this tuner as soon as it is detected.";
      this.checkBoxPreloadTuner.UseVisualStyleBackColor = true;
      // 
      // groupBoxAdvancedSettings
      // 
      this.groupBoxAdvancedSettings.Controls.Add(this.checkBoxUseCustomTuning);
      this.groupBoxAdvancedSettings.Controls.Add(this.labelNetworkProvider);
      this.groupBoxAdvancedSettings.Controls.Add(this.labelPidFilterMode);
      this.groupBoxAdvancedSettings.Controls.Add(this.comboBoxNetworkProvider);
      this.groupBoxAdvancedSettings.Controls.Add(this.comboBoxPidFilterMode);
      this.groupBoxAdvancedSettings.Controls.Add(this.labelReadDocumentation);
      this.groupBoxAdvancedSettings.Controls.Add(this.comboBoxIdleMode);
      this.groupBoxAdvancedSettings.Controls.Add(this.labelIdleMode);
      this.groupBoxAdvancedSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAdvancedSettings.Location = new System.Drawing.Point(22, 331);
      this.groupBoxAdvancedSettings.Name = "groupBoxAdvancedSettings";
      this.groupBoxAdvancedSettings.Size = new System.Drawing.Size(383, 148);
      this.groupBoxAdvancedSettings.TabIndex = 3;
      this.groupBoxAdvancedSettings.TabStop = false;
      this.groupBoxAdvancedSettings.Text = "Advanced";
      // 
      // checkBoxUseCustomTuning
      // 
      this.checkBoxUseCustomTuning.AutoSize = true;
      this.checkBoxUseCustomTuning.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseCustomTuning.Location = new System.Drawing.Point(16, 121);
      this.checkBoxUseCustomTuning.Name = "checkBoxUseCustomTuning";
      this.checkBoxUseCustomTuning.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxUseCustomTuning.Size = new System.Drawing.Size(199, 17);
      this.checkBoxUseCustomTuning.TabIndex = 7;
      this.checkBoxUseCustomTuning.Text = "Use direct/custom tuning if available.";
      this.checkBoxUseCustomTuning.UseVisualStyleBackColor = true;
      // 
      // labelNetworkProvider
      // 
      this.labelNetworkProvider.AutoSize = true;
      this.labelNetworkProvider.Location = new System.Drawing.Point(13, 70);
      this.labelNetworkProvider.Name = "labelNetworkProvider";
      this.labelNetworkProvider.Size = new System.Drawing.Size(91, 13);
      this.labelNetworkProvider.TabIndex = 3;
      this.labelNetworkProvider.Text = "Network provider:";
      // 
      // labelPidFilterMode
      // 
      this.labelPidFilterMode.AutoSize = true;
      this.labelPidFilterMode.Location = new System.Drawing.Point(13, 97);
      this.labelPidFilterMode.Name = "labelPidFilterMode";
      this.labelPidFilterMode.Size = new System.Drawing.Size(79, 13);
      this.labelPidFilterMode.TabIndex = 5;
      this.labelPidFilterMode.Text = "PID filter mode:";
      // 
      // comboBoxNetworkProvider
      // 
      this.comboBoxNetworkProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxNetworkProvider.FormattingEnabled = true;
      this.comboBoxNetworkProvider.Location = new System.Drawing.Point(158, 67);
      this.comboBoxNetworkProvider.Name = "comboBoxNetworkProvider";
      this.comboBoxNetworkProvider.Size = new System.Drawing.Size(103, 21);
      this.comboBoxNetworkProvider.TabIndex = 4;
      // 
      // comboBoxPidFilterMode
      // 
      this.comboBoxPidFilterMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPidFilterMode.FormattingEnabled = true;
      this.comboBoxPidFilterMode.Location = new System.Drawing.Point(158, 94);
      this.comboBoxPidFilterMode.Name = "comboBoxPidFilterMode";
      this.comboBoxPidFilterMode.Size = new System.Drawing.Size(103, 21);
      this.comboBoxPidFilterMode.TabIndex = 6;
      // 
      // labelReadDocumentation
      // 
      this.labelReadDocumentation.AutoSize = true;
      this.labelReadDocumentation.Location = new System.Drawing.Point(6, 16);
      this.labelReadDocumentation.Name = "labelReadDocumentation";
      this.labelReadDocumentation.Size = new System.Drawing.Size(355, 13);
      this.labelReadDocumentation.TabIndex = 0;
      this.labelReadDocumentation.Text = "Please read the documentation in our wiki before changing these settings.";
      // 
      // comboBoxIdleMode
      // 
      this.comboBoxIdleMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxIdleMode.FormattingEnabled = true;
      this.comboBoxIdleMode.Location = new System.Drawing.Point(158, 40);
      this.comboBoxIdleMode.Name = "comboBoxIdleMode";
      this.comboBoxIdleMode.Size = new System.Drawing.Size(103, 21);
      this.comboBoxIdleMode.TabIndex = 2;
      // 
      // labelIdleMode
      // 
      this.labelIdleMode.AutoSize = true;
      this.labelIdleMode.Location = new System.Drawing.Point(13, 43);
      this.labelIdleMode.Name = "labelIdleMode";
      this.labelIdleMode.Size = new System.Drawing.Size(56, 13);
      this.labelIdleMode.TabIndex = 1;
      this.labelIdleMode.Text = "Idle mode:";
      // 
      // textBoxTunerName
      // 
      this.textBoxTunerName.Location = new System.Drawing.Point(96, 19);
      this.textBoxTunerName.Name = "textBoxTunerName";
      this.textBoxTunerName.Size = new System.Drawing.Size(272, 20);
      this.textBoxTunerName.TabIndex = 1;
      // 
      // labelTunerName
      // 
      this.labelTunerName.AutoSize = true;
      this.labelTunerName.Location = new System.Drawing.Point(13, 22);
      this.labelTunerName.Name = "labelTunerName";
      this.labelTunerName.Size = new System.Drawing.Size(67, 13);
      this.labelTunerName.TabIndex = 0;
      this.labelTunerName.Text = "Tuner name:";
      // 
      // groupBoxGeneralSettings
      // 
      this.groupBoxGeneralSettings.Controls.Add(this.labelTunerName);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxEpgGrabEnabled);
      this.groupBoxGeneralSettings.Controls.Add(this.textBoxTunerName);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxPreloadTuner);
      this.groupBoxGeneralSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(22, 12);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(383, 95);
      this.groupBoxGeneralSettings.TabIndex = 0;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "General";
      // 
      // groupBoxDiseqcSettings
      // 
      this.groupBoxDiseqcSettings.Controls.Add(this.labelDiseqcRepeatCount2);
      this.groupBoxDiseqcSettings.Controls.Add(this.numericUpDownDiseqcCommandRepeatCount);
      this.groupBoxDiseqcSettings.Controls.Add(this.labelDiseqcRepeatCount1);
      this.groupBoxDiseqcSettings.Controls.Add(this.checkBoxAlwaysSendDiseqcCommands);
      this.groupBoxDiseqcSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxDiseqcSettings.Location = new System.Drawing.Point(22, 254);
      this.groupBoxDiseqcSettings.Name = "groupBoxDiseqcSettings";
      this.groupBoxDiseqcSettings.Size = new System.Drawing.Size(383, 68);
      this.groupBoxDiseqcSettings.TabIndex = 2;
      this.groupBoxDiseqcSettings.TabStop = false;
      this.groupBoxDiseqcSettings.Text = "DiSEqC";
      // 
      // labelDiseqcRepeatCount2
      // 
      this.labelDiseqcRepeatCount2.AutoSize = true;
      this.labelDiseqcRepeatCount2.Location = new System.Drawing.Point(158, 44);
      this.labelDiseqcRepeatCount2.Name = "labelDiseqcRepeatCount2";
      this.labelDiseqcRepeatCount2.Size = new System.Drawing.Size(40, 13);
      this.labelDiseqcRepeatCount2.TabIndex = 3;
      this.labelDiseqcRepeatCount2.Text = "time(s).";
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
      this.numericUpDownDiseqcCommandRepeatCount.TabIndex = 2;
      this.numericUpDownDiseqcCommandRepeatCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // labelDiseqcRepeatCount1
      // 
      this.labelDiseqcRepeatCount1.AutoSize = true;
      this.labelDiseqcRepeatCount1.Location = new System.Drawing.Point(13, 44);
      this.labelDiseqcRepeatCount1.Name = "labelDiseqcRepeatCount1";
      this.labelDiseqcRepeatCount1.Size = new System.Drawing.Size(102, 13);
      this.labelDiseqcRepeatCount1.TabIndex = 1;
      this.labelDiseqcRepeatCount1.Text = "Repeat command(s)";
      // 
      // checkBoxAlwaysSendDiseqcCommands
      // 
      this.checkBoxAlwaysSendDiseqcCommands.AutoSize = true;
      this.checkBoxAlwaysSendDiseqcCommands.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAlwaysSendDiseqcCommands.Location = new System.Drawing.Point(16, 19);
      this.checkBoxAlwaysSendDiseqcCommands.Name = "checkBoxAlwaysSendDiseqcCommands";
      this.checkBoxAlwaysSendDiseqcCommands.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxAlwaysSendDiseqcCommands.Size = new System.Drawing.Size(218, 17);
      this.checkBoxAlwaysSendDiseqcCommands.TabIndex = 0;
      this.checkBoxAlwaysSendDiseqcCommands.Text = "Send command(s) on every tune attempt.";
      this.checkBoxAlwaysSendDiseqcCommands.UseVisualStyleBackColor = true;
      // 
      // FormEditCard
      // 
      this.AcceptButton = this.buttonSave;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(429, 528);
      this.Controls.Add(this.groupBoxDiseqcSettings);
      this.Controls.Add(this.groupBoxAdvancedSettings);
      this.Controls.Add(this.groupBoxGeneralSettings);
      this.Controls.Add(this.groupBoxConditionalAccessSettings);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonSave);
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

    private MPLabel labelDecryptLimit1;
    private MPLabel labelDecryptLimit2;
    private MPButton buttonSave;
    private System.Windows.Forms.NumericUpDown numericUpDownDecryptLimit;
    private MPButton buttonCancel;
    private MPGroupBox groupBoxConditionalAccessSettings;
    private MPCheckBox checkBoxEpgGrabEnabled;
    private MPCheckBox checkBoxPreloadTuner;
    private MPGroupBox groupBoxAdvancedSettings;
    private MPCheckBox checkBoxConditionalAccessEnabled;
    private MPLabel labelCamType;
    private MPComboBox comboBoxCamType;
    private MPComboBox comboBoxNetworkProvider;
    private MPLabel labelMultiChannelDecryptMode;
    private MPComboBox comboBoxMultiChannelDecryptMode;
    private MPTextBox textBoxTunerName;
    private MPLabel labelTunerName;
    private MPLabel labelNetworkProvider;
    private MPLabel labelPidFilterMode;
    private MPComboBox comboBoxPidFilterMode;
    private MPLabel labelReadDocumentation;
    private MPComboBox comboBoxIdleMode;
    private MPLabel labelIdleMode;
    private MPGroupBox groupBoxGeneralSettings;
    private MPCheckBox checkBoxUseCustomTuning;
    private MPGroupBox groupBoxDiseqcSettings;
    private MPLabel labelDiseqcRepeatCount2;
    private System.Windows.Forms.NumericUpDown numericUpDownDiseqcCommandRepeatCount;
    private MPLabel labelDiseqcRepeatCount1;
    private MPCheckBox checkBoxAlwaysSendDiseqcCommands;
  }
}
