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

using System.Collections.Generic;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class UserPriorities : SectionSettings
  {
    private int _priorityScheduler = -1;
    private int _priorityChannelScanner = -1;
    private int _priorityEpgGrabber = -1;
    private int _priorityOtherDefault = -1;
    private readonly IDictionary<string, int> _prioritiesOtherCustom = new Dictionary<string, int>();

    public UserPriorities(ServerConfigurationChangedEventHandler handler)
      : base("User Priorities", handler)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("user priorities: activating");

      _priorityScheduler = ServiceAgents.Instance.SettingServiceAgent.GetValue("userPriorityScheduler", 100);
      numericUpDownScheduler.Value = _priorityScheduler;
      _priorityChannelScanner = ServiceAgents.Instance.SettingServiceAgent.GetValue("userPriorityChannelScanner", 2);
      numericUpDownChannelScanner.Value = _priorityChannelScanner;
      _priorityEpgGrabber = ServiceAgents.Instance.SettingServiceAgent.GetValue("userPriorityEpgGrabber", 1);
      numericUpDownEpgGrabber.Value = _priorityEpgGrabber;
      _priorityOtherDefault = ServiceAgents.Instance.SettingServiceAgent.GetValue("userPriorityOtherDefault", 3);
      numericUpDownOtherDefault.Value = _priorityOtherDefault;
      this.LogDebug("  scheduler       = {0}", _priorityScheduler);
      this.LogDebug("  channel scanner = {0}", _priorityEpgGrabber);
      this.LogDebug("  EPG grabber     = {0}", _priorityEpgGrabber);
      this.LogDebug("  other default   = {0}", _priorityOtherDefault);

      _prioritiesOtherCustom.Clear();
      dataGridViewUserPriorities.Rows.Clear();
      string[] users = ServiceAgents.Instance.SettingServiceAgent.GetValue("userPrioritiesOtherCustom", string.Empty).Split(';');
      foreach (string user in users)
      {
        int lastCommaIndex = user.LastIndexOf(",");
        if (lastCommaIndex < 0)
        {
          continue;
        }
        string userName = user.Substring(0, lastCommaIndex).Trim();
        string priorityString = user.Substring(lastCommaIndex + 1);
        int priority;
        if (!string.IsNullOrEmpty(userName) && int.TryParse(priorityString, out priority))
        {
          this.LogDebug("  {0, -15} = {1}", userName, priority);
          _prioritiesOtherCustom[userName] = priority;
          dataGridViewUserPriorities.Rows.Add(new string[2] { userName.Trim(), priority.ToString() });
        }
      }

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("user priorities: deactivating");

      bool needReload = false;
      if (_priorityScheduler != numericUpDownScheduler.Value)
      {
        this.LogInfo("user priorities: scheduler priority changed from {0} to {1}", _priorityScheduler, numericUpDownScheduler.Value);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("userPriorityScheduler", (int)numericUpDownScheduler.Value);
        needReload = true;
      }
      if (_priorityChannelScanner == numericUpDownChannelScanner.Value)
      {
        this.LogInfo("user priorities: channel scanner priority changed from {0} to {1}", _priorityChannelScanner, numericUpDownChannelScanner.Value);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("userPriorityChannelScanner", (int)numericUpDownChannelScanner.Value);
        needReload = true;
      }
      if (_priorityEpgGrabber == numericUpDownEpgGrabber.Value)
      {
        this.LogInfo("user priorities: EPG grabber priority changed from {0} to {1}", _priorityEpgGrabber, numericUpDownEpgGrabber.Value);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("userPriorityEpgGrabber", (int)numericUpDownEpgGrabber.Value);
        needReload = true;
      }
      if (_priorityOtherDefault == numericUpDownOtherDefault.Value)
      {
        this.LogInfo("user priorities: other default priority changed from {0} to {1}", _priorityOtherDefault, numericUpDownOtherDefault.Value);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("userPriorityOtherDefault", (int)numericUpDownOtherDefault.Value);
        needReload = true;
      }

      List<string> parts = new List<string>(dataGridViewUserPriorities.Rows.Count);
      bool customPrioritiesChanged = false;
      foreach (DataGridViewRow row in dataGridViewUserPriorities.Rows)
      {
        string userName = row.Cells["dataGridViewColumnUser"].Value as string;
        string priorityString = row.Cells["dataGridViewColumnPriority"].Value as string;
        int priority;
        if (!string.IsNullOrWhiteSpace(userName) && int.TryParse(priorityString, out priority))
        {
          userName = userName.Trim();
          if (priority < 1)
          {
            priority = 1;
          }
          else if (priority > 100)
          {
            priority = 100;
          }
          int currentPriority;
          if (!_prioritiesOtherCustom.TryGetValue(userName, out currentPriority))
          {
            this.LogInfo("user priorities: added custom priority, user name = {0}, priority = {1}", userName, priority);
            customPrioritiesChanged = true;
          }
          else if (priority != currentPriority)
          {
            this.LogInfo("user priorities: changed custom priority, user name = {0}, old priority = {1}, new priority = {2}", userName, currentPriority, priority);
            customPrioritiesChanged = true;
            _prioritiesOtherCustom.Remove(userName);
          }
          parts.Add(string.Format("{0},{1}", userName, priority));
        }
      }
      foreach (KeyValuePair<string, int> user in _prioritiesOtherCustom)
      {
        this.LogInfo("user priorities: deleted custom priority, user name = {0}, priority = {1}", user.Key, user.Value);
        customPrioritiesChanged = true;
      }

      if (customPrioritiesChanged)
      {
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("userPrioritiesOtherCustom", string.Join(";", parts));
        needReload = true;
      }

      if (needReload)
      {
        OnServerConfigurationChanged(this, true, null);
      }

      base.OnSectionDeActivated();
    }
  }
}