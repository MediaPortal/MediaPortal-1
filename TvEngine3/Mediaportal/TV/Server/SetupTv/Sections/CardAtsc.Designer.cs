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
      this.progressBarQuality = new System.Windows.Forms.ProgressBar();
      this.progressBarLevel = new System.Windows.Forms.ProgressBar();
      this.label2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.label1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.mpButtonScanTv = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.listViewStatus = new System.Windows.Forms.ListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpLabel1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpComboBoxFrequencies = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.mpTabControl1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabControl();
      this.tabPageScan = new System.Windows.Forms.TabPage();
      this.mpLabel2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpComboBoxTuningMode = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.tabPageCaMenu = new System.Windows.Forms.TabPage();
      this.mpTabControl1.SuspendLayout();
      this.tabPageScan.SuspendLayout();
      this.SuspendLayout();
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarQuality.Location = new System.Drawing.Point(121, 159);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(402, 10);
      this.progressBarQuality.TabIndex = 8;
      // 
      // progressBarLevel
      // 
      this.progressBarLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarLevel.Location = new System.Drawing.Point(121, 139);
      this.progressBarLevel.Name = "progressBarLevel";
      this.progressBarLevel.Size = new System.Drawing.Size(402, 10);
      this.progressBarLevel.TabIndex = 6;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(19, 158);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 13);
      this.label2.TabIndex = 7;
      this.label2.Text = "Signal quality:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(19, 138);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 5;
      this.label1.Text = "Signal level:";
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(22, 189);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(501, 10);
      this.progressBar1.TabIndex = 9;
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Location = new System.Drawing.Point(392, 69);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(131, 23);
      this.mpButtonScanTv.TabIndex = 4;
      this.mpButtonScanTv.Text = "Scan for channels";
      this.mpButtonScanTv.UseVisualStyleBackColor = true;
      this.mpButtonScanTv.Click += new System.EventHandler(this.mpButtonScanTv_Click);
      // 
      // listViewStatus
      // 
      this.listViewStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.listViewStatus.Location = new System.Drawing.Point(22, 218);
      this.listViewStatus.Name = "listViewStatus";
      this.listViewStatus.Size = new System.Drawing.Size(501, 168);
      this.listViewStatus.TabIndex = 10;
      this.listViewStatus.UseCompatibleStateImageBehavior = false;
      this.listViewStatus.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Status";
      this.columnHeader1.Width = 350;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(19, 45);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(124, 13);
      this.mpLabel1.TabIndex = 2;
      this.mpLabel1.Text = "QAM tuning frequencies:";
      // 
      // mpComboBoxFrequencies
      // 
      this.mpComboBoxFrequencies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxFrequencies.FormattingEnabled = true;
      this.mpComboBoxFrequencies.Location = new System.Drawing.Point(176, 42);
      this.mpComboBoxFrequencies.Name = "mpComboBoxFrequencies";
      this.mpComboBoxFrequencies.Size = new System.Drawing.Size(347, 21);
      this.mpComboBoxFrequencies.TabIndex = 3;
      // 
      // mpTabControl1
      // 
      this.mpTabControl1.AllowDrop = true;
      this.mpTabControl1.AllowReorderTabs = false;
      this.mpTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpTabControl1.Controls.Add(this.tabPageScan);
      this.mpTabControl1.Controls.Add(this.tabPageCaMenu);
      this.mpTabControl1.Location = new System.Drawing.Point(4, 4);
      this.mpTabControl1.Name = "mpTabControl1";
      this.mpTabControl1.SelectedIndex = 0;
      this.mpTabControl1.Size = new System.Drawing.Size(558, 433);
      this.mpTabControl1.TabIndex = 0;
      // 
      // tabPageScan
      // 
      this.tabPageScan.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageScan.Controls.Add(this.mpLabel2);
      this.tabPageScan.Controls.Add(this.mpComboBoxTuningMode);
      this.tabPageScan.Controls.Add(this.progressBarLevel);
      this.tabPageScan.Controls.Add(this.mpLabel1);
      this.tabPageScan.Controls.Add(this.progressBar1);
      this.tabPageScan.Controls.Add(this.mpComboBoxFrequencies);
      this.tabPageScan.Controls.Add(this.progressBarQuality);
      this.tabPageScan.Controls.Add(this.mpButtonScanTv);
      this.tabPageScan.Controls.Add(this.listViewStatus);
      this.tabPageScan.Controls.Add(this.label2);
      this.tabPageScan.Controls.Add(this.label1);
      this.tabPageScan.Location = new System.Drawing.Point(4, 22);
      this.tabPageScan.Name = "tabPageScan";
      this.tabPageScan.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageScan.Size = new System.Drawing.Size(550, 407);
      this.tabPageScan.TabIndex = 0;
      this.tabPageScan.Text = "Scanning";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(19, 18);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(72, 13);
      this.mpLabel2.TabIndex = 0;
      this.mpLabel2.Text = "Tuning mode:";
      // 
      // mpComboBoxTuningMode
      // 
      this.mpComboBoxTuningMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxTuningMode.FormattingEnabled = true;
      this.mpComboBoxTuningMode.Items.AddRange(new object[] {
            "ATSC Digital Terrestrial",
            "Clear QAM Cable",
            "Digital Cable"});
      this.mpComboBoxTuningMode.Location = new System.Drawing.Point(176, 15);
      this.mpComboBoxTuningMode.Name = "mpComboBoxTuningMode";
      this.mpComboBoxTuningMode.Size = new System.Drawing.Size(347, 21);
      this.mpComboBoxTuningMode.TabIndex = 1;
      // 
      // tabPageCaMenu
      // 
      this.tabPageCaMenu.Location = new System.Drawing.Point(4, 22);
      this.tabPageCaMenu.Name = "tabPageCaMenu";
      this.tabPageCaMenu.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageCaMenu.Size = new System.Drawing.Size(550, 407);
      this.tabPageCaMenu.TabIndex = 1;
      this.tabPageCaMenu.Text = "CI Menu";
      this.tabPageCaMenu.UseVisualStyleBackColor = true;
      // 
      // CardAtsc
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpTabControl1);
      this.Name = "CardAtsc";
      this.Size = new System.Drawing.Size(565, 441);
      this.mpTabControl1.ResumeLayout(false);
      this.tabPageScan.ResumeLayout(false);
      this.tabPageScan.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ProgressBar progressBarQuality;
    private System.Windows.Forms.ProgressBar progressBarLevel;
    private MPLabel label2;
    private MPLabel label1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private MPButton mpButtonScanTv;
    private System.Windows.Forms.ListView listViewStatus;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private MPLabel mpLabel1;
    private MPComboBox mpComboBoxFrequencies;
    private MPTabControl mpTabControl1;
    private System.Windows.Forms.TabPage tabPageScan;
    private System.Windows.Forms.TabPage tabPageCaMenu;
    private MPLabel mpLabel2;
    private MPComboBox mpComboBoxTuningMode;
  }
}