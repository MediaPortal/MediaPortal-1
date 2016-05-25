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
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;
using DbTuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;
using FileTuningDetail = Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardDvbS : SectionSettings
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
          return ScanType.PredefinedProvider;
        }
        return (ScanType)typeof(ScanType).GetEnumFromDescription((string)comboBoxScanType.SelectedItem);
      }
    }

    #endregion

    public CardDvbS(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
      base.Text = name;
    }

    #region activate/deactivate

    public override void OnSectionActivated()
    {
      this.LogDebug("satellite: activating, tuner ID = {0}", _tunerId);

      // First activation.
      if (comboBoxLnbType.Items.Count == 0)
      {
        groupBoxAdvancedOptions.Top = groupBoxProgress.Top;

        comboBoxLnbType.DataSource = ServiceAgents.Instance.TunerServiceAgent.ListAllLnbTypes();
        comboBoxDiseqc.Items.AddRange(typeof(DiseqcPort).GetDescriptions());
        comboBoxScanType.Items.AddRange(typeof(ScanType).GetDescriptions());
        comboBoxScanType.SelectedIndex = 0;
        comboBoxPolarisation.Items.AddRange(typeof(Polarisation).GetDescriptions());
        comboBoxModulation.Items.AddRange(typeof(ModulationSchemePsk).GetDescriptions());
        comboBoxFecCodeRate.Items.AddRange(typeof(FecCodeRate).GetDescriptions());
        comboBoxPilotTonesState.Items.AddRange(typeof(PilotTonesState).GetDescriptions());
        comboBoxRollOffFactor.Items.AddRange(typeof(RollOffFactor).GetDescriptions());
      }

      Tuner tuner = ServiceAgents.Instance.TunerServiceAgent.GetTuner(_tunerId, TunerIncludeRelationEnum.None);

      _tuningDetailFilter = new TuningDetailFilter("dvbs", comboBoxSatellite);

      comboBoxBroadcastStandard.Items.Clear();
      _tunerSupportedBroadcastStandards = (BroadcastStandard)tuner.SupportedBroadcastStandards;
      comboBoxBroadcastStandard.Items.AddRange(typeof(BroadcastStandard).GetDescriptions(tuner.SupportedBroadcastStandards, false));

      comboBoxSatellite.SelectedItem = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "Satellite", string.Empty);
      if (comboBoxSatellite.SelectedItem == null)
      {
        comboBoxSatellite.SelectedIndex = 0;
      }
      int lnbTypeId = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "LnbType", 1);
      foreach (LnbType lnbType in comboBoxLnbType.Items)
      {
        if (lnbType.IdLnbType == lnbTypeId)
        {
          comboBoxLnbType.SelectedItem = lnbType;
          break;
        }
      }
      comboBoxDiseqc.SelectedItem = ((DiseqcPort)ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "Diseqc", (int)DiseqcPort.None)).GetDescription();
      if (comboBoxDiseqc.SelectedItem == null)
      {
        comboBoxDiseqc.SelectedIndex = 0;
      }
      comboBoxBroadcastStandard.SelectedItem = ((BroadcastStandard)ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "BroadcastStandard", (int)BroadcastStandard.DvbS)).GetDescription();
      if (comboBoxBroadcastStandard.SelectedItem == null)
      {
        comboBoxBroadcastStandard.SelectedIndex = 0;
      }
      numericTextBoxFrequency.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "Frequency", 11097000);
      comboBoxPolarisation.SelectedItem = ((Polarisation)ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "Polarisation", (int)Polarisation.Automatic)).GetDescription();
      if (comboBoxPolarisation.SelectedItem == null)
      {
        comboBoxPolarisation.SelectedIndex = 0;
      }
      comboBoxModulation.SelectedItem = ((ModulationSchemePsk)ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "Modulation", (int)ModulationSchemePsk.Automatic)).GetDescription();
      if (comboBoxModulation.SelectedItem == null)
      {
        comboBoxModulation.SelectedIndex = 0;
      }
      numericTextBoxSymbolRate.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "SymbolRate", 25000);
      comboBoxFecCodeRate.SelectedItem = ((FecCodeRate)ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "FecCodeRate", (int)FecCodeRate.Automatic)).GetDescription();
      if (comboBoxFecCodeRate.SelectedItem == null)
      {
        comboBoxFecCodeRate.SelectedIndex = 0;
      }
      comboBoxPilotTonesState.SelectedItem = ((PilotTonesState)ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "PilotTonesState", (int)PilotTonesState.Automatic)).GetDescription();
      if (comboBoxPilotTonesState.SelectedItem == null)
      {
        comboBoxPilotTonesState.SelectedIndex = 0;
      }
      comboBoxRollOffFactor.SelectedItem = ((RollOffFactor)ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "RollOffFactor", (int)RollOffFactor.Automatic)).GetDescription();
      if (comboBoxRollOffFactor.SelectedItem == null)
      {
        comboBoxRollOffFactor.SelectedIndex = 0;
      }
      numericTextBoxInputStreamId.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _tunerId + "InputStreamId", -1);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("satellite: deactivating, tuner ID = {0}", _tunerId);

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "Satellite", ((CustomFileName)comboBoxSatellite.SelectedItem).ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "LnbType", ((LnbType)comboBoxLnbType.SelectedItem).IdLnbType);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "Diseqc", Convert.ToInt32(typeof(DiseqcPort).GetEnumFromDescription((string)comboBoxDiseqc.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "BroadcastStandard", Convert.ToInt32(typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "Frequency", numericTextBoxFrequency.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "Polarisation", Convert.ToInt32(typeof(Polarisation).GetEnumFromDescription((string)comboBoxPolarisation.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "Modulation", Convert.ToInt32(typeof(ModulationSchemePsk).GetEnumFromDescription((string)comboBoxModulation.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "SymbolRate", numericTextBoxSymbolRate.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "FecCodeRate", Convert.ToInt32(typeof(FecCodeRate).GetEnumFromDescription((string)comboBoxFecCodeRate.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "PilotTonesState", Convert.ToInt32(typeof(PilotTonesState).GetEnumFromDescription((string)comboBoxPilotTonesState.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "RollOffFactor", Convert.ToInt32(typeof(RollOffFactor).GetEnumFromDescription((string)comboBoxRollOffFactor.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _tunerId + "InputStreamId", numericTextBoxInputStreamId.Value);

      base.OnSectionDeActivated();
    }

    #endregion

    #region scan handling

    private FileTuningDetail GetManualTuning()
    {
      FileTuningDetail tuningDetail = new FileTuningDetail();
      tuningDetail.BroadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.Polarisation = (Polarisation)typeof(Polarisation).GetEnumFromDescription((string)comboBoxPolarisation.SelectedItem);
      tuningDetail.ModulationScheme = ((ModulationSchemePsk)typeof(ModulationSchemePsk).GetEnumFromDescription((string)comboBoxModulation.SelectedItem)).ToString();
      tuningDetail.SymbolRate = numericTextBoxSymbolRate.Value;
      tuningDetail.FecCodeRate = (FecCodeRate)typeof(FecCodeRate).GetEnumFromDescription((string)comboBoxFecCodeRate.SelectedItem);
      if (tuningDetail.BroadcastStandard == BroadcastStandard.DvbS2)
      {
        tuningDetail.PilotTonesState = (PilotTonesState)typeof(PilotTonesState).GetEnumFromDescription((string)comboBoxPilotTonesState.SelectedItem);
        tuningDetail.RollOffFactor = (RollOffFactor)typeof(RollOffFactor).GetEnumFromDescription((string)comboBoxRollOffFactor.SelectedItem);
        tuningDetail.StreamId = numericTextBoxInputStreamId.Value;
      }
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
            CustomFileName tuningFile = (CustomFileName)comboBoxSatellite.SelectedItem;
            this.LogInfo("satellite: start scanning, satellite = {0}...", comboBoxSatellite.SelectedItem);
            tuningDetails = new List<FileTuningDetail>(100);
            foreach (FileTuningDetail td in _tuningDetailFilter.LoadList(tuningFile.FileName))
            {
              if (_tunerSupportedBroadcastStandards.HasFlag(td.BroadcastStandard))
              {
                tuningDetails.Add(td);
              }
            }
          }
          if (tuningDetails == null || tuningDetails.Count == 0)
          {
            return;
          }

          ILnbType lnbType = new LnbTypeBLL((LnbType)comboBoxLnbType.SelectedItem);
          DiseqcPort diseqcPort = (DiseqcPort)typeof(DiseqcPort).GetEnumFromDescription((string)comboBoxDiseqc.SelectedItem);
          foreach (FileTuningDetail tuningDetail in tuningDetails)
          {
            tuningDetail.LnbType = lnbType;
            tuningDetail.DiseqcPort = diseqcPort;
          }

          _scanHelper = new ChannelScanHelper(_tunerId, listViewProgress, progressBarProgress, OnNitScanFoundTransmitters, OnGetDbExistingTuningDetailCandidates, OnScanCompleted, progressBarSignalStrength, progressBarSignalQuality);
          bool result;
          if (scanType != ScanType.PredefinedProvider)
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
      ILnbType lnbType = new LnbTypeBLL((LnbType)comboBoxLnbType.SelectedItem);
      DiseqcPort diseqcPort = (DiseqcPort)typeof(DiseqcPort).GetEnumFromDescription((string)comboBoxDiseqc.SelectedItem);

      IList<FileTuningDetail> tunableTransmitters = new List<FileTuningDetail>(transmitters.Count);
      foreach (FileTuningDetail transmitter in transmitters)
      {
        if (_tunerSupportedBroadcastStandards.HasFlag(transmitter.BroadcastStandard))
        {
          transmitter.LnbType = lnbType;
          transmitter.DiseqcPort = diseqcPort;
          tunableTransmitters.Add(transmitter);
        }
      }
      return tunableTransmitters;
    }

    private IList<DbTuningDetail> OnGetDbExistingTuningDetailCandidates(ScannedChannel foundChannel, bool useChannelMovementDetection)
    {
      // Freesat channel movement detection is always active. Each channel has
      // a unique identifier.
      int freesatChannelId = 0;
      ChannelDvbS dvbsChannel = foundChannel.Channel as ChannelDvbS;
      if (dvbsChannel != null)
      {
        freesatChannelId = dvbsChannel.FreesatChannelId;
      }
      else
      {
        ChannelDvbS2 dvbs2Channel = foundChannel.Channel as ChannelDvbS2;
        if (dvbs2Channel != null)
        {
          freesatChannelId = dvbs2Channel.FreesatChannelId;
        }
      }
      IList<DbTuningDetail> tuningDetails;
      if (freesatChannelId > 0)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetFreesatTuningDetails(freesatChannelId);
        if (tuningDetails != null && tuningDetails.Count > 0)
        {
          return tuningDetails;
        }
      }

      // OpenTV channel movement detection is always active. Each channel has a
      // unique identifier.
      int? originalNetworkId = null;
      ChannelDvbBase dvbChannel = foundChannel.Channel as ChannelDvbBase;
      if (dvbChannel != null)
      {
        if (dvbChannel.OpenTvChannelId > 0)
        {
          tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetOpenTvTuningDetails(dvbChannel.OpenTvChannelId);
          if (tuningDetails != null && tuningDetails.Count > 0)
          {
            return tuningDetails;
          }
        }

        originalNetworkId = dvbChannel.OriginalNetworkId;
      }

      // If previous DVB service identifiers are available then assume the
      // service has moved recently and use the identifiers to locate the
      // tuning detail.
      BroadcastStandard broadcastStandardSearchMask = BroadcastStandard.MaskSatellite;
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
      // Unfortunately, unlike with DVB-C/C2 and DVB-T/T2 there are many
      // broadcasters who do not maintain unique ONID + SID combinations. In
      // particular, "feeds" (temporary transmissions) often use low-value ONID
      // and/or TSID and/or SID values. In some cases ONID + TSID + SID is a
      // suitable replacement identifier. However, the consequence of using the
      // TSID as part of the identifier is that channel movement tracking won't
      // work (the TSID is associated with the transmitter).
      ChannelMpeg2Base mpeg2Channel = foundChannel.Channel as ChannelMpeg2Base;
      if (mpeg2Channel == null)
      {
        return null;
      }

      int? frequency = null;
      if (
        (dvbChannel == null || dvbChannel.OriginalNetworkId < 3 || dvbChannel.OriginalNetworkId > ushort.MaxValue - 3) &&
        (mpeg2Channel.TransportStreamId < 3 || mpeg2Channel.TransportStreamId > ushort.MaxValue - 3) &&
        (mpeg2Channel.ProgramNumber < 3 || mpeg2Channel.ProgramNumber > ushort.MaxValue - 3)
      )
      {
        // Feeds, provider private transmissions etc. - even ONID + TSID + SID will not be unique.
        useChannelMovementDetection = false;
        IChannelPhysical channelPhysical = foundChannel.Channel as IChannelPhysical;
        if (channelPhysical != null)
        {
          frequency = channelPhysical.Frequency;
        }
      }

      if (!originalNetworkId.HasValue)
      {
        if (useChannelMovementDetection)
        {
          tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetMpeg2TuningDetails(broadcastStandardSearchMask, mpeg2Channel.ProgramNumber, null, frequency);
          if (tuningDetails == null || tuningDetails.Count == 1)
          {
            return tuningDetails;
          }
        }
        return ServiceAgents.Instance.ChannelServiceAgent.GetMpeg2TuningDetails(broadcastStandardSearchMask, mpeg2Channel.ProgramNumber, mpeg2Channel.TransportStreamId, frequency);
      }

      if (useChannelMovementDetection)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, originalNetworkId.Value, mpeg2Channel.ProgramNumber, null, frequency);
        if (tuningDetails == null || tuningDetails.Count == 1)
        {
          return tuningDetails;
        }
      }
      return ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, originalNetworkId.Value, mpeg2Channel.ProgramNumber, mpeg2Channel.TransportStreamId, frequency);
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
      if (showScanProgress)
      {
        comboBoxSatellite.Enabled = false;
        comboBoxLnbType.Enabled = false;
        comboBoxDiseqc.Enabled = false;
        checkBoxUseAdvancedOptions.Enabled = false;
        groupBoxAdvancedOptions.Visible = false;
        listViewProgress.Items.Clear();
        groupBoxProgress.Visible = true;
        groupBoxProgress.BringToFront();
        UpdateZOrder();
      }
      else
      {
        comboBoxSatellite.Enabled = true;
        comboBoxLnbType.Enabled = true;
        comboBoxDiseqc.Enabled = true;
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

    private void checkBoxUseAdvancedScanningOptions_CheckedChanged(object sender, EventArgs e)
    {
      groupBoxAdvancedOptions.Visible = !groupBoxAdvancedOptions.Visible;
    }

    private void comboBoxScanType_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool isPredefinedScan = ActiveScanType == ScanType.PredefinedProvider;
      comboBoxBroadcastStandard.Enabled = !isPredefinedScan;
      numericTextBoxFrequency.Enabled = !isPredefinedScan;
      comboBoxPolarisation.Enabled = !isPredefinedScan;
      comboBoxModulation.Enabled = !isPredefinedScan;
      numericTextBoxSymbolRate.Enabled = !isPredefinedScan;
      comboBoxFecCodeRate.Enabled = !isPredefinedScan;

      comboBoxBroadcastStandard_SelectedIndexChanged(null, null);
    }

    private void comboBoxBroadcastStandard_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (comboBoxBroadcastStandard.SelectedItem == null)
      {
        return;
      }
      BroadcastStandard broadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      bool enableFields = ActiveScanType != ScanType.PredefinedProvider && broadcastStandard == BroadcastStandard.DvbS2;
      comboBoxPilotTonesState.Enabled = enableFields;
      comboBoxRollOffFactor.Enabled = enableFields;
      numericTextBoxInputStreamId.Enabled = enableFields;
    }

    #endregion
  }
}