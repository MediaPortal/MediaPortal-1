namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    partial class SoundGraphImonSettingsForm
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
            this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabImon = new System.Windows.Forms.TabPage();
            this.groupBoxWhenPlaying = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textReenableWhenPlayingAfterDelayInSeconds = new System.Windows.Forms.MaskedTextBox();
            this.checkReenableWhenPlayingAfter = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textDisableWhenPlayingDelayInSeconds = new System.Windows.Forms.MaskedTextBox();
            this.checkDisableWhenPlaying = new System.Windows.Forms.CheckBox();
            this.groupBoxWhenIdle = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textReenableWhenIdleAfterDelayInSeconds = new System.Windows.Forms.MaskedTextBox();
            this.checkReenableWhenIdleAfter = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textDisableWhenIdleDelayInSeconds = new System.Windows.Forms.MaskedTextBox();
            this.checkDisableWhenIdle = new System.Windows.Forms.CheckBox();
            this.checkDisableWhenInBackground = new System.Windows.Forms.CheckBox();
            this.tabEqualizer = new System.Windows.Forms.TabPage();
            this.textEqEnabledTimeInSeconds = new System.Windows.Forms.MaskedTextBox();
            this.textEqDisabledTimeInSeconds = new System.Windows.Forms.MaskedTextBox();
            this.textEqStartDelayInSeconds = new System.Windows.Forms.MaskedTextBox();
            this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpLabelEQTitleDisplay = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpEQTitleDisplay = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpSmoothEQ = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpEqDisplay = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpRestrictEQ = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.cmbEqRate = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.mpDelayEQ = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.tabLcd = new System.Windows.Forms.TabPage();
            this.gbxLineOptions = new System.Windows.Forms.GroupBox();
            this.gbxPlayback = new System.Windows.Forms.GroupBox();
            this.rbtnPlaybackSecondLine = new System.Windows.Forms.RadioButton();
            this.rbtnPlaybackFirstLine = new System.Windows.Forms.RadioButton();
            this.gbxGeneral = new System.Windows.Forms.GroupBox();
            this.rbtnGeneralSecondLine = new System.Windows.Forms.RadioButton();
            this.rbtnGeneralFirstLine = new System.Windows.Forms.RadioButton();
            this.tabControl.SuspendLayout();
            this.tabImon.SuspendLayout();
            this.groupBoxWhenPlaying.SuspendLayout();
            this.groupBoxWhenIdle.SuspendLayout();
            this.tabEqualizer.SuspendLayout();
            this.tabLcd.SuspendLayout();
            this.gbxLineOptions.SuspendLayout();
            this.gbxPlayback.SuspendLayout();
            this.gbxGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(249, 297);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(78, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabImon);
            this.tabControl.Controls.Add(this.tabEqualizer);
            this.tabControl.Controls.Add(this.tabLcd);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(315, 279);
            this.tabControl.TabIndex = 125;
            // 
            // tabImon
            // 
            this.tabImon.Controls.Add(this.groupBoxWhenPlaying);
            this.tabImon.Controls.Add(this.groupBoxWhenIdle);
            this.tabImon.Controls.Add(this.checkDisableWhenInBackground);
            this.tabImon.Location = new System.Drawing.Point(4, 22);
            this.tabImon.Name = "tabImon";
            this.tabImon.Padding = new System.Windows.Forms.Padding(3);
            this.tabImon.Size = new System.Drawing.Size(307, 253);
            this.tabImon.TabIndex = 0;
            this.tabImon.Text = "iMON";
            this.tabImon.UseVisualStyleBackColor = true;
            // 
            // groupBoxWhenPlaying
            // 
            this.groupBoxWhenPlaying.Controls.Add(this.label3);
            this.groupBoxWhenPlaying.Controls.Add(this.textReenableWhenPlayingAfterDelayInSeconds);
            this.groupBoxWhenPlaying.Controls.Add(this.checkReenableWhenPlayingAfter);
            this.groupBoxWhenPlaying.Controls.Add(this.label4);
            this.groupBoxWhenPlaying.Controls.Add(this.textDisableWhenPlayingDelayInSeconds);
            this.groupBoxWhenPlaying.Controls.Add(this.checkDisableWhenPlaying);
            this.groupBoxWhenPlaying.Location = new System.Drawing.Point(6, 97);
            this.groupBoxWhenPlaying.Name = "groupBoxWhenPlaying";
            this.groupBoxWhenPlaying.Size = new System.Drawing.Size(295, 66);
            this.groupBoxWhenPlaying.TabIndex = 9;
            this.groupBoxWhenPlaying.TabStop = false;
            this.groupBoxWhenPlaying.Text = "When playing";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(173, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "second(s)";
            // 
            // textReenableWhenPlayingAfterDelayInSeconds
            // 
            this.textReenableWhenPlayingAfterDelayInSeconds.Location = new System.Drawing.Point(128, 40);
            this.textReenableWhenPlayingAfterDelayInSeconds.Mask = "00000";
            this.textReenableWhenPlayingAfterDelayInSeconds.Name = "textReenableWhenPlayingAfterDelayInSeconds";
            this.textReenableWhenPlayingAfterDelayInSeconds.PromptChar = ' ';
            this.textReenableWhenPlayingAfterDelayInSeconds.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textReenableWhenPlayingAfterDelayInSeconds.Size = new System.Drawing.Size(39, 20);
            this.textReenableWhenPlayingAfterDelayInSeconds.TabIndex = 12;
            this.textReenableWhenPlayingAfterDelayInSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textReenableWhenPlayingAfterDelayInSeconds.ValidatingType = typeof(int);
            // 
            // checkReenableWhenPlayingAfter
            // 
            this.checkReenableWhenPlayingAfter.AutoSize = true;
            this.checkReenableWhenPlayingAfter.Location = new System.Drawing.Point(28, 42);
            this.checkReenableWhenPlayingAfter.Name = "checkReenableWhenPlayingAfter";
            this.checkReenableWhenPlayingAfter.Size = new System.Drawing.Size(96, 17);
            this.checkReenableWhenPlayingAfter.TabIndex = 11;
            this.checkReenableWhenPlayingAfter.Text = "Reenable after";
            this.checkReenableWhenPlayingAfter.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(173, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "second(s)";
            // 
            // textDisableWhenPlayingDelayInSeconds
            // 
            this.textDisableWhenPlayingDelayInSeconds.Location = new System.Drawing.Point(128, 17);
            this.textDisableWhenPlayingDelayInSeconds.Mask = "00000";
            this.textDisableWhenPlayingDelayInSeconds.Name = "textDisableWhenPlayingDelayInSeconds";
            this.textDisableWhenPlayingDelayInSeconds.PromptChar = ' ';
            this.textDisableWhenPlayingDelayInSeconds.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textDisableWhenPlayingDelayInSeconds.Size = new System.Drawing.Size(39, 20);
            this.textDisableWhenPlayingDelayInSeconds.TabIndex = 9;
            this.textDisableWhenPlayingDelayInSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textDisableWhenPlayingDelayInSeconds.ValidatingType = typeof(int);
            // 
            // checkDisableWhenPlaying
            // 
            this.checkDisableWhenPlaying.AutoSize = true;
            this.checkDisableWhenPlaying.Location = new System.Drawing.Point(28, 19);
            this.checkDisableWhenPlaying.Name = "checkDisableWhenPlaying";
            this.checkDisableWhenPlaying.Size = new System.Drawing.Size(85, 17);
            this.checkDisableWhenPlaying.TabIndex = 8;
            this.checkDisableWhenPlaying.Text = "Disable after";
            this.checkDisableWhenPlaying.UseVisualStyleBackColor = true;
            // 
            // groupBoxWhenIdle
            // 
            this.groupBoxWhenIdle.Controls.Add(this.label2);
            this.groupBoxWhenIdle.Controls.Add(this.textReenableWhenIdleAfterDelayInSeconds);
            this.groupBoxWhenIdle.Controls.Add(this.checkReenableWhenIdleAfter);
            this.groupBoxWhenIdle.Controls.Add(this.label1);
            this.groupBoxWhenIdle.Controls.Add(this.textDisableWhenIdleDelayInSeconds);
            this.groupBoxWhenIdle.Controls.Add(this.checkDisableWhenIdle);
            this.groupBoxWhenIdle.Location = new System.Drawing.Point(6, 29);
            this.groupBoxWhenIdle.Name = "groupBoxWhenIdle";
            this.groupBoxWhenIdle.Size = new System.Drawing.Size(295, 66);
            this.groupBoxWhenIdle.TabIndex = 8;
            this.groupBoxWhenIdle.TabStop = false;
            this.groupBoxWhenIdle.Text = "When Idle";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(173, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "second(s)";
            // 
            // textReenableWhenIdleAfterDelayInSeconds
            // 
            this.textReenableWhenIdleAfterDelayInSeconds.Location = new System.Drawing.Point(128, 40);
            this.textReenableWhenIdleAfterDelayInSeconds.Mask = "00000";
            this.textReenableWhenIdleAfterDelayInSeconds.Name = "textReenableWhenIdleAfterDelayInSeconds";
            this.textReenableWhenIdleAfterDelayInSeconds.PromptChar = ' ';
            this.textReenableWhenIdleAfterDelayInSeconds.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textReenableWhenIdleAfterDelayInSeconds.Size = new System.Drawing.Size(39, 20);
            this.textReenableWhenIdleAfterDelayInSeconds.TabIndex = 12;
            this.textReenableWhenIdleAfterDelayInSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textReenableWhenIdleAfterDelayInSeconds.ValidatingType = typeof(int);
            // 
            // checkReenableWhenIdleAfter
            // 
            this.checkReenableWhenIdleAfter.AutoSize = true;
            this.checkReenableWhenIdleAfter.Location = new System.Drawing.Point(28, 42);
            this.checkReenableWhenIdleAfter.Name = "checkReenableWhenIdleAfter";
            this.checkReenableWhenIdleAfter.Size = new System.Drawing.Size(96, 17);
            this.checkReenableWhenIdleAfter.TabIndex = 11;
            this.checkReenableWhenIdleAfter.Text = "Reenable after";
            this.checkReenableWhenIdleAfter.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(173, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "second(s)";
            // 
            // textDisableWhenIdleDelayInSeconds
            // 
            this.textDisableWhenIdleDelayInSeconds.Location = new System.Drawing.Point(128, 17);
            this.textDisableWhenIdleDelayInSeconds.Mask = "00000";
            this.textDisableWhenIdleDelayInSeconds.Name = "textDisableWhenIdleDelayInSeconds";
            this.textDisableWhenIdleDelayInSeconds.PromptChar = ' ';
            this.textDisableWhenIdleDelayInSeconds.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textDisableWhenIdleDelayInSeconds.Size = new System.Drawing.Size(39, 20);
            this.textDisableWhenIdleDelayInSeconds.TabIndex = 9;
            this.textDisableWhenIdleDelayInSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textDisableWhenIdleDelayInSeconds.ValidatingType = typeof(int);
            // 
            // checkDisableWhenIdle
            // 
            this.checkDisableWhenIdle.AutoSize = true;
            this.checkDisableWhenIdle.Location = new System.Drawing.Point(28, 19);
            this.checkDisableWhenIdle.Name = "checkDisableWhenIdle";
            this.checkDisableWhenIdle.Size = new System.Drawing.Size(85, 17);
            this.checkDisableWhenIdle.TabIndex = 8;
            this.checkDisableWhenIdle.Text = "Disable after";
            this.checkDisableWhenIdle.UseVisualStyleBackColor = true;
            // 
            // checkDisableWhenInBackground
            // 
            this.checkDisableWhenInBackground.AutoSize = true;
            this.checkDisableWhenInBackground.Location = new System.Drawing.Point(6, 6);
            this.checkDisableWhenInBackground.Name = "checkDisableWhenInBackground";
            this.checkDisableWhenInBackground.Size = new System.Drawing.Size(161, 17);
            this.checkDisableWhenInBackground.TabIndex = 4;
            this.checkDisableWhenInBackground.Text = "Disable when in background";
            this.checkDisableWhenInBackground.UseVisualStyleBackColor = true;
            this.checkDisableWhenInBackground.Visible = false;
            // 
            // tabEqualizer
            // 
            this.tabEqualizer.Controls.Add(this.textEqEnabledTimeInSeconds);
            this.tabEqualizer.Controls.Add(this.textEqDisabledTimeInSeconds);
            this.tabEqualizer.Controls.Add(this.textEqStartDelayInSeconds);
            this.tabEqualizer.Controls.Add(this.mpLabel2);
            this.tabEqualizer.Controls.Add(this.mpLabelEQTitleDisplay);
            this.tabEqualizer.Controls.Add(this.mpEQTitleDisplay);
            this.tabEqualizer.Controls.Add(this.mpSmoothEQ);
            this.tabEqualizer.Controls.Add(this.mpEqDisplay);
            this.tabEqualizer.Controls.Add(this.mpRestrictEQ);
            this.tabEqualizer.Controls.Add(this.cmbEqRate);
            this.tabEqualizer.Controls.Add(this.mpDelayEQ);
            this.tabEqualizer.Location = new System.Drawing.Point(4, 22);
            this.tabEqualizer.Name = "tabEqualizer";
            this.tabEqualizer.Padding = new System.Windows.Forms.Padding(3);
            this.tabEqualizer.Size = new System.Drawing.Size(307, 253);
            this.tabEqualizer.TabIndex = 1;
            this.tabEqualizer.Text = "Equalizer";
            this.tabEqualizer.UseVisualStyleBackColor = true;
            // 
            // textEqEnabledTimeInSeconds
            // 
            this.textEqEnabledTimeInSeconds.Location = new System.Drawing.Point(166, 131);
            this.textEqEnabledTimeInSeconds.Mask = "00000";
            this.textEqEnabledTimeInSeconds.Name = "textEqEnabledTimeInSeconds";
            this.textEqEnabledTimeInSeconds.PromptChar = ' ';
            this.textEqEnabledTimeInSeconds.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textEqEnabledTimeInSeconds.Size = new System.Drawing.Size(42, 20);
            this.textEqEnabledTimeInSeconds.TabIndex = 157;
            this.textEqEnabledTimeInSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textEqEnabledTimeInSeconds.ValidatingType = typeof(int);
            // 
            // textEqDisabledTimeInSeconds
            // 
            this.textEqDisabledTimeInSeconds.Location = new System.Drawing.Point(38, 131);
            this.textEqDisabledTimeInSeconds.Mask = "00000";
            this.textEqDisabledTimeInSeconds.Name = "textEqDisabledTimeInSeconds";
            this.textEqDisabledTimeInSeconds.PromptChar = ' ';
            this.textEqDisabledTimeInSeconds.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textEqDisabledTimeInSeconds.Size = new System.Drawing.Size(39, 20);
            this.textEqDisabledTimeInSeconds.TabIndex = 156;
            this.textEqDisabledTimeInSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textEqDisabledTimeInSeconds.ValidatingType = typeof(int);
            // 
            // textEqStartDelayInSeconds
            // 
            this.textEqStartDelayInSeconds.Location = new System.Drawing.Point(159, 68);
            this.textEqStartDelayInSeconds.Mask = "00000";
            this.textEqStartDelayInSeconds.Name = "textEqStartDelayInSeconds";
            this.textEqStartDelayInSeconds.PromptChar = ' ';
            this.textEqStartDelayInSeconds.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textEqStartDelayInSeconds.Size = new System.Drawing.Size(42, 20);
            this.textEqStartDelayInSeconds.TabIndex = 155;
            this.textEqStartDelayInSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textEqStartDelayInSeconds.ValidatingType = typeof(int);
            // 
            // mpLabel2
            // 
            this.mpLabel2.Location = new System.Drawing.Point(97, 50);
            this.mpLabel2.Name = "mpLabel2";
            this.mpLabel2.Size = new System.Drawing.Size(116, 17);
            this.mpLabel2.TabIndex = 152;
            this.mpLabel2.Text = "updates per Seconds";
            this.mpLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mpLabelEQTitleDisplay
            // 
            this.mpLabelEQTitleDisplay.Location = new System.Drawing.Point(83, 134);
            this.mpLabelEQTitleDisplay.Name = "mpLabelEQTitleDisplay";
            this.mpLabelEQTitleDisplay.Size = new System.Drawing.Size(197, 17);
            this.mpLabelEQTitleDisplay.TabIndex = 151;
            this.mpLabelEQTitleDisplay.Text = "Seconds every                    Seconds";
            this.mpLabelEQTitleDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mpEQTitleDisplay
            // 
            this.mpEQTitleDisplay.AutoSize = true;
            this.mpEQTitleDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpEQTitleDisplay.Location = new System.Drawing.Point(18, 113);
            this.mpEQTitleDisplay.Name = "mpEQTitleDisplay";
            this.mpEQTitleDisplay.Size = new System.Drawing.Size(118, 17);
            this.mpEQTitleDisplay.TabIndex = 149;
            this.mpEQTitleDisplay.Text = "Show Track Info for";
            this.mpEQTitleDisplay.UseVisualStyleBackColor = true;
            // 
            // mpSmoothEQ
            // 
            this.mpSmoothEQ.AutoSize = true;
            this.mpSmoothEQ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpSmoothEQ.Location = new System.Drawing.Point(18, 92);
            this.mpSmoothEQ.Name = "mpSmoothEQ";
            this.mpSmoothEQ.Size = new System.Drawing.Size(222, 17);
            this.mpSmoothEQ.TabIndex = 147;
            this.mpSmoothEQ.Text = "Use Equalizer Smoothing (Delayed decay)";
            this.mpSmoothEQ.UseVisualStyleBackColor = true;
            // 
            // mpEqDisplay
            // 
            this.mpEqDisplay.AutoSize = true;
            this.mpEqDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpEqDisplay.Location = new System.Drawing.Point(18, 8);
            this.mpEqDisplay.Name = "mpEqDisplay";
            this.mpEqDisplay.Size = new System.Drawing.Size(124, 17);
            this.mpEqDisplay.TabIndex = 144;
            this.mpEqDisplay.Text = "Use Equalizer display";
            this.mpEqDisplay.UseVisualStyleBackColor = true;
            // 
            // mpRestrictEQ
            // 
            this.mpRestrictEQ.AutoSize = true;
            this.mpRestrictEQ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpRestrictEQ.Location = new System.Drawing.Point(18, 29);
            this.mpRestrictEQ.Name = "mpRestrictEQ";
            this.mpRestrictEQ.Size = new System.Drawing.Size(183, 17);
            this.mpRestrictEQ.TabIndex = 145;
            this.mpRestrictEQ.Text = "Limit Equalizer display update rate";
            this.mpRestrictEQ.UseVisualStyleBackColor = true;
            // 
            // cmbEqRate
            // 
            this.cmbEqRate.BorderColor = System.Drawing.Color.Empty;
            this.cmbEqRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEqRate.Items.AddRange(new object[] {
            "MAX",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
            "60"});
            this.cmbEqRate.Location = new System.Drawing.Point(34, 48);
            this.cmbEqRate.Name = "cmbEqRate";
            this.cmbEqRate.Size = new System.Drawing.Size(57, 21);
            this.cmbEqRate.TabIndex = 142;
            // 
            // mpDelayEQ
            // 
            this.mpDelayEQ.AutoSize = true;
            this.mpDelayEQ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpDelayEQ.Location = new System.Drawing.Point(18, 71);
            this.mpDelayEQ.Name = "mpDelayEQ";
            this.mpDelayEQ.Size = new System.Drawing.Size(247, 17);
            this.mpDelayEQ.TabIndex = 146;
            this.mpDelayEQ.Text = "Delay Equalizer Start by                       Seconds";
            this.mpDelayEQ.UseVisualStyleBackColor = true;
            // 
            // tabLcd
            // 
            this.tabLcd.Controls.Add(this.gbxLineOptions);
            this.tabLcd.Location = new System.Drawing.Point(4, 22);
            this.tabLcd.Name = "tabLcd";
            this.tabLcd.Padding = new System.Windows.Forms.Padding(3);
            this.tabLcd.Size = new System.Drawing.Size(307, 253);
            this.tabLcd.TabIndex = 2;
            this.tabLcd.Text = "LCD";
            this.tabLcd.UseVisualStyleBackColor = true;
            // 
            // gbxLineOptions
            // 
            this.gbxLineOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxLineOptions.Controls.Add(this.gbxPlayback);
            this.gbxLineOptions.Controls.Add(this.gbxGeneral);
            this.gbxLineOptions.Location = new System.Drawing.Point(6, 6);
            this.gbxLineOptions.Name = "gbxLineOptions";
            this.gbxLineOptions.Size = new System.Drawing.Size(295, 118);
            this.gbxLineOptions.TabIndex = 9;
            this.gbxLineOptions.TabStop = false;
            this.gbxLineOptions.Text = "Line Options";
            // 
            // gbxPlayback
            // 
            this.gbxPlayback.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxPlayback.Controls.Add(this.rbtnPlaybackSecondLine);
            this.gbxPlayback.Controls.Add(this.rbtnPlaybackFirstLine);
            this.gbxPlayback.Location = new System.Drawing.Point(7, 69);
            this.gbxPlayback.Name = "gbxPlayback";
            this.gbxPlayback.Size = new System.Drawing.Size(282, 43);
            this.gbxPlayback.TabIndex = 1;
            this.gbxPlayback.TabStop = false;
            this.gbxPlayback.Text = "During Playback";
            // 
            // rbtnPlaybackSecondLine
            // 
            this.rbtnPlaybackSecondLine.AutoSize = true;
            this.rbtnPlaybackSecondLine.Location = new System.Drawing.Point(145, 18);
            this.rbtnPlaybackSecondLine.Name = "rbtnPlaybackSecondLine";
            this.rbtnPlaybackSecondLine.Size = new System.Drawing.Size(116, 17);
            this.rbtnPlaybackSecondLine.TabIndex = 3;
            this.rbtnPlaybackSecondLine.TabStop = true;
            this.rbtnPlaybackSecondLine.Text = "Prefer Second Line";
            this.rbtnPlaybackSecondLine.UseVisualStyleBackColor = true;
            // 
            // rbtnPlaybackFirstLine
            // 
            this.rbtnPlaybackFirstLine.AutoSize = true;
            this.rbtnPlaybackFirstLine.Location = new System.Drawing.Point(7, 19);
            this.rbtnPlaybackFirstLine.Name = "rbtnPlaybackFirstLine";
            this.rbtnPlaybackFirstLine.Size = new System.Drawing.Size(98, 17);
            this.rbtnPlaybackFirstLine.TabIndex = 2;
            this.rbtnPlaybackFirstLine.TabStop = true;
            this.rbtnPlaybackFirstLine.Text = "Prefer First Line";
            this.rbtnPlaybackFirstLine.UseVisualStyleBackColor = true;
            // 
            // gbxGeneral
            // 
            this.gbxGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxGeneral.Controls.Add(this.rbtnGeneralSecondLine);
            this.gbxGeneral.Controls.Add(this.rbtnGeneralFirstLine);
            this.gbxGeneral.Location = new System.Drawing.Point(7, 20);
            this.gbxGeneral.Name = "gbxGeneral";
            this.gbxGeneral.Size = new System.Drawing.Size(282, 43);
            this.gbxGeneral.TabIndex = 0;
            this.gbxGeneral.TabStop = false;
            this.gbxGeneral.Text = "General";
            // 
            // rbtnGeneralSecondLine
            // 
            this.rbtnGeneralSecondLine.AutoSize = true;
            this.rbtnGeneralSecondLine.Location = new System.Drawing.Point(145, 19);
            this.rbtnGeneralSecondLine.Name = "rbtnGeneralSecondLine";
            this.rbtnGeneralSecondLine.Size = new System.Drawing.Size(116, 17);
            this.rbtnGeneralSecondLine.TabIndex = 1;
            this.rbtnGeneralSecondLine.TabStop = true;
            this.rbtnGeneralSecondLine.Text = "Prefer Second Line";
            this.rbtnGeneralSecondLine.UseVisualStyleBackColor = true;
            // 
            // rbtnGeneralFirstLine
            // 
            this.rbtnGeneralFirstLine.AutoSize = true;
            this.rbtnGeneralFirstLine.Location = new System.Drawing.Point(7, 20);
            this.rbtnGeneralFirstLine.Name = "rbtnGeneralFirstLine";
            this.rbtnGeneralFirstLine.Size = new System.Drawing.Size(98, 17);
            this.rbtnGeneralFirstLine.TabIndex = 0;
            this.rbtnGeneralFirstLine.TabStop = true;
            this.rbtnGeneralFirstLine.Text = "Prefer First Line";
            this.rbtnGeneralFirstLine.UseVisualStyleBackColor = true;
            // 
            // SoundGraphImonSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 332);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SoundGraphImonSettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MiniDisplay - Setup - Advanced Settings";
            this.tabControl.ResumeLayout(false);
            this.tabImon.ResumeLayout(false);
            this.tabImon.PerformLayout();
            this.groupBoxWhenPlaying.ResumeLayout(false);
            this.groupBoxWhenPlaying.PerformLayout();
            this.groupBoxWhenIdle.ResumeLayout(false);
            this.groupBoxWhenIdle.PerformLayout();
            this.tabEqualizer.ResumeLayout(false);
            this.tabEqualizer.PerformLayout();
            this.tabLcd.ResumeLayout(false);
            this.gbxLineOptions.ResumeLayout(false);
            this.gbxPlayback.ResumeLayout(false);
            this.gbxPlayback.PerformLayout();
            this.gbxGeneral.ResumeLayout(false);
            this.gbxGeneral.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private UserInterface.Controls.MPButton btnOK;
        public System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabImon;
        private System.Windows.Forms.TabPage tabEqualizer;
        private UserInterface.Controls.MPLabel mpLabel2;
        private UserInterface.Controls.MPLabel mpLabelEQTitleDisplay;
        private UserInterface.Controls.MPCheckBox mpEQTitleDisplay;
        private UserInterface.Controls.MPCheckBox mpSmoothEQ;
        private UserInterface.Controls.MPCheckBox mpEqDisplay;
        private UserInterface.Controls.MPCheckBox mpRestrictEQ;
        private UserInterface.Controls.MPComboBox cmbEqRate;
        private UserInterface.Controls.MPCheckBox mpDelayEQ;
        private System.Windows.Forms.CheckBox checkDisableWhenInBackground;
        public System.Windows.Forms.TabPage tabLcd;
        private System.Windows.Forms.GroupBox gbxLineOptions;
        private System.Windows.Forms.GroupBox gbxPlayback;
        private System.Windows.Forms.RadioButton rbtnPlaybackSecondLine;
        private System.Windows.Forms.RadioButton rbtnPlaybackFirstLine;
        private System.Windows.Forms.GroupBox gbxGeneral;
        private System.Windows.Forms.RadioButton rbtnGeneralSecondLine;
        private System.Windows.Forms.RadioButton rbtnGeneralFirstLine;
        private System.Windows.Forms.MaskedTextBox textEqEnabledTimeInSeconds;
        private System.Windows.Forms.MaskedTextBox textEqDisabledTimeInSeconds;
        private System.Windows.Forms.MaskedTextBox textEqStartDelayInSeconds;
        private System.Windows.Forms.GroupBox groupBoxWhenPlaying;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MaskedTextBox textReenableWhenPlayingAfterDelayInSeconds;
        private System.Windows.Forms.CheckBox checkReenableWhenPlayingAfter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.MaskedTextBox textDisableWhenPlayingDelayInSeconds;
        private System.Windows.Forms.CheckBox checkDisableWhenPlaying;
        private System.Windows.Forms.GroupBox groupBoxWhenIdle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox textReenableWhenIdleAfterDelayInSeconds;
        private System.Windows.Forms.CheckBox checkReenableWhenIdleAfter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MaskedTextBox textDisableWhenIdleDelayInSeconds;
        private System.Windows.Forms.CheckBox checkDisableWhenIdle;
    }
}