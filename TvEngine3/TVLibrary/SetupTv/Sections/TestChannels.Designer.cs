namespace SetupTv.Sections
{
  partial class TestChannels
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestChannels));
      this.mpLabelChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtFirstFail = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabel9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtAvgMsec = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpListViewLog = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
      this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtTotal = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtFailed = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtSucceded = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabelRecording = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelTimeShift = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonTimeShift = new MediaPortal.UserInterface.Controls.MPButton();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.User = new System.Windows.Forms.ColumnHeader();
      this.cardName = new System.Windows.Forms.ColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.comboBoxGroups = new SetupControls.ComboBoxEx();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtConcurrentTunes = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtRndFrom = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.txtRndTo = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.chkRepeatTest = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.txtTuneDelay = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabel11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.chkShareChannels = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpLabelChannel
      // 
      this.mpLabelChannel.AutoEllipsis = true;
      this.mpLabelChannel.Location = new System.Drawing.Point(113, 47);
      this.mpLabelChannel.Name = "mpLabelChannel";
      this.mpLabelChannel.Size = new System.Drawing.Size(303, 26);
      this.mpLabelChannel.TabIndex = 48;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpLabel10);
      this.mpGroupBox1.Controls.Add(this.txtFirstFail);
      this.mpGroupBox1.Controls.Add(this.mpLabel9);
      this.mpGroupBox1.Controls.Add(this.txtAvgMsec);
      this.mpGroupBox1.Controls.Add(this.mpListViewLog);
      this.mpGroupBox1.Controls.Add(this.mpButton1);
      this.mpGroupBox1.Controls.Add(this.mpLabel8);
      this.mpGroupBox1.Controls.Add(this.txtTotal);
      this.mpGroupBox1.Controls.Add(this.mpLabel7);
      this.mpGroupBox1.Controls.Add(this.txtFailed);
      this.mpGroupBox1.Controls.Add(this.mpLabel6);
      this.mpGroupBox1.Controls.Add(this.txtSucceded);
      this.mpGroupBox1.Controls.Add(this.mpLabelRecording);
      this.mpGroupBox1.Controls.Add(this.mpLabelTimeShift);
      this.mpGroupBox1.Controls.Add(this.mpLabelChannel);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(12, 12);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(446, 199);
      this.mpGroupBox1.TabIndex = 53;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Status:";
      // 
      // mpLabel10
      // 
      this.mpLabel10.AutoSize = true;
      this.mpLabel10.Location = new System.Drawing.Point(126, 177);
      this.mpLabel10.Name = "mpLabel10";
      this.mpLabel10.Size = new System.Drawing.Size(40, 13);
      this.mpLabel10.TabIndex = 76;
      this.mpLabel10.Text = "1st fail:";
      // 
      // txtFirstFail
      // 
      this.txtFirstFail.Location = new System.Drawing.Point(170, 173);
      this.txtFirstFail.Name = "txtFirstFail";
      this.txtFirstFail.ReadOnly = true;
      this.txtFirstFail.Size = new System.Drawing.Size(46, 20);
      this.txtFirstFail.TabIndex = 75;
      this.txtFirstFail.Text = "0";
      this.txtFirstFail.Value = 0;
      // 
      // mpLabel9
      // 
      this.mpLabel9.AutoSize = true;
      this.mpLabel9.Location = new System.Drawing.Point(4, 177);
      this.mpLabel9.Name = "mpLabel9";
      this.mpLabel9.Size = new System.Drawing.Size(59, 13);
      this.mpLabel9.TabIndex = 74;
      this.mpLabel9.Text = "Avg mSec:";
      // 
      // txtAvgMsec
      // 
      this.txtAvgMsec.Location = new System.Drawing.Point(74, 173);
      this.txtAvgMsec.Name = "txtAvgMsec";
      this.txtAvgMsec.ReadOnly = true;
      this.txtAvgMsec.Size = new System.Drawing.Size(46, 20);
      this.txtAvgMsec.TabIndex = 73;
      this.txtAvgMsec.Text = "0";
      this.txtAvgMsec.Value = 0;
      // 
      // mpListViewLog
      // 
      this.mpListViewLog.AllowDrop = true;
      this.mpListViewLog.AllowRowReorder = true;
      this.mpListViewLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListViewLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11});
      this.mpListViewLog.FullRowSelect = true;
      this.mpListViewLog.HideSelection = false;
      this.mpListViewLog.IsChannelListView = false;
      this.mpListViewLog.Location = new System.Drawing.Point(7, 19);
      this.mpListViewLog.MultiSelect = false;
      this.mpListViewLog.Name = "mpListViewLog";
      this.mpListViewLog.Size = new System.Drawing.Size(433, 121);
      this.mpListViewLog.TabIndex = 72;
      this.mpListViewLog.UseCompatibleStateImageBehavior = false;
      this.mpListViewLog.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader12
      // 
      this.columnHeader12.Text = "#";
      this.columnHeader12.Width = 20;
      // 
      // columnHeader13
      // 
      this.columnHeader13.Text = "Time";
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "State";
      this.columnHeader6.Width = 40;
      // 
      // columnHeader7
      // 
      this.columnHeader7.Text = "Channel";
      this.columnHeader7.Width = 100;
      // 
      // columnHeader8
      // 
      this.columnHeader8.Text = "mSec";
      this.columnHeader8.Width = 40;
      // 
      // columnHeader9
      // 
      this.columnHeader9.Text = "Name";
      this.columnHeader9.Width = 80;
      // 
      // columnHeader10
      // 
      this.columnHeader10.Text = "Card";
      this.columnHeader10.Width = 40;
      // 
      // columnHeader11
      // 
      this.columnHeader11.Text = "Details";
      this.columnHeader11.Width = 175;
      // 
      // mpButton1
      // 
      this.mpButton1.Location = new System.Drawing.Point(365, 146);
      this.mpButton1.Name = "mpButton1";
      this.mpButton1.Size = new System.Drawing.Size(75, 25);
      this.mpButton1.TabIndex = 71;
      this.mpButton1.Text = "Clipboard";
      this.mpButton1.UseVisualStyleBackColor = true;
      this.mpButton1.Click += new System.EventHandler(this.mpButton1_Click);
      // 
      // mpLabel8
      // 
      this.mpLabel8.AutoSize = true;
      this.mpLabel8.Location = new System.Drawing.Point(222, 155);
      this.mpLabel8.Name = "mpLabel8";
      this.mpLabel8.Size = new System.Drawing.Size(34, 13);
      this.mpLabel8.TabIndex = 70;
      this.mpLabel8.Text = "Total:";
      // 
      // txtTotal
      // 
      this.txtTotal.Location = new System.Drawing.Point(266, 151);
      this.txtTotal.Name = "txtTotal";
      this.txtTotal.ReadOnly = true;
      this.txtTotal.Size = new System.Drawing.Size(46, 20);
      this.txtTotal.TabIndex = 69;
      this.txtTotal.Text = "0";
      this.txtTotal.Value = 0;
      // 
      // mpLabel7
      // 
      this.mpLabel7.AutoSize = true;
      this.mpLabel7.Location = new System.Drawing.Point(126, 156);
      this.mpLabel7.Name = "mpLabel7";
      this.mpLabel7.Size = new System.Drawing.Size(38, 13);
      this.mpLabel7.TabIndex = 68;
      this.mpLabel7.Text = "Failed:";
      // 
      // txtFailed
      // 
      this.txtFailed.Location = new System.Drawing.Point(170, 152);
      this.txtFailed.Name = "txtFailed";
      this.txtFailed.ReadOnly = true;
      this.txtFailed.Size = new System.Drawing.Size(46, 20);
      this.txtFailed.TabIndex = 67;
      this.txtFailed.Text = "0";
      this.txtFailed.Value = 0;
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(3, 156);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(65, 13);
      this.mpLabel6.TabIndex = 66;
      this.mpLabel6.Text = "Succeeded:";
      // 
      // txtSucceded
      // 
      this.txtSucceded.Location = new System.Drawing.Point(74, 153);
      this.txtSucceded.Name = "txtSucceded";
      this.txtSucceded.ReadOnly = true;
      this.txtSucceded.Size = new System.Drawing.Size(46, 20);
      this.txtSucceded.TabIndex = 65;
      this.txtSucceded.Text = "0";
      this.txtSucceded.Value = 0;
      // 
      // mpLabelRecording
      // 
      this.mpLabelRecording.AutoSize = true;
      this.mpLabelRecording.Location = new System.Drawing.Point(27, 152);
      this.mpLabelRecording.Name = "mpLabelRecording";
      this.mpLabelRecording.Size = new System.Drawing.Size(0, 13);
      this.mpLabelRecording.TabIndex = 56;
      // 
      // mpLabelTimeShift
      // 
      this.mpLabelTimeShift.AutoSize = true;
      this.mpLabelTimeShift.Location = new System.Drawing.Point(27, 129);
      this.mpLabelTimeShift.Name = "mpLabelTimeShift";
      this.mpLabelTimeShift.Size = new System.Drawing.Size(0, 13);
      this.mpLabelTimeShift.TabIndex = 55;
      // 
      // mpButtonTimeShift
      // 
      this.mpButtonTimeShift.Location = new System.Drawing.Point(222, 266);
      this.mpButtonTimeShift.Name = "mpButtonTimeShift";
      this.mpButtonTimeShift.Size = new System.Drawing.Size(115, 23);
      this.mpButtonTimeShift.TabIndex = 56;
      this.mpButtonTimeShift.Text = "Start test";
      this.mpButtonTimeShift.UseVisualStyleBackColor = true;
      this.mpButtonTimeShift.Click += new System.EventHandler(this.mpButtonTimeShift_Click);
      // 
      // timer1
      // 
      this.timer1.Interval = 1000;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.User,
            this.cardName});
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.HideSelection = false;
      this.mpListView1.IsChannelListView = false;
      this.mpListView1.Location = new System.Drawing.Point(12, 295);
      this.mpListView1.MultiSelect = false;
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(446, 134);
      this.mpListView1.TabIndex = 58;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Card";
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Type";
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "State";
      this.columnHeader3.Width = 100;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Name";
      this.columnHeader4.Width = 120;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Scrambled";
      this.columnHeader5.Width = 80;
      // 
      // User
      // 
      this.User.Text = "User";
      this.User.Width = 100;
      // 
      // cardName
      // 
      this.cardName.Text = "Card Name";
      this.cardName.Width = 120;
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "radio_fta_.png");
      this.imageList1.Images.SetKeyName(1, "radio_scrambled.png");
      this.imageList1.Images.SetKeyName(2, "icon.radio_scrambled_and_fta.png");
      this.imageList1.Images.SetKeyName(3, "tv_fta_.png");
      this.imageList1.Images.SetKeyName(4, "tv_scrambled.png");
      this.imageList1.Images.SetKeyName(5, "icon.tv_scrambled_and_fta.png");
      // 
      // comboBoxGroups
      // 
      this.comboBoxGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.comboBoxGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxGroups.FormattingEnabled = true;
      this.comboBoxGroups.ImageList = this.imageList1;
      this.comboBoxGroups.Location = new System.Drawing.Point(65, 268);
      this.comboBoxGroups.Name = "comboBoxGroups";
      this.comboBoxGroups.Size = new System.Drawing.Size(151, 21);
      this.comboBoxGroups.TabIndex = 61;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(10, 271);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(39, 13);
      this.mpLabel2.TabIndex = 62;
      this.mpLabel2.Text = "Group:";
      // 
      // txtConcurrentTunes
      // 
      this.txtConcurrentTunes.Location = new System.Drawing.Point(222, 217);
      this.txtConcurrentTunes.Name = "txtConcurrentTunes";
      this.txtConcurrentTunes.Size = new System.Drawing.Size(46, 20);
      this.txtConcurrentTunes.TabIndex = 63;
      this.txtConcurrentTunes.Text = "2";
      this.txtConcurrentTunes.Value = 2;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(9, 220);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(207, 13);
      this.mpLabel1.TabIndex = 64;
      this.mpLabel1.Text = "Number of concurrent virtual users (tunes):";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(9, 245);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(192, 13);
      this.mpLabel3.TabIndex = 66;
      this.mpLabel3.Text = "Each tune should last between (mSec):";
      // 
      // txtRndFrom
      // 
      this.txtRndFrom.Location = new System.Drawing.Point(222, 242);
      this.txtRndFrom.Name = "txtRndFrom";
      this.txtRndFrom.Size = new System.Drawing.Size(33, 20);
      this.txtRndFrom.TabIndex = 65;
      this.txtRndFrom.Text = "500";
      this.txtRndFrom.Value = 500;
      this.txtRndFrom.TextChanged += new System.EventHandler(this.txtRndFrom_TextChanged);
      // 
      // txtRndTo
      // 
      this.txtRndTo.Location = new System.Drawing.Point(274, 242);
      this.txtRndTo.Name = "txtRndTo";
      this.txtRndTo.Size = new System.Drawing.Size(33, 20);
      this.txtRndTo.TabIndex = 67;
      this.txtRndTo.Text = "1000";
      this.txtRndTo.Value = 1000;
      this.txtRndTo.TextChanged += new System.EventHandler(this.txtRndTo_TextChanged);
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(258, 245);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(10, 13);
      this.mpLabel5.TabIndex = 69;
      this.mpLabel5.Text = "-";
      // 
      // chkRepeatTest
      // 
      this.chkRepeatTest.AutoSize = true;
      this.chkRepeatTest.Checked = true;
      this.chkRepeatTest.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkRepeatTest.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkRepeatTest.Location = new System.Drawing.Point(349, 271);
      this.chkRepeatTest.Name = "chkRepeatTest";
      this.chkRepeatTest.Size = new System.Drawing.Size(79, 17);
      this.chkRepeatTest.TabIndex = 71;
      this.chkRepeatTest.Text = "Repeat test";
      this.chkRepeatTest.UseVisualStyleBackColor = true;
      this.chkRepeatTest.CheckedChanged += new System.EventHandler(this.chkRepeatTest_CheckedChanged);
      // 
      // txtTuneDelay
      // 
      this.txtTuneDelay.Location = new System.Drawing.Point(412, 220);
      this.txtTuneDelay.Name = "txtTuneDelay";
      this.txtTuneDelay.Size = new System.Drawing.Size(46, 20);
      this.txtTuneDelay.TabIndex = 72;
      this.txtTuneDelay.Text = "1000";
      this.txtTuneDelay.Value = 1000;
      // 
      // mpLabel11
      // 
      this.mpLabel11.AutoSize = true;
      this.mpLabel11.Location = new System.Drawing.Point(304, 223);
      this.mpLabel11.Name = "mpLabel11";
      this.mpLabel11.Size = new System.Drawing.Size(102, 13);
      this.mpLabel11.TabIndex = 73;
      this.mpLabel11.Text = "Tune delay (mSec) :";
      // 
      // chkShareChannels
      // 
      this.chkShareChannels.AutoSize = true;
      this.chkShareChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkShareChannels.Location = new System.Drawing.Point(332, 245);
      this.chkShareChannels.Name = "chkShareChannels";
      this.chkShareChannels.Size = new System.Drawing.Size(126, 17);
      this.chkShareChannels.TabIndex = 74;
      this.chkShareChannels.Text = "Users share channels";
      this.chkShareChannels.UseVisualStyleBackColor = true;
      // 
      // TestChannels
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.chkShareChannels);
      this.Controls.Add(this.mpLabel11);
      this.Controls.Add(this.txtTuneDelay);
      this.Controls.Add(this.chkRepeatTest);
      this.Controls.Add(this.mpLabel5);
      this.Controls.Add(this.txtRndTo);
      this.Controls.Add(this.mpLabel3);
      this.Controls.Add(this.txtRndFrom);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.txtConcurrentTunes);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.comboBoxGroups);
      this.Controls.Add(this.mpListView1);
      this.Controls.Add(this.mpButtonTimeShift);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "TestChannels";
      this.Size = new System.Drawing.Size(470, 450);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannel;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonTimeShift;
    private System.Windows.Forms.Timer timer1;
    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ColumnHeader User;
    private System.Windows.Forms.ColumnHeader cardName;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelRecording;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelTimeShift;
    private SetupControls.ComboBoxEx comboBoxGroups;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtConcurrentTunes;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtRndFrom;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtRndTo;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkRepeatTest;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel8;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtTotal;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel7;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtFailed;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel6;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtSucceded;
    private MediaPortal.UserInterface.Controls.MPButton mpButton1;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewLog;
    private System.Windows.Forms.ColumnHeader columnHeader6;
    private System.Windows.Forms.ColumnHeader columnHeader7;
    private System.Windows.Forms.ColumnHeader columnHeader8;
    private System.Windows.Forms.ColumnHeader columnHeader9;
    private System.Windows.Forms.ColumnHeader columnHeader10;
    private System.Windows.Forms.ColumnHeader columnHeader11;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtTuneDelay;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel10;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtFirstFail;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel9;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtAvgMsec;
    private System.Windows.Forms.ColumnHeader columnHeader12;
    private System.Windows.Forms.ColumnHeader columnHeader13;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel11;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkShareChannels;
  }
}
