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
using Mediaportal.TV.Server.Common.Types.Channel.Constant;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;
using TuningDetail = Mediaportal.TV.Server.SetupTV.Sections.Helpers.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class ScanSatellite : SectionSettings
  {
    public const BroadcastStandard SUPPORTED_BROADCAST_STANDARDS = BroadcastStandard.MaskSatellite;

    #region variables

    private readonly int _tunerId;
    private ChannelScanHelper _scanHelper = null;
    private ScanState _scanState = ScanState.Initialized;

    #endregion

    public ScanSatellite(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
    }

    #region activate/deactivate

    public override void OnSectionActivated()
    {
      this.LogDebug("scan satellite: activating, tuner ID = {0}", _tunerId);

      // First activation.
      if (comboBoxScanType.Items.Count == 0)
      {
        groupBoxAdvancedOptions.Top = groupBoxProgress.Top;

        comboBoxScanType.Items.AddRange(typeof(ScanType).GetDescriptions());
        comboBoxScanType.SelectedIndex = 0;
        comboBoxPolarisation.Items.AddRange(typeof(Polarisation).GetDescriptions());
        comboBoxPilotTonesState.Items.AddRange(typeof(PilotTonesState).GetDescriptions());
      }

      if (_scanState == ScanState.Initialized)
      {
        IList<TunerSatellite> tunerSatellites = ServiceAgents.Instance.TunerServiceAgent.ListAllTunerSatellitesByTuner(_tunerId, TunerSatelliteRelation.Satellite);
        if (tunerSatellites.Count == 0)
        {
          Enabled = false;
          MessageBox.Show("Please configure the available satellites for this tuner first.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Information);
          return;
        }
        Enabled = true;

        int longitude = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "Satellite", Satellite.DefaultSatelliteLongitude.GetValueOrDefault(100000));
        comboBoxSatellite.BeginUpdate();
        try
        {
          comboBoxSatellite.Items.Clear();
          HashSet<int> seenSatelliteIds = new HashSet<int>();
          foreach (TunerSatellite tunerSatellite in tunerSatellites)
          {
            if (!seenSatelliteIds.Contains(tunerSatellite.IdSatellite))
            {
              seenSatelliteIds.Add(tunerSatellite.IdSatellite);
              comboBoxSatellite.Items.Add(tunerSatellite.Satellite);
              if (longitude == tunerSatellite.Satellite.Longitude)
              {
                comboBoxSatellite.SelectedIndex = comboBoxSatellite.Items.Count - 1;
              }
            }
          }
          if (comboBoxSatellite.SelectedItem == null)
          {
            comboBoxSatellite.SelectedIndex = 0;
          }
        }
        finally
        {
          comboBoxSatellite.EndUpdate();
        }

        string transmitter = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "Transmitter", TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
        foreach (object item in comboBoxTransmitter.Items)
        {
          if (string.Equals(item.ToString(), transmitter))
          {
            comboBoxTransmitter.SelectedItem = item;
            break;
          }
        }
        if (comboBoxTransmitter.SelectedItem == null)
        {
          comboBoxTransmitter.SelectedIndex = 0;
        }

        Tuner tuner = ServiceAgents.Instance.TunerServiceAgent.GetTuner(_tunerId, TunerRelation.None);
        comboBoxBroadcastStandard.Items.Clear();
        comboBoxBroadcastStandard.Items.AddRange(typeof(BroadcastStandard).GetDescriptions(tuner.SupportedBroadcastStandards & (int)SUPPORTED_BROADCAST_STANDARDS, false));
        comboBoxBroadcastStandard.SelectedItem = ((BroadcastStandard)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "BroadcastStandard", (int)BroadcastStandard.DvbS)).GetDescription();
        if (comboBoxBroadcastStandard.SelectedItem == null)
        {
          comboBoxBroadcastStandard.SelectedIndex = 0;
        }
      }

      numericTextBoxFrequency.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "Frequency", 11097000);
      comboBoxPolarisation.SelectedItem = ((Polarisation)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "Polarisation", (int)Polarisation.Automatic)).GetDescription();
      if (comboBoxPolarisation.SelectedItem == null)
      {
        comboBoxPolarisation.SelectedIndex = 0;
      }
      comboBoxModulation.SelectedItem = ((ModulationSchemePsk)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "Modulation", (int)ModulationSchemePsk.Automatic)).GetDescription();
      if (comboBoxModulation.SelectedItem == null)
      {
        comboBoxModulation.SelectedIndex = 0;
      }
      numericTextBoxSymbolRate.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "SymbolRate", 25000);
      comboBoxFecCodeRate.SelectedItem = ((FecCodeRate)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "FecCodeRate", (int)FecCodeRate.Automatic)).GetDescription();
      if (comboBoxFecCodeRate.SelectedItem == null)
      {
        comboBoxFecCodeRate.SelectedIndex = 0;
      }
      comboBoxRollOffFactor.SelectedItem = ((RollOffFactor)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "RollOffFactor", (int)RollOffFactor.Automatic)).GetDescription();
      if (comboBoxRollOffFactor.SelectedItem == null)
      {
        comboBoxRollOffFactor.SelectedIndex = 0;
      }
      comboBoxPilotTonesState.SelectedItem = ((PilotTonesState)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "PilotTonesState", (int)PilotTonesState.Automatic)).GetDescription();
      if (comboBoxPilotTonesState.SelectedItem == null)
      {
        comboBoxPilotTonesState.SelectedIndex = 0;
      }
      numericTextBoxInputStreamId.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "InputStreamId", -1);

      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("scan satellite: deactivating, tuner ID = {0}", _tunerId);

      DebugSettings();
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "Satellite", ((Satellite)comboBoxSatellite.SelectedItem).Longitude);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "Transmitter", comboBoxTransmitter.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "BroadcastStandard", Convert.ToInt32(typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "Frequency", numericTextBoxFrequency.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "Polarisation", Convert.ToInt32(typeof(Polarisation).GetEnumFromDescription((string)comboBoxPolarisation.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "Modulation", Convert.ToInt32(typeof(ModulationSchemePsk).GetEnumFromDescription((string)comboBoxModulation.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "SymbolRate", numericTextBoxSymbolRate.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "FecCodeRate", Convert.ToInt32(typeof(FecCodeRate).GetEnumFromDescription((string)comboBoxFecCodeRate.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "RollOffFactor", Convert.ToInt32(typeof(RollOffFactor).GetEnumFromDescription((string)comboBoxRollOffFactor.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "PilotTonesState", Convert.ToInt32(typeof(PilotTonesState).GetEnumFromDescription((string)comboBoxPilotTonesState.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "InputStreamId", numericTextBoxInputStreamId.Value);

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("  satellite         = {0}", comboBoxSatellite.SelectedItem);
      this.LogDebug("  transmitter       = {0}", comboBoxTransmitter.SelectedItem);
      this.LogDebug("  standard          = {0}", comboBoxBroadcastStandard.SelectedItem);
      this.LogDebug("  frequency         = {0} kHz", numericTextBoxFrequency.Text);
      this.LogDebug("  polarisation      = {0}", comboBoxPolarisation.SelectedItem);
      this.LogDebug("  modulation        = {0}", comboBoxModulation.SelectedItem);
      this.LogDebug("  symbol rate       = {0} ks/s", numericTextBoxSymbolRate.Text);
      this.LogDebug("  FEC code rate     = {0}", comboBoxFecCodeRate.SelectedItem);
      this.LogDebug("  roll-off factor   = {0}", comboBoxRollOffFactor.SelectedItem);
      this.LogDebug("  pilot tones state = {0}", comboBoxPilotTonesState.SelectedItem);
      this.LogDebug("  input stream ID   = {0}", numericTextBoxInputStreamId.Text);
    }

    #endregion

    #region scan handling

    private TuningDetail GetManualTuning()
    {
      TuningDetail tuningDetail = new TuningDetail();
      tuningDetail.BroadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      Satellite satellite = (Satellite)comboBoxSatellite.SelectedItem;
      tuningDetail.Longitude = satellite.Longitude;
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.Polarisation = (Polarisation)typeof(Polarisation).GetEnumFromDescription((string)comboBoxPolarisation.SelectedItem);
      tuningDetail.ModulationScheme = ((ModulationSchemePsk)typeof(ModulationSchemePsk).GetEnumFromDescription((string)comboBoxModulation.SelectedItem)).ToString();
      tuningDetail.SymbolRate = numericTextBoxSymbolRate.Value;
      tuningDetail.FecCodeRate = (FecCodeRate)typeof(FecCodeRate).GetEnumFromDescription((string)comboBoxFecCodeRate.SelectedItem);
      if (comboBoxRollOffFactor.Enabled)
      {
        tuningDetail.RollOffFactor = (RollOffFactor)typeof(RollOffFactor).GetEnumFromDescription((string)comboBoxRollOffFactor.SelectedItem);
      }
      if (comboBoxPilotTonesState.Enabled)
      {
        tuningDetail.PilotTonesState = (PilotTonesState)typeof(PilotTonesState).GetEnumFromDescription((string)comboBoxPilotTonesState.SelectedItem);
      }
      if (numericTextBoxInputStreamId.Enabled)
      {
        tuningDetail.StreamId = numericTextBoxInputStreamId.Value;
      }
      return tuningDetail;
    }

    private void buttonScan_Click(object sender, EventArgs e)
    {
      if (_scanState == ScanState.Done)
      {
        buttonScan.Text = "&Scan for channels";
        _scanState = ScanState.Initialized;
        ShowOrHideScanProgress(false);
        return;
      }
      else if (_scanState == ScanState.Scanning)
      {
        buttonScan.Enabled = false;
        buttonScan.Text = "Stopping...";
        _scanState = ScanState.Stopping;
        if (_scanHelper != null)
        {
          _scanHelper.StopScan();
        }
        return;
      }

      Satellite satellite = (Satellite)comboBoxSatellite.SelectedItem;
      this.LogInfo("scan satellite: start scanning, satellite = {0}", satellite);

      IList<TuningDetail> tuningDetails = null;
      if (checkBoxUseAdvancedOptions.Enabled && checkBoxUseAdvancedOptions.Checked && checkBoxUseManualTuning.Checked)
      {
        TuningDetail tuningDetail = GetManualTuning();
        if (!ServiceAgents.Instance.ControllerServiceAgent.CanTune(_tunerId, tuningDetail.GetTuningChannel()))
        {
          MessageBox.Show("The tuner cannot tune with the configured settings.", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        tuningDetails = new List<TuningDetail> { tuningDetail };
      }
      else
      {
        tuningDetails = TuningDetailFilter.GetTuningDetails(comboBoxTransmitter);
        if (tuningDetails == null || tuningDetails.Count == 0)
        {
          return;
        }
      }

      _scanHelper = new ChannelScanHelper(_tunerId, listViewProgress, progressBarProgress, null, OnScanCompleted, progressBarSignalStrength, progressBarSignalQuality);
      ScanType scanType = ScanType.Standard;
      if (checkBoxUseAdvancedOptions.Enabled && checkBoxUseAdvancedOptions.Checked)
      {
        scanType = (ScanType)typeof(ScanType).GetEnumFromDescription((string)comboBoxScanType.SelectedItem);
      }
      if (_scanHelper.StartScan(tuningDetails, scanType))
      {
        _scanState = ScanState.Scanning;
        buttonScan.Text = "&Stop";
        ShowOrHideScanProgress(true);
      }
    }

    private void OnScanCompleted()
    {
      _scanState = ScanState.Done;
      this.Invoke((MethodInvoker)delegate
      {
        buttonScan.Text = "&New scan";
        buttonScan.Enabled = true;
      });
      _scanHelper = null;
    }

    #endregion

    #region GUI handling

    private void ShowOrHideScanProgress(bool showScanProgress)
    {
      comboBoxSatellite.Enabled = !showScanProgress;
      comboBoxTransmitter.Enabled = !showScanProgress;
      groupBoxProgress.Visible = showScanProgress;

      if (showScanProgress)
      {
        checkBoxUseAdvancedOptions.Enabled = false;
        groupBoxAdvancedOptions.Visible = false;
        listViewProgress.Items.Clear();
        groupBoxProgress.BringToFront();
        UpdateZOrder();
      }
      else
      {
        checkBoxUseAdvancedOptions.Enabled = !string.Equals(comboBoxTransmitter.SelectedItem.ToString(), TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
        groupBoxAdvancedOptions.Visible = checkBoxUseAdvancedOptions.Checked;
        if (groupBoxAdvancedOptions.Visible)
        {
          groupBoxAdvancedOptions.BringToFront();
          UpdateZOrder();
        }
      }
    }

    private void comboBoxSatellite_SelectedIndexChanged(object sender, EventArgs e)
    {
      Satellite satellite = (Satellite)comboBoxSatellite.SelectedItem;
      TuningDetailFilter.Load(_tunerId, TuningDetailGroup.Satellite, satellite.ToString() + ".xml", comboBoxTransmitter);
    }

    private void comboBoxTransmitter_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool enableAdvancedOptions = !string.Equals(comboBoxTransmitter.SelectedItem.ToString(), TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
      checkBoxUseAdvancedOptions.Enabled = enableAdvancedOptions;
      groupBoxAdvancedOptions.Enabled = enableAdvancedOptions;
    }

    private void checkBoxUseAdvancedOptions_CheckedChanged(object sender, EventArgs e)
    {
      groupBoxAdvancedOptions.Visible = !groupBoxAdvancedOptions.Visible;
    }

    private void checkBoxUseManualTuning_CheckedChanged(object sender, EventArgs e)
    {
      groupBoxManualTuning.Enabled = checkBoxUseManualTuning.Checked;
    }

    private void comboBoxBroadcastStandard_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (comboBoxBroadcastStandard.SelectedItem == null)
      {
        return;
      }
      BroadcastStandard broadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      bool isDvbS2 = BroadcastStandard.MaskDvbS2.HasFlag(broadcastStandard);
      comboBoxRollOffFactor.Enabled = isDvbS2 || BroadcastStandard.DvbDsng == broadcastStandard;
      comboBoxPilotTonesState.Enabled = isDvbS2;
      numericTextBoxInputStreamId.Enabled = isDvbS2;

      string selectedModulationDescription = (string)comboBoxModulation.SelectedItem;
      comboBoxModulation.BeginUpdate();
      try
      {
        comboBoxModulation.Items.Clear();
        string[] newModulationDescriptions = typeof(ModulationSchemePsk).GetDescriptions(ModCod.SATELLITE[broadcastStandard]);
        foreach (string modulationDescription in newModulationDescriptions)
        {
          comboBoxModulation.Items.Add(modulationDescription);
          if (string.Equals(modulationDescription, selectedModulationDescription))
          {
            comboBoxModulation.SelectedIndex = comboBoxModulation.Items.Count - 1;
          }
        }
        if (comboBoxModulation.SelectedItem == null)
        {
          comboBoxModulation.SelectedIndex = 0;
        }
      }
      finally
      {
        comboBoxModulation.EndUpdate();
      }
      // Ensure that the choices for FEC code rate are updated when the
      // modulation scheme selection doesn't change. This may be unnecessary.
      comboBoxModulation_SelectedIndexChanged(null, null);

      string selectedRollOffFactorDescription = (string)comboBoxRollOffFactor.SelectedItem;
      comboBoxRollOffFactor.BeginUpdate();
      try
      {
        comboBoxRollOffFactor.Items.Clear();
        string[] newRollOffFactorDescriptions = typeof(RollOffFactor).GetDescriptions(ModCod.SATELLITE_ROLL_OFF_FACTOR[broadcastStandard]);
        foreach (string rollOffFactorDescription in newRollOffFactorDescriptions)
        {
          comboBoxRollOffFactor.Items.Add(rollOffFactorDescription);
          if (string.Equals(rollOffFactorDescription, selectedRollOffFactorDescription))
          {
            comboBoxRollOffFactor.SelectedIndex = comboBoxRollOffFactor.Items.Count - 1;
          }
        }
        if (comboBoxRollOffFactor.SelectedItem == null)
        {
          comboBoxRollOffFactor.SelectedIndex = 0;
        }
      }
      finally
      {
        comboBoxRollOffFactor.EndUpdate();
      }
    }

    private void comboBoxModulation_SelectedIndexChanged(object sender, EventArgs e)
    {
      BroadcastStandard broadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      ModulationSchemePsk modulationScheme = (ModulationSchemePsk)typeof(ModulationSchemePsk).GetEnumFromDescription((string)comboBoxModulation.SelectedItem);
      string selectedFecCodeRateDescription = (string)comboBoxFecCodeRate.SelectedItem;
      comboBoxFecCodeRate.BeginUpdate();
      try
      {
        comboBoxFecCodeRate.Items.Clear();
        string[] newFecCodeRateDescriptions = typeof(FecCodeRate).GetDescriptions(ModCod.SATELLITE_CODE_RATE[broadcastStandard][modulationScheme]);
        foreach (string fecCodeRateDescription in newFecCodeRateDescriptions)
        {
          comboBoxFecCodeRate.Items.Add(fecCodeRateDescription);
          if (string.Equals(fecCodeRateDescription, selectedFecCodeRateDescription))
          {
            comboBoxFecCodeRate.SelectedIndex = comboBoxFecCodeRate.Items.Count - 1;
          }
        }
        if (comboBoxFecCodeRate.SelectedItem == null)
        {
          comboBoxFecCodeRate.SelectedIndex = 0;
        }
      }
      finally
      {
        comboBoxFecCodeRate.EndUpdate();
      }
    }

    #endregion
  }
}