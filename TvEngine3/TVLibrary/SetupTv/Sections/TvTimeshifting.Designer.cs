namespace SetupTv.Sections
{
  partial class TvTimeshifting
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvTimeshifting));
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tpTimeshifing = new System.Windows.Forms.TabPage();
      this.grpBoxTimeshiftingInfo = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lblMinFileSizeNeeded = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblFileSizeNeeded = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblTimeSD = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblTimeHD = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblOverhead = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox10 = new System.Windows.Forms.GroupBox();
      this.label48 = new System.Windows.Forms.Label();
      this.numericUpDownMaxFreeCardsToTry = new System.Windows.Forms.NumericUpDown();
      this.label47 = new System.Windows.Forms.Label();
      this.numericUpDownWaitTimeshifting = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownWaitUnscrambled = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownMaxFileSize = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownMaxFiles = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownMinFiles = new System.Windows.Forms.NumericUpDown();
      this.label26 = new System.Windows.Forms.Label();
      this.label31 = new System.Windows.Forms.Label();
      this.label32 = new System.Windows.Forms.Label();
      this.label33 = new System.Windows.Forms.Label();
      this.label34 = new System.Windows.Forms.Label();
      this.label35 = new System.Windows.Forms.Label();
      this.label36 = new System.Windows.Forms.Label();
      this.label37 = new System.Windows.Forms.Label();
      this.label39 = new System.Windows.Forms.Label();
      this.label46 = new System.Windows.Forms.Label();
      this.tpTimeshifingFolders = new System.Windows.Forms.TabPage();
      this.groupBoxTimeshiftSettings = new System.Windows.Forms.GroupBox();
      this.buttonSameTimeshiftFolder = new System.Windows.Forms.Button();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.labelTSFolder = new System.Windows.Forms.Label();
      this.comboBoxCards = new System.Windows.Forms.ComboBox();
      this.buttonTimeShiftBrowse = new System.Windows.Forms.Button();
      this.label22 = new System.Windows.Forms.Label();
      this.textBoxTimeShiftFolder = new System.Windows.Forms.TextBox();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.label21 = new System.Windows.Forms.Label();
      this.tabPageTS = new System.Windows.Forms.TabPage();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.label43 = new System.Windows.Forms.Label();
      this.label42 = new System.Windows.Forms.Label();
      this.label41 = new System.Windows.Forms.Label();
      this.label40 = new System.Windows.Forms.Label();
      this.label16 = new System.Windows.Forms.Label();
      this.label17 = new System.Windows.Forms.Label();
      this.label20 = new System.Windows.Forms.Label();
      this.label18 = new System.Windows.Forms.Label();
      this.label19 = new System.Windows.Forms.Label();
      this.label25 = new System.Windows.Forms.Label();
      this.tabControl1.SuspendLayout();
      this.tpTimeshifing.SuspendLayout();
      this.grpBoxTimeshiftingInfo.SuspendLayout();
      this.groupBox10.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxFreeCardsToTry)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWaitTimeshifting)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWaitUnscrambled)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxFileSize)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxFiles)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinFiles)).BeginInit();
      this.tpTimeshifingFolders.SuspendLayout();
      this.groupBoxTimeshiftSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.tabPageTS.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tpTimeshifing);
      this.tabControl1.Controls.Add(this.tpTimeshifingFolders);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(465, 400);
      this.tabControl1.TabIndex = 0;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tpTimeshifing
      // 
      this.tpTimeshifing.Controls.Add(this.grpBoxTimeshiftingInfo);
      this.tpTimeshifing.Controls.Add(this.groupBox10);
      this.tpTimeshifing.Location = new System.Drawing.Point(4, 22);
      this.tpTimeshifing.Name = "tpTimeshifing";
      this.tpTimeshifing.Padding = new System.Windows.Forms.Padding(3);
      this.tpTimeshifing.Size = new System.Drawing.Size(457, 374);
      this.tpTimeshifing.TabIndex = 4;
      this.tpTimeshifing.Text = "General";
      this.tpTimeshifing.UseVisualStyleBackColor = true;
      // 
      // grpBoxTimeshiftingInfo
      // 
      this.grpBoxTimeshiftingInfo.Controls.Add(this.lblMinFileSizeNeeded);
      this.grpBoxTimeshiftingInfo.Controls.Add(this.lblFileSizeNeeded);
      this.grpBoxTimeshiftingInfo.Controls.Add(this.lblTimeSD);
      this.grpBoxTimeshiftingInfo.Controls.Add(this.lblTimeHD);
      this.grpBoxTimeshiftingInfo.Controls.Add(this.lblOverhead);
      this.grpBoxTimeshiftingInfo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.grpBoxTimeshiftingInfo.Location = new System.Drawing.Point(6, 190);
      this.grpBoxTimeshiftingInfo.Name = "grpBoxTimeshiftingInfo";
      this.grpBoxTimeshiftingInfo.Size = new System.Drawing.Size(304, 105);
      this.grpBoxTimeshiftingInfo.TabIndex = 20;
      this.grpBoxTimeshiftingInfo.TabStop = false;
      this.grpBoxTimeshiftingInfo.Text = "Timeshifting Info";
      // 
      // lblMinFileSizeNeeded
      // 
      this.lblMinFileSizeNeeded.AutoSize = true;
      this.lblMinFileSizeNeeded.ForeColor = System.Drawing.Color.Red;
      this.lblMinFileSizeNeeded.Location = new System.Drawing.Point(6, 16);
      this.lblMinFileSizeNeeded.Name = "lblMinFileSizeNeeded";
      this.lblMinFileSizeNeeded.Size = new System.Drawing.Size(64, 13);
      this.lblMinFileSizeNeeded.TabIndex = 18;
      this.lblMinFileSizeNeeded.Text = "Filesize calc";
      // 
      // lblFileSizeNeeded
      // 
      this.lblFileSizeNeeded.AutoSize = true;
      this.lblFileSizeNeeded.Location = new System.Drawing.Point(6, 31);
      this.lblFileSizeNeeded.Name = "lblFileSizeNeeded";
      this.lblFileSizeNeeded.Size = new System.Drawing.Size(80, 13);
      this.lblFileSizeNeeded.TabIndex = 19;
      this.lblFileSizeNeeded.Text = "Filesize needed";
      // 
      // lblTimeSD
      // 
      this.lblTimeSD.AutoSize = true;
      this.lblTimeSD.Location = new System.Drawing.Point(6, 89);
      this.lblTimeSD.Name = "lblTimeSD";
      this.lblTimeSD.Size = new System.Drawing.Size(48, 13);
      this.lblTimeSD.TabIndex = 19;
      this.lblTimeSD.Text = "Time SD";
      // 
      // lblTimeHD
      // 
      this.lblTimeHD.AutoSize = true;
      this.lblTimeHD.Location = new System.Drawing.Point(6, 75);
      this.lblTimeHD.Name = "lblTimeHD";
      this.lblTimeHD.Size = new System.Drawing.Size(49, 13);
      this.lblTimeHD.TabIndex = 19;
      this.lblTimeHD.Text = "Time HD";
      // 
      // lblOverhead
      // 
      this.lblOverhead.AutoSize = true;
      this.lblOverhead.Location = new System.Drawing.Point(6, 44);
      this.lblOverhead.Name = "lblOverhead";
      this.lblOverhead.Size = new System.Drawing.Size(54, 13);
      this.lblOverhead.TabIndex = 19;
      this.lblOverhead.Text = "Overhead";
      // 
      // groupBox10
      // 
      this.groupBox10.Controls.Add(this.label48);
      this.groupBox10.Controls.Add(this.numericUpDownMaxFreeCardsToTry);
      this.groupBox10.Controls.Add(this.label47);
      this.groupBox10.Controls.Add(this.numericUpDownWaitTimeshifting);
      this.groupBox10.Controls.Add(this.numericUpDownWaitUnscrambled);
      this.groupBox10.Controls.Add(this.numericUpDownMaxFileSize);
      this.groupBox10.Controls.Add(this.numericUpDownMaxFiles);
      this.groupBox10.Controls.Add(this.numericUpDownMinFiles);
      this.groupBox10.Controls.Add(this.label26);
      this.groupBox10.Controls.Add(this.label31);
      this.groupBox10.Controls.Add(this.label32);
      this.groupBox10.Controls.Add(this.label33);
      this.groupBox10.Controls.Add(this.label34);
      this.groupBox10.Controls.Add(this.label35);
      this.groupBox10.Controls.Add(this.label36);
      this.groupBox10.Controls.Add(this.label37);
      this.groupBox10.Controls.Add(this.label39);
      this.groupBox10.Controls.Add(this.label46);
      this.groupBox10.Location = new System.Drawing.Point(6, 6);
      this.groupBox10.Name = "groupBox10";
      this.groupBox10.Size = new System.Drawing.Size(304, 180);
      this.groupBox10.TabIndex = 17;
      this.groupBox10.TabStop = false;
      this.groupBox10.Text = "Timeshifting";
      // 
      // label48
      // 
      this.label48.AutoSize = true;
      this.label48.Location = new System.Drawing.Point(246, 152);
      this.label48.Name = "label48";
      this.label48.Size = new System.Drawing.Size(29, 13);
      this.label48.TabIndex = 37;
      this.label48.Text = "0=all";
      // 
      // numericUpDownMaxFreeCardsToTry
      // 
      this.numericUpDownMaxFreeCardsToTry.Location = new System.Drawing.Point(153, 149);
      this.numericUpDownMaxFreeCardsToTry.Name = "numericUpDownMaxFreeCardsToTry";
      this.numericUpDownMaxFreeCardsToTry.Size = new System.Drawing.Size(87, 20);
      this.numericUpDownMaxFreeCardsToTry.TabIndex = 36;
      this.numericUpDownMaxFreeCardsToTry.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // label47
      // 
      this.label47.AutoSize = true;
      this.label47.Location = new System.Drawing.Point(6, 151);
      this.label47.Name = "label47";
      this.label47.Size = new System.Drawing.Size(127, 13);
      this.label47.TabIndex = 35;
      this.label47.Text = "Maximum free cards to try";
      // 
      // numericUpDownWaitTimeshifting
      // 
      this.numericUpDownWaitTimeshifting.Location = new System.Drawing.Point(153, 123);
      this.numericUpDownWaitTimeshifting.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.numericUpDownWaitTimeshifting.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownWaitTimeshifting.Name = "numericUpDownWaitTimeshifting";
      this.numericUpDownWaitTimeshifting.Size = new System.Drawing.Size(87, 20);
      this.numericUpDownWaitTimeshifting.TabIndex = 34;
      this.numericUpDownWaitTimeshifting.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownWaitTimeshifting.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // numericUpDownWaitUnscrambled
      // 
      this.numericUpDownWaitUnscrambled.Location = new System.Drawing.Point(153, 97);
      this.numericUpDownWaitUnscrambled.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.numericUpDownWaitUnscrambled.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownWaitUnscrambled.Name = "numericUpDownWaitUnscrambled";
      this.numericUpDownWaitUnscrambled.Size = new System.Drawing.Size(87, 20);
      this.numericUpDownWaitUnscrambled.TabIndex = 33;
      this.numericUpDownWaitUnscrambled.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownWaitUnscrambled.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
      // 
      // numericUpDownMaxFileSize
      // 
      this.numericUpDownMaxFileSize.Location = new System.Drawing.Point(153, 71);
      this.numericUpDownMaxFileSize.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
      this.numericUpDownMaxFileSize.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
      this.numericUpDownMaxFileSize.Name = "numericUpDownMaxFileSize";
      this.numericUpDownMaxFileSize.Size = new System.Drawing.Size(87, 20);
      this.numericUpDownMaxFileSize.TabIndex = 32;
      this.numericUpDownMaxFileSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownMaxFileSize.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
      this.numericUpDownMaxFileSize.ValueChanged += new System.EventHandler(this.numericUpDownMaxFileSize_ValueChanged);
      // 
      // numericUpDownMaxFiles
      // 
      this.numericUpDownMaxFiles.Location = new System.Drawing.Point(153, 45);
      this.numericUpDownMaxFiles.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
      this.numericUpDownMaxFiles.Name = "numericUpDownMaxFiles";
      this.numericUpDownMaxFiles.Size = new System.Drawing.Size(87, 20);
      this.numericUpDownMaxFiles.TabIndex = 31;
      this.numericUpDownMaxFiles.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownMaxFiles.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
      // 
      // numericUpDownMinFiles
      // 
      this.numericUpDownMinFiles.Location = new System.Drawing.Point(153, 19);
      this.numericUpDownMinFiles.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
      this.numericUpDownMinFiles.Name = "numericUpDownMinFiles";
      this.numericUpDownMinFiles.Size = new System.Drawing.Size(87, 20);
      this.numericUpDownMinFiles.TabIndex = 30;
      this.numericUpDownMinFiles.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownMinFiles.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
      this.numericUpDownMinFiles.ValueChanged += new System.EventHandler(this.numericUpDownMinFiles_ValueChanged);
      // 
      // label26
      // 
      this.label26.AutoSize = true;
      this.label26.Location = new System.Drawing.Point(246, 125);
      this.label26.Name = "label26";
      this.label26.Size = new System.Drawing.Size(24, 13);
      this.label26.TabIndex = 29;
      this.label26.Text = "sec";
      // 
      // label31
      // 
      this.label31.AutoSize = true;
      this.label31.Location = new System.Drawing.Point(246, 99);
      this.label31.Name = "label31";
      this.label31.Size = new System.Drawing.Size(24, 13);
      this.label31.TabIndex = 28;
      this.label31.Text = "sec";
      // 
      // label32
      // 
      this.label32.AutoSize = true;
      this.label32.Location = new System.Drawing.Point(6, 125);
      this.label32.Name = "label32";
      this.label32.Size = new System.Drawing.Size(115, 13);
      this.label32.TabIndex = 26;
      this.label32.Text = "Wait for timeshifting file";
      // 
      // label33
      // 
      this.label33.AutoSize = true;
      this.label33.Location = new System.Drawing.Point(6, 99);
      this.label33.Name = "label33";
      this.label33.Size = new System.Drawing.Size(137, 13);
      this.label33.TabIndex = 24;
      this.label33.Text = "Wait for unscrambled signal";
      // 
      // label34
      // 
      this.label34.AutoSize = true;
      this.label34.Location = new System.Drawing.Point(246, 73);
      this.label34.Name = "label34";
      this.label34.Size = new System.Drawing.Size(55, 13);
      this.label34.TabIndex = 23;
      this.label34.Text = "MByte/file";
      // 
      // label35
      // 
      this.label35.AutoSize = true;
      this.label35.Location = new System.Drawing.Point(246, 47);
      this.label35.Name = "label35";
      this.label35.Size = new System.Drawing.Size(25, 13);
      this.label35.TabIndex = 22;
      this.label35.Text = "files";
      // 
      // label36
      // 
      this.label36.AutoSize = true;
      this.label36.Location = new System.Drawing.Point(6, 73);
      this.label36.Name = "label36";
      this.label36.Size = new System.Drawing.Size(41, 13);
      this.label36.TabIndex = 17;
      this.label36.Text = "Filesize";
      // 
      // label37
      // 
      this.label37.AutoSize = true;
      this.label37.Location = new System.Drawing.Point(246, 21);
      this.label37.Name = "label37";
      this.label37.Size = new System.Drawing.Size(25, 13);
      this.label37.TabIndex = 21;
      this.label37.Text = "files";
      // 
      // label39
      // 
      this.label39.AutoSize = true;
      this.label39.Location = new System.Drawing.Point(6, 47);
      this.label39.Name = "label39";
      this.label39.Size = new System.Drawing.Size(51, 13);
      this.label39.TabIndex = 16;
      this.label39.Text = "Maximum";
      // 
      // label46
      // 
      this.label46.AutoSize = true;
      this.label46.Location = new System.Drawing.Point(6, 21);
      this.label46.Name = "label46";
      this.label46.Size = new System.Drawing.Size(48, 13);
      this.label46.TabIndex = 15;
      this.label46.Text = "Minimum";
      // 
      // tpTimeshifingFolders
      // 
      this.tpTimeshifingFolders.Controls.Add(this.groupBoxTimeshiftSettings);
      this.tpTimeshifingFolders.Location = new System.Drawing.Point(4, 22);
      this.tpTimeshifingFolders.Name = "tpTimeshifingFolders";
      this.tpTimeshifingFolders.Size = new System.Drawing.Size(457, 374);
      this.tpTimeshifingFolders.TabIndex = 3;
      this.tpTimeshifingFolders.Text = "Folders";
      this.tpTimeshifingFolders.UseVisualStyleBackColor = true;
      // 
      // groupBoxTimeshiftSettings
      // 
      this.groupBoxTimeshiftSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxTimeshiftSettings.Controls.Add(this.buttonSameTimeshiftFolder);
      this.groupBoxTimeshiftSettings.Controls.Add(this.pictureBox1);
      this.groupBoxTimeshiftSettings.Controls.Add(this.labelTSFolder);
      this.groupBoxTimeshiftSettings.Controls.Add(this.comboBoxCards);
      this.groupBoxTimeshiftSettings.Controls.Add(this.buttonTimeShiftBrowse);
      this.groupBoxTimeshiftSettings.Controls.Add(this.label22);
      this.groupBoxTimeshiftSettings.Controls.Add(this.textBoxTimeShiftFolder);
      this.groupBoxTimeshiftSettings.Location = new System.Drawing.Point(6, 3);
      this.groupBoxTimeshiftSettings.Name = "groupBoxTimeshiftSettings";
      this.groupBoxTimeshiftSettings.Size = new System.Drawing.Size(445, 365);
      this.groupBoxTimeshiftSettings.TabIndex = 26;
      this.groupBoxTimeshiftSettings.TabStop = false;
      this.groupBoxTimeshiftSettings.Text = "Card settings";
      // 
      // buttonSameTimeshiftFolder
      // 
      this.buttonSameTimeshiftFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonSameTimeshiftFolder.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonSameTimeshiftFolder.Location = new System.Drawing.Point(110, 228);
      this.buttonSameTimeshiftFolder.Name = "buttonSameTimeshiftFolder";
      this.buttonSameTimeshiftFolder.Size = new System.Drawing.Size(181, 20);
      this.buttonSameTimeshiftFolder.TabIndex = 28;
      this.buttonSameTimeshiftFolder.Text = "Same timeshift folder for all cards";
      this.buttonSameTimeshiftFolder.UseVisualStyleBackColor = true;
      this.buttonSameTimeshiftFolder.Click += new System.EventHandler(this.buttonSameTimeshiftFolder_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(23, 43);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(33, 23);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBox1.TabIndex = 18;
      this.pictureBox1.TabStop = false;
      // 
      // labelTSFolder
      // 
      this.labelTSFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelTSFolder.AutoSize = true;
      this.labelTSFolder.Location = new System.Drawing.Point(20, 170);
      this.labelTSFolder.Name = "labelTSFolder";
      this.labelTSFolder.Size = new System.Drawing.Size(362, 26);
      this.labelTSFolder.TabIndex = 25;
      this.labelTSFolder.Text = "Placing a timeshift folder on a dedicated disk will improve your performance \r\n(e" +
          ".g. with many simultaneous clients / recordings)";
      // 
      // comboBoxCards
      // 
      this.comboBoxCards.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxCards.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCards.FormattingEnabled = true;
      this.comboBoxCards.Location = new System.Drawing.Point(74, 43);
      this.comboBoxCards.Name = "comboBoxCards";
      this.comboBoxCards.Size = new System.Drawing.Size(352, 21);
      this.comboBoxCards.TabIndex = 0;
      this.comboBoxCards.SelectedIndexChanged += new System.EventHandler(this.comboBoxCards_SelectedIndexChanged);
      // 
      // buttonTimeShiftBrowse
      // 
      this.buttonTimeShiftBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTimeShiftBrowse.Location = new System.Drawing.Point(355, 120);
      this.buttonTimeShiftBrowse.Name = "buttonTimeShiftBrowse";
      this.buttonTimeShiftBrowse.Size = new System.Drawing.Size(51, 20);
      this.buttonTimeShiftBrowse.TabIndex = 21;
      this.buttonTimeShiftBrowse.Text = "Browse";
      this.buttonTimeShiftBrowse.UseVisualStyleBackColor = true;
      this.buttonTimeShiftBrowse.Click += new System.EventHandler(this.buttonTimeShiftBrowse_Click);
      // 
      // label22
      // 
      this.label22.AutoSize = true;
      this.label22.Location = new System.Drawing.Point(20, 101);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(81, 13);
      this.label22.TabIndex = 20;
      this.label22.Text = "Timeshift folder:";
      // 
      // textBoxTimeShiftFolder
      // 
      this.textBoxTimeShiftFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxTimeShiftFolder.Location = new System.Drawing.Point(23, 120);
      this.textBoxTimeShiftFolder.Name = "textBoxTimeShiftFolder";
      this.textBoxTimeShiftFolder.ReadOnly = true;
      this.textBoxTimeShiftFolder.Size = new System.Drawing.Size(326, 20);
      this.textBoxTimeShiftFolder.TabIndex = 19;
      this.textBoxTimeShiftFolder.TextChanged += new System.EventHandler(this.textBoxTimeShiftFolder_TextChanged);
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
      // 
      // label21
      // 
      this.label21.AutoSize = true;
      this.label21.Location = new System.Drawing.Point(189, 157);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(29, 13);
      this.label21.TabIndex = 21;
      this.label21.Text = "secs";
      // 
      // tabPageTS
      // 
      this.tabPageTS.Controls.Add(this.groupBox3);
      this.tabPageTS.Location = new System.Drawing.Point(4, 22);
      this.tabPageTS.Name = "tabPageTS";
      this.tabPageTS.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTS.Size = new System.Drawing.Size(476, 428);
      this.tabPageTS.TabIndex = 3;
      this.tabPageTS.Text = "Timeshifting";
      this.tabPageTS.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.label43);
      this.groupBox3.Controls.Add(this.label42);
      this.groupBox3.Controls.Add(this.label41);
      this.groupBox3.Controls.Add(this.label40);
      this.groupBox3.Controls.Add(this.label16);
      this.groupBox3.Controls.Add(this.label17);
      this.groupBox3.Controls.Add(this.label20);
      this.groupBox3.Controls.Add(this.label18);
      this.groupBox3.Controls.Add(this.label19);
      this.groupBox3.Controls.Add(this.label25);
      this.groupBox3.Location = new System.Drawing.Point(6, 6);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(304, 152);
      this.groupBox3.TabIndex = 17;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Timeshifting";
      // 
      // label43
      // 
      this.label43.AutoSize = true;
      this.label43.Location = new System.Drawing.Point(246, 125);
      this.label43.Name = "label43";
      this.label43.Size = new System.Drawing.Size(24, 13);
      this.label43.TabIndex = 29;
      this.label43.Text = "sec";
      // 
      // label42
      // 
      this.label42.AutoSize = true;
      this.label42.Location = new System.Drawing.Point(246, 99);
      this.label42.Name = "label42";
      this.label42.Size = new System.Drawing.Size(24, 13);
      this.label42.TabIndex = 28;
      this.label42.Text = "sec";
      // 
      // label41
      // 
      this.label41.AutoSize = true;
      this.label41.Location = new System.Drawing.Point(6, 125);
      this.label41.Name = "label41";
      this.label41.Size = new System.Drawing.Size(115, 13);
      this.label41.TabIndex = 26;
      this.label41.Text = "Wait for timeshifting file";
      // 
      // label40
      // 
      this.label40.AutoSize = true;
      this.label40.Location = new System.Drawing.Point(6, 99);
      this.label40.Name = "label40";
      this.label40.Size = new System.Drawing.Size(137, 13);
      this.label40.TabIndex = 24;
      this.label40.Text = "Wait for unscrambled signal";
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(246, 73);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(55, 13);
      this.label16.TabIndex = 23;
      this.label16.Text = "MByte/file";
      // 
      // label17
      // 
      this.label17.AutoSize = true;
      this.label17.Location = new System.Drawing.Point(246, 47);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(25, 13);
      this.label17.TabIndex = 22;
      this.label17.Text = "files";
      // 
      // label20
      // 
      this.label20.AutoSize = true;
      this.label20.Location = new System.Drawing.Point(6, 73);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(41, 13);
      this.label20.TabIndex = 17;
      this.label20.Text = "Filesize";
      // 
      // label18
      // 
      this.label18.AutoSize = true;
      this.label18.Location = new System.Drawing.Point(246, 21);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(25, 13);
      this.label18.TabIndex = 21;
      this.label18.Text = "files";
      // 
      // label19
      // 
      this.label19.AutoSize = true;
      this.label19.Location = new System.Drawing.Point(6, 47);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(51, 13);
      this.label19.TabIndex = 16;
      this.label19.Text = "Maximum";
      // 
      // label25
      // 
      this.label25.AutoSize = true;
      this.label25.Location = new System.Drawing.Point(6, 21);
      this.label25.Name = "label25";
      this.label25.Size = new System.Drawing.Size(48, 13);
      this.label25.TabIndex = 15;
      this.label25.Text = "Minimum";
      // 
      // TvTimeshifting
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "TvTimeshifting";
      this.Size = new System.Drawing.Size(474, 412);
      this.tabControl1.ResumeLayout(false);
      this.tpTimeshifing.ResumeLayout(false);
      this.grpBoxTimeshiftingInfo.ResumeLayout(false);
      this.grpBoxTimeshiftingInfo.PerformLayout();
      this.groupBox10.ResumeLayout(false);
      this.groupBox10.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxFreeCardsToTry)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWaitTimeshifting)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWaitUnscrambled)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxFileSize)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxFiles)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinFiles)).EndInit();
      this.tpTimeshifingFolders.ResumeLayout(false);
      this.groupBoxTimeshiftSettings.ResumeLayout(false);
      this.groupBoxTimeshiftSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.tabPageTS.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tpTimeshifingFolders;
    private System.Windows.Forms.ComboBox comboBoxCards;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Button buttonTimeShiftBrowse;
    private System.Windows.Forms.Label label22;
    private System.Windows.Forms.TextBox textBoxTimeShiftFolder;
    private System.Windows.Forms.Label labelTSFolder;
    private System.Windows.Forms.GroupBox groupBoxTimeshiftSettings;
    private System.Windows.Forms.Button buttonSameTimeshiftFolder;
    private System.Windows.Forms.Label label21;
    private System.Windows.Forms.TabPage tabPageTS;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Label label43;
    private System.Windows.Forms.Label label42;
    private System.Windows.Forms.Label label41;
    private System.Windows.Forms.Label label40;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.Label label17;
    private System.Windows.Forms.Label label20;
    private System.Windows.Forms.Label label18;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.Label label25;
    private System.Windows.Forms.TabPage tpTimeshifing;
    private System.Windows.Forms.GroupBox groupBox10;
    private System.Windows.Forms.NumericUpDown numericUpDownWaitTimeshifting;
    private System.Windows.Forms.NumericUpDown numericUpDownWaitUnscrambled;
    private System.Windows.Forms.NumericUpDown numericUpDownMaxFileSize;
    private System.Windows.Forms.NumericUpDown numericUpDownMaxFiles;
    private System.Windows.Forms.NumericUpDown numericUpDownMinFiles;
    private System.Windows.Forms.Label label26;
    private System.Windows.Forms.Label label31;
    private System.Windows.Forms.Label label32;
    private System.Windows.Forms.Label label33;
    private System.Windows.Forms.Label label34;
    private System.Windows.Forms.Label label35;
    private System.Windows.Forms.Label label36;
    private System.Windows.Forms.Label label37;
    private System.Windows.Forms.Label label39;
    private System.Windows.Forms.Label label46;
    private MediaPortal.UserInterface.Controls.MPLabel lblMinFileSizeNeeded;
    private MediaPortal.UserInterface.Controls.MPLabel lblOverhead;
    private MediaPortal.UserInterface.Controls.MPLabel lblFileSizeNeeded;
    private MediaPortal.UserInterface.Controls.MPGroupBox grpBoxTimeshiftingInfo;
    private MediaPortal.UserInterface.Controls.MPLabel lblTimeSD;
    private MediaPortal.UserInterface.Controls.MPLabel lblTimeHD;
    private System.Windows.Forms.NumericUpDown numericUpDownMaxFreeCardsToTry;
    private System.Windows.Forms.Label label47;
    private System.Windows.Forms.Label label48;
  }
}