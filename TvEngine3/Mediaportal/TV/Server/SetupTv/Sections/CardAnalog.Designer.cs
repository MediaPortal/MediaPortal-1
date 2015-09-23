using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class CardAnalog
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
      this.mpTabControl1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabControl();
      this.tabPageScan = new System.Windows.Forms.TabPage();
      this.labelCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelScanMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxScanMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.progressBarSignalStrength = new System.Windows.Forms.ProgressBar();
      this.progressBarProgress = new System.Windows.Forms.ProgressBar();
      this.progressBarSignalQuality = new System.Windows.Forms.ProgressBar();
      this.buttonScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.listViewProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.labelSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.openFileDialogExternalTunerChannelList = new System.Windows.Forms.OpenFileDialog();
      this.mpTabControl1.SuspendLayout();
      this.tabPageScan.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpTabControl1
      // 
      this.mpTabControl1.AllowDrop = true;
      this.mpTabControl1.AllowReorderTabs = false;
      this.mpTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpTabControl1.Controls.Add(this.tabPageScan);
      this.mpTabControl1.Location = new System.Drawing.Point(0, 0);
      this.mpTabControl1.Name = "mpTabControl1";
      this.mpTabControl1.SelectedIndex = 0;
      this.mpTabControl1.Size = new System.Drawing.Size(480, 420);
      this.mpTabControl1.TabIndex = 0;
      // 
      // tabPageScan
      // 
      this.tabPageScan.BackColor = System.Drawing.Color.Transparent;
      this.tabPageScan.Controls.Add(this.labelCountry);
      this.tabPageScan.Controls.Add(this.comboBoxCountry);
      this.tabPageScan.Controls.Add(this.labelScanMode);
      this.tabPageScan.Controls.Add(this.comboBoxScanMode);
      this.tabPageScan.Controls.Add(this.progressBarSignalStrength);
      this.tabPageScan.Controls.Add(this.progressBarProgress);
      this.tabPageScan.Controls.Add(this.progressBarSignalQuality);
      this.tabPageScan.Controls.Add(this.buttonScan);
      this.tabPageScan.Controls.Add(this.listViewProgress);
      this.tabPageScan.Controls.Add(this.labelSignalQuality);
      this.tabPageScan.Controls.Add(this.labelSignalStrength);
      this.tabPageScan.Location = new System.Drawing.Point(4, 22);
      this.tabPageScan.Name = "tabPageScan";
      this.tabPageScan.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageScan.Size = new System.Drawing.Size(472, 394);
      this.tabPageScan.TabIndex = 0;
      this.tabPageScan.Text = "Scanning";
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
      this.comboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCountry.FormattingEnabled = true;
      this.comboBoxCountry.Location = new System.Drawing.Point(105, 42);
      this.comboBoxCountry.Name = "comboBoxCountry";
      this.comboBoxCountry.Size = new System.Drawing.Size(340, 21);
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
      this.comboBoxScanMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxScanMode.FormattingEnabled = true;
      this.comboBoxScanMode.Location = new System.Drawing.Point(105, 15);
      this.comboBoxScanMode.Name = "comboBoxScanMode";
      this.comboBoxScanMode.Size = new System.Drawing.Size(340, 21);
      this.comboBoxScanMode.TabIndex = 1;
      this.comboBoxScanMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxScanMode_SelectedIndexChanged);
      // 
      // progressBarSignalStrength
      // 
      this.progressBarSignalStrength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalStrength.Location = new System.Drawing.Point(105, 109);
      this.progressBarSignalStrength.Name = "progressBarSignalStrength";
      this.progressBarSignalStrength.Size = new System.Drawing.Size(340, 10);
      this.progressBarSignalStrength.TabIndex = 6;
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarProgress.Location = new System.Drawing.Point(22, 141);
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(423, 10);
      this.progressBarProgress.TabIndex = 9;
      // 
      // progressBarSignalQuality
      // 
      this.progressBarSignalQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalQuality.Location = new System.Drawing.Point(105, 125);
      this.progressBarSignalQuality.Name = "progressBarSignalQuality";
      this.progressBarSignalQuality.Size = new System.Drawing.Size(340, 10);
      this.progressBarSignalQuality.TabIndex = 8;
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(335, 69);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(110, 23);
      this.buttonScan.TabIndex = 4;
      this.buttonScan.Text = "Scan for channels";
      this.buttonScan.UseVisualStyleBackColor = true;
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // listViewProgress
      // 
      this.listViewProgress.AllowRowReorder = false;
      this.listViewProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewProgress.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStatus});
      this.listViewProgress.Location = new System.Drawing.Point(22, 157);
      this.listViewProgress.Name = "listViewProgress";
      this.listViewProgress.Size = new System.Drawing.Size(423, 216);
      this.listViewProgress.TabIndex = 10;
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
      this.labelSignalQuality.Location = new System.Drawing.Point(19, 122);
      this.labelSignalQuality.Name = "labelSignalQuality";
      this.labelSignalQuality.Size = new System.Drawing.Size(72, 13);
      this.labelSignalQuality.TabIndex = 7;
      this.labelSignalQuality.Text = "Signal quality:";
      // 
      // labelSignalStrength
      // 
      this.labelSignalStrength.AutoSize = true;
      this.labelSignalStrength.Location = new System.Drawing.Point(19, 106);
      this.labelSignalStrength.Name = "labelSignalStrength";
      this.labelSignalStrength.Size = new System.Drawing.Size(80, 13);
      this.labelSignalStrength.TabIndex = 5;
      this.labelSignalStrength.Text = "Signal strength:";
      // 
      // openFileDialogExternalTunerChannelList
      // 
      this.openFileDialogExternalTunerChannelList.Filter = "Channel List Files (*.txt)|*.txt|All Files|*.*";
      this.openFileDialogExternalTunerChannelList.Title = "Select a channel list.";
      // 
      // CardAnalog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.mpTabControl1);
      this.Name = "CardAnalog";
      this.Size = new System.Drawing.Size(480, 420);
      this.mpTabControl1.ResumeLayout(false);
      this.tabPageScan.ResumeLayout(false);
      this.tabPageScan.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MPTabControl mpTabControl1;
    private System.Windows.Forms.TabPage tabPageScan;
    private MPLabel labelScanMode;
    private MPComboBox comboBoxScanMode;
    private System.Windows.Forms.ProgressBar progressBarSignalStrength;
    private System.Windows.Forms.ProgressBar progressBarProgress;
    private System.Windows.Forms.ProgressBar progressBarSignalQuality;
    private MPButton buttonScan;
    private MPListView listViewProgress;
    private System.Windows.Forms.ColumnHeader columnHeaderStatus;
    private MPLabel labelSignalQuality;
    private MPLabel labelSignalStrength;
    private MPLabel labelCountry;
    private MPComboBox comboBoxCountry;
    private System.Windows.Forms.OpenFileDialog openFileDialogExternalTunerChannelList;
  }
}