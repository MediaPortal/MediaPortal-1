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
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Config
{
  internal partial class MicrosoftBlasterConfig : SectionSettings
  {
    private class ComboBoxTransmitPort
    {
      private TransmitPort _port = TransmitPort.None;
      private bool _isConnected = false;
      private string _displayName = string.Empty;

      public ComboBoxTransmitPort(TransmitPort port, bool isConnected)
      {
        _port = port;
        _isConnected = isConnected;

        if (_port != TransmitPort.None)
        {
          List<string> numbers = new List<string>();
          Array allPorts = System.Enum.GetValues(typeof(TransmitPort));
          foreach (TransmitPort p in allPorts)
          {
            if (p != TransmitPort.None && port.HasFlag(p))
            {
              numbers.Add(p.GetDescription());
              port &= ~p;
            }
          }

          _displayName = string.Join(", ", numbers);
          if (!_isConnected)
          {
            _displayName += "*";
          }
        }
      }

      public TransmitPort Port
      {
        get
        {
          return _port;
        }
      }

      public bool IsConnected
      {
        get
        {
          return _isConnected;
        }
      }

      public override string ToString()
      {
        return _displayName;
      }
    }

    private string _learningTransceiverDevicePath = null;

    public MicrosoftBlasterConfig()
      : base("Microsoft Blaster")
    {
      ServiceAgents.Instance.AddGenericService<IMicrosoftBlasterConfigService>();
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("Microsoft blaster config: activating");

      dataGridViewConfig.Rows.Clear();
      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None);

      IDictionary<string, TransceiverDetail> comboBoxValueLookup = null;
      TransceiverDetail blankTransceiverEntry = new TransceiverDetail();
      if (tuners.Count > 0)
      {
        ICollection<TransceiverDetail> connectedTransceiverDetails = ServiceAgents.Instance.PluginService<IMicrosoftBlasterConfigService>().GetAllTransceiverDetails();
        this.LogDebug("Microsoft blaster config: transceiver count = {0}", connectedTransceiverDetails.Count);
        List<TransceiverDetail> columnDataSource = new List<TransceiverDetail>(connectedTransceiverDetails.Count + 1);
        columnDataSource.Add(blankTransceiverEntry);
        comboBoxValueLookup.Add(blankTransceiverEntry.DevicePath, blankTransceiverEntry);
        foreach (TransceiverDetail detail in connectedTransceiverDetails)
        {
          this.LogDebug("  {0}", detail.DevicePath);
          this.LogDebug("    is receive supported?      = {0}", detail.IsReceiveSupported);
          this.LogDebug("    is learning supported?     = {0}", detail.IsLearningSupported);
          this.LogDebug("    transmit port(s)           = [{0}]", detail.AllTransmitPorts);
          this.LogDebug("    connected transmit port(s) = [{0}]", detail.ConnectedTransmitPorts);
          comboBoxValueLookup.Add(detail.DevicePath, detail);
        }
        columnDataSource.AddRange(connectedTransceiverDetails);

        DataGridViewComboBoxColumn column = (DataGridViewComboBoxColumn)dataGridViewConfig.Columns["dataGridViewColumnTransceiver"];
        column.DataSource = columnDataSource;

        IList<string> stbProfileNames = ServiceAgents.Instance.PluginService<IMicrosoftBlasterConfigService>().GetAllSetTopBoxProfileNames();
        this.LogDebug("Microsoft blaster config: STB profile count = {0}", stbProfileNames.Count);
        column = (DataGridViewComboBoxColumn)dataGridViewConfig.Columns["dataGridViewColumnSetTopBoxProfile"];
        column.DataSource = stbProfileNames;
      }

      this.LogDebug("Microsoft blaster config: total tuner count = {0}", tuners.Count);
      foreach (Tuner tuner in tuners)
      {
        BroadcastStandard tunerSupportedBroadcastStandards = (BroadcastStandard)tuner.SupportedBroadcastStandards;
        if (!tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.AnalogTelevision) && !tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput))
        {
          continue;
        }

        SetTopBoxConfig tunerStbConfig = ServiceAgents.Instance.PluginService<IMicrosoftBlasterConfigService>().GetSetTopBoxConfigurationForTuner(tuner.ExternalId);
        this.LogDebug("Microsoft blaster config: tuner...");
        this.LogDebug("  tuner ID       = {0}", tuner.IdTuner);
        this.LogDebug("  tuner name     = {0}", tuner.Name);
        this.LogDebug("  transceiver    = {0}", tunerStbConfig.TransceiverDevicePath);
        this.LogDebug("  transmit port  = {0}", tunerStbConfig.TransmitPort);
        this.LogDebug("  STB profile    = {0}", tunerStbConfig.ProfileName);
        this.LogDebug("  power control? = {0}", tunerStbConfig.IsPowerControlEnabled);

        DataGridViewRow row = dataGridViewConfig.Rows[dataGridViewConfig.Rows.Add()];
        DataGridViewCell cell = row.Cells["dataGridViewColumnTunerId"];
        cell.Value = tuner.IdTuner.ToString();
        cell.Tag = tuner;

        row.Cells["dataGridViewColumnTunerName"].Value = tuner.Name;

        cell = row.Cells["dataGridViewColumnTransceiver"];
        cell.Tag = tunerStbConfig;
        TransceiverDetail selectedTransceiver;
        if (!comboBoxValueLookup.TryGetValue(tunerStbConfig.TransceiverDevicePath, out selectedTransceiver))
        {
          this.LogWarn("Microsoft blaster config: a configured transceiver is not connected, tuner ID = {0}, transceiver device path = {1}", tuner.IdTuner, tunerStbConfig.TransceiverDevicePath);
          selectedTransceiver = blankTransceiverEntry;
        }
        cell.Value = selectedTransceiver;

        DataGridViewComboBoxCell comboBoxCell = (DataGridViewComboBoxCell)row.Cells["dataGridViewColumnTransmitPort"];
        foreach (ComboBoxTransmitPort port in (List<ComboBoxTransmitPort>)comboBoxCell.DataSource)
        {
          if (port.Port == tunerStbConfig.TransmitPort)
          {
            comboBoxCell.Value = port;
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
      this.LogDebug("Microsoft blaster config: analog tuner count = {0}", dataGridViewConfig.Rows.Count);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("Microsoft blaster config: deactivating, tuner count = {0}", dataGridViewConfig.Rows.Count);

      ICollection<SetTopBoxConfig> settings = new List<SetTopBoxConfig>(dataGridViewConfig.Rows.Count);
      foreach (DataGridViewRow row in dataGridViewConfig.Rows)
      {
        bool isChanged = false;
        DataGridViewCell cell = row.Cells["dataGridViewColumnTunerId"];
        Tuner tuner = cell.Tag as Tuner;

        cell = row.Cells["dataGridViewColumnTransceiver"];
        SetTopBoxConfig tunerStbConfig = cell.Tag as SetTopBoxConfig;
        TransceiverDetail newTransceiver = cell.Value as TransceiverDetail;
        string newTransceiverDevicePath = string.Empty;
        if (newTransceiver != null && !string.IsNullOrWhiteSpace(newTransceiver.DevicePath))
        {
          newTransceiverDevicePath = newTransceiver.DevicePath;
        }
        if (!string.Equals(newTransceiverDevicePath, tunerStbConfig.TransceiverDevicePath))
        {
          this.LogInfo("Microsoft blaster config: transceiver for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.TransceiverDevicePath, newTransceiverDevicePath);
          tunerStbConfig.TransceiverDevicePath = newTransceiverDevicePath;
          isChanged = true;
        }

        cell = row.Cells["dataGridViewColumnTransmitPort"];
        ComboBoxTransmitPort newTransmitPortItem = cell.Value as ComboBoxTransmitPort;
        TransmitPort newTransmitPort = TransmitPort.None;
        if (newTransmitPortItem != null)
        {
          newTransmitPort = newTransmitPortItem.Port;
        }
        if (newTransmitPort != tunerStbConfig.TransmitPort)
        {
          this.LogInfo("Microsoft blaster config: transmit port for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.TransmitPort, newTransmitPort);
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
          this.LogInfo("Microsoft blaster config: STB profile for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.ProfileName, newStbProfile);
          isChanged = true;
        }

        cell = row.Cells["dataGridViewColumnPowerControl"];
        bool newIsPowerControlEnabled = (bool)cell.Value;
        if (newIsPowerControlEnabled != tunerStbConfig.IsPowerControlEnabled)
        {
          this.LogInfo("Microsoft blaster config: power control for tuner {0} changed from {1} to {2}", tuner.IdTuner, tunerStbConfig.IsPowerControlEnabled, newIsPowerControlEnabled);
          tunerStbConfig.IsPowerControlEnabled = newIsPowerControlEnabled;
          isChanged = true;
        }

        if (isChanged)
        {
          settings.Add(tunerStbConfig);
        }
      }

      if (settings.Count > 0)
      {
        ServiceAgents.Instance.PluginService<IMicrosoftBlasterConfigService>().SaveSetTopBoxConfiguration(settings);
      }

      base.OnSectionDeActivated();
    }

    private LearnResult LearnCommand(TimeSpan timeLimit, out string command)
    {
      return ServiceAgents.Instance.PluginService<IMicrosoftBlasterConfigService>().Learn(_learningTransceiverDevicePath, timeLimit, out command);
    }

    private void buttonLearn_Click(object sender, EventArgs e)
    {
      DataGridViewRow row = dataGridViewConfig.SelectedRows[0];
      if (row == null)
      {
        return;
      }
      TransceiverDetail transceiver = (TransceiverDetail)row.Cells["dataGridViewColumnTransceiver"].Value;
      if (transceiver == null || string.IsNullOrEmpty(transceiver.DevicePath))
      {
        MessageBox.Show("Please select a transceiver to learn with.", MESSAGE_CAPTION);
        return;
      }
      if (!transceiver.IsLearningSupported)
      {
        MessageBox.Show("The selected transceiver does not support learning.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      _learningTransceiverDevicePath = transceiver.DevicePath;
      this.LogInfo("Microsoft blaster config: learn, transceiver = {0}", _learningTransceiverDevicePath);
      LearnSetTopBox learnDlg = new LearnSetTopBox(LearnCommand);
      if (learnDlg.ShowDialog() == DialogResult.OK)
      {
        this.LogInfo("Microsoft blaster config: save STB profile, name = {0}", learnDlg.Profile.Name);
        if (!ServiceAgents.Instance.PluginService<IMicrosoftBlasterConfigService>().SaveSetTopBoxProfile(learnDlg.Profile))
        {
          this.LogError("Microsoft blaster config: failed to save STB profile, name = {0}", learnDlg.Profile.Name);
          MessageBox.Show("Failed to save the profile." + Environment.NewLine + SENTENCE_CHECK_LOG_FILES, MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        // This works without changing existing selections because:
        // - strings have an Equals() override
        // - we can assume that profiles can't have been deleted
        IList<string> stbProfileNames = ServiceAgents.Instance.PluginService<IMicrosoftBlasterConfigService>().GetAllSetTopBoxProfileNames();
        ((DataGridViewComboBoxColumn)dataGridViewConfig.Columns["dataGridViewColumnSetTopBoxProfile"]).DataSource = stbProfileNames;
      }
      _learningTransceiverDevicePath = null;
    }

    private void buttonTest_Click(object sender, System.EventArgs e)
    {
      DataGridViewRow row = dataGridViewConfig.SelectedRows[0];
      if (row == null)
      {
        return;
      }
      TransceiverDetail transceiver = (TransceiverDetail)row.Cells["dataGridViewColumnTransceiver"].Value;
      if (transceiver == null || string.IsNullOrEmpty(transceiver.DevicePath))
      {
        MessageBox.Show("Please select a transceiver to test.", MESSAGE_CAPTION);
        return;
      }
      ComboBoxTransmitPort transmitPort = (ComboBoxTransmitPort)row.Cells["dataGridViewColumnTransmitPort"].Value;
      if (transmitPort == null || transmitPort.Port == TransmitPort.None)
      {
        MessageBox.Show("Please select a port to test.", MESSAGE_CAPTION);
        return;
      }
      if (!transmitPort.IsConnected)
      {
        MessageBox.Show("An emitter is not connected to the selected port. Please select a different port to test.", MESSAGE_CAPTION);
        return;
      }

      string stbProfileName = (string)row.Cells["dataGridViewColumnSetTopBoxProfile"].Value;
      this.LogInfo("Microsoft blaster config: test");
      this.LogInfo("  transceiver   = {0}", transceiver.DevicePath);
      this.LogInfo("  transmit port = {0}", transmitPort.Port);
      this.LogInfo("  STB profile   = {0}", stbProfileName);
      this.LogInfo("  channel #     = {0}", numericUpDownTest.Value);
      TransmitResult result = ServiceAgents.Instance.PluginService<IMicrosoftBlasterConfigService>().Transmit(transceiver.DevicePath, transmitPort.Port, stbProfileName, numericUpDownTest.Value.ToString());
      this.LogInfo("  result        = {0}", result);
      if (result == TransmitResult.Success)
      {
        MessageBox.Show("The channel number was transmitted successfully.");
        return;
      }

      if (result == TransmitResult.EmitterNotConnected)
      {
        MessageBox.Show("An emitter is not connected to the selected port.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.Fail)
      {
        MessageBox.Show("Failed to transmit the test channel number." + Environment.NewLine + SENTENCE_CHECK_LOG_FILES, MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.InvalidCommand)
      {
        MessageBox.Show("One of the commands is not valid. Please try (re-)learning the commands.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.InvalidProfile)
      {
        MessageBox.Show("The selected set-top-box profile is not valid. Please try (re-)learning the commands.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.NotOpen)
      {
        MessageBox.Show("The selected transceiver's interface is currently closed.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.Unavailable)
      {
        MessageBox.Show("The selected transceiver is currently not available.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else if (result == TransmitResult.Unsupported)
      {
        MessageBox.Show("The selected transceiver does not support learning.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void dataGridViewConfig_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
      if (
        dataGridViewConfig.CurrentCell == null ||
        !dataGridViewConfig.CurrentCell.OwningColumn.Name.Equals("dataGridViewColumnTransceiver") ||
        dataGridViewConfig.CurrentCell.RowIndex < 0
      )
      {
        return;
      }

      DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)dataGridViewConfig.CurrentCell.OwningRow.Cells["dataGridViewColumnTransmitPort"];
      TransmitPort selectedPortBefore = TransmitPort.None;
      if (cell.Value != null)
      {
        selectedPortBefore = ((ComboBoxTransmitPort)cell.Value).Port;
      }

      List<ComboBoxTransmitPort> cellDataSource;
      ComboBoxTransmitPort blankEntry = new ComboBoxTransmitPort(TransmitPort.None, false);
      ComboBoxTransmitPort selectedPortAfter = blankEntry;
      TransceiverDetail detail = dataGridViewConfig.CurrentCell.Value as TransceiverDetail;
      if (detail == null || string.IsNullOrEmpty(detail.DevicePath) || detail.AllTransmitPorts == TransmitPort.None)
      {
        cellDataSource = new List<ComboBoxTransmitPort>(1) { blankEntry };
      }
      else
      {
        cellDataSource = new List<ComboBoxTransmitPort>();
        cellDataSource.Add(blankEntry);
        foreach (TransmitPort port in System.Enum.GetValues(typeof(TransmitPort)))
        {
          if (detail.AllTransmitPorts.HasFlag(port))
          {
            ComboBoxTransmitPort p = new ComboBoxTransmitPort(port, detail.ConnectedTransmitPorts.HasFlag(port));
            cellDataSource.Add(p);
            if (port == selectedPortBefore)
            {
              selectedPortAfter = p;
            }
          }
        }
      }
      cell.Value = selectedPortAfter;
    }

    private void dataGridViewConfig_SelectionChanged(object sender, EventArgs e)
    {
      buttonLearn.Enabled = dataGridViewConfig.SelectedRows != null && dataGridViewConfig.SelectedRows.Count == 1;
      buttonTest.Enabled = buttonLearn.Enabled;
    }
  }
}