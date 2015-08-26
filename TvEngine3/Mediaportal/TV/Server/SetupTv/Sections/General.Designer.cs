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
      this.numericUpDownTimeLimitScan = new System.Windows.Forms.NumericUpDown();
      this.labelTimeLimitScanUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeLimitScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxScanAutomaticChannelGroupsChannelProviders = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.groupBoxScanning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonUpdateTuningDetails = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.checkBoxScanChannelMovementDetection = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxScanAutomaticChannelGroupsSatellites = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelScanAutomaticChannelGroups = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownTimeLimitSignal = new System.Windows.Forms.NumericUpDown();
      this.labelTimeLimitSignalUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeLimitSignal = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxGeneral.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTunerDetectionDelay)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitScan)).BeginInit();
      this.groupBoxScanning.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitSignal)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxGeneral
      // 
      this.groupBoxGeneral.Controls.Add(this.numericUpDownTimeLimitSignal);
      this.groupBoxGeneral.Controls.Add(this.labelTimeLimitSignalUnit);
      this.groupBoxGeneral.Controls.Add(this.labelTimeLimitSignal);
      this.groupBoxGeneral.Controls.Add(this.labelTunerDetectionDelayUnit);
      this.groupBoxGeneral.Controls.Add(this.labelTunerDetectionDelay);
      this.groupBoxGeneral.Controls.Add(this.numericUpDownTunerDetectionDelay);
      this.groupBoxGeneral.Controls.Add(this.labelServicePriority);
      this.groupBoxGeneral.Controls.Add(this.comboBoxServicePriority);
      this.groupBoxGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGeneral.Location = new System.Drawing.Point(3, 3);
      this.groupBoxGeneral.Name = "groupBoxGeneral";
      this.groupBoxGeneral.Size = new System.Drawing.Size(263, 105);
      this.groupBoxGeneral.TabIndex = 0;
      this.groupBoxGeneral.TabStop = false;
      this.groupBoxGeneral.Text = "General";
      // 
      // labelTunerDetectionDelayUnit
      // 
      this.labelTunerDetectionDelayUnit.AutoSize = true;
      this.labelTunerDetectionDelayUnit.Location = new System.Drawing.Point(197, 48);
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
      this.numericUpDownTunerDetectionDelay.Location = new System.Drawing.Point(131, 46);
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
      this.comboBoxServicePriority.Location = new System.Drawing.Point(131, 19);
      this.comboBoxServicePriority.Name = "comboBoxServicePriority";
      this.comboBoxServicePriority.Size = new System.Drawing.Size(120, 21);
      this.comboBoxServicePriority.TabIndex = 1;
      // 
      // numericUpDownTimeLimitScan
      // 
      this.numericUpDownTimeLimitScan.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitScan.Location = new System.Drawing.Point(65, 134);
      this.numericUpDownTimeLimitScan.Maximum = new decimal(new int[] {
            300000,
            0,
            0,
            0});
      this.numericUpDownTimeLimitScan.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitScan.Name = "numericUpDownTimeLimitScan";
      this.numericUpDownTimeLimitScan.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTimeLimitScan.TabIndex = 6;
      this.numericUpDownTimeLimitScan.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTimeLimitScan.Value = new decimal(new int[] {
            20000,
            0,
            0,
            0});
      // 
      // labelTimeLimitScanUnit
      // 
      this.labelTimeLimitScanUnit.AutoSize = true;
      this.labelTimeLimitScanUnit.Location = new System.Drawing.Point(131, 136);
      this.labelTimeLimitScanUnit.Name = "labelTimeLimitScanUnit";
      this.labelTimeLimitScanUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTimeLimitScanUnit.TabIndex = 7;
      this.labelTimeLimitScanUnit.Text = "ms";
      // 
      // labelTimeLimitScan
      // 
      this.labelTimeLimitScan.AutoSize = true;
      this.labelTimeLimitScan.Location = new System.Drawing.Point(6, 136);
      this.labelTimeLimitScan.Name = "labelTimeLimitScan";
      this.labelTimeLimitScan.Size = new System.Drawing.Size(53, 13);
      this.labelTimeLimitScan.TabIndex = 5;
      this.labelTimeLimitScan.Text = "Time limit:";
      // 
      // checkBoxScanAutomaticChannelGroupsBroadcastStandards
      // 
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.AutoSize = true;
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxScanAutomaticChannelGroupsBroadcastStandards.Location = new System.Drawing.Point(22, 84);
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
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.Location = new System.Drawing.Point(22, 61);
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.Name = "checkBoxScanAutomaticChannelGroupsChannelProviders";
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.Size = new System.Drawing.Size(130, 17);
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.TabIndex = 2;
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.Text = "each channel provider";
      this.checkBoxScanAutomaticChannelGroupsChannelProviders.UseVisualStyleBackColor = true;
      // 
      // groupBoxScanning
      // 
      this.groupBoxScanning.Controls.Add(this.numericUpDownTimeLimitScan);
      this.groupBoxScanning.Controls.Add(this.labelTimeLimitScanUnit);
      this.groupBoxScanning.Controls.Add(this.buttonUpdateTuningDetails);
      this.groupBoxScanning.Controls.Add(this.labelTimeLimitScan);
      this.groupBoxScanning.Controls.Add(this.checkBoxScanChannelMovementDetection);
      this.groupBoxScanning.Controls.Add(this.checkBoxScanAutomaticChannelGroupsSatellites);
      this.groupBoxScanning.Controls.Add(this.labelScanAutomaticChannelGroups);
      this.groupBoxScanning.Controls.Add(this.checkBoxScanAutomaticChannelGroupsBroadcastStandards);
      this.groupBoxScanning.Controls.Add(this.checkBoxScanAutomaticChannelGroupsChannelProviders);
      this.groupBoxScanning.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxScanning.Location = new System.Drawing.Point(3, 114);
      this.groupBoxScanning.Name = "groupBoxScanning";
      this.groupBoxScanning.Size = new System.Drawing.Size(220, 204);
      this.groupBoxScanning.TabIndex = 1;
      this.groupBoxScanning.TabStop = false;
      this.groupBoxScanning.Text = "Scanning";
      // 
      // buttonUpdateTuningDetails
      // 
      this.buttonUpdateTuningDetails.Location = new System.Drawing.Point(9, 165);
      this.buttonUpdateTuningDetails.Name = "buttonUpdateTuningDetails";
      this.buttonUpdateTuningDetails.Size = new System.Drawing.Size(120, 23);
      this.buttonUpdateTuningDetails.TabIndex = 8;
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
      this.checkBoxScanAutomaticChannelGroupsSatellites.Location = new System.Drawing.Point(22, 107);
      this.checkBoxScanAutomaticChannelGroupsSatellites.Name = "checkBoxScanAutomaticChannelGroupsSatellites";
      this.checkBoxScanAutomaticChannelGroupsSatellites.Size = new System.Drawing.Size(86, 17);
      this.checkBoxScanAutomaticChannelGroupsSatellites.TabIndex = 4;
      this.checkBoxScanAutomaticChannelGroupsSatellites.Text = "each satellite";
      this.checkBoxScanAutomaticChannelGroupsSatellites.UseVisualStyleBackColor = true;
      // 
      // labelScanAutomaticChannelGroups
      // 
      this.labelScanAutomaticChannelGroups.AutoSize = true;
      this.labelScanAutomaticChannelGroups.Location = new System.Drawing.Point(6, 45);
      this.labelScanAutomaticChannelGroups.Name = "labelScanAutomaticChannelGroups";
      this.labelScanAutomaticChannelGroups.Size = new System.Drawing.Size(202, 13);
      this.labelScanAutomaticChannelGroups.TabIndex = 1;
      this.labelScanAutomaticChannelGroups.Text = "Automatically create channel groups for...";
      // 
      // numericUpDownTimeLimitSignal
      // 
      this.numericUpDownTimeLimitSignal.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitSignal.Location = new System.Drawing.Point(131, 72);
      this.numericUpDownTimeLimitSignal.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
      this.numericUpDownTimeLimitSignal.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitSignal.Name = "numericUpDownTimeLimitSignal";
      this.numericUpDownTimeLimitSignal.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTimeLimitSignal.TabIndex = 6;
      this.numericUpDownTimeLimitSignal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTimeLimitSignal.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
      // 
      // labelTimeLimitSignalUnit
      // 
      this.labelTimeLimitSignalUnit.AutoSize = true;
      this.labelTimeLimitSignalUnit.Location = new System.Drawing.Point(197, 74);
      this.labelTimeLimitSignalUnit.Name = "labelTimeLimitSignalUnit";
      this.labelTimeLimitSignalUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTimeLimitSignalUnit.TabIndex = 7;
      this.labelTimeLimitSignalUnit.Text = "ms";
      // 
      // labelTimeLimitSignal
      // 
      this.labelTimeLimitSignal.AutoSize = true;
      this.labelTimeLimitSignal.Location = new System.Drawing.Point(6, 74);
      this.labelTimeLimitSignal.Name = "labelTimeLimitSignal";
      this.labelTimeLimitSignal.Size = new System.Drawing.Size(119, 13);
      this.labelTimeLimitSignal.TabIndex = 5;
      this.labelTimeLimitSignal.Text = "Wait-for-signal time limit:";
      // 
      // General
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.groupBoxScanning);
      this.Controls.Add(this.groupBoxGeneral);
      this.Name = "General";
      this.Size = new System.Drawing.Size(480, 420);
      this.groupBoxGeneral.ResumeLayout(false);
      this.groupBoxGeneral.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTunerDetectionDelay)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitScan)).EndInit();
      this.groupBoxScanning.ResumeLayout(false);
      this.groupBoxScanning.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitSignal)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MPComboBox comboBoxServicePriority;
    private MPLabel labelServicePriority;
    private MPGroupBox groupBoxGeneral;
    private System.Windows.Forms.NumericUpDown numericUpDownTimeLimitScan;
    private MPLabel labelTimeLimitScanUnit;
    private MPLabel labelTimeLimitScan;
    private MPCheckBox checkBoxScanAutomaticChannelGroupsBroadcastStandards;
    private MPCheckBox checkBoxScanAutomaticChannelGroupsChannelProviders;
    private MPLabel labelTunerDetectionDelayUnit;
    private MPLabel labelTunerDetectionDelay;
    private System.Windows.Forms.NumericUpDown numericUpDownTunerDetectionDelay;
    private MPGroupBox groupBoxScanning;
    private MPCheckBox checkBoxScanChannelMovementDetection;
    private MPCheckBox checkBoxScanAutomaticChannelGroupsSatellites;
    private MPLabel labelScanAutomaticChannelGroups;
    private MPButton buttonUpdateTuningDetails;
    private System.Windows.Forms.NumericUpDown numericUpDownTimeLimitSignal;
    private MPLabel labelTimeLimitSignalUnit;
    private MPLabel labelTimeLimitSignal;
  }
}