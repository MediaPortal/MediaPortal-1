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
      checkBox1.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerShutdownActive", false);

      numericUpDown1.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerIdleTimeout", 5);

      numUpDownStandbyAllowedStartHour.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerStandbyAllowedStart", 0);

      numUpDownStandbyAllowedEndHour.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerStandbyAllowedEnd", 24);

      checkBox2.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerWakeupActive", false);

      comboBox1.SelectedIndex = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerShutdownMode", 2);

      checkBox3.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerForceShutdown", false);

      checkBox4.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerExtensiveLogging", false);

      numericUpDown2.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerPreWakeupTime", 60);

      numericUpDown4.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerPreNoShutdownTime", 300);

      numericUpDown3.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerCheckInterval", 60);

      checkBox5.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerReinitializeController", false);

      textBox2.Text = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerCommand", string.Empty);

      checkBox6.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("PreventStandbyWhenGrabbingEPG", false);

      checkBox7.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("WakeupSystemForEPGGrabbing", false);


      var config = new EPGWakeupConfig(ServiceAgents.Instance.SettingServiceAgent.GetValue("EPGWakeupConfig", String.Empty));
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

      tbEpgCmd.Text = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerEpgCommand", String.Empty);

      textBox1.Text = ServiceAgents.Instance.SettingServiceAgent.GetValue("PowerSchedulerProcesses", "SetupTv, Configuration");

      // Load share monitoring configuration for standby prevention
      shareMonitoring.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("PreventStandybyWhenSharesInUse", true);

      string sharesSetting = ServiceAgents.Instance.SettingServiceAgent.GetValue("PreventStandybyWhenSpecificSharesInUse", "");
      inhibitStandbyShares.Rows.Clear();
      string[] shares = sharesSetting.Split(';');
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

      checkBox15.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("NetworkMonitorEnabled", false);

      numericUpDown5.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("NetworkMonitorIdleLimit", 2);
    }

    public override void SaveSettings()
    {      
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerShutdownActive", checkBox1.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerIdleTimeout", (int) numericUpDown1.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerStandbyAllowedStart", (int) numUpDownStandbyAllowedStartHour.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerStandbyAllowedEnd", (int) numUpDownStandbyAllowedEndHour.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerWakeupActive", checkBox2.Checked);            
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerShutdownMode", comboBox1.SelectedIndex);            
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerForceShutdown", checkBox3.Checked);            
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerExtensiveLogging", checkBox4.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerPreWakeupTime", (int) numericUpDown2.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerPreNoShutdownTime", (int) numericUpDown4.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerCheckInterval", (int) numericUpDown3.Value);      
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerReinitializeController", checkBox5.Checked);      
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerCommand", textBox2.Text);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PreventStandbyWhenGrabbingEPG", checkBox6.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("WakeupSystemForEPGGrabbing", checkBox7.Checked);
      

      string setting = ServiceAgents.Instance.SettingServiceAgent.GetValue("EPGWakeupConfig", String.Empty);
      var cfg = new EPGWakeupConfig(setting);
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
        ServiceAgents.Instance.SettingServiceAgent.GetValue("EPGWakeupConfig", newcfg.SerializeAsString());        
      }

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerEpgCommand", tbEpgCmd.Text);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PowerSchedulerProcesses", textBox1.Text);            

      // Persist share monitoring configuration for standby prevention
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PreventStandybyWhenSharesInUse", shareMonitoring.Checked);      
      
      var shares = new StringBuilder();
      foreach (DataGridViewRow row in inhibitStandbyShares.Rows)
      {
        shares.AppendFormat("{0},{1},{2};", row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value);
      }
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("PreventStandybyWhenSpecificSharesInUse", shares.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("NetworkMonitorEnabled", checkBox15.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("NetworkMonitorIdleLimit", (int)numericUpDown5.Value);
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