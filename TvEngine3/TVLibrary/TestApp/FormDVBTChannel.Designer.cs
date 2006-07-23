namespace TestApp
{
  partial class FormDVBTChannel
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
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.textboxFreq = new System.Windows.Forms.TextBox();
      this.textBoxONID = new System.Windows.Forms.TextBox();
      this.textBoxTSID = new System.Windows.Forms.TextBox();
      this.textBoxSID = new System.Windows.Forms.TextBox();
      this.comboBoxBandWidth = new System.Windows.Forms.ComboBox();
      this.buttonOK = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(60, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Frequency:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(13, 42);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(59, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "NetworkId:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(13, 95);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(55, 13);
      this.label3.TabIndex = 2;
      this.label3.Text = "ServiceId:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(13, 68);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(64, 13);
      this.label4.TabIndex = 3;
      this.label4.Text = "TransportId:";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(13, 123);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(60, 13);
      this.label5.TabIndex = 4;
      this.label5.Text = "Bandwidth:";
      // 
      // textboxFreq
      // 
      this.textboxFreq.Location = new System.Drawing.Point(91, 13);
      this.textboxFreq.Name = "textboxFreq";
      this.textboxFreq.Size = new System.Drawing.Size(146, 20);
      this.textboxFreq.TabIndex = 5;
      this.textboxFreq.Text = "698000";
      // 
      // textBoxONID
      // 
      this.textBoxONID.Location = new System.Drawing.Point(91, 39);
      this.textBoxONID.Name = "textBoxONID";
      this.textBoxONID.Size = new System.Drawing.Size(146, 20);
      this.textBoxONID.TabIndex = 6;
      this.textBoxONID.Text = "-1";
      // 
      // textBoxTSID
      // 
      this.textBoxTSID.Location = new System.Drawing.Point(91, 65);
      this.textBoxTSID.Name = "textBoxTSID";
      this.textBoxTSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxTSID.TabIndex = 7;
      this.textBoxTSID.Text = "-1";
      // 
      // textBoxSID
      // 
      this.textBoxSID.Location = new System.Drawing.Point(91, 91);
      this.textBoxSID.Name = "textBoxSID";
      this.textBoxSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxSID.TabIndex = 8;
      this.textBoxSID.Text = "-1";
      // 
      // comboBoxBandWidth
      // 
      this.comboBoxBandWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxBandWidth.FormattingEnabled = true;
      this.comboBoxBandWidth.Items.AddRange(new object[] {
            "7 MHz",
            "8 MHz"});
      this.comboBoxBandWidth.Location = new System.Drawing.Point(91, 118);
      this.comboBoxBandWidth.Name = "comboBoxBandWidth";
      this.comboBoxBandWidth.Size = new System.Drawing.Size(146, 21);
      this.comboBoxBandWidth.TabIndex = 9;
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(162, 161);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(75, 23);
      this.buttonOK.TabIndex = 10;
      this.buttonOK.Text = "Ok";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // FormDVBTChannel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 214);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.comboBoxBandWidth);
      this.Controls.Add(this.textBoxSID);
      this.Controls.Add(this.textBoxTSID);
      this.Controls.Add(this.textBoxONID);
      this.Controls.Add(this.textboxFreq);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "FormDVBTChannel";
      this.Text = "DVBT Channel";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox textboxFreq;
    private System.Windows.Forms.TextBox textBoxONID;
    private System.Windows.Forms.TextBox textBoxTSID;
    private System.Windows.Forms.TextBox textBoxSID;
    private System.Windows.Forms.ComboBox comboBoxBandWidth;
    private System.Windows.Forms.Button buttonOK;
  }
}