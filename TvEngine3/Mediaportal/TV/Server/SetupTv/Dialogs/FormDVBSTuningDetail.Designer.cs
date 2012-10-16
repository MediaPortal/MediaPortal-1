using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormDVBSTuningDetail
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
      this.checkBoxDVBSfta = new System.Windows.Forms.CheckBox();
      this.label27 = new System.Windows.Forms.Label();
      this.textBoxDVBSProvider = new System.Windows.Forms.TextBox();
      this.textBoxDVBSPmt = new System.Windows.Forms.TextBox();
      this.textBoxDVBSChannel = new System.Windows.Forms.TextBox();
      this.textBoxSymbolRate = new System.Windows.Forms.TextBox();
      this.textBoxServiceId = new System.Windows.Forms.TextBox();
      this.textBoxTransportId = new System.Windows.Forms.TextBox();
      this.textBoxNetworkId = new System.Windows.Forms.TextBox();
      this.textBoxFrequency = new System.Windows.Forms.TextBox();
      this.label34 = new System.Windows.Forms.Label();
      this.label47 = new System.Windows.Forms.Label();
      this.comboBoxRollOff = new System.Windows.Forms.ComboBox();
      this.label36 = new System.Windows.Forms.Label();
      this.comboBoxPilot = new System.Windows.Forms.ComboBox();
      this.label35 = new System.Windows.Forms.Label();
      this.comboBoxInnerFecRate = new System.Windows.Forms.ComboBox();
      this.label33 = new System.Windows.Forms.Label();
      this.comboBoxModulation = new System.Windows.Forms.ComboBox();
      this.label32 = new System.Windows.Forms.Label();
      this.comboBoxDiseqc = new System.Windows.Forms.ComboBox();
      this.label10 = new System.Windows.Forms.Label();
      this.comboBoxPol = new System.Windows.Forms.ComboBox();
      this.label11 = new System.Windows.Forms.Label();
      this.label12 = new System.Windows.Forms.Label();
      this.label13 = new System.Windows.Forms.Label();
      this.label14 = new System.Windows.Forms.Label();
      this.label15 = new System.Windows.Forms.Label();
      this.label16 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.comboBoxLnbType = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Location = new System.Drawing.Point(362, 282);
      this.mpButtonCancel.TabIndex = 35;
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(263, 282);
      this.mpButtonOk.TabIndex = 34;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // checkBoxDVBSfta
      // 
      this.checkBoxDVBSfta.AutoSize = true;
      this.checkBoxDVBSfta.Location = new System.Drawing.Point(335, 144);
      this.checkBoxDVBSfta.Name = "checkBoxDVBSfta";
      this.checkBoxDVBSfta.Size = new System.Drawing.Size(78, 17);
      this.checkBoxDVBSfta.TabIndex = 33;
      this.checkBoxDVBSfta.Text = "Free To Air";
      this.checkBoxDVBSfta.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.checkBoxDVBSfta.UseVisualStyleBackColor = true;
      // 
      // label27
      // 
      this.label27.AutoSize = true;
      this.label27.Location = new System.Drawing.Point(260, 119);
      this.label27.Name = "label27";
      this.label27.Size = new System.Drawing.Size(49, 13);
      this.label27.TabIndex = 31;
      this.label27.Text = "Provider:";
      // 
      // textBoxDVBSProvider
      // 
      this.textBoxDVBSProvider.Location = new System.Drawing.Point(335, 116);
      this.textBoxDVBSProvider.Name = "textBoxDVBSProvider";
      this.textBoxDVBSProvider.Size = new System.Drawing.Size(95, 20);
      this.textBoxDVBSProvider.TabIndex = 32;
      // 
      // textBoxDVBSPmt
      // 
      this.textBoxDVBSPmt.Location = new System.Drawing.Point(335, 90);
      this.textBoxDVBSPmt.Name = "textBoxDVBSPmt";
      this.textBoxDVBSPmt.Size = new System.Drawing.Size(95, 20);
      this.textBoxDVBSPmt.TabIndex = 30;
      this.textBoxDVBSPmt.Text = "-1";
      // 
      // textBoxDVBSChannel
      // 
      this.textBoxDVBSChannel.Location = new System.Drawing.Point(92, 12);
      this.textBoxDVBSChannel.Name = "textBoxDVBSChannel";
      this.textBoxDVBSChannel.Size = new System.Drawing.Size(130, 20);
      this.textBoxDVBSChannel.TabIndex = 2;
      this.textBoxDVBSChannel.Text = "0";
      // 
      // textBoxSymbolRate
      // 
      this.textBoxSymbolRate.Location = new System.Drawing.Point(93, 116);
      this.textBoxSymbolRate.Name = "textBoxSymbolRate";
      this.textBoxSymbolRate.Size = new System.Drawing.Size(130, 20);
      this.textBoxSymbolRate.TabIndex = 11;
      this.textBoxSymbolRate.Text = "22000";
      // 
      // textBoxServiceId
      // 
      this.textBoxServiceId.Location = new System.Drawing.Point(335, 64);
      this.textBoxServiceId.Name = "textBoxServiceId";
      this.textBoxServiceId.Size = new System.Drawing.Size(95, 20);
      this.textBoxServiceId.TabIndex = 28;
      this.textBoxServiceId.Text = "-1";
      // 
      // textBoxTransportId
      // 
      this.textBoxTransportId.Location = new System.Drawing.Point(335, 38);
      this.textBoxTransportId.Name = "textBoxTransportId";
      this.textBoxTransportId.Size = new System.Drawing.Size(95, 20);
      this.textBoxTransportId.TabIndex = 26;
      this.textBoxTransportId.Text = "-1";
      // 
      // textBoxNetworkId
      // 
      this.textBoxNetworkId.Location = new System.Drawing.Point(335, 12);
      this.textBoxNetworkId.Name = "textBoxNetworkId";
      this.textBoxNetworkId.Size = new System.Drawing.Size(95, 20);
      this.textBoxNetworkId.TabIndex = 24;
      this.textBoxNetworkId.Text = "-1";
      // 
      // textBoxFrequency
      // 
      this.textBoxFrequency.Location = new System.Drawing.Point(92, 90);
      this.textBoxFrequency.Name = "textBoxFrequency";
      this.textBoxFrequency.Size = new System.Drawing.Size(130, 20);
      this.textBoxFrequency.TabIndex = 8;
      this.textBoxFrequency.Text = "11097000";
      // 
      // label34
      // 
      this.label34.AutoSize = true;
      this.label34.Location = new System.Drawing.Point(260, 93);
      this.label34.Name = "label34";
      this.label34.Size = new System.Drawing.Size(54, 13);
      this.label34.TabIndex = 29;
      this.label34.Text = "PMT PID:";
      // 
      // label47
      // 
      this.label47.AutoSize = true;
      this.label47.Location = new System.Drawing.Point(11, 15);
      this.label47.Name = "label47";
      this.label47.Size = new System.Drawing.Size(49, 13);
      this.label47.TabIndex = 1;
      this.label47.Text = "Channel:";
      // 
      // comboBoxRollOff
      // 
      this.comboBoxRollOff.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxRollOff.FormattingEnabled = true;
      this.comboBoxRollOff.Items.AddRange(new object[] {
            "Not Set",
            "Not Defined",
            ".20 Roll Off",
            ".25 Roll Off",
            ".35 Roll Off"});
      this.comboBoxRollOff.Location = new System.Drawing.Point(93, 250);
      this.comboBoxRollOff.Name = "comboBoxRollOff";
      this.comboBoxRollOff.Size = new System.Drawing.Size(129, 21);
      this.comboBoxRollOff.TabIndex = 22;
      // 
      // label36
      // 
      this.label36.AutoSize = true;
      this.label36.Location = new System.Drawing.Point(11, 253);
      this.label36.Name = "label36";
      this.label36.Size = new System.Drawing.Size(45, 13);
      this.label36.TabIndex = 21;
      this.label36.Text = "Roll-Off:";
      // 
      // comboBoxPilot
      // 
      this.comboBoxPilot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPilot.FormattingEnabled = true;
      this.comboBoxPilot.Items.AddRange(new object[] {
            "Not Set",
            "Not Defined",
            "Off",
            "On"});
      this.comboBoxPilot.Location = new System.Drawing.Point(93, 223);
      this.comboBoxPilot.Name = "comboBoxPilot";
      this.comboBoxPilot.Size = new System.Drawing.Size(129, 21);
      this.comboBoxPilot.TabIndex = 20;
      // 
      // label35
      // 
      this.label35.AutoSize = true;
      this.label35.Location = new System.Drawing.Point(11, 226);
      this.label35.Name = "label35";
      this.label35.Size = new System.Drawing.Size(30, 13);
      this.label35.TabIndex = 19;
      this.label35.Text = "Pilot:";
      // 
      // comboBoxInnerFecRate
      // 
      this.comboBoxInnerFecRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxInnerFecRate.FormattingEnabled = true;
      this.comboBoxInnerFecRate.Items.AddRange(new object[] {
            "Not Set",
            "Not Defined",
            "1/2",
            "2/3",
            "3/4",
            "3/5",
            "4/5",
            "5/6",
            "5/11",
            "7/8",
            "1/4",
            "1/3",
            "2/5",
            "6/7",
            "8/9",
            "9/10"});
      this.comboBoxInnerFecRate.Location = new System.Drawing.Point(93, 196);
      this.comboBoxInnerFecRate.Name = "comboBoxInnerFecRate";
      this.comboBoxInnerFecRate.Size = new System.Drawing.Size(129, 21);
      this.comboBoxInnerFecRate.TabIndex = 18;
      // 
      // label33
      // 
      this.label33.AutoSize = true;
      this.label33.Location = new System.Drawing.Point(11, 199);
      this.label33.Name = "label33";
      this.label33.Size = new System.Drawing.Size(83, 13);
      this.label33.TabIndex = 17;
      this.label33.Text = "Inner FEC Rate:";
      // 
      // comboBoxModulation
      // 
      this.comboBoxModulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxModulation.FormattingEnabled = true;
      this.comboBoxModulation.Items.AddRange(new object[] {
            "Not Set",
            "Not Defined",
            "16 QAM",
            "32 QAM",
            "64 QAM",
            "80 QAM",
            "96 QAM",
            "112 QAM",
            "128 QAM",
            "160 QAM",
            "192 QAM",
            "224 QAM",
            "256 QAM",
            "320 QAM",
            "384 QAM",
            "448 QAM",
            "512 QAM",
            "640 QAM",
            "768 QAM",
            "896 QAM",
            "1024 QAM",
            "QPSK",
            "BPSK",
            "OQPSK",
            "8 VSB",
            "16 VSB",
            "Analog Amplitude",
            "Analog Frequency",
            "8 PSK",
            "RF",
            "16 APSK",
            "32 APSK",
            "QPSK2 (DVB-S2)",
            "8 PSK2 (DVB-S2)",
            "DirectTV"});
      this.comboBoxModulation.Location = new System.Drawing.Point(93, 169);
      this.comboBoxModulation.Name = "comboBoxModulation";
      this.comboBoxModulation.Size = new System.Drawing.Size(129, 21);
      this.comboBoxModulation.TabIndex = 16;
      // 
      // label32
      // 
      this.label32.AutoSize = true;
      this.label32.Location = new System.Drawing.Point(11, 172);
      this.label32.Name = "label32";
      this.label32.Size = new System.Drawing.Size(62, 13);
      this.label32.TabIndex = 15;
      this.label32.Text = "Modulation:";
      // 
      // comboBoxDiseqc
      // 
      this.comboBoxDiseqc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDiseqc.FormattingEnabled = true;
      this.comboBoxDiseqc.Items.AddRange(new object[] {
            "None",
            "Simple A (tone burst)",
            "Simple B (data burst)",
            "Port A (option A, position A)",
            "Port B (option A, position B)",
            "Port C (option B, position A)",
            "Port D (option B, position B)",
            "Port 1",
            "Port 2",
            "Port 3",
            "Port 4",
            "Port 5",
            "Port 6",
            "Port 7",
            "Port 8",
            "Port 9",
            "Port 10",
            "Port 11",
            "Port 12",
            "Port 13",
            "Port 14",
            "Port 15",
            "Port 16"});
      this.comboBoxDiseqc.Location = new System.Drawing.Point(93, 37);
      this.comboBoxDiseqc.Name = "comboBoxDiseqc";
      this.comboBoxDiseqc.Size = new System.Drawing.Size(129, 21);
      this.comboBoxDiseqc.TabIndex = 4;
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(11, 41);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(47, 13);
      this.label10.TabIndex = 3;
      this.label10.Text = "DiSEqC:";
      // 
      // comboBoxPol
      // 
      this.comboBoxPol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPol.FormattingEnabled = true;
      this.comboBoxPol.Items.AddRange(new object[] {
            "Not Set",
            "Not Defined",
            "Horizontal",
            "Vertical",
            "Circular Left",
            "Circular Right"});
      this.comboBoxPol.Location = new System.Drawing.Point(93, 142);
      this.comboBoxPol.Name = "comboBoxPol";
      this.comboBoxPol.Size = new System.Drawing.Size(129, 21);
      this.comboBoxPol.TabIndex = 14;
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(11, 145);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(64, 13);
      this.label11.TabIndex = 13;
      this.label11.Text = "Polarisation:";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(11, 119);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(70, 13);
      this.label12.TabIndex = 10;
      this.label12.Text = "Symbol Rate:";
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(260, 41);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(69, 13);
      this.label13.TabIndex = 25;
      this.label13.Text = "Transport ID:";
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(260, 67);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(60, 13);
      this.label14.TabIndex = 27;
      this.label14.Text = "Service ID:";
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(260, 15);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(64, 13);
      this.label15.TabIndex = 23;
      this.label15.Text = "Network ID:";
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(11, 93);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(60, 13);
      this.label16.TabIndex = 7;
      this.label16.Text = "Frequency:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(222, 93);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(26, 13);
      this.label1.TabIndex = 9;
      this.label1.Text = "kHz";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(223, 119);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(28, 13);
      this.label2.TabIndex = 12;
      this.label2.Text = "ks/s";
      // 
      // comboBoxLnbType
      // 
      this.comboBoxLnbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxLnbType.FormattingEnabled = true;
      IList<LnbType> tempLnbTypes = ServiceAgents.Instance.CardServiceAgent.ListAllLnbTypes();
      LnbType[] lnbTypes = new LnbType[tempLnbTypes.Count];
      tempLnbTypes.CopyTo(lnbTypes, 0);
      this.comboBoxLnbType.Items.AddRange(lnbTypes);
      this.comboBoxLnbType.Location = new System.Drawing.Point(93, 63);
      this.comboBoxLnbType.Name = "comboBoxLnbType";
      this.comboBoxLnbType.Size = new System.Drawing.Size(129, 21);
      this.comboBoxLnbType.TabIndex = 6;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(11, 67);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(58, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "LNB Type:";
      // 
      // FormDVBSTuningDetail
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(449, 317);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.comboBoxLnbType);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.checkBoxDVBSfta);
      this.Controls.Add(this.label27);
      this.Controls.Add(this.textBoxDVBSProvider);
      this.Controls.Add(this.textBoxDVBSPmt);
      this.Controls.Add(this.textBoxDVBSChannel);
      this.Controls.Add(this.textBoxSymbolRate);
      this.Controls.Add(this.textBoxServiceId);
      this.Controls.Add(this.textBoxTransportId);
      this.Controls.Add(this.textBoxNetworkId);
      this.Controls.Add(this.textBoxFrequency);
      this.Controls.Add(this.label34);
      this.Controls.Add(this.label47);
      this.Controls.Add(this.comboBoxRollOff);
      this.Controls.Add(this.label36);
      this.Controls.Add(this.comboBoxPilot);
      this.Controls.Add(this.label35);
      this.Controls.Add(this.comboBoxInnerFecRate);
      this.Controls.Add(this.label33);
      this.Controls.Add(this.comboBoxModulation);
      this.Controls.Add(this.label32);
      this.Controls.Add(this.comboBoxDiseqc);
      this.Controls.Add(this.label10);
      this.Controls.Add(this.comboBoxPol);
      this.Controls.Add(this.label11);
      this.Controls.Add(this.label12);
      this.Controls.Add(this.label13);
      this.Controls.Add(this.label14);
      this.Controls.Add(this.label15);
      this.Controls.Add(this.label16);
      this.Name = "FormDVBSTuningDetail";
      this.Text = "Add / Edit DVB-S Tuningdetail";
      this.Load += new System.EventHandler(this.FormDVBSTuningDetail_Load);
      this.Controls.SetChildIndex(this.mpButtonOk, 0);
      this.Controls.SetChildIndex(this.mpButtonCancel, 0);
      this.Controls.SetChildIndex(this.label16, 0);
      this.Controls.SetChildIndex(this.label15, 0);
      this.Controls.SetChildIndex(this.label14, 0);
      this.Controls.SetChildIndex(this.label13, 0);
      this.Controls.SetChildIndex(this.label12, 0);
      this.Controls.SetChildIndex(this.label11, 0);
      this.Controls.SetChildIndex(this.comboBoxPol, 0);
      this.Controls.SetChildIndex(this.label10, 0);
      this.Controls.SetChildIndex(this.comboBoxDiseqc, 0);
      this.Controls.SetChildIndex(this.label32, 0);
      this.Controls.SetChildIndex(this.comboBoxModulation, 0);
      this.Controls.SetChildIndex(this.label33, 0);
      this.Controls.SetChildIndex(this.comboBoxInnerFecRate, 0);
      this.Controls.SetChildIndex(this.label35, 0);
      this.Controls.SetChildIndex(this.comboBoxPilot, 0);
      this.Controls.SetChildIndex(this.label36, 0);
      this.Controls.SetChildIndex(this.comboBoxRollOff, 0);
      this.Controls.SetChildIndex(this.label47, 0);
      this.Controls.SetChildIndex(this.label34, 0);
      this.Controls.SetChildIndex(this.textBoxFrequency, 0);
      this.Controls.SetChildIndex(this.textBoxNetworkId, 0);
      this.Controls.SetChildIndex(this.textBoxTransportId, 0);
      this.Controls.SetChildIndex(this.textBoxServiceId, 0);
      this.Controls.SetChildIndex(this.textBoxSymbolRate, 0);
      this.Controls.SetChildIndex(this.textBoxDVBSChannel, 0);
      this.Controls.SetChildIndex(this.textBoxDVBSPmt, 0);
      this.Controls.SetChildIndex(this.textBoxDVBSProvider, 0);
      this.Controls.SetChildIndex(this.label27, 0);
      this.Controls.SetChildIndex(this.checkBoxDVBSfta, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.label2, 0);
      this.Controls.SetChildIndex(this.comboBoxLnbType, 0);
      this.Controls.SetChildIndex(this.label3, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.CheckBox checkBoxDVBSfta;
    private System.Windows.Forms.Label label27;
    private System.Windows.Forms.TextBox textBoxDVBSProvider;
    private System.Windows.Forms.TextBox textBoxDVBSPmt;
    private System.Windows.Forms.TextBox textBoxDVBSChannel;
    private System.Windows.Forms.TextBox textBoxSymbolRate;
    private System.Windows.Forms.TextBox textBoxServiceId;
    private System.Windows.Forms.TextBox textBoxTransportId;
    private System.Windows.Forms.TextBox textBoxNetworkId;
    private System.Windows.Forms.TextBox textBoxFrequency;
    private System.Windows.Forms.Label label34;
    private System.Windows.Forms.Label label47;
    private System.Windows.Forms.ComboBox comboBoxRollOff;
    private System.Windows.Forms.Label label36;
    private System.Windows.Forms.ComboBox comboBoxPilot;
    private System.Windows.Forms.Label label35;
    private System.Windows.Forms.ComboBox comboBoxInnerFecRate;
    private System.Windows.Forms.Label label33;
    private System.Windows.Forms.ComboBox comboBoxModulation;
    private System.Windows.Forms.Label label32;
    private System.Windows.Forms.ComboBox comboBoxDiseqc;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.ComboBox comboBoxPol;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox comboBoxLnbType;
    private System.Windows.Forms.Label label3;
  }
}
