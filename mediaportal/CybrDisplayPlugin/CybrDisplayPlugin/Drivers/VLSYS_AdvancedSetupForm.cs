namespace CybrDisplayPlugin.Drivers
{
    using CybrDisplayPlugin;
    using MediaPortal.Configuration;
    using MediaPortal.GUI.Library;
    using MediaPortal.InputDevices;
    using MediaPortal.UserInterface.Controls;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    public class VLSYS_AdvancedSetupForm : Form
    {
        private bool _FormLoaded;
        private MPButton btnOK;
        private MPButton btnRemoteSetup;
        private MPButton btnReset;
        private MPButton btnTest;
        private CheckBox cbDisableRepeat;
        private ComboBox cbFan1_SetOff;
        private ComboBox cbFan1_SetOn;
        private CheckBox cbFan1Auto;
        private CheckBox cbFan1ShutdownManual;
        private ComboBox cbFan2_SetOff;
        private ComboBox cbFan2_SetOn;
        private CheckBox cbFan2Auto;
        private CheckBox cbFan2ShutdownManual;
        private CheckBox cbManageMHC;
        private CheckBox cbNoRemote;
        private RadioButton cbNormalEQ;
        private RadioButton cbStereoEQ;
        private CheckBox cbUseClockOnShutdown;
        private CheckBox cbUseFans;
        private RadioButton cbUseVUmeter;
        private RadioButton cbUseVUmeter2;
        private CheckBox cbVUindicators;
        private MPComboBox cmbBlankIdleTime;
        private MPComboBox cmbDelayEqTime;
        private ComboBox cmbDeviceType;
        private MPComboBox cmbEqRate;
        private MPComboBox cmbEQTitleDisplayTime;
        private MPComboBox cmbEQTitleShowTime;
        private readonly IContainer components;
        private object ControlStateLock = new object();
        private GroupBox Fan1Box;
        private GroupBox Fan2Box;
        private MPGroupBox groupBox1;
        private GroupBox groupBox3;
        private GroupBox groupBox6;
        private GroupBox groupBoxDisplay;
        private GroupBox groupBoxEqualizer;
        private GroupBox groupBoxFan;
        private GroupBox groupBoxRemote;
        private GroupBox groupEQstyle;
        private Label label3;
        private Label lblDelay;
        private MPLabel lblEQTitleDisplay;
        private Label lblFan1;
        private Label lblFan1_SetOff;
        private Label lblFan1_SetOn;
        private Label lblFan2;
        private Label lblFan2_SetOff;
        private Label lblFan2_SetOn;
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
        private object sUpdateMutex = new object();
        private TrackBar tbDelay;
        private TrackBar tbFan1;
        private TrackBar tbFan2;

        public VLSYS_AdvancedSetupForm()
        {
            Log.Debug("VLSYS_Mplay.VLSYS_AdvancedSetupForm(): Constructor started", new object[0]);
            this.InitializeComponent();
            this.cbManageMHC.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "ManageMHC");
            this.cbNoRemote.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "DisableRemote");
            this.cbDisableRepeat.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "DisableRepeat");
            this.cbUseFans.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "UseFans");
            this.tbFan1.DataBindings.Add("Value", VLSYS_Mplay.AdvancedSettings.Instance, "Fan1");
            this.tbFan2.DataBindings.Add("Value", VLSYS_Mplay.AdvancedSettings.Instance, "Fan2");
            this.cbFan1Auto.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "Fan1Auto");
            this.cbFan1_SetOff.DataBindings.Add("SelectedIndex", VLSYS_Mplay.AdvancedSettings.Instance, "Fan1_SetOff");
            this.cbFan1_SetOn.DataBindings.Add("SelectedIndex", VLSYS_Mplay.AdvancedSettings.Instance, "Fan1_SetOn");
            this.cbFan1ShutdownManual.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "Fan1_AutoMS");
            this.cbFan2Auto.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "Fan2Auto");
            this.cbFan2_SetOff.DataBindings.Add("SelectedIndex", VLSYS_Mplay.AdvancedSettings.Instance, "Fan2_SetOff");
            this.cbFan2_SetOn.DataBindings.Add("SelectedIndex", VLSYS_Mplay.AdvancedSettings.Instance, "Fan2_SetOn");
            this.cbFan2ShutdownManual.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "Fan2_AutoMS");
            this.tbDelay.DataBindings.Add("Value", VLSYS_Mplay.AdvancedSettings.Instance, "RepeatDelay");
            this.cmbDeviceType.DataBindings.Add("SelectedItem", VLSYS_Mplay.AdvancedSettings.Instance, "DeviceType");
            this.mpEqDisplay.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "EqDisplay");
            this.mpRestrictEQ.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "RestrictEQ");
            this.cmbEqRate.DataBindings.Add("SelectedIndex", VLSYS_Mplay.AdvancedSettings.Instance, "EqRate");
            this.mpDelayEQ.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "DelayEQ");
            this.cmbDelayEqTime.DataBindings.Add("SelectedIndex", VLSYS_Mplay.AdvancedSettings.Instance, "DelayEqTime");
            this.mpSmoothEQ.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "SmoothEQ");
            this.mpBlankDisplayWithVideo.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "BlankDisplayWithVideo");
            this.mpEnableDisplayAction.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "EnableDisplayAction");
            this.mpEnableDisplayActionTime.DataBindings.Add("SelectedIndex", VLSYS_Mplay.AdvancedSettings.Instance, "EnableDisplayActionTime");
            this.mpEQTitleDisplay.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "EQTitleDisplay");
            this.cmbEQTitleDisplayTime.DataBindings.Add("SelectedIndex", VLSYS_Mplay.AdvancedSettings.Instance, "EQTitleDisplayTime");
            this.cmbEQTitleShowTime.DataBindings.Add("SelectedIndex", VLSYS_Mplay.AdvancedSettings.Instance, "EQTitleShowTime");
            this.mpBlankDisplayWhenIdle.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "BlankDisplayWhenIdle");
            this.cmbBlankIdleTime.DataBindings.Add("SelectedIndex", VLSYS_Mplay.AdvancedSettings.Instance, "BlankIdleTime");
            if (VLSYS_Mplay.AdvancedSettings.Instance.NormalEQ)
            {
                this.cbNormalEQ.Checked = true;
            }
            else if (VLSYS_Mplay.AdvancedSettings.Instance.StereoEQ)
            {
                this.cbStereoEQ.Checked = true;
            }
            else if (VLSYS_Mplay.AdvancedSettings.Instance.VUmeter)
            {
                this.cbUseVUmeter.Checked = true;
            }
            else
            {
                this.cbUseVUmeter2.Checked = true;
            }
            this.cbVUindicators.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "VUindicators");
            this.cbUseClockOnShutdown.DataBindings.Add("Checked", VLSYS_Mplay.AdvancedSettings.Instance, "UseClockOnShutdown");
            this.Refresh();
            this.SetControlState();
            Log.Debug("VLSYS_Mplay.VLSYS_AdvancedSetupForm(): Constructor completed", new object[0]);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Log.Debug("VLSYS_Mplay.AdvancedSetupForm.btnOK_Click(): started", new object[0]);
            if (this.cbNormalEQ.Checked)
            {
                VLSYS_Mplay.AdvancedSettings.Instance.NormalEQ = true;
                VLSYS_Mplay.AdvancedSettings.Instance.StereoEQ = false;
                VLSYS_Mplay.AdvancedSettings.Instance.VUmeter = false;
                VLSYS_Mplay.AdvancedSettings.Instance.VUmeter2 = false;
            }
            else if (this.cbStereoEQ.Checked)
            {
                VLSYS_Mplay.AdvancedSettings.Instance.NormalEQ = false;
                VLSYS_Mplay.AdvancedSettings.Instance.StereoEQ = true;
                VLSYS_Mplay.AdvancedSettings.Instance.VUmeter = false;
                VLSYS_Mplay.AdvancedSettings.Instance.VUmeter2 = false;
            }
            else if (this.cbUseVUmeter.Checked)
            {
                VLSYS_Mplay.AdvancedSettings.Instance.NormalEQ = false;
                VLSYS_Mplay.AdvancedSettings.Instance.StereoEQ = false;
                VLSYS_Mplay.AdvancedSettings.Instance.VUmeter = true;
                VLSYS_Mplay.AdvancedSettings.Instance.VUmeter2 = false;
            }
            else
            {
                VLSYS_Mplay.AdvancedSettings.Instance.NormalEQ = false;
                VLSYS_Mplay.AdvancedSettings.Instance.StereoEQ = false;
                VLSYS_Mplay.AdvancedSettings.Instance.VUmeter = false;
                VLSYS_Mplay.AdvancedSettings.Instance.VUmeter2 = true;
            }
            VLSYS_Mplay.AdvancedSettings.Save();
            base.Hide();
            base.Close();
            Log.Debug("VLSYS_Mplay.AdvancedSetupForm.btnOK_Click(): Completed", new object[0]);
        }

        private void btnRemoteSetup_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "VLSYS_Mplay.xml")))
                {
                    VLSYS_Mplay.AdvancedSettings.CreateDefaultRemoteMapping();
                }
                new InputMappingForm("VLSYS_Mplay").ShowDialog(this);
            }
            catch (Exception exception)
            {
                Log.Info("VLSYS_AdvancedSetupForm.btnRemoteSetup_Click() CAUGHT EXCEPTION: {0}", new object[] { exception });
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Log.Debug("VLSYS_Mplay.AdvancedSetupForm.btnReset_Click(): started", new object[0]);
            VLSYS_Mplay.AdvancedSettings.SetDefaults();
            this.cbManageMHC.Checked = VLSYS_Mplay.AdvancedSettings.Instance.ManageMHC;
            this.cbNoRemote.Checked = VLSYS_Mplay.AdvancedSettings.Instance.DisableRemote;
            this.cbDisableRepeat.Checked = VLSYS_Mplay.AdvancedSettings.Instance.DisableRepeat;
            this.cbUseFans.Checked = VLSYS_Mplay.AdvancedSettings.Instance.UseFans;
            this.tbFan1.Value = VLSYS_Mplay.AdvancedSettings.Instance.Fan1;
            this.tbFan2.Value = VLSYS_Mplay.AdvancedSettings.Instance.Fan2;
            this.cbFan1Auto.Checked = VLSYS_Mplay.AdvancedSettings.Instance.Fan1Auto;
            this.cbFan1_SetOff.SelectedIndex = VLSYS_Mplay.AdvancedSettings.Instance.Fan1_SetOff;
            this.cbFan1_SetOn.SelectedIndex = VLSYS_Mplay.AdvancedSettings.Instance.Fan1_SetOn;
            this.cbFan1ShutdownManual.Checked = VLSYS_Mplay.AdvancedSettings.Instance.Fan1_AutoMS;
            this.cbFan2Auto.Checked = VLSYS_Mplay.AdvancedSettings.Instance.Fan2Auto;
            this.cbFan2_SetOff.SelectedIndex = VLSYS_Mplay.AdvancedSettings.Instance.Fan2_SetOff;
            this.cbFan2_SetOn.SelectedIndex = VLSYS_Mplay.AdvancedSettings.Instance.Fan2_SetOn;
            this.cbFan2ShutdownManual.Checked = VLSYS_Mplay.AdvancedSettings.Instance.Fan2_AutoMS;
            this.tbDelay.Value = VLSYS_Mplay.AdvancedSettings.Instance.RepeatDelay;
            this.cmbDeviceType.SelectedItem = VLSYS_Mplay.AdvancedSettings.Instance.DeviceType;
            if (VLSYS_Mplay.AdvancedSettings.Instance.NormalEQ)
            {
                this.cbNormalEQ.Checked = true;
            }
            else if (VLSYS_Mplay.AdvancedSettings.Instance.StereoEQ)
            {
                this.cbStereoEQ.Checked = true;
            }
            else if (VLSYS_Mplay.AdvancedSettings.Instance.VUmeter)
            {
                this.cbUseVUmeter.Checked = true;
            }
            else
            {
                this.cbUseVUmeter2.Checked = true;
            }
            this.mpEqDisplay.Checked = VLSYS_Mplay.AdvancedSettings.Instance.EqDisplay;
            this.cbVUindicators.Checked = VLSYS_Mplay.AdvancedSettings.Instance.VUindicators;
            this.mpRestrictEQ.Checked = VLSYS_Mplay.AdvancedSettings.Instance.RestrictEQ;
            this.cmbEqRate.SelectedIndex = VLSYS_Mplay.AdvancedSettings.Instance.EqRate;
            this.mpDelayEQ.Checked = VLSYS_Mplay.AdvancedSettings.Instance.DelayEQ;
            this.cmbDelayEqTime.SelectedIndex = VLSYS_Mplay.AdvancedSettings.Instance.DelayEqTime;
            this.mpSmoothEQ.Checked = VLSYS_Mplay.AdvancedSettings.Instance.SmoothEQ;
            this.mpBlankDisplayWithVideo.Checked = VLSYS_Mplay.AdvancedSettings.Instance.BlankDisplayWithVideo;
            this.mpEnableDisplayAction.Checked = VLSYS_Mplay.AdvancedSettings.Instance.EnableDisplayAction;
            this.mpEnableDisplayActionTime.SelectedIndex = VLSYS_Mplay.AdvancedSettings.Instance.EnableDisplayActionTime;
            this.mpEQTitleDisplay.Checked = VLSYS_Mplay.AdvancedSettings.Instance.EQTitleDisplay;
            this.cmbEQTitleDisplayTime.SelectedIndex = VLSYS_Mplay.AdvancedSettings.Instance.EQTitleDisplayTime;
            this.cmbEQTitleShowTime.SelectedIndex = VLSYS_Mplay.AdvancedSettings.Instance.EQTitleShowTime;
            this.mpBlankDisplayWhenIdle.Checked = VLSYS_Mplay.AdvancedSettings.Instance.BlankDisplayWhenIdle;
            this.cmbBlankIdleTime.SelectedIndex = VLSYS_Mplay.AdvancedSettings.Instance.BlankIdleTime;
            this.cbUseClockOnShutdown.Checked = VLSYS_Mplay.AdvancedSettings.Instance.UseClockOnShutdown;
            this.SetControlState();
            this.SetFanLabel(this.tbFan1);
            this.SetFanLabel(this.tbFan2);
            this.Refresh();
            Log.Debug("VLSYS_Mplay.AdvancedSetupForm.btnReset_Click(): Completed", new object[0]);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                new MessageEditForm("ExternalDisplay.xml").ShowDialog(this);
            }
            catch (Exception exception)
            {
                Log.Info("VLSYS_AdvancedSetupForm.btnRemoteSetup_Click() CAUGHT EXCEPTION: {0}", new object[] { exception });
            }
        }

        private void cbDisableRepeat_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void cbFan1_SetOff_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((this.cbFan1_SetOn.Text != string.Empty) && (this.cbFan1_SetOff.SelectedIndex > this.cbFan1_SetOn.SelectedIndex))
            {
                this.cbFan1_SetOff.SelectedIndex = this.cbFan1_SetOn.SelectedIndex - 1;
            }
        }

        private void cbFan1_SetOn_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((this.cbFan1_SetOff.Text != string.Empty) && (this.cbFan1_SetOn.SelectedIndex < this.cbFan1_SetOff.SelectedIndex))
            {
                this.cbFan1_SetOn.SelectedIndex = this.cbFan1_SetOff.SelectedIndex + 1;
            }
        }

        private void cbFan1Auto_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
            this.SetFanLabel(this.tbFan1);
        }

        private void cbFan1ShutdownManual_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
            this.SetFanLabel(this.tbFan1);
        }

        private void cbFan2_SetOff_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((this.cbFan2_SetOn.Text != string.Empty) && (this.cbFan2_SetOff.SelectedIndex > this.cbFan2_SetOn.SelectedIndex))
            {
                this.cbFan2_SetOff.SelectedIndex = this.cbFan2_SetOn.SelectedIndex - 1;
            }
        }

        private void cbFan2_SetOn_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((this.cbFan2_SetOff.Text != string.Empty) && (this.cbFan2_SetOn.SelectedIndex < this.cbFan2_SetOff.SelectedIndex))
            {
                this.cbFan2_SetOn.SelectedIndex = this.cbFan2_SetOff.SelectedIndex + 1;
            }
        }

        private void cbFan2Auto_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
            this.SetFanLabel(this.tbFan2);
        }

        private void cbFan2ShutdownManual_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
            this.SetFanLabel(this.tbFan2);
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

        private void cbUseFans_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void cbUseVUmeter_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void cbUseVuMeter2_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void cmbDeviceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._FormLoaded)
            {
                this.SetControlState();
            }
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
            this.groupBoxEqualizer = new GroupBox();
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
            this.groupBoxDisplay = new GroupBox();
            this.cbUseClockOnShutdown = new CheckBox();
            this.mpEnableDisplayActionTime = new MPComboBox();
            this.cmbBlankIdleTime = new MPComboBox();
            this.mpEnableDisplayAction = new CheckBox();
            this.mpBlankDisplayWithVideo = new CheckBox();
            this.mpBlankDisplayWhenIdle = new CheckBox();
            this.label3 = new Label();
            this.cmbDeviceType = new ComboBox();
            this.cbManageMHC = new CheckBox();
            this.groupBoxRemote = new GroupBox();
            this.lblDelay = new Label();
            this.tbDelay = new TrackBar();
            this.cbDisableRepeat = new CheckBox();
            this.cbNoRemote = new CheckBox();
            this.groupBoxFan = new GroupBox();
            this.cbFan1Auto = new CheckBox();
            this.groupBox3 = new GroupBox();
            this.cbFan1ShutdownManual = new CheckBox();
            this.cbFan1_SetOn = new ComboBox();
            this.cbFan1_SetOff = new ComboBox();
            this.lblFan1_SetOn = new Label();
            this.lblFan1_SetOff = new Label();
            this.cbUseFans = new CheckBox();
            this.lblFan1 = new Label();
            this.tbFan1 = new TrackBar();
            this.Fan1Box = new GroupBox();
            this.Fan2Box = new GroupBox();
            this.cbFan2Auto = new CheckBox();
            this.groupBox6 = new GroupBox();
            this.cbFan2ShutdownManual = new CheckBox();
            this.cbFan2_SetOn = new ComboBox();
            this.cbFan2_SetOff = new ComboBox();
            this.lblFan2_SetOn = new Label();
            this.lblFan2_SetOff = new Label();
            this.tbFan2 = new TrackBar();
            this.lblFan2 = new Label();
            this.btnOK = new MPButton();
            this.btnReset = new MPButton();
            this.btnRemoteSetup = new MPButton();
            this.btnTest = new MPButton();
            this.groupBox1.SuspendLayout();
            this.groupBoxEqualizer.SuspendLayout();
            this.groupEQstyle.SuspendLayout();
            this.groupBoxDisplay.SuspendLayout();
            this.groupBoxRemote.SuspendLayout();
            this.tbDelay.BeginInit();
            this.groupBoxFan.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tbFan1.BeginInit();
            this.Fan2Box.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.tbFan2.BeginInit();
            base.SuspendLayout();
            this.groupBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.groupBox1.Controls.Add(this.groupBoxEqualizer);
            this.groupBox1.Controls.Add(this.groupBoxDisplay);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cmbDeviceType);
            this.groupBox1.Controls.Add(this.cbManageMHC);
            this.groupBox1.Controls.Add(this.groupBoxRemote);
            this.groupBox1.Controls.Add(this.groupBoxFan);
            this.groupBox1.FlatStyle = FlatStyle.Popup;
            this.groupBox1.Location = new Point(9, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(0x21f, 0x1f3);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "VLSYS_Mplay Configuration";
            this.groupBoxEqualizer.Controls.Add(this.groupEQstyle);
            this.groupBoxEqualizer.Controls.Add(this.cmbDelayEqTime);
            this.groupBoxEqualizer.Controls.Add(this.lblRestrictEQ);
            this.groupBoxEqualizer.Controls.Add(this.cmbEQTitleDisplayTime);
            this.groupBoxEqualizer.Controls.Add(this.cmbEQTitleShowTime);
            this.groupBoxEqualizer.Controls.Add(this.mpEQTitleDisplay);
            this.groupBoxEqualizer.Controls.Add(this.mpSmoothEQ);
            this.groupBoxEqualizer.Controls.Add(this.mpEqDisplay);
            this.groupBoxEqualizer.Controls.Add(this.mpRestrictEQ);
            this.groupBoxEqualizer.Controls.Add(this.cmbEqRate);
            this.groupBoxEqualizer.Controls.Add(this.mpDelayEQ);
            this.groupBoxEqualizer.Controls.Add(this.lblEQTitleDisplay);
            this.groupBoxEqualizer.Location = new Point(10, 260);
            this.groupBoxEqualizer.Name = "groupBoxEqualizer";
            this.groupBoxEqualizer.Size = new Size(0x152, 0xe9);
            this.groupBoxEqualizer.TabIndex = 0x67;
            this.groupBoxEqualizer.TabStop = false;
            this.groupBoxEqualizer.Text = " Equalizer Options";
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
            this.cbUseVUmeter2.Location = new Point(0xd4, 0x11);
            this.cbUseVUmeter2.Name = "cbUseVUmeter2";
            this.cbUseVUmeter2.Size = new Size(0x4f, 0x11);
            this.cbUseVUmeter2.TabIndex = 0x79;
            this.cbUseVUmeter2.Text = "VU Meter 2";
            this.cbUseVUmeter2.UseVisualStyleBackColor = true;
            this.cbUseVUmeter2.CheckedChanged += new EventHandler(this.cbUseVuMeter2_CheckedChanged);
            this.cbVUindicators.AutoSize = true;
            this.cbVUindicators.Location = new Point(8, 40);
            this.cbVUindicators.Name = "cbVUindicators";
            this.cbVUindicators.Size = new Size(0xd5, 0x11);
            this.cbVUindicators.TabIndex = 120;
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
            this.cmbDelayEqTime.Location = new Point(160, 0x8f);
            this.cmbDelayEqTime.Name = "cmbDelayEqTime";
            this.cmbDelayEqTime.Size = new Size(0x34, 0x15);
            this.cmbDelayEqTime.TabIndex = 0x68;
            this.lblRestrictEQ.Location = new Point(100, 0x7c);
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
            this.cmbEQTitleDisplayTime.Location = new Point(0xa5, 0xcf);
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
            this.groupBoxDisplay.Controls.Add(this.cbUseClockOnShutdown);
            this.groupBoxDisplay.Controls.Add(this.mpEnableDisplayActionTime);
            this.groupBoxDisplay.Controls.Add(this.cmbBlankIdleTime);
            this.groupBoxDisplay.Controls.Add(this.mpEnableDisplayAction);
            this.groupBoxDisplay.Controls.Add(this.mpBlankDisplayWithVideo);
            this.groupBoxDisplay.Controls.Add(this.mpBlankDisplayWhenIdle);
            this.groupBoxDisplay.Location = new Point(10, 0x9c);
            this.groupBoxDisplay.Name = "groupBoxDisplay";
            this.groupBoxDisplay.Size = new Size(0x152, 0x67);
            this.groupBoxDisplay.TabIndex = 0x17;
            this.groupBoxDisplay.TabStop = false;
            this.groupBoxDisplay.Text = " Display Control Options ";
            this.cbUseClockOnShutdown.AutoSize = true;
            this.cbUseClockOnShutdown.Location = new Point(8, 0x51);
            this.cbUseClockOnShutdown.Name = "cbUseClockOnShutdown";
            this.cbUseClockOnShutdown.Size = new Size(0xcc, 0x11);
            this.cbUseClockOnShutdown.TabIndex = 100;
            this.cbUseClockOnShutdown.Text = "Use Clock on shutdown (If supported)";
            this.cbUseClockOnShutdown.UseVisualStyleBackColor = true;
            this.mpEnableDisplayActionTime.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.mpEnableDisplayActionTime.BorderColor = Color.Empty;
            this.mpEnableDisplayActionTime.DropDownStyle = ComboBoxStyle.DropDownList;
            this.mpEnableDisplayActionTime.Items.AddRange(new object[] { 
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", 
                "16", "17", "18", "19", "20"
             });
            this.mpEnableDisplayActionTime.Location = new Point(0xb5, 0x24);
            this.mpEnableDisplayActionTime.Name = "mpEnableDisplayActionTime";
            this.mpEnableDisplayActionTime.Size = new Size(0x2a, 0x15);
            this.mpEnableDisplayActionTime.TabIndex = 0x60;
            this.cmbBlankIdleTime.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.cmbBlankIdleTime.BorderColor = Color.Empty;
            this.cmbBlankIdleTime.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbBlankIdleTime.Items.AddRange(new object[] { 
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", 
                "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"
             });
            this.cmbBlankIdleTime.Location = new Point(0xa7, 0x3a);
            this.cmbBlankIdleTime.Name = "cmbBlankIdleTime";
            this.cmbBlankIdleTime.Size = new Size(0x2a, 0x15);
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
            this.label3.AutoSize = true;
            this.label3.Location = new Point(15, 0x16);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x44, 13);
            this.label3.TabIndex = 0x16;
            this.label3.Text = "Device Type";
            this.cmbDeviceType.FormattingEnabled = true;
            this.cmbDeviceType.Items.AddRange(new object[] { "AUTOMATIC", "LE2 - M.Play 202", "ME4 - M.Play 202 Plus", "ME5 - M.Play 202 Plus R2", "ME7 - M.Play MR300 / M.Play Blast", "MP4 - M.Play 202", "MP5 - M.Play 202 Plus", "MP7 - M.Play Blast", "MR2 - M.Play Blast", "MZ4 - M.Play MR300", "MZ5 - M.Play MR700 (RC ONLY)", "LIS2 - VLSysytems LIS2 display" });
            this.cmbDeviceType.Location = new Point(0x59, 0x13);
            this.cmbDeviceType.Name = "cmbDeviceType";
            this.cmbDeviceType.Size = new Size(0xc5, 0x15);
            this.cmbDeviceType.TabIndex = 0x15;
            this.cmbDeviceType.SelectedIndexChanged += new EventHandler(this.cmbDeviceType_SelectedIndexChanged);
            this.cbManageMHC.AutoSize = true;
            this.cbManageMHC.Location = new Point(15, 0x2b);
            this.cbManageMHC.Name = "cbManageMHC";
            this.cbManageMHC.Size = new Size(150, 0x11);
            this.cbManageMHC.TabIndex = 8;
            this.cbManageMHC.Text = "Automatically control MHC";
            this.cbManageMHC.UseVisualStyleBackColor = true;
            this.groupBoxRemote.Controls.Add(this.lblDelay);
            this.groupBoxRemote.Controls.Add(this.tbDelay);
            this.groupBoxRemote.Controls.Add(this.cbDisableRepeat);
            this.groupBoxRemote.Controls.Add(this.cbNoRemote);
            this.groupBoxRemote.Location = new Point(10, 0x40);
            this.groupBoxRemote.Name = "groupBoxRemote";
            this.groupBoxRemote.Size = new Size(0x152, 90);
            this.groupBoxRemote.TabIndex = 7;
            this.groupBoxRemote.TabStop = false;
            this.groupBoxRemote.Text = " Remote Control Options ";
            this.lblDelay.Location = new Point(0xbc, 0x12);
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.Size = new Size(0x7e, 0x11);
            this.lblDelay.TabIndex = 0x18;
            this.lblDelay.Text = "Repeat Delay: 1000ms";
            this.lblDelay.TextAlign = ContentAlignment.MiddleCenter;
            this.lblDelay.Click += new EventHandler(this.lblDelay_Click);
            this.tbDelay.LargeChange = 1;
            this.tbDelay.Location = new Point(0xc7, 0x25);
            this.tbDelay.Maximum = 20;
            this.tbDelay.Name = "tbDelay";
            this.tbDelay.Size = new Size(0x68, 0x2d);
            this.tbDelay.TabIndex = 0x17;
            this.tbDelay.TickStyle = TickStyle.TopLeft;
            this.tbDelay.Scroll += new EventHandler(this.tbDelay_Scroll);
            this.cbDisableRepeat.AutoSize = true;
            this.cbDisableRepeat.Location = new Point(0x19, 0x25);
            this.cbDisableRepeat.Name = "cbDisableRepeat";
            this.cbDisableRepeat.Size = new Size(120, 0x11);
            this.cbDisableRepeat.TabIndex = 6;
            this.cbDisableRepeat.Text = "Disable Key Repeat";
            this.cbDisableRepeat.UseVisualStyleBackColor = true;
            this.cbDisableRepeat.CheckedChanged += new EventHandler(this.cbDisableRepeat_CheckedChanged);
            this.cbNoRemote.AutoSize = true;
            this.cbNoRemote.Location = new Point(5, 0x13);
            this.cbNoRemote.Name = "cbNoRemote";
            this.cbNoRemote.Size = new Size(0x89, 0x11);
            this.cbNoRemote.TabIndex = 5;
            this.cbNoRemote.Text = "Disable Remote Control";
            this.cbNoRemote.UseVisualStyleBackColor = true;
            this.cbNoRemote.CheckedChanged += new EventHandler(this.cbNoRemote_CheckedChanged);
            this.groupBoxFan.BackColor = SystemColors.Control;
            this.groupBoxFan.Controls.Add(this.cbFan1Auto);
            this.groupBoxFan.Controls.Add(this.groupBox3);
            this.groupBoxFan.Controls.Add(this.cbUseFans);
            this.groupBoxFan.Controls.Add(this.lblFan1);
            this.groupBoxFan.Controls.Add(this.tbFan1);
            this.groupBoxFan.Controls.Add(this.Fan1Box);
            this.groupBoxFan.Controls.Add(this.Fan2Box);
            this.groupBoxFan.Location = new Point(0x169, 11);
            this.groupBoxFan.Name = "groupBoxFan";
            this.groupBoxFan.Size = new Size(0xb0, 0x1e2);
            this.groupBoxFan.TabIndex = 6;
            this.groupBoxFan.TabStop = false;
            this.groupBoxFan.Text = " Fan Control Options ";
            this.cbFan1Auto.AutoSize = true;
            this.cbFan1Auto.Location = new Point(14, 0x74);
            this.cbFan1Auto.Name = "cbFan1Auto";
            this.cbFan1Auto.Size = new Size(0x83, 0x11);
            this.cbFan1Auto.TabIndex = 12;
            this.cbFan1Auto.Text = "Use Automatic Control";
            this.cbFan1Auto.UseVisualStyleBackColor = true;
            this.cbFan1Auto.CheckedChanged += new EventHandler(this.cbFan1Auto_CheckedChanged);
            this.groupBox3.Controls.Add(this.cbFan1ShutdownManual);
            this.groupBox3.Controls.Add(this.cbFan1_SetOn);
            this.groupBox3.Controls.Add(this.cbFan1_SetOff);
            this.groupBox3.Controls.Add(this.lblFan1_SetOn);
            this.groupBox3.Controls.Add(this.lblFan1_SetOff);
            this.groupBox3.Location = new Point(14, 0x88);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new Size(0x8e, 0x70);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Automatic Settings";
            this.cbFan1ShutdownManual.Location = new Point(11, 0x44);
            this.cbFan1ShutdownManual.Name = "cbFan1ShutdownManual";
            this.cbFan1ShutdownManual.Size = new Size(0x80, 0x22);
            this.cbFan1ShutdownManual.TabIndex = 0x16;
            this.cbFan1ShutdownManual.Text = "Use manual speed as shutdown speed";
            this.cbFan1ShutdownManual.UseVisualStyleBackColor = true;
            this.cbFan1ShutdownManual.CheckedChanged += new EventHandler(this.cbFan1ShutdownManual_CheckedChanged);
            this.cbFan1_SetOn.FormattingEnabled = true;
            this.cbFan1_SetOn.Items.AddRange(new object[] { 
                "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", 
                "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", 
                "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", 
                "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", 
                "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100"
             });
            this.cbFan1_SetOn.Location = new Point(0x4e, 0x29);
            this.cbFan1_SetOn.Name = "cbFan1_SetOn";
            this.cbFan1_SetOn.Size = new Size(0x2c, 0x15);
            this.cbFan1_SetOn.TabIndex = 0x15;
            this.cbFan1_SetOff.FormattingEnabled = true;
            this.cbFan1_SetOff.Items.AddRange(new object[] { 
                "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", 
                "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", 
                "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", 
                "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", 
                "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100"
             });
            this.cbFan1_SetOff.Location = new Point(0x4e, 0x11);
            this.cbFan1_SetOff.Name = "cbFan1_SetOff";
            this.cbFan1_SetOff.Size = new Size(0x2c, 0x15);
            this.cbFan1_SetOff.TabIndex = 20;
            this.lblFan1_SetOn.AutoSize = true;
            this.lblFan1_SetOn.Location = new Point(6, 0x2c);
            this.lblFan1_SetOn.Name = "lblFan1_SetOn";
            this.lblFan1_SetOn.Size = new Size(0x85, 13);
            this.lblFan1_SetOn.TabIndex = 14;
            this.lblFan1_SetOn.Text = "100% if above                \x00b0C";
            this.lblFan1_SetOff.AutoSize = true;
            this.lblFan1_SetOff.Location = new Point(8, 20);
            this.lblFan1_SetOff.Name = "lblFan1_SetOff";
            this.lblFan1_SetOff.Size = new Size(0x83, 13);
            this.lblFan1_SetOff.TabIndex = 13;
            this.lblFan1_SetOff.Text = "OFF if below                  \x00b0C";
            this.cbUseFans.AutoSize = true;
            this.cbUseFans.Location = new Point(5, 0x12);
            this.cbUseFans.Name = "cbUseFans";
            this.cbUseFans.Size = new Size(0x71, 0x11);
            this.cbUseFans.TabIndex = 10;
            this.cbUseFans.Text = "Use Fan Controller";
            this.cbUseFans.UseVisualStyleBackColor = true;
            this.cbUseFans.CheckedChanged += new EventHandler(this.cbUseFans_CheckedChanged);
            this.lblFan1.Location = new Point(0x27, 0x36);
            this.lblFan1.Name = "lblFan1";
            this.lblFan1.Size = new Size(90, 0x11);
            this.lblFan1.TabIndex = 8;
            this.lblFan1.Text = "Manual:  100%";
            this.lblFan1.TextAlign = ContentAlignment.MiddleCenter;
            this.tbFan1.LargeChange = 1;
            this.tbFan1.Location = new Point(0x1f, 70);
            this.tbFan1.Name = "tbFan1";
            this.tbFan1.Size = new Size(0x68, 0x2d);
            this.tbFan1.TabIndex = 7;
            this.tbFan1.TickStyle = TickStyle.Both;
            this.tbFan1.ValueChanged += new EventHandler(this.tbFan_Scroll);
            this.tbFan1.Scroll += new EventHandler(this.tbFan_Scroll);
            this.Fan1Box.Location = new Point(8, 0x26);
            this.Fan1Box.Name = "Fan1Box";
            this.Fan1Box.Size = new Size(0x9e, 0xd8);
            this.Fan1Box.TabIndex = 13;
            this.Fan1Box.TabStop = false;
            this.Fan1Box.Text = "Fan #1";
            this.Fan2Box.Controls.Add(this.cbFan2Auto);
            this.Fan2Box.Controls.Add(this.groupBox6);
            this.Fan2Box.Controls.Add(this.tbFan2);
            this.Fan2Box.Controls.Add(this.lblFan2);
            this.Fan2Box.Location = new Point(8, 260);
            this.Fan2Box.Name = "Fan2Box";
            this.Fan2Box.Size = new Size(0x9e, 0xd8);
            this.Fan2Box.TabIndex = 14;
            this.Fan2Box.TabStop = false;
            this.Fan2Box.Text = "Fan #2";
            this.cbFan2Auto.AutoSize = true;
            this.cbFan2Auto.Location = new Point(6, 0x4e);
            this.cbFan2Auto.Name = "cbFan2Auto";
            this.cbFan2Auto.Size = new Size(0x83, 0x11);
            this.cbFan2Auto.TabIndex = 13;
            this.cbFan2Auto.Text = "Use Automatic Control";
            this.cbFan2Auto.UseVisualStyleBackColor = true;
            this.cbFan2Auto.CheckedChanged += new EventHandler(this.cbFan2Auto_CheckedChanged);
            this.groupBox6.Controls.Add(this.cbFan2ShutdownManual);
            this.groupBox6.Controls.Add(this.cbFan2_SetOn);
            this.groupBox6.Controls.Add(this.cbFan2_SetOff);
            this.groupBox6.Controls.Add(this.lblFan2_SetOn);
            this.groupBox6.Controls.Add(this.lblFan2_SetOff);
            this.groupBox6.Location = new Point(8, 0x62);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new Size(0x8e, 0x70);
            this.groupBox6.TabIndex = 12;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Automatic Settings";
            this.cbFan2ShutdownManual.Location = new Point(11, 0x44);
            this.cbFan2ShutdownManual.Name = "cbFan2ShutdownManual";
            this.cbFan2ShutdownManual.Size = new Size(0x80, 0x22);
            this.cbFan2ShutdownManual.TabIndex = 0x17;
            this.cbFan2ShutdownManual.Text = "Use manual speed as shutdown speed";
            this.cbFan2ShutdownManual.UseVisualStyleBackColor = true;
            this.cbFan2ShutdownManual.CheckedChanged += new EventHandler(this.cbFan2ShutdownManual_CheckedChanged);
            this.cbFan2_SetOn.FormattingEnabled = true;
            this.cbFan2_SetOn.Items.AddRange(new object[] { 
                "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", 
                "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", 
                "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", 
                "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", 
                "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100"
             });
            this.cbFan2_SetOn.Location = new Point(0x4e, 0x29);
            this.cbFan2_SetOn.Name = "cbFan2_SetOn";
            this.cbFan2_SetOn.Size = new Size(0x2c, 0x15);
            this.cbFan2_SetOn.TabIndex = 0x15;
            this.cbFan2_SetOff.FormattingEnabled = true;
            this.cbFan2_SetOff.Items.AddRange(new object[] { 
                "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", 
                "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", 
                "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", 
                "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", 
                "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100"
             });
            this.cbFan2_SetOff.Location = new Point(0x4e, 0x11);
            this.cbFan2_SetOff.Name = "cbFan2_SetOff";
            this.cbFan2_SetOff.Size = new Size(0x2c, 0x15);
            this.cbFan2_SetOff.TabIndex = 20;
            this.lblFan2_SetOn.AutoSize = true;
            this.lblFan2_SetOn.Location = new Point(6, 0x2c);
            this.lblFan2_SetOn.Name = "lblFan2_SetOn";
            this.lblFan2_SetOn.Size = new Size(0x85, 13);
            this.lblFan2_SetOn.TabIndex = 14;
            this.lblFan2_SetOn.Text = "100% if above                \x00b0C";
            this.lblFan2_SetOff.AutoSize = true;
            this.lblFan2_SetOff.Location = new Point(8, 20);
            this.lblFan2_SetOff.Name = "lblFan2_SetOff";
            this.lblFan2_SetOff.Size = new Size(0x83, 13);
            this.lblFan2_SetOff.TabIndex = 13;
            this.lblFan2_SetOff.Text = "OFF if below                  \x00b0C";
            this.tbFan2.LargeChange = 1;
            this.tbFan2.Location = new Point(0x1a, 0x24);
            this.tbFan2.Name = "tbFan2";
            this.tbFan2.Size = new Size(0x68, 0x2d);
            this.tbFan2.TabIndex = 6;
            this.tbFan2.TickStyle = TickStyle.Both;
            this.tbFan2.ValueChanged += new EventHandler(this.tbFan_Scroll);
            this.tbFan2.Scroll += new EventHandler(this.tbFan_Scroll);
            this.lblFan2.Location = new Point(0x1f, 0x10);
            this.lblFan2.Name = "lblFan2";
            this.lblFan2.Size = new Size(90, 0x11);
            this.lblFan2.TabIndex = 9;
            this.lblFan2.Text = "Manual:  100%";
            this.lblFan2.TextAlign = ContentAlignment.MiddleCenter;
            this.btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnOK.Location = new Point(0x1d3, 0x1ff);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x58, 0x17);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.btnReset.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnReset.Location = new Point(370, 0x1ff);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new Size(0x58, 0x17);
            this.btnReset.TabIndex = 6;
            this.btnReset.Text = "&RESET";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new EventHandler(this.btnReset_Click);
            this.btnRemoteSetup.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnRemoteSetup.Location = new Point(12, 0x1ff);
            this.btnRemoteSetup.Name = "btnRemoteSetup";
            this.btnRemoteSetup.Size = new Size(0x6a, 0x17);
            this.btnRemoteSetup.TabIndex = 7;
            this.btnRemoteSetup.Text = "R&EMOTE SETUP";
            this.btnRemoteSetup.UseVisualStyleBackColor = true;
            this.btnRemoteSetup.Click += new EventHandler(this.btnRemoteSetup_Click);
            this.btnTest.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnTest.Enabled = false;
            this.btnTest.Location = new Point(0x7c, 0x1ff);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new Size(0x47, 0x17);
            this.btnTest.TabIndex = 8;
            this.btnTest.Text = "test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Visible = false;
            this.btnTest.Click += new EventHandler(this.btnTest_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x232, 0x21b);
            base.Controls.Add(this.btnTest);
            base.Controls.Add(this.btnRemoteSetup);
            base.Controls.Add(this.btnOK);
            base.Controls.Add(this.btnReset);
            base.Controls.Add(this.groupBox1);
            base.Name = "VLSYS_AdvancedSetupForm";
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Advanced Settings";
            base.Load += new EventHandler(this.VLSYS_AdvancedSetupForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxEqualizer.ResumeLayout(false);
            this.groupBoxEqualizer.PerformLayout();
            this.groupEQstyle.ResumeLayout(false);
            this.groupEQstyle.PerformLayout();
            this.groupBoxDisplay.ResumeLayout(false);
            this.groupBoxDisplay.PerformLayout();
            this.groupBoxRemote.ResumeLayout(false);
            this.groupBoxRemote.PerformLayout();
            this.tbDelay.EndInit();
            this.groupBoxFan.ResumeLayout(false);
            this.groupBoxFan.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tbFan1.EndInit();
            this.Fan2Box.ResumeLayout(false);
            this.Fan2Box.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.tbFan2.EndInit();
            base.ResumeLayout(false);
        }

        private void lblDelay_Click(object sender, EventArgs e)
        {
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

        private void mpEditIcon_Click(object sender, EventArgs e)
        {
            Form form = new iMONLCDg_IconEdit();
            base.Visible = false;
            form.ShowDialog();
            form.Dispose();
            base.Visible = true;
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
            lock (this.sUpdateMutex)
            {
                Log.Info("VLSYS_Mplay_AdvancedSetupForm.SetControlState(): cmbDeviceType.SelectedIndex = {0}", new object[] { this.cmbDeviceType.SelectedIndex });
                if (this.cmbDeviceType.SelectedIndex == 11)
                {
                    this.groupBoxFan.Enabled = false;
                    this.groupBoxRemote.Enabled = false;
                    this.groupBoxDisplay.Enabled = true;
                    this.groupBoxEqualizer.Enabled = true;
                }
                else if (this.cmbDeviceType.SelectedIndex == 10)
                {
                    this.groupBoxFan.Enabled = false;
                    this.groupBoxRemote.Enabled = true;
                    this.groupBoxDisplay.Enabled = false;
                    this.groupBoxEqualizer.Enabled = false;
                }
                else
                {
                    this.groupBoxFan.Enabled = true;
                    this.groupBoxRemote.Enabled = true;
                    this.groupBoxDisplay.Enabled = true;
                    this.groupBoxEqualizer.Enabled = true;
                }
                if (this.groupBoxRemote.Enabled)
                {
                    if (this.cbNoRemote.Checked)
                    {
                        this.cbDisableRepeat.Enabled = false;
                        this.lblDelay.Enabled = false;
                        this.tbDelay.Enabled = false;
                    }
                    else
                    {
                        this.cbDisableRepeat.Enabled = true;
                        if (this.cbDisableRepeat.Checked)
                        {
                            this.lblDelay.Enabled = false;
                            this.tbDelay.Enabled = false;
                        }
                        else
                        {
                            this.tbDelay.Enabled = true;
                            this.lblDelay.Enabled = true;
                        }
                    }
                }
                if (this.groupBoxFan.Enabled)
                {
                    if (this.cbUseFans.Checked)
                    {
                        this.cbFan1Auto.Enabled = true;
                        if (this.cbFan1Auto.Checked)
                        {
                            this.lblFan1.Enabled = false;
                            this.tbFan1.Enabled = false;
                            this.cbFan1_SetOff.Enabled = true;
                            this.lblFan1_SetOff.Enabled = true;
                            this.cbFan1_SetOn.Enabled = true;
                            this.lblFan1_SetOn.Enabled = true;
                            this.cbFan1ShutdownManual.Enabled = true;
                            if (this.cbFan1ShutdownManual.Checked)
                            {
                                this.lblFan1.Enabled = true;
                                this.tbFan1.Enabled = true;
                            }
                        }
                        else
                        {
                            this.lblFan1.Enabled = true;
                            this.tbFan1.Enabled = true;
                            this.cbFan1_SetOff.Enabled = false;
                            this.lblFan1_SetOff.Enabled = false;
                            this.cbFan1_SetOn.Enabled = false;
                            this.lblFan1_SetOn.Enabled = false;
                            this.cbFan1ShutdownManual.Enabled = false;
                        }
                        this.cbFan2Auto.Enabled = true;
                        if (this.cbFan2Auto.Checked)
                        {
                            this.lblFan2.Enabled = false;
                            this.tbFan2.Enabled = false;
                            this.cbFan2_SetOff.Enabled = true;
                            this.lblFan2_SetOff.Enabled = true;
                            this.cbFan2_SetOn.Enabled = true;
                            this.lblFan2_SetOn.Enabled = true;
                            this.cbFan2ShutdownManual.Enabled = true;
                            if (this.cbFan2ShutdownManual.Checked)
                            {
                                this.lblFan2.Enabled = true;
                                this.tbFan2.Enabled = true;
                            }
                        }
                        else
                        {
                            this.lblFan2.Enabled = true;
                            this.tbFan2.Enabled = true;
                            this.cbFan2_SetOff.Enabled = false;
                            this.lblFan2_SetOff.Enabled = false;
                            this.cbFan2_SetOn.Enabled = false;
                            this.lblFan2_SetOn.Enabled = false;
                            this.cbFan2ShutdownManual.Enabled = false;
                        }
                    }
                    else
                    {
                        this.lblFan1.Enabled = false;
                        this.tbFan1.Enabled = false;
                        this.cbFan1Auto.Enabled = false;
                        this.cbFan1_SetOff.Enabled = false;
                        this.lblFan1_SetOff.Enabled = false;
                        this.cbFan1_SetOn.Enabled = false;
                        this.lblFan1_SetOn.Enabled = false;
                        this.cbFan1ShutdownManual.Enabled = false;
                        this.lblFan2.Enabled = false;
                        this.tbFan2.Enabled = false;
                        this.cbFan2Auto.Enabled = false;
                        this.cbFan2_SetOff.Enabled = false;
                        this.lblFan2_SetOff.Enabled = false;
                        this.cbFan2_SetOn.Enabled = false;
                        this.lblFan2_SetOn.Enabled = false;
                        this.cbFan2ShutdownManual.Enabled = false;
                    }
                }
                if (this.groupBoxEqualizer.Enabled)
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
                }
                if (this.groupBoxDisplay.Enabled)
                {
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

        private void SetDelayLabel()
        {
            this.lblDelay.Text = "Repeat Delay: " + ((this.tbDelay.Value * 0x19)).ToString() + "ms";
        }

        private void SetFanLabel(object sender)
        {
            TrackBar bar;
            Label label;
            bool flag = false;
            if (((TrackBar) sender) == this.tbFan1)
            {
                bar = this.tbFan1;
                label = this.lblFan1;
                flag = this.cbFan1Auto.Checked && this.cbFan1ShutdownManual.Checked;
            }
            else
            {
                bar = this.tbFan2;
                label = this.lblFan2;
                flag = this.cbFan2Auto.Checked && this.cbFan2ShutdownManual.Checked;
            }
            string str = string.Empty;
            switch (bar.Value)
            {
                case 0:
                    str = "0%";
                    break;

                case 1:
                    str = "10%";
                    break;

                case 2:
                    str = "20%";
                    break;

                case 3:
                    str = "30%";
                    break;

                case 4:
                    str = "40%";
                    break;

                case 5:
                    str = "50%";
                    break;

                case 6:
                    str = "60%";
                    break;

                case 7:
                    str = "70%";
                    break;

                case 8:
                    str = "80%";
                    break;

                case 9:
                    str = "90%";
                    break;

                case 10:
                    str = "100%";
                    break;
            }
            if (flag)
            {
                label.Text = "Shutdown: " + str;
            }
            else
            {
                label.Text = "Manual: " + str;
            }
        }

        private void tbDelay_Scroll(object sender, EventArgs e)
        {
            this.SetDelayLabel();
        }

        private void tbFan_Scroll(object sender, EventArgs e)
        {
            this.SetFanLabel(sender);
        }

        private void tbFan_ValueChanged(object sender, EventArgs e)
        {
            this.SetFanLabel(sender);
        }

        private void VLSYS_AdvancedSetupForm_Load(object sender, EventArgs e)
        {
            this.cbFan1_SetOff.SelectedIndexChanged += new EventHandler(this.cbFan1_SetOff_SelectedIndexChanged);
            this.cbFan1_SetOn.SelectedIndexChanged += new EventHandler(this.cbFan1_SetOn_SelectedIndexChanged);
            this.cbFan2_SetOff.SelectedIndexChanged += new EventHandler(this.cbFan2_SetOff_SelectedIndexChanged);
            this.cbFan2_SetOn.SelectedIndexChanged += new EventHandler(this.cbFan2_SetOn_SelectedIndexChanged);
            this.SetFanLabel(this.tbFan1);
            this.SetFanLabel(this.tbFan2);
            this.SetDelayLabel();
            this._FormLoaded = true;
        }
    }
}

