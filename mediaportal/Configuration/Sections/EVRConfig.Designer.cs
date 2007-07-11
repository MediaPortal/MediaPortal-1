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
        this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.EVRCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.VMR9Tips = new MediaPortal.UserInterface.Controls.MPToolTip();
        this.mpGroupBox1.SuspendLayout();
        this.SuspendLayout();
        // 
        // mpGroupBox1
        // 
        this.mpGroupBox1.Controls.Add(this.EVRCheckBox);
        this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpGroupBox1.Location = new System.Drawing.Point(13, 16);
        this.mpGroupBox1.Name = "mpGroupBox1";
        this.mpGroupBox1.Size = new System.Drawing.Size(429, 349);
        this.mpGroupBox1.TabIndex = 1;
        this.mpGroupBox1.TabStop = false;
        this.mpGroupBox1.Text = "Enhanced Video Renderer Settings";
        // 
        // EVRCheckBox
        // 
        this.EVRCheckBox.AutoSize = true;
        this.EVRCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.EVRCheckBox.Location = new System.Drawing.Point(31, 38);
        this.EVRCheckBox.Name = "EVRCheckBox";
        this.EVRCheckBox.Size = new System.Drawing.Size(251, 17);
        this.EVRCheckBox.TabIndex = 6;
        this.EVRCheckBox.Text = "Use Enhanced Video Renderer (Windows Vista)";
        this.VMR9Tips.SetToolTip(this.EVRCheckBox, "Provides hardware-accelerated playback for certain video types under Windows Vist" +
                "a, if your graphics card supports it.");
        this.EVRCheckBox.UseVisualStyleBackColor = true;
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

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
      private MediaPortal.UserInterface.Controls.MPToolTip VMR9Tips;
      private MediaPortal.UserInterface.Controls.MPCheckBox EVRCheckBox;
  }
}
