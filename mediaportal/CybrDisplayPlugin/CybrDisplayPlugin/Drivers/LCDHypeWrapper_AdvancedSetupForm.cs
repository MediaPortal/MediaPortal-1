namespace CybrDisplayPlugin.Drivers
{
    using CybrDisplayPlugin;
    using MediaPortal.GUI.Library;
    using MediaPortal.UserInterface.Controls;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class LCDHypeWrapper_AdvancedSetupForm : Form
    {
        private MPButton btnOK;
        private MPButton btnReset;
        private RadioButton cbNormalEQ;
        private RadioButton cbStereoEQ;
        private RadioButton cbUseVUmeter;
        private RadioButton cbUseVUmeter2;
        private CheckBox cbVUindicators;
        private MPComboBox cmbBlankIdleTime;
        private MPComboBox cmbDelayEqTime;
        private MPComboBox cmbEqRate;
        private MPComboBox cmbEQTitleDisplayTime;
        private MPComboBox cmbEQTitleShowTime;
        private readonly IContainer components;
        private object ControlStateLock = new object();
        private MPGroupBox groupBox1;
        private GroupBox groupBox5;
        private GroupBox groupBox7;
        private GroupBox groupEQstyle;
        private MPLabel lblEQTitleDisplay;
        private MPLabel lblRestrictEQ;
        private CheckBox mpBlankDisplayWhenIdle;
        private CheckBox mpBlankDisplayWithVideo;
        private CheckBox mpDelayEQ;
        private CheckBox mpEnableDisplayAction;
        private MPComboBox mpEnableDisplayActionTime;
        private CheckBox mpEqDisplay;
        private CheckBox mpEQTitleDisplay;
        private CheckBox mpRestrictEQ;
        private CheckBox mpSmoothEQ;

        public LCDHypeWrapper_AdvancedSetupForm()
        {
            Log.Debug("LCDHypeWrapper_AdvancedSetupForm(): Constructor started", new object[0]);
            this.InitializeComponent();
            this.mpEqDisplay.DataBindings.Add("Checked", LCDHypeWrapper.AdvancedSettings.Instance, "EqDisplay");
            this.mpRestrictEQ.DataBindings.Add("Checked", LCDHypeWrapper.AdvancedSettings.Instance, "RestrictEQ");
            this.cmbEqRate.DataBindings.Add("SelectedIndex", LCDHypeWrapper.AdvancedSettings.Instance, "EqRate");
            this.mpDelayEQ.DataBindings.Add("Checked", LCDHypeWrapper.AdvancedSettings.Instance, "DelayEQ");
            this.cmbDelayEqTime.DataBindings.Add("SelectedIndex", LCDHypeWrapper.AdvancedSettings.Instance, "DelayEqTime");
            this.mpSmoothEQ.DataBindings.Add("Checked", LCDHypeWrapper.AdvancedSettings.Instance, "SmoothEQ");
            this.mpBlankDisplayWithVideo.DataBindings.Add("Checked", LCDHypeWrapper.AdvancedSettings.Instance, "BlankDisplayWithVideo");
            this.mpEnableDisplayAction.DataBindings.Add("Checked", LCDHypeWrapper.AdvancedSettings.Instance, "EnableDisplayAction");
            this.mpEnableDisplayActionTime.DataBindings.Add("SelectedIndex", LCDHypeWrapper.AdvancedSettings.Instance, "EnableDisplayActionTime");
            this.mpEQTitleDisplay.DataBindings.Add("Checked", LCDHypeWrapper.AdvancedSettings.Instance, "EQTitleDisplay");
            this.cmbEQTitleDisplayTime.DataBindings.Add("SelectedIndex", LCDHypeWrapper.AdvancedSettings.Instance, "EQTitleDisplayTime");
            this.cmbEQTitleShowTime.DataBindings.Add("SelectedIndex", LCDHypeWrapper.AdvancedSettings.Instance, "EQTitleShowTime");
            this.mpBlankDisplayWhenIdle.DataBindings.Add("Checked", LCDHypeWrapper.AdvancedSettings.Instance, "BlankDisplayWhenIdle");
            this.cmbBlankIdleTime.DataBindings.Add("SelectedIndex", LCDHypeWrapper.AdvancedSettings.Instance, "BlankIdleTime");
            if (LCDHypeWrapper.AdvancedSettings.Instance.NormalEQ)
            {
                this.cbNormalEQ.Checked = true;
            }
            else if (LCDHypeWrapper.AdvancedSettings.Instance.StereoEQ)
            {
                this.cbStereoEQ.Checked = true;
            }
            else if (LCDHypeWrapper.AdvancedSettings.Instance.VUmeter)
            {
                this.cbUseVUmeter.Checked = true;
            }
            else
            {
                this.cbUseVUmeter2.Checked = true;
            }
            this.cbVUindicators.DataBindings.Add("Checked", LCDHypeWrapper.AdvancedSettings.Instance, "VUindicators");
            this.Refresh();
            this.SetControlState();
            Log.Debug("LCDHypeWrapper_AdvancedSetupForm(): Constructor completed", new object[0]);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Log.Debug("LCDHypeWrapper_AdvancedSetupForm.btnOK_Click(): started", new object[0]);
            if (this.cbNormalEQ.Checked)
            {
                LCDHypeWrapper.AdvancedSettings.Instance.NormalEQ = true;
                LCDHypeWrapper.AdvancedSettings.Instance.StereoEQ = false;
                LCDHypeWrapper.AdvancedSettings.Instance.VUmeter = false;
                LCDHypeWrapper.AdvancedSettings.Instance.VUmeter2 = false;
            }
            else if (this.cbStereoEQ.Checked)
            {
                LCDHypeWrapper.AdvancedSettings.Instance.NormalEQ = false;
                LCDHypeWrapper.AdvancedSettings.Instance.StereoEQ = true;
                LCDHypeWrapper.AdvancedSettings.Instance.VUmeter = false;
                LCDHypeWrapper.AdvancedSettings.Instance.VUmeter2 = false;
            }
            else if (this.cbUseVUmeter.Checked)
            {
                LCDHypeWrapper.AdvancedSettings.Instance.NormalEQ = false;
                LCDHypeWrapper.AdvancedSettings.Instance.StereoEQ = false;
                LCDHypeWrapper.AdvancedSettings.Instance.VUmeter = true;
                LCDHypeWrapper.AdvancedSettings.Instance.VUmeter2 = false;
            }
            else
            {
                LCDHypeWrapper.AdvancedSettings.Instance.NormalEQ = false;
                LCDHypeWrapper.AdvancedSettings.Instance.StereoEQ = false;
                LCDHypeWrapper.AdvancedSettings.Instance.VUmeter = false;
                LCDHypeWrapper.AdvancedSettings.Instance.VUmeter2 = true;
            }
            LCDHypeWrapper.AdvancedSettings.Save();
            base.Hide();
            base.Close();
            Log.Debug("LCDHypeWrapper_AdvancedSetupForm.btnOK_Click(): Completed", new object[0]);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Log.Debug("LCDHypeWrapper_AdvancedSetupForm.btnReset_Click(): started", new object[0]);
            LCDHypeWrapper.AdvancedSettings.SetDefaults();
            if (LCDHypeWrapper.AdvancedSettings.Instance.NormalEQ)
            {
                this.cbNormalEQ.Checked = true;
            }
            else if (LCDHypeWrapper.AdvancedSettings.Instance.StereoEQ)
            {
                this.cbStereoEQ.Checked = true;
            }
            else if (LCDHypeWrapper.AdvancedSettings.Instance.VUmeter)
            {
                this.cbUseVUmeter.Checked = true;
            }
            else
            {
                this.cbUseVUmeter2.Checked = true;
            }
            this.mpEqDisplay.Checked = LCDHypeWrapper.AdvancedSettings.Instance.EqDisplay;
            this.mpRestrictEQ.Checked = LCDHypeWrapper.AdvancedSettings.Instance.RestrictEQ;
            this.cmbEqRate.SelectedIndex = LCDHypeWrapper.AdvancedSettings.Instance.EqRate;
            this.mpDelayEQ.Checked = LCDHypeWrapper.AdvancedSettings.Instance.DelayEQ;
            this.cmbDelayEqTime.SelectedIndex = LCDHypeWrapper.AdvancedSettings.Instance.DelayEqTime;
            this.mpSmoothEQ.Checked = LCDHypeWrapper.AdvancedSettings.Instance.SmoothEQ;
            this.mpBlankDisplayWithVideo.Checked = LCDHypeWrapper.AdvancedSettings.Instance.BlankDisplayWithVideo;
            this.mpEnableDisplayAction.Checked = LCDHypeWrapper.AdvancedSettings.Instance.EnableDisplayAction;
            this.mpEnableDisplayActionTime.SelectedIndex = LCDHypeWrapper.AdvancedSettings.Instance.EnableDisplayActionTime;
            this.mpEQTitleDisplay.Checked = LCDHypeWrapper.AdvancedSettings.Instance.EQTitleDisplay;
            this.cmbEQTitleDisplayTime.SelectedIndex = LCDHypeWrapper.AdvancedSettings.Instance.EQTitleDisplayTime;
            this.cmbEQTitleShowTime.SelectedIndex = LCDHypeWrapper.AdvancedSettings.Instance.EQTitleShowTime;
            this.mpBlankDisplayWhenIdle.Checked = LCDHypeWrapper.AdvancedSettings.Instance.BlankDisplayWhenIdle;
            this.cmbBlankIdleTime.SelectedIndex = LCDHypeWrapper.AdvancedSettings.Instance.BlankIdleTime;
            this.Refresh();
            Log.Debug("LCDHypeWrapper_AdvancedSetupForm.btnReset_Click(): Completed", new object[0]);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                new MessageEditForm("ExternalDisplay.xml").ShowDialog(this);
            }
            catch (Exception exception)
            {
                Log.Info("LCDHypeWrapper_AdvancedSetupForm.btnRemoteSetup_Click() CAUGHT EXCEPTION: {0}", new object[] { exception });
            }
        }

        private void cbDisableRepeat_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void cbNoRemote_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void cbNormalEQ_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void cbStereoEQ_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void cbUseVUmeter_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void cmbEqRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
          this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
          this.groupBox7 = new System.Windows.Forms.GroupBox();
          this.groupEQstyle = new System.Windows.Forms.GroupBox();
          this.cbUseVUmeter2 = new System.Windows.Forms.RadioButton();
          this.cbVUindicators = new System.Windows.Forms.CheckBox();
          this.cbUseVUmeter = new System.Windows.Forms.RadioButton();
          this.cbStereoEQ = new System.Windows.Forms.RadioButton();
          this.cbNormalEQ = new System.Windows.Forms.RadioButton();
          this.cmbDelayEqTime = new MediaPortal.UserInterface.Controls.MPComboBox();
          this.lblRestrictEQ = new MediaPortal.UserInterface.Controls.MPLabel();
          this.cmbEQTitleDisplayTime = new MediaPortal.UserInterface.Controls.MPComboBox();
          this.cmbEQTitleShowTime = new MediaPortal.UserInterface.Controls.MPComboBox();
          this.mpEQTitleDisplay = new System.Windows.Forms.CheckBox();
          this.mpSmoothEQ = new System.Windows.Forms.CheckBox();
          this.mpEqDisplay = new System.Windows.Forms.CheckBox();
          this.mpRestrictEQ = new System.Windows.Forms.CheckBox();
          this.cmbEqRate = new MediaPortal.UserInterface.Controls.MPComboBox();
          this.mpDelayEQ = new System.Windows.Forms.CheckBox();
          this.lblEQTitleDisplay = new MediaPortal.UserInterface.Controls.MPLabel();
          this.groupBox5 = new System.Windows.Forms.GroupBox();
          this.mpEnableDisplayActionTime = new MediaPortal.UserInterface.Controls.MPComboBox();
          this.cmbBlankIdleTime = new MediaPortal.UserInterface.Controls.MPComboBox();
          this.mpEnableDisplayAction = new System.Windows.Forms.CheckBox();
          this.mpBlankDisplayWithVideo = new System.Windows.Forms.CheckBox();
          this.mpBlankDisplayWhenIdle = new System.Windows.Forms.CheckBox();
          this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
          this.btnReset = new MediaPortal.UserInterface.Controls.MPButton();
          this.groupBox1.SuspendLayout();
          this.groupBox7.SuspendLayout();
          this.groupEQstyle.SuspendLayout();
          this.groupBox5.SuspendLayout();
          this.SuspendLayout();
          // 
          // groupBox1
          // 
          this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.groupBox1.Controls.Add(this.groupBox7);
          this.groupBox1.Controls.Add(this.groupBox5);
          this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
          this.groupBox1.Location = new System.Drawing.Point(9, 6);
          this.groupBox1.Name = "groupBox1";
          this.groupBox1.Size = new System.Drawing.Size(357, 357);
          this.groupBox1.TabIndex = 4;
          this.groupBox1.TabStop = false;
          this.groupBox1.Text = " LCDHype Display Configuration ";
          // 
          // groupBox7
          // 
          this.groupBox7.Controls.Add(this.groupEQstyle);
          this.groupBox7.Controls.Add(this.cmbDelayEqTime);
          this.groupBox7.Controls.Add(this.lblRestrictEQ);
          this.groupBox7.Controls.Add(this.cmbEQTitleDisplayTime);
          this.groupBox7.Controls.Add(this.cmbEQTitleShowTime);
          this.groupBox7.Controls.Add(this.mpEQTitleDisplay);
          this.groupBox7.Controls.Add(this.mpSmoothEQ);
          this.groupBox7.Controls.Add(this.mpEqDisplay);
          this.groupBox7.Controls.Add(this.mpRestrictEQ);
          this.groupBox7.Controls.Add(this.cmbEqRate);
          this.groupBox7.Controls.Add(this.mpDelayEQ);
          this.groupBox7.Controls.Add(this.lblEQTitleDisplay);
          this.groupBox7.Location = new System.Drawing.Point(10, 118);
          this.groupBox7.Name = "groupBox7";
          this.groupBox7.Size = new System.Drawing.Size(338, 233);
          this.groupBox7.TabIndex = 103;
          this.groupBox7.TabStop = false;
          this.groupBox7.Text = " Equalizer Options";
          // 
          // groupEQstyle
          // 
          this.groupEQstyle.Controls.Add(this.cbUseVUmeter2);
          this.groupEQstyle.Controls.Add(this.cbVUindicators);
          this.groupEQstyle.Controls.Add(this.cbUseVUmeter);
          this.groupEQstyle.Controls.Add(this.cbStereoEQ);
          this.groupEQstyle.Controls.Add(this.cbNormalEQ);
          this.groupEQstyle.Location = new System.Drawing.Point(32, 38);
          this.groupEQstyle.Name = "groupEQstyle";
          this.groupEQstyle.Size = new System.Drawing.Size(300, 60);
          this.groupEQstyle.TabIndex = 118;
          this.groupEQstyle.TabStop = false;
          this.groupEQstyle.Text = " Equalizer Style ";
          // 
          // cbUseVUmeter2
          // 
          this.cbUseVUmeter2.AutoSize = true;
          this.cbUseVUmeter2.Location = new System.Drawing.Point(211, 17);
          this.cbUseVUmeter2.Name = "cbUseVUmeter2";
          this.cbUseVUmeter2.Size = new System.Drawing.Size(79, 17);
          this.cbUseVUmeter2.TabIndex = 122;
          this.cbUseVUmeter2.Text = "VU Meter 2";
          this.cbUseVUmeter2.UseVisualStyleBackColor = true;
          // 
          // cbVUindicators
          // 
          this.cbVUindicators.AutoSize = true;
          this.cbVUindicators.Location = new System.Drawing.Point(9, 39);
          this.cbVUindicators.Name = "cbVUindicators";
          this.cbVUindicators.Size = new System.Drawing.Size(213, 17);
          this.cbVUindicators.TabIndex = 121;
          this.cbVUindicators.Text = "Show Channel indicators for VU Display";
          this.cbVUindicators.UseVisualStyleBackColor = true;
          // 
          // cbUseVUmeter
          // 
          this.cbUseVUmeter.AutoSize = true;
          this.cbUseVUmeter.Checked = true;
          this.cbUseVUmeter.Location = new System.Drawing.Point(135, 17);
          this.cbUseVUmeter.Name = "cbUseVUmeter";
          this.cbUseVUmeter.Size = new System.Drawing.Size(70, 17);
          this.cbUseVUmeter.TabIndex = 2;
          this.cbUseVUmeter.TabStop = true;
          this.cbUseVUmeter.Text = "VU Meter";
          this.cbUseVUmeter.UseVisualStyleBackColor = true;
          this.cbUseVUmeter.CheckedChanged += new System.EventHandler(this.cbUseVUmeter_CheckedChanged);
          // 
          // cbStereoEQ
          // 
          this.cbStereoEQ.AutoSize = true;
          this.cbStereoEQ.Location = new System.Drawing.Point(77, 17);
          this.cbStereoEQ.Name = "cbStereoEQ";
          this.cbStereoEQ.Size = new System.Drawing.Size(56, 17);
          this.cbStereoEQ.TabIndex = 1;
          this.cbStereoEQ.Text = "Stereo";
          this.cbStereoEQ.UseVisualStyleBackColor = true;
          this.cbStereoEQ.CheckedChanged += new System.EventHandler(this.cbStereoEQ_CheckedChanged);
          // 
          // cbNormalEQ
          // 
          this.cbNormalEQ.AutoSize = true;
          this.cbNormalEQ.Location = new System.Drawing.Point(13, 17);
          this.cbNormalEQ.Name = "cbNormalEQ";
          this.cbNormalEQ.Size = new System.Drawing.Size(58, 17);
          this.cbNormalEQ.TabIndex = 0;
          this.cbNormalEQ.Text = "Normal";
          this.cbNormalEQ.UseVisualStyleBackColor = true;
          this.cbNormalEQ.CheckedChanged += new System.EventHandler(this.cbNormalEQ_CheckedChanged);
          // 
          // cmbDelayEqTime
          // 
          this.cmbDelayEqTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
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
          this.cmbDelayEqTime.Location = new System.Drawing.Point(163, 143);
          this.cmbDelayEqTime.Name = "cmbDelayEqTime";
          this.cmbDelayEqTime.Size = new System.Drawing.Size(52, 21);
          this.cmbDelayEqTime.TabIndex = 104;
          // 
          // lblRestrictEQ
          // 
          this.lblRestrictEQ.Location = new System.Drawing.Point(103, 124);
          this.lblRestrictEQ.Name = "lblRestrictEQ";
          this.lblRestrictEQ.Size = new System.Drawing.Size(116, 17);
          this.lblRestrictEQ.TabIndex = 115;
          this.lblRestrictEQ.Text = "updates per Seconds";
          this.lblRestrictEQ.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
          // 
          // cmbEQTitleDisplayTime
          // 
          this.cmbEQTitleDisplayTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
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
          this.cmbEQTitleDisplayTime.Location = new System.Drawing.Point(167, 207);
          this.cmbEQTitleDisplayTime.Name = "cmbEQTitleDisplayTime";
          this.cmbEQTitleDisplayTime.Size = new System.Drawing.Size(52, 21);
          this.cmbEQTitleDisplayTime.TabIndex = 110;
          // 
          // cmbEQTitleShowTime
          // 
          this.cmbEQTitleShowTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
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
          this.cmbEQTitleShowTime.Location = new System.Drawing.Point(32, 207);
          this.cmbEQTitleShowTime.Name = "cmbEQTitleShowTime";
          this.cmbEQTitleShowTime.Size = new System.Drawing.Size(52, 21);
          this.cmbEQTitleShowTime.TabIndex = 113;
          // 
          // mpEQTitleDisplay
          // 
          this.mpEQTitleDisplay.AutoSize = true;
          this.mpEQTitleDisplay.Location = new System.Drawing.Point(21, 187);
          this.mpEQTitleDisplay.Name = "mpEQTitleDisplay";
          this.mpEQTitleDisplay.Size = new System.Drawing.Size(120, 17);
          this.mpEQTitleDisplay.TabIndex = 112;
          this.mpEQTitleDisplay.Text = "Show Track Info for";
          this.mpEQTitleDisplay.UseVisualStyleBackColor = true;
          this.mpEQTitleDisplay.CheckedChanged += new System.EventHandler(this.mpEQTitleDisplay_CheckedChanged);
          // 
          // mpSmoothEQ
          // 
          this.mpSmoothEQ.AutoSize = true;
          this.mpSmoothEQ.Location = new System.Drawing.Point(21, 166);
          this.mpSmoothEQ.Name = "mpSmoothEQ";
          this.mpSmoothEQ.Size = new System.Drawing.Size(224, 17);
          this.mpSmoothEQ.TabIndex = 109;
          this.mpSmoothEQ.Text = "Use Equalizer Smoothing (Delayed decay)";
          this.mpSmoothEQ.UseVisualStyleBackColor = true;
          // 
          // mpEqDisplay
          // 
          this.mpEqDisplay.AutoSize = true;
          this.mpEqDisplay.Location = new System.Drawing.Point(5, 21);
          this.mpEqDisplay.Name = "mpEqDisplay";
          this.mpEqDisplay.Size = new System.Drawing.Size(126, 17);
          this.mpEqDisplay.TabIndex = 106;
          this.mpEqDisplay.Text = "Use Equalizer display";
          this.mpEqDisplay.UseVisualStyleBackColor = true;
          this.mpEqDisplay.CheckedChanged += new System.EventHandler(this.mpEqDisplay_CheckedChanged);
          // 
          // mpRestrictEQ
          // 
          this.mpRestrictEQ.AutoSize = true;
          this.mpRestrictEQ.Location = new System.Drawing.Point(21, 103);
          this.mpRestrictEQ.Name = "mpRestrictEQ";
          this.mpRestrictEQ.Size = new System.Drawing.Size(185, 17);
          this.mpRestrictEQ.TabIndex = 107;
          this.mpRestrictEQ.Text = "Limit Equalizer display update rate";
          this.mpRestrictEQ.UseVisualStyleBackColor = true;
          this.mpRestrictEQ.CheckedChanged += new System.EventHandler(this.mpRestrictEQ_CheckedChanged);
          // 
          // cmbEqRate
          // 
          this.cmbEqRate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
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
          this.cmbEqRate.Location = new System.Drawing.Point(32, 122);
          this.cmbEqRate.Name = "cmbEqRate";
          this.cmbEqRate.Size = new System.Drawing.Size(69, 21);
          this.cmbEqRate.TabIndex = 103;
          this.cmbEqRate.SelectedIndexChanged += new System.EventHandler(this.cmbEqRate_SelectedIndexChanged);
          // 
          // mpDelayEQ
          // 
          this.mpDelayEQ.AutoSize = true;
          this.mpDelayEQ.Location = new System.Drawing.Point(21, 144);
          this.mpDelayEQ.Name = "mpDelayEQ";
          this.mpDelayEQ.Size = new System.Drawing.Size(246, 17);
          this.mpDelayEQ.TabIndex = 108;
          this.mpDelayEQ.Text = "Delay Equalizer Start by                      Seconds";
          this.mpDelayEQ.UseVisualStyleBackColor = true;
          this.mpDelayEQ.CheckedChanged += new System.EventHandler(this.mpDelayEQ_CheckedChanged);
          // 
          // lblEQTitleDisplay
          // 
          this.lblEQTitleDisplay.Location = new System.Drawing.Point(86, 208);
          this.lblEQTitleDisplay.Name = "lblEQTitleDisplay";
          this.lblEQTitleDisplay.Size = new System.Drawing.Size(201, 17);
          this.lblEQTitleDisplay.TabIndex = 114;
          this.lblEQTitleDisplay.Text = "Seconds every                     Seconds";
          this.lblEQTitleDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
          // 
          // groupBox5
          // 
          this.groupBox5.Controls.Add(this.mpEnableDisplayActionTime);
          this.groupBox5.Controls.Add(this.cmbBlankIdleTime);
          this.groupBox5.Controls.Add(this.mpEnableDisplayAction);
          this.groupBox5.Controls.Add(this.mpBlankDisplayWithVideo);
          this.groupBox5.Controls.Add(this.mpBlankDisplayWhenIdle);
          this.groupBox5.Location = new System.Drawing.Point(10, 19);
          this.groupBox5.Name = "groupBox5";
          this.groupBox5.Size = new System.Drawing.Size(338, 97);
          this.groupBox5.TabIndex = 23;
          this.groupBox5.TabStop = false;
          this.groupBox5.Text = " Display Control Options ";
          // 
          // mpEnableDisplayActionTime
          // 
          this.mpEnableDisplayActionTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.mpEnableDisplayActionTime.BorderColor = System.Drawing.Color.Empty;
          this.mpEnableDisplayActionTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.mpEnableDisplayActionTime.Items.AddRange(new object[] {
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
            "20"});
          this.mpEnableDisplayActionTime.Location = new System.Drawing.Point(166, 36);
          this.mpEnableDisplayActionTime.Name = "mpEnableDisplayActionTime";
          this.mpEnableDisplayActionTime.Size = new System.Drawing.Size(48, 21);
          this.mpEnableDisplayActionTime.TabIndex = 96;
          // 
          // cmbBlankIdleTime
          // 
          this.cmbBlankIdleTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.cmbBlankIdleTime.BorderColor = System.Drawing.Color.Empty;
          this.cmbBlankIdleTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.cmbBlankIdleTime.Items.AddRange(new object[] {
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
          this.cmbBlankIdleTime.Location = new System.Drawing.Point(170, 58);
          this.cmbBlankIdleTime.Name = "cmbBlankIdleTime";
          this.cmbBlankIdleTime.Size = new System.Drawing.Size(48, 21);
          this.cmbBlankIdleTime.TabIndex = 98;
          // 
          // mpEnableDisplayAction
          // 
          this.mpEnableDisplayAction.AutoSize = true;
          this.mpEnableDisplayAction.Location = new System.Drawing.Point(7, 38);
          this.mpEnableDisplayAction.Name = "mpEnableDisplayAction";
          this.mpEnableDisplayAction.Size = new System.Drawing.Size(258, 17);
          this.mpEnableDisplayAction.TabIndex = 97;
          this.mpEnableDisplayAction.Text = "Enable Display on Action for                   Seconds";
          this.mpEnableDisplayAction.UseVisualStyleBackColor = true;
          this.mpEnableDisplayAction.CheckedChanged += new System.EventHandler(this.mpEnableDisplayAction_CheckedChanged);
          // 
          // mpBlankDisplayWithVideo
          // 
          this.mpBlankDisplayWithVideo.AutoSize = true;
          this.mpBlankDisplayWithVideo.Location = new System.Drawing.Point(7, 17);
          this.mpBlankDisplayWithVideo.Name = "mpBlankDisplayWithVideo";
          this.mpBlankDisplayWithVideo.Size = new System.Drawing.Size(207, 17);
          this.mpBlankDisplayWithVideo.TabIndex = 95;
          this.mpBlankDisplayWithVideo.Text = "Turn off display during Video Playback";
          this.mpBlankDisplayWithVideo.UseVisualStyleBackColor = true;
          this.mpBlankDisplayWithVideo.CheckedChanged += new System.EventHandler(this.mpBlankDisplayWithVideo_CheckedChanged);
          // 
          // mpBlankDisplayWhenIdle
          // 
          this.mpBlankDisplayWhenIdle.AutoSize = true;
          this.mpBlankDisplayWhenIdle.Location = new System.Drawing.Point(7, 60);
          this.mpBlankDisplayWhenIdle.Name = "mpBlankDisplayWhenIdle";
          this.mpBlankDisplayWhenIdle.Size = new System.Drawing.Size(261, 17);
          this.mpBlankDisplayWhenIdle.TabIndex = 99;
          this.mpBlankDisplayWhenIdle.Text = "Turn off display when idle for                    seconds";
          this.mpBlankDisplayWhenIdle.UseVisualStyleBackColor = true;
          this.mpBlankDisplayWhenIdle.CheckedChanged += new System.EventHandler(this.mpBlankDisplayWhenIdle_CheckedChanged);
          // 
          // btnOK
          // 
          this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.btnOK.Location = new System.Drawing.Point(286, 369);
          this.btnOK.Name = "btnOK";
          this.btnOK.Size = new System.Drawing.Size(80, 23);
          this.btnOK.TabIndex = 108;
          this.btnOK.Text = "&OK";
          this.btnOK.UseVisualStyleBackColor = true;
          this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
          // 
          // btnReset
          // 
          this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.btnReset.Location = new System.Drawing.Point(200, 369);
          this.btnReset.Name = "btnReset";
          this.btnReset.Size = new System.Drawing.Size(80, 23);
          this.btnReset.TabIndex = 109;
          this.btnReset.Text = "&RESET";
          this.btnReset.UseVisualStyleBackColor = true;
          this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
          // 
          // LCDHypeWrapper_AdvancedSetupForm
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(378, 397);
          this.Controls.Add(this.btnOK);
          this.Controls.Add(this.btnReset);
          this.Controls.Add(this.groupBox1);
          this.Name = "LCDHypeWrapper_AdvancedSetupForm";
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
          this.Text = "Advanced Settings";
          this.Load += new System.EventHandler(this.LCDHypeWrapper_AdvancedSetupForm_Load);
          this.groupBox1.ResumeLayout(false);
          this.groupBox7.ResumeLayout(false);
          this.groupBox7.PerformLayout();
          this.groupEQstyle.ResumeLayout(false);
          this.groupEQstyle.PerformLayout();
          this.groupBox5.ResumeLayout(false);
          this.groupBox5.PerformLayout();
          this.ResumeLayout(false);
        }

        private void LCDHypeWrapper_AdvancedSetupForm_Load(object sender, EventArgs e)
        {
            this.groupBox1.Text = " LCDHype " + Settings.Instance.Type + " Display Configuration ";
        }

        private void mpBlankDisplayWhenIdle_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void mpBlankDisplayWithVideo_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void mpDelayEQ_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void mpEnableDisplayAction_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void mpEqDisplay_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void mpEQTitleDisplay_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void mpRestrictEQ_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void SetControlState()
        {
            if (this.mpEqDisplay.Checked)
            {
                this.groupEQstyle.Enabled = true;
                if (this.cbUseVUmeter.Checked || this.cbUseVUmeter2.Checked)
                {
                    this.cbVUindicators.Enabled = true;
                }
                else
                {
                    this.cbVUindicators.Enabled = false;
                }
                this.mpRestrictEQ.Enabled = true;
                if (this.mpRestrictEQ.Checked)
                {
                    this.lblRestrictEQ.Enabled = true;
                    this.cmbEqRate.Enabled = true;
                }
                else
                {
                    this.lblRestrictEQ.Enabled = false;
                    this.cmbEqRate.Enabled = false;
                }
                this.mpDelayEQ.Enabled = true;
                if (this.mpDelayEQ.Checked)
                {
                    this.cmbDelayEqTime.Enabled = true;
                }
                else
                {
                    this.cmbDelayEqTime.Enabled = false;
                }
                this.mpSmoothEQ.Enabled = true;
                this.mpEQTitleDisplay.Enabled = true;
                if (this.mpEQTitleDisplay.Checked)
                {
                    this.lblEQTitleDisplay.Enabled = true;
                    this.cmbEQTitleDisplayTime.Enabled = true;
                    this.cmbEQTitleShowTime.Enabled = true;
                }
                else
                {
                    this.lblEQTitleDisplay.Enabled = false;
                    this.cmbEQTitleDisplayTime.Enabled = false;
                    this.cmbEQTitleShowTime.Enabled = false;
                }
            }
            else
            {
                this.groupEQstyle.Enabled = false;
                this.mpRestrictEQ.Enabled = false;
                this.lblRestrictEQ.Enabled = false;
                this.cmbEqRate.Enabled = false;
                this.mpDelayEQ.Enabled = false;
                this.cmbDelayEqTime.Enabled = false;
                this.mpSmoothEQ.Enabled = false;
                this.mpEQTitleDisplay.Enabled = false;
                this.lblEQTitleDisplay.Enabled = false;
                this.cmbEQTitleDisplayTime.Enabled = false;
                this.cmbEQTitleShowTime.Enabled = false;
            }
            //if (this.mpBlankDisplayWithVideo.Checked)
            //{
            //    this.mpEnableDisplayAction.Enabled = true;
            //    if (this.mpEnableDisplayAction.Checked)
            //    {
            //        this.mpEnableDisplayActionTime.Enabled = true;
            //    }
            //    else
            //    {
            //        this.mpEnableDisplayActionTime.Enabled = false;
            //    }
            //}
            //else
            //{
            //    this.mpEnableDisplayAction.Enabled = false;
            //    this.mpEnableDisplayActionTime.Enabled = false;
            //}
            if (this.mpBlankDisplayWhenIdle.Checked)
            {
                this.cmbBlankIdleTime.Enabled = true;
            }
            else
            {
                this.cmbBlankIdleTime.Enabled = false;
            }
        }
    }
}

