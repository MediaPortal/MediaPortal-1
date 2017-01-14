using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEditTuningDetailStream
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
      this.textBoxUrl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.labelUrl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
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
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).BeginInit();
      this.groupBoxEpgSource.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(501, 201);
      this.buttonCancel.TabIndex = 24;
      // 
      // buttonOkay
      // 
      this.buttonOkay.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonOkay.Location = new System.Drawing.Point(406, 201);
      this.buttonOkay.TabIndex = 23;
      // 
      // labelNumber
      // 
      this.labelNumber.Location = new System.Drawing.Point(12, 40);
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
      // textBoxUrl
      // 
      this.textBoxUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxUrl.Location = new System.Drawing.Point(123, 166);
      this.textBoxUrl.MaxLength = 200;
      this.textBoxUrl.Name = "textBoxUrl";
      this.textBoxUrl.Size = new System.Drawing.Size(453, 20);
      this.textBoxUrl.TabIndex = 13;
      // 
      // labelUrl
      // 
      this.labelUrl.AutoSize = true;
      this.labelUrl.Location = new System.Drawing.Point(12, 169);
      this.labelUrl.Name = "labelUrl";
      this.labelUrl.Size = new System.Drawing.Size(32, 13);
      this.labelUrl.TabIndex = 12;
      this.labelUrl.Text = "URL:";
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
      this.groupBoxEpgSource.Location = new System.Drawing.Point(409, 18);
      this.groupBoxEpgSource.Name = "groupBoxEpgSource";
      this.groupBoxEpgSource.Size = new System.Drawing.Size(167, 103);
      this.groupBoxEpgSource.TabIndex = 22;
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
      this.labelEpgServiceId.Location = new System.Drawing.Point(9, 76);
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
      this.numericTextBoxPmtPid.TabIndex = 21;
      this.numericTextBoxPmtPid.Text = "0";
      this.numericTextBoxPmtPid.Value = 0;
      // 
      // labelOriginalNetworkId
      // 
      this.labelOriginalNetworkId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelOriginalNetworkId.AutoSize = true;
      this.labelOriginalNetworkId.Location = new System.Drawing.Point(230, 40);
      this.labelOriginalNetworkId.Name = "labelOriginalNetworkId";
      this.labelOriginalNetworkId.Size = new System.Drawing.Size(100, 13);
      this.labelOriginalNetworkId.TabIndex = 14;
      this.labelOriginalNetworkId.Text = "Original network ID:";
      // 
      // numericTextBoxServiceId
      // 
      this.numericTextBoxServiceId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxServiceId.Location = new System.Drawing.Point(339, 90);
      this.numericTextBoxServiceId.MaximumValue = 65535;
      this.numericTextBoxServiceId.MaxLength = 5;
      this.numericTextBoxServiceId.MinimumValue = 0;
      this.numericTextBoxServiceId.Name = "numericTextBoxServiceId";
      this.numericTextBoxServiceId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxServiceId.TabIndex = 19;
      this.numericTextBoxServiceId.Text = "0";
      this.numericTextBoxServiceId.Value = 0;
      // 
      // numericTextBoxTransportStreamId
      // 
      this.numericTextBoxTransportStreamId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxTransportStreamId.Location = new System.Drawing.Point(339, 64);
      this.numericTextBoxTransportStreamId.MaximumValue = 65535;
      this.numericTextBoxTransportStreamId.MaxLength = 5;
      this.numericTextBoxTransportStreamId.MinimumValue = 0;
      this.numericTextBoxTransportStreamId.Name = "numericTextBoxTransportStreamId";
      this.numericTextBoxTransportStreamId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxTransportStreamId.TabIndex = 17;
      this.numericTextBoxTransportStreamId.Text = "0";
      this.numericTextBoxTransportStreamId.Value = 0;
      // 
      // numericTextBoxOriginalNetworkId
      // 
      this.numericTextBoxOriginalNetworkId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericTextBoxOriginalNetworkId.Location = new System.Drawing.Point(339, 37);
      this.numericTextBoxOriginalNetworkId.MaximumValue = 65535;
      this.numericTextBoxOriginalNetworkId.MaxLength = 5;
      this.numericTextBoxOriginalNetworkId.MinimumValue = 0;
      this.numericTextBoxOriginalNetworkId.Name = "numericTextBoxOriginalNetworkId";
      this.numericTextBoxOriginalNetworkId.Size = new System.Drawing.Size(40, 20);
      this.numericTextBoxOriginalNetworkId.TabIndex = 15;
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
      this.labelPmtPid.TabIndex = 20;
      this.labelPmtPid.Text = "PMT PID:";
      // 
      // labelTransportStreamId
      // 
      this.labelTransportStreamId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelTransportStreamId.AutoSize = true;
      this.labelTransportStreamId.Location = new System.Drawing.Point(230, 67);
      this.labelTransportStreamId.Name = "labelTransportStreamId";
      this.labelTransportStreamId.Size = new System.Drawing.Size(103, 13);
      this.labelTransportStreamId.TabIndex = 16;
      this.labelTransportStreamId.Text = "Transport stream ID:";
      // 
      // labelServiceId
      // 
      this.labelServiceId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelServiceId.AutoSize = true;
      this.labelServiceId.Location = new System.Drawing.Point(230, 93);
      this.labelServiceId.Name = "labelServiceId";
      this.labelServiceId.Size = new System.Drawing.Size(60, 13);
      this.labelServiceId.TabIndex = 18;
      this.labelServiceId.Text = "Service ID:";
      // 
      // FormEditTuningDetailStream
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(588, 236);
      this.Controls.Add(this.groupBoxEpgSource);
      this.Controls.Add(this.numericTextBoxPmtPid);
      this.Controls.Add(this.labelOriginalNetworkId);
      this.Controls.Add(this.numericTextBoxServiceId);
      this.Controls.Add(this.numericTextBoxTransportStreamId);
      this.Controls.Add(this.numericTextBoxOriginalNetworkId);
      this.Controls.Add(this.labelPmtPid);
      this.Controls.Add(this.labelTransportStreamId);
      this.Controls.Add(this.labelServiceId);
      this.Controls.Add(this.textBoxUrl);
      this.Controls.Add(this.labelUrl);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(596, 262);
      this.Name = "FormEditTuningDetailStream";
      this.Text = "Add/Edit Stream Tuning Detail";
      this.Controls.SetChildIndex(this.checkBoxIsHighDefinition, 0);
      this.Controls.SetChildIndex(this.labelIsHighDefinition, 0);
      this.Controls.SetChildIndex(this.checkBoxIsThreeDimensional, 0);
      this.Controls.SetChildIndex(this.labelIsThreeDimensional, 0);
      this.Controls.SetChildIndex(this.labelIsEncrypted, 0);
      this.Controls.SetChildIndex(this.textBoxProvider, 0);
      this.Controls.SetChildIndex(this.labelProvider, 0);
      this.Controls.SetChildIndex(this.checkBoxIsEncrypted, 0);
      this.Controls.SetChildIndex(this.textBoxName, 0);
      this.Controls.SetChildIndex(this.labelName, 0);
      this.Controls.SetChildIndex(this.channelNumberUpDownNumber, 0);
      this.Controls.SetChildIndex(this.labelNumber, 0);
      this.Controls.SetChildIndex(this.buttonOkay, 0);
      this.Controls.SetChildIndex(this.buttonCancel, 0);
      this.Controls.SetChildIndex(this.labelUrl, 0);
      this.Controls.SetChildIndex(this.textBoxUrl, 0);
      this.Controls.SetChildIndex(this.labelServiceId, 0);
      this.Controls.SetChildIndex(this.labelTransportStreamId, 0);
      this.Controls.SetChildIndex(this.labelPmtPid, 0);
      this.Controls.SetChildIndex(this.numericTextBoxOriginalNetworkId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxTransportStreamId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxServiceId, 0);
      this.Controls.SetChildIndex(this.labelOriginalNetworkId, 0);
      this.Controls.SetChildIndex(this.numericTextBoxPmtPid, 0);
      this.Controls.SetChildIndex(this.groupBoxEpgSource, 0);
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).EndInit();
      this.groupBoxEpgSource.ResumeLayout(false);
      this.groupBoxEpgSource.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPTextBox textBoxUrl;
    private MPLabel labelUrl;
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
  }
}
