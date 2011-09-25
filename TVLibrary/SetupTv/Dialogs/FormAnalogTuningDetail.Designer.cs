namespace SetupTv.Dialogs
{
  partial class FormAnalogTuningDetail
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
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.label51 = new System.Windows.Forms.Label();
      this.checkBoxVCR = new System.Windows.Forms.CheckBox();
      this.label30 = new System.Windows.Forms.Label();
      this.textBoxAnalogFrequency = new System.Windows.Forms.TextBox();
      this.label29 = new System.Windows.Forms.Label();
      this.textBoxChannel = new System.Windows.Forms.TextBox();
      this.comboBoxCountry = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.comboBoxInput = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.comboBoxAudioSource = new System.Windows.Forms.ComboBox();
      this.label52 = new System.Windows.Forms.Label();
      this.comboBoxVideoSource = new System.Windows.Forms.ComboBox();
      this.label28 = new System.Windows.Forms.Label();
      this.label31 = new System.Windows.Forms.Label();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Location = new System.Drawing.Point(353, 313);
      this.mpButtonCancel.TabIndex = 7;
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(254, 313);
      this.mpButtonOk.TabIndex = 6;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.label51);
      this.groupBox2.Controls.Add(this.checkBoxVCR);
      this.groupBox2.Controls.Add(this.label30);
      this.groupBox2.Controls.Add(this.textBoxAnalogFrequency);
      this.groupBox2.Controls.Add(this.label29);
      this.groupBox2.Controls.Add(this.textBoxChannel);
      this.groupBox2.Controls.Add(this.comboBoxCountry);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.comboBoxInput);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Location = new System.Drawing.Point(12, 12);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(416, 159);
      this.groupBox2.TabIndex = 24;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Tuner parameters";
      // 
      // label51
      // 
      this.label51.AutoSize = true;
      this.label51.Location = new System.Drawing.Point(15, 127);
      this.label51.Name = "label51";
      this.label51.Size = new System.Drawing.Size(64, 13);
      this.label51.TabIndex = 182;
      this.label51.Text = "VCR Signal:";
      // 
      // checkBoxVCR
      // 
      this.checkBoxVCR.AutoSize = true;
      this.checkBoxVCR.Location = new System.Drawing.Point(95, 127);
      this.checkBoxVCR.Name = "checkBoxVCR";
      this.checkBoxVCR.Size = new System.Drawing.Size(15, 14);
      this.checkBoxVCR.TabIndex = 400;
      this.checkBoxVCR.TabStop = false;
      this.checkBoxVCR.UseVisualStyleBackColor = true;
      // 
      // label30
      // 
      this.label30.AutoSize = true;
      this.label30.Location = new System.Drawing.Point(214, 48);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(123, 13);
      this.label30.TabIndex = 16;
      this.label30.Text = "MHz (leave 0 for default)";
      // 
      // textBoxAnalogFrequency
      // 
      this.textBoxAnalogFrequency.Location = new System.Drawing.Point(95, 45);
      this.textBoxAnalogFrequency.Name = "textBoxAnalogFrequency";
      this.textBoxAnalogFrequency.Size = new System.Drawing.Size(100, 20);
      this.textBoxAnalogFrequency.TabIndex = 1;
      this.textBoxAnalogFrequency.Text = "0";
      // 
      // label29
      // 
      this.label29.AutoSize = true;
      this.label29.Location = new System.Drawing.Point(15, 48);
      this.label29.Name = "label29";
      this.label29.Size = new System.Drawing.Size(60, 13);
      this.label29.TabIndex = 142;
      this.label29.Text = "Frequency:";
      // 
      // textBoxChannel
      // 
      this.textBoxChannel.Location = new System.Drawing.Point(95, 19);
      this.textBoxChannel.Name = "textBoxChannel";
      this.textBoxChannel.Size = new System.Drawing.Size(100, 20);
      this.textBoxChannel.TabIndex = 0;
      this.textBoxChannel.Text = "4";
      // 
      // comboBoxCountry
      // 
      this.comboBoxCountry.DisplayMember = "Name";
      this.comboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCountry.FormattingEnabled = true;
      this.comboBoxCountry.Location = new System.Drawing.Point(95, 100);
      this.comboBoxCountry.Name = "comboBoxCountry";
      this.comboBoxCountry.Size = new System.Drawing.Size(258, 21);
      this.comboBoxCountry.TabIndex = 3;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(15, 103);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(46, 13);
      this.label3.TabIndex = 102;
      this.label3.Text = "Country:";
      // 
      // comboBoxInput
      // 
      this.comboBoxInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxInput.IntegralHeight = false;
      this.comboBoxInput.Items.AddRange(new object[] {
            "Antenna",
            "Cable"});
      this.comboBoxInput.Location = new System.Drawing.Point(95, 73);
      this.comboBoxInput.Name = "comboBoxInput";
      this.comboBoxInput.Size = new System.Drawing.Size(258, 21);
      this.comboBoxInput.TabIndex = 2;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(15, 76);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(57, 13);
      this.label2.TabIndex = 82;
      this.label2.Text = "Input type:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(15, 26);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(49, 13);
      this.label1.TabIndex = 252;
      this.label1.Text = "Channel:";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.comboBoxAudioSource);
      this.groupBox1.Controls.Add(this.label52);
      this.groupBox1.Controls.Add(this.comboBoxVideoSource);
      this.groupBox1.Controls.Add(this.label28);
      this.groupBox1.Location = new System.Drawing.Point(14, 177);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(414, 99);
      this.groupBox1.TabIndex = 23;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Crossbar parameters";
      // 
      // comboBoxAudioSource
      // 
      this.comboBoxAudioSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxAudioSource.FormattingEnabled = true;
      this.comboBoxAudioSource.Items.AddRange(new object[] {
            "Automatic",
            "Tuner",
            "AUX In #1",
            "AUX In #2",
            "AUX In #3",
            "Line In #1",
            "Line In #2",
            "Line In #3",
            "SPDIF In #1",
            "SPDIF In #2",
            "SPDIF In #3"});
      this.comboBoxAudioSource.Location = new System.Drawing.Point(93, 59);
      this.comboBoxAudioSource.Name = "comboBoxAudioSource";
      this.comboBoxAudioSource.Size = new System.Drawing.Size(258, 21);
      this.comboBoxAudioSource.TabIndex = 5;
      // 
      // label52
      // 
      this.label52.AutoSize = true;
      this.label52.Location = new System.Drawing.Point(13, 62);
      this.label52.Name = "label52";
      this.label52.Size = new System.Drawing.Size(74, 13);
      this.label52.TabIndex = 192;
      this.label52.Text = "Audio Source:";
      // 
      // comboBoxVideoSource
      // 
      this.comboBoxVideoSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxVideoSource.FormattingEnabled = true;
      this.comboBoxVideoSource.Items.AddRange(new object[] {
            "Tuner",
            "CVBS #1",
            "CVBS #2",
            "CVBS #3",
            "SVHS #1",
            "SVHS #2",
            "SVHS #3",
            "RGB #1",
            "RGB #2",
            "RGB #3",
            "YRYBY #1",
            "YRYBY #2",
            "YRYBY #3",
            "HDMI #1",
            "HDMI #2",
            "HDMI #3"});
      this.comboBoxVideoSource.Location = new System.Drawing.Point(93, 32);
      this.comboBoxVideoSource.Name = "comboBoxVideoSource";
      this.comboBoxVideoSource.Size = new System.Drawing.Size(258, 21);
      this.comboBoxVideoSource.TabIndex = 4;
      // 
      // label28
      // 
      this.label28.AutoSize = true;
      this.label28.Location = new System.Drawing.Point(13, 35);
      this.label28.Name = "label28";
      this.label28.Size = new System.Drawing.Size(74, 13);
      this.label28.TabIndex = 122;
      this.label28.Text = "Video Source:";
      // 
      // label31
      // 
      this.label31.AutoSize = true;
      this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label31.Location = new System.Drawing.Point(28, 279);
      this.label31.Name = "label31";
      this.label31.Size = new System.Drawing.Size(360, 13);
      this.label31.TabIndex = 222;
      this.label31.Text = "Note: A reboot might be needed when changing the frequency";
      // 
      // FormAnalogTuningDetail
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(440, 348);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label31);
      this.Name = "FormAnalogTuningDetail";
      this.Text = "Add/Edit Analog Tuningdetail";
      this.Load += new System.EventHandler(this.FormAnalogTuningDetail_Load);
      this.Controls.SetChildIndex(this.mpButtonOk, 0);
      this.Controls.SetChildIndex(this.mpButtonCancel, 0);
      this.Controls.SetChildIndex(this.label31, 0);
      this.Controls.SetChildIndex(this.groupBox1, 0);
      this.Controls.SetChildIndex(this.groupBox2, 0);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label label51;
    private System.Windows.Forms.CheckBox checkBoxVCR;
    private System.Windows.Forms.Label label30;
    private System.Windows.Forms.TextBox textBoxAnalogFrequency;
    private System.Windows.Forms.Label label29;
    private System.Windows.Forms.TextBox textBoxChannel;
    private System.Windows.Forms.ComboBox comboBoxCountry;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox comboBoxInput;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox comboBoxAudioSource;
    private System.Windows.Forms.Label label52;
    private System.Windows.Forms.ComboBox comboBoxVideoSource;
    private System.Windows.Forms.Label label28;
    private System.Windows.Forms.Label label31;
  }
}
