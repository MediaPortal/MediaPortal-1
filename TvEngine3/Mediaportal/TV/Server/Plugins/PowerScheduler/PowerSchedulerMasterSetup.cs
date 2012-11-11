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

#region Usings

using System;
using System.Text;
using System.Windows.Forms;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;

#endregion

namespace Mediaportal.TV.Server.Plugins.PowerScheduler
{
  public partial class PowerSchedulerMasterSetup : SectionSettings
  {
    

    public PowerSchedulerMasterSetup()
    {
      InitializeComponent();      
    }

    public override void LoadSettings()
    {     
      Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerShutdownActive", "false");
      checkBox1.Checked = Convert.ToBoolean(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerIdleTimeout", "5");
      numericUpDown1.Value = Convert.ToDecimal(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerStandbyAllowedStart", "0");
      numUpDownStandbyAllowedStartHour.Value = Convert.ToDecimal(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerStandbyAllowedEnd", "24");
      numUpDownStandbyAllowedEndHour.Value = Convert.ToDecimal(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerWakeupActive", "false");
      checkBox2.Checked = Convert.ToBoolean(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerShutdownMode", "2");
      comboBox1.SelectedIndex = Convert.ToInt32(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerForceShutdown", "false");
      checkBox3.Checked = Convert.ToBoolean(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerExtensiveLogging", "false");
      checkBox4.Checked = Convert.ToBoolean(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerPreWakeupTime", "60");
      numericUpDown2.Value = Convert.ToDecimal(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerPreNoShutdownTime", "300");
      numericUpDown4.Value = Convert.ToDecimal(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerCheckInterval", "60");
      numericUpDown3.Value = Convert.ToDecimal(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerReinitializeController", "false");
      checkBox5.Checked = Convert.ToBoolean(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerCommand", string.Empty);
      textBox2.Text = setting.Value;

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PreventStandbyWhenGrabbingEPG", "false");
      checkBox6.Checked = Convert.ToBoolean(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("WakeupSystemForEPGGrabbing", "false");
      checkBox7.Checked = Convert.ToBoolean(setting.Value);


      EPGWakeupConfig config = new EPGWakeupConfig(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("EPGWakeupConfig", String.Empty).Value);
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

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerEpgCommand", String.Empty);
      tbEpgCmd.Text = setting.Value;

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PowerSchedulerProcesses", "SetupTv, Configuration");
      textBox1.Text = setting.Value;

      // Load share monitoring configuration for standby prevention
      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PreventStandybyWhenSharesInUse", "true");
      shareMonitoring.Checked = Convert.ToBoolean(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("PreventStandybyWhenSpecificSharesInUse", "");
      inhibitStandbyShares.Rows.Clear();
      string[] shares = setting.Value.Split(';');
      foreach (string share in shares)
      {
        string[] shareItem = share.Split(',');
        if ((shareItem.Length.Equals(3)) &&
            ((shareItem[0].Trim().Length > 0) ||
             (shareItem[1].Trim().Length > 0) ||
             (shareItem[2].Trim().Length > 0)))
        {
          inhibitStandbyShares.Rows.Add(shareItem);
        }
      }

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("NetworkMonitorEnabled", "false");
      checkBox15.Checked = Convert.ToBoolean(setting.Value);

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("NetworkMonitorIdleLimit", "2");
      numericUpDown5.Value = Convert.ToDecimal(setting.Value);
    }

    public override void SaveSettings()
    {      
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerShutdownActive", checkBox1.Checked.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerIdleTimeout", numericUpDown1.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerStandbyAllowedStart", numUpDownStandbyAllowedStartHour.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerStandbyAllowedEnd", numUpDownStandbyAllowedEndHour.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerWakeupActive", checkBox2.Checked.ToString());            
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerShutdownMode", comboBox1.SelectedIndex.ToString());            
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerForceShutdown", checkBox3.Checked.ToString());            
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerExtensiveLogging", checkBox4.Checked.ToString());            
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerPreWakeupTime", numericUpDown2.Value.ToString());            
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerPreNoShutdownTime", numericUpDown4.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerCheckInterval", numericUpDown3.Value.ToString());      
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerReinitializeController", checkBox5.Checked.ToString());      
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerCommand", textBox2.Text);
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PreventStandbyWhenGrabbingEPG", checkBox6.Checked.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("WakeupSystemForEPGGrabbing", checkBox7.Checked.ToString());
      

      Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("EPGWakeupConfig", String.Empty);
      var cfg = new EPGWakeupConfig(setting.Value);
      var newcfg = new EPGWakeupConfig {Hour = cfg.Hour, Minutes = cfg.Minutes, LastRun = cfg.LastRun};
      // newcfg.Days = cfg.Days;
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
        ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("EPGWakeupConfig", newcfg.SerializeAsString());        
      }

      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerEpgCommand", tbEpgCmd.Text);
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PowerSchedulerProcesses", textBox1.Text);            

      // Persist share monitoring configuration for standby prevention
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PreventStandybyWhenSharesInUse", shareMonitoring.Checked.ToString());      
      
      var shares = new StringBuilder();
      foreach (DataGridViewRow row in inhibitStandbyShares.Rows)
      {
        shares.AppendFormat("{0},{1},{2};", row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value);
      }
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PreventStandybyWhenSpecificSharesInUse", shares.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("NetworkMonitorEnabled", checkBox15.Checked.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("NetworkMonitorIdleLimit", numericUpDown5.Value.ToString());
    }

    private void CheckDay(EPGWakeupConfig cfg, EPGGrabDays day, bool enabled)
    {
      if (enabled)
        cfg.Days.Add(day);
    }

    private void button1_Click(object sender, EventArgs e)
    {
      using (var spf = new SelectProcessForm()) {
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