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
using System.Threading;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Service;
using Mediaportal.TV.Server.TVLibrary.Implementations.Scte.Parser;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.CP.DeviceTree;

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

    private struct PhysicalChannel
    {
      public int Channel;
      public int Frequency;   // unit = kHz
    }

    #region constants

    private const int TABLE_REREQUEST_TIMEOUT = 30000;  // unit = ms

    #endregion

    #region variables

    private ITVCard _tuner = null;
    private ServiceFdc _fdcService = null;
    private IChannelScannerHelper _scanHelper = null;
    private bool _isScanning = false;
    private int _scanTimeOut = 20000;   // milliseconds

    private volatile ScanStage _scanStage = ScanStage.NotScanning;
    private ManualResetEvent _event = null;

    // parsers
    private ParserMgt _parserMgt = new ParserMgt();
    private ParserNit _parserNit = new ParserNit();
    private ParserNtt _parserNtt = new ParserNtt();
    private ParserLvct _parserLvct = new ParserLvct();
    private ParserSvct _parserSvct = new ParserSvct();

    IList<ModulationType> _modulationModes = null;
    IList<PhysicalChannel> _carrierFrequencies = null;
    IDictionary<int, ATSCChannel> _channels = new Dictionary<int, ATSCChannel>();
    HashSet<int> _sourcesWithoutNames = new HashSet<int>();

    #endregion

    public ChannelScannerDri(ITVCard tuner, ServiceFdc fdcService)
    {
      _tuner = tuner;
      _fdcService = fdcService;
      _scanHelper = new ChannelScannerHelperAtsc();
    }

    #region delegates

    private void OnEventSubscriptionFailed(CpService service, UPnPError error)
    {
      this.LogWarn("DRI scan: failed to subscribe to FDC service, scanning is expected to fail");
    }

    private void OnTableSection(CpStateVariable stateVariable, object newValue)
    {
      byte[] section = null;
      try
      {
        if (!stateVariable.Name.Equals("TableSection"))
        {
          return;
        }
        section = (byte[])newValue;
        if (section == null || section.Length < 3)
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

    public void OnCarrierDefinition(AtscTransmissionMedium transmissionMedium, byte index, int carrierFrequency)
    {
      if (transmissionMedium != AtscTransmissionMedium.Cable)
      {
        return;
      }
      if (carrierFrequency > 1750)
      {
        // Convert from centre frequency to the analog video carrier frequency.
        // This is a BDA convention.
        PhysicalChannel channel = new PhysicalChannel();
        channel.Frequency = carrierFrequency - 1750;
        channel.Channel = ATSCChannel.GetPhysicalChannelFromCableFrequency(carrierFrequency);
        _carrierFrequencies[index] = channel;
      }
    }

    public void OnModulationMode(AtscTransmissionMedium transmissionMedium, byte index, TransmissionSystem transmissionSystem,
      BinaryConvolutionCodeRate innerCodingMode, bool isSplitBitstreamMode, ModulationType modulationFormat, int symbolRate)
    {
      if (transmissionMedium != AtscTransmissionMedium.Cable)
      {
        return;
      }
      _modulationModes[index] = modulationFormat;
    }

    public void OnLvctChannelDetail(MgtTableType tableType, string shortName, int majorChannelNumber, int minorChannelNumber,
      ModulationMode modulationMode, uint carrierFrequency, int channelTsid, int programNumber, EtmLocation etmLocation,
      bool accessControlled, bool hidden, int pathSelect, bool outOfBand, bool hideGuide, AtscServiceType serviceType, int sourceId)
    {
      if (programNumber == 0 || outOfBand || modulationMode == ModulationMode.Analog || modulationMode == ModulationMode.PrivateDescriptor ||
        (serviceType != AtscServiceType.Audio && serviceType != AtscServiceType.DigitalTelevision) || sourceId == 0)
      {
        // Not tunable/supported.
        return;
      }

      ATSCChannel channel = null;
      if (_channels.TryGetValue(sourceId, out channel))
      {
        return;
      }

      channel = new ATSCChannel();
      _channels.Add(sourceId, channel);

      carrierFrequency /= 1000; // Hz => kHz
      if (carrierFrequency > 1750)
      {
        // Convert from centre frequency to the analog video carrier
        // frequency. This is a BDA convention.
        channel.Frequency = carrierFrequency - 1750;
      }
      channel.PhysicalChannel = ATSCChannel.GetPhysicalChannelFromCableFrequency((int)carrierFrequency);

      switch (modulationMode)
      {
        case ModulationMode.Atsc8Vsb:
          channel.ModulationType = ModulationType.Mod8Vsb;
          break;
        case ModulationMode.Atsc16Vsb:
          channel.ModulationType = ModulationType.Mod16Vsb;
          break;
        case ModulationMode.ScteMode1:
          channel.ModulationType = ModulationType.Mod64Qam;
          break;
        default:
          channel.ModulationType = ModulationType.Mod256Qam;
          break;
      }
      channel.FreeToAir = !accessControlled;
      channel.MediaType = _scanHelper.GetMediaType((int)serviceType, 0, 0).Value;

      // TODO these two lines should be removed when ATSC channel gets a real LCN field
      channel.MajorChannel = majorChannelNumber;
      channel.MinorChannel = minorChannelNumber;

      if (minorChannelNumber == 0)
      {
        channel.LogicalChannelNumber = majorChannelNumber;
      }
      else
      {
        channel.LogicalChannelNumber = (majorChannelNumber * 1000) + minorChannelNumber;
      }
      channel.Name = shortName;
      if (tableType == MgtTableType.TvctCurrentNext1 || tableType == MgtTableType.TvctCurrentNext0)
      {
        channel.Provider = "Terrestrial";
      }
      else
      {
        channel.Provider = "Cable";
      }
      channel.NetworkId = sourceId;
      channel.PmtPid = -1;  // The engine will automatically lookup and save the correct PID from the PAT when the channel is tuned.
      channel.ServiceId = programNumber;
      channel.TransportId = channelTsid;
    }

    public void OnSvctChannelDetail(AtscTransmissionMedium transmissionMedium, int vctId, int virtualChannelNumber, bool applicationVirtualChannel,
      int bitstreamSelect, int pathSelect, ChannelType channelType, int sourceId, byte cdsReference, int programNumber, byte mmsReference)
    {
      if (transmissionMedium != AtscTransmissionMedium.Cable || applicationVirtualChannel || programNumber == 0 || sourceId == 0)
      {
        // Not tunable/supported.
        return;
      }

      ATSCChannel channel = null;
      if (_channels.TryGetValue(sourceId, out channel))
      {
        return;
      }

      channel = new ATSCChannel();
      _channels.Add(sourceId, channel);

      channel.LogicalChannelNumber = virtualChannelNumber;
      channel.MediaType = MediaTypeEnum.TV;
      channel.FreeToAir = false;
      channel.Frequency = _carrierFrequencies[cdsReference].Frequency;
      channel.MajorChannel = virtualChannelNumber;
      channel.MinorChannel = 0;
      channel.ModulationType = _modulationModes[mmsReference];
      channel.NetworkId = sourceId;
      channel.PhysicalChannel = _carrierFrequencies[cdsReference].Channel;
      channel.PmtPid = -1;  // The engine will automatically lookup and save the correct PID from the PAT when the channel is tuned.
      channel.Provider = "Cable";
      channel.ServiceId = programNumber;
      channel.TransportId = 0;  // We don't have these details.
      _sourcesWithoutNames.Add(sourceId);
    }

    public void OnSourceName(AtscTransmissionMedium transmissionMedium, bool applicationType, int sourceId, string name)
    {
      if (transmissionMedium != AtscTransmissionMedium.Cable || applicationType)
      {
        return;
      }
      ATSCChannel channel = null;
      if (_channels.TryGetValue(sourceId, out channel))
      {
        channel.Name = name;
        channel.NetworkId = sourceId;
        _sourcesWithoutNames.Remove(sourceId);
        if (_sourcesWithoutNames.Count == 0)
        {
          this.LogInfo("DRI scan: all sources now have names, assuming NTT is complete");
          OnTableComplete(MgtTableType.NttSns);
        }
      }
    }

    public void OnMgtTableDetail(MgtTableType tableType, int pid, int versionNumber, uint byteCount)
    {
    }

    #endregion

    #region IScannerInternal member

    /// <summary>
    /// Set the scanner's tuner.
    /// </summary>
    public virtual ITVCard Tuner
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
      _scanTimeOut = SettingsManagement.GetValue("timeoutSDT", 20) * 1000;
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
      // TODO
    }

    public List<IChannel> Scan(IChannel channel)
    {
      _channels.Clear();
      _sourcesWithoutNames.Clear();
      _modulationModes = new ModulationType[255];
      _carrierFrequencies = new PhysicalChannel[255];
      _event = new ManualResetEvent(false);
      try
      {
        _isScanning = true;
        _tuner.Tune(0, channel);
        _fdcService.SubscribeStateVariables(OnTableSection, OnEventSubscriptionFailed);

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
        int availableTimeMilliseconds = _scanTimeOut;
        bool completedStage = false;
        while (availableTimeMilliseconds > 0)
        {
          _fdcService.RequestTables(new List<byte> { 0xc2, 0xc7 });
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
          this.LogError("DRI scan: failed to complete NIT scan stage, check firewall permissions and consider increasing SDT/VCT timeout");
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
        while (availableTimeMilliseconds > 0)
        {
          _fdcService.RequestTables(new List<byte> { 0xc4, 0xc7, 0xc8, 0xc9 });
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
        while (availableTimeMilliseconds > 0)
        {
          _fdcService.RequestTables(new List<byte> { 0xc3, 0xc7 });
          int waitTime = Math.Min(TABLE_REREQUEST_TIMEOUT, availableTimeMilliseconds);
          if (_event.WaitOne(waitTime))
          {
            completedStage = true;
            break;
          }
          availableTimeMilliseconds -= waitTime;
        }

        _scanStage = ScanStage.NotScanning;

        if (!completedStage)
        {
          this.LogWarn("DRI scan: failed to complete NTT scan stage, consider increasing SDT/VCT timeout");
        }
        if (_sourcesWithoutNames.Count > 0)
        {
          this.LogWarn("DRI scan: have {0} source(s) with missing names", _sourcesWithoutNames.Count);
        }

        List<IChannel> channels = new List<IChannel>();
        foreach (ATSCChannel atscChannel in _channels.Values)
        {
          IChannel c = atscChannel as IChannel;
          _scanHelper.UpdateChannel(ref c);
          channels.Add(c);
        }
        return channels;
      }
      finally
      {
        _event.Close();
        _isScanning = false;
        _fdcService.UnsubscribeStateVariables();
      }
    }

    public List<IChannel> ScanNIT(IChannel channel)
    {
      throw new NotImplementedException("DRI scan: NIT scanning not implemented");
    }
  }
}