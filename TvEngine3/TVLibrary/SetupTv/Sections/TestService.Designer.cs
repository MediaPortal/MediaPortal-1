namespace SetupTv.Sections
{
  partial class TestService
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestService));
      this.progressBarQuality = new System.Windows.Forms.ProgressBar();
      this.progressBarLevel = new System.Windows.Forms.ProgressBar();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelTunerLocked = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label12 = new System.Windows.Forms.Label();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabelSignalQuality = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelSignalLevel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelRecording = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelTimeShift = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonTimeShift = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonRec = new MediaPortal.UserInterface.Controls.MPButton();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.User = new System.Windows.Forms.ColumnHeader();
      this.cardName = new System.Windows.Forms.ColumnHeader();
      this.buttonRestart = new System.Windows.Forms.Button();
      this.mpButtonReGrabEpg = new MediaPortal.UserInterface.Controls.MPButton();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpComboBoxChannels = new SetupControls.ComboBoxEx();
      this.comboBoxGroups = new SetupControls.ComboBoxEx();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Location = new System.Drawing.Point(113, 102);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(303, 10);
      this.progressBarQuality.TabIndex = 52;
      // 
      // progressBarLevel
      // 
      this.progressBarLevel.Location = new System.Drawing.Point(113, 79);
      this.progressBarLevel.Name = "progressBarLevel";
      this.progressBarLevel.Size = new System.Drawing.Size(303, 10);
      this.progressBarLevel.TabIndex = 51;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(21, 99);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 13);
      this.label2.TabIndex = 50;
      this.label2.Text = "Signal quality:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(21, 76);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 49;
      this.label1.Text = "Signal level:";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(21, 47);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(49, 13);
      this.mpLabel3.TabIndex = 47;
      this.mpLabel3.Text = "Channel:";
      // 
      // mpLabelTunerLocked
      // 
      this.mpLabelTunerLocked.AutoSize = true;
      this.mpLabelTunerLocked.Location = new System.Drawing.Point(113, 24);
      this.mpLabelTunerLocked.Name = "mpLabelTunerLocked";
      this.mpLabelTunerLocked.Size = new System.Drawing.Size(19, 13);
      this.mpLabelTunerLocked.TabIndex = 46;
      this.mpLabelTunerLocked.Text = "no";
      // 
      // mpLabelChannel
      // 
      this.mpLabelChannel.AutoEllipsis = true;
      this.mpLabelChannel.Location = new System.Drawing.Point(113, 47);
      this.mpLabelChannel.Name = "mpLabelChannel";
      this.mpLabelChannel.Size = new System.Drawing.Size(303, 26);
      this.mpLabelChannel.TabIndex = 48;
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(21, 24);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(73, 13);
      this.label12.TabIndex = 45;
      this.label12.Text = "Tuner locked:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpLabelSignalQuality);
      this.mpGroupBox1.Controls.Add(this.mpLabelSignalLevel);
      this.mpGroupBox1.Controls.Add(this.mpLabelRecording);
      this.mpGroupBox1.Controls.Add(this.mpLabelTimeShift);
      this.mpGroupBox1.Controls.Add(this.label1);
      this.mpGroupBox1.Controls.Add(this.progressBarQuality);
      this.mpGroupBox1.Controls.Add(this.label12);
      this.mpGroupBox1.Controls.Add(this.progressBarLevel);
      this.mpGroupBox1.Controls.Add(this.mpLabelChannel);
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.Controls.Add(this.mpLabelTunerLocked);
      this.mpGroupBox1.Controls.Add(this.mpLabel3);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(12, 12);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(446, 177);
      this.mpGroupBox1.TabIndex = 53;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Status:";
      // 
      // mpLabelSignalQuality
      // 
      this.mpLabelSignalQuality.AutoSize = true;
      this.mpLabelSignalQuality.Location = new System.Drawing.Point(420, 99);
      this.mpLabelSignalQuality.Name = "mpLabelSignalQuality";
      this.mpLabelSignalQuality.Size = new System.Drawing.Size(13, 13);
      this.mpLabelSignalQuality.TabIndex = 58;
      this.mpLabelSignalQuality.Text = "0";
      // 
      // mpLabelSignalLevel
      // 
      this.mpLabelSignalLevel.AutoSize = true;
      this.mpLabelSignalLevel.Location = new System.Drawing.Point(420, 76);
      this.mpLabelSignalLevel.Name = "mpLabelSignalLevel";
      this.mpLabelSignalLevel.Size = new System.Drawing.Size(13, 13);
      this.mpLabelSignalLevel.TabIndex = 57;
      this.mpLabelSignalLevel.Text = "0";
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
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(9, 223);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(49, 13);
      this.mpLabel1.TabIndex = 55;
      this.mpLabel1.Text = "Channel:";
      // 
      // mpButtonTimeShift
      // 
      this.mpButtonTimeShift.Location = new System.Drawing.Point(221, 218);
      this.mpButtonTimeShift.Name = "mpButtonTimeShift";
      this.mpButtonTimeShift.Size = new System.Drawing.Size(115, 23);
      this.mpButtonTimeShift.TabIndex = 56;
      this.mpButtonTimeShift.Text = "TimeShift";
      this.mpButtonTimeShift.UseVisualStyleBackColor = true;
      this.mpButtonTimeShift.Click += new System.EventHandler(this.mpButtonTimeShift_Click);
      // 
      // mpButtonRec
      // 
      this.mpButtonRec.Enabled = false;
      this.mpButtonRec.Location = new System.Drawing.Point(343, 218);
      this.mpButtonRec.Name = "mpButtonRec";
      this.mpButtonRec.Size = new System.Drawing.Size(115, 23);
      this.mpButtonRec.TabIndex = 57;
      this.mpButtonRec.Text = "Record";
      this.mpButtonRec.UseVisualStyleBackColor = true;
      this.mpButtonRec.Click += new System.EventHandler(this.mpButtonRec_Click);
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
      this.mpListView1.Location = new System.Drawing.Point(12, 252);
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
      // buttonRestart
      // 
      this.buttonRestart.Location = new System.Drawing.Point(241, 392);
      this.buttonRestart.Name = "buttonRestart";
      this.buttonRestart.Size = new System.Drawing.Size(95, 23);
      this.buttonRestart.TabIndex = 59;
      this.buttonRestart.Text = "Restart Service";
      this.buttonRestart.UseVisualStyleBackColor = true;
      this.buttonRestart.Click += new System.EventHandler(this.buttonRestart_Click);
      // 
      // mpButtonReGrabEpg
      // 
      this.mpButtonReGrabEpg.Location = new System.Drawing.Point(343, 392);
      this.mpButtonReGrabEpg.Name = "mpButtonReGrabEpg";
      this.mpButtonReGrabEpg.Size = new System.Drawing.Size(115, 23);
      this.mpButtonReGrabEpg.TabIndex = 60;
      this.mpButtonReGrabEpg.Text = "Refresh DVB EPG";
      this.mpButtonReGrabEpg.UseVisualStyleBackColor = true;
      this.mpButtonReGrabEpg.Click += new System.EventHandler(this.mpButtonReGrabEpg_Click);
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "radio_scrambled.png");
      this.imageList1.Images.SetKeyName(1, "tv_fta_.png");
      this.imageList1.Images.SetKeyName(2, "tv_scrambled.png");
      this.imageList1.Images.SetKeyName(3, "radio_fta_.png");
      // 
      // mpComboBoxChannels
      // 
      this.mpComboBoxChannels.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.mpComboBoxChannels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxChannels.FormattingEnabled = true;
      this.mpComboBoxChannels.ImageList = null;
      this.mpComboBoxChannels.Location = new System.Drawing.Point(64, 220);
      this.mpComboBoxChannels.Name = "mpComboBoxChannels";
      this.mpComboBoxChannels.Size = new System.Drawing.Size(151, 21);
      this.mpComboBoxChannels.TabIndex = 54;
      // 
      // comboBoxGroups
      // 
      this.comboBoxGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.comboBoxGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxGroups.FormattingEnabled = true;
      this.comboBoxGroups.ImageList = this.imageList1;
      this.comboBoxGroups.Location = new System.Drawing.Point(64, 196);
      this.comboBoxGroups.Name = "comboBoxGroups";
      this.comboBoxGroups.Size = new System.Drawing.Size(151, 21);
      this.comboBoxGroups.TabIndex = 61;
      this.comboBoxGroups.SelectedIndexChanged += new System.EventHandler(this.comboBoxGroups_SelectedIndexChanged);
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(9, 199);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(39, 13);
      this.mpLabel2.TabIndex = 62;
      this.mpLabel2.Text = "Group:";
      // 
      // TestService
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.comboBoxGroups);
      this.Controls.Add(this.mpButtonReGrabEpg);
      this.Controls.Add(this.buttonRestart);
      this.Controls.Add(this.mpListView1);
      this.Controls.Add(this.mpButtonRec);
      this.Controls.Add(this.mpButtonTimeShift);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpComboBoxChannels);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "TestService";
      this.Size = new System.Drawing.Size(470, 450);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ProgressBar progressBarQuality;
    private System.Windows.Forms.ProgressBar progressBarLevel;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelTunerLocked;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannel;
    private System.Windows.Forms.Label label12;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private SetupControls.ComboBoxEx mpComboBoxChannels;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonTimeShift;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonRec;
    private System.Windows.Forms.Timer timer1;
    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.Button buttonRestart;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonReGrabEpg;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ColumnHeader User;
    private System.Windows.Forms.ColumnHeader cardName;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelRecording;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelTimeShift;
    private SetupControls.ComboBoxEx comboBoxGroups;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelSignalLevel;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelSignalQuality;
  }
}
