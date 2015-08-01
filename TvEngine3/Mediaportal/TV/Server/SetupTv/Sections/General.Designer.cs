using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class General
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
      this.groupBoxGeneral = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelTunerDetectionDelayUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTunerDetectionDelay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownTunerDetectionDelay = new System.Windows.Forms.NumericUpDown();
      this.labelServicePriority = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxServicePriority = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.groupBoxScanTuneTimeOuts = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.numericUpDownTimeOutScan = new System.Windows.Forms.NumericUpDown();
      this.labelTimeOutScanUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeOutScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownTimeOutProgramMapTable = new System.Windows.Forms.NumericUpDown();
      this.labelTimeOutProgramMapTableUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeOutProgramMapTable = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownTimeOutConditionalAccessTable = new System.Windows.Forms.NumericUpDown();
      this.labelTimeOutConditionalAccessTableUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeOutConditionalAccessTable = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownTimeOutSignal = new System.Windows.Forms.NumericUpDown();
      this.labelTimeOutSignalUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeOutSignal = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxScanAutomaticChannelGroupsChannelProviders = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.groupBoxScanning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonUpdateTuningDetails = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.checkBoxScanChannelMovementDetection = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxScanAutomaticChannelGroupsSatellites = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelScanAutomaticChannelGroups = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxGeneral.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTunerDetectionDelay)).BeginInit();
      this.groupBoxScanTuneTimeOuts.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOutScan)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOutProgramMapTable)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOutConditionalAccessTable)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOutSignal)).BeginInit();
      this.groupBoxScanning.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxGeneral
      // 
      this.groupBoxGeneral.Controls.Add(this.labelTunerDetectionDelayUnit);
      this.groupBoxGeneral.Controls.Add(this.labelTunerDetectionDelay);
      this.groupBoxGeneral.Controls.Add(this.numericUpDownTunerDetectionDelay);
      this.groupBoxGeneral.Controls.Add(this.labelServicePriority);
      this.groupBoxGeneral.Controls.Add(this.comboBoxServicePriority);
      this.groupBoxGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGeneral.Location = new System.Drawing.Point(3, 3);
      this.groupBoxGeneral.Name = "groupBoxGeneral";
      this.groupBoxGeneral.Size = new System.Drawing.Size(257, 76);
      this.groupBoxGeneral.TabIndex = 0;
      this.groupBoxGeneral.TabStop = false;
      this.groupBoxGeneral.Text = "General";
      // 
      // labelTunerDetectionDelayUnit
      // 
      this.labelTunerDetectionDelayUnit.AutoSize = true;
      this.labelTunerDetectionDelayUnit.Location = new System.Drawing.Point(191, 48);
      this.labelTunerDetectionDelayUnit.Name = "labelTunerDetectionDelayUnit";
      this.labelTunerDetectionDelayUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTunerDetectionDelayUnit.TabIndex = 4;
      this.labelTunerDetectionDelayUnit.Text = "ms";
      // 
      // labelTunerDetectionDelay
      // 
      this.labelTunerDetectionDelay.AutoSize = true;
      this.labelTunerDetectionDelay.Location = new System.Drawing.Point(6, 48);
      this.labelTunerDetectionDelay.Name = "labelTunerDetectionDelay";
      this.labelTunerDetectionDelay.Size = new System.Drawing.Size(113, 13);
      this.labelTunerDetectionDelay.TabIndex = 2;
      this.labelTunerDetectionDelay.Text = "Tuner detection delay:";
      // 
      // numericUpDownTunerDetectionDelay
      // 
      this.numericUpDownTunerDetectionDelay.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTunerDetectionDelay.Location = new System.Drawing.Point(125, 46);
      this.numericUpDownTunerDetectionDelay.Maximum = new decimal(new int[] {
            300000,
            0,
            0,
            0});
      this.numericUpDownTunerDetectionDelay.Name = "numericUpDownTunerDetectionDelay";
      this.numericUpDownTunerDetectionDelay.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTunerDetectionDelay.TabIndex = 3;
      this.numericUpDownTunerDetectionDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // labelServicePriority
      // 
      this.labelServicePriority.AutoSize = true;
      this.labelServicePriority.Location = new System.Drawing.Point(6, 22);
      this.labelServicePriority.Name = "labelServicePriority";
      this.labelServicePriority.Size = new System.Drawing.Size(79, 13);
      this.labelServicePriority.TabIndex = 0;
      this.labelServicePriority.Text = "Service priority:";
      // 
      // comboBoxServicePriority
      // 
      this.comboBoxServicePriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxServicePriority.FormattingEnabled = true;
      this.comboBoxServicePriority.Location = new System.Drawing.Point(125, 19);
      this.comboBoxServicePriority.Name = "comboBoxServicePriority";
      this.comboBoxServicePriority.Size = new System.Drawing.Size(120, 21);
      this.comboBoxServicePriority.TabIndex = 1;
      // 
      // groupBoxScanTuneTimeOuts
      // 
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.numericUpDownTimeOutScan);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.labelTimeOutScanUnit);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.labelTimeOutScan);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.numericUpDownTimeOutProgramMapTable);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.labelTimeOutProgramMapTableUnit);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.labelTimeOutProgramMapTable);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.numericUpDownTimeOutConditionalAccessTable);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.labelTimeOutConditionalAccessTableUnit);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.labelTimeOutConditionalAccessTable);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.numericUpDownTimeOutSignal);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.labelTimeOutSignalUnit);
      this.groupBoxScanTuneTimeOuts.Controls.Add(this.labelTimeOutSignal);
      this.groupBoxScanTuneTimeOuts.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxScanTuneTimeOuts.Location = new System.Drawing.Point(3, 85);
      this.groupBoxScanTuneTimeOuts.Name = "groupBoxScanTuneTimeOuts";
      this.groupBoxScanTuneTimeOuts.Size = new System.Drawing.Size(146, 157);
      this.groupBoxScanTuneTimeOuts.TabIndex = 1;
      this.groupBoxScanTuneTimeOuts.TabStop = false;
      this.groupBoxScanTuneTimeOuts.Text = "Scan/tune time-outs";
      // 
      // numericUpDownTimeOutScan
      // 
      this.numericUpDownTimeOutScan.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeOutScan.Location = new System.Drawing.Point(51, 97);
      this.numericUpDownTimeOutScan.Maximum = new decimal(new int[] {
            300000,
            0,
            0,
            0});
      this.numericUpDownTimeOutScan.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeOutScan.Name = "numericUpDownTimeOutScan";
      this.numericUpDownTimeOutScan.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTimeOutScan.TabIndex = 10;
      this.numericUpDownTimeOutScan.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTimeOutScan.Value = new decimal(new int[] {
            20000,
            0,
            0,
            0});
      // 
      // labelTimeOutScanUnit
      // 
      this.labelTimeOutScanUnit.AutoSize = true;
      this.labelTimeOutScanUnit.Location = new System.Drawing.Point(117, 99);
      this.labelTimeOutScanUnit.Name = "labelTimeOutScanUnit";
      this.labelTimeOutScanUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTimeOutScanUnit.TabIndex = 11;
      this.labelTimeOutScanUnit.Text = "ms";
      // 
      // labelTimeOutScan
      // 
      this.labelTimeOutScan.AutoSize = true;
      this.labelTimeOutScan.Location = new System.Drawing.Point(6, 99);
      this.labelTimeOutScan.Name = "labelTimeOutScan";
      this.labelTimeOutScan.Size = new System.Drawing.Size(35, 13);
      this.labelTimeOutScan.TabIndex = 9;
      this.labelTimeOutScan.Text = "Scan:";
      // 
      // numericUpDownTimeOutProgramMapTable
      // 
      this.numericUpDownTimeOutProgramMapTable.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeOutProgramMapTable.Location = new System.Drawing.Point(51, 71);
      this.numericUpDownTimeOutProgramMapTable.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
      this.numericUpDownTimeOutProgramMapTable.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeOutProgramMapTable.Name = "numericUpDownTimeOutProgramMapTable";
      this.numericUpDownTimeOutProgramMapTable.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTimeOutProgramMapTable.TabIndex = 7;
      this.numericUpDownTimeOutProgramMapTable.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTimeOutProgramMapTable.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
      // 
      // labelTimeOutProgramMapTableUnit
      // 
      this.labelTimeOutProgramMapTableUnit.AutoSize = true;
      this.labelTimeOutProgramMapTableUnit.Location = new System.Drawing.Point(117, 73);
      this.labelTimeOutProgramMapTableUnit.Name = "labelTimeOutProgramMapTableUnit";
      this.labelTimeOutProgramMapTableUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTimeOutProgramMapTableUnit.TabIndex = 8;
      this.labelTimeOutProgramMapTableUnit.Text = "ms";
      // 
      // labelTimeOutProgramMapTable
      // 
      this.labelTimeOutProgramMapTable.AutoSize = true;
      this.labelTimeOutProgramMapTable.Location = new System.Drawing.Point(6, 73);
      this.labelTimeOutProgramMapTable.Name = "labelTimeOutProgramMapTable";
      this.labelTimeOutProgramMapTable.Size = new System.Drawing.Size(33, 13);
      this.labelTimeOutProgramMapTable.TabIndex = 6;
      this.labelTimeOutProgramMapTable.Text = "PMT:";
      // 
      // numericUpDownTimeOutConditionalAccessTable
      // 
      this.numericUpDownTimeOutConditionalAccessTable.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeOutConditionalAccessTable.Location = new System.Drawing.Point(51, 45);
      this.numericUpDownTimeOutConditionalAccessTable.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
      this.numericUpDownTimeOutConditionalAccessTable.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeOutConditionalAccessTable.Name = "numericUpDownTimeOutConditionalAccessTable";
      this.numericUpDownTimeOutConditionalAccessTable.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTimeOutConditionalAccessTable.TabIndex = 4;
      this.numericUpDownTimeOutConditionalAccessTable.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTimeOutConditionalAccessTable.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
      // 
      // labelTimeOutConditionalAccessTableUnit
      // 
      this.labelTimeOutConditionalAccessTableUnit.AutoSize = true;
      this.labelTimeOutConditionalAccessTableUnit.Location = new System.Drawing.Point(117, 47);
      this.labelTimeOutConditionalAccessTableUnit.Name = "labelTimeOutConditionalAccessTableUnit";
      this.labelTimeOutConditionalAccessTableUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTimeOutConditionalAccessTableUnit.TabIndex = 5;
      this.labelTimeOutConditionalAccessTableUnit.Text = "ms";
      // 
      // labelTimeOutConditionalAccessTable
      // 
      this.labelTimeOutConditionalAccessTable.AutoSize = true;
      this.labelTimeOutConditionalAccessTable.Location = new System.Drawing.Point(6, 47);
      this.labelTimeOutConditionalAccessTable.Name = "labelTimeOutConditionalAccessTable";
      this.labelTimeOutConditionalAccessTable.Size = new System.Drawing.Size(31, 13);
      this.labelTimeOutConditionalAccessTable.TabIndex = 3;
      this.labelTimeOutConditionalAccessTable.Text = "CAT:";
      // 
      // numericUpDownTimeOutSignal
      // 
      this.numericUpDownTimeOutSignal.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeOutSignal.Location = new System.Drawing.Point(51, 19);
      this.numericUpDownTimeOutSignal.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
      this.numericUpDownTimeOutSignal.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeOutSignal.Name = "numericUpDownTimeOutSignal";
      this.numericUpDownTimeOutSignal.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTimeOutSignal.TabIndex = 1;
      this.numericUpDownTimeOutSignal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTimeOutSignal.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
      // 
      // labelTimeOutSignalUnit
      // 
      this.labelTimeOutSignalUnit.AutoSize = true;
      this.labelTimeOutSignalUnit.Location = new System.Drawing.Point(117, 21);
      this.labelTimeOutSignalUnit.Name = "labelTimeOutSignalUnit";
      this.labelTimeOutSignalUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTimeOutSignalUnit.TabIndex = 2;
      this.labelTimeOutSignalUnit.Text = "ms";
      // 
      // labelTimeOutSignal
      // 
      this.labelTimeOutSignal.AutoSize = true;
      this.labelTimeOutSignal.Location = new System.Drawing.Point(6, 21);
      this.labelTimeOutSignal.Name = "labelTimeOutSignal";
      this.labelTimeOutSignal.Size = new System.Drawing.Size(39, 13);
      this.labelTimeOutSignal.TabIndex = 0;
      this.labelTimeOutSignal.Text = "Signal:";
      // 
      // checkBoxScanAutomaticChannelGroupsBroadcastStandards
      // 
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.AutoSize = true;
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.Location = new System.Drawing.Point(22, 78);
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.Name = "checkBoxScanAutomaticChannelGroupsBroadcastStandards";
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.Size = new System.Drawing.Size(142, 17);
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.TabIndex = 3;
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.Text = "each broadcast standard";
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.UseVisualStyleBackColor = true;
      // 
      // checkBoxScanAutomaticChannelGroupsChannelProviders
      // 
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.AutoSize = true;
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.Location = new System.Drawing.Point(22, 55);
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.Name = "checkBoxScanAutomaticChannelGroupsChannelProviders";
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.Size = new System.Drawing.Size(130, 17);
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.TabIndex = 2;
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.Text = "each channel provider";
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.UseVisualStyleBackColor = true;
      // 
      // groupBoxScanning
      // 
      this.groupBoxScanning.Controls.Add(this.buttonUpdateTuningDetails);
      this.groupBoxScanning.Controls.Add(this.checkBoxScanChannelMovementDetection);
      this.groupBoxScanning.Controls.Add(this.checkBoxScanAutomaticChannelGroupsSatellites);
      this.groupBoxScanning.Controls.Add(this.labelScanAutomaticChannelGroups);
      this.groupBoxScanning.Controls.Add(this.checkBoxScanAutomaticChannelGroupsBroadcastStandards);
      this.groupBoxScanning.Controls.Add(this.checkBoxScanAutomaticChannelGroupsChannelProviders);
      this.groupBoxScanning.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxScanning.Location = new System.Drawing.Point(155, 85);
      this.groupBoxScanning.Name = "groupBoxScanning";
      this.groupBoxScanning.Size = new System.Drawing.Size(220, 157);
      this.groupBoxScanning.TabIndex = 2;
      this.groupBoxScanning.TabStop = false;
      this.groupBoxScanning.Text = "Scanning";
      // 
      // buttonUpdateTuningDetails
      // 
      this.buttonUpdateTuningDetails.Location = new System.Drawing.Point(9, 124);
      this.buttonUpdateTuningDetails.Name = "buttonUpdateTuningDetails";
      this.buttonUpdateTuningDetails.Size = new System.Drawing.Size(120, 23);
      this.buttonUpdateTuningDetails.TabIndex = 5;
      this.buttonUpdateTuningDetails.Text = "Update tuning details";
      this.buttonUpdateTuningDetails.UseVisualStyleBackColor = true;
      this.buttonUpdateTuningDetails.Click += new System.EventHandler(this.buttonUpdateTuningDetails_Click);
      // 
      // checkBoxScanChannelMovementDetection
      // 
      this.checkBoxScanChannelMovementDetection.AutoSize = true;
      this.checkBoxScanChannelMovementDetection.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxScanChannelMovementDetection.Location = new System.Drawing.Point(9, 19);
      this.checkBoxScanChannelMovementDetection.Name = "checkBoxScanChannelMovementDetection";
      this.checkBoxScanChannelMovementDetection.Size = new System.Drawing.Size(200, 17);
      this.checkBoxScanChannelMovementDetection.TabIndex = 0;
      this.checkBoxScanChannelMovementDetection.Text = "Enable channel movement detection.";
      this.checkBoxScanChannelMovementDetection.UseVisualStyleBackColor = true;
      // 
      // checkBoxScanAutomaticChannelGroupsSatellites
      // 
      this.checkBoxScanAutomaticChannelGroupsSatellites.AutoSize = true;
      this.checkBoxScanAutomaticChannelGroupsSatellites.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxScanAutomaticChannelGroupsSatellites.Location = new System.Drawing.Point(22, 101);
      this.checkBoxScanAutomaticChannelGroupsSatellites.Name = "checkBoxScanAutomaticChannelGroupsSatellites";
      this.checkBoxScanAutomaticChannelGroupsSatellites.Size = new System.Drawing.Size(86, 17);
      this.checkBoxScanAutomaticChannelGroupsSatellites.TabIndex = 4;
      this.checkBoxScanAutomaticChannelGroupsSatellites.Text = "each satellite";
      this.checkBoxScanAutomaticChannelGroupsSatellites.UseVisualStyleBackColor = true;
      // 
      // labelScanAutomaticChannelGroups
      // 
      this.labelScanAutomaticChannelGroups.AutoSize = true;
      this.labelScanAutomaticChannelGroups.Location = new System.Drawing.Point(6, 39);
      this.labelScanAutomaticChannelGroups.Name = "labelScanAutomaticChannelGroups";
      this.labelScanAutomaticChannelGroups.Size = new System.Drawing.Size(202, 13);
      this.labelScanAutomaticChannelGroups.TabIndex = 1;
      this.labelScanAutomaticChannelGroups.Text = "Automatically create channel groups for...";
      // 
      // General
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.groupBoxScanning);
      this.Controls.Add(this.groupBoxGeneral);
      this.Controls.Add(this.groupBoxScanTuneTimeOuts);
      this.Name = "General";
      this.Size = new System.Drawing.Size(480, 420);
      this.groupBoxGeneral.ResumeLayout(false);
      this.groupBoxGeneral.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTunerDetectionDelay)).EndInit();
      this.groupBoxScanTuneTimeOuts.ResumeLayout(false);
      this.groupBoxScanTuneTimeOuts.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOutScan)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOutProgramMapTable)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOutConditionalAccessTable)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOutSignal)).EndInit();
      this.groupBoxScanning.ResumeLayout(false);
      this.groupBoxScanning.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MPComboBox comboBoxServicePriority;
    private MPLabel labelServicePriority;
    private MPGroupBox groupBoxGeneral;
    private MPGroupBox groupBoxScanTuneTimeOuts;
    private System.Windows.Forms.NumericUpDown numericUpDownTimeOutScan;
    private MPLabel labelTimeOutScanUnit;
    private MPLabel labelTimeOutScan;
    private System.Windows.Forms.NumericUpDown numericUpDownTimeOutProgramMapTable;
    private MPLabel labelTimeOutProgramMapTableUnit;
    private MPLabel labelTimeOutProgramMapTable;
    private System.Windows.Forms.NumericUpDown numericUpDownTimeOutConditionalAccessTable;
    private MPLabel labelTimeOutConditionalAccessTableUnit;
    private MPLabel labelTimeOutConditionalAccessTable;
    private MPCheckBox checkBoxScanAutomaticChannelGroupsBroadcastStandards;
    private MPCheckBox checkBoxScanAutomaticChannelGroupsChannelProviders;
    private System.Windows.Forms.NumericUpDown numericUpDownTimeOutSignal;
    private MPLabel labelTimeOutSignalUnit;
    private MPLabel labelTimeOutSignal;
    private MPLabel labelTunerDetectionDelayUnit;
    private MPLabel labelTunerDetectionDelay;
    private System.Windows.Forms.NumericUpDown numericUpDownTunerDetectionDelay;
    private MPGroupBox groupBoxScanning;
    private MPCheckBox checkBoxScanChannelMovementDetection;
    private MPCheckBox checkBoxScanAutomaticChannelGroupsSatellites;
    private MPLabel labelScanAutomaticChannelGroups;
    private MPButton buttonUpdateTuningDetails;
  }
}