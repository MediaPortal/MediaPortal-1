namespace TestApp
{
  partial class FormDVBSChannel
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
      this.textBoxSymbolRate = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.buttonOK = new System.Windows.Forms.Button();
      this.textBoxSID = new System.Windows.Forms.TextBox();
      this.textBoxTSID = new System.Windows.Forms.TextBox();
      this.textBoxONID = new System.Windows.Forms.TextBox();
      this.textboxFreq = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.comboBoxPol = new System.Windows.Forms.ComboBox();
      this.label5 = new System.Windows.Forms.Label();
      this.comboBoxDisEqc = new System.Windows.Forms.ComboBox();
      this.label7 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.textBoxSwitch = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // textBoxSymbolRate
      // 
      this.textBoxSymbolRate.Location = new System.Drawing.Point(106, 125);
      this.textBoxSymbolRate.Name = "textBoxSymbolRate";
      this.textBoxSymbolRate.Size = new System.Drawing.Size(146, 20);
      this.textBoxSymbolRate.TabIndex = 34;
      this.textBoxSymbolRate.Text = "22000";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(28, 128);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(59, 13);
      this.label6.TabIndex = 33;
      this.label6.Text = "Symbolrate";
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(177, 243);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(75, 23);
      this.buttonOK.TabIndex = 32;
      this.buttonOK.Text = "Ok";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // textBoxSID
      // 
      this.textBoxSID.Location = new System.Drawing.Point(106, 102);
      this.textBoxSID.Name = "textBoxSID";
      this.textBoxSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxSID.TabIndex = 31;
      this.textBoxSID.Text = "-1";
      // 
      // textBoxTSID
      // 
      this.textBoxTSID.Location = new System.Drawing.Point(106, 76);
      this.textBoxTSID.Name = "textBoxTSID";
      this.textBoxTSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxTSID.TabIndex = 30;
      this.textBoxTSID.Text = "-1";
      // 
      // textBoxONID
      // 
      this.textBoxONID.Location = new System.Drawing.Point(106, 50);
      this.textBoxONID.Name = "textBoxONID";
      this.textBoxONID.Size = new System.Drawing.Size(146, 20);
      this.textBoxONID.TabIndex = 29;
      this.textBoxONID.Text = "-1";
      // 
      // textboxFreq
      // 
      this.textboxFreq.Location = new System.Drawing.Point(106, 24);
      this.textboxFreq.Name = "textboxFreq";
      this.textboxFreq.Size = new System.Drawing.Size(146, 20);
      this.textboxFreq.TabIndex = 28;
      this.textboxFreq.Text = "12515000";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(28, 79);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(64, 13);
      this.label4.TabIndex = 27;
      this.label4.Text = "TransportId:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(28, 106);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(55, 13);
      this.label3.TabIndex = 26;
      this.label3.Text = "ServiceId:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(28, 53);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(59, 13);
      this.label2.TabIndex = 25;
      this.label2.Text = "NetworkId:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(28, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(60, 13);
      this.label1.TabIndex = 24;
      this.label1.Text = "Frequency:";
      // 
      // comboBoxPol
      // 
      this.comboBoxPol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPol.FormattingEnabled = true;
      this.comboBoxPol.Items.AddRange(new object[] {
            "Horizontal",
            "Vertical"});
      this.comboBoxPol.Location = new System.Drawing.Point(106, 176);
      this.comboBoxPol.Name = "comboBoxPol";
      this.comboBoxPol.Size = new System.Drawing.Size(146, 21);
      this.comboBoxPol.TabIndex = 36;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(28, 178);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(64, 13);
      this.label5.TabIndex = 35;
      this.label5.Text = "Polarisation:";
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
      this.comboBoxDisEqc.Location = new System.Drawing.Point(106, 204);
      this.comboBoxDisEqc.Name = "comboBoxDisEqc";
      this.comboBoxDisEqc.Size = new System.Drawing.Size(146, 21);
      this.comboBoxDisEqc.TabIndex = 38;
      this.comboBoxDisEqc.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(28, 209);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(44, 13);
      this.label7.TabIndex = 37;
      this.label7.Text = "DisEqc:";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(29, 153);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(63, 13);
      this.label8.TabIndex = 39;
      this.label8.Text = "LBN Switch";
      // 
      // textBoxSwitch
      // 
      this.textBoxSwitch.Location = new System.Drawing.Point(106, 150);
      this.textBoxSwitch.Name = "textBoxSwitch";
      this.textBoxSwitch.Size = new System.Drawing.Size(146, 20);
      this.textBoxSwitch.TabIndex = 40;
      this.textBoxSwitch.Text = "11700000";
      this.textBoxSwitch.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
      // 
      // FormDVBSChannel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 306);
      this.Controls.Add(this.textBoxSwitch);
      this.Controls.Add(this.label8);
      this.Controls.Add(this.comboBoxDisEqc);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.comboBoxPol);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.textBoxSymbolRate);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.textBoxSID);
      this.Controls.Add(this.textBoxTSID);
      this.Controls.Add(this.textBoxONID);
      this.Controls.Add(this.textboxFreq);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "FormDVBSChannel";
      this.Text = "DVBS Channel";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox textBoxSymbolRate;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.TextBox textBoxSID;
    private System.Windows.Forms.TextBox textBoxTSID;
    private System.Windows.Forms.TextBox textBoxONID;
    private System.Windows.Forms.TextBox textboxFreq;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox comboBoxPol;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.ComboBox comboBoxDisEqc;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox textBoxSwitch;
  }
}