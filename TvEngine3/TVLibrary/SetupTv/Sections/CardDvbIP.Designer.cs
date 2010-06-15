namespace SetupTv.Sections
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
      this.checkBoxCreateGroups = new System.Windows.Forms.CheckBox();
      this.listViewStatus = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.progressBarQuality = new System.Windows.Forms.ProgressBar();
      this.progressBarLevel = new System.Windows.Forms.ProgressBar();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.mpButtonScanTv = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxService = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpBeveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.SuspendLayout();
      // 
      // checkBoxCreateGroups
      // 
      this.checkBoxCreateGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkBoxCreateGroups.AutoSize = true;
      this.checkBoxCreateGroups.Location = new System.Drawing.Point(23, 320);
      this.checkBoxCreateGroups.Name = "checkBoxCreateGroups";
      this.checkBoxCreateGroups.Size = new System.Drawing.Size(175, 17);
      this.checkBoxCreateGroups.TabIndex = 86;
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
      this.listViewStatus.Location = new System.Drawing.Point(23, 192);
      this.listViewStatus.Name = "listViewStatus";
      this.listViewStatus.Size = new System.Drawing.Size(427, 122);
      this.listViewStatus.TabIndex = 84;
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
      this.progressBarQuality.Location = new System.Drawing.Point(111, 112);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(328, 10);
      this.progressBarQuality.TabIndex = 83;
      // 
      // progressBarLevel
      // 
      this.progressBarLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarLevel.Location = new System.Drawing.Point(111, 89);
      this.progressBarLevel.Name = "progressBarLevel";
      this.progressBarLevel.Size = new System.Drawing.Size(328, 10);
      this.progressBarLevel.TabIndex = 82;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(21, 109);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(74, 13);
      this.label2.TabIndex = 81;
      this.label2.Text = "Signal Quality:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(21, 86);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 80;
      this.label1.Text = "Signal level:";
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(24, 162);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(424, 10);
      this.progressBar1.TabIndex = 79;
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonScanTv.Location = new System.Drawing.Point(319, 320);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(131, 23);
      this.mpButtonScanTv.TabIndex = 76;
      this.mpButtonScanTv.Text = "Scan for channels";
      this.mpButtonScanTv.UseVisualStyleBackColor = true;
      this.mpButtonScanTv.Click += new System.EventHandler(this.mpButtonScanTv_Click);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(28, 32);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(46, 13);
      this.mpLabel1.TabIndex = 78;
      this.mpLabel1.Text = "Service:";
      // 
      // mpComboBoxService
      // 
      this.mpComboBoxService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxService.FormattingEnabled = true;
      this.mpComboBoxService.Location = new System.Drawing.Point(80, 25);
      this.mpComboBoxService.Name = "mpComboBoxService";
      this.mpComboBoxService.Size = new System.Drawing.Size(175, 21);
      this.mpComboBoxService.TabIndex = 74;
      // 
      // mpBeveledLine1
      // 
      this.mpBeveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpBeveledLine1.Location = new System.Drawing.Point(16, 16);
      this.mpBeveledLine1.Name = "mpBeveledLine1";
      this.mpBeveledLine1.Size = new System.Drawing.Size(423, 43);
      this.mpBeveledLine1.TabIndex = 77;
      // 
      // CardDvbIP
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.checkBoxCreateGroups);
      this.Controls.Add(this.listViewStatus);
      this.Controls.Add(this.progressBarQuality);
      this.Controls.Add(this.progressBarLevel);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.mpButtonScanTv);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpComboBoxService);
      this.Controls.Add(this.mpBeveledLine1);
      this.Name = "CardDvbIP";
      this.Size = new System.Drawing.Size(468, 420);
      this.Load += new System.EventHandler(this.CardDvbIP_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

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
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxService;
    private MediaPortal.UserInterface.Controls.MPBeveledLine mpBeveledLine1;

  }
}
