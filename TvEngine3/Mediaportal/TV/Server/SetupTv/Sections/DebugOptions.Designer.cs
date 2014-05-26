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
      this.mpLabelGeneralWarning = new MPLabel();
      this.mpCheckBoxTsWriterDumpInputs = new MPCheckBox();
      this.mpCheckBoxTsMuxerDumpInputs = new MPCheckBox();
      this.mpLabelTsWriterDumpInputsWarning = new MPLabel();
      this.mpLabelTsMuxerDumpInputsWarning = new MPLabel();
      this.SuspendLayout();
      // 
      // mpLabelGeneralWarning
      // 
      this.mpLabelGeneralWarning.ForeColor = System.Drawing.Color.Red;
      this.mpLabelGeneralWarning.Location = new System.Drawing.Point(9, 6);
      this.mpLabelGeneralWarning.Name = "mpLabelGeneralWarning";
      this.mpLabelGeneralWarning.Size = new System.Drawing.Size(464, 39);
      this.mpLabelGeneralWarning.TabIndex = 0;
      this.mpLabelGeneralWarning.Text = "This section provides access to test and debug settings. Some are experimental an" +
          "d may cause unexpected problems. Please don\'t modify anything here unless you kn" +
          "ow what you are doing.";
      // 
      // mpCheckBoxTsWriterDumpInputs
      // 
      this.mpCheckBoxTsWriterDumpInputs.AutoSize = true;
      this.mpCheckBoxTsWriterDumpInputs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxTsWriterDumpInputs.Location = new System.Drawing.Point(12, 48);
      this.mpCheckBoxTsWriterDumpInputs.Name = "mpCheckBoxTsWriterDumpInputs";
      this.mpCheckBoxTsWriterDumpInputs.Size = new System.Drawing.Size(124, 17);
      this.mpCheckBoxTsWriterDumpInputs.TabIndex = 1;
      this.mpCheckBoxTsWriterDumpInputs.Text = "dump TsWriter inputs";
      this.mpCheckBoxTsWriterDumpInputs.UseVisualStyleBackColor = true;
      // 
      // mpCheckBoxTsMuxerDumpInputs
      // 
      this.mpCheckBoxTsMuxerDumpInputs.AutoSize = true;
      this.mpCheckBoxTsMuxerDumpInputs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxTsMuxerDumpInputs.Location = new System.Drawing.Point(12, 123);
      this.mpCheckBoxTsMuxerDumpInputs.Name = "mpCheckBoxTsMuxerDumpInputs";
      this.mpCheckBoxTsMuxerDumpInputs.Size = new System.Drawing.Size(125, 17);
      this.mpCheckBoxTsMuxerDumpInputs.TabIndex = 3;
      this.mpCheckBoxTsMuxerDumpInputs.Text = "dump TsMuxer inputs";
      this.mpCheckBoxTsMuxerDumpInputs.UseVisualStyleBackColor = true;
      // 
      // mpLabelTsWriterDumpInputsWarning
      // 
      this.mpLabelTsWriterDumpInputsWarning.Location = new System.Drawing.Point(9, 68);
      this.mpLabelTsWriterDumpInputsWarning.Name = "mpLabelTsWriterDumpInputsWarning";
      this.mpLabelTsWriterDumpInputsWarning.Size = new System.Drawing.Size(450, 41);
      this.mpLabelTsWriterDumpInputsWarning.TabIndex = 2;
      this.mpLabelTsWriterDumpInputsWarning.Text = resources.GetString("mpLabelTsWriterDumpInputsWarning.Text");
      // 
      // mpLabelTsMuxerDumpInputsWarning
      // 
      this.mpLabelTsMuxerDumpInputsWarning.Location = new System.Drawing.Point(9, 143);
      this.mpLabelTsMuxerDumpInputsWarning.Name = "mpLabelTsMuxerDumpInputsWarning";
      this.mpLabelTsMuxerDumpInputsWarning.Size = new System.Drawing.Size(450, 46);
      this.mpLabelTsMuxerDumpInputsWarning.TabIndex = 4;
      this.mpLabelTsMuxerDumpInputsWarning.Text = resources.GetString("mpLabelTsMuxerDumpInputsWarning.Text");
      // 
      // DebugOptions
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpLabelTsMuxerDumpInputsWarning);
      this.Controls.Add(this.mpLabelTsWriterDumpInputsWarning);
      this.Controls.Add(this.mpCheckBoxTsMuxerDumpInputs);
      this.Controls.Add(this.mpCheckBoxTsWriterDumpInputs);
      this.Controls.Add(this.mpLabelGeneralWarning);
      this.Name = "DebugOptions";
      this.Padding = new System.Windows.Forms.Padding(6);
      this.Size = new System.Drawing.Size(482, 419);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPLabel mpLabelGeneralWarning;
    private MPCheckBox mpCheckBoxTsWriterDumpInputs;
    private MPCheckBox mpCheckBoxTsMuxerDumpInputs;
    private MPLabel mpLabelTsWriterDumpInputsWarning;
    private MPLabel mpLabelTsMuxerDumpInputsWarning;
  }
}
