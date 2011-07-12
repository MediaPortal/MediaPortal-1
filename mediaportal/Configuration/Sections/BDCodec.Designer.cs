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
      this.mpGroupBox1.Text = "Settings";
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(9, 76);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(146, 16);
      this.mpLabel2.TabIndex = 10;
      this.mpLabel2.Text = "VC1 Video decoder:";
      // 
      // vc1videoCodecComboBox
      // 
      this.vc1videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.vc1videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.vc1videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.vc1videoCodecComboBox.Location = new System.Drawing.Point(168, 72);
      this.vc1videoCodecComboBox.Name = "vc1videoCodecComboBox";
      this.vc1videoCodecComboBox.Size = new System.Drawing.Size(278, 21);
      this.vc1videoCodecComboBox.Sorted = true;
      this.vc1videoCodecComboBox.TabIndex = 11;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(9, 52);
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
      this.h264videoCodecComboBox.Size = new System.Drawing.Size(278, 21);
      this.h264videoCodecComboBox.Sorted = true;
      this.h264videoCodecComboBox.TabIndex = 9;
      this.h264videoCodecComboBox.SelectedIndexChanged += new System.EventHandler(this.h264videoCodecComboBox_SelectedIndexChanged);
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(168, 120);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(278, 21);
      this.audioRendererComboBox.Sorted = true;
      this.audioRendererComboBox.TabIndex = 7;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(9, 124);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(88, 17);
      this.label3.TabIndex = 6;
      this.label3.Text = "Audio renderer:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(9, 28);
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
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 96);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(278, 21);
      this.audioCodecComboBox.Sorted = true;
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
      this.videoCodecComboBox.Size = new System.Drawing.Size(278, 21);
      this.videoCodecComboBox.Sorted = true;
      this.videoCodecComboBox.TabIndex = 1;
      this.videoCodecComboBox.SelectedIndexChanged += new System.EventHandler(this.videoCodecComboBox_SelectedIndexChanged);
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(9, 100);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(179, 21);
      this.label5.TabIndex = 2;
      this.label5.Text = "Mpeg/AC3/DTS audio decoder:";
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
  }
}
