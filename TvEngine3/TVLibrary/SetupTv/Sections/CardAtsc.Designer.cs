namespace SetupTv.Sections
{
  partial class CardAtsc
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
      this.labelScan2 = new System.Windows.Forms.Label();
      this.labelScan1 = new System.Windows.Forms.Label();
      this.progressBarQuality = new System.Windows.Forms.ProgressBar();
      this.progressBarLevel = new System.Windows.Forms.ProgressBar();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelTunerLocked = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label12 = new System.Windows.Forms.Label();
      this.mpButtonScanTv = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // labelScan2
      // 
      this.labelScan2.AutoSize = true;
      this.labelScan2.Location = new System.Drawing.Point(21, 225);
      this.labelScan2.Name = "labelScan2";
      this.labelScan2.Size = new System.Drawing.Size(73, 13);
      this.labelScan2.TabIndex = 61;
      this.labelScan2.Text = "Tuner locked:";
      // 
      // labelScan1
      // 
      this.labelScan1.AutoSize = true;
      this.labelScan1.Location = new System.Drawing.Point(21, 201);
      this.labelScan1.Name = "labelScan1";
      this.labelScan1.Size = new System.Drawing.Size(73, 13);
      this.labelScan1.TabIndex = 60;
      this.labelScan1.Text = "Tuner locked:";
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Location = new System.Drawing.Point(111, 127);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(328, 10);
      this.progressBarQuality.TabIndex = 59;
      // 
      // progressBarLevel
      // 
      this.progressBarLevel.Location = new System.Drawing.Point(111, 104);
      this.progressBarLevel.Name = "progressBarLevel";
      this.progressBarLevel.Size = new System.Drawing.Size(328, 10);
      this.progressBarLevel.TabIndex = 58;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(21, 124);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(74, 13);
      this.label2.TabIndex = 57;
      this.label2.Text = "Signal Quality:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(21, 101);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 56;
      this.label1.Text = "Signal level:";
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(24, 177);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(328, 10);
      this.progressBar1.TabIndex = 55;
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(163, 77);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(86, 13);
      this.mpLabel3.TabIndex = 53;
      this.mpLabel3.Text = "Current Channel:";
      // 
      // mpLabelTunerLocked
      // 
      this.mpLabelTunerLocked.AutoSize = true;
      this.mpLabelTunerLocked.Location = new System.Drawing.Point(109, 77);
      this.mpLabelTunerLocked.Name = "mpLabelTunerLocked";
      this.mpLabelTunerLocked.Size = new System.Drawing.Size(19, 13);
      this.mpLabelTunerLocked.TabIndex = 52;
      this.mpLabelTunerLocked.Text = "no";
      // 
      // mpLabelChannel
      // 
      this.mpLabelChannel.AutoSize = true;
      this.mpLabelChannel.Location = new System.Drawing.Point(255, 77);
      this.mpLabelChannel.Name = "mpLabelChannel";
      this.mpLabelChannel.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannel.TabIndex = 54;
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(21, 77);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(73, 13);
      this.label12.TabIndex = 51;
      this.label12.Text = "Tuner locked:";
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Location = new System.Drawing.Point(319, 335);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(131, 23);
      this.mpButtonScanTv.TabIndex = 50;
      this.mpButtonScanTv.Text = "Scan for Tv Channels";
      this.mpButtonScanTv.UseVisualStyleBackColor = true;
      this.mpButtonScanTv.Click += new System.EventHandler(this.mpButtonScanTv_Click);
      // 
      // CardAtsc
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.labelScan2);
      this.Controls.Add(this.labelScan1);
      this.Controls.Add(this.progressBarQuality);
      this.Controls.Add(this.progressBarLevel);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.mpLabel3);
      this.Controls.Add(this.mpLabelTunerLocked);
      this.Controls.Add(this.mpLabelChannel);
      this.Controls.Add(this.label12);
      this.Controls.Add(this.mpButtonScanTv);
      this.Name = "CardAtsc";
      this.Size = new System.Drawing.Size(467, 388);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelScan2;
    private System.Windows.Forms.Label labelScan1;
    private System.Windows.Forms.ProgressBar progressBarQuality;
    private System.Windows.Forms.ProgressBar progressBarLevel;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelTunerLocked;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannel;
    private System.Windows.Forms.Label label12;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanTv;
  }
}