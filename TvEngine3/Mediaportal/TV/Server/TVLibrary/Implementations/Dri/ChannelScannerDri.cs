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
using System.Net;
using System.Threading;
using System.Xml;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc;
using Mediaportal.TV.Server.TVLibrary.Implementations.Scte.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Scte.Parser;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Atsc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  internal class ChannelScannerDri : IChannelScannerInternal
  {
    private enum ScanStage
    {
      NotScanning,
      Mgt,
      Nit,
      Vct,
      Ntt
    }

    public delegate void RequestFdcTablesDelegate(List<byte> tableIds);

    #region constants

    private const int TABLE_REREQUEST_TIMEOUT = 30000;  // unit = ms

    #endregion

    #region variables

    private ITuner _tuner = null;
    private string _tunerIpAddress = null;
    private RequestFdcTablesDelegate _requestFdcTables = null;
    private IChannelScannerHelper _scanHelper = null;
    private bool _isScanning = false;
    private int _scanTimeLimit = 20000;   // unit = milli-seconds

    private volatile ScanStage _scanStage = ScanStage.NotScanning;
    private ManualResetEvent _event = null;
    private volatile bool _cancelScan = false;

    // parsers
    private ParserMgt _parserMgt = new ParserMgt();
    private ParserNit _parserNit = new ParserNit();
    private ParserNtt _parserNtt = new ParserNtt();
    private ParserLvct _parserLvct = new ParserLvct();
    private ParserSvct _parserSvct = new ParserSvct();

    IList<ModulationMode> _modulationModes = null;
    IList<int> _carrierFrequencies = null;
    IDictionary<string, IChannel> _channels = new Dictionary<string, IChannel>();                       // channel number => channel
    IDictionary<ushort, HashSet<string>> _sourceChannels = new Dictionary<ushort, HashSet<string>>();   // source ID => channel numbers
    HashSet<string> _hiddenChannels = new HashSet<string>();        // channel numbers
    HashSet<string> _ignoredChannels = new HashSet<string>();       // channel numbers
    HashSet<ushort> _sourcesWithoutNames = new HashSet<ushort>();   // source IDs

    #endregion

    public ChannelScannerDri(ITuner tuner, string tunerIpAddress, RequestFdcTablesDelegate requestFdcTables)
    {
      _tuner = tuner;
      _tunerIpAddress = tunerIpAddress;
      _requestFdcTables = requestFdcTables;
      _scanHelper = new ChannelScannerHelperAtsc();
    }

    #region delegates

    public void OnTableSection(byte[] section)
    {
      try
      {
        if (!_isScanning || section == null || section.Length < 3)
        {
          return;
        }
        int pid = (section[0] << 8) + section[1];
        byte tableId = section[2];
        this.LogDebug("DRI scan: scan stage = {0}, PID = {1}, table ID = {2}, size = {3}", _scanStage, pid, tableId, section.Length);
        //Dump.DumpBinary(section, section.Length, 0);
        if (tableId == 0xc7)
        {
          _parserMgt.Decode(section);
        }
        else
        {
          switch (_scanStage)
          {
            case ScanStage.Nit:
              if (tableId == 0xc2)
              {
                _parserNit.Decode(section);
              }
              break;
            case ScanStage.Ntt:
              if (tableId == 0xc3)
              {
                _parserNtt.Decode(section);
              }
              break;
            case ScanStage.Vct:
              if (tableId == 0xc8 || tableId == 0xc9)
              {
                _parserLvct.Decode(section);
              }
              else if (tableId == 0xc4)
              {
                _parserSvct.Decode(section);
              }
              break;
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI scan: failed to parse table section");
        if (section != null)
        {
          Dump.DumpBinary(section);
        }
      }
    }

    public void OnTableComplete(MgtTableType tableType)
    {
      this.LogInfo("DRI scan: on table complete, table type = {0}", tableType);
      if (_event != null && _scanStage != ScanStage.NotScanning)
      {
        _event.Set();
      }
    }

    public void OnCarrierDefinition(TransmissionMedium transmissionMedium, byte index, int carrierFrequency)
    {
      if (transmissionMedium != TransmissionMedium.Cable && transmissionMedium != TransmissionMedium.OverTheAir)
      {
        return;
      }
      if (carrierFrequency >= 997250)
      {
        // Charter includes SDV channels in the CableCARD channel map, but
        // assigns them to physical channel 158 (which isn't actually used).
        this.LogDebug("DRI scan: recognised physical channel 158 for Charter SDV handling");
        carrierFrequency = 0;
      }
      _carrierFrequencies[index] = carrierFrequency;    // standard centre frequency
    }

    public void OnModulationMode(TransmissionMedium transmissionMedium, byte index, TransmissionSystem transmissionSystem,
      FecCodeRate innerCodingMode, bool isSplitBitstreamMode, ModulationMode modulationFormat, int symbolRate)
    {
      if (transmissionMedium != TransmissionMedium.Cable && transmissionMedium != TransmissionMedium.OverTheAir)
      {
        return;
      }
      _modulationModes[index] = modulationFormat;
    }

    public void OnLvctChannelDetail(MgtTableType tableType, string shortName, ushort majorChannelNumber, ushort minorChannelNumber,
      ModulationMode modulationMode, uint carrierFrequency, ushort channelTsid, ushort programNumber, EtmLocation etmLocation,
      bool accessControlled, bool hidden, byte pathSelect, bool outOfBand, bool hideGuide, ServiceType serviceType, ushort sourceId)
    {
      string logicalChannelNumber;
      if (minorChannelNumber == 0)
      {
        logicalChannelNumber = majorChannelNumber.ToString();
      }
      else
      {
        logicalChannelNumber = string.Format("{0}.{1}", majorChannelNumber, minorChannelNumber);
      }

      if (outOfBand || (serviceType != ServiceType.Audio && serviceType != ServiceType.DigitalTelevision))
      {
        // Not tunable/supported.
        this.LogWarn("DRI scan: ignoring untunable L-VCT channel entry, out of band = {0}, service type = {1}", outOfBand, serviceType);
        _ignoredChannels.Add(logicalChannelNumber);
        return;
      }

      IChannel channel = null;
      if (_channels.TryGetValue(logicalChannelNumber, out channel))
      {
        return;
      }

      // record the channel details
      int frequency = (int)carrierFrequency / 1000;   // Hz => kHz, standard centre frequency
      if (modulationMode == ModulationMode.Atsc8Vsb || modulationMode == ModulationMode.Atsc16Vsb)
      {
        ChannelAtsc atscChannel = new ChannelAtsc();
        atscChannel.SourceId = sourceId;
        atscChannel.Frequency = frequency;
        switch (modulationMode)
        {
          case ModulationMode.Atsc8Vsb:
            atscChannel.ModulationScheme = ModulationSchemeVsb.Vsb8;
            break;
          case ModulationMode.Atsc16Vsb:
            atscChannel.ModulationScheme = ModulationSchemeVsb.Vsb16;
            break;
        }
        channel = atscChannel;
      }
      else if (modulationMode == ModulationMode.ScteMode1 || modulationMode == ModulationMode.ScteMode2)
      {
        ChannelScte scteChannel = new ChannelScte();
        scteChannel.SourceId = sourceId;
        scteChannel.Frequency = frequency;
        switch (modulationMode)
        {
          case ModulationMode.ScteMode1:
            scteChannel.ModulationScheme = ModulationSchemeQam.Qam64;
            break;
          case ModulationMode.ScteMode2:
            scteChannel.ModulationScheme = ModulationSchemeQam.Qam256;
            break;
        }
        channel = scteChannel;
      }
      else
      {
        this.LogWarn("DRI scan: ignoring L-VCT channel definition, unsupported modulation mode {0}", modulationMode);
        return;
      }

      _channels.Add(logicalChannelNumber, channel);

      channel.Name = shortName;
      if (tableType == MgtTableType.TvctCurrentNext1 || tableType == MgtTableType.TvctCurrentNext0)
      {
        channel.Provider = "Terrestrial";
      }
      else
      {
        channel.Provider = "Cable";
      }
      channel.LogicalChannelNumber = logicalChannelNumber;
      channel.MediaType = _scanHelper.GetMediaType((int)serviceType, 0, 0).Value;
      channel.IsEncrypted = accessControlled;

      ChannelMpeg2Base mpeg2Channel = channel as ChannelMpeg2Base;
      mpeg2Channel.TransportStreamId = channelTsid;
      mpeg2Channel.ProgramNumber = programNumber;
      mpeg2Channel.PmtPid = 0;   // The engine will automatically lookup the correct PID from the PAT when the channel is tuned.

      // progress tracking...
      if (sourceId > 0)
      {
        HashSet<string> sourceChannels;
        if (!_sourceChannels.TryGetValue(sourceId, out sourceChannels))
        {
          sourceChannels = new HashSet<string>();
          _sourceChannels[sourceId] = sourceChannels;
        }
        sourceChannels.Add(logicalChannelNumber);
      }
      if (hidden)
      {
        _hiddenChannels.Add(logicalChannelNumber);
      }
    }

    public void OnSvctChannelDetail(TransmissionMedium transmissionMedium, ushort vctId, ushort virtualChannelNumber, bool applicationVirtualChannel,
      byte bitstreamSelect, byte pathSelect, ChannelType channelType, ushort sourceId, byte cdsReference, ushort programNumber, byte mmsReference)
    {
      string logicalChannelNumber = virtualChannelNumber.ToString();

      if ((transmissionMedium != TransmissionMedium.Cable && transmissionMedium != TransmissionMedium.OverTheAir) || applicationVirtualChannel)
      {
        // Not tunable/supported.
        this.LogWarn("DRI scan: ignoring untunable S-VCT channel entry, transmission medium = {0}, application virtual channel = {1}", transmissionMedium, applicationVirtualChannel);
        _ignoredChannels.Add(logicalChannelNumber);
        return;
      }

      IChannel channel = null;
      if (_channels.TryGetValue(logicalChannelNumber, out channel))
      {
        return;
      }

      // record the channel details
      ModulationMode modulationMode = _modulationModes[mmsReference];
      int frequency = _carrierFrequencies[cdsReference];
      if (modulationMode == ModulationMode.Atsc8Vsb || modulationMode == ModulationMode.Atsc16Vsb)
      {
        ChannelAtsc atscChannel = new ChannelAtsc();
        atscChannel.Provider = "Terrestrial";
        atscChannel.SourceId = sourceId;
        atscChannel.Frequency = frequency;
        switch (modulationMode)
        {
          case ModulationMode.Atsc8Vsb:
            atscChannel.ModulationScheme = ModulationSchemeVsb.Vsb8;
            break;
          case ModulationMode.Atsc16Vsb:
            atscChannel.ModulationScheme = ModulationSchemeVsb.Vsb16;
            break;
        }
        channel = atscChannel;
      }
      else if (modulationMode == ModulationMode.ScteMode1 || modulationMode == ModulationMode.ScteMode2)
      {
        ChannelScte scteChannel = new ChannelScte();
        scteChannel.Provider = "Cable";
        scteChannel.SourceId = sourceId;
        scteChannel.Frequency = frequency;
        switch (modulationMode)
        {
          case ModulationMode.ScteMode1:
            scteChannel.ModulationScheme = ModulationSchemeQam.Qam64;
            break;
          case ModulationMode.ScteMode2:
            scteChannel.ModulationScheme = ModulationSchemeQam.Qam256;
            break;
        }
        channel = scteChannel;
      }
      else
      {
        this.LogWarn("DRI scan: ignoring S-VCT channel definition, unsupported modulation format {0}", modulationMode);
        return;
      }

      _channels.Add(logicalChannelNumber, channel);

      channel.LogicalChannelNumber = logicalChannelNumber;
      channel.MediaType = MediaType.Television;
      channel.IsEncrypted = true;

      ChannelMpeg2Base mpeg2Channel = channel as ChannelMpeg2Base;
      mpeg2Channel.TransportStreamId = 0;   // We don't have this information.
      mpeg2Channel.ProgramNumber = programNumber;
      mpeg2Channel.PmtPid = 0;              // The engine will automatically lookup the correct PID from the PAT when the channel is tuned.

      // progress tracking...
      if (sourceId > 0)
      {
        HashSet<string> sourceChannels;
        if (!_sourceChannels.TryGetValue(sourceId, out sourceChannels))
        {
          sourceChannels = new HashSet<string>();
          _sourceChannels[sourceId] = sourceChannels;
        }
        sourceChannels.Add(logicalChannelNumber);
        _sourcesWithoutNames.Add(sourceId);
      }
      if (channelType == ChannelType.Hidden)
      {
        _hiddenChannels.Add(logicalChannelNumber);
      }
    }

    public void OnSourceName(TransmissionMedium transmissionMedium, bool applicationType, ushort sourceId, string name)
    {
      if ((transmissionMedium != TransmissionMedium.Cable && transmissionMedium != TransmissionMedium.OverTheAir) || applicationType || sourceId <= 0)
      {
        return;
      }

      // Set the name for all channels linked to the source.
      HashSet<string> sourceChannels;
      if (_sourceChannels.TryGetValue(sourceId, out sourceChannels))
      {
        foreach (string logicalChannelNumber in sourceChannels)
        {
          IChannel channel = null;
          if (_channels.TryGetValue(logicalChannelNumber, out channel))
          {
            channel.Name = name;
            _sourcesWithoutNames.Remove(sourceId);
            if (_sourcesWithoutNames.Count == 0)
            {
              this.LogInfo("DRI scan: all sources now have names, assuming NTT is complete");
              OnTableComplete(MgtTableType.NttSns);
            }
          }
        }
      }
    }

    public void OnMgtTableDetail(MgtTableType tableType, int pid, int versionNumber, uint byteCount)
    {
    }

    #endregion

    #region IChannelScannerInternal members

    /// <summary>
    /// Set the scanner's tuner.
    /// </summary>
    public virtual ITuner Tuner
    {
      set
      {
        _tuner = value;
      }
    }

    /// <summary>
    /// Set the scanner's helper.
    /// </summary>
    public IChannelScannerHelper Helper
    {
      set
      {
        _scanHelper = value;
      }
    }

    #endregion

    /// <summary>
    /// Reload the scanner's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      this.LogDebug("DRI scan: reload configuration");
      _scanTimeLimit = SettingsManagement.GetValue("timeLimitScan", 20000);
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
    /// Abort scanning for channels.
    /// </summary>
    public void AbortScanning()
    {
      this.LogInfo("DRI scan: abort");
      try
      {
        _cancelScan = true;
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

    /// <summary>
    /// Tune to a specified channel and scan for channel information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the channel information found</returns>
    public List<IChannel> Scan(IChannel channel)
    {
      _cancelScan = false;
      _channels.Clear();
      _sourceChannels.Clear();
      _hiddenChannels.Clear();
      _ignoredChannels.Clear();
      _sourcesWithoutNames.Clear();
      _modulationModes = new ModulationMode[255];
      _carrierFrequencies = new int[255];
      _event = new ManualResetEvent(false);
      try
      {
        _isScanning = true;
        _tuner.Tune(0, channel);

        // Configure the MGT parser. We don't use MGT info right now but we
        // want to know if it exists and what it contains. Currently I have not
        // encountered a provider which delivers MGT. If such a provider
        // existed then we might be able to pull EPG from the stream... which
        // would be awesome.
        _parserMgt.Reset();
        _parserMgt.OnTableDetail += OnMgtTableDetail;
        _parserMgt.OnTableComplete += OnTableComplete;

        _scanStage = ScanStage.Nit;
        _parserNit.Reset();
        _parserNit.OnCarrierDefinition += OnCarrierDefinition;
        _parserNit.OnModulationMode += OnModulationMode;
        _parserNit.OnTableComplete += OnTableComplete;
        _event.Reset();
        int availableTimeMilliseconds = _scanTimeLimit;
        bool completedStage = false;
        while (!_cancelScan && availableTimeMilliseconds > 0)
        {
          _requestFdcTables(new List<byte> { 0xc2, 0xc7 });
          int waitTime = Math.Min(TABLE_REREQUEST_TIMEOUT, availableTimeMilliseconds);
          if (_event.WaitOne(waitTime))
          {
            completedStage = true;
            break;
          }
          availableTimeMilliseconds -= waitTime;
        }

        if (!completedStage)
        {
          this.LogError("DRI scan: failed to complete NIT scan stage, check firewall(s) allow full access to your tuner and consider increasing SDT/VCT timeout");
          return new List<IChannel>();
        }

        _scanStage = ScanStage.Vct;
        _parserSvct.Reset();
        _parserSvct.OnChannelDetail += OnSvctChannelDetail;
        _parserSvct.OnTableComplete += OnTableComplete;
        _parserLvct.Reset();
        _parserLvct.OnChannelDetail += OnLvctChannelDetail;
        _parserLvct.OnTableComplete += OnTableComplete;
        _event.Reset();
        completedStage = false;
        while (!_cancelScan && availableTimeMilliseconds > 0)
        {
          _requestFdcTables(new List<byte> { 0xc4, 0xc7, 0xc8, 0xc9 });
          int waitTime = Math.Min(TABLE_REREQUEST_TIMEOUT, availableTimeMilliseconds);
          if (_event.WaitOne(waitTime))
          {
            completedStage = true;
            break;
          }
          availableTimeMilliseconds -= waitTime;
        }

        if (!completedStage)
        {
          this.LogError("DRI scan: failed to complete VCT scan stage, consider increasing SDT/VCT timeout");
          return new List<IChannel>();
        }

        _scanStage = ScanStage.Ntt;
        _parserNtt.Reset();
        _parserNtt.OnSourceName += OnSourceName;
        _parserNtt.OnTableComplete += OnTableComplete;
        _event.Reset();
        completedStage = false;
        while (!_cancelScan && availableTimeMilliseconds > 0)
        {
          _requestFdcTables(new List<byte> { 0xc3, 0xc7 });
          int waitTime = Math.Min(TABLE_REREQUEST_TIMEOUT, availableTimeMilliseconds);
          if (_event.WaitOne(waitTime))
          {
            completedStage = true;
            break;
          }
          availableTimeMilliseconds -= waitTime;
        }

        if (!completedStage)
        {
          this.LogWarn("DRI scan: failed to complete NTT scan stage, consider increasing SDT/VCT timeout");
        }
        else if (_cancelScan)
        {
          return new List<IChannel>();
        }

        _scanStage = ScanStage.NotScanning;
        this.LogInfo("DRI scan: stats...");
        this.LogInfo("  scanned = {0}", _channels.Count);
        this.LogInfo("  no name = {0} [{1}]", _sourcesWithoutNames.Count, string.Join(", ", _sourcesWithoutNames));
        this.LogInfo("  hidden  = {0} [{1}]", _hiddenChannels.Count, string.Join(", ", _hiddenChannels));
        this.LogInfo("  ignored = {0} [{1}]", _ignoredChannels.Count, string.Join(", ", _ignoredChannels));

        // Get the SiliconDust proprietary channel list.
        // We use this for two purposes:
        // 1. Filtering out inaccessible channels.
        // 2. Adding channels delivered using switched digital video (SDV).
        //
        // We can determine whether a given channel is accessible or not by
        // trying to tune it directly... but this is incredibly time consuming
        // and error prone for hundreds of channels. Therefore we use the
        // proprietary list instead.
        //
        // Channels delivered via SDV are not meant to be included in the
        // CableCARD's channel list. Sometimes they are, but apparently that
        // is a mistake which we can't rely on. The only certain way to get a
        // complete channel list when SDV is active is to request it from the
        // tuning adaptor/resolver. Only the tuner can communicate directly
        // with the TA/TR, and we have no way to ask the tuner to ask the TA/TR
        // to give us the information. Therefore we have to use the proprietary
        // list.
        this.LogDebug("DRI scan: merge with SiliconDust lineup info...");
        IDictionary<string, string> subscribedAccessible;
        IDictionary<string, string> notSubscribed;
        IDictionary<string, string> notAccessible;
        bool gotSiliconDustInfo = GetSiliconDustChannelSets(out subscribedAccessible, out notSubscribed, out notAccessible);

        // Build/filter the final channel list.
        List<IChannel> channels = new List<IChannel>();
        foreach (IChannel scannedChannel in _channels.Values)
        {
          // If the SiliconDust list is available, filter to subscribed and
          // accessible channels only.
          if (!gotSiliconDustInfo || subscribedAccessible.Remove(scannedChannel.LogicalChannelNumber))
          {
            IChannel c = scannedChannel;
            _scanHelper.UpdateChannel(ref c);
            channels.Add(c);
          }
          else if (!notSubscribed.Remove(channel.LogicalChannelNumber) && !notAccessible.Remove(scannedChannel.LogicalChannelNumber))
          {
            this.LogWarn("DRI scan: unknown channel accessibility, number = {0}, name = {1}", scannedChannel.LogicalChannelNumber, scannedChannel.Name ?? "[unknown]");
            IChannel c = scannedChannel;
            _scanHelper.UpdateChannel(ref c);
            channels.Add(c);
          }
        }

        // Any channels remaining in the SiliconDust subscribed and accessible
        // list are switched digital video channels that weren't in the
        // CableCARD's channel list. Add them. Note these channels won't be
        // tunable by clear QAM tuners even if they're actually not encrypted,
        // because we can't get the physical tuning details.
        if (gotSiliconDustInfo && subscribedAccessible.Count > 0)
        {
          this.LogInfo("  switched digital   = {0}", subscribedAccessible.Count);
          foreach (KeyValuePair<string, string> pair in subscribedAccessible)
          {
            channels.Add(CreateSdvChannel(pair.Value, pair.Key));
          }
        }
        else if (!gotSiliconDustInfo)
        {
          // This may or may not catch SDV channels.
          HashSet<string> sdvChannels = new HashSet<string>();
          foreach (int lcn in _parserSvct.DefinedChannels)
          {
            sdvChannels.Add(lcn.ToString());
          }
          sdvChannels.ExceptWith(_channels.Keys);
          sdvChannels.ExceptWith(_ignoredChannels);
          if (sdvChannels.Count > 0)
          {
            this.LogInfo("  switched digital   = {0}", sdvChannels.Count);
            foreach (string channelNumber in sdvChannels)
            {
              channels.Add(CreateSdvChannel(channelNumber));
            }
          }
        }

        return channels;
      }
      finally
      {
        _event.Close();
        _event = null;
        _isScanning = false;
      }
    }

    /// <summary>
    /// Tune to a specified channel and scan for network information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the network information found</returns>
    public List<TuningDetail> ScanNIT(IChannel channel)
    {
      throw new NotImplementedException();
    }

    private bool GetSiliconDustChannelSets(out IDictionary<string, string> subscribedAccessible, out IDictionary<string, string> notSubscribed, out IDictionary<string, string> notAccessible)
    {
      subscribedAccessible = new Dictionary<string, string>();  // channel number => name
      notSubscribed = new SortedDictionary<string, string>();   // channel number => name
      notAccessible = new Dictionary<string, string>();         // channel number => name

      IDictionary<string, string> subscribedInaccessible;
      if (!GetSiliconDustChannelLineUp(new Uri(new Uri(_tunerIpAddress), "lineup.xml"), out subscribedAccessible, out subscribedInaccessible))
      {
        return false;
      }

      int subscribedCount = subscribedAccessible.Count + notAccessible.Count;
      this.LogInfo("  subscribed         = {0}", subscribedCount);
      this.LogInfo("  subscribed DRM     = {0}", subscribedInaccessible.Count);
      foreach (KeyValuePair<string, string> channel in subscribedInaccessible)
      {
        this.LogDebug("  {0:-6} = {1}", channel.Key, channel.Value);
      }

      IDictionary<string, string> allAccessibleIncludingNotSubscribed;
      if (!GetSiliconDustChannelLineUp(new Uri(new Uri(_tunerIpAddress), "lineup.xml?show=all"), out allAccessibleIncludingNotSubscribed, out notAccessible))
      {
        return false;
      }

      HashSet<string> temp = new HashSet<string>(allAccessibleIncludingNotSubscribed.Keys);
      temp.ExceptWith(subscribedAccessible.Keys);
      this.LogInfo("  not subscribed     = {0}", temp.Count);
      foreach (string channelNumber in temp)
      {
        string name = allAccessibleIncludingNotSubscribed[channelNumber];
        notSubscribed.Add(channelNumber, name);
        this.LogDebug("  {0:-6} = {1}", channelNumber, name);
      }

      temp = new HashSet<string>(notAccessible.Keys);
      temp.ExceptWith(subscribedInaccessible.Keys);
      this.LogInfo("  not subscribed DRM = {0}", temp.Count);
      foreach (string channelNumber in temp)
      {
        this.LogDebug("  {0:-6} = {1}", channelNumber, notAccessible[channelNumber]);
      }
      return true;
    }

    private bool GetSiliconDustChannelLineUp(Uri uri, out IDictionary<string, string> channelsAccessible, out IDictionary<string, string> channelsInaccessible)
    {
      channelsAccessible = new SortedDictionary<string, string>();    // channel number => name
      channelsInaccessible = new SortedDictionary<string, string>();  // channel number => name

      // Request.
      HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
      request.Timeout = 5000;
      HttpWebResponse response = null;
      try
      {
        response = (HttpWebResponse)request.GetResponse();
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "DRI scan: failed to get SiliconDust XML lineup from tuner, URI = ", uri);
        request.Abort();
        return false;
      }

      // Response.
      string content = string.Empty;
      try
      {
        using (Stream responseStream = response.GetResponseStream())
        {
          using (TextReader textReader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
          {
            using (XmlReader xmlReader = XmlReader.Create(textReader))
            {
              string number = string.Empty;
              string name = string.Empty;
              bool isCopyFreely = true;
              while (!xmlReader.EOF)
              {
                if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name.Equals("Program"))
                {
                  if (string.IsNullOrEmpty(number))
                  {
                    if (!isCopyFreely)
                    {
                      channelsInaccessible.Add(number, name);
                    }
                    else
                    {
                      channelsAccessible.Add(number, name);
                    }
                  }
                  number = string.Empty;
                  name = string.Empty;
                  isCopyFreely = true;
                }
                else if (xmlReader.NodeType == XmlNodeType.Element)
                {
                  if (xmlReader.Name.Equals("GuideNumber"))
                  {
                    // I don't know whether SiliconDust would have two-part
                    // channel numbers in this list, or what format they would
                    // have (eg. X.Y or X-Y) if they did. For now assume they
                    // either don't have them or use our format (X.Y).
                    number = xmlReader.ReadElementContentAsString();
                  }
                  else if (xmlReader.Name.Equals("GuideName"))
                  {
                    name = xmlReader.ReadElementContentAsString();
                  }
                  else if ((xmlReader.Name.Equals("DRM") && xmlReader.ReadElementContentAsInt() == 1) ||      // new format
                    xmlReader.Name.Equals("Tags") && xmlReader.ReadElementContentAsString().Contains("drm"))  // old format
                  {
                    isCopyFreely = false;
                  }
                  else
                  {
                    xmlReader.Read();
                  }
                  continue;
                }
                xmlReader.Read();
              }
              xmlReader.Close();
            }
            textReader.Close();
          }
          responseStream.Close();
        }
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI scan: failed to handle SiliconDust XML lineup response from tuner, URI = {0}", uri);
        return false;
      }
      finally
      {
        if (response != null)
        {
          response.Close();
        }
      }
    }

    private IChannel CreateSdvChannel(string number, string name = null)
    {
      ChannelScte channel = new ChannelScte();
      if (string.IsNullOrEmpty(name))
      {
        channel.Name = string.Format("Unknown SDV {0}", number);
      }
      else
      {
        channel.Name = name;
      }
      channel.LogicalChannelNumber = number;
      channel.Provider = "Cable";
      channel.MediaType = MediaType.Television;
      channel.IsEncrypted = true;
      channel.TransportStreamId = 0;    // doesn't really matter
      channel.SourceId = 0;             // ideally we should have this in order to update channel details
      channel.ProgramNumber = 1;        // must be non-zero in order to tune successfully with a Ceton tuner (which delivers a single-program TS... with PAT and PMT for all channels in the mux)
      channel.PmtPid = 0;               // lookup the correct PID from the PAT when the channel is tuned
      channel.Frequency = 0;
      channel.ModulationScheme = ModulationSchemeQam.Automatic;
      return channel;
    }
  }
}