#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

#region Usings

using System;
using System.Windows.Forms;
using TvEngine.PowerScheduler.Interfaces;
#if SERVER
using TvEngine.PowerScheduler;
#endif
#if CLIENT
using MediaPortal.Plugins.Process;
#endif

#endregion

namespace PowerScheduler.Setup
{
  public partial class PowerSettingsForm : Form
  {

    #region Variables

    private const bool AC = true;
    private const bool DC = false;

    public struct PowerSettings
    {
      public bool allowAwayMode;
      public bool requirePassword;
      public bool hybridSleep;
      public bool allowWakeTimers;
      public uint idleTimeout;
      public uint hibernateAfter;
      public uint lidCloseAction;
      public uint powerButtonAction;
      public uint sleepButtonAction;
      public uint whenSharingMedia;
    };

    // Power settings
    private PowerSettings _powerSettingsAC = new PowerSettings();
    private PowerSettings _powerSettingsDC = new PowerSettings();

    private PowerSettings _recommendedSettingsAC = new PowerSettings();
    private PowerSettings _recommendedSettingsDC = new PowerSettings();

    #endregion

    #region Public Methods

    /// <summary>
    /// Init power settings form
    /// </summary>
    /// <param name="selectedProfile"></param>
    public PowerSettingsForm(PowerSettings recommendedSettingsAC, PowerSettings recommendedSettingsDC)
    {
      InitializeComponent();

      // Show DC settings only if there is a DC power source
      if (PowerManager.HasDCPowerSource)
      {
        this.Width = 656; // needs more space for DC options

        checkBoxAllowAwayModeAC.Text += " on AC";
        checkBoxRequirePasswordAC.Text += " on AC";
        checkBoxHybridSleepAC.Text += " on AC";
        labelIdleTimeoutAC2.Text += " on AC";
        labelHibernateAfterAC2.Text += " on AC";
        checkBoxAllowWakeTimersAC.Text += " on AC";
        labelLidCloseActionAC.Text += " on AC";
        labelPowerButtonActionAC.Text += " on AC";
        labelSleepButtonActionAC.Text += " on AC";
        labelWhenSharingMediaAC.Text += " on AC";

        checkBoxAllowAwayModeDC.Visible = true;
        checkBoxRequirePasswordDC.Visible = true;
        checkBoxHybridSleepDC.Visible = true;
        labelIdleTimeoutDC1.Visible = true;
        numericUpDownIdleTimeoutDC.Visible = true;
        labelIdleTimeoutDC2.Visible = true;
        labelHibernateAfterDC1.Visible = true;
        numericUpDownHibernateAfterDC.Visible = true;
        labelHibernateAfterDC2.Visible = true;
        checkBoxAllowWakeTimersDC.Visible = true;
        labelLidCloseActionDC.Visible = true;
        comboBoxLidCloseActionDC.Visible = true;
        labelPowerButtonActionDC.Visible = true;
        comboBoxPowerButtonActionDC.Visible = true;
        labelSleepButtonActionDC.Visible = true;
        comboBoxSleepButtonActionDC.Visible = true;
        labelWhenSharingMediaDC.Visible = true;
        comboBoxWhenSharingMediaDC.Visible = true;
      }

      // Show lid close options only if there is a lid
      if (!PowerManager.HasLid)
      {
        labelLidCloseActionAC.Visible = false;
        comboBoxLidCloseActionAC.Visible = false;
        labelLidCloseActionDC.Visible = false;
        comboBoxLidCloseActionDC.Visible = false;
      }

      // Show hibernate options only if system can hibernate
      if (!PowerManager.CanHibernate)
      {
        labelHibernateAfterAC1.Visible = false;
        numericUpDownHibernateAfterAC.Visible = false;
        labelHibernateAfterAC2.Visible = false;
        labelHibernateAfterDC1.Visible = false;
        numericUpDownHibernateAfterDC.Visible = false;
        labelHibernateAfterDC2.Visible = false;

        checkBoxHybridSleepAC.Visible = false;
        checkBoxHybridSleepDC.Visible = false;
      }

      // Hide unused options and modify button actions for Windows XP
      if (Environment.OSVersion.Version.Major < 6)
      {
        checkBoxAllowAwayModeAC.Visible = false;
        checkBoxAllowAwayModeDC.Visible = false;

        checkBoxRequirePasswordAC.Visible = false;
        checkBoxRequirePasswordDC.Visible = false;

        checkBoxHybridSleepAC.Visible = false;
        checkBoxHybridSleepDC.Visible = false;

        checkBoxAllowWakeTimersAC.Visible = false;
        checkBoxAllowWakeTimersDC.Visible = false;

        comboBoxLidCloseActionAC.Items.Add("Ask User");
        comboBoxLidCloseActionDC.Items.Add("Ask User");
        comboBoxPowerButtonActionAC.Items.Add("Ask User");
        comboBoxPowerButtonActionDC.Items.Add("Ask User");
        comboBoxSleepButtonActionAC.Items.Add("Ask User");
        comboBoxSleepButtonActionDC.Items.Add("Ask User");

        labelWhenSharingMediaAC.Visible = false;
        comboBoxWhenSharingMediaAC.Visible = false;
        labelWhenSharingMediaDC.Visible = false;
        comboBoxWhenSharingMediaDC.Visible = false;
      }

      _recommendedSettingsAC = recommendedSettingsAC;
      _recommendedSettingsDC = recommendedSettingsDC;

      LoadSystemSettings();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Load power setting values from system settings to form
    /// </summary>
    private void LoadSystemSettings()
    {
      try
      {
        // AC settings
        _powerSettingsAC.allowAwayMode = PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_AWAY_MODE, AC) == 1;
        checkBoxAllowAwayModeAC.Checked = _powerSettingsAC.allowAwayMode;
        checkBoxAllowAwayModeAC.ForeColor = System.Drawing.SystemColors.ControlText;

        _powerSettingsAC.requirePassword = PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.LOCK_CONSOLE_ON_WAKE, AC) == 1;
        checkBoxRequirePasswordAC.Checked = _powerSettingsAC.requirePassword;
        checkBoxRequirePasswordAC.ForeColor = System.Drawing.SystemColors.ControlText;

        _powerSettingsAC.hybridSleep = PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_HYBRID_SLEEP, AC) == 1;
        checkBoxHybridSleepAC.Checked = _powerSettingsAC.hybridSleep;
        checkBoxHybridSleepAC.ForeColor = System.Drawing.SystemColors.ControlText;

        _powerSettingsAC.allowWakeTimers = PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_RTC_WAKE, AC) == 1;
        checkBoxAllowWakeTimersAC.Checked = _powerSettingsAC.allowWakeTimers;
        checkBoxAllowWakeTimersAC.ForeColor = System.Drawing.SystemColors.ControlText;

        _powerSettingsAC.idleTimeout = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.STANDBYIDLE, AC) / 60;
        numericUpDownIdleTimeoutAC.Value = (int)_powerSettingsAC.idleTimeout;
        numericUpDownIdleTimeoutAC.ForeColor = System.Drawing.SystemColors.ControlText;
        labelIdleTimeoutAC1.ForeColor = System.Drawing.SystemColors.ControlText;
        labelIdleTimeoutAC2.ForeColor = System.Drawing.SystemColors.ControlText;

        _powerSettingsAC.hibernateAfter = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.HIBERNATE_AFTER, AC) / 60;
        numericUpDownHibernateAfterAC.Value = (int)_powerSettingsAC.hibernateAfter;
        numericUpDownHibernateAfterAC.ForeColor = System.Drawing.SystemColors.ControlText;
        labelHibernateAfterAC1.ForeColor = System.Drawing.SystemColors.ControlText;
        labelHibernateAfterAC2.ForeColor = System.Drawing.SystemColors.ControlText;

        _powerSettingsAC.lidCloseAction = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.LID_CLOSE_ACTION, AC);
        comboBoxLidCloseActionAC.SelectedIndex = (int)_powerSettingsAC.lidCloseAction;
        labelLidCloseActionAC.ForeColor = System.Drawing.SystemColors.ControlText;

        _powerSettingsAC.powerButtonAction = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.POWER_BUTTON_ACTION, AC);
        comboBoxPowerButtonActionAC.SelectedIndex = (int)_powerSettingsAC.powerButtonAction;
        labelPowerButtonActionAC.ForeColor = System.Drawing.SystemColors.ControlText;

        _powerSettingsAC.sleepButtonAction = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.SLEEP_BUTTON_ACTION, AC);
        comboBoxSleepButtonActionAC.SelectedIndex = (int)_powerSettingsAC.sleepButtonAction;
        labelSleepButtonActionAC.ForeColor = System.Drawing.SystemColors.ControlText;

        _powerSettingsAC.whenSharingMedia = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.WHEN_SHARING_MEDIA, AC);
        comboBoxWhenSharingMediaAC.SelectedIndex = (int)_powerSettingsAC.whenSharingMedia;
        labelWhenSharingMediaAC.ForeColor = System.Drawing.SystemColors.ControlText;


        // DC settings
        if (PowerManager.HasDCPowerSource)
        {
          _powerSettingsDC.allowAwayMode = PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_AWAY_MODE, DC) == 1;
          checkBoxAllowAwayModeDC.Checked = _powerSettingsDC.allowAwayMode;
          checkBoxAllowAwayModeDC.ForeColor = System.Drawing.SystemColors.ControlText;

          _powerSettingsDC.requirePassword = PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.LOCK_CONSOLE_ON_WAKE, DC) == 1;
          checkBoxRequirePasswordDC.Checked = _powerSettingsDC.requirePassword;
          checkBoxRequirePasswordDC.ForeColor = System.Drawing.SystemColors.ControlText;

          _powerSettingsDC.hybridSleep = PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_HYBRID_SLEEP, DC) == 1;
          checkBoxHybridSleepDC.Checked = _powerSettingsDC.hybridSleep;
          checkBoxHybridSleepDC.ForeColor = System.Drawing.SystemColors.ControlText;

          _powerSettingsDC.allowWakeTimers = PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_RTC_WAKE, DC) == 1;
          checkBoxAllowWakeTimersDC.Checked = _powerSettingsDC.allowWakeTimers;
          checkBoxAllowWakeTimersDC.ForeColor = System.Drawing.SystemColors.ControlText;

          _powerSettingsDC.idleTimeout = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.STANDBYIDLE, DC) / 60;
          numericUpDownIdleTimeoutDC.Value = _powerSettingsDC.idleTimeout;
          numericUpDownIdleTimeoutDC.ForeColor = System.Drawing.SystemColors.ControlText;
          labelIdleTimeoutDC1.ForeColor = System.Drawing.SystemColors.ControlText;
          labelIdleTimeoutDC2.ForeColor = System.Drawing.SystemColors.ControlText;

          _powerSettingsDC.hibernateAfter = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.HIBERNATE_AFTER, DC) / 60;
          numericUpDownHibernateAfterDC.Value = _powerSettingsDC.hibernateAfter;
          numericUpDownHibernateAfterDC.ForeColor = System.Drawing.SystemColors.ControlText;
          labelHibernateAfterDC1.ForeColor = System.Drawing.SystemColors.ControlText;
          labelHibernateAfterDC2.ForeColor = System.Drawing.SystemColors.ControlText;

          _powerSettingsDC.lidCloseAction = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.LID_CLOSE_ACTION, DC);
          comboBoxLidCloseActionDC.SelectedIndex =(int)_powerSettingsDC.lidCloseAction;
          labelLidCloseActionDC.ForeColor = System.Drawing.SystemColors.ControlText;

          _powerSettingsDC.powerButtonAction = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.POWER_BUTTON_ACTION, DC);
          comboBoxPowerButtonActionDC.SelectedIndex = (int)_powerSettingsDC.powerButtonAction;
          labelPowerButtonActionDC.ForeColor = System.Drawing.SystemColors.ControlText;

          _powerSettingsDC.sleepButtonAction = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.SLEEP_BUTTON_ACTION, DC);
          comboBoxSleepButtonActionDC.SelectedIndex = (int)_powerSettingsDC.sleepButtonAction;
          labelSleepButtonActionDC.ForeColor = System.Drawing.SystemColors.ControlText;

          _powerSettingsDC.whenSharingMedia = (uint)PowerManager.GetPowerSetting(PowerManager.SystemPowerSettingType.WHEN_SHARING_MEDIA, DC);
          comboBoxWhenSharingMediaDC.SelectedIndex = (int)_powerSettingsDC.whenSharingMedia;
          labelWhenSharingMediaDC.ForeColor = System.Drawing.SystemColors.ControlText;
        }

      }
      catch (Exception)
      {
        MessageBox.Show("Could not load Windows Power Settings", "Set Windows Power Settings",
          MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
        this.Close();
      }

      buttonReloadSettings.Enabled = false;
    }

    /// <summary>
    /// Load recommended power setting values to form
    /// </summary>
    private void LoadRecommendedSettings()
    {
      checkBoxAllowAwayModeAC.Checked = _recommendedSettingsAC.allowAwayMode;
      checkBoxRequirePasswordAC.Checked = _recommendedSettingsAC.requirePassword;
      checkBoxHybridSleepAC.Checked = _recommendedSettingsAC.hybridSleep;
      checkBoxAllowWakeTimersAC.Checked = _recommendedSettingsAC.allowWakeTimers;
      numericUpDownIdleTimeoutAC.Value = _recommendedSettingsAC.idleTimeout;
      numericUpDownHibernateAfterAC.Value = _recommendedSettingsAC.hibernateAfter;
      comboBoxLidCloseActionAC.SelectedIndex = (int)_recommendedSettingsAC.lidCloseAction;
      comboBoxPowerButtonActionAC.SelectedIndex = (int)_recommendedSettingsAC.powerButtonAction;
      comboBoxSleepButtonActionAC.SelectedIndex = (int)_recommendedSettingsAC.sleepButtonAction;
      comboBoxWhenSharingMediaAC.SelectedIndex = (int)_recommendedSettingsAC.whenSharingMedia;

      checkBoxAllowAwayModeDC.Checked = _recommendedSettingsDC.allowAwayMode;
      checkBoxRequirePasswordDC.Checked = _recommendedSettingsDC.requirePassword;
      checkBoxHybridSleepDC.Checked = _recommendedSettingsDC.hybridSleep;
      checkBoxAllowWakeTimersDC.Checked = _recommendedSettingsDC.allowWakeTimers;
      numericUpDownIdleTimeoutDC.Value = _recommendedSettingsDC.idleTimeout;
      numericUpDownHibernateAfterDC.Value = _recommendedSettingsDC.hibernateAfter;
      comboBoxLidCloseActionDC.SelectedIndex = (int)_recommendedSettingsDC.lidCloseAction;
      comboBoxPowerButtonActionDC.SelectedIndex = (int)_recommendedSettingsDC.powerButtonAction;
      comboBoxSleepButtonActionDC.SelectedIndex = (int)_recommendedSettingsDC.sleepButtonAction;
      comboBoxWhenSharingMediaDC.SelectedIndex = (int)_recommendedSettingsDC.whenSharingMedia;
    }

    /// <summary>
    /// Set system power settings from form values
    /// </summary>
    private void SaveSettings()
    {
      PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_AWAY_MODE, AC, (uint)(checkBoxAllowAwayModeAC.Checked ? 1 : 0));
      PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.LOCK_CONSOLE_ON_WAKE, AC, (uint)(checkBoxRequirePasswordAC.Checked ? 1 : 0));
      PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_HYBRID_SLEEP, AC, (uint)(checkBoxHybridSleepAC.Checked ? 1 : 0));
      PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_RTC_WAKE, AC, (uint)(checkBoxAllowWakeTimersAC.Checked ? 1 : 0));
      PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.STANDBYIDLE, AC, (uint)numericUpDownIdleTimeoutAC.Value * 60);
      PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.HIBERNATE_AFTER, AC, (uint)numericUpDownHibernateAfterAC.Value * 60);
      PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.LID_CLOSE_ACTION, AC, (uint)comboBoxLidCloseActionAC.SelectedIndex);
      PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.POWER_BUTTON_ACTION, AC, (uint)comboBoxPowerButtonActionAC.SelectedIndex);
      PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.SLEEP_BUTTON_ACTION, AC, (uint)comboBoxSleepButtonActionAC.SelectedIndex);
      PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.WHEN_SHARING_MEDIA, AC, (uint)comboBoxWhenSharingMediaAC.SelectedIndex);

      if (PowerManager.HasDCPowerSource)
      {
        PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_AWAY_MODE, DC, (uint)(checkBoxAllowAwayModeDC.Checked ? 1 : 0));
        PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.LOCK_CONSOLE_ON_WAKE, DC, (uint)(checkBoxRequirePasswordDC.Checked ? 1 : 0));
        PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_HYBRID_SLEEP, DC, (uint)(checkBoxHybridSleepDC.Checked ? 1 : 0));
        PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.ALLOW_RTC_WAKE, DC, (uint)(checkBoxAllowWakeTimersDC.Checked ? 1 : 0));
        PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.STANDBYIDLE, DC, (uint)numericUpDownIdleTimeoutDC.Value * 60);
        PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.HIBERNATE_AFTER, DC, (uint)numericUpDownHibernateAfterDC.Value * 60);
        PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.LID_CLOSE_ACTION, DC, (uint)comboBoxLidCloseActionDC.SelectedIndex);
        PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.POWER_BUTTON_ACTION, DC, (uint)comboBoxPowerButtonActionDC.SelectedIndex);
        PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.SLEEP_BUTTON_ACTION, DC, (uint)comboBoxSleepButtonActionDC.SelectedIndex);
        PowerManager.SetPowerSetting(PowerManager.SystemPowerSettingType.WHEN_SHARING_MEDIA, DC, (uint)comboBoxWhenSharingMediaDC.SelectedIndex);
      }
    }

    #endregion

    #region Event Handlers

    private void buttonReloadSettings_Click(object sender, EventArgs e)
    {
      LoadSystemSettings();
    }

    private void buttonRecommendedSettings_Click(object sender, EventArgs e)
    {
      LoadRecommendedSettings();
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      SaveSettings();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void valueChanged(object sender, EventArgs e)
    {
      buttonReloadSettings.Enabled = false;
      buttonRecommendedSettings.Enabled = false;

      // AllowAwayMode
      if (checkBoxAllowAwayModeAC.Checked != _powerSettingsAC.allowAwayMode)
      {
        checkBoxAllowAwayModeAC.ForeColor = System.Drawing.SystemColors.HotTrack;
        buttonReloadSettings.Enabled = true;
      }
      else
        checkBoxAllowAwayModeAC.ForeColor = System.Drawing.SystemColors.ControlText;
      if (checkBoxAllowAwayModeAC.Checked != _recommendedSettingsAC.allowAwayMode)
        buttonRecommendedSettings.Enabled = true;

      // RequirePassword
      if (checkBoxRequirePasswordAC.Checked != _powerSettingsAC.requirePassword)
      {
        checkBoxRequirePasswordAC.ForeColor = System.Drawing.SystemColors.HotTrack;
        buttonReloadSettings.Enabled = true;
      }
      else
        checkBoxRequirePasswordAC.ForeColor = System.Drawing.SystemColors.ControlText;
      if (checkBoxRequirePasswordAC.Checked != _recommendedSettingsAC.requirePassword)
        buttonRecommendedSettings.Enabled = true;

      // HybridSleep
      if (checkBoxHybridSleepAC.Checked != _powerSettingsAC.hybridSleep)
      {
        checkBoxHybridSleepAC.ForeColor = System.Drawing.SystemColors.HotTrack;
        buttonReloadSettings.Enabled = true;
      }
      else
        checkBoxHybridSleepAC.ForeColor = System.Drawing.SystemColors.ControlText;
      if (checkBoxHybridSleepAC.Checked != _recommendedSettingsAC.hybridSleep)
        buttonRecommendedSettings.Enabled = true;

      // IdleTimeout
      if (numericUpDownIdleTimeoutAC.Value != _powerSettingsAC.idleTimeout)
      {
        numericUpDownIdleTimeoutAC.ForeColor = System.Drawing.SystemColors.HotTrack;
        labelIdleTimeoutAC1.ForeColor = System.Drawing.SystemColors.HotTrack;
        labelIdleTimeoutAC2.ForeColor = System.Drawing.SystemColors.HotTrack;
        buttonReloadSettings.Enabled = true;
      }
      else
      {
        numericUpDownIdleTimeoutAC.ForeColor = System.Drawing.SystemColors.ControlText;
        labelIdleTimeoutAC1.ForeColor = System.Drawing.SystemColors.ControlText;
        labelIdleTimeoutAC2.ForeColor = System.Drawing.SystemColors.ControlText;
      }
      if (numericUpDownIdleTimeoutAC.Value != _recommendedSettingsAC.idleTimeout)
        buttonRecommendedSettings.Enabled = true;

      // HibernateAfter
      if (numericUpDownHibernateAfterAC.Value != _powerSettingsAC.hibernateAfter)
      {
        numericUpDownHibernateAfterAC.ForeColor = System.Drawing.SystemColors.HotTrack;
        labelHibernateAfterAC1.ForeColor = System.Drawing.SystemColors.HotTrack;
        labelHibernateAfterAC2.ForeColor = System.Drawing.SystemColors.HotTrack;
        buttonReloadSettings.Enabled = true;
      }
      else
      {
        numericUpDownHibernateAfterAC.ForeColor = System.Drawing.SystemColors.ControlText;
        labelHibernateAfterAC1.ForeColor = System.Drawing.SystemColors.ControlText;
        labelHibernateAfterAC2.ForeColor = System.Drawing.SystemColors.ControlText;
      }
      if (numericUpDownHibernateAfterAC.Value != _recommendedSettingsAC.hibernateAfter)
        buttonRecommendedSettings.Enabled = true;

      // AllowWakeTimers
      if (checkBoxAllowWakeTimersAC.Checked != _powerSettingsAC.allowWakeTimers)
      {
        checkBoxAllowWakeTimersAC.ForeColor = System.Drawing.SystemColors.HotTrack;
        buttonReloadSettings.Enabled = true;
      }
      else
        checkBoxAllowWakeTimersAC.ForeColor = System.Drawing.SystemColors.ControlText;
      if (checkBoxAllowWakeTimersAC.Checked != _recommendedSettingsAC.allowWakeTimers)
        buttonRecommendedSettings.Enabled = true;

      // LidCloseAction
      if (!PowerManager.CanHibernate && comboBoxLidCloseActionAC.SelectedIndex == 3)
        comboBoxLidCloseActionAC.SelectedIndex = 2;
      if (comboBoxLidCloseActionAC.SelectedIndex != _powerSettingsAC.lidCloseAction)
      {
        labelLidCloseActionAC.ForeColor = System.Drawing.SystemColors.HotTrack;
        buttonReloadSettings.Enabled = true;
      }
      else
        labelLidCloseActionAC.ForeColor = System.Drawing.SystemColors.ControlText;
      if (comboBoxLidCloseActionAC.SelectedIndex != _recommendedSettingsAC.lidCloseAction)
        buttonRecommendedSettings.Enabled = true;

      // PowerButtonAction
      if (!PowerManager.CanHibernate && comboBoxPowerButtonActionAC.SelectedIndex == 3)
        comboBoxPowerButtonActionAC.SelectedIndex = 2;
      if (comboBoxPowerButtonActionAC.SelectedIndex != _powerSettingsAC.powerButtonAction)
      {
        labelPowerButtonActionAC.ForeColor = System.Drawing.SystemColors.HotTrack;
        buttonReloadSettings.Enabled = true;
      }
      else
        labelPowerButtonActionAC.ForeColor = System.Drawing.SystemColors.ControlText;
      if (comboBoxPowerButtonActionAC.SelectedIndex != _recommendedSettingsAC.powerButtonAction)
        buttonRecommendedSettings.Enabled = true;

      // SleepButtonAction
      if (!PowerManager.CanHibernate && comboBoxSleepButtonActionAC.SelectedIndex == 3)
        comboBoxSleepButtonActionAC.SelectedIndex = 2;
      if (comboBoxSleepButtonActionAC.SelectedIndex != _powerSettingsAC.sleepButtonAction)
      {
        labelSleepButtonActionAC.ForeColor = System.Drawing.SystemColors.HotTrack;
        buttonReloadSettings.Enabled = true;
      }
      else
        labelSleepButtonActionAC.ForeColor = System.Drawing.SystemColors.ControlText;
      if (comboBoxSleepButtonActionAC.SelectedIndex != _recommendedSettingsAC.sleepButtonAction)
        buttonRecommendedSettings.Enabled = true;

      // WhenSharingMedia
      if (comboBoxWhenSharingMediaAC.SelectedIndex != _powerSettingsAC.whenSharingMedia)
      {
        labelWhenSharingMediaAC.ForeColor = System.Drawing.SystemColors.HotTrack;
        buttonReloadSettings.Enabled = true;
      }
      else
        labelWhenSharingMediaAC.ForeColor = System.Drawing.SystemColors.ControlText;
      if (comboBoxWhenSharingMediaAC.SelectedIndex != _recommendedSettingsAC.whenSharingMedia)
        buttonRecommendedSettings.Enabled = true;

      // Set DC power values only if there is a DC power source
      if (PowerManager.HasDCPowerSource)
      {
        // AllowAwayMode
        if (checkBoxAllowAwayModeDC.Checked != _powerSettingsDC.allowAwayMode)
        {
          checkBoxAllowAwayModeDC.ForeColor = System.Drawing.SystemColors.HotTrack;
          buttonReloadSettings.Enabled = true;
        }
        else
          checkBoxAllowAwayModeDC.ForeColor = System.Drawing.SystemColors.ControlText;
        if (checkBoxAllowAwayModeDC.Checked != _recommendedSettingsDC.allowAwayMode)
          buttonRecommendedSettings.Enabled = true;

        // RequirePassword
        if (checkBoxRequirePasswordDC.Checked != _powerSettingsDC.requirePassword)
        {
          checkBoxRequirePasswordDC.ForeColor = System.Drawing.SystemColors.HotTrack;
          buttonReloadSettings.Enabled = true;
        }
        else
          checkBoxRequirePasswordDC.ForeColor = System.Drawing.SystemColors.ControlText;
        if (checkBoxRequirePasswordDC.Checked != _recommendedSettingsDC.requirePassword)
          buttonRecommendedSettings.Enabled = true;

        // HybridSleep
        if (checkBoxHybridSleepDC.Checked != _powerSettingsDC.hybridSleep)
        {
          checkBoxHybridSleepDC.ForeColor = System.Drawing.SystemColors.HotTrack;
          buttonReloadSettings.Enabled = true;
        }
        else
          checkBoxHybridSleepDC.ForeColor = System.Drawing.SystemColors.ControlText;
        if (checkBoxHybridSleepDC.Checked != _recommendedSettingsDC.hybridSleep)
          buttonRecommendedSettings.Enabled = true;

        // IdleTimeout
        if (numericUpDownIdleTimeoutDC.Value != _powerSettingsDC.idleTimeout)
        {
          numericUpDownIdleTimeoutDC.ForeColor = System.Drawing.SystemColors.HotTrack;
          labelIdleTimeoutDC1.ForeColor = System.Drawing.SystemColors.HotTrack;
          labelIdleTimeoutDC2.ForeColor = System.Drawing.SystemColors.HotTrack;
          buttonReloadSettings.Enabled = true;
        }
        else
        {
          numericUpDownIdleTimeoutDC.ForeColor = System.Drawing.SystemColors.ControlText;
          labelIdleTimeoutDC1.ForeColor = System.Drawing.SystemColors.ControlText;
          labelIdleTimeoutDC2.ForeColor = System.Drawing.SystemColors.ControlText;
        }
        if (numericUpDownIdleTimeoutDC.Value != _recommendedSettingsDC.idleTimeout)
          buttonRecommendedSettings.Enabled = true;

        // HibernateAfter
        if (numericUpDownHibernateAfterDC.Value != _powerSettingsDC.hibernateAfter)
        {
          numericUpDownHibernateAfterDC.ForeColor = System.Drawing.SystemColors.HotTrack;
          labelHibernateAfterDC1.ForeColor = System.Drawing.SystemColors.HotTrack;
          labelHibernateAfterDC2.ForeColor = System.Drawing.SystemColors.HotTrack;
          buttonReloadSettings.Enabled = true;
        }
        else
        {
          numericUpDownHibernateAfterDC.ForeColor = System.Drawing.SystemColors.ControlText;
          labelHibernateAfterDC1.ForeColor = System.Drawing.SystemColors.ControlText;
          labelHibernateAfterDC2.ForeColor = System.Drawing.SystemColors.ControlText;
        }
        if (numericUpDownHibernateAfterDC.Value != _recommendedSettingsDC.hibernateAfter)
          buttonRecommendedSettings.Enabled = true;

        // AllowWakeTimers
        if (checkBoxAllowWakeTimersDC.Checked != _powerSettingsDC.allowWakeTimers)
        {
          checkBoxAllowWakeTimersDC.ForeColor = System.Drawing.SystemColors.HotTrack;
          buttonReloadSettings.Enabled = true;
        }
        else
          checkBoxAllowWakeTimersDC.ForeColor = System.Drawing.SystemColors.ControlText;
        if (checkBoxAllowWakeTimersDC.Checked != _recommendedSettingsDC.allowWakeTimers)
          buttonRecommendedSettings.Enabled = true;

        // LidCloseAction
        if (!PowerManager.CanHibernate && comboBoxLidCloseActionDC.SelectedIndex == 2)
          comboBoxLidCloseActionDC.SelectedIndex = 1;
        if (comboBoxLidCloseActionDC.SelectedIndex != _powerSettingsDC.lidCloseAction)
        {
          labelLidCloseActionDC.ForeColor = System.Drawing.SystemColors.HotTrack;
          buttonReloadSettings.Enabled = true;
        }
        else
          labelLidCloseActionDC.ForeColor = System.Drawing.SystemColors.ControlText;
        if (comboBoxLidCloseActionDC.SelectedIndex != _recommendedSettingsDC.lidCloseAction)
          buttonRecommendedSettings.Enabled = true;

        // PowerButtonAction
        if (!PowerManager.CanHibernate && comboBoxPowerButtonActionDC.SelectedIndex == 2)
          comboBoxPowerButtonActionDC.SelectedIndex = 1;
        if (comboBoxPowerButtonActionDC.SelectedIndex != _powerSettingsDC.powerButtonAction)
        {
          labelPowerButtonActionDC.ForeColor = System.Drawing.SystemColors.HotTrack;
          buttonReloadSettings.Enabled = true;
        }
        else
          labelPowerButtonActionDC.ForeColor = System.Drawing.SystemColors.ControlText;
        if (comboBoxPowerButtonActionDC.SelectedIndex != _recommendedSettingsDC.powerButtonAction)
          buttonRecommendedSettings.Enabled = true;

        // SleepButtonAction
        if (!PowerManager.CanHibernate && comboBoxSleepButtonActionDC.SelectedIndex == 2)
          comboBoxSleepButtonActionDC.SelectedIndex = 1;
        if (comboBoxSleepButtonActionDC.SelectedIndex != _powerSettingsDC.sleepButtonAction)
        {
          labelSleepButtonActionDC.ForeColor = System.Drawing.SystemColors.HotTrack;
          buttonReloadSettings.Enabled = true;
        }
        else
          labelSleepButtonActionDC.ForeColor = System.Drawing.SystemColors.ControlText;
        if (comboBoxSleepButtonActionDC.SelectedIndex != _recommendedSettingsDC.sleepButtonAction)
          buttonRecommendedSettings.Enabled = true;

        // WhenSharingMedia
        if (comboBoxWhenSharingMediaDC.SelectedIndex != _powerSettingsDC.whenSharingMedia)
        {
          labelWhenSharingMediaDC.ForeColor = System.Drawing.SystemColors.HotTrack;
          buttonReloadSettings.Enabled = true;
        }
        else
          labelWhenSharingMediaDC.ForeColor = System.Drawing.SystemColors.ControlText;
        if (comboBoxWhenSharingMediaDC.SelectedIndex != _recommendedSettingsDC.whenSharingMedia)
          buttonRecommendedSettings.Enabled = true;
      }
    }

    #endregion

  }
}

