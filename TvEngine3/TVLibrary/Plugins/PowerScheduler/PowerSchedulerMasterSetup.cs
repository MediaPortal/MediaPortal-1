#region Copyright (C) 2007 Team MediaPortal
/* 
 *	Copyright (C) 2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
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
  public partial class PowerSchedulerMasterSetup : SectionSettings
  {

    TvBusinessLayer _layer;

    public PowerSchedulerMasterSetup() : base("PowerSchedulerPlugin")
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

      setting = _layer.GetSetting("PowerSchedulerWakeupActive", "false");
      checkBox2.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerForceShutdown", "false");
      checkBox3.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerShutdownMode", "2");
      comboBox1.SelectedIndex = Convert.ToInt32(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerPreWakeupTime", "60");
      numericUpDown2.Value = Convert.ToDecimal(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerCheckInterval", "60");
      numericUpDown3.Value = Convert.ToDecimal(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerExtensiveLogging", "false");
      checkBox4.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("PreventStandbyWhenGrabbingEPG", "false");
      checkBox5.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("WakeupSystemForEPGGrabbing", "false");
      checkBox6.Checked = Convert.ToBoolean(setting.Value);

      EPGWakeupConfig config = new EPGWakeupConfig((_layer.GetSetting("EPGWakeupConfig", String.Empty).Value));
      foreach (EPGGrabDays day in config.Days)
      {
        switch (day)
        {
          case EPGGrabDays.Monday:
            checkBox7.Checked = true;
            break;
          case EPGGrabDays.Tuesday:
            checkBox8.Checked = true;
            break;
          case EPGGrabDays.Wednesday:
            checkBox9.Checked = true;
            break;
          case EPGGrabDays.Thursday:
            checkBox10.Checked = true;
            break;
          case EPGGrabDays.Friday:
            checkBox11.Checked = true;
            break;
          case EPGGrabDays.Saturday:
            checkBox12.Checked = true;
            break;
          case EPGGrabDays.Sunday:
            checkBox13.Checked = true;
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

      setting = _layer.GetSetting("PowerSchedulerProcesses", "SetupTv, Configuration");
      textBox1.Text = setting.Value;

    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerShutdownActive", "false");
      if (checkBox1.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();
    }

    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerIdleTimeout", "5");
      setting.Value = numericUpDown1.Value.ToString();
      setting.Persist();
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerWakeupActive", "false");
      if (checkBox2.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();
    }

    private void checkBox3_CheckedChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerForceShutdown", "false");
      if (checkBox3.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerShutdownMode", "2");
      setting.Value = comboBox1.SelectedIndex.ToString();
      setting.Persist();
    }

    private void numericUpDown2_ValueChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerPreWakeupTime", "60");
      setting.Value = numericUpDown2.Value.ToString();
      setting.Persist();
    }

    private void numericUpDown3_ValueChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerCheckInterval", "60");
      setting.Value = numericUpDown3.Value.ToString();
      setting.Persist();
    }

    private void checkBox4_CheckedChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerExtensiveLogging", "false");
      if (checkBox4.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();
    }

    public override void OnSectionDeActivated()
    {
      // EPG grabber settings are only stored when the section is deactivated
      Setting setting;

      setting = _layer.GetSetting("PreventStandbyWhenGrabbingEPG", "false");
      if (checkBox5.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();

      setting = _layer.GetSetting("WakeupSystemForEPGGrabbing", "false");
      if (checkBox6.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();

      setting = _layer.GetSetting("EPGWakeupConfig", String.Empty);
      EPGWakeupConfig cfg = new EPGWakeupConfig(setting.Value);
      EPGWakeupConfig newcfg = new EPGWakeupConfig();
      newcfg.Hour = cfg.Hour;
      newcfg.Minutes = cfg.Minutes;
      newcfg.Days = cfg.Days;
      newcfg.LastRun = cfg.LastRun;
      string[] time = maskedTextBox1.Text.Split(':');
      newcfg.Hour = Convert.ToInt32(time[0]);
      newcfg.Minutes = Convert.ToInt32(time[1]);
      CheckDay(newcfg, EPGGrabDays.Monday, checkBox7.Checked);
      CheckDay(newcfg, EPGGrabDays.Tuesday, checkBox8.Checked);
      CheckDay(newcfg, EPGGrabDays.Wednesday, checkBox9.Checked);
      CheckDay(newcfg, EPGGrabDays.Thursday, checkBox10.Checked);
      CheckDay(newcfg, EPGGrabDays.Friday, checkBox11.Checked);
      CheckDay(newcfg, EPGGrabDays.Saturday, checkBox12.Checked);
      CheckDay(newcfg, EPGGrabDays.Sunday, checkBox12.Checked);
      if (!cfg.Equals(newcfg))
      {
        setting.Value = newcfg.SerializeAsString();
        setting.Persist();
      }

      // Process settings are only stored when the section is deactivated
      setting = _layer.GetSetting("PowerSchedulerProcesses", "SetupTv, Configuration");
      setting.Value = textBox1.Text;
      setting.Persist();
    }

    private void CheckDay(EPGWakeupConfig cfg, EPGGrabDays day, bool enabled)
    {
      if (enabled)
      {
        if (!cfg.Days.Contains(day))
          cfg.Days.Add(day);
      }
      else
      {
        if (cfg.Days.Contains(day))
          cfg.Days.Remove(day);
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      SelectProcessForm spf = new SelectProcessForm();
      Form f = spf as Form;
      DialogResult dr = f.ShowDialog();
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

  }

}
