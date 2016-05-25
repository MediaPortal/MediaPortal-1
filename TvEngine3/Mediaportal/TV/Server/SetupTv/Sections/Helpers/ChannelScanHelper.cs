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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using MediaPortal.Common.Utils.ExtensionMethods;
using DbTuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;
using FileTuningDetail = Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  internal class ChannelScanHelper
  {
    private class Counter
    {
      public IList<IChannel> New = new List<IChannel>();
      public IList<IChannel> Updated = new List<IChannel>();
      public IList<IChannel> Ignored = new List<IChannel>();
    }

    public delegate IList<DbTuningDetail> GetDbExistingTuningDetailCandidatesDelegate(ScannedChannel foundChannel, bool useChannelMovementDetection);
    public delegate IList<FileTuningDetail> NitScanFoundTransmittersDelegate(IList<FileTuningDetail> transmitters);
    public delegate void ScanCompletedDelegate();

    #region variables

    private int _tunerId;
    private bool _stopScanning = false;

    private readonly MPListView _listViewProgress = null;
    private readonly ProgressBar _progressBarProgress = null;

    private readonly NitScanFoundTransmittersDelegate _nitScanFoundTransmittersDelegate = null;
    private readonly GetDbExistingTuningDetailCandidatesDelegate _tuningDetailLookupDelegate = null;
    private readonly ScanCompletedDelegate _scanCompletedDelegate = null;

    private readonly System.Timers.Timer _signalStatusUpdateTimer = new System.Timers.Timer();
    private readonly ProgressBar _progressBarSignalStrength = null;
    private readonly ProgressBar _progressBarSignalQuality = null;

    private Thread _scanThread = null;

    #endregion

    public ChannelScanHelper(int tunerId, MPListView listViewProgress, ProgressBar progressBarProgress, NitScanFoundTransmittersDelegate nitScanFoundTransmittersDelegate, GetDbExistingTuningDetailCandidatesDelegate tuningDetailLookupDelegate, ScanCompletedDelegate scanCompletedDelegate, ProgressBar progressBarSignalStrength, ProgressBar progressBarSignalQuality)
    {
      _tunerId = tunerId;
      _listViewProgress = listViewProgress;
      _progressBarProgress = progressBarProgress;
      _nitScanFoundTransmittersDelegate = nitScanFoundTransmittersDelegate;
      _tuningDetailLookupDelegate = tuningDetailLookupDelegate;
      _scanCompletedDelegate = scanCompletedDelegate;
      _progressBarSignalStrength = progressBarSignalStrength;
      _progressBarSignalQuality = progressBarSignalQuality;

      _signalStatusUpdateTimer.Interval = 1000;
      _signalStatusUpdateTimer.Elapsed += UpdateSignalStatus;
    }

    ~ChannelScanHelper()
    {
      if (_signalStatusUpdateTimer != null)
      {
        _signalStatusUpdateTimer.Dispose();
      }
    }

    private void UpdateSignalStatus(object sender, System.Timers.ElapsedEventArgs e)
    {
      bool isLocked;
      bool isPresent;
      int strength;
      int quality;
      ServiceAgents.Instance.ControllerServiceAgent.GetSignalStatus(_tunerId, false, out isLocked, out isPresent, out strength, out quality);
      if (_progressBarSignalStrength.InvokeRequired)
      {
        _progressBarSignalStrength.Invoke((MethodInvoker)delegate
        {
          _progressBarSignalStrength.Value = strength;
        });
      }
      else
      {
        _progressBarSignalStrength.Value = strength;
      }
      if (_progressBarSignalQuality.InvokeRequired)
      {
        _progressBarSignalQuality.Invoke((MethodInvoker)delegate
        {
          _progressBarSignalQuality.Value = quality;
        });
      }
      else
      {
        _progressBarSignalQuality.Value = quality;
      }
    }

    public bool StartNitScan(FileTuningDetail tuningDetail)
    {
      return StartScan(new List<FileTuningDetail> { tuningDetail }, ScanType.FullNetworkInformationTable);
    }

    public bool StartScan(IList<FileTuningDetail> tuningDetails, ScanType scanType = ScanType.PredefinedProvider)
    {
      if (tuningDetails == null || tuningDetails.Count == 0)
      {
        return false;
      }
      if (!ServiceAgents.Instance.TunerServiceAgent.GetTuner(_tunerId, TunerIncludeRelationEnum.None).IsEnabled)
      {
        MessageBox.Show("Tuner is disabled. Please enable the tuner before scanning.", SectionSettings.MESSAGE_CAPTION);
        this.LogInfo("channel scan: tuner {0} disabled", _tunerId);
        return false;
      }
      if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(_tunerId))
      {
        MessageBox.Show("Tuner is not found. Please make sure the tuner is present before scanning.", SectionSettings.MESSAGE_CAPTION);
        this.LogInfo("channel scan: tuner {0} not present", _tunerId);
        return false;
      }
      IUser user;
      if (ServiceAgents.Instance.ControllerServiceAgent.IsCardInUse(_tunerId, out user))
      {
        MessageBox.Show("Tuner is locked. Scanning is not possible at the moment. Perhaps you are using another part of a hybrid/multi-mode tuner?", SectionSettings.MESSAGE_CAPTION);
        this.LogInfo("channel scan: tuner {0} in use", _tunerId);
        return false;
      }

      this.LogInfo("channel scan: tuner {0} start scan, type = {1}", _tunerId, scanType);
      _signalStatusUpdateTimer.Start();

      _stopScanning = false;
      ThreadStart starter;
      string threadName;
      if (scanType == ScanType.FullNetworkInformationTable)
      {
        starter = delegate { DoNitScan(tuningDetails[0]); };
        threadName = "NIT channel scanner";
      }
      else
      {
        starter = delegate { DoScan(tuningDetails, scanType == ScanType.FastNetworkInformation); };
        threadName = "standard channel scanner";
      }
      _scanThread = new Thread(starter);
      _scanThread.Name = threadName;
      _scanThread.Start();
      return true;
    }

    public void StopScan()
    {
      this.LogInfo("channel scan: tuner {0} stop scanning", _tunerId);
      _stopScanning = true;
    }

    private void DoScan(IList<FileTuningDetail> tuningDetails, bool isFastNetworkScan)
    {
      IDictionary<MediaType, Counter> overallCounters = new Dictionary<MediaType, Counter>(2);
      IDictionary<MediaType, IDictionary<string, int>> channelGroupsByMediaType = new Dictionary<MediaType, IDictionary<string, int>>(2);
      List<GroupMap> newChannelGroupMappings = new List<GroupMap>(tuningDetails.Count * 10);

      try
      {
        TvResult result = ServiceAgents.Instance.ControllerServiceAgent.Scan(string.Format("{0} - TV Server Configuration tuner {1} scanner", System.Net.Dns.GetHostName(), _tunerId), _tunerId, tuningDetails[0].GetTuningChannel());
        if (result == TvResult.SWEncoderMissing)
        {
          this.LogError("channel scan: failed to scan, missing software encoder");
          MessageBox.Show("Please install supported software encoders for your tuner.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        if (result == TvResult.GraphBuildingFailed)
        {
          this.LogError("channel scan: failed to scan, tuner loading failed");
          MessageBox.Show("Failed to load the tuner. Your tuner is probably not supported. Please create a report in our forum.",
            SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        bool useChannelMovementDetection = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanEnableChannelMovementDetection", false);
        bool storeEncryptedChannels = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanStoreEncryptedChannels", true);

        this.LogInfo("channel scan: settings...");
        this.LogInfo("  detect channel movement?  = {0}", useChannelMovementDetection);
        this.LogInfo("  store encrypted channels? = {0}", storeEncryptedChannels);
        Dictionary<ChannelGroupType, string[]> channelGroupConfiguration = ReadAutomaticChannelGroupConfig();
        this.LogInfo("  channel groups...");
        foreach (var groupType in channelGroupConfiguration)
        {
          if (groupType.Value.Length > 0)
          {
            this.LogInfo("    {0, 13} = [{1}]", groupType.Key, string.Join(", ", groupType.Value));
          }
          else
          {
            this.LogInfo("    {0, 13} = [all]", groupType.Key);
          }
        }

        HashSet<IChannel> tunedTransmitters = new HashSet<IChannel>();
        HashSet<uint> tunedTransportStreams = new HashSet<uint>();
        for (int i = 0; i < tuningDetails.Count; i++)
        {
          if (_stopScanning)
          {
            return;
          }

          float progressPercentage = (float)i * 100 / tuningDetails.Count;
          if (progressPercentage > 100f)
          {
            progressPercentage = 100;
          }
          _progressBarProgress.Invoke((MethodInvoker)delegate
          {
            _progressBarProgress.Value = (int)progressPercentage;
          });

          FileTuningDetail tuningDetail = tuningDetails[i];

          // Check if we've already found/scanned the target transport stream
          // at a different frequency.
          uint transportStreamKey = ((uint)tuningDetail.OriginalNetworkId << 16) | tuningDetail.TransportStreamId;
          if (transportStreamKey > 0 && tunedTransportStreams.Contains(transportStreamKey))
          {
            continue;
          }

          // Assemble the list of possible frequencies for this transmitter.
          if (tuningDetail.Frequencies == null || tuningDetail.Frequencies.Count == 0)
          {
            tuningDetail.Frequencies = new List<int> { tuningDetail.Frequency };
          }
          IList<Tuple<int, bool>> frequencies = new List<Tuple<int, bool>>(tuningDetail.Frequencies.Count * 3);
          foreach (int frequency in tuningDetail.Frequencies)
          {
            if (frequency > 0)
            {
              frequencies.Add(new Tuple<int, bool>(frequency, false));
              if (tuningDetail.FrequencyOffset != 0)
              {
                frequencies.Add(new Tuple<int, bool>(frequency + tuningDetail.FrequencyOffset, true));
                frequencies.Add(new Tuple<int, bool>(frequency - tuningDetail.FrequencyOffset, true));
              }
            }
          }
          tuningDetail.Frequencies.Clear();

          IChannel tuneChannel = null;
          IList<ScannedChannel> channels = null;
          IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames = null;
          string line;
          bool skipOffsetFrequencies = false;
          foreach (Tuple<int, bool> frequency in frequencies)
          {
            if (frequency.Item2 && skipOffsetFrequencies)
            {
              // Skip offset frequencies when a previous frequency was locked.
              continue;
            }
            skipOffsetFrequencies = false;

            tuningDetail.Frequency = frequency.Item1;
            tuneChannel = tuningDetail.GetTuningChannel();
            if (tunedTransmitters.Contains(tuneChannel))
            {
              // Skip tuning detail combinations that have previously been tried.
              continue;
            }
            tunedTransmitters.Add(tuneChannel);

            // Don't log/display the offset +/- if this is an offset frequency.
            int offset = tuningDetail.FrequencyOffset;
            if (frequency.Item2)
            {
              tuningDetail.FrequencyOffset = 0;
            }
            line = tuningDetail.ToString();
            if (frequency.Item2)
            {
              tuningDetail.FrequencyOffset = offset;
            }

            this.LogInfo("channel scan: {0}", line);
            line += "...";
            ListViewItem item = null;
            _listViewProgress.Invoke((MethodInvoker)delegate
            {
              item = _listViewProgress.Items.Add(new ListViewItem(line));
              item.EnsureVisible();
            });

            ServiceAgents.Instance.ControllerServiceAgent.Scan(_tunerId, tuneChannel, isFastNetworkScan, out channels, out groupNames);
            if (channels == null || channels.Count == 0)
            {
              bool isSignalLocked;
              bool isSignalPresent;
              int signalStrength;
              int signalQuality;
              ServiceAgents.Instance.ControllerServiceAgent.GetSignalStatus(_tunerId, false, out isSignalLocked, out isSignalPresent, out signalStrength, out signalQuality);
              if (!isSignalLocked)
              {
                line += " no signal";
                this.LogInfo("  no signal");
              }
              else
              {
                line += " signal locked, no channels found";
                this.LogInfo("  signal locked, no channels found");
              }
              _listViewProgress.Invoke((MethodInvoker)delegate
              {
                item.Text = line;
                item.ForeColor = Color.Red;
              });
              continue;
            }

            // The tuner is locked onto signal => no need to scan offset frequencies.
            skipOffsetFrequencies = frequency.Item2;

            bool foundTargetTransportStream = false;
            IDictionary<MediaType, Counter> transmitterCounters = new Dictionary<MediaType, Counter>(2);
            foreach (ScannedChannel scannedChannel in channels)
            {
              // Update counters.
              IChannel channel = scannedChannel.Channel;
              Counter overallCounter;
              if (!overallCounters.TryGetValue(channel.MediaType, out overallCounter))
              {
                overallCounter = new Counter();
                overallCounters.Add(channel.MediaType, overallCounter);
              }
              Counter transmitterCounter;
              if (!transmitterCounters.TryGetValue(channel.MediaType, out transmitterCounter))
              {
                transmitterCounter = new Counter();
                transmitterCounters.Add(channel.MediaType, transmitterCounter);
              }

              // Check if this is the target transport stream.
              if (transportStreamKey > 0)
              {
                ChannelDvbBase dvbChannel = channel as ChannelDvbBase;
                if (dvbChannel != null)
                {
                  uint actualTsKey = ((uint)dvbChannel.OriginalNetworkId << 16) | (ushort)dvbChannel.TransportStreamId;
                  foundTargetTransportStream = transportStreamKey == actualTsKey;
                  tunedTransportStreams.Add(actualTsKey);
                  transportStreamKey = 0;
                }
              }

              // Skip encrypted channels if configured to do so.
              if (!storeEncryptedChannels && channel.IsEncrypted)
              {
                overallCounter.Ignored.Add(channel);
                transmitterCounter.Ignored.Add(channel);
                continue;
              }

              // Find matching tuning details in the database.
              IList<DbTuningDetail> possibleTuningDetails = _tuningDetailLookupDelegate(scannedChannel, useChannelMovementDetection);
              DbTuningDetail dbTuningDetail = null;
              if (possibleTuningDetails != null)
              {
                if (possibleTuningDetails.Count == 1)
                {
                  dbTuningDetail = possibleTuningDetails[0];
                }
                else
                {
                  foreach (DbTuningDetail td in possibleTuningDetails)
                  {
                    if (string.Equals(td.Name, channel.Name))
                    {
                      dbTuningDetail = td;
                      break;
                    }
                  }
                }
              }

              // Update or add a channel as appropriate.
              Channel dbChannel;
              if (dbTuningDetail != null)
              {
                UpdateChannel(channel, scannedChannel.IsVisibleInGuide, dbTuningDetail);
                overallCounter.Updated.Add(channel);
                transmitterCounter.Updated.Add(channel);
              }
              else
              {
                dbChannel = AddChannel(channel, scannedChannel.IsVisibleInGuide);
                overallCounter.New.Add(channel);
                transmitterCounter.New.Add(channel);

                // Automatic channel grouping...
                ICollection<string> channelGroupNames = GetGroupNamesForChannel(channelGroupConfiguration, scannedChannel.Groups, groupNames);
                newChannelGroupMappings.AddRange(CreateChannelGroupMapsForChannel(channelGroupNames, channel.MediaType, dbChannel.IdChannel, channelGroupsByMediaType));
              }
            }

            // Log and show progress/details.
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (KeyValuePair<MediaType, Counter> mediaTypeCounter in transmitterCounters)
            {
              line = string.Format("  {0} updated, count = {1}...", mediaTypeCounter.Key.GetDescription(), mediaTypeCounter.Value.Updated.Count);
              this.LogInfo(line);
              items.Add(new ListViewItem(line));
              foreach (IChannel c in mediaTypeCounter.Value.Updated)
              {
                this.LogDebug("    {0}", c);
                items.Add(new ListViewItem(string.Format("    {0}", c.Name)));
              }

              line = string.Format("  {0} new, count = {1}...", mediaTypeCounter.Key.GetDescription(), mediaTypeCounter.Value.New.Count);
              this.LogInfo(line);
              items.Add(new ListViewItem(line));
              foreach (IChannel c in mediaTypeCounter.Value.New)
              {
                this.LogDebug("    {0}", c);
                items.Add(new ListViewItem(string.Format("    {0}", c.Name)));
              }

              if (!storeEncryptedChannels)
              {
                line = string.Format("  {0} ignored, count = {1}...", mediaTypeCounter.Key.GetDescription(), mediaTypeCounter.Value.Ignored.Count);
                this.LogInfo(line);
                items.Add(new ListViewItem(line));
                foreach (IChannel c in mediaTypeCounter.Value.Ignored)
                {
                  this.LogDebug("    {0}", c);
                  items.Add(new ListViewItem(string.Format("    {0}", c.Name)));
                }
              }
            }
            _listViewProgress.Invoke((MethodInvoker)delegate
            {
              _listViewProgress.BeginUpdate();
              try
              {
                _listViewProgress.Items.AddRange(items.ToArray());
                _listViewProgress.Items[_listViewProgress.Items.Count - 1].EnsureVisible();
              }
              finally
              {
                _listViewProgress.EndUpdate();
              }
            });

            if (foundTargetTransportStream)
            {
              // If we found the target transport stream then there's no need
              // to scan any more possible frequencies for the transmitter.
              break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "channel scan: unexpected scanning exception");
        _listViewProgress.Invoke((MethodInvoker)delegate
        {
          ListViewItem item = _listViewProgress.Items.Add(new ListViewItem("Unexpected error. Please create a report in our forum."));
          item.ForeColor = Color.Red;
          item.EnsureVisible();
        });
        MessageBox.Show("Encountered unexpected error. Please create a report in our forum.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        if (newChannelGroupMappings.Count > 0)
        {
          string line = string.Format("create {0} channel group mapping(s)...", newChannelGroupMappings.Count);
          this.LogInfo("channel scan: {0}", line);
          ServiceAgents.Instance.ChannelServiceAgent.SaveChannelGroupMaps(newChannelGroupMappings);
        }

        _listViewProgress.Invoke((MethodInvoker)delegate
        {
          this.LogInfo("channel scan: final summary...");
          _listViewProgress.BeginUpdate();
          try
          {
            _listViewProgress.Items.Add("final summary...");
            if (overallCounters.Count == 0)
            {
              _listViewProgress.Items.Add("  no channels found");
              this.LogInfo("  no channels found");
            }
            else
            {
              foreach (KeyValuePair<MediaType, Counter> mediaTypeCounter in overallCounters)
              {
                string line = string.Format("  {0} updated, count = {1}", mediaTypeCounter.Key.GetDescription(), mediaTypeCounter.Value.Updated.Count);
                this.LogInfo(line);
                _listViewProgress.Items.Add(line);
                line = string.Format("  {0} new, count = {1}", mediaTypeCounter.Key.GetDescription(), mediaTypeCounter.Value.New.Count);
                this.LogInfo(line);
                _listViewProgress.Items.Add(line);
                line = string.Format("  {0} ignored, count = {1}", mediaTypeCounter.Key.GetDescription(), mediaTypeCounter.Value.Ignored.Count);
                this.LogInfo(line);
                ListViewItem item = _listViewProgress.Items.Add(line);
                item.EnsureVisible();
              }
            }
          }
          finally
          {
            _listViewProgress.EndUpdate();
          }
        });

        ServiceAgents.Instance.ControllerServiceAgent.StopScan(_tunerId);
        _signalStatusUpdateTimer.Stop();
        _scanCompletedDelegate();
      }
    }

    private void DoNitScan(FileTuningDetail tuningDetail)
    {
      IDictionary<MediaType, Counter> overallCounters = new Dictionary<MediaType, Counter>(2);
      FileTuningDetail[] tuningDetails = null;

      try
      {
        IChannel tuneChannel = tuningDetail.GetTuningChannel();
        TvResult result = ServiceAgents.Instance.ControllerServiceAgent.Scan(string.Format("{0} - TV Server Configuration tuner {1} NIT scanner", System.Net.Dns.GetHostName(), _tunerId), _tunerId, tuneChannel);
        if (result == TvResult.GraphBuildingFailed)
        {
          this.LogError("channel scan: failed to scan, tuner loading failed");
          MessageBox.Show("Failed to load the tuner. Your tuner is probably not supported. Please create a report in our forum.",
            SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        string line = string.Format("search for transmitters, {0}", tuningDetail);
        this.LogInfo("channel scan: {0}", line);
        line += "...";
        ListViewItem item = null;
        _listViewProgress.Invoke((MethodInvoker)delegate
        {
          item = _listViewProgress.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
        });

        tuningDetails = ServiceAgents.Instance.ControllerServiceAgent.ScanNIT(_tunerId, tuneChannel);
        if (tuningDetails == null || tuningDetails.Length == 0)
        {
          tuningDetails = null;
          bool isSignalLocked;
          bool isSignalPresent;
          int signalStrength;
          int signalQuality;
          ServiceAgents.Instance.ControllerServiceAgent.GetSignalStatus(_tunerId, false, out isSignalLocked, out isSignalPresent, out signalStrength, out signalQuality);
          if (!isSignalLocked)
          {
            line += " no signal";
            this.LogInfo("  no signal");
          }
          else
          {
            line += " signal locked, no transmitters found";
            this.LogInfo("  signal locked, no transmitters found");
          }
          _listViewProgress.Invoke((MethodInvoker)delegate
          {
            item.Text = line;
            item.ForeColor = Color.Red;
          });
          return;
        }

        line = string.Format("  transmitters, count = {0}...", tuningDetails.Length);
        this.LogInfo(line);
        _listViewProgress.Invoke((MethodInvoker)delegate
        {
          _listViewProgress.BeginUpdate();
          try
          {
            _listViewProgress.Items.Add(new ListViewItem(line));
            foreach (FileTuningDetail td in tuningDetails)
            {
              this.LogDebug("    {0}", td);
              _listViewProgress.Items.Add(new ListViewItem(string.Format("    {0}", td)));
            }
            _listViewProgress.Items[_listViewProgress.Items.Count - 1].EnsureVisible();
          }
          finally
          {
            _listViewProgress.EndUpdate();
          }
        });

        if (_nitScanFoundTransmittersDelegate != null)
        {
          IList<FileTuningDetail> tuningDetailList = new List<FileTuningDetail>(tuningDetails);
          tuningDetailList = _nitScanFoundTransmittersDelegate(tuningDetailList);
          tuningDetails = new FileTuningDetail[tuningDetailList.Count];
          tuningDetailList.CopyTo(tuningDetails, 0);
        }

        DoScan(tuningDetails, false);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "channel scan: unexpected NIT scanning exception");
        tuningDetails = null;
        _listViewProgress.Invoke((MethodInvoker)delegate
        {
          ListViewItem item = _listViewProgress.Items.Add(new ListViewItem("Unexpected error. Please create a report in our forum."));
          item.ForeColor = Color.Red;
          item.EnsureVisible();
        });
        MessageBox.Show("Encountered unexpected error. Please create a report in our forum.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        if (tuningDetails == null)
        {
          ServiceAgents.Instance.ControllerServiceAgent.StopScan(_tunerId);
          _signalStatusUpdateTimer.Stop();
          _scanCompletedDelegate();
        }
      }
    }

    private ICollection<string> GetGroupNamesForChannel(IDictionary<ChannelGroupType, string[]> channelGroupConfiguration, IDictionary<ChannelGroupType, ICollection<ulong>> channelGroupings, IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      HashSet<string> groupNamesForChannel = new HashSet<string>();

      // For each type of group where automatic grouping is enabled...
      foreach (var groupType in channelGroupConfiguration)
      {
        // If the channel is associated with any of the corresponding groups,
        // and we have names for the groups...
        ICollection<ulong> groupIds;
        IDictionary<ulong, string> groupNamesForGroupType;
        if (!channelGroupings.TryGetValue(groupType.Key, out groupIds) || !groupNames.TryGetValue(groupType.Key, out groupNamesForGroupType))
        {
          continue;
        }

        // Build a list of the group identifiers that the channel is associated with.
        List<ulong> actualGroupIds = new List<ulong>(groupIds.Count);
        foreach (ulong groupId in groupIds)
        {
          if (
            (groupType.Key == ChannelGroupType.DishNetworkMarket && groupId == 0xffa) ||  // Dish non-regional
            (groupType.Key == ChannelGroupType.OpenTvRegion && groupId == 0xffff)         // OpenTV all regions
          )
          {
            foreach (string configuredGroupIdString in groupType.Value)
            {
              ulong configuredGroupId;
              if (ulong.TryParse(configuredGroupIdString, out configuredGroupId) && !groupIds.Contains(groupId))
              {
                actualGroupIds.Add(configuredGroupId);
              }
            }
          }
          else
          {
            actualGroupIds.Add(groupId);
          }
        }

        // If configured to create the group, add the group's name to our list.
        foreach (ulong groupId in actualGroupIds)
        {
          string groupName;
          if (!groupNamesForGroupType.TryGetValue(groupId, out groupName))
          {
            continue;
          }

          if (
            groupType.Key == ChannelGroupType.DishNetworkMarket ||
            groupType.Key == ChannelGroupType.FreesatRegion ||
            groupType.Key == ChannelGroupType.FreeviewSatellite ||
            groupType.Key == ChannelGroupType.OpenTvRegion
          )
          {
            if (groupType.Value.Length > 0 && Array.IndexOf(groupType.Value, groupId.ToString()) < 0)
            {
              continue;
            }
          }
          else if (groupType.Value.Length > 0 && Array.IndexOf(groupType.Value, groupName) < 0)
          {
            continue;
          }

          groupNamesForChannel.Add(groupName);
        }
      }
      return groupNamesForChannel;
    }

    private IList<GroupMap> CreateChannelGroupMapsForChannel(ICollection<string> groupNames, MediaType channelMediaType, int channelId, IDictionary<MediaType, IDictionary<string, int>> channelGroupsByMediaType)
    {
      IList<GroupMap> groupMaps = new List<GroupMap>(groupNames.Count);

      IDictionary<string, int> channelGroupIds;
      if (!channelGroupsByMediaType.TryGetValue(channelMediaType, out channelGroupIds))
      {
        channelGroupIds = new Dictionary<string, int>(20);
        channelGroupsByMediaType.Add(channelMediaType, channelGroupIds);
      }

      foreach (string groupName in groupNames)
      {
        int channelGroupId;
        if (!channelGroupIds.TryGetValue(groupName, out channelGroupId))
        {
          channelGroupId = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(groupName, channelMediaType).IdGroup;
          channelGroupIds.Add(groupName, channelGroupId);
        }

        groupMaps.Add(new GroupMap
        {
          IdGroup = channelGroupId,
          IdChannel = channelId
        });
      }
      return groupMaps;
    }

    private Dictionary<ChannelGroupType, string[]> ReadAutomaticChannelGroupConfig()
    {
      Dictionary<ChannelGroupType, string[]> channelGroupConfiguration = new Dictionary<ChannelGroupType, string[]>(30);
      ChannelGroupType channelGroupTypes = ChannelGroupType.FreesatChannelCategory | ChannelGroupType.NorDigChannelList | ChannelGroupType.VirginMediaChannelCategory;
      channelGroupTypes = (ChannelGroupType)ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateChannelGroups", (int)channelGroupTypes);

      if (channelGroupTypes.HasFlag(ChannelGroupType.ChannelProvider))
      {
        channelGroupConfiguration[ChannelGroupType.ChannelProvider] = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateChannelGroupsChannelProviders", string.Empty).Split('|');
      }
      if (channelGroupTypes.HasFlag(ChannelGroupType.DvbNetwork))
      {
        channelGroupConfiguration[ChannelGroupType.DvbNetwork] = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateChannelGroupsDvbNetworks", string.Empty).Split('|');
      }
      if (channelGroupTypes.HasFlag(ChannelGroupType.DvbBouquet))
      {
        channelGroupConfiguration[ChannelGroupType.DvbBouquet] = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateChannelGroupsDvbBouquets", string.Empty).Split('|');
      }
      if (channelGroupTypes.HasFlag(ChannelGroupType.DvbTargetRegion))
      {
        channelGroupConfiguration[ChannelGroupType.DvbTargetRegion] = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateChannelGroupsDvbTargetRegions", string.Empty).Split('|');
      }
      if (channelGroupTypes.HasFlag(ChannelGroupType.OpenTvRegion))
      {
        string[] configParts = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanProviderOpenTv", string.Empty).Split(',');
        int bouquetId;
        int regionId;
        if (configParts.Length >= 2 && int.TryParse(configParts[0], out bouquetId) && int.TryParse(configParts[1], out regionId))
        {
          channelGroupConfiguration[ChannelGroupType.OpenTvRegion] = new string[1] { ((bouquetId << 16) | regionId).ToString() };
        }
      }
      if (channelGroupTypes.HasFlag(ChannelGroupType.FreeviewSatellite))
      {
        string config = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanProviderFreeviewSatellite", string.Empty);
        int bouquetId;
        if (int.TryParse(config, out bouquetId))
        {
          channelGroupConfiguration[ChannelGroupType.FreeviewSatellite] = new string[1] { config };
        }
      }
      if (channelGroupTypes.HasFlag(ChannelGroupType.FreesatRegion))
      {
        string[] configParts = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanProviderFreesat", string.Empty).Split(',');
        int bouquetId;
        int regionId;
        if (configParts.Length >= 2 && int.TryParse(configParts[0], out bouquetId) && int.TryParse(configParts[1], out regionId))
        {
          channelGroupConfiguration[ChannelGroupType.FreesatRegion] = new string[1] { ((bouquetId << 16) | regionId).ToString() };
        }
      }
      if (channelGroupTypes.HasFlag(ChannelGroupType.DishNetworkMarket))
      {
        string[] configParts = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanProviderDishNetwork", string.Empty).Split(',');
        if (configParts.Length >= 2)
        {
          channelGroupConfiguration[ChannelGroupType.DishNetworkMarket] = new string[1] { configParts[0] };
        }
      }

      foreach (ChannelGroupType groupType in System.Enum.GetValues(typeof(ChannelGroupType)))
      {
        if (groupType != ChannelGroupType.Manual && channelGroupTypes.HasFlag(groupType) && !channelGroupConfiguration.ContainsKey(groupType))
        {
          channelGroupConfiguration[groupType] = new string[0];
        }
      }
      return channelGroupConfiguration;
    }

    public static Channel AddChannel(IChannel channel, bool isVisibleInGuide)
    {
      Channel dbChannel = new Channel();
      dbChannel.Name = channel.Name;
      dbChannel.ChannelNumber = channel.LogicalChannelNumber;
      dbChannel.MediaType = (int)channel.MediaType;
      dbChannel.VisibleInGuide = isVisibleInGuide;
      dbChannel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
      dbChannel.AcceptChanges();
      ServiceAgents.Instance.ChannelServiceAgent.AddTuningDetail(dbChannel.IdChannel, channel);
      return dbChannel;
    }

    public static void UpdateChannel(IChannel channel, bool isVisibleInGuide, DbTuningDetail dbTuningDetail)
    {
      Channel dbChannel = dbTuningDetail.Channel;
      if (dbChannel != null)
      {
        // Update channel name if...
        bool channelUpdated = false;
        if (
          // ...scan found a valid name, and...
          !channel.Name.StartsWith("Unknown ") &&
          (
          // ...the current channel name is invalid, or...
            dbChannel.Name.StartsWith("Unknown ") ||
          // ...the current [valid] channel name has not been customised by the user.
            (
              string.Equals(dbChannel.Name, dbTuningDetail.Name) &&
              !string.Equals(channel.Name, dbTuningDetail.Name)
            )
          )
        )
        {
          dbChannel.Name = channel.Name;
          channelUpdated = true;
        }

        // Update channel LCN if...
        if (
          // ...scan found a valid LCN, and...
          !string.Equals(channel.LogicalChannelNumber, channel.DefaultLogicalChannelNumber) &&
          (
          // ...the current channel LCN is invalid, or...
            string.Equals(dbChannel.ChannelNumber, channel.DefaultLogicalChannelNumber) ||
          // ...the current [valid] channel LCN has not been customised by the user.
            (
              string.Equals(dbChannel.ChannelNumber, dbTuningDetail.LogicalChannelNumber) &&
              !string.Equals(channel.LogicalChannelNumber, dbTuningDetail.LogicalChannelNumber)
            )
          )
        )
        {
          dbChannel.ChannelNumber = channel.LogicalChannelNumber;
          channelUpdated = true;
        }

        if (channelUpdated)
        {
          dbChannel.VisibleInGuide = isVisibleInGuide;
          ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
        }
      }

      ServiceAgents.Instance.ChannelServiceAgent.UpdateTuningDetail(dbTuningDetail.IdChannel, dbTuningDetail.IdTuning, channel);
    }
  }
}