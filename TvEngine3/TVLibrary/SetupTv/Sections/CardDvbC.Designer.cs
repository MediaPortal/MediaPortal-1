namespace SetupTv.Sections
{
  partial class CardDvbC
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
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonScanTv = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpBeveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxCountry = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.listViewStatus = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBox1Cam = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxCreateGroups = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Location = new System.Drawing.Point(111, 146);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(328, 10);
      this.progressBarQuality.TabIndex = 44;
      // 
      // progressBarLevel
      // 
      this.progressBarLevel.Location = new System.Drawing.Point(111, 123);
      this.progressBarLevel.Name = "progressBarLevel";
      this.progressBarLevel.Size = new System.Drawing.Size(328, 10);
      this.progressBarLevel.TabIndex = 43;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(21, 143);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(74, 13);
      this.label2.TabIndex = 42;
      this.label2.Text = "Signal Quality:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(21, 120);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 41;
      this.label1.Text = "Signal level:";
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(16, 210);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(328, 10);
      this.progressBar1.TabIndex = 40;
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(21, 96);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(86, 13);
      this.mpLabel3.TabIndex = 38;
      this.mpLabel3.Text = "Current Channel:";
      // 
      // mpLabelChannel
      // 
      this.mpLabelChannel.AutoSize = true;
      this.mpLabelChannel.Location = new System.Drawing.Point(113, 96);
      this.mpLabelChannel.Name = "mpLabelChannel";
      this.mpLabelChannel.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannel.TabIndex = 39;
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Location = new System.Drawing.Point(319, 354);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(131, 23);
      this.mpButtonScanTv.TabIndex = 35;
      this.mpButtonScanTv.Text = "Scan for Tv Channels";
      this.mpButtonScanTv.UseVisualStyleBackColor = true;
      this.mpButtonScanTv.Click += new System.EventHandler(this.mpButtonScanTv_Click_1);
      // 
      // mpBeveledLine1
      // 
      this.mpBeveledLine1.Location = new System.Drawing.Point(16, 31);
      this.mpBeveledLine1.Name = "mpBeveledLine1";
      this.mpBeveledLine1.Size = new System.Drawing.Size(423, 43);
      this.mpBeveledLine1.TabIndex = 32;
      this.mpBeveledLine1.Load += new System.EventHandler(this.mpBeveledLine1_Load);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(28, 46);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(46, 13);
      this.mpLabel1.TabIndex = 34;
      this.mpLabel1.Text = "Country:";
      // 
      // mpComboBoxCountry
      // 
      this.mpComboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCountry.FormattingEnabled = true;
      this.mpComboBoxCountry.Location = new System.Drawing.Point(80, 39);
      this.mpComboBoxCountry.Name = "mpComboBoxCountry";
      this.mpComboBoxCountry.Size = new System.Drawing.Size(175, 21);
      this.mpComboBoxCountry.TabIndex = 33;
      this.mpComboBoxCountry.SelectedIndexChanged += new System.EventHandler(this.mpComboBoxCountry_SelectedIndexChanged);
      // 
      // listViewStatus
      // 
      this.listViewStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.listViewStatus.Location = new System.Drawing.Point(16, 226);
      this.listViewStatus.Name = "listViewStatus";
      this.listViewStatus.Size = new System.Drawing.Size(427, 122);
      this.listViewStatus.TabIndex = 68;
      this.listViewStatus.UseCompatibleStateImageBehavior = false;
      this.listViewStatus.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Status";
      this.columnHeader1.Width = 350;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(280, 46);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(33, 13);
      this.mpLabel2.TabIndex = 70;
      this.mpLabel2.Text = "CAM:";
      // 
      // mpComboBox1Cam
      // 
      this.mpComboBox1Cam.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBox1Cam.FormattingEnabled = true;
      this.mpComboBox1Cam.Items.AddRange(new object[] {
            "default",
            "viaccess",
            "aston",
            "conax",
            "cryptoworks"});
      this.mpComboBox1Cam.Location = new System.Drawing.Point(319, 39);
      this.mpComboBox1Cam.Name = "mpComboBox1Cam";
      this.mpComboBox1Cam.Size = new System.Drawing.Size(103, 21);
      this.mpComboBox1Cam.TabIndex = 69;
      this.mpComboBox1Cam.SelectedIndexChanged += new System.EventHandler(this.mpComboBox1_SelectedIndexChanged);
      // 
      // checkBoxCreateGroups
      // 
      this.checkBoxCreateGroups.AutoSize = true;
      this.checkBoxCreateGroups.Location = new System.Drawing.Point(16, 354);
      this.checkBoxCreateGroups.Name = "checkBoxCreateGroups";
      this.checkBoxCreateGroups.Size = new System.Drawing.Size(175, 17);
      this.checkBoxCreateGroups.TabIndex = 71;
      this.checkBoxCreateGroups.Text = "Create groups for each provider";
      this.checkBoxCreateGroups.UseVisualStyleBackColor = true;
      // 
      // CardDvbC
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.checkBoxCreateGroups);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.mpComboBox1Cam);
      this.Controls.Add(this.listViewStatus);
      this.Controls.Add(this.progressBarQuality);
      this.Controls.Add(this.progressBarLevel);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.mpLabel3);
      this.Controls.Add(this.mpLabelChannel);
      this.Controls.Add(this.mpButtonScanTv);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpComboBoxCountry);
      this.Controls.Add(this.mpBeveledLine1);
      this.Name = "CardDvbC";
      this.Size = new System.Drawing.Size(468, 420);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ProgressBar progressBarQuality;
    private System.Windows.Forms.ProgressBar progressBarLevel;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannel;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanTv;
    private MediaPortal.UserInterface.Controls.MPBeveledLine mpBeveledLine1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxCountry;
    private System.Windows.Forms.ListView listViewStatus;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBox1Cam;
    private System.Windows.Forms.CheckBox checkBoxCreateGroups;
  }
}