namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    partial class SoundGraphImonVfdAdvancedSetupForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabImon = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.textReenableAfterDelayInSeconds = new System.Windows.Forms.MaskedTextBox();
            this.checkReenableAfter = new System.Windows.Forms.CheckBox();
            this.checkDisableWhenInBackground = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textDisableWhenIdleDelayInSeconds = new System.Windows.Forms.MaskedTextBox();
            this.checkDisableWhenIdle = new System.Windows.Forms.CheckBox();
            this.tabEqualizer = new System.Windows.Forms.TabPage();
            this.cmbDelayEqTime = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.groupEQstyle = new System.Windows.Forms.GroupBox();
            this.mpUseVUmeter2 = new System.Windows.Forms.RadioButton();
            this.cbVUindicators = new System.Windows.Forms.CheckBox();
            this.mpUseVUmeter = new System.Windows.Forms.RadioButton();
            this.mpUseStereoEQ = new System.Windows.Forms.RadioButton();
            this.mpNormalEQ = new System.Windows.Forms.RadioButton();
            this.cmbEqMode = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.mpLabelEQmode = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.cmbEQTitleDisplayTime = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.mpLabelEQTitleDisplay = new MediaPortal.UserInterface.Controls.MPLabel();
            this.cmbEQTitleShowTime = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.mpEQTitleDisplay = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpSmoothEQ = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpEqDisplay = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpRestrictEQ = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.cmbEqRate = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.mpDelayEQ = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.tabControl1.SuspendLayout();
            this.tabImon.SuspendLayout();
            this.tabEqualizer.SuspendLayout();
            this.groupEQstyle.SuspendLayout();
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
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabImon);
            this.tabControl1.Controls.Add(this.tabEqualizer);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(315, 279);
            this.tabControl1.TabIndex = 125;
            // 
            // tabImon
            // 
            this.tabImon.Controls.Add(this.label2);
            this.tabImon.Controls.Add(this.textReenableAfterDelayInSeconds);
            this.tabImon.Controls.Add(this.checkReenableAfter);
            this.tabImon.Controls.Add(this.checkDisableWhenInBackground);
            this.tabImon.Controls.Add(this.label1);
            this.tabImon.Controls.Add(this.textDisableWhenIdleDelayInSeconds);
            this.tabImon.Controls.Add(this.checkDisableWhenIdle);
            this.tabImon.Location = new System.Drawing.Point(4, 22);
            this.tabImon.Name = "tabImon";
            this.tabImon.Padding = new System.Windows.Forms.Padding(3);
            this.tabImon.Size = new System.Drawing.Size(307, 253);
            this.tabImon.TabIndex = 0;
            this.tabImon.Text = "iMON";
            this.tabImon.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(190, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "second(s)";
            // 
            // textReenableAfterDelayInSeconds
            // 
            this.textReenableAfterDelayInSeconds.Location = new System.Drawing.Point(145, 50);
            this.textReenableAfterDelayInSeconds.Mask = "00000";
            this.textReenableAfterDelayInSeconds.Name = "textReenableAfterDelayInSeconds";
            this.textReenableAfterDelayInSeconds.PromptChar = ' ';
            this.textReenableAfterDelayInSeconds.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textReenableAfterDelayInSeconds.Size = new System.Drawing.Size(39, 20);
            this.textReenableAfterDelayInSeconds.TabIndex = 6;
            this.textReenableAfterDelayInSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textReenableAfterDelayInSeconds.ValidatingType = typeof(int);
            // 
            // checkReenableAfter
            // 
            this.checkReenableAfter.AutoSize = true;
            this.checkReenableAfter.Location = new System.Drawing.Point(6, 52);
            this.checkReenableAfter.Name = "checkReenableAfter";
            this.checkReenableAfter.Size = new System.Drawing.Size(96, 17);
            this.checkReenableAfter.TabIndex = 5;
            this.checkReenableAfter.Text = "Reenable after";
            this.checkReenableAfter.UseVisualStyleBackColor = true;
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
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(190, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "second(s)";
            // 
            // textDisableWhenIdleDelayInSeconds
            // 
            this.textDisableWhenIdleDelayInSeconds.Location = new System.Drawing.Point(145, 27);
            this.textDisableWhenIdleDelayInSeconds.Mask = "00000";
            this.textDisableWhenIdleDelayInSeconds.Name = "textDisableWhenIdleDelayInSeconds";
            this.textDisableWhenIdleDelayInSeconds.PromptChar = ' ';
            this.textDisableWhenIdleDelayInSeconds.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textDisableWhenIdleDelayInSeconds.Size = new System.Drawing.Size(39, 20);
            this.textDisableWhenIdleDelayInSeconds.TabIndex = 2;
            this.textDisableWhenIdleDelayInSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textDisableWhenIdleDelayInSeconds.ValidatingType = typeof(int);
            // 
            // checkDisableWhenIdle
            // 
            this.checkDisableWhenIdle.AutoSize = true;
            this.checkDisableWhenIdle.Location = new System.Drawing.Point(6, 29);
            this.checkDisableWhenIdle.Name = "checkDisableWhenIdle";
            this.checkDisableWhenIdle.Size = new System.Drawing.Size(133, 17);
            this.checkDisableWhenIdle.TabIndex = 0;
            this.checkDisableWhenIdle.Text = "Disable when idle after";
            this.checkDisableWhenIdle.UseVisualStyleBackColor = true;
            // 
            // tabEqualizer
            // 
            this.tabEqualizer.Controls.Add(this.cmbDelayEqTime);
            this.tabEqualizer.Controls.Add(this.groupEQstyle);
            this.tabEqualizer.Controls.Add(this.cmbEqMode);
            this.tabEqualizer.Controls.Add(this.mpLabelEQmode);
            this.tabEqualizer.Controls.Add(this.mpLabel2);
            this.tabEqualizer.Controls.Add(this.cmbEQTitleDisplayTime);
            this.tabEqualizer.Controls.Add(this.mpLabelEQTitleDisplay);
            this.tabEqualizer.Controls.Add(this.cmbEQTitleShowTime);
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
            // cmbDelayEqTime
            // 
            this.cmbDelayEqTime.BorderColor = System.Drawing.Color.Empty;
            this.cmbDelayEqTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDelayEqTime.Items.AddRange(new object[] {
            "0",
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
            "30"});
            this.cmbDelayEqTime.Location = new System.Drawing.Point(173, 153);
            this.cmbDelayEqTime.Name = "cmbDelayEqTime";
            this.cmbDelayEqTime.Size = new System.Drawing.Size(53, 21);
            this.cmbDelayEqTime.TabIndex = 143;
            // 
            // groupEQstyle
            // 
            this.groupEQstyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupEQstyle.Controls.Add(this.mpUseVUmeter2);
            this.groupEQstyle.Controls.Add(this.cbVUindicators);
            this.groupEQstyle.Controls.Add(this.mpUseVUmeter);
            this.groupEQstyle.Controls.Add(this.mpUseStereoEQ);
            this.groupEQstyle.Controls.Add(this.mpNormalEQ);
            this.groupEQstyle.Location = new System.Drawing.Point(13, 49);
            this.groupEQstyle.Name = "groupEQstyle";
            this.groupEQstyle.Size = new System.Drawing.Size(288, 60);
            this.groupEQstyle.TabIndex = 154;
            this.groupEQstyle.TabStop = false;
            this.groupEQstyle.Text = " Equalizer Style ";
            // 
            // mpUseVUmeter2
            // 
            this.mpUseVUmeter2.AutoSize = true;
            this.mpUseVUmeter2.Location = new System.Drawing.Point(204, 17);
            this.mpUseVUmeter2.Name = "mpUseVUmeter2";
            this.mpUseVUmeter2.Size = new System.Drawing.Size(79, 17);
            this.mpUseVUmeter2.TabIndex = 121;
            this.mpUseVUmeter2.Text = "VU Meter 2";
            this.mpUseVUmeter2.UseVisualStyleBackColor = true;
            // 
            // cbVUindicators
            // 
            this.cbVUindicators.AutoSize = true;
            this.cbVUindicators.Location = new System.Drawing.Point(8, 40);
            this.cbVUindicators.Name = "cbVUindicators";
            this.cbVUindicators.Size = new System.Drawing.Size(213, 17);
            this.cbVUindicators.TabIndex = 120;
            this.cbVUindicators.Text = "Show Channel indicators for VU Display";
            this.cbVUindicators.UseVisualStyleBackColor = true;
            // 
            // mpUseVUmeter
            // 
            this.mpUseVUmeter.AutoSize = true;
            this.mpUseVUmeter.Location = new System.Drawing.Point(131, 17);
            this.mpUseVUmeter.Name = "mpUseVUmeter";
            this.mpUseVUmeter.Size = new System.Drawing.Size(70, 17);
            this.mpUseVUmeter.TabIndex = 2;
            this.mpUseVUmeter.Text = "VU Meter";
            this.mpUseVUmeter.UseVisualStyleBackColor = true;
            // 
            // mpUseStereoEQ
            // 
            this.mpUseStereoEQ.AutoSize = true;
            this.mpUseStereoEQ.Location = new System.Drawing.Point(69, 17);
            this.mpUseStereoEQ.Name = "mpUseStereoEQ";
            this.mpUseStereoEQ.Size = new System.Drawing.Size(56, 17);
            this.mpUseStereoEQ.TabIndex = 1;
            this.mpUseStereoEQ.Text = "Stereo";
            this.mpUseStereoEQ.UseVisualStyleBackColor = true;
            // 
            // mpNormalEQ
            // 
            this.mpNormalEQ.AutoSize = true;
            this.mpNormalEQ.Checked = true;
            this.mpNormalEQ.Location = new System.Drawing.Point(13, 17);
            this.mpNormalEQ.Name = "mpNormalEQ";
            this.mpNormalEQ.Size = new System.Drawing.Size(58, 17);
            this.mpNormalEQ.TabIndex = 0;
            this.mpNormalEQ.TabStop = true;
            this.mpNormalEQ.Text = "Normal";
            this.mpNormalEQ.UseVisualStyleBackColor = true;
            // 
            // cmbEqMode
            // 
            this.cmbEqMode.BorderColor = System.Drawing.Color.Empty;
            this.cmbEqMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEqMode.Items.AddRange(new object[] {
            "Up from bottom",
            "Down from top",
            "Expand from middle"});
            this.cmbEqMode.Location = new System.Drawing.Point(139, 27);
            this.cmbEqMode.Name = "cmbEqMode";
            this.cmbEqMode.Size = new System.Drawing.Size(104, 21);
            this.cmbEqMode.TabIndex = 141;
            // 
            // mpLabelEQmode
            // 
            this.mpLabelEQmode.Location = new System.Drawing.Point(47, 29);
            this.mpLabelEQmode.Name = "mpLabelEQmode";
            this.mpLabelEQmode.Size = new System.Drawing.Size(95, 17);
            this.mpLabelEQmode.TabIndex = 153;
            this.mpLabelEQmode.Text = "EQ Display Mode:";
            this.mpLabelEQmode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mpLabel2
            // 
            this.mpLabel2.Location = new System.Drawing.Point(113, 134);
            this.mpLabel2.Name = "mpLabel2";
            this.mpLabel2.Size = new System.Drawing.Size(116, 17);
            this.mpLabel2.TabIndex = 152;
            this.mpLabel2.Text = "updates per Seconds";
            this.mpLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbEQTitleDisplayTime
            // 
            this.cmbEQTitleDisplayTime.BorderColor = System.Drawing.Color.Empty;
            this.cmbEQTitleDisplayTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEQTitleDisplayTime.Items.AddRange(new object[] {
            "0",
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
            "30"});
            this.cmbEQTitleDisplayTime.Location = new System.Drawing.Point(177, 216);
            this.cmbEQTitleDisplayTime.Name = "cmbEQTitleDisplayTime";
            this.cmbEQTitleDisplayTime.Size = new System.Drawing.Size(49, 21);
            this.cmbEQTitleDisplayTime.TabIndex = 148;
            // 
            // mpLabelEQTitleDisplay
            // 
            this.mpLabelEQTitleDisplay.Location = new System.Drawing.Point(99, 218);
            this.mpLabelEQTitleDisplay.Name = "mpLabelEQTitleDisplay";
            this.mpLabelEQTitleDisplay.Size = new System.Drawing.Size(197, 17);
            this.mpLabelEQTitleDisplay.TabIndex = 151;
            this.mpLabelEQTitleDisplay.Text = "Seconds every                    Seconds";
            this.mpLabelEQTitleDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbEQTitleShowTime
            // 
            this.cmbEQTitleShowTime.BorderColor = System.Drawing.Color.Empty;
            this.cmbEQTitleShowTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEQTitleShowTime.Items.AddRange(new object[] {
            "0",
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
            "30"});
            this.cmbEQTitleShowTime.Location = new System.Drawing.Point(50, 216);
            this.cmbEQTitleShowTime.Name = "cmbEQTitleShowTime";
            this.cmbEQTitleShowTime.Size = new System.Drawing.Size(49, 21);
            this.cmbEQTitleShowTime.TabIndex = 150;
            // 
            // mpEQTitleDisplay
            // 
            this.mpEQTitleDisplay.AutoSize = true;
            this.mpEQTitleDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpEQTitleDisplay.Location = new System.Drawing.Point(34, 197);
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
            this.mpSmoothEQ.Location = new System.Drawing.Point(34, 176);
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
            this.mpRestrictEQ.Location = new System.Drawing.Point(34, 113);
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
            this.cmbEqRate.Location = new System.Drawing.Point(50, 132);
            this.cmbEqRate.Name = "cmbEqRate";
            this.cmbEqRate.Size = new System.Drawing.Size(57, 21);
            this.cmbEqRate.TabIndex = 142;
            // 
            // mpDelayEQ
            // 
            this.mpDelayEQ.AutoSize = true;
            this.mpDelayEQ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpDelayEQ.Location = new System.Drawing.Point(34, 155);
            this.mpDelayEQ.Name = "mpDelayEQ";
            this.mpDelayEQ.Size = new System.Drawing.Size(247, 17);
            this.mpDelayEQ.TabIndex = 146;
            this.mpDelayEQ.Text = "Delay Equalizer Start by                       Seconds";
            this.mpDelayEQ.UseVisualStyleBackColor = true;
            // 
            // SoundGraphImonVfdAdvancedSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 332);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SoundGraphImonVfdAdvancedSetupForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MiniDisplay - Setup - Advanced Settings";
            this.tabControl1.ResumeLayout(false);
            this.tabImon.ResumeLayout(false);
            this.tabImon.PerformLayout();
            this.tabEqualizer.ResumeLayout(false);
            this.tabEqualizer.PerformLayout();
            this.groupEQstyle.ResumeLayout(false);
            this.groupEQstyle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private UserInterface.Controls.MPButton btnOK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabImon;
        private System.Windows.Forms.CheckBox checkDisableWhenIdle;
        private System.Windows.Forms.TabPage tabEqualizer;
        private UserInterface.Controls.MPComboBox cmbDelayEqTime;
        private System.Windows.Forms.GroupBox groupEQstyle;
        private System.Windows.Forms.RadioButton mpUseVUmeter2;
        private System.Windows.Forms.CheckBox cbVUindicators;
        private System.Windows.Forms.RadioButton mpUseVUmeter;
        private System.Windows.Forms.RadioButton mpUseStereoEQ;
        private System.Windows.Forms.RadioButton mpNormalEQ;
        private UserInterface.Controls.MPComboBox cmbEqMode;
        private UserInterface.Controls.MPLabel mpLabelEQmode;
        private UserInterface.Controls.MPLabel mpLabel2;
        private UserInterface.Controls.MPComboBox cmbEQTitleDisplayTime;
        private UserInterface.Controls.MPLabel mpLabelEQTitleDisplay;
        private UserInterface.Controls.MPComboBox cmbEQTitleShowTime;
        private UserInterface.Controls.MPCheckBox mpEQTitleDisplay;
        private UserInterface.Controls.MPCheckBox mpSmoothEQ;
        private UserInterface.Controls.MPCheckBox mpEqDisplay;
        private UserInterface.Controls.MPCheckBox mpRestrictEQ;
        private UserInterface.Controls.MPComboBox cmbEqRate;
        private UserInterface.Controls.MPCheckBox mpDelayEQ;
        private System.Windows.Forms.MaskedTextBox textDisableWhenIdleDelayInSeconds;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkDisableWhenInBackground;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox textReenableAfterDelayInSeconds;
        private System.Windows.Forms.CheckBox checkReenableAfter;
    }
}