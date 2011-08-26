namespace TestApp
{
  partial class FormATSCChannel
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
      this.textBoxTSID = new System.Windows.Forms.TextBox();
      this.textBoxONID = new System.Windows.Forms.TextBox();
      this.textboxFreq = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(168, 160);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(75, 23);
      this.buttonOK.TabIndex = 21;
      this.buttonOK.Text = "Ok";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // textBoxTSID
      // 
      this.textBoxTSID.Location = new System.Drawing.Point(97, 64);
      this.textBoxTSID.Name = "textBoxTSID";
      this.textBoxTSID.Size = new System.Drawing.Size(146, 20);
      this.textBoxTSID.TabIndex = 18;
      this.textBoxTSID.Text = "-1";
      // 
      // textBoxONID
      // 
      this.textBoxONID.Location = new System.Drawing.Point(97, 38);
      this.textBoxONID.Name = "textBoxONID";
      this.textBoxONID.Size = new System.Drawing.Size(146, 20);
      this.textBoxONID.TabIndex = 17;
      this.textBoxONID.Text = "-1";
      // 
      // textboxFreq
      // 
      this.textboxFreq.Location = new System.Drawing.Point(97, 12);
      this.textboxFreq.Name = "textboxFreq";
      this.textboxFreq.Size = new System.Drawing.Size(146, 20);
      this.textboxFreq.TabIndex = 16;
      this.textboxFreq.Text = "34";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(19, 67);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(36, 13);
      this.label4.TabIndex = 14;
      this.label4.Text = "Minor:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(19, 41);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(36, 13);
      this.label2.TabIndex = 12;
      this.label2.Text = "Major:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(19, 12);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(49, 13);
      this.label1.TabIndex = 11;
      this.label1.Text = "Channel:";
      // 
      // FormATSCChannel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 209);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.textBoxTSID);
      this.Controls.Add(this.textBoxONID);
      this.Controls.Add(this.textboxFreq);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "FormATSCChannel";
      this.Text = "ATSC Channel";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.TextBox textBoxTSID;
    private System.Windows.Forms.TextBox textBoxONID;
    private System.Windows.Forms.TextBox textboxFreq;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
  }
}