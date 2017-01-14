using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class ScanAtscScte
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
      this.progressBarSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.progressBarSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.labelSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.progressBarProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.buttonScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.listViewProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderStatus = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.labelTransmitter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxTransmitter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelScanMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxScanMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.SuspendLayout();
      // 
      // progressBarSignalQuality
      // 
      this.progressBarSignalQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalQuality.Location = new System.Drawing.Point(105, 129);
      this.progressBarSignalQuality.Name = "progressBarSignalQuality";
      this.progressBarSignalQuality.Size = new System.Drawing.Size(351, 10);
      this.progressBarSignalQuality.TabIndex = 8;
      // 
      // progressBarSignalStrength
      // 
      this.progressBarSignalStrength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalStrength.Location = new System.Drawing.Point(105, 113);
      this.progressBarSignalStrength.Name = "progressBarSignalStrength";
      this.progressBarSignalStrength.Size = new System.Drawing.Size(351, 10);
      this.progressBarSignalStrength.TabIndex = 6;
      // 
      // labelSignalQuality
      // 
      this.labelSignalQuality.AutoSize = true;
      this.labelSignalQuality.Location = new System.Drawing.Point(19, 126);
      this.labelSignalQuality.Name = "labelSignalQuality";
      this.labelSignalQuality.Size = new System.Drawing.Size(72, 13);
      this.labelSignalQuality.TabIndex = 7;
      this.labelSignalQuality.Text = "Signal quality:";
      // 
      // labelSignalStrength
      // 
      this.labelSignalStrength.AutoSize = true;
      this.labelSignalStrength.Location = new System.Drawing.Point(19, 110);
      this.labelSignalStrength.Name = "labelSignalStrength";
      this.labelSignalStrength.Size = new System.Drawing.Size(80, 13);
      this.labelSignalStrength.TabIndex = 5;
      this.labelSignalStrength.Text = "Signal strength:";
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarProgress.Location = new System.Drawing.Point(22, 145);
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(434, 10);
      this.progressBarProgress.TabIndex = 9;
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(346, 69);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(110, 23);
      this.buttonScan.TabIndex = 4;
      this.buttonScan.Text = "&Scan for channels";
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // listViewProgress
      // 
      this.listViewProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewProgress.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStatus});
      this.listViewProgress.Location = new System.Drawing.Point(22, 161);
      this.listViewProgress.Name = "listViewProgress";
      this.listViewProgress.Size = new System.Drawing.Size(434, 239);
      this.listViewProgress.TabIndex = 10;
      this.listViewProgress.UseCompatibleStateImageBehavior = false;
      this.listViewProgress.View = System.Windows.Forms.View.Details;
      // 
      // columnHeaderStatus
      // 
      this.columnHeaderStatus.Text = "Status";
      this.columnHeaderStatus.Width = 388;
      // 
      // labelTransmitter
      // 
      this.labelTransmitter.AutoSize = true;
      this.labelTransmitter.Location = new System.Drawing.Point(19, 45);
      this.labelTransmitter.Name = "labelTransmitter";
      this.labelTransmitter.Size = new System.Drawing.Size(62, 13);
      this.labelTransmitter.TabIndex = 2;
      this.labelTransmitter.Text = "Transmitter:";
      // 
      // comboBoxTransmitter
      // 
      this.comboBoxTransmitter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxTransmitter.FormattingEnabled = true;
      this.comboBoxTransmitter.Location = new System.Drawing.Point(105, 42);
      this.comboBoxTransmitter.Name = "comboBoxTransmitter";
      this.comboBoxTransmitter.Size = new System.Drawing.Size(351, 21);
      this.comboBoxTransmitter.TabIndex = 3;
      // 
      // labelScanMode
      // 
      this.labelScanMode.AutoSize = true;
      this.labelScanMode.Location = new System.Drawing.Point(19, 18);
      this.labelScanMode.Name = "labelScanMode";
      this.labelScanMode.Size = new System.Drawing.Size(64, 13);
      this.labelScanMode.TabIndex = 0;
      this.labelScanMode.Text = "Scan mode:";
      // 
      // comboBoxScanMode
      // 
      this.comboBoxScanMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxScanMode.FormattingEnabled = true;
      this.comboBoxScanMode.Location = new System.Drawing.Point(105, 15);
      this.comboBoxScanMode.Name = "comboBoxScanMode";
      this.comboBoxScanMode.Size = new System.Drawing.Size(351, 21);
      this.comboBoxScanMode.TabIndex = 1;
      this.comboBoxScanMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxScanMode_SelectedIndexChanged);
      // 
      // ScanAtscScte
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.labelTransmitter);
      this.Controls.Add(this.comboBoxTransmitter);
      this.Controls.Add(this.labelScanMode);
      this.Controls.Add(this.comboBoxScanMode);
      this.Controls.Add(this.progressBarSignalStrength);
      this.Controls.Add(this.progressBarProgress);
      this.Controls.Add(this.progressBarSignalQuality);
      this.Controls.Add(this.buttonScan);
      this.Controls.Add(this.listViewProgress);
      this.Controls.Add(this.labelSignalQuality);
      this.Controls.Add(this.labelSignalStrength);
      this.Name = "ScanAtscScte";
      this.Size = new System.Drawing.Size(480, 420);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPProgressBar progressBarSignalQuality;
    private MPProgressBar progressBarSignalStrength;
    private MPLabel labelSignalQuality;
    private MPLabel labelSignalStrength;
    private MPProgressBar progressBarProgress;
    private MPButton buttonScan;
    private MPListView listViewProgress;
    private MPColumnHeader columnHeaderStatus;
    private MPLabel labelScanMode;
    private MPComboBox comboBoxScanMode;
    private MPLabel labelTransmitter;
    private MPComboBox comboBoxTransmitter;
  }
}