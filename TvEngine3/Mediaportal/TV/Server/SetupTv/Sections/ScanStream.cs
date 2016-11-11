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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using TuningDetail = Mediaportal.TV.Server.SetupTV.Sections.Helpers.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class ScanStream : SectionSettings
  {
    public const BroadcastStandard SUPPORTED_BROADCAST_STANDARDS = BroadcastStandard.DvbIp;

    private readonly int _tunerId;
    private TuningDetailFilter _tuningDetailFilter = null;
    private ChannelScanHelper _scanHelper = null;
    private IDictionary<string, TuningDetail> _scanTuningDetails = null;

    public ScanStream(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("scan stream: activating, tuner ID = {0}", _tunerId);

      if (_scanHelper == null)
      {
        string service = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanStream" + _tunerId + "Service", string.Empty);
        string stream = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanStream" + _tunerId + "Stream", TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
        _tuningDetailFilter = new TuningDetailFilter(_tunerId, TuningDetailGroup.Stream, comboBoxService, service, comboBoxStream, stream);
      }
      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("scan stream: deactivating, tuner ID = {0}", _tunerId);

      DebugSettings();
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanStream" + _tunerId + "Service", comboBoxService.SelectedItem.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanStream" + _tunerId + "Stream", comboBoxService.SelectedItem.ToString());

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("  service = {0}", comboBoxService.SelectedItem);
      this.LogDebug("  stream  = {0}", comboBoxStream.SelectedItem);
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

      IList<TuningDetail> tuningDetails = _tuningDetailFilter.TuningDetails;
      if (tuningDetails == null || tuningDetails.Count == 0)
      {
        return;
      }
      _scanTuningDetails = new Dictionary<string, TuningDetail>(tuningDetails.Count);
      foreach (TuningDetail tuningDetail in tuningDetails)
      {
        _scanTuningDetails[tuningDetail.Url] = tuningDetail;
      }

      listViewProgress.Items.Clear();
      ListViewItem item = listViewProgress.Items.Add(string.Format("start scanning {0}...", comboBoxService.SelectedItem));
      item.EnsureVisible();
      this.LogInfo("scan stream: start scanning, service = {0}", comboBoxService.SelectedItem);

      _scanHelper = new ChannelScanHelper(_tunerId, listViewProgress, progressBarProgress, null, OnScanCompleted, progressBarSignalStrength, progressBarSignalQuality);
      if (_scanHelper.StartScan(tuningDetails))
      {
        comboBoxService.Enabled = false;
        comboBoxStream.Enabled = false;
        buttonScan.Text = "&Stop";
      }
    }

    private void OnScanCompleted()
    {
      this.Invoke((MethodInvoker)delegate
      {
        comboBoxService.Enabled = true;
        comboBoxStream.Enabled = true;
        buttonScan.Text = "&Scan for channels";
        buttonScan.Enabled = true;
      });
      _scanHelper = null;
    }
  }
}