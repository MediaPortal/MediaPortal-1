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
      this.lblIgnored = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtIgnored = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.lblDisc = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtDisc = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.lblFirstFail = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtFirstFail = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.lblAvgMsec = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtAvgMsec = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpListViewLog = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader14 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
      this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.lblTotal = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtTotal = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.lblFailed = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtFailed = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.lblSucceeded = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtSucceded = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabelRecording = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelTimeShift = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonTimeShift = new MediaPortal.UserInterface.Controls.MPButton();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.comboBoxGroups = new SetupControls.ComboBoxEx();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtConcurrentTunes = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.lblNrOfConcurrentUsers = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblEachTuneWillLast = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtRndFrom = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.txtRndTo = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.chkRepeatTest = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.txtTuneDelay = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.lblTuneDelayMsec = new MediaPortal.UserInterface.Controls.MPLabel();
      this.chkShareChannels = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.User = new System.Windows.Forms.ColumnHeader();
      this.cardName = new System.Windows.Forms.ColumnHeader();
      this.subchannels = new System.Windows.Forms.ColumnHeader();
      this.chkSynch = new MediaPortal.UserInterface.Controls.MPCheckBox();
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
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.lblIgnored);
      this.mpGroupBox1.Controls.Add(this.txtIgnored);
      this.mpGroupBox1.Controls.Add(this.lblDisc);
      this.mpGroupBox1.Controls.Add(this.txtDisc);
      this.mpGroupBox1.Controls.Add(this.lblFirstFail);
      this.mpGroupBox1.Controls.Add(this.txtFirstFail);
      this.mpGroupBox1.Controls.Add(this.lblAvgMsec);
      this.mpGroupBox1.Controls.Add(this.txtAvgMsec);
      this.mpGroupBox1.Controls.Add(this.mpListViewLog);
      this.mpGroupBox1.Controls.Add(this.mpButton1);
      this.mpGroupBox1.Controls.Add(this.lblTotal);
      this.mpGroupBox1.Controls.Add(this.txtTotal);
      this.mpGroupBox1.Controls.Add(this.lblFailed);
      this.mpGroupBox1.Controls.Add(this.txtFailed);
      this.mpGroupBox1.Controls.Add(this.lblSucceeded);
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
      // lblIgnored
      // 
      this.lblIgnored.AutoSize = true;
      this.lblIgnored.Location = new System.Drawing.Point(222, 156);
      this.lblIgnored.Name = "lblIgnored";
      this.lblIgnored.Size = new System.Drawing.Size(46, 13);
      this.lblIgnored.TabIndex = 80;
      this.lblIgnored.Text = "Ignored:";
      // 
      // txtIgnored
      // 
      this.txtIgnored.Location = new System.Drawing.Point(268, 151);
      this.txtIgnored.Name = "txtIgnored";
      this.txtIgnored.ReadOnly = true;
      this.txtIgnored.Size = new System.Drawing.Size(46, 20);
      this.txtIgnored.TabIndex = 79;
      this.txtIgnored.Text = "0";
      this.txtIgnored.Value = 0;
      // 
      // lblDisc
      // 
      this.lblDisc.AutoSize = true;
      this.lblDisc.Location = new System.Drawing.Point(222, 177);
      this.lblDisc.Name = "lblDisc";
      this.lblDisc.Size = new System.Drawing.Size(34, 13);
      this.lblDisc.TabIndex = 78;
      this.lblDisc.Text = "Disc.:";
      // 
      // txtDisc
      // 
      this.txtDisc.Location = new System.Drawing.Point(268, 172);
      this.txtDisc.Name = "txtDisc";
      this.txtDisc.ReadOnly = true;
      this.txtDisc.Size = new System.Drawing.Size(46, 20);
      this.txtDisc.TabIndex = 77;
      this.txtDisc.Text = "0";
      this.txtDisc.Value = 0;
      // 
      // lblFirstFail
      // 
      this.lblFirstFail.AutoSize = true;
      this.lblFirstFail.Location = new System.Drawing.Point(126, 177);
      this.lblFirstFail.Name = "lblFirstFail";
      this.lblFirstFail.Size = new System.Drawing.Size(40, 13);
      this.lblFirstFail.TabIndex = 76;
      this.lblFirstFail.Text = "1st fail:";
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
      // lblAvgMsec
      // 
      this.lblAvgMsec.AutoSize = true;
      this.lblAvgMsec.Location = new System.Drawing.Point(4, 177);
      this.lblAvgMsec.Name = "lblAvgMsec";
      this.lblAvgMsec.Size = new System.Drawing.Size(59, 13);
      this.lblAvgMsec.TabIndex = 74;
      this.lblAvgMsec.Text = "Avg mSec:";
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
      this.mpListViewLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListViewLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader14,
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
      this.mpListViewLog.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListViewLog_ColumnClick);
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
      // columnHeader14
      // 
      this.columnHeader14.Text = "Disc.";
      // 
      // columnHeader11
      // 
      this.columnHeader11.Text = "Details";
      this.columnHeader11.Width = 175;
      // 
      // mpButton1
      // 
      this.mpButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButton1.Location = new System.Drawing.Point(365, 146);
      this.mpButton1.Name = "mpButton1";
      this.mpButton1.Size = new System.Drawing.Size(75, 25);
      this.mpButton1.TabIndex = 71;
      this.mpButton1.Text = "Clipboard";
      this.mpButton1.UseVisualStyleBackColor = true;
      this.mpButton1.Click += new System.EventHandler(this.mpButton1_Click);
      // 
      // lblTotal
      // 
      this.lblTotal.AutoSize = true;
      this.lblTotal.Location = new System.Drawing.Point(350, 176);
      this.lblTotal.Name = "lblTotal";
      this.lblTotal.Size = new System.Drawing.Size(34, 13);
      this.lblTotal.TabIndex = 70;
      this.lblTotal.Text = "Total:";
      // 
      // txtTotal
      // 
      this.txtTotal.Location = new System.Drawing.Point(394, 172);
      this.txtTotal.Name = "txtTotal";
      this.txtTotal.ReadOnly = true;
      this.txtTotal.Size = new System.Drawing.Size(46, 20);
      this.txtTotal.TabIndex = 69;
      this.txtTotal.Text = "0";
      this.txtTotal.Value = 0;
      // 
      // lblFailed
      // 
      this.lblFailed.AutoSize = true;
      this.lblFailed.Location = new System.Drawing.Point(126, 156);
      this.lblFailed.Name = "lblFailed";
      this.lblFailed.Size = new System.Drawing.Size(38, 13);
      this.lblFailed.TabIndex = 68;
      this.lblFailed.Text = "Failed:";
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
      // lblSucceeded
      // 
      this.lblSucceeded.AutoSize = true;
      this.lblSucceeded.Location = new System.Drawing.Point(3, 156);
      this.lblSucceeded.Name = "lblSucceeded";
      this.lblSucceeded.Size = new System.Drawing.Size(65, 13);
      this.lblSucceeded.TabIndex = 66;
      this.lblSucceeded.Text = "Succeeded:";
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
      // lblNrOfConcurrentUsers
      // 
      this.lblNrOfConcurrentUsers.AutoSize = true;
      this.lblNrOfConcurrentUsers.Location = new System.Drawing.Point(9, 220);
      this.lblNrOfConcurrentUsers.Name = "lblNrOfConcurrentUsers";
      this.lblNrOfConcurrentUsers.Size = new System.Drawing.Size(207, 13);
      this.lblNrOfConcurrentUsers.TabIndex = 64;
      this.lblNrOfConcurrentUsers.Text = "Number of concurrent virtual users (tunes):";
      // 
      // lblEachTuneWillLast
      // 
      this.lblEachTuneWillLast.AutoSize = true;
      this.lblEachTuneWillLast.Location = new System.Drawing.Point(9, 245);
      this.lblEachTuneWillLast.Name = "lblEachTuneWillLast";
      this.lblEachTuneWillLast.Size = new System.Drawing.Size(175, 13);
      this.lblEachTuneWillLast.TabIndex = 66;
      this.lblEachTuneWillLast.Text = "Each tune will last between (mSec):";
      // 
      // txtRndFrom
      // 
      this.txtRndFrom.Location = new System.Drawing.Point(183, 242);
      this.txtRndFrom.Name = "txtRndFrom";
      this.txtRndFrom.Size = new System.Drawing.Size(33, 20);
      this.txtRndFrom.TabIndex = 65;
      this.txtRndFrom.Text = "500";
      this.txtRndFrom.Value = 500;
      this.txtRndFrom.TextChanged += new System.EventHandler(this.txtRndFrom_TextChanged);
      // 
      // txtRndTo
      // 
      this.txtRndTo.Location = new System.Drawing.Point(235, 242);
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
      this.mpLabel5.Location = new System.Drawing.Point(219, 245);
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
      this.chkRepeatTest.Size = new System.Drawing.Size(104, 17);
      this.chkRepeatTest.TabIndex = 71;
      this.chkRepeatTest.Text = "Repeat until stop";
      this.chkRepeatTest.UseVisualStyleBackColor = true;
      this.chkRepeatTest.CheckedChanged += new System.EventHandler(this.chkRepeatTest_CheckedChanged);
      // 
      // txtTuneDelay
      // 
      this.txtTuneDelay.Location = new System.Drawing.Point(412, 220);
      this.txtTuneDelay.Name = "txtTuneDelay";
      this.txtTuneDelay.Size = new System.Drawing.Size(46, 20);
      this.txtTuneDelay.TabIndex = 72;
      this.txtTuneDelay.Text = "0";
      this.txtTuneDelay.Value = 0;
      // 
      // lblTuneDelayMsec
      // 
      this.lblTuneDelayMsec.AutoSize = true;
      this.lblTuneDelayMsec.Location = new System.Drawing.Point(304, 223);
      this.lblTuneDelayMsec.Name = "lblTuneDelayMsec";
      this.lblTuneDelayMsec.Size = new System.Drawing.Size(99, 13);
      this.lblTuneDelayMsec.TabIndex = 73;
      this.lblTuneDelayMsec.Text = "Tune delay (mSec):";
      // 
      // chkShareChannels
      // 
      this.chkShareChannels.AutoSize = true;
      this.chkShareChannels.Checked = true;
      this.chkShareChannels.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkShareChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkShareChannels.Location = new System.Drawing.Point(332, 245);
      this.chkShareChannels.Name = "chkShareChannels";
      this.chkShareChannels.Size = new System.Drawing.Size(126, 17);
      this.chkShareChannels.TabIndex = 74;
      this.chkShareChannels.Text = "Users share channels";
      this.chkShareChannels.UseVisualStyleBackColor = true;
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.User,
            this.cardName,
            this.subchannels});
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.HideSelection = false;
      this.mpListView1.IsChannelListView = false;
      this.mpListView1.Location = new System.Drawing.Point(12, 295);
      this.mpListView1.MultiSelect = false;
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(446, 134);
      this.mpListView1.TabIndex = 75;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Card";
      this.columnHeader1.Width = 35;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Type";
      this.columnHeader2.Width = 40;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "State";
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Name";
      this.columnHeader4.Width = 120;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Scrambled";
      this.columnHeader5.Width = 65;
      // 
      // User
      // 
      this.User.Text = "User";
      // 
      // cardName
      // 
      this.cardName.Text = "Card Name";
      this.cardName.Width = 120;
      // 
      // subchannels
      // 
      this.subchannels.Text = "Subchannels";
      this.subchannels.Width = 100;
      // 
      // chkSynch
      // 
      this.chkSynch.AutoSize = true;
      this.chkSynch.Checked = true;
      this.chkSynch.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkSynch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkSynch.Location = new System.Drawing.Point(278, 245);
      this.chkSynch.Name = "chkSynch";
      this.chkSynch.Size = new System.Drawing.Size(48, 17);
      this.chkSynch.TabIndex = 76;
      this.chkSynch.Text = "Sync";
      this.chkSynch.UseVisualStyleBackColor = true;
      // 
      // TestChannels
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.chkSynch);
      this.Controls.Add(this.mpListView1);
      this.Controls.Add(this.chkShareChannels);
      this.Controls.Add(this.lblTuneDelayMsec);
      this.Controls.Add(this.txtTuneDelay);
      this.Controls.Add(this.chkRepeatTest);
      this.Controls.Add(this.mpLabel5);
      this.Controls.Add(this.txtRndTo);
      this.Controls.Add(this.lblEachTuneWillLast);
      this.Controls.Add(this.txtRndFrom);
      this.Controls.Add(this.lblNrOfConcurrentUsers);
      this.Controls.Add(this.txtConcurrentTunes);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.comboBoxGroups);
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
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelRecording;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelTimeShift;
    private SetupControls.ComboBoxEx comboBoxGroups;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtConcurrentTunes;
    private MediaPortal.UserInterface.Controls.MPLabel lblNrOfConcurrentUsers;
    private MediaPortal.UserInterface.Controls.MPLabel lblEachTuneWillLast;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtRndFrom;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtRndTo;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkRepeatTest;
    private MediaPortal.UserInterface.Controls.MPLabel lblTotal;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtTotal;
    private MediaPortal.UserInterface.Controls.MPLabel lblFailed;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtFailed;
    private MediaPortal.UserInterface.Controls.MPLabel lblSucceeded;
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
    private MediaPortal.UserInterface.Controls.MPLabel lblFirstFail;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtFirstFail;
    private MediaPortal.UserInterface.Controls.MPLabel lblAvgMsec;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtAvgMsec;
    private System.Windows.Forms.ColumnHeader columnHeader12;
    private System.Windows.Forms.ColumnHeader columnHeader13;
    private MediaPortal.UserInterface.Controls.MPLabel lblTuneDelayMsec;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkShareChannels;
    private System.Windows.Forms.ColumnHeader columnHeader14;
    private MediaPortal.UserInterface.Controls.MPLabel lblDisc;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtDisc;
    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ColumnHeader User;
    private System.Windows.Forms.ColumnHeader cardName;
    private System.Windows.Forms.ColumnHeader subchannels;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkSynch;
    private MediaPortal.UserInterface.Controls.MPLabel lblIgnored;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox txtIgnored;
  }
}
