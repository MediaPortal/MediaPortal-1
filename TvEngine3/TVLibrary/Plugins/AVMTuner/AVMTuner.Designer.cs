namespace AVMTuner
{
  partial class AVMTuner
  {
    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Komponenten-Designer generierter Code

    /// <summary> 
    /// Erforderliche Methode für die Designerunterstützung. 
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.listDevices = new System.Windows.Forms.ListBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblTunerNumber = new MediaPortal.UserInterface.Controls.MPLabel();
      this.listViewStatus = new System.Windows.Forms.ListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.checkBoxCreateGroups = new System.Windows.Forms.CheckBox();
      this.mpButtonScanTv = new MediaPortal.UserInterface.Controls.MPButton();
      this.chkScanSD = new System.Windows.Forms.CheckBox();
      this.chkScanHD = new System.Windows.Forms.CheckBox();
      this.chkScanRadio = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(3, 11);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(301, 13);
      this.mpLabel1.TabIndex = 1;
      this.mpLabel1.Text = "Imports channels from AVM FritzBox / WLAN Repeater DVB-C";
      // 
      // listDevices
      // 
      this.listDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listDevices.DisplayMember = "Text";
      this.listDevices.FormattingEnabled = true;
      this.listDevices.Location = new System.Drawing.Point(6, 36);
      this.listDevices.Name = "listDevices";
      this.listDevices.Size = new System.Drawing.Size(883, 43);
      this.listDevices.TabIndex = 3;
      this.listDevices.SelectedIndexChanged += new System.EventHandler(this.listDevices_SelectedIndexChanged);
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(7, 82);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(91, 13);
      this.mpLabel2.TabIndex = 4;
      this.mpLabel2.Text = "Number of tuners:";
      // 
      // lblTunerNumber
      // 
      this.lblTunerNumber.AutoSize = true;
      this.lblTunerNumber.Location = new System.Drawing.Point(119, 82);
      this.lblTunerNumber.Name = "lblTunerNumber";
      this.lblTunerNumber.Size = new System.Drawing.Size(0, 13);
      this.lblTunerNumber.TabIndex = 4;
      // 
      // listViewStatus
      // 
      this.listViewStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.listViewStatus.Location = new System.Drawing.Point(6, 244);
      this.listViewStatus.Name = "listViewStatus";
      this.listViewStatus.Size = new System.Drawing.Size(883, 285);
      this.listViewStatus.TabIndex = 9;
      this.listViewStatus.UseCompatibleStateImageBehavior = false;
      this.listViewStatus.View = System.Windows.Forms.View.Details;
      this.listViewStatus.SelectedIndexChanged += new System.EventHandler(this.listViewStatus_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Status";
      this.columnHeader1.Width = 350;
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(7, 214);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(882, 10);
      this.progressBar1.TabIndex = 8;
      // 
      // checkBoxCreateGroups
      // 
      this.checkBoxCreateGroups.AutoSize = true;
      this.checkBoxCreateGroups.Location = new System.Drawing.Point(10, 116);
      this.checkBoxCreateGroups.Name = "checkBoxCreateGroups";
      this.checkBoxCreateGroups.Size = new System.Drawing.Size(175, 17);
      this.checkBoxCreateGroups.TabIndex = 11;
      this.checkBoxCreateGroups.Text = "Create groups for each provider";
      this.checkBoxCreateGroups.UseVisualStyleBackColor = true;
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Location = new System.Drawing.Point(295, 112);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(131, 23);
      this.mpButtonScanTv.TabIndex = 13;
      this.mpButtonScanTv.Text = "Scan for channels";
      this.mpButtonScanTv.UseVisualStyleBackColor = true;
      this.mpButtonScanTv.Click += new System.EventHandler(this.btnImport_Click);
      // 
      // chkScanSD
      // 
      this.chkScanSD.AutoSize = true;
      this.chkScanSD.Checked = true;
      this.chkScanSD.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkScanSD.Location = new System.Drawing.Point(10, 139);
      this.chkScanSD.Name = "chkScanSD";
      this.chkScanSD.Size = new System.Drawing.Size(105, 17);
      this.chkScanSD.TabIndex = 11;
      this.chkScanSD.Text = "TV Channels SD";
      this.chkScanSD.UseVisualStyleBackColor = true;
      // 
      // chkScanHD
      // 
      this.chkScanHD.AutoSize = true;
      this.chkScanHD.Checked = true;
      this.chkScanHD.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkScanHD.Location = new System.Drawing.Point(10, 162);
      this.chkScanHD.Name = "chkScanHD";
      this.chkScanHD.Size = new System.Drawing.Size(106, 17);
      this.chkScanHD.TabIndex = 11;
      this.chkScanHD.Text = "TV Channels HD";
      this.chkScanHD.UseVisualStyleBackColor = true;
      // 
      // chkScanRadio
      // 
      this.chkScanRadio.AutoSize = true;
      this.chkScanRadio.Checked = true;
      this.chkScanRadio.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkScanRadio.Location = new System.Drawing.Point(10, 185);
      this.chkScanRadio.Name = "chkScanRadio";
      this.chkScanRadio.Size = new System.Drawing.Size(101, 17);
      this.chkScanRadio.TabIndex = 11;
      this.chkScanRadio.Text = "Radio Channels";
      this.chkScanRadio.UseVisualStyleBackColor = true;
      // 
      // AVMTuner
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.chkScanRadio);
      this.Controls.Add(this.chkScanHD);
      this.Controls.Add(this.chkScanSD);
      this.Controls.Add(this.checkBoxCreateGroups);
      this.Controls.Add(this.mpButtonScanTv);
      this.Controls.Add(this.listViewStatus);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.lblTunerNumber);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.listDevices);
      this.Controls.Add(this.mpLabel1);
      this.Name = "AVMTuner";
      this.Size = new System.Drawing.Size(902, 543);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private System.Windows.Forms.ListBox listDevices;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel lblTunerNumber;
    private System.Windows.Forms.ListView listViewStatus;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.CheckBox checkBoxCreateGroups;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanTv;
    private System.Windows.Forms.CheckBox chkScanSD;
    private System.Windows.Forms.CheckBox chkScanHD;
    private System.Windows.Forms.CheckBox chkScanRadio;
  }
}
