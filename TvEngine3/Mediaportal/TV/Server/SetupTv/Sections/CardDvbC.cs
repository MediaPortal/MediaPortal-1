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
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;
using DbTuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;
using FileTuningDetail = Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardDvbC : SectionSettings
  {
    #region variables

    private readonly int _tunerId;
    private TuningDetailFilter _tuningDetailFilter;
    private ChannelScanHelper _scanHelper = null;
    private ScanState _scanState = ScanState.Initialized;

    #endregion

    public CardDvbC(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
    }

    #region activate/deactivate

    public override void OnSectionActivated()
    {
      this.LogDebug("scan DVB-C: activating, tuner ID = {0}", _tunerId);

      // First activation.
      if (comboBoxScanType.Items.Count == 0)
      {
        groupBoxAdvancedOptions.Top = groupBoxProgress.Top;

        comboBoxScanType.Items.AddRange(typeof(ScanType).GetDescriptions());
        comboBoxScanType.SelectedIndex = 0;
        comboBoxModulation.Items.AddRange(typeof(ModulationSchemeQam).GetDescriptions());
      }

      if (_scanState == ScanState.Initialized)
      {
        string country = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbc" + _tunerId + "Country", System.Globalization.RegionInfo.CurrentRegion.EnglishName);
        string region = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbc" + _tunerId + "Region", string.Empty);
        string transmitter = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbc" + _tunerId + "Transmitter", TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
        _tuningDetailFilter = new TuningDetailFilter(BroadcastStandard.DvbC, comboBoxCountry, country, comboBoxRegionProvider, region, comboBoxTransmitter, transmitter);
      }

      numericTextBoxFrequency.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbc" + _tunerId + "Frequency", 163000);
      comboBoxModulation.SelectedItem = ((ModulationSchemeQam)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbc" + _tunerId + "Modulation", (int)ModulationSchemeQam.Qam256)).GetDescription();
      if (comboBoxModulation.SelectedItem == null)
      {
        comboBoxModulation.SelectedIndex = 0;
      }
      numericTextBoxSymbolRate.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanDvbc" + _tunerId + "SymbolRate", 6875);

      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("scan DVB-C: deactivating, tuner ID = {0}", _tunerId);

      DebugSettings();
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbc" + _tunerId + "Country", comboBoxCountry.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbc" + _tunerId + "Region", comboBoxRegionProvider.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbc" + _tunerId + "Transmitter", comboBoxTransmitter.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbc" + _tunerId + "Frequency", numericTextBoxFrequency.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbc" + _tunerId + "Modulation", Convert.ToInt32(typeof(ModulationSchemeQam).GetEnumFromDescription((string)comboBoxModulation.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanDvbc" + _tunerId + "SymbolRate", numericTextBoxSymbolRate.Value);

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("  country     = {0}", comboBoxCountry.SelectedItem);
      this.LogDebug("  region      = {0}", comboBoxRegionProvider.SelectedItem);
      this.LogDebug("  transmitter = {0}", comboBoxTransmitter.SelectedItem);
      this.LogDebug("  frequency   = {0} kHz", numericTextBoxFrequency.Text);
      this.LogDebug("  modulation  = {0}", comboBoxModulation.SelectedItem);
      this.LogDebug("  symbol rate = {0} ks/s", numericTextBoxSymbolRate.Text);
    }

    #endregion

    #region scan handling

    private FileTuningDetail GetManualTuning()
    {
      FileTuningDetail tuningDetail = new FileTuningDetail();
      tuningDetail.BroadcastStandard = BroadcastStandard.DvbC;
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.ModulationScheme = ((ModulationSchemeQam)typeof(ModulationSchemeQam).GetEnumFromDescription((string)comboBoxModulation.SelectedItem)).ToString();
      tuningDetail.SymbolRate = numericTextBoxSymbolRate.Value;
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
        this.LogInfo("scan DVB-C: start scanning, country = {0}, region = {1}", comboBoxCountry.SelectedItem, comboBoxRegionProvider.SelectedItem);
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
        if (transmitter.BroadcastStandard == BroadcastStandard.DvbC)
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
      BroadcastStandard broadcastStandardSearchMask = BroadcastStandard.MaskCable & (BroadcastStandard.MaskDvb | BroadcastStandard.MaskIsdb);
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
      // Unlike with satellite, most DVB-C/C2 broadcasters maintain unique ONID
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
      bool enableAdvancedOptions = string.Equals(comboBoxTransmitter.SelectedItem.ToString(), TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
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

    #endregion
  }
}