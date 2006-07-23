namespace TestApp
{
  partial class Form1
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
      this.components = new System.ComponentModel.Container();
      this.comboBoxCards = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.buttonScan = new System.Windows.Forms.Button();
      this.buttonTune = new System.Windows.Forms.Button();
      this.btnEPG = new System.Windows.Forms.Button();
      this.buttonTimeShift = new System.Windows.Forms.Button();
      this.buttonRecord = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.labelTunerLock = new System.Windows.Forms.Label();
      this.progressBarLevel = new System.Windows.Forms.ProgressBar();
      this.progressBarQuality = new System.Windows.Forms.ProgressBar();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.listViewChannels = new System.Windows.Forms.ListView();
      this.label5 = new System.Windows.Forms.Label();
      this.labelChannel = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.button1 = new System.Windows.Forms.Button();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.label7 = new System.Windows.Forms.Label();
      this.labelScrambled = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // comboBoxCards
      // 
      this.comboBoxCards.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCards.FormattingEnabled = true;
      this.comboBoxCards.Location = new System.Drawing.Point(73, 12);
      this.comboBoxCards.Name = "comboBoxCards";
      this.comboBoxCards.Size = new System.Drawing.Size(310, 21);
      this.comboBoxCards.TabIndex = 0;
      this.comboBoxCards.SelectedIndexChanged += new System.EventHandler(this.comboBoxCards_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(20, 15);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(47, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Tv card:";
      // 
      // buttonScan
      // 
      this.buttonScan.Location = new System.Drawing.Point(611, 12);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(158, 23);
      this.buttonScan.TabIndex = 2;
      this.buttonScan.Text = "Scan current transponder";
      this.buttonScan.UseVisualStyleBackColor = true;
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // buttonTune
      // 
      this.buttonTune.Location = new System.Drawing.Point(611, 41);
      this.buttonTune.Name = "buttonTune";
      this.buttonTune.Size = new System.Drawing.Size(158, 23);
      this.buttonTune.TabIndex = 3;
      this.buttonTune.Text = "Tune";
      this.buttonTune.UseVisualStyleBackColor = true;
      this.buttonTune.Click += new System.EventHandler(this.buttonTune_Click);
      // 
      // btnEPG
      // 
      this.btnEPG.Location = new System.Drawing.Point(611, 70);
      this.btnEPG.Name = "btnEPG";
      this.btnEPG.Size = new System.Drawing.Size(158, 23);
      this.btnEPG.TabIndex = 4;
      this.btnEPG.Text = "GrabEpg";
      this.btnEPG.UseVisualStyleBackColor = true;
      this.btnEPG.Click += new System.EventHandler(this.btnEPG_Click);
      // 
      // buttonTimeShift
      // 
      this.buttonTimeShift.Location = new System.Drawing.Point(574, 99);
      this.buttonTimeShift.Name = "buttonTimeShift";
      this.buttonTimeShift.Size = new System.Drawing.Size(94, 23);
      this.buttonTimeShift.TabIndex = 5;
      this.buttonTimeShift.Text = "TimeShift dvr-ms";
      this.buttonTimeShift.UseVisualStyleBackColor = true;
      this.buttonTimeShift.Click += new System.EventHandler(this.buttonTimeShift_Click);
      // 
      // buttonRecord
      // 
      this.buttonRecord.Location = new System.Drawing.Point(674, 99);
      this.buttonRecord.Name = "buttonRecord";
      this.buttonRecord.Size = new System.Drawing.Size(96, 23);
      this.buttonRecord.TabIndex = 6;
      this.buttonRecord.Text = "Record dvr-ms";
      this.buttonRecord.UseVisualStyleBackColor = true;
      this.buttonRecord.Click += new System.EventHandler(this.buttonRecord_Click);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(19, 41);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(77, 13);
      this.label2.TabIndex = 7;
      this.label2.Text = "Tuner Locked:";
      // 
      // labelTunerLock
      // 
      this.labelTunerLock.AutoSize = true;
      this.labelTunerLock.Location = new System.Drawing.Point(105, 41);
      this.labelTunerLock.Name = "labelTunerLock";
      this.labelTunerLock.Size = new System.Drawing.Size(77, 13);
      this.labelTunerLock.TabIndex = 8;
      this.labelTunerLock.Text = "Tuner Locked:";
      // 
      // progressBarLevel
      // 
      this.progressBarLevel.Location = new System.Drawing.Point(105, 60);
      this.progressBarLevel.Name = "progressBarLevel";
      this.progressBarLevel.Size = new System.Drawing.Size(278, 10);
      this.progressBarLevel.Step = 1;
      this.progressBarLevel.TabIndex = 9;
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Location = new System.Drawing.Point(105, 76);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(278, 10);
      this.progressBarQuality.Step = 1;
      this.progressBarQuality.TabIndex = 10;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(19, 73);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(72, 13);
      this.label3.TabIndex = 11;
      this.label3.Text = "Signal quality:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(19, 57);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(64, 13);
      this.label4.TabIndex = 12;
      this.label4.Text = "Signal level:";
      // 
      // timer1
      // 
      this.timer1.Interval = 500;
      // 
      // listViewChannels
      // 
      this.listViewChannels.Location = new System.Drawing.Point(12, 257);
      this.listViewChannels.Name = "listViewChannels";
      this.listViewChannels.Size = new System.Drawing.Size(439, 190);
      this.listViewChannels.TabIndex = 13;
      this.listViewChannels.UseCompatibleStateImageBehavior = false;
      this.listViewChannels.View = System.Windows.Forms.View.List;
      this.listViewChannels.SelectedIndexChanged += new System.EventHandler(this.listViewChannels_SelectedIndexChanged);
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(12, 176);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(85, 13);
      this.label5.TabIndex = 14;
      this.label5.Text = "Current channel:";
      // 
      // labelChannel
      // 
      this.labelChannel.AutoSize = true;
      this.labelChannel.Location = new System.Drawing.Point(13, 202);
      this.labelChannel.Name = "labelChannel";
      this.labelChannel.Size = new System.Drawing.Size(337, 13);
      this.labelChannel.TabIndex = 15;
      this.labelChannel.Text = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(12, 241);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(258, 13);
      this.label6.TabIndex = 16;
      this.label6.Text = "Channel List (double click on an channel to tune to it)";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(611, 157);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(158, 23);
      this.button1.TabIndex = 17;
      this.button1.Text = "Grab Teletext Page 100";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Location = new System.Drawing.Point(468, 202);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(301, 245);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 18;
      this.pictureBox1.TabStop = false;
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(673, 128);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(96, 23);
      this.button2.TabIndex = 20;
      this.button2.Text = "Record .mpg";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(573, 128);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(94, 23);
      this.button3.TabIndex = 19;
      this.button3.Text = "TimeShift .ts";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(19, 99);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(60, 13);
      this.label7.TabIndex = 21;
      this.label7.Text = "Scrambled:";
      // 
      // labelScrambled
      // 
      this.labelScrambled.AutoSize = true;
      this.labelScrambled.Location = new System.Drawing.Point(105, 99);
      this.labelScrambled.Name = "labelScrambled";
      this.labelScrambled.Size = new System.Drawing.Size(10, 13);
      this.labelScrambled.TabIndex = 22;
      this.labelScrambled.Tag = "";
      this.labelScrambled.Text = "-";
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(790, 459);
      this.Controls.Add(this.labelScrambled);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.labelChannel);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.listViewChannels);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.progressBarQuality);
      this.Controls.Add(this.progressBarLevel);
      this.Controls.Add(this.labelTunerLock);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.buttonRecord);
      this.Controls.Add(this.buttonTimeShift);
      this.Controls.Add(this.btnEPG);
      this.Controls.Add(this.buttonTune);
      this.Controls.Add(this.buttonScan);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.comboBoxCards);
      this.Name = "Form1";
      this.Text = "TvLibrary test application";
      this.Load += new System.EventHandler(this.Form1_Load);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBoxCards;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button buttonScan;
    private System.Windows.Forms.Button buttonTune;
    private System.Windows.Forms.Button btnEPG;
    private System.Windows.Forms.Button buttonTimeShift;
    private System.Windows.Forms.Button buttonRecord;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label labelTunerLock;
    private System.Windows.Forms.ProgressBar progressBarLevel;
    private System.Windows.Forms.ProgressBar progressBarQuality;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Timer timer1;
    private System.Windows.Forms.ListView listViewChannels;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label labelChannel;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label labelScrambled;
  }
}

