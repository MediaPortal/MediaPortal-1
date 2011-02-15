#pragma warning disable 108

namespace SetupTv.Sections
{
  public partial class InfoPage : SetupTv.SectionSettings
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
      this.groupBoxInfo = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lblInfoText = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxInfo.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxInfo
      // 
      this.groupBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxInfo.Controls.Add(this.lblInfoText);
      this.groupBoxInfo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxInfo.Location = new System.Drawing.Point(0, 0);
      this.groupBoxInfo.Name = "groupBoxInfo";
      this.groupBoxInfo.Size = new System.Drawing.Size(472, 405);
      this.groupBoxInfo.TabIndex = 0;
      this.groupBoxInfo.TabStop = false;
      // 
      // lblInfoText
      // 
      this.lblInfoText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblInfoText.Location = new System.Drawing.Point(16, 24);
      this.lblInfoText.Name = "lblInfoText";
      this.lblInfoText.Size = new System.Drawing.Size(440, 367);
      this.lblInfoText.TabIndex = 0;
      this.lblInfoText.Text = "No information available";
      // 
      // Project
      // 
      this.Controls.Add(this.groupBoxInfo);
      this.Name = "Project";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxInfo.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxInfo;
    private MediaPortal.UserInterface.Controls.MPLabel lblInfoText;
  }
}
