namespace SetupTv.Sections
{
  partial class CardDvbT
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
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPageScan = new System.Windows.Forms.TabPage();
      this.checkBoxAdvancedTuning = new System.Windows.Forms.CheckBox();
      this.mpGrpAdvancedTuning = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.scanNIT = new System.Windows.Forms.RadioButton();
      this.scanSingleTransponder = new System.Windows.Forms.RadioButton();
      this.scanPredefProvider = new System.Windows.Forms.RadioButton();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxFreq = new System.Windows.Forms.TextBox();
      this.textBoxBandwidth = new System.Windows.Forms.TextBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGrpScanProgress = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.progressBarLevel = new System.Windows.Forms.ProgressBar();
      this.progressBarQuality = new System.Windows.Forms.ProgressBar();
      this.listViewStatus = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.checkBoxCreateGroups = new System.Windows.Forms.CheckBox();
      this.mpButtonScanTv = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxRegion = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpComboBoxCountry = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.tabPageCIMenu = new System.Windows.Forms.TabPage();
      this.tabControl1.SuspendLayout();
      this.tabPageScan.SuspendLayout();
      this.mpGrpAdvancedTuning.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.mpGrpScanProgress.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPageScan);
      this.tabControl1.Controls.Add(this.tabPageCIMenu);
      this.tabControl1.Location = new System.Drawing.Point(4, 4);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(558, 433);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPageScan
      // 
      this.tabPageScan.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageScan.Controls.Add(this.checkBoxAdvancedTuning);
      this.tabPageScan.Controls.Add(this.mpGrpAdvancedTuning);
      this.tabPageScan.Controls.Add(this.mpGrpScanProgress);
      this.tabPageScan.Controls.Add(this.checkBoxCreateGroups);
      this.tabPageScan.Controls.Add(this.mpButtonScanTv);
      this.tabPageScan.Controls.Add(this.mpLabel6);
      this.tabPageScan.Controls.Add(this.mpLabel1);
      this.tabPageScan.Controls.Add(this.mpComboBoxRegion);
      this.tabPageScan.Controls.Add(this.mpComboBoxCountry);
      this.tabPageScan.Location = new System.Drawing.Point(4, 22);
      this.tabPageScan.Name = "tabPageScan";
      this.tabPageScan.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageScan.Size = new System.Drawing.Size(550, 407);
      this.tabPageScan.TabIndex = 0;
      this.tabPageScan.Text = "Scanning";
      // 
      // checkBoxAdvancedTuning
      // 
      this.checkBoxAdvancedTuning.AutoSize = true;
      this.checkBoxAdvancedTuning.Location = new System.Drawing.Point(13, 85);
      this.checkBoxAdvancedTuning.Name = "checkBoxAdvancedTuning";
      this.checkBoxAdvancedTuning.Size = new System.Drawing.Size(165, 17);
      this.checkBoxAdvancedTuning.TabIndex = 104;
      this.checkBoxAdvancedTuning.Text = "Use advanced tuning options";
      this.checkBoxAdvancedTuning.UseVisualStyleBackColor = true;
      this.checkBoxAdvancedTuning.CheckedChanged += new System.EventHandler(this.UpdateGUIControls);
      // 
      // mpGrpAdvancedTuning
      // 
      this.mpGrpAdvancedTuning.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGrpAdvancedTuning.Controls.Add(this.mpLabel2);
      this.mpGrpAdvancedTuning.Controls.Add(this.groupBox2);
      this.mpGrpAdvancedTuning.Controls.Add(this.mpLabel5);
      this.mpGrpAdvancedTuning.Controls.Add(this.textBoxFreq);
      this.mpGrpAdvancedTuning.Controls.Add(this.textBoxBandwidth);
      this.mpGrpAdvancedTuning.Controls.Add(this.mpLabel4);
      this.mpGrpAdvancedTuning.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGrpAdvancedTuning.Location = new System.Drawing.Point(2, 257);
      this.mpGrpAdvancedTuning.Name = "mpGrpAdvancedTuning";
      this.mpGrpAdvancedTuning.Size = new System.Drawing.Size(542, 123);
      this.mpGrpAdvancedTuning.TabIndex = 103;
      this.mpGrpAdvancedTuning.TabStop = false;
      this.mpGrpAdvancedTuning.Text = "Advanced tuning options";
      this.mpGrpAdvancedTuning.Visible = false;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(8, 31);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(60, 13);
      this.mpLabel2.TabIndex = 92;
      this.mpLabel2.Text = "Frequency:";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.scanNIT);
      this.groupBox2.Controls.Add(this.scanSingleTransponder);
      this.groupBox2.Controls.Add(this.scanPredefProvider);
      this.groupBox2.Location = new System.Drawing.Point(267, 16);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(174, 89);
      this.groupBox2.TabIndex = 101;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Scan type";
      // 
      // scanNIT
      // 
      this.scanNIT.AutoSize = true;
      this.scanNIT.Location = new System.Drawing.Point(7, 62);
      this.scanNIT.Name = "scanNIT";
      this.scanNIT.Size = new System.Drawing.Size(146, 17);
      this.scanNIT.TabIndex = 0;
      this.scanNIT.Text = "search for transponder list";
      this.scanNIT.UseVisualStyleBackColor = true;
      this.scanNIT.CheckedChanged += new System.EventHandler(this.UpdateGUIControls);
      // 
      // scanSingleTransponder
      // 
      this.scanSingleTransponder.AutoSize = true;
      this.scanSingleTransponder.Location = new System.Drawing.Point(7, 39);
      this.scanSingleTransponder.Name = "scanSingleTransponder";
      this.scanSingleTransponder.Size = new System.Drawing.Size(111, 17);
      this.scanSingleTransponder.TabIndex = 0;
      this.scanSingleTransponder.Text = "single transponder";
      this.scanSingleTransponder.UseVisualStyleBackColor = true;
      this.scanSingleTransponder.CheckedChanged += new System.EventHandler(this.UpdateGUIControls);
      // 
      // scanPredefProvider
      // 
      this.scanPredefProvider.AutoSize = true;
      this.scanPredefProvider.Checked = true;
      this.scanPredefProvider.Location = new System.Drawing.Point(7, 16);
      this.scanPredefProvider.Name = "scanPredefProvider";
      this.scanPredefProvider.Size = new System.Drawing.Size(116, 17);
      this.scanPredefProvider.TabIndex = 0;
      this.scanPredefProvider.TabStop = true;
      this.scanPredefProvider.Text = "predefined provider";
      this.scanPredefProvider.UseVisualStyleBackColor = true;
      this.scanPredefProvider.CheckedChanged += new System.EventHandler(this.UpdateGUIControls);
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(154, 31);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(27, 13);
      this.mpLabel5.TabIndex = 99;
      this.mpLabel5.Text = "KHz";
      // 
      // textBoxFreq
      // 
      this.textBoxFreq.Location = new System.Drawing.Point(98, 28);
      this.textBoxFreq.MaxLength = 6;
      this.textBoxFreq.Name = "textBoxFreq";
      this.textBoxFreq.Size = new System.Drawing.Size(50, 20);
      this.textBoxFreq.TabIndex = 3;
      this.textBoxFreq.Text = "163000";
      // 
      // textBoxBandwidth
      // 
      this.textBoxBandwidth.Location = new System.Drawing.Point(98, 51);
      this.textBoxBandwidth.MaxLength = 2;
      this.textBoxBandwidth.Name = "textBoxBandwidth";
      this.textBoxBandwidth.Size = new System.Drawing.Size(50, 20);
      this.textBoxBandwidth.TabIndex = 4;
      this.textBoxBandwidth.Text = "8";
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(8, 54);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(60, 13);
      this.mpLabel4.TabIndex = 97;
      this.mpLabel4.Text = "Bandwidth:";
      // 
      // mpGrpScanProgress
      // 
      this.mpGrpScanProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGrpScanProgress.Controls.Add(this.mpLabel3);
      this.mpGrpScanProgress.Controls.Add(this.progressBar1);
      this.mpGrpScanProgress.Controls.Add(this.label1);
      this.mpGrpScanProgress.Controls.Add(this.label2);
      this.mpGrpScanProgress.Controls.Add(this.progressBarLevel);
      this.mpGrpScanProgress.Controls.Add(this.progressBarQuality);
      this.mpGrpScanProgress.Controls.Add(this.listViewStatus);
      this.mpGrpScanProgress.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGrpScanProgress.Location = new System.Drawing.Point(2, 108);
      this.mpGrpScanProgress.Name = "mpGrpScanProgress";
      this.mpGrpScanProgress.Size = new System.Drawing.Size(545, 293);
      this.mpGrpScanProgress.TabIndex = 102;
      this.mpGrpScanProgress.TabStop = false;
      this.mpGrpScanProgress.Text = "Scan progress";
      this.mpGrpScanProgress.Visible = false;
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(8, 22);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(85, 13);
      this.mpLabel3.TabIndex = 84;
      this.mpLabel3.Text = "Current channel:";
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(12, 93);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(530, 10);
      this.progressBar1.TabIndex = 85;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(8, 43);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 86;
      this.label1.Text = "Signal level:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(8, 66);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 13);
      this.label2.TabIndex = 87;
      this.label2.Text = "Signal quality:";
      // 
      // progressBarLevel
      // 
      this.progressBarLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarLevel.Location = new System.Drawing.Point(98, 46);
      this.progressBarLevel.Name = "progressBarLevel";
      this.progressBarLevel.Size = new System.Drawing.Size(444, 10);
      this.progressBarLevel.TabIndex = 88;
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarQuality.Location = new System.Drawing.Point(98, 69);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(444, 10);
      this.progressBarQuality.TabIndex = 89;
      // 
      // listViewStatus
      // 
      this.listViewStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.listViewStatus.Location = new System.Drawing.Point(1, 109);
      this.listViewStatus.Name = "listViewStatus";
      this.listViewStatus.Size = new System.Drawing.Size(541, 178);
      this.listViewStatus.TabIndex = 90;
      this.listViewStatus.UseCompatibleStateImageBehavior = false;
      this.listViewStatus.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Status";
      this.columnHeader1.Width = 450;
      // 
      // checkBoxCreateGroups
      // 
      this.checkBoxCreateGroups.AutoSize = true;
      this.checkBoxCreateGroups.Location = new System.Drawing.Point(13, 62);
      this.checkBoxCreateGroups.Name = "checkBoxCreateGroups";
      this.checkBoxCreateGroups.Size = new System.Drawing.Size(175, 17);
      this.checkBoxCreateGroups.TabIndex = 8;
      this.checkBoxCreateGroups.Text = "Create groups for each provider";
      this.checkBoxCreateGroups.UseVisualStyleBackColor = true;
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Location = new System.Drawing.Point(336, 28);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(107, 23);
      this.mpButtonScanTv.TabIndex = 2;
      this.mpButtonScanTv.Text = "Scan for channels";
      this.mpButtonScanTv.UseVisualStyleBackColor = true;
      this.mpButtonScanTv.Click += new System.EventHandler(this.mpButtonScanTv_Click_1);
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(10, 33);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(88, 13);
      this.mpLabel6.TabIndex = 83;
      this.mpLabel6.Text = "Region/Provider:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(10, 6);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(46, 13);
      this.mpLabel1.TabIndex = 83;
      this.mpLabel1.Text = "Country:";
      // 
      // mpComboBoxRegion
      // 
      this.mpComboBoxRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxRegion.FormattingEnabled = true;
      this.mpComboBoxRegion.Location = new System.Drawing.Point(100, 30);
      this.mpComboBoxRegion.Name = "mpComboBoxRegion";
      this.mpComboBoxRegion.Size = new System.Drawing.Size(224, 21);
      this.mpComboBoxRegion.TabIndex = 1;
      // 
      // mpComboBoxCountry
      // 
      this.mpComboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCountry.FormattingEnabled = true;
      this.mpComboBoxCountry.Location = new System.Drawing.Point(100, 3);
      this.mpComboBoxCountry.Name = "mpComboBoxCountry";
      this.mpComboBoxCountry.Size = new System.Drawing.Size(224, 21);
      this.mpComboBoxCountry.TabIndex = 0;
      // 
      // tabPageCIMenu
      // 
      this.tabPageCIMenu.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageCIMenu.Location = new System.Drawing.Point(4, 22);
      this.tabPageCIMenu.Name = "tabPageCIMenu";
      this.tabPageCIMenu.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageCIMenu.Size = new System.Drawing.Size(550, 407);
      this.tabPageCIMenu.TabIndex = 1;
      this.tabPageCIMenu.Text = "CI Menu";
      // 
      // CardDvbT
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "CardDvbT";
      this.Size = new System.Drawing.Size(565, 441);
      this.tabControl1.ResumeLayout(false);
      this.tabPageScan.ResumeLayout(false);
      this.tabPageScan.PerformLayout();
      this.mpGrpAdvancedTuning.ResumeLayout(false);
      this.mpGrpAdvancedTuning.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.mpGrpScanProgress.ResumeLayout(false);
      this.mpGrpScanProgress.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPageScan;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
    private System.Windows.Forms.TextBox textBoxBandwidth;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private System.Windows.Forms.TextBox textBoxFreq;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private System.Windows.Forms.CheckBox checkBoxCreateGroups;
    private System.Windows.Forms.ListView listViewStatus;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ProgressBar progressBarQuality;
    private System.Windows.Forms.ProgressBar progressBarLevel;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanTv;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxCountry;
    private System.Windows.Forms.TabPage tabPageCIMenu;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel6;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxRegion;
    private System.Windows.Forms.GroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGrpScanProgress;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGrpAdvancedTuning;
    private System.Windows.Forms.CheckBox checkBoxAdvancedTuning;
    private System.Windows.Forms.RadioButton scanNIT;
    private System.Windows.Forms.RadioButton scanSingleTransponder;
    private System.Windows.Forms.RadioButton scanPredefProvider;

  }
}