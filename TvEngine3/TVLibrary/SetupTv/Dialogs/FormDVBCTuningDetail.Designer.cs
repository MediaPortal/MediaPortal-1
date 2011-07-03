namespace SetupTv.Dialogs
{
  partial class FormDVBCTuningDetail
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
      this.comboBoxDvbCModulation = new System.Windows.Forms.ComboBox();
      this.label42 = new System.Windows.Forms.Label();
      this.checkBoxDVBCfta = new System.Windows.Forms.CheckBox();
      this.label46 = new System.Windows.Forms.Label();
      this.textBoxDVBCProvider = new System.Windows.Forms.TextBox();
      this.textBoxDVBCPmt = new System.Windows.Forms.TextBox();
      this.textBoxSymbolRate = new System.Windows.Forms.TextBox();
      this.textBoxSID = new System.Windows.Forms.TextBox();
      this.textBoxTSID = new System.Windows.Forms.TextBox();
      this.textBoxONID = new System.Windows.Forms.TextBox();
      this.textboxFreq = new System.Windows.Forms.TextBox();
      this.label43 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.textBoxChannel = new System.Windows.Forms.TextBox();
      this.label47 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Location = new System.Drawing.Point(188, 275);
      this.mpButtonCancel.TabIndex = 11;
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(90, 275);
      this.mpButtonOk.TabIndex = 10;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // comboBoxDvbCModulation
      // 
      this.comboBoxDvbCModulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDvbCModulation.FormattingEnabled = true;
      this.comboBoxDvbCModulation.Location = new System.Drawing.Point(90, 86);
      this.comboBoxDvbCModulation.Name = "comboBoxDvbCModulation";
      this.comboBoxDvbCModulation.Size = new System.Drawing.Size(147, 21);
      this.comboBoxDvbCModulation.TabIndex = 3;
      // 
      // label42
      // 
      this.label42.AutoSize = true;
      this.label42.Location = new System.Drawing.Point(13, 89);
      this.label42.Name = "label42";
      this.label42.Size = new System.Drawing.Size(62, 13);
      this.label42.TabIndex = 98;
      this.label42.Text = "Modulation:";
      // 
      // checkBoxDVBCfta
      // 
      this.checkBoxDVBCfta.AutoSize = true;
      this.checkBoxDVBCfta.Location = new System.Drawing.Point(90, 243);
      this.checkBoxDVBCfta.Name = "checkBoxDVBCfta";
      this.checkBoxDVBCfta.Size = new System.Drawing.Size(78, 17);
      this.checkBoxDVBCfta.TabIndex = 9;
      this.checkBoxDVBCfta.Text = "Free To Air";
      this.checkBoxDVBCfta.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.checkBoxDVBCfta.UseVisualStyleBackColor = true;
      // 
      // label46
      // 
      this.label46.AutoSize = true;
      this.label46.Location = new System.Drawing.Point(13, 220);
      this.label46.Name = "label46";
      this.label46.Size = new System.Drawing.Size(49, 13);
      this.label46.TabIndex = 95;
      this.label46.Text = "Provider:";
      // 
      // textBoxDVBCProvider
      // 
      this.textBoxDVBCProvider.Location = new System.Drawing.Point(90, 217);
      this.textBoxDVBCProvider.Name = "textBoxDVBCProvider";
      this.textBoxDVBCProvider.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBCProvider.TabIndex = 8;
      // 
      // textBoxDVBCPmt
      // 
      this.textBoxDVBCPmt.Location = new System.Drawing.Point(90, 191);
      this.textBoxDVBCPmt.Name = "textBoxDVBCPmt";
      this.textBoxDVBCPmt.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBCPmt.TabIndex = 7;
      this.textBoxDVBCPmt.Text = "-1";
      // 
      // textBoxSymbolRate
      // 
      this.textBoxSymbolRate.Location = new System.Drawing.Point(90, 60);
      this.textBoxSymbolRate.Name = "textBoxSymbolRate";
      this.textBoxSymbolRate.Size = new System.Drawing.Size(146, 20);
      this.textBoxSymbolRate.TabIndex = 2;
      this.textBoxSymbolRate.Text = "6875";
      // 
      // textBoxSID
      // 
      this.textBoxSID.Location = new System.Drawing.Point(90, 165);
      this.textBoxSID.Name = "textBoxSID";
      this.textBoxSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxSID.TabIndex = 6;
      this.textBoxSID.Text = "-1";
      // 
      // textBoxTSID
      // 
      this.textBoxTSID.Location = new System.Drawing.Point(90, 139);
      this.textBoxTSID.Name = "textBoxTSID";
      this.textBoxTSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxTSID.TabIndex = 5;
      this.textBoxTSID.Text = "-1";
      // 
      // textBoxONID
      // 
      this.textBoxONID.Location = new System.Drawing.Point(90, 113);
      this.textBoxONID.Name = "textBoxONID";
      this.textBoxONID.Size = new System.Drawing.Size(146, 20);
      this.textBoxONID.TabIndex = 4;
      this.textBoxONID.Text = "-1";
      // 
      // textboxFreq
      // 
      this.textboxFreq.Location = new System.Drawing.Point(90, 34);
      this.textboxFreq.Name = "textboxFreq";
      this.textboxFreq.Size = new System.Drawing.Size(147, 20);
      this.textboxFreq.TabIndex = 1;
      this.textboxFreq.Text = "388000";
      // 
      // label43
      // 
      this.label43.AutoSize = true;
      this.label43.Location = new System.Drawing.Point(13, 194);
      this.label43.Name = "label43";
      this.label43.Size = new System.Drawing.Size(54, 13);
      this.label43.TabIndex = 93;
      this.label43.Text = "PMT PID:";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(13, 63);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(70, 13);
      this.label6.TabIndex = 91;
      this.label6.Text = "Symbol Rate:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(13, 142);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(69, 13);
      this.label4.TabIndex = 90;
      this.label4.Text = "Transport ID:";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(13, 168);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(60, 13);
      this.label5.TabIndex = 89;
      this.label5.Text = "Service ID:";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(13, 116);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(64, 13);
      this.label7.TabIndex = 88;
      this.label7.Text = "Network ID:";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(13, 37);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(60, 13);
      this.label8.TabIndex = 87;
      this.label8.Text = "Frequency:";
      // 
      // textBoxChannel
      // 
      this.textBoxChannel.Location = new System.Drawing.Point(90, 8);
      this.textBoxChannel.Name = "textBoxChannel";
      this.textBoxChannel.Size = new System.Drawing.Size(147, 20);
      this.textBoxChannel.TabIndex = 0;
      this.textBoxChannel.Text = "0";
      // 
      // label47
      // 
      this.label47.AutoSize = true;
      this.label47.Location = new System.Drawing.Point(13, 11);
      this.label47.Name = "label47";
      this.label47.Size = new System.Drawing.Size(49, 13);
      this.label47.TabIndex = 119;
      this.label47.Text = "Channel:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(239, 37);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(26, 13);
      this.label1.TabIndex = 120;
      this.label1.Text = "kHz";
      // 
      // FormDVBCTuningDetail
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(275, 310);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textBoxChannel);
      this.Controls.Add(this.label47);
      this.Controls.Add(this.comboBoxDvbCModulation);
      this.Controls.Add(this.label42);
      this.Controls.Add(this.checkBoxDVBCfta);
      this.Controls.Add(this.label46);
      this.Controls.Add(this.textBoxDVBCProvider);
      this.Controls.Add(this.textBoxDVBCPmt);
      this.Controls.Add(this.textBoxSymbolRate);
      this.Controls.Add(this.textBoxSID);
      this.Controls.Add(this.textBoxTSID);
      this.Controls.Add(this.textBoxONID);
      this.Controls.Add(this.textboxFreq);
      this.Controls.Add(this.label43);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.label8);
      this.Name = "FormDVBCTuningDetail";
      this.Text = "Add / Edit DVB-C Tuningdetail";
      this.Load += new System.EventHandler(this.FormDVBCTuningDetail_Load);
      this.Controls.SetChildIndex(this.mpButtonOk, 0);
      this.Controls.SetChildIndex(this.mpButtonCancel, 0);
      this.Controls.SetChildIndex(this.label8, 0);
      this.Controls.SetChildIndex(this.label7, 0);
      this.Controls.SetChildIndex(this.label5, 0);
      this.Controls.SetChildIndex(this.label4, 0);
      this.Controls.SetChildIndex(this.label6, 0);
      this.Controls.SetChildIndex(this.label43, 0);
      this.Controls.SetChildIndex(this.textboxFreq, 0);
      this.Controls.SetChildIndex(this.textBoxONID, 0);
      this.Controls.SetChildIndex(this.textBoxTSID, 0);
      this.Controls.SetChildIndex(this.textBoxSID, 0);
      this.Controls.SetChildIndex(this.textBoxSymbolRate, 0);
      this.Controls.SetChildIndex(this.textBoxDVBCPmt, 0);
      this.Controls.SetChildIndex(this.textBoxDVBCProvider, 0);
      this.Controls.SetChildIndex(this.label46, 0);
      this.Controls.SetChildIndex(this.checkBoxDVBCfta, 0);
      this.Controls.SetChildIndex(this.label42, 0);
      this.Controls.SetChildIndex(this.comboBoxDvbCModulation, 0);
      this.Controls.SetChildIndex(this.label47, 0);
      this.Controls.SetChildIndex(this.textBoxChannel, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBoxDvbCModulation;
    private System.Windows.Forms.Label label42;
    private System.Windows.Forms.CheckBox checkBoxDVBCfta;
    private System.Windows.Forms.Label label46;
    private System.Windows.Forms.TextBox textBoxDVBCProvider;
    private System.Windows.Forms.TextBox textBoxDVBCPmt;
    private System.Windows.Forms.TextBox textBoxSymbolRate;
    private System.Windows.Forms.TextBox textBoxSID;
    private System.Windows.Forms.TextBox textBoxTSID;
    private System.Windows.Forms.TextBox textBoxONID;
    private System.Windows.Forms.TextBox textboxFreq;
    private System.Windows.Forms.Label label43;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox textBoxChannel;
    private System.Windows.Forms.Label label47;
    private System.Windows.Forms.Label label1;
  }
}
