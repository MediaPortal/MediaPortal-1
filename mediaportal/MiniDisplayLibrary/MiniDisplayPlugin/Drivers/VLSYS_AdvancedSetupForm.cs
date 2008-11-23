using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class VLSYS_AdvancedSetupForm : MediaPortal.UserInterface.Controls.MPConfigForm
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
    private readonly IContainer components = null;
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
      } catch (Exception exception)
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
      } catch (Exception exception)
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBoxEqualizer = new System.Windows.Forms.GroupBox();
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
      this.groupBoxDisplay = new System.Windows.Forms.GroupBox();
      this.cbUseClockOnShutdown = new System.Windows.Forms.CheckBox();
      this.mpEnableDisplayActionTime = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cmbBlankIdleTime = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpEnableDisplayAction = new System.Windows.Forms.CheckBox();
      this.mpBlankDisplayWithVideo = new System.Windows.Forms.CheckBox();
      this.mpBlankDisplayWhenIdle = new System.Windows.Forms.CheckBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cmbDeviceType = new System.Windows.Forms.ComboBox();
      this.cbManageMHC = new System.Windows.Forms.CheckBox();
      this.groupBoxRemote = new System.Windows.Forms.GroupBox();
      this.lblDelay = new System.Windows.Forms.Label();
      this.tbDelay = new System.Windows.Forms.TrackBar();
      this.cbDisableRepeat = new System.Windows.Forms.CheckBox();
      this.cbNoRemote = new System.Windows.Forms.CheckBox();
      this.groupBoxFan = new System.Windows.Forms.GroupBox();
      this.cbFan1Auto = new System.Windows.Forms.CheckBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.cbFan1ShutdownManual = new System.Windows.Forms.CheckBox();
      this.cbFan1_SetOn = new System.Windows.Forms.ComboBox();
      this.cbFan1_SetOff = new System.Windows.Forms.ComboBox();
      this.lblFan1_SetOn = new System.Windows.Forms.Label();
      this.lblFan1_SetOff = new System.Windows.Forms.Label();
      this.cbUseFans = new System.Windows.Forms.CheckBox();
      this.lblFan1 = new System.Windows.Forms.Label();
      this.tbFan1 = new System.Windows.Forms.TrackBar();
      this.Fan1Box = new System.Windows.Forms.GroupBox();
      this.Fan2Box = new System.Windows.Forms.GroupBox();
      this.cbFan2Auto = new System.Windows.Forms.CheckBox();
      this.groupBox6 = new System.Windows.Forms.GroupBox();
      this.cbFan2ShutdownManual = new System.Windows.Forms.CheckBox();
      this.cbFan2_SetOn = new System.Windows.Forms.ComboBox();
      this.cbFan2_SetOff = new System.Windows.Forms.ComboBox();
      this.lblFan2_SetOn = new System.Windows.Forms.Label();
      this.lblFan2_SetOff = new System.Windows.Forms.Label();
      this.tbFan2 = new System.Windows.Forms.TrackBar();
      this.lblFan2 = new System.Windows.Forms.Label();
      this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnReset = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnRemoteSetup = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnTest = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1.SuspendLayout();
      this.groupBoxEqualizer.SuspendLayout();
      this.groupEQstyle.SuspendLayout();
      this.groupBoxDisplay.SuspendLayout();
      this.groupBoxRemote.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbDelay)).BeginInit();
      this.groupBoxFan.SuspendLayout();
      this.groupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbFan1)).BeginInit();
      this.Fan2Box.SuspendLayout();
      this.groupBox6.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbFan2)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.groupBoxEqualizer);
      this.groupBox1.Controls.Add(this.groupBoxDisplay);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.cmbDeviceType);
      this.groupBox1.Controls.Add(this.cbManageMHC);
      this.groupBox1.Controls.Add(this.groupBoxRemote);
      this.groupBox1.Controls.Add(this.groupBoxFan);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(9, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(543, 499);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "VLSYS_Mplay Configuration";
      // 
      // groupBoxEqualizer
      // 
      this.groupBoxEqualizer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
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
      this.groupBoxEqualizer.Location = new System.Drawing.Point(10, 260);
      this.groupBoxEqualizer.Name = "groupBoxEqualizer";
      this.groupBoxEqualizer.Size = new System.Drawing.Size(338, 233);
      this.groupBoxEqualizer.TabIndex = 103;
      this.groupBoxEqualizer.TabStop = false;
      this.groupBoxEqualizer.Text = " Equalizer Options";
      // 
      // groupEQstyle
      // 
      this.groupEQstyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
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
      this.cbUseVUmeter2.Location = new System.Drawing.Point(212, 17);
      this.cbUseVUmeter2.Name = "cbUseVUmeter2";
      this.cbUseVUmeter2.Size = new System.Drawing.Size(79, 17);
      this.cbUseVUmeter2.TabIndex = 121;
      this.cbUseVUmeter2.Text = "VU Meter 2";
      this.cbUseVUmeter2.UseVisualStyleBackColor = true;
      this.cbUseVUmeter2.CheckedChanged += new System.EventHandler(this.cbUseVuMeter2_CheckedChanged);
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
      // cbUseVUmeter
      // 
      this.cbUseVUmeter.AutoSize = true;
      this.cbUseVUmeter.Location = new System.Drawing.Point(135, 17);
      this.cbUseVUmeter.Name = "cbUseVUmeter";
      this.cbUseVUmeter.Size = new System.Drawing.Size(70, 17);
      this.cbUseVUmeter.TabIndex = 2;
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
      this.cbNormalEQ.Checked = true;
      this.cbNormalEQ.Location = new System.Drawing.Point(13, 17);
      this.cbNormalEQ.Name = "cbNormalEQ";
      this.cbNormalEQ.Size = new System.Drawing.Size(58, 17);
      this.cbNormalEQ.TabIndex = 0;
      this.cbNormalEQ.TabStop = true;
      this.cbNormalEQ.Text = "Normal";
      this.cbNormalEQ.UseVisualStyleBackColor = true;
      this.cbNormalEQ.CheckedChanged += new System.EventHandler(this.cbNormalEQ_CheckedChanged);
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
      this.cmbDelayEqTime.Location = new System.Drawing.Point(160, 143);
      this.cmbDelayEqTime.Name = "cmbDelayEqTime";
      this.cmbDelayEqTime.Size = new System.Drawing.Size(52, 21);
      this.cmbDelayEqTime.TabIndex = 104;
      // 
      // lblRestrictEQ
      // 
      this.lblRestrictEQ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.lblRestrictEQ.Location = new System.Drawing.Point(100, 124);
      this.lblRestrictEQ.Name = "lblRestrictEQ";
      this.lblRestrictEQ.Size = new System.Drawing.Size(116, 17);
      this.lblRestrictEQ.TabIndex = 115;
      this.lblRestrictEQ.Text = "updates per Seconds";
      this.lblRestrictEQ.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
      this.cmbEQTitleDisplayTime.Location = new System.Drawing.Point(165, 207);
      this.cmbEQTitleDisplayTime.Name = "cmbEQTitleDisplayTime";
      this.cmbEQTitleDisplayTime.Size = new System.Drawing.Size(52, 21);
      this.cmbEQTitleDisplayTime.TabIndex = 110;
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
      // groupBoxDisplay
      // 
      this.groupBoxDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxDisplay.Controls.Add(this.cbUseClockOnShutdown);
      this.groupBoxDisplay.Controls.Add(this.mpEnableDisplayActionTime);
      this.groupBoxDisplay.Controls.Add(this.cmbBlankIdleTime);
      this.groupBoxDisplay.Controls.Add(this.mpEnableDisplayAction);
      this.groupBoxDisplay.Controls.Add(this.mpBlankDisplayWithVideo);
      this.groupBoxDisplay.Controls.Add(this.mpBlankDisplayWhenIdle);
      this.groupBoxDisplay.Location = new System.Drawing.Point(10, 156);
      this.groupBoxDisplay.Name = "groupBoxDisplay";
      this.groupBoxDisplay.Size = new System.Drawing.Size(338, 103);
      this.groupBoxDisplay.TabIndex = 23;
      this.groupBoxDisplay.TabStop = false;
      this.groupBoxDisplay.Text = " Display Control Options ";
      // 
      // cbUseClockOnShutdown
      // 
      this.cbUseClockOnShutdown.AutoSize = true;
      this.cbUseClockOnShutdown.Location = new System.Drawing.Point(8, 81);
      this.cbUseClockOnShutdown.Name = "cbUseClockOnShutdown";
      this.cbUseClockOnShutdown.Size = new System.Drawing.Size(204, 17);
      this.cbUseClockOnShutdown.TabIndex = 100;
      this.cbUseClockOnShutdown.Text = "Use Clock on shutdown (If supported)";
      this.cbUseClockOnShutdown.UseVisualStyleBackColor = true;
      // 
      // mpEnableDisplayActionTime
      // 
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
      this.mpEnableDisplayActionTime.Location = new System.Drawing.Point(181, 36);
      this.mpEnableDisplayActionTime.Name = "mpEnableDisplayActionTime";
      this.mpEnableDisplayActionTime.Size = new System.Drawing.Size(42, 21);
      this.mpEnableDisplayActionTime.TabIndex = 96;
      // 
      // cmbBlankIdleTime
      // 
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
      this.cmbBlankIdleTime.Location = new System.Drawing.Point(167, 58);
      this.cmbBlankIdleTime.Name = "cmbBlankIdleTime";
      this.cmbBlankIdleTime.Size = new System.Drawing.Size(42, 21);
      this.cmbBlankIdleTime.TabIndex = 98;
      // 
      // mpEnableDisplayAction
      // 
      this.mpEnableDisplayAction.AutoSize = true;
      this.mpEnableDisplayAction.Location = new System.Drawing.Point(23, 38);
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
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(15, 22);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(68, 13);
      this.label3.TabIndex = 22;
      this.label3.Text = "Device Type";
      // 
      // cmbDeviceType
      // 
      this.cmbDeviceType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cmbDeviceType.FormattingEnabled = true;
      this.cmbDeviceType.Items.AddRange(new object[] {
            "AUTOMATIC",
            "LE2 - M.Play 202",
            "ME4 - M.Play 202 Plus",
            "ME5 - M.Play 202 Plus R2",
            "ME7 - M.Play MR300 / M.Play Blast",
            "MP4 - M.Play 202",
            "MP5 - M.Play 202 Plus",
            "MP7 - M.Play Blast",
            "MR2 - M.Play Blast",
            "MZ4 - M.Play MR300",
            "MZ5 - M.Play MR700 (RC ONLY)",
            "LIS2 - VLSysytems LIS2 display"});
      this.cmbDeviceType.Location = new System.Drawing.Point(89, 19);
      this.cmbDeviceType.Name = "cmbDeviceType";
      this.cmbDeviceType.Size = new System.Drawing.Size(197, 21);
      this.cmbDeviceType.TabIndex = 21;
      this.cmbDeviceType.SelectedIndexChanged += new System.EventHandler(this.cmbDeviceType_SelectedIndexChanged);
      // 
      // cbManageMHC
      // 
      this.cbManageMHC.AutoSize = true;
      this.cbManageMHC.Location = new System.Drawing.Point(15, 43);
      this.cbManageMHC.Name = "cbManageMHC";
      this.cbManageMHC.Size = new System.Drawing.Size(150, 17);
      this.cbManageMHC.TabIndex = 8;
      this.cbManageMHC.Text = "Automatically control MHC";
      this.cbManageMHC.UseVisualStyleBackColor = true;
      // 
      // groupBoxRemote
      // 
      this.groupBoxRemote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxRemote.Controls.Add(this.lblDelay);
      this.groupBoxRemote.Controls.Add(this.tbDelay);
      this.groupBoxRemote.Controls.Add(this.cbDisableRepeat);
      this.groupBoxRemote.Controls.Add(this.cbNoRemote);
      this.groupBoxRemote.Location = new System.Drawing.Point(10, 64);
      this.groupBoxRemote.Name = "groupBoxRemote";
      this.groupBoxRemote.Size = new System.Drawing.Size(338, 90);
      this.groupBoxRemote.TabIndex = 7;
      this.groupBoxRemote.TabStop = false;
      this.groupBoxRemote.Text = " Remote Control Options ";
      // 
      // lblDelay
      // 
      this.lblDelay.Location = new System.Drawing.Point(188, 18);
      this.lblDelay.Name = "lblDelay";
      this.lblDelay.Size = new System.Drawing.Size(126, 17);
      this.lblDelay.TabIndex = 24;
      this.lblDelay.Text = "Repeat Delay: 1000ms";
      this.lblDelay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.lblDelay.Click += new System.EventHandler(this.lblDelay_Click);
      // 
      // tbDelay
      // 
      this.tbDelay.LargeChange = 1;
      this.tbDelay.Location = new System.Drawing.Point(199, 37);
      this.tbDelay.Maximum = 20;
      this.tbDelay.Name = "tbDelay";
      this.tbDelay.Size = new System.Drawing.Size(104, 45);
      this.tbDelay.TabIndex = 23;
      this.tbDelay.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
      this.tbDelay.Scroll += new System.EventHandler(this.tbDelay_Scroll);
      // 
      // cbDisableRepeat
      // 
      this.cbDisableRepeat.AutoSize = true;
      this.cbDisableRepeat.Location = new System.Drawing.Point(25, 37);
      this.cbDisableRepeat.Name = "cbDisableRepeat";
      this.cbDisableRepeat.Size = new System.Drawing.Size(120, 17);
      this.cbDisableRepeat.TabIndex = 6;
      this.cbDisableRepeat.Text = "Disable Key Repeat";
      this.cbDisableRepeat.UseVisualStyleBackColor = true;
      this.cbDisableRepeat.CheckedChanged += new System.EventHandler(this.cbDisableRepeat_CheckedChanged);
      // 
      // cbNoRemote
      // 
      this.cbNoRemote.AutoSize = true;
      this.cbNoRemote.Location = new System.Drawing.Point(5, 19);
      this.cbNoRemote.Name = "cbNoRemote";
      this.cbNoRemote.Size = new System.Drawing.Size(137, 17);
      this.cbNoRemote.TabIndex = 5;
      this.cbNoRemote.Text = "Disable Remote Control";
      this.cbNoRemote.UseVisualStyleBackColor = true;
      this.cbNoRemote.CheckedChanged += new System.EventHandler(this.cbNoRemote_CheckedChanged);
      // 
      // groupBoxFan
      // 
      this.groupBoxFan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxFan.BackColor = System.Drawing.SystemColors.Control;
      this.groupBoxFan.Controls.Add(this.cbFan1Auto);
      this.groupBoxFan.Controls.Add(this.groupBox3);
      this.groupBoxFan.Controls.Add(this.cbUseFans);
      this.groupBoxFan.Controls.Add(this.lblFan1);
      this.groupBoxFan.Controls.Add(this.tbFan1);
      this.groupBoxFan.Controls.Add(this.Fan1Box);
      this.groupBoxFan.Controls.Add(this.Fan2Box);
      this.groupBoxFan.Location = new System.Drawing.Point(361, 11);
      this.groupBoxFan.Name = "groupBoxFan";
      this.groupBoxFan.Size = new System.Drawing.Size(176, 482);
      this.groupBoxFan.TabIndex = 6;
      this.groupBoxFan.TabStop = false;
      this.groupBoxFan.Text = " Fan Control Options ";
      // 
      // cbFan1Auto
      // 
      this.cbFan1Auto.AutoSize = true;
      this.cbFan1Auto.Location = new System.Drawing.Point(14, 116);
      this.cbFan1Auto.Name = "cbFan1Auto";
      this.cbFan1Auto.Size = new System.Drawing.Size(131, 17);
      this.cbFan1Auto.TabIndex = 12;
      this.cbFan1Auto.Text = "Use Automatic Control";
      this.cbFan1Auto.UseVisualStyleBackColor = true;
      this.cbFan1Auto.CheckedChanged += new System.EventHandler(this.cbFan1Auto_CheckedChanged);
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.cbFan1ShutdownManual);
      this.groupBox3.Controls.Add(this.cbFan1_SetOn);
      this.groupBox3.Controls.Add(this.cbFan1_SetOff);
      this.groupBox3.Controls.Add(this.lblFan1_SetOn);
      this.groupBox3.Controls.Add(this.lblFan1_SetOff);
      this.groupBox3.Location = new System.Drawing.Point(14, 136);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(142, 112);
      this.groupBox3.TabIndex = 11;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Automatic Settings";
      // 
      // cbFan1ShutdownManual
      // 
      this.cbFan1ShutdownManual.Location = new System.Drawing.Point(11, 68);
      this.cbFan1ShutdownManual.Name = "cbFan1ShutdownManual";
      this.cbFan1ShutdownManual.Size = new System.Drawing.Size(128, 34);
      this.cbFan1ShutdownManual.TabIndex = 22;
      this.cbFan1ShutdownManual.Text = "Use manual speed as shutdown speed";
      this.cbFan1ShutdownManual.UseVisualStyleBackColor = true;
      this.cbFan1ShutdownManual.CheckedChanged += new System.EventHandler(this.cbFan1ShutdownManual_CheckedChanged);
      // 
      // cbFan1_SetOn
      // 
      this.cbFan1_SetOn.FormattingEnabled = true;
      this.cbFan1_SetOn.Items.AddRange(new object[] {
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
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100"});
      this.cbFan1_SetOn.Location = new System.Drawing.Point(78, 41);
      this.cbFan1_SetOn.Name = "cbFan1_SetOn";
      this.cbFan1_SetOn.Size = new System.Drawing.Size(44, 21);
      this.cbFan1_SetOn.TabIndex = 21;
      // 
      // cbFan1_SetOff
      // 
      this.cbFan1_SetOff.FormattingEnabled = true;
      this.cbFan1_SetOff.Items.AddRange(new object[] {
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
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100"});
      this.cbFan1_SetOff.Location = new System.Drawing.Point(78, 17);
      this.cbFan1_SetOff.Name = "cbFan1_SetOff";
      this.cbFan1_SetOff.Size = new System.Drawing.Size(44, 21);
      this.cbFan1_SetOff.TabIndex = 20;
      // 
      // lblFan1_SetOn
      // 
      this.lblFan1_SetOn.AutoSize = true;
      this.lblFan1_SetOn.Location = new System.Drawing.Point(6, 44);
      this.lblFan1_SetOn.Name = "lblFan1_SetOn";
      this.lblFan1_SetOn.Size = new System.Drawing.Size(133, 13);
      this.lblFan1_SetOn.TabIndex = 14;
      this.lblFan1_SetOn.Text = "100% if above                °C";
      // 
      // lblFan1_SetOff
      // 
      this.lblFan1_SetOff.AutoSize = true;
      this.lblFan1_SetOff.Location = new System.Drawing.Point(8, 20);
      this.lblFan1_SetOff.Name = "lblFan1_SetOff";
      this.lblFan1_SetOff.Size = new System.Drawing.Size(131, 13);
      this.lblFan1_SetOff.TabIndex = 13;
      this.lblFan1_SetOff.Text = "OFF if below                  °C";
      // 
      // cbUseFans
      // 
      this.cbUseFans.AutoSize = true;
      this.cbUseFans.Location = new System.Drawing.Point(5, 18);
      this.cbUseFans.Name = "cbUseFans";
      this.cbUseFans.Size = new System.Drawing.Size(113, 17);
      this.cbUseFans.TabIndex = 10;
      this.cbUseFans.Text = "Use Fan Controller";
      this.cbUseFans.UseVisualStyleBackColor = true;
      this.cbUseFans.CheckedChanged += new System.EventHandler(this.cbUseFans_CheckedChanged);
      // 
      // lblFan1
      // 
      this.lblFan1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblFan1.Location = new System.Drawing.Point(39, 54);
      this.lblFan1.Name = "lblFan1";
      this.lblFan1.Size = new System.Drawing.Size(90, 17);
      this.lblFan1.TabIndex = 8;
      this.lblFan1.Text = "Manual:  100%";
      this.lblFan1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // tbFan1
      // 
      this.tbFan1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbFan1.LargeChange = 1;
      this.tbFan1.Location = new System.Drawing.Point(31, 70);
      this.tbFan1.Name = "tbFan1";
      this.tbFan1.Size = new System.Drawing.Size(104, 45);
      this.tbFan1.TabIndex = 7;
      this.tbFan1.TickStyle = System.Windows.Forms.TickStyle.Both;
      this.tbFan1.ValueChanged += new System.EventHandler(this.tbFan_Scroll);
      this.tbFan1.Scroll += new System.EventHandler(this.tbFan_Scroll);
      // 
      // Fan1Box
      // 
      this.Fan1Box.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.Fan1Box.Location = new System.Drawing.Point(8, 38);
      this.Fan1Box.Name = "Fan1Box";
      this.Fan1Box.Size = new System.Drawing.Size(158, 216);
      this.Fan1Box.TabIndex = 13;
      this.Fan1Box.TabStop = false;
      this.Fan1Box.Text = "Fan #1";
      // 
      // Fan2Box
      // 
      this.Fan2Box.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.Fan2Box.Controls.Add(this.cbFan2Auto);
      this.Fan2Box.Controls.Add(this.groupBox6);
      this.Fan2Box.Controls.Add(this.tbFan2);
      this.Fan2Box.Controls.Add(this.lblFan2);
      this.Fan2Box.Location = new System.Drawing.Point(8, 260);
      this.Fan2Box.Name = "Fan2Box";
      this.Fan2Box.Size = new System.Drawing.Size(158, 216);
      this.Fan2Box.TabIndex = 14;
      this.Fan2Box.TabStop = false;
      this.Fan2Box.Text = "Fan #2";
      // 
      // cbFan2Auto
      // 
      this.cbFan2Auto.AutoSize = true;
      this.cbFan2Auto.Location = new System.Drawing.Point(6, 78);
      this.cbFan2Auto.Name = "cbFan2Auto";
      this.cbFan2Auto.Size = new System.Drawing.Size(131, 17);
      this.cbFan2Auto.TabIndex = 13;
      this.cbFan2Auto.Text = "Use Automatic Control";
      this.cbFan2Auto.UseVisualStyleBackColor = true;
      this.cbFan2Auto.CheckedChanged += new System.EventHandler(this.cbFan2Auto_CheckedChanged);
      // 
      // groupBox6
      // 
      this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox6.Controls.Add(this.cbFan2ShutdownManual);
      this.groupBox6.Controls.Add(this.cbFan2_SetOn);
      this.groupBox6.Controls.Add(this.cbFan2_SetOff);
      this.groupBox6.Controls.Add(this.lblFan2_SetOn);
      this.groupBox6.Controls.Add(this.lblFan2_SetOff);
      this.groupBox6.Location = new System.Drawing.Point(8, 98);
      this.groupBox6.Name = "groupBox6";
      this.groupBox6.Size = new System.Drawing.Size(142, 112);
      this.groupBox6.TabIndex = 12;
      this.groupBox6.TabStop = false;
      this.groupBox6.Text = "Automatic Settings";
      // 
      // cbFan2ShutdownManual
      // 
      this.cbFan2ShutdownManual.Location = new System.Drawing.Point(11, 68);
      this.cbFan2ShutdownManual.Name = "cbFan2ShutdownManual";
      this.cbFan2ShutdownManual.Size = new System.Drawing.Size(128, 34);
      this.cbFan2ShutdownManual.TabIndex = 23;
      this.cbFan2ShutdownManual.Text = "Use manual speed as shutdown speed";
      this.cbFan2ShutdownManual.UseVisualStyleBackColor = true;
      this.cbFan2ShutdownManual.CheckedChanged += new System.EventHandler(this.cbFan2ShutdownManual_CheckedChanged);
      // 
      // cbFan2_SetOn
      // 
      this.cbFan2_SetOn.FormattingEnabled = true;
      this.cbFan2_SetOn.Items.AddRange(new object[] {
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
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100"});
      this.cbFan2_SetOn.Location = new System.Drawing.Point(78, 41);
      this.cbFan2_SetOn.Name = "cbFan2_SetOn";
      this.cbFan2_SetOn.Size = new System.Drawing.Size(44, 21);
      this.cbFan2_SetOn.TabIndex = 21;
      // 
      // cbFan2_SetOff
      // 
      this.cbFan2_SetOff.FormattingEnabled = true;
      this.cbFan2_SetOff.Items.AddRange(new object[] {
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
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100"});
      this.cbFan2_SetOff.Location = new System.Drawing.Point(78, 17);
      this.cbFan2_SetOff.Name = "cbFan2_SetOff";
      this.cbFan2_SetOff.Size = new System.Drawing.Size(44, 21);
      this.cbFan2_SetOff.TabIndex = 20;
      // 
      // lblFan2_SetOn
      // 
      this.lblFan2_SetOn.AutoSize = true;
      this.lblFan2_SetOn.Location = new System.Drawing.Point(6, 44);
      this.lblFan2_SetOn.Name = "lblFan2_SetOn";
      this.lblFan2_SetOn.Size = new System.Drawing.Size(133, 13);
      this.lblFan2_SetOn.TabIndex = 14;
      this.lblFan2_SetOn.Text = "100% if above                °C";
      // 
      // lblFan2_SetOff
      // 
      this.lblFan2_SetOff.AutoSize = true;
      this.lblFan2_SetOff.Location = new System.Drawing.Point(8, 20);
      this.lblFan2_SetOff.Name = "lblFan2_SetOff";
      this.lblFan2_SetOff.Size = new System.Drawing.Size(131, 13);
      this.lblFan2_SetOff.TabIndex = 13;
      this.lblFan2_SetOff.Text = "OFF if below                  °C";
      // 
      // tbFan2
      // 
      this.tbFan2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbFan2.LargeChange = 1;
      this.tbFan2.Location = new System.Drawing.Point(26, 36);
      this.tbFan2.Name = "tbFan2";
      this.tbFan2.Size = new System.Drawing.Size(104, 45);
      this.tbFan2.TabIndex = 6;
      this.tbFan2.TickStyle = System.Windows.Forms.TickStyle.Both;
      this.tbFan2.ValueChanged += new System.EventHandler(this.tbFan_Scroll);
      this.tbFan2.Scroll += new System.EventHandler(this.tbFan_Scroll);
      // 
      // lblFan2
      // 
      this.lblFan2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblFan2.Location = new System.Drawing.Point(31, 16);
      this.lblFan2.Name = "lblFan2";
      this.lblFan2.Size = new System.Drawing.Size(90, 17);
      this.lblFan2.TabIndex = 9;
      this.lblFan2.Text = "Manual:  100%";
      this.lblFan2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(467, 511);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(88, 23);
      this.btnOK.TabIndex = 6;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnReset
      // 
      this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnReset.Location = new System.Drawing.Point(370, 511);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new System.Drawing.Size(88, 23);
      this.btnReset.TabIndex = 6;
      this.btnReset.Text = "&RESET";
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // btnRemoteSetup
      // 
      this.btnRemoteSetup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnRemoteSetup.Location = new System.Drawing.Point(12, 511);
      this.btnRemoteSetup.Name = "btnRemoteSetup";
      this.btnRemoteSetup.Size = new System.Drawing.Size(106, 23);
      this.btnRemoteSetup.TabIndex = 7;
      this.btnRemoteSetup.Text = "R&EMOTE SETUP";
      this.btnRemoteSetup.UseVisualStyleBackColor = true;
      this.btnRemoteSetup.Click += new System.EventHandler(this.btnRemoteSetup_Click);
      // 
      // btnTest
      // 
      this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnTest.Enabled = false;
      this.btnTest.Location = new System.Drawing.Point(124, 511);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new System.Drawing.Size(71, 23);
      this.btnTest.TabIndex = 8;
      this.btnTest.Text = "test";
      this.btnTest.UseVisualStyleBackColor = true;
      this.btnTest.Visible = false;
      this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // VLSYS_AdvancedSetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(562, 539);
      this.Controls.Add(this.btnTest);
      this.Controls.Add(this.btnRemoteSetup);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnReset);
      this.Controls.Add(this.groupBox1);
      this.Name = "VLSYS_AdvancedSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MiniDisplay - Setup - Advanced Settings";
      this.Load += new System.EventHandler(this.VLSYS_AdvancedSetupForm_Load);
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
      ((System.ComponentModel.ISupportInitialize)(this.tbDelay)).EndInit();
      this.groupBoxFan.ResumeLayout(false);
      this.groupBoxFan.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbFan1)).EndInit();
      this.Fan2Box.ResumeLayout(false);
      this.Fan2Box.PerformLayout();
      this.groupBox6.ResumeLayout(false);
      this.groupBox6.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbFan2)).EndInit();
      this.ResumeLayout(false);

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
      if (((TrackBar)sender) == this.tbFan1)
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

