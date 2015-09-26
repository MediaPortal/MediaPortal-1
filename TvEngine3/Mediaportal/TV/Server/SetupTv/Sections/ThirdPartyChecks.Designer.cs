using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
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
      this.groupBoxMcs = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonMcs = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelMcsStatusValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelMcsStatus = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxBdaHotFix = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.linkLabelBdaHotFix = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLinkLabel();
      this.labelBdaHotFixStatusValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelBdaHotFixStatus = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxMcs.SuspendLayout();
      this.groupBoxBdaHotFix.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxMcs
      // 
      this.groupBoxMcs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxMcs.Controls.Add(this.buttonMcs);
      this.groupBoxMcs.Controls.Add(this.labelMcsStatusValue);
      this.groupBoxMcs.Controls.Add(this.labelMcsStatus);
      this.groupBoxMcs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMcs.Location = new System.Drawing.Point(6, 6);
      this.groupBoxMcs.Name = "groupBoxMcs";
      this.groupBoxMcs.Size = new System.Drawing.Size(468, 87);
      this.groupBoxMcs.TabIndex = 0;
      this.groupBoxMcs.TabStop = false;
      this.groupBoxMcs.Text = "Microsoft Media Center Services";
      // 
      // buttonMcs
      // 
      this.buttonMcs.Location = new System.Drawing.Point(9, 46);
      this.buttonMcs.Name = "buttonMcs";
      this.buttonMcs.Size = new System.Drawing.Size(120, 23);
      this.buttonMcs.TabIndex = 2;
      this.buttonMcs.Text = "Re-enable Services";
      this.buttonMcs.UseVisualStyleBackColor = true;
      this.buttonMcs.Click += new System.EventHandler(this.buttonMcs_Click);
      // 
      // labelMcsStatusValue
      // 
      this.labelMcsStatusValue.AutoSize = true;
      this.labelMcsStatusValue.ForeColor = System.Drawing.Color.Red;
      this.labelMcsStatusValue.Location = new System.Drawing.Point(52, 21);
      this.labelMcsStatusValue.Name = "labelMcsStatusValue";
      this.labelMcsStatusValue.Size = new System.Drawing.Size(45, 13);
      this.labelMcsStatusValue.TabIndex = 1;
      this.labelMcsStatusValue.Text = "stopped";
      // 
      // labelMcsStatus
      // 
      this.labelMcsStatus.AutoSize = true;
      this.labelMcsStatus.Location = new System.Drawing.Point(6, 21);
      this.labelMcsStatus.Name = "labelMcsStatus";
      this.labelMcsStatus.Size = new System.Drawing.Size(40, 13);
      this.labelMcsStatus.TabIndex = 0;
      this.labelMcsStatus.Text = "Status:";
      // 
      // groupBoxBdaHotFix
      // 
      this.groupBoxBdaHotFix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxBdaHotFix.Controls.Add(this.linkLabelBdaHotFix);
      this.groupBoxBdaHotFix.Controls.Add(this.labelBdaHotFixStatusValue);
      this.groupBoxBdaHotFix.Controls.Add(this.labelBdaHotFixStatus);
      this.groupBoxBdaHotFix.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxBdaHotFix.Location = new System.Drawing.Point(6, 99);
      this.groupBoxBdaHotFix.Name = "groupBoxBdaHotFix";
      this.groupBoxBdaHotFix.Size = new System.Drawing.Size(468, 71);
      this.groupBoxBdaHotFix.TabIndex = 1;
      this.groupBoxBdaHotFix.TabStop = false;
      this.groupBoxBdaHotFix.Text = "Microsoft BDA Hot Fix";
      // 
      // linkLabelBdaHotFix
      // 
      this.linkLabelBdaHotFix.AutoSize = true;
      this.linkLabelBdaHotFix.Location = new System.Drawing.Point(6, 42);
      this.linkLabelBdaHotFix.Name = "linkLabelBdaHotFix";
      this.linkLabelBdaHotFix.Size = new System.Drawing.Size(87, 13);
      this.linkLabelBdaHotFix.TabIndex = 2;
      this.linkLabelBdaHotFix.TabStop = true;
      this.linkLabelBdaHotFix.Text = "Learn more here.";
      this.linkLabelBdaHotFix.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelBdaHotFix_LinkClicked);
      // 
      // labelBdaHotFixStatusValue
      // 
      this.labelBdaHotFixStatusValue.AutoSize = true;
      this.labelBdaHotFixStatusValue.ForeColor = System.Drawing.Color.Red;
      this.labelBdaHotFixStatusValue.Location = new System.Drawing.Point(52, 21);
      this.labelBdaHotFixStatusValue.Name = "labelBdaHotFixStatusValue";
      this.labelBdaHotFixStatusValue.Size = new System.Drawing.Size(61, 13);
      this.labelBdaHotFixStatusValue.TabIndex = 1;
      this.labelBdaHotFixStatusValue.Text = "not needed";
      // 
      // labelBdaHotFixStatus
      // 
      this.labelBdaHotFixStatus.AutoSize = true;
      this.labelBdaHotFixStatus.Location = new System.Drawing.Point(6, 21);
      this.labelBdaHotFixStatus.Name = "labelBdaHotFixStatus";
      this.labelBdaHotFixStatus.Size = new System.Drawing.Size(40, 13);
      this.labelBdaHotFixStatus.TabIndex = 0;
      this.labelBdaHotFixStatus.Text = "Status:";
      // 
      // ThirdPartyChecks
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.groupBoxBdaHotFix);
      this.Controls.Add(this.groupBoxMcs);
      this.Name = "ThirdPartyChecks";
      this.Size = new System.Drawing.Size(480, 420);
      this.groupBoxMcs.ResumeLayout(false);
      this.groupBoxMcs.PerformLayout();
      this.groupBoxBdaHotFix.ResumeLayout(false);
      this.groupBoxBdaHotFix.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MPGroupBox groupBoxMcs;
    private MPGroupBox groupBoxBdaHotFix;
    private MPLabel labelMcsStatus;
    private MPLabel labelBdaHotFixStatus;
    private MPLabel labelMcsStatusValue;
    private MPLabel labelBdaHotFixStatusValue;
    private MPButton buttonMcs;
    private MPLinkLabel linkLabelBdaHotFix;
  }
}
