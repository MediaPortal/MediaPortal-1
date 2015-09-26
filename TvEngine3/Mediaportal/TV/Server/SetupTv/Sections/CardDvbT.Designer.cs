using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class CardDvbT
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
      this.tabControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabControl();
      this.tabPageScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.checkBoxUseAdvancedOptions = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.groupBoxAdvancedOptions = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.numericTextBoxPlpId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelPlpId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelScanType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxScanType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelBandwidthUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequencyUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.numericTextBoxBandwidth = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelBandwidth = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.progressBarProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.labelSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.progressBarSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.progressBarSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.listViewProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeader1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.buttonScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelRegionProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxRegionProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.comboBoxCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.tabControl.SuspendLayout();
      this.tabPageScan.SuspendLayout();
      this.groupBoxAdvancedOptions.SuspendLayout();
      this.groupBoxProgress.SuspendLayout();
      this.SuspendLayout();
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
      this.tabPageScan.Controls.Add(this.checkBoxUseAdvancedOptions);
      this.tabPageScan.Controls.Add(this.groupBoxAdvancedOptions);
      this.tabPageScan.Controls.Add(this.groupBoxProgress);
      this.tabPageScan.Controls.Add(this.buttonScan);
      this.tabPageScan.Controls.Add(this.labelRegionProvider);
      this.tabPageScan.Controls.Add(this.labelCountry);
      this.tabPageScan.Controls.Add(this.comboBoxRegionProvider);
      this.tabPageScan.Controls.Add(this.comboBoxCountry);
      this.tabPageScan.Location = new System.Drawing.Point(4, 22);
      this.tabPageScan.Name = "tabPageScan";
      this.tabPageScan.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageScan.Size = new System.Drawing.Size(472, 394);
      this.tabPageScan.TabIndex = 0;
      this.tabPageScan.Text = "Scanning";
      // 
      // checkBoxUseAdvancedOptions
      // 
      this.checkBoxUseAdvancedOptions.AutoSize = true;
      this.checkBoxUseAdvancedOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseAdvancedOptions.Location = new System.Drawing.Point(9, 87);
      this.checkBoxUseAdvancedOptions.Name = "checkBoxUseAdvancedOptions";
      this.checkBoxUseAdvancedOptions.Size = new System.Drawing.Size(134, 17);
      this.checkBoxUseAdvancedOptions.TabIndex = 5;
      this.checkBoxUseAdvancedOptions.Text = "Use advanced options.";
      this.checkBoxUseAdvancedOptions.UseVisualStyleBackColor = true;
      this.checkBoxUseAdvancedOptions.CheckedChanged += new System.EventHandler(this.checkBoxUseAdvancedScanningOptions_CheckedChanged);
      // 
      // groupBoxAdvancedOptions
      // 
      this.groupBoxAdvancedOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAdvancedOptions.Controls.Add(this.numericTextBoxPlpId);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelPlpId);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelScanType);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxScanType);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelBandwidthUnit);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelFrequency);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelFrequencyUnit);
      this.groupBoxAdvancedOptions.Controls.Add(this.numericTextBoxFrequency);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelBroadcastStandard);
      this.groupBoxAdvancedOptions.Controls.Add(this.comboBoxBroadcastStandard);
      this.groupBoxAdvancedOptions.Controls.Add(this.numericTextBoxBandwidth);
      this.groupBoxAdvancedOptions.Controls.Add(this.labelBandwidth);
      this.groupBoxAdvancedOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAdvancedOptions.Location = new System.Drawing.Point(2, 207);
      this.groupBoxAdvancedOptions.Name = "groupBoxAdvancedOptions";
      this.groupBoxAdvancedOptions.Size = new System.Drawing.Size(468, 159);
      this.groupBoxAdvancedOptions.TabIndex = 6;
      this.groupBoxAdvancedOptions.TabStop = false;
      this.groupBoxAdvancedOptions.Text = "Advanced Options";
      this.groupBoxAdvancedOptions.Visible = false;
      // 
      // numericTextBoxPlpId
      // 
      this.numericTextBoxPlpId.Enabled = false;
      this.numericTextBoxPlpId.Location = new System.Drawing.Point(70, 125);
      this.numericTextBoxPlpId.MaximumValue = 255;
      this.numericTextBoxPlpId.MaxLength = 5;
      this.numericTextBoxPlpId.MinimumValue = -1;
      this.numericTextBoxPlpId.Name = "numericTextBoxPlpId";
      this.numericTextBoxPlpId.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxPlpId.TabIndex = 11;
      this.numericTextBoxPlpId.Text = "-1";
      this.numericTextBoxPlpId.Value = -1;
      // 
      // labelPlpId
      // 
      this.labelPlpId.AutoSize = true;
      this.labelPlpId.Location = new System.Drawing.Point(4, 128);
      this.labelPlpId.Name = "labelPlpId";
      this.labelPlpId.Size = new System.Drawing.Size(44, 13);
      this.labelPlpId.TabIndex = 10;
      this.labelPlpId.Text = "PLP ID:";
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
      this.comboBoxScanType.Location = new System.Drawing.Point(70, 19);
      this.comboBoxScanType.Name = "comboBoxScanType";
      this.comboBoxScanType.Size = new System.Drawing.Size(120, 21);
      this.comboBoxScanType.TabIndex = 1;
      this.comboBoxScanType.SelectedIndexChanged += new System.EventHandler(this.comboBoxScanType_SelectedIndexChanged);
      // 
      // labelBandwidthUnit
      // 
      this.labelBandwidthUnit.AutoSize = true;
      this.labelBandwidthUnit.Location = new System.Drawing.Point(131, 102);
      this.labelBandwidthUnit.Name = "labelBandwidthUnit";
      this.labelBandwidthUnit.Size = new System.Drawing.Size(26, 13);
      this.labelBandwidthUnit.TabIndex = 9;
      this.labelBandwidthUnit.Text = "kHz";
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
      this.labelFrequencyUnit.Location = new System.Drawing.Point(131, 76);
      this.labelFrequencyUnit.Name = "labelFrequencyUnit";
      this.labelFrequencyUnit.Size = new System.Drawing.Size(26, 13);
      this.labelFrequencyUnit.TabIndex = 6;
      this.labelFrequencyUnit.Text = "kHz";
      // 
      // numericTextBoxFrequency
      // 
      this.numericTextBoxFrequency.Enabled = false;
      this.numericTextBoxFrequency.Location = new System.Drawing.Point(70, 73);
      this.numericTextBoxFrequency.MaximumValue = 999999;
      this.numericTextBoxFrequency.MaxLength = 6;
      this.numericTextBoxFrequency.MinimumValue = 50000;
      this.numericTextBoxFrequency.Name = "numericTextBoxFrequency";
      this.numericTextBoxFrequency.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxFrequency.TabIndex = 5;
      this.numericTextBoxFrequency.Text = "163000";
      this.numericTextBoxFrequency.Value = 163000;
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
      this.comboBoxBroadcastStandard.Enabled = false;
      this.comboBoxBroadcastStandard.FormattingEnabled = true;
      this.comboBoxBroadcastStandard.ItemHeight = 13;
      this.comboBoxBroadcastStandard.Location = new System.Drawing.Point(70, 46);
      this.comboBoxBroadcastStandard.Name = "comboBoxBroadcastStandard";
      this.comboBoxBroadcastStandard.Size = new System.Drawing.Size(87, 21);
      this.comboBoxBroadcastStandard.TabIndex = 3;
      this.comboBoxBroadcastStandard.SelectedIndexChanged += new System.EventHandler(this.comboBoxBroadcastStandard_SelectedIndexChanged);
      // 
      // numericTextBoxBandwidth
      // 
      this.numericTextBoxBandwidth.Enabled = false;
      this.numericTextBoxBandwidth.Location = new System.Drawing.Point(70, 99);
      this.numericTextBoxBandwidth.MaximumValue = 10000;
      this.numericTextBoxBandwidth.MaxLength = 5;
      this.numericTextBoxBandwidth.MinimumValue = 100;
      this.numericTextBoxBandwidth.Name = "numericTextBoxBandwidth";
      this.numericTextBoxBandwidth.Size = new System.Drawing.Size(55, 20);
      this.numericTextBoxBandwidth.TabIndex = 8;
      this.numericTextBoxBandwidth.Text = "8000";
      this.numericTextBoxBandwidth.Value = 8000;
      // 
      // labelBandwidth
      // 
      this.labelBandwidth.AutoSize = true;
      this.labelBandwidth.Location = new System.Drawing.Point(4, 102);
      this.labelBandwidth.Name = "labelBandwidth";
      this.labelBandwidth.Size = new System.Drawing.Size(60, 13);
      this.labelBandwidth.TabIndex = 7;
      this.labelBandwidth.Text = "Bandwidth:";
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
      this.groupBoxProgress.Location = new System.Drawing.Point(2, 110);
      this.groupBoxProgress.Name = "groupBoxProgress";
      this.groupBoxProgress.Size = new System.Drawing.Size(468, 278);
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
            this.columnHeader1});
      this.listViewProgress.Location = new System.Drawing.Point(7, 67);
      this.listViewProgress.Name = "listViewProgress";
      this.listViewProgress.Size = new System.Drawing.Size(457, 205);
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
      this.buttonScan.Location = new System.Drawing.Point(356, 60);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(110, 23);
      this.buttonScan.TabIndex = 4;
      this.buttonScan.Text = "Scan for channels";
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
      this.comboBoxRegionProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxRegionProvider.FormattingEnabled = true;
      this.comboBoxRegionProvider.Location = new System.Drawing.Point(100, 33);
      this.comboBoxRegionProvider.Name = "comboBoxRegionProvider";
      this.comboBoxRegionProvider.Size = new System.Drawing.Size(366, 21);
      this.comboBoxRegionProvider.TabIndex = 3;
      // 
      // comboBoxCountry
      // 
      this.comboBoxCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCountry.FormattingEnabled = true;
      this.comboBoxCountry.Location = new System.Drawing.Point(100, 6);
      this.comboBoxCountry.Name = "comboBoxCountry";
      this.comboBoxCountry.Size = new System.Drawing.Size(366, 21);
      this.comboBoxCountry.TabIndex = 1;
      // 
      // CardDvbT
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.tabControl);
      this.Name = "CardDvbT";
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

    private MPTabControl tabControl;
    private MPTabPage tabPageScan;
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
  }
}