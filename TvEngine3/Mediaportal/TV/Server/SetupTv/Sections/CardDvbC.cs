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

    #region properties

    private ScanType ActiveScanType
    {
      get
      {
        if (!checkBoxUseAdvancedOptions.Checked)
        {
          return ScanType.PredefinedProvider;
        }
        return (ScanType)typeof(ScanType).GetEnumFromDescription((string)comboBoxScanType.SelectedItem);
      }
    }

    #endregion

    public CardDvbC(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
      base.Text = name;
    }

    #region activate/deactivate

    public override void OnSectionActivated()
    {
      this.LogDebug("DVB-C: activating, tuner ID = {0}", _tunerId);

      // First activation.
      if (comboBoxScanType.Items.Count == 0)
      {
        groupBoxAdvancedOptions.Top = groupBoxProgress.Top;

        comboBoxScanType.Items.AddRange(typeof(ScanType).GetDescriptions());
        comboBoxScanType.SelectedIndex = 0;
        comboBoxModulation.Items.AddRange(typeof(ModulationSchemeQam).GetDescriptions());
      }

      _tuningDetailFilter = new TuningDetailFilter("dvbc", comboBoxCountry, comboBoxRegionProvider);

      comboBoxCountry.SelectedItem = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbc" + _tunerId + "Country", System.Globalization.RegionInfo.CurrentRegion.EnglishName);
      if (comboBoxCountry.SelectedItem == null)
      {
        comboBoxCountry.SelectedIndex = 0;
      }
      comboBoxRegionProvider.SelectedItem = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbc" + _tunerId + "Region", string.Empty);
      if (comboBoxRegionProvider.SelectedItem == null)
      {
        comboBoxRegionProvider.SelectedIndex = 0;
      }
      numericTextBoxFrequency.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbc" + _tunerId + "Frequency", 163000);
      comboBoxModulation.SelectedItem = ((ModulationSchemeQam)ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbc" + _tunerId + "Modulation", (int)ModulationSchemeQam.Qam256)).GetDescription();
      if (comboBoxModulation.SelectedItem == null)
      {
        comboBoxModulation.SelectedIndex = 0;
      }
      numericTextBoxSymbolRate.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbc" + _tunerId + "SymbolRate", 6875);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("DVB-C: deactivating, tuner ID = {0}", _tunerId);

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbc" + _tunerId + "Country", (string)comboBoxCountry.SelectedItem);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbc" + _tunerId + "Region", ((CustomFileName)comboBoxRegionProvider.SelectedItem).ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbc" + _tunerId + "Frequency", numericTextBoxFrequency.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbc" + _tunerId + "Modulation", Convert.ToInt32(typeof(ModulationSchemeQam).GetEnumFromDescription((string)comboBoxModulation.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbc" + _tunerId + "SymbolRate", numericTextBoxSymbolRate.Value);

      base.OnSectionDeActivated();
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
      switch (_scanState)
      {
        case ScanState.Done:
          buttonScan.Text = "Scan for channels";
          _scanState = ScanState.Initialized;
          ShowOrHideScanProgress(false);
          return;
        case ScanState.Scanning:
          buttonScan.Text = "Cancelling...";
          _scanState = ScanState.Cancel;
          if (_scanHelper != null)
          {
            _scanHelper.StopScan();
          }
          break;
        case ScanState.Initialized:
          List<FileTuningDetail> tuningDetails = null;
          ScanType scanType = ActiveScanType;
          if (scanType != ScanType.PredefinedProvider)
          {
            tuningDetails = new List<FileTuningDetail> { GetManualTuning() };
          }
          else
          {
            CustomFileName tuningFile = (CustomFileName)comboBoxRegionProvider.SelectedItem;
            this.LogInfo("DVB-C: start scanning, country = {0}, region = {1}...", comboBoxCountry.SelectedItem, tuningFile);
            tuningDetails = _tuningDetailFilter.LoadList(tuningFile.FileName);
          }
          if (tuningDetails == null || tuningDetails.Count == 0)
          {
            return;
          }

          _scanHelper = new ChannelScanHelper(_tunerId, listViewProgress, progressBarProgress, OnNitScanFoundTransmitters, OnGetDbExistingTuningDetailCandidates, OnScanCompleted, progressBarSignalStrength, progressBarSignalQuality);
          bool result;
          if (scanType == ScanType.FullNetworkInformationTable)
          {
            result = _scanHelper.StartNitScan(tuningDetails[0]);
          }
          else
          {
            result = _scanHelper.StartScan(tuningDetails, scanType);
          }
          if (result)
          {
            _scanState = ScanState.Scanning;
            buttonScan.Text = "Cancel...";
            ShowOrHideScanProgress(true);
          }
          break;
      }
    }

    private IList<FileTuningDetail> OnNitScanFoundTransmitters(IList<FileTuningDetail> transmitters)
    {
      this.Invoke((MethodInvoker)delegate
      {
        _tuningDetailFilter.SaveList(string.Format("NIT Scans.{0}.xml", DateTime.Now.ToString("yyyy-MM-dd")), transmitters);
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

    private IList<DbTuningDetail> OnGetDbExistingTuningDetailCandidates(ScannedChannel foundChannel, bool useChannelMovementDetection)
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
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetOpenTvTuningDetails(dvbChannel.OpenTvChannelId);
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
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, foundChannel.PreviousOriginalNetworkId, foundChannel.PreviousServiceId, foundChannel.PreviousTransportStreamId);
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
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, dvbChannel.OriginalNetworkId, dvbChannel.ServiceId);
        if (tuningDetails == null || tuningDetails.Count == 1)
        {
          return tuningDetails;
        }
      }
      return ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, dvbChannel.OriginalNetworkId, dvbChannel.ServiceId, dvbChannel.TransportStreamId);
    }

    private void OnScanCompleted()
    {
      _scanState = ScanState.Done;
      this.Invoke((MethodInvoker)delegate
      {
        buttonScan.Text = "New scan";
      });
      _scanHelper = null;
    }

    #endregion

    #region GUI handling

    private void ShowOrHideScanProgress(bool showScanProgress)
    {
      EnableOrDisablePredefinedScanFields();
      if (showScanProgress)
      {
        checkBoxUseAdvancedOptions.Enabled = false;
        groupBoxAdvancedOptions.Visible = false;
        listViewProgress.Items.Clear();
        groupBoxProgress.Visible = true;
        groupBoxProgress.BringToFront();
        UpdateZOrder();
      }
      else
      {
        checkBoxUseAdvancedOptions.Enabled = true;
        groupBoxAdvancedOptions.Visible = checkBoxUseAdvancedOptions.Checked;
        groupBoxProgress.Visible = false;
        if (groupBoxAdvancedOptions.Visible)
        {
          groupBoxAdvancedOptions.BringToFront();
          UpdateZOrder();
        }
      }
    }

    private void EnableOrDisablePredefinedScanFields()
    {
      bool enableFields = _scanState == ScanState.Initialized && ActiveScanType == ScanType.PredefinedProvider;
      comboBoxCountry.Enabled = enableFields;
      comboBoxRegionProvider.Enabled = enableFields;
    }

    private void checkBoxUseAdvancedScanningOptions_CheckedChanged(object sender, EventArgs e)
    {
      groupBoxAdvancedOptions.Visible = !groupBoxAdvancedOptions.Visible;
      EnableOrDisablePredefinedScanFields();
    }

    private void comboBoxScanType_SelectedIndexChanged(object sender, EventArgs e)
    {
      EnableOrDisablePredefinedScanFields();

      bool isPredefinedScan = ActiveScanType == ScanType.PredefinedProvider;
      numericTextBoxFrequency.Enabled = !isPredefinedScan;
      comboBoxModulation.Enabled = !isPredefinedScan;
      numericTextBoxSymbolRate.Enabled = !isPredefinedScan;
    }

    #endregion
  }
}