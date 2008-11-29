namespace SetupTv.Sections
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
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.mpButtonScanTv = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewStatus = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.checkBoxQAM = new System.Windows.Forms.CheckBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxFrequencies = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.SuspendLayout();
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarQuality.Location = new System.Drawing.Point(111, 127);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(328, 10);
      this.progressBarQuality.TabIndex = 59;
      // 
      // progressBarLevel
      // 
      this.progressBarLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarLevel.Location = new System.Drawing.Point(111, 104);
      this.progressBarLevel.Name = "progressBarLevel";
      this.progressBarLevel.Size = new System.Drawing.Size(328, 10);
      this.progressBarLevel.TabIndex = 58;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(21, 124);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(74, 13);
      this.label2.TabIndex = 57;
      this.label2.Text = "Signal Quality:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(21, 101);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 56;
      this.label1.Text = "Signal level:";
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(24, 177);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(417, 10);
      this.progressBar1.TabIndex = 55;
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonScanTv.Location = new System.Drawing.Point(319, 354);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(131, 23);
      this.mpButtonScanTv.TabIndex = 50;
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
      this.listViewStatus.Location = new System.Drawing.Point(16, 226);
      this.listViewStatus.Name = "listViewStatus";
      this.listViewStatus.Size = new System.Drawing.Size(427, 122);
      this.listViewStatus.TabIndex = 69;
      this.listViewStatus.UseCompatibleStateImageBehavior = false;
      this.listViewStatus.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Status";
      this.columnHeader1.Width = 350;
      // 
      // checkBoxQAM
      // 
      this.checkBoxQAM.AutoSize = true;
      this.checkBoxQAM.Location = new System.Drawing.Point(24, 18);
      this.checkBoxQAM.Name = "checkBoxQAM";
      this.checkBoxQAM.Size = new System.Drawing.Size(320, 17);
      this.checkBoxQAM.TabIndex = 70;
      this.checkBoxQAM.Text = "Tune QAM instead of ATSC (Requires a cable TV connection)";
      this.checkBoxQAM.UseVisualStyleBackColor = true;
      this.checkBoxQAM.CheckedChanged += new System.EventHandler(this.checkBoxQAM_CheckedChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(21, 48);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(124, 13);
      this.mpLabel1.TabIndex = 72;
      this.mpLabel1.Text = "QAM tuning frequencies:";
      // 
      // mpComboBoxFrequencies
      // 
      this.mpComboBoxFrequencies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxFrequencies.FormattingEnabled = true;
      this.mpComboBoxFrequencies.Location = new System.Drawing.Point(166, 45);
      this.mpComboBoxFrequencies.Name = "mpComboBoxFrequencies";
      this.mpComboBoxFrequencies.Size = new System.Drawing.Size(273, 21);
      this.mpComboBoxFrequencies.TabIndex = 71;
      // 
      // CardAtsc
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpComboBoxFrequencies);
      this.Controls.Add(this.checkBoxQAM);
      this.Controls.Add(this.listViewStatus);
      this.Controls.Add(this.progressBarQuality);
      this.Controls.Add(this.progressBarLevel);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.mpButtonScanTv);
      this.Name = "CardAtsc";
      this.Size = new System.Drawing.Size(468, 397);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ProgressBar progressBarQuality;
    private System.Windows.Forms.ProgressBar progressBarLevel;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanTv;
    private System.Windows.Forms.ListView listViewStatus;
    private System.Windows.Forms.ColumnHeader columnHeader1;
      private System.Windows.Forms.CheckBox checkBoxQAM;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxFrequencies;
  }
}