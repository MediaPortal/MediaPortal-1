using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEditTuningDetailCable
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
      this.comboBoxModulation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelModulation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxSymbolRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelSymbolRate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequencyUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSymbolRateUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxOpenTvChannelId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelOpenTvChannelId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxEpgSource = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelEpgOriginalNetworkId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxEpgServiceId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxEpgTransportStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxEpgOriginalNetworkId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelEpgTransportStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelEpgServiceId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxPmtPid = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelOriginalNetworkId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxServiceId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxTransportStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxOriginalNetworkId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelPmtPid = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTransportStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelServiceId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelBroadcastStandard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).BeginInit();
      this.groupBoxEpgSource.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(313, 280);
      this.buttonCancel.TabIndex = 34;
      // 
      // buttonOkay
      // 
      this.buttonOkay.Location = new System.Drawing.Point(218, 280);
      this.buttonOkay.TabIndex = 33;
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
      this.textBoxName.Size = new System.Drawing.Size(75, 20);
      // 
      // checkBoxIsEncrypted
      // 
      this.checkBoxIsEncrypted.Location = new System.Drawing.Point(123, 91);
      // 
      // textBoxProvider
      // 
      this.textBoxProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxProvider.Size = new System.Drawing.Size(75, 20);
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
      // comboBoxModulation
      // 
      this.comboBoxModulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxModulation.FormattingEnabled = true;
      this.comboBoxModulation.Location = new System.Drawing.Point(123, 219);
      this.comboBoxModulation.Name = "comboBoxModulation";
      this.comboBoxModulation.Size = new System.Drawing.Size(75, 21);
      this.comboBoxModulation.TabIndex = 18;
      // 
      // labelModulation
      // 
      this.labelModulation.AutoSize = true;
      this.labelModulation.Location = new System.Drawing.Point(12, 222);
      this.labelModulation.Name = "labelModulation";
      this.labelModulation.Size = new System.Drawing.Size(62, 13);
      this.labelModulation.TabIndex = 17;
      this.labelModulation.Text = "Modulation:";
      // 
      // numericTextBoxSymbolRate
      // 
      this.numericTextBoxSymbolRate.Location = new System.Drawing.Point(123, 246);
      this.numericTextBoxSymbolRate.MaximumValue = 9999;
      this.numericTextBoxSymbolRate.MaxLength = 4;
      this.numericTextBoxSymbolRate.MinimumValue = 100;
      this.numericTextBoxSymbolRate.Name = "numericTextBoxSymbolRate";
      this.numericTextBoxSymbolRate.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxSymbolRate.TabIndex = 20;
      this.numericTextBoxSymbolRate.Text = "6875";
      this.numericTextBoxSymbolRate.Value = 6875;
      // 
      // numericTextBoxFrequency
      // 
      this.numericTextBoxFrequency.Location = new System.Drawing.Point(123, 193);
      this.numericTextBoxFrequency.MaximumValue = 999999;
      this.numericTextBoxFrequency.MaxLength = 6;
      this.numericTextBoxFrequency.MinimumValue = 50000;
      this.numericTextBoxFrequency.Name = "numericTextBoxFrequency";
      this.numericTextBoxFrequency.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxFrequency.TabIndex = 15;
      this.numericTextBoxFrequency.Text = "388000";
      this.numericTextBoxFrequency.Value = 388000;
      // 
      // labelSymbolRate
      // 
      this.labelSymbolRate.AutoSize = true;
      this.labelSymbolRate.Location = new System.Drawing.Point(12, 249);
      this.labelSymbolRate.Name = "labelSymbolRate";
      this.labelSymbolRate.Size = new System.Drawing.Size(65, 13);
      this.labelSymbolRate.TabIndex = 19;
      this.labelSymbolRate.Text = "Symbol rate:";
      // 
      // labelFrequency
      // 
      this.labelFrequency.AutoSize = true;
      this.labelFrequency.Location = new System.Drawing.Point(12, 196);
      this.labelFrequency.Name = "labelFrequency";
      this.labelFrequency.Size = new System.Drawing.Size(60, 13);
      this.labelFrequency.TabIndex = 14;
      this.labelFrequency.Text = "Frequency:";
      // 
      // labelFrequencyUnit
      // 
      this.labelFrequencyUnit.AutoSize = true;
      this.labelFrequencyUnit.Location = new System.Drawing.Point(174, 196);
      this.labelFrequencyUnit.Name = "labelFrequencyUnit";
      this.labelFrequencyUnit.Size = new System.Drawing.Size(26, 13);
      this.labelFrequencyUnit.TabIndex = 16;
      this.labelFrequencyUnit.Text = "kHz";
      // 
      // labelSymbolRateUnit
      // 
      this.labelSymbolRateUnit.AutoSize = true;
      this.labelSymbolRateUnit.Location = new System.Drawing.Point(174, 249);
      this.labelSymbolRateUnit.Name = "labelSymbolRateUnit";
      this.labelSymbolRateUnit.Size = new System.Drawing.Size(28, 13);
      this.labelSymbolRateUnit.TabIndex = 21;
      this.labelSymbolRateUnit.Text = "ks/s";
      // 
      // numericTextBoxOpenTvChannelId
      // 
      this.numericTextBoxOpenTvChannelId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxOpenTvChannelId.Location = new System.Drawing.Point(339, 90);
      this.numericTextBoxOpenTvChannelId.MaximumValue = 65535;
      this.numericTextBoxOpenTvChannelId.MaxLength = 5;
      this.numericTextBoxOpenTvChannelId.MinimumValue = 0;
      this.numericTextBoxOpenTvChannelId.Name = "numericTextBoxOpenTvChannelId";
      this.numericTextBoxOpenTvChannelId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxOpenTvChannelId.TabIndex = 29;
      this.numericTextBoxOpenTvChannelId.Text = "0";
      this.numericTextBoxOpenTvChannelId.Value = 0;
      // 
      // labelOpenTvChannelId
      // 
      this.labelOpenTvChannelId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelOpenTvChannelId.AutoSize = true;
      this.labelOpenTvChannelId.Location = new System.Drawing.Point(230, 93);
      this.labelOpenTvChannelId.Name = "labelOpenTvChannelId";
      this.labelOpenTvChannelId.Size = new System.Drawing.Size(105, 13);
      this.labelOpenTvChannelId.TabIndex = 28;
      this.labelOpenTvChannelId.Text = "OpenTV channel ID:";
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
      this.groupBoxEpgSource.Location = new System.Drawing.Point(221, 147);
      this.groupBoxEpgSource.Name = "groupBoxEpgSource";
      this.groupBoxEpgSource.Size = new System.Drawing.Size(167, 103);
      this.groupBoxEpgSource.TabIndex = 32;
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
      // numericTextBoxPmtPid
      // 
      this.numericTextBoxPmtPid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxPmtPid.Location = new System.Drawing.Point(339, 116);
      this.numericTextBoxPmtPid.MaximumValue = 65535;
      this.numericTextBoxPmtPid.MaxLength = 5;
      this.numericTextBoxPmtPid.MinimumValue = 0;
      this.numericTextBoxPmtPid.Name = "numericTextBoxPmtPid";
      this.numericTextBoxPmtPid.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxPmtPid.TabIndex = 31;
      this.numericTextBoxPmtPid.Text = "0";
      this.numericTextBoxPmtPid.Value = 0;
      // 
      // labelOriginalNetworkId
      // 
      this.labelOriginalNetworkId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelOriginalNetworkId.AutoSize = true;
      this.labelOriginalNetworkId.Location = new System.Drawing.Point(230, 15);
      this.labelOriginalNetworkId.Name = "labelOriginalNetworkId";
      this.labelOriginalNetworkId.Size = new System.Drawing.Size(100, 13);
      this.labelOriginalNetworkId.TabIndex = 22;
      this.labelOriginalNetworkId.Text = "Original network ID:";
      // 
      // numericTextBoxServiceId
      // 
      this.numericTextBoxServiceId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxServiceId.Location = new System.Drawing.Point(339, 64);
      this.numericTextBoxServiceId.MaximumValue = 65535;
      this.numericTextBoxServiceId.MaxLength = 5;
      this.numericTextBoxServiceId.MinimumValue = 0;
      this.numericTextBoxServiceId.Name = "numericTextBoxServiceId";
      this.numericTextBoxServiceId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxServiceId.TabIndex = 27;
      this.numericTextBoxServiceId.Text = "0";
      this.numericTextBoxServiceId.Value = 0;
      // 
      // numericTextBoxTransportStreamId
      // 
      this.numericTextBoxTransportStreamId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxTransportStreamId.Location = new System.Drawing.Point(339, 38);
      this.numericTextBoxTransportStreamId.MaximumValue = 65535;
      this.numericTextBoxTransportStreamId.MaxLength = 5;
      this.numericTextBoxTransportStreamId.MinimumValue = 0;
      this.numericTextBoxTransportStreamId.Name = "numericTextBoxTransportStreamId";
      this.numericTextBoxTransportStreamId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxTransportStreamId.TabIndex = 25;
      this.numericTextBoxTransportStreamId.Text = "0";
      this.numericTextBoxTransportStreamId.Value = 0;
      // 
      // numericTextBoxOriginalNetworkId
      // 
      this.numericTextBoxOriginalNetworkId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxOriginalNetworkId.Location = new System.Drawing.Point(339, 12);
      this.numericTextBoxOriginalNetworkId.MaximumValue = 65535;
      this.numericTextBoxOriginalNetworkId.MaxLength = 5;
      this.numericTextBoxOriginalNetworkId.MinimumValue = 0;
      this.numericTextBoxOriginalNetworkId.Name = "numericTextBoxOriginalNetworkId";
      this.numericTextBoxOriginalNetworkId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxOriginalNetworkId.TabIndex = 23;
      this.numericTextBoxOriginalNetworkId.Text = "0";
      this.numericTextBoxOriginalNetworkId.Value = 0;
      // 
      // labelPmtPid
      // 
      this.labelPmtPid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelPmtPid.AutoSize = true;
      this.labelPmtPid.Location = new System.Drawing.Point(230, 119);
      this.labelPmtPid.Name = "labelPmtPid";
      this.labelPmtPid.Size = new System.Drawing.Size(54, 13);
      this.labelPmtPid.TabIndex = 30;
      this.labelPmtPid.Text = "PMT PID:";
      // 
      // labelTransportStreamId
      // 
      this.labelTransportStreamId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelTransportStreamId.AutoSize = true;
      this.labelTransportStreamId.Location = new System.Drawing.Point(230, 41);
      this.labelTransportStreamId.Name = "labelTransportStreamId";
      this.labelTransportStreamId.Size = new System.Drawing.Size(103, 13);
      this.labelTransportStreamId.TabIndex = 24;
      this.labelTransportStreamId.Text = "Transport stream ID:";
      // 
      // labelServiceId
      // 
      this.labelServiceId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelServiceId.AutoSize = true;
      this.labelServiceId.Location = new System.Drawing.Point(230, 67);
      this.labelServiceId.Name = "labelServiceId";
      this.labelServiceId.Size = new System.Drawing.Size(60, 13);
      this.labelServiceId.TabIndex = 26;
      this.labelServiceId.Text = "Service ID:";
      // 
      // comboBoxBroadcastStandard
      // 
      this.comboBoxBroadcastStandard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxBroadcastStandard.FormattingEnabled = true;
      this.comboBoxBroadcastStandard.Location = new System.Drawing.Point(123, 166);
      this.comboBoxBroadcastStandard.Name = "comboBoxBroadcastStandard";
      this.comboBoxBroadcastStandard.Size = new System.Drawing.Size(75, 21);
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
      // FormEditTuningDetailDvbC
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(400, 315);
      this.Controls.Add(this.comboBoxBroadcastStandard);
      this.Controls.Add(this.labelBroadcastStandard);
      this.Controls.Add(this.numericTextBoxOpenTvChannelId);
      this.Controls.Add(this.labelOpenTvChannelId);
      this.Controls.Add(this.groupBoxEpgSource);
      this.Controls.Add(this.numericTextBoxPmtPid);
      this.Controls.Add(this.labelOriginalNetworkId);
      this.Controls.Add(this.numericTextBoxServiceId);
      this.Controls.Add(this.numericTextBoxTransportStreamId);
      this.Controls.Add(this.numericTextBoxOriginalNetworkId);
      this.Controls.Add(this.labelPmtPid);
      this.Controls.Add(this.labelTransportStreamId);
      this.Controls.Add(this.labelServiceId);
      this.Controls.Add(this.labelSymbolRateUnit);
      this.Controls.Add(this.labelFrequencyUnit);
      this.Controls.Add(this.comboBoxModulation);
      this.Controls.Add(this.labelModulation);
      this.Controls.Add(this.numericTextBoxSymbolRate);
      this.Controls.Add(this.labelFrequency);
      this.Controls.Add(this.numericTextBoxFrequency);
      this.Controls.Add(this.labelSymbolRate);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(408, 341);
      this.Name = "FormEditTuningDetailDvbC";
      this.Text = "Add/Edit Cable Tuning Detail";
      this.Controls.SetChildIndex(this.checkBoxIsHighDefinition, 0);
      this.Controls.SetChildIndex(this.labelIsHighDefinition, 0);
      this.Controls.SetChildIndex(this.checkBoxIsThreeDimensional, 0);
      this.Controls.SetChildIndex(this.labelIsThreeDimensional, 0);
      this.Controls.SetChildIndex(this.labelIsEncrypted, 0);
      this.Controls.SetChildIndex(this.checkBoxIsEncrypted, 0);
      this.Controls.SetChildIndex(this.labelProvider, 0);
      this.Controls.SetChildIndex(this.labelName, 0);
      this.Controls.SetChildIndex(this.channelNumberUpDownNumber, 0);
      this.Controls.SetChildIndex(this.labelNumber, 0);
      this.Controls.SetChildIndex(this.textBoxProvider, 0);
      this.Controls.SetChildIndex(this.textBoxName, 0);
      this.Controls.SetChildIndex(this.labelSymbolRate, 0);
      this.Controls.SetChildIndex(this.numericTextBoxFrequency, 0);
      this.Controls.SetChildIndex(this.labelFrequency, 0);
      this.Controls.SetChildIndex(this.numericTextBoxSymbolRate, 0);
      this.Controls.SetChildIndex(this.labelModulation, 0);
      this.Controls.SetChildIndex(this.comboBoxModulation, 0);
      this.Controls.SetChildIndex(this.buttonOkay, 0);
      this.Controls.SetChildIndex(this.labelFrequencyUnit, 0);
      this.Controls.SetChildIndex(this.labelSymbolRateUnit, 0);
      this.Controls.SetChildIndex(this.buttonCancel, 0);
      this.Controls.SetChildIndex(this.labelServiceId, 0);
      this.Controls.SetChildIndex(this.labelTransportStreamId, 0);
      this.Controls.SetChildIndex(this.labelPmtPid, 0);
      this.Controls.SetChildIndex(this.numericTextBoxOriginalNetworkId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxTransportStreamId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxServiceId, 0);
      this.Controls.SetChildIndex(this.labelOriginalNetworkId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxPmtPid, 0);
      this.Controls.SetChildIndex(this.groupBoxEpgSource, 0);
      this.Controls.SetChildIndex(this.labelOpenTvChannelId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxOpenTvChannelId, 0);
      this.Controls.SetChildIndex(this.labelBroadcastStandard, 0);
      this.Controls.SetChildIndex(this.comboBoxBroadcastStandard, 0);
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).EndInit();
      this.groupBoxEpgSource.ResumeLayout(false);
      this.groupBoxEpgSource.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPComboBox comboBoxModulation;
    private MPLabel labelModulation;
    private MPNumericTextBox numericTextBoxSymbolRate;
    private MPNumericTextBox numericTextBoxFrequency;
    private MPLabel labelSymbolRate;
    private MPLabel labelFrequency;
    private MPLabel labelFrequencyUnit;
    private MPLabel labelSymbolRateUnit;
    private MPNumericTextBox numericTextBoxOpenTvChannelId;
    private MPLabel labelOpenTvChannelId;
    private MPGroupBox groupBoxEpgSource;
    private MPLabel labelEpgOriginalNetworkId;
    private MPNumericTextBox numericTextBoxEpgServiceId;
    private MPNumericTextBox numericTextBoxEpgTransportStreamId;
    private MPNumericTextBox numericTextBoxEpgOriginalNetworkId;
    private MPLabel labelEpgTransportStreamId;
    private MPLabel labelEpgServiceId;
    private MPNumericTextBox numericTextBoxPmtPid;
    private MPLabel labelOriginalNetworkId;
    private MPNumericTextBox numericTextBoxServiceId;
    private MPNumericTextBox numericTextBoxTransportStreamId;
    private MPNumericTextBox numericTextBoxOriginalNetworkId;
    private MPLabel labelPmtPid;
    private MPLabel labelTransportStreamId;
    private MPLabel labelServiceId;
    private MPComboBox comboBoxBroadcastStandard;
    private MPLabel labelBroadcastStandard;
  }
}
