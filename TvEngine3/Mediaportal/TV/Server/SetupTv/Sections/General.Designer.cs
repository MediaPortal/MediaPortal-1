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
      this.labelServicePriority = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxServicePriority = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.numericUpDownTimeLimitSignalLock = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelTimeLimitSignalLockUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeLimitSignalLock = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxPreviewCodecs = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.comboBoxPreviewCodecAudio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelPreviewCodecAudio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxPreviewCodecVideo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelPreviewCodecVideo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownTimeLimitReceiveStreamInfo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelTimeLimitReceiveStreamInfoUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeLimitReceiveStreamInfo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxTimeLimits = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.numericUpDownTimeLimitReceiveVideoAudio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelTimeLimitReceiveVideoAudioUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTimeLimitReceiveVideoAudio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelStreamTunerPorts = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownStreamTunerPortMinimum = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.numericUpDownStreamTunerPortMaximum = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelStreamTunerPortsSeparator = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxGeneral.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitSignalLock)).BeginInit();
      this.groupBoxPreviewCodecs.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitReceiveStreamInfo)).BeginInit();
      this.groupBoxTimeLimits.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitReceiveVideoAudio)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStreamTunerPortMinimum)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStreamTunerPortMaximum)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxGeneral
      // 
      this.groupBoxGeneral.Controls.Add(this.labelStreamTunerPortsSeparator);
      this.groupBoxGeneral.Controls.Add(this.numericUpDownStreamTunerPortMaximum);
      this.groupBoxGeneral.Controls.Add(this.numericUpDownStreamTunerPortMinimum);
      this.groupBoxGeneral.Controls.Add(this.labelStreamTunerPorts);
      this.groupBoxGeneral.Controls.Add(this.labelServicePriority);
      this.groupBoxGeneral.Controls.Add(this.comboBoxServicePriority);
      this.groupBoxGeneral.Location = new System.Drawing.Point(3, 3);
      this.groupBoxGeneral.Name = "groupBoxGeneral";
      this.groupBoxGeneral.Size = new System.Drawing.Size(280, 77);
      this.groupBoxGeneral.TabIndex = 0;
      this.groupBoxGeneral.TabStop = false;
      this.groupBoxGeneral.Text = "General";
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
      this.comboBoxServicePriority.FormattingEnabled = true;
      this.comboBoxServicePriority.Location = new System.Drawing.Point(125, 19);
      this.comboBoxServicePriority.Name = "comboBoxServicePriority";
      this.comboBoxServicePriority.Size = new System.Drawing.Size(140, 21);
      this.comboBoxServicePriority.TabIndex = 1;
      // 
      // numericUpDownTimeLimitSignalLock
      // 
      this.numericUpDownTimeLimitSignalLock.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitSignalLock.Location = new System.Drawing.Point(125, 19);
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
      this.numericUpDownTimeLimitSignalLock.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
      // 
      // labelTimeLimitSignalLockUnit
      // 
      this.labelTimeLimitSignalLockUnit.AutoSize = true;
      this.labelTimeLimitSignalLockUnit.Location = new System.Drawing.Point(187, 21);
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
      // groupBoxPreviewCodecs
      // 
      this.groupBoxPreviewCodecs.Controls.Add(this.comboBoxPreviewCodecAudio);
      this.groupBoxPreviewCodecs.Controls.Add(this.labelPreviewCodecAudio);
      this.groupBoxPreviewCodecs.Controls.Add(this.comboBoxPreviewCodecVideo);
      this.groupBoxPreviewCodecs.Controls.Add(this.labelPreviewCodecVideo);
      this.groupBoxPreviewCodecs.Location = new System.Drawing.Point(3, 86);
      this.groupBoxPreviewCodecs.Name = "groupBoxPreviewCodecs";
      this.groupBoxPreviewCodecs.Size = new System.Drawing.Size(414, 78);
      this.groupBoxPreviewCodecs.TabIndex = 1;
      this.groupBoxPreviewCodecs.TabStop = false;
      this.groupBoxPreviewCodecs.Text = "Preview Codecs";
      // 
      // comboBoxPreviewCodecAudio
      // 
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
      // numericUpDownTimeLimitReceiveStreamInfo
      // 
      this.numericUpDownTimeLimitReceiveStreamInfo.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitReceiveStreamInfo.Location = new System.Drawing.Point(125, 45);
      this.numericUpDownTimeLimitReceiveStreamInfo.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
      this.numericUpDownTimeLimitReceiveStreamInfo.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitReceiveStreamInfo.Name = "numericUpDownTimeLimitReceiveStreamInfo";
      this.numericUpDownTimeLimitReceiveStreamInfo.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTimeLimitReceiveStreamInfo.TabIndex = 4;
      this.numericUpDownTimeLimitReceiveStreamInfo.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
      // 
      // labelTimeLimitReceiveStreamInfoUnit
      // 
      this.labelTimeLimitReceiveStreamInfoUnit.AutoSize = true;
      this.labelTimeLimitReceiveStreamInfoUnit.Location = new System.Drawing.Point(187, 47);
      this.labelTimeLimitReceiveStreamInfoUnit.Name = "labelTimeLimitReceiveStreamInfoUnit";
      this.labelTimeLimitReceiveStreamInfoUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTimeLimitReceiveStreamInfoUnit.TabIndex = 5;
      this.labelTimeLimitReceiveStreamInfoUnit.Text = "ms";
      // 
      // labelTimeLimitReceiveStreamInfo
      // 
      this.labelTimeLimitReceiveStreamInfo.AutoSize = true;
      this.labelTimeLimitReceiveStreamInfo.Location = new System.Drawing.Point(6, 47);
      this.labelTimeLimitReceiveStreamInfo.Name = "labelTimeLimitReceiveStreamInfo";
      this.labelTimeLimitReceiveStreamInfo.Size = new System.Drawing.Size(101, 13);
      this.labelTimeLimitReceiveStreamInfo.TabIndex = 3;
      this.labelTimeLimitReceiveStreamInfo.Text = "Wait for stream info:";
      // 
      // groupBoxTimeLimits
      // 
      this.groupBoxTimeLimits.Controls.Add(this.numericUpDownTimeLimitReceiveVideoAudio);
      this.groupBoxTimeLimits.Controls.Add(this.labelTimeLimitReceiveVideoAudioUnit);
      this.groupBoxTimeLimits.Controls.Add(this.labelTimeLimitReceiveVideoAudio);
      this.groupBoxTimeLimits.Controls.Add(this.numericUpDownTimeLimitReceiveStreamInfo);
      this.groupBoxTimeLimits.Controls.Add(this.labelTimeLimitReceiveStreamInfoUnit);
      this.groupBoxTimeLimits.Controls.Add(this.labelTimeLimitSignalLock);
      this.groupBoxTimeLimits.Controls.Add(this.labelTimeLimitReceiveStreamInfo);
      this.groupBoxTimeLimits.Controls.Add(this.labelTimeLimitSignalLockUnit);
      this.groupBoxTimeLimits.Controls.Add(this.numericUpDownTimeLimitSignalLock);
      this.groupBoxTimeLimits.Location = new System.Drawing.Point(3, 170);
      this.groupBoxTimeLimits.Name = "groupBoxTimeLimits";
      this.groupBoxTimeLimits.Size = new System.Drawing.Size(280, 104);
      this.groupBoxTimeLimits.TabIndex = 2;
      this.groupBoxTimeLimits.TabStop = false;
      this.groupBoxTimeLimits.Text = "Tuning Time Limits";
      // 
      // numericUpDownTimeLimitReceiveVideoAudio
      // 
      this.numericUpDownTimeLimitReceiveVideoAudio.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitReceiveVideoAudio.Location = new System.Drawing.Point(125, 71);
      this.numericUpDownTimeLimitReceiveVideoAudio.Maximum = new decimal(new int[] {
            600000,
            0,
            0,
            0});
      this.numericUpDownTimeLimitReceiveVideoAudio.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownTimeLimitReceiveVideoAudio.Name = "numericUpDownTimeLimitReceiveVideoAudio";
      this.numericUpDownTimeLimitReceiveVideoAudio.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTimeLimitReceiveVideoAudio.TabIndex = 7;
      this.numericUpDownTimeLimitReceiveVideoAudio.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
      // 
      // labelTimeLimitReceiveVideoAudioUnit
      // 
      this.labelTimeLimitReceiveVideoAudioUnit.AutoSize = true;
      this.labelTimeLimitReceiveVideoAudioUnit.Location = new System.Drawing.Point(187, 73);
      this.labelTimeLimitReceiveVideoAudioUnit.Name = "labelTimeLimitReceiveVideoAudioUnit";
      this.labelTimeLimitReceiveVideoAudioUnit.Size = new System.Drawing.Size(20, 13);
      this.labelTimeLimitReceiveVideoAudioUnit.TabIndex = 8;
      this.labelTimeLimitReceiveVideoAudioUnit.Text = "ms";
      // 
      // labelTimeLimitReceiveVideoAudio
      // 
      this.labelTimeLimitReceiveVideoAudio.AutoSize = true;
      this.labelTimeLimitReceiveVideoAudio.Location = new System.Drawing.Point(6, 73);
      this.labelTimeLimitReceiveVideoAudio.Name = "labelTimeLimitReceiveVideoAudio";
      this.labelTimeLimitReceiveVideoAudio.Size = new System.Drawing.Size(107, 13);
      this.labelTimeLimitReceiveVideoAudio.TabIndex = 6;
      this.labelTimeLimitReceiveVideoAudio.Text = "Wait for video/audio:";
      // 
      // labelStreamTunerPorts
      // 
      this.labelStreamTunerPorts.AutoSize = true;
      this.labelStreamTunerPorts.Location = new System.Drawing.Point(6, 48);
      this.labelStreamTunerPorts.Name = "labelStreamTunerPorts";
      this.labelStreamTunerPorts.Size = new System.Drawing.Size(96, 13);
      this.labelStreamTunerPorts.TabIndex = 2;
      this.labelStreamTunerPorts.Text = "Stream tuner ports:";
      // 
      // numericUpDownStreamTunerPortMinimum
      // 
      this.numericUpDownStreamTunerPortMinimum.Location = new System.Drawing.Point(125, 46);
      this.numericUpDownStreamTunerPortMinimum.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
      this.numericUpDownStreamTunerPortMinimum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownStreamTunerPortMinimum.Name = "numericUpDownStreamTunerPortMinimum";
      this.numericUpDownStreamTunerPortMinimum.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownStreamTunerPortMinimum.TabIndex = 3;
      this.numericUpDownStreamTunerPortMinimum.Value = new decimal(new int[] {
            49152,
            0,
            0,
            0});
      this.numericUpDownStreamTunerPortMinimum.ValueChanged += new System.EventHandler(this.numericUpDownStreamTunerPortMinimum_ValueChanged);
      // 
      // numericUpDownStreamTunerPortMaximum
      // 
      this.numericUpDownStreamTunerPortMaximum.Location = new System.Drawing.Point(205, 46);
      this.numericUpDownStreamTunerPortMaximum.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
      this.numericUpDownStreamTunerPortMaximum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownStreamTunerPortMaximum.Name = "numericUpDownStreamTunerPortMaximum";
      this.numericUpDownStreamTunerPortMaximum.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownStreamTunerPortMaximum.TabIndex = 5;
      this.numericUpDownStreamTunerPortMaximum.Value = new decimal(new int[] {
            65535,
            0,
            0,
            0});
      this.numericUpDownStreamTunerPortMaximum.ValueChanged += new System.EventHandler(this.numericUpDownStreamTunerPortMaximum_ValueChanged);
      // 
      // labelStreamTunerPortsSeparator
      // 
      this.labelStreamTunerPortsSeparator.AutoSize = true;
      this.labelStreamTunerPortsSeparator.Location = new System.Drawing.Point(187, 48);
      this.labelStreamTunerPortsSeparator.Name = "labelStreamTunerPortsSeparator";
      this.labelStreamTunerPortsSeparator.Size = new System.Drawing.Size(16, 13);
      this.labelStreamTunerPortsSeparator.TabIndex = 4;
      this.labelStreamTunerPortsSeparator.Text = "to";
      // 
      // General
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.groupBoxTimeLimits);
      this.Controls.Add(this.groupBoxPreviewCodecs);
      this.Controls.Add(this.groupBoxGeneral);
      this.Name = "General";
      this.Size = new System.Drawing.Size(480, 420);
      this.groupBoxGeneral.ResumeLayout(false);
      this.groupBoxGeneral.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitSignalLock)).EndInit();
      this.groupBoxPreviewCodecs.ResumeLayout(false);
      this.groupBoxPreviewCodecs.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitReceiveStreamInfo)).EndInit();
      this.groupBoxTimeLimits.ResumeLayout(false);
      this.groupBoxTimeLimits.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeLimitReceiveVideoAudio)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStreamTunerPortMinimum)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStreamTunerPortMaximum)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MPComboBox comboBoxServicePriority;
    private MPLabel labelServicePriority;
    private MPGroupBox groupBoxGeneral;
    private MPNumericUpDown numericUpDownTimeLimitSignalLock;
    private MPLabel labelTimeLimitSignalLockUnit;
    private MPLabel labelTimeLimitSignalLock;
    private MPGroupBox groupBoxPreviewCodecs;
    private MPComboBox comboBoxPreviewCodecAudio;
    private MPLabel labelPreviewCodecAudio;
    private MPComboBox comboBoxPreviewCodecVideo;
    private MPLabel labelPreviewCodecVideo;
    private MPNumericUpDown numericUpDownTimeLimitReceiveStreamInfo;
    private MPLabel labelTimeLimitReceiveStreamInfoUnit;
    private MPLabel labelTimeLimitReceiveStreamInfo;
    private MPGroupBox groupBoxTimeLimits;
    private MPNumericUpDown numericUpDownTimeLimitReceiveVideoAudio;
    private MPLabel labelTimeLimitReceiveVideoAudioUnit;
    private MPLabel labelTimeLimitReceiveVideoAudio;
    private MPNumericUpDown numericUpDownStreamTunerPortMinimum;
    private MPLabel labelStreamTunerPorts;
    private MPLabel labelStreamTunerPortsSeparator;
    private MPNumericUpDown numericUpDownStreamTunerPortMaximum;
  }
}