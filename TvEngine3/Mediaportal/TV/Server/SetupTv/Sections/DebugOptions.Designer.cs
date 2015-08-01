using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugOptions));
      this.labelGeneralWarning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxTsWriterDumpInputs = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.checkBoxTsMuxerDumpInputs = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelTsWriterDumpInputsWarning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelTsMuxerDumpInputsWarning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.checkBoxTsWriterDisableCrcCheck = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.labelTsWriterDisableCrcCheckWarning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.SuspendLayout();
      // 
      // labelGeneralWarning
      // 
      this.labelGeneralWarning.ForeColor = System.Drawing.Color.Red;
      this.labelGeneralWarning.Location = new System.Drawing.Point(9, 6);
      this.labelGeneralWarning.Name = "labelGeneralWarning";
      this.labelGeneralWarning.Size = new System.Drawing.Size(464, 39);
      this.labelGeneralWarning.TabIndex = 0;
      this.labelGeneralWarning.Text = "This section provides access to test and debug settings. Some are experimental an" +
          "d may cause unexpected problems. Please don\'t modify anything here unless you kn" +
          "ow what you are doing.";
      // 
      // checkBoxTsWriterDumpInputs
      // 
      this.checkBoxTsWriterDumpInputs.AutoSize = true;
      this.checkBoxTsWriterDumpInputs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxTsWriterDumpInputs.Location = new System.Drawing.Point(12, 48);
      this.checkBoxTsWriterDumpInputs.Name = "checkBoxTsWriterDumpInputs";
      this.checkBoxTsWriterDumpInputs.Size = new System.Drawing.Size(129, 17);
      this.checkBoxTsWriterDumpInputs.TabIndex = 1;
      this.checkBoxTsWriterDumpInputs.Text = "Dump TsWriter inputs.";
      this.checkBoxTsWriterDumpInputs.UseVisualStyleBackColor = true;
      // 
      // checkBoxTsMuxerDumpInputs
      // 
      this.checkBoxTsMuxerDumpInputs.AutoSize = true;
      this.checkBoxTsMuxerDumpInputs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxTsMuxerDumpInputs.Location = new System.Drawing.Point(12, 123);
      this.checkBoxTsMuxerDumpInputs.Name = "checkBoxTsMuxerDumpInputs";
      this.checkBoxTsMuxerDumpInputs.Size = new System.Drawing.Size(130, 17);
      this.checkBoxTsMuxerDumpInputs.TabIndex = 3;
      this.checkBoxTsMuxerDumpInputs.Text = "Dump TsMuxer inputs.";
      this.checkBoxTsMuxerDumpInputs.UseVisualStyleBackColor = true;
      // 
      // labelTsWriterDumpInputsWarning
      // 
      this.labelTsWriterDumpInputsWarning.Location = new System.Drawing.Point(9, 68);
      this.labelTsWriterDumpInputsWarning.Name = "labelTsWriterDumpInputsWarning";
      this.labelTsWriterDumpInputsWarning.Size = new System.Drawing.Size(450, 41);
      this.labelTsWriterDumpInputsWarning.TabIndex = 2;
      this.labelTsWriterDumpInputsWarning.Text = resources.GetString("labelTsWriterDumpInputsWarning.Text");
      // 
      // labelTsMuxerDumpInputsWarning
      // 
      this.labelTsMuxerDumpInputsWarning.Location = new System.Drawing.Point(9, 143);
      this.labelTsMuxerDumpInputsWarning.Name = "labelTsMuxerDumpInputsWarning";
      this.labelTsMuxerDumpInputsWarning.Size = new System.Drawing.Size(450, 46);
      this.labelTsMuxerDumpInputsWarning.TabIndex = 4;
      this.labelTsMuxerDumpInputsWarning.Text = resources.GetString("labelTsMuxerDumpInputsWarning.Text");
      // 
      // checkBoxTsWriterDisableCrcCheck
      // 
      this.checkBoxTsWriterDisableCrcCheck.AutoSize = true;
      this.checkBoxTsWriterDisableCrcCheck.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxTsWriterDisableCrcCheck.Location = new System.Drawing.Point(12, 201);
      this.checkBoxTsWriterDisableCrcCheck.Name = "checkBoxTsWriterDisableCrcCheck";
      this.checkBoxTsWriterDisableCrcCheck.Size = new System.Drawing.Size(177, 17);
      this.checkBoxTsWriterDisableCrcCheck.TabIndex = 5;
      this.checkBoxTsWriterDisableCrcCheck.Text = "Disable TsWriter CRC checking.";
      this.checkBoxTsWriterDisableCrcCheck.UseVisualStyleBackColor = true;
      // 
      // labelTsWriterDisableCrcCheckWarning
      // 
      this.labelTsWriterDisableCrcCheckWarning.Location = new System.Drawing.Point(9, 221);
      this.labelTsWriterDisableCrcCheckWarning.Name = "labelTsWriterDisableCrcCheckWarning";
      this.labelTsWriterDisableCrcCheckWarning.Size = new System.Drawing.Size(450, 57);
      this.labelTsWriterDisableCrcCheckWarning.TabIndex = 6;
      this.labelTsWriterDisableCrcCheckWarning.Text = resources.GetString("labelTsWriterDisableCrcCheckWarning.Text");
      // 
      // DebugOptions
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.labelTsWriterDisableCrcCheckWarning);
      this.Controls.Add(this.checkBoxTsWriterDisableCrcCheck);
      this.Controls.Add(this.labelTsMuxerDumpInputsWarning);
      this.Controls.Add(this.labelTsWriterDumpInputsWarning);
      this.Controls.Add(this.checkBoxTsMuxerDumpInputs);
      this.Controls.Add(this.checkBoxTsWriterDumpInputs);
      this.Controls.Add(this.labelGeneralWarning);
      this.Name = "DebugOptions";
      this.Padding = new System.Windows.Forms.Padding(6);
      this.Size = new System.Drawing.Size(480, 420);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPLabel labelGeneralWarning;
    private MPCheckBox checkBoxTsWriterDumpInputs;
    private MPCheckBox checkBoxTsMuxerDumpInputs;
    private MPLabel labelTsWriterDumpInputsWarning;
    private MPLabel labelTsMuxerDumpInputsWarning;
    private MPCheckBox checkBoxTsWriterDisableCrcCheck;
    private MPLabel labelTsWriterDisableCrcCheckWarning;
  }
}
