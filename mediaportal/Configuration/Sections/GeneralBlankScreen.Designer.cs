namespace MediaPortal.Configuration.Sections
{
  partial class GeneralBlankScreen
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.chbEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.nudIdleTime = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudIdleTime)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.nudIdleTime);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.chbEnabled);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(3, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 98);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(18, 62);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(70, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Idle Time [s] :";
      // 
      // chbEnabled
      // 
      this.chbEnabled.AutoSize = true;
      this.chbEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chbEnabled.Location = new System.Drawing.Point(19, 32);
      this.chbEnabled.Name = "chbEnabled";
      this.chbEnabled.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.chbEnabled.Size = new System.Drawing.Size(291, 17);
      this.chbEnabled.TabIndex = 0;
      this.chbEnabled.Text = "Blank screen in fullscreen mode when MediaPortal is idle";
      this.chbEnabled.UseVisualStyleBackColor = true;
      // 
      // nudIdleTime
      // 
      this.nudIdleTime.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.nudIdleTime.Location = new System.Drawing.Point(94, 60);
      this.nudIdleTime.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.nudIdleTime.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.nudIdleTime.Name = "nudIdleTime";
      this.nudIdleTime.Size = new System.Drawing.Size(77, 20);
      this.nudIdleTime.TabIndex = 3;
      this.nudIdleTime.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // GeneralBlankScreen
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox1);
      this.Name = "GeneralBlankScreen";
      this.Size = new System.Drawing.Size(446, 327);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudIdleTime)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPCheckBox chbEnabled;
    private MediaPortal.UserInterface.Controls.MPNumericUpDown nudIdleTime;
  }
}
