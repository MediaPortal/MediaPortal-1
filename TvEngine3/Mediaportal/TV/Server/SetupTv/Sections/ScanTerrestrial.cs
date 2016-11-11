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
  public partial class ScanTerrestrial : SectionSettings
  {
    public const BroadcastStandard SUPPORTED_BROADCAST_STANDARDS = BroadcastStandard.DvbT | BroadcastStandard.DvbT2 | BroadcastStandard.IsdbT;

    #region variables

    private readonly int _tunerId;
    private TuningDetailFilter _tuningDetailFilter;
    private ChannelScanHelper _scanHelper = null;
    private ScanState _scanState = ScanState.Initialized;

    #endregion

    public ScanTerrestrial(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
    }

    #region activate/deactivate

    public override void OnSectionActivated()
    {
      this.LogDebug("scan terrestrial: activating, tuner ID = {0}", _tunerId);

      // First activation.
      if (comboBoxScanType.Items.Count == 0)
      {
        groupBoxAdvancedOptions.Top = groupBoxProgress.Top;

        comboBoxScanType.Items.AddRange(typeof(ScanType).GetDescriptions());
        comboBoxScanType.SelectedIndex = 0;
      }

      if (_scanState == ScanState.Initialized)
      {
        string country = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanTerrestrial" + _tunerId + "Country", System.Globalization.RegionInfo.CurrentRegion.EnglishName);
        string region = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanTerrestrial" + _tunerId + "Region", string.Empty);
        string transmitter = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanTerrestrial" + _tunerId + "Transmitter", TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
        _tuningDetailFilter = new TuningDetailFilter(_tunerId, TuningDetailGroup.Terrestrial, comboBoxCountry, country, comboBoxRegionProvider, region, comboBoxTransmitter, transmitter);

        Tuner tuner = ServiceAgents.Instance.TunerServiceAgent.GetTuner(_tunerId, TunerRelation.None);
        comboBoxBroadcastStandard.Items.Clear();
        comboBoxBroadcastStandard.Items.AddRange(typeof(BroadcastStandard).GetDescriptions(tuner.SupportedBroadcastStandards & (int)SUPPORTED_BROADCAST_STANDARDS, false));
        comboBoxBroadcastStandard.SelectedItem = ((BroadcastStandard)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanTerrestrial" + _tunerId + "BroadcastStandard", (int)BroadcastStandard.DvbT)).GetDescription();
        if (comboBoxBroadcastStandard.SelectedItem == null)
        {
          comboBoxBroadcastStandard.SelectedIndex = 0;
        }
      }

      numericTextBoxFrequency.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanTerrestrial" + _tunerId + "Frequency", 163000);
      numericTextBoxBandwidth.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanTerrestrial" + _tunerId + "Bandwidth", 8000);
      numericTextBoxPlpId.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanTerrestrial" + _tunerId + "PlpId", -1);

      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("scan terrestrial: deactivating, tuner ID = {0}", _tunerId);

      DebugSettings();
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanTerrestrial" + _tunerId + "Country", comboBoxCountry.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanTerrestrial" + _tunerId + "Region", comboBoxRegionProvider.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanTerrestrial" + _tunerId + "Transmitter", comboBoxRegionProvider.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanTerrestrial" + _tunerId + "BroadcastStandard", Convert.ToInt32(typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanTerrestrial" + _tunerId + "Frequency", numericTextBoxFrequency.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanTerrestrial" + _tunerId + "Bandwidth", numericTextBoxBandwidth.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanTerrestrial" + _tunerId + "PlpId", numericTextBoxPlpId.Value);

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("  country     = {0}", comboBoxCountry.SelectedItem);
      this.LogDebug("  region      = {0}", comboBoxRegionProvider.SelectedItem);
      this.LogDebug("  transmitter = {0}", comboBoxTransmitter.SelectedItem);
      this.LogDebug("  standard    = {0}", comboBoxBroadcastStandard.SelectedItem);
      this.LogDebug("  frequency   = {0} kHz", numericTextBoxFrequency.Text);
      this.LogDebug("  bandwidth   = {0} kHz", numericTextBoxBandwidth.Text);
      this.LogDebug("  PLP ID      = {0}", numericTextBoxPlpId.Text);
    }

    #endregion

    #region scan handling

    private TuningDetail GetManualTuning()
    {
      TuningDetail tuningDetail = new TuningDetail();
      tuningDetail.BroadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.Bandwidth = numericTextBoxBandwidth.Value;
      if (numericTextBoxPlpId.Enabled)
      {
        tuningDetail.StreamId = numericTextBoxPlpId.Value;
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
        this.LogInfo("scan terrestrial: start scanning, country = {0}, region = {1}", comboBoxCountry.SelectedItem, comboBoxRegionProvider.SelectedItem);
        tuningDetails = _tuningDetailFilter.TuningDetails;
      }

      _scanHelper = new ChannelScanHelper(_tunerId, listViewProgress, progressBarProgress, OnNitScanFoundTransmitters, OnScanCompleted, progressBarSignalStrength, progressBarSignalQuality);
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

    private void OnNitScanFoundTransmitters(IList<TuningDetail> transmitters)
    {
      this.Invoke((MethodInvoker)delegate
      {
        _tuningDetailFilter.Save(string.Format("NIT Scans.{0}.xml", DateTime.Now.ToString("yyyy-MM-dd")), transmitters);
      });
    }

    private void OnScanCompleted()
    {
      _scanState = ScanState.Done;
      buttonScan.Invoke((MethodInvoker)delegate
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
      comboBoxCountry.Enabled = !showScanProgress;
      comboBoxRegionProvider.Enabled = !showScanProgress;
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
      numericTextBoxPlpId.Enabled = broadcastStandard == BroadcastStandard.DvbT2;
    }

    #endregion
  }
}