namespace MediaPortal.Configuration.Sections
{
  partial class MovieCodec
  {
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
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelAACDecoder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.aacAudioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.enableAudioDualMonoModes = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.autoDecoderSettings = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.h264videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.audioRendererComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.labelAACDecoder);
      this.mpGroupBox1.Controls.Add(this.aacAudioCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.enableAudioDualMonoModes);
      this.mpGroupBox1.Controls.Add(this.autoDecoderSettings);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.h264videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.audioRendererComboBox);
      this.mpGroupBox1.Controls.Add(this.label3);
      this.mpGroupBox1.Controls.Add(this.label6);
      this.mpGroupBox1.Controls.Add(this.audioCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.label5);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(3, 3);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(466, 226);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Settings";
      // 
      // labelAACDecoder
      // 
      this.labelAACDecoder.Location = new System.Drawing.Point(16, 99);
      this.labelAACDecoder.Name = "labelAACDecoder";
      this.labelAACDecoder.Size = new System.Drawing.Size(146, 17);
      this.labelAACDecoder.TabIndex = 14;
      this.labelAACDecoder.Text = "AAC audio decoder:";
      // 
      // aacAudioCodecComboBox
      // 
      this.aacAudioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.aacAudioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.aacAudioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.aacAudioCodecComboBox.Location = new System.Drawing.Point(168, 96);
      this.aacAudioCodecComboBox.Name = "aacAudioCodecComboBox";
      this.aacAudioCodecComboBox.Size = new System.Drawing.Size(282, 21);
      this.aacAudioCodecComboBox.TabIndex = 15;
      // 
      // enableAudioDualMonoModes
      // 
      this.enableAudioDualMonoModes.AutoSize = true;
      this.enableAudioDualMonoModes.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.enableAudioDualMonoModes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.enableAudioDualMonoModes.Location = new System.Drawing.Point(19, 185);
      this.enableAudioDualMonoModes.Name = "enableAudioDualMonoModes";
      this.enableAudioDualMonoModes.Size = new System.Drawing.Size(386, 30);
      this.enableAudioDualMonoModes.TabIndex = 10;
      this.enableAudioDualMonoModes.Text = "Enable AudioDualMono mode switching\r\n(if 1 audio stream contains 2x mono channels" +
          ", you can switch between them)";
      this.enableAudioDualMonoModes.UseVisualStyleBackColor = true;
      // 
      // autoDecoderSettings
      // 
      this.autoDecoderSettings.AutoSize = true;
      this.autoDecoderSettings.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.autoDecoderSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.autoDecoderSettings.Location = new System.Drawing.Point(19, 156);
      this.autoDecoderSettings.Name = "autoDecoderSettings";
      this.autoDecoderSettings.Size = new System.Drawing.Size(309, 30);
      this.autoDecoderSettings.TabIndex = 0;
      this.autoDecoderSettings.Text = "Automatic Decoder Settings \r\n(use with caution - knowledge of DirectShow merits r" +
          "equired)";
      this.autoDecoderSettings.UseVisualStyleBackColor = true;
      this.autoDecoderSettings.CheckedChanged += new System.EventHandler(this.autoDecoderSettings_CheckedChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(16, 52);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(146, 16);
      this.mpLabel1.TabIndex = 8;
      this.mpLabel1.Text = "H.264 Video decoder:";
      // 
      // h264videoCodecComboBox
      // 
      this.h264videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.h264videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.h264videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.h264videoCodecComboBox.Location = new System.Drawing.Point(168, 48);
      this.h264videoCodecComboBox.Name = "h264videoCodecComboBox";
      this.h264videoCodecComboBox.Size = new System.Drawing.Size(282, 21);
      this.h264videoCodecComboBox.TabIndex = 9;
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(168, 120);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(282, 21);
      this.audioRendererComboBox.TabIndex = 7;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 124);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(88, 17);
      this.label3.TabIndex = 6;
      this.label3.Text = "Audio renderer:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 28);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(146, 16);
      this.label6.TabIndex = 0;
      this.label6.Text = "MPEG-2 Video decoder:";
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 72);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(282, 21);
      this.audioCodecComboBox.TabIndex = 3;
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(168, 24);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(282, 21);
      this.videoCodecComboBox.TabIndex = 1;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 76);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(159, 21);
      this.label5.TabIndex = 2;
      this.label5.Text = "MPEG / AC3 audio decoder:";
      // 
      // mpLabelNote
      // 
      this.mpLabelNote.AutoSize = true;
      this.mpLabelNote.Location = new System.Drawing.Point(106, 26);
      this.mpLabelNote.Name = "mpLabelNote";
      this.mpLabelNote.Size = new System.Drawing.Size(247, 13);
      this.mpLabelNote.TabIndex = 2;
      this.mpLabelNote.Text = "All .ts files will be played using TV codecs settings !";
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.mpLabelNote);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(3, 248);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(466, 61);
      this.mpGroupBox2.TabIndex = 3;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Note";
      // 
      // MovieCodec
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "MovieCodec";
      this.Size = new System.Drawing.Size(472, 391);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel labelAACDecoder;
    private MediaPortal.UserInterface.Controls.MPComboBox aacAudioCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox enableAudioDualMonoModes;
    private MediaPortal.UserInterface.Controls.MPCheckBox autoDecoderSettings;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox h264videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox audioRendererComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPComboBox audioCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelNote;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
  }
}
