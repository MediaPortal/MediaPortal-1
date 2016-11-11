using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEditTuner
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
      this.buttonOkay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.tabControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabControl();
      this.tabPageGeneral = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBoxDebug = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelDebugDoNotEnable = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxTsMuxerDumpInputs = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxTsWriterDisableCrcCheck = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxTsWriterDumpInputs = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.groupBoxAdvanced = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.checkBoxUseCustomTuning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelBdaNetworkProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelPidFilterMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxBdaNetworkProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.comboBoxPidFilterMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelAdvancedReadDocumentation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxIdleMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelIdleMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxGeneral = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.checkBoxAlwaysSendDiseqcCommands = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelTunerName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxUseForEpgGrabbing = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.textBoxTunerName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.checkBoxPreload = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.tabPageConditionalAccess = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBoxCaMenu = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.textBoxCaMenuAnswer = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.buttonCaMenuOkaySelect = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelCaMenuFooter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.listBoxCaMenuChoices = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListBox();
      this.buttonCaMenuBackClose = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelCaMenuEnquiry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonCaMenuOpen = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelCaMenuTitle = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelCaMenuSubTitle = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxConditionalAccess = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelConditionalAccessProviders = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.textBoxConditionalAccessProviders = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.labelMultiChannelDecryptMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxMultiChannelDecryptMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelCamType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxCamType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.checkBoxUseConditionalAccess = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelDecryptLimit1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelDecryptLimit2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownDecryptLimit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.tabPageAnalog = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBoxEncoderSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonEncoderSettingsCheckSupport = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelEncoderSettingsRecording = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownEncoderBitRateValuePeakRecording = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelEncoderBitRateValuePeakUnitRecording = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelEncoderBitRateValuePeakRecording = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownEncoderBitRateValueRecording = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelEncoderBitRateValueUnitRecording = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxEncoderBitRateModeRecording = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelEncoderBitRateModeRecording = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelEncoderBitRateValueRecording = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelEncoderSettingsTimeShifting = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownEncoderBitRateValuePeakTimeShifting = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelEncoderBitRateValuePeakUnitTimeShifting = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelEncoderBitRateValuePeakTimeShifting = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownEncoderBitRateValueTimeShifting = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelEncoderBitRateValueUnitTimeShifting = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxEncoderBitRateModeTimeShifting = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelEncoderBitRateModeTimeShifting = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelEncoderBitRateValueTimeShifting = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxVideo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelAnalogVideoStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxAnalogVideoStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.comboBoxFrameSize = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.comboBoxFrameRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelFrameRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrameSize = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxSoftwareEncoders = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.comboBoxSoftwareEncoderAudio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelSoftwareEncoderAudio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSoftwareEncoderVideo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxSoftwareEncoderVideo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.groupBoxVideoAndCameraProperties = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.checkBoxVideoOrCameraPropertyValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.buttonRestoreAllDefaults = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelVideoOrCameraPropertyValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelVideoOrCameraProperty = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxVideoOrCameraProperty = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.scrollBarVideoOrCameraPropertyValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPHScrollBar();
      this.labelVideoOrCameraPropertyValueDisplay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonRestoreDefault = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.tabPageExternalInput = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBoxExternalTuner = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.textBoxExternalTunerProgram = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.labelExternalTunerProgram = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelExternalTunerProgramArguments = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonExternalTunerProgramBrowse = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.textBoxExternalTunerProgramArguments = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.groupBoxExternalInput = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelExternalInputPhysicalChannelNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelExternalInputCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxExternalInputCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.comboBoxExternalInputSourceAudio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelExternalInputSourceAudio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelExternalInputSourceVideo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxExternalInputSourceVideo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.numericUpDownExternalInputPhysicalChannelNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.openFileDialogExternalTunerProgram = new System.Windows.Forms.OpenFileDialog();
      this.tabControl.SuspendLayout();
      this.tabPageGeneral.SuspendLayout();
      this.groupBoxDebug.SuspendLayout();
      this.groupBoxAdvanced.SuspendLayout();
      this.groupBoxGeneral.SuspendLayout();
      this.tabPageConditionalAccess.SuspendLayout();
      this.groupBoxCaMenu.SuspendLayout();
      this.groupBoxConditionalAccess.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDecryptLimit)).BeginInit();
      this.tabPageAnalog.SuspendLayout();
      this.groupBoxEncoderSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEncoderBitRateValuePeakRecording)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEncoderBitRateValueRecording)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEncoderBitRateValuePeakTimeShifting)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEncoderBitRateValueTimeShifting)).BeginInit();
      this.groupBoxVideo.SuspendLayout();
      this.groupBoxSoftwareEncoders.SuspendLayout();
      this.groupBoxVideoAndCameraProperties.SuspendLayout();
      this.tabPageExternalInput.SuspendLayout();
      this.groupBoxExternalTuner.SuspendLayout();
      this.groupBoxExternalInput.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExternalInputPhysicalChannelNumber)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonOkay
      // 
      this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOkay.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonOkay.Location = new System.Drawing.Point(162, 503);
      this.buttonOkay.Name = "buttonOkay";
      this.buttonOkay.Size = new System.Drawing.Size(75, 23);
      this.buttonOkay.TabIndex = 1;
      this.buttonOkay.Text = "&OK";
      this.buttonOkay.UseVisualStyleBackColor = true;
      this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(243, 503);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      // 
      // tabControl
      // 
      this.tabControl.AllowDrop = true;
      this.tabControl.AllowReorderTabs = false;
      this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl.Controls.Add(this.tabPageGeneral);
      this.tabControl.Controls.Add(this.tabPageConditionalAccess);
      this.tabControl.Controls.Add(this.tabPageAnalog);
      this.tabControl.Controls.Add(this.tabPageExternalInput);
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(331, 496);
      this.tabControl.TabIndex = 0;
      // 
      // tabPageGeneral
      // 
      this.tabPageGeneral.Controls.Add(this.groupBoxDebug);
      this.tabPageGeneral.Controls.Add(this.groupBoxAdvanced);
      this.tabPageGeneral.Controls.Add(this.groupBoxGeneral);
      this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabPageGeneral.Name = "tabPageGeneral";
      this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageGeneral.Size = new System.Drawing.Size(323, 470);
      this.tabPageGeneral.TabIndex = 0;
      this.tabPageGeneral.Text = "General";
      this.tabPageGeneral.UseVisualStyleBackColor = true;
      // 
      // groupBoxDebug
      // 
      this.groupBoxDebug.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxDebug.Controls.Add(this.labelDebugDoNotEnable);
      this.groupBoxDebug.Controls.Add(this.checkBoxTsMuxerDumpInputs);
      this.groupBoxDebug.Controls.Add(this.checkBoxTsWriterDisableCrcCheck);
      this.groupBoxDebug.Controls.Add(this.checkBoxTsWriterDumpInputs);
      this.groupBoxDebug.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxDebug.Location = new System.Drawing.Point(6, 282);
      this.groupBoxDebug.Name = "groupBoxDebug";
      this.groupBoxDebug.Size = new System.Drawing.Size(308, 114);
      this.groupBoxDebug.TabIndex = 2;
      this.groupBoxDebug.TabStop = false;
      this.groupBoxDebug.Text = "Debug";
      // 
      // labelDebugDoNotEnable
      // 
      this.labelDebugDoNotEnable.AutoSize = true;
      this.labelDebugDoNotEnable.Location = new System.Drawing.Point(6, 16);
      this.labelDebugDoNotEnable.Name = "labelDebugDoNotEnable";
      this.labelDebugDoNotEnable.Size = new System.Drawing.Size(291, 13);
      this.labelDebugDoNotEnable.TabIndex = 0;
      this.labelDebugDoNotEnable.Text = "Do not enable these settings unless suggested by an expert.";
      // 
      // checkBoxTsMuxerDumpInputs
      // 
      this.checkBoxTsMuxerDumpInputs.AutoSize = true;
      this.checkBoxTsMuxerDumpInputs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxTsMuxerDumpInputs.Location = new System.Drawing.Point(9, 85);
      this.checkBoxTsMuxerDumpInputs.Name = "checkBoxTsMuxerDumpInputs";
      this.checkBoxTsMuxerDumpInputs.Size = new System.Drawing.Size(130, 17);
      this.checkBoxTsMuxerDumpInputs.TabIndex = 3;
      this.checkBoxTsMuxerDumpInputs.Text = "Dump TsMuxer inputs.";
      this.checkBoxTsMuxerDumpInputs.UseVisualStyleBackColor = true;
      // 
      // checkBoxTsWriterDisableCrcCheck
      // 
      this.checkBoxTsWriterDisableCrcCheck.AutoSize = true;
      this.checkBoxTsWriterDisableCrcCheck.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxTsWriterDisableCrcCheck.Location = new System.Drawing.Point(9, 62);
      this.checkBoxTsWriterDisableCrcCheck.Name = "checkBoxTsWriterDisableCrcCheck";
      this.checkBoxTsWriterDisableCrcCheck.Size = new System.Drawing.Size(177, 17);
      this.checkBoxTsWriterDisableCrcCheck.TabIndex = 2;
      this.checkBoxTsWriterDisableCrcCheck.Text = "Disable TsWriter CRC checking.";
      this.checkBoxTsWriterDisableCrcCheck.UseVisualStyleBackColor = true;
      // 
      // checkBoxTsWriterDumpInputs
      // 
      this.checkBoxTsWriterDumpInputs.AutoSize = true;
      this.checkBoxTsWriterDumpInputs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxTsWriterDumpInputs.Location = new System.Drawing.Point(9, 39);
      this.checkBoxTsWriterDumpInputs.Name = "checkBoxTsWriterDumpInputs";
      this.checkBoxTsWriterDumpInputs.Size = new System.Drawing.Size(129, 17);
      this.checkBoxTsWriterDumpInputs.TabIndex = 1;
      this.checkBoxTsWriterDumpInputs.Text = "Dump TsWriter inputs.";
      this.checkBoxTsWriterDumpInputs.UseVisualStyleBackColor = true;
      // 
      // groupBoxAdvanced
      // 
      this.groupBoxAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAdvanced.Controls.Add(this.checkBoxUseCustomTuning);
      this.groupBoxAdvanced.Controls.Add(this.labelBdaNetworkProvider);
      this.groupBoxAdvanced.Controls.Add(this.labelPidFilterMode);
      this.groupBoxAdvanced.Controls.Add(this.comboBoxBdaNetworkProvider);
      this.groupBoxAdvanced.Controls.Add(this.comboBoxPidFilterMode);
      this.groupBoxAdvanced.Controls.Add(this.labelAdvancedReadDocumentation);
      this.groupBoxAdvanced.Controls.Add(this.comboBoxIdleMode);
      this.groupBoxAdvanced.Controls.Add(this.labelIdleMode);
      this.groupBoxAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAdvanced.Location = new System.Drawing.Point(6, 128);
      this.groupBoxAdvanced.Name = "groupBoxAdvanced";
      this.groupBoxAdvanced.Size = new System.Drawing.Size(308, 148);
      this.groupBoxAdvanced.TabIndex = 1;
      this.groupBoxAdvanced.TabStop = false;
      this.groupBoxAdvanced.Text = "Advanced";
      // 
      // checkBoxUseCustomTuning
      // 
      this.checkBoxUseCustomTuning.AutoSize = true;
      this.checkBoxUseCustomTuning.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseCustomTuning.Location = new System.Drawing.Point(9, 121);
      this.checkBoxUseCustomTuning.Name = "checkBoxUseCustomTuning";
      this.checkBoxUseCustomTuning.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxUseCustomTuning.Size = new System.Drawing.Size(199, 17);
      this.checkBoxUseCustomTuning.TabIndex = 7;
      this.checkBoxUseCustomTuning.Text = "Use direct/custom tuning if available.";
      this.checkBoxUseCustomTuning.UseVisualStyleBackColor = true;
      // 
      // labelBdaNetworkProvider
      // 
      this.labelBdaNetworkProvider.AutoSize = true;
      this.labelBdaNetworkProvider.Location = new System.Drawing.Point(6, 70);
      this.labelBdaNetworkProvider.Name = "labelBdaNetworkProvider";
      this.labelBdaNetworkProvider.Size = new System.Drawing.Size(114, 13);
      this.labelBdaNetworkProvider.TabIndex = 3;
      this.labelBdaNetworkProvider.Text = "BDA network provider:";
      // 
      // labelPidFilterMode
      // 
      this.labelPidFilterMode.AutoSize = true;
      this.labelPidFilterMode.Location = new System.Drawing.Point(6, 97);
      this.labelPidFilterMode.Name = "labelPidFilterMode";
      this.labelPidFilterMode.Size = new System.Drawing.Size(79, 13);
      this.labelPidFilterMode.TabIndex = 5;
      this.labelPidFilterMode.Text = "PID filter mode:";
      // 
      // comboBoxBdaNetworkProvider
      // 
      this.comboBoxBdaNetworkProvider.FormattingEnabled = true;
      this.comboBoxBdaNetworkProvider.Location = new System.Drawing.Point(126, 67);
      this.comboBoxBdaNetworkProvider.Name = "comboBoxBdaNetworkProvider";
      this.comboBoxBdaNetworkProvider.Size = new System.Drawing.Size(100, 21);
      this.comboBoxBdaNetworkProvider.TabIndex = 4;
      // 
      // comboBoxPidFilterMode
      // 
      this.comboBoxPidFilterMode.FormattingEnabled = true;
      this.comboBoxPidFilterMode.Location = new System.Drawing.Point(126, 94);
      this.comboBoxPidFilterMode.Name = "comboBoxPidFilterMode";
      this.comboBoxPidFilterMode.Size = new System.Drawing.Size(100, 21);
      this.comboBoxPidFilterMode.TabIndex = 6;
      // 
      // labelAdvancedReadDocumentation
      // 
      this.labelAdvancedReadDocumentation.AutoSize = true;
      this.labelAdvancedReadDocumentation.Location = new System.Drawing.Point(6, 16);
      this.labelAdvancedReadDocumentation.Name = "labelAdvancedReadDocumentation";
      this.labelAdvancedReadDocumentation.Size = new System.Drawing.Size(287, 13);
      this.labelAdvancedReadDocumentation.TabIndex = 0;
      this.labelAdvancedReadDocumentation.Text = "Please read documentation before changing these settings.";
      // 
      // comboBoxIdleMode
      // 
      this.comboBoxIdleMode.FormattingEnabled = true;
      this.comboBoxIdleMode.Location = new System.Drawing.Point(126, 40);
      this.comboBoxIdleMode.Name = "comboBoxIdleMode";
      this.comboBoxIdleMode.Size = new System.Drawing.Size(100, 21);
      this.comboBoxIdleMode.TabIndex = 2;
      // 
      // labelIdleMode
      // 
      this.labelIdleMode.AutoSize = true;
      this.labelIdleMode.Location = new System.Drawing.Point(6, 43);
      this.labelIdleMode.Name = "labelIdleMode";
      this.labelIdleMode.Size = new System.Drawing.Size(56, 13);
      this.labelIdleMode.TabIndex = 1;
      this.labelIdleMode.Text = "Idle mode:";
      // 
      // groupBoxGeneral
      // 
      this.groupBoxGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGeneral.Controls.Add(this.checkBoxAlwaysSendDiseqcCommands);
      this.groupBoxGeneral.Controls.Add(this.labelTunerName);
      this.groupBoxGeneral.Controls.Add(this.checkBoxUseForEpgGrabbing);
      this.groupBoxGeneral.Controls.Add(this.textBoxTunerName);
      this.groupBoxGeneral.Controls.Add(this.checkBoxPreload);
      this.groupBoxGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGeneral.Location = new System.Drawing.Point(6, 6);
      this.groupBoxGeneral.Name = "groupBoxGeneral";
      this.groupBoxGeneral.Size = new System.Drawing.Size(308, 116);
      this.groupBoxGeneral.TabIndex = 0;
      this.groupBoxGeneral.TabStop = false;
      this.groupBoxGeneral.Text = "General";
      // 
      // checkBoxAlwaysSendDiseqcCommands
      // 
      this.checkBoxAlwaysSendDiseqcCommands.AutoSize = true;
      this.checkBoxAlwaysSendDiseqcCommands.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAlwaysSendDiseqcCommands.Location = new System.Drawing.Point(9, 91);
      this.checkBoxAlwaysSendDiseqcCommands.Name = "checkBoxAlwaysSendDiseqcCommands";
      this.checkBoxAlwaysSendDiseqcCommands.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxAlwaysSendDiseqcCommands.Size = new System.Drawing.Size(258, 17);
      this.checkBoxAlwaysSendDiseqcCommands.TabIndex = 4;
      this.checkBoxAlwaysSendDiseqcCommands.Text = "Send DiSEqC command(s) on every tune attempt.";
      this.checkBoxAlwaysSendDiseqcCommands.UseVisualStyleBackColor = true;
      // 
      // labelTunerName
      // 
      this.labelTunerName.AutoSize = true;
      this.labelTunerName.Location = new System.Drawing.Point(6, 22);
      this.labelTunerName.Name = "labelTunerName";
      this.labelTunerName.Size = new System.Drawing.Size(38, 13);
      this.labelTunerName.TabIndex = 0;
      this.labelTunerName.Text = "Name:";
      // 
      // checkBoxUseForEpgGrabbing
      // 
      this.checkBoxUseForEpgGrabbing.AutoSize = true;
      this.checkBoxUseForEpgGrabbing.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseForEpgGrabbing.Location = new System.Drawing.Point(9, 45);
      this.checkBoxUseForEpgGrabbing.Name = "checkBoxUseForEpgGrabbing";
      this.checkBoxUseForEpgGrabbing.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxUseForEpgGrabbing.Size = new System.Drawing.Size(285, 17);
      this.checkBoxUseForEpgGrabbing.TabIndex = 2;
      this.checkBoxUseForEpgGrabbing.Text = "Use this tuner to grab electronic programme guide data.";
      this.checkBoxUseForEpgGrabbing.UseVisualStyleBackColor = true;
      // 
      // textBoxTunerName
      // 
      this.textBoxTunerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxTunerName.Location = new System.Drawing.Point(50, 19);
      this.textBoxTunerName.Name = "textBoxTunerName";
      this.textBoxTunerName.Size = new System.Drawing.Size(249, 20);
      this.textBoxTunerName.TabIndex = 1;
      // 
      // checkBoxPreload
      // 
      this.checkBoxPreload.AutoSize = true;
      this.checkBoxPreload.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxPreload.Location = new System.Drawing.Point(9, 68);
      this.checkBoxPreload.Name = "checkBoxPreload";
      this.checkBoxPreload.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxPreload.Size = new System.Drawing.Size(214, 17);
      this.checkBoxPreload.TabIndex = 3;
      this.checkBoxPreload.Text = "Load this tuner as soon as it is detected.";
      this.checkBoxPreload.UseVisualStyleBackColor = true;
      // 
      // tabPageConditionalAccess
      // 
      this.tabPageConditionalAccess.Controls.Add(this.groupBoxCaMenu);
      this.tabPageConditionalAccess.Controls.Add(this.groupBoxConditionalAccess);
      this.tabPageConditionalAccess.Location = new System.Drawing.Point(4, 22);
      this.tabPageConditionalAccess.Name = "tabPageConditionalAccess";
      this.tabPageConditionalAccess.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageConditionalAccess.Size = new System.Drawing.Size(323, 470);
      this.tabPageConditionalAccess.TabIndex = 1;
      this.tabPageConditionalAccess.Text = "Conditional Access";
      this.tabPageConditionalAccess.UseVisualStyleBackColor = true;
      // 
      // groupBoxCaMenu
      // 
      this.groupBoxCaMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxCaMenu.Controls.Add(this.textBoxCaMenuAnswer);
      this.groupBoxCaMenu.Controls.Add(this.buttonCaMenuOkaySelect);
      this.groupBoxCaMenu.Controls.Add(this.labelCaMenuFooter);
      this.groupBoxCaMenu.Controls.Add(this.listBoxCaMenuChoices);
      this.groupBoxCaMenu.Controls.Add(this.buttonCaMenuBackClose);
      this.groupBoxCaMenu.Controls.Add(this.labelCaMenuEnquiry);
      this.groupBoxCaMenu.Controls.Add(this.buttonCaMenuOpen);
      this.groupBoxCaMenu.Controls.Add(this.labelCaMenuTitle);
      this.groupBoxCaMenu.Controls.Add(this.labelCaMenuSubTitle);
      this.groupBoxCaMenu.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCaMenu.Location = new System.Drawing.Point(6, 166);
      this.groupBoxCaMenu.Name = "groupBoxCaMenu";
      this.groupBoxCaMenu.Size = new System.Drawing.Size(311, 276);
      this.groupBoxCaMenu.TabIndex = 1;
      this.groupBoxCaMenu.TabStop = false;
      this.groupBoxCaMenu.Text = "Menu";
      // 
      // textBoxCaMenuAnswer
      // 
      this.textBoxCaMenuAnswer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxCaMenuAnswer.Location = new System.Drawing.Point(9, 214);
      this.textBoxCaMenuAnswer.Name = "textBoxCaMenuAnswer";
      this.textBoxCaMenuAnswer.Size = new System.Drawing.Size(296, 20);
      this.textBoxCaMenuAnswer.TabIndex = 5;
      // 
      // buttonCaMenuOkaySelect
      // 
      this.buttonCaMenuOkaySelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonCaMenuOkaySelect.Enabled = false;
      this.buttonCaMenuOkaySelect.Location = new System.Drawing.Point(90, 243);
      this.buttonCaMenuOkaySelect.Name = "buttonCaMenuOkaySelect";
      this.buttonCaMenuOkaySelect.Size = new System.Drawing.Size(75, 23);
      this.buttonCaMenuOkaySelect.TabIndex = 7;
      this.buttonCaMenuOkaySelect.Text = "OK/&Select";
      this.buttonCaMenuOkaySelect.UseVisualStyleBackColor = true;
      this.buttonCaMenuOkaySelect.Click += new System.EventHandler(this.buttonCaMenuOkaySelect_Click);
      // 
      // labelCaMenuFooter
      // 
      this.labelCaMenuFooter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelCaMenuFooter.AutoSize = true;
      this.labelCaMenuFooter.Location = new System.Drawing.Point(6, 178);
      this.labelCaMenuFooter.Name = "labelCaMenuFooter";
      this.labelCaMenuFooter.Size = new System.Drawing.Size(37, 13);
      this.labelCaMenuFooter.TabIndex = 3;
      this.labelCaMenuFooter.Text = "Footer";
      // 
      // listBoxCaMenuChoices
      // 
      this.listBoxCaMenuChoices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxCaMenuChoices.FormattingEnabled = true;
      this.listBoxCaMenuChoices.Location = new System.Drawing.Point(9, 54);
      this.listBoxCaMenuChoices.Name = "listBoxCaMenuChoices";
      this.listBoxCaMenuChoices.Size = new System.Drawing.Size(296, 121);
      this.listBoxCaMenuChoices.TabIndex = 2;
      // 
      // buttonCaMenuBackClose
      // 
      this.buttonCaMenuBackClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonCaMenuBackClose.Enabled = false;
      this.buttonCaMenuBackClose.Location = new System.Drawing.Point(171, 243);
      this.buttonCaMenuBackClose.Name = "buttonCaMenuBackClose";
      this.buttonCaMenuBackClose.Size = new System.Drawing.Size(75, 23);
      this.buttonCaMenuBackClose.TabIndex = 8;
      this.buttonCaMenuBackClose.Text = "&Back/Close";
      this.buttonCaMenuBackClose.UseVisualStyleBackColor = true;
      this.buttonCaMenuBackClose.Click += new System.EventHandler(this.buttonCaMenuBackClose_Click);
      // 
      // labelCaMenuEnquiry
      // 
      this.labelCaMenuEnquiry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelCaMenuEnquiry.AutoSize = true;
      this.labelCaMenuEnquiry.Location = new System.Drawing.Point(6, 198);
      this.labelCaMenuEnquiry.Name = "labelCaMenuEnquiry";
      this.labelCaMenuEnquiry.Size = new System.Drawing.Size(42, 13);
      this.labelCaMenuEnquiry.TabIndex = 4;
      this.labelCaMenuEnquiry.Text = "Enquiry";
      // 
      // buttonCaMenuOpen
      // 
      this.buttonCaMenuOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonCaMenuOpen.Location = new System.Drawing.Point(9, 243);
      this.buttonCaMenuOpen.Name = "buttonCaMenuOpen";
      this.buttonCaMenuOpen.Size = new System.Drawing.Size(75, 23);
      this.buttonCaMenuOpen.TabIndex = 6;
      this.buttonCaMenuOpen.Text = "&Open";
      this.buttonCaMenuOpen.UseVisualStyleBackColor = true;
      this.buttonCaMenuOpen.Click += new System.EventHandler(this.buttonCaMenuOpen_Click);
      // 
      // labelCaMenuTitle
      // 
      this.labelCaMenuTitle.AutoSize = true;
      this.labelCaMenuTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelCaMenuTitle.Location = new System.Drawing.Point(6, 16);
      this.labelCaMenuTitle.Name = "labelCaMenuTitle";
      this.labelCaMenuTitle.Size = new System.Drawing.Size(35, 15);
      this.labelCaMenuTitle.TabIndex = 0;
      this.labelCaMenuTitle.Text = "Title";
      // 
      // labelCaMenuSubTitle
      // 
      this.labelCaMenuSubTitle.AutoSize = true;
      this.labelCaMenuSubTitle.Location = new System.Drawing.Point(6, 38);
      this.labelCaMenuSubTitle.Name = "labelCaMenuSubTitle";
      this.labelCaMenuSubTitle.Size = new System.Drawing.Size(46, 13);
      this.labelCaMenuSubTitle.TabIndex = 1;
      this.labelCaMenuSubTitle.Text = "SubTitle";
      // 
      // groupBoxConditionalAccess
      // 
      this.groupBoxConditionalAccess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxConditionalAccess.Controls.Add(this.labelConditionalAccessProviders);
      this.groupBoxConditionalAccess.Controls.Add(this.textBoxConditionalAccessProviders);
      this.groupBoxConditionalAccess.Controls.Add(this.labelMultiChannelDecryptMode);
      this.groupBoxConditionalAccess.Controls.Add(this.comboBoxMultiChannelDecryptMode);
      this.groupBoxConditionalAccess.Controls.Add(this.labelCamType);
      this.groupBoxConditionalAccess.Controls.Add(this.comboBoxCamType);
      this.groupBoxConditionalAccess.Controls.Add(this.checkBoxUseConditionalAccess);
      this.groupBoxConditionalAccess.Controls.Add(this.labelDecryptLimit1);
      this.groupBoxConditionalAccess.Controls.Add(this.labelDecryptLimit2);
      this.groupBoxConditionalAccess.Controls.Add(this.numericUpDownDecryptLimit);
      this.groupBoxConditionalAccess.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxConditionalAccess.Location = new System.Drawing.Point(6, 6);
      this.groupBoxConditionalAccess.Name = "groupBoxConditionalAccess";
      this.groupBoxConditionalAccess.Size = new System.Drawing.Size(311, 154);
      this.groupBoxConditionalAccess.TabIndex = 0;
      this.groupBoxConditionalAccess.TabStop = false;
      this.groupBoxConditionalAccess.Text = "Settings";
      // 
      // labelConditionalAccessProviders
      // 
      this.labelConditionalAccessProviders.AutoSize = true;
      this.labelConditionalAccessProviders.Location = new System.Drawing.Point(6, 44);
      this.labelConditionalAccessProviders.Name = "labelConditionalAccessProviders";
      this.labelConditionalAccessProviders.Size = new System.Drawing.Size(54, 13);
      this.labelConditionalAccessProviders.TabIndex = 1;
      this.labelConditionalAccessProviders.Text = "Providers:";
      // 
      // textBoxConditionalAccessProviders
      // 
      this.textBoxConditionalAccessProviders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxConditionalAccessProviders.Location = new System.Drawing.Point(68, 41);
      this.textBoxConditionalAccessProviders.Name = "textBoxConditionalAccessProviders";
      this.textBoxConditionalAccessProviders.Size = new System.Drawing.Size(237, 20);
      this.textBoxConditionalAccessProviders.TabIndex = 2;
      // 
      // labelMultiChannelDecryptMode
      // 
      this.labelMultiChannelDecryptMode.AutoSize = true;
      this.labelMultiChannelDecryptMode.Location = new System.Drawing.Point(6, 123);
      this.labelMultiChannelDecryptMode.Name = "labelMultiChannelDecryptMode";
      this.labelMultiChannelDecryptMode.Size = new System.Drawing.Size(140, 13);
      this.labelMultiChannelDecryptMode.TabIndex = 8;
      this.labelMultiChannelDecryptMode.Text = "Multi-channel decrypt mode:";
      // 
      // comboBoxMultiChannelDecryptMode
      // 
      this.comboBoxMultiChannelDecryptMode.FormattingEnabled = true;
      this.comboBoxMultiChannelDecryptMode.Location = new System.Drawing.Point(152, 120);
      this.comboBoxMultiChannelDecryptMode.Name = "comboBoxMultiChannelDecryptMode";
      this.comboBoxMultiChannelDecryptMode.Size = new System.Drawing.Size(100, 21);
      this.comboBoxMultiChannelDecryptMode.TabIndex = 9;
      // 
      // labelCamType
      // 
      this.labelCamType.AutoSize = true;
      this.labelCamType.Location = new System.Drawing.Point(6, 70);
      this.labelCamType.Name = "labelCamType";
      this.labelCamType.Size = new System.Drawing.Size(56, 13);
      this.labelCamType.TabIndex = 3;
      this.labelCamType.Text = "CAM type:";
      // 
      // comboBoxCamType
      // 
      this.comboBoxCamType.FormattingEnabled = true;
      this.comboBoxCamType.Location = new System.Drawing.Point(68, 67);
      this.comboBoxCamType.Name = "comboBoxCamType";
      this.comboBoxCamType.Size = new System.Drawing.Size(90, 21);
      this.comboBoxCamType.TabIndex = 4;
      // 
      // checkBoxUseConditionalAccess
      // 
      this.checkBoxUseConditionalAccess.AutoSize = true;
      this.checkBoxUseConditionalAccess.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseConditionalAccess.Location = new System.Drawing.Point(9, 18);
      this.checkBoxUseConditionalAccess.Name = "checkBoxUseConditionalAccess";
      this.checkBoxUseConditionalAccess.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxUseConditionalAccess.Size = new System.Drawing.Size(179, 17);
      this.checkBoxUseConditionalAccess.TabIndex = 0;
      this.checkBoxUseConditionalAccess.Text = "This tuner can decrypt channels.";
      this.checkBoxUseConditionalAccess.UseVisualStyleBackColor = true;
      this.checkBoxUseConditionalAccess.CheckedChanged += new System.EventHandler(this.checkBoxConditionalAccessEnabled_CheckedChanged);
      // 
      // labelDecryptLimit1
      // 
      this.labelDecryptLimit1.AutoSize = true;
      this.labelDecryptLimit1.Location = new System.Drawing.Point(6, 96);
      this.labelDecryptLimit1.Name = "labelDecryptLimit1";
      this.labelDecryptLimit1.Size = new System.Drawing.Size(113, 13);
      this.labelDecryptLimit1.TabIndex = 5;
      this.labelDecryptLimit1.Text = "This tuner can decrypt";
      // 
      // labelDecryptLimit2
      // 
      this.labelDecryptLimit2.AutoSize = true;
      this.labelDecryptLimit2.Location = new System.Drawing.Point(159, 96);
      this.labelDecryptLimit2.Name = "labelDecryptLimit2";
      this.labelDecryptLimit2.Size = new System.Drawing.Size(130, 13);
      this.labelDecryptLimit2.TabIndex = 7;
      this.labelDecryptLimit2.Text = "channel(s) simultaneously.";
      // 
      // numericUpDownDecryptLimit
      // 
      this.numericUpDownDecryptLimit.Location = new System.Drawing.Point(120, 94);
      this.numericUpDownDecryptLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownDecryptLimit.Name = "numericUpDownDecryptLimit";
      this.numericUpDownDecryptLimit.Size = new System.Drawing.Size(38, 20);
      this.numericUpDownDecryptLimit.TabIndex = 6;
      this.numericUpDownDecryptLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownDecryptLimit.TruncateDecimalPlaces = false;
      this.numericUpDownDecryptLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // tabPageAnalog
      // 
      this.tabPageAnalog.Controls.Add(this.groupBoxEncoderSettings);
      this.tabPageAnalog.Controls.Add(this.groupBoxVideo);
      this.tabPageAnalog.Controls.Add(this.groupBoxSoftwareEncoders);
      this.tabPageAnalog.Controls.Add(this.groupBoxVideoAndCameraProperties);
      this.tabPageAnalog.Location = new System.Drawing.Point(4, 22);
      this.tabPageAnalog.Name = "tabPageAnalog";
      this.tabPageAnalog.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageAnalog.Size = new System.Drawing.Size(323, 470);
      this.tabPageAnalog.TabIndex = 3;
      this.tabPageAnalog.Text = "Analog";
      this.tabPageAnalog.UseVisualStyleBackColor = true;
      // 
      // groupBoxEncoderSettings
      // 
      this.groupBoxEncoderSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxEncoderSettings.Controls.Add(this.buttonEncoderSettingsCheckSupport);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderSettingsRecording);
      this.groupBoxEncoderSettings.Controls.Add(this.numericUpDownEncoderBitRateValuePeakRecording);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderBitRateValuePeakUnitRecording);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderBitRateValuePeakRecording);
      this.groupBoxEncoderSettings.Controls.Add(this.numericUpDownEncoderBitRateValueRecording);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderBitRateValueUnitRecording);
      this.groupBoxEncoderSettings.Controls.Add(this.comboBoxEncoderBitRateModeRecording);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderBitRateModeRecording);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderBitRateValueRecording);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderSettingsTimeShifting);
      this.groupBoxEncoderSettings.Controls.Add(this.numericUpDownEncoderBitRateValuePeakTimeShifting);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderBitRateValuePeakUnitTimeShifting);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderBitRateValuePeakTimeShifting);
      this.groupBoxEncoderSettings.Controls.Add(this.numericUpDownEncoderBitRateValueTimeShifting);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderBitRateValueUnitTimeShifting);
      this.groupBoxEncoderSettings.Controls.Add(this.comboBoxEncoderBitRateModeTimeShifting);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderBitRateModeTimeShifting);
      this.groupBoxEncoderSettings.Controls.Add(this.labelEncoderBitRateValueTimeShifting);
      this.groupBoxEncoderSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxEncoderSettings.Location = new System.Drawing.Point(6, 339);
      this.groupBoxEncoderSettings.Name = "groupBoxEncoderSettings";
      this.groupBoxEncoderSettings.Size = new System.Drawing.Size(311, 120);
      this.groupBoxEncoderSettings.TabIndex = 3;
      this.groupBoxEncoderSettings.TabStop = false;
      this.groupBoxEncoderSettings.Text = "Encoder Settings";
      // 
      // buttonEncoderSettingsCheckSupport
      // 
      this.buttonEncoderSettingsCheckSupport.Location = new System.Drawing.Point(9, 19);
      this.buttonEncoderSettingsCheckSupport.Name = "buttonEncoderSettingsCheckSupport";
      this.buttonEncoderSettingsCheckSupport.Size = new System.Drawing.Size(110, 23);
      this.buttonEncoderSettingsCheckSupport.TabIndex = 0;
      this.buttonEncoderSettingsCheckSupport.Text = "Check For &Support";
      this.buttonEncoderSettingsCheckSupport.UseVisualStyleBackColor = true;
      this.buttonEncoderSettingsCheckSupport.Click += new System.EventHandler(this.buttonEncoderSettingsCheckSupport_Click);
      // 
      // labelEncoderSettingsRecording
      // 
      this.labelEncoderSettingsRecording.AutoSize = true;
      this.labelEncoderSettingsRecording.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelEncoderSettingsRecording.Location = new System.Drawing.Point(162, 16);
      this.labelEncoderSettingsRecording.Name = "labelEncoderSettingsRecording";
      this.labelEncoderSettingsRecording.Size = new System.Drawing.Size(69, 13);
      this.labelEncoderSettingsRecording.TabIndex = 10;
      this.labelEncoderSettingsRecording.Text = "Recording:";
      this.labelEncoderSettingsRecording.Visible = false;
      // 
      // numericUpDownEncoderBitRateValuePeakRecording
      // 
      this.numericUpDownEncoderBitRateValuePeakRecording.Enabled = false;
      this.numericUpDownEncoderBitRateValuePeakRecording.Location = new System.Drawing.Point(205, 90);
      this.numericUpDownEncoderBitRateValuePeakRecording.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      this.numericUpDownEncoderBitRateValuePeakRecording.Name = "numericUpDownEncoderBitRateValuePeakRecording";
      this.numericUpDownEncoderBitRateValuePeakRecording.Size = new System.Drawing.Size(40, 20);
      this.numericUpDownEncoderBitRateValuePeakRecording.TabIndex = 17;
      this.numericUpDownEncoderBitRateValuePeakRecording.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownEncoderBitRateValuePeakRecording.TruncateDecimalPlaces = false;
      this.numericUpDownEncoderBitRateValuePeakRecording.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      this.numericUpDownEncoderBitRateValuePeakRecording.Visible = false;
      this.numericUpDownEncoderBitRateValuePeakRecording.ValueChanged += new System.EventHandler(this.numericUpDownEncoderBitRateValuePeakRecording_ValueChanged);
      // 
      // labelEncoderBitRateValuePeakUnitRecording
      // 
      this.labelEncoderBitRateValuePeakUnitRecording.AutoSize = true;
      this.labelEncoderBitRateValuePeakUnitRecording.Location = new System.Drawing.Point(247, 92);
      this.labelEncoderBitRateValuePeakUnitRecording.Name = "labelEncoderBitRateValuePeakUnitRecording";
      this.labelEncoderBitRateValuePeakUnitRecording.Size = new System.Drawing.Size(15, 13);
      this.labelEncoderBitRateValuePeakUnitRecording.TabIndex = 18;
      this.labelEncoderBitRateValuePeakUnitRecording.Text = "%";
      this.labelEncoderBitRateValuePeakUnitRecording.Visible = false;
      // 
      // labelEncoderBitRateValuePeakRecording
      // 
      this.labelEncoderBitRateValuePeakRecording.AutoSize = true;
      this.labelEncoderBitRateValuePeakRecording.Location = new System.Drawing.Point(162, 92);
      this.labelEncoderBitRateValuePeakRecording.Name = "labelEncoderBitRateValuePeakRecording";
      this.labelEncoderBitRateValuePeakRecording.Size = new System.Drawing.Size(35, 13);
      this.labelEncoderBitRateValuePeakRecording.TabIndex = 16;
      this.labelEncoderBitRateValuePeakRecording.Text = "Peak:";
      this.labelEncoderBitRateValuePeakRecording.Visible = false;
      // 
      // numericUpDownEncoderBitRateValueRecording
      // 
      this.numericUpDownEncoderBitRateValueRecording.Enabled = false;
      this.numericUpDownEncoderBitRateValueRecording.Location = new System.Drawing.Point(205, 64);
      this.numericUpDownEncoderBitRateValueRecording.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      this.numericUpDownEncoderBitRateValueRecording.Name = "numericUpDownEncoderBitRateValueRecording";
      this.numericUpDownEncoderBitRateValueRecording.Size = new System.Drawing.Size(40, 20);
      this.numericUpDownEncoderBitRateValueRecording.TabIndex = 14;
      this.numericUpDownEncoderBitRateValueRecording.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownEncoderBitRateValueRecording.TruncateDecimalPlaces = false;
      this.numericUpDownEncoderBitRateValueRecording.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      this.numericUpDownEncoderBitRateValueRecording.Visible = false;
      this.numericUpDownEncoderBitRateValueRecording.ValueChanged += new System.EventHandler(this.numericUpDownEncoderBitRateValueRecording_ValueChanged);
      // 
      // labelEncoderBitRateValueUnitRecording
      // 
      this.labelEncoderBitRateValueUnitRecording.AutoSize = true;
      this.labelEncoderBitRateValueUnitRecording.Location = new System.Drawing.Point(247, 66);
      this.labelEncoderBitRateValueUnitRecording.Name = "labelEncoderBitRateValueUnitRecording";
      this.labelEncoderBitRateValueUnitRecording.Size = new System.Drawing.Size(15, 13);
      this.labelEncoderBitRateValueUnitRecording.TabIndex = 15;
      this.labelEncoderBitRateValueUnitRecording.Text = "%";
      this.labelEncoderBitRateValueUnitRecording.Visible = false;
      // 
      // comboBoxEncoderBitRateModeRecording
      // 
      this.comboBoxEncoderBitRateModeRecording.FormattingEnabled = true;
      this.comboBoxEncoderBitRateModeRecording.Location = new System.Drawing.Point(205, 37);
      this.comboBoxEncoderBitRateModeRecording.Name = "comboBoxEncoderBitRateModeRecording";
      this.comboBoxEncoderBitRateModeRecording.Size = new System.Drawing.Size(100, 21);
      this.comboBoxEncoderBitRateModeRecording.TabIndex = 12;
      this.comboBoxEncoderBitRateModeRecording.Visible = false;
      this.comboBoxEncoderBitRateModeRecording.SelectedIndexChanged += new System.EventHandler(this.comboBoxEncoderBitRateModeRecording_SelectedIndexChanged);
      // 
      // labelEncoderBitRateModeRecording
      // 
      this.labelEncoderBitRateModeRecording.AutoSize = true;
      this.labelEncoderBitRateModeRecording.Location = new System.Drawing.Point(162, 40);
      this.labelEncoderBitRateModeRecording.Name = "labelEncoderBitRateModeRecording";
      this.labelEncoderBitRateModeRecording.Size = new System.Drawing.Size(37, 13);
      this.labelEncoderBitRateModeRecording.TabIndex = 11;
      this.labelEncoderBitRateModeRecording.Text = "Mode:";
      this.labelEncoderBitRateModeRecording.Visible = false;
      // 
      // labelEncoderBitRateValueRecording
      // 
      this.labelEncoderBitRateValueRecording.AutoSize = true;
      this.labelEncoderBitRateValueRecording.Location = new System.Drawing.Point(162, 66);
      this.labelEncoderBitRateValueRecording.Name = "labelEncoderBitRateValueRecording";
      this.labelEncoderBitRateValueRecording.Size = new System.Drawing.Size(37, 13);
      this.labelEncoderBitRateValueRecording.TabIndex = 13;
      this.labelEncoderBitRateValueRecording.Text = "Value:";
      this.labelEncoderBitRateValueRecording.Visible = false;
      // 
      // labelEncoderSettingsTimeShifting
      // 
      this.labelEncoderSettingsTimeShifting.AutoSize = true;
      this.labelEncoderSettingsTimeShifting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelEncoderSettingsTimeShifting.Location = new System.Drawing.Point(6, 16);
      this.labelEncoderSettingsTimeShifting.Name = "labelEncoderSettingsTimeShifting";
      this.labelEncoderSettingsTimeShifting.Size = new System.Drawing.Size(83, 13);
      this.labelEncoderSettingsTimeShifting.TabIndex = 1;
      this.labelEncoderSettingsTimeShifting.Text = "Time-shifting:";
      this.labelEncoderSettingsTimeShifting.Visible = false;
      // 
      // numericUpDownEncoderBitRateValuePeakTimeShifting
      // 
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.Enabled = false;
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.Location = new System.Drawing.Point(49, 90);
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.Name = "numericUpDownEncoderBitRateValuePeakTimeShifting";
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.Size = new System.Drawing.Size(40, 20);
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.TabIndex = 8;
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.TruncateDecimalPlaces = false;
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.Visible = false;
      this.numericUpDownEncoderBitRateValuePeakTimeShifting.ValueChanged += new System.EventHandler(this.numericUpDownEncoderBitRateValuePeakTimeShifting_ValueChanged);
      // 
      // labelEncoderBitRateValuePeakUnitTimeShifting
      // 
      this.labelEncoderBitRateValuePeakUnitTimeShifting.AutoSize = true;
      this.labelEncoderBitRateValuePeakUnitTimeShifting.Location = new System.Drawing.Point(91, 92);
      this.labelEncoderBitRateValuePeakUnitTimeShifting.Name = "labelEncoderBitRateValuePeakUnitTimeShifting";
      this.labelEncoderBitRateValuePeakUnitTimeShifting.Size = new System.Drawing.Size(15, 13);
      this.labelEncoderBitRateValuePeakUnitTimeShifting.TabIndex = 9;
      this.labelEncoderBitRateValuePeakUnitTimeShifting.Text = "%";
      this.labelEncoderBitRateValuePeakUnitTimeShifting.Visible = false;
      // 
      // labelEncoderBitRateValuePeakTimeShifting
      // 
      this.labelEncoderBitRateValuePeakTimeShifting.AutoSize = true;
      this.labelEncoderBitRateValuePeakTimeShifting.Location = new System.Drawing.Point(6, 92);
      this.labelEncoderBitRateValuePeakTimeShifting.Name = "labelEncoderBitRateValuePeakTimeShifting";
      this.labelEncoderBitRateValuePeakTimeShifting.Size = new System.Drawing.Size(35, 13);
      this.labelEncoderBitRateValuePeakTimeShifting.TabIndex = 7;
      this.labelEncoderBitRateValuePeakTimeShifting.Text = "Peak:";
      this.labelEncoderBitRateValuePeakTimeShifting.Visible = false;
      // 
      // numericUpDownEncoderBitRateValueTimeShifting
      // 
      this.numericUpDownEncoderBitRateValueTimeShifting.Enabled = false;
      this.numericUpDownEncoderBitRateValueTimeShifting.Location = new System.Drawing.Point(49, 64);
      this.numericUpDownEncoderBitRateValueTimeShifting.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      this.numericUpDownEncoderBitRateValueTimeShifting.Name = "numericUpDownEncoderBitRateValueTimeShifting";
      this.numericUpDownEncoderBitRateValueTimeShifting.Size = new System.Drawing.Size(40, 20);
      this.numericUpDownEncoderBitRateValueTimeShifting.TabIndex = 5;
      this.numericUpDownEncoderBitRateValueTimeShifting.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownEncoderBitRateValueTimeShifting.TruncateDecimalPlaces = false;
      this.numericUpDownEncoderBitRateValueTimeShifting.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      this.numericUpDownEncoderBitRateValueTimeShifting.Visible = false;
      this.numericUpDownEncoderBitRateValueTimeShifting.ValueChanged += new System.EventHandler(this.numericUpDownEncoderBitRateValueTimeShifting_ValueChanged);
      // 
      // labelEncoderBitRateValueUnitTimeShifting
      // 
      this.labelEncoderBitRateValueUnitTimeShifting.AutoSize = true;
      this.labelEncoderBitRateValueUnitTimeShifting.Location = new System.Drawing.Point(91, 66);
      this.labelEncoderBitRateValueUnitTimeShifting.Name = "labelEncoderBitRateValueUnitTimeShifting";
      this.labelEncoderBitRateValueUnitTimeShifting.Size = new System.Drawing.Size(15, 13);
      this.labelEncoderBitRateValueUnitTimeShifting.TabIndex = 6;
      this.labelEncoderBitRateValueUnitTimeShifting.Text = "%";
      this.labelEncoderBitRateValueUnitTimeShifting.Visible = false;
      // 
      // comboBoxEncoderBitRateModeTimeShifting
      // 
      this.comboBoxEncoderBitRateModeTimeShifting.FormattingEnabled = true;
      this.comboBoxEncoderBitRateModeTimeShifting.Location = new System.Drawing.Point(49, 37);
      this.comboBoxEncoderBitRateModeTimeShifting.Name = "comboBoxEncoderBitRateModeTimeShifting";
      this.comboBoxEncoderBitRateModeTimeShifting.Size = new System.Drawing.Size(100, 21);
      this.comboBoxEncoderBitRateModeTimeShifting.TabIndex = 3;
      this.comboBoxEncoderBitRateModeTimeShifting.Visible = false;
      this.comboBoxEncoderBitRateModeTimeShifting.SelectedIndexChanged += new System.EventHandler(this.comboBoxEncoderBitRateModeTimeShifting_SelectedIndexChanged);
      // 
      // labelEncoderBitRateModeTimeShifting
      // 
      this.labelEncoderBitRateModeTimeShifting.AutoSize = true;
      this.labelEncoderBitRateModeTimeShifting.Location = new System.Drawing.Point(6, 40);
      this.labelEncoderBitRateModeTimeShifting.Name = "labelEncoderBitRateModeTimeShifting";
      this.labelEncoderBitRateModeTimeShifting.Size = new System.Drawing.Size(37, 13);
      this.labelEncoderBitRateModeTimeShifting.TabIndex = 2;
      this.labelEncoderBitRateModeTimeShifting.Text = "Mode:";
      this.labelEncoderBitRateModeTimeShifting.Visible = false;
      // 
      // labelEncoderBitRateValueTimeShifting
      // 
      this.labelEncoderBitRateValueTimeShifting.AutoSize = true;
      this.labelEncoderBitRateValueTimeShifting.Location = new System.Drawing.Point(6, 66);
      this.labelEncoderBitRateValueTimeShifting.Name = "labelEncoderBitRateValueTimeShifting";
      this.labelEncoderBitRateValueTimeShifting.Size = new System.Drawing.Size(37, 13);
      this.labelEncoderBitRateValueTimeShifting.TabIndex = 4;
      this.labelEncoderBitRateValueTimeShifting.Text = "Value:";
      this.labelEncoderBitRateValueTimeShifting.Visible = false;
      // 
      // groupBoxVideo
      // 
      this.groupBoxVideo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxVideo.Controls.Add(this.labelAnalogVideoStandard);
      this.groupBoxVideo.Controls.Add(this.comboBoxAnalogVideoStandard);
      this.groupBoxVideo.Controls.Add(this.comboBoxFrameSize);
      this.groupBoxVideo.Controls.Add(this.comboBoxFrameRate);
      this.groupBoxVideo.Controls.Add(this.labelFrameRate);
      this.groupBoxVideo.Controls.Add(this.labelFrameSize);
      this.groupBoxVideo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxVideo.Location = new System.Drawing.Point(6, 6);
      this.groupBoxVideo.Name = "groupBoxVideo";
      this.groupBoxVideo.Size = new System.Drawing.Size(311, 106);
      this.groupBoxVideo.TabIndex = 0;
      this.groupBoxVideo.TabStop = false;
      this.groupBoxVideo.Text = "Video";
      // 
      // labelAnalogVideoStandard
      // 
      this.labelAnalogVideoStandard.AutoSize = true;
      this.labelAnalogVideoStandard.Location = new System.Drawing.Point(6, 22);
      this.labelAnalogVideoStandard.Name = "labelAnalogVideoStandard";
      this.labelAnalogVideoStandard.Size = new System.Drawing.Size(53, 13);
      this.labelAnalogVideoStandard.TabIndex = 0;
      this.labelAnalogVideoStandard.Text = "Standard:";
      // 
      // comboBoxAnalogVideoStandard
      // 
      this.comboBoxAnalogVideoStandard.FormattingEnabled = true;
      this.comboBoxAnalogVideoStandard.Location = new System.Drawing.Point(72, 19);
      this.comboBoxAnalogVideoStandard.Name = "comboBoxAnalogVideoStandard";
      this.comboBoxAnalogVideoStandard.Size = new System.Drawing.Size(110, 21);
      this.comboBoxAnalogVideoStandard.TabIndex = 1;
      // 
      // comboBoxFrameSize
      // 
      this.comboBoxFrameSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxFrameSize.FormattingEnabled = true;
      this.comboBoxFrameSize.Location = new System.Drawing.Point(72, 73);
      this.comboBoxFrameSize.Name = "comboBoxFrameSize";
      this.comboBoxFrameSize.Size = new System.Drawing.Size(233, 21);
      this.comboBoxFrameSize.TabIndex = 5;
      // 
      // comboBoxFrameRate
      // 
      this.comboBoxFrameRate.FormattingEnabled = true;
      this.comboBoxFrameRate.Location = new System.Drawing.Point(72, 46);
      this.comboBoxFrameRate.Name = "comboBoxFrameRate";
      this.comboBoxFrameRate.Size = new System.Drawing.Size(110, 21);
      this.comboBoxFrameRate.TabIndex = 3;
      // 
      // labelFrameRate
      // 
      this.labelFrameRate.AutoSize = true;
      this.labelFrameRate.Location = new System.Drawing.Point(6, 49);
      this.labelFrameRate.Name = "labelFrameRate";
      this.labelFrameRate.Size = new System.Drawing.Size(60, 13);
      this.labelFrameRate.TabIndex = 2;
      this.labelFrameRate.Text = "Frame rate:";
      // 
      // labelFrameSize
      // 
      this.labelFrameSize.AutoSize = true;
      this.labelFrameSize.Location = new System.Drawing.Point(6, 76);
      this.labelFrameSize.Name = "labelFrameSize";
      this.labelFrameSize.Size = new System.Drawing.Size(60, 13);
      this.labelFrameSize.TabIndex = 4;
      this.labelFrameSize.Text = "Frame size:";
      // 
      // groupBoxSoftwareEncoders
      // 
      this.groupBoxSoftwareEncoders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSoftwareEncoders.Controls.Add(this.comboBoxSoftwareEncoderAudio);
      this.groupBoxSoftwareEncoders.Controls.Add(this.labelSoftwareEncoderAudio);
      this.groupBoxSoftwareEncoders.Controls.Add(this.labelSoftwareEncoderVideo);
      this.groupBoxSoftwareEncoders.Controls.Add(this.comboBoxSoftwareEncoderVideo);
      this.groupBoxSoftwareEncoders.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSoftwareEncoders.Location = new System.Drawing.Point(6, 255);
      this.groupBoxSoftwareEncoders.Name = "groupBoxSoftwareEncoders";
      this.groupBoxSoftwareEncoders.Size = new System.Drawing.Size(311, 78);
      this.groupBoxSoftwareEncoders.TabIndex = 2;
      this.groupBoxSoftwareEncoders.TabStop = false;
      this.groupBoxSoftwareEncoders.Text = "Software Encoders";
      // 
      // comboBoxSoftwareEncoderAudio
      // 
      this.comboBoxSoftwareEncoderAudio.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxSoftwareEncoderAudio.FormattingEnabled = true;
      this.comboBoxSoftwareEncoderAudio.Location = new System.Drawing.Point(72, 46);
      this.comboBoxSoftwareEncoderAudio.Name = "comboBoxSoftwareEncoderAudio";
      this.comboBoxSoftwareEncoderAudio.Size = new System.Drawing.Size(233, 21);
      this.comboBoxSoftwareEncoderAudio.TabIndex = 3;
      // 
      // labelSoftwareEncoderAudio
      // 
      this.labelSoftwareEncoderAudio.AutoSize = true;
      this.labelSoftwareEncoderAudio.Location = new System.Drawing.Point(6, 49);
      this.labelSoftwareEncoderAudio.Name = "labelSoftwareEncoderAudio";
      this.labelSoftwareEncoderAudio.Size = new System.Drawing.Size(37, 13);
      this.labelSoftwareEncoderAudio.TabIndex = 2;
      this.labelSoftwareEncoderAudio.Text = "Audio:";
      // 
      // labelSoftwareEncoderVideo
      // 
      this.labelSoftwareEncoderVideo.AutoSize = true;
      this.labelSoftwareEncoderVideo.Location = new System.Drawing.Point(6, 22);
      this.labelSoftwareEncoderVideo.Name = "labelSoftwareEncoderVideo";
      this.labelSoftwareEncoderVideo.Size = new System.Drawing.Size(37, 13);
      this.labelSoftwareEncoderVideo.TabIndex = 0;
      this.labelSoftwareEncoderVideo.Text = "Video:";
      // 
      // comboBoxSoftwareEncoderVideo
      // 
      this.comboBoxSoftwareEncoderVideo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxSoftwareEncoderVideo.FormattingEnabled = true;
      this.comboBoxSoftwareEncoderVideo.Location = new System.Drawing.Point(72, 19);
      this.comboBoxSoftwareEncoderVideo.Name = "comboBoxSoftwareEncoderVideo";
      this.comboBoxSoftwareEncoderVideo.Size = new System.Drawing.Size(233, 21);
      this.comboBoxSoftwareEncoderVideo.TabIndex = 1;
      // 
      // groupBoxVideoAndCameraProperties
      // 
      this.groupBoxVideoAndCameraProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxVideoAndCameraProperties.Controls.Add(this.checkBoxVideoOrCameraPropertyValue);
      this.groupBoxVideoAndCameraProperties.Controls.Add(this.buttonRestoreAllDefaults);
      this.groupBoxVideoAndCameraProperties.Controls.Add(this.labelVideoOrCameraPropertyValue);
      this.groupBoxVideoAndCameraProperties.Controls.Add(this.labelVideoOrCameraProperty);
      this.groupBoxVideoAndCameraProperties.Controls.Add(this.comboBoxVideoOrCameraProperty);
      this.groupBoxVideoAndCameraProperties.Controls.Add(this.scrollBarVideoOrCameraPropertyValue);
      this.groupBoxVideoAndCameraProperties.Controls.Add(this.labelVideoOrCameraPropertyValueDisplay);
      this.groupBoxVideoAndCameraProperties.Controls.Add(this.buttonRestoreDefault);
      this.groupBoxVideoAndCameraProperties.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxVideoAndCameraProperties.Location = new System.Drawing.Point(6, 118);
      this.groupBoxVideoAndCameraProperties.Name = "groupBoxVideoAndCameraProperties";
      this.groupBoxVideoAndCameraProperties.Size = new System.Drawing.Size(311, 131);
      this.groupBoxVideoAndCameraProperties.TabIndex = 1;
      this.groupBoxVideoAndCameraProperties.TabStop = false;
      this.groupBoxVideoAndCameraProperties.Text = "Video && Camera Properties";
      // 
      // checkBoxVideoOrCameraPropertyValue
      // 
      this.checkBoxVideoOrCameraPropertyValue.AutoSize = true;
      this.checkBoxVideoOrCameraPropertyValue.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxVideoOrCameraPropertyValue.Location = new System.Drawing.Point(72, 52);
      this.checkBoxVideoOrCameraPropertyValue.Name = "checkBoxVideoOrCameraPropertyValue";
      this.checkBoxVideoOrCameraPropertyValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxVideoOrCameraPropertyValue.Size = new System.Drawing.Size(109, 17);
      this.checkBoxVideoOrCameraPropertyValue.TabIndex = 3;
      this.checkBoxVideoOrCameraPropertyValue.Text = "Automatic control.";
      this.checkBoxVideoOrCameraPropertyValue.UseVisualStyleBackColor = true;
      this.checkBoxVideoOrCameraPropertyValue.CheckedChanged += new System.EventHandler(this.checkBoxVideoOrCameraPropertyValue_CheckedChanged);
      // 
      // buttonRestoreAllDefaults
      // 
      this.buttonRestoreAllDefaults.Location = new System.Drawing.Point(188, 98);
      this.buttonRestoreAllDefaults.Name = "buttonRestoreAllDefaults";
      this.buttonRestoreAllDefaults.Size = new System.Drawing.Size(110, 23);
      this.buttonRestoreAllDefaults.TabIndex = 7;
      this.buttonRestoreAllDefaults.Text = "Restore &All Defaults";
      this.buttonRestoreAllDefaults.UseVisualStyleBackColor = true;
      this.buttonRestoreAllDefaults.Click += new System.EventHandler(this.buttonRestoreAllDefaults_Click);
      // 
      // labelVideoOrCameraPropertyValue
      // 
      this.labelVideoOrCameraPropertyValue.AutoSize = true;
      this.labelVideoOrCameraPropertyValue.Location = new System.Drawing.Point(6, 54);
      this.labelVideoOrCameraPropertyValue.Name = "labelVideoOrCameraPropertyValue";
      this.labelVideoOrCameraPropertyValue.Size = new System.Drawing.Size(37, 13);
      this.labelVideoOrCameraPropertyValue.TabIndex = 2;
      this.labelVideoOrCameraPropertyValue.Text = "Value:";
      // 
      // labelVideoOrCameraProperty
      // 
      this.labelVideoOrCameraProperty.AutoSize = true;
      this.labelVideoOrCameraProperty.Location = new System.Drawing.Point(6, 22);
      this.labelVideoOrCameraProperty.Name = "labelVideoOrCameraProperty";
      this.labelVideoOrCameraProperty.Size = new System.Drawing.Size(49, 13);
      this.labelVideoOrCameraProperty.TabIndex = 0;
      this.labelVideoOrCameraProperty.Text = "Property:";
      // 
      // comboBoxVideoOrCameraProperty
      // 
      this.comboBoxVideoOrCameraProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxVideoOrCameraProperty.FormattingEnabled = true;
      this.comboBoxVideoOrCameraProperty.Location = new System.Drawing.Point(72, 19);
      this.comboBoxVideoOrCameraProperty.Name = "comboBoxVideoOrCameraProperty";
      this.comboBoxVideoOrCameraProperty.Size = new System.Drawing.Size(233, 21);
      this.comboBoxVideoOrCameraProperty.TabIndex = 1;
      this.comboBoxVideoOrCameraProperty.SelectedIndexChanged += new System.EventHandler(this.comboBoxVideoOrCameraProperty_SelectedIndexChanged);
      // 
      // scrollBarVideoOrCameraPropertyValue
      // 
      this.scrollBarVideoOrCameraPropertyValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.scrollBarVideoOrCameraPropertyValue.Location = new System.Drawing.Point(72, 74);
      this.scrollBarVideoOrCameraPropertyValue.Name = "scrollBarVideoOrCameraPropertyValue";
      this.scrollBarVideoOrCameraPropertyValue.Size = new System.Drawing.Size(190, 13);
      this.scrollBarVideoOrCameraPropertyValue.TabIndex = 4;
      this.scrollBarVideoOrCameraPropertyValue.ValueChanged += new System.EventHandler(this.scrollBarVideoOrCameraPropertyValue_ValueChanged);
      // 
      // labelVideoOrCameraPropertyValueDisplay
      // 
      this.labelVideoOrCameraPropertyValueDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelVideoOrCameraPropertyValueDisplay.AutoSize = true;
      this.labelVideoOrCameraPropertyValueDisplay.Location = new System.Drawing.Point(265, 74);
      this.labelVideoOrCameraPropertyValueDisplay.Name = "labelVideoOrCameraPropertyValueDisplay";
      this.labelVideoOrCameraPropertyValueDisplay.Size = new System.Drawing.Size(33, 13);
      this.labelVideoOrCameraPropertyValueDisplay.TabIndex = 5;
      this.labelVideoOrCameraPropertyValueDisplay.Text = "value";
      // 
      // buttonRestoreDefault
      // 
      this.buttonRestoreDefault.Location = new System.Drawing.Point(72, 98);
      this.buttonRestoreDefault.Name = "buttonRestoreDefault";
      this.buttonRestoreDefault.Size = new System.Drawing.Size(110, 23);
      this.buttonRestoreDefault.TabIndex = 6;
      this.buttonRestoreDefault.Text = "&Restore Default";
      this.buttonRestoreDefault.UseVisualStyleBackColor = true;
      this.buttonRestoreDefault.Click += new System.EventHandler(this.buttonRestoreDefault_Click);
      // 
      // tabPageExternalInput
      // 
      this.tabPageExternalInput.Controls.Add(this.groupBoxExternalTuner);
      this.tabPageExternalInput.Controls.Add(this.groupBoxExternalInput);
      this.tabPageExternalInput.Location = new System.Drawing.Point(4, 22);
      this.tabPageExternalInput.Name = "tabPageExternalInput";
      this.tabPageExternalInput.Size = new System.Drawing.Size(323, 470);
      this.tabPageExternalInput.TabIndex = 4;
      this.tabPageExternalInput.Text = "External Input";
      this.tabPageExternalInput.UseVisualStyleBackColor = true;
      // 
      // groupBoxExternalTuner
      // 
      this.groupBoxExternalTuner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxExternalTuner.Controls.Add(this.textBoxExternalTunerProgram);
      this.groupBoxExternalTuner.Controls.Add(this.labelExternalTunerProgram);
      this.groupBoxExternalTuner.Controls.Add(this.labelExternalTunerProgramArguments);
      this.groupBoxExternalTuner.Controls.Add(this.buttonExternalTunerProgramBrowse);
      this.groupBoxExternalTuner.Controls.Add(this.textBoxExternalTunerProgramArguments);
      this.groupBoxExternalTuner.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxExternalTuner.Location = new System.Drawing.Point(6, 143);
      this.groupBoxExternalTuner.Name = "groupBoxExternalTuner";
      this.groupBoxExternalTuner.Size = new System.Drawing.Size(311, 77);
      this.groupBoxExternalTuner.TabIndex = 1;
      this.groupBoxExternalTuner.TabStop = false;
      this.groupBoxExternalTuner.Text = "External Tuner";
      // 
      // textBoxExternalTunerProgram
      // 
      this.textBoxExternalTunerProgram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxExternalTunerProgram.Location = new System.Drawing.Point(89, 19);
      this.textBoxExternalTunerProgram.Name = "textBoxExternalTunerProgram";
      this.textBoxExternalTunerProgram.Size = new System.Drawing.Size(186, 20);
      this.textBoxExternalTunerProgram.TabIndex = 1;
      // 
      // labelExternalTunerProgram
      // 
      this.labelExternalTunerProgram.AutoSize = true;
      this.labelExternalTunerProgram.Location = new System.Drawing.Point(6, 22);
      this.labelExternalTunerProgram.Name = "labelExternalTunerProgram";
      this.labelExternalTunerProgram.Size = new System.Drawing.Size(49, 13);
      this.labelExternalTunerProgram.TabIndex = 0;
      this.labelExternalTunerProgram.Text = "Program:";
      // 
      // labelExternalTunerProgramArguments
      // 
      this.labelExternalTunerProgramArguments.AutoSize = true;
      this.labelExternalTunerProgramArguments.Location = new System.Drawing.Point(6, 48);
      this.labelExternalTunerProgramArguments.Name = "labelExternalTunerProgramArguments";
      this.labelExternalTunerProgramArguments.Size = new System.Drawing.Size(39, 13);
      this.labelExternalTunerProgramArguments.TabIndex = 3;
      this.labelExternalTunerProgramArguments.Text = "Inputs:";
      // 
      // buttonExternalTunerProgramBrowse
      // 
      this.buttonExternalTunerProgramBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonExternalTunerProgramBrowse.Location = new System.Drawing.Point(281, 17);
      this.buttonExternalTunerProgramBrowse.Name = "buttonExternalTunerProgramBrowse";
      this.buttonExternalTunerProgramBrowse.Size = new System.Drawing.Size(24, 23);
      this.buttonExternalTunerProgramBrowse.TabIndex = 2;
      this.buttonExternalTunerProgramBrowse.Text = "...";
      this.buttonExternalTunerProgramBrowse.UseVisualStyleBackColor = true;
      this.buttonExternalTunerProgramBrowse.Click += new System.EventHandler(this.buttonExternalTunerProgramBrowse_Click);
      // 
      // textBoxExternalTunerProgramArguments
      // 
      this.textBoxExternalTunerProgramArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxExternalTunerProgramArguments.Location = new System.Drawing.Point(89, 45);
      this.textBoxExternalTunerProgramArguments.Name = "textBoxExternalTunerProgramArguments";
      this.textBoxExternalTunerProgramArguments.Size = new System.Drawing.Size(216, 20);
      this.textBoxExternalTunerProgramArguments.TabIndex = 4;
      // 
      // groupBoxExternalInput
      // 
      this.groupBoxExternalInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxExternalInput.Controls.Add(this.labelExternalInputPhysicalChannelNumber);
      this.groupBoxExternalInput.Controls.Add(this.labelExternalInputCountry);
      this.groupBoxExternalInput.Controls.Add(this.comboBoxExternalInputCountry);
      this.groupBoxExternalInput.Controls.Add(this.comboBoxExternalInputSourceAudio);
      this.groupBoxExternalInput.Controls.Add(this.labelExternalInputSourceAudio);
      this.groupBoxExternalInput.Controls.Add(this.labelExternalInputSourceVideo);
      this.groupBoxExternalInput.Controls.Add(this.comboBoxExternalInputSourceVideo);
      this.groupBoxExternalInput.Controls.Add(this.numericUpDownExternalInputPhysicalChannelNumber);
      this.groupBoxExternalInput.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxExternalInput.Location = new System.Drawing.Point(6, 6);
      this.groupBoxExternalInput.Name = "groupBoxExternalInput";
      this.groupBoxExternalInput.Size = new System.Drawing.Size(311, 131);
      this.groupBoxExternalInput.TabIndex = 0;
      this.groupBoxExternalInput.TabStop = false;
      this.groupBoxExternalInput.Text = "Settings";
      // 
      // labelExternalInputPhysicalChannelNumber
      // 
      this.labelExternalInputPhysicalChannelNumber.AutoSize = true;
      this.labelExternalInputPhysicalChannelNumber.Location = new System.Drawing.Point(6, 103);
      this.labelExternalInputPhysicalChannelNumber.Name = "labelExternalInputPhysicalChannelNumber";
      this.labelExternalInputPhysicalChannelNumber.Size = new System.Drawing.Size(77, 13);
      this.labelExternalInputPhysicalChannelNumber.TabIndex = 6;
      this.labelExternalInputPhysicalChannelNumber.Text = "Phys. channel:";
      // 
      // labelExternalInputCountry
      // 
      this.labelExternalInputCountry.AutoSize = true;
      this.labelExternalInputCountry.Location = new System.Drawing.Point(6, 76);
      this.labelExternalInputCountry.Name = "labelExternalInputCountry";
      this.labelExternalInputCountry.Size = new System.Drawing.Size(46, 13);
      this.labelExternalInputCountry.TabIndex = 4;
      this.labelExternalInputCountry.Text = "Country:";
      // 
      // comboBoxExternalInputCountry
      // 
      this.comboBoxExternalInputCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxExternalInputCountry.FormattingEnabled = true;
      this.comboBoxExternalInputCountry.Location = new System.Drawing.Point(89, 73);
      this.comboBoxExternalInputCountry.Name = "comboBoxExternalInputCountry";
      this.comboBoxExternalInputCountry.Size = new System.Drawing.Size(216, 21);
      this.comboBoxExternalInputCountry.TabIndex = 5;
      // 
      // comboBoxExternalInputSourceAudio
      // 
      this.comboBoxExternalInputSourceAudio.FormattingEnabled = true;
      this.comboBoxExternalInputSourceAudio.Location = new System.Drawing.Point(89, 46);
      this.comboBoxExternalInputSourceAudio.Name = "comboBoxExternalInputSourceAudio";
      this.comboBoxExternalInputSourceAudio.Size = new System.Drawing.Size(90, 21);
      this.comboBoxExternalInputSourceAudio.TabIndex = 3;
      // 
      // labelExternalInputSourceAudio
      // 
      this.labelExternalInputSourceAudio.AutoSize = true;
      this.labelExternalInputSourceAudio.Location = new System.Drawing.Point(6, 49);
      this.labelExternalInputSourceAudio.Name = "labelExternalInputSourceAudio";
      this.labelExternalInputSourceAudio.Size = new System.Drawing.Size(72, 13);
      this.labelExternalInputSourceAudio.TabIndex = 2;
      this.labelExternalInputSourceAudio.Text = "Audio source:";
      // 
      // labelExternalInputSourceVideo
      // 
      this.labelExternalInputSourceVideo.AutoSize = true;
      this.labelExternalInputSourceVideo.Location = new System.Drawing.Point(6, 22);
      this.labelExternalInputSourceVideo.Name = "labelExternalInputSourceVideo";
      this.labelExternalInputSourceVideo.Size = new System.Drawing.Size(72, 13);
      this.labelExternalInputSourceVideo.TabIndex = 0;
      this.labelExternalInputSourceVideo.Text = "Video source:";
      // 
      // comboBoxExternalInputSourceVideo
      // 
      this.comboBoxExternalInputSourceVideo.FormattingEnabled = true;
      this.comboBoxExternalInputSourceVideo.Location = new System.Drawing.Point(89, 19);
      this.comboBoxExternalInputSourceVideo.Name = "comboBoxExternalInputSourceVideo";
      this.comboBoxExternalInputSourceVideo.Size = new System.Drawing.Size(90, 21);
      this.comboBoxExternalInputSourceVideo.TabIndex = 1;
      this.comboBoxExternalInputSourceVideo.SelectedIndexChanged += new System.EventHandler(this.comboBoxExternalInputSourceVideo_SelectedIndexChanged);
      // 
      // numericUpDownExternalInputPhysicalChannelNumber
      // 
      this.numericUpDownExternalInputPhysicalChannelNumber.Location = new System.Drawing.Point(89, 100);
      this.numericUpDownExternalInputPhysicalChannelNumber.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
      this.numericUpDownExternalInputPhysicalChannelNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownExternalInputPhysicalChannelNumber.Name = "numericUpDownExternalInputPhysicalChannelNumber";
      this.numericUpDownExternalInputPhysicalChannelNumber.Size = new System.Drawing.Size(50, 20);
      this.numericUpDownExternalInputPhysicalChannelNumber.TabIndex = 7;
      this.numericUpDownExternalInputPhysicalChannelNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownExternalInputPhysicalChannelNumber.TruncateDecimalPlaces = false;
      this.numericUpDownExternalInputPhysicalChannelNumber.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
      // 
      // openFileDialogExternalTunerProgram
      // 
      this.openFileDialogExternalTunerProgram.Filter = "Executables (*.exe, *.bat)|*.exe;*.bat|All Files|*.*";
      this.openFileDialogExternalTunerProgram.Title = "Select the program that controls the external tuner.";
      // 
      // FormEditTuner
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(329, 534);
      this.Controls.Add(this.tabControl);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOkay);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(337, 560);
      this.Name = "FormEditTuner";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit Tuner Settings";
      this.Load += new System.EventHandler(this.FormEditCard_Load);
      this.tabControl.ResumeLayout(false);
      this.tabPageGeneral.ResumeLayout(false);
      this.groupBoxDebug.ResumeLayout(false);
      this.groupBoxDebug.PerformLayout();
      this.groupBoxAdvanced.ResumeLayout(false);
      this.groupBoxAdvanced.PerformLayout();
      this.groupBoxGeneral.ResumeLayout(false);
      this.groupBoxGeneral.PerformLayout();
      this.tabPageConditionalAccess.ResumeLayout(false);
      this.groupBoxCaMenu.ResumeLayout(false);
      this.groupBoxCaMenu.PerformLayout();
      this.groupBoxConditionalAccess.ResumeLayout(false);
      this.groupBoxConditionalAccess.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDecryptLimit)).EndInit();
      this.tabPageAnalog.ResumeLayout(false);
      this.groupBoxEncoderSettings.ResumeLayout(false);
      this.groupBoxEncoderSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEncoderBitRateValuePeakRecording)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEncoderBitRateValueRecording)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEncoderBitRateValuePeakTimeShifting)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEncoderBitRateValueTimeShifting)).EndInit();
      this.groupBoxVideo.ResumeLayout(false);
      this.groupBoxVideo.PerformLayout();
      this.groupBoxSoftwareEncoders.ResumeLayout(false);
      this.groupBoxSoftwareEncoders.PerformLayout();
      this.groupBoxVideoAndCameraProperties.ResumeLayout(false);
      this.groupBoxVideoAndCameraProperties.PerformLayout();
      this.tabPageExternalInput.ResumeLayout(false);
      this.groupBoxExternalTuner.ResumeLayout(false);
      this.groupBoxExternalTuner.PerformLayout();
      this.groupBoxExternalInput.ResumeLayout(false);
      this.groupBoxExternalInput.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExternalInputPhysicalChannelNumber)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MPButton buttonOkay;
    private MPButton buttonCancel;
    private MPTabControl tabControl;
    private MPTabPage tabPageGeneral;
    private MPTabPage tabPageConditionalAccess;
    private MPGroupBox groupBoxAdvanced;
    private MPCheckBox checkBoxUseCustomTuning;
    private MPLabel labelBdaNetworkProvider;
    private MPLabel labelPidFilterMode;
    private MPComboBox comboBoxBdaNetworkProvider;
    private MPComboBox comboBoxPidFilterMode;
    private MPLabel labelAdvancedReadDocumentation;
    private MPComboBox comboBoxIdleMode;
    private MPLabel labelIdleMode;
    private MPGroupBox groupBoxGeneral;
    private MPLabel labelTunerName;
    private MPCheckBox checkBoxUseForEpgGrabbing;
    private MPTextBox textBoxTunerName;
    private MPCheckBox checkBoxPreload;
    private MPGroupBox groupBoxConditionalAccess;
    private MPLabel labelMultiChannelDecryptMode;
    private MPComboBox comboBoxMultiChannelDecryptMode;
    private MPLabel labelCamType;
    private MPComboBox comboBoxCamType;
    private MPCheckBox checkBoxUseConditionalAccess;
    private MPLabel labelDecryptLimit1;
    private MPLabel labelDecryptLimit2;
    private MPNumericUpDown numericUpDownDecryptLimit;
    private MPGroupBox groupBoxCaMenu;
    private MPTextBox textBoxCaMenuAnswer;
    private MPLabel labelCaMenuFooter;
    private MPListBox listBoxCaMenuChoices;
    private MPLabel labelCaMenuEnquiry;
    private MPLabel labelCaMenuTitle;
    private MPLabel labelCaMenuSubTitle;
    private MPButton buttonCaMenuOpen;
    private MPButton buttonCaMenuBackClose;
    private MPButton buttonCaMenuOkaySelect;
    private MPCheckBox checkBoxTsWriterDisableCrcCheck;
    private MPCheckBox checkBoxTsMuxerDumpInputs;
    private MPCheckBox checkBoxTsWriterDumpInputs;
    private MPGroupBox groupBoxDebug;
    private MPLabel labelDebugDoNotEnable;
    private MPTabPage tabPageAnalog;
    private MPGroupBox groupBoxVideoAndCameraProperties;
    private MPHScrollBar scrollBarVideoOrCameraPropertyValue;
    private MPLabel labelVideoOrCameraPropertyValueDisplay;
    private MPButton buttonRestoreDefault;
    private MPComboBox comboBoxFrameSize;
    private MPLabel labelFrameSize;
    private MPLabel labelFrameRate;
    private MPComboBox comboBoxFrameRate;
    private MPComboBox comboBoxAnalogVideoStandard;
    private MPLabel labelAnalogVideoStandard;
    private MPLabel labelVideoOrCameraProperty;
    private MPComboBox comboBoxVideoOrCameraProperty;
    private MPTextBox textBoxConditionalAccessProviders;
    private MPLabel labelConditionalAccessProviders;
    private MPLabel labelVideoOrCameraPropertyValue;
    private MPButton buttonRestoreAllDefaults;
    private MPGroupBox groupBoxSoftwareEncoders;
    private MPComboBox comboBoxSoftwareEncoderAudio;
    private MPLabel labelSoftwareEncoderAudio;
    private MPLabel labelSoftwareEncoderVideo;
    private MPComboBox comboBoxSoftwareEncoderVideo;
    private MPGroupBox groupBoxVideo;
    private MPGroupBox groupBoxEncoderSettings;
    private MPLabel labelEncoderBitRateModeTimeShifting;
    private MPComboBox comboBoxEncoderBitRateModeTimeShifting;
    private MPLabel labelEncoderSettingsRecording;
    private MPNumericUpDown numericUpDownEncoderBitRateValuePeakRecording;
    private MPLabel labelEncoderBitRateValuePeakUnitRecording;
    private MPLabel labelEncoderBitRateValuePeakRecording;
    private MPNumericUpDown numericUpDownEncoderBitRateValueRecording;
    private MPLabel labelEncoderBitRateValueUnitRecording;
    private MPComboBox comboBoxEncoderBitRateModeRecording;
    private MPLabel labelEncoderBitRateModeRecording;
    private MPLabel labelEncoderBitRateValueRecording;
    private MPLabel labelEncoderSettingsTimeShifting;
    private MPNumericUpDown numericUpDownEncoderBitRateValuePeakTimeShifting;
    private MPLabel labelEncoderBitRateValuePeakUnitTimeShifting;
    private MPLabel labelEncoderBitRateValuePeakTimeShifting;
    private MPNumericUpDown numericUpDownEncoderBitRateValueTimeShifting;
    private MPLabel labelEncoderBitRateValueUnitTimeShifting;
    private MPLabel labelEncoderBitRateValueTimeShifting;
    private MPCheckBox checkBoxAlwaysSendDiseqcCommands;
    private MPCheckBox checkBoxVideoOrCameraPropertyValue;
    private MPTabPage tabPageExternalInput;
    private MPGroupBox groupBoxExternalInput;
    private MPLabel labelExternalInputPhysicalChannelNumber;
    private MPLabel labelExternalInputCountry;
    private MPComboBox comboBoxExternalInputCountry;
    private MPComboBox comboBoxExternalInputSourceAudio;
    private MPLabel labelExternalInputSourceAudio;
    private MPLabel labelExternalInputSourceVideo;
    private MPComboBox comboBoxExternalInputSourceVideo;
    private MPTextBox textBoxExternalTunerProgram;
    private MPNumericUpDown numericUpDownExternalInputPhysicalChannelNumber;
    private MPLabel labelExternalTunerProgram;
    private MPButton buttonExternalTunerProgramBrowse;
    private MPLabel labelExternalTunerProgramArguments;
    private MPTextBox textBoxExternalTunerProgramArguments;
    private System.Windows.Forms.OpenFileDialog openFileDialogExternalTunerProgram;
    private MPGroupBox groupBoxExternalTuner;
    private MPButton buttonEncoderSettingsCheckSupport;
  }
}
