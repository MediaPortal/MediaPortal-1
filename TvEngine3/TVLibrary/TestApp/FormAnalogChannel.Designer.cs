namespace TestApp
{
  partial class FormAnalogChannel
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
      this.textBoxChannel = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.comboBoxInput = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.comboBoxCountry = new System.Windows.Forms.ComboBox();
      this.buttonOk = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(49, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Channel:";
      // 
      // textBoxChannel
      // 
      this.textBoxChannel.Location = new System.Drawing.Point(90, 21);
      this.textBoxChannel.Name = "textBoxChannel";
      this.textBoxChannel.Size = new System.Drawing.Size(100, 20);
      this.textBoxChannel.TabIndex = 1;
      this.textBoxChannel.Text = "4";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 57);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(57, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Input type:";
      // 
      // comboBoxInput
      // 
      this.comboBoxInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxInput.FormattingEnabled = true;
      this.comboBoxInput.Items.AddRange(new object[] {
            "Antenna",
            "Cable"});
      this.comboBoxInput.Location = new System.Drawing.Point(90, 54);
      this.comboBoxInput.Name = "comboBoxInput";
      this.comboBoxInput.Size = new System.Drawing.Size(258, 21);
      this.comboBoxInput.TabIndex = 3;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(12, 97);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(46, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Country:";
      // 
      // comboBoxCountry
      // 
      this.comboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCountry.FormattingEnabled = true;
      this.comboBoxCountry.Location = new System.Drawing.Point(90, 94);
      this.comboBoxCountry.Name = "comboBoxCountry";
      this.comboBoxCountry.Size = new System.Drawing.Size(258, 21);
      this.comboBoxCountry.TabIndex = 5;
      // 
      // buttonOk
      // 
      this.buttonOk.Location = new System.Drawing.Point(273, 130);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(75, 23);
      this.buttonOk.TabIndex = 6;
      this.buttonOk.Text = "Ok";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // FormAnalogChannel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(360, 184);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.comboBoxCountry);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.comboBoxInput);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.textBoxChannel);
      this.Controls.Add(this.label1);
      this.Name = "FormAnalogChannel";
      this.Text = "Analog Tv Channel:";
      this.Load += new System.EventHandler(this.FormAnalogChannel_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBoxChannel;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox comboBoxInput;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox comboBoxCountry;
    private System.Windows.Forms.Button buttonOk;
  }
}