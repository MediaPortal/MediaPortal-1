namespace SetupTv.Sections
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
      this.mpBeveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.mpComboBoxCountry = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxSource = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpButtonScanTv = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonScanRadio = new MediaPortal.UserInterface.Controls.MPButton();
      this.label12 = new System.Windows.Forms.Label();
      this.mpLabelTunerLocked = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxSensitivity = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.SuspendLayout();
      // 
      // mpBeveledLine1
      // 
      this.mpBeveledLine1.Location = new System.Drawing.Point(17, 3);
      this.mpBeveledLine1.Name = "mpBeveledLine1";
      this.mpBeveledLine1.Size = new System.Drawing.Size(423, 80);
      this.mpBeveledLine1.TabIndex = 0;
      this.mpBeveledLine1.Load += new System.EventHandler(this.mpBeveledLine1_Load);
      // 
      // mpComboBoxCountry
      // 
      this.mpComboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCountry.FormattingEnabled = true;
      this.mpComboBoxCountry.Location = new System.Drawing.Point(74, 12);
      this.mpComboBoxCountry.Name = "mpComboBoxCountry";
      this.mpComboBoxCountry.Size = new System.Drawing.Size(175, 21);
      this.mpComboBoxCountry.TabIndex = 1;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(22, 19);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(46, 13);
      this.mpLabel1.TabIndex = 2;
      this.mpLabel1.Text = "Country:";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(252, 19);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(44, 13);
      this.mpLabel2.TabIndex = 3;
      this.mpLabel2.Text = "Source:";
      // 
      // mpComboBoxSource
      // 
      this.mpComboBoxSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxSource.FormattingEnabled = true;
      this.mpComboBoxSource.Location = new System.Drawing.Point(303, 12);
      this.mpComboBoxSource.Name = "mpComboBoxSource";
      this.mpComboBoxSource.Size = new System.Drawing.Size(121, 21);
      this.mpComboBoxSource.TabIndex = 4;
      // 
      // mpButtonScanTv
      // 
      this.mpButtonScanTv.Location = new System.Drawing.Point(320, 319);
      this.mpButtonScanTv.Name = "mpButtonScanTv";
      this.mpButtonScanTv.Size = new System.Drawing.Size(131, 23);
      this.mpButtonScanTv.TabIndex = 5;
      this.mpButtonScanTv.Text = "Scan for Tv Channels";
      this.mpButtonScanTv.UseVisualStyleBackColor = true;
      this.mpButtonScanTv.Click += new System.EventHandler(this.mpButtonScan_Click);
      // 
      // mpButtonScanRadio
      // 
      this.mpButtonScanRadio.Location = new System.Drawing.Point(319, 348);
      this.mpButtonScanRadio.Name = "mpButtonScanRadio";
      this.mpButtonScanRadio.Size = new System.Drawing.Size(132, 23);
      this.mpButtonScanRadio.TabIndex = 6;
      this.mpButtonScanRadio.Text = "Scan for Radio channels";
      this.mpButtonScanRadio.UseVisualStyleBackColor = true;
      this.mpButtonScanRadio.Click += new System.EventHandler(this.mpButtonScanRadio_Click);
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(21, 86);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(73, 13);
      this.label12.TabIndex = 7;
      this.label12.Text = "Tuner locked:";
      // 
      // mpLabelTunerLocked
      // 
      this.mpLabelTunerLocked.AutoSize = true;
      this.mpLabelTunerLocked.Location = new System.Drawing.Point(109, 86);
      this.mpLabelTunerLocked.Name = "mpLabelTunerLocked";
      this.mpLabelTunerLocked.Size = new System.Drawing.Size(19, 13);
      this.mpLabelTunerLocked.TabIndex = 8;
      this.mpLabelTunerLocked.Text = "no";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(163, 86);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(86, 13);
      this.mpLabel3.TabIndex = 9;
      this.mpLabel3.Text = "Current Channel:";
      // 
      // mpLabelChannel
      // 
      this.mpLabelChannel.AutoSize = true;
      this.mpLabelChannel.Location = new System.Drawing.Point(255, 86);
      this.mpLabelChannel.Name = "mpLabelChannel";
      this.mpLabelChannel.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannel.TabIndex = 10;
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
      this.mpListView1.Location = new System.Drawing.Point(17, 129);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(434, 184);
      this.mpListView1.TabIndex = 11;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Channel";
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Name";
      this.columnHeader2.Width = 200;
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(16, 112);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(328, 10);
      this.progressBar1.TabIndex = 12;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(240, 46);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(57, 13);
      this.mpLabel4.TabIndex = 14;
      this.mpLabel4.Text = "Sensitivity:";
      // 
      // mpComboBoxSensitivity
      // 
      this.mpComboBoxSensitivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxSensitivity.FormattingEnabled = true;
      this.mpComboBoxSensitivity.Items.AddRange(new object[] {
            "Low",
            "Medium",
            "High"});
      this.mpComboBoxSensitivity.Location = new System.Drawing.Point(303, 43);
      this.mpComboBoxSensitivity.Name = "mpComboBoxSensitivity";
      this.mpComboBoxSensitivity.Size = new System.Drawing.Size(121, 21);
      this.mpComboBoxSensitivity.TabIndex = 15;
      // 
      // CardAnalog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpComboBoxSensitivity);
      this.Controls.Add(this.mpLabel4);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.mpListView1);
      this.Controls.Add(this.mpLabelChannel);
      this.Controls.Add(this.mpLabel3);
      this.Controls.Add(this.mpLabelTunerLocked);
      this.Controls.Add(this.label12);
      this.Controls.Add(this.mpButtonScanRadio);
      this.Controls.Add(this.mpButtonScanTv);
      this.Controls.Add(this.mpComboBoxSource);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpComboBoxCountry);
      this.Controls.Add(this.mpBeveledLine1);
      this.Name = "CardAnalog";
      this.Size = new System.Drawing.Size(467, 388);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPBeveledLine mpBeveledLine1;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxCountry;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxSource;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanTv;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonScanRadio;
    private System.Windows.Forms.Label label12;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelTunerLocked;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannel;
    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ProgressBar progressBar1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxSensitivity;
  }
}