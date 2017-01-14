using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEditTuningDetailCommon
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
      this.buttonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonOkay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.channelNumberUpDownNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPChannelNumberUpDown();
      this.labelName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.textBoxName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.checkBoxIsEncrypted = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.textBoxProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.labelIsEncrypted = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelIsHighDefinition = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxIsHighDefinition = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelIsThreeDimensional = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxIsThreeDimensional = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(198, 227);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 13;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // buttonOkay
      // 
      this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOkay.Location = new System.Drawing.Point(117, 227);
      this.buttonOkay.Name = "buttonOkay";
      this.buttonOkay.Size = new System.Drawing.Size(75, 23);
      this.buttonOkay.TabIndex = 12;
      this.buttonOkay.Text = "&OK";
      this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
      // 
      // labelNumber
      // 
      this.labelNumber.AutoSize = true;
      this.labelNumber.Location = new System.Drawing.Point(12, 41);
      this.labelNumber.Name = "labelNumber";
      this.labelNumber.Size = new System.Drawing.Size(47, 13);
      this.labelNumber.TabIndex = 2;
      this.labelNumber.Text = "Number:";
      // 
      // channelNumberUpDownNumber
      // 
      this.channelNumberUpDownNumber.DecimalPlaces = 3;
      this.channelNumberUpDownNumber.Location = new System.Drawing.Point(123, 38);
      this.channelNumberUpDownNumber.Maximum = new decimal(new int[] {
            65535999,
            0,
            0,
            196608});
      this.channelNumberUpDownNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.channelNumberUpDownNumber.Name = "channelNumberUpDownNumber";
      this.channelNumberUpDownNumber.Size = new System.Drawing.Size(75, 20);
      this.channelNumberUpDownNumber.TabIndex = 3;
      this.channelNumberUpDownNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // labelName
      // 
      this.labelName.AutoSize = true;
      this.labelName.Location = new System.Drawing.Point(12, 15);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(38, 13);
      this.labelName.TabIndex = 0;
      this.labelName.Text = "Name:";
      // 
      // textBoxName
      // 
      this.textBoxName.Location = new System.Drawing.Point(123, 12);
      this.textBoxName.MaxLength = 200;
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new System.Drawing.Size(150, 20);
      this.textBoxName.TabIndex = 1;
      // 
      // checkBoxIsEncrypted
      // 
      this.checkBoxIsEncrypted.AutoSize = true;
      this.checkBoxIsEncrypted.Location = new System.Drawing.Point(123, 90);
      this.checkBoxIsEncrypted.Name = "checkBoxIsEncrypted";
      this.checkBoxIsEncrypted.Size = new System.Drawing.Size(27, 17);
      this.checkBoxIsEncrypted.TabIndex = 7;
      this.checkBoxIsEncrypted.Text = " ";
      this.checkBoxIsEncrypted.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // labelProvider
      // 
      this.labelProvider.AutoSize = true;
      this.labelProvider.Location = new System.Drawing.Point(12, 67);
      this.labelProvider.Name = "labelProvider";
      this.labelProvider.Size = new System.Drawing.Size(49, 13);
      this.labelProvider.TabIndex = 4;
      this.labelProvider.Text = "Provider:";
      // 
      // textBoxProvider
      // 
      this.textBoxProvider.Location = new System.Drawing.Point(123, 64);
      this.textBoxProvider.MaxLength = 200;
      this.textBoxProvider.Name = "textBoxProvider";
      this.textBoxProvider.Size = new System.Drawing.Size(150, 20);
      this.textBoxProvider.TabIndex = 5;
      // 
      // labelIsEncrypted
      // 
      this.labelIsEncrypted.AutoSize = true;
      this.labelIsEncrypted.Location = new System.Drawing.Point(12, 92);
      this.labelIsEncrypted.Name = "labelIsEncrypted";
      this.labelIsEncrypted.Size = new System.Drawing.Size(58, 13);
      this.labelIsEncrypted.TabIndex = 6;
      this.labelIsEncrypted.Text = "Encrypted:";
      // 
      // labelIsHighDefinition
      // 
      this.labelIsHighDefinition.AutoSize = true;
      this.labelIsHighDefinition.Location = new System.Drawing.Point(12, 115);
      this.labelIsHighDefinition.Name = "labelIsHighDefinition";
      this.labelIsHighDefinition.Size = new System.Drawing.Size(102, 13);
      this.labelIsHighDefinition.TabIndex = 8;
      this.labelIsHighDefinition.Text = "High definition (HD):";
      // 
      // checkBoxIsHighDefinition
      // 
      this.checkBoxIsHighDefinition.AutoSize = true;
      this.checkBoxIsHighDefinition.Location = new System.Drawing.Point(123, 113);
      this.checkBoxIsHighDefinition.Name = "checkBoxIsHighDefinition";
      this.checkBoxIsHighDefinition.Size = new System.Drawing.Size(27, 17);
      this.checkBoxIsHighDefinition.TabIndex = 9;
      this.checkBoxIsHighDefinition.Text = " ";
      this.checkBoxIsHighDefinition.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // labelIsThreeDimensional
      // 
      this.labelIsThreeDimensional.AutoSize = true;
      this.labelIsThreeDimensional.Location = new System.Drawing.Point(12, 138);
      this.labelIsThreeDimensional.Name = "labelIsThreeDimensional";
      this.labelIsThreeDimensional.Size = new System.Drawing.Size(83, 13);
      this.labelIsThreeDimensional.TabIndex = 10;
      this.labelIsThreeDimensional.Text = "Three dim. (3D):";
      // 
      // checkBoxIsThreeDimensional
      // 
      this.checkBoxIsThreeDimensional.AutoSize = true;
      this.checkBoxIsThreeDimensional.Location = new System.Drawing.Point(123, 136);
      this.checkBoxIsThreeDimensional.Name = "checkBoxIsThreeDimensional";
      this.checkBoxIsThreeDimensional.Size = new System.Drawing.Size(27, 17);
      this.checkBoxIsThreeDimensional.TabIndex = 11;
      this.checkBoxIsThreeDimensional.Text = " ";
      this.checkBoxIsThreeDimensional.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // FormEditTuningDetailCommon
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(285, 262);
      this.Controls.Add(this.labelIsThreeDimensional);
      this.Controls.Add(this.checkBoxIsThreeDimensional);
      this.Controls.Add(this.labelIsHighDefinition);
      this.Controls.Add(this.checkBoxIsHighDefinition);
      this.Controls.Add(this.labelIsEncrypted);
      this.Controls.Add(this.labelNumber);
      this.Controls.Add(this.channelNumberUpDownNumber);
      this.Controls.Add(this.labelName);
      this.Controls.Add(this.textBoxName);
      this.Controls.Add(this.checkBoxIsEncrypted);
      this.Controls.Add(this.labelProvider);
      this.Controls.Add(this.textBoxProvider);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOkay);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormEditTuningDetailCommon";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "FormTuningDetailCommon";
      this.Load += new System.EventHandler(this.FormEditTuningDetailCommon_Load);
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    protected MPButton buttonCancel;
    protected MPButton buttonOkay;
    protected MPLabel labelNumber;
    protected MPChannelNumberUpDown channelNumberUpDownNumber;
    protected MPLabel labelName;
    protected MPTextBox textBoxName;
    protected MPCheckBox checkBoxIsEncrypted;
    protected MPLabel labelProvider;
    protected MPTextBox textBoxProvider;
    protected MPLabel labelIsEncrypted;
    protected MPLabel labelIsHighDefinition;
    protected MPCheckBox checkBoxIsHighDefinition;
    protected MPLabel labelIsThreeDimensional;
    protected MPCheckBox checkBoxIsThreeDimensional;
  }
}
