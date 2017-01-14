using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEditTuningDetailAnalogTv
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
      this.comboBoxCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxSource = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelSource = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequencyUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      this.labelPhysicalChannelNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxPhysicalChannelNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(218, 259);
      this.buttonCancel.TabIndex = 22;
      // 
      // buttonOkay
      // 
      this.buttonOkay.Location = new System.Drawing.Point(123, 259);
      this.buttonOkay.TabIndex = 21;
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
      this.textBoxName.Size = new System.Drawing.Size(170, 20);
      // 
      // textBoxProvider
      // 
      this.textBoxProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxProvider.Size = new System.Drawing.Size(170, 20);
      // 
      // labelIsThreeDimensional
      // 
      this.labelIsThreeDimensional.Visible = false;
      // 
      // checkBoxIsThreeDimensional
      // 
      this.checkBoxIsThreeDimensional.Visible = false;
      // 
      // comboBoxCountry
      // 
      this.comboBoxCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxCountry.DisplayMember = "Name";
      this.comboBoxCountry.DropDownWidth = 245;
      this.comboBoxCountry.FormattingEnabled = true;
      this.comboBoxCountry.Location = new System.Drawing.Point(123, 188);
      this.comboBoxCountry.Name = "comboBoxCountry";
      this.comboBoxCountry.Size = new System.Drawing.Size(170, 21);
      this.comboBoxCountry.TabIndex = 18;
      // 
      // labelCountry
      // 
      this.labelCountry.AutoSize = true;
      this.labelCountry.Location = new System.Drawing.Point(12, 191);
      this.labelCountry.Name = "labelCountry";
      this.labelCountry.Size = new System.Drawing.Size(46, 13);
      this.labelCountry.TabIndex = 17;
      this.labelCountry.Text = "Country:";
      // 
      // comboBoxSource
      // 
      this.comboBoxSource.IntegralHeight = false;
      this.comboBoxSource.Location = new System.Drawing.Point(123, 215);
      this.comboBoxSource.Name = "comboBoxSource";
      this.comboBoxSource.Size = new System.Drawing.Size(75, 21);
      this.comboBoxSource.TabIndex = 20;
      // 
      // labelSource
      // 
      this.labelSource.AutoSize = true;
      this.labelSource.Location = new System.Drawing.Point(12, 218);
      this.labelSource.Name = "labelSource";
      this.labelSource.Size = new System.Drawing.Size(44, 13);
      this.labelSource.TabIndex = 19;
      this.labelSource.Text = "Source:";
      // 
      // labelFrequencyUnit
      // 
      this.labelFrequencyUnit.AutoSize = true;
      this.labelFrequencyUnit.Location = new System.Drawing.Point(174, 165);
      this.labelFrequencyUnit.Name = "labelFrequencyUnit";
      this.labelFrequencyUnit.Size = new System.Drawing.Size(108, 13);
      this.labelFrequencyUnit.TabIndex = 16;
      this.labelFrequencyUnit.Text = "kHz (set 0 for default)";
      // 
      // labelFrequency
      // 
      this.labelFrequency.AutoSize = true;
      this.labelFrequency.Location = new System.Drawing.Point(12, 165);
      this.labelFrequency.Name = "labelFrequency";
      this.labelFrequency.Size = new System.Drawing.Size(60, 13);
      this.labelFrequency.TabIndex = 14;
      this.labelFrequency.Text = "Frequency:";
      // 
      // numericTextBoxFrequency
      // 
      this.numericTextBoxFrequency.Location = new System.Drawing.Point(123, 162);
      this.numericTextBoxFrequency.MaximumValue = 999999;
      this.numericTextBoxFrequency.MaxLength = 6;
      this.numericTextBoxFrequency.MinimumValue = 0;
      this.numericTextBoxFrequency.Name = "numericTextBoxFrequency";
      this.numericTextBoxFrequency.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxFrequency.TabIndex = 15;
      this.numericTextBoxFrequency.Text = "0";
      this.numericTextBoxFrequency.Value = 0;
      // 
      // labelPhysicalChannelNumber
      // 
      this.labelPhysicalChannelNumber.AutoSize = true;
      this.labelPhysicalChannelNumber.Location = new System.Drawing.Point(12, 139);
      this.labelPhysicalChannelNumber.Name = "labelPhysicalChannelNumber";
      this.labelPhysicalChannelNumber.Size = new System.Drawing.Size(90, 13);
      this.labelPhysicalChannelNumber.TabIndex = 12;
      this.labelPhysicalChannelNumber.Text = "Physical channel:";
      // 
      // numericTextBoxPhysicalChannelNumber
      // 
      this.numericTextBoxPhysicalChannelNumber.Location = new System.Drawing.Point(123, 136);
      this.numericTextBoxPhysicalChannelNumber.MaximumValue = 32768;
      this.numericTextBoxPhysicalChannelNumber.MaxLength = 5;
      this.numericTextBoxPhysicalChannelNumber.MinimumValue = 1;
      this.numericTextBoxPhysicalChannelNumber.Name = "numericTextBoxPhysicalChannelNumber";
      this.numericTextBoxPhysicalChannelNumber.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxPhysicalChannelNumber.TabIndex = 13;
      this.numericTextBoxPhysicalChannelNumber.Text = "1";
      this.numericTextBoxPhysicalChannelNumber.Value = 1;
      // 
      // FormEditTuningDetailAnalogTv
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(305, 294);
      this.Controls.Add(this.comboBoxSource);
      this.Controls.Add(this.comboBoxCountry);
      this.Controls.Add(this.labelSource);
      this.Controls.Add(this.labelCountry);
      this.Controls.Add(this.labelPhysicalChannelNumber);
      this.Controls.Add(this.numericTextBoxPhysicalChannelNumber);
      this.Controls.Add(this.labelFrequencyUnit);
      this.Controls.Add(this.labelFrequency);
      this.Controls.Add(this.numericTextBoxFrequency);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(313, 320);
      this.Name = "FormEditTuningDetailAnalogTv";
      this.Text = "Add/Edit Analog TV Tuning Detail";
      this.Controls.SetChildIndex(this.checkBoxIsHighDefinition, 0);
      this.Controls.SetChildIndex(this.labelIsHighDefinition, 0);
      this.Controls.SetChildIndex(this.checkBoxIsThreeDimensional, 0);
      this.Controls.SetChildIndex(this.labelIsThreeDimensional, 0);
      this.Controls.SetChildIndex(this.textBoxProvider, 0);
      this.Controls.SetChildIndex(this.labelProvider, 0);
      this.Controls.SetChildIndex(this.checkBoxIsEncrypted, 0);
      this.Controls.SetChildIndex(this.textBoxName, 0);
      this.Controls.SetChildIndex(this.labelName, 0);
      this.Controls.SetChildIndex(this.channelNumberUpDownNumber, 0);
      this.Controls.SetChildIndex(this.labelNumber, 0);
      this.Controls.SetChildIndex(this.labelIsEncrypted, 0);
      this.Controls.SetChildIndex(this.buttonOkay, 0);
      this.Controls.SetChildIndex(this.buttonCancel, 0);
      this.Controls.SetChildIndex(this.numericTextBoxFrequency, 0);
      this.Controls.SetChildIndex(this.labelFrequency, 0);
      this.Controls.SetChildIndex(this.labelFrequencyUnit, 0);
      this.Controls.SetChildIndex(this.numericTextBoxPhysicalChannelNumber, 0);
      this.Controls.SetChildIndex(this.labelPhysicalChannelNumber, 0);
      this.Controls.SetChildIndex(this.labelCountry, 0);
      this.Controls.SetChildIndex(this.labelSource, 0);
      this.Controls.SetChildIndex(this.comboBoxCountry, 0);
      this.Controls.SetChildIndex(this.comboBoxSource, 0);
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPComboBox comboBoxCountry;
    private MPLabel labelCountry;
    private MPComboBox comboBoxSource;
    private MPLabel labelSource;
    private MPLabel labelFrequencyUnit;
    private MPLabel labelFrequency;
    private MPNumericTextBox numericTextBoxFrequency;
    private MPLabel labelPhysicalChannelNumber;
    private MPNumericTextBox numericTextBoxPhysicalChannelNumber;
  }
}
