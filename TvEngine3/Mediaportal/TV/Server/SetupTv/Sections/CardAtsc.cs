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
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using DbTuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;
using FileTuningDetail = Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardAtsc : SectionSettings
  {
    private const string SCAN_MODE_ATSC = "ATSC Digital Terrestrial (Over-The-Air)";
    private const string SCAN_MODE_SCTE_CABLECARD = "SCTE Digital Cable, CableCARD";
    private const string SCAN_MODE_SCTE_STANDARD = "SCTE Digital Cable, Standard Band Plan";
    private const string SCAN_MODE_SCTE_HRC = "SCTE Digital Cable, HRC Band Plan";

    private readonly int _tunerId;
    private ChannelScanHelper _scanHelper = null;

    public CardAtsc(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("scan ATSC/SCTE: activating, tuner ID = {0}", _tunerId);

      Tuner tuner = ServiceAgents.Instance.TunerServiceAgent.GetTuner(_tunerId, TunerRelation.None);
      BroadcastStandard tunerSupportedBroadcastStandards = (BroadcastStandard)tuner.SupportedBroadcastStandards;
      this.LogDebug("  standard(s) = [{0}]", tunerSupportedBroadcastStandards);

      if (_scanHelper != null)
      {
        DebugSettings();
        base.OnSectionActivated();
        return;
      }

      comboBoxScanMode.BeginUpdate();
      try
      {
        comboBoxScanMode.Items.Clear();
        if (tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Atsc))
        {
          comboBoxScanMode.Items.Add(SCAN_MODE_ATSC);
        }
        if (tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Scte))
        {
          if (tuner.UseConditionalAccess)
          {
            comboBoxScanMode.Items.Add(SCAN_MODE_SCTE_CABLECARD);
          }
          comboBoxScanMode.Items.Add(SCAN_MODE_SCTE_STANDARD);
          comboBoxScanMode.Items.Add(SCAN_MODE_SCTE_HRC);
        }
      }
      finally
      {
        comboBoxScanMode.EndUpdate();
      }

      comboBoxScanMode.SelectedItem = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAtscScte" + _tunerId + "ScanMode", SCAN_MODE_ATSC);
      if (comboBoxScanMode.SelectedItem == null)
      {
        comboBoxScanMode.SelectedIndex = 0;
      }

      string transmitter = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAtscScte" + _tunerId + "Transmitter", TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
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

      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("scan ATSC/SCTE: deactivating, tuner ID = {0}", _tunerId);

      DebugSettings();
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAtscScte" + _tunerId + "ScanMode", (string)comboBoxScanMode.SelectedItem);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAtscScte" + _tunerId + "Transmitter", comboBoxTransmitter.SelectedItem.ToString());

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("  scan mode   = {0}", comboBoxScanMode.SelectedItem);
      this.LogDebug("  transmitter = {0}", comboBoxTransmitter.SelectedItem);
    }

    private void buttonScan_Click(object sender, EventArgs e)
    {
      if (_scanHelper != null)
      {
        buttonScan.Enabled = false;
        buttonScan.Text = "Stopping...";
        _scanHelper.StopScan();
        return;
      }

      IList<FileTuningDetail> tuningDetails = null;
      string scanMode = (string)comboBoxScanMode.SelectedItem;
      if (string.Equals(scanMode, SCAN_MODE_SCTE_CABLECARD))
      {
        tuningDetails = new List<FileTuningDetail>(1)
        {
          new FileTuningDetail
          {
            BroadcastStandard = BroadcastStandard.Scte,
            Frequency = -1    // special value
          }
        };
      }
      else if (string.Equals(scanMode, SCAN_MODE_ATSC) || string.Equals(scanMode, SCAN_MODE_SCTE_STANDARD) || string.Equals(scanMode, SCAN_MODE_SCTE_HRC))
      {
        tuningDetails = TuningDetailFilter.GetTuningDetails(comboBoxTransmitter);
      }
      if (tuningDetails == null || tuningDetails.Count == 0)
      {
        return;
      }

      listViewProgress.Items.Clear();
      ListViewItem item = listViewProgress.Items.Add(string.Format("start scanning {0}...", scanMode));
      item.EnsureVisible();
      this.LogInfo("scan ATSC/SCTE: start scanning, mode = {0}", scanMode);

      _scanHelper = new ChannelScanHelper(_tunerId, listViewProgress, progressBarProgress, null, OnGetDbExistingTuningDetailCandidates, OnScanCompleted, progressBarSignalStrength, progressBarSignalQuality);
      if (_scanHelper.StartScan(tuningDetails))
      {
        comboBoxScanMode.Enabled = false;
        comboBoxTransmitter.Enabled = false;
        buttonScan.Text = "&Stop";
      }
    }

    private IList<DbTuningDetail> OnGetDbExistingTuningDetailCandidates(ScannedChannel foundChannel, bool useChannelMovementDetection, TuningDetailRelation includeRelations)
    {
      // ATSC over-the-air channels don't generally move, but cable channels
      // can occasionally be moved. ATSC and SCTE SI have the MPEG 2 TSID and
      // program number (SID), as well as their own source ID. None of these
      // are suitable for tracking:
      // - TSID + program number is not unique across transmitters.
      //    Broadcasters don't seem to co-ordinate TSIDs, and often use the
      //    same low value program numbers (1, 2, 3 etc.).
      // - Source ID is not available unless VCT is received, is not unique
      //    across ATSC transmitters, and may legitimately refer to two
      //    channels on cable.
      // We use the logical channel number for channel identification, and
      // throw in the frequency for:
      // - ATSC, in case the user can receive duplicate transmissions intended
      //    for other regions
      // - clear QAM, because it is tuned by frequency (whereas CableCARD
      //    tuning is by channel number or source ID)
      BroadcastStandard broadcastStandard = BroadcastStandard.Atsc;
      if (foundChannel.Channel is ChannelScte)
      {
        broadcastStandard = BroadcastStandard.Scte;
      }
      string lcn = foundChannel.Channel.LogicalChannelNumber;
      string scanMode = string.Empty;
      comboBoxScanMode.Invoke((MethodInvoker)delegate
      {
        scanMode = (string)comboBoxScanMode.SelectedItem;
      });
      IChannelPhysical physicalChannel = foundChannel.Channel as IChannelPhysical;
      if (physicalChannel == null || string.Equals(scanMode, SCAN_MODE_SCTE_CABLECARD))
      {
        // CableCARD or frequency not available: support channel movement detection by LCN.
        return ServiceAgents.Instance.ChannelServiceAgent.GetAtscScteTuningDetails(broadcastStandard, lcn, includeRelations);
      }
      return ServiceAgents.Instance.ChannelServiceAgent.GetAtscScteTuningDetails(broadcastStandard, lcn, includeRelations, physicalChannel.Frequency);
    }

    private void OnScanCompleted()
    {
      this.Invoke((MethodInvoker)delegate
      {
        comboBoxScanMode.Enabled = true;
        string scanMode = (string)comboBoxScanMode.SelectedItem;
        if (
          string.Equals(scanMode, SCAN_MODE_ATSC) ||
          string.Equals(scanMode, SCAN_MODE_SCTE_STANDARD) ||
          string.Equals(scanMode, SCAN_MODE_SCTE_HRC)
        )
        {
          comboBoxTransmitter.Enabled = true;
        }
        buttonScan.Text = "&Scan for channels";
        buttonScan.Enabled = true;
      });
      _scanHelper = null;
    }

    private void comboBoxScanMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      string scanMode = (string)comboBoxScanMode.SelectedItem;

      if (string.Equals(scanMode, SCAN_MODE_ATSC))
      {
        TuningDetailFilter.Load(BroadcastStandard.Atsc, "ATSC.xml", comboBoxTransmitter);
      }
      else if (string.Equals(scanMode, SCAN_MODE_SCTE_STANDARD))
      {
        TuningDetailFilter.Load(BroadcastStandard.Scte, "QAM Standard.xml", comboBoxTransmitter);
      }
      else if (string.Equals(scanMode, SCAN_MODE_SCTE_HRC))
      {
        TuningDetailFilter.Load(BroadcastStandard.Scte, "QAM HRC.xml", comboBoxTransmitter);
      }
      else
      {
        TuningDetailFilter.Load(BroadcastStandard.Unknown, null, comboBoxTransmitter);
      }
    }
  }
}