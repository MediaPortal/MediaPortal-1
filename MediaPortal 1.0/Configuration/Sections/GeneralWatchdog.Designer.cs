namespace MediaPortal.Configuration.Sections
{
  partial class GeneralWatchdog
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
      this.checkBoxEnableWatchdog = new System.Windows.Forms.CheckBox();
      this.checkBoxAutoRestart = new System.Windows.Forms.CheckBox();
      this.numericUpDownDelay = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).BeginInit();
      this.SuspendLayout();
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(14, 32);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(305, 13);
      this.label6.TabIndex = 8;
      this.label6.Text = "make a zip on your desktop and restart MediaPortal if it crashes";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(14, 10);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(332, 13);
      this.label5.TabIndex = 7;
      this.label5.Text = "The watchdog monitors MP and can automatically gather the logfiles,";
      // 
      // checkBoxEnableWatchdog
      // 
      this.checkBoxEnableWatchdog.AutoSize = true;
      this.checkBoxEnableWatchdog.Location = new System.Drawing.Point(17, 74);
      this.checkBoxEnableWatchdog.Name = "checkBoxEnableWatchdog";
      this.checkBoxEnableWatchdog.Size = new System.Drawing.Size(287, 17);
      this.checkBoxEnableWatchdog.TabIndex = 9;
      this.checkBoxEnableWatchdog.Text = "Watchdog enabled (MPTestTool won\'t be started at all)";
      this.checkBoxEnableWatchdog.UseVisualStyleBackColor = true;
      // 
      // checkBoxAutoRestart
      // 
      this.checkBoxAutoRestart.AutoSize = true;
      this.checkBoxAutoRestart.Location = new System.Drawing.Point(17, 97);
      this.checkBoxAutoRestart.Name = "checkBoxAutoRestart";
      this.checkBoxAutoRestart.Size = new System.Drawing.Size(141, 17);
      this.checkBoxAutoRestart.TabIndex = 10;
      this.checkBoxAutoRestart.Text = "automatically restart MP ";
      this.checkBoxAutoRestart.UseVisualStyleBackColor = true;
      // 
      // numericUpDownDelay
      // 
      this.numericUpDownDelay.Location = new System.Drawing.Point(33, 121);
      this.numericUpDownDelay.Name = "numericUpDownDelay";
      this.numericUpDownDelay.Size = new System.Drawing.Size(41, 20);
      this.numericUpDownDelay.TabIndex = 11;
      this.numericUpDownDelay.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(82, 125);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(216, 13);
      this.label1.TabIndex = 12;
      this.label1.Text = "Delay in seconds after which MP is restarted";
      // 
      // GeneralWatchdog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.label1);
      this.Controls.Add(this.numericUpDownDelay);
      this.Controls.Add(this.checkBoxAutoRestart);
      this.Controls.Add(this.checkBoxEnableWatchdog);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label5);
      this.Name = "GeneralWatchdog";
      this.Size = new System.Drawing.Size(385, 279);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.CheckBox checkBoxEnableWatchdog;
    private System.Windows.Forms.CheckBox checkBoxAutoRestart;
    private System.Windows.Forms.NumericUpDown numericUpDownDelay;
    private System.Windows.Forms.Label label1;
  }
}
