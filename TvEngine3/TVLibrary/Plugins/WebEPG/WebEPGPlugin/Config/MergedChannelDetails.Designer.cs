namespace SetupTv.Sections.WebEPGConfig
{
  partial class MergedChannelDetails
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
      this.gbGrabber = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tbGrabDays = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lGuideDays = new MediaPortal.UserInterface.Controls.MPLabel();
      this.bGrabber = new MediaPortal.UserInterface.Controls.MPButton();
      this.Grabber = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbGrabSite = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bChannelID = new MediaPortal.UserInterface.Controls.MPButton();
      this.l_cID = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbChannelName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bOk = new System.Windows.Forms.Button();
      this.bCancel = new System.Windows.Forms.Button();
      this.gbMergeTime = new System.Windows.Forms.GroupBox();
      this.tbEnd = new System.Windows.Forms.MaskedTextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.tbStart = new System.Windows.Forms.MaskedTextBox();
      this.lStart = new System.Windows.Forms.Label();
      this.gbGrabber.SuspendLayout();
      this.gbMergeTime.SuspendLayout();
      this.SuspendLayout();
      // 
      // gbGrabber
      // 
      this.gbGrabber.Controls.Add(this.tbGrabDays);
      this.gbGrabber.Controls.Add(this.lGuideDays);
      this.gbGrabber.Controls.Add(this.bGrabber);
      this.gbGrabber.Controls.Add(this.Grabber);
      this.gbGrabber.Controls.Add(this.tbGrabSite);
      this.gbGrabber.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbGrabber.Location = new System.Drawing.Point(6, 32);
      this.gbGrabber.Name = "gbGrabber";
      this.gbGrabber.Size = new System.Drawing.Size(298, 65);
      this.gbGrabber.TabIndex = 19;
      this.gbGrabber.TabStop = false;
      this.gbGrabber.Text = "Grabber Details";
      // 
      // tbGrabDays
      // 
      this.tbGrabDays.Location = new System.Drawing.Point(68, 39);
      this.tbGrabDays.Name = "tbGrabDays";
      this.tbGrabDays.ReadOnly = true;
      this.tbGrabDays.Size = new System.Drawing.Size(116, 20);
      this.tbGrabDays.TabIndex = 7;
      // 
      // lGuideDays
      // 
      this.lGuideDays.Location = new System.Drawing.Point(3, 42);
      this.lGuideDays.Name = "lGuideDays";
      this.lGuideDays.Size = new System.Drawing.Size(71, 17);
      this.lGuideDays.TabIndex = 8;
      this.lGuideDays.Text = "Guide Days";
      // 
      // bGrabber
      // 
      this.bGrabber.Location = new System.Drawing.Point(262, 12);
      this.bGrabber.Name = "bGrabber";
      this.bGrabber.Size = new System.Drawing.Size(22, 20);
      this.bGrabber.TabIndex = 6;
      this.bGrabber.Text = "...";
      this.bGrabber.UseVisualStyleBackColor = true;
      this.bGrabber.Visible = false;
      this.bGrabber.Click += new System.EventHandler(this.bGrabber_Click);
      // 
      // Grabber
      // 
      this.Grabber.Location = new System.Drawing.Point(3, 16);
      this.Grabber.Name = "Grabber";
      this.Grabber.Size = new System.Drawing.Size(56, 23);
      this.Grabber.TabIndex = 1;
      this.Grabber.Text = "Site";
      // 
      // tbGrabSite
      // 
      this.tbGrabSite.Location = new System.Drawing.Point(68, 13);
      this.tbGrabSite.Name = "tbGrabSite";
      this.tbGrabSite.ReadOnly = true;
      this.tbGrabSite.Size = new System.Drawing.Size(188, 20);
      this.tbGrabSite.TabIndex = 0;
      // 
      // bChannelID
      // 
      this.bChannelID.Location = new System.Drawing.Point(268, 6);
      this.bChannelID.Name = "bChannelID";
      this.bChannelID.Size = new System.Drawing.Size(22, 20);
      this.bChannelID.TabIndex = 18;
      this.bChannelID.Text = "...";
      this.bChannelID.UseVisualStyleBackColor = true;
      this.bChannelID.Click += new System.EventHandler(this.bChannelID_Click);
      // 
      // l_cID
      // 
      this.l_cID.Location = new System.Drawing.Point(12, 9);
      this.l_cID.Name = "l_cID";
      this.l_cID.Size = new System.Drawing.Size(56, 20);
      this.l_cID.TabIndex = 17;
      this.l_cID.Text = "Channel";
      // 
      // tbChannelName
      // 
      this.tbChannelName.Location = new System.Drawing.Point(73, 6);
      this.tbChannelName.Name = "tbChannelName";
      this.tbChannelName.ReadOnly = true;
      this.tbChannelName.Size = new System.Drawing.Size(189, 20);
      this.tbChannelName.TabIndex = 16;
      // 
      // bOk
      // 
      this.bOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.bOk.Location = new System.Drawing.Point(148, 169);
      this.bOk.Name = "bOk";
      this.bOk.Size = new System.Drawing.Size(75, 23);
      this.bOk.TabIndex = 20;
      this.bOk.Text = "Ok";
      this.bOk.UseVisualStyleBackColor = true;
      // 
      // bCancel
      // 
      this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.bCancel.Location = new System.Drawing.Point(229, 169);
      this.bCancel.Name = "bCancel";
      this.bCancel.Size = new System.Drawing.Size(75, 23);
      this.bCancel.TabIndex = 21;
      this.bCancel.Text = "Cancel";
      this.bCancel.UseVisualStyleBackColor = true;
      this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
      // 
      // gbMergeTime
      // 
      this.gbMergeTime.Controls.Add(this.tbEnd);
      this.gbMergeTime.Controls.Add(this.label1);
      this.gbMergeTime.Controls.Add(this.tbStart);
      this.gbMergeTime.Controls.Add(this.lStart);
      this.gbMergeTime.Location = new System.Drawing.Point(6, 104);
      this.gbMergeTime.Name = "gbMergeTime";
      this.gbMergeTime.Size = new System.Drawing.Size(294, 59);
      this.gbMergeTime.TabIndex = 22;
      this.gbMergeTime.TabStop = false;
      this.gbMergeTime.Text = "Merge Time";
      // 
      // tbEnd
      // 
      this.tbEnd.Location = new System.Drawing.Point(195, 19);
      this.tbEnd.Mask = "90:00";
      this.tbEnd.Name = "tbEnd";
      this.tbEnd.Size = new System.Drawing.Size(61, 20);
      this.tbEnd.TabIndex = 3;
      this.tbEnd.ValidatingType = typeof(System.DateTime);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(153, 23);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(26, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "End";
      // 
      // tbStart
      // 
      this.tbStart.Location = new System.Drawing.Point(67, 20);
      this.tbStart.Mask = "90:00";
      this.tbStart.Name = "tbStart";
      this.tbStart.Size = new System.Drawing.Size(56, 20);
      this.tbStart.TabIndex = 1;
      this.tbStart.ValidatingType = typeof(System.DateTime);
      // 
      // lStart
      // 
      this.lStart.AutoSize = true;
      this.lStart.Location = new System.Drawing.Point(6, 23);
      this.lStart.Name = "lStart";
      this.lStart.Size = new System.Drawing.Size(29, 13);
      this.lStart.TabIndex = 0;
      this.lStart.Text = "Start";
      // 
      // MergedChannelDetails
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(312, 201);
      this.Controls.Add(this.gbMergeTime);
      this.Controls.Add(this.bCancel);
      this.Controls.Add(this.bOk);
      this.Controls.Add(this.gbGrabber);
      this.Controls.Add(this.bChannelID);
      this.Controls.Add(this.l_cID);
      this.Controls.Add(this.tbChannelName);
      this.Name = "MergedChannelDetails";
      this.Text = "Merged Channel Details";
      this.gbGrabber.ResumeLayout(false);
      this.gbGrabber.PerformLayout();
      this.gbMergeTime.ResumeLayout(false);
      this.gbMergeTime.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox gbGrabber;
    private MediaPortal.UserInterface.Controls.MPTextBox tbGrabDays;
    private MediaPortal.UserInterface.Controls.MPLabel lGuideDays;
    private MediaPortal.UserInterface.Controls.MPButton bGrabber;
    private MediaPortal.UserInterface.Controls.MPLabel Grabber;
    private MediaPortal.UserInterface.Controls.MPTextBox tbGrabSite;
    private MediaPortal.UserInterface.Controls.MPButton bChannelID;
    private MediaPortal.UserInterface.Controls.MPLabel l_cID;
    private MediaPortal.UserInterface.Controls.MPTextBox tbChannelName;
    private System.Windows.Forms.Button bOk;
    private System.Windows.Forms.Button bCancel;
    private System.Windows.Forms.GroupBox gbMergeTime;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label lStart;
    private System.Windows.Forms.MaskedTextBox tbEnd;
    private System.Windows.Forms.MaskedTextBox tbStart;
  }
}