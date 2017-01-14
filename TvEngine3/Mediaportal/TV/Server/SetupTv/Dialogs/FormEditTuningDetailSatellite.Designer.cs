using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEditTuningDetailSatellite
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
      this.comboBoxRollOffFactor = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelRollOffFactor = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxPilotTonesState = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelPilotTonesState = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxFecCodeRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelFecCodeRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxPolarisation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelPolarisation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxInputStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelInputStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxPmtPid = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelOriginalNetworkId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxServiceId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxTransportStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxOriginalNetworkId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelPmtPid = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTransportStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelServiceId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSymbolRateUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequencyUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxModulation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelModulation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxSymbolRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelSymbolRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxSatellite = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelSatellite = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxEpgSource = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelEpgOriginalNetworkId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxEpgServiceId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxEpgTransportStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxEpgOriginalNetworkId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelEpgTransportStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelEpgServiceId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxFreesatChannelId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelFreesatChannelId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxOpenTvChannelId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelOpenTvChannelId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).BeginInit();
      this.groupBoxEpgSource.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(338, 438);
      this.buttonCancel.TabIndex = 48;
      // 
      // buttonOkay
      // 
      this.buttonOkay.Location = new System.Drawing.Point(243, 438);
      this.buttonOkay.TabIndex = 47;
      // 
      // labelNumber
      // 
      this.labelNumber.Location = new System.Drawing.Point(12, 40);
      // 
      // channelNumberUpDownNumber
      // 
      this.channelNumberUpDownNumber.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      // 
      // textBoxName
      // 
      this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxName.MinimumSize = new System.Drawing.Size(105, 4);
      this.textBoxName.Size = new System.Drawing.Size(105, 20);
      // 
      // checkBoxIsEncrypted
      // 
      this.checkBoxIsEncrypted.Location = new System.Drawing.Point(123, 91);
      // 
      // textBoxProvider
      // 
      this.textBoxProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxProvider.MinimumSize = new System.Drawing.Size(105, 4);
      this.textBoxProvider.Size = new System.Drawing.Size(105, 20);
      // 
      // labelIsEncrypted
      // 
      this.labelIsEncrypted.Location = new System.Drawing.Point(12, 93);
      // 
      // labelIsHighDefinition
      // 
      this.labelIsHighDefinition.Location = new System.Drawing.Point(12, 119);
      // 
      // checkBoxIsHighDefinition
      // 
      this.checkBoxIsHighDefinition.Location = new System.Drawing.Point(123, 117);
      // 
      // labelIsThreeDimensional
      // 
      this.labelIsThreeDimensional.Location = new System.Drawing.Point(12, 145);
      // 
      // checkBoxIsThreeDimensional
      // 
      this.checkBoxIsThreeDimensional.Location = new System.Drawing.Point(123, 143);
      // 
      // comboBoxRollOffFactor
      // 
      this.comboBoxRollOffFactor.FormattingEnabled = true;
      this.comboBoxRollOffFactor.Location = new System.Drawing.Point(123, 353);
      this.comboBoxRollOffFactor.Name = "comboBoxRollOffFactor";
      this.comboBoxRollOffFactor.Size = new System.Drawing.Size(105, 21);
      this.comboBoxRollOffFactor.TabIndex = 29;
      // 
      // labelRollOffFactor
      // 
      this.labelRollOffFactor.AutoSize = true;
      this.labelRollOffFactor.Location = new System.Drawing.Point(12, 356);
      this.labelRollOffFactor.Name = "labelRollOffFactor";
      this.labelRollOffFactor.Size = new System.Drawing.Size(73, 13);
      this.labelRollOffFactor.TabIndex = 28;
      this.labelRollOffFactor.Text = "Roll-off factor:";
      // 
      // comboBoxPilotTonesState
      // 
      this.comboBoxPilotTonesState.FormattingEnabled = true;
      this.comboBoxPilotTonesState.Location = new System.Drawing.Point(123, 380);
      this.comboBoxPilotTonesState.Name = "comboBoxPilotTonesState";
      this.comboBoxPilotTonesState.Size = new System.Drawing.Size(105, 21);
      this.comboBoxPilotTonesState.TabIndex = 31;
      // 
      // labelPilotTonesState
      // 
      this.labelPilotTonesState.AutoSize = true;
      this.labelPilotTonesState.Location = new System.Drawing.Point(12, 383);
      this.labelPilotTonesState.Name = "labelPilotTonesState";
      this.labelPilotTonesState.Size = new System.Drawing.Size(85, 13);
      this.labelPilotTonesState.TabIndex = 30;
      this.labelPilotTonesState.Text = "Pilot tones state:";
      // 
      // comboBoxFecCodeRate
      // 
      this.comboBoxFecCodeRate.FormattingEnabled = true;
      this.comboBoxFecCodeRate.Location = new System.Drawing.Point(123, 326);
      this.comboBoxFecCodeRate.Name = "comboBoxFecCodeRate";
      this.comboBoxFecCodeRate.Size = new System.Drawing.Size(105, 21);
      this.comboBoxFecCodeRate.TabIndex = 27;
      // 
      // labelFecCodeRate
      // 
      this.labelFecCodeRate.AutoSize = true;
      this.labelFecCodeRate.Location = new System.Drawing.Point(12, 329);
      this.labelFecCodeRate.Name = "labelFecCodeRate";
      this.labelFecCodeRate.Size = new System.Drawing.Size(78, 13);
      this.labelFecCodeRate.TabIndex = 26;
      this.labelFecCodeRate.Text = "FEC code rate:";
      // 
      // comboBoxPolarisation
      // 
      this.comboBoxPolarisation.FormattingEnabled = true;
      this.comboBoxPolarisation.Location = new System.Drawing.Point(123, 246);
      this.comboBoxPolarisation.Name = "comboBoxPolarisation";
      this.comboBoxPolarisation.Size = new System.Drawing.Size(105, 21);
      this.comboBoxPolarisation.TabIndex = 20;
      // 
      // labelPolarisation
      // 
      this.labelPolarisation.AutoSize = true;
      this.labelPolarisation.Location = new System.Drawing.Point(12, 249);
      this.labelPolarisation.Name = "labelPolarisation";
      this.labelPolarisation.Size = new System.Drawing.Size(64, 13);
      this.labelPolarisation.TabIndex = 19;
      this.labelPolarisation.Text = "Polarisation:";
      // 
      // numericTextBoxInputStreamId
      // 
      this.numericTextBoxInputStreamId.Location = new System.Drawing.Point(123, 407);
      this.numericTextBoxInputStreamId.MaximumValue = 255;
      this.numericTextBoxInputStreamId.MaxLength = 3;
      this.numericTextBoxInputStreamId.MinimumValue = -1;
      this.numericTextBoxInputStreamId.Name = "numericTextBoxInputStreamId";
      this.numericTextBoxInputStreamId.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxInputStreamId.TabIndex = 33;
      this.numericTextBoxInputStreamId.Text = "-1";
      this.numericTextBoxInputStreamId.Value = -1;
      // 
      // labelInputStreamId
      // 
      this.labelInputStreamId.AutoSize = true;
      this.labelInputStreamId.Location = new System.Drawing.Point(12, 410);
      this.labelInputStreamId.Name = "labelInputStreamId";
      this.labelInputStreamId.Size = new System.Drawing.Size(82, 13);
      this.labelInputStreamId.TabIndex = 32;
      this.labelInputStreamId.Text = "Input stream ID:";
      // 
      // numericTextBoxPmtPid
      // 
      this.numericTextBoxPmtPid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxPmtPid.Location = new System.Drawing.Point(364, 142);
      this.numericTextBoxPmtPid.MaximumValue = 65535;
      this.numericTextBoxPmtPid.MaxLength = 5;
      this.numericTextBoxPmtPid.MinimumValue = 0;
      this.numericTextBoxPmtPid.Name = "numericTextBoxPmtPid";
      this.numericTextBoxPmtPid.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxPmtPid.TabIndex = 45;
      this.numericTextBoxPmtPid.Text = "0";
      this.numericTextBoxPmtPid.Value = 0;
      // 
      // labelOriginalNetworkId
      // 
      this.labelOriginalNetworkId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelOriginalNetworkId.AutoSize = true;
      this.labelOriginalNetworkId.Location = new System.Drawing.Point(255, 15);
      this.labelOriginalNetworkId.Name = "labelOriginalNetworkId";
      this.labelOriginalNetworkId.Size = new System.Drawing.Size(100, 13);
      this.labelOriginalNetworkId.TabIndex = 34;
      this.labelOriginalNetworkId.Text = "Original network ID:";
      // 
      // numericTextBoxServiceId
      // 
      this.numericTextBoxServiceId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxServiceId.Location = new System.Drawing.Point(364, 64);
      this.numericTextBoxServiceId.MaximumValue = 65535;
      this.numericTextBoxServiceId.MaxLength = 5;
      this.numericTextBoxServiceId.MinimumValue = 0;
      this.numericTextBoxServiceId.Name = "numericTextBoxServiceId";
      this.numericTextBoxServiceId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxServiceId.TabIndex = 39;
      this.numericTextBoxServiceId.Text = "0";
      this.numericTextBoxServiceId.Value = 0;
      // 
      // numericTextBoxTransportStreamId
      // 
      this.numericTextBoxTransportStreamId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxTransportStreamId.Location = new System.Drawing.Point(364, 37);
      this.numericTextBoxTransportStreamId.MaximumValue = 65535;
      this.numericTextBoxTransportStreamId.MaxLength = 5;
      this.numericTextBoxTransportStreamId.MinimumValue = 0;
      this.numericTextBoxTransportStreamId.Name = "numericTextBoxTransportStreamId";
      this.numericTextBoxTransportStreamId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxTransportStreamId.TabIndex = 37;
      this.numericTextBoxTransportStreamId.Text = "0";
      this.numericTextBoxTransportStreamId.Value = 0;
      // 
      // numericTextBoxOriginalNetworkId
      // 
      this.numericTextBoxOriginalNetworkId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxOriginalNetworkId.Location = new System.Drawing.Point(364, 12);
      this.numericTextBoxOriginalNetworkId.MaximumValue = 65535;
      this.numericTextBoxOriginalNetworkId.MaxLength = 5;
      this.numericTextBoxOriginalNetworkId.MinimumValue = 0;
      this.numericTextBoxOriginalNetworkId.Name = "numericTextBoxOriginalNetworkId";
      this.numericTextBoxOriginalNetworkId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxOriginalNetworkId.TabIndex = 35;
      this.numericTextBoxOriginalNetworkId.Text = "0";
      this.numericTextBoxOriginalNetworkId.Value = 0;
      // 
      // labelPmtPid
      // 
      this.labelPmtPid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelPmtPid.AutoSize = true;
      this.labelPmtPid.Location = new System.Drawing.Point(255, 145);
      this.labelPmtPid.Name = "labelPmtPid";
      this.labelPmtPid.Size = new System.Drawing.Size(54, 13);
      this.labelPmtPid.TabIndex = 44;
      this.labelPmtPid.Text = "PMT PID:";
      // 
      // labelTransportStreamId
      // 
      this.labelTransportStreamId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelTransportStreamId.AutoSize = true;
      this.labelTransportStreamId.Location = new System.Drawing.Point(255, 40);
      this.labelTransportStreamId.Name = "labelTransportStreamId";
      this.labelTransportStreamId.Size = new System.Drawing.Size(103, 13);
      this.labelTransportStreamId.TabIndex = 36;
      this.labelTransportStreamId.Text = "Transport stream ID:";
      // 
      // labelServiceId
      // 
      this.labelServiceId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelServiceId.AutoSize = true;
      this.labelServiceId.Location = new System.Drawing.Point(255, 67);
      this.labelServiceId.Name = "labelServiceId";
      this.labelServiceId.Size = new System.Drawing.Size(60, 13);
      this.labelServiceId.TabIndex = 38;
      this.labelServiceId.Text = "Service ID:";
      // 
      // labelSymbolRateUnit
      // 
      this.labelSymbolRateUnit.AutoSize = true;
      this.labelSymbolRateUnit.Location = new System.Drawing.Point(184, 303);
      this.labelSymbolRateUnit.Name = "labelSymbolRateUnit";
      this.labelSymbolRateUnit.Size = new System.Drawing.Size(28, 13);
      this.labelSymbolRateUnit.TabIndex = 25;
      this.labelSymbolRateUnit.Text = "ks/s";
      // 
      // labelFrequencyUnit
      // 
      this.labelFrequencyUnit.AutoSize = true;
      this.labelFrequencyUnit.Location = new System.Drawing.Point(184, 223);
      this.labelFrequencyUnit.Name = "labelFrequencyUnit";
      this.labelFrequencyUnit.Size = new System.Drawing.Size(26, 13);
      this.labelFrequencyUnit.TabIndex = 18;
      this.labelFrequencyUnit.Text = "kHz";
      // 
      // comboBoxModulation
      // 
      this.comboBoxModulation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxModulation.DropDownWidth = 155;
      this.comboBoxModulation.FormattingEnabled = true;
      this.comboBoxModulation.Location = new System.Drawing.Point(123, 273);
      this.comboBoxModulation.MaximumSize = new System.Drawing.Size(155, 0);
      this.comboBoxModulation.MinimumSize = new System.Drawing.Size(105, 0);
      this.comboBoxModulation.Name = "comboBoxModulation";
      this.comboBoxModulation.Size = new System.Drawing.Size(105, 21);
      this.comboBoxModulation.TabIndex = 22;
      this.comboBoxModulation.SelectedIndexChanged += new System.EventHandler(this.comboBoxModulation_SelectedIndexChanged);
      // 
      // labelModulation
      // 
      this.labelModulation.AutoSize = true;
      this.labelModulation.Location = new System.Drawing.Point(12, 276);
      this.labelModulation.Name = "labelModulation";
      this.labelModulation.Size = new System.Drawing.Size(62, 13);
      this.labelModulation.TabIndex = 21;
      this.labelModulation.Text = "Modulation:";
      // 
      // numericTextBoxSymbolRate
      // 
      this.numericTextBoxSymbolRate.Location = new System.Drawing.Point(123, 300);
      this.numericTextBoxSymbolRate.MaximumValue = 99999;
      this.numericTextBoxSymbolRate.MaxLength = 5;
      this.numericTextBoxSymbolRate.MinimumValue = 100;
      this.numericTextBoxSymbolRate.Name = "numericTextBoxSymbolRate";
      this.numericTextBoxSymbolRate.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxSymbolRate.TabIndex = 24;
      this.numericTextBoxSymbolRate.Text = "25000";
      this.numericTextBoxSymbolRate.Value = 25000;
      // 
      // labelFrequency
      // 
      this.labelFrequency.AutoSize = true;
      this.labelFrequency.Location = new System.Drawing.Point(12, 223);
      this.labelFrequency.Name = "labelFrequency";
      this.labelFrequency.Size = new System.Drawing.Size(60, 13);
      this.labelFrequency.TabIndex = 16;
      this.labelFrequency.Text = "Frequency:";
      // 
      // numericTextBoxFrequency
      // 
      this.numericTextBoxFrequency.Location = new System.Drawing.Point(123, 220);
      this.numericTextBoxFrequency.MaximumValue = 22000000;
      this.numericTextBoxFrequency.MaxLength = 8;
      this.numericTextBoxFrequency.MinimumValue = 3400000;
      this.numericTextBoxFrequency.Name = "numericTextBoxFrequency";
      this.numericTextBoxFrequency.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxFrequency.TabIndex = 17;
      this.numericTextBoxFrequency.Text = "11097000";
      this.numericTextBoxFrequency.Value = 11097000;
      // 
      // labelSymbolRate
      // 
      this.labelSymbolRate.AutoSize = true;
      this.labelSymbolRate.Location = new System.Drawing.Point(12, 303);
      this.labelSymbolRate.Name = "labelSymbolRate";
      this.labelSymbolRate.Size = new System.Drawing.Size(65, 13);
      this.labelSymbolRate.TabIndex = 23;
      this.labelSymbolRate.Text = "Symbol rate:";
      // 
      // comboBoxSatellite
      // 
      this.comboBoxSatellite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxSatellite.FormattingEnabled = true;
      this.comboBoxSatellite.Location = new System.Drawing.Point(123, 193);
      this.comboBoxSatellite.MinimumSize = new System.Drawing.Size(105, 0);
      this.comboBoxSatellite.Name = "comboBoxSatellite";
      this.comboBoxSatellite.Size = new System.Drawing.Size(105, 21);
      this.comboBoxSatellite.TabIndex = 15;
      // 
      // labelSatellite
      // 
      this.labelSatellite.AutoSize = true;
      this.labelSatellite.Location = new System.Drawing.Point(12, 196);
      this.labelSatellite.Name = "labelSatellite";
      this.labelSatellite.Size = new System.Drawing.Size(47, 13);
      this.labelSatellite.TabIndex = 14;
      this.labelSatellite.Text = "Satellite:";
      // 
      // comboBoxBroadcastStandard
      // 
      this.comboBoxBroadcastStandard.FormattingEnabled = true;
      this.comboBoxBroadcastStandard.Location = new System.Drawing.Point(123, 166);
      this.comboBoxBroadcastStandard.Name = "comboBoxBroadcastStandard";
      this.comboBoxBroadcastStandard.Size = new System.Drawing.Size(105, 21);
      this.comboBoxBroadcastStandard.TabIndex = 13;
      this.comboBoxBroadcastStandard.SelectedIndexChanged += new System.EventHandler(this.comboBoxBroadcastStandard_SelectedIndexChanged);
      // 
      // labelBroadcastStandard
      // 
      this.labelBroadcastStandard.AutoSize = true;
      this.labelBroadcastStandard.Location = new System.Drawing.Point(12, 169);
      this.labelBroadcastStandard.Name = "labelBroadcastStandard";
      this.labelBroadcastStandard.Size = new System.Drawing.Size(102, 13);
      this.labelBroadcastStandard.TabIndex = 12;
      this.labelBroadcastStandard.Text = "Broadcast standard:";
      // 
      // groupBoxEpgSource
      // 
      this.groupBoxEpgSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxEpgSource.Controls.Add(this.labelEpgOriginalNetworkId);
      this.groupBoxEpgSource.Controls.Add(this.numericTextBoxEpgServiceId);
      this.groupBoxEpgSource.Controls.Add(this.numericTextBoxEpgTransportStreamId);
      this.groupBoxEpgSource.Controls.Add(this.numericTextBoxEpgOriginalNetworkId);
      this.groupBoxEpgSource.Controls.Add(this.labelEpgTransportStreamId);
      this.groupBoxEpgSource.Controls.Add(this.labelEpgServiceId);
      this.groupBoxEpgSource.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxEpgSource.Location = new System.Drawing.Point(246, 174);
      this.groupBoxEpgSource.Name = "groupBoxEpgSource";
      this.groupBoxEpgSource.Size = new System.Drawing.Size(167, 103);
      this.groupBoxEpgSource.TabIndex = 46;
      this.groupBoxEpgSource.TabStop = false;
      this.groupBoxEpgSource.Text = "EPG Source";
      // 
      // labelEpgOriginalNetworkId
      // 
      this.labelEpgOriginalNetworkId.AutoSize = true;
      this.labelEpgOriginalNetworkId.Location = new System.Drawing.Point(9, 22);
      this.labelEpgOriginalNetworkId.Name = "labelEpgOriginalNetworkId";
      this.labelEpgOriginalNetworkId.Size = new System.Drawing.Size(100, 13);
      this.labelEpgOriginalNetworkId.TabIndex = 0;
      this.labelEpgOriginalNetworkId.Text = "Original network ID:";
      // 
      // numericTextBoxEpgServiceId
      // 
      this.numericTextBoxEpgServiceId.Location = new System.Drawing.Point(118, 72);
      this.numericTextBoxEpgServiceId.MaximumValue = 65535;
      this.numericTextBoxEpgServiceId.MaxLength = 5;
      this.numericTextBoxEpgServiceId.MinimumValue = 0;
      this.numericTextBoxEpgServiceId.Name = "numericTextBoxEpgServiceId";
      this.numericTextBoxEpgServiceId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxEpgServiceId.TabIndex = 5;
      this.numericTextBoxEpgServiceId.Text = "0";
      this.numericTextBoxEpgServiceId.Value = 0;
      // 
      // numericTextBoxEpgTransportStreamId
      // 
      this.numericTextBoxEpgTransportStreamId.Location = new System.Drawing.Point(118, 46);
      this.numericTextBoxEpgTransportStreamId.MaximumValue = 65535;
      this.numericTextBoxEpgTransportStreamId.MaxLength = 5;
      this.numericTextBoxEpgTransportStreamId.MinimumValue = 0;
      this.numericTextBoxEpgTransportStreamId.Name = "numericTextBoxEpgTransportStreamId";
      this.numericTextBoxEpgTransportStreamId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxEpgTransportStreamId.TabIndex = 3;
      this.numericTextBoxEpgTransportStreamId.Text = "0";
      this.numericTextBoxEpgTransportStreamId.Value = 0;
      // 
      // numericTextBoxEpgOriginalNetworkId
      // 
      this.numericTextBoxEpgOriginalNetworkId.Location = new System.Drawing.Point(118, 19);
      this.numericTextBoxEpgOriginalNetworkId.MaximumValue = 65535;
      this.numericTextBoxEpgOriginalNetworkId.MaxLength = 5;
      this.numericTextBoxEpgOriginalNetworkId.MinimumValue = 0;
      this.numericTextBoxEpgOriginalNetworkId.Name = "numericTextBoxEpgOriginalNetworkId";
      this.numericTextBoxEpgOriginalNetworkId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxEpgOriginalNetworkId.TabIndex = 1;
      this.numericTextBoxEpgOriginalNetworkId.Text = "0";
      this.numericTextBoxEpgOriginalNetworkId.Value = 0;
      // 
      // labelEpgTransportStreamId
      // 
      this.labelEpgTransportStreamId.AutoSize = true;
      this.labelEpgTransportStreamId.Location = new System.Drawing.Point(9, 49);
      this.labelEpgTransportStreamId.Name = "labelEpgTransportStreamId";
      this.labelEpgTransportStreamId.Size = new System.Drawing.Size(103, 13);
      this.labelEpgTransportStreamId.TabIndex = 2;
      this.labelEpgTransportStreamId.Text = "Transport stream ID:";
      // 
      // labelEpgServiceId
      // 
      this.labelEpgServiceId.AutoSize = true;
      this.labelEpgServiceId.Location = new System.Drawing.Point(9, 75);
      this.labelEpgServiceId.Name = "labelEpgServiceId";
      this.labelEpgServiceId.Size = new System.Drawing.Size(60, 13);
      this.labelEpgServiceId.TabIndex = 4;
      this.labelEpgServiceId.Text = "Service ID:";
      // 
      // numericTextBoxFreesatChannelId
      // 
      this.numericTextBoxFreesatChannelId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxFreesatChannelId.Location = new System.Drawing.Point(364, 90);
      this.numericTextBoxFreesatChannelId.MaximumValue = 65535;
      this.numericTextBoxFreesatChannelId.MaxLength = 5;
      this.numericTextBoxFreesatChannelId.MinimumValue = 0;
      this.numericTextBoxFreesatChannelId.Name = "numericTextBoxFreesatChannelId";
      this.numericTextBoxFreesatChannelId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxFreesatChannelId.TabIndex = 41;
      this.numericTextBoxFreesatChannelId.Text = "0";
      this.numericTextBoxFreesatChannelId.Value = 0;
      // 
      // labelFreesatChannelId
      // 
      this.labelFreesatChannelId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelFreesatChannelId.AutoSize = true;
      this.labelFreesatChannelId.Location = new System.Drawing.Point(255, 93);
      this.labelFreesatChannelId.Name = "labelFreesatChannelId";
      this.labelFreesatChannelId.Size = new System.Drawing.Size(100, 13);
      this.labelFreesatChannelId.TabIndex = 40;
      this.labelFreesatChannelId.Text = "Freesat channel ID:";
      // 
      // numericTextBoxOpenTvChannelId
      // 
      this.numericTextBoxOpenTvChannelId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxOpenTvChannelId.Location = new System.Drawing.Point(364, 116);
      this.numericTextBoxOpenTvChannelId.MaximumValue = 65535;
      this.numericTextBoxOpenTvChannelId.MaxLength = 5;
      this.numericTextBoxOpenTvChannelId.MinimumValue = 0;
      this.numericTextBoxOpenTvChannelId.Name = "numericTextBoxOpenTvChannelId";
      this.numericTextBoxOpenTvChannelId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxOpenTvChannelId.TabIndex = 43;
      this.numericTextBoxOpenTvChannelId.Text = "0";
      this.numericTextBoxOpenTvChannelId.Value = 0;
      // 
      // labelOpenTvChannelId
      // 
      this.labelOpenTvChannelId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelOpenTvChannelId.AutoSize = true;
      this.labelOpenTvChannelId.Location = new System.Drawing.Point(255, 119);
      this.labelOpenTvChannelId.Name = "labelOpenTvChannelId";
      this.labelOpenTvChannelId.Size = new System.Drawing.Size(105, 13);
      this.labelOpenTvChannelId.TabIndex = 42;
      this.labelOpenTvChannelId.Text = "OpenTV channel ID:";
      // 
      // FormEditTuningDetailSatellite
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(425, 473);
      this.Controls.Add(this.numericTextBoxOpenTvChannelId);
      this.Controls.Add(this.labelOpenTvChannelId);
      this.Controls.Add(this.numericTextBoxFreesatChannelId);
      this.Controls.Add(this.labelFreesatChannelId);
      this.Controls.Add(this.groupBoxEpgSource);
      this.Controls.Add(this.comboBoxBroadcastStandard);
      this.Controls.Add(this.labelBroadcastStandard);
      this.Controls.Add(this.comboBoxSatellite);
      this.Controls.Add(this.labelSatellite);
      this.Controls.Add(this.labelSymbolRateUnit);
      this.Controls.Add(this.labelFrequencyUnit);
      this.Controls.Add(this.comboBoxModulation);
      this.Controls.Add(this.labelModulation);
      this.Controls.Add(this.numericTextBoxSymbolRate);
      this.Controls.Add(this.labelFrequency);
      this.Controls.Add(this.numericTextBoxFrequency);
      this.Controls.Add(this.labelSymbolRate);
      this.Controls.Add(this.numericTextBoxInputStreamId);
      this.Controls.Add(this.labelInputStreamId);
      this.Controls.Add(this.numericTextBoxPmtPid);
      this.Controls.Add(this.labelOriginalNetworkId);
      this.Controls.Add(this.numericTextBoxServiceId);
      this.Controls.Add(this.numericTextBoxTransportStreamId);
      this.Controls.Add(this.numericTextBoxOriginalNetworkId);
      this.Controls.Add(this.labelPmtPid);
      this.Controls.Add(this.labelTransportStreamId);
      this.Controls.Add(this.labelServiceId);
      this.Controls.Add(this.comboBoxRollOffFactor);
      this.Controls.Add(this.labelRollOffFactor);
      this.Controls.Add(this.comboBoxPilotTonesState);
      this.Controls.Add(this.labelPilotTonesState);
      this.Controls.Add(this.comboBoxFecCodeRate);
      this.Controls.Add(this.labelFecCodeRate);
      this.Controls.Add(this.comboBoxPolarisation);
      this.Controls.Add(this.labelPolarisation);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(433, 499);
      this.Name = "FormEditTuningDetailSatellite";
      this.Text = "Add/Edit Satellite Tuning Detail";
      this.Controls.SetChildIndex(this.labelPolarisation, 0);
      this.Controls.SetChildIndex(this.comboBoxPolarisation, 0);
      this.Controls.SetChildIndex(this.labelFecCodeRate, 0);
      this.Controls.SetChildIndex(this.comboBoxFecCodeRate, 0);
      this.Controls.SetChildIndex(this.labelPilotTonesState, 0);
      this.Controls.SetChildIndex(this.comboBoxPilotTonesState, 0);
      this.Controls.SetChildIndex(this.labelRollOffFactor, 0);
      this.Controls.SetChildIndex(this.comboBoxRollOffFactor, 0);
      this.Controls.SetChildIndex(this.labelServiceId, 0);
      this.Controls.SetChildIndex(this.labelTransportStreamId, 0);
      this.Controls.SetChildIndex(this.labelPmtPid, 0);
      this.Controls.SetChildIndex(this.numericTextBoxOriginalNetworkId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxTransportStreamId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxServiceId, 0);
      this.Controls.SetChildIndex(this.labelOriginalNetworkId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxPmtPid, 0);
      this.Controls.SetChildIndex(this.labelInputStreamId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxInputStreamId, 0);
      this.Controls.SetChildIndex(this.labelSymbolRate, 0);
      this.Controls.SetChildIndex(this.numericTextBoxFrequency, 0);
      this.Controls.SetChildIndex(this.labelFrequency, 0);
      this.Controls.SetChildIndex(this.numericTextBoxSymbolRate, 0);
      this.Controls.SetChildIndex(this.labelModulation, 0);
      this.Controls.SetChildIndex(this.comboBoxModulation, 0);
      this.Controls.SetChildIndex(this.labelFrequencyUnit, 0);
      this.Controls.SetChildIndex(this.labelSymbolRateUnit, 0);
      this.Controls.SetChildIndex(this.labelSatellite, 0);
      this.Controls.SetChildIndex(this.comboBoxSatellite, 0);
      this.Controls.SetChildIndex(this.labelBroadcastStandard, 0);
      this.Controls.SetChildIndex(this.comboBoxBroadcastStandard, 0);
      this.Controls.SetChildIndex(this.groupBoxEpgSource, 0);
      this.Controls.SetChildIndex(this.labelFreesatChannelId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxFreesatChannelId, 0);
      this.Controls.SetChildIndex(this.checkBoxIsHighDefinition, 0);
      this.Controls.SetChildIndex(this.labelIsHighDefinition, 0);
      this.Controls.SetChildIndex(this.checkBoxIsThreeDimensional, 0);
      this.Controls.SetChildIndex(this.labelIsThreeDimensional, 0);
      this.Controls.SetChildIndex(this.labelIsEncrypted, 0);
      this.Controls.SetChildIndex(this.textBoxProvider, 0);
      this.Controls.SetChildIndex(this.labelProvider, 0);
      this.Controls.SetChildIndex(this.checkBoxIsEncrypted, 0);
      this.Controls.SetChildIndex(this.textBoxName, 0);
      this.Controls.SetChildIndex(this.labelName, 0);
      this.Controls.SetChildIndex(this.channelNumberUpDownNumber, 0);
      this.Controls.SetChildIndex(this.labelNumber, 0);
      this.Controls.SetChildIndex(this.buttonOkay, 0);
      this.Controls.SetChildIndex(this.buttonCancel, 0);
      this.Controls.SetChildIndex(this.labelOpenTvChannelId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxOpenTvChannelId, 0);
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).EndInit();
      this.groupBoxEpgSource.ResumeLayout(false);
      this.groupBoxEpgSource.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPComboBox comboBoxRollOffFactor;
    private MPLabel labelRollOffFactor;
    private MPComboBox comboBoxPilotTonesState;
    private MPLabel labelPilotTonesState;
    private MPComboBox comboBoxFecCodeRate;
    private MPLabel labelFecCodeRate;
    private MPComboBox comboBoxPolarisation;
    private MPLabel labelPolarisation;
    private MPNumericTextBox numericTextBoxInputStreamId;
    private MPLabel labelInputStreamId;
    private MPNumericTextBox numericTextBoxPmtPid;
    private MPLabel labelOriginalNetworkId;
    private MPNumericTextBox numericTextBoxServiceId;
    private MPNumericTextBox numericTextBoxTransportStreamId;
    private MPNumericTextBox numericTextBoxOriginalNetworkId;
    private MPLabel labelPmtPid;
    private MPLabel labelTransportStreamId;
    private MPLabel labelServiceId;
    private MPLabel labelSymbolRateUnit;
    private MPLabel labelFrequencyUnit;
    private MPComboBox comboBoxModulation;
    private MPLabel labelModulation;
    private MPNumericTextBox numericTextBoxSymbolRate;
    private MPLabel labelFrequency;
    private MPNumericTextBox numericTextBoxFrequency;
    private MPLabel labelSymbolRate;
    private MPComboBox comboBoxSatellite;
    private MPLabel labelSatellite;
    private MPComboBox comboBoxBroadcastStandard;
    private MPLabel labelBroadcastStandard;
    private MPGroupBox groupBoxEpgSource;
    private MPLabel labelEpgOriginalNetworkId;
    private MPNumericTextBox numericTextBoxEpgServiceId;
    private MPNumericTextBox numericTextBoxEpgTransportStreamId;
    private MPNumericTextBox numericTextBoxEpgOriginalNetworkId;
    private MPLabel labelEpgTransportStreamId;
    private MPLabel labelEpgServiceId;
    private MPNumericTextBox numericTextBoxFreesatChannelId;
    private MPLabel labelFreesatChannelId;
    private MPNumericTextBox numericTextBoxOpenTvChannelId;
    private MPLabel labelOpenTvChannelId;
  }
}
