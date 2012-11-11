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
using System.Collections.Specialized;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Epg : SectionSettings
  {
    private string crcSettingsFile = DebugSettings.SettingPath("DisableCRCCheck");

    public Epg()
      : this("DVB EPG") {}

    public Epg(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      

      checkBoxAlwaysFillHoles.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("generalEPGAlwaysFillHoles", false);
      checkBoxAlwaysUpdate.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("generalEPGAlwaysReplace", false);
      checkboxSameTransponder.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("generalGrapOnlyForSameTransponder", false);

      checkBoxEnableEPGWhileIdle.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("idleEPGGrabberEnabled", true);
      checkBoxEnableCRCCheck.Checked = !DebugSettings.DisableCRCCheck;
      numericUpDownEpgTimeOut.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeoutEPG", 10);
      numericUpDownEpgRefresh.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeoutEPGRefresh", 240);
      checkBoxEnableEpgWhileTimeshifting.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeshiftingEpgGrabberEnabled", false);
      numericUpDownTSEpgTimeout.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeshiftingEpgGrabberTimeout", 2);

      edTitleTemplate.Text = ServiceAgents.Instance.SettingServiceAgent.GetValue("epgTitleTemplate", "%TITLE%");
      edDescriptionTemplate.Text = ServiceAgents.Instance.SettingServiceAgent.GetValue("epgDescriptionTemplate", "%DESCRIPTION%");
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("generalEPGAlwaysFillHoles", checkBoxAlwaysFillHoles.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("generalEPGAlwaysReplace", checkBoxAlwaysUpdate.Checked);      
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("generalGrapOnlyForSameTransponder", checkboxSameTransponder.Checked);
      DebugSettings.DisableCRCCheck = !checkBoxEnableCRCCheck.Checked;
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("idleEPGGrabberEnabled", checkBoxEnableEPGWhileIdle.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeoutEPG", (int)numericUpDownEpgTimeOut.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeoutEPGRefresh", (int)numericUpDownEpgRefresh.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeshiftingEpgGrabberEnabled", checkBoxEnableEpgWhileTimeshifting.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeshiftingEpgGrabberTimeout", (int)numericUpDownTSEpgTimeout.Value);      
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("epgTitleTemplate", edTitleTemplate.Text);      
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("epgDescriptionTemplate", edDescriptionTemplate.Text);      
    }

    private static string EvalTemplate(string template, NameValueCollection values)
    {
      for (int i = 0; i < values.Count; i++)
        template = template.Replace(values.Keys[i], values[i]);
      return template;
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
      NameValueCollection defaults = new NameValueCollection();
      defaults.Add("%TITLE%", "Over the hedge");
      defaults.Add("%DESCRIPTION%",
                   "A scheming raccoon fools a mismatched family of forest creatures into helping him repay a debt of food, by invading the new suburban sprawl that popped up while they were hibernating...and learns a lesson about family himself.");
      defaults.Add("%GENRE%", "movie/drama (general)");
      defaults.Add("%STARRATING%", "6");
      defaults.Add("%STARRATING_STR%", "***+");
      defaults.Add("%CLASSIFICATION%", "PG");
      defaults.Add("%PARENTALRATING%", "8");
      defaults.Add("%NEWLINE%", Environment.NewLine);
      edTitleTest.Text = EvalTemplate(edTitleTemplate.Text, defaults);
      edDescriptionTest.Text = EvalTemplate(edDescriptionTemplate.Text, defaults);
    }
  }
}