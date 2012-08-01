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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MovieCodec));
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabelNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.configSplitterSync = new MediaPortal.UserInterface.Controls.MPButton();
      this.configSplitterSource = new MediaPortal.UserInterface.Controls.MPButton();
      this.configAudioRenderer = new MediaPortal.UserInterface.Controls.MPButton();
      this.configAACAudio = new MediaPortal.UserInterface.Controls.MPButton();
      this.configMPEGAudio = new MediaPortal.UserInterface.Controls.MPButton();
      this.configDivxXvid = new MediaPortal.UserInterface.Controls.MPButton();
      this.configVC1i = new MediaPortal.UserInterface.Controls.MPButton();
      this.configVC1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.configH264 = new MediaPortal.UserInterface.Controls.MPButton();
      this.configMPEG = new MediaPortal.UserInterface.Controls.MPButton();
      this.xvidvideoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabelXVID = new MediaPortal.UserInterface.Controls.MPLabel();
      this.ForceSourceSplitter = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.SplitterFileComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.SplitterComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.vc1ivideoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.vc1videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelAACDecoder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.aacAudioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.autoDecoderSettings = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.h264videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.audioRendererComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.mpLabelNote);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(6, 359);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(462, 45);
      this.mpGroupBox2.TabIndex = 3;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Note";
      // 
      // mpLabelNote
      // 
      this.mpLabelNote.AutoSize = true;
      this.mpLabelNote.Location = new System.Drawing.Point(106, 19);
      this.mpLabelNote.Name = "mpLabelNote";
      this.mpLabelNote.Size = new System.Drawing.Size(247, 13);
      this.mpLabelNote.TabIndex = 2;
      this.mpLabelNote.Text = "All .ts files will be played using TV codecs settings !";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.configSplitterSync);
      this.mpGroupBox1.Controls.Add(this.configSplitterSource);
      this.mpGroupBox1.Controls.Add(this.configAudioRenderer);
      this.mpGroupBox1.Controls.Add(this.configAACAudio);
      this.mpGroupBox1.Controls.Add(this.configMPEGAudio);
      this.mpGroupBox1.Controls.Add(this.configDivxXvid);
      this.mpGroupBox1.Controls.Add(this.configVC1i);
      this.mpGroupBox1.Controls.Add(this.configVC1);
      this.mpGroupBox1.Controls.Add(this.configH264);
      this.mpGroupBox1.Controls.Add(this.configMPEG);
      this.mpGroupBox1.Controls.Add(this.xvidvideoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.mpLabelXVID);
      this.mpGroupBox1.Controls.Add(this.ForceSourceSplitter);
      this.mpGroupBox1.Controls.Add(this.SplitterFileComboBox);
      this.mpGroupBox1.Controls.Add(this.mpLabel5);
      this.mpGroupBox1.Controls.Add(this.SplitterComboBox);
      this.mpGroupBox1.Controls.Add(this.mpLabel4);
      this.mpGroupBox1.Controls.Add(this.mpLabel3);
      this.mpGroupBox1.Controls.Add(this.vc1ivideoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.Controls.Add(this.vc1videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.labelAACDecoder);
      this.mpGroupBox1.Controls.Add(this.aacAudioCodecComboBox);
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
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(462, 353);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Settings Decoder";
      // 
      // configSplitterSync
      // 
      this.configSplitterSync.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configSplitterSync.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configSplitterSync.Location = new System.Drawing.Point(422, 256);
      this.configSplitterSync.Name = "configSplitterSync";
      this.configSplitterSync.Size = new System.Drawing.Size(35, 21);
      this.configSplitterSync.TabIndex = 78;
      this.configSplitterSync.UseVisualStyleBackColor = true;
      this.configSplitterSync.Click += new System.EventHandler(this.configSplitterSync_Click);
      // 
      // configSplitterSource
      // 
      this.configSplitterSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configSplitterSource.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configSplitterSource.Location = new System.Drawing.Point(422, 232);
      this.configSplitterSource.Name = "configSplitterSource";
      this.configSplitterSource.Size = new System.Drawing.Size(35, 21);
      this.configSplitterSource.TabIndex = 77;
      this.configSplitterSource.UseVisualStyleBackColor = true;
      this.configSplitterSource.Click += new System.EventHandler(this.configSplitterSource_Click);
      // 
      // configAudioRenderer
      // 
      this.configAudioRenderer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configAudioRenderer.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configAudioRenderer.Location = new System.Drawing.Point(422, 208);
      this.configAudioRenderer.Name = "configAudioRenderer";
      this.configAudioRenderer.Size = new System.Drawing.Size(35, 21);
      this.configAudioRenderer.TabIndex = 76;
      this.configAudioRenderer.UseVisualStyleBackColor = true;
      this.configAudioRenderer.Click += new System.EventHandler(this.configAudioRenderer_Click);
      // 
      // configAACAudio
      // 
      this.configAACAudio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configAACAudio.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configAACAudio.Location = new System.Drawing.Point(422, 176);
      this.configAACAudio.Name = "configAACAudio";
      this.configAACAudio.Size = new System.Drawing.Size(35, 21);
      this.configAACAudio.TabIndex = 75;
      this.configAACAudio.UseVisualStyleBackColor = true;
      this.configAACAudio.Click += new System.EventHandler(this.configAACAudio_Click);
      // 
      // configMPEGAudio
      // 
      this.configMPEGAudio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configMPEGAudio.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configMPEGAudio.Location = new System.Drawing.Point(422, 152);
      this.configMPEGAudio.Name = "configMPEGAudio";
      this.configMPEGAudio.Size = new System.Drawing.Size(35, 21);
      this.configMPEGAudio.TabIndex = 74;
      this.configMPEGAudio.UseVisualStyleBackColor = true;
      this.configMPEGAudio.Click += new System.EventHandler(this.configMPEGAudio_Click);
      // 
      // configDivxXvid
      // 
      this.configDivxXvid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configDivxXvid.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configDivxXvid.Location = new System.Drawing.Point(422, 119);
      this.configDivxXvid.Name = "configDivxXvid";
      this.configDivxXvid.Size = new System.Drawing.Size(35, 21);
      this.configDivxXvid.TabIndex = 73;
      this.configDivxXvid.UseVisualStyleBackColor = true;
      this.configDivxXvid.Click += new System.EventHandler(this.configDivxXvid_Click);
      // 
      // configVC1i
      // 
      this.configVC1i.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configVC1i.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configVC1i.Location = new System.Drawing.Point(422, 96);
      this.configVC1i.Name = "configVC1i";
      this.configVC1i.Size = new System.Drawing.Size(35, 21);
      this.configVC1i.TabIndex = 72;
      this.configVC1i.UseVisualStyleBackColor = true;
      this.configVC1i.Click += new System.EventHandler(this.configVC1i_Click);
      // 
      // configVC1
      // 
      this.configVC1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configVC1.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configVC1.Location = new System.Drawing.Point(422, 72);
      this.configVC1.Name = "configVC1";
      this.configVC1.Size = new System.Drawing.Size(35, 21);
      this.configVC1.TabIndex = 71;
      this.configVC1.UseVisualStyleBackColor = true;
      this.configVC1.Click += new System.EventHandler(this.configVC1_Click);
      // 
      // configH264
      // 
      this.configH264.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configH264.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configH264.Location = new System.Drawing.Point(422, 48);
      this.configH264.Name = "configH264";
      this.configH264.Size = new System.Drawing.Size(35, 21);
      this.configH264.TabIndex = 70;
      this.configH264.UseVisualStyleBackColor = true;
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
      this.configMPEG.TabIndex = 69;
      this.configMPEG.Text = "\r\n";
      this.configMPEG.UseVisualStyleBackColor = false;
      this.configMPEG.Click += new System.EventHandler(this.configMPEG_Click);
      // 
      // xvidvideoCodecComboBox
      // 
      this.xvidvideoCodecComboBox.AccessibleName = resources.GetString("xvidvideoCodecComboBox.AccessibleName");
      this.xvidvideoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.xvidvideoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.xvidvideoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.xvidvideoCodecComboBox.Location = new System.Drawing.Point(122, 119);
      this.xvidvideoCodecComboBox.Name = "xvidvideoCodecComboBox";
      this.xvidvideoCodecComboBox.Size = new System.Drawing.Size(295, 21);
      this.xvidvideoCodecComboBox.Sorted = true;
      this.xvidvideoCodecComboBox.TabIndex = 9;
      // 
      // mpLabelXVID
      // 
      this.mpLabelXVID.AccessibleName = resources.GetString("mpLabelXVID.AccessibleName");
      this.mpLabelXVID.Location = new System.Drawing.Point(16, 123);
      this.mpLabelXVID.Name = "mpLabelXVID";
      this.mpLabelXVID.Size = new System.Drawing.Size(116, 18);
      this.mpLabelXVID.TabIndex = 8;
      this.mpLabelXVID.Text = "DivX / Xvid Video :";
      // 
      // ForceSourceSplitter
      // 
      this.ForceSourceSplitter.AutoSize = true;
      this.ForceSourceSplitter.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.ForceSourceSplitter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ForceSourceSplitter.Location = new System.Drawing.Point(19, 325);
      this.ForceSourceSplitter.Name = "ForceSourceSplitter";
      this.ForceSourceSplitter.Size = new System.Drawing.Size(272, 17);
      this.ForceSourceSplitter.TabIndex = 21;
      this.ForceSourceSplitter.Text = "Source Splitter Selection (Try to use Splitter Settings)";
      this.ForceSourceSplitter.UseVisualStyleBackColor = true;
      this.ForceSourceSplitter.CheckedChanged += new System.EventHandler(this.ForceSourceSplitter_CheckedChanged);
      // 
      // SplitterFileComboBox
      // 
      this.SplitterFileComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.SplitterFileComboBox.BorderColor = System.Drawing.Color.Empty;
      this.SplitterFileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.SplitterFileComboBox.Location = new System.Drawing.Point(122, 256);
      this.SplitterFileComboBox.Name = "SplitterFileComboBox";
      this.SplitterFileComboBox.Size = new System.Drawing.Size(295, 21);
      this.SplitterFileComboBox.Sorted = true;
      this.SplitterFileComboBox.TabIndex = 19;
      // 
      // mpLabel5
      // 
      this.mpLabel5.Location = new System.Drawing.Point(16, 260);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(116, 17);
      this.mpLabel5.TabIndex = 18;
      this.mpLabel5.Text = "Splitter Filesync :";
      // 
      // SplitterComboBox
      // 
      this.SplitterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.SplitterComboBox.BorderColor = System.Drawing.Color.Empty;
      this.SplitterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.SplitterComboBox.Location = new System.Drawing.Point(122, 232);
      this.SplitterComboBox.Name = "SplitterComboBox";
      this.SplitterComboBox.Size = new System.Drawing.Size(295, 21);
      this.SplitterComboBox.Sorted = true;
      this.SplitterComboBox.TabIndex = 17;
      // 
      // mpLabel4
      // 
      this.mpLabel4.Location = new System.Drawing.Point(16, 236);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(98, 17);
      this.mpLabel4.TabIndex = 16;
      this.mpLabel4.Text = "Splitter Source :";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AccessibleName = resources.GetString("mpLabel3.AccessibleName");
      this.mpLabel3.Location = new System.Drawing.Point(16, 101);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(98, 16);
      this.mpLabel3.TabIndex = 6;
      this.mpLabel3.Text = "VC-1i Video :";
      // 
      // vc1ivideoCodecComboBox
      // 
      this.vc1ivideoCodecComboBox.AccessibleName = resources.GetString("vc1ivideoCodecComboBox.AccessibleName");
      this.vc1ivideoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.vc1ivideoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.vc1ivideoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.vc1ivideoCodecComboBox.Location = new System.Drawing.Point(122, 96);
      this.vc1ivideoCodecComboBox.Name = "vc1ivideoCodecComboBox";
      this.vc1ivideoCodecComboBox.Size = new System.Drawing.Size(295, 21);
      this.vc1ivideoCodecComboBox.Sorted = true;
      this.vc1ivideoCodecComboBox.TabIndex = 7;
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(16, 76);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(98, 16);
      this.mpLabel2.TabIndex = 4;
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
      // labelAACDecoder
      // 
      this.labelAACDecoder.Location = new System.Drawing.Point(16, 179);
      this.labelAACDecoder.Name = "labelAACDecoder";
      this.labelAACDecoder.Size = new System.Drawing.Size(98, 17);
      this.labelAACDecoder.TabIndex = 12;
      this.labelAACDecoder.Text = "AAC audio :";
      // 
      // aacAudioCodecComboBox
      // 
      this.aacAudioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.aacAudioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.aacAudioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.aacAudioCodecComboBox.Location = new System.Drawing.Point(122, 176);
      this.aacAudioCodecComboBox.Name = "aacAudioCodecComboBox";
      this.aacAudioCodecComboBox.Size = new System.Drawing.Size(295, 21);
      this.aacAudioCodecComboBox.Sorted = true;
      this.aacAudioCodecComboBox.TabIndex = 13;
      // 
      // autoDecoderSettings
      // 
      this.autoDecoderSettings.AutoSize = true;
      this.autoDecoderSettings.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.autoDecoderSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.autoDecoderSettings.Location = new System.Drawing.Point(19, 292);
      this.autoDecoderSettings.Name = "autoDecoderSettings";
      this.autoDecoderSettings.Size = new System.Drawing.Size(309, 30);
      this.autoDecoderSettings.TabIndex = 20;
      this.autoDecoderSettings.Text = "Automatic Decoder Settings \r\n(use with caution - knowledge of DirectShow merits r" +
    "equired)";
      this.autoDecoderSettings.UseVisualStyleBackColor = true;
      this.autoDecoderSettings.CheckedChanged += new System.EventHandler(this.autoDecoderSettings_CheckedChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(16, 52);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(98, 16);
      this.mpLabel1.TabIndex = 2;
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
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(122, 208);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(295, 21);
      this.audioRendererComboBox.Sorted = true;
      this.audioRendererComboBox.TabIndex = 15;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 212);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(98, 17);
      this.label3.TabIndex = 14;
      this.label3.Text = "Audio renderer:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 28);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(98, 16);
      this.label6.TabIndex = 0;
      this.label6.Text = "MPEG-2 Video :";
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(122, 152);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(295, 21);
      this.audioCodecComboBox.Sorted = true;
      this.audioCodecComboBox.TabIndex = 11;
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
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 156);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(127, 21);
      this.label5.TabIndex = 10;
      this.label5.Text = "MPEG / AC3 audio :";
      // 
      // MovieCodec
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "MovieCodec";
      this.Size = new System.Drawing.Size(472, 412);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel labelAACDecoder;
    private MediaPortal.UserInterface.Controls.MPComboBox aacAudioCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox autoDecoderSettings;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox h264videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPComboBox vc1videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPComboBox vc1ivideoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox audioRendererComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPComboBox audioCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelNote;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPComboBox SplitterComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
    private MediaPortal.UserInterface.Controls.MPComboBox SplitterFileComboBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox ForceSourceSplitter;
    private UserInterface.Controls.MPComboBox xvidvideoCodecComboBox;
    private UserInterface.Controls.MPLabel mpLabelXVID;
    private UserInterface.Controls.MPButton configSplitterSync;
    private UserInterface.Controls.MPButton configSplitterSource;
    private UserInterface.Controls.MPButton configAudioRenderer;
    private UserInterface.Controls.MPButton configAACAudio;
    private UserInterface.Controls.MPButton configMPEGAudio;
    private UserInterface.Controls.MPButton configDivxXvid;
    private UserInterface.Controls.MPButton configVC1i;
    private UserInterface.Controls.MPButton configVC1;
    private UserInterface.Controls.MPButton configH264;
    private UserInterface.Controls.MPButton configMPEG;
  }
}
