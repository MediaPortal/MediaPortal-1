namespace MediaPortal.Configuration.Sections
{
  partial class VMR9Config
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
      this.mpNonsquare = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxDecimateMask = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.labelFilteringHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxVMRWebStreams = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.DXEclusiveCheckbox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpVMR9FilterMethod = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.VMR9Tips = new MediaPortal.UserInterface.Controls.MPToolTip();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpNonsquare
      // 
      this.mpNonsquare.AutoSize = true;
      this.mpNonsquare.Checked = true;
      this.mpNonsquare.CheckState = System.Windows.Forms.CheckState.Checked;
      this.mpNonsquare.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpNonsquare.Location = new System.Drawing.Point(30, 94);
      this.mpNonsquare.Name = "mpNonsquare";
      this.mpNonsquare.Size = new System.Drawing.Size(303, 17);
      this.mpNonsquare.TabIndex = 0;
      this.mpNonsquare.Text = "Use nonsquare mixing for scaling (recommended for quality)";
      this.VMR9Tips.SetToolTip(this.mpNonsquare, "Using non-square mixing avoids unnecessary scaling operations in the VMR9");
      this.mpNonsquare.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.checkBoxDecimateMask);
      this.mpGroupBox1.Controls.Add(this.labelFilteringHint);
      this.mpGroupBox1.Controls.Add(this.checkBoxVMRWebStreams);
      this.mpGroupBox1.Controls.Add(this.DXEclusiveCheckbox);
      this.mpGroupBox1.Controls.Add(this.mpVMR9FilterMethod);
      this.mpGroupBox1.Controls.Add(this.label1);
      this.mpGroupBox1.Controls.Add(this.mpNonsquare);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(459, 180);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Video Mixing Renderer 9 - Advanced settings";
      // 
      // checkBoxDecimateMask
      // 
      this.checkBoxDecimateMask.AutoSize = true;
      this.checkBoxDecimateMask.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDecimateMask.Location = new System.Drawing.Point(30, 117);
      this.checkBoxDecimateMask.Name = "checkBoxDecimateMask";
      this.checkBoxDecimateMask.Size = new System.Drawing.Size(399, 17);
      this.checkBoxDecimateMask.TabIndex = 8;
      this.checkBoxDecimateMask.Text = "Use Decimate (enable only if TV/Video has >= double resolution of your screen)";
      this.checkBoxDecimateMask.UseVisualStyleBackColor = true;
      this.checkBoxDecimateMask.Visible = false;
      // 
      // labelFilteringHint
      // 
      this.labelFilteringHint.AutoSize = true;
      this.labelFilteringHint.Location = new System.Drawing.Point(174, 147);
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
      this.checkBoxVMRWebStreams.Location = new System.Drawing.Point(30, 36);
      this.checkBoxVMRWebStreams.Name = "checkBoxVMRWebStreams";
      this.checkBoxVMRWebStreams.Size = new System.Drawing.Size(317, 17);
      this.checkBoxVMRWebStreams.TabIndex = 6;
      this.checkBoxVMRWebStreams.Text = "Use VMR9 for playback of web streams (enables OSD menus)";
      this.checkBoxVMRWebStreams.UseVisualStyleBackColor = true;
      // 
      // DXEclusiveCheckbox
      // 
      this.DXEclusiveCheckbox.AutoSize = true;
      this.DXEclusiveCheckbox.Checked = true;
      this.DXEclusiveCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.DXEclusiveCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.DXEclusiveCheckbox.Location = new System.Drawing.Point(30, 71);
      this.DXEclusiveCheckbox.Name = "DXEclusiveCheckbox";
      this.DXEclusiveCheckbox.Size = new System.Drawing.Size(423, 17);
      this.DXEclusiveCheckbox.TabIndex = 5;
      this.DXEclusiveCheckbox.Text = "Use VMR9 exclusive mode (avoids tearing / MP stays on top during media playback)";
      this.VMR9Tips.SetToolTip(this.DXEclusiveCheckbox, "Prevents video \"tearing\" during playback");
      this.DXEclusiveCheckbox.UseVisualStyleBackColor = true;
      // 
      // mpVMR9FilterMethod
      // 
      this.mpVMR9FilterMethod.BorderColor = System.Drawing.Color.Empty;
      this.mpVMR9FilterMethod.FormattingEnabled = true;
      this.mpVMR9FilterMethod.Items.AddRange(new object[] {
            "None",
            "Point Filtering",
            "Bilinear Filtering",
            "Anisotropic Filtering",
            "Pyrimidal Quad Filtering",
            "Gaussian Quad Filtering"});
      this.mpVMR9FilterMethod.Location = new System.Drawing.Point(30, 144);
      this.mpVMR9FilterMethod.Name = "mpVMR9FilterMethod";
      this.mpVMR9FilterMethod.Size = new System.Drawing.Size(138, 21);
      this.mpVMR9FilterMethod.TabIndex = 2;
      this.mpVMR9FilterMethod.Text = "Gaussian Quad Filtering";
      this.VMR9Tips.SetToolTip(this.mpVMR9FilterMethod, "The filtering method determines the scaling algorithm used by the VMR9");
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(27, 67);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(0, 13);
      this.label1.TabIndex = 1;
      // 
      // VMR9Config
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "VMR9Config";
      this.Size = new System.Drawing.Size(459, 384);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPCheckBox mpNonsquare;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPToolTip VMR9Tips;
    private MediaPortal.UserInterface.Controls.MPComboBox mpVMR9FilterMethod;
    private System.Windows.Forms.Label label1;
    private MediaPortal.UserInterface.Controls.MPCheckBox DXEclusiveCheckbox;
    private MediaPortal.UserInterface.Controls.MPLabel labelFilteringHint;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxVMRWebStreams;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDecimateMask;
  }
}
