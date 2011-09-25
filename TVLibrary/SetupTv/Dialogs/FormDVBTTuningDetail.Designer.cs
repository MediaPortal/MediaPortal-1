namespace SetupTv.Dialogs
{
  partial class FormDVBTTuningDetail
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
      this.checkBoxDVBTfta = new System.Windows.Forms.CheckBox();
      this.label48 = new System.Windows.Forms.Label();
      this.textBoxDVBTProvider = new System.Windows.Forms.TextBox();
      this.textBoxPmt = new System.Windows.Forms.TextBox();
      this.textBoxDVBTChannel = new System.Windows.Forms.TextBox();
      this.textBoxServiceId = new System.Windows.Forms.TextBox();
      this.textBoxTransportId = new System.Windows.Forms.TextBox();
      this.textBoxNetworkId = new System.Windows.Forms.TextBox();
      this.textBoxDVBTfreq = new System.Windows.Forms.TextBox();
      this.label50 = new System.Windows.Forms.Label();
      this.channelDVBT = new System.Windows.Forms.Label();
      this.comboBoxBandWidth = new System.Windows.Forms.ComboBox();
      this.label17 = new System.Windows.Forms.Label();
      this.label18 = new System.Windows.Forms.Label();
      this.label19 = new System.Windows.Forms.Label();
      this.label20 = new System.Windows.Forms.Label();
      this.label21 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Location = new System.Drawing.Point(197, 258);
      this.mpButtonCancel.TabIndex = 10;
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(106, 258);
      this.mpButtonOk.TabIndex = 9;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // checkBoxDVBTfta
      // 
      this.checkBoxDVBTfta.AutoSize = true;
      this.checkBoxDVBTfta.Location = new System.Drawing.Point(87, 221);
      this.checkBoxDVBTfta.Name = "checkBoxDVBTfta";
      this.checkBoxDVBTfta.Size = new System.Drawing.Size(78, 17);
      this.checkBoxDVBTfta.TabIndex = 8;
      this.checkBoxDVBTfta.Text = "Free To Air";
      this.checkBoxDVBTfta.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.checkBoxDVBTfta.UseVisualStyleBackColor = true;
      // 
      // label48
      // 
      this.label48.AutoSize = true;
      this.label48.Location = new System.Drawing.Point(9, 198);
      this.label48.Name = "label48";
      this.label48.Size = new System.Drawing.Size(49, 13);
      this.label48.TabIndex = 95;
      this.label48.Text = "Provider:";
      // 
      // textBoxDVBTProvider
      // 
      this.textBoxDVBTProvider.Location = new System.Drawing.Point(87, 195);
      this.textBoxDVBTProvider.Name = "textBoxDVBTProvider";
      this.textBoxDVBTProvider.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBTProvider.TabIndex = 7;
      // 
      // textBoxPmt
      // 
      this.textBoxPmt.Location = new System.Drawing.Point(87, 169);
      this.textBoxPmt.Name = "textBoxPmt";
      this.textBoxPmt.Size = new System.Drawing.Size(146, 20);
      this.textBoxPmt.TabIndex = 6;
      this.textBoxPmt.Text = "-1";
      // 
      // textBoxDVBTChannel
      // 
      this.textBoxDVBTChannel.Location = new System.Drawing.Point(87, 12);
      this.textBoxDVBTChannel.Name = "textBoxDVBTChannel";
      this.textBoxDVBTChannel.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBTChannel.TabIndex = 0;
      this.textBoxDVBTChannel.Text = "0";
      // 
      // textBoxServiceId
      // 
      this.textBoxServiceId.Location = new System.Drawing.Point(87, 143);
      this.textBoxServiceId.Name = "textBoxServiceId";
      this.textBoxServiceId.Size = new System.Drawing.Size(146, 20);
      this.textBoxServiceId.TabIndex = 5;
      this.textBoxServiceId.Text = "-1";
      // 
      // textBoxTransportId
      // 
      this.textBoxTransportId.Location = new System.Drawing.Point(87, 117);
      this.textBoxTransportId.Name = "textBoxTransportId";
      this.textBoxTransportId.Size = new System.Drawing.Size(146, 20);
      this.textBoxTransportId.TabIndex = 4;
      this.textBoxTransportId.Text = "-1";
      // 
      // textBoxNetworkId
      // 
      this.textBoxNetworkId.Location = new System.Drawing.Point(87, 91);
      this.textBoxNetworkId.Name = "textBoxNetworkId";
      this.textBoxNetworkId.Size = new System.Drawing.Size(146, 20);
      this.textBoxNetworkId.TabIndex = 3;
      this.textBoxNetworkId.Text = "-1";
      // 
      // textBoxDVBTfreq
      // 
      this.textBoxDVBTfreq.Location = new System.Drawing.Point(87, 37);
      this.textBoxDVBTfreq.Name = "textBoxDVBTfreq";
      this.textBoxDVBTfreq.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBTfreq.TabIndex = 1;
      this.textBoxDVBTfreq.Text = "698000";
      // 
      // label50
      // 
      this.label50.AutoSize = true;
      this.label50.Location = new System.Drawing.Point(9, 172);
      this.label50.Name = "label50";
      this.label50.Size = new System.Drawing.Size(54, 13);
      this.label50.TabIndex = 92;
      this.label50.Text = "PMT PID:";
      // 
      // channelDVBT
      // 
      this.channelDVBT.AutoSize = true;
      this.channelDVBT.Location = new System.Drawing.Point(9, 15);
      this.channelDVBT.Name = "channelDVBT";
      this.channelDVBT.Size = new System.Drawing.Size(49, 13);
      this.channelDVBT.TabIndex = 91;
      this.channelDVBT.Text = "Channel:";
      // 
      // comboBoxBandWidth
      // 
      this.comboBoxBandWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxBandWidth.FormattingEnabled = true;
      this.comboBoxBandWidth.Items.AddRange(new object[] {
            "7 MHz",
            "8 MHz"});
      this.comboBoxBandWidth.Location = new System.Drawing.Point(87, 63);
      this.comboBoxBandWidth.Name = "comboBoxBandWidth";
      this.comboBoxBandWidth.Size = new System.Drawing.Size(146, 21);
      this.comboBoxBandWidth.TabIndex = 2;
      // 
      // label17
      // 
      this.label17.AutoSize = true;
      this.label17.Location = new System.Drawing.Point(9, 66);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(60, 13);
      this.label17.TabIndex = 89;
      this.label17.Text = "Bandwidth:";
      // 
      // label18
      // 
      this.label18.AutoSize = true;
      this.label18.Location = new System.Drawing.Point(9, 120);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(69, 13);
      this.label18.TabIndex = 88;
      this.label18.Text = "Transport ID:";
      // 
      // label19
      // 
      this.label19.AutoSize = true;
      this.label19.Location = new System.Drawing.Point(9, 147);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(60, 13);
      this.label19.TabIndex = 87;
      this.label19.Text = "Service ID:";
      // 
      // label20
      // 
      this.label20.AutoSize = true;
      this.label20.Location = new System.Drawing.Point(9, 94);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(64, 13);
      this.label20.TabIndex = 86;
      this.label20.Text = "Network ID:";
      // 
      // label21
      // 
      this.label21.AutoSize = true;
      this.label21.Location = new System.Drawing.Point(9, 40);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(60, 13);
      this.label21.TabIndex = 85;
      this.label21.Text = "Frequency:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(235, 40);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(26, 13);
      this.label1.TabIndex = 118;
      this.label1.Text = "kHz";
      // 
      // FormDVBTTuningDetail
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(284, 293);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.checkBoxDVBTfta);
      this.Controls.Add(this.label48);
      this.Controls.Add(this.textBoxDVBTProvider);
      this.Controls.Add(this.textBoxPmt);
      this.Controls.Add(this.textBoxDVBTChannel);
      this.Controls.Add(this.textBoxServiceId);
      this.Controls.Add(this.textBoxTransportId);
      this.Controls.Add(this.textBoxNetworkId);
      this.Controls.Add(this.textBoxDVBTfreq);
      this.Controls.Add(this.label50);
      this.Controls.Add(this.channelDVBT);
      this.Controls.Add(this.comboBoxBandWidth);
      this.Controls.Add(this.label17);
      this.Controls.Add(this.label18);
      this.Controls.Add(this.label19);
      this.Controls.Add(this.label20);
      this.Controls.Add(this.label21);
      this.Name = "FormDVBTTuningDetail";
      this.Text = "Add / Edit DVB-T Tuningdetail";
      this.Load += new System.EventHandler(this.FormDVBTTuningDetail_Load);
      this.Controls.SetChildIndex(this.mpButtonOk, 0);
      this.Controls.SetChildIndex(this.mpButtonCancel, 0);
      this.Controls.SetChildIndex(this.label21, 0);
      this.Controls.SetChildIndex(this.label20, 0);
      this.Controls.SetChildIndex(this.label19, 0);
      this.Controls.SetChildIndex(this.label18, 0);
      this.Controls.SetChildIndex(this.label17, 0);
      this.Controls.SetChildIndex(this.comboBoxBandWidth, 0);
      this.Controls.SetChildIndex(this.channelDVBT, 0);
      this.Controls.SetChildIndex(this.label50, 0);
      this.Controls.SetChildIndex(this.textBoxDVBTfreq, 0);
      this.Controls.SetChildIndex(this.textBoxNetworkId, 0);
      this.Controls.SetChildIndex(this.textBoxTransportId, 0);
      this.Controls.SetChildIndex(this.textBoxServiceId, 0);
      this.Controls.SetChildIndex(this.textBoxDVBTChannel, 0);
      this.Controls.SetChildIndex(this.textBoxPmt, 0);
      this.Controls.SetChildIndex(this.textBoxDVBTProvider, 0);
      this.Controls.SetChildIndex(this.label48, 0);
      this.Controls.SetChildIndex(this.checkBoxDVBTfta, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.CheckBox checkBoxDVBTfta;
    private System.Windows.Forms.Label label48;
    private System.Windows.Forms.TextBox textBoxDVBTProvider;
    private System.Windows.Forms.TextBox textBoxPmt;
    private System.Windows.Forms.TextBox textBoxDVBTChannel;
    private System.Windows.Forms.TextBox textBoxServiceId;
    private System.Windows.Forms.TextBox textBoxTransportId;
    private System.Windows.Forms.TextBox textBoxNetworkId;
    private System.Windows.Forms.TextBox textBoxDVBTfreq;
    private System.Windows.Forms.Label label50;
    private System.Windows.Forms.Label channelDVBT;
    private System.Windows.Forms.ComboBox comboBoxBandWidth;
    private System.Windows.Forms.Label label17;
    private System.Windows.Forms.Label label18;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.Label label20;
    private System.Windows.Forms.Label label21;
    private System.Windows.Forms.Label label1;
  }
}
