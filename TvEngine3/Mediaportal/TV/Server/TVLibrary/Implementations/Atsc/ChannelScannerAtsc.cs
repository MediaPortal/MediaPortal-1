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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using MediaPortal.Common.Utils.ExtensionMethods;
using Polarisation = Mediaportal.TV.Server.TVLibrary.Implementations.Atsc.Enum.Polarisation;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Atsc
{
  /// <summary>
  /// An implementation of <see cref="IChannelScanner"/> for ATSC-compliant
  /// and SCTE-compliant transport streams.
  /// </summary>
  internal class ChannelScannerAtsc : IChannelScannerInternal, ICallBackGrabber
  {
    #region enums, constants and private classes

    [Flags]
    private enum TableType
    {
      None = 0,
      Pat = 0x01,
      Cat = 0x02,
      Pmt = 0x04,

      AtscNit = 0x0100,
      AtscNtt = 0x0200,
      AtscSvct = 0x0400,
      AtscMgt = 0x0800,
      AtscLvctTerrestrial = 0x1000,
      AtscLvctCable = 0x2000,
      AtscEam = 0x4000,

      ScteNit = 0x0100000,
      ScteNtt = 0x0200000,
      ScteSvct = 0x0400000,
      ScteMgt = 0x0800000,
      ScteLvctTerrestrial = 0x1000000,
      ScteLvctCable = 0x2000000,
      ScteEam = 0x4000000
    }

    private const int NAME_BUFFER_SIZE = 1000;

    private const byte COUNT_AUDIO_LANGUAGES = 15;
    private const byte COUNT_CAPTIONS_LANGUAGES = 15;
    private const byte COUNT_SUBTITLES_LANGUAGES = 15;

    private class ProgramInfo
    {
      public MediaType? MediaType;
      public ushort ProgramNumber;
      public ushort PmtPid;
      public bool IsEncrypted;
      public bool IsEncryptionDetectionAccurate;
      public bool IsThreeDimensional;
    }

    #endregion

    #region variables

    private bool _isScanning = false;

    // timing - unit = milli-seconds
    private int _minimumTime = 2000;
    private int _timeLimitSingleTransmitter = 15000;
    private int _timeLimitCableCard = 300000;

    private IGrabberSiMpeg _grabberMpeg = null;
    private IGrabberSiAtsc _grabberAtsc = null;
    private IGrabberSiScte _grabberScte = null;
    private TableType _seenTables = TableType.None;
    private TableType _completeTables = TableType.None;
    private ITuner _tuner = null;
    private AutoResetEvent _event = null;
    private volatile bool _cancelScan = false;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="ChannelScannerAtsc"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    /// <param name="grabberMpeg">The MPEG 2 transport stream analyser instance to use for scanning.</param>
    /// <param name="grabberAtsc">The ATSC stream analyser instance to use for scanning.</param>
    /// <param name="grabberScte">The SCTE stream analyser instance to use for scanning.</param>
    public ChannelScannerAtsc(ITuner tuner, IGrabberSiMpeg grabberMpeg, IGrabberSiAtsc grabberAtsc, IGrabberSiScte grabberScte)
    {
      _tuner = tuner;
      _grabberMpeg = grabberMpeg;
      _grabberAtsc = grabberAtsc;
      _grabberScte = grabberScte;
    }

    #endregion

    /// <summary>
    /// A delegate to invoke when a program specific information table section
    /// is received from the tuner's out-of-band channel.
    /// </summary>
    /// <param name="section">The PSI table section.</param>
    public void OnOutOfBandSectionReceived(byte[] section)
    {
      if (_grabberScte != null)
      {
        _grabberScte.OnOutOfBandSectionReceived(section, (ushort)section.Length);
      }
    }

    /// <summary>
    /// Create a scanned channel instance representing a switched-digital-video
    /// (SDV) channel.
    /// </summary>
    /// <param name="name">The channel's name.</param>
    /// <param name="number">The channel's logical/virtual channel number.</param>
    /// <param name="isVisibleInGuide"><c>True</c> if the channel should be visible in the programme guide.</param>
    /// <param name="groupNames">A dictionary of channel group names.</param>
    /// <returns>the created scanned channel instance</returns>
    public static ScannedChannel CreateSdvChannel(string name, string number, bool isVisibleInGuide, IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      ChannelScte newChannel = new ChannelScte();
      newChannel.Name = name;
      newChannel.LogicalChannelNumber = number;
      newChannel.Provider = "Cable";
      newChannel.MediaType = MediaType.Television;
      newChannel.IsEncrypted = true;
      newChannel.Frequency = 0;             // only CableCARD tuners will be able to tune this channel
      newChannel.ModulationScheme = ModulationSchemeQam.Automatic;
      newChannel.TransportStreamId = 0;     // doesn't really matter
      newChannel.SourceId = 0;              // ideally we should have this; EPG data will have to be sourced externally
      newChannel.ProgramNumber = 0;         // lookup the correct program number from the tuner when the channel is tuned
      newChannel.PmtPid = 0;                // lookup the correct PID from the PAT when the channel is tuned
      return CreateScannedChannel(newChannel, isVisibleInGuide, BroadcastStandard.Scte, groupNames);
    }

    /// <summary>
    /// Create a scanned channel instance representing a channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="isVisibleInGuide"><c>True</c> if the channel should be visible in the programme guide.</param>
    /// <param name="broadcastStandard">The standard used to broadcast the channel.</param>
    /// <param name="groupNames">A dictionary of channel group names.</param>
    /// <returns>the created scanned channel instance</returns>
    public static ScannedChannel CreateScannedChannel(IChannel channel, bool isVisibleInGuide, BroadcastStandard broadcastStandard, IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      ScannedChannel scannedChannel = new ScannedChannel(channel);
      scannedChannel.IsVisibleInGuide = isVisibleInGuide;

      // Constructed/derived groups.
      if (broadcastStandard != BroadcastStandard.Unknown)
      {
        if (!groupNames[ChannelGroupType.BroadcastStandard].ContainsKey((ulong)broadcastStandard))
        {
          groupNames[ChannelGroupType.BroadcastStandard][(ulong)broadcastStandard] = broadcastStandard.GetDescription();
        }
        scannedChannel.Groups.Add(ChannelGroupType.BroadcastStandard, new List<ulong> { (ulong)broadcastStandard });
      }
      if (!string.IsNullOrEmpty(channel.Provider))
      {
        ulong hashCode = (ulong)channel.Provider.GetHashCode();
        groupNames[ChannelGroupType.ChannelProvider][hashCode] = channel.Provider;
        scannedChannel.Groups.Add(ChannelGroupType.ChannelProvider, new List<ulong> { hashCode });
      }
      return scannedChannel;
    }

    #region ICallBackGrabber members

    /// <summary>
    /// This function is invoked when the first section from a table is received.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that was received.</param>
    public virtual void OnTableSeen(ushort pid, byte tableId)
    {
      this.LogInfo("scan ATSC: on table seen, PID = {0}, table ID = {1}", pid, tableId);
      TableType tableType = GetTableType(pid, tableId);
      if (tableType != TableType.None)
      {
        _seenTables |= tableType;
        _event.Set();
      }
    }

    /// <summary>
    /// This function is invoked after the last section from a table is received.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that was completed.</param>
    public virtual void OnTableComplete(ushort pid, byte tableId)
    {
      this.LogInfo("scan ATSC: on table complete, PID = {0}, table ID = {1}", pid, tableId);
      TableType tableType = GetTableType(pid, tableId);
      if (tableType != TableType.None)
      {
        _completeTables |= tableType;
        _event.Set();
      }
    }

    /// <summary>
    /// This function is invoked after any section from a table changes.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that changed.</param>
    public virtual void OnTableChange(ushort pid, byte tableId)
    {
      this.LogDebug("scan ATSC: on table change, PID = {0}, table ID = {1}", pid, tableId);
      TableType tableType = GetTableType(pid, tableId);
      if (tableType != TableType.None)
      {
        _seenTables |= tableType;
        _completeTables &= ~tableType;
        _event.Set();
      }
    }

    #endregion

    #region IChannelScannerInternal member

    /// <summary>
    /// Set the scanner's tuner.
    /// </summary>
    public ITuner Tuner
    {
      set
      {
        _tuner = value;
      }
    }

    #endregion

    #region IChannelScanner members

    /// <summary>
    /// Reload the scanner's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      this.LogDebug("scan ATSC: reload configuration");

      _minimumTime = SettingsManagement.GetValue("minimumScanTime", 2000);
      _timeLimitSingleTransmitter = SettingsManagement.GetValue("timeLimitScanSingleTransmitter", 15000);
      _timeLimitCableCard = SettingsManagement.GetValue("timeLimitScanCableCard", 300000);
      this.LogDebug("  timing...");
      this.LogDebug("    minimum            = {0} ms", _minimumTime);
      this.LogDebug("    single transmitter = {0} ms", _timeLimitSingleTransmitter);
      this.LogDebug("    CableCARD          = {0} ms", _timeLimitCableCard);
    }

    /// <summary>
    /// Get the scanner's current status.
    /// </summary>
    /// <value><c>true</c> if the scanner is scanning, otherwise <c>false</c></value>
    public bool IsScanning
    {
      get
      {
        return _isScanning;
      }
    }

    /// <summary>
    /// Tune to a specified channel and scan for channel information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="isFastNetworkScan"><c>True</c> to do a fast network scan.</param>
    /// <param name="channels">The channel information found.</param>
    /// <param name="groupNames">The names of the groups referenced in <paramref name="channels"/>.</param>
    public void Scan(IChannel channel, bool isFastNetworkScan, out IList<ScannedChannel> channels, out IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      ISet<string> ignoredChannelNumbers;
      Scan(channel, isFastNetworkScan, out channels, out groupNames, out ignoredChannelNumbers);
    }

    /// <summary>
    /// Tune to a specified channel and scan for channel information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="isFastNetworkScan"><c>True</c> to do a fast network scan.</param>
    /// <param name="channels">The channel information found.</param>
    /// <param name="groupNames">The names of the groups referenced in <paramref name="channels"/>.</param>
    /// <param name="ignoredChannelNumbers">A set of the channel numbers that were ignored for whatever reason.</param>
    public void Scan(IChannel channel, bool isFastNetworkScan, out IList<ScannedChannel> channels, out IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames, out ISet<string> ignoredChannelNumbers)
    {
      channels = new List<ScannedChannel>(100);
      groupNames = new Dictionary<ChannelGroupType, IDictionary<ulong, string>>(50);
      ignoredChannelNumbers = new HashSet<string>();

      if (_grabberMpeg == null && _grabberAtsc == null && _grabberScte == null)
      {
        this.LogError("scan ATSC: grabber interfaces not available, not possible to scan");
        return;
      }

      try
      {
        _cancelScan = false;
        _isScanning = true;
        _event = new AutoResetEvent(false);
        _seenTables = TableType.None;
        _completeTables = TableType.None;
        if (_grabberMpeg != null)
        {
          _grabberMpeg.SetCallBack(this);
        }
        if (_grabberAtsc != null)
        {
          _grabberAtsc.SetCallBack(this);
        }
        if (_grabberScte != null)
        {
          _grabberScte.SetCallBack(this);
        }

        // An exception is thrown here if tuning fails for whatever reason.
        _tuner.Tune(0, channel);

        // Enforce minimum scan time.
        DateTime start = DateTime.Now;
        int remainingTime = _minimumTime;
        while (remainingTime > 0)
        {
          if (!_event.WaitOne(remainingTime))
          {
            break;
          }
          if (_cancelScan)
          {
            return;
          }
          remainingTime = _minimumTime - (int)(DateTime.Now - start).TotalMilliseconds;
        }

        if (_seenTables == TableType.None)
        {
          this.LogWarn("scan ATSC: tables not received, is the tuner delivering a stream?");
          return;
        }

        // Wait for scanning to complete.
        int timeLimit = _timeLimitSingleTransmitter;
        ChannelScte scteChannel = channel as ChannelScte;
        if (scteChannel != null && scteChannel.Frequency <= 0)
        {
          timeLimit = _timeLimitCableCard;
        }
        do
        {
          if (_cancelScan)
          {
            return;
          }

          // Check for scan completion.
          if (
            // Basic requirement: PAT and PMT must have been received, and all
            // seen tables must be complete.
            _completeTables.HasFlag(TableType.Pat | TableType.Pmt) &&
            _seenTables == _completeTables &&
            // Any one of the L-VCT tables must be complete, or the S-VCT, NIT
            // and NTT must be complete.
            (
              _completeTables.HasFlag(TableType.AtscLvctCable) ||
              _completeTables.HasFlag(TableType.AtscLvctTerrestrial) ||
              _completeTables.HasFlag(TableType.AtscNit | TableType.AtscNtt | TableType.AtscSvct) ||
              _completeTables.HasFlag(TableType.ScteLvctCable) ||
              _completeTables.HasFlag(TableType.ScteLvctTerrestrial) ||
              _completeTables.HasFlag(TableType.ScteNit | TableType.ScteNtt | TableType.ScteSvct)
            )
          )
          {
            this.LogInfo("scan ATSC: scan completed, tables complete = [{0}]", _completeTables);
            break;
          }

          // Flip over to the "CableCARD" time limit if we receive NIT, NTT or
          // SVCT from a clear QAM tuner.
          if (
            _seenTables.HasFlag(TableType.AtscNit) ||
            _seenTables.HasFlag(TableType.AtscNtt) ||
            _seenTables.HasFlag(TableType.AtscSvct) ||
            _seenTables.HasFlag(TableType.ScteNit) ||
            _seenTables.HasFlag(TableType.ScteNtt) ||
            _seenTables.HasFlag(TableType.ScteSvct)
          )
          {
            timeLimit = _timeLimitCableCard;
          }

          remainingTime = timeLimit - (int)(DateTime.Now - start).TotalMilliseconds;
          if (!_event.WaitOne(remainingTime))
          {
            this.LogWarn("scan ATSC: scan time limit reached, tables seen = [{0}], tables complete = [{1}]", _seenTables, _completeTables);
            break;
          }
        }
        while (remainingTime > 0);

        // Should we pull channel details from S-VCT or L-VCT?
        ushort transportStreamId = 0;
        IDictionary<uint, ProgramInfo> programs = new Dictionary<uint, ProgramInfo>(0);
        IDictionary<uint, ScannedChannel> atscChannels;
        IDictionary<ChannelGroupType, IDictionary<ulong, string>> atscGroupNames;
        ISet<string> atscIgnoredChannelNumbers;
        IDictionary<uint, ScannedChannel> scteChannels;
        IDictionary<ChannelGroupType, IDictionary<ulong, string>> scteGroupNames;
        ISet<string> scteIgnoredChannelNumbers;
        if (_seenTables.HasFlag(TableType.AtscSvct) || _seenTables.HasFlag(TableType.ScteSvct))
        {
          CollectSvctVirtualChannels(_grabberAtsc, channel, out atscChannels, out atscGroupNames, out atscIgnoredChannelNumbers);
          if (_cancelScan)
          {
            return;
          }

          CollectSvctVirtualChannels(_grabberScte, channel, out scteChannels, out scteGroupNames, out scteIgnoredChannelNumbers);
          if (_cancelScan)
          {
            return;
          }
        }
        else
        {
          // Read MPEG 2 TS program information.
          if (_grabberMpeg != null)
          {
            CollectPrograms(out transportStreamId, out programs);
            if (_cancelScan)
            {
              return;
            }
          }

          // Construct channels from the ATSC channel information and MPEG 2 TS
          // program information.
          CollectLvctChannels(_grabberAtsc, channel, transportStreamId, programs, out atscChannels, out atscGroupNames);
          if (_cancelScan)
          {
            return;
          }

          // Construct channels from the SCTE channel information and MPEG 2 TS
          // program information.
          CollectLvctChannels(_grabberScte, channel, transportStreamId, programs, out scteChannels, out scteGroupNames);
          if (_cancelScan)
          {
            return;
          }

          atscIgnoredChannelNumbers = new HashSet<string>();
          scteIgnoredChannelNumbers = new HashSet<string>();
        }

        // Combine the ATSC and SCTE channel and group information.
        IDictionary<uint, ScannedChannel> finalChannels;
        if (channel is ChannelScte)
        {
          CombineChannels(scteChannels, atscChannels);
          CombineGroupNames(scteGroupNames, atscGroupNames);
          finalChannels = scteChannels;
          groupNames = scteGroupNames;
        }
        else
        {
          CombineChannels(atscChannels, scteChannels);
          CombineGroupNames(atscGroupNames, scteGroupNames);
          finalChannels = atscChannels;
          groupNames = atscGroupNames;
        }
        ignoredChannelNumbers = atscIgnoredChannelNumbers;
        ignoredChannelNumbers.UnionWith(scteIgnoredChannelNumbers);

        // Add channels for programs that don't have VCT information.
        foreach (var program in programs)
        {
          if (program.Value.MediaType.HasValue && !finalChannels.ContainsKey(program.Key))
          {
            IChannel newChannel = (IChannel)channel.Clone();
            newChannel.Name = string.Empty;
            newChannel.LogicalChannelNumber = string.Empty;
            newChannel.MediaType = program.Value.MediaType.Value;
            newChannel.IsEncrypted = program.Value.IsEncrypted;
            newChannel.IsThreeDimensional = program.Value.IsThreeDimensional;

            ChannelMpeg2Base mpeg2Channel = newChannel as ChannelMpeg2Base;
            if (mpeg2Channel != null)
            {
              mpeg2Channel.TransportStreamId = transportStreamId;
              mpeg2Channel.ProgramNumber = program.Value.ProgramNumber;
              mpeg2Channel.PmtPid = program.Value.PmtPid;
            }

            ScannedChannel scannedChannel = new ScannedChannel(newChannel);
            scannedChannel.IsVisibleInGuide = true;
            finalChannels[program.Key] = scannedChannel;
          }
        }

        // Assign names and LCNs for channels that don't already have them.
        channels = finalChannels.Values.ToList();
        foreach (var c in channels)
        {
          if (string.IsNullOrEmpty(c.Channel.LogicalChannelNumber))
          {
            c.Channel.LogicalChannelNumber = c.Channel.DefaultLogicalChannelNumber;
          }

          if (string.IsNullOrEmpty(c.Channel.Name))
          {
            c.Channel.Name = GetNameForChannel(c.Channel);
          }
        }
      }
      finally
      {
        if (_grabberMpeg != null)
        {
          _grabberMpeg.SetCallBack(null);
        }
        if (_grabberAtsc != null)
        {
          _grabberAtsc.SetCallBack(null);
        }
        if (_grabberScte != null)
        {
          _grabberScte.SetCallBack(null);
        }
        _event.Close();
        _event = null;
        _isScanning = false;
      }
    }

    /// <summary>
    /// Tune to a specified channel and scan for transmitter tuning details
    /// within the available network information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the tuning details found</returns>
    public IList<TuningDetail> ScanNetworkInformation(IChannel channel)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Abort scanning for channels and/or network information.
    /// </summary>
    public void AbortScanning()
    {
      this.LogInfo("scan ATSC: abort");
      _cancelScan = true;
      try
      {
        if (_tuner != null)
        {
          _tuner.CancelTune(0);
        }
        if (_event != null)
        {
          _event.Set();
        }
      }
      catch
      {
      }
    }

    #endregion

    #region private functions

    private static TableType GetTableType(ushort pid, byte tableId)
    {
      switch (pid)
      {
        case 0:
          switch (tableId)
          {
            case 0:
              return TableType.Pat;
            case 2:
              return TableType.Pmt;
          }
          break;
        case 1:
          return TableType.Cat;
        case 0x1ffb:
          switch (tableId)
          {
            case 0xc2:
              return TableType.AtscNit;
            case 0xc3:
              return TableType.AtscNtt;
            case 0xc4:
              return TableType.AtscSvct;
            case 0xc7:
              return TableType.AtscMgt;
            case 0xc8:
              return TableType.AtscLvctTerrestrial;
            case 0xc9:
              return TableType.AtscLvctCable;
            case 0xd8:
              return TableType.AtscEam;
          }
          break;
        case 0x1ffc:
          switch (tableId)
          {
            case 0xc2:
              return TableType.ScteNit;
            case 0xc3:
              return TableType.ScteNtt;
            case 0xc4:
              return TableType.ScteSvct;
            case 0xc7:
              return TableType.ScteMgt;
            case 0xc8:
              return TableType.ScteLvctTerrestrial;
            case 0xc9:
              return TableType.ScteLvctCable;
            case 0xd8:
              return TableType.ScteEam;
          }
          break;
      }
      return TableType.None;
    }

    /// <summary>
    /// Collect the program information from an MPEG 2 transport stream.
    /// </summary>
    /// <param name="transportStreamId">The transport stream's identifier.</param>
    /// <param name="programs">A dictionary of programs, keyed on the transport stream identifier and program number.</param>
    private void CollectPrograms(out ushort transportStreamId, out IDictionary<uint, ProgramInfo> programs)
    {
      ushort networkPid;
      ushort programCount;
      _grabberMpeg.GetTransportStreamDetail(out transportStreamId, out networkPid, out programCount);
      this.LogInfo("scan ATSC: TSID = {0}, network PID = {1}, program count = {2}", transportStreamId, networkPid, programCount);
      programs = new Dictionary<uint, ProgramInfo>(programCount);

      ushort programNumber;
      ushort pmtPid;
      bool isPmtReceived;
      ushort streamCountVideo;
      ushort streamCountAudio;
      bool isEncrypted;
      bool isEncryptionDetectionAccurate;
      bool isThreeDimensional;
      byte audioLanguageCount = COUNT_AUDIO_LANGUAGES;
      Iso639Code[] audioLanguages = new Iso639Code[audioLanguageCount];
      byte subtitlesLanguageCount = COUNT_SUBTITLES_LANGUAGES;
      Iso639Code[] subtitlesLanguages = new Iso639Code[subtitlesLanguageCount];
      for (ushort i = 0; i < programCount; i++)
      {
        if (_cancelScan)
        {
          return;
        }

        audioLanguageCount = COUNT_AUDIO_LANGUAGES;
        subtitlesLanguageCount = COUNT_SUBTITLES_LANGUAGES;
        if (!_grabberMpeg.GetProgramByIndex(i, out programNumber, out pmtPid, out isPmtReceived,
                                            out streamCountVideo, out streamCountAudio,
                                            out isEncrypted, out isEncryptionDetectionAccurate,
                                            out isThreeDimensional,
                                            audioLanguages, ref audioLanguageCount,
                                            subtitlesLanguages, ref subtitlesLanguageCount))
        {
          this.LogWarn("scan ATSC: failed to get MPEG 2 program, index = {0}", i);
          break;
        }
        this.LogInfo("  {0, -2}: program number = {1, -5}, PMT PID = {2, -5}, is PMT received = {3, -5}, video stream count = {4}, audio stream count = {5}, is encrypted = {6, -5} (accurate = {7, -5}), is 3D = {8, -5}",
                      i + 1, programNumber, pmtPid, isPmtReceived, streamCountVideo, streamCountAudio,
                      isEncrypted, isEncryptionDetectionAccurate, isThreeDimensional);
        this.LogDebug("    audio language count = {0}, languages = {1}", audioLanguageCount, string.Join(", ", audioLanguages.Take(audioLanguageCount)));
        this.LogDebug("    subtitles language count = {0}, languages = {1}", subtitlesLanguageCount, string.Join(", ", subtitlesLanguages.Take(subtitlesLanguageCount)));

        ProgramInfo program = new ProgramInfo();
        program.ProgramNumber = programNumber;
        program.PmtPid = pmtPid;
        if (isPmtReceived)
        {
          if (streamCountVideo > 0)
          {
            program.MediaType = MediaType.Television;
          }
          else if (streamCountAudio > 0)
          {
            program.MediaType = MediaType.Radio;
          }
          program.IsEncrypted = isEncrypted;
          program.IsEncryptionDetectionAccurate = isEncryptionDetectionAccurate;
          program.IsThreeDimensional = isThreeDimensional;
        }
        programs[((uint)transportStreamId << 16) | programNumber] = program;
      }
    }

    /// <summary>
    /// Collect the channel information from an ATSC or SCTE short-form virtual
    /// channel table.
    /// </summary>
    /// <param name="grabber">The channel information grabber.</param>
    /// <param name="tuningChannel">The tuning details used to tune the current transport stream.</param>
    /// <param name="channels">A dictionary of channels, keyed on the major and minor channel numbers.</param>
    /// <param name="groupNames">A dictionary of channel group names.</param>
    /// <param name="ignoredChannelNumbers">A set of the channel numbers that were ignored for whatever reason.</param>
    private void CollectSvctVirtualChannels(IGrabberSiAtsc grabber, IChannel tuningChannel, out IDictionary<uint, ScannedChannel> channels, out IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames, out ISet<string> ignoredChannelNumbers)
    {
      ushort channelCount = 0;
      if (grabber != null)
      {
        channelCount = grabber.GetSvctVirtualChannelCount();
      }
      this.LogInfo("scan ATSC: S-VCT virtual channel count = {0}", channelCount);

      channels = new Dictionary<uint, ScannedChannel>(channelCount);
      groupNames = new Dictionary<ChannelGroupType, IDictionary<ulong, string>>(5);
      ignoredChannelNumbers = new HashSet<string>();

      int j = 1;
      TransmissionMedium transmissionMedium;
      ushort vctId;
      ushort majorChannelNumber;
      if (channelCount > 0)
      {
        IntPtr mapNameBuffer = Marshal.AllocCoTaskMem(NAME_BUFFER_SIZE);
        IntPtr sourceNameBuffer = Marshal.AllocCoTaskMem(NAME_BUFFER_SIZE);
        try
        {
          Iso639Code mapNameLanguage;
          bool splice;
          uint activationTime;
          bool hdtvChannel;
          bool preferredSource;
          bool applicationVirtualChannel;
          ushort minorChannelNumber;
          ushort sourceId;
          Iso639Code sourceNameLanguage;
          bool accessControlled;
          bool hideGuide;
          ServiceType serviceType;
          bool outOfBand;
          BitstreamSelect bitstreamSelect;
          PathSelect pathSelect;
          ChannelType channelType;
          ushort nvodChannelBase;
          TransportType transportType;
          bool wideBandwidthVideo;
          WaveformStandard waveformStandard;
          VideoStandard videoStandard;
          bool wideBandwidthAudio;
          bool compandedAudio;
          MatrixMode matrixMode;
          ushort subcarrier2Offset;
          ushort subcarrier1Offset;
          bool suppressVideo;
          AudioSelection audioSelection;
          ushort programNumber;
          ushort transportStreamId;
          byte satelliteId;
          Iso639Code satelliteNameLanguage;
          ushort satelliteReferenceNameBufferSize = 0;
          ushort satelliteFullNameBufferSize = 0;
          Hemisphere hemisphere;
          ushort orbitalPosition;
          bool youAreHere;
          FrequencyBand frequencyBand;
          bool outOfService;
          PolarisationType polarisationType;
          byte transponderNumber;
          Iso639Code transponderNameLanguage;
          ushort transponderNameBufferSize = 0;
          bool rootTransponder;
          ToneSelect toneSelect;
          Polarisation polarisation;
          uint frequency;
          uint symbolRate;
          TransmissionSystem transmissionSystem;
          InnerCodingMode innerCodingMode;
          bool splitBitstreamMode;
          ModulationFormat modulationFormat;
          for (ushort i = 0; i < channelCount; i++)
          {
            if (_cancelScan)
            {
              channels.Clear();
              groupNames.Clear();
              return;
            }

            ushort mapNameBufferSize = NAME_BUFFER_SIZE;
            ushort sourceNameBufferSize = NAME_BUFFER_SIZE;
            if (!grabber.GetSvctVirtualChannel(i,
                                                out transmissionMedium,
                                                out vctId,
                                                out mapNameLanguage,
                                                mapNameBuffer,
                                                ref mapNameBufferSize,
                                                out splice,
                                                out activationTime,
                                                out hdtvChannel,
                                                out preferredSource,
                                                out applicationVirtualChannel,
                                                out majorChannelNumber,
                                                out minorChannelNumber,
                                                out sourceId,
                                                out sourceNameLanguage,
                                                sourceNameBuffer,
                                                ref sourceNameBufferSize,
                                                out accessControlled,
                                                out hideGuide,
                                                out serviceType,
                                                out outOfBand,
                                                out bitstreamSelect,
                                                out pathSelect,
                                                out channelType,
                                                out nvodChannelBase,
                                                out transportType,
                                                out wideBandwidthVideo,
                                                out waveformStandard,
                                                out videoStandard,
                                                out wideBandwidthAudio,
                                                out compandedAudio,
                                                out matrixMode,
                                                out subcarrier2Offset,
                                                out subcarrier1Offset,
                                                out suppressVideo,
                                                out audioSelection,
                                                out programNumber,
                                                out transportStreamId,
                                                out satelliteId,
                                                out satelliteNameLanguage,
                                                IntPtr.Zero,
                                                ref satelliteReferenceNameBufferSize,
                                                IntPtr.Zero,
                                                ref satelliteFullNameBufferSize,
                                                out hemisphere,
                                                out orbitalPosition,
                                                out youAreHere,
                                                out frequencyBand,
                                                out outOfService,
                                                out polarisationType,
                                                out transponderNumber,
                                                out transponderNameLanguage,
                                                IntPtr.Zero,
                                                ref transponderNameBufferSize,
                                                out rootTransponder,
                                                out toneSelect,
                                                out polarisation,
                                                out frequency,
                                                out symbolRate,
                                                out transmissionSystem,
                                                out innerCodingMode,
                                                out splitBitstreamMode,
                                                out modulationFormat))
            {
              this.LogWarn("scan ATSC: failed to get S-VCT virtual channel, index = {0}", i);
              break;
            }

            string mapName = DvbTextConverter.Convert(mapNameBuffer, mapNameBufferSize);
            if (string.IsNullOrEmpty(mapName))
            {
              mapName = string.Empty;
            }
            else
            {
              mapName = mapName.Trim();
            }
            this.LogInfo("  {0, -3}: transmission medium = {1}, VCT ID = {2, -5}, map name = {3}, map name language = {4}",
                          j++, transmissionMedium, vctId, mapName, mapNameLanguage.Code);
            this.LogDebug("    splice = {0, -5}, activation time = {1}, HDTV channel = {2, -5}, preferred source = {3, -5}",
                          splice, activationTime, hdtvChannel, preferredSource);
            this.LogInfo("    is application = {0, -5}, major channel number = {1, -4}, minor channel number = {2, -4}, source ID = {3, -5}",
                          applicationVirtualChannel, majorChannelNumber, minorChannelNumber, sourceId);

            string sourceName = DvbTextConverter.Convert(sourceNameBuffer, sourceNameBufferSize);
            if (string.IsNullOrEmpty(sourceName))
            {
              sourceName = string.Empty;
            }
            else
            {
              sourceName = sourceName.Trim();
            }
            this.LogInfo("    source name = {0}, source name language = {1}", sourceName, sourceNameLanguage.Code);

            this.LogInfo("    access controlled = {0, -5}, hide guide = {1, -5}, channel type = {2}, transport type = {3}, service type = {4, -2}",
                          accessControlled, hideGuide, channelType, transportType, serviceType);
            this.LogDebug("    path select = {0}, out of band = {1, -5}, bit-stream select = {2}, NVOD channel base = {3}",
                          pathSelect, outOfBand, bitstreamSelect, nvodChannelBase);
            if (channelType != ChannelType.NvodAccess)
            {
              if (transportType == TransportType.NonMpeg2)
              {
                this.LogInfo("    wide bandwidth video = {0, -5}, waveform standard = {1}, video standard = {2}, suppress video = {3, -5}",
                              wideBandwidthVideo, waveformStandard, videoStandard, suppressVideo);
                this.LogInfo("    wide bandwidth audio = {0, -5}, companded audio = {1, -5}, matrix mode = {2}, sub-carrier 2 offset = {3, -6} kHz, sub-carrier 1 offset = {4, -6} kHz, audio selection = {5}",
                              wideBandwidthVideo, compandedAudio, matrixMode, subcarrier2Offset, subcarrier1Offset, audioSelection);
              }
              else
              {
                this.LogInfo("    TSID = {0, -5}, program number = {1, -5}", transportStreamId, programNumber);
              }

              if (transmissionMedium == TransmissionMedium.Satellite)
              {
                this.LogInfo("    satellite ID = {0, -3}, hemisphere = {1}, orbital position = {2, -4}, you are here = {3, -5}, frequency band = {4, -9}, out of service = {5, -5}, polarisation type = {6, -8}",
                              satelliteId, hemisphere, orbitalPosition, youAreHere, frequencyBand, outOfService, polarisationType);
                this.LogInfo("    transponder number = {0, -3}, root transponder = {1, -5}, tone select = {2, -3}, polarisation = {3, -14}",
                              transponderNumber, rootTransponder, toneSelect, polarisation);
              }
              this.LogInfo("    frequency = {0, -6} kHz, symbol rate = {1, -8} s/s, transmission system = {2, -14}, inner coding mode = {3, -8}, split bit-stream mode = {4, -5}, modulation format = {5, -7}",
                            frequency, symbolRate, transmissionSystem, innerCodingMode, splitBitstreamMode, modulationFormat);
            }

            // NOTE: this function assumes that we will only ever receive S-VCT
            // from a cable feed. Technically there are older ATSC standards
            // that indicate S-VCT might be used with satellite and terrestrial
            // broadcasts as well. However we currently don't support those
            // possibilities.
            string lcn;
            if (minorChannelNumber == 0)
            {
              lcn = majorChannelNumber.ToString();
            }
            else
            {
              lcn = string.Format("{0}.{1}", majorChannelNumber, minorChannelNumber);
            }
            if (transmissionMedium != TransmissionMedium.Cable || applicationVirtualChannel || pathSelect == PathSelect.Path2 || outOfBand)   // not cable OR application (data) channel OR alternative feed
            {
              ignoredChannelNumbers.Add(lcn);
              continue;
            }

            IChannel newChannel;
            BroadcastStandard broadcastStandard = BroadcastStandard.Unknown;
            if (transportType == TransportType.NonMpeg2 || serviceType == ServiceType.AnalogTelevision)
            {
              if (frequency == 0 || frequency == 0xffffffff)
              {
                // By definition this channel must be broadcast by a different
                // transmitter. If the frequency in the VCT is not valid, we
                // can't create a valid channel.
                continue;
              }

              ChannelAnalogTv analogTvChannel = new ChannelAnalogTv();
              analogTvChannel.MediaType = MediaType.Television;
              analogTvChannel.Country = CountryCollection.Instance.GetCountryByIsoCode("US");
              analogTvChannel.Frequency = (int)frequency;   // assumed to be the analog video carrier frequency (ie. no adjustment required)
              analogTvChannel.PhysicalChannelNumber = ChannelScte.GetPhysicalChannelNumberForFrequency(analogTvChannel.Frequency + 1750);   // analog video carrier => digital centre
              analogTvChannel.TunerSource = AnalogTunerSource.Cable;

              newChannel = analogTvChannel;
              broadcastStandard = BroadcastStandard.AnalogTelevision;
            }
            else  // MPEG 2 transport OR non analog TV service
            {
              if (frequency >= 997250)
              {
                // Charter includes SDV channels in the CableCARD channel map,
                // but assigns them to physical channel 158 (which isn't
                // actually used).
                frequency = 0;
              }

              ChannelScte scteChannel = new ChannelScte();
              scteChannel.Frequency = (int)frequency;
              switch (modulationFormat)
              {
                case ModulationFormat.Qam16:
                  scteChannel.ModulationScheme = ModulationSchemeQam.Qam16;
                  break;
                case ModulationFormat.Qam32:
                  scteChannel.ModulationScheme = ModulationSchemeQam.Qam32;
                  break;
                case ModulationFormat.Qam64:
                  scteChannel.ModulationScheme = ModulationSchemeQam.Qam64;
                  break;
                case ModulationFormat.Qam128:
                  scteChannel.ModulationScheme = ModulationSchemeQam.Qam128;
                  break;
                case ModulationFormat.Qam512:
                  scteChannel.ModulationScheme = ModulationSchemeQam.Qam512;
                  break;
                case ModulationFormat.Qam1024:
                  scteChannel.ModulationScheme = ModulationSchemeQam.Qam1024;
                  break;
                default:
                  this.LogWarn("scan ATSC: unsupported cable modulation format {0}, falling back to automatic", modulationFormat);
                  scteChannel.ModulationScheme = ModulationSchemeQam.Automatic;
                  break;
              }

              if (serviceType == ServiceType.Audio)
              {
                scteChannel.MediaType = MediaType.Radio;
              }
              else
              {
                scteChannel.MediaType = MediaType.Television;
              }

              scteChannel.TransportStreamId = transportStreamId;    // may not be populated
              scteChannel.ProgramNumber = programNumber;
              scteChannel.SourceId = sourceId;

              newChannel = scteChannel;
              broadcastStandard = BroadcastStandard.Scte;
            }

            newChannel.Name = sourceName;
            if (!string.IsNullOrEmpty(mapName))
            {
              newChannel.Provider = "Cable";
            }
            else
            {
              newChannel.Provider = mapName;
            }
            newChannel.LogicalChannelNumber = lcn;
            newChannel.IsEncrypted = accessControlled;
            newChannel.IsHighDefinition = hdtvChannel;

            channels.Add(((uint)majorChannelNumber << 16) | minorChannelNumber, CreateScannedChannel(newChannel, channelType != ChannelType.Hidden || !hideGuide, broadcastStandard, groupNames));
          }
        }
        finally
        {
          if (mapNameBuffer != IntPtr.Zero)
          {
            Marshal.FreeCoTaskMem(mapNameBuffer);
          }
          if (sourceNameBuffer != IntPtr.Zero)
          {
            Marshal.FreeCoTaskMem(sourceNameBuffer);
          }
        }
      }

      // Create switched-digital-video (SDV) channels for the defined channels
      // which didn't have a virtual channel definition. This is speculative
      // and may not actually be helpful.
      j = 1;
      channelCount = 0;
      if (grabber != null)
      {
        grabber.GetSvctDefinedChannelCount();
      }
      this.LogInfo("scan ATSC: S-VCT defined channel count = {0}", channelCount);
      for (ushort i = 0; i < channelCount; i++)
      {
        if (!grabber.GetSvctDefinedChannel(i, out transmissionMedium, out vctId, out majorChannelNumber))
        {
          this.LogWarn("scan ATSC: failed to get S-VCT defined channel, index = {0}", i);
          break;
        }

        uint key = (uint)majorChannelNumber << 16;
        if (
          channels.ContainsKey(key) ||
          ignoredChannelNumbers.Contains(majorChannelNumber.ToString()) ||
          transmissionMedium != TransmissionMedium.Cable
        )
        {
          continue;
        }

        this.LogInfo("  {0, -3}: transmission medium = {1}, VCT ID = {2, -5}, virtual channel number = {3}",
                      j++, transmissionMedium, vctId, majorChannelNumber);
        channels.Add(key, CreateSdvChannel(string.Empty, majorChannelNumber.ToString(), true, groupNames));
      }
    }

    /// <summary>
    /// Collect the channel information from an ATSC or SCTE long-form virtual
    /// channel table.
    /// </summary>
    /// <remarks>
    /// L-VCT channel information is supplemented with MPEG 2 program information.
    /// </remarks>
    /// <param name="grabber">The channel information grabber.</param>
    /// <param name="tuningChannel">The tuning details used to tune the current transport stream.</param>
    /// <param name="currentTransportStreamId">The current transport stream's identifier.</param>
    /// <param name="programs">A dictionary of programs, keyed on the transport stream identifier and program number.</param>
    /// <param name="channels">A dictionary of channels, keyed on the transport stream identifier and program number.</param>
    /// <param name="groupNames">A dictionary of channel group names.</param>
    private void CollectLvctChannels(IGrabberSiAtsc grabber, IChannel tuningChannel, ushort currentTransportStreamId, IDictionary<uint, ProgramInfo> programs, out IDictionary<uint, ScannedChannel> channels, out IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      ushort channelCount = 0;
      if (grabber != null)
      {
        channelCount = grabber.GetLvctChannelCount();
      }
      this.LogInfo("scan ATSC: L-VCT channel count = {0}", channelCount);

      channels = new Dictionary<uint, ScannedChannel>(channelCount);
      groupNames = new Dictionary<ChannelGroupType, IDictionary<ulong, string>>(5);
      if (channelCount == 0)
      {
        return;
      }

      IntPtr shortNameBuffer = Marshal.AllocCoTaskMem(NAME_BUFFER_SIZE);
      try
      {
        int j = 1;
        byte tableId;
        ushort sectionTransportStreamId;
        ushort mapId;
        byte longNameCount;
        ushort majorChannelNumber;
        ushort minorChannelNumber;
        ModulationMode modulationMode;
        uint carrierFrequency;
        ushort transportStreamId;
        ushort programNumber;
        EtmLocation etmLocation;
        bool accessControlled;
        bool hidden;
        PathSelect pathSelect;
        bool outOfBand;
        bool hideGuide;
        ServiceType serviceType;
        ushort sourceId;
        byte streamCountVideo;
        byte streamCountAudio;
        bool isThreeDimensional;
        Iso639Code[] audioLanguages = new Iso639Code[COUNT_AUDIO_LANGUAGES];
        Iso639Code[] captionsLanguages = new Iso639Code[COUNT_CAPTIONS_LANGUAGES];
        for (ushort i = 0; i < channelCount; i++)
        {
          if (_cancelScan)
          {
            channels.Clear();
            groupNames.Clear();
            return;
          }

          ushort shortNameBufferSize = NAME_BUFFER_SIZE;
          byte audioLanguageCount = COUNT_AUDIO_LANGUAGES;
          byte captionsLanguageCount = COUNT_CAPTIONS_LANGUAGES;
          if (!grabber.GetLvctChannel(i,
                                      out tableId,
                                      out sectionTransportStreamId,
                                      out mapId,
                                      shortNameBuffer,
                                      ref shortNameBufferSize,
                                      out longNameCount,
                                      out majorChannelNumber,
                                      out minorChannelNumber,
                                      out modulationMode,
                                      out carrierFrequency,
                                      out transportStreamId,
                                      out programNumber,
                                      out etmLocation,
                                      out accessControlled,
                                      out hidden,
                                      out pathSelect,
                                      out outOfBand,
                                      out hideGuide,
                                      out serviceType,
                                      out sourceId,
                                      out streamCountVideo,
                                      out streamCountAudio,
                                      out isThreeDimensional,
                                      audioLanguages,
                                      ref audioLanguageCount,
                                      captionsLanguages,
                                      ref captionsLanguageCount))
          {
            this.LogWarn("scan ATSC: failed to get L-VCT channel, index = {0}", i);
            break;
          }

          if (tuningChannel is ChannelAtsc && currentTransportStreamId != transportStreamId)
          {
            // We don't support creating channels for other ATSC transport
            // streams.
            continue;
          }

          this.LogInfo("  {0, -3}: table ID = {1, -2}, section TSID = {2, -5}, map ID = {3, -5, carrier frequency = {4} Hz, modulation mode = {5}",
                        j++, tableId, sectionTransportStreamId, mapId, carrierFrequency, modulationMode);

          string shortName = DvbTextConverter.Convert(shortNameBuffer, shortNameBufferSize);
          if (shortName == null)
          {
            shortName = string.Empty;
          }
          else
          {
            shortName = shortName.Trim();
          }
          this.LogInfo("    TSID = {0, -5}, program number = {1, -5}, major channel number = {2, -4}, minor channel number = {3, -4}, short name = {4}",
                        transportStreamId, programNumber, majorChannelNumber, minorChannelNumber, shortName);

          List<string> longNames;
          List<string> longNameLanguages;
          CollectLvctChannelNames(grabber, i, longNameCount, out longNames, out longNameLanguages);
          this.LogInfo("    long name count = {0, -2}, names = [{1}], languages = [{2}]", longNameCount, string.Join(", ", longNames), string.Join(", ", longNameLanguages));

          this.LogInfo("    source ID = {0, -5}, video stream count = {1}, audio stream count = {2}, is 3D = {3, -5}, service type = {4}",
                        sourceId, streamCountVideo, streamCountAudio, isThreeDimensional, serviceType);
          this.LogInfo("    access controlled = {0, -5}, hidden = {1, -5}, path select = {2}, out of band = {3, -5}, hide guide = {4, -5}, ETM location = {5}",
                        accessControlled, hidden, pathSelect, outOfBand, hideGuide, etmLocation);

          if (audioLanguageCount > 0)
          {
            this.LogDebug("    audio language count    = {0}, languages  = [{1}]", audioLanguageCount, string.Join(", ", audioLanguages.Take(audioLanguageCount)));
          }
          if (captionsLanguageCount > 0)
          {
            this.LogDebug("    captions language count = {0}, languages  = [{1}]", captionsLanguageCount, string.Join(", ", captionsLanguages.Take(captionsLanguageCount)));
          }

          if (tuningChannel is ChannelScte && (pathSelect == PathSelect.Path2 || outOfBand))
          {
            // We don't create cable channels if they aren't available in the
            // current feed.
            continue;
          }

          uint programKey = ((uint)transportStreamId << 16) | programNumber;
          ProgramInfo program = null;
          if (programs != null)
          {
            programs.TryGetValue(programKey, out program);
          }

          IChannel newChannel;
          BroadcastStandard broadcastStandard = BroadcastStandard.Unknown;
          if (serviceType == ServiceType.AnalogTelevision || modulationMode == ModulationMode.Analog)
          {
            if (carrierFrequency == 0 || carrierFrequency == 0xffffffff)
            {
              // By definition this channel must be broadcast by a different
              // transmitter. If the frequency in the VCT is not valid, we
              // can't create a valid channel.
              continue;
            }

            ChannelAnalogTv analogTvChannel = new ChannelAnalogTv();
            analogTvChannel.MediaType = MediaType.Television;
            analogTvChannel.Country = CountryCollection.Instance.GetCountryByIsoCode("US");
            analogTvChannel.Frequency = (int)(carrierFrequency / 1000);   // Hz to kHz; analog video carrier
            if (tuningChannel is ChannelAtsc)
            {
              analogTvChannel.PhysicalChannelNumber = ChannelAtsc.GetPhysicalChannelNumberForFrequency(analogTvChannel.Frequency + 1750);   // analog video carrier => digital centre
              analogTvChannel.TunerSource = AnalogTunerSource.Antenna;
              analogTvChannel.Provider = "Terrestrial";
            }
            else
            {
              analogTvChannel.PhysicalChannelNumber = ChannelScte.GetPhysicalChannelNumberForFrequency(analogTvChannel.Frequency + 1750);   // analog video carrier => digital centre
              analogTvChannel.TunerSource = AnalogTunerSource.Cable;
              analogTvChannel.Provider = "Cable";
            }
            newChannel = analogTvChannel;
            broadcastStandard = BroadcastStandard.AnalogTelevision;
          }
          else
          {
            newChannel = (IChannel)tuningChannel.Clone();
            if (serviceType == ServiceType.DigitalTelevision || streamCountVideo > 0)
            {
              newChannel.MediaType = MediaType.Television;
            }
            else if (serviceType == ServiceType.Audio || streamCountAudio > 0)
            {
              newChannel.MediaType = MediaType.Radio;
            }
            else
            {
              if (program == null || !program.MediaType.HasValue)
              {
                continue;
              }
              newChannel.MediaType = program.MediaType.Value;
            }
          }

          newChannel.Name = shortName;
          if (longNames.Count > 0)
          {
            newChannel.Name = longNames[0];
            if (!string.IsNullOrEmpty(shortName))
            {
              string lowerCaseShortName = shortName.ToLowerInvariant();
              string lowerCaseLongName = newChannel.Name.ToLowerInvariant();
              if (!lowerCaseShortName.Contains(lowerCaseLongName) && !lowerCaseLongName.Contains(lowerCaseShortName))
              {
                newChannel.Name += string.Format(" ({0})", shortName);
              }
            }
          }
          if (minorChannelNumber == 0)
          {
            newChannel.LogicalChannelNumber = majorChannelNumber.ToString();
          }
          else
          {
            newChannel.LogicalChannelNumber = string.Format("{0}.{1}", majorChannelNumber, minorChannelNumber);
          }
          newChannel.IsEncrypted = accessControlled;
          if (program != null)
          {
            if (program.IsEncryptionDetectionAccurate)
            {
              newChannel.IsEncrypted = program.IsEncrypted;
            }
            else
            {
              newChannel.IsEncrypted |= program.IsEncrypted;
            }
          }
          newChannel.IsThreeDimensional = isThreeDimensional;
          if (program != null)
          {
            newChannel.IsThreeDimensional |= program.IsThreeDimensional;
          }

          ChannelAtsc atscChannel = newChannel as ChannelAtsc;
          if (atscChannel != null)
          {
            broadcastStandard = BroadcastStandard.Atsc;
            atscChannel.Provider = "Terrestrial";
            atscChannel.TransportStreamId = transportStreamId;
            atscChannel.ProgramNumber = programNumber;
            atscChannel.SourceId = sourceId;
            if (program != null)
            {
              atscChannel.PmtPid = program.PmtPid;
            }
          }
          else
          {
            ChannelScte scteChannel = newChannel as ChannelScte;
            if (scteChannel != null)
            {
              broadcastStandard = BroadcastStandard.Scte;
              if (
                (currentTransportStreamId != 0 && currentTransportStreamId != transportStreamId) ||   // Channel from another transport stream.
                (scteChannel.Frequency <= 0)    // CableCARD scan
              )
              {
                // This channel is broadcast by a different transmitter.
                scteChannel.Frequency = (int)(carrierFrequency / 1000);
                if (modulationMode == ModulationMode.ScteMode1)
                {
                  scteChannel.ModulationScheme = ModulationSchemeQam.Qam64;
                }
                else if (modulationMode == ModulationMode.ScteMode2)
                {
                  scteChannel.ModulationScheme = ModulationSchemeQam.Qam256;
                }
                else
                {
                  this.LogWarn("scan ATSC: unsupported cable modulation scheme {0}, falling back to automatic", modulationMode);
                  scteChannel.ModulationScheme = ModulationSchemeQam.Automatic;
                }
              }

              scteChannel.Provider = "Cable";
              scteChannel.TransportStreamId = transportStreamId;
              scteChannel.ProgramNumber = programNumber;
              scteChannel.SourceId = sourceId;
              if (program != null)
              {
                scteChannel.PmtPid = program.PmtPid;
              }
            }
          }

          channels.Add(programKey, CreateScannedChannel(newChannel, !hidden || !hideGuide, broadcastStandard, groupNames));
        }
      }
      finally
      {
        if (shortNameBuffer != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(shortNameBuffer);
        }
      }
    }

    /// <summary>
    /// Collect the names for an ATSC or SCTE long-form virtual channel table
    /// channel.
    /// </summary>
    /// <param name="grabber">The channel information grabber.</param>
    /// <param name="channelIndex">The channel's index.</param>
    /// <param name="nameCount">The number of names available for the channel.</param>
    /// <param name="names">The channel's names.</param>
    /// <param name="languages">The languages associated with the <paramref name="names">channel names</paramref>.</param>
    private static void CollectLvctChannelNames(IGrabberSiAtsc grabber, ushort channelIndex, byte nameCount, out List<string> names, out List<string> languages)
    {
      names = new List<string>(nameCount);
      languages = new List<string>(nameCount);
      if (nameCount == 0)
      {
        return;
      }

      Iso639Code language;
      ushort nameBufferSize;
      IntPtr nameBuffer = Marshal.AllocCoTaskMem(NAME_BUFFER_SIZE);
      try
      {
        for (byte i = 0; i < nameCount; i++)
        {
          nameBufferSize = NAME_BUFFER_SIZE;
          if (grabber.GetLvctChannelLongNameByIndex(channelIndex, i, out language, nameBuffer, ref nameBufferSize))
          {
            string name = DvbTextConverter.Convert(nameBuffer, nameBufferSize);
            if (string.IsNullOrWhiteSpace(name))
            {
              continue;
            }
            names.Add(name.Trim());
            languages.Add(language.Code);
          }
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(nameBuffer);
      }
    }

    /// <summary>
    /// Combine two sets of channels.
    /// </summary>
    /// <remarks>
    /// Some channel definitions in each set may be unique; others may need to
    /// be merged. Some details within a channel definition may be unique;
    /// others may be common/shared.
    /// </remarks>
    /// <typeparam name="T">The common/shared key type of the channel sets.</typeparam>
    /// <param name="preferredChannels">The first channel set containing the preferred details.</param>
    /// <param name="secondaryChannels">The second channel set containing alternative/secondary details.</param>
    private void CombineChannels<T>(IDictionary<T, ScannedChannel> preferredChannels, IDictionary<T, ScannedChannel> secondaryChannels)
    {
      foreach (var secondaryScannedChannel in secondaryChannels)
      {
        ScannedChannel preferredScannedChannel;
        if (!preferredChannels.TryGetValue(secondaryScannedChannel.Key, out preferredScannedChannel))
        {
          preferredChannels[secondaryScannedChannel.Key] = secondaryScannedChannel.Value;
          continue;
        }

        IChannel preferredChannel = preferredScannedChannel.Channel;
        IChannel secondaryChannel = secondaryScannedChannel.Value.Channel;
        if (string.IsNullOrEmpty(preferredChannel.Name))
        {
          preferredChannel.Name = secondaryChannel.Name;
        }
        if (string.IsNullOrEmpty(preferredChannel.Provider))
        {
          preferredChannel.Provider = secondaryChannel.Provider;
        }
        if (string.IsNullOrEmpty(preferredChannel.LogicalChannelNumber))
        {
          preferredChannel.LogicalChannelNumber = secondaryChannel.LogicalChannelNumber;
        }
        if (preferredChannel.MediaType != secondaryChannel.MediaType)
        {
          preferredChannel.MediaType = MediaType.Television;  // assumption: we only have TV and radio channels
        }
        if (preferredChannel.IsEncrypted != secondaryChannel.IsEncrypted)
        {
          preferredChannel.IsEncrypted = true;
        }
        if (preferredChannel.IsHighDefinition != secondaryChannel.IsHighDefinition)
        {
          preferredChannel.IsHighDefinition = true;
        }
        if (preferredChannel.IsThreeDimensional != secondaryChannel.IsThreeDimensional)
        {
          preferredChannel.IsThreeDimensional = true;
        }

        foreach (var group in secondaryScannedChannel.Value.Groups)
        {
          ICollection<ulong> groupIds;
          if (!preferredScannedChannel.Groups.TryGetValue(group.Key, out groupIds))
          {
            preferredScannedChannel.Groups[group.Key] = group.Value;
            continue;
          }

          foreach (ulong groupId in group.Value)
          {
            if (!groupIds.Contains(groupId))
            {
              groupIds.Add(groupId);
            }
          }
        }
      }
    }

    /// <summary>
    /// Combine two sets of channel group names.
    /// </summary>
    /// <remarks>
    /// Some group names in each set may be unique; others may be
    /// common/shared.
    /// </remarks>
    /// <param name="preferredGroupNames">The first channel group name set containing the preferred names.</param>
    /// <param name="secondaryGroupNames">The second channel group name set containing alternative/secondary names.</param>
    private void CombineGroupNames(IDictionary<ChannelGroupType, IDictionary<ulong, string>> preferredGroupNames, IDictionary<ChannelGroupType, IDictionary<ulong, string>> secondaryGroupNames)
    {
      foreach (var groupType in secondaryGroupNames)
      {
        IDictionary<ulong, string> groupNames;
        if (!preferredGroupNames.TryGetValue(groupType.Key, out groupNames))
        {
          preferredGroupNames[groupType.Key] = groupType.Value;
          continue;
        }

        foreach (var group in groupType.Value)
        {
          if (!groupNames.ContainsKey(group.Key))
          {
            groupNames[group.Key] = group.Value;
          }
        }
      }
    }

    /// <summary>
    /// Get a name for a channel that would otherwise be nameless.
    /// </summary>
    /// <param name="channel">The nameless channel.</param>
    /// <returns>a name for the channel</returns>
    private string GetNameForChannel(IChannel channel)
    {
      ChannelScte scteChannel = channel as ChannelScte;
      if (scteChannel != null && scteChannel.Frequency <= 0)
      {
        return string.Format("Unknown SDV {0}", channel.LogicalChannelNumber);
      }
      return string.Format("Unknown {0}", channel.LogicalChannelNumber);
    }

    #endregion
  }
}