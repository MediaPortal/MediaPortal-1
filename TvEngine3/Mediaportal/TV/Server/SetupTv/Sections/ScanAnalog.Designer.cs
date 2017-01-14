using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class ScanAnalog
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
      this.labelTransmitter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxTransmitter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelScanMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxScanMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.progressBarSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.progressBarProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.progressBarSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.buttonScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.listViewProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderStatus = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.labelSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.openFileDialogExternalTunerChannelList = new System.Windows.Forms.OpenFileDialog();
      this.SuspendLayout();
      // 
      // labelTransmitter
      // 
      this.labelTransmitter.AutoSize = true;
      this.labelTransmitter.Location = new System.Drawing.Point(19, 72);
      this.labelTransmitter.Name = "labelTransmitter";
      this.labelTransmitter.Size = new System.Drawing.Size(62, 13);
      this.labelTransmitter.TabIndex = 4;
      this.labelTransmitter.Text = "Transmitter:";
      // 
      // comboBoxTransmitter
      // 
      this.comboBoxTransmitter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxTransmitter.FormattingEnabled = true;
      this.comboBoxTransmitter.Location = new System.Drawing.Point(105, 69);
      this.comboBoxTransmitter.Name = "comboBoxTransmitter";
      this.comboBoxTransmitter.Size = new System.Drawing.Size(351, 21);
      this.comboBoxTransmitter.TabIndex = 5;
      // 
      // labelCountry
      // 
      this.labelCountry.AutoSize = true;
      this.labelCountry.Location = new System.Drawing.Point(19, 45);
      this.labelCountry.Name = "labelCountry";
      this.labelCountry.Size = new System.Drawing.Size(46, 13);
      this.labelCountry.TabIndex = 2;
      this.labelCountry.Text = "Country:";
      // 
      // comboBoxCountry
      // 
      this.comboBoxCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxCountry.FormattingEnabled = true;
      this.comboBoxCountry.Location = new System.Drawing.Point(105, 42);
      this.comboBoxCountry.Name = "comboBoxCountry";
      this.comboBoxCountry.Size = new System.Drawing.Size(351, 21);
      this.comboBoxCountry.TabIndex = 3;
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
      // progressBarSignalStrength
      // 
      this.progressBarSignalStrength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalStrength.Location = new System.Drawing.Point(105, 136);
      this.progressBarSignalStrength.Name = "progressBarSignalStrength";
      this.progressBarSignalStrength.Size = new System.Drawing.Size(351, 10);
      this.progressBarSignalStrength.TabIndex = 8;
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarProgress.Location = new System.Drawing.Point(22, 168);
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(434, 10);
      this.progressBarProgress.TabIndex = 11;
      // 
      // progressBarSignalQuality
      // 
      this.progressBarSignalQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalQuality.Location = new System.Drawing.Point(105, 152);
      this.progressBarSignalQuality.Name = "progressBarSignalQuality";
      this.progressBarSignalQuality.Size = new System.Drawing.Size(351, 10);
      this.progressBarSignalQuality.TabIndex = 10;
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(346, 96);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(110, 23);
      this.buttonScan.TabIndex = 6;
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
      this.listViewProgress.Location = new System.Drawing.Point(22, 184);
      this.listViewProgress.Name = "listViewProgress";
      this.listViewProgress.Size = new System.Drawing.Size(434, 216);
      this.listViewProgress.TabIndex = 12;
      this.listViewProgress.UseCompatibleStateImageBehavior = false;
      this.listViewProgress.View = System.Windows.Forms.View.Details;
      // 
      // columnHeaderStatus
      // 
      this.columnHeaderStatus.Text = "Status";
      this.columnHeaderStatus.Width = 388;
      // 
      // labelSignalQuality
      // 
      this.labelSignalQuality.AutoSize = true;
      this.labelSignalQuality.Location = new System.Drawing.Point(19, 149);
      this.labelSignalQuality.Name = "labelSignalQuality";
      this.labelSignalQuality.Size = new System.Drawing.Size(72, 13);
      this.labelSignalQuality.TabIndex = 9;
      this.labelSignalQuality.Text = "Signal quality:";
      // 
      // labelSignalStrength
      // 
      this.labelSignalStrength.AutoSize = true;
      this.labelSignalStrength.Location = new System.Drawing.Point(19, 133);
      this.labelSignalStrength.Name = "labelSignalStrength";
      this.labelSignalStrength.Size = new System.Drawing.Size(80, 13);
      this.labelSignalStrength.TabIndex = 7;
      this.labelSignalStrength.Text = "Signal strength:";
      // 
      // openFileDialogExternalTunerChannelList
      // 
      this.openFileDialogExternalTunerChannelList.Filter = "Channel List Files (*.txt)|*.txt|All Files|*.*";
      this.openFileDialogExternalTunerChannelList.Title = "Select a channel list.";
      // 
      // ScanAnalog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.labelTransmitter);
      this.Controls.Add(this.comboBoxTransmitter);
      this.Controls.Add(this.labelCountry);
      this.Controls.Add(this.comboBoxCountry);
      this.Controls.Add(this.labelScanMode);
      this.Controls.Add(this.comboBoxScanMode);
      this.Controls.Add(this.progressBarSignalStrength);
      this.Controls.Add(this.progressBarProgress);
      this.Controls.Add(this.progressBarSignalQuality);
      this.Controls.Add(this.buttonScan);
      this.Controls.Add(this.listViewProgress);
      this.Controls.Add(this.labelSignalQuality);
      this.Controls.Add(this.labelSignalStrength);
      this.Name = "ScanAnalog";
      this.Size = new System.Drawing.Size(480, 420);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPLabel labelScanMode;
    private MPComboBox comboBoxScanMode;
    private MPProgressBar progressBarSignalStrength;
    private MPProgressBar progressBarProgress;
    private MPProgressBar progressBarSignalQuality;
    private MPButton buttonScan;
    private MPListView listViewProgress;
    private MPColumnHeader columnHeaderStatus;
    private MPLabel labelSignalQuality;
    private MPLabel labelSignalStrength;
    private MPLabel labelCountry;
    private MPComboBox comboBoxCountry;
    private System.Windows.Forms.OpenFileDialog openFileDialogExternalTunerChannelList;
    private MPLabel labelTransmitter;
    private MPComboBox comboBoxTransmitter;
  }
}