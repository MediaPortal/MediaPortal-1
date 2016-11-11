using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEditTuningDetailAnalogRadio
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
      this.labelFrequencyUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericTextBoxFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericTextBox();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(123, 147);
      this.buttonCancel.TabIndex = 16;
      // 
      // buttonOkay
      // 
      this.buttonOkay.Location = new System.Drawing.Point(28, 147);
      this.buttonOkay.TabIndex = 15;
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
      // textBoxProvider
      // 
      this.textBoxProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxProvider.Size = new System.Drawing.Size(75, 20);
      // 
      // labelIsHighDefinition
      // 
      this.labelIsHighDefinition.Visible = false;
      // 
      // checkBoxIsHighDefinition
      // 
      this.checkBoxIsHighDefinition.Visible = false;
      // 
      // labelIsThreeDimensional
      // 
      this.labelIsThreeDimensional.Visible = false;
      // 
      // checkBoxIsThreeDimensional
      // 
      this.checkBoxIsThreeDimensional.Visible = false;
      // 
      // labelFrequencyUnit
      // 
      this.labelFrequencyUnit.AutoSize = true;
      this.labelFrequencyUnit.Location = new System.Drawing.Point(174, 116);
      this.labelFrequencyUnit.Name = "labelFrequencyUnit";
      this.labelFrequencyUnit.Size = new System.Drawing.Size(26, 13);
      this.labelFrequencyUnit.TabIndex = 14;
      this.labelFrequencyUnit.Text = "kHz";
      // 
      // labelFrequency
      // 
      this.labelFrequency.AutoSize = true;
      this.labelFrequency.Location = new System.Drawing.Point(12, 116);
      this.labelFrequency.Name = "labelFrequency";
      this.labelFrequency.Size = new System.Drawing.Size(60, 13);
      this.labelFrequency.TabIndex = 12;
      this.labelFrequency.Text = "Frequency:";
      // 
      // numericTextBoxFrequency
      // 
      this.numericTextBoxFrequency.Location = new System.Drawing.Point(123, 113);
      this.numericTextBoxFrequency.MaximumValue = 999999;
      this.numericTextBoxFrequency.MaxLength = 6;
      this.numericTextBoxFrequency.MinimumValue = 0;
      this.numericTextBoxFrequency.Name = "numericTextBoxFrequency";
      this.numericTextBoxFrequency.Size = new System.Drawing.Size(45, 20);
      this.numericTextBoxFrequency.TabIndex = 13;
      this.numericTextBoxFrequency.Text = "1449";
      this.numericTextBoxFrequency.Value = 1449;
      // 
      // FormEditTuningDetailAnalogRadio
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(210, 182);
      this.Controls.Add(this.labelFrequencyUnit);
      this.Controls.Add(this.labelFrequency);
      this.Controls.Add(this.numericTextBoxFrequency);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(218, 208);
      this.Name = "FormEditTuningDetailAnalogRadio";
      this.Text = "Add/Edit AM Radio Tuning Detail";
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
      this.Controls.SetChildIndex(this.numericTextBoxFrequency, 0);
      this.Controls.SetChildIndex(this.labelFrequency, 0);
      this.Controls.SetChildIndex(this.labelFrequencyUnit, 0);
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPLabel labelFrequencyUnit;
    private MPLabel labelFrequency;
    private MPNumericTextBox numericTextBoxFrequency;

  }
}
