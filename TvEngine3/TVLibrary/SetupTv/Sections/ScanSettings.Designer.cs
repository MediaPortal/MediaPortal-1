namespace SetupTv.Sections
{
  partial class ScanSettings
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
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPageApplication = new System.Windows.Forms.TabPage();
      this.groupBox8 = new System.Windows.Forms.GroupBox();
      this.lblPriority = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxPrio = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox6 = new System.Windows.Forms.GroupBox();
      this.label45 = new System.Windows.Forms.Label();
      this.label44 = new System.Windows.Forms.Label();
      this.delayDetectUpDown = new System.Windows.Forms.NumericUpDown();
      this.tabPageScan = new System.Windows.Forms.TabPage();
      this.numericUpDownAnalog = new System.Windows.Forms.NumericUpDown();
      this.label21 = new System.Windows.Forms.Label();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.checkBoxEnableLinkageScanner = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label24 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.numericUpDownSDT = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownPMT = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownCAT = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownPAT = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownTune = new System.Windows.Forms.NumericUpDown();
      this.label13 = new System.Windows.Forms.Label();
      this.label12 = new System.Windows.Forms.Label();
      this.label11 = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.tabControl1.SuspendLayout();
      this.tabPageApplication.SuspendLayout();
      this.groupBox8.SuspendLayout();
      this.groupBox6.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.delayDetectUpDown)).BeginInit();
      this.tabPageScan.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAnalog)).BeginInit();
      this.groupBox4.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSDT)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPMT)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCAT)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPAT)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTune)).BeginInit();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPageApplication);
      this.tabControl1.Controls.Add(this.tabPageScan);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(484, 454);
      this.tabControl1.TabIndex = 15;
      // 
      // tabPageApplication
      // 
      this.tabPageApplication.Controls.Add(this.groupBox8);
      this.tabPageApplication.Controls.Add(this.groupBox6);
      this.tabPageApplication.Location = new System.Drawing.Point(4, 22);
      this.tabPageApplication.Name = "tabPageApplication";
      this.tabPageApplication.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageApplication.Size = new System.Drawing.Size(476, 428);
      this.tabPageApplication.TabIndex = 1;
      this.tabPageApplication.Text = "Application";
      this.tabPageApplication.UseVisualStyleBackColor = true;
      // 
      // groupBox8
      // 
      this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox8.Controls.Add(this.lblPriority);
      this.groupBox8.Controls.Add(this.mpComboBoxPrio);
      this.groupBox8.Location = new System.Drawing.Point(6, 6);
      this.groupBox8.Name = "groupBox8";
      this.groupBox8.Size = new System.Drawing.Size(464, 50);
      this.groupBox8.TabIndex = 81;
      this.groupBox8.TabStop = false;
      this.groupBox8.Text = "TVService";
      // 
      // lblPriority
      // 
      this.lblPriority.AutoSize = true;
      this.lblPriority.Location = new System.Drawing.Point(6, 22);
      this.lblPriority.Name = "lblPriority";
      this.lblPriority.Size = new System.Drawing.Size(93, 13);
      this.lblPriority.TabIndex = 76;
      this.lblPriority.Text = "TVService priority:";
      // 
      // mpComboBoxPrio
      // 
      this.mpComboBoxPrio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxPrio.FormattingEnabled = true;
      this.mpComboBoxPrio.Items.AddRange(new object[] {
            "Not Defined ",
            "16 QAM ",
            "32 QAM",
            "64 QAM",
            "80 QAM",
            "96 QAM",
            "112 QAM",
            "128 QAM",
            "160 QAM",
            "192 QAM",
            "224 QAM",
            "256 QAM",
            "320 QAM",
            "384 QAM",
            "448 QAM",
            "512 QAM",
            "640 QAM",
            "768 QAM",
            "896 QAM",
            "1024 QAM",
            "Qpsk",
            "Bpsk",
            "Oqpsk ",
            "8Vsb ",
            "16Vsb ",
            "AnalogAmplitude ",
            "AnalogFrequency ",
            "8psk ",
            "Rf ",
            "16Apsk ",
            "32Apsk",
            "Qpsk2 ",
            "8psk2 ",
            "DirectTV  "});
      this.mpComboBoxPrio.Location = new System.Drawing.Point(125, 19);
      this.mpComboBoxPrio.Name = "mpComboBoxPrio";
      this.mpComboBoxPrio.Size = new System.Drawing.Size(179, 21);
      this.mpComboBoxPrio.TabIndex = 77;
      this.mpComboBoxPrio.SelectedIndexChanged += new System.EventHandler(this.mpComboBoxPrio_SelectedIndexChanged);
      // 
      // groupBox6
      // 
      this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox6.Controls.Add(this.label45);
      this.groupBox6.Controls.Add(this.label44);
      this.groupBox6.Controls.Add(this.delayDetectUpDown);
      this.groupBox6.Location = new System.Drawing.Point(6, 62);
      this.groupBox6.Name = "groupBox6";
      this.groupBox6.Size = new System.Drawing.Size(464, 107);
      this.groupBox6.TabIndex = 80;
      this.groupBox6.TabStop = false;
      this.groupBox6.Text = "Delay for TV card detection";
      // 
      // label45
      // 
      this.label45.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label45.Location = new System.Drawing.Point(6, 27);
      this.label45.Name = "label45";
      this.label45.Size = new System.Drawing.Size(452, 35);
      this.label45.TabIndex = 80;
      this.label45.Text = "Some cards (i.e. Hauppauge Nova-T 500) take a long time to initialize after stand" +
          "by. Therefore use this option below to force a delay should it be required.";
      // 
      // label44
      // 
      this.label44.AutoSize = true;
      this.label44.Location = new System.Drawing.Point(6, 67);
      this.label44.Name = "label44";
      this.label44.Size = new System.Drawing.Size(236, 13);
      this.label44.TabIndex = 78;
      this.label44.Text = "Delay in seconds before TVServer detects cards";
      // 
      // delayDetectUpDown
      // 
      this.delayDetectUpDown.Location = new System.Drawing.Point(257, 65);
      this.delayDetectUpDown.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.delayDetectUpDown.Name = "delayDetectUpDown";
      this.delayDetectUpDown.Size = new System.Drawing.Size(47, 20);
      this.delayDetectUpDown.TabIndex = 79;
      // 
      // tabPageScan
      // 
      this.tabPageScan.Controls.Add(this.numericUpDownAnalog);
      this.tabPageScan.Controls.Add(this.label21);
      this.tabPageScan.Controls.Add(this.groupBox4);
      this.tabPageScan.Controls.Add(this.label24);
      this.tabPageScan.Controls.Add(this.groupBox1);
      this.tabPageScan.Location = new System.Drawing.Point(4, 22);
      this.tabPageScan.Name = "tabPageScan";
      this.tabPageScan.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageScan.Size = new System.Drawing.Size(476, 428);
      this.tabPageScan.TabIndex = 0;
      this.tabPageScan.Text = "Scan";
      this.tabPageScan.UseVisualStyleBackColor = true;
      // 
      // numericUpDownAnalog
      // 
      this.numericUpDownAnalog.Location = new System.Drawing.Point(95, 155);
      this.numericUpDownAnalog.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownAnalog.Name = "numericUpDownAnalog";
      this.numericUpDownAnalog.Size = new System.Drawing.Size(88, 20);
      this.numericUpDownAnalog.TabIndex = 22;
      this.numericUpDownAnalog.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownAnalog.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
      // 
      // label21
      // 
      this.label21.AutoSize = true;
      this.label21.Location = new System.Drawing.Point(189, 157);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(29, 13);
      this.label21.TabIndex = 21;
      this.label21.Text = "secs";
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.checkBoxEnableLinkageScanner);
      this.groupBox4.Location = new System.Drawing.Point(6, 195);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(221, 46);
      this.groupBox4.TabIndex = 18;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Linkage Scanner";
      // 
      // checkBoxEnableLinkageScanner
      // 
      this.checkBoxEnableLinkageScanner.AutoSize = true;
      this.checkBoxEnableLinkageScanner.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.checkBoxEnableLinkageScanner.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEnableLinkageScanner.Location = new System.Drawing.Point(17, 19);
      this.checkBoxEnableLinkageScanner.Name = "checkBoxEnableLinkageScanner";
      this.checkBoxEnableLinkageScanner.Size = new System.Drawing.Size(63, 17);
      this.checkBoxEnableLinkageScanner.TabIndex = 17;
      this.checkBoxEnableLinkageScanner.Text = "Enabled";
      this.checkBoxEnableLinkageScanner.UseVisualStyleBackColor = true;
      // 
      // label24
      // 
      this.label24.AutoSize = true;
      this.label24.Location = new System.Drawing.Point(12, 157);
      this.label24.Name = "label24";
      this.label24.Size = new System.Drawing.Size(43, 13);
      this.label24.TabIndex = 20;
      this.label24.Text = "Analog:";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.numericUpDownSDT);
      this.groupBox1.Controls.Add(this.numericUpDownPMT);
      this.groupBox1.Controls.Add(this.numericUpDownCAT);
      this.groupBox1.Controls.Add(this.numericUpDownPAT);
      this.groupBox1.Controls.Add(this.numericUpDownTune);
      this.groupBox1.Controls.Add(this.label13);
      this.groupBox1.Controls.Add(this.label12);
      this.groupBox1.Controls.Add(this.label11);
      this.groupBox1.Controls.Add(this.label10);
      this.groupBox1.Controls.Add(this.label9);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Location = new System.Drawing.Point(6, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(221, 183);
      this.groupBox1.TabIndex = 15;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Scanning";
      // 
      // numericUpDownSDT
      // 
      this.numericUpDownSDT.Location = new System.Drawing.Point(89, 123);
      this.numericUpDownSDT.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownSDT.Name = "numericUpDownSDT";
      this.numericUpDownSDT.Size = new System.Drawing.Size(88, 20);
      this.numericUpDownSDT.TabIndex = 19;
      this.numericUpDownSDT.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownSDT.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
      // 
      // numericUpDownPMT
      // 
      this.numericUpDownPMT.Location = new System.Drawing.Point(89, 97);
      this.numericUpDownPMT.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownPMT.Name = "numericUpDownPMT";
      this.numericUpDownPMT.Size = new System.Drawing.Size(88, 20);
      this.numericUpDownPMT.TabIndex = 18;
      this.numericUpDownPMT.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownPMT.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // numericUpDownCAT
      // 
      this.numericUpDownCAT.Location = new System.Drawing.Point(89, 71);
      this.numericUpDownCAT.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownCAT.Name = "numericUpDownCAT";
      this.numericUpDownCAT.Size = new System.Drawing.Size(88, 20);
      this.numericUpDownCAT.TabIndex = 17;
      this.numericUpDownCAT.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownCAT.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // numericUpDownPAT
      // 
      this.numericUpDownPAT.Location = new System.Drawing.Point(89, 45);
      this.numericUpDownPAT.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownPAT.Name = "numericUpDownPAT";
      this.numericUpDownPAT.Size = new System.Drawing.Size(88, 20);
      this.numericUpDownPAT.TabIndex = 16;
      this.numericUpDownPAT.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownPAT.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // numericUpDownTune
      // 
      this.numericUpDownTune.Location = new System.Drawing.Point(89, 19);
      this.numericUpDownTune.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownTune.Name = "numericUpDownTune";
      this.numericUpDownTune.Size = new System.Drawing.Size(88, 20);
      this.numericUpDownTune.TabIndex = 15;
      this.numericUpDownTune.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTune.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(183, 125);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(29, 13);
      this.label13.TabIndex = 14;
      this.label13.Text = "secs";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(183, 99);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(29, 13);
      this.label12.TabIndex = 13;
      this.label12.Text = "secs";
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(183, 73);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(29, 13);
      this.label11.TabIndex = 12;
      this.label11.Text = "secs";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(183, 47);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(29, 13);
      this.label10.TabIndex = 11;
      this.label10.Text = "secs";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(183, 21);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(29, 13);
      this.label9.TabIndex = 5;
      this.label9.Text = "secs";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 21);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(32, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Tune";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(6, 47);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(31, 13);
      this.label3.TabIndex = 3;
      this.label3.Text = "PAT:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(6, 73);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(31, 13);
      this.label4.TabIndex = 4;
      this.label4.Text = "CAT:";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(6, 99);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(33, 13);
      this.label5.TabIndex = 5;
      this.label5.Text = "PMT:";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(6, 125);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(58, 13);
      this.label6.TabIndex = 6;
      this.label6.Text = "SDT/VCT:";
      // 
      // ScanSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "ScanSettings";
      this.Size = new System.Drawing.Size(484, 454);
      this.tabControl1.ResumeLayout(false);
      this.tabPageApplication.ResumeLayout(false);
      this.groupBox8.ResumeLayout(false);
      this.groupBox8.PerformLayout();
      this.groupBox6.ResumeLayout(false);
      this.groupBox6.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.delayDetectUpDown)).EndInit();
      this.tabPageScan.ResumeLayout(false);
      this.tabPageScan.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAnalog)).EndInit();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSDT)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPMT)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCAT)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPAT)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTune)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPageScan;
    private System.Windows.Forms.GroupBox groupBox4;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableLinkageScanner;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TabPage tabPageApplication;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxPrio;
    private MediaPortal.UserInterface.Controls.MPLabel lblPriority;
    private System.Windows.Forms.NumericUpDown delayDetectUpDown;
    private System.Windows.Forms.Label label44;
    private System.Windows.Forms.GroupBox groupBox6;
    private System.Windows.Forms.Label label45;
    private System.Windows.Forms.NumericUpDown numericUpDownSDT;
    private System.Windows.Forms.NumericUpDown numericUpDownPMT;
    private System.Windows.Forms.NumericUpDown numericUpDownCAT;
    private System.Windows.Forms.NumericUpDown numericUpDownPAT;
    private System.Windows.Forms.NumericUpDown numericUpDownTune;
    private System.Windows.Forms.GroupBox groupBox8;
    private System.Windows.Forms.NumericUpDown numericUpDownAnalog;
    private System.Windows.Forms.Label label21;
    private System.Windows.Forms.Label label24;

  }
}