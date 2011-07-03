namespace SetupTv.Dialogs
{
  partial class FormDVBIPTuningDetail
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
      this.textBoxDVBIPUrl = new System.Windows.Forms.TextBox();
      this.textBoxDVBIPProvider = new System.Windows.Forms.TextBox();
      this.textBoxDVBIPPmtPid = new System.Windows.Forms.TextBox();
      this.textBoxDVBIPServiceId = new System.Windows.Forms.TextBox();
      this.textBoxDVBIPTransportId = new System.Windows.Forms.TextBox();
      this.textBoxDVBIPNetworkId = new System.Windows.Forms.TextBox();
      this.textBoxDVBIPChannel = new System.Windows.Forms.TextBox();
      this.label53 = new System.Windows.Forms.Label();
      this.checkBoxDVBIPfta = new System.Windows.Forms.CheckBox();
      this.labelDVBIPProvider = new System.Windows.Forms.Label();
      this.ipPmtLabel = new System.Windows.Forms.Label();
      this.label54 = new System.Windows.Forms.Label();
      this.label55 = new System.Windows.Forms.Label();
      this.label56 = new System.Windows.Forms.Label();
      this.label57 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Location = new System.Drawing.Point(343, 221);
      this.mpButtonCancel.TabIndex = 9;
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(244, 221);
      this.mpButtonOk.TabIndex = 8;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // textBoxDVBIPUrl
      // 
      this.textBoxDVBIPUrl.Location = new System.Drawing.Point(90, 38);
      this.textBoxDVBIPUrl.Name = "textBoxDVBIPUrl";
      this.textBoxDVBIPUrl.Size = new System.Drawing.Size(321, 20);
      this.textBoxDVBIPUrl.TabIndex = 1;
      // 
      // textBoxDVBIPProvider
      // 
      this.textBoxDVBIPProvider.Location = new System.Drawing.Point(90, 168);
      this.textBoxDVBIPProvider.Name = "textBoxDVBIPProvider";
      this.textBoxDVBIPProvider.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBIPProvider.TabIndex = 6;
      // 
      // textBoxDVBIPPmtPid
      // 
      this.textBoxDVBIPPmtPid.Location = new System.Drawing.Point(90, 142);
      this.textBoxDVBIPPmtPid.Name = "textBoxDVBIPPmtPid";
      this.textBoxDVBIPPmtPid.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBIPPmtPid.TabIndex = 5;
      this.textBoxDVBIPPmtPid.Text = "-1";
      // 
      // textBoxDVBIPServiceId
      // 
      this.textBoxDVBIPServiceId.Location = new System.Drawing.Point(90, 116);
      this.textBoxDVBIPServiceId.Name = "textBoxDVBIPServiceId";
      this.textBoxDVBIPServiceId.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBIPServiceId.TabIndex = 4;
      this.textBoxDVBIPServiceId.Text = "-1";
      // 
      // textBoxDVBIPTransportId
      // 
      this.textBoxDVBIPTransportId.Location = new System.Drawing.Point(90, 90);
      this.textBoxDVBIPTransportId.Name = "textBoxDVBIPTransportId";
      this.textBoxDVBIPTransportId.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBIPTransportId.TabIndex = 3;
      this.textBoxDVBIPTransportId.Text = "-1";
      // 
      // textBoxDVBIPNetworkId
      // 
      this.textBoxDVBIPNetworkId.Location = new System.Drawing.Point(90, 64);
      this.textBoxDVBIPNetworkId.Name = "textBoxDVBIPNetworkId";
      this.textBoxDVBIPNetworkId.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBIPNetworkId.TabIndex = 2;
      this.textBoxDVBIPNetworkId.Text = "-1";
      // 
      // textBoxDVBIPChannel
      // 
      this.textBoxDVBIPChannel.Location = new System.Drawing.Point(90, 12);
      this.textBoxDVBIPChannel.Name = "textBoxDVBIPChannel";
      this.textBoxDVBIPChannel.Size = new System.Drawing.Size(146, 20);
      this.textBoxDVBIPChannel.TabIndex = 0;
      this.textBoxDVBIPChannel.Text = "0";
      // 
      // label53
      // 
      this.label53.AutoSize = true;
      this.label53.Location = new System.Drawing.Point(12, 41);
      this.label53.Name = "label53";
      this.label53.Size = new System.Drawing.Size(32, 13);
      this.label53.TabIndex = 110;
      this.label53.Text = "URL:";
      // 
      // checkBoxDVBIPfta
      // 
      this.checkBoxDVBIPfta.AutoSize = true;
      this.checkBoxDVBIPfta.Location = new System.Drawing.Point(90, 194);
      this.checkBoxDVBIPfta.Name = "checkBoxDVBIPfta";
      this.checkBoxDVBIPfta.Size = new System.Drawing.Size(78, 17);
      this.checkBoxDVBIPfta.TabIndex = 7;
      this.checkBoxDVBIPfta.Text = "Free To Air";
      this.checkBoxDVBIPfta.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.checkBoxDVBIPfta.UseVisualStyleBackColor = true;
      // 
      // labelDVBIPProvider
      // 
      this.labelDVBIPProvider.AutoSize = true;
      this.labelDVBIPProvider.Location = new System.Drawing.Point(12, 171);
      this.labelDVBIPProvider.Name = "labelDVBIPProvider";
      this.labelDVBIPProvider.Size = new System.Drawing.Size(49, 13);
      this.labelDVBIPProvider.TabIndex = 108;
      this.labelDVBIPProvider.Text = "Provider:";
      // 
      // ipPmtLabel
      // 
      this.ipPmtLabel.AutoSize = true;
      this.ipPmtLabel.Location = new System.Drawing.Point(12, 145);
      this.ipPmtLabel.Name = "ipPmtLabel";
      this.ipPmtLabel.Size = new System.Drawing.Size(54, 13);
      this.ipPmtLabel.TabIndex = 106;
      this.ipPmtLabel.Text = "PMT PID:";
      // 
      // label54
      // 
      this.label54.AutoSize = true;
      this.label54.Location = new System.Drawing.Point(12, 93);
      this.label54.Name = "label54";
      this.label54.Size = new System.Drawing.Size(69, 13);
      this.label54.TabIndex = 104;
      this.label54.Text = "Transport ID:";
      // 
      // label55
      // 
      this.label55.AutoSize = true;
      this.label55.Location = new System.Drawing.Point(12, 119);
      this.label55.Name = "label55";
      this.label55.Size = new System.Drawing.Size(60, 13);
      this.label55.TabIndex = 103;
      this.label55.Text = "Service ID:";
      // 
      // label56
      // 
      this.label56.AutoSize = true;
      this.label56.Location = new System.Drawing.Point(12, 67);
      this.label56.Name = "label56";
      this.label56.Size = new System.Drawing.Size(64, 13);
      this.label56.TabIndex = 102;
      this.label56.Text = "Network ID:";
      // 
      // label57
      // 
      this.label57.AutoSize = true;
      this.label57.Location = new System.Drawing.Point(12, 15);
      this.label57.Name = "label57";
      this.label57.Size = new System.Drawing.Size(49, 13);
      this.label57.TabIndex = 101;
      this.label57.Text = "Channel:";
      // 
      // FormDVBIPTuningDetail
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(430, 256);
      this.Controls.Add(this.textBoxDVBIPUrl);
      this.Controls.Add(this.textBoxDVBIPProvider);
      this.Controls.Add(this.textBoxDVBIPPmtPid);
      this.Controls.Add(this.textBoxDVBIPServiceId);
      this.Controls.Add(this.textBoxDVBIPTransportId);
      this.Controls.Add(this.textBoxDVBIPNetworkId);
      this.Controls.Add(this.textBoxDVBIPChannel);
      this.Controls.Add(this.label53);
      this.Controls.Add(this.checkBoxDVBIPfta);
      this.Controls.Add(this.labelDVBIPProvider);
      this.Controls.Add(this.ipPmtLabel);
      this.Controls.Add(this.label54);
      this.Controls.Add(this.label55);
      this.Controls.Add(this.label56);
      this.Controls.Add(this.label57);
      this.Name = "FormDVBIPTuningDetail";
      this.Text = "Add / Edit DVB-IP Tuningdetail";
      this.Load += new System.EventHandler(this.FormDVBIPTuningDetail_Load);
      this.Controls.SetChildIndex(this.mpButtonOk, 0);
      this.Controls.SetChildIndex(this.mpButtonCancel, 0);
      this.Controls.SetChildIndex(this.label57, 0);
      this.Controls.SetChildIndex(this.label56, 0);
      this.Controls.SetChildIndex(this.label55, 0);
      this.Controls.SetChildIndex(this.label54, 0);
      this.Controls.SetChildIndex(this.ipPmtLabel, 0);
      this.Controls.SetChildIndex(this.labelDVBIPProvider, 0);
      this.Controls.SetChildIndex(this.checkBoxDVBIPfta, 0);
      this.Controls.SetChildIndex(this.label53, 0);
      this.Controls.SetChildIndex(this.textBoxDVBIPChannel, 0);
      this.Controls.SetChildIndex(this.textBoxDVBIPNetworkId, 0);
      this.Controls.SetChildIndex(this.textBoxDVBIPTransportId, 0);
      this.Controls.SetChildIndex(this.textBoxDVBIPServiceId, 0);
      this.Controls.SetChildIndex(this.textBoxDVBIPPmtPid, 0);
      this.Controls.SetChildIndex(this.textBoxDVBIPProvider, 0);
      this.Controls.SetChildIndex(this.textBoxDVBIPUrl, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox textBoxDVBIPUrl;
    private System.Windows.Forms.TextBox textBoxDVBIPProvider;
    private System.Windows.Forms.TextBox textBoxDVBIPPmtPid;
    private System.Windows.Forms.TextBox textBoxDVBIPServiceId;
    private System.Windows.Forms.TextBox textBoxDVBIPTransportId;
    private System.Windows.Forms.TextBox textBoxDVBIPNetworkId;
    private System.Windows.Forms.TextBox textBoxDVBIPChannel;
    private System.Windows.Forms.Label label53;
    private System.Windows.Forms.CheckBox checkBoxDVBIPfta;
    private System.Windows.Forms.Label labelDVBIPProvider;
    private System.Windows.Forms.Label ipPmtLabel;
    private System.Windows.Forms.Label label54;
    private System.Windows.Forms.Label label55;
    private System.Windows.Forms.Label label56;
    private System.Windows.Forms.Label label57;
  }
}
