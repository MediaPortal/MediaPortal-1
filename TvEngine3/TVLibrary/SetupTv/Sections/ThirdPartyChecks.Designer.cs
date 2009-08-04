namespace SetupTv.Sections
{
  partial class ThirdPartyChecks
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
      this.mpGroupBoxMCS = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButtonMCS = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabelStatusMCS = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelStatus1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabelDVBHotfix = new System.Windows.Forms.LinkLabel();
      this.mpLabelStatusDVBHotfix = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelStatus2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBoxStreamingPort = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabelStreamingPort = new System.Windows.Forms.LinkLabel();
      this.mpLabelStatusStreamingPort = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelStatus3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBoxMCS.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBoxStreamingPort.SuspendLayout();
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
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.linkLabelDVBHotfix);
      this.mpGroupBox2.Controls.Add(this.mpLabelStatusDVBHotfix);
      this.mpGroupBox2.Controls.Add(this.mpLabelStatus2);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(12, 145);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(445, 112);
      this.mpGroupBox2.TabIndex = 1;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Microsoft DVB hotfix";
      // 
      // linkLabelDVBHotfix
      // 
      this.linkLabelDVBHotfix.AutoSize = true;
      this.linkLabelDVBHotfix.Location = new System.Drawing.Point(113, 79);
      this.linkLabelDVBHotfix.Name = "linkLabelDVBHotfix";
      this.linkLabelDVBHotfix.Size = new System.Drawing.Size(229, 13);
      this.linkLabelDVBHotfix.TabIndex = 3;
      this.linkLabelDVBHotfix.TabStop = true;
      this.linkLabelDVBHotfix.Text = "How to download and install ? Learn more here";
      this.linkLabelDVBHotfix.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDVBHotfix_LinkClicked);
      // 
      // mpLabelStatusDVBHotfix
      // 
      this.mpLabelStatusDVBHotfix.AutoSize = true;
      this.mpLabelStatusDVBHotfix.ForeColor = System.Drawing.Color.Red;
      this.mpLabelStatusDVBHotfix.Location = new System.Drawing.Point(142, 43);
      this.mpLabelStatusDVBHotfix.Name = "mpLabelStatusDVBHotfix";
      this.mpLabelStatusDVBHotfix.Size = new System.Drawing.Size(138, 13);
      this.mpLabelStatusDVBHotfix.TabIndex = 2;
      this.mpLabelStatusDVBHotfix.Text = "not needed on Vista and up";
      // 
      // mpLabelStatus2
      // 
      this.mpLabelStatus2.AutoSize = true;
      this.mpLabelStatus2.Location = new System.Drawing.Point(75, 43);
      this.mpLabelStatus2.Name = "mpLabelStatus2";
      this.mpLabelStatus2.Size = new System.Drawing.Size(40, 13);
      this.mpLabelStatus2.TabIndex = 1;
      this.mpLabelStatus2.Text = "Status:";
      // 
      // mpGroupBoxStreamingPort
      // 
      this.mpGroupBoxStreamingPort.Controls.Add(this.linkLabelStreamingPort);
      this.mpGroupBoxStreamingPort.Controls.Add(this.mpLabelStatusStreamingPort);
      this.mpGroupBoxStreamingPort.Controls.Add(this.mpLabelStatus3);
      this.mpGroupBoxStreamingPort.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxStreamingPort.Location = new System.Drawing.Point(13, 280);
      this.mpGroupBoxStreamingPort.Name = "mpGroupBoxStreamingPort";
      this.mpGroupBoxStreamingPort.Size = new System.Drawing.Size(445, 112);
      this.mpGroupBoxStreamingPort.TabIndex = 2;
      this.mpGroupBoxStreamingPort.TabStop = false;
      this.mpGroupBoxStreamingPort.Text = "Streaming port";
      // 
      // linkLabelStreamingPort
      // 
      this.linkLabelStreamingPort.AutoSize = true;
      this.linkLabelStreamingPort.Location = new System.Drawing.Point(74, 78);
      this.linkLabelStreamingPort.Name = "linkLabelStreamingPort";
      this.linkLabelStreamingPort.Size = new System.Drawing.Size(298, 13);
      this.linkLabelStreamingPort.TabIndex = 4;
      this.linkLabelStreamingPort.TabStop = true;
      this.linkLabelStreamingPort.Text = "Please check with TCPVIEW which program is using that port";
      this.linkLabelStreamingPort.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelStreamingPort_LinkClicked);
      // 
      // mpLabelStatusStreamingPort
      // 
      this.mpLabelStatusStreamingPort.AutoSize = true;
      this.mpLabelStatusStreamingPort.ForeColor = System.Drawing.Color.Red;
      this.mpLabelStatusStreamingPort.Location = new System.Drawing.Point(141, 39);
      this.mpLabelStatusStreamingPort.Name = "mpLabelStatusStreamingPort";
      this.mpLabelStatusStreamingPort.Size = new System.Drawing.Size(126, 13);
      this.mpLabelStatusStreamingPort.TabIndex = 3;
      this.mpLabelStatusStreamingPort.Text = "port 554 is already bound";
      // 
      // mpLabelStatus3
      // 
      this.mpLabelStatus3.AutoSize = true;
      this.mpLabelStatus3.Location = new System.Drawing.Point(74, 39);
      this.mpLabelStatus3.Name = "mpLabelStatus3";
      this.mpLabelStatus3.Size = new System.Drawing.Size(40, 13);
      this.mpLabelStatus3.TabIndex = 2;
      this.mpLabelStatus3.Text = "Status:";
      // 
      // ThirdPartyChecks
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBoxStreamingPort);
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBoxMCS);
      this.Name = "ThirdPartyChecks";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBoxMCS.ResumeLayout(false);
      this.mpGroupBoxMCS.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBoxStreamingPort.ResumeLayout(false);
      this.mpGroupBoxStreamingPort.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxMCS;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelStatus1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelStatus2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelStatusMCS;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelStatusDVBHotfix;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonMCS;
    private System.Windows.Forms.LinkLabel linkLabelDVBHotfix;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxStreamingPort;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelStatus3;
    private System.Windows.Forms.LinkLabel linkLabelStreamingPort;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelStatusStreamingPort;
  }
}
