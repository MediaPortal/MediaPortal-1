namespace TestApp
{
  partial class FormDVBCChannel
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
      this.buttonOK = new System.Windows.Forms.Button();
      this.textBoxSID = new System.Windows.Forms.TextBox();
      this.textBoxTSID = new System.Windows.Forms.TextBox();
      this.textBoxONID = new System.Windows.Forms.TextBox();
      this.textboxFreq = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.textBoxSymbolRate = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(183, 165);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(75, 23);
      this.buttonOK.TabIndex = 21;
      this.buttonOK.Text = "Ok";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // textBoxSID
      // 
      this.textBoxSID.Location = new System.Drawing.Point(112, 105);
      this.textBoxSID.Name = "textBoxSID";
      this.textBoxSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxSID.TabIndex = 19;
      this.textBoxSID.Text = "-1";
      // 
      // textBoxTSID
      // 
      this.textBoxTSID.Location = new System.Drawing.Point(112, 79);
      this.textBoxTSID.Name = "textBoxTSID";
      this.textBoxTSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxTSID.TabIndex = 18;
      this.textBoxTSID.Text = "-1";
      // 
      // textBoxONID
      // 
      this.textBoxONID.Location = new System.Drawing.Point(112, 53);
      this.textBoxONID.Name = "textBoxONID";
      this.textBoxONID.Size = new System.Drawing.Size(146, 20);
      this.textBoxONID.TabIndex = 17;
      this.textBoxONID.Text = "-1";
      // 
      // textboxFreq
      // 
      this.textboxFreq.Location = new System.Drawing.Point(112, 27);
      this.textboxFreq.Name = "textboxFreq";
      this.textboxFreq.Size = new System.Drawing.Size(146, 20);
      this.textboxFreq.TabIndex = 16;
      this.textboxFreq.Text = "388000";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(34, 82);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(64, 13);
      this.label4.TabIndex = 14;
      this.label4.Text = "TransportId:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(34, 109);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(55, 13);
      this.label3.TabIndex = 13;
      this.label3.Text = "ServiceId:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(34, 56);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(59, 13);
      this.label2.TabIndex = 12;
      this.label2.Text = "NetworkId:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(34, 27);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(60, 13);
      this.label1.TabIndex = 11;
      this.label1.Text = "Frequency:";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(34, 131);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(59, 13);
      this.label6.TabIndex = 22;
      this.label6.Text = "Symbolrate";
      // 
      // textBoxSymbolRate
      // 
      this.textBoxSymbolRate.Location = new System.Drawing.Point(112, 128);
      this.textBoxSymbolRate.Name = "textBoxSymbolRate";
      this.textBoxSymbolRate.Size = new System.Drawing.Size(146, 20);
      this.textBoxSymbolRate.TabIndex = 23;
      this.textBoxSymbolRate.Text = "6875";
      // 
      // FormDVBCChannel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 210);
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
      this.Name = "FormDVBCChannel";
      this.Text = "DVBC Channel";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.TextBox textBoxSID;
    private System.Windows.Forms.TextBox textBoxTSID;
    private System.Windows.Forms.TextBox textBoxONID;
    private System.Windows.Forms.TextBox textboxFreq;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox textBoxSymbolRate;
  }
}