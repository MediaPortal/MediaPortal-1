using TvLibrary.Interfaces;
using System;
namespace SetupTv.Sections
{
  partial class FormEditCard
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
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.numericUpDownDecryptLimit = new System.Windows.Forms.NumericUpDown();
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxConditionalAccessSettings = new System.Windows.Forms.GroupBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxMultiChannelDecryptMode = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxCamType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxConditionalAccessEnabled = new System.Windows.Forms.CheckBox();
      this.checkBoxAllowEpgGrab = new System.Windows.Forms.CheckBox();
      this.checkBoxPreloadCard = new System.Windows.Forms.CheckBox();
      this.groupBoxAdvancedSettings = new System.Windows.Forms.GroupBox();
      this.checkBoxUseCustomTuning = new System.Windows.Forms.CheckBox();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxNetworkProvider = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpComboBoxPidFilterMode = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxIdleMode = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTextBoxDeviceName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxGeneralSettings = new System.Windows.Forms.GroupBox();
      this.groupBoxDiseqcSettings = new System.Windows.Forms.GroupBox();
      this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.numericUpDownDiseqcCommandRepeatCount = new System.Windows.Forms.NumericUpDown();
      this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxAlwaysSendDiseqcCommands = new System.Windows.Forms.CheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDecryptLimit)).BeginInit();
      this.groupBoxConditionalAccessSettings.SuspendLayout();
      this.groupBoxAdvancedSettings.SuspendLayout();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.groupBoxDiseqcSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiseqcCommandRepeatCount)).BeginInit();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(14, 43);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(354, 29);
      this.label1.TabIndex = 7;
      this.label1.Text = "If your device has a CAM, please specify the number of channels that the CAM can " +
          "decrypt simultaneously.";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(14, 82);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(121, 13);
      this.label3.TabIndex = 8;
      this.label3.Text = "This device can decrypt";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(175, 82);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(130, 13);
      this.label4.TabIndex = 10;
      this.label4.Text = "channel(s) simultaneously.";
      // 
      // mpButtonSave
      // 
      this.mpButtonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.mpButtonSave.Location = new System.Drawing.Point(249, 521);
      this.mpButtonSave.Name = "mpButtonSave";
      this.mpButtonSave.Size = new System.Drawing.Size(75, 23);
      this.mpButtonSave.TabIndex = 30;
      this.mpButtonSave.Text = "Save";
      this.mpButtonSave.UseVisualStyleBackColor = true;
      this.mpButtonSave.Click += new System.EventHandler(this.mpButtonSave_Click);
      // 
      // numericUpDownDecryptLimit
      // 
      this.numericUpDownDecryptLimit.Location = new System.Drawing.Point(136, 80);
      this.numericUpDownDecryptLimit.Name = "numericUpDownDecryptLimit";
      this.numericUpDownDecryptLimit.Size = new System.Drawing.Size(38, 20);
      this.numericUpDownDecryptLimit.TabIndex = 9;
      this.numericUpDownDecryptLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownDecryptLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(330, 521);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 31;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.mpButtonCancel_Click);
      // 
      // groupBoxConditionalAccessSettings
      // 
      this.groupBoxConditionalAccessSettings.Controls.Add(this.mpLabel3);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.mpComboBoxMultiChannelDecryptMode);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.label5);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.mpComboBoxCamType);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.checkBoxConditionalAccessEnabled);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.label1);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.label3);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.label4);
      this.groupBoxConditionalAccessSettings.Controls.Add(this.numericUpDownDecryptLimit);
      this.groupBoxConditionalAccessSettings.Location = new System.Drawing.Point(22, 90);
      this.groupBoxConditionalAccessSettings.Name = "groupBoxConditionalAccessSettings";
      this.groupBoxConditionalAccessSettings.Size = new System.Drawing.Size(383, 168);
      this.groupBoxConditionalAccessSettings.TabIndex = 5;
      this.groupBoxConditionalAccessSettings.TabStop = false;
      this.groupBoxConditionalAccessSettings.Text = "Conditional Access Settings";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(15, 109);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(140, 13);
      this.mpLabel3.TabIndex = 11;
      this.mpLabel3.Text = "Multi-channel decrypt mode:";
      // 
      // mpComboBoxMultiChannelDecryptMode
      // 
      this.mpComboBoxMultiChannelDecryptMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxMultiChannelDecryptMode.FormattingEnabled = true;
      this.mpComboBoxMultiChannelDecryptMode.Items.AddRange(new object[] {
            "Disabled",
            "List",
            "Changes"});
      this.mpComboBoxMultiChannelDecryptMode.Location = new System.Drawing.Point(159, 107);
      this.mpComboBoxMultiChannelDecryptMode.Name = "mpComboBoxMultiChannelDecryptMode";
      this.mpComboBoxMultiChannelDecryptMode.Size = new System.Drawing.Size(103, 21);
      this.mpComboBoxMultiChannelDecryptMode.TabIndex = 12;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(15, 137);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(64, 13);
      this.label5.TabIndex = 13;
      this.label5.Text = "CAM model:";
      // 
      // mpComboBoxCamType
      // 
      this.mpComboBoxCamType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCamType.FormattingEnabled = true;
      this.mpComboBoxCamType.Items.AddRange(new object[] {
            "Default",
            "Astoncrypt2"});
      this.mpComboBoxCamType.Location = new System.Drawing.Point(159, 134);
      this.mpComboBoxCamType.Name = "mpComboBoxCamType";
      this.mpComboBoxCamType.Size = new System.Drawing.Size(103, 21);
      this.mpComboBoxCamType.TabIndex = 14;
      // 
      // checkBoxConditionalAccessEnabled
      // 
      this.checkBoxConditionalAccessEnabled.AutoSize = true;
      this.checkBoxConditionalAccessEnabled.Location = new System.Drawing.Point(17, 19);
      this.checkBoxConditionalAccessEnabled.Name = "checkBoxConditionalAccessEnabled";
      this.checkBoxConditionalAccessEnabled.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxConditionalAccessEnabled.Size = new System.Drawing.Size(189, 17);
      this.checkBoxConditionalAccessEnabled.TabIndex = 6;
      this.checkBoxConditionalAccessEnabled.Text = "This device can decrypt channels.";
      this.checkBoxConditionalAccessEnabled.UseVisualStyleBackColor = true;
      this.checkBoxConditionalAccessEnabled.CheckedChanged += new System.EventHandler(this.checkBoxCAMenabled_CheckedChanged);
      // 
      // checkBoxAllowEpgGrab
      // 
      this.checkBoxAllowEpgGrab.AutoSize = true;
      this.checkBoxAllowEpgGrab.Location = new System.Drawing.Point(17, 45);
      this.checkBoxAllowEpgGrab.Name = "checkBoxAllowEpgGrab";
      this.checkBoxAllowEpgGrab.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxAllowEpgGrab.Size = new System.Drawing.Size(245, 17);
      this.checkBoxAllowEpgGrab.TabIndex = 4;
      this.checkBoxAllowEpgGrab.Text = "Allow this device to be used for EPG grabbing.";
      this.checkBoxAllowEpgGrab.UseVisualStyleBackColor = true;
      // 
      // checkBoxPreloadCard
      // 
      this.checkBoxPreloadCard.AutoSize = true;
      this.checkBoxPreloadCard.Location = new System.Drawing.Point(17, 121);
      this.checkBoxPreloadCard.Name = "checkBoxPreloadCard";
      this.checkBoxPreloadCard.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxPreloadCard.Size = new System.Drawing.Size(188, 17);
      this.checkBoxPreloadCard.TabIndex = 28;
      this.checkBoxPreloadCard.Text = "Allow this device to be pre-loaded.";
      this.checkBoxPreloadCard.UseVisualStyleBackColor = true;
      // 
      // groupBoxAdvancedSettings
      // 
      this.groupBoxAdvancedSettings.Controls.Add(this.checkBoxUseCustomTuning);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpLabel6);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpLabel5);
      this.groupBoxAdvancedSettings.Controls.Add(this.checkBoxPreloadCard);
      this.groupBoxAdvancedSettings.Controls.Add(this.comboBoxNetworkProvider);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpComboBoxPidFilterMode);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpLabel1);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpComboBoxIdleMode);
      this.groupBoxAdvancedSettings.Controls.Add(this.mpLabel4);
      this.groupBoxAdvancedSettings.Location = new System.Drawing.Point(23, 341);
      this.groupBoxAdvancedSettings.Name = "groupBoxAdvancedSettings";
      this.groupBoxAdvancedSettings.Size = new System.Drawing.Size(383, 174);
      this.groupBoxAdvancedSettings.TabIndex = 20;
      this.groupBoxAdvancedSettings.TabStop = false;
      this.groupBoxAdvancedSettings.Text = "Advanced Settings";
      // 
      // checkBoxUseCustomTuning
      // 
      this.checkBoxUseCustomTuning.AutoSize = true;
      this.checkBoxUseCustomTuning.Location = new System.Drawing.Point(17, 144);
      this.checkBoxUseCustomTuning.Name = "checkBoxUseCustomTuning";
      this.checkBoxUseCustomTuning.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxUseCustomTuning.Size = new System.Drawing.Size(201, 17);
      this.checkBoxUseCustomTuning.TabIndex = 29;
      this.checkBoxUseCustomTuning.Text = "Use direct/custom tuning if available.";
      this.checkBoxUseCustomTuning.UseVisualStyleBackColor = true;
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(14, 70);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(91, 13);
      this.mpLabel6.TabIndex = 24;
      this.mpLabel6.Text = "Network provider:";
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(14, 97);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(79, 13);
      this.mpLabel5.TabIndex = 26;
      this.mpLabel5.Text = "PID filter mode:";
      // 
      // comboBoxNetworkProvider
      // 
      this.comboBoxNetworkProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxNetworkProvider.FormattingEnabled = true;
      this.comboBoxNetworkProvider.Location = new System.Drawing.Point(158, 67);
      this.comboBoxNetworkProvider.Name = "comboBoxNetworkProvider";
      this.comboBoxNetworkProvider.Size = new System.Drawing.Size(103, 21);
      this.comboBoxNetworkProvider.TabIndex = 25;
      // 
      // mpComboBoxPidFilterMode
      // 
      this.mpComboBoxPidFilterMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxPidFilterMode.FormattingEnabled = true;
      this.mpComboBoxPidFilterMode.Items.AddRange(new object[] {
            "Disabled",
            "Enabled",
            "Auto"});
      this.mpComboBoxPidFilterMode.Location = new System.Drawing.Point(158, 94);
      this.mpComboBoxPidFilterMode.Name = "mpComboBoxPidFilterMode";
      this.mpComboBoxPidFilterMode.Size = new System.Drawing.Size(103, 21);
      this.mpComboBoxPidFilterMode.TabIndex = 27;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(14, 16);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(355, 13);
      this.mpLabel1.TabIndex = 21;
      this.mpLabel1.Text = "Please read the documentation in our wiki before changing these settings.";
      // 
      // mpComboBoxIdleMode
      // 
      this.mpComboBoxIdleMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxIdleMode.FormattingEnabled = true;
      this.mpComboBoxIdleMode.Items.AddRange(new object[] {
            "Pause",
            "Stop",
            "Unload",
            "AlwaysOn"});
      this.mpComboBoxIdleMode.Location = new System.Drawing.Point(158, 40);
      this.mpComboBoxIdleMode.Name = "mpComboBoxIdleMode";
      this.mpComboBoxIdleMode.Size = new System.Drawing.Size(103, 21);
      this.mpComboBoxIdleMode.TabIndex = 23;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(14, 43);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(56, 13);
      this.mpLabel4.TabIndex = 22;
      this.mpLabel4.Text = "Idle mode:";
      // 
      // mpTextBoxDeviceName
      // 
      this.mpTextBoxDeviceName.Location = new System.Drawing.Point(96, 19);
      this.mpTextBoxDeviceName.Name = "mpTextBoxDeviceName";
      this.mpTextBoxDeviceName.Size = new System.Drawing.Size(272, 20);
      this.mpTextBoxDeviceName.TabIndex = 3;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(15, 22);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(75, 13);
      this.mpLabel2.TabIndex = 2;
      this.mpLabel2.Text = "Device Name:";
      // 
      // groupBoxGeneralSettings
      // 
      this.groupBoxGeneralSettings.Controls.Add(this.mpLabel2);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxAllowEpgGrab);
      this.groupBoxGeneralSettings.Controls.Add(this.mpTextBoxDeviceName);
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(22, 12);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(383, 72);
      this.groupBoxGeneralSettings.TabIndex = 1;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "General Settings";
      // 
      // groupBoxDiseqcSettings
      // 
      this.groupBoxDiseqcSettings.Controls.Add(this.mpLabel7);
      this.groupBoxDiseqcSettings.Controls.Add(this.numericUpDownDiseqcCommandRepeatCount);
      this.groupBoxDiseqcSettings.Controls.Add(this.mpLabel8);
      this.groupBoxDiseqcSettings.Controls.Add(this.checkBoxAlwaysSendDiseqcCommands);
      this.groupBoxDiseqcSettings.Location = new System.Drawing.Point(23, 264);
      this.groupBoxDiseqcSettings.Name = "groupBoxDiseqcSettings";
      this.groupBoxDiseqcSettings.Size = new System.Drawing.Size(383, 68);
      this.groupBoxDiseqcSettings.TabIndex = 15;
      this.groupBoxDiseqcSettings.TabStop = false;
      this.groupBoxDiseqcSettings.Text = "DiSEqC Settings";
      // 
      // mpLabel7
      // 
      this.mpLabel7.AutoSize = true;
      this.mpLabel7.Location = new System.Drawing.Point(158, 44);
      this.mpLabel7.Name = "mpLabel7";
      this.mpLabel7.Size = new System.Drawing.Size(40, 13);
      this.mpLabel7.TabIndex = 19;
      this.mpLabel7.Text = "time(s).";
      // 
      // numericUpDownDiseqcCommandRepeatCount
      // 
      this.numericUpDownDiseqcCommandRepeatCount.Location = new System.Drawing.Point(117, 42);
      this.numericUpDownDiseqcCommandRepeatCount.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.numericUpDownDiseqcCommandRepeatCount.Name = "numericUpDownDiseqcCommandRepeatCount";
      this.numericUpDownDiseqcCommandRepeatCount.Size = new System.Drawing.Size(38, 20);
      this.numericUpDownDiseqcCommandRepeatCount.TabIndex = 18;
      this.numericUpDownDiseqcCommandRepeatCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // mpLabel8
      // 
      this.mpLabel8.AutoSize = true;
      this.mpLabel8.Location = new System.Drawing.Point(13, 44);
      this.mpLabel8.Name = "mpLabel8";
      this.mpLabel8.Size = new System.Drawing.Size(102, 13);
      this.mpLabel8.TabIndex = 17;
      this.mpLabel8.Text = "Repeat command(s)";
      // 
      // checkBoxAlwaysSendDiseqcCommands
      // 
      this.checkBoxAlwaysSendDiseqcCommands.AutoSize = true;
      this.checkBoxAlwaysSendDiseqcCommands.Location = new System.Drawing.Point(16, 19);
      this.checkBoxAlwaysSendDiseqcCommands.Name = "checkBoxAlwaysSendDiseqcCommands";
      this.checkBoxAlwaysSendDiseqcCommands.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxAlwaysSendDiseqcCommands.Size = new System.Drawing.Size(142, 17);
      this.checkBoxAlwaysSendDiseqcCommands.TabIndex = 16;
      this.checkBoxAlwaysSendDiseqcCommands.Text = "Always send commands.";
      this.checkBoxAlwaysSendDiseqcCommands.UseVisualStyleBackColor = true;
      // 
      // FormEditCard
      // 
      this.AcceptButton = this.mpButtonSave;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(429, 556);
      this.Controls.Add(this.groupBoxDiseqcSettings);
      this.Controls.Add(this.groupBoxAdvancedSettings);
      this.Controls.Add(this.groupBoxGeneralSettings);
      this.Controls.Add(this.groupBoxConditionalAccessSettings);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonSave);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormEditCard";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit Device Settings";
      this.Load += new System.EventHandler(this.FormEditCard_Load);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDecryptLimit)).EndInit();
      this.groupBoxConditionalAccessSettings.ResumeLayout(false);
      this.groupBoxConditionalAccessSettings.PerformLayout();
      this.groupBoxAdvancedSettings.ResumeLayout(false);
      this.groupBoxAdvancedSettings.PerformLayout();
      this.groupBoxGeneralSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.PerformLayout();
      this.groupBoxDiseqcSettings.ResumeLayout(false);
      this.groupBoxDiseqcSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiseqcCommandRepeatCount)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonSave;
    private System.Windows.Forms.NumericUpDown numericUpDownDecryptLimit;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
    private System.Windows.Forms.GroupBox groupBoxConditionalAccessSettings;
    private System.Windows.Forms.CheckBox checkBoxAllowEpgGrab;
    private System.Windows.Forms.CheckBox checkBoxPreloadCard;
    private System.Windows.Forms.GroupBox groupBoxAdvancedSettings;
    private System.Windows.Forms.CheckBox checkBoxConditionalAccessEnabled;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxCamType;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxNetworkProvider;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxMultiChannelDecryptMode;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxDeviceName;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel6;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxPidFilterMode;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxIdleMode;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private System.Windows.Forms.GroupBox groupBoxGeneralSettings;
    private System.Windows.Forms.CheckBox checkBoxUseCustomTuning;
    private System.Windows.Forms.GroupBox groupBoxDiseqcSettings;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel7;
    private System.Windows.Forms.NumericUpDown numericUpDownDiseqcCommandRepeatCount;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel8;
    private System.Windows.Forms.CheckBox checkBoxAlwaysSendDiseqcCommands;
  }
}
