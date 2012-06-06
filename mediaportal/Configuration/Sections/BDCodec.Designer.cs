namespace MediaPortal.Configuration.Sections
{
  partial class BDCodec
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
      this.configAudioRenderer = new MediaPortal.UserInterface.Controls.MPButton();
      this.configAUDIO = new MediaPortal.UserInterface.Controls.MPButton();
      this.configVC1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.configH264 = new MediaPortal.UserInterface.Controls.MPButton();
      this.configMPEG = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.vc1videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.h264videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.audioRendererComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.configAudioRenderer);
      this.mpGroupBox1.Controls.Add(this.configAUDIO);
      this.mpGroupBox1.Controls.Add(this.configVC1);
      this.mpGroupBox1.Controls.Add(this.configH264);
      this.mpGroupBox1.Controls.Add(this.configMPEG);
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.Controls.Add(this.vc1videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.h264videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.audioRendererComboBox);
      this.mpGroupBox1.Controls.Add(this.label3);
      this.mpGroupBox1.Controls.Add(this.label6);
      this.mpGroupBox1.Controls.Add(this.audioCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.label5);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(462, 313);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Settings Decoder";
      // 
      // configAudioRenderer
      // 
      this.configAudioRenderer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configAudioRenderer.BackColor = System.Drawing.Color.Transparent;
      this.configAudioRenderer.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configAudioRenderer.Location = new System.Drawing.Point(422, 127);
      this.configAudioRenderer.Name = "configAudioRenderer";
      this.configAudioRenderer.Size = new System.Drawing.Size(35, 21);
      this.configAudioRenderer.TabIndex = 74;
      this.configAudioRenderer.Text = "\r\n";
      this.configAudioRenderer.UseVisualStyleBackColor = false;
      this.configAudioRenderer.Click += new System.EventHandler(this.configAudioRenderer_Click);
      // 
      // configAUDIO
      // 
      this.configAUDIO.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configAUDIO.BackColor = System.Drawing.Color.Transparent;
      this.configAUDIO.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configAUDIO.Location = new System.Drawing.Point(422, 103);
      this.configAUDIO.Name = "configAUDIO";
      this.configAUDIO.Size = new System.Drawing.Size(35, 21);
      this.configAUDIO.TabIndex = 73;
      this.configAUDIO.Text = "\r\n";
      this.configAUDIO.UseVisualStyleBackColor = false;
      this.configAUDIO.Click += new System.EventHandler(this.configAUDIO_Click);
      // 
      // configVC1
      // 
      this.configVC1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configVC1.BackColor = System.Drawing.Color.Transparent;
      this.configVC1.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configVC1.Location = new System.Drawing.Point(422, 72);
      this.configVC1.Name = "configVC1";
      this.configVC1.Size = new System.Drawing.Size(35, 21);
      this.configVC1.TabIndex = 72;
      this.configVC1.Text = "\r\n";
      this.configVC1.UseVisualStyleBackColor = false;
      this.configVC1.Click += new System.EventHandler(this.configVC1_Click);
      // 
      // configH264
      // 
      this.configH264.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configH264.BackColor = System.Drawing.Color.Transparent;
      this.configH264.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configH264.Location = new System.Drawing.Point(422, 48);
      this.configH264.Name = "configH264";
      this.configH264.Size = new System.Drawing.Size(35, 21);
      this.configH264.TabIndex = 71;
      this.configH264.Text = "\r\n";
      this.configH264.UseVisualStyleBackColor = false;
      this.configH264.Click += new System.EventHandler(this.configH264_Click);
      // 
      // configMPEG
      // 
      this.configMPEG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configMPEG.BackColor = System.Drawing.Color.Transparent;
      this.configMPEG.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configMPEG.Location = new System.Drawing.Point(422, 24);
      this.configMPEG.Name = "configMPEG";
      this.configMPEG.Size = new System.Drawing.Size(35, 21);
      this.configMPEG.TabIndex = 70;
      this.configMPEG.Text = "\r\n";
      this.configMPEG.UseVisualStyleBackColor = false;
      this.configMPEG.Click += new System.EventHandler(this.configMPEG_Click);
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(16, 76);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(88, 16);
      this.mpLabel2.TabIndex = 10;
      this.mpLabel2.Text = "VC-1 Video :";
      // 
      // vc1videoCodecComboBox
      // 
      this.vc1videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.vc1videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.vc1videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.vc1videoCodecComboBox.Location = new System.Drawing.Point(122, 72);
      this.vc1videoCodecComboBox.Name = "vc1videoCodecComboBox";
      this.vc1videoCodecComboBox.Size = new System.Drawing.Size(295, 21);
      this.vc1videoCodecComboBox.Sorted = true;
      this.vc1videoCodecComboBox.TabIndex = 5;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(16, 52);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(88, 16);
      this.mpLabel1.TabIndex = 8;
      this.mpLabel1.Text = "H.264 Video :";
      // 
      // h264videoCodecComboBox
      // 
      this.h264videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.h264videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.h264videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.h264videoCodecComboBox.Location = new System.Drawing.Point(122, 48);
      this.h264videoCodecComboBox.Name = "h264videoCodecComboBox";
      this.h264videoCodecComboBox.Size = new System.Drawing.Size(295, 21);
      this.h264videoCodecComboBox.Sorted = true;
      this.h264videoCodecComboBox.TabIndex = 3;
      this.h264videoCodecComboBox.SelectedIndexChanged += new System.EventHandler(this.h264videoCodecComboBox_SelectedIndexChanged);
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(122, 128);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(295, 21);
      this.audioRendererComboBox.Sorted = true;
      this.audioRendererComboBox.TabIndex = 9;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 132);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(88, 17);
      this.label3.TabIndex = 6;
      this.label3.Text = "Audio renderer:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 28);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(88, 16);
      this.label6.TabIndex = 0;
      this.label6.Text = "MPEG-2 Video :";
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(122, 104);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(295, 21);
      this.audioCodecComboBox.Sorted = true;
      this.audioCodecComboBox.TabIndex = 7;
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(122, 24);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(295, 21);
      this.videoCodecComboBox.Sorted = true;
      this.videoCodecComboBox.TabIndex = 1;
      this.videoCodecComboBox.SelectedIndexChanged += new System.EventHandler(this.videoCodecComboBox_SelectedIndexChanged);
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 108);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(125, 21);
      this.label5.TabIndex = 2;
      this.label5.Text = "LPCM/AC3/DTS :";
      // 
      // BDCodec
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "BDCodec";
      this.Size = new System.Drawing.Size(472, 391);
      this.mpGroupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox h264videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox audioRendererComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPComboBox audioCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPComboBox vc1videoCodecComboBox;
    private UserInterface.Controls.MPButton configMPEG;
    private UserInterface.Controls.MPButton configAudioRenderer;
    private UserInterface.Controls.MPButton configAUDIO;
    private UserInterface.Controls.MPButton configVC1;
    private UserInterface.Controls.MPButton configH264;
  }
}
