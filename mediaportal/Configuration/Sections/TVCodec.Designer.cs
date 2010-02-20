namespace MediaPortal.Configuration.Sections
{
  partial class TVCodec
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.audioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelAACDecoder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.aacAudioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.h264videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelH264Decoder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelMPEG2Decoder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAudioDecoder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAudioRenderer = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioRendererComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabelNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.audioCodecComboBox);
      this.groupBox1.Controls.Add(this.labelAACDecoder);
      this.groupBox1.Controls.Add(this.aacAudioCodecComboBox);
      this.groupBox1.Controls.Add(this.h264videoCodecComboBox);
      this.groupBox1.Controls.Add(this.labelH264Decoder);
      this.groupBox1.Controls.Add(this.videoCodecComboBox);
      this.groupBox1.Controls.Add(this.labelMPEG2Decoder);
      this.groupBox1.Controls.Add(this.labelAudioDecoder);
      this.groupBox1.Controls.Add(this.labelAudioRenderer);
      this.groupBox1.Controls.Add(this.audioRendererComboBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(3, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(466, 154);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
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
      this.audioCodecComboBox.TabIndex = 5;
      // 
      // labelAACDecoder
      // 
      this.labelAACDecoder.Location = new System.Drawing.Point(16, 99);
      this.labelAACDecoder.Name = "labelAACDecoder";
      this.labelAACDecoder.Size = new System.Drawing.Size(146, 17);
      this.labelAACDecoder.TabIndex = 12;
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
      this.aacAudioCodecComboBox.TabIndex = 13;
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
      this.h264videoCodecComboBox.TabIndex = 3;
      this.h264videoCodecComboBox.SelectedIndexChanged += new System.EventHandler(this.h264videoCodecComboBox_SelectedIndexChanged);
      // 
      // labelH264Decoder
      // 
      this.labelH264Decoder.Location = new System.Drawing.Point(16, 52);
      this.labelH264Decoder.Name = "labelH264Decoder";
      this.labelH264Decoder.Size = new System.Drawing.Size(132, 17);
      this.labelH264Decoder.TabIndex = 2;
      this.labelH264Decoder.Text = "H.264 Video decoder:";
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
      this.videoCodecComboBox.SelectedIndexChanged += new System.EventHandler(this.videoCodecComboBox_SelectedIndexChanged);
      // 
      // labelMPEG2Decoder
      // 
      this.labelMPEG2Decoder.Location = new System.Drawing.Point(16, 28);
      this.labelMPEG2Decoder.Name = "labelMPEG2Decoder";
      this.labelMPEG2Decoder.Size = new System.Drawing.Size(132, 17);
      this.labelMPEG2Decoder.TabIndex = 0;
      this.labelMPEG2Decoder.Text = "MPEG-2 Video decoder:";
      // 
      // labelAudioDecoder
      // 
      this.labelAudioDecoder.Location = new System.Drawing.Point(16, 76);
      this.labelAudioDecoder.Name = "labelAudioDecoder";
      this.labelAudioDecoder.Size = new System.Drawing.Size(157, 18);
      this.labelAudioDecoder.TabIndex = 4;
      this.labelAudioDecoder.Text = "MPEG / AC3 audio decoder:";
      // 
      // labelAudioRenderer
      // 
      this.labelAudioRenderer.Location = new System.Drawing.Point(16, 124);
      this.labelAudioRenderer.Name = "labelAudioRenderer";
      this.labelAudioRenderer.Size = new System.Drawing.Size(88, 16);
      this.labelAudioRenderer.TabIndex = 6;
      this.labelAudioRenderer.Text = "Audio renderer:";
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
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.mpLabelNote);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(3, 248);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(466, 61);
      this.mpGroupBox2.TabIndex = 14;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Note";
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
      // TVCodec
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "TVCodec";
      this.Size = new System.Drawing.Size(472, 391);
      this.groupBox1.ResumeLayout(false);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPComboBox audioCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel labelAACDecoder;
    private MediaPortal.UserInterface.Controls.MPComboBox aacAudioCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox h264videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel labelH264Decoder;
    private MediaPortal.UserInterface.Controls.MPComboBox videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel labelMPEG2Decoder;
    private MediaPortal.UserInterface.Controls.MPLabel labelAudioDecoder;
    private MediaPortal.UserInterface.Controls.MPLabel labelAudioRenderer;
    private MediaPortal.UserInterface.Controls.MPComboBox audioRendererComboBox;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelNote;
  }
}
