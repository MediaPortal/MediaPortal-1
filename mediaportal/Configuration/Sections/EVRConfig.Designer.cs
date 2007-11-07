namespace MediaPortal.Configuration.Sections
{
  partial class EVRConfig
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer evrcomponents = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (evrcomponents != null))
      {
        evrcomponents.Dispose();
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EVRConfig));
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelEVRHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.EVRCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.VMR9Tips = new MediaPortal.UserInterface.Controls.MPToolTip();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.labelEVRHint);
      this.mpGroupBox1.Controls.Add(this.EVRCheckBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(459, 194);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Enhanced Video Renderer Settings";
      // 
      // labelEVRHint
      // 
      this.labelEVRHint.AutoSize = true;
      this.labelEVRHint.Location = new System.Drawing.Point(46, 67);
      this.labelEVRHint.Name = "labelEVRHint";
      this.labelEVRHint.Size = new System.Drawing.Size(263, 78);
      this.labelEVRHint.TabIndex = 7;
      this.labelEVRHint.Text = resources.GetString("labelEVRHint.Text");
      // 
      // EVRCheckBox
      // 
      this.EVRCheckBox.AutoSize = true;
      this.EVRCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.EVRCheckBox.Location = new System.Drawing.Point(30, 36);
      this.EVRCheckBox.Name = "EVRCheckBox";
      this.EVRCheckBox.Size = new System.Drawing.Size(172, 17);
      this.EVRCheckBox.TabIndex = 6;
      this.EVRCheckBox.Text = "Use Enhanced Video Renderer";
      this.VMR9Tips.SetToolTip(this.EVRCheckBox, "Provides hardware-accelerated playback for certain video types under Windows Vist" +
              "a, if your graphics card supports it.");
      this.EVRCheckBox.UseVisualStyleBackColor = true;
      // 
      // EVRConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "EVRConfig";
      this.Size = new System.Drawing.Size(459, 384);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
      private MediaPortal.UserInterface.Controls.MPToolTip VMR9Tips;
      private MediaPortal.UserInterface.Controls.MPCheckBox EVRCheckBox;
    private MediaPortal.UserInterface.Controls.MPLabel labelEVRHint;
  }
}
