using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEditTunerSatellite
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
      this.labelIsToroidalDish = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxIsToroidalDish = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.comboBoxTone22kState = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelTone22kState = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelToneBurst = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxToneBurst = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelDiseqcMotorPosition = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelDiseqcSwitchPort = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxDiseqcSwitchPort = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelLnbType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxLnbType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelTuner = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxTuner = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelSatellite = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxSatellite = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.numericUpDownDiseqcMotorPosition = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.comboBoxDiseqcMotorPositionType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiseqcMotorPosition)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(258, 235);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 18;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // buttonOkay
      // 
      this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOkay.Location = new System.Drawing.Point(177, 235);
      this.buttonOkay.Name = "buttonOkay";
      this.buttonOkay.Size = new System.Drawing.Size(75, 23);
      this.buttonOkay.TabIndex = 17;
      this.buttonOkay.Text = "&OK";
      this.buttonOkay.UseVisualStyleBackColor = true;
      this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
      // 
      // labelIsToroidalDish
      // 
      this.labelIsToroidalDish.AutoSize = true;
      this.labelIsToroidalDish.Location = new System.Drawing.Point(12, 203);
      this.labelIsToroidalDish.Name = "labelIsToroidalDish";
      this.labelIsToroidalDish.Size = new System.Drawing.Size(70, 13);
      this.labelIsToroidalDish.TabIndex = 15;
      this.labelIsToroidalDish.Text = "Toroidal dish:";
      // 
      // checkBoxIsToroidalDish
      // 
      this.checkBoxIsToroidalDish.AutoSize = true;
      this.checkBoxIsToroidalDish.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxIsToroidalDish.Location = new System.Drawing.Point(133, 201);
      this.checkBoxIsToroidalDish.Name = "checkBoxIsToroidalDish";
      this.checkBoxIsToroidalDish.Size = new System.Drawing.Size(27, 17);
      this.checkBoxIsToroidalDish.TabIndex = 16;
      this.checkBoxIsToroidalDish.Text = " ";
      this.checkBoxIsToroidalDish.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.checkBoxIsToroidalDish.UseVisualStyleBackColor = true;
      // 
      // comboBoxTone22kState
      // 
      this.comboBoxTone22kState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxTone22kState.FormattingEnabled = true;
      this.comboBoxTone22kState.Location = new System.Drawing.Point(133, 174);
      this.comboBoxTone22kState.Name = "comboBoxTone22kState";
      this.comboBoxTone22kState.Size = new System.Drawing.Size(129, 21);
      this.comboBoxTone22kState.TabIndex = 14;
      // 
      // labelTone22kState
      // 
      this.labelTone22kState.AutoSize = true;
      this.labelTone22kState.Location = new System.Drawing.Point(12, 177);
      this.labelTone22kState.Name = "labelTone22kState";
      this.labelTone22kState.Size = new System.Drawing.Size(94, 13);
      this.labelTone22kState.TabIndex = 13;
      this.labelTone22kState.Text = "22 kHz tone state:";
      // 
      // labelToneBurst
      // 
      this.labelToneBurst.AutoSize = true;
      this.labelToneBurst.Location = new System.Drawing.Point(12, 150);
      this.labelToneBurst.Name = "labelToneBurst";
      this.labelToneBurst.Size = new System.Drawing.Size(61, 13);
      this.labelToneBurst.TabIndex = 11;
      this.labelToneBurst.Text = "Tone burst:";
      // 
      // comboBoxToneBurst
      // 
      this.comboBoxToneBurst.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxToneBurst.FormattingEnabled = true;
      this.comboBoxToneBurst.Location = new System.Drawing.Point(133, 147);
      this.comboBoxToneBurst.Name = "comboBoxToneBurst";
      this.comboBoxToneBurst.Size = new System.Drawing.Size(200, 21);
      this.comboBoxToneBurst.TabIndex = 12;
      // 
      // labelDiseqcMotorPosition
      // 
      this.labelDiseqcMotorPosition.AutoSize = true;
      this.labelDiseqcMotorPosition.Location = new System.Drawing.Point(12, 123);
      this.labelDiseqcMotorPosition.Name = "labelDiseqcMotorPosition";
      this.labelDiseqcMotorPosition.Size = new System.Drawing.Size(115, 13);
      this.labelDiseqcMotorPosition.TabIndex = 8;
      this.labelDiseqcMotorPosition.Text = "DiSEqC motor position:";
      // 
      // labelDiseqcSwitchPort
      // 
      this.labelDiseqcSwitchPort.AutoSize = true;
      this.labelDiseqcSwitchPort.Location = new System.Drawing.Point(12, 96);
      this.labelDiseqcSwitchPort.Name = "labelDiseqcSwitchPort";
      this.labelDiseqcSwitchPort.Size = new System.Drawing.Size(101, 13);
      this.labelDiseqcSwitchPort.TabIndex = 6;
      this.labelDiseqcSwitchPort.Text = "DiSEqC switch port:";
      // 
      // comboBoxDiseqcSwitchPort
      // 
      this.comboBoxDiseqcSwitchPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxDiseqcSwitchPort.FormattingEnabled = true;
      this.comboBoxDiseqcSwitchPort.Location = new System.Drawing.Point(133, 93);
      this.comboBoxDiseqcSwitchPort.Name = "comboBoxDiseqcSwitchPort";
      this.comboBoxDiseqcSwitchPort.Size = new System.Drawing.Size(200, 21);
      this.comboBoxDiseqcSwitchPort.TabIndex = 7;
      // 
      // labelLnbType
      // 
      this.labelLnbType.AutoSize = true;
      this.labelLnbType.Location = new System.Drawing.Point(12, 69);
      this.labelLnbType.Name = "labelLnbType";
      this.labelLnbType.Size = new System.Drawing.Size(54, 13);
      this.labelLnbType.TabIndex = 4;
      this.labelLnbType.Text = "LNB type:";
      // 
      // comboBoxLnbType
      // 
      this.comboBoxLnbType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxLnbType.FormattingEnabled = true;
      this.comboBoxLnbType.Location = new System.Drawing.Point(133, 66);
      this.comboBoxLnbType.Name = "comboBoxLnbType";
      this.comboBoxLnbType.Size = new System.Drawing.Size(200, 21);
      this.comboBoxLnbType.TabIndex = 5;
      this.comboBoxLnbType.SelectedIndexChanged += new System.EventHandler(this.comboBoxLnbType_SelectedIndexChanged);
      this.comboBoxLnbType.Enter += new System.EventHandler(this.comboBoxLnbType_Enter);
      // 
      // labelTuner
      // 
      this.labelTuner.AutoSize = true;
      this.labelTuner.Location = new System.Drawing.Point(12, 42);
      this.labelTuner.Name = "labelTuner";
      this.labelTuner.Size = new System.Drawing.Size(38, 13);
      this.labelTuner.TabIndex = 2;
      this.labelTuner.Text = "Tuner:";
      // 
      // comboBoxTuner
      // 
      this.comboBoxTuner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxTuner.FormattingEnabled = true;
      this.comboBoxTuner.Location = new System.Drawing.Point(133, 39);
      this.comboBoxTuner.Name = "comboBoxTuner";
      this.comboBoxTuner.Size = new System.Drawing.Size(200, 21);
      this.comboBoxTuner.TabIndex = 3;
      // 
      // labelSatellite
      // 
      this.labelSatellite.AutoSize = true;
      this.labelSatellite.Location = new System.Drawing.Point(12, 15);
      this.labelSatellite.Name = "labelSatellite";
      this.labelSatellite.Size = new System.Drawing.Size(47, 13);
      this.labelSatellite.TabIndex = 0;
      this.labelSatellite.Text = "Satellite:";
      // 
      // comboBoxSatellite
      // 
      this.comboBoxSatellite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxSatellite.FormattingEnabled = true;
      this.comboBoxSatellite.Location = new System.Drawing.Point(133, 12);
      this.comboBoxSatellite.Name = "comboBoxSatellite";
      this.comboBoxSatellite.Size = new System.Drawing.Size(200, 21);
      this.comboBoxSatellite.TabIndex = 1;
      this.comboBoxSatellite.SelectedIndexChanged += new System.EventHandler(this.comboBoxSatellite_SelectedIndexChanged);
      // 
      // numericUpDownDiseqcMotorPosition
      // 
      this.numericUpDownDiseqcMotorPosition.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericUpDownDiseqcMotorPosition.Location = new System.Drawing.Point(217, 121);
      this.numericUpDownDiseqcMotorPosition.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
      this.numericUpDownDiseqcMotorPosition.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownDiseqcMotorPosition.Name = "numericUpDownDiseqcMotorPosition";
      this.numericUpDownDiseqcMotorPosition.Size = new System.Drawing.Size(45, 20);
      this.numericUpDownDiseqcMotorPosition.TabIndex = 10;
      this.numericUpDownDiseqcMotorPosition.TruncateDecimalPlaces = false;
      this.numericUpDownDiseqcMotorPosition.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // comboBoxDiseqcMotorPositionType
      // 
      this.comboBoxDiseqcMotorPositionType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxDiseqcMotorPositionType.FormattingEnabled = true;
      this.comboBoxDiseqcMotorPositionType.Location = new System.Drawing.Point(133, 120);
      this.comboBoxDiseqcMotorPositionType.Name = "comboBoxDiseqcMotorPositionType";
      this.comboBoxDiseqcMotorPositionType.Size = new System.Drawing.Size(70, 21);
      this.comboBoxDiseqcMotorPositionType.TabIndex = 9;
      this.comboBoxDiseqcMotorPositionType.SelectedIndexChanged += new System.EventHandler(this.comboBoxDiseqcMotorPositionType_SelectedIndexChanged);
      // 
      // FormEditTunerSatellite
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(345, 270);
      this.Controls.Add(this.comboBoxDiseqcMotorPositionType);
      this.Controls.Add(this.numericUpDownDiseqcMotorPosition);
      this.Controls.Add(this.labelSatellite);
      this.Controls.Add(this.comboBoxSatellite);
      this.Controls.Add(this.labelTuner);
      this.Controls.Add(this.comboBoxTuner);
      this.Controls.Add(this.labelLnbType);
      this.Controls.Add(this.comboBoxLnbType);
      this.Controls.Add(this.labelDiseqcSwitchPort);
      this.Controls.Add(this.comboBoxDiseqcSwitchPort);
      this.Controls.Add(this.labelDiseqcMotorPosition);
      this.Controls.Add(this.labelToneBurst);
      this.Controls.Add(this.comboBoxToneBurst);
      this.Controls.Add(this.labelTone22kState);
      this.Controls.Add(this.comboBoxTone22kState);
      this.Controls.Add(this.labelIsToroidalDish);
      this.Controls.Add(this.checkBoxIsToroidalDish);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOkay);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "FormEditTunerSatellite";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add/Edit Tuner Satellite";
      this.Load += new System.EventHandler(this.FormEditTunerSatellite_Load);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiseqcMotorPosition)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    protected MPButton buttonCancel;
    protected MPButton buttonOkay;
    protected MPLabel labelIsToroidalDish;
    protected MPCheckBox checkBoxIsToroidalDish;
    private MPComboBox comboBoxTone22kState;
    protected MPLabel labelTone22kState;
    protected MPLabel labelToneBurst;
    private MPComboBox comboBoxToneBurst;
    protected MPLabel labelDiseqcMotorPosition;
    protected MPLabel labelDiseqcSwitchPort;
    private MPComboBox comboBoxDiseqcSwitchPort;
    protected MPLabel labelLnbType;
    private MPComboBox comboBoxLnbType;
    protected MPLabel labelTuner;
    private MPComboBox comboBoxTuner;
    protected MPLabel labelSatellite;
    private MPComboBox comboBoxSatellite;
    private MPNumericUpDown numericUpDownDiseqcMotorPosition;
    private MPComboBox comboBoxDiseqcMotorPositionType;
  }
}
