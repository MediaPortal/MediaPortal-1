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
      this.checkBoxCreateGroups = new System.Windows.Forms.CheckBox();
      this.listViewStatus = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.progressBarQuality = new System.Windows.Forms.ProgressBar();
      this.progressBarLevel = new System.Windows.Forms.ProgressBar();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.mpButtonScanTv = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxRegion = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpBeveledLine2 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.mpComboBoxCountry = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.tabPageCIMenu = new System.Windows.Forms.TabPage();
      this.tabControl1.SuspendLayout();
      this.tabPageScan.SuspendLayout();
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
      this.tabPageScan.Controls.Add(this.checkBoxCreateGroups);
      this.tabPageScan.Controls.Add(this.listViewStatus);
      this.tabPageScan.Controls.Add(this.progressBarQuality);
      this.tabPageScan.Controls.Add(this.progressBarLevel);
      this.tabPageScan.Controls.Add(this.label2);
      this.tabPageScan.Controls.Add(this.label1);
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
      // checkBoxCreateGroups
      // 
      this.checkBoxCreateGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkBoxCreateGroups.AutoSize = true;
      this.checkBoxCreateGroups.Location = new System.Drawing.Point(17, 412);
      this.checkBoxCreateGroups.Name = "checkBoxCreateGroups";
      this.checkBoxCreateGroups.Size = new System.Drawing.Size(175, 17);
      this.checkBoxCreateGroups.TabIndex = 84;
      this.checkBoxCreateGroups.Text = "Create groups for each provider";
      this.checkBoxCreateGroups.UseVisualStyleBackColor = true;
      // 
      // listViewStatus
      // 
      this.listViewStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.listViewStatus.Location = new System.Drawing.Point(17, 207);
      this.listViewStatus.Name = "listViewStatus";
      this.listViewStatus.Size = new System.Drawing.Size(430, 122);
      this.listViewStatus.TabIndex = 83;
      this.listViewStatus.UseCompatibleStateImageBehavior = false;
      this.listViewStatus.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Status";
      this.columnHeader1.Width = 350;
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarQuality.Location = new System.Drawing.Point(105, 127);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(331, 10);
      this.progressBarQuality.TabIndex = 82;
      // 
      // progressBarLevel
      // 
      this.progressBarLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarLevel.Location = new System.Drawing.Point(105, 104);
      this.progressBarLevel.Name = "progressBarLevel";
      this.progressBarLevel.Size = new System.Drawing.Size(331, 10);
      this.progressBarLevel.TabIndex = 81;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(15, 124);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 13);
      this.label2.TabIndex = 80;
      this.label2.Text = "Signal quality:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(15, 101);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 79;
      this.label1.Text = "Signal level:";
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(18, 177);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(427, 10);
      this.progressBar1.TabIndex = 78;
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonScanTv.Location = new System.Drawing.Point(316, 335);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(131, 23);
      this.mpButtonScanTv.TabIndex = 75;
      this.mpButtonScanTv.Text = "Scan for channels";
      this.mpButtonScanTv.UseVisualStyleBackColor = true;
      this.mpButtonScanTv.Click += new System.EventHandler(this.mpButtonScanTv_Click);
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(15, 53);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(88, 13);
      this.mpLabel2.TabIndex = 77;
      this.mpLabel2.Text = "Region/Provider:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(15, 23);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(46, 13);
      this.mpLabel1.TabIndex = 77;
      this.mpLabel1.Text = "Country:";
      // 
      // mpComboBoxRegion
      // 
      this.mpComboBoxRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxRegion.FormattingEnabled = true;
      this.mpComboBoxRegion.Location = new System.Drawing.Point(105, 50);
      this.mpComboBoxRegion.Name = "mpComboBoxRegion";
      this.mpComboBoxRegion.Size = new System.Drawing.Size(328, 21);
      this.mpComboBoxRegion.TabIndex = 74;
      // 
      // mpBeveledLine2
      // 
      this.mpBeveledLine2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpBeveledLine2.Location = new System.Drawing.Point(17, 82);
      this.mpBeveledLine2.Name = "mpBeveledLine2";
      this.mpBeveledLine2.Size = new System.Drawing.Size(426, 64);
      this.mpBeveledLine2.TabIndex = 76;
      // 
      // mpComboBoxCountry
      // 
      this.mpComboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCountry.FormattingEnabled = true;
      this.mpComboBoxCountry.Location = new System.Drawing.Point(105, 20);
      this.mpComboBoxCountry.Name = "mpComboBoxCountry";
      this.mpComboBoxCountry.Size = new System.Drawing.Size(328, 21);
      this.mpComboBoxCountry.TabIndex = 74;
      // 
      // tabPageCIMenu
      // 
      this.tabPageCIMenu.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageCIMenu.Location = new System.Drawing.Point(4, 22);
      this.tabPageCIMenu.Name = "tabPageCIMenu";
      this.tabPageCIMenu.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageCIMenu.Size = new System.Drawing.Size(454, 388);
      this.tabPageCIMenu.TabIndex = 1;
      this.tabPageCIMenu.Text = "CI Menu";
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

  }
}