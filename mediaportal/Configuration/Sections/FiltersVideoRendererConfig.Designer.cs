namespace MediaPortal.Configuration.Sections
{
  partial class FiltersVideoRenderer
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer vmr9components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (vmr9components != null))
      {
        vmr9components.Dispose();
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
      this.VMR9Tips = new MediaPortal.UserInterface.Controls.MPToolTip();
      this.checkboxDXEclusive = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpVMR9FilterMethod = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkboxMpNonsquare = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.UseMadVideoRenderer3D = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.DisableLowLatencyMode = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.UseEVRMadVRForTV = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpMadLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.radioButtonMadVR = new System.Windows.Forms.RadioButton();
      this.radioButtonEVR = new System.Windows.Forms.RadioButton();
      this.radioButtonVMR9 = new System.Windows.Forms.RadioButton();
      this.labelEVRHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxDecimateMask = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.labelFilteringHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxVMRWebStreams = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // checkboxDXEclusive
      // 
      this.checkboxDXEclusive.AutoSize = true;
      this.checkboxDXEclusive.Checked = true;
      this.checkboxDXEclusive.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkboxDXEclusive.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkboxDXEclusive.Location = new System.Drawing.Point(43, 84);
      this.checkboxDXEclusive.Name = "checkboxDXEclusive";
      this.checkboxDXEclusive.Size = new System.Drawing.Size(385, 17);
      this.checkboxDXEclusive.TabIndex = 5;
      this.checkboxDXEclusive.Text = "Use exclusive mode (avoids tearing, MP stays on top during media playback)";
      this.VMR9Tips.SetToolTip(this.checkboxDXEclusive, "Prevents video \"tearing\" during playback");
      this.checkboxDXEclusive.UseVisualStyleBackColor = true;
      // 
      // mpVMR9FilterMethod
      // 
      this.mpVMR9FilterMethod.BorderColor = System.Drawing.Color.Empty;
      this.mpVMR9FilterMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpVMR9FilterMethod.FormattingEnabled = true;
      this.mpVMR9FilterMethod.Items.AddRange(new object[] {
            "None",
            "Point Filtering",
            "Bilinear Filtering",
            "Anisotropic Filtering",
            "Pyrimidal Quad Filtering",
            "Gaussian Quad Filtering"});
      this.mpVMR9FilterMethod.Location = new System.Drawing.Point(43, 152);
      this.mpVMR9FilterMethod.Name = "mpVMR9FilterMethod";
      this.mpVMR9FilterMethod.Size = new System.Drawing.Size(138, 21);
      this.mpVMR9FilterMethod.TabIndex = 2;
      this.VMR9Tips.SetToolTip(this.mpVMR9FilterMethod, "The filtering method determines the scaling algorithm used by the VMR9");
      // 
      // checkboxMpNonsquare
      // 
      this.checkboxMpNonsquare.AutoSize = true;
      this.checkboxMpNonsquare.Checked = true;
      this.checkboxMpNonsquare.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkboxMpNonsquare.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkboxMpNonsquare.Location = new System.Drawing.Point(43, 107);
      this.checkboxMpNonsquare.Name = "checkboxMpNonsquare";
      this.checkboxMpNonsquare.Size = new System.Drawing.Size(303, 17);
      this.checkboxMpNonsquare.TabIndex = 0;
      this.checkboxMpNonsquare.Text = "Use nonsquare mixing for scaling (recommended for quality)";
      this.VMR9Tips.SetToolTip(this.checkboxMpNonsquare, "Using non-square mixing avoids unnecessary scaling operations in the VMR9");
      this.checkboxMpNonsquare.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.UseMadVideoRenderer3D);
      this.mpGroupBox1.Controls.Add(this.DisableLowLatencyMode);
      this.mpGroupBox1.Controls.Add(this.UseEVRMadVRForTV);
      this.mpGroupBox1.Controls.Add(this.mpMadLabel);
      this.mpGroupBox1.Controls.Add(this.radioButtonMadVR);
      this.mpGroupBox1.Controls.Add(this.radioButtonEVR);
      this.mpGroupBox1.Controls.Add(this.radioButtonVMR9);
      this.mpGroupBox1.Controls.Add(this.labelEVRHint);
      this.mpGroupBox1.Controls.Add(this.checkBoxDecimateMask);
      this.mpGroupBox1.Controls.Add(this.labelFilteringHint);
      this.mpGroupBox1.Controls.Add(this.checkBoxVMRWebStreams);
      this.mpGroupBox1.Controls.Add(this.checkboxDXEclusive);
      this.mpGroupBox1.Controls.Add(this.mpVMR9FilterMethod);
      this.mpGroupBox1.Controls.Add(this.label1);
      this.mpGroupBox1.Controls.Add(this.checkboxMpNonsquare);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(428, 409);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Video renderer - advanced settings";
      // 
      // UseMadVideoRenderer3D
      // 
      this.UseMadVideoRenderer3D.AutoSize = true;
      this.UseMadVideoRenderer3D.Checked = true;
      this.UseMadVideoRenderer3D.CheckState = System.Windows.Forms.CheckState.Checked;
      this.UseMadVideoRenderer3D.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.UseMadVideoRenderer3D.Location = new System.Drawing.Point(64, 344);
      this.UseMadVideoRenderer3D.Name = "UseMadVideoRenderer3D";
      this.UseMadVideoRenderer3D.Size = new System.Drawing.Size(182, 17);
      this.UseMadVideoRenderer3D.TabIndex = 17;
      this.UseMadVideoRenderer3D.Text = "Force MP window refresh (for 3D)";
      this.UseMadVideoRenderer3D.UseVisualStyleBackColor = true;
      // 
      // DisableLowLatencyMode
      // 
      this.DisableLowLatencyMode.AutoSize = true;
      this.DisableLowLatencyMode.Checked = true;
      this.DisableLowLatencyMode.CheckState = System.Windows.Forms.CheckState.Checked;
      this.DisableLowLatencyMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.DisableLowLatencyMode.Location = new System.Drawing.Point(64, 327);
      this.DisableLowLatencyMode.Name = "DisableLowLatencyMode";
      this.DisableLowLatencyMode.Size = new System.Drawing.Size(310, 17);
      this.DisableLowLatencyMode.TabIndex = 16;
      this.DisableLowLatencyMode.Text = "Disable low latency rendering mode (can cause stuttering UI)";
      this.DisableLowLatencyMode.UseVisualStyleBackColor = true;
      this.DisableLowLatencyMode.CheckedChanged += new System.EventHandler(this.DisableLowLatencyMode_CheckedChanged);
      // 
      // UseEVRMadVRForTV
      // 
      this.UseEVRMadVRForTV.AutoSize = true;
      this.UseEVRMadVRForTV.Checked = true;
      this.UseEVRMadVRForTV.CheckState = System.Windows.Forms.CheckState.Checked;
      this.UseEVRMadVRForTV.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.UseEVRMadVRForTV.Location = new System.Drawing.Point(64, 308);
      this.UseEVRMadVRForTV.Name = "UseEVRMadVRForTV";
      this.UseEVRMadVRForTV.Size = new System.Drawing.Size(120, 17);
      this.UseEVRMadVRForTV.TabIndex = 15;
      this.UseEVRMadVRForTV.Text = "Use EVR for LiveTV";
      this.UseEVRMadVRForTV.UseVisualStyleBackColor = true;
      this.UseEVRMadVRForTV.CheckedChanged += new System.EventHandler(this.mpCheckBox1_CheckedChanged);
      // 
      // mpMadLabel
      // 
      this.mpMadLabel.AutoSize = true;
      this.mpMadLabel.Location = new System.Drawing.Point(61, 367);
      this.mpMadLabel.Name = "mpMadLabel";
      this.mpMadLabel.Size = new System.Drawing.Size(269, 13);
      this.mpMadLabel.TabIndex = 14;
      this.mpMadLabel.Text = "You need to install madVR manually to be able to use it.";
      // 
      // radioButtonMadVR
      // 
      this.radioButtonMadVR.AutoSize = true;
      this.radioButtonMadVR.Location = new System.Drawing.Point(30, 285);
      this.radioButtonMadVR.Name = "radioButtonMadVR";
      this.radioButtonMadVR.Size = new System.Drawing.Size(187, 17);
      this.radioButtonMadVR.TabIndex = 13;
      this.radioButtonMadVR.TabStop = true;
      this.radioButtonMadVR.Text = "Madshi\'s Video Renderer (madVR)";
      this.radioButtonMadVR.UseVisualStyleBackColor = true;
      this.radioButtonMadVR.CheckedChanged += new System.EventHandler(this.radioButtonMadVR_CheckedChanged);
      // 
      // radioButtonEVR
      // 
      this.radioButtonEVR.AutoSize = true;
      this.radioButtonEVR.Location = new System.Drawing.Point(30, 188);
      this.radioButtonEVR.Name = "radioButtonEVR";
      this.radioButtonEVR.Size = new System.Drawing.Size(182, 17);
      this.radioButtonEVR.TabIndex = 12;
      this.radioButtonEVR.TabStop = true;
      this.radioButtonEVR.Text = "Enhanced Video Renderer (EVR)";
      this.radioButtonEVR.UseVisualStyleBackColor = true;
      this.radioButtonEVR.CheckedChanged += new System.EventHandler(this.radioButtonEVR_CheckedChanged);
      // 
      // radioButtonVMR9
      // 
      this.radioButtonVMR9.AutoSize = true;
      this.radioButtonVMR9.Checked = true;
      this.radioButtonVMR9.Location = new System.Drawing.Point(30, 32);
      this.radioButtonVMR9.Name = "radioButtonVMR9";
      this.radioButtonVMR9.Size = new System.Drawing.Size(180, 17);
      this.radioButtonVMR9.TabIndex = 11;
      this.radioButtonVMR9.TabStop = true;
      this.radioButtonVMR9.Text = "Video Mixing Renderer 9 (VMR9)";
      this.radioButtonVMR9.UseVisualStyleBackColor = true;
      this.radioButtonVMR9.CheckedChanged += new System.EventHandler(this.radioButtonVMR9_CheckedChanged);
      // 
      // labelEVRHint
      // 
      this.labelEVRHint.AutoSize = true;
      this.labelEVRHint.Location = new System.Drawing.Point(61, 217);
      this.labelEVRHint.Name = "labelEVRHint";
      this.labelEVRHint.Size = new System.Drawing.Size(283, 52);
      this.labelEVRHint.TabIndex = 9;
      this.labelEVRHint.Text = "WARNING: Under XP this renderer is only software based.\r\nIf you experience any is" +
    "sue please switch back to VMR9.\r\n\r\nNOTE: You need .NET 3.0 Framework or Vista to" +
    " use this!\r\n";
      // 
      // checkBoxDecimateMask
      // 
      this.checkBoxDecimateMask.AutoSize = true;
      this.checkBoxDecimateMask.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDecimateMask.Location = new System.Drawing.Point(43, 129);
      this.checkBoxDecimateMask.Name = "checkBoxDecimateMask";
      this.checkBoxDecimateMask.Size = new System.Drawing.Size(397, 17);
      this.checkBoxDecimateMask.TabIndex = 8;
      this.checkBoxDecimateMask.Text = "Use decimate (enable only if TV/Video has >= double resolution of your screen)";
      this.checkBoxDecimateMask.UseVisualStyleBackColor = true;
      this.checkBoxDecimateMask.Visible = false;
      // 
      // labelFilteringHint
      // 
      this.labelFilteringHint.AutoSize = true;
      this.labelFilteringHint.Location = new System.Drawing.Point(187, 155);
      this.labelFilteringHint.Name = "labelFilteringHint";
      this.labelFilteringHint.Size = new System.Drawing.Size(224, 13);
      this.labelFilteringHint.TabIndex = 7;
      this.labelFilteringHint.Text = "Filtering mode (Gaussian Quad recommended)";
      // 
      // checkBoxVMRWebStreams
      // 
      this.checkBoxVMRWebStreams.AutoSize = true;
      this.checkBoxVMRWebStreams.Checked = true;
      this.checkBoxVMRWebStreams.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxVMRWebStreams.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxVMRWebStreams.Location = new System.Drawing.Point(43, 61);
      this.checkBoxVMRWebStreams.Name = "checkBoxVMRWebStreams";
      this.checkBoxVMRWebStreams.Size = new System.Drawing.Size(284, 17);
      this.checkBoxVMRWebStreams.TabIndex = 6;
      this.checkBoxVMRWebStreams.Text = "Use for playback of web streams (enables OSD menus)";
      this.checkBoxVMRWebStreams.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(27, 67);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(0, 13);
      this.label1.TabIndex = 1;
      // 
      // FiltersVideoRenderer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "FiltersVideoRenderer";
      this.Size = new System.Drawing.Size(438, 409);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPCheckBox checkboxMpNonsquare;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPToolTip VMR9Tips;
    private MediaPortal.UserInterface.Controls.MPComboBox mpVMR9FilterMethod;
    private System.Windows.Forms.Label label1;
      private MediaPortal.UserInterface.Controls.MPCheckBox checkboxDXEclusive;
    private MediaPortal.UserInterface.Controls.MPLabel labelFilteringHint;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxVMRWebStreams;
      private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDecimateMask;
      private MediaPortal.UserInterface.Controls.MPLabel labelEVRHint;
      private System.Windows.Forms.RadioButton radioButtonEVR;
      private System.Windows.Forms.RadioButton radioButtonVMR9;
      private UserInterface.Controls.MPLabel mpMadLabel;
      private System.Windows.Forms.RadioButton radioButtonMadVR;
    private UserInterface.Controls.MPCheckBox UseEVRMadVRForTV;
    private UserInterface.Controls.MPCheckBox DisableLowLatencyMode;
    private UserInterface.Controls.MPCheckBox UseMadVideoRenderer3D;
  }
}
