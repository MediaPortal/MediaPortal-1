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
using System.Diagnostics;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class ThirdPartyChecks : SectionSettings
  {
    private bool _isMcsInstalled = false;
    private bool _isMcsRunning = false;
    private bool _isMcsPolicyActive = false;

    public ThirdPartyChecks()
      : base("Third Party Checks")
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("checks: activating");

      ServiceAgents.Instance.ControllerServiceAgent.GetMceServiceStatus(out _isMcsInstalled, out _isMcsRunning, out _isMcsPolicyActive);
      this.LogDebug("checks: Media Center services, installed = {0}, running = {1}, policy active = {2}", _isMcsInstalled, _isMcsRunning, _isMcsPolicyActive);
      UpdateMcsFields();

      bool isApplicable = false;
      bool isNeeded = false;
      ServiceAgents.Instance.ControllerServiceAgent.GetBdaFixStatus(out isApplicable, out isNeeded);
      this.LogDebug("checks: BDA hot fix, applicable = {0}, needed = {1}", isApplicable, isNeeded);
      if (isApplicable && isNeeded)
      {
        labelBdaHotFixStatusValue.Text = "not installed";
        labelBdaHotFixStatusValue.ForeColor = System.Drawing.Color.Red;
        linkLabelBdaHotFix.Enabled = true;
        linkLabelBdaHotFix.Visible = true;
      }
      else
      {
        labelBdaHotFixStatusValue.Text = !isApplicable ? "not needed" : "installed";
        labelBdaHotFixStatusValue.ForeColor = System.Drawing.Color.Green;
        linkLabelBdaHotFix.Enabled = false;
        linkLabelBdaHotFix.Visible = false;
      }

      base.OnSectionActivated();
    }

    private void UpdateMcsFields()
    {
      buttonMcs.Enabled = _isMcsInstalled;
      buttonMcs.Text = "Disable Services";
      labelMcsStatusValue.ForeColor = _isMcsRunning ? System.Drawing.Color.Red : System.Drawing.Color.Green;
      if (_isMcsPolicyActive)
      {
        labelMcsStatusValue.Text = "services disabled by policy";
        buttonMcs.Text = "Re-enable Services";
      }
      else if (_isMcsRunning)
      {
        labelMcsStatusValue.Text = "services running";
      }
      else if (_isMcsInstalled)
      {
        labelMcsStatusValue.Text = "services stopped";
      }
      else
      {
        labelMcsStatusValue.Text = "services not installed";
        buttonMcs.Text = "Not Applicable";
      }
    }

    private void buttonMcs_Click(object sender, EventArgs e)
    {
      if (_isMcsPolicyActive)
      {
        this.LogInfo("checks: remove Media Center services policy");
        ServiceAgents.Instance.ControllerServiceAgent.RemoveMceServicePolicy();
      }
      else
      {
        this.LogInfo("checks: apply Media Center services policy");
        ServiceAgents.Instance.ControllerServiceAgent.ApplyMceServicePolicy();
      }
      _isMcsPolicyActive = !_isMcsPolicyActive;
      UpdateMcsFields();
    }

    private void linkLabelBdaHotFix_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      // BDA hot fix download link
      Process.Start(@"http://forum.team-mediaportal.com/threads/patch-tuner-issue-and-channel-scan-crash-windows-xp.6344/");
      // http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/11_Preparing_Your_System/XP_User_Guide#BDA_.2F_DVB_hotfix
    }
  }
}