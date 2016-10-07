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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;
using DbTuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;
using FileTuningDetail = Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardDvbT : SectionSettings
  {
    #region variables

    private readonly int _tunerId;
    private BroadcastStandard _tunerSupportedBroadcastStandards;
    private TuningDetailFilter _tuningDetailFilter;
    private ChannelScanHelper _scanHelper = null;
    private ScanState _scanState = ScanState.Initialized;

    #endregion

    #region properties

    private ScanType ActiveScanType
    {
      get
      {
        if (!checkBoxUseAdvancedOptions.Checked)
        {
          return ScanType.Standard;
        }
        return (ScanType)typeof(ScanType).GetEnumFromDescription((string)comboBoxScanType.SelectedItem);
      }
    }

    #endregion

    public CardDvbT(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
    }

    #region activate/deactivate

    public override void OnSectionActivated()
    {
      this.LogDebug("scan DVB-T/T2: activating, tuner ID = {0}", _tunerId);

      // First activation.
      if (comboBoxScanType.Items.Count == 0)
      {
        groupBoxAdvancedOptions.Top = groupBoxProgress.Top;
        comboBoxScanType.Items.AddRange(typeof(ScanType).GetDescriptions());
        comboBoxScanType.SelectedIndex = 0;
      }

      if (_scanState == ScanState.Initialized)
      {
        Tuner tuner = ServiceAgents.Instance.TunerServiceAgent.GetTuner(_tunerId, TunerRelation.None);
        _tunerSupportedBroadcastStandards = (BroadcastStandard)tuner.SupportedBroadcastStandards;

        string country = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbt" + _tunerId + "Country", System.Globalization.RegionInfo.CurrentRegion.EnglishName);
        string region = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbt" + _tunerId + "Region", string.Empty);
        string transmitter = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbt" + _tunerId + "Transmitter", TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
        _tuningDetailFilter = new TuningDetailFilter(_tunerSupportedBroadcastStandards, comboBoxCountry, country, comboBoxRegionProvider, region, comboBoxTransmitter, transmitter);

        comboBoxBroadcastStandard.Items.Clear();
        comboBoxBroadcastStandard.Items.AddRange(typeof(BroadcastStandard).GetDescriptions(tuner.SupportedBroadcastStandards, false));
        comboBoxBroadcastStandard.SelectedItem = ((BroadcastStandard)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbt" + _tunerId + "BroadcastStandard", (int)BroadcastStandard.DvbT)).GetDescription();
        if (comboBoxBroadcastStandard.SelectedItem == null)
        {
          comboBoxBroadcastStandard.SelectedIndex = 0;
        }
      }

      numericTextBoxFrequency.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbt" + _tunerId + "Frequency", 163000);
      numericTextBoxBandwidth.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbt" + _tunerId + "Bandwidth", 8000);
      numericTextBoxPlpId.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbt" + _tunerId + "PlpId", -1);

      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("scan DVB-T/T2: deactivating, tuner ID = {0}", _tunerId);

      DebugSettings();
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbt" + _tunerId + "Country", comboBoxCountry.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbt" + _tunerId + "Region", comboBoxRegionProvider.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbt" + _tunerId + "Transmitter", comboBoxRegionProvider.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbt" + _tunerId + "BroadcastStandard", Convert.ToInt32(typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbt" + _tunerId + "Frequency", numericTextBoxFrequency.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbt" + _tunerId + "Bandwidth", numericTextBoxBandwidth.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbt" + _tunerId + "PlpId", numericTextBoxPlpId.Value);

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("  tuner standard(s) = [{0}]", _tunerSupportedBroadcastStandards);
      this.LogDebug("  country           = {0}", comboBoxCountry.SelectedItem);
      this.LogDebug("  region            = {0}", comboBoxRegionProvider.SelectedItem);
      this.LogDebug("  transmitter       = {0}", comboBoxTransmitter.SelectedItem);
      this.LogDebug("  standard          = {0}", comboBoxBroadcastStandard.SelectedItem);
      this.LogDebug("  frequency         = {0} kHz", numericTextBoxFrequency.Text);
      this.LogDebug("  bandwidth         = {0} kHz", numericTextBoxBandwidth.Text);
      this.LogDebug("  PLP ID            = {0}", numericTextBoxPlpId.Text);
    }

    #endregion

    #region scan handling

    private FileTuningDetail GetManualTuning()
    {
      FileTuningDetail tuningDetail = new FileTuningDetail();
      tuningDetail.BroadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.Bandwidth = numericTextBoxBandwidth.Value;
      if (tuningDetail.BroadcastStandard == BroadcastStandard.DvbT2)
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

      IList<FileTuningDetail> tuningDetails = null;
      if (checkBoxUseAdvancedOptions.Enabled && checkBoxUseAdvancedOptions.Checked && checkBoxUseManualTuning.Checked)
      {
        tuningDetails = new List<FileTuningDetail> { GetManualTuning() };
      }
      else
      {
        this.LogInfo("scan DVB-T/T2: start scanning, country = {0}, region = {1}", comboBoxCountry.SelectedItem, comboBoxRegionProvider.SelectedItem);
        tuningDetails = _tuningDetailFilter.TuningDetails;
      }

      _scanHelper = new ChannelScanHelper(_tunerId, listViewProgress, progressBarProgress, OnNitScanFoundTransmitters, OnGetDbExistingTuningDetailCandidates, OnScanCompleted, progressBarSignalStrength, progressBarSignalQuality);
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

    private IList<FileTuningDetail> OnNitScanFoundTransmitters(IList<FileTuningDetail> transmitters)
    {
      this.Invoke((MethodInvoker)delegate
      {
        _tuningDetailFilter.Save(string.Format("NIT Scans.{0}.xml", DateTime.Now.ToString("yyyy-MM-dd")), transmitters);
      });

      IList<FileTuningDetail> tunableTransmitters = new List<FileTuningDetail>(transmitters.Count);
      foreach (FileTuningDetail transmitter in transmitters)
      {
        if (_tunerSupportedBroadcastStandards.HasFlag(transmitter.BroadcastStandard))
        {
          tunableTransmitters.Add(transmitter);
        }
      }
      return tunableTransmitters;
    }

    private IList<DbTuningDetail> OnGetDbExistingTuningDetailCandidates(ScannedChannel foundChannel, bool useChannelMovementDetection, TuningDetailRelation includeRelations)
    {
      ChannelDvbBase dvbChannel = foundChannel.Channel as ChannelDvbBase;
      if (dvbChannel == null)
      {
        return null;
      }

      // OpenTV channel movement detection is always active. Each channel has a
      // unique identifier.
      IList<DbTuningDetail> tuningDetails;
      if (dvbChannel.OpenTvChannelId > 0)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetOpenTvTuningDetails(dvbChannel.OpenTvChannelId, includeRelations);
        if (tuningDetails != null && tuningDetails.Count > 0)
        {
          return tuningDetails;
        }
      }

      // If previous DVB service identifiers are available then assume the
      // service has moved recently and use the identifiers to locate the
      // tuning detail.
      BroadcastStandard broadcastStandardSearchMask = BroadcastStandard.MaskTerrestrial & (BroadcastStandard.MaskDvb | BroadcastStandard.MaskIsdb);
      if (foundChannel.PreviousOriginalNetworkId > 0)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, foundChannel.PreviousOriginalNetworkId, foundChannel.PreviousServiceId, includeRelations, foundChannel.PreviousTransportStreamId);
        if (tuningDetails != null && tuningDetails.Count > 0)
        {
          return tuningDetails;
        }
      }

      // According to the DVB specifications ONID + SID should be a sufficient
      // service identifier. The specification also recommends that the SID
      // should not change if a service moves. This theoretically allows us to
      // track channel movements.
      // Unlike with satellite, most DVB-T/T2 broadcasters maintain unique ONID
      // + SID combinations. We provide an ONID + TSID + SID fall-back option
      // for the exceptions.
      if (useChannelMovementDetection)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, dvbChannel.OriginalNetworkId, dvbChannel.ServiceId, includeRelations);
        if (tuningDetails == null || tuningDetails.Count == 1)
        {
          return tuningDetails;
        }
      }
      return ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, dvbChannel.OriginalNetworkId, dvbChannel.ServiceId, includeRelations, dvbChannel.TransportStreamId);
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
      bool enableFields = broadcastStandard == BroadcastStandard.DvbT2;
      numericTextBoxPlpId.Enabled = enableFields;
    }

    #endregion
  }
}