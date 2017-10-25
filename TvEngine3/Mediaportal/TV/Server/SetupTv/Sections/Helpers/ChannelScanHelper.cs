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
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using MediaPortal.Common.Utils.ExtensionMethods;
using DbTuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;
using FileTuningDetail = Mediaportal.TV.Server.SetupTV.Sections.Helpers.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  internal class ChannelScanHelper
  {
    private class Counter
    {
      public IList<DbTuningDetail> New = new List<DbTuningDetail>();
      public IList<DbTuningDetail> Updated = new List<DbTuningDetail>();
      public IList<IChannel> Ignored = new List<IChannel>();
    }

    public delegate void NitScanFoundTransmittersDelegate(IList<FileTuningDetail> transmitters);
    public delegate void ScanCompletedDelegate();

    #region variables

    private int _tunerId;
    private bool _stopScanning = false;

    private readonly MPListView _listViewProgress = null;
    private readonly ProgressBar _progressBarProgress = null;

    private readonly NitScanFoundTransmittersDelegate _nitScanFoundTransmittersDelegate = null;
    private readonly ScanCompletedDelegate _scanCompletedDelegate = null;

    private readonly System.Timers.Timer _signalStatusUpdateTimer = new System.Timers.Timer();
    private readonly ProgressBar _progressBarSignalStrength = null;
    private readonly ProgressBar _progressBarSignalQuality = null;

    private Thread _scanThread = null;

    #endregion

    public ChannelScanHelper(int tunerId, MPListView listViewProgress, ProgressBar progressBarProgress, NitScanFoundTransmittersDelegate nitScanFoundTransmittersDelegate, ScanCompletedDelegate scanCompletedDelegate, ProgressBar progressBarSignalStrength, ProgressBar progressBarSignalQuality)
    {
      _tunerId = tunerId;
      _listViewProgress = listViewProgress;
      _progressBarProgress = progressBarProgress;
      _nitScanFoundTransmittersDelegate = nitScanFoundTransmittersDelegate;
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

    public bool StartScan(IList<FileTuningDetail> tuningDetails, ScanType scanType = ScanType.Standard)
    {
      if (tuningDetails == null || tuningDetails.Count == 0)
      {
        return false;
      }
      if (!ServiceAgents.Instance.TunerServiceAgent.GetTuner(_tunerId, TunerRelation.None).IsEnabled)
      {
        this.LogInfo("channel scan: tuner {0} disabled", _tunerId);
        MessageBox.Show("Tuner disabled. Please enable the tuner if you want to use it to scan.", SectionSettings.MESSAGE_CAPTION);
        return false;
      }
      if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(_tunerId))
      {
        this.LogInfo("channel scan: tuner {0} not present", _tunerId);
        MessageBox.Show("Tuner not found. Please ensure the tuner is connected, enabled, available and accessible.", SectionSettings.MESSAGE_CAPTION);
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
        starter = delegate { DoScan(tuningDetails, scanType); };
        threadName = "standard channel scanner";
      }
      _scanThread = new Thread(starter);
      _scanThread.Name = threadName;
      _scanThread.Start();
      return true;
    }

    public void StopScan()
    {
      _listViewProgress.Items.Add("stopping...");
      this.LogInfo("channel scan: tuner {0} stop scanning", _tunerId);
      _stopScanning = true;
    }

    private void DoScan(IList<FileTuningDetail> tuningDetails, ScanType scanType)
    {
      IDictionary<MediaType, Counter> overallCounters = new Dictionary<MediaType, Counter>(2);
      IDictionary<MediaType, IDictionary<string, int>> channelGroupsByMediaType = new Dictionary<MediaType, IDictionary<string, int>>(2);
      List<ChannelGroupChannelMapping> newChannelGroupMappings = new List<ChannelGroupChannelMapping>(tuningDetails.Count * 10);
      bool isFastNetworkScan = scanType == ScanType.FastNetworkInformation;
      bool isFastNetworkScanResult = false;
      HashSet<int> touchedTuningDetailIds = new HashSet<int>();
      IDictionary<int, HashSet<int>> transportStreamIds = new Dictionary<int, HashSet<int>>();   // ONID -> [TSIDs]

      try
      {
        TvResult result = ServiceAgents.Instance.ControllerServiceAgent.Scan(string.Format("{0} - TV Server Configuration tuner {1} scanner", System.Net.Dns.GetHostName(), _tunerId), _tunerId, tuningDetails[0].GetTuningChannel());
        if (result == TvResult.TunerLoadFailedSoftwareEncoderRequired)
        {
          this.LogError("channel scan: failed to scan, missing software encoder(s)");
          _listViewProgress.Invoke((MethodInvoker)delegate
          {
            MessageBox.Show("Please install software encoders for your tuner.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
          });
          return;
        }
        if (result == TvResult.TunerLoadFailed)
        {
          this.LogError("channel scan: failed to scan, tuner loading failed");
          _listViewProgress.Invoke((MethodInvoker)delegate
          {
            MessageBox.Show("Failed to load the tuner. Please report this error on our forum.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
          });
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
        IDictionary<int, int> satelliteIdsByLongitude = null;
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
            skipOffsetFrequencies = true;

            // Lazy loading for the satellite-IDs-by-longitude dictionary.
            if (satelliteIdsByLongitude == null && tuneChannel is IChannelSatellite)
            {
              IList<Satellite> satellites = ServiceAgents.Instance.TunerServiceAgent.ListAllSatellites();
              satelliteIdsByLongitude = new Dictionary<int, int>(satellites.Count);
              foreach (Satellite satellite in satellites)
              {
                satelliteIdsByLongitude[satellite.Longitude] = satellite.IdSatellite;
              }
            }

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

              IChannelDvbCompatible dvbCompatibleChannel = scannedChannel.Channel as IChannelDvbCompatible;
              if (dvbCompatibleChannel != null)
              {
                // Check if this is the target transport stream.
                if (transportStreamKey > 0)
                {
                  uint actualTsKey = ((uint)dvbCompatibleChannel.OriginalNetworkId << 16) | (ushort)dvbCompatibleChannel.TransportStreamId;
                  foundTargetTransportStream = transportStreamKey == actualTsKey;
                  tunedTransportStreams.Add(actualTsKey);
                  transportStreamKey = 0;
                }

                // Maintain a dictionary of original network and transport
                // stream identifiers.
                HashSet<int> originalNetworkTransportStreamIds;
                if (!transportStreamIds.TryGetValue(dvbCompatibleChannel.OriginalNetworkId, out originalNetworkTransportStreamIds))
                {
                  originalNetworkTransportStreamIds = new HashSet<int>();
                  transportStreamIds.Add(dvbCompatibleChannel.OriginalNetworkId, originalNetworkTransportStreamIds);
                }
                originalNetworkTransportStreamIds.Add(dvbCompatibleChannel.TransportStreamId);
              }
              else
              {
                IChannelMpeg2Ts mpeg2TsChannel = scannedChannel.Channel as IChannelMpeg2Ts;
                if (mpeg2TsChannel != null)
                {
                  HashSet<int> originalNetworkTransportStreamIds;
                  if (!transportStreamIds.TryGetValue(0, out originalNetworkTransportStreamIds))
                  {
                    originalNetworkTransportStreamIds = new HashSet<int>();
                    transportStreamIds.Add(0, originalNetworkTransportStreamIds);
                  }
                  originalNetworkTransportStreamIds.Add(dvbCompatibleChannel.TransportStreamId);
                }
              }

              // For a fast network scan, confirm whether the information
              // required to enable a fast scan was actually found.
              if (isFastNetworkScan && !isFastNetworkScanResult)
              {
                isFastNetworkScanResult = tuneChannel.IsDifferentTransmitter(scannedChannel.Channel);
              }

              // Skip encrypted channels if configured to do so.
              if (!storeEncryptedChannels && channel.IsEncrypted)
              {
                overallCounter.Ignored.Add(channel);
                transmitterCounter.Ignored.Add(channel);
                continue;
              }

              // Find matching tuning details in the database.
              IList<DbTuningDetail> possibleTuningDetails = GetDbExistingTuningDetailCandidates(scannedChannel, useChannelMovementDetection, TuningDetailRelation.Channel, satelliteIdsByLongitude);
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
              if (dbTuningDetail != null)
              {
                UpdateChannel(channel, scannedChannel.IsVisibleInGuide, ref dbTuningDetail, satelliteIdsByLongitude);
                overallCounter.Updated.Add(dbTuningDetail);
                transmitterCounter.Updated.Add(dbTuningDetail);
              }
              else
              {
                dbTuningDetail = AddChannel(channel, scannedChannel.IsVisibleInGuide, satelliteIdsByLongitude);
                overallCounter.New.Add(dbTuningDetail);
                transmitterCounter.New.Add(dbTuningDetail);

                // Automatic channel grouping...
                ICollection<string> channelGroupNames = GetGroupNamesForChannel(channelGroupConfiguration, scannedChannel.Groups, groupNames);
                newChannelGroupMappings.AddRange(CreateChannelGroupMappingsForChannel(channelGroupNames, channel.MediaType, dbTuningDetail.IdChannel, channelGroupsByMediaType));
              }
              touchedTuningDetailIds.Add(dbTuningDetail.IdTuningDetail);
            }

            // Log and show progress/details.
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (KeyValuePair<MediaType, Counter> mediaTypeCounter in transmitterCounters)
            {
              line = string.Format("  {0} updated, count = {1}...", mediaTypeCounter.Key.GetDescription(), mediaTypeCounter.Value.Updated.Count);
              this.LogInfo(line);
              items.Add(new ListViewItem(line));
              foreach (DbTuningDetail td in mediaTypeCounter.Value.Updated)
              {
                this.LogDebug("    {0}", td.GetDescription());
                items.Add(new ListViewItem(string.Format("    {0}", td.Name)));
              }

              line = string.Format("  {0} new, count = {1}...", mediaTypeCounter.Key.GetDescription(), mediaTypeCounter.Value.New.Count);
              this.LogInfo(line);
              items.Add(new ListViewItem(line));
              foreach (DbTuningDetail td in mediaTypeCounter.Value.New)
              {
                this.LogDebug("    {0}", td.GetDescription());
                items.Add(new ListViewItem(string.Format("    {0}", td.Name)));
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
          ListViewItem item = _listViewProgress.Items.Add(new ListViewItem("Unexpected error. Please report this error on our forum."));
          item.ForeColor = Color.Red;
          item.EnsureVisible();
          MessageBox.Show("Encountered unexpected error. Please report this error on our forum.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
        });
      }
      finally
      {
        if (newChannelGroupMappings.Count > 0)
        {
          string line = string.Format("create {0} channel group mapping(s)...", newChannelGroupMappings.Count);
          this.LogInfo("channel scan: {0}", line);
          ServiceAgents.Instance.ChannelServiceAgent.SaveChannelGroupMappings(newChannelGroupMappings);
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

          if (touchedTuningDetailIds.Count > 0)
          {
            if (scanType == ScanType.FastNetworkInformation && !isFastNetworkScanResult)
            {
              scanType = ScanType.Standard;
            }
            DeleteDiscontinuedTuningDetails(_tunerId, tuningDetails, scanType, touchedTuningDetailIds, transportStreamIds);
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
      ScannedTransmitter[] transmitters = null;

      try
      {
        IChannel tuneChannel = tuningDetail.GetTuningChannel();
        TvResult result = ServiceAgents.Instance.ControllerServiceAgent.Scan(string.Format("{0} - TV Server Configuration tuner {1} NIT scanner", System.Net.Dns.GetHostName(), _tunerId), _tunerId, tuneChannel);
        if (result == TvResult.TunerLoadFailed)
        {
          this.LogError("channel scan: failed to scan, tuner loading failed");
          _listViewProgress.Invoke((MethodInvoker)delegate
          {
            MessageBox.Show("Failed to load the tuner. Please report this error on our forum.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
          });
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

        transmitters = ServiceAgents.Instance.ControllerServiceAgent.ScanNIT(_tunerId, tuneChannel);
        if (transmitters == null || transmitters.Length == 0)
        {
          transmitters = null;
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

        line = string.Format("  transmitters, count = {0}...", transmitters.Length);
        this.LogInfo(line);
        IList<FileTuningDetail> allTuningDetails = new List<FileTuningDetail>(transmitters.Length);
        IList<FileTuningDetail> tunableTuningDetails = new List<FileTuningDetail>(transmitters.Length);
        _listViewProgress.Invoke((MethodInvoker)delegate
        {
          _listViewProgress.BeginUpdate();
          try
          {
            _listViewProgress.Items.Add(new ListViewItem(line));
            foreach (ScannedTransmitter transmitter in transmitters)
            {
              FileTuningDetail td = new FileTuningDetail
              {
                Bandwidth = transmitter.Bandwidth,
                BroadcastStandard = transmitter.BroadcastStandard,
                FecCodeRate = transmitter.FecCodeRate,
                Longitude = transmitter.Longitude,
                OriginalNetworkId = transmitter.OriginalNetworkId,
                Polarisation = transmitter.Polarisation,
                RollOffFactor = transmitter.RollOffFactor,
                StreamId = transmitter.StreamId,
                SymbolRate = transmitter.SymbolRate,
                TransportStreamId = transmitter.TransportStreamId
              };
              if (transmitter.ModulationSchemePsk != ModulationSchemePsk.Automatic)
              {
                td.ModulationScheme = transmitter.ModulationSchemePsk.ToString();
              }
              else
              {
                td.ModulationScheme = transmitter.ModulationSchemeQam.ToString();
              }
              if (transmitter.Frequencies.Count == 1)
              {
                td.Frequency = transmitter.Frequencies[0];
              }
              else
              {
                td.Frequencies = new List<int>(transmitter.Frequencies);
              }

              line = string.Format("    {0}", td);
              this.LogDebug(line);
              _listViewProgress.Items.Add(new ListViewItem(line));

              allTuningDetails.Add(td);
              if (ServiceAgents.Instance.ControllerServiceAgent.CanTune(_tunerId, td.GetTuningChannel()))
              {
                tunableTuningDetails.Add(td);
              }
              else
              {
                line = string.Format("      not tunable");
                this.LogDebug(line);
                _listViewProgress.Items.Add(new ListViewItem(line));
              }
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
          _nitScanFoundTransmittersDelegate(allTuningDetails);
        }

        if (tunableTuningDetails.Count == 0)
        {
          transmitters = null;
          return;
        }
        FileTuningDetail[] tuningDetailArray = new FileTuningDetail[tunableTuningDetails.Count];
        tunableTuningDetails.CopyTo(tuningDetailArray, 0);
        DoScan(tuningDetailArray, ScanType.FullNetworkInformationTable);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "channel scan: unexpected NIT scanning exception");
        transmitters = null;
        _listViewProgress.Invoke((MethodInvoker)delegate
        {
          ListViewItem item = _listViewProgress.Items.Add(new ListViewItem("Unexpected error. Please report this error on our forum."));
          item.ForeColor = Color.Red;
          item.EnsureVisible();
          MessageBox.Show("Encountered unexpected error. Please report this error on our forum.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
        });
      }
      finally
      {
        if (transmitters == null)
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
              if (ulong.TryParse(configuredGroupIdString, out configuredGroupId) && !groupIds.Contains(configuredGroupId))
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

    private IList<ChannelGroupChannelMapping> CreateChannelGroupMappingsForChannel(ICollection<string> groupNames, MediaType channelMediaType, int channelId, IDictionary<MediaType, IDictionary<string, int>> channelGroupsByMediaType)
    {
      IList<ChannelGroupChannelMapping> mappings = new List<ChannelGroupChannelMapping>(groupNames.Count);

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
          channelGroupId = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateChannelGroup(groupName, channelMediaType).IdChannelGroup;
          channelGroupIds.Add(groupName, channelGroupId);
        }

        mappings.Add(new ChannelGroupChannelMapping
        {
          IdChannelGroup = channelGroupId,
          IdChannel = channelId
        });
      }
      return mappings;
    }

    private Dictionary<ChannelGroupType, string[]> ReadAutomaticChannelGroupConfig()
    {
      Dictionary<ChannelGroupType, string[]> channelGroupConfiguration = new Dictionary<ChannelGroupType, string[]>(30);
      ChannelGroupType channelGroupTypes = ChannelGroupType.CyfrowyPolsatChannelCategory | ChannelGroupType.FreesatChannelCategory | ChannelGroupType.MediaHighwayChannelCategory | ChannelGroupType.NorDigChannelList | ChannelGroupType.OpenTvChannelCategory | ChannelGroupType.VirginMediaChannelCategory;
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

    public static DbTuningDetail AddChannel(IChannel channel, bool isVisibleInGuide, IDictionary<int, int> satelliteIdsByLongitude = null)
    {
      Channel dbChannel = new Channel();
      dbChannel.Name = channel.Name;
      dbChannel.ChannelNumber = channel.LogicalChannelNumber;
      dbChannel.MediaType = (int)channel.MediaType;
      dbChannel.VisibleInGuide = isVisibleInGuide;
      dbChannel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
      dbChannel.AcceptChanges();

      DbTuningDetail tuningDetail = new DbTuningDetail();
      tuningDetail.IdChannel = dbChannel.IdChannel;
      tuningDetail.GrabEpg = channel.GrabEpg;   // assign on creation; user-controlled after that
      UpdateTuningDetail(channel, ref tuningDetail, satelliteIdsByLongitude);
      return tuningDetail;
    }

    public static void UpdateChannel(IChannel channel, bool isVisibleInGuide, ref DbTuningDetail dbTuningDetail, IDictionary<int, int> satelliteIdsByLongitude = null)
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

      UpdateTuningDetail(channel, ref dbTuningDetail, satelliteIdsByLongitude);
    }

    private static void UpdateTuningDetail(IChannel channel, ref DbTuningDetail tuningDetail, IDictionary<int, int> satelliteIdsByLongitude)
    {
      tuningDetail.Name = channel.Name;
      tuningDetail.Provider = channel.Provider;
      tuningDetail.LogicalChannelNumber = channel.LogicalChannelNumber;
      tuningDetail.MediaType = (int)channel.MediaType;
      //tuningDetail.GrabEpg = channel.GrabEpg;     only assign on creation; user-controlled after that
      tuningDetail.IsEncrypted = channel.IsEncrypted;
      tuningDetail.IsHighDefinition = channel.IsHighDefinition;
      tuningDetail.IsThreeDimensional = channel.IsThreeDimensional;

      IChannelPhysical physicalChannel = channel as IChannelPhysical;
      if (physicalChannel != null)
      {
        tuningDetail.Frequency = physicalChannel.Frequency;

        IChannelOfdm ofdmChannel = channel as IChannelOfdm;
        if (ofdmChannel != null)
        {
          tuningDetail.Bandwidth = ofdmChannel.Bandwidth;
        }
        else
        {
          IChannelQam qamChannel = channel as IChannelQam;
          if (qamChannel != null)
          {
            tuningDetail.Modulation = (int)qamChannel.ModulationScheme;
            tuningDetail.SymbolRate = qamChannel.SymbolRate;
          }
          else
          {
            IChannelSatellite satelliteChannel = channel as IChannelSatellite;
            if (satelliteChannel != null)
            {
              int satelliteId;
              if (satelliteIdsByLongitude == null || !satelliteIdsByLongitude.TryGetValue(satelliteChannel.Longitude, out satelliteId))
              {
                Log.Error("channel scan: failed to determine ID for satellite at longitude {0}", satelliteChannel.Longitude);
                return;
              }
              tuningDetail.IdSatellite = satelliteId;
              tuningDetail.Polarisation = (int)satelliteChannel.Polarisation;
              tuningDetail.Modulation = (int)satelliteChannel.ModulationScheme;
              tuningDetail.SymbolRate = satelliteChannel.SymbolRate;
              tuningDetail.FecCodeRate = (int)satelliteChannel.FecCodeRate;
            }
          }
        }
      }

      IChannelMpeg2Ts mpeg2TsChannel = channel as IChannelMpeg2Ts;
      if (mpeg2TsChannel != null)
      {
        tuningDetail.PmtPid = mpeg2TsChannel.PmtPid;
        tuningDetail.ServiceId = mpeg2TsChannel.ProgramNumber;
        tuningDetail.TransportStreamId = mpeg2TsChannel.TransportStreamId;

        IChannelDvbCompatible dvbCompatibleChannel = channel as IChannelDvbCompatible;
        if (dvbCompatibleChannel != null)
        {
          tuningDetail.OriginalNetworkId = dvbCompatibleChannel.OriginalNetworkId;
          tuningDetail.EpgOriginalNetworkId = dvbCompatibleChannel.EpgOriginalNetworkId;
          tuningDetail.EpgTransportStreamId = dvbCompatibleChannel.EpgTransportStreamId;
          tuningDetail.EpgServiceId = dvbCompatibleChannel.EpgServiceId;

          IChannelFreesat freesatChannel = channel as IChannelFreesat;
          if (freesatChannel != null)
          {
            tuningDetail.FreesatChannelId = freesatChannel.FreesatChannelId;
          }
          IChannelOpenTv openTvChannel = channel as IChannelOpenTv;
          if (openTvChannel != null)
          {
            tuningDetail.OpenTvChannelId = openTvChannel.OpenTvChannelId;
          }
        }
      }

      if (channel is ChannelAmRadio)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.AmRadio;
      }
      ChannelAnalogTv analogTvChannel = channel as ChannelAnalogTv;
      if (analogTvChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.AnalogTelevision;
        tuningDetail.PhysicalChannelNumber = analogTvChannel.PhysicalChannelNumber;
        tuningDetail.CountryId = analogTvChannel.Country.Id;
        tuningDetail.TuningSource = (int)analogTvChannel.TunerSource;
      }
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (atscChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.Atsc;
        tuningDetail.Modulation = (int)atscChannel.ModulationScheme;
        tuningDetail.SourceId = atscChannel.SourceId;
      }
      ChannelCapture captureChannel = channel as ChannelCapture;
      if (captureChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.ExternalInput;
        tuningDetail.VideoSource = (int)captureChannel.VideoSource;
        tuningDetail.AudioSource = (int)captureChannel.AudioSource;
        tuningDetail.IsVcrSignal = captureChannel.IsVcrSignal;
      }
      if (channel is ChannelDigiCipher2)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DigiCipher2;
      }
      if (channel is ChannelDvbC)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbC;
      }
      ChannelDvbC2 dvbc2Channel = channel as ChannelDvbC2;
      if (dvbc2Channel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbC2;
        tuningDetail.StreamId = dvbc2Channel.PlpId;
      }
      ChannelDvbDsng dvbDsngChannel = channel as ChannelDvbDsng;
      if (dvbDsngChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbDsng;
        tuningDetail.RollOffFactor = (int)dvbDsngChannel.RollOffFactor;
      }
      if (channel is ChannelDvbS)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbS;
      }
      ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
      if (dvbs2Channel != null)
      {
        tuningDetail.BroadcastStandard = (int)dvbs2Channel.BroadcastStandard;
        tuningDetail.RollOffFactor = (int)dvbs2Channel.RollOffFactor;
        tuningDetail.PilotTonesState = (int)dvbs2Channel.PilotTonesState;
        tuningDetail.StreamId = dvbs2Channel.StreamId;
      }
      if (channel is ChannelDvbT)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbT;
      }
      ChannelDvbT2 dvbt2Channel = channel as ChannelDvbT2;
      if (dvbt2Channel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbT2;
        tuningDetail.StreamId = dvbt2Channel.PlpId;
      }
      if (channel is ChannelFmRadio)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.FmRadio;
      }
      if (channel is ChannelIsdbC)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.IsdbC;
      }
      if (channel is ChannelIsdbS)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.IsdbS;
      }
      if (channel is ChannelIsdbT)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.IsdbT;
      }
      if (channel is ChannelSatelliteTurboFec)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.SatelliteTurboFec;
      }
      ChannelScte scteChannel = channel as ChannelScte;
      if (scteChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.Scte;
        tuningDetail.SourceId = scteChannel.SourceId;
      }
      ChannelStream streamChannel = channel as ChannelStream;
      if (streamChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbIp;
        tuningDetail.Url = streamChannel.Url;
      }

      tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);
      tuningDetail.AcceptChanges();
    }

    public static void DeleteDiscontinuedTuningDetails(int tunerId, IList<FileTuningDetail> tuningDetails, ScanType scanType, HashSet<int> touchedTuningDetailIds, IDictionary<int, HashSet<int>> transportStreamIds)
    {
      // Which tuning details did we expect to find/update?
      bool isNetworkScan = scanType == ScanType.FastNetworkInformation || scanType == ScanType.FullNetworkInformationTable;
      IList<DbTuningDetail> expectedTuningDetails = null;
      bool isExpectedSetAccurate = false;
      if (tuningDetails == null)
      {
        // Special case: external tuner channel list import.
        isExpectedSetAccurate = true;
        expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetExternalTunerTuningDetails(TuningDetailRelation.None);
      }
      else if (isNetworkScan)
      {
        // This is tricky. In an ideal world we'd know the target NID(s) and be
        // able to query the database for the associated tuning details. In
        // reality neither of those things are known/possible. The best we can
        // do is to use the set of tuning details that match any of the
        // ONID/TSID combinations referenced in the scan result. Assumptions
        // that may fail:
        // 1. All services in a given transport stream will be associated with
        //    the same networks.
        // 2. Transport streams are never removed from a network.
        expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.ListAllTuningDetailsByOriginalNetworkIds(transportStreamIds.Keys, TuningDetailRelation.None);
      }
      else if (tuningDetails.Count == 1)
      {
        // Identifing the tuning details associated with the transmitter that
        // was scanned is also tricky.
        FileTuningDetail tuningDetail = tuningDetails[0];
        IChannel channel = tuningDetail.GetTuningChannel();
        ChannelScte scteChannel = channel as ChannelScte;
        if (scteChannel != null && scteChannel.IsOutOfBandScanChannel())
        {
          // CableCARD OOB scanning is exceptional. Very similar to a fast
          // network scan.
          expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.ListAllTuningDetailsByBroadcastStandard(BroadcastStandard.AnalogTelevision | BroadcastStandard.Scte, TuningDetailRelation.None);
        }
        else
        {
          // Specific handling by transmitter type...
          switch (tuningDetail.BroadcastStandard)
          {
            case BroadcastStandard.AmRadio:
              expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetAmRadioTuningDetails(tuningDetail.Frequency, TuningDetailRelation.None);
              break;
            case BroadcastStandard.AnalogTelevision:
              expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetAnalogTelevisionTuningDetails(tuningDetail.PhysicalChannelNumber, TuningDetailRelation.None);
              break;
            case BroadcastStandard.ExternalInput:
              expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetCaptureTuningDetails(tunerId, TuningDetailRelation.None);
              break;
            case BroadcastStandard.FmRadio:
              expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetFmRadioTuningDetails(tuningDetail.Frequency, TuningDetailRelation.None);
              break;
            case BroadcastStandard.DvbIp:
              expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetStreamTuningDetails(tuningDetail.Url, TuningDetailRelation.None);
              break;
            default:
              // Handle all transmissions based on MPEG 2 TS. We expect a
              // single ONID + TSID combination in the transport stream IDs
              // dictionary. That combination identifies the tuning details
              // associated with the transmitter.
              if ((channel is IChannelDvbCompatible || channel is IChannelMpeg2Ts) && transportStreamIds.Count == 1)
              {
                bool isDvbCompatible = channel is IChannelDvbCompatible;
                IEnumerator<int> e = transportStreamIds.Keys.GetEnumerator();
                e.MoveNext();
                int originalNetworkId = e.Current;
                HashSet<int> originalNetworkTransportStreamIds = transportStreamIds[originalNetworkId];
                if (originalNetworkTransportStreamIds.Count == 1)
                {
                  e = originalNetworkTransportStreamIds.GetEnumerator();
                  e.MoveNext();
                  int transportStreamId = e.Current;
                  int? satelliteId = null;
                  int? frequency = null;
                  if (BroadcastStandard.MaskSatellite.HasFlag(tuningDetail.BroadcastStandard))
                  {
                    Satellite satellite = ServiceAgents.Instance.TunerServiceAgent.GetSatelliteByLongitude(tuningDetail.Longitude);
                    if (satellite != null)
                    {
                      satelliteId = satellite.IdSatellite;
                    }

                    if (IsFeed(originalNetworkId, transportStreamId))
                    {
                      frequency = tuningDetail.Frequency;
                    }
                  }
                  if (isDvbCompatible)
                  {
                    expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(tuningDetail.BroadcastStandard, originalNetworkId, TuningDetailRelation.Satellite, null, transportStreamId, frequency, satelliteId);
                  }
                  else
                  {
                    expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetMpeg2TuningDetails(tuningDetail.BroadcastStandard, TuningDetailRelation.Satellite, null, transportStreamId, frequency, satelliteId);
                  }
                }
              }
              break;
          }
        }

        if (expectedTuningDetails != null)
        {
          isExpectedSetAccurate = true;
        }
      }

      BroadcastStandard broadcastStandards = BroadcastStandard.Unknown;
      if (expectedTuningDetails == null)
      {
        // If we get to here, the tuner scanned all cable, satellite or
        // terrestrial transmitters that it was able to tune for a given
        // provider, satellite or region.
        HashSet<int> longitudes = new HashSet<int>();
        int longitude = 0;
        foreach (FileTuningDetail tuningDetail in tuningDetails)
        {
          broadcastStandards |= tuningDetail.BroadcastStandard;
          if (BroadcastStandard.MaskSatellite.HasFlag(tuningDetail.BroadcastStandard))
          {
            longitudes.Add(tuningDetail.Longitude);
            longitude = tuningDetail.Longitude;
          }
        }

        // Limit to a single satellite if appropriate.
        int? satelliteId = null;
        if (longitudes.Count == 1)
        {
          Satellite satellite = ServiceAgents.Instance.TunerServiceAgent.GetSatelliteByLongitude(longitude);
          if (satellite != null)
          {
            satelliteId = satellite.IdSatellite;
          }
        }

        // Handling for families of standards...
        if (tuningDetails.Count != 1)
        {
          if (broadcastStandards.HasFlag(BroadcastStandard.DvbC) || broadcastStandards.HasFlag(BroadcastStandard.DvbC2))
          {
            broadcastStandards |= BroadcastStandard.DvbC | BroadcastStandard.DvbC2;
          }
          if (BroadcastStandard.MaskSatellite.HasFlag(broadcastStandards))
          {
            broadcastStandards |= BroadcastStandard.MaskSatellite;
          }
          if (broadcastStandards.HasFlag(BroadcastStandard.DvbT) || broadcastStandards.HasFlag(BroadcastStandard.DvbT2))
          {
            broadcastStandards |= BroadcastStandard.DvbT | BroadcastStandard.DvbT2;
          }
        }

        expectedTuningDetails = ServiceAgents.Instance.ChannelServiceAgent.ListAllTuningDetailsByBroadcastStandard(broadcastStandards, TuningDetailRelation.Satellite, satelliteId);
      }

      IList<DbTuningDetail> deletionCandidates = null;
      if (isExpectedSetAccurate)
      {
        deletionCandidates = expectedTuningDetails;
      }
      else
      {
        deletionCandidates = new List<DbTuningDetail>(expectedTuningDetails.Count);
        foreach (DbTuningDetail td in expectedTuningDetails)
        {
          if (touchedTuningDetailIds.Contains(td.IdTuningDetail))
          {
            continue;
          }

          if (
            (isNetworkScan && transportStreamIds[td.OriginalNetworkId].Contains(td.TransportStreamId)) ||
            (!isNetworkScan && tuningDetails.Count == 1 && ServiceAgents.Instance.ControllerServiceAgent.CanTune(tunerId, TuningDetailManagement.GetTuningChannel(td))) ||
            (!isNetworkScan && tuningDetails.Count != 1 && !TuningDetailManagement.GetTuningChannel(td).IsDifferentTransmitter(tuningDetails[0].GetTuningChannel()))
          )
          {
            deletionCandidates.Add(td);
          }
        }
      }

      Log.Info("channel scan: database tuning detail counts, expected = {0}, found = {1}, deletion candidates = {2}", expectedTuningDetails.Count, touchedTuningDetailIds.Count, deletionCandidates.Count);
      if (deletionCandidates.Count == 0)
      {
        return;
      }

      foreach (DbTuningDetail tuningDetail in deletionCandidates)
      {
        Log.Debug("  {0}", tuningDetail.GetDescription());
      }

      DbTuningDetail[] deletionCandidatesArray = new DbTuningDetail[deletionCandidates.Count];
      deletionCandidates.CopyTo(deletionCandidatesArray, 0);
      List<int> tuningDetailIds = new List<int>(deletionCandidates.Count);
      HashSet<int> channelIds = new HashSet<int>();
      using (FormSelectItems dlgSelect = new FormSelectItems("Delete Discontinued Channel(s)/Tuning Detail(s)", "Please select the tuning details to delete:", deletionCandidatesArray, "Name"))
      {
        if (dlgSelect.ShowDialog() != DialogResult.OK || dlgSelect.Items == null || dlgSelect.Items.Count == 0)
        {
          return;
        }

        foreach (DbTuningDetail tuningDetail in dlgSelect.Items)
        {
          tuningDetailIds.Add(tuningDetail.IdTuningDetail);
          channelIds.Add(tuningDetail.IdChannel);
        }
      }

      NotifyForm dlgNotify = null;
      try
      {
        if (deletionCandidates.Count > 10)
        {
          dlgNotify = new NotifyForm("Deleting selected tuning details...", "This can take some time." + Environment.NewLine + Environment.NewLine + "Please be patient...");
          dlgNotify.Show();
          dlgNotify.WaitForDisplay();
        }

        Log.Info("channel scan: deleting, tuning detail IDs = [{0}], channel IDs = [{1}]", tuningDetailIds, channelIds);
        foreach (int tuningDetailId in tuningDetailIds)
        {
          ServiceAgents.Instance.ChannelServiceAgent.DeleteTuningDetail(tuningDetailId);
        }
        ServiceAgents.Instance.ChannelServiceAgent.DeleteOrphanedChannels(channelIds);
      }
      finally
      {
        if (dlgNotify != null)
        {
          dlgNotify.Close();
          dlgNotify.Dispose();
        }
      }
    }

    private static bool IsFeed(int? originalNetworkId, int transportStreamId, int? serviceId = null)
    {
      if (
        (!originalNetworkId.HasValue || originalNetworkId.Value < 3 || originalNetworkId.Value > ushort.MaxValue - 3) &&
        (transportStreamId < 3 || transportStreamId > ushort.MaxValue - 3) &&
        (!serviceId.HasValue || serviceId.Value < 3 || serviceId.Value > ushort.MaxValue - 3)
      )
      {
        return true;
      }
      return false;
    }

    public static IList<DbTuningDetail> GetDbExistingTuningDetailCandidates(ScannedChannel foundChannel, bool useChannelMovementDetection, TuningDetailRelation includeRelations, IDictionary<int, int> satelliteIdsByLongitude = null)
    {
      ChannelCapture captureChannel = foundChannel.Channel as ChannelCapture;
      if (captureChannel != null)
      {
        return ServiceAgents.Instance.ChannelServiceAgent.GetCaptureTuningDetails(foundChannel.Channel.Name, includeRelations);
      }
      ChannelFmRadio fmRadioChannel = foundChannel.Channel as ChannelFmRadio;
      if (fmRadioChannel != null)
      {
        return ServiceAgents.Instance.ChannelServiceAgent.GetFmRadioTuningDetails(fmRadioChannel.Frequency, includeRelations);
      }
      ChannelAnalogTv analogTvChannel = foundChannel.Channel as ChannelAnalogTv;
      if (analogTvChannel != null)
      {
        return ServiceAgents.Instance.ChannelServiceAgent.GetAnalogTelevisionTuningDetails(analogTvChannel.PhysicalChannelNumber, includeRelations);
      }
      ChannelAmRadio amRadioChannel = foundChannel.Channel as ChannelAmRadio;
      if (amRadioChannel != null)
      {
        return ServiceAgents.Instance.ChannelServiceAgent.GetAmRadioTuningDetails(amRadioChannel.Frequency, includeRelations);
      }

      // ATSC and SCTE are a bit awkward. Logical channel number + frequency
      // (in case the user can receive out-of-region transmissions) seems to be
      // the most reliable lookup key.
      // - MPEG 2 TS ID + program number is not unique across ATSC
      //    transmitters. Broadcasters don't seem to co-ordinate TSIDs, and
      //    often use the same low value program numbers (1, 2, 3 etc.).
      // - Source ID is not unique across ATSC transmitters, and may
      //    legitimately refer to two channels on cable.
      ChannelAtsc atscChannel = foundChannel.Channel as ChannelAtsc;
      if (atscChannel != null)
      {
        return ServiceAgents.Instance.ChannelServiceAgent.GetAtscScteTuningDetails(BroadcastStandard.Atsc, atscChannel.LogicalChannelNumber, includeRelations, atscChannel.Frequency);
      }
      ChannelScte scteChannel = foundChannel.Channel as ChannelScte;
      if (scteChannel != null)
      {
        if (scteChannel.IsOutOfBandScanChannel())
        {
          return ServiceAgents.Instance.ChannelServiceAgent.GetAtscScteTuningDetails(BroadcastStandard.Scte, scteChannel.LogicalChannelNumber, includeRelations);
        }
        return ServiceAgents.Instance.ChannelServiceAgent.GetAtscScteTuningDetails(BroadcastStandard.Scte, scteChannel.LogicalChannelNumber, includeRelations, scteChannel.Frequency);
      }

      BroadcastStandard broadcastStandardSearchMask = BroadcastStandard.Unknown;
      IList<DbTuningDetail> tuningDetails = null;

      // Most streams are single program transport streams (SPTSs), so we can
      // simply lookup the tuning detail by URL. Otherwise use DVB-compatible
      // logic.
      ChannelStream streamChannel = foundChannel.Channel as ChannelStream;
      if (streamChannel != null)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetStreamTuningDetails(streamChannel.Url, includeRelations);
        if (tuningDetails != null && tuningDetails.Count == 1)
        {
          return tuningDetails;
        }
        broadcastStandardSearchMask = BroadcastStandard.DvbIp;
      }

      if (foundChannel.Channel is IChannelQam)
      {
        broadcastStandardSearchMask = BroadcastStandard.DvbC | BroadcastStandard.IsdbC;
      }
      else if (foundChannel.Channel is IChannelOfdm)
      {
        broadcastStandardSearchMask = BroadcastStandard.DvbT | BroadcastStandard.DvbT2 | BroadcastStandard.IsdbT;
      }
      else if (foundChannel.Channel is IChannelSatellite)
      {
        broadcastStandardSearchMask = BroadcastStandard.MaskSatellite;
      }

      // Freesat channel movement detection is always active. Each channel has
      // a unique identifier.
      IChannelFreesat freesatChannel = foundChannel.Channel as IChannelFreesat;
      if (freesatChannel != null && freesatChannel.FreesatChannelId > 0)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetFreesatTuningDetails(freesatChannel.FreesatChannelId, includeRelations);
        if (tuningDetails != null && tuningDetails.Count > 0)
        {
          return tuningDetails;
        }
      }

      // OpenTV channel movement detection is always active. Each channel has a
      // unique identifier.
      IChannelOpenTv openTvChannel = foundChannel.Channel as IChannelOpenTv;
      if (openTvChannel != null && openTvChannel.OpenTvChannelId > 0)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetOpenTvTuningDetails(broadcastStandardSearchMask & BroadcastStandard.MaskOpenTvSi, openTvChannel.OpenTvChannelId, includeRelations);
        if (tuningDetails != null && tuningDetails.Count > 0)
        {
          return tuningDetails;
        }
      }

      // Where applicable, the remaining channel movement detection methods are
      // confined to the satellite that is broadcasting the channel.
      int? satelliteId = null;
      IChannelSatellite satelliteChannel = foundChannel.Channel as IChannelSatellite;
      if (satelliteChannel != null)
      {
        int tempId;
        if (satelliteIdsByLongitude.TryGetValue(satelliteChannel.Longitude, out tempId))
        {
          satelliteId = tempId;
        }
      }

      // If previous DVB service identifiers are available then assume the
      // service has moved recently and use the identifiers to locate the
      // tuning detail.
      if (foundChannel.PreviousOriginalNetworkId > 0)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, foundChannel.PreviousOriginalNetworkId, includeRelations, foundChannel.PreviousServiceId, foundChannel.PreviousTransportStreamId, null, satelliteId);
        if (tuningDetails != null && tuningDetails.Count > 0)
        {
          return tuningDetails;
        }
      }

      // According to DVB specifications ONID + SID should be a sufficient
      // service identifier. The specification also recommends that the SID
      // shouldn't change if a service moves. This theoretically allows us to
      // track channel movements. In practice this works most of the time for
      // co-ordinated networks. Unfortunately satellite networks are often not
      // co-ordinated. Even ONID + TSID + SID may not be unique.
      IChannelMpeg2Ts mpeg2TsChannel = foundChannel.Channel as IChannelMpeg2Ts;
      if (mpeg2TsChannel == null)
      {
        return null;
      }

      int? originalNetworkId = null;
      IChannelDvbCompatible dvbCompatibleChannel = foundChannel.Channel as IChannelDvbCompatible;
      if (dvbCompatibleChannel != null)
      {
        originalNetworkId = dvbCompatibleChannel.OriginalNetworkId;
      }

      int? frequency = null;
      if (satelliteChannel != null && IsFeed(originalNetworkId, mpeg2TsChannel.TransportStreamId, mpeg2TsChannel.ProgramNumber))
      {
        // Feeds, private transmissions etc. where even ONID + TSID + SID is
        // not necessarily unique.
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
          tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetMpeg2TuningDetails(broadcastStandardSearchMask, includeRelations, mpeg2TsChannel.ProgramNumber, null, frequency, satelliteId);
          if (tuningDetails == null || tuningDetails.Count < 2)
          {
            return tuningDetails;
          }
        }
        return ServiceAgents.Instance.ChannelServiceAgent.GetMpeg2TuningDetails(broadcastStandardSearchMask, includeRelations, mpeg2TsChannel.ProgramNumber, mpeg2TsChannel.TransportStreamId, frequency, satelliteId);
      }

      if (useChannelMovementDetection)
      {
        tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, originalNetworkId.Value, includeRelations, mpeg2TsChannel.ProgramNumber, null, frequency);
        if (tuningDetails == null || tuningDetails.Count < 2)
        {
          return tuningDetails;
        }
      }
      return ServiceAgents.Instance.ChannelServiceAgent.GetDvbTuningDetails(broadcastStandardSearchMask, originalNetworkId.Value, includeRelations, mpeg2TsChannel.ProgramNumber, mpeg2TsChannel.TransportStreamId, frequency);
    }
  }
}