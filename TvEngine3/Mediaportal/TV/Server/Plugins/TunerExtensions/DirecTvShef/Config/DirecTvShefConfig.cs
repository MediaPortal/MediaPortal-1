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
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Config
{
  internal partial class DirecTvShefConfig : SectionSettings
  {
    private const string SELECT_GENIE_MINI_NONE = "(none)";

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
        cell.Value = tunerStbConfig.IpAddress;
        cell.Tag = tunerStbConfig;

        cell = row.Cells["dataGridViewColumnGenieMini"];
        cell.Value = tunerStbConfig.Location;
        cell.Tag = tunerStbConfig.Location;

        row.Cells["dataGridViewColumnPowerControl"].Value = tunerStbConfig.EnablePowerControl;

        this.LogDebug("DirecTV SHEF config: tuner...");
        this.LogDebug("  tuner ID      = {0}", tuner.IdTuner);
        this.LogDebug("  tuner name    = {0}", tuner.Name);
        this.LogDebug("  IP address    = {0}", tunerStbConfig.IpAddress);
        this.LogDebug("  location      = {0}", tunerStbConfig.Location);
        this.LogDebug("  MAC address   = {0}", tunerStbConfig.MacAddress);
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
          this.LogInfo("DirecTV SHEF config: receiver IP address for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.IpAddress, newIpAddress);
          tunerStbConfig.IpAddress = newIpAddress;
          isChanged = true;
        }

        cell = row.Cells["dataGridViewColumnGenieMini"];
        string newGenieMini = cell.Value as string;
        string originalGenieMini = cell.Tag as string;
        if (!string.Equals(newGenieMini, originalGenieMini))
        {
          this.LogInfo("DirecTV SHEF config: Genie Mini for tuner {0} changed from {1} to {2}", tuner.IdTuner, originalGenieMini, newGenieMini);
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

    private void buttonSelectGenieMini_Click(object sender, EventArgs e)
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
      DataGridViewCell cell = row.Cells["dataGridViewColumnSetTopBoxIpAddress"];
      string ipAddress = cell.Value as string;
      if (string.IsNullOrWhiteSpace(ipAddress))
      {
        MessageBox.Show("Can't select Genie Mini without an IP address.", MESSAGE_CAPTION);
        return;
      }

      SetTopBoxConfig tunerStbConfig = cell.Tag as SetTopBoxConfig;
      cell = row.Cells["dataGridViewColumnTunerId"];
      Tuner tuner = cell.Tag as Tuner;
      this.LogInfo("DirecTV SHEF config: select Genie Mini, tuner ID = {0}, IP address = {1}, current Genie Mini = {2} ({3})", tuner.IdTuner, ipAddress, tunerStbConfig.Location, tunerStbConfig.MacAddress);

      IDictionary<string, string> locations;
      if (!ServiceAgents.Instance.PluginService<IDirecTvShefConfigService>().GetSetTopBoxLocations(ipAddress, out locations))
      {
        this.LogInfo("DirecTV SHEF config: get locations failed");
        MessageBox.Show(
          string.Format("Failed to communicate with the receiver at IP address '{0}'.", ipAddress) + Environment.NewLine +
            "Is it possible your configuration is incorrect?" + Environment.NewLine +
             SENTENCE_CHECK_LOG_FILES,
          MESSAGE_CAPTION,
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
        );
        return;
      }

      object[] genieMiniArray = new object[locations.Count];
      genieMiniArray[0] = SELECT_GENIE_MINI_NONE;
      int i = 1;
      object currentGenieMini = SELECT_GENIE_MINI_NONE;
      foreach (var location in locations)
      {
        this.LogDebug("  MAC address = {0}, location = {1}", location.Value, location.Key);
        if (!string.Equals(location.Value, "0"))    // not main Genie location
        {
          genieMiniArray[i++] = location.Key;
          if (string.Equals(location.Value, tunerStbConfig.MacAddress))
          {
            currentGenieMini = location.Key;
          }
        }
      }

      SelectGenieMini dialog = new SelectGenieMini(genieMiniArray, currentGenieMini);
      if (dialog.ShowDialog() != DialogResult.OK)
      {
        return;
      }

      string selectedGenieMiniLocation = dialog.Item as string;
      if (selectedGenieMiniLocation == null || string.Equals(selectedGenieMiniLocation, SELECT_GENIE_MINI_NONE))
      {
        tunerStbConfig.Location = string.Empty;
        tunerStbConfig.MacAddress = string.Empty;
      }
      else
      {
        tunerStbConfig.Location = selectedGenieMiniLocation;
        tunerStbConfig.MacAddress = locations[selectedGenieMiniLocation];
      }
      row.Cells["dataGridViewColumnGenieMini"].Value = tunerStbConfig.Location;
      this.LogInfo("DirecTV SHEF config: select Genie Mini, location = {0}, MAC address = {1}", tunerStbConfig.Location, tunerStbConfig.MacAddress);
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
      string accessCardId;
      string receiverId;
      string stbSoftwareVersion;
      string shefVersion;
      int systemTime;
      if (!ServiceAgents.Instance.PluginService<IDirecTvShefConfigService>().GetSetTopBoxVersion(ipAddress, out accessCardId, out receiverId, out stbSoftwareVersion, out shefVersion, out systemTime))
      {
        this.LogInfo("DirecTV SHEF config: test failed");
        MessageBox.Show(
          string.Format("Failed to communicate with the receiver at IP address '{0}'.", ipAddress) + Environment.NewLine +
            "Is it possible your configuration is incorrect?" + Environment.NewLine +
             SENTENCE_CHECK_LOG_FILES,
          MESSAGE_CAPTION,
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
        );
        return;
      }

      MessageBox.Show("Success!" + Environment.NewLine +
          "access card ID = " + accessCardId + Environment.NewLine +
          "receiver ID = " + receiverId + Environment.NewLine +
          "software version = " + stbSoftwareVersion + Environment.NewLine +
          "SHEF version = " + shefVersion,
        MESSAGE_CAPTION,
        MessageBoxButtons.OK,
        MessageBoxIcon.Information
      );
      this.LogInfo("DirecTV SHEF config: test succeeded");
      this.LogDebug("  access card ID   = {0}", accessCardId);
      this.LogDebug("  receiver ID      = {0}", receiverId);
      this.LogDebug("  software version = {0}", stbSoftwareVersion);
      this.LogDebug("  SHEF version     = {0}", shefVersion);
      this.LogDebug("  system time      = {0}", systemTime);
    }
  }
}