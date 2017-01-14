using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEditTuningDetailAtscScte
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
      this.numericTextBoxPmtPid = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelSourceId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxProgramNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxTransportStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.numericTextBoxSourceId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelPmtPid = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTransportStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelProgramNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequencyUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxModulation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelModulation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(123, 325);
      this.buttonCancel.TabIndex = 26;
      // 
      // buttonOkay
      // 
      this.buttonOkay.Location = new System.Drawing.Point(28, 325);
      this.buttonOkay.TabIndex = 25;
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
      // textBoxProvider
      // 
      this.textBoxProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxProvider.Size = new System.Drawing.Size(75, 20);
      // 
      // numericTextBoxPmtPid
      // 
      this.numericTextBoxPmtPid.Location = new System.Drawing.Point(123, 290);
      this.numericTextBoxPmtPid.MaximumValue = 65535;
      this.numericTextBoxPmtPid.MaxLength = 5;
      this.numericTextBoxPmtPid.MinimumValue = 0;
      this.numericTextBoxPmtPid.Name = "numericTextBoxPmtPid";
      this.numericTextBoxPmtPid.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxPmtPid.TabIndex = 24;
      this.numericTextBoxPmtPid.Text = "0";
      this.numericTextBoxPmtPid.Value = 0;
      // 
      // labelSourceId
      // 
      this.labelSourceId.AutoSize = true;
      this.labelSourceId.Location = new System.Drawing.Point(12, 267);
      this.labelSourceId.Name = "labelSourceId";
      this.labelSourceId.Size = new System.Drawing.Size(58, 13);
      this.labelSourceId.TabIndex = 21;
      this.labelSourceId.Text = "Source ID:";
      // 
      // numericTextBoxProgramNumber
      // 
      this.numericTextBoxProgramNumber.Location = new System.Drawing.Point(123, 238);
      this.numericTextBoxProgramNumber.MaximumValue = 65535;
      this.numericTextBoxProgramNumber.MaxLength = 5;
      this.numericTextBoxProgramNumber.MinimumValue = 0;
      this.numericTextBoxProgramNumber.Name = "numericTextBoxProgramNumber";
      this.numericTextBoxProgramNumber.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxProgramNumber.TabIndex = 20;
      this.numericTextBoxProgramNumber.Text = "0";
      this.numericTextBoxProgramNumber.Value = 0;
      // 
      // numericTextBoxTransportStreamId
      // 
      this.numericTextBoxTransportStreamId.Location = new System.Drawing.Point(123, 212);
      this.numericTextBoxTransportStreamId.MaximumValue = 65535;
      this.numericTextBoxTransportStreamId.MaxLength = 5;
      this.numericTextBoxTransportStreamId.MinimumValue = 0;
      this.numericTextBoxTransportStreamId.Name = "numericTextBoxTransportStreamId";
      this.numericTextBoxTransportStreamId.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxTransportStreamId.TabIndex = 18;
      this.numericTextBoxTransportStreamId.Text = "0";
      this.numericTextBoxTransportStreamId.Value = 0;
      // 
      // numericTextBoxSourceId
      // 
      this.numericTextBoxSourceId.Location = new System.Drawing.Point(123, 264);
      this.numericTextBoxSourceId.MaximumValue = 65535;
      this.numericTextBoxSourceId.MaxLength = 5;
      this.numericTextBoxSourceId.MinimumValue = 0;
      this.numericTextBoxSourceId.Name = "numericTextBoxSourceId";
      this.numericTextBoxSourceId.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxSourceId.TabIndex = 22;
      this.numericTextBoxSourceId.Text = "0";
      this.numericTextBoxSourceId.Value = 0;
      // 
      // labelPmtPid
      // 
      this.labelPmtPid.AutoSize = true;
      this.labelPmtPid.Location = new System.Drawing.Point(12, 293);
      this.labelPmtPid.Name = "labelPmtPid";
      this.labelPmtPid.Size = new System.Drawing.Size(54, 13);
      this.labelPmtPid.TabIndex = 23;
      this.labelPmtPid.Text = "PMT PID:";
      // 
      // labelTransportStreamId
      // 
      this.labelTransportStreamId.AutoSize = true;
      this.labelTransportStreamId.Location = new System.Drawing.Point(12, 215);
      this.labelTransportStreamId.Name = "labelTransportStreamId";
      this.labelTransportStreamId.Size = new System.Drawing.Size(103, 13);
      this.labelTransportStreamId.TabIndex = 17;
      this.labelTransportStreamId.Text = "Transport stream ID:";
      // 
      // labelProgramNumber
      // 
      this.labelProgramNumber.AutoSize = true;
      this.labelProgramNumber.Location = new System.Drawing.Point(12, 241);
      this.labelProgramNumber.Name = "labelProgramNumber";
      this.labelProgramNumber.Size = new System.Drawing.Size(87, 13);
      this.labelProgramNumber.TabIndex = 19;
      this.labelProgramNumber.Text = "Program number:";
      // 
      // labelFrequencyUnit
      // 
      this.labelFrequencyUnit.AutoSize = true;
      this.labelFrequencyUnit.Location = new System.Drawing.Point(174, 162);
      this.labelFrequencyUnit.Name = "labelFrequencyUnit";
      this.labelFrequencyUnit.Size = new System.Drawing.Size(26, 13);
      this.labelFrequencyUnit.TabIndex = 14;
      this.labelFrequencyUnit.Text = "kHz";
      // 
      // comboBoxModulation
      // 
      this.comboBoxModulation.FormattingEnabled = true;
      this.comboBoxModulation.Location = new System.Drawing.Point(123, 185);
      this.comboBoxModulation.Name = "comboBoxModulation";
      this.comboBoxModulation.Size = new System.Drawing.Size(75, 21);
      this.comboBoxModulation.TabIndex = 16;
      // 
      // labelModulation
      // 
      this.labelModulation.AutoSize = true;
      this.labelModulation.Location = new System.Drawing.Point(12, 188);
      this.labelModulation.Name = "labelModulation";
      this.labelModulation.Size = new System.Drawing.Size(62, 13);
      this.labelModulation.TabIndex = 15;
      this.labelModulation.Text = "Modulation:";
      // 
      // labelFrequency
      // 
      this.labelFrequency.AutoSize = true;
      this.labelFrequency.Location = new System.Drawing.Point(12, 162);
      this.labelFrequency.Name = "labelFrequency";
      this.labelFrequency.Size = new System.Drawing.Size(60, 13);
      this.labelFrequency.TabIndex = 12;
      this.labelFrequency.Text = "Frequency:";
      // 
      // numericTextBoxFrequency
      // 
      this.numericTextBoxFrequency.Location = new System.Drawing.Point(123, 159);
      this.numericTextBoxFrequency.MaximumValue = 999999;
      this.numericTextBoxFrequency.MaxLength = 6;
      this.numericTextBoxFrequency.MinimumValue = 0;
      this.numericTextBoxFrequency.Name = "numericTextBoxFrequency";
      this.numericTextBoxFrequency.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxFrequency.TabIndex = 13;
      this.numericTextBoxFrequency.Text = "0";
      this.numericTextBoxFrequency.Value = 0;
      // 
      // FormEditTuningDetailAtscScte
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(210, 360);
      this.Controls.Add(this.labelFrequencyUnit);
      this.Controls.Add(this.comboBoxModulation);
      this.Controls.Add(this.labelModulation);
      this.Controls.Add(this.labelFrequency);
      this.Controls.Add(this.numericTextBoxFrequency);
      this.Controls.Add(this.numericTextBoxPmtPid);
      this.Controls.Add(this.labelSourceId);
      this.Controls.Add(this.numericTextBoxProgramNumber);
      this.Controls.Add(this.numericTextBoxTransportStreamId);
      this.Controls.Add(this.numericTextBoxSourceId);
      this.Controls.Add(this.labelPmtPid);
      this.Controls.Add(this.labelTransportStreamId);
      this.Controls.Add(this.labelProgramNumber);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(218, 386);
      this.Name = "FormEditTuningDetailAtscScte";
      this.Text = "Add/Edit ATSC Tuning Detail";
      this.Controls.SetChildIndex(this.checkBoxIsHighDefinition, 0);
      this.Controls.SetChildIndex(this.labelIsHighDefinition, 0);
      this.Controls.SetChildIndex(this.checkBoxIsThreeDimensional, 0);
      this.Controls.SetChildIndex(this.labelIsThreeDimensional, 0);
      this.Controls.SetChildIndex(this.labelIsEncrypted, 0);
      this.Controls.SetChildIndex(this.labelProvider, 0);
      this.Controls.SetChildIndex(this.checkBoxIsEncrypted, 0);
      this.Controls.SetChildIndex(this.textBoxProvider, 0);
      this.Controls.SetChildIndex(this.labelName, 0);
      this.Controls.SetChildIndex(this.textBoxName, 0);
      this.Controls.SetChildIndex(this.channelNumberUpDownNumber, 0);
      this.Controls.SetChildIndex(this.labelNumber, 0);
      this.Controls.SetChildIndex(this.buttonOkay, 0);
      this.Controls.SetChildIndex(this.buttonCancel, 0);
      this.Controls.SetChildIndex(this.labelProgramNumber, 0);
      this.Controls.SetChildIndex(this.labelTransportStreamId, 0);
      this.Controls.SetChildIndex(this.labelPmtPid, 0);
      this.Controls.SetChildIndex(this.numericTextBoxSourceId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxTransportStreamId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxProgramNumber, 0);
      this.Controls.SetChildIndex(this.labelSourceId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxPmtPid, 0);
      this.Controls.SetChildIndex(this.numericTextBoxFrequency, 0);
      this.Controls.SetChildIndex(this.labelFrequency, 0);
      this.Controls.SetChildIndex(this.labelModulation, 0);
      this.Controls.SetChildIndex(this.comboBoxModulation, 0);
      this.Controls.SetChildIndex(this.labelFrequencyUnit, 0);
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPNumericTextBox numericTextBoxPmtPid;
    private MPLabel labelSourceId;
    private MPNumericTextBox numericTextBoxProgramNumber;
    private MPNumericTextBox numericTextBoxTransportStreamId;
    private MPNumericTextBox numericTextBoxSourceId;
    private MPLabel labelPmtPid;
    private MPLabel labelTransportStreamId;
    private MPLabel labelProgramNumber;
    private MPLabel labelFrequencyUnit;
    private MPComboBox comboBoxModulation;
    private MPLabel labelModulation;
    private MPLabel labelFrequency;
    private MPNumericTextBox numericTextBoxFrequency;

  }
}
