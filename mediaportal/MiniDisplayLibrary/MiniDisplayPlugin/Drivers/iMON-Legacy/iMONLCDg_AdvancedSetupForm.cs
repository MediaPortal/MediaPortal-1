#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class iMONLCDg_AdvancedSetupForm : MPConfigForm
  {
    private MPButton btnOK;
    private MPButton btnReset;
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
    private GroupBox groupBox1;
    private MPGroupBox groupBoxConfiguration;
    private GroupBox groupBoxDisplayControl;
    private GroupBox groupBoxDisplayOptions;
    private GroupBox groupboxEqualizerOptions;
    private GroupBox groupBoxHardware;
    private GroupBox groupBoxManager;
    private GroupBox groupEQstyle;
    private MPLabel label1;
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
    private MPCheckBox mpForceManagerReload;
    private MPCheckBox mpForceManagerRestart;
    private MPLabel mpLabel2;
    private MPLabel mpLabelEQmode;
    private MPLabel mpLabelEQTitleDisplay;
    private MPCheckBox mpLeaveFrontviewActive;
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

    public iMONLCDg_AdvancedSetupForm()
    {
      Log.Debug("iMONLCDg.AdvancedSetupForm(): Constructor started");
      InitializeComponent();
      cmbType.SelectedIndex = 0;
      cmbType.DataBindings.Add("SelectedItem", iMONLCDg.AdvancedSettings.Instance, "DisplayType");
      mpVolumeDisplay.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "VolumeDisplay");
      mpEqDisplay.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "EqDisplay");
      cmbEqMode.SelectedIndex = 0;
      cmbEqMode.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "EqMode");
      mpRestrictEQ.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "RestrictEQ");
      mpProgressBar.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "ProgressDisplay");
      ckDiskIcon.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DiskIcon");
      ckDiskMediaStatus.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DiskMediaStatus");
      ckDeviceMonitor.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DeviceMonitor");
      mpUseCustomFont.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "UseCustomFont");
      mpUseLargeIcons.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "UseLargeIcons");
      mpUseCustomIcons.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "UseCustomIcons");
      mpUseInvertedIcons.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "UseInvertedIcons");
      mpBlankDisplayWithVideo.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance,
                                               "BlankDisplayWithVideo");
      mpEnableDisplayAction.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "EnableDisplayAction");
      mpEnableDisplayActionTime.SelectedIndex = 0;
      mpEnableDisplayActionTime.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance,
                                                 "EnableDisplayActionTime");
      mpVFD_UseV3DLL.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "VFD_UseV3DLL");
      cmbEqRate.SelectedIndex = 0;
      cmbEqRate.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "EqRate");
      mpDelayEQ.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DelayEQ");
      cmbDelayEqTime.SelectedIndex = 0;
      cmbDelayEqTime.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "DelayEqTime");
      mpMonitorPowerState.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "MonitorPowerState");
      mpSmoothEQ.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "SmoothEQ");
      mpEQTitleDisplay.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "EQTitleDisplay");
      cmbEQTitleDisplayTime.SelectedIndex = 0;
      cmbEQTitleDisplayTime.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance,
                                             "EQTitleDisplayTime");
      cmbEQTitleShowTime.SelectedIndex = 0;
      cmbEQTitleShowTime.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "EQTitleShowTime");
      mpBlankDisplayWhenIdle.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "BlankDisplayWhenIdle");
      cmbBlankIdleTime.SelectedIndex = 0;
      cmbBlankIdleTime.DataBindings.Add("SelectedIndex", iMONLCDg.AdvancedSettings.Instance, "BlankIdleTime");
      if (iMONLCDg.AdvancedSettings.Instance.NormalEQ)
      {
        mpNormalEQ.Checked = true;
      }
      else if (iMONLCDg.AdvancedSettings.Instance.StereoEQ)
      {
        mpUseStereoEQ.Checked = true;
      }
      else if (iMONLCDg.AdvancedSettings.Instance.VUmeter)
      {
        mpUseVUmeter.Checked = true;
      }
      else
      {
        mpUseVUmeter2.Checked = true;
      }
      cbVUindicators.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "VUindicators");
      mpDelayStartup.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "DelayStartup");
      mpEnsureManagerStartup.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "EnsureManagerStartup");
      mpForceManagerRestart.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "ForceManagerRestart");
      mpForceManagerReload.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "ForceManagerReload");
      mpRestartFrontview.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "RestartFrontviewOnExit");
      mpLeaveFrontviewActive.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "LeaveFrontviewActive");
      mpForceKeyBoardMode.DataBindings.Add("Checked", iMONLCDg.AdvancedSettings.Instance, "ForceKeyBoardMode");
      cmbType_Changed();
      Log.Debug("iMONLCDg.AdvancedSetupForm(): Constructor completed");
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Log.Debug("iMONLCDg.AdvancedSetupForm.btnOK_Click(): started");
      if (mpNormalEQ.Checked)
      {
        iMONLCDg.AdvancedSettings.Instance.NormalEQ = true;
        iMONLCDg.AdvancedSettings.Instance.StereoEQ = false;
        iMONLCDg.AdvancedSettings.Instance.VUmeter = false;
        iMONLCDg.AdvancedSettings.Instance.VUmeter2 = false;
      }
      else if (mpUseStereoEQ.Checked)
      {
        iMONLCDg.AdvancedSettings.Instance.NormalEQ = false;
        iMONLCDg.AdvancedSettings.Instance.StereoEQ = true;
        iMONLCDg.AdvancedSettings.Instance.VUmeter = false;
        iMONLCDg.AdvancedSettings.Instance.VUmeter2 = false;
      }
      else if (mpUseVUmeter.Checked)
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
      Hide();
      Close();
      Log.Debug("iMONLCDg.AdvancedSetupForm.btnOK_Click(): Completed");
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      Log.Debug("iMONLCDg.AdvancedSetupForm.btnReset_Click(): started");
      mpDelayStartup.Checked = false;
      mpEnsureManagerStartup.Checked = false;
      mpForceManagerRestart.Checked = false;
      mpForceManagerReload.Checked = false;
      mpRestartFrontview.Checked = false;
      mpLeaveFrontviewActive.Checked = false;
      mpForceKeyBoardMode.Checked = false;
      cmbType.SelectedIndex = 0;
      mpVFD_UseV3DLL.Checked = false;
      ckDiskIcon.Checked = false;
      ckDiskMediaStatus.Checked = false;
      ckDeviceMonitor.Checked = false;
      mpVolumeDisplay.Checked = false;
      mpProgressBar.Checked = false;
      mpUseCustomFont.Checked = false;
      mpUseCustomIcons.Checked = false;
      mpUseInvertedIcons.Checked = false;
      mpUseLargeIcons.Checked = false;
      mpEqDisplay.Checked = false;
      cbVUindicators.Checked = false;
      cmbEqMode.SelectedIndex = 0;
      mpRestrictEQ.Checked = false;
      cmbEqRate.SelectedIndex = 10;
      mpDelayEQ.Checked = false;
      cmbDelayEqTime.SelectedIndex = 10;
      mpBlankDisplayWithVideo.Checked = false;
      mpMonitorPowerState.Checked = false;
      mpSmoothEQ.Enabled = false;
      mpEQTitleDisplay.Checked = false;
      cmbEQTitleDisplayTime.SelectedIndex = 10;
      cmbEQTitleShowTime.SelectedIndex = 2;
      if (iMONLCDg.AdvancedSettings.Instance.NormalEQ)
      {
        mpNormalEQ.Checked = true;
      }
      else if (iMONLCDg.AdvancedSettings.Instance.StereoEQ)
      {
        mpUseStereoEQ.Checked = true;
      }
      else if (iMONLCDg.AdvancedSettings.Instance.VUmeter)
      {
        mpUseVUmeter.Checked = true;
      }
      else
      {
        mpUseVUmeter2.Checked = true;
      }
      iMONLCDg.AdvancedSettings.SetDefaults();
      cmbType_Changed();
      Refresh();
      Log.Debug("iMONLCDg.AdvancedSetupForm.btnReset_Click(): Completed");
    }

    private void cmbType_Changed()
    {
      mpEnsureManagerStartup.Enabled = true;
      mpForceManagerRestart.Enabled = true;
      mpForceManagerReload.Enabled = true;

      cmbType.Enabled = !mpVFD_UseV3DLL.Checked;
      switch (cmbType.SelectedItem.ToString())
      {
        case "AutoDetect":
          Log.Debug("iMONLCDg.AdvancedSetupForm.cmbType_changed: Enabling Advanced Setup options for AutoDetect",
                    new object[0]);
          mpBlankDisplayWhenIdle.Enabled = true;
          cmbBlankIdleTime.Enabled = mpBlankDisplayWhenIdle.Checked;
          mpBlankDisplayWithVideo.Enabled = true;
          if (mpBlankDisplayWithVideo.Checked)
          {
            mpEnableDisplayAction.Enabled = true;
            mpEnableDisplayActionTime.Enabled = mpEnableDisplayAction.Checked;
          }
          else
          {
            mpEnableDisplayAction.Enabled = false;
            mpEnableDisplayActionTime.Enabled = false;
          }
          mpProgressBar.Enabled = true;
          mpUseCustomFont.Enabled = true;
          mpEditFont.Enabled = mpUseCustomFont.Checked;
          mpVFD_UseV3DLL.Enabled = true;
          mpVolumeDisplay.Enabled = true;
          ckDiskIcon.Enabled = true;
          if (ckDiskIcon.Checked)
          {
            ckDiskMediaStatus.Enabled = true;
            ckDeviceMonitor.Enabled = true;
          }
          else
          {
            ckDiskMediaStatus.Enabled = false;
            ckDeviceMonitor.Enabled = false;
          }
          mpUseLargeIcons.Enabled = true;
          if (mpUseLargeIcons.Checked)
          {
            mpUseCustomIcons.Enabled = true;
            mpEditIcon.Enabled = mpUseCustomIcons.Checked;
            mpUseInvertedIcons.Enabled = true;
          }
          else
          {
            mpUseCustomIcons.Enabled = false;
            mpUseInvertedIcons.Enabled = false;
            mpEditIcon.Enabled = false;
          }
          mpEqDisplay.Enabled = true;
          if (mpEqDisplay.Checked)
          {
            cmbEqMode.Enabled = true;
            mpLabelEQmode.Enabled = true;
            groupEQstyle.Enabled = true;
            mpUseVUmeter.Enabled = true;
            mpUseVUmeter2.Enabled = true;
            cbVUindicators.Enabled = false;
            if (mpUseVUmeter.Checked || mpUseVUmeter2.Checked)
            {
              cbVUindicators.Enabled = true;
            }
            mpRestrictEQ.Enabled = true;
            cmbEqRate.Enabled = mpRestrictEQ.Checked;
            mpDelayEQ.Enabled = true;
            cmbDelayEqTime.Enabled = mpDelayEQ.Checked;
            mpSmoothEQ.Enabled = true;
            mpEQTitleDisplay.Enabled = true;
            mpLabelEQTitleDisplay.Enabled = true;
            if (mpEQTitleDisplay.Checked)
            {
              cmbEQTitleDisplayTime.Enabled = true;
              cmbEQTitleShowTime.Enabled = true;
            }
            else
            {
              cmbEQTitleDisplayTime.Enabled = false;
              cmbEQTitleShowTime.Enabled = false;
            }
          }
          else
          {
            cmbEqMode.Enabled = false;
            mpLabelEQmode.Enabled = false;
            groupEQstyle.Enabled = false;
            mpRestrictEQ.Enabled = false;
            cmbEqRate.Enabled = false;
            mpDelayEQ.Enabled = false;
            cmbDelayEqTime.Enabled = false;
            mpSmoothEQ.Enabled = false;
            mpEQTitleDisplay.Enabled = false;
            mpLabelEQTitleDisplay.Enabled = false;
            cmbEQTitleDisplayTime.Enabled = false;
            cmbEQTitleShowTime.Enabled = false;
          }
          break;
        case "LCD2":
        case "LCD":
          Log.Debug("iMONLCDg.AdvancedSetupForm.cmbType_changed: Enabling Advanced Setup options for LCD/LCD2",
                    new object[0]);
          mpBlankDisplayWhenIdle.Enabled = true;
          cmbBlankIdleTime.Enabled = mpBlankDisplayWhenIdle.Checked;
          mpBlankDisplayWithVideo.Enabled = true;
          if (mpBlankDisplayWithVideo.Checked)
          {
            mpEnableDisplayAction.Enabled = true;
            mpEnableDisplayActionTime.Enabled = mpEnableDisplayAction.Checked;
          }
          else
          {
            mpEnableDisplayAction.Enabled = false;
            mpEnableDisplayActionTime.Enabled = false;
          }
          mpProgressBar.Enabled = true;
          mpUseCustomFont.Enabled = true;
          mpEditFont.Enabled = mpUseCustomFont.Checked;
          mpVolumeDisplay.Enabled = true;
          ckDiskIcon.Enabled = true;
          if (ckDiskIcon.Checked)
          {
            ckDiskMediaStatus.Enabled = true;
            ckDeviceMonitor.Enabled = true;
          }
          else
          {
            ckDiskMediaStatus.Enabled = false;
            ckDeviceMonitor.Enabled = false;
          }
          mpUseLargeIcons.Enabled = true;
          if (mpUseLargeIcons.Checked)
          {
            mpUseCustomIcons.Enabled = true;
            mpEditIcon.Enabled = mpUseCustomIcons.Checked;
            mpUseInvertedIcons.Enabled = true;
          }
          else
          {
            mpUseCustomIcons.Enabled = false;
            mpEditIcon.Enabled = false;
            mpUseInvertedIcons.Enabled = false;
          }
          mpEqDisplay.Enabled = true;
          if (mpEqDisplay.Checked)
          {
            groupEQstyle.Enabled = true;
            mpUseVUmeter.Enabled = true;
            mpUseVUmeter2.Enabled = true;
            cbVUindicators.Enabled = false;
            if (mpUseVUmeter.Checked || mpUseVUmeter2.Checked)
            {
              cbVUindicators.Enabled = true;
            }
            cmbEqMode.Enabled = true;
            mpLabelEQmode.Enabled = true;
            mpRestrictEQ.Enabled = true;
            cmbEqRate.Enabled = mpRestrictEQ.Checked;
            mpDelayEQ.Enabled = true;
            cmbDelayEqTime.Enabled = mpDelayEQ.Checked;
            mpSmoothEQ.Enabled = true;
            mpEQTitleDisplay.Enabled = true;
            mpLabelEQTitleDisplay.Enabled = true;
            if (mpEQTitleDisplay.Checked)
            {
              cmbEQTitleDisplayTime.Enabled = true;
              cmbEQTitleShowTime.Enabled = true;
            }
            else
            {
              cmbEQTitleDisplayTime.Enabled = false;
              cmbEQTitleShowTime.Enabled = false;
            }
          }
          else
          {
            mpLabelEQmode.Enabled = false;
            groupEQstyle.Enabled = false;
            cmbEqMode.Enabled = false;
            mpRestrictEQ.Enabled = false;
            cmbEqRate.Enabled = false;
            mpDelayEQ.Enabled = false;
            cmbDelayEqTime.Enabled = false;
            mpSmoothEQ.Enabled = false;
            mpEQTitleDisplay.Enabled = false;
            mpLabelEQTitleDisplay.Enabled = false;
            cmbEQTitleDisplayTime.Enabled = false;
            cmbEQTitleShowTime.Enabled = false;
          }
          break;
        case "VFD":
          Log.Debug("iMONLCDg.AdvancedSetupForm.cmbType_changed: Enabling Advanced Setup options for VFD");
          mpBlankDisplayWhenIdle.Enabled = true;
          cmbBlankIdleTime.Enabled = mpBlankDisplayWhenIdle.Checked;
          mpBlankDisplayWithVideo.Enabled = true;
          if (mpBlankDisplayWithVideo.Checked)
          {
            mpEnableDisplayAction.Enabled = true;
            mpEnableDisplayActionTime.Enabled = mpEnableDisplayAction.Checked;
          }
          else
          {
            mpEnableDisplayAction.Enabled = false;
            mpEnableDisplayActionTime.Enabled = false;
          }
          mpEqDisplay.Enabled = true;
          if (mpEqDisplay.Checked)
          {
            groupEQstyle.Enabled = true;
            mpUseVUmeter.Enabled = true;
            mpUseVUmeter2.Enabled = true;
            cbVUindicators.Enabled = false;
            if (mpUseVUmeter.Checked || mpUseVUmeter2.Checked)
            {
              cbVUindicators.Enabled = true;
            }
            cmbEqMode.SelectedIndex = 0;
            cmbEqMode.Enabled = false;
            mpLabelEQmode.Enabled = false;
            mpRestrictEQ.Enabled = true;
            cmbEqRate.Enabled = mpRestrictEQ.Checked;
            mpDelayEQ.Enabled = true;
            cmbDelayEqTime.Enabled = mpDelayEQ.Checked;
            mpSmoothEQ.Enabled = true;
            mpEQTitleDisplay.Enabled = true;
            mpLabelEQTitleDisplay.Enabled = true;
            if (mpEQTitleDisplay.Checked)
            {
              cmbEQTitleDisplayTime.Enabled = true;
              cmbEQTitleShowTime.Enabled = true;
            }
            else
            {
              cmbEQTitleDisplayTime.Enabled = false;
              cmbEQTitleShowTime.Enabled = false;
            }
          }
          else
          {
            groupEQstyle.Enabled = false;
            cmbEqMode.SelectedIndex = 0;
            cmbEqMode.Enabled = false;
            mpLabelEQmode.Enabled = true;
            mpRestrictEQ.Enabled = false;
            cmbEqRate.Enabled = false;
            mpDelayEQ.Enabled = false;
            cmbDelayEqTime.Enabled = false;
            mpSmoothEQ.Enabled = false;
            mpEQTitleDisplay.Enabled = false;
            mpLabelEQTitleDisplay.Enabled = false;
            cmbEQTitleDisplayTime.Enabled = false;
            cmbEQTitleShowTime.Enabled = false;
          }
          mpProgressBar.Enabled = false;
          mpUseCustomFont.Enabled = false;
          mpEditFont.Enabled = false;
          mpUseCustomIcons.Enabled = false;
          mpEditIcon.Enabled = false;
          mpUseInvertedIcons.Enabled = false;
          mpUseLargeIcons.Enabled = false;
          mpVolumeDisplay.Enabled = false;
          ckDiskIcon.Enabled = false;
          ckDiskMediaStatus.Enabled = false;
          ckDeviceMonitor.Enabled = false;
          break;
        case "LCD3R":
          Log.Debug("iMONLCDg.AdvancedSetupForm.cmbType_changed: Enabling Advanced Setup options for LCD3R",
                    new object[0]);
          mpBlankDisplayWhenIdle.Enabled = true;
          cmbBlankIdleTime.Enabled = mpBlankDisplayWhenIdle.Checked;
          mpBlankDisplayWithVideo.Enabled = true;
          if (mpBlankDisplayWithVideo.Checked)
          {
            mpEnableDisplayAction.Enabled = true;
            mpEnableDisplayActionTime.Enabled = mpEnableDisplayAction.Checked;
          }
          else
          {
            mpEnableDisplayAction.Enabled = false;
            mpEnableDisplayActionTime.Enabled = false;
          }
          mpEqDisplay.Enabled = true;
          if (mpEqDisplay.Checked)
          {
            groupEQstyle.Enabled = true;
            cbVUindicators.Enabled = false;
            mpUseVUmeter.Enabled = false;
            mpUseVUmeter2.Enabled = false;
            cbVUindicators.Enabled = true;
            cmbEqMode.SelectedIndex = 0;
            cmbEqMode.Enabled = false;
            mpLabelEQmode.Enabled = false;
            mpRestrictEQ.Enabled = true;
            cmbEqRate.Enabled = mpRestrictEQ.Checked;
            mpDelayEQ.Enabled = true;
            cmbDelayEqTime.Enabled = mpDelayEQ.Checked;
            mpSmoothEQ.Enabled = true;
            mpEQTitleDisplay.Enabled = true;
            mpLabelEQTitleDisplay.Enabled = true;
            if (mpEQTitleDisplay.Checked)
            {
              cmbEQTitleDisplayTime.Enabled = true;
              cmbEQTitleShowTime.Enabled = true;
            }
            else
            {
              cmbEQTitleDisplayTime.Enabled = false;
              cmbEQTitleShowTime.Enabled = false;
            }
          }
          else
          {
            groupEQstyle.Enabled = false;
            cmbEqMode.SelectedIndex = 0;
            cmbEqMode.Enabled = false;
            mpLabelEQmode.Enabled = true;
            mpRestrictEQ.Enabled = false;
            cmbEqRate.Enabled = false;
            mpDelayEQ.Enabled = false;
            cmbDelayEqTime.Enabled = false;
            mpSmoothEQ.Enabled = false;
            mpEQTitleDisplay.Enabled = false;
            mpLabelEQTitleDisplay.Enabled = false;
            cmbEQTitleDisplayTime.Enabled = false;
            cmbEQTitleShowTime.Enabled = false;
          }
          mpProgressBar.Enabled = false;
          mpUseCustomFont.Enabled = false;
          mpEditFont.Enabled = false;
          mpUseCustomIcons.Enabled = false;
          mpEditIcon.Enabled = false;
          mpUseInvertedIcons.Enabled = false;
          mpUseLargeIcons.Enabled = false;
          mpVolumeDisplay.Enabled = false;
          ckDiskIcon.Enabled = false;
          ckDiskMediaStatus.Enabled = false;
          ckDeviceMonitor.Enabled = false;
          break;
      }
    }

    private void InitializeComponent()
    {
      groupBoxConfiguration = new MPGroupBox();
      groupBox1 = new GroupBox();
      mpMonitorPowerState = new MPCheckBox();
      groupBoxManager = new GroupBox();
      mpLeaveFrontviewActive = new MPCheckBox();
      mpForceManagerReload = new MPCheckBox();
      mpRestartFrontview = new MPCheckBox();
      mpForceKeyBoardMode = new MPCheckBox();
      mpForceManagerRestart = new MPCheckBox();
      mpEnsureManagerStartup = new MPCheckBox();
      groupBoxHardware = new GroupBox();
      mpDelayStartup = new MPCheckBox();
      label1 = new MPLabel();
      cmbType = new MPComboBox();
      mpVFD_UseV3DLL = new MPCheckBox();
      groupBoxDisplayControl = new GroupBox();
      cmbBlankIdleTime = new MPComboBox();
      mpEnableDisplayActionTime = new MPComboBox();
      mpEnableDisplayAction = new MPCheckBox();
      mpBlankDisplayWithVideo = new MPCheckBox();
      mpBlankDisplayWhenIdle = new MPCheckBox();
      groupboxEqualizerOptions = new GroupBox();
      cmbDelayEqTime = new MPComboBox();
      groupEQstyle = new GroupBox();
      mpUseVUmeter2 = new RadioButton();
      cbVUindicators = new CheckBox();
      mpUseVUmeter = new RadioButton();
      mpUseStereoEQ = new RadioButton();
      mpNormalEQ = new RadioButton();
      cmbEqMode = new MPComboBox();
      mpLabelEQmode = new MPLabel();
      mpLabel2 = new MPLabel();
      cmbEQTitleDisplayTime = new MPComboBox();
      mpLabelEQTitleDisplay = new MPLabel();
      cmbEQTitleShowTime = new MPComboBox();
      mpEQTitleDisplay = new MPCheckBox();
      mpSmoothEQ = new MPCheckBox();
      mpEqDisplay = new MPCheckBox();
      mpRestrictEQ = new MPCheckBox();
      cmbEqRate = new MPComboBox();
      mpDelayEQ = new MPCheckBox();
      groupBoxDisplayOptions = new GroupBox();
      ckDiskIcon = new MPCheckBox();
      ckDiskMediaStatus = new MPCheckBox();
      ckDeviceMonitor = new MPCheckBox();
      mpVolumeDisplay = new MPCheckBox();
      mpProgressBar = new MPCheckBox();
      mpEditFont = new MPButton();
      mpEditIcon = new MPButton();
      mpUseCustomFont = new MPCheckBox();
      mpUseLargeIcons = new MPCheckBox();
      mpUseCustomIcons = new MPCheckBox();
      mpUseInvertedIcons = new MPCheckBox();
      btnOK = new MPButton();
      btnReset = new MPButton();
      groupBoxConfiguration.SuspendLayout();
      groupBox1.SuspendLayout();
      groupBoxManager.SuspendLayout();
      groupBoxHardware.SuspendLayout();
      groupBoxDisplayControl.SuspendLayout();
      groupboxEqualizerOptions.SuspendLayout();
      groupEQstyle.SuspendLayout();
      groupBoxDisplayOptions.SuspendLayout();
      SuspendLayout();
      // 
      // groupBoxConfiguration
      // 
      groupBoxConfiguration.Anchor = (((((AnchorStyles.Top | AnchorStyles.Bottom)
                                         | AnchorStyles.Left)
                                        | AnchorStyles.Right)));
      groupBoxConfiguration.Controls.Add(groupBox1);
      groupBoxConfiguration.Controls.Add(groupBoxManager);
      groupBoxConfiguration.Controls.Add(groupBoxHardware);
      groupBoxConfiguration.Controls.Add(groupBoxDisplayControl);
      groupBoxConfiguration.Controls.Add(groupboxEqualizerOptions);
      groupBoxConfiguration.Controls.Add(groupBoxDisplayOptions);
      groupBoxConfiguration.FlatStyle = FlatStyle.Popup;
      groupBoxConfiguration.Location = new Point(9, 6);
      groupBoxConfiguration.Name = "groupBoxConfiguration";
      groupBoxConfiguration.Size = new Size(632, 464);
      groupBoxConfiguration.TabIndex = 4;
      groupBoxConfiguration.TabStop = false;
      groupBoxConfiguration.Text = "Configuration";
      // 
      // groupBox1
      // 
      groupBox1.Controls.Add(mpMonitorPowerState);
      groupBox1.Location = new Point(321, 373);
      groupBox1.Name = "groupBox1";
      groupBox1.Size = new Size(301, 30);
      groupBox1.TabIndex = 134;
      groupBox1.TabStop = false;
      // 
      // mpMonitorPowerState
      // 
      mpMonitorPowerState.AutoSize = true;
      mpMonitorPowerState.FlatStyle = FlatStyle.Popup;
      mpMonitorPowerState.Location = new Point(13, 9);
      mpMonitorPowerState.Name = "mpMonitorPowerState";
      mpMonitorPowerState.Size = new Size(153, 17);
      mpMonitorPowerState.TabIndex = 80;
      mpMonitorPowerState.Text = "Monitor PowerState Events";
      mpMonitorPowerState.UseVisualStyleBackColor = true;
      // 
      // groupBoxManager
      // 
      groupBoxManager.Controls.Add(mpLeaveFrontviewActive);
      groupBoxManager.Controls.Add(mpForceManagerReload);
      groupBoxManager.Controls.Add(mpRestartFrontview);
      groupBoxManager.Controls.Add(mpForceKeyBoardMode);
      groupBoxManager.Controls.Add(mpForceManagerRestart);
      groupBoxManager.Controls.Add(mpEnsureManagerStartup);
      groupBoxManager.Location = new Point(9, 116);
      groupBoxManager.Name = "groupBoxManager";
      groupBoxManager.Size = new Size(306, 146);
      groupBoxManager.TabIndex = 133;
      groupBoxManager.TabStop = false;
      groupBoxManager.Text = "Antec/iMON Manager";
      // 
      // mpLeaveFrontviewActive
      // 
      mpLeaveFrontviewActive.AutoSize = true;
      mpLeaveFrontviewActive.FlatStyle = FlatStyle.Popup;
      mpLeaveFrontviewActive.Location = new Point(14, 93);
      mpLeaveFrontviewActive.Name = "mpLeaveFrontviewActive";
      mpLeaveFrontviewActive.Size = new Size(136, 17);
      mpLeaveFrontviewActive.TabIndex = 131;
      mpLeaveFrontviewActive.Text = "Leave FrontView active";
      mpLeaveFrontviewActive.UseVisualStyleBackColor = true;
      // 
      // mpForceManagerReload
      // 
      mpForceManagerReload.AutoSize = true;
      mpForceManagerReload.FlatStyle = FlatStyle.Popup;
      mpForceManagerReload.Location = new Point(14, 53);
      mpForceManagerReload.Name = "mpForceManagerReload";
      mpForceManagerReload.Size = new Size(167, 17);
      mpForceManagerReload.TabIndex = 130;
      mpForceManagerReload.Text = "Force reload during driver start";
      mpForceManagerReload.UseVisualStyleBackColor = true;
      // 
      // mpRestartFrontview
      // 
      mpRestartFrontview.AutoSize = true;
      mpRestartFrontview.FlatStyle = FlatStyle.Popup;
      mpRestartFrontview.Location = new Point(14, 73);
      mpRestartFrontview.Name = "mpRestartFrontview";
      mpRestartFrontview.Size = new Size(142, 17);
      mpRestartFrontview.TabIndex = 128;
      mpRestartFrontview.Text = "Restart FrontView on exit";
      mpRestartFrontview.UseVisualStyleBackColor = true;
      // 
      // mpForceKeyBoardMode
      // 
      mpForceKeyBoardMode.AutoSize = true;
      mpForceKeyBoardMode.FlatStyle = FlatStyle.Popup;
      mpForceKeyBoardMode.Location = new Point(14, 116);
      mpForceKeyBoardMode.Name = "mpForceKeyBoardMode";
      mpForceKeyBoardMode.Size = new Size(235, 17);
      mpForceKeyBoardMode.TabIndex = 129;
      mpForceKeyBoardMode.Text = "Force to use KeyBoard mode with iMON Pad";
      mpForceKeyBoardMode.UseVisualStyleBackColor = true;
      // 
      // mpForceManagerRestart
      // 
      mpForceManagerRestart.AutoSize = true;
      mpForceManagerRestart.FlatStyle = FlatStyle.Popup;
      mpForceManagerRestart.Location = new Point(14, 33);
      mpForceManagerRestart.Name = "mpForceManagerRestart";
      mpForceManagerRestart.Size = new Size(159, 17);
      mpForceManagerRestart.TabIndex = 127;
      mpForceManagerRestart.Text = "Force restart after driver start";
      mpForceManagerRestart.UseVisualStyleBackColor = true;
      // 
      // mpEnsureManagerStartup
      // 
      mpEnsureManagerStartup.AutoSize = true;
      mpEnsureManagerStartup.FlatStyle = FlatStyle.Popup;
      mpEnsureManagerStartup.Location = new Point(14, 13);
      mpEnsureManagerStartup.Name = "mpEnsureManagerStartup";
      mpEnsureManagerStartup.Size = new Size(190, 17);
      mpEnsureManagerStartup.TabIndex = 126;
      mpEnsureManagerStartup.Text = "Ensure is running before driver start";
      mpEnsureManagerStartup.UseVisualStyleBackColor = true;
      // 
      // groupBoxHardware
      // 
      groupBoxHardware.Controls.Add(mpDelayStartup);
      groupBoxHardware.Controls.Add(label1);
      groupBoxHardware.Controls.Add(cmbType);
      groupBoxHardware.Controls.Add(mpVFD_UseV3DLL);
      groupBoxHardware.Location = new Point(10, 15);
      groupBoxHardware.Name = "groupBoxHardware";
      groupBoxHardware.Size = new Size(306, 95);
      groupBoxHardware.TabIndex = 132;
      groupBoxHardware.TabStop = false;
      groupBoxHardware.Text = " Hardware Options ";
      // 
      // mpDelayStartup
      // 
      mpDelayStartup.AutoSize = true;
      mpDelayStartup.FlatStyle = FlatStyle.Popup;
      mpDelayStartup.Location = new Point(13, 44);
      mpDelayStartup.Name = "mpDelayStartup";
      mpDelayStartup.Size = new Size(265, 17);
      mpDelayStartup.TabIndex = 126;
      mpDelayStartup.Text = "Delay driver initialization (Problematic USB devices)";
      mpDelayStartup.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      label1.Location = new Point(16, 18);
      label1.Name = "label1";
      label1.Size = new Size(73, 23);
      label1.TabIndex = 13;
      label1.Text = "Display Type";
      label1.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // cmbType
      // 
      cmbType.Anchor = ((((AnchorStyles.Top | AnchorStyles.Left)
                          | AnchorStyles.Right)));
      cmbType.BorderColor = Color.Empty;
      cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
      cmbType.Items.AddRange(new object[]
                               {
                                 "AutoDetect",
                                 "LCD",
                                 "LCD2",
                                 "VFD",
                                 "LCD3R"
                               });
      cmbType.Location = new Point(95, 18);
      cmbType.Name = "cmbType";
      cmbType.Size = new Size(195, 21);
      cmbType.TabIndex = 12;
      cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;
      // 
      // mpVFD_UseV3DLL
      // 
      mpVFD_UseV3DLL.AutoSize = true;
      mpVFD_UseV3DLL.FlatStyle = FlatStyle.Popup;
      mpVFD_UseV3DLL.Location = new Point(13, 67);
      mpVFD_UseV3DLL.Name = "mpVFD_UseV3DLL";
      mpVFD_UseV3DLL.Size = new Size(234, 17);
      mpVFD_UseV3DLL.TabIndex = 77;
      mpVFD_UseV3DLL.Text = "Use SG_VFD.dll  V3  (VFD with old firmware)";
      mpVFD_UseV3DLL.UseVisualStyleBackColor = true;
      mpVFD_UseV3DLL.CheckedChanged += mpVFD_UseV3DLL_CheckedChanged;
      // 
      // groupBoxDisplayControl
      // 
      groupBoxDisplayControl.Anchor = ((((AnchorStyles.Top | AnchorStyles.Left)
                                         | AnchorStyles.Right)));
      groupBoxDisplayControl.Controls.Add(cmbBlankIdleTime);
      groupBoxDisplayControl.Controls.Add(mpEnableDisplayActionTime);
      groupBoxDisplayControl.Controls.Add(mpEnableDisplayAction);
      groupBoxDisplayControl.Controls.Add(mpBlankDisplayWithVideo);
      groupBoxDisplayControl.Controls.Add(mpBlankDisplayWhenIdle);
      groupBoxDisplayControl.Location = new Point(322, 15);
      groupBoxDisplayControl.Name = "groupBoxDisplayControl";
      groupBoxDisplayControl.Size = new Size(300, 82);
      groupBoxDisplayControl.TabIndex = 124;
      groupBoxDisplayControl.TabStop = false;
      groupBoxDisplayControl.Text = " Display Control Options ";
      // 
      // cmbBlankIdleTime
      // 
      cmbBlankIdleTime.BorderColor = Color.Empty;
      cmbBlankIdleTime.DropDownStyle = ComboBoxStyle.DropDownList;
      cmbBlankIdleTime.Items.AddRange(new object[]
                                        {
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
                                          "30"
                                        });
      cmbBlankIdleTime.Location = new Point(174, 55);
      cmbBlankIdleTime.Name = "cmbBlankIdleTime";
      cmbBlankIdleTime.Size = new Size(42, 21);
      cmbBlankIdleTime.TabIndex = 98;
      // 
      // mpEnableDisplayActionTime
      // 
      mpEnableDisplayActionTime.BorderColor = Color.Empty;
      mpEnableDisplayActionTime.DropDownStyle = ComboBoxStyle.DropDownList;
      mpEnableDisplayActionTime.Items.AddRange(new object[]
                                                 {
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
                                                   "20"
                                                 });
      mpEnableDisplayActionTime.Location = new Point(188, 33);
      mpEnableDisplayActionTime.Name = "mpEnableDisplayActionTime";
      mpEnableDisplayActionTime.Size = new Size(42, 21);
      mpEnableDisplayActionTime.TabIndex = 96;
      // 
      // mpEnableDisplayAction
      // 
      mpEnableDisplayAction.AutoSize = true;
      mpEnableDisplayAction.FlatStyle = FlatStyle.Popup;
      mpEnableDisplayAction.Location = new Point(30, 35);
      mpEnableDisplayAction.Name = "mpEnableDisplayAction";
      mpEnableDisplayAction.Size = new Size(256, 17);
      mpEnableDisplayAction.TabIndex = 97;
      mpEnableDisplayAction.Text = "Enable Display on Action for                   Seconds";
      mpEnableDisplayAction.UseVisualStyleBackColor = true;
      mpEnableDisplayAction.CheckedChanged += mpEnableDisplayAction_CheckedChanged;
      // 
      // mpBlankDisplayWithVideo
      // 
      mpBlankDisplayWithVideo.AutoSize = true;
      mpBlankDisplayWithVideo.FlatStyle = FlatStyle.Popup;
      mpBlankDisplayWithVideo.Location = new Point(14, 14);
      mpBlankDisplayWithVideo.Name = "mpBlankDisplayWithVideo";
      mpBlankDisplayWithVideo.Size = new Size(205, 17);
      mpBlankDisplayWithVideo.TabIndex = 95;
      mpBlankDisplayWithVideo.Text = "Turn off display during Video Playback";
      mpBlankDisplayWithVideo.UseVisualStyleBackColor = true;
      mpBlankDisplayWithVideo.CheckedChanged += mpBlankDisplayWithVideo_CheckedChanged;
      // 
      // mpBlankDisplayWhenIdle
      // 
      mpBlankDisplayWhenIdle.AutoSize = true;
      mpBlankDisplayWhenIdle.FlatStyle = FlatStyle.Popup;
      mpBlankDisplayWhenIdle.Location = new Point(14, 57);
      mpBlankDisplayWhenIdle.Name = "mpBlankDisplayWhenIdle";
      mpBlankDisplayWhenIdle.Size = new Size(259, 17);
      mpBlankDisplayWhenIdle.TabIndex = 99;
      mpBlankDisplayWhenIdle.Text = "Turn off display when idle for                    seconds";
      mpBlankDisplayWhenIdle.UseVisualStyleBackColor = true;
      mpBlankDisplayWhenIdle.CheckedChanged += mpBlankDisplayWhenIdle_CheckedChanged;
      // 
      // groupboxEqualizerOptions
      // 
      groupboxEqualizerOptions.Anchor = ((((AnchorStyles.Top | AnchorStyles.Left)
                                           | AnchorStyles.Right)));
      groupboxEqualizerOptions.Controls.Add(cmbDelayEqTime);
      groupboxEqualizerOptions.Controls.Add(groupEQstyle);
      groupboxEqualizerOptions.Controls.Add(cmbEqMode);
      groupboxEqualizerOptions.Controls.Add(mpLabelEQmode);
      groupboxEqualizerOptions.Controls.Add(mpLabel2);
      groupboxEqualizerOptions.Controls.Add(cmbEQTitleDisplayTime);
      groupboxEqualizerOptions.Controls.Add(mpLabelEQTitleDisplay);
      groupboxEqualizerOptions.Controls.Add(cmbEQTitleShowTime);
      groupboxEqualizerOptions.Controls.Add(mpEQTitleDisplay);
      groupboxEqualizerOptions.Controls.Add(mpSmoothEQ);
      groupboxEqualizerOptions.Controls.Add(mpEqDisplay);
      groupboxEqualizerOptions.Controls.Add(mpRestrictEQ);
      groupboxEqualizerOptions.Controls.Add(cmbEqRate);
      groupboxEqualizerOptions.Controls.Add(mpDelayEQ);
      groupboxEqualizerOptions.Location = new Point(322, 103);
      groupboxEqualizerOptions.Name = "groupboxEqualizerOptions";
      groupboxEqualizerOptions.Size = new Size(300, 259);
      groupboxEqualizerOptions.TabIndex = 123;
      groupboxEqualizerOptions.TabStop = false;
      groupboxEqualizerOptions.Text = " Equalizer Options ";
      // 
      // cmbDelayEqTime
      // 
      cmbDelayEqTime.BorderColor = Color.Empty;
      cmbDelayEqTime.DropDownStyle = ComboBoxStyle.DropDownList;
      cmbDelayEqTime.Items.AddRange(new object[]
                                      {
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
                                        "30"
                                      });
      cmbDelayEqTime.Location = new Point(168, 169);
      cmbDelayEqTime.Name = "cmbDelayEqTime";
      cmbDelayEqTime.Size = new Size(53, 21);
      cmbDelayEqTime.TabIndex = 124;
      // 
      // groupEQstyle
      // 
      groupEQstyle.Anchor = ((((AnchorStyles.Top | AnchorStyles.Left)
                               | AnchorStyles.Right)));
      groupEQstyle.Controls.Add(mpUseVUmeter2);
      groupEQstyle.Controls.Add(cbVUindicators);
      groupEQstyle.Controls.Add(mpUseVUmeter);
      groupEQstyle.Controls.Add(mpUseStereoEQ);
      groupEQstyle.Controls.Add(mpNormalEQ);
      groupEQstyle.Location = new Point(8, 65);
      groupEQstyle.Name = "groupEQstyle";
      groupEQstyle.Size = new Size(285, 60);
      groupEQstyle.TabIndex = 140;
      groupEQstyle.TabStop = false;
      groupEQstyle.Text = " Equalizer Style ";
      // 
      // mpUseVUmeter2
      // 
      mpUseVUmeter2.AutoSize = true;
      mpUseVUmeter2.Location = new Point(204, 17);
      mpUseVUmeter2.Name = "mpUseVUmeter2";
      mpUseVUmeter2.Size = new Size(79, 17);
      mpUseVUmeter2.TabIndex = 121;
      mpUseVUmeter2.Text = "VU Meter 2";
      mpUseVUmeter2.UseVisualStyleBackColor = true;
      mpUseVUmeter2.CheckedChanged += mpUseVUmeter2_CheckedChanged;
      // 
      // cbVUindicators
      // 
      cbVUindicators.AutoSize = true;
      cbVUindicators.Location = new Point(8, 40);
      cbVUindicators.Name = "cbVUindicators";
      cbVUindicators.Size = new Size(213, 17);
      cbVUindicators.TabIndex = 120;
      cbVUindicators.Text = "Show Channel indicators for VU Display";
      cbVUindicators.UseVisualStyleBackColor = true;
      // 
      // mpUseVUmeter
      // 
      mpUseVUmeter.AutoSize = true;
      mpUseVUmeter.Location = new Point(131, 17);
      mpUseVUmeter.Name = "mpUseVUmeter";
      mpUseVUmeter.Size = new Size(70, 17);
      mpUseVUmeter.TabIndex = 2;
      mpUseVUmeter.Text = "VU Meter";
      mpUseVUmeter.UseVisualStyleBackColor = true;
      mpUseVUmeter.CheckedChanged += mpUseVUmeter_CheckedChanged;
      // 
      // mpUseStereoEQ
      // 
      mpUseStereoEQ.AutoSize = true;
      mpUseStereoEQ.Location = new Point(73, 17);
      mpUseStereoEQ.Name = "mpUseStereoEQ";
      mpUseStereoEQ.Size = new Size(56, 17);
      mpUseStereoEQ.TabIndex = 1;
      mpUseStereoEQ.Text = "Stereo";
      mpUseStereoEQ.UseVisualStyleBackColor = true;
      mpUseStereoEQ.CheckedChanged += mpUseStereoEQ_CheckedChanged;
      // 
      // mpNormalEQ
      // 
      mpNormalEQ.AutoSize = true;
      mpNormalEQ.Checked = true;
      mpNormalEQ.Location = new Point(13, 17);
      mpNormalEQ.Name = "mpNormalEQ";
      mpNormalEQ.Size = new Size(58, 17);
      mpNormalEQ.TabIndex = 0;
      mpNormalEQ.TabStop = true;
      mpNormalEQ.Text = "Normal";
      mpNormalEQ.UseVisualStyleBackColor = true;
      mpNormalEQ.CheckedChanged += mpNormalEQ_CheckedChanged;
      // 
      // cmbEqMode
      // 
      cmbEqMode.Anchor = ((((AnchorStyles.Top | AnchorStyles.Left)
                            | AnchorStyles.Right)));
      cmbEqMode.BorderColor = Color.Empty;
      cmbEqMode.DropDownStyle = ComboBoxStyle.DropDownList;
      cmbEqMode.Items.AddRange(new object[]
                                 {
                                   "Up from bottom",
                                   "Down from top",
                                   "Expand from middle"
                                 });
      cmbEqMode.Location = new Point(134, 43);
      cmbEqMode.Name = "cmbEqMode";
      cmbEqMode.Size = new Size(97, 21);
      cmbEqMode.TabIndex = 122;
      // 
      // mpLabelEQmode
      // 
      mpLabelEQmode.Location = new Point(42, 45);
      mpLabelEQmode.Name = "mpLabelEQmode";
      mpLabelEQmode.Size = new Size(95, 17);
      mpLabelEQmode.TabIndex = 136;
      mpLabelEQmode.Text = "EQ Display Mode:";
      mpLabelEQmode.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // mpLabel2
      // 
      mpLabel2.Location = new Point(108, 150);
      mpLabel2.Name = "mpLabel2";
      mpLabel2.Size = new Size(116, 17);
      mpLabel2.TabIndex = 135;
      mpLabel2.Text = "updates per Seconds";
      mpLabel2.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // cmbEQTitleDisplayTime
      // 
      cmbEQTitleDisplayTime.BorderColor = Color.Empty;
      cmbEQTitleDisplayTime.DropDownStyle = ComboBoxStyle.DropDownList;
      cmbEQTitleDisplayTime.Items.AddRange(new object[]
                                             {
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
                                               "30"
                                             });
      cmbEQTitleDisplayTime.Location = new Point(172, 232);
      cmbEQTitleDisplayTime.Name = "cmbEQTitleDisplayTime";
      cmbEQTitleDisplayTime.Size = new Size(49, 21);
      cmbEQTitleDisplayTime.TabIndex = 130;
      // 
      // mpLabelEQTitleDisplay
      // 
      mpLabelEQTitleDisplay.Location = new Point(94, 234);
      mpLabelEQTitleDisplay.Name = "mpLabelEQTitleDisplay";
      mpLabelEQTitleDisplay.Size = new Size(197, 17);
      mpLabelEQTitleDisplay.TabIndex = 134;
      mpLabelEQTitleDisplay.Text = "Seconds every                    Seconds";
      mpLabelEQTitleDisplay.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // cmbEQTitleShowTime
      // 
      cmbEQTitleShowTime.BorderColor = Color.Empty;
      cmbEQTitleShowTime.DropDownStyle = ComboBoxStyle.DropDownList;
      cmbEQTitleShowTime.Items.AddRange(new object[]
                                          {
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
                                            "30"
                                          });
      cmbEQTitleShowTime.Location = new Point(45, 232);
      cmbEQTitleShowTime.Name = "cmbEQTitleShowTime";
      cmbEQTitleShowTime.Size = new Size(49, 21);
      cmbEQTitleShowTime.TabIndex = 133;
      // 
      // mpEQTitleDisplay
      // 
      mpEQTitleDisplay.AutoSize = true;
      mpEQTitleDisplay.FlatStyle = FlatStyle.Popup;
      mpEQTitleDisplay.Location = new Point(29, 213);
      mpEQTitleDisplay.Name = "mpEQTitleDisplay";
      mpEQTitleDisplay.Size = new Size(118, 17);
      mpEQTitleDisplay.TabIndex = 132;
      mpEQTitleDisplay.Text = "Show Track Info for";
      mpEQTitleDisplay.UseVisualStyleBackColor = true;
      mpEQTitleDisplay.CheckedChanged += mpEQTitleDisplay_CheckedChanged;
      // 
      // mpSmoothEQ
      // 
      mpSmoothEQ.AutoSize = true;
      mpSmoothEQ.FlatStyle = FlatStyle.Popup;
      mpSmoothEQ.Location = new Point(29, 192);
      mpSmoothEQ.Name = "mpSmoothEQ";
      mpSmoothEQ.Size = new Size(222, 17);
      mpSmoothEQ.TabIndex = 129;
      mpSmoothEQ.Text = "Use Equalizer Smoothing (Delayed decay)";
      mpSmoothEQ.UseVisualStyleBackColor = true;
      // 
      // mpEqDisplay
      // 
      mpEqDisplay.AutoSize = true;
      mpEqDisplay.FlatStyle = FlatStyle.Popup;
      mpEqDisplay.Location = new Point(13, 24);
      mpEqDisplay.Name = "mpEqDisplay";
      mpEqDisplay.Size = new Size(124, 17);
      mpEqDisplay.TabIndex = 126;
      mpEqDisplay.Text = "Use Equalizer display";
      mpEqDisplay.UseVisualStyleBackColor = true;
      mpEqDisplay.CheckedChanged += mpEqDisplay_CheckedChanged;
      // 
      // mpRestrictEQ
      // 
      mpRestrictEQ.AutoSize = true;
      mpRestrictEQ.FlatStyle = FlatStyle.Popup;
      mpRestrictEQ.Location = new Point(29, 129);
      mpRestrictEQ.Name = "mpRestrictEQ";
      mpRestrictEQ.Size = new Size(183, 17);
      mpRestrictEQ.TabIndex = 127;
      mpRestrictEQ.Text = "Limit Equalizer display update rate";
      mpRestrictEQ.UseVisualStyleBackColor = true;
      mpRestrictEQ.CheckedChanged += mpRestrictEQ_CheckedChanged;
      // 
      // cmbEqRate
      // 
      cmbEqRate.BorderColor = Color.Empty;
      cmbEqRate.DropDownStyle = ComboBoxStyle.DropDownList;
      cmbEqRate.Items.AddRange(new object[]
                                 {
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
                                   "60"
                                 });
      cmbEqRate.Location = new Point(45, 148);
      cmbEqRate.Name = "cmbEqRate";
      cmbEqRate.Size = new Size(57, 21);
      cmbEqRate.TabIndex = 123;
      // 
      // mpDelayEQ
      // 
      mpDelayEQ.AutoSize = true;
      mpDelayEQ.FlatStyle = FlatStyle.Popup;
      mpDelayEQ.Location = new Point(29, 171);
      mpDelayEQ.Name = "mpDelayEQ";
      mpDelayEQ.Size = new Size(247, 17);
      mpDelayEQ.TabIndex = 128;
      mpDelayEQ.Text = "Delay Equalizer Start by                       Seconds";
      mpDelayEQ.UseVisualStyleBackColor = true;
      mpDelayEQ.CheckedChanged += mpDelayEQ_CheckedChanged;
      // 
      // groupBoxDisplayOptions
      // 
      groupBoxDisplayOptions.Anchor = ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                         | AnchorStyles.Left)));
      groupBoxDisplayOptions.Controls.Add(ckDiskIcon);
      groupBoxDisplayOptions.Controls.Add(ckDiskMediaStatus);
      groupBoxDisplayOptions.Controls.Add(ckDeviceMonitor);
      groupBoxDisplayOptions.Controls.Add(mpVolumeDisplay);
      groupBoxDisplayOptions.Controls.Add(mpProgressBar);
      groupBoxDisplayOptions.Controls.Add(mpEditFont);
      groupBoxDisplayOptions.Controls.Add(mpEditIcon);
      groupBoxDisplayOptions.Controls.Add(mpUseCustomFont);
      groupBoxDisplayOptions.Controls.Add(mpUseLargeIcons);
      groupBoxDisplayOptions.Controls.Add(mpUseCustomIcons);
      groupBoxDisplayOptions.Controls.Add(mpUseInvertedIcons);
      groupBoxDisplayOptions.Location = new Point(10, 268);
      groupBoxDisplayOptions.Name = "groupBoxDisplayOptions";
      groupBoxDisplayOptions.Size = new Size(306, 188);
      groupBoxDisplayOptions.TabIndex = 130;
      groupBoxDisplayOptions.TabStop = false;
      groupBoxDisplayOptions.Text = "Display Options (Valid only with LCD/LCD2 displays) ";
      // 
      // ckDiskIcon
      // 
      ckDiskIcon.AutoSize = true;
      ckDiskIcon.FlatStyle = FlatStyle.Popup;
      ckDiskIcon.Location = new Point(16, 20);
      ckDiskIcon.Name = "ckDiskIcon";
      ckDiskIcon.Size = new Size(91, 17);
      ckDiskIcon.TabIndex = 73;
      ckDiskIcon.Text = "Use Disk Icon";
      ckDiskIcon.UseVisualStyleBackColor = true;
      ckDiskIcon.CheckedChanged += ckDiskIcon_CheckedChanged;
      // 
      // ckDiskMediaStatus
      // 
      ckDiskMediaStatus.AutoSize = true;
      ckDiskMediaStatus.FlatStyle = FlatStyle.Popup;
      ckDiskMediaStatus.Location = new Point(32, 38);
      ckDiskMediaStatus.Name = "ckDiskMediaStatus";
      ckDiskMediaStatus.Size = new Size(171, 17);
      ckDiskMediaStatus.TabIndex = 73;
      ckDiskMediaStatus.Text = "Display Media Transport Status";
      ckDiskMediaStatus.UseVisualStyleBackColor = true;
      // 
      // ckDeviceMonitor
      // 
      ckDeviceMonitor.AutoSize = true;
      ckDeviceMonitor.FlatStyle = FlatStyle.Popup;
      ckDeviceMonitor.Location = new Point(32, 54);
      ckDeviceMonitor.Name = "ckDeviceMonitor";
      ckDeviceMonitor.Size = new Size(172, 17);
      ckDeviceMonitor.TabIndex = 73;
      ckDeviceMonitor.Text = "Display CD/DVD volume status";
      ckDeviceMonitor.UseVisualStyleBackColor = true;
      // 
      // mpVolumeDisplay
      // 
      mpVolumeDisplay.AutoSize = true;
      mpVolumeDisplay.FlatStyle = FlatStyle.Popup;
      mpVolumeDisplay.Location = new Point(16, 72);
      mpVolumeDisplay.Name = "mpVolumeDisplay";
      mpVolumeDisplay.Size = new Size(171, 17);
      mpVolumeDisplay.TabIndex = 75;
      mpVolumeDisplay.Text = "Use Top Bar as Volume display";
      mpVolumeDisplay.UseVisualStyleBackColor = true;
      // 
      // mpProgressBar
      // 
      mpProgressBar.AutoSize = true;
      mpProgressBar.FlatStyle = FlatStyle.Popup;
      mpProgressBar.Location = new Point(16, 90);
      mpProgressBar.Name = "mpProgressBar";
      mpProgressBar.Size = new Size(193, 17);
      mpProgressBar.TabIndex = 74;
      mpProgressBar.Text = "Use Bottom Bar as Progress Display";
      mpProgressBar.UseVisualStyleBackColor = true;
      // 
      // mpEditFont
      // 
      mpEditFont.Location = new Point(215, 105);
      mpEditFont.Name = "mpEditFont";
      mpEditFont.Size = new Size(63, 23);
      mpEditFont.TabIndex = 79;
      mpEditFont.Text = "&Edit Font";
      mpEditFont.UseVisualStyleBackColor = true;
      mpEditFont.Click += mpEditFont_Click;
      // 
      // mpEditIcon
      // 
      mpEditIcon.Location = new Point(215, 141);
      mpEditIcon.Name = "mpEditIcon";
      mpEditIcon.Size = new Size(63, 23);
      mpEditIcon.TabIndex = 78;
      mpEditIcon.Text = "&Edit Icons";
      mpEditIcon.UseVisualStyleBackColor = true;
      mpEditIcon.Click += mpEditIcon_Click;
      // 
      // mpUseCustomFont
      // 
      mpUseCustomFont.AutoSize = true;
      mpUseCustomFont.FlatStyle = FlatStyle.Popup;
      mpUseCustomFont.Location = new Point(16, 108);
      mpUseCustomFont.Name = "mpUseCustomFont";
      mpUseCustomFont.Size = new Size(105, 17);
      mpUseCustomFont.TabIndex = 77;
      mpUseCustomFont.Text = "Use Custom Font";
      mpUseCustomFont.UseVisualStyleBackColor = true;
      mpUseCustomFont.CheckedChanged += mpUseCustomFont_CheckedChanged;
      // 
      // mpUseLargeIcons
      // 
      mpUseLargeIcons.AutoSize = true;
      mpUseLargeIcons.FlatStyle = FlatStyle.Popup;
      mpUseLargeIcons.Location = new Point(16, 126);
      mpUseLargeIcons.Name = "mpUseLargeIcons";
      mpUseLargeIcons.Size = new Size(102, 17);
      mpUseLargeIcons.TabIndex = 77;
      mpUseLargeIcons.Text = "Use Large Icons";
      mpUseLargeIcons.UseVisualStyleBackColor = true;
      mpUseLargeIcons.CheckedChanged += mpUseLargeIcons_CheckedChanged;
      // 
      // mpUseCustomIcons
      // 
      mpUseCustomIcons.AutoSize = true;
      mpUseCustomIcons.FlatStyle = FlatStyle.Popup;
      mpUseCustomIcons.Location = new Point(32, 144);
      mpUseCustomIcons.Name = "mpUseCustomIcons";
      mpUseCustomIcons.Size = new Size(140, 17);
      mpUseCustomIcons.TabIndex = 77;
      mpUseCustomIcons.Text = "Use Custom Large Icons";
      mpUseCustomIcons.UseVisualStyleBackColor = true;
      mpUseCustomIcons.CheckedChanged += mpUseCustomIcons_CheckedChanged;
      // 
      // mpUseInvertedIcons
      // 
      mpUseInvertedIcons.AutoSize = true;
      mpUseInvertedIcons.FlatStyle = FlatStyle.Popup;
      mpUseInvertedIcons.Location = new Point(32, 162);
      mpUseInvertedIcons.Name = "mpUseInvertedIcons";
      mpUseInvertedIcons.Size = new Size(172, 17);
      mpUseInvertedIcons.TabIndex = 77;
      mpUseInvertedIcons.Text = "Invert (reverse) the Large Icons";
      mpUseInvertedIcons.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      btnOK.Anchor = (((AnchorStyles.Bottom | AnchorStyles.Right)));
      btnOK.Location = new Point(563, 476);
      btnOK.Name = "btnOK";
      btnOK.Size = new Size(78, 23);
      btnOK.TabIndex = 6;
      btnOK.Text = "&OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += btnOK_Click;
      // 
      // btnReset
      // 
      btnReset.Anchor = (((AnchorStyles.Bottom | AnchorStyles.Right)));
      btnReset.Location = new Point(479, 476);
      btnReset.Name = "btnReset";
      btnReset.Size = new Size(78, 23);
      btnReset.TabIndex = 6;
      btnReset.Text = "&RESET";
      btnReset.UseVisualStyleBackColor = true;
      btnReset.Click += btnReset_Click;
      // 
      // iMONLCDg_AdvancedSetupForm
      // 
      AutoScaleDimensions = new SizeF(6F, 13F);
      ClientSize = new Size(650, 504);
      Controls.Add(btnOK);
      Controls.Add(btnReset);
      Controls.Add(groupBoxConfiguration);
      Name = "iMONLCDg_AdvancedSetupForm";
      StartPosition = FormStartPosition.CenterParent;
      Text = "MiniDisplay - Setup - Advanced Settings";
      groupBoxConfiguration.ResumeLayout(false);
      groupBox1.ResumeLayout(false);
      groupBox1.PerformLayout();
      groupBoxManager.ResumeLayout(false);
      groupBoxManager.PerformLayout();
      groupBoxHardware.ResumeLayout(false);
      groupBoxHardware.PerformLayout();
      groupBoxDisplayControl.ResumeLayout(false);
      groupBoxDisplayControl.PerformLayout();
      groupboxEqualizerOptions.ResumeLayout(false);
      groupboxEqualizerOptions.PerformLayout();
      groupEQstyle.ResumeLayout(false);
      groupEQstyle.PerformLayout();
      groupBoxDisplayOptions.ResumeLayout(false);
      groupBoxDisplayOptions.PerformLayout();
      ResumeLayout(false);
    }

    private void ckDiskIcon_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpBlankDisplayWhenIdle_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpBlankDisplayWithVideo_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpDelayEQ_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpEditFont_Click(object sender, EventArgs e)
    {
      Form form = new iMONLCDg_FontEdit();
      Visible = false;
      form.ShowDialog();
      form.Dispose();
      Visible = true;
    }

    private void mpEditIcon_Click(object sender, EventArgs e)
    {
      Form form = new iMONLCDg_IconEdit();
      Visible = false;
      form.ShowDialog();
      form.Dispose();
      Visible = true;
    }

    private void mpEnableDisplayAction_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpEqDisplay_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpEQTitleDisplay_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpNormalEQ_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpRestrictEQ_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpUseCustomFont_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpUseCustomIcons_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpUseLargeIcons_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpUseStereoEQ_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpUseVUmeter_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpUseVUmeter2_CheckedChanged(object sender, EventArgs e)
    {
      cmbType_Changed();
    }

    private void mpVFD_UseV3DLL_CheckedChanged(object sender, EventArgs e)
    {
      if (mpVFD_UseV3DLL.Checked)
      {
        cmbType.SelectedIndex = 3;
        cmbType.SelectedValue = 3;
        cmbType.Enabled = false;
        cmbType.Refresh();
      }
      else
      {
        cmbType.Enabled = true;
        cmbType.Refresh();
      }
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