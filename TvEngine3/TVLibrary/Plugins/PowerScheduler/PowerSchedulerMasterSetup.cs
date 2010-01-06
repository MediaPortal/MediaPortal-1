#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using TvEngine.PowerScheduler.Interfaces;
using SetupTv;
using TvDatabase;

#endregion

namespace TvEngine.PowerScheduler
{
  public partial class PowerSchedulerMasterSetup : SetupTv.SectionSettings
  {
    private TvBusinessLayer _layer;

    public PowerSchedulerMasterSetup()
    {
      InitializeComponent();
      _layer = new TvBusinessLayer();
    }

    public override void LoadSettings()
    {
      Setting setting;
      setting = _layer.GetSetting("PowerSchedulerShutdownActive", "false");
      checkBox1.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerIdleTimeout", "5");
      numericUpDown1.Value = Convert.ToDecimal(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerStandbyAllowedStart", "0");
      numUpDownStandbyAllowedStartHour.Value = Convert.ToDecimal(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerStandbyAllowedEnd", "24");
      numUpDownStandbyAllowedEndHour.Value = Convert.ToDecimal(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerWakeupActive", "false");
      checkBox2.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerShutdownMode", "2");
      comboBox1.SelectedIndex = Convert.ToInt32(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerForceShutdown", "false");
      checkBox3.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerExtensiveLogging", "false");
      checkBox4.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerPreWakeupTime", "60");
      numericUpDown2.Value = Convert.ToDecimal(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerPreNoShutdownTime", "300");
      numericUpDown4.Value = Convert.ToDecimal(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerCheckInterval", "60");
      numericUpDown3.Value = Convert.ToDecimal(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerReinitializeController", "false");
      checkBox5.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerCommand", string.Empty);
      textBox2.Text = setting.Value;

      setting = _layer.GetSetting("PreventStandbyWhenGrabbingEPG", "false");
      checkBox6.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("WakeupSystemForEPGGrabbing", "false");
      checkBox7.Checked = Convert.ToBoolean(setting.Value);


      EPGWakeupConfig config = new EPGWakeupConfig(_layer.GetSetting("EPGWakeupConfig", String.Empty).Value);
      foreach (EPGGrabDays day in config.Days)
      {
        switch (day)
        {
          case EPGGrabDays.Monday:
            checkBox8.Checked = true;
            break;
          case EPGGrabDays.Tuesday:
            checkBox9.Checked = true;
            break;
          case EPGGrabDays.Wednesday:
            checkBox10.Checked = true;
            break;
          case EPGGrabDays.Thursday:
            checkBox11.Checked = true;
            break;
          case EPGGrabDays.Friday:
            checkBox12.Checked = true;
            break;
          case EPGGrabDays.Saturday:
            checkBox13.Checked = true;
            break;
          case EPGGrabDays.Sunday:
            checkBox14.Checked = true;
            break;
        }
      }
      string hFormat, mFormat;
      if (config.Hour < 10)
        hFormat = "0{0}";
      else
        hFormat = "{0}";
      if (config.Minutes < 10)
        mFormat = "0{0}";
      else
        mFormat = "{0}";
      maskedTextBox1.Text = String.Format(hFormat, config.Hour) + ":" + String.Format(mFormat, config.Minutes);

      setting = _layer.GetSetting("PowerSchedulerEpgCommand", String.Empty);
      tbEpgCmd.Text = setting.Value;

      setting = _layer.GetSetting("PowerSchedulerProcesses", "SetupTv, Configuration");
      textBox1.Text = setting.Value;

      setting = _layer.GetSetting("NetworkMonitorEnabled", "false");
      checkBox15.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("NetworkMonitorIdleLimit", "2");
      numericUpDown5.Value = Convert.ToDecimal(setting.Value);
    }

    public override void SaveSettings()
    {
      Setting setting;

      setting = _layer.GetSetting("PowerSchedulerShutdownActive", "false");
      setting.Value = checkBox1.Checked.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerIdleTimeout", "5");
      setting.Value = numericUpDown1.Value.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerStandbyAllowedStart", "0");
      setting.Value = numUpDownStandbyAllowedStartHour.Value.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerStandbyAllowedEnd", "24");
      setting.Value = numUpDownStandbyAllowedEndHour.Value.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerWakeupActive", "false");
      setting.Value = checkBox2.Checked.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerShutdownMode", "2");
      setting.Value = comboBox1.SelectedIndex.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerForceShutdown", "false");
      setting.Value = checkBox3.Checked.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerExtensiveLogging", "false");
      setting.Value = checkBox4.Checked.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerPreWakeupTime", "60");
      setting.Value = numericUpDown2.Value.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerPreNoShutdownTime", "300");
      setting.Value = numericUpDown4.Value.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerCheckInterval", "60");
      setting.Value = numericUpDown3.Value.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerReinitializeController", "false");
      setting.Value = checkBox5.Checked.ToString();
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerCommand", string.Empty);
      setting.Value = textBox2.Text;
      setting.Persist();

      setting = _layer.GetSetting("PreventStandbyWhenGrabbingEPG", "false");
      setting.Value = checkBox6.Checked.ToString();
      setting.Persist();

      setting = _layer.GetSetting("WakeupSystemForEPGGrabbing", "false");
      setting.Value = checkBox7.Checked.ToString();
      setting.Persist();

      setting = _layer.GetSetting("EPGWakeupConfig", String.Empty);
      EPGWakeupConfig cfg = new EPGWakeupConfig(setting.Value);
      EPGWakeupConfig newcfg = new EPGWakeupConfig();
      newcfg.Hour = cfg.Hour;
      newcfg.Minutes = cfg.Minutes;
      // newcfg.Days = cfg.Days;
      newcfg.LastRun = cfg.LastRun;
      string[] time = maskedTextBox1.Text.Split(System.Globalization.DateTimeFormatInfo.CurrentInfo.TimeSeparator[0]);
      newcfg.Hour = Convert.ToInt32(time[0]);
      newcfg.Minutes = Convert.ToInt32(time[1]);
      CheckDay(newcfg, EPGGrabDays.Monday, checkBox8.Checked);
      CheckDay(newcfg, EPGGrabDays.Tuesday, checkBox9.Checked);
      CheckDay(newcfg, EPGGrabDays.Wednesday, checkBox10.Checked);
      CheckDay(newcfg, EPGGrabDays.Thursday, checkBox11.Checked);
      CheckDay(newcfg, EPGGrabDays.Friday, checkBox12.Checked);
      CheckDay(newcfg, EPGGrabDays.Saturday, checkBox13.Checked);
      CheckDay(newcfg, EPGGrabDays.Sunday, checkBox14.Checked);

      if (!cfg.Equals(newcfg))
      {
        setting.Value = newcfg.SerializeAsString();
        setting.Persist();
      }

      setting = _layer.GetSetting("PowerSchedulerEpgCommand", String.Empty);
      setting.Value = tbEpgCmd.Text;
      setting.Persist();

      setting = _layer.GetSetting("PowerSchedulerProcesses", "SetupTv, Configuration");
      setting.Value = textBox1.Text;
      setting.Persist();

      setting = _layer.GetSetting("NetworkMonitorEnabled", "false");
      setting.Value = checkBox15.Checked.ToString();
      setting.Persist();

      setting = _layer.GetSetting("NetworkMonitorIdleLimit", "2");
      setting.Value = numericUpDown5.Value.ToString();
      setting.Persist();
    }

    private void CheckDay(EPGWakeupConfig cfg, EPGGrabDays day, bool enabled)
    {
      if (enabled)
        cfg.Days.Add(day);
    }

    private void button1_Click(object sender, EventArgs e)
    {
      SelectProcessForm spf = new SelectProcessForm();
      DialogResult dr = spf.ShowDialog();
      if (DialogResult.OK == dr)
      {
        if (!spf.SelectedProcess.Equals(String.Empty))
        {
          if (textBox1.Text.Equals(String.Empty))
          {
            textBox1.Text = spf.SelectedProcess;
          }
          else
          {
            textBox1.Text = String.Format("{0}, {1}", textBox1.Text, spf.SelectedProcess);
          }
        }
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      DialogResult r = openFileDialog1.ShowDialog();
      if (r == DialogResult.OK)
      {
        textBox2.Text = openFileDialog1.FileName;
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      DialogResult r = openFileDialog1.ShowDialog();
      if (r == DialogResult.OK)
      {
        tbEpgCmd.Text = openFileDialog1.FileName;
      }
    }
  }
}