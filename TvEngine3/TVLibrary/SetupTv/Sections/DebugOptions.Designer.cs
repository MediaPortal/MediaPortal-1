namespace SetupTv.Sections
{
  partial class DebugOptions
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

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
      this.mpWarningLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpResetGraphCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpUsePatLookupCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpDumpRawTSCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpFormToolTips = new MediaPortal.UserInterface.Controls.MPToolTip();
      this.SuspendLayout();
      // 
      // mpWarningLabel
      // 
      this.mpWarningLabel.ForeColor = System.Drawing.Color.Red;
      this.mpWarningLabel.Location = new System.Drawing.Point(9, 6);
      this.mpWarningLabel.Name = "mpWarningLabel";
      this.mpWarningLabel.Size = new System.Drawing.Size(464, 48);
      this.mpWarningLabel.TabIndex = 0;
      this.mpWarningLabel.Text = "This section provides special/debugging settings that are not supported by the Te" +
          "am. Some of these settings are experimental. Do not alter any of the settings be" +
          "low unless you know what you are doing.";
      // 
      // mpResetGraphCheckBox
      // 
      this.mpResetGraphCheckBox.AutoSize = true;
      this.mpResetGraphCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpResetGraphCheckBox.Location = new System.Drawing.Point(12, 57);
      this.mpResetGraphCheckBox.Name = "mpResetGraphCheckBox";
      this.mpResetGraphCheckBox.Size = new System.Drawing.Size(160, 17);
      this.mpResetGraphCheckBox.TabIndex = 1;
      this.mpResetGraphCheckBox.Text = "Always reset graph after stop";
      this.mpResetGraphCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpUsePatLookupCheckBox
      // 
      this.mpUsePatLookupCheckBox.AutoSize = true;
      this.mpUsePatLookupCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpUsePatLookupCheckBox.Location = new System.Drawing.Point(12, 81);
      this.mpUsePatLookupCheckBox.Name = "mpUsePatLookupCheckBox";
      this.mpUsePatLookupCheckBox.Size = new System.Drawing.Size(136, 17);
      this.mpUsePatLookupCheckBox.TabIndex = 2;
      this.mpUsePatLookupCheckBox.Text = "Always use PAT lookup";
      this.mpUsePatLookupCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpDumpRawTSCheckBox
      // 
      this.mpDumpRawTSCheckBox.AutoSize = true;
      this.mpDumpRawTSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpDumpRawTSCheckBox.Location = new System.Drawing.Point(12, 105);
      this.mpDumpRawTSCheckBox.Name = "mpDumpRawTSCheckBox";
      this.mpDumpRawTSCheckBox.Size = new System.Drawing.Size(175, 17);
      this.mpDumpRawTSCheckBox.TabIndex = 3;
      this.mpDumpRawTSCheckBox.Text = "Dump raw TS (use with caution)";
      this.mpFormToolTips.SetToolTip(this.mpDumpRawTSCheckBox, "When enabled the entire TS will be written to a file. This can easily create huge" +
              " files and use up your disk space fast. Use with care and only for testing/debug" +
              "ing.");
      this.mpDumpRawTSCheckBox.UseVisualStyleBackColor = true;
      // 
      // DebugOptions
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpDumpRawTSCheckBox);
      this.Controls.Add(this.mpUsePatLookupCheckBox);
      this.Controls.Add(this.mpResetGraphCheckBox);
      this.Controls.Add(this.mpWarningLabel);
      this.Name = "DebugOptions";
      this.Padding = new System.Windows.Forms.Padding(6);
      this.Size = new System.Drawing.Size(482, 419);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel mpWarningLabel;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpResetGraphCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpUsePatLookupCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpDumpRawTSCheckBox;
    private MediaPortal.UserInterface.Controls.MPToolTip mpFormToolTips;
  }
}
