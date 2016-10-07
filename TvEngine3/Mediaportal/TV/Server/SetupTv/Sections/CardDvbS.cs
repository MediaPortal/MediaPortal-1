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
    private ChannelScanHelper _scanHelper = null;
    private ScanState _scanState = ScanState.Initialized;
    private IDictionary<int, int> _satelliteIdsByLongitude;

    #endregion

    public CardDvbS(string name, int tunerId)
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
        comboBoxModulation.Items.AddRange(typeof(ModulationSchemePsk).GetDescriptions());
        comboBoxFecCodeRate.Items.AddRange(typeof(FecCodeRate).GetDescriptions());
        comboBoxPilotTonesState.Items.AddRange(typeof(PilotTonesState).GetDescriptions());
        comboBoxRollOffFactor.Items.AddRange(typeof(RollOffFactor).GetDescriptions());
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

        Tuner tuner = ServiceAgents.Instance.TunerServiceAgent.GetTuner(_tunerId, TunerRelation.None);
        _tunerSupportedBroadcastStandards = (BroadcastStandard)tuner.SupportedBroadcastStandards;

        IList<Satellite> satellites = ServiceAgents.Instance.TunerServiceAgent.ListAllSatellites();
        int longitude = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "Satellite", Satellite.DefaultSatelliteLongitude.GetValueOrDefault(100000));
        comboBoxSatellite.BeginUpdate();
        try
        {
          comboBoxSatellite.Items.Clear();
          foreach (TunerSatellite tunerSatellite in tunerSatellites)
          {
            comboBoxSatellite.Items.Add(tunerSatellite.Satellite);
            if (longitude == tunerSatellite.Satellite.Longitude)
            {
              comboBoxSatellite.SelectedItem = tunerSatellite.Satellite;
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

        comboBoxBroadcastStandard.Items.Clear();
        comboBoxBroadcastStandard.Items.AddRange(typeof(BroadcastStandard).GetDescriptions(tuner.SupportedBroadcastStandards, false));
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
      comboBoxPilotTonesState.SelectedItem = ((PilotTonesState)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "PilotTonesState", (int)PilotTonesState.Automatic)).GetDescription();
      if (comboBoxPilotTonesState.SelectedItem == null)
      {
        comboBoxPilotTonesState.SelectedIndex = 0;
      }
      comboBoxRollOffFactor.SelectedItem = ((RollOffFactor)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanSatellite" + _tunerId + "RollOffFactor", (int)RollOffFactor.Automatic)).GetDescription();
      if (comboBoxRollOffFactor.SelectedItem == null)
      {
        comboBoxRollOffFactor.SelectedIndex = 0;
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
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "PilotTonesState", Convert.ToInt32(typeof(PilotTonesState).GetEnumFromDescription((string)comboBoxPilotTonesState.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "RollOffFactor", Convert.ToInt32(typeof(RollOffFactor).GetEnumFromDescription((string)comboBoxRollOffFactor.SelectedItem)));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanSatellite" + _tunerId + "InputStreamId", numericTextBoxInputStreamId.Value);

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("  tuner standard(s) = [{0}]", _tunerSupportedBroadcastStandards);
      this.LogDebug("  satellite         = {0}", comboBoxSatellite.SelectedItem);
      this.LogDebug("  transmitter       = {0}", comboBoxTransmitter.SelectedItem);
      this.LogDebug("  standard          = {0}", comboBoxBroadcastStandard.SelectedItem);
      this.LogDebug("  frequency         = {0} kHz", numericTextBoxFrequency.Text);
      this.LogDebug("  polarisation      = {0}", comboBoxPolarisation.SelectedItem);
      this.LogDebug("  modulation        = {0}", comboBoxModulation.SelectedItem);
      this.LogDebug("  symbol rate       = {0} ks/s", numericTextBoxSymbolRate.Text);
      this.LogDebug("  FEC code rate     = {0}", comboBoxFecCodeRate.SelectedItem);
      this.LogDebug("  pilot tones state = {0}", comboBoxPilotTonesState.SelectedItem);
      this.LogDebug("  roll-off factor   = {0}", comboBoxRollOffFactor.SelectedItem);
      this.LogDebug("  input stream ID   = {0}", numericTextBoxInputStreamId.Text);
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

      IList<FileTuningDetail> tuningDetails = null;
      if (checkBoxUseAdvancedOptions.Enabled && checkBoxUseAdvancedOptions.Checked && checkBoxUseManualTuning.Checked)
      {
        FileTuningDetail tuningDetail = GetManualTuning();
        tuningDetail.Longitude = satellite.Longitude;
        tuningDetails = new List<FileTuningDetail>(1) { tuningDetail };
      }
      else
      {
        tuningDetails = TuningDetailFilter.GetTuningDetails(comboBoxTransmitter);
        if (tuningDetails == null || tuningDetails.Count == 0)
        {
          return;
        }
        foreach (FileTuningDetail tuningDetail in tuningDetails)
        {
          tuningDetail.Longitude = satellite.Longitude;
        }
      }

      // We need a dictionary of longitude => ID for looking up the tuning
      // detail candidates for each service during the scan. Note that we
      // include all satellites in the dictionary, not just the satellites that
      // this tuner can receive.
      IList<Satellite> satellites = ServiceAgents.Instance.TunerServiceAgent.ListAllSatellites();
      _satelliteIdsByLongitude = new Dictionary<int, int>(satellites.Count);
      foreach (Satellite s in satellites)
      {
        _satelliteIdsByLongitude[s.Longitude] = s.IdSatellite;
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
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetFreesatTuningDetails(freesatChannelId, includeRelations);
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
          tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetOpenTvTuningDetails(dvbChannel.OpenTvChannelId, includeRelations);
          if (tuningDetails != null && tuningDetails.Count > 0)
          {
            return tuningDetails;
          }
        }

        originalNetworkId = dvbChannel.OriginalNetworkId;
      }

      // The remaining channel movement detection methods are confined to the
      // satellite that is broadcasting the channel.
      int? satelliteId = null;
      IChannelSatellite satelliteChannel = foundChannel.Channel as IChannelSatellite;
      if (satelliteChannel != null)
      {
        int tempId;
        if (_satelliteIdsByLongitude.TryGetValue(satelliteChannel.Longitude, out tempId))
        {
          satelliteId = tempId;
        }
      }

      // If previous DVB service identifiers are available then assume the
      // service has moved recently and use the identifiers to locate the
      // tuning detail.
      BroadcastStandard broadcastStandardSearchMask = BroadcastStandard.MaskSatellite;
      if (foundChannel.PreviousOriginalNetworkId > 0)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, foundChannel.PreviousOriginalNetworkId, foundChannel.PreviousServiceId, includeRelations, foundChannel.PreviousTransportStreamId, null, satelliteId);
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
          tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetMpeg2TuningDetails(broadcastStandardSearchMask, mpeg2Channel.ProgramNumber, includeRelations, null, frequency, satelliteId);
          if (tuningDetails == null || tuningDetails.Count == 1)
          {
            return tuningDetails;
          }
        }
        return ServiceAgents.Instance.ChannelServiceAgent.GetMpeg2TuningDetails(broadcastStandardSearchMask, mpeg2Channel.ProgramNumber, includeRelations, mpeg2Channel.TransportStreamId, frequency, satelliteId);
      }

      if (useChannelMovementDetection)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, originalNetworkId.Value, mpeg2Channel.ProgramNumber, includeRelations, null, frequency);
        if (tuningDetails == null || tuningDetails.Count == 1)
        {
          return tuningDetails;
        }
      }
      return ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, originalNetworkId.Value, mpeg2Channel.ProgramNumber, includeRelations, mpeg2Channel.TransportStreamId, frequency);
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
      TuningDetailFilter.Load(_tunerSupportedBroadcastStandards, satellite.ToString() + ".xml", comboBoxTransmitter);
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
      bool enableFields = broadcastStandard == BroadcastStandard.DvbS2;
      comboBoxPilotTonesState.Enabled = enableFields;
      comboBoxRollOffFactor.Enabled = enableFields;
      numericTextBoxInputStreamId.Enabled = enableFields;
    }

    #endregion
  }
}