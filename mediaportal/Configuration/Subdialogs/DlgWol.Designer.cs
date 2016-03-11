namespace MediaPortal.Configuration
{
  partial class DlgWol
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
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpUpDownWolResend = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpUpDownWolTimeout = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButtonOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpUpDownWaitTime = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      ((System.ComponentModel.ISupportInitialize)(this.mpUpDownWolResend)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.mpUpDownWolTimeout)).BeginInit();
      this.mpGroupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.mpUpDownWaitTime)).BeginInit();
      this.SuspendLayout();
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(25, 98);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(145, 13);
      this.mpLabel2.TabIndex = 14;
      this.mpLabel2.Text = "WOL resend time (in minutes)";
      // 
      // mpUpDownWolResend
      // 
      this.mpUpDownWolResend.Location = new System.Drawing.Point(202, 95);
      this.mpUpDownWolResend.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.mpUpDownWolResend.Name = "mpUpDownWolResend";
      this.mpUpDownWolResend.Size = new System.Drawing.Size(39, 20);
      this.mpUpDownWolResend.TabIndex = 13;
      this.mpUpDownWolResend.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.mpUpDownWolResend.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(25, 71);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(129, 13);
      this.mpLabel1.TabIndex = 12;
      this.mpLabel1.Text = "WOL timeout (in seconds)";
      // 
      // mpUpDownWolTimeout
      // 
      this.mpUpDownWolTimeout.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.mpUpDownWolTimeout.Location = new System.Drawing.Point(182, 58);
      this.mpUpDownWolTimeout.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
      this.mpUpDownWolTimeout.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.mpUpDownWolTimeout.Name = "mpUpDownWolTimeout";
      this.mpUpDownWolTimeout.Size = new System.Drawing.Size(40, 20);
      this.mpUpDownWolTimeout.TabIndex = 11;
      this.mpUpDownWolTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.mpUpDownWolTimeout.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(25, 23);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(162, 13);
      this.mpLabel3.TabIndex = 15;
      this.mpLabel3.Text = "Global Wake On Lan parameters";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpUpDownWaitTime);
      this.mpGroupBox1.Controls.Add(this.mpLabel4);
      this.mpGroupBox1.Controls.Add(this.mpUpDownWolTimeout);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(19, 8);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(253, 159);
      this.mpGroupBox1.TabIndex = 16;
      this.mpGroupBox1.TabStop = false;
      // 
      // mpButtonOK
      // 
      this.mpButtonOK.Location = new System.Drawing.Point(110, 186);
      this.mpButtonOK.Name = "mpButtonOK";
      this.mpButtonOK.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOK.TabIndex = 17;
      this.mpButtonOK.Text = "OK";
      this.mpButtonOK.UseVisualStyleBackColor = true;
      this.mpButtonOK.Click += new System.EventHandler(this.mpButtonOK_Click);
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(6, 116);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(163, 13);
      this.mpLabel4.TabIndex = 15;
      this.mpLabel4.Text = "Wait time after WOL (in seconds)";
      // 
      // mpUpDownWaitTime
      // 
      this.mpUpDownWaitTime.Location = new System.Drawing.Point(183, 113);
      this.mpUpDownWaitTime.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
      this.mpUpDownWaitTime.Name = "mpUpDownWaitTime";
      this.mpUpDownWaitTime.Size = new System.Drawing.Size(39, 20);
      this.mpUpDownWaitTime.TabIndex = 16;
      this.mpUpDownWaitTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // DlgWol
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 223);
      this.Controls.Add(this.mpButtonOK);
      this.Controls.Add(this.mpLabel3);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.mpUpDownWolResend);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "DlgWol";
      this.Text = "Edit WOL parameters";
      ((System.ComponentModel.ISupportInitialize)(this.mpUpDownWolResend)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.mpUpDownWolTimeout)).EndInit();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.mpUpDownWaitTime)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private UserInterface.Controls.MPLabel mpLabel2;
    private UserInterface.Controls.MPNumericUpDown mpUpDownWolResend;
    private UserInterface.Controls.MPLabel mpLabel1;
    private UserInterface.Controls.MPNumericUpDown mpUpDownWolTimeout;
    private UserInterface.Controls.MPLabel mpLabel3;
    private UserInterface.Controls.MPGroupBox mpGroupBox1;
    private UserInterface.Controls.MPButton mpButtonOK;
    private UserInterface.Controls.MPNumericUpDown mpUpDownWaitTime;
    private UserInterface.Controls.MPLabel mpLabel4;
  }
}