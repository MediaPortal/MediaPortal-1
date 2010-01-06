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

using System;
using System.ServiceProcess;
using Microsoft.Win32;

namespace MediaPortal.Configuration.Sections
{
  public partial class ThirdPartyChecks : SectionSettings
  {
    private static McsPolicyStatus _mcsServices;

    public ThirdPartyChecks()
      : this("Additional 3rd party checks") {}

    public ThirdPartyChecks(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      _mcsServices = McsPolicyCheck();

      RefreshForm();
    }

    private void RefreshForm()
    {
      switch (_mcsServices)
      {
        case McsPolicyStatus.PolicyInPlace:
          mpLabelStatusMCS.Text = "services disabled by policy";
          mpLabelStatusMCS.ForeColor = System.Drawing.Color.Green;
          mpButtonMCS.Text = "Re-enable services";
          mpButtonMCS.Visible = true;
          mpButtonMCS.Enabled = true;
          mpGroupBoxWarningMce.Visible = false;
          break;
        case McsPolicyStatus.ServicesStopped:
          mpLabelStatusMCS.Text = "services stopped";
          mpLabelStatusMCS.ForeColor = System.Drawing.Color.Green;
          mpButtonMCS.Text = "Enable policy to prevent services startup";
          mpButtonMCS.Visible = true;
          mpButtonMCS.Enabled = true;
          mpGroupBoxWarningMce.Visible = true;
          break;
        case McsPolicyStatus.NotAMceSystem:
          mpLabelStatusMCS.Text = "services not installed";
          mpLabelStatusMCS.ForeColor = System.Drawing.Color.Green;
          mpButtonMCS.Visible = false;
          mpButtonMCS.Enabled = false;
          mpGroupBoxWarningMce.Visible = false;
          break;
        default:
          mpLabelStatusMCS.Text = "services running";
          mpLabelStatusMCS.ForeColor = System.Drawing.Color.Red;
          mpButtonMCS.Text = "Enable policy to prevent services startup";
          mpButtonMCS.Visible = true;
          mpButtonMCS.Enabled = true;
          mpGroupBoxWarningMce.Visible = true;
          break;
      }
    }

    #region MCS Policy Check

    private static McsPolicyStatus McsPolicyCheck()
    {
      // Check for status of MCE services
      bool mceSystem = false;
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController srv in services)
      {
        if (srv.ServiceName == "ehRecvr" || srv.ServiceName == "ehSched")
        {
          mceSystem = true;
          if (srv.Status == ServiceControllerStatus.Running)
          {
            return McsPolicyStatus.ServicesRunning;
          }
        }
      }

      // If services are not found, then this is not a MCE system
      if (!mceSystem)
      {
        return McsPolicyStatus.NotAMceSystem;
      }

      // Check for policy registry key
      if (McsPolicyManipulation(true) == McsPolicyStatus.PolicyInPlace)
      {
        return McsPolicyStatus.PolicyInPlace;
      }
      // No MCE services running and no policy: services are stopped
      return McsPolicyStatus.ServicesStopped;
    }

    private static McsPolicyStatus McsPolicyManipulation(bool checkonly)
    {
      const string keyPath = "SOFTWARE\\Policies\\Microsoft\\WindowsMediaCenter";

      RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, !checkonly);

      if (checkonly)
      {
        if (key != null)
        {
          object objValue = key.GetValue("MediaCenter");
          key.Close();
          if (objValue != null && objValue.ToString() == "1")
          {
            _mcsServices = McsPolicyStatus.PolicyInPlace;
          }
        }
      }
      else
      {
        if (_mcsServices == McsPolicyStatus.PolicyInPlace)
        {
          key.DeleteValue("MediaCenter");
          key.Close();
          _mcsServices = McsPolicyStatus.ServicesStopped;
        }
        else
        {
          if (key == null)
          {
            key = Registry.LocalMachine.CreateSubKey(keyPath);
          }
          key.SetValue("MediaCenter", "1", RegistryValueKind.DWord);
          key.Close();
          _mcsServices = McsPolicyStatus.PolicyInPlace;
        }
      }
      return _mcsServices;
    }

    private enum McsPolicyStatus
    {
      NotAMceSystem,
      ServicesRunning,
      ServicesStopped,
      PolicyInPlace
    }

    private void mpButtonMCS_Click(object sender, EventArgs e)
    {
      McsPolicyManipulation(false);
      RefreshForm();
    }

    #endregion
  }
}