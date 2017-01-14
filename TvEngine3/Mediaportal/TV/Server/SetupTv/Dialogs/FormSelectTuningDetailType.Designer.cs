using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormSelectTuningDetailType
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
      this.groupBoxType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.radioButtonCapture = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonFmRadio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonScte = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonAnalogTv = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonStream = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonTerrestrial = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonSatellite = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonCable = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonAtsc = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonAmRadio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.groupBoxType.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(93, 269);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // buttonOkay
      // 
      this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonOkay.Location = new System.Drawing.Point(12, 269);
      this.buttonOkay.Name = "buttonOkay";
      this.buttonOkay.Size = new System.Drawing.Size(75, 23);
      this.buttonOkay.TabIndex = 1;
      this.buttonOkay.Text = "&OK";
      this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
      // 
      // groupBoxType
      // 
      this.groupBoxType.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxType.Controls.Add(this.radioButtonAmRadio);
      this.groupBoxType.Controls.Add(this.radioButtonCapture);
      this.groupBoxType.Controls.Add(this.radioButtonFmRadio);
      this.groupBoxType.Controls.Add(this.radioButtonScte);
      this.groupBoxType.Controls.Add(this.radioButtonAnalogTv);
      this.groupBoxType.Controls.Add(this.radioButtonStream);
      this.groupBoxType.Controls.Add(this.radioButtonTerrestrial);
      this.groupBoxType.Controls.Add(this.radioButtonSatellite);
      this.groupBoxType.Controls.Add(this.radioButtonCable);
      this.groupBoxType.Controls.Add(this.radioButtonAtsc);
      this.groupBoxType.Location = new System.Drawing.Point(12, 12);
      this.groupBoxType.Name = "groupBoxType";
      this.groupBoxType.Size = new System.Drawing.Size(156, 251);
      this.groupBoxType.TabIndex = 0;
      this.groupBoxType.TabStop = false;
      this.groupBoxType.Text = "Types";
      // 
      // radioButtonCapture
      // 
      this.radioButtonCapture.AutoSize = true;
      this.radioButtonCapture.Location = new System.Drawing.Point(6, 88);
      this.radioButtonCapture.Name = "radioButtonCapture";
      this.radioButtonCapture.Size = new System.Drawing.Size(61, 17);
      this.radioButtonCapture.TabIndex = 3;
      this.radioButtonCapture.Text = "Capture";
      // 
      // radioButtonFmRadio
      // 
      this.radioButtonFmRadio.AutoSize = true;
      this.radioButtonFmRadio.Location = new System.Drawing.Point(6, 157);
      this.radioButtonFmRadio.Name = "radioButtonFmRadio";
      this.radioButtonFmRadio.Size = new System.Drawing.Size(70, 17);
      this.radioButtonFmRadio.TabIndex = 6;
      this.radioButtonFmRadio.Text = "FM Radio";
      // 
      // radioButtonScte
      // 
      this.radioButtonScte.AutoSize = true;
      this.radioButtonScte.Location = new System.Drawing.Point(6, 203);
      this.radioButtonScte.Name = "radioButtonScte";
      this.radioButtonScte.Size = new System.Drawing.Size(52, 17);
      this.radioButtonScte.TabIndex = 8;
      this.radioButtonScte.Text = "SCTE";
      // 
      // radioButtonAnalogTv
      // 
      this.radioButtonAnalogTv.AutoSize = true;
      this.radioButtonAnalogTv.Location = new System.Drawing.Point(6, 42);
      this.radioButtonAnalogTv.Name = "radioButtonAnalogTv";
      this.radioButtonAnalogTv.Size = new System.Drawing.Size(74, 17);
      this.radioButtonAnalogTv.TabIndex = 1;
      this.radioButtonAnalogTv.Text = "Analog TV";
      // 
      // radioButtonStream
      // 
      this.radioButtonStream.AutoSize = true;
      this.radioButtonStream.Location = new System.Drawing.Point(6, 226);
      this.radioButtonStream.Name = "radioButtonStream";
      this.radioButtonStream.Size = new System.Drawing.Size(57, 17);
      this.radioButtonStream.TabIndex = 9;
      this.radioButtonStream.Text = "Stream";
      // 
      // radioButtonTerrestrial
      // 
      this.radioButtonTerrestrial.AutoSize = true;
      this.radioButtonTerrestrial.Location = new System.Drawing.Point(6, 134);
      this.radioButtonTerrestrial.Name = "radioButtonTerrestrial";
      this.radioButtonTerrestrial.Size = new System.Drawing.Size(115, 17);
      this.radioButtonTerrestrial.TabIndex = 5;
      this.radioButtonTerrestrial.Text = "DVB-T/T2, ISDB-T";
      // 
      // radioButtonSatellite
      // 
      this.radioButtonSatellite.AutoSize = true;
      this.radioButtonSatellite.Location = new System.Drawing.Point(6, 180);
      this.radioButtonSatellite.Name = "radioButtonSatellite";
      this.radioButtonSatellite.Size = new System.Drawing.Size(61, 17);
      this.radioButtonSatellite.TabIndex = 7;
      this.radioButtonSatellite.Text = "Satellite";
      // 
      // radioButtonCable
      // 
      this.radioButtonCable.AutoSize = true;
      this.radioButtonCable.Location = new System.Drawing.Point(6, 111);
      this.radioButtonCable.Name = "radioButtonCable";
      this.radioButtonCable.Size = new System.Drawing.Size(97, 17);
      this.radioButtonCable.TabIndex = 4;
      this.radioButtonCable.Text = "DVB-C, ISDB-C";
      // 
      // radioButtonAtsc
      // 
      this.radioButtonAtsc.AutoSize = true;
      this.radioButtonAtsc.Location = new System.Drawing.Point(6, 65);
      this.radioButtonAtsc.Name = "radioButtonAtsc";
      this.radioButtonAtsc.Size = new System.Drawing.Size(52, 17);
      this.radioButtonAtsc.TabIndex = 2;
      this.radioButtonAtsc.Text = "ATSC";
      // 
      // radioButtonAmRadio
      // 
      this.radioButtonAmRadio.AutoSize = true;
      this.radioButtonAmRadio.Location = new System.Drawing.Point(6, 19);
      this.radioButtonAmRadio.Name = "radioButtonAmRadio";
      this.radioButtonAmRadio.Size = new System.Drawing.Size(71, 17);
      this.radioButtonAmRadio.TabIndex = 0;
      this.radioButtonAmRadio.Text = "AM Radio";
      // 
      // FormSelectTuningDetailType
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(181, 302);
      this.Controls.Add(this.groupBoxType);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOkay);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormSelectTuningDetailType";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Select Tuning Detail Type";
      this.groupBoxType.ResumeLayout(false);
      this.groupBoxType.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MPButton buttonCancel;
    private MPButton buttonOkay;
    private MPGroupBox groupBoxType;
    private MPRadioButton radioButtonStream;
    private MPRadioButton radioButtonTerrestrial;
    private MPRadioButton radioButtonSatellite;
    private MPRadioButton radioButtonCable;
    private MPRadioButton radioButtonAtsc;
    private MPRadioButton radioButtonAnalogTv;
    private MPRadioButton radioButtonFmRadio;
    private MPRadioButton radioButtonScte;
    private MPRadioButton radioButtonCapture;
    private MPRadioButton radioButtonAmRadio;
  }
}
