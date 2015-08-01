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
      this.radioButtonFmRadio = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonScte = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonAnalogTv = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonStream = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonDvbT = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonSatellite = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonDvbC = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonAtsc = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.radioButtonCapture = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.groupBoxType.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(93, 248);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // buttonOkay
      // 
      this.buttonOkay.Location = new System.Drawing.Point(12, 248);
      this.buttonOkay.Name = "buttonOkay";
      this.buttonOkay.Size = new System.Drawing.Size(75, 23);
      this.buttonOkay.TabIndex = 1;
      this.buttonOkay.Text = "&OK";
      this.buttonOkay.UseVisualStyleBackColor = true;
      this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
      // 
      // groupBoxType
      // 
      this.groupBoxType.Controls.Add(this.radioButtonCapture);
      this.groupBoxType.Controls.Add(this.radioButtonFmRadio);
      this.groupBoxType.Controls.Add(this.radioButtonScte);
      this.groupBoxType.Controls.Add(this.radioButtonAnalogTv);
      this.groupBoxType.Controls.Add(this.radioButtonStream);
      this.groupBoxType.Controls.Add(this.radioButtonDvbT);
      this.groupBoxType.Controls.Add(this.radioButtonSatellite);
      this.groupBoxType.Controls.Add(this.radioButtonDvbC);
      this.groupBoxType.Controls.Add(this.radioButtonAtsc);
      this.groupBoxType.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxType.Location = new System.Drawing.Point(12, 12);
      this.groupBoxType.Name = "groupBoxType";
      this.groupBoxType.Size = new System.Drawing.Size(156, 230);
      this.groupBoxType.TabIndex = 0;
      this.groupBoxType.TabStop = false;
      this.groupBoxType.Text = "Types";
      // 
      // radioButtonFmRadio
      // 
      this.radioButtonFmRadio.AutoSize = true;
      this.radioButtonFmRadio.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonFmRadio.Location = new System.Drawing.Point(6, 134);
      this.radioButtonFmRadio.Name = "radioButtonFmRadio";
      this.radioButtonFmRadio.Size = new System.Drawing.Size(70, 17);
      this.radioButtonFmRadio.TabIndex = 5;
      this.radioButtonFmRadio.Text = "FM Radio";
      this.radioButtonFmRadio.UseVisualStyleBackColor = true;
      // 
      // radioButtonScte
      // 
      this.radioButtonScte.AutoSize = true;
      this.radioButtonScte.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonScte.Location = new System.Drawing.Point(6, 180);
      this.radioButtonScte.Name = "radioButtonScte";
      this.radioButtonScte.Size = new System.Drawing.Size(52, 17);
      this.radioButtonScte.TabIndex = 7;
      this.radioButtonScte.Text = "SCTE";
      this.radioButtonScte.UseVisualStyleBackColor = true;
      // 
      // radioButtonAnalogTv
      // 
      this.radioButtonAnalogTv.AutoSize = true;
      this.radioButtonAnalogTv.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonAnalogTv.Location = new System.Drawing.Point(6, 19);
      this.radioButtonAnalogTv.Name = "radioButtonAnalogTv";
      this.radioButtonAnalogTv.Size = new System.Drawing.Size(74, 17);
      this.radioButtonAnalogTv.TabIndex = 0;
      this.radioButtonAnalogTv.Text = "Analog TV";
      this.radioButtonAnalogTv.UseVisualStyleBackColor = true;
      // 
      // radioButtonStream
      // 
      this.radioButtonStream.AutoSize = true;
      this.radioButtonStream.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonStream.Location = new System.Drawing.Point(6, 203);
      this.radioButtonStream.Name = "radioButtonStream";
      this.radioButtonStream.Size = new System.Drawing.Size(57, 17);
      this.radioButtonStream.TabIndex = 8;
      this.radioButtonStream.Text = "Stream";
      this.radioButtonStream.UseVisualStyleBackColor = true;
      // 
      // radioButtonDvbT
      // 
      this.radioButtonDvbT.AutoSize = true;
      this.radioButtonDvbT.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonDvbT.Location = new System.Drawing.Point(6, 111);
      this.radioButtonDvbT.Name = "radioButtonDvbT";
      this.radioButtonDvbT.Size = new System.Drawing.Size(74, 17);
      this.radioButtonDvbT.TabIndex = 4;
      this.radioButtonDvbT.Text = "DVB-T/T2";
      this.radioButtonDvbT.UseVisualStyleBackColor = true;
      // 
      // radioButtonSatellite
      // 
      this.radioButtonSatellite.AutoSize = true;
      this.radioButtonSatellite.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonSatellite.Location = new System.Drawing.Point(6, 157);
      this.radioButtonSatellite.Name = "radioButtonSatellite";
      this.radioButtonSatellite.Size = new System.Drawing.Size(61, 17);
      this.radioButtonSatellite.TabIndex = 6;
      this.radioButtonSatellite.Text = "Satellite";
      this.radioButtonSatellite.UseVisualStyleBackColor = true;
      // 
      // radioButtonDvbC
      // 
      this.radioButtonDvbC.AutoSize = true;
      this.radioButtonDvbC.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonDvbC.Location = new System.Drawing.Point(6, 88);
      this.radioButtonDvbC.Name = "radioButtonDvbC";
      this.radioButtonDvbC.Size = new System.Drawing.Size(56, 17);
      this.radioButtonDvbC.TabIndex = 3;
      this.radioButtonDvbC.Text = "DVB-C";
      this.radioButtonDvbC.UseVisualStyleBackColor = true;
      // 
      // radioButtonAtsc
      // 
      this.radioButtonAtsc.AutoSize = true;
      this.radioButtonAtsc.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonAtsc.Location = new System.Drawing.Point(6, 42);
      this.radioButtonAtsc.Name = "radioButtonAtsc";
      this.radioButtonAtsc.Size = new System.Drawing.Size(52, 17);
      this.radioButtonAtsc.TabIndex = 1;
      this.radioButtonAtsc.Text = "ATSC";
      this.radioButtonAtsc.UseVisualStyleBackColor = true;
      // 
      // radioButtonCapture
      // 
      this.radioButtonCapture.AutoSize = true;
      this.radioButtonCapture.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonCapture.Location = new System.Drawing.Point(6, 65);
      this.radioButtonCapture.Name = "radioButtonCapture";
      this.radioButtonCapture.Size = new System.Drawing.Size(61, 17);
      this.radioButtonCapture.TabIndex = 2;
      this.radioButtonCapture.Text = "Capture";
      this.radioButtonCapture.UseVisualStyleBackColor = true;
      // 
      // FormSelectTuningDetailType
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(181, 281);
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
    private MPRadioButton radioButtonDvbT;
    private MPRadioButton radioButtonSatellite;
    private MPRadioButton radioButtonDvbC;
    private MPRadioButton radioButtonAtsc;
    private MPRadioButton radioButtonAnalogTv;
    private MPRadioButton radioButtonFmRadio;
    private MPRadioButton radioButtonScte;
    private MPRadioButton radioButtonCapture;
  }
}
