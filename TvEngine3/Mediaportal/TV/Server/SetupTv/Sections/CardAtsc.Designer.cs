using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class CardAtsc
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
      this.columnHeaderStatus = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.tabControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabControl();
      this.tabPageScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.labelScanMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxScanMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.tabControl.SuspendLayout();
      this.tabPageScan.SuspendLayout();
      this.SuspendLayout();
      // 
      // progressBarSignalQuality
      // 
      this.progressBarSignalQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalQuality.Location = new System.Drawing.Point(105, 102);
      this.progressBarSignalQuality.Name = "progressBarSignalQuality";
      this.progressBarSignalQuality.Size = new System.Drawing.Size(340, 10);
      this.progressBarSignalQuality.TabIndex = 6;
      // 
      // progressBarSignalStrength
      // 
      this.progressBarSignalStrength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarSignalStrength.Location = new System.Drawing.Point(105, 86);
      this.progressBarSignalStrength.Name = "progressBarSignalStrength";
      this.progressBarSignalStrength.Size = new System.Drawing.Size(340, 10);
      this.progressBarSignalStrength.TabIndex = 4;
      // 
      // labelSignalQuality
      // 
      this.labelSignalQuality.AutoSize = true;
      this.labelSignalQuality.Location = new System.Drawing.Point(19, 99);
      this.labelSignalQuality.Name = "labelSignalQuality";
      this.labelSignalQuality.Size = new System.Drawing.Size(72, 13);
      this.labelSignalQuality.TabIndex = 5;
      this.labelSignalQuality.Text = "Signal quality:";
      // 
      // labelSignalStrength
      // 
      this.labelSignalStrength.AutoSize = true;
      this.labelSignalStrength.Location = new System.Drawing.Point(19, 83);
      this.labelSignalStrength.Name = "labelSignalStrength";
      this.labelSignalStrength.Size = new System.Drawing.Size(80, 13);
      this.labelSignalStrength.TabIndex = 3;
      this.labelSignalStrength.Text = "Signal strength:";
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarProgress.Location = new System.Drawing.Point(22, 118);
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(423, 10);
      this.progressBarProgress.TabIndex = 7;
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(335, 42);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(110, 23);
      this.buttonScan.TabIndex = 2;
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
      this.listViewProgress.Location = new System.Drawing.Point(22, 134);
      this.listViewProgress.Name = "listViewProgress";
      this.listViewProgress.Size = new System.Drawing.Size(423, 239);
      this.listViewProgress.TabIndex = 8;
      this.listViewProgress.UseCompatibleStateImageBehavior = false;
      this.listViewProgress.View = System.Windows.Forms.View.Details;
      // 
      // columnHeaderStatus
      // 
      this.columnHeaderStatus.Text = "Status";
      this.columnHeaderStatus.Width = 388;
      // 
      // tabControl
      // 
      this.tabControl.AllowDrop = true;
      this.tabControl.AllowReorderTabs = false;
      this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl.Controls.Add(this.tabPageScan);
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(480, 420);
      this.tabControl.TabIndex = 0;
      // 
      // tabPageScan
      // 
      this.tabPageScan.BackColor = System.Drawing.Color.Transparent;
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
      // 
      // CardAtsc
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.tabControl);
      this.Name = "CardAtsc";
      this.Size = new System.Drawing.Size(480, 420);
      this.tabControl.ResumeLayout(false);
      this.tabPageScan.ResumeLayout(false);
      this.tabPageScan.PerformLayout();
      this.ResumeLayout(false);

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
    private MPTabControl tabControl;
    private MPTabPage tabPageScan;
    private MPLabel labelScanMode;
    private MPComboBox comboBoxScanMode;
  }
}