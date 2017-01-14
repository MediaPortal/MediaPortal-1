using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class ScanTerrestrial
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
      this.labelTransmitter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxTransmitter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.checkBoxUseAdvancedOptions = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.groupBoxAdvancedOptions = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.groupBoxManualTuning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.comboBoxBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelBandwidth = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxPlpId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxBandwidth = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelPlpId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelFrequencyUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelBandwidthUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
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
      this.columnHeader1 = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.buttonScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelRegionProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxRegionProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.comboBoxCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.groupBoxAdvancedOptions.SuspendLayout();
      this.groupBoxManualTuning.SuspendLayout();
      this.groupBoxProgress.SuspendLayout();
      this.SuspendLayout();
      // 
      // labelTransmitter
      // 
      this.labelTransmitter.AutoSize = true;
      this.labelTransmitter.Location = new System.Drawing.Point(6, 63);
      this.labelTransmitter.Name = "labelTransmitter";
      this.labelTransmitter.Size = new System.Drawing.Size(62, 13);
      this.labelTransmitter.TabIndex = 4;
      this.labelTransmitter.Text = "Transmitter:";
      // 
      // comboBoxTransmitter
      // 
      this.comboBoxTransmitter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxTransmitter.FormattingEnabled = true;
      this.comboBoxTransmitter.Location = new System.Drawing.Point(100, 60);
      this.comboBoxTransmitter.Name = "comboBoxTransmitter";
      this.comboBoxTransmitter.Size = new System.Drawing.Size(372, 21);
      this.comboBoxTransmitter.TabIndex = 5;
      this.comboBoxTransmitter.SelectedIndexChanged += new System.EventHandler(this.comboBoxTransmitter_SelectedIndexChanged);
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
      this.checkBoxUseAdvancedOptions.CheckedChanged += new System.EventHandler(this.checkBoxUseAdvancedOptions_CheckedChanged);
      // 
      // groupBoxAdvancedOptions
      // 
      this.groupBoxAdvancedOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAdvancedOptions.Controls.Add(this.groupBoxManualTuning);
      this.groupBoxAdvancedOptions.Controls.Add(this.checkBoxUseManualTuning);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelScanType);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxScanType);
      this.groupBoxAdvancedOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAdvancedOptions.Location = new System.Drawing.Point(2, 197);
      this.groupBoxAdvancedOptions.Name = "groupBoxAdvancedOptions";
      this.groupBoxAdvancedOptions.Size = new System.Drawing.Size(476, 205);
      this.groupBoxAdvancedOptions.TabIndex = 8;
      this.groupBoxAdvancedOptions.TabStop = false;
      this.groupBoxAdvancedOptions.Text = "Advanced Options";
      this.groupBoxAdvancedOptions.Visible = false;
      // 
      // groupBoxManualTuning
      // 
      this.groupBoxManualTuning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxManualTuning.Controls.Add(this.comboBoxBroadcastStandard);
      this.groupBoxManualTuning.Controls.Add(this.labelBandwidth);
      this.groupBoxManualTuning.Controls.Add(this.numericTextBoxPlpId);
      this.groupBoxManualTuning.Controls.Add(this.numericTextBoxBandwidth);
      this.groupBoxManualTuning.Controls.Add(this.labelPlpId);
      this.groupBoxManualTuning.Controls.Add(this.labelBroadcastStandard);
      this.groupBoxManualTuning.Controls.Add(this.numericTextBoxFrequency);
      this.groupBoxManualTuning.Controls.Add(this.labelFrequencyUnit);
      this.groupBoxManualTuning.Controls.Add(this.labelBandwidthUnit);
      this.groupBoxManualTuning.Controls.Add(this.labelFrequency);
      this.groupBoxManualTuning.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxManualTuning.Location = new System.Drawing.Point(7, 69);
      this.groupBoxManualTuning.Name = "groupBoxManualTuning";
      this.groupBoxManualTuning.Size = new System.Drawing.Size(463, 128);
      this.groupBoxManualTuning.TabIndex = 3;
      this.groupBoxManualTuning.TabStop = false;
      this.groupBoxManualTuning.Text = "Manual Tuning";
      // 
      // comboBoxBroadcastStandard
      // 
      this.comboBoxBroadcastStandard.Enabled = false;
      this.comboBoxBroadcastStandard.FormattingEnabled = true;
      this.comboBoxBroadcastStandard.Location = new System.Drawing.Point(91, 19);
      this.comboBoxBroadcastStandard.Name = "comboBoxBroadcastStandard";
      this.comboBoxBroadcastStandard.Size = new System.Drawing.Size(73, 21);
      this.comboBoxBroadcastStandard.TabIndex = 1;
      this.comboBoxBroadcastStandard.SelectedIndexChanged += new System.EventHandler(this.comboBoxBroadcastStandard_SelectedIndexChanged);
      // 
      // labelBandwidth
      // 
      this.labelBandwidth.AutoSize = true;
      this.labelBandwidth.Location = new System.Drawing.Point(6, 75);
      this.labelBandwidth.Name = "labelBandwidth";
      this.labelBandwidth.Size = new System.Drawing.Size(60, 13);
      this.labelBandwidth.TabIndex = 5;
      this.labelBandwidth.Text = "Bandwidth:";
      // 
      // numericTextBoxPlpId
      // 
      this.numericTextBoxPlpId.Enabled = false;
      this.numericTextBoxPlpId.Location = new System.Drawing.Point(91, 98);
      this.numericTextBoxPlpId.MaximumValue = 255;
      this.numericTextBoxPlpId.MaxLength = 5;
      this.numericTextBoxPlpId.MinimumValue = -1;
      this.numericTextBoxPlpId.Name = "numericTextBoxPlpId";
      this.numericTextBoxPlpId.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxPlpId.TabIndex = 9;
      this.numericTextBoxPlpId.Text = "-1";
      this.numericTextBoxPlpId.Value = -1;
      // 
      // numericTextBoxBandwidth
      // 
      this.numericTextBoxBandwidth.Enabled = false;
      this.numericTextBoxBandwidth.Location = new System.Drawing.Point(91, 72);
      this.numericTextBoxBandwidth.MaximumValue = 10000;
      this.numericTextBoxBandwidth.MaxLength = 5;
      this.numericTextBoxBandwidth.MinimumValue = 100;
      this.numericTextBoxBandwidth.Name = "numericTextBoxBandwidth";
      this.numericTextBoxBandwidth.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxBandwidth.TabIndex = 6;
      this.numericTextBoxBandwidth.Text = "8000";
      this.numericTextBoxBandwidth.Value = 8000;
      // 
      // labelPlpId
      // 
      this.labelPlpId.AutoSize = true;
      this.labelPlpId.Location = new System.Drawing.Point(6, 101);
      this.labelPlpId.Name = "labelPlpId";
      this.labelPlpId.Size = new System.Drawing.Size(44, 13);
      this.labelPlpId.TabIndex = 8;
      this.labelPlpId.Text = "PLP ID:";
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
      // numericTextBoxFrequency
      // 
      this.numericTextBoxFrequency.Enabled = false;
      this.numericTextBoxFrequency.Location = new System.Drawing.Point(91, 46);
      this.numericTextBoxFrequency.MaximumValue = 999999;
      this.numericTextBoxFrequency.MaxLength = 6;
      this.numericTextBoxFrequency.MinimumValue = 50000;
      this.numericTextBoxFrequency.Name = "numericTextBoxFrequency";
      this.numericTextBoxFrequency.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxFrequency.TabIndex = 3;
      this.numericTextBoxFrequency.Text = "163000";
      this.numericTextBoxFrequency.Value = 163000;
      // 
      // labelFrequencyUnit
      // 
      this.labelFrequencyUnit.AutoSize = true;
      this.labelFrequencyUnit.Location = new System.Drawing.Point(138, 49);
      this.labelFrequencyUnit.Name = "labelFrequencyUnit";
      this.labelFrequencyUnit.Size = new System.Drawing.Size(26, 13);
      this.labelFrequencyUnit.TabIndex = 4;
      this.labelFrequencyUnit.Text = "kHz";
      // 
      // labelBandwidthUnit
      // 
      this.labelBandwidthUnit.AutoSize = true;
      this.labelBandwidthUnit.Location = new System.Drawing.Point(138, 75);
      this.labelBandwidthUnit.Name = "labelBandwidthUnit";
      this.labelBandwidthUnit.Size = new System.Drawing.Size(26, 13);
      this.labelBandwidthUnit.TabIndex = 7;
      this.labelBandwidthUnit.Text = "kHz";
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
      // checkBoxUseManualTuning
      // 
      this.checkBoxUseManualTuning.AutoSize = true;
      this.checkBoxUseManualTuning.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseManualTuning.Location = new System.Drawing.Point(7, 46);
      this.checkBoxUseManualTuning.Name = "checkBoxUseManualTuning";
      this.checkBoxUseManualTuning.Size = new System.Drawing.Size(115, 17);
      this.checkBoxUseManualTuning.TabIndex = 2;
      this.checkBoxUseManualTuning.Text = "Use manual tuning.";
      this.checkBoxUseManualTuning.UseVisualStyleBackColor = true;
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
      this.comboBoxScanType.Size = new System.Drawing.Size(120, 21);
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
      this.groupBoxProgress.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxProgress.Location = new System.Drawing.Point(2, 137);
      this.groupBoxProgress.Name = "groupBoxProgress";
      this.groupBoxProgress.Size = new System.Drawing.Size(476, 278);
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
            this.columnHeader1});
      this.listViewProgress.Location = new System.Drawing.Point(7, 67);
      this.listViewProgress.Name = "listViewProgress";
      this.listViewProgress.Size = new System.Drawing.Size(463, 205);
      this.listViewProgress.TabIndex = 5;
      this.listViewProgress.UseCompatibleStateImageBehavior = false;
      this.listViewProgress.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Status";
      this.columnHeader1.Width = 430;
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(362, 87);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(110, 23);
      this.buttonScan.TabIndex = 6;
      this.buttonScan.Text = "&Scan for channels";
      this.buttonScan.UseVisualStyleBackColor = true;
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // labelRegionProvider
      // 
      this.labelRegionProvider.AutoSize = true;
      this.labelRegionProvider.Location = new System.Drawing.Point(6, 36);
      this.labelRegionProvider.Name = "labelRegionProvider";
      this.labelRegionProvider.Size = new System.Drawing.Size(87, 13);
      this.labelRegionProvider.TabIndex = 2;
      this.labelRegionProvider.Text = "Region/provider:";
      // 
      // labelCountry
      // 
      this.labelCountry.AutoSize = true;
      this.labelCountry.Location = new System.Drawing.Point(6, 9);
      this.labelCountry.Name = "labelCountry";
      this.labelCountry.Size = new System.Drawing.Size(46, 13);
      this.labelCountry.TabIndex = 0;
      this.labelCountry.Text = "Country:";
      // 
      // comboBoxRegionProvider
      // 
      this.comboBoxRegionProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxRegionProvider.FormattingEnabled = true;
      this.comboBoxRegionProvider.Location = new System.Drawing.Point(100, 33);
      this.comboBoxRegionProvider.Name = "comboBoxRegionProvider";
      this.comboBoxRegionProvider.Size = new System.Drawing.Size(372, 21);
      this.comboBoxRegionProvider.TabIndex = 3;
      // 
      // comboBoxCountry
      // 
      this.comboBoxCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxCountry.FormattingEnabled = true;
      this.comboBoxCountry.Location = new System.Drawing.Point(100, 6);
      this.comboBoxCountry.Name = "comboBoxCountry";
      this.comboBoxCountry.Size = new System.Drawing.Size(372, 21);
      this.comboBoxCountry.TabIndex = 1;
      // 
      // ScanTerrestrial
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.labelTransmitter);
      this.Controls.Add(this.comboBoxTransmitter);
      this.Controls.Add(this.checkBoxUseAdvancedOptions);
      this.Controls.Add(this.groupBoxAdvancedOptions);
      this.Controls.Add(this.groupBoxProgress);
      this.Controls.Add(this.buttonScan);
      this.Controls.Add(this.labelRegionProvider);
      this.Controls.Add(this.labelCountry);
      this.Controls.Add(this.comboBoxRegionProvider);
      this.Controls.Add(this.comboBoxCountry);
      this.Name = "ScanTerrestrial";
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

    private MPCheckBox checkBoxUseAdvancedOptions;
    private MPGroupBox groupBoxAdvancedOptions;
    private MPLabel labelBandwidthUnit;
    private MPLabel labelFrequency;
    private MPLabel labelFrequencyUnit;
    private MPNumericTextBox numericTextBoxFrequency;
    private MPLabel labelBroadcastStandard;
    private MPComboBox comboBoxBroadcastStandard;
    private MPNumericTextBox numericTextBoxBandwidth;
    private MPLabel labelBandwidth;
    private MPGroupBox groupBoxProgress;
    private MPProgressBar progressBarProgress;
    private MPLabel labelSignalStrength;
    private MPLabel labelSignalQuality;
    private MPProgressBar progressBarSignalStrength;
    private MPProgressBar progressBarSignalQuality;
    private MPListView listViewProgress;
    private MPColumnHeader columnHeader1;
    private MPButton buttonScan;
    private MPLabel labelRegionProvider;
    private MPLabel labelCountry;
    private MPComboBox comboBoxRegionProvider;
    private MPComboBox comboBoxCountry;
    private MPLabel labelScanType;
    private MPComboBox comboBoxScanType;
    private MPNumericTextBox numericTextBoxPlpId;
    private MPLabel labelPlpId;
    private MPLabel labelTransmitter;
    private MPComboBox comboBoxTransmitter;
    private MPCheckBox checkBoxUseManualTuning;
    private MPGroupBox groupBoxManualTuning;
  }
}