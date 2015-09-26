using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class CardDvbS
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
      this.tabControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabControl();
      this.tabPageScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBoxAdvancedOptions = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.numericTextBoxInputStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelInputStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelScanType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxScanType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelSymbolRateUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelModulation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxModulation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.numericTextBoxSymbolRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelSymbolRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequencyUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelFecCodeRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxFecCodeRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelRollOffFactor = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxRollOffFactor = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelPilotTonesState = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxPilotTonesState = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelPolarisation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxPolarisation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.groupBoxProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.progressBarProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.labelSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.progressBarSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.progressBarSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.listViewProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderStatus = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.comboBoxSatellite = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.comboBoxDiseqc = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.comboBoxLnbType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelLnbType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSatellite = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelDiseqc = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxUseAdvancedOptions = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.tabControl.SuspendLayout();
      this.tabPageScan.SuspendLayout();
      this.groupBoxAdvancedOptions.SuspendLayout();
      this.groupBoxProgress.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(356, 87);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(110, 23);
      this.buttonScan.TabIndex = 6;
      this.buttonScan.Text = "Scan for channels";
      this.buttonScan.UseVisualStyleBackColor = true;
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // tabControl
      // 
      this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl.Controls.Add(this.tabPageScan);
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(480, 420);
      this.tabControl.TabIndex = 0;
      // 
      // tabPageScan
      // 
      this.tabPageScan.BackColor = System.Drawing.Color.Transparent;
      this.tabPageScan.Controls.Add(this.groupBoxAdvancedOptions);
      this.tabPageScan.Controls.Add(this.groupBoxProgress);
      this.tabPageScan.Controls.Add(this.comboBoxSatellite);
      this.tabPageScan.Controls.Add(this.comboBoxDiseqc);
      this.tabPageScan.Controls.Add(this.comboBoxLnbType);
      this.tabPageScan.Controls.Add(this.labelLnbType);
      this.tabPageScan.Controls.Add(this.labelSatellite);
      this.tabPageScan.Controls.Add(this.labelDiseqc);
      this.tabPageScan.Controls.Add(this.checkBoxUseAdvancedOptions);
      this.tabPageScan.Controls.Add(this.buttonScan);
      this.tabPageScan.Location = new System.Drawing.Point(4, 22);
      this.tabPageScan.Name = "tabPageScan";
      this.tabPageScan.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageScan.Size = new System.Drawing.Size(472, 394);
      this.tabPageScan.TabIndex = 0;
      this.tabPageScan.Text = "Scanning";
      // 
      // groupBoxAdvancedOptions
      // 
      this.groupBoxAdvancedOptions.Controls.Add(this.numericTextBoxInputStreamId);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelInputStreamId);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelScanType);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxScanType);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelSymbolRateUnit);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelModulation);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxModulation);
      this.groupBoxAdvancedOptions.Controls.Add(this.numericTextBoxSymbolRate);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelSymbolRate);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelFrequency);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelFrequencyUnit);
      this.groupBoxAdvancedOptions.Controls.Add(this.numericTextBoxFrequency);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelBroadcastStandard);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxBroadcastStandard);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelFecCodeRate);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxFecCodeRate);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelRollOffFactor);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxRollOffFactor);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelPilotTonesState);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxPilotTonesState);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelPolarisation);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxPolarisation);
      this.groupBoxAdvancedOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAdvancedOptions.Location = new System.Drawing.Point(2, 202);
      this.groupBoxAdvancedOptions.Name = "groupBoxAdvancedOptions";
      this.groupBoxAdvancedOptions.Size = new System.Drawing.Size(468, 180);
      this.groupBoxAdvancedOptions.TabIndex = 8;
      this.groupBoxAdvancedOptions.TabStop = false;
      this.groupBoxAdvancedOptions.Text = "Advanced Options";
      this.groupBoxAdvancedOptions.Visible = false;
      // 
      // numericTextBoxInputStreamId
      // 
      this.numericTextBoxInputStreamId.Location = new System.Drawing.Point(363, 154);
      this.numericTextBoxInputStreamId.MaximumValue = 255;
      this.numericTextBoxInputStreamId.MaxLength = 3;
      this.numericTextBoxInputStreamId.MinimumValue = -1;
      this.numericTextBoxInputStreamId.Name = "numericTextBoxInputStreamId";
      this.numericTextBoxInputStreamId.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxInputStreamId.TabIndex = 21;
      this.numericTextBoxInputStreamId.Text = "-1";
      this.numericTextBoxInputStreamId.Value = -1;
      // 
      // labelInputStreamId
      // 
      this.labelInputStreamId.AutoSize = true;
      this.labelInputStreamId.Location = new System.Drawing.Point(272, 157);
      this.labelInputStreamId.Name = "labelInputStreamId";
      this.labelInputStreamId.Size = new System.Drawing.Size(82, 13);
      this.labelInputStreamId.TabIndex = 20;
      this.labelInputStreamId.Text = "Input stream ID:";
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
      this.comboBoxScanType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxScanType.FormattingEnabled = true;
      this.comboBoxScanType.ItemHeight = 13;
      this.comboBoxScanType.Location = new System.Drawing.Point(75, 19);
      this.comboBoxScanType.Name = "comboBoxScanType";
      this.comboBoxScanType.Size = new System.Drawing.Size(150, 21);
      this.comboBoxScanType.TabIndex = 1;
      this.comboBoxScanType.SelectedIndexChanged += new System.EventHandler(this.comboBoxScanType_SelectedIndexChanged);
      // 
      // labelSymbolRateUnit
      // 
      this.labelSymbolRateUnit.AutoSize = true;
      this.labelSymbolRateUnit.Location = new System.Drawing.Point(136, 157);
      this.labelSymbolRateUnit.Name = "labelSymbolRateUnit";
      this.labelSymbolRateUnit.Size = new System.Drawing.Size(28, 13);
      this.labelSymbolRateUnit.TabIndex = 13;
      this.labelSymbolRateUnit.Text = "ks/s";
      // 
      // labelModulation
      // 
      this.labelModulation.AutoSize = true;
      this.labelModulation.Location = new System.Drawing.Point(4, 130);
      this.labelModulation.Name = "labelModulation";
      this.labelModulation.Size = new System.Drawing.Size(62, 13);
      this.labelModulation.TabIndex = 9;
      this.labelModulation.Text = "Modulation:";
      // 
      // comboBoxModulation
      // 
      this.comboBoxModulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxModulation.FormattingEnabled = true;
      this.comboBoxModulation.ItemHeight = 13;
      this.comboBoxModulation.Location = new System.Drawing.Point(75, 127);
      this.comboBoxModulation.Name = "comboBoxModulation";
      this.comboBoxModulation.Size = new System.Drawing.Size(150, 21);
      this.comboBoxModulation.TabIndex = 10;
      // 
      // numericTextBoxSymbolRate
      // 
      this.numericTextBoxSymbolRate.Location = new System.Drawing.Point(75, 154);
      this.numericTextBoxSymbolRate.MaximumValue = 99999;
      this.numericTextBoxSymbolRate.MaxLength = 5;
      this.numericTextBoxSymbolRate.MinimumValue = 100;
      this.numericTextBoxSymbolRate.Name = "numericTextBoxSymbolRate";
      this.numericTextBoxSymbolRate.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxSymbolRate.TabIndex = 12;
      this.numericTextBoxSymbolRate.Text = "25000";
      this.numericTextBoxSymbolRate.Value = 25000;
      // 
      // labelSymbolRate
      // 
      this.labelSymbolRate.AutoSize = true;
      this.labelSymbolRate.Location = new System.Drawing.Point(4, 157);
      this.labelSymbolRate.Name = "labelSymbolRate";
      this.labelSymbolRate.Size = new System.Drawing.Size(65, 13);
      this.labelSymbolRate.TabIndex = 11;
      this.labelSymbolRate.Text = "Symbol rate:";
      // 
      // labelFrequency
      // 
      this.labelFrequency.AutoSize = true;
      this.labelFrequency.Location = new System.Drawing.Point(4, 76);
      this.labelFrequency.Name = "labelFrequency";
      this.labelFrequency.Size = new System.Drawing.Size(60, 13);
      this.labelFrequency.TabIndex = 4;
      this.labelFrequency.Text = "Frequency:";
      // 
      // labelFrequencyUnit
      // 
      this.labelFrequencyUnit.AutoSize = true;
      this.labelFrequencyUnit.Location = new System.Drawing.Point(136, 76);
      this.labelFrequencyUnit.Name = "labelFrequencyUnit";
      this.labelFrequencyUnit.Size = new System.Drawing.Size(26, 13);
      this.labelFrequencyUnit.TabIndex = 6;
      this.labelFrequencyUnit.Text = "kHz";
      // 
      // numericTextBoxFrequency
      // 
      this.numericTextBoxFrequency.Location = new System.Drawing.Point(75, 73);
      this.numericTextBoxFrequency.MaximumValue = 20000000;
      this.numericTextBoxFrequency.MaxLength = 8;
      this.numericTextBoxFrequency.MinimumValue = 1500000;
      this.numericTextBoxFrequency.Name = "numericTextBoxFrequency";
      this.numericTextBoxFrequency.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxFrequency.TabIndex = 5;
      this.numericTextBoxFrequency.Text = "11097000";
      this.numericTextBoxFrequency.Value = 11097000;
      // 
      // labelBroadcastStandard
      // 
      this.labelBroadcastStandard.AutoSize = true;
      this.labelBroadcastStandard.Location = new System.Drawing.Point(4, 49);
      this.labelBroadcastStandard.Name = "labelBroadcastStandard";
      this.labelBroadcastStandard.Size = new System.Drawing.Size(53, 13);
      this.labelBroadcastStandard.TabIndex = 2;
      this.labelBroadcastStandard.Text = "Standard:";
      // 
      // comboBoxBroadcastStandard
      // 
      this.comboBoxBroadcastStandard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxBroadcastStandard.FormattingEnabled = true;
      this.comboBoxBroadcastStandard.ItemHeight = 13;
      this.comboBoxBroadcastStandard.Location = new System.Drawing.Point(75, 46);
      this.comboBoxBroadcastStandard.Name = "comboBoxBroadcastStandard";
      this.comboBoxBroadcastStandard.Size = new System.Drawing.Size(87, 21);
      this.comboBoxBroadcastStandard.TabIndex = 3;
      this.comboBoxBroadcastStandard.SelectedIndexChanged += new System.EventHandler(this.comboBoxBroadcastStandard_SelectedIndexChanged);
      // 
      // labelFecCodeRate
      // 
      this.labelFecCodeRate.AutoSize = true;
      this.labelFecCodeRate.Location = new System.Drawing.Point(272, 76);
      this.labelFecCodeRate.Name = "labelFecCodeRate";
      this.labelFecCodeRate.Size = new System.Drawing.Size(78, 13);
      this.labelFecCodeRate.TabIndex = 14;
      this.labelFecCodeRate.Text = "FEC code rate:";
      // 
      // comboBoxFecCodeRate
      // 
      this.comboBoxFecCodeRate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxFecCodeRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxFecCodeRate.FormattingEnabled = true;
      this.comboBoxFecCodeRate.Location = new System.Drawing.Point(363, 73);
      this.comboBoxFecCodeRate.Name = "comboBoxFecCodeRate";
      this.comboBoxFecCodeRate.Size = new System.Drawing.Size(87, 21);
      this.comboBoxFecCodeRate.TabIndex = 15;
      // 
      // labelRollOffFactor
      // 
      this.labelRollOffFactor.AutoSize = true;
      this.labelRollOffFactor.Location = new System.Drawing.Point(272, 130);
      this.labelRollOffFactor.Name = "labelRollOffFactor";
      this.labelRollOffFactor.Size = new System.Drawing.Size(73, 13);
      this.labelRollOffFactor.TabIndex = 18;
      this.labelRollOffFactor.Text = "Roll-off factor:";
      // 
      // comboBoxRollOffFactor
      // 
      this.comboBoxRollOffFactor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxRollOffFactor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxRollOffFactor.FormattingEnabled = true;
      this.comboBoxRollOffFactor.Location = new System.Drawing.Point(363, 127);
      this.comboBoxRollOffFactor.Name = "comboBoxRollOffFactor";
      this.comboBoxRollOffFactor.Size = new System.Drawing.Size(87, 21);
      this.comboBoxRollOffFactor.TabIndex = 19;
      // 
      // labelPilotTonesState
      // 
      this.labelPilotTonesState.AutoSize = true;
      this.labelPilotTonesState.Location = new System.Drawing.Point(272, 103);
      this.labelPilotTonesState.Name = "labelPilotTonesState";
      this.labelPilotTonesState.Size = new System.Drawing.Size(85, 13);
      this.labelPilotTonesState.TabIndex = 16;
      this.labelPilotTonesState.Text = "Pilot tones state:";
      // 
      // comboBoxPilotTonesState
      // 
      this.comboBoxPilotTonesState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxPilotTonesState.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPilotTonesState.FormattingEnabled = true;
      this.comboBoxPilotTonesState.Location = new System.Drawing.Point(363, 100);
      this.comboBoxPilotTonesState.Name = "comboBoxPilotTonesState";
      this.comboBoxPilotTonesState.Size = new System.Drawing.Size(87, 21);
      this.comboBoxPilotTonesState.TabIndex = 17;
      // 
      // labelPolarisation
      // 
      this.labelPolarisation.AutoSize = true;
      this.labelPolarisation.Location = new System.Drawing.Point(4, 103);
      this.labelPolarisation.Name = "labelPolarisation";
      this.labelPolarisation.Size = new System.Drawing.Size(64, 13);
      this.labelPolarisation.TabIndex = 7;
      this.labelPolarisation.Text = "Polarisation:";
      // 
      // comboBoxPolarisation
      // 
      this.comboBoxPolarisation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxPolarisation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPolarisation.FormattingEnabled = true;
      this.comboBoxPolarisation.Location = new System.Drawing.Point(75, 100);
      this.comboBoxPolarisation.Name = "comboBoxPolarisation";
      this.comboBoxPolarisation.Size = new System.Drawing.Size(150, 21);
      this.comboBoxPolarisation.TabIndex = 8;
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
      this.groupBoxProgress.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxProgress.Location = new System.Drawing.Point(2, 137);
      this.groupBoxProgress.Name = "groupBoxProgress";
      this.groupBoxProgress.Size = new System.Drawing.Size(468, 251);
      this.groupBoxProgress.TabIndex = 9;
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
      this.progressBarProgress.Size = new System.Drawing.Size(457, 10);
      this.progressBarProgress.TabIndex = 4;
      // 
      // labelSignalStrength
      // 
      this.labelSignalStrength.AutoSize = true;
      this.labelSignalStrength.Location = new System.Drawing.Point(4, 16);
      this.labelSignalStrength.Name = "labelSignalStrength";
      this.labelSignalStrength.Size = new System.Drawing.Size(64, 13);
      this.labelSignalStrength.TabIndex = 0;
      this.labelSignalStrength.Text = "Signal level:";
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
      this.progressBarSignalStrength.Size = new System.Drawing.Size(366, 10);
      this.progressBarSignalStrength.TabIndex = 1;
      // 
      // progressBarSignalQuality
      // 
      this.progressBarSignalQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalQuality.Location = new System.Drawing.Point(98, 35);
      this.progressBarSignalQuality.Name = "progressBarSignalQuality";
      this.progressBarSignalQuality.Size = new System.Drawing.Size(366, 10);
      this.progressBarSignalQuality.TabIndex = 3;
      // 
      // listViewProgress
      // 
      this.listViewProgress.AllowRowReorder = false;
      this.listViewProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewProgress.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStatus});
      this.listViewProgress.Location = new System.Drawing.Point(7, 67);
      this.listViewProgress.Name = "listViewProgress";
      this.listViewProgress.Size = new System.Drawing.Size(457, 178);
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
      this.comboBoxSatellite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSatellite.FormattingEnabled = true;
      this.comboBoxSatellite.Location = new System.Drawing.Point(100, 6);
      this.comboBoxSatellite.Name = "comboBoxSatellite";
      this.comboBoxSatellite.Size = new System.Drawing.Size(366, 21);
      this.comboBoxSatellite.TabIndex = 1;
      // 
      // comboBoxDiseqc
      // 
      this.comboBoxDiseqc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxDiseqc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDiseqc.FormattingEnabled = true;
      this.comboBoxDiseqc.Location = new System.Drawing.Point(100, 60);
      this.comboBoxDiseqc.Name = "comboBoxDiseqc";
      this.comboBoxDiseqc.Size = new System.Drawing.Size(366, 21);
      this.comboBoxDiseqc.TabIndex = 5;
      // 
      // comboBoxLnbType
      // 
      this.comboBoxLnbType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxLnbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxLnbType.FormattingEnabled = true;
      this.comboBoxLnbType.Location = new System.Drawing.Point(100, 33);
      this.comboBoxLnbType.Name = "comboBoxLnbType";
      this.comboBoxLnbType.Size = new System.Drawing.Size(366, 21);
      this.comboBoxLnbType.TabIndex = 3;
      // 
      // labelLnbType
      // 
      this.labelLnbType.AutoSize = true;
      this.labelLnbType.Location = new System.Drawing.Point(6, 36);
      this.labelLnbType.Name = "labelLnbType";
      this.labelLnbType.Size = new System.Drawing.Size(54, 13);
      this.labelLnbType.TabIndex = 2;
      this.labelLnbType.Text = "LNB type:";
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
      // labelDiseqc
      // 
      this.labelDiseqc.AutoSize = true;
      this.labelDiseqc.Location = new System.Drawing.Point(6, 63);
      this.labelDiseqc.Name = "labelDiseqc";
      this.labelDiseqc.Size = new System.Drawing.Size(80, 13);
      this.labelDiseqc.TabIndex = 4;
      this.labelDiseqc.Text = "DiSEqC switch:";
      // 
      // checkBoxUseAdvancedOptions
      // 
      this.checkBoxUseAdvancedOptions.AutoSize = true;
      this.checkBoxUseAdvancedOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseAdvancedOptions.Location = new System.Drawing.Point(9, 114);
      this.checkBoxUseAdvancedOptions.Name = "checkBoxUseAdvancedOptions";
      this.checkBoxUseAdvancedOptions.Size = new System.Drawing.Size(134, 17);
      this.checkBoxUseAdvancedOptions.TabIndex = 7;
      this.checkBoxUseAdvancedOptions.Text = "Use advanced options.";
      this.checkBoxUseAdvancedOptions.UseVisualStyleBackColor = true;
      this.checkBoxUseAdvancedOptions.CheckedChanged += new System.EventHandler(this.checkBoxUseAdvancedScanningOptions_CheckedChanged);
      // 
      // CardDvbS
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.tabControl);
      this.Name = "CardDvbS";
      this.Size = new System.Drawing.Size(480, 420);
      this.tabControl.ResumeLayout(false);
      this.tabPageScan.ResumeLayout(false);
      this.tabPageScan.PerformLayout();
      this.groupBoxAdvancedOptions.ResumeLayout(false);
      this.groupBoxAdvancedOptions.PerformLayout();
      this.groupBoxProgress.ResumeLayout(false);
      this.groupBoxProgress.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MPButton buttonScan;
    private MPTabControl tabControl;
    private MPTabPage tabPageScan; 
    private MPComboBox comboBoxPolarisation;
    private MPLabel labelPolarisation;
    private MPGroupBox groupBoxAdvancedOptions;
    private MPCheckBox checkBoxUseAdvancedOptions;
    private MPComboBox comboBoxSatellite;
    private MPComboBox comboBoxDiseqc;
    private MPComboBox comboBoxLnbType;
    private MPLabel labelLnbType;
    private MPLabel labelSatellite;
    private MPLabel labelDiseqc;
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
  }
}
