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
      this.labelGeneralWarning = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
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
      // DebugOptions
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.labelGeneralWarning);
      this.Name = "DebugOptions";
      this.Padding = new System.Windows.Forms.Padding(6);
      this.Size = new System.Drawing.Size(480, 420);
      this.ResumeLayout(false);

    }

    #endregion

    private MPLabel labelGeneralWarning;
  }
}
