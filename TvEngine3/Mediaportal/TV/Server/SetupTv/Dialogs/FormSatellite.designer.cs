using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormSatellite
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpButtonOK = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.mpButtonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.txtSatName = new System.Windows.Forms.TextBox();
      this.label47 = new System.Windows.Forms.Label();
      this.groupBoxGeneralSettings = new System.Windows.Forms.GroupBox();
      this.label3 = new System.Windows.Forms.Label();
      this.txtTransponderListUrl = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.txtPos = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.txtLocalTranspoderFile = new System.Windows.Forms.TextBox();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpButtonOK
      // 
      this.mpButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonOK.Location = new System.Drawing.Point(187, 158);
      this.mpButtonOK.Name = "mpButtonOK";
      this.mpButtonOK.Size = new System.Drawing.Size(75, 25);
      this.mpButtonOK.TabIndex = 1;
      this.mpButtonOK.Text = "OK";
      this.mpButtonOK.UseVisualStyleBackColor = true;
      this.mpButtonOK.Click += new System.EventHandler(this.mpButtonOK_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(268, 158);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 25);
      this.mpButtonCancel.TabIndex = 2;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.mpButtonCancel_Click);
      // 
      // txtSatName
      // 
      this.txtSatName.Location = new System.Drawing.Point(127, 19);
      this.txtSatName.Name = "txtSatName";
      this.txtSatName.Size = new System.Drawing.Size(123, 20);
      this.txtSatName.TabIndex = 120;
      // 
      // label47
      // 
      this.label47.AutoSize = true;
      this.label47.Location = new System.Drawing.Point(8, 22);
      this.label47.Name = "label47";
      this.label47.Size = new System.Drawing.Size(38, 13);
      this.label47.TabIndex = 121;
      this.label47.Text = "Name:";
      // 
      // groupBoxGeneralSettings
      // 
      this.groupBoxGeneralSettings.Controls.Add(this.label3);
      this.groupBoxGeneralSettings.Controls.Add(this.txtTransponderListUrl);
      this.groupBoxGeneralSettings.Controls.Add(this.label2);
      this.groupBoxGeneralSettings.Controls.Add(this.txtPos);
      this.groupBoxGeneralSettings.Controls.Add(this.label1);
      this.groupBoxGeneralSettings.Controls.Add(this.txtLocalTranspoderFile);
      this.groupBoxGeneralSettings.Controls.Add(this.label47);
      this.groupBoxGeneralSettings.Controls.Add(this.txtSatName);
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(12, 12);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(330, 136);
      this.groupBoxGeneralSettings.TabIndex = 122;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "Satellite Settings";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(8, 102);
      this.label3.Name = "label3";
      this.label3.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.label3.Size = new System.Drawing.Size(99, 13);
      this.label3.TabIndex = 129;
      this.label3.Text = "TransponderListUrl:";
      // 
      // txtTransponderListUrl
      // 
      this.txtTransponderListUrl.Location = new System.Drawing.Point(127, 99);
      this.txtTransponderListUrl.Name = "txtTransponderListUrl";
      this.txtTransponderListUrl.Size = new System.Drawing.Size(123, 20);
      this.txtTransponderListUrl.TabIndex = 128;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(8, 75);
      this.label2.Name = "label2";
      this.label2.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.label2.Size = new System.Drawing.Size(47, 13);
      this.label2.TabIndex = 127;
      this.label2.Text = "Position:";
      // 
      // txtPos
      // 
      this.txtPos.Location = new System.Drawing.Point(127, 72);
      this.txtPos.Name = "txtPos";
      this.txtPos.Size = new System.Drawing.Size(43, 20);
      this.txtPos.TabIndex = 126;
      this.txtPos.Text = "0";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(8, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(106, 13);
      this.label1.TabIndex = 123;
      this.label1.Text = "LocalTranspoderFile:";
      // 
      // txtLocalTranspoderFile
      // 
      this.txtLocalTranspoderFile.Location = new System.Drawing.Point(127, 45);
      this.txtLocalTranspoderFile.Name = "txtLocalTranspoderFile";
      this.txtLocalTranspoderFile.Size = new System.Drawing.Size(123, 20);
      this.txtLocalTranspoderFile.TabIndex = 122;
      // 
      // FormSatellite
      // 
      this.AcceptButton = this.mpButtonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(365, 214);
      this.Controls.Add(this.groupBoxGeneralSettings);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormSatellite";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit Satellite";
      this.Load += new System.EventHandler(this.FormTunerSatellite_Load);
      this.groupBoxGeneralSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MPButton mpButtonOK;
    private MPButton mpButtonCancel;
    private System.Windows.Forms.TextBox txtSatName;
    private System.Windows.Forms.Label label47;
    private System.Windows.Forms.GroupBox groupBoxGeneralSettings;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txtLocalTranspoderFile;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox txtPos;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox txtTransponderListUrl;
  }
}