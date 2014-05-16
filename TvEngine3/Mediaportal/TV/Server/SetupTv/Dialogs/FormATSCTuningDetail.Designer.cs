using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormATSCTuningDetail
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
      this.checkBoxQamfta = new MPCheckBox();
      this.label49 = new MPLabel();
      this.textBoxQamProvider = new MPTextBox();
      this.textBoxQamPmt = new MPTextBox();
      this.textBoxQamSID = new MPTextBox();
      this.textBoxQamTSID = new MPTextBox();
      this.textBoxQamSource = new MPTextBox();
      this.textBoxFrequency = new MPTextBox();
      this.textBoxMinor = new MPTextBox();
      this.textBoxMajor = new MPTextBox();
      this.textBoxPhysicalChannel = new MPTextBox();
      this.label26 = new MPLabel();
      this.label39 = new MPLabel();
      this.label40 = new MPLabel();
      this.label41 = new MPLabel();
      this.label38 = new MPLabel();
      this.comboBoxQAMModulation = new MPComboBox();
      this.label37 = new MPLabel();
      this.label22 = new MPLabel();
      this.label23 = new MPLabel();
      this.label24 = new MPLabel();
      this.label1 = new MPLabel();
      this.SuspendLayout();
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Location = new System.Drawing.Point(184, 308);
      this.mpButtonCancel.TabIndex = 23;
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(85, 308);
      this.mpButtonOk.TabIndex = 22;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // checkBoxQamfta
      // 
      this.checkBoxQamfta.AutoSize = true;
      this.checkBoxQamfta.Location = new System.Drawing.Point(88, 278);
      this.checkBoxQamfta.Name = "checkBoxQamfta";
      this.checkBoxQamfta.Size = new System.Drawing.Size(78, 17);
      this.checkBoxQamfta.TabIndex = 21;
      this.checkBoxQamfta.Text = "Free To Air";
      this.checkBoxQamfta.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.checkBoxQamfta.UseVisualStyleBackColor = true;
      // 
      // label49
      // 
      this.label49.AutoSize = true;
      this.label49.Location = new System.Drawing.Point(8, 255);
      this.label49.Name = "label49";
      this.label49.Size = new System.Drawing.Size(49, 13);
      this.label49.TabIndex = 19;
      this.label49.Text = "Provider:";
      // 
      // textBoxQamProvider
      // 
      this.textBoxQamProvider.Location = new System.Drawing.Point(88, 252);
      this.textBoxQamProvider.Name = "textBoxQamProvider";
      this.textBoxQamProvider.Size = new System.Drawing.Size(146, 20);
      this.textBoxQamProvider.TabIndex = 20;
      // 
      // textBoxQamPmt
      // 
      this.textBoxQamPmt.Location = new System.Drawing.Point(88, 226);
      this.textBoxQamPmt.Name = "textBoxQamPmt";
      this.textBoxQamPmt.Size = new System.Drawing.Size(146, 20);
      this.textBoxQamPmt.TabIndex = 18;
      this.textBoxQamPmt.Text = "-1";
      // 
      // textBoxQamSID
      // 
      this.textBoxQamSID.Location = new System.Drawing.Point(88, 174);
      this.textBoxQamSID.Name = "textBoxQamSID";
      this.textBoxQamSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxQamSID.TabIndex = 14;
      this.textBoxQamSID.Text = "-1";
      // 
      // textBoxQamTSID
      // 
      this.textBoxQamTSID.Location = new System.Drawing.Point(88, 148);
      this.textBoxQamTSID.Name = "textBoxQamTSID";
      this.textBoxQamTSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxQamTSID.TabIndex = 12;
      this.textBoxQamTSID.Text = "-1";
      // 
      // textBoxQamSourceID
      // 
      this.textBoxQamSource.Location = new System.Drawing.Point(88, 200);
      this.textBoxQamSource.Name = "textBoxQamSourceID";
      this.textBoxQamSource.Size = new System.Drawing.Size(146, 20);
      this.textBoxQamSource.TabIndex = 16;
      this.textBoxQamSource.Text = "-1";
      // 
      // textBoxFrequency
      // 
      this.textBoxFrequency.Location = new System.Drawing.Point(88, 43);
      this.textBoxFrequency.Name = "textBoxFrequency";
      this.textBoxFrequency.Size = new System.Drawing.Size(146, 20);
      this.textBoxFrequency.TabIndex = 3;
      this.textBoxFrequency.Text = "-1";
      // 
      // textBoxMinor
      // 
      this.textBoxMinor.Location = new System.Drawing.Point(88, 95);
      this.textBoxMinor.Name = "textBoxMinor";
      this.textBoxMinor.Size = new System.Drawing.Size(146, 20);
      this.textBoxMinor.TabIndex = 8;
      this.textBoxMinor.Text = "-1";
      // 
      // textBoxMajor
      // 
      this.textBoxMajor.Location = new System.Drawing.Point(88, 69);
      this.textBoxMajor.Name = "textBoxMajor";
      this.textBoxMajor.Size = new System.Drawing.Size(146, 20);
      this.textBoxMajor.TabIndex = 6;
      this.textBoxMajor.Text = "-1";
      // 
      // textBoxProgram
      // 
      this.textBoxPhysicalChannel.Location = new System.Drawing.Point(88, 17);
      this.textBoxPhysicalChannel.Name = "textBoxProgram";
      this.textBoxPhysicalChannel.Size = new System.Drawing.Size(146, 20);
      this.textBoxPhysicalChannel.TabIndex = 1;
      this.textBoxPhysicalChannel.Text = "1";
      // 
      // label26
      // 
      this.label26.AutoSize = true;
      this.label26.Location = new System.Drawing.Point(8, 229);
      this.label26.Name = "label26";
      this.label26.Size = new System.Drawing.Size(54, 13);
      this.label26.TabIndex = 17;
      this.label26.Text = "PMT PID:";
      // 
      // label39
      // 
      this.label39.AutoSize = true;
      this.label39.Location = new System.Drawing.Point(8, 151);
      this.label39.Name = "label39";
      this.label39.Size = new System.Drawing.Size(69, 13);
      this.label39.TabIndex = 11;
      this.label39.Text = "Transport ID:";
      // 
      // label40
      // 
      this.label40.AutoSize = true;
      this.label40.Location = new System.Drawing.Point(8, 177);
      this.label40.Name = "label40";
      this.label40.Size = new System.Drawing.Size(60, 13);
      this.label40.TabIndex = 13;
      this.label40.Text = "Service ID:";
      // 
      // label41
      // 
      this.label41.AutoSize = true;
      this.label41.Location = new System.Drawing.Point(8, 203);
      this.label41.Name = "label41";
      this.label41.Size = new System.Drawing.Size(58, 13);
      this.label41.TabIndex = 15;
      this.label41.Text = "Source ID:";
      // 
      // label38
      // 
      this.label38.AutoSize = true;
      this.label38.Location = new System.Drawing.Point(8, 46);
      this.label38.Name = "label38";
      this.label38.Size = new System.Drawing.Size(60, 13);
      this.label38.TabIndex = 2;
      this.label38.Text = "Frequency:";
      // 
      // comboBoxQAMModulation
      // 
      this.comboBoxQAMModulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxQAMModulation.FormattingEnabled = true;
      this.comboBoxQAMModulation.Items.AddRange(new object[] {
            "Not Set",
            "8 VSB",
            "64 QAM",
            "256 QAM"});
      this.comboBoxQAMModulation.Location = new System.Drawing.Point(88, 121);
      this.comboBoxQAMModulation.Name = "comboBoxQAMModulation";
      this.comboBoxQAMModulation.Size = new System.Drawing.Size(146, 21);
      this.comboBoxQAMModulation.TabIndex = 10;
      // 
      // label37
      // 
      this.label37.AutoSize = true;
      this.label37.Location = new System.Drawing.Point(8, 124);
      this.label37.Name = "label37";
      this.label37.Size = new System.Drawing.Size(62, 13);
      this.label37.TabIndex = 9;
      this.label37.Text = "Modulation:";
      // 
      // label22
      // 
      this.label22.AutoSize = true;
      this.label22.Location = new System.Drawing.Point(8, 98);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(36, 13);
      this.label22.TabIndex = 7;
      this.label22.Text = "Minor:";
      // 
      // label23
      // 
      this.label23.AutoSize = true;
      this.label23.Location = new System.Drawing.Point(8, 72);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(36, 13);
      this.label23.TabIndex = 5;
      this.label23.Text = "Major:";
      // 
      // label24
      // 
      this.label24.AutoSize = true;
      this.label24.Location = new System.Drawing.Point(8, 20);
      this.label24.Name = "label24";
      this.label24.Size = new System.Drawing.Size(49, 13);
      this.label24.TabIndex = 0;
      this.label24.Text = "Channel:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(235, 46);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(26, 13);
      this.label1.TabIndex = 4;
      this.label1.Text = "kHz";
      // 
      // FormATSCTuningDetail
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(271, 343);
      this.Controls.Add(this.textBoxQamProvider);
      this.Controls.Add(this.textBoxQamPmt);
      this.Controls.Add(this.checkBoxQamfta);
      this.Controls.Add(this.label49);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textBoxFrequency);
      this.Controls.Add(this.textBoxMinor);
      this.Controls.Add(this.textBoxQamSID);
      this.Controls.Add(this.textBoxQamTSID);
      this.Controls.Add(this.textBoxMajor);
      this.Controls.Add(this.textBoxPhysicalChannel);
      this.Controls.Add(this.textBoxQamSource);
      this.Controls.Add(this.label26);
      this.Controls.Add(this.label39);
      this.Controls.Add(this.label38);
      this.Controls.Add(this.label40);
      this.Controls.Add(this.comboBoxQAMModulation);
      this.Controls.Add(this.label37);
      this.Controls.Add(this.label41);
      this.Controls.Add(this.label22);
      this.Controls.Add(this.label23);
      this.Controls.Add(this.label24);
      this.Name = "FormATSCTuningDetail";
      this.Text = "Add / Edit ATSC Tuningdetail";
      this.Load += new System.EventHandler(this.FormATSCTuningDetail_Load);
      this.Controls.SetChildIndex(this.label24, 0);
      this.Controls.SetChildIndex(this.label23, 0);
      this.Controls.SetChildIndex(this.label22, 0);
      this.Controls.SetChildIndex(this.label41, 0);
      this.Controls.SetChildIndex(this.label37, 0);
      this.Controls.SetChildIndex(this.comboBoxQAMModulation, 0);
      this.Controls.SetChildIndex(this.label40, 0);
      this.Controls.SetChildIndex(this.label38, 0);
      this.Controls.SetChildIndex(this.label39, 0);
      this.Controls.SetChildIndex(this.label26, 0);
      this.Controls.SetChildIndex(this.textBoxQamSource, 0);
      this.Controls.SetChildIndex(this.textBoxPhysicalChannel, 0);
      this.Controls.SetChildIndex(this.textBoxMajor, 0);
      this.Controls.SetChildIndex(this.textBoxQamTSID, 0);
      this.Controls.SetChildIndex(this.textBoxQamSID, 0);
      this.Controls.SetChildIndex(this.textBoxMinor, 0);
      this.Controls.SetChildIndex(this.textBoxFrequency, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.label49, 0);
      this.Controls.SetChildIndex(this.checkBoxQamfta, 0);
      this.Controls.SetChildIndex(this.textBoxQamPmt, 0);
      this.Controls.SetChildIndex(this.textBoxQamProvider, 0);
      this.Controls.SetChildIndex(this.mpButtonOk, 0);
      this.Controls.SetChildIndex(this.mpButtonCancel, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPCheckBox checkBoxQamfta;
    private MPLabel label49;
    private MPTextBox textBoxQamProvider;
    private MPTextBox textBoxQamPmt;
    private MPTextBox textBoxQamSID;
    private MPTextBox textBoxQamTSID;
    private MPTextBox textBoxQamSource;
    private MPTextBox textBoxFrequency;
    private MPTextBox textBoxMinor;
    private MPTextBox textBoxMajor;
    private MPTextBox textBoxPhysicalChannel;
    private MPLabel label26;
    private MPLabel label39;
    private MPLabel label40;
    private MPLabel label41;
    private MPLabel label38;
    private MPComboBox comboBoxQAMModulation;
    private MPLabel label37;
    private MPLabel label22;
    private MPLabel label23;
    private MPLabel label24;
    private MPLabel label1;

  }
}
