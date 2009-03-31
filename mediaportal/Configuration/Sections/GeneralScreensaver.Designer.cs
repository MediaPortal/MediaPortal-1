namespace MediaPortal.Configuration.Sections
{
  partial class GeneralScreensaver
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private new System.ComponentModel.IContainer components = null;

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
      this.checkBoxEnableScreensaver = new System.Windows.Forms.CheckBox();
      this.numericUpDownDelay = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.groupBoxScreenSaver = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBoxIdleAction = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioBtnFPSReduce = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioBtnBlankScreen = new MediaPortal.UserInterface.Controls.MPRadioButton();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).BeginInit();
      this.groupBoxScreenSaver.SuspendLayout();
      this.groupBoxIdleAction.SuspendLayout();
      this.SuspendLayout();
      // 
      // checkBoxEnableScreensaver
      // 
      this.checkBoxEnableScreensaver.AutoSize = true;
      this.checkBoxEnableScreensaver.Checked = true;
      this.checkBoxEnableScreensaver.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxEnableScreensaver.Location = new System.Drawing.Point(18, 22);
      this.checkBoxEnableScreensaver.Name = "checkBoxEnableScreensaver";
      this.checkBoxEnableScreensaver.Size = new System.Drawing.Size(109, 17);
      this.checkBoxEnableScreensaver.TabIndex = 9;
      this.checkBoxEnableScreensaver.Text = "Idle timer enabled";
      this.checkBoxEnableScreensaver.UseVisualStyleBackColor = true;
      this.checkBoxEnableScreensaver.CheckedChanged += new System.EventHandler(this.checkBoxEnableScreensaver_CheckedChanged);
      // 
      // numericUpDownDelay
      // 
      this.numericUpDownDelay.Location = new System.Drawing.Point(133, 21);
      this.numericUpDownDelay.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
      this.numericUpDownDelay.Name = "numericUpDownDelay";
      this.numericUpDownDelay.Size = new System.Drawing.Size(52, 20);
      this.numericUpDownDelay.TabIndex = 11;
      this.numericUpDownDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownDelay.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(191, 23);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(254, 13);
      this.label1.TabIndex = 12;
      this.label1.Text = "Delay in seconds after which MP will enter idle mode";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(15, 51);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(377, 34);
      this.label2.TabIndex = 13;
      this.label2.Text = "The idle time starts to count whenever there is no user activity and no moving pi" +
          "cture is shown in fullscreen. Therefore the only exceptions are:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(15, 85);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(161, 13);
      this.label3.TabIndex = 14;
      this.label3.Text = "- Playing TV / Video in fullscreen";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(15, 101);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(189, 13);
      this.label4.TabIndex = 15;
      this.label4.Text = "- Displaying a visualisation in fullscreen";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(15, 118);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(164, 13);
      this.label7.TabIndex = 16;
      this.label7.Text = "- Showing a slideshow of pictures";
      // 
      // groupBoxScreenSaver
      // 
      this.groupBoxScreenSaver.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxScreenSaver.Controls.Add(this.groupBoxIdleAction);
      this.groupBoxScreenSaver.Controls.Add(this.numericUpDownDelay);
      this.groupBoxScreenSaver.Controls.Add(this.label1);
      this.groupBoxScreenSaver.Controls.Add(this.label7);
      this.groupBoxScreenSaver.Controls.Add(this.label2);
      this.groupBoxScreenSaver.Controls.Add(this.checkBoxEnableScreensaver);
      this.groupBoxScreenSaver.Controls.Add(this.label4);
      this.groupBoxScreenSaver.Controls.Add(this.label3);
      this.groupBoxScreenSaver.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxScreenSaver.Location = new System.Drawing.Point(3, 3);
      this.groupBoxScreenSaver.Name = "groupBoxScreenSaver";
      this.groupBoxScreenSaver.Size = new System.Drawing.Size(461, 221);
      this.groupBoxScreenSaver.TabIndex = 17;
      this.groupBoxScreenSaver.TabStop = false;
      this.groupBoxScreenSaver.Text = "Idle timer";
      // 
      // groupBoxIdleAction
      // 
      this.groupBoxIdleAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxIdleAction.Controls.Add(this.radioBtnFPSReduce);
      this.groupBoxIdleAction.Controls.Add(this.radioBtnBlankScreen);
      this.groupBoxIdleAction.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxIdleAction.Location = new System.Drawing.Point(18, 152);
      this.groupBoxIdleAction.Name = "groupBoxIdleAction";
      this.groupBoxIdleAction.Size = new System.Drawing.Size(427, 53);
      this.groupBoxIdleAction.TabIndex = 17;
      this.groupBoxIdleAction.TabStop = false;
      this.groupBoxIdleAction.Text = "Idle action";
      // 
      // radioBtnFPSReduce
      // 
      this.radioBtnFPSReduce.AutoSize = true;
      this.radioBtnFPSReduce.Checked = true;
      this.radioBtnFPSReduce.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioBtnFPSReduce.Location = new System.Drawing.Point(207, 23);
      this.radioBtnFPSReduce.Name = "radioBtnFPSReduce";
      this.radioBtnFPSReduce.Size = new System.Drawing.Size(213, 17);
      this.radioBtnFPSReduce.TabIndex = 1;
      this.radioBtnFPSReduce.TabStop = true;
      this.radioBtnFPSReduce.Text = "Reduce Framerate (save CPU / Energy)";
      this.radioBtnFPSReduce.UseVisualStyleBackColor = true;
      // 
      // radioBtnBlankScreen
      // 
      this.radioBtnBlankScreen.AutoSize = true;
      this.radioBtnBlankScreen.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioBtnBlankScreen.Location = new System.Drawing.Point(15, 23);
      this.radioBtnBlankScreen.Name = "radioBtnBlankScreen";
      this.radioBtnBlankScreen.Size = new System.Drawing.Size(186, 17);
      this.radioBtnBlankScreen.TabIndex = 0;
      this.radioBtnBlankScreen.Text = "Blank the screen (prevent Burn-In)";
      this.radioBtnBlankScreen.UseVisualStyleBackColor = true;
      // 
      // GeneralScreensaver
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBoxScreenSaver);
      this.Name = "GeneralScreensaver";
      this.Size = new System.Drawing.Size(468, 226);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).EndInit();
      this.groupBoxScreenSaver.ResumeLayout(false);
      this.groupBoxScreenSaver.PerformLayout();
      this.groupBoxIdleAction.ResumeLayout(false);
      this.groupBoxIdleAction.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.CheckBox checkBoxEnableScreensaver;
    private System.Windows.Forms.NumericUpDown numericUpDownDelay;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label7;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxScreenSaver;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxIdleAction;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioBtnBlankScreen;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioBtnFPSReduce;
  }
}
