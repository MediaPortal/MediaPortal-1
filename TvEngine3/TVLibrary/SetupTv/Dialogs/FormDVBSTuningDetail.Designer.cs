namespace SetupTv.Dialogs
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
      this.textBoxSwitch = new System.Windows.Forms.TextBox();
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
      this.label9 = new System.Windows.Forms.Label();
      this.comboBoxDisEqc = new System.Windows.Forms.ComboBox();
      this.label10 = new System.Windows.Forms.Label();
      this.comboBoxPol = new System.Windows.Forms.ComboBox();
      this.label11 = new System.Windows.Forms.Label();
      this.label12 = new System.Windows.Forms.Label();
      this.label13 = new System.Windows.Forms.Label();
      this.label14 = new System.Windows.Forms.Label();
      this.label15 = new System.Windows.Forms.Label();
      this.label16 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Location = new System.Drawing.Point(320, 341);
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(221, 341);
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // checkBoxDVBSfta
      // 
      this.checkBoxDVBSfta.AutoSize = true;
      this.checkBoxDVBSfta.Location = new System.Drawing.Point(276, 119);
      this.checkBoxDVBSfta.Name = "checkBoxDVBSfta";
      this.checkBoxDVBSfta.Size = new System.Drawing.Size(78, 17);
      this.checkBoxDVBSfta.TabIndex = 107;
      this.checkBoxDVBSfta.Text = "Free To Air";
      this.checkBoxDVBSfta.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.checkBoxDVBSfta.UseVisualStyleBackColor = true;
      // 
      // label27
      // 
      this.label27.AutoSize = true;
      this.label27.Location = new System.Drawing.Point(220, 92);
      this.label27.Name = "label27";
      this.label27.Size = new System.Drawing.Size(49, 13);
      this.label27.TabIndex = 106;
      this.label27.Text = "Provider:";
      // 
      // textBoxDVBSProvider
      // 
      this.textBoxDVBSProvider.Location = new System.Drawing.Point(276, 89);
      this.textBoxDVBSProvider.Name = "textBoxDVBSProvider";
      this.textBoxDVBSProvider.Size = new System.Drawing.Size(114, 20);
      this.textBoxDVBSProvider.TabIndex = 105;
      // 
      // textBoxDVBSPmt
      // 
      this.textBoxDVBSPmt.Location = new System.Drawing.Point(276, 63);
      this.textBoxDVBSPmt.Name = "textBoxDVBSPmt";
      this.textBoxDVBSPmt.Size = new System.Drawing.Size(114, 20);
      this.textBoxDVBSPmt.TabIndex = 104;
      this.textBoxDVBSPmt.Text = "-1";
      // 
      // textBoxDVBSChannel
      // 
      this.textBoxDVBSChannel.Location = new System.Drawing.Point(84, 12);
      this.textBoxDVBSChannel.Name = "textBoxDVBSChannel";
      this.textBoxDVBSChannel.Size = new System.Drawing.Size(130, 20);
      this.textBoxDVBSChannel.TabIndex = 101;
      this.textBoxDVBSChannel.Text = "0";
      // 
      // textBoxSwitch
      // 
      this.textBoxSwitch.Location = new System.Drawing.Point(84, 163);
      this.textBoxSwitch.Name = "textBoxSwitch";
      this.textBoxSwitch.Size = new System.Drawing.Size(129, 20);
      this.textBoxSwitch.TabIndex = 82;
      this.textBoxSwitch.Text = "11700000";
      // 
      // textBoxSymbolRate
      // 
      this.textBoxSymbolRate.Location = new System.Drawing.Point(84, 139);
      this.textBoxSymbolRate.Name = "textBoxSymbolRate";
      this.textBoxSymbolRate.Size = new System.Drawing.Size(129, 20);
      this.textBoxSymbolRate.TabIndex = 81;
      this.textBoxSymbolRate.Text = "22000";
      // 
      // textBoxServiceId
      // 
      this.textBoxServiceId.Location = new System.Drawing.Point(84, 115);
      this.textBoxServiceId.Name = "textBoxServiceId";
      this.textBoxServiceId.Size = new System.Drawing.Size(129, 20);
      this.textBoxServiceId.TabIndex = 80;
      this.textBoxServiceId.Text = "-1";
      // 
      // textBoxTransportId
      // 
      this.textBoxTransportId.Location = new System.Drawing.Point(84, 89);
      this.textBoxTransportId.Name = "textBoxTransportId";
      this.textBoxTransportId.Size = new System.Drawing.Size(129, 20);
      this.textBoxTransportId.TabIndex = 79;
      this.textBoxTransportId.Text = "-1";
      // 
      // textBoxNetworkId
      // 
      this.textBoxNetworkId.Location = new System.Drawing.Point(84, 63);
      this.textBoxNetworkId.Name = "textBoxNetworkId";
      this.textBoxNetworkId.Size = new System.Drawing.Size(129, 20);
      this.textBoxNetworkId.TabIndex = 78;
      this.textBoxNetworkId.Text = "-1";
      // 
      // textBoxFrequency
      // 
      this.textBoxFrequency.Location = new System.Drawing.Point(84, 37);
      this.textBoxFrequency.Name = "textBoxFrequency";
      this.textBoxFrequency.Size = new System.Drawing.Size(129, 20);
      this.textBoxFrequency.TabIndex = 77;
      this.textBoxFrequency.Text = "11097000";
      // 
      // label34
      // 
      this.label34.AutoSize = true;
      this.label34.Location = new System.Drawing.Point(220, 66);
      this.label34.Name = "label34";
      this.label34.Size = new System.Drawing.Size(43, 13);
      this.label34.TabIndex = 103;
      this.label34.Text = "PmtPid:";
      // 
      // label47
      // 
      this.label47.AutoSize = true;
      this.label47.Location = new System.Drawing.Point(10, 15);
      this.label47.Name = "label47";
      this.label47.Size = new System.Drawing.Size(49, 13);
      this.label47.TabIndex = 102;
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
            ".35 Roll Off",
            "Max"});
      this.comboBoxRollOff.Location = new System.Drawing.Point(276, 37);
      this.comboBoxRollOff.Name = "comboBoxRollOff";
      this.comboBoxRollOff.Size = new System.Drawing.Size(114, 21);
      this.comboBoxRollOff.TabIndex = 99;
      // 
      // label36
      // 
      this.label36.AutoSize = true;
      this.label36.Location = new System.Drawing.Point(220, 41);
      this.label36.Name = "label36";
      this.label36.Size = new System.Drawing.Size(45, 13);
      this.label36.TabIndex = 100;
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
            "On",
            "Max"});
      this.comboBoxPilot.Location = new System.Drawing.Point(276, 10);
      this.comboBoxPilot.Name = "comboBoxPilot";
      this.comboBoxPilot.Size = new System.Drawing.Size(114, 21);
      this.comboBoxPilot.TabIndex = 97;
      // 
      // label35
      // 
      this.label35.AutoSize = true;
      this.label35.Location = new System.Drawing.Point(220, 15);
      this.label35.Name = "label35";
      this.label35.Size = new System.Drawing.Size(30, 13);
      this.label35.TabIndex = 98;
      this.label35.Text = "Pilot:";
      // 
      // comboBoxInnerFecRate
      // 
      this.comboBoxInnerFecRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxInnerFecRate.FormattingEnabled = true;
      this.comboBoxInnerFecRate.Items.AddRange(new object[] {
            "Default",
            "Undefined",
            "1/2",
            "2/3",
            "3/4",
            "3/5",
            "4/5",
            "5/6",
            "5/11",
            "7/8",
            "9/10",
            "Max"});
      this.comboBoxInnerFecRate.Location = new System.Drawing.Point(85, 270);
      this.comboBoxInnerFecRate.Name = "comboBoxInnerFecRate";
      this.comboBoxInnerFecRate.Size = new System.Drawing.Size(129, 21);
      this.comboBoxInnerFecRate.TabIndex = 86;
      // 
      // label33
      // 
      this.label33.AutoSize = true;
      this.label33.Location = new System.Drawing.Point(10, 273);
      this.label33.Name = "label33";
      this.label33.Size = new System.Drawing.Size(75, 13);
      this.label33.TabIndex = 96;
      this.label33.Text = "InnerFecRate:";
      // 
      // comboBoxModulation
      // 
      this.comboBoxModulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxModulation.FormattingEnabled = true;
      this.comboBoxModulation.Items.AddRange(new object[] {
            "ModNotSet",
            "ModNotDefined",
            "Mod16Qam",
            "Mod32Qam",
            "Mod64Qam",
            "Mod80Qam",
            "Mod96Qam",
            "Mod112Qam",
            "Mod128Qam",
            "Mod160Qam",
            "Mod192Qam",
            "Mod224Qam",
            "Mod256Qam",
            "Mod320Qam",
            "Mod384Qam",
            "Mod448Qam",
            "Mod512Qam",
            "Mod640Qam",
            "Mod768Qam",
            "Mod896Qam",
            "Mod1024Qam",
            "ModQpsk",
            "ModBpsk",
            "ModOqpsk",
            "Mod8Vsb",
            "Mod16Vsb",
            "ModAnalogAmplitude",
            "ModAnalogFrequency",
            "Mod8psk",
            "ModRf",
            "Mod16Apsk",
            "Mod32Apsk",
            "ModQpsk2",
            "Mod8psk2",
            "ModDirectTV",
            "ModMax"});
      this.comboBoxModulation.Location = new System.Drawing.Point(84, 244);
      this.comboBoxModulation.Name = "comboBoxModulation";
      this.comboBoxModulation.Size = new System.Drawing.Size(129, 21);
      this.comboBoxModulation.TabIndex = 85;
      // 
      // label32
      // 
      this.label32.AutoSize = true;
      this.label32.Location = new System.Drawing.Point(10, 249);
      this.label32.Name = "label32";
      this.label32.Size = new System.Drawing.Size(62, 13);
      this.label32.TabIndex = 95;
      this.label32.Text = "Modulation:";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(10, 166);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(63, 13);
      this.label9.TabIndex = 94;
      this.label9.Text = "LBN Switch";
      // 
      // comboBoxDisEqc
      // 
      this.comboBoxDisEqc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDisEqc.FormattingEnabled = true;
      this.comboBoxDisEqc.Items.AddRange(new object[] {
            "None",
            "Simple A",
            "Simple B",
            "Level 1 A/A",
            "Level 1 B/A",
            "Level 1 A/B",
            "Level 1 B/B"});
      this.comboBoxDisEqc.Location = new System.Drawing.Point(84, 217);
      this.comboBoxDisEqc.Name = "comboBoxDisEqc";
      this.comboBoxDisEqc.Size = new System.Drawing.Size(129, 21);
      this.comboBoxDisEqc.TabIndex = 84;
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(10, 222);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(44, 13);
      this.label10.TabIndex = 93;
      this.label10.Text = "DisEqc:";
      // 
      // comboBoxPol
      // 
      this.comboBoxPol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPol.FormattingEnabled = true;
      this.comboBoxPol.Items.AddRange(new object[] {
            "NotSet",
            "NotDefined",
            "Horizontal",
            "Vertical",
            "CircularL",
            "CircularR",
            "Max"});
      this.comboBoxPol.Location = new System.Drawing.Point(84, 189);
      this.comboBoxPol.Name = "comboBoxPol";
      this.comboBoxPol.Size = new System.Drawing.Size(129, 21);
      this.comboBoxPol.TabIndex = 83;
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(10, 191);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(64, 13);
      this.label11.TabIndex = 92;
      this.label11.Text = "Polarisation:";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(10, 142);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(59, 13);
      this.label12.TabIndex = 91;
      this.label12.Text = "Symbolrate";
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(10, 92);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(64, 13);
      this.label13.TabIndex = 90;
      this.label13.Text = "TransportId:";
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(10, 119);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(55, 13);
      this.label14.TabIndex = 89;
      this.label14.Text = "ServiceId:";
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(10, 66);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(59, 13);
      this.label15.TabIndex = 88;
      this.label15.Text = "NetworkId:";
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(10, 37);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(60, 13);
      this.label16.TabIndex = 87;
      this.label16.Text = "Frequency:";
      // 
      // FormDVBSTuningDetail
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(407, 376);
      this.Controls.Add(this.checkBoxDVBSfta);
      this.Controls.Add(this.label27);
      this.Controls.Add(this.textBoxDVBSProvider);
      this.Controls.Add(this.textBoxDVBSPmt);
      this.Controls.Add(this.textBoxDVBSChannel);
      this.Controls.Add(this.textBoxSwitch);
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
      this.Controls.Add(this.label9);
      this.Controls.Add(this.comboBoxDisEqc);
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
      this.Controls.SetChildIndex(this.comboBoxDisEqc, 0);
      this.Controls.SetChildIndex(this.label9, 0);
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
      this.Controls.SetChildIndex(this.textBoxSwitch, 0);
      this.Controls.SetChildIndex(this.textBoxDVBSChannel, 0);
      this.Controls.SetChildIndex(this.textBoxDVBSPmt, 0);
      this.Controls.SetChildIndex(this.textBoxDVBSProvider, 0);
      this.Controls.SetChildIndex(this.label27, 0);
      this.Controls.SetChildIndex(this.checkBoxDVBSfta, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.CheckBox checkBoxDVBSfta;
    private System.Windows.Forms.Label label27;
    private System.Windows.Forms.TextBox textBoxDVBSProvider;
    private System.Windows.Forms.TextBox textBoxDVBSPmt;
    private System.Windows.Forms.TextBox textBoxDVBSChannel;
    private System.Windows.Forms.TextBox textBoxSwitch;
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
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.ComboBox comboBoxDisEqc;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.ComboBox comboBoxPol;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.Label label16;
  }
}
