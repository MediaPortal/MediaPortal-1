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
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using MediaPortal.Common.Utils.ExtensionMethods;
using DbTuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;
using FileTuningDetail = Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  internal class ChannelScanHelper
  {
    private class Counter
    {
      public IList<IChannel> New = new List<IChannel>();
      public IList<IChannel> Updated = new List<IChannel>();
    }

    public delegate IList<DbTuningDetail> GetDbExistingTuningDetailCandidatesDelegate(FileTuningDetail tuningDetail, IChannel tuneChannel, IChannel foundChannel, bool useChannelMovementDetection);
    public delegate void NewChannelDelegate(FileTuningDetail tuningDetail, IChannel tuneChannel, Channel dbChannel, IChannel channel);
    public delegate IList<FileTuningDetail> NitScanFoundTransmittersDelegate(IList<FileTuningDetail> transmitters);
    public delegate void ScanCompletedDelegate();

    private int _tunerId;
    private bool _stopScanning = false;
    private readonly System.Timers.Timer _signalStatusUpdateTimer = new System.Timers.Timer();
    private ProgressBar _progressBarSignalStrength = null;
    private ProgressBar _progressBarSignalQuality = null;
    private Thread _scanThread = null;

    public ChannelScanHelper(int tunerId)
    {
      _tunerId = tunerId;
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
      _progressBarSignalStrength.Invoke((MethodInvoker)delegate
      {
        _progressBarSignalStrength.Value = strength;
      });
      _progressBarSignalQuality.Invoke((MethodInvoker)delegate
      {
        _progressBarSignalQuality.Value = quality;
      });
    }

    public bool StartScan(IList<FileTuningDetail> tuningDetails, MPListView listViewStatus, ProgressBar barProgress, GetDbExistingTuningDetailCandidatesDelegate tuningDetailLookupDelegate, NewChannelDelegate newChannelDelegate, ScanCompletedDelegate scanCompletedDelegate, ProgressBar barSignalStrength, ProgressBar barSignalQuality)
    {
      return StartScan(false, tuningDetails, listViewStatus, barProgress, null, tuningDetailLookupDelegate, newChannelDelegate, scanCompletedDelegate, barSignalStrength, barSignalQuality);
    }

    public bool StartNitScan(FileTuningDetail tuningDetail, MPListView listViewStatus, ProgressBar barProgress, NitScanFoundTransmittersDelegate foundTransmittersDelegate, GetDbExistingTuningDetailCandidatesDelegate tuningDetailLookupDelegate, NewChannelDelegate newChannelDelegate, ScanCompletedDelegate scanCompletedDelegate, ProgressBar barSignalStrength, ProgressBar barSignalQuality)
    {
      return StartScan(true, new List<FileTuningDetail> { tuningDetail }, listViewStatus, barProgress, foundTransmittersDelegate, tuningDetailLookupDelegate, newChannelDelegate, scanCompletedDelegate, barSignalStrength, barSignalQuality);
    }

    private bool StartScan(bool isNitScan, IList<FileTuningDetail> tuningDetails, MPListView listViewStatus, ProgressBar barProgress, NitScanFoundTransmittersDelegate foundTransmittersDelegate, GetDbExistingTuningDetailCandidatesDelegate tuningDetailLookupDelegate, NewChannelDelegate newChannelDelegate, ScanCompletedDelegate scanCompletedDelegate, ProgressBar barSignalStrength, ProgressBar barSignalQuality)
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

      if (isNitScan)
      {
        this.LogInfo("channel scan: tuner {0} start NIT scan", _tunerId);
      }
      else
      {
        this.LogInfo("channel scan: tuner {0} start standard scan", _tunerId);
      }
      _progressBarSignalStrength = barSignalStrength;
      _progressBarSignalQuality = barSignalQuality;
      _signalStatusUpdateTimer.Start();

      _stopScanning = false;
      ThreadStart starter;
      string threadName;
      if (isNitScan)
      {
        starter = delegate { DoNitScan(tuningDetails[0], listViewStatus, barProgress, foundTransmittersDelegate, tuningDetailLookupDelegate, newChannelDelegate, scanCompletedDelegate); };
        threadName = "NIT channel scanner";
      }
      else
      {
        starter = delegate { DoScan(tuningDetails, listViewStatus, barProgress, tuningDetailLookupDelegate, newChannelDelegate, scanCompletedDelegate); };
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

    private void DoScan(IList<FileTuningDetail> tuningDetails, MPListView listViewStatus, ProgressBar barProgress, GetDbExistingTuningDetailCandidatesDelegate tuningDetailLookupDelegate, NewChannelDelegate newChannelDelegate, ScanCompletedDelegate scanCompletedDelegate)
    {
      IDictionary<MediaType, Counter> overallCounters = new Dictionary<MediaType, Counter>(2);
      IDictionary<MediaType, IDictionary<string, int>> channelGroupsByMediaType = new Dictionary<MediaType, IDictionary<string, int>>(2);
      IList<GroupMap> newChannelGroupMappings = new List<GroupMap>(tuningDetails.Count * 10);

      bool useChannelMovementDetection = ServiceAgents.Instance.SettingServiceAgent.GetValue("channelMovementDetectionEnabled", false);
      bool createChannelGroupsChannelProviders = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateProviderChannelGroups", false);
      bool createChannelGroupsBroadcastStandards = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateBroadcastStandardChannelGroups", false);

      try
      {
        TvResult result = ServiceAgents.Instance.ControllerServiceAgent.Scan(string.Format("scanner_{0}", _tunerId), _tunerId, tuningDetails[0].GetTuningChannel());
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

        this.LogInfo("channel scan: settings, channel movement = {0}, provider groups = {1}, broadcast standard groups = {2}", useChannelMovementDetection, createChannelGroupsChannelProviders, createChannelGroupsBroadcastStandards);
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
          barProgress.Invoke((MethodInvoker)delegate
          {
            barProgress.Value = (int)progressPercentage;
          });

          FileTuningDetail tuningDetail = tuningDetails[i];
          IChannel tuneChannel = tuningDetail.GetTuningChannel();

          string line = tuningDetail.ToString();
          this.LogInfo("channel scan: {0}", line);
          line += "...";
          ListViewItem item = null;
          listViewStatus.Invoke((MethodInvoker)delegate
          {
            item = listViewStatus.Items.Add(new ListViewItem(line));
            item.EnsureVisible();
          });

          IChannel[] channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_tunerId, tuneChannel);
          if (channels == null || channels.Length == 0)
          {
            bool noChannels = true;
            bool isSignalLocked;
            bool isSignalPresent;
            int signalStrength;
            int signalQuality;
            ServiceAgents.Instance.ControllerServiceAgent.GetSignalStatus(_tunerId, false, out isSignalLocked, out isSignalPresent, out signalStrength, out signalQuality);
            if (!isSignalLocked && tuningDetail.FrequencyOffset != 0)
            {
              tuningDetail.Frequency += tuningDetail.FrequencyOffset;
              tuneChannel = tuningDetail.GetTuningChannel();
              channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_tunerId, tuneChannel);
              noChannels = channels == null || channels.Length == 0;
              if (noChannels)
              {
                ServiceAgents.Instance.ControllerServiceAgent.GetSignalStatus(_tunerId, false, out isSignalLocked, out isSignalPresent, out signalStrength, out signalQuality);
                if (!isSignalLocked)
                {
                  tuningDetail.Frequency -= (2 * tuningDetail.FrequencyOffset);
                  tuneChannel = tuningDetail.GetTuningChannel();
                  channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_tunerId, tuneChannel);
                  noChannels = channels == null || channels.Length == 0;
                  if (noChannels)
                  {
                    ServiceAgents.Instance.ControllerServiceAgent.GetSignalStatus(_tunerId, false, out isSignalLocked, out isSignalPresent, out signalStrength, out signalQuality);
                  }
                }
              }
            }

            if (noChannels)
            {
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
              listViewStatus.Invoke((MethodInvoker)delegate
              {
                item.Text = line;
                item.ForeColor = Color.Red;
              });
              continue;
            }
          }

          IDictionary<MediaType, Counter> transmitterCounters = new Dictionary<MediaType, Counter>(2);
          foreach (IChannel channel in channels)
          {
            IList<DbTuningDetail> possibleTuningDetails = tuningDetailLookupDelegate(tuningDetail, tuneChannel, channel, useChannelMovementDetection);
            DbTuningDetail dbTuningDetail = null;
            if (possibleTuningDetails != null)
            {
              if (possibleTuningDetails.Count == 1)
              {
                dbTuningDetail = possibleTuningDetails[0];
              }
              else
              {
                foreach (TuningDetail td in possibleTuningDetails)
                {
                  if (string.Equals(td.Name, channel.Name))
                  {
                    dbTuningDetail = td;
                    break;
                  }
                }
              }
            }

            Channel dbChannel;
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
            if (dbTuningDetail == null)
            {
              dbChannel = AddChannel(channel);
              overallCounter.New.Add(channel);
              transmitterCounter.New.Add(channel);

              // Automatic channel groups.
              if (createChannelGroupsChannelProviders && !string.IsNullOrEmpty(channel.Provider))
              {
                GroupMap mapping = CreateChannelGroupMapForChannel(channel.Provider, channel.MediaType, dbChannel.IdChannel, channelGroupsByMediaType);
                if (mapping != null)
                {
                  newChannelGroupMappings.Add(mapping);
                }
              }
              if (createChannelGroupsBroadcastStandards)
              {
                GroupMap mapping = CreateChannelGroupMapForChannel(tuningDetail.BroadcastStandard.GetDescription(), channel.MediaType, dbChannel.IdChannel, channelGroupsByMediaType);
                if (mapping != null)
                {
                  newChannelGroupMappings.Add(mapping);
                }
              }
              if (newChannelDelegate != null)
              {
                newChannelDelegate(tuningDetail, tuneChannel, dbChannel, channel);
              }
            }
            else
            {
              UpdateChannel(channel, dbTuningDetail);
              overallCounter.Updated.Add(channel);
              transmitterCounter.Updated.Add(channel);
            }
          }

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
          }
          listViewStatus.Invoke((MethodInvoker)delegate
          {
            listViewStatus.BeginUpdate();
            try
            {
              listViewStatus.Items.AddRange(items.ToArray());
              listViewStatus.Items[listViewStatus.Items.Count - 1].EnsureVisible();
            }
            finally
            {
              listViewStatus.EndUpdate();
            }
          });
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "channel scan: unexpected scanning exception");
        listViewStatus.Invoke((MethodInvoker)delegate
        {
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem("Unexpected error. Please create a report in our forum."));
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

        listViewStatus.Invoke((MethodInvoker)delegate
        {
          this.LogInfo("channel scan: final summary...");
          listViewStatus.BeginUpdate();
          try
          {
            listViewStatus.Items.Add("final summary...");
            if (overallCounters.Count == 0)
            {
              listViewStatus.Items.Add("  no channels found");
              this.LogInfo("  no channels found");
            }
            else
            {
              foreach (KeyValuePair<MediaType, Counter> mediaTypeCounter in overallCounters)
              {
                string line = string.Format("  {0} updated, count = {1}", mediaTypeCounter.Key.GetDescription(), mediaTypeCounter.Value.Updated.Count);
                this.LogInfo(line);
                listViewStatus.Items.Add(line);
                line = string.Format("  {0} new, count = {1}", mediaTypeCounter.Key.GetDescription(), mediaTypeCounter.Value.New.Count);
                this.LogInfo(line);
                ListViewItem item = listViewStatus.Items.Add(line);
                item.EnsureVisible();
              }
            }
          }
          finally
          {
            listViewStatus.EndUpdate();
          }
        });

        ServiceAgents.Instance.ControllerServiceAgent.StopCard(_tunerId);
        _signalStatusUpdateTimer.Stop();
        scanCompletedDelegate();
      }
    }

    private void DoNitScan(FileTuningDetail tuningDetail, MPListView listViewStatus, ProgressBar barProgress, NitScanFoundTransmittersDelegate foundTransmittersDelegate, GetDbExistingTuningDetailCandidatesDelegate tuningDetailLookupDelegate, NewChannelDelegate newChannelDelegate, ScanCompletedDelegate scanCompletedDelegate)
    {
      IDictionary<MediaType, Counter> overallCounters = new Dictionary<MediaType, Counter>(2);
      FileTuningDetail[] tuningDetails = null;

      try
      {
        IChannel tuneChannel = tuningDetail.GetTuningChannel();
        TvResult result = ServiceAgents.Instance.ControllerServiceAgent.Scan(string.Format("NIT_scanner_{0}", _tunerId), _tunerId, tuneChannel);
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
        listViewStatus.Invoke((MethodInvoker)delegate
        {
          item = listViewStatus.Items.Add(new ListViewItem(line));
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
          listViewStatus.Invoke((MethodInvoker)delegate
          {
            item.Text = line;
            item.ForeColor = Color.Red;
          });
          return;
        }

        line = string.Format("  transmitters, count = {0}...", tuningDetails.Length);
        this.LogInfo(line);
        listViewStatus.Invoke((MethodInvoker)delegate
        {
          listViewStatus.BeginUpdate();
          try
          {
            listViewStatus.Items.Add(new ListViewItem(line));
            foreach (FileTuningDetail td in tuningDetails)
            {
              this.LogDebug("    {0}", td);
              listViewStatus.Items.Add(new ListViewItem(string.Format("    {0}", td)));
            }
            listViewStatus.Items[listViewStatus.Items.Count - 1].EnsureVisible();
          }
          finally
          {
            listViewStatus.EndUpdate();
          }
        });

        if (foundTransmittersDelegate != null)
        {
          IList<FileTuningDetail> tuningDetailList = new List<FileTuningDetail>(tuningDetails);
          tuningDetailList = foundTransmittersDelegate(tuningDetailList);
          tuningDetails = new FileTuningDetail[tuningDetailList.Count];
          tuningDetailList.CopyTo(tuningDetails, 0);
        }

        DoScan(tuningDetails, listViewStatus, barProgress, tuningDetailLookupDelegate, newChannelDelegate, scanCompletedDelegate);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "channel scan: unexpected NIT scanning exception");
        tuningDetails = null;
        listViewStatus.Invoke((MethodInvoker)delegate
        {
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem("Unexpected error. Please create a report in our forum."));
          item.ForeColor = Color.Red;
          item.EnsureVisible();
        });
        MessageBox.Show("Encountered unexpected error. Please create a report in our forum.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        if (tuningDetails == null)
        {
          ServiceAgents.Instance.ControllerServiceAgent.StopCard(_tunerId);
          _signalStatusUpdateTimer.Stop();
          scanCompletedDelegate();
        }
      }
    }

    private GroupMap CreateChannelGroupMapForChannel(string channelGroupName, MediaType channelGroupMediaType, int channelId, IDictionary<MediaType, IDictionary<string, int>> channelGroupsByMediaType)
    {
      IDictionary<string, int> channelGroupIds;
      if (!channelGroupsByMediaType.TryGetValue(channelGroupMediaType, out channelGroupIds))
      {
        channelGroupIds = new Dictionary<string, int>(20);
        channelGroupsByMediaType.Add(channelGroupMediaType, channelGroupIds);
      }
      int channelGroupId;
      if (!channelGroupIds.TryGetValue(channelGroupName, out channelGroupId))
      {
        channelGroupId = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(channelGroupName, channelGroupMediaType).IdGroup;
        channelGroupIds.Add(channelGroupName, channelGroupId);
      }
      return new GroupMap
      {
        IdGroup = channelGroupId,
        IdChannel = channelId
      };
    }

    public static Channel AddChannel(IChannel channel)
    {
      Channel dbChannel = new Channel();
      dbChannel.Name = channel.Name;
      dbChannel.ChannelNumber = channel.LogicalChannelNumber;
      dbChannel.MediaType = (int)channel.MediaType;
      dbChannel.VisibleInGuide = true;
      dbChannel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
      dbChannel.AcceptChanges();
      ServiceAgents.Instance.ChannelServiceAgent.AddTuningDetail(dbChannel.IdChannel, channel);
      return dbChannel;
    }

    public static void UpdateChannel(IChannel channel, DbTuningDetail dbTuningDetail)
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
          !string.Equals(channel.LogicalChannelNumber, "10000") &&
          (
          // ...the current channel LCN is invalid, or...
            string.Equals(dbChannel.ChannelNumber, "10000") ||
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
          ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
        }
      }

      ServiceAgents.Instance.ChannelServiceAgent.UpdateTuningDetail(dbTuningDetail.IdChannel, dbTuningDetail.IdTuning, channel);
    }
  }
}