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
using Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Config
{
  internal partial class UsbUirtConfig : SectionSettings
  {
    public UsbUirtConfig()
      : base("USB-UIRT")
    {
      ServiceAgents.Instance.AddGenericService<IUsbUirtConfigService>();
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("USB-UIRT config: activating");

      dataGridViewConfig.Rows.Clear();
      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None);

      IDictionary<int, UsbUirtDetail> comboBoxValueLookup = null;
      UsbUirtDetail blankUsbUirtEntry = new UsbUirtDetail();
      if (tuners.Count > 0)
      {
        ICollection<UsbUirtDetail> connectedUsbUirtDetails = ServiceAgents.Instance.PluginService<IUsbUirtConfigService>().GetAllUsbUirtDetails();
        this.LogDebug("USB-UIRT config: USB-UIRT count = {0}", connectedUsbUirtDetails.Count);
        List<UsbUirtDetail> columnDataSource = new List<UsbUirtDetail>(connectedUsbUirtDetails.Count + 1);
        columnDataSource.Add(blankUsbUirtEntry);
        comboBoxValueLookup.Add(blankUsbUirtEntry.Index, blankUsbUirtEntry);
        foreach (UsbUirtDetail detail in connectedUsbUirtDetails)
        {
          this.LogDebug("  name             = {0}", detail.Name);
          this.LogDebug("  index            = {0}", detail.Index);
          this.LogDebug("  transmit zone(s) = [{0}]", detail.TransmitZones);
          comboBoxValueLookup.Add(detail.Index, detail);
        }
        columnDataSource.AddRange(connectedUsbUirtDetails);

        DataGridViewComboBoxColumn column = (DataGridViewComboBoxColumn)dataGridViewConfig.Columns["dataGridViewColumnUsbUirt"];
        column.DataSource = columnDataSource;

        IList<string> stbProfileNames = ServiceAgents.Instance.PluginService<IUsbUirtConfigService>().GetAllSetTopBoxProfileNames();
        this.LogDebug("USB-UIRT config: STB profile count = {0}", stbProfileNames.Count);
        column = (DataGridViewComboBoxColumn)dataGridViewConfig.Columns["dataGridViewColumnSetTopBoxProfile"];
        column.DataSource = stbProfileNames;
      }

      this.LogDebug("USB-UIRT config: total tuner count = {0}", tuners.Count);
      foreach (Tuner tuner in tuners)
      {
        BroadcastStandard tunerSupportedBroadcastStandards = (BroadcastStandard)tuner.SupportedBroadcastStandards;
        if (!tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.AnalogTelevision) && !tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput))
        {
          continue;
        }

        TunerSetTopBoxConfig tunerStbConfig = ServiceAgents.Instance.PluginService<IUsbUirtConfigService>().GetSetTopBoxConfigurationForTuner(tuner.ExternalId);
        this.LogDebug("USB-UIRT config: tuner...");
        this.LogDebug("  tuner ID       = {0}", tuner.IdTuner);
        this.LogDebug("  tuner name     = {0}", tuner.Name);
        this.LogDebug("  USB-UIRT index = {0}", tunerStbConfig.UsbUirtIndex);
        this.LogDebug("  transmit zone  = {0}", tunerStbConfig.TransmitZone);
        this.LogDebug("  STB profile    = {0}", tunerStbConfig.ProfileName);
        this.LogDebug("  power control? = {0}", tunerStbConfig.IsPowerControlEnabled);

        DataGridViewRow row = dataGridViewConfig.Rows[dataGridViewConfig.Rows.Add()];
        DataGridViewCell cell = row.Cells["dataGridViewColumnTunerId"];
        cell.Value = tuner.IdTuner.ToString();
        cell.Tag = tuner;

        row.Cells["dataGridViewColumnTunerName"].Value = tuner.Name;

        cell = row.Cells["dataGridViewColumnUsbUirt"];
        cell.Tag = tunerStbConfig;
        UsbUirtDetail selectedUsbUirt;
        if (!comboBoxValueLookup.TryGetValue(tunerStbConfig.UsbUirtIndex, out selectedUsbUirt))
        {
          this.LogWarn("USB-UIRT config: a configured USB-UIRT is not connected, tuner ID = {0}, USB-UIRT index = {1}", tuner.IdTuner, tunerStbConfig.UsbUirtIndex);
          selectedUsbUirt = blankUsbUirtEntry;
        }
        cell.Value = selectedUsbUirt;

        DataGridViewComboBoxCell comboBoxCell = (DataGridViewComboBoxCell)row.Cells["dataGridViewColumnTransmitZone"];
        foreach (string zone in (List<string>)comboBoxCell.DataSource)
        {
          if (string.Equals(zone, tunerStbConfig.TransmitZone.GetDescription()))
          {
            comboBoxCell.Value = zone;
            break;
          }
        }

        comboBoxCell = (DataGridViewComboBoxCell)row.Cells["dataGridViewColumnSetTopBoxProfile"];
        foreach (string profileName in (IList<string>)comboBoxCell.DataSource)
        {
          if (string.Equals(profileName, tunerStbConfig.ProfileName))
          {
            comboBoxCell.Value = profileName;
            break;
          }
        }

        row.Cells["dataGridViewColumnPowerControl"].Value = tunerStbConfig.IsPowerControlEnabled;
      }
      this.LogDebug("USB-UIRT config: analog tuner count = {0}", dataGridViewConfig.Rows.Count);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("USB-UIRT config: deactivating, tuner count = {0}", dataGridViewConfig.Rows.Count);

      ICollection<TunerSetTopBoxConfig> configToSave = new List<TunerSetTopBoxConfig>(dataGridViewConfig.Rows.Count);
      foreach (DataGridViewRow row in dataGridViewConfig.Rows)
      {
        bool isChanged = false;
        DataGridViewCell cell = row.Cells["dataGridViewColumnTunerId"];
        Tuner tuner = cell.Tag as Tuner;

        cell = row.Cells["dataGridViewColumnUsbUirt"];
        TunerSetTopBoxConfig tunerStbConfig = cell.Tag as TunerSetTopBoxConfig;
        UsbUirtDetail newUsbUirt = cell.Value as UsbUirtDetail;
        int newUsbUirtIndex = -1;
        if (newUsbUirt != null)
        {
          newUsbUirtIndex = newUsbUirt.Index;
        }
        if (newUsbUirtIndex != tunerStbConfig.UsbUirtIndex)
        {
          this.LogInfo("USB-UIRT config: USB-UIRT for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.UsbUirtIndex, newUsbUirtIndex);
          tunerStbConfig.UsbUirtIndex = newUsbUirtIndex;
          isChanged = true;
        }

        cell = row.Cells["dataGridViewColumnTransmitZone"];
        string newTransmitZoneString = cell.Value as string;
        TransmitZone newTransmitZone = TransmitZone.None;
        if (!string.IsNullOrEmpty(newTransmitZoneString))
        {
          newTransmitZone = (TransmitZone)(typeof(TransmitZone).GetEnumFromDescription((string)cell.Value));
        }
        if (newTransmitZone != tunerStbConfig.TransmitZone)
        {
          this.LogInfo("USB-UIRT config: transmit zone for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.TransmitZone, newTransmitZone);
          isChanged = true;
        }

        cell = row.Cells["dataGridViewColumnSetTopBoxProfile"];
        string newStbProfile = cell.Value as string;
        if (string.IsNullOrWhiteSpace(newStbProfile))
        {
          newStbProfile = string.Empty;
        }
        if (!string.Equals(newStbProfile, tunerStbConfig.ProfileName))
        {
          this.LogInfo("USB-UIRT config: STB profile for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.ProfileName, newStbProfile);
          isChanged = true;
        }

        cell = row.Cells["dataGridViewColumnPowerControl"];
        bool newIsPowerControlEnabled = (bool)cell.Value;
        if (newIsPowerControlEnabled != tunerStbConfig.IsPowerControlEnabled)
        {
          this.LogInfo("USB-UIRT config: power control for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.IsPowerControlEnabled, newIsPowerControlEnabled);
          tunerStbConfig.IsPowerControlEnabled = newIsPowerControlEnabled;
          isChanged = true;
        }

        if (isChanged)
        {
          configToSave.Add(tunerStbConfig);
        }
      }

      if (configToSave.Count > 0)
      {
        ServiceAgents.Instance.PluginService<IUsbUirtConfigService>().SaveSetTopBoxConfiguration(configToSave);
      }

      base.OnSectionDeActivated();
    }

    private void buttonLearn_Click(object sender, EventArgs e)
    {
      DataGridViewRow row = dataGridViewConfig.SelectedRows[0];
      if (row == null)
      {
        return;
      }
      UsbUirtDetail usbUirt = (UsbUirtDetail)row.Cells["dataGridViewColumnUsbUirt"].Value;
      if (usbUirt == null || usbUirt.Index < 0)
      {
        MessageBox.Show("Please select a USB-UIRT to learn with.", MESSAGE_CAPTION);
        return;
      }

      using (LearnSetTopBox dlg = new LearnSetTopBox((uint)usbUirt.Index))
      {
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          // This works without changing existing selections because:
          // - strings have an Equals() override
          // - we can assume that profiles can't have been deleted
          IList<string> stbProfileNames = ServiceAgents.Instance.PluginService<IUsbUirtConfigService>().GetAllSetTopBoxProfileNames();
          ((DataGridViewComboBoxColumn)dataGridViewConfig.Columns["dataGridViewColumnSetTopBoxProfile"]).DataSource = stbProfileNames;
        }
      }
    }

    private void buttonTest_Click(object sender, System.EventArgs e)
    {
      DataGridViewRow row = dataGridViewConfig.SelectedRows[0];
      if (row == null)
      {
        return;
      }
      UsbUirtDetail usbUirt = (UsbUirtDetail)row.Cells["dataGridViewColumnUsbUirt"].Value;
      if (usbUirt == null || usbUirt.Index < 0)
      {
        MessageBox.Show("Please select a USB-UIRT to test.", MESSAGE_CAPTION);
        return;
      }
      string transmitZoneString = (string)row.Cells["dataGridViewColumnTransmitZone"].Value;
      if (string.IsNullOrEmpty(transmitZoneString))
      {
        MessageBox.Show("Please select a zone to test.", MESSAGE_CAPTION);
        return;
      }

      TransmitZone transmitZone = (TransmitZone)(typeof(TransmitZone).GetEnumFromDescription(transmitZoneString));

      string stbProfileName = (string)row.Cells["dataGridViewColumnSetTopBoxProfile"].Value;
      this.LogInfo("USB-UIRT config: test");
      this.LogInfo("  USB-UIRT index = {0}", usbUirt.Index);
      this.LogInfo("  transmit zone  = {0}", transmitZone);
      this.LogInfo("  STB profile    = {0}", stbProfileName);
      this.LogInfo("  channel #      = {0}", numericUpDownTest.Value);
      TransmitResult result = ServiceAgents.Instance.PluginService<IUsbUirtConfigService>().Transmit((uint)usbUirt.Index, transmitZone, stbProfileName, numericUpDownTest.Value.ToString());
      this.LogInfo("  result         = {0}", result);
      if (result == TransmitResult.Success)
      {
        MessageBox.Show("The channel number was transmitted successfully.");
        return;
      }

      if (result == TransmitResult.Fail)
      {
        MessageBox.Show("Failed to transmit the test channel number." + Environment.NewLine + SENTENCE_CHECK_LOG_FILES, MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.InvalidCommand)
      {
        MessageBox.Show("At least one of the commands required to transmit the channel number is not valid. Please try (re-)learning the commands.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.InvalidProfile)
      {
        MessageBox.Show("The selected set-top-box profile is not valid. Please try (re-)learning the commands.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.NotOpen)
      {
        MessageBox.Show("The selected USB-UIRT's interface is currently closed.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.Unavailable)
      {
        MessageBox.Show("The selected USB-UIRT seems to have been disconnected.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.ZoneNotAvailable)
      {
        MessageBox.Show("There is no emitter connected to the selected port.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void dataGridViewConfig_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
      if (
        dataGridViewConfig.CurrentCell == null ||
        !dataGridViewConfig.CurrentCell.OwningColumn.Name.Equals("dataGridViewColumnUsbUirt") ||
        dataGridViewConfig.CurrentCell.RowIndex < 0
      )
      {
        return;
      }

      DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)dataGridViewConfig.CurrentCell.OwningRow.Cells["dataGridViewColumnTransmitZone"];
      TransmitZone selectedZoneBefore = TransmitZone.None;
      if (!string.IsNullOrEmpty((string)cell.Value))
      {
        selectedZoneBefore = (TransmitZone)(typeof(TransmitZone).GetEnumFromDescription((string)cell.Value));
      }

      List<string> cellDataSource;
      string selectedZoneAfter = string.Empty;
      UsbUirtDetail detail = dataGridViewConfig.CurrentCell.Value as UsbUirtDetail;
      if (detail == null || detail.Index < 0 || detail.TransmitZones == TransmitZone.None)
      {
        cellDataSource = new List<string>(1) { string.Empty };
      }
      else
      {
        cellDataSource = new List<string>();
        foreach (TransmitZone zone in System.Enum.GetValues(typeof(TransmitZone)))
        {
          if (detail.TransmitZones.HasFlag(zone))
          {
            string z = zone.GetDescription();
            cellDataSource.Add(z);
            if (zone == selectedZoneBefore)
            {
              selectedZoneAfter = z;
            }
          }
        }
      }
      cell.Value = selectedZoneAfter;
    }

    private void dataGridViewConfig_SelectionChanged(object sender, EventArgs e)
    {
      buttonLearn.Enabled = dataGridViewConfig.SelectedRows != null && dataGridViewConfig.SelectedRows.Count == 1;
      buttonTest.Enabled = buttonLearn.Enabled;
    }
  }
}