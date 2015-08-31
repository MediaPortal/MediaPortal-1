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
      this.numericUpDownTimeLimitSignalLock = new System.Windows.Forms.NumericUpDown();
      this.labelTimeLimitSignalLockUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeLimitSignalLock = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
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
      this.groupBoxPreviewCodecs = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.comboBoxPreviewCodecAudio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelPreviewCodecAudio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxPreviewCodecVideo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelPreviewCodecVideo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownTimeLimitReceiveStream = new System.Windows.Forms.NumericUpDown();
      this.labelTimeLimitReceiveStreamUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeLimitReceiveStream = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxTimeLimits = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.groupBoxGeneral.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTunerDetectionDelay)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitSignalLock)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitScan)).BeginInit();
      this.groupBoxScanning.SuspendLayout();
      this.groupBoxPreviewCodecs.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitReceiveStream)).BeginInit();
      this.groupBoxTimeLimits.SuspendLayout();
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
      this.groupBoxGeneral.Size = new System.Drawing.Size(263, 76);
      this.groupBoxGeneral.TabIndex = 0;
      this.groupBoxGeneral.TabStop = false;
      this.groupBoxGeneral.Text = "General";
      // 
      // labelTunerDetectionDelayUnit
      // 
      this.labelTunerDetectionDelayUnit.AutoSize = true;
      this.labelTunerDetectionDelayUnit.Location = new System.Drawing.Point(187, 48);
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
      // numericUpDownTimeLimitSignalLock
      // 
      this.numericUpDownTimeLimitSignalLock.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitSignalLock.Location = new System.Drawing.Point(93, 19);
      this.numericUpDownTimeLimitSignalLock.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
      this.numericUpDownTimeLimitSignalLock.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitSignalLock.Name = "numericUpDownTimeLimitSignalLock";
      this.numericUpDownTimeLimitSignalLock.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTimeLimitSignalLock.TabIndex = 1;
      this.numericUpDownTimeLimitSignalLock.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTimeLimitSignalLock.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
      // 
      // labelTimeLimitSignalLockUnit
      // 
      this.labelTimeLimitSignalLockUnit.AutoSize = true;
      this.labelTimeLimitSignalLockUnit.Location = new System.Drawing.Point(155, 21);
      this.labelTimeLimitSignalLockUnit.Name = "labelTimeLimitSignalLockUnit";
      this.labelTimeLimitSignalLockUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTimeLimitSignalLockUnit.TabIndex = 2;
      this.labelTimeLimitSignalLockUnit.Text = "ms";
      // 
      // labelTimeLimitSignalLock
      // 
      this.labelTimeLimitSignalLock.AutoSize = true;
      this.labelTimeLimitSignalLock.Location = new System.Drawing.Point(6, 21);
      this.labelTimeLimitSignalLock.Name = "labelTimeLimitSignalLock";
      this.labelTimeLimitSignalLock.Size = new System.Drawing.Size(77, 13);
      this.labelTimeLimitSignalLock.TabIndex = 0;
      this.labelTimeLimitSignalLock.Text = "Wait for signal:";
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
      this.labelTimeLimitScanUnit.Location = new System.Drawing.Point(127, 136);
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
      this.groupBoxScanning.Location = new System.Drawing.Point(3, 165);
      this.groupBoxScanning.Name = "groupBoxScanning";
      this.groupBoxScanning.Size = new System.Drawing.Size(221, 204);
      this.groupBoxScanning.TabIndex = 2;
      this.groupBoxScanning.TabStop = false;
      this.groupBoxScanning.Text = "Scanning";
      // 
      // buttonUpdateTuningDetails
      // 
      this.buttonUpdateTuningDetails.Location = new System.Drawing.Point(9, 165);
      this.buttonUpdateTuningDetails.Name = "buttonUpdateTuningDetails";
      this.buttonUpdateTuningDetails.Size = new System.Drawing.Size(130, 23);
      this.buttonUpdateTuningDetails.TabIndex = 8;
      this.buttonUpdateTuningDetails.Text = "&Update Tuning Details";
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
      // groupBoxPreviewCodecs
      // 
      this.groupBoxPreviewCodecs.Controls.Add(this.comboBoxPreviewCodecAudio);
      this.groupBoxPreviewCodecs.Controls.Add(this.labelPreviewCodecAudio);
      this.groupBoxPreviewCodecs.Controls.Add(this.comboBoxPreviewCodecVideo);
      this.groupBoxPreviewCodecs.Controls.Add(this.labelPreviewCodecVideo);
      this.groupBoxPreviewCodecs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxPreviewCodecs.Location = new System.Drawing.Point(3, 85);
      this.groupBoxPreviewCodecs.Name = "groupBoxPreviewCodecs";
      this.groupBoxPreviewCodecs.Size = new System.Drawing.Size(414, 78);
      this.groupBoxPreviewCodecs.TabIndex = 1;
      this.groupBoxPreviewCodecs.TabStop = false;
      this.groupBoxPreviewCodecs.Text = "Preview Codecs";
      // 
      // comboBoxPreviewCodecAudio
      // 
      this.comboBoxPreviewCodecAudio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPreviewCodecAudio.FormattingEnabled = true;
      this.comboBoxPreviewCodecAudio.Location = new System.Drawing.Point(49, 46);
      this.comboBoxPreviewCodecAudio.MaxDropDownItems = 20;
      this.comboBoxPreviewCodecAudio.Name = "comboBoxPreviewCodecAudio";
      this.comboBoxPreviewCodecAudio.Size = new System.Drawing.Size(353, 21);
      this.comboBoxPreviewCodecAudio.TabIndex = 3;
      // 
      // labelPreviewCodecAudio
      // 
      this.labelPreviewCodecAudio.AutoSize = true;
      this.labelPreviewCodecAudio.Location = new System.Drawing.Point(6, 48);
      this.labelPreviewCodecAudio.Name = "labelPreviewCodecAudio";
      this.labelPreviewCodecAudio.Size = new System.Drawing.Size(37, 13);
      this.labelPreviewCodecAudio.TabIndex = 2;
      this.labelPreviewCodecAudio.Text = "Audio:";
      // 
      // comboBoxPreviewCodecVideo
      // 
      this.comboBoxPreviewCodecVideo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPreviewCodecVideo.FormattingEnabled = true;
      this.comboBoxPreviewCodecVideo.Location = new System.Drawing.Point(49, 19);
      this.comboBoxPreviewCodecVideo.MaxDropDownItems = 20;
      this.comboBoxPreviewCodecVideo.Name = "comboBoxPreviewCodecVideo";
      this.comboBoxPreviewCodecVideo.Size = new System.Drawing.Size(353, 21);
      this.comboBoxPreviewCodecVideo.TabIndex = 1;
      // 
      // labelPreviewCodecVideo
      // 
      this.labelPreviewCodecVideo.AutoSize = true;
      this.labelPreviewCodecVideo.Location = new System.Drawing.Point(6, 21);
      this.labelPreviewCodecVideo.Name = "labelPreviewCodecVideo";
      this.labelPreviewCodecVideo.Size = new System.Drawing.Size(37, 13);
      this.labelPreviewCodecVideo.TabIndex = 0;
      this.labelPreviewCodecVideo.Text = "Video:";
      // 
      // numericUpDownTimeLimitReceiveStream
      // 
      this.numericUpDownTimeLimitReceiveStream.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitReceiveStream.Location = new System.Drawing.Point(93, 45);
      this.numericUpDownTimeLimitReceiveStream.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
      this.numericUpDownTimeLimitReceiveStream.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitReceiveStream.Name = "numericUpDownTimeLimitReceiveStream";
      this.numericUpDownTimeLimitReceiveStream.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTimeLimitReceiveStream.TabIndex = 4;
      this.numericUpDownTimeLimitReceiveStream.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTimeLimitReceiveStream.Value = new decimal(new int[] {
            7500,
            0,
            0,
            0});
      // 
      // labelTimeLimitReceiveStreamUnit
      // 
      this.labelTimeLimitReceiveStreamUnit.AutoSize = true;
      this.labelTimeLimitReceiveStreamUnit.Location = new System.Drawing.Point(155, 47);
      this.labelTimeLimitReceiveStreamUnit.Name = "labelTimeLimitReceiveStreamUnit";
      this.labelTimeLimitReceiveStreamUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTimeLimitReceiveStreamUnit.TabIndex = 5;
      this.labelTimeLimitReceiveStreamUnit.Text = "ms";
      // 
      // labelTimeLimitReceiveStream
      // 
      this.labelTimeLimitReceiveStream.AutoSize = true;
      this.labelTimeLimitReceiveStream.Location = new System.Drawing.Point(6, 47);
      this.labelTimeLimitReceiveStream.Name = "labelTimeLimitReceiveStream";
      this.labelTimeLimitReceiveStream.Size = new System.Drawing.Size(81, 13);
      this.labelTimeLimitReceiveStream.TabIndex = 3;
      this.labelTimeLimitReceiveStream.Text = "Wait for stream:";
      // 
      // groupBoxTimeLimits
      // 
      this.groupBoxTimeLimits.Controls.Add(this.numericUpDownTimeLimitReceiveStream);
      this.groupBoxTimeLimits.Controls.Add(this.labelTimeLimitReceiveStreamUnit);
      this.groupBoxTimeLimits.Controls.Add(this.labelTimeLimitSignalLock);
      this.groupBoxTimeLimits.Controls.Add(this.labelTimeLimitReceiveStream);
      this.groupBoxTimeLimits.Controls.Add(this.labelTimeLimitSignalLockUnit);
      this.groupBoxTimeLimits.Controls.Add(this.numericUpDownTimeLimitSignalLock);
      this.groupBoxTimeLimits.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxTimeLimits.Location = new System.Drawing.Point(230, 165);
      this.groupBoxTimeLimits.Name = "groupBoxTimeLimits";
      this.groupBoxTimeLimits.Size = new System.Drawing.Size(187, 78);
      this.groupBoxTimeLimits.TabIndex = 3;
      this.groupBoxTimeLimits.TabStop = false;
      this.groupBoxTimeLimits.Text = "Tuning Time Limits";
      // 
      // General
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.groupBoxTimeLimits);
      this.Controls.Add(this.groupBoxPreviewCodecs);
      this.Controls.Add(this.groupBoxScanning);
      this.Controls.Add(this.groupBoxGeneral);
      this.Name = "General";
      this.Size = new System.Drawing.Size(480, 420);
      this.groupBoxGeneral.ResumeLayout(false);
      this.groupBoxGeneral.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTunerDetectionDelay)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitSignalLock)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitScan)).EndInit();
      this.groupBoxScanning.ResumeLayout(false);
      this.groupBoxScanning.PerformLayout();
      this.groupBoxPreviewCodecs.ResumeLayout(false);
      this.groupBoxPreviewCodecs.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitReceiveStream)).EndInit();
      this.groupBoxTimeLimits.ResumeLayout(false);
      this.groupBoxTimeLimits.PerformLayout();
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
    private System.Windows.Forms.NumericUpDown numericUpDownTimeLimitSignalLock;
    private MPLabel labelTimeLimitSignalLockUnit;
    private MPLabel labelTimeLimitSignalLock;
    private MPGroupBox groupBoxPreviewCodecs;
    private MPComboBox comboBoxPreviewCodecAudio;
    private MPLabel labelPreviewCodecAudio;
    private MPComboBox comboBoxPreviewCodecVideo;
    private MPLabel labelPreviewCodecVideo;
    private System.Windows.Forms.NumericUpDown numericUpDownTimeLimitReceiveStream;
    private MPLabel labelTimeLimitReceiveStreamUnit;
    private MPLabel labelTimeLimitReceiveStream;
    private MPGroupBox groupBoxTimeLimits;
  }
}