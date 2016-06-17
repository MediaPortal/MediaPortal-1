using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class CardDvbIP
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
      this.labelService = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxService = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.progressBarSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.progressBarProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.progressBarSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.buttonScan = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.listViewProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderStatus = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.labelSignalQuality = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelSignalStrength = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelStream = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxStream = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.SuspendLayout();
      // 
      // labelService
      // 
      this.labelService.AutoSize = true;
      this.labelService.Location = new System.Drawing.Point(19, 18);
      this.labelService.Name = "labelService";
      this.labelService.Size = new System.Drawing.Size(46, 13);
      this.labelService.TabIndex = 0;
      this.labelService.Text = "Service:";
      // 
      // comboBoxService
      // 
      this.comboBoxService.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxService.FormattingEnabled = true;
      this.comboBoxService.Location = new System.Drawing.Point(105, 15);
      this.comboBoxService.Name = "comboBoxService";
      this.comboBoxService.Size = new System.Drawing.Size(351, 21);
      this.comboBoxService.TabIndex = 1;
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
      // progressBarProgress
      // 
      this.progressBarProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarProgress.Location = new System.Drawing.Point(22, 145);
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(434, 10);
      this.progressBarProgress.TabIndex = 9;
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
      // buttonScan
      // 
      this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(346, 69);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(110, 23);
      this.buttonScan.TabIndex = 4;
      this.buttonScan.Text = "&Scan for channels";
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
      // labelStream
      // 
      this.labelStream.AutoSize = true;
      this.labelStream.Location = new System.Drawing.Point(19, 45);
      this.labelStream.Name = "labelStream";
      this.labelStream.Size = new System.Drawing.Size(43, 13);
      this.labelStream.TabIndex = 2;
      this.labelStream.Text = "Stream:";
      // 
      // comboBoxStream
      // 
      this.comboBoxStream.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxStream.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxStream.FormattingEnabled = true;
      this.comboBoxStream.Location = new System.Drawing.Point(105, 42);
      this.comboBoxStream.Name = "comboBoxStream";
      this.comboBoxStream.Size = new System.Drawing.Size(351, 21);
      this.comboBoxStream.TabIndex = 3;
      // 
      // CardDvbIP
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.labelStream);
      this.Controls.Add(this.comboBoxStream);
      this.Controls.Add(this.labelService);
      this.Controls.Add(this.comboBoxService);
      this.Controls.Add(this.progressBarSignalStrength);
      this.Controls.Add(this.progressBarProgress);
      this.Controls.Add(this.progressBarSignalQuality);
      this.Controls.Add(this.buttonScan);
      this.Controls.Add(this.listViewProgress);
      this.Controls.Add(this.labelSignalQuality);
      this.Controls.Add(this.labelSignalStrength);
      this.Name = "CardDvbIP";
      this.Size = new System.Drawing.Size(480, 420);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPLabel labelService;
    private MPComboBox comboBoxService;
    private MPProgressBar progressBarSignalStrength;
    private MPProgressBar progressBarProgress;
    private MPProgressBar progressBarSignalQuality;
    private MPButton buttonScan;
    private MPListView listViewProgress;
    private MPColumnHeader columnHeaderStatus;
    private MPLabel labelSignalQuality;
    private MPLabel labelSignalStrength;
    private MPLabel labelStream;
    private MPComboBox comboBoxStream;
  }
}
