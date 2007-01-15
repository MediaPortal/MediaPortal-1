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
      this.DXEclusiveCheckbox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label3 = new System.Windows.Forms.Label();
      this.mpVMR9FilterMethod = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.VMR9Tips = new MediaPortal.UserInterface.Controls.MPToolTip();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpNonsquare
      // 
      this.mpNonsquare.AutoSize = true;
      this.mpNonsquare.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpNonsquare.Location = new System.Drawing.Point(30, 36);
      this.mpNonsquare.Name = "mpNonsquare";
      this.mpNonsquare.Size = new System.Drawing.Size(128, 17);
      this.mpNonsquare.TabIndex = 0;
      this.mpNonsquare.Text = "Use nonsquare mixing";
      this.VMR9Tips.SetToolTip(this.mpNonsquare, "Using non-square mixing avoids unnecessary scaling operations in the VMR9");
      this.mpNonsquare.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.DXEclusiveCheckbox);
      this.mpGroupBox1.Controls.Add(this.label3);
      this.mpGroupBox1.Controls.Add(this.mpVMR9FilterMethod);
      this.mpGroupBox1.Controls.Add(this.label1);
      this.mpGroupBox1.Controls.Add(this.mpNonsquare);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(13, 16);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(429, 349);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Video Mixing Renderer 9 Advanced Settings";
      // 
      // DXEclusiveCheckbox
      // 
      this.DXEclusiveCheckbox.AutoSize = true;
      this.DXEclusiveCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.DXEclusiveCheckbox.Location = new System.Drawing.Point(30, 92);
      this.DXEclusiveCheckbox.Name = "DXEclusiveCheckbox";
      this.DXEclusiveCheckbox.Size = new System.Drawing.Size(157, 17);
      this.DXEclusiveCheckbox.TabIndex = 5;
      this.DXEclusiveCheckbox.Text = "Use VMR 9 Exclusive Mode";
      this.VMR9Tips.SetToolTip(this.DXEclusiveCheckbox, "Prevents video \"tearing\" during playback");
      this.DXEclusiveCheckbox.UseVisualStyleBackColor = true;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(27, 138);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(106, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "VMR9 Filtering Mode";
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
      this.mpVMR9FilterMethod.Location = new System.Drawing.Point(30, 154);
      this.mpVMR9FilterMethod.Name = "mpVMR9FilterMethod";
      this.mpVMR9FilterMethod.Size = new System.Drawing.Size(203, 21);
      this.mpVMR9FilterMethod.TabIndex = 2;
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
    private System.Windows.Forms.Label label3;
    private MediaPortal.UserInterface.Controls.MPCheckBox DXEclusiveCheckbox;
  }
}
