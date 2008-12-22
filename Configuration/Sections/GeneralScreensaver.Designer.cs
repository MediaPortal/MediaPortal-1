namespace MediaPortal.Configuration.Sections
{
  partial class GeneralScreensaver
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.label6 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.checkBoxEnableScreensaver = new System.Windows.Forms.CheckBox();
      this.numericUpDownDelay = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).BeginInit();
      this.SuspendLayout();
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(14, 32);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(156, 13);
      this.label6.TabIndex = 8;
      this.label6.Text = "to prevent burn marks in your tv";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(14, 10);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(360, 13);
      this.label5.TabIndex = 7;
      this.label5.Text = "The screensaver blanks the screen after the configured amount of idle time";
      // 
      // checkBoxEnableScreensaver
      // 
      this.checkBoxEnableScreensaver.AutoSize = true;
      this.checkBoxEnableScreensaver.Location = new System.Drawing.Point(17, 166);
      this.checkBoxEnableScreensaver.Name = "checkBoxEnableScreensaver";
      this.checkBoxEnableScreensaver.Size = new System.Drawing.Size(127, 17);
      this.checkBoxEnableScreensaver.TabIndex = 9;
      this.checkBoxEnableScreensaver.Text = "Screensaver enabled";
      this.checkBoxEnableScreensaver.UseVisualStyleBackColor = true;
      // 
      // numericUpDownDelay
      // 
      this.numericUpDownDelay.Location = new System.Drawing.Point(33, 190);
      this.numericUpDownDelay.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
      this.numericUpDownDelay.Name = "numericUpDownDelay";
      this.numericUpDownDelay.Size = new System.Drawing.Size(43, 20);
      this.numericUpDownDelay.TabIndex = 11;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(82, 194);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(309, 13);
      this.label1.TabIndex = 12;
      this.label1.Text = "Delay in seconds after which the screensaver blanks the screen";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(14, 63);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(377, 34);
      this.label2.TabIndex = 13;
      this.label2.Text = "The idle time starts to count whenever there is no user activity and no moving pi" +
          "cture is shown in fullscreen. So the only exceptions are:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(14, 97);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(179, 13);
      this.label3.TabIndex = 14;
      this.label3.Text = "- Playing a video (or TV) in fullscreen";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(14, 113);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(189, 13);
      this.label4.TabIndex = 15;
      this.label4.Text = "- Displaying a visualisation in fullscreen";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(14, 130);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(164, 13);
      this.label7.TabIndex = 16;
      this.label7.Text = "- Showing a slideshow of pictures";
      // 
      // GeneralScreensaver
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.label7);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.numericUpDownDelay);
      this.Controls.Add(this.checkBoxEnableScreensaver);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label5);
      this.Name = "GeneralScreensaver";
      this.Size = new System.Drawing.Size(543, 279);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.CheckBox checkBoxEnableScreensaver;
    private System.Windows.Forms.NumericUpDown numericUpDownDelay;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label7;
  }
}
