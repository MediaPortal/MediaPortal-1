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
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using DbTuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;
using FileTuningDetail = Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardDvbIP : SectionSettings
  {
    private readonly int _tunerId;
    private TuningDetailFilter _tuningDetailFilter;
    private ChannelScanHelper _scanHelper = null;
    private IDictionary<string, ChannelStream> _m3uChannels;

    public CardDvbIP(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
      base.Text = name;
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("stream: activating, tuner ID = {0}", _tunerId);

      _tuningDetailFilter = new TuningDetailFilter("dvbip", comboBoxService);
      comboBoxService.SelectedItem = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbip" + _tunerId + "Service", string.Empty);
      if (comboBoxService.SelectedItem == null)
      {
        comboBoxService.SelectedIndex = 0;
      }

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("stream: deactivating, tuner ID = {0}", _tunerId);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbip" + _tunerId + "Service", ((CustomFileName)comboBoxService.SelectedItem).ToString());
      base.OnSectionDeActivated();
    }

    private void buttonScan_Click(object sender, EventArgs e)
    {
      if (_scanHelper != null)
      {
        buttonScan.Text = "Cancelling...";
        _scanHelper.StopScan();
        return;
      }

      _m3uChannels = ReadM3uPlaylist(((CustomFileName)comboBoxService.SelectedItem).FileName);
      List<FileTuningDetail> tuningDetails = new List<FileTuningDetail>(_m3uChannels.Count);
      foreach (ChannelStream channel in _m3uChannels.Values)
      {
        tuningDetails.Add(new FileTuningDetail
        {
          BroadcastStandard = BroadcastStandard.DvbIp,
          Url = channel.Url
        });
      }
      if (tuningDetails.Count == 0)
      {
        return;
      }

      listViewProgress.Items.Clear();
      ListViewItem item = listViewProgress.Items.Add(string.Format("start scanning {0}...", comboBoxService.SelectedItem));
      item.EnsureVisible();
      this.LogInfo("stream: start scanning {0}...", comboBoxService.SelectedItem);

      _scanHelper = new ChannelScanHelper(_tunerId, listViewProgress, progressBarProgress, null, OnGetDbExistingTuningDetailCandidates, OnScanCompleted, progressBarSignalStrength, progressBarSignalQuality);
      if (_scanHelper.StartScan(tuningDetails))
      {
        comboBoxService.Enabled = false;
        buttonScan.Text = "Cancel...";
      }
    }

    private IDictionary<string, ChannelStream> ReadM3uPlaylist(string fileName)
    {
      IDictionary<string, ChannelStream> channels = new Dictionary<string, ChannelStream>(50);

      try
      {
        ChannelStream channel = new ChannelStream();
        foreach (string line in File.ReadLines(fileName))
        {
          if (!string.IsNullOrWhiteSpace(line))
          {
            Match m = Regex.Match(line, @"^\s*#EXTINF\s*:\s*(\d+)\s*,\s*([^\s].*?)\s*$");
            if (m.Success)
            {
              if (!string.IsNullOrEmpty(channel.Url))
              {
                channels.Add(channel.Url, channel);
                channel = new ChannelStream();
              }
              string lcn = m.Groups[1].Captures[0].Value;
              if (!string.Equals(lcn, "0"))
              {
                channel.LogicalChannelNumber = lcn;
              }
              channel.Name = m.Groups[2].Captures[0].Value;
            }
            else
            {
              string l = line.Trim();
              if (!l.StartsWith("#"))
              {
                channel.Url = l;
              }
            }
          }
        }
        if (!string.IsNullOrEmpty(channel.Url))
        {
          channels.Add(channel.Url, channel);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "stream: failed to parse \"{0}\"", fileName);
      }
      return channels;
    }

    private IList<DbTuningDetail> OnGetDbExistingTuningDetailCandidates(ScannedChannel foundChannel, bool useChannelMovementDetection)
    {
      // Most sources are single program transport streams (SPTSs), so we can
      // simply lookup the tuning detail by URL. However, also support MPTSs
      // with "safe" DVB ONID + TSID + SID lookup.
      ChannelStream streamChannel = foundChannel.Channel as ChannelStream;
      if (streamChannel == null)
      {
        return null;
      }

      // Set name and/or LCN from M3U if not found in the stream.
      ChannelStream m3uChannel;
      if (_m3uChannels.TryGetValue(streamChannel.Url, out m3uChannel))
      {
        if (!string.IsNullOrEmpty(m3uChannel.Name) && string.Equals(streamChannel.Name, streamChannel.Url))
        {
          streamChannel.Name = m3uChannel.Name;
        }
        if (!string.IsNullOrEmpty(m3uChannel.LogicalChannelNumber) && string.Equals(streamChannel.LogicalChannelNumber, streamChannel.DefaultLogicalChannelNumber))
        {
          streamChannel.LogicalChannelNumber = m3uChannel.LogicalChannelNumber;
        }
      }

      if (useChannelMovementDetection)
      {
        IList<DbTuningDetail> tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(BroadcastStandard.DvbIp, streamChannel.OriginalNetworkId, streamChannel.ServiceId, streamChannel.TransportStreamId);
        if (tuningDetails != null && tuningDetails.Count == 1)
        {
          return tuningDetails;
        }
      }
      return ServiceAgents.Instance.ChannelServiceAgent.GetStreamTuningDetails(streamChannel.Url);
    }

    private void OnScanCompleted()
    {
      this.Invoke((MethodInvoker)delegate
      {
        buttonScan.Text = "Scan for channels";
        comboBoxService.Enabled = true;
      });
      _scanHelper = null;
    }
  }
}