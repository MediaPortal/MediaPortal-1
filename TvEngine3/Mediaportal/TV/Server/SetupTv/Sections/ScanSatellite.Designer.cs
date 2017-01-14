using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class ScanSatellite
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
      this.buttonScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.comboBoxTransmitter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelTransmitter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxAdvancedOptions = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.groupBoxManualTuning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.comboBoxBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.numericTextBoxInputStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.comboBoxPolarisation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelInputStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelPolarisation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxPilotTonesState = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelPilotTonesState = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSymbolRateUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxRollOffFactor = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelModulation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelRollOffFactor = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxModulation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.comboBoxFecCodeRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.numericTextBoxSymbolRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelFecCodeRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSymbolRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelFrequencyUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxUseManualTuning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelScanType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxScanType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.groupBoxProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.progressBarProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.labelSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.progressBarSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.progressBarSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.listViewProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderStatus = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.comboBoxSatellite = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelSatellite = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxUseAdvancedOptions = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.groupBoxAdvancedOptions.SuspendLayout();
      this.groupBoxManualTuning.SuspendLayout();
      this.groupBoxProgress.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(362, 60);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(110, 23);
      this.buttonScan.TabIndex = 4;
      this.buttonScan.Text = "&Scan for channels";
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // comboBoxTransmitter
      // 
      this.comboBoxTransmitter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxTransmitter.FormattingEnabled = true;
      this.comboBoxTransmitter.Location = new System.Drawing.Point(100, 33);
      this.comboBoxTransmitter.Name = "comboBoxTransmitter";
      this.comboBoxTransmitter.Size = new System.Drawing.Size(372, 21);
      this.comboBoxTransmitter.TabIndex = 3;
      this.comboBoxTransmitter.SelectedIndexChanged += new System.EventHandler(this.comboBoxTransmitter_SelectedIndexChanged);
      // 
      // labelTransmitter
      // 
      this.labelTransmitter.AutoSize = true;
      this.labelTransmitter.Location = new System.Drawing.Point(6, 36);
      this.labelTransmitter.Name = "labelTransmitter";
      this.labelTransmitter.Size = new System.Drawing.Size(62, 13);
      this.labelTransmitter.TabIndex = 2;
      this.labelTransmitter.Text = "Transmitter:";
      // 
      // groupBoxAdvancedOptions
      // 
      this.groupBoxAdvancedOptions.Controls.Add(this.groupBoxManualTuning);
      this.groupBoxAdvancedOptions.Controls.Add(this.checkBoxUseManualTuning);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelScanType);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxScanType);
      this.groupBoxAdvancedOptions.Location = new System.Drawing.Point(2, 167);
      this.groupBoxAdvancedOptions.Name = "groupBoxAdvancedOptions";
      this.groupBoxAdvancedOptions.Size = new System.Drawing.Size(476, 233);
      this.groupBoxAdvancedOptions.TabIndex = 6;
      this.groupBoxAdvancedOptions.TabStop = false;
      this.groupBoxAdvancedOptions.Text = "Advanced Options";
      this.groupBoxAdvancedOptions.Visible = false;
      // 
      // groupBoxManualTuning
      // 
      this.groupBoxManualTuning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxManualTuning.Controls.Add(this.comboBoxBroadcastStandard);
      this.groupBoxManualTuning.Controls.Add(this.numericTextBoxInputStreamId);
      this.groupBoxManualTuning.Controls.Add(this.comboBoxPolarisation);
      this.groupBoxManualTuning.Controls.Add(this.labelInputStreamId);
      this.groupBoxManualTuning.Controls.Add(this.labelPolarisation);
      this.groupBoxManualTuning.Controls.Add(this.comboBoxPilotTonesState);
      this.groupBoxManualTuning.Controls.Add(this.labelPilotTonesState);
      this.groupBoxManualTuning.Controls.Add(this.labelSymbolRateUnit);
      this.groupBoxManualTuning.Controls.Add(this.comboBoxRollOffFactor);
      this.groupBoxManualTuning.Controls.Add(this.labelModulation);
      this.groupBoxManualTuning.Controls.Add(this.labelRollOffFactor);
      this.groupBoxManualTuning.Controls.Add(this.comboBoxModulation);
      this.groupBoxManualTuning.Controls.Add(this.comboBoxFecCodeRate);
      this.groupBoxManualTuning.Controls.Add(this.numericTextBoxSymbolRate);
      this.groupBoxManualTuning.Controls.Add(this.labelFecCodeRate);
      this.groupBoxManualTuning.Controls.Add(this.labelSymbolRate);
      this.groupBoxManualTuning.Controls.Add(this.labelBroadcastStandard);
      this.groupBoxManualTuning.Controls.Add(this.labelFrequency);
      this.groupBoxManualTuning.Controls.Add(this.numericTextBoxFrequency);
      this.groupBoxManualTuning.Controls.Add(this.labelFrequencyUnit);
      this.groupBoxManualTuning.Enabled = false;
      this.groupBoxManualTuning.Location = new System.Drawing.Point(7, 69);
      this.groupBoxManualTuning.Name = "groupBoxManualTuning";
      this.groupBoxManualTuning.Size = new System.Drawing.Size(463, 156);
      this.groupBoxManualTuning.TabIndex = 3;
      this.groupBoxManualTuning.TabStop = false;
      this.groupBoxManualTuning.Text = "Manual Tuning";
      // 
      // comboBoxBroadcastStandard
      // 
      this.comboBoxBroadcastStandard.FormattingEnabled = true;
      this.comboBoxBroadcastStandard.Location = new System.Drawing.Point(91, 19);
      this.comboBoxBroadcastStandard.Name = "comboBoxBroadcastStandard";
      this.comboBoxBroadcastStandard.Size = new System.Drawing.Size(87, 21);
      this.comboBoxBroadcastStandard.TabIndex = 1;
      this.comboBoxBroadcastStandard.SelectedIndexChanged += new System.EventHandler(this.comboBoxBroadcastStandard_SelectedIndexChanged);
      // 
      // numericTextBoxInputStreamId
      // 
      this.numericTextBoxInputStreamId.Location = new System.Drawing.Point(353, 127);
      this.numericTextBoxInputStreamId.MaximumValue = 255;
      this.numericTextBoxInputStreamId.MaxLength = 3;
      this.numericTextBoxInputStreamId.MinimumValue = -1;
      this.numericTextBoxInputStreamId.Name = "numericTextBoxInputStreamId";
      this.numericTextBoxInputStreamId.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxInputStreamId.TabIndex = 19;
      this.numericTextBoxInputStreamId.Text = "-1";
      this.numericTextBoxInputStreamId.Value = -1;
      // 
      // comboBoxPolarisation
      // 
      this.comboBoxPolarisation.FormattingEnabled = true;
      this.comboBoxPolarisation.Location = new System.Drawing.Point(91, 73);
      this.comboBoxPolarisation.Name = "comboBoxPolarisation";
      this.comboBoxPolarisation.Size = new System.Drawing.Size(155, 21);
      this.comboBoxPolarisation.TabIndex = 6;
      // 
      // labelInputStreamId
      // 
      this.labelInputStreamId.AutoSize = true;
      this.labelInputStreamId.Location = new System.Drawing.Point(262, 130);
      this.labelInputStreamId.Name = "labelInputStreamId";
      this.labelInputStreamId.Size = new System.Drawing.Size(82, 13);
      this.labelInputStreamId.TabIndex = 18;
      this.labelInputStreamId.Text = "Input stream ID:";
      // 
      // labelPolarisation
      // 
      this.labelPolarisation.AutoSize = true;
      this.labelPolarisation.Location = new System.Drawing.Point(6, 76);
      this.labelPolarisation.Name = "labelPolarisation";
      this.labelPolarisation.Size = new System.Drawing.Size(64, 13);
      this.labelPolarisation.TabIndex = 5;
      this.labelPolarisation.Text = "Polarisation:";
      // 
      // comboBoxPilotTonesState
      // 
      this.comboBoxPilotTonesState.FormattingEnabled = true;
      this.comboBoxPilotTonesState.Location = new System.Drawing.Point(353, 100);
      this.comboBoxPilotTonesState.Name = "comboBoxPilotTonesState";
      this.comboBoxPilotTonesState.Size = new System.Drawing.Size(95, 21);
      this.comboBoxPilotTonesState.TabIndex = 17;
      // 
      // labelPilotTonesState
      // 
      this.labelPilotTonesState.AutoSize = true;
      this.labelPilotTonesState.Location = new System.Drawing.Point(262, 103);
      this.labelPilotTonesState.Name = "labelPilotTonesState";
      this.labelPilotTonesState.Size = new System.Drawing.Size(85, 13);
      this.labelPilotTonesState.TabIndex = 16;
      this.labelPilotTonesState.Text = "Pilot tones state:";
      // 
      // labelSymbolRateUnit
      // 
      this.labelSymbolRateUnit.AutoSize = true;
      this.labelSymbolRateUnit.Location = new System.Drawing.Point(148, 130);
      this.labelSymbolRateUnit.Name = "labelSymbolRateUnit";
      this.labelSymbolRateUnit.Size = new System.Drawing.Size(28, 13);
      this.labelSymbolRateUnit.TabIndex = 11;
      this.labelSymbolRateUnit.Text = "ks/s";
      // 
      // comboBoxRollOffFactor
      // 
      this.comboBoxRollOffFactor.FormattingEnabled = true;
      this.comboBoxRollOffFactor.Location = new System.Drawing.Point(353, 73);
      this.comboBoxRollOffFactor.Name = "comboBoxRollOffFactor";
      this.comboBoxRollOffFactor.Size = new System.Drawing.Size(95, 21);
      this.comboBoxRollOffFactor.TabIndex = 15;
      // 
      // labelModulation
      // 
      this.labelModulation.AutoSize = true;
      this.labelModulation.Location = new System.Drawing.Point(6, 103);
      this.labelModulation.Name = "labelModulation";
      this.labelModulation.Size = new System.Drawing.Size(62, 13);
      this.labelModulation.TabIndex = 7;
      this.labelModulation.Text = "Modulation:";
      // 
      // labelRollOffFactor
      // 
      this.labelRollOffFactor.AutoSize = true;
      this.labelRollOffFactor.Location = new System.Drawing.Point(262, 76);
      this.labelRollOffFactor.Name = "labelRollOffFactor";
      this.labelRollOffFactor.Size = new System.Drawing.Size(73, 13);
      this.labelRollOffFactor.TabIndex = 14;
      this.labelRollOffFactor.Text = "Roll-off factor:";
      // 
      // comboBoxModulation
      // 
      this.comboBoxModulation.FormattingEnabled = true;
      this.comboBoxModulation.Location = new System.Drawing.Point(91, 100);
      this.comboBoxModulation.Name = "comboBoxModulation";
      this.comboBoxModulation.Size = new System.Drawing.Size(155, 21);
      this.comboBoxModulation.TabIndex = 8;
      this.comboBoxModulation.SelectedIndexChanged += new System.EventHandler(this.comboBoxModulation_SelectedIndexChanged);
      // 
      // comboBoxFecCodeRate
      // 
      this.comboBoxFecCodeRate.FormattingEnabled = true;
      this.comboBoxFecCodeRate.Location = new System.Drawing.Point(353, 46);
      this.comboBoxFecCodeRate.Name = "comboBoxFecCodeRate";
      this.comboBoxFecCodeRate.Size = new System.Drawing.Size(95, 21);
      this.comboBoxFecCodeRate.TabIndex = 13;
      // 
      // numericTextBoxSymbolRate
      // 
      this.numericTextBoxSymbolRate.Location = new System.Drawing.Point(91, 127);
      this.numericTextBoxSymbolRate.MaximumValue = 99999;
      this.numericTextBoxSymbolRate.MaxLength = 5;
      this.numericTextBoxSymbolRate.MinimumValue = 100;
      this.numericTextBoxSymbolRate.Name = "numericTextBoxSymbolRate";
      this.numericTextBoxSymbolRate.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxSymbolRate.TabIndex = 10;
      this.numericTextBoxSymbolRate.Text = "25000";
      this.numericTextBoxSymbolRate.Value = 25000;
      // 
      // labelFecCodeRate
      // 
      this.labelFecCodeRate.AutoSize = true;
      this.labelFecCodeRate.Location = new System.Drawing.Point(262, 49);
      this.labelFecCodeRate.Name = "labelFecCodeRate";
      this.labelFecCodeRate.Size = new System.Drawing.Size(78, 13);
      this.labelFecCodeRate.TabIndex = 12;
      this.labelFecCodeRate.Text = "FEC code rate:";
      // 
      // labelSymbolRate
      // 
      this.labelSymbolRate.AutoSize = true;
      this.labelSymbolRate.Location = new System.Drawing.Point(6, 130);
      this.labelSymbolRate.Name = "labelSymbolRate";
      this.labelSymbolRate.Size = new System.Drawing.Size(65, 13);
      this.labelSymbolRate.TabIndex = 9;
      this.labelSymbolRate.Text = "Symbol rate:";
      // 
      // labelBroadcastStandard
      // 
      this.labelBroadcastStandard.AutoSize = true;
      this.labelBroadcastStandard.Location = new System.Drawing.Point(6, 22);
      this.labelBroadcastStandard.Name = "labelBroadcastStandard";
      this.labelBroadcastStandard.Size = new System.Drawing.Size(53, 13);
      this.labelBroadcastStandard.TabIndex = 0;
      this.labelBroadcastStandard.Text = "Standard:";
      // 
      // labelFrequency
      // 
      this.labelFrequency.AutoSize = true;
      this.labelFrequency.Location = new System.Drawing.Point(6, 49);
      this.labelFrequency.Name = "labelFrequency";
      this.labelFrequency.Size = new System.Drawing.Size(60, 13);
      this.labelFrequency.TabIndex = 2;
      this.labelFrequency.Text = "Frequency:";
      // 
      // numericTextBoxFrequency
      // 
      this.numericTextBoxFrequency.Location = new System.Drawing.Point(91, 46);
      this.numericTextBoxFrequency.MaximumValue = 22000000;
      this.numericTextBoxFrequency.MaxLength = 8;
      this.numericTextBoxFrequency.MinimumValue = 3400000;
      this.numericTextBoxFrequency.Name = "numericTextBoxFrequency";
      this.numericTextBoxFrequency.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxFrequency.TabIndex = 3;
      this.numericTextBoxFrequency.Text = "11097000";
      this.numericTextBoxFrequency.Value = 11097000;
      // 
      // labelFrequencyUnit
      // 
      this.labelFrequencyUnit.AutoSize = true;
      this.labelFrequencyUnit.Location = new System.Drawing.Point(148, 49);
      this.labelFrequencyUnit.Name = "labelFrequencyUnit";
      this.labelFrequencyUnit.Size = new System.Drawing.Size(26, 13);
      this.labelFrequencyUnit.TabIndex = 4;
      this.labelFrequencyUnit.Text = "kHz";
      // 
      // checkBoxUseManualTuning
      // 
      this.checkBoxUseManualTuning.AutoSize = true;
      this.checkBoxUseManualTuning.Location = new System.Drawing.Point(7, 46);
      this.checkBoxUseManualTuning.Name = "checkBoxUseManualTuning";
      this.checkBoxUseManualTuning.Size = new System.Drawing.Size(115, 17);
      this.checkBoxUseManualTuning.TabIndex = 2;
      this.checkBoxUseManualTuning.Text = "Use manual tuning.";
      this.checkBoxUseManualTuning.CheckedChanged += new System.EventHandler(this.checkBoxUseManualTuning_CheckedChanged);
      // 
      // labelScanType
      // 
      this.labelScanType.AutoSize = true;
      this.labelScanType.Location = new System.Drawing.Point(4, 22);
      this.labelScanType.Name = "labelScanType";
      this.labelScanType.Size = new System.Drawing.Size(58, 13);
      this.labelScanType.TabIndex = 0;
      this.labelScanType.Text = "Scan type:";
      // 
      // comboBoxScanType
      // 
      this.comboBoxScanType.FormattingEnabled = true;
      this.comboBoxScanType.Location = new System.Drawing.Point(98, 19);
      this.comboBoxScanType.Name = "comboBoxScanType";
      this.comboBoxScanType.Size = new System.Drawing.Size(155, 21);
      this.comboBoxScanType.TabIndex = 1;
      // 
      // groupBoxProgress
      // 
      this.groupBoxProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxProgress.Controls.Add(this.progressBarProgress);
      this.groupBoxProgress.Controls.Add(this.labelSignalStrength);
      this.groupBoxProgress.Controls.Add(this.labelSignalQuality);
      this.groupBoxProgress.Controls.Add(this.progressBarSignalStrength);
      this.groupBoxProgress.Controls.Add(this.progressBarSignalQuality);
      this.groupBoxProgress.Controls.Add(this.listViewProgress);
      this.groupBoxProgress.Location = new System.Drawing.Point(2, 110);
      this.groupBoxProgress.Name = "groupBoxProgress";
      this.groupBoxProgress.Size = new System.Drawing.Size(476, 305);
      this.groupBoxProgress.TabIndex = 7;
      this.groupBoxProgress.TabStop = false;
      this.groupBoxProgress.Text = "Progress";
      this.groupBoxProgress.Visible = false;
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarProgress.Location = new System.Drawing.Point(7, 51);
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(463, 10);
      this.progressBarProgress.TabIndex = 4;
      // 
      // labelSignalStrength
      // 
      this.labelSignalStrength.AutoSize = true;
      this.labelSignalStrength.Location = new System.Drawing.Point(4, 16);
      this.labelSignalStrength.Name = "labelSignalStrength";
      this.labelSignalStrength.Size = new System.Drawing.Size(80, 13);
      this.labelSignalStrength.TabIndex = 0;
      this.labelSignalStrength.Text = "Signal strength:";
      // 
      // labelSignalQuality
      // 
      this.labelSignalQuality.AutoSize = true;
      this.labelSignalQuality.Location = new System.Drawing.Point(4, 32);
      this.labelSignalQuality.Name = "labelSignalQuality";
      this.labelSignalQuality.Size = new System.Drawing.Size(72, 13);
      this.labelSignalQuality.TabIndex = 2;
      this.labelSignalQuality.Text = "Signal quality:";
      // 
      // progressBarSignalStrength
      // 
      this.progressBarSignalStrength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalStrength.Location = new System.Drawing.Point(98, 19);
      this.progressBarSignalStrength.Name = "progressBarSignalStrength";
      this.progressBarSignalStrength.Size = new System.Drawing.Size(372, 10);
      this.progressBarSignalStrength.TabIndex = 1;
      // 
      // progressBarSignalQuality
      // 
      this.progressBarSignalQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalQuality.Location = new System.Drawing.Point(98, 35);
      this.progressBarSignalQuality.Name = "progressBarSignalQuality";
      this.progressBarSignalQuality.Size = new System.Drawing.Size(372, 10);
      this.progressBarSignalQuality.TabIndex = 3;
      // 
      // listViewProgress
      // 
      this.listViewProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewProgress.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStatus});
      this.listViewProgress.Location = new System.Drawing.Point(7, 67);
      this.listViewProgress.Name = "listViewProgress";
      this.listViewProgress.Size = new System.Drawing.Size(463, 232);
      this.listViewProgress.TabIndex = 5;
      this.listViewProgress.UseCompatibleStateImageBehavior = false;
      this.listViewProgress.View = System.Windows.Forms.View.Details;
      // 
      // columnHeaderStatus
      // 
      this.columnHeaderStatus.Text = "Status";
      this.columnHeaderStatus.Width = 436;
      // 
      // comboBoxSatellite
      // 
      this.comboBoxSatellite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxSatellite.FormattingEnabled = true;
      this.comboBoxSatellite.Location = new System.Drawing.Point(100, 6);
      this.comboBoxSatellite.Name = "comboBoxSatellite";
      this.comboBoxSatellite.Size = new System.Drawing.Size(372, 21);
      this.comboBoxSatellite.TabIndex = 1;
      this.comboBoxSatellite.SelectedIndexChanged += new System.EventHandler(this.comboBoxSatellite_SelectedIndexChanged);
      // 
      // labelSatellite
      // 
      this.labelSatellite.AutoSize = true;
      this.labelSatellite.Location = new System.Drawing.Point(6, 9);
      this.labelSatellite.Name = "labelSatellite";
      this.labelSatellite.Size = new System.Drawing.Size(47, 13);
      this.labelSatellite.TabIndex = 0;
      this.labelSatellite.Text = "Satellite:";
      // 
      // checkBoxUseAdvancedOptions
      // 
      this.checkBoxUseAdvancedOptions.AutoSize = true;
      this.checkBoxUseAdvancedOptions.Location = new System.Drawing.Point(9, 87);
      this.checkBoxUseAdvancedOptions.Name = "checkBoxUseAdvancedOptions";
      this.checkBoxUseAdvancedOptions.Size = new System.Drawing.Size(134, 17);
      this.checkBoxUseAdvancedOptions.TabIndex = 5;
      this.checkBoxUseAdvancedOptions.Text = "Use advanced options.";
      this.checkBoxUseAdvancedOptions.CheckedChanged += new System.EventHandler(this.checkBoxUseAdvancedOptions_CheckedChanged);
      // 
      // ScanSatellite
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.comboBoxTransmitter);
      this.Controls.Add(this.labelTransmitter);
      this.Controls.Add(this.groupBoxAdvancedOptions);
      this.Controls.Add(this.groupBoxProgress);
      this.Controls.Add(this.comboBoxSatellite);
      this.Controls.Add(this.labelSatellite);
      this.Controls.Add(this.checkBoxUseAdvancedOptions);
      this.Controls.Add(this.buttonScan);
      this.Name = "ScanSatellite";
      this.Size = new System.Drawing.Size(480, 420);
      this.groupBoxAdvancedOptions.ResumeLayout(false);
      this.groupBoxAdvancedOptions.PerformLayout();
      this.groupBoxManualTuning.ResumeLayout(false);
      this.groupBoxManualTuning.PerformLayout();
      this.groupBoxProgress.ResumeLayout(false);
      this.groupBoxProgress.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPButton buttonScan;
    private MPComboBox comboBoxPolarisation;
    private MPLabel labelPolarisation;
    private MPGroupBox groupBoxAdvancedOptions;
    private MPCheckBox checkBoxUseAdvancedOptions;
    private MPComboBox comboBoxSatellite;
    private MPLabel labelSatellite;
    private MPComboBox comboBoxPilotTonesState;
    private MPLabel labelRollOffFactor;
    private MPComboBox comboBoxRollOffFactor;
    private MPLabel labelPilotTonesState;
    private MPLabel labelFecCodeRate;
    private MPComboBox comboBoxFecCodeRate;
    private MPGroupBox groupBoxProgress;
    private MPProgressBar progressBarProgress;
    private MPLabel labelSignalStrength;
    private MPLabel labelSignalQuality;
    private MPProgressBar progressBarSignalStrength;
    private MPProgressBar progressBarSignalQuality;
    private MPListView listViewProgress;
    private MPColumnHeader columnHeaderStatus;
    private MPLabel labelFrequency;
    private MPLabel labelFrequencyUnit;
    private MPNumericTextBox numericTextBoxFrequency;
    private MPLabel labelBroadcastStandard;
    private MPComboBox comboBoxBroadcastStandard;
    private MPLabel labelSymbolRateUnit;
    private MPLabel labelModulation;
    private MPComboBox comboBoxModulation;
    private MPNumericTextBox numericTextBoxSymbolRate;
    private MPLabel labelSymbolRate;
    private MPLabel labelScanType;
    private MPComboBox comboBoxScanType;
    private MPNumericTextBox numericTextBoxInputStreamId;
    private MPLabel labelInputStreamId;
    private MPGroupBox groupBoxManualTuning;
    private MPCheckBox checkBoxUseManualTuning;
    private MPComboBox comboBoxTransmitter;
    private MPLabel labelTransmitter;
  }
}
