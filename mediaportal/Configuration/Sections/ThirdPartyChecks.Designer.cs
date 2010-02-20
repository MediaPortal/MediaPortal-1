namespace MediaPortal.Configuration.Sections
{
  partial class ThirdPartyChecks
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
      this.mpGroupBoxMCS = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButtonMCS = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabelStatusMCS = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelStatus1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBoxWarningMce = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabelWarningMce = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBoxMpTray = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCheckBoxStartMPTray = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBoxMCS.SuspendLayout();
      this.mpGroupBoxWarningMce.SuspendLayout();
      this.mpGroupBoxMpTray.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBoxMCS
      // 
      this.mpGroupBoxMCS.Controls.Add(this.mpButtonMCS);
      this.mpGroupBoxMCS.Controls.Add(this.mpLabelStatusMCS);
      this.mpGroupBoxMCS.Controls.Add(this.mpLabelStatus1);
      this.mpGroupBoxMCS.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxMCS.Location = new System.Drawing.Point(12, 11);
      this.mpGroupBoxMCS.Name = "mpGroupBoxMCS";
      this.mpGroupBoxMCS.Size = new System.Drawing.Size(445, 112);
      this.mpGroupBoxMCS.TabIndex = 0;
      this.mpGroupBoxMCS.TabStop = false;
      this.mpGroupBoxMCS.Text = "Microsoft Media Center Services";
      // 
      // mpButtonMCS
      // 
      this.mpButtonMCS.Location = new System.Drawing.Point(102, 69);
      this.mpButtonMCS.Name = "mpButtonMCS";
      this.mpButtonMCS.Size = new System.Drawing.Size(258, 23);
      this.mpButtonMCS.TabIndex = 2;
      this.mpButtonMCS.Text = "Enable policy to prevent services startup";
      this.mpButtonMCS.UseVisualStyleBackColor = true;
      this.mpButtonMCS.Click += new System.EventHandler(this.mpButtonMCS_Click);
      // 
      // mpLabelStatusMCS
      // 
      this.mpLabelStatusMCS.AutoSize = true;
      this.mpLabelStatusMCS.ForeColor = System.Drawing.Color.Red;
      this.mpLabelStatusMCS.Location = new System.Drawing.Point(142, 32);
      this.mpLabelStatusMCS.Name = "mpLabelStatusMCS";
      this.mpLabelStatusMCS.Size = new System.Drawing.Size(45, 13);
      this.mpLabelStatusMCS.TabIndex = 1;
      this.mpLabelStatusMCS.Text = "stopped";
      // 
      // mpLabelStatus1
      // 
      this.mpLabelStatus1.AutoSize = true;
      this.mpLabelStatus1.Location = new System.Drawing.Point(75, 32);
      this.mpLabelStatus1.Name = "mpLabelStatus1";
      this.mpLabelStatus1.Size = new System.Drawing.Size(40, 13);
      this.mpLabelStatus1.TabIndex = 0;
      this.mpLabelStatus1.Text = "Status:";
      // 
      // mpGroupBoxWarningMce
      // 
      this.mpGroupBoxWarningMce.Controls.Add(this.mpLabelWarningMce);
      this.mpGroupBoxWarningMce.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxWarningMce.Location = new System.Drawing.Point(12, 153);
      this.mpGroupBoxWarningMce.Name = "mpGroupBoxWarningMce";
      this.mpGroupBoxWarningMce.Size = new System.Drawing.Size(445, 112);
      this.mpGroupBoxWarningMce.TabIndex = 1;
      this.mpGroupBoxWarningMce.TabStop = false;
      this.mpGroupBoxWarningMce.Text = "Note for MCE remote users";
      // 
      // mpLabelWarningMce
      // 
      this.mpLabelWarningMce.AutoSize = true;
      this.mpLabelWarningMce.Location = new System.Drawing.Point(28, 43);
      this.mpLabelWarningMce.Name = "mpLabelWarningMce";
      this.mpLabelWarningMce.Size = new System.Drawing.Size(395, 39);
      this.mpLabelWarningMce.TabIndex = 0;
      this.mpLabelWarningMce.Text = "If you want to use full functionality of MCE remote, please enable the above poli" +
          "cy.\r\n\r\nYou will be able to start MP with \"The Green Button\" then.";
      // 
      // mpGroupBoxMpTray
      // 
      this.mpGroupBoxMpTray.Controls.Add(this.mpCheckBoxStartMPTray);
      this.mpGroupBoxMpTray.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxMpTray.Location = new System.Drawing.Point(12, 153);
      this.mpGroupBoxMpTray.Name = "mpGroupBoxMpTray";
      this.mpGroupBoxMpTray.Size = new System.Drawing.Size(445, 64);
      this.mpGroupBoxMpTray.TabIndex = 2;
      this.mpGroupBoxMpTray.TabStop = false;
      this.mpGroupBoxMpTray.Text = "MPTray";
      this.mpGroupBoxMpTray.Visible = false;
      // 
      // mpCheckBoxStartMPTray
      // 
      this.mpCheckBoxStartMPTray.AutoSize = true;
      this.mpCheckBoxStartMPTray.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxStartMPTray.Location = new System.Drawing.Point(11, 23);
      this.mpCheckBoxStartMPTray.Name = "mpCheckBoxStartMPTray";
      this.mpCheckBoxStartMPTray.Size = new System.Drawing.Size(349, 17);
      this.mpCheckBoxStartMPTray.TabIndex = 0;
      this.mpCheckBoxStartMPTray.Text = "Autostart MPTray to launch MediaPortal with the MediaCenter Button";
      this.mpCheckBoxStartMPTray.UseVisualStyleBackColor = true;
      this.mpCheckBoxStartMPTray.Click += new System.EventHandler(this.mpCheckBoxStartMPTray_CheckedChanged);
      // 
      // ThirdPartyChecks
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBoxMpTray);
      this.Controls.Add(this.mpGroupBoxWarningMce);
      this.Controls.Add(this.mpGroupBoxMCS);
      this.Name = "ThirdPartyChecks";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBoxMCS.ResumeLayout(false);
      this.mpGroupBoxMCS.PerformLayout();
      this.mpGroupBoxWarningMce.ResumeLayout(false);
      this.mpGroupBoxWarningMce.PerformLayout();
      this.mpGroupBoxMpTray.ResumeLayout(false);
      this.mpGroupBoxMpTray.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxMCS;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelStatus1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelStatusMCS;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonMCS;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxWarningMce;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelWarningMce;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxMpTray;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxStartMPTray;
  }
}