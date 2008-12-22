using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class iMONLCDg_AdvancedSetupForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    private MPButton btnOK;
    private MPButton btnReset;
    private MPCheckBox cbDisableRepeat;
    private MPComboBox cbRemoteType;
    private MPCheckBox cbUseRC;
    private CheckBox cbVUindicators;
    private MPCheckBox ckDeviceMonitor;
    private MPCheckBox ckDiskIcon;
    private MPCheckBox ckDiskMediaStatus;
    private MPComboBox cmbBlankIdleTime;
    private MPComboBox cmbDelayEqTime;
    private MPComboBox cmbEqMode;
    private MPComboBox cmbEqRate;
    private MPComboBox cmbEQTitleDisplayTime;
    private MPComboBox cmbEQTitleShowTime;
    private MPComboBox cmbType;
    private readonly IContainer components = null;
    private MPGroupBox groupBoxConfiguration;
    private GroupBox groupBoxDisplayControl;
    private GroupBox groupBoxDisplayOptions;
    private GroupBox groupboxEqualizerOptions;
    private GroupBox groupBoxHardware;
    private GroupBox groupBoxRemoteControl;
    private GroupBox groupEQstyle;
    private MPLabel label1;
    private Label lblDelay;
    private MPLabel lblRemoteType;
    private MPCheckBox mpBlankDisplayWhenIdle;
    private MPCheckBox mpBlankDisplayWithVideo;
    private MPCheckBox mpDelayEQ;
    private MPCheckBox mpDelayStartup;
    private MPButton mpEditFont;
    private MPButton mpEditIcon;
    private MPCheckBox mpEnableDisplayAction;
    private MPComboBox mpEnableDisplayActionTime;
    private MPCheckBox mpEnsureManagerStartup;
    private MPCheckBox mpEqDisplay;
    private MPCheckBox mpEQTitleDisplay;
    private MPCheckBox mpForceKeyBoardMode;
    private MPCheckBox mpForceManagerRestart;
    private MPLabel mpLabel2;
    private MPLabel mpLabelEQmode;
    private MPLabel mpLabelEQTitleDisplay;
    private MPCheckBox mpMonitorPowerState;
    private RadioButton mpNormalEQ;
    private MPCheckBox mpProgressBar;
    private MPCheckBox mpRestartFrontview;
    private MPCheckBox mpRestrictEQ;
    private MPCheckBox mpSmoothEQ;
    private MPCheckBox mpUseCustomFont;
    private MPCheckBox mpUseCustomIcons;
    private MPCheckBox mpUseInvertedIcons;
    private MPCheckBox mpUseLargeIcons;
    private RadioButton mpUseStereoEQ;
    private RadioButton mpUseVUmeter;
    private RadioButton mpUseVUmeter2;
    private MPCheckBox mpVFD_UseV3DLL;
    private MPCheckBox mpVolumeDisplay;
    private GroupBox groupBoxManager;
    private GroupBox groupBox1;
    private MPCheckBox mpForceManagerReload;
    private TrackBar tbDelay;

    public iMONLCDg_AdvancedSetupForm()
    {
      Log.Debug("iMONLCDg.AdvancedSetupForm(): Constructor started", new object[0]);
      this.InitializeComponent();
      this.cmbType.SelectedIndex = 0;
      this.cmbType.DataBindings.Add("SelectedItem", iMONLCDg.AdvancedSettings.Instance, "DisplayType");
      this.mpVolumeDisplay.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "VolumeDisplay");
      this.mpEqDisplay.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "EqDisplay");
      this.cmbEqMode.SelectedIndex = 0;
      this.cmbEqMode.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "EqMode");
      this.mpRestrictEQ.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "RestrictEQ");
      this.mpProgressBar.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "ProgressDisplay");
      this.ckDiskIcon.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DiskIcon");
      this.ckDiskMediaStatus.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DiskMediaStatus");
      this.ckDeviceMonitor.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DeviceMonitor");
      this.mpUseCustomFont.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "UseCustomFont");
      this.mpUseLargeIcons.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "UseLargeIcons");
      this.mpUseCustomIcons.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "UseCustomIcons");
      this.mpUseInvertedIcons.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "UseInvertedIcons");
      this.mpBlankDisplayWithVideo.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "BlankDisplayWithVideo");
      this.mpEnableDisplayAction.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "EnableDisplayAction");
      this.mpEnableDisplayActionTime.SelectedIndex = 0;
      this.mpEnableDisplayActionTime.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "EnableDisplayActionTime");
      this.mpVFD_UseV3DLL.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "VFD_UseV3DLL");
      this.cmbEqRate.SelectedIndex = 0;
      this.cmbEqRate.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "EqRate");
      this.mpDelayEQ.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DelayEQ");
      this.cmbDelayEqTime.SelectedIndex = 0;
      this.cmbDelayEqTime.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "DelayEqTime");
      this.mpMonitorPowerState.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "MonitorPowerState");
      this.mpSmoothEQ.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "SmoothEQ");
      this.mpEQTitleDisplay.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "EQTitleDisplay");
      this.cmbEQTitleDisplayTime.SelectedIndex = 0;
      this.cmbEQTitleDisplayTime.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "EQTitleDisplayTime");
      this.cmbEQTitleShowTime.SelectedIndex = 0;
      this.cmbEQTitleShowTime.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "EQTitleShowTime");
      this.mpBlankDisplayWhenIdle.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "BlankDisplayWhenIdle");
      this.cmbBlankIdleTime.SelectedIndex = 0;
      this.cmbBlankIdleTime.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "BlankIdleTime");
      if (iMONLCDg.AdvancedSettings.Instance.NormalEQ)
      {
        this.mpNormalEQ.Checked = true;
      }
      else if (iMONLCDg.AdvancedSettings.Instance.StereoEQ)
      {
        this.mpUseStereoEQ.Checked = true;
      }
      else if (iMONLCDg.AdvancedSettings.Instance.VUmeter)
      {
        this.mpUseVUmeter.Checked = true;
      }
      else
      {
        this.mpUseVUmeter2.Checked = true;
      }
      this.cbVUindicators.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "VUindicators");
      this.mpDelayStartup.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DelayStartup");
      this.mpEnsureManagerStartup.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "EnsureManagerStartup");
      this.mpForceManagerRestart.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "ForceManagerRestart");
      this.mpForceManagerReload.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "ForceManagerReload");
      this.mpRestartFrontview.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "RestartFrontviewOnExit");
      this.mpForceKeyBoardMode.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "ForceKeyBoardMode");
      this.cbUseRC.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "UseRC");
      this.cbRemoteType.SelectedIndex = 0;
      this.cbRemoteType.DataBindings.Add("SelectedItem", iMONLCDg.AdvancedSettings.Instance, "RemoteType");
      this.cbDisableRepeat.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DisableRepeat");
      this.tbDelay.DataBindings.Add("Value", iMONLCDg.AdvancedSettings.Instance, "RepeatDelay");
      if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "iMon_Remote.xml")))
      {
        iMONLCDg.AdvancedSettings.CreateDefaultRemoteMapping();
      }
      this.cmbType_Changed(this, EventArgs.Empty);
      Log.Debug("iMONLCDg.AdvancedSetupForm(): Constructor completed", new object[0]);
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Log.Debug("iMONLCDg.AdvancedSetupForm.btnOK_Click(): started", new object[0]);
      if (this.mpNormalEQ.Checked)
      {
        iMONLCDg.AdvancedSettings.Instance.NormalEQ = true;
        iMONLCDg.AdvancedSettings.Instance.StereoEQ = false;
        iMONLCDg.AdvancedSettings.Instance.VUmeter = false;
        iMONLCDg.AdvancedSettings.Instance.VUmeter2 = false;
      }
      else if (this.mpUseStereoEQ.Checked)
      {
        iMONLCDg.AdvancedSettings.Instance.NormalEQ = false;
        iMONLCDg.AdvancedSettings.Instance.StereoEQ = true;
        iMONLCDg.AdvancedSettings.Instance.VUmeter = false;
        iMONLCDg.AdvancedSettings.Instance.VUmeter2 = false;
      }
      else if (this.mpUseVUmeter.Checked)
      {
        iMONLCDg.AdvancedSettings.Instance.NormalEQ = false;
        iMONLCDg.AdvancedSettings.Instance.StereoEQ = false;
        iMONLCDg.AdvancedSettings.Instance.VUmeter = true;
        iMONLCDg.AdvancedSettings.Instance.VUmeter2 = false;
      }
      else
      {
        iMONLCDg.AdvancedSettings.Instance.NormalEQ = false;
        iMONLCDg.AdvancedSettings.Instance.StereoEQ = false;
        iMONLCDg.AdvancedSettings.Instance.VUmeter = false;
        iMONLCDg.AdvancedSettings.Instance.VUmeter2 = true;
      }
      iMONLCDg.AdvancedSettings.Save();
      base.Hide();
      base.Close();
      Log.Debug("iMONLCDg.AdvancedSetupForm.btnOK_Click(): Completed", new object[0]);
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      Log.Debug("iMONLCDg.AdvancedSetupForm.btnReset_Click(): started", new object[0]);
      this.mpDelayStartup.Checked = false;
      this.mpEnsureManagerStartup.Checked = false;
      this.mpForceManagerRestart.Checked = false;
      this.mpForceManagerReload.Checked = false;
      this.mpRestartFrontview.Checked = false;
      this.mpForceKeyBoardMode.Checked = false;
      this.cmbType.SelectedIndex = 0;
      this.mpVFD_UseV3DLL.Checked = false;
      this.ckDiskIcon.Checked = false;
      this.ckDiskMediaStatus.Checked = false;
      this.ckDeviceMonitor.Checked = false;
      this.mpVolumeDisplay.Checked = false;
      this.mpProgressBar.Checked = false;
      this.mpUseCustomFont.Checked = false;
      this.mpUseCustomIcons.Checked = false;
      this.mpUseInvertedIcons.Checked = false;
      this.mpUseLargeIcons.Checked = false;
      this.mpEqDisplay.Checked = false;
      this.cbVUindicators.Checked = false;
      this.cmbEqMode.SelectedIndex = 0;
      this.mpRestrictEQ.Checked = false;
      this.cmbEqRate.SelectedIndex = 10;
      this.mpDelayEQ.Checked = false;
      this.cmbDelayEqTime.SelectedIndex = 10;
      this.mpBlankDisplayWithVideo.Checked = false;
      this.mpMonitorPowerState.Checked = false;
      this.mpSmoothEQ.Enabled = false;
      this.mpEQTitleDisplay.Checked = false;
      this.cmbEQTitleDisplayTime.SelectedIndex = 10;
      this.cmbEQTitleShowTime.SelectedIndex = 2;
      if (iMONLCDg.AdvancedSettings.Instance.NormalEQ)
      {
        this.mpNormalEQ.Checked = true;
      }
      else if (iMONLCDg.AdvancedSettings.Instance.StereoEQ)
      {
        this.mpUseStereoEQ.Checked = true;
      }
      else if (iMONLCDg.AdvancedSettings.Instance.VUmeter)
      {
        this.mpUseVUmeter.Checked = true;
      }
      else
      {
        this.mpUseVUmeter2.Checked = true;
      }
      this.cbUseRC.Checked = false;
      this.cbRemoteType.SelectedItem = "MCE";
      this.cbDisableRepeat.Checked = false;
      this.tbDelay.Value = 4;
      iMONLCDg.AdvancedSettings.SetDefaults();
      this.cmbType_Changed(this, EventArgs.Empty);
      this.Refresh();
      Log.Debug("iMONLCDg.AdvancedSetupForm.btnReset_Click(): Completed", new object[0]);
    }

    private void cbDisableRepeat_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void cbUseRC_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void ckDiskIcon_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void cmbEQTitleDisplayTime_SelectedValueChanged(object sender, EventArgs e)
    {
      if (this.cmbEQTitleDisplayTime.SelectedIndex < this.cmbEQTitleShowTime.SelectedIndex)
      {
        this.cmbEQTitleShowTime.SelectedIndex = this.cmbEQTitleDisplayTime.SelectedIndex;
      }
    }

    private void cmbEQTitleShowTime_SelectedValueChanged(object sender, EventArgs e)
    {
      if (this.cmbEQTitleShowTime.SelectedIndex > this.cmbEQTitleDisplayTime.SelectedIndex)
      {
        this.cmbEQTitleDisplayTime.SelectedIndex = this.cmbEQTitleShowTime.SelectedIndex;
      }
    }

    private void cmbType_Changed(object sender, EventArgs e)
    {
      this.SetDelayLabel();
      if (this.cbUseRC.Checked)
      {
        this.mpEnsureManagerStartup.Enabled = false;
        this.mpForceManagerRestart.Enabled = false;
        this.mpForceManagerReload.Enabled = false;
        this.cbRemoteType.Enabled = true;
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
      else
      {
        this.mpEnsureManagerStartup.Enabled = true;
        this.mpForceManagerRestart.Enabled = true;
        this.mpForceManagerReload.Enabled = true;
        this.cbRemoteType.Enabled = false;
        this.cbDisableRepeat.Enabled = false;
        this.lblDelay.Enabled = false;
        this.tbDelay.Enabled = false;
      }
      if (this.mpVFD_UseV3DLL.Checked)
      {
        this.cmbType.Enabled = false;
      }
      else
      {
        this.cmbType.Enabled = true;
      }
      if (this.cmbType.SelectedItem.ToString() == "AutoDetect")
      {
        Log.Debug("iMONLCDg.AdvancedSetupForm.cmbType_changed: Enabling Advanced Setup options for AutoDetect", new object[0]);
        this.mpBlankDisplayWhenIdle.Enabled = true;
        if (this.mpBlankDisplayWhenIdle.Checked)
        {
          this.cmbBlankIdleTime.Enabled = true;
        }
        else
        {
          this.cmbBlankIdleTime.Enabled = false;
        }
        this.mpBlankDisplayWithVideo.Enabled = true;
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
        this.mpProgressBar.Enabled = true;
        this.mpUseCustomFont.Enabled = true;
        if (this.mpUseCustomFont.Checked)
        {
          this.mpEditFont.Enabled = true;
        }
        else
        {
          this.mpEditFont.Enabled = false;
        }
        this.mpVFD_UseV3DLL.Enabled = true;
        this.mpVolumeDisplay.Enabled = true;
        this.ckDiskIcon.Enabled = true;
        if (this.ckDiskIcon.Checked)
        {
          this.ckDiskMediaStatus.Enabled = true;
          this.ckDeviceMonitor.Enabled = true;
        }
        else
        {
          this.ckDiskMediaStatus.Enabled = false;
          this.ckDeviceMonitor.Enabled = false;
        }
        this.mpUseLargeIcons.Enabled = true;
        if (this.mpUseLargeIcons.Checked)
        {
          this.mpUseCustomIcons.Enabled = true;
          if (this.mpUseCustomIcons.Checked)
          {
            this.mpEditIcon.Enabled = true;
          }
          else
          {
            this.mpEditIcon.Enabled = false;
          }
          this.mpUseInvertedIcons.Enabled = true;
        }
        else
        {
          this.mpUseCustomIcons.Enabled = false;
          this.mpUseInvertedIcons.Enabled = false;
          this.mpEditIcon.Enabled = false;
        }
        this.mpEqDisplay.Enabled = true;
        if (this.mpEqDisplay.Checked)
        {
          this.cmbEqMode.Enabled = true;
          this.mpLabelEQmode.Enabled = true;
          this.groupEQstyle.Enabled = true;
          this.mpUseVUmeter.Enabled = true;
          this.mpUseVUmeter2.Enabled = true;
          this.cbVUindicators.Enabled = false;
          if (this.mpUseVUmeter.Checked || this.mpUseVUmeter2.Checked)
          {
            this.cbVUindicators.Enabled = true;
          }
          this.mpRestrictEQ.Enabled = true;
          if (this.mpRestrictEQ.Checked)
          {
            this.cmbEqRate.Enabled = true;
          }
          else
          {
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
          this.mpLabelEQTitleDisplay.Enabled = true;
          if (this.mpEQTitleDisplay.Checked)
          {
            this.cmbEQTitleDisplayTime.Enabled = true;
            this.cmbEQTitleShowTime.Enabled = true;
          }
          else
          {
            this.cmbEQTitleDisplayTime.Enabled = false;
            this.cmbEQTitleShowTime.Enabled = false;
          }
        }
        else
        {
          this.cmbEqMode.Enabled = false;
          this.mpLabelEQmode.Enabled = false;
          this.groupEQstyle.Enabled = false;
          this.mpRestrictEQ.Enabled = false;
          this.cmbEqRate.Enabled = false;
          this.mpDelayEQ.Enabled = false;
          this.cmbDelayEqTime.Enabled = false;
          this.mpSmoothEQ.Enabled = false;
          this.mpEQTitleDisplay.Enabled = false;
          this.mpLabelEQTitleDisplay.Enabled = false;
          this.cmbEQTitleDisplayTime.Enabled = false;
          this.cmbEQTitleShowTime.Enabled = false;
        }
      }
      else if ((this.cmbType.SelectedItem.ToString() == "LCD") || (this.cmbType.SelectedItem.ToString() == "LCD2"))
      {
        Log.Debug("iMONLCDg.AdvancedSetupForm.cmbType_changed: Enabling Advanced Setup options for LCD/LCD2", new object[0]);
        this.mpBlankDisplayWhenIdle.Enabled = true;
        if (this.mpBlankDisplayWhenIdle.Checked)
        {
          this.cmbBlankIdleTime.Enabled = true;
        }
        else
        {
          this.cmbBlankIdleTime.Enabled = false;
        }
        this.mpBlankDisplayWithVideo.Enabled = true;
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
        this.mpProgressBar.Enabled = true;
        this.mpUseCustomFont.Enabled = true;
        if (this.mpUseCustomFont.Checked)
        {
          this.mpEditFont.Enabled = true;
        }
        else
        {
          this.mpEditFont.Enabled = false;
        }
        this.mpVolumeDisplay.Enabled = true;
        this.ckDiskIcon.Enabled = true;
        if (this.ckDiskIcon.Checked)
        {
          this.ckDiskMediaStatus.Enabled = true;
          this.ckDeviceMonitor.Enabled = true;
        }
        else
        {
          this.ckDiskMediaStatus.Enabled = false;
          this.ckDeviceMonitor.Enabled = false;
        }
        this.mpUseLargeIcons.Enabled = true;
        if (this.mpUseLargeIcons.Checked)
        {
          this.mpUseCustomIcons.Enabled = true;
          if (this.mpUseCustomIcons.Checked)
          {
            this.mpEditIcon.Enabled = true;
          }
          else
          {
            this.mpEditIcon.Enabled = false;
          }
          this.mpUseInvertedIcons.Enabled = true;
        }
        else
        {
          this.mpUseCustomIcons.Enabled = false;
          this.mpEditIcon.Enabled = false;
          this.mpUseInvertedIcons.Enabled = false;
        }
        this.mpEqDisplay.Enabled = true;
        if (this.mpEqDisplay.Checked)
        {
          this.groupEQstyle.Enabled = true;
          this.mpUseVUmeter.Enabled = true;
          this.mpUseVUmeter2.Enabled = true;
          this.cbVUindicators.Enabled = false;
          if (this.mpUseVUmeter.Checked || this.mpUseVUmeter2.Checked)
          {
            this.cbVUindicators.Enabled = true;
          }
          this.cmbEqMode.Enabled = true;
          this.mpLabelEQmode.Enabled = true;
          this.mpRestrictEQ.Enabled = true;
          if (this.mpRestrictEQ.Checked)
          {
            this.cmbEqRate.Enabled = true;
          }
          else
          {
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
          this.mpLabelEQTitleDisplay.Enabled = true;
          if (this.mpEQTitleDisplay.Checked)
          {
            this.cmbEQTitleDisplayTime.Enabled = true;
            this.cmbEQTitleShowTime.Enabled = true;
          }
          else
          {
            this.cmbEQTitleDisplayTime.Enabled = false;
            this.cmbEQTitleShowTime.Enabled = false;
          }
        }
        else
        {
          this.mpLabelEQmode.Enabled = false;
          this.groupEQstyle.Enabled = false;
          this.cmbEqMode.Enabled = false;
          this.mpRestrictEQ.Enabled = false;
          this.cmbEqRate.Enabled = false;
          this.mpDelayEQ.Enabled = false;
          this.cmbDelayEqTime.Enabled = false;
          this.mpSmoothEQ.Enabled = false;
          this.mpEQTitleDisplay.Enabled = false;
          this.mpLabelEQTitleDisplay.Enabled = false;
          this.cmbEQTitleDisplayTime.Enabled = false;
          this.cmbEQTitleShowTime.Enabled = false;
        }
      }
      else if (this.cmbType.SelectedItem.ToString() == "VFD")
      {
        Log.Debug("iMONLCDg.AdvancedSetupForm.cmbType_changed: Enabling Advanced Setup options for VFD", new object[0]);
        this.mpBlankDisplayWhenIdle.Enabled = true;
        if (this.mpBlankDisplayWhenIdle.Checked)
        {
          this.cmbBlankIdleTime.Enabled = true;
        }
        else
        {
          this.cmbBlankIdleTime.Enabled = false;
        }
        this.mpBlankDisplayWithVideo.Enabled = true;
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
        this.mpEqDisplay.Enabled = true;
        if (this.mpEqDisplay.Checked)
        {
          this.groupEQstyle.Enabled = true;
          this.mpUseVUmeter.Enabled = true;
          this.mpUseVUmeter2.Enabled = true;
          this.cbVUindicators.Enabled = false;
          if (this.mpUseVUmeter.Checked || this.mpUseVUmeter2.Checked)
          {
            this.cbVUindicators.Enabled = true;
          }
          this.cmbEqMode.SelectedIndex = 0;
          this.cmbEqMode.Enabled = false;
          this.mpLabelEQmode.Enabled = false;
          this.mpRestrictEQ.Enabled = true;
          if (this.mpRestrictEQ.Checked)
          {
            this.cmbEqRate.Enabled = true;
          }
          else
          {
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
          this.mpLabelEQTitleDisplay.Enabled = true;
          if (this.mpEQTitleDisplay.Checked)
          {
            this.cmbEQTitleDisplayTime.Enabled = true;
            this.cmbEQTitleShowTime.Enabled = true;
          }
          else
          {
            this.cmbEQTitleDisplayTime.Enabled = false;
            this.cmbEQTitleShowTime.Enabled = false;
          }
        }
        else
        {
          this.groupEQstyle.Enabled = false;
          this.cmbEqMode.SelectedIndex = 0;
          this.cmbEqMode.Enabled = false;
          this.mpLabelEQmode.Enabled = true;
          this.mpRestrictEQ.Enabled = false;
          this.cmbEqRate.Enabled = false;
          this.mpDelayEQ.Enabled = false;
          this.cmbDelayEqTime.Enabled = false;
          this.mpSmoothEQ.Enabled = false;
          this.mpEQTitleDisplay.Enabled = false;
          this.mpLabelEQTitleDisplay.Enabled = false;
          this.cmbEQTitleDisplayTime.Enabled = false;
          this.cmbEQTitleShowTime.Enabled = false;
        }
        this.mpProgressBar.Enabled = false;
        this.mpUseCustomFont.Enabled = false;
        this.mpEditFont.Enabled = false;
        this.mpUseCustomIcons.Enabled = false;
        this.mpEditIcon.Enabled = false;
        this.mpUseInvertedIcons.Enabled = false;
        this.mpUseLargeIcons.Enabled = false;
        this.mpVolumeDisplay.Enabled = false;
        this.ckDiskIcon.Enabled = false;
        this.ckDiskMediaStatus.Enabled = false;
        this.ckDeviceMonitor.Enabled = false;
      }
      else if (this.cmbType.SelectedItem.ToString() == "LCD3R")
      {
        Log.Debug("iMONLCDg.AdvancedSetupForm.cmbType_changed: Enabling Advanced Setup options for LCD3R", new object[0]);
        this.mpBlankDisplayWhenIdle.Enabled = true;
        if (this.mpBlankDisplayWhenIdle.Checked)
        {
          this.cmbBlankIdleTime.Enabled = true;
        }
        else
        {
          this.cmbBlankIdleTime.Enabled = false;
        }
        this.mpBlankDisplayWithVideo.Enabled = true;
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
        this.mpEqDisplay.Enabled = true;
        if (this.mpEqDisplay.Checked)
        {
          this.groupEQstyle.Enabled = true;
          this.cbVUindicators.Enabled = false;
          this.mpUseVUmeter.Enabled = false;
          this.mpUseVUmeter2.Enabled = false;
          this.cbVUindicators.Enabled = true;
          this.cmbEqMode.SelectedIndex = 0;
          this.cmbEqMode.Enabled = false;
          this.mpLabelEQmode.Enabled = false;
          this.mpRestrictEQ.Enabled = true;
          if (this.mpRestrictEQ.Checked)
          {
            this.cmbEqRate.Enabled = true;
          }
          else
          {
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
          this.mpLabelEQTitleDisplay.Enabled = true;
          if (this.mpEQTitleDisplay.Checked)
          {
            this.cmbEQTitleDisplayTime.Enabled = true;
            this.cmbEQTitleShowTime.Enabled = true;
          }
          else
          {
            this.cmbEQTitleDisplayTime.Enabled = false;
            this.cmbEQTitleShowTime.Enabled = false;
          }
        }
        else
        {
          this.groupEQstyle.Enabled = false;
          this.cmbEqMode.SelectedIndex = 0;
          this.cmbEqMode.Enabled = false;
          this.mpLabelEQmode.Enabled = true;
          this.mpRestrictEQ.Enabled = false;
          this.cmbEqRate.Enabled = false;
          this.mpDelayEQ.Enabled = false;
          this.cmbDelayEqTime.Enabled = false;
          this.mpSmoothEQ.Enabled = false;
          this.mpEQTitleDisplay.Enabled = false;
          this.mpLabelEQTitleDisplay.Enabled = false;
          this.cmbEQTitleDisplayTime.Enabled = false;
          this.cmbEQTitleShowTime.Enabled = false;
        }
        this.mpProgressBar.Enabled = false;
        this.mpUseCustomFont.Enabled = false;
        this.mpEditFont.Enabled = false;
        this.mpUseCustomIcons.Enabled = false;
        this.mpEditIcon.Enabled = false;
        this.mpUseInvertedIcons.Enabled = false;
        this.mpUseLargeIcons.Enabled = false;
        this.mpVolumeDisplay.Enabled = false;
        this.ckDiskIcon.Enabled = false;
        this.ckDiskMediaStatus.Enabled = false;
        this.ckDeviceMonitor.Enabled = false;
      }
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
      this.groupBoxConfiguration = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.mpMonitorPowerState = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxManager = new System.Windows.Forms.GroupBox();
      this.mpForceManagerReload = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpRestartFrontview = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpForceKeyBoardMode = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpForceManagerRestart = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpEnsureManagerStartup = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxHardware = new System.Windows.Forms.GroupBox();
      this.mpDelayStartup = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cmbType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpVFD_UseV3DLL = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxRemoteControl = new System.Windows.Forms.GroupBox();
      this.lblRemoteType = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbRemoteType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lblDelay = new System.Windows.Forms.Label();
      this.tbDelay = new System.Windows.Forms.TrackBar();
      this.cbDisableRepeat = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbUseRC = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxDisplayControl = new System.Windows.Forms.GroupBox();
      this.cmbBlankIdleTime = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpEnableDisplayActionTime = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpEnableDisplayAction = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpBlankDisplayWithVideo = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpBlankDisplayWhenIdle = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupboxEqualizerOptions = new System.Windows.Forms.GroupBox();
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
      this.groupBoxDisplayOptions = new System.Windows.Forms.GroupBox();
      this.ckDiskIcon = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.ckDiskMediaStatus = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.ckDeviceMonitor = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpVolumeDisplay = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpProgressBar = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpEditFont = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpEditIcon = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpUseCustomFont = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpUseLargeIcons = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpUseCustomIcons = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpUseInvertedIcons = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnReset = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxConfiguration.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBoxManager.SuspendLayout();
      this.groupBoxHardware.SuspendLayout();
      this.groupBoxRemoteControl.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbDelay)).BeginInit();
      this.groupBoxDisplayControl.SuspendLayout();
      this.groupboxEqualizerOptions.SuspendLayout();
      this.groupEQstyle.SuspendLayout();
      this.groupBoxDisplayOptions.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxConfiguration
      // 
      this.groupBoxConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxConfiguration.Controls.Add(this.groupBox1);
      this.groupBoxConfiguration.Controls.Add(this.groupBoxManager);
      this.groupBoxConfiguration.Controls.Add(this.groupBoxHardware);
      this.groupBoxConfiguration.Controls.Add(this.groupBoxRemoteControl);
      this.groupBoxConfiguration.Controls.Add(this.groupBoxDisplayControl);
      this.groupBoxConfiguration.Controls.Add(this.groupboxEqualizerOptions);
      this.groupBoxConfiguration.Controls.Add(this.groupBoxDisplayOptions);
      this.groupBoxConfiguration.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxConfiguration.Location = new System.Drawing.Point(9, 6);
      this.groupBoxConfiguration.Name = "groupBoxConfiguration";
      this.groupBoxConfiguration.Size = new System.Drawing.Size(632, 464);
      this.groupBoxConfiguration.TabIndex = 4;
      this.groupBoxConfiguration.TabStop = false;
      this.groupBoxConfiguration.Text = "Configuration";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.mpMonitorPowerState);
      this.groupBox1.Location = new System.Drawing.Point(10, 232);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(305, 30);
      this.groupBox1.TabIndex = 134;
      this.groupBox1.TabStop = false;
      // 
      // mpMonitorPowerState
      // 
      this.mpMonitorPowerState.AutoSize = true;
      this.mpMonitorPowerState.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpMonitorPowerState.Location = new System.Drawing.Point(13, 9);
      this.mpMonitorPowerState.Name = "mpMonitorPowerState";
      this.mpMonitorPowerState.Size = new System.Drawing.Size(153, 17);
      this.mpMonitorPowerState.TabIndex = 80;
      this.mpMonitorPowerState.Text = "Monitor PowerState Events";
      this.mpMonitorPowerState.UseVisualStyleBackColor = true;
      // 
      // groupBoxManager
      // 
      this.groupBoxManager.Controls.Add(this.mpForceManagerReload);
      this.groupBoxManager.Controls.Add(this.mpRestartFrontview);
      this.groupBoxManager.Controls.Add(this.mpForceKeyBoardMode);
      this.groupBoxManager.Controls.Add(this.mpForceManagerRestart);
      this.groupBoxManager.Controls.Add(this.mpEnsureManagerStartup);
      this.groupBoxManager.Location = new System.Drawing.Point(9, 116);
      this.groupBoxManager.Name = "groupBoxManager";
      this.groupBoxManager.Size = new System.Drawing.Size(306, 115);
      this.groupBoxManager.TabIndex = 133;
      this.groupBoxManager.TabStop = false;
      this.groupBoxManager.Text = "Antec/iMON Manager";
      // 
      // mpForceManagerReload
      // 
      this.mpForceManagerReload.AutoSize = true;
      this.mpForceManagerReload.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpForceManagerReload.Location = new System.Drawing.Point(14, 53);
      this.mpForceManagerReload.Name = "mpForceManagerReload";
      this.mpForceManagerReload.Size = new System.Drawing.Size(167, 17);
      this.mpForceManagerReload.TabIndex = 130;
      this.mpForceManagerReload.Text = "Force reload during driver start";
      this.mpForceManagerReload.UseVisualStyleBackColor = true;
      // 
      // mpRestartFrontview
      // 
      this.mpRestartFrontview.AutoSize = true;
      this.mpRestartFrontview.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRestartFrontview.Location = new System.Drawing.Point(14, 73);
      this.mpRestartFrontview.Name = "mpRestartFrontview";
      this.mpRestartFrontview.Size = new System.Drawing.Size(142, 17);
      this.mpRestartFrontview.TabIndex = 128;
      this.mpRestartFrontview.Text = "Restart FrontView on exit";
      this.mpRestartFrontview.UseVisualStyleBackColor = true;
      // 
      // mpForceKeyBoardMode
      // 
      this.mpForceKeyBoardMode.AutoSize = true;
      this.mpForceKeyBoardMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpForceKeyBoardMode.Location = new System.Drawing.Point(14, 93);
      this.mpForceKeyBoardMode.Name = "mpForceKeyBoardMode";
      this.mpForceKeyBoardMode.Size = new System.Drawing.Size(235, 17);
      this.mpForceKeyBoardMode.TabIndex = 129;
      this.mpForceKeyBoardMode.Text = "Force to use KeyBoard mode with iMON Pad";
      this.mpForceKeyBoardMode.UseVisualStyleBackColor = true;
      // 
      // mpForceManagerRestart
      // 
      this.mpForceManagerRestart.AutoSize = true;
      this.mpForceManagerRestart.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpForceManagerRestart.Location = new System.Drawing.Point(14, 33);
      this.mpForceManagerRestart.Name = "mpForceManagerRestart";
      this.mpForceManagerRestart.Size = new System.Drawing.Size(159, 17);
      this.mpForceManagerRestart.TabIndex = 127;
      this.mpForceManagerRestart.Text = "Force restart after driver start";
      this.mpForceManagerRestart.UseVisualStyleBackColor = true;
      // 
      // mpEnsureManagerStartup
      // 
      this.mpEnsureManagerStartup.AutoSize = true;
      this.mpEnsureManagerStartup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpEnsureManagerStartup.Location = new System.Drawing.Point(14, 13);
      this.mpEnsureManagerStartup.Name = "mpEnsureManagerStartup";
      this.mpEnsureManagerStartup.Size = new System.Drawing.Size(190, 17);
      this.mpEnsureManagerStartup.TabIndex = 126;
      this.mpEnsureManagerStartup.Text = "Ensure is running before driver start";
      this.mpEnsureManagerStartup.UseVisualStyleBackColor = true;
      // 
      // groupBoxHardware
      // 
      this.groupBoxHardware.Controls.Add(this.mpDelayStartup);
      this.groupBoxHardware.Controls.Add(this.label1);
      this.groupBoxHardware.Controls.Add(this.cmbType);
      this.groupBoxHardware.Controls.Add(this.mpVFD_UseV3DLL);
      this.groupBoxHardware.Location = new System.Drawing.Point(10, 15);
      this.groupBoxHardware.Name = "groupBoxHardware";
      this.groupBoxHardware.Size = new System.Drawing.Size(306, 95);
      this.groupBoxHardware.TabIndex = 132;
      this.groupBoxHardware.TabStop = false;
      this.groupBoxHardware.Text = " Hardware Options ";
      // 
      // mpDelayStartup
      // 
      this.mpDelayStartup.AutoSize = true;
      this.mpDelayStartup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpDelayStartup.Location = new System.Drawing.Point(13, 44);
      this.mpDelayStartup.Name = "mpDelayStartup";
      this.mpDelayStartup.Size = new System.Drawing.Size(265, 17);
      this.mpDelayStartup.TabIndex = 126;
      this.mpDelayStartup.Text = "Delay driver initialization (Problematic USB devices)";
      this.mpDelayStartup.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 18);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(73, 23);
      this.label1.TabIndex = 13;
      this.label1.Text = "Display Type";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cmbType
      // 
      this.cmbType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cmbType.BorderColor = System.Drawing.Color.Empty;
      this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbType.Items.AddRange(new object[] {
            "AutoDetect",
            "LCD",
            "LCD2",
            "VFD",
            "LCD3R"});
      this.cmbType.Location = new System.Drawing.Point(95, 18);
      this.cmbType.Name = "cmbType";
      this.cmbType.Size = new System.Drawing.Size(195, 21);
      this.cmbType.TabIndex = 12;
      this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
      // 
      // mpVFD_UseV3DLL
      // 
      this.mpVFD_UseV3DLL.AutoSize = true;
      this.mpVFD_UseV3DLL.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpVFD_UseV3DLL.Location = new System.Drawing.Point(13, 67);
      this.mpVFD_UseV3DLL.Name = "mpVFD_UseV3DLL";
      this.mpVFD_UseV3DLL.Size = new System.Drawing.Size(234, 17);
      this.mpVFD_UseV3DLL.TabIndex = 77;
      this.mpVFD_UseV3DLL.Text = "Use SG_VFD.dll  V3  (VFD with old firmware)";
      this.mpVFD_UseV3DLL.UseVisualStyleBackColor = true;
      this.mpVFD_UseV3DLL.CheckedChanged += new System.EventHandler(this.mpVFD_UseV3DLL_CheckedChanged);
      // 
      // groupBoxRemoteControl
      // 
      this.groupBoxRemoteControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxRemoteControl.Controls.Add(this.lblRemoteType);
      this.groupBoxRemoteControl.Controls.Add(this.cbRemoteType);
      this.groupBoxRemoteControl.Controls.Add(this.lblDelay);
      this.groupBoxRemoteControl.Controls.Add(this.tbDelay);
      this.groupBoxRemoteControl.Controls.Add(this.cbDisableRepeat);
      this.groupBoxRemoteControl.Controls.Add(this.cbUseRC);
      this.groupBoxRemoteControl.Location = new System.Drawing.Point(322, 368);
      this.groupBoxRemoteControl.Name = "groupBoxRemoteControl";
      this.groupBoxRemoteControl.Size = new System.Drawing.Size(300, 88);
      this.groupBoxRemoteControl.TabIndex = 131;
      this.groupBoxRemoteControl.TabStop = false;
      this.groupBoxRemoteControl.Text = " Remote Control Options";
      this.groupBoxRemoteControl.Visible = false;
      // 
      // lblRemoteType
      // 
      this.lblRemoteType.Location = new System.Drawing.Point(13, 37);
      this.lblRemoteType.Name = "lblRemoteType";
      this.lblRemoteType.Size = new System.Drawing.Size(73, 23);
      this.lblRemoteType.TabIndex = 140;
      this.lblRemoteType.Text = "Remote Type";
      this.lblRemoteType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbRemoteType
      // 
      this.cbRemoteType.BorderColor = System.Drawing.Color.Empty;
      this.cbRemoteType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbRemoteType.Items.AddRange(new object[] {
            "MCE",
            "PAD"});
      this.cbRemoteType.Location = new System.Drawing.Point(86, 37);
      this.cbRemoteType.Name = "cbRemoteType";
      this.cbRemoteType.Size = new System.Drawing.Size(83, 21);
      this.cbRemoteType.TabIndex = 139;
      // 
      // lblDelay
      // 
      this.lblDelay.Location = new System.Drawing.Point(169, 17);
      this.lblDelay.Name = "lblDelay";
      this.lblDelay.Size = new System.Drawing.Size(126, 17);
      this.lblDelay.TabIndex = 138;
      this.lblDelay.Text = "Repeat Delay: 1000ms";
      this.lblDelay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // tbDelay
      // 
      this.tbDelay.LargeChange = 1;
      this.tbDelay.Location = new System.Drawing.Point(180, 36);
      this.tbDelay.Maximum = 20;
      this.tbDelay.Name = "tbDelay";
      this.tbDelay.Size = new System.Drawing.Size(104, 45);
      this.tbDelay.TabIndex = 137;
      this.tbDelay.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
      this.tbDelay.Scroll += new System.EventHandler(this.tbDelay_Scroll);
      // 
      // cbDisableRepeat
      // 
      this.cbDisableRepeat.AutoSize = true;
      this.cbDisableRepeat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbDisableRepeat.Location = new System.Drawing.Point(14, 64);
      this.cbDisableRepeat.Name = "cbDisableRepeat";
      this.cbDisableRepeat.Size = new System.Drawing.Size(118, 17);
      this.cbDisableRepeat.TabIndex = 136;
      this.cbDisableRepeat.Text = "Disable Key Repeat";
      this.cbDisableRepeat.UseVisualStyleBackColor = true;
      this.cbDisableRepeat.CheckedChanged += new System.EventHandler(this.cbDisableRepeat_CheckedChanged);
      // 
      // cbUseRC
      // 
      this.cbUseRC.AutoSize = true;
      this.cbUseRC.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbUseRC.Location = new System.Drawing.Point(14, 17);
      this.cbUseRC.Name = "cbUseRC";
      this.cbUseRC.Size = new System.Drawing.Size(133, 17);
      this.cbUseRC.TabIndex = 126;
      this.cbUseRC.Text = "Enable Remote Control";
      this.cbUseRC.UseVisualStyleBackColor = true;
      this.cbUseRC.CheckedChanged += new System.EventHandler(this.cbUseRC_CheckedChanged);
      // 
      // groupBoxDisplayControl
      // 
      this.groupBoxDisplayControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxDisplayControl.Controls.Add(this.cmbBlankIdleTime);
      this.groupBoxDisplayControl.Controls.Add(this.mpEnableDisplayActionTime);
      this.groupBoxDisplayControl.Controls.Add(this.mpEnableDisplayAction);
      this.groupBoxDisplayControl.Controls.Add(this.mpBlankDisplayWithVideo);
      this.groupBoxDisplayControl.Controls.Add(this.mpBlankDisplayWhenIdle);
      this.groupBoxDisplayControl.Location = new System.Drawing.Point(322, 15);
      this.groupBoxDisplayControl.Name = "groupBoxDisplayControl";
      this.groupBoxDisplayControl.Size = new System.Drawing.Size(300, 82);
      this.groupBoxDisplayControl.TabIndex = 124;
      this.groupBoxDisplayControl.TabStop = false;
      this.groupBoxDisplayControl.Text = " Display Control Options ";
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
      this.cmbBlankIdleTime.Location = new System.Drawing.Point(174, 55);
      this.cmbBlankIdleTime.Name = "cmbBlankIdleTime";
      this.cmbBlankIdleTime.Size = new System.Drawing.Size(42, 21);
      this.cmbBlankIdleTime.TabIndex = 98;
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
      this.mpEnableDisplayActionTime.Location = new System.Drawing.Point(188, 33);
      this.mpEnableDisplayActionTime.Name = "mpEnableDisplayActionTime";
      this.mpEnableDisplayActionTime.Size = new System.Drawing.Size(42, 21);
      this.mpEnableDisplayActionTime.TabIndex = 96;
      // 
      // mpEnableDisplayAction
      // 
      this.mpEnableDisplayAction.AutoSize = true;
      this.mpEnableDisplayAction.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpEnableDisplayAction.Location = new System.Drawing.Point(30, 35);
      this.mpEnableDisplayAction.Name = "mpEnableDisplayAction";
      this.mpEnableDisplayAction.Size = new System.Drawing.Size(256, 17);
      this.mpEnableDisplayAction.TabIndex = 97;
      this.mpEnableDisplayAction.Text = "Enable Display on Action for                   Seconds";
      this.mpEnableDisplayAction.UseVisualStyleBackColor = true;
      this.mpEnableDisplayAction.CheckedChanged += new System.EventHandler(this.mpEnableDisplayAction_CheckedChanged);
      // 
      // mpBlankDisplayWithVideo
      // 
      this.mpBlankDisplayWithVideo.AutoSize = true;
      this.mpBlankDisplayWithVideo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpBlankDisplayWithVideo.Location = new System.Drawing.Point(14, 14);
      this.mpBlankDisplayWithVideo.Name = "mpBlankDisplayWithVideo";
      this.mpBlankDisplayWithVideo.Size = new System.Drawing.Size(205, 17);
      this.mpBlankDisplayWithVideo.TabIndex = 95;
      this.mpBlankDisplayWithVideo.Text = "Turn off display during Video Playback";
      this.mpBlankDisplayWithVideo.UseVisualStyleBackColor = true;
      this.mpBlankDisplayWithVideo.CheckedChanged += new System.EventHandler(this.mpBlankDisplayWithVideo_CheckedChanged);
      // 
      // mpBlankDisplayWhenIdle
      // 
      this.mpBlankDisplayWhenIdle.AutoSize = true;
      this.mpBlankDisplayWhenIdle.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpBlankDisplayWhenIdle.Location = new System.Drawing.Point(14, 57);
      this.mpBlankDisplayWhenIdle.Name = "mpBlankDisplayWhenIdle";
      this.mpBlankDisplayWhenIdle.Size = new System.Drawing.Size(259, 17);
      this.mpBlankDisplayWhenIdle.TabIndex = 99;
      this.mpBlankDisplayWhenIdle.Text = "Turn off display when idle for                    seconds";
      this.mpBlankDisplayWhenIdle.UseVisualStyleBackColor = true;
      this.mpBlankDisplayWhenIdle.CheckedChanged += new System.EventHandler(this.mpBlankDisplayWhenIdle_CheckedChanged);
      // 
      // groupboxEqualizerOptions
      // 
      this.groupboxEqualizerOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupboxEqualizerOptions.Controls.Add(this.cmbDelayEqTime);
      this.groupboxEqualizerOptions.Controls.Add(this.groupEQstyle);
      this.groupboxEqualizerOptions.Controls.Add(this.cmbEqMode);
      this.groupboxEqualizerOptions.Controls.Add(this.mpLabelEQmode);
      this.groupboxEqualizerOptions.Controls.Add(this.mpLabel2);
      this.groupboxEqualizerOptions.Controls.Add(this.cmbEQTitleDisplayTime);
      this.groupboxEqualizerOptions.Controls.Add(this.mpLabelEQTitleDisplay);
      this.groupboxEqualizerOptions.Controls.Add(this.cmbEQTitleShowTime);
      this.groupboxEqualizerOptions.Controls.Add(this.mpEQTitleDisplay);
      this.groupboxEqualizerOptions.Controls.Add(this.mpSmoothEQ);
      this.groupboxEqualizerOptions.Controls.Add(this.mpEqDisplay);
      this.groupboxEqualizerOptions.Controls.Add(this.mpRestrictEQ);
      this.groupboxEqualizerOptions.Controls.Add(this.cmbEqRate);
      this.groupboxEqualizerOptions.Controls.Add(this.mpDelayEQ);
      this.groupboxEqualizerOptions.Location = new System.Drawing.Point(322, 103);
      this.groupboxEqualizerOptions.Name = "groupboxEqualizerOptions";
      this.groupboxEqualizerOptions.Size = new System.Drawing.Size(300, 259);
      this.groupboxEqualizerOptions.TabIndex = 123;
      this.groupboxEqualizerOptions.TabStop = false;
      this.groupboxEqualizerOptions.Text = " Equalizer Options ";
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
      this.cmbDelayEqTime.Location = new System.Drawing.Point(168, 169);
      this.cmbDelayEqTime.Name = "cmbDelayEqTime";
      this.cmbDelayEqTime.Size = new System.Drawing.Size(53, 21);
      this.cmbDelayEqTime.TabIndex = 124;
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
      this.groupEQstyle.Location = new System.Drawing.Point(8, 65);
      this.groupEQstyle.Name = "groupEQstyle";
      this.groupEQstyle.Size = new System.Drawing.Size(285, 60);
      this.groupEQstyle.TabIndex = 140;
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
      this.mpUseVUmeter2.CheckedChanged += new System.EventHandler(this.mpUseVUmeter2_CheckedChanged);
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
      this.mpUseVUmeter.CheckedChanged += new System.EventHandler(this.mpUseVUmeter_CheckedChanged);
      // 
      // mpUseStereoEQ
      // 
      this.mpUseStereoEQ.AutoSize = true;
      this.mpUseStereoEQ.Location = new System.Drawing.Point(73, 17);
      this.mpUseStereoEQ.Name = "mpUseStereoEQ";
      this.mpUseStereoEQ.Size = new System.Drawing.Size(56, 17);
      this.mpUseStereoEQ.TabIndex = 1;
      this.mpUseStereoEQ.Text = "Stereo";
      this.mpUseStereoEQ.UseVisualStyleBackColor = true;
      this.mpUseStereoEQ.CheckedChanged += new System.EventHandler(this.mpUseStereoEQ_CheckedChanged);
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
      this.mpNormalEQ.CheckedChanged += new System.EventHandler(this.mpNormalEQ_CheckedChanged);
      // 
      // cmbEqMode
      // 
      this.cmbEqMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cmbEqMode.BorderColor = System.Drawing.Color.Empty;
      this.cmbEqMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbEqMode.Items.AddRange(new object[] {
            "Up from bottom",
            "Down from top",
            "Expand from middle"});
      this.cmbEqMode.Location = new System.Drawing.Point(134, 43);
      this.cmbEqMode.Name = "cmbEqMode";
      this.cmbEqMode.Size = new System.Drawing.Size(97, 21);
      this.cmbEqMode.TabIndex = 122;
      // 
      // mpLabelEQmode
      // 
      this.mpLabelEQmode.Location = new System.Drawing.Point(42, 45);
      this.mpLabelEQmode.Name = "mpLabelEQmode";
      this.mpLabelEQmode.Size = new System.Drawing.Size(95, 17);
      this.mpLabelEQmode.TabIndex = 136;
      this.mpLabelEQmode.Text = "EQ Display Mode:";
      this.mpLabelEQmode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(108, 150);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(116, 17);
      this.mpLabel2.TabIndex = 135;
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
      this.cmbEQTitleDisplayTime.Location = new System.Drawing.Point(172, 232);
      this.cmbEQTitleDisplayTime.Name = "cmbEQTitleDisplayTime";
      this.cmbEQTitleDisplayTime.Size = new System.Drawing.Size(49, 21);
      this.cmbEQTitleDisplayTime.TabIndex = 130;
      // 
      // mpLabelEQTitleDisplay
      // 
      this.mpLabelEQTitleDisplay.Location = new System.Drawing.Point(94, 234);
      this.mpLabelEQTitleDisplay.Name = "mpLabelEQTitleDisplay";
      this.mpLabelEQTitleDisplay.Size = new System.Drawing.Size(197, 17);
      this.mpLabelEQTitleDisplay.TabIndex = 134;
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
      this.cmbEQTitleShowTime.Location = new System.Drawing.Point(45, 232);
      this.cmbEQTitleShowTime.Name = "cmbEQTitleShowTime";
      this.cmbEQTitleShowTime.Size = new System.Drawing.Size(49, 21);
      this.cmbEQTitleShowTime.TabIndex = 133;
      // 
      // mpEQTitleDisplay
      // 
      this.mpEQTitleDisplay.AutoSize = true;
      this.mpEQTitleDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpEQTitleDisplay.Location = new System.Drawing.Point(29, 213);
      this.mpEQTitleDisplay.Name = "mpEQTitleDisplay";
      this.mpEQTitleDisplay.Size = new System.Drawing.Size(118, 17);
      this.mpEQTitleDisplay.TabIndex = 132;
      this.mpEQTitleDisplay.Text = "Show Track Info for";
      this.mpEQTitleDisplay.UseVisualStyleBackColor = true;
      this.mpEQTitleDisplay.CheckedChanged += new System.EventHandler(this.mpEQTitleDisplay_CheckedChanged);
      // 
      // mpSmoothEQ
      // 
      this.mpSmoothEQ.AutoSize = true;
      this.mpSmoothEQ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpSmoothEQ.Location = new System.Drawing.Point(29, 192);
      this.mpSmoothEQ.Name = "mpSmoothEQ";
      this.mpSmoothEQ.Size = new System.Drawing.Size(222, 17);
      this.mpSmoothEQ.TabIndex = 129;
      this.mpSmoothEQ.Text = "Use Equalizer Smoothing (Delayed decay)";
      this.mpSmoothEQ.UseVisualStyleBackColor = true;
      // 
      // mpEqDisplay
      // 
      this.mpEqDisplay.AutoSize = true;
      this.mpEqDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpEqDisplay.Location = new System.Drawing.Point(13, 24);
      this.mpEqDisplay.Name = "mpEqDisplay";
      this.mpEqDisplay.Size = new System.Drawing.Size(124, 17);
      this.mpEqDisplay.TabIndex = 126;
      this.mpEqDisplay.Text = "Use Equalizer display";
      this.mpEqDisplay.UseVisualStyleBackColor = true;
      this.mpEqDisplay.CheckedChanged += new System.EventHandler(this.mpEqDisplay_CheckedChanged);
      // 
      // mpRestrictEQ
      // 
      this.mpRestrictEQ.AutoSize = true;
      this.mpRestrictEQ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRestrictEQ.Location = new System.Drawing.Point(29, 129);
      this.mpRestrictEQ.Name = "mpRestrictEQ";
      this.mpRestrictEQ.Size = new System.Drawing.Size(183, 17);
      this.mpRestrictEQ.TabIndex = 127;
      this.mpRestrictEQ.Text = "Limit Equalizer display update rate";
      this.mpRestrictEQ.UseVisualStyleBackColor = true;
      this.mpRestrictEQ.CheckedChanged += new System.EventHandler(this.mpRestrictEQ_CheckedChanged);
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
      this.cmbEqRate.Location = new System.Drawing.Point(45, 148);
      this.cmbEqRate.Name = "cmbEqRate";
      this.cmbEqRate.Size = new System.Drawing.Size(57, 21);
      this.cmbEqRate.TabIndex = 123;
      // 
      // mpDelayEQ
      // 
      this.mpDelayEQ.AutoSize = true;
      this.mpDelayEQ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpDelayEQ.Location = new System.Drawing.Point(29, 171);
      this.mpDelayEQ.Name = "mpDelayEQ";
      this.mpDelayEQ.Size = new System.Drawing.Size(247, 17);
      this.mpDelayEQ.TabIndex = 128;
      this.mpDelayEQ.Text = "Delay Equalizer Start by                       Seconds";
      this.mpDelayEQ.UseVisualStyleBackColor = true;
      this.mpDelayEQ.CheckedChanged += new System.EventHandler(this.mpDelayEQ_CheckedChanged);
      // 
      // groupBoxDisplayOptions
      // 
      this.groupBoxDisplayOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.groupBoxDisplayOptions.Controls.Add(this.ckDiskIcon);
      this.groupBoxDisplayOptions.Controls.Add(this.ckDiskMediaStatus);
      this.groupBoxDisplayOptions.Controls.Add(this.ckDeviceMonitor);
      this.groupBoxDisplayOptions.Controls.Add(this.mpVolumeDisplay);
      this.groupBoxDisplayOptions.Controls.Add(this.mpProgressBar);
      this.groupBoxDisplayOptions.Controls.Add(this.mpEditFont);
      this.groupBoxDisplayOptions.Controls.Add(this.mpEditIcon);
      this.groupBoxDisplayOptions.Controls.Add(this.mpUseCustomFont);
      this.groupBoxDisplayOptions.Controls.Add(this.mpUseLargeIcons);
      this.groupBoxDisplayOptions.Controls.Add(this.mpUseCustomIcons);
      this.groupBoxDisplayOptions.Controls.Add(this.mpUseInvertedIcons);
      this.groupBoxDisplayOptions.Location = new System.Drawing.Point(10, 268);
      this.groupBoxDisplayOptions.Name = "groupBoxDisplayOptions";
      this.groupBoxDisplayOptions.Size = new System.Drawing.Size(306, 188);
      this.groupBoxDisplayOptions.TabIndex = 130;
      this.groupBoxDisplayOptions.TabStop = false;
      this.groupBoxDisplayOptions.Text = "Display Options (Valid only with LCD/LCD2 displays) ";
      // 
      // ckDiskIcon
      // 
      this.ckDiskIcon.AutoSize = true;
      this.ckDiskIcon.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ckDiskIcon.Location = new System.Drawing.Point(16, 20);
      this.ckDiskIcon.Name = "ckDiskIcon";
      this.ckDiskIcon.Size = new System.Drawing.Size(91, 17);
      this.ckDiskIcon.TabIndex = 73;
      this.ckDiskIcon.Text = "Use Disk Icon";
      this.ckDiskIcon.UseVisualStyleBackColor = true;
      this.ckDiskIcon.CheckedChanged += new System.EventHandler(this.ckDiskIcon_CheckedChanged);
      // 
      // ckDiskMediaStatus
      // 
      this.ckDiskMediaStatus.AutoSize = true;
      this.ckDiskMediaStatus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ckDiskMediaStatus.Location = new System.Drawing.Point(32, 38);
      this.ckDiskMediaStatus.Name = "ckDiskMediaStatus";
      this.ckDiskMediaStatus.Size = new System.Drawing.Size(171, 17);
      this.ckDiskMediaStatus.TabIndex = 73;
      this.ckDiskMediaStatus.Text = "Display Media Transport Status";
      this.ckDiskMediaStatus.UseVisualStyleBackColor = true;
      // 
      // ckDeviceMonitor
      // 
      this.ckDeviceMonitor.AutoSize = true;
      this.ckDeviceMonitor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ckDeviceMonitor.Location = new System.Drawing.Point(32, 54);
      this.ckDeviceMonitor.Name = "ckDeviceMonitor";
      this.ckDeviceMonitor.Size = new System.Drawing.Size(172, 17);
      this.ckDeviceMonitor.TabIndex = 73;
      this.ckDeviceMonitor.Text = "Display CD/DVD volume status";
      this.ckDeviceMonitor.UseVisualStyleBackColor = true;
      // 
      // mpVolumeDisplay
      // 
      this.mpVolumeDisplay.AutoSize = true;
      this.mpVolumeDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpVolumeDisplay.Location = new System.Drawing.Point(16, 72);
      this.mpVolumeDisplay.Name = "mpVolumeDisplay";
      this.mpVolumeDisplay.Size = new System.Drawing.Size(171, 17);
      this.mpVolumeDisplay.TabIndex = 75;
      this.mpVolumeDisplay.Text = "Use Top Bar as Volume display";
      this.mpVolumeDisplay.UseVisualStyleBackColor = true;
      // 
      // mpProgressBar
      // 
      this.mpProgressBar.AutoSize = true;
      this.mpProgressBar.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpProgressBar.Location = new System.Drawing.Point(16, 90);
      this.mpProgressBar.Name = "mpProgressBar";
      this.mpProgressBar.Size = new System.Drawing.Size(193, 17);
      this.mpProgressBar.TabIndex = 74;
      this.mpProgressBar.Text = "Use Bottom Bar as Progress Display";
      this.mpProgressBar.UseVisualStyleBackColor = true;
      // 
      // mpEditFont
      // 
      this.mpEditFont.Location = new System.Drawing.Point(215, 105);
      this.mpEditFont.Name = "mpEditFont";
      this.mpEditFont.Size = new System.Drawing.Size(63, 23);
      this.mpEditFont.TabIndex = 79;
      this.mpEditFont.Text = "&Edit Font";
      this.mpEditFont.UseVisualStyleBackColor = true;
      this.mpEditFont.Click += new System.EventHandler(this.mpEditFont_Click);
      // 
      // mpEditIcon
      // 
      this.mpEditIcon.Location = new System.Drawing.Point(215, 141);
      this.mpEditIcon.Name = "mpEditIcon";
      this.mpEditIcon.Size = new System.Drawing.Size(63, 23);
      this.mpEditIcon.TabIndex = 78;
      this.mpEditIcon.Text = "&Edit Icons";
      this.mpEditIcon.UseVisualStyleBackColor = true;
      this.mpEditIcon.Click += new System.EventHandler(this.mpEditIcon_Click);
      // 
      // mpUseCustomFont
      // 
      this.mpUseCustomFont.AutoSize = true;
      this.mpUseCustomFont.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpUseCustomFont.Location = new System.Drawing.Point(16, 108);
      this.mpUseCustomFont.Name = "mpUseCustomFont";
      this.mpUseCustomFont.Size = new System.Drawing.Size(105, 17);
      this.mpUseCustomFont.TabIndex = 77;
      this.mpUseCustomFont.Text = "Use Custom Font";
      this.mpUseCustomFont.UseVisualStyleBackColor = true;
      this.mpUseCustomFont.CheckedChanged += new System.EventHandler(this.mpUseCustomFont_CheckedChanged);
      // 
      // mpUseLargeIcons
      // 
      this.mpUseLargeIcons.AutoSize = true;
      this.mpUseLargeIcons.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpUseLargeIcons.Location = new System.Drawing.Point(16, 126);
      this.mpUseLargeIcons.Name = "mpUseLargeIcons";
      this.mpUseLargeIcons.Size = new System.Drawing.Size(102, 17);
      this.mpUseLargeIcons.TabIndex = 77;
      this.mpUseLargeIcons.Text = "Use Large Icons";
      this.mpUseLargeIcons.UseVisualStyleBackColor = true;
      this.mpUseLargeIcons.CheckedChanged += new System.EventHandler(this.mpUseLargeIcons_CheckedChanged);
      // 
      // mpUseCustomIcons
      // 
      this.mpUseCustomIcons.AutoSize = true;
      this.mpUseCustomIcons.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpUseCustomIcons.Location = new System.Drawing.Point(32, 144);
      this.mpUseCustomIcons.Name = "mpUseCustomIcons";
      this.mpUseCustomIcons.Size = new System.Drawing.Size(140, 17);
      this.mpUseCustomIcons.TabIndex = 77;
      this.mpUseCustomIcons.Text = "Use Custom Large Icons";
      this.mpUseCustomIcons.UseVisualStyleBackColor = true;
      this.mpUseCustomIcons.CheckedChanged += new System.EventHandler(this.mpUseCustomIcons_CheckedChanged);
      // 
      // mpUseInvertedIcons
      // 
      this.mpUseInvertedIcons.AutoSize = true;
      this.mpUseInvertedIcons.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpUseInvertedIcons.Location = new System.Drawing.Point(32, 162);
      this.mpUseInvertedIcons.Name = "mpUseInvertedIcons";
      this.mpUseInvertedIcons.Size = new System.Drawing.Size(172, 17);
      this.mpUseInvertedIcons.TabIndex = 77;
      this.mpUseInvertedIcons.Text = "Invert (reverse) the Large Icons";
      this.mpUseInvertedIcons.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(563, 476);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(78, 23);
      this.btnOK.TabIndex = 6;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnReset
      // 
      this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnReset.Location = new System.Drawing.Point(479, 476);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new System.Drawing.Size(78, 23);
      this.btnReset.TabIndex = 6;
      this.btnReset.Text = "&RESET";
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // iMONLCDg_AdvancedSetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(650, 504);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnReset);
      this.Controls.Add(this.groupBoxConfiguration);
      this.Name = "iMONLCDg_AdvancedSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MiniDisplay - Setup - Advanced Settings";
      this.groupBoxConfiguration.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBoxManager.ResumeLayout(false);
      this.groupBoxManager.PerformLayout();
      this.groupBoxHardware.ResumeLayout(false);
      this.groupBoxHardware.PerformLayout();
      this.groupBoxRemoteControl.ResumeLayout(false);
      this.groupBoxRemoteControl.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbDelay)).EndInit();
      this.groupBoxDisplayControl.ResumeLayout(false);
      this.groupBoxDisplayControl.PerformLayout();
      this.groupboxEqualizerOptions.ResumeLayout(false);
      this.groupboxEqualizerOptions.PerformLayout();
      this.groupEQstyle.ResumeLayout(false);
      this.groupEQstyle.PerformLayout();
      this.groupBoxDisplayOptions.ResumeLayout(false);
      this.groupBoxDisplayOptions.PerformLayout();
      this.ResumeLayout(false);

    }

    private void mpBlankDisplayWhenIdle_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpBlankDisplayWithVideo_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpDelayEQ_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpEditFont_Click(object sender, EventArgs e)
    {
      Form form = new iMONLCDg_FontEdit();
      base.Visible = false;
      form.ShowDialog();
      form.Dispose();
      base.Visible = true;
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
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpEqDisplay_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpEQTitleDisplay_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpNormalEQ_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpRestrictEQ_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpUseCustomFont_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpUseCustomIcons_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpUseLargeIcons_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpUseStereoEQ_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpUseVUmeter_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpUseVUmeter2_CheckedChanged(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void mpVFD_UseV3DLL_CheckedChanged(object sender, EventArgs e)
    {
      if (this.mpVFD_UseV3DLL.Checked)
      {
        this.cmbType.SelectedIndex = 3;
        this.cmbType.SelectedValue = 3;
        this.cmbType.Enabled = false;
        this.cmbType.Refresh();
      }
      else
      {
        this.cmbType.Enabled = true;
        this.cmbType.Refresh();
      }
    }

    private void SetDelayLabel()
    {
      this.lblDelay.Text = "Repeat Delay: " + ((this.tbDelay.Value * 0x19)).ToString() + "ms";
    }

    private void tbDelay_Scroll(object sender, EventArgs e)
    {
      this.cmbType_Changed(this, EventArgs.Empty);
    }

    private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cmbType.Text == "LCD" || cmbType.Text == "LCD2" || cmbType.Text == "AutoDetect")
      {
        groupBoxDisplayOptions.Enabled = true;
        ckDeviceMonitor.Enabled = true;
        ckDiskIcon.Enabled = true;
        ckDiskMediaStatus.Enabled = true;
        mpVolumeDisplay.Enabled = true;
        mpProgressBar.Enabled = true;
        mpUseCustomFont.Enabled = true;
        mpUseLargeIcons.Enabled = true;
        mpUseCustomIcons.Enabled = true;
        mpUseInvertedIcons.Enabled = true;
        mpEditFont.Enabled = true;
        mpEditIcon.Enabled = true;
      }
      else
      {
        groupBoxDisplayOptions.Enabled = false;
      }
    }
  }
}

