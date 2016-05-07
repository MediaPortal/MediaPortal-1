namespace SetupTv.Sections
{
  partial class Epg
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
      this.groupBox9 = new System.Windows.Forms.GroupBox();
      this.label31 = new System.Windows.Forms.Label();
      this.edTitleTemplate = new System.Windows.Forms.TextBox();
      this.label27 = new System.Windows.Forms.Label();
      this.label28 = new System.Windows.Forms.Label();
      this.label38 = new System.Windows.Forms.Label();
      this.edDescriptionTemplate = new System.Windows.Forms.TextBox();
      this.label30 = new System.Windows.Forms.Label();
      this.edTitleTest = new System.Windows.Forms.TextBox();
      this.label29 = new System.Windows.Forms.Label();
      this.edDescriptionTest = new System.Windows.Forms.TextBox();
      this.btnTest = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.numericUpDownEpgRefresh = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownEpgTimeOut = new System.Windows.Forms.NumericUpDown();
      this.checkBoxEnableEPGWhileIdle = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label15 = new System.Windows.Forms.Label();
      this.label14 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.checkBoxEnableEPGWhileIdleOnAllTuners = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox5 = new System.Windows.Forms.GroupBox();
      this.numericUpDownEpgTimeshiftRefresh = new System.Windows.Forms.NumericUpDown();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.numericUpDownTSEpgTimeout = new System.Windows.Forms.NumericUpDown();
      this.checkBoxEnableEpgWhileTimeshifting = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label22 = new System.Windows.Forms.Label();
      this.label23 = new System.Windows.Forms.Label();
      this.groupBox7 = new System.Windows.Forms.GroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.numericEpgCardLimit = new System.Windows.Forms.NumericUpDown();
      this.checkboxSameTransponder = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.checkBoxEnableCRCCheck = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxAlwaysUpdate = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxAlwaysFillHoles = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tabControlEpg = new System.Windows.Forms.TabControl();
      this.groupBox9.SuspendLayout();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgRefresh)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgTimeOut)).BeginInit();
      this.groupBox5.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgTimeshiftRefresh)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTSEpgTimeout)).BeginInit();
      this.groupBox7.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericEpgCardLimit)).BeginInit();
      this.tabPage1.SuspendLayout();
      this.tabControlEpg.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox9
      // 
      this.groupBox9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox9.Controls.Add(this.label31);
      this.groupBox9.Controls.Add(this.edTitleTemplate);
      this.groupBox9.Controls.Add(this.label27);
      this.groupBox9.Controls.Add(this.label28);
      this.groupBox9.Controls.Add(this.label38);
      this.groupBox9.Controls.Add(this.edDescriptionTemplate);
      this.groupBox9.Controls.Add(this.label30);
      this.groupBox9.Controls.Add(this.edTitleTest);
      this.groupBox9.Controls.Add(this.label29);
      this.groupBox9.Controls.Add(this.edDescriptionTest);
      this.groupBox9.Controls.Add(this.btnTest);
      this.groupBox9.Location = new System.Drawing.Point(3, 215);
      this.groupBox9.Name = "groupBox9";
      this.groupBox9.Size = new System.Drawing.Size(457, 192);
      this.groupBox9.TabIndex = 41;
      this.groupBox9.TabStop = false;
      this.groupBox9.Text = "Display options";
      // 
      // label31
      // 
      this.label31.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label31.Location = new System.Drawing.Point(326, 80);
      this.label31.Name = "label31";
      this.label31.Size = new System.Drawing.Size(125, 106);
      this.label31.TabIndex = 37;
      this.label31.Text = "%TITLE%\r\n%DESCRIPTION%\r\n%GENRE%\r\n%STARRATING%\r\n%STARRATING_STR%\r\n%CLASSIFICATION%" +
    "\r\n%PARENTALRATING%\r\n%NEWLINE%";
      // 
      // edTitleTemplate
      // 
      this.edTitleTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edTitleTemplate.Location = new System.Drawing.Point(75, 19);
      this.edTitleTemplate.Name = "edTitleTemplate";
      this.edTitleTemplate.Size = new System.Drawing.Size(245, 20);
      this.edTitleTemplate.TabIndex = 20;
      // 
      // label27
      // 
      this.label27.AutoSize = true;
      this.label27.Location = new System.Drawing.Point(6, 22);
      this.label27.Name = "label27";
      this.label27.Size = new System.Drawing.Size(30, 13);
      this.label27.TabIndex = 19;
      this.label27.Text = "Title:";
      // 
      // label28
      // 
      this.label28.AutoSize = true;
      this.label28.Location = new System.Drawing.Point(6, 48);
      this.label28.Name = "label28";
      this.label28.Size = new System.Drawing.Size(63, 13);
      this.label28.TabIndex = 21;
      this.label28.Text = "Description:";
      // 
      // label38
      // 
      this.label38.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label38.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label38.Location = new System.Drawing.Point(326, 16);
      this.label38.Name = "label38";
      this.label38.Size = new System.Drawing.Size(125, 61);
      this.label38.TabIndex = 35;
      this.label38.Text = "You can use any combination of the placeholders shown below";
      // 
      // edDescriptionTemplate
      // 
      this.edDescriptionTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edDescriptionTemplate.Location = new System.Drawing.Point(75, 45);
      this.edDescriptionTemplate.Name = "edDescriptionTemplate";
      this.edDescriptionTemplate.Size = new System.Drawing.Size(245, 20);
      this.edDescriptionTemplate.TabIndex = 22;
      // 
      // label30
      // 
      this.label30.AutoSize = true;
      this.label30.Location = new System.Drawing.Point(6, 103);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(30, 13);
      this.label30.TabIndex = 23;
      this.label30.Text = "Title:";
      // 
      // edTitleTest
      // 
      this.edTitleTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edTitleTest.Location = new System.Drawing.Point(75, 100);
      this.edTitleTest.Name = "edTitleTest";
      this.edTitleTest.ReadOnly = true;
      this.edTitleTest.Size = new System.Drawing.Size(245, 20);
      this.edTitleTest.TabIndex = 24;
      // 
      // label29
      // 
      this.label29.AutoSize = true;
      this.label29.Location = new System.Drawing.Point(6, 129);
      this.label29.Name = "label29";
      this.label29.Size = new System.Drawing.Size(63, 13);
      this.label29.TabIndex = 25;
      this.label29.Text = "Description:";
      // 
      // edDescriptionTest
      // 
      this.edDescriptionTest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edDescriptionTest.Location = new System.Drawing.Point(75, 126);
      this.edDescriptionTest.Multiline = true;
      this.edDescriptionTest.Name = "edDescriptionTest";
      this.edDescriptionTest.ReadOnly = true;
      this.edDescriptionTest.Size = new System.Drawing.Size(245, 58);
      this.edDescriptionTest.TabIndex = 26;
      // 
      // btnTest
      // 
      this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTest.Location = new System.Drawing.Point(245, 71);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new System.Drawing.Size(75, 23);
      this.btnTest.TabIndex = 27;
      this.btnTest.Text = "Test";
      this.btnTest.UseVisualStyleBackColor = true;
      this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.numericUpDownEpgRefresh);
      this.groupBox2.Controls.Add(this.numericUpDownEpgTimeOut);
      this.groupBox2.Controls.Add(this.checkBoxEnableEPGWhileIdle);
      this.groupBox2.Controls.Add(this.label15);
      this.groupBox2.Controls.Add(this.label14);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Location = new System.Drawing.Point(235, 123);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(225, 88);
      this.groupBox2.TabIndex = 40;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "EPG grabbing while idle";
      // 
      // numericUpDownEpgRefresh
      // 
      this.numericUpDownEpgRefresh.Location = new System.Drawing.Point(86, 63);
      this.numericUpDownEpgRefresh.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownEpgRefresh.Name = "numericUpDownEpgRefresh";
      this.numericUpDownEpgRefresh.Size = new System.Drawing.Size(85, 20);
      this.numericUpDownEpgRefresh.TabIndex = 10;
      this.numericUpDownEpgRefresh.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownEpgRefresh.Value = new decimal(new int[] {
            240,
            0,
            0,
            0});
      // 
      // numericUpDownEpgTimeOut
      // 
      this.numericUpDownEpgTimeOut.Location = new System.Drawing.Point(86, 37);
      this.numericUpDownEpgTimeOut.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownEpgTimeOut.Name = "numericUpDownEpgTimeOut";
      this.numericUpDownEpgTimeOut.Size = new System.Drawing.Size(85, 20);
      this.numericUpDownEpgTimeOut.TabIndex = 10;
      this.numericUpDownEpgTimeOut.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownEpgTimeOut.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // checkBoxEnableEPGWhileIdle
      // 
      this.checkBoxEnableEPGWhileIdle.AutoSize = true;
      this.checkBoxEnableEPGWhileIdle.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEnableEPGWhileIdle.Location = new System.Drawing.Point(10, 19);
      this.checkBoxEnableEPGWhileIdle.Name = "checkBoxEnableEPGWhileIdle";
      this.checkBoxEnableEPGWhileIdle.Size = new System.Drawing.Size(63, 17);
      this.checkBoxEnableEPGWhileIdle.TabIndex = 11;
      this.checkBoxEnableEPGWhileIdle.Text = "Enabled";
      this.checkBoxEnableEPGWhileIdle.UseVisualStyleBackColor = true;
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(177, 63);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(28, 13);
      this.label15.TabIndex = 7;
      this.label15.Text = "mins";
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(7, 65);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(76, 13);
      this.label14.TabIndex = 5;
      this.label14.Text = "Refresh every:";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(177, 39);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(28, 13);
      this.label8.TabIndex = 4;
      this.label8.Text = "mins";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(7, 39);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(48, 13);
      this.label7.TabIndex = 2;
      this.label7.Text = "Timeout:";
      // 
      // checkBoxEnableEPGWhileIdleOnAllTuners
      // 
      this.checkBoxEnableEPGWhileIdleOnAllTuners.AutoSize = true;
      this.checkBoxEnableEPGWhileIdleOnAllTuners.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEnableEPGWhileIdleOnAllTuners.Location = new System.Drawing.Point(11, 87);
      this.checkBoxEnableEPGWhileIdleOnAllTuners.Name = "checkBoxEnableEPGWhileIdleOnAllTuners";
      this.checkBoxEnableEPGWhileIdleOnAllTuners.Size = new System.Drawing.Size(71, 17);
      this.checkBoxEnableEPGWhileIdleOnAllTuners.TabIndex = 12;
      this.checkBoxEnableEPGWhileIdleOnAllTuners.Text = "Multi-EPG";
      this.checkBoxEnableEPGWhileIdleOnAllTuners.UseVisualStyleBackColor = true;
      this.checkBoxEnableEPGWhileIdleOnAllTuners.CheckedChanged += new System.EventHandler(this.mpCheckBox1_CheckedChanged);
      // 
      // groupBox5
      // 
      this.groupBox5.Controls.Add(this.numericUpDownEpgTimeshiftRefresh);
      this.groupBox5.Controls.Add(this.label3);
      this.groupBox5.Controls.Add(this.label4);
      this.groupBox5.Controls.Add(this.numericUpDownTSEpgTimeout);
      this.groupBox5.Controls.Add(this.checkBoxEnableEpgWhileTimeshifting);
      this.groupBox5.Controls.Add(this.label22);
      this.groupBox5.Controls.Add(this.label23);
      this.groupBox5.Location = new System.Drawing.Point(3, 123);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(226, 88);
      this.groupBox5.TabIndex = 39;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "EPG grabbing while timeshifting/recording";
      // 
      // numericUpDownEpgTimeshiftRefresh
      // 
      this.numericUpDownEpgTimeshiftRefresh.Location = new System.Drawing.Point(140, 61);
      this.numericUpDownEpgTimeshiftRefresh.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownEpgTimeshiftRefresh.Name = "numericUpDownEpgTimeshiftRefresh";
      this.numericUpDownEpgTimeshiftRefresh.Size = new System.Drawing.Size(47, 20);
      this.numericUpDownEpgTimeshiftRefresh.TabIndex = 13;
      this.numericUpDownEpgTimeshiftRefresh.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownEpgTimeshiftRefresh.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(193, 61);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(29, 13);
      this.label3.TabIndex = 12;
      this.label3.Text = "secs";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(8, 63);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(76, 13);
      this.label4.TabIndex = 11;
      this.label4.Text = "Refresh every:";
      // 
      // numericUpDownTSEpgTimeout
      // 
      this.numericUpDownTSEpgTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.numericUpDownTSEpgTimeout.Location = new System.Drawing.Point(140, 37);
      this.numericUpDownTSEpgTimeout.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownTSEpgTimeout.Name = "numericUpDownTSEpgTimeout";
      this.numericUpDownTSEpgTimeout.Size = new System.Drawing.Size(48, 20);
      this.numericUpDownTSEpgTimeout.TabIndex = 10;
      this.numericUpDownTSEpgTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTSEpgTimeout.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
      this.numericUpDownTSEpgTimeout.Visible = false;
      // 
      // checkBoxEnableEpgWhileTimeshifting
      // 
      this.checkBoxEnableEpgWhileTimeshifting.AutoSize = true;
      this.checkBoxEnableEpgWhileTimeshifting.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEnableEpgWhileTimeshifting.Location = new System.Drawing.Point(11, 19);
      this.checkBoxEnableEpgWhileTimeshifting.Name = "checkBoxEnableEpgWhileTimeshifting";
      this.checkBoxEnableEpgWhileTimeshifting.Size = new System.Drawing.Size(63, 17);
      this.checkBoxEnableEpgWhileTimeshifting.TabIndex = 9;
      this.checkBoxEnableEpgWhileTimeshifting.Text = "Enabled";
      this.checkBoxEnableEpgWhileTimeshifting.UseVisualStyleBackColor = true;
      // 
      // label22
      // 
      this.label22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label22.AutoSize = true;
      this.label22.Location = new System.Drawing.Point(193, 42);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(29, 13);
      this.label22.TabIndex = 7;
      this.label22.Text = "secs";
      this.label22.Visible = false;
      // 
      // label23
      // 
      this.label23.AutoSize = true;
      this.label23.Location = new System.Drawing.Point(8, 39);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(127, 13);
      this.label23.TabIndex = 5;
      this.label23.Text = "Timeout (value + refresh):";
      this.label23.Visible = false;
      this.label23.Click += new System.EventHandler(this.label23_Click);
      // 
      // groupBox7
      // 
      this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox7.Controls.Add(this.label2);
      this.groupBox7.Controls.Add(this.numericEpgCardLimit);
      this.groupBox7.Controls.Add(this.checkBoxEnableEPGWhileIdleOnAllTuners);
      this.groupBox7.Controls.Add(this.checkboxSameTransponder);
      this.groupBox7.Controls.Add(this.label1);
      this.groupBox7.Controls.Add(this.checkBoxEnableCRCCheck);
      this.groupBox7.Controls.Add(this.checkBoxAlwaysUpdate);
      this.groupBox7.Controls.Add(this.checkBoxAlwaysFillHoles);
      this.groupBox7.Location = new System.Drawing.Point(3, 3);
      this.groupBox7.Name = "groupBox7";
      this.groupBox7.Size = new System.Drawing.Size(457, 114);
      this.groupBox7.TabIndex = 38;
      this.groupBox7.TabStop = false;
      this.groupBox7.Text = "General";
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(229, 91);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(141, 13);
      this.label2.TabIndex = 11;
      this.label2.Text = "Number of tuners to be used";
      this.label2.Click += new System.EventHandler(this.label2_Click);
      // 
      // numericEpgCardLimit
      // 
      this.numericEpgCardLimit.Location = new System.Drawing.Point(140, 87);
      this.numericEpgCardLimit.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericEpgCardLimit.Name = "numericEpgCardLimit";
      this.numericEpgCardLimit.Size = new System.Drawing.Size(86, 20);
      this.numericEpgCardLimit.TabIndex = 11;
      this.numericEpgCardLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericEpgCardLimit.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
      this.numericEpgCardLimit.ValueChanged += new System.EventHandler(this.numericEpgCardLimit_ValueChanged);
      // 
      // checkboxSameTransponder
      // 
      this.checkboxSameTransponder.AutoSize = true;
      this.checkboxSameTransponder.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkboxSameTransponder.Location = new System.Drawing.Point(11, 65);
      this.checkboxSameTransponder.Name = "checkboxSameTransponder";
      this.checkboxSameTransponder.Size = new System.Drawing.Size(257, 17);
      this.checkboxSameTransponder.TabIndex = 14;
      this.checkboxSameTransponder.Text = "Grab EPG only for channels on same transponder";
      this.checkboxSameTransponder.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(132, 39);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(321, 26);
      this.label1.TabIndex = 13;
      this.label1.Text = "(This will increase stability of EPG grabbing.If your provider doesn\'t \r\nbroadcas" +
    "t CRC checksums, you have to disable it)";
      // 
      // checkBoxEnableCRCCheck
      // 
      this.checkBoxEnableCRCCheck.AutoSize = true;
      this.checkBoxEnableCRCCheck.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEnableCRCCheck.Location = new System.Drawing.Point(11, 42);
      this.checkBoxEnableCRCCheck.Name = "checkBoxEnableCRCCheck";
      this.checkBoxEnableCRCCheck.Size = new System.Drawing.Size(115, 17);
      this.checkBoxEnableCRCCheck.TabIndex = 12;
      this.checkBoxEnableCRCCheck.Text = "Enable CRC check";
      this.checkBoxEnableCRCCheck.UseVisualStyleBackColor = true;
      // 
      // checkBoxAlwaysUpdate
      // 
      this.checkBoxAlwaysUpdate.AutoSize = true;
      this.checkBoxAlwaysUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAlwaysUpdate.Location = new System.Drawing.Point(140, 19);
      this.checkBoxAlwaysUpdate.Name = "checkBoxAlwaysUpdate";
      this.checkBoxAlwaysUpdate.Size = new System.Drawing.Size(310, 17);
      this.checkBoxAlwaysUpdate.TabIndex = 11;
      this.checkBoxAlwaysUpdate.Text = "Always try to update existing entries (might raise CPU usage!)";
      this.checkBoxAlwaysUpdate.UseVisualStyleBackColor = true;
      // 
      // checkBoxAlwaysFillHoles
      // 
      this.checkBoxAlwaysFillHoles.AutoSize = true;
      this.checkBoxAlwaysFillHoles.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAlwaysFillHoles.Location = new System.Drawing.Point(11, 19);
      this.checkBoxAlwaysFillHoles.Name = "checkBoxAlwaysFillHoles";
      this.checkBoxAlwaysFillHoles.Size = new System.Drawing.Size(123, 17);
      this.checkBoxAlwaysFillHoles.TabIndex = 9;
      this.checkBoxAlwaysFillHoles.Text = "Always try to fill holes";
      this.checkBoxAlwaysFillHoles.UseVisualStyleBackColor = true;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.groupBox9);
      this.tabPage1.Controls.Add(this.groupBox7);
      this.tabPage1.Controls.Add(this.groupBox2);
      this.tabPage1.Controls.Add(this.groupBox5);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(466, 410);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "DVB EPG";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // tabControlEpg
      // 
      this.tabControlEpg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlEpg.Controls.Add(this.tabPage1);
      this.tabControlEpg.Location = new System.Drawing.Point(0, 0);
      this.tabControlEpg.Name = "tabControlEpg";
      this.tabControlEpg.SelectedIndex = 0;
      this.tabControlEpg.Size = new System.Drawing.Size(474, 436);
      this.tabControlEpg.TabIndex = 1;
      // 
      // Epg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControlEpg);
      this.Name = "Epg";
      this.Size = new System.Drawing.Size(474, 439);
      this.groupBox9.ResumeLayout(false);
      this.groupBox9.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgRefresh)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgTimeOut)).EndInit();
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgTimeshiftRefresh)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTSEpgTimeout)).EndInit();
      this.groupBox7.ResumeLayout(false);
      this.groupBox7.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericEpgCardLimit)).EndInit();
      this.tabPage1.ResumeLayout(false);
      this.tabControlEpg.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox7;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAlwaysUpdate;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAlwaysFillHoles;
    private System.Windows.Forms.GroupBox groupBox5;
    private System.Windows.Forms.NumericUpDown numericUpDownTSEpgTimeout;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableEpgWhileTimeshifting;
    private System.Windows.Forms.Label label22;
    private System.Windows.Forms.Label label23;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.NumericUpDown numericUpDownEpgRefresh;
    private System.Windows.Forms.NumericUpDown numericUpDownEpgTimeOut;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableEPGWhileIdle;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.GroupBox groupBox9;
    private System.Windows.Forms.Label label31;
    private System.Windows.Forms.TextBox edTitleTemplate;
    private System.Windows.Forms.Label label27;
    private System.Windows.Forms.Label label28;
    private System.Windows.Forms.Label label38;
    private System.Windows.Forms.TextBox edDescriptionTemplate;
    private System.Windows.Forms.Label label30;
    private System.Windows.Forms.TextBox edTitleTest;
    private System.Windows.Forms.Label label29;
    private System.Windows.Forms.TextBox edDescriptionTest;
    private System.Windows.Forms.Button btnTest;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabControl tabControlEpg;
    private System.Windows.Forms.Label label1;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableCRCCheck;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkboxSameTransponder;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableEPGWhileIdleOnAllTuners;
    private System.Windows.Forms.NumericUpDown numericEpgCardLimit;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.NumericUpDown numericUpDownEpgTimeshiftRefresh;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
  }
}
