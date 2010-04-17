namespace MediaPortal.Configuration.Sections
{
  partial class TVZoom
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
      this.gAllowedModes = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbAllowNormal = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowZoom149 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowOriginal = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowZoom = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowLetterbox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowStretch = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowNonLinearStretch = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBoxZoomDefault = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.defaultZoomModeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.gAllowedModes.SuspendLayout();
      this.mpGroupBoxZoomDefault.SuspendLayout();
      this.SuspendLayout();
      // 
      // gAllowedModes
      // 
      this.gAllowedModes.Controls.Add(this.cbAllowNormal);
      this.gAllowedModes.Controls.Add(this.cbAllowZoom149);
      this.gAllowedModes.Controls.Add(this.cbAllowOriginal);
      this.gAllowedModes.Controls.Add(this.cbAllowZoom);
      this.gAllowedModes.Controls.Add(this.cbAllowLetterbox);
      this.gAllowedModes.Controls.Add(this.cbAllowStretch);
      this.gAllowedModes.Controls.Add(this.cbAllowNonLinearStretch);
      this.gAllowedModes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gAllowedModes.Location = new System.Drawing.Point(15, 12);
      this.gAllowedModes.Name = "gAllowedModes";
      this.gAllowedModes.Size = new System.Drawing.Size(186, 187);
      this.gAllowedModes.TabIndex = 2;
      this.gAllowedModes.TabStop = false;
      this.gAllowedModes.Text = "Allowed zoom modes";
      // 
      // cbAllowNormal
      // 
      this.cbAllowNormal.AutoSize = true;
      this.cbAllowNormal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowNormal.Location = new System.Drawing.Point(17, 24);
      this.cbAllowNormal.Name = "cbAllowNormal";
      this.cbAllowNormal.Size = new System.Drawing.Size(151, 17);
      this.cbAllowNormal.TabIndex = 0;
      this.cbAllowNormal.Text = "Normal (aspect auto mode)";
      this.cbAllowNormal.UseVisualStyleBackColor = true;
      // 
      // cbAllowZoom149
      // 
      this.cbAllowZoom149.AutoSize = true;
      this.cbAllowZoom149.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowZoom149.Location = new System.Drawing.Point(17, 93);
      this.cbAllowZoom149.Name = "cbAllowZoom149";
      this.cbAllowZoom149.Size = new System.Drawing.Size(73, 17);
      this.cbAllowZoom149.TabIndex = 3;
      this.cbAllowZoom149.Text = "14:9 zoom";
      this.cbAllowZoom149.UseVisualStyleBackColor = true;
      // 
      // cbAllowOriginal
      // 
      this.cbAllowOriginal.AutoSize = true;
      this.cbAllowOriginal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowOriginal.Location = new System.Drawing.Point(17, 47);
      this.cbAllowOriginal.Name = "cbAllowOriginal";
      this.cbAllowOriginal.Size = new System.Drawing.Size(126, 17);
      this.cbAllowOriginal.TabIndex = 1;
      this.cbAllowOriginal.Text = "Original source format";
      this.cbAllowOriginal.UseVisualStyleBackColor = true;
      // 
      // cbAllowZoom
      // 
      this.cbAllowZoom.AutoSize = true;
      this.cbAllowZoom.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowZoom.Location = new System.Drawing.Point(17, 70);
      this.cbAllowZoom.Name = "cbAllowZoom";
      this.cbAllowZoom.Size = new System.Drawing.Size(51, 17);
      this.cbAllowZoom.TabIndex = 2;
      this.cbAllowZoom.Text = "Zoom";
      this.cbAllowZoom.UseVisualStyleBackColor = true;
      // 
      // cbAllowLetterbox
      // 
      this.cbAllowLetterbox.AutoSize = true;
      this.cbAllowLetterbox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowLetterbox.Location = new System.Drawing.Point(17, 162);
      this.cbAllowLetterbox.Name = "cbAllowLetterbox";
      this.cbAllowLetterbox.Size = new System.Drawing.Size(86, 17);
      this.cbAllowLetterbox.TabIndex = 6;
      this.cbAllowLetterbox.Text = "4:3 Letterbox";
      this.cbAllowLetterbox.UseVisualStyleBackColor = true;
      // 
      // cbAllowStretch
      // 
      this.cbAllowStretch.AutoSize = true;
      this.cbAllowStretch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowStretch.Location = new System.Drawing.Point(17, 116);
      this.cbAllowStretch.Name = "cbAllowStretch";
      this.cbAllowStretch.Size = new System.Drawing.Size(107, 17);
      this.cbAllowStretch.TabIndex = 4;
      this.cbAllowStretch.Text = "Fullscreen stretch";
      this.cbAllowStretch.UseVisualStyleBackColor = true;
      // 
      // cbAllowNonLinearStretch
      // 
      this.cbAllowNonLinearStretch.AutoSize = true;
      this.cbAllowNonLinearStretch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowNonLinearStretch.Location = new System.Drawing.Point(17, 139);
      this.cbAllowNonLinearStretch.Name = "cbAllowNonLinearStretch";
      this.cbAllowNonLinearStretch.Size = new System.Drawing.Size(140, 17);
      this.cbAllowNonLinearStretch.TabIndex = 5;
      this.cbAllowNonLinearStretch.Text = "Non-linear stretch && crop";
      this.cbAllowNonLinearStretch.UseVisualStyleBackColor = true;
      // 
      // mpGroupBoxZoomDefault
      // 
      this.mpGroupBoxZoomDefault.Controls.Add(this.defaultZoomModeComboBox);
      this.mpGroupBoxZoomDefault.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxZoomDefault.Location = new System.Drawing.Point(207, 14);
      this.mpGroupBoxZoomDefault.Name = "mpGroupBoxZoomDefault";
      this.mpGroupBoxZoomDefault.Size = new System.Drawing.Size(262, 62);
      this.mpGroupBoxZoomDefault.TabIndex = 3;
      this.mpGroupBoxZoomDefault.TabStop = false;
      this.mpGroupBoxZoomDefault.Text = "Default mode";
      // 
      // defaultZoomModeComboBox
      // 
      this.defaultZoomModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultZoomModeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultZoomModeComboBox.Location = new System.Drawing.Point(18, 22);
      this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
      this.defaultZoomModeComboBox.Size = new System.Drawing.Size(226, 21);
      this.defaultZoomModeComboBox.TabIndex = 13;
      // 
      // TVZoom
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBoxZoomDefault);
      this.Controls.Add(this.gAllowedModes);
      this.Name = "TVZoom";
      this.Size = new System.Drawing.Size(472, 427);
      this.gAllowedModes.ResumeLayout(false);
      this.gAllowedModes.PerformLayout();
      this.mpGroupBoxZoomDefault.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox gAllowedModes;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowNormal;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowZoom149;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowOriginal;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowZoom;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowLetterbox;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowStretch;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowNonLinearStretch;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxZoomDefault;
    private MediaPortal.UserInterface.Controls.MPComboBox defaultZoomModeComboBox;
  }
}
