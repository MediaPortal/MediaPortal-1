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
            this.groupBox1 = new MPGroupBox();
            this.groupBox7 = new GroupBox();
            this.groupEQstyle = new GroupBox();
            this.cbUseVUmeter2 = new RadioButton();
            this.cbVUindicators = new CheckBox();
            this.cbUseVUmeter = new RadioButton();
            this.cbStereoEQ = new RadioButton();
            this.cbNormalEQ = new RadioButton();
            this.cmbDelayEqTime = new MPComboBox();
            this.lblRestrictEQ = new MPLabel();
            this.cmbEQTitleDisplayTime = new MPComboBox();
            this.cmbEQTitleShowTime = new MPComboBox();
            this.mpEQTitleDisplay = new CheckBox();
            this.mpSmoothEQ = new CheckBox();
            this.mpEqDisplay = new CheckBox();
            this.mpRestrictEQ = new CheckBox();
            this.cmbEqRate = new MPComboBox();
            this.mpDelayEQ = new CheckBox();
            this.lblEQTitleDisplay = new MPLabel();
            this.groupBox5 = new GroupBox();
            this.mpEnableDisplayActionTime = new MPComboBox();
            this.cmbBlankIdleTime = new MPComboBox();
            this.mpEnableDisplayAction = new CheckBox();
            this.mpBlankDisplayWithVideo = new CheckBox();
            this.mpBlankDisplayWhenIdle = new CheckBox();
            this.btnOK = new MPButton();
            this.btnReset = new MPButton();
            this.groupBox1.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupEQstyle.SuspendLayout();
            this.groupBox5.SuspendLayout();
            base.SuspendLayout();
            this.groupBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.groupBox1.Controls.Add(this.groupBox7);
            this.groupBox1.Controls.Add(this.groupBox5);
            this.groupBox1.FlatStyle = FlatStyle.Popup;
            this.groupBox1.Location = new Point(9, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(0x165, 0x165);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " LCDHype Display Configuration ";
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
            this.groupBox7.Location = new Point(10, 0x76);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new Size(0x152, 0xe9);
            this.groupBox7.TabIndex = 0x67;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = " Equalizer Options";
            this.groupEQstyle.Controls.Add(this.cbUseVUmeter2);
            this.groupEQstyle.Controls.Add(this.cbVUindicators);
            this.groupEQstyle.Controls.Add(this.cbUseVUmeter);
            this.groupEQstyle.Controls.Add(this.cbStereoEQ);
            this.groupEQstyle.Controls.Add(this.cbNormalEQ);
            this.groupEQstyle.Location = new Point(0x20, 0x26);
            this.groupEQstyle.Name = "groupEQstyle";
            this.groupEQstyle.Size = new Size(300, 60);
            this.groupEQstyle.TabIndex = 0x76;
            this.groupEQstyle.TabStop = false;
            this.groupEQstyle.Text = " Equalizer Style ";
            this.cbUseVUmeter2.AutoSize = true;
            this.cbUseVUmeter2.Location = new Point(0xd3, 0x11);
            this.cbUseVUmeter2.Name = "cbUseVUmeter2";
            this.cbUseVUmeter2.Size = new Size(0x4f, 0x11);
            this.cbUseVUmeter2.TabIndex = 0x7a;
            this.cbUseVUmeter2.Text = "VU Meter 2";
            this.cbUseVUmeter2.UseVisualStyleBackColor = true;
            this.cbVUindicators.AutoSize = true;
            this.cbVUindicators.Location = new Point(9, 0x27);
            this.cbVUindicators.Name = "cbVUindicators";
            this.cbVUindicators.Size = new Size(0xd5, 0x11);
            this.cbVUindicators.TabIndex = 0x79;
            this.cbVUindicators.Text = "Show Channel indicators for VU Display";
            this.cbVUindicators.UseVisualStyleBackColor = true;
            this.cbUseVUmeter.AutoSize = true;
            this.cbUseVUmeter.Location = new Point(0x87, 0x11);
            this.cbUseVUmeter.Name = "cbUseVUmeter";
            this.cbUseVUmeter.Size = new Size(70, 0x11);
            this.cbUseVUmeter.TabIndex = 2;
            this.cbUseVUmeter.Text = "VU Meter";
            this.cbUseVUmeter.UseVisualStyleBackColor = true;
            this.cbUseVUmeter.CheckedChanged += new EventHandler(this.cbUseVUmeter_CheckedChanged);
            this.cbStereoEQ.AutoSize = true;
            this.cbStereoEQ.Location = new Point(0x4d, 0x11);
            this.cbStereoEQ.Name = "cbStereoEQ";
            this.cbStereoEQ.Size = new Size(0x38, 0x11);
            this.cbStereoEQ.TabIndex = 1;
            this.cbStereoEQ.Text = "Stereo";
            this.cbStereoEQ.UseVisualStyleBackColor = true;
            this.cbStereoEQ.CheckedChanged += new EventHandler(this.cbStereoEQ_CheckedChanged);
            this.cbNormalEQ.AutoSize = true;
            this.cbNormalEQ.Checked = true;
            this.cbNormalEQ.Location = new Point(13, 0x11);
            this.cbNormalEQ.Name = "cbNormalEQ";
            this.cbNormalEQ.Size = new Size(0x3a, 0x11);
            this.cbNormalEQ.TabIndex = 0;
            this.cbNormalEQ.TabStop = true;
            this.cbNormalEQ.Text = "Normal";
            this.cbNormalEQ.UseVisualStyleBackColor = true;
            this.cbNormalEQ.CheckedChanged += new EventHandler(this.cbNormalEQ_CheckedChanged);
            this.cmbDelayEqTime.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.cmbDelayEqTime.BorderColor = Color.Empty;
            this.cmbDelayEqTime.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbDelayEqTime.Items.AddRange(new object[] { 
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", 
                "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"
             });
            this.cmbDelayEqTime.Location = new Point(0xa3, 0x8f);
            this.cmbDelayEqTime.Name = "cmbDelayEqTime";
            this.cmbDelayEqTime.Size = new Size(0x34, 0x15);
            this.cmbDelayEqTime.TabIndex = 0x68;
            this.lblRestrictEQ.Location = new Point(0x67, 0x7c);
            this.lblRestrictEQ.Name = "lblRestrictEQ";
            this.lblRestrictEQ.Size = new Size(0x74, 0x11);
            this.lblRestrictEQ.TabIndex = 0x73;
            this.lblRestrictEQ.Text = "updates per Seconds";
            this.lblRestrictEQ.TextAlign = ContentAlignment.MiddleLeft;
            this.cmbEQTitleDisplayTime.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.cmbEQTitleDisplayTime.BorderColor = Color.Empty;
            this.cmbEQTitleDisplayTime.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbEQTitleDisplayTime.Items.AddRange(new object[] { 
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", 
                "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"
             });
            this.cmbEQTitleDisplayTime.Location = new Point(0xa7, 0xcf);
            this.cmbEQTitleDisplayTime.Name = "cmbEQTitleDisplayTime";
            this.cmbEQTitleDisplayTime.Size = new Size(0x34, 0x15);
            this.cmbEQTitleDisplayTime.TabIndex = 110;
            this.cmbEQTitleShowTime.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.cmbEQTitleShowTime.BorderColor = Color.Empty;
            this.cmbEQTitleShowTime.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbEQTitleShowTime.Items.AddRange(new object[] { 
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", 
                "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"
             });
            this.cmbEQTitleShowTime.Location = new Point(0x20, 0xcf);
            this.cmbEQTitleShowTime.Name = "cmbEQTitleShowTime";
            this.cmbEQTitleShowTime.Size = new Size(0x34, 0x15);
            this.cmbEQTitleShowTime.TabIndex = 0x71;
            this.mpEQTitleDisplay.AutoSize = true;
            this.mpEQTitleDisplay.Location = new Point(0x15, 0xbb);
            this.mpEQTitleDisplay.Name = "mpEQTitleDisplay";
            this.mpEQTitleDisplay.Size = new Size(120, 0x11);
            this.mpEQTitleDisplay.TabIndex = 0x70;
            this.mpEQTitleDisplay.Text = "Show Track Info for";
            this.mpEQTitleDisplay.UseVisualStyleBackColor = true;
            this.mpEQTitleDisplay.CheckedChanged += new EventHandler(this.mpEQTitleDisplay_CheckedChanged);
            this.mpSmoothEQ.AutoSize = true;
            this.mpSmoothEQ.Location = new Point(0x15, 0xa6);
            this.mpSmoothEQ.Name = "mpSmoothEQ";
            this.mpSmoothEQ.Size = new Size(0xe0, 0x11);
            this.mpSmoothEQ.TabIndex = 0x6d;
            this.mpSmoothEQ.Text = "Use Equalizer Smoothing (Delayed decay)";
            this.mpSmoothEQ.UseVisualStyleBackColor = true;
            this.mpEqDisplay.AutoSize = true;
            this.mpEqDisplay.Location = new Point(5, 0x15);
            this.mpEqDisplay.Name = "mpEqDisplay";
            this.mpEqDisplay.Size = new Size(0x7e, 0x11);
            this.mpEqDisplay.TabIndex = 0x6a;
            this.mpEqDisplay.Text = "Use Equalizer display";
            this.mpEqDisplay.UseVisualStyleBackColor = true;
            this.mpEqDisplay.CheckedChanged += new EventHandler(this.mpEqDisplay_CheckedChanged);
            this.mpRestrictEQ.AutoSize = true;
            this.mpRestrictEQ.Location = new Point(0x15, 0x67);
            this.mpRestrictEQ.Name = "mpRestrictEQ";
            this.mpRestrictEQ.Size = new Size(0xb9, 0x11);
            this.mpRestrictEQ.TabIndex = 0x6b;
            this.mpRestrictEQ.Text = "Limit Equalizer display update rate";
            this.mpRestrictEQ.UseVisualStyleBackColor = true;
            this.mpRestrictEQ.CheckedChanged += new EventHandler(this.mpRestrictEQ_CheckedChanged);
            this.cmbEqRate.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.cmbEqRate.BorderColor = Color.Empty;
            this.cmbEqRate.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbEqRate.Items.AddRange(new object[] { 
                "MAX", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", 
                "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", 
                "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", 
                "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60"
             });
            this.cmbEqRate.Location = new Point(0x20, 0x7a);
            this.cmbEqRate.Name = "cmbEqRate";
            this.cmbEqRate.Size = new Size(0x45, 0x15);
            this.cmbEqRate.TabIndex = 0x67;
            this.cmbEqRate.SelectedIndexChanged += new EventHandler(this.cmbEqRate_SelectedIndexChanged);
            this.mpDelayEQ.AutoSize = true;
            this.mpDelayEQ.Location = new Point(0x15, 0x90);
            this.mpDelayEQ.Name = "mpDelayEQ";
            this.mpDelayEQ.Size = new Size(0xf6, 0x11);
            this.mpDelayEQ.TabIndex = 0x6c;
            this.mpDelayEQ.Text = "Delay Equalizer Start by                      Seconds";
            this.mpDelayEQ.UseVisualStyleBackColor = true;
            this.mpDelayEQ.CheckedChanged += new EventHandler(this.mpDelayEQ_CheckedChanged);
            this.lblEQTitleDisplay.Location = new Point(0x56, 0xd0);
            this.lblEQTitleDisplay.Name = "lblEQTitleDisplay";
            this.lblEQTitleDisplay.Size = new Size(0xc9, 0x11);
            this.lblEQTitleDisplay.TabIndex = 0x72;
            this.lblEQTitleDisplay.Text = "Seconds every                     Seconds";
            this.lblEQTitleDisplay.TextAlign = ContentAlignment.MiddleLeft;
            this.groupBox5.Controls.Add(this.mpEnableDisplayActionTime);
            this.groupBox5.Controls.Add(this.cmbBlankIdleTime);
            this.groupBox5.Controls.Add(this.mpEnableDisplayAction);
            this.groupBox5.Controls.Add(this.mpBlankDisplayWithVideo);
            this.groupBox5.Controls.Add(this.mpBlankDisplayWhenIdle);
            this.groupBox5.Location = new Point(10, 0x13);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new Size(0x152, 0x61);
            this.groupBox5.TabIndex = 0x17;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = " Display Control Options ";
            this.mpEnableDisplayActionTime.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.mpEnableDisplayActionTime.BorderColor = Color.Empty;
            this.mpEnableDisplayActionTime.DropDownStyle = ComboBoxStyle.DropDownList;
            this.mpEnableDisplayActionTime.Items.AddRange(new object[] { 
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", 
                "16", "17", "18", "19", "20"
             });
            this.mpEnableDisplayActionTime.Location = new Point(0xb7, 0x24);
            this.mpEnableDisplayActionTime.Name = "mpEnableDisplayActionTime";
            this.mpEnableDisplayActionTime.Size = new Size(0x30, 0x15);
            this.mpEnableDisplayActionTime.TabIndex = 0x60;
            this.cmbBlankIdleTime.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.cmbBlankIdleTime.BorderColor = Color.Empty;
            this.cmbBlankIdleTime.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbBlankIdleTime.Items.AddRange(new object[] { 
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", 
                "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"
             });
            this.cmbBlankIdleTime.Location = new Point(170, 0x3a);
            this.cmbBlankIdleTime.Name = "cmbBlankIdleTime";
            this.cmbBlankIdleTime.Size = new Size(0x30, 0x15);
            this.cmbBlankIdleTime.TabIndex = 0x62;
            this.mpEnableDisplayAction.AutoSize = true;
            this.mpEnableDisplayAction.Location = new Point(0x17, 0x26);
            this.mpEnableDisplayAction.Name = "mpEnableDisplayAction";
            this.mpEnableDisplayAction.Size = new Size(0x102, 0x11);
            this.mpEnableDisplayAction.TabIndex = 0x61;
            this.mpEnableDisplayAction.Text = "Enable Display on Action for                   Seconds";
            this.mpEnableDisplayAction.UseVisualStyleBackColor = true;
            this.mpEnableDisplayAction.CheckedChanged += new EventHandler(this.mpEnableDisplayAction_CheckedChanged);
            this.mpBlankDisplayWithVideo.AutoSize = true;
            this.mpBlankDisplayWithVideo.Location = new Point(7, 0x11);
            this.mpBlankDisplayWithVideo.Name = "mpBlankDisplayWithVideo";
            this.mpBlankDisplayWithVideo.Size = new Size(0xcf, 0x11);
            this.mpBlankDisplayWithVideo.TabIndex = 0x5f;
            this.mpBlankDisplayWithVideo.Text = "Turn off display during Video Playback";
            this.mpBlankDisplayWithVideo.UseVisualStyleBackColor = true;
            this.mpBlankDisplayWithVideo.CheckedChanged += new EventHandler(this.mpBlankDisplayWithVideo_CheckedChanged);
            this.mpBlankDisplayWhenIdle.AutoSize = true;
            this.mpBlankDisplayWhenIdle.Location = new Point(7, 60);
            this.mpBlankDisplayWhenIdle.Name = "mpBlankDisplayWhenIdle";
            this.mpBlankDisplayWhenIdle.Size = new Size(0x105, 0x11);
            this.mpBlankDisplayWhenIdle.TabIndex = 0x63;
            this.mpBlankDisplayWhenIdle.Text = "Turn off display when idle for                    seconds";
            this.mpBlankDisplayWhenIdle.UseVisualStyleBackColor = true;
            this.mpBlankDisplayWhenIdle.CheckedChanged += new EventHandler(this.mpBlankDisplayWhenIdle_CheckedChanged);
            this.btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnOK.Location = new Point(0x11e, 0x171);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(80, 0x17);
            this.btnOK.TabIndex = 0x6c;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.btnReset.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnReset.Location = new Point(200, 0x171);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new Size(80, 0x17);
            this.btnReset.TabIndex = 0x6d;
            this.btnReset.Text = "&RESET";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new EventHandler(this.btnReset_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x17a, 0x18d);
            base.Controls.Add(this.btnOK);
            base.Controls.Add(this.btnReset);
            base.Controls.Add(this.groupBox1);
            base.Name = "LCDHypeWrapper_AdvancedSetupForm";
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Advanced Settings";
            base.Load += new EventHandler(this.LCDHypeWrapper_AdvancedSetupForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupEQstyle.ResumeLayout(false);
            this.groupEQstyle.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            base.ResumeLayout(false);
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
            if (this.mpBlankDisplayWithVideo.Checked)
            {
                this.mpEnableDisplayAction.Enabled = true;
                if (this.mpEnableDisplayAction.Checked)
                {
                    this.mpEnableDisplayActionTime.Enabled = true;
                }
                else
                {
                    this.mpEnableDisplayActionTime.Enabled = false;
                }
            }
            else
            {
                this.mpEnableDisplayAction.Enabled = false;
                this.mpEnableDisplayActionTime.Enabled = false;
            }
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

