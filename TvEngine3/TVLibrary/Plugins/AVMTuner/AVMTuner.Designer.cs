using System.Drawing;

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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AVMTuner));
      this.listViewStatus = new System.Windows.Forms.ListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.mpButtonScanTv = new MediaPortal.UserInterface.Controls.MPButton();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.panel1 = new System.Windows.Forms.Panel();
      this.lblTunerNumber = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.listDevices = new System.Windows.Forms.ListBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.grpTuningOptions = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.chkScanRadio = new System.Windows.Forms.CheckBox();
      this.chkScanHD = new System.Windows.Forms.CheckBox();
      this.chkScanSD = new System.Windows.Forms.CheckBox();
      this.checkBoxCreateGroups = new System.Windows.Forms.CheckBox();
      this.btnDetect = new MediaPortal.UserInterface.Controls.MPButton();
      this.chkAutoMapTuner = new System.Windows.Forms.CheckBox();
      this.panel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.grpTuningOptions.SuspendLayout();
      this.SuspendLayout();
      // 
      // listViewStatus
      // 
      this.listViewStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.listViewStatus.Location = new System.Drawing.Point(3, 224);
      this.listViewStatus.Name = "listViewStatus";
      this.listViewStatus.Size = new System.Drawing.Size(896, 133);
      this.listViewStatus.TabIndex = 9;
      this.listViewStatus.UseCompatibleStateImageBehavior = false;
      this.listViewStatus.View = System.Windows.Forms.View.Details;
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
      this.progressBar1.Location = new System.Drawing.Point(3, 208);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(896, 10);
      this.progressBar1.TabIndex = 8;
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Location = new System.Drawing.Point(326, 119);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(91, 37);
      this.mpButtonScanTv.TabIndex = 13;
      this.mpButtonScanTv.Text = "Scan for channels";
      this.mpButtonScanTv.UseVisualStyleBackColor = true;
      this.mpButtonScanTv.Click += new System.EventHandler(this.btnImport_Click);
      // 
      // imageList1
      // 
      this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // panel1
      // 
      this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(110)))), ((int)(((byte)(192)))));
      this.panel1.Controls.Add(this.lblTunerNumber);
      this.panel1.Controls.Add(this.mpLabel2);
      this.panel1.Controls.Add(this.listDevices);
      this.panel1.Controls.Add(this.mpLabel1);
      this.panel1.Controls.Add(this.pictureBox1);
      this.panel1.Location = new System.Drawing.Point(3, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(899, 107);
      this.panel1.TabIndex = 16;
      // 
      // lblTunerNumber
      // 
      this.lblTunerNumber.AutoSize = true;
      this.lblTunerNumber.ForeColor = System.Drawing.Color.White;
      this.lblTunerNumber.Location = new System.Drawing.Point(99, 79);
      this.lblTunerNumber.Name = "lblTunerNumber";
      this.lblTunerNumber.Size = new System.Drawing.Size(10, 13);
      this.lblTunerNumber.TabIndex = 20;
      this.lblTunerNumber.Text = " ";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.ForeColor = System.Drawing.Color.White;
      this.mpLabel2.Location = new System.Drawing.Point(4, 79);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(91, 13);
      this.mpLabel2.TabIndex = 19;
      this.mpLabel2.Text = "Number of tuners:";
      // 
      // listDevices
      // 
      this.listDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listDevices.DisplayMember = "Text";
      this.listDevices.FormattingEnabled = true;
      this.listDevices.Location = new System.Drawing.Point(7, 33);
      this.listDevices.Name = "listDevices";
      this.listDevices.Size = new System.Drawing.Size(744, 43);
      this.listDevices.TabIndex = 17;
      this.listDevices.SelectedIndexChanged += new System.EventHandler(this.listDevices_SelectedIndexChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.mpLabel1.ForeColor = System.Drawing.Color.White;
      this.mpLabel1.Location = new System.Drawing.Point(4, 5);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(357, 13);
      this.mpLabel1.TabIndex = 16;
      this.mpLabel1.Text = "Imports channels from AVM FritzBox / WLAN Repeater DVB-C";
      // 
      // pictureBox1
      // 
      this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.InitialImage = null;
      this.pictureBox1.Location = new System.Drawing.Point(711, 5);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(188, 99);
      this.pictureBox1.TabIndex = 18;
      this.pictureBox1.TabStop = false;
      // 
      // grpTuningOptions
      // 
      this.grpTuningOptions.Controls.Add(this.chkScanRadio);
      this.grpTuningOptions.Controls.Add(this.chkScanHD);
      this.grpTuningOptions.Controls.Add(this.chkScanSD);
      this.grpTuningOptions.Controls.Add(this.chkAutoMapTuner);
      this.grpTuningOptions.Controls.Add(this.checkBoxCreateGroups);
      this.grpTuningOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.grpTuningOptions.Location = new System.Drawing.Point(3, 113);
      this.grpTuningOptions.Name = "grpTuningOptions";
      this.grpTuningOptions.Size = new System.Drawing.Size(317, 89);
      this.grpTuningOptions.TabIndex = 17;
      this.grpTuningOptions.TabStop = false;
      this.grpTuningOptions.Text = "Tuning options";
      // 
      // chkScanRadio
      // 
      this.chkScanRadio.AutoSize = true;
      this.chkScanRadio.Checked = true;
      this.chkScanRadio.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkScanRadio.Location = new System.Drawing.Point(196, 63);
      this.chkScanRadio.Name = "chkScanRadio";
      this.chkScanRadio.Size = new System.Drawing.Size(101, 17);
      this.chkScanRadio.TabIndex = 12;
      this.chkScanRadio.Text = "Radio Channels";
      this.chkScanRadio.UseVisualStyleBackColor = true;
      // 
      // chkScanHD
      // 
      this.chkScanHD.AutoSize = true;
      this.chkScanHD.Checked = true;
      this.chkScanHD.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkScanHD.Location = new System.Drawing.Point(196, 40);
      this.chkScanHD.Name = "chkScanHD";
      this.chkScanHD.Size = new System.Drawing.Size(106, 17);
      this.chkScanHD.TabIndex = 13;
      this.chkScanHD.Text = "TV Channels HD";
      this.chkScanHD.UseVisualStyleBackColor = true;
      // 
      // chkScanSD
      // 
      this.chkScanSD.AutoSize = true;
      this.chkScanSD.Checked = true;
      this.chkScanSD.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkScanSD.Location = new System.Drawing.Point(196, 17);
      this.chkScanSD.Name = "chkScanSD";
      this.chkScanSD.Size = new System.Drawing.Size(105, 17);
      this.chkScanSD.TabIndex = 14;
      this.chkScanSD.Text = "TV Channels SD";
      this.chkScanSD.UseVisualStyleBackColor = true;
      // 
      // checkBoxCreateGroups
      // 
      this.checkBoxCreateGroups.AutoSize = true;
      this.checkBoxCreateGroups.Location = new System.Drawing.Point(16, 20);
      this.checkBoxCreateGroups.Name = "checkBoxCreateGroups";
      this.checkBoxCreateGroups.Size = new System.Drawing.Size(175, 17);
      this.checkBoxCreateGroups.TabIndex = 15;
      this.checkBoxCreateGroups.Text = "Create groups for each provider";
      this.checkBoxCreateGroups.UseVisualStyleBackColor = true;
      // 
      // btnDetect
      // 
      this.btnDetect.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
      this.btnDetect.Location = new System.Drawing.Point(326, 162);
      this.btnDetect.Name = "btnDetect";
      this.btnDetect.Size = new System.Drawing.Size(91, 37);
      this.btnDetect.TabIndex = 13;
      this.btnDetect.Text = "Detect Devices";
      this.btnDetect.UseVisualStyleBackColor = true;
      this.btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
      // 
      // chkAutoMapTuner
      // 
      this.chkAutoMapTuner.AutoSize = true;
      this.chkAutoMapTuner.Checked = true;
      this.chkAutoMapTuner.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkAutoMapTuner.Location = new System.Drawing.Point(16, 43);
      this.chkAutoMapTuner.Name = "chkAutoMapTuner";
      this.chkAutoMapTuner.Size = new System.Drawing.Size(160, 17);
      this.chkAutoMapTuner.TabIndex = 15;
      this.chkAutoMapTuner.Text = "Auto map to available tuners";
      this.chkAutoMapTuner.UseVisualStyleBackColor = true;
      // 
      // AVMTuner
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.grpTuningOptions);
      this.Controls.Add(this.btnDetect);
      this.Controls.Add(this.mpButtonScanTv);
      this.Controls.Add(this.listViewStatus);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.panel1);
      this.Name = "AVMTuner";
      this.Size = new System.Drawing.Size(902, 369);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.grpTuningOptions.ResumeLayout(false);
      this.grpTuningOptions.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.ListView listViewStatus;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanTv;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.ListBox listDevices;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private System.Windows.Forms.PictureBox pictureBox1;
    private MediaPortal.UserInterface.Controls.MPLabel lblTunerNumber;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPGroupBox grpTuningOptions;
    private System.Windows.Forms.CheckBox chkScanRadio;
    private System.Windows.Forms.CheckBox chkScanHD;
    private System.Windows.Forms.CheckBox chkScanSD;
    private System.Windows.Forms.CheckBox checkBoxCreateGroups;
    private MediaPortal.UserInterface.Controls.MPButton btnDetect;
    private System.Windows.Forms.CheckBox chkAutoMapTuner;
  }
}
