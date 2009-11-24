namespace MediaPortal.Configuration.Sections
{
  partial class DVDZoom
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
      this.displayModeLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.displayModeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.defaultZoomModeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.aspectRatioComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.aspectRatioLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.pixelRatioCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.displayModeLabel);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.displayModeComboBox);
      this.mpGroupBox1.Controls.Add(this.defaultZoomModeComboBox);
      this.mpGroupBox1.Controls.Add(this.aspectRatioComboBox);
      this.mpGroupBox1.Controls.Add(this.aspectRatioLabel);
      this.mpGroupBox1.Controls.Add(this.pixelRatioCheckBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 3);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 138);
      this.mpGroupBox1.TabIndex = 2;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Aspect Ratio";
      // 
      // displayModeLabel
      // 
      this.displayModeLabel.AutoSize = true;
      this.displayModeLabel.Location = new System.Drawing.Point(16, 74);
      this.displayModeLabel.Name = "displayModeLabel";
      this.displayModeLabel.Size = new System.Drawing.Size(73, 13);
      this.displayModeLabel.TabIndex = 3;
      this.displayModeLabel.Text = "Display mode:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(16, 101);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(112, 16);
      this.mpLabel1.TabIndex = 2;
      this.mpLabel1.Text = "Default zoom mode:";
      // 
      // displayModeComboBox
      // 
      this.displayModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.displayModeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.displayModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.displayModeComboBox.Items.AddRange(new object[] {
            "Default",
            "16:9",
            "4:3 Pan Scan",
            "4:3 Letterbox"});
      this.displayModeComboBox.Location = new System.Drawing.Point(168, 71);
      this.displayModeComboBox.Name = "displayModeComboBox";
      this.displayModeComboBox.Size = new System.Drawing.Size(288, 21);
      this.displayModeComboBox.TabIndex = 4;
      // 
      // defaultZoomModeComboBox
      // 
      this.defaultZoomModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultZoomModeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultZoomModeComboBox.Location = new System.Drawing.Point(168, 98);
      this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
      this.defaultZoomModeComboBox.Size = new System.Drawing.Size(288, 21);
      this.defaultZoomModeComboBox.TabIndex = 3;
      // 
      // aspectRatioComboBox
      // 
      this.aspectRatioComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.aspectRatioComboBox.BorderColor = System.Drawing.Color.Empty;
      this.aspectRatioComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.aspectRatioComboBox.Items.AddRange(new object[] {
            "Crop",
            "Letterbox",
            "Stretch",
            "Follow stream"});
      this.aspectRatioComboBox.Location = new System.Drawing.Point(168, 44);
      this.aspectRatioComboBox.Name = "aspectRatioComboBox";
      this.aspectRatioComboBox.Size = new System.Drawing.Size(288, 21);
      this.aspectRatioComboBox.TabIndex = 2;
      // 
      // aspectRatioLabel
      // 
      this.aspectRatioLabel.AutoSize = true;
      this.aspectRatioLabel.Location = new System.Drawing.Point(16, 47);
      this.aspectRatioLabel.Name = "aspectRatioLabel";
      this.aspectRatioLabel.Size = new System.Drawing.Size(145, 13);
      this.aspectRatioLabel.TabIndex = 1;
      this.aspectRatioLabel.Text = "Aspect ratio correction mode:";
      // 
      // pixelRatioCheckBox
      // 
      this.pixelRatioCheckBox.AutoSize = true;
      this.pixelRatioCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.pixelRatioCheckBox.Location = new System.Drawing.Point(168, 20);
      this.pixelRatioCheckBox.Name = "pixelRatioCheckBox";
      this.pixelRatioCheckBox.Size = new System.Drawing.Size(140, 17);
      this.pixelRatioCheckBox.TabIndex = 0;
      this.pixelRatioCheckBox.Text = "Use pixel ratio correction";
      this.pixelRatioCheckBox.UseVisualStyleBackColor = true;
      // 
      // DVDZoom
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "DVDZoom";
      this.Size = new System.Drawing.Size(472, 391);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel displayModeLabel;
    private MediaPortal.UserInterface.Controls.MPComboBox displayModeComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox aspectRatioComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel aspectRatioLabel;
    private MediaPortal.UserInterface.Controls.MPCheckBox pixelRatioCheckBox;
    private MediaPortal.UserInterface.Controls.MPComboBox defaultZoomModeComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
  }
}
