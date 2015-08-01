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
using System.Collections.Generic;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Service;
using Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef;
using Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef.Request;
using Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef.Response;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Config
{
  internal partial class DirecTvShefConfig : SectionSettings
  {
    public DirecTvShefConfig()
      : base("DirecTV SHEF")
    {
      ServiceAgents.Instance.AddGenericService<IDirecTvShefConfigService>();
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("DirecTV SHEF config: activating");

      dataGridViewConfig.Rows.Clear();

      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None);
      this.LogDebug("DirecTV SHEF config: total tuner count = {0}", tuners.Count);
      foreach (Tuner tuner in tuners)
      {
        BroadcastStandard tunerSupportedBroadcastStandards = (BroadcastStandard)tuner.SupportedBroadcastStandards;
        if (!tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.AnalogTelevision) && !tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput))
        {
          continue;
        }

        DataGridViewRow row = dataGridViewConfig.Rows[dataGridViewConfig.Rows.Add()];
        DataGridViewCell cell = row.Cells["dataGridViewColumnTunerId"];
        cell.Value = tuner.IdTuner.ToString();
        cell.Tag = tuner;

        row.Cells["dataGridViewColumnTunerName"].Value = tuner.Name;

        SetTopBoxConfig tunerStbConfig = ServiceAgents.Instance.PluginService<IDirecTvShefConfigService>().GetSetTopBoxConfigurationForTuner(tuner.ExternalId);
        cell = row.Cells["dataGridViewColumnSetTopBoxIpAddress"];
        tunerStbConfig.IpAddress = tunerStbConfig.IpAddress ?? string.Empty;
        cell.Value = tunerStbConfig.IpAddress;
        cell.Tag = tunerStbConfig;

        row.Cells["dataGridViewColumnPowerControl"].Value = tunerStbConfig.EnablePowerControl;

        this.LogDebug("DirecTV SHEF config: tuner...");
        this.LogDebug("  tuner ID      = {0}", tuner.IdTuner);
        this.LogDebug("  tuner name    = {0}", tuner.Name);
        this.LogDebug("  IP address    = {0}", tunerStbConfig.IpAddress);
        this.LogDebug("  power control = {0}", tunerStbConfig.EnablePowerControl);
      }
      this.LogDebug("DirecTV SHEF config: analog tuner count = {0}", dataGridViewConfig.Rows.Count);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("DirecTV SHEF config: deactivating, tuner count = {0}", dataGridViewConfig.Rows.Count);

      ICollection<SetTopBoxConfig> settings = new List<SetTopBoxConfig>(dataGridViewConfig.Rows.Count);
      foreach (DataGridViewRow row in dataGridViewConfig.Rows)
      {
        bool isChanged = false;
        DataGridViewCell cell = row.Cells["dataGridViewColumnTunerId"];
        Tuner tuner = cell.Tag as Tuner;

        cell = row.Cells["dataGridViewColumnSetTopBoxIpAddress"];
        SetTopBoxConfig tunerStbConfig = cell.Tag as SetTopBoxConfig;
        string newIpAddress = cell.Value as string;
        if (string.IsNullOrWhiteSpace(newIpAddress))
        {
          newIpAddress = string.Empty;
        }
        if (!string.Equals(newIpAddress, tunerStbConfig.IpAddress))
        {
          this.LogInfo("DirecTV SHEF config: IP address for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.IpAddress, newIpAddress);
          tunerStbConfig.IpAddress = newIpAddress;
          isChanged = true;
        }

        cell = row.Cells["dataGridViewColumnPowerControl"];
        bool newPowerControl = (bool)cell.Value;
        if (newPowerControl != tunerStbConfig.EnablePowerControl)
        {
          this.LogInfo("DirecTV SHEF config: power control for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.EnablePowerControl, newPowerControl);
          tunerStbConfig.EnablePowerControl = newPowerControl;
          isChanged = true;
        }

        if (isChanged)
        {
          settings.Add(tunerStbConfig);
        }
      }

      if (settings.Count > 0)
      {
        ServiceAgents.Instance.PluginService<IDirecTvShefConfigService>().SaveSetTopBoxConfiguration(settings);
      }

      base.OnSectionDeActivated();
    }

    private void buttonTest_Click(object sender, System.EventArgs e)
    {
      if (dataGridViewConfig.SelectedRows.Count != 1)
      {
        return;
      }
      DataGridViewRow row = dataGridViewConfig.SelectedRows[0];
      if (row == null)
      {
        return;
      }
      string ipAddress = row.Cells["dataGridViewColumnSetTopBoxIpAddress"].Value as string;
      if (string.IsNullOrWhiteSpace(ipAddress))
      {
        MessageBox.Show("Can't test without an IP address.", MESSAGE_CAPTION);
        return;
      }

      this.LogInfo("DirecTV SHEF config: test, IP address = {0}", ipAddress);
      ShefClient client = new ShefClient(ipAddress);
      IShefResponse shefResponse;
      if (!client.SendRequest(new ShefRequestGetVersion(), out shefResponse))
      {
        this.LogInfo("DirecTV SHEF config: test failed");
        MessageBox.Show(
          string.Format("Failed to communicate with the set top box at IP address '{0}'.", ipAddress) + Environment.NewLine +
            "Is it possible your configuration is incorrect?" + Environment.NewLine +
             SENTENCE_CHECK_LOG_FILES,
          MESSAGE_CAPTION,
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
        );
        return;
      }

      ShefResponseGetVersion response = shefResponse as ShefResponseGetVersion;
      MessageBox.Show("Success!" + Environment.NewLine +
          "access card ID = " + response.AccessCardId + Environment.NewLine +
          "receiver ID = " + response.ReceiverId + Environment.NewLine +
          "software version = " + response.StbSoftwareVersion + Environment.NewLine +
          "SHEF version = " + response.Version,
        MESSAGE_CAPTION,
        MessageBoxButtons.OK,
        MessageBoxIcon.Information
      );
      this.LogInfo("DirecTV SHEF config: test succeeded");
      this.LogDebug("  access card ID   = {0}", response.AccessCardId);
      this.LogDebug("  receiver ID      = {0}", response.ReceiverId);
      this.LogDebug("  software version = {0}", response.StbSoftwareVersion);
      this.LogDebug("  SHEF version     = {0}", response.Version);
      this.LogDebug("  system time      = {0}", response.SystemTime);
    }
  }
}