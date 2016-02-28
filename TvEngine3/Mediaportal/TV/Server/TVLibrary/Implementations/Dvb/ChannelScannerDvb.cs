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
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Common.Types.Provider;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using MediaPortal.Common.Utils.ExtensionMethods;
using DvbPolarisation = Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum.Polarisation;
using DvbRollOffFactor = Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum.RollOffFactor;
using TvePolarisation = Mediaportal.TV.Server.Common.Types.Enum.Polarisation;
using TveRollOffFactor = Mediaportal.TV.Server.Common.Types.Enum.RollOffFactor;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dvb
{
  /// <summary>
  /// An implementation of <see cref="IChannelScanner"/> for DVB-compliant
  /// transport streams.
  /// </summary>
  internal class ChannelScannerDvb : IChannelScannerInternal, ICallBackGrabber
  {
    #region enums, constants and private classes

    [Flags]
    private enum TableType
    {
      None = 0,
      Pat = 0x01,
      Cat = 0x02,
      Pmt = 0x04,

      SdtActual = 0x0100,
      SdtOther = 0x0200,
      NitActual = 0x0400,
      NitOther = 0x0800,
      Bat = 0x1000,

      FreesatSdt = 0x01000000,
      FreesatNit = 0x02000000,
      FreesatBat = 0x04000000
    }

    private const int NAME_BUFFER_SIZE = 1000;

    private const byte COUNT_AUDIO_LANGUAGES = 15;
    private const byte COUNT_AVAILABLE_IN_CELLS = 100;
    private const byte COUNT_AVAILABLE_IN_COUNTRIES = 15;
    private const byte COUNT_BOUQUET_IDS = 30;
    private const byte COUNT_FREESAT_CHANNEL_CATEGORY_IDS = 50;
    private const byte COUNT_FREESAT_REGION_IDS = 100;
    private const byte COUNT_FREQUENCIES = 50;
    private const ushort COUNT_LOGICAL_CHANNEL_NUMBERS = 500;
    private const byte COUNT_NETWORK_IDS = 15;
    private const byte COUNT_NORDIG_CHANNEL_LIST_IDS = 50;
    private const byte COUNT_OPENTV_REGION_IDS = 100;
    private const byte COUNT_SUBTITLES_LANGUAGES = 15;
    private const byte COUNT_TARGET_REGION_IDS = 100;
    private const byte COUNT_UNAVAILABLE_IN_CELLS = 100;
    private const byte COUNT_UNAVAILABLE_IN_COUNTRIES = 15;

    private static readonly IDictionary<ulong, string> VIRGIN_MEDIA_CHANNEL_CATEGORY_NAMES = new Dictionary<ulong, string>
    {
      { 1, "Factual" },
      { 2, "Entertainment" },
      { 3, "International" },
      { 4, "Radio" },
      { 5, "Kids" },
      { 6, "Lifestyle" },
      { 7, "Movies" },
      { 8, "Music" },
      { 9, "News" },
      { 10, "Sport" },
      //{ 11, string.Empty },     "live events channel"
      { 12, "Adult" },
      { 13, "Shopping" }
      //{ 14, string.Empty },     "Virgin iD"
      //{ 15, string.Empty }      "Top Left 4KTV test", "Bot Left 4KTV test"
    };

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
    private int _timeLimitNetworkInformation = 15000;

    // provider preferences
    private string _dishNetworkStateAbbreviation = string.Empty;
    private ushort _provider1BouquetId = 0;
    private ushort _provider1RegionId = 0;
    private ushort _provider2BouquetId = 0;
    private ushort _provider2RegionId = 0;
    private bool _preferProvider2ChannelDetails = false;
    private bool _preferHighDefinitionChannelNumbers = true;

    private IGrabberSiMpeg _grabberMpeg = null;
    private IGrabberSiDvb _grabberDvb = null;
    private IGrabberSiFreesat _grabberFreesat = null;
    private TableType _seenTables = TableType.None;
    private TableType _completeTables = TableType.None;
    private ITuner _tuner = null;
    private AutoResetEvent _event = null;
    private volatile bool _cancelScan = false;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="ChannelScannerDvb"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    /// <param name="grabberMpeg">The MPEG 2 transport stream analyser instance to use for scanning.</param>
    /// <param name="grabberDvb">The DVB stream analyser instance to use for scanning.</param>
    /// <param name="grabberFreesat">The Freesat stream analyser instance to use for scanning.</param>
    public ChannelScannerDvb(ITuner tuner, IGrabberSiMpeg grabberMpeg, IGrabberSiDvb grabberDvb, IGrabberSiFreesat grabberFreesat)
    {
      _tuner = tuner;
      _grabberMpeg = grabberMpeg;
      _grabberDvb = grabberDvb;
      _grabberFreesat = grabberFreesat;
    }

    #endregion

    #region ICallBackGrabber members

    /// <summary>
    /// This function is invoked when the first section from a table is received.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that was received.</param>
    public void OnTableSeen(ushort pid, byte tableId)
    {
      this.LogInfo("scan DVB: on table seen, PID = {0}, table ID = {1}", pid, tableId);
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
    public void OnTableComplete(ushort pid, byte tableId)
    {
      this.LogInfo("scan DVB: on table complete, PID = {0}, table ID = {1}", pid, tableId);
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
    public void OnTableChange(ushort pid, byte tableId)
    {
      this.LogDebug("scan DVB: on table change, PID = {0}, table ID = {1}", pid, tableId);
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
      this.LogDebug("scan DVB: reload configuration");

      // timing
      _minimumTime = SettingsManagement.GetValue("minimumScanTime", 2000);
      _timeLimitSingleTransmitter = SettingsManagement.GetValue("timeLimitScanSingleTransmitter", 15000);
      _timeLimitNetworkInformation = SettingsManagement.GetValue("timeLimitScanNetworkInformation", 15000);
      this.LogDebug("  timing...");
      this.LogDebug("    minimum             = {0} ms", _minimumTime);
      this.LogDebug("    single transmitter  = {0} ms", _timeLimitSingleTransmitter);
      this.LogDebug("    network information = {0} ms", _timeLimitNetworkInformation);

      // provider preferences
      string countryName = RegionInfo.CurrentRegion.EnglishName;
      this.LogDebug("  country               = {0}", countryName);
      if (string.Equals(countryName, "United States"))
      {
        string[] configParts = SettingsManagement.GetValue("scanProviderDishNetwork", string.Empty).Split(',');
        if (configParts.Length >= 2)
        {
          _dishNetworkStateAbbreviation = configParts[1];
        }
        this.LogDebug("  Dish Network state    = {0}", _dishNetworkStateAbbreviation);
        return;
      }

      string config = SettingsManagement.GetValue("scanProviderOpenTv", string.Empty);
      string[] parts = config.Split(',');
      if (parts.Length >= 2)
      {
        ushort.TryParse(parts[0], out _provider1BouquetId);
        ushort.TryParse(parts[1], out _provider1RegionId);
      }
      if (string.Equals(countryName, "New Zealand"))
      {
        _provider2BouquetId = (ushort)SettingsManagement.GetValue("scanProviderFreeviewSatellite", 0);
      }
      else
      {
        config = SettingsManagement.GetValue("scanProviderFreesat", string.Empty);
        parts = config.Split(',');
        if (parts.Length >= 2)
        {
          ushort.TryParse(parts[0], out _provider2BouquetId);
          ushort.TryParse(parts[1], out _provider2RegionId);
        }
      }
      _preferProvider2ChannelDetails = SettingsManagement.GetValue("scanPreferProvider2ChannelDetails", false);
      _preferHighDefinitionChannelNumbers = SettingsManagement.GetValue("scanPreferHighDefinitionChannelNumbers", true);

      this.LogDebug("  provider 1...");
      this.LogDebug("    bouquet ID          = {0}", _provider1BouquetId);
      this.LogDebug("    region ID           = {0}", _provider1RegionId);
      this.LogDebug("  provider 2...");
      this.LogDebug("    bouquet ID          = {0}", _provider2BouquetId);
      this.LogDebug("    region ID           = {0}", _provider2RegionId);
      this.LogDebug("  prefer provider 2?    = {0}", _preferProvider2ChannelDetails);
      this.LogDebug("  prefer HD LCNs?       = {0}", _preferHighDefinitionChannelNumbers);
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
    public virtual void Scan(IChannel channel, bool isFastNetworkScan, out IList<ScannedChannel> channels, out IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      channels = new List<ScannedChannel>(100);
      groupNames = new Dictionary<ChannelGroupType, IDictionary<ulong, string>>(50);

      if (_grabberMpeg == null || _grabberDvb == null)
      {
        this.LogError("scan DVB: grabber interfaces not available, not possible to scan");
        return;
      }

      try
      {
        _cancelScan = false;
        _isScanning = true;
        _event = new AutoResetEvent(false);
        _seenTables = TableType.None;
        _completeTables = TableType.None;
        _grabberMpeg.SetCallBack(this);
        _grabberDvb.SetCallBack(this);
        if (_grabberFreesat != null)
        {
          _grabberFreesat.SetCallBack(this);
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

        if (!_seenTables.HasFlag(TableType.Pat))
        {
          this.LogWarn("scan DVB: PAT not available, is the tuner delivering a stream?");
          return;
        }

        // Wait for scanning to complete.
        do
        {
          if (_cancelScan)
          {
            return;
          }

          // Check for scan completion.
          if (
            // Basic requirement: PAT and PMT must have been received.
            _completeTables.HasFlag(TableType.Pat | TableType.Pmt) &&
            // Either SDT actual or SDT other should have been received.
            // For a non-network scan we normally only need SDT actual. However
            // as a special case we support Dish Network, who only broadcast
            // SDT other (which actually includes definitions for all transport
            // streams).
            (
              _completeTables.HasFlag(TableType.SdtActual) ||
              _completeTables.HasFlag(TableType.SdtOther)
            ) &&
            // For a network scan all seen tables must be complete. Otherwise
            // SDT other may be incomplete as long as SDT actual is complete.
            // We assume that this condition will ensure NIT and/or BAT are
            // complete if available.
            (
              _seenTables == _completeTables ||
              (!isFastNetworkScan && _seenTables == (_completeTables | TableType.SdtOther))
            ) &&
            // Freesat tables must all be complete... or not seen at all.
            (
              (!_seenTables.HasFlag(TableType.FreesatSdt) && !_seenTables.HasFlag(TableType.FreesatNit) && !_seenTables.HasFlag(TableType.FreesatBat)) ||
              _completeTables.HasFlag(TableType.FreesatSdt | TableType.FreesatNit | TableType.FreesatBat)
            )
          )
          {
            this.LogInfo("scan DVB: scan completed, tables complete = [{0}]", _completeTables);
            break;
          }

          remainingTime = _timeLimitSingleTransmitter - (int)(DateTime.Now - start).TotalMilliseconds;
          if (!_event.WaitOne(remainingTime))
          {
            this.LogWarn("scan DVB: scan time limit reached, tables seen = [{0}], tables complete = [{1}]", _seenTables, _completeTables);
            break;
          }
        }
        while (remainingTime > 0);

        // Read MPEG 2 TS program information.
        ushort transportStreamId;
        IDictionary<uint, ProgramInfo> programs;
        CollectPrograms(out transportStreamId, out programs);
        if (_cancelScan)
        {
          return;
        }

        // Construct channels from the DVB service information and MPEG 2 TS
        // program information.
        ushort originalNetworkId;
        IDictionary<uint, ScannedChannel> dvbChannels;
        IDictionary<ChannelGroupType, IDictionary<ulong, string>> dvbGroupNames;
        CollectServices(_grabberDvb, channel, transportStreamId, programs, isFastNetworkScan, out originalNetworkId, out dvbChannels, out dvbGroupNames);
        if (_cancelScan)
        {
          return;
        }

        // Construct channels from the Freesat service information and MPEG 2
        // TS program information.
        IDictionary<uint, ScannedChannel> freesatChannels;
        IDictionary<ChannelGroupType, IDictionary<ulong, string>> freesatGroupNames;
        if (_grabberFreesat == null)
        {
          freesatChannels = new Dictionary<uint, ScannedChannel>(0);
          freesatGroupNames = new Dictionary<ChannelGroupType, IDictionary<ulong, string>>(0);
        }
        else
        {
          ushort freesatOnid;
          CollectServices(_grabberFreesat, channel, transportStreamId, programs, isFastNetworkScan, out freesatOnid, out freesatChannels, out freesatGroupNames);
          if (_cancelScan)
          {
            return;
          }
        }

        // Combine the DVB and Freesat channel and group information.
        IDictionary<uint, ScannedChannel> finalChannels;
        if (_preferProvider2ChannelDetails)
        {
          CombineChannels(freesatChannels, dvbChannels);
          CombineGroupNames(freesatGroupNames, dvbGroupNames);
          finalChannels = freesatChannels;
          groupNames = freesatGroupNames;
        }
        else
        {
          CombineChannels(dvbChannels, freesatChannels);
          CombineGroupNames(dvbGroupNames, freesatGroupNames);
          finalChannels = dvbChannels;
          groupNames = dvbGroupNames;
        }

        // Add channels for programs that don't have SDT information.
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

            ChannelDvbBase dvbChannel = newChannel as ChannelDvbBase;
            if (dvbChannel != null)
            {
              dvbChannel.OriginalNetworkId = originalNetworkId;
            }

            ScannedChannel scannedChannel = new ScannedChannel(newChannel);
            scannedChannel.IsVisibleInGuide = true;
            finalChannels[program.Key] = scannedChannel;
          }
        }

        // Assign names and LCNs for channels that don't already have them.
        foreach (var c in finalChannels)
        {
          if (string.IsNullOrEmpty(c.Value.Channel.LogicalChannelNumber))
          {
            c.Value.Channel.LogicalChannelNumber = c.Value.Channel.DefaultLogicalChannelNumber;
          }

          if (string.IsNullOrEmpty(c.Value.Channel.Name))
          {
            c.Value.Channel.Name = GetNameForChannel(c.Value.Channel);
          }
        }

        channels = finalChannels.Values.ToList();
      }
      finally
      {
        _grabberMpeg.SetCallBack(null);
        _grabberDvb.SetCallBack(null);
        if (_grabberFreesat != null)
        {
          _grabberFreesat.SetCallBack(null);
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
    public virtual IList<TuningDetail> ScanNetworkInformation(IChannel channel)
    {
      IList<TuningDetail> tuningDetails = new List<TuningDetail>();
      if (_grabberMpeg == null || _grabberDvb == null)
      {
        this.LogError("scan DVB: grabber interfaces not available, not possible to scan");
        return tuningDetails;
      }

      try
      {
        _cancelScan = false;
        _isScanning = true;
        _event = new AutoResetEvent(false);
        _seenTables = TableType.None;
        _completeTables = TableType.None;
        _grabberMpeg.SetCallBack(this);
        _grabberDvb.SetCallBack(this);
        if (_grabberFreesat != null)
        {
          _grabberFreesat.SetCallBack(this);
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
            return tuningDetails;
          }
          remainingTime = _minimumTime - (int)(DateTime.Now - start).TotalMilliseconds;
        }

        if (!_seenTables.HasFlag(TableType.NitActual) && !_seenTables.HasFlag(TableType.NitOther))
        {
          this.LogInfo("scan DVB: NIT not available");
          return tuningDetails;
        }

        // Wait for scanning to complete.
        do
        {
          if (_cancelScan)
          {
            return tuningDetails;
          }

          // Check for scan completion.
          if (
            // PAT and all available NIT must have been received.
            _completeTables.HasFlag(TableType.Pat) &&
            (!_seenTables.HasFlag(TableType.NitActual) || _completeTables.HasFlag(TableType.NitActual)) &&
            (!_seenTables.HasFlag(TableType.NitOther) || _completeTables.HasFlag(TableType.NitOther)) &&
            (!_seenTables.HasFlag(TableType.FreesatNit) || _completeTables.HasFlag(TableType.FreesatNit))
          )
          {
            this.LogInfo("scan DVB: NIT scan completed, tables complete = [{0}]", _completeTables);
            break;
          }

          remainingTime = _timeLimitNetworkInformation - (int)(DateTime.Now - start).TotalMilliseconds;
          if (!_event.WaitOne(remainingTime))
          {
            this.LogWarn("scan DVB: NIT scan time limit reached, tables seen = [{0}], tables complete = [{1}]", _seenTables, _completeTables);
            break;
          }
        }
        while (remainingTime > 0);

        tuningDetails = CollectTransmitters(_grabberDvb);
        if (_grabberFreesat != null)
        {
          IList<TuningDetail> freesatTuningDetails = CollectTransmitters(_grabberFreesat);
          foreach (TuningDetail td in freesatTuningDetails)
          {
            tuningDetails.Add(td);
          }
        }

        return tuningDetails;
      }
      finally
      {
        _grabberMpeg.SetCallBack(null);
        _grabberDvb.SetCallBack(null);
        if (_grabberFreesat != null)
        {
          _grabberFreesat.SetCallBack(null);
        }
        _event.Close();
        _event = null;
        _isScanning = false;
      }
    }

    /// <summary>
    /// Abort scanning for channels and/or network information.
    /// </summary>
    public void AbortScanning()
    {
      this.LogInfo("scan DVB: abort");
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

    #region protected virtual functions

    /// <summary>
    /// Get a name for a channel that would otherwise be nameless.
    /// </summary>
    /// <param name="channel">The nameless channel.</param>
    /// <returns>a name for the channel</returns>
    protected virtual string GetNameForChannel(IChannel channel)
    {
      ChannelStream streamChannel = channel as ChannelStream;
      if (streamChannel != null)
      {
        // Streams often don't have meaningful PSI. Just use the URL in those
        // cases.
        return streamChannel.Url;
      }

      // Try to use "Unknown <frequency>.<program number>". At least that way
      // people can see which transmitter the channel came from.
      ChannelMpeg2Base mpeg2Channel = channel as ChannelMpeg2Base;
      IChannelPhysical physicalChannel = channel as IChannelPhysical;
      if (mpeg2Channel != null && physicalChannel != null)
      {
        return string.Format("Unknown {0}.{1}", (int)(physicalChannel.Frequency / 1000), mpeg2Channel.ProgramNumber);
      }
      if (mpeg2Channel != null)
      {
        return string.Format("Unknown {0}", mpeg2Channel.ProgramNumber);
      }

      // In theory this code should never be reached.
      if (physicalChannel != null)
      {
        return string.Format("Unknown {0}", (int)(physicalChannel.Frequency / 1000));
      }
      return "Unknown Non-MPEG 2";
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
        case 0x11:
          switch (tableId)
          {
            case 0x40:
              return TableType.NitActual;
            case 0x41:
              return TableType.NitOther;
            case 0x42:
              return TableType.SdtActual;
            case 0x46:
              return TableType.SdtOther;
            case 0x4a:
              return TableType.Bat;
          }
          break;
        default:
          switch (tableId)
          {
            case 0x41:
              return TableType.FreesatNit;
            case 0x46:
              return TableType.FreesatSdt;
            case 0x4a:
              return TableType.FreesatBat;
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
      this.LogInfo("scan DVB: TSID = {0}, network PID = {1}, program count = {2}", transportStreamId, networkPid, programCount);
      programs = new Dictionary<uint, ProgramInfo>(programCount);
      if (programCount == 0)
      {
        return;
      }

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
          this.LogWarn("scan DVB: failed to get MPEG 2 program, index = {0}", i);
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
    /// Collect the service information from a DVB or Freesat service
    /// description table.
    /// </summary>
    /// <remarks>
    /// SDT service information is supplemented with MPEG 2 program information.
    /// </remarks>
    /// <param name="grabber">The service information grabber.</param>
    /// <param name="tuningChannel">The tuning details used to tune the current transport stream.</param>
    /// <param name="currentTransportStreamId">The current transport stream's identifier.</param>
    /// <param name="programs">A dictionary of programs, keyed on the transport stream identifier and program number.</param>
    /// <param name="isFastNetworkScan"><c>True</c> if performing a fast scan using network information.</param>
    /// <param name="currentTransportStreamOriginalNetworkId">The identifier of the original network which the current transport stream is associated with.</param>
    /// <param name="channels">A dictionary of channels, keyed on the transport stream identifier and program number.</param>
    /// <param name="groupNames">A dictionary of channel group names.</param>
    private void CollectServices(IGrabberSiDvb grabber, IChannel tuningChannel, ushort currentTransportStreamId, IDictionary<uint, ProgramInfo> programs, bool isFastNetworkScan, out ushort currentTransportStreamOriginalNetworkId, out IDictionary<uint, ScannedChannel> channels, out IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      ushort serviceCount;
      grabber.GetServiceCount(out currentTransportStreamOriginalNetworkId, out serviceCount);
      this.LogInfo("scan DVB: ONID = {0}, service count = {1}", currentTransportStreamOriginalNetworkId, serviceCount);

      channels = new Dictionary<uint, ScannedChannel>(serviceCount);
      groupNames = new Dictionary<ChannelGroupType, IDictionary<ulong, string>>(50);
      if (serviceCount == 0)
      {
        return;
      }

      groupNames[ChannelGroupType.BroadcastStandard] = new Dictionary<ulong, string>(50);
      groupNames[ChannelGroupType.ChannelProvider] = new Dictionary<ulong, string>(serviceCount);

      IDictionary<ushort, IDictionary<ushort, IChannel>> tuningChannels;
      if (isFastNetworkScan)
      {
        tuningChannels = DetermineTransportStreamTuningDetails(grabber, tuningChannel, currentTransportStreamOriginalNetworkId, currentTransportStreamId);
        if (_cancelScan)
        {
          return;
        }
      }
      else
      {
        tuningChannels = new Dictionary<ushort, IDictionary<ushort, IChannel>>(1);
        if (currentTransportStreamOriginalNetworkId > 0 && currentTransportStreamId > 0)
        {
          tuningChannels.Add(currentTransportStreamOriginalNetworkId, new Dictionary<ushort, IChannel>(1) { { currentTransportStreamId, tuningChannel } });
        }
      }

      int j = 1;
      byte tableId;
      ushort originalNetworkId;
      ushort transportStreamId;
      ushort serviceId;
      ushort referenceServiceId;
      ushort freesatChannelId;
      ushort openTvChannelId;
      ushort logicalChannelNumberCount = COUNT_LOGICAL_CHANNEL_NUMBERS;
      LogicalChannelNumber[] logicalChannelNumbers = new LogicalChannelNumber[logicalChannelNumberCount];
      byte dishSubChannelNumber;
      bool eitScheduleFlag;
      bool eitPresentFollowingFlag;
      RunningStatus runningStatus;
      bool freeCaMode;
      ServiceType serviceType;
      byte serviceNameCount;
      bool visibleInGuide;
      ushort streamCountVideo;
      ushort streamCountAudio;
      bool isHighDefinition;
      bool isStandardDefinition;
      bool isThreeDimensional;
      byte audioLanguageCount = COUNT_AUDIO_LANGUAGES;
      Iso639Code[] audioLanguages = new Iso639Code[audioLanguageCount];
      byte subtitlesLanguageCount = COUNT_SUBTITLES_LANGUAGES;
      Iso639Code[] subtitlesLanguages = new Iso639Code[subtitlesLanguageCount];
      byte networkIdCount = COUNT_NETWORK_IDS;
      ushort[] networkIds = new ushort[networkIdCount];
      byte bouquetIdCount = COUNT_BOUQUET_IDS;
      ushort[] bouquetIds = new ushort[bouquetIdCount];
      byte availableInCountryCount = COUNT_AVAILABLE_IN_COUNTRIES;
      Iso639Code[] availableInCountries = new Iso639Code[availableInCountryCount];
      byte unavailableInCountryCount = COUNT_UNAVAILABLE_IN_COUNTRIES;
      Iso639Code[] unavailableInCountries = new Iso639Code[unavailableInCountryCount];
      byte availableInCellCount = COUNT_AVAILABLE_IN_CELLS;
      uint[] availableInCells = new uint[availableInCellCount];
      byte unavailableInCellCount = COUNT_UNAVAILABLE_IN_CELLS;
      uint[] unavailableInCells = new uint[unavailableInCellCount];
      byte targetRegionIdCount = COUNT_TARGET_REGION_IDS;
      ulong[] targetRegionIds = new ulong[targetRegionIdCount];
      byte freesatRegionIdCount = COUNT_FREESAT_REGION_IDS;
      uint[] freesatRegionIds = new uint[freesatRegionIdCount];
      byte openTvRegionIdCount = COUNT_OPENTV_REGION_IDS;
      uint[] openTvRegionIds = new uint[openTvRegionIdCount];
      byte freesatChannelCategoryIdCount = COUNT_FREESAT_CHANNEL_CATEGORY_IDS;
      ushort[] freesatChannelCategoryIds = new ushort[freesatChannelCategoryIdCount];
      byte virginMediaChannelCategoryId;
      ushort dishNetworkMarketId;
      byte norDigChannelListIdCount = COUNT_NORDIG_CHANNEL_LIST_IDS;
      byte[] norDigChannelListIds = new byte[norDigChannelListIdCount];
      ushort previousOriginalNetworkId;
      ushort previousTransportStreamId;
      ushort previousServiceId;
      ushort epgOriginalNetworkId;
      ushort epgTransportStreamId;
      ushort epgServiceId;
      for (ushort i = 0; i < serviceCount; i++)
      {
        if (_cancelScan)
        {
          channels.Clear();
          groupNames.Clear();
          return;
        }

        logicalChannelNumberCount = COUNT_LOGICAL_CHANNEL_NUMBERS;
        audioLanguageCount = COUNT_AUDIO_LANGUAGES;
        subtitlesLanguageCount = COUNT_SUBTITLES_LANGUAGES;
        networkIdCount = COUNT_NETWORK_IDS;
        bouquetIdCount = COUNT_BOUQUET_IDS;
        availableInCountryCount = COUNT_AVAILABLE_IN_COUNTRIES;
        unavailableInCountryCount = COUNT_UNAVAILABLE_IN_COUNTRIES;
        availableInCellCount = COUNT_AVAILABLE_IN_CELLS;
        unavailableInCellCount = COUNT_UNAVAILABLE_IN_CELLS;
        targetRegionIdCount = COUNT_TARGET_REGION_IDS;
        freesatRegionIdCount = COUNT_FREESAT_REGION_IDS;
        openTvRegionIdCount = COUNT_OPENTV_REGION_IDS;
        freesatChannelCategoryIdCount = COUNT_FREESAT_CHANNEL_CATEGORY_IDS;
        norDigChannelListIdCount = COUNT_NORDIG_CHANNEL_LIST_IDS;
        if (!grabber.GetService(i,
                                out tableId, out originalNetworkId, out transportStreamId, out serviceId, out referenceServiceId,
                                out freesatChannelId, out openTvChannelId,
                                logicalChannelNumbers, ref logicalChannelNumberCount, out dishSubChannelNumber,
                                out eitScheduleFlag, out eitPresentFollowingFlag,
                                out runningStatus, out freeCaMode, out serviceType, out serviceNameCount, out visibleInGuide,
                                out streamCountVideo, out streamCountAudio,
                                out isHighDefinition, out isStandardDefinition, out isThreeDimensional,
                                audioLanguages, ref audioLanguageCount,
                                subtitlesLanguages, ref subtitlesLanguageCount,
                                networkIds, ref networkIdCount,
                                bouquetIds, ref bouquetIdCount,
                                availableInCountries, ref availableInCountryCount, unavailableInCountries, ref unavailableInCountryCount,
                                availableInCells, ref availableInCellCount, unavailableInCells, ref unavailableInCellCount,
                                targetRegionIds, ref targetRegionIdCount,
                                freesatRegionIds, ref freesatRegionIdCount,
                                openTvRegionIds, ref openTvRegionIdCount,
                                freesatChannelCategoryIds, ref freesatChannelCategoryIdCount,
                                out virginMediaChannelCategoryId,
                                out dishNetworkMarketId,
                                norDigChannelListIds, ref norDigChannelListIdCount,
                                out previousOriginalNetworkId, out previousTransportStreamId, out previousServiceId,
                                out epgOriginalNetworkId, out epgTransportStreamId, out epgServiceId))
        {
          this.LogWarn("scan DVB: failed to get service, index = {0}", i);
          break;
        }

        if (
          !isFastNetworkScan &&
          (
            (_seenTables.HasFlag(TableType.SdtActual) && tableId != 0x42) ||
            (!_seenTables.HasFlag(TableType.SdtActual) && (tableId != 0x46 || transportStreamId != currentTransportStreamId))
          )
        )
        {
          // Service not from the current/actual transport stream => ignore.
          continue;
        }

        this.LogInfo("  {0, -3}: table ID = {1, -2}, ONID = {2, -5}, TSID = {3, -5}, service ID = {4, -5}, ref. service ID = {5, -5}, Freesat CID = {6, -5}, OpenTV CID = {7, -5}",
                      j++, tableId, originalNetworkId, transportStreamId, serviceId, referenceServiceId, freesatChannelId, openTvChannelId);

        List<string> serviceNames;
        List<string> providerNames;
        List<string> nameLanguages;
        CollectServiceNames(grabber, i, serviceNameCount, out serviceNames, out providerNames, out nameLanguages);
        this.LogInfo("    name count = {0, -2}, service names = [{1}], provider names = [{2}], languages = [{3}]", serviceNameCount, string.Join(", ", serviceNames), string.Join(", ", providerNames), string.Join(", ", nameLanguages));

        HashSet<ushort> distinctLcns = new HashSet<ushort>();
        ushort lcn = 0;
        for (ushort n = 0; n < logicalChannelNumberCount; n++)
        {
          ushort tempLcn = logicalChannelNumbers[n].ChannelNumber;
          if (tempLcn != 0 && tempLcn != 0xffff)
          {
            lcn = tempLcn;
            distinctLcns.Add(tempLcn);
          }
        }
        this.LogInfo("    LCN count = {0, -3}, distinct values = [{1}], Dish sub-channel number = {2}", logicalChannelNumberCount, string.Join(", ", distinctLcns), dishSubChannelNumber);

        this.LogInfo("    EIT schedule = {0, -5}, EIT P/F = {1, -5}, free CA mode = {2, -5}, visible in guide = {3, -5}, running status = {4}, service type = {5}",
                      eitScheduleFlag, eitPresentFollowingFlag, freeCaMode, visibleInGuide, runningStatus, serviceType);
        this.LogInfo("    video stream count = {0}, audio stream count = {1}, is HD = {2, -5}, is SD = {3, -5}, is 3D = {4, -5}",
                      streamCountVideo, streamCountAudio, isHighDefinition, isStandardDefinition, isThreeDimensional);
        if (previousOriginalNetworkId > 0)
        {
          this.LogInfo("    previous identifiers, ONID = {0, -5}, TSID = {1, -5}, service ID = {2, -5}", previousOriginalNetworkId, previousTransportStreamId, previousServiceId);
        }
        if (epgOriginalNetworkId > 0)
        {
          this.LogInfo("    EPG service identifiers, ONID = {0, -5}, TSID = {1, -5}, service ID = {2, -5}", epgOriginalNetworkId, epgTransportStreamId, epgServiceId);
        }

        if (audioLanguageCount > 0)
        {
          this.LogDebug("    audio language count           = {0}, languages  = [{1}]", audioLanguageCount, string.Join(", ", audioLanguages.Take(audioLanguageCount)));
        }
        if (subtitlesLanguageCount > 0)
        {
          this.LogDebug("    subtitles language count       = {0}, languages  = [{1}]", subtitlesLanguageCount, string.Join(", ", subtitlesLanguages.Take(subtitlesLanguageCount)));
        }
        if (availableInCountryCount > 0)
        {
          this.LogDebug("    available in country count     = {0}, countries  = [{1}]", availableInCountryCount, string.Join(", ", availableInCountries.Take(availableInCountryCount)));
        }
        if (unavailableInCountryCount > 0)
        {
          this.LogDebug("    unavailable in country count   = {0}, countries  = [{1}]", unavailableInCountryCount, string.Join(", ", unavailableInCountries.Take(unavailableInCountryCount)));
        }
        if (availableInCellCount > 0)
        {
          this.LogDebug("    available in cell count        = {0}, cells      = [{1}]", availableInCellCount, string.Join(", ", availableInCells.Take(availableInCellCount)));
        }
        if (unavailableInCellCount > 0)
        {
          this.LogDebug("    unavailable in cell count      = {0}, cells      = [{1}]", unavailableInCellCount, string.Join(", ", unavailableInCells.Take(unavailableInCellCount)));
        }

        IDictionary<ChannelGroupType, ICollection<ulong>> groups = new Dictionary<ChannelGroupType, ICollection<ulong>>(20);
        if (networkIdCount > 0)
        {
          groups.Add(ChannelGroupType.DvbNetwork, BuildGroup(grabber, groupNames, ChannelGroupType.DvbNetwork, networkIds, networkIdCount));
        }
        if (bouquetIdCount > 0)
        {
          groups.Add(ChannelGroupType.DvbBouquet, BuildGroup(grabber, groupNames, ChannelGroupType.DvbBouquet, bouquetIds, bouquetIdCount));
        }
        if (targetRegionIdCount > 0)
        {
          groups.Add(ChannelGroupType.DvbTargetRegion, BuildGroup(grabber, groupNames, ChannelGroupType.DvbTargetRegion, targetRegionIds, targetRegionIdCount));
        }
        if (freesatRegionIdCount > 0)
        {
          groups.Add(ChannelGroupType.FreesatRegion, BuildGroup(grabber, groupNames, ChannelGroupType.FreesatRegion, freesatRegionIds, freesatRegionIdCount));
        }
        if (openTvRegionIdCount > 0)
        {
          groups.Add(ChannelGroupType.OpenTvRegion, BuildGroup(grabber, groupNames, ChannelGroupType.OpenTvRegion, openTvRegionIds, openTvRegionIdCount));
        }
        if (freesatChannelCategoryIdCount > 0)
        {
          groups.Add(ChannelGroupType.FreesatChannelCategory, BuildGroup(grabber, groupNames, ChannelGroupType.FreesatChannelCategory, freesatChannelCategoryIds, freesatChannelCategoryIdCount));
        }
        if (norDigChannelListIdCount > 0)
        {
          groups.Add(ChannelGroupType.NorDigChannelList, BuildGroup(grabber, groupNames, ChannelGroupType.NorDigChannelList, norDigChannelListIds, norDigChannelListIdCount));
        }

        if (virginMediaChannelCategoryId > 0)
        {
          groups.Add(ChannelGroupType.VirginMediaChannelCategory, new List<ulong> { virginMediaChannelCategoryId });

          string vmCategoryName;
          if (VIRGIN_MEDIA_CHANNEL_CATEGORY_NAMES.TryGetValue(virginMediaChannelCategoryId, out vmCategoryName))
          {
            this.LogDebug("    Virgin Media channel category = {0} [{1}]", vmCategoryName, virginMediaChannelCategoryId);
          }
          else
          {
            this.LogDebug("    Virgin Media channel category = {0}", virginMediaChannelCategoryId);
          }
          if (!groupNames.ContainsKey(ChannelGroupType.VirginMediaChannelCategory))
          {
            groupNames.Add(ChannelGroupType.VirginMediaChannelCategory, VIRGIN_MEDIA_CHANNEL_CATEGORY_NAMES);
          }
        }

        if (dishNetworkMarketId > 0)
        {
          groups.Add(ChannelGroupType.DishNetworkMarket, new List<ulong> { dishNetworkMarketId });

          DishNetworkMarket market = DishNetworkMarket.GetValue(dishNetworkMarketId, _dishNetworkStateAbbreviation);
          if (market != null)
          {
            this.LogDebug("    Dish Network market = {0} [{1}]", market, dishNetworkMarketId);
          }
          else
          {
            this.LogDebug("    Dish Network market = {0}", dishNetworkMarketId);
          }
        }

        uint serviceKey = ((uint)transportStreamId << 16) | serviceId;
        ProgramInfo program = null;
        if (programs != null)
        {
          programs.TryGetValue(serviceKey, out program);
        }

        MediaType mediaType;
        if (
          streamCountVideo > 0 ||
          serviceType == ServiceType.DigitalTelevision ||
          serviceType == ServiceType.NvodTimeShifted ||
          serviceType == ServiceType.Mpeg2HdDigitalTelevision ||
          serviceType == ServiceType.AdvancedCodecSdDigitalTelevision ||
          serviceType == ServiceType.AdvancedCodecSdNvodTimeShifted ||
          serviceType == ServiceType.AdvancedCodecHdDigitalTelevision ||
          serviceType == ServiceType.AdvancedCodecHdNvodTimeShifted ||
          serviceType == ServiceType.AdvancedCodecFrameCompatiblePlanoStereoscopicHdDigitalTelevision ||
          serviceType == ServiceType.AdvancedCodecFrameCompatiblePlanoStereoscopicHdNvodTimeShifted ||
          serviceType == ServiceType.HevcDigitalTelevision ||
          serviceType == ServiceType.SkyGermanyOptionChannel
        )
        {
          mediaType = MediaType.Television;
        }
        else if (
          streamCountAudio > 0 ||
          serviceType == ServiceType.DigitalRadio ||
          serviceType == ServiceType.FmRadio ||
          serviceType == ServiceType.AdvancedCodecDigitalRadio
        )
        {
          mediaType = MediaType.Radio;
        }
        else
        {
          if (program == null || !program.MediaType.HasValue)
          {
            continue;
          }
          mediaType = program.MediaType.Value;
        }

        IDictionary<ushort, IChannel> transportStreamTuningChannels;
        if (!tuningChannels.TryGetValue(originalNetworkId, out transportStreamTuningChannels) || !transportStreamTuningChannels.TryGetValue(transportStreamId, out tuningChannel))
        {
          this.LogWarn("scan DVB: service tuning detail are not available, ONID = {0}, TSID = {1}, service ID = {2}", originalNetworkId, transportStreamId, serviceId);
          continue;
        }

        IChannel newChannel = (IChannel)tuningChannel.Clone();
        if (serviceNames.Count != 0)
        {
          foreach (string name in serviceNames)
          {
            if (!string.IsNullOrEmpty(name))
            {
              newChannel.Name = name;
              break;
            }
          }
        }
        else
        {
          newChannel.Name = string.Empty;
        }
        if (providerNames.Count != 0)
        {
          foreach (string name in providerNames)
          {
            if (!string.IsNullOrEmpty(name))
            {
              newChannel.Provider = name;
              break;
            }
          }
        }
        else
        {
          newChannel.Provider = string.Empty;
        }
        if (distinctLcns.Count == 1)
        {
          if (dishSubChannelNumber != 0)
          {
            newChannel.LogicalChannelNumber = string.Format("{0}.{1}", lcn, dishSubChannelNumber);
          }
          else
          {
            newChannel.LogicalChannelNumber = lcn.ToString();
          }
        }
        else if (distinctLcns.Count > 1)
        {
          newChannel.LogicalChannelNumber = SelectPreferredChannelNumber(logicalChannelNumbers, logicalChannelNumberCount);
        }
        else
        {
          newChannel.LogicalChannelNumber = string.Empty;
        }
        newChannel.MediaType = mediaType;
        newChannel.IsEncrypted = freeCaMode;
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
        newChannel.IsHighDefinition = isHighDefinition && !isStandardDefinition;
        newChannel.IsThreeDimensional = isThreeDimensional;
        if (program != null)
        {
          newChannel.IsThreeDimensional |= program.IsThreeDimensional;
        }

        ChannelMpeg2Base mpeg2Channel = newChannel as ChannelMpeg2Base;
        if (mpeg2Channel != null)
        {
          mpeg2Channel.TransportStreamId = transportStreamId;
          mpeg2Channel.ProgramNumber = serviceId;
          mpeg2Channel.PmtPid = 0;
          if (program != null)
          {
            mpeg2Channel.PmtPid = program.PmtPid;
          }
        }

        ChannelDvbBase dvbChannel = newChannel as ChannelDvbBase;
        if (dvbChannel != null)
        {
          dvbChannel.OriginalNetworkId = originalNetworkId;
          dvbChannel.OpenTvChannelId = openTvChannelId;
          dvbChannel.EpgOriginalNetworkId = epgOriginalNetworkId;
          dvbChannel.EpgTransportStreamId = epgTransportStreamId;
          dvbChannel.EpgServiceId = epgServiceId;
        }

        ChannelDvbS dvbsChannel = newChannel as ChannelDvbS;
        if (dvbsChannel != null)
        {
          dvbsChannel.FreesatChannelId = freesatChannelId;
        }
        else
        {
          ChannelDvbS2 dvbs2Channel = newChannel as ChannelDvbS2;
          if (dvbs2Channel != null)
          {
            dvbs2Channel.FreesatChannelId = freesatChannelId;
          }
        }

        ScannedChannel scannedChannel = new ScannedChannel(newChannel);
        if (
          (program == null || !program.MediaType.HasValue) &&
          (runningStatus != RunningStatus.NotRunning && runningStatus != RunningStatus.ServiceOffAir) &&
          (availableInCellCount != 0 || unavailableInCellCount != 0)
        )
        {
          // PMT not received, the service is running, and cell availability
          // information is available => service not available in this cell.
          scannedChannel.IsVisibleInGuide = false;
        }
        else
        {
          scannedChannel.IsVisibleInGuide = visibleInGuide;
        }
        scannedChannel.PreviousOriginalNetworkId = previousOriginalNetworkId;
        scannedChannel.PreviousTransportStreamId = previousTransportStreamId;
        scannedChannel.PreviousServiceId = previousServiceId;
        foreach (var group in groups)
        {
          scannedChannel.Groups.Add(group.Key, group.Value);
        }

        // Constructed/derived groups.
        BroadcastStandard broadcastStandard = GetBroadcastStandardFromChannelInstance(newChannel);
        if (broadcastStandard != BroadcastStandard.Unknown)
        {
          if (!groupNames[ChannelGroupType.BroadcastStandard].ContainsKey((ulong)broadcastStandard))
          {
            groupNames[ChannelGroupType.BroadcastStandard][(ulong)broadcastStandard] = broadcastStandard.GetDescription();
          }
          scannedChannel.Groups.Add(ChannelGroupType.BroadcastStandard, new List<ulong> { (ulong)broadcastStandard });
        }
        if (!string.IsNullOrEmpty(newChannel.Provider))
        {
          ulong hashCode = (ulong)newChannel.Provider.GetHashCode();
          groupNames[ChannelGroupType.ChannelProvider][hashCode] = newChannel.Provider;
          scannedChannel.Groups.Add(ChannelGroupType.ChannelProvider, new List<ulong> { hashCode });
        }
        if (bouquetIdCount > 0)
        {
          // Freeview Satellite NZ groups are based on bouquets.
          if (string.Equals(RegionInfo.CurrentRegion.EnglishName, "New Zealand"))
          {
            List<ulong> freeviewBouquetIds = new List<ulong>(bouquetIdCount);
            for (byte b = 0; b < bouquetIdCount; b++)
            {
              ushort bouquetId = bouquetIds[b];
              if (System.Enum.IsDefined(typeof(BouquetFreeviewSatellite), bouquetId))
              {
                freeviewBouquetIds.Add(bouquetId);
              }
            }
            if (freeviewBouquetIds.Count > 0)
            {
              Dictionary<ulong, string> freeviewGroupNames = new Dictionary<ulong, string>(freeviewBouquetIds.Count);
              foreach (System.Enum b in System.Enum.GetValues(typeof(BouquetFreeviewSatellite)))
              {
                freeviewGroupNames[Convert.ToUInt64(b)] = b.GetDescription();
              }
              groupNames.Add(ChannelGroupType.FreeviewSatellite, freeviewGroupNames);
              scannedChannel.Groups.Add(ChannelGroupType.FreeviewSatellite, freeviewBouquetIds);
            }
          }
        }
        // TODO: satellite group

        channels.Add(serviceKey, scannedChannel);
      }
    }

    /// <summary>
    /// Collect the names for a DVB or Freesat service description table
    /// service.
    /// </summary>
    /// <param name="grabber">The service information grabber.</param>
    /// <param name="serviceIndex">The service's index.</param>
    /// <param name="nameCount">The number of names available for the service.</param>
    /// <param name="serviceNames">The service's names.</param>
    /// <param name="providerNames">The service's provider's names.</param>
    /// <param name="languages">The languages associated with the <paramref name="serviceNames">service</paramref> and <paramref name="providerNames">provider</paramref> names.</param>
    private static void CollectServiceNames(IGrabberSiDvb grabber, ushort serviceIndex, byte nameCount, out List<string> serviceNames, out List<string> providerNames, out List<string> languages)
    {
      serviceNames = new List<string>(nameCount);
      providerNames = new List<string>(nameCount);
      languages = new List<string>(nameCount);
      if (nameCount == 0)
      {
        return;
      }

      Iso639Code language;
      ushort serviceNameBufferSize;
      IntPtr serviceNameBuffer = Marshal.AllocCoTaskMem(NAME_BUFFER_SIZE);
      ushort providerNameBufferSize;
      IntPtr providerNameBuffer = Marshal.AllocCoTaskMem(NAME_BUFFER_SIZE);
      try
      {
        for (byte i = 0; i < nameCount; i++)
        {
          serviceNameBufferSize = NAME_BUFFER_SIZE;
          providerNameBufferSize = NAME_BUFFER_SIZE;
          if (grabber.GetServiceNameByIndex(serviceIndex, i, out language, providerNameBuffer, ref providerNameBufferSize, serviceNameBuffer, ref serviceNameBufferSize))
          {
            string name = DvbTextConverter.Convert(serviceNameBuffer, serviceNameBufferSize);
            if (name == null)
            {
              name = string.Empty;
            }
            serviceNames.Add(name.Trim());
            name = DvbTextConverter.Convert(providerNameBuffer, providerNameBufferSize);
            if (name == null)
            {
              name = string.Empty;
            }
            providerNames.Add(name.Trim());
            languages.Add(language.Code);
          }
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(serviceNameBuffer);
        Marshal.FreeCoTaskMem(providerNameBuffer);
      }
    }

    /// <summary>
    /// Build a list of the channel group identifiers for a given group type
    /// which a channel is associated with. At the same time, populate the
    /// <paramref name="groupNames">channel group name</paramref> dictionary.
    /// </summary>
    /// <param name="grabber">The service information grabber (used to retrieve many group names).</param>
    /// <param name="groupNames">A dictionary of channel group names.</param>
    /// <param name="groupType">The channel group type.</param>
    /// <param name="groupIds">The channel group identifiers. This array is usually larger than <paramref name="groupCount"/>.</param>
    /// <param name="groupCount">The number of valid <paramref name="groupIds">channel group identifiers</paramref>.</param>
    /// <returns>a list of the valid channel group identifiers from <paramref name="groupIds"/></returns>
    private static List<ulong> BuildGroup(IGrabberSiDvb grabber, IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames, ChannelGroupType groupType, Array groupIds, byte groupCount)
    {
      List<ulong> groupIdList = new List<ulong>(groupCount);
      List<string> logNames = new List<string>(groupCount);
      if (groupCount == 0)
      {
        return groupIdList;
      }

      IDictionary<ulong, string> names;
      if (!groupNames.TryGetValue(groupType, out names))
      {
        names = new Dictionary<ulong, string>(100);
        groupNames.Add(groupType, names);
      }

      string countryName = RegionInfo.CurrentRegion.EnglishName;
      string logFormat = string.Empty;
      Iso639Code language;
      ushort nameBufferSize;
      IntPtr nameBuffer = Marshal.AllocCoTaskMem(NAME_BUFFER_SIZE);
      try
      {
        for (byte i = 0; i < groupCount; i++)
        {
          ulong groupId = Convert.ToUInt64(groupIds.GetValue(i));
          string groupIdString = groupId.ToString();
          groupIdList.Add(groupId);

          string nameString = null;
          if (!names.TryGetValue(groupId, out nameString))
          {
            nameBufferSize = NAME_BUFFER_SIZE;
            if (groupType == ChannelGroupType.DvbNetwork)
            {
              logFormat = "    network count                  = {0}, networks   = [{1}]";
              if (grabber.GetNetworkNameCount((ushort)groupId) > 0 && grabber.GetNetworkNameByIndex((ushort)groupId, 0, out language, nameBuffer, ref nameBufferSize))
              {
                nameString = DvbTextConverter.Convert(nameBuffer, nameBufferSize);
              }
            }
            else if (groupType == ChannelGroupType.DvbBouquet)
            {
              logFormat = "    bouquet count                  = {0}, bouquets   = [{1}]";
              if (grabber.GetBouquetNameCount((ushort)groupId) > 0 && grabber.GetBouquetNameByIndex((ushort)groupId, 0, out language, nameBuffer, ref nameBufferSize))
              {
                nameString = DvbTextConverter.Convert(nameBuffer, nameBufferSize);
              }
            }
            else if (groupType == ChannelGroupType.DvbTargetRegion)
            {
              logFormat = "    target region count            = {0}, regions    = [{1}]";
              if (grabber.GetTargetRegionNameCount(groupId) > 0 && grabber.GetTargetRegionNameByIndex(groupId, 0, out language, nameBuffer, ref nameBufferSize))
              {
                nameString = DvbTextConverter.Convert(nameBuffer, nameBufferSize);
              }
            }
            else if (groupType == ChannelGroupType.FreesatChannelCategory)
            {
              logFormat = "    Freesat channel category count = {0}, categories = [{1}]";
              if (grabber.GetFreesatChannelCategoryNameCount((ushort)groupId) > 0 && grabber.GetFreesatChannelCategoryNameByIndex((ushort)groupId, 0, out language, nameBuffer, ref nameBufferSize))
              {
                nameString = DvbTextConverter.Convert(nameBuffer, nameBufferSize);
              }
            }
            else if (groupType == ChannelGroupType.FreesatRegion)
            {
              logFormat = "    Freesat region count           = {0}, regions    = [{1}]";

              // We can use the grabbed names, but they're not as nice or clear
              // as the names we use for configuration.
              int regionId = (ushort)groupId;     // group ID = (bouquet ID << 16) | region ID; Remove the bouquet ID.
              nameString = ((RegionFreesat)regionId).ToString();
              /*if (grabber.GetFreesatRegionNameCount((ushort)groupId) > 0 && grabber.GetFreesatRegionNameByIndex((ushort)groupId, 0, out language, nameBuffer, ref nameBufferSize))
              {
                nameString = DvbTextConverter.Convert(nameBuffer, nameBufferSize);
              }*/
            }
            else if (groupType == ChannelGroupType.NorDigChannelList)
            {
              logFormat = "    NorDig channel list count      = {0}, lists      = [{1}]";
              if (grabber.GetNorDigChannelListNameCount((byte)groupId) > 0 && grabber.GetNorDigChannelListNameByIndex((byte)groupId, 0, out language, nameBuffer, ref nameBufferSize))
              {
                nameString = DvbTextConverter.Convert(nameBuffer, nameBufferSize);
              }
            }
            else if (groupType == ChannelGroupType.OpenTvRegion)
            {
              logFormat = "    OpenTV region count            = {0}, regions    = [{1}]";
              ushort bouquetId = (ushort)(groupId >> 16);
              ushort regionId = (ushort)groupId;
              groupIdString = string.Format("{0}/{1}", bouquetId, regionId);
              if (string.Equals(countryName, "New Zealand"))
              {
                nameString = ((RegionOpenTvSkyNz)regionId).GetDescription();
              }
              else if (string.Equals(countryName, "Australia"))
              {
                nameString = RegionOpenTvFoxtel.GetValue(regionId, (BouquetOpenTvFoxtel)bouquetId).Region;
              }
              else if (string.Equals(countryName, "Italy"))
              {
                // not known
              }
              else
              {
                // assume Sky UK
                nameString = ((RegionOpenTvSkyUk)regionId).ToString();
              }
            }

            if (!string.IsNullOrWhiteSpace(nameString))
            {
              names[groupId] = nameString.Trim();
            }
          }
          else if (groupType == ChannelGroupType.OpenTvRegion)
          {
            ushort bouquetId = (ushort)(groupId >> 16);
            ushort regionId = (ushort)groupId;
            groupIdString = string.Format("{0}/{1}", bouquetId, regionId);
          }

          if (!string.IsNullOrEmpty(nameString))
          {
            logNames.Add(string.Format("{0} [{1}]", nameString, groupIdString));
          }
          else
          {
            logNames.Add(string.Format("{0}", groupIdString));
          }
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(nameBuffer);
      }

      Log.Debug(logFormat, groupCount, string.Join(", ", logNames));
      return groupIdList;
    }

    /// <summary>
    /// Select the preferred logical/virtual channel number for a channel.
    /// </summary>
    /// <param name="logicalChannelNumbers">The channel number candidates.</param>
    /// <param name="logicalChannelNumberCount">The number of <paramref name="logicalChannelNumbers">candidates</paramref>.</param>
    /// <returns>the preferred number for the channel</returns>
    private string SelectPreferredChannelNumber(LogicalChannelNumber[] logicalChannelNumbers, ushort logicalChannelNumberCount)
    {
      // Priority system:
      // 10 = provider 1 specific region HD
      // 9 = provider 1 specific region SD
      // 8 = provider 1 general region HD
      // 7 = provider 1 general region SD
      // 6 = provider 2 specific region HD
      // 5 = provider 2 specific region SD
      // 4 = provider 2 general region HD
      // 3 = provider 2 general region SD
      // 2 = other HD
      // 1 = other SD
      byte preferredLcnPriority = 0;
      ushort preferredLcn = 0;
      ushort provider1BouquetId = _provider1BouquetId;
      ushort provider1RegionId = _provider1RegionId;
      ushort provider2BouquetId = _provider2BouquetId;
      ushort provider2RegionId = _provider2RegionId;
      if (_preferProvider2ChannelDetails)
      {
        provider1BouquetId = _provider2BouquetId;
        provider1RegionId = _provider2RegionId;
        provider2BouquetId = _provider1BouquetId;
        provider2RegionId = _provider1RegionId;
      }
      for (ushort n = 0; n < logicalChannelNumberCount; n++)
      {
        LogicalChannelNumber lcn = logicalChannelNumbers[n];

        // Ignore invalid channel numbers.
        if (lcn.ChannelNumber == 0 || lcn.ChannelNumber == 0xffff)
        {
          continue;
        }

        byte lcnPriority = 1;
        if (lcn.TableId == 0x4a)
        {
          if (lcn.TableIdExtension == provider1BouquetId)
          {
            if (lcn.RegionId == provider1RegionId)
            {
              lcnPriority = 9;
            }
            else if (lcn.RegionId == 0xffff)    // 0xffff = OpenTV all regions code
            {
              lcnPriority = 7;
            }
          }
          else if (lcn.TableIdExtension == provider2BouquetId)
          {
            if (lcn.RegionId == provider2RegionId)
            {
              lcnPriority = 5;
            }
            else if (lcn.RegionId == 0xffff)
            {
              lcnPriority = 3;
            }
          }
        }
        if (_preferHighDefinitionChannelNumbers && lcn.IsHighDefinition)
        {
          lcnPriority++;
        }

        if (preferredLcnPriority < lcnPriority || (preferredLcnPriority == lcnPriority && lcn.ChannelNumber < preferredLcn))
        {
          preferredLcn = lcn.ChannelNumber;
          preferredLcnPriority = lcnPriority;
        }
      }
      return preferredLcn.ToString();
    }

    private static BroadcastStandard GetBroadcastStandardFromChannelInstance(IChannel channel)
    {
      if (channel is ChannelAnalogTv)
      {
        return BroadcastStandard.AnalogTelevision;
      }
      if (channel is ChannelAtsc)
      {
        return BroadcastStandard.Atsc;
      }
      if (channel is ChannelCapture)
      {
        return BroadcastStandard.ExternalInput;
      }
      if (channel is ChannelDigiCipher2)
      {
        return BroadcastStandard.DigiCipher2;
      }
      if (channel is ChannelDvbC)
      {
        return BroadcastStandard.DvbC;
      }
      if (channel is ChannelDvbC2)
      {
        return BroadcastStandard.DvbC2;
      }
      if (channel is ChannelDvbS)
      {
        return BroadcastStandard.DvbS;
      }
      if (channel is ChannelDvbS2)
      {
        return BroadcastStandard.DvbS2;
      }
      if (channel is ChannelDvbT)
      {
        return BroadcastStandard.DvbT;
      }
      if (channel is ChannelDvbT2)
      {
        return BroadcastStandard.DvbT2;
      }
      if (channel is ChannelFmRadio)
      {
        return BroadcastStandard.FmRadio;
      }
      if (channel is ChannelSatelliteTurboFec)
      {
        return BroadcastStandard.SatelliteTurboFec;
      }
      if (channel is ChannelScte)
      {
        return BroadcastStandard.Scte;
      }
      if (channel is ChannelStream)
      {
        return BroadcastStandard.DvbIp;
      }
      return BroadcastStandard.Unknown;
    }

    /// <summary>
    /// Combine two sets of channels.
    /// </summary>
    /// <remarks>
    /// Some channel definitions in each set may be unique; others may need to
    /// be merged. Some details within a channel definition may be unique;
    /// others may be common/shared.
    /// </remarks>
    /// <param name="preferredChannels">The first channel set containing the preferred details.</param>
    /// <param name="secondaryChannels">The second channel set containing alternative/secondary details.</param>
    private void CombineChannels(IDictionary<uint, ScannedChannel> preferredChannels, IDictionary<uint, ScannedChannel> secondaryChannels)
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

        ChannelDvbBase preferredDvbChannel = preferredChannel as ChannelDvbBase;
        if (preferredDvbChannel != null)
        {
          ChannelDvbBase secondaryDvbChannel = secondaryChannel as ChannelDvbBase;
          if (preferredDvbChannel.OpenTvChannelId <= 0)
          {
            preferredDvbChannel.OpenTvChannelId = secondaryDvbChannel.OpenTvChannelId;
          }
          if (preferredDvbChannel.EpgOriginalNetworkId <= 0)
          {
            preferredDvbChannel.EpgOriginalNetworkId = secondaryDvbChannel.EpgOriginalNetworkId;
            preferredDvbChannel.EpgTransportStreamId = secondaryDvbChannel.EpgTransportStreamId;
            preferredDvbChannel.EpgServiceId = secondaryDvbChannel.EpgServiceId;
          }
        }

        ChannelDvbS preferredDvbsChannel = preferredChannel as ChannelDvbS;
        if (preferredDvbsChannel != null && preferredDvbsChannel.FreesatChannelId <= 0)
        {
          preferredDvbsChannel.FreesatChannelId = (secondaryChannel as ChannelDvbS).FreesatChannelId;
        }
        else
        {
          ChannelDvbS2 preferredDvbs2Channel = preferredChannel as ChannelDvbS2;
          if (preferredDvbs2Channel != null && preferredDvbs2Channel.FreesatChannelId <= 0)
          {
            preferredDvbs2Channel.FreesatChannelId = (secondaryChannel as ChannelDvbS2).FreesatChannelId;
          }
        }

        if (preferredScannedChannel.PreviousOriginalNetworkId <= 0)
        {
          preferredScannedChannel.PreviousOriginalNetworkId = secondaryScannedChannel.Value.PreviousOriginalNetworkId;
          preferredScannedChannel.PreviousTransportStreamId = secondaryScannedChannel.Value.PreviousTransportStreamId;
          preferredScannedChannel.PreviousServiceId = secondaryScannedChannel.Value.PreviousServiceId;
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
    /// Determine the correct tuning details for each transport stream in a DVB
    /// or Freesat network.
    /// </summary>
    /// <param name="grabber">The network information grabber.</param>
    /// <param name="currentTuningChannel">The tuning details used to tune the current transport stream.</param>
    /// <param name="currentOriginalNetworkId">The identifier of the original network which the current transport stream is associated with.</param>
    /// <param name="currentTransportStreamId">The current transport stream's identifier.</param>
    /// <returns>a dictionary of the tuning channels for each transport stream, keyed on original network identifier (primary) and transport stream identifier (secondary)</returns>
    private IDictionary<ushort, IDictionary<ushort, IChannel>> DetermineTransportStreamTuningDetails(IGrabberSiDvb grabber, IChannel currentTuningChannel, ushort currentOriginalNetworkId, ushort currentTransportStreamId)
    {
      IList<TuningDetail> tuningDetails = CollectTransmitters(grabber);

      this.LogInfo("scan DVB: determine correct tuning details for other transport streams");

      // Sort tuning details into a dictionary keyed on ONID/TSID.
      Dictionary<uint, List<TuningDetail>> possibleTuningDetailsByTransportStream = new Dictionary<uint, List<TuningDetail>>(tuningDetails.Count);
      foreach (TuningDetail td in tuningDetails)
      {
        uint key = ((uint)td.OriginalNetworkId << 16) | td.TransportStreamId;
        List<TuningDetail> transportStreamTuningDetails;
        if (!possibleTuningDetailsByTransportStream.TryGetValue(key, out transportStreamTuningDetails))
        {
          transportStreamTuningDetails = new List<TuningDetail>(5);
          possibleTuningDetailsByTransportStream[key] = transportStreamTuningDetails;
        }
        transportStreamTuningDetails.Add(td);
      }

      // Build a dictionary of the actual tuning channel for each transport stream.
      HashSet<IChannel> seenTuningChannels = new HashSet<IChannel>() { currentTuningChannel };
      Dictionary<ushort, IDictionary<ushort, IChannel>> transportStreamTuningChannels = new Dictionary<ushort, IDictionary<ushort, IChannel>>(possibleTuningDetailsByTransportStream.Count);
      transportStreamTuningChannels.Add(currentOriginalNetworkId, new Dictionary<ushort, IChannel>(tuningDetails.Count) { { currentTransportStreamId, currentTuningChannel } });
      foreach (var transportStream in possibleTuningDetailsByTransportStream)
      {
        ushort targetOriginalNetworkId = (ushort)(transportStream.Key >> 16);
        ushort targetTransportStreamId = (ushort)transportStream.Key;
        if (targetOriginalNetworkId == currentOriginalNetworkId && targetTransportStreamId == currentTransportStreamId)
        {
          // We already have the tuning channel for the current transport stream.
          continue;
        }

        IDictionary<ushort, IChannel> networkTransportStreams;
        if (!transportStreamTuningChannels.TryGetValue(targetOriginalNetworkId, out networkTransportStreams))
        {
          networkTransportStreams = new Dictionary<ushort, IChannel>(possibleTuningDetailsByTransportStream.Count);
          transportStreamTuningChannels.Add(targetOriginalNetworkId, networkTransportStreams);
        }

        // If we only have one possible tuning detail with one frequency then
        // it must be correct.
        IChannel tuningChannel;
        if (transportStream.Value.Count == 1 && (transportStream.Value[0].Frequencies == null || transportStream.Value[0].Frequencies.Count == 0))
        {
          tuningChannel = transportStream.Value[0].GetTuningChannel();
          networkTransportStreams.Add(targetTransportStreamId, tuningChannel);
          seenTuningChannels.Add(tuningChannel);
          continue;
        }

        // Otherwise we must check each tuning detail + frequency combination.
        bool foundTransportStream = false;
        foreach (TuningDetail td in transportStream.Value)
        {
          // Build a list of the valid frequencies for the tuning detail.
          IList<Tuple<int, bool>> frequencies = new List<Tuple<int, bool>>(td.Frequencies.Count * 3);
          foreach (int frequency in td.Frequencies)
          {
            if (frequency > 0)
            {
              frequencies.Add(new Tuple<int, bool>(frequency, false));
              if (td.FrequencyOffset != 0)
              {
                frequencies.Add(new Tuple<int, bool>(frequency + td.FrequencyOffset, true));
                frequencies.Add(new Tuple<int, bool>(frequency - td.FrequencyOffset, true));
              }
            }
          }

          // If we only have one possible tuning detail with one valid
          // frequency then it must be correct.
          if (transportStream.Value.Count == 1 && frequencies.Count == 1)
          {
            foundTransportStream = true;
            td.Frequency = frequencies[0].Item1;
            tuningChannel = td.GetTuningChannel();
            networkTransportStreams.Add(targetTransportStreamId, tuningChannel);
            seenTuningChannels.Add(tuningChannel);
            break;
          }

          // For each possible frequency...
          bool skipOffsetFrequencies = false;
          foreach (Tuple<int, bool> frequency in frequencies)
          {
            if (_cancelScan)
            {
              transportStreamTuningChannels.Clear();
              return transportStreamTuningChannels;
            }
            if (frequency.Item2 && skipOffsetFrequencies)
            {
              // Skip offset frequencies when a previous frequency was locked.
              continue;
            }
            skipOffsetFrequencies = false;

            // Tune and check the actual transport stream details.
            td.Frequency = frequency.Item1;
            tuningChannel = td.GetTuningChannel();
            seenTuningChannels.Add(tuningChannel);
            ushort tunedOriginalNetworkId;
            ushort tunedTransportStreamId;
            if (CollectTransportStreamInformation(tuningChannel, grabber, out tunedOriginalNetworkId, out tunedTransportStreamId))
            {
              skipOffsetFrequencies = frequency.Item2;

              // Add the transport stream details to our dictionary.
              if (!transportStreamTuningChannels.TryGetValue(tunedOriginalNetworkId, out networkTransportStreams))
              {
                networkTransportStreams = new Dictionary<ushort, IChannel>(possibleTuningDetailsByTransportStream.Count);
                transportStreamTuningChannels.Add(tunedOriginalNetworkId, networkTransportStreams);
              }
              networkTransportStreams.Add(tunedTransportStreamId, tuningChannel);

              if (tunedOriginalNetworkId == targetOriginalNetworkId && tunedTransportStreamId == targetTransportStreamId)
              {
                // If the transport stream we're receiving is the one we were
                // looking for then there's no need to try any other
                // frequencies.
                foundTransportStream = true;
                break;
              }
            }
          }

          if (foundTransportStream)
          {
            break;
          }
        }

        if (!foundTransportStream)
        {
          this.LogWarn("scan DVB: failed to determine correct tuning details, ONID = {0}, TSID = {1}", targetOriginalNetworkId, targetTransportStreamId);
        }
      }

      return transportStreamTuningChannels;
    }

    /// <summary>
    /// Collect the transmitter information from a DVB or Freesat network
    /// information table.
    /// </summary>
    /// <param name="grabber">The network information grabber.</param>
    /// <returns>the tuning details for each transmitter</returns>
    private IList<TuningDetail> CollectTransmitters(IGrabberSiDvb grabber)
    {
      List<TuningDetail> tuningDetails = new List<TuningDetail>();

      ushort transmitterCount = grabber.GetTransmitterCount();
      this.LogInfo("scan DVB: transmitter count = {0}", transmitterCount);

      byte tableId;
      ushort networkId;
      ushort originalNetworkId;
      ushort transportStreamId;
      bool isHomeTransmitter;
      BroadcastStandard broadcastStandard;
      byte frequencyCount = COUNT_FREQUENCIES;
      uint[] frequencies = new uint[frequencyCount];
      DvbPolarisation polarisation;
      byte modulation;
      uint symbolRate;
      ushort bandwidth;
      FecCodeRateDvbCS innerFecRate;
      DvbRollOffFactor rollOffFactor;
      short longitude;
      ushort cellId;
      byte cellIdExtension;
      bool isMultipleInputStream;
      byte plpId;
      for (ushort i = 0; i < transmitterCount; i++)
      {
        if (_cancelScan)
        {
          tuningDetails.Clear();
          return tuningDetails;
        }

        frequencyCount = COUNT_FREQUENCIES;
        if (!grabber.GetTransmitter(i,
                                    out tableId, out networkId, out originalNetworkId, out transportStreamId,
                                    out isHomeTransmitter, out broadcastStandard,
                                    frequencies, ref frequencyCount,
                                    out polarisation, out modulation, out symbolRate,
                                    out bandwidth, out innerFecRate, out rollOffFactor,
                                    out longitude, out cellId, out cellIdExtension,
                                    out isMultipleInputStream, out plpId))
        {
          this.LogWarn("scan DVB: failed to get transmitter, index = {0}", i);
          break;
        }

        this.LogInfo("  {0, -2}: table ID = {1}, NID = {2, -5}, ONID = {3, -5}, TSID = {4, -5}, is home transmitter = {5, -5}, broadcast standard = {6}",
                      i + 1, tableId, networkId, originalNetworkId, transportStreamId, isHomeTransmitter, broadcastStandard);
        this.LogInfo("    frequency count = {0}, frequencies = [{1} kHz]", frequencyCount, string.Join(" kHz, ", frequencies));
        this.LogInfo("    polarisation = {0}, modulation = {1}, symbol rate = {2, -5} ks/s, bandwidth = {3, -5} kHz, inner FEC rate = {4}, roll-off factor = {5}, longitude = {6}",
                      polarisation, modulation, symbolRate, bandwidth, innerFecRate, rollOffFactor, longitude);
        this.LogInfo("    cell ID = {0, -5}, cell ID extension = {1, -3}, is multiple input stream = {2, -5}, PLP ID = {3}",
                      cellId, cellIdExtension, isMultipleInputStream, plpId);

        TuningDetail tuningDetail = new TuningDetail();
        tuningDetail.BroadcastStandard = broadcastStandard;
        tuningDetail.SymbolRate = (int)symbolRate;
        tuningDetail.Bandwidth = bandwidth;
        tuningDetail.OriginalNetworkId = originalNetworkId;
        tuningDetail.TransportStreamId = transportStreamId;
        if (frequencyCount == 1)
        {
          tuningDetail.Frequency = (int)frequencies[0];
        }
        else
        {
          for (byte f = 0; f < frequencyCount; f++)
          {
            tuningDetail.Frequencies.Add((int)frequencies[f]);
          }
        }
        if (isMultipleInputStream)
        {
          tuningDetail.StreamId = plpId;
        }

        if (broadcastStandard == BroadcastStandard.DvbC)
        {
          ModulationSchemeQam modulationScheme;
          switch (modulation)
          {
            case 1:
              modulationScheme = ModulationSchemeQam.Qam16;
              break;
            case 2:
              modulationScheme = ModulationSchemeQam.Qam32;
              break;
            case 3:
              modulationScheme = ModulationSchemeQam.Qam64;
              break;
            case 4:
              modulationScheme = ModulationSchemeQam.Qam128;
              break;
            case 5:
              modulationScheme = ModulationSchemeQam.Qam256;
              break;
            default:
              this.LogWarn("scan DVB: unsupported DVB-C modulation scheme {0}, falling back to automatic", modulation);
              modulationScheme = ModulationSchemeQam.Automatic;
              break;
          }
          tuningDetail.ModulationScheme = modulationScheme.ToString();
        }
        else if (broadcastStandard == BroadcastStandard.DvbS2)
        {
          switch (rollOffFactor)
          {
            case DvbRollOffFactor.ThirtyFive:
              tuningDetail.RollOffFactor = TveRollOffFactor.ThirtyFive;
              break;
            case DvbRollOffFactor.TwentyFive:
              tuningDetail.RollOffFactor = TveRollOffFactor.TwentyFive;
              break;
            case DvbRollOffFactor.Twenty:
              tuningDetail.RollOffFactor = TveRollOffFactor.Twenty;
              break;
            default:
              this.LogWarn("scan DVB: unsupported DVB-S2 roll-off factor {0}, falling back to automatic", rollOffFactor);
              tuningDetail.RollOffFactor = TveRollOffFactor.Automatic;
              break;
          }
        }
        else if (
          broadcastStandard != BroadcastStandard.DvbC2 &&
          broadcastStandard != BroadcastStandard.DvbS &&
          broadcastStandard != BroadcastStandard.DvbT &&
          broadcastStandard != BroadcastStandard.DvbT2
        )
        {
          throw new TvException("Unsupported transmitter broadcast standard {0}.", broadcastStandard);
        }

        if ((broadcastStandard & BroadcastStandard.MaskSatellite) != 0)
        {
          ModulationSchemePsk modulationScheme;
          switch (modulation)
          {
            case 0:
              modulationScheme = ModulationSchemePsk.Automatic;
              this.LogWarn("scan DVB: automatic satellite modulation specified, not supported by all hardware");
              break;
            case 1:
              modulationScheme = ModulationSchemePsk.Psk4;
              break;
            case 2:
              modulationScheme = ModulationSchemePsk.Psk8;
              break;
            default:
              // 16 QAM and any other unsupported value
              this.LogWarn("scan DVB: unsupported DVB-S/S2 modulation scheme {0}, falling back to automatic", modulation);
              modulationScheme = ModulationSchemePsk.Automatic;
              break;
          }
          tuningDetail.ModulationScheme = modulationScheme.ToString();

          switch (innerFecRate)
          {
            case FecCodeRateDvbCS.Rate1_2:
              tuningDetail.FecCodeRate = FecCodeRate.Rate1_2;
              break;
            case FecCodeRateDvbCS.Rate2_3:
              tuningDetail.FecCodeRate = FecCodeRate.Rate2_3;
              break;
            case FecCodeRateDvbCS.Rate3_4:
              tuningDetail.FecCodeRate = FecCodeRate.Rate3_4;
              break;
            case FecCodeRateDvbCS.Rate5_6:
              tuningDetail.FecCodeRate = FecCodeRate.Rate5_6;
              break;
            case FecCodeRateDvbCS.Rate7_8:
              tuningDetail.FecCodeRate = FecCodeRate.Rate7_8;
              break;
            case FecCodeRateDvbCS.Rate8_9:
              tuningDetail.FecCodeRate = FecCodeRate.Rate8_9;
              break;
            case FecCodeRateDvbCS.Rate3_5:
              tuningDetail.FecCodeRate = FecCodeRate.Rate3_5;
              break;
            case FecCodeRateDvbCS.Rate4_5:
              tuningDetail.FecCodeRate = FecCodeRate.Rate4_5;
              break;
            case FecCodeRateDvbCS.Rate9_10:
              tuningDetail.FecCodeRate = FecCodeRate.Rate9_10;
              break;
            default:
              this.LogWarn("scan DVB: unsupported DVB-S/S2 FEC code rate {0}, falling back to automatic", modulation);
              tuningDetail.FecCodeRate = FecCodeRate.Automatic;
              break;
          }

          switch (polarisation)
          {
            case DvbPolarisation.LinearHorizontal:
              tuningDetail.Polarisation = TvePolarisation.LinearHorizontal;
              break;
            case DvbPolarisation.LinearVertical:
              tuningDetail.Polarisation = TvePolarisation.LinearVertical;
              break;
            case DvbPolarisation.CircularLeft:
              tuningDetail.Polarisation = TvePolarisation.CircularLeft;
              break;
            case DvbPolarisation.CircularRight:
              tuningDetail.Polarisation = TvePolarisation.CircularRight;
              break;
            default:
              this.LogWarn("scan DVB: unsupported DVB-S/S2 polarisation {0}, falling back to automatic", polarisation);
              tuningDetail.Polarisation = TvePolarisation.Automatic;
              break;
          }
        }

        tuningDetails.Add(tuningDetail);
      }

      return tuningDetails;
    }

    /// <summary>
    /// Collect information about the current transport stream.
    /// </summary>
    /// <param name="tuningChannel">The tuning details used to tune the current transport stream.</param>
    /// <param name="grabber">The transport stream information grabber.</param>
    /// <param name="originalNetworkId">The identifier of the original network which the current transport stream is associated with.</param>
    /// <param name="transportStreamId">The current transport stream's identifier.</param>
    /// <returns><c>true</c> if the transport stream information is collected successfully, otherwise <c>false</c></returns>
    private bool CollectTransportStreamInformation(IChannel tuningChannel, IGrabberSiDvb grabber, out ushort originalNetworkId, out ushort transportStreamId)
    {
      originalNetworkId = 0;
      transportStreamId = 0;
      try
      {
        _seenTables = TableType.None;
        _completeTables = TableType.None;
        _tuner.Tune(0, tuningChannel);
      }
      catch
      {
        return false;
      }

      if (_cancelScan)
      {
        return false;
      }

      // Wait for scanning to complete.
      DateTime start = DateTime.Now;
      int remainingTime;
      do
      {
        if (_cancelScan)
        {
          return false;
        }

        if (_seenTables.HasFlag(TableType.Pat | TableType.SdtActual))
        {
          break;
        }

        remainingTime = _timeLimitSingleTransmitter - (int)(DateTime.Now - start).TotalMilliseconds;
        if (!_event.WaitOne(remainingTime))
        {
          this.LogWarn("scan DVB: scan time limit reached, tables seen = [{0}], tables complete = [{1}]", _seenTables, _completeTables);
          break;
        }
      }
      while (remainingTime > 0);

      ushort networkPid;
      ushort programCount;
      _grabberMpeg.GetTransportStreamDetail(out transportStreamId, out networkPid, out programCount);
      if (transportStreamId == 0 || programCount == 0)
      {
        return false;
      }
      grabber.GetServiceCount(out originalNetworkId, out programCount);
      if (originalNetworkId == 0 || programCount == 0)
      {
        return false;
      }
      return true;
    }

    #endregion
  }
}