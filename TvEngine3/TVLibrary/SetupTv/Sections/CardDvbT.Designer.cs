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
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxCreateGroups = new System.Windows.Forms.CheckBox();
      this.textBoxBandWidth = new System.Windows.Forms.TextBox();
      this.listViewStatus = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBarQuality = new System.Windows.Forms.ProgressBar();
      this.progressBarLevel = new System.Windows.Forms.ProgressBar();
      this.label2 = new System.Windows.Forms.Label();
      this.textBoxFreq = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.mpButtonScanTv = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxRegion = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpBeveledLine2 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.mpComboBoxCountry = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.tabPageCIMenu = new System.Windows.Forms.TabPage();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.mpButtonScanSingleTP = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.mpButtonSaveList = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonScanNIT = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabControl1.SuspendLayout();
      this.tabPageScan.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPageScan);
      this.tabControl1.Controls.Add(this.tabPageCIMenu);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(462, 414);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPageScan
      // 
      this.tabPageScan.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageScan.Controls.Add(this.groupBox2);
      this.tabPageScan.Controls.Add(this.groupBox1);
      this.tabPageScan.Controls.Add(this.mpLabel5);
      this.tabPageScan.Controls.Add(this.checkBoxCreateGroups);
      this.tabPageScan.Controls.Add(this.textBoxBandWidth);
      this.tabPageScan.Controls.Add(this.listViewStatus);
      this.tabPageScan.Controls.Add(this.mpLabel4);
      this.tabPageScan.Controls.Add(this.progressBarQuality);
      this.tabPageScan.Controls.Add(this.progressBarLevel);
      this.tabPageScan.Controls.Add(this.label2);
      this.tabPageScan.Controls.Add(this.textBoxFreq);
      this.tabPageScan.Controls.Add(this.label1);
      this.tabPageScan.Controls.Add(this.mpLabel3);
      this.tabPageScan.Controls.Add(this.progressBar1);
      this.tabPageScan.Controls.Add(this.mpButtonScanTv);
      this.tabPageScan.Controls.Add(this.mpLabel2);
      this.tabPageScan.Controls.Add(this.mpLabel1);
      this.tabPageScan.Controls.Add(this.mpComboBoxRegion);
      this.tabPageScan.Controls.Add(this.mpBeveledLine2);
      this.tabPageScan.Controls.Add(this.mpComboBoxCountry);
      this.tabPageScan.Location = new System.Drawing.Point(4, 22);
      this.tabPageScan.Name = "tabPageScan";
      this.tabPageScan.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageScan.Size = new System.Drawing.Size(454, 388);
      this.tabPageScan.TabIndex = 0;
      this.tabPageScan.Text = "Scanning";
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(163, 83);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(27, 13);
      this.mpLabel5.TabIndex = 106;
      this.mpLabel5.Text = "KHz";
      // 
      // checkBoxCreateGroups
      // 
      this.checkBoxCreateGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkBoxCreateGroups.AutoSize = true;
      this.checkBoxCreateGroups.Location = new System.Drawing.Point(17, 399);
      this.checkBoxCreateGroups.Name = "checkBoxCreateGroups";
      this.checkBoxCreateGroups.Size = new System.Drawing.Size(175, 17);
      this.checkBoxCreateGroups.TabIndex = 84;
      this.checkBoxCreateGroups.Text = "Create groups for each provider";
      this.checkBoxCreateGroups.UseVisualStyleBackColor = true;
      // 
      // textBoxBandWidth
      // 
      this.textBoxBandWidth.Location = new System.Drawing.Point(107, 103);
      this.textBoxBandWidth.MaxLength = 1;
      this.textBoxBandWidth.Name = "textBoxBandWidth";
      this.textBoxBandWidth.Size = new System.Drawing.Size(50, 20);
      this.textBoxBandWidth.TabIndex = 101;
      this.textBoxBandWidth.Text = "8";
      // 
      // listViewStatus
      // 
      this.listViewStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.listViewStatus.Location = new System.Drawing.Point(18, 249);
      this.listViewStatus.Name = "listViewStatus";
      this.listViewStatus.Size = new System.Drawing.Size(427, 122);
      this.listViewStatus.TabIndex = 83;
      this.listViewStatus.UseCompatibleStateImageBehavior = false;
      this.listViewStatus.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Status";
      this.columnHeader1.Width = 350;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(17, 106);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(60, 13);
      this.mpLabel4.TabIndex = 105;
      this.mpLabel4.Text = "Bandwidth:";
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarQuality.Location = new System.Drawing.Point(105, 217);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(328, 10);
      this.progressBarQuality.TabIndex = 82;
      // 
      // progressBarLevel
      // 
      this.progressBarLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarLevel.Location = new System.Drawing.Point(105, 194);
      this.progressBarLevel.Name = "progressBarLevel";
      this.progressBarLevel.Size = new System.Drawing.Size(328, 10);
      this.progressBarLevel.TabIndex = 81;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(15, 214);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 13);
      this.label2.TabIndex = 80;
      this.label2.Text = "Signal quality:";
      // 
      // textBoxFreq
      // 
      this.textBoxFreq.Location = new System.Drawing.Point(107, 80);
      this.textBoxFreq.MaxLength = 6;
      this.textBoxFreq.Name = "textBoxFreq";
      this.textBoxFreq.Size = new System.Drawing.Size(50, 20);
      this.textBoxFreq.TabIndex = 100;
      this.textBoxFreq.Text = "163000";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(15, 194);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 79;
      this.label1.Text = "Signal level:";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(17, 83);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(60, 13);
      this.mpLabel3.TabIndex = 104;
      this.mpLabel3.Text = "Frequency:";
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(18, 233);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(424, 10);
      this.progressBar1.TabIndex = 78;
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonScanTv.Location = new System.Drawing.Point(335, 31);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(113, 23);
      this.mpButtonScanTv.TabIndex = 75;
      this.mpButtonScanTv.Text = "Scan for channels";
      this.mpButtonScanTv.UseVisualStyleBackColor = true;
      this.mpButtonScanTv.Click += new System.EventHandler(this.mpButtonScanTv_Click);
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(15, 36);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(88, 13);
      this.mpLabel2.TabIndex = 77;
      this.mpLabel2.Text = "Region/Provider:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(15, 9);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(46, 13);
      this.mpLabel1.TabIndex = 77;
      this.mpLabel1.Text = "Country:";
      // 
      // mpComboBoxRegion
      // 
      this.mpComboBoxRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxRegion.FormattingEnabled = true;
      this.mpComboBoxRegion.Location = new System.Drawing.Point(105, 33);
      this.mpComboBoxRegion.Name = "mpComboBoxRegion";
      this.mpComboBoxRegion.Size = new System.Drawing.Size(223, 21);
      this.mpComboBoxRegion.TabIndex = 74;
      // 
      // mpBeveledLine2
      // 
      this.mpBeveledLine2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpBeveledLine2.Location = new System.Drawing.Point(6, 60);
      this.mpBeveledLine2.Name = "mpBeveledLine2";
      this.mpBeveledLine2.Size = new System.Drawing.Size(445, 114);
      this.mpBeveledLine2.TabIndex = 76;
      // 
      // mpComboBoxCountry
      // 
      this.mpComboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCountry.FormattingEnabled = true;
      this.mpComboBoxCountry.Location = new System.Drawing.Point(105, 6);
      this.mpComboBoxCountry.Name = "mpComboBoxCountry";
      this.mpComboBoxCountry.Size = new System.Drawing.Size(223, 21);
      this.mpComboBoxCountry.TabIndex = 74;
      // 
      // tabPageCIMenu
      // 
      this.tabPageCIMenu.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageCIMenu.Location = new System.Drawing.Point(4, 22);
      this.tabPageCIMenu.Name = "tabPageCIMenu";
      this.tabPageCIMenu.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageCIMenu.Size = new System.Drawing.Size(457, 388);
      this.tabPageCIMenu.TabIndex = 1;
      this.tabPageCIMenu.Text = "CI Menu";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.mpButtonScanSingleTP);
      this.groupBox2.Location = new System.Drawing.Point(196, 75);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(113, 78);
      this.groupBox2.TabIndex = 108;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Single Transponder";
      // 
      // mpButtonScanSingleTP
      // 
      this.mpButtonScanSingleTP.Location = new System.Drawing.Point(25, 16);
      this.mpButtonScanSingleTP.Name = "mpButtonScanSingleTP";
      this.mpButtonScanSingleTP.Size = new System.Drawing.Size(67, 23);
      this.mpButtonScanSingleTP.TabIndex = 7;
      this.mpButtonScanSingleTP.Text = "Scan";
      this.mpButtonScanSingleTP.UseVisualStyleBackColor = true;
      this.mpButtonScanSingleTP.Click += new System.EventHandler(this.mpButtonScanSingleTP_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.mpButtonSaveList);
      this.groupBox1.Controls.Add(this.mpButtonScanNIT);
      this.groupBox1.Location = new System.Drawing.Point(315, 75);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(129, 78);
      this.groupBox1.TabIndex = 107;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Transponder List (NIT)";
      // 
      // mpButtonSaveList
      // 
      this.mpButtonSaveList.Location = new System.Drawing.Point(20, 45);
      this.mpButtonSaveList.Name = "mpButtonSaveList";
      this.mpButtonSaveList.Size = new System.Drawing.Size(69, 23);
      this.mpButtonSaveList.TabIndex = 7;
      this.mpButtonSaveList.Text = "Save";
      this.mpButtonSaveList.UseVisualStyleBackColor = true;
      this.mpButtonSaveList.Click += new System.EventHandler(this.mpButtonSaveList_Click);
      // 
      // mpButtonScanNIT
      // 
      this.mpButtonScanNIT.Location = new System.Drawing.Point(20, 16);
      this.mpButtonScanNIT.Name = "mpButtonScanNIT";
      this.mpButtonScanNIT.Size = new System.Drawing.Size(69, 23);
      this.mpButtonScanNIT.TabIndex = 6;
      this.mpButtonScanNIT.Text = "Scan";
      this.mpButtonScanNIT.UseVisualStyleBackColor = true;
      this.mpButtonScanNIT.Click += new System.EventHandler(this.mpButtonManualScan_Click);
      // 
      // CardDvbT
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "CardDvbT";
      this.Size = new System.Drawing.Size(468, 420);
      this.Load += new System.EventHandler(this.CardDvbT_Load);
      this.tabControl1.ResumeLayout(false);
      this.tabPageScan.ResumeLayout(false);
      this.tabPageScan.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPageScan;
    private System.Windows.Forms.CheckBox checkBoxCreateGroups;
    private System.Windows.Forms.ListView listViewStatus;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ProgressBar progressBarQuality;
    private System.Windows.Forms.ProgressBar progressBarLevel;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanTv;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxCountry;
    private System.Windows.Forms.TabPage tabPageCIMenu;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxRegion;
    private MediaPortal.UserInterface.Controls.MPBeveledLine mpBeveledLine2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
    private System.Windows.Forms.TextBox textBoxBandWidth;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private System.Windows.Forms.TextBox textBoxFreq;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private System.Windows.Forms.GroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanSingleTP;
    private System.Windows.Forms.GroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonSaveList;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanNIT;

  }
}